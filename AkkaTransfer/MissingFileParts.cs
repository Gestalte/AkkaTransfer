namespace AkkaTransfer
{
    public sealed class MissingFileParts
    {
        public MissingFileParts(List<MissingFilePart> fileParts)
        {
            FileParts = fileParts;
        }

        public List<MissingFilePart> FileParts { get; set; }
    }
}
