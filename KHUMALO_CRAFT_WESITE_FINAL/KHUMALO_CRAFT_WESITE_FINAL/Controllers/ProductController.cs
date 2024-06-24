using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using KHUMALO_CRAFT_WESITE_FINAL.Models;

namespace KHUMALO_CRAFT_WESITE_FINAL.Controllers
{
    public class ProductController : Controller
    {
        private readonly Khumalocraftdb1Context _context;

        public ProductController(Khumalocraftdb1Context context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index(string searchString, string category)
        {
            // Fetch unique categories
            var categories = await _context.Products
                                           .Select(p => p.ProductCategory)
                                           .Distinct()
                                           .ToListAsync();

            // Store the categories in ViewBag
            ViewBag.Categories = categories;

            // Start with an IQueryable so that we can apply filters dynamically
            IQueryable<Product> products = _context.Products;

            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.ProductName.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(category))
            {
                products = products.Where(x => x.ProductCategory == category);
            }

            // Execute the query and get the list of products
            var productList = await products.ToListAsync();

            return View(productList);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name.Contains("admin.co.za"))
            {
                ViewBag.UserIds = _context.Users.Select(u => u.Userid).ToList();
                return View();
            }

            else
            {
                return Forbid();
            }
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,ProductDescription,ProductCategory,ProductAvailability,ProductPrice,ProductImage,Userid")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,ProductDescription,ProductCategory,ProductAvailability,ProductPrice,ProductImage,Userid")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
            return View(product);
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            // Redirect to the Index view after successful deletion
            return RedirectToAction(nameof(Index));
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
