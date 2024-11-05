
using BoardBloom.Data;
using BoardBloom.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MessagePack.Formatters;
using System.ComponentModel;
using System.Diagnostics;


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


        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> New([FromForm] Community community)
        {
            var user = await _userManager.GetUserAsync(User);
            ModelState.ClearValidationState(nameof(community));
            community.CreatedDate = DateTime.Now;
            community.CreatedBy = user.Id;


            if (TryValidateModel(community, nameof(community)))
            {
                AddUserToCommunity(ref community, user);
                db.Communities.Add(community);
                db.SaveChanges();
                return Redirect("/Communities/Show/" + community.Id);
            }
            else
            {
                return View("New");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Join([FromQuery] int id)
        {
            var community = db.Communities.Find(id);

            var user = await _userManager.GetUserAsync(User);
            if (community == null)
            {
                return NotFound();
            }


            AddUserToCommunity(ref community, user);


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

        [NonAction]
        private int AddUserToCommunity(ref Community community, ApplicationUser user)
        {
            if (community.Users.Contains(user))
            {
                return 0;
            }

            if (community.CreatedBy == user.Id)
            {
                community.Moderators.Add(user);
            }

            community.Users.Add(user);


            if (db.Entry(community).State == EntityState.Modified) // check if entity is tracked 
            {
                return db.SaveChanges();
            }
            else return 1;
        }
    }
}

