
using BoardBloom.Data;
using BoardBloom.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;


namespace BoardBloom.Controllers
{
    public class CommunitiesController : Controller
    {

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public CommunitiesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        [HttpPost]
        //[Authorize(Roles = "User,Admin")]
        public IActionResult New(Community community)
        {
            community.CreatedBy= _userManager.GetUserId(User);
            community.CreatedDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Communities.Add(community);
                 db.SaveChangesAsync();
                return Redirect("/Communities/Show/" + community.Id);
            }
            else
            {
                return View("New");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Join([FromQuery]int id)
        {
            var community = db.Communities.Find(id);
            if (community == null)
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync(User);

            community.Users.Add(user);
            await db.SaveChangesAsync();

            return Redirect("/Communities/Show/" + community.Id);
        }

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show([FromQuery] int id)
        {

            var community = db.Communities
                .Include(c => c.Users)
                .Include(c => c.Moderators)
                .Include(c => c.Blooms)
                .FirstOrDefault(c => c.Id == id);

            if (community == null)
            {
                return NotFound();
            }

            return View(community);
        }
    }
}

