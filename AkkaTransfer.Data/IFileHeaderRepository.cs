using AkkaTransfer.Messages;

namespace AkkaTransfer.Data
{
    public interface IFileHeaderRepository
    {
        void AddNewPieceUnitOfWork(FilePartMessage filePartMessage);
        FileHeader? GetFileHeaderByFilename(string fileName);
        bool HasEntireFileBeenReceived(int fileHeaderId);
    }
}