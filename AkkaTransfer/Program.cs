using Akka.Actor;
using Akka.Configuration;
using AkkaTransfer.Actors;
using AkkaTransfer.Common;
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

            IManifestRepository receiveManifestRepo = new ReceiveManifestRepository(dbContext);
            IManifestRepository sendManifestRepo = new SendManifestRepository(dbContext);
            ISendFileHeaderRepository sendFileHeaderRepo = new SendFileHeaderRepository(dbContext);
            IReceiveFileHeaderRepository receiveFileHeaderRepo = new ReceiveFileHeaderRepository(dbContext);

            FileBox fileSendBox = new("SendBox");
            FileBox fileReceiveBox = new("ReceiveBox");

            var sendManifestHelper = new ManifestHelper(fileSendBox, sendManifestRepo);
            var receiveManifestHelper = new ManifestHelper(fileReceiveBox, receiveManifestRepo);

            Config hocon = HoconLoader.FromFile("akka.net.hocon");
            ActorSystem system = ActorSystem.Create("file-transfer-system", hocon);

            Props manifestProps = Props.Create(() => new ManifestActor
                (HoconLoader.ReadSendIpAndPort("hocon.send")
                , sendManifestHelper
                , receiveManifestHelper
                , sendFileHeaderRepo
                , fileSendBox
                ));
            IActorRef manifestActor = system.ActorOf(manifestProps, "manifest-actor");

            Props sendProps = Props.Create(() => new SendFileCoordinator(fileSendBox, sendFileHeaderRepo));
            IActorRef sendActor = system.ActorOf(sendProps, "send-file-coordinator-actor");

            Props receiveProps = Props.Create(() => new ReceiveFileCoordinatorActor(fileReceiveBox, receiveFileHeaderRepo));
            IActorRef receiveActor = system.ActorOf(receiveProps, "receive-file-coordinator-actor");

            Props rebuildProps = Props.Create(() => new FileRebuilderActor(fileReceiveBox, receiveFileHeaderRepo, receiveManifestRepo));
            IActorRef rebuildActor = system.ActorOf(rebuildProps, "file-rebuilder-actor");

            Props timeoutProps = Props.Create(() => new FileReceiveTimeoutActor(receiveManifestRepo, receiveFileHeaderRepo));
            IActorRef timeoutActor = system.ActorOf(timeoutProps, "file-receive-timeout-actor");

            RequestInput(manifestActor);
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
                    Console.ReadLine();
                    break;
                default:
                    Console.WriteLine("Command not recognised.");
                    RequestInput(manifestActor);
                    break;
            }
        }
    }
}
// TODO: File transfer progress bars
// TODO: Mechanism that tells you when the download is finished.
// Maybe an actor similar to rebuilder that checks the folder vs the manifest
// on a sheduler.