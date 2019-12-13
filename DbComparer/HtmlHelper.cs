using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Bentley.OPEF.Database;
using System.Web;
using HtmlAgilityPack;

namespace Bentley.OPEF.Utilities.DbCompare
{
    public class HtmlHelper
    {

        public enum ToHTMLStatus
        {
            UnableToLoadTemplate,
            UnableToLocateNode,
            Success
        }

        public ToHTMLStatus ToHTMLFile(string htmlOutput, string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode dbCompareResultsNode = doc.GetElementbyId("resultsView");
            if (dbCompareResultsNode == null)
            {
                return ToHTMLStatus.UnableToLocateNode;
            }

            dbCompareResultsNode.InnerHtml = html.ToString();

            doc.Save(htmlOutput);

            return ToHTMLStatus.Success;
        }

        public string ApplyTemplate(string htmlTemplate, string html)
        {
            if (String.IsNullOrEmpty(htmlTemplate) || !System.IO.File.Exists(htmlTemplate))
            {
                return null;
            }

            HtmlDocument doc = new HtmlDocument();
            doc.Load(htmlTemplate);

            HtmlNode dbCompareResultsNode = doc.GetElementbyId("resultsView");
            if (dbCompareResultsNode == null)
            {
                return null;
            }

            dbCompareResultsNode.InnerHtml = html;

            var sb = new StringBuilder();
            using (var writer = new System.IO.StringWriter(sb))
            {
                doc.Save(writer);
            }
                
            return sb.ToString();

        }

        public string DbInfoToHTML(IDatabase db, string label)
        {
            return $"<span class='dbNameLabel'>{label}:</span><span class='dbName'>{db.Connection.ConnectionString}</span><br />";
        }

        public string ResultsViewToHTML(ResultsView rv, TableSettings ts)
        {
            StringBuilder html = new StringBuilder();

            html.Append($"<span class='tableNameLabel'>Table Name:</span><span class='tableName'>{rv.TableName}</span><br />");
            html.Append(RowDifferencesToHTML(rv.RowDifferences, ts));
            html.Append(OneSideOnlyToHTML(rv.LeftOnly, "Left Side Only"));
            html.Append(OneSideOnlyToHTML(rv.RightOnly, "Right Side Only"));
            html.Append("<hr/>");

            return html.ToString();
        }

        private string RowDifferencesToHTML(IList<RowDifference> rowDiffs, TableSettings ts)
        {
            StringBuilder html = new StringBuilder();

            if (rowDiffs.Count > 0)
            {
                html.Append("<div class=differences>");
                html.Append($"<span class='tableLabel'>Differences ({rowDiffs.Count}):</span><br/>");

                html.Append($"<table class='differencesTable'>");
                foreach (RowDifference rowDiff in rowDiffs)
                {
                    IList<String> colNames = GetColumnNames(rowDiff.LeftRow, rowDiff.RightRow);
                    html.Append(GetHtmlHeader(colNames));
                    html.Append(GetHtmlRowDiff(rowDiff.LeftRow, "leftRow", colNames, rowDiff.DiffCols, ts.IgnoreColumns));
                    html.Append(GetHtmlRowDiff(rowDiff.RightRow, "rightRow", colNames, rowDiff.DiffCols, ts.IgnoreColumns));
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
                html.Append($"<span class='tableLabel'>{label} ({side.Rows.Count}):</span><br/>");

                html.Append($"<table class='oneSideOnlyTable'>");
                bool firstRow = true;
                foreach (DataRow row in side.Rows)
                {
                    if (firstRow)
                    {
                        IList<String> columnNames = GetColumnNames(side.Columns);
                        html.Append(GetHtmlHeader(columnNames));
                        firstRow = false;
                    }

                    html.Append(GetHtmlRow(row));

                }
                html.Append($"</table>");
                html.Append("</div>");
            }

            return html.ToString();
        }

        private string GetHtmlHeader(IList<String> colNames)
        {
            StringBuilder html = new StringBuilder();

            html.Append("<tr>");
            foreach (string col in colNames)
            {
                html.Append($"<th>{col}</th>");
            }
            html.Append("</tr>");

            return html.ToString();
        }

        private string GetHtmlRowDiff(DataRow row, string className, IList<String> colNames, IList<String> diffCols, IList<String> ignoreCols)
        {
            StringBuilder html = new StringBuilder();
            html.Append($"<tr class={className}>");
            foreach (string colName in colNames)
            {
               html.Append(GetHtmlRowDiffCell(row, colName, diffCols, ignoreCols));
            }
            html.Append("</tr>");

            return html.ToString();
        }

        private string GetHtmlRowDiffCell(DataRow row, string colName, IList<String> diffCols, IList<String> ignoreCols)
        {
            if(row == null || String.IsNullOrEmpty(colName))
            {
                return "<td></td>";
            }

            StringBuilder html = new StringBuilder();
            DataColumn col = row.Table.Columns.Contains(colName) ? row.Table.Columns[colName] : null;
            
            string htmlVal = (col != null) ? GetHtmlValue(row, col) : "";
            string classStr = diffCols.Contains(colName) ? " class=valueDiff" :
                              ignoreCols.Contains(colName) ? " class=ignoreCol" : "";
            html.Append($"<td{classStr}>{htmlVal}</td>");

            return html.ToString();
        }

        private string GetHtmlRow(DataRow row)
        {
            StringBuilder html = new StringBuilder();

            html.Append("<tr>");
            foreach (DataColumn col in row.Table.Columns)
            {
                string htmlVal = GetHtmlValue(row, col);
                html.Append($"<td>{htmlVal}</td>");
            }
            html.Append("</tr>");

            return html.ToString();
        }

        private string GetHtmlValue(DataRow row, DataColumn col)
        {
            string val = String.Empty;
            try
            {
                if (col.DataType.Equals(typeof(byte[])))
                    val = "BLOB";
                else
                    val = row[col].ToString();
                return HttpUtility.HtmlEncode(val);
            }
            catch
            {
                return String.Empty;
            }
        }

        private IList<String> GetColumnNames(DataRow leftRow, DataRow rightRow)
        {
            IList<String> colNames = GetColumnNames(leftRow);
            foreach (DataColumn col in rightRow.Table.Columns)
            {
                if(!colNames.Contains(col.ColumnName))
                    colNames.Add(col.ColumnName);
            }

            return colNames;
        }

        private IList<String> GetColumnNames(DataRow row)
        {
            return GetColumnNames(row.Table.Columns);
        }

        private IList<String> GetColumnNames(DataColumnCollection cols)
        {
            IList<String> colNames = new List<String>();
            foreach (DataColumn col in cols)
            {
                colNames.Add(col.ColumnName);
            }
            return colNames;
        }
    }
}
