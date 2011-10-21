using System;
using System.IO;
using System.Diagnostics;

namespace Simple.Data.Informix.Tests
{
    internal static class TraceHelper
    {
        private const string MY_TRACE_LISTENER_FILENAME = "Simple.Data.Informix.Tests.log";
        private const string MY_TRACE_LISTENER_NAME = "MyTextWriterTraceListener";
        private const string MY_TRACE_CATEGORY = "Simple.Data.Informix.Tests";

        public static void BeginTrace()
        {
            if (Trace.Listeners[MY_TRACE_LISTENER_NAME] == null) {
                Trace.Listeners.Add(new TextWriterTraceListener(MY_TRACE_LISTENER_FILENAME, MY_TRACE_LISTENER_NAME));
            }

            // IFXDOTNETTRACE
            //  0 No tracing
            //  1 Tracing of API entry and exit, with return code
            //  2 Tracing of API entry and exit, with return code, plus tracing of parameters to the API

            // IFXDOTNETTRACEFILE

            string ifxTraceFileName = Path.Combine(Directory.GetCurrentDirectory(), "IfxTrace.log");

            // Enabling tracing in the Informix .NET Provider seems to cause a problem
            // when executing unit tests via the DevExpress Unit Test Runner Tool Window:
            //
            // System.ArgumentNullException : Value cannot be null.
            // Parameter name: type
            // System.Reflection.Assembly.GetAssembly(Type type)
            // IBM.Data.Informix.IfxTrace.IsSelfCall(StackTrace stackTrace)
            // IBM.Data.Informix.IfxTrace.GetIfxTrace()
            // IBM.Data.Informix.IfxConnection..ctor(String connectionString)
            // Simple.Data.Informix.InformixConnectionProvider.CreateConnection() in InformixConnectionProvider.cs
            // Simple.Data.Ado.AdoAdapter.CreateConnection() in AdoAdapter.cs
            // Simple.Data.Ado.AdoAdapterInserter.ExecuteSingletonQuery(String insertSql, String selectSql, Object[] values) in AdoAdapterInserter.cs
            // Simple.Data.Ado.AdoAdapterInserter.Insert(String tableName, IEnumerable`1 data) in AdoAdapterInserter.cs
            // Simple.Data.Ado.AdoAdapter.Insert(String tableName, IDictionary`2 data) in AdoAdapter.cs
            // Simple.Data.Database.Insert(String tableName, IDictionary`2 data) in Database.cs
            // Simple.Data.Commands.InsertCommand.InsertEntity(Object entity, DataStrategy dataStrategy, String tableName) in InsertCommand.cs
            // Simple.Data.Commands.InsertCommand.DoInsert(InvokeMemberBinder binder, Object[] args, DataStrategy dataStrategy, String tableName) in InsertCommand.cs
            // Simple.Data.Commands.InsertCommand.Execute(DataStrategy dataStrategy, DynamicTable table, InvokeMemberBinder binder, Object[] args) in InsertCommand.cs
            // Simple.Data.DynamicTable.TryInvokeMember(InvokeMemberBinder binder, Object[] args, Object& result) in DynamicTable.cs
            // Simple.Data.ObjectReference.TryInvokeMember(InvokeMemberBinder binder, Object[] args, Object& result) in ObjectReference.cs
            // ..Target(, CallSite, , Object, , Object)
            // System.Dynamic.UpdateDelegates.UpdateAndExecute2[T0,T1,TRet](CallSite site, T0 arg0, T1 arg1)
            // Simple.Data.Informix.Tests.ConversionTests.InsertingWeirdTypesFromExpando() in ConversionTests.cs
            //
            // Therefore, I am commenting out the next couple of lines to disable this tracing.
            // Environment.SetEnvironmentVariable("IFXDOTNETTRACE", "2");
            // Environment.SetEnvironmentVariable("IFXDOTNETTRACEFILE", ifxTraceFileName);
        }

        public static void EndTrace()
        {
            var textWriterTraceListener = Trace.Listeners[MY_TRACE_LISTENER_NAME] as TextWriterTraceListener;
            if (textWriterTraceListener != null) {
                Trace.Listeners.Remove(textWriterTraceListener);
                textWriterTraceListener.Close();
            }

            //Environment.SetEnvironmentVariable("IFXDOTNETTRACE", null);
            //Environment.SetEnvironmentVariable("IFXDOTNETTRACEFILE", null);
        }

        public static void TraceTestName()
        {
            StackTrace stackTrace = new StackTrace(false /* fNeedFileInfo */);
            string testName = stackTrace.GetFrame(1).GetMethod().Name;
            Trace.WriteLine(testName, MY_TRACE_CATEGORY);
        }

        public static void WriteLine(string message)
        {
            Trace.WriteLine(message, MY_TRACE_CATEGORY);
        }
    }
}
