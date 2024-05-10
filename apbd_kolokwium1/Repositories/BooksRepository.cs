using System.Data.SqlClient;
using apbd_kolokwium1.Models;
using apbd_kolokwium1.Models.DTOs;

namespace apbd_kolokwium1.Repositories;

public class BooksRepository : IBooksRepository
{
    private readonly IConfiguration _configuration;

    public BooksRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> DoesBookExist(int id)
    {
        var query = "SELECT 1 FROM books WHERE PK = @ID";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }
    
    public async Task<bool> DoesBookExist(string title)
    {
        var query = "SELECT 1 FROM books WHERE title = @Title";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@Title", title);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> DoesGenreExist(int id)
    {
        var query = "SELECT 1 FROM genres WHERE PK = @ID";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<BookDTO> GetBooksWithGenres(int id)
    {
        var query = @"SELECT 
                    books.PK AS BookId
                    books.title AS Title
                    genres.name AS GenresName
                    FROM books
                    JOIN books_genres ON books_genres.FK_book = books.PK
                    JOIN genres ON genres.PK = books_genres.FK_genre
                    WHERE books.PK = @ID";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();

        var bookIdOrdinal = reader.GetOrdinal("BookId");
        var bookTitleOrdinal = reader.GetOrdinal("Title");
        var genreNameOrdinal = reader.GetOrdinal("GenreName");

        BookDTO booksDto = null;

        while (await reader.ReadAsync())
        {
            if (booksDto is not null)
            {
                booksDto.genres.Add(new Genres()
                {
                    Name = reader.GetString(genreNameOrdinal)
                });
            }
            else
            {
                booksDto = new BookDTO()
                {
                    id = reader.GetInt32(bookIdOrdinal),
                    title = reader.GetString(bookTitleOrdinal),
                    genres = new List<Genres>()
                    {
                        new Genres()
                        {
                            Name = reader.GetString(genreNameOrdinal)
                        }
                    }
                };
            }
        }

        if (booksDto is null) throw new Exception();

        return booksDto;
    }

    public async Task<int> AddBookWithGenres(NewBookWithGenresDTO newBookWithGenres)
    {
        var query = @"INSERT INTO books VALUES(@BookTitle)" +
                    "SELECT @@IDENTITY AS ID";
        
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@BookTitle", newBookWithGenres.Title);
        
        await connection.OpenAsync();

        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        
        try
        {
            var id = await command.ExecuteScalarAsync();
    
            foreach (var genreId in newBookWithGenres.Genres)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO books_genres VALUES(@FK_book, @Fk_genre);";
                command.Parameters.AddWithValue("@FK_book", id);
                command.Parameters.AddWithValue("@Fk_genre", genreId);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
		    
            return Convert.ToInt32(id);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}