using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.ViewModels.Customer
{
    public class IndexCustomerViewModel
    {
        public IEnumerable<Models.Customer> Customers { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterCustomerViewModel FilterCustomerViewModel { get; set; }
        public SortCustomerViewModel SortCustomerViewModel { get; set; }
    }
}
