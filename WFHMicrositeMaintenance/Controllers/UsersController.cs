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

            var user = await _context.User.FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }
            user.Notes = GetNotes(user.UserId);
            user.Product = await _context.Product.Where(x => x.ProductId == user.ProductId).Select(y => y.Chair).FirstOrDefaultAsync();
            user.UserSelections = await _context.UserSelection.Where(x => x.UserId == id).ToListAsync();
            ProductOption productOption;
            foreach (var item in user.UserSelections)
            {
                productOption = await _context.ProductOption.Where(x => x.ProductOptionId == item.ProductOptionId).FirstOrDefaultAsync();
                if (item.Type == "Fabric")
                {
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                }
                if (item.Type == "Mesh")
                {
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                }
                if (item.Type == "Frame")
                {
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                }
            }

            return View(user);
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            User user = new User
            {
                Language = "English"
            };
            string pin = "";
            do
            { 
                pin = new Random().Next(10000000, 99999999).ToString();
                if (await _context.User.Where(x => x.Pin == pin).FirstOrDefaultAsync() != null)
                {
                    pin = "";
                }
            } while (pin == "");
            user.Pin = pin;
            user.Products = await _context.Product.ToListAsync();
            user.Languages = new SelectList(new List<string>() { "English", "French" });
            return View(user);
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,ProductId,EmailAddress,Language,Pin,Address1,Address2,City,ProvinceState,PostalZip,Country,SpecialInstructions,Commercial,Emailed,Completed,InProduction,Shipped")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                var product = await _context.Product.FirstOrDefaultAsync(m => m.ProductId == user.ProductId);
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
                    if (product.VerifyOnly)
                        file = _env.WebRootPath + "\\emails\\email1v_" + user.Language + ".txt";
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
                    emessage.Bcc.Add("wfh@allseating.com");
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
            user.Languages = new SelectList(new List<string>() { "English", "French" });
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
        public async Task<IActionResult> Edit(int id, [Bind("UserId,ProductId,EmailAddress,Language,Pin,AttnName,PhoneNumber,Address1,Address2,City,ProvinceState,PostalZip,Country,SpecialInstructions,Commercial,Emailed,Completed,InProduction,Shipped")] User user)
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
            user.Languages = new SelectList(new List<string>() { "English", "French" });
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

        // GET: Users/Options
        public async Task<IActionResult> Selections(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User user = await _context.User.Where(x => x.UserId == id).FirstOrDefaultAsync();
            List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == id).OrderBy(y => y.Type).ToListAsync();
            Selections selections = new Selections
            {
                UserId = user.UserId,
                ProductId = user.ProductId,
                EmailAddress = user.EmailAddress
            };
            foreach (var item in userSelections)
            {
                item.Name = user.EmailAddress;
                item.Options = await _context.ProductOption.Where(x => x.ProductId == user.ProductId && x.Type == item.Type).ToListAsync();
                foreach (var option in item.Options)
                {
                    if (option.ProductOptionId == item.ProductOptionId)
                    {
                        option.Default = true;
                        switch (option.Type)
                        {
                            case "Fabric":
                                selections.Fabric = option.ProductOptionId;
                                break;
                            case "Mesh":
                                selections.Mesh = option.ProductOptionId;
                                break;
                            case "Frame":
                                selections.Frame = option.ProductOptionId;
                                break;
                        }
                    }
                    else
                        option.Default = false;
                }
            }
            selections.UserSelections = userSelections;
            return View(selections);
        }

        // POST: Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Selections(Selections selections)
        {
            List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == selections.UserId).ToListAsync();
            foreach (var item in userSelections)
            {
                switch (item.Type)
                {
                    case "Fabric":
                        item.ProductOptionId = selections.Fabric;
                        break;
                    case "Mesh":
                        item.ProductOptionId = selections.Mesh;
                        break;
                    case "Frame":
                        item.ProductOptionId = selections.Frame;
                        break;
                }
                _context.Entry(item).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Users/Emails/5
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
                if (user.Completed == null)
                {
                    var product = await _context.Product.FirstOrDefaultAsync(m => m.ProductId == user.ProductId);
                    string url = _configuration.GetValue<string>("AppSettings:UserUrl") + user.ProductId;
                    string body = "<a href='" + url + "'>Click here</a> to access the site.<br/><br/>Your log in PIN is " + user.Pin + ".";
                    string file = _env.WebRootPath + "\\emails\\email1_" + user.Language + ".txt";
                    if (product.VerifyOnly)
                        file = _env.WebRootPath + "\\emails\\email1v_" + user.Language + ".txt";
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
                    emessage.Bcc.Add("wfh@allseating.com");
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
                else
                {
                    var userSelections = await _context.UserSelection.Where(x => x.UserId == id).ToListAsync();
                    int option1 = 0;
                    int option2 = 0;
                    int option3 = 0;
                    foreach (var item in userSelections)
                    {
                        switch (item.Type)
                        {
                            case "Fabric":
                                option1 = item.ProductOptionId;
                                break;
                            case "Mesh":
                                option2 = item.ProductOptionId;
                                break;
                            case "Frame":
                                option3 = item.ProductOptionId;
                                break;
                        }
                    }
                    var productImage = await _context.ProductImage.Where(x => x.ProductId == user.ProductId && x.ProductOption1Id == option1 && x.ProductOption2Id == option2 && x.ProductOption3Id == option3).FirstOrDefaultAsync();
                    string url = "";
                    string body = "Your address and selections have been saved.<br/><br/><a href='" + url + ">Click here</a> to review your selections.";
                    string file = _env.WebRootPath + "\\emails\\email2_" + user.Language + ".txt";
                    StreamReader sr = new StreamReader(file, System.Text.Encoding.UTF8);
                    if (sr != null)
                    {
                        string[] parameters = new string[] { user.UserId.ToString().PadLeft(8, '0'), url };
                        body = string.Format(sr.ReadToEnd(), parameters);
                        sr.Close();
                        sr.Dispose();
                    }
                    string subject = user.Language == "English" ? "Thank you for your order!" : "Nous vous remercions de votre commande!";
                    var emessage = new MailMessage("postmaster@allseating.com", user.EmailAddress, subject, body);
                    emessage.Bcc.Add("admin@allseating.com");
                    emessage.Bcc.Add("wfh@allseating.com");
                    emessage.BodyEncoding = System.Text.Encoding.UTF8;
                    if (productImage != null)
                    {
                        emessage.Attachments.Add(new Attachment(new MemoryStream(productImage.Image), "Your Chair"));
                        emessage.Attachments[0].ContentId = "imageRef";
                    }
                    emessage.IsBodyHtml = true;
                    emessage.BodyEncoding = System.Text.Encoding.UTF8;
                    using SmtpClient SmtpMail = new SmtpClient("allfs90.allseating.com", 25)
                    {
                        UseDefaultCredentials = true
                    };
                    SmtpMail.Send(emessage);
                    emessage.Dispose();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Users/ResetProd/5
        public async Task<IActionResult> ResetProd(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User user = await _context.User.Where(x => x.UserId == id).FirstOrDefaultAsync();
            user.InProduction = null;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Users/ResetShip/5
        public async Task<IActionResult> ResetShip(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User user = await _context.User.Where(x => x.UserId == id).FirstOrDefaultAsync();
            user.Shipped = null;
            _context.Update(user);
            await _context.SaveChangesAsync();

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
