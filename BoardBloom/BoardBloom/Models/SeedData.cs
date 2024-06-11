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
                            UserName = "theo@test.com",
                            EmailConfirmed = true,
                            NormalizedEmail = "THEO@TEST.COM",
                            Email = "theo@test.com",
                            NormalizedUserName = "THEO@TEST.COM",
                            PasswordHash = hasher.HashPassword(null, "Theo1!")
                        },
                        new ApplicationUser
                        {
                            Id = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                            UserName = "roby@test.com",
                            EmailConfirmed = true,
                            NormalizedEmail = "ROBY@TEST.COM",
                            Email = "roby.test.com",
                            NormalizedUserName = "ROBY@TEST.COM",
                            PasswordHash = hasher.HashPassword(null, "Roby1!")
                        }
                    );
                }

                // Check if user roles are already populated
                if (!context.UserRoles.Any())
                {
                    context.UserRoles.AddRange(
                        new IdentityUserRole<string> { RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210", UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0" },
                        new IdentityUserRole<string> { RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212", UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2" },
                        new IdentityUserRole<string> { RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212", UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4" }
                    );
                }


                if (!context.Blooms.Any())
                {
                    context.Blooms.AddRange(
                        new Bloom
                        {
                            Title = "AMazing PARROT !!!",
                            Content = "Look at it how red it is!",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                            Image = "https://i.natgeofe.com/n/e3ae5fbf-ddc9-4b18-8c75-81d2daf962c1/3948225.jpg"
                        },
                        new Bloom
                        {
                            Title = "What a nice cat :))",
                            Content = "PURRRRR",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                            Image = "https://th-thumbnailer.cdn-si-edu.com/ii_ZQzqzZgBKT6z9DVNhfPhZe5g=/fit-in/1600x0/filters:focal(1061x707:1062x708)/https://tf-cmsv2-smithsonianmag-media.s3.amazonaws.com/filer_public/55/95/55958815-3a8a-4032-ac7a-ff8c8ec8898a/gettyimages-1067956982.jpg"

                        },
                        new Bloom 
                        {
                            Title = "Simply Nature",
                            Content = "Nature is the best",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                            Image = "https://images.squarespace-cdn.com/content/v1/61c4da8eb1b30a201b9669f2/1696691175374-MJY4VWB1KS8NU3DE3JK1/Sounds-of-Nature.jpg"
                        },
                        new Bloom
                        {
                            Title = "Beautiful Sunset",
                            Content = "Sunset is the best",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                            Image = "https://t4.ftcdn.net/jpg/00/67/24/59/360_F_67245954_ejVa8C414CwJ9X0UadIFu1QEUjeLuFnO.jpg"
                        },
                        new Bloom
                        {
                            Title = "Amazing Waterfall",
                            Content = "Waterfall is the best",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                            Image = "https://www.americanrivers.org/wp-content/uploads/2022/08/Untitled-design-43-2-1024x576.png"
                        },
                        new Bloom
                        {
                            Title = "Roses...",
                            Content = "R.O.S.E.S",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                            Image = "https://m.media-amazon.com/images/I/613TchXe1WL._AC_UF894,1000_QL80_.jpg"
                        },
                        new Bloom
                        {
                            Title = "Funny Monkey",
                            Content = "L:O:L",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                            Image = "https://media.posterlounge.com/img/products/700000/697380/697380_poster.jpg"
                        }
                    );
                }
                context.SaveChanges();

                if (!context.Comments.Any())
                {
                    context.Comments.AddRange(
                        new Comment
                        {
                            Content = "Look how red it is!!!!",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                            BloomId = 1
                        },
                        new Comment
                        {
                            Content = "Can't belive such beauty exists!",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                            BloomId = 1
                        },
                        new Comment
                        {
                            Content = "I know right!",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                            BloomId = 1
                        },
                        new Comment
                        {
                            Content = "PUR",
                            Date = DateTime.Now,
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                            BloomId = 2
                        },
                        new Comment
                        {
                            Content = "PURrrrr",
                            Date = DateTime.Now,
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
                            Name = "Animals Board",
                            UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                        },
                        new Board
                        {
                            Name = "AMAZING Board",
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
                            BoardId = 1,
                            BoardDate = DateTime.Now
                        },
                        new BloomBoard
                        {
                            BloomId = 7,
                            BoardId = 1,
                            BoardDate = DateTime.Now
                        },
                        new BloomBoard
                        {
                            BloomId = 3,
                            BoardId = 2,
                            BoardDate = DateTime.Now
                        },
                        new BloomBoard
                        {
                            BloomId = 4,
                            BoardId = 2,
                            BoardDate = DateTime.Now
                        },
                        new BloomBoard
                        {
                            BloomId = 5,
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
