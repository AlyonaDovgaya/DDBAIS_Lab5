using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab5.Data;
using Lab5.Models;
using Lab5.ViewModels.Customer;
using static Lab5.ViewModels.Customer.SortCustomerViewModel;

namespace Lab5.Controllers
{
    public class CustomersController : Controller
    {
        private readonly Lab5Context _context;
        private readonly int _pageSize;

        public CustomersController(Lab5Context context)
        {
            _context = context;
            _pageSize = 5;
        }

        // GET: Customers
        public async Task<IActionResult> Index(string selectedCustomerName, int? page, SortState? sortOrder)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
            GetSetCookieValuesOrSetDefault(ref selectedCustomerName, ref page, ref sortOrder);

            var customers = _context.Customers.AsQueryable();

            customers = Filter(customers, selectedCustomerName);

            customers = Sort(customers, (SortState)sortOrder);

            var count = await customers.CountAsync();

            customers = Paging(ref page, customers, count);

            IndexCustomerViewModel model = new IndexCustomerViewModel
            {
                Customers = await customers.ToListAsync(),
                FilterCustomerViewModel = new FilterCustomerViewModel(selectedCustomerName),
                SortCustomerViewModel = new SortCustomerViewModel((SortState)sortOrder),
                PageViewModel = new ViewModels.PageViewModel(count, (int)page, _pageSize)
            };

            return View(model);
        }

        private IQueryable<Customer> Paging(ref int? page, IQueryable<Customer> customers, int count)
        {
            /*
             * Если мы меняем фильтр и находимся на странице, значение которой превышает максимальное количество страниц новых записей,
             * то необходимо перейти на максимальную страницу для записей с текущей фильтрацией
             */
            var pageModel = new ViewModels.PageViewModel(count, 1, _pageSize);
            if (page > pageModel.TotalPages)
            {
                page = pageModel.TotalPages > 0 ? pageModel.TotalPages : 1;
                Response.Cookies.Append(User.Identity.Name + "customerPage", page.ToString());
            }
            return customers.Skip(((int)page - 1) * _pageSize).Take(_pageSize);
        }

        private IQueryable<Customer> Sort(IQueryable<Customer> customers, SortState sortOrder)
        {
            switch (sortOrder)
            {
                case SortState.CustomerNameAsc:
                    customers = customers.OrderBy(c => c.CustomerName);
                    break;
                case SortState.CustomerNameDesc:
                    customers = customers.OrderByDescending(c => c.CustomerName);
                    break;
            }
            return customers;
        }

        private IQueryable<Customer> Filter(IQueryable<Customer> customers, string selectedCustomerName)
        {
            if (!string.IsNullOrEmpty(selectedCustomerName))
            {
                customers = customers.Where(c => c.CustomerName.Contains(selectedCustomerName));
            }
            return customers;
        }

        private void GetSetCookieValuesOrSetDefault(ref string selectedCustomerName, ref int? page, ref SortState? sortOrder)
        {
            if (string.IsNullOrEmpty(selectedCustomerName))
            {
                /*
                 * Если пользователь обращается из формы фильтрации, то тогда даже если передаётся пустая строка, значит он ввёл пустую строку
                 * и все компоненты необходимо фильтровать по пустому полю модели
                 */
                if (HttpContext.Request.Query["isFromFilter"] == "true")
                {
                    selectedCustomerName = "";
                    Response.Cookies.Append(User.Identity.Name + "customerSelectedName", "");
                }
                else
                {
                    Request.Cookies.TryGetValue(User.Identity.Name + "customerSelectedName", out selectedCustomerName);
                }
            }
            else
            {
                Response.Cookies.Append(User.Identity.Name + "customerSelectedName", selectedCustomerName);
            }
            if (page == null)
            {
                if (Request.Cookies.TryGetValue(User.Identity.Name + "customerPage", out string pageStr))
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
                Response.Cookies.Append(User.Identity.Name + "customerPage", page.ToString());
            }
            if (sortOrder == null)
            {
                if (Request.Cookies.TryGetValue(User.Identity.Name + "customerSortState", out string sortStateStr))
                {
                    sortOrder = (SortState)Enum.Parse(typeof(SortState), sortStateStr);
                }
                else
                {
                    sortOrder = SortState.CustomerNameAsc;
                }
            }
            else
            {
                Response.Cookies.Append(User.Identity.Name + "customerSortState", sortOrder.ToString());
            }
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,CustomerName,CustomerAddress,TelNumber")] Customer customer)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
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

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,CustomerName,CustomerAddress,TelNumber")] Customer customer)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
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
            return View(customer);
        }

        // GET: Customers/Delete/5
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

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!User.IsInRole(Areas.Identity.Roles.Admin))
            {
                return RedirectToAction("Index", "Home");
            }
            var customer = await _context.Customers.FindAsync(id);
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
