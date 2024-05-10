namespace apbd_kolokwium1.Models.DTOs;

public class NewBookWithGenresDTO
{
    public string Title { get; set; } = string.Empty;
    public IEnumerable<int> Genres { get; set; } = new List<int>();
}