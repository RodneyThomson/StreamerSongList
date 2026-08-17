namespace StreamerSongList.Datatypes;

public class SongAttributeList
{
    public List<SongAttribute> Items { get; set; } = new();
    public int Total { get; set; } = 0;
}

public class SongAttribute
{
    // Only care about ID and Name for now
    public int Id { get; set; } = 0;
    public string Name { get; set; } = "";
    //public string? Image { get; set; } = null;
}