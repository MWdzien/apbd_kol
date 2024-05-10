using apbd_kolokwium1.Models;
using apbd_kolokwium1.Models.DTOs;
using apbd_kolokwium1.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace apbd_kolokwium1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBooksRepository _booksRepository;

    public BooksController(IBooksRepository booksRepository)
    {
        _booksRepository = booksRepository;
    }

    [HttpGet("{id}/genres")]
    public async Task<IActionResult> GetBookWithGenres(int id)
    {
        if (!await _booksRepository.DoesBookExist(id))
        {
            return NotFound("Book not found");
        }

        BookDTO book = await _booksRepository.GetBooksWithGenres(id);

        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> AddBook(NewBookWithGenresDTO NewBookWithGenres)
    {
        if (!await _booksRepository.DoesBookExist(NewBookWithGenres.Title))
        {
            return NotFound("Book not found");
        }

        foreach (var GenreId in NewBookWithGenres.Genres)
        {
            if (!await _booksRepository.DoesBookExist(NewBookWithGenres.Title))
            {
                return NotFound("Genre not found");
            }
        }


        int id = await _booksRepository.AddBookWithGenres(NewBookWithGenres);

        var book = await _booksRepository.GetBooksWithGenres(id);

        return Created(Request.Path.Value ?? "api/books", book);
    }
}