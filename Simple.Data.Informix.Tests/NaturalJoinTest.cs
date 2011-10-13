using System;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class NaturalJoinTest
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            DatabaseHelper.Reset(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7);
        }

        [Test]
        public void CustomerDotOrdersDotOrderDateShouldReturnOneRow()
        {
            var db = DatabaseHelper.Open();
            var row = db.Customers.Find(db.Customers.Orders.OrderDate == new DateTime(1994, 6, 17));
            Assert.IsNotNull(row);
            Assert.AreEqual("Alfred", row.FName.Trim());
        }

        //[Test]
        //public void CustomerDotOrdersDotOrderItemsDotItemDotNameShouldReturnOneRow()
        //{
        //    var db = DatabaseHelper.Open();
        //    var customer = db.Customers.Find(db.Customers.Orders.OrderItems.Item.Name == "Widget");
        //    Assert.IsNotNull(customer);
        //    Assert.AreEqual("Test", customer.Name);
        //    foreach (var order in customer.Orders) {
        //        Assert.AreEqual(1, order.OrderId);
        //    }
        //}
    }
}
