using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct CapacityFinalCalleeAlternateSuccessorWritesCapture(
    uint? EntryECX,
    uint? EntryEAX);

public enum CapacityFinalCalleeAlternateSuccessorWritesOutcome
{
    MissingEntryECX,
    MissingEntryEAX,
    MissingEntryECXAndEAX,
    Completed
}

public readonly record struct CapacityFinalCalleeAlternateSuccessorWritesWrite(
    uint RawFileOffset,
    uint Address,
    uint Value);

public readonly record struct CapacityFinalCalleeAlternateSuccessorWritesInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    uint FlatIdaAddress,
    uint VirtualAddress,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? BranchTargetVirtualAddress = null);

public sealed record CapacityFinalCalleeAlternateSuccessorWritesPlan(
    CapacityFinalCalleeAlternateSuccessorWritesCapture Capture,
    CapacityFinalCalleeAlternateSuccessorWritesOutcome Outcome)
{
    public bool EntryECXCaptured => Capture.EntryECX.HasValue;
    public bool EntryEAXCaptured => Capture.EntryEAX.HasValue;
    public bool FirstStoreReached { get; init; }
    public uint? FirstStoreAddress { get; init; }
    public uint? FirstStoreValue { get; init; }
    public bool SecondStoreReached { get; init; }
    public uint? SecondStoreAddress { get; init; }
    public uint? SecondStoreValue { get; init; }
    public bool JumpReached { get; init; }
    public uint? JumpTargetRawFileOffset { get; init; }
    public uint? JumpTargetVirtualAddress { get; init; }
    public bool ExactCleanExitAdmitted { get; init; }
    public bool FlagsPreserved => true;
    public ImmutableArray<CapacityFinalCalleeAlternateSuccessorWritesWrite> Writes { get; init; } =
        ImmutableArray<CapacityFinalCalleeAlternateSuccessorWritesWrite>.Empty;
    public uint StopAtRawFileOffsetExclusive { get; init; }
}

/// <summary>
/// Projects the two ordered successor stores and the exact jump to 92BD.
/// It performs no memory reads or writes; captured register values are
/// projected as writes, and MOV/JMP preserve flags.
/// </summary>
public static class CapacityFinalCalleeAlternateSuccessorWritesProjection
{
    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint EntryRawFileOffset = 0x012C9190;
    public const uint EntryVirtualAddress = 0x016C9D90;
    public const uint FirstStoreRawFileOffset = 0x012C9190;
    public const uint SecondStoreRawFileOffset = 0x012C9193;
    public const uint JumpRawFileOffset = 0x012C9196;
    public const uint BoundaryRawFileOffsetExclusive = 0x012C919B;
    public const uint JumpTargetRawFileOffset = 0x012C92BD;
    public const uint JumpTargetVirtualAddress = 0x016C9EBD;
    public const uint FirstStoreDisplacement = 0x0C;
    public const uint SecondStoreDisplacement = 0x08;

    public const string CapturedWindowBytes =
        "89 41 0C 89 48 08 E9 22 01 00 00";

    public static ImmutableArray<CapacityFinalCalleeAlternateSuccessorWritesInstructionEvidence>
        Instructions { get; } =
        ImmutableArray.Create(
            Instruction("Store entry EAX at entry ECX+0C",
                FirstStoreRawFileOffset, "89 41 0C"),
            Instruction("Store entry ECX at entry EAX+8",
                SecondStoreRawFileOffset, "89 48 08"),
            Instruction("Jump to the exact 92BD bounded exit",
                JumpRawFileOffset, "E9 22 01 00 00",
                branchTargetRawFileOffset: JumpTargetRawFileOffset));

    public static uint RawFileOffsetToVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset);

    public static uint DeriveFirstStoreAddress(uint entryEcx) =>
        unchecked(entryEcx + FirstStoreDisplacement);

    public static uint DeriveSecondStoreAddress(uint entryEax) =>
        unchecked(entryEax + SecondStoreDisplacement);

    public static CapacityFinalCalleeAlternateSuccessorWritesPlan Project(
        CapacityFinalCalleeAlternateSuccessorWritesCapture capture)
    {
        if (capture.EntryECX is not uint entryEcx
            || capture.EntryEAX is not uint entryEax)
        {
            CapacityFinalCalleeAlternateSuccessorWritesOutcome outcome =
                (capture.EntryECX, capture.EntryEAX) switch
                {
                    (null, null) =>
                        CapacityFinalCalleeAlternateSuccessorWritesOutcome
                            .MissingEntryECXAndEAX,
                    (null, _) =>
                        CapacityFinalCalleeAlternateSuccessorWritesOutcome
                            .MissingEntryECX,
                    (_, null) =>
                        CapacityFinalCalleeAlternateSuccessorWritesOutcome
                            .MissingEntryEAX,
                    _ => throw new InvalidOperationException()
                };
            return new(capture, outcome)
            {
                StopAtRawFileOffsetExclusive = EntryRawFileOffset
            };
        }

        uint firstStoreAddress = DeriveFirstStoreAddress(entryEcx);
        uint secondStoreAddress = DeriveSecondStoreAddress(entryEax);
        return new(
            capture,
            CapacityFinalCalleeAlternateSuccessorWritesOutcome.Completed)
        {
            FirstStoreReached = true,
            FirstStoreAddress = firstStoreAddress,
            FirstStoreValue = entryEax,
            SecondStoreReached = true,
            SecondStoreAddress = secondStoreAddress,
            SecondStoreValue = entryEcx,
            JumpReached = true,
            JumpTargetRawFileOffset = JumpTargetRawFileOffset,
            JumpTargetVirtualAddress = JumpTargetVirtualAddress,
            ExactCleanExitAdmitted = true,
            Writes = ImmutableArray.Create(
                new CapacityFinalCalleeAlternateSuccessorWritesWrite(
                    FirstStoreRawFileOffset, firstStoreAddress, entryEax),
                new CapacityFinalCalleeAlternateSuccessorWritesWrite(
                    SecondStoreRawFileOffset, secondStoreAddress, entryEcx)),
            StopAtRawFileOffsetExclusive = JumpTargetRawFileOffset
        };
    }

    private static CapacityFinalCalleeAlternateSuccessorWritesInstructionEvidence
        Instruction(
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
