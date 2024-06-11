using BoardBloom.Data;
using BoardBloom.Models;
using BoardBloom.Data;
using BoardBloom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardBloom.Controllers
{
    public class CommentsController : Controller
    {

        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public CommentsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

           _userManager = userManager;

            _roleManager = roleManager;
        }



        // Stergerea unui comentariu 
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
            {
                db.Comments.Remove(comm);
                db.SaveChanges();
                return Redirect("/Blooms/Show/" + comm.BloomId);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti comentariul";
                TempData["messageType"] = "alert-danger";
                return Redirect("/Blooms/Show/" + comm.BloomId);
            }
        }


        // Editare comment
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            Comment comm = db.Comments.Find(id);

            if (comm.UserId == _userManager.GetUserId(User))
            {
                return View(comm);
            }

            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati comentariul";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Blooms");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User, Admin")]
        public IActionResult Edit([FromQuery]int id, [FromForm] string content)
        {
            Comment comm =db.Comments.Include("User").Where(c => c.Id == id).First();

            if (comm.UserId == _userManager.GetUserId(User))
            {
                comm.Content = content;

                db.SaveChanges();

                return PartialView("_CommentPartial", comm);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Blooms");
            }
        }


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New(Comment comm)
        {
            comm.UserId = _userManager.GetUserId(User);
            comm.Date = System.DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Comments.Add(comm);
                db.SaveChanges();
                return Redirect("/Blooms/Show/" + comm.BloomId);
            }
            else
            {
                return Redirect("/Blooms/Show/" + comm.BloomId);
            }
        }

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public IActionResult GetComm([FromQuery]int id)
        {   
            // Get the comment
            Comment comm = db.Comments.Include("User").Where(c => c.Id == id).First();
            if (comm != null)
            {
                return PartialView("_CommentPartial", comm);
            }
            else
            {
                ViewBag.Error = "comm id is not valid";
                return RedirectToAction("Index", "Home");
            }

        }

    }
}