using AkkaTransfer.Messages;

namespace AkkaTransfer.Data
{
    public interface IFileHeaderRepository
    {
        int AddNewPieceUnitOfWork(FilePartMessage filePartMessage);
        FileHeader? GetFileHeaderById(int Id);
        void DeleteFileHeader(int Id);
        bool HasEntireFileBeenReceived(int fileHeaderId);
    }
}