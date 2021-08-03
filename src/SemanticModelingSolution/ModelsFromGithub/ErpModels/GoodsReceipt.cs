using System;

namespace ERP_Model.Models
{
    public class GoodsReceipt
    {
        public Guid GoodsReceiptGuid { get; set; }

        public virtual Supply GoodsReceiptSupply { get; set; }
        public bool GoodsReceiptDeleted { get; set; }
    }

    public class GoodsReceiptItem
    {
        public Guid GoodsReceiptItemGuid { get; set; }

        public virtual GoodsReceipt GoodsReceiptItemGoodsReceipt { get; set; }
        public virtual SupplyItem GoodsReceiptItemSupplyItem { get; set; }
        public int GoodsReceiptItemQuantity { get; set; }
        public bool GoodsReceiptItemDeleted { get; set; }
    }
}