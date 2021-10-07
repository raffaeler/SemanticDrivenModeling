using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using SimpleDomain1;

namespace ApiServer
{
    public class RepositoryService
    {
        public IEnumerable<Order> GetOrders() => Samples.GetOrders();
    }
}
