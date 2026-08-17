using System.Text.Json.Serialization;

namespace StreamerSongList.Datatypes;

public class SearchResult
{
    [JsonIgnore]
    public bool Exists => Items.Count > 0;

    public List<Song> Items { get; set; } = new();

    public string Token { get; set; } = ""; // Cursor for next page of results. If null, no more results

    public int Total { get; set; }
}