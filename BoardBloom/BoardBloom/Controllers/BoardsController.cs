using BoardBloom.Data;
using BoardBloom.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BoardBloom.Controllers
{
	[Authorize]
	public class BoardsController : Controller
	{
		private readonly ApplicationDbContext db;

		private readonly UserManager<ApplicationUser> _userManager;

		private readonly RoleManager<IdentityRole> _roleManager;

		public BoardsController(
			ApplicationDbContext context,
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager
			)
		{
			db = context;

			_userManager = userManager;

			_roleManager = roleManager;
		}


		// toti utilizatorii pot vedea Bloom-urile existente in platforma
		// fiecare utilizator vede bloom-urile pe care le-a creat

		[Authorize(Roles = "User,Admin")]
		public IActionResult Index(string userId)
		{


			if (userId == null)
			{
				userId = _userManager.GetUserId(User);
			}
			ViewBag.CurrentUserId = userId;

			var UserName = (from usr in db.Users
							where usr.Id == userId
							select usr.UserName).FirstOrDefault();

			ViewBag.UserName = UserName;

			var userPP = (from usr in db.Users
						  where usr.Id == userId
						  select usr.ProfilePicture).FirstOrDefault();

			ViewBag.UserProfilePicture = userPP;


			if (TempData.ContainsKey("message"))
			{
				ViewBag.Message = TempData["message"];
				ViewBag.Alert = TempData["messageType"];
			}

			SetAccessRights();
			var boards = db.Boards
				.Include(b => b.User)  // Include the User associated with the Board
				.Include(b => b.BloomBoards)  // Include the join table entries
					.ThenInclude(bb => bb.Bloom)  // Include the Blooms associated via the join table
				.Where(b => b.UserId == userId)  // Filter by User ID
				.ToList();

			ViewBag.Boards = boards;
			ViewBag.IsBloomPreviewable = false;

			return View();

			/*
			else
			{
				TempData["message"] = "Nu aveti drepturi asupra bloomi";
				TempData["messageType"] = "alert-danger";
				return RedirectToAction("Index", "Blooms");
			}
			*/

		}

		// Afisarea tuturor bloomurilor pe care utilizatorul le-a salvat
		// in categorii

		[Authorize(Roles = "User,Admin")]
		public IActionResult Show(int id)
		{

			if (TempData.ContainsKey("message"))
			{
				ViewBag.Message = TempData["message"];
				ViewBag.Alert = TempData["messageType"];
			}

			SetAccessRights();

			var blooms = db.BloomBoards
					.Where(bb => bb.BoardId == id)
					.Include(bb => bb.Bloom)
					.ThenInclude(b => b.Comments)
					.Include(bb => bb.Board)
					.Select(bb => bb.Bloom)
					.ToList(); 
							
			ViewBag.Blooms = blooms;

			var board = db.Boards
					.Where(b => b.Id == id)
					.Include(b => b.BloomBoards)
					.ThenInclude(bb => bb.Bloom)
					.FirstOrDefault();
					
			ViewBag.IsBloomPreviewable = true;
			return View(board);
		}


		[Authorize(Roles = "User,Admin")]
		public IActionResult New()
		{
			return View();
		}

		[HttpPost]
		[Authorize(Roles = "User,Admin")]
		public ActionResult New(Board cat)
		{
			cat.UserId = _userManager.GetUserId(User);

			if (ModelState.IsValid)
			{
				db.Boards.Add(cat);
				db.SaveChanges();
				TempData["message"] = "Categoia a fost adaugata";
				TempData["messageType"] = "alert-success";
				return RedirectToAction("Index");
			}

			else
			{
				return View(cat);
			}
		}

		[Authorize(Roles = "User,Admin")]
		public IActionResult Edit(int id)
		{
			Board categ = db.Boards.Where(cat => cat.Id == id)
										.First();

			if (categ.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
			{
				return View(categ);
			}
			else
			{
				TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unei categori care nu va apartine";
				TempData["messageType"] = "alert-danger";
				return RedirectToAction("Index");
			}
		}


		[HttpPost]
		[Authorize(Roles = "User,Admin")]
		public IActionResult Edit(int id, Board requestBoard)
		{

			Board categ = db.Boards.Find(id);

			try
			{
				categ.Name = requestBoard.Name;

				categ.Note = requestBoard.Note;

				categ.BloomBoards = categ.BloomBoards;

				db.SaveChanges();

				return RedirectToAction("Show", new { categ.Id });
			}
			catch (Exception)
			{
				return RedirectToAction("Edit", categ.Id);
			}

		}


		public ActionResult Delete(int id)
		{


			var bloomBoards = db.BloomBoards
									.Where(cat => cat.Id == id);

			var categ = db.Boards.Include(c => c.BloomBoards)
									  .ThenInclude(bc => bc.Bloom)
									  .FirstOrDefault(c => c.Id == id);

			if (categ.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
			{
				if (categ != null)
				{
					foreach (var bloomBoard in categ.BloomBoards)
					{
						db.BloomBoards.Remove(bloomBoard);
					}

					db.Boards.Remove(categ);

					db.SaveChanges();
					TempData["message"] = "Bloomul a fost stersa";
					TempData["messageType"] = "alert-success";
					return RedirectToAction("Index", new { categ.UserId });
				}
				return RedirectToAction("Index", new { categ.UserId });
			}

			else
			{
				TempData["message"] = "Nu aveti dreptul sa stergeti un bloom care nu va apartine";
				TempData["messageType"] = "alert-danger";
				return RedirectToAction("Index", new { categ.UserId });
			}
		}

		[Authorize(Roles = "User,Admin")]
		[HttpPost]
		public IActionResult RemoveBloomFromBoard( int id, [FromForm] int bloomId)
		{
			var bloomBoard = db.BloomBoards
								.Where(bb => bb.BoardId == id && bb.BloomId == bloomId)
								.FirstOrDefault();

			var board = db.Boards
						.Where(b => b.Id == id)
						.Include(b => b.BloomBoards)
						.ThenInclude(bb => bb.Bloom)
						.FirstOrDefault();

			if (board.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
			{
				db.BloomBoards.Remove(bloomBoard);
				db.SaveChanges();
				TempData["message"] = "Bloomul a fost sters din categoria";
				TempData["messageType"] = "alert-success";
				return RedirectToAction("Show", new { id });
			}
			else
			{
				TempData["message"] = "Nu aveti dreptul sa stergeti un bloom care nu va apartine";
				TempData["messageType"] = "alert-danger";
				return RedirectToAction("Show", new { id });
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
	}
}
