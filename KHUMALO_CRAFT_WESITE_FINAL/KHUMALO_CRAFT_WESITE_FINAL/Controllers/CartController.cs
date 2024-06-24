using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KHUMALO_CRAFT_WESITE_FINAL.Models;
using System.Text;

namespace KHUMALO_CRAFT_WESITE_FINAL.Controllers
{
    public class CartController : Controller
    {
        private readonly Khumalocraftdb1Context _context;

        public CartController(Khumalocraftdb1Context context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var userId = GetUserId();
            var cartItem = await _context.Carts.FirstOrDefaultAsync(c => c.ProductId == productId && c.Userid == userId);

            if (cartItem == null)
            {
                cartItem = new Cart
                {
                    ProductId = productId,
                    Userid = userId
                };
                _context.Carts.Add(cartItem);
            }
            else
            {
                // Handle scenario where product already exists in cart
            }

            await _context.SaveChangesAsync();

            // Trigger Azure Function to start payment orchestration
            using (var httpClient = new HttpClient())
            {
                var requestPayload = new { orderId = cartItem.CartId }; // Assuming orderId is CartId for simplicity
                var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(requestPayload);
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var functionUrl = "https://durablefunction20240623213939.azurewebsites.net"; // Replace with your Azure Function URL
                var response = await httpClient.PostAsync(functionUrl, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    // Handle failure scenario
                    // Example: return a view indicating failure to start payment process
                    return View("PaymentError");
                }
            }

            return RedirectToAction("Index", "Product");
        }


        private int GetUserId()
        {
           
            return 1; 
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId(); // Replace with actual method to get the current user's ID
            var userCartItems = _context.Carts.Include(c => c.Product).Include(c => c.User)
                                              .Where(c => c.Userid == userId);
            return View(await userCartItems.ToListAsync());
        }



        // GET: Cart/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Product)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CartId == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // GET: Cart/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId");
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid");
            return View();
        }

        // POST: Cart/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CartId,ProductId,Userid")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", cart.ProductId);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", cart.Userid);
            return View(cart);
        }

        // GET: Cart/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", cart.ProductId);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", cart.Userid);
            return View(cart);
        }

        // POST: Cart/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CartId,ProductId,Userid")] Cart cart)
        {
            if (id != cart.CartId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartExists(cart.CartId))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductId", cart.ProductId);
            ViewData["Userid"] = new SelectList(_context.Users, "Userid", "Userid", cart.Userid);
            return View(cart);
        }

        // GET: Cart/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cart = await _context.Carts
                .Include(c => c.Product)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CartId == id);
            if (cart == null)
            {
                return NotFound();
            }

            return View(cart);
        }

        // POST: Cart/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.CartId == id);
        }
    }
}
