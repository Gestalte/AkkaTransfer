namespace AkkaTransfer.Data
{
    public class FilePiece
    {
        public int FilePieceId { get; set; }
        public string Content { get; set; }
        public int Position { get; set; }
        public int FileHeaderId { get; set; }
    }
}
