using VerticalCsv.Models;
using VerticalCsv.Services;

namespace VerticalCsv;

class Program
{
    static async Task Main(string[] args)
    {
        var parser = new CsvParser();

        Console.WriteLine("=== Vertical CSV Demo: Better UX with Schema Evolution ===\n");

        Console.WriteLine("1. HORIZONTAL CSV (Traditional Format):");
        Console.WriteLine("   - Harder to read when many columns");
        Console.WriteLine("   - Requires horizontal scrolling");
        Console.WriteLine("   - Less intuitive for configuration-like data\n");

        await TestHorizontalCsv(parser);

        Console.WriteLine("\n" + new string('=', 60) + "\n");

        Console.WriteLine("2. VERTICAL CSV (Better UX Format):");
        Console.WriteLine("   - Easy to read field names");
        Console.WriteLine("   - No horizontal scrolling needed");
        Console.WriteLine("   - Perfect for configuration and structured data");
        Console.WriteLine("   - Scales better with schema evolution\n");

        await TestVerticalCsv(parser);

        Console.WriteLine("\n" + new string('=', 60) + "\n");
        Console.WriteLine("3. V4 RELATIONAL DATA & ARRAYS:");
        Console.WriteLine("   - Complex data structures (arrays, nested objects)");
        Console.WriteLine("   - Multiple projects per person");
        Console.WriteLine("   - Address information");
        Console.WriteLine("   - Skills and language arrays\n");

        await TestV4Features(parser);

        Console.WriteLine("\n" + new string('=', 60) + "\n");
        Console.WriteLine("4. SCHEMA EVOLUTION DEMO:");
        Console.WriteLine("   - Same application supports V1, V2, V3, and V4 schemas");
        Console.WriteLine("   - Graceful handling of missing optional fields");
        Console.WriteLine("   - Vertical format makes schema changes more visible\n");

        await TestSchemaEvolution(parser);
    }

    static async Task TestHorizontalCsv(CsvParser parser)
    {
        Console.WriteLine("Loading horizontal_v1.csv (Schema V1):");
        var recordsH1 = new List<PersonRecord>();
        await foreach (var record in parser.ParseHorizontalCsvFromFileAsync("SampleData/horizontal_v1.csv", SchemaVersion.V1))
        {
            recordsH1.Add(record);
        }
        DisplayRecords(recordsH1, "Horizontal V1");

        Console.WriteLine("\nLoading horizontal_v3.csv (Schema V3):");
        var recordsH3 = new List<PersonRecord>();
        await foreach (var record in parser.ParseHorizontalCsvFromFileAsync("SampleData/horizontal_v3.csv", SchemaVersion.V3))
        {
            recordsH3.Add(record);
        }
        DisplayRecords(recordsH3, "Horizontal V3");
    }

    static async Task TestVerticalCsv(CsvParser parser)
    {
        Console.WriteLine("Loading vertical_v1.csv (Schema V1):");
        var recordsV1 = new List<PersonRecord>();
        await foreach (var record in parser.ParseVerticalCsvFromFileAsync("SampleData/vertical_v1.csv", SchemaVersion.V1))
        {
            recordsV1.Add(record);
        }
        DisplayRecords(recordsV1, "Vertical V1");

        Console.WriteLine("\nLoading vertical_v3.csv (Schema V3):");
        var recordsV3 = new List<PersonRecord>();
        await foreach (var record in parser.ParseVerticalCsvFromFileAsync("SampleData/vertical_v3.csv", SchemaVersion.V3))
        {
            recordsV3.Add(record);
        }
        DisplayRecords(recordsV3, "Vertical V3");
    }

