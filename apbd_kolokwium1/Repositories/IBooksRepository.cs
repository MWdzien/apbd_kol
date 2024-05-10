using apbd_kolokwium1.Models;
using apbd_kolokwium1.Models.DTOs;

namespace apbd_kolokwium1.Repositories;

public interface IBooksRepository
{
    Task<bool> DoesBookExist(int id);
    Task<bool> DoesGenreExist(int id);
    Task<BookDTO> GetBooksWithGenres(int id);
    Task<int> AddBookWithGenres(NewBookWithGenresDTO newBookWithGenres);
    Task<bool> DoesBookExist(string title);
}