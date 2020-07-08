using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using WFHMicrositeMaintenance.Models;

namespace WFHMicrositeMaintenance.Controllers
{
    public class ProductOptionsController : Controller
    {
        private readonly WFHMicrositeContext _context;

        public ProductOptionsController(WFHMicrositeContext context)
        {
            _context = context;
        }

        // GET: ProductOptions
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            List<ProductOption> productOptions = await _context.ProductOption.Where(x => x.ProductId == id).OrderBy(y => y.Type).ToListAsync();
            if (productOptions.Count == 0)
            {
                productOptions.Add(new ProductOption() { ProductId = (int)id });
            }
            productOptions.FirstOrDefault().Product = await _context.Product.Where(x => x.ProductId == id).Select(y => y.Chair).FirstOrDefaultAsync();

            return View(productOptions);
        }

        // GET: ProductOptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productOption = await _context.ProductOption
                .FirstOrDefaultAsync(m => m.ProductOptionId == id);
            if (productOption == null)
            {
                return NotFound();
            }
            productOption.Product = await _context.Product.Where(x => x.ProductId == id).Select(y => y.Chair).FirstOrDefaultAsync();

            return View(productOption);
        }

        // GET: ProductOptions/Create
        public async Task<IActionResult> Create(int id)
        {
            ProductOption productOption = new ProductOption
            {
                ProductId = id,
                Product = await _context.Product.Where(x => x.ProductId == id).Select(y => y.Chair).FirstOrDefaultAsync(),
                Types = new SelectList(new List<string>() { "Fabric", "Mesh", "Frame" })
            };
            return View(productOption);
        }

        // POST: ProductOptions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductOptionId,ProductId,Type,Name,FormFile,Default,StockCode,Disabled")] ProductOption productOption)
        {
            if (ModelState.IsValid)
            {
                if (productOption.FormFile != null)
                {
                    MemoryStream ms = new MemoryStream();
                    await productOption.FormFile.CopyToAsync(ms);
                    productOption.FileName = productOption.FormFile.FileName;
                    productOption.Image = ms.ToArray();
                }

                _context.Add(productOption);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = productOption.ProductId });
            }
            return View(productOption);
        }

        // GET: ProductOptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productOption = await _context.ProductOption.FindAsync(id);
            if (productOption == null)
            {
                return NotFound();
            }
            productOption.Product = await _context.Product.Where(x => x.ProductId == productOption.ProductId).Select(y => y.Chair).FirstOrDefaultAsync();
            productOption.Types = new SelectList(new List<string>() { "Fabric", "Mesh", "Frame" });

            return View(productOption);
        }

        // POST: ProductOptions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductOptionId,ProductId,Type,Name,FileName,Image,FormFile,Default,StockCode,Disabled")] ProductOption productOption)
        {
            if (id != productOption.ProductOptionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (productOption.FormFile != null)
                {
                    MemoryStream ms = new MemoryStream();
                    await productOption.FormFile.CopyToAsync(ms);
                    productOption.FileName = productOption.FormFile.FileName;
                    productOption.Image = ms.ToArray();
                }

                try
                {
                    _context.Update(productOption);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductOptionExists(productOption.ProductOptionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = productOption.ProductId });
            }
            return View(productOption);
        }

        // GET: ProductOptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productOption = await _context.ProductOption
                .FirstOrDefaultAsync(m => m.ProductOptionId == id);
            if (productOption == null)
            {
                return NotFound();
            }
            productOption.Product = await _context.Product.Where(x => x.ProductId == id).Select(y => y.Chair).FirstOrDefaultAsync();

            return View(productOption);
        }

        // POST: ProductOptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productOption = await _context.ProductOption.FindAsync(id);
            _context.ProductOption.Remove(productOption);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = productOption.ProductId });
        }

        private bool ProductOptionExists(int id)
        {
            return _context.ProductOption.Any(e => e.ProductOptionId == id);
        }
    }
}
