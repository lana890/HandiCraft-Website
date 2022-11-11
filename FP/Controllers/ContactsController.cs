using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FP.Models;
using Microsoft.AspNetCore.Http;

namespace FP.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ModelContext _context;

        public ContactsController(ModelContext context)
        {
            _context = context;
        }

        // GET: Contacts
        public  IActionResult ContactUs()
        {
            ViewBag.cart = HttpContext.Session.GetInt32("cart");
            var contact = _context.Details.Where(x => x.Id == 1).SingleOrDefault();

            return View(contact);
        }
        [HttpPost]
        public async Task<IActionResult> AddContact(String subject,String message)
        {
            ViewBag.cart = HttpContext.Session.GetInt32("cart");

            Contact contact = new Contact();
            contact.Subject = subject;
            contact.Massege = message;
            contact.UserId = HttpContext.Session.GetInt32("UserId");
             _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();


            return View();
        }

        


       public IActionResult ContactUsVendor()
        {
            var contact = _context.Details.Where(x => x.Id == 1).SingleOrDefault();
            return View(contact);
        }
        [HttpPost]
        public async Task<IActionResult> AddContactVendor(String subject, String message)
        {

            Contact contact = new Contact();
            contact.Subject = subject;
            contact.Massege = message;
            contact.UserId = HttpContext.Session.GetInt32("VendorId");
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();


            return View();
        }

        public async Task<IActionResult> ViewContact()
        {

            var contact =await _context.Contacts.Include(x=>x.User).ToListAsync();
            return View(contact);

        }

        public async Task<IActionResult> Details( decimal id)
        {

            var contact =  _context.Contacts.Where(x=>x.Id==id).Include(x => x.User).SingleOrDefault();
            return View(contact);

        }


        public async Task<IActionResult> Edit(decimal id)
        {

            var contact =  _context.Contacts.Where(x => x.Id == id).Include(x => x.User).SingleOrDefault();
            return View(contact);

        }

        public async Task<IActionResult> Delete(decimal id)
        {

            var contact =  _context.Contacts.Where(x => x.Id == id).SingleOrDefault();

            _context.Contacts.Remove(contact);
           await  _context.SaveChangesAsync();

            return RedirectToAction("ViewContact","Contacts");

        }


        // GET: Contacts/Details/5

    }
}
