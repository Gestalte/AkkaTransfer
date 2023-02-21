﻿using AkkaTransfer.Messages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AkkaTransfer.Data
{
    public class FileHeaderRepository : IFileHeaderRepository
    {
        private readonly ReceiveDbContext context;

        public FileHeaderRepository(ReceiveDbContext context)
        {

            this.context = context;
        }

        public FileHeader? GetFileHeaderById(int Id)
        {
            return this.context.FileHeaders
                .AsNoTracking()
                .Where(s => s.FileHeaderId == Id)
                .Include(i => i.FilePieces)
                .FirstOrDefault();
        }

        public void DeleteFileHeader(int Id)
        {
            var header = this.context.FileHeaders
                .AsNoTracking()
                .Where(s => s.FileHeaderId == Id)
                .Include(i => i.FilePieces)
                .FirstOrDefault();

            //foreach (var piece in header.FilePieces)
            //{
            //    header.FilePieces.Remove(piece);
            //}

            this.context.FileHeaders.Remove(header);
        }

        public bool HasEntireFileBeenReceived(int fileHeaderId)
        {
            var header = this.context.FileHeaders
                .Where(s => s.FileHeaderId == fileHeaderId)
                .Include(i => i.FilePieces)
                .FirstOrDefault();

            if (header == null || header.FilePieces == null)
            {
                return false;
            }

            return header.PieceCount == header.FilePieces.Count;
        }

        public int AddNewPieceUnitOfWork(FilePartMessage filePartMessage)
        {
            var header = this.context.FileHeaders
                .Where(s => s.FileName == filePartMessage.Filename)
                .Include(i => i.FilePieces)
                .FirstOrDefault();

            if (header != null)
            {
                var piece = this.context.FilePieces
                    .Where(w => w.Position == filePartMessage.Position && w.FileHeaderId == header.FileHeaderId)
                    .AsNoTracking()
                    .FirstOrDefault();

                if (piece != null)
                {
                    Debug.WriteLine($"Attempted to save duplicate piece at position {filePartMessage.Position} of {filePartMessage.Count}.");

                    return -1;
                }

                FilePiece newPiece = new FilePiece
                {
                    Content = filePartMessage.FilePart,
                    Position = filePartMessage.Position,
                };

                header.FilePieces.Add(newPiece);
            }
            else
            {
                header = this.context.FileHeaders.Add(new FileHeader
                {
                    FileName = filePartMessage.Filename,
                    PieceCount = filePartMessage.Count,
                    FilePieces = new List<FilePiece>
                    {
                        new FilePiece
                        {
                            Content=filePartMessage.FilePart,
                            Position = filePartMessage.Position,
                        }
                    }
                }).Entity;
            }

            this.context.SaveChanges();

            return header.FileHeaderId;
        }
    }
}
