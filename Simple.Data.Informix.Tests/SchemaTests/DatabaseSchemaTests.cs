using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Simple.Data.TestHelper;
using Simple.Data.Informix.Tests.Properties;

namespace Simple.Data.Informix.Tests.SchemaTests
{
    internal class DatabaseSchemaTests :  DatabaseSchemaTestsBase
    {
        protected string _connectionString = null;

        public DatabaseSchemaTests() {
            _connectionString = Settings.Default.ConnectionString_V7;
        }

        protected override Database GetDatabase()
        {
            return Database.OpenConnection(_connectionString);
        }

        protected void ResetPrivateSchemaField()
        {
            FieldInfo[] fieldInfos = GetType().BaseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo fieldInfo =
                (from f in fieldInfos
                 where f.Name == "_schema"
                 select f).SingleOrDefault();

            if (fieldInfo == null) {
                fieldInfos = GetType().BaseType.BaseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                fieldInfo =
                    (from f in fieldInfos
                     where f.Name == "_schema"
                     select f).SingleOrDefault();
            }

            if (fieldInfo != null) {
                fieldInfo.SetValue(this, null);
            }
        }

        [Test]
        public void TestTables()
        {
            Assert.AreEqual(1, Schema.Tables.Count(t => t.ActualName == "stock"));
        }

        [Test]
        public void TestColumns()
        {
            var table = Schema.FindTable("stock");
            Assert.AreEqual(1, table.Columns.Count(c => c.ActualName == "unit_price"));
        }

        [Test]
        public void TestPrimaryKey()
        {
            var table = Schema.FindTable("customer");
            Assert.AreEqual(1, table.PrimaryKey.Length);
            Assert.AreEqual("customer_num", table.PrimaryKey[0]);
        }

        [Test]
        public void TestCompoundPrimaryKey()
        {
            var table = Schema.FindTable("stock");
            Assert.AreEqual(2, table.PrimaryKey.Length);
            Assert.AreEqual("stock_num", table.PrimaryKey[0]);
            Assert.AreEqual("manu_code", table.PrimaryKey[1]);
        }

        [Test]
        public void TestForeignKey()
        {
            var table = Schema.FindTable("stock");
            var fkey = table.ForeignKeys.Single();
            Assert.AreEqual("stock", fkey.DetailTable.Name);
            Assert.AreEqual("manu_code", fkey.Columns[0]);
            Assert.AreEqual("manufact", fkey.MasterTable.Name);
            Assert.AreEqual("manu_code", fkey.UniqueColumns[0]);
        }

        [Test]
        public void TestCompoundForeignKey()
        {
            var table = Schema.FindTable("items");
            var fkeys = table.ForeignKeys;

            Assert.AreEqual(2, fkeys.Count);

            Assert.AreEqual("items", fkeys[0].DetailTable.Name);
            Assert.AreEqual("order_num", fkeys[0].Columns[0]);
            Assert.AreEqual("orders", fkeys[0].MasterTable.Name);
            Assert.AreEqual("order_num", fkeys[0].UniqueColumns[0]);

            Assert.AreEqual("items", fkeys[1].DetailTable.Name);
            Assert.AreEqual("stock_num", fkeys[1].Columns[0]);
            Assert.AreEqual("manu_code", fkeys[1].Columns[1]);
            Assert.AreEqual("stock", fkeys[1].MasterTable.Name);
            Assert.AreEqual("stock_num", fkeys[1].UniqueColumns[0]);
            Assert.AreEqual("manu_code", fkeys[1].UniqueColumns[1]);
        }

        [Test]
        public void TestIdentityIsTrueWhenItShouldBe()
        {
            var column = Schema.FindTable("customer").FindColumn("customer_num");
            Assert.IsTrue(column.IsIdentity);
        }

        [Test]
        public void TestIdentityIsFalseWhenItShouldBe()
        {
            var column = Schema.FindTable("manufact").FindColumn("manu_code");
            Assert.IsFalse(column.IsIdentity);
        }

        [Test]
        public void TestQuoteObjectName_AddsQuotesWhenNotAlreadyQuoted()
        {
            Assert.AreEqual(@"""my_table""", Schema.QuoteObjectName("my_table"));
        }

        [Test]
        public void TestQuoteObjectName_DoesNotAddMoreQuotesWhenAlreadyQuoted()
        {
            Assert.AreEqual(@"""my_table""", Schema.QuoteObjectName(@"""my_table"""));
        }

        [Test]
        public void TestQuoteObjectName_DoesNotAddQuotesWhenDelimIdentOff()
        {
            string savedConnectionString = _connectionString;

            ResetPrivateSchemaField();
            _connectionString += "; DELIMIDENT=N";
            InformixSchemaProvider.ResetIsDelimIdentInEffect();

            Assert.AreEqual("my_table", Schema.QuoteObjectName("my_table"));

            ResetPrivateSchemaField();
            _connectionString = savedConnectionString;
            InformixSchemaProvider.ResetIsDelimIdentInEffect();
        }
    }

    [TestFixture]
    internal class DatabaseSchemaTests_V11 : DatabaseSchemaTests
    {
        public DatabaseSchemaTests_V11()
        {
            _connectionString = Settings.Default.ConnectionString_V11;
        }
    }
}
