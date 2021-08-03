using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Models
{
    public class Warehouse
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string Description { get; set; }
        public int BranchId { get; set; }
    }
}
