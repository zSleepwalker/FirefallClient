namespace Firefall.Reconstruction.Awesomium;

public readonly record struct ChildProcessRoleEntry(string Key, uint KeyVa, uint CallbackVa);

public enum ChildProcessRouteKind
{
    ImmediateReturn,
    KnownCallback,
    DelegateFallback
}

public readonly record struct ChildProcessRoute(
    ChildProcessRouteKind Kind, uint? CallbackVa, int? ImmediateReturnValue);

/// <summary>Static entry/dispatch decisions only; never loads or calls the browser DLL.</summary>
public static class ChildProcessRoleNativeLayout
{
    public const string CppEntryExport = "?ChildProcessMain@Awesomium@@YAHPAUHINSTANCE__@@@Z";
    public const string CWrapperExport = "_Awe_ChildProcessMain@4";
    public const uint CppEntryRva = 0x3A710;
    public const uint CWrapperRva = 0x4C840;
    public const uint DispatcherVa = 0x10064940;
    public const uint RoleTableVa = 0x1124D400;
    public const int RoleEntryByteCount = 8;
    public const int RoleKeyPointerOffset = 0;
    public const int RoleCallbackPointerOffset = 4;
    public const int DelegateDispatchVtableOffset = 0x10;

    public static IReadOnlyList<ChildProcessRoleEntry> Roles { get; } =
        Array.AsReadOnly(new ChildProcessRoleEntry[]
        {
            new("", 0x11240E81, 0x100AF940),
            new("renderer", 0x11265E38, 0x101BCD70),
            new("plugin", 0x11265CEC, 0x1019E1D0),
            new("worker", 0x11265FBC, 0x1020A320),
            new("ppapi", 0x11265D88, 0x101A4FA0),
            new("ppapi-broker", 0x11265D0C, 0x101A5190),
            new("utility", 0x11265F34, 0x10209660),
            new("gpu-process", 0x11265BB4, 0x1019B1C0)
        });

    public static ChildProcessRoute CaptureExportEntry(string? parsedType, bool hasDelegate) =>
        string.IsNullOrEmpty(parsedType)
            ? new(ChildProcessRouteKind.ImmediateReturn, null, 0)
            : CaptureContentDispatch(parsedType, hasDelegate);

    /// <summary>The content dispatcher has an empty-key handler that the exported entry gate excludes.</summary>
    public static ChildProcessRoute CaptureContentDispatch(string type, bool hasDelegate)
    {
        ArgumentNullException.ThrowIfNull(type);
        foreach (var role in Roles)
            if (string.Equals(type, role.Key, StringComparison.Ordinal))
                return new(ChildProcessRouteKind.KnownCallback, role.CallbackVa, null);
        return hasDelegate
            ? new(ChildProcessRouteKind.DelegateFallback, null, null)
            : new(ChildProcessRouteKind.ImmediateReturn, null, 1);
    }
}
