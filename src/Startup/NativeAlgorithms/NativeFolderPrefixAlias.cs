using System.Globalization;

namespace Firefall.Reconstruction.Startup;

public sealed record NativeFolderRootAttempt(bool? NativeAvailable, string? CapturedRoot);

public enum NativeFolderAliasOutcome
{
    AliasMatched,
    OriginalTextFallback,
    UnresolvedRootCapture,
    UnresolvedUppercaseCapture
}

public sealed record NativeFolderAliasResult(
    NativeFolderAliasOutcome Outcome, string? TextForLowercaseStage, uint? FolderId)
{
    public bool? NativeReturnValue => Outcome switch
    {
        NativeFolderAliasOutcome.AliasMatched => true,
        NativeFolderAliasOutcome.OriginalTextFallback => false,
        NativeFolderAliasOutcome.UnresolvedRootCapture or NativeFolderAliasOutcome.UnresolvedUppercaseCapture => null,
        _ => throw new InvalidOperationException("Unknown folder-alias outcome.")
    };
}

/// <summary>Captured-provider folder prefix substitution; no temporary/shell/application path queries or locale calls.</summary>
public static class NativeFolderPrefixAlias
{
    public const uint MethodFileOffset = 0x126DAC0;
    public const uint RemoveTrailingSeparatorFileOffset = 0x126D750;
    public const uint RemoveExtendedPrefixFileOffset = 0x126D7D0;
    public const uint PrefixCompareFileOffset = 0x1270D40;
    public const uint ExtendedPrefixInitializerFileOffset = 0x2E33D0;
    public const uint ExtendedPrefixLiteralVirtualAddress = 0x1C7EA48;
    public const uint FolderOrderVirtualAddress = 0x1C0E16C;
    public const string ExtendedPrefix = @"\\?\";
    private static readonly uint[] folderOrder = [254, 255, 37, 41, 36, 38, 42, 5, 28, 26, 35, 0, 25, 40, 7];
    public static ReadOnlySpan<uint> FolderOrder => folderOrder;

    public static string RemoveOneTrailingSeparator(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return text.Length != 0 && text[^1] is '\\' or '/' ? text[..^1] : text;
    }

    public static NativeFolderAliasResult Project(
        string originalText, IReadOnlyDictionary<uint, NativeFolderRootAttempt> capturedRoots,
        Func<char, char?> capturedTowupper)
    {
        ArgumentNullException.ThrowIfNull(originalText);
        ArgumentNullException.ThrowIfNull(capturedRoots);
        ArgumentNullException.ThrowIfNull(capturedTowupper);
        if (originalText.Length == 0)
            return new(NativeFolderAliasOutcome.OriginalTextFallback, originalText, null);
        string candidate = RemoveOneTrailingSeparator(originalText);
        if (candidate.StartsWith(ExtendedPrefix, StringComparison.Ordinal))
            candidate = candidate[ExtendedPrefix.Length..];
        foreach (uint id in folderOrder)
        {
            if (!capturedRoots.TryGetValue(id, out var rootAttempt) || rootAttempt.NativeAvailable is null)
                return new(NativeFolderAliasOutcome.UnresolvedRootCapture, null, id);
            if (rootAttempt.NativeAvailable == false)
                continue;
            if (rootAttempt.CapturedRoot is null)
                return new(NativeFolderAliasOutcome.UnresolvedRootCapture, null, id);
            string root = RemoveOneTrailingSeparator(rootAttempt.CapturedRoot);
            if (root.Length == 0 || candidate.Length < root.Length)
                continue;
            bool match = true;
            for (int i = 0; i < root.Length; i++)
            {
                if (candidate[i] == root[i])
                    continue;
                char? rootUpper = capturedTowupper(root[i]), candidateUpper = capturedTowupper(candidate[i]);
                if (rootUpper is null || candidateUpper is null)
                    return new(NativeFolderAliasOutcome.UnresolvedUppercaseCapture, null, id);
                if (rootUpper != candidateUpper)
                {
                    match = false;
                    break;
                }
            }
            if (!match || candidate.Length > root.Length && candidate[root.Length] != '\\')
                continue;
            int remainderStart = root.Length + (candidate.Length > root.Length ? 1 : 0);
            string token = "<" + id.ToString(CultureInfo.InvariantCulture) + ">";
            return new(NativeFolderAliasOutcome.AliasMatched, token + candidate[remainderStart..], id);
        }
        return new(NativeFolderAliasOutcome.OriginalTextFallback, originalText, null);
    }

    /// <summary>Explicit ASCII/C-locale snapshot only; non-ASCII case mapping remains unobserved.</summary>
    public static char? AsciiUppercaseCapture(char value) =>
        value > 0x7F ? null : value is >= 'a' and <= 'z' ? (char)(value - 'a' + 'A') : value;
}
