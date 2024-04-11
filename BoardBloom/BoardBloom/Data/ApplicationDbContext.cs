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
			// definirea relatiei many-to-many dintre Bookmark si Category

			base.OnModelCreating(modelBuilder);

			// definire primary key compus
			modelBuilder.Entity<BloomBoard>()
				.HasKey(bc => new { bc.Id, bc.BloomId, bc.BoardId });


			// definire relatii cu modelele Bookmark si Category
			//Foreign Key

			modelBuilder.Entity<BloomBoard>()
				.HasOne(bc => bc.Bloom)
				.WithMany(bc => bc.BloomBoards)
				.HasForeignKey(bc => bc.BoardId);

			modelBuilder.Entity<BloomBoard>()
				.HasOne(bc => bc.Board)
				.WithMany(bc => bc.BloomBoards)
				.HasForeignKey(bc => bc.BoardId);
		}
	}
}
