using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDomain2
{
    public static class Samples
    {
        public static IList<OnlineOrder> GetOnlineOrders()
        {
            var orders = new List<OnlineOrder>()
            {
                new OnlineOrder()
                {
                    Id = Guid.NewGuid(),
                    Description = "Order 101",
                    OrderLines = new List<OrderLine>()
                    {
                        new OrderLine()
                        {
                            Id = Guid.NewGuid(),
                            ProductName = "Pesto alla genovese",
                            ProductCode = "00AA0011",
                            Expiry = DateTimeOffset.Now,
                            
                            CustomerId = Guid.NewGuid(),
                            CustomerName = "Helios",
                            Street = "Piazza Giuseppe Garibaldi",
                            City = "Monterosso",
                            State = "Liguria",
                            Country = "Italy",

                            Net = 10,
                            Payment = 9.40,
                        },

                        new OrderLine()
                        {
                            Id = Guid.NewGuid(),
                            ProductName = "Basilico",
                            ProductCode = "00AA0012",
                            Expiry = DateTimeOffset.Now,

                            CustomerId = Guid.NewGuid(),
                            CustomerName = "Astra",
                            Street = "Salita dei Capuccini",
                            City = "Monterosso",
                            State = "Liguria",
                            Country = "Italy",

                            Net = 40.8,
                            Payment = 2.20,
                        },
                    },
                },

                new OnlineOrder()
                {
                    Id = Guid.NewGuid(),
                    Description = "Order 102",
                    OrderLines = new List<OrderLine>()
                    {
                        new OrderLine()
                        {
                            Id = Guid.NewGuid(),
                            ProductName = "Acciuga fresca",
                            ProductCode = "00AB0011",
                            //Expiry = DateTimeOffset.Now,  // default value!

                            CustomerId = Guid.NewGuid(),
                            CustomerName = "Mare e salute",
                            Street = "via Fegina",
                            City = "Monterosso",
                            State = "Liguria",
                            Country = "Italy",

                            Net = 10,
                            Payment = 6.5,
                        },
                        new OrderLine()
                        {
                            Id = Guid.NewGuid(),
                            ProductName = "Orata fresca",
                            ProductCode = "00AB0012",
                            Expiry = new DateTimeOffset(2022, 02, 26, 2, 3, 4, TimeSpan.FromHours(1)),

                            CustomerId = Guid.NewGuid(),
                            CustomerName = "Mare e salute",
                            Street = "via Fegina",
                            City = "Monterosso",
                            State = "Liguria",
                            Country = "Italy",

                            Net = 10,
                            Payment = 9.40,
                        },
                    },
                },


            };

            return orders;
        }
    }

    public class OnlineOrder
    {
        public Guid Id { get; set; }
        public string Description   { get; set; }
        public List<OrderLine> OrderLines { get; set; }
    }

    public class OrderLine
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public DateTimeOffset Expiry { get; set; }


        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

        public double Net { get; set; }
        public double Payment { get; set; }
    }

}
