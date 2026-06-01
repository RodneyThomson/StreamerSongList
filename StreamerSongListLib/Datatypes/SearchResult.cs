using System.Text.Json.Serialization;

namespace StreamerSongList.Datatypes;

public class SearchResult
{
    [JsonIgnore]
    public bool Exists => Items.Count > 0;

    public List<Song> Items { get; set; } = new();
}