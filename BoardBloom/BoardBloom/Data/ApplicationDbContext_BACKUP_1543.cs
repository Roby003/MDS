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
<<<<<<< .merge_file_lPqy4M
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BloomBoard> BloomBoards{ get; set; }


=======

        public DbSet<Bloom> Blooms { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
>>>>>>> .merge_file_XHQOu6
    }
}