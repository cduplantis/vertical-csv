# Vertical CSV Parser C# Demo

A comprehensive C# demonstration project showcasing **Vertical CSV format** advantages over traditional horizontal CSV, featuring async streaming, schema evolution, and robust parsing capabilities.

## 🚀 Quick Start

```bash
git clone <repository-url>
cd vertical-cs
dotnet run
```

## 🎯 What is Vertical CSV?

**Traditional Horizontal CSV:**
```csv
Name,Age,Email,Phone,Department,Skills[0],Skills[1],Address.Street,Address.City
John,30,john@email.com,555-1234,Engineering,C#,Python,123 Main St,New York
Jane,25,jane@email.com,555-5678,Marketing,Design,Analytics,456 Oak Ave,Los Angeles
```

**Vertical CSV (Better UX):**
```csv
Name,John,Jane
Age,30,25
Email,john@email.com,jane@email.com
Phone,555-1234,555-5678
Department,Engineering,Marketing
Skills[0],C#,Design
Skills[1],Python,Analytics
Address.Street,123 Main St,456 Oak Ave
Address.City,New York,Los Angeles
```

## ✨ Key Advantages of Vertical CSV

### 🔍 **Better Readability**
- **Field names clearly visible** on the left column
- **No horizontal scrolling** required
- **Easy to scan** and understand data structure
- **Perfect for configuration-like data**

### 📈 **Schema Evolution Friendly**
- **New fields easily visible** when added
- **Missing fields obvious** in vertical layout
- **Schema changes more transparent**
- **Backward compatibility easier to manage**

### 🏗️ **Complex Data Support**
- **Arrays**: `Skills[0]`, `Skills[1]`, `Languages[0]`
- **Nested Objects**: `Address.Street`, `Address.City`, `Address.State`
- **Variable Array Lengths**: Each record can have different array sizes
- **Flattened Structure**: Complex relationships in simple CSV format

## 🏢 Key Features

### ⚡ **Async Streaming Architecture**
```csharp
// Async streaming
await foreach (var record in parser.ParseVerticalCsvFromFileAsync(filePath, schema, cancellationToken))
{
    // Process records one at a time - constant memory usage
    ProcessRecord(record);
}
```

### 💾 **Memory Efficiency**
- **IAsyncEnumerable<T>** for lazy evaluation
- **Streaming parser** - no memory limits
- **Memory-mapped files** for files >100MB
- **Constant memory usage** regardless of file size
- **No OutOfMemoryException** risk

### 🔧 **Production Ready**
- **Cancellation token support** for responsive applications
- **ConfigureAwait(false)** for library usage
- **Robust error handling** with proper async patterns
- **Thread pool efficiency** for concurrent operations

## 📊 Schema Evolution (V1 → V4)

### V1 Schema (Basic)
```csharp
RequiredFields: Name, Age, Email
```

### V2 Schema (Contact Info)
```csharp
RequiredFields: Name, Age, Email
OptionalFields: Phone
```

### V3 Schema (Professional)
```csharp
RequiredFields: Name, Age, Email
OptionalFields: Phone, Department, StartDate
```

### V4 Schema (Complex Data)
```csharp
RequiredFields: Name, Age, Email
OptionalFields: Phone, Department, StartDate, Notes, Address.Street, Address.City...
OptionalFieldPatterns: Skills[\d+], Languages[\d+], Projects[\d+].Name...
```

## 🛠️ Robust CSV Parsing

Handles complex CSV data including:

- ✅ **Quoted fields with commas**: `"Smith, John"`
- ✅ **Embedded quotes**: `"John ""The Expert"" Doe"`
- ✅ **Multi-line content**: Notes with line breaks
- ✅ **Mixed line endings**: `\r\n`, `\n`
- ✅ **Special characters**: Apostrophes, Unicode
- ✅ **Excel compatibility**: Handles Excel CSV exports

## 📁 Project Structure