    static async Task TestV4Features(CsvParser parser)
    {
        Console.WriteLine("Loading vertical_v4_flattened.csv (Schema V4 - Flattened Complex Data):");
        var recordsV4 = new List<PersonRecord>();
        await foreach (var record in parser.ParseVerticalCsvFromFileAsync("SampleData/vertical_v4_flattened.csv", SchemaVersion.V4))
        {
            recordsV4.Add(record);
        }
        DisplayRecordsV4(recordsV4, "Vertical V4 Flattened");

        Console.WriteLine("\nCompare: Loading horizontal_v4_flattened.csv (Same data, much harder to read):");
        var recordsH4 = new List<PersonRecord>();
        await foreach (var record in parser.ParseHorizontalCsvFromFileAsync("SampleData/horizontal_v4_flattened.csv", SchemaVersion.V4))
        {
            recordsH4.Add(record);
        }
        DisplayRecordsV4(recordsH4, "Horizontal V4 Flattened");

        Console.WriteLine("NOTICE: With flattened structure, vertical format is significantly more readable!");
        Console.WriteLine("- Field names are clearly visible on the left");
        Console.WriteLine("- Arrays and nested objects are explicit (Skills[0], Address.Street)");
        Console.WriteLine("- No need to scroll horizontally to see all fields");
        Console.WriteLine("- Easy to understand data structure at a glance");

        Console.WriteLine("\n" + new string('-', 40));
        Console.WriteLine("VARIABLE ARRAY LENGTHS DEMO:");
        Console.WriteLine("Loading vertical_v4_variable.csv (Different array sizes per person):");
        var recordsV4Variable = new List<PersonRecord>();
        await foreach (var record in parser.ParseVerticalCsvFromFileAsync("SampleData/vertical_v4_variable.csv", SchemaVersion.V4))
        {
            recordsV4Variable.Add(record);
        }
        DisplayRecordsV4(recordsV4Variable, "Variable Arrays V4");

        Console.WriteLine("NOTICE: Each person can have different numbers of skills and projects!");
        Console.WriteLine("- Alice: 1 skill, 1 project");
        Console.WriteLine("- Bob: 6 skills, 3 projects");
        Console.WriteLine("- Carol: 4 skills, 4 projects");
        Console.WriteLine("- David: 3 skills, 2 projects");
        Console.WriteLine("Schema patterns handle any number of array elements dynamically!");

        Console.WriteLine("\n" + new string('-', 40));
        Console.WriteLine("ROBUST CSV PARSING DEMO:");
        Console.WriteLine("Testing problematic CSV data (quotes, commas, newlines in data)...");

        Console.WriteLine("\nLoading vertical_problematic.csv:");
        var recordsVP = new List<PersonRecord>();
        await foreach (var record in parser.ParseVerticalCsvFromFileAsync("SampleData/vertical_problematic.csv", SchemaVersion.V4))
        {
            recordsVP.Add(record);
        }
        DisplayRecordsWithNotes(recordsVP, "Vertical Problematic");

        Console.WriteLine("Loading horizontal_problematic.csv:");
        var recordsHP = new List<PersonRecord>();
        await foreach (var record in parser.ParseHorizontalCsvFromFileAsync("SampleData/horizontal_problematic.csv", SchemaVersion.V4))
        {
            recordsHP.Add(record);
        }
        DisplayRecordsWithNotes(recordsHP, "Horizontal Problematic");

        Console.WriteLine("ROBUST PARSING FEATURES:");
        Console.WriteLine("✓ Handles quoted fields with commas");
        Console.WriteLine("✓ Manages embedded quotes (\"\")");
        Console.WriteLine("✓ Supports multi-line content within fields");
        Console.WriteLine("✓ Processes mixed line endings (\\r\\n, \\n)");
        Console.WriteLine("✓ Handles apostrophes and special characters");
        Console.WriteLine("✓ Excel-compatible CSV parsing");

        Console.WriteLine("\n" + new string('-', 40));
        Console.WriteLine("STREAMING DEMO:");
        Console.WriteLine("Testing streaming parser for memory efficiency...");

        Console.WriteLine("\nStreaming vertical_v4_variable.csv (record-by-record processing):");
        var streamingRecords = parser.ParseVerticalCsvFromFileAsync("SampleData/vertical_v4_variable.csv", SchemaVersion.V4);

        int recordCount = 0;
        Console.WriteLine("Processing records lazily:");
        await foreach (var record in streamingRecords)
        {
            recordCount++;
            Console.WriteLine($"  Record {recordCount}: {record.Name} ({record.Department})");

            if (recordCount >= 2)
            {
                Console.WriteLine("  ... (stopping early to demonstrate lazy evaluation)");
                break;
            }
        }

        Console.WriteLine("\nFEATURES:");
        Console.WriteLine("✓ Streaming parser - no memory limits");
        Console.WriteLine("✓ IEnumerable<T> lazy evaluation");
        Console.WriteLine("✓ Memory-mapped files for files >100MB");
        Console.WriteLine("✓ Configurable buffer sizes");
        Console.WriteLine("✓ File-based API for direct file access");
        Console.WriteLine("✓ Automatic large file detection");
        Console.WriteLine("✓ No OutOfMemoryException risk");

        Console.WriteLine("\n" + new string('-', 40));
        Console.WriteLine("LARGE FILE SIMULATION:");
        Console.WriteLine("Generating large CSV file (10,000 records) for testing...");

        var largeFilePath = "SampleData/large_test.csv";
        TestDataGenerator.GenerateLargeVerticalCsv(largeFilePath, 10000);

        var fileInfo = new FileInfo(largeFilePath);
        Console.WriteLine($"Generated file: {fileInfo.Length / 1024.0 / 1024.0:F2} MB");

        Console.WriteLine("\nStreaming large file (processing only first 3 records):");
        var largeFileRecords = parser.ParseVerticalCsvFromFileAsync(largeFilePath, SchemaVersion.V4);

        int count = 0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await foreach (var record in largeFileRecords)
        {
            count++;
            Console.WriteLine($"  Record {count}: {record.Name}, {record.Age} years old, {record.Department}");

            if (count >= 3) break;
        }
        stopwatch.Stop();

        Console.WriteLine($"\nProcessed {count} records in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine("✓ Memory usage: Constant (no matter how large the file)");
        Console.WriteLine("✓ Processing time: Linear with processed records, not file size");
        Console.WriteLine("✓ Can handle files larger than available RAM");

        Console.WriteLine("\n" + new string('-', 40));
        Console.WriteLine("ASYNC/AWAIT DEMO:");
        Console.WriteLine("Testing async streaming for scalability and responsiveness...");

        Console.WriteLine("\nAsync processing of large file (with cancellation support):");
        await TestAsyncProcessing(parser, largeFilePath);

        Console.WriteLine("\nASYNC BENEFITS:");
        Console.WriteLine("✓ Non-blocking IO operations");
        Console.WriteLine("✓ Thread pool efficiency for concurrent operations");
        Console.WriteLine("✓ Cancellation token support");
        Console.WriteLine("✓ IAsyncEnumerable<T> for async streaming");
        Console.WriteLine("✓ Better scalability under load");
        Console.WriteLine("✓ Responsive UI applications");
        Console.WriteLine("✓ Modern API standards");
    }

    static async Task TestAsyncProcessing(CsvParser parser, string filePath)
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5)); // 5 second timeout for demo

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            int recordCount = 0;

            await foreach (var record in parser.ParseVerticalCsvFromFileAsync(filePath, SchemaVersion.V4, cts.Token))
            {
                recordCount++;
                Console.WriteLine($"  Async Record {recordCount}: {record.Name}, {record.Age} years old");

                if (recordCount >= 5) break; // Process only first 5 for demo
            }

            stopwatch.Stop();
            Console.WriteLine($"Async processed {recordCount} records in {stopwatch.ElapsedMilliseconds}ms");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was cancelled (demonstrating cancellation support)");
        }
    }

    static async Task TestSchemaEvolution(CsvParser parser)
    {
        Console.WriteLine("Testing backward compatibility:");

        Console.WriteLine("- Parsing V4 flattened data with V1 schema (should work, ignoring complex fields):");
        var v4DataWithV1Schema = new List<PersonRecord>();
        await foreach (var record in parser.ParseVerticalCsvFromFileAsync("SampleData/vertical_v4_flattened.csv", SchemaVersion.V1))
        {
            v4DataWithV1Schema.Add(record);
        }
        DisplayRecords(v4DataWithV1Schema, "V4 Flattened + V1 Schema");

        Console.WriteLine("- Parsing V4 flattened data with V3 schema (partial fields):");
        var v4DataWithV3Schema = new List<PersonRecord>();
        await foreach (var record in parser.ParseVerticalCsvFromFileAsync("SampleData/vertical_v4_flattened.csv", SchemaVersion.V3))
        {
            v4DataWithV3Schema.Add(record);
        }
        DisplayRecords(v4DataWithV3Schema, "V4 Flattened + V3 Schema");

        Console.WriteLine("- Parsing V4 flattened data with V4 schema (full flattened fields):");
        var v4DataWithV4Schema = new List<PersonRecord>();
        await foreach (var record in parser.ParseVerticalCsvFromFileAsync("SampleData/vertical_v4_flattened.csv", SchemaVersion.V4))
        {
            v4DataWithV4Schema.Add(record);
        }
        DisplayRecordsV4(v4DataWithV4Schema, "V4 Flattened + V4 Schema");
    }

    static void DisplayRecords(List<PersonRecord> records, string title)
    {
        Console.WriteLine($"[{title}] Found {records.Count} records:");
        foreach (var record in records)
        {
            Console.WriteLine($"  Name: {record.Name}, Age: {record.Age}, Email: {record.Email}");
            if (!string.IsNullOrEmpty(record.Phone))
                Console.WriteLine($"    Phone: {record.Phone}");
            if (!string.IsNullOrEmpty(record.Department))
                Console.WriteLine($"    Department: {record.Department}");
            if (record.StartDate.HasValue)
                Console.WriteLine($"    Start Date: {record.StartDate:yyyy-MM-dd}");
        }
        Console.WriteLine();
    }

    static void DisplayRecordsV4(List<PersonRecord> records, string title)
    {
        Console.WriteLine($"[{title}] Found {records.Count} records:");
        foreach (var record in records)
        {
            Console.WriteLine($"  Name: {record.Name}, Age: {record.Age}, Email: {record.Email}");
            if (!string.IsNullOrEmpty(record.Phone))
                Console.WriteLine($"    Phone: {record.Phone}");
            if (!string.IsNullOrEmpty(record.Department))
                Console.WriteLine($"    Department: {record.Department}");
            if (record.StartDate.HasValue)
                Console.WriteLine($"    Start Date: {record.StartDate:yyyy-MM-dd}");

            var nonEmptySkills = record.Skills.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (nonEmptySkills.Any())
                Console.WriteLine($"    Skills: {string.Join(", ", nonEmptySkills)}");

            var nonEmptyLanguages = record.Languages.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            if (nonEmptyLanguages.Any())
                Console.WriteLine($"    Languages: {string.Join(", ", nonEmptyLanguages)}");

            if (record.Address != null)
                Console.WriteLine($"    Address: {record.Address.Street}, {record.Address.City}, {record.Address.State} {record.Address.ZipCode}");

            var nonEmptyProjects = record.Projects.Where(p => !string.IsNullOrWhiteSpace(p.Name)).ToList();
            if (nonEmptyProjects.Any())
            {
                Console.WriteLine($"    Projects ({nonEmptyProjects.Count}):");
                foreach (var project in nonEmptyProjects)
                {
                    var endDateStr = project.EndDate?.ToString("yyyy-MM-dd") ?? "Ongoing";
                    Console.WriteLine($"      - {project.Name} as {project.Role} ({project.StartDate:yyyy-MM-dd} to {endDateStr})");
                }
            }
        }
        Console.WriteLine();
    }

    static void DisplayRecordsWithNotes(List<PersonRecord> records, string title)
    {
        Console.WriteLine($"[{title}] Found {records.Count} records:");
        foreach (var record in records)
        {
            Console.WriteLine($"  Name: {record.Name}, Age: {record.Age}, Email: {record.Email}");
            if (!string.IsNullOrEmpty(record.Phone))
                Console.WriteLine($"    Phone: {record.Phone}");
            if (!string.IsNullOrEmpty(record.Department))
                Console.WriteLine($"    Department: {record.Department}");
            if (record.StartDate.HasValue)
                Console.WriteLine($"    Start Date: {record.StartDate:yyyy-MM-dd}");

            if (!string.IsNullOrEmpty(record.Notes))
            {
                Console.WriteLine($"    Notes: {record.Notes}");
            }

            var nonEmptySkills = record.Skills.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (nonEmptySkills.Any())
                Console.WriteLine($"    Skills: {string.Join(", ", nonEmptySkills)}");

            var nonEmptyLanguages = record.Languages.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            if (nonEmptyLanguages.Any())
                Console.WriteLine($"    Languages: {string.Join(", ", nonEmptyLanguages)}");

            if (record.Address != null)
                Console.WriteLine($"    Address: {record.Address.Street}, {record.Address.City}, {record.Address.State} {record.Address.ZipCode}");
        }
        Console.WriteLine();
    }
}
