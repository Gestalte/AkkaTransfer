namespace AkkaTransfer.Data.SendFile
{
    public class SendFileHeader
    {
        public int SendFileHeaderId { get; set; }
        public string FileName { get; set; }
        public int PieceCount { get; set; }
        public ICollection<SendFilePiece> SendFilePieces { get; set; }
    }
}
