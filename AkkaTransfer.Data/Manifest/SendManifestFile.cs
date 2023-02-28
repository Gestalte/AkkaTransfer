namespace AkkaTransfer.Data.Manifest
{
    public class SendManifestFile
    {
        public int SendManifestFileId { get; set; }
        public string Filename { get; set; }
        public string FileHash { get; set; }
        public int SendManifestId { get; set; }
    }
}
