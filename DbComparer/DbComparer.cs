using System;
using System.Collections.Generic;
using System.Data;
using Bentley.OPEF.Database;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bentley.OPEF.Utilities.DbCompare
{
    public enum SourceDb
    {
        Left,
        Right
    }

    public class DbComparer
    {
        private IDatabase Db1 { get; set; }
        private IDatabase Db2 { get; set; }
        private Settings Settings { get; set; }
        private Results Results {get; set; }

        public DbComparer(IDatabase db1, IDatabase db2, Settings settings)
        {
            Db1 = db1;
            Db2 = db2;
            Settings = settings;
        }

        public Results CompareDbs()
        {
            Results = new Results(Db1, Db2, Settings);

            if (Db1 == null || Db2 == null || Settings == null) 
                return Results;

            foreach (TableSettings ts in Settings.TableSettings)
            {
                if (!ts.ProcessTable.GetValueOrDefault(true))
                { 
                    Results.AddSkippedTable(ts.TableName, $"Skipped table {ts.TableName}");
                    continue;
                }

                Results.AddProcessingTable(ts.TableName, $"Processing table {ts.TableName}");

                CompareTable(Db1, Db2, ts);
            }

            return Results;
        }

        private void CompareTable(Database.IDatabase db1, Database.IDatabase db2, TableSettings tblSettings)
        {
            if (db1 == null || db2 == null || tblSettings == null)
                return;

            DataTable dt1 = GetTable(db1, tblSettings.TableName);

            DataTable dt2 = GetTable(db2, tblSettings.TableName);


            if (dt1 == null || dt2 == null)
                return;

            int numDifferences = 0;
            foreach(DataRow row1 in dt1.Rows)
            {
                string whereClause = CreateWhereClause(db1, tblSettings.SelectColumns, row1);
                DataRow row2 = FindMatchingRow(db2, SourceDb.Left, dt2, row1, whereClause, tblSettings);
                if(row2 == null)
                { 
                    numDifferences++;
                    continue;
                }

                var array1 = row1.ItemArray;
                var array2 = row2.ItemArray;

                if (array1.SequenceEqual(array2))
                    continue;

                bool differencesFound = false;
                string diffCols = "";
                string diffMsg = "";
                foreach (DataColumn col1 in dt1.Columns)
                {
                    string colName = col1.ColumnName;
                    if(tblSettings.IgnoreColumns.Contains(colName, StringComparer.InvariantCultureIgnoreCase))
                        continue;

                    if (!dt2.Columns.Contains(colName))
                    {
                        Results.AddError(dt2.TableName, $"Column {col1.ColumnName} not found in {SourceDb.Right.ToString()}");
                        continue;
                    }
                    object val1 = row1[colName];
                    object val2 = row2[colName];

                    if (CompareValues(val1, val2, tblSettings))
                        continue;

                    differencesFound = true;
                    string val1Str = (val1 is DBNull) ? "Null" : $"'{val1.ToString()}'";
                    string val2Str = (val2 is DBNull) ? "Null" : $"'{val2.ToString()}'";

                    diffMsg = $"{diffMsg}[{colName}]: {val1Str} => {val2Str}; ";

                    diffCols = $"{diffCols}{colName};";


                }

                if(differencesFound)
                {
                    numDifferences++;
                    Results.AddDifference(dt1.TableName, whereClause, diffCols.TrimEnd(new char[] { ' ', ';' }), diffMsg.TrimEnd(new char[] {' ', ';'}));
                }
            }

            // Now check for RightOnly rows
            foreach (DataRow row2 in dt2.Rows)
            {
                string whereClause = CreateWhereClause(db2, tblSettings.SelectColumns, row2);
                DataRow row1 = FindMatchingRow(db1, SourceDb.Right, dt1, row2, whereClause, tblSettings, false);
                if(row1 == null)
                    numDifferences++;
            }
            if(numDifferences == 0)
                Results.AddNoDifferences(dt1.TableName, $"No differences found in {dt1.TableName}");
        }

        private bool CompareValues(object obj1, object obj2, TableSettings tblSettings)
        {
            if (obj1 == null || obj2 == null || tblSettings == null)
                return false;

            Type t1 = obj1.GetType();
            Type t2 = obj2.GetType();

            string dataType1 = t1.ToString().ToLower();
            string dataType2 = t2.ToString().ToLower();

            //Whe table is created from another table, Double is changed to a Decimal
            if ((dataType1.Equals(Database.DatabaseConstants.SystemTypeDouble) ||
                dataType2.Equals(Database.DatabaseConstants.SystemTypeDecimal)) &&
                (dataType1.Equals(Database.DatabaseConstants.SystemTypeDouble) ||
                dataType2.Equals(Database.DatabaseConstants.SystemTypeDecimal))
                )
                if (obj1.ToString().Equals(obj2.ToString()))
                    return true;

            if (CompareStrings(obj1, obj2, tblSettings))
                return true;

            if (!t1.Equals(t2))
                return false;

            if (t1.IsPrimitive || typeof(string).Equals(t1))
                return obj1.Equals(obj2);

            return true;
        }

        private bool CompareStrings(object obj1, object obj2, TableSettings tblSettings)
        {
            string val1;
            string val2;

            if((obj1 is DBNull) && tblSettings.TreatNullAsEmptyString.GetValueOrDefault(false))
                val1 = String.Empty;
            else if (obj1 is String)
                val1 = obj1.ToString();
            else
                return false;
                
            if ((obj2 is DBNull) && tblSettings.TreatNullAsEmptyString.GetValueOrDefault(false))
                val2 = String.Empty;
            else if (obj2 is String)
                val2 = obj2.ToString();
            else
                return false;

            if(tblSettings.TrimValues.GetValueOrDefault(false))
            {
                val1 = val1.Trim();
                val2 = val2.Trim();
            }

            if(tblSettings.IgnoreCase.GetValueOrDefault(false))
                return val1.Equals(val2, StringComparison.InvariantCultureIgnoreCase);

            return val1.Equals(val2);
        }


        private DataRow FindMatchingRow(Database.IDatabase db, SourceDb source, DataTable dt,  DataRow sourceRow, 
                                        string whereClause, TableSettings tblSettings, bool reportMultMatches=true)
        {
            if(dt == null || sourceRow == null || String.IsNullOrEmpty(whereClause) || tblSettings == null)
                return null;

            DataRow[] results = dt.Select(whereClause);
            IList<String> allColumns = GetColumnNames(dt);
            if (results.Count() < 1)
            {
                Results.AddOneSideOnly(dt.TableName, source == SourceDb.Left ? ResultTypes.LeftOnly : ResultTypes.RightOnly, whereClause,
                           CreateMessage(allColumns, sourceRow));
                return null;
            }
            else if (results.Count() == 1)
            {
                return results[0];
            }
            else
            {
                Results.AddMultipleMatches(dt.TableName, whereClause,
                           CreateMessage(tblSettings.SelectColumns, sourceRow));

                //ToDo: How should we handle this case?
                //return results[0];
                return null;
            }

        }

        private DataTable GetTable(Database.IDatabase db, string tableName)
        {
            if (db == null || tableName == null)
                return null;

            if (!db.TableExists(tableName))
            {
                Results.AddError(tableName, $"Table {tableName} not found in {db.ToString()}");
                return null;
            }

            string sql = $"SELECT * FROM {tableName}";

            DataTable dt = db.GetDataTable(sql);
            if(dt == null)
            {
                Results.AddError(tableName, $"Unable to get table {tableName} in {db.ToString()}");
            }

            return dt;

        }

        private string CreateWhereClause(Database.IDatabase db, IList<string> selectCols, DataRow row)
        {
            if(db == null || selectCols == null || row == null)
                return null;

            string where = string.Empty;
            foreach (string selCol in selectCols)
            {
                object val = null;
                try
                {
                   val = row[selCol];
                }
                catch
                {
                    Results.AddError("", $"Invalid Select Column specified: {selCol}");
                }
                if(val == null)
                    continue;

                string safeColName = selCol;
                string colName = db.IsKeyWord(selCol, out safeColName) ? safeColName : selCol;
                string clause = ((val is DBNull) || (val == null)) ? $"{colName} is null" : $"{colName} = '{val.ToString()}'";

                if (string.IsNullOrEmpty(where))
                    where = clause;
                else
                    where = string.Format("{0} AND {1}", where, clause);
            }

            return where;
        }

        private string CreateMessage(IList<string> cols, DataRow row)
        {
            string msg = String.Empty;

            foreach (string col in cols)
            {
                object val = null;
                try
                {
                    val = row[col];
                }
                catch
                {
                    //Logger.Add("", LogMessageType.Error, "", $"Invalid Column specified: {col}");
                }
                if (val == null)
                    continue;

                msg += $"{col}='{val.ToString()}', ";
            }

            return msg.TrimEnd( new char[] {',',' '});
        }

        private IList<String> GetColumnNames(DataTable dt)
        {
            IList<String> colNames = new List<String>();
            if (dt == null)
                return colNames;

            DataColumnCollection dcc = dt.Columns;
            foreach (DataColumn dc in dcc)
            {
                colNames.Add(dc.ColumnName);
            }

            return colNames;

        }



    }
}
