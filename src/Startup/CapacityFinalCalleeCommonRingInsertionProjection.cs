using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonRingInsertionCapture(
    uint? EntryEAX,
    uint? EntryESI,
    uint? LateEntryEDX);

public enum CapacityFinalCalleeCommonRingInsertionOutcome
{
    MissingEntryEAXForFirstWrite,
    MissingEntryESIForFirstWrite,
    MissingLateEntryEDXForSecondWrite,
    CompletedToBoundaryBeforeDec
}

public readonly record struct CapacityFinalCalleeCommonRingInsertionInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public readonly record struct CapacityFinalCalleeCommonRingInsertionWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public sealed record CapacityFinalCalleeCommonRingInsertionPlan(
    CapacityFinalCalleeCommonRingInsertionCapture Capture,
    CapacityFinalCalleeCommonRingInsertionOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeCommonRingInsertionWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonRingInsertionWrite>.Empty;
    public uint EntryRawFileOffset { get; init; }
    public uint BoundaryRawFileOffsetExclusive { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonRingInsertionInstructionEvidence>
        InstructionEvidence { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonRingInsertionInstructionEvidence>.Empty;
    public bool EntryEAXReadReached { get; init; }
    public bool EntryESIReadReached { get; init; }
    public bool LateEntryEDXReadReached { get; init; }
    public bool UnreachedLateEntryEDXCaptureProvided { get; init; }
    public bool LateEntryEDXUnusedBecauseFirstStoreBlocked { get; init; }
    public bool BoundaryReached { get; init; }
    public bool BoundaryInstructionEntered { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool DestinationEntered { get; init; }
    public bool MovInstructionsPreserveFlags { get; init; }
    public bool StandaloneEntryFlagsKnown { get; init; }
    public bool IncomingExternalFlagsCanBeRetained { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the five ordered common-ring insertion stores, stopping before the DEC at 012C942F.
/// </summary>
public static class CapacityFinalCalleeCommonRingInsertionProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C941C;
    public const uint EntryFlatIdaAddress = 0x012C941C;
    public const uint EntryVirtualAddress = 0x016CA01C;
    public const uint FirstWriteRawFileOffset = 0x012C941C;
    public const uint SecondWriteRawFileOffset = 0x012C941F;
    public const uint ThirdWriteRawFileOffset = 0x012C9422;
    public const uint FourthWriteRawFileOffset = 0x012C9425;
    public const uint FifthWriteRawFileOffset = 0x012C9428;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C942F;

    public const uint EAXDisplacement = 0xC;
    public const uint EDXDisplacement = 8;
    public const uint ESIPlus8Displacement = 8;
    public const uint ESIPlusCDisplacement = 0xC;
    public const uint ESIPlus18Displacement = 0x18;

    public const string CapturedWindowBytes =
        "89 70 0C 89 72 08 89 46 08 89 56 0C C7 46 18 00 00 00 00";

    public static ImmutableArray<CapacityFinalCalleeCommonRingInsertionInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Store entry ESI at entry EAX+C",
                FirstWriteRawFileOffset,
                "89 70 0C"),
            Instruction(
                "Store entry ESI at late entry EDX+8",
                SecondWriteRawFileOffset,
                "89 72 08"),
            Instruction(
                "Store entry EAX at entry ESI+8",
                ThirdWriteRawFileOffset,
                "89 46 08"),
            Instruction(
                "Store entry EDX at entry ESI+C",
                FourthWriteRawFileOffset,
                "89 56 0C"),
            Instruction(
                "Store zero at entry ESI+18",
                FifthWriteRawFileOffset,
                "C7 46 18 00 00 00 00"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonRingInsertionPlan Project(
        CapacityFinalCalleeCommonRingInsertionCapture capture)
    {
        var writes = ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonRingInsertionWrite>();
        bool entryEAXReadReached = false;
        bool entryESIReadReached = false;
        bool lateEntryEDXReadReached = false;

        CapacityFinalCalleeCommonRingInsertionPlan Finish(
            CapacityFinalCalleeCommonRingInsertionOutcome outcome,
            uint stopAt,
            bool exactCleanCompletion = false,
            bool boundaryReached = false) =>
            new(capture, outcome)
            {
                Writes = writes.ToImmutable(),
                EntryRawFileOffset = EntryRawFileOffset,
                BoundaryRawFileOffsetExclusive = BoundaryRawFileOffsetExclusive,
                InstructionEvidence = Instructions,
                EntryEAXReadReached = entryEAXReadReached,
                EntryESIReadReached = entryESIReadReached,
                LateEntryEDXReadReached = lateEntryEDXReadReached,
                UnreachedLateEntryEDXCaptureProvided =
                    !lateEntryEDXReadReached && capture.LateEntryEDX.HasValue,
                LateEntryEDXUnusedBecauseFirstStoreBlocked =
                    !entryEAXReadReached
                    || !entryESIReadReached
                    || !capture.EntryEAX.HasValue
                    || !capture.EntryESI.HasValue,
                BoundaryReached = boundaryReached,
                BoundaryInstructionEntered = false,
                ExactCleanCompletion = exactCleanCompletion,
                DestinationEntered = false,
                MovInstructionsPreserveFlags = true,
                StandaloneEntryFlagsKnown = false,
                IncomingExternalFlagsCanBeRetained = true,
                StopAtRawFileOffsetExclusive = stopAt
            };

        entryEAXReadReached = true;
        entryESIReadReached = true;
        AddWrite(
            writes,
            FirstWriteRawFileOffset,
            Add(capture.EntryEAX, EAXDisplacement),
            capture.EntryEAX.HasValue,
            capture.EntryESI,
            capture.EntryESI.HasValue);
        if (!capture.EntryEAX.HasValue)
        {
            return Finish(
                CapacityFinalCalleeCommonRingInsertionOutcome.MissingEntryEAXForFirstWrite,
                FirstWriteRawFileOffset);
        }

        if (!capture.EntryESI.HasValue)
        {
            return Finish(
                CapacityFinalCalleeCommonRingInsertionOutcome.MissingEntryESIForFirstWrite,
                FirstWriteRawFileOffset);
        }

        lateEntryEDXReadReached = true;
        AddWrite(
            writes,
            SecondWriteRawFileOffset,
            Add(capture.LateEntryEDX, EDXDisplacement),
            capture.LateEntryEDX.HasValue,
            capture.EntryESI,
            capture.EntryESI.HasValue);
        if (!capture.LateEntryEDX.HasValue)
        {
            return Finish(
                CapacityFinalCalleeCommonRingInsertionOutcome.MissingLateEntryEDXForSecondWrite,
                SecondWriteRawFileOffset);
        }

        AddWrite(
            writes,
            ThirdWriteRawFileOffset,
            Add(capture.EntryESI, ESIPlus8Displacement),
            capture.EntryESI.HasValue,
            capture.EntryEAX,
            capture.EntryEAX.HasValue);
        AddWrite(
            writes,
            FourthWriteRawFileOffset,
            Add(capture.EntryESI, ESIPlusCDisplacement),
            capture.EntryESI.HasValue,
            capture.LateEntryEDX,
            capture.LateEntryEDX.HasValue);
        AddWrite(
            writes,
            FifthWriteRawFileOffset,
            Add(capture.EntryESI, ESIPlus18Displacement),
            capture.EntryESI.HasValue,
            0,
            valueCaptured: true);

        return Finish(
            CapacityFinalCalleeCommonRingInsertionOutcome.CompletedToBoundaryBeforeDec,
            BoundaryRawFileOffsetExclusive,
            exactCleanCompletion: true,
            boundaryReached: true);
    }

    private static uint? Add(uint? value, uint displacement) =>
        value is uint address ? unchecked(address + displacement) : null;

    private static void AddWrite(
        ImmutableArray<CapacityFinalCalleeCommonRingInsertionWrite>.Builder writes,
        uint rawFileOffset,
        uint? address,
        bool addressCaptured,
        uint? value,
        bool valueCaptured) =>
        writes.Add(new(
            rawFileOffset,
            address,
            addressCaptured,
            valueCaptured,
            value));

    private static CapacityFinalCalleeCommonRingInsertionInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes);
}
