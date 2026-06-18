using FFMpegCore.Enums;

namespace FFMpegCore;

public class VideoStream : MediaStream, IVideoStream
{
    public double AvgFrameRate { get; set; }
    public int BitsPerRawSample { get; set; }
    public (int Width, int Height) DisplayAspectRatio { get; set; }
    public (int Width, int Height) SampleAspectRatio { get; set; }
    public string? Profile { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double FrameRate { get; set; }
    public string? PixelFormat { get; set; }
    public int Level { get; set; }
    public string? FieldOrder { get; set; }
    public int HasBFrames { get; set; }
    public bool? IsAvc { get; set; }
    public int NalLengthSize { get; set; }
    public int Rotation { get; set; }
    public double AverageFrameRate { get; set; }
    public string? ColorRange { get; set; }
    public string? ColorSpace { get; set; }
    public string? ColorTransfer { get; set; }
    public string? ColorPrimaries { get; set; }

    public PixelFormat GetPixelFormatInfo()
    {
        return FFMpeg.GetPixelFormat(PixelFormat ?? throw new InvalidOperationException("Stream has no pixel format."));
    }
}
