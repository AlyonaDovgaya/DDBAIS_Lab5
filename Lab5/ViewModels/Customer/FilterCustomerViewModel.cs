using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.ViewModels.Customer
{
    public class FilterCustomerViewModel
    {
        public string SelectedCustomerName { get; set; }

        public FilterCustomerViewModel(string selectedCustomerName)
        {
            SelectedCustomerName = selectedCustomerName;
        }
    }
}
