# Tests

## Order to OnlineOrder [Version 1]


Source type: SimpleDomain1.Order => Candidate type: SimpleDomain2.OnlineOrder (Type score: 75) Prop score: 3933
Mappings:
Order.Id => OnlineOrder.Id [1234]
Order.OrderItems.$.Article.Description => OnlineOrder.Description [1234]
Order.Id => OnlineOrder.OrderLines.$.Id [100]
Order.OrderItems.$.Article.Name => OnlineOrder.OrderLines.$.ProductName [150]
Order.OrderItems.$.Article.Name => OnlineOrder.OrderLines.$.ProductCode [150]
Order.OrderItems.$.Article.ExpirationDate => OnlineOrder.OrderLines.$.Expiry [100]
Order.Id => OnlineOrder.OrderLines.$.CustomerId [75]
Order.OrderItems.$.Customer.Name => OnlineOrder.OrderLines.$.CustomerName [150]
Order.OrderItems.$.Customer.Address.Street => OnlineOrder.OrderLines.$.Street [160]
Order.OrderItems.$.Customer.Address.City => OnlineOrder.OrderLines.$.City [160]
Order.OrderItems.$.Customer.Address.State => OnlineOrder.OrderLines.$.State [160]
Order.OrderItems.$.Customer.Address.Country => OnlineOrder.OrderLines.$.Country [160]
Order.OrderItems.$.Price => OnlineOrder.OrderLines.$.Payment [100]


Source type: SimpleDomain1.Order => Candidate type: SimpleDomain2.OrderLine (Type score: 75) Prop score: 13574
Mappings:
Order.Id => OrderLine.Id [1234]
Order.OrderItems.$.Article.Name => OrderLine.ProductName [1234]
Order.OrderItems.$.Article.Name => OrderLine.ProductCode [1234]
Order.OrderItems.$.Article.ExpirationDate => OrderLine.Expiry [1234]
Order.Id => OrderLine.CustomerId [1234]
Order.OrderItems.$.Article.Name => OrderLine.CustomerName [1234]
Order.OrderItems.$.Customer.Address.Street => OrderLine.Street [1234]
Order.OrderItems.$.Customer.Address.City => OrderLine.City [1234]
Order.OrderItems.$.Customer.Address.State => OrderLine.State [1234]
Order.OrderItems.$.Customer.Address.Country => OrderLine.Country [1234]
Order.OrderItems.$.Price => OrderLine.Payment [1234]


## Order to OnlineOrder [Version 2]

Source type: SimpleDomain1.Order => Candidate type: SimpleDomain2.OnlineOrder (Type score: 75) Prop score: 2132
Mappings:
Order.Id => OnlineOrder.Id [1234]
Order.OrderItems.$.Article.Description => OnlineOrder.Description [70]
Order.OrderItems.$.Id => OnlineOrder.OrderLines.$.Id [100]
Order.OrderItems.$.Article.Name => OnlineOrder.OrderLines.$.ProductName [52]
Order.OrderItems.$.Article.Name => OnlineOrder.OrderLines.$.ProductCode [52]
Order.Id => OnlineOrder.OrderLines.$.CustomerId [52]
**Order.OrderItems.$.Article.Name => OnlineOrder.OrderLines.$.CustomerName [52]**
Order.OrderItems.$.Customer.Address.Street => OnlineOrder.OrderLines.$.Street [130]
Order.OrderItems.$.Customer.Address.City => OnlineOrder.OrderLines.$.City [130]
Order.OrderItems.$.Customer.Address.State => OnlineOrder.OrderLines.$.State [130]
Order.OrderItems.$.Customer.Address.Country => OnlineOrder.OrderLines.$.Country [130]


Source type: SimpleDomain1.Order => Candidate type: SimpleDomain2.OrderLine (Type score: 75) Prop score: 3144
Mappings:
Order.Id => OrderLine.Id [1234]
Order.OrderItems.$.Article.Name => OrderLine.ProductName [52]
Order.OrderItems.$.Article.Name => OrderLine.ProductCode [52]
Order.Id => OrderLine.CustomerId [1234]
Order.OrderItems.$.Article.Name => OrderLine.CustomerName [52]
Order.OrderItems.$.Customer.Address.Street => OrderLine.Street [130]
Order.OrderItems.$.Customer.Address.City => OrderLine.City [130]
Order.OrderItems.$.Customer.Address.State => OrderLine.State [130]
Order.OrderItems.$.Customer.Address.Country => OrderLine.Country [130]





## OnlineOrder to Order [Version 1]



Source type: SimpleDomain2.OnlineOrder => Candidate type: SimpleDomain1.Order (Type score: 75) Prop score: 3099
Mappings:
OnlineOrder.Id => Order.Id [1234]
OnlineOrder.Id => Order.OrderItems.$.Id [130]
OnlineOrder.Id => Order.OrderItems.$.Article.Id [100]
OnlineOrder.OrderLines.$.ProductName => Order.OrderItems.$.Article.Name [180]
OnlineOrder.Description => Order.OrderItems.$.Article.Description [100]
OnlineOrder.OrderLines.$.Expiry => Order.OrderItems.$.Article.ExpirationDate [100]
OnlineOrder.OrderLines.$.CustomerId => Order.OrderItems.$.Customer.Id [105]
OnlineOrder.OrderLines.$.CustomerName => Order.OrderItems.$.Customer.Name [180]
OnlineOrder.Description => Order.OrderItems.$.Customer.Description [100]
OnlineOrder.Id => Order.OrderItems.$.Customer.Address.Id [100]
OnlineOrder.OrderLines.$.Street => Order.OrderItems.$.Customer.Address.Street [160]
OnlineOrder.OrderLines.$.City => Order.OrderItems.$.Customer.Address.City [160]
OnlineOrder.OrderLines.$.State => Order.OrderItems.$.Customer.Address.State [160]
OnlineOrder.OrderLines.$.Country => Order.OrderItems.$.Customer.Address.Country [160]
OnlineOrder.OrderLines.$.Payment => Order.OrderItems.$.Price [130]
