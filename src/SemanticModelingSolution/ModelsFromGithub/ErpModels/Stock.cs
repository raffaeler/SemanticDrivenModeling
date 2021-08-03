using System;

namespace ERP_Model.Models
{
    public class Stock
    {
        public Guid StockGuid { get; set; }

        public string StockName { get; set; }

        public virtual Address StockAddress { get; set; }

        public string StockMethod { get; set; }
        public bool StockDeleted { get; set; }
    }

    public class StockItem
    {
        public Guid StockItemGuid { get; set; }

        public virtual Product StockItemProduct { get; set; }
        public virtual Stock StockItemStock { get; set; }
        public int StockItemMinimumQuantity { get; set; }
        public int StockItemMaximumQuantity { get; set; }
        public bool StockItemDeleted { get; set; }
    }

    public class StockTransaction
    {
        public Guid StockTransactionGuid { get; set; }

        public virtual StockItem StockTransactionItem { get; set; }
        public int StockTransactionQuantity { get; set; }
        public DateTime StockTransactionDate { get; set; }
        //public virtual ApplicationUser StockTransactionUser { get; set; }
        public bool StockTransactionDeleted { get; set; }
        public Order StockTransactionOrder { get; set; }
        public Supply StockTransactionSupply { get; set; }
    }
}