using Akka.Actor;
using AkkaTransfer.Data;
using AkkaTransfer.Data.Manifest;
using AkkaTransfer.Data.ReceiveFile;

namespace AkkaTransfer.Actors
{
    internal sealed class FileReceiveTimeoutActor : ReceiveActor
    {
        private DateTime LastReceivedTimestamp = DateTime.UtcNow;

        private IScheduler scheduler;
        private ICancelable schedulerCancel;

        private readonly string ipAndPort;
        private readonly string address;

        public FileReceiveTimeoutActor()
        {
            this.scheduler = Context.System.Scheduler;
            this.schedulerCancel = default!;

            this.ipAndPort = HoconLoader.ReadSendIpAndPort("hocon.send");
            this.address = $"akka.tcp://file-transfer-system@{ipAndPort}/user/send-file-coordinator-actor";

            Become(Sleeping);
        }

        public void Awake()
        {
            Console.WriteLine("Awake", nameof(FileReceiveTimeoutActor));
            this.schedulerCancel = this.scheduler.ScheduleTellRepeatedlyCancelable(0, 1000, Self, new EmptyMessage(), Self);

            Receive<int>(id =>
            {
                LastReceivedTimestamp = DateTime.UtcNow;
            });

            Receive<EmptyMessage>(msg =>
            {
                if (DateTime.UtcNow - LastReceivedTimestamp > TimeSpan.FromSeconds(10.0))
                {
                    var receiveFileHeaderRepository = new ReceiveFileHeaderRepository(new DbContextFactory());
                    var receiveManifestRepository = new ReceiveManifestRepository(new DbContextFactory());

                    Console.WriteLine("Receiving files timed out, requesting missing file pieces.");

                    var manifest = receiveManifestRepository.LoadNewestManifest();

                    var missingParts = receiveFileHeaderRepository.GetMissingPieces(manifest);

                    var sendCoordinator = Context.ActorSelection(this.address);

                    sendCoordinator.Tell(missingParts);

                    Become(Sleeping);
                }
            });
        }

        public void Sleeping()
        {
            Console.WriteLine("Sleeping", nameof(FileReceiveTimeoutActor));
            this.schedulerCancel?.Cancel();

            Receive<int>(x => Become(Awake));
        }
    }
}
