using Microsoft.EntityFrameworkCore;

namespace App.API.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(500);
                entity.Property(e => e.NameEmbedding).HasColumnType("vector(1536)");
            });
        }
    }
}
