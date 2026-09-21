using FFMpegCore.Builders.MetaData;

namespace FFMpegCore;

internal class MediaAnalysis : IMediaAnalysis
{
    internal MediaAnalysis(FFProbeAnalysis analysis)
    {
        Format = ParseFormat(analysis.Format);
        Chapters = analysis.Chapters.Select(c => ParseChapter(c)).ToList();
        VideoStreams = analysis.Streams.Where(stream => stream.CodecType == "video").Select(ParseVideoStream).ToList();
        AudioStreams = analysis.Streams.Where(stream => stream.CodecType == "audio").Select(ParseAudioStream).ToList();
        SubtitleStreams = analysis.Streams.Where(stream => stream.CodecType == "subtitle").Select(ParseSubtitleStream).ToList();
        AttachmentStreams = analysis.Streams.Where(stream => stream.CodecType == "attachment").Select(ParseAttachmentStream).ToList();
        ErrorData = analysis.ErrorData ?? Array.Empty<string>();
    }

    public TimeSpan Duration => new[] { Format.Duration, PrimaryVideoStream?.Duration ?? TimeSpan.Zero, PrimaryAudioStream?.Duration ?? TimeSpan.Zero }.Max();

    public MediaFormat Format { get; }

    public List<ChapterData> Chapters { get; }

    public AudioStream? PrimaryAudioStream => AudioStreams.OrderBy(stream => stream.Index).FirstOrDefault();
    public VideoStream? PrimaryVideoStream => VideoStreams.OrderBy(stream => stream.Index).FirstOrDefault();
    public SubtitleStream? PrimarySubtitleStream => SubtitleStreams.OrderBy(stream => stream.Index).FirstOrDefault();

    public List<VideoStream> VideoStreams { get; }
    public List<AudioStream> AudioStreams { get; }
    public List<SubtitleStream> SubtitleStreams { get; }
    public List<AttachmentStream> AttachmentStreams { get; }
    public IReadOnlyList<string> ErrorData { get; }

    private MediaFormat ParseFormat(Format analysisFormat)
    {
        return new MediaFormat
        {
            Filename = analysisFormat.Filename,
            Duration = MediaAnalysisUtils.ParseDuration(analysisFormat.Duration),
            StartTime = MediaAnalysisUtils.ParseDuration(analysisFormat.StartTime),
            FormatName = analysisFormat.FormatName,
            FormatLongName = analysisFormat.FormatLongName,
            StreamCount = analysisFormat.NbStreams,
            ProgramCount = analysisFormat.NbPrograms,
            ProbeScore = analysisFormat.ProbeScore,
            BitRate = long.Parse(analysisFormat.BitRate ?? "0"),
            Size = long.Parse(analysisFormat.Size ?? "0"),
            Tags = analysisFormat.Tags.ToCaseInsensitive()
        };
    }

    private string GetValue(string tagName, Dictionary<string, string>? tags, string defaultValue)
    {
        return tags == null ? defaultValue : tags.TryGetValue(tagName, out var value) ? value : defaultValue;
    }

    private ChapterData ParseChapter(Chapter analysisChapter)
    {
        return new ChapterData
        {
            Id = (int)analysisChapter.Id,
            Title = GetValue("title", analysisChapter.Tags, string.Empty),
            Start = MediaAnalysisUtils.ParseDuration(analysisChapter.StartTime),
            End = MediaAnalysisUtils.ParseDuration(analysisChapter.EndTime),
            TimeBase = MediaAnalysisUtils.ParseRatioInt(analysisChapter.TimeBase, '/'),
            StartPts = analysisChapter.Start,
            EndPts = analysisChapter.End,
            Tags = analysisChapter.Tags.ToCaseInsensitive()
        };
    }

    private int? GetBitDepth(FFProbeStream stream)
    {
        var bitDepth = int.TryParse(stream.BitsPerRawSample, out var bprs) ? bprs : stream.BitsPerSample;
        return bitDepth == 0 ? null : bitDepth;
    }

    private VideoStream ParseVideoStream(FFProbeStream stream)
    {
        return new VideoStream
        {
            Index = stream.Index,
            AvgFrameRate = MediaAnalysisUtils.DivideRatio(MediaAnalysisUtils.ParseRatioDouble(stream.AvgFrameRate, '/')),
            BitRate = !string.IsNullOrEmpty(stream.BitRate) ? MediaAnalysisUtils.ParseLongInvariant(stream.BitRate) : default,
            BitsPerRawSample = !string.IsNullOrEmpty(stream.BitsPerRawSample) ? MediaAnalysisUtils.ParseIntInvariant(stream.BitsPerRawSample) : default,
            CodecName = stream.CodecName,
            CodecLongName = stream.CodecLongName,
            CodecTag = stream.CodecTag,
            CodecTagString = stream.CodecTagString,
            DisplayAspectRatio = MediaAnalysisUtils.ParseRatioInt(stream.DisplayAspectRatio, ':'),
            SampleAspectRatio = MediaAnalysisUtils.ParseRatioInt(stream.SampleAspectRatio, ':'),
            Duration = MediaAnalysisUtils.ParseDuration(stream.Duration),
            StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
            StartPts = stream.StartPts,
            TimeBase = MediaAnalysisUtils.ParseRatioInt(stream.TimeBase, '/'),
            FrameRate = MediaAnalysisUtils.DivideRatio(MediaAnalysisUtils.ParseRatioDouble(stream.FrameRate, '/')),
            Height = stream.Height ?? 0,
            Width = stream.Width ?? 0,
            Profile = stream.Profile,
            PixelFormat = stream.PixelFormat,
            Level = stream.Level,
            FieldOrder = stream.FieldOrder,
            HasBFrames = stream.HasBFrames ?? default,
            IsAvc = !string.IsNullOrEmpty(stream.IsAvc) && bool.TryParse(stream.IsAvc, out var isAvc) ? isAvc : null,
            NalLengthSize = !string.IsNullOrEmpty(stream.NalLengthSize) ? MediaAnalysisUtils.ParseIntInvariant(stream.NalLengthSize) : default,
            ColorRange = stream.ColorRange,
            ColorSpace = stream.ColorSpace,
            ColorTransfer = stream.ColorTransfer,
            ColorPrimaries = stream.ColorPrimaries,
            Rotation = MediaAnalysisUtils.ParseRotation(stream),
            Language = stream.GetLanguage(),
            Disposition = MediaAnalysisUtils.FormatDisposition(stream.Disposition),
            Tags = stream.Tags.ToCaseInsensitive(),
            BitDepth = GetBitDepth(stream),
            SideData = stream.SideData
        };
    }

    private AudioStream ParseAudioStream(FFProbeStream stream)
    {
        return new AudioStream
        {
            Index = stream.Index,
            BitRate = !string.IsNullOrEmpty(stream.BitRate) ? MediaAnalysisUtils.ParseLongInvariant(stream.BitRate) : default,
            CodecName = stream.CodecName,
            CodecLongName = stream.CodecLongName,
            CodecTag = stream.CodecTag,
            CodecTagString = stream.CodecTagString,
            Channels = stream.Channels ?? default,
            ChannelLayout = stream.ChannelLayout,
            Duration = MediaAnalysisUtils.ParseDuration(stream.Duration),
            StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
            TimeBase = MediaAnalysisUtils.ParseRatioInt(stream.TimeBase, '/'),
            StartPts = stream.StartPts,
            SampleRateHz = !string.IsNullOrEmpty(stream.SampleRate) ? MediaAnalysisUtils.ParseIntInvariant(stream.SampleRate) : default,
            SampleFormat = stream.SampleFormat,
            Profile = stream.Profile,
            DmixMode = stream.DmixMode,
            LtrtCmixlev = stream.LtrtCmixlev,
            LtrtSurmixlev = stream.LtrtSurmixlev,
            LoroCmixlev = stream.LoroCmixlev,
            LoroSurmixlev = stream.LoroSurmixlev,
            Language = stream.GetLanguage(),
            Disposition = MediaAnalysisUtils.FormatDisposition(stream.Disposition),
            Tags = stream.Tags.ToCaseInsensitive(),
            BitDepth = GetBitDepth(stream),
            SideData = stream.SideData
        };
    }

    private SubtitleStream ParseSubtitleStream(FFProbeStream stream)
    {
        return new SubtitleStream
        {
            Index = stream.Index,
            BitRate = !string.IsNullOrEmpty(stream.BitRate) ? MediaAnalysisUtils.ParseLongInvariant(stream.BitRate) : default,
            CodecName = stream.CodecName,
            CodecLongName = stream.CodecLongName,
            CodecTag = stream.CodecTag,
            CodecTagString = stream.CodecTagString,
            Profile = stream.Profile,
            Duration = MediaAnalysisUtils.ParseDuration(stream.Duration),
            StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
            StartPts = stream.StartPts,
            TimeBase = MediaAnalysisUtils.ParseRatioInt(stream.TimeBase, '/'),
            Language = stream.GetLanguage(),
            Disposition = MediaAnalysisUtils.FormatDisposition(stream.Disposition),
            Tags = stream.Tags.ToCaseInsensitive(),
            SideData = stream.SideData
        };
    }

    private AttachmentStream ParseAttachmentStream(FFProbeStream stream)
    {
        return new AttachmentStream
        {
            Index = stream.Index,
            BitRate = !string.IsNullOrEmpty(stream.BitRate) ? MediaAnalysisUtils.ParseLongInvariant(stream.BitRate) : default,
            CodecName = stream.CodecName,
            CodecLongName = stream.CodecLongName,
            CodecTag = stream.CodecTag,
            CodecTagString = stream.CodecTagString,
            Duration = MediaAnalysisUtils.ParseDuration(stream.Duration),
            StartTime = MediaAnalysisUtils.ParseDuration(stream.StartTime),
            StartPts = stream.StartPts,
            TimeBase = MediaAnalysisUtils.ParseRatioInt(stream.TimeBase, '/'),
            Language = stream.GetLanguage(),
            Disposition = MediaAnalysisUtils.FormatDisposition(stream.Disposition),
            Tags = stream.Tags.ToCaseInsensitive(),
            SideData = stream.SideData
        };
    }
}
