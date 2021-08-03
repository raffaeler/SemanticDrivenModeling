using System;
using System.Collections.Generic;
using System.Text;

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

namespace SimpleDomain2
{
    public static class Types
    {
        public static Type[] All = new Type[]
        {
            typeof(SimpleDomain2.OnlineOrder),
            typeof(SimpleDomain2.OrderLine),
        };
    }
}
