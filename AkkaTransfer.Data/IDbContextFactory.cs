using AkkaTransfer.Data;
using Microsoft.EntityFrameworkCore;

namespace AkkaTransfer
{
    public interface IDbContextFactory
    {
        ReceiveDbContext CreateDbContext();
    }
}