using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class QueryTests_V7
    {
        protected string _connectionString = null;

        public QueryTests_V7()
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
        public void CountWithNoCriteriaShouldSelectTwentyEight()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            Assert.AreEqual(28, db.Customers.GetCount());
        }

        [Test]
        public void CountWithCriteriaShouldSelectTwo()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            Assert.AreEqual(2, db.Customers.GetCount(db.Customers.FName == "Frank"));
        }

        [Test]
        public void CountByShouldSelectOne()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            Assert.AreEqual(1, db.Customers.GetCountByFName("Kim"));
        }

        [Test]
        public void ExistsWithNoCriteriaShouldReturnTrue()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            Assert.AreEqual(true, db.Customers.Exists());
        }

        [Test]
        public void ExistsWithValidCriteriaShouldReturnTrue()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            Assert.AreEqual(true, db.Customers.Exists(db.Customers.FName == "Frank"));
        }

        [Test]
        public void ExistsWithInvalidCriteriaShouldReturnFalse()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            Assert.AreEqual(false, db.Customers.Exists(db.Customers.FName == "Bogus"));
        }

        [Test]
        public void ExistsByValidValueShouldReturnTrue()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            Assert.AreEqual(true, db.Customers.ExistsByFName("Frank"));
        }

        [Test]
        public void ExistsByInvalidValueShouldReturnFalse()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            Assert.AreEqual(false, db.Customers.ExistsByFName("Dracula"));
        }

        [Test]
        public void ColumnAliasShouldChangeDynamicPropertyName()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var actual = db.Customers.QueryByCustomerNum(101).Select(db.Customers.FName.As("MyFNameAlias")).First();
            Assert.AreEqual("Ludwig", actual.MyFNameAlias.Trim());
        }

        [Test]
        public void ShouldDirectlyQueryDetailTable()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var order = db.Customers.QueryByFNameAndCompany("Ludwig", "All Sports Supplies").Orders.FirstOrDefault();
            Assert.IsNotNull(order);
            Assert.AreEqual(1002, order.OrderNum);
        }

        [Test]
        public void ToScalarListShouldReturnStringList()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
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
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
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
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
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
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
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
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var row = db.Customers.Query()
                .Having(db.Customers.Orders.CustomerNum.Count() == 4)
                .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("Higgins", row.LName.Trim());
        }

        [Test]
        public void HavingWithAverageShouldReturnCorrectRow()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var row = db.Customers.Query()
                .Having(db.Customers.Orders.ShipCharge.Average() == 9.50)
                .FirstOrDefault();
            Assert.IsNotNull(row);
            Assert.AreEqual("Higgins", row.LName.Trim());
        }

        [Test]
        public void ToScalarOrDefault()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            int max = db.Orders.FindAllByCustomerNum(99).Select(db.Orders.OrderNum.Max()).ToScalarOrDefault<int>();
            Assert.AreEqual(0, max);
        }
    }

    [TestFixture]
    internal class QueryTests_V11 : QueryTests_V7
    {
        public QueryTests_V11()
        {
            _connectionString = Properties.Settings.Default.ConnectionString_V11;
        }

        // TODO - raise an issue against Simple.Data
        // Get stack overflow when we try to iterate the first item of a query involving Skip()/Take() if the
        // ADO Provider does not implement IQueryPager (as a MEF export).
        //
        // Experiment:
        // I wonder if this problem would be seen with the SqlServer ADO Provider
        // if I made a local change to it so as not to implement IQueryPager ???

        [Test]
        public void ShouldSelectFirstFive()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var query = db.Customers.All().OrderByCustomerNum().Take(5);
            int customerNum = 101;
            foreach (var row in query) {
                Assert.AreEqual(customerNum++, row.CustomerNum);
            }
        }

        [Test]
        public void ShouldSelectFromElevenToFifteen()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var query = db.Customers.All().OrderByCustomerNum().Skip(10).Take(5);
            int customerNum = 111;
            foreach (var row in query) {
                Assert.AreEqual(customerNum++, row.CustomerNum);
            }
        }

        [Test]
        public void ShouldSelectLastTen()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();
            var query = db.Customers.All().OrderByDescending(db.Customers.CustomerNum).Skip(0).Take(10);
            int customerNum = 128;
            foreach (var row in query) {
                Assert.AreEqual(customerNum--, row.CustomerNum);
            }
        }

        [Test]
        public void ShouldReturnNullWhenNoRowFound()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();

            string name = db.Customers
                .Query()
                .Select(db.Customers.FName)
                .Where(db.Customers.CustomerNum < 101)
                .OrderByFName()
                .Take(1)
                .ToScalarOrDefault<string>();

            Assert.IsNull(name);
        }


        // The remaining tests below involve WithTotalCount(). When the property,
        // InformixConnectionProvider.SupportsCompoundStatements, is set to return
        // false, these tests fail with a stack overflow around the area of:
        //
        // AdoAdapter.RunQuery()
        // AdoAdapter.RunQueryWithCount()
        // AdoAdapter.RunQueries()
        //
        // Experiment:
        // I wonder if this problem would be seen with the SqlServer ADO Provider
        // if I made a local change so that SupportsCompoundStatements returned false ???
        //
        // When the property is set to return true, these tests fail with:
        // ERROR [HY000] [Informix .NET provider][Informix]Cannot use a select or any of the database statements in a multi-query prepare.


        [Test]
        [Ignore]
        public void WithTotalCountShouldGiveCount()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();

            Promise<int> count;
            var list = db.Customers.Query(db.Customers.CustomerNum >= 101)
                .WithTotalCount(out count)
                .Take(10)
                .ToList();

            Assert.AreEqual(10, list.Count);
            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(28, count.Value);
            Assert.AreEqual(28, count);
        }

        [Test]
        [Ignore]
        public void WithTotalCountWithExplicitSelectShouldGiveCount()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();

            Promise<int> count;
            List<dynamic> list = db.Customers.All()
                .Select(db.Customers.CustomerNum)
                .WithTotalCount(out count)
                .Take(10)
                .ToList();

            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(28, count.Value);
            Assert.AreEqual(28, count);
            Assert.AreEqual(10, list.Count);
            foreach (var dictionary in list.Cast<IDictionary<string, object>>()) {
                Assert.AreEqual(1, dictionary.Count);
            }
        }

        [Test]
        [Ignore]
        public void WithTotalCountShouldGiveCount_ObsoleteFutureVersion()
        {
            TraceHelper.TraceTestName();
            var db = OpenDatabase();

            Future<int> count;
            var list = db.Customers.Query(db.Customers.CustomerNum >= 101)
                .WithTotalCount(out count)
                .Take(10)
                .ToList();

            Assert.AreEqual(10, list.Count);
            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(28, count.Value);
            Assert.AreEqual(28, count);
        }
    }
}
