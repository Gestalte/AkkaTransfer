using System.Security.Cryptography;
using System.Text;

namespace AkkaTransfer.Common
{
    sealed class Manifest
    {
        public Manifest(FileBox fileBox)
        {
            var files = Directory.GetFiles(fileBox.BoxPath);

            Timestamp = DateTime.Now;
            Files = files?.Select(s => new ManifestFile(s)).ToArray();
        }

        public Manifest(ManifestFile[]? files)
        {
            Timestamp = DateTime.Now;
            Files = files;
        }

        public DateTime Timestamp { get; private set; }
        public ManifestFile[]? Files { get; private set; }

        /// <summary>
        /// Returns only the files present in the newer Manifest that are not 
        /// present in the older Manifest.
        /// </summary>
        /// <param name="manifest1"></param>
        /// <param name="Manifest2"></param>
        /// <returns>Manifest</returns>
        public static Manifest SubtractManifestFiles(Manifest newerManifest, Manifest olderManifest)
        {
            if (newerManifest.Files == olderManifest.Files)
            {
                return new Manifest(newerManifest.Files);
            }
            else if (newerManifest.Files == null && olderManifest.Files != null)
            {
                return new Manifest(olderManifest.Files);
            }
            else if (olderManifest.Files == null && newerManifest.Files != null)
            {
                return new Manifest(newerManifest.Files);
            }
            else
            {
                // Sselect files that have hash codes that do not exist in
                // the old Manifest. The idea is that if a file exists in both
                // manifests, it should not be copied again.
                var files = newerManifest?.Files?
                    .Where(s => olderManifest?.Files?
                        .Select(s => s.FileHash)
                        .Contains(s.FileHash) ?? false == false)
                    .ToArray();

                return new Manifest(files);
            }
        }
    }

    sealed class ManifestFile
    {
        public ManifestFile(string filepath)
        {
            Filename = Path.GetFileName(filepath);
            FileHash = CalculateMD5Hash(filepath);
        }

        public string Filename { get; private set; }
        public string FileHash { get; private set; }

        private static string CalculateMD5Hash(string filepath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filepath);

            return Encoding.Default.GetString(md5.ComputeHash(stream));
        }

        public override bool Equals(object? obj)
        {
            return obj is ManifestFile file &&
                   Filename == file.Filename &&
                   FileHash == file.FileHash;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Filename, FileHash);
        }
    }
}
