namespace AkkaTransfer.Data.ReceiveFile
{
    public class ReceiveFileHeader
    {
        public int ReceiveFileHeaderId { get; set; }
        public string FileName { get; set; }
        public int PieceCount { get; set; }
        public ICollection<ReceiveFilePiece> ReceiveFilePieces { get; set; }
    }
}
