using Akka.Actor;
using Akka.Routing;
using AkkaTransfer.Common;
using AkkaTransfer.Data.SendFile;

namespace AkkaTransfer.Actors
{
    public sealed class SendFileCoordinator : ReceiveActor
    {
        public SendFileCoordinator()
        {
            var sendProps = Props.Create(() => new SendFileActor())
                .WithRouter(new RoundRobinPool(5, new DefaultResizer(5, 1000)));

            var sendFileRouter = Context.ActorOf(sendProps);

            Receive<Manifest>(manifest =>
            {
                System.Diagnostics.Debug.WriteLine($"Received Manifest: {manifest}", nameof(SendFileCoordinator));

                manifest.Files
                    .Select(s => s.Filename)
                    .ToList()
                    .ForEach(filename =>
                    {
                        sendFileRouter.Tell(filename);
                    });
            });

            Receive<MissingFileParts>(missingParts =>
            {
                System.Diagnostics.Debug.WriteLine($"Received MissingFileParts: {missingParts}", nameof(SendFileCoordinator));

                missingParts.FileParts.ForEach(s => sendFileRouter.Tell(s));
            });
        }
    }
}
