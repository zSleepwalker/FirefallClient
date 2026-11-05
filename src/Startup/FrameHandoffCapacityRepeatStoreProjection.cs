using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacityRepeatStoreCapture(
    bool EntryEcxValidatedNonzero,
    uint EntryEdx,
    uint EntryEcx,
    uint EntryEbp,
    bool? DirectionFlagSet,
    bool? RepReturnedNormally,
    uint? FreshFrameMinus8Value);

public readonly record struct FrameHandoffCapacityRepeatStoreDescriptor(
    uint Count,
    uint StartAddress,
    int StrideBytes,
    uint Value);

public enum FrameHandoffCapacityRepeatStoreOutcome
{
    WrongEntry,
    MissingDirectionFlagCapture,
    MissingRepCompletionCapture,
    RepDidNotReturnNormally,
    MissingFreshFrameMinus8Capture,
    Completed
}

public readonly record struct FrameHandoffCapacityRepeatStorePlan(
    FrameHandoffCapacityRepeatStoreCapture Capture,
    FrameHandoffCapacityRepeatStoreOutcome Outcome,
    bool EntryValidationAccepted,
    bool XorEaxReached,
    uint? EaxValue,
    bool MoveEdiFromEdxReached,
    uint? EdiBeforeRep,
    bool RepInstructionReached,
    bool? DirectionFlagSet,
    bool AllStoresCompleted,
    FrameHandoffCapacityRepeatStoreDescriptor? StoreDescriptor,
    uint? EcxAfterRep,
    uint? EdiAfterRep,
    bool FreshFrameMinus8ReadReached,
    uint? FreshFrameMinus8ReadAddress,
    uint? FreshFrameMinus8Value,
    bool UnreachedDirectionFlagCaptureProvided,
    bool UnreachedRepCompletionCaptureProvided,
    bool UnreachedFreshFrameMinus8CaptureProvided,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct FrameHandoffCapacityRepeatStoreInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint VirtualAddress,
    string Bytes);

/// <summary>
/// Projects the bounded REP STOSD slice without enumerating its individual writes.
/// </summary>
public static class FrameHandoffCapacityRepeatStoreProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6C41;
    public const uint XorEaxRawFileOffset = EntryRawFileOffset;
    public const uint MoveEdiFromEdxRawFileOffset = 0x00BE6C43;
    public const uint RepStosdRawFileOffset = 0x00BE6C45;
    public const uint FreshFrameMinus8ReloadRawFileOffset = 0x00BE6C47;
    public const uint EndRawFileOffsetExclusive = 0x00BE6C4A;
    public const uint FreshFrameMinus8Offset = 8;
    public const uint DwordSize = 4;
    public const uint CapturedWindowByteCount = EndRawFileOffsetExclusive - EntryRawFileOffset;

    public const string CapturedWindowBytes = "33C08BFAF3AB8B7DF8";

    public static ImmutableArray<FrameHandoffCapacityRepeatStoreInstructionEvidence> Instructions { get; } =
        ImmutableArray.Create(
            new FrameHandoffCapacityRepeatStoreInstructionEvidence(
                "Zero EAX before the repeat store",
                XorEaxRawFileOffset,
                RawFileOffsetToVirtualAddress(XorEaxRawFileOffset),
                "33 C0"),
            new FrameHandoffCapacityRepeatStoreInstructionEvidence(
                "Copy entry EDX to EDI as the repeat-store start address",
                MoveEdiFromEdxRawFileOffset,
                RawFileOffsetToVirtualAddress(MoveEdiFromEdxRawFileOffset),
                "8B FA"),
            new FrameHandoffCapacityRepeatStoreInstructionEvidence(
                "REP STOSD stores EAX to [EDI] for the entry ECX count using the current DF",
                RepStosdRawFileOffset,
                RawFileOffsetToVirtualAddress(RepStosdRawFileOffset),
                "F3 AB"),
            new FrameHandoffCapacityRepeatStoreInstructionEvidence(
                "Reload EDI independently from [EBP-8]",
                FreshFrameMinus8ReloadRawFileOffset,
                RawFileOffsetToVirtualAddress(FreshFrameMinus8ReloadRawFileOffset),
                "8B 7D F8"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint FreshFrameMinus8ReadAddress(uint entryEbp) =>
        unchecked(entryEbp - FreshFrameMinus8Offset);

    public static FrameHandoffCapacityRepeatStorePlan Project(
        FrameHandoffCapacityRepeatStoreCapture capture)
    {
        if (!capture.EntryEcxValidatedNonzero || capture.EntryEcx == 0)
        {
            return Stop(
                capture,
                FrameHandoffCapacityRepeatStoreOutcome.WrongEntry,
                unreachedDirectionFlagCaptureProvided: capture.DirectionFlagSet.HasValue,
                unreachedRepCompletionCaptureProvided: capture.RepReturnedNormally.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8Value.HasValue,
                stopAt: EntryRawFileOffset);
        }

        if (capture.DirectionFlagSet is not bool directionFlagSet)
        {
            return Stop(
                capture,
                FrameHandoffCapacityRepeatStoreOutcome.MissingDirectionFlagCapture,
                entryValidationAccepted: true,
                xorEaxReached: true,
                eaxValue: 0,
                moveEdiFromEdxReached: true,
                ediBeforeRep: capture.EntryEdx,
                repInstructionReached: true,
                unreachedRepCompletionCaptureProvided: capture.RepReturnedNormally.HasValue,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8Value.HasValue,
                stopAt: RepStosdRawFileOffset);
        }

        if (capture.RepReturnedNormally is not bool repReturnedNormally)
        {
            return Stop(
                capture,
                FrameHandoffCapacityRepeatStoreOutcome.MissingRepCompletionCapture,
                entryValidationAccepted: true,
                xorEaxReached: true,
                eaxValue: 0,
                moveEdiFromEdxReached: true,
                ediBeforeRep: capture.EntryEdx,
                repInstructionReached: true,
                directionFlagSet: directionFlagSet,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8Value.HasValue,
                stopAt: RepStosdRawFileOffset);
        }

        if (!repReturnedNormally)
        {
            return Stop(
                capture,
                FrameHandoffCapacityRepeatStoreOutcome.RepDidNotReturnNormally,
                entryValidationAccepted: true,
                xorEaxReached: true,
                eaxValue: 0,
                moveEdiFromEdxReached: true,
                ediBeforeRep: capture.EntryEdx,
                repInstructionReached: true,
                directionFlagSet: directionFlagSet,
                unreachedFreshFrameMinus8CaptureProvided:
                    capture.FreshFrameMinus8Value.HasValue,
                stopAt: RepStosdRawFileOffset);
        }

        int strideBytes = directionFlagSet ? -(int)DwordSize : (int)DwordSize;
        uint byteDistance = unchecked(capture.EntryEcx * DwordSize);
        uint ediAfterRep = directionFlagSet
            ? unchecked(capture.EntryEdx - byteDistance)
            : unchecked(capture.EntryEdx + byteDistance);
        var descriptor = new FrameHandoffCapacityRepeatStoreDescriptor(
            capture.EntryEcx,
            capture.EntryEdx,
            strideBytes,
            0);
        uint freshFrameMinus8ReadAddress = FreshFrameMinus8ReadAddress(capture.EntryEbp);

        if (capture.FreshFrameMinus8Value is not uint freshFrameMinus8Value)
        {
            return Stop(
                capture,
                FrameHandoffCapacityRepeatStoreOutcome.MissingFreshFrameMinus8Capture,
                entryValidationAccepted: true,
                xorEaxReached: true,
                eaxValue: 0,
                moveEdiFromEdxReached: true,
                ediBeforeRep: capture.EntryEdx,
                repInstructionReached: true,
                directionFlagSet: directionFlagSet,
                allStoresCompleted: true,
                storeDescriptor: descriptor,
                ecxAfterRep: 0,
                ediAfterRep: ediAfterRep,
                freshFrameMinus8ReadReached: true,
                freshFrameMinus8ReadAddress: freshFrameMinus8ReadAddress,
                stopAt: FreshFrameMinus8ReloadRawFileOffset);
        }

        return Stop(
            capture,
            FrameHandoffCapacityRepeatStoreOutcome.Completed,
            entryValidationAccepted: true,
            xorEaxReached: true,
            eaxValue: 0,
            moveEdiFromEdxReached: true,
            ediBeforeRep: capture.EntryEdx,
            repInstructionReached: true,
            directionFlagSet: directionFlagSet,
            allStoresCompleted: true,
            storeDescriptor: descriptor,
            ecxAfterRep: 0,
            ediAfterRep: ediAfterRep,
            freshFrameMinus8ReadReached: true,
            freshFrameMinus8ReadAddress: freshFrameMinus8ReadAddress,
            freshFrameMinus8Value: freshFrameMinus8Value,
            stopAt: EndRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacityRepeatStorePlan Stop(
        FrameHandoffCapacityRepeatStoreCapture capture,
        FrameHandoffCapacityRepeatStoreOutcome outcome,
        bool entryValidationAccepted = false,
        bool xorEaxReached = false,
        uint? eaxValue = null,
        bool moveEdiFromEdxReached = false,
        uint? ediBeforeRep = null,
        bool repInstructionReached = false,
        bool? directionFlagSet = null,
        bool allStoresCompleted = false,
        FrameHandoffCapacityRepeatStoreDescriptor? storeDescriptor = null,
        uint? ecxAfterRep = null,
        uint? ediAfterRep = null,
        bool freshFrameMinus8ReadReached = false,
        uint? freshFrameMinus8ReadAddress = null,
        uint? freshFrameMinus8Value = null,
        bool unreachedDirectionFlagCaptureProvided = false,
        bool unreachedRepCompletionCaptureProvided = false,
        bool unreachedFreshFrameMinus8CaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            entryValidationAccepted,
            xorEaxReached,
            eaxValue,
            moveEdiFromEdxReached,
            ediBeforeRep,
            repInstructionReached,
            directionFlagSet,
            allStoresCompleted,
            storeDescriptor,
            ecxAfterRep,
            ediAfterRep,
            freshFrameMinus8ReadReached,
            freshFrameMinus8ReadAddress,
            freshFrameMinus8Value,
            unreachedDirectionFlagCaptureProvided,
            unreachedRepCompletionCaptureProvided,
            unreachedFreshFrameMinus8CaptureProvided,
            stopAt);
}
