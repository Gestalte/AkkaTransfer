using Akka.Actor;
using AkkaTransfer.Data.Manifest;
using AkkaTransfer.Data.ReceiveFile;

namespace AkkaTransfer.Actors
{
    internal sealed class FileReceiveTimeoutActor : ReceiveActor
    {
        private readonly IManifestRepository receiveManifestRepository;
        private readonly IReceiveFileHeaderRepository receiveFileHeaderRepository;

        private DateTime LastReceivedTimestamp = DateTime.UtcNow;

        private IScheduler scheduler;
        private ICancelable schedulerCancel;

        private readonly string ipAndPort;
        private readonly string address;

        public FileReceiveTimeoutActor(IManifestRepository receiveManifestRepository, IReceiveFileHeaderRepository receiveFileHeaderRepository)
        {
            this.receiveFileHeaderRepository = receiveFileHeaderRepository;
            this.receiveManifestRepository = receiveManifestRepository;

            this.scheduler = Context.System.Scheduler;
            this.schedulerCancel = default!;

            this.ipAndPort = HoconLoader.ReadSendIpAndPort("hocon.send");
            this.address = $"akka.tcp://file-transfer-system@{ipAndPort}/user/send-file-coordinator-actor";

            Become(Sleeping);
        }

        public void Awake()
        {
            this.schedulerCancel = this.scheduler.ScheduleTellRepeatedlyCancelable(0, 1000, Self, new EmptyMessage(), Self);

            Receive<int>(id =>
            {
                LastReceivedTimestamp = DateTime.UtcNow;
            });

            Receive<EmptyMessage>(msg =>
            {
                if (DateTime.UtcNow - LastReceivedTimestamp > TimeSpan.FromSeconds(10.0))
                {
                    Console.WriteLine("Receiving files timed out, requesting missing file pieces.");

                    var manifest = this.receiveManifestRepository.LoadNewestManifest();

                    var missingParts = this.receiveFileHeaderRepository.GetMissingPieces(manifest);

                    var sendCoordinator = Context.ActorSelection(this.address);

                    sendCoordinator.Tell(missingParts);

                    Become(Sleeping);
                }
            });
        }

        public void Sleeping()
        {
            this.schedulerCancel?.Cancel();

            Receive<int>(x => Become(Awake));
        }
    }
}
