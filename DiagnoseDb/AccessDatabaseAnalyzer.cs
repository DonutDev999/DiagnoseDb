using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;

namespace DiagnoseDb
{
    public class AccessDatabaseAnalyzer
    {
        private string connectionString;
        private string databasePath;

        public AccessDatabaseAnalyzer(string dbPath)
        {
            databasePath = dbPath;
            connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={dbPath};";
        }

        public DatabaseSchema AnalyzeDatabase()
        {
            var schema = new DatabaseSchema
            {
                DatabasePath = databasePath,
                Tables = new List<TableInfo>(),
                Relationships = new List<RelationshipInfo>()
            };

            try
            {
                using (var connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    
                    // Get all tables
                    schema.Tables = GetTables(connection);
                    
                    // Get relationships
                    schema.Relationships = GetRelationships(connection);
                    
                    // Get table details for each table
                    foreach (var table in schema.Tables)
                    {
                        table.Columns = GetTableColumns(connection, table.TableName);
                        table.Indexes = GetTableIndexes(connection, table.TableName);
                        table.RecordCount = GetRecordCount(connection, table.TableName);
                    }
                }
            }
            catch (Exception ex)
            {
                schema.ErrorMessage = $"Error analyzing database: {ex.Message}";
            }

            return schema;
        }

        private List<TableInfo> GetTables(OleDbConnection connection)
        {
            var tables = new List<TableInfo>();
            
            try
            {
                var schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                
                foreach (DataRow row in schemaTable.Rows)
                {
                    string tableType = row["TABLE_TYPE"].ToString();
                    if (tableType == "TABLE")
                    {
                        tables.Add(new TableInfo
                        {
                            TableName = row["TABLE_NAME"].ToString(),
                            TableType = tableType
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error - could be due to permissions or database format
                Console.WriteLine($"Error getting tables: {ex.Message}");
            }

            return tables;
        }

        private List<ColumnInfo> GetTableColumns(OleDbConnection connection, string tableName)
        {
            var columns = new List<ColumnInfo>();
            
            try
            {
                var restrictions = new object[] { null, null, tableName, null };
                var schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restrictions);
                
                foreach (DataRow row in schemaTable.Rows)
                {
                    columns.Add(new ColumnInfo
                    {
                        ColumnName = row["COLUMN_NAME"].ToString(),
                        DataType = row["DATA_TYPE"].ToString(),
                        IsNullable = Convert.ToBoolean(row["IS_NULLABLE"]),
                        MaxLength = row["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? 
                                   Convert.ToInt32(row["CHARACTER_MAXIMUM_LENGTH"]) : 0,
                        OrdinalPosition = Convert.ToInt32(row["ORDINAL_POSITION"]),
                        DefaultValue = row["COLUMN_DEFAULT"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting columns for {tableName}: {ex.Message}");
            }

            return columns;
        }

        private List<IndexInfo> GetTableIndexes(OleDbConnection connection, string tableName)
        {
            var indexes = new List<IndexInfo>();
            
            try
            {
                var restrictions = new object[] { null, null, null, null, tableName };
                var schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Indexes, restrictions);
                
                foreach (DataRow row in schemaTable.Rows)
                {
                    indexes.Add(new IndexInfo
                    {
                        IndexName = row["INDEX_NAME"].ToString(),
                        ColumnName = row["COLUMN_NAME"].ToString(),
                        IsPrimaryKey = Convert.ToBoolean(row["PRIMARY_KEY"]),
                        IsUnique = Convert.ToBoolean(row["UNIQUE"]),
                        OrdinalPosition = Convert.ToInt32(row["ORDINAL_POSITION"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting indexes for {tableName}: {ex.Message}");
            }

            return indexes;
        }

        private List<RelationshipInfo> GetRelationships(OleDbConnection connection)
        {
            var relationships = new List<RelationshipInfo>();
            
            try
            {
                var schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, null);
                
                foreach (DataRow row in schemaTable.Rows)
                {
                    relationships.Add(new RelationshipInfo
                    {
                        ForeignKeyTable = row["FK_TABLE_NAME"].ToString(),
                        ForeignKeyColumn = row["FK_COLUMN_NAME"].ToString(),
                        PrimaryKeyTable = row["PK_TABLE_NAME"].ToString(),
                        PrimaryKeyColumn = row["PK_COLUMN_NAME"].ToString(),
                        ConstraintName = row["FK_NAME"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting relationships: {ex.Message}");
            }

            return relationships;
        }

        private int GetRecordCount(OleDbConnection connection, string tableName)
        {
            try
            {
                using (var command = new OleDbCommand($"SELECT COUNT(*) FROM [{tableName}]", connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch
            {
                return -1; // Error getting count
            }
        }

        public void ExportSchemaToTextFile(DatabaseSchema schema, string outputPath)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine("ACCESS 97 RETAIL MANAGER DATABASE SCHEMA ANALYSIS");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine($"Database: {schema.DatabasePath}");
            sb.AppendLine($"Analysis Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(schema.ErrorMessage))
            {
                sb.AppendLine("ERROR:");
                sb.AppendLine(schema.ErrorMessage);
                sb.AppendLine();
            }

            // Tables Summary
            sb.AppendLine("DATABASE SUMMARY");
            sb.AppendLine("-".PadRight(40, '-'));
            sb.AppendLine($"Total Tables: {schema.Tables.Count}");
            sb.AppendLine($"Total Relationships: {schema.Relationships.Count}");
            sb.AppendLine();

            // Table Details
            sb.AppendLine("TABLE STRUCTURES");
            sb.AppendLine("=".PadRight(80, '='));
            
            foreach (var table in schema.Tables)
            {
                sb.AppendLine($"Table: {table.TableName}");
                sb.AppendLine($"Type: {table.TableType}");
                sb.AppendLine($"Record Count: {(table.RecordCount >= 0 ? table.RecordCount.ToString() : "Unknown")}");
                sb.AppendLine();

                // Columns
                sb.AppendLine("Columns:");
                sb.AppendLine("-".PadRight(60, '-'));
                sb.AppendLine($"{"Name",-25} {"Type",-15} {"Nullable",-10} {"Max Length",-12}");
                sb.AppendLine("-".PadRight(60, '-'));
                
                foreach (var column in table.Columns)
                {
                    sb.AppendLine($"{column.ColumnName,-25} {column.DataType,-15} {column.IsNullable,-10} {column.MaxLength,-12}");
                }
                sb.AppendLine();

                // Indexes
                if (table.Indexes.Count > 0)
                {
                    sb.AppendLine("Indexes:");
                    sb.AppendLine("-".PadRight(60, '-'));
                    sb.AppendLine($"{"Name",-25} {"Column",-20} {"Primary",-8} {"Unique",-8}");
                    sb.AppendLine("-".PadRight(60, '-'));
                    
                    foreach (var index in table.Indexes)
                    {
                        sb.AppendLine($"{index.IndexName,-25} {index.ColumnName,-20} {index.IsPrimaryKey,-8} {index.IsUnique,-8}");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("=".PadRight(80, '='));
                sb.AppendLine();
            }

            // Relationships
            if (schema.Relationships.Count > 0)
            {
                sb.AppendLine("RELATIONSHIPS");
                sb.AppendLine("=".PadRight(80, '='));
                sb.AppendLine($"{"Foreign Table",-20} {"Foreign Column",-20} {"Primary Table",-20} {"Primary Column",-20}");
                sb.AppendLine("-".PadRight(80, '-'));
                
                foreach (var rel in schema.Relationships)
                {
                    sb.AppendLine($"{rel.ForeignKeyTable,-20} {rel.ForeignKeyColumn,-20} {rel.PrimaryKeyTable,-20} {rel.PrimaryKeyColumn,-20}");
                }
                sb.AppendLine();
            }

            // Data Integrity Analysis
            sb.AppendLine("DATA INTEGRITY ANALYSIS");
            sb.AppendLine("=".PadRight(80, '='));
            
            foreach (var table in schema.Tables)
            {
                sb.AppendLine($"Table: {table.TableName}");
                
                // Check for primary keys
                var primaryKeys = table.Indexes.FindAll(i => i.IsPrimaryKey);
                if (primaryKeys.Count == 0)
                {
                    sb.AppendLine("  WARNING: No primary key defined");
                }
                else
                {
                    sb.AppendLine($"  Primary Key(s): {string.Join(", ", primaryKeys.ConvertAll(pk => pk.ColumnName))}");
                }

                // Check for nullable columns
                var nullableColumns = table.Columns.FindAll(c => c.IsNullable);
                if (nullableColumns.Count > 0)
                {
                    sb.AppendLine($"  Nullable Columns: {nullableColumns.Count}");
                }

                sb.AppendLine();
            }

            // Write to file
            File.WriteAllText(outputPath, sb.ToString());
        }
    }

    // Data classes for schema information
    public class DatabaseSchema
    {
        public string DatabasePath { get; set; }
        public List<TableInfo> Tables { get; set; }
        public List<RelationshipInfo> Relationships { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class TableInfo
    {
        public string TableName { get; set; }
        public string TableType { get; set; }
        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();
        public List<IndexInfo> Indexes { get; set; } = new List<IndexInfo>();
        public int RecordCount { get; set; }
    }

    public class ColumnInfo
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public bool IsNullable { get; set; }
        public int MaxLength { get; set; }
        public int OrdinalPosition { get; set; }
        public string DefaultValue { get; set; }
    }

    public class IndexInfo
    {
        public string IndexName { get; set; }
        public string ColumnName { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
        public int OrdinalPosition { get; set; }
    }

    public class RelationshipInfo
    {
        public string ForeignKeyTable { get; set; }
        public string ForeignKeyColumn { get; set; }
        public string PrimaryKeyTable { get; set; }
        public string PrimaryKeyColumn { get; set; }
        public string ConstraintName { get; set; }
    }
}