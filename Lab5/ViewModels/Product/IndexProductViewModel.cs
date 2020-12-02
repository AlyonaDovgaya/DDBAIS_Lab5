using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.ViewModels.Product
{
    public class IndexProductViewModel
    {
        public IEnumerable<Models.Product> Products { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public FilterProductViewModel FilterProductViewModel { get; set; }
        public SortProductViewModel SortProductViewModel { get; set; }
    }
}
