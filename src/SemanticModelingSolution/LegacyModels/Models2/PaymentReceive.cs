using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Models
{
    public class PaymentReceive
    {
        public int PaymentReceiveId { get; set; }
        public string PaymentReceiveName { get; set; }
        public int InvoiceId { get; set; }
        public DateTimeOffset PaymentDate { get; set; }
        public int PaymentTypeId { get; set; }
        public double PaymentAmount { get; set; }
        public bool IsFullPayment { get; set; } = true;
    }
}
