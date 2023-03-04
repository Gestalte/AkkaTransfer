using AkkaTransfer.Common;

namespace AkkaTransfer.Data.ReceiveFile
{
    public interface IReceiveFileHeaderRepository
    {
        int AddNewPieceUnitOfWork(FilePartMessage filePartMessage);
#nullable enable
        ReceiveFileHeader? GetFileHeaderById(int Id);
#nullable disable
        void DeleteFileHeader(int Id);
        void DeleteAll();
        bool HasEntireFileBeenReceived(int fileHeaderId);

        MissingFileParts GetMissingPieces(Common.Manifest manifest);
    }
}