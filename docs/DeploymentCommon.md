# DeploymentCommon.dll

- Path: `D:\SteamLibrary\steamapps\common\Firefall\system\bin\DeploymentCommon.dll`, 76,800 bytes
- SHA256: `33848E8CA80DFAD7B6883D5A57FC55834411E574CEE6467C753B77AD4523DAE0`
- Tooling note: it is a **managed .NET 2.0 assembly** (v1.0.0.0, WPF). IDA sees only metadata/IL (316 "functions", no native exports/imports, no native strings), so it is a poor fit. No ilspycmd/ildasm was installed. Types, methods, user strings and IL were read read-only with `System.Reflection.Metadata` (no assembly load, no execution). Nothing was written to the IDB.

## Observed
- References: mscorlib, System, WPF (PresentationFramework/Core, WindowsBase), System.Windows.Forms, System.Drawing, System.Management, **SevenZipSharp 0.64**, Interop.IWshRuntimeLibrary (shortcuts). No native exports.
- Namespaces/classes: `BuildInformation` (singleton, registry/ini/URL logic), `Downloads.{IDownloader, FileDownloader, ThreadedFileDownloader, ParallelFileDownloader, DownloadObject, ExceededRangeRequestRetryLimitException}`, `FileHelper` (MD5, download, 7z/LZMA compress/decompress, IsSameOrSubfolder), `IniFile` (kernel32 Get/WritePrivateProfile*), `JSON` (hand-written parser/serializer), `Logger` (file log + POST to operator events), `Utils` (mutex, launcher start, shortcuts, uninstaller registration, tracert, nslookup resolver identity, WMI UUID, icacls reset), `DialogHelper`, `Promotion`, WPF converters (`BoolNegater`, `BoolVisibility`, `Png`), `Wow` (IsWow64Process).
- Purpose: shared library of the Firefall installer/launcher/patcher (Red 5 Studios deployment).
- Version discovery: `BuildInformation.Instance` fetches JSON from `http://operator.firefall.com/api/v1/products/{ProductIdentifier}` (`Firefall`/`Firefall_Beta` strings present); requires keys `build`, `environment`, `region`, `patch_level` (number).
- Hosts/URLs: `dl-{env}.firefall.com`; installer `http://dl-{env}.firefall.com/installer/client/{env}/{region}/{build}[/{patchLevel}]` + `/FirefallInstaller.7z`, `.7z_partmd5`, `FirefallInstaller_md5`; patcher base `.../patcher/client/{env}/{region}/{build}[/{patchLevel}]` + `/patch_files`, `/FirefallPatchManifest.7z`, `_md5`, `/FirefallPatcher.7z`, `_md5`, and binary diffs `{base}/{instVer}r{instPatch}_{build}r{patch}.dfc` (+`_partmd5`, `_md5`). Patch-level `"0"` omits the trailing level segment (branch seen in IL). Events: `POST http://operator.firefall.com/api/v1/events`. Launcher links: firefall.com community/launcher/player_guides/patch_notes; banner JSON uses `promotions` with `image_url`, `link_url`, `title`, `body`.
- Hashing: MD5 (MD5CryptoServiceProvider over FileStream, lowercase `x2` hex); compared case-insensitively (`String.Equals` mode 3 = ordinal ignore case) against expected `_md5`/part MD5. Part-md5 file: lines `partNumber:md5`.
- Download: HEAD for Content-Length; resumes with `Range: bytes=a-b`; chunks of 10 MiB (10485760), files `<local>.part<N>`; workers = min(10, ServicePointManager.DefaultConnectionLimit, chunks); 10 retries per range/MD5 failure; progress timer 2000 ms; chunk reuse when size and MD5 match; reassembly by concatenating parts then deleting them. User agent `Firefall DL Stream ({OS})`; CloudFront headers `x-edge-request-id`, `x-amz-cf-id` logged.
- Registry/ini: `HKLM\Software\Red 5 Studios\{product}\` values InstalledVersion, InstalledPatchLevel, InstallLocation, SettingsDirectory, TempDownloadDirectory, RcaInstalled, DirectXInstalled, CppRedistX86/X64Installed, XiphCodecsInstalled. Uninstall entry under `SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall` (GUID `{CFEF8DB5-B45E-4b05-90BE-D02AA6F45354}` present, support_us@red5studios.com). `firefall.ini`: `[FilePaths] WritePath`, `VGLocalPath` = `system/vg/static.vgeo`, `AssetStreamPath` cleared. Client exe `system\bin\FirefallClient.exe`; default cache dir = settings dir + `cache`. Extras: DirectX web setup, opencodecs_0.85.17777.exe, `icacls /RESET /T /Q /C`.

## Interpreted
- `.dfc` is a binary-diff (delta) container fetched when an install exists; the patcher uses a manifest (7z) + `patch_files` for per-file patching. Format not examined.
- First-chunk end clamp in `DoFileDownload` was not fully isolated; reconstruction uses inclusive `[start, min(total-1,start+10MiB-1)]` (`DownloadObject` length = end-start+1 supports inclusive ranges).

## Unresolved
- Real runtime values (env/region names), `.dfc`/manifest/part-md5 file internals beyond line format, uninstaller/shortcut/Utils bodies, LZMA/7z extraction semantics, JSON parser details (hand-written, standard), WMI/traceroute usage specifics.

## Reconstruction
`src\DeploymentCommon\` (`BuildInformation.cs` URL/JSON model, `DownloadPlanning.cs` chunking/part-md5/file naming/MD5). Built with `dotnet build Firefall.Reconstruction.slnx -v:q` (0 warnings/errors). No tests added.
