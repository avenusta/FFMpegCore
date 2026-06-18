using System.Text.Json;
using System.Text.Json.Nodes;
using FFMpegCore.Enums;

namespace FFMpegCore;

public abstract class MediaStream : IMediaStream, ITagsContainer
{
    public int Index { get; set; }
    public string? CodecName { get; set; }
    public string? CodecLongName { get; set; }
    public string? CodecTagString { get; set; }
    public string? CodecTag { get; set; }
    public long BitRate { get; set; }
    public TimeSpan StartTime { get; set; }
    public long? StartPts { get; set; }
    public TimeSpan Duration { get; set; }
    public (int Numerator, int Denominator) TimeBase { get; set; }
    public string? Language { get; set; }
    public Dictionary<string, bool>? Disposition { get; set; }
    public int? BitDepth { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
    public List<Dictionary<string, JsonValue>>? SideData { get; set; }

    public Codec GetCodecInfo()
    {
        return FFMpeg.GetCodec(CodecName ?? throw new InvalidOperationException("Stream has no codec name."));
    }

    /// <summary>
    /// Returns the side-data entry whose ffprobe <c>side_data_type</c> matches <paramref name="sideDataType"/>,
    /// deserialized into <typeparamref name="T"/>, or <c>null</c> when no such entry is present.
    /// </summary>
    public T? GetSideData<T>(string sideDataType) where T : class
    {
        var entry = SideData?.Find(item =>
            item.TryGetValue("side_data_type", out var rawType) &&
            rawType.GetValueKind() == JsonValueKind.String &&
            rawType.GetValue<string>() == sideDataType);

        if (entry == null)
        {
            return null;
        }

        var node = new JsonObject();
        foreach (var pair in entry)
        {
            node[pair.Key] = pair.Value?.DeepClone();
        }

        return node.Deserialize<T>();
    }

    /// <summary>Returns the Dolby Vision configuration record, or <c>null</c> when absent.</summary>
    public DoviConfiguration? GetDoviConfiguration()
    {
        return GetSideData<DoviConfiguration>(DoviConfiguration.SideDataType);
    }

    /// <summary>Returns the display matrix (transform/rotation), or <c>null</c> when absent.</summary>
    public DisplayMatrix? GetDisplayMatrix()
    {
        return GetSideData<DisplayMatrix>(DisplayMatrix.SideDataType);
    }
}
