using System;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class ExplicitJoinTests_V7
    {
        protected string _connectionString = null;

        public ExplicitJoinTests_V7()
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
        public void TestExplicitJoinNamedParametersForm()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();

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
            TraceHelper.TraceTestName();
            var db = OpenDatabase();

            var rows = db.Customers.Query()
                .Join(db.Orders).On(db.Customers.CustomerNum == db.Orders.CustomerNum)
                .Where(db.Orders.OrderDate == new DateTime(1994, 6, 17)).ToList();

            Assert.AreEqual(1, rows.Count);
            var row = rows[0];
            Assert.IsNotNull(row);
            Assert.AreEqual("Alfred", row.FName.Trim());
        }
    }

    internal class ExplicitJoinTests_V11 : ExplicitJoinTests_V7
    {
        public ExplicitJoinTests_V11()
        {
            _connectionString = Properties.Settings.Default.ConnectionString_V11;
        }
    }
}
