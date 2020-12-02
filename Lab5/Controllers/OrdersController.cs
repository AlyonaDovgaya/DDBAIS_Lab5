using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab5.Data;
using Lab5.Models;
using Lab5.ViewModels.Order;

namespace Lab5.Controllers
{
    public class OrdersController : Controller
    {
        private readonly int _pageSize;
        private readonly Lab5Context _db;
        Services.CachingService _cachingService;

        public OrdersController(Data.Lab5Context db, Services.CachingService cachingService)
        {
            _pageSize = 5;
            _db = db;
            _cachingService = cachingService;
        }



        // GET: Orders
        public async Task<ActionResult> Index(int? selectedCustomerId, int? selectedProductId, string selectedDelivery,
            int? page, SortState? sortOrder)
        {
            if (!User.IsInRole(Areas.Identity.Roles.User))
            {
                return Redirect("~/Identity/Account/Login");
            }
            if (_cachingService.TryGetValue($"orders-{selectedCustomerId}-{selectedProductId}-{selectedDelivery}" +
                $"-{page}-{sortOrder}", out IndexOrderViewModel cachedModel))
            {
                return View(cachedModel);
            }
            else
            {
                GetSetCookieValuesOrSetDefault(ref selectedCustomerId, ref selectedProductId, ref selectedDelivery, ref page, ref sortOrder);

                IQueryable<Order> orders = _db.Orders
                    .Include(c => c.Product)
                    .Include(c => c.Customer);

                orders = Filter(orders, selectedCustomerId, selectedProductId, selectedDelivery);

                orders = Sort(orders, (SortState)sortOrder);

                var count = await orders.CountAsync();

                orders = Paging(ref page, orders, count);

                var customers = _db.Customers;
                var products = _db.Products;

                IndexOrderViewModel model = new IndexOrderViewModel
                {
                    Orders = await orders.ToListAsync(),
                    FilterOrderViewModel = new FilterOrderViewModel(await products.ToListAsync(), await customers.ToListAsync(),
                                  selectedProductId, selectedCustomerId, selectedDelivery),
                    SortOrderViewModel = new SortOrderViewModel((SortState)sortOrder),
                    PageViewModel = new ViewModels.PageViewModel(count, (int)page, _pageSize)
                };

                _cachingService.Set($"orders-{selectedCustomerId}-{selectedProductId}-{selectedDelivery}" +
                $"-{page}-{sortOrder}", model);

                return View(model);
            }
        }

        private IQueryable<Order> Paging(ref int? page, IQueryable<Order> orders, int count)
        {
            /*
             * Если мы меняем фильтр и находимся на странице, значение которой превышает максимальное количество страниц новых записей,
             * то необходимо перейти на максимальную страницу для записей с текущей фильтрацией
             */
            var pageModel = new ViewModels.PageViewModel(count, 1, _pageSize);
            if (page > pageModel.TotalPages)
            {
                page = pageModel.TotalPages > 0 ? pageModel.TotalPages : 1;
                Response.Cookies.Append(User.Identity.Name + "orderPage", page.ToString());
            }
            return orders.Skip(((int)page - 1) * _pageSize).Take(_pageSize);
        }

        private void GetSetCookieValuesOrSetDefault(ref int? selectedCustomerId, ref int? selectedProductId, ref string selectedDelivery, ref int? page, ref SortState? sortOrder)
        {
            if (selectedCustomerId == null)
            {
                if (Request.Cookies.TryGetValue(User.Identity.Name + "orderSelectedCustomerId", out string selectedCustomerIdStr))
                {
                    selectedCustomerId = int.Parse(selectedCustomerIdStr);
                }
            }
            else
            {
                Response.Cookies.Append(User.Identity.Name + "orderSelectedCustomerId", selectedCustomerId.ToString());
            }
            if (selectedProductId == null)
            {
                if (Request.Cookies.TryGetValue(User.Identity.Name + "orderSelectedProductId", out string selectedProductIdStr))
                {
                    selectedProductId = int.Parse(selectedProductIdStr);
                }
            }
            else
            {
                Response.Cookies.Append(User.Identity.Name + "orderSelectedProductId", selectedProductId.ToString());
            }
            if (string.IsNullOrEmpty(selectedDelivery))
            {
                if (HttpContext.Request.Query["isFromFilter"] == "true")
                {
                    selectedDelivery = "";
                    Response.Cookies.Append(User.Identity.Name + "orderSelectedDelivery", "");
                }
                else
                {
                    Request.Cookies.TryGetValue(User.Identity.Name + "orderSelectedDelivery", out selectedDelivery);
                }
            }
            else
            {
                Response.Cookies.Append(User.Identity.Name + "orderSelectedDelivery", selectedDelivery);
            }
            if (page == null)
            {
                if (Request.Cookies.TryGetValue(User.Identity.Name + "orderPage", out string pageStr))
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
                Response.Cookies.Append(User.Identity.Name + "orderPage", page.ToString());
            }
            if (sortOrder == null)
            {
                if (Request.Cookies.TryGetValue(User.Identity.Name + "orderSortState", out string sortStateStr))
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
                Response.Cookies.Append(User.Identity.Name + "orderSortState", sortOrder.ToString());
            }
        }

