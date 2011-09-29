using System;
using System.Collections.Generic;
using System.Data;
using Simple.Data.Ado.Schema;
using IBM.Data.Informix;

namespace Simple.Data.Informix
{
    internal static class GetTablesHelper
    {
        public static IEnumerable<Table> GetTables(IfxConnection cn)
        {
            var tables = new List<Table>();

            cn.Open();

            var command = cn.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                    select trim(st.tabname), st.tabtype
                    from 'informix'.systables st
                    where st.tabid >= 100
                    and (st.tabtype = 'T' or st.tabtype = 'V')";

            using (var reader = command.ExecuteReader()) {
                for (; reader.Read(); ) {
                    string tabName = reader[0] as string;
                    string tabType = reader[1] as string;
                    tables.Add(new Table(tabName, null, (tabType == "T") ? TableType.Table : TableType.View));
                }
            }

            return tables;
        }
    }
}
