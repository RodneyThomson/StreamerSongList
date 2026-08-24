using CommandLine;
using CsvHelper;
using StreamerSongList;
using System.Diagnostics;
using System.Globalization;

class SongExport
{
    // id,title,artist,active,comment,tabs,lyrics,chords,capo,bypassRequestLimits,minAmount,attributes
    public int id { get; set; }
    public string title { get; set; } = string.Empty;
    public string artist { get; set; } = string.Empty;
    public bool active { get; set; }
    public string comment { get; set; } = string.Empty;
    public string tabs { get; set; } = string.Empty;
    public string lyrics { get; set; } = string.Empty;
    public string chords { get; set; } = string.Empty;
    public string capo { get; set; } = string.Empty;
    public bool bypassRequestLimits { get; set; }
    public int minAmount { get; set; }
    public string attributes { get; set; } = string.Empty;
}

class Program
{
    static async Task<int> Main(string[] args)
    {
        var results = Parser.Default.ParseArguments<Options>(args);

        if (results.Errors.Any())
            return 1;

        var opts = results.Value;

        //=============================================================================
        // Arguments
        //=============================================================================
        Console.WriteLine($"Input parameters:");
        Console.WriteLine($"input        : {opts.InputFile}");
        Console.WriteLine($"stream_id    : {opts.StreamId}");
        Console.WriteLine($"write        : {opts.Write}");
        Console.WriteLine();

        var ssl = new StreamerSongListClient(opts.AccessToken);

        int streamId = -1;
        if (!int.TryParse(opts.StreamId, out streamId)) // not a number, must be a name
        {
            streamId = await ssl.GetStreamerIdFromName(opts.StreamId);
            Console.WriteLine($"Obtained SSL stream ID of {streamId} from {opts.StreamId}");
        }

        if (streamId < 0)
        {
            Console.WriteLine($"Invalid Stream ID: {opts.StreamId}");
            return 1;
        }

        Console.WriteLine();

        // Get list of all available attributes ([id][name])
        var attributes = await ssl.GetAllAttributes(streamId);

        Console.WriteLine($"Stream {streamId} has {attributes.Count} attributes:");
        foreach (var attribute in attributes)
        {
            Console.WriteLine($"[{attribute.Key}] = {attribute.Value}");
        }

        //=============================================================================
        // Parse and clean song list from file
        //=============================================================================

        List<SongExport> records = new List<SongExport>();

        // Open the file stream
        using (var reader = new StreamReader(opts.InputFile))

        // Initialize the CsvReader with standard culture formatting
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            // Automatically maps headers to matching object properties
            records = csv.GetRecords<SongExport>().ToList();
        }

        if (!records.Any())
        {
            Console.WriteLine("No songs found in export");
            return 1;
        }

        Console.WriteLine($"Loaded {records.Count} song titles");

        // For each song in the export:
        // - Search for the matching song in SSL (Not sure if I trust that the IDs are the same... will check)
        // - If found, then parse and check the attributes against the loaded set from the stream ID
        int apiCalls = 0;
        Stopwatch sw = Stopwatch.StartNew();

        foreach (var song in records)
        {
            // Search for the song in SSL by title and artist

            var matchSongs = await ssl.SearchSong(streamId, $"{song.artist} - {song.title}");
            apiCalls++;

            if (matchSongs.Items.Count == 0) // No match! Can't do much about that
            {
                Console.WriteLine($"Song not found in SSL: {song.artist} - {song.title}");
                continue;
            }

            var matchedSong = matchSongs?.Items[0];

            // Ok, found a matching song, set the attributes on it
            // First - get the attribute IDs from the list of attributes in the export
            var attributeIds = new List<int>();
            var attributeNames = song.attributes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var attribute in attributeNames)
            {
                var matchingAttribute = attributes.FirstOrDefault(a => a.Value.Equals(attribute, StringComparison.OrdinalIgnoreCase));
                if (matchingAttribute.Key != 0) // Found a match
                {
                    attributeIds.Add(matchingAttribute.Key);
                }
                else
                {
                    Console.WriteLine($"WARNING: Attribute '{attribute}' not found in SSL for stream {opts.StreamId}");
                }
            }

            if (song.id != matchedSong?.Id)
            {
                Console.WriteLine($"WARNING: Song ID mismatch for '{song.artist} - {song.title}': CSV ID {song.id} vs SSL ID {matchedSong?.Id}");
            }

            Console.WriteLine($"Setting attributes for song '[CSV: {song.artist} - {song.title}' | SSL: {matchedSong.Artist} - {matchedSong.Title}] to [{string.Join(", ", attributeIds)}]");

            if (opts.Write)
            { 
                await ssl.SetAttributes(streamId, matchedSong, attributeIds);
                apiCalls++;
            }

            // Make sure we don't hit rate limit - 0.5s per call or 120 per minute
            var earliestTime = apiCalls * TimeSpan.FromSeconds(0.5);
            var requiredSleep = earliestTime - sw.Elapsed;
            if (requiredSleep.TotalSeconds > 0)
                Thread.Sleep((int)requiredSleep.TotalMilliseconds);

        }

        Console.WriteLine("Processing complete.");
        return 0;
    }
}

