using Akka.Actor;
using AkkaTransfer.Common;
using AkkaTransfer.Data;
using AkkaTransfer.Data.ReceiveFile;

namespace AkkaTransfer.Actors
{
    public class ReceiveFileActor : ReceiveActor
    {
        public readonly FileBox box;

        public ReceiveFileActor(FileBox box)
        {
            this.box = box;

            Receive<FilePartMessage>(message =>
            {
                ReceiveFileHeaderRepository fileHeaderRepository = new(new DbContextFactory());

                System.Diagnostics.Debug.WriteLine($"Receive part {message.Position} of {message.Count}");

                var id = fileHeaderRepository.AddNewPieceUnitOfWork(message);

                if (id == -1)
                {
                    return;
                }

                var transactionActor = Context.ActorSelection("akka://file-transfer-system/user/file-rebuilder-actor");

                transactionActor.Tell(id);
            });
        }
    }
}
