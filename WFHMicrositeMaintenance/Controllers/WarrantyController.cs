using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WFHMicrositeMaintenance.Models;

namespace WFHMicrositeMaintenance.Controllers
{
    [Authorize]
    public class WarrantyController : Controller
    {
        private readonly WFHMicrositeContext _context;

        public WarrantyController(WFHMicrositeContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            Production production = new Production() { List = new List<ProductionList>() };
            List<User> users = await _context.User.Where(x => x.Shipped != null).ToListAsync();
            foreach (var user in users)
            {
                user.OrderNumber = user.UserId.ToString().PadLeft(8, '0');
                production.List.Add(new ProductionList()
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
                    item.Name = productOption.StockCode;
                }
                if (item.Type == "Mesh")
                {
                    mesh = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                }
                if (item.Type == "Frame")
                {
                    frame = item.ProductOptionId;
                    item.Image = productOption.Image;
                    item.Name = productOption.StockCode;
                }
            }
            Production production = new Production
            {
                User = user,
                Product = await _context.Product.FindAsync(user.ProductId),
                UserSelections = userSelections,
                Image = await _context.ProductImage.Where(x => x.ProductId == user.ProductId && x.ProductOption1Id == fabric && x.ProductOption2Id == mesh && x.ProductOption3Id == frame).Select(y => y.Image).FirstOrDefaultAsync()
            };

            return View(production);
        }
    }
}