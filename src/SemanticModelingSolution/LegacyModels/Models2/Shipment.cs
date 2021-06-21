using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Models
{
    public class Shipment
    {
        public int ShipmentId { get; set; }
        public string ShipmentName { get; set; }
        public int SalesOrderId { get; set; }
        public DateTimeOffset ShipmentDate { get; set; }
        public int ShipmentTypeId { get; set; }
        public int WarehouseId { get; set; }
        public bool IsFullShipment { get; set; } = true;
    }
}
