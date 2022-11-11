using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FP.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using System.Net.Mime;
using IronPdf;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Data;
using System.Xml;
using iTextSharp.text.html.simpleparser;
using System.Text;
using System.Web.UI;
using Grpc.Core;

namespace FP.Controllers
{
    public class CartsController : Controller
    {
        private readonly ModelContext _context;

        public CartsController(ModelContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> AddToCart()
        {
            var id = HttpContext.Session.GetInt32("UserId");
            var carts = _context.Carts.Count(x => x.UserId == id);
            HttpContext.Session.SetInt32("cart", carts);
            ViewBag.cart = HttpContext.Session.GetInt32("cart");
          

            return View(await _context.Carts.Where(x => x.UserId == HttpContext.Session.GetInt32("UserId")).Include(x => x.Product).Include(x => x.User).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(decimal id, decimal amount)
        {
            Cart cart = new Cart();
            cart.UserId = HttpContext.Session.GetInt32("UserId");
            cart.ProductId = id;
            cart.Amount = amount;

            _context.Add(cart);
            await _context.SaveChangesAsync();
            var carts = await _context.Carts.Where(x => x.UserId == HttpContext.Session.GetInt32("UserId")).Include(x => x.Product).ToListAsync();
            HttpContext.Session.SetInt32("cart", carts.Count);
            if (carts != null)
                ViewBag.cart = HttpContext.Session.GetInt32("cart");
            else
                ViewBag.cart = 0;

            return View(carts);


        }

        public async Task<IActionResult> CheckOut()
        {
            var id = HttpContext.Session.GetInt32("UserId");
            var user = _context.Users.Where(x => x.Id == id).SingleOrDefault();
            var carts = _context.Carts.Count(x => x.UserId == id);
            HttpContext.Session.SetInt32("cart", carts);
            ViewBag.cart = HttpContext.Session.GetInt32("cart");

            return View(await _context.Carts.Where(x => x.UserId == HttpContext.Session.GetInt32("UserId")).Include(x => x.Product).ToListAsync());
        }



        public async Task<IActionResult> Done(String Country, String Street, String PostaZip, String Exp, String Cvv, String SerNumber, String Note, String sum)
        {
            Location loc = new Location();
            loc.Country = Country;
            loc.Street = Street;
            loc.PostaZip = PostaZip;
            loc.UserId = HttpContext.Session.GetInt32("UserId");
            _context.Locations.Add(loc);
            await _context.SaveChangesAsync();

            var ser = _context.Visas.Where(x => (x.SerNumber == SerNumber) && (x.Cvv == Cvv) && (x.Exp == Exp) && (x.Balance >= Convert.ToInt32(sum))).FirstOrDefault();
            if (ser != null)
            {
                ser.Balance -= Convert.ToInt32(sum);
                _context.Visas.Update(ser);
                await _context.SaveChangesAsync();

                var Adminser = _context.Visas.Where(x => x.SerNumber == "1234567890" && x.Cvv == "1234" && x.Exp == "05/22").SingleOrDefault();
                Adminser.Balance += Convert.ToInt32(sum);
                _context.Visas.Update(Adminser);
                await _context.SaveChangesAsync();

                var orderedItem = await _context.Carts.Where(x => x.UserId == HttpContext.Session.GetInt32("UserId")).Include(x => x.Product).ToListAsync();
                var product = await _context.Products.ToListAsync();
                foreach (var i in orderedItem)
                {
                    foreach (var j in product)
                    {
                        if (i.Product.Id == j.Id)
                        {
                            j.Amount -= i.Amount;
                            _context.Products.Update(j);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                var massege = "<center><p> Thank for Ordering </p></center>" + ",";
                massege = "Your Order Is" + ',';

                foreach (var i in orderedItem)
                {

                    Order order = new Order();
                    order.Datee = DateTime.Now;
                    order.Paid = Convert.ToInt32(sum);
                    order.Note = Note;
                    order.LocationId = loc.Id;
                    order.UserId = HttpContext.Session.GetInt32("UserId");
                    order.Amount = i.Amount;
                    order.ProductId = i.ProductId;
                    order.VendorId = i.Product.VendorId;
                    massege += i.Product.Name + " X" + i.Amount + ',';

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                }
                massege += "<div>Your Payment is: " + sum +"</div>"+ ',';
                massege += "Thanks for ordering";
                Invoice inv = new Invoice();
                inv.UserId = HttpContext.Session.GetInt32("UserId");
                inv.Massege = massege;
                inv.Datee = DateTime.Now;

                _context.Invoices.Add(inv);
                await _context.SaveChangesAsync();

                SmtpClient client = new SmtpClient("smtp-mail.outlook.com");
                client.Port = 587;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                System.Net.NetworkCredential credential = new System.Net.NetworkCredential("lana_abubaker890@outlook.com", "227LAna@");

                var user = _context.Users.Where(x => x.Id == HttpContext.Session.GetInt32("UserId")).SingleOrDefault();
                client.EnableSsl = true;
                client.Credentials = credential;
                client.Credentials = credential;


                MailMessage message = new MailMessage("lana_abubaker890@outlook.com", user.Email);
                message.Subject = "HandiCraft Invoice";
                var ms = massege.Split(',');


                using (StringWriter sw = new StringWriter())
                {
                    using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                    {

                        StringBuilder sb = new StringBuilder();
                        sb.Append("<table border='2'>");

                        foreach (var i in ms)
                        {
                            sb.Append("<tr><td>");
                            sb.Append("<Strong>");
                            sb.Append(i);
                            sb.Append("</Strong>");
                            sb.Append("</td></tr>");


                        }
                        sb.Append("</table>");



                        StringReader sr = new StringReader(sb.ToString());


                        Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                        HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                            pdfDoc.Open();
                            htmlparser.Parse(sr);
                            pdfDoc.Close();
                            byte[] bytes = memoryStream.ToArray();
                            memoryStream.Close();


                            message.Subject = "Invoice PDF";
                            message.Body = "Your Invoice";

                            message.IsBodyHtml = true;
                           
                            message.Attachments.Add(new Attachment(new MemoryStream(bytes),
                           "Invoice.pdf"));



                            client.Send(message);





                        }
                    }
                }

                var cart = _context.Carts.Where(x => x.UserId == HttpContext.Session.GetInt32("UserId")).ToList();
                foreach (var i in cart)
                {
                    _context.Carts.Remove(i);
                    await _context.SaveChangesAsync();
                }
                return View();

        }
            else return View();
        }

        // GET: Carts
        public async Task<IActionResult> Index()
        {
            var modelContext = _context.Carts.Include(c => c.Product).Include(c => c.User);
            return View(await modelContext.ToListAsync());
        }

        // GET: Carts/Details/5
        public async Task<IActionResult> Details(decimal? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Product)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // GET: Carts/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Carts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,UserId")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", cart.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", cart.UserId);
            return View(cart);
        }

        // GET: Carts/Edit/5
        public async Task<IActionResult> Edit(decimal? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", cart.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", cart.UserId);
            return View(cart);
        }

        // POST: Carts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(decimal id, [Bind("Id,ProductId,UserId")] Cart cart)
        {
            if (id != cart.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.Id))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Id", cart.ProductId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", cart.UserId);
            return View(cart);
        }

        // GET: Carts/Delete/5
        public async Task<IActionResult> Delete(decimal? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Product)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Carts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(decimal id)
        {
            var cart = await _context.Carts.FindAsync(id);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index),"User");
        }

        private bool CartExists(decimal id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }
    }
}
