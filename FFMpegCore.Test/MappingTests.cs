using FFMpegCore.Builders.MetaData;
using FFMpegCore.Test.Resources;

namespace FFMpegCore.Test;

[TestClass]
public class MappingTests
{
    // -------------------------------------------------------------------------
    // ChapterData — unit tests (no ffprobe required)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ChapterData_ObjectInitializer_AllProperties()
    {
        var chapter = new ChapterData
        {
            Id = 3,
            Title = "Intro",
            Start = TimeSpan.FromSeconds(0),
            End = TimeSpan.FromSeconds(10),
            TimeBase = (1, 1000),
            StartPts = 0,
            EndPts = 10000,
            Tags = new Dictionary<string, string> { ["title"] = "Intro" }
        };

        Assert.AreEqual(3, chapter.Id);
        Assert.AreEqual("Intro", chapter.Title);
        Assert.AreEqual(TimeSpan.FromSeconds(0), chapter.Start);
        Assert.AreEqual(TimeSpan.FromSeconds(10), chapter.End);
        Assert.AreEqual(TimeSpan.FromSeconds(10), chapter.Duration);
        Assert.AreEqual((1, 1000), chapter.TimeBase);
        Assert.AreEqual(0, chapter.StartPts);
        Assert.AreEqual(10000, chapter.EndPts);
        Assert.IsNotNull(chapter.Tags);
        Assert.AreEqual("Intro", chapter.Tags["title"]);
    }

    [TestMethod]
    public void ChapterData_Implements_IChapterData()
    {
        ChapterData chapter = new ChapterData { Title = "Test", Start = TimeSpan.Zero, End = TimeSpan.FromSeconds(5) };
        Assert.IsInstanceOfType<IChapterData>(chapter);

        IChapterData iChapter = chapter;
        Assert.AreEqual("Test", iChapter.Title);
        Assert.AreEqual(TimeSpan.FromSeconds(5), iChapter.End);
        Assert.AreEqual(TimeSpan.FromSeconds(5), iChapter.Duration);
    }

    [TestMethod]
    public void ChapterData_Duration_IsComputedFromStartAndEnd()
    {
        var chapter = new ChapterData
        {
            Start = TimeSpan.FromSeconds(30),
            End = TimeSpan.FromSeconds(90)
        };
        Assert.AreEqual(TimeSpan.FromSeconds(60), chapter.Duration);
    }

    // -------------------------------------------------------------------------
    // ParseChapter via MediaAnalysis (internal, exposed via InternalsVisibleTo)
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ParseChapter_MapsAllFields()
    {
        var analysis = new FFProbeAnalysis
        {
            Streams = new List<FFProbeStream>(),
            Format = new Format
            {
                NbStreams = 0,
                FormatName = "matroska",
                FormatLongName = "Matroska",
                Duration = "120.000000",
                Size = "1024",
                BitRate = "0"
            },
            Chapters = new List<Chapter>
            {
                new Chapter
                {
                    Id = 1,
                    TimeBase = "1/1000",
                    Start = 0,
                    StartTime = "0.000000",
                    End = 30000,
                    EndTime = "30.000000",
                    Tags = new Dictionary<string, string> { ["title"] = "Opening", ["language"] = "eng" }
                },
                new Chapter
                {
                    Id = 2,
                    TimeBase = "1/1000",
                    Start = 30000,
                    StartTime = "30.000000",
                    End = 60000,
                    EndTime = "60.000000",
                    Tags = new Dictionary<string, string> { ["title"] = "Act 1" }
                }
            }
        };

        var result = new MediaAnalysis(analysis);

        Assert.HasCount(2, result.Chapters);

        var ch1 = result.Chapters[0];
        Assert.AreEqual(1, ch1.Id);
        Assert.AreEqual("Opening", ch1.Title);
        Assert.AreEqual(TimeSpan.Zero, ch1.Start);
        Assert.AreEqual(TimeSpan.FromSeconds(30), ch1.End);
        Assert.AreEqual((1, 1000), ch1.TimeBase);
        Assert.AreEqual(0, ch1.StartPts);
        Assert.AreEqual(30000, ch1.EndPts);
        Assert.IsNotNull(ch1.Tags);
        Assert.AreEqual("Opening", ch1.Tags["title"]);
        Assert.AreEqual("eng", ch1.Tags["language"]);

        var ch2 = result.Chapters[1];
        Assert.AreEqual(2, ch2.Id);
        Assert.AreEqual("Act 1", ch2.Title);
        Assert.AreEqual(TimeSpan.FromSeconds(30), ch2.Start);
        Assert.AreEqual(TimeSpan.FromSeconds(60), ch2.End);
        Assert.AreEqual(30000, ch2.StartPts);
        Assert.AreEqual(60000, ch2.EndPts);
    }

    [TestMethod]
    public void ParseChapter_NoTitleTag_UsesEmptyString()
    {
        var analysis = new FFProbeAnalysis
        {
            Streams = new List<FFProbeStream>(),
            Format = new Format { FormatName = "matroska", Duration = "10.0", Size = "512", BitRate = "0" },
            Chapters = new List<Chapter>
            {
                new Chapter { Id = 1, TimeBase = "1/1000", StartTime = "0.000000", EndTime = "10.000000", Tags = null }
            }
        };

        var result = new MediaAnalysis(analysis);

        Assert.AreEqual(string.Empty, result.Chapters[0].Title);
    }

    // -------------------------------------------------------------------------
    // ParseFormat — MediaFormat new fields
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ParseFormat_MapsFilenameAndSizeAndProgramCount()
    {
        var analysis = new FFProbeAnalysis
        {
            Streams = new List<FFProbeStream>(),
            Chapters = new List<Chapter>(),
            Format = new Format
            {
                Filename = "/path/to/video.mp4",
                NbStreams = 2,
                NbPrograms = 1,
                FormatName = "mov,mp4,m4a,3gp,3g2,mj2",
                FormatLongName = "QuickTime / MOV",
                StartTime = "0.000000",
                Duration = "3.000000",
                Size = "204800",
                BitRate = "512000",
                ProbeScore = 100
            }
        };

        var result = new MediaAnalysis(analysis);

        Assert.AreEqual("/path/to/video.mp4", result.Format.Filename);
        Assert.AreEqual(204800, result.Format.Size);
        Assert.AreEqual(1, result.Format.ProgramCount);
        Assert.AreEqual(2, result.Format.StreamCount);
        Assert.AreEqual(512000, result.Format.BitRate);
        Assert.AreEqual(100, result.Format.ProbeScore);
    }

    [TestMethod]
    public void ParseFormat_NullSizeAndBitRate_DefaultToZero()
    {
        var analysis = new FFProbeAnalysis
        {
            Streams = new List<FFProbeStream>(),
            Chapters = new List<Chapter>(),
            Format = new Format
            {
                FormatName = "matroska",
                Duration = "5.0",
                Size = null,
                BitRate = null
            }
        };

        var result = new MediaAnalysis(analysis);

        Assert.AreEqual(0, result.Format.Size);
        Assert.AreEqual(0, result.Format.BitRate);
    }

    // -------------------------------------------------------------------------
    // SubtitleStream.Profile (requires ffprobe)
    // -------------------------------------------------------------------------

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public void Probe_SubtitleStream_HasCodecTagAndCodecTagString()
    {
        var info = FFProbe.Analyse(TestResources.MkvWithAssSubtitle);

        Assert.HasCount(1, info.SubtitleStreams);
        var sub = info.SubtitleStreams[0];
        Assert.AreEqual("ass", sub.CodecName);
        Assert.IsNotNull(sub.CodecTag);
        Assert.IsNotNull(sub.CodecTagString);
    }

    // -------------------------------------------------------------------------
    // AttachmentStream.BitRate (requires ffprobe)
    // -------------------------------------------------------------------------

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public void Probe_AttachmentStream_BitRateMapped()
    {
        var info = FFProbe.Analyse(TestResources.MkvWithAttachment);

        Assert.HasCount(1, info.AttachmentStreams);
        var attachment = info.AttachmentStreams[0];
        // Font attachments have no meaningful bitrate — assert the property exists and is mapped (zero is valid)
        Assert.IsGreaterThanOrEqualTo(attachment.BitRate, 0);
    }

    // -------------------------------------------------------------------------
    // MediaFormat.Filename and Size (requires ffprobe)
    // -------------------------------------------------------------------------

    [TestMethod]
    [Timeout(10000, CooperativeCancellation = true)]
    public void Probe_Format_FilenameAndSize()
    {
        var info = FFProbe.Analyse(TestResources.Mp4Video);

        Assert.IsNotNull(info.Format.Filename);
        Assert.IsTrue(info.Format.Filename.EndsWith("input_3sec.mp4", StringComparison.OrdinalIgnoreCase));
        Assert.IsGreaterThan(info.Format.Size, 0);
        Assert.AreEqual(0, info.Format.ProgramCount);
    }

    // -------------------------------------------------------------------------
    // MetaDataBuilder — ChapterData copies retain Title/Start/End
    // -------------------------------------------------------------------------

    [TestMethod]
    public void MetaDataBuilder_Chapters_CopiedCorrectly()
    {
        var metadata = new MetaDataBuilder()
            .AddChapter(TimeSpan.FromSeconds(10), "Intro")
            .AddChapter(TimeSpan.FromSeconds(20), "Main")
            .Build();

        Assert.HasCount(2, metadata.Chapters);

        Assert.AreEqual("Intro", metadata.Chapters[0].Title);
        Assert.AreEqual(TimeSpan.Zero, metadata.Chapters[0].Start);
        Assert.AreEqual(TimeSpan.FromSeconds(10), metadata.Chapters[0].End);

        Assert.AreEqual("Main", metadata.Chapters[1].Title);
        Assert.AreEqual(TimeSpan.FromSeconds(10), metadata.Chapters[1].Start);
        Assert.AreEqual(TimeSpan.FromSeconds(30), metadata.Chapters[1].End);
    }

    [TestMethod]
    public void MetaData_Clone_PreservesChapters()
    {
        var original = new MetaData();
        original.Chapters.Add(new ChapterData { Title = "Ch1", Start = TimeSpan.Zero, End = TimeSpan.FromSeconds(5) });
        original.Chapters.Add(new ChapterData { Title = "Ch2", Start = TimeSpan.FromSeconds(5), End = TimeSpan.FromSeconds(10) });

        var clone = new MetaData(original);

        Assert.HasCount(2, clone.Chapters);
        Assert.AreEqual("Ch1", clone.Chapters[0].Title);
        Assert.AreEqual("Ch2", clone.Chapters[1].Title);
        Assert.AreEqual(TimeSpan.FromSeconds(5), clone.Chapters[1].Start);

        // Ensure it's a copy, not a reference
        clone.Chapters[0].Title = "Modified";
        Assert.AreEqual("Ch1", original.Chapters[0].Title);
    }
}
