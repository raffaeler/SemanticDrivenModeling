using System;
using System.Collections.Generic;

using NorthwindDataLayer.Helpers;

namespace NorthwindDataLayer.Models
{
    public partial class Employee
    {
        public Employee()
        {
            this.Employees1 = new List<Employee>();
            this.Orders = new List<Order>();
            this.Territories = new List<Territory>();
        }

        public long EmployeeID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Title { get; set; }
        public string TitleOfCourtesy { get; set; }
        public Nullable<System.DateTimeOffset> BirthDate
        {
            get { return BirthDateInternal.Convert(); }
            set { BirthDateInternal = value.Convert(); }
        }

        private Nullable<System.DateTime> BirthDateInternal { get; set; }   // fix for oData v4
        public Nullable<System.DateTimeOffset> HireDate
        {
            get { return HireDateInternal.Convert(); }
            set { HireDateInternal = value.Convert(); }
        }
        private Nullable<System.DateTime> HireDateInternal { get; set; }  // fix for oData v4
        public string Address { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string HomePhone { get; set; }
        public string Extension { get; set; }
        public byte[] Photo { get; set; }
        public string Notes { get; set; }
        public Nullable<long> ReportsTo { get; set; }
        public string PhotoPath { get; set; }
        public virtual ICollection<Employee> Employees1 { get; set; }
        public virtual Employee Employee1 { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Territory> Territories { get; set; }


        internal static class RemapExpressions
        {
            public static readonly System.Linq.Expressions.Expression<Func<Employee, DateTime?>>
                BirthDate = c => c.BirthDateInternal;

            public static readonly System.Linq.Expressions.Expression<Func<Employee, DateTime?>>
                HireDate = c => c.HireDateInternal;

        }
    }
}
