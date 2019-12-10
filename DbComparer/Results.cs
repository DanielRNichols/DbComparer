using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bentley.OPEF.Utilities.DbCompare
{
    public enum ResultTypes
    {
        ProcessingTable,
        SkippedTable,
        Difference,
        LeftOnly,
        RightOnly,
        NoDifferences,
        MultipleMatches,
        Error

    }


    public class Results
    {
        private DataTable ResultsTable {get; set; }

        public const string IdColName = "Id";
        public const string TableNameColName = "TableName";
        public const string EntryTypeColName = "EntryType";
        public const string WhereClauseColName = "WhereClause";
        public const string DifferenceColumnColName = "DifferenceColumn";
        public const string MsgColName = "Msg";

        public Results()
        {
            Clear();
        }

        public void Clear()
        {
            ResultsTable = new DataTable("Results");

            DataColumn idCol = ResultsTable.Columns.Add(IdColName, typeof(Int32));
            idCol.AutoIncrement = true;
            idCol.AutoIncrementSeed = 1;

            ResultsTable.Columns.Add(TableNameColName, typeof(String));
            ResultsTable.Columns.Add(EntryTypeColName, typeof(String));
            ResultsTable.Columns.Add(WhereClauseColName, typeof(String));
            ResultsTable.Columns.Add(DifferenceColumnColName, typeof(String));
            ResultsTable.Columns.Add(MsgColName, typeof(String));

        }

        public void AddProcessingTable(string tableName, string msg)
        {
            Add(tableName, ResultTypes.ProcessingTable, "", "", msg);
        }

        public void AddSkippedTable(string tableName, string msg)
        {
            Add(tableName, ResultTypes.SkippedTable, "", "", msg);
        }

        public void AddNoDifferences(string tableName, string msg)
        {
            Add(tableName, ResultTypes.NoDifferences, "", "", msg);
        }

        public void AddDifference(string tableName, string whereClause, string diffCol, string msg)
        {
            Add(tableName, ResultTypes.Difference, whereClause, diffCol, msg);
        }

        public void AddOneSideOnly(string tableName, ResultTypes entryType,string whereClause, string msg)
        {
            Add(tableName, entryType, whereClause, "", msg);
        }

        public void AddLeftOnly(string tableName, string whereClause, string msg)
        {
            Add(tableName, ResultTypes.LeftOnly, whereClause, "", msg);
        }

        public void AddRightOnly(string tableName, string whereClause, string msg)
        {
            Add(tableName, ResultTypes.RightOnly, whereClause, "", msg);
        }

        public void AddMultipleMatches(string tableName, string whereClause, string msg)
        {
            Add(tableName, ResultTypes.MultipleMatches, whereClause, "", msg);
        }

        public void AddError(string tableName, string msg)
        {
            Add(tableName, ResultTypes.Error, "", "", msg);
        }

        private void Add(string tableName, ResultTypes entryType, string whereClause, string diffCol, string msg)
        {
            DataRow row = ResultsTable.NewRow();
            row[TableNameColName] = tableName;
            row[EntryTypeColName] = entryType.ToString();
            row[WhereClauseColName] = whereClause;
            row[DifferenceColumnColName] = diffCol;
            row[MsgColName] = msg;
            ResultsTable.Rows.Add(row);
        }

        public IList<String> ToStringList(ResultTypes entryType)
        {
            IList<String> msgs = new List<String>();

            string whereClause = $"{EntryTypeColName}='{entryType.ToString()}'";
            DataRow[] rows = ResultsTable.Select(whereClause);
            foreach (DataRow row in rows)
            {
                msgs.Add(RowToString(row, false));
            }

            return msgs;
        }

        public IList<String> ToStringList()
        {
            IList<String> msgs = new List<String>();

            foreach (DataRow row in ResultsTable.Rows)
            {
                msgs.Add(RowToString(row));
            }

            return msgs;
        }

        private string RowToString(DataRow row, bool includeId=true) 
        {
            string idStr = includeId ? $"{row[IdColName].ToString()}. ": "";
            string tableName = row[TableNameColName].ToString();
            string entryType = row[EntryTypeColName].ToString();
            string whereClause = row[WhereClauseColName].ToString();
            string msg = row[MsgColName].ToString();

            return $"{idStr}[{tableName}] [{entryType.ToString()}] [{whereClause}] - {msg}";
        }

        public DataRow[] GetDifferences(string tableName = null)
        {
            string whereClause = $"{EntryTypeColName}='{ResultTypes.Difference.ToString()}'";
            if(!String.IsNullOrEmpty(tableName))
                whereClause += $" AND {TableNameColName}='{tableName}'";

            return  ResultsTable.Select(whereClause);

        }

        public IList<String> GetTablesSkipped()
        {
            return GetUniqueTableNamesForType(ResultTypes.SkippedTable);
        }

        public IList<String> GetTablesProcessed()
        {
            return GetUniqueTableNamesForType(ResultTypes.ProcessingTable);
        }

        public IList<String> GetTablesWithDifferences()
        {
            return GetUniqueTableNamesForType(ResultTypes.Difference, ResultTypes.LeftOnly, ResultTypes.RightOnly);
        }

        public IList<String> GetTablesWithRowDifferences()
        {
            return GetUniqueTableNamesForType(ResultTypes.Difference);
        }

        public IList<String> GetTablesWithLeftOnly()
        {
            return GetUniqueTableNamesForType(ResultTypes.LeftOnly);
        }

        public IList<String> GetTablesWithRightOnly()
        {
            return GetUniqueTableNamesForType(ResultTypes.RightOnly);
        }

        public IList<String> GetTablesWithErrors()
        {
            return GetUniqueTableNamesForType(ResultTypes.Error);
        }

        public IList<String> GetTablesWithMultipleMatches()
        {
            return GetUniqueTableNamesForType(ResultTypes.MultipleMatches);
        }

        public IList<String> GetTablesWithNoDifferences()
        {
            return GetUniqueTableNamesForType(ResultTypes.NoDifferences);
        }


        private IList<String> GetUniqueTableNamesForType(params ResultTypes[] entryTypes)
        {
            IList<String> tblNames = new List<String>();

            // Use dv to get distinct values
            DataView dv = new DataView(ResultsTable);
            DataTable dt = dv.ToTable(true, EntryTypeColName, TableNameColName);

            string whereClause = null;
            foreach (ResultTypes entryType in entryTypes)
            { 
                if(String.IsNullOrEmpty(whereClause))
                    whereClause = $"{EntryTypeColName}='{entryType.ToString()}'";
                else
                    whereClause = $"{whereClause} OR {EntryTypeColName}='{entryType.ToString()}'";
            }
            DataRow[] rows = dt.Select(whereClause, $"{TableNameColName} ASC");
            foreach (DataRow row in rows)
            {
                string tblName = row[TableNameColName].ToString();
                if (!tblNames.Contains(tblName))
                    tblNames.Add(tblName);
            }


            return tblNames;
        }

        public DataRow[] GetMatchingRows(string tableName, ResultTypes entryType)
        {
            if(String.IsNullOrEmpty(tableName))
                tableName = "";

            string filter = $"{TableNameColName}='{tableName}' AND {EntryTypeColName}='{entryType.ToString()}'";
            return ResultsTable.Select(filter);
        }

        public IList<String> GetWhereClausesForTableAndType(string tableName, ResultTypes entryType)
        {
            IList<String> whereClauses = new List<String>();

            DataRow[] rows = GetMatchingRows(tableName, entryType);
            if(rows.Count() < 1)
                return whereClauses;

            foreach(DataRow row in rows)
            {
                try
                {
                    string whereClause = row[WhereClauseColName].ToString();
                    whereClauses.Add(whereClause);
                }
                catch
                {
                }
            }

            return whereClauses;
        }
    }
}