        private IQueryable<Order> Filter(IQueryable<Order> orders,
            int? selectedCustomerId, int? selectedProductId, string selectedDelivery)
        {
            if (selectedCustomerId != null && selectedCustomerId != 0)
            {
                orders = orders.Where(order => order.CustomerId == selectedCustomerId);
            }
            if (selectedProductId != null && selectedProductId != 0)
            {
                orders = orders.Where(order => order.ProductId == selectedProductId);
            }
            if (!string.IsNullOrEmpty(selectedDelivery))
            {
                orders = orders.Where(order => order.Delivery == (selectedDelivery));
            }
            return orders;
        }

        private IQueryable<Order> Sort(IQueryable<Order> orders, SortState sortOrder)
        {
            switch (sortOrder)
            {
                case SortState.ProductNameAsc:
                    orders = orders.OrderBy(c => c.Product.ProductName);
                    break;
                case SortState.ProductNameDesc:
                    orders = orders.OrderByDescending(c => c.Product.ProductName);
                    break;
                case SortState.CustomerNameAsc:
                    orders = orders.OrderBy(c => c.Customer.CustomerName);
                    break;
                case SortState.CustomerNameDesc:
                    orders = orders.OrderByDescending(c => c.Customer.CustomerName);
                    break;
                case SortState.OrderDateAsc:
                    orders = orders.OrderBy(c => c.OrderDate);
                    break;
                case SortState.OrderDateDesc:
                    orders = orders.OrderByDescending(c => c.OrderDate);
                    break;
                case SortState.DeliveryAsc:
                    orders = orders.OrderBy(c => c.Delivery);
                    break;
                case SortState.DeliveryDesc:
                    orders = orders.OrderByDescending(c => c.Delivery);
                    break;
                case SortState.VolumeAsc:
                    orders = orders.OrderBy(c => c.Volume);
                    break;
                case SortState.VolumeDesc:
                    orders = orders.OrderByDescending(c => c.Volume);
                    break;
            }
            return orders;
        }

        // GET: OrdersController/Create
        public async Task<ActionResult> Create()
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Orders");
            }
            ViewBag.Customers = new SelectList(await _db.Customers.ToListAsync(), "CustomerId", "CustomerName");
            ViewBag.Products = new SelectList(await _db.Products.ToListAsync(), "ProductId", "ProductName");
            return View();
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!User.IsInRole(Areas.Identity.Roles.User))
            {
                return Redirect("~/Identity/Account/Login");
            }
            if (id == null)
            {
                return NotFound();
            }

            var order = await _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,OrderDate,Delivery,Volume,ProductId,CustomerId")] Order order)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Orders");
            }
            try
            {
                if (order.OrderDate < new DateTime(1980, 1, 1))
                {
                    ModelState.AddModelError("OrderDate", "Date must be later than 01.01.1980");
                }
                else if (order.OrderDate > DateTime.Now)
                {
                    ModelState.AddModelError("OrderDate", "Date must be earlier than now");
                }
                if (ModelState.IsValid)
                {
                    await _db.Orders.AddAsync(order);
                    await _db.SaveChangesAsync();
                    _cachingService.Clean();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw new Exception("Model state is not valid. Redirect back");
                }
            }
            catch
            {
                ViewBag.Customers = new SelectList(await _db.Customers.ToListAsync(), "CustomerId", "CustomerName");
                ViewBag.Products = new SelectList(await _db.Products.ToListAsync(), "ProductId", "ProductName");
                return View();
            }
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int orderId)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Orders");
            }
            ViewBag.Customers = new SelectList(await _db.Customers.ToListAsync(), "CustomerId", "CustomerName");
            ViewBag.Products = new SelectList(await _db.Products.ToListAsync(), "ProductId", "ProductName");
            return View(await _db.Orders
                .Include(c => c.Product)
                .Include(c => c.Customer)
                .FirstOrDefaultAsync(c => c.OrderId == orderId));
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Order order)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Orders");
            }
            try
            {
                if (order.OrderDate < new DateTime(1980, 1, 1))
                {
                    ModelState.AddModelError("OrderDate", "Date must be later than 01.01.1980");
                }
                else if (order.OrderDate > DateTime.Now)
                {
                    ModelState.AddModelError("OrderDate", "Date must be earlier than now");
                }
                if (ModelState.IsValid)
                {
                    _db.Orders.Update(order);
                    await _db.SaveChangesAsync();
                    _cachingService.Clean();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw new Exception("Model state is not valid. Redirect back");
                }
            }
            catch
            {
                ViewBag.Customers = new SelectList(await _db.Customers.ToListAsync(), "CustomerId", "CustomerName");
                ViewBag.Products = new SelectList(await _db.Products.ToListAsync(), "ProductId", "ProductName");
                return View(await _db.Orders
                    .Include(c => c.Product)
                    .Include(c => c.Customer)
                    .FirstOrDefaultAsync(c => c.OrderId == order.OrderId));
            }
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int orderId)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Orders");
            }
            return View(await _db.Orders
                    .Include(c => c.Product)
                    .Include(c => c.Customer)
                    .FirstOrDefaultAsync(c => c.OrderId == orderId));
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int orderId)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Orders");
            }
            try
            {
                var order = await _db.Orders.FirstOrDefaultAsync(c => c.OrderId == orderId);
                _db.Orders.Remove(order);
                await _db.SaveChangesAsync();
                _cachingService.Clean();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(await _db.Orders
                   .Include(c => c.Product)
                   .Include(c => c.Customer)
                   .FirstOrDefaultAsync(c => c.OrderId == orderId));
            }
        }
    }
}
