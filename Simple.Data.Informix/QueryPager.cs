using System;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using Simple.Data.Ado;

namespace Simple.Data.Informix
{
    [Export(typeof(IQueryPager))]
    public class QueryPager : IQueryPager
    {
        private static readonly Regex ColumnExtract = new Regex(@"SELECT\s*(.*)\s*(FROM.*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public string ApplyPaging(string sql, string skipParameterName, string takeParameterName)
        {
            var match = ColumnExtract.Match(sql);
            var columns = match.Groups[1].Value.Trim();
            var fromEtc = match.Groups[2].Value.Trim();

            string result = string.Format(
                "select skip {0} first {1} {2} {3}",
                skipParameterName,
                takeParameterName,
                columns,
                fromEtc);

            return result;
        }
    }
}
