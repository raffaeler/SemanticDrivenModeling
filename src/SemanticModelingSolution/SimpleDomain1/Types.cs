using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDomain1
{
    public static class Types
    {
        public static Type[] All = new Type[]
        {
            typeof(SimpleDomain1.Order),
            typeof(SimpleDomain1.OrderItem),
            typeof(SimpleDomain1.Article),
            typeof(SimpleDomain1.Company),
            typeof(SimpleDomain1.Address),
        };
    }
}
