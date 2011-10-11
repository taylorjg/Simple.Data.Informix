using System;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class DatabaseOpenerTests
    {
        [Test]
        public void OpenNamedConnectionTest()
        {
            var db = Database.OpenNamedConnection("Test");
            Assert.IsNotNull(db);
            var customer = db.Customers.FindByCustomerNum(101);
            Assert.AreEqual(101, customer.customer_num);
        }
    }
}
