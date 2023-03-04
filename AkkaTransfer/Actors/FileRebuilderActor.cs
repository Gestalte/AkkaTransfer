using Akka.Actor;
using AkkaTransfer.Data;
using AkkaTransfer.Data.ReceiveFile;
using System.Diagnostics;

namespace AkkaTransfer.Actors
{
    public class FileRebuilderActor : ReceiveActor
    {
        private readonly FileBox receiveBox;
        private readonly ActorSelection timeoutActor;

        public FileRebuilderActor(FileBox receiveBox)
        {
            this.receiveBox = receiveBox;

            this.timeoutActor = Context.ActorSelection($"akka://file-transfer-system/user/file-receive-timeout-actor");

            Receive<int>(WriteFileIfComplete);
        }

        private List<int> idsToCheck = new();

        public void WriteFileIfComplete(int id)
        {
            Debug.WriteLine($"{nameof(WriteFileIfComplete)} Receive id: {id}", nameof(FileRebuilderActor));

            this.timeoutActor.Tell(id);

            if (idsToCheck.Contains(id) == false)
            {
                idsToCheck.Add(id);
            }

            ReceiveFileHeaderRepository fileHeaderRepository = new(new DbContextFactory());

            for (int i = 0; i < idsToCheck.Count; i++)
            {
                var checkId = idsToCheck[i];

                if (fileHeaderRepository.HasEntireFileBeenReceived(checkId))
                {
                    var header = fileHeaderRepository.GetFileHeaderById(checkId);

                    var headerPieces = header!.ReceiveFilePieces
                        //.AsParallel()
                        //.AsOrdered()
                        .Select(s => (s.Position, s.Content))
                        .Distinct()
                        .OrderBy(o => o.Position)
                        .Select(s => s.Content)
                        .Aggregate((a, b) => a + b);

                    byte[] newBytes = Convert.FromBase64String(headerPieces);

                    File.WriteAllBytes(Path.Combine(this.receiveBox.BoxPath, header.FileName), newBytes);

                    fileHeaderRepository.DeleteFileHeader(checkId);

                    var manifestCompleteActor = Context.ActorSelection("akka://file-transfer-system/user/manifest-complete-actor");
                    manifestCompleteActor.Tell(header.FileName);
                }
            }
        }
    }
}
