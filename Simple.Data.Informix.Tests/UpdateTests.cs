using System;
using System.Dynamic;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class UpdateTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7);
        }

        [Test]
        public void TestUpdateWithNamedArguments()
        {
            var db = DatabaseHelper.Open();

            db.Customers.UpdateByCustomerNum(CustomerNum: 105, LName: "Chandler", State: "GA");
            var customer = db.Customers.FindByCustomerNum(105);
            Assert.IsNotNull(customer);
            Assert.AreEqual("Chandler", customer.LName.Trim());
            Assert.AreEqual("GA", customer.State);
        }

        [Test]
        public void TestUpdateWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            var customer = new Customer { CustomerNum = 113, FName = "Lana-X", LName="Beatty-Y", Company = "Sportstown-Z" };

            db.Customers.Update(customer);

            Customer actual = db.Customers.FindByCustomerNum(113);

            Assert.IsNotNull(actual);
            Assert.AreEqual("Lana-X", actual.FName.Trim());
            Assert.AreEqual("Beatty-Y", actual.LName.Trim());
            Assert.AreEqual("Sportstown-Z", actual.Company.Trim());
        }

        [Test]
        public void TestUpdateWithDynamicTypeObject()
        {
            var db = DatabaseHelper.Open();

            dynamic customer = new ExpandoObject();
            customer.CustomerNum = 114;
            customer.FName = "Frank-X";
            customer.LName = "Albertson-Y";
            customer.Company = "Sporting Place-Z";

            db.Customers.Update(customer);

            var actual = db.Customers.FindByCustomerNum(114);

            Assert.IsNotNull(actual);
            Assert.AreEqual("Frank-X", actual.FName.Trim());
            Assert.AreEqual("Albertson-Y", actual.LName.Trim());
            Assert.AreEqual("Sporting Place-Z", actual.Company.Trim());
        }

        [Test]
        public void TestUpdateWithVarBinaryMaxColumn()
        {
            var db = DatabaseHelper.Open();

            var data = new byte[56];
            for (int i = 0; i < data.Length; i++) {
                data[i] = (byte)i;
            }

            db.Catalogs.UpdateByCatalogNum(CatalogNum: 10001, CatPicture: data);

            var actual = db.Catalogs.FindByCatalogNum(10001);

            Assert.IsNotNull(actual);
            Assert.AreEqual(data, actual.CatPicture);
        }
    }
}
