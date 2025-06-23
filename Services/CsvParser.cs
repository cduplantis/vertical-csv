using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.RegularExpressions;
using VerticalCsv.Models;

namespace VerticalCsv.Services;

public class CsvParser
{
    private const int DefaultBufferSize = 8192;


    private PersonRecord? ParseRecord(string[] headers, string[] values, SchemaVersion schema)
    {
        var record = new PersonRecord();

        for (int i = 0; i < Math.Min(headers.Length, values.Length); i++)
        {
            var header = headers[i];
            var value = values[i];

            if (string.IsNullOrWhiteSpace(value)) continue;

            switch (header.ToLower())
            {
                case "name":
                    record.Name = value;
                    break;
                case "age":
                    if (int.TryParse(value, out int age))
                        record.Age = age;
                    break;
                case "email":
                    record.Email = value;
                    break;
                case "phone":
                    record.Phone = value;
                    break;
                case "department":
                    record.Department = value;
                    break;
                case "startdate":
                    if (DateTime.TryParse(value, out DateTime startDate))
                        record.StartDate = startDate;
                    break;
                case "notes":
                    record.Notes = value;
                    break;
                default:
                    ParseFlattenedField(record, header, value);
                    break;
            }
        }

        if (IsValidRecord(record, schema))
            return record;

        return null;
    }

    private bool IsValidRecord(PersonRecord record, SchemaVersion schema)
    {
        foreach (var field in schema.RequiredFields)
        {
            switch (field.ToLower())
            {
                case "name" when string.IsNullOrWhiteSpace(record.Name):
                case "email" when string.IsNullOrWhiteSpace(record.Email):
                case "age" when record.Age <= 0:
                    return false;
            }
        }
        return true;
    }

    private List<string> ParseArray(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<string>();

        return value.Split(';').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
    }

    private Address? ParseAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var parts = value.Split('|').Select(s => s.Trim()).ToArray();
        if (parts.Length < 4)
            return null;

