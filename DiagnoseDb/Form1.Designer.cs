namespace DiagnoseDb
{
    partial class Form1
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
            this.btnAnalyzeArchive = new System.Windows.Forms.Button();
            this.btnAnalyzeRecent = new System.Windows.Forms.Button();
            this.btnSelectDatabase = new System.Windows.Forms.Button();
            this.btnSelectSchemaAnalysis = new System.Windows.Forms.Button();
            this.btnSelectSqlAnalysis = new System.Windows.Forms.Button();
            this.btnDiagnosticAnalysis = new System.Windows.Forms.Button();
            this.txtDatabasePath = new System.Windows.Forms.TextBox();
            this.lblDatabasePath = new System.Windows.Forms.Label();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.btnExportToFile = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.btnSqlAnalyzeArchive = new System.Windows.Forms.Button();
            this.btnSqlAnalyzeRecent = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnAnalyzeArchive
            // 
            this.btnAnalyzeArchive.Location = new System.Drawing.Point(16, 15);
            this.btnAnalyzeArchive.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAnalyzeArchive.Name = "btnAnalyzeArchive";
            this.btnAnalyzeArchive.Size = new System.Drawing.Size(133, 37);
            this.btnAnalyzeArchive.TabIndex = 0;
            this.btnAnalyzeArchive.Text = "Archive (Schema)";
            this.btnAnalyzeArchive.UseVisualStyleBackColor = true;
            this.btnAnalyzeArchive.Click += new System.EventHandler(this.btnAnalyzeArchive_Click);
            // 
            // btnAnalyzeRecent
            // 
            this.btnAnalyzeRecent.Location = new System.Drawing.Point(157, 15);
            this.btnAnalyzeRecent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAnalyzeRecent.Name = "btnAnalyzeRecent";
            this.btnAnalyzeRecent.Size = new System.Drawing.Size(133, 37);
            this.btnAnalyzeRecent.TabIndex = 1;
            this.btnAnalyzeRecent.Text = "Recent (Schema)";
            this.btnAnalyzeRecent.UseVisualStyleBackColor = true;
            this.btnAnalyzeRecent.Click += new System.EventHandler(this.btnAnalyzeRecent_Click);
            // 
            // btnSelectDatabase
            // 
            this.btnSelectDatabase.Location = new System.Drawing.Point(299, 15);
            this.btnSelectDatabase.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSelectDatabase.Name = "btnSelectDatabase";
            this.btnSelectDatabase.Size = new System.Drawing.Size(133, 37);
            this.btnSelectDatabase.TabIndex = 2;
            this.btnSelectDatabase.Text = "Browse Database";
            this.btnSelectDatabase.UseVisualStyleBackColor = true;
            this.btnSelectDatabase.Click += new System.EventHandler(this.btnSelectDatabase_Click);
            // 
            // btnSelectSchemaAnalysis
            // 
            this.btnSelectSchemaAnalysis.Location = new System.Drawing.Point(722, 15);
            this.btnSelectSchemaAnalysis.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSelectSchemaAnalysis.Name = "btnSelectSchemaAnalysis";
            this.btnSelectSchemaAnalysis.Size = new System.Drawing.Size(150, 37);
            this.btnSelectSchemaAnalysis.TabIndex = 11;
            this.btnSelectSchemaAnalysis.Text = "Analyze (Schema)";
            this.btnSelectSchemaAnalysis.UseVisualStyleBackColor = true;
            this.btnSelectSchemaAnalysis.Enabled = false;
            this.btnSelectSchemaAnalysis.Click += new System.EventHandler(this.btnSelectSchemaAnalysis_Click);
            // 
            // btnSelectSqlAnalysis
            // 
            this.btnSelectSqlAnalysis.Location = new System.Drawing.Point(880, 15);
            this.btnSelectSqlAnalysis.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSelectSqlAnalysis.Name = "btnSelectSqlAnalysis";
            this.btnSelectSqlAnalysis.Size = new System.Drawing.Size(150, 37);
            this.btnSelectSqlAnalysis.TabIndex = 12;
            this.btnSelectSqlAnalysis.Text = "Analyze (SQL)";
            this.btnSelectSqlAnalysis.UseVisualStyleBackColor = true;
            this.btnSelectSqlAnalysis.Enabled = false;
            this.btnSelectSqlAnalysis.Click += new System.EventHandler(this.btnSelectSqlAnalysis_Click);
            // 
            // btnDiagnosticAnalysis
            // 
            this.btnDiagnosticAnalysis.BackColor = System.Drawing.Color.LightCoral;
            this.btnDiagnosticAnalysis.Location = new System.Drawing.Point(440, 60);
            this.btnDiagnosticAnalysis.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnDiagnosticAnalysis.Name = "btnDiagnosticAnalysis";
            this.btnDiagnosticAnalysis.Size = new System.Drawing.Size(200, 30);
            this.btnDiagnosticAnalysis.TabIndex = 13;
            this.btnDiagnosticAnalysis.Text = "🔍 Diagnostic Analysis";
            this.btnDiagnosticAnalysis.UseVisualStyleBackColor = false;
            this.btnDiagnosticAnalysis.Enabled = false;
            this.btnDiagnosticAnalysis.Click += new System.EventHandler(this.btnDiagnosticAnalysis_Click);
            // 
            // txtDatabasePath
            // 
            this.txtDatabasePath.Location = new System.Drawing.Point(133, 68);
            this.txtDatabasePath.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtDatabasePath.Name = "txtDatabasePath";
            this.txtDatabasePath.ReadOnly = true;
            this.txtDatabasePath.Size = new System.Drawing.Size(799, 22);
            this.txtDatabasePath.TabIndex = 3;
            // 
            // lblDatabasePath
            // 
            this.lblDatabasePath.AutoSize = true;
            this.lblDatabasePath.Location = new System.Drawing.Point(16, 71);
            this.lblDatabasePath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDatabasePath.Name = "lblDatabasePath";
            this.lblDatabasePath.Size = new System.Drawing.Size(100, 16);
            this.lblDatabasePath.TabIndex = 4;
            this.lblDatabasePath.Text = "Database Path:";
            // 
            // txtResults
            // 
            this.txtResults.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtResults.Location = new System.Drawing.Point(16, 105);
            this.txtResults.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ReadOnly = true;
            this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResults.Size = new System.Drawing.Size(1012, 393);
            this.txtResults.TabIndex = 5;
            // 
            // btnExportToFile
            // 
            this.btnExportToFile.Enabled = false;
            this.btnExportToFile.Location = new System.Drawing.Point(16, 511);
            this.btnExportToFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnExportToFile.Name = "btnExportToFile";
            this.btnExportToFile.Size = new System.Drawing.Size(160, 37);
            this.btnExportToFile.TabIndex = 6;
            this.btnExportToFile.Text = "Export to Text File";
            this.btnExportToFile.UseVisualStyleBackColor = true;
            this.btnExportToFile.Click += new System.EventHandler(this.btnExportToFile_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(399, 517);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(533, 28);
            this.progressBar.TabIndex = 7;
            this.progressBar.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(200, 523);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(48, 16);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "Ready";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "Access Database Files|*.mdb;*.accdb|Access 97-2003 (*.mdb)|*.mdb|Access 2007+ (*.accdb)|*.accdb|All Files|*.*";
            this.openFileDialog.Title = "Select Database for Analysis";
            this.openFileDialog.CheckFileExists = true;
            this.openFileDialog.CheckPathExists = true;
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "txt";
            this.saveFileDialog.Filter = "Text Files|*.txt|All Files|*.*";
            this.saveFileDialog.Title = "Export Schema Analysis";
            // 
            // btnSqlAnalyzeArchive
            // 
            this.btnSqlAnalyzeArchive.Location = new System.Drawing.Point(440, 15);
            this.btnSqlAnalyzeArchive.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSqlAnalyzeArchive.Name = "btnSqlAnalyzeArchive";
            this.btnSqlAnalyzeArchive.Size = new System.Drawing.Size(133, 37);
            this.btnSqlAnalyzeArchive.TabIndex = 9;
            this.btnSqlAnalyzeArchive.Text = "Archive (SQL)";
            this.btnSqlAnalyzeArchive.UseVisualStyleBackColor = true;
            this.btnSqlAnalyzeArchive.Click += new System.EventHandler(this.btnSqlAnalyzeArchive_Click);
            // 
            // btnSqlAnalyzeRecent
            // 
            this.btnSqlAnalyzeRecent.Location = new System.Drawing.Point(581, 15);
            this.btnSqlAnalyzeRecent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSqlAnalyzeRecent.Name = "btnSqlAnalyzeRecent";
            this.btnSqlAnalyzeRecent.Size = new System.Drawing.Size(133, 37);
            this.btnSqlAnalyzeRecent.TabIndex = 10;
            this.btnSqlAnalyzeRecent.Text = "Recent (SQL)";
            this.btnSqlAnalyzeRecent.UseVisualStyleBackColor = true;
            this.btnSqlAnalyzeRecent.Click += new System.EventHandler(this.btnSqlAnalyzeRecent_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1045, 567);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnExportToFile);
            this.Controls.Add(this.txtResults);
            this.Controls.Add(this.lblDatabasePath);
            this.Controls.Add(this.txtDatabasePath);
            this.Controls.Add(this.btnDiagnosticAnalysis);
            this.Controls.Add(this.btnSelectSqlAnalysis);
            this.Controls.Add(this.btnSelectSchemaAnalysis);
            this.Controls.Add(this.btnSqlAnalyzeRecent);
            this.Controls.Add(this.btnSqlAnalyzeArchive);
            this.Controls.Add(this.btnSelectDatabase);
            this.Controls.Add(this.btnAnalyzeRecent);
            this.Controls.Add(this.btnAnalyzeArchive);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "Database Schema Analyzer - Flexible Analysis Tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAnalyzeArchive;
        private System.Windows.Forms.Button btnAnalyzeRecent;
        private System.Windows.Forms.Button btnSelectDatabase;
        private System.Windows.Forms.Button btnSelectSchemaAnalysis;
        private System.Windows.Forms.Button btnSelectSqlAnalysis;
        private System.Windows.Forms.Button btnDiagnosticAnalysis;
        private System.Windows.Forms.Button btnSqlAnalyzeArchive;
        private System.Windows.Forms.Button btnSqlAnalyzeRecent;
        private System.Windows.Forms.TextBox txtDatabasePath;
        private System.Windows.Forms.Label lblDatabasePath;
        private System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.Button btnExportToFile;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}

