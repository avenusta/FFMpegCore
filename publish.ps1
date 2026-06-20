param(
    [Parameter(Mandatory)]
    [string]$Version
)

$repo = "avenusta/FFMpegCore"
$ErrorActionPreference = "Stop"

# Commit and tag on main
git checkout main
git tag "v$Version"
git push origin main
git push origin "v$Version"

# Merge main into release and push (triggers NuGet workflow)
git checkout release
git merge main --no-ff -m "Merge branch 'main' into release"
git push origin release

# Create GitHub release
gh release create "v$Version" --repo $repo --title "v$Version" --notes "Release v$Version"

# Trigger NuGet deployment and watch it
gh workflow run release.yml --repo $repo --ref release
Start-Sleep -Seconds 6
$runId = gh run list --repo $repo --workflow release.yml --limit 1 --json databaseId --jq ".[0].databaseId"
gh run watch $runId --repo $repo