        return new Address
        {
            Street = parts[0],
            City = parts[1],
            State = parts[2],
            ZipCode = parts[3]
        };
    }

    private List<Project> ParseProjects(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<Project>();

        var projects = new List<Project>();
        var projectStrings = value.Split('~').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));

        foreach (var projectString in projectStrings)
        {
            var parts = projectString.Split('|').Select(s => s.Trim()).ToArray();
            if (parts.Length >= 3)
            {
                var project = new Project
                {
                    Name = parts[0],
                    Role = parts[1]
                };

                if (DateTime.TryParse(parts[2], out DateTime startDate))
                    project.StartDate = startDate;

                if (parts.Length > 3 && DateTime.TryParse(parts[3], out DateTime endDate))
                    project.EndDate = endDate;

                projects.Add(project);
            }
        }

        return projects;
    }

    private void ParseFlattenedField(PersonRecord record, string header, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var lowerHeader = header.ToLower();

        if (lowerHeader.StartsWith("skills[") && lowerHeader.EndsWith("]"))
        {
            var indexStr = lowerHeader.Substring(7, lowerHeader.Length - 8);
            if (int.TryParse(indexStr, out int index))
            {
                while (record.Skills.Count <= index)
                    record.Skills.Add(string.Empty);
                record.Skills[index] = value;
            }
        }
        else if (lowerHeader.StartsWith("languages[") && lowerHeader.EndsWith("]"))
        {
            var indexStr = lowerHeader.Substring(10, lowerHeader.Length - 11);
            if (int.TryParse(indexStr, out int index))
            {
                while (record.Languages.Count <= index)
                    record.Languages.Add(string.Empty);
                record.Languages[index] = value;
            }
        }
        else if (lowerHeader.StartsWith("address."))
        {
            if (record.Address == null)
                record.Address = new Address();

            switch (lowerHeader)
            {
                case "address.street":
                    record.Address.Street = value;
                    break;
                case "address.city":
                    record.Address.City = value;
                    break;
                case "address.state":
                    record.Address.State = value;
                    break;
                case "address.zipcode":
                    record.Address.ZipCode = value;
                    break;
            }
        }
        else if (lowerHeader.StartsWith("projects[") && lowerHeader.Contains("]."))
        {
            var bracketEnd = lowerHeader.IndexOf(']');
            var indexStr = lowerHeader.Substring(9, bracketEnd - 9);
            var property = lowerHeader.Substring(bracketEnd + 2);

            if (int.TryParse(indexStr, out int index))
            {
                while (record.Projects.Count <= index)
                    record.Projects.Add(new Project());

                var project = record.Projects[index];
                switch (property)
                {
                    case "name":
                        project.Name = value;
                        break;
                    case "role":
                        project.Role = value;
                        break;
                    case "startdate":
                        if (DateTime.TryParse(value, out DateTime startDate))
                            project.StartDate = startDate;
                        break;
                    case "enddate":
                        if (DateTime.TryParse(value, out DateTime endDate))
                            project.EndDate = endDate;
                        break;
                }
            }
        }
    }

    private bool IsFieldValidForSchema(string fieldName, SchemaVersion schema)
    {
        var lowerFieldName = fieldName.ToLower();
        var lowerRequiredFields = schema.RequiredFields.Select(f => f.ToLower()).ToList();
        var lowerOptionalFields = schema.OptionalFields.Select(f => f.ToLower()).ToList();

        if (lowerRequiredFields.Contains(lowerFieldName) || lowerOptionalFields.Contains(lowerFieldName))
            return true;

        foreach (var pattern in schema.OptionalFieldPatterns)
        {
            if (Regex.IsMatch(fieldName, pattern, RegexOptions.IgnoreCase))
                return true;
        }

        return false;
    }

    // Async versions for scenarios
    public async IAsyncEnumerable<PersonRecord> ParseHorizontalCsvAsync(Stream stream, SchemaVersion schema, int bufferSize = DefaultBufferSize, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize);
        await foreach (var record in ParseHorizontalCsvAsync(reader, schema, cancellationToken))
        {
            yield return record;
        }
    }

    public async IAsyncEnumerable<PersonRecord> ParseHorizontalCsvAsync(TextReader reader, SchemaVersion schema, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var headerLine = await ParseCsvLineAsync(reader, cancellationToken);
        if (headerLine == null || headerLine.Count == 0) yield break;

        var headers = headerLine.ToArray();

        List<string>? line;
        while ((line = await ParseCsvLineAsync(reader, cancellationToken)) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (line.Count == 0) continue;

            var record = ParseRecord(headers, line.ToArray(), schema);
            if (record != null)
                yield return record;
        }
    }

    public async IAsyncEnumerable<PersonRecord> ParseVerticalCsvAsync(Stream stream, SchemaVersion schema, int bufferSize = DefaultBufferSize, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize);
        await foreach (var record in ParseVerticalCsvAsync(reader, schema, cancellationToken))
        {
            yield return record;
        }
    }

    public async IAsyncEnumerable<PersonRecord> ParseVerticalCsvAsync(TextReader reader, SchemaVersion schema, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var fieldNames = new List<string>();
        var recordData = new List<List<string>>();

        List<string>? line;
        while ((line = await ParseCsvLineAsync(reader, cancellationToken)) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (line.Count == 0) continue;

            fieldNames.Add(line[0]);
            var values = line.Skip(1).ToList();

            for (int i = 0; i < values.Count; i++)
            {
                if (recordData.Count <= i)
                    recordData.Add(new List<string>());
                recordData[i].Add(values[i]);
            }
        }

        for (int i = 0; i < recordData.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var record = ParseRecord(fieldNames.ToArray(), recordData[i].ToArray(), schema);
            if (record != null)
                yield return record;
        }
    }

    public async IAsyncEnumerable<PersonRecord> ParseHorizontalCsvFromFileAsync(string filePath, SchemaVersion schema, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var fileInfo = new FileInfo(filePath);

        if (fileInfo.Length > 100 * 1024 * 1024)
        {
            await foreach (var record in ParseHorizontalCsvFromLargeFileAsync(filePath, schema, cancellationToken))
            {
                yield return record;
            }
        }
        else
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, useAsync: true);
            await foreach (var record in ParseHorizontalCsvAsync(stream, schema, DefaultBufferSize, cancellationToken))
            {
                yield return record;
            }
        }
    }

    public async IAsyncEnumerable<PersonRecord> ParseVerticalCsvFromFileAsync(string filePath, SchemaVersion schema, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var fileInfo = new FileInfo(filePath);

        if (fileInfo.Length > 100 * 1024 * 1024)
        {
            await foreach (var record in ParseVerticalCsvFromLargeFileAsync(filePath, schema, cancellationToken))
            {
                yield return record;
            }
        }
        else
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, useAsync: true);
            await foreach (var record in ParseVerticalCsvAsync(stream, schema, DefaultBufferSize, cancellationToken))
            {
                yield return record;
            }
        }
    }

    private async IAsyncEnumerable<PersonRecord> ParseHorizontalCsvFromLargeFileAsync(string filePath, SchemaVersion schema, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, "csv", 0, MemoryMappedFileAccess.Read);
        using var stream = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
        await foreach (var record in ParseHorizontalCsvAsync(stream, schema, DefaultBufferSize, cancellationToken))
        {
            yield return record;
        }
    }

    private async IAsyncEnumerable<PersonRecord> ParseVerticalCsvFromLargeFileAsync(string filePath, SchemaVersion schema, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var mmf = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, "csv", 0, MemoryMappedFileAccess.Read);
        using var stream = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
        await foreach (var record in ParseVerticalCsvAsync(stream, schema, DefaultBufferSize, cancellationToken))
        {
            yield return record;
        }
    }

    private async Task<List<string>?> ParseCsvLineAsync(TextReader reader, CancellationToken cancellationToken = default)
    {
        var fields = new List<string>();
        var currentField = new StringBuilder();
        bool inQuotes = false;
        bool escapeNext = false;

        var buffer = new char[1];
        while (await reader.ReadAsync(buffer, 0, 1).ConfigureAwait(false) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            char c = buffer[0];
            int nextCharInt = reader.Peek();
            char? next = nextCharInt != -1 ? (char)nextCharInt : null;

            if (escapeNext)
            {
                currentField.Append(c);
                escapeNext = false;
                continue;
            }

            if (c == '\\' && inQuotes)
            {
                escapeNext = true;
                continue;
            }

            if (c == '"')
            {
                if (inQuotes && next == '"')
                {
                    currentField.Append('"');
                    await reader.ReadAsync(buffer, 0, 1).ConfigureAwait(false);
                    continue;
                }
                inQuotes = !inQuotes;
                continue;
            }

            if (!inQuotes)
            {
                if (c == ',')
                {
                    fields.Add(currentField.ToString().Trim());
                    currentField.Clear();
                    continue;
                }

                if (c == '\r' || c == '\n')
                {
                    if (c == '\r' && next == '\n')
                        await reader.ReadAsync(buffer, 0, 1).ConfigureAwait(false);

                    fields.Add(currentField.ToString().Trim());
                    return fields.Any(field => !string.IsNullOrWhiteSpace(field)) ? fields : new List<string>();
                }
            }

            currentField.Append(c);
        }

        if (currentField.Length > 0 || fields.Count > 0)
        {
            fields.Add(currentField.ToString().Trim());
            return fields.Any(field => !string.IsNullOrWhiteSpace(field)) ? fields : null;
        }

        return null;
    }

    // Legacy sync support for string-based parsing (backward compatibility)
    public async Task<List<PersonRecord>> ParseHorizontalCsvFromStringAsync(string csvContent, SchemaVersion schema, CancellationToken cancellationToken = default)
    {
        using var reader = new StringReader(csvContent);
        var records = new List<PersonRecord>();
        await foreach (var record in ParseHorizontalCsvAsync(reader, schema, cancellationToken))
        {
            records.Add(record);
        }
        return records;
    }

    public async Task<List<PersonRecord>> ParseVerticalCsvFromStringAsync(string csvContent, SchemaVersion schema, CancellationToken cancellationToken = default)
    {
        using var reader = new StringReader(csvContent);
        var records = new List<PersonRecord>();
        await foreach (var record in ParseVerticalCsvAsync(reader, schema, cancellationToken))
        {
            records.Add(record);
        }
        return records;
    }
}
