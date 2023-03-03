namespace AkkaTransfer.Data
{
    public class DbContextFactory : IDbContextFactory
    {
        public ReceiveDbContext CreateDbContext()
        {
            return new ReceiveDbContext();
        }
    }
}
