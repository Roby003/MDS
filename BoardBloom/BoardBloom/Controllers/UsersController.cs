﻿using BoardBloom.Data;
using BoardBloom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BoardBloom.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }

        [Authorize(Roles = "User, Admin")]
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var _perPage = 8; // Number of users per page

            var query = db.Users.AsQueryable();
            ViewBag.CurrentSearch = search;

            // Apply search if provided
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(u =>
                    u.UserName.ToLower().Contains(searchLower) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(searchLower)));
            }

            // Include required relationships and order
            query = query
                .Include(u => u.Blooms)
                .Include(u => u.Comments)
                .Include(u => u.Communities)
                .OrderBy(u => u.UserName);

            // Calculate pagination
            var totalUsers = await query.CountAsync();
            ViewBag.LastPage = Math.Ceiling((decimal)totalUsers / _perPage);
            ViewBag.CurrentPage = page;

            var users = await query
                .Skip((page - 1) * _perPage)
                .Take(_perPage)
                .ToListAsync();

            // Get most recent bloom for each user
            var userRecentBlooms = new Dictionary<string, Bloom>();
            foreach (var user in users)
            {
                var recentBloom = await db.Blooms
                    .Where(b => b.UserId == user.Id)
                    .OrderByDescending(b => b.Date)
                    .FirstOrDefaultAsync();

                if (recentBloom != null)
                {
                    userRecentBlooms[user.Id] = recentBloom;
                }
            }

            ViewBag.UserRecentBlooms = userRecentBlooms;
            ViewBag.Users = users;

            return View();
        }

        [Authorize(Roles = "User, Admin")]
        public async Task<ActionResult> Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.Roles = roles;

            return View(user);
        }

        [Authorize(Roles = "User, Admin")]
        public async Task<ActionResult> Edit(string id)
        {
            if (id != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
            {
                return RedirectToAction("Index");
            }
            ApplicationUser user = db.Users.Find(id);


            // metoda asta e cea mai ciudata pe care am vazut o vreodata
            user.AllRoles = GetAllRoles();

            var roleNames = await _userManager.GetRolesAsync(user); // Lista de nume de roluri

            // Cautam ID-ul rolului in baza de date
            var currentUserRole = _roleManager.Roles
                                                .Where(r => roleNames.Contains(r.Name))
                                                .Select(r => r.Id)
                                                .First(); // Selectam 1 singur rol
            ViewBag.UserRole = currentUserRole;
            
            
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole)
        {
            ApplicationUser user = db.Users.Find(id);

            user.AllRoles = GetAllRoles();


            if (ModelState.IsValid)
            {
                user.UserName = newData.UserName;
                user.Email = newData.Email;
                user.FirstName = newData.FirstName;
                user.LastName = newData.LastName;
                user.PhoneNumber = newData.PhoneNumber;


                // Cautam toate rolurile din baza de date
                var roles = db.Roles.ToList();

                foreach (var role in roles)
                {
                    // Scoatem userul din rolurile anterioare
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                // Adaugam noul rol selectat
                var roleName = await _roleManager.FindByIdAsync(newRole);
                await _userManager.AddToRoleAsync(user, roleName.ToString());

                db.SaveChanges();

            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult Delete(string id)
        {
            if (id != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
            {
                return RedirectToAction("Index");
            }
            var user = db.Users
                         .Include("Comments")
                         .Include("Blooms")
                         .Where(u => u.Id == id)
                         .First();

            // Delete user comments
            if (user.Comments.Count > 0)
            {
                foreach (var comment in user.Comments)
                {
                    db.Comments.Remove(comment);
                }
            }

            // Delete user blooms
            if (user.Blooms.Count > 0)
            {
                foreach (var bloom in user.Blooms)
                {

                    db.Blooms.Remove(bloom);
                }
            }

            var userLikes = db.Likes.Where(l => l.UserId == id);

            foreach (var lk in userLikes)
            {

                var likedBlooms = db.Blooms.SingleOrDefault(b => b.Id == lk.BloomId);

                if (likedBlooms != null)
                {
                    likedBlooms.TotalLikes--;
                    db.Likes.Remove(lk);
                }

            }

            var board = db.Boards.Include(c => c.BloomBoards)
                                         .ThenInclude(bc => bc.Bloom)
                                         .FirstOrDefault(cat => cat.UserId == id);

            if (board != null)
            {
                foreach (var bloomBoard in board.BloomBoards)
                {
                    db.BloomBoards.Remove(bloomBoard);
                }

                db.Boards.Remove(board);

                //db.SaveChanges();
            }



            db.ApplicationUsers.Remove(user);

            db.SaveChanges();

            return RedirectToAction("Index");
        }


        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }


        public IActionResult UserProfile(string userId)
        {
            // Get the user with their communities included
            var user = db.Users
                .Include(u => u.Communities)
                    .ThenInclude(c => c.Users)
                .Include(u => u.Communities)
                    .ThenInclude(c => c.Blooms)
                .Include(u => u.Communities)
                    .ThenInclude(c => c.Moderators)
                .FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            // Get user's blooms with all necessary data
            var blooms = db.Blooms
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Date)
                .ToList();

            ViewBag.Blooms = blooms;

            // Get user's boards with all necessary relationships
            var boards = db.Boards
                .Include(b => b.BloomBoards)
                    .ThenInclude(bb => bb.Bloom)
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Id)
                .ToList();

            ViewBag.Boards = boards;

            // Get user's moderated communities
            var moderatedCommunities = db.Communities
                .Include(c => c.Moderators)
                .Where(c => c.Moderators.Any(m => m.Id == userId))
                .ToList();

            ViewBag.ModeratedCommunities = moderatedCommunities;

            // Get communities created by the user
            var createdCommunities = db.Communities
                .Include(c => c.Users)
                .Include(c => c.Blooms)
                .Include(c => c.Moderators)
                .Where(c => c.CreatedBy == userId)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            ViewBag.CreatedCommunities = createdCommunities;

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfilePicture(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

       [HttpPost]
        public async Task<IActionResult> ChangeProfilePicture(string id, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                // Handle the case where no file was uploaded
                ModelState.AddModelError("File", "Please upload a file.");
                return RedirectToAction("EditProfilePicture", new { id = id });
            }

            // Ensure the file is an image
            if (!file.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("File", "Only image files are allowed.");
                return RedirectToAction("EditProfilePicture", new { id = id });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                // Handle the case where the user does not exist
                return NotFound();
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);

                // Check the size of the image; adjust size limit as necessary
                if (memoryStream.Length < 2097152)  // less than 2 MB
                {
                    user.ProfilePicture = memoryStream.ToArray();
                }
                else
                {
                    ModelState.AddModelError("File", "The file is too large.");
                    return RedirectToAction("EditProfilePicture", new { id = id });
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("UserProfile", new { userId = user.Id });
            }
            else
            {
                // Handle errors during update
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return RedirectToAction("EditProfilePicture", new { id = id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProfilePicture(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                // Handle the case where the user does not exist
                return NotFound();
            }

            user.ProfilePicture = null;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok();
            }
            else
            {
                // Handle errors during update
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest();
            }
        }
    }
}
