using System.Collections.Immutable;
using System.Numerics;
using System.Threading;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCapture(
    NetworkDispatcherEntryFallbackDiagnosticReturnCapture ParentCapture,
    uint? FreshPostCallESI,
    uint? FreshDWORDAtESIPlusA8,
    uint? FreshDWORDAtESIPlusA4);

public enum NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
{
    Unreached,
    Missing,
    Captured,
    SuppliedUnused
}

public enum NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
{
    ParentCallReturnNotAccepted,
    MissingFreshPostCallESI,
    MissingFreshDWORDAtESIPlusA8,
    MissingFreshDWORDAtESIPlusA4,
    BoundedBeforeJz
}

public enum NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
{
    MoveEaxFromEsiPlusA8,
    XorEbxEbx,
    SubEaxFromEsiPlusA4,
    SarEaxByTwo,
    TestEaxEax
}

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixRead(
    uint InstructionRawFileOffset,
    uint Address,
    byte Width,
    bool Attempted,
    bool Captured,
    uint? Value);

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind Kind,
    uint? EAXBefore,
    uint? EAXAfter,
    uint? EBXBefore,
    uint? EBXAfter,
    bool Attempted,
    bool Reached,
    bool Committed,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter);

public sealed class NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan
{
    private readonly Lazy<NetworkDispatcherEntryFallbackDiagnosticReturnPlan>
        reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan(
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCapture capture,
        NetworkDispatcherEntryFallbackDiagnosticReturnPlan parentPlan,
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome outcome,
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState esiCaptureState,
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
            dwordAtA8CaptureState,
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
            dwordAtA4CaptureState,
        ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixRead> reads,
        ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction>
            instructions,
        ImmutableArray<uint> completedInstructionOffsets,
        uint? stopAtRawFileOffsetExclusive,
        uint? esi,
        uint? eax,
        uint? ebx,
        NetworkDispatcherLockGateFlags? flagsAtStop,
        bool branchBoundaryReached,
        bool projectionEntered)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Outcome = outcome;
        EsiCaptureState = esiCaptureState;
        DWORDAtA8CaptureState = dwordAtA8CaptureState;
        DWORDAtA4CaptureState = dwordAtA4CaptureState;
        Reads = reads;
        Instructions = instructions;
        CompletedInstructionOffsets = completedInstructionOffsets;
        StopAtRawFileOffsetExclusive = stopAtRawFileOffsetExclusive;
        ESI = esi;
        EAX = eax;
        EBX = ebx;
        FlagsAtStop = flagsAtStop;
        BranchBoundaryReached = branchBoundaryReached;
        ProjectionEntered = projectionEntered;
        reprojectedParentPlan = new(
            () => NetworkDispatcherEntryFallbackDiagnosticReturnProjection.Project(
                Capture.ParentCapture),
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

    public NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCapture Capture { get; }
    public NetworkDispatcherEntryFallbackDiagnosticReturnPlan ParentPlan { get; }
    public NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome Outcome { get; }
    public NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
        EsiCaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
        DWORDAtA8CaptureState { get; }
    public NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
        DWORDAtA4CaptureState { get; }
    public ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixRead> Reads
    {
        get;
    }
    public ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction>
        Instructions { get; }
    public ImmutableArray<uint> CompletedInstructionOffsets { get; }
    public uint EntryRawFileOffset =>
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.EntryRawFileOffset;
    public uint? StopAtRawFileOffsetExclusive { get; }
    public uint? ESI { get; }
    public uint? EAX { get; }
    public uint? EBX { get; }
    public NetworkDispatcherLockGateFlags? FlagsAtStop { get; }
    public bool BranchBoundaryReached { get; }
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
        && Outcome == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
            .BoundedBeforeJz
        && EsiCaptureState
            == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured
        && DWORDAtA8CaptureState
            == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured
        && DWORDAtA4CaptureState
            == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured
        && Instructions.Length
            == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .MaximumProjectedInstructionCount
        && Instructions.All(instruction => instruction is { Reached: true, Committed: true })
        && CompletedInstructionOffsets.SequenceEqual(
            ParentPlan.CompletedInstructionOffsets
                .Add(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .MoveEaxRawFileOffset)
                .Add(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .XorEbxRawFileOffset)
                .Add(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .SubEaxRawFileOffset)
                .Add(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .SarEaxRawFileOffset)
                .Add(NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .TestEaxRawFileOffset))
        && CompletedInstructionOffsets[^1]
            == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .TestEaxRawFileOffset
        && StopAtRawFileOffsetExclusive
            == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .BranchRawFileOffset
        && BranchBoundaryReached
        && ESI == Capture.FreshPostCallESI
        && EAX == ExpectedTestResult
        && EBX == 0
        && FlagsAtStop == TestFlags(ExpectedTestResult!.Value);

    private bool ComputeExactCleanCompletion() =>
        ComputeExactBoundedCompletion()
        && OrderedSemanticPrefixPreserved
        && Reads.Length == 2
        && Reads.All(read => read is { Attempted: true, Captured: true })
        && Instructions[0] is {
            InstructionRawFileOffset:
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .MoveEaxRawFileOffset,
            Kind: NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                .MoveEaxFromEsiPlusA8,
            EAXAfter: not null,
            Committed: true
        }
        && Instructions[1] is {
            InstructionRawFileOffset:
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .XorEbxRawFileOffset,
            Kind: NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                .XorEbxEbx,
            EBXAfter: 0,
            Committed: true
        }
        && Instructions[2] is {
            InstructionRawFileOffset:
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .SubEaxRawFileOffset,
            Kind: NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                .SubEaxFromEsiPlusA4,
            Committed: true
        }
        && Instructions[3] is {
            InstructionRawFileOffset:
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .SarEaxRawFileOffset,
            Kind: NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                .SarEaxByTwo,
            Committed: true
        }
        && Instructions[4] is {
            InstructionRawFileOffset:
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .TestEaxRawFileOffset,
            Kind: NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                .TestEaxEax,
            Committed: true
        };

    private uint? ExpectedTestResult =>
        Capture.FreshDWORDAtESIPlusA8 is uint a8
        && Capture.FreshDWORDAtESIPlusA4 is uint a4
            ? unchecked((uint)((int)unchecked(a8 - a4) >> 2))
            : null;

    private bool ComputeOrderedSemanticPrefixPreserved()
    {
        NetworkDispatcherEntryFallbackDiagnosticReturnPlan actual = ParentPlan;
        NetworkDispatcherEntryFallbackDiagnosticReturnPlan replay =
            reprojectedParentPlan.Value;
        if (actual.Capture != replay.Capture
            || actual.Outcome != replay.Outcome
            || actual.Call != replay.Call
            || actual.NormalReturnCaptureState != replay.NormalReturnCaptureState
            || !actual.CompletedInstructionOffsets.SequenceEqual(
                replay.CompletedInstructionOffsets)
            || actual.StopAtRawFileOffsetExclusive != replay.StopAtRawFileOffsetExclusive
            || actual.FlagsAtStop != replay.FlagsAtStop
            || actual.CallBoundaryReached != replay.CallBoundaryReached
            || actual.NextInstructionBoundaryReached != replay.NextInstructionBoundaryReached
            || actual.ProjectionEntered != replay.ProjectionEntered
            || actual.ExactBoundedCompletion != replay.ExactBoundedCompletion
            || actual.ExactCleanCompletion != replay.ExactCleanCompletion
            || !actual.OrderedSemanticPrefixPreserved)
        {
            return false;
        }

        if (!ProjectionEntered)
        {
            return Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                        .ParentCallReturnNotAccepted
                && Instructions.IsEmpty
                && Reads.IsEmpty
                && CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets)
                && StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive
                && FlagsAtStop == ParentPlan.FlagsAtStop
                && !BranchBoundaryReached
                && EsiCaptureState == StateForUnreachedParent(Capture.FreshPostCallESI)
                && DWORDAtA8CaptureState
                    == StateForUnreachedParent(Capture.FreshDWORDAtESIPlusA8)
                && DWORDAtA4CaptureState
                    == StateForUnreachedParent(Capture.FreshDWORDAtESIPlusA4);
        }

        if (!ParentAccepted(actual))
        {
            return false;
        }

        return ProjectionMatchesCaptureState()
            && Instructions.SequenceEqual(ExpectedInstructionPrefix())
            && Reads.SequenceEqual(ExpectedReads())
            && CompletedInstructionOffsets.SequenceEqual(ExpectedCompletedOffsets())
            && StopAtRawFileOffsetExclusive == ExpectedStopOffset()
            && ESI == Capture.FreshPostCallESI
            && EAX == ExpectedCurrentEAX()
            && EBX == ExpectedCurrentEBX()
            && FlagsAtStop == ExpectedCurrentFlags()
            && BranchBoundaryReached
                == (Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                        .BoundedBeforeJz);
    }

    private bool ProjectionMatchesCaptureState()
    {
        if (Capture.FreshPostCallESI is not uint esi)
        {
            return Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                        .MissingFreshPostCallESI
                && EsiCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
                        .Missing
                && DWORDAtA8CaptureState == StateForUnused(Capture.FreshDWORDAtESIPlusA8)
                && DWORDAtA4CaptureState == StateForUnused(Capture.FreshDWORDAtESIPlusA4);
        }

        if (Capture.FreshDWORDAtESIPlusA8 is not uint a8)
        {
            return Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                        .MissingFreshDWORDAtESIPlusA8
                && EsiCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
                        .Captured
                && DWORDAtA8CaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
                        .Missing
                && DWORDAtA4CaptureState == StateForUnused(Capture.FreshDWORDAtESIPlusA4);
        }

        if (Capture.FreshDWORDAtESIPlusA4 is not uint a4)
        {
            return Outcome
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                        .MissingFreshDWORDAtESIPlusA4
                && EsiCaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
                        .Captured
                && DWORDAtA8CaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
                        .Captured
                && DWORDAtA4CaptureState
                    == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
                        .Missing;
        }

        return Outcome
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                    .BoundedBeforeJz
            && EsiCaptureState
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured
            && DWORDAtA8CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured
            && DWORDAtA4CaptureState
                == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured
            && ESI == esi
            && EBX == 0
            && EAX == unchecked((uint)((int)unchecked(a8 - a4) >> 2));
    }

    private ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction>
        ExpectedInstructionPrefix()
    {
        var result =
            ImmutableArray.CreateBuilder<
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction>();
        NetworkDispatcherLockGateFlags flags = ParentPlan.FlagsAtStop ?? UnknownFlags;
        uint? esi = Capture.FreshPostCallESI;
        uint? eax = null;
        uint? ebx = null;
        if (esi is not uint freshESI)
        {
            result.Add(Instruction(
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .MoveEaxRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                    .MoveEaxFromEsiPlusA8,
                eax,
                eax,
                ebx,
                ebx,
                flags,
                flags,
                committed: false));
            return result.ToImmutable();
        }

        uint? a8 = Capture.FreshDWORDAtESIPlusA8;
        if (a8 is not uint firstDWORD)
        {
            result.Add(Instruction(
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .MoveEaxRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                    .MoveEaxFromEsiPlusA8,
                eax,
                eax,
                ebx,
                ebx,
                flags,
                flags,
                committed: false));
            return result.ToImmutable();
        }

        result.Add(Instruction(
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .MoveEaxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                .MoveEaxFromEsiPlusA8,
            eax,
            firstDWORD,
            ebx,
            ebx,
            flags,
            flags,
            committed: true));
        eax = firstDWORD;
        NetworkDispatcherLockGateFlags xorFlags = XorFlags(0);
        result.Add(Instruction(
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .XorEbxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind.XorEbxEbx,
            eax,
            eax,
            ebx,
            0,
            flags,
            xorFlags,
            committed: true));
        ebx = 0;
        flags = xorFlags;

        if (Capture.FreshDWORDAtESIPlusA4 is not uint secondDWORD)
        {
            result.Add(Instruction(
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .SubEaxRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                    .SubEaxFromEsiPlusA4,
                eax,
                eax,
                ebx,
                ebx,
                flags,
                flags,
                committed: false));
            return result.ToImmutable();
        }

        uint subResult = unchecked(firstDWORD - secondDWORD);
        NetworkDispatcherLockGateFlags subFlags = SubFlags(firstDWORD, secondDWORD, subResult);
        result.Add(Instruction(
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .SubEaxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                .SubEaxFromEsiPlusA4,
            eax,
            subResult,
            ebx,
            ebx,
            flags,
            subFlags,
            committed: true));
        eax = subResult;
        flags = subFlags;

        uint sarResult = unchecked((uint)((int)eax.Value >> 2));
        NetworkDispatcherLockGateFlags sarFlags = SarFlags(eax.Value, sarResult);
        result.Add(Instruction(
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .SarEaxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind.SarEaxByTwo,
            eax,
            sarResult,
            ebx,
            ebx,
            flags,
            sarFlags,
            committed: true));
        eax = sarResult;
        flags = sarFlags;

        NetworkDispatcherLockGateFlags testFlags = TestFlags(eax.Value);
        result.Add(Instruction(
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .TestEaxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind.TestEaxEax,
            eax,
            eax,
            ebx,
            ebx,
            flags,
            testFlags,
            committed: true));
        return result.ToImmutable();
    }

    private ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixRead>
        ExpectedReads()
    {
        var result =
            ImmutableArray.CreateBuilder<
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixRead>();
        if (Capture.FreshPostCallESI is not uint esi)
        {
            return result.ToImmutable();
        }

        result.Add(new(
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .MoveEaxRawFileOffset,
            unchecked(esi
                + NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .EsiA8Displacement),
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.DWORDWidth,
            Attempted: true,
            Captured: Capture.FreshDWORDAtESIPlusA8.HasValue,
            Value: Capture.FreshDWORDAtESIPlusA8));
        if (Capture.FreshDWORDAtESIPlusA8 is not uint)
        {
            return result.ToImmutable();
        }

        result.Add(new(
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .SubEaxRawFileOffset,
            unchecked(esi
                + NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .EsiA4Displacement),
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection.DWORDWidth,
            Attempted: true,
            Captured: Capture.FreshDWORDAtESIPlusA4.HasValue,
            Value: Capture.FreshDWORDAtESIPlusA4));
        return result.ToImmutable();
    }

    private ImmutableArray<uint> ExpectedCompletedOffsets() =>
        ParentPlan.CompletedInstructionOffsets
            .AddRange(ExpectedInstructionPrefix()
                .Where(instruction => instruction.Committed)
                .Select(instruction => instruction.InstructionRawFileOffset));

    private uint? ExpectedStopOffset()
    {
        foreach (NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction instruction
            in ExpectedInstructionPrefix())
        {
            if (!instruction.Committed)
            {
                return instruction.InstructionRawFileOffset;
            }
        }

        return NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
            .BranchRawFileOffset;
    }

    private uint? ExpectedCurrentEAX()
    {
        ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction>
            instructions = ExpectedInstructionPrefix();
        return instructions.IsEmpty ? null : instructions[^1].EAXAfter;
    }

    private uint? ExpectedCurrentEBX()
    {
        ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction>
            instructions = ExpectedInstructionPrefix();
        return instructions.IsEmpty ? null : instructions[^1].EBXAfter;
    }

    private NetworkDispatcherLockGateFlags? ExpectedCurrentFlags()
    {
        ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction>
            instructions = ExpectedInstructionPrefix();
        return instructions.IsEmpty
            ? ParentPlan.FlagsAtStop
            : instructions[^1].FlagsAfter;
    }

    internal static bool ParentAccepted(
        NetworkDispatcherEntryFallbackDiagnosticReturnPlan parent) =>
        parent.ExactCleanCompletion
        && parent.Outcome
            == NetworkDispatcherEntryFallbackDiagnosticReturnOutcome.BoundedAfterDirectCall
        && parent.NextInstructionBoundaryReached
        && parent.StopAtRawFileOffsetExclusive
            == NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .EntryRawFileOffset;

