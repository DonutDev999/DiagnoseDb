using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace DiagnoseDb
{
    public class DatabaseDiagnosticAnalyzer
    {
        private string connectionString;
        private string databasePath;

        public DatabaseDiagnosticAnalyzer(string dbPath)
        {
            databasePath = dbPath;
            connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={dbPath};";
        }

        public string AnalyzeDatabaseIntegrity(DateTime? startDate = null, DateTime? endDate = null)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine("DATABASE INTEGRITY DIAGNOSTIC ANALYSIS");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine($"Database: {databasePath}");
            sb.AppendLine($"Analysis Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            if (startDate.HasValue && endDate.HasValue)
            {
                sb.AppendLine($"Date Range: {startDate.Value:yyyy-MM-dd} to {endDate.Value:yyyy-MM-dd}");
            }
            else
            {
                sb.AppendLine("Date Range: All available data");
            }
            sb.AppendLine();

            try
            {
                using (var connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    
                    GenerateDatabaseOverview(sb, connection);
                    AnalyzeTableRelationships(sb, connection);
                    CheckOrphanedRecords(sb, connection);
                    AnalyzeDateBasedPatterns(sb, connection, startDate, endDate);
                    CheckArchiveRelatedFields(sb, connection);
                    ValidateDataIntegrity(sb, connection);
                    AnalyzeRecordDistribution(sb, connection);
                    GenerateDynamicRecommendations(sb, connection);
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"ERROR: {ex.Message}");
            }

            return sb.ToString();
        }

        private void GenerateDatabaseOverview(StringBuilder sb, OleDbConnection connection)
        {
            sb.AppendLine("1. DATABASE OVERVIEW");
            sb.AppendLine("=".PadRight(80, '='));

            try
            {
                var tables = GetTableNames(connection);
                sb.AppendLine($"Total Tables Found: {tables.Count}");
                sb.AppendLine();

                sb.AppendLine("Table Summary:");
                sb.AppendLine("Table Name                     Record Count    Est. Size");
                sb.AppendLine("-".PadRight(60, '-'));

                foreach (var table in tables.Take(20))
                {
                    try
                    {
                        string countQuery = $"SELECT COUNT(*) FROM [{table}]";
                        using (var cmd = new OleDbCommand(countQuery, connection))
                        {
                            var count = (int)cmd.ExecuteScalar();
                            var sizeEst = count < 1000 ? "Small" : count < 10000 ? "Medium" : "Large";
                            sb.AppendLine($"{table.PadRight(30)} {count.ToString("N0").PadLeft(12)} {sizeEst.PadLeft(12)}");
                        }
                    }
                    catch
                    {
                        sb.AppendLine($"{table.PadRight(30)} {"Error".PadLeft(12)} {"Unknown".PadLeft(12)}");
                    }
                }
                sb.AppendLine();
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error generating overview: {ex.Message}");
            }
        }

        private void AnalyzeTableRelationships(StringBuilder sb, OleDbConnection connection)
        {
            sb.AppendLine("2. TABLE RELATIONSHIP ANALYSIS");
            sb.AppendLine("=".PadRight(80, '='));

            try
            {
                var tables = GetTableNames(connection);
                var relationshipTables = new Dictionary<string, List<string>>();

                foreach (var table in tables)
                {
                    var columns = GetTableColumns(connection, table);
                    var foreignKeys = columns.Where(c => c.ToLower().EndsWith("_id") && 
                        !c.ToLower().Equals(table.ToLower() + "_id")).ToList();
                    
                    if (foreignKeys.Count > 0)
                    {
                        relationshipTables[table] = foreignKeys;
                    }
                }

                sb.AppendLine("Potential Table Relationships:");
                sb.AppendLine("-".PadRight(60, '-'));
                
                foreach (var kvp in relationshipTables.Take(10))
                {
                    sb.AppendLine($"Table: {kvp.Key}");
                    foreach (var fk in kvp.Value)
                    {
                        sb.AppendLine($"  → References: {fk}");
                    }
                    sb.AppendLine();
                }

                AnalyzeRetailPatterns(sb, tables);
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error analyzing relationships: {ex.Message}");
            }

            sb.AppendLine();
        }

        private void AnalyzeRetailPatterns(StringBuilder sb, List<string> tables)
        {
            sb.AppendLine("Common Retail Patterns Detected:");
            sb.AppendLine("-".PadRight(50, '-'));

            var patterns = new Dictionary<string, List<string>>
            {
                ["Sales/Invoice Tables"] = tables.Where(t => 
                    t.ToLower().Contains("docket") || 
                    t.ToLower().Contains("invoice") || 
                    t.ToLower().Contains("sale")).ToList(),
                    
                ["Product/Stock Tables"] = tables.Where(t => 
                    t.ToLower().Contains("stock") || 
                    t.ToLower().Contains("product") || 
                    t.ToLower().Contains("item")).ToList(),
                    
                ["Customer Tables"] = tables.Where(t => 
                    t.ToLower().Contains("customer") || 
                    t.ToLower().Contains("client")).ToList(),
                    
                ["Order Tables"] = tables.Where(t => 
                    t.ToLower().Contains("order") || 
                    t.ToLower().Contains("layby")).ToList(),
                    
                ["Payment Tables"] = tables.Where(t => 
                    t.ToLower().Contains("payment") || 
                    t.ToLower().Contains("cashup")).ToList(),
                    
                ["Package/Line Tables"] = tables.Where(t => 
                    t.ToLower().Contains("package") || 
                    t.ToLower().Contains("line") || 
                    t.ToLower().Contains("detail")).ToList()
            };

            foreach (var pattern in patterns)
            {
                if (pattern.Value.Count > 0)
                {
                    sb.AppendLine($"{pattern.Key}: {string.Join(", ", pattern.Value)}");
                }
            }
            sb.AppendLine();
        }

        private void CheckOrphanedRecords(StringBuilder sb, OleDbConnection connection)
        {
            sb.AppendLine("3. ORPHANED RECORDS CHECK");
            sb.AppendLine("=".PadRight(80, '='));

            try
            {
                var tables = GetTableNames(connection);
                var orphanedIssues = new List<string>();

                foreach (var table in tables)
                {
                    var columns = GetTableColumns(connection, table);
                    var foreignKeys = columns.Where(c => c.ToLower().EndsWith("_id") && 
                        !c.ToLower().Equals(table.ToLower() + "_id")).ToList();

                    foreach (var fkColumn in foreignKeys.Take(3))
                    {
                        try
                        {
                            var referencedTable = tables.FirstOrDefault(t => 
                                fkColumn.ToLower().StartsWith(t.ToLower() + "_") ||
                                fkColumn.ToLower().Replace("_id", "").Equals(t.ToLower()));

                            if (referencedTable != null)
                            {
                                var pkColumn = GetTableColumns(connection, referencedTable)
                                    .FirstOrDefault(c => c.ToLower().Contains("_id") || c.ToLower() == "id");

                                if (pkColumn != null)
                                {
                                    string orphanQuery = $@"
                                        SELECT COUNT(*) 
                                        FROM [{table}] t1 
                                        LEFT JOIN [{referencedTable}] t2 ON t1.[{fkColumn}] = t2.[{pkColumn}]
                                        WHERE t2.[{pkColumn}] IS NULL AND t1.[{fkColumn}] IS NOT NULL";

                                    using (var cmd = new OleDbCommand(orphanQuery, connection))
                                    {
                                        var orphanCount = (int)cmd.ExecuteScalar();
                                        if (orphanCount > 0)
                                        {
                                            orphanedIssues.Add($"{table}.{fkColumn} → {referencedTable}.{pkColumn}: {orphanCount} orphaned records");
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Skip if analysis fails for this relationship
                        }
                    }
                }

                if (orphanedIssues.Count > 0)
                {
                    sb.AppendLine("⚠️  Orphaned Records Found:");
                    foreach (var issue in orphanedIssues)
                    {
                        sb.AppendLine($"  • {issue}");
                    }
                }
                else
                {
                    sb.AppendLine("✅ No obvious orphaned records detected.");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error checking orphaned records: {ex.Message}");
            }

            sb.AppendLine();
        }

        private void AnalyzeDateBasedPatterns(StringBuilder sb, OleDbConnection connection, DateTime? startDate, DateTime? endDate)
        {
            sb.AppendLine("4. DATE-BASED PATTERN ANALYSIS");
            sb.AppendLine("=".PadRight(80, '='));

            try
            {
                var tables = GetTableNames(connection);
                var dateAnalyzed = false;

                foreach (var table in tables.Take(5))
                {
                    var columns = GetTableColumns(connection, table);
                    var dateColumns = columns.Where(c => 
                        c.ToLower().Contains("date") || 
                        c.ToLower().Contains("time") ||
                        c.ToLower().Contains("created") ||
                        c.ToLower().Contains("modified")).ToList();

                    if (dateColumns.Count > 0)
                    {
                        foreach (var dateCol in dateColumns.Take(2))
                        {
                            try
                            {
                                string query = $@"
                                    SELECT 
                                        Format([{dateCol}], 'yyyy-mm') as YearMonth,
                                        COUNT(*) as RecordCount
                                    FROM [{table}]
                                    WHERE [{dateCol}] IS NOT NULL";

                                if (startDate.HasValue && endDate.HasValue)
                                {
                                    query += $" AND [{dateCol}] >= #{startDate.Value:yyyy-MM-dd}# AND [{dateCol}] <= #{endDate.Value:yyyy-MM-dd}#";
                                }

                                query += $" GROUP BY Format([{dateCol}], 'yyyy-mm') ORDER BY Format([{dateCol}], 'yyyy-mm')";

                                using (var cmd = new OleDbCommand(query, connection))
                                {
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {
                                            sb.AppendLine($"Table: {table}, Column: {dateCol}");
                                            sb.AppendLine("Month      Records");
                                            sb.AppendLine("-".PadRight(25, '-'));
                                            
                                            while (reader.Read())
                                            {
                                                var month = reader["YearMonth"]?.ToString() ?? "Unknown";
                                                var count = reader.GetInt32("RecordCount");
                                                sb.AppendLine(month.PadRight(10) + " " + count.ToString("N0"));
                                            }
                                            sb.AppendLine();
                                            dateAnalyzed = true;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // Skip if date analysis fails for this column
                            }
                        }
                    }
                }

                if (!dateAnalyzed)
                {
                    sb.AppendLine("No suitable date columns found for pattern analysis.");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error analyzing date patterns: {ex.Message}");
            }

            sb.AppendLine();
        }

        private void CheckArchiveRelatedFields(StringBuilder sb, OleDbConnection connection)
        {
            sb.AppendLine("5. ARCHIVE-RELATED FIELDS ANALYSIS");
            sb.AppendLine("=".PadRight(80, '='));

            try
            {
                var tables = GetTableNames(connection);
                
                foreach (var table in tables)
                {
                    var columns = GetTableColumns(connection, table);
                    var archiveColumns = columns.Where(c => c.ToLower().Contains("archive")).ToList();
                    
                    if (archiveColumns.Count > 0)
                    {
                        sb.AppendLine($"Table: {table}");
                        sb.AppendLine($"  Archive-related columns: {string.Join(", ", archiveColumns)}");
                        
                        foreach (var column in archiveColumns)
                        {
                            try
                            {
                                string query = $@"
                                    SELECT 
                                        {column},
                                        COUNT(*) as RecordCount
                                    FROM {table}
                                    GROUP BY {column}";

                                using (var cmd = new OleDbCommand(query, connection))
                                {
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        sb.AppendLine($"    {column} distribution:");
                                        while (reader.Read())
                                        {
                                            var value = reader[column]?.ToString() ?? "NULL";
                                            var count = reader.GetInt32("RecordCount");
                                            sb.AppendLine("      " + value + ": " + count.ToString("N0") + " records");
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                sb.AppendLine($"      Error analyzing {column}");
                            }
                        }
                        sb.AppendLine();
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error analyzing archive fields: {ex.Message}");
            }
        }

        private void ValidateDataIntegrity(StringBuilder sb, OleDbConnection connection)
        {
            sb.AppendLine("6. DATA INTEGRITY VALIDATION");
            sb.AppendLine("=".PadRight(80, '='));

            try
            {
                var tables = GetTableNames(connection);
                var issues = new List<string>();

                foreach (var table in tables)
                {
                    try
                    {
                        var columns = GetTableColumns(connection, table);
                        var idColumn = columns.FirstOrDefault(c => c.ToLower().Contains("_id") || c.ToLower() == "id");
                        
                        if (idColumn != null)
                        {
                            string query = $@"
                                SELECT {idColumn}, COUNT(*) as DuplicateCount
                                FROM {table}
                                GROUP BY {idColumn}
                                HAVING COUNT(*) > 1";

                            using (var cmd = new OleDbCommand(query, connection))
                            {
                                using (var reader = cmd.ExecuteReader())
                                {
                                    var duplicates = 0;
                                    while (reader.Read())
                                    {
                                        duplicates++;
                                    }
                                    
                                    if (duplicates > 0)
                                    {
                                        issues.Add($"Table {table}: {duplicates} duplicate {idColumn} values found");
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Skip tables that can't be analyzed
                    }
                }

                if (issues.Count > 0)
                {
                    sb.AppendLine("⚠️  Data Integrity Issues Found:");
                    foreach (var issue in issues)
                    {
                        sb.AppendLine($"  • {issue}");
                    }
                }
                else
                {
                    sb.AppendLine("✅ No obvious data integrity issues detected.");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error validating data integrity: {ex.Message}");
            }

            sb.AppendLine();
        }

        private void AnalyzeRecordDistribution(StringBuilder sb, OleDbConnection connection)
        {
            sb.AppendLine("7. RECORD COUNT DISTRIBUTION ANALYSIS");
            sb.AppendLine("=".PadRight(80, '='));

            try
            {
                var tables = GetTableNames(connection);
                var tableSizes = new List<(string Table, int Count)>();

                foreach (var table in tables)
                {
                    try
                    {
                        string query = $"SELECT COUNT(*) FROM [{table}]";
                        using (var cmd = new OleDbCommand(query, connection))
                        {
                            var count = (int)cmd.ExecuteScalar();
                            tableSizes.Add((table, count));
                        }
                    }
                    catch
                    {
                        tableSizes.Add((table, -1));
                    }
                }

                var sortedTables = tableSizes.OrderByDescending(t => t.Count).ToList();

                sb.AppendLine("Table Size Distribution:");
                sb.AppendLine("Table Name                     Record Count    Percentage");
                sb.AppendLine("-".PadRight(60, '-'));

                var totalRecords = sortedTables.Where(t => t.Count > 0).Sum(t => t.Count);

                foreach (var table in sortedTables.Take(15))
                {
                    if (table.Count >= 0)
                    {
                        var percentage = totalRecords > 0 ? (double)table.Count / totalRecords * 100 : 0;
                        sb.AppendLine($"{table.Table.PadRight(30)} {table.Count.ToString("N0").PadLeft(12)} {percentage.ToString("F1").PadLeft(10)}%");
                    }
                    else
                    {
                        sb.AppendLine($"{table.Table.PadRight(30)} {"Error".PadLeft(12)} {"N/A".PadLeft(13)}");
                    }
                }

                sb.AppendLine();
                sb.AppendLine($"Total Records Across All Tables: {totalRecords:N0}");
                if (sortedTables.Count > 0 && sortedTables.First().Count > 0)
                {
                    sb.AppendLine($"Largest Table: {sortedTables.First().Table} ({sortedTables.First().Count:N0} records)");
                }
                
                var emptyTables = sortedTables.Where(t => t.Count == 0).Count();
                if (emptyTables > 0)
                {
                    sb.AppendLine($"Empty Tables: {emptyTables}");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error analyzing record distribution: {ex.Message}");
            }

            sb.AppendLine();
        }

        private void GenerateDynamicRecommendations(StringBuilder sb, OleDbConnection connection)
        {
            sb.AppendLine("8. DYNAMIC DIAGNOSTIC RECOMMENDATIONS");
            sb.AppendLine("=".PadRight(80, '='));

            try
            {
                var tables = GetTableNames(connection);
                var recommendations = new List<string>();

                var totalTables = tables.Count;
                var largeTables = 0;
                var tablesWithArchiveFields = 0;
                var tablesWithDateFields = 0;

                foreach (var table in tables)
                {
                    try
                    {
                        string countQuery = $"SELECT COUNT(*) FROM [{table}]";
                        using (var cmd = new OleDbCommand(countQuery, connection))
                        {
                            var count = (int)cmd.ExecuteScalar();
                            if (count > 10000) largeTables++;
                        }

                        var columns = GetTableColumns(connection, table);
                        if (columns.Any(c => c.ToLower().Contains("archive")))
                            tablesWithArchiveFields++;
                        if (columns.Any(c => c.ToLower().Contains("date")))
                            tablesWithDateFields++;
                    }
                    catch
                    {
                        // Skip if analysis fails
                    }
                }

                sb.AppendLine("Based on your database characteristics:");
                sb.AppendLine();

                if (largeTables > 5)
                {
                    recommendations.Add("🔍 PERFORMANCE: Consider indexing strategies for large tables");
                    recommendations.Add("📊 MONITORING: Implement regular performance monitoring");
                }

                if (tablesWithArchiveFields > 0)
                {
                    recommendations.Add("🗄️ ARCHIVE PROCESS: Review archive field usage and consistency");
                    recommendations.Add("🔄 BACKUP: Create backups before running archive operations");
                }

                if (tablesWithDateFields > 0)
                {
                    recommendations.Add("📅 DATE ANALYSIS: Monitor date-based patterns for anomalies");
                    recommendations.Add("⏰ TEMPORAL: Consider date-based partitioning for large datasets");
                }

                recommendations.Add("🔍 INTEGRITY: Regular orphaned record checks");
                recommendations.Add("📈 MONITORING: Implement database health monitoring");
                recommendations.Add("🛠️ MAINTENANCE: Schedule regular database maintenance");
                recommendations.Add("💾 BACKUP: Implement automated backup strategies");

                foreach (var recommendation in recommendations)
                {
                    sb.AppendLine($"  • {recommendation}");
                }

                sb.AppendLine();
                sb.AppendLine("🎯 CUSTOM INVESTIGATION STEPS:");
                sb.AppendLine("  1. Identify specific business processes causing issues");
                sb.AppendLine("  2. Enable detailed logging for problematic operations");
                sb.AppendLine("  3. Create test scenarios to reproduce issues");
                sb.AppendLine("  4. Monitor database during peak usage periods");
                sb.AppendLine("  5. Validate data integrity after major operations");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Error generating recommendations: {ex.Message}");
            }

            sb.AppendLine();
        }

        private List<string> GetTableNames(OleDbConnection connection)
        {
            var tables = new List<string>();
            try
            {
                var schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                foreach (DataRow row in schemaTable.Rows)
                {
                    if (row["TABLE_TYPE"].ToString() == "TABLE" && 
                        !row["TABLE_NAME"].ToString().StartsWith("MSys"))
                    {
                        tables.Add(row["TABLE_NAME"].ToString());
                    }
                }
            }
            catch
            {
                tables.AddRange(new[] { "Docket", "DocketPackages", "DocketLine", "Packages" });
            }
            return tables;
        }

        private List<string> GetTableColumns(OleDbConnection connection, string tableName)
        {
            var columns = new List<string>();
            try
            {
                var restrictions = new object[] { null, null, tableName, null };
                var schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restrictions);
                foreach (DataRow row in schemaTable.Rows)
                {
                    columns.Add(row["COLUMN_NAME"].ToString());
                }
            }
            catch
            {
                // Return empty list if can't get columns
            }
            return columns;
        }
    }
}