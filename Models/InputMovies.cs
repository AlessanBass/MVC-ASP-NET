namespace MvcMovie.Models;
public class InputMovies
{
    public string? Title { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public decimal Price { get; set; }
    public int StudioId { get; set; }
    public ICollection<string>? Artists { get; set; }
}
