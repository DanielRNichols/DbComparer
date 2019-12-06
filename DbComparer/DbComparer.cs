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
        public Database.DatabaseType DbType { get; set; }
        private ILogger Logger {get; set; }

        public DbComparer()
        {
        }

        public void CompareDbs(String dbName1, String dbName2, Settings settings, ILogger logger)
        {
            Logger = logger;
            Logger.Clear();

            Database.IDatabase db1 = Connect(dbName1);
            if(db1 == null)
                return;

            Database.IDatabase db2 = Connect(dbName2);
            if (db2 == null)
                return;

            foreach (TableSettings ts in settings.TableSettings)
            {
                if (!ts.ProcessTable.GetValueOrDefault(true))
                { 
                    Logger.Add(ts.TableName, LogMessageType.SkippedTable, $"Skipped table {ts.TableName}");
                    continue;
                }

                Logger.Add(ts.TableName, LogMessageType.ProcessingTable, $"Processing table {ts.TableName}");
                ts.TreatNullAsEmptyString = ts.TreatNullAsEmptyString.GetValueOrDefault(settings.GlobalSettings.TreatNullAsEmptyString.GetValueOrDefault(false));
                ts.IgnoreColumns = ts.IgnoreColumns == null ? settings.GlobalSettings.IgnoreColumns : ts.IgnoreColumns;
                ts.IgnoreCase = ts.IgnoreCase == null ? settings.GlobalSettings.IgnoreCase : ts.IgnoreCase;
                ts.TrimValues = ts.TrimValues == null ? settings.GlobalSettings.TrimValues : ts.TrimValues;

                CompareTable(db1, db2, ts);
            }



            return;
        }

        private void CompareTable(Database.IDatabase db1, Database.IDatabase db2, TableSettings tblSettings)
        {
            DataTable dt1 = GetTable(db1, tblSettings.TableName);
            DataTable dt2 = GetTable(db2, tblSettings.TableName);
            if (dt1 == null || dt2 == null)
                return;

            foreach(DataRow row1 in dt1.Rows)
            {
                DataRow row2 = FindMatchingRow(db2, SourceDb.Left, dt2, row1, tblSettings);
                if(row2 == null)
                    continue;

                var array1 = row1.ItemArray;
                var array2 = row2.ItemArray;

                if (array1.SequenceEqual(array2))
                    continue;

                foreach (DataColumn col1 in dt1.Columns)
                {
                    string colName = col1.ColumnName;
                    if(tblSettings.IgnoreColumns.Contains(colName, StringComparer.InvariantCultureIgnoreCase))
                        continue;

                    if (!dt2.Columns.Contains(colName))
                    {
                        Logger.Add(dt2.TableName, LogMessageType.Error, $"Column {col1.ColumnName} not found in {SourceDb.Right.ToString()}");
                        continue;
                    }
                    object val1 = row1[colName];
                    object val2 = row2[colName];

                    if (CompareValues(val1, val2, tblSettings))
                        continue;

                    string val1Str = (val1 is DBNull) ? "Null" : $"'{val1.ToString()}'";
                    string val2Str = (val2 is DBNull) ? "Null" : $"'{val2.ToString()}'";

                    Logger.Add(dt1.TableName, LogMessageType.Difference,
                    $"{CreateLogMessage(tblSettings.SelectColumns, row1)}" +
                            $" - Column[{colName}]: " +
                            $"{SourceDb.Left.ToString()}={val1Str} " +
                            $"{SourceDb.Right.ToString()}={val2Str}");

                }
            }

            foreach (DataRow row2 in dt2.Rows)
            {
                DataRow row1 = FindMatchingRow(db1, SourceDb.Right, dt1, row2, tblSettings);
            }
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


        private DataRow FindMatchingRow(Database.IDatabase db, SourceDb source, DataTable dt,  DataRow sourceRow, TableSettings tblSettings)
        {
            if(dt == null || sourceRow == null || tblSettings == null)
                return null;

            string whereClause = CreateWhereClause(db, tblSettings.SelectColumns, sourceRow);

            DataRow[] results = dt.Select(whereClause);
            if (results.Count() < 1)
            {
                Logger.Add(dt.TableName, source == SourceDb.Left ? LogMessageType.LeftOnly : LogMessageType.RightOnly,
                           CreateLogMessage(tblSettings.DisplayColumns, sourceRow));
                return null;
            }
            else if (results.Count() == 1)
            {
                return results[0];
            }
            else
            {
                Logger.Add(dt.TableName, LogMessageType.MultipleMatches,
                           CreateLogMessage(tblSettings.SelectColumns, sourceRow));
                return null;
            }

        }

        private DataTable GetTable(Database.IDatabase db, string tableName)
        {
            if (db == null || tableName == null)
                return null;

            if (!db.TableExists(tableName))
            {
                Logger.Add(tableName, LogMessageType.Error, $"Table {tableName} not found in {db.ToString()}");
                return null;
            }

            string sql = $"SELECT * FROM {tableName}";

            DataTable dt = db.GetDataTable(sql);
            if(dt == null)
            {
                Logger.Add(tableName, LogMessageType.Error, $"Unable to get table {tableName} in {db.ToString()}");
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
                    Logger.Add("", LogMessageType.Error, $"Invalid Select Column specified: {selCol}");
                }
                if(val == null)
                    continue;

                string safeColName = selCol;
                string colName = db.IsKeyWord(selCol, out safeColName) ? safeColName : selCol;
                string clause = (val == null) ? $"{colName} is null" : $"{colName} = '{val.ToString()}'";

                if (string.IsNullOrEmpty(where))
                    where = clause;
                else
                    where = string.Format("{0} AND {1}", where, clause);
            }

            return where;
        }

        private string CreateLogMessage(IList<string> cols, DataRow row)
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
                    //Logger.Add("", LogMessageType.Error, $"Invalid Column specified: {col}");
                }
                if (val == null)
                    continue;

                msg += $"{col}={val.ToString()} ";
            }

            return msg;
        }


        private Database.IDatabase Connect(string dbName)
        {
            if (String.IsNullOrEmpty(dbName))
            {
                Logger.Add(dbName, LogMessageType.Error, "Invalid database name");
                return null;
            }
            if (!System.IO.File.Exists(dbName))
            {
                Logger.Add(dbName, LogMessageType.Error, $"Database not found: {dbName}");
                return null;
            }

            Database.IDatabase db = Database.DatabaseFactory.CreateDatabase(DbType, dbName);

            if (db == null)
            {
                Logger.Add(dbName, LogMessageType.Error, $"Unable to connect to database: {dbName}");
            }

            return db;
        }


    }
}
