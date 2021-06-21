using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Models
{
    public class GoodsReceivedNote
    {
        public int GoodsReceivedNoteId { get; set; }
        public string GoodsReceivedNoteName { get; set; }
        public int PurchaseOrderId { get; set; }
        public DateTimeOffset GRNDate { get; set; }
        public string VendorDONumber { get; set; }
        public string VendorInvoiceNumber { get; set; }
        public int WarehouseId { get; set; }
        public bool IsFullReceive { get; set; } = true;
    }
}
