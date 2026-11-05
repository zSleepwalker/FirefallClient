using System.Collections.Immutable;
using System.Threading;

namespace Firefall.Reconstruction.Startup;

public sealed record NetworkDispatcherEntryFallbackDiagnosticPreparationCapture(
    NetworkDispatcherEntryFallbackSelectorFallthroughCapture ParentCapture);

public enum NetworkDispatcherEntryFallbackDiagnosticPreparationOutcome
{
    ParentFallthroughNotAccepted,
    JgTakenRejected,
    ProjectedThreeInstructionPrefix
}

public enum NetworkDispatcherEntryFallbackDiagnosticPreparationInstructionKind
{
    PushImmediateDword,
    PushSignExtendedImmediateByte,
    MoveEcxImmediateDword
}

public readonly record struct NetworkDispatcherEntryFallbackDiagnosticPreparationInstruction(
    uint InstructionRawFileOffset,
    uint InstructionCodeVirtualAddress,
    byte InstructionByteLength,
    string InstructionBytes,
    NetworkDispatcherEntryFallbackDiagnosticPreparationInstructionKind Kind,
    uint StackPointerBefore,
    uint? StackAddress,
    uint StackPointerAfter,
    uint? OperandValue,
    bool Attempted,
    bool Reached,
    bool Committed,
    NetworkDispatcherLockGateFlags FlagsBefore,
    NetworkDispatcherLockGateFlags FlagsAfter);

public sealed class NetworkDispatcherEntryFallbackDiagnosticPreparationPlan
{
    private readonly Lazy<NetworkDispatcherEntryFallbackSelectorFallthroughPlan> reprojectedParentPlan;
    private readonly Lazy<bool> orderedSemanticPrefixPreserved;
    private readonly Lazy<bool> exactBoundedCompletion;
    private readonly Lazy<bool> exactCleanCompletion;

    internal NetworkDispatcherEntryFallbackDiagnosticPreparationPlan(
        NetworkDispatcherEntryFallbackDiagnosticPreparationCapture capture,
        NetworkDispatcherEntryFallbackSelectorFallthroughPlan parentPlan,
        NetworkDispatcherEntryFallbackDiagnosticPreparationOutcome outcome,
        ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticPreparationInstruction> instructions,
        ImmutableArray<uint> completedInstructionOffsets,
        uint? stopAtRawFileOffsetExclusive,
        uint? finalESP,
        uint? ecxAfterMove,
        bool parentFallthroughAccepted,
        bool projectionEntered)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Outcome = outcome;
        Instructions = instructions;
        CompletedInstructionOffsets = completedInstructionOffsets;
        StopAtRawFileOffsetExclusive = stopAtRawFileOffsetExclusive;
        FinalESP = finalESP;
        ECXAfterMove = ecxAfterMove;
        ParentFallthroughAccepted = parentFallthroughAccepted;
        ProjectionEntered = projectionEntered;
        reprojectedParentPlan = new(
            () => NetworkDispatcherEntryFallbackSelectorFallthroughProjection.Project(
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

    public NetworkDispatcherEntryFallbackDiagnosticPreparationCapture Capture { get; }
    public NetworkDispatcherEntryFallbackSelectorFallthroughPlan ParentPlan { get; }
    public NetworkDispatcherEntryFallbackDiagnosticPreparationOutcome Outcome { get; }
    public ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticPreparationInstruction>
        Instructions { get; }
    public ImmutableArray<uint> CompletedInstructionOffsets { get; }
    public uint EntryRawFileOffset =>
        NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.EntryRawFileOffset;
    public uint? StopAtRawFileOffsetExclusive { get; }
    public uint? FinalESP { get; }
    public uint? ECXAfterMove { get; }
    public bool ParentFallthroughAccepted { get; }
    public bool ProjectionEntered { get; }
    public NetworkDispatcherLockGateFlags? FlagsAtStop =>
        Instructions.IsEmpty ? ParentPlan.FlagsAtStop : Instructions[^1].FlagsAfter;
    public bool CallBoundaryReached =>
        ParentFallthroughAccepted
        && StopAtRawFileOffsetExclusive
            == NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .FirstCallBoundaryRawFileOffset;
    public bool CallExecutionAttempted => false;
    public bool NativeInstructionsExecuted => false;
    public bool NativeMemoryReadsExecuted => false;
    public bool NativeMemoryWritesExecuted => false;
    public bool ParentInstructionsExecuted => false;
    public bool OrderedSemanticPrefixPreserved => orderedSemanticPrefixPreserved.Value;
    public bool ExactBoundedCompletion => exactBoundedCompletion.Value;
    public bool ExactCleanCompletion => exactCleanCompletion.Value;

    private bool ComputeExactBoundedCompletion()
    {
        if (!ParentFallthroughAccepted
            || !ProjectionEntered
            || !ParentPlan.ExactCleanCompletion
            || ParentPlan.FlagsAtStop is not NetworkDispatcherLockGateFlags flags
            || ParentPlan.FinalESP is not uint entryESP
            || Instructions.Length
                != NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                    .MaximumProjectedInstructionCount)
        {
            return false;
        }

        uint firstESP = unchecked(entryESP
            - NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .StackPointerDecrement);
        uint secondESP = unchecked(firstESP
            - NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                .StackPointerDecrement);
        uint ecxImmediate =
            NetworkDispatcherEntryFallbackDiagnosticPreparationProjection.EcxImmediateValue;
        return Instructions[0] is {
                InstructionRawFileOffset: 0x00BBDE1A,
                InstructionCodeVirtualAddress: 0x00FBEA1A,
                InstructionByteLength: 5,
                InstructionBytes: "683C4FBB01",
                Kind: NetworkDispatcherEntryFallbackDiagnosticPreparationInstructionKind
                    .PushImmediateDword,
                OperandValue: 0x01BB4F3C,
                Attempted: true,
                Reached: true,
                Committed: true
            }
            && Instructions[0].StackPointerBefore == entryESP
            && Instructions[0].StackAddress == firstESP
            && Instructions[0].StackPointerAfter == firstESP
            && Instructions[0].FlagsBefore == flags
            && Instructions[0].FlagsAfter == flags
            && Instructions[1] is {
                InstructionRawFileOffset: 0x00BBDE1F,
                InstructionCodeVirtualAddress: 0x00FBEA1F,
                InstructionByteLength: 2,
                InstructionBytes: "6A01",
                Kind: NetworkDispatcherEntryFallbackDiagnosticPreparationInstructionKind
                    .PushSignExtendedImmediateByte,
                OperandValue: 1,
                Attempted: true,
                Reached: true,
                Committed: true
            }
            && Instructions[1].StackPointerBefore == firstESP
            && Instructions[1].StackAddress == secondESP
            && Instructions[1].StackPointerAfter == secondESP
            && Instructions[1].FlagsBefore == flags
            && Instructions[1].FlagsAfter == flags
            && Instructions[2] is {
                InstructionRawFileOffset: 0x00BBDE21,
                InstructionCodeVirtualAddress: 0x00FBEA21,
                InstructionByteLength: 5,
                InstructionBytes: "B98085E601",
                Kind: NetworkDispatcherEntryFallbackDiagnosticPreparationInstructionKind
                    .MoveEcxImmediateDword,
                OperandValue: 0x01E68580,
                Attempted: true,
                Reached: true,
                Committed: true
            }
            && Instructions[2].StackPointerBefore == secondESP
            && Instructions[2].StackAddress is null
            && Instructions[2].StackPointerAfter == secondESP
            && Instructions[2].FlagsBefore == flags
            && Instructions[2].FlagsAfter == flags
            && FinalESP == secondESP
            && ECXAfterMove == ecxImmediate
            && CompletedInstructionOffsets.SequenceEqual(
                ParentPlan.CompletedInstructionOffsets
                    .Add(0x00BBDE1A)
                    .Add(0x00BBDE1F)
                    .Add(0x00BBDE21))
            && CompletedInstructionOffsets[^1] == 0x00BBDE21
            && StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                    .FirstCallBoundaryRawFileOffset
            && FlagsAtStop == flags
            && CallBoundaryReached
            && !CallExecutionAttempted;
    }

    private bool ComputeExactCleanCompletion() =>
        ComputeExactBoundedCompletion()
        && OrderedSemanticPrefixPreserved
        && ParentPlan.ExactCleanCompletion;

    private bool ComputeOrderedSemanticPrefixPreserved()
    {
        NetworkDispatcherEntryFallbackSelectorFallthroughPlan actual = ParentPlan;
        NetworkDispatcherEntryFallbackSelectorFallthroughPlan reprojected =
            reprojectedParentPlan.Value;
        if (actual.Capture != reprojected.Capture
            || actual.Outcome != reprojected.Outcome
            || !actual.RegisterReads.SequenceEqual(reprojected.RegisterReads)
            || !actual.Pushes.SequenceEqual(reprojected.Pushes)
            || !actual.CompletedInstructionOffsets.SequenceEqual(
                reprojected.CompletedInstructionOffsets)
            || actual.StopAtRawFileOffsetExclusive != reprojected.StopAtRawFileOffsetExclusive
            || actual.FinalESP != reprojected.FinalESP
            || actual.ParentJgNotTakenAccepted != reprojected.ParentJgNotTakenAccepted
            || actual.ProjectionEntered != reprojected.ProjectionEntered
            || actual.FlagsAtStop != reprojected.FlagsAtStop
            || actual.FlagsPreserved != reprojected.FlagsPreserved
            || actual.ExactBoundedCompletion != reprojected.ExactBoundedCompletion
            || actual.ExactCleanCompletion != reprojected.ExactCleanCompletion
            || !actual.OrderedSemanticPrefixPreserved
            || !actual.ParentPlan.OrderedSemanticPrefixPreserved)
        {
            return false;
        }

        if (!ParentFallthroughAccepted)
        {
            return Instructions.IsEmpty
                && CompletedInstructionOffsets.SequenceEqual(
                    ParentPlan.CompletedInstructionOffsets)
                && StopAtRawFileOffsetExclusive == ParentPlan.StopAtRawFileOffsetExclusive
                && FinalESP == ParentPlan.FinalESP
                && ECXAfterMove is null
                && !ProjectionEntered;
        }

        return Instructions.Length
                == NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                    .MaximumProjectedInstructionCount
            && CompletedInstructionOffsets.SequenceEqual(
                ParentPlan.CompletedInstructionOffsets
                    .Add(0x00BBDE1A)
                    .Add(0x00BBDE1F)
                    .Add(0x00BBDE21))
            && StopAtRawFileOffsetExclusive
                == NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
                    .FirstCallBoundaryRawFileOffset
            && CallBoundaryReached
            && !CallExecutionAttempted;
    }
}

/// <summary>
/// Reprojects the three immediate stack/register preparations after BBDE1A and stops before CALL at BBDE26.
/// </summary>
public static class NetworkDispatcherEntryFallbackDiagnosticPreparationProjection
{
    public const uint EntryRawFileOffset = 0x00BBDE1A;
    public const uint SecondPushRawFileOffset = 0x00BBDE1F;
    public const uint MoveEcxRawFileOffset = 0x00BBDE21;
    public const uint FirstCallBoundaryRawFileOffset = 0x00BBDE26;
    public const byte PushImmediateDwordInstructionByteLength = 5;
    public const byte PushSignExtendedImmediateByteInstructionByteLength = 2;
    public const byte MoveEcxImmediateDwordInstructionByteLength = 5;
    public const byte CallBoundaryInstructionByteLength = 5;
    public const byte PushDwordWidth = sizeof(uint);
    public const uint StackPointerDecrement = sizeof(uint);
    public const uint PushImmediateValue = 0x01BB4F3C;
    public const uint SecondPushImmediateValue = 1;
    public const uint EcxImmediateValue = 0x01E68580;
    public const int MaximumProjectedInstructionCount = 3;
    public const string PushImmediateDwordInstructionBytes = "683C4FBB01";
    public const string PushSignExtendedImmediateByteInstructionBytes = "6A01";
    public const string MoveEcxImmediateDwordInstructionBytes = "B98085E601";
    public const string CallBoundaryInstructionBytes = "E805427000";
    public const string CapturedPreparationAndCallBoundaryBytes =
        "683C4FBB016A01B98085E601E805427000";
    public const bool CallBoundaryIsIndirect = false;
    public const bool BytesIndependentlyVerified = true;

    public static uint EntryCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(EntryRawFileOffset);
    public static uint SecondPushCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(SecondPushRawFileOffset);
    public static uint MoveEcxCodeVirtualAddress =>
        RawFileOffsetToCodeVirtualAddress(MoveEcxRawFileOffset);
    public static bool ExecutesNativeInstructions => false;

    public static NetworkDispatcherEntryFallbackDiagnosticPreparationPlan Project(
        NetworkDispatcherEntryFallbackDiagnosticPreparationCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);

        NetworkDispatcherEntryFallbackSelectorFallthroughPlan parent =
            NetworkDispatcherEntryFallbackSelectorFallthroughProjection.Project(
                capture.ParentCapture);
        bool parentFallthroughAccepted =
            parent.ExactCleanCompletion
            && parent.ParentJgNotTakenAccepted
            && parent.Outcome
                == NetworkDispatcherEntryFallbackSelectorFallthroughOutcome
                    .ProjectedTwoPushPrefix
            && parent.PushPrefixProjected
            && parent.FinalESP.HasValue
            && parent.StopAtRawFileOffsetExclusive == EntryRawFileOffset
            && parent.FlagsPreserved
            && parent.FlagsAtStop == parent.ParentPlan.FlagsAtStop
            && parent.CompletedInstructionOffsets.Length
                == parent.ParentPlan.CompletedInstructionOffsets.Length + 2;

        if (!parentFallthroughAccepted)
        {
            bool takenJg =
                parent.ParentPlan.ExactCleanCompletion
                && parent.ParentPlan.ParentSelectorCompareAccepted
                && parent.ParentPlan.Outcome
                    == NetworkDispatcherEntryFallbackSelectorBranchOutcome.JgTakenToBBDE2B
                && parent.JgTakenRejected
                && parent.StopAtRawFileOffsetExclusive
                    == parent.ParentPlan.StopAtRawFileOffsetExclusive;
            return new(
                capture,
                parent,
                takenJg
                    ? NetworkDispatcherEntryFallbackDiagnosticPreparationOutcome.JgTakenRejected
                    : NetworkDispatcherEntryFallbackDiagnosticPreparationOutcome
                        .ParentFallthroughNotAccepted,
                ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticPreparationInstruction>
                    .Empty,
                parent.CompletedInstructionOffsets,
                parent.StopAtRawFileOffsetExclusive,
                parent.FinalESP,
                null,
                parentFallthroughAccepted: false,
                projectionEntered: false);
        }

        NetworkDispatcherLockGateFlags flags = parent.FlagsAtStop
            ?? throw new InvalidOperationException("The accepted selector fallthrough has no flags.");
        uint initialESP = parent.FinalESP
            ?? throw new InvalidOperationException("The accepted selector fallthrough has no ESP.");
        uint firstESP = unchecked(initialESP - StackPointerDecrement);
        uint secondESP = unchecked(firstESP - StackPointerDecrement);
        ImmutableArray<NetworkDispatcherEntryFallbackDiagnosticPreparationInstruction> instructions =
            ImmutableArray.Create(
                new NetworkDispatcherEntryFallbackDiagnosticPreparationInstruction(
                    EntryRawFileOffset,
                    EntryCodeVirtualAddress,
                    PushImmediateDwordInstructionByteLength,
                    PushImmediateDwordInstructionBytes,
                    NetworkDispatcherEntryFallbackDiagnosticPreparationInstructionKind
                        .PushImmediateDword,
                    initialESP,
                    firstESP,
                    firstESP,
                    PushImmediateValue,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: flags,
                    FlagsAfter: flags),
                new NetworkDispatcherEntryFallbackDiagnosticPreparationInstruction(
                    SecondPushRawFileOffset,
                    SecondPushCodeVirtualAddress,
                    PushSignExtendedImmediateByteInstructionByteLength,
                    PushSignExtendedImmediateByteInstructionBytes,
                    NetworkDispatcherEntryFallbackDiagnosticPreparationInstructionKind
                        .PushSignExtendedImmediateByte,
                    firstESP,
                    secondESP,
                    secondESP,
                    SecondPushImmediateValue,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: flags,
                    FlagsAfter: flags),
                new NetworkDispatcherEntryFallbackDiagnosticPreparationInstruction(
                    MoveEcxRawFileOffset,
                    MoveEcxCodeVirtualAddress,
                    MoveEcxImmediateDwordInstructionByteLength,
                    MoveEcxImmediateDwordInstructionBytes,
                    NetworkDispatcherEntryFallbackDiagnosticPreparationInstructionKind
                        .MoveEcxImmediateDword,
                    secondESP,
                    null,
                    secondESP,
                    EcxImmediateValue,
                    Attempted: true,
                    Reached: true,
                    Committed: true,
                    FlagsBefore: flags,
                    FlagsAfter: flags));

        return new(
            capture,
            parent,
            NetworkDispatcherEntryFallbackDiagnosticPreparationOutcome
                .ProjectedThreeInstructionPrefix,
            instructions,
            parent.CompletedInstructionOffsets
                .Add(EntryRawFileOffset)
                .Add(SecondPushRawFileOffset)
                .Add(MoveEcxRawFileOffset),
            FirstCallBoundaryRawFileOffset,
            secondESP,
            EcxImmediateValue,
            parentFallthroughAccepted: true,
            projectionEntered: true);
    }

    public static uint RawFileOffsetToCodeVirtualAddress(uint rawFileOffset) =>
        unchecked(rawFileOffset + NetworkDispatcherLockGateProjection.RawToVirtualAddressDelta);
}
