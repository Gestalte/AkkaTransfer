namespace AkkaTransfer.Data
{
    public class ReceiveFilePiece
    {
        public int ReceiveFilePieceId { get; set; }
        public string Content { get; set; }
        public int Position { get; set; }
        public int ReceiveFileHeaderId { get; set; }
    }
}
