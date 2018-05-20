using Microsoft.EntityFrameworkCore;
using WebApplication7.Models.EntitySQL;

namespace QUANLYBANHANG.Models
{
    public class AccessContext : DbContext
    {
        public AccessContext(DbContextOptions<AccessContext> options) : base(options)
        {
        }
        public DbSet<Permission> Permission { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>().ToTable("Permission");
        }
    }
}
