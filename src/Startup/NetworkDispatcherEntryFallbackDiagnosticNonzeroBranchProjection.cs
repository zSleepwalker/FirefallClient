using System.Collections.Immutable;
using System.Numerics;
using System.Threading;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCapture(
    NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCapture ParentCapture,
    uint? FreshDWORDAtESIPlusA4,
    uint? FreshArrayEntryDWORDAtAddress);

public enum NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome
{
    ParentCallTargetPrefixNotAccepted,
    JzTaken,
    MissingFreshDWORDAtESIPlusA4,
    MissingFreshArrayEntryDWORD,
    BoundedBeforeJnz
}

public enum NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
{
    MoveEaxFromEsiPlusA4,
    MoveEdiFromEaxPlusScaledEbx,
    TestEdiEdi
}

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchRead(
    uint InstructionRawFileOffset,
    uint Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value);

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstruction(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind Kind,
    uint? EAXBefore,
    uint? EAXAfter,
    uint? EBXBefore,
    uint? EBXAfter,
    uint? EDIBefore,
    uint? EDIAfter,
    bool Attempted,
    bool Reached,
    bool Committed,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter);

public sealed class NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan
{
    private readonly Lazy<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan>
        reprojectedParentPlan;
    private readonly Lazy<NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan>
        reprojectedPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCapture capture,
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan parentPlan,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome outcome,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
            dwordAtA4CaptureState,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
            arrayEntryCaptureState,
        ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchRead> reads,
        ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstruction>
            instructions,
        ImmutableArray<uint> completedInstructionOffsets,
        uint? stopAtRawFileOffsetExclusive,
        uint? esi,
        uint? eax,
        uint? ebx,
        uint? edi,
        NetworkDispatcherLockGateFlags? flagsAtStop,
        bool branchNotTakenAccepted,
        bool jnzBoundaryReached,
        bool projectionEntered)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Outcome = outcome;
        DWORDAtA4CaptureState = dwordAtA4CaptureState;
        ArrayEntryCaptureState = arrayEntryCaptureState;
        Reads = reads;
        Instructions = instructions;
        CompletedInstructionOffsets = completedInstructionOffsets;
        StopAtRawFileOffsetExclusive = stopAtRawFileOffsetExclusive;
        ESI = esi;
        EAX = eax;
        EBX = ebx;
        EDI = edi;
        FlagsAtStop = flagsAtStop;
        BranchNotTakenAccepted = branchNotTakenAccepted;
        JnzBoundaryReached = jnzBoundaryReached;
        ProjectionEntered = projectionEntered;
        reprojectedParentPlan = new(
            () => NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.Project(
                Capture.ParentCapture),
            LazyThreadSafetyMode.ExecutionAndPublication);
        reprojectedPlan = new(
            () => NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection.Project(
                Capture),
            LazyThreadSafetyMode.ExecutionAndPublication);
        orderedSemanticPrefixPreserved = new(
            ComputeOrderedSemanticPrefixPreserved,
            LazyThreadSafetyMode.ExecutionAndPublication);
        exactBoundedCompletion = new(
            ComputeExactBoundedCompletion,
            LazyThreadSafetyMode.ExecutionAndPublication);
        exactCleanCompletion = new(
            ComputeExactCleanCompletion,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCapture Capture { get; }
    public NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan ParentPlan { get; }
    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome Outcome { get; }
    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
        DWORDAtA4CaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
        ArrayEntryCaptureState { get; }
    public ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchRead> Reads
    {
        get;
    }
    public ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstruction>
        Instructions { get; }
    public ImmutableArray<uint> CompletedInstructionOffsets { get; }
    public uint EntryRawFileOffset =>
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection.EntryRawFileOffset;
    public uint? StopAtRawFileOffsetExclusive { get; }
    public uint? ESI { get; }
    public uint? EAX { get; }
    public uint? EBX { get; }
    public uint? EDI { get; }
    public NetworkDispatcherLockGateFlags? FlagsAtStop { get; }
    public bool BranchNotTakenAccepted { get; }
    public bool JnzBoundaryReached { get; }
    public bool ConditionalBranchProjected => false;
    public bool ProjectionEntered { get; }
    public bool ParentInstructionsExecuted => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryReadsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;

    private bool ComputeExactBoundedCompletion() =>
        ProjectionEntered
        && ParentPlan.ExactCleanCompletion
        && BranchNotTakenAccepted
        && Outcome == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome
            .BoundedBeforeJnz
        && DWORDAtA4CaptureState
            == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Captured
        && ArrayEntryCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Captured
        && Instructions.Length
            == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .MaximumProjectedInstructionCount
        && Instructions.All(instruction => instruction is { Reached: true, Committed: true })
        && CompletedInstructionOffsets.SequenceEqual(
            ParentPlan.CompletedInstructionOffsets
                .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .MoveEaxRawFileOffset)
                .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .MoveEdiRawFileOffset)
                .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .TestEdiRawFileOffset))
        && CompletedInstructionOffsets[^1]
            == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .TestEdiRawFileOffset
        && StopAtRawFileOffsetExclusive
            == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .BranchRawFileOffset
        && JnzBoundaryReached
        && ESI == ParentPlan.ESI
        && EBX == ParentPlan.EBX
        && EAX == Capture.FreshDWORDAtESIPlusA4
        && EDI == Capture.FreshArrayEntryDWORDAtAddress
        && FlagsAtStop == TestFlags(Capture.FreshArrayEntryDWORDAtAddress!.Value);

    private bool ComputeExactCleanCompletion() =>
        ComputeExactBoundedCompletion()
        && OrderedSemanticPrefixPreserved
        && Reads.Length == 2
        && Reads.All(read => read is { Attempted: true, Captured: true })
        && Instructions[0] is {
            InstructionRawFileOffset:
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .MoveEaxRawFileOffset,
            Kind: NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                .MoveEaxFromEsiPlusA4,
            Committed: true
        }
        && Instructions[1] is {
            InstructionRawFileOffset:
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .MoveEdiRawFileOffset,
            Kind: NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                .MoveEdiFromEaxPlusScaledEbx,
            Committed: true
        }
        && Instructions[2] is {
            InstructionRawFileOffset:
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .TestEdiRawFileOffset,
            Kind: NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                .TestEdiEdi,
            Committed: true
        };

    private bool ComputeOrderedSemanticPrefixPreserved()
    {
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan actualParent = ParentPlan;
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan replayParent =
            reprojectedParentPlan.Value;
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan replay =
            reprojectedPlan.Value;
        if (actualParent.Capture != replayParent.Capture
            || actualParent.Outcome != replayParent.Outcome
            || actualParent.EsiCaptureState != replayParent.EsiCaptureState
            || actualParent.DWORDAtA8CaptureState != replayParent.DWORDAtA8CaptureState
            || actualParent.DWORDAtA4CaptureState != replayParent.DWORDAtA4CaptureState
            || !actualParent.Reads.SequenceEqual(replayParent.Reads)
            || !actualParent.Instructions.SequenceEqual(replayParent.Instructions)
            || !actualParent.CompletedInstructionOffsets.SequenceEqual(
                replayParent.CompletedInstructionOffsets)
            || actualParent.StopAtRawFileOffsetExclusive
                != replayParent.StopAtRawFileOffsetExclusive
            || actualParent.ESI != replayParent.ESI
            || actualParent.EAX != replayParent.EAX
            || actualParent.EBX != replayParent.EBX
            || actualParent.FlagsAtStop != replayParent.FlagsAtStop
            || actualParent.BranchBoundaryReached != replayParent.BranchBoundaryReached
            || actualParent.ProjectionEntered != replayParent.ProjectionEntered
            || actualParent.ExactBoundedCompletion != replayParent.ExactBoundedCompletion
            || actualParent.ExactCleanCompletion != replayParent.ExactCleanCompletion
            || !actualParent.OrderedSemanticPrefixPreserved
            || Capture != replay.Capture
            || Outcome != replay.Outcome
            || DWORDAtA4CaptureState != replay.DWORDAtA4CaptureState
            || ArrayEntryCaptureState != replay.ArrayEntryCaptureState
            || !Reads.SequenceEqual(replay.Reads)
            || !Instructions.SequenceEqual(replay.Instructions)
            || !CompletedInstructionOffsets.SequenceEqual(
                replay.CompletedInstructionOffsets)
            || StopAtRawFileOffsetExclusive != replay.StopAtRawFileOffsetExclusive
            || ESI != replay.ESI
            || EAX != replay.EAX
            || EBX != replay.EBX
            || EDI != replay.EDI
            || FlagsAtStop != replay.FlagsAtStop
            || BranchNotTakenAccepted != replay.BranchNotTakenAccepted
            || JnzBoundaryReached != replay.JnzBoundaryReached
            || ProjectionEntered != replay.ProjectionEntered)
        {
            return false;
        }

        if (!ProjectionEntered)
        {
            bool jzTaken =
                ParentPlan.ExactCleanCompletion
                && ParentPlan.Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                        .BoundedBeforeJz
                && ParentPlan.BranchBoundaryReached
                && ParentPlan.StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .EntryJzRawFileOffset
                && ParentPlan.FlagsAtStop is { Zero: true }
                && ParentPlan.EAX == 0;
            return Outcome == (jzTaken
                    ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome.JzTaken
                    : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome
                        .ParentCallTargetPrefixNotAccepted)
                && Instructions.IsEmpty
                && Reads.IsEmpty
                && CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets)
                && StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive
                && FlagsAtStop == ParentPlan.FlagsAtStop
                && !BranchNotTakenAccepted
                && !JnzBoundaryReached
                && ESI is null
                && EAX is null
                && EBX is null
                && EDI is null
                && DWORDAtA4CaptureState == StateForUnused(Capture.FreshDWORDAtESIPlusA4)
                && ArrayEntryCaptureState
                    == StateForUnused(Capture.FreshArrayEntryDWORDAtAddress);
        }

        if (!BranchAdmitted(ParentPlan))
        {
            return false;
        }

        if (!BranchNotTakenAccepted || !ProjectionEntered)
        {
            return false;
        }

        NetworkDispatcherLockGateFlags inheritedFlags = ParentPlan.FlagsAtStop ?? UnknownFlags;
        if (Capture.FreshDWORDAtESIPlusA4 is not uint firstDWORD)
        {
            return Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome
                        .MissingFreshDWORDAtESIPlusA4
                && DWORDAtA4CaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Missing
                && ArrayEntryCaptureState
                    == StateForUnused(Capture.FreshArrayEntryDWORDAtAddress)
                && Reads is [{
                    InstructionRawFileOffset:
                        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                            .MoveEaxRawFileOffset,
                    Address: var firstAddress,
                    Attempted: true,
                    Captured: false,
                    Value: null
                }]
                && firstAddress == unchecked(ParentPlan.ESI!.Value
                    + NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .EsiA4Displacement)
                && Instructions is [{
                    InstructionRawFileOffset:
                        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                            .MoveEaxRawFileOffset,
                    EAXBefore: var eaxBefore,
                    EAXAfter: var eaxAfter,
                    Committed: false,
                    FlagsBefore: var flagsBefore,
                    FlagsAfter: var flagsAfter
                }]
                && eaxBefore == ParentPlan.EAX
                && eaxAfter == ParentPlan.EAX
                && flagsBefore == inheritedFlags
                && flagsAfter == inheritedFlags
                && CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets)
                && StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .MoveEaxRawFileOffset
                && ESI == ParentPlan.ESI
                && EAX == ParentPlan.EAX
                && EBX == ParentPlan.EBX
                && EDI is null
                && FlagsAtStop == ParentPlan.FlagsAtStop
                && !JnzBoundaryReached;
        }

        if (Capture.FreshArrayEntryDWORDAtAddress is not uint arrayEntryDWORD)
        {
            uint firstAddress = unchecked(firstDWORD
                + ParentPlan.EBX!.Value
                    * NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .ArrayIndexScale);
            return Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome
                        .MissingFreshArrayEntryDWORD
                && DWORDAtA4CaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
                        .Captured
                && ArrayEntryCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Missing
                && Reads.Length == 2
                && Reads[0].InstructionRawFileOffset
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .MoveEaxRawFileOffset
                && Reads[0].Address == unchecked(ParentPlan.ESI!.Value
                    + NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .EsiA4Displacement)
                && Reads[0].Value == firstDWORD
                && Reads[0].Captured
                && Reads[1] is {
                    InstructionRawFileOffset:
                        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                            .MoveEdiRawFileOffset,
                    Address: var secondAddress,
                    Attempted: true,
                    Captured: false,
                    Value: null
                }
                && secondAddress == firstAddress
                && Instructions.Length == 2
                && Instructions[0] is {
                    InstructionRawFileOffset:
                        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                            .MoveEaxRawFileOffset,
                    EAXAfter: var firstEAXAfter,
                    Committed: true
                }
                && firstEAXAfter == firstDWORD
                && Instructions[1] is {
                    InstructionRawFileOffset:
                        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                            .MoveEdiRawFileOffset,
                    Committed: false
                }
                && CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets.Add(
                        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                            .MoveEaxRawFileOffset))
                && StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .MoveEdiRawFileOffset
                && ESI == ParentPlan.ESI
                && EAX == firstDWORD
                && EBX == ParentPlan.EBX
                && EDI is null
                && FlagsAtStop == ParentPlan.FlagsAtStop
                && !JnzBoundaryReached;
        }

        uint completedArrayAddress = unchecked(firstDWORD
            + ParentPlan.EBX!.Value
                * NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .ArrayIndexScale);
        return Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome.BoundedBeforeJnz
            && DWORDAtA4CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Captured
            && ArrayEntryCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Captured
            && Reads.Length == 2
            && Reads[0] is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .MoveEaxRawFileOffset,
                Attempted: true,
                Captured: true,
                Value: var firstReadValue
            }
            && firstReadValue == firstDWORD
            && Reads[0].Address == unchecked(ParentPlan.ESI!.Value
                + NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .EsiA4Displacement)
            && Reads[1] is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .MoveEdiRawFileOffset,
                Address: var completedAddress,
                Attempted: true,
                Captured: true,
                Value: var secondReadValue
            }
            && completedAddress == completedArrayAddress
            && secondReadValue == arrayEntryDWORD
            && Instructions.Length == 3
            && Instructions[0] is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .MoveEaxRawFileOffset,
                Committed: true
            }
            && Instructions[1] is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .MoveEdiRawFileOffset,
                Committed: true
            }
            && Instructions[2] is {
                InstructionRawFileOffset:
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .TestEdiRawFileOffset,
                Kind: NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                    .TestEdiEdi,
                Committed: true
            }
            && CompletedInstructionOffsets.SequenceEqual(
                ParentPlan.CompletedInstructionOffsets
                    .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .MoveEaxRawFileOffset)
                    .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .MoveEdiRawFileOffset)
                    .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .TestEdiRawFileOffset))
            && StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .BranchRawFileOffset
            && ESI == ParentPlan.ESI
            && EAX == firstDWORD
            && EBX == ParentPlan.EBX
            && EDI == arrayEntryDWORD
            && FlagsAtStop == TestFlags(arrayEntryDWORD)
            && JnzBoundaryReached;
    }

    private static bool BranchAdmitted(
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan parent) =>
        parent.ExactCleanCompletion
        && parent.Outcome
            == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome.BoundedBeforeJz
        && parent.BranchBoundaryReached
        && parent.StopAtRawFileOffsetExclusive
            == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .EntryJzRawFileOffset
        && parent.EAX is uint eax
        && eax != 0
        && parent.EBX == 0
        && parent.ESI.HasValue
        && parent.FlagsAtStop is { Zero: false };

    private static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstruction Instruction(
        uint rawFileOffset,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind kind,
        uint? eaxBefore,
        uint? eaxAfter,
        uint? ebxBefore,
        uint? ebxAfter,
        uint? ediBefore,
        uint? ediAfter,
        NetworkDispatcherLockGateFlags flagsBefore,
        NetworkDispatcherLockGateFlags flagsAfter,
        bool committed)
    {
        (byte length, string bytes) = InstructionEncoding(kind);
        return new(
            rawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                .RawFileOffsetToCodeVirtualAddress(rawFileOffset),
            length,
            bytes,
            kind,
            eaxBefore,
            eaxAfter,
            ebxBefore,
            ebxAfter,
            ediBefore,
            ediAfter,
            Attempted: true,
            Reached: true,
            Committed: committed,
            FlagsBefore: flagsBefore,
            FlagsAfter: flagsAfter);
    }

    private static (byte Length, string Bytes) InstructionEncoding(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind kind) =>
        kind switch
        {
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                .MoveEaxFromEsiPlusA4 => (6,
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .MoveEaxInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                .MoveEdiFromEaxPlusScaledEbx => (3,
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                        .MoveEdiInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind.TestEdiEdi =>
                (2, NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
                    .TestEdiInstructionBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    private static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
        StateForUnreachedParent(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Unreached;

    private static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
        StateForUnused(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Unreached;

    private static NetworkDispatcherLockGateFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);

    private static NetworkDispatcherLockGateFlags TestFlags(uint value) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)value),
            Auxiliary: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;
}

/// <summary>
/// Reprojects the nonzero fallthrough at BBDE40 and stops before JNZ at BBDE4B.
/// </summary>
public static class NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
{
    public const uint EntryRawFileOffset = 0x00BBDE40;
    public const uint EntryJzRawFileOffset = 0x00BBDE3E;
    public const uint MoveEaxRawFileOffset = 0x00BBDE40;
    public const uint MoveEdiRawFileOffset = 0x00BBDE46;
    public const uint TestEdiRawFileOffset = 0x00BBDE49;
    public const uint BranchRawFileOffset = 0x00BBDE4B;
    public const uint BranchNextRawFileOffset = 0x00BBDE4D;
    public const uint BranchTargetRawFileOffset = 0x00BBDE66;
    public const uint EsiA4Displacement = 0xA4;
    public const byte ArrayIndexScale = sizeof(uint);
    public const byte DWORDWidth = sizeof(uint);
    public const byte BranchInstructionByteLength = 2;
    public const byte CoordinatorProvidedByteLength = 13;
    public const int MaximumProjectedInstructionCount = 3;
    public const string MoveEaxInstructionBytes = "8B86A4000000";
    public const string MoveEdiInstructionBytes = "8B3C98";
    public const string TestEdiInstructionBytes = "85FF";
    public const string BranchInstructionBytes = "7519";
    public const string CoordinatorProvidedBytes = "8B86A40000008B3C9885FF7519";
    public const bool BytesIndependentlyVerified = true;

    public static uint EntryCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(EntryRawFileOffset);
    public static uint BranchCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(BranchRawFileOffset);
    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan Project(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentCapture);

        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan parent =
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.Project(
                capture.ParentCapture);
        if (!BranchAdmitted(parent))
        {
            bool jzTaken =
                parent.ExactCleanCompletion
                && parent.Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                        .BoundedBeforeJz
                && parent.BranchBoundaryReached
                && parent.StopAtRawFileOffsetExclusive == EntryJzRawFileOffset
                && parent.FlagsAtStop is { Zero: true }
                && parent.EAX == 0;
            return new(
                capture,
                parent,
                jzTaken
                    ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome.JzTaken
                    : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome
                        .ParentCallTargetPrefixNotAccepted,
                StateForUnreachedParent(capture.FreshDWORDAtESIPlusA4),
                StateForUnreachedParent(capture.FreshArrayEntryDWORDAtAddress),
                ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchRead>.Empty,
                ImmutableArray<
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstruction>.Empty,
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive,
                null,
                null,
                null,
                null,
                parent.FlagsAtStop,
                branchNotTakenAccepted: false,
                jnzBoundaryReached: false,
                projectionEntered: false);
        }

        NetworkDispatcherLockGateFlags flags = parent.FlagsAtStop ?? UnknownFlags;
        uint esi = parent.ESI!.Value;
        uint eaxAtEntry = parent.EAX!.Value;
        uint ebx = parent.EBX!.Value;
        var firstRead = new NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchRead(
            MoveEaxRawFileOffset,
            unchecked(esi + EsiA4Displacement),
            DWORDWidth,
            Attempted: true,
            Captured: capture.FreshDWORDAtESIPlusA4.HasValue,
            Value: capture.FreshDWORDAtESIPlusA4);
        if (capture.FreshDWORDAtESIPlusA4 is not uint firstDWORD)
        {
            var attemptedMove = Instruction(
                MoveEaxRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                    .MoveEaxFromEsiPlusA4,
                eaxAtEntry,
                eaxAtEntry,
                ebx,
                ebx,
                null,
                null,
                flags,
                flags,
                committed: false);
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome
                    .MissingFreshDWORDAtESIPlusA4,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Missing,
                StateForUnused(capture.FreshArrayEntryDWORDAtAddress),
                ImmutableArray.Create(firstRead),
                ImmutableArray.Create(attemptedMove),
                parent.CompletedInstructionOffsets,
                MoveEaxRawFileOffset,
                esi,
                eaxAtEntry,
                ebx,
                null,
                flags,
                branchNotTakenAccepted: true,
                jnzBoundaryReached: false,
                projectionEntered: true);
        }

        var moveEax = Instruction(
            MoveEaxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                .MoveEaxFromEsiPlusA4,
            eaxAtEntry,
            firstDWORD,
            ebx,
            ebx,
            null,
            null,
            flags,
            flags,
            committed: true);
        uint arrayAddress = unchecked(firstDWORD + ebx * ArrayIndexScale);
        var secondRead = new NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchRead(
            MoveEdiRawFileOffset,
            arrayAddress,
            DWORDWidth,
            Attempted: true,
            Captured: capture.FreshArrayEntryDWORDAtAddress.HasValue,
            Value: capture.FreshArrayEntryDWORDAtAddress);
        if (capture.FreshArrayEntryDWORDAtAddress is not uint arrayEntryDWORD)
        {
            var attemptedMoveEdi = Instruction(
                MoveEdiRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                    .MoveEdiFromEaxPlusScaledEbx,
                firstDWORD,
                firstDWORD,
                ebx,
                ebx,
                null,
                null,
                flags,
                flags,
                committed: false);
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome
                    .MissingFreshArrayEntryDWORD,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Captured,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Missing,
                ImmutableArray.Create(firstRead, secondRead),
                ImmutableArray.Create(moveEax, attemptedMoveEdi),
                parent.CompletedInstructionOffsets.Add(MoveEaxRawFileOffset),
                MoveEdiRawFileOffset,
                esi,
                firstDWORD,
                ebx,
                null,
                flags,
                branchNotTakenAccepted: true,
                jnzBoundaryReached: false,
                projectionEntered: true);
        }

        var moveEdi = Instruction(
            MoveEdiRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                .MoveEdiFromEaxPlusScaledEbx,
            firstDWORD,
            firstDWORD,
            ebx,
            ebx,
            null,
            arrayEntryDWORD,
            flags,
            flags,
            committed: true);
        NetworkDispatcherLockGateFlags testFlags = TestFlags(arrayEntryDWORD);
        var testEdi = Instruction(
            TestEdiRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind.TestEdiEdi,
            firstDWORD,
            firstDWORD,
            ebx,
            ebx,
            arrayEntryDWORD,
            arrayEntryDWORD,
            flags,
            testFlags,
            committed: true);
        return new(
            capture,
            parent,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome.BoundedBeforeJnz,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Captured,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Captured,
            ImmutableArray.Create(firstRead, secondRead),
            ImmutableArray.Create(moveEax, moveEdi, testEdi),
            parent.CompletedInstructionOffsets
                .Add(MoveEaxRawFileOffset)
                .Add(MoveEdiRawFileOffset)
                .Add(TestEdiRawFileOffset),
            BranchRawFileOffset,
            esi,
            firstDWORD,
            ebx,
            arrayEntryDWORD,
            testFlags,
            branchNotTakenAccepted: true,
            jnzBoundaryReached: true,
            projectionEntered: true);
    }

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset
            + NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .RawToCodeVirtualAddressDelta);

    private static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstruction Instruction(
        uint rawFileOffset,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind kind,
        uint? eaxBefore,
        uint? eaxAfter,
        uint? ebxBefore,
        uint? ebxAfter,
        uint? ediBefore,
        uint? ediAfter,
        NetworkDispatcherLockGateFlags flagsBefore,
        NetworkDispatcherLockGateFlags flagsAfter,
        bool committed)
    {
        (byte length, string bytes) = InstructionEncoding(kind);
        return new(
            rawFileOffset,
            RawFileOffsetToCodeVirtualAddress(rawFileOffset),
            length,
            bytes,
            kind,
            eaxBefore,
            eaxAfter,
            ebxBefore,
            ebxAfter,
            ediBefore,
            ediAfter,
            Attempted: true,
            Reached: true,
            Committed: committed,
            FlagsBefore: flagsBefore,
            FlagsAfter: flagsAfter);
    }

    private static (byte Length, string Bytes) InstructionEncoding(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind kind) =>
        kind switch
        {
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                .MoveEaxFromEsiPlusA4 => (6, MoveEaxInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind
                .MoveEdiFromEaxPlusScaledEbx => (3, MoveEdiInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchInstructionKind.TestEdiEdi =>
                (2, TestEdiInstructionBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    private static bool BranchAdmitted(
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan parent) =>
        parent.ExactCleanCompletion
        && parent.Outcome
            == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome.BoundedBeforeJz
        && parent.BranchBoundaryReached
        && parent.StopAtRawFileOffsetExclusive == EntryJzRawFileOffset
        && parent.EAX is uint eax
        && eax != 0
        && parent.EBX == 0
        && parent.ESI.HasValue
        && parent.FlagsAtStop is { Zero: false };

    private static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
        StateForUnreachedParent(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Unreached;

    private static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState
        StateForUnused(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCaptureState.Unreached;

    private static NetworkDispatcherLockGateFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);

    private static NetworkDispatcherLockGateFlags TestFlags(uint value) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)value),
            Auxiliary: null,
            Zero: value == 0,
            Sign: (value & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;
}

public sealed record NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCapture(
    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCapture ParentCapture,
    bool? FreshJnzTaken,
    uint? FreshEntryESP,
    uint? FreshDWORDAtEDI);

public enum NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
{
    ParentNotAccepted,
    MissingJnzObservation,
    JnzObservationMismatch,
    MissingFreshDWORDAtEDI,
    MissingFreshEntryESPAtFallthroughPush,
    MissingFreshEntryESPAtTargetPush,
    BoundedBeforeFallthroughCall,
    BoundedBeforeTargetCall
}

public enum NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
{
    Jnz,
    PushEdi,
    PushImmediate,
    MoveEaxFromEdi,
    MoveEcxFromEdi,
    PushOne
}

public readonly record struct
    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationRead(
        uint InstructionRawFileOffset,
        uint Address,
        byte Width,
        bool Attempted,
        bool Captured,
        uint? Value);

public readonly record struct
    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationStackWrite(
        uint InstructionRawFileOffset,
        uint Address,
        uint Value);

public readonly record struct
    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstruction(
        uint InstructionRawFileOffset,
        uint InstructionCodeVirtualAddress,
        byte InstructionByteLength,
        string InstructionBytes,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind Kind,
        uint? EAXBefore,
        uint? EAXAfter,
        uint? EBXBefore,
        uint? EBXAfter,
        uint? ECXBefore,
        uint? ECXAfter,
        uint? EDIBefore,
        uint? EDIAfter,
        uint? ESPBefore,
        uint? ESPAfter,
        bool? BranchTaken,
        uint? BranchTargetRawFileOffset,
        uint? StackWriteAddress,
        uint? StackWriteValue,
        bool Attempted,
        bool Reached,
        bool Committed,
        NetworkDispatcherLockGateFlags FlagsBefore,
        NetworkDispatcherLockGateFlags FlagsAfter);

public sealed class
    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan
{
    private readonly Lazy<NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan>
        reprojectedParentPlan;
    private readonly Lazy<NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan>
        reprojectedPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCapture capture,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan parentPlan,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome outcome,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
            branchCaptureState,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
            ediReadCaptureState,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
            espCaptureState,
        ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationRead>
            reads,
        ImmutableArray<
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationStackWrite>
            stackWrites,
        ImmutableArray<
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstruction>
            instructions,
        ImmutableArray<uint> completedInstructionOffsets,
        uint? stopAtRawFileOffsetExclusive,
        uint? selectedBranchTargetRawFileOffset,
        bool? conditionalBranchTaken,
        uint? esi,
        uint? eax,
        uint? ebx,
        uint? ecx,
        uint? edi,
        uint? esp,
        NetworkDispatcherLockGateFlags? flagsAtStop,
        bool branchObservationAccepted,
        bool branchInstructionProjected,
        bool projectionEntered)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Outcome = outcome;
        BranchCaptureState = branchCaptureState;
        EDIReadCaptureState = ediReadCaptureState;
        ESPCaptureState = espCaptureState;
        Reads = reads;
        StackWrites = stackWrites;
        Instructions = instructions;
        CompletedInstructionOffsets = completedInstructionOffsets;
        StopAtRawFileOffsetExclusive = stopAtRawFileOffsetExclusive;
        SelectedBranchTargetRawFileOffset = selectedBranchTargetRawFileOffset;
        ConditionalBranchTaken = conditionalBranchTaken;
        ESI = esi;
        EAX = eax;
        EBX = ebx;
        ECX = ecx;
        EDI = edi;
        ESP = esp;
        FlagsAtStop = flagsAtStop;
        BranchObservationAccepted = branchObservationAccepted;
        BranchInstructionProjected = branchInstructionProjected;
        ProjectionEntered = projectionEntered;
        reprojectedParentPlan = new(
            () => NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection.Project(
                Capture.ParentCapture),
            LazyThreadSafetyMode.ExecutionAndPublication);
        reprojectedPlan = new(
            () => NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .Project(Capture),
            LazyThreadSafetyMode.ExecutionAndPublication);
        orderedSemanticPrefixPreserved = new(
            ComputeOrderedSemanticPrefixPreserved,
            LazyThreadSafetyMode.ExecutionAndPublication);
        exactBoundedCompletion = new(
            ComputeExactBoundedCompletion,
            LazyThreadSafetyMode.ExecutionAndPublication);
        exactCleanCompletion = new(
            ComputeExactCleanCompletion,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCapture
        Capture { get; }
    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan ParentPlan { get; }
    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome Outcome
    {
        get;
    }
    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
        BranchCaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
        EDIReadCaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
        ESPCaptureState { get; }
    public ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationRead>
        Reads { get; }
    public ImmutableArray<
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationStackWrite>
        StackWrites { get; }
    public ImmutableArray<
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstruction>
        Instructions { get; }
    public ImmutableArray<uint> CompletedInstructionOffsets { get; }
    public uint? StopAtRawFileOffsetExclusive { get; }
    public uint? SelectedBranchTargetRawFileOffset { get; }
    public bool? ConditionalBranchTaken { get; }
    public uint? ESI { get; }
    public uint? EAX { get; }
    public uint? EBX { get; }
    public uint? ECX { get; }
    public uint? EDI { get; }
    public uint? ESP { get; }
    public NetworkDispatcherLockGateFlags? FlagsAtStop { get; }
    public bool BranchObservationAccepted { get; }
    public bool BranchInstructionProjected { get; }
    public bool ConditionalBranchProjected => BranchInstructionProjected;
    public bool ProjectionEntered { get; }
    public bool ParentInstructionsExecuted => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryReadsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;

    private bool ComputeExactBoundedCompletion() =>
        ProjectionEntered
        && ParentPlan.ExactCleanCompletion
        && BranchObservationAccepted
        && BranchInstructionProjected
        && BranchCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                .Captured
        && Instructions.Length
            == (ConditionalBranchTaken == true
                ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .TakenPathProjectedInstructionCount
                : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .FallthroughPathProjectedInstructionCount)
        && Instructions.All(instruction => instruction is { Reached: true, Committed: true })
        && CompletedInstructionOffsets.SequenceEqual(
            ExpectedCompletedOffsets())
        && ConditionalBranchTaken.HasValue
        && SelectedBranchTargetRawFileOffset
            == (ConditionalBranchTaken.Value
                ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .TakenTargetRawFileOffset
                : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .FallthroughRawFileOffset)
        && FlagsAtStop == ParentPlan.FlagsAtStop
        && (ConditionalBranchTaken.Value
            ? Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .BoundedBeforeTargetCall
                && StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                        .TargetCallRawFileOffset
            : Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .BoundedBeforeFallthroughCall
                && StopAtRawFileOffsetExclusive
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                        .FallthroughCallRawFileOffset);

    private bool ComputeExactCleanCompletion() =>
        ComputeExactBoundedCompletion()
        && OrderedSemanticPrefixPreserved
        && Reads.All(read => read is { Attempted: true, Captured: true })
        && StackWrites.SequenceEqual(
            Instructions
                .Where(instruction =>
                    instruction.StackWriteAddress.HasValue
                    && instruction.StackWriteValue.HasValue)
                .Select(instruction =>
                    new NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationStackWrite(
                        instruction.InstructionRawFileOffset,
                        instruction.StackWriteAddress!.Value,
                        instruction.StackWriteValue!.Value)))
        && (ConditionalBranchTaken == true
            ? EDIReadCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                        .Captured
                && ESPCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                        .Captured
                && Reads is [{
                    InstructionRawFileOffset:
                        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                            .TargetMoveEaxRawFileOffset,
                    Address: var address,
                    Value: var value
                }]
                && address == ParentPlan.EDI
                && value == EAX
                && StackWrites is [{
                    InstructionRawFileOffset:
                        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                            .TargetPushOneRawFileOffset,
                    Value: 1
                }]
            : EDIReadCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                        .SuppliedUnused
                && ESPCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                        .Captured
                && Reads.IsEmpty
                && StackWrites.Length
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                        .FallthroughPushCount);

    private bool ComputeOrderedSemanticPrefixPreserved()
    {
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan replayedParent =
            reprojectedParentPlan.Value;
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan replay =
            reprojectedPlan.Value;

        return SameParentPlan(ParentPlan, replayedParent)
            && ParentPlan.OrderedSemanticPrefixPreserved
            && Capture == replay.Capture
            && Outcome == replay.Outcome
            && BranchCaptureState == replay.BranchCaptureState
            && EDIReadCaptureState == replay.EDIReadCaptureState
            && ESPCaptureState == replay.ESPCaptureState
            && Reads.SequenceEqual(replay.Reads)
            && StackWrites.SequenceEqual(replay.StackWrites)
            && Instructions.SequenceEqual(replay.Instructions)
            && CompletedInstructionOffsets.SequenceEqual(
                replay.CompletedInstructionOffsets)
            && StopAtRawFileOffsetExclusive == replay.StopAtRawFileOffsetExclusive
            && SelectedBranchTargetRawFileOffset
                == replay.SelectedBranchTargetRawFileOffset
            && ConditionalBranchTaken == replay.ConditionalBranchTaken
            && ESI == replay.ESI
            && EAX == replay.EAX
            && EBX == replay.EBX
            && ECX == replay.ECX
            && EDI == replay.EDI
            && ESP == replay.ESP
            && FlagsAtStop == replay.FlagsAtStop
            && BranchObservationAccepted == replay.BranchObservationAccepted
            && BranchInstructionProjected == replay.BranchInstructionProjected
            && ProjectionEntered == replay.ProjectionEntered;
    }

    private bool SameParentPlan(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan left,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan right) =>
        left.Capture == right.Capture
        && left.Outcome == right.Outcome
        && left.DWORDAtA4CaptureState == right.DWORDAtA4CaptureState
        && left.ArrayEntryCaptureState == right.ArrayEntryCaptureState
        && left.Reads.SequenceEqual(right.Reads)
        && left.Instructions.SequenceEqual(right.Instructions)
        && left.CompletedInstructionOffsets.SequenceEqual(right.CompletedInstructionOffsets)
        && left.StopAtRawFileOffsetExclusive == right.StopAtRawFileOffsetExclusive
        && left.ESI == right.ESI
        && left.EAX == right.EAX
        && left.EBX == right.EBX
        && left.EDI == right.EDI
        && left.FlagsAtStop == right.FlagsAtStop
        && left.BranchNotTakenAccepted == right.BranchNotTakenAccepted
        && left.JnzBoundaryReached == right.JnzBoundaryReached
        && left.ProjectionEntered == right.ProjectionEntered
        && left.ExactBoundedCompletion == right.ExactBoundedCompletion
        && left.ExactCleanCompletion == right.ExactCleanCompletion;

    private ImmutableArray<uint> ExpectedCompletedOffsets()
    {
        ImmutableArray<uint> expected =
            ParentPlan.CompletedInstructionOffsets.Add(
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .JnzRawFileOffset);
        if (ConditionalBranchTaken == true)
        {
            return expected
                .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .TargetMoveEaxRawFileOffset)
                .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .TargetMoveEcxRawFileOffset)
                .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                    .TargetPushOneRawFileOffset);
        }

        return expected
            .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .FallthroughPushEdiRawFileOffset)
            .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .FallthroughPushArgumentRawFileOffset)
            .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .FallthroughPushFirstPointerRawFileOffset)
            .Add(NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .FallthroughPushSecondPointerRawFileOffset);
    }
}

