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

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }



            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedBlooms = searchedBlooms.Skip(offset).Take(_perPage);

            int totalItems = searchedBlooms.Count();

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);


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

            var isLiked = db.Likes
                .Where(l => l.BloomId == id)
                .Where(l => l.UserId == _userManager.GetUserId(User))
                .Count() > 0;

            ViewBag.IsLiked = isLiked;

            SetAccessRights();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }


            ViewBag.isBloomEditable = true;
            return View(bloom);
        }

        // Se afiseaza formularul in care se vor completa datele unui bloom

        [Authorize(Roles = "User,Admin")]
        public IActionResult New(/*bool? url_link*/)
        {
            //ViewBag.Url_Link = true;



            //if (url_link == false) ViewBag.Url_Link = false;

            Bloom bloom = new Bloom();

            return View(bloom);
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Bloom bloom = db.Blooms.Find(id);

            if (bloom.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(bloom);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati un bloom care nu va apartine";
                TempData["messageType"] = "alert-danger";

                return RedirectToAction("Index", "Home");
            }
        }   


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(Bloom bloom)
        {
            if (ModelState.IsValid)
            {
                var user = db.ApplicationUsers.Find(_userManager.GetUserId(User));

                var oldBloom = db.Blooms.Find(bloom.Id);
                oldBloom.Title = bloom.Title;
                oldBloom.Content = bloom.Content;
                oldBloom.Image = bloom.Image;

                db.SaveChanges();

                return RedirectToAction("Show", new { id = bloom.Id });
            }
            else
            {
                // If ModelState is not valid, return BadRequest with ModelState errors
                return BadRequest(ModelState);
            }
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

                TempData["message"] = "Bloom-ul a fost sters";
                TempData["messageType"] = "alert-success";

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un bloom care nu va apartine";
                TempData["messageType"] = "alert-danger";

                return RedirectToAction("Index", "Home");
            }
        }


        // Conditiile de afisare a butoanelor 
        private void SetAccessRights()
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
        public IActionResult Like(int bloomId)
        {

            string userId = _userManager.GetUserId(User);


            if (!db.Likes.Any(l => l.BloomId == bloomId && l.UserId == userId))
            {
                // Create a new like
                var like = new Like
                {
                    BloomId = bloomId,
                    UserId = userId
                };
                var bloom = db.Blooms.Find(bloomId);

                bloom.TotalLikes++;

                db.Likes.Add(like);
                db.SaveChanges();

                ViewData["UserLikes"] = GetCurrentUserLikes();
            }
            else
            {
                var like = db.Likes.FirstOrDefault(l => l.BloomId == bloomId && l.UserId == userId);

                var bloom = db.Blooms.Find(bloomId);
                
                if(like != null)
                {
                    bloom.TotalLikes--;
                    db.Likes.Remove(like);
                    db.SaveChanges();
                }
            }
            
            return RedirectToAction("Show", new { id = bloomId });
        }

        [HttpGet]
        public IActionResult EditComm(int commId)
        {
            Comment comm = db.Comments.Find(commId);
            if (comm == null)
                return RedirectToAction("Index", "Blooms");


            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return PartialView("CommEditModal",comm);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati comentariul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Blooms");
            }
        }

        [HttpPost]
        public IActionResult EditComm(Comment reqComm)
        {
            Comment c = db.Comments.Find(reqComm.Id);
            if (c == null)
            return RedirectToAction("Index", "Blooms");
            if (ModelState.IsValid)
            {
             
                    c.Content = reqComm.Content;
                    TempData["message"] = "comment modified successfully";

            }
                db.SaveChanges();
                return PartialView("CommEditModal", c);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Preview([FromBody] Bloom bloom)
        {
            if (ModelState.IsValid)
            {
                var user = db.ApplicationUsers.Find(_userManager.GetUserId(User));

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
        public IActionResult NewBloom([FromBody] Bloom bloom)
        {
            var user = db.ApplicationUsers.Find(_userManager.GetUserId(User));

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
            };

            db.Blooms.Add(newBloom);
            db.SaveChanges();

            return Ok(new { id = newBloom.Id });
        }
    }
}
