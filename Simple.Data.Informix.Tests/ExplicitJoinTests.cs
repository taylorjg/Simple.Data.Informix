using System;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class ExplicitJoinTests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            DatabaseHelper.Reset(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7);
        }

        [Test]
        public void TestExplicitJoinNamedParametersForm()
        {
            var db = DatabaseHelper.Open();

            var rows = db.Customers.Query()
                .Join(db.Orders, CustomerNum: db.Customers.CustomerNum)
                .Where(db.Orders.OrderDate == new DateTime(1994, 6, 17)).ToList();

            Assert.AreEqual(1, rows.Count);
            var row = rows[0];
            Assert.IsNotNull(row);
            Assert.AreEqual("Alfred", row.FName.Trim());
        }

        [Test]
        public void TestExplicitJoinOnXxxForm()
        {
            var db = DatabaseHelper.Open();

            var rows = db.Customers.Query()
                .Join(db.Orders).On(db.Customers.CustomerNum == db.Orders.CustomerNum)
                .Where(db.Orders.OrderDate == new DateTime(1994, 6, 17)).ToList();

            Assert.AreEqual(1, rows.Count);
            var row = rows[0];
            Assert.IsNotNull(row);
            Assert.AreEqual("Alfred", row.FName.Trim());
        }
    }
}