```
VerticalCsv/
├── Models/
│   ├── PersonRecord.cs      # Data model with V1-V4 schemas
│   ├── Address.cs           # Nested object model
│   └── Project.cs           # Complex object model
├── Services/
│   └── CsvParser.cs         # Async CSV parser
├── SampleData/              # Example CSV files
│   ├── vertical_v1.csv      # Basic vertical format
│   ├── horizontal_v1.csv    # Traditional format
│   ├── vertical_v4_flattened.csv # Complex data
│   └── vertical_problematic.csv  # Edge cases
├── TestDataGenerator.cs     # Large file generator
├── Program.cs              # Demo application
└── README.md              # This file
```

## 🚀 Usage Examples

### Basic Parsing
```csharp
var parser = new CsvParser();

// Parse vertical CSV with V4 schema
await foreach (var record in parser.ParseVerticalCsvFromFileAsync("data.csv", SchemaVersion.V4))
{
    Console.WriteLine($"{record.Name}: {record.Skills.Count} skills");
}
```

### Large File Processing
```csharp
// Handles files >100MB with memory-mapped files automatically
var largeFileRecords = parser.ParseVerticalCsvFromFileAsync("large-file.csv", SchemaVersion.V4);

await foreach (var record in largeFileRecords)
{
    // Constant memory usage - can process unlimited file sizes
    await ProcessRecordAsync(record);
}
```

### Schema Backward Compatibility
```csharp
// V4 data parsed with V1 schema - ignores complex fields gracefully
var records = parser.ParseVerticalCsvFromFileAsync("v4-data.csv", SchemaVersion.V1);

await foreach (var record in records)
{
    // Only basic fields (Name, Age, Email) will be populated
    Console.WriteLine($"{record.Name} - {record.Email}");
}
```

## 🧪 Demo Features

Run `dotnet run` to see demonstrations of:

1. **Horizontal vs Vertical Comparison** - Side-by-side format comparison
2. **Schema Evolution** - V1→V4 progression with same application
3. **Complex Data Structures** - Arrays, nested objects, variable lengths
4. **Robust Parsing** - Problematic CSV data handling
5. **Streaming** - Memory-efficient large file processing
6. **Async Performance** - Cancellation tokens and scalability
7. **No dependencies** - No external dependencies

## 📈 Performance Characteristics

| Feature | Traditional Parser | Vertical CSV Parser |
|---------|-------------------|-------------------|
| Memory Usage | O(file size) | O(1) constant |
| Large Files | OutOfMemoryException | Unlimited size |
| Processing | Synchronous blocking | Async non-blocking |
| Scalability | Poor under load | Excellent |
| Cancellation | Not supported | Full support |
| Schema Evolution | Difficult | Natural |

## 🔍 When to Use Vertical CSV

### ✅ **Ideal For:**
- **Configuration data** with many optional fields
- **Schema evolution** requirements
- **Complex data structures** (arrays, nested objects)
- **Human-readable** data files
- **Variable field counts** per record
- **Data with wide schemas** (many columns)

### ❌ **Consider Horizontal For:**
- **Simple tabular data** with fixed schema
- **Existing systems** expecting horizontal format
- **High-frequency trading** data (performance critical)
- **Database exports** in standard format

## 🛡️ Security

- **No code injection** vulnerabilities
- **Proper input validation** and sanitization
- **Safe parsing** of untrusted CSV files
- **Memory safety** with bounded allocations
- **Exception safety** with proper async patterns

## 🔧 Configuration

### Buffer Sizes
```csharp
// Custom buffer size for performance tuning
parser.ParseVerticalCsvAsync(stream, schema, bufferSize: 16384);
```

### Large File Threshold
```csharp
// Files >100MB automatically use memory-mapped files
// Configurable in CsvParser constructor if needed
```

## 📝 License

This project is a demonstration of C# patterns and CSV parsing techniques. Use as reference for your own implementations.

## 🤝 Contributing

Feel free to use the code as reference for your own CSV parsing implementations.

---

**💡 Key Takeaway**: Vertical CSV format provides significantly better UX for complex data structures, schema evolution, and human readability while maintaining performance and scalability.
