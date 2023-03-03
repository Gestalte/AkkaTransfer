namespace AkkaTransfer
{
    public sealed class MissingFilePart
    {
        public MissingFilePart(string filename, List<int> missingPiecePositions)
        {
            Filename = filename;
            MissingPiecePositions = missingPiecePositions;
        }

        public string Filename { get; set; }
        public List<int> MissingPiecePositions { get; set; }
    }
}
