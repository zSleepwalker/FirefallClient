using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeCommonRingWritesCapture(
    uint? EntryEDX,
    uint? EntryEAX,
    uint? EntryESI);

public enum CapacityFinalCalleeCommonRingWritesOutcome
{
    MissingEntryEDXForFirstWrite,
    MissingEntryESIForFirstWrite,
    MissingEntryEAXForSecondWrite,
    CompletedTo012C9441
}

public readonly record struct CapacityFinalCalleeCommonRingWritesInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public readonly record struct CapacityFinalCalleeCommonRingWrite(
    uint RawFileOffset,
    uint? Address,
    bool AddressCaptured,
    bool ValueCaptured,
    uint? Value)
{
    public bool Committed => AddressCaptured && ValueCaptured;
}

public sealed record CapacityFinalCalleeCommonRingWritesPlan(
    CapacityFinalCalleeCommonRingWritesCapture Capture,
    CapacityFinalCalleeCommonRingWritesOutcome Outcome)
{
    public ImmutableArray<CapacityFinalCalleeCommonRingWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeCommonRingWrite>.Empty;
    public uint EntryRawFileOffset { get; init; }
    public uint BranchNextRawFileOffset { get; init; }
    public ImmutableArray<CapacityFinalCalleeCommonRingWritesInstructionEvidence>
        InstructionEvidence { get; init; } =
            ImmutableArray<CapacityFinalCalleeCommonRingWritesInstructionEvidence>.Empty;
    public bool EntryEDXReadReached { get; init; }
    public bool EntryEAXReadReached { get; init; }
    public bool EntryESIReadReached { get; init; }
    public bool UnreachedEntryEAXCaptureProvided { get; init; }
    public bool BranchReached { get; init; }
    public uint? BranchTargetRawFileOffset { get; init; }
    public bool FlagsUnaffected { get; init; }
    public bool ExactCleanCompletion { get; init; }
    public bool DestinationEntered { get; init; }
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

internal readonly record struct CapacityFinalCalleeCommonRingWritesRoute(
    uint EntryRawFileOffset,
    uint FirstWriteRawFileOffset,
    uint SecondWriteRawFileOffset,
    uint ThirdWriteRawFileOffset,
    uint FourthWriteRawFileOffset,
    uint BranchRawFileOffset,
    uint BranchNextRawFileOffset,
    uint BranchTargetRawFileOffset,
    ImmutableArray<CapacityFinalCalleeCommonRingWritesInstructionEvidence>
        InstructionEvidence);

/// <summary>
/// Projects the four ordered DWORD stores and their unconditional branch.
/// </summary>
public static class CapacityFinalCalleeCommonRingWritesProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9323;
    public const uint EntryFlatIdaAddress = 0x012C9323;
    public const uint EntryVirtualAddress = 0x016C9F23;
    public const uint FirstWriteRawFileOffset = 0x012C9323;
    public const uint SecondWriteRawFileOffset = 0x012C9326;
    public const uint ThirdWriteRawFileOffset = 0x012C9329;
    public const uint FourthWriteRawFileOffset = 0x012C932C;
    public const uint BranchRawFileOffset = 0x012C932F;
    public const uint BranchNextRawFileOffset = 0x012C9334;
    public const uint BranchTargetRawFileOffset = 0x012C9441;
    public const uint BoundaryRawFileOffsetExclusive = BranchTargetRawFileOffset;

    public const uint EDXDisplacement = 8;
    public const uint EAXDisplacement = 0xC;
    public const uint ESIPlus8Displacement = 8;
    public const uint ESIPlusCDisplacement = 0xC;

    public const string CapturedWindowBytes =
        "89 72 08 89 70 0C 89 46 08 89 56 0C E9 0D 01 00 00";

    public static ImmutableArray<CapacityFinalCalleeCommonRingWritesInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction(
                "Store entry ESI at entry EDX+8",
                FirstWriteRawFileOffset,
                "89 72 08"),
            Instruction(
                "Store entry ESI at entry EAX+C",
                SecondWriteRawFileOffset,
                "89 70 0C"),
            Instruction(
                "Store entry EAX at entry ESI+8",
                ThirdWriteRawFileOffset,
                "89 46 08"),
            Instruction(
                "Store entry EDX at entry ESI+C",
                FourthWriteRawFileOffset,
                "89 56 0C"),
            Instruction(
                "Jump to raw offset 012C9441",
                BranchRawFileOffset,
                "E9 0D 01 00 00",
                BranchTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static CapacityFinalCalleeCommonRingWritesPlan Project(
        CapacityFinalCalleeCommonRingWritesCapture capture)
        => ProjectCore(
            capture,
            new(
                EntryRawFileOffset,
                FirstWriteRawFileOffset,
                SecondWriteRawFileOffset,
                ThirdWriteRawFileOffset,
                FourthWriteRawFileOffset,
                BranchRawFileOffset,
                BranchNextRawFileOffset,
                BranchTargetRawFileOffset,
                Instructions));

    internal static CapacityFinalCalleeCommonRingWritesPlan ProjectCore(
        CapacityFinalCalleeCommonRingWritesCapture capture,
        CapacityFinalCalleeCommonRingWritesRoute route)
    {
        var writes = ImmutableArray.CreateBuilder<CapacityFinalCalleeCommonRingWrite>();
        bool entryEDXReadReached = false;
        bool entryEAXReadReached = false;
        bool entryESIReadReached = false;
        bool branchReached = false;

        CapacityFinalCalleeCommonRingWritesPlan Finish(
            CapacityFinalCalleeCommonRingWritesOutcome outcome,
            uint stopAt,
            bool exactCleanCompletion = false) =>
            new(capture, outcome)
            {
                Writes = writes.ToImmutable(),
                EntryRawFileOffset = route.EntryRawFileOffset,
                BranchNextRawFileOffset = route.BranchNextRawFileOffset,
                InstructionEvidence = route.InstructionEvidence,
                EntryEDXReadReached = entryEDXReadReached,
                EntryEAXReadReached = entryEAXReadReached,
                EntryESIReadReached = entryESIReadReached,
                UnreachedEntryEAXCaptureProvided =
                    !entryEAXReadReached && capture.EntryEAX.HasValue,
                BranchReached = branchReached,
                BranchTargetRawFileOffset =
                    branchReached ? route.BranchTargetRawFileOffset : null,
                FlagsUnaffected = true,
                ExactCleanCompletion = exactCleanCompletion,
                DestinationEntered = false,
                StopAtRawFileOffsetExclusive = stopAt
            };

        entryEDXReadReached = true;
        entryESIReadReached = true;
        AddWrite(
            writes,
            route.FirstWriteRawFileOffset,
            Add(capture.EntryEDX, EDXDisplacement),
            capture.EntryEDX.HasValue,
            capture.EntryESI,
            capture.EntryESI.HasValue);
        if (!capture.EntryEDX.HasValue)
        {
            return Finish(
                CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryEDXForFirstWrite,
                route.FirstWriteRawFileOffset);
        }

        if (!capture.EntryESI.HasValue)
        {
            return Finish(
                CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryESIForFirstWrite,
                route.FirstWriteRawFileOffset);
        }

        entryEAXReadReached = true;
        entryESIReadReached = true;
        AddWrite(
            writes,
            route.SecondWriteRawFileOffset,
            Add(capture.EntryEAX, EAXDisplacement),
            capture.EntryEAX.HasValue,
            capture.EntryESI,
            capture.EntryESI.HasValue);
        if (!capture.EntryEAX.HasValue)
        {
            return Finish(
                CapacityFinalCalleeCommonRingWritesOutcome.MissingEntryEAXForSecondWrite,
                route.SecondWriteRawFileOffset);
        }

        entryESIReadReached = true;
        entryEAXReadReached = true;
        AddWrite(
            writes,
            route.ThirdWriteRawFileOffset,
            Add(capture.EntryESI, ESIPlus8Displacement),
            capture.EntryESI.HasValue,
            capture.EntryEAX,
            capture.EntryEAX.HasValue);

        entryESIReadReached = true;
        entryEDXReadReached = true;
        AddWrite(
            writes,
            route.FourthWriteRawFileOffset,
            Add(capture.EntryESI, ESIPlusCDisplacement),
            capture.EntryESI.HasValue,
            capture.EntryEDX,
            capture.EntryEDX.HasValue);

        branchReached = true;
        return Finish(
            CapacityFinalCalleeCommonRingWritesOutcome.CompletedTo012C9441,
            route.BranchTargetRawFileOffset,
            exactCleanCompletion: true);
    }

    private static uint? Add(uint? value, uint displacement) =>
        value is uint address ? unchecked(address + displacement) : null;

    private static void AddWrite(
        ImmutableArray<CapacityFinalCalleeCommonRingWrite>.Builder writes,
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

    private static CapacityFinalCalleeCommonRingWritesInstructionEvidence Instruction(
        string observation,
        uint rawFileOffset,
        string bytes,
        uint? branchTargetRawFileOffset = null) =>
        new(
            observation,
            rawFileOffset,
            rawFileOffset,
            RawFileOffsetToVirtualAddress(rawFileOffset),
            bytes,
            branchTargetRawFileOffset,
            branchTargetRawFileOffset is uint targetRaw
                ? RawFileOffsetToVirtualAddress(targetRaw)
                : null);
}
