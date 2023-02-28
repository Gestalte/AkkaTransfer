namespace AkkaTransfer.Data.Manifest
{
    public class ReceiveManifestFile
    {
        public int ReceiveManifestFileId { get; set; }
        public string Filename { get; set; }
        public string FileHash { get; set; }
        public int ReceiveManifestId { get; set; }
    }
}
