namespace EfCorePostgres.ApiService.Models;

public class YouTubeLink
{
    public int Id { get; set; }
    public required string Key { get; set; }
    public required string Url { get; set; }
}
