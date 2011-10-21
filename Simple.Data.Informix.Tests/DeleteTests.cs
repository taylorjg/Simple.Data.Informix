using System;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class DeleteTests_V7
    {
        protected string _connectionString = null;

        public DeleteTests_V7()
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
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            TraceHelper.EndTrace();
        }

        [SetUp]
        public void Setup()
        {
            DatabaseHelper.Reset(_connectionString);
        }

        [Test]
        public void TestDeleteByItemNumOrderNum()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var item1 = db.Items.FindByOrderNumAndItemNum(1004, 4);
            Assert.IsNotNull(item1);
            db.Items.DeleteByOrderNumAndItemNum(1004, 4);
            var item2 = db.Items.FindByOrderNumAndItemNum(1004, 4);
            Assert.IsNull(item2);
        }

        [Test]
        public void TestDeleteAllOrders()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            db.Items.DeleteAll();
            Assert.AreEqual(0, db.Items.GetCount());
        }
    }

    internal class DeleteTests_V11 : DeleteTests_V7
    {
        public DeleteTests_V11()
        {
            _connectionString = Properties.Settings.Default.ConnectionString_V11;
        }
    }
}
