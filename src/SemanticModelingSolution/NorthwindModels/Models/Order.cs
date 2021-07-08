using System;
using System.Collections.Generic;

using NorthwindDataLayer.Helpers;

namespace NorthwindDataLayer.Models
{
    public partial class Order
    {
        public Order()
        {
            this.Order_Details = new List<Order_Detail>();
        }

        public long OrderID { get; set; }
        //[System.ComponentModel.DataAnnotations.Schema.ForeignKey("Customer")]
        public string CustomerID { get; set; }
        public Nullable<long> EmployeeID { get; set; }
        public Nullable<System.DateTimeOffset> OrderDate
        {
            get { return OrderDateInternal.Convert(); }
            set { OrderDateInternal = value.Convert(); }
        }
        private Nullable<System.DateTime> OrderDateInternal { get; set; }  // fix for oData v4


        public Nullable<System.DateTimeOffset> RequiredDate
        {
            get { return RequiredDateInternal.Convert(); }
            set { RequiredDateInternal = value.Convert(); }
        }

        private Nullable<System.DateTime> RequiredDateInternal { get; set; }  // fix for oData v4

        public Nullable<System.DateTimeOffset> ShippedDate
        {
            get { return ShippedDateInternal.Convert(); }
            set { ShippedDateInternal = value.Convert(); }
        }

        private Nullable<System.DateTime> ShippedDateInternal { get; set; }  // fix for oData v4

        public Nullable<long> ShipVia { get; set; }
        public Nullable<decimal> Freight { get; set; }
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipRegion { get; set; }
        public string ShipPostalCode { get; set; }
        public string ShipCountry { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual ICollection<Order_Detail> Order_Details { get; set; }
        public virtual Shipper Shipper { get; set; }


        internal static class RemapExpressions
        {
            public static readonly System.Linq.Expressions.Expression<Func<Order, DateTime?>>
                OrderDate = c => c.OrderDateInternal;

            public static readonly System.Linq.Expressions.Expression<Func<Order, DateTime?>>
                RequiredDate = c => c.RequiredDateInternal;

            public static readonly System.Linq.Expressions.Expression<Func<Order, DateTime?>>
                ShippedDate = c => c.ShippedDateInternal;
        }
    }
}
