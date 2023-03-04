using Akka.Actor;
using AkkaTransfer.Data;
using AkkaTransfer.Data.Manifest;
using AkkaTransfer.Data.ReceiveFile;
using System.Diagnostics;

namespace AkkaTransfer.Actors
{
    public class ManifestCompleteActor : ReceiveActor
    {
        private readonly List<string> completedFiles;

        public static event Action? ManifestReceived;

        public ManifestCompleteActor()
        {
            this.completedFiles = new();

            Receive<string>(filename =>
            {
                Debug.WriteLine($"Received string: {filename}", nameof(ManifestCompleteActor));

                IManifestRepository receiveManifestRepo = new ReceiveManifestRepository(new DbContextFactory());

                IReceiveFileHeaderRepository receiveFileHeaderRepository = new ReceiveFileHeaderRepository(new DbContextFactory());

                completedFiles.Add(filename);

                var manifest = receiveManifestRepo.LoadNewestManifest();

                var filesNotReceived = manifest.Files
                    .Select(s => s.Filename)
                    .ToHashSet()
                    .Except(completedFiles);

                if (!filesNotReceived.Any())
                {
                    receiveFileHeaderRepository.DeleteAll();

                    ManifestReceived?.Invoke();
                }
            });
        }
    }
}
