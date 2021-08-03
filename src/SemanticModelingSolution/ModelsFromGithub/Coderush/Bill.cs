using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Models
{
    public class Bill
    {
        public int BillId { get; set; }
        public string BillName { get; set; }
        public int GoodsReceivedNoteId { get; set; }
        public string VendorDONumber { get; set; }
        public string VendorInvoiceNumber { get; set; }
        public DateTimeOffset BillDate { get; set; }
        public DateTimeOffset BillDueDate { get; set; }
        public int BillTypeId { get; set; }
    }
}
