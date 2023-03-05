namespace AkkaTransfer.Data.Manifest
{
    public interface IManifestRepository
    {
        Common.Manifest LoadNewestManifest();
        void Save(Common.Manifest manifest);
    }
}
