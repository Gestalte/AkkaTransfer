using Akka.Actor;
using AkkaTransfer.Data.Manifest;
using AkkaTransfer.Data.ReceiveFile;

namespace AkkaTransfer.Actors
{
    public class FileRebuilderActor : ReceiveActor
    {
        private readonly IReceiveFileHeaderRepository fileHeaderRepository;
        private readonly FileBox receiveBox;
        ActorSelection timeoutActor;

        public FileRebuilderActor(FileBox receiveBox, IReceiveFileHeaderRepository fileHeaderRepository)
        {
            this.fileHeaderRepository = fileHeaderRepository;
            this.receiveBox = receiveBox;

            string ipAndPort = HoconLoader.ReadSendIpAndPort("hocon.send");
            string address = $"akka.tcp://file-transfer-system@{ipAndPort}/user/file-receive-timeout-actor";

            this.timeoutActor = Context.ActorSelection(address);

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

                Console.WriteLine("File fully received: " + header.FileName);

                var transactionActor = Context.ActorSelection("user/file-receive-timeout-actor");


            }
            else
            {
                Thread.Sleep(5000);

                await WriteFileIfComplete(id);
            }
        }
    }

    sealed class EmptyMessage { }

    internal sealed class FileReceiveTimeoutActor : ReceiveActor
    {
        private readonly ReceiveManifestRepository receiveManifestRepository;
        private readonly ReceiveFileHeaderRepository receiveFileHeaderRepository;

        private DateTime LastReceivedTimestamp = DateTime.UtcNow;

        public FileReceiveTimeoutActor
            (ReceiveManifestRepository receiveManifestRepository
            , ReceiveFileHeaderRepository receiveFileHeaderRepository
            )
        {
            this.receiveManifestRepository = receiveManifestRepository;
            this.receiveFileHeaderRepository = receiveFileHeaderRepository;

            var scheduler = Context.System.Scheduler;

            scheduler.ScheduleTellRepeatedlyCancelable(0, 1000, Self, new EmptyMessage(), Self);

            Receive<EmptyMessage>(msg =>
            {
                if (DateTime.UtcNow - LastReceivedTimestamp > TimeSpan.FromSeconds(10.0))
                {
                    Console.WriteLine("Receiving files timed out, requesting missing file pieces.");

                    var manifest = this.receiveManifestRepository.LoadNewestManifest();

                    var missingParts = this.receiveFileHeaderRepository.GetMissingPieces(manifest);

                    // Move this setup to constructor.
                    string ipAndPort = HoconLoader.ReadSendIpAndPort("hocon.send");
                    string address = $"akka.tcp://file-transfer-system@{ipAndPort}/user/send-file-coordinator-actor";

                    var sendActor = Context.ActorSelection(address);

                    sendActor.Tell(missingParts);
                }
            });

            Receive<int>(id =>
            {
                LastReceivedTimestamp = DateTime.UtcNow;
            });
        }
    }
}
