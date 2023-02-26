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
        public DbSet<Manifest> Manifests { get; set; }
        public DbSet<ManifestFile> ManifestFiles { get; set; }
    }
}
