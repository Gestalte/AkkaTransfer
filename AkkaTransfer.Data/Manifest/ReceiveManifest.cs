namespace AkkaTransfer.Data.Manifest
{
    public class ReceiveManifest
    {
        public int ReceiveManifestId { get; set; }
        public DateTime Timestamp { get; set; }
        public ICollection<ReceiveManifestFile> ReceiveManifestFiles { get; set; }
    }
}
