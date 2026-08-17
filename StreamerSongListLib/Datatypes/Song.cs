namespace StreamerSongList.Datatypes;

public class Song
{
    public int Id { get; set; } = 0;
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public bool Active { get; set; } = false;
    public int MinTokens { get; set; } = 0;
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.MinValue;
    public DateTimeOffset? LastPlayed { get; set; } = DateTimeOffset.MinValue;
    public int TimesPlayed { get; set; } = 0;
    public int NumQueued { get; set; } = 0;

    public List<SongAttribute> Attributes { get; set; } = new();

    public override string ToString()
    {
        return $"{Artist} | {Title}";
    }
}

public class UpdateSongBody
{
    //public bool Active { get; set; } = false;
    //public string Artist { get; set; } = "";
    public List<int> AttributeIds { get; set; } = new();
    //public bool BypassRequestLimits { get; set; } = false;
    //public string Capo { get; set; } = "";
    //public string Chords { get; set; } = "";
    //public string Comment { get; set; } = "";
    //public int DurationSeconds { get; set; } = 0;
    //public string Lyrics { get; set; } = "";

    public int MinAmount { get; set; } = 0; // Should be MinTokens, but the API uses MinAmount for some reason
    //public string Tabs { get; set; } = "";
    //public string Title { get; set; } = "";    
}