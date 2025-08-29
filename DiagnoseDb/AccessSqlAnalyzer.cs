using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace DiagnoseDb
{
    public class AccessSqlAnalyzer
    {
        private string connectionString;
        private string databasePath;

        public AccessSqlAnalyzer(string dbPath)
        {
            databasePath = dbPath;
            connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={dbPath};";
        }

        public DatabaseSqlSchema AnalyzeDatabaseWithSql()
        {
            var schema = new DatabaseSqlSchema
            {
                DatabasePath = databasePath,
                Tables = new List<SqlTableInfo>(),
                Relationships = new List<SqlRelationshipInfo>(),
                Indexes = new List<SqlIndexInfo>()
            };

            try
            {
                using (var connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    
                    // Get actual tables using SQL queries
                    schema.Tables = GetTablesUsingSql(connection);
                    
                    // Get columns for each table
                    foreach (var table in schema.Tables)
                    {
                        table.Columns = GetTableColumnsUsingSql(connection, table.TableName);
                        table.RecordCount = GetRecordCountUsingSql(connection, table.TableName);
                    }
                    
                    // Get indexes using SQL
                    schema.Indexes = GetIndexesUsingSql(connection);
                    
                    // Get relationships using SQL
                    schema.Relationships = GetRelationshipsUsingSql(connection);
                }
            }
            catch (Exception ex)
            {
                schema.ErrorMessage = $"Error analyzing database with SQL: {ex.Message}";
            }

            return schema;
        }

        private List<SqlTableInfo> GetTablesUsingSql(OleDbConnection connection)
        {
            var tables = new List<SqlTableInfo>();
            
            try
            {
                // Query MSysObjects to get actual user tables
                string sql = @"
                    SELECT Name, Type, Flags, DateCreate, DateUpdate
                    FROM MSysObjects 
                    WHERE Type = 1 
                    AND Flags = 0 
                    AND Left(Name, 4) <> 'MSys'
                    AND Left(Name, 1) <> '~'
                    ORDER BY Name";

                using (var command = new OleDbCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(new SqlTableInfo
                            {
                                TableName = reader["Name"].ToString(),
                                TableType = "TABLE",
                                DateCreated = reader["DateCreate"] != DBNull.Value ? 
                                            Convert.ToDateTime(reader["DateCreate"]) : DateTime.MinValue,
                                DateUpdated = reader["DateUpdate"] != DBNull.Value ? 
                                            Convert.ToDateTime(reader["DateUpdate"]) : DateTime.MinValue,
                                ObjectType = Convert.ToInt32(reader["Type"]),
                                Flags = Convert.ToInt32(reader["Flags"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback to schema tables if MSysObjects is not accessible
                Console.WriteLine($"MSysObjects not accessible, using schema tables: {ex.Message}");
                return GetTablesUsingSchemaTable(connection);
            }

            return tables;
        }

        private List<SqlTableInfo> GetTablesUsingSchemaTable(OleDbConnection connection)
        {
            var tables = new List<SqlTableInfo>();
            
            try
            {
                var schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                
                foreach (DataRow row in schemaTable.Rows)
                {
                    string tableType = row["TABLE_TYPE"].ToString();
                    string tableName = row["TABLE_NAME"].ToString();
                    
                    if (tableType == "TABLE" && !tableName.StartsWith("MSys") && !tableName.StartsWith("~"))
                    {
                        tables.Add(new SqlTableInfo
                        {
                            TableName = tableName,
                            TableType = tableType,
                            DateCreated = DateTime.MinValue,
                            DateUpdated = DateTime.MinValue
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting tables using schema: {ex.Message}");
            }

            return tables;
        }

        private List<SqlColumnInfo> GetTableColumnsUsingSql(OleDbConnection connection, string tableName)
        {
            var columns = new List<SqlColumnInfo>();
            
            try
            {
                // First try MSysObjects/MSysColumns approach
                string sql = @"
                    SELECT 
                        c.Name as ColumnName,
                        c.Type as DataType,
                        c.Size as ColumnSize,
                        c.Order as OrdinalPosition,
                        c.Required as IsRequired,
                        c.AllowZeroLength as AllowZeroLength,
                        c.DefaultValue as DefaultValue,
                        c.ValidationRule as ValidationRule,
                        c.ValidationText as ValidationText
                    FROM MSysObjects o 
                    INNER JOIN MSysColumns c ON o.Id = c.ObjectId
                    WHERE o.Name = ? AND o.Type = 1
                    ORDER BY c.Order";

                using (var command = new OleDbCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("?", tableName);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            columns.Add(new SqlColumnInfo
                            {
                                ColumnName = reader["ColumnName"].ToString(),
                                DataType = GetAccessDataTypeName(Convert.ToInt32(reader["DataType"])),
                                DataTypeCode = Convert.ToInt32(reader["DataType"]),
                                ColumnSize = reader["ColumnSize"] != DBNull.Value ? Convert.ToInt32(reader["ColumnSize"]) : 0,
                                OrdinalPosition = Convert.ToInt32(reader["OrdinalPosition"]),
                                IsRequired = reader["IsRequired"] != DBNull.Value ? Convert.ToBoolean(reader["IsRequired"]) : false,
                                AllowZeroLength = reader["AllowZeroLength"] != DBNull.Value ? Convert.ToBoolean(reader["AllowZeroLength"]) : false,
                                DefaultValue = reader["DefaultValue"]?.ToString(),
                                ValidationRule = reader["ValidationRule"]?.ToString(),
                                ValidationText = reader["ValidationText"]?.ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MSysColumns not accessible for {tableName}, using schema approach: {ex.Message}");
                return GetTableColumnsUsingSchema(connection, tableName);
            }

            return columns;
        }

        private List<SqlColumnInfo> GetTableColumnsUsingSchema(OleDbConnection connection, string tableName)
        {
            var columns = new List<SqlColumnInfo>();
            
            try
            {
                var restrictions = new object[] { null, null, tableName, null };
                var schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restrictions);
                
                foreach (DataRow row in schemaTable.Rows)
                {
                    columns.Add(new SqlColumnInfo
                    {
                        ColumnName = row["COLUMN_NAME"].ToString(),
                        DataType = row["DATA_TYPE"].ToString(),
                        DataTypeCode = Convert.ToInt32(row["DATA_TYPE"]),
                        ColumnSize = row["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? 
                                   Convert.ToInt32(row["CHARACTER_MAXIMUM_LENGTH"]) : 0,
                        OrdinalPosition = Convert.ToInt32(row["ORDINAL_POSITION"]),
                        IsRequired = !Convert.ToBoolean(row["IS_NULLABLE"]),
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

        private List<SqlIndexInfo> GetIndexesUsingSql(OleDbConnection connection)
        {
            var indexes = new List<SqlIndexInfo>();
            
            try
            {
                // Query MSysIndexes for detailed index information
                string sql = @"
                    SELECT 
                        i.Name as IndexName,
                        o.Name as TableName,
                        i.Primary as IsPrimary,
                        i.Unique as IsUnique,
                        i.Foreign as IsForeign,
                        i.Required as IsRequired,
                        i.IgnoreNulls as IgnoreNulls,
                        i.Clustered as IsClustered
                    FROM MSysObjects o 
                    INNER JOIN MSysIndexes i ON o.Id = i.ObjectId
                    WHERE o.Type = 1 
                    AND Left(o.Name, 4) <> 'MSys'
                    ORDER BY o.Name, i.Name";

                using (var command = new OleDbCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            indexes.Add(new SqlIndexInfo
                            {
                                IndexName = reader["IndexName"].ToString(),
                                TableName = reader["TableName"].ToString(),
                                IsPrimary = Convert.ToBoolean(reader["IsPrimary"]),
                                IsUnique = Convert.ToBoolean(reader["IsUnique"]),
                                IsForeign = Convert.ToBoolean(reader["IsForeign"]),
                                IsRequired = Convert.ToBoolean(reader["IsRequired"]),
                                IgnoreNulls = Convert.ToBoolean(reader["IgnoreNulls"]),
                                IsClustered = Convert.ToBoolean(reader["IsClustered"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MSysIndexes not accessible, using schema approach: {ex.Message}");
                return GetIndexesUsingSchema(connection);
            }

            return indexes;
        }

        private List<SqlIndexInfo> GetIndexesUsingSchema(OleDbConnection connection)
        {
            var indexes = new List<SqlIndexInfo>();
            
            try
            {
                var schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Indexes, null);
                
                foreach (DataRow row in schemaTable.Rows)
                {
                    indexes.Add(new SqlIndexInfo
                    {
                        IndexName = row["INDEX_NAME"].ToString(),
                        TableName = row["TABLE_NAME"].ToString(),
                        ColumnName = row["COLUMN_NAME"].ToString(),
                        IsPrimary = Convert.ToBoolean(row["PRIMARY_KEY"]),
                        IsUnique = Convert.ToBoolean(row["UNIQUE"]),
                        OrdinalPosition = Convert.ToInt32(row["ORDINAL_POSITION"])
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting indexes: {ex.Message}");
            }

            return indexes;
        }

        private List<SqlRelationshipInfo> GetRelationshipsUsingSql(OleDbConnection connection)
        {
            var relationships = new List<SqlRelationshipInfo>();
            
            try
            {
                // Query MSysRelationships for detailed relationship information
                string sql = @"
                    SELECT 
                        r.szRelationship as RelationshipName,
                        r.szReferencedObject as PrimaryTable,
                        r.szObject as ForeignTable,
                        r.szReferencedColumn as PrimaryColumn,
                        r.szColumn as ForeignColumn,
                        r.grbit as Attributes,
                        r.icolKeyPos as KeyPosition
                    FROM MSysRelationships r
                    ORDER BY r.szRelationship, r.icolKeyPos";

                using (var command = new OleDbCommand(sql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            relationships.Add(new SqlRelationshipInfo
                            {
                                RelationshipName = reader["RelationshipName"].ToString(),
                                PrimaryTable = reader["PrimaryTable"].ToString(),
                                ForeignTable = reader["ForeignTable"].ToString(),
                                PrimaryColumn = reader["PrimaryColumn"].ToString(),
                                ForeignColumn = reader["ForeignColumn"].ToString(),
                                Attributes = Convert.ToInt32(reader["Attributes"]),
                                KeyPosition = Convert.ToInt32(reader["KeyPosition"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MSysRelationships not accessible, using schema approach: {ex.Message}");
                return GetRelationshipsUsingSchema(connection);
            }

            return relationships;
        }

        private List<SqlRelationshipInfo> GetRelationshipsUsingSchema(OleDbConnection connection)
        {
            var relationships = new List<SqlRelationshipInfo>();
            
            try
            {
                var schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, null);
                
                foreach (DataRow row in schemaTable.Rows)
                {
                    relationships.Add(new SqlRelationshipInfo
                    {
                        RelationshipName = row["FK_NAME"].ToString(),
                        PrimaryTable = row["PK_TABLE_NAME"].ToString(),
                        ForeignTable = row["FK_TABLE_NAME"].ToString(),
                        PrimaryColumn = row["PK_COLUMN_NAME"].ToString(),
                        ForeignColumn = row["FK_COLUMN_NAME"].ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting relationships: {ex.Message}");
            }

            return relationships;
        }

        private int GetRecordCountUsingSql(OleDbConnection connection, string tableName)
        {
            try
            {
                string sql = $"SELECT COUNT(*) FROM [{tableName}]";
                using (var command = new OleDbCommand(sql, connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch
            {
                return -1; // Error getting count
            }
        }

        private string GetAccessDataTypeName(int dataTypeCode)
        {
            switch (dataTypeCode)
            {
                case 1: return "Boolean";
                case 2: return "Byte";
                case 3: return "Integer";
                case 4: return "Long";
                case 5: return "Currency";
                case 6: return "Single";
                case 7: return "Double";
                case 8: return "DateTime";
                case 9: return "Binary";
                case 10: return "Text";
                case 11: return "OLE Object";
                case 12: return "Memo";
                case 15: return "ReplicationID";
                case 16: return "Numeric";
                case 17: return "Decimal";
                case 18: return "Char";
                case 19: return "BigInt";
                case 20: return "VarBinary";
                case 21: return "LongVarBinary";
                default: return $"Unknown({dataTypeCode})";
            }
        }

        public void ExportSqlAnalysisToTextFile(DatabaseSqlSchema schema, string outputPath)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine("DATABASE SCHEMA ANALYSIS - SQL METHOD");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine($"Database: {schema.DatabasePath}");
            sb.AppendLine($"Analysis Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Analysis Method: Direct SQL Queries to Access System Tables");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(schema.ErrorMessage))
            {
                sb.AppendLine("ERROR:");
                sb.AppendLine(schema.ErrorMessage);
                sb.AppendLine();
            }

            // Database Summary
            sb.AppendLine("DATABASE SUMMARY");
            sb.AppendLine("-".PadRight(50, '-'));
            sb.AppendLine($"Total Tables: {schema.Tables.Count}");
            sb.AppendLine($"Total Indexes: {schema.Indexes.Count}");
            sb.AppendLine($"Total Relationships: {schema.Relationships.Count}");
            sb.AppendLine($"Total Columns: {schema.Tables.Sum(t => t.Columns.Count)}");
            sb.AppendLine($"Total Records: {schema.Tables.Sum(t => Math.Max(0, t.RecordCount)):N0}");
            sb.AppendLine();

            // Tables Overview
            sb.AppendLine("TABLES OVERVIEW");
            sb.AppendLine("=".PadRight(80, '='));
            sb.AppendLine($"{"Table Name",-30} {"Records",-12} {"Columns",-8} {"Created",-20}");
            sb.AppendLine("-".PadRight(80, '-'));
            
            foreach (var table in schema.Tables.OrderBy(t => t.TableName))
            {
                var recordCount = table.RecordCount >= 0 ? table.RecordCount.ToString("N0") : "Unknown";
                var created = table.DateCreated != DateTime.MinValue ? table.DateCreated.ToString("yyyy-MM-dd HH:mm") : "-";
                sb.AppendLine($"{table.TableName,-30} {recordCount,-12} {table.Columns.Count,-8} {created,-20}");
            }
            sb.AppendLine();

            // Detailed Table Information
            sb.AppendLine("DETAILED TABLE STRUCTURES");
            sb.AppendLine("=".PadRight(80, '='));
            
            foreach (var table in schema.Tables)
            {
                sb.AppendLine($"Table: {table.TableName}");
                sb.AppendLine($"Type: {table.TableType}");
                sb.AppendLine($"Record Count: {(table.RecordCount >= 0 ? table.RecordCount.ToString() : "Unknown")}");
                if (table.DateCreated != DateTime.MinValue)
                    sb.AppendLine($"Created: {table.DateCreated:yyyy-MM-dd HH:mm:ss}");
                if (table.DateUpdated != DateTime.MinValue)
                    sb.AppendLine($"Updated: {table.DateUpdated:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine();

                // Columns with detailed information
                sb.AppendLine("Columns (SQL Analysis):");
                sb.AppendLine("-".PadRight(80, '-'));
                sb.AppendLine($"{"Name",-25} {"Type",-15} {"Size",-8} {"Required",-10} {"Default",-15}");
                sb.AppendLine("-".PadRight(80, '-'));
                
                foreach (var column in table.Columns)
                {
                    var defaultVal = string.IsNullOrEmpty(column.DefaultValue) ? "-" : column.DefaultValue;
                    var size = column.ColumnSize > 0 ? column.ColumnSize.ToString() : "-";
                    sb.AppendLine($"{column.ColumnName,-25} {column.DataType,-15} {size,-8} {column.IsRequired,-10} {defaultVal,-15}");
                }
                sb.AppendLine();

                // Validation rules if available
                var columnsWithValidation = table.Columns.FindAll(c => !string.IsNullOrEmpty(c.ValidationRule));
                if (columnsWithValidation.Count > 0)
                {
                    sb.AppendLine("Validation Rules:");
                    sb.AppendLine("-".PadRight(60, '-'));
                    foreach (var column in columnsWithValidation)
                    {
                        sb.AppendLine($"{column.ColumnName}: {column.ValidationRule}");
                        if (!string.IsNullOrEmpty(column.ValidationText))
                            sb.AppendLine($"  Message: {column.ValidationText}");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("=".PadRight(80, '='));
                sb.AppendLine();
            }

            // Index Information
            if (schema.Indexes.Count > 0)
            {
                sb.AppendLine("DATABASE INDEXES (SQL Analysis)");
                sb.AppendLine("=".PadRight(80, '='));
                sb.AppendLine($"{"Index Name",-30} {"Table",-20} {"Primary",-8} {"Unique",-8} {"Foreign",-8}");
                sb.AppendLine("-".PadRight(80, '-'));
                
                foreach (var index in schema.Indexes)
                {
                    sb.AppendLine($"{index.IndexName,-30} {index.TableName,-20} {index.IsPrimary,-8} {index.IsUnique,-8} {index.IsForeign,-8}");
                }
                sb.AppendLine();
            }

            // Relationship Information
            if (schema.Relationships.Count > 0)
            {
                sb.AppendLine("DATABASE RELATIONSHIPS (SQL Analysis)");
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

            // Write to file
            System.IO.File.WriteAllText(outputPath, sb.ToString());
        }
    }

    // SQL-specific data classes
    public class DatabaseSqlSchema
    {
        public string DatabasePath { get; set; }
        public List<SqlTableInfo> Tables { get; set; }
        public List<SqlRelationshipInfo> Relationships { get; set; }
        public List<SqlIndexInfo> Indexes { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class SqlTableInfo
    {
        public string TableName { get; set; }
        public string TableType { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public int ObjectType { get; set; }
        public int Flags { get; set; }
        public List<SqlColumnInfo> Columns { get; set; } = new List<SqlColumnInfo>();
        public int RecordCount { get; set; }
    }

    public class SqlColumnInfo
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public int DataTypeCode { get; set; }
        public int ColumnSize { get; set; }
        public int OrdinalPosition { get; set; }
        public bool IsRequired { get; set; }
        public bool AllowZeroLength { get; set; }
        public string DefaultValue { get; set; }
        public string ValidationRule { get; set; }
        public string ValidationText { get; set; }
    }

    public class SqlIndexInfo
    {
        public string IndexName { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsUnique { get; set; }
        public bool IsForeign { get; set; }
        public bool IsRequired { get; set; }
        public bool IgnoreNulls { get; set; }
        public bool IsClustered { get; set; }
        public int OrdinalPosition { get; set; }
    }

    public class SqlRelationshipInfo
    {
        public string RelationshipName { get; set; }
        public string PrimaryTable { get; set; }
        public string ForeignTable { get; set; }
        public string PrimaryColumn { get; set; }
        public string ForeignColumn { get; set; }
        public int Attributes { get; set; }
        public int KeyPosition { get; set; }
    }
}