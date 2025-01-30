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
        public virtual DbSet<Board> Boards { get; set; }
        public virtual DbSet<BloomBoard> BloomBoards { get; set; }
        public virtual DbSet<Bloom> Blooms { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Like> Likes { get; set; }

        public virtual DbSet<Community> Communities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BloomBoard>()
                .HasKey(bb => bb.Id);  // If Id is intended to be the primary key.

            // Correcting the relationships
            modelBuilder.Entity<BloomBoard>()
             .HasOne(bb => bb.Bloom)
             .WithMany(b => b.BloomBoards)
             .HasForeignKey(bb => bb.BloomId)
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BloomBoard>()
                .HasOne(bb => bb.Board)
                .WithMany(b => b.BloomBoards)
                .HasForeignKey(bb => bb.BoardId)
                .OnDelete(DeleteBehavior.Cascade);


            // Configure the relationship for CreatedBy
            modelBuilder.Entity<Community>()
                .HasOne(c => c.CreatedByNavigation)
                .WithMany(u => u.CreatedCommunities)
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict); 

            // Configure the many-to-many relationship for users in a community
            modelBuilder.Entity<Community>()
                .HasMany(c => c.Users)
                .WithMany(u => u.Communities)
                .UsingEntity<Dictionary<string, object>>(
                    "UserCommunity",
                    j => j.HasOne<ApplicationUser>().WithMany().HasForeignKey("UserId"),
                    j => j.HasOne<Community>().WithMany().HasForeignKey("CommunityId"));

            // relationship for moderators
            modelBuilder.Entity<Community>()
               .HasMany(c => c.Moderators)
               .WithMany(u => u.ModeratedCommunities)
               .UsingEntity<Dictionary<string, object>>(
                   "ModeratorCommunity",
                   j => j.HasOne<ApplicationUser>().WithMany().HasForeignKey("UserId"),
                   j => j.HasOne<Community>().WithMany().HasForeignKey("CommunityId"));

        }
    }
}
