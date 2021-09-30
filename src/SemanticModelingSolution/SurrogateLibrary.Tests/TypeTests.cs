using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SurrogateLibrary.Tests
{
    [TestClass]
    public class TypeTests
    {
        public class TA { public TA A { get; set; } public TB B { get; set; } public int X { get; set; } }
        public class TB { public TA A { get; set; } public TB B { get; set; } public int Y { get; set; } }
        public record Info(int Id, string Name);


        [TestMethod]
        public void BasicTypes()
        {
            TypeSystem ts = new TypeSystem();

            Assert.IsNotNull(ts.Types);
            Assert.IsNotNull(ts.TypesByFullName);
            Assert.IsNotNull(ts.Properties);
            Assert.IsNotNull(ts.DefaultBindings);

            Assert.IsTrue(ts.Types.Count > 10);        // all the basic types
            Assert.IsTrue(ts.Types.All(t => t.Key < KnownConstants.MaxIndexForBasicTypes));
        }


        [TestMethod]
        public void UpdateCacheTest()
        {
            TypeSystem ts = new TypeSystem();
            var orderType1 = ts.GetOrCreate(typeof(SimpleDomain1.Order));
            var orderType2 = ts.GetOrCreate(typeof(SimpleDomain1.Order));
            Assert.AreEqual(orderType1, orderType2);
            Assert.IsTrue(orderType1.PropertyIndexes.Count > 0);
            Assert.IsTrue(orderType1.Properties.Count == 0);

            Assert.IsNull(orderType1.InnerType1);
            Assert.IsNull(orderType1.InnerType2);

            ts.UpdateCache();
            Assert.AreEqual(orderType1, orderType2);

            Assert.AreEqual(orderType1.PropertyIndexes.Count, orderType1.Properties.Count);

            Assert.IsNull(orderType1.InnerType1);
            Assert.IsNull(orderType1.InnerType2);

            var dict = ts.GetOrCreate(typeof(Dictionary<int, string>));
            ts.UpdateCache();
            Assert.IsNotNull(dict.InnerType1);
            Assert.IsNotNull(dict.InnerType2);
        }

        [TestMethod]
        public void NoCircularReference()
        {
            var ts = new TypeSystem();
            var ta = ts.GetOrCreate(typeof(TA));
            ts.UpdateCache();
        }

        [TestMethod]
        public void AddingTypes()
        {
            var t1 = typeof(List<string>);
            var t2 = typeof(IDictionary<string, Action<int>>);
            var t3 = typeof(List<>);
            var t4 = typeof(IDictionary<,>);
            var t5 = typeof(string);
            var t6 = typeof(TypeTests);

            var ts = new TypeSystem();
            var s1 = ts.GetOrCreate(t1);
            var s2 = ts.GetOrCreate(t2);
            var s3 = ts.GetOrCreate(t3);
            var s4 = ts.GetOrCreate(t4);
            var s5 = ts.GetOrCreate(t5);
            var s6 = ts.GetOrCreate(t6);
            var s7 = ts.GetOrCreate(typeof(TA));

        }

        [TestMethod]
        public void Serialization1()
        {
            var ts = new TypeSystem();
            var ta = ts.GetOrCreate(typeof(TA));

            var json = JsonSerializer.Serialize(ta);
            var taClone = JsonSerializer.Deserialize<SurrogateType<VoidType>>(json);

            Assert.IsTrue(ta == taClone);
            Assert.IsTrue(ta.Equals(taClone));
            Assert.IsTrue(taClone.Equals(ta));
        }

        [TestMethod]
        public void Serialization2()
        {
            var ts = new TypeSystem<Info>();
            var order = ts.GetOrCreate(typeof(SimpleDomain1.Order));
            order.SetInfo(new(1, "Raf"));

            var json = JsonSerializer.Serialize(order);
            var orderClone = JsonSerializer.Deserialize<SurrogateType<Info>>(json);

            Assert.IsTrue(order == orderClone);
            Assert.IsTrue(order.Equals(orderClone));
            Assert.IsTrue(orderClone.Equals(order));

            ts.UpdateCache();
            orderClone.UpdateCache(ts);

            Assert.IsTrue(order == orderClone);
            Assert.IsTrue(order.Equals(orderClone));
            Assert.IsTrue(orderClone.Equals(order));
        }

        [TestMethod]
        public void CollectionsAndCollected()
        {
            var ts = new TypeSystem<Info>();
            var order = ts.GetOrCreate(typeof(SimpleDomain1.Order));
            ts.UpdateCache();

            var orderItemsProperty = order.Properties.Values.Where(o => o.Name == "OrderItems").Single();
            var collectionType = orderItemsProperty.PropertyType;
            var collectedType = collectionType.InnerType1;
            var coreType = collectionType.GetCoreType();

            var orderItemsKind = orderItemsProperty.GetKind();
            var orderItemsIsCollection = collectionType.IsCollection();
            var collectedIsCollection = collectedType.IsCollection();
            var coreTypeIsCollection = coreType.IsCollection();

            Assert.IsTrue(orderItemsKind == PropertyKind.OneToMany);
            Assert.IsTrue(orderItemsIsCollection);
            Assert.IsFalse(collectedIsCollection);
            Assert.IsFalse(coreTypeIsCollection);
        }

        [TestMethod]
        public void Navigation()
        {
            var ts = new TypeSystem<Info>();
            var order = ts.GetOrCreate(typeof(SimpleDomain1.Order));
            ts.UpdateCache();

            var orderItemsProperty = order.Properties.Values.Where(o => o.Name == "OrderItems").Single();
            var collectionType = orderItemsProperty.PropertyType;
            var collectedType = collectionType.InnerType1;
            var coreType = collectionType.GetCoreType();

            var orderItemsKind = orderItemsProperty.GetKind();
            var orderItemsIsCollection = collectionType.IsCollection();
            var collectedIsCollection = collectedType.IsCollection();
            var coreTypeIsCollection = coreType.IsCollection();

            var navigations = GraphFlattener.FlattenHierarchy(order, ts);
            NavigationSegment<Info> street = navigations
                .Select(n => n.GetLeaf())
                .Where(n => n.Property != null && n.Property.Name == "Street")
                .Single();

            Assert.IsFalse(street.IsOneToMany);
            var address = street.Previous;
            Assert.IsTrue(address.IsOneToOne);
            var customer = address.Previous;
            Assert.IsTrue(customer.IsOneToOne);
            var orderItem = customer.Previous;
            Assert.IsTrue(orderItem.IsCollectedItem);
            var orderItems = orderItem.Previous;
            Assert.IsTrue(orderItems.IsOneToMany);
            var order2 = orderItems.Previous;
            Assert.IsTrue(order2.IsRoot);
        }



    }
}
