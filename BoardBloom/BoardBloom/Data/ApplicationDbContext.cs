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

            // BloomBoard configuration
            modelBuilder.Entity<BloomBoard>(entity =>
            {
                entity.HasKey(bb => bb.Id);

                entity.HasOne(bb => bb.Bloom)
                    .WithMany(b => b.BloomBoards)
                    .HasForeignKey(bb => bb.BloomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bb => bb.Board)
                    .WithMany(b => b.BloomBoards)
                    .HasForeignKey(bb => bb.BoardId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Community configuration
            modelBuilder.Entity<Community>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.CreatedByNavigation)
                    .WithMany(u => u.CreatedCommunities)
                    .HasForeignKey(c => c.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure Users relationship
                entity.HasMany(c => c.Users)
                    .WithMany(u => u.Communities)
                    .UsingEntity(
                        "UserCommunity",
                        l => l.HasOne(typeof(ApplicationUser))
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade),
                        r => r.HasOne(typeof(Community))
                            .WithMany()
                            .HasForeignKey("CommunityId")
                            .OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey("CommunityId", "UserId");
                            j.ToTable("UserCommunity");
                        }
                    );

                // Configure Moderators relationship
                entity.HasMany(c => c.Moderators)
                    .WithMany(u => u.ModeratedCommunities)
                    .UsingEntity(
                        "ModeratorCommunity",
                        l => l.HasOne(typeof(ApplicationUser))
                            .WithMany()
                            .HasForeignKey("UserId")
                            .OnDelete(DeleteBehavior.Cascade),
                        r => r.HasOne(typeof(Community))
                            .WithMany()
                            .HasForeignKey("CommunityId")
                            .OnDelete(DeleteBehavior.Cascade),
                        j =>
                        {
                            j.HasKey("CommunityId", "UserId");
                            j.ToTable("ModeratorCommunity");
                        }
                    );
            });
        }
    }
}