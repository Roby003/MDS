﻿using BoardBloom.Data;
using BoardBloom.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;


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
			
			// Get the user's profile picture
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
		}

		// Afisarea tuturor bloomurilor pe care utilizatorul le-a salvat
		// in board-uri

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
		public ActionResult New(Board board)
		{
			board.UserId = _userManager.GetUserId(User);

			if (ModelState.IsValid)
			{
				db.Boards.Add(board);
				db.SaveChanges();

				TempData["message"] = "Board created successfully";
				TempData["messageType"] = "alert-success";

				// Redirect to Show action with the new board's ID
				return RedirectToAction("Show", new { id = board.Id });
			}
			else
			{
				return View(board);
			}
		}

		[Authorize(Roles = "User,Admin")]
		// admin ar trb scos
		public IActionResult Edit(int id)
		{
			Board categ = db.Boards
				.Where(c => c.Id == id)
				.FirstOrDefault();

			// If the current user can edit the category
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

			// If the current user can delete the category
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

			// If the current user can delete the category
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

		[Authorize(Roles = "User,Admin")]
		[HttpPost]
		public IActionResult AddBloomToBoards(int id, [FromQuery] string boardsIds)
		{
			var bloom = db.Blooms
						.Where(b => b.Id == id)
						.Include(b => b.BloomBoards)
						.ThenInclude(bb => bb.Board)
						.FirstOrDefault();

			// iterate through the boards and add the bloom to them
			if (String.Empty == boardsIds || boardsIds ==null)
				return NotFound();
            foreach (string boardId in boardsIds.Split(","))
			{
				var board = db.Boards
							.Where(b => b.Id == int.Parse(boardId))
							.Include(b => b.BloomBoards)
							.ThenInclude(bb => bb.Bloom)
							.FirstOrDefault();

				if (board.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
				{
					if(db.BloomBoards.Any(bb => bb.BoardId == board.Id && bb.BloomId == bloom.Id))
					{
						db.BloomBoards.Remove(db.BloomBoards.FirstOrDefault(bb => bb.BoardId == board.Id && bb.BloomId == bloom.Id));
						db.SaveChanges();
					} else 
					{
						var bloomBoard = new BloomBoard
						{
							BloomId = bloom.Id,
							BoardId = board.Id,
							BoardDate = System.DateTime.Now
						};

						if(!db.BloomBoards.Any(bb => bb.BloomId == bloom.Id && bb.BoardId == board.Id))
						{
							db.BloomBoards.Add(bloomBoard);
							db.SaveChanges();
						}
					}
				}
				else
				{
					TempData["message"] = "Nu aveti dreptul sa adaugati un bloom in categoria care nu va apartine";
					TempData["messageType"] = "alert-danger";
					return StatusCode(403);
				}
			}

			return Ok();
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
		[Authorize(Roles = "User,Admin")]
		public IActionResult GetAllUserBoards([FromQuery] string userId, [FromQuery] string bloomId)
		{
			var boards = db.Boards
				.Where(b => b.UserId == userId)
				.Include(b => b.User)
				.ToList();

			// return the boards and the number of blooms in each board
			var json = Json(boards.Select(board => new
			{
				board,
				bloomsCount = db.BloomBoards.Where(bb => bb.BoardId == board.Id).Count(),
				saved = db.BloomBoards.Any(bb => bb.BoardId == board.Id && bb.BloomId == int.Parse(bloomId))
			}));

			return json;
		}
	}
}