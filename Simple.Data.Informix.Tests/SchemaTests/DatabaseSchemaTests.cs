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
            Assert.AreEqual(1, Schema.Tables.Count(t => t.ActualName == "name"));
        }

        [Test]
        public void TestColumns()
        {
            var table = Schema.FindTable("name");
            Assert.AreEqual(1, table.Columns.Count(c => c.ActualName == "rec_num"));
        }

        [Test]
        public void TestPrimaryKey()
        {
            var table = Schema.FindTable("name");
            Assert.AreEqual(1, table.PrimaryKey.Length);
            Assert.AreEqual("rec_num", table.PrimaryKey[0]);
        }

        [Test]
        public void TestCompoundPrimaryKey()
        {
            var table = Schema.FindTable("accounts");
            Assert.AreEqual(2, table.PrimaryKey.Length);
            Assert.AreEqual("acc_num", table.PrimaryKey[0]);
            Assert.AreEqual("acc_type", table.PrimaryKey[1]);
        }

        [Test]
        public void TestForeignKey()
        {
            var table = Schema.FindTable("child");
            var fkey = table.ForeignKeys.Single();
            Assert.AreEqual("rec_num", fkey.Columns[0]);
            Assert.AreEqual("name", fkey.MasterTable.Name);
            Assert.AreEqual("rec_num", fkey.UniqueColumns[0]);
        }

        [Test]
        public void TestCompoundForeignKey()
        {
            var table = Schema.FindTable("sub_accounts");
            var fkey = table.ForeignKeys.Single();
            Assert.AreEqual("ref_num", fkey.Columns[0]);
            Assert.AreEqual("ref_type", fkey.Columns[1]);
            Assert.AreEqual("accounts", fkey.MasterTable.Name);
            Assert.AreEqual("acc_num", fkey.UniqueColumns[0]);
            Assert.AreEqual("acc_type", fkey.UniqueColumns[1]);
        }

        [Test]
        public void TestIdentityIsTrueWhenItShouldBe()
        {
            var column = Schema.FindTable("name").FindColumn("rec_num");
            Assert.IsTrue(column.IsIdentity);
        }

        [Test]
        public void TestIdentityIsFalseWhenItShouldBe()
        {
            var column = Schema.FindTable("accounts").FindColumn("acc_num");
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
