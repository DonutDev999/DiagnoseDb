using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiagnoseDb
{
    public class PureSchemaAnalyzer
    {
        private DatabaseSchema schema;

        public PureSchemaAnalyzer(DatabaseSchema databaseSchema)
        {
            schema = databaseSchema;
        }

        public string GeneratePureSchemaReport()
        {
            var sb = new StringBuilder();

            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine("DATABASE SCHEMA ANALYSIS - ACTUAL STRUCTURE");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine($"Database: {schema.DatabasePath}");
            sb.AppendLine($"Analysis Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(schema.ErrorMessage))
            {
                sb.AppendLine("ERROR:");
                sb.AppendLine(schema.ErrorMessage);
                sb.AppendLine();
                return sb.ToString();
            }

            // Database Summary
            sb.AppendLine("DATABASE SUMMARY");
            sb.AppendLine("-".PadRight(50, '-'));
            sb.AppendLine($"Total Tables: {schema.Tables.Count}");
            sb.AppendLine($"Total Relationships: {schema.Relationships.Count}");
            sb.AppendLine($"Total Columns: {schema.Tables.Sum(t => t.Columns.Count)}");
            sb.AppendLine($"Total Indexes: {schema.Tables.Sum(t => t.Indexes.Count)}");
            sb.AppendLine($"Total Records: {schema.Tables.Sum(t => Math.Max(0, t.RecordCount)):N0}");
            sb.AppendLine();

            // Tables Overview
            sb.AppendLine("TABLES OVERVIEW");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine($"{"Table Name",-30} {"Records",-12} {"Columns",-8} {"Indexes",-8} {"Primary Key",-12}");
            sb.AppendLine("-".PadRight(80, '-'));
            
            foreach (var table in schema.Tables.OrderBy(t => t.TableName))
            {
                var hasPrimaryKey = table.Indexes.Any(i => i.IsPrimaryKey) ? "Yes" : "No";
                var recordCount = table.RecordCount >= 0 ? table.RecordCount.ToString("N0") : "Unknown";
                sb.AppendLine($"{table.TableName,-30} {recordCount,-12} {table.Columns.Count,-8} {table.Indexes.Count,-8} {hasPrimaryKey,-12}");
            }
            sb.AppendLine();

            // Detailed Table Structures
            sb.AppendLine("DETAILED TABLE STRUCTURES");
            sb.AppendLine("=".PadRight(80, '='));
            GenerateDetailedTableStructures(sb);

            // Relationships
            if (schema.Relationships.Count > 0)
            {
                sb.AppendLine("DATABASE RELATIONSHIPS");
                sb.AppendLine("=".PadRight(80, '='));
                GenerateRelationshipDetails(sb);
            }

            // Data Integrity Analysis
            sb.AppendLine("DATA INTEGRITY ANALYSIS");
            sb.AppendLine("=".PadRight(80, '='));
            GenerateDataIntegrityAnalysis(sb);

            return sb.ToString();
        }

        private void GenerateDetailedTableStructures(StringBuilder sb)
        {
            foreach (var table in schema.Tables.OrderBy(t => t.TableName))
            {
                sb.AppendLine($"Table: {table.TableName}");
                sb.AppendLine($"Type: {table.TableType}");
                sb.AppendLine($"Record Count: {(table.RecordCount >= 0 ? table.RecordCount.ToString("N0") : "Unknown")}");
                sb.AppendLine();

                // Columns
                sb.AppendLine("Columns:");
                sb.AppendLine("-".PadRight(80, '-'));
                sb.AppendLine($"{"Name",-25} {"Type",-15} {"Nullable",-10} {"Max Length",-12} {"Default",-15}");
                sb.AppendLine("-".PadRight(80, '-'));
                
                foreach (var column in table.Columns.OrderBy(c => c.OrdinalPosition))
                {
                    var defaultVal = string.IsNullOrEmpty(column.DefaultValue) ? "-" : 
                        (column.DefaultValue.Length > 12 ? column.DefaultValue.Substring(0, 12) + "..." : column.DefaultValue);
                    var maxLength = column.MaxLength > 0 ? column.MaxLength.ToString() : "-";
                    sb.AppendLine($"{column.ColumnName,-25} {column.DataType,-15} {column.IsNullable,-10} {maxLength,-12} {defaultVal,-15}");
                }
                sb.AppendLine();

                // Indexes
                if (table.Indexes.Count > 0)
                {
                    sb.AppendLine("Indexes:");
                    sb.AppendLine("-".PadRight(80, '-'));
                    sb.AppendLine($"{"Name",-30} {"Column",-20} {"Primary",-8} {"Unique",-8}");
                    sb.AppendLine("-".PadRight(80, '-'));
                    
                    foreach (var index in table.Indexes.OrderBy(i => i.IndexName))
                    {
                        sb.AppendLine($"{index.IndexName,-30} {index.ColumnName,-20} {index.IsPrimaryKey,-8} {index.IsUnique,-8}");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("=".PadRight(80, '='));
                sb.AppendLine();
            }
        }

        private void GenerateRelationshipDetails(StringBuilder sb)
        {
            sb.AppendLine($"{"Foreign Table",-25} {"Foreign Column",-20} {"Primary Table",-25} {"Primary Column",-20}");
            sb.AppendLine("-".PadRight(90, '-'));
            
            foreach (var rel in schema.Relationships.OrderBy(r => r.ForeignKeyTable).ThenBy(r => r.PrimaryKeyTable))
            {
                sb.AppendLine($"{rel.ForeignKeyTable,-25} {rel.ForeignKeyColumn,-20} {rel.PrimaryKeyTable,-25} {rel.PrimaryKeyColumn,-20}");
            }
            sb.AppendLine();

            // Relationship Statistics
            var parentTables = schema.Relationships.GroupBy(r => r.PrimaryKeyTable)
                .OrderByDescending(g => g.Count())
                .Take(10);
            
            if (parentTables.Any())
            {
                sb.AppendLine("Most Referenced Tables:");
                sb.AppendLine("-".PadRight(50, '-'));
                foreach (var group in parentTables)
                {
                    sb.AppendLine($"• {group.Key} (referenced by {group.Count()} table{(group.Count() > 1 ? "s" : "")})");
                }
                sb.AppendLine();
            }
        }

        private void GenerateDataIntegrityAnalysis(StringBuilder sb)
        {
            var tablesWithoutPK = schema.Tables.Where(t => !t.Indexes.Any(i => i.IsPrimaryKey)).ToList();
            var emptyTables = schema.Tables.Where(t => t.RecordCount == 0).ToList();
            var tablesWithManyNulls = schema.Tables.Where(t => 
            {
                if (t.Columns.Count == 0) return false;
                var nullableCount = t.Columns.Count(c => c.IsNullable);
                return (double)nullableCount / t.Columns.Count > 0.7;
            }).ToList();

            // Primary Key Analysis
            if (tablesWithoutPK.Count > 0)
            {
                sb.AppendLine("Tables Without Primary Keys:");
                sb.AppendLine("-".PadRight(50, '-'));
                foreach (var table in tablesWithoutPK)
                {
                    sb.AppendLine($"⚠ {table.TableName}");
                }
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("✓ All tables have primary keys defined");
                sb.AppendLine();
            }

            // Empty Tables
            if (emptyTables.Count > 0)
            {
                sb.AppendLine("Empty Tables:");
                sb.AppendLine("-".PadRight(50, '-'));
                foreach (var table in emptyTables)
                {
                    sb.AppendLine($"• {table.TableName}");
                }
                sb.AppendLine();
            }

            // Tables with Many Nullable Columns
            if (tablesWithManyNulls.Count > 0)
            {
                sb.AppendLine("Tables with High Nullable Column Percentage (>70%):");
                sb.AppendLine("-".PadRight(50, '-'));
                foreach (var table in tablesWithManyNulls)
                {
                    var nullableCount = table.Columns.Count(c => c.IsNullable);
                    var percentage = (double)nullableCount / table.Columns.Count * 100;
                    sb.AppendLine($"• {table.TableName} ({percentage:F1}% nullable)");
                }
                sb.AppendLine();
            }

            // Summary Statistics
            sb.AppendLine("Integrity Summary:");
            sb.AppendLine("-".PadRight(50, '-'));
            sb.AppendLine($"Tables with Primary Keys: {schema.Tables.Count - tablesWithoutPK.Count}/{schema.Tables.Count}");
            sb.AppendLine($"Tables with Data: {schema.Tables.Count - emptyTables.Count}/{schema.Tables.Count}");
            sb.AppendLine($"Tables in Relationships: {GetTablesInRelationships()}/{schema.Tables.Count}");
            sb.AppendLine();
        }

        private int GetTablesInRelationships()
        {
            var tablesInRelationships = new HashSet<string>();
            foreach (var rel in schema.Relationships)
            {
                tablesInRelationships.Add(rel.ForeignKeyTable);
                tablesInRelationships.Add(rel.PrimaryKeyTable);
            }
            return tablesInRelationships.Count;
        }
    }
}