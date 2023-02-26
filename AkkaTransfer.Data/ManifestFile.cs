namespace AkkaTransfer.Data
{
    public class ManifestFile
    {
        public int ManifestFileId { get; set; }
        public string Filename { get; set; }
        public string FileHash { get; set; }
        public int ManifestId { get; set; }
    }
}
