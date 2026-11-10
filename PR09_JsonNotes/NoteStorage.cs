using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace PR09_JsonNotes;

/// <summary>
/// Збереження та відновлення списку нотаток у JSON-файлі.
/// </summary>
public class NoteStorage
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        // щоб кирилиця у файлі була читабельною, а не З...
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public string FilePath { get; }

    public NoteStorage(string filePath)
    {
        FilePath = filePath;
    }

    public void Save(List<Note> notes)
    {
        string json = JsonSerializer.Serialize(notes, Options);
        File.WriteAllText(FilePath, json);
    }

    public List<Note> Load()
    {
        if (!File.Exists(FilePath))
            return new List<Note>();

        string json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<Note>>(json, Options) ?? new List<Note>();
    }
}
