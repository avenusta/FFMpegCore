using System.Text.Json;
using FFMpegCore.Exceptions;
using FFMpegCore.Helpers;

namespace FFMpegCore.Test;

[TestClass]
public class ExecutablePathTests
{
    [TestMethod]
    public void Overrides_Default_To_Legacy_Lookup()
    {
        var options = new FFOptions();
        Assert.IsNull(options.FFMpegBinaryPath);
        Assert.IsNull(options.FFProbeBinaryPath);
    }

    [TestMethod]
    [DataRow("/missing/media tools/custom-encoder", "/missing/media tools/custom-probe")]
    [DataRow("custom-encoder", "custom-probe")]
    [DataRow("C:\\Media Tools\\encode.exe", "C:\\Media Tools\\probe.exe")]
    public void Explicit_Commands_Are_Returned_Unchanged(string encoder, string probe)
    {
        var options = new FFOptions
        {
            BinaryFolder = "ignored",
            FFMpegBinaryPath = encoder,
            FFProbeBinaryPath = probe
        };
        Assert.AreEqual(encoder, GlobalFFOptions.GetFFMpegBinaryPath(options));
        Assert.AreEqual(probe, GlobalFFOptions.GetFFProbeBinaryPath(options));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" \t")]
    public void Blank_Overrides_Preserve_Legacy_Resolution(string value)
    {
        var legacy = new FFOptions { BinaryFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()) };
        var options = legacy.Clone();
        options.FFMpegBinaryPath = value;
        options.FFProbeBinaryPath = value;
        Assert.AreEqual(GlobalFFOptions.GetFFMpegBinaryPath(legacy), GlobalFFOptions.GetFFMpegBinaryPath(options));
        Assert.AreEqual(GlobalFFOptions.GetFFProbeBinaryPath(legacy), GlobalFFOptions.GetFFProbeBinaryPath(options));
    }

    [TestMethod]
    public void Overrides_Are_Independent()
    {
        var options = new FFOptions { FFMpegBinaryPath = "custom-encoder" };
        Assert.AreEqual(GlobalFFOptions.GetFFProbeBinaryPath(new FFOptions()), GlobalFFOptions.GetFFProbeBinaryPath(options));
        options = new FFOptions { FFProbeBinaryPath = "custom-probe" };
        Assert.AreEqual(GlobalFFOptions.GetFFMpegBinaryPath(new FFOptions()), GlobalFFOptions.GetFFMpegBinaryPath(options));
    }

    [TestMethod]
    public void Clone_And_Json_Preserve_Overrides()
    {
        var options = new FFOptions { FFMpegBinaryPath = "encoder", FFProbeBinaryPath = "probe" };
        var clone = options.Clone();
        clone.FFMpegBinaryPath = "other";
        Assert.AreEqual("encoder", options.FFMpegBinaryPath);
        Assert.AreEqual("probe", clone.FFProbeBinaryPath);
        var roundTrip = JsonSerializer.Deserialize<FFOptions>(JsonSerializer.Serialize(options));
        Assert.AreEqual(options.FFMpegBinaryPath, roundTrip.FFMpegBinaryPath);
        Assert.AreEqual(options.FFProbeBinaryPath, roundTrip.FFProbeBinaryPath);
    }

    [TestMethod]
    public void Explicit_Commands_Do_Not_Require_A_BinaryFolder()
    {
        var options = new FFOptions { BinaryFolder = null, FFMpegBinaryPath = "encoder", FFProbeBinaryPath = "probe" };
        FFMpegHelper.RootExceptionCheck(options);
        FFProbeHelper.RootExceptionCheck(options);
        options.FFMpegBinaryPath = null;
        Assert.ThrowsExactly<FFOptionsException>(() => FFMpegHelper.RootExceptionCheck(options));
        options.FFProbeBinaryPath = null;
        Assert.ThrowsExactly<FFOptionsException>(() => FFProbeHelper.RootExceptionCheck(options));
    }

    [TestMethod]
    public async Task Per_Call_Resolution_Does_Not_Change_Global_Options()
    {
        var global = GlobalFFOptions.Current;
        var encoder = global.FFMpegBinaryPath;
        var probe = global.FFProbeBinaryPath;
        await Task.WhenAll(Enumerable.Range(0, 32).Select(index => Task.Run(() =>
        {
            var options = new FFOptions { FFMpegBinaryPath = $"encoder-{index}", FFProbeBinaryPath = $"probe-{index}" };
            Assert.AreEqual($"encoder-{index}", GlobalFFOptions.GetFFMpegBinaryPath(options));
            Assert.AreEqual($"probe-{index}", GlobalFFOptions.GetFFProbeBinaryPath(options));
        })));
        Assert.AreSame(global, GlobalFFOptions.Current);
        Assert.AreEqual(encoder, global.FFMpegBinaryPath);
        Assert.AreEqual(probe, global.FFProbeBinaryPath);
    }

    [TestMethod]
    public void Explicit_Paths_Win_Over_Existing_Architecture_Binaries()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"ffmpeg-paths-{Guid.NewGuid():N}");
        try
        {
            var architecture = Path.Combine(directory, Environment.Is64BitProcess ? "x64" : "x86");
            Directory.CreateDirectory(architecture);
            var extension = OperatingSystem.IsWindows() ? ".exe" : "";
            foreach (var name in new[] { "ffmpeg", "ffprobe" })
            {
                File.WriteAllText(Path.Combine(directory, name + extension), "");
                File.WriteAllText(Path.Combine(architecture, name + extension), "");
            }

            var options = new FFOptions { BinaryFolder = directory };
            Assert.AreEqual(Path.Combine(architecture, "ffmpeg" + extension), GlobalFFOptions.GetFFMpegBinaryPath(options));
            Assert.AreEqual(Path.Combine(architecture, "ffprobe" + extension), GlobalFFOptions.GetFFProbeBinaryPath(options));
            options.FFMpegBinaryPath = "explicit-encoder";
            options.FFProbeBinaryPath = "explicit-probe";
            Assert.AreEqual("explicit-encoder", GlobalFFOptions.GetFFMpegBinaryPath(options));
            Assert.AreEqual("explicit-probe", GlobalFFOptions.GetFFProbeBinaryPath(options));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
