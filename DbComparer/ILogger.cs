using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bentley.OPEF.Utilities.DbCompare
{
    public interface ILogger
    {
        void Clear();
        void Add(string tableName, LogMessageType msgType, string msg);
    }
}
