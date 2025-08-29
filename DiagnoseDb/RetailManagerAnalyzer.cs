using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiagnoseDb
{
    public class RetailManagerAnalyzer
    {
        private DatabaseSchema schema;
        private List<string> retailManagerTables = new List<string>
        {
            "Products", "Categories", "Suppliers", "Customers", "Orders", "OrderDetails",
            "Inventory", "Sales", "SalesDetails", "Employees", "Stores", "Transactions",
            "PriceHistory", "Discounts", "Returns", "StockMovements", "Vendors"
        };

        public RetailManagerAnalyzer(DatabaseSchema databaseSchema)
        {
            schema = databaseSchema;
        }

        public string GenerateAnalysisReport()
        {
            var sb = new StringBuilder();

            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine("RETAIL MANAGER DATABASE ANALYSIS REPORT");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine($"Database: {schema.DatabasePath}");
            sb.AppendLine($"Analysis Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(schema.ErrorMessage))
            {
                sb.AppendLine("CRITICAL ERROR:");
                sb.AppendLine(schema.ErrorMessage);
                sb.AppendLine();
                return sb.ToString();
            }

            // Executive Summary
            sb.AppendLine("EXECUTIVE SUMMARY");
            sb.AppendLine("-".PadRight(50, '-'));
            sb.AppendLine($"Total Tables Found: {schema.Tables.Count}");
            sb.AppendLine($"Total Relationships: {schema.Relationships.Count}");
            sb.AppendLine($"Retail Manager Tables Identified: {GetRetailManagerTableCount()}");
            sb.AppendLine($"Database Health Score: {CalculateHealthScore()}/100");
            sb.AppendLine();

            // Retail Manager Table Analysis
            sb.AppendLine("RETAIL MANAGER TABLE ANALYSIS");
            sb.AppendLine("=".PadRight(80, '='));
            AnalyzeRetailManagerTables(sb);

            // Data Integrity Assessment
            sb.AppendLine("DATA INTEGRITY ASSESSMENT");
            sb.AppendLine("=".PadRight(80, '='));
            AnalyzeDataIntegrity(sb);

            // Relationship Analysis
            sb.AppendLine("RELATIONSHIP ANALYSIS");
            sb.AppendLine("=".PadRight(80, '='));
            AnalyzeRelationships(sb);

            // Performance Recommendations
            sb.AppendLine("PERFORMANCE RECOMMENDATIONS");
            sb.AppendLine("=".PadRight(80, '='));
            GenerateRecommendations(sb);

            // Detailed Table Structures
            sb.AppendLine("DETAILED TABLE STRUCTURES");
            sb.AppendLine("=".PadRight(80, '='));
            GenerateDetailedTableInfo(sb);

            return sb.ToString();
        }

        private int GetRetailManagerTableCount()
        {
            return schema.Tables.Count(t => retailManagerTables.Any(rt => 
                t.TableName.ToLower().Contains(rt.ToLower()) || 
                rt.ToLower().Contains(t.TableName.ToLower())));
        }

        private int CalculateHealthScore()
        {
            int score = 100;
            
            // Deduct points for missing primary keys
            foreach (var table in schema.Tables)
            {
                if (!table.Indexes.Any(i => i.IsPrimaryKey))
                {
                    score -= 10;
                }
            }

            // Deduct points for tables without relationships
            var tablesWithRelationships = new HashSet<string>();
            foreach (var rel in schema.Relationships)
            {
                tablesWithRelationships.Add(rel.ForeignKeyTable);
                tablesWithRelationships.Add(rel.PrimaryKeyTable);
            }
            
            int isolatedTables = schema.Tables.Count - tablesWithRelationships.Count;
            score -= isolatedTables * 5;

            // Deduct points for empty tables
            foreach (var table in schema.Tables)
            {
                if (table.RecordCount == 0)
                {
                    score -= 3;
                }
            }

            return Math.Max(0, score);
        }

        private void AnalyzeRetailManagerTables(StringBuilder sb)
        {
            sb.AppendLine("Expected Retail Manager Tables:");
            sb.AppendLine("-".PadRight(60, '-'));
            
            foreach (var expectedTable in retailManagerTables)
            {
                var foundTable = schema.Tables.FirstOrDefault(t => 
                    t.TableName.ToLower().Contains(expectedTable.ToLower()) ||
                    expectedTable.ToLower().Contains(t.TableName.ToLower()));
                
                if (foundTable != null)
                {
                    sb.AppendLine($"✓ {expectedTable,-20} -> Found: {foundTable.TableName} ({foundTable.RecordCount} records)");
                }
                else
                {
                    sb.AppendLine($"✗ {expectedTable,-20} -> NOT FOUND");
                }
            }
            sb.AppendLine();

            sb.AppendLine("Additional Tables Found:");
            sb.AppendLine("-".PadRight(60, '-'));
            var additionalTables = schema.Tables.Where(t => 
                !retailManagerTables.Any(rt => 
                    t.TableName.ToLower().Contains(rt.ToLower()) || 
                    rt.ToLower().Contains(t.TableName.ToLower())));
            
            foreach (var table in additionalTables)
            {
                sb.AppendLine($"• {table.TableName} ({table.RecordCount} records)");
            }
            sb.AppendLine();
        }

        private void AnalyzeDataIntegrity(StringBuilder sb)
        {
            var issues = new List<string>();

            foreach (var table in schema.Tables)
            {
                // Check for primary keys
                if (!table.Indexes.Any(i => i.IsPrimaryKey))
                {
                    issues.Add($"Table '{table.TableName}' lacks a primary key");
                }

                // Check for excessive nullable columns
                var nullableCount = table.Columns.Count(c => c.IsNullable);
                var nullablePercentage = (double)nullableCount / table.Columns.Count * 100;
                if (nullablePercentage > 70)
                {
                    issues.Add($"Table '{table.TableName}' has {nullablePercentage:F1}% nullable columns (may indicate design issues)");
                }

                // Check for empty tables
                if (table.RecordCount == 0)
                {
                    issues.Add($"Table '{table.TableName}' is empty");
                }
            }

            if (issues.Count == 0)
            {
                sb.AppendLine("✓ No major data integrity issues detected.");
            }
            else
            {
                sb.AppendLine("Data Integrity Issues Found:");
                sb.AppendLine("-".PadRight(60, '-'));
                foreach (var issue in issues)
                {
                    sb.AppendLine($"⚠ {issue}");
                }
            }
            sb.AppendLine();
        }

        private void AnalyzeRelationships(StringBuilder sb)
        {
            if (schema.Relationships.Count == 0)
            {
                sb.AppendLine("⚠ No foreign key relationships found in the database.");
                sb.AppendLine("  This may indicate:");
                sb.AppendLine("  - Relationships are enforced at application level");
                sb.AppendLine("  - Database design lacks referential integrity");
                sb.AppendLine();
                return;
            }

            sb.AppendLine("Foreign Key Relationships:");
            sb.AppendLine("-".PadRight(80, '-'));
            sb.AppendLine($"{"Parent Table",-20} {"Parent Column",-20} {"Child Table",-20} {"Child Column",-20}");
            sb.AppendLine("-".PadRight(80, '-'));

            foreach (var rel in schema.Relationships)
            {
                sb.AppendLine($"{rel.PrimaryKeyTable,-20} {rel.PrimaryKeyColumn,-20} {rel.ForeignKeyTable,-20} {rel.ForeignKeyColumn,-20}");
            }
            sb.AppendLine();

            // Analyze relationship patterns
            var parentTables = schema.Relationships.GroupBy(r => r.PrimaryKeyTable)
                .OrderByDescending(g => g.Count());
            
            sb.AppendLine("Most Referenced Tables (Potential Master Tables):");
            sb.AppendLine("-".PadRight(60, '-'));
            foreach (var group in parentTables.Take(5))
            {
                sb.AppendLine($"• {group.Key} (referenced by {group.Count()} tables)");
            }
            sb.AppendLine();
        }

        private void GenerateRecommendations(StringBuilder sb)
        {
            var recommendations = new List<string>();

            // Check for missing indexes on foreign keys
            foreach (var rel in schema.Relationships)
            {
                var childTable = schema.Tables.FirstOrDefault(t => t.TableName == rel.ForeignKeyTable);
                if (childTable != null)
                {
                    var hasIndex = childTable.Indexes.Any(i => i.ColumnName == rel.ForeignKeyColumn);
                    if (!hasIndex)
                    {
                        recommendations.Add($"Add index on {rel.ForeignKeyTable}.{rel.ForeignKeyColumn} for better join performance");
                    }
                }
            }

            // Check for large tables without indexes
            foreach (var table in schema.Tables.Where(t => t.RecordCount > 1000))
            {
                if (table.Indexes.Count <= 1) // Only primary key or no indexes
                {
                    recommendations.Add($"Consider adding indexes to table '{table.TableName}' ({table.RecordCount} records)");
                }
            }

            // Check for tables with too many columns
            foreach (var table in schema.Tables.Where(t => t.Columns.Count > 20))
            {
                recommendations.Add($"Table '{table.TableName}' has {table.Columns.Count} columns - consider normalization");
            }

            if (recommendations.Count == 0)
            {
                sb.AppendLine("✓ No immediate performance recommendations.");
            }
            else
            {
                foreach (var recommendation in recommendations)
                {
                    sb.AppendLine($"• {recommendation}");
                }
            }
            sb.AppendLine();
        }

        private void GenerateDetailedTableInfo(StringBuilder sb)
        {
            foreach (var table in schema.Tables.OrderBy(t => t.TableName))
            {
                sb.AppendLine($"TABLE: {table.TableName}");
                sb.AppendLine("=".PadRight(60, '='));
                sb.AppendLine($"Records: {table.RecordCount}");
                sb.AppendLine($"Columns: {table.Columns.Count}");
                sb.AppendLine($"Indexes: {table.Indexes.Count}");
                sb.AppendLine();

                // Column details
                sb.AppendLine("Columns:");
                sb.AppendLine("-".PadRight(70, '-'));
                sb.AppendLine($"{"Name",-25} {"Type",-15} {"Nullable",-10} {"Length",-10} {"Default",-15}");
                sb.AppendLine("-".PadRight(70, '-'));
                
                foreach (var column in table.Columns.OrderBy(c => c.OrdinalPosition))
                {
                    var defaultVal = string.IsNullOrEmpty(column.DefaultValue) ? "-" : column.DefaultValue;
                    var length = column.MaxLength > 0 ? column.MaxLength.ToString() : "-";
                    sb.AppendLine($"{column.ColumnName,-25} {column.DataType,-15} {column.IsNullable,-10} {length,-10} {defaultVal,-15}");
                }
                sb.AppendLine();

                // Index details
                if (table.Indexes.Count > 0)
                {
                    sb.AppendLine("Indexes:");
                    sb.AppendLine("-".PadRight(70, '-'));
                    sb.AppendLine($"{"Name",-30} {"Column",-20} {"Primary",-8} {"Unique",-8}");
                    sb.AppendLine("-".PadRight(70, '-'));
                    
                    foreach (var index in table.Indexes.OrderBy(i => i.IndexName))
                    {
                        sb.AppendLine($"{index.IndexName,-30} {index.ColumnName,-20} {index.IsPrimaryKey,-8} {index.IsUnique,-8}");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine();
            }
        }
    }
}