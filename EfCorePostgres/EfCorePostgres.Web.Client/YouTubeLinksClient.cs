using System.Net.Http.Json;

namespace EfCorePostgres.Web.Client;

public class YouTubeLinksClient(HttpClient http)
{
    public Task<YouTubeLinkModel[]?> GetAllAsync() =>
        http.GetFromJsonAsync<YouTubeLinkModel[]>("/api/youtube-links");

    public Task<HttpResponseMessage> CreateAsync(string key, string url) =>
        http.PostAsJsonAsync("/api/youtube-links", new { Key = key, Url = url });

    public Task<HttpResponseMessage> DeleteAsync(string key) =>
        http.DeleteAsync($"/api/youtube-links/{Uri.EscapeDataString(key)}");
}

public record YouTubeLinkModel(int Id, string Key, string Url);
