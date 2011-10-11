using System;
using System.Collections.Generic;
using System.Dynamic;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class InsertTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7);
        }

        [Test]
        public void TestInsertWithNamedArguments()
        {
            var db = DatabaseHelper.Open();

            var customer = db.Customers.Insert(FName: "Joseph", LName: "Heller", Company: "Catch 22");

            Assert.IsNotNull(customer);
            Assert.AreEqual("Joseph", customer.FName.Trim());
            Assert.AreEqual("Heller", customer.LName.Trim());
            Assert.AreEqual("Catch 22", customer.Company.Trim());
        }

        [Test]
        public void TestInsertWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            var customer = new Customer { FName = "Charles", LName = "Dickens", Company = "Bleak House" };

            var actual = db.Customers.Insert(customer);

            Assert.IsNotNull(actual);
            Assert.AreEqual("Charles", actual.FName.Trim());
            Assert.AreEqual("Dickens", actual.LName.Trim());
            Assert.AreEqual("Bleak House", actual.Company.Trim());
        }

        [Test]
        public void TestMultiInsertWithStaticTypeObjects()
        {
            var db = DatabaseHelper.Open();

            var customers = new[]
                            {
                                new Customer { FName = "Bill", LName = "Gates", Company = "Microsoft" },
                                new Customer { FName = "Donald", LName = "Trump", Company = "Windy Property Inc" }
                            };

            IList<Customer> actuals = db.Customers.Insert(customers).ToList<Customer>();

            Assert.AreEqual(2, actuals.Count);

            Assert.AreNotEqual(0, actuals[0].CustomerNum);
            Assert.AreEqual("Bill", actuals[0].FName.Trim());
            Assert.AreEqual("Gates", actuals[0].LName.Trim());
            Assert.AreEqual("Microsoft", actuals[0].Company.Trim());

            Assert.AreNotEqual(0, actuals[1].CustomerNum);
            Assert.AreEqual("Donald", actuals[1].FName.Trim());
            Assert.AreEqual("Trump", actuals[1].LName.Trim());
            Assert.AreEqual("Windy Property Inc", actuals[1].Company.Trim());
        }

        [Test]
        public void TestInsertWithDynamicTypeObject()
        {
            var db = DatabaseHelper.Open();

            dynamic customer = new ExpandoObject();
            customer.FName = "Jon";
            customer.LName = "Skeet";
            customer.Company = "C# in Depth";

            var actual = db.Customers.Insert(customer);

            Assert.IsNotNull(actual);
            Assert.AreEqual("Jon", actual.FName.Trim());
            Assert.AreEqual("Skeet", actual.LName.Trim());
            Assert.AreEqual("C# in Depth", actual.Company.Trim());
        }

        [Test]
        public void TestMultiInsertWithDynamicTypeObjects()
        {
            var db = DatabaseHelper.Open();

            dynamic customer1 = new ExpandoObject();
            customer1.FName = "Maurice";
            customer1.LName = "Ravel";
            customer1.Company = "Concerto (Left Hand)";

            dynamic customer2 = new ExpandoObject();
            customer2.FName = "Ludwig";
            customer2.LName = "van Beethoven";
            customer2.Company = "Symphony No 9";

            var customers = new[] { customer1, customer2 };

            IList<dynamic> actuals = db.Customers.Insert(customers).ToList();

            Assert.AreEqual(2, actuals.Count);

            Assert.AreNotEqual(0, actuals[0].CustomerNum);
            Assert.AreEqual("Maurice", actuals[0].FName.Trim());
            Assert.AreEqual("Ravel", actuals[0].LName.Trim());
            Assert.AreEqual("Concerto (Left Hand)", actuals[0].Company.Trim());

            Assert.AreNotEqual(0, actuals[1].CustomerNum);
            Assert.AreEqual("Ludwig", actuals[1].FName.Trim());
            Assert.AreEqual("van Beethoven", actuals[1].LName.Trim());
            Assert.AreEqual("Symphony No 9", actuals[1].Company.Trim());
        }

        //[Test]
        //public void TestWithImageColumn()
        //{
        //    var db = DatabaseHelper.Open();
        //    try
        //    {
        //        var image = GetImage.Image;
        //        db.Images.Insert(Id: 1, TheImage: image);
        //        var img = (DbImage)db.Images.FindById(1);
        //        Assert.IsTrue(image.SequenceEqual(img.TheImage));
        //    }
        //    finally
        //    {
        //        db.Images.DeleteById(1);
        //    }
        //}

        [Test]
        public void TestInsertWithVarBinaryMaxColumn()
        {
            var db = DatabaseHelper.Open();

            var data = new byte[56];
            for (int i = 0; i < data.Length; i++) {
                data[i] = (byte)i;
            }

            var actual = db.Catalogs.Insert(StockNum: 111, ManuCode: "SHM", CatPicture: data);

            Assert.IsNotNull(actual);
            Assert.AreEqual(data, actual.CatPicture);
        }
    }
}