    private static NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction
        Instruction(
            uint rawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind kind,
            uint? eaxBefore,
            uint? eaxAfter,
            uint? ebxBefore,
            uint? ebxAfter,
            NetworkDispatcherLockGateFlags flagsBefore,
            NetworkDispatcherLockGateFlags flagsAfter,
            bool committed)
    {
        (byte length, string bytes) = InstructionEncoding(kind);
        return new(
            rawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                .RawFileOffsetToCodeVirtualAddress(rawFileOffset),
            length,
            bytes,
            kind,
            eaxBefore,
            eaxAfter,
            ebxBefore,
            ebxAfter,
            Attempted: true,
            Reached: true,
            Committed: committed,
            FlagsBefore: flagsBefore,
            FlagsAfter: flagsAfter);
    }

    internal static (byte Length, string Bytes) InstructionEncoding(
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind kind) =>
        kind switch
        {
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                .MoveEaxFromEsiPlusA8 => (6,
                    NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                        .MoveEaxInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind.XorEbxEbx =>
                (2, NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .XorEbxInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                .SubEaxFromEsiPlusA4 => (6,
                    NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                        .SubEaxInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind.SarEaxByTwo =>
                (3, NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .SarEaxInstructionBytes),
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind.TestEaxEax =>
                (2, NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
                    .TestEaxInstructionBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    private static NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
        StateForUnreachedParent(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Unreached;

    private static NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
        StateForUnused(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Unreached;

    private static NetworkDispatcherLockGateFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);

    private static NetworkDispatcherLockGateFlags XorFlags(uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: false);

    private static NetworkDispatcherLockGateFlags SubFlags(uint left, uint right, uint result) =>
        new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            Auxiliary: (left & 0xF) < (right & 0xF),
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static NetworkDispatcherLockGateFlags SarFlags(uint source, uint result) =>
        new(
            Carry: (source & 0x2) != 0,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: null);

    private static NetworkDispatcherLockGateFlags TestFlags(uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;
}

/// <summary>
/// Reprojects the post-call array-count prefix at BBDE2B and stops before its JZ.
/// </summary>
public static class NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixProjection
{
    public const uint EntryRawFileOffset = 0x00BBDE2B;
    public const uint MoveEaxRawFileOffset = 0x00BBDE2B;
    public const uint XorEbxRawFileOffset = 0x00BBDE31;
    public const uint SubEaxRawFileOffset = 0x00BBDE33;
    public const uint SarEaxRawFileOffset = 0x00BBDE39;
    public const uint TestEaxRawFileOffset = 0x00BBDE3C;
    public const uint BranchRawFileOffset = 0x00BBDE3E;
    public const uint BranchTargetRawFileOffset = 0x00BBDE83;
    public const uint BranchNextRawFileOffset = 0x00BBDE40;
    public const uint EsiA8Displacement = 0xA8;
    public const uint EsiA4Displacement = 0xA4;
    public const byte DWORDWidth = sizeof(uint);
    public const byte SarImmediate = 2;
    public const byte BranchInstructionByteLength = 2;
    public const byte CoordinatorProvidedByteLength = 21;
    public const int MaximumProjectedInstructionCount = 5;
    public const string MoveEaxInstructionBytes = "8B86A8000000";
    public const string XorEbxInstructionBytes = "33DB";
    public const string SubEaxInstructionBytes = "2B86A4000000";
    public const string SarEaxInstructionBytes = "C1F802";
    public const string TestEaxInstructionBytes = "85C0";
    public const string BranchInstructionBytes = "7443";
    public const string CoordinatorProvidedBytes =
        "8B86A800000033DB2B86A4000000C1F80285C07443";
    public const bool BytesIndependentlyVerified = true;

    public static uint EntryCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(EntryRawFileOffset);
    public static uint BranchCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(BranchRawFileOffset);
    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan Project(
        NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        ArgumentNullException.ThrowIfNull(capture.ParentCapture);

        NetworkDispatcherEntryFallbackDiagnosticReturnPlan parent =
            NetworkDispatcherEntryFallbackDiagnosticReturnProjection.Project(
                capture.ParentCapture);
        if (!NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan.ParentAccepted(parent))
        {
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                    .ParentCallReturnNotAccepted,
                StateForUnreachedParent(capture.FreshPostCallESI),
                StateForUnreachedParent(capture.FreshDWORDAtESIPlusA8),
                StateForUnreachedParent(capture.FreshDWORDAtESIPlusA4),
                ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixRead>.Empty,
                ImmutableArray<
                    NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction>.Empty,
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive,
                null,
                null,
                null,
                parent.FlagsAtStop,
                branchBoundaryReached: false,
                projectionEntered: false);
        }

        NetworkDispatcherLockGateFlags flags = parent.FlagsAtStop ?? UnknownFlags;
        if (capture.FreshPostCallESI is not uint esi)
        {
            var attemptedMove = Instruction(
                MoveEaxRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                    .MoveEaxFromEsiPlusA8,
                null,
                null,
                null,
                null,
                flags,
                flags,
                committed: false);
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                    .MissingFreshPostCallESI,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Missing,
                StateForUnused(capture.FreshDWORDAtESIPlusA8),
                StateForUnused(capture.FreshDWORDAtESIPlusA4),
                ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixRead>.Empty,
                ImmutableArray.Create(attemptedMove),
                parent.CompletedInstructionOffsets,
                MoveEaxRawFileOffset,
                null,
                null,
                null,
                flags,
                branchBoundaryReached: false,
                projectionEntered: true);
        }

        var firstRead = new NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixRead(
            MoveEaxRawFileOffset,
            unchecked(esi + EsiA8Displacement),
            DWORDWidth,
            Attempted: true,
            Captured: capture.FreshDWORDAtESIPlusA8.HasValue,
            Value: capture.FreshDWORDAtESIPlusA8);
        if (capture.FreshDWORDAtESIPlusA8 is not uint firstDWORD)
        {
            var attemptedMove = Instruction(
                MoveEaxRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                    .MoveEaxFromEsiPlusA8,
                null,
                null,
                null,
                null,
                flags,
                flags,
                committed: false);
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                    .MissingFreshDWORDAtESIPlusA8,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Missing,
                StateForUnused(capture.FreshDWORDAtESIPlusA4),
                ImmutableArray.Create(firstRead),
                ImmutableArray.Create(attemptedMove),
                parent.CompletedInstructionOffsets,
                MoveEaxRawFileOffset,
                esi,
                null,
                null,
                flags,
                branchBoundaryReached: false,
                projectionEntered: true);
        }

        var move = Instruction(
            MoveEaxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                .MoveEaxFromEsiPlusA8,
            null,
            firstDWORD,
            null,
            null,
            flags,
            flags,
            committed: true);
        NetworkDispatcherLockGateFlags xorFlags = XorFlags(0);
        var xor = Instruction(
            XorEbxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind.XorEbxEbx,
            firstDWORD,
            firstDWORD,
            null,
            0,
            flags,
            xorFlags,
            committed: true);
        ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction>
            instructions = ImmutableArray.Create(move, xor);
        ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixRead> reads =
            ImmutableArray.Create(firstRead);
        ImmutableArray<uint> completedOffsets = parent.CompletedInstructionOffsets
            .Add(MoveEaxRawFileOffset)
            .Add(XorEbxRawFileOffset);

        var secondRead = new NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixRead(
            SubEaxRawFileOffset,
            unchecked(esi + EsiA4Displacement),
            DWORDWidth,
            Attempted: true,
            Captured: capture.FreshDWORDAtESIPlusA4.HasValue,
            Value: capture.FreshDWORDAtESIPlusA4);
        if (capture.FreshDWORDAtESIPlusA4 is not uint secondDWORD)
        {
            var attemptedSub = Instruction(
                SubEaxRawFileOffset,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                    .SubEaxFromEsiPlusA4,
                firstDWORD,
                firstDWORD,
                0,
                0,
                xorFlags,
                xorFlags,
                committed: false);
            return new(
                capture,
                parent,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome
                    .MissingFreshDWORDAtESIPlusA4,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured,
                NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Missing,
                reads.Add(secondRead),
                instructions.Add(attemptedSub),
                completedOffsets,
                SubEaxRawFileOffset,
                esi,
                firstDWORD,
                0,
                xorFlags,
                branchBoundaryReached: false,
                projectionEntered: true);
        }

        uint subResult = unchecked(firstDWORD - secondDWORD);
        NetworkDispatcherLockGateFlags subFlags = SubFlags(firstDWORD, secondDWORD, subResult);
        var sub = Instruction(
            SubEaxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind
                .SubEaxFromEsiPlusA4,
            firstDWORD,
            subResult,
            0,
            0,
            xorFlags,
            subFlags,
            committed: true);
        uint sarResult = unchecked((uint)((int)subResult >> SarImmediate));
        NetworkDispatcherLockGateFlags sarFlags = SarFlags(subResult, sarResult);
        var sar = Instruction(
            SarEaxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind.SarEaxByTwo,
            subResult,
            sarResult,
            0,
            0,
            subFlags,
            sarFlags,
            committed: true);
        NetworkDispatcherLockGateFlags testFlags = TestFlags(sarResult);
        var test = Instruction(
            TestEaxRawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind.TestEaxEax,
            sarResult,
            sarResult,
            0,
            0,
            sarFlags,
            testFlags,
            committed: true);

        return new(
            capture,
            parent,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixOutcome.BoundedBeforeJz,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Captured,
            reads.Add(secondRead),
            instructions.Add(sub).Add(sar).Add(test),
            completedOffsets
                .Add(SubEaxRawFileOffset)
                .Add(SarEaxRawFileOffset)
                .Add(TestEaxRawFileOffset),
            BranchRawFileOffset,
            esi,
            sarResult,
            0,
            testFlags,
            branchBoundaryReached: true,
            projectionEntered: true);
    }

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset
            + NetworkDispatcherEntryFallbackDiagnosticReturnProjection
                .RawToCodeVirtualAddressDelta);

    private static NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
        StateForUnreachedParent(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Unreached;

    private static NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState
        StateForUnused(uint? value) =>
        value.HasValue
            ? NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.SuppliedUnused
            : NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixCaptureState.Unreached;

    private static NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstruction
        Instruction(
            uint rawFileOffset,
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixInstructionKind kind,
            uint? eaxBefore,
            uint? eaxAfter,
            uint? ebxBefore,
            uint? ebxAfter,
            NetworkDispatcherLockGateFlags flagsBefore,
            NetworkDispatcherLockGateFlags flagsAfter,
            bool committed)
    {
        (byte length, string bytes) =
            NetworkDispatcherEntryFallbackDiagnosticCallTargetPrefixPlan.InstructionEncoding(kind);
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
            Attempted: true,
            Reached: true,
            Committed: committed,
            FlagsBefore: flagsBefore,
            FlagsAfter: flagsAfter);
    }

    private static NetworkDispatcherLockGateFlags UnknownFlags =>
        new(Carry: null, Parity: null, Auxiliary: null, Zero: null, Sign: null, Overflow: null);

    private static NetworkDispatcherLockGateFlags XorFlags(uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: false);

    private static NetworkDispatcherLockGateFlags SubFlags(uint left, uint right, uint result) =>
        new(
            Carry: left < right,
            Parity: HasEvenParity((byte)result),
            Auxiliary: (left & 0xF) < (right & 0xF),
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);

    private static NetworkDispatcherLockGateFlags SarFlags(uint source, uint result) =>
        new(
            Carry: (source & 0x2) != 0,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: null);

    private static NetworkDispatcherLockGateFlags TestFlags(uint result) =>
        new(
            Carry: false,
            Parity: HasEvenParity((byte)result),
            Auxiliary: null,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: false);

    private static bool HasEvenParity(byte value) =>
        (BitOperations.PopCount((uint)value) & 1) == 0;
}
