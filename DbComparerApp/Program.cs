using System;
using System.Collections.Generic;
using System.Data;
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

            Settings settings = SettingsUtilities.Deserialize(@"D:\CONNECT\DbCompare\DbComparerApp\briefcaseConfig.json");

            DbComparer dbComparer = new DbComparer();
            dbComparer.DbType = Database.DatabaseType.SQLite;
            Results results = new Results();

            dbComparer.CompareDbs(db1, db2, settings, results);

            IList<String> msgs = results.ToStringList();

            IList<String> processedTables = results.GetTablesProcessed();
            IList<String> skippedTables = results.GetTablesSkipped();
            IList<String> tablesWithNoDifferences = results.GetTablesWithNoDifferences();
            IList<String> tablesWithDifferences = results.GetTablesWithDifferences();
            IList<String> tablesWithLeftOnly = results.GetTablesWithLeftOnly();
            IList<String> tablesWithRightOnly = results.GetTablesWithRightOnly();
            IList<String> tablesWithErrors = results.GetTablesWithErrors();
            IList<String> tablesWithMultipleMatches = results.GetTablesWithMultipleMatches();
        }



    }
}
