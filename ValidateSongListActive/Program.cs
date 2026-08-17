using CommandLine;
using StreamerSongList;
using StreamerSongList.Datatypes;
using System.Diagnostics;

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
        Console.WriteLine($"role         : {opts.Role}");
        Console.WriteLine();

        int streamId = -1;
        if (!int.TryParse(opts.StreamId, out streamId)) // not a number, must be a name
        {
            streamId = await StreamerSongListClient.GetStreamerIdFromName(opts.StreamId);
            Console.WriteLine($"Obtained SSL stream ID of {streamId} from {opts.StreamId}");
        }

        if (streamId < 0)
        {
            Console.WriteLine($"Invalid Stream ID: {opts.StreamId}");
            return 1;
        }

        Console.WriteLine();

        //=============================================================================
        // Constants
        //=============================================================================

        // Song list filter
        string[] removeText = { "NEW 💎" };

        //=============================================================================
        // Parse and clean song list from file
        //=============================================================================
        var lines = File.ReadLines(opts.InputFile);

        List<string> validSongs = new List<string>();
        foreach (var line in lines)
        {
            var cleanLine = line;
            // Remove any annoying text
            foreach (var text in removeText)
                cleanLine = line.Replace(text, "");

            cleanLine = cleanLine.Trim();

            if (string.IsNullOrEmpty(cleanLine))
                continue;

            validSongs.Add(cleanLine);
        }

        Console.WriteLine($"Loaded {validSongs.Count} song titles");

        //=============================================================================
        // Find matching songs in SSL and add the attribute to it
        //=============================================================================
        var ssl = new StreamerSongListClient(streamId,
                                             opts.Role,
                                             opts.AccessToken);

        // Get list of all available songs in SSL (both active and inactive)
        var allSongs = await ssl.GetAllSongs(false);
        var allSongsStrings = allSongs.Select(s => $"{s.Artist} | {s.Title}").ToList();

        Console.WriteLine($"Streamer has {allSongs.Count} songs in total");

        

        // For each song in the supplied song list:
        //  - Determine the best match against the entire SSL list
        //  - compare against the SSL search query result
        Console.WriteLine();

        // There is an API rate limit of about 200 per minute (which I hit...). So do at most 1 song every second
        // which requires 2x API calls (1x get song, 1x write attribute).
        // In future might be able to batch update??
        Stopwatch sw = Stopwatch.StartNew();
        int apiCalls = 0;
        foreach (var song in validSongs)
        {
            Console.WriteLine($"searching for {song}...");
            // using Levenshtein distance
            var bestMatch = allSongs[StringSearch.GetBestMatch(song, allSongsStrings)];

            // use SSL
            var sslMatchResults = await ssl.SearchSong(song);
            var sslMatch = sslMatchResults.Items.First();
            if (sslMatch == null)
            {
                Console.WriteLine($"No SSL match found for {song}");
                continue;
            }
            apiCalls++;

            // Did we pick the same song? If not, report on the difference
            if (bestMatch.Id == sslMatch.Id)
                Console.WriteLine($"YES! Best match for {song} is {bestMatch.Artist} | {bestMatch.Title}");
            else
                Console.WriteLine($"NO! Best match for {song} is {bestMatch.Artist} | {bestMatch.Title} but SSL picked {sslMatch.Artist} | {sslMatch.Title}");

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