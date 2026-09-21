using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;
using FFMpegCore.Test.Resources;

namespace FFMpegCore.Test;

[TestClass]
public class ExecutablePathIntegrationTests
{
    [TestMethod]
    public async Task Probe_And_Encode_Invoke_Explicit_Wrappers_With_Spaces()
    {
        using var commands = new Commands();
        var options = commands.Options;
        var analysis = await FFProbe.AnalyseAsync(TestResources.Mp4Video, options);
        Assert.IsNotNull(analysis.PrimaryVideoStream);
        var output = Path.Combine(commands.DirectoryPath, "output.mp4");
        await FFMpegArguments.FromFileInput(TestResources.Mp4Video)
            .OutputToFile(output, true, arguments => arguments.CopyChannel())
            .ProcessAsynchronously(true, options);
        Assert.IsTrue(File.Exists(output));
        var probeLog = File.ReadAllText(options.FFProbeBinaryPath + ".log");
        var encoderLog = File.ReadAllText(options.FFMpegBinaryPath + ".log");
        StringAssert.Contains(probeLog, "-version");
        StringAssert.Contains(probeLog, "-show_streams");
        StringAssert.Contains(encoderLog, "-version");
        StringAssert.Contains(encoderLog, "output.mp4");
    }

    [TestMethod]
    public async Task Concurrent_Probes_Use_Their_Own_Commands()
    {
        using var first = new Commands();
        using var second = new Commands();
        var results = await Task.WhenAll(
            FFProbe.AnalyseAsync(TestResources.Mp4Video, first.Options),
            FFProbe.AnalyseAsync(TestResources.Mp4Video, second.Options));
        Assert.IsTrue(results.All(result => result.PrimaryVideoStream != null));
        StringAssert.Contains(File.ReadAllText(first.Options.FFProbeBinaryPath + ".log"), "-show_streams");
        StringAssert.Contains(File.ReadAllText(second.Options.FFProbeBinaryPath + ".log"), "-show_streams");
    }

    [TestMethod]
    public void Failed_Explicit_Commands_Do_Not_Inherit_Previous_Verification()
    {
        using var commands = new Commands();
        FFMpegHelper.VerifyFFMpegExists(new FFOptions());
        FFProbeHelper.VerifyFFProbeExists(new FFOptions());
        FFMpegHelper.VerifyFFMpegExists(commands.Options);
        FFProbeHelper.VerifyFFProbeExists(commands.Options);
        File.WriteAllText(commands.Options.FFMpegBinaryPath, "#!/bin/sh\nexit 23\n");
        File.WriteAllText(commands.Options.FFProbeBinaryPath, "#!/bin/sh\nexit 23\n");
        Assert.ThrowsExactly<FFMpegException>(() => FFMpegHelper.VerifyFFMpegExists(commands.Options));
        Assert.ThrowsExactly<FFProbeException>(() => FFProbeHelper.VerifyFFProbeExists(commands.Options));
    }

    [TestMethod]
    public async Task Missing_Explicit_Probe_Does_Not_Fall_Back_To_Path()
    {
        using var commands = new Commands();
        FFProbeHelper.VerifyFFProbeExists(new FFOptions());
        File.Delete(commands.Options.FFProbeBinaryPath);
        await Assert.ThrowsAsync<Instances.Exceptions.InstanceFileNotFoundException>(() =>
            FFProbe.AnalyseAsync(TestResources.Mp4Video, commands.Options));
    }

    private sealed class Commands : IDisposable
    {
        public string DirectoryPath { get; }
        public FFOptions Options { get; }

        public Commands()
        {
            if (OperatingSystem.IsWindows())
            {
                Assert.Inconclusive("POSIX wrapper invocation test; path resolution tests cover Windows executable names.");
            }

            DirectoryPath = Path.Combine(Path.GetTempPath(), $"ffmpeg commands {Guid.NewGuid():N}");
            Directory.CreateDirectory(DirectoryPath);
            Options = new FFOptions
            {
                BinaryFolder = null,
                FFMpegBinaryPath = Create("custom encoder", "ffmpeg"),
                FFProbeBinaryPath = Create("custom probe", "ffprobe")
            };
        }

        private string Create(string name, string command)
        {
            var path = Path.Combine(DirectoryPath, name);
            File.WriteAllText(path, $"#!/bin/sh\nprintf '%s\\n' \"$*\" >> \"$0.log\"\nexec {command} \"$@\"\n");
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }

            return path;
        }

        public void Dispose() => Directory.Delete(DirectoryPath, true);
    }
}
