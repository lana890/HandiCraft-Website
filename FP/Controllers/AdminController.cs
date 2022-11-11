using FP.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FP.Controllers
{

    public class AdminController : Controller
    {
        private readonly ModelContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public AdminController(ModelContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {

            ViewBag.customers = _context.Logins.Count(x => x.RoleId == 2);
            ViewBag.vendors = _context.Logins.Count(x => x.RoleId == 3);
            var prod = _context.Orders.ToList();
            var sum = 0;
            foreach(var i in prod)
            {
                sum += (int)i.Amount;

            }
            ViewBag.products = sum;
            return View();
        }

        public async Task<IActionResult> Vendors()
        {
            var allvendors = await _context.Logins.Where(x => x.RoleId == 3).ToListAsync();
            var allUsers = await _context.Users.ToListAsync();
            List<User> pendings = new List<User>();
           
            foreach (var i in allUsers)
            {
                foreach (var j in allvendors)
                {
                    if (i.Id == j.UserId && i.Status == "pending")
                        pendings.Add(i);
                  
                }

            }
            return View(pendings);
        }
        [HttpGet]

        public async Task<IActionResult> Massage(decimal? id)
        {
          

            var vendor = await _context.Users.FindAsync(id);
            var vendors =  _context.Logins.Where(x => x.UserId == id).SingleOrDefault();

            SmtpClient client = new SmtpClient("smtp-mail.outlook.com");
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            System.Net.NetworkCredential credential = new System.Net.NetworkCredential("lana_abubaker890@outlook.com", "227LAna@");

            client.EnableSsl = true;
            client.Credentials = credential;
            MailMessage message = new MailMessage("lana_abubaker890@outlook.com", vendor.Email);
            message.Subject = "Vendor Verification";
            message.Body = "<div><h3>Your account has been verified</h3></div><div style='color:green'>user Name: " + vendors.Username+" </div>"+ "<div style='color:red'>Password: " + vendors.Password + " </div>";
            message.IsBodyHtml = true;
            client.Send(message);
            vendor.Status = "accept";
            _context.Users.Update(vendor);
            await _context.SaveChangesAsync();


            return RedirectToAction("Vendors");
        }
       

       public async Task<IActionResult> Users()
        {
            var users = _context.Logins.Where(x => x.RoleId == 2).Include(x => x.User).ToList();
            return View(users);
        }

        public async Task<IActionResult> DisplayVendor()
        {
            var vendors = _context.Logins.Where(x => x.RoleId == 3).Include(x => x.User).ToList();
            return View(vendors);
        }

        public async Task<IActionResult> Orders()
        {
            var orders = _context.Orders.Include(x => x.Product).Include(x=>x.User).ToList();
            return View(orders);
        }

        public async Task<IActionResult> Sales()
        {
            var sales =await _context.Orders.Include(x => x.Product).Include(x => x.User).ToListAsync();
            ViewBag.TotalQuantity = sales.Sum(x => x.Amount);
            ViewBag.Totalprices = sales.Sum(x => x.Paid);
            return View(sales);
        }

        [HttpPost]
        public async Task<IActionResult> Sales(DateTime? startDate, DateTime? endDate, string name)
        {
            if (name != null)
            {
                var res=_context.Products.Where(x => x.Name.Contains(name)).ToListAsync();
            }
            var result = _context.Orders.Include(p => p.User).Include(p => p.Product);
            if (startDate == null && endDate == null)
            {
                ViewBag.TotalQuantity = result.Sum(x => x.Amount);
                ViewBag.Totalprices = result.Sum(x => x.Paid);


                return View(await result.ToListAsync());
            }
            else if (startDate == null && endDate != null)
            {
                var res = await result.Where(x => x.Datee.Value.Date == endDate).Include(x=>x.Product).Include(x=>x.User).ToListAsync();
                ViewBag.TotalQuantity = res.Sum(x => x.Amount);
                ViewBag.Totalprices = res.Sum(x => x.Paid);

                return View(res);
            }
            else if (startDate != null && endDate == null)
            {
                var res = await result.Where(x => x.Datee.Value.Date == startDate).Include(x => x.Product).Include(x => x.User).ToListAsync();
                ViewBag.TotalQuantity =res.Sum(x => x.Amount);
                ViewBag.Totalprices = res.Sum(x => x.Paid);

                return View(res);
            }
            else
            {
                var res = await result.Where(x => x.Datee.Value.Date >= startDate && x.Datee.Value.Date <= endDate).Include(x => x.Product).Include(x => x.User).ToListAsync();
                ViewBag.TotalQuantity = result.Where(x => x.Datee.Value.Date >= startDate && x.Datee.Value.Date <= endDate).Sum(x => x.Amount);
                ViewBag.Totalprices = res.Sum(x => x.Paid);

                return View(res);
            }
        }
        public async Task<IActionResult> Logout()
        {
            
            HttpContext.Session.Remove("AdminId");
            return RedirectToAction("Index", "Logins");
        }

        public async Task<IActionResult> EditProfile()
        {
            var id = HttpContext.Session.GetInt32("AdminId");
            var user = _context.Logins.Where(x => x.UserId == id).Include(x => x.User).SingleOrDefault();
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(string Fname, string Lname, String Email, String PhoneNumber, string UserName, string Password, decimal Age, string og)
        {
            var log = _context.Logins.Where(x => x.UserId == HttpContext.Session.GetInt32("AdminId")).SingleOrDefault();
            log.Password = Password;

            var nameAlreadyExists = _context.Logins.Where(x => x.Username == UserName).SingleOrDefault();

            if (UserName != og)
            {
                if (nameAlreadyExists != null)
                {
                    //adding error message to ModelState
                    ModelState.AddModelError("username", "User Name Already Exists.");
                    var use = _context.Logins.Where(x => x.UserId == HttpContext.Session.GetInt32("AdminId")).Include(x => x.User).SingleOrDefault();

                    return View(use);
                }
                else log.Username = UserName;
            }
            else log.Username = og;

            _context.Logins.Update(log);
            await _context.SaveChangesAsync();

            var user = _context.Users.Where(x => x.Id == HttpContext.Session.GetInt32("AdminId")).SingleOrDefault();
            user.Fname = Fname;
            user.Lname = Lname;
            user.Age = Age;
            user.Email = Email;
            user.Phonenumber = PhoneNumber;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", "Admin");



        }

        [HttpPost]
        public async Task<IActionResult> UpdatePic([Bind("ImageFile,Fname,Lname,Phonenumber,Age,Status,Email")] User user)
        {
            user.Id = (int)HttpContext.Session.GetInt32("AdminId");
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

            return RedirectToAction("Profile", "Admin");
        }

        public IActionResult Profile()
        {
            var id = HttpContext.Session.GetInt32("AdminId");
            var user = _context.Logins.Where(x => x.UserId == id).Include(x => x.User).SingleOrDefault();
            return View(user);
        }

        


    }
}