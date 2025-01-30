    
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
using System.Xml.Linq;


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



        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> Index()
        {
            ViewBag.communities = await db.Communities
                .Include(c => c.Users)
                .Include(c => c.Blooms)
                .Include(c => c.Moderators)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> GetCommunitiesByName([FromQuery] string? name)
        {
            ViewBag.communities = await db.Communities
                .Include(c => c.Users)
                .Include(c => c.Blooms)
                .Include(c => c.Moderators)
                .Where(c => c.Name.Contains(name ?? ""))
                .ToListAsync();

            return PartialView("_CommunitiesListPartial");
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
                // First save the community
                db.Communities.Add(community);
                await db.SaveChangesAsync();

                // Now that we have the community ID, we can set up relationships
                community.Users = new List<ApplicationUser> { user };
                community.Moderators = new List<ApplicationUser> { user };

                // Save the relationships
                await db.SaveChangesAsync();

                return Redirect("/Communities/Show/" + community.Id);
            }
            else
            {
                return View("New");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Join(int id)
        {
            var community = db.Communities
                .Include(c => c.Users)
                .Include(c => c.Moderators)
                .FirstOrDefault(c => c.Id == id);

            var user = await _userManager.GetUserAsync(User);
            if (community == null)
            {
                return NotFound();
            }

            AddUserToCommunity(ref community, user);
            await db.SaveChangesAsync();  // Added explicit save

            return RedirectToAction("Show", new { id = community.Id });
        }




        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
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

            ViewBag.IsUserInCommunity = community.Users.Contains(_userManager.GetUserAsync(User).Result);
            ViewBag.IsUserModerator = community.Moderators.Contains(_userManager.GetUserAsync(User).Result);
            ViewBag.IsUserCreator = community.CreatedBy == _userManager.GetUserAsync(User).Result.Id;

            return View(community);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Leave(int id)
        {
            var community = db.Communities
                .Include(c => c.Users)
                .Include(c => c.Moderators)
                .FirstOrDefault(c => c.Id == id);

            var user = await _userManager.GetUserAsync(User);

            if (community == null)
            {
                return NotFound();
            }

            if (community.CreatedBy == user.Id)
            {
                return BadRequest("You cannot leave a community you created");
            }

            if (community.Users != null && community.Users.Contains(user))
            {
                community.Users.Remove(user);
            }

            if (community.Moderators != null && community.Moderators.Contains(user))
            {
                community.Moderators.Remove(user);
            }

            await db.SaveChangesAsync();  // Make sure to save the changes

            return RedirectToAction("Show", new { id = community.Id });
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> MakeUserModerator([FromQuery] int communityId, [FromQuery] string userId)
        {
            var community = db.Communities.Find(communityId);

            var user = await _userManager.FindByIdAsync(userId);
            if (community == null)
            {
                return NotFound();
            }

            if (community.CreatedBy != _userManager.GetUserAsync(User).Result.Id)
            {
                return BadRequest("You cannot make a user moderator in a community you did not create");
            }

            if (community.Moderators.Contains(user))
            {
                return BadRequest("User is already a moderator");
            }

            community.Moderators.Add(user);

            await db.SaveChangesAsync();

            return Redirect("/Communities/Show/" + community.Id);
        }



        //all the logic for adding a user to a community,
        //! community should be transmitted by reference in case it is not tracked by the db context
        [NonAction]
        private void AddUserToCommunity(ref Community community, ApplicationUser user)
        {
            if (community.Users == null)
                community.Users = new List<ApplicationUser>();

            if (community.Moderators == null)
                community.Moderators = new List<ApplicationUser>();

            if (!community.Users.Contains(user))
            {
                community.Users.Add(user);

                if (community.CreatedBy == user.Id)
                {
                    community.Moderators.Add(user);
                }

                db.Entry(community).State = EntityState.Modified;
            }
        }


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> UpdateModeratorStatus(int communityId, string userId, bool isPromoting)
        {
            var community = await db.Communities
                .Include(c => c.Users)
                .Include(c => c.Moderators)
                .FirstOrDefaultAsync(c => c.Id == communityId);

            var user = await _userManager.FindByIdAsync(userId);
            var currentUser = await _userManager.GetUserAsync(User);

            if (community == null || user == null)
            {
                return NotFound();
            }

            // Only creator can promote/demote moderators
            if (community.CreatedBy != currentUser.Id)
            {
                return BadRequest("Only the community creator can manage moderators");
            }

            if (isPromoting)
            {
                if (!community.Moderators.Contains(user))
                {
                    community.Moderators.Add(user);
                }
            }
            else
            {
                if (community.Moderators.Contains(user))
                {
                    community.Moderators.Remove(user);
                }
            }

            await db.SaveChangesAsync();
            return RedirectToAction("Show", new { id = communityId });
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> KickUser(int communityId, string userId)
        {
            var community = await db.Communities
                .Include(c => c.Users)
                .Include(c => c.Moderators)
                .FirstOrDefaultAsync(c => c.Id == communityId);

            var userToKick = await _userManager.FindByIdAsync(userId);
            var currentUser = await _userManager.GetUserAsync(User);

            if (community == null || userToKick == null)
            {
                return NotFound();
            }

            // Check if current user is creator or moderator
            if (community.CreatedBy != currentUser.Id && !community.Moderators.Contains(currentUser))
            {
                return BadRequest("Only moderators can kick users");
            }

            // Can't kick the creator
            if (userToKick.Id == community.CreatedBy)
            {
                return BadRequest("Cannot kick the community creator");
            }

            // Can't kick other moderators unless you're the creator
            if (community.Moderators.Contains(userToKick) && community.CreatedBy != currentUser.Id)
            {
                return BadRequest("Only the creator can kick moderators");
            }

            if (community.Users.Contains(userToKick))
            {
                community.Users.Remove(userToKick);
                // Also remove from moderators if applicable
                if (community.Moderators.Contains(userToKick))
                {
                    community.Moderators.Remove(userToKick);
                }
            }

            await db.SaveChangesAsync();
            return RedirectToAction("Show", new { id = communityId });
        }


    }
}

