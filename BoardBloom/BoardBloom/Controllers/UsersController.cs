using BoardBloom.Data;
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
        public IActionResult Index()
        {
            var users = from user in db.Users
                        orderby user.UserName
                        select user;

            ViewBag.UsersList = users;


            return View();
        }

        public async Task<ActionResult> Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.Roles = roles;

            return View(user);
        }

        public async Task<ActionResult> Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);

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
        public IActionResult Delete(string id)
        {
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

            int _perPage = 5;

            var blooms = db.Blooms.Include("User")
                            .Where(b => b.UserId == userId )
                            .OrderByDescending(a => a.TotalLikes)
                            .ToList();

            ViewBag.Blooms = blooms;
            
            var user = db.Users.Find(userId);

            var boards = db.Boards.Include("BloomBoards")
                            .Where(b => b.UserId == userId)
                            .Include("User")
                            .Include("BloomBoards.Bloom")
                            .ToList();

            ViewBag.Boards = boards;

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
