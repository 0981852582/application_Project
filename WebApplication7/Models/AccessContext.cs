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
        public DbSet<MenuBar> MenuBar { get; set; }
        public DbSet<StatusMenuBar> StatusMenuBar { get; set; }
        public DbSet<PermissionOfPage> PermissionOfPage { get; set; }
        public DbSet<MenuOfPage> MenuOfPage { get; set; }
        public DbSet<Account> Account { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>().ToTable("Permission");
            modelBuilder.Entity<MenuBar>().ToTable("MenuBar");
            modelBuilder.Entity<StatusMenuBar>().ToTable("StatusMenuBar");
            modelBuilder.Entity<PermissionOfPage>().ToTable("PermissionOfPage");
            modelBuilder.Entity<MenuOfPage>().ToTable("MenuOfPage");
            modelBuilder.Entity<Account>().ToTable("Account");
        }
    }
}
