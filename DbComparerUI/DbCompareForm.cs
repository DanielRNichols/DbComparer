﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bentley.OPEF.Database;

namespace Bentley.OPEF.Utilities.DbCompare
{
    public partial class DbCompareForm : Form
    {
        private string _htmlTemplateFileName {get; set; }
        private string _title { get; set; }
        private string _db1Name { get; set; }
        private string _db2Name { get; set; }
        private Settings _settings { get; set; }
        private IDatabase _db1 { get; set; }
        private IDatabase _db2 { get; set; }

        public DbCompareForm()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            _htmlTemplateFileName = @"D:\CONNECT\DbCompare\DbComparerApp\template.html";

            _title = "OOTB V6 vs Hatch Upgraded to V6";
            //string htmlOutputFileName = @"x:\tmp\OOTBV6-HatchV6.html";
            _db1Name = @"D:\CONNECT\DbCompare\Data\OPSEMixedMetric-OOTB-V6.db";
            _db2Name = @"D:\CONNECT\DbCompare\Data\OPSEMixedMetric-HatchUpgraded-V6.db";

            //string _title = "Hatch V3 vs Hatch V6";
            //string _htmlOutputFileName = @"x:\tmp\HatchV3-HatchV6.html";
            //String _db1Name = @"D:\CONNECT\DbCompare\Data\OPSEMMetricPSA.db";
            //String _db2Name = @"D:\CONNECT\DbCompare\Data\OPSEMixedMetric-HatchUpgraded-V6.db";

            _db1 = Connect(_db1Name, Database.DatabaseType.SQLite);
            _db2 = Connect(_db2Name, Database.DatabaseType.SQLite);

            _settings = SettingsUtilities.Initialize(@"D:\CONNECT\DbCompare\DbComparerApp\briefcaseConfig.json");

            InitializeTableCheckBoxList(_settings);

        }

        private void InitializeTableCheckBoxList(Settings settings)
        {
            this.tablesCheckedListBox.Items.Clear();
            foreach(TableSettings ts in settings.TableSettings)
            {
                this.tablesCheckedListBox.Items.Add(ts.TableName, ts.ProcessTable.GetValueOrDefault(false));
            }
        }

        private void CompareButton_Click(object sender, EventArgs e)
        {
            Cursor currentCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                this.htmlView.DocumentText = "";
                DbComparer dbComparer = new DbComparer(_db1, _db2, _settings);
                Results results = dbComparer.CompareDbs();
                string html = results.ToHTML(_title, _htmlTemplateFileName);
                this.htmlView.DocumentText = html;
            }
            catch
            {
            }
            finally
            {
                this.Cursor = currentCursor;
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

        private void tablesCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selected = this.tablesCheckedListBox.SelectedIndex;
            if (selected != -1)
            {
                bool processTable = this.tablesCheckedListBox.GetItemChecked(selected);
                string tableName = this.tablesCheckedListBox.Items[selected].ToString();
                TableSettings ts = SettingsUtilities.FindSettings(_settings.TableSettings, tableName);
                if(ts != null)
                    ts.ProcessTable = processTable;
                //this.Text = this.tablesCheckedListBox.Items[selected].ToString();
            }
        }
    }

}
