using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiagnoseDb
{
    public partial class Form1 : Form
    {
        private DatabaseSchema currentSchema;
        private DatabaseSqlSchema currentSqlSchema;
        private string currentDatabasePath;
        private string selectedDatabasePath;
        private string currentDiagnosticReport;
        private bool isUsingSqlAnalysis = false;
        private bool isUsingDiagnosticAnalysis = false;

        public Form1()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Set initial status
            lblStatus.Text = "Ready - Select a database to analyze";
            
            // Disable flexible analysis buttons initially
            btnSelectSchemaAnalysis.Enabled = false;
            btnSelectSqlAnalysis.Enabled = false;
            
            // Set tooltips for better user experience
            var toolTip = new ToolTip();
            toolTip.SetToolTip(btnAnalyzeArchive, "Analyze archive.mdb using Schema method");
            toolTip.SetToolTip(btnAnalyzeRecent, "Analyze recent.mdb using Schema method");
            toolTip.SetToolTip(btnSelectDatabase, "Browse and select any database file");
            toolTip.SetToolTip(btnSelectSchemaAnalysis, "Analyze selected database using Schema method (compatible)");
            toolTip.SetToolTip(btnSelectSqlAnalysis, "Analyze selected database using SQL method (detailed)");
            toolTip.SetToolTip(btnSqlAnalyzeArchive, "Analyze archive.mdb using SQL method");
            toolTip.SetToolTip(btnSqlAnalyzeRecent, "Analyze recent.mdb using SQL method");
            toolTip.SetToolTip(btnDiagnosticAnalysis, "Run comprehensive database integrity diagnostic analysis");
        }

        private void btnAnalyzeArchive_Click(object sender, EventArgs e)
        {
            AnalyzeDatabase("archive.mdb");
        }

        private void btnAnalyzeRecent_Click(object sender, EventArgs e)
        {
            AnalyzeDatabase("recent.mdb");
        }

        private void btnSelectDatabase_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    selectedDatabasePath = openFileDialog.FileName;
                    
                    // Validate file exists and is accessible
                    if (!File.Exists(selectedDatabasePath))
                    {
                        MessageBox.Show("Selected file does not exist.", "File Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    // Check file size and provide info
                    var fileInfo = new FileInfo(selectedDatabasePath);
                    var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);
                    
                    txtDatabasePath.Text = Path.GetFullPath(selectedDatabasePath);
                    
                    // Enable analysis buttons for selected database
                    btnSelectSchemaAnalysis.Enabled = true;
                    btnSelectSqlAnalysis.Enabled = true;
                    btnDiagnosticAnalysis.Enabled = true;
                    
                    lblStatus.Text = $"Database selected: {Path.GetFileName(selectedDatabasePath)} ({fileSizeMB:F1} MB)";
                    
                    // Clear previous results
                    txtResults.Clear();
                    btnExportToFile.Enabled = false;
                    isUsingSqlAnalysis = false;
                    currentSchema = null;
                    currentSqlSchema = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error selecting database: {ex.Message}", "Selection Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lblStatus.Text = "Error selecting database";
                }
            }
        }

        private void btnSelectSchemaAnalysis_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedDatabasePath))
            {
                if (ValidateSelectedDatabase())
                {
                    AnalyzeDatabase(selectedDatabasePath);
                }
            }
            else
            {
                MessageBox.Show("Please select a database first using 'Browse Database' button.", 
                    "No Database Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSelectSqlAnalysis_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedDatabasePath))
            {
                if (ValidateSelectedDatabase())
                {
                    AnalyzeDatabaseWithSql(selectedDatabasePath);
                }
            }
            else
            {
                MessageBox.Show("Please select a database first using 'Browse Database' button.", 
                    "No Database Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool ValidateSelectedDatabase()
        {
            try
            {
                if (!File.Exists(selectedDatabasePath))
                {
                    MessageBox.Show("Selected database file no longer exists. Please select a new database.", 
                        "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    // Reset selection
                    selectedDatabasePath = null;
                    txtDatabasePath.Clear();
                    btnSelectSchemaAnalysis.Enabled = false;
                    btnSelectSqlAnalysis.Enabled = false;
                    lblStatus.Text = "Database file not found - please select again";
                    
                    return false;
                }
                
                // Check if file is locked or accessible
                using (var stream = File.Open(selectedDatabasePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // File is accessible
                }
                
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied to the selected database file. Please check file permissions.", 
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Database file is in use or locked: {ex.Message}", 
                    "File In Use", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error validating database file: {ex.Message}", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnDiagnosticAnalysis_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedDatabasePath))
            {
                if (ValidateSelectedDatabase())
                {
                    RunDiagnosticAnalysis(selectedDatabasePath);
                }
            }
            else
            {
                MessageBox.Show("Please select a database first using 'Browse Database' button.", 
                    "No Database Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void RunDiagnosticAnalysis(string dbPath)
        {
            try
            {
                // Update UI
                lblStatus.Text = "Running diagnostic analysis...";
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                DisableAllButtons();

                currentDatabasePath = dbPath;
                txtDatabasePath.Text = Path.GetFullPath(dbPath);
                isUsingSqlAnalysis = false;
                isUsingDiagnosticAnalysis = true;

                string diagnosticReport = "";

                // Run diagnostic analysis in background
                await Task.Run(() =>
                {
                    var diagnosticAnalyzer = new DatabaseDiagnosticAnalyzer(dbPath);
                    
                    // Run comprehensive diagnostic analysis (no specific date range)
                    diagnosticReport = diagnosticAnalyzer.AnalyzeDatabaseIntegrity();
                });

                // Store diagnostic report
                currentDiagnosticReport = diagnosticReport;
                
                // Display results
                txtResults.Text = diagnosticReport;
                btnExportToFile.Enabled = true;
                lblStatus.Text = "Diagnostic analysis complete - Review database integrity findings";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running diagnostic analysis: {ex.Message}", "Diagnostic Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Diagnostic analysis error occurred";
            }
            finally
            {
                // Reset UI
                progressBar.Visible = false;
                EnableAllButtons();
            }
        }

        private async void AnalyzeDatabase(string dbPath)
        {
            try
            {
                // Update UI
                lblStatus.Text = "Analyzing database...";
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                DisableAllButtons();

                currentDatabasePath = dbPath;
                txtDatabasePath.Text = Path.GetFullPath(dbPath);

                // Run analysis in background
                await Task.Run(() =>
                {
                    var analyzer = new AccessDatabaseAnalyzer(dbPath);
                    currentSchema = analyzer.AnalyzeDatabase();
                });

                // Create pure schema analysis (no expected tables)
                var pureAnalyzer = new PureSchemaAnalyzer(currentSchema);
                var analysisReport = pureAnalyzer.GeneratePureSchemaReport();

                // Display results
                txtResults.Text = analysisReport;
                btnExportToFile.Enabled = true;
                lblStatus.Text = "Analysis complete";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error analyzing database: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Error occurred";
            }
            finally
            {
                // Reset UI
                progressBar.Visible = false;
                EnableAllButtons();
            }
        }

        private void btnSqlAnalyzeArchive_Click(object sender, EventArgs e)
        {
            AnalyzeDatabaseWithSql("archive.mdb");
        }

        private void btnSqlAnalyzeRecent_Click(object sender, EventArgs e)
        {
            AnalyzeDatabaseWithSql("recent.mdb");
        }

        private async void AnalyzeDatabaseWithSql(string dbPath)
        {
            try
            {
                // Update UI
                lblStatus.Text = "Analyzing database with SQL queries...";
                progressBar.Visible = true;
                progressBar.Style = ProgressBarStyle.Marquee;
                DisableAllButtons();

                currentDatabasePath = dbPath;
                txtDatabasePath.Text = Path.GetFullPath(dbPath);
                isUsingSqlAnalysis = true;

                // Run SQL analysis in background
                await Task.Run(() =>
                {
                    var sqlAnalyzer = new AccessSqlAnalyzer(dbPath);
                    currentSqlSchema = sqlAnalyzer.AnalyzeDatabaseWithSql();
                });

                // Generate pure SQL-based report (no expected tables)
                var sqlReport = GeneratePureSqlAnalysisReport(currentSqlSchema);

                // Display results
                txtResults.Text = sqlReport;
                btnExportToFile.Enabled = true;
                lblStatus.Text = "SQL analysis complete - Database structure extracted";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error analyzing database with SQL: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "SQL analysis error occurred";
            }
            finally
            {
                // Reset UI
                progressBar.Visible = false;
                EnableAllButtons();
            }
        }

        private string GeneratePureSqlAnalysisReport(DatabaseSqlSchema schema)
        {
            var sb = new StringBuilder();

            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine("DATABASE SCHEMA ANALYSIS - SQL METHOD");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine($"Database: {schema.DatabasePath}");
            sb.AppendLine($"Analysis Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Method: Direct SQL queries to Access system tables");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(schema.ErrorMessage))
            {
                sb.AppendLine("ERROR:");
                sb.AppendLine(schema.ErrorMessage);
                sb.AppendLine();
                return sb.ToString();
            }

            // Summary
            sb.AppendLine("ACTUAL DATABASE CONTENTS");
            sb.AppendLine("-".PadRight(50, '-'));
            sb.AppendLine($"Real Tables Found: {schema.Tables.Count}");
            sb.AppendLine($"Total Indexes: {schema.Indexes.Count}");
            sb.AppendLine($"Relationships: {schema.Relationships.Count}");
            sb.AppendLine();

            // List all real tables
            sb.AppendLine("REAL TABLES IN DATABASE:");
            sb.AppendLine("-".PadRight(60, '-'));
            foreach (var table in schema.Tables)
            {
                sb.AppendLine($"• {table.TableName} ({table.RecordCount} records)");
                if (table.DateCreated != DateTime.MinValue)
                    sb.AppendLine($"  Created: {table.DateCreated:yyyy-MM-dd HH:mm:ss}");
            }
            sb.AppendLine();

            // Detailed table analysis
            foreach (var table in schema.Tables)
            {
                sb.AppendLine($"TABLE: {table.TableName}");
                sb.AppendLine("=".PadRight(60, '='));
                sb.AppendLine($"Records: {table.RecordCount}");
                sb.AppendLine($"Columns: {table.Columns.Count}");
                if (table.DateCreated != DateTime.MinValue)
                    sb.AppendLine($"Created: {table.DateCreated:yyyy-MM-dd HH:mm:ss}");
                if (table.DateUpdated != DateTime.MinValue)
                    sb.AppendLine($"Updated: {table.DateUpdated:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine();

                // Column details
                sb.AppendLine("COLUMNS:");
                sb.AppendLine("-".PadRight(70, '-'));
                sb.AppendLine($"{"Name",-25} {"Type",-15} {"Size",-8} {"Required",-10} {"Default",-10}");
                sb.AppendLine("-".PadRight(70, '-'));
                
                foreach (var column in table.Columns)
                {
                    var size = column.ColumnSize > 0 ? column.ColumnSize.ToString() : "-";
                    var defaultVal = string.IsNullOrEmpty(column.DefaultValue) ? "-" : column.DefaultValue;
                    if (defaultVal.Length > 10) defaultVal = defaultVal.Substring(0, 10) + "...";
                    
                    sb.AppendLine($"{column.ColumnName,-25} {column.DataType,-15} {size,-8} {column.IsRequired,-10} {defaultVal,-10}");
                }
                sb.AppendLine();

                // Validation rules
                var validationColumns = table.Columns.FindAll(c => !string.IsNullOrEmpty(c.ValidationRule));
                if (validationColumns.Count > 0)
                {
                    sb.AppendLine("VALIDATION RULES:");
                    sb.AppendLine("-".PadRight(50, '-'));
                    foreach (var col in validationColumns)
                    {
                        sb.AppendLine($"{col.ColumnName}: {col.ValidationRule}");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            // Indexes
            if (schema.Indexes.Count > 0)
            {
                sb.AppendLine("DATABASE INDEXES");
                sb.AppendLine("=".PadRight(80, '='));
                sb.AppendLine($"{"Index Name",-30} {"Table",-20} {"Primary",-8} {"Unique",-8} {"Foreign",-8}");
                sb.AppendLine("-".PadRight(80, '-'));
                
                foreach (var index in schema.Indexes)
                {
                    sb.AppendLine($"{index.IndexName,-30} {index.TableName,-20} {index.IsPrimary,-8} {index.IsUnique,-8} {index.IsForeign,-8}");
                }
                sb.AppendLine();
            }

            // Relationships
            if (schema.Relationships.Count > 0)
            {
                sb.AppendLine("DATABASE RELATIONSHIPS");
                sb.AppendLine("=".PadRight(80, '='));
                sb.AppendLine($"{"Relationship",-25} {"Primary Table",-20} {"Foreign Table",-20} {"Columns",-15}");
                sb.AppendLine("-".PadRight(80, '-'));
                
                foreach (var rel in schema.Relationships)
                {
                    var columns = $"{rel.PrimaryColumn}→{rel.ForeignColumn}";
                    sb.AppendLine($"{rel.RelationshipName,-25} {rel.PrimaryTable,-20} {rel.ForeignTable,-20} {columns,-15}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private void DisableAllButtons()
        {
            btnAnalyzeArchive.Enabled = false;
            btnAnalyzeRecent.Enabled = false;
            btnSelectDatabase.Enabled = false;
            btnSelectSchemaAnalysis.Enabled = false;
            btnSelectSqlAnalysis.Enabled = false;
            btnDiagnosticAnalysis.Enabled = false;
            btnSqlAnalyzeArchive.Enabled = false;
            btnSqlAnalyzeRecent.Enabled = false;
            btnExportToFile.Enabled = false;
        }

        private void EnableAllButtons()
        {
            btnAnalyzeArchive.Enabled = true;
            btnAnalyzeRecent.Enabled = true;
            btnSelectDatabase.Enabled = true;
            btnSqlAnalyzeArchive.Enabled = true;
            btnSqlAnalyzeRecent.Enabled = true;
            
            // Enable flexible analysis buttons only if database is selected
            if (!string.IsNullOrEmpty(selectedDatabasePath))
            {
                btnSelectSchemaAnalysis.Enabled = true;
                btnSelectSqlAnalysis.Enabled = true;
                btnDiagnosticAnalysis.Enabled = true;
            }
        }

        private void btnExportToFile_Click(object sender, EventArgs e)
        {
            if (currentSchema == null && currentSqlSchema == null && string.IsNullOrEmpty(currentDiagnosticReport))
            {
                MessageBox.Show("No analysis data to export.", "Warning", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string analysisType = isUsingDiagnosticAnalysis ? "Diagnostic" : (isUsingSqlAnalysis ? "SQL" : "Schema");
            saveFileDialog.FileName = $"Database_{analysisType}_Analysis_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (isUsingDiagnosticAnalysis && !string.IsNullOrEmpty(currentDiagnosticReport))
                    {
                        System.IO.File.WriteAllText(saveFileDialog.FileName, currentDiagnosticReport);
                    }
                    else if (isUsingSqlAnalysis && currentSqlSchema != null)
                    {
                        var sqlAnalyzer = new AccessSqlAnalyzer(currentDatabasePath);
                        sqlAnalyzer.ExportSqlAnalysisToTextFile(currentSqlSchema, saveFileDialog.FileName);
                    }
                    else if (currentSchema != null)
                    {
                        var pureAnalyzer = new PureSchemaAnalyzer(currentSchema);
                        var pureReport = pureAnalyzer.GeneratePureSchemaReport();
                        System.IO.File.WriteAllText(saveFileDialog.FileName, pureReport);
                    }
                    
                    MessageBox.Show($"Analysis exported to:\n{saveFileDialog.FileName}", 
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    lblStatus.Text = "Export complete";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting file: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
