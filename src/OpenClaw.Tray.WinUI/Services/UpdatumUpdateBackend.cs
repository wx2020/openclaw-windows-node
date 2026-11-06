using OpenClaw.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Updatum;

namespace OpenClawTray.Services;

internal sealed class UpdatumUpdateBackend : IUpdateBackend
{
    // This fork ships its own Chinese build. Pointing the updater at the upstream
    // repository would let an official (English) release replace it, so updates
    // come from the fork's releases instead.
    private const string UpdateRepositoryOwner = "wx2020";
    private const string UpdateRepositoryName = "openclaw-windows-node";

    private readonly UpdatumManager _updater = new(UpdateRepositoryOwner, UpdateRepositoryName)
    {
        FetchOnlyLatestRelease = true,
        InstallUpdateSingleFileExecutableName = "OpenClaw.Tray.WinUI",
    };

    public Task<bool> CheckForUpdatesAsync() => _updater.CheckForUpdatesAsync();

    public string? GetChangelog() => _updater.GetChangelog(true);

    public async Task<bool> DownloadAndInstallAsync(Action downloadCompleted)
    {
        var downloadedAsset = await _updater.DownloadUpdateAsync();
        downloadCompleted();
        if (downloadedAsset == null || !File.Exists(downloadedAsset.FilePath))
        {
            Logger.Error("Update download failed or file missing");
            return false;
        }

        Logger.Info("Installing update and restarting...");
        await _updater.InstallUpdateAsync(downloadedAsset);
        return true;
    }

    public UpdateReleaseCandidate? GetSelectedRelease() =>
        _updater.LatestRelease is { } release
            ? CreateCandidate(release)
            : null;

    public IEnumerable<UpdateReleaseCandidate> GetReleaseCandidates()
    {
        foreach (var release in _updater.Releases)
            yield return CreateCandidate(release);
    }

    private UpdateReleaseCandidate CreateCandidate(Octokit.Release release) => new(
        release.TagName,
        release.Body,
        HasTrustedMetadata: true,
        release.Draft,
        release.Prerelease,
        release.PublishedAt is not null,
        () => _updater.GetCompatibleReleaseAsset(release) is not null,
        () => _updater.ForceTriggerUpdateFromRelease(release));
}
