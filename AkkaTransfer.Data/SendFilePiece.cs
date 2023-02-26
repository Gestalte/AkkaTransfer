namespace AkkaTransfer.Data
{
    public class SendFilePiece
    {
        public int SendFilePieceId { get; set; }
        public string Content { get; set; }
        public int Position { get; set; }
        public int SendFileHeaderId { get; set; }
    }
}
