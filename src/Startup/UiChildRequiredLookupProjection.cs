namespace Firefall.Reconstruction.Startup;

public static class UiChildRequiredLookupProjectionRules
{
    public const uint FirstLookupHelperRawFileOffset = 0xCA7090;
    public const int FirstLookupDependencyVirtualSlotOffset = 0x16C;
    public const uint SecondLookupHelperRawFileOffset = 0xCA71F0;
    public const int SecondLookupDependencyVirtualSlotOffset = 0x1A8;

    // Applies only after reaching the final dispatcher/output test, not the earlier lookup stages.
    public static bool IsRequiredLookupSatisfied(
        int capturedDispatcherStatus,
        uint capturedOutputPointer)
        => capturedDispatcherStatus >= 0 && capturedOutputPointer != 0;
}
