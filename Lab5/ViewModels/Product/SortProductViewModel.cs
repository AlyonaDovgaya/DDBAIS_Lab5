using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.ViewModels.Product
{
    public class SortProductViewModel
    {
        public enum SortState
        {
            ProductNameAsc,
            ProductNameDesc
        }
        public SortState ProductSort { get; set; }
        public SortState Current { get; set; }
        public SortProductViewModel(SortState sortOrder)
        {
            ProductSort = sortOrder == SortState.ProductNameAsc ? SortState.ProductNameDesc : SortState.ProductNameAsc;
            Current = sortOrder;
        }
    }
}
