using System;
using System.Collections.Generic;
using System.Data;
using Bentley.OPEF.Database;
using System.Linq;
using System.Text;
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

        public string ToHTML()
        {
            StringBuilder html = new StringBuilder();

            html.Append($"<span class='tableNameLabel'>Table Name:</span><span class='tableName'>{TableName}</span><br />");
            html.Append(RowDifferencesToHTML());
            html.Append(OneSideOnlyToHTML(LeftOnly, "Left Side Only"));
            html.Append(OneSideOnlyToHTML(RightOnly, "Right Side Only"));

            return html.ToString();
        }

        private string RowDifferencesToHTML()
        {
            StringBuilder html = new StringBuilder();

            if (RowDifferences.Count > 0)
            {
                html.Append("<div class=differences>");
                html.Append($"<span class='differencesLabel'>Differences:</span><br />");

                html.Append($"<table class='differencesTable'");
                foreach (RowDifference rowDiff in RowDifferences)
                {
                    html.Append("<tr>");
                    foreach (DataColumn col in rowDiff.LeftRow.Table.Columns)
                    {
                        html.Append($"<th>{col.ColumnName}</th>");
                    }
                    html.Append("</tr>");

                    html.Append("<tr>");
                    foreach (DataColumn col in rowDiff.LeftRow.Table.Columns)
                    {
                        if (rowDiff.DiffCols.Contains(col.ColumnName))
                            html.Append($"<td class=valueDiff>{rowDiff.LeftRow[col].ToString()}</td>");
                        else
                            html.Append($"<td>{rowDiff.LeftRow[col].ToString()}</td>");
                    }
                    html.Append("</tr>");

                    html.Append("<tr>");
                    foreach (DataColumn col in rowDiff.RightRow.Table.Columns)
                    {
                        if (rowDiff.DiffCols.Contains(col.ColumnName))
                            html.Append($"<td class=valueDiff>{rowDiff.RightRow[col].ToString()}</td>");
                        else
                            html.Append($"<td>{rowDiff.RightRow[col].ToString()}</td>");
                    }
                    html.Append("</tr>");
                    html.Append("<tr>");
                    foreach (DataColumn col in rowDiff.RightRow.Table.Columns)
                    {
                        html.Append($"<td class=rowSpacer></td>");
                    }
                    html.Append("</tr>");
                }
                html.Append($"</table>");
                html.Append("</div>");
            }

            return html.ToString();
        }

        private string OneSideOnlyToHTML(DataTable side, string label)
        {
            StringBuilder html = new StringBuilder();

            if (side.Rows.Count > 0)
            {
                html.Append("<div class=oneSideOnly>");
                html.Append($"<span class='oneSideOnlyLabel'>{label}:</span><br />");

                html.Append($"<table class='oneSideOnlyTable'");
                bool firstRow = true;
                foreach (DataRow row in side.Rows)
                {
                    if(firstRow)
                    {
                        html.Append("<tr>");
                        foreach (DataColumn col in side.Columns)
                        {
                            html.Append($"<th>{col.ColumnName}</th>");
                        }
                        html.Append("</tr>");
                        firstRow = false;
                    }

                    html.Append("<tr>");
                    foreach (DataColumn col in side.Columns)
                    {
                        html.Append($"<td>{row[col].ToString()}</td>");
                    }
                    html.Append("</tr>");
                }
                html.Append($"</table>");
                html.Append("</div>");
            }

            return html.ToString();
        }


    }
}
