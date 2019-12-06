using System;
using System.Collections.Generic;
using Bentley.OPEF.Database;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bentley.OPEF.Utilities.DbCompare
{
    class Program
    {
        static void Main(string[] args)
        {
            String db1 = @"C:\ProgramData\Bentley\OpenPlant CONNECT Edition\Configuration\Workspaces\WorkSpace\WorkSets\OpenPlantMixedMetric\Standards\OpenPlant\ApplicationDb\OPSEMixedMetric.db";
            String db2 = @"C:\ProgramData\Bentley\OpenPlant CONNECT Edition\Configuration\Workspaces\WorkSpace\WorkSets\OP_CE_Metric\Standards\OpenPlant\ApplicationDb\OPSEMMetricPSA.db";

            Settings settings = SettingsDeserializer.DeserializeSettings(@"D:\CONNECT\DbCompare\DbComparerApp\briefcaseConfig.json");

            DbComparer dbComparer = new DbComparer();
            dbComparer.DbType = Database.DatabaseType.SQLite;
            Logger logger = new Logger();

            dbComparer.CompareDbs(db1, db2, settings, logger);

            IList<String> msgs = logger.ToStringList();
        }



    }
}
