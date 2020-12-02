using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.ViewModels.Order
{
    public class IndexOrderViewModel
    {
        public IEnumerable<Models.Order> Orders { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterOrderViewModel FilterOrderViewModel { get; set; }
        public SortOrderViewModel SortOrderViewModel { get; set; }
    }
}
