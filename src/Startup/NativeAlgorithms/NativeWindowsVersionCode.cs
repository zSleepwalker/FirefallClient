namespace Firefall.Reconstruction.Startup;

public sealed record NativeWindowsVersionCapture(
    bool? ApiSucceeded, uint MajorVersion, uint MinorVersion, ushort ServicePackMajor);

/// <summary>Projects captured GetVersionExW fields; does not query the host or infer its actual OS.</summary>
public static class NativeWindowsVersionCode
{
    public const uint MethodFileOffset = 0x12800F0;
    public const uint GetVersionExSlotVirtualAddress = 0x1AC42E8;
    public const uint VersionInfoSize = 284;

    public static uint? Project(NativeWindowsVersionCapture capture, bool ignoreServicePack = false)
    {
        ArgumentNullException.ThrowIfNull(capture);
        if (capture.ApiSucceeded is null)
            return null;
        if (capture.ApiSucceeded == false)
            return 0;
        uint packed = unchecked(((capture.MajorVersion << 8) | capture.MinorVersion) << 8)
            | (ignoreServicePack ? 0u : capture.ServicePackMajor);
        return packed switch
        {
            0x50000 => 1,
            0x50004 => 2,
            0x50100 => 3,
            0x50101 => 4,
            0x50102 => 5,
            0x50103 => 6,
            0x60000 => 7,
            0x60001 => 8,
            0x60002 => 9,
            0x60100 => 10,
            0x60101 => 11,
            0x60200 => 12,
            > 0x60200 => 13,
            _ => 0
        };
    }
}
