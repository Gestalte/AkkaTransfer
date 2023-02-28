using Akka.Actor;
using AkkaTransfer.Common;
using AkkaTransfer.Data.Manifest;
using System.Security.Cryptography;
using System.Text;

namespace AkkaTransfer
{
    internal class ManifestHelper
    {
        private readonly FileBox fileBox;
        private readonly IManifestRepository manifestRepository;

        public ManifestHelper(FileBox fileBox, IManifestRepository manifestRepository)
        {
            this.fileBox = fileBox;
            this.manifestRepository = manifestRepository;
        }

        public Manifest Difference(Manifest oldManifest, Manifest newManifest)
        {
            var fileDifference = oldManifest.Files.Except(newManifest.Files).ToHashSet();

            return new Manifest(DateTime.Now, fileDifference);
        }

        private string CalculateMD5Hash(string filepath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filepath);

            return Encoding.Default.GetString(md5.ComputeHash(stream));
        }

        private Manifest LoadManifestFromDB()
        {
            return this.manifestRepository.LoadNewestManifest();
        }

        private void WriteManifestToDB(Manifest directoryManifest)
        {
            this.manifestRepository.Save(directoryManifest);
        }

        private Manifest MapManifestFromDirectory(string directory)
        {
            var filepaths = Directory.GetFiles(directory);

            HashSet<ManifestFile> files = filepaths
                .Select(path => new ManifestFile(Path.GetFileName(path), CalculateMD5Hash(path)))
                .ToHashSet();

            return new Manifest(DateTime.Now, files);
        }

        public Manifest CreateManifest()
        {
            Manifest dbManifest = LoadManifestFromDB();
            Manifest directoryManifest = MapManifestFromDirectory(this.fileBox.BoxPath);

            if (dbManifest == directoryManifest)
            {
                return dbManifest;
            }

            WriteManifestToDB(directoryManifest);

            return directoryManifest;
        }
    }

    public sealed class ManifestRequest { }

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
        }

        public void ReceiveManifest(Manifest manifest)
        {
            Manifest oldManifest = this.receiverManifestHelper.CreateManifest();

            Manifest difference = this.receiverManifestHelper.Difference(oldManifest, manifest);

            var sendActor = Context.ActorSelection(foreignAddress + SendActorName);

            sendActor.Tell(difference);
        }

        public void SendManifest(ManifestRequest _)
        {
            Manifest newManifest = this.senderManifestHelper.CreateManifest();

            var manifestActor = Context.ActorSelection(foreignAddress + ManifestActorName);

            manifestActor.Tell(newManifest);
        }

        public async Task RequestManifestAsync()
        {
            var manifestActor = Context.ActorSelection(foreignAddress + ManifestActorName);

            var receivedManifest = await manifestActor.Ask<Manifest>(new ManifestRequest(), TimeSpan.FromSeconds(5));

            ReceiveManifest(receivedManifest);
        }
    }
}
