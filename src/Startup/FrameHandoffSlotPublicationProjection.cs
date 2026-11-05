using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public enum FrameHandoffSlotPublicationEntry
{
    PublicationStorePathAt6508,
    ExistingNonzeroSlotBranchAt650E
}

public readonly record struct FrameHandoffSlotPublicationCapture(
    FrameHandoffSlotPublicationEntry? Entry,
    uint ReceiverBaseEsi,
    uint SlotArrayBaseEbx,
    uint IncomingEdi,
    uint? IncomingEcx,
    uint? ReceiverPlus04At6508,
    uint? ReceiverPlus04At650E,
    uint? SlotDwordAtEbxPlusFreshBase);

public enum FrameHandoffSlotPublicationOutcome
{
    MissingBranchEntryCapture,
    InvalidBranchEntryCapture,
    MissingStoreBaseCapture,
    MissingStoreEcxCapture,
    MissingFreshBaseCapture,
    MissingSlotDwordCapture,
    ProjectionComplete
}

public readonly record struct FrameHandoffSlotPublicationPlan(
    FrameHandoffSlotPublicationCapture Capture,
    FrameHandoffSlotPublicationOutcome Outcome,
    uint? StoreBaseReadAddress,
    uint? StoreBaseReadValue,
    bool StoreBaseReadReached,
    bool StoreBaseCaptured,
    bool StoreInstructionReached,
    bool StoreWriteProjected,
    uint? StoreWriteAddress,
    uint? StoreWriteValue,
    uint? FreshBaseReadAddress,
    uint? FreshBaseReadValue,
    bool FreshBaseReadReached,
    bool FreshBaseCaptured,
    uint? SlotReadAddress,
    uint? SlotReadValue,
    bool SlotReadReached,
    bool SlotDwordCaptured,
    uint? MaskedEdi,
    uint? ComputedEcxBeforeTest,
    ImmutableArray<string> StackPopRegisters,
    bool UnreachedStoreBaseCaptureProvided,
    bool UnreachedEcxCaptureProvided,
    bool UnreachedFreshBaseCaptureProvided,
    bool UnreachedSlotDwordCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct FrameHandoffSlotPublicationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the conditional slot store and its independent fresh-base continuation through
/// the instruction before the ECX test at raw 0xBE651C. A projected store does not guarantee
/// that the memory access succeeds.
/// </summary>
public static class FrameHandoffSlotPublicationProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint PublicationStoreEntryRawFileOffset = 0x00BE6508;
    public const uint ExistingNonzeroSlotBranchEntryRawFileOffset = 0x00BE650E;
    public const uint StoreBaseReadRawFileOffset = 0x00BE6508;
    public const uint StoreRawFileOffset = 0x00BE650B;
    public const uint FreshBaseReadRawFileOffset = 0x00BE650E;
    public const uint SlotReadRawFileOffset = 0x00BE6514;
    public const uint EdiMaskRawFileOffset = 0x00BE6511;
    public const uint EcxAddressFormationRawFileOffset = 0x00BE6517;
    public const uint FirstStackPopRawFileOffset = 0x00BE651A;
    public const uint EcxTestRawFileOffset = 0x00BE651C;
    public const uint EcxTestBranchRawFileOffset = 0x00BE651E;
    public const uint EcxTestBranchTargetRawFileOffset = 0x00BE6527;
    public const uint StopBeforeEcxTestRawFileOffsetExclusive = EcxTestRawFileOffset;

    public const uint ReceiverPlus04Offset = 0x04;
    public const uint EdiMask = 3;
    public const uint EdiScale = 4;

    public const uint StorePathBranchRawFileOffset = 0x00BE6500;
    public const uint StorePathBranchTargetRawFileOffset = PublicationStoreEntryRawFileOffset;
    public const uint ExistingSlotBranchRawFileOffset = 0x00BE64F0;
    public const uint ExistingSlotBranchTargetRawFileOffset =
        ExistingNonzeroSlotBranchEntryRawFileOffset;

    public const string CapturedWindowBytes =
        "8B 46 04 89 0C 03 8B 46 04 83 E7 03 8B 04 18 8D 0C B8 5F 5B";
    public const string EcxTestBoundaryBytes = "85 C9";
    public const string EcxTestBranchBytes = "74 07";
    public const string StorePathBranchBytes = "75 06";
    public const string ExistingSlotBranchBytes = "75 1C";

    public static ImmutableArray<FrameHandoffSlotPublicationInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new FrameHandoffSlotPublicationInstructionEvidence(
                "Independently reload [ESI+4] for the conditional publication store",
                StoreBaseReadRawFileOffset,
                StoreBaseReadRawFileOffset,
                RawFileOffsetToVirtualAddress(StoreBaseReadRawFileOffset),
                "8B 46 04"),
            new FrameHandoffSlotPublicationInstructionEvidence(
                "Store the captured ECX DWORD at [EBX+the first reloaded base]",
                StoreRawFileOffset,
                StoreRawFileOffset,
                RawFileOffsetToVirtualAddress(StoreRawFileOffset),
                "89 0C 03"),
            new FrameHandoffSlotPublicationInstructionEvidence(
                "Independently reload [ESI+4] for the following slot read",
                FreshBaseReadRawFileOffset,
                FreshBaseReadRawFileOffset,
                RawFileOffsetToVirtualAddress(FreshBaseReadRawFileOffset),
                "8B 46 04"),
            new FrameHandoffSlotPublicationInstructionEvidence(
                "Mask EDI to its low two bits",
                EdiMaskRawFileOffset,
                EdiMaskRawFileOffset,
                RawFileOffsetToVirtualAddress(EdiMaskRawFileOffset),
                "83 E7 03"),
            new FrameHandoffSlotPublicationInstructionEvidence(
                "Read the DWORD at [EBX+the independently reloaded base]",
                SlotReadRawFileOffset,
                SlotReadRawFileOffset,
                RawFileOffsetToVirtualAddress(SlotReadRawFileOffset),
                "8B 04 18"),
            new FrameHandoffSlotPublicationInstructionEvidence(
                "Form wrapping ECX = slot DWORD + masked EDI*4",
                EcxAddressFormationRawFileOffset,
                EcxAddressFormationRawFileOffset,
                RawFileOffsetToVirtualAddress(EcxAddressFormationRawFileOffset),
                "8D 0C B8"),
            new FrameHandoffSlotPublicationInstructionEvidence(
                "Pop EDI and then EBX before the ECX test",
                FirstStackPopRawFileOffset,
                FirstStackPopRawFileOffset,
                RawFileOffsetToVirtualAddress(FirstStackPopRawFileOffset),
                "5F 5B"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverPlus04Address(uint receiverBaseEsi) =>
        unchecked(receiverBaseEsi + ReceiverPlus04Offset);

    public static uint StoreWriteAddress(uint slotArrayBaseEbx, uint firstReloadedBase) =>
        unchecked(slotArrayBaseEbx + firstReloadedBase);

    public static uint SlotReadAddress(uint slotArrayBaseEbx, uint freshReloadedBase) =>
        unchecked(slotArrayBaseEbx + freshReloadedBase);

    public static uint ComputeEcxBeforeTest(uint slotDword, uint incomingEdi) =>
        unchecked(slotDword + ((incomingEdi & EdiMask) * EdiScale));

    public static FrameHandoffSlotPublicationPlan Project(
        FrameHandoffSlotPublicationCapture capture)
    {
        bool storePath =
            capture.Entry == FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508;
        bool existingSlotBranch =
            capture.Entry == FrameHandoffSlotPublicationEntry.ExistingNonzeroSlotBranchAt650E;
        if (!storePath && !existingSlotBranch)
        {
            return Stop(
                capture,
                capture.Entry.HasValue
                    ? FrameHandoffSlotPublicationOutcome.InvalidBranchEntryCapture
                    : FrameHandoffSlotPublicationOutcome.MissingBranchEntryCapture,
                unreachedStoreBaseCaptureProvided: capture.ReceiverPlus04At6508.HasValue,
                unreachedEcxCaptureProvided: capture.IncomingEcx.HasValue,
                unreachedFreshBaseCaptureProvided: capture.ReceiverPlus04At650E.HasValue,
                unreachedSlotDwordCaptureProvided: capture.SlotDwordAtEbxPlusFreshBase.HasValue,
                stopAt: PublicationStoreEntryRawFileOffset);
        }

        if (storePath && !capture.ReceiverPlus04At6508.HasValue)
        {
            return Stop(
                capture,
                FrameHandoffSlotPublicationOutcome.MissingStoreBaseCapture,
                storeBaseReadAddress: ReceiverPlus04Address(capture.ReceiverBaseEsi),
                storeBaseReadReached: true,
                unreachedEcxCaptureProvided: capture.IncomingEcx.HasValue,
                unreachedFreshBaseCaptureProvided: capture.ReceiverPlus04At650E.HasValue,
                unreachedSlotDwordCaptureProvided: capture.SlotDwordAtEbxPlusFreshBase.HasValue,
                stopAt: StoreBaseReadRawFileOffset);
        }

        uint? storeReadAddress = storePath
            ? ReceiverPlus04Address(capture.ReceiverBaseEsi)
            : null;
        uint? storeAddress = null;
        uint? storeValue = null;
        bool storeInstructionReached = storePath;
        bool storeProjected = false;
        if (storePath)
        {
            uint capturedStoreBase = capture.ReceiverPlus04At6508!.Value;
            storeAddress = StoreWriteAddress(capture.SlotArrayBaseEbx, capturedStoreBase);
            if (capture.IncomingEcx is not uint incomingEcx)
            {
                return Stop(
                    capture,
                    FrameHandoffSlotPublicationOutcome.MissingStoreEcxCapture,
                    storeReadAddress,
                    capturedStoreBase,
                    storeBaseReadReached: true,
                    storeBaseCaptured: true,
                    storeInstructionReached: true,
                    storeWriteAddress: storeAddress,
                    unreachedFreshBaseCaptureProvided: capture.ReceiverPlus04At650E.HasValue,
                    unreachedSlotDwordCaptureProvided: capture.SlotDwordAtEbxPlusFreshBase.HasValue,
                    stopAt: StoreRawFileOffset);
            }

            storeValue = incomingEcx;
            storeProjected = true;
        }

        uint freshBaseReadAddress = ReceiverPlus04Address(capture.ReceiverBaseEsi);
        if (capture.ReceiverPlus04At650E is not uint freshBase)
        {
            return Stop(
                capture,
                FrameHandoffSlotPublicationOutcome.MissingFreshBaseCapture,
                storeReadAddress,
                storePath ? capture.ReceiverPlus04At6508 : null,
                storeBaseReadReached: storePath,
                storeBaseCaptured: storePath,
                storeInstructionReached: storeInstructionReached,
                storeWriteProjected: storeProjected,
                storeWriteAddress: storeAddress,
                storeWriteValue: storeValue,
                freshBaseReadAddress: freshBaseReadAddress,
                freshBaseReadReached: true,
                unreachedEcxCaptureProvided:
                    !storePath && capture.IncomingEcx.HasValue,
                unreachedStoreBaseCaptureProvided:
                    !storePath && capture.ReceiverPlus04At6508.HasValue,
                unreachedSlotDwordCaptureProvided: capture.SlotDwordAtEbxPlusFreshBase.HasValue,
                stopAt: FreshBaseReadRawFileOffset);
        }

        uint slotReadAddress = SlotReadAddress(capture.SlotArrayBaseEbx, freshBase);
        uint maskedEdi = capture.IncomingEdi & EdiMask;
        if (capture.SlotDwordAtEbxPlusFreshBase is not uint slotDword)
        {
            return Stop(
                capture,
                FrameHandoffSlotPublicationOutcome.MissingSlotDwordCapture,
                storeBaseReadAddress: storeReadAddress,
                storeBaseReadValue: storePath ? capture.ReceiverPlus04At6508 : null,
                storeBaseReadReached: storePath,
                storeBaseCaptured: storePath,
                storeInstructionReached: storeInstructionReached,
                storeWriteProjected: storeProjected,
                storeWriteAddress: storeAddress,
                storeWriteValue: storeValue,
                freshBaseReadAddress: freshBaseReadAddress,
                freshBaseReadValue: freshBase,
                freshBaseReadReached: true,
                freshBaseCaptured: true,
                slotReadAddress: slotReadAddress,
                slotReadReached: true,
                maskedEdi: maskedEdi,
                unreachedEcxCaptureProvided:
                    !storePath && capture.IncomingEcx.HasValue,
                unreachedStoreBaseCaptureProvided:
                    !storePath && capture.ReceiverPlus04At6508.HasValue,
                stopAt: SlotReadRawFileOffset);
        }

        return new FrameHandoffSlotPublicationPlan(
            Capture: capture,
            Outcome: FrameHandoffSlotPublicationOutcome.ProjectionComplete,
            StoreBaseReadAddress: storeReadAddress,
            StoreBaseReadValue: storePath ? capture.ReceiverPlus04At6508 : null,
            StoreBaseReadReached: storePath,
            StoreBaseCaptured: storePath,
            StoreInstructionReached: storeInstructionReached,
            StoreWriteProjected: storeProjected,
            StoreWriteAddress: storeAddress,
            StoreWriteValue: storeValue,
            FreshBaseReadAddress: freshBaseReadAddress,
            FreshBaseReadValue: freshBase,
            FreshBaseReadReached: true,
            FreshBaseCaptured: true,
            SlotReadAddress: slotReadAddress,
            SlotReadValue: slotDword,
            SlotReadReached: true,
            SlotDwordCaptured: true,
            MaskedEdi: maskedEdi,
            ComputedEcxBeforeTest: ComputeEcxBeforeTest(slotDword, capture.IncomingEdi),
            StackPopRegisters: ImmutableArray.Create("EDI", "EBX"),
            UnreachedStoreBaseCaptureProvided:
                !storePath && capture.ReceiverPlus04At6508.HasValue,
            UnreachedEcxCaptureProvided:
                !storePath && capture.IncomingEcx.HasValue,
            UnreachedFreshBaseCaptureProvided: false,
            UnreachedSlotDwordCaptureProvided: false,
            StopAtRawFileOffsetExclusive: StopBeforeEcxTestRawFileOffsetExclusive);
    }

    private static FrameHandoffSlotPublicationPlan Stop(
        FrameHandoffSlotPublicationCapture capture,
        FrameHandoffSlotPublicationOutcome outcome,
        uint? storeBaseReadAddress = null,
        uint? storeBaseReadValue = null,
        bool storeBaseReadReached = false,
        bool storeBaseCaptured = false,
        bool storeInstructionReached = false,
        bool storeWriteProjected = false,
        uint? storeWriteAddress = null,
        uint? storeWriteValue = null,
        uint? freshBaseReadAddress = null,
        uint? freshBaseReadValue = null,
        bool freshBaseReadReached = false,
        bool freshBaseCaptured = false,
        uint? slotReadAddress = null,
        uint? slotReadValue = null,
        bool slotReadReached = false,
        bool slotDwordCaptured = false,
        uint? maskedEdi = null,
        bool unreachedStoreBaseCaptureProvided = false,
        bool unreachedEcxCaptureProvided = false,
        bool unreachedFreshBaseCaptureProvided = false,
        bool unreachedSlotDwordCaptureProvided = false,
        uint stopAt = StopBeforeEcxTestRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            storeBaseReadAddress,
            storeBaseReadValue,
            storeBaseReadReached,
            storeBaseCaptured,
            storeInstructionReached,
            storeWriteProjected,
            storeWriteAddress,
            storeWriteValue,
            freshBaseReadAddress,
            freshBaseReadValue,
            freshBaseReadReached,
            freshBaseCaptured,
            slotReadAddress,
            slotReadValue,
            slotReadReached,
            slotDwordCaptured,
            maskedEdi,
            ComputedEcxBeforeTest: null,
            StackPopRegisters: ImmutableArray<string>.Empty,
            UnreachedStoreBaseCaptureProvided: unreachedStoreBaseCaptureProvided,
            UnreachedEcxCaptureProvided: unreachedEcxCaptureProvided,
            UnreachedFreshBaseCaptureProvided: unreachedFreshBaseCaptureProvided,
            UnreachedSlotDwordCaptureProvided: unreachedSlotDwordCaptureProvided,
            StopAtRawFileOffsetExclusive: stopAt);
}
