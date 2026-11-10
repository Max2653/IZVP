using System.Globalization;
using Microsoft.Data.Sqlite;

namespace PR10_SqliteProducts;

/// <summary>
/// CRUD-операції з таблицею Products у локальній базі SQLite.
/// </summary>
public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
        EnsureCreated();
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    /// <summary>Створює таблицю, якщо її ще немає.</summary>
    private void EnsureCreated()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Products (
                Id       INTEGER PRIMARY KEY AUTOINCREMENT,
                Name     TEXT    NOT NULL,
                Price    REAL    NOT NULL,
                Quantity INTEGER NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    // Create
    public void Add(Product product)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Products (Name, Price, Quantity) VALUES ($name, $price, $qty);";
        command.Parameters.AddWithValue("$name", product.Name);
        command.Parameters.AddWithValue("$price", (double)product.Price);
        command.Parameters.AddWithValue("$qty", product.Quantity);
        command.ExecuteNonQuery();
    }

    // Read
    public List<Product> GetAll()
    {
        var result = new List<Product>();
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Price, Quantity FROM Products ORDER BY Id;";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = Convert.ToDecimal(reader.GetDouble(2), CultureInfo.InvariantCulture),
                Quantity = reader.GetInt32(3)
            });
        }
        return result;
    }

    // Update
    public void Update(Product product)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Products SET Name = $name, Price = $price, Quantity = $qty WHERE Id = $id;";
        command.Parameters.AddWithValue("$name", product.Name);
        command.Parameters.AddWithValue("$price", (double)product.Price);
        command.Parameters.AddWithValue("$qty", product.Quantity);
        command.Parameters.AddWithValue("$id", product.Id);
        command.ExecuteNonQuery();
    }

    // Delete
    public void Delete(int id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Products WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }
}
