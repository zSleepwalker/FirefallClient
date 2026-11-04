namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolResetFinalizationCapture(
    uint ReturnedCountEdi,
    uint ReceiverPointer,
    uint TempBeginPointer,
    uint PoolCounterPointer,
    uint LocalZeroAddress,
    uint PoolCounterReadbackValue,
    bool ReleaseHelperReturnedNormally = true);

public enum UiShaderSourcePoolResetFinalizationBoundary
{
    ReleaseHelperDidNotReturnNormally,
    NormalReturn
}

public readonly record struct UiShaderSourcePoolResetFinalizationProjection(
    UiShaderSourcePoolResetFinalizationCapture Capture,
    bool LocalZeroInitialized,
    bool StoreFenceReached,
    bool LoadFenceReached,
    bool PoolCounterZeroStoreReached,
    uint? PoolCounterZeroStoreValue,
    bool PoolCounterReadbackReached,
    uint? PoolCounterReadbackValue,
    bool ReleaseHelperCallReached,
    UiShaderSourcePoolResetFinalizationBoundary Boundary,
    bool SehChainRestored,
    uint? ReturnedEax,
    uint? ReturnRawFileOffset,
    uint? ReturnStackPopBytes);

public readonly record struct UiShaderSourcePoolResetFinalizationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? CallTargetRawFileOffset = null);

/// <summary>
/// Projects only the already-reached post-secondary-drain finalization. The
/// receiver, temp-begin pointer, returned count, pool-cell address, and local
/// zero address are independent captures. No secondary traversal, callback,
/// helper side effects, or OS deallocation is modeled.
/// </summary>
public static class UiShaderSourcePoolResetFinalizationProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint FunctionRawFileOffset = 0x12CDC50;
    public const uint FinalizationStartRawFileOffset = 0x12CDE01;
    public const uint FinalizationEndRawFileOffsetExclusive = 0x12CDE44;
    public const uint FinalizationCodeVirtualAddress =
        FinalizationStartRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint ReturnedCountLoadRawFileOffset = 0x12CDE01;
    public const uint PoolCounterPointerLoadRawFileOffset = 0x12CDE04;
    public const uint LocalZeroAddressRawFileOffset = 0x12CDE07;
    public const uint LocalZeroInitializeRawFileOffset = 0x12CDE0A;
    public const uint AliasCompareRawFileOffset = 0x12CDE11;
    public const uint AliasBranchRawFileOffset = 0x12CDE13;
    public const uint AliasBranchTargetRawFileOffset = 0x12CDE20;
    public const uint StoreFenceRawFileOffset = 0x12CDE15;
    public const uint LoadFenceRawFileOffset = 0x12CDE18;
    public const uint PoolCounterZeroStoreRawFileOffset = 0x12CDE1E;
    public const uint PoolCounterReadbackRawFileOffset = 0x12CDE20;
    public const uint TempBeginLoadRawFileOffset = 0x12CDE22;
    public const uint TempBeginNullTestRawFileOffset = 0x12CDE25;
    public const uint TempBeginNullBranchRawFileOffset = 0x12CDE27;
    public const uint TempBeginNullBranchTargetRawFileOffset = 0x12CDE2E;
    public const uint ReleaseHelperCallRawFileOffset = 0x12CDE29;
    public const uint ReleaseHelperTargetRawFileOffset = 0x12C8D60;
    public const uint ReturnedCountMoveToEaxRawFileOffset = 0x12CDE2E;
    public const uint SavedSehChainLoadRawFileOffset = 0x12CDE30;
    public const uint SehChainRestoreRawFileOffset = 0x12CDE33;
    public const uint FrameRestoreRawFileOffset = 0x12CDE3E;
    public const uint SavedFramePointerRestoreRawFileOffset = 0x12CDE40;
    public const uint NormalReturnRawFileOffset = 0x12CDE41;
    public const uint NormalReturnStackPopBytes = 4;

    public const int ReturnedCountStackOffset = -0x18;
    public const int PoolCounterPointerStackOffset = -0x28;
    public const int LocalZeroStackOffset = -0x24;
    public const int TempBeginStackOffset = -0x34;
    public const int SavedSehChainStackOffset = -0x0C;
    public const uint LocalZeroValue = 0;

    public const string CapturedFinalizationBytes =
        "8B 7D E8 8B 4D D8 8D 45 DC C7 45 DC 00 00 00 00 " +
        "3B C1 74 0B 0F AE F8 0F AE E8 8B 45 DC 89 01 8B " +
        "01 8B 4D CC 85 C9 74 05 E8 32 AF FF FF 8B C7 8B " +
        "4D F4 64 89 0D 00 00 00 00 59 5F 5E 5B 8B E5 5D " +
        "C2 04 00";

    public static IReadOnlyList<UiShaderSourcePoolResetFinalizationInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Capture the reached count in EDI",
                ReturnedCountLoadRawFileOffset,
                "8B 7D E8"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Load the pool-counter pointer into ECX",
                PoolCounterPointerLoadRawFileOffset,
                "8B 4D D8"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Form the local zero address in EAX",
                LocalZeroAddressRawFileOffset,
                "8D 45 DC"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Initialize the local DWORD to zero",
                LocalZeroInitializeRawFileOffset,
                "C7 45 DC 00 00 00 00"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Compare the local-zero address with the pool-counter address",
                AliasCompareRawFileOffset,
                "3B C1"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Equal addresses skip both fences and the zero store",
                AliasBranchRawFileOffset,
                "74 0B",
                BranchTargetRawFileOffset: AliasBranchTargetRawFileOffset),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Execute SFENCE on the non-alias path",
                StoreFenceRawFileOffset,
                "0F AE F8"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Execute LFENCE after SFENCE on the non-alias path",
                LoadFenceRawFileOffset,
                "0F AE E8"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Load the initialized local zero",
                0x12CDE1B,
                "8B 45 DC"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Store that zero through the captured pool-counter pointer",
                PoolCounterZeroStoreRawFileOffset,
                "89 01"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Read back the pool-counter DWORD on both alias outcomes",
                PoolCounterReadbackRawFileOffset,
                "8B 01"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Load tempbegin independently for the null gate",
                TempBeginLoadRawFileOffset,
                "8B 4D CC"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Test tempbegin for null",
                TempBeginNullTestRawFileOffset,
                "85 C9"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Null tempbegin skips the helper call",
                TempBeginNullBranchRawFileOffset,
                "74 05",
                BranchTargetRawFileOffset: TempBeginNullBranchTargetRawFileOffset),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Call the opaque helper only for non-null tempbegin",
                ReleaseHelperCallRawFileOffset,
                "E8 32 AF FF FF",
                CallTargetRawFileOffset: ReleaseHelperTargetRawFileOffset),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Move the independently captured count from EDI to EAX",
                ReturnedCountMoveToEaxRawFileOffset,
                "8B C7"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Load the saved SEH chain",
                SavedSehChainLoadRawFileOffset,
                "8B 4D F4"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Restore FS:[0] from the saved SEH chain",
                SehChainRestoreRawFileOffset,
                "64 89 0D 00 00 00 00"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Restore the frame before the normal return",
                FrameRestoreRawFileOffset,
                "8B E5"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Restore the saved frame pointer",
                SavedFramePointerRestoreRawFileOffset,
                "5D"),
            new UiShaderSourcePoolResetFinalizationInstructionEvidence(
                "Reach the normal RET 4 boundary",
                NormalReturnRawFileOffset,
                "C2 04 00")
        });

    public static UiShaderSourcePoolResetFinalizationProjection Project(
        UiShaderSourcePoolResetFinalizationCapture capture)
    {
        bool releaseHelperCallReached = capture.TempBeginPointer != 0;
        if (!releaseHelperCallReached && !capture.ReleaseHelperReturnedNormally)
        {
            throw new ArgumentException(
                "A helper return outcome is invalid when the null gate skips the helper call.",
                nameof(capture));
        }

        bool zeroStoreReached = capture.LocalZeroAddress != capture.PoolCounterPointer;
        if (releaseHelperCallReached && !capture.ReleaseHelperReturnedNormally)
        {
            return new UiShaderSourcePoolResetFinalizationProjection(
                capture,
                LocalZeroInitialized: true,
                StoreFenceReached: zeroStoreReached,
                LoadFenceReached: zeroStoreReached,
                PoolCounterZeroStoreReached: zeroStoreReached,
                PoolCounterZeroStoreValue: zeroStoreReached ? LocalZeroValue : null,
                PoolCounterReadbackReached: true,
                PoolCounterReadbackValue: capture.PoolCounterReadbackValue,
                ReleaseHelperCallReached: true,
                Boundary: UiShaderSourcePoolResetFinalizationBoundary.ReleaseHelperDidNotReturnNormally,
                SehChainRestored: false,
                ReturnedEax: null,
                ReturnRawFileOffset: null,
                ReturnStackPopBytes: null);
        }

        return new UiShaderSourcePoolResetFinalizationProjection(
            capture,
            LocalZeroInitialized: true,
            StoreFenceReached: zeroStoreReached,
            LoadFenceReached: zeroStoreReached,
            PoolCounterZeroStoreReached: zeroStoreReached,
            PoolCounterZeroStoreValue: zeroStoreReached ? LocalZeroValue : null,
            PoolCounterReadbackReached: true,
            PoolCounterReadbackValue: capture.PoolCounterReadbackValue,
            ReleaseHelperCallReached: releaseHelperCallReached,
            Boundary: UiShaderSourcePoolResetFinalizationBoundary.NormalReturn,
            SehChainRestored: true,
            ReturnedEax: capture.ReturnedCountEdi,
            ReturnRawFileOffset: NormalReturnRawFileOffset,
            ReturnStackPopBytes: NormalReturnStackPopBytes);
    }
}
