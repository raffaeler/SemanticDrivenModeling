using System;
using System.Linq;
using System.Text.Json;

using SemanticLibrary;

namespace DeserializationConsole
{
    public class Program
    {
        static Type[] _domainTypes1 = new Type[]
        {
            typeof(ERP_Model.Models.Supply           ),
            typeof(ERP_Model.Models.SupplyItem       ),
            typeof(ERP_Model.Models.Address          ),
            typeof(ERP_Model.Models.Customer         ),
            typeof(ERP_Model.Models.Delivery         ),
            typeof(ERP_Model.Models.DeliveryItem     ),
            typeof(ERP_Model.Models.GoodsReceipt     ),
            typeof(ERP_Model.Models.GoodsReceiptItem ),
            typeof(ERP_Model.Models.Order            ),
            typeof(ERP_Model.Models.OrderItem        ),
            typeof(ERP_Model.Models.Product          ),
            typeof(ERP_Model.Models.Stock            ),
            typeof(ERP_Model.Models.StockItem        ),
            typeof(ERP_Model.Models.Supplier         ),
            typeof(ERP_Model.Models.Supply           ),
            typeof(ERP_Model.Models.SupplyItem       ),
        };

        static Type[] _domainTypes2 = new Type[]
        {
            typeof(coderush.Models.Bill               ),
            typeof(coderush.Models.BillType           ),
            typeof(coderush.Models.Branch             ),
            typeof(coderush.Models.CashBank           ),
            typeof(coderush.Models.Currency           ),
            typeof(coderush.Models.Customer           ),
            typeof(coderush.Models.CustomerType       ),
            typeof(coderush.Models.GoodsReceivedNote  ),
            typeof(coderush.Models.Invoice            ),
            typeof(coderush.Models.InvoiceType        ),
            typeof(coderush.Models.NumberSequence     ),
            typeof(coderush.Models.PaymentReceive     ),
            typeof(coderush.Models.PaymentType        ),
            typeof(coderush.Models.PaymentVoucher     ),
            typeof(coderush.Models.Product            ),
            typeof(coderush.Models.ProductType        ),
            typeof(coderush.Models.PurchaseOrder      ),
            typeof(coderush.Models.PurchaseOrderLine  ),
            typeof(coderush.Models.PurchaseType       ),
            typeof(coderush.Models.SalesOrder         ),
            typeof(coderush.Models.SalesOrderLine     ),
            typeof(coderush.Models.SalesType          ),
            typeof(coderush.Models.Shipment           ),
            typeof(coderush.Models.ShipmentType       ),
            typeof(coderush.Models.UnitOfMeasure      ),
            typeof(coderush.Models.Vendor             ),
            typeof(coderush.Models.VendorType         ),
            typeof(coderush.Models.Warehouse          ),
        };

        static Type[] _domainTypesNW = new Type[]
        {
            typeof(NorthwindDataLayer.Models.Alphabetical_list_of_product        ),
            typeof(NorthwindDataLayer.Models.Category                            ),
            typeof(NorthwindDataLayer.Models.Category_Sales_for_1997             ),
            typeof(NorthwindDataLayer.Models.Current_Product_List                ),
            typeof(NorthwindDataLayer.Models.Customer                            ),
            typeof(NorthwindDataLayer.Models.Customer_and_Suppliers_by_City      ),
            typeof(NorthwindDataLayer.Models.CustomerDemographic                 ),
            typeof(NorthwindDataLayer.Models.Employee                            ),
            typeof(NorthwindDataLayer.Models.Invoice                             ),
            typeof(NorthwindDataLayer.Models.Order                               ),
            typeof(NorthwindDataLayer.Models.Order_Detail                        ),
            typeof(NorthwindDataLayer.Models.Order_Details_Extended              ),
            typeof(NorthwindDataLayer.Models.Order_Subtotal                      ),
            typeof(NorthwindDataLayer.Models.Orders_Qry                          ),
            typeof(NorthwindDataLayer.Models.Product                             ),
            typeof(NorthwindDataLayer.Models.Product_Sales_for_1997              ),
            typeof(NorthwindDataLayer.Models.Products_Above_Average_Price        ),
            typeof(NorthwindDataLayer.Models.Products_by_Category                ),
            typeof(NorthwindDataLayer.Models.Region                              ),
            typeof(NorthwindDataLayer.Models.Sales_by_Category                   ),
            typeof(NorthwindDataLayer.Models.Sales_Totals_by_Amount              ),
            typeof(NorthwindDataLayer.Models.Shipper                             ),
            typeof(NorthwindDataLayer.Models.Summary_of_Sales_by_Quarter         ),
            typeof(NorthwindDataLayer.Models.Summary_of_Sales_by_Year            ),
            typeof(NorthwindDataLayer.Models.Supplier                            ),
            typeof(NorthwindDataLayer.Models.Territory                           ),
        };

