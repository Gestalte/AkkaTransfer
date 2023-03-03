using AkkaTransfer.Common;
using Microsoft.EntityFrameworkCore;

namespace AkkaTransfer.Data.SendFile
{
    public sealed class SendFileHeaderRepository : ISendFileHeaderRepository
    {
        private readonly ReceiveDbContext context;
        private readonly IDbContextFactory dbContextFactory;

        public SendFileHeaderRepository(IDbContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
            this.context = this.dbContextFactory.CreateDbContext();
        }

#nullable enable
        public SendFileHeader? GetFileHeaderByFilename(string filename)
        {
            return context.SendFileHeaders
                .AsNoTracking()
                .Where(s => s.FileName == filename)
                .Include(i => i.SendFilePieces)
                .FirstOrDefault();
        }
#nullable disable

        public SendFilePiece[] GetFilePiecesByFilenameAndPosition(string filename, int[] positions)
        {
            var header = context.SendFileHeaders
                .AsNoTracking()
                .Where(s => s.FileName == filename)
                .Include(i => i.SendFilePieces.Where(w => positions.Contains(w.Position)))
                .FirstOrDefault();

            return header?.SendFilePieces.ToArray() ?? Array.Empty<SendFilePiece>();
        }

        public void AddFileHeaderAndPieces(FilePartMessage filePartMessage)
        {
            var header = context.SendFileHeaders
                .Where(s => s.FileName == filePartMessage.Filename)
                .Include(i => i.SendFilePieces)
                .FirstOrDefault();

            if (header != null)
            {
                header.SendFilePieces.Add(new SendFilePiece()
                {
                    Content = filePartMessage.FilePart,
                    Position = filePartMessage.Position,
                });
            }
            else
            {
                context.SendFileHeaders.Add(new SendFileHeader
                {
                    FileName = filePartMessage.Filename,
                    PieceCount = filePartMessage.Count,
                    SendFilePieces = new List<SendFilePiece>
                    {
                        new SendFilePiece
                        {
                            Content = filePartMessage.FilePart,
                            Position = filePartMessage.Position,
                        }
                    }
                });
            }

            context.SaveChanges();
        }

        public void DeleteAll()
        {
            context.SendFilePieces.ToList().Clear();
            context.SendFileHeaders.ToList().Clear();

            context.SaveChanges();
        }
    }
}
