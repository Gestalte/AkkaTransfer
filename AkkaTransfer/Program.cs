﻿using Akka.Actor;
using Akka.Configuration;
using AkkaTransfer.Actors;
using AkkaTransfer.Data;
using AkkaTransfer.Data.Manifest;
using AkkaTransfer.Data.ReceiveFile;
using AkkaTransfer.Data.SendFile;
using Microsoft.EntityFrameworkCore;

namespace AkkaTransfer
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            var dbContext = new ReceiveDbContext();

            if (dbContext.Database.GetPendingMigrations().ToList().Count != 0)
            {
                dbContext.Database.Migrate();
            }

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
                if (progressBar.progressBars.Select(s => s.Item1).Contains(f.Filename))
                {
                    (string fileName, int position, int progress) = progressBar.progressBars.Where(w => w.Item1 == f.Filename).FirstOrDefault();

                    progressBar.UpdateProgressBar(f.Filename, f.Count, progress, position);
                }
                else
                {
                    progressBar.DrawProgressBar(f.Filename, f.Count, 1);
                }
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
        public List<(string fileName, int position, int progress)> progressBars = new();

        public (string fileName, int position, int progress) DrawProgressBar(string filename, int length, int progress)
        {
            float percent = ((float)progress / length) * 50;

            string barLength = "";
            if (percent > 1)
            {
                barLength = Enumerable.Range(0, (int)percent).Select(i => "=").Aggregate((a, b) => a + b);
            }
            Console.WriteLine("{0}[{1}]", filename.PadRight(48), barLength.PadRight(50));

            return (filename, Console.GetCursorPosition().Top - 1, 1);
        }

        public void UpdateProgressBar(string filename, int length, int progress, int position)
        {
            var lenSpaces = Enumerable.Range(0, length).Select(i => " ").Aggregate((a, b) => a + b);

            var currentPos = Console.GetCursorPosition().Top;
            Console.SetCursorPosition(0, Console.CursorTop - (currentPos - position));

            float percent = ((float)progress / length) * 50;
            string barLength = "";
            if (percent > 1)
            {
                barLength = Enumerable.Range(0, (int)percent).Select(i => "=").Aggregate((a, b) => a + b);
            }
            Console.WriteLine("{0}[{1}]", filename.PadRight(48), barLength.PadRight(50));

            Console.SetCursorPosition(0, currentPos);
        }
    }
}