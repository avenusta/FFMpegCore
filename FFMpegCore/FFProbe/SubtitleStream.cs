namespace FFMpegCore;

public class SubtitleStream : MediaStream, ISubtitleStream
{
    public string? Profile { get; set; }
}
