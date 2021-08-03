using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Models
{
    public class PaymentVoucher
    {
        public int PaymentvoucherId { get; set; }
        public string PaymentVoucherName { get; set; }
        public int BillId { get; set; }
        public DateTimeOffset PaymentDate { get; set; }
        public int PaymentTypeId { get; set; }
        public double PaymentAmount { get; set; }
        public int CashBankId { get; set; }
        public bool IsFullPayment { get; set; } = true;
    }
}
