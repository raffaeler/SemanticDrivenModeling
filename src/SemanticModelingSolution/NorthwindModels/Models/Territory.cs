using System;
using System.Collections.Generic;

namespace NorthwindDataLayer.Models
{
    public partial class Territory
    {
        public Territory()
        {
            this.Employees = new List<Employee>();
        }

        public string TerritoryID { get; set; }
        public string TerritoryDescription { get; set; }
        public long RegionID { get; set; }
        public virtual Region Region { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public virtual ICollection<Employee> Employees { get; set; }
    }
}
