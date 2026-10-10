namespace Firefall.Reconstruction.Startup;

public sealed record NativeModuleFilenameAttempt(
    uint BufferCodeUnits, uint? NativeReturnValue, uint? LastError);

public sealed record NativeModulePathSplitCapture(
    bool? NativeSucceeded, string? Drive, string? Directory, string? Filename, string? Extension);

public enum NativeModulePathOutcome
{
    CapturedPathReady,
    ModuleQueryFailed,
    SplitFailed,
    UnsupportedSelector,
    UnresolvedCapture
}

public sealed record NativeModulePathResult(
    NativeModulePathOutcome Outcome, string? SelectedText, int AttemptsConsumed, uint NextBufferCodeUnits)
{
    public bool? NativeReturnValue => Outcome switch
    {
        NativeModulePathOutcome.CapturedPathReady => true,
        NativeModulePathOutcome.UnresolvedCapture => null,
        NativeModulePathOutcome.ModuleQueryFailed or NativeModulePathOutcome.SplitFailed
            or NativeModulePathOutcome.UnsupportedSelector => false,
        _ => throw new InvalidOperationException("Unknown module-path outcome.")
    };

    public NativeFolderRootAttempt AsFolderRootCapture() => new(NativeReturnValue, SelectedText);
}

/// <summary>Captured GetModuleFileNameW/error/split-path results; no module-path query, CRT splitter or native API execution.</summary>
public static class NativeModulePathProvider
{
    public const uint MethodFileOffset = 0x126CBD0;
    public const uint SplitMethodFileOffset = 0x126D9E0;
    public const uint GetModuleFilenameSlotVirtualAddress = 0x1AC4260;
    public const uint GetLastErrorSlotVirtualAddress = 0x1AC4378;
    public const uint SplitPathSlotVirtualAddress = 0x1AC4AC0;
    public const uint InitialBufferCodeUnits = 260;
    public const uint InsufficientBufferError = 122;
    public const uint FolderAliasDirectorySelector = 3;

    public static uint NextBufferLength(uint previous) => unchecked(260 + 4 * previous);

    public static NativeModulePathResult Project(
        uint selector, IReadOnlyList<NativeModuleFilenameAttempt> attempts,
        string? capturedFullModulePath, NativeModulePathSplitCapture? split)
    {
        ArgumentNullException.ThrowIfNull(attempts);
        uint length = 0;
        for (int index = 0; ; index++)
        {
            length = NextBufferLength(length);
            if (index == attempts.Count)
                return new(NativeModulePathOutcome.UnresolvedCapture, null, index, length);
            var attempt = attempts[index];
            if (attempt.BufferCodeUnits != length)
                throw new ArgumentException("Captured module-path query capacity does not follow the native retry sequence.", nameof(attempts));
            if (attempt.LastError is null)
                return new(NativeModulePathOutcome.UnresolvedCapture, null, index + 1, length);
            if (attempt.LastError == InsufficientBufferError)
                continue;
            if (attempt.NativeReturnValue is null)
                return new(NativeModulePathOutcome.UnresolvedCapture, null, index + 1, length);
            if (attempt.NativeReturnValue == 0)
                return new(NativeModulePathOutcome.ModuleQueryFailed, null, index + 1, length);
            if (split?.NativeSucceeded is null)
                return new(NativeModulePathOutcome.UnresolvedCapture, null, index + 1, length);
            if (split.NativeSucceeded == false)
                return new(NativeModulePathOutcome.SplitFailed, null, index + 1, length);
            if (selector > 3)
                return new(NativeModulePathOutcome.UnsupportedSelector, null, index + 1, length);
            string? selected = selector switch
            {
                0 => capturedFullModulePath,
                1 => split.Filename,
                2 => split.Filename is { } filename && split.Extension is { } extension ? filename + extension : null,
                3 => split.Drive is { } drive && split.Directory is { } directory ? drive + directory : null,
                _ => throw new InvalidOperationException("Selector was already validated.")
            };
            return selected is null
                ? new(NativeModulePathOutcome.UnresolvedCapture, null, index + 1, length)
                : new(NativeModulePathOutcome.CapturedPathReady, selected, index + 1, length);
        }
    }
}
