using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct QueueEmbeddedPoolDrainCapture(
    uint SavedEmbeddedPoolPointer,
    uint CapturedQueuePointer,
    uint? InitialPopResult,
    bool? ReleaseCallReturnedNormally,
    uint? NextPopResult,
    bool? CleanupCallReturnedNormally);

public enum QueueEmbeddedPoolDrainOutcome
{
    InitialPopReturnUnavailableAtCallBoundary,
    ReleaseCallReturnUnavailableAtCallBoundary,
    ReleaseCallDidNotReturnAtCallBoundary,
    NextPopReturnUnavailableAtCallBoundary,
    NextPopNonzeroReachesSingleCycleRepeatBoundary,
    CleanupCallReturnUnavailableAtCallBoundary,
    CleanupCallDidNotReturnAtCallBoundary,
    CleanupReturnedToDestructorContinuationBoundary
}

public enum QueueEmbeddedPoolDrainActionKind
{
    ReloadSavedEmbeddedPoolPointer,
    FormInitialPopReceiver,
    CallInitialPop,
    TestInitialPopResult,
    BranchInitialPopZeroToCleanup,
    CallRelease,
    FormNextPopReceiver,
    CallNextPop,
    TestNextPopResult,
    BranchNextPopNonzeroToRepeatBoundary,
    ReachSingleCycleRepeatBoundary,
    FallThroughToCleanup,
    SelectCapturedQueueForCleanup,
    CallCleanup,
    ReachDestructorContinuationBoundary
}

public readonly record struct QueueEmbeddedPoolDrainAction(
    QueueEmbeddedPoolDrainActionKind Kind,
    uint RawFileOffset,
    uint? Pointer = null,
    uint? CapturedValue = null,
    bool? ConditionMet = null,
    bool? CallReturnedNormally = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

public readonly record struct QueueEmbeddedPoolDrainPlan(
    QueueEmbeddedPoolDrainCapture Capture,
    QueueEmbeddedPoolDrainOutcome Outcome,
    ImmutableArray<QueueEmbeddedPoolDrainAction> Actions,
    uint InitialPopReceiver,
    uint? NextPopReceiver,
    uint? CleanupReceiver,
    bool? InitialPopReturnedZero,
    bool? NextPopReturnedZero,
    uint PopTargetRawFileOffset,
    uint PopTargetVirtualAddress,
    uint ReleaseTargetRawFileOffset,
    uint ReleaseTargetVirtualAddress,
    uint CleanupTargetRawFileOffset,
    uint CleanupTargetVirtualAddress);

public readonly record struct QueueEmbeddedPoolDrainInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

/// <summary>
/// Projects the embedded-pool drain slice of the destructor for one captured
/// cycle. Pool and queue pointers are independent captures. Pop, release, and
/// cleanup calls are opaque boundaries; this does not model full termination,
/// freeing, or helper-side effects.
/// </summary>
public static class QueueEmbeddedPoolDrainProjection
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint DestructorEntryRawFileOffset = 0x12C02F0;
    public const uint SavedEmbeddedPoolPointerAddressRawFileOffset = 0x12C0333;
    public const uint SavedEmbeddedPoolPointerStoreRawFileOffset = 0x12C033D;
    public const uint PoolResetCallRawFileOffset = 0x12C0340;
    public const uint QueuePointerCaptureRawFileOffset = 0x12C0345;
    public const uint DrainStartRawFileOffset = 0x12C0382;
    public const uint DrainEndRawFileOffsetExclusive = 0x12C03AF;
    public const uint SavedEmbeddedPoolPointerReloadRawFileOffset = 0x12C0382;
    public const uint InitialPopReceiverSetupRawFileOffset = 0x12C0385;
    public const uint InitialPopCallRawFileOffset = 0x12C0388;
    public const uint InitialPopTestRawFileOffset = 0x12C038D;
    public const uint InitialPopZeroBranchRawFileOffset = 0x12C038F;
    public const uint ReleaseCallRawFileOffset = 0x12C0393;
    public const uint NextPopReceiverSetupRawFileOffset = 0x12C0398;
    public const uint NextPopCallRawFileOffset = 0x12C039B;
    public const uint NextPopTestRawFileOffset = 0x12C03A0;
    public const uint NextPopNonzeroBranchRawFileOffset = 0x12C03A2;
    public const uint SingleCycleRepeatBoundaryRawFileOffset = 0x12C0391;
    public const uint CleanupReceiverSetupRawFileOffset = 0x12C03A4;
    public const uint CleanupCallRawFileOffset = 0x12C03AA;
    public const uint DestructorContinuationBoundaryRawFileOffset = 0x12C03AF;

    public const uint PopTargetRawFileOffset = 0x12CDAE0;
    public const uint ReleaseTargetRawFileOffset = 0x12C8D60;
    public const uint CleanupTargetRawFileOffset = 0x12CE280;
    public const uint PoolAndQueueFieldOffset = 8;

    public const uint DrainStartVirtualAddress =
        DrainStartRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint DrainEndVirtualAddressExclusive =
        DrainEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint PopTargetVirtualAddress =
        PopTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint ReleaseTargetVirtualAddress =
        ReleaseTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint CleanupTargetVirtualAddress =
        CleanupTargetRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const string CapturedDrainBytes =
        "8B 7D F0 8D 4F 08 E8 53 D7 00 00 85 C0 74 13 8B C8 " +
        "E8 C8 89 00 00 8D 4F 08 E8 40 D7 00 00 85 C0 75 ED " +
        "8B CE C6 45 FC 00 E8 D1 DE 00 00";

    public static IReadOnlyList<QueueEmbeddedPoolDrainInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Form the embedded-pool address before the reset call",
                SavedEmbeddedPoolPointerAddressRawFileOffset,
                "8D 43 20"),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Save that embedded-pool pointer in the EBP-0x10 local",
                SavedEmbeddedPoolPointerStoreRawFileOffset,
                "89 45 F0"),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Call the pool reset routine after saving the pointer",
                PoolResetCallRawFileOffset,
                "E8 0B D9 00 00",
                0x12CDC50,
                0x16CE850),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Capture the queue pointer in ESI independently as this+8",
                QueuePointerCaptureRawFileOffset,
                "8D 73 08"),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Reload the saved embedded-pool pointer from EBP-0x10",
                SavedEmbeddedPoolPointerReloadRawFileOffset,
                "8B 7D F0"),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Form the initial pop receiver at savedPool+8",
                InitialPopReceiverSetupRawFileOffset,
                "8D 4F 08"),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Call the pop helper through the initial receiver",
                InitialPopCallRawFileOffset,
                "E8 53 D7 00 00",
                PopTargetRawFileOffset,
                PopTargetVirtualAddress),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Test the initial pop result in EAX",
                InitialPopTestRawFileOffset,
                "85 C0"),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Branch to queue cleanup when the initial pop result is zero",
                InitialPopZeroBranchRawFileOffset,
                "74 13",
                CleanupReceiverSetupRawFileOffset,
                CleanupReceiverSetupRawFileOffset
                    + CodeVirtualAddressDeltaFromRawFileOffset),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Pass a nonzero initial pop result in ECX to the release helper",
                ReleaseCallRawFileOffset - 2,
                "8B C8"),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Call the opaque release helper",
                ReleaseCallRawFileOffset,
                "E8 C8 89 00 00",
                ReleaseTargetRawFileOffset,
                ReleaseTargetVirtualAddress),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Form the next pop receiver again from the saved embedded-pool pointer",
                NextPopReceiverSetupRawFileOffset,
                "8D 4F 08"),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Call the pop helper again; its EAX result is tested but not passed to release here",
                NextPopCallRawFileOffset,
                "E8 40 D7 00 00",
                PopTargetRawFileOffset,
                PopTargetVirtualAddress),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Test the next pop result in EAX",
                NextPopTestRawFileOffset,
                "85 C0"),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Branch back to the release point when the next pop result is nonzero",
                NextPopNonzeroBranchRawFileOffset,
                "75 ED",
                SingleCycleRepeatBoundaryRawFileOffset,
                SingleCycleRepeatBoundaryRawFileOffset
                    + CodeVirtualAddressDeltaFromRawFileOffset),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Select the independently captured queue pointer in ESI for cleanup",
                CleanupReceiverSetupRawFileOffset,
                "8B CE"),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Clear the local unwind marker before calling cleanup",
                CleanupCallRawFileOffset - 4,
                "C6 45 FC 00"),
            new QueueEmbeddedPoolDrainInstructionEvidence(
                "Call the opaque queue cleanup helper",
                CleanupCallRawFileOffset,
                "E8 D1 DE 00 00",
                CleanupTargetRawFileOffset,
                CleanupTargetVirtualAddress)
        });

    /// <summary>
    /// Captures only the first pop and, if nonzero, its release plus one later
    /// pop. A nonzero later result stops at the observed repeat branch rather
    /// than simulating another iteration.
    /// </summary>
    public static QueueEmbeddedPoolDrainPlan Project(
        QueueEmbeddedPoolDrainCapture capture)
    {
        uint initialPopReceiver = unchecked(
            capture.SavedEmbeddedPoolPointer + PoolAndQueueFieldOffset);
        var actions = ImmutableArray.CreateBuilder<QueueEmbeddedPoolDrainAction>(9);
        actions.Add(new QueueEmbeddedPoolDrainAction(
            QueueEmbeddedPoolDrainActionKind.ReloadSavedEmbeddedPoolPointer,
            SavedEmbeddedPoolPointerReloadRawFileOffset,
            CapturedValue: capture.SavedEmbeddedPoolPointer));
        actions.Add(new QueueEmbeddedPoolDrainAction(
            QueueEmbeddedPoolDrainActionKind.FormInitialPopReceiver,
            InitialPopReceiverSetupRawFileOffset,
            Pointer: initialPopReceiver));
        actions.Add(new QueueEmbeddedPoolDrainAction(
            QueueEmbeddedPoolDrainActionKind.CallInitialPop,
            InitialPopCallRawFileOffset,
            Pointer: initialPopReceiver,
            CallReturnedNormally: capture.InitialPopResult.HasValue,
            TargetRawFileOffset: PopTargetRawFileOffset,
            TargetVirtualAddress: PopTargetVirtualAddress));

        if (capture.InitialPopResult is not uint initialPopResult)
        {
            Reject(capture.ReleaseCallReturnedNormally is null
                && capture.NextPopResult is null
                && capture.CleanupCallReturnedNormally is null,
                nameof(capture),
                "Later captures are unreachable before the initial pop returns.");
            return CreatePlan(
                capture,
                QueueEmbeddedPoolDrainOutcome.InitialPopReturnUnavailableAtCallBoundary,
                actions,
                initialPopReceiver,
                null,
                null,
                null,
                null);
        }

        bool initialPopReturnedZero = initialPopResult == 0;
        actions.Add(new QueueEmbeddedPoolDrainAction(
            QueueEmbeddedPoolDrainActionKind.TestInitialPopResult,
            InitialPopTestRawFileOffset,
            CapturedValue: initialPopResult,
            ConditionMet: initialPopReturnedZero));

        if (!initialPopReturnedZero)
        {
            actions.Add(new QueueEmbeddedPoolDrainAction(
                QueueEmbeddedPoolDrainActionKind.CallRelease,
                ReleaseCallRawFileOffset,
                Pointer: initialPopResult,
                CallReturnedNormally: capture.ReleaseCallReturnedNormally,
                TargetRawFileOffset: ReleaseTargetRawFileOffset,
                TargetVirtualAddress: ReleaseTargetVirtualAddress));

            if (capture.ReleaseCallReturnedNormally is not true)
            {
                Reject(capture.NextPopResult is null
                    && capture.CleanupCallReturnedNormally is null,
                    nameof(capture),
                    "The next pop and cleanup are unreachable unless the release call returns normally.");
                return CreatePlan(
                    capture,
                    capture.ReleaseCallReturnedNormally is null
                        ? QueueEmbeddedPoolDrainOutcome.ReleaseCallReturnUnavailableAtCallBoundary
                        : QueueEmbeddedPoolDrainOutcome.ReleaseCallDidNotReturnAtCallBoundary,
                    actions,
                    initialPopReceiver,
                    null,
                    null,
                    false,
                    null);
            }

            uint nextPopReceiver = initialPopReceiver;
            actions.Add(new QueueEmbeddedPoolDrainAction(
                QueueEmbeddedPoolDrainActionKind.FormNextPopReceiver,
                NextPopReceiverSetupRawFileOffset,
                Pointer: nextPopReceiver));
            actions.Add(new QueueEmbeddedPoolDrainAction(
                QueueEmbeddedPoolDrainActionKind.CallNextPop,
                NextPopCallRawFileOffset,
                Pointer: nextPopReceiver,
                CallReturnedNormally: capture.NextPopResult.HasValue,
                TargetRawFileOffset: PopTargetRawFileOffset,
                TargetVirtualAddress: PopTargetVirtualAddress));

            if (capture.NextPopResult is not uint nextPopResult)
            {
                Reject(capture.CleanupCallReturnedNormally is null,
                    nameof(capture),
                    "Cleanup is unreachable before the next pop result is captured.");
                return CreatePlan(
                    capture,
                    QueueEmbeddedPoolDrainOutcome.NextPopReturnUnavailableAtCallBoundary,
                    actions,
                    initialPopReceiver,
                    nextPopReceiver,
                    null,
                    false,
                    null);
            }

            bool nextPopReturnedZero = nextPopResult == 0;
            actions.Add(new QueueEmbeddedPoolDrainAction(
                QueueEmbeddedPoolDrainActionKind.TestNextPopResult,
                NextPopTestRawFileOffset,
                CapturedValue: nextPopResult,
                ConditionMet: nextPopReturnedZero));
            actions.Add(new QueueEmbeddedPoolDrainAction(
                QueueEmbeddedPoolDrainActionKind.BranchNextPopNonzeroToRepeatBoundary,
                NextPopNonzeroBranchRawFileOffset,
                ConditionMet: !nextPopReturnedZero,
                TargetRawFileOffset: SingleCycleRepeatBoundaryRawFileOffset,
                TargetVirtualAddress: SingleCycleRepeatBoundaryRawFileOffset
                    + CodeVirtualAddressDeltaFromRawFileOffset));

            if (!nextPopReturnedZero)
            {
                Reject(capture.CleanupCallReturnedNormally is null,
                    nameof(capture),
                    "Cleanup is unreachable after a nonzero next pop result.");
                actions.Add(new QueueEmbeddedPoolDrainAction(
                    QueueEmbeddedPoolDrainActionKind.ReachSingleCycleRepeatBoundary,
                    SingleCycleRepeatBoundaryRawFileOffset));
                return CreatePlan(
                    capture,
                    QueueEmbeddedPoolDrainOutcome.NextPopNonzeroReachesSingleCycleRepeatBoundary,
                    actions,
                    initialPopReceiver,
                    nextPopReceiver,
                    null,
                    false,
                    false);
            }

            actions.Add(new QueueEmbeddedPoolDrainAction(
                QueueEmbeddedPoolDrainActionKind.FallThroughToCleanup,
                CleanupReceiverSetupRawFileOffset,
                ConditionMet: true));
            return ProjectCleanup(
                capture,
                actions,
                initialPopReceiver,
                nextPopReceiver,
                false,
                true);
        }

        Reject(capture.ReleaseCallReturnedNormally is null
            && capture.NextPopResult is null,
            nameof(capture),
            "The release and next-pop captures are unreachable after an initial zero result.");
        actions.Add(new QueueEmbeddedPoolDrainAction(
            QueueEmbeddedPoolDrainActionKind.BranchInitialPopZeroToCleanup,
            InitialPopZeroBranchRawFileOffset,
            ConditionMet: true,
            TargetRawFileOffset: CleanupReceiverSetupRawFileOffset,
            TargetVirtualAddress: CleanupReceiverSetupRawFileOffset
                + CodeVirtualAddressDeltaFromRawFileOffset));
        return ProjectCleanup(
            capture,
            actions,
            initialPopReceiver,
            null,
            true,
            null);
    }

    private static QueueEmbeddedPoolDrainPlan ProjectCleanup(
        QueueEmbeddedPoolDrainCapture capture,
        ImmutableArray<QueueEmbeddedPoolDrainAction>.Builder actions,
        uint initialPopReceiver,
        uint? nextPopReceiver,
        bool? initialPopReturnedZero,
        bool? nextPopReturnedZero)
    {
        actions.Add(new QueueEmbeddedPoolDrainAction(
            QueueEmbeddedPoolDrainActionKind.SelectCapturedQueueForCleanup,
            CleanupReceiverSetupRawFileOffset,
            Pointer: capture.CapturedQueuePointer));
        actions.Add(new QueueEmbeddedPoolDrainAction(
            QueueEmbeddedPoolDrainActionKind.CallCleanup,
            CleanupCallRawFileOffset,
            Pointer: capture.CapturedQueuePointer,
            CallReturnedNormally: capture.CleanupCallReturnedNormally,
            TargetRawFileOffset: CleanupTargetRawFileOffset,
            TargetVirtualAddress: CleanupTargetVirtualAddress));

        if (capture.CleanupCallReturnedNormally is not true)
        {
            return CreatePlan(
                capture,
                capture.CleanupCallReturnedNormally is null
                    ? QueueEmbeddedPoolDrainOutcome.CleanupCallReturnUnavailableAtCallBoundary
                    : QueueEmbeddedPoolDrainOutcome.CleanupCallDidNotReturnAtCallBoundary,
                actions,
                initialPopReceiver,
                nextPopReceiver,
                capture.CapturedQueuePointer,
                initialPopReturnedZero,
                nextPopReturnedZero);
        }

        actions.Add(new QueueEmbeddedPoolDrainAction(
            QueueEmbeddedPoolDrainActionKind.ReachDestructorContinuationBoundary,
            DestructorContinuationBoundaryRawFileOffset));
        return CreatePlan(
            capture,
            QueueEmbeddedPoolDrainOutcome.CleanupReturnedToDestructorContinuationBoundary,
            actions,
            initialPopReceiver,
            nextPopReceiver,
            capture.CapturedQueuePointer,
            initialPopReturnedZero,
            nextPopReturnedZero);
    }

    private static QueueEmbeddedPoolDrainPlan CreatePlan(
        QueueEmbeddedPoolDrainCapture capture,
        QueueEmbeddedPoolDrainOutcome outcome,
        ImmutableArray<QueueEmbeddedPoolDrainAction>.Builder actions,
        uint initialPopReceiver,
        uint? nextPopReceiver,
        uint? cleanupReceiver,
        bool? initialPopReturnedZero,
        bool? nextPopReturnedZero) =>
        new(
            capture,
            outcome,
            actions.ToImmutable(),
            initialPopReceiver,
            nextPopReceiver,
            cleanupReceiver,
            initialPopReturnedZero,
            nextPopReturnedZero,
            PopTargetRawFileOffset,
            PopTargetVirtualAddress,
            ReleaseTargetRawFileOffset,
            ReleaseTargetVirtualAddress,
            CleanupTargetRawFileOffset,
            CleanupTargetVirtualAddress);

    private static void Reject(bool condition, string parameterName, string message)
    {
        if (!condition)
        {
            throw new ArgumentException(message, parameterName);
        }
    }
}
