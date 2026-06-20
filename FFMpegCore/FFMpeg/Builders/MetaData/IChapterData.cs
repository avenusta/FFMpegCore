namespace FFMpegCore;

public interface IChapterData
{
    int Id { get; set; }
    string Title { get; set; }
    TimeSpan Start { get; set; }
    TimeSpan End { get; set; }
    TimeSpan Duration { get; }
    (int Numerator, int Denominator) TimeBase { get; set; }
    long StartPts { get; set; }
    long EndPts { get; set; }
    Dictionary<string, string>? Tags { get; set; }
}
