using AkkaTransfer.Common;

namespace AkkaTransfer.Data {
    public interface IReceiveFileHeaderRepository
    {
        int AddNewPieceUnitOfWork(FilePartMessage filePartMessage);
#nullable enable
        ReceiveFileHeader? GetFileHeaderById(int Id);
#nullable disable
        void DeleteFileHeader(int Id);
        bool HasEntireFileBeenReceived(int fileHeaderId);
    }
}