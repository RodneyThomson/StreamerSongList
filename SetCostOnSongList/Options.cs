using CommandLine;
using StreamerSongList;

internal class Options
{
    [Option('i', "input", Required = true, HelpText = "Input file containing rows of Artist | Title")]
    public string InputFile { get; set; } = null!;

    [Option('s', "stream_id", Required = true, HelpText = "Either streamer name, or SSL stream ID (number)")]
    public string StreamId { get; set; } = null!;

    [Option('t', "access_token", Required = true, HelpText = "SSL User Access Token. Get from your Profile Settings -> API Access")]
    public string AccessToken { get; set; } = null!;

    [Option('p', "price", Required = true, HelpText = "Desired price to apply to all songs in the song list")]
    public int Price { get; set; } = 0;

    [Option('w', "write", Required = false, HelpText = "Without -w, the attribute will NOT be written. Running without -w can be used to check for missing attributes (+ next to their name in the list)")]
    public bool Write { get; set; } = false;
}
