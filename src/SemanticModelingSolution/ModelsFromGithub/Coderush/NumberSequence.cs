using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Models
{
    public class NumberSequence
    {
        public int NumberSequenceId { get; set; }
        public string NumberSequenceName { get; set; }
        public string Module { get; set; }
        public string Prefix { get; set; }
        public int LastNumber { get; set; }
    }
}
