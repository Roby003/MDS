using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BoardBloom.Data;
using BoardBloom.Models;
using System;
using System.Threading.Tasks;


namespace BoardBloom.Controllers
{
    public class BloomsController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private IWebHostEnvironment _env;

        public BloomsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;

            _env = env;
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

            ViewBag.UserBoards = db.Boards
                                      .Where(c => c.UserId == _userManager.GetUserId(User))
                                      .ToList();

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

            return View(bloom);
        }


        // Adaugarea unui comentariu 
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Blooms/Show/" + comment.BloomId);
            }

            else
            {
                /*//aici prob bug
                Bloom bloom = db.Blooms.Include("User")
                                         .Include("Comments")
                                         //.Include("Likes")
                                         .Include("Comments.User")
                                         .Where(bloom => bloom.Id == comment.BloomId)
                                         .First();


                // Adaugam bloom-urile utilizatorului pentru dropdown
                ViewBag.Boards = db.Boards
                                          .Where(c => c.UserId == _userManager.GetUserId(User))
                                          .ToList();

                SetAccessRights();

                return View(bloom);*/

                //bug fix
                return Redirect("/Blooms/Show/" + comment.BloomId);

            }
        }

        [HttpPost]
        public IActionResult AddBoard([FromForm] BloomBoard bloomBoard)
        {
            // Daca modelul este valid
            if (ModelState.IsValid)
            {

                // Verificam daca avem deja bloom in board
                if (db.BloomBoards
                    .Where(bl => bl.BloomId == bloomBoard.BloomId)
                    .Where(bl => bl.BoardId == bloomBoard.BoardId)
                    .Count() > 0)
                {
                    TempData["message"] = "Acest bloom este deja adaugat in acest board";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {

                    db.BloomBoards.Add(bloomBoard);

                    db.SaveChanges();

                    // Adaugam un mesaj de succes
                    TempData["message"] = "Bloom-ul a fost adaugat in board-ul selectat";
                    TempData["messageType"] = "alert-success";
                }

            }
            else
            {
                TempData["message"] = "Nu s-a putut adauga bloom-ul in board";
                TempData["messageType"] = "alert-danger";
            }


            return Redirect("/Blooms/Show/" + bloomBoard.BloomId);
        }

        [HttpPost]
        public IActionResult RemoveFromBoard(int bloomId, int boardId)
        {
            // luam BC-ul 
            var bloomBoard = db.BloomBoards
                .FirstOrDefault(bb => bb.BloomId == bloomId && bb.BoardId == boardId);

            var board = db.Boards
                           .Where(br => br.Id == boardId)
                           .FirstOrDefault();

            if (board.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                if (bloomBoard != null)
                {
                    try
                    {

                        db.BloomBoards.Remove(bloomBoard);
                        db.SaveChanges();

                        TempData["message"] = "Bloom a fost sters din boarul selectata";
                        TempData["messageType"] = "alert-success";
                    }
                    catch (Exception ex)
                    {
                        TempData["message"] = "Nu s-a putut sterge bloomul din board. Eroare: " + ex.Message;
                        TempData["messageType"] = "alert-danger";
                    }
                }
                else
                {
                    TempData["message"] = "Bloomul nu a fost gasit in boardul selectata";
                    TempData["messageType"] = "alert-danger";
                }
            }

            TempData["message"] = "Bloomul a fost sters din boardul selectata";
            TempData["messageType"] = "alert-success";
            return Redirect("/Boards/Show/" + board.Id);
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



        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New(Bloom bloom, IFormFile? BloomImage, string? EBD)
        {
            bloom.Date = DateTime.Now;


            bloom.UserId = _userManager.GetUserId(User);
            bloom.User = db.ApplicationUsers.Find(bloom.UserId);

            if (BloomImage != null && BloomImage.Length > 0 && BloomImage is IFormFile)
            {
                var storagePath = Path.Combine(_env.WebRootPath, "images", BloomImage.FileName);
                var databaseFileName = "/images/" + BloomImage.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    BloomImage.CopyTo(fileStream);
                }

                bloom.Image = databaseFileName;
            }
            else if (EBD != null && EBD.Length > 0)
            {

                bloom.Image = EBD;
            }

            if (ModelState.IsValid)
            {
                
                db.Blooms.Add(bloom);
                db.SaveChanges();
                TempData["message"] = "Bloom-ul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                return View(bloom);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {

            Bloom bloom = db.Blooms.Where(bl => bl.Id == id)
                                        .First();

            var usr = db.ApplicationUsers.Find(_userManager.GetUserId(User));



            if (bloom.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                return View(bloom);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui bloom care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

        }



        // Verificam rolul utilizatorilor care au dreptul sa editeze
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Bloom requestBloom, IFormFile? UpdatedImage, string? EBD)
        {

            Bloom bloom = db.Blooms.Find(id);

            requestBloom.Image = bloom.Image;

            if (ModelState.IsValid)
            {
                // permisia
                if (bloom.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {

                    bloom.Title = requestBloom.Title;
                    bloom.Content = requestBloom.Content;

                    if (UpdatedImage != null && UpdatedImage.Length > 0)
                    {

                        var storagePath = Path.Combine(_env.WebRootPath, "images", UpdatedImage.FileName);
                        var databaseFileName = "/images/" + UpdatedImage.FileName;


                        using (var fileStream = new FileStream(storagePath, FileMode.Create))
                        {
                            UpdatedImage.CopyTo(fileStream);
                        }

                        bloom.Image = databaseFileName;
                    }
                    else if (EBD != null && EBD.Length > 0)
                    {

                        bloom.Image = EBD;
                    }



                    db.SaveChanges();

                    TempData["message"] = "Bloom-ul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui bloom care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {

                return View(requestBloom);
            }
        }




        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public ActionResult Delete(int id)
        {
            Bloom bloom = db.Blooms.Include("Comments")
                                         .Include("Likes")
                                         .Where(bl => bl.Id == id)
                                         .First();

            if (bloom.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Blooms.Remove(bloom);
                db.Likes.RemoveRange(bloom.Likes);
                db.SaveChanges();
                TempData["message"] = "Bloom-ul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un bloom care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
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


        [NonAction]
        public IEnumerable<SelectListItem> GetAllBoards()
        {
            var selectList = new List<SelectListItem>();

            string userId = _userManager.GetUserId(User);

            var boards = db.Boards
                            .Where(br => br.UserId == userId);
            // .ToList();


            foreach (var board in boards)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul boardului si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = board.Id.ToString(),
                    Text = board.Name.ToString()
                });
            }

            return selectList;
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

        [HttpPost]
        public IActionResult Unlike(int bloomId)
        {

            var userId = _userManager.GetUserId(User);

            var like = db.Likes.FirstOrDefault(l => l.BloomId == bloomId && l.UserId == userId);

            if (like != null)
            {
                var bloom = db.Blooms.Find(bloomId);

                bloom.TotalLikes--;

                db.Likes.Remove(like);
                db.SaveChanges();

                ViewData["UserLikes"] = GetCurrentUserLikes();
            }


            return RedirectToAction("Index", new { id = bloomId });
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllLikes()
        {

            var selectList = new List<SelectListItem>();

            var likes = from lk in db.Likes
                            //where lk.BloomId = bloomId
                        select lk;

            foreach (var like in likes)
            {

                selectList.Add(new SelectListItem
                {
                    Value = like.Id.ToString(),
                    Text = like.BloomId.ToString()
                });
            }
            return selectList;
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
    }
}
