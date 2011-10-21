using System;
using System.ComponentModel.Composition;
using System.Data;
using IBM.Data.Informix;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Informix
{
    [Export(typeof(IConnectionProvider))]
    [Export("IBM.Data.Informix", typeof(IConnectionProvider))]
    public class InformixConnectionProvider : IConnectionProvider
    {
        private string _connectionString;

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public IDbConnection CreateConnection()
        {
            return new IfxConnection(_connectionString);
        }

        public string GetIdentityFunction()
        {
            return "(SELECT DBINFO('sqlca.sqlerrd1') FROM systables WHERE tabid = 1)";
        }

        public IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            throw new NotImplementedException();
        }

        public ISchemaProvider GetSchemaProvider()
        {
            return new InformixSchemaProvider(this);
        }

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool SupportsCompoundStatements
        {
            // I guess not. If I set this property to return true then the following line...
            // db.Customers.All().WithTotalCount(out count).ToList();
            // ...fails with this:
            // ERROR [HY000] [Informix .NET provider][Informix]Cannot use a select or any of the database statements in a multi-query prepare.
            get { return false; }
        }

        public bool SupportsStoredProcedures
        {
            get { return false; }
        }
    }
}
