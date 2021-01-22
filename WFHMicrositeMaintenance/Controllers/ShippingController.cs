using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rotativa.AspNetCore;
using WFHMicrositeMaintenance.Models;

namespace WFHMicrositeMaintenance.Controllers
{
    [Authorize]
    public class ShippingController : Controller
    {
        private readonly WFHMicrositeContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public ShippingController(WFHMicrositeContext context, IWebHostEnvironment env, IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _configuration = configuration;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            Production production = new Production() { List = new List<ProductionList>() };
            List<User> users = await _context.User.Where(x => x.Completed != null && x.InProduction != null && (x.Shipped == null || x.TrackingNumber == null)).ToListAsync();
            bool completed;
            foreach (var user in users)
            {
                completed = await _context.Product.Where(x => x.ProductId == user.ProductId).Select(y => y.Completed).FirstOrDefaultAsync();
                if (!completed)
                {
                    user.OrderNumber = user.UserId.ToString().PadLeft(8, '0');
                    production.List.Add(new ProductionList()
                    {
                        User = user,
                        Product = await _context.Product.FindAsync(user.ProductId)
                    });
                }
            }
            return View(production);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User user = await _context.User.FindAsync(id);
            user.OrderNumber = user.UserId.ToString().PadLeft(8, '0');
            user.AttnName = user.AttnName.ToUpper();
            user.PhoneNumber = String.Format("{0:(###) ###-####}", user.PhoneNumber);
            user.Address1 = user.Address1.ToUpper();
            user.Address2 = !string.IsNullOrEmpty(user.Address2) ? user.Address2 : " ";
            user.City = user.City.ToUpper();
            user.ProvinceState = user.ProvinceState.ToUpper();
            user.PostalZip = user.PostalZip.ToUpper();
            user.Country = user.Country.ToUpper();
            int fabric = 0;
            int mesh = 0;
            int frame = 0;
            List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == id).ToListAsync();
            ProductOption productOption;
            foreach (var item in userSelections)
            {
                productOption = await _context.ProductOption.Where(x => x.ProductOptionId == item.ProductOptionId).FirstOrDefaultAsync();
                if (item.Type == "Fabric")
                {
                    fabric = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.Name;
                }
                if (item.Type == "Mesh")
                {
                    mesh = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.Name;
                }
                if (item.Type == "Frame")
                {
                    frame = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.Name;
                }
            }
            Production production = new Production
            {
                User = user,
                Product = await _context.Product.FindAsync(user.ProductId),
                UserSelections = userSelections,
                Image = await _context.ProductImage.Where(x => x.ProductId == user.ProductId && x.ProductOption1Id == fabric && x.ProductOption2Id == mesh && x.ProductOption3Id == frame).Select(y => y.Image).FirstOrDefaultAsync()
            };
            if (production.Image == null)
            {
                production.Image = production.Product.Image;
            }

            return View(production);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            User user = await _context.User.FindAsync(id);
            user.OrderNumber = user.UserId.ToString().PadLeft(8, '0');
            user.AttnName = user.AttnName.ToUpper();
            user.PhoneNumber = String.Format("{0:(###) ###-####}", user.PhoneNumber);
            user.Address1 = user.Address1.ToUpper();
            user.Address2 = !string.IsNullOrEmpty(user.Address2) ? user.Address2 : " ";
            user.City = user.City.ToUpper();
            user.ProvinceState = user.ProvinceState.ToUpper();
            user.PostalZip = user.PostalZip.ToUpper();
            user.Country = user.Country.ToUpper();
            int fabric = 0;
            int mesh = 0;
            int frame = 0;
            List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == id).ToListAsync();
            ProductOption productOption;
            foreach (var item in userSelections)
            {
                productOption = await _context.ProductOption.Where(x => x.ProductOptionId == item.ProductOptionId).FirstOrDefaultAsync();
                if (item.Type == "Fabric")
                {
                    fabric = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.Name;
                }
                if (item.Type == "Mesh")
                {
                    mesh = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.Name;
                }
                if (item.Type == "Frame")
                {
                    frame = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.Name;
                }
            }
            Production production = new Production
            {
                User = user,
                Product = await _context.Product.FindAsync(user.ProductId),
                UserSelections = userSelections,
                Image = await _context.ProductImage.Where(x => x.ProductId == user.ProductId && x.ProductOption1Id == fabric && x.ProductOption2Id == mesh && x.ProductOption3Id == frame).Select(y => y.Image).FirstOrDefaultAsync()
            };
            if (production.Image == null)
            {
                production.Image = production.Product.Image;
            }

            return View(production);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Production production)
        {
            if (id != production.User.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (production.User.Shipped == null)
                    {
                        production.User.Shipped = DateTime.Now;
                    }
                    _context.Update(production.User);
                    await _context.SaveChangesAsync();
                    if (!string.IsNullOrEmpty(production.User.TrackingNumber))
                    {
                        EmailUser(production.User);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(production.User.UserId))
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
            return View(production);
        }

        public async Task<byte[]> GetOrderPdf(int id, string trackingnumber)
        {
            Production production = new Production();
            int fabric = 0;
            int mesh = 0;
            int frame = 0;
            production.User = await _context.User.FindAsync(id);
            List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == id).ToListAsync();
            ProductOption productOption;
            foreach (var item in userSelections)
            {
                productOption = await _context.ProductOption.Where(x => x.ProductOptionId == item.ProductOptionId).FirstOrDefaultAsync();
                if (item.Type == "Fabric")
                {
                    fabric = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.Name;
                }
                if (item.Type == "Mesh")
                {
                    mesh = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.Name;
                }
                if (item.Type == "Frame")
                {
                    frame = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.Name;
                }
            }
            production.UserSelections = userSelections;
            production.User.PhoneNumber = String.Format("{0:(###) ###-####}", production.User.PhoneNumber);
            production.Image = await _context.ProductImage.Where(x => x.ProductId == production.User.ProductId && x.ProductOption1Id == fabric && x.ProductOption2Id == mesh && x.ProductOption3Id == frame).Select(y => y.Image).FirstOrDefaultAsync();
            if (production.Image == null)
            {
                production.Image = await _context.Product.Where(x => x.ProductId == production.User.ProductId).Select(y => y.Image).FirstOrDefaultAsync();
            }
            production.User.OrderNumber = production.User.UserId.ToString().PadLeft(8, '0');
            production.User.TrackingNumber = trackingnumber;

            var report = new ViewAsPdf("ShipPdf", production)
            {
                PageSize = Rotativa.AspNetCore.Options.Size.Letter,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 0, 0, 10),
                MinimumFontSize = 22
            };
            var byteArray = report.BuildFile(this.ControllerContext);
            return byteArray.Result;
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
            SmtpMail.Send(emessage);
            emessage.Dispose();
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserId == id);
        }
    }
}