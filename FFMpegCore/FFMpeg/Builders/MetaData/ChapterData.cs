namespace FFMpegCore.Builders.MetaData;

public class ChapterData : IChapterData
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }
    public (int Numerator, int Denominator) TimeBase { get; set; }
    public long StartPts { get; set; }
    public long EndPts { get; set; }
    public Dictionary<string, string>? Tags { get; set; }

    public TimeSpan Duration => End - Start;
}
