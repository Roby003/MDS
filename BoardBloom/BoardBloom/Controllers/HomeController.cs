using BoardBloom.Data;
using BoardBloom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BoardBloom.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private IWebHostEnvironment _env;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env
            )
        {
            _logger = logger;

            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _env = env;
        }

        [Authorize(Roles = "User, Admin")]
        public IActionResult Index()
        {
            // Get recent blooms - limit to 6 most recent
            var recentBlooms = db.Blooms
                .Include(b => b.User)
                .Include(b => b.Comments)
                .Include(b => b.Likes)
                .OrderByDescending(b => b.Date)
                .Take(6)
                .ToList();

            ViewBag.RecentBlooms = recentBlooms;

            // Get popular communities - based on member count and activity
            var popularCommunities = db.Communities
                .Include(c => c.Users)
                .Include(c => c.Blooms)
                .Include(c => c.Moderators)
                .OrderByDescending(c => c.Users.Count)
                .ThenByDescending(c => c.Blooms.Count)
                .Take(3)  // Show top 5 communities
                .ToList();

            ViewBag.PopularCommunities = popularCommunities;

            // Get top users - based on contributions (blooms, comments, communities)
            var topUsers = db.Users
                .Include(u => u.Blooms)
                .Include(u => u.Comments)
                .Include(u => u.Communities)
                .Select(u => new
                {
                    User = u,
                    Score = (u.Blooms.Count * 3) + (u.Comments.Count) + (u.Communities.Count * 2)
                })
                .OrderByDescending(u => u.Score)
                .Take(5)  // Show top 10 users
                .Select(u => u.User)
                .ToList();

            ViewBag.TopUsers = topUsers;

            // If user is authenticated, get their communities
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var userCommunities = db.Communities
                    .Include(c => c.Users)
                    .Where(c => c.Users.Any(u => u.Id == userId))
                    .Take(3)  // Show 3 of their communities
                    .ToList();

                ViewBag.UserCommunities = userCommunities;
            }

            return View();
        }


        [Authorize(Roles = "User, Admin")]
        public IActionResult Privacy()
        {
            return View();
        }

		[Authorize(Roles = "User, Admin")]
		public IActionResult TermsOfService()
		{
			return View();
		}

		[Authorize(Roles = "User, Admin")]
		public IActionResult Guidelines()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("404");
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}