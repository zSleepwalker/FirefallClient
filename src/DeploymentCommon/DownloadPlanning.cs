using System.Globalization;

namespace Firefall.Reconstruction.DeploymentCommon;

// Reconstructed from ThreadedFileDownloader / DownloadObject / FileHelper IL.
public sealed record DownloadChunk(uint PartNumber, long StartOffset, long EndOffset, string LocalChunkPath, string? PartMd5)
{
    public long Length => EndOffset - StartOffset + 1;
}

public static class DownloadPlanning
{
    public const long ChunkSize = 10 * 1024 * 1024;
    public const int MaxWorkers = 10;
    public const int MaxRetries = 10;

    // Part-md5 file: one "partNumber:md5" line per chunk (split on ':', UInt32.Parse + value).
    public static Dictionary<uint, string> ParsePartMd5(IEnumerable<string> lines)
    {
        var map = new Dictionary<uint, string>();
        foreach (var line in lines)
        {
            var f = line.Split(':');
            map.Add(uint.Parse(f[0], CultureInfo.InvariantCulture), f[1]);
        }
        return map;
    }

    // The first inclusive end uses totalSize, not totalSize - 1; preserve the native off-by-one.
    public static IReadOnlyList<DownloadChunk> PlanChunks(string localFilePath, long totalSize, IReadOnlyDictionary<uint, string> partMd5)
    {
        var list = new List<DownloadChunk>();
        uint part = 0;
        long start = 0;
        long end = Math.Min(totalSize, ChunkSize);
        while (start < totalSize)
        {
            partMd5.TryGetValue(part, out var md5);
            list.Add(new DownloadChunk(part, start, end, localFilePath + ".part" + part.ToString(CultureInfo.InvariantCulture), md5));
            part = unchecked(part + 1);
            start = unchecked(end + 1);
            end = Math.Min(unchecked(totalSize - 1), unchecked(end + ChunkSize));
        }
        return list;
    }

    public static int WorkerCount(int defaultConnectionLimit, int chunkCount) => Math.Min(MaxWorkers, Math.Min(defaultConnectionLimit, chunkCount));
}

public static class FileNaming
{
    // "{0}_{1}_{2}_{3}.{4}" = name(with / \ space -> _), product, build, patch level, extension (dots removed).
    public static string GenerateBuildSpecificFileName(string name, string extension, string product, string build, string patchLevel)
    {
        name = name.Replace('/', '_').Replace('\\', '_').Replace(' ', '_');
        extension = extension.Replace(".", "");
        return $"{name}_{product}_{build}_{patchLevel}.{extension}";
    }

    // FileHelper.GetMd5Hash: lowercase hex ("x2") of MD5 over the file bytes.
    public static string Md5Hex(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(System.Security.Cryptography.MD5.HashData(fs)).ToLowerInvariant();
    }
}
