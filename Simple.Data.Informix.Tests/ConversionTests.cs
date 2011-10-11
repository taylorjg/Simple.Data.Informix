using System;
using System.Dynamic;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class ConversionTests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7);
        }


        [Test]
        public void WeirdTypeGetsConvertedToInt()
        {
            var db = DatabaseHelper.Open();

            var weirdValue = new WeirdType(101);
            var customer = db.Customers.FindByCustomerNum(weirdValue);
            Assert.AreEqual(101, customer.CustomerNum);
        }

        [Test]
        public void WeirdTypeUsedInQueryGetsConvertedToInt()
        {
            var db = DatabaseHelper.Open();

            var weirdValue = new WeirdType(101);
            var customer = db.Customers.QueryByCustomerNum(weirdValue).FirstOrDefault();
            Assert.IsNotNull(customer);
            Assert.AreEqual(101, customer.CustomerNum);
        }

        [Test]
        public void InsertingWeirdTypesFromExpando()
        {
            var db = DatabaseHelper.Open();

            dynamic expando = new ExpandoObject();
            expando.FName = new WeirdType("Raymond");
            expando.LName = new WeirdType("Duck");
            expando.Company = new WeirdType("Monty's first agent");
            expando.ThisIsNotAColumn = new WeirdType(123);

            var customer = db.Customers.Insert(expando);
            Assert.IsInstanceOf<int>(customer.CustomerNum);
            Assert.AreEqual("Raymond", customer.FName.Trim());
            Assert.AreEqual("Duck", customer.LName.Trim());
            Assert.AreEqual("Monty's first agent", customer.Company.Trim());
        }
    }

    internal class WeirdType : DynamicObject
    {
        private readonly object _value;

        public WeirdType(object value)
        {
            _value = value;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = Convert.ChangeType(_value, binder.Type);
            return true;
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
