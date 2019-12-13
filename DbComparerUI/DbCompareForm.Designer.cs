namespace Bentley.OPEF.Utilities.DbCompare
{
    partial class DbCompareForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.htmlView = new System.Windows.Forms.WebBrowser();
            this.CompareButton = new System.Windows.Forms.Button();
            this.tablesCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // htmlView
            // 
            this.htmlView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.htmlView.Location = new System.Drawing.Point(189, 41);
            this.htmlView.MinimumSize = new System.Drawing.Size(20, 20);
            this.htmlView.Name = "htmlView";
            this.htmlView.Size = new System.Drawing.Size(599, 397);
            this.htmlView.TabIndex = 0;
            // 
            // CompareButton
            // 
            this.CompareButton.Location = new System.Drawing.Point(189, 12);
            this.CompareButton.Name = "CompareButton";
            this.CompareButton.Size = new System.Drawing.Size(75, 23);
            this.CompareButton.TabIndex = 1;
            this.CompareButton.Text = "Compare";
            this.CompareButton.UseVisualStyleBackColor = true;
            this.CompareButton.Click += new System.EventHandler(this.CompareButton_Click);
            // 
            // tablesCheckedListBox
            // 
            this.tablesCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tablesCheckedListBox.CheckOnClick = true;
            this.tablesCheckedListBox.FormattingEnabled = true;
            this.tablesCheckedListBox.Location = new System.Drawing.Point(12, 12);
            this.tablesCheckedListBox.Name = "tablesCheckedListBox";
            this.tablesCheckedListBox.Size = new System.Drawing.Size(165, 424);
            this.tablesCheckedListBox.TabIndex = 2;
            this.tablesCheckedListBox.SelectedIndexChanged += new System.EventHandler(this.tablesCheckedListBox_SelectedIndexChanged);
            // 
            // DbCompareForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tablesCheckedListBox);
            this.Controls.Add(this.CompareButton);
            this.Controls.Add(this.htmlView);
            this.Name = "DbCompareForm";
            this.Text = "DbComparer";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser htmlView;
        private System.Windows.Forms.Button CompareButton;
        private System.Windows.Forms.CheckedListBox tablesCheckedListBox;
    }
}

