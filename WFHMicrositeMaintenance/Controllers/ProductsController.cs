using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WFHMicrositeMaintenance.Models;

namespace WFHMicrositeMaintenance.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly WFHMicrositeContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public ProductsController(WFHMicrositeContext context, IWebHostEnvironment env, IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Product.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            Product product = new Product
            {
                Language = "English"
            };
            product.Languages = new SelectList(new List<string>() { "English", "French" });
            product.Shippers = new SelectList(new List<string>() { "FEDEX", "UPS" });
            return View(product);
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,DealerCode,Ponumber,Chair,Language,FormFile,FormFile2,InstallGuide,UserGuide,VideoUrl,SitFitGuide,VerifyOnly,Shipper")] Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.FormFile != null)
                {
                    MemoryStream ms = new MemoryStream();
                    await product.FormFile.CopyToAsync(ms);
                    product.LogoFile = product.FormFile.FileName;
                    product.LogoImage = ms.ToArray();
                }
                if (product.FormFile2 != null)
                {
                    MemoryStream ms = new MemoryStream();
                    await product.FormFile2.CopyToAsync(ms);
                    product.LogoFile2 = product.FormFile2.FileName;
                    product.LogoImage2 = ms.ToArray();
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            product.Languages = new SelectList(new List<string>() { "English", "French" });
            product.Shippers = new SelectList(new List<string>() { "FEDEX", "UPS" });
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            product.Languages = new SelectList(new List<string>() { "English", "French" });
            product.Shippers = new SelectList(new List<string>() { "FEDEX", "UPS" });
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,DealerCode,Ponumber,Chair,Language,LogoFile,LogoImage,FormFile,LogoFile2,LogoImage2,FormFile2,InstallGuide,UserGuide,VideoUrl,SitFitGuide,VerifyOnly,Shipper")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (product.FormFile != null)
                {
                    MemoryStream ms = new MemoryStream();
                    await product.FormFile.CopyToAsync(ms);
                    product.LogoFile = product.FormFile.FileName;
                    product.LogoImage = ms.ToArray();
                }
                if (product.FormFile2 != null)
                {
                    MemoryStream ms = new MemoryStream();
                    await product.FormFile2.CopyToAsync(ms);
                    product.LogoFile2 = product.FormFile2.FileName;
                    product.LogoImage2 = ms.ToArray();
                }

                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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
            product.Languages = new SelectList(new List<string>() { "English", "French" });
            product.Shippers = new SelectList(new List<string>() { "FEDEX", "UPS" });
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            _context.Product.Remove(product);
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

            var product = await _context.Product.FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            if (System.IO.File.Exists(_env.WebRootPath + "//EmailLog.txt"))
            {
                System.IO.File.Delete(_env.WebRootPath + "//EmailLog.txt");
            }
            StreamWriter sw = new StreamWriter(_env.WebRootPath + "//EmailLog.txt", true);

            List<User> users = await _context.User.Where(x => x.ProductId == id && x.Emailed == null).Take(_configuration.GetValue<int>("AppSettings:EmailCount")).ToListAsync();
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.EmailAddress))
                {
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
                        IsBodyHtml = true
                    };
                    emessage.Bcc.Add("admin@allseating.com");
                    emessage.Bcc.Add("wfh@allseating.com");
                    using SmtpClient SmtpMail = new SmtpClient("allfs90.allseating.com", 25)
                    {
                        UseDefaultCredentials = true
                    };
                    try
                    {
                        SmtpMail.Send(emessage);
                        emessage.Dispose();
                        user.Emailed = DateTime.Now;
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                    }
                    catch
                    {
                        if (sw != null)
                        {
                            sw.WriteLine("Unable to send email to " + user.EmailAddress);
                            sw.Flush();
                        }
                    }
                }
            }

            if (sw != null)
            {
                sw.Close();
                sw.Dispose();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Complete/5
        public async Task<IActionResult> Complete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            List<User> users = await _context.User.Where(x => x.ProductId == id && x.Emailed != null && x.Completed == null).ToListAsync();
            foreach (var user in users)
            {
                user.Completed = DateTime.Now;
                _context.Update(user);
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Complete/5
        public async Task<IActionResult> Ship(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            product.Languages = new SelectList(new List<string>() { "English", "French" });
            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ship(int? id, Product product)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.TrackingNumber))
            {
                List<User> users = await _context.User.Where(x => x.ProductId == id && x.Emailed != null && x.Completed != null && x.InProduction != null && x.Shipped == null).ToListAsync();
                foreach (var user in users)
                {
                    user.Shipped = DateTime.Now;
                    user.TrackingNumber = product.TrackingNumber;
                    _context.Update(user);
                    EmailUser(user);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                List<User> users = await _context.User.Where(x => x.ProductId == id && x.Emailed != null && x.Completed != null && x.InProduction != null && x.Shipped == null && x.TrackingNumber != null).ToListAsync();
                foreach (var user in users)
                {
                    user.Shipped = DateTime.Now;
                    _context.Update(user);
                    EmailUser(user);
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void EmailUser(User user)
        {
            string shipper = _context.Product.Where(x => x.ProductId == user.ProductId).Select(y => y.Shipper).FirstOrDefault() == "UPS" ? "AppSettings:UpsUrl" : "AppSettings:FdxUrl";
            string url = _configuration.GetValue<string>(shipper) + user.TrackingNumber;
            string body = "Your customized Allseating order can now be tracked.<br/><br/><a href='" + url + "'>Click here</a> to track your chair.";
            string file = _env.WebRootPath + "\\emails\\email3_" + user.Language + ".txt";
            StreamReader sr = new StreamReader(file, System.Text.Encoding.UTF8);
            if (sr != null)
            {
                string[] parameters = new string[] { url, user.TrackingNumber };
                body = string.Format(sr.ReadToEnd(), parameters);
                sr.Close();
                sr.Dispose();
            }
            string subject = "Your custom chair can now be tracked!";
            if (user.Language == "French") subject = "Il vous est maintenant possible de suivre votre chaise personnalisée!";
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
            try
            {
                SmtpMail.Send(emessage);
                emessage.Dispose();
            }
            catch { }
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}
