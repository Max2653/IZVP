namespace PR09_JsonNotes;

/// <summary>
/// Простий клас, об'єкти якого серіалізуються у JSON.
/// </summary>
public class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public override string ToString() => $"#{Id} {Title} ({CreatedAt:dd.MM.yyyy HH:mm})";
}
