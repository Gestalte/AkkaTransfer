using Akka.Actor;
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

            Props manifestProps = Props.Create(() => new ManifestActor(HoconLoader.ReadSendIpAndPort("hocon.send"), sendManifestHelper, receiveManifestHelper));
            IActorRef manifestActor = system.ActorOf(manifestProps, "manifest-actor");

            Props sendProps = Props.Create(() => new SendFileCoordinator(fileSendBox, sendFileHeaderRepo));
            IActorRef sendActor = system.ActorOf(sendProps, "send-file-coordinator-actor");

            Props receiveProps = Props.Create(() => new ReceiveFileActor(fileReceiveBox, receiveFileHeaderRepo));
            IActorRef receiveActor = system.ActorOf(receiveProps);

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
