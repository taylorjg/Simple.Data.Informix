using System;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    public class OrderDetailTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7);
        }

        [Test]
        public void TestOrderDetail()
        {
            var db = DatabaseHelper.Open();

            var order = db.Orders.FindByOrderDate(new DateTime(1994, 6, 18));
            Assert.IsNotNull(order);

            var item = order.Items.FirstOrDefault();
            Assert.IsNotNull(item);
            Assert.AreEqual(99.0, item.TotalPrice);
        }
    }
}
