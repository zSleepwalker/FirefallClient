using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct FrameHandoffCapacitySharedRepeatCapture(
    bool EntryValidatedNonzero,
    uint EntryEcx,
    uint EntryEdi,
    bool? DirectionFlagSet,
    bool? RepCompletedNormally);

public enum FrameHandoffCapacitySharedRepeatOutcome
{
    EntryNotValidatedNonzero,
    MissingDirectionFlagCapture,
    MissingRepCompletionCapture,
    RepDidNotCompleteNormally,
    Completed
}

public readonly record struct FrameHandoffCapacitySharedRepeatInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct FrameHandoffCapacitySharedRepeatPlan(
    FrameHandoffCapacitySharedRepeatCapture Capture,
    FrameHandoffCapacitySharedRepeatOutcome Outcome,
    bool EntryValidationAccepted,
    bool XorEaxReached,
    uint? EaxValue,
    bool RepInstructionReached,
    bool? DirectionFlagSet,
    bool AllStoresCompleted,
    FrameHandoffCapacityRepeatStoreDescriptor? StoreDescriptor,
    uint? EcxAfterRep,
    uint? EdiAfterRep,
    bool UnreachedDirectionFlagCaptureProvided,
    bool UnreachedRepCompletionCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the shared XOR EAX / REP STOSD window as a compressed store descriptor.
/// </summary>
public static class FrameHandoffCapacitySharedRepeatProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x00BE6C9E;
    public const uint XorEaxRawFileOffset = EntryRawFileOffset;
    public const uint RepStosdRawFileOffset = 0x00BE6CA0;
    public const uint EndRawFileOffsetExclusive = 0x00BE6CA2;
    public const uint EntryVirtualAddress = 0x00FE789E;
    public const uint EndVirtualAddressExclusive = 0x00FE78A2;
    public const uint DwordSize = 4;
    public const uint CapturedWindowByteCount =
        EndRawFileOffsetExclusive - EntryRawFileOffset;
    public const string CapturedWindowBytes = "33 C0 F3 AB";

    public static ImmutableArray<FrameHandoffCapacitySharedRepeatInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Zero EAX before the shared repeat store",
                XorEaxRawFileOffset, "33 C0"),
            Instruction("REP STOSD stores zero using entry ECX, entry EDI, and the current DF",
                RepStosdRawFileOffset, "F3 AB"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static FrameHandoffCapacitySharedRepeatPlan Project(
        FrameHandoffCapacitySharedRepeatCapture capture)
    {
        if (!capture.EntryValidatedNonzero || capture.EntryEcx == 0)
        {
            return Stop(
                capture,
                FrameHandoffCapacitySharedRepeatOutcome.EntryNotValidatedNonzero,
                unreachedDirectionFlagCaptureProvided: capture.DirectionFlagSet.HasValue,
                unreachedRepCompletionCaptureProvided: capture.RepCompletedNormally.HasValue,
                stopAt: EntryRawFileOffset);
        }

        if (capture.DirectionFlagSet is not bool directionFlagSet)
        {
            return Stop(
                capture,
                FrameHandoffCapacitySharedRepeatOutcome.MissingDirectionFlagCapture,
                entryValidationAccepted: true,
                xorEaxReached: true,
                eaxValue: 0,
                repInstructionReached: true,
                unreachedRepCompletionCaptureProvided: capture.RepCompletedNormally.HasValue,
                stopAt: RepStosdRawFileOffset);
        }

        if (capture.RepCompletedNormally is not bool repCompletedNormally)
        {
            return Stop(
                capture,
                FrameHandoffCapacitySharedRepeatOutcome.MissingRepCompletionCapture,
                entryValidationAccepted: true,
                xorEaxReached: true,
                eaxValue: 0,
                repInstructionReached: true,
                directionFlagSet: directionFlagSet,
                stopAt: RepStosdRawFileOffset);
        }

        if (!repCompletedNormally)
        {
            return Stop(
                capture,
                FrameHandoffCapacitySharedRepeatOutcome.RepDidNotCompleteNormally,
                entryValidationAccepted: true,
                xorEaxReached: true,
                eaxValue: 0,
                repInstructionReached: true,
                directionFlagSet: directionFlagSet,
                stopAt: RepStosdRawFileOffset);
        }

        uint byteDistance = unchecked(capture.EntryEcx * DwordSize);
        uint ediAfterRep = directionFlagSet
            ? unchecked(capture.EntryEdi - byteDistance)
            : unchecked(capture.EntryEdi + byteDistance);
        var descriptor = new FrameHandoffCapacityRepeatStoreDescriptor(
            capture.EntryEcx,
            capture.EntryEdi,
            directionFlagSet ? -(int)DwordSize : (int)DwordSize,
            0);

        return Stop(
            capture,
            FrameHandoffCapacitySharedRepeatOutcome.Completed,
            entryValidationAccepted: true,
            xorEaxReached: true,
            eaxValue: 0,
            repInstructionReached: true,
            directionFlagSet: directionFlagSet,
            allStoresCompleted: true,
            storeDescriptor: descriptor,
            ecxAfterRep: 0,
            ediAfterRep: ediAfterRep,
            stopAt: EndRawFileOffsetExclusive);
    }

    private static FrameHandoffCapacitySharedRepeatInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes);

    private static FrameHandoffCapacitySharedRepeatPlan Stop(
        FrameHandoffCapacitySharedRepeatCapture capture,
        FrameHandoffCapacitySharedRepeatOutcome outcome,
        bool entryValidationAccepted = false,
        bool xorEaxReached = false,
        uint? eaxValue = null,
        bool repInstructionReached = false,
        bool? directionFlagSet = null,
        bool allStoresCompleted = false,
        FrameHandoffCapacityRepeatStoreDescriptor? storeDescriptor = null,
        uint? ecxAfterRep = null,
        uint? ediAfterRep = null,
        bool unreachedDirectionFlagCaptureProvided = false,
        bool unreachedRepCompletionCaptureProvided = false,
        uint stopAt = EntryRawFileOffset) =>
        new(
            capture,
            outcome,
            entryValidationAccepted,
            xorEaxReached,
            eaxValue,
            repInstructionReached,
            directionFlagSet,
            allStoresCompleted,
            storeDescriptor,
            ecxAfterRep,
            ediAfterRep,
            unreachedDirectionFlagCaptureProvided,
            unreachedRepCompletionCaptureProvided,
            stopAt);
}
