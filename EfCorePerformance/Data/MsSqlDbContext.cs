using EFTestApp.Model;
using Microsoft.EntityFrameworkCore;

namespace EFTestApp.Data
{
    public sealed class MsSqlDbContext : DbContext
    {
        public MsSqlDbContext(DbContextOptions<MsSqlDbContext> options) :
            base(options)
        {
            // Database.AutoTransactionsEnabled = false;
        }
        
        public DbSet<AutoGenId> AutoGenId { set; get; }
        
        public DbSet<Guid> Guid { set; get; }
    }
}