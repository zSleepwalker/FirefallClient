namespace Firefall.Reconstruction.Startup;

public static class MatrixAnnouncementNativeLayout
{
    public const uint HandlerFileOffset = 0x491400;
    public const uint DisplayConsumerFileOffset = 0x6E3460;
    public const uint TypeTextVirtualAddress = 0x1B51DFC;
    public const uint DisplayLabelVirtualAddress = 0x1B51E04;
    public const string MatchedType = "sysmsg";
    public const string DisplayLabel = "broadcast";

    public static bool SelectDisplay(ReadOnlySpan<byte> decodedTypeBytes, bool modePresent) =>
        modePresent && decodedTypeBytes.SequenceEqual("sysmsg"u8);
}
