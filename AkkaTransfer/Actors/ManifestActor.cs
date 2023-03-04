using Akka.Actor;
using AkkaTransfer.Common;
using AkkaTransfer.Data;
using AkkaTransfer.Data.Manifest;
using AkkaTransfer.Data.ReceiveFile;
using AkkaTransfer.Data.SendFile;

namespace AkkaTransfer.Actors
{
    public sealed class ManifestRequest { }
    public sealed class SendManifestRequest { }
    internal sealed class ManifestActor : ReceiveActor
    {
        private const string ManifestActorName = "manifest-actor";
        private const string SendActorName = "send-file-coordinator-actor";

        private readonly string foreignAddress;

        private readonly FileBox fileSendbox;

        public ManifestActor(string ipAndPort, FileBox fileSendbox)
        {
            this.fileSendbox = fileSendbox;
            this.foreignAddress = $"akka.tcp://file-transfer-system@{ipAndPort}/user/";

            Receive<ManifestRequest>(SendManifest);
            Receive<SendManifestRequest>(RequestManifest);
        }

        // File sending actor
        public void SendManifest(ManifestRequest _)
        {
            SendManifestRepository sendManifestRepository = new SendManifestRepository(new ReceiveDbContext());
            SendFileHeaderRepository sendFileRepo = new SendFileHeaderRepository(new DbContextFactory());

            ManifestHelper senderManifestHelper = new ManifestHelper(this.fileSendbox, sendManifestRepository);

            Manifest oldManifest = senderManifestHelper.LoadManifestFromDB();
            Manifest newManifest = senderManifestHelper.ReadManifest();

            Sender.Tell(newManifest);

            if (newManifest.Files != oldManifest.Files)
            {
                // Replace the old manifest's pieces with those in the new manifest.

                sendFileRepo.DeleteAll();

                var filepaths = this.fileSendbox.GetFilesInBox();

                for (int i = 0; i < filepaths.Count; i++)
                {
                    var messages = FileHelper.SplitIntoMessages(filepaths[i], Path.GetFileName(filepaths[i]));

                    for (int j = 0; j < messages.Length; j++)
                    {
                        sendFileRepo.AddFileHeaderAndPieces(messages[j]);
                    }
                }
            }
        }

        // File receiving actor
        public void RequestManifest(SendManifestRequest _)
        {
            FileBox fileBox = new("ReceiveBox");

            var receiveManifestRepository = new ReceiveManifestRepository(new DbContextFactory());
            var receiverManifestHelper = new ManifestHelper(fileBox, receiveManifestRepository);

            // Request manifest
            var manifestActor = Context.ActorSelection(foreignAddress + ManifestActorName);

            var receivedManifest = manifestActor.Ask<Manifest>(new ManifestRequest(), TimeSpan.FromSeconds(5)).Result;

            Console.Out.WriteLine("Received Manifest:");
            ManifestHelper.PrintManifest(receivedManifest);

            // Calculate which files to ask for.
            Manifest oldManifest = receiverManifestHelper.ReadManifest();

            Manifest difference = receiverManifestHelper.Difference(oldManifest, receivedManifest);

            receiverManifestHelper.WriteManifestToDB(difference);

            Console.Out.WriteLine();
            Console.Out.WriteLine("Local files differ by:");
            ManifestHelper.PrintManifest(difference);

            // Ask for files.
            var sendActor = Context.ActorSelection(foreignAddress + SendActorName);

            sendActor.Tell(difference);
        }
    }
}
