using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bentley.OPEF.Utilities.DbCompare
{
    public class Settings
    {
        public GlobalSettings GlobalSettings { get; set; }
        public IList<TableSettings> TableSettings { get; set; }


    }

    public class GlobalSettings
    {
        public IList<string> IgnoreColumns { get; set; }
        public bool? TreatNullAsEmptyString { get; set; }
        public bool? IgnoreCase { get; set; }
        public bool? TrimValues { get; set; }
    }

    public class TableSettings
    {
        public string TableName { get; set; }
        public bool? ProcessTable { get; set; }
        public bool? TreatNullAsEmptyString { get; set; }
        public bool? IgnoreCase { get; set; }
        public bool? TrimValues { get; set; }
        public IList<string> IgnoreColumns { get; set; }
        public IList<string> SelectColumns { get; set; }
    }

    public class SettingsUtilities
    {
        public static Settings Deserialize(string settingsFile)
        {
            if (String.IsNullOrEmpty(settingsFile) || !System.IO.File.Exists(settingsFile))
                return null;

            try
            {
                JObject jsonData = JObject.Parse(System.IO.File.ReadAllText(settingsFile));

                return jsonData.ToObject<Settings>();
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Unable to parse json file: {0}", settingsFile), ex);
            }
        }

        public static TableSettings FindSettings(IList<TableSettings> tblSettings, string tableName)
        {
            if(tblSettings == null || String.IsNullOrEmpty(tableName))
                return null;
            
            foreach(TableSettings ts in tblSettings)
            {
                if(ts.TableName == null)
                    continue;

                if(ts.TableName.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))
                    return ts;
            }

            return null;

        }
    }


}
