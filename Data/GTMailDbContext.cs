using GTBStatementService.Models;
using Microsoft.EntityFrameworkCore;

namespace GTBStatementService.Data
{
    public class GTMailDbContext : DbContext
    {
        public GTMailDbContext(DbContextOptions<GTMailDbContext> options) : base(options)
        {
        }

        public DbSet<CustomerProfile> Profiles { get; set; }
        public DbSet<Statement> Statements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CustomerProfile>(entity =>
            {
                entity.ToTable("Profile", "dbo");
                entity.HasKey(e => e.CustomerNo);
            });
        }
    }
}
