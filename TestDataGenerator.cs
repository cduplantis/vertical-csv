using System.Text;

namespace VerticalCsv;

public static class TestDataGenerator
{
    public static void GenerateLargeVerticalCsv(string filePath, int recordCount = 10000)
    {
        var departments = new[] { "Engineering", "Marketing", "Sales", "HR", "Finance", "Operations" };
        var skills = new[] { "C#", "Python", "Java", "JavaScript", "SQL", "Docker", "Kubernetes", "React", "Angular", "Node.js" };
        var languages = new[] { "English", "Spanish", "French", "German", "Chinese", "Japanese" };
        var cities = new[] { "New York", "Los Angeles", "Chicago", "Houston", "Phoenix", "Philadelphia" };
        var states = new[] { "NY", "CA", "IL", "TX", "AZ", "PA" };
        
        var random = new Random(42);
        
        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        
        writer.WriteLine($"Name,{string.Join(",", Enumerable.Range(0, recordCount).Select(i => $"Person_{i:D6}"))}");
        writer.WriteLine($"Age,{string.Join(",", Enumerable.Range(0, recordCount).Select(i => random.Next(22, 65)))}");
        writer.WriteLine($"Email,{string.Join(",", Enumerable.Range(0, recordCount).Select(i => $"person_{i:D6}@company.com"))}");
        writer.WriteLine($"Phone,{string.Join(",", Enumerable.Range(0, recordCount).Select(i => $"555-{random.Next(1000, 9999)}"))}");
        writer.WriteLine($"Department,{string.Join(",", Enumerable.Range(0, recordCount).Select(i => departments[random.Next(departments.Length)]))}");
        writer.WriteLine($"StartDate,{string.Join(",", Enumerable.Range(0, recordCount).Select(i => DateTime.Now.AddDays(-random.Next(365 * 5)).ToString("yyyy-MM-dd")))}");
        
        for (int skillIndex = 0; skillIndex < 5; skillIndex++)
        {
            writer.WriteLine($"Skills[{skillIndex}],{string.Join(",", Enumerable.Range(0, recordCount).Select(i => random.Next(100) < 70 ? skills[random.Next(skills.Length)] : ""))}");
        }
        
        for (int langIndex = 0; langIndex < 3; langIndex++)
        {
            writer.WriteLine($"Languages[{langIndex}],{string.Join(",", Enumerable.Range(0, recordCount).Select(i => random.Next(100) < 80 ? languages[random.Next(languages.Length)] : ""))}");
        }
        
        writer.WriteLine($"Address.Street,{string.Join(",", Enumerable.Range(0, recordCount).Select(i => $"{random.Next(1, 9999)} Main St"))}");
        writer.WriteLine($"Address.City,{string.Join(",", Enumerable.Range(0, recordCount).Select(i => cities[random.Next(cities.Length)]))}");
        writer.WriteLine($"Address.State,{string.Join(",", Enumerable.Range(0, recordCount).Select(i => states[random.Next(states.Length)]))}");
        writer.WriteLine($"Address.ZipCode,{string.Join(",", Enumerable.Range(0, recordCount).Select(i => random.Next(10000, 99999)))}");
    }
}