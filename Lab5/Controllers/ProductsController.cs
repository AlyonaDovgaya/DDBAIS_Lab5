using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab5.Data;
using Lab5.Models;
using static Lab5.ViewModels.Product.SortProductViewModel;
using Lab5.ViewModels.Product;

namespace Lab5.Controllers
{
    public class ProductsController : Controller
    {
        private readonly Lab5Context _context;
        private readonly int _pageSize;

        public ProductsController(Lab5Context context)
        {
            _context = context;
            _pageSize = 5;
        }

        // GET: Products
        public async Task<IActionResult> Index(string selectedProductName, int? page, SortState? sortOrder)
        {
            if(!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
            GetSetCookieValuesOrSetDefault(ref selectedProductName, ref page, ref sortOrder);

            var products = _context.Products.AsQueryable();

            products = Filter(products, selectedProductName);

            products = Sort(products, (SortState)sortOrder);

            var count = await products.CountAsync();

            products = Paging(ref page, products, count);

            IndexProductViewModel model = new IndexProductViewModel
            {
                Products = await products.ToListAsync(),
                FilterProductViewModel = new FilterProductViewModel(selectedProductName),
                SortProductViewModel = new SortProductViewModel((SortState)sortOrder),
                PageViewModel = new ViewModels.PageViewModel(count, (int)page, _pageSize)
            };

            return View(model);
        }

        private IQueryable<Product> Paging(ref int? page, IQueryable<Product> products, int count)
        {
            /*
             * Если мы меняем фильтр и находимся на странице, значение которой превышает максимальное количество страниц новых записей,
             * то необходимо перейти на максимальную страницу для записей с текущей фильтрацией
             */
            var pageModel = new ViewModels.PageViewModel(count, 1, _pageSize);
            if (page > pageModel.TotalPages)
            {
                page = pageModel.TotalPages > 0 ? pageModel.TotalPages : 1;
                Response.Cookies.Append(User.Identity.Name + "productPage", page.ToString());
            }
            return products.Skip(((int)page - 1) * _pageSize).Take(_pageSize);
        }

        private IQueryable<Product> Sort(IQueryable<Product> products, SortState sortOrder)
        {
            switch (sortOrder)
            {
                case SortState.ProductNameAsc:
                    products = products.OrderBy(c => c.ProductName);
                    break;
                case SortState.ProductNameDesc:
                    products = products.OrderByDescending(c => c.ProductName);
                    break;
            }
            return products;
        }

        private IQueryable<Product> Filter(IQueryable<Product> products, string selectedProductName)
        {
            if (!string.IsNullOrEmpty(selectedProductName))
            {
                products = products.Where(c => c.ProductName.Contains(selectedProductName));
            }
            return products;
        }

        private void GetSetCookieValuesOrSetDefault(ref string selectedProductName, ref int? page, ref SortState? sortOrder)
        {
            if (string.IsNullOrEmpty(selectedProductName))
            {
                if (HttpContext.Request.Query["isFromFilter"] == "true")
                {
                    selectedProductName = "";
                    Response.Cookies.Append(User.Identity.Name + "productSelectedName", "");
                }
                else
                {
                    Request.Cookies.TryGetValue(User.Identity.Name + "productSelectedName", out selectedProductName);
                }
            }
            else
            {
                Response.Cookies.Append(User.Identity.Name + "productSelectedName", selectedProductName);
            }
            if (page == null)
            {
                if (Request.Cookies.TryGetValue(User.Identity.Name + "productPage", out string pageStr))
                {
                    int pg;
                    if (int.TryParse(pageStr, out pg))
                    {
                        page = pg;
                    }
                    else
                    {
                        page = 1;
                    }
                }
                else
                {
                    page = 1;
                }
            }
            else
            {
                Response.Cookies.Append(User.Identity.Name + "productPage", page.ToString());
            }
            if (sortOrder == null)
            {
                if (Request.Cookies.TryGetValue(User.Identity.Name + "productSortState", out string sortStateStr))
                {
                    sortOrder = (SortState)Enum.Parse(typeof(SortState), sortStateStr);
                }
                else
                {
                    sortOrder = SortState.ProductNameAsc;
                }
            }
            else
            {
                Response.Cookies.Append(User.Identity.Name + "productSortState", sortOrder.ToString());
            }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
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

        // GET: Products/Create
        public IActionResult Create()
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,Storage,Packaging")] Product product)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
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

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Storage,Packaging")] Product product)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
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

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
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

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
