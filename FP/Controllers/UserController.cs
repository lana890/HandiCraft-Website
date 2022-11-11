using FP.Models;
using FP.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FP.Controllers
{
    public class UserController : Controller
    {
        private readonly ModelContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserController(ModelContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var id = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.Where(x => x.Id == id).SingleOrDefault();
            var carts = _context.Carts.Count(x => x.UserId == id);
            HttpContext.Session.SetInt32("cart", carts);
            ViewBag.cart = HttpContext.Session.GetInt32("cart");
            return View(user);
        }

        public IActionResult Profile()
        {
            ViewBag.cart = HttpContext.Session.GetInt32("cart");
            var id = HttpContext.Session.GetInt32("UserId");
            var user = _context.Logins.Where(x => x.UserId == id).Include(x=>x.User).SingleOrDefault();
            return View(user);
        }

        public async Task<IActionResult> Search(String search)
        {
            ViewBag.cart = HttpContext.Session.GetInt32("cart");

            if (search != null)
            {
                var items = _context.Products.Where(x => x.Name.Contains(search)).ToList();
                return View(items);
            }
            else
            {
                var items = _context.Products.ToList();
                return View(items);
            }
        }

        public async Task<IActionResult> Feedback()
        {
            ViewBag.cart = HttpContext.Session.GetInt32("cart");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddFeedback(String Body)
        {
            ViewBag.cart = HttpContext.Session.GetInt32("cart");

            Feedback fb = new Feedback();
            fb.Body = Body;
            fb.UserId = HttpContext.Session.GetInt32("UserId");
            _context.Feedbacks.Add(fb);
            await _context.SaveChangesAsync();
            return View();
        }
        public async Task<IActionResult> ViewFeedback()
        {
            ViewBag.cart = HttpContext.Session.GetInt32("cart");
            var feedBacks =await _context.Feedbacks.Include(x => x.User).ToListAsync();
            return View(feedBacks);


        }

        public async Task<IActionResult> EditProfile()
        {
            ViewBag.cart = HttpContext.Session.GetInt32("cart");
            var id = HttpContext.Session.GetInt32("UserId");
            var user = _context.Logins.Where(x => x.UserId == id).Include(x => x.User).SingleOrDefault();
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(string Fname, string Lname, String Email, String PhoneNumber, string UserName, string Password, decimal Age,string og)
        {
            var log = _context.Logins.Where(x => x.UserId == HttpContext.Session.GetInt32("UserId")).SingleOrDefault();
            log.Password = Password;

            var nameAlreadyExists = _context.Logins.Where(x => x.Username == UserName).SingleOrDefault();

            if (UserName != og)
            {
                if (nameAlreadyExists != null)
                {
                    //adding error message to ModelState
                    ModelState.AddModelError("username", "User Name Already Exists.");
                    var use = _context.Logins.Where(x => x.UserId == HttpContext.Session.GetInt32("UserId")).Include(x => x.User).SingleOrDefault();

                    return View(use);
                }
                else log.Username = UserName;
            }
            else log.Username = og;
           
            _context.Logins.Update(log);
           await  _context.SaveChangesAsync();

            var user = _context.Users.Where(x => x.Id == HttpContext.Session.GetInt32("UserId")).SingleOrDefault();
            user.Fname = Fname;
            user.Lname = Lname;
            user.Age = Age;
            user.Email = Email;
            user.Phonenumber = PhoneNumber;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile","User");



        }
        
        [HttpPost] 
        public async Task<IActionResult> UpdatePic([Bind("ImageFile,Fname,Lname,Phonenumber,Age,Status,Email")]User user )
        {
            user.Id =(int) HttpContext.Session.GetInt32("UserId");
            string wwwrootPath = _webHostEnvironment.WebRootPath; // wwwrootpath
            string imageName = Guid.NewGuid().ToString() + "_" + user.ImageFile.FileName; // image name
            string path = Path.Combine(wwwrootPath + "/img/", imageName); // wwwroot/Image/imagename

            using (var filestream = new FileStream(path, FileMode.Create))
            {
                await user.ImageFile.CopyToAsync(filestream);
            }
            user.ImagePath = imageName;
           
            _context.Users.Update(user);
           await _context.SaveChangesAsync();

            return RedirectToAction("Profile", "User");
        }
        public async Task<IActionResult> Logout()
        {
            var id = HttpContext.Session.GetInt32("UserId");
            var cart = await _context.Carts.ToListAsync();
            foreach (var item in cart)
            {
                if (item.UserId == id)
                {
                    _context.Carts.Remove(item);
                    await _context.SaveChangesAsync();
                }
            }
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Index", "Logins");
        }

        public async Task<IActionResult> About()
        {
            ViewBag.cart = HttpContext.Session.GetInt32("cart");

            var about = _context.About.Where(x => x.Id == 1).SingleOrDefault();
            return View(about);
        }
    }
}
