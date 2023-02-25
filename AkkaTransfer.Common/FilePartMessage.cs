namespace AkkaTransfer.Common
{
    public class FilePartMessage
    {
        public FilePartMessage(byte[] filePart, int position, int count, string filename)
        {
            FilePart = filePart;
            Position = position;
            Count = count;
            Filename = filename;
        }

        public byte[] FilePart { get; set; }
        public int Position { get; set; }
        public int Count { get; set; }
        public string Filename { get; set; }
    }
}
