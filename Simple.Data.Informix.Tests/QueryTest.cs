using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class QueryTest
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            DatabaseHelper.Reset(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7);
        }

        [Test]
        public void CountWithNoCriteriaShouldSelectThree()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(28, db.Customers.GetCount());
        }

        [Test]
        public void CountWithCriteriaShouldSelectTwo()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(2, db.Customers.GetCount(db.Customers.FName == "Frank"));
        }

        [Test]
        public void CountByShouldSelectOne()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(1, db.Customers.GetCountByFName("Kim"));
        }

        [Test]
        public void ExistsWithNoCriteriaShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(true, db.Customers.Exists());
        }

        [Test]
        public void ExistsWithValidCriteriaShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(true, db.Customers.Exists(db.Customers.FName == "Frank"));
        }

        [Test]
        public void ExistsWithInvalidCriteriaShouldReturnFalse()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(false, db.Customers.Exists(db.Customers.FName == "Bogus"));
        }

        [Test]
        public void ExistsByValidValueShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(true, db.Customers.ExistsByFName("Frank"));
        }

        [Test]
        public void ExistsByInvalidValueShouldReturnFalse()
        {
            var db = DatabaseHelper.Open();
            Assert.AreEqual(false, db.Customers.ExistsByFName("Dracula"));
        }

        [Test]
        public void ColumnAliasShouldChangeDynamicPropertyName()
        {
            var db = DatabaseHelper.Open();
            var actual = db.Customers.QueryByCustomerNum(101).Select(db.Customers.FName.As("MyFNameAlias")).First();
            Assert.AreEqual("Ludwig", actual.MyFNameAlias.Trim());
        }

        //[Test]
        //public void ShouldSelectFromOneToTen()
        //{
        //    var db = DatabaseHelper.Open();
        //    var query = db.Customers.QueryByCustomerNum(101.to(127)).Take(10);
        //    int index = 101;
        //    // Get stack overflow when we try to iterate the first item if the
        //    // connection provider does not implement (export via MEF) IQueryPager.
        //    foreach (var row in query) {
        //        Assert.AreEqual(index, row.CustomerNum);
        //        index++;
        //    }
        //}

        //[Test]
        //public void ShouldSelectFromElevenToTwenty()
        //{
        //    var db = DatabaseHelper.Open();
        //    var query = db.PagingTest.QueryById(1.to(100)).Skip(10).Take(10);
        //    int index = 11;
        //    foreach (var row in query)
        //    {
        //        Assert.AreEqual(index, row.Id);
        //        index++;
        //    }
        //}

        //[Test]
        //public void ShouldSelectFromOneHundredToNinetyOne()
        //{
        //    var db = DatabaseHelper.Open();
        //    var query = db.PagingTest.QueryById(1.to(100)).OrderByDescending(db.PagingTest.Id).Skip(0).Take(10);
        //    int index = 100;
        //    foreach (var row in query)
        //    {
        //        Assert.AreEqual(index, row.Id);
        //        index--;
        //    }
        //}

        //[Test]
        //public void WithTotalCountShouldGiveCount()
        //{
        //    Promise<int> count;
        //    var db = DatabaseHelper.Open();
        //    var list = db.PagingTest.QueryById(1.to(50))
        //        .WithTotalCount(out count)
        //        .Take(10)
        //        .ToList();

        //    Assert.AreEqual(10, list.Count);
        //    Assert.IsTrue(count.HasValue);
        //    Assert.AreEqual(50, count);
        //}

        //[Test]
        //public void WithTotalCountWithExplicitSelectShouldGiveCount()
        //{
        //    Promise<int> count;
        //    var db = DatabaseHelper.Open();
        //    List<dynamic> list = db.PagingTest.QueryById(1.to(50))
        //        .Select(db.PagingTest.Id)
        //        .WithTotalCount(out count)
        //        .Take(10)
        //        .ToList();

        //    Assert.IsTrue(count.HasValue);
        //    Assert.AreEqual(50, count);
        //    Assert.AreEqual(10, list.Count);
        //    foreach (var dictionary in list.Cast<IDictionary<string,object>>())
        //    {
        //        Assert.AreEqual(1, dictionary.Count);
        //    }
        //}

        //[Test]
        //public void WithTotalCountShouldGiveCount_ObsoleteFutureVersion()
        //{
        //    Future<int> count;
        //    var db = DatabaseHelper.Open();
        //    var list = db.PagingTest.QueryById(1.to(50))
        //        .WithTotalCount(out count)
        //        .Take(10)
        //        .ToList();

        //    Assert.AreEqual(10, list.Count);
        //    Assert.IsTrue(count.HasValue);
        //    Assert.AreEqual(50, count);
        //}

        [Test]
        public void ShouldDirectlyQueryDetailTable()
        {
            var db = DatabaseHelper.Open();
            var order = db.Customers.QueryByFNameAndCompany("Ludwig", "All Sports Supplies").Orders.FirstOrDefault();
            Assert.IsNotNull(order);
            Assert.AreEqual(1002, order.OrderNum);
        }

        //[Test]
        //public void ShouldReturnNullWhenNoRowFound()
        //{
        //    var db = DatabaseHelper.Open();
        //    string name = db.Customers
        //                .Query()
        //                .Select(db.Customers.FName)
        //                .Where(db.Customers.CustomerNum == 99) // There is no CustomerNum 99
        //                .OrderByFName()
        //                .Take(1) // Should return only one record no matter what
        //                .ToScalarOrDefault<string>();
        //    Assert.IsNull(name);
        //}

        [Test]
        public void ToScalarListShouldReturnStringList()
        {
            var db = DatabaseHelper.Open();
            List<string> name = db.Customers
                        .Query()
                        .Select(db.Customers.FName)
                        .OrderByFName()
                        .ToScalarList<string>();
            Assert.IsNotNull(name);
            Assert.AreNotEqual(0, name.Count);
        }

        [Test]
        public void ToScalarArrayShouldReturnStringArray()
        {
            var db = DatabaseHelper.Open();
            string[] name = db.Customers
                        .Query()
                        .Select(db.Customers.FName)
                        .OrderByFName()
                        .ToScalarArray<string>();
            Assert.IsNotNull(name);
            Assert.AreNotEqual(0, name.Length);
        }

        [Test]
        public void HavingWithMinDateShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row =
                db.Customers.Query().Having(db.Customers.Orders.OrderDate.Min() <=
                                                  new DateTime(1994, 5, 20))
                                                  .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual(104, row.CustomerNum);
        }

        [Test]
        public void HavingWithMaxDateShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row =
                db.Customers.QueryByState("CA").Having(db.Customers.Orders.OrderDate.Max() >=
                                                  new DateTime(1994, 6, 27))
                                                  .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("Roy", row.FName.Trim());
        }

        [Test]
        public void HavingWithCountShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row = db.Customers.Query()
                .Having(db.Customers.Orders.CustomerNum.Count() == 4)
                .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("Higgins", row.LName.Trim());
        }

        [Test]
        public void HavingWithAverageShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row = db.Customers.Query()
                .Having(db.Customers.Orders.ShipCharge.Average() == 9.50)
                .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("Higgins", row.LName.Trim());
        }

        [Test]
        public void ToScalarOrDefault()
        {
            var db = DatabaseHelper.Open();
            int max = db.Orders.FindAllByCustomerNum(99).Select(db.Orders.OrderNum.Max()).ToScalarOrDefault<int>();
            Assert.AreEqual(0, max);
        }
    }
}