        static Type _typeVendor = typeof(coderush.Models.Vendor);
        static Type _typeSupplier = typeof(ERP_Model.Models.Supplier);
        static Type _typeVendorArray = typeof(coderush.Models.Vendor[]);
        static Type _typeSupplierArray = typeof(ERP_Model.Models.Supplier[]);

        JsonSerializerOptions _settingsVanilla = new JsonSerializerOptions()
        {
            WriteIndented = true,
        };

        //JsonSerializerOptions _settingsCustom = new JsonSerializerOptions()
        //{
        //    WriteIndented = true,
        //    Converters =
        //    {
        //        new CodeGenerationLibrary.Serialization.TesterConverterFactory(),
        //    },
        //};

        static void Main(string[] args)
        {
            new Program().Start();
        }

        private void Start()
        {
            var mapping = GetMapping();
            var settings = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters =
                {
                    new CodeGenerationLibrary.Serialization.TesterConverterFactory(mapping),
                },
            };

            // vendor => supplier
            var v1 = GetVendors1();
            var jv1 = GetJson(v1);
            var v2 = JsonSerializer.Deserialize(jv1, _typeVendorArray);
            var vd1 = FromJson(jv1, _typeSupplierArray, settings);


            var s1 = GetSupplier1();
            var js1 = GetJson(s1);
            var sd1 = FromJson(js1, _typeVendorArray, settings);
        }

        public coderush.Models.Vendor[] GetVendors1() => new coderush.Models.Vendor[]
        {
            new ()
            {
                VendorId = 1,
                Address = "Address1",
                City = "City1",
                ContactPerson = "ContactPerson1",
                Email = "Email1",
                Phone = "Phone1",
                State = "State1",
                VendorName = "VendorName1",
                VendorTypeId = 991,
                ZipCode = "ZipCode1",
            },
            new ()
            {
                VendorId = 2,
                Address = "Address2",
                City = "City2",
                ContactPerson = "ContactPerson2",
                Email = "Email2",
                Phone = "Phone2",
                State = "State2",
                VendorName = "VendorName2",
                VendorTypeId = 992,
                ZipCode = "ZipCode2",
            },
        };

        public ERP_Model.Models.Supplier[] GetSupplier1() => new ERP_Model.Models.Supplier[]
        {
            new ERP_Model.Models.Supplier
            {
                SupplierGuid= Guid.NewGuid(),
                SupplierCompany = "Company1",
                SupplierAddress= new ERP_Model.Models.Address()
                {
                    AddressGuid = Guid.NewGuid(),
                    AddressCity = "AddressCity1",
                    AddressCompany = "AddressCompany1",
                    AddressCountry = "AddressCountry1",
                    AddressDeleted = false,
                    AddressDescription = "AddressDescription1",
                    AddressEmail = "AddressEmail1",
                    AddressForName = "AddressForName1",
                    AddressLastName = "AddressLastName1",
                    AddressPhone = 1,
                    AddressStreet = "AddressStreet1",
                    AddressZipCode = "AddressZipCode1",
                },
                SupplierDeleted = false,
                SupplierForName= "ForeName1",
                SupplierLastName= "LastName1",
            },
            new ERP_Model.Models.Supplier
            {
                SupplierGuid= Guid.NewGuid(),
                SupplierCompany = "Company2",
                SupplierAddress= new ERP_Model.Models.Address()
                {
                    AddressGuid = Guid.NewGuid(),
                    AddressCity = "AddressCity2",
                    AddressCompany = "AddressCompany2",
                    AddressCountry = "AddressCountry2",
                    AddressDeleted = false,
                    AddressDescription = "AddressDescription2",
                    AddressEmail = "AddressEmail2",
                    AddressForName = "AddressForName2",
                    AddressLastName = "AddressLastName2",
                    AddressPhone = 2,
                    AddressStreet = "AddressStreet2",
                    AddressZipCode = "AddressZipCode2",
                },
                SupplierDeleted = false,
                SupplierForName= "ForeName2",
                SupplierLastName= "LastName2",
            },
        };

        public string GetJson<T>(T[] item) => JsonSerializer.Serialize(item, _settingsVanilla);

        public object FromJson(string json, Type type, JsonSerializerOptions options) => JsonSerializer.Deserialize(json, type, options);

        public ScoredTypeMapping GetMapping()
        {
            var domain = new GeneratedCode.Domain();
            var visitor1 = new DomainTypesGraphVisitor(domain, _domainTypes1);
            var modelsDomain1 = visitor1.Visit(null, null, null);


            var visitor2 = new DomainTypesGraphVisitor(domain, _domainTypes2);
            var models2 = visitor2.Visit(null, null, new[] { typeof(coderush.Models.Vendor) });
            var model2 = models2.Single();

            var matcher = new ConceptMatchingRule(true);
            matcher.ComputeMappings(model2, modelsDomain1);
            return matcher.CandidateTypes.First();
        }
    }
}
