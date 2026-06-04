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
        Console.WriteLine($"price        : {opts.Price}");
        Console.WriteLine($"write        : {opts.Write}");
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

        if (!opts.Write)
            Console.WriteLine("==== WRITE not enabled. Checking songs only ====");

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
        // Find matching songs in SSL and set the price on it
        //=============================================================================
        var ssl = new StreamerSongListClient(streamId,
                                             opts.Role,
                                             opts.AccessToken);

        const int TITLE_WIDTH = 60;
        Console.WriteLine();
        Console.WriteLine($"{new string('-', 2 * TITLE_WIDTH + 8)}");
        Console.WriteLine($"| {"Song List Title",-TITLE_WIDTH} <-> {"SSL Title",-TITLE_WIDTH}|");
        Console.WriteLine($"{new string('-', 2 * TITLE_WIDTH + 8)}");

        // For each song in the song list, search for a unique match

        // There is an API rate limit of about 200 per minute (which I hit...). So do at most 1 song every second
        // which requires 2x API calls (1x get song, 1x write attribute).
        // In future might be able to batch update??
        Stopwatch sw = Stopwatch.StartNew();
        int apiCalls = 0;
        foreach (var searchSong in validSongs)
        {
            var matchSongs = await ssl.SearchSong(searchSong);
            apiCalls++;

            Song matchedSong = new Song();
            if (matchSongs.Items.Count == 0) // No match! Can't do much about that
            {
                Console.WriteLine($"| {searchSong.Substring(0, Math.Min(searchSong.Length, TITLE_WIDTH)),-TITLE_WIDTH} <-> {"NOT FOUND",TITLE_WIDTH}|");
                continue;
            }

            // SSL Search always seems to return all matching songs from a particular artist.
            // Options:
            //   1) Pick the 1st entry (typically right)
            //   2) Get ALL songs from the streamer and do the song title matching myself (maybe later?)
            //
            // For now I'm doing option 1)
            matchedSong = matchSongs.Items[0];
            var matchedSongTitle = matchedSong.ToString();

            // Check if price already set on song
            bool priceSet = (matchedSong.MinAmount == opts.Price);

            // Print the song list title next to the matched SSL song title
            Console.WriteLine($"|{(priceSet ? " " : "+")}{searchSong.Substring(0, Math.Min(searchSong.Length, TITLE_WIDTH)),-TITLE_WIDTH} <-> {matchedSongTitle.Substring(0, Math.Min(matchedSongTitle.Length, TITLE_WIDTH)),-TITLE_WIDTH}|");

            // Set the price if required, and if the write flag is set
            if (!priceSet && opts.Write)
            {
                await ssl.SetPrice(matchedSong, opts.Price);
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