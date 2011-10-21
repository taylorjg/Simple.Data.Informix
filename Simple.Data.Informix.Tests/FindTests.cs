using System;
using System.Collections.Generic;
using System.Linq;
using Simple.Data.Ado;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class FindTests_V7
    {
        protected string _connectionString = null;

        public FindTests_V7()
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
        public void TestProviderIsInformixConnectionProvider()
        {
            TraceHelper.TraceTestName();
            var provider = new ProviderHelper().GetProviderByConnectionString(_connectionString);
            Assert.IsInstanceOf(typeof(InformixConnectionProvider), provider);
        }

        [Test]
        public void TestProviderIsInformixConnectionProviderFromOpen()
        {
            TraceHelper.TraceTestName();
            Database db = OpenDatabase();
            Assert.IsInstanceOf(typeof(AdoAdapter), db.GetAdapter());
            Assert.IsInstanceOf(typeof(InformixConnectionProvider), ((AdoAdapter)db.GetAdapter()).ConnectionProvider);
        }

        [Test]
        public void TestFindByCustomerNum()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var customer = db.Customers.FindByCustomerNum(101);
            Assert.AreEqual(101, customer.customer_num);
        }

        [Test]
        public void TestFindByIdWithCast()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var customer = (Customer)db.Customers.FindByCustomerNum(101);
            Assert.AreEqual(101, customer.CustomerNum);
            Assert.AreEqual("Ludwig", customer.FName.Trim());
            Assert.AreEqual("Pauli", customer.LName.Trim());
            Assert.AreEqual("All Sports Supplies", customer.Company.Trim());
            Assert.AreEqual("213 Erstwild Court", customer.Address1.Trim());
            Assert.AreEqual(null, customer.Address2);
            Assert.AreEqual("Sunnyvale", customer.City.Trim());
            Assert.AreEqual("CA", customer.State.Trim());
            Assert.AreEqual("94086", customer.Zipcode.Trim());
            Assert.AreEqual("408-789-8075", customer.Phone.Trim());
        }

        [Test]
        public void TestFindByReturnsOne()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var customer = (Customer)db.Customers.FindByFName("Bob");
            Assert.AreEqual(119, customer.CustomerNum);
        }

        [Test]
        public void TestFindAllByName()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            IEnumerable<Customer> customers = db.Customers.FindAllByFName("Bob").Cast<Customer>();
            Assert.AreEqual(1, customers.Count());
        }

        [Test]
        public void TestFindAllByPartialName()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            IEnumerable<Customer> customers = db.Customers.FindAll(db.Customers.FName.Like("Bob")).ToList<Customer>();
            Assert.AreEqual(1, customers.Count());
        }

        [Test]
        public void TestAllCount()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var count = db.Customers.All().ToList().Count;
            Assert.AreEqual(28, count);
        }

        //[Test]
        //public void TestAllWithSkipCount()
        //{
        //    var db = DatabaseHelper.Open();
        //    var count = db.Customers.All().Skip(8).ToList().Count;
        //    Assert.AreEqual(20, count);
        //}

        [Test]
        public void TestImplicitCast()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            Customer customer = db.Customers.FindByCustomerNum(101);
            Assert.AreEqual(101, customer.CustomerNum);
        }

        [Test]
        public void TestImplicitEnumerableCast()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            foreach (Customer customer in db.Customers.All()) {
                Assert.IsNotNull(customer);
            }
        }

        //[Test]
        //public void TestFindWithSchemaQualification()
        //{
        //    var db = DatabaseHelper.Open();
            
        //    var dboActual = db.dbo.SchemaTable.FindById(1);
        //    var testActual = db.test.SchemaTable.FindById(1);

        //    Assert.IsNotNull(dboActual);
        //    Assert.AreEqual("Pass", dboActual.Description);
        //    Assert.IsNull(testActual);
        //}

        //[Test]
        //public void TestFindWithCriteriaAndSchemaQualification()
        //{
        //    var db = DatabaseHelper.Open();

        //    var dboActual = db.dbo.SchemaTable.Find(db.dbo.SchemaTable.Id == 1);

        //    Assert.IsNotNull(dboActual);
        //    Assert.AreEqual("Pass", dboActual.Description);
        //}

        [Test]
        public void TestFindOnAView()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var custView = db.CustView.FindByFirstName("Frank");
            Assert.IsNotNull(custView);
            Assert.AreEqual("Albertson", custView.LastName.Trim());
        }

        [Test]
        public void TestCast()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var customerQuery = db.Customers.All().Cast<Customer>() as IEnumerable<Customer>;
            Assert.IsNotNull(customerQuery);
            var customers = customerQuery.ToList();
            Assert.AreEqual(28, customers.Count);
        }
    }
}
