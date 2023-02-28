using Akka.Actor;
using AkkaTransfer.Common;

namespace AkkaTransfer.Actors
{
    public sealed class ManifestRequest { }
    public sealed class SendManifestRequest { }
    internal sealed class ManifestActor : ReceiveActor
    {
        private const string ManifestActorName = "manifest-actor";
        private const string SendActorName = "send-actor";

        private readonly string foreignAddress;
        private readonly ManifestHelper senderManifestHelper;
        private readonly ManifestHelper receiverManifestHelper;

        public ManifestActor(string ipAndPort, ManifestHelper senderManifestHelper, ManifestHelper receiverManifestHelper)
        {
            this.senderManifestHelper = senderManifestHelper;
            this.receiverManifestHelper = receiverManifestHelper;
            this.foreignAddress = $"akka.tcp://file-transfer-system@{ipAndPort}/user/";

            Receive<ManifestRequest>(SendManifest);
            Receive<SendManifestRequest>(async r => await RequestManifest(r));
        }

        // File sending actor
        public void SendManifest(ManifestRequest _)
        {
            Manifest newManifest = this.senderManifestHelper.CreateManifest();

            Sender.Tell(newManifest);
        }

        // File receiving actor
        public async Task RequestManifest(SendManifestRequest _)
        {
            // Request manifest
            var manifestActor = Context.ActorSelection(foreignAddress + ManifestActorName);

            var receivedManifest = await manifestActor.Ask<Manifest>(new ManifestRequest(), TimeSpan.FromSeconds(5));

            await Console.Out.WriteLineAsync("Received Manifest:");
            ManifestHelper.PrintManifest(receivedManifest);

            // Calculate which files to ask for.
            Manifest oldManifest = this.receiverManifestHelper.CreateManifest();

            Manifest difference = this.receiverManifestHelper.Difference(oldManifest, receivedManifest);

            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("Local files differ by:");
            ManifestHelper.PrintManifest(difference);

            // Ask for files.
            var sendActor = Context.ActorSelection(foreignAddress + SendActorName);

            sendActor.Tell(difference); // TODO: Make this actor only send the files in the difference list.
        }
    }
}
