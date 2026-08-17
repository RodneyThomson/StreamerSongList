using StreamerSongList.Datatypes;
using System;
using System.Drawing;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StreamerSongList;

public class StreamerSongListClient
{
    private readonly HttpClient _http = new HttpClient();
    
    /// <summary>
    /// Construct the StreamerSongListClient. As a number of operations require authorisation, 
    /// you must provide the authenticationType and accessToken.
    /// </summary>    
    /// <param name="accessToken"></param>
    public StreamerSongListClient(string accessToken)
    {
        // Add authorization header.
        // Note: Not all API calls require authorization - however for write settings, it is needed
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("User", accessToken);
    }

    /// <summary>Search for StreamerID from supplied name. Returns -1 if not found</summary>
    public async Task<int> GetStreamerIdFromName(
        string streamerName,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.streamersonglist.com/streamers?platform=twitch&streamer_name={streamerName}";

        var response = await _http.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return -1;
        
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = JsonSerializer.Deserialize<StreamerInfo>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result == null)
            return -1;
        else
            return result.Id;
    }

    /// <summary>Return all songs from SSL for specified streamer ID</summary>
    /// <returns>List of songs</returns>
    /// <remarks>showInactive will show both active and inactive songs. Requires mod/streamer authentication</remarks>
    public async Task<List<Song>> GetAllSongs(
        int streamerId,
        bool showInactive = true,
        CancellationToken cancellationToken = default)
    {
        var allSongs = new List<Song>();

        int size = 100; // Request 100 songs at a time (max allowed by API)
        string cursor = "";

        // While more songs remain, keep calling API and adding to results
        do
        {
            var url = showInactive ? $"https://api.streamersonglist.com/songs/all?streamer_id={streamerId}&limit={size}&after={cursor}" :
                                     $"https://api.streamersonglist.com/songs/all?streamer_id={streamerId}&limit={size}&after={cursor}&active=true";

            var response = await _http.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<SearchResult>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result != null && result.Items != null)
            {
                allSongs.AddRange(result.Items);
                cursor = result.Token;
                
                if (allSongs.Count == result.Total)
                    break; // All songs have been retrieved
            }
            else
                break; // Unknown number songs to retrieve
        } 
        while (true); 

        return allSongs;
    }

    /// <summary>Search for a song by title or artist. Throws exception if search cannot be completed</summary>
    /// <returns> SearchResult which contains a list of 0 or more Songs found</returns>
    /// <remarks>showInactive requires mod/streamer authentication</remarks>
    public async Task<SearchResult> SearchSong(
        int streamerId,
        string searchText,
        bool showInactive = true,
        CancellationToken cancellationToken = default)
    {
        var url = showInactive ? $"https://api.streamersonglist.com/songs/all?streamer_id={streamerId}&search={Uri.EscapeDataString(searchText)}" :
                                 $"https://api.streamersonglist.com/songs/all?streamer_id={streamerId}&search={Uri.EscapeDataString(searchText)}&active=true";

        var response = await _http.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = JsonSerializer.Deserialize<SearchResult>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return result ?? new SearchResult();
    }

    /// <summary>
    /// Returns all attributes for a channel. Requires authorisation.
    /// Attributes are a Dictionary keyed on attribute ID
    /// </summary>
    public async Task<Dictionary<int, string>> GetAllAttributes(int streamerId,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.streamersonglist.com/attributes?streamer_id={streamerId}&include_hidden=true";
        var response = await _http.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var results = JsonSerializer.Deserialize<SongAttributeList>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        // convert to dictionary keyed by attribute ID
        var attributes = new Dictionary<int, string>();

        if (results != null)
        {
            foreach (var result in results.Items)
                attributes[result.Id] = result.Name;
        }

        return attributes;
    }

    /// <summary>
    /// Add attribute to specified song
    /// </summary>
    public async Task AddAttribute(
        int streamerId,
        Song song,
        int attributeId,
        CancellationToken cancellationToken = default)
    {
        // Does the Song already have the attribute set? If so - return
        if (song.Attributes.Any(a => a.Id == attributeId))
            return;
        
        // Otherwise add to song
        var url = $"https://api.streamersonglist.com/songs/{song.Id}?song_id={song.Id}";

        var songUpdate = new UpdateSongBody() { AttributeIds = [.. song.Attributes.Select(x => x.Id).ToArray(), attributeId] };
    
        var payload = JsonSerializer.Serialize(songUpdate, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var content = new StringContent(
            payload,
            Encoding.UTF8,
            "application/json");

        var response =
            await _http.PatchAsync(
                url,
                content,
                cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Add attribute to specified song
    /// </summary>
    public async Task SetPrice(
        int streamerId,
        Song song,
        int price,
        CancellationToken cancellationToken = default)
    {
        // Does the Song already have the correct price set? If so - return
        if (song.MinTokens == price)
            return;

        // Otherwise add to song
        var url = $"https://api.streamersonglist.com/songs/{song.Id}";

        // When doing a PATCH of a song, only need to supply changed attributes
        // The /songs/{song_id} PATCH takes a UpdateSongBody parameter: https://dev.streamersonglist.com/api-reference?method=patch&path=%2Fsongs%2F%7Bsong_id%7D
        var updateSongBody = new UpdateSongBody() { MinAmount = price }; // Should be MinTokens, but the API uses MinAmount for some reason
        
        var payload = JsonSerializer.Serialize(updateSongBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var content = new StringContent(
            payload,
            Encoding.UTF8,
            "application/json");

        var response =
            await _http.PatchAsync(
                url,
                content,
                cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}