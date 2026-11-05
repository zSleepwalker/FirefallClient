using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityPrefixCapture(
    uint EntryEbx,
    uint EntryEbp,
    uint EntryEsi,
    uint EntryEdx,
    uint? ReceiverC);

public enum FrameHandoffCapacityPrefixOutcome
{
    MissingReceiverCRead,
    ZeroSumBranchToRawFileOffsetCA,
    AboveGuardBranchToRawFileOffsetBC4,
    ScaledPushBeforeCall
}

public readonly record struct FrameHandoffCapacityPrefixFrameStore(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct FrameHandoffCapacityPrefixPlan(
    FrameHandoffCapacityPrefixCapture Capture,
    FrameHandoffCapacityPrefixOutcome Outcome,
    bool ReceiverCReadReached,
    uint ReceiverCReadAddress,
    bool ReceiverCCaptured,
    uint? EdiAfterReceiverCRead,
    bool EsiSubtractReached,
    uint? EsiAfterSubtract,
    bool EdiShiftReached,
    uint? EdiAfterShift,
    ImmutableArray<FrameHandoffCapacityPrefixFrameStore> FrameStores,
    bool LeaReached,
    uint? EaxAfterLea,
    bool TestReached,
    bool? ZeroFlag,
    bool ZeroBranchReached,
    bool? ZeroBranchTaken,
    bool CompareReached,
    bool? AboveGuardBranchTaken,
    bool ScaleReached,
    uint? EaxAfterScale,
    bool PushReached,
    uint? PushedValue,
    uint? BranchTargetRawFileOffset,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct FrameHandoffCapacityPrefixInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

/// <summary>
/// Projects the capacity-prefix instructions through PUSH EAX, stopping before the call.
/// </summary>
public static class FrameHandoffCapacityPrefixProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6B91;
    public const uint EntryVirtualAddress = 0x00FE7791;
    public const uint EndRawFileOffsetExclusive = 0x00BE6BB5;
    public const uint EndVirtualAddressExclusive = 0x00FE77B5;
    public const uint ZeroBranchTargetRawFileOffset = 0x00BE6BCA;
    public const uint AboveGuardBranchTargetRawFileOffset = 0x00BE6BC4;
    public const uint ReceiverCOffset = 0x0C;
    public const uint FrameEdiStoreOffset = 0x08;
    public const uint FrameZeroStoreOffset = 0x04;
    public const uint MaximumUnscaledValue = 0x3FFFFFFF;

    public const uint EdiReadRawFileOffset = 0x00BE6B91;
    public const uint EsiSubtractRawFileOffset = 0x00BE6B94;
    public const uint EdiShiftRawFileOffset = 0x00BE6B96;
    public const uint EdiFrameStoreRawFileOffset = 0x00BE6B99;
    public const uint ZeroFrameStoreRawFileOffset = 0x00BE6B9C;
    public const uint EaxLeaRawFileOffset = 0x00BE6BA3;
    public const uint EaxTestRawFileOffset = 0x00BE6BA6;
    public const uint ZeroBranchRawFileOffset = 0x00BE6BA8;
    public const uint MaximumCompareRawFileOffset = 0x00BE6BAA;
    public const uint AboveGuardBranchRawFileOffset = 0x00BE6BAF;
    public const uint EaxScaleRawFileOffset = 0x00BE6BB1;
    public const uint PushEaxRawFileOffset = 0x00BE6BB4;

    public const string CapturedWindowBytes =
        "8B 7B 0C 2B F2 C1 EF 02 89 7D F8 C7 45 FC 00 00 00 00 "
        + "8D 04 32 85 C0 74 20 3D FF FF FF 3F 77 13 C1 E0 02 50";

    public static ImmutableArray<FrameHandoffCapacityPrefixInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new FrameHandoffCapacityPrefixInstructionEvidence(
                "Read DWORD [EBX+0x0C] into EDI",
                EdiReadRawFileOffset,
                RawFileOffsetToVirtualAddress(EdiReadRawFileOffset),
                "8B 7B 0C"),
            new FrameHandoffCapacityPrefixInstructionEvidence(
                "Subtract entry EDX from ESI with 32-bit wraparound",
                EsiSubtractRawFileOffset,
                RawFileOffsetToVirtualAddress(EsiSubtractRawFileOffset),
                "2B F2"),
            new FrameHandoffCapacityPrefixInstructionEvidence(
                "Shift EDI right by two",
                EdiShiftRawFileOffset,
                RawFileOffsetToVirtualAddress(EdiShiftRawFileOffset),
                "C1 EF 02"),
            new FrameHandoffCapacityPrefixInstructionEvidence(
                "Store shifted EDI at [EBP-8]",
                EdiFrameStoreRawFileOffset,
                RawFileOffsetToVirtualAddress(EdiFrameStoreRawFileOffset),
                "89 7D F8"),
            new FrameHandoffCapacityPrefixInstructionEvidence(
                "Store zero at [EBP-4]",
                ZeroFrameStoreRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroFrameStoreRawFileOffset),
                "C7 45 FC 00 00 00 00"),
            new FrameHandoffCapacityPrefixInstructionEvidence(
                "Form EAX as EDX+ESI with 32-bit wraparound",
                EaxLeaRawFileOffset,
                RawFileOffsetToVirtualAddress(EaxLeaRawFileOffset),
                "8D 04 32"),
            new FrameHandoffCapacityPrefixInstructionEvidence(
                "Test EAX against zero",
                EaxTestRawFileOffset,
                RawFileOffsetToVirtualAddress(EaxTestRawFileOffset),
                "85 C0"),
            new FrameHandoffCapacityPrefixInstructionEvidence(
                "JZ routes to raw 0xBE6BCA",
                ZeroBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroBranchRawFileOffset),
                "74 20",
                ZeroBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(ZeroBranchTargetRawFileOffset)),
            new FrameHandoffCapacityPrefixInstructionEvidence(
                "Compare EAX with 0x3FFFFFFF",
                MaximumCompareRawFileOffset,
                RawFileOffsetToVirtualAddress(MaximumCompareRawFileOffset),
                "3D FF FF FF 3F"),
            new FrameHandoffCapacityPrefixInstructionEvidence(
                "Unsigned JA routes above the guard to raw 0xBE6BC4",
                AboveGuardBranchRawFileOffset,
                RawFileOffsetToVirtualAddress(AboveGuardBranchRawFileOffset),
                "77 13",
                AboveGuardBranchTargetRawFileOffset,
                RawFileOffsetToVirtualAddress(AboveGuardBranchTargetRawFileOffset)),
            new FrameHandoffCapacityPrefixInstructionEvidence(
                "Shift EAX left by two with 32-bit wraparound",
                EaxScaleRawFileOffset,
                RawFileOffsetToVirtualAddress(EaxScaleRawFileOffset),
                "C1 E0 02"),
            new FrameHandoffCapacityPrefixInstructionEvidence(
                "Push EAX; the following call at raw 0xBE6BB5 is excluded",
                PushEaxRawFileOffset,
                RawFileOffsetToVirtualAddress(PushEaxRawFileOffset),
                "50"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint ReceiverCReadAddress(uint entryEbx) =>
        unchecked(entryEbx + ReceiverCOffset);

    public static FrameHandoffCapacityPrefixPlan Project(
        FrameHandoffCapacityPrefixCapture capture)
    {
        uint receiverCReadAddress = ReceiverCReadAddress(capture.EntryEbx);

        if (capture.ReceiverC is not uint receiverC)
        {
            return new(
                capture,
                FrameHandoffCapacityPrefixOutcome.MissingReceiverCRead,
                true,
                receiverCReadAddress,
                false,
                null,
                false,
                null,
                false,
                null,
                ImmutableArray<FrameHandoffCapacityPrefixFrameStore>.Empty,
                false,
                null,
                false,
                null,
                false,
                null,
                false,
                null,
                false,
                null,
                false,
                null,
                null,
                EntryRawFileOffset);
        }

        uint esiAfterSubtract = unchecked(capture.EntryEsi - capture.EntryEdx);
        uint ediAfterShift = receiverC >> 2;
        ImmutableArray<FrameHandoffCapacityPrefixFrameStore> frameStores =
            ImmutableArray.Create(
                new FrameHandoffCapacityPrefixFrameStore(
                    EdiFrameStoreRawFileOffset,
                    unchecked(capture.EntryEbp - FrameEdiStoreOffset),
                    ediAfterShift),
                new FrameHandoffCapacityPrefixFrameStore(
                    ZeroFrameStoreRawFileOffset,
                    unchecked(capture.EntryEbp - FrameZeroStoreOffset),
                    0));
        uint eaxAfterLea = unchecked(capture.EntryEdx + esiAfterSubtract);
        bool zeroFlag = eaxAfterLea == 0;

        if (zeroFlag)
        {
            return new(
                capture,
                FrameHandoffCapacityPrefixOutcome.ZeroSumBranchToRawFileOffsetCA,
                true,
                receiverCReadAddress,
                true,
                receiverC,
                true,
                esiAfterSubtract,
                true,
                ediAfterShift,
                frameStores,
                true,
                eaxAfterLea,
                true,
                zeroFlag,
                true,
                true,
                false,
                null,
                false,
                null,
                false,
                null,
                ZeroBranchTargetRawFileOffset,
                ZeroBranchTargetRawFileOffset);
        }

        bool aboveGuard = eaxAfterLea > MaximumUnscaledValue;
        if (aboveGuard)
        {
            return new(
                capture,
                FrameHandoffCapacityPrefixOutcome.AboveGuardBranchToRawFileOffsetBC4,
                true,
                receiverCReadAddress,
                true,
                receiverC,
                true,
                esiAfterSubtract,
                true,
                ediAfterShift,
                frameStores,
                true,
                eaxAfterLea,
                true,
                zeroFlag,
                true,
                false,
                true,
                true,
                false,
                null,
                false,
                null,
                AboveGuardBranchTargetRawFileOffset,
                AboveGuardBranchTargetRawFileOffset);
        }

        uint eaxAfterScale = unchecked(eaxAfterLea << 2);
        return new(
            capture,
            FrameHandoffCapacityPrefixOutcome.ScaledPushBeforeCall,
            true,
            receiverCReadAddress,
            true,
            receiverC,
            true,
            esiAfterSubtract,
            true,
            ediAfterShift,
            frameStores,
            true,
            eaxAfterLea,
            true,
            zeroFlag,
            true,
            false,
            true,
            false,
            true,
            eaxAfterScale,
            true,
            eaxAfterScale,
            null,
            EndRawFileOffsetExclusive);
    }
}
