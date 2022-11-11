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
    public class VendorController : Controller
    {
        private readonly ModelContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VendorController(ModelContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var id = HttpContext.Session.GetInt32("VendorId");
            var myproducts =await _context.Products.Where(x => x.VendorId == id).ToListAsync();
            return View(myproducts);
        }

        public async Task<IActionResult> View(decimal id)
        {
            var product =await _context.Orders.Where(x => x.ProductId == id).Include(x => x.Product).Include(x=>x.User).ToListAsync();
            return View(product);
        }

        public async Task<IActionResult> SoldProduct()
        {
            var SOLD = await _context.Orders.Where(x => x.VendorId == HttpContext.Session.GetInt32("VendorId")).Include(x => x.Product).Include(x=>x.User).ToListAsync();
            return View(SOLD);
        }
        public async Task<IActionResult> Feedback()
        {
           

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddFeedback(String Body)
        {

            Feedback fb = new Feedback();
            fb.Body = Body;
            fb.UserId = HttpContext.Session.GetInt32("VendorId");
            _context.Feedbacks.Add(fb);
            await _context.SaveChangesAsync();
            return View();
        }
        public async Task<IActionResult> ViewFeedback()
        {
          
            var feedBacks = await _context.Feedbacks.Include(x => x.User).ToListAsync();
            return View(feedBacks);


        }
        public async Task<IActionResult> Logout()
        {

            HttpContext.Session.Remove("AdminId");
            return RedirectToAction("Index", "Logins");
        }

        public IActionResult Profile()
        {
            var id = HttpContext.Session.GetInt32("VendorId");
            var user = _context.Logins.Where(x => x.UserId == id).Include(x => x.User).SingleOrDefault();
            return View(user);
        }

        public async Task<IActionResult> About()
        {

            var about = _context.About.Where(x => x.Id == 1).SingleOrDefault();
            return View(about);
        }


        public async Task<IActionResult> EditProfile()
        {
            var id = HttpContext.Session.GetInt32("VendorId");
            var user = _context.Logins.Where(x => x.UserId == id).Include(x => x.User).SingleOrDefault();
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(string Fname, string Lname, String Email, String PhoneNumber, string UserName, string Password, decimal Age,string og)
        {
            var log = _context.Logins.Where(x => x.UserId == HttpContext.Session.GetInt32("VendorId")).SingleOrDefault();
            log.Password = Password;

            var nameAlreadyExists = _context.Logins.Where(x => x.Username == UserName).SingleOrDefault();
            if (UserName != og)
            {
                if (nameAlreadyExists != null)
                {
                    //adding error message to ModelState
                    ModelState.AddModelError("username", "User Name Already Exists.");
                    var use = _context.Logins.Where(x => x.UserId == HttpContext.Session.GetInt32("VendorId")).Include(x => x.User).SingleOrDefault();

                    return View(use);
                }
                else log.Username = UserName;
            }
            else log.Username = og;
           
            _context.Logins.Update(log);
            await _context.SaveChangesAsync();

            var user = _context.Users.Where(x => x.Id == HttpContext.Session.GetInt32("VendorId")).SingleOrDefault();
            user.Fname = Fname;
            user.Lname = Lname;
            user.Age = Age;
            user.Email = Email;
            user.Phonenumber = PhoneNumber;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", "Vendor");



        }

        [HttpPost]
        public async Task<IActionResult> UpdatePic([Bind("ImageFile,Fname,Lname,Phonenumber,Age,Status,Email")] User user)
        {
            user.Id = (int)HttpContext.Session.GetInt32("VendorId");
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

            return RedirectToAction("Profile", "Vendor");
        }
    }
}
