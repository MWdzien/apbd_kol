namespace apbd_kolokwium1.Models.DTOs;

public class BookDTO
{
    public int id { get; set; }
    public string title { get; set; }
    public List<Genres> genres { get; set; }
}