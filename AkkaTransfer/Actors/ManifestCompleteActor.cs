using Akka.Actor;
using AkkaTransfer.Data.Manifest;

namespace AkkaTransfer.Actors
{
    public class ManifestCompleteActor : ReceiveActor
    {
        private readonly IManifestRepository receiveManifestRepo;
        private readonly List<string> completedFiles;

        public static event Action? ManifestReceived;

        public ManifestCompleteActor(IManifestRepository receiveManifestRepo)
        {
            this.receiveManifestRepo = receiveManifestRepo;

            this.completedFiles = new();

            Receive<string>(filename =>
            {
                completedFiles.Add(filename);

                var manifest = this.receiveManifestRepo.LoadNewestManifest();

                var filesNotReceived = manifest.Files
                    .Select(s => s.Filename)
                    .ToHashSet()
                    .Except(completedFiles);

                if (!filesNotReceived.Any())
                {
                    ManifestReceived?.Invoke();
                }
            });
        }
    }
}
