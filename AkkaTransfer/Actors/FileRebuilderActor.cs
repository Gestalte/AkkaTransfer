using Akka.Actor;
using Akka.Util.Internal;
using AkkaTransfer.Data.ReceiveFile;
using System.Diagnostics;

namespace AkkaTransfer.Actors
{
    public class FileRebuilderActor : ReceiveActor
    {
        private readonly IReceiveFileHeaderRepository fileHeaderRepository;
        private readonly FileBox receiveBox;

        

        public FileRebuilderActor(FileBox receiveBox, IReceiveFileHeaderRepository fileHeaderRepository)
        {
            this.fileHeaderRepository = fileHeaderRepository;
            this.receiveBox = receiveBox;

            Receive<int>(async id => await WriteFileIfComplete(id));
        }

        public async Task WriteFileIfComplete(int id)
        {
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

                Console.WriteLine("File fully received: " + header.FileName);

                Process.Start(this.receiveBox.BoxPath); // Open ReceiveBox folder.                
            }
            else
            {
                Thread.Sleep(5000);

                await WriteFileIfComplete(id);
            }
        }
    }

    sealed class EmptyMessage { }

    sealed class FileReceiveTimeoutActor : ReceiveActor
    {
        private Dictionary<int, DateTime> fileTimes = new();

        public FileReceiveTimeoutActor()
        {
            var scheduler = Context.System.Scheduler;

            scheduler.ScheduleTellRepeatedlyCancelable(0, 1000, Self, new EmptyMessage(), Self);

            Receive<EmptyMessage>(msg =>
            {
                // If ten seconds have passed without receiving any file parts,
                // request that they be sent again.
                fileTimes
                .Where(w => DateTime.UtcNow - w.Value > TimeSpan.FromSeconds(10.0))
                .ForEach(f =>
                {
                    // TODO: Request missing pieces.
                    // TODO: What to do about missing files (Entire file is missing)
                });
            });

            Receive<int>(id =>
            {
                fileTimes.Add(id, DateTime.UtcNow);
            });
        }
    }
}
