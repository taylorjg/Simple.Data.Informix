using System;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class DeleteTests
    {
        [SetUp]
        public void Setup()
        {
            DatabaseHelper.Reset(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7);
        }

        [Test]
        public void TestDeleteByItemNumOrderNum()
        {
            var db = DatabaseHelper.Open();
            var item1 = db.Items.FindByOrderNumAndItemNum(1004, 4);
            Assert.IsNotNull(item1);
            db.Items.DeleteByOrderNumAndItemNum(1004, 4);
            var item2 = db.Items.FindByOrderNumAndItemNum(1004, 4);
            Assert.IsNull(item2);
        }

        [Test]
        public void TestDeleteAllOrders()
        {
            var db = DatabaseHelper.Open();
            db.Items.DeleteAll();
            Assert.AreEqual(0, db.Items.GetCount());
        }
    }
}
