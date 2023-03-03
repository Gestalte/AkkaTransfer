using Akka.Actor;
using AkkaTransfer.Data.ReceiveFile;

namespace AkkaTransfer.Actors
{
    public class FileRebuilderActor : ReceiveActor
    {
        private readonly IReceiveFileHeaderRepository fileHeaderRepository;
        private readonly FileBox receiveBox;

        private readonly ActorSelection timeoutActor;

        public FileRebuilderActor
            (FileBox receiveBox
            , IReceiveFileHeaderRepository fileHeaderRepository
            )
        {
            this.fileHeaderRepository = fileHeaderRepository;
            this.receiveBox = receiveBox;

            this.timeoutActor = Context.ActorSelection($"/user/file-receive-timeout-actor");

            Receive<int>(async id => await WriteFileIfComplete(id));
        }

        public async Task WriteFileIfComplete(int id)
        {
            this.timeoutActor.Tell(id);

            if (this.fileHeaderRepository.HasEntireFileBeenReceived(id))
            {
                var header = this.fileHeaderRepository.GetFileHeaderById(id);

                var headerPieces = header!.ReceiveFilePieces
                    .AsParallel()
                    .AsOrdered()
                    .Select(s => s.Content)
                    .Aggregate((a, b) => a + b);

                byte[] newBytes = Convert.FromBase64String(headerPieces);

                File.WriteAllBytes(Path.Combine(this.receiveBox.BoxPath, header.FileName), newBytes);

                this.fileHeaderRepository.DeleteFileHeader(id);

                var manifestCompleteActor = Context.ActorSelection("/user/manifest-complete-actor"); // TODO: test that this works.
                manifestCompleteActor.Tell(header.FileName);
            }
            else
            {
                Thread.Sleep(5000);

                await WriteFileIfComplete(id);
            }
        }
    }
}
