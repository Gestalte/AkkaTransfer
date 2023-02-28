using AkkaTransfer.Common;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AkkaTransfer.Data.ReceiveFile
{
    public class ReceiveFileHeaderRepository : IReceiveFileHeaderRepository
    {
        private readonly ReceiveDbContext context;

        public ReceiveFileHeaderRepository(ReceiveDbContext context)
        {
            this.context = context;
        }

#nullable enable
        public ReceiveFileHeader? GetFileHeaderById(int Id)
        {
            return context.ReceiveFileHeaders
                .AsNoTracking()
                .Where(s => s.ReceiveFileHeaderId == Id)
                .Include(i => i.ReceiveFilePieces.OrderBy(o => o.Position))
                .FirstOrDefault();
        }
#nullable disable

        public void DeleteFileHeader(int Id)
        {
            var header = context.ReceiveFileHeaders
                .AsNoTracking()
                .Where(s => s.ReceiveFileHeaderId == Id)
                .Include(i => i.ReceiveFilePieces)
                .FirstOrDefault();

            //foreach (var piece in header.FilePieces)
            //{
            //    header.FilePieces.Remove(piece);
            //}

            context.ReceiveFileHeaders.Remove(header);
        }

        public bool HasEntireFileBeenReceived(int fileHeaderId)
        {
            var header = context.ReceiveFileHeaders
                .AsNoTracking()
                .Where(s => s.ReceiveFileHeaderId == fileHeaderId)
                .Include(i => i.ReceiveFilePieces)
                .FirstOrDefault();

            if (header == null || header.ReceiveFilePieces == null)
            {
                return false;
            }

            return header.PieceCount == header.ReceiveFilePieces.Count;
        }

        public int AddNewPieceUnitOfWork(FilePartMessage filePartMessage)
        {
            var header = context.ReceiveFileHeaders
                .Where(s => s.FileName == filePartMessage.Filename)
                .Include(i => i.ReceiveFilePieces)
                .FirstOrDefault();

            if (header != null)
            {
                var piece = context.ReceiveFilePieces
                    .Where(w => w.Position == filePartMessage.Position && w.ReceiveFileHeaderId == header.ReceiveFileHeaderId)
                    .AsNoTracking()
                    .FirstOrDefault();

                if (piece != null)
                {
                    Debug.WriteLine($"Attempted to save duplicate piece at position {filePartMessage.Position} of {filePartMessage.Count}.");

                    return -1;
                }

                ReceiveFilePiece newPiece = new()
                {
                    Content = filePartMessage.FilePart,
                    Position = filePartMessage.Position,
                };

                header.ReceiveFilePieces.Add(newPiece);
            }
            else
            {
                header = context.ReceiveFileHeaders.Add(new ReceiveFileHeader
                {
                    FileName = filePartMessage.Filename,
                    PieceCount = filePartMessage.Count,
                    ReceiveFilePieces = new List<ReceiveFilePiece>
                    {
                        new ReceiveFilePiece
                        {
                            Content = filePartMessage.FilePart,
                            Position = filePartMessage.Position,
                        }
                    }
                }).Entity;
            }

            context.SaveChanges();

            return header.ReceiveFileHeaderId;
        }
    }
}
