using Akka.Actor;
using Akka.Routing;
using AkkaTransfer.Common;
using AkkaTransfer.Data.SendFile;
using System.Security.Cryptography.X509Certificates;

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
                System.Diagnostics.Debug.WriteLine($"Received MissingFileParts:", nameof(SendFileCoordinator));
                PrintMissingParts(missingParts);

                missingParts.FileParts.ForEach(s => sendFileRouter.Tell(s));
            });
        }

        private void PrintMissingParts(MissingFileParts missingParts)
        {
            missingParts.FileParts.ForEach(f =>
            {
                System.Diagnostics.Debug.WriteLine("\tFilename: " + f.Filename, nameof(SendFileCoordinator));
                System.Diagnostics.Debug.WriteLine("\tMissing piece count:" + f.MissingPiecePositions.Count, nameof(SendFileCoordinator));
            });
        }
    }
}
