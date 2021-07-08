using System;
using System.Collections.Generic;

namespace NorthwindDataLayer.Models
{
    public partial class Region
    {
        public Region()
        {
            this.Territories = new List<Territory>();
        }

        public long RegionID { get; set; }
        public string RegionDescription { get; set; }
        public virtual ICollection<Territory> Territories { get; set; }
    }
}
