using AkkaTransfer.Common;
using Microsoft.EntityFrameworkCore;

namespace AkkaTransfer.Data
{
    sealed class SendFileHeaderRepository : ISendFileHeaderRepository
    {
        private readonly ReceiveDbContext context;

        public SendFileHeaderRepository(ReceiveDbContext context)
        {
            this.context = context;
        }

#nullable enable
        public SendFileHeader? GetFileHeaderByFilename(string filename)
        {
            return this.context.SendFileHeaders
                .AsNoTracking()
                .Where(s => s.FileName == filename)
                .Include(i => i.SendFilePieces)
                .FirstOrDefault();
        }
#nullable disable

        public SendFilePiece[] GetFilePiecesByFilenameAndPosition(string filename, int[] positions)
        {
            var header = this.context.SendFileHeaders
                .AsNoTracking()
                .Where(s => s.FileName == filename)
                .Include(i => i.SendFilePieces.Where(w => positions.Contains(w.Position)))
                .FirstOrDefault();

            return header?.SendFilePieces.ToArray() ?? Array.Empty<SendFilePiece>();
        }

        public void AddFileHeaderAndPieces(FilePartMessage filePartMessage)
        {
            var header = this.context.SendFileHeaders
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
                this.context.SendFileHeaders.Add(new SendFileHeader
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

            this.context.SaveChanges();
        }

        public void DeleteAll()
        {
            this.context.SendFilePieces.ToList().Clear();
            this.context.SendFileHeaders.ToList().Clear();

            this.context.SaveChanges();
        }
    }
}
