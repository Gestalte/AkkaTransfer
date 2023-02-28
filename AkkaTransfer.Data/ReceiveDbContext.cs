using AkkaTransfer.Common;
using AkkaTransfer.Data.Manifest;
using AkkaTransfer.Data.ReceiveFile;
using AkkaTransfer.Data.SendFile;
using Microsoft.EntityFrameworkCore;

namespace AkkaTransfer.Data
{
    public class ReceiveDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=receive.db");
        }

        public DbSet<ReceiveFileHeader> ReceiveFileHeaders { get; set; }
        public DbSet<ReceiveFilePiece> ReceiveFilePieces { get; set; }
        public DbSet<SendFileHeader> SendFileHeaders { get; set; }
        public DbSet<SendFilePiece> SendFilePieces { get; set; }
        public DbSet<SendManifest> SendManifests { get; set; }
        public DbSet<SendManifestFile> SendManifestFiles { get; set; }
        public DbSet<ReceiveManifest> ReceiveManifests { get; set; }
        public DbSet<ReceiveManifestFile> ReceiveManifestFiles { get; set; }
    }
}
