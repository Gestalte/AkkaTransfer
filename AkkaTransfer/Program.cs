using Akka.Actor;
using Akka.Configuration;
using AkkaTransfer.Actors;
using AkkaTransfer.Data;
using AkkaTransfer.Data.Manifest;
using AkkaTransfer.Data.ReceiveFile;
using AkkaTransfer.Data.SendFile;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;

namespace AkkaTransfer
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            var dbContext = new ReceiveDbContext();

            Debug.WriteLine("Checking for DB migrations...");
            if (dbContext.Database.GetPendingMigrations().ToList().Count != 0)
            {
                Debug.WriteLine("Applying DB migrations...");
                dbContext.Database.Migrate();
            }
            Debug.WriteLine("Migrations finished.");

            Debug.WriteLine("Creating Actors...");

            ProgressBar progressBar = new();

            IDbContextFactory dbContextFactory = new DbContextFactory();

            IManifestRepository receiveManifestRepo = new ReceiveManifestRepository(dbContextFactory);
            IManifestRepository sendManifestRepo = new SendManifestRepository(dbContext);
            ISendFileHeaderRepository sendFileHeaderRepo = new SendFileHeaderRepository(dbContextFactory);
            IReceiveFileHeaderRepository receiveFileHeaderRepo = new ReceiveFileHeaderRepository(dbContextFactory);

            FileBox fileSendBox = new("SendBox");
            FileBox fileReceiveBox = new("ReceiveBox");

            var sendManifestHelper = new ManifestHelper(fileSendBox, sendManifestRepo);
            var receiveManifestHelper = new ManifestHelper(fileReceiveBox, receiveManifestRepo);

            Config hocon = HoconLoader.FromFile("akka.net.hocon");
            ActorSystem system = ActorSystem.Create("file-transfer-system", hocon);

            Props manifestProps = Props.Create(() => new ManifestActor
                (HoconLoader.ReadSendIpAndPort("hocon.send")
                , fileSendBox
                ));
            IActorRef manifestActor = system.ActorOf(manifestProps, "manifest-actor");

            Props sendProps = Props.Create(() => new SendFileCoordinator());
            IActorRef sendActor = system.ActorOf(sendProps, "send-file-coordinator-actor");

            Props receiveProps = Props.Create(() => new ReceiveFileCoordinatorActor(fileReceiveBox));
            IActorRef receiveActor = system.ActorOf(receiveProps, "receive-file-coordinator-actor");

            ReceiveFileCoordinatorActor.FilePartMessageReceived += f =>
            {
                Thread.Sleep(10);

                if (progressBar.ReceivedPieces.Contains((f.Filename, f.Position)))
                {
                    return;
                }
                
                if (progressBar.progressBars.ContainsKey(f.Filename))
                {
                    int position = progressBar.progressBars[f.Filename].position;
                    int progress = progressBar.progressBars[f.Filename].progress;

                    var localProgress = progress + 1;

                    progressBar.UpdateProgressBar(f.Filename, f.Count, progress, position);

                    progressBar.progressBars[f.Filename] = (position, localProgress);
                }
                else
                {
                    var (fileName, position, progress) = progressBar.DrawProgressBar(f.Filename, f.Count, 1);
                    progressBar.progressBars.Add(fileName, (position, progress));
                }

                progressBar.ReceivedPieces.Add((f.Filename, f.Position));
            };

            Props rebuildProps = Props.Create(() => new FileRebuilderActor(fileReceiveBox));
            IActorRef rebuildActor = system.ActorOf(rebuildProps, "file-rebuilder-actor");

            Props timeoutProps = Props.Create(() => new FileReceiveTimeoutActor());
            IActorRef timeoutActor = system.ActorOf(timeoutProps, "file-receive-timeout-actor");

            Props manifestCompleteProps = Props.Create(() => new ManifestCompleteActor());
            IActorRef manifestCompleteActor = system.ActorOf(manifestCompleteProps, "manifest-complete-actor");
            ManifestCompleteActor.ManifestReceived += () =>
            {
                Console.WriteLine();
                Console.WriteLine("All files have been received.");

                RequestInput(manifestActor);
            };

            Debug.WriteLine("Finished Creating Actors.");

            RequestInput(manifestActor);

            while (true)
            {
                Thread.Sleep(100);
            }
        }

        internal static void RequestInput(IActorRef manifestActor)
        {
            Console.WriteLine();
            Console.WriteLine("Type \"receive\" or just \"r\" to request file transfer.");

            var input = Console.ReadLine();

            switch (input)
            {
                case "receive":
                    manifestActor.Tell(new SendManifestRequest());
                    break;
                case "r":
                    manifestActor.Tell(new SendManifestRequest());
                    break;
                default:
                    Console.WriteLine("Command not recognised.");
                    RequestInput(manifestActor);
                    break;
            }
        }
    }

    internal class ProgressBar
    {
        public Dictionary<string, (int position, int progress)> progressBars = new();

        public (string fileName, int position, int progress) DrawProgressBar(string filename, int length, int progress)
        {
            float percent = (float)progress / length * 100;

            string barLength = "";

            if (percent > 100)
            {
                percent = 100;
            }

            float disiredLength = percent / 2.0f;

            if (disiredLength > 1)
            {
                barLength = Enumerable.Range(0, (int)disiredLength)
                    .Select(i => "=")
                    .Aggregate((a, b) => a + b);
            }

            Console.WriteLine("{0}[{1}]", filename.PadRight(48), barLength.PadRight(50));

            return (filename, Console.CursorTop - 1, 1);
        }

        string lenSpaces = Enumerable.Range(0, 100).Select(i => " ").Aggregate((a, b) => a + b);

        public List<(string,int)> ReceivedPieces = new();

        // TODO: update progress bars so that they are full when the whole file has been received.

        public void UpdateProgressBar(string filename, int length, int progress, int position)
        {
            var currentPos = Console.CursorTop;

            Console.SetCursorPosition(0, Console.CursorTop - (currentPos - position));

            Console.Write(lenSpaces);

            float percent = (float)progress / length * 100;

            string barLength = "";

            if (percent > 100)
            {
                percent = 100;
            }

            float disiredLength = percent / 2.0f;

            if (disiredLength > 1)
            {
                barLength = Enumerable.Range(0, (int)disiredLength)
                    .Select(i => "=")
                    .Aggregate((a, b) => a + b);
            }

            Console.SetCursorPosition(0, Console.CursorTop);

            Debug.Assert(percent < 101);
            Debug.Assert(barLength.Length < 101);

            Console.WriteLine("{0}[{1}]", filename.PadRight(48), barLength.PadRight(50));

            Console.SetCursorPosition(0, currentPos);
        }
    }
}