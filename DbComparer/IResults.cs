using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bentley.OPEF.Utilities.DbCompare
{
    public interface IResults
    {
        void Clear();
        void AddProcessingTable(string tableName, string msg);
        void AddSkippedTable(string tableName, string msg);
        void AddNoDifferences(string tableName, string msg);
        void AddDifference(string tableName, string whereClause, string diffCol, string msg);
        void AddOneSideOnly(string tableName, ResultTypes msgType, string whereClause, string msg);
        void AddLeftOnly(string tableName, string whereClause, string msg);
        void AddRightOnly(string tableName, string whereClause, string msg);
        void AddMultipleMatches(string tableName, string whereClause, string msg);
        void AddError(string tableName, string msg);
    }
}
