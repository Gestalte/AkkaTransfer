using Akka.Actor;
using Akka.Routing;
using AkkaTransfer.Common;

namespace AkkaTransfer.Actors
{
    public class ReceiveFileCoordinatorActor : ReceiveActor
    {
        public readonly FileBox box;

        public static event Action<FilePartMessage>? FilePartMessageReceived;

        public ReceiveFileCoordinatorActor(FileBox box)
        {
            this.box = box;

            Props props = Props.Create(() => new ReceiveFileActor(this.box))
                .WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 1000)));

            var receiveRouter = Context.ActorOf(props);

            Receive<FilePartMessage>(message =>
            {
                FilePartMessageReceived?.Invoke(message);
                receiveRouter.Tell(message);
            });
        }
    }
}
