namespace FFMpegCore.Builders.MetaData;

public class MetaData : IReadOnlyMetaData
{
    public MetaData()
    {
        Entries = new Dictionary<string, string>();
        Chapters = new List<ChapterData>();
    }

    public MetaData(MetaData cloneSource)
    {
        Entries = new Dictionary<string, string>(cloneSource.Entries);
        Chapters = cloneSource.Chapters
            .Select(x => new ChapterData { Start = x.Start, End = x.End, Title = x.Title })
            .ToList();
    }

    public Dictionary<string, string> Entries { get; }
    public List<ChapterData> Chapters { get; }

    IReadOnlyList<ChapterData> IReadOnlyMetaData.Chapters => Chapters;
    IReadOnlyDictionary<string, string> IReadOnlyMetaData.Entries => Entries;
}
