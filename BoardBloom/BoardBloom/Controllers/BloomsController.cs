using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoardBloom.Data;
using BoardBloom.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace BoardBloom.Controllers
{
    public class BloomsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private IWebHostEnvironment _env;

        private readonly ICompositeViewEngine _viewEngine;

        public BloomsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env,
            ICompositeViewEngine viewEngine
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _env = env;

            _viewEngine = viewEngine;
        }


        //[Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            int _perPage = 5;

            var search = "";

            // IF there is a search query in the URL
            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();
            }


            var searchedBlooms = db.Blooms.Include("User")
                                      .Where(book => book.Title.Contains(search)
                                        || book.Content.Contains(search))
                                      .OrderByDescending(b => b.TotalLikes)
                                      .ToList();


            
            var blooms = db.Blooms.Include("User").OrderByDescending(b => b.TotalLikes);

            // setting the message for the user
            if (TempData != null &&TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            // Pagination
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedBlooms = searchedBlooms.Skip(offset).Take(_perPage);

            int totalItems = searchedBlooms.Count();

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);


            // If there is a search query in the URL
            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Blooms/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Blooms/Index/?page";
            }


            ViewBag.blooms = blooms;

            var userId = _userManager.GetUserId(User);

            var userLikes = db.Likes
                .Where(l => l.UserId == userId)
                .Select(l => l.BloomId)
                .ToList();

            ViewData["UserLikes"] = userLikes;

            ViewBag.Blooms = paginatedBlooms;


            ViewBag.Search = search;

            return View();
        }


        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            Bloom bloom = db.Blooms.Include("User")
                                         .Include("Comments")
                                         .Include("Likes")
                                         .Include("Comments.User")
                                         .Where(bl => bl.Id == id)
                                         .First();

            // Check if the current user has liked the post
            var isLiked = db.Likes
                .Where(l => l.BloomId == id)
                .Where(l => l.UserId == _userManager.GetUserId(User))
                .Count() > 0;

            ViewBag.IsLiked = isLiked;

            SetAccessRights();

            if (TempData!= null && TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            ViewBag.isBloomEditable = true;
            return View(bloom);
        }

        // Se afiseaza formularul in care se vor completa datele unui bloom
        [Authorize(Roles = "User,Admin")]
        public IActionResult New([FromQuery] int? communityId) 
        {

            Bloom bloom = new Bloom();
            ViewBag.communityId = communityId;
            return View(bloom);
        }

        [Authorize(Roles = "User,Admin")]
        // eu aici as zice ca admin-ul nu are de ce sa editeze un board doar sa il stearga daca e impotriba TOS
        public IActionResult Edit(int id)
        {
            Bloom bloom = db.Blooms
                .Include(b => b.User)
                .Where(b => b.Id == id)
                .FirstOrDefault();
            var _=_userManager.GetUserId(User);

            // Check if the current user can edit the post
            if (bloom.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(bloom);
            }
            else
            {
                if (TempData != null)
                {
                    TempData["message"] = "Nu aveti dreptul sa editati un bloom care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                }

                return RedirectToAction("Index", "Home");
            }
        }   
        
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(Bloom bloom)
        {
            if (ModelState.IsValid)
            {
                // Find the bloom in the database
                var oldBloom = db.Blooms.Find(bloom.Id);

                if (oldBloom != null)
                {
                    oldBloom.Title = bloom.Title;
                    oldBloom.Content = bloom.Content;
                    oldBloom.Image = bloom.Image;

                    db.SaveChanges();

                    return RedirectToAction("Show", new { id = bloom.Id });
                }
                else
                {
                    return NotFound(); // Return a 404 Not Found response if the bloom is not found in the database
                }
            }
            else
            {
                // If ModelState is not valid, return BadRequest with ModelState errors
                return BadRequest(ModelState);
            }

            return BadRequest();
        }


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Bloom bloom = db.Blooms.Find(id);

            if (bloom.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Likes.RemoveRange(db.Likes.Where(l => l.BloomId == id));
                db.Comments.RemoveRange(db.Comments.Where(c => c.BloomId == id));

                db.Blooms.Remove(bloom);
                db.SaveChanges();
                if (TempData != null)
                {
                    TempData["message"] = "Bloom-ul a fost sters";
                    TempData["messageType"] = "alert-success";
                }
          

                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (TempData != null)
                {
                    TempData["message"] = "Nu aveti dreptul sa stergeti un bloom care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                }

                return RedirectToAction("Index", "Home");
            }
        }


        // Conditiile de afisare a butoanelor 
        public void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("User"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");

            ViewBag.UserCurent = _userManager.GetUserId(User);
        }

        public List<int?> GetCurrentUserLikes()
        {
            List<int?> userLikes = new List<int?>();

            if (User.Identity.IsAuthenticated)
            {
                string userId = _userManager.GetUserId(User);

                var likedBlooms = db.Likes
                                       .Where(l => l.UserId == userId)
                                       .Select(l => l.BloomId)
                                       .ToList();

                userLikes.AddRange(likedBlooms);
            }

            return userLikes;
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Like([FromQuery]int bloomId)
        {
            var bloom = db.Blooms.Find(bloomId);
            var userId = _userManager.GetUserId(User);
            var userLikedPost = false;

            if (!db.Likes.Any(l => l.BloomId == bloomId && l.UserId == userId))
            {
                // Create a new like
                var like = new Like
                {
                    BloomId = bloomId,
                    UserId = userId
                };

                bloom.TotalLikes++;

                db.Likes.Add(like);
                db.SaveChanges();

                ViewData["UserLikes"] = GetCurrentUserLikes();
                userLikedPost = true;
            }
            else
            {
                var like = db.Likes.FirstOrDefault(l => l.BloomId == bloomId && l.UserId == userId);

                
                if(like != null)
                {
                    bloom.TotalLikes--;
                    db.Likes.Remove(like);
                    db.SaveChanges();
                }
            }
            return Json(new { LikeCount = bloom.TotalLikes, userLikedPost=userLikedPost });
            
        }

        [Authorize(Roles ="User,Admin")]
        public IActionResult CheckLike(int bloomId)
        {
            var userId = _userManager.GetUserId(User);

            var liked = false;
            // Check if the user has liked the post
            if(db.Likes.Any(l => l.BloomId == bloomId && l.UserId == userId))
            {
                liked = true;
            }
            return Json(new { liked = liked });
        }
        
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Preview([FromBody] Bloom bloom)
        {
            if (ModelState.IsValid)
            {
                var user = db.ApplicationUsers.Find(_userManager.GetUserId(User));

                // Create a new Bloom object with the data from the request
                Bloom previewBloom = new Bloom
                {
                    Title = bloom.Title,
                    Content = bloom.Content,
                    Image = bloom.Image,
                    Date = DateTime.Now,
                    User = user,
                    Likes = new List<Like>(),
                    Comments = new List<Comment>(),
                    TotalLikes = 0,
                    UserId = user.Id,
                    BloomBoards = new List<BloomBoard>(),
                };

                ViewBag.isBloomEditable = false;

                // Render the partial view to a string
                var viewResult = _viewEngine.FindView(ControllerContext, "_BloomPartial", false);
                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), ModelState);
                viewData.Model = previewBloom;

                using (var writer = new StringWriter())
                {   
                    // Render the view to a string
                    var viewContext = new ViewContext(ControllerContext, viewResult.View, viewData, TempData, writer, new HtmlHelperOptions());
                    viewResult.View.RenderAsync(viewContext).GetAwaiter().GetResult();
                    var html = writer.ToString();

                    // Return the HTML in the API response
                    return Ok(new { html });
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult NewBloom([FromBody] Bloom bloom, [FromQuery] int? communityId) // poti adauga un bloom specific pt o comunitate
        {
            var user = db.ApplicationUsers.Find(_userManager.GetUserId(User));

            // Create a new Bloom object with the data from the request
            Bloom newBloom = new Bloom
            {
                Title = bloom.Title,
                Content = bloom.Content,
                Image = bloom.Image,
                Date = DateTime.Now,
                User = user,
                Likes = new List<Like>(),
                Comments = new List<Comment>(),
                TotalLikes = 0,
                UserId = user.Id,
                BloomBoards = new List<BloomBoard>(),
                CommunityId = communityId,
            };

            db.Blooms.Add(newBloom);
            db.SaveChanges();

            return Ok(new { id = newBloom.Id });
        }

      
    }

    
}
