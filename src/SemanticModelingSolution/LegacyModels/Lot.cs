using System;
using System.Collections.Generic;
using System.Text;

namespace LegacyModels
{
    public class Lot
    {
        public int Id { get; set; }
        public Article Article { get; set; }
        public string LotCode { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public decimal Quantity { get; set; }
        public bool IsClosed { get; set; }
        public string Notes { get; set; }

    }
}
