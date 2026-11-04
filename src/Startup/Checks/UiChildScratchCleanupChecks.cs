using Firefall.Reconstruction.Startup;

internal static class UiChildScratchCleanupChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("UI child scratch cleanup anchors the initial call and scratch initialization without assigning call semantics",
            UiChildScratchCleanupNativeLayout.CleanupEntryAddress == 0xCA6C80
            && UiChildScratchCleanupNativeLayout.InitialCallTargetAddress == 0x12BB2A0
            && UiChildScratchCleanupNativeLayout.InitialCallFailureBypassesScratchCleanup
            && UiChildScratchCleanupNativeLayout.ScratchInitializationAddress == 0xCA6D72
            && UiChildScratchCleanupNativeLayout.ScratchInitializationByteCount == 0x40);

        check("UI child scratch cleanup records all eleven lookup call gates in order",
            UiChildScratchCleanupNativeLayout.LookupCallGateAddresses.SequenceEqual(new uint[]
            {
                0xCA6D8B, 0xCA6DA9, 0xCA6DD5, 0xCA6DF3, 0xCA6E11, 0xCA6E2F,
                0xCA6E4D, 0xCA6E6B, 0xCA6E89, 0xCA6EB5, 0xCA6EE1
            }));

        check("UI child lookup failure reaches the separate eight-pair cleanup boundary",
            UiChildScratchCleanupNativeLayout.LookupFailureTargetAddress == 0xCA6F50
            && UiChildScratchCleanupNativeLayout.DestinationReplacementLoopAddress == 0xCA6EF0
            && UiChildScratchCleanupNativeLayout.CleanupPairCount == 8
            && UiChildScratchCleanupNativeLayout.ScratchDwordCount == 16);

        check("UI child cleanup records scratch local-name suffixes without assigning runtime pointer values",
            UiChildScratchCleanupNativeLayout.FirstScratchLocalNameSuffix == 0x58
            && UiChildScratchCleanupNativeLayout.LastScratchLocalNameSuffix == 0x1C
            && UiChildScratchCleanupNativeLayout.ScratchLocalNameSuffixStep == -4
            && UiChildScratchCleanupNativeLayout.ScratchLocalNameSuffixes.Count == 16
            && UiChildScratchCleanupNativeLayout.ScratchLocalNameSuffixes.SequenceEqual(new[]
            {
                0x58, 0x54, 0x50, 0x4C, 0x48, 0x44, 0x40, 0x3C,
                0x38, 0x34, 0x30, 0x2C, 0x28, 0x24, 0x20, 0x1C
            }));

        check("UI child cleanup conditionally calls vtable slot +8 for nonzero pointers and then clears them",
            UiChildScratchCleanupNativeLayout.CleanupVirtualCallRequiresNonzeroPointer
            && UiChildScratchCleanupNativeLayout.CleanupVirtualCallVtableSlotOffset == 0x08
            && UiChildScratchCleanupNativeLayout.ClearScratchPointerAfterVirtualCall);

        check("UI child lookup failure has no direct destination writes while called side effects remain unresolved",
            !UiChildScratchCleanupNativeLayout.LookupFailurePathHasDirectDestinationWrites24Through60);

        check("UI child failure, success, and common result instruction sites remain distinct",
            UiChildScratchCleanupNativeLayout.FailureResultInstructionFileOffset == 0xCA6FCF
            && UiChildScratchCleanupNativeLayout.SuccessResultInstructionFileOffset == 0xCA6F49
            && UiChildScratchCleanupNativeLayout.CommonResultInstructionFileOffset == 0xCA7061);
    }
}
