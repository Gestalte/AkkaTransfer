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

        public ReceiveFileCoordinatorActor(FileBox box, IReceiveFileHeaderRepository fileHeaderRepository)
        {
            this.box = box;
            this.fileHeaderRepository = fileHeaderRepository;

            Props props = Props.Create(() => new ReceiveFileActor(this.box, this.fileHeaderRepository))
                .WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 1000)));

            var receiveRouter = Context.ActorOf(props);
        }
    }

    public class ReceiveFileActor : ReceiveActor
    {
        private readonly IReceiveFileHeaderRepository fileHeaderRepository;

        public readonly FileBox box;

        public ReceiveFileActor(FileBox box, IReceiveFileHeaderRepository fileHeaderRepository)
        {
            this.fileHeaderRepository = fileHeaderRepository;

            this.box = box;

            Receive<FilePartMessage>(Handle);
        }

        private void Handle(FilePartMessage message)
        {
            System.Diagnostics.Debug.WriteLine($"Receive part {message.Position} of {message.Count}");
            Console.WriteLine($"Receive part {message.Position} of {message.Count}");

            var id = this.fileHeaderRepository.AddNewPieceUnitOfWork(message);

            var transactionActor = Context.ActorSelection("user/file-rebuilder-actor");

            if (id == -1)
            {
                return;
            }

            transactionActor.Tell(id);
        }
    }
}
