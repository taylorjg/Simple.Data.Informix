using System;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class TransactionTests_V7
    {
        protected string _connectionString = null;

        public TransactionTests_V7()
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
        public void TestCommit()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();

            int oldNumOrders = db.Orders.All().ToList().Count;
            int oldNumItems = db.Items.All().ToList().Count;

            using (var tx = db.BeginTransaction()) {
                try
                {
                    var order = tx.Orders.Insert(CustomerNum: 122, OrderDate: DateTime.Today);
                    tx.Items.Insert(OrderNum: order.OrderNum, ItemNum: 1, StockNum: 204, ManuCode: "KAR", Quantity: 3, TotalPrice: 33.30);
                    tx.Commit();
                }
                catch {
                    tx.Rollback();
                    throw;
                }
            }

            Assert.AreEqual(oldNumOrders + 1, db.Orders.All().ToList().Count);
            Assert.AreEqual(oldNumItems  + 1, db.Items.All().ToList().Count);
        }

        [Test]
        public void TestRollback()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();

            int oldNumOrders = db.Orders.All().ToList().Count;
            int oldNumItems = db.Items.All().ToList().Count;

            using (var tx = db.BeginTransaction()) {
                var order = tx.Orders.Insert(CustomerNum: 122, OrderDate: DateTime.Today);
                tx.Items.Insert(OrderNum: order.OrderNum, ItemNum: 1, StockNum: 204, ManuCode: "KAR", Quantity: 3, TotalPrice: 33.30);
                tx.Rollback();
            }

            Assert.AreEqual(oldNumOrders, db.Orders.All().ToList().Count);
            Assert.AreEqual(oldNumItems, db.Items.All().ToList().Count);
        }

        [Test]
        public void QueryInsideTransaction()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();

            using (var tx = db.BeginTransaction()) {
                tx.Customers.Insert(FName: "Geoff", LName: "Woade");
                Customer customer = tx.Customers.FindByLName("Woade");
                Assert.IsNotNull(customer);
                Assert.AreEqual("Geoff", customer.FName.Trim());
                Assert.AreEqual("Woade", customer.LName.Trim());
            }
        } 
    }

    internal class TransactionTests_V11 : TransactionTests_V7
    {
        public TransactionTests_V11()
        {
            _connectionString = Properties.Settings.Default.ConnectionString_V11;
        }
    }
}
