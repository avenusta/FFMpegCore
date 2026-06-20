using System.Text.Json.Nodes;
using FFMpegCore.Enums;

namespace FFMpegCore;

public interface IMediaStream
{
    int Index { get; set; }
    string? CodecName { get; set; }
    string? CodecLongName { get; set; }
    string? CodecTagString { get; set; }
    string? CodecTag { get; set; }
    long BitRate { get; set; }
    TimeSpan StartTime { get; set; }
    long? StartPts { get; set; }
    TimeSpan Duration { get; set; }
    (int Numerator, int Denominator) TimeBase { get; set; }
    string? Language { get; set; }
    int? BitDepth { get; set; }
    Dictionary<string, bool>? Disposition { get; set; }
    Dictionary<string, string>? Tags { get; set; }
    List<Dictionary<string, JsonValue>>? SideData { get; set; }
}

public interface IVideoStream : IMediaStream
{
    double AvgFrameRate { get; set; }
    int BitsPerRawSample { get; set; }
    (int Width, int Height) DisplayAspectRatio { get; set; }
    (int Width, int Height) SampleAspectRatio { get; set; }
    string? Profile { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    double FrameRate { get; set; }
    string? PixelFormat { get; set; }
    int Level { get; set; }
    string? FieldOrder { get; set; }
    int HasBFrames { get; set; }
    bool? IsAvc { get; set; }
    int NalLengthSize { get; set; }
    int Rotation { get; set; }
    double AverageFrameRate { get; set; }
    string? ColorRange { get; set; }
    string? ColorSpace { get; set; }
    string? ColorTransfer { get; set; }
    string? ColorPrimaries { get; set; }
}

public interface IAudioStream : IMediaStream
{
    int Channels { get; set; }
    string? ChannelLayout { get; set; }
    int SampleRateHz { get; set; }
    string? SampleFormat { get; set; }
    string? Profile { get; set; }
    string? DmixMode { get; set; }
    string? LtrtCmixlev { get; set; }
    string? LtrtSurmixlev { get; set; }
    string? LoroCmixlev { get; set; }
    string? LoroSurmixlev { get; set; }
}

public interface ISubtitleStream : IMediaStream { }

public interface IAttachmentStream : IMediaStream { }

public interface IMediaFormat : ITagsContainer
{
    string? Filename { get; set; }
    TimeSpan Duration { get; set; }
    TimeSpan StartTime { get; set; }
    string? FormatName { get; set; }
    string? FormatLongName { get; set; }
    int StreamCount { get; set; }
    int ProgramCount { get; set; }
    double ProbeScore { get; set; }
    double BitRate { get; set; }
    long Size { get; set; }
    Dictionary<string, string>? Tags { get; set; }
}
