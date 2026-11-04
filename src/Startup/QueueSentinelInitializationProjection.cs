namespace Firefall.Reconstruction.Startup;

public enum QueueSentinelInitializationCaptureKind
{
    Missing,
    Unreached,
    Captured
}

public readonly struct QueueSentinelInitializationCapture<T> where T : struct
{
    private QueueSentinelInitializationCapture(
        QueueSentinelInitializationCaptureKind kind,
        T? value)
    {
        Kind = kind;
        Value = value;
    }

    public QueueSentinelInitializationCaptureKind Kind { get; }
    public T? Value { get; }

    public static QueueSentinelInitializationCapture<T> Missing() =>
        new(QueueSentinelInitializationCaptureKind.Missing, null);

    public static QueueSentinelInitializationCapture<T> Unreached() =>
        new(QueueSentinelInitializationCaptureKind.Unreached, null);

    public static QueueSentinelInitializationCapture<T> Captured(T value) =>
        new(QueueSentinelInitializationCaptureKind.Captured, value);
}

public readonly record struct QueueSentinelInitializationCaptures(
    QueueSentinelInitializationCapture<uint> ReceiverPointer,
    QueueSentinelInitializationCapture<uint> InitialPoolPointer,
    QueueSentinelInitializationCapture<bool> PoolInitializerReturned,
    QueueSentinelInitializationCapture<bool> AlignmentDiagnosticReturned,
    QueueSentinelInitializationCapture<uint> ReloadedPoolPointer,
    QueueSentinelInitializationCapture<uint> AcquiredPointer);

public enum QueueSentinelInitializationOutcome
{
    ReceiverPointerUnavailable,
    InitialPoolPointerUnavailable,
    PoolInitializerReturnUnavailable,
    PoolInitializerDidNotReturn,
    AlignmentDiagnosticReturnUnavailable,
    AlignmentDiagnosticDidNotReturn,
    ReloadedPoolPointerUnavailable,
    AcquisitionReturnUnavailable,
    NormalReturnReached
}

public enum QueueSentinelInitializationActionKind
{
    CaptureReceiverPointer,
    ClearReceiverWord10,
    CaptureInitialPoolPointer,
    CallOpaquePoolInitializer,
    CapturePoolInitializerReturn,
    ContinueAfterPoolInitializer,
    SkipPoolInitializer,
    TestReceiverAlignment,
    SkipAlignmentDiagnostic,
    ReportAlignmentDiagnostic,
    CaptureAlignmentDiagnosticReturn,
    ContinueAfterAlignmentDiagnostic,
    CleanupAlignmentDiagnosticArguments,
    ReloadPoolPointer,
    CallOpaquePoolAcquisition,
    CaptureAcquiredPointer,
    TestAcquiredPointer,
    ZeroAcquiredQueueQword,
    SkipAcquiredQueueQwordStore,
    SetNullAcquiredPointer,
    PublishQueueTail,
    PublishQueueHead,
    CaptureReturnReceiver,
    ClearReceiverWord0C,
    ClearReceiverWord4,
    ReturnReceiver,
    StopAtMissingReceiverPointer,
    StopAtMissingInitialPoolPointer,
    StopAtMissingPoolInitializerReturn,
    StopAtPoolInitializerNonreturn,
    StopAtMissingAlignmentDiagnosticReturn,
    StopAtAlignmentDiagnosticNonreturn,
    StopAtMissingReloadedPoolPointer,
    StopAtMissingAcquisitionReturn
}

public readonly record struct QueueSentinelInitializationAction(
    QueueSentinelInitializationActionKind Kind,
    uint InstructionRawFileOffset,
    int? ReceiverRelativeOffset = null,
    uint? CapturedValue = null,
    uint? Value = null,
    uint? SecondaryValue = null,
    uint? DiagnosticId = null,
    bool? HelperReturned = null,
    uint? ReferencedVirtualAddress = null,
    uint? CallTargetRawEa = null,
    uint? BranchTargetRawFileOffset = null);

public readonly record struct QueueSentinelInitializationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? ReceiverRelativeOffset = null,
    uint? ReferencedVirtualAddress = null,
    uint? DiagnosticId = null,
    uint? BranchTargetRawFileOffset = null,
    uint? CallTargetRawEa = null);

public readonly record struct QueueSentinelInitializationPlan(
    QueueSentinelInitializationCaptures Captures,
    QueueSentinelInitializationOutcome Outcome,
    IReadOnlyList<QueueSentinelInitializationAction> Actions,
    uint? ReturnReceiverPointer);

/// <summary>
/// Projects the queue initializer's captured inputs and stores over raw file
/// offsets 0x12CE210-0x12CE280. Opaque initializer, diagnostic, and acquisition
/// callees are represented only by captured return boundaries.
/// </summary>
public static class QueueSentinelInitializationProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint EntryRawFileOffset = 0x12CE210;
    public const uint EndRawFileOffsetExclusive = 0x12CE280;
    public const uint EntryVirtualAddress =
        EntryRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint EndVirtualAddress =
        EndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint PoolInitializerCallTargetRawEa = 0x00BBFAC0;
    public const uint PoolBaseVirtualAddress = 0x01E003AC;
    public const uint PoolPointerGlobalVirtualAddress = 0x01E003BC;
    public const uint AcquisitionHelperCallTargetRawEa = 0x012CDE60;
    public const uint DiagnosticReporterCellVirtualAddress = 0x01C15EA0;
    public const uint AlignmentDiagnosticId = 0x2CD;

    public const uint ReceiverPointerCaptureRawFileOffset = 0x12CE211;
    public const uint ReceiverWord10ClearRawFileOffset = 0x12CE213;
    public const uint InitialPoolPointerCaptureRawFileOffset = 0x12CE21A;
    public const uint PoolInitializerCallRawFileOffset = 0x12CE228;
    public const uint PoolInitializerContinuationRawFileOffset = 0x12CE22D;
    public const uint AlignmentTestRawFileOffset = 0x12CE22D;
    public const uint AlignmentDiagnosticBranchRawFileOffset = 0x12CE230;
    public const uint AlignmentDiagnosticCallRawFileOffset = 0x12CE243;
    public const uint AlignmentDiagnosticContinuationRawFileOffset = 0x12CE249;
    public const uint ReloadedPoolPointerCaptureRawFileOffset = 0x12CE24C;
    public const uint AcquisitionCallRawFileOffset = 0x12CE252;
    public const uint AcquiredPointerTestRawFileOffset = 0x12CE257;
    public const uint AcquiredQueueQwordZeroRawFileOffset = 0x12CE25E;
    public const uint AcquiredPointerNullBranchRawFileOffset = 0x12CE259;
    public const uint QueueTailPublishRawFileOffset = 0x12CE266;
    public const uint QueueHeadPublishRawFileOffset = 0x12CE269;
    public const uint ReturnReceiverCaptureRawFileOffset = 0x12CE26B;
    public const uint ReceiverWord0CClearRawFileOffset = 0x12CE26D;
    public const uint ReceiverWord4ClearRawFileOffset = 0x12CE274;
    public const uint ReturnRawFileOffset = 0x12CE27C;

    public const int ReceiverWord10Offset = 0x10;
    public const int QueueHeadOffset = 0x00;
    public const int QueueTailOffset = 0x08;
    public const int ReceiverWord0COffset = 0x0C;
    public const int ReceiverWord4Offset = 0x04;
    public const int AcquiredQueueQwordSize = 8;

    public const uint ConstructorQueueReceiverSetupRawFileOffset = 0x12C0239;
    public const uint ConstructorQueueInitializationCallRawFileOffset = 0x12C0242;
    public const uint ConstructorQueueInitializationCallVirtualAddress =
        ConstructorQueueInitializationCallRawFileOffset
            + CodeVirtualAddressDeltaFromRawFileOffset;
    public const int ConstructorQueueReceiverOffset = 0x08;
    public const uint ConstructorAllocatorConfigCallRawFileOffset = 0x12C0254;
    public const int ConstructorAllocatorConfigReceiverOffset = 0x20;
    public const uint ConstructorAllocatorConfigArgument0 = 12;
    public const uint ConstructorAllocatorConfigArgument1 = 8;
    public const uint ConstructorAllocatorConfigArgument2 = 1;
    public const uint AllocatorConfigFunctionCallTargetRawEa = 0x012CDB40;

    public const string CapturedFunctionBytes =
        "53 8B D9 C7 43 10 00 00 00 00 83 3D BC 03 E0 01 00 75 0A " +
        "B9 AC 03 E0 01 E8 93 18 8F FF F6 C3 03 74 1A 6A 00 68 CD " +
        "02 00 00 68 AB 43 B7 01 68 C3 43 B7 01 FF 15 A0 5E C1 01 " +
        "83 C4 10 8B 0D BC 03 E0 01 E8 09 FC FF FF 85 C0 74 09 0F " +
        "57 C0 66 0F D6 00 EB 02 33 C0 89 43 08 89 03 8B C3 C7 43 " +
        "0C 00 00 00 00 C7 43 04 00 00 00 00 5B C3 CC CC CC";

    public static IReadOnlyList<QueueSentinelInitializationInstructionEvidence> Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new QueueSentinelInitializationInstructionEvidence(
                "Capture the receiver in EBX",
                ReceiverPointerCaptureRawFileOffset,
                "8B D9"),
            new QueueSentinelInitializationInstructionEvidence(
                "Clear receiver +0x10 before any call",
                ReceiverWord10ClearRawFileOffset,
                "C7 43 10 00 00 00 00",
                ReceiverWord10Offset),
            new QueueSentinelInitializationInstructionEvidence(
                "Read the initial pool-pointer gate",
                InitialPoolPointerCaptureRawFileOffset,
                "83 3D BC 03 E0 01 00",
                ReferencedVirtualAddress: PoolPointerGlobalVirtualAddress),
            new QueueSentinelInitializationInstructionEvidence(
                "Skip opaque pool initialization when the initial gate is nonzero",
                0x12CE221,
                "75 0A",
                BranchTargetRawFileOffset: PoolInitializerContinuationRawFileOffset),
            new QueueSentinelInitializationInstructionEvidence(
                "Load the pool base before the opaque initializer call",
                0x12CE223,
                "B9 AC 03 E0 01",
                ReferencedVirtualAddress: PoolBaseVirtualAddress),
            new QueueSentinelInitializationInstructionEvidence(
                "Call the opaque pool initializer",
                PoolInitializerCallRawFileOffset,
                "E8 93 18 8F FF",
                CallTargetRawEa: PoolInitializerCallTargetRawEa),
            new QueueSentinelInitializationInstructionEvidence(
                "Test the captured receiver's low alignment bits",
                AlignmentTestRawFileOffset,
                "F6 C3 03"),
            new QueueSentinelInitializationInstructionEvidence(
                "Skip the alignment diagnostic for a four-byte-aligned receiver",
                AlignmentDiagnosticBranchRawFileOffset,
                "74 1A",
                BranchTargetRawFileOffset: ReloadedPoolPointerCaptureRawFileOffset),
            new QueueSentinelInitializationInstructionEvidence(
                "Push diagnostic ID 0x2CD",
                0x12CE232,
                "6A 00 68 CD 02 00 00",
                DiagnosticId: AlignmentDiagnosticId),
            new QueueSentinelInitializationInstructionEvidence(
                "Call the diagnostic reporter through its indirect cell",
                AlignmentDiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                ReferencedVirtualAddress: DiagnosticReporterCellVirtualAddress),
            new QueueSentinelInitializationInstructionEvidence(
                "Clean up diagnostic arguments after its normal return",
                AlignmentDiagnosticContinuationRawFileOffset,
                "83 C4 10"),
            new QueueSentinelInitializationInstructionEvidence(
                "Independently reload the global pool pointer after the diagnostic path",
                ReloadedPoolPointerCaptureRawFileOffset,
                "8B 0D BC 03 E0 01",
                ReferencedVirtualAddress: PoolPointerGlobalVirtualAddress),
            new QueueSentinelInitializationInstructionEvidence(
                "Call opaque queue acquisition with the reloaded pool pointer",
                AcquisitionCallRawFileOffset,
                "E8 09 FC FF FF",
                CallTargetRawEa: AcquisitionHelperCallTargetRawEa),
            new QueueSentinelInitializationInstructionEvidence(
                "Test the captured acquisition result",
                AcquiredPointerTestRawFileOffset,
                "85 C0"),
            new QueueSentinelInitializationInstructionEvidence(
                "Skip the qword zero store when acquisition returned null",
                AcquiredPointerNullBranchRawFileOffset,
                "74 09",
                BranchTargetRawFileOffset: 0x12CE264),
            new QueueSentinelInitializationInstructionEvidence(
                "Zero eight bytes at a non-null acquired queue pointer",
                AcquiredQueueQwordZeroRawFileOffset,
                "66 0F D6 00"),
            new QueueSentinelInitializationInstructionEvidence(
                "Join the non-null and null acquisition paths",
                0x12CE262,
                "EB 02",
                BranchTargetRawFileOffset: QueueTailPublishRawFileOffset),
            new QueueSentinelInitializationInstructionEvidence(
                "Set the null acquisition result explicitly to zero",
                0x12CE264,
                "33 C0"),
            new QueueSentinelInitializationInstructionEvidence(
                "Publish the acquired pointer at receiver +8",
                QueueTailPublishRawFileOffset,
                "89 43 08",
                QueueTailOffset),
            new QueueSentinelInitializationInstructionEvidence(
                "Publish the acquired pointer at receiver +0",
                QueueHeadPublishRawFileOffset,
                "89 03",
                QueueHeadOffset),
            new QueueSentinelInitializationInstructionEvidence(
                "Move the receiver into the return register before trailing clears",
                ReturnReceiverCaptureRawFileOffset,
                "8B C3"),
            new QueueSentinelInitializationInstructionEvidence(
                "Clear receiver +0x0C after publishing both pointers",
                ReceiverWord0CClearRawFileOffset,
                "C7 43 0C 00 00 00 00",
                ReceiverWord0COffset),
            new QueueSentinelInitializationInstructionEvidence(
                "Clear receiver +4 after receiver +0x0C",
                ReceiverWord4ClearRawFileOffset,
                "C7 43 04 00 00 00 00",
                ReceiverWord4Offset),
            new QueueSentinelInitializationInstructionEvidence(
                "Return the captured receiver",
                ReturnRawFileOffset,
                "C3")
        });

    public static IReadOnlyList<QueueSentinelInitializationInstructionEvidence> ConstructorBindingInstructions { get; } =
        Array.AsReadOnly(new[]
        {
            new QueueSentinelInitializationInstructionEvidence(
                "Set ECX to constructor this +8 for queue initialization",
                ConstructorQueueReceiverSetupRawFileOffset,
                "8D 4E 08",
                ReceiverRelativeOffset: ConstructorQueueReceiverOffset),
            new QueueSentinelInitializationInstructionEvidence(
                "Install the constructor vtable between receiver setup and queue initialization",
                0x12C023C,
                "C7 06 54 0C 80 01"),
            new QueueSentinelInitializationInstructionEvidence(
                "Call queue initialization from the constructor",
                ConstructorQueueInitializationCallRawFileOffset,
                "E8 C9 DF 00 00",
                CallTargetRawEa: EntryRawFileOffset),
            new QueueSentinelInitializationInstructionEvidence(
                "Pass allocator configuration argument 1",
                0x12C0247,
                "6A 01"),
            new QueueSentinelInitializationInstructionEvidence(
                "Pass allocator configuration argument 8",
                0x12C0249,
                "6A 08"),
            new QueueSentinelInitializationInstructionEvidence(
                "Set the embedded allocator configuration receiver to constructor this +0x20",
                0x12C024B,
                "8D 4E 20",
                ReceiverRelativeOffset: ConstructorAllocatorConfigReceiverOffset),
            new QueueSentinelInitializationInstructionEvidence(
                "Pass allocator configuration argument 12",
                0x12C0252,
                "6A 0C"),
            new QueueSentinelInitializationInstructionEvidence(
                "Call the embedded allocator configuration initializer",
                ConstructorAllocatorConfigCallRawFileOffset,
                "E8 E7 D8 00 00",
                CallTargetRawEa: AllocatorConfigFunctionCallTargetRawEa)
        });

    public static QueueSentinelInitializationPlan Project(
        QueueSentinelInitializationCaptures captures)
    {
        var actions = new List<QueueSentinelInitializationAction>();

        if (!TryCapture(
                captures.ReceiverPointer,
                ReceiverPointerCaptureRawFileOffset,
                QueueSentinelInitializationActionKind.CaptureReceiverPointer,
                QueueSentinelInitializationActionKind.StopAtMissingReceiverPointer,
                actions,
                out uint receiver))
        {
            RequireUnreached(
                captures.InitialPoolPointer,
                nameof(captures.InitialPoolPointer));
            RequireUnreached(
                captures.PoolInitializerReturned,
                nameof(captures.PoolInitializerReturned));
            RequireUnreached(
                captures.AlignmentDiagnosticReturned,
                nameof(captures.AlignmentDiagnosticReturned));
            RequireUnreached(
                captures.ReloadedPoolPointer,
                nameof(captures.ReloadedPoolPointer));
            RequireUnreached(
                captures.AcquiredPointer,
                nameof(captures.AcquiredPointer));
            return Plan(captures, QueueSentinelInitializationOutcome.ReceiverPointerUnavailable, actions);
        }

        actions.Add(new QueueSentinelInitializationAction(
            QueueSentinelInitializationActionKind.ClearReceiverWord10,
            ReceiverWord10ClearRawFileOffset,
            ReceiverWord10Offset,
            Value: 0));

        if (!TryCapture(
                captures.InitialPoolPointer,
                InitialPoolPointerCaptureRawFileOffset,
                QueueSentinelInitializationActionKind.CaptureInitialPoolPointer,
                QueueSentinelInitializationActionKind.StopAtMissingInitialPoolPointer,
                actions,
                out uint initialPool))
        {
            RequireUnreached(
                captures.PoolInitializerReturned,
                nameof(captures.PoolInitializerReturned));
            RequireUnreached(
                captures.AlignmentDiagnosticReturned,
                nameof(captures.AlignmentDiagnosticReturned));
            RequireUnreached(
                captures.ReloadedPoolPointer,
                nameof(captures.ReloadedPoolPointer));
            RequireUnreached(
                captures.AcquiredPointer,
                nameof(captures.AcquiredPointer));
            return Plan(captures, QueueSentinelInitializationOutcome.InitialPoolPointerUnavailable, actions);
        }

        if (initialPool == 0)
        {
            actions.Add(new QueueSentinelInitializationAction(
                QueueSentinelInitializationActionKind.CallOpaquePoolInitializer,
                PoolInitializerCallRawFileOffset,
                ReferencedVirtualAddress: PoolBaseVirtualAddress,
                CallTargetRawEa: PoolInitializerCallTargetRawEa));
            if (!TryCapture(
                    captures.PoolInitializerReturned,
                    PoolInitializerContinuationRawFileOffset,
                    QueueSentinelInitializationActionKind.CapturePoolInitializerReturn,
                    QueueSentinelInitializationActionKind.StopAtMissingPoolInitializerReturn,
                    actions,
                    out bool initializerReturned))
            {
                RequireUnreached(
                    captures.AlignmentDiagnosticReturned,
                    nameof(captures.AlignmentDiagnosticReturned));
                RequireUnreached(
                    captures.ReloadedPoolPointer,
                    nameof(captures.ReloadedPoolPointer));
                RequireUnreached(
                    captures.AcquiredPointer,
                    nameof(captures.AcquiredPointer));
                return Plan(captures, QueueSentinelInitializationOutcome.PoolInitializerReturnUnavailable, actions);
            }

            if (!initializerReturned)
            {
                actions.Add(new QueueSentinelInitializationAction(
                    QueueSentinelInitializationActionKind.StopAtPoolInitializerNonreturn,
                    PoolInitializerCallRawFileOffset,
                    HelperReturned: false));
                RequireUnreached(
                    captures.AlignmentDiagnosticReturned,
                    nameof(captures.AlignmentDiagnosticReturned));
                RequireUnreached(
                    captures.ReloadedPoolPointer,
                    nameof(captures.ReloadedPoolPointer));
                RequireUnreached(
                    captures.AcquiredPointer,
                    nameof(captures.AcquiredPointer));
                return Plan(captures, QueueSentinelInitializationOutcome.PoolInitializerDidNotReturn, actions);
            }

            actions.Add(new QueueSentinelInitializationAction(
                QueueSentinelInitializationActionKind.ContinueAfterPoolInitializer,
                PoolInitializerContinuationRawFileOffset,
                HelperReturned: true));
        }
        else
        {
            RequireUnreached(
                captures.PoolInitializerReturned,
                nameof(captures.PoolInitializerReturned));
            actions.Add(new QueueSentinelInitializationAction(
                QueueSentinelInitializationActionKind.SkipPoolInitializer,
                0x12CE221,
                CapturedValue: initialPool,
                BranchTargetRawFileOffset: PoolInitializerContinuationRawFileOffset));
        }

        actions.Add(new QueueSentinelInitializationAction(
            QueueSentinelInitializationActionKind.TestReceiverAlignment,
            AlignmentTestRawFileOffset,
            CapturedValue: receiver,
            Value: receiver & 3));

        if ((receiver & 3) == 0)
        {
            RequireUnreached(
                captures.AlignmentDiagnosticReturned,
                nameof(captures.AlignmentDiagnosticReturned));
            actions.Add(new QueueSentinelInitializationAction(
                QueueSentinelInitializationActionKind.SkipAlignmentDiagnostic,
                AlignmentDiagnosticBranchRawFileOffset,
                CapturedValue: receiver & 3,
                BranchTargetRawFileOffset: ReloadedPoolPointerCaptureRawFileOffset));
        }
        else
        {
            actions.Add(new QueueSentinelInitializationAction(
                QueueSentinelInitializationActionKind.ReportAlignmentDiagnostic,
                AlignmentDiagnosticCallRawFileOffset,
                DiagnosticId: AlignmentDiagnosticId,
                ReferencedVirtualAddress: DiagnosticReporterCellVirtualAddress));
            if (!TryCapture(
                    captures.AlignmentDiagnosticReturned,
                    AlignmentDiagnosticContinuationRawFileOffset,
                    QueueSentinelInitializationActionKind.CaptureAlignmentDiagnosticReturn,
                    QueueSentinelInitializationActionKind.StopAtMissingAlignmentDiagnosticReturn,
                    actions,
                    out bool diagnosticReturned))
            {
                RequireUnreached(
                    captures.ReloadedPoolPointer,
                    nameof(captures.ReloadedPoolPointer));
                RequireUnreached(
                    captures.AcquiredPointer,
                    nameof(captures.AcquiredPointer));
                return Plan(captures, QueueSentinelInitializationOutcome.AlignmentDiagnosticReturnUnavailable, actions);
            }

            if (!diagnosticReturned)
            {
                actions.Add(new QueueSentinelInitializationAction(
                    QueueSentinelInitializationActionKind.StopAtAlignmentDiagnosticNonreturn,
                    AlignmentDiagnosticCallRawFileOffset,
                    DiagnosticId: AlignmentDiagnosticId,
                    HelperReturned: false));
                RequireUnreached(
                    captures.ReloadedPoolPointer,
                    nameof(captures.ReloadedPoolPointer));
                RequireUnreached(
                    captures.AcquiredPointer,
                    nameof(captures.AcquiredPointer));
                return Plan(captures, QueueSentinelInitializationOutcome.AlignmentDiagnosticDidNotReturn, actions);
            }

            actions.Add(new QueueSentinelInitializationAction(
                QueueSentinelInitializationActionKind.ContinueAfterAlignmentDiagnostic,
                AlignmentDiagnosticContinuationRawFileOffset,
                DiagnosticId: AlignmentDiagnosticId,
                HelperReturned: true));
            actions.Add(new QueueSentinelInitializationAction(
                QueueSentinelInitializationActionKind.CleanupAlignmentDiagnosticArguments,
                AlignmentDiagnosticContinuationRawFileOffset,
                Value: 0x10));
        }

        if (!TryCapture(
                captures.ReloadedPoolPointer,
                ReloadedPoolPointerCaptureRawFileOffset,
                QueueSentinelInitializationActionKind.ReloadPoolPointer,
                QueueSentinelInitializationActionKind.StopAtMissingReloadedPoolPointer,
                actions,
                out uint reloadedPool))
        {
            RequireUnreached(
                captures.AcquiredPointer,
                nameof(captures.AcquiredPointer));
            return Plan(captures, QueueSentinelInitializationOutcome.ReloadedPoolPointerUnavailable, actions);
        }

        actions.Add(new QueueSentinelInitializationAction(
            QueueSentinelInitializationActionKind.CallOpaquePoolAcquisition,
            AcquisitionCallRawFileOffset,
            CapturedValue: reloadedPool,
            CallTargetRawEa: AcquisitionHelperCallTargetRawEa));
        if (!TryCapture(
                captures.AcquiredPointer,
                AcquiredPointerTestRawFileOffset,
                QueueSentinelInitializationActionKind.CaptureAcquiredPointer,
                QueueSentinelInitializationActionKind.StopAtMissingAcquisitionReturn,
                actions,
                out uint acquiredPointer))
        {
            return Plan(captures, QueueSentinelInitializationOutcome.AcquisitionReturnUnavailable, actions);
        }

        actions.Add(new QueueSentinelInitializationAction(
            QueueSentinelInitializationActionKind.TestAcquiredPointer,
            AcquiredPointerTestRawFileOffset,
            CapturedValue: acquiredPointer));
        if (acquiredPointer != 0)
        {
            actions.Add(new QueueSentinelInitializationAction(
                QueueSentinelInitializationActionKind.ZeroAcquiredQueueQword,
                AcquiredQueueQwordZeroRawFileOffset,
                CapturedValue: acquiredPointer,
                Value: 0,
                SecondaryValue: AcquiredQueueQwordSize));
        }
        else
        {
            actions.Add(new QueueSentinelInitializationAction(
                QueueSentinelInitializationActionKind.SkipAcquiredQueueQwordStore,
                AcquiredPointerNullBranchRawFileOffset,
                CapturedValue: acquiredPointer,
                BranchTargetRawFileOffset: 0x12CE264));
            actions.Add(new QueueSentinelInitializationAction(
                QueueSentinelInitializationActionKind.SetNullAcquiredPointer,
                0x12CE264,
                Value: 0));
        }

        actions.Add(new QueueSentinelInitializationAction(
            QueueSentinelInitializationActionKind.PublishQueueTail,
            QueueTailPublishRawFileOffset,
            QueueTailOffset,
            CapturedValue: acquiredPointer));
        actions.Add(new QueueSentinelInitializationAction(
            QueueSentinelInitializationActionKind.PublishQueueHead,
            QueueHeadPublishRawFileOffset,
            QueueHeadOffset,
            CapturedValue: acquiredPointer));
        actions.Add(new QueueSentinelInitializationAction(
            QueueSentinelInitializationActionKind.CaptureReturnReceiver,
            ReturnReceiverCaptureRawFileOffset,
            CapturedValue: receiver));
        actions.Add(new QueueSentinelInitializationAction(
            QueueSentinelInitializationActionKind.ClearReceiverWord0C,
            ReceiverWord0CClearRawFileOffset,
            ReceiverWord0COffset,
            Value: 0));
        actions.Add(new QueueSentinelInitializationAction(
            QueueSentinelInitializationActionKind.ClearReceiverWord4,
            ReceiverWord4ClearRawFileOffset,
            ReceiverWord4Offset,
            Value: 0));
        actions.Add(new QueueSentinelInitializationAction(
            QueueSentinelInitializationActionKind.ReturnReceiver,
            ReturnRawFileOffset,
            CapturedValue: receiver));

        return Plan(
            captures,
            QueueSentinelInitializationOutcome.NormalReturnReached,
            actions,
            receiver);
    }

    private static bool TryCapture<T>(
        QueueSentinelInitializationCapture<T> capture,
        uint instructionRawFileOffset,
        QueueSentinelInitializationActionKind captureActionKind,
        QueueSentinelInitializationActionKind missingActionKind,
        List<QueueSentinelInitializationAction> actions,
        out T value)
        where T : struct
    {
        if (capture.Kind == QueueSentinelInitializationCaptureKind.Unreached)
        {
            throw new ArgumentException(
                $"Capture at raw file offset 0x{instructionRawFileOffset:X} was supplied as unreached.");
        }

        if (capture.Kind == QueueSentinelInitializationCaptureKind.Missing)
        {
            if (capture.Value.HasValue)
            {
                throw new ArgumentException("A missing capture cannot have a value.");
            }

            actions.Add(new QueueSentinelInitializationAction(
                missingActionKind,
                instructionRawFileOffset));
            value = default;
            return false;
        }

        if (capture.Kind != QueueSentinelInitializationCaptureKind.Captured
            || !capture.Value.HasValue)
        {
            throw new ArgumentException("A captured value is required at the reached instruction.");
        }

        value = capture.Value.Value;
        var action = new QueueSentinelInitializationAction(
            captureActionKind,
            instructionRawFileOffset);
        if (value is bool returned)
        {
            action = action with { HelperReturned = returned };
        }
        else
        {
            action = action with { CapturedValue = Convert.ToUInt32(value) };
        }
        actions.Add(action);
        return true;
    }

    private static void RequireUnreached<T>(
        QueueSentinelInitializationCapture<T> capture,
        string captureName)
        where T : struct
    {
        if (capture.Kind != QueueSentinelInitializationCaptureKind.Unreached
            || capture.Value.HasValue)
        {
            throw new ArgumentException(
                $"{captureName} must be explicitly marked unreached on this path.");
        }
    }

    private static QueueSentinelInitializationPlan Plan(
        QueueSentinelInitializationCaptures captures,
        QueueSentinelInitializationOutcome outcome,
        List<QueueSentinelInitializationAction> actions,
        uint? returnReceiverPointer = null) =>
        new(captures, outcome, Array.AsReadOnly(actions.ToArray()), returnReceiverPointer);
}
