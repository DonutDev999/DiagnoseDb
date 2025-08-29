# DiagnoseDb - Database Schema Analysis Tool

A comprehensive Access database analysis tool for RetailManager systems with flexible database selection and diagnostic capabilities.

## Features

### 🔍 **Universal Database Analysis**
- **Flexible Database Selection** - Browse and analyze any Access database (.mdb/.accdb)
- **Dual Analysis Methods** - Choose between SQL and Schema analysis approaches
- **Pure Schema Analysis** - Clean output without hardcoded table expectations
- **Dynamic Diagnostic Analysis** - Comprehensive database integrity checking

### 📊 **Analysis Capabilities**
- **Table Structure Analysis** - Complete table, column, index, and relationship mapping
- **Data Integrity Validation** - Orphaned record detection and duplicate checking
- **Date-Based Pattern Analysis** - Temporal data distribution analysis
- **Archive Field Analysis** - Archive-related field usage and distribution
- **Record Distribution Analysis** - Table size analysis and optimization recommendations

### 🛠️ **Technical Features**
- **Automatic Fallback** - SQL method with Schema method fallback for maximum compatibility
- **Professional Reports** - Clean, structured text file exports
- **Real-time Analysis** - Progress tracking and status updates
- **Error Handling** - Robust error handling and graceful degradation

## Quick Start

### Prerequisites
- .NET Framework 4.7.2 or higher
- Microsoft Access Database Engine (for .mdb/.accdb support)
- Windows operating system

### Installation
1. Clone this repository
2. Open `DiagnoseDb.sln` in Visual Studio
3. Build the solution (Ctrl+Shift+B)
4. Run the application (F5)

### Usage

#### Traditional Analysis (Quick Start)
1. Click **"Analyze Archive"** or **"Analyze Recent"** for default databases
2. Choose between Schema or SQL analysis methods
3. View results and export reports

#### Flexible Analysis (Any Database)
1. Click **"Browse Database"** to select any Access database
2. Choose your analysis method:
   - **"Analyze (Schema)"** - Compatible OLEDB method
   - **"Analyze (SQL)"** - Detailed Access system table queries
   - **"🔍 Diagnostic Analysis"** - Comprehensive integrity analysis
3. Review detailed findings and export reports

## Analysis Methods

### Schema Method (AccessDatabaseAnalyzer)
- Uses standard OLEDB schema queries
- Compatible across different database providers
- Reliable fallback option
- Basic table/column structure analysis

### SQL Method (AccessSqlAnalyzer)
- Direct queries against Access system tables (MSysObjects, MSysColumns, etc.)
- Rich metadata including creation dates and validation rules
- Access-specific detailed analysis
- Automatic fallback to Schema method if needed

### Diagnostic Analysis (DatabaseDiagnosticAnalyzer)
- Comprehensive database integrity checking
- Orphaned record detection
- Date-based pattern analysis
- Archive field distribution analysis
- Dynamic recommendations based on database characteristics

## Output Examples

### Database Overview
```
DATABASE SCHEMA ANALYSIS - ACTUAL STRUCTURE
============================================
Total Tables: 76
Total Relationships: 84
Total Columns: 450
Total Records: 1,234,567

TABLES OVERVIEW
Table Name                     Records      Columns  Indexes
Stock                          1,234        15       3
Categories                     567          8        2
SalesOrder                     29,619       16       4
```

### Diagnostic Analysis
```
DATABASE INTEGRITY DIAGNOSTIC ANALYSIS
======================================
1. Database Overview
2. Table Relationship Analysis
3. Orphaned Records Check
4. Date-Based Pattern Analysis
5. Archive-Related Fields Analysis
6. Data Integrity Validation
7. Record Distribution Analysis
8. Dynamic Recommendations
```

## Architecture

```
DiagnoseDb Application
├── PureSchemaAnalyzer (Clean Schema Analysis)
├── AccessDatabaseAnalyzer (OLEDB Schema Method)
├── AccessSqlAnalyzer (SQL System Tables Method)
├── DatabaseDiagnosticAnalyzer (Integrity Analysis)
├── Windows Forms UI (User Interface)
└── Export Functionality (Text File Reports)
```

## Key Benefits

✅ **Universal** - Works with any Access database structure  
✅ **Flexible** - Multiple analysis methods for different needs  
✅ **Professional** - Clean, structured output without assumptions  
✅ **Comprehensive** - From basic schema to detailed diagnostics  
✅ **Reliable** - Automatic fallback and robust error handling  
✅ **User-Friendly** - Intuitive interface with clear workflows  

## Use Cases

- **Database Health Checks** - Regular integrity and performance analysis
- **Migration Planning** - Pre-migration database assessment
- **Troubleshooting** - Identify orphaned records and integrity issues
- **Documentation** - Generate comprehensive database documentation
- **Maintenance** - Ongoing database health monitoring

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Version History

- **v1.0** - Initial release with basic schema analysis
- **v1.1** - Added SQL analysis method with automatic fallback
- **v1.2** - Implemented flexible database selection
- **v1.3** - Added comprehensive diagnostic analysis
- **v1.4** - Pure schema analysis without hardcoded expectations

## Support

For support, issues, or feature requests, please open an issue on GitHub.

---

**DiagnoseDb** - Professional database analysis for RetailManager systems and beyond.