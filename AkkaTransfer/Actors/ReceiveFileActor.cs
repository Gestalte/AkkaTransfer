using Akka.Actor;
using AkkaTransfer.Common;
using AkkaTransfer.Data;

namespace AkkaTransfer.Actors {
    public class ReceiveFileActor : ReceiveActor
    {
        private readonly IReceiveFileHeaderRepository fileHeaderRepository;

        public readonly FileBox box;

        public ReceiveFileActor(FileBox box, IReceiveFileHeaderRepository fileHeaderRepository)
        {
            this.fileHeaderRepository = fileHeaderRepository;

            this.box = box;

            Receive<FilePartMessage>(message => Handle(message));
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
