using FFMpegCore.Exceptions;
using Instances;

namespace FFMpegCore.Helpers;

public static class FFMpegHelper
{
    private static bool _ffmpegVerified;

    public static void ConversionSizeExceptionCheck(IMediaAnalysis info)
    {
        ConversionSizeExceptionCheck(info.PrimaryVideoStream!.Width, info.PrimaryVideoStream.Height);
    }

    public static void ConversionSizeExceptionCheck(int width, int height)
    {
        if (height % 2 != 0 || width % 2 != 0)
        {
            throw new ArgumentException("FFMpeg yuv420p encoding requires the width and height to be a multiple of 2!");
        }
    }

    public static void ExtensionExceptionCheck(string filename, string extension)
    {
        if (!extension.Equals(Path.GetExtension(filename), StringComparison.OrdinalIgnoreCase))
        {
            throw new FFMpegException(FFMpegExceptionType.File,
                $"Invalid output file. File extension should be '{extension}' required.");
        }
    }

    public static void RootExceptionCheck() => RootExceptionCheck(GlobalFFOptions.Current);

    public static void RootExceptionCheck(FFOptions options)
    {
        if (options.BinaryFolder == null && string.IsNullOrWhiteSpace(options.FFMpegBinaryPath))
        {
            throw new FFOptionsException("FFMpeg root is not configured in app config. Missing key 'BinaryFolder'.");
        }
    }

    public static void VerifyFFMpegExists(FFOptions ffMpegOptions)
    {
        var explicitCommand = !string.IsNullOrWhiteSpace(ffMpegOptions.FFMpegBinaryPath);
        if (!explicitCommand && _ffmpegVerified)
        {
            return;
        }

        var result = Instance.Finish(GlobalFFOptions.GetFFMpegBinaryPath(ffMpegOptions), "-version");
        // An explicit command must never inherit or populate the legacy global verification cache.
        if (!explicitCommand)
        {
            _ffmpegVerified = result.ExitCode == 0;
        }

        if (result.ExitCode != 0)
        {
            throw new FFMpegException(FFMpegExceptionType.Operation, "ffmpeg was not found on your system");
        }
    }
}
