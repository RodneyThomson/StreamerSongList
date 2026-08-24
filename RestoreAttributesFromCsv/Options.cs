using CommandLine;

internal class Options
{
    [Option('i', "input", Required = true, HelpText = "Input CSV file containing SSL song export")]
    public string InputFile { get; set; } = null!;

    [Option('s', "stream_id", Required = true, HelpText = "Either streamer name, or SSL stream ID (number)")]
    public string StreamId { get; set; } = null!;

    [Option('t', "access_token", Required = true, HelpText = "SSL User Access Token. Get from your Profile Settings -> API Access")]
    public string AccessToken { get; set; } = null!;

    [Option('w', "write", Required = false, HelpText = "Without -w, the attributes will NOT be written. Running without -w can be used to check for song ID mismatches")]
    public bool Write { get; set; } = false;
}
