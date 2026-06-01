namespace StreamerSongList.Datatypes;

public class Song
{
    public int Id { get; set; } = 0;
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public bool Active { get; set; } = false;
    public float MinAmount { get; set; } = 0.0f;
    public DateTimeOffset? CreatedAt { get; set; } = DateTimeOffset.MinValue;
    public DateTimeOffset? LastPlayed { get; set; } = DateTimeOffset.MinValue;
    public int TimesPlayed { get; set; } = 0;
    public int NumQueued { get; set; } = 0;

    public List<int> Attributes { get; set; } = new();
    public List<int> AttributeIds { get; set; } = new();

    public override string ToString()
    {
        return $"{Artist} | {Title}";
    }
}