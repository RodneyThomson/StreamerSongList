using System.Text.Json.Serialization;

namespace StreamerSongList.Datatypes;

/// <summary>
///   https://dev.streamersonglist.com/api-reference?method=get&path=%2Fsongs%2Fall
/// </summary>
public class SongsResponseBody
{
    [JsonIgnore]
    public bool Exists => Items.Count > 0;

    public List<Song> Items { get; set; } = new();

    public string Token { get; set; } = ""; // Cursor for next page of results. If null, no more results

    public int Total { get; set; }
}