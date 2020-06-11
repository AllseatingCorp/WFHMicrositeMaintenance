using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Rotativa.AspNetCore;
using WFHMicrositeMaintenance.Models;

namespace WFHMicrositeMaintenance.Controllers
{
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
            List<Production> production = new List<Production>();
            List<User> users = await _context.User.Where(x => x.Completed != null && x.InProduction != null).ToListAsync();
            foreach (var user in users)
            {
                user.OrderNumber = user.UserId.ToString().PadLeft(8, '0');
                production.Add(new Production()
                {
                    User = user,
                    Product = await _context.Product.FindAsync(user.ProductId)
                });
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
            Production production = new Production
            {
                User = user,
                Product = await _context.Product.FindAsync(user.ProductId)
            };
            int fabric = 0;
            int mesh = 0;
            List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == id).ToListAsync();
            foreach (var item in userSelections)
            {
                if (item.Type == "Fabric") fabric = item.ProductOptionId;
                if (item.Type == "Mesh") mesh = item.ProductOptionId;
            }
            production.Image = await _context.ProductImage.Where(x => x.ProductId == production.User.ProductId && x.ProductOption1Id == fabric && x.ProductOption2Id == mesh).Select(y => y.Image).FirstOrDefaultAsync();

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
            Production production = new Production
            {
                User = user,
                Product = await _context.Product.FindAsync(user.ProductId)
            };
            int fabric = 0;
            int mesh = 0;
            List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == id).ToListAsync();
            foreach (var item in userSelections)
            {
                if (item.Type == "Fabric") fabric = item.ProductOptionId;
                if (item.Type == "Mesh") mesh = item.ProductOptionId;
            }
            production.Image = await _context.ProductImage.Where(x => x.ProductId == production.User.ProductId && x.ProductOption1Id == fabric && x.ProductOption2Id == mesh).Select(y => y.Image).FirstOrDefaultAsync();

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
                    production.User.Shipped = DateTime.Now;
                    _context.Update(production.User);
                    await _context.SaveChangesAsync();
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

        public async Task<byte[]> GetOrderPdf(int id)
        {
            Production production = new Production();
            int fabric = 0;
            int mesh = 0;
            production.User = await _context.User.FindAsync(id);
            List<UserSelection> userSelections = await _context.UserSelection.Where(x => x.UserId == id).ToListAsync();
            foreach (var item in userSelections)
            {
                if (item.Type == "Fabric") fabric = item.ProductOptionId;
                if (item.Type == "Mesh") mesh = item.ProductOptionId;
            }
            production.User.PhoneNumber = String.Format("{0:(###) ###-####}", production.User.PhoneNumber);
            production.Image = await _context.ProductImage.Where(x => x.ProductId == production.User.ProductId && x.ProductOption1Id == fabric && x.ProductOption2Id == mesh).Select(y => y.Image).FirstOrDefaultAsync();
            production.User.OrderNumber = production.User.UserId.ToString().PadLeft(8, '0');

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

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserId == id);
        }
    }
}