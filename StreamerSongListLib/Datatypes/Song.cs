namespace StreamerSongList.Datatypes;

public class Song
{
    public int Id { get; set; } = 0;
    public string? Title { get; set; } = null;
    public string? Artist { get; set; } = null;
    public bool? Active { get; set; } = null;
    public int? MinTokens { get; set; } = null;
    public DateTimeOffset? CreatedAt { get; set; } = null;
    public DateTimeOffset? LastPlayed { get; set; } = null;
    public int? TimesPlayed { get; set; } = null;
    public int? NumQueued { get; set; } = null;

    public List<SongAttribute>? Attributes { get; set; } = null;

    public List<int>? AttributeIds { get; set; } = null;

    public override string ToString()
    {
        return $"{Artist ?? "Unknown Artist"} | {Title ?? "Unknown Title"}";
    }
}

public class UpdateSongBody
{
    //public bool Active { get; set; } = false;
    //public string Artist { get; set; } = "";
    public List<int>? AttributeIds { get; set; } = null;
    //public bool BypassRequestLimits { get; set; } = false;
    //public string Capo { get; set; } = "";
    //public string Chords { get; set; } = "";
    //public string Comment { get; set; } = "";
    //public int DurationSeconds { get; set; } = 0;
    //public string Lyrics { get; set; } = "";

    public int? MinAmount { get; set; } = null; // Should be MinTokens, but the API uses MinAmount for some reason
    //public string Tabs { get; set; } = "";
    //public string Title { get; set; } = "";    
}