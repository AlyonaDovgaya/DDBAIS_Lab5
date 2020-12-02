using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.ViewModels.Product
{
    public class FilterProductViewModel
    {
        public string SelectedProductName { get; set; }
        public FilterProductViewModel(string selectedProductName)
        {
            SelectedProductName = selectedProductName;
        }
    }
}
