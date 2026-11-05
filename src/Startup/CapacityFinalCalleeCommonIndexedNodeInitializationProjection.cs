using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonIndexedNodeInitializationCapture(
    uint? EntryESI,
    uint? EntryECX);

public enum CapacityFinalCalleeCommonIndexedNodeInitializationOutcome
{
    MissingBothEntryOperands,
    MissingEntryESI,
    MissingEntryECX,
    Completed
}

public readonly record struct CapacityFinalCalleeCommonIndexedNodeInitializationWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeCommonIndexedNodeInitializationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes);

public sealed record CapacityFinalCalleeCommonIndexedNodeInitializationPlan(
    CapacityFinalCalleeCommonIndexedNodeInitializationCapture Capture,
    CapacityFinalCalleeCommonIndexedNodeInitializationOutcome Outcome)
{
    public bool EntryESICaptured { get; init; }
    public bool EntryECXCaptured { get; init; }
    public uint? FirstStoreAddress { get; init; }
    public uint? FirstStoreValue { get; init; }
    public bool FirstStoreReached { get; init; }
    public bool FirstStoreAttempted => true;
    public bool FirstStoreCommitted => !Writes.IsEmpty;
    public bool LEAReached { get; init; }
    public uint? LEAIndex { get; init; }
    public uint? LEAScaledIndex { get; init; }
    public uint? LEABaseAddress { get; init; }
    public uint? DerivedEBX { get; init; }
    public bool AddressOnlyLEA { get; init; }
    public bool MemoryReadPerformed => false;
    public bool MoveEDXReached { get; init; }
    public uint? DerivedEDX { get; init; }
    public bool SecondStoreReached { get; init; }
    public uint? SecondStoreAddress { get; init; }
    public uint? SecondStoreValue { get; init; }
    public bool ThirdStoreReached { get; init; }
    public uint? ThirdStoreAddress { get; init; }
    public uint? ThirdStoreValue { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonIndexedNodeInitializationWrite> Writes
        { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonIndexedNodeInitializationWrite>.Empty;
    public bool FlagsModified => false;
    public bool FlagsAtStopKnown => false;
    public bool InstructionBeyondBoundaryExecuted => false;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the bounded indexed-node initialization stores at raw 9384..93A1.
/// </summary>
public static class CapacityFinalCalleeCommonIndexedNodeInitializationProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9384;
    public const uint EntryVirtualAddress = 0x016C9F84;
    public const uint FirstStoreRawFileOffset = 0x012C9384;
    public const uint LEARawFileOffset = 0x012C9387;
    public const uint MoveEDXRawFileOffset = 0x012C938E;
    public const uint SecondStoreRawFileOffset = 0x012C9393;
    public const uint ThirdStoreRawFileOffset = 0x012C939A;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C93A1;

    public const uint LEABaseVirtualAddress = 0x01EDAEF8;
    public const uint FirstStoreFieldOffset = 0x1C;
    public const uint SecondStoreFieldOffset = 0x14;
    public const uint ThirdStoreFieldOffset = 0x10;
    public const uint MoveEDXImmediate = 1;

    public const string CapturedWindowBytes =
        "89 4E 1C 8D 1C 8D F8 AE ED 01 BA 01 00 00 00 "
        + "C7 46 14 00 00 00 00 C7 46 10 00 00 00 00";

    public static ImmutableArray<CapacityFinalCalleeCommonIndexedNodeInitializationInstructionEvidence>
        Instructions { get; } =
            ImmutableArray.Create(
                Instruction(
                    "Store entry ECX at [entry ESI+1Ch]",
                    FirstStoreRawFileOffset,
                    "89 4E 1C"),
                Instruction(
                    "Form EBX = entry ECX*4 + 01EDAEF8h with address-only LEA",
                    LEARawFileOffset,
                    "8D 1C 8D F8 AE ED 01"),
                Instruction(
                    "Set EDX to 1",
                    MoveEDXRawFileOffset,
                    "BA 01 00 00 00"),
                Instruction(
                    "Store zero at [entry ESI+14h]",
                    SecondStoreRawFileOffset,
                    "C7 46 14 00 00 00 00"),
                Instruction(
                    "Store zero at [entry ESI+10h]",
                    ThirdStoreRawFileOffset,
                    "C7 46 10 00 00 00 00"));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveEBX(uint ecx) =>
        unchecked((ecx << 2) + LEABaseVirtualAddress);

    public static CapacityFinalCalleeCommonIndexedNodeInitializationPlan Project(
        CapacityFinalCalleeCommonIndexedNodeInitializationCapture capture)
    {
        bool esiCaptured = capture.EntryESI.HasValue;
        bool ecxCaptured = capture.EntryECX.HasValue;
        uint? firstStoreAddress = capture.EntryESI is uint esi
            ? unchecked(esi + FirstStoreFieldOffset)
            : null;
        uint? firstStoreValue = capture.EntryECX;

        CapacityFinalCalleeCommonIndexedNodeInitializationPlan Finish(
            CapacityFinalCalleeCommonIndexedNodeInitializationOutcome outcome,
            bool firstStoreReached = false,
            bool leaReached = false,
            uint? leaIndex = null,
            uint? leaScaledIndex = null,
            uint? derivedEBX = null,
            bool moveEDXReached = false,
            uint? derivedEDX = null,
            bool secondStoreReached = false,
            uint? secondStoreAddress = null,
            uint? secondStoreValue = null,
            bool thirdStoreReached = false,
            uint? thirdStoreAddress = null,
            uint? thirdStoreValue = null,
            ImmutableArray<CapacityFinalCalleeCommonIndexedNodeInitializationWrite> writes =
                default,
            uint stopAt = EntryRawFileOffset) =>
                new(capture, outcome)
                {
                    EntryESICaptured = esiCaptured,
                    EntryECXCaptured = ecxCaptured,
                    FirstStoreAddress = firstStoreAddress,
                    FirstStoreValue = firstStoreValue,
                    FirstStoreReached = firstStoreReached,
                    LEAReached = leaReached,
                    LEAIndex = leaIndex,
                    LEAScaledIndex = leaScaledIndex,
                    LEABaseAddress = leaReached ? LEABaseVirtualAddress : null,
                    DerivedEBX = derivedEBX,
                    AddressOnlyLEA = leaReached,
                    MoveEDXReached = moveEDXReached,
                    DerivedEDX = derivedEDX,
                    SecondStoreReached = secondStoreReached,
                    SecondStoreAddress = secondStoreAddress,
                    SecondStoreValue = secondStoreValue,
                    ThirdStoreReached = thirdStoreReached,
                    ThirdStoreAddress = thirdStoreAddress,
                    ThirdStoreValue = thirdStoreValue,
                    Writes = writes.IsDefault
                        ? ImmutableArray<
                            CapacityFinalCalleeCommonIndexedNodeInitializationWrite>.Empty
                        : writes,
                    StopAtRawFileOffsetExclusive = stopAt
                };

        if (!esiCaptured && !ecxCaptured)
        {
            return Finish(
                CapacityFinalCalleeCommonIndexedNodeInitializationOutcome
                    .MissingBothEntryOperands);
        }

        if (!esiCaptured)
        {
            return Finish(
                CapacityFinalCalleeCommonIndexedNodeInitializationOutcome.MissingEntryESI);
        }

        if (!ecxCaptured)
        {
            return Finish(
                CapacityFinalCalleeCommonIndexedNodeInitializationOutcome.MissingEntryECX);
        }

        uint entryESI = capture.EntryESI!.Value;
        uint entryECX = capture.EntryECX!.Value;
        uint firstAddress = firstStoreAddress!.Value;
        uint derivedEBX = DeriveEBX(entryECX);
        uint secondAddress = unchecked(entryESI + SecondStoreFieldOffset);
        uint thirdAddress = unchecked(entryESI + ThirdStoreFieldOffset);

        ImmutableArray<CapacityFinalCalleeCommonIndexedNodeInitializationWrite> writes =
            ImmutableArray.Create(
                new CapacityFinalCalleeCommonIndexedNodeInitializationWrite(
                    FirstStoreRawFileOffset,
                    firstAddress,
                    entryECX),
                new CapacityFinalCalleeCommonIndexedNodeInitializationWrite(
                    SecondStoreRawFileOffset,
                    secondAddress,
                    0),
                new CapacityFinalCalleeCommonIndexedNodeInitializationWrite(
                    ThirdStoreRawFileOffset,
                    thirdAddress,
                    0));

        return Finish(
            CapacityFinalCalleeCommonIndexedNodeInitializationOutcome.Completed,
            firstStoreReached: true,
            leaReached: true,
            leaIndex: entryECX,
            leaScaledIndex: unchecked(entryECX << 2),
            derivedEBX: derivedEBX,
            moveEDXReached: true,
            derivedEDX: MoveEDXImmediate,
            secondStoreReached: true,
            secondStoreAddress: secondAddress,
            secondStoreValue: 0,
            thirdStoreReached: true,
            thirdStoreAddress: thirdAddress,
            thirdStoreValue: 0,
            writes: writes,
            stopAt: BoundaryRawFileOffsetExclusive);
    }

    private static CapacityFinalCalleeCommonIndexedNodeInitializationInstructionEvidence
        Instruction(string observation, uint rawFileOffset, string bytes) =>
            new(
                observation,
                rawFileOffset,
                rawFileOffset,
                RawFileOffsetToVirtualAddress(rawFileOffset),
                bytes);
}
