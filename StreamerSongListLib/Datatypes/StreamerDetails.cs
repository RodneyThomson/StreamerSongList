namespace StreamerSongList.Datatypes;

/// <summary>
///   https://dev.streamersonglist.com/api-reference?method=get&path=%2Fstreamers
/// </summary>
public class StreamerDetails
{
    // Only care about Id for now
    public int Id { get; set; } = -1;
}
