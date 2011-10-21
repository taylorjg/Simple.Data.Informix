using System;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class NaturalJoinTests_V7
    {
        protected string _connectionString = null;

        public NaturalJoinTests_V7()
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
        public void TestNaturalJoinDynamicForm()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var row = db.Customers.Find(db.Customers.Orders.OrderDate == new DateTime(1994, 6, 17));
            Assert.IsNotNull(row);
            Assert.AreEqual("Alfred", row.FName.Trim());
        }

        [Test]
        public void TestNaturalJoinIndexersForm()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var row = db["Customers"].Find(db["Customers"]["Orders"]["OrderDate"] == new DateTime(1994, 6, 17));
            Assert.IsNotNull(row);
            Assert.AreEqual("Alfred", row.FName.Trim());
        }
    }

    internal class NaturalJoinTests_V11 : NaturalJoinTests_V7
    {
        public NaturalJoinTests_V11()
        {
            _connectionString = Properties.Settings.Default.ConnectionString_V11;
        }
    }
}
