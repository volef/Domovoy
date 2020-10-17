using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace BookmakerHouse.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Chat> Chats { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Chat>()
                .HasKey(a => a.Id);
            builder.Entity<Chat>()
                .Property(a => a.Id)
                .ValueGeneratedNever();
            builder.Entity<Chat>()
                .Property(a => a.lat)
                .HasColumnType("decimal(18,2)");
            builder.Entity<Chat>()
                .Property(a => a.@long)
                .HasColumnType("decimal(18,2)");

            base.OnModelCreating(builder);
        }
    }
}