namespace TodoApi.Models;

public class Todo
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Blockchain properties for Challenge B
    public string? Hash { get; set; }
    public string? PreviousHash { get; set; }
}
