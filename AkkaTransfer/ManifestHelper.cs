using Akka.Util.Internal;
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
            var fileDifference = newManifest.Files.Except(oldManifest.Files).ToHashSet();

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

        public Manifest ReadManifest()
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

        public static void PrintManifest(Manifest manifest)
        {
            Console.WriteLine("Manifest creation:" + manifest.Timesstamp);
            Console.WriteLine("Manifest content:");
            manifest.Files.ForEach(f => Console.WriteLine($"\t{f.Filename}\t{f.FileHash}"));
        }
    }
}
