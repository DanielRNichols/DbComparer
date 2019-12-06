using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bentley.OPEF.Utilities.DbCompare
{
    public enum LogMessageType
    {
        ProcessingTable,
        SkippedTable,
        Difference,
        LeftOnly,
        RightOnly,
        MultipleMatches,
        Error

    }


    public class Logger: ILogger
    {
        private DataTable LogTable {get; set; }

        private const string IdCol = "Id";
        private const string TableNameCol = "TableName";
        private const string MsgTypeCol = "MsgType";
        private const string MsgCol = "Msg";

        public Logger()
        {
            Clear();
        }

        public void Clear()
        {
            LogTable = new DataTable("Log");

            DataColumn idCol = LogTable.Columns.Add(IdCol, typeof(Int32));
            idCol.AutoIncrement = true;
            idCol.AutoIncrementSeed = 1;

            LogTable.Columns.Add(TableNameCol, typeof(String));
            LogTable.Columns.Add(MsgTypeCol, typeof(String));
            LogTable.Columns.Add(MsgCol, typeof(String));

        }

        public void Add(string tableName, LogMessageType msgType, string msg)
        {
            DataRow row = LogTable.NewRow();
            row[TableNameCol] = tableName;
            row[MsgTypeCol] = msgType.ToString();
            row[MsgCol] = msg;
            LogTable.Rows.Add(row);
            
        }

        public IList<String> ToStringList()
        {
            IList<String> msgs = new List<String>();

            foreach(DataRow row in LogTable.Rows)
            {
                string id = row[IdCol].ToString();
                string tableName = row[TableNameCol].ToString();
                string msgType = row[MsgTypeCol].ToString();
                string msg = row[MsgCol].ToString();

                msgs.Add($"{id}. [{tableName}] [{msgType.ToString()}] - {msg}");
            }

            return msgs;
        }
    }
}
