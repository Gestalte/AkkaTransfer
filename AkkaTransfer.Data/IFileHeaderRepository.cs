using AkkaTransfer.Common;

namespace AkkaTransfer.Data {
    public interface IFileHeaderRepository
    {
        int AddNewPieceUnitOfWork(FilePartMessage filePartMessage);
#nullable enable
        FileHeader? GetFileHeaderById(int Id);
#nullable disable
        void DeleteFileHeader(int Id);
        bool HasEntireFileBeenReceived(int fileHeaderId);
    }
}