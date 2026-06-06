using StreamerSongList.Datatypes;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace StreamerSongList;

public enum AuthenticationType
{
    mod,
    streamer
}

public class StreamerSongListClient
{
    private readonly HttpClient _http = new HttpClient();
    private readonly int _streamerId;
    private readonly AuthenticationType _authentication;

    /// <summary>
    /// Construct the StreamerSongListClient. As a number of operations require authorisation, 
    /// you must provide the authenticationType and accessToken.
    /// </summary>
    /// <param name="streamerId"></param>
    /// <param name="authenticationType"></param>
    /// <param name="accessToken"></param>
    public StreamerSongListClient(
        int streamerId,
        AuthenticationType authenticationType,
        string accessToken)
    {
        _streamerId     = streamerId;
        _authentication = authenticationType;

        // Add authorization header.
        // Note: Not all API calls require authorization - however for write settings, it is needed
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);            
        _http.DefaultRequestHeaders.Add("x-ssl-user-types", _authentication.ToString()); // streamer or mod
    }

    /// <summary>Search for StreamerID from supplied name. Returns -1 if not found</summary>
    public static async Task<int> GetStreamerIdFromName(
        string streamerName,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.streamersonglist.com/v1/streamers/{streamerName}?platform=twitch&isUsername=true";

        HttpClient httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url, cancellationToken);
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

    /// <summary>Return all songs from SSL</summary>
    /// <returns>List of songs</returns>
    /// <remarks>showInactive requires mod/streamer authentication</remarks>
    public async Task<List<Song>> GetAllSongs(
        bool showInactive = true,
        CancellationToken cancellationToken = default)
    {
        var allSongs = new List<Song>();

        int size = 100; // Request 100 songs at a time (max allowed by API)
        int offset = 0;

        // While more songs remain, keep calling API and adding to results
        do
        {
            var url = $"https://api.streamersonglist.com/v1/streamers/{_streamerId}/songs?size={size}&showInactive={(showInactive ? "true" : "false")}&order=asc&current={offset}";

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
                offset++;
                
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
        string searchText,
        bool showInactive = true,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.streamersonglist.com/v1/streamers/{_streamerId}/songs?showInactive={(showInactive ? "true" : "false")}&order=asc&filterText={Uri.EscapeDataString(searchText)}";
        
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
    public async Task<Dictionary<int, string>> GetAllAttributes(
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.streamersonglist.com/v1/streamers/{_streamerId}/songAttributes";
        var response = await _http.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var results = JsonSerializer.Deserialize<List<SongAttribute>>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        // convert to dictionary keyed by attribute ID
        var attributes = new Dictionary<int, string>();

        if (results != null)
        {
            foreach (var result in results)
                attributes[result.Id] = result.Name;
        }

        return attributes;
    }

    /// <summary>
    /// Add attribute to specified song
    /// </summary>
    public async Task AddAttribute(
        Song song,
        int attributeId,
        CancellationToken cancellationToken = default)
    {
        // Does the Song already have the attribute set? If so - return
        if (song.AttributeIds.Contains(attributeId))
            return;
        
        // Otherwise add to song
        var url = $"https://api.streamersonglist.com/v1/streamers/{_streamerId}/songs/{song.Id}";

        // When ADDING attributes, must go to the Attributes property (NOT AttributeIds)
        song.Attributes = [.. song.AttributeIds, attributeId]; // Take a copy of existing song.AttributeIds and add

        var payload = JsonSerializer.Serialize(song, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var content = new StringContent(
            payload,
            Encoding.UTF8,
            "application/json");

        var response =
            await _http.PutAsync(
                url,
                content,
                cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Add attribute to specified song
    /// </summary>
    public async Task SetPrice(
        Song song,
        float price,
        CancellationToken cancellationToken = default)
    {
        // Does the Song already have the correct price set? If so - return
        if (song.MinAmount == price)
            return;

        // Otherwise add to song
        var url = $"https://api.streamersonglist.com/v1/streamers/{_streamerId}/songs/{song.Id}";

        // When doing a SET of a song, attributes must be provided in Attributes, but they are GET from AttributeIds
        // otherwise the attributes are wiped clean
        song.Attributes = song.AttributeIds; // Take a copy of existing song.AttributeIds
        song.MinAmount = price;

        var payload = JsonSerializer.Serialize(song, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var content = new StringContent(
            payload,
            Encoding.UTF8,
            "application/json");

        var response =
            await _http.PutAsync(
                url,
                content,
                cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}