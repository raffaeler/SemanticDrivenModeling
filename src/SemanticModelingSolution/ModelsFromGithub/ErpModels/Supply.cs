using System;

namespace ERP_Model.Models
{
    public class Supply
    {
        public Guid SupplyGuid { get; set; }
        public virtual Supplier SupplySupplier { get; set; }
        public DateTime SupplyDate { get; set; }
        public DateTime SupplyDeliveryDate { get; set; }
        public bool SupplyDeleted { get; set; }
    }

    public class SupplyItem
    {
        public Guid SupplyItemGuid { get; set; }

        public virtual StockItem SupplyItemStockItem { get; set; }
        public virtual Supply SupplyItemSupply { get; set; }
        public int SupplyQuantity { get; set; }
        public bool SupplyItemDeleted { get; set; }
    }
}