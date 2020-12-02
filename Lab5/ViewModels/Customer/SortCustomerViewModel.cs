using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.ViewModels.Customer
{
    public class SortCustomerViewModel
    {
        public enum SortState
        {
            CustomerNameAsc,
            CustomerNameDesc
        }
        public SortState CustomerSort;
        public SortState Current;

        public SortCustomerViewModel(SortState sortOrder)
        {
            CustomerSort = sortOrder == SortState.CustomerNameAsc ? SortState.CustomerNameDesc : SortState.CustomerNameAsc;
            Current = sortOrder;
        }
    }
}
