using System.Text.Json.Serialization;

namespace FFMpegCore;

/// <summary>
/// Strongly-typed view of a "Display Matrix" side-data entry (transform / rotation).
/// </summary>
public class DisplayMatrix
{
    public const string SideDataType = "Display Matrix";

    [JsonPropertyName("rotation")] public int Rotation { get; set; }

    [JsonPropertyName("displaymatrix")] public string? Matrix { get; set; }
}