/// <summary>
/// Reprojects JNZ at BBDE4B and both bounded paths, stopping before their indirect calls.
/// </summary>
public static class
    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
{
    public const uint JnzRawFileOffset = 0x00BBDE4B;
    public const uint FallthroughRawFileOffset = 0x00BBDE4D;
    public const uint TakenTargetRawFileOffset = 0x00BBDE66;
    public const uint FallthroughCallRawFileOffset = 0x00BBDE5D;
    public const uint TargetCallRawFileOffset = 0x00BBDE6C;
    public const uint FallthroughPushEdiRawFileOffset = 0x00BBDE4D;
    public const uint FallthroughPushArgumentRawFileOffset = 0x00BBDE4E;
    public const uint FallthroughPushFirstPointerRawFileOffset = 0x00BBDE53;
    public const uint FallthroughPushSecondPointerRawFileOffset = 0x00BBDE58;
    public const uint TargetMoveEaxRawFileOffset = 0x00BBDE66;
    public const uint TargetMoveEcxRawFileOffset = 0x00BBDE68;
    public const uint TargetPushOneRawFileOffset = 0x00BBDE6A;
    public const int FallthroughPushCount = 4;
    public const int FallthroughPathProjectedInstructionCount = 5;
    public const int TakenPathProjectedInstructionCount = 4;
    public const byte JnzInstructionByteLength = 2;
    public const byte FallthroughCallInstructionByteLength = 6;
    public const byte TargetCallInstructionByteLength = 3;
    public const byte DWORDWidth = sizeof(uint);
    public const uint BranchDisplacement = 0x19;
    public const uint FirstFallthroughImmediate = 0x23B;
    public const uint SecondFallthroughImmediate = 0x01B5717F;
    public const uint ThirdFallthroughImmediate = 0x01B5717E;
    public const uint TargetPushImmediate = 1;
    public const string JnzInstructionBytes = "7519";
    public const string FallthroughPushEdiInstructionBytes = "57";
    public const string FallthroughPushArgumentInstructionBytes = "683B020000";
    public const string FallthroughPushFirstPointerInstructionBytes = "687F71B501";
    public const string FallthroughPushSecondPointerInstructionBytes = "687E71B501";
    public const string FallthroughCallInstructionBytes = "FF15A05EC101";
    public const string TargetMoveEaxInstructionBytes = "8B07";
    public const string TargetMoveEcxInstructionBytes = "8BCF";
    public const string TargetPushOneInstructionBytes = "6A01";
    public const string TargetCallInstructionBytes = "FF5010";
    public const bool BytesIndependentlyVerified = true;

    public static uint JnzCodeVirtualAddress =>
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
            .RawFileOffsetToCodeVirtualAddress(JnzRawFileOffset);
    public static uint FallthroughCodeVirtualAddress =>
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
            .RawFileOffsetToCodeVirtualAddress(FallthroughRawFileOffset);
    public static uint TakenTargetCodeVirtualAddress =>
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
            .RawFileOffsetToCodeVirtualAddress(TakenTargetRawFileOffset);
    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan Project(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentCapture);
        ArgumentNullException.ThrowIfNull(capture.ParentCapture.ParentCapture);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan parent =
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection.Project(
                capture.ParentCapture);
        bool parentAccepted =
            parent.ExactCleanCompletion
            && parent.Outcome
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchOutcome.BoundedBeforeJnz
            && parent.JnzBoundaryReached
            && parent.StopAtRawFileOffsetExclusive == JnzRawFileOffset
            && parent.EDI.HasValue
            && parent.FlagsAtStop.HasValue;
        if (!parentAccepted)
        {
            return CreatePlan(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .ParentNotAccepted,
                StateForUnused(capture.FreshJnzTaken),
                StateForUnused(capture.FreshDWORDAtEDI),
                StateForUnused(capture.FreshEntryESP),
                ImmutableArray<
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationRead>.Empty,
                ImmutableArray<
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationStackWrite>
                    .Empty,
                ImmutableArray<
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstruction>
                    .Empty,
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                parent.FlagsAtStop,
                branchObservationAccepted: false,
                branchInstructionProjected: false,
                projectionEntered: false);
        }

        NetworkDispatcherLockGateFlags flags = parent.FlagsAtStop!.Value;
        bool expectedTaken = flags.Zero == false;
        if (capture.FreshJnzTaken is not bool observedTaken)
        {
            var attemptedBranch = CreateInstruction(
                JnzRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                    .Jnz,
                parent.EAX,
                parent.EAX,
                parent.EBX,
                parent.EBX,
                null,
                null,
                parent.EDI,
                parent.EDI,
                null,
                null,
                null,
                null,
                null,
                null,
                attempted: true,
                committed: false,
                flags,
                flags);
            return CreatePlan(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .MissingJnzObservation,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Missing,
                StateForUnused(capture.FreshDWORDAtEDI),
                StateForUnused(capture.FreshEntryESP),
                ImmutableArray<
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationRead>.Empty,
                ImmutableArray<
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationStackWrite>
                    .Empty,
                ImmutableArray.Create(attemptedBranch),
                parent.CompletedInstructionOffsets,
                JnzRawFileOffset,
                null,
                null,
                parent.ESI,
                parent.EAX,
                parent.EBX,
                null,
                parent.EDI,
                null,
                flags,
                branchObservationAccepted: false,
                branchInstructionProjected: false,
                projectionEntered: true);
        }

        if (observedTaken != expectedTaken)
        {
            var attemptedBranch = CreateInstruction(
                JnzRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                    .Jnz,
                parent.EAX,
                parent.EAX,
                parent.EBX,
                parent.EBX,
                null,
                null,
                parent.EDI,
                parent.EDI,
                null,
                null,
                observedTaken,
                null,
                null,
                null,
                attempted: true,
                committed: false,
                flags,
                flags);
            return CreatePlan(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .JnzObservationMismatch,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Captured,
                StateForUnused(capture.FreshDWORDAtEDI),
                StateForUnused(capture.FreshEntryESP),
                ImmutableArray<
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationRead>.Empty,
                ImmutableArray<
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationStackWrite>
                    .Empty,
                ImmutableArray.Create(attemptedBranch),
                parent.CompletedInstructionOffsets,
                JnzRawFileOffset,
                null,
                null,
                parent.ESI,
                parent.EAX,
                parent.EBX,
                null,
                parent.EDI,
                null,
                flags,
                branchObservationAccepted: false,
                branchInstructionProjected: false,
                projectionEntered: true);
        }

        uint selectedTarget = observedTaken ? TakenTargetRawFileOffset : FallthroughRawFileOffset;
        var branchInstruction = CreateInstruction(
            JnzRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                .Jnz,
            parent.EAX,
            parent.EAX,
            parent.EBX,
            parent.EBX,
            null,
            null,
            parent.EDI,
            parent.EDI,
            null,
            null,
            observedTaken,
            selectedTarget,
            null,
            null,
            attempted: true,
            committed: true,
            flags,
            flags);
        ImmutableArray<uint> completed = parent.CompletedInstructionOffsets.Add(JnzRawFileOffset);
        var instructions =
            ImmutableArray.CreateBuilder<
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstruction>();
        var reads =
            ImmutableArray.CreateBuilder<
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationRead>();
        var stackWrites =
            ImmutableArray.CreateBuilder<
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationStackWrite>();
        instructions.Add(branchInstruction);

        if (!observedTaken)
        {
            if (capture.FreshEntryESP is not uint entryESP)
            {
                instructions.Add(CreateInstruction(
                    FallthroughPushEdiRawFileOffset,
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                        .PushEdi,
                    parent.EAX,
                    parent.EAX,
                    parent.EBX,
                    parent.EBX,
                    null,
                    null,
                    parent.EDI,
                    parent.EDI,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    attempted: true,
                    committed: false,
                    flags,
                    flags));
                return CreatePlan(
                    capture,
                    parent,
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                        .MissingFreshEntryESPAtFallthroughPush,
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                        .Captured,
                    StateForUnused(capture.FreshDWORDAtEDI),
                    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                        .Missing,
                    reads.ToImmutable(),
                    stackWrites.ToImmutable(),
                    instructions.ToImmutable(),
                    completed,
                    FallthroughPushEdiRawFileOffset,
                    selectedTarget,
                    observedTaken,
                    parent.ESI,
                    parent.EAX,
                    parent.EBX,
                    null,
                    parent.EDI,
                    null,
                    flags,
                    branchObservationAccepted: true,
                    branchInstructionProjected: true,
                    projectionEntered: true);
            }

            uint esp = entryESP;
            (esp, completed) = AddPush(
                instructions,
                stackWrites,
                completed,
                FallthroughPushEdiRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                    .PushEdi,
                FallthroughPushEdiInstructionBytes,
                1,
                parent.EAX,
                parent.EBX,
                null,
                parent.EDI,
                esp,
                parent.EDI!.Value,
                flags);
            (esp, completed) = AddPush(
                instructions,
                stackWrites,
                completed,
                FallthroughPushArgumentRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                    .PushImmediate,
                FallthroughPushArgumentInstructionBytes,
                5,
                parent.EAX,
                parent.EBX,
                null,
                parent.EDI,
                esp,
                FirstFallthroughImmediate,
                flags);
            (esp, completed) = AddPush(
                instructions,
                stackWrites,
                completed,
                FallthroughPushFirstPointerRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                    .PushImmediate,
                FallthroughPushFirstPointerInstructionBytes,
                5,
                parent.EAX,
                parent.EBX,
                null,
                parent.EDI,
                esp,
                SecondFallthroughImmediate,
                flags);
            (esp, completed) = AddPush(
                instructions,
                stackWrites,
                completed,
                FallthroughPushSecondPointerRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                    .PushImmediate,
                FallthroughPushSecondPointerInstructionBytes,
                5,
                parent.EAX,
                parent.EBX,
                null,
                parent.EDI,
                esp,
                ThirdFallthroughImmediate,
                flags);
            return CreatePlan(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .BoundedBeforeFallthroughCall,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Captured,
                StateForUnused(capture.FreshDWORDAtEDI),
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Captured,
                reads.ToImmutable(),
                stackWrites.ToImmutable(),
                instructions.ToImmutable(),
                completed,
                FallthroughCallRawFileOffset,
                selectedTarget,
                observedTaken,
                parent.ESI,
                parent.EAX,
                parent.EBX,
                null,
                parent.EDI,
                esp,
                flags,
                branchObservationAccepted: true,
                branchInstructionProjected: true,
                projectionEntered: true);
        }

        var ediRead = new NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationRead(
            TargetMoveEaxRawFileOffset,
            parent.EDI!.Value,
            DWORDWidth,
            Attempted: true,
            Captured: capture.FreshDWORDAtEDI.HasValue,
            Value: capture.FreshDWORDAtEDI);
        reads.Add(ediRead);
        if (capture.FreshDWORDAtEDI is not uint eaxFromEdi)
        {
            instructions.Add(CreateInstruction(
                TargetMoveEaxRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                    .MoveEaxFromEdi,
                parent.EAX,
                parent.EAX,
                parent.EBX,
                parent.EBX,
                null,
                null,
                parent.EDI,
                parent.EDI,
                null,
                null,
                null,
                null,
                null,
                null,
                attempted: true,
                committed: false,
                flags,
                flags));
            return CreatePlan(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .MissingFreshDWORDAtEDI,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Captured,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Missing,
                StateForUnused(capture.FreshEntryESP),
                reads.ToImmutable(),
                stackWrites.ToImmutable(),
                instructions.ToImmutable(),
                completed,
                TargetMoveEaxRawFileOffset,
                selectedTarget,
                observedTaken,
                parent.ESI,
                parent.EAX,
                parent.EBX,
                null,
                parent.EDI,
                null,
                flags,
                branchObservationAccepted: true,
                branchInstructionProjected: true,
                projectionEntered: true);
        }

        instructions.Add(CreateInstruction(
            TargetMoveEaxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                .MoveEaxFromEdi,
            parent.EAX,
            eaxFromEdi,
            parent.EBX,
            parent.EBX,
            null,
            null,
            parent.EDI,
            parent.EDI,
            capture.FreshEntryESP,
            capture.FreshEntryESP,
            null,
            null,
            null,
            null,
            attempted: true,
            committed: true,
            flags,
            flags));
        completed = completed.Add(TargetMoveEaxRawFileOffset);
        instructions.Add(CreateInstruction(
            TargetMoveEcxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                .MoveEcxFromEdi,
            eaxFromEdi,
            eaxFromEdi,
            parent.EBX,
            parent.EBX,
            null,
            parent.EDI,
            parent.EDI,
            parent.EDI,
            capture.FreshEntryESP,
            capture.FreshEntryESP,
            null,
            null,
            null,
            null,
            attempted: true,
            committed: true,
            flags,
            flags));
        completed = completed.Add(TargetMoveEcxRawFileOffset);
        if (capture.FreshEntryESP is not uint targetEntryESP)
        {
            instructions.Add(CreateInstruction(
                TargetPushOneRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                    .PushOne,
                eaxFromEdi,
                eaxFromEdi,
                parent.EBX,
                parent.EBX,
                parent.EDI,
                parent.EDI,
                parent.EDI,
                parent.EDI,
                null,
                null,
                null,
                null,
                null,
                null,
                attempted: true,
                committed: false,
                flags,
                flags));
            return CreatePlan(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                    .MissingFreshEntryESPAtTargetPush,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Captured,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Captured,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                    .Missing,
                reads.ToImmutable(),
                stackWrites.ToImmutable(),
                instructions.ToImmutable(),
                completed,
                TargetPushOneRawFileOffset,
                selectedTarget,
                observedTaken,
                parent.ESI,
                eaxFromEdi,
                parent.EBX,
                parent.EDI,
                parent.EDI,
                null,
                flags,
                branchObservationAccepted: true,
                branchInstructionProjected: true,
                projectionEntered: true);
        }

        _ = AddPush(
            instructions,
            stackWrites,
            completed,
            TargetPushOneRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                .PushOne,
            TargetPushOneInstructionBytes,
            2,
            eaxFromEdi,
            parent.EBX,
            parent.EDI,
            parent.EDI,
            targetEntryESP,
            TargetPushImmediate,
            flags);
        return CreatePlan(
            capture,
            parent,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                .BoundedBeforeTargetCall,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                .Captured,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                .Captured,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                .Captured,
            reads.ToImmutable(),
            stackWrites.ToImmutable(),
            instructions.ToImmutable(),
            completed.Add(TargetPushOneRawFileOffset),
            TargetCallRawFileOffset,
            selectedTarget,
            observedTaken,
            parent.ESI,
            eaxFromEdi,
            parent.EBX,
            parent.EDI,
            parent.EDI,
            unchecked(targetEntryESP - DWORDWidth),
            flags,
            branchObservationAccepted: true,
            branchInstructionProjected: true,
            projectionEntered: true);
    }

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchProjection
            .RawFileOffsetToCodeVirtualAddress(rawFileOffset);

    private static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan
        CreatePlan(
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCapture capture,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchPlan parent,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome outcome,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                branchCaptureState,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                ediReadCaptureState,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                espCaptureState,
            ImmutableArray<
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationRead> reads,
            ImmutableArray<
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationStackWrite>
                stackWrites,
            ImmutableArray<
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstruction>
                instructions,
            ImmutableArray<uint> completedInstructionOffsets,
            uint? stopAtRawFileOffsetExclusive,
            uint? selectedBranchTargetRawFileOffset,
            bool? conditionalBranchTaken,
            uint? esi,
            uint? eax,
            uint? ebx,
            uint? ecx,
            uint? edi,
            uint? esp,
            NetworkDispatcherLockGateFlags? flagsAtStop,
            bool branchObservationAccepted,
            bool branchInstructionProjected,
            bool projectionEntered) =>
        new(
            capture,
            parent,
            outcome,
            branchCaptureState,
            ediReadCaptureState,
            espCaptureState,
            reads,
            stackWrites,
            instructions,
            completedInstructionOffsets,
            stopAtRawFileOffsetExclusive,
            selectedBranchTargetRawFileOffset,
            conditionalBranchTaken,
            esi,
            eax,
            ebx,
            ecx,
            edi,
            esp,
            flagsAtStop,
            branchObservationAccepted,
            branchInstructionProjected,
            projectionEntered);

    private static (uint ESP, ImmutableArray<uint> CompletedOffsets) AddPush(
        ImmutableArray<
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstruction>.Builder
            instructions,
        ImmutableArray<
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationStackWrite>.Builder
            stackWrites,
        ImmutableArray<uint> completedOffsets,
        uint rawFileOffset,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind kind,
        string bytes,
        byte byteLength,
        uint? eax,
        uint? ebx,
        uint? ecx,
        uint? edi,
        uint espBefore,
        uint value,
        NetworkDispatcherLockGateFlags flags)
    {
        uint espAfter = unchecked(espBefore - DWORDWidth);
        instructions.Add(CreateInstruction(
            rawFileOffset,
            kind,
            eax,
            eax,
            ebx,
            ebx,
            ecx,
            ecx,
            edi,
            edi,
            espBefore,
            espAfter,
            null,
            null,
            espAfter,
            value,
            attempted: true,
            committed: true,
            flags,
            flags,
            byteLength,
            bytes));
        stackWrites.Add(new(rawFileOffset, espAfter, value));
        return (espAfter, completedOffsets.Add(rawFileOffset));
    }

    private static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstruction
        CreateInstruction(
            uint rawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind kind,
            uint? eaxBefore,
            uint? eaxAfter,
            uint? ebxBefore,
            uint? ebxAfter,
            uint? ecxBefore,
            uint? ecxAfter,
            uint? ediBefore,
            uint? ediAfter,
            uint? espBefore,
            uint? espAfter,
            bool? branchTaken,
            uint? branchTargetRawFileOffset,
            uint? stackWriteAddress,
            uint? stackWriteValue,
            bool attempted,
            bool committed,
            NetworkDispatcherLockGateFlags flagsBefore,
            NetworkDispatcherLockGateFlags flagsAfter,
            byte? byteLength = null,
            string? bytes = null)
    {
        (byte defaultLength, string defaultBytes) = Encoding(kind);
        return new(
            rawFileOffset,
            RawFileOffsetToCodeVirtualAddress(rawFileOffset),
            byteLength ?? defaultLength,
            bytes ?? defaultBytes,
            kind,
            eaxBefore,
            eaxAfter,
            ebxBefore,
            ebxAfter,
            ecxBefore,
            ecxAfter,
            ediBefore,
            ediAfter,
            espBefore,
            espAfter,
            branchTaken,
            branchTargetRawFileOffset,
            stackWriteAddress,
            stackWriteValue,
            Attempted: attempted,
            Reached: true,
            Committed: committed,
            FlagsBefore: flagsBefore,
            FlagsAfter: flagsAfter);
    }

    private static (byte Length, string Bytes) Encoding(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind kind) =>
        kind switch
        {
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind.Jnz =>
                (JnzInstructionByteLength, JnzInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind.PushEdi =>
                (1, FallthroughPushEdiInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                .PushImmediate => (5, FallthroughPushArgumentInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                .MoveEaxFromEdi => (2, TargetMoveEaxInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                .MoveEcxFromEdi => (2, TargetMoveEcxInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationInstructionKind
                .PushOne => (2, TargetPushOneInstructionBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    private static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
        StateForUnused(bool? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                .SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                .Unreached;

    private static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
        StateForUnused(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                .SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCaptureState
                .Unreached;
}

public enum NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
{
    FallthroughAtBBDE5D,
    TakenTargetAtBBDE6C
}

public sealed record NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCapture(
    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationCapture ParentCapture,
    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite CallSite,
    bool? NormalReturnObserved);

public enum NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
{
    Unreached,
    Pending,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
{
    ParentNotAccepted,
    CallSiteNotReached,
    NormalReturnNotObserved,
    CallDidNotReturnNormally,
    BoundedAfterFallthroughCall,
    BoundedAfterTargetCall
}

public readonly record struct
    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallEvidence(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite CallSite,
        uint InstructionRawFileOffset,
        uint InstructionCodeVirtualAddress,
        byte InstructionByteLength,
        string InstructionBytes,
        bool InstructionReached,
        bool Attempted,
        bool? ReturnedNormally,
        bool Completed);

public sealed class
    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan
{
    private readonly Lazy<
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan>
        reprojectedParentPlan;
    private readonly Lazy<
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan>
        reprojectedPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCapture capture,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan parentPlan,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome outcome,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallEvidence? call,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
            normalReturnCaptureState,
        ImmutableArray<uint> completedInstructionOffsets,
        uint? stopAtRawFileOffsetExclusive,
        NetworkDispatcherLockGateFlags? flagsAtStop,
        bool callBoundaryReached,
        bool nextInstructionBoundaryReached,
        bool projectionEntered)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Outcome = outcome;
        Call = call;
        NormalReturnCaptureState = normalReturnCaptureState;
        CompletedInstructionOffsets = completedInstructionOffsets;
        StopAtRawFileOffsetExclusive = stopAtRawFileOffsetExclusive;
        FlagsAtStop = flagsAtStop;
        CallBoundaryReached = callBoundaryReached;
        NextInstructionBoundaryReached = nextInstructionBoundaryReached;
        ProjectionEntered = projectionEntered;
        reprojectedParentPlan = new(
            () => NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
                .Project(Capture.ParentCapture),
            LazyThreadSafetyMode.ExecutionAndPublication);
        reprojectedPlan = new(
            () => NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .Project(Capture),
            LazyThreadSafetyMode.ExecutionAndPublication);
        orderedSemanticPrefixPreserved = new(
            ComputeOrderedSemanticPrefixPreserved,
            LazyThreadSafetyMode.ExecutionAndPublication);
        exactBoundedCompletion = new(
            ComputeExactBoundedCompletion,
            LazyThreadSafetyMode.ExecutionAndPublication);
        exactCleanCompletion = new(
            ComputeExactCleanCompletion,
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCapture Capture
    {
        get;
    }
    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan ParentPlan
    {
        get;
    }
    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome Outcome
    {
        get;
    }
    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallEvidence? Call { get; }
    public NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
        NormalReturnCaptureState { get; }
    public ImmutableArray<uint> CompletedInstructionOffsets { get; }
    public uint? StopAtRawFileOffsetExclusive { get; }
    public NetworkDispatcherLockGateFlags? FlagsAtStop { get; }
    public bool CallBoundaryReached { get; }
    public bool NextInstructionBoundaryReached { get; }
    public bool ProjectionEntered { get; }
    public bool PostCallFlagsForwarded => false;
    public bool PostCallRegistersForwarded => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeCallExecuted => false;
    public bool NativeMemoryReadsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool ParentInstructionsExecuted => false;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;

    private bool ComputeExactBoundedCompletion()
    {
        if (!ProjectionEntered)
        {
            return false;
        }

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome expectedOutcome =
            Capture.CallSite
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .FallthroughAtBBDE5D
                ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .BoundedAfterFallthroughCall
                : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .BoundedAfterTargetCall;
        uint callRawFileOffset =
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .CallRawFileOffset(Capture.CallSite);
        uint nextRawFileOffset =
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .NextInstructionRawFileOffset(Capture.CallSite);
        return ProjectionEntered
            && ParentPlan.ExactCleanCompletion
            && Outcome == expectedOutcome
            && Call is {
                InstructionReached: true,
                Attempted: true,
                ReturnedNormally: true,
                Completed: true
            }
            && Call.Value.CallSite == Capture.CallSite
            && Call.Value.InstructionRawFileOffset == callRawFileOffset
            && CallBoundaryReached
            && NextInstructionBoundaryReached
            && CompletedInstructionOffsets.SequenceEqual(
                ParentPlan.CompletedInstructionOffsets.Add(callRawFileOffset))
            && CompletedInstructionOffsets[^1] == callRawFileOffset
            && StopAtRawFileOffsetExclusive == nextRawFileOffset
            && FlagsAtStop == UnknownFlags;
    }

    private bool ComputeExactCleanCompletion() =>
        ComputeExactBoundedCompletion()
        && OrderedSemanticPrefixPreserved
        && NormalReturnCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                .Captured;

    private bool ComputeOrderedSemanticPrefixPreserved()
    {
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan replayedParent =
            reprojectedParentPlan.Value;
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan replay =
            reprojectedPlan.Value;
        if (!SameParentPlan(ParentPlan, replayedParent)
            || !ParentPlan.OrderedSemanticPrefixPreserved
            || Capture != replay.Capture
            || Outcome != replay.Outcome
            || Call != replay.Call
            || NormalReturnCaptureState != replay.NormalReturnCaptureState
            || !CompletedInstructionOffsets.SequenceEqual(
                replay.CompletedInstructionOffsets)
            || StopAtRawFileOffsetExclusive != replay.StopAtRawFileOffsetExclusive
            || FlagsAtStop != replay.FlagsAtStop
            || CallBoundaryReached != replay.CallBoundaryReached
            || NextInstructionBoundaryReached != replay.NextInstructionBoundaryReached
            || ProjectionEntered != replay.ProjectionEntered)
        {
            return false;
        }

        if (!ProjectionEntered)
        {
            return Call is null
                && CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets)
                && StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive
                && FlagsAtStop == ParentPlan.FlagsAtStop
                && !CallBoundaryReached
                && !NextInstructionBoundaryReached;
        }

        uint callRawFileOffset =
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .CallRawFileOffset(Capture.CallSite);
        uint nextRawFileOffset =
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                .NextInstructionRawFileOffset(Capture.CallSite);
        bool normalReturnObserved = Capture.NormalReturnObserved == true;
        bool isPending = Capture.NormalReturnObserved is null;
        return CallBoundaryReached
            && Call is {
                InstructionRawFileOffset: var actualCallRawFileOffset,
                InstructionCodeVirtualAddress: var callCodeVirtualAddress,
                InstructionByteLength: var callByteLength,
                InstructionBytes: var callBytes,
                InstructionReached: true,
                Attempted: true
            }
            && Call.Value.CallSite == Capture.CallSite
            && actualCallRawFileOffset == callRawFileOffset
            && callCodeVirtualAddress
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                    .RawFileOffsetToCodeVirtualAddress(callRawFileOffset)
            && callByteLength
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                    .CallInstructionByteLength(Capture.CallSite)
            && callBytes
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
                    .CallInstructionBytes(Capture.CallSite)
            && Call.Value.ReturnedNormally == Capture.NormalReturnObserved
            && Call.Value.Completed == normalReturnObserved
            && NormalReturnCaptureState == (isPending
                ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                    .Pending
                : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                    .Captured)
            && (normalReturnObserved
                ? Outcome == (Capture.CallSite
                        == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                            .FallthroughAtBBDE5D
                        ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                            .BoundedAfterFallthroughCall
                        : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                            .BoundedAfterTargetCall)
                    && CompletedInstructionOffsets.SequenceEqual(
                        ParentPlan.CompletedInstructionOffsets.Add(callRawFileOffset))
                    && StopAtRawFileOffsetExclusive == nextRawFileOffset
                    && NextInstructionBoundaryReached
                    && FlagsAtStop == UnknownFlags
                : Outcome == (isPending
                        ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                            .NormalReturnNotObserved
                        : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                            .CallDidNotReturnNormally)
                    && CompletedInstructionOffsets.SequenceEqual(
                        ParentPlan.CompletedInstructionOffsets)
                    && StopAtRawFileOffsetExclusive == callRawFileOffset
                    && !NextInstructionBoundaryReached
                    && FlagsAtStop == ParentPlan.FlagsAtStop);
    }

    private bool SameParentPlan(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan left,
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan right) =>
        left.Capture == right.Capture
        && left.ParentPlan.Capture == right.ParentPlan.Capture
        && left.Outcome == right.Outcome
        && left.BranchCaptureState == right.BranchCaptureState
        && left.EDIReadCaptureState == right.EDIReadCaptureState
        && left.ESPCaptureState == right.ESPCaptureState
        && left.Reads.SequenceEqual(right.Reads)
        && left.StackWrites.SequenceEqual(right.StackWrites)
        && left.Instructions.SequenceEqual(right.Instructions)
        && left.CompletedInstructionOffsets.SequenceEqual(
            right.CompletedInstructionOffsets)
        && left.StopAtRawFileOffsetExclusive == right.StopAtRawFileOffsetExclusive
        && left.SelectedBranchTargetRawFileOffset == right.SelectedBranchTargetRawFileOffset
        && left.ConditionalBranchTaken == right.ConditionalBranchTaken
        && left.ESI == right.ESI
        && left.EAX == right.EAX
        && left.EBX == right.EBX
        && left.ECX == right.ECX
        && left.EDI == right.EDI
        && left.ESP == right.ESP
        && left.FlagsAtStop == right.FlagsAtStop
        && left.BranchObservationAccepted == right.BranchObservationAccepted
        && left.BranchInstructionProjected == right.BranchInstructionProjected
        && left.ProjectionEntered == right.ProjectionEntered
        && left.ExactBoundedCompletion == right.ExactBoundedCompletion
        && left.ExactCleanCompletion == right.ExactCleanCompletion;

    private static NetworkDispatcherLockGateFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);
}

/// <summary>
/// Reprojects either opaque indirect call and stops at its immediate return boundary.
/// </summary>
public static class
    NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnProjection
{
    public const uint FallthroughCallRawFileOffset = 0x00BBDE5D;
    public const uint FallthroughNextInstructionRawFileOffset = 0x00BBDE63;
    public const uint TargetCallRawFileOffset = 0x00BBDE6C;
    public const uint TargetNextInstructionRawFileOffset = 0x00BBDE6F;
    public const byte FallthroughCallInstructionByteLength = 6;
    public const byte TargetCallInstructionByteLength = 3;
    public const string FallthroughCallInstructionBytes = "FF15A05EC101";
    public const string FallthroughNextInstructionBytes = "83C410";
    public const string TargetCallInstructionBytes = "FF5010";
    public const string TargetNextInstructionBytes = "8B86A8000000";
    public const bool CallsAreIndirect = true;
    public const bool BytesIndependentlyVerified = true;

    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan Project(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentCapture);
        ArgumentNullException.ThrowIfNull(capture.ParentCapture.ParentCapture);

        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan parent =
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection.Project(
                capture.ParentCapture);
        bool parentAccepted =
            parent.ExactCleanCompletion
            && parent.BranchObservationAccepted
            && parent.BranchInstructionProjected;
        bool callSiteReached =
            parentAccepted
            && ((capture.CallSite
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                        .FallthroughAtBBDE5D
                && parent.Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                        .BoundedBeforeFallthroughCall
                && parent.StopAtRawFileOffsetExclusive == FallthroughCallRawFileOffset)
                || (capture.CallSite
                        == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                            .TakenTargetAtBBDE6C
                    && parent.Outcome
                        == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationOutcome
                            .BoundedBeforeTargetCall
                    && parent.StopAtRawFileOffsetExclusive == TargetCallRawFileOffset));

        if (!parentAccepted || !callSiteReached)
        {
            return CreatePlan(
                capture,
                parent,
                !parentAccepted
                    ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                        .ParentNotAccepted
                    : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                        .CallSiteNotReached,
                null,
                StateForUnreachedParent(capture.NormalReturnObserved),
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive,
                parent.FlagsAtStop,
                callBoundaryReached: false,
                nextInstructionBoundaryReached: false,
                projectionEntered: false);
        }

        uint callRawFileOffset = CallRawFileOffset(capture.CallSite);
        var call = new NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallEvidence(
            capture.CallSite,
            callRawFileOffset,
            RawFileOffsetToCodeVirtualAddress(callRawFileOffset),
            CallInstructionByteLength(capture.CallSite),
            CallInstructionBytes(capture.CallSite),
            InstructionReached: true,
            Attempted: true,
            ReturnedNormally: capture.NormalReturnObserved,
            Completed: capture.NormalReturnObserved == true);
        if (capture.NormalReturnObserved is null)
        {
            return CreatePlan(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .NormalReturnNotObserved,
                call,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                    .Pending,
                parent.CompletedInstructionOffsets,
                callRawFileOffset,
                parent.FlagsAtStop,
                callBoundaryReached: true,
                nextInstructionBoundaryReached: false,
                projectionEntered: true);
        }

        if (capture.NormalReturnObserved == false)
        {
            return CreatePlan(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .CallDidNotReturnNormally,
                call,
                NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                    .Captured,
                parent.CompletedInstructionOffsets,
                callRawFileOffset,
                parent.FlagsAtStop,
                callBoundaryReached: true,
                nextInstructionBoundaryReached: false,
                projectionEntered: true);
        }

        return CreatePlan(
            capture,
            parent,
            capture.CallSite
                == NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite
                    .FallthroughAtBBDE5D
                ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .BoundedAfterFallthroughCall
                : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome
                    .BoundedAfterTargetCall,
            call,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState.Captured,
            parent.CompletedInstructionOffsets.Add(callRawFileOffset),
            NextInstructionRawFileOffset(capture.CallSite),
            UnknownFlags,
            callBoundaryReached: true,
            nextInstructionBoundaryReached: true,
            projectionEntered: true);
    }

    public static uint CallRawFileOffset(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite callSite) =>
        callSite switch
        {
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite.FallthroughAtBBDE5D =>
                FallthroughCallRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite.TakenTargetAtBBDE6C =>
                TargetCallRawFileOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(callSite))
        };

    public static uint NextInstructionRawFileOffset(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite callSite) =>
        callSite switch
        {
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite.FallthroughAtBBDE5D =>
                FallthroughNextInstructionRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite.TakenTargetAtBBDE6C =>
                TargetNextInstructionRawFileOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(callSite))
        };

    public static byte CallInstructionByteLength(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite callSite) =>
        callSite switch
        {
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite.FallthroughAtBBDE5D =>
                FallthroughCallInstructionByteLength,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite.TakenTargetAtBBDE6C =>
                TargetCallInstructionByteLength,
            _ => throw new ArgumentOutOfRangeException(nameof(callSite))
        };

    public static string CallInstructionBytes(
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite callSite) =>
        callSite switch
        {
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite.FallthroughAtBBDE5D =>
                FallthroughCallInstructionBytes,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallSite.TakenTargetAtBBDE6C =>
                TargetCallInstructionBytes,
            _ => throw new ArgumentOutOfRangeException(nameof(callSite))
        };

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationProjection
            .RawFileOffsetToCodeVirtualAddress(rawFileOffset);

    private static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnPlan
        CreatePlan(
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCapture capture,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchContinuationPlan parent,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnOutcome outcome,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallEvidence? call,
            NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                normalReturnCaptureState,
            ImmutableArray<uint> completedInstructionOffsets,
            uint? stopAtRawFileOffsetExclusive,
            NetworkDispatcherLockGateFlags? flagsAtStop,
            bool callBoundaryReached,
            bool nextInstructionBoundaryReached,
            bool projectionEntered) =>
        new(
            capture,
            parent,
            outcome,
            call,
            normalReturnCaptureState,
            completedInstructionOffsets,
            stopAtRawFileOffsetExclusive,
            flagsAtStop,
            callBoundaryReached,
            nextInstructionBoundaryReached,
            projectionEntered);

    private static NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
        StateForUnreachedParent(bool? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                .SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticNonzeroBranchCallReturnCaptureState
                .Unreached;

    private static NetworkDispatcherLockGateFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);
}
