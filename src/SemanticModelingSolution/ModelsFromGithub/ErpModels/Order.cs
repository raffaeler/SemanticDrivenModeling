using System;

namespace ERP_Model.Models
{
    public class Order
    {
        public Guid OrderGuid { get; set; }

        public virtual Customer OrderCustomer { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime OrderDeliveryDate { get; set; }
        public bool OrderDeleted { get; set; }
    }

    public class OrderItem
    {
        public Guid OrderItemGuid { get; set; }

        public virtual StockItem OrderItemStockItem { get; set; }
        public virtual Order OrderItemOrder { get; set; }
        public int OrderQuantity { get; set; }
        public bool OrderItemDeleted { get; set; }
    }
}