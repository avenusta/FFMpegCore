---
name: create-release
description: >-
  Create a new Avenusta.FFMpegCore release. Use when the user says "create a new release": update version and release notes on main, commit and push, merge into release and push, then return to main.
---

# Create a new release

1. Inspect Git status and diffs; fetch `origin` and tags. Work on `main`, fast-forwarding from `origin/main` when needed. Never discard changes, force-push, or silently resolve divergent branches. Ask if any pending files are unrelated or unsafe to publish.
2. Use the version requested by the user; otherwise ask for the next version. It must be newer than the current published version.
3. Identify the last published **Avenusta.FFMpegCore** release and its tag/commit (not an upstream release). Review all changes since then, including pending changes. If the baseline is unclear, ask.
4. Update `<Version>` and `<PackageReleaseNotes>` in `FFMpegCore/FFMpegCore.csproj`. Write concise, user-facing notes covering changes since that release. Preserve package identity and extension versions.
5. Run `dotnet test FFMpegCore.sln` and `dotnet pack FFMpegCore/FFMpegCore.csproj -c Release`. If either fails, report the failure and ask before proceeding.
6. Review and stage the intended changes, including version and notes. Commit on `main` as `Release <version>`, with the release details in the commit body. Push with `git push origin main`; stop if it fails.
7. Switch to `release` (create it tracking `origin/release` if absent), fast-forward from `origin/release`, then merge `main`. Stop and ask on conflicts. Push with `git push origin release` only after a successful merge. This triggers `.github/workflows/release.yml`; do not publish manually or create tags separately.
8. Switch back to `main`. Report the version, commit, push results, and publishing workflow status if available. A successful push alone does not mean NuGet publishing succeeded.

On failure, stop subsequent publishing steps and explain what completed. Return to `main` when safe; never discard an unresolved merge to do so.
