using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public string InvoiceName { get; set; }
        public int ShipmentId { get; set; }
        public DateTimeOffset InvoiceDate { get; set; }
        public DateTimeOffset InvoiceDueDate { get; set; }
        public int InvoiceTypeId { get; set; }
    }
}
