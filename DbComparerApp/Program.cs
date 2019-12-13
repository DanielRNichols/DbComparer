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
            string htmlTemplateFileName = @"D:\CONNECT\DbCompare\DbComparerApp\template.html";

            string title = "OOTB V6 vs Hatch Upgraded to V6";
            string htmlOutputFileName = @"x:\tmp\OOTBV6-HatchV6.html";
            String dbName1 = @"C:\ProgramData\Bentley\OpenPlant CONNECT Edition\Configuration\Workspaces\WorkSpace\WorkSets\OpenPlantMixedMetric\Standards\OpenPlant\ApplicationDb\OPSEMixedMetric-OOTB-V6.db";
            String dbName2 = @"C:\ProgramData\Bentley\OpenPlant CONNECT Edition\Configuration\Workspaces\WorkSpace\WorkSets\OpenPlantMixedMetric\Standards\OpenPlant\ApplicationDb\OPSEMixedMetric-HatchUpgraded-V6.db";

            //string title = "Hatch V3 vs Hatch V6";
            //string htmlOutputFileName = @"x:\tmp\HatchV3-HatchV6.html";
            //String dbName1 = @"C:\ProgramData\Bentley\OpenPlant CONNECT Edition\Configuration\Workspaces\WorkSpace\WorkSets\OP_CE_Metric\Standards\OpenPlant\ApplicationDb\OPSEMMetricPSA.db";
            //String dbName2 = @"C:\ProgramData\Bentley\OpenPlant CONNECT Edition\Configuration\Workspaces\WorkSpace\WorkSets\OpenPlantMixedMetric\Standards\OpenPlant\ApplicationDb\OPSEMixedMetric-HatchUpgraded-V6.db";


            //String dbName1 = @"D:\CONNECT\WIP\CurrDev\OPSE\out\Winx64\Product\PowerOPSE\Configuration\WorkSpaces\WorkSpace\WorkSets\OpenPlantMixedMetric\Standards\OpenPlant\ApplicationDb\OPSEMixedMetric.db";



            IDatabase db1 = Connect(dbName1, Database.DatabaseType.SQLite);
            IDatabase db2 = Connect(dbName2, Database.DatabaseType.SQLite);


            Settings settings = SettingsUtilities.Deserialize(@"D:\CONNECT\DbCompare\DbComparerApp\briefcaseConfig.json");

            DbComparer dbComparer = new DbComparer(db1, db2, settings);

            Results results = dbComparer.CompareDbs();

            IList<String> msgs = results.ToStringList();

            IList<String> diffMsgs = results.ToStringList(ResultTypes.Difference);

            IList<String> processedTables = results.GetTablesProcessed();
            IList<String> skippedTables = results.GetTablesSkipped();
            IList<String> tablesWithNoDifferences = results.GetTablesWithNoDifferences();
            IList<String> tablesWithRowDifferences = results.GetTablesWithRowDifferences();
            IList<String> tablesWithLeftOnly = results.GetTablesWithLeftOnly();
            IList<String> tablesWithRightOnly = results.GetTablesWithRightOnly();
            IList<String> tablesWithErrors = results.GetTablesWithErrors();
            IList<String> tablesWithMultipleMatches = results.GetTablesWithMultipleMatches();
            IList<String> tablesWithDifferences = results.GetTablesWithDifferences();

            results.ToHTMLFile(title, htmlTemplateFileName, htmlOutputFileName);

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
