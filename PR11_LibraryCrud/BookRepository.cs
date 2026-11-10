using Microsoft.Data.Sqlite;

namespace PR11_LibraryCrud;

/// <summary>
/// Шар доступу до даних: усі SQL-запити CRUD для таблиці Books.
/// </summary>
public class BookRepository
{
    private readonly string _connectionString;

    public BookRepository(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Books (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Title       TEXT    NOT NULL,
                Author      TEXT    NOT NULL,
                Year        INTEGER NOT NULL,
                Genre       TEXT    NOT NULL DEFAULT '',
                IsAvailable INTEGER NOT NULL DEFAULT 1
            );
            """;
        command.ExecuteNonQuery();
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static void BindBook(SqliteCommand command, Book book)
    {
        command.Parameters.AddWithValue("$title", book.Title);
        command.Parameters.AddWithValue("$author", book.Author);
        command.Parameters.AddWithValue("$year", book.Year);
        command.Parameters.AddWithValue("$genre", book.Genre);
        command.Parameters.AddWithValue("$available", book.IsAvailable ? 1 : 0);
    }

    // C — INSERT
    public int Create(Book book)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Books (Title, Author, Year, Genre, IsAvailable)
            VALUES ($title, $author, $year, $genre, $available);
            SELECT last_insert_rowid();
            """;
        BindBook(command, book);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    // R — SELECT (з необов'язковим пошуком за назвою/автором)
    public List<Book> Read(string? search = null)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Title, Author, Year, Genre, IsAvailable
            FROM Books
            WHERE $search IS NULL OR Title LIKE $pattern OR Author LIKE $pattern
            ORDER BY Title;
            """;
        bool hasSearch = !string.IsNullOrWhiteSpace(search);
        command.Parameters.AddWithValue("$search", hasSearch ? search! : DBNull.Value);
        command.Parameters.AddWithValue("$pattern", hasSearch ? $"%{search!.Trim()}%" : DBNull.Value);

        var books = new List<Book>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            books.Add(new Book
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Author = reader.GetString(2),
                Year = reader.GetInt32(3),
                Genre = reader.GetString(4),
                IsAvailable = reader.GetInt32(5) == 1
            });
        }
        return books;
    }

    // U — UPDATE
    public bool Update(Book book)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Books
            SET Title = $title, Author = $author, Year = $year, Genre = $genre, IsAvailable = $available
            WHERE Id = $id;
            """;
        BindBook(command, book);
        command.Parameters.AddWithValue("$id", book.Id);
        return command.ExecuteNonQuery() == 1;
    }

    // D — DELETE
    public bool Delete(int id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Books WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        return command.ExecuteNonQuery() == 1;
    }
}
