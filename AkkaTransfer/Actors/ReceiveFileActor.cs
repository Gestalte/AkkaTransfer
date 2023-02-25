using Akka.Actor;
using AkkaTransfer.Data;
using AkkaTransfer.Messages;

namespace AkkaTransfer.Actors
{
    public class ReceiveFileActor : ReceiveActor
    {
        private readonly IFileHeaderRepository fileHeaderRepository;

        public readonly FileBox box;

        public ReceiveFileActor(FileBox box, IFileHeaderRepository fileHeaderRepository)
        {
            this.fileHeaderRepository = fileHeaderRepository;

            this.box = box;

            Receive<FilePartMessage>(message => Handle(message));
        }

        private void Handle(FilePartMessage message)
        {
            System.Diagnostics.Debug.WriteLine($"Receive part {message.Position} of {message.Count}");
            var id = this.fileHeaderRepository.AddNewPieceUnitOfWork(message);

            var props = Props.Create(() => new FileRebuilderActor(this.box, this.fileHeaderRepository));
            var transactionActor = Context.ActorOf(props, "file-rebuilder-actor");

            if (id == -1)
            {
                return;
            }

            transactionActor.Tell(id);
        }
    }
}
