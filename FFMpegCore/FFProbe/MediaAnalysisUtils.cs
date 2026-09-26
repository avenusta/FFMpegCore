using System.Globalization;
using System.Text.RegularExpressions;

namespace FFMpegCore;

public static class MediaAnalysisUtils
{
    private static readonly Regex DurationRegex = new(@"^(\d+):(\d{1,2}):(\d{1,2})\.(\d{1,3})", RegexOptions.Compiled);

    internal static Dictionary<string, string> ToCaseInsensitive(this Dictionary<string, string>? dictionary)
    {
        return dictionary?.ToDictionary(tag => tag.Key, tag => tag.Value, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, string>();
    }

    public static double? DivideRatio((double, double) ratio)
    {
        var result = ratio.Item1 / ratio.Item2;
        return double.IsNaN(result) || double.IsInfinity(result) ? null : result;
    }

    public static (int, int) ParseRatioInt(string input, char separator)
    {
        if (string.IsNullOrEmpty(input))
        {
            return (0, 0);
        }

        var ratio = input.Split(separator);
        return (ParseIntInvariant(ratio[0]), ParseIntInvariant(ratio[1]));
    }

    public static (double, double) ParseRatioDouble(string input, char separator)
    {
        if (string.IsNullOrEmpty(input))
        {
            return (0, 0);
        }

        var ratio = input.Split(separator);
        return (ratio.Length > 0 ? ParseDoubleInvariant(ratio[0]) : 0, ratio.Length > 1 ? ParseDoubleInvariant(ratio[1]) : 0);
    }

    /// <summary>
    /// Parses ffprobe's <c>-show_data</c> hex dump ("00000000: 0164 001f ...  .d..") into bytes.
    /// Each line holds an 8-digit offset, ": ", a fixed 41-character hex column and an ASCII column.
    /// </summary>
    public static byte[]? ParseHexDump(string? dump)
    {
        if (dump is null || string.IsNullOrWhiteSpace(dump))
        {
            return null;
        }

        var bytes = new List<byte>();
        foreach (var line in dump.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Length < 10 || line[8] != ':')
            {
                continue;
            }

            var hex = line.Substring(10, Math.Min(41, line.Length - 10)).Replace(" ", string.Empty);
            for (var i = 0; i + 1 < hex.Length; i += 2)
            {
                bytes.Add(byte.Parse(hex.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            }
        }

        return bytes.Count == 0 ? null : bytes.ToArray();
    }

    public static double ParseDoubleInvariant(string line)
    {
        return double.TryParse(line, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0;
    }

    public static int ParseIntInvariant(string line)
    {
        return int.Parse(line, NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    public static long ParseLongInvariant(string line)
    {
        return long.Parse(line, NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    public static TimeSpan ParseDuration(string duration)
    {
        if (!string.IsNullOrEmpty(duration))
        {
            var match = DurationRegex.Match(duration);
            if (match.Success)
            {
                // ffmpeg may provide < 3-digit number of milliseconds (omitting trailing zeros), which won't simply parse correctly
                // e.g. 00:12:02.11 -> 12 minutes 2 seconds and 110 milliseconds
                var millisecondsPart = match.Groups[4].Value;
                if (millisecondsPart.Length < 3)
                {
                    millisecondsPart = millisecondsPart.PadRight(3, '0');
                }

                var hours = int.Parse(match.Groups[1].Value);
                var minutes = int.Parse(match.Groups[2].Value);
                var seconds = int.Parse(match.Groups[3].Value);
                var milliseconds = int.Parse(millisecondsPart);
                return new TimeSpan(0, hours, minutes, seconds, milliseconds);
            }

            return TimeSpan.Zero;
        }

        return TimeSpan.Zero;
    }

    public static int ParseRotation(FFProbeStream fFProbeStream)
    {
        var displayMatrixSideData = fFProbeStream.SideData?.Find(item =>
            item.TryGetValue("side_data_type", out var rawSideDataType) && rawSideDataType.ToString() == "Display Matrix");

        if (displayMatrixSideData?.TryGetValue("rotation", out var rawRotation) ?? false)
        {
            return (int)float.Parse(rawRotation.ToString());
        }

        return (int)float.Parse(fFProbeStream.GetRotate() ?? "0");
    }

    public static Dictionary<string, bool>? FormatDisposition(Dictionary<string, int>? disposition)
    {
        if (disposition == null)
        {
            return null;
        }

        var result = new Dictionary<string, bool>(disposition.Count, StringComparer.Ordinal);

        foreach (var pair in disposition)
        {
            result.Add(pair.Key, ToBool(pair.Value));
        }

        static bool ToBool(int value)
        {
            return value switch
            {
                0 => false,
                1 => true,
                _ => throw new ArgumentOutOfRangeException(nameof(value),
                    $"Not expected disposition state value: {value}")
            };
        }

        return result;
    }
}
