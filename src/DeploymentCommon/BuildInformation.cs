using System.Globalization;

namespace Firefall.Reconstruction.DeploymentCommon;

// Reconstructed from DeploymentCommon.dll (.NET 2.0 assembly) BuildInformation IL.
public sealed record BuildInformation(string BuildIdentifier, string DeploymentEnvironmentName, string DeploymentRegionName, string PatchLevel)
{
    public const string OperatorHost = "operator.firefall.com";
    public const string ProductsUrlFormat = "http://{0}/api/v1/products/{1}";
    public const string EventsUrl = "http://operator.firefall.com/api/v1/events";

    public string DownloadHost => $"dl-{DeploymentEnvironmentName}.firefall.com";

    private bool IsBasePatchLevel => PatchLevel == "0";

    public string InstallerBaseUrl => IsBasePatchLevel
        ? $"http://dl-{DeploymentEnvironmentName}.firefall.com/installer/client/{DeploymentEnvironmentName}/{DeploymentRegionName}/{BuildIdentifier}"
        : $"http://dl-{DeploymentEnvironmentName}.firefall.com/installer/client/{DeploymentEnvironmentName}/{DeploymentRegionName}/{BuildIdentifier}/{PatchLevel}";

    public string PatchBaseUrl => IsBasePatchLevel
        ? $"http://dl-{DeploymentEnvironmentName}.firefall.com/patcher/client/{DeploymentEnvironmentName}/{DeploymentRegionName}/{BuildIdentifier}"
        : $"http://dl-{DeploymentEnvironmentName}.firefall.com/patcher/client/{DeploymentEnvironmentName}/{DeploymentRegionName}/{BuildIdentifier}/{PatchLevel}";

    public string InstallerUrl => InstallerBaseUrl + "/FirefallInstaller.7z";
    public string InstallerPartMd5Url => InstallerBaseUrl + "/FirefallInstaller.7z_partmd5";
    public string InstallerMd5Url => InstallerBaseUrl + "/FirefallInstaller_md5";
    public string PatchFilesUrl => PatchBaseUrl + "/patch_files";
    public string ManifestUrl => PatchBaseUrl + "/FirefallPatchManifest.7z";
    public string ManifestMd5Url => PatchBaseUrl + "/FirefallPatchManifest_md5";
    public string PatcherUrl => PatchBaseUrl + "/FirefallPatcher.7z";
    public string PatcherMd5Url => PatchBaseUrl + "/FirefallPatcher_md5";

    // "{base}/{installedVersion}r{installedPatch}_{build}r{patch}.dfc" (+ _partmd5 / _md5 siblings).
    public string BinaryDiffUrl(string installedVersion, string installedPatchLevel) =>
        string.Format(CultureInfo.InvariantCulture, "{0}/{1}r{2}_{3}r{4}.dfc", PatchBaseUrl, installedVersion, installedPatchLevel, BuildIdentifier, PatchLevel);

    public string BinaryDiffPartMd5Url(string v, string p) => BinaryDiffUrl(v, p) + "_partmd5";
    public string BinaryDiffMd5Url(string v, string p) => BinaryDiffUrl(v, p) + "_md5";

    // JSON from /api/v1/products/{id}: build, environment, region (strings), patch_level (JSON number -> double.ToString).
    public static BuildInformation FromOperatorJson(IReadOnlyDictionary<string, object?> json)
    {
        string Str(string key, string missing) => json.TryGetValue(key, out var v) && v is not null ? (string)v : throw new InvalidDataException(missing);
        var build = Str("build", "Missing build number in versioning server response.");
        var env = Str("environment", "Missing environment information in versioning server response.");
        var region = Str("region", "Missing region information in versioning server response.");
        if (!json.TryGetValue("patch_level", out var pl) || pl is null)
            throw new InvalidDataException("Missing patch level information in versioning server response.");
        return new BuildInformation(build, env, region, Convert.ToDouble(pl, CultureInfo.InvariantCulture).ToString(CultureInfo.CurrentCulture));
    }

    public static string InstallRegistryKeyPath(string productIdentifier) => $@"Software\Red 5 Studios\{productIdentifier}\";
}
