using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FP.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace FP.Controllers
{
    public class LoginsController : Controller
    {
        private readonly ModelContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public LoginsController(ModelContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Logins
        public IActionResult Index()
        {
            
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index([Bind("Username,Password")] Login user)
        {
            var userr = _context.Logins.Where(x => x.Username == user.Username && x.Password == user.Password).SingleOrDefault();
            
            if (userr != null)
            {
                var vendor = _context.Users.Where(x => x.Id == userr.Id).SingleOrDefault();

                switch (userr.RoleId)
                {
                    case 1: // Admin
                        HttpContext.Session.SetInt32("AdminId", (int)userr.UserId);
                        return RedirectToAction("Index", "Admin");
                    case 2: //Customer

                        HttpContext.Session.SetInt32("UserId", (int)userr.UserId);
                        return RedirectToAction("Index", "User");


                    case 3: //vendor
                        if (vendor.Status == "pending")
                        {
                            ModelState.AddModelError("status", "this user hasn't been confirmed yet");
                            return View();
                        }
                        else
                        {
                            HttpContext.Session.SetInt32("VendorId", (int)userr.UserId);
                            return RedirectToAction("Index", "Vendor");
                        }
                }
            }
            else
            
                ModelState.AddModelError("error", "incorrect username or password");
                return View();
            
        }
        public IActionResult SignUp()
        {
           
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp([Bind("Id,Fname,Lname,Age,ImagePath,Phonenumber,Email,ImageFile,Status")] User customer, string? username, string? password, string isVendor)
        {
            // customer => fname , lname , imagefile
            var error = ModelState.Values.SelectMany(x => x.Errors);

            if (ModelState.IsValid)
            {
                var nameAlreadyExists = _context.Logins.Where(x => x.Username == username).SingleOrDefault();

                if (nameAlreadyExists != null)
                {
                    //adding error message to ModelState
                    ModelState.AddModelError("username", "User Name Already Exists.");

                    return View(customer);
                }
                else if (customer.ImageFile != null)
                {
                    string wwwrootPath = _webHostEnvironment.WebRootPath; // wwwrootpath
                    string imageName = Guid.NewGuid().ToString() + "_" + customer.ImageFile.FileName; // image name
                    string path = Path.Combine(wwwrootPath + "/img/", imageName); // wwwroot/Image/imagename

                    using (var filestream = new FileStream(path, FileMode.Create))
                    {
                        await customer.ImageFile.CopyToAsync(filestream);
                    }
                    customer.ImagePath = imageName;
                    Login user = new Login();
                    user.Username = username;
                    user.Password = password;
                    if (isVendor == "true")
                    {

                        user.RoleId = 3;
                        customer.Status = "pending";
                    }
                    else
                    {
                        user.RoleId = 2;
                        customer.Status = "accept";
                    }

                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    user.UserId = customer.Id;

                    _context.Add(user);
                    await _context.SaveChangesAsync();


                }

                return RedirectToAction("Index");
            }


        
           

            return View(customer);
        }

    
    

        // GET: Logins/Details/5
        public async Task<IActionResult> Details(decimal? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var login = await _context.Logins
                .Include(l => l.Role)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (login == null)
            {
                return NotFound();
            }

            return View(login);
        }

        // GET: Logins/Create
        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Logins/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,Password,RoleId,UserId")] Login login)
        {
            if (ModelState.IsValid)
            {
                _context.Add(login);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Id", login.RoleId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", login.UserId);
            return View(login);
        }

        // GET: Logins/Edit/5
        public async Task<IActionResult> Edit(decimal? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var login = await _context.Logins.FindAsync(id);
            if (login == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Id", login.RoleId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", login.UserId);
            return View(login);
        }

        // POST: Logins/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(decimal id, [Bind("Id,Username,Password,RoleId,UserId")] Login login)
        {
            if (id != login.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(login);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoginExists(login.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Id", login.RoleId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", login.UserId);
            return View(login);
        }

        // GET: Logins/Delete/5
        public async Task<IActionResult> Delete(decimal? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var login = await _context.Logins
                .Include(l => l.Role)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (login == null)
            {
                return NotFound();
            }

            return View(login);
        }

        // POST: Logins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(decimal id)
        {
            var login = await _context.Logins.FindAsync(id);
            _context.Logins.Remove(login);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LoginExists(decimal id)
        {
            return _context.Logins.Any(e => e.Id == id);
        }
    }
}
