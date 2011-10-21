using System;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    public class OrderDetailTests_V7
    {
        protected string _connectionString = null;

        public OrderDetailTests_V7()
        {
            _connectionString = Properties.Settings.Default.ConnectionString_V7;
        }

        protected dynamic OpenDatabase()
        {
            return DatabaseHelper.Open(_connectionString);
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            TraceHelper.BeginTrace();
            DatabaseHelper.Reset(_connectionString);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            TraceHelper.EndTrace();
        }

        [Test]
        public void TestOrderDetail()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();

            var order = db.Orders.FindByOrderDate(new DateTime(1994, 6, 18));
            Assert.IsNotNull(order);

            var item = order.Items.FirstOrDefault();
            Assert.IsNotNull(item);
            Assert.AreEqual(99.0, item.TotalPrice);
        }
    }

    internal class OrderDetailTests_V11 : OrderDetailTests_V7
    {
        public OrderDetailTests_V11()
        {
            _connectionString = Properties.Settings.Default.ConnectionString_V11;
        }
    }
}
