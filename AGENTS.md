# Agent guide

- Fork of FFMpegCore, published as `Avenusta.FFMpegCore`. Preserve fork APIs and executable-path overrides when merging upstream.
- Build/test: `dotnet test FFMpegCore.sln` (requires `ffmpeg` and `ffprobe` on PATH).
- Lint: `dotnet format FFMpegCore.sln --severity warn --verify-no-changes`.
- Use existing C# style and add tests for behavior changes.
- Publishing uses `.github/workflows/release.yml` on the `release` branch; CI must not publish upstream packages.
