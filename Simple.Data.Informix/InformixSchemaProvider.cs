using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using IBM.Data.Informix;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Informix
{
    public class InformixSchemaProvider : ISchemaProvider
    {
        private readonly IConnectionProvider _connectionProvider;

        public InformixSchemaProvider(IConnectionProvider connectionProvider)
        {
            if (connectionProvider == null) throw new ArgumentNullException("connectionProvider");
            _connectionProvider = connectionProvider;
        }

        public IConnectionProvider ConnectionProvider
        {
            get { return _connectionProvider; }
        }

        public IEnumerable<Table> GetTables()
        {
            using (var cn = ConnectionProvider.CreateConnection()) {
                return GetTablesHelper.GetTables(cn as IfxConnection);
            }
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            using (var cn = ConnectionProvider.CreateConnection()) {
                return GetColumnsHelper.GetColumns(table, cn as IfxConnection);
            }
        }

        public Key GetPrimaryKey(Table table)
        {
            using (var cn = ConnectionProvider.CreateConnection()) {
                return GetPrimaryKeyHelper.GetPrimaryKey(table, cn as IfxConnection);
            }
        }

        public IEnumerable<ForeignKey> GetForeignKeys(Table table)
        {
            using (var cn = ConnectionProvider.CreateConnection()) {
                return GetForeignKeysHelper.GetForeignKeys(table, cn as IfxConnection);
            }
        }

        public IEnumerable<Procedure> GetStoredProcedures()
        {
            return Enumerable.Empty<Procedure>();
        }

        public IEnumerable<Parameter> GetParameters(Procedure storedProcedure)
        {
            return Enumerable.Empty<Parameter>();
        }

        [ThreadStatic]
        private static int m_s_numPaddingSpaces;
        private static readonly int MAX_PADDING_SPACES = 9;

        public string NameParameter(string baseName)
        {
            if (baseName == null) throw new ArgumentNullException("baseName");
            if (baseName.Length == 0) throw new ArgumentException("Base name must be provided");

            // "The IBM Informix .NET Provider Reference Guide" says the following about ? parameter markers:

            // Using ? parameter markers
            //
            // You can use the question mark symbol (?) to mark a parameter’s place in an SQL
            // statement or stored procedure. Because the IBM Informix .NET Provider does not
            // have access to names for these parameters, you must pass them in the correct
            // order. The order you add IfxParameter objects to an IfxParameterCollection object
            // must directly correspond to the position of the placeholder ? symbol for that
            // parameter. You use the ParameterCollection.Add method to add a parameter to a
            // collection.

            // So it looks as though we need to use ? for all parameter names.
            // However, if we simply return "?", then we get an exception if a SQL statement
            // contains more than one parameter. The exception is:
            //
            // [System.ArgumentException]	{"An item with the same key has already been added."}
            // 
            // So we need to use unique names but they all need to be "?" !!!
            // I have implemented the following hack. I add whitespace padding to the
            // end of each parameter name. I do this up to a maximum of MAX_PADDING_SPACES.
            // For example, if MAX_PADDING_SPACES is 9, then this gives us the following
            // parameter names in rotation:
            //
            // "?"              (0 spaces)
            // "? "             (1 space)
            // "?  "            (2 spaces)
            // "?   "           (3 spaces)
            // "?    "          (4 spaces)
            // "?     "         (5 spaces)
            // "?      "        (6 spaces)
            // "?       "       (7 spaces)
            // "?        "      (8 spaces)
            // "?         "     (9 spaces)

            string padding = new string(' ', m_s_numPaddingSpaces++);

            if (m_s_numPaddingSpaces > MAX_PADDING_SPACES) {
                m_s_numPaddingSpaces = 0;
            }

            return "?" + padding;
        }

        public string QuoteObjectName(string unquotedName)
        {
            if (unquotedName == null) {
                throw new ArgumentNullException("unquotedName");
            }

            if (!IsDelimIdentInEffect()) {
                return unquotedName;
            }

            if (unquotedName.StartsWith(@"""")) {
                return unquotedName;
            }

            return String.Concat(@"""", unquotedName, @"""");
        }

        private static bool? m_s_isDelimIdentInEffect = null;
        private static object m_s_isDelimIdentInEffectLockObject = new object();

        private bool IsDelimIdentInEffect()
        {
            lock (m_s_isDelimIdentInEffectLockObject) {

                // Have we already called IsDelimIdentInEffectHelper() ?
                if (!m_s_isDelimIdentInEffect.HasValue) {
                    // No - so call it now.
                    m_s_isDelimIdentInEffect = IsDelimIdentInEffectHelper();
                }
            }

            return m_s_isDelimIdentInEffect.Value;
        }

        private bool IsDelimIdentInEffectHelper()
        {
            // Assume DELIMIDENT is in effect unless tests suggest otherwise.
            // From the "IBM Informix .NET Provider Reference Guide" (Version 3.50):
            //
            //      "Note: In compliance with industry standards, the IBM Informix .NET Provider acts
            //             as though DELIMIDENT is set to Y unless you explicitly set it to N."
            bool result = true;

            bool? unquotedTest = null;
            bool? quotedTest = null;

            try {
                using (var cn = ConnectionProvider.CreateConnection()) {

                    cn.Open();

                    var command = cn.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"select count(*) from ""informix"".systables";
                    try {
                        command.ExecuteScalar();
                        unquotedTest = true;
                    }
                    catch (IfxException) {
                        unquotedTest = false;
                    }

                    command.CommandText = @"select count(*) from ""informix"".""systables""";
                    try {
                        command.ExecuteScalar();
                        quotedTest = true;
                    }
                    catch (IfxException) {
                        quotedTest = false;
                    }
                }
            }
            catch (Exception) {
            }

            // Did we actually execute both tests?
            if (unquotedTest.HasValue && quotedTest.HasValue) {
                // If the unquoted test succeeded and the quoted test failed...
                if (unquotedTest.Value && !quotedTest.Value) {
                    // ...then since the only difference between the tests is the
                    // quoting of the table name, it looks like DELIMIDENT is not
                    // in effect.
                    result = false;
                }
            }

            return result;
        }
    }
}
