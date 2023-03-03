using Akka.Actor;
using AkkaTransfer.Common;
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
        private readonly ManifestHelper senderManifestHelper;
        private readonly ManifestHelper receiverManifestHelper;

        private readonly ISendFileHeaderRepository sendFileRepo;

        private readonly FileBox fileSendbox;

        public ManifestActor
            (string ipAndPort
            , ManifestHelper senderManifestHelper
            , ManifestHelper receiverManifestHelper
            , ISendFileHeaderRepository sendFileRepo
            , FileBox fileSendbox
            )
        {
            this.sendFileRepo = sendFileRepo;
            this.fileSendbox = fileSendbox;
            this.senderManifestHelper = senderManifestHelper;
            this.receiverManifestHelper = receiverManifestHelper;
            this.foreignAddress = $"akka.tcp://file-transfer-system@{ipAndPort}/user/";

            Receive<ManifestRequest>(SendManifest);
            Receive<SendManifestRequest>(RequestManifest);
        }

        // File sending actor
        public void SendManifest(ManifestRequest _)
        {
            Manifest oldManifest = this.senderManifestHelper.LoadManifestFromDB();
            Manifest newManifest = this.senderManifestHelper.ReadManifest();

            if (newManifest.Files != oldManifest.Files)
            {
                // Replace the old manifest's pieces with those in the new manifest.

                this.sendFileRepo.DeleteAll();

                var filepaths = this.fileSendbox.GetFilesInBox();

                for (int i = 0; i < filepaths.Count; i++)
                {
                    var messages = FileHelper.SplitIntoMessages(filepaths[i], Path.GetFileName(filepaths[i]));

                    for (int j = 0; j < messages.Length; j++)
                    {
                        this.sendFileRepo.AddFileHeaderAndPieces(messages[j]);
                    }
                }
            }

            Sender.Tell(newManifest);
        }

        // File receiving actor
        public void RequestManifest(SendManifestRequest _)
        {
            // Request manifest
            var manifestActor = Context.ActorSelection(foreignAddress + ManifestActorName);

            var receivedManifest = manifestActor.Ask<Manifest>(new ManifestRequest(), TimeSpan.FromSeconds(5)).Result;

            Console.Out.WriteLine("Received Manifest:");
            ManifestHelper.PrintManifest(receivedManifest);

            // Calculate which files to ask for.
            Manifest oldManifest = this.receiverManifestHelper.ReadManifest();

            Manifest difference = this.receiverManifestHelper.Difference(oldManifest, receivedManifest);

            Console.Out.WriteLine();
            Console.Out.WriteLine("Local files differ by:");
            ManifestHelper.PrintManifest(difference);

            // Ask for files.
            var sendActor = Context.ActorSelection(foreignAddress + SendActorName);

            sendActor.Tell(difference);
        }
    }
}
