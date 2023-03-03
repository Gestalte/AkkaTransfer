using Akka.Actor;
using Akka.Routing;
using AkkaTransfer.Common;
using AkkaTransfer.Data.ReceiveFile;

namespace AkkaTransfer.Actors
{
    public class ReceiveFileCoordinatorActor : ReceiveActor
    {
        private readonly IReceiveFileHeaderRepository fileHeaderRepository;

        public readonly FileBox box;

        public static event Action<FilePartMessage>? FilePartMessageReceived;

        public ReceiveFileCoordinatorActor(FileBox box, IReceiveFileHeaderRepository fileHeaderRepository)
        {
            this.box = box;
            this.fileHeaderRepository = fileHeaderRepository;

            Props props = Props.Create(() => new ReceiveFileActor(this.box, this.fileHeaderRepository))
                .WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 1000)));

            var receiveRouter = Context.ActorOf(props);

            Receive<FilePartMessage>(receiveRouter.Tell);
        }
    }
}
