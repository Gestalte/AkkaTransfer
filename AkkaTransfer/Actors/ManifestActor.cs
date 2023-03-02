using Akka.Actor;
using AkkaTransfer.Common;

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

        public ManifestActor(string ipAndPort, ManifestHelper senderManifestHelper, ManifestHelper receiverManifestHelper)
        {
            this.senderManifestHelper = senderManifestHelper;
            this.receiverManifestHelper = receiverManifestHelper;
            this.foreignAddress = $"akka.tcp://file-transfer-system@{ipAndPort}/user/";

            Receive<ManifestRequest>(SendManifest);
            Receive<SendManifestRequest>(RequestManifest);
        }

        // File sending actor
        public void SendManifest(ManifestRequest _)
        {
            Manifest newManifest = this.senderManifestHelper.ReadManifest();

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
