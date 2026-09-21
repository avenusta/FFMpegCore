using FFMpegCore.Exceptions;

namespace FFMpegCore.Helpers;

public static class FFProbeHelper
{
    private static bool _ffprobeVerified;

    public static void RootExceptionCheck() => RootExceptionCheck(GlobalFFOptions.Current);

    public static void RootExceptionCheck(FFOptions options)
    {
        if (options.BinaryFolder == null && string.IsNullOrWhiteSpace(options.FFProbeBinaryPath))
        {
            throw new FFOptionsException("FFProbe root is not configured in app config. Missing key 'BinaryFolder'.");
        }
    }

    public static void VerifyFFProbeExists(FFOptions ffMpegOptions)
    {
        var explicitCommand = !string.IsNullOrWhiteSpace(ffMpegOptions.FFProbeBinaryPath);
        if (!explicitCommand && _ffprobeVerified)
        {
            return;
        }

        var result = ProcessHelper.Run(GlobalFFOptions.GetFFProbeBinaryPath(ffMpegOptions), "-version");
        // An explicit command must never inherit or populate the legacy global verification cache.
        if (!explicitCommand)
        {
            _ffprobeVerified = result.ExitCode == 0;
        }

        if (result.ExitCode != 0)
        {
            throw new FFProbeException("ffprobe was not found on your system");
        }
    }
}
