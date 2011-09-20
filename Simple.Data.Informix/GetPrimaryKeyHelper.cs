using System;
using System.Collections.Generic;
using System.Data;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using IBM.Data.Informix;
using System.Linq;

namespace Simple.Data.Informix
{
    internal static class GetPrimaryKeyHelper
    {
        // http://www.ibm.com/developerworks/data/zones/informix/library/techarticle/0305parker/0305parker.html
        // http://stackoverflow.com/questions/320045/how-do-i-get-constraint-details-from-the-name-in-informix
        public static Key GetPrimaryKey(Table table, IfxConnection cn)
        {
            var columnNames = new List<string>();

            cn.Open();

            var command = cn.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "select st.tabid from 'informix'.systables st where st.tabname = ?";
            command.Parameters.Add("st.tabname", table.ActualName);
            int? tabid = null;
            try {
                tabid = command.ExecuteScalar() as int?;
            }
            catch (Exception) {
            }

            if (tabid.HasValue) {

                command.CommandText = @"
                    select
                    si.part1,  si.part2,  si.part3,  si.part4,
                    si.part5,  si.part6,  si.part7,  si.part8,
                    si.part9,  si.part10, si.part11, si.part12,
                    si.part13, si.part14, si.part15, si.part16
                    from
                    'informix'.sysconstraints sc,
                    'informix'.sysindexes si
                    where
                    sc.tabid = ?
                    and sc.constrtype = 'P'
                    and si.tabid = sc.tabid
                    and si.idxname = sc.idxname";
                command.Parameters.Clear();
                command.Parameters.Add("sc.tabid", tabid);

                var parts = new List<short>();

                using (var reader = command.ExecuteReader()) {
                    if (reader.Read()) {
                        for (int i = 0; i < 16; i++) {
                            short part = (short)reader.GetValue(i);
                            if (part == 0) break;
                            parts.Add(part);
                        }
                    }
                }

                command.CommandText = "select trim(sc.colname) from 'informix'.syscolumns sc where sc.tabid = ? and sc.colno = ?";
                command.Parameters.Clear();
                command.Parameters.Add("sc.tabid", tabid);
                command.Parameters.Add("sc.colno", null);

                foreach (short part in parts) {
                    command.Parameters["sc.colno"].Value = part;
                    string columnName = command.ExecuteScalar() as string;
                    columnNames.Add(columnName);
                }
            }

            return new Key(columnNames);
        }
    }
}
