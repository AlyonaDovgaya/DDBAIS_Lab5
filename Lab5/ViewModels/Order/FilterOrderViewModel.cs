using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.ViewModels.Order
{
    public class FilterOrderViewModel
    {
        public SelectList Products { get; set; }
        public int? SelectedProductId { get; set; }

        public SelectList Customers { get; set; }
        public int? SelectedCustomerId { get; set; }

        public string SelectedDelivery { get; set; }

        public FilterOrderViewModel(List<Models.Product> products,
                                        List<Models.Customer> customers,
                                        int? selectedProductId,
                                        int? selectedCustomerId,
                                        string selectedDelivery)
        {
            products.Insert(0, new Models.Product { ProductId = 0, ProductName = "All" });
            customers.Insert(0, new Models.Customer { CustomerId = 0, CustomerName = "All" });

            Products = new SelectList(products, "ProductId", "ProductName", selectedProductId);
            SelectedProductId = selectedProductId;

            Customers = new SelectList(customers, "CustomerId", "CustomerName", selectedCustomerId);
            SelectedCustomerId = selectedCustomerId;

            SelectedDelivery = selectedDelivery;
        }
    }
}
