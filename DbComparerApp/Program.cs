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
            String dbName1 = @"C:\ProgramData\Bentley\OpenPlant CONNECT Edition\Configuration\Workspaces\WorkSpace\WorkSets\OpenPlantMixedMetric\Standards\OpenPlant\ApplicationDb\OPSEMixedMetric.db";
            //String dbName2 = @"C:\ProgramData\Bentley\OpenPlant CONNECT Edition\Configuration\Workspaces\WorkSpace\WorkSets\OpenPlantMixedMetric\Standards\OpenPlant\ApplicationDb\OPSEMixedMetric - Copy.db";
            String dbName2 = @"C:\ProgramData\Bentley\OpenPlant CONNECT Edition\Configuration\Workspaces\WorkSpace\WorkSets\OP_CE_Metric\Standards\OpenPlant\ApplicationDb\OPSEMMetricPSA.db";

            IDatabase db1 = Connect(dbName1, Database.DatabaseType.SQLite);
            IDatabase db2 = Connect(dbName2, Database.DatabaseType.SQLite);


            Settings settings = SettingsUtilities.Deserialize(@"D:\CONNECT\DbCompare\DbComparerApp\briefcaseConfig.json");

            DbComparer dbComparer = new DbComparer();

            Results results = dbComparer.CompareDbs(db1, db2, settings);

            IList<String> msgs = results.ToStringList();

            IList<String> processedTables = results.GetTablesProcessed();
            IList<String> skippedTables = results.GetTablesSkipped();
            IList<String> tablesWithNoDifferences = results.GetTablesWithNoDifferences();
            IList<String> tablesWithDifferences = results.GetTablesWithDifferences();
            IList<String> tablesWithRowDifferences = results.GetTablesWithRowDifferences();
            IList<String> tablesWithLeftOnly = results.GetTablesWithLeftOnly();
            IList<String> tablesWithRightOnly = results.GetTablesWithRightOnly();
            IList<String> tablesWithErrors = results.GetTablesWithErrors();
            IList<String> tablesWithMultipleMatches = results.GetTablesWithMultipleMatches();

            foreach(string tblName in tablesWithDifferences)
            {
                ResultsView rv = new ResultsView(results, db1, db2, tblName);

            }
        }

        private static Database.IDatabase Connect(string dbName, Database.DatabaseType dbType)
        {
            if (String.IsNullOrEmpty(dbName))
                return null;
            
                if (!System.IO.File.Exists(dbName))
                return null;

            return Database.DatabaseFactory.CreateDatabase(dbType, dbName);
        }



    }
}
