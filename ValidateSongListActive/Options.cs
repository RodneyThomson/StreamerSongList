using CommandLine;
using StreamerSongList;
using System.Runtime.InteropServices;

internal class Options
{
    [Option('i', "input", Required = true, HelpText = "Input file containing rows of Artist | Title")]
    public string InputFile { get; set; } = null!;

    [Option('s', "stream_id", Required = true, HelpText = "Either streamer name, or SSL stream ID (number)")]
    public string StreamId { get; set; } = null!;

    [Option('t', "access_token", Required = true, HelpText = "SSL User Access Token. Get from your Profile Settings -> API Access")]
    public string AccessToken { get; set; } = null!;
}
