using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleePairedFieldWritesCapture(
    uint? ValidatedEntryEBP,
    uint? ValidatedEntryEAX,
    uint? FreshFrameMinus4,
    uint? FreshGlobalValue);

public enum CapacityFinalCalleePairedFieldWritesOutcome
{
    MissingValidatedEntryEBP,
    MissingFreshFrameMinus4,
    MissingValidatedEntryEAX,
    MissingFreshGlobalValue,
    Completed
}

public readonly record struct CapacityFinalCalleePairedFieldWritesInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? PeVirtualAddressOperand = null);

public readonly record struct CapacityFinalCalleePairedFieldWritesPlan(
    CapacityFinalCalleePairedFieldWritesCapture Capture,
    CapacityFinalCalleePairedFieldWritesOutcome Outcome,
    uint? FreshFrameMinus4Address,
    bool FreshFrameMinus4ReadReached,
    bool FreshFrameMinus4Captured,
    bool UnreachedFreshFrameMinus4CaptureProvided,
    uint? ECXLoaded,
    uint? EAXRetained,
    bool FirstWriteReached,
    uint? FirstWriteAddress,
    uint? FirstWriteValue,
    bool SecondWriteReached,
    uint? SecondWriteAddress,
    uint? SecondWriteValue,
    bool FreshGlobalReadReached,
    bool FreshGlobalValueCaptured,
    uint? EBXFresh,
    bool UnreachedValidatedEntryEAXCaptureProvided,
    bool UnreachedFreshGlobalValueCaptureProvided,
    uint StopAtRawFileOffsetExclusive);

/// <summary>
/// Projects the two scalar field writes and subsequent fresh global load.
/// Captures remain independent even when an address aliases a written field;
/// no pointer validity, ownership, or memory effects are inferred.
/// </summary>
public static class CapacityFinalCalleePairedFieldWritesProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C8EE9;
    public const uint EntryVirtualAddress = 0x016C9AE9;
    public const uint FirstWriteRawFileOffset = 0x012C8EEC;
    public const uint SecondWriteRawFileOffset = 0x012C8EEF;
    public const uint FreshGlobalLoadRawFileOffset = 0x012C8EF2;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C8EF8;

    public const uint FrameMinus4Displacement = 4;
    public const uint FirstWriteDisplacement = 0xC;
    public const uint SecondWriteDisplacement = 8;
    public const uint GlobalValuePeVirtualAddress = 0x01EDADD8;

    public const string CapturedWindowBytes =
        "8B 4D FC 89 41 0C 89 48 08 8B 1D D8 AD ED 01";

    public static ImmutableArray<CapacityFinalCalleePairedFieldWritesInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Load a fresh DWORD at entry EBP-4 into ECX",
                EntryRawFileOffset, "8B 4D FC"),
            Instruction("Write retained entry EAX to the DWORD at loaded ECX+0C",
                FirstWriteRawFileOffset, "89 41 0C"),
            Instruction("Write loaded ECX to the DWORD at retained entry EAX+8",
                SecondWriteRawFileOffset, "89 48 08"),
            Instruction("Load a fresh DWORD from the absolute PE VA 1EDADD8 into EBX",
                FreshGlobalLoadRawFileOffset, "8B 1D D8 AD ED 01",
                peVirtualAddressOperand: GlobalValuePeVirtualAddress));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFrameMinus4Address(uint entryEbp) =>
        unchecked(entryEbp - FrameMinus4Displacement);

    public static uint DeriveFirstWriteAddress(uint loadedEcx) =>
        unchecked(loadedEcx + FirstWriteDisplacement);

    public static uint DeriveSecondWriteAddress(uint retainedEax) =>
        unchecked(retainedEax + SecondWriteDisplacement);

    public static CapacityFinalCalleePairedFieldWritesPlan Project(
        CapacityFinalCalleePairedFieldWritesCapture capture)
    {
        if (capture.ValidatedEntryEBP is not uint entryEbp)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleePairedFieldWritesOutcome.MissingValidatedEntryEBP,
                unreachedFreshFrameMinus4CaptureProvided:
                    capture.FreshFrameMinus4.HasValue,
                eaxRetained: capture.ValidatedEntryEAX,
                unreachedValidatedEntryEAXCaptureProvided:
                    capture.ValidatedEntryEAX.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: EntryRawFileOffset);
        }

        uint frameMinus4Address = DeriveFrameMinus4Address(entryEbp);
        if (capture.FreshFrameMinus4 is not uint freshFrameMinus4)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleePairedFieldWritesOutcome.MissingFreshFrameMinus4,
                freshFrameMinus4Address: frameMinus4Address,
                freshFrameMinus4ReadReached: true,
                eaxRetained: capture.ValidatedEntryEAX,
                unreachedValidatedEntryEAXCaptureProvided:
                    capture.ValidatedEntryEAX.HasValue,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: EntryRawFileOffset);
        }

        if (capture.ValidatedEntryEAX is not uint entryEax)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleePairedFieldWritesOutcome.MissingValidatedEntryEAX,
                freshFrameMinus4Address: frameMinus4Address,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                ecxLoaded: freshFrameMinus4,
                unreachedFreshGlobalValueCaptureProvided:
                    capture.FreshGlobalValue.HasValue,
                stopAt: FirstWriteRawFileOffset);
        }

        uint firstWriteAddress = DeriveFirstWriteAddress(freshFrameMinus4);
        uint secondWriteAddress = DeriveSecondWriteAddress(entryEax);
        if (capture.FreshGlobalValue is not uint freshGlobalValue)
        {
            return BuildPlan(
                capture,
                CapacityFinalCalleePairedFieldWritesOutcome.MissingFreshGlobalValue,
                freshFrameMinus4Address: frameMinus4Address,
                freshFrameMinus4ReadReached: true,
                freshFrameMinus4Captured: true,
                ecxLoaded: freshFrameMinus4,
                eaxRetained: entryEax,
                firstWriteReached: true,
                firstWriteAddress: firstWriteAddress,
                firstWriteValue: entryEax,
                secondWriteReached: true,
                secondWriteAddress: secondWriteAddress,
                secondWriteValue: freshFrameMinus4,
                freshGlobalReadReached: true,
                stopAt: FreshGlobalLoadRawFileOffset);
        }

        return BuildPlan(
            capture,
            CapacityFinalCalleePairedFieldWritesOutcome.Completed,
            freshFrameMinus4Address: frameMinus4Address,
            freshFrameMinus4ReadReached: true,
            freshFrameMinus4Captured: true,
            ecxLoaded: freshFrameMinus4,
            eaxRetained: entryEax,
            firstWriteReached: true,
            firstWriteAddress: firstWriteAddress,
            firstWriteValue: entryEax,
            secondWriteReached: true,
            secondWriteAddress: secondWriteAddress,
            secondWriteValue: freshFrameMinus4,
            freshGlobalReadReached: true,
            freshGlobalValueCaptured: true,
            ebxFresh: freshGlobalValue,
            stopAt: BoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleePairedFieldWritesPlan BuildPlan(
        CapacityFinalCalleePairedFieldWritesCapture capture,
        CapacityFinalCalleePairedFieldWritesOutcome outcome,
        uint? freshFrameMinus4Address = null,
        bool freshFrameMinus4ReadReached = false,
        bool freshFrameMinus4Captured = false,
        bool unreachedFreshFrameMinus4CaptureProvided = false,
        uint? ecxLoaded = null,
        uint? eaxRetained = null,
        bool firstWriteReached = false,
        uint? firstWriteAddress = null,
        uint? firstWriteValue = null,
        bool secondWriteReached = false,
        uint? secondWriteAddress = null,
        uint? secondWriteValue = null,
        bool freshGlobalReadReached = false,
        bool freshGlobalValueCaptured = false,
        uint? ebxFresh = null,
        bool unreachedValidatedEntryEAXCaptureProvided = false,
        bool unreachedFreshGlobalValueCaptureProvided = false,
        uint stopAt = BoundaryRawFileOffsetExclusive) =>
        new(
            capture,
            outcome,
            freshFrameMinus4Address,
            freshFrameMinus4ReadReached,
            freshFrameMinus4Captured,
            unreachedFreshFrameMinus4CaptureProvided,
            ecxLoaded,
            eaxRetained,
            firstWriteReached,
            firstWriteAddress,
            firstWriteValue,
            secondWriteReached,
            secondWriteAddress,
            secondWriteValue,
            freshGlobalReadReached,
            freshGlobalValueCaptured,
            ebxFresh,
            unreachedValidatedEntryEAXCaptureProvided,
            unreachedFreshGlobalValueCaptureProvided,
            stopAt);

    private static CapacityFinalCalleePairedFieldWritesInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? peVirtualAddressOperand = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            peVirtualAddressOperand);
}
