using BoardBloom.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoardBloom.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Check if roles are already populated
                if (!context.Roles.Any())
                {
                    context.Roles.AddRange(
                        new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                        new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7212", Name = "User", NormalizedName = "User".ToUpper() }
                    );
                }

                var hasher = new PasswordHasher<ApplicationUser>();

                // Check if users are already populated
                if (!context.Users.Any())
                {
                    context.Users.AddRange(
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                            UserName = "admin@test.com",
                            EmailConfirmed = true,
                            NormalizedEmail = "ADMIN@TEST.COM",
                            Email = "admin@test.com",
                            NormalizedUserName = "ADMIN@TEST.COM",
                            PasswordHash = hasher.HashPassword(null, "Admin1!")
                        },
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                            UserName = "user@test.com",
                            EmailConfirmed = true,
                            NormalizedEmail = "USER@TEST.COM",
                            Email = "user@test.com",
                            NormalizedUserName = "USER@TEST.COM",
                            PasswordHash = hasher.HashPassword(null, "User1!")
                        }
                    );
                }

                // Check if user roles are already populated
                if (!context.UserRoles.Any())
                {
                    context.UserRoles.AddRange(
                        new IdentityUserRole<string> { RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210", UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0" },
                        new IdentityUserRole<string> { RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212", UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2" }
                    );
                }


                if (!context.Blooms.Any())
                {
                    context.Blooms.AddRange(
                        new Bloom
                        {
                            Title = "Bloom 1",
                            Content = "Content 1",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                        },
                        new Bloom
                        {
                            Title = "Bloom 2",
                            Content = "Content 2",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                        }
                    );
                }
                context.SaveChanges();

                if (!context.Comments.Any())
                {
                    context.Comments.AddRange(
                        new Comment
                        {
                            Content = "Comment 1",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                            BloomId = 1
                        },
                        new Comment
                        {
                            Content = "Comment 2",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                            BloomId = 2
                        }
                    );
                }

                if(!context.Likes.Any())
                {
                    context.Likes.AddRange(
                        new Like
                        {
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                            BloomId = 1
                        },
                        new Like
                        {
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                            BloomId = 2
                        }
                    );
                }

                if(!context.Boards.Any())
                {
                    context.Boards.AddRange(
                        new Board
                        {
                            Name = "Board 1",
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                        },
                        new Board
                        {
                            Name = "Board 2",
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                        }
                    );
                }
                context.SaveChanges();

                if (!context.BloomBoards.Any())
                {
                    context.BloomBoards.AddRange(
                        new BloomBoard
                        {
                            BloomId = 1,
                            BoardId = 1,
                            BoardDate = DateTime.Now
                        },
                        new BloomBoard
                        {
                            BloomId = 2,
                            BoardId = 2,
                            BoardDate = DateTime.Now
                        }
                    );
                }


                // Save any changes to the database
                context.SaveChanges();
            }
        }
    }
}
