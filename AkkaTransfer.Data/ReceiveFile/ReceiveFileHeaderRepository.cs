using AkkaTransfer.Common;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AkkaTransfer.Data.ReceiveFile
{
    public class ReceiveFileHeaderRepository : IReceiveFileHeaderRepository
    {
        private readonly ReceiveDbContext context;
        private readonly IDbContextFactory dbContextFactory;

        public ReceiveFileHeaderRepository(IDbContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
            this.context = this.dbContextFactory.CreateDbContext();
        }

#nullable enable
        public ReceiveFileHeader? GetFileHeaderById(int Id)
        {
            return context.ReceiveFileHeaders
                .AsNoTracking()
                .Where(s => s.ReceiveFileHeaderId == Id)
                .Include(i => i.ReceiveFilePieces.OrderBy(o => o.Position))
                .FirstOrDefault();

            // TODO: how to make ReceiveFilePieces Distinct?
        }
#nullable disable

        public void DeleteFileHeader(int Id)
        {
            var header = context.ReceiveFileHeaders
                .AsNoTracking()
                .Where(s => s.ReceiveFileHeaderId == Id)
                .Include(i => i.ReceiveFilePieces)
                .FirstOrDefault();

            context.ReceiveFileHeaders.Remove(header);
            context.SaveChanges();
        }

        public void DeleteAll()
        {
            var headers = context.ReceiveFileHeaders
                .AsNoTracking()
                .Include(i => i.ReceiveFilePieces)
                .ToList();

            context.ReceiveFileHeaders.RemoveRange(headers);
            context.SaveChanges();
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

            return header.PieceCount == header.ReceiveFilePieces
                .Where(w => w.ReceiveFileHeaderId == fileHeaderId)
                .Select(s => s.Position)
                .Distinct()
                .Count();
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

        public MissingFileParts GetMissingPieces(Common.Manifest manifest)
        {
            List<int> getMissing(int target, HashSet<int> have)
                => Enumerable.Range(0, target)
                    .ToHashSet()
                    .Except(have)
                    .ToList();

            return new MissingFileParts(this.context.ReceiveFileHeaders
                .AsNoTracking()
                .Include(i => i.ReceiveFilePieces)
                .Where(w => manifest.Files.Select(s => s.Filename).Contains(w.FileName))
                .ToList()
                .Select(s => new MissingFilePart(s.FileName, getMissing(s.PieceCount, s.ReceiveFilePieces.Select(f => f.Position).ToHashSet())))
                .ToList());
        }
    }
}
