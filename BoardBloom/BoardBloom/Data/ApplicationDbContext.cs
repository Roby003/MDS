using BoardBloom.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BoardBloom.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BloomBoard> BloomBoards{ get; set; }
        public DbSet<Bloom> Blooms { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<BloomBoard>()
        .HasKey(bb => bb.Id);  // If Id is intended to be the primary key.

            // Correcting the relationships
            modelBuilder.Entity<BloomBoard>()
             .HasOne(bb => bb.Bloom)
             .WithMany(b => b.BloomBoards)
             .HasForeignKey(bb => bb.BloomId);

            modelBuilder.Entity<BloomBoard>()
                .HasOne(bb => bb.Board)
                .WithMany(b => b.BloomBoards)
                .HasForeignKey(bb => bb.BoardId);
        }
    }
}
