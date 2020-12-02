using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.ViewModels.Order
{
    public enum SortState
    {
        ProductNameAsc,
        ProductNameDesc,
        CustomerNameAsc,
        CustomerNameDesc,

        OrderDateAsc,
        OrderDateDesc,

        DeliveryAsc,
        DeliveryDesc,

        VolumeAsc,
        VolumeDesc
    }
    public class SortOrderViewModel
    {
        public SortState ProductNameSort { get; set; }
        public SortState CustomerNameSort { get; set; }
        public SortState OrderDateSort { get; set; }
        public SortState DeliverySort { get; set; }
        public SortState VolumeSort { get; set; }
        public SortState Current { get; set; }

        public SortOrderViewModel(SortState sortOrder)
        {
            ProductNameSort = sortOrder == SortState.ProductNameAsc ? SortState.ProductNameDesc : SortState.ProductNameAsc;
            CustomerNameSort = sortOrder == SortState.CustomerNameAsc ? SortState.CustomerNameDesc : SortState.CustomerNameAsc;
            OrderDateSort = sortOrder == SortState.OrderDateAsc ? SortState.OrderDateDesc : SortState.OrderDateAsc;
            DeliverySort = sortOrder == SortState.DeliveryAsc ? SortState.DeliveryDesc : SortState.DeliveryAsc;
            VolumeSort = sortOrder == SortState.VolumeAsc ? SortState.VolumeDesc : SortState.VolumeAsc;
            Current = sortOrder;
        }
    }
}
