namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Static structural evidence at raw file offsets; local-name suffixes are not runtime pointers.
/// </summary>
public static class UiChildScratchCleanupNativeLayout
{
    public const uint CleanupEntryAddress = 0xCA6C80;
    public const uint InitialCallTargetAddress = 0x12BB2A0;
    public const uint ScratchInitializationAddress = 0xCA6D72;
    public const int ScratchInitializationByteCount = 0x40;

    public const uint LookupFailureTargetAddress = 0xCA6F50;
    public const uint DestinationReplacementLoopAddress = 0xCA6EF0;
    public const int CleanupPairCount = 8;
    public const int ScratchDwordCount = 16;
    public const int FirstScratchLocalNameSuffix = 0x58;
    public const int LastScratchLocalNameSuffix = 0x1C;
    public const int ScratchLocalNameSuffixStep = -4;
    public const int CleanupVirtualCallVtableSlotOffset = 0x08;

    public const uint FailureResultInstructionFileOffset = 0xCA6FCF;
    public const uint SuccessResultInstructionFileOffset = 0xCA6F49;
    public const uint CommonResultInstructionFileOffset = 0xCA7061;

    public const bool InitialCallFailureBypassesScratchCleanup = true;
    public const bool CleanupVirtualCallRequiresNonzeroPointer = true;
    public const bool ClearScratchPointerAfterVirtualCall = true;
    public const bool LookupFailurePathHasDirectDestinationWrites24Through60 = false;

    public static IReadOnlyList<uint> LookupCallGateAddresses { get; } =
        Array.AsReadOnly(new uint[]
        {
            0xCA6D8B,
            0xCA6DA9,
            0xCA6DD5,
            0xCA6DF3,
            0xCA6E11,
            0xCA6E2F,
            0xCA6E4D,
            0xCA6E6B,
            0xCA6E89,
            0xCA6EB5,
            0xCA6EE1
        });

    public static IReadOnlyList<int> ScratchLocalNameSuffixes { get; } =
        Array.AsReadOnly(new[]
        {
            0x58, 0x54, 0x50, 0x4C, 0x48, 0x44, 0x40, 0x3C,
            0x38, 0x34, 0x30, 0x2C, 0x28, 0x24, 0x20, 0x1C
        });
}
