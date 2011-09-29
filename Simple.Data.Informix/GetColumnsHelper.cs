using System;
using System.Collections.Generic;
using System.Data;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using IBM.Data.Informix;
using System.Linq;

namespace Simple.Data.Informix
{
    internal static class GetColumnsHelper
    {
        public static IEnumerable<Column> GetColumns(Table table, IfxConnection cn)
        {
            cn.Open();

            var command = cn.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = @"
                select trim(sc.colname), sc.coltype, sc.collength
                from 'informix'.systables st, 'informix'.syscolumns sc
                where st.tabname = ?
                and (st.tabtype = 'T' or st.tabtype = 'V')
                and st.tabid >= 100
                and st.tabid = sc.tabid";
            command.Parameters.Add("st.tabname", table.ActualName);

            var informixColumnInfos = new List<InformixColumnInfo>();

            using (var reader = command.ExecuteReader()) {
                for (; reader.Read(); ) {
                    string colName = reader[0] as string;
                    int colType = Convert.ToInt32(reader[1]);
                    int colLength = Convert.ToInt32(reader[2]);
                    var informixColumnInfo = InformixColumnInfoCreator.CreateColumnInfo(
                        colName,
                        colType,
                        colLength);
                    informixColumnInfos.Add(informixColumnInfo);
                }
            }

            var results =
                from c in informixColumnInfos
                select new Column(c.Name, table, c.IsAutoincrement, c.DbType, c.Capacity);

            return results;
        }
    }
}
