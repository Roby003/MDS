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

            int _perPage = 5;

            var blooms = db.Blooms.Include("User")
                            .Where(b => b.UserId == userId )
                            .OrderByDescending(a => a.TotalLikes);

            int totalItems = blooms.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            ViewBag.CurrentPage = currentPage;

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedBlooms = blooms.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            // userii si blooms

            if (userId == null)
            {
                userId = _userManager.GetUserId(User);
            }

            var user = db.Users.Include(u => u.Blooms).SingleOrDefault(u => u.Id == userId);


            // .Where(u => u.Id == userId)
            // .FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {

                return NotFound();
            }

            var boards = db.Boards
                .Include("User")
                .ToList();

            ViewBag.Boards = boards.ToList();

            ViewBag.UserCurent = _userManager.GetUserId(User);

            //bloom urile placute de user
            var likedBlooms = db.Likes
               .Where(l => l.UserId == userId)
                .Select(l => l.Bloom)
                .ToList();

            ViewBag.Blooms = paginatedBlooms;

            return View(user);
        }
    }
}
