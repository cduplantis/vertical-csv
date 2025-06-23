namespace VerticalCsv.Models;

public class PersonRecord
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public DateTime? StartDate { get; set; }
    
    public List<string> Skills { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public Address? Address { get; set; }
    public List<Project> Projects { get; set; } = new();
    public string? Notes { get; set; }
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

public class Project
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class SchemaVersion
{
    public int Version { get; set; }
    public List<string> RequiredFields { get; set; } = new();
    public List<string> OptionalFields { get; set; } = new();
    public List<string> OptionalFieldPatterns { get; set; } = new();
    
    public static SchemaVersion V1 => new()
    {
        Version = 1,
        RequiredFields = new() { "Name", "Age", "Email" },
        OptionalFields = new()
    };
    
    public static SchemaVersion V2 => new()
    {
        Version = 2,
        RequiredFields = new() { "Name", "Age", "Email" },
        OptionalFields = new() { "Phone" }
    };
    
    public static SchemaVersion V3 => new()
    {
        Version = 3,
        RequiredFields = new() { "Name", "Age", "Email" },
        OptionalFields = new() { "Phone", "Department", "StartDate" }
    };
    
    public static SchemaVersion V4 => new()
    {
        Version = 4,
        RequiredFields = new() { "Name", "Age", "Email" },
        OptionalFields = new() { 
            "Phone", "Department", "StartDate", "Notes",
            "Address.Street", "Address.City", "Address.State", "Address.ZipCode"
        },
        OptionalFieldPatterns = new()
        {
            @"^Skills\[\d+\]$",
            @"^Languages\[\d+\]$", 
            @"^Projects\[\d+\]\.Name$",
            @"^Projects\[\d+\]\.Role$",
            @"^Projects\[\d+\]\.StartDate$",
            @"^Projects\[\d+\]\.EndDate$"
        }
    };
}