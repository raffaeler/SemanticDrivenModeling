using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDomain1
{
    public static class Samples
    {
        public static IList<Order> GetOrders()
        {
            var orders = new List<Order>()
            {
                new Order
                {
                    Id = Guid.NewGuid(),
                    Reference = "Order1",
                    OrderItems = new List<OrderItem>()
                    {
                        new OrderItem()
                        {
                            Id = Guid.NewGuid(),
                            Article = new Article()
                            {
                                Id = Guid.NewGuid(),
                                Name = "Mayonnaise",
                                Description = "Regular mayonnaise",
                                ExpirationDate = DateTime.Now,
                            },
                            Customer = new Company()
                            {
                                Id = Guid.NewGuid(),
                                Name = "Acme",
                                Description = "Some description",
                                Address = new Address()
                                {
                                    Id = Guid.NewGuid(),
                                    Street = "via Belvedere",
                                    State = "Liguria",
                                    City = "Santa Margherita Ligure",
                                    Country = "Italy",
                                },
                            },
                            Price = 6.5m,
                            Discount = 0.65m,
                            Quantity = 10,
                        },

                        new OrderItem()
                        {
                            Id = Guid.NewGuid(),
                            Article = new Article()
                            {
                                Id = Guid.NewGuid(),
                                Name = "Ketchup",
                                Description = "Tomato Ketchup",
                                ExpirationDate = DateTime.Now,
                            },
                            Customer = new Company()
                            {
                                Id = Guid.NewGuid(),
                                Name = "Iris",
                                Description = "Some other description",
                                Address = new Address()
                                {
                                    Id = Guid.NewGuid(),
                                    Street = "Salita Montebello",
                                    State = "Liguria",
                                    City = "Santa Margherita Ligure",
                                    Country = "Italy",
                                },
                            },
                            Price = 7.5m,
                            Discount = 0.75m,
                            Quantity = 10,
                        },
                    },
                },

                new Order
                {
                    Id = Guid.NewGuid(),
                    Reference = "Order2",
                    OrderItems = new List<OrderItem>()
                    {
                        new OrderItem()
                        {
                            Id = Guid.NewGuid(),
                            Article = new Article()
                            {
                                Id = Guid.NewGuid(),
                                Name = "Tonno in scatola",
                                Description = "Tuna",
                                ExpirationDate = DateTime.Now,
                            },
                            Customer = new Company()
                            {
                                Id = Guid.NewGuid(),
                                Name = "Futura",
                                Description = "Some description",
                                Address = new Address()
                                {
                                    Id = Guid.NewGuid(),
                                    Street = "Piazza del Mare",
                                    State = "Liguria",
                                    City = "Varigotti",
                                    Country = "Italy",
                                },
                            },
                            Price = 4.3m,
                            Discount = 0.43m,
                            Quantity = 10,
                        },

                        new OrderItem()
                        {
                            Id = Guid.NewGuid(),
                            Article = new Article()
                            {
                                Id = Guid.NewGuid(),
                                Name = "Salmon",
                                Description = "Smoked salmon",
                                ExpirationDate = DateTime.Now,
                            },
                            Customer = new Company()
                            {
                                Id = Guid.NewGuid(),
                                Name = "Alpha distribuzione",
                                Description = "Some other description",
                                Address = new Address()
                                {
                                    Id = Guid.NewGuid(),
                                    Street = "Strada degli Ulivi",
                                    State = "Liguria",
                                    City = "Varigotti",
                                    Country = "Italy",
                                },
                            },
                            Price = 10.5m,
                            Discount = 0.1m,
                            Quantity = 10,
                        },
                    },
                },

            };

            return orders;
        }
    }

    public class Order
    {
        public Guid Id { get; set; }
        public string Reference { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }

    public class OrderItem
    {
        public Guid Id { get; set; }
        public Article Article { get; set; }
        public Company Customer { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
    }


    public class Article
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

    public class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Address Address { get; set; }
    }

    public class Address
    {
        public Guid Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }
}
