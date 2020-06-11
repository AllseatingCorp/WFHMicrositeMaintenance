using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using WFHMicrositeMaintenance.Models;

namespace WFHMicrositeMaintenance.Controllers
{
    public class UsersController : Controller
    {
        private readonly WFHMicrositeContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public UsersController(WFHMicrositeContext context, IWebHostEnvironment env, IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            List<User> users = await _context.User.ToListAsync();
            foreach (var user in users)
            {
                user.OrderNumber = user.UserId.ToString().PadLeft(8, '0');
            }
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }
            user.Notes = GetNotes(user.UserId);
            user.Product = await _context.Product.Where(x => x.ProductId == user.ProductId).Select(y => y.Chair).FirstOrDefaultAsync();

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            User user = new User
            {
                Language = "English"
            };
            string pin = "";
            do
            { 
                pin = new Random().Next(10000000, 99999999).ToString();
                if (_context.User.Where(x => x.Pin == pin).FirstOrDefault() != null)
                {
                    pin = "";
                }
            } while (pin == "");
            user.Pin = pin;
            user.Products = _context.Product.ToList();
            user.Languages = new SelectList(new List<string>() { "English", "French" });
            return View(user);
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,ProductId,EmailAddress,Language,Pin,Address1,Address2,City,ProvinceState,PostalZip,Country,Commercial,Emailed,Completed,InProduction,Shipped")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                List<ProductOption> productOptions = await _context.ProductOption.Where(x => x.ProductId == user.ProductId).ToListAsync();
                foreach(var item in productOptions)
                {
                    if (item.Default)
                    {
                        _context.Add(new UserSelection() { UserId = user.UserId, ProductOptionId = item.ProductOptionId, Type = item.Type });
                    }
                }
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(user.Pin) && user.Emailed == null)
                {
                    string url = _configuration.GetValue<string>("AppSettings:UserUrl") + user.ProductId;
                    string body = "<a href='" + url + "'>Click here</a> to access the site.<br/><br/>Your log in PIN is " + user.Pin + ".";
                    string file = _env.WebRootPath + "\\emails\\email1_" + user.Language + ".txt";
                    StreamReader sr = new StreamReader(file);
                    if (sr != null)
                    {
                        string[] parameters = new string[] { url, user.Pin };
                        body = string.Format(sr.ReadToEnd(), parameters);
                        sr.Close();
                        sr.Dispose();
                    }
                    string subject = "The way we sit matters.";
                    if (user.Language == "French") subject = "La position dans laquelle nous nous assoyons révèle bien des choses.";
                    var emessage = new MailMessage("postmaster@allseating.com", user.EmailAddress, subject, body)
                    {
                        IsBodyHtml = true,
                        BodyEncoding = System.Text.Encoding.UTF8
                    };
                    emessage.Bcc.Add("admin@allseating.com");
                    using SmtpClient SmtpMail = new SmtpClient("allfs90.allseating.com", 25)
                    {
                        UseDefaultCredentials = true
                    };
                    SmtpMail.Send(emessage);
                    emessage.Dispose();
                    user.Emailed = DateTime.Now;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            if (string.IsNullOrEmpty(user.Pin))
            {
                string pin = "";
                do
                {
                    pin = new Random().Next(10000000, 99999999).ToString();
                    if (_context.User.Where(x => x.Pin == pin).FirstOrDefault() != null)
                    {
                        pin = "";
                    }
                } while (pin == "");
                user.Pin = pin;
            }
            user.Products = await _context.Product.ToListAsync();
            user.Languages = new SelectList(new List<string>() { "English", "French" });
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,ProductId,EmailAddress,Language,Pin,AttnName,PhoneNumber,Address1,Address2,City,ProvinceState,PostalZip,Country,Commercial,Emailed,Completed,InProduction,Shipped")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
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
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }
            user.Product = await _context.Product.Where(x => x.ProductId == user.ProductId).Select(y => y.Chair).FirstOrDefaultAsync();

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            _context.User.Remove(user);
            List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == user.UserId).ToListAsync();
            _context.UserSelection.RemoveRange(userSelections);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Emails/5
        public async Task<IActionResult> Emails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(user.EmailAddress))
            {
                string url = _configuration.GetValue<string>("AppSettings:UserUrl") + user.ProductId;
                string body = "<a href='" + url + "'>Click here</a> to access the site.<br/><br/>Your log in PIN is " + user.Pin + ".";
                string file = _env.WebRootPath + "\\emails\\email1_" + user.Language + ".txt";
                StreamReader sr = new StreamReader(file, System.Text.Encoding.UTF8);
                if (sr != null)
                {
                    string[] parameters = new string[] { url, user.Pin };
                    body = string.Format(sr.ReadToEnd(), parameters);
                    sr.Close();
                    sr.Dispose();
                }
                string subject = "The way we sit matters.";
                if (user.Language == "French") subject = "La position dans laquelle nous nous assoyons révèle bien des choses.";
                var emessage = new MailMessage("postmaster@allseating.com", user.EmailAddress, subject, body)
                {
                    IsBodyHtml = true,
                    BodyEncoding = System.Text.Encoding.UTF8
                };
                emessage.Bcc.Add("admin@allseating.com");
                using SmtpClient SmtpMail = new SmtpClient("allfs90.allseating.com", 25)
                {
                    UseDefaultCredentials = true
                };
                SmtpMail.Send(emessage);
                emessage.Dispose();
                user.Emailed = DateTime.Now;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public string GetNewPin()
        {
            string pin = "";
            do
            {
                pin = new Random().Next(10000000, 99999999).ToString();
                if (_context.User.Where(x => x.Pin == pin).FirstOrDefault() != null)
                {
                    pin = "";
                }
            } while (pin == "");

            return pin;
        }

        public string GetNotes(int id)
        {
            string notes = "";
            List<UserNote> userNotes = _context.UserNote.Where(x => x.UserId == id).ToList();
            int count = userNotes.Count;
            foreach (var note in userNotes)
            {
                notes += "<b>" + note.Csuser + "</b> " + note.Date.ToString() + "<br/>";
                notes += note.Note.Replace("\n", "<br/>");
                if (--count > 0) notes += "<br/><br/>";
            }

            return notes;
        }

        public string SaveNotes(int id, string notes)
        {
            if (!string.IsNullOrEmpty(notes))
            {
                _context.UserNote.Add(new UserNote() { UserId = id, Csuser = User.Name(), Note = notes, Date = DateTime.Now });
                _context.SaveChanges();
            }

            return "";
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserId == id);
        }
    }
}
