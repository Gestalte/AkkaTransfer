namespace AkkaTransfer.Data.Manifest
{
    public class SendManifest
    {
        public int SendManifestId { get; set; }
        public DateTime Timestamp { get; set; }
        public ICollection<SendManifestFile> SendManifestFiles { get; set; }
    }
}
