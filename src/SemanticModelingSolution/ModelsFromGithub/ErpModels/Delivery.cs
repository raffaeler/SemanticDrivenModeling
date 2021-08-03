using System;

namespace ERP_Model.Models
{
    public class Delivery
    {
        public Guid DeliveryGuid { get; set; }

        public virtual Order DeliveryOrder { get; set; }

        public bool DeliveryDeleted { get; set; }
    }

    public class DeliveryItem
    {
        public Guid DeliveryItemGuid { get; set; }

        public virtual Delivery DeliveryItemDelivery { get; set; }
        public virtual OrderItem DeliveryItemOrderItem { get; set; }
        public int DeliveryItemQuantity { get; set; }
        public bool DeliveryItemDeleted { get; set; }
    }
}