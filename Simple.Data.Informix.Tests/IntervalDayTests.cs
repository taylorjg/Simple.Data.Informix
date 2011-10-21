using System;
using NUnit.Framework;

namespace Simple.Data.Informix.Tests
{
    [TestFixture]
    internal class IntervalDayTests
    {
        [TestFixtureSetUp]
        public static void TestFixtureSetUp()
        {
            TraceHelper.BeginTrace();
            DatabaseHelper.Reset(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7);
        }

        [TestFixtureTearDown]
        public static void TestFixtureTearDown()
        {
            TraceHelper.EndTrace();
        }

        [Test]
        public void FindUsingInformixAdoDotNetProviderDirectly()
        {
            using (var cn = new IBM.Data.Informix.IfxConnection(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7)) {
                cn.Open();
                using (var cmd = cn.CreateCommand()) {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "select * from manufact where lead_time = ?";
                    cmd.Parameters.Add("lead_time", "3");
                    using (var reader = cmd.ExecuteReader()) {
                        for (; reader.Read(); ) {
                            var manu_code = reader[0];
                            var manu_name = reader[1];
                            var lead_time = reader[2];
                        }
                    }
                }
            }
        }

        [Test]
        public void UpdateUsingInformixAdoDotNetProviderDirectly()
        {
            using (var cn = new IBM.Data.Informix.IfxConnection(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7)) {
                cn.Open();
                using (var cmd = cn.CreateCommand()) {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "update manufact set lead_time = ? where manu_code = ?";
                    cmd.Parameters.Add("lead_time", "16");
                    cmd.Parameters.Add("manu_code", "NKL");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // https://www.ibm.com/developerworks/forums/thread.jspa?threadID=394088
        //
        // Using string literal "17" as a value for the lead_time column fails with the
        // following stack trace. This is especially odd because it works just fine for
        // select, update and delete statements. It only fails for an insert statement.
        // The workaround is to use IfxTimeSpan(17, IfxTimeUnit.Day).
        //
        // System.ArgumentException : Unknown SQL type - INTERVAL_DAY.
        // IBM.Data.Informix.TypeMap.FromSqlType(SQL_TYPE sqlType)
        // IBM.Data.Informix.IfxParameter.Bind(IfxDataReader tmpReader, IntPtr stmt, IfxCommand parent, Int16 ordinal, CNativeBuffer valueBuffer, CNativeBuffer lenIndBuffer)
        // IBM.Data.Informix.IfxCommand.ExecuteReaderObject(CommandBehavior behavior, String method)
        // IBM.Data.Informix.IfxCommand.ExecuteNonQuery()
        // Simple.Data.Informix.Tests.IntervalDayTests.InsertUsingInformixAdoDotNetProviderDirectly() in IntervalDayTests.cs
        [Test]
        [Ignore]
        public void InsertUsingInformixAdoDotNetProviderDirectly()
        {
            using (var cn = new IBM.Data.Informix.IfxConnection(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7)) {
                cn.Open();
                using (var cmd = cn.CreateCommand()) {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "insert into manufact (manu_code, manu_name, lead_time) values (?, ?, ?)";
                    cmd.Parameters.Add("manu_code", "J17");
                    cmd.Parameters.Add("manu_name", "Jon 17");

                    cmd.Parameters.Add("lead_time", "17");
                    //cmd.Parameters.Add("lead_time", new IBM.Data.Informix.IfxTimeSpan(17, IBM.Data.Informix.IfxTimeUnit.Day));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        [Test]
        public void DeleteUsingInformixAdoDotNetProviderDirectly()
        {
            using (var cn = new IBM.Data.Informix.IfxConnection(Simple.Data.Informix.Tests.Properties.Settings.Default.ConnectionString_V7)) {
                cn.Open();
                using (var cmd = cn.CreateCommand()) {

                    cmd.CommandType = System.Data.CommandType.Text;

                    cmd.CommandText = "insert into manufact (manu_code, manu_name, lead_time) values (?, ?, ?)";
                    cmd.Parameters.Add("manu_code", "J42");
                    cmd.Parameters.Add("manu_name", "Jon 42");
                    cmd.Parameters.Add("lead_time", new IBM.Data.Informix.IfxTimeSpan(42, IBM.Data.Informix.IfxTimeUnit.Day));
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete from manufact where lead_time = ?";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add("lead_time", "42");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [Test]
        public void FindAllByIntervalDayColumn()
        {
            TraceHelper.TraceTestName();
            var db = DatabaseHelper.Open();
            var rows = db.ManuFacts.FindAllByLeadTime("21").ToList();
            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual(typeof(TimeSpan), rows[0].LeadTime.GetType());
        }

        [Test]
        public void UpdateByManuCode()
        {
            TraceHelper.TraceTestName();
            var db = DatabaseHelper.Open();
            db.ManuFacts.UpdateByManuCode(ManuCode: "HSK", LeadTime: "12");
        }

        [Test]
        public void DeleteAllByIntervalDayColumn()
        {
            TraceHelper.TraceTestName();
            var db = DatabaseHelper.Open();

            db.ManuFacts.Insert(ManuCode: "J14", ManuName: "Jon 14");
            db.ManuFacts.UpdateByManuCode(ManuCode: "J14", LeadTime: "14");

            db.ManuFacts.DeleteByLeadTime("14");
        }

        // https://www.ibm.com/developerworks/forums/thread.jspa?threadID=394088
        //
        // This test fails with:
        //
        // System.ArgumentException : Unknown SQL type - INTERVAL_DAY.
        // IBM.Data.Informix.TypeMap.FromSqlType(SQL_TYPE sqlType)
        // IBM.Data.Informix.IfxParameter.Bind(IfxDataReader tmpReader, IntPtr stmt, IfxCommand parent, Int16 ordinal, CNativeBuffer valueBuffer, CNativeBuffer lenIndBuffer)
        // IBM.Data.Informix.IfxCommand.ExecuteReaderObject(CommandBehavior behavior, String method)
        // IBM.Data.Informix.IfxCommand.ExecuteNonQuery()
        // Simple.Data.Ado.AdoAdapterInserter.TryExecute(IDbCommand command) in AdoAdapterInserter.cs
        // Simple.Data.Ado.AdoAdapterInserter.Execute(String sql, Object[] values) in AdoAdapterInserter.cs
        // Simple.Data.Ado.AdoAdapterInserter.Insert(String tableName, IEnumerable`1 data) in AdoAdapterInserter.cs
        // Simple.Data.Ado.AdoAdapter.Insert(String tableName, IDictionary`2 data) in AdoAdapter.cs
        // Simple.Data.Database.Insert(String tableName, IDictionary`2 data) in Database.cs
        // Simple.Data.Commands.InsertCommand.InsertDictionary(InvokeMemberBinder binder, Object[] args, DataStrategy dataStrategy, String tableName) in InsertCommand.cs
        // Simple.Data.Commands.InsertCommand.DoInsert(InvokeMemberBinder binder, Object[] args, DataStrategy dataStrategy, String tableName) in InsertCommand.cs
        // Simple.Data.Commands.InsertCommand.Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, Object[] args) in InsertCommand.cs
        // Simple.Data.DynamicTable.TryInvokeMember(InvokeMemberBinder binder, Object[] args, Object& result) in DynamicTable.cs
        // Simple.Data.ObjectReference.TryInvokeMember(InvokeMemberBinder binder, Object[] args, Object& result) in ObjectReference.cs
        // ..Target(, CallSite, , Object, , String, , String, , String)
        // System.Dynamic.UpdateDelegates.UpdateAndExecuteVoid4[T0,T1,T2,T3](CallSite site, T0 arg0, T1 arg1, T2 arg2, T3 arg3)
        // Simple.Data.Informix.Tests.IntervalDayTests.InsertManuFact() in IntervalDayTests.cs
        [Test]
        [Ignore]
        public void InsertManuFact()
        {
            TraceHelper.TraceTestName();
            var db = DatabaseHelper.Open();
            db.ManuFacts.Insert(ManuCode: "J19", ManuName: "Jon 19", LeadTime: "19");
        }

        [Test]
        public void InsertManuFactWorkaround()
        {
            TraceHelper.TraceTestName();
            var db = DatabaseHelper.Open();
            db.ManuFacts.Insert(ManuCode: "J20", ManuName: "Jon 20");
            db.ManuFacts.UpdateByManuCode(ManuCode: "J20", LeadTime: "20");
        }
    }
}
