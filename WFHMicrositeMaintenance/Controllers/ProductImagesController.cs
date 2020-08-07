using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WFHMicrositeMaintenance.Models;

namespace WFHMicrositeMaintenance.Controllers
{
    public class ProductImagesController : Controller
    {
        private readonly WFHMicrositeContext _context;

        public ProductImagesController(WFHMicrositeContext context)
        {
            _context = context;
        }

        // GET: ProductImages
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            List<ProductImage> productImages = await _context.ProductImage.Where(x => x.ProductId == id).ToListAsync();
            if (productImages.Count == 0)
            {
                productImages.Add(new ProductImage() { ProductId = (int)id });
            }
            productImages.FirstOrDefault().Product = await _context.Product.Where(x => x.ProductId == id).Select(y => y.Chair).FirstOrDefaultAsync();
            foreach (var item in productImages)
            {
                item.Option1 = await _context.ProductOption.Where(x => x.ProductOptionId == item.ProductOption1Id).Select(y => y.Name).FirstOrDefaultAsync();
                item.Option2 = await _context.ProductOption.Where(x => x.ProductOptionId == item.ProductOption2Id).Select(y => y.Name).FirstOrDefaultAsync();
                item.Option3 = await _context.ProductOption.Where(x => x.ProductOptionId == item.ProductOption3Id).Select(y => y.Name).FirstOrDefaultAsync();
            }

            return View(productImages);
        }

        // GET: ProductImages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productImage = await _context.ProductImage
                .FirstOrDefaultAsync(m => m.ProductImageId == id);
            if (productImage == null)
            {
                return NotFound();
            }
            productImage.Product = await _context.Product.Where(x => x.ProductId == id).Select(y => y.Chair).FirstOrDefaultAsync();
            productImage.Option1 = await _context.ProductOption.Where(x => x.ProductOptionId == productImage.ProductOption1Id).Select(y => y.Name).FirstOrDefaultAsync();
            productImage.Option2 = await _context.ProductOption.Where(x => x.ProductOptionId == productImage.ProductOption2Id).Select(y => y.Name).FirstOrDefaultAsync();
            productImage.Option3 = await _context.ProductOption.Where(x => x.ProductOptionId == productImage.ProductOption3Id).Select(y => y.Name).FirstOrDefaultAsync();

            return View(productImage);
        }

        // GET: ProductImages/Create
        public async Task<IActionResult> Create(int id)
        {
            ProductImage productImage = new ProductImage
            {
                ProductId = id,
                Product = await _context.Product.Where(x => x.ProductId == id).Select(y => y.Chair).FirstOrDefaultAsync(),
                Options1 = new SelectList(await _context.ProductOption.Where(x => x.ProductId == id && x.Type == "Fabric").ToListAsync(), "ProductOptionId", "Name"),
                Options2 = new SelectList(await _context.ProductOption.Where(x => x.ProductId == id && x.Type == "Mesh").ToListAsync(), "ProductOptionId", "Name"),
                Options3 = new SelectList(await _context.ProductOption.Where(x => x.ProductId == id && x.Type == "Frame").ToListAsync(), "ProductOptionId", "Name")
            };
            return View(productImage);
        }

        // POST: ProductImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductImageId,ProductId,ProductOption1Id,ProductOption2Id,ProductOption3Id,FileName,FormFile")] ProductImage productImage)
        {
            if (ModelState.IsValid)
            {
                if (productImage.FormFile != null)
                {
                    MemoryStream ms = new MemoryStream();
                    await productImage.FormFile.CopyToAsync(ms);
                    productImage.FileName = productImage.FormFile.FileName;
                    productImage.Image = ms.ToArray();
                }

                _context.Add(productImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = productImage.ProductId });
            }
            return View(productImage);
        }

        // GET: ProductImages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productImage = await _context.ProductImage.FindAsync(id);
            if (productImage == null)
            {
                return NotFound();
            }
            productImage.Product = await _context.Product.Where(x => x.ProductId == productImage.ProductId).Select(y => y.Chair).FirstOrDefaultAsync();
            productImage.Options1 = new SelectList(await _context.ProductOption.Where(x => x.ProductId == productImage.ProductId && x.Type == "Fabric").ToListAsync(), "ProductOptionId", "Name");
            productImage.Options2 = new SelectList(await _context.ProductOption.Where(x => x.ProductId == productImage.ProductId && x.Type == "Mesh").ToListAsync(), "ProductOptionId", "Name");
            productImage.Options3 = new SelectList(await _context.ProductOption.Where(x => x.ProductId == productImage.ProductId && x.Type == "Frame").ToListAsync(), "ProductOptionId", "Name");
            return View(productImage);
        }

        // POST: ProductImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductImageId,ProductId,ProductOption1Id,ProductOption2Id,ProductOption3Id,FileName,Image,FormFile")] ProductImage productImage)
        {
            if (id != productImage.ProductImageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (productImage.FormFile != null)
                    {
                        MemoryStream ms = new MemoryStream();
                        await productImage.FormFile.CopyToAsync(ms);
                        productImage.FileName = productImage.FormFile.FileName;
                        productImage.Image = ms.ToArray();
                    }

                    _context.Update(productImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductImageExists(productImage.ProductImageId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = productImage.ProductId });
            }
            return View(productImage);
        }

        // GET: ProductImages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productImage = await _context.ProductImage
                .FirstOrDefaultAsync(m => m.ProductImageId == id);
            if (productImage == null)
            {
                return NotFound();
            }
            productImage.Product = await _context.Product.Where(x => x.ProductId == id).Select(y => y.Chair).FirstOrDefaultAsync();
            productImage.Option1 = await _context.ProductOption.Where(x => x.ProductOptionId == productImage.ProductOption1Id).Select(y => y.Name).FirstOrDefaultAsync();
            productImage.Option2 = await _context.ProductOption.Where(x => x.ProductOptionId == productImage.ProductOption2Id).Select(y => y.Name).FirstOrDefaultAsync();
            productImage.Option3 = await _context.ProductOption.Where(x => x.ProductOptionId == productImage.ProductOption3Id).Select(y => y.Name).FirstOrDefaultAsync();

            return View(productImage);
        }

        // POST: ProductImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productImage = await _context.ProductImage.FindAsync(id);
            _context.ProductImage.Remove(productImage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = productImage.ProductId });
        }

        private bool ProductImageExists(int id)
        {
            return _context.ProductImage.Any(e => e.ProductImageId == id);
        }
    }
}
