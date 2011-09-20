using System;
using System.Collections.Generic;
using System.Data;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using IBM.Data.Informix;
using System.Linq;

namespace Simple.Data.Informix
{
    internal static class GetForeignKeysHelper
    {
        private class ForeignKeyQueryInfo
        {
            public ForeignKeyQueryInfo()
            {
                FKParts = new List<short>();
                FKColumnNames = new List<string>();
                PKParts = new List<short>();
                PKColumnNames = new List<string>();
            }

            public string FKTabName { get; set; }
            public int FKTabId { get; set; }
            public List<short> FKParts { get; private set; }
            public List<string> FKColumnNames { get; private set; }
            public string PKTabName { get; set; }
            public int PKTabId { get; set; }
            public List<short> PKParts { get; private set; }
            public List<string> PKColumnNames { get; private set; }
            public int PKConstraintId { get; set; }
        }

        public static IEnumerable<ForeignKey> GetForeignKeys(Table table, IfxConnection cn)
        {
            var fkqis = new List<ForeignKeyQueryInfo>();

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
                    si.part13, si.part14, si.part15, si.part16, 
                    trim(st.tabname) as fktabname,
                    si.tabid as fktabid,
                    trim(rt.tabname) as pktabname,
                    rc.tabid as pktabid,
                    sr.primary as pkconstraintid
                    from
                    'informix'.systables st,
                    'informix'.sysconstraints sc,
                    'informix'.sysindexes si,
                    'informix'.sysreferences sr,
                    'informix'.systables rt,
                    'informix'.sysconstraints rc
                    where
                    st.tabid = ?
                    and st.tabid = sc.tabid
                    and sc.constrtype = 'R'
                    and sc.constrid = sr.constrid
                    and sc.tabid = si.tabid
                    and sc.idxname = si.idxname
                    and rt.tabid = sr.ptabid
                    and rc.tabid = sr.ptabid
                    and sr.primary = rc.constrid";
                command.Parameters.Clear();
                command.Parameters.Add("st.tabid", tabid);

                using (var reader = command.ExecuteReader()) {
                    for (; reader.Read(); ) {
                        var fkqi = new ForeignKeyQueryInfo();
                        fkqi.FKTabName = reader["fktabname"] as string;
                        fkqi.FKTabId = (int)reader["fktabid"];
                        fkqi.PKTabName = reader["pktabname"] as string;
                        fkqi.PKTabId = (int)reader["pktabid"];
                        fkqi.PKConstraintId = (int)reader["pkconstraintid"];
                        for (int i = 0; i < 16; i++) {
                            short part = (short)reader.GetValue(i);
                            if (part == 0) break;
                            fkqi.FKParts.Add(part);
                        }
                        fkqis.Add(fkqi);
                    }
                }

                command.CommandText = @"
                    select
                    part1,  part2,  part3,  part4,
                    part5,  part6,  part7,  part8,
                    part9,  part10, part11, part12,
                    part13, part14, part15, part16
                    from
                    'informix'.sysindexes si,
                    'informix'.sysconstraints sc
                    where
                    si.tabid = sc.tabid
                    and si.idxname = sc.idxname
                    and sc.constrid = ?";
                command.Parameters.Clear();
                command.Parameters.Add("sc.constrid", null);

                foreach (var fkqi in fkqis) {
                    command.Parameters["sc.constrid"].Value = fkqi.PKConstraintId;
                    using (var reader = command.ExecuteReader()) {
                        for (; reader.Read(); ) {
                            for (int i = 0; i < 16; i++) {
                                short part = (short)reader.GetValue(i);
                                if (part == 0) break;
                                fkqi.PKParts.Add(part);
                            }
                        }
                    }
                }

                command.CommandText = "select trim(sc.colname) from 'informix'.syscolumns sc where sc.tabid = ? and sc.colno = ?";
                command.Parameters.Clear();
                command.Parameters.Add("sc.tabid", null);
                command.Parameters.Add("sc.colno", null);

                foreach (var fkqi in fkqis) {

                    command.Parameters["sc.tabid"].Value = fkqi.FKTabId;
                    foreach (short part in fkqi.FKParts) {
                        command.Parameters["sc.colno"].Value = part;
                        string columnName = command.ExecuteScalar() as string;
                        fkqi.FKColumnNames.Add(columnName);
                    }

                    command.Parameters["sc.tabid"].Value = fkqi.PKTabId;
                    foreach (short part in fkqi.PKParts) {
                        command.Parameters["sc.colno"].Value = part;
                        string columnName = command.ExecuteScalar() as string;
                        fkqi.PKColumnNames.Add(columnName);
                    }
                }
            }

            var results =
                from fkqi in fkqis
                select new ForeignKey(
                    detailTable: new ObjectName(null, fkqi.FKTabName),
                    columns: fkqi.FKColumnNames,
                    masterTable: new ObjectName(null, fkqi.PKTabName),
                    masterColumns: fkqi.PKColumnNames);

            return results.AsEnumerable();
        }
    }
}
