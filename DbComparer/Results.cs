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


    public class Results: IResults
    {
        private DataTable ResultsTable {get; set; }

        private const string IdColName = "Id";
        private const string TableNameColName = "TableName";
        private const string MsgTypeColName = "MsgType";
        private const string WhereClauseColName = "WhereClause";
        private const string DifferenceColumnColName = "DifferenceColumn";
        private const string MsgColName = "Msg";

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
            ResultsTable.Columns.Add(MsgTypeColName, typeof(String));
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

        public void AddOneSideOnly(string tableName, ResultTypes msgType,string whereClause, string msg)
        {
            Add(tableName, msgType, whereClause, "", msg);
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

        private void Add(string tableName, ResultTypes msgType, string whereClause, string diffCol, string msg)
        {
            DataRow row = ResultsTable.NewRow();
            row[TableNameColName] = tableName;
            row[MsgTypeColName] = msgType.ToString();
            row[WhereClauseColName] = whereClause;
            row[DifferenceColumnColName] = diffCol;
            row[MsgColName] = msg;
            ResultsTable.Rows.Add(row);
        }

        public IList<String> ToStringList(DataRow[] dataRows)
        {
            IList<String> msgs = new List<String>();

            foreach(DataRow row in dataRows)
            {
                msgs.Add(RowToString(row));
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

        private string RowToString(DataRow row) 
        {
            string id = row[IdColName].ToString();
            string tableName = row[TableNameColName].ToString();
            string msgType = row[MsgTypeColName].ToString();
            string whereClause = row[WhereClauseColName].ToString();
            string msg = row[MsgColName].ToString();

            return $"{id}. [{tableName}] [{msgType.ToString()}] [{whereClause}] - {msg}";
        }

        public DataRow[] GetDifferences(string tableName = null)
        {
            string whereClause = $"{MsgTypeColName}='{ResultTypes.Difference.ToString()}'";
            if(!String.IsNullOrEmpty(tableName))
                whereClause = whereClause + $" AND {TableNameColName}='{tableName}'";

            return  ResultsTable.Select(whereClause);

        }

        public IList<String> GetTablesSkipped()
        {
            return GetUniqueTableNamesForMessageType(ResultTypes.SkippedTable);
        }

        public IList<String> GetTablesProcessed()
        {
            return GetUniqueTableNamesForMessageType(ResultTypes.ProcessingTable);
        }

        public IList<String> GetTablesWithDifferences()
        {
            return GetUniqueTableNamesForMessageType(ResultTypes.Difference);
        }

        public IList<String> GetTablesWithLeftOnly()
        {
            return GetUniqueTableNamesForMessageType(ResultTypes.LeftOnly);
        }

        public IList<String> GetTablesWithRightOnly()
        {
            return GetUniqueTableNamesForMessageType(ResultTypes.RightOnly);
        }

        public IList<String> GetTablesWithErrors()
        {
            return GetUniqueTableNamesForMessageType(ResultTypes.Error);
        }

        public IList<String> GetTablesWithMultipleMatches()
        {
            return GetUniqueTableNamesForMessageType(ResultTypes.MultipleMatches);
        }

        public IList<String> GetTablesWithNoDifferences()
        {
            return GetUniqueTableNamesForMessageType(ResultTypes.NoDifferences);
        }


        private IList<String> GetUniqueTableNamesForMessageType(ResultTypes msgType)
        {
            IList<String> tblNames = new List<String>();

            DataView dv = new DataView(ResultsTable);
            DataTable dt = dv.ToTable(true, MsgTypeColName, TableNameColName);

            string whereClause = $"{MsgTypeColName}='{msgType.ToString()}'";
            DataRow[] rows = dt.Select(whereClause);
            foreach (DataRow row in rows)
            {
                tblNames.Add(row[TableNameColName].ToString());
            }


            return tblNames;
        }
    }
}
