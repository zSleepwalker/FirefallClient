using System.Numerics;

namespace Firefall.Reconstruction.HWStats;

public readonly record struct RamLineArguments(
    string NativeFormat, uint? IntegerLowDword, uint? IntegerHighDword, double? FloatingArgument);

/// <summary>Observed arguments and mask decisions only; never queries host memory or affinity.</summary>
public static class RamAndProcessorNativeLayout
{
    public const uint SectionVa = 0x4035A0;
    public const int MemoryStatusExByteCount = 64;
    public const int TotalPhysicalMemoryOffset = 8;
    public const string SmallRamFormat = "ram = %ull MB\n";
    public const string GigabytesRamFormat = "ram = %.2f GB\n";
    public const string ProcessorCountKey = "nprocessors";
    public const bool ChecksMemoryApiResult = false;
    public const bool ChecksAffinityApiResult = false;

    public static RamLineArguments CaptureRamArguments(ulong observedTotalPhysicalBytes)
    {
        uint wholeMegabytesLowDword = unchecked((uint)(observedTotalPhysicalBytes >> 20));
        if (wholeMegabytesLowDword < 1024)
            return new(SmallRamFormat, unchecked((uint)observedTotalPhysicalBytes),
                (uint)(observedTotalPhysicalBytes >> 32), null);
        float unsignedMegabytes = wholeMegabytesLowDword;
        float gigabytes = unsignedMegabytes * (1f / 1024);
        return new(GigabytesRamFormat, null, null, gigabytes);
    }

    public static int CaptureProcessorCount(uint observedProcessAffinityMask, uint observedSystemAffinityMask)
    {
        _ = observedProcessAffinityMask;
        return BitOperations.PopCount(observedSystemAffinityMask);
    }
}
