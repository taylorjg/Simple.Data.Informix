using System;
using System.Collections.Generic;
using System.Linq;
using Simple.Data.Ado;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class FindTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7);
        }

        [Test]
        public void TestProviderIsInformixConnectionProvider()
        {
            var provider = new ProviderHelper().GetProviderByConnectionString(Properties.Settings.Default.ConnectionString_V7);
            Assert.IsInstanceOf(typeof(InformixConnectionProvider), provider);
        }

        [Test]
        public void TestProviderIsInformixConnectionProviderFromOpen()
        {
            Database db = DatabaseHelper.Open();
            Assert.IsInstanceOf(typeof(AdoAdapter), db.GetAdapter());
            Assert.IsInstanceOf(typeof(InformixConnectionProvider), ((AdoAdapter)db.GetAdapter()).ConnectionProvider);
        }

        [Test]
        public void TestFindByCustomerNum()
        {
            var db = DatabaseHelper.Open();
            var customer = db.Customers.FindByCustomerNum(101);
            Assert.AreEqual(101, customer.customer_num);
        }

        [Test]
        public void TestFindByIdWithCast()
        {
            var db = DatabaseHelper.Open();
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
            var db = DatabaseHelper.Open();
            var customer = (Customer)db.Customers.FindByFName("Bob");
            Assert.AreEqual(119, customer.CustomerNum);
        }

        [Test]
        public void TestFindAllByName()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<Customer> customers = db.Customers.FindAllByFName("Bob").Cast<Customer>();
            Assert.AreEqual(1, customers.Count());
        }

        [Test]
        public void TestFindAllByPartialName()
        {
            var db = DatabaseHelper.Open();
            IEnumerable<Customer> customers = db.Customers.FindAll(db.Customers.FName.Like("Bob")).ToList<Customer>();
            Assert.AreEqual(1, customers.Count());
        }

        [Test]
        public void TestAllCount()
        {
            var db = DatabaseHelper.Open();
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
            var db = DatabaseHelper.Open();
            Customer customer = db.Customers.FindByCustomerNum(101);
            Assert.AreEqual(101, customer.CustomerNum);
        }

        [Test]
        public void TestImplicitEnumerableCast()
        {
            var db = DatabaseHelper.Open();
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
            var db = DatabaseHelper.Open();
            var custView = db.CustView.FindByFirstName("Frank");
            Assert.IsNotNull(custView);
            Assert.AreEqual("Albertson", custView.LastName.Trim());
        }

        [Test]
        public void TestCast()
        {
            var db = DatabaseHelper.Open();
            var customerQuery = db.Customers.All().Cast<Customer>() as IEnumerable<Customer>;
            Assert.IsNotNull(customerQuery);
            var customers = customerQuery.ToList();
            Assert.AreEqual(28, customers.Count);
        }
    }
}
