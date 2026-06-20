namespace FFMpegCore;

public class MediaFormat : ITagsContainer
{
    public string? Filename { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan StartTime { get; set; }
    public string? FormatName { get; set; }
    public string? FormatLongName { get; set; }
    public int StreamCount { get; set; }
    public int ProgramCount { get; set; }
    public double ProbeScore { get; set; }
    public double BitRate { get; set; }
    public long Size { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
}
