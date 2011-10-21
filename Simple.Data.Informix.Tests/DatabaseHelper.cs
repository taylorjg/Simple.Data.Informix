using System;
using System.Data;
using System.Linq;
using System.Diagnostics;
using IBM.Data.Informix;

namespace Simple.Data.Informix.Tests
{
    internal static class DatabaseHelper
    {
        public static dynamic Open()
        {
            return Database.Opener.OpenConnection(Properties.Settings.Default.ConnectionString_V7);
        }

        public static dynamic Open(string connectionString)
        {
            return Database.Opener.OpenConnection(connectionString);
        }

        public static void Reset(string connectionString)
        {
            // Ensure that our date format matches that of the .unl files.
            Environment.SetEnvironmentVariable("DBDATE", "MDY4/");

            using (var cn = new IfxConnection(connectionString)) {

                cn.Open();

                DeleteRecords(cn, "catalog");
                DeleteRecords(cn, "cust_calls");
                DeleteRecords(cn, "call_type");
                DeleteRecords(cn, "state");
                DeleteRecords(cn, "items");
                DeleteRecords(cn, "orders");
                DeleteRecords(cn, "stock");
                DeleteRecords(cn, "manufact");
                DeleteRecords(cn, "customer");

                LoadRecords(cn, connectionString, "customer");
                LoadRecords(cn, connectionString, "manufact");
                LoadRecords(cn, connectionString, "stock");
                LoadRecords(cn, connectionString, "orders");
                LoadRecords(cn, connectionString, "items");
                LoadRecords(cn, connectionString, "state");
                LoadRecords(cn, connectionString, "call_type");
                LoadRecords(cn, connectionString, "cust_calls");
                LoadRecords(cn, connectionString, "catalog");
            }

            Environment.SetEnvironmentVariable("DBDATE", null);
        }

        private static void DeleteRecords(IfxConnection cn, string tableName)
        {
            using (var cmd = cn.CreateCommand()) {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = string.Format("delete from {0}", tableName);
                cmd.ExecuteNonQuery();
            }
        }

        private static void LoadRecords(IfxConnection cn, string connectionString, string tableName)
        {
            DatabaseSchemaHelper databaseSchemaHelper = new DatabaseSchemaHelper(connectionString);

            var schemaColumns = databaseSchemaHelper.GetColumns(tableName);
            string[] columnNames = (from schemaColumn in schemaColumns select schemaColumn.ActualName).ToArray();

            string joinedColumnNames = string.Join(", ", columnNames);
            int numColumns = columnNames.Length;

            // Load the .unl file (renamed to .txt and added to the project as a resource file).
            string fileContents = Resources.Resources.ResourceManager.GetString(tableName);
            string[] lines = fileContents.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            string[] placeHolders = new string[numColumns];
            for (int i = 0; i < numColumns; i++) {
                placeHolders[i] = "?";
            }
            string joinedPlaceHolders = string.Join(", ", placeHolders);

            using (var cmd = cn.CreateCommand()) {

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = string.Format("insert into {0} ({1}) values ({2})", tableName, joinedColumnNames, joinedPlaceHolders);

                for (int i = 0; i < numColumns; i++) {
                    cmd.Parameters.Add(columnNames[i], null);
                }

                foreach (var line in lines) {

                    string[] values = line.Split('|');

                    // "value1|value2|value3|" after splitting on '|' yields "value1", "value2", value3, ""
                    // i.e. there is an empty extra value at the end of the array.
                    // And, there is usually a blank line at the end of the file.
                    if (values.Length != (numColumns + 1)) {
                        break;
                    }

                    for (int i = 0; i < numColumns; i++) {
                        if (!string.IsNullOrEmpty(values[i])) {
                            // This is a temporary hack for the "lead_time" column of the "manufact" table.
                            // We should lookup the coltype and collength of the column
                            // using tableName and schemaColumns[i].ActualName. Then, if coltype
                            // is 14 (INTERVAL) then we need to look at collength to determine the
                            // detailed type of INTERVAL. Then, create a new instance of IfxTimeSpan
                            // or IfxMonthSpan and parse the string, values[i]. There may be other column types
                            // that need to be handled specially. I guess we need to test lots more
                            // different column types.
                            //
                            // SYSCOLUMNS              
                            //  http://publib.boulder.ibm.com/infocenter/idshelp/v115/index.jsp?topic=%2Fcom.ibm.sqlr.doc%2Fids_sqr_025.htm
                            //
                            // Storing Column Length    
                            //  http://publib.boulder.ibm.com/infocenter/idshelp/v115/index.jsp?topic=%2Fcom.ibm.sqlr.doc%2Fids_sqr_027.htm
                            //
                            // Using the Informix System Catalogs
                            //  http://www.ibm.com/developerworks/data/zones/informix/library/techarticle/0305parker/0305parker.html
                            //
                            // collength = 836 = 0x344
                            // length = 3
                            // first_qualifier = 4
                            // last_qualifier = 4
                            //
                            if (tableName == "manufact" && schemaColumns[i].ActualName == "lead_time") {
                                cmd.Parameters[i].Value = new IfxTimeSpan(Convert.ToInt32(values[i]), IfxTimeUnit.Day);
                            }
                            else {
                                cmd.Parameters[i].Value = values[i];
                            }
                        }
                        else {
                            cmd.Parameters[i].Value = DBNull.Value;
                        }
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    internal class DatabaseSchemaHelper : Simple.Data.TestHelper.DatabaseSchemaTestsBase
    {
        protected string _connectionString = null;

        public DatabaseSchemaHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override Database GetDatabase()
        {
            return Database.OpenConnection(_connectionString);
        }

        public Ado.Schema.Column[] GetColumns(string tableName)
        {
            return Schema.FindTable(tableName).Columns.ToArray();
        }

        public string[] GetColumnNames(string tableName)
        {
            var columns = Schema.FindTable(tableName).Columns;
            return (from c in columns select c.ActualName).ToArray();
        }
    }
}
