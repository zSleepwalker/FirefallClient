namespace Firefall.Reconstruction.HWStats;

public readonly record struct DriveSpaceAmounts(float Megabytes, float Gigabytes);

public enum DriveSpaceDisplayUnit
{
    None,
    Megabytes,
    Gigabytes
}

/// <summary>Observed numeric/output decisions only; never probes drives or disk space.</summary>
public static class DriveSpaceNativeLayout
{
    public const uint SectionVa = 0x4037F0;
    public const uint AvailableSpaceHelperVa = 0x403690;
    public const uint FixedDriveType = 3;
    public const char FirstDrive = 'C';
    public const char LastDrive = 'Z';
    public const string Header = "DriveSpace";
    public const string DriveGigabytesFormat = "%c = %.3f GB\n";
    public const string DriveMegabytesFormat = "%c = %.3f MB\n";
    public const string TotalGigabytesFormat = "total = %.3f GB\n";
    public const string TotalMegabytesFormat = "total = %.3f MB\n";

    public static bool CaptureDriveAdmission(uint nativeDriveType) => nativeDriveType == FixedDriveType;

    public static char CaptureHelperDriveLetter(byte candidate) =>
        candidate is >= (byte)'C' and <= (byte)'Z' or >= (byte)'c' and <= (byte)'z'
            ? (char)candidate : 'C';

    /// <summary>apiSucceeded is GetDiskFreeSpaceExW's BOOL, not the helper's clobbered EAX return.</summary>
    public static DriveSpaceAmounts CaptureAvailableSpace(bool apiSucceeded, ulong availableBytesToCaller)
    {
        if (!apiSucceeded)
            return new(0, 0);
        uint low = unchecked((uint)availableBytesToCaller);
        uint high = (uint)(availableBytesToCaller >> 32);
        uint shiftedLow = (low >> 20) | (high << 12);
        float fractionalMegabytes = (float)(low & 0xFFFFF) * (1f / 1048576);
        float megabytes = (float)unchecked((int)shiftedLow) + fractionalMegabytes;
        // Preserve the extra high-part term, rather than replacing native arithmetic with bytes/2^30.
        float scaledMegabytes = megabytes * (1f / 1024);
        float gigabytes = (float)(high >> 8) + scaledMegabytes;
        return new(megabytes, gigabytes);
    }

    public static DriveSpaceAmounts CaptureAccumulation(DriveSpaceAmounts running, DriveSpaceAmounts current) =>
        new(running.Megabytes + current.Megabytes, running.Gigabytes + current.Gigabytes);

    public static DriveSpaceDisplayUnit CaptureDisplayUnit(DriveSpaceAmounts amounts) =>
        amounts.Gigabytes > 1f ? DriveSpaceDisplayUnit.Gigabytes
        : amounts.Megabytes > 1f ? DriveSpaceDisplayUnit.Megabytes
        : DriveSpaceDisplayUnit.None;
}
