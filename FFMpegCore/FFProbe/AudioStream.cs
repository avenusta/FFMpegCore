namespace FFMpegCore;

public class AudioStream : MediaStream
{
    public int Channels { get; set; }
    public string? ChannelLayout { get; set; }
    public int SampleRateHz { get; set; }
    public string? SampleFormat { get; set; }
    public string? Profile { get; set; }
    public string? DmixMode { get; set; }
    public string? LtrtCmixlev { get; set; }
    public string? LtrtSurmixlev { get; set; }
    public string? LoroCmixlev { get; set; }
    public string? LoroSurmixlev { get; set; }
}
