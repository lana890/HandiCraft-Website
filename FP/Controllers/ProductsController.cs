using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace FP.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ModelContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public ProductsController(ModelContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> ViewProduct(decimal? id)
        {
            var product = _context.Products.Where(x => x.Id == id).SingleOrDefault();
            var carts = _context.Carts.Count(x => x.UserId == HttpContext.Session.GetInt32("UserId"));
            HttpContext.Session.SetInt32("cart", carts);
            ViewBag.cart = HttpContext.Session.GetInt32("cart");
            return View(product);
        }

        // GET: Products
      


        public async Task<IActionResult> EditProduct(decimal? id)
        {
            var product = _context.Products.Where(x => x.Id == id).SingleOrDefault();
            return View(product);

        }
        [HttpPost]
        public async Task<IActionResult> EditProduct(decimal id,string name,decimal amount,decimal price )
        {
            var product = _context.Products.Where(x => x.Id == id).SingleOrDefault();
            product.Name = name;
            product.Amount = amount;
            product.Price = price;
            
             _context.Products.Update(product);
            await _context.SaveChangesAsync();



            return RedirectToAction("Index","Vendor");

        }


        // GET: Products/Details/5
        public async Task<IActionResult> Details(decimal? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Cartagory)
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CartagoryId"] = new SelectList(_context.Catagories, "Id", "Id");
            ViewData["VendorId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Amount,Price,ImagePath,CartagoryId,VendorId")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CartagoryId"] = new SelectList(_context.Catagories, "Id", "Id", product.CartagoryId);
            ViewData["VendorId"] = new SelectList(_context.Users, "Id", "Id", product.VendorId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(decimal? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CartagoryId"] = new SelectList(_context.Catagories, "Id", "Id", product.CartagoryId);
            ViewData["VendorId"] = new SelectList(_context.Users, "Id", "Id", product.VendorId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(decimal id, [Bind("Id,Name,Amount,Price,ImagePath,CartagoryId,VendorId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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
            ViewData["CartagoryId"] = new SelectList(_context.Catagories, "Id", "Id", product.CartagoryId);
            ViewData["VendorId"] = new SelectList(_context.Users, "Id", "Id", product.VendorId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(decimal? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Cartagory)
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [HttpGet]

        public  IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([Bind("Id,Name,Amount,Price,VendorId,CartagoryId,ImageFile,ImagePath")] Product product)
        {

            if (ModelState.IsValid)
            {
                product.VendorId = HttpContext.Session.GetInt32("VendorId");
                if (product.ImageFile != null)
                {
                    string wwwrootPath = _webHostEnvironment.WebRootPath; // wwwrootpath
                    string imageName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName; // image name
                    string path = Path.Combine(wwwrootPath + "/User/images/", imageName); // wwwroot/Image/imagename

                    using (var filestream = new FileStream(path, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(filestream);
                    }
                    product.ImagePath = imageName;
                   
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                  

                }

                return RedirectToAction("Index","Vendor");
            }
            return View();
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(decimal id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index","Vendor");
        }

        private bool ProductExists(decimal id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
