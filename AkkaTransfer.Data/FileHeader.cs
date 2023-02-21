namespace AkkaTransfer.Data
{
    public class FileHeader
    {
        public int FileHeaderId { get; set; }
        public string FileName { get; set; }
        public int PieceCount { get; set; }
        public ICollection<FilePiece> FilePieces { get; set; }
    }
}
