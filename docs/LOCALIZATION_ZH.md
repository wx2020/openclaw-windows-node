# Simplified Chinese (zh-cn) build

This fork ships a Simplified Chinese build of OpenClaw Companion. It is built
from `v2026.10.8` on branch `zh-cn` and only touches localization and the
update channel; everything else tracks upstream.

## What changed against upstream

| Area | Change |
|---|---|
| `Strings/zh-cn/Resources.resw` | 529 entries that were still seeded with en-US copy are translated (WorkspaceShell, DiagnosticsPage, V2 onboarding, PermissionsPage, InstancesPage, CommandCenter, SettingsRow, remaining XAML keys). 34 technical values (time zones, OTLP protocol names, sample hosts, brands) intentionally stay English. |
| `LocalizationValidationTests` | The all-or-none rule now allows the Chinese locales to lead: a key translated in `zh-cn`/`zh-tw` while the Latin-script locales still carry the en-US fallback is a rollout order, not a partial translation. |
| `ChannelsPage` | Channel badge text is no longer the status token from the gateway payload. `ResolveHeaderState` returns a `ChannelBadge` enum and the label comes from `ChannelsPage_Badge_*` resources, so translating the badge can never change how a channel is classified. Channel card copy (counts, probe/event lines, reconnect attempts, save banners) uses resw format strings. |
| `LocalizationHelper` | Added `GetString(key, fallback)` so composed keys cannot render as a raw resource key when a resource is missing. |
| `UpdatumUpdateBackend` / `UpdateBackendLoader` | Update checks and the manual release link point at this fork, so an upstream release cannot replace the Chinese build. |

## Building locally (Windows 10 21H2 / Windows 11)

Requirements: .NET SDK 10.0.400, a NuGet feed, and the Windows App SDK packages.

```bash
dotnet restore src/OpenClaw.Tray.WinUI/OpenClaw.Tray.WinUI.csproj -r win-x64
dotnet build   src/OpenClaw.Tray.WinUI/OpenClaw.Tray.WinUI.csproj -c Release -r win-x64 --no-restore
dotnet test    tests/OpenClaw.Tray.Tests/OpenClaw.Tray.Tests.csproj -c Release --filter "FullyQualifiedName~Localization"
```

Notes specific to this machine:

- `api.nuget.org` is truncated by the local proxy. Use a mirror, for example
  `dotnet restore -p:RestoreConfigFile=.tmp/zh-cn/NuGet.Config` with the Huawei
  Cloud `nuget-remote` v3 index.
- On Windows 10 21H2 the .NET 10 SDK's `csc.exe` apphost aborts with
  `Your Windows doesn't fully support CET`. Clear the `IMAGE_DEBUG_TYPE_EX_DLLCHARACTERISTICS`
  CET bit on `sdk/10.0.400/Roslyn/bincore/csc.exe` and `VBCSCompiler.exe`
  (backups as `*.bak-cet`). The built application itself is unaffected.
- `build.ps1` refuses to run without a Visual Studio install (app-local VC
  redistributable). Build the project directly, or run the `zh-cn build`
  workflow on GitHub Actions where the runner has VS.

## Verification

| Check | Command |
|---|---|
| All-or-none, key parity, placeholders, mojibake | `dotnet test ... --filter "FullyQualifiedName~Localization"` |
| Candidate hard-coded XAML text | `powershell -File scripts/Test-Localization.ps1 -StrictHardcodedXaml -SkipDotNetTests` |
| PRI actually carries the translations | `makepri dump /if <pri> /of dump.xml /o`, then read the `Language-ZH-CN` candidate of any key |

## Rebasing onto a new upstream release

1. `git fetch origin --tags`
2. `git rebase origin/vX.Y.Z` (or merge that tag into `zh-cn`).
3. Resolve `Resources.resw` conflicts by keeping the upstream key set and
   re-applying this branch's values; conflicts are almost always append-only.
4. If upstream added new keys, run
   `python .tmp/zh-cn/import_tm.py --tm-dir <old patch project>` to see which
   new entries can be filled from the legacy translation memory.
5. Re-run the verification commands above and push.

## Known remaining work

- Remaining hard-coded UI copy in `ChannelsPage` (banners, taglines, state
  subtitles), `CronPage` (`next in ...`), `ConnectionPagePlan` (`up ...`) and
  `SettingsPage` has not been moved to resw yet.
- `Settings`, `Usage`, `About` in the workspace owner menu are still English:
  the label doubles as the `WorkspaceShell_` key suffix. They can now be
  translated safely because only the resource value changes, but the upstream
  key set should be reviewed before doing so.
- The SetupEngine wizard reads the Tray assembly's resource map, so its copy
  follows the same resw files and needs no separate translation.
