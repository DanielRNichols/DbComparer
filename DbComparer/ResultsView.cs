using System;
using System.Collections.Generic;
using System.Data;
using Bentley.OPEF.Database;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;

namespace Bentley.OPEF.Utilities.DbCompare
{

    public class RowDifference
    {
        public DataRow LeftRow { get; set; }
        public DataRow RightRow { get; set; }
        public string[] DiffCols { get; set; }
    }

    public class ResultsView
    {
        public string TableName { get; private set;}
        public IList<RowDifference> RowDifferences { get; private set; }
        public DataTable LeftOnly { get; private set; }
        public DataTable RightOnly { get; private set; }

        public ResultsView(Results results, IDatabase db1, IDatabase db2, string tableName)
        {
            if(results == null || db1 == null || db2 == null || String.IsNullOrEmpty(tableName))
                return;

            TableName = tableName;
            DataTable dt1 = GetTable(db1, tableName);
            DataTable dt2 = GetTable(db2, tableName);
            if (dt1 == null || dt2 == null)
                return;

            RowDifferences = GetRowDifferences(results, dt1, dt2);
            LeftOnly = GetOneSideOnly(results, dt1, ResultTypes.LeftOnly);
            RightOnly = GetOneSideOnly(results, dt2, ResultTypes.RightOnly);

        }

        private IList<RowDifference> GetRowDifferences(Results results, DataTable leftDt, DataTable rightDt)
        {
            IList<RowDifference> rowDiffs = new List<RowDifference>();

            DataRow[] rows = results.GetMatchingRows(leftDt.TableName, ResultTypes.Difference);

            foreach(DataRow row in rows)
            {
                string whereClause = row[Results.WhereClauseColName].ToString();
                DataRow[] leftRows = leftDt.Select(whereClause);
                DataRow[] rightRows = rightDt.Select(whereClause);

                if (leftRows == null || leftRows.Count() == 0 ||
                    rightRows == null || rightRows.Count() == 0
                    )
                    continue;

                RowDifference rd = new RowDifference();
                rd.LeftRow = leftRows[0];
                rd.RightRow = rightRows[0];
                rd.DiffCols = row[Results.DifferenceColumnColName].ToString().Split(';');

                rowDiffs.Add(rd);
            }

            return rowDiffs;
        }

        private DataTable GetOneSideOnly(Results results, DataTable dt, ResultTypes side)
        {
            if (results == null || dt == null)
                return null;


            DataTable dtRet = dt.Clone();

            IList<String> whereClauses = results.GetWhereClausesForTableAndType(dt.TableName, side);

            foreach(string whereClause in whereClauses)
            {
                DataRow[] rows = dt.Select(whereClause);
                if(rows == null || rows.Count() == 0)
                    continue;
                foreach(DataRow row in rows)
                { 
                    dtRet.Rows.Add(row.ItemArray);
                }
            }


            return dtRet;

        }


        private DataTable GetTable(Database.IDatabase db, string tableName)
        {
            if (db == null || String.IsNullOrEmpty(tableName))
                return null;

            if (!db.TableExists(tableName))
                return null;

            string sql = $"SELECT * FROM {tableName}";

            DataTable dt = db.GetDataTable(sql);

            return dt;
        }


    }
}
