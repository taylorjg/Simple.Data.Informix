using System;
using System.Data;
using System.Linq;
using IBM.Data.Informix;
using Simple.Data.TestHelper;

namespace Simple.Data.Informix.Tests
{
    internal static class DatabaseHelper
    {
        public static dynamic Open()
        {
            return Database.Opener.OpenConnection(Properties.Settings.Default.ConnectionString_V7);
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
                //DeleteRecords(cn, "manufact");
                DeleteRecords(cn, "customer");

                LoadRecords(cn, connectionString, "customer");
                //LoadRecords(cn, connectionString, "manufact");
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
            var columnNames = databaseSchemaHelper.GetColumnNames(tableName);
            int numColumns = columnNames.Length;
            string joinedColumnNames = string.Join(", ", columnNames);

            // Load the .unl file (renamed to .txt and added as a resource file).
            string fileContents = Resources.Resources.ResourceManager.GetString(tableName);
            string[] lines = fileContents.Split(new[] { "\r\n" }, StringSplitOptions.None);

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

                    if (values.Length - 1 != numColumns) {
                        break;
                    }

                    for (int i = 0; i < numColumns; i++) {
                        if (!string.IsNullOrEmpty(values[i])) {
                            cmd.Parameters[i].Value = values[i];
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

    internal class DatabaseSchemaHelper : DatabaseSchemaTestsBase
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
