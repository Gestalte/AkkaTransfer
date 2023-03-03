using Akka.Actor;
using AkkaTransfer.Data;
using AkkaTransfer.Data.ReceiveFile;

namespace AkkaTransfer.Actors
{
    public class FileRebuilderActor : ReceiveActor
    {
        private readonly FileBox receiveBox;
        private readonly ActorSelection timeoutActor;

        public FileRebuilderActor(FileBox receiveBox)
        {
            this.receiveBox = receiveBox;

            this.timeoutActor = Context.ActorSelection($"/user/file-receive-timeout-actor");

            Receive<int>(async id => await WriteFileIfComplete(id));
        }

        public async Task WriteFileIfComplete(int id)
        {
            this.timeoutActor.Tell(id);

            ReceiveFileHeaderRepository fileHeaderRepository = new ReceiveFileHeaderRepository(new DbContextFactory());

            if (fileHeaderRepository.HasEntireFileBeenReceived(id))
            {
                var header = fileHeaderRepository.GetFileHeaderById(id);

                var headerPieces = header!.ReceiveFilePieces
                    .AsParallel()
                    .AsOrdered()
                    .Select(s => s.Content)
                    .Aggregate((a, b) => a + b);

                byte[] newBytes = Convert.FromBase64String(headerPieces);

                File.WriteAllBytes(Path.Combine(this.receiveBox.BoxPath, header.FileName), newBytes);

                fileHeaderRepository.DeleteFileHeader(id);

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
