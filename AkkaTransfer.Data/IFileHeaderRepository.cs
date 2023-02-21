using AkkaTransfer.Messages;

namespace AkkaTransfer.Data
{
    public interface IFileHeaderRepository
    {
        void AddNewPieceUnitOfWork(FilePartMessage filePartMessage);
        FileHeader? GetFileHeaderById(int Id);
        void DeleteFileHeader(int Id);
        bool HasEntireFileBeenReceived(int fileHeaderId);
    }
}