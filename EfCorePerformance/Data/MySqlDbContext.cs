using EFTestApp.Model;
using Microsoft.EntityFrameworkCore;

namespace EFTestApp.Data
{
    public class MySqlDbContext : DbContext
    {
        public MySqlDbContext(DbContextOptions<MySqlDbContext> options) :
            base(options)
        {
            
        }
        
        public DbSet<AutoGenId> AutoGenId { set; get; }
    }
}