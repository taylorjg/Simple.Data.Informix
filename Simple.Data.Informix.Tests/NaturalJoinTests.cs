using System;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class NaturalJoinTests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            DatabaseHelper.Reset(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7);
        }

        [Test]
        public void TestNaturalJoinDynamicForm()
        {
            var db = DatabaseHelper.Open();
            var row = db.Customers.Find(db.Customers.Orders.OrderDate == new DateTime(1994, 6, 17));
            Assert.IsNotNull(row);
            Assert.AreEqual("Alfred", row.FName.Trim());
        }

        [Test]
        public void TestNaturalJoinIndexersForm()
        {
            var db = DatabaseHelper.Open();
            var row = db["Customers"].Find(db["Customers"]["Orders"]["OrderDate"] == new DateTime(1994, 6, 17));
            Assert.IsNotNull(row);
            Assert.AreEqual("Alfred", row.FName.Trim());
        }
    }
}
