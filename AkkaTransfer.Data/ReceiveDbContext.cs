using Microsoft.EntityFrameworkCore;

namespace AkkaTransfer.Data
{
    public class ReceiveDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=receive.db");
        }

        public DbSet<FileHeader> FileHeaders { get; set; }
        public DbSet<FilePiece> FilePieces { get; set; }
    }
}
