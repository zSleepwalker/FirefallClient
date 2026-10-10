using System.Globalization;
using System.Text;

namespace Firefall.Scripting.LuaCache;

/// <summary>
/// Cache-key derivation and freshness rules for the UI Lua bytecode cache.
/// Raw offsets refer to FirefallClient.exe file offsets (IDA raw-mapped database).
/// </summary>
public static class LuaCacheKey
{
    /// <summary>Format literal at raw 0x17BC43C (VA 0x1BBD83C): <c>cache\gui\lua_%016llx.bytecode</c>.</summary>
    public const string RelativePathFormat = @"cache\gui\lua_{0:x16}.bytecode";

    /// <summary>Console/config variable name at raw 0x17BC424 (VA 0x1BBD824). Non-zero value disables the cache.</summary>
    public const string DisableCacheVariable = "ui.noLuaByteCodeCache";

    public const ulong FnvOffsetBasis = 0xCBF29CE484222325UL; // raw 0x12F17E3 (lo) / 0x12F17E9 (hi)
    public const ulong FnvPrime = 0x00000100000001B3UL;       // raw 0x12F17FA/0x12F1800 pushed to _allmul (raw 0x159DC30)

    /// <summary>Tolerated mtime skew (seconds) in raw 0xC53570: equal, or differing by exactly 0xE10.</summary>
    public const long ToleratedSkewSeconds = 3600;

    /// <summary>
    /// raw 0x12F17E0: 64-bit FNV-1a over the NUL-terminated source path bytes exactly as passed
    /// in ECX (no case folding or slash normalisation in this function or in raw 0xC3BCD0).
    /// Bytes are zero-extended (movzx + cdq on a non-negative value), so high dword XOR is 0.
    /// </summary>
    public static ulong HashSourcePath(ReadOnlySpan<byte> sourcePathBytes)
    {
        ulong h = FnvOffsetBasis;
        foreach (byte c in sourcePathBytes)
        {
            if (c == 0) break;
            h = (h ^ c) * FnvPrime;
        }
        return h;
    }

    /// <summary>The client passes UTF-8 C strings (later converted with MultiByteToWideChar(CP_UTF8=0xFDE9)).</summary>
    public static ulong HashSourcePath(string sourcePath) => HashSourcePath(Encoding.UTF8.GetBytes(sourcePath));

    /// <summary>
    /// Relative cache path produced by VIBE_FormatSharedText (raw 0x12B9230) at raw 0xC3BD5D.
    /// The result is then composed onto VIBE_GetCachedComposedBasePath() (raw 0x12BBD80) via
    /// VIBE_ComposeSharedTextPath (raw 0x12BBEC0); the base directory is not recovered here.
    /// </summary>
    public static string GetRelativeCachePath(ulong key) =>
        string.Format(CultureInfo.InvariantCulture, RelativePathFormat, key);

    public static string GetRelativeCachePath(string sourcePath) => GetRelativeCachePath(HashSourcePath(sourcePath));

    /// <summary>
    /// raw 0xC3BDBF..0xC3BDF9: cache is used only when the cache file mtime (from _wstat64,
    /// 0 on failure) is non-zero and <see cref="MtimesMatch"/> holds against the source mtime.
    /// </summary>
    public static bool IsCacheFresh(long sourceMtime, long cacheMtime) =>
        cacheMtime != 0 && MtimesMatch(cacheMtime, sourceMtime);

    /// <summary>raw 0xC53570 (cdecl, two signed 64-bit values).</summary>
    public static bool MtimesMatch(long a, long b) =>
        a == b || a - b == ToleratedSkewSeconds || b - a == ToleratedSkewSeconds;
}
