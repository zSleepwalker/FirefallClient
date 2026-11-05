using Firefall.Reconstruction.Startup;

internal static class QueueSentinelInitializationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Queue initializer anchors use the pinned sample, bounded raw slice, and raw-to-VA mapping",
            QueueSentinelInitializationProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && QueueSentinelInitializationProjection.EntryRawFileOffset == 0x12CE210
            && QueueSentinelInitializationProjection.EndRawFileOffsetExclusive == 0x12CE280
            && QueueSentinelInitializationProjection.EntryVirtualAddress == 0x16CEE10
            && QueueSentinelInitializationProjection.EndVirtualAddress == 0x16CEE80
            && QueueSentinelInitializationProjection.EntryRawFileOffset
                + QueueSentinelInitializationProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueSentinelInitializationProjection.EntryVirtualAddress
            && QueueSentinelInitializationProjection.EndRawFileOffsetExclusive
                + QueueSentinelInitializationProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueSentinelInitializationProjection.EndVirtualAddress
            && QueueSentinelInitializationProjection.CapturedFunctionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x70
            && QueueSentinelInitializationProjection.Instructions.All(instruction =>
                HasSlice(
                    QueueSentinelInitializationProjection.CapturedFunctionBytes,
                    QueueSentinelInitializationProjection.EntryRawFileOffset,
                    instruction.RawFileOffset,
                    instruction.Bytes))
            && QueueSentinelInitializationProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.ReceiverRelativeOffset, instruction.ReferencedVirtualAddress,
                        instruction.DiagnosticId, instruction.BranchTargetRawFileOffset,
                        instruction.CallTargetRawEa))
                .SequenceEqual(new
                    (uint RawFileOffset, string Bytes, int? ReceiverRelativeOffset,
                        uint? ReferencedVirtualAddress, uint? DiagnosticId,
                        uint? BranchTargetRawFileOffset, uint? CallTargetRawEa)[]
                {
                    (0x12CE211, "8B D9", null, null, null, null, null),
                    (0x12CE213, "C7 43 10 00 00 00 00", 0x10, null, null, null, null),
                    (0x12CE21A, "83 3D BC 03 E0 01 00", null, 0x1E003BC, null, null, null),
                    (0x12CE221, "75 0A", null, null, null, 0x12CE22D, null),
                    (0x12CE223, "B9 AC 03 E0 01", null, 0x1E003AC, null, null, null),
                    (0x12CE228, "E8 93 18 8F FF", null, null, null, null, 0xBBFAC0),
                    (0x12CE22D, "F6 C3 03", null, null, null, null, null),
                    (0x12CE230, "74 1A", null, null, null, 0x12CE24C, null),
                    (0x12CE232, "6A 00 68 CD 02 00 00", null, null, 0x2CD, null, null),
                    (0x12CE243, "FF 15 A0 5E C1 01", null, 0x1C15EA0, null, null, null),
                    (0x12CE249, "83 C4 10", null, null, null, null, null),
                    (0x12CE24C, "8B 0D BC 03 E0 01", null, 0x1E003BC, null, null, null),
                    (0x12CE252, "E8 09 FC FF FF", null, null, null, null, 0x12CDE60),
                    (0x12CE257, "85 C0", null, null, null, null, null),
                    (0x12CE259, "74 09", null, null, null, 0x12CE264, null),
                    (0x12CE25E, "66 0F D6 00", null, null, null, null, null),
                    (0x12CE262, "EB 02", null, null, null, 0x12CE266, null),
                    (0x12CE264, "33 C0", null, null, null, null, null),
                    (0x12CE266, "89 43 08", 0x08, null, null, null, null),
                    (0x12CE269, "89 03", 0x00, null, null, null, null),
                    (0x12CE26B, "8B C3", null, null, null, null, null),
                    (0x12CE26D, "C7 43 0C 00 00 00 00", 0x0C, null, null, null, null),
                    (0x12CE274, "C7 43 04 00 00 00 00", 0x04, null, null, null, null),
                    (0x12CE27C, "C3", null, null, null, null, null)
                })
            && RelativeCallTargets(0x12CE228, "E8 93 18 8F FF", 0x00BBFAC0)
            && RelativeCallTargets(0x12CE252, "E8 09 FC FF FF", 0x012CDE60));

        check("Constructor binds queue initialization at this +8 and records only embedded config constants",
            QueueSentinelInitializationProjection.ConstructorQueueReceiverOffset == 8
            && QueueSentinelInitializationProjection.ConstructorQueueInitializationCallRawFileOffset
                == 0x12C0242
            && QueueSentinelInitializationProjection.ConstructorQueueInitializationCallVirtualAddress
                == 0x16C0E42
            && QueueSentinelInitializationProjection.ConstructorAllocatorConfigReceiverOffset == 0x20
            && QueueSentinelInitializationProjection.ConstructorAllocatorConfigArgument0 == 12
            && QueueSentinelInitializationProjection.ConstructorAllocatorConfigArgument1 == 8
            && QueueSentinelInitializationProjection.ConstructorAllocatorConfigArgument2 == 1
            && QueueSentinelInitializationProjection.ConstructorBindingInstructions.All(instruction =>
                HasSlice(
                    QueueSentinelInitializationProjection.CapturedFunctionBytes,
                    QueueSentinelInitializationProjection.EntryRawFileOffset,
                    instruction.RawFileOffset,
                    instruction.Bytes)
                || HasSlice(
                    "8D 4E 08 C7 06 54 0C 80 01 E8 C9 DF 00 00 "
                        + "6A 01 6A 08 8D 4E 20 C6 45 FC 01 6A 0C E8 E7 D8 00 00",
                    QueueSentinelInitializationProjection.ConstructorQueueReceiverSetupRawFileOffset,
                    instruction.RawFileOffset,
                    instruction.Bytes))
            && QueueSentinelInitializationProjection.ConstructorBindingInstructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes,
                        instruction.ReceiverRelativeOffset, instruction.ReferencedVirtualAddress,
                        instruction.DiagnosticId, instruction.BranchTargetRawFileOffset,
                        instruction.CallTargetRawEa))
                .SequenceEqual(new
                    (uint RawFileOffset, string Bytes, int? ReceiverRelativeOffset,
                        uint? ReferencedVirtualAddress, uint? DiagnosticId,
                        uint? BranchTargetRawFileOffset, uint? CallTargetRawEa)[]
                {
                    (0x12C0239, "8D 4E 08", 0x08, null, null, null, null),
                    (0x12C0242, "E8 C9 DF 00 00", null, null, null, null, 0x12CE210),
                    (0x12C0247, "6A 01", null, null, null, null, null),
                    (0x12C0249, "6A 08", null, null, null, null, null),
                    (0x12C024B, "8D 4E 20", 0x20, null, null, null, null),
                    (0x12C0252, "6A 0C", null, null, null, null, null),
                    (0x12C0254, "E8 E7 D8 00 00", null, null, null, null, 0x12CDB40)
                })
            && RelativeCallTargets(0x12C0242, "E8 C9 DF 00 00", 0x12CE210)
            && RelativeCallTargets(0x12C0254, "E8 E7 D8 00 00", 0x12CDB40));

        QueueSentinelInitializationPlan nonnullAcquisition =
            QueueSentinelInitializationProjection.Project(Captures(
                initialPool: QueueSentinelInitializationCapture<uint>.Captured(0x2000),
                reloadedPool: QueueSentinelInitializationCapture<uint>.Captured(0x3000),
                acquiredPointer: QueueSentinelInitializationCapture<uint>.Captured(0x4000)));
        check("A nonzero initial pool skips initialization but is independently reloaded before acquisition",
            nonnullAcquisition.Outcome == QueueSentinelInitializationOutcome.NormalReturnReached
            && nonnullAcquisition.ReturnReceiverPointer == 0x1000
            && nonnullAcquisition.Actions.Select(action => action.Kind).SequenceEqual(
            [
                QueueSentinelInitializationActionKind.CaptureReceiverPointer,
                QueueSentinelInitializationActionKind.ClearReceiverWord10,
                QueueSentinelInitializationActionKind.CaptureInitialPoolPointer,
                QueueSentinelInitializationActionKind.SkipPoolInitializer,
                QueueSentinelInitializationActionKind.TestReceiverAlignment,
                QueueSentinelInitializationActionKind.SkipAlignmentDiagnostic,
                QueueSentinelInitializationActionKind.ReloadPoolPointer,
                QueueSentinelInitializationActionKind.CallOpaquePoolAcquisition,
                QueueSentinelInitializationActionKind.CaptureAcquiredPointer,
                QueueSentinelInitializationActionKind.TestAcquiredPointer,
                QueueSentinelInitializationActionKind.ZeroAcquiredQueueQword,
                QueueSentinelInitializationActionKind.PublishQueueTail,
                QueueSentinelInitializationActionKind.PublishQueueHead,
                QueueSentinelInitializationActionKind.CaptureReturnReceiver,
                QueueSentinelInitializationActionKind.ClearReceiverWord0C,
                QueueSentinelInitializationActionKind.ClearReceiverWord4,
                QueueSentinelInitializationActionKind.ReturnReceiver
            ])
            && nonnullAcquisition.Actions[1].ReceiverRelativeOffset == 0x10
            && nonnullAcquisition.Actions[1].InstructionRawFileOffset
                < nonnullAcquisition.Actions[2].InstructionRawFileOffset
            && nonnullAcquisition.Actions
                .Single(action => action.Kind
                    == QueueSentinelInitializationActionKind.CaptureInitialPoolPointer)
                .CapturedValue == 0x2000
            && nonnullAcquisition.Actions
                .Single(action => action.Kind
                    == QueueSentinelInitializationActionKind.ReloadPoolPointer)
                .CapturedValue == 0x3000
            && nonnullAcquisition.Actions
                .Single(action => action.Kind
                    == QueueSentinelInitializationActionKind.CallOpaquePoolAcquisition)
                .CallTargetRawEa == 0x12CDE60
            && nonnullAcquisition.Actions
                .Single(action => action.Kind
                    == QueueSentinelInitializationActionKind.ZeroAcquiredQueueQword)
                is { CapturedValue: 0x4000, Value: 0, SecondaryValue: 8,
                    InstructionRawFileOffset: 0x12CE25E }
            && nonnullAcquisition.Actions
                .Where(action => action.Kind is
                    QueueSentinelInitializationActionKind.PublishQueueTail
                    or QueueSentinelInitializationActionKind.PublishQueueHead)
                .Select(action => (action.ReceiverRelativeOffset, action.CapturedValue))
                .SequenceEqual(new (int? Offset, uint? Pointer)[]
                {
                    (0x08, 0x4000), (0x00, 0x4000)
                })
            && nonnullAcquisition.Actions
                .Where(action => action.Kind is
                    QueueSentinelInitializationActionKind.ClearReceiverWord10
                    or QueueSentinelInitializationActionKind.ClearReceiverWord0C
                    or QueueSentinelInitializationActionKind.ClearReceiverWord4)
                .Select(action => (action.ReceiverRelativeOffset, action.InstructionRawFileOffset))
                .SequenceEqual(new (int? Offset, uint RawFileOffset)[]
                {
                    (0x10, 0x12CE213), (0x0C, 0x12CE26D), (0x04, 0x12CE274)
                }));

        QueueSentinelInitializationPlan nullAcquisition =
            QueueSentinelInitializationProjection.Project(Captures(acquiredPointer:
                QueueSentinelInitializationCapture<uint>.Captured(0)));
        check("A null acquisition skips only the acquired qword clear and still publishes both zero pointers",
            nullAcquisition.Outcome == QueueSentinelInitializationOutcome.NormalReturnReached
            && nullAcquisition.Actions.Any(action =>
                action.Kind == QueueSentinelInitializationActionKind.SkipAcquiredQueueQwordStore
                && action.BranchTargetRawFileOffset == 0x12CE264)
            && !nullAcquisition.Actions.Any(action =>
                action.Kind == QueueSentinelInitializationActionKind.ZeroAcquiredQueueQword)
            && nullAcquisition.Actions.Any(action =>
                action.Kind == QueueSentinelInitializationActionKind.SetNullAcquiredPointer
                && action.InstructionRawFileOffset == 0x12CE264)
            && nullAcquisition.Actions
                .Where(action => action.Kind is
                    QueueSentinelInitializationActionKind.PublishQueueTail
                    or QueueSentinelInitializationActionKind.PublishQueueHead)
                .All(action => action.CapturedValue == 0)
            && nullAcquisition.Actions
                .Where(action => action.Kind is
                    QueueSentinelInitializationActionKind.PublishQueueTail
                    or QueueSentinelInitializationActionKind.PublishQueueHead
                    or QueueSentinelInitializationActionKind.CaptureReturnReceiver
                    or QueueSentinelInitializationActionKind.ClearReceiverWord0C
                    or QueueSentinelInitializationActionKind.ClearReceiverWord4
                    or QueueSentinelInitializationActionKind.ReturnReceiver)
                .Select(action => action.InstructionRawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x12CE266, 0x12CE269, 0x12CE26B, 0x12CE26D, 0x12CE274, 0x12CE27C
                }));

        QueueSentinelInitializationPlan zeroPoolMisalignedReceiver =
            QueueSentinelInitializationProjection.Project(Captures(
                receiver: QueueSentinelInitializationCapture<uint>.Captured(0x1001),
                initialPool: QueueSentinelInitializationCapture<uint>.Captured(0),
                initializerReturned: QueueSentinelInitializationCapture<bool>.Captured(true),
                diagnosticReturned: QueueSentinelInitializationCapture<bool>.Captured(true),
                reloadedPool: QueueSentinelInitializationCapture<uint>.Captured(0x5000)));
        check("Zero initial pool calls the opaque initializer and a returning misalignment report reaches the reload",
            zeroPoolMisalignedReceiver.Outcome == QueueSentinelInitializationOutcome.NormalReturnReached
            && zeroPoolMisalignedReceiver.Actions
                .Where(action => action.Kind is
                    QueueSentinelInitializationActionKind.CallOpaquePoolInitializer
                    or QueueSentinelInitializationActionKind.CapturePoolInitializerReturn
                    or QueueSentinelInitializationActionKind.ContinueAfterPoolInitializer
                    or QueueSentinelInitializationActionKind.ReportAlignmentDiagnostic
                    or QueueSentinelInitializationActionKind.CaptureAlignmentDiagnosticReturn
                    or QueueSentinelInitializationActionKind.ContinueAfterAlignmentDiagnostic
                    or QueueSentinelInitializationActionKind.CleanupAlignmentDiagnosticArguments
                    or QueueSentinelInitializationActionKind.ReloadPoolPointer)
                .Select(action => action.Kind)
                .SequenceEqual(new[]
                {
                    QueueSentinelInitializationActionKind.CallOpaquePoolInitializer,
                    QueueSentinelInitializationActionKind.CapturePoolInitializerReturn,
                    QueueSentinelInitializationActionKind.ContinueAfterPoolInitializer,
                    QueueSentinelInitializationActionKind.ReportAlignmentDiagnostic,
                    QueueSentinelInitializationActionKind.CaptureAlignmentDiagnosticReturn,
                    QueueSentinelInitializationActionKind.ContinueAfterAlignmentDiagnostic,
                    QueueSentinelInitializationActionKind.CleanupAlignmentDiagnosticArguments,
                    QueueSentinelInitializationActionKind.ReloadPoolPointer
                })
            && zeroPoolMisalignedReceiver.Actions
                .Single(action => action.Kind
                    == QueueSentinelInitializationActionKind.CallOpaquePoolInitializer)
                .ReferencedVirtualAddress == 0x1E003AC
            && zeroPoolMisalignedReceiver.Actions
                .Single(action => action.Kind
                    == QueueSentinelInitializationActionKind.CallOpaquePoolInitializer)
                .CallTargetRawEa == 0xBBFAC0
            && zeroPoolMisalignedReceiver.Actions
                .Single(action => action.Kind
                    == QueueSentinelInitializationActionKind.ContinueAfterPoolInitializer)
                .HelperReturned == true
            && zeroPoolMisalignedReceiver.Actions
                .Single(action => action.Kind
                    == QueueSentinelInitializationActionKind.ReportAlignmentDiagnostic)
                .DiagnosticId == 0x2CD
            && zeroPoolMisalignedReceiver.Actions
                .Single(action => action.Kind
                    == QueueSentinelInitializationActionKind.ContinueAfterAlignmentDiagnostic)
                .HelperReturned == true
            && zeroPoolMisalignedReceiver.Actions
                .Single(action => action.Kind
                    == QueueSentinelInitializationActionKind.ReloadPoolPointer)
                .CapturedValue == 0x5000);

        QueueSentinelInitializationPlan missingReceiver =
            QueueSentinelInitializationProjection.Project(Captures(
                receiver: QueueSentinelInitializationCapture<uint>.Missing(),
                initialPool: QueueSentinelInitializationCapture<uint>.Unreached(),
                initializerReturned: QueueSentinelInitializationCapture<bool>.Unreached(),
                diagnosticReturned: QueueSentinelInitializationCapture<bool>.Unreached(),
                reloadedPool: QueueSentinelInitializationCapture<uint>.Unreached(),
                acquiredPointer: QueueSentinelInitializationCapture<uint>.Unreached()));
        QueueSentinelInitializationPlan missingInitialPool =
            QueueSentinelInitializationProjection.Project(Captures(
                initialPool: QueueSentinelInitializationCapture<uint>.Missing(),
                initializerReturned: QueueSentinelInitializationCapture<bool>.Unreached(),
                diagnosticReturned: QueueSentinelInitializationCapture<bool>.Unreached(),
                reloadedPool: QueueSentinelInitializationCapture<uint>.Unreached(),
                acquiredPointer: QueueSentinelInitializationCapture<uint>.Unreached()));
        QueueSentinelInitializationPlan missingPoolInitializerReturn =
            QueueSentinelInitializationProjection.Project(Captures(
                initialPool: QueueSentinelInitializationCapture<uint>.Captured(0),
                initializerReturned: QueueSentinelInitializationCapture<bool>.Missing(),
                reloadedPool: QueueSentinelInitializationCapture<uint>.Unreached(),
                acquiredPointer: QueueSentinelInitializationCapture<uint>.Unreached()));
        QueueSentinelInitializationPlan missingDiagnosticReturn =
            QueueSentinelInitializationProjection.Project(Captures(
                receiver: QueueSentinelInitializationCapture<uint>.Captured(0x1001),
                diagnosticReturned: QueueSentinelInitializationCapture<bool>.Missing(),
                reloadedPool: QueueSentinelInitializationCapture<uint>.Unreached(),
                acquiredPointer: QueueSentinelInitializationCapture<uint>.Unreached()));
        QueueSentinelInitializationPlan missingReloadedPool =
            QueueSentinelInitializationProjection.Project(Captures(
                reloadedPool: QueueSentinelInitializationCapture<uint>.Missing(),
                acquiredPointer: QueueSentinelInitializationCapture<uint>.Unreached()));
        QueueSentinelInitializationPlan missingAcquiredPointer =
            QueueSentinelInitializationProjection.Project(Captures(
                acquiredPointer: QueueSentinelInitializationCapture<uint>.Missing()));
        check("Missing captures stop at their reached instruction boundaries",
            missingReceiver.Outcome == QueueSentinelInitializationOutcome.ReceiverPointerUnavailable
            && missingReceiver.Actions[^1].Kind
                == QueueSentinelInitializationActionKind.StopAtMissingReceiverPointer
            && missingInitialPool.Outcome
                == QueueSentinelInitializationOutcome.InitialPoolPointerUnavailable
            && missingInitialPool.Actions[^1].Kind
                == QueueSentinelInitializationActionKind.StopAtMissingInitialPoolPointer
            && missingPoolInitializerReturn.Outcome
                == QueueSentinelInitializationOutcome.PoolInitializerReturnUnavailable
            && missingPoolInitializerReturn.Actions[^1].Kind
                == QueueSentinelInitializationActionKind.StopAtMissingPoolInitializerReturn
            && missingPoolInitializerReturn.Actions[^1].InstructionRawFileOffset == 0x12CE22D
            && missingDiagnosticReturn.Outcome
                == QueueSentinelInitializationOutcome.AlignmentDiagnosticReturnUnavailable
            && missingDiagnosticReturn.Actions[^1].Kind
                == QueueSentinelInitializationActionKind.StopAtMissingAlignmentDiagnosticReturn
            && missingDiagnosticReturn.Actions[^1].InstructionRawFileOffset == 0x12CE249
            && missingReloadedPool.Outcome
                == QueueSentinelInitializationOutcome.ReloadedPoolPointerUnavailable
            && missingReloadedPool.Actions[^1].Kind
                == QueueSentinelInitializationActionKind.StopAtMissingReloadedPoolPointer
            && missingAcquiredPointer.Outcome
                == QueueSentinelInitializationOutcome.AcquisitionReturnUnavailable
            && missingAcquiredPointer.Actions[^1].Kind
                == QueueSentinelInitializationActionKind.StopAtMissingAcquisitionReturn
            && missingAcquiredPointer.Actions[^1].InstructionRawFileOffset == 0x12CE257);

        QueueSentinelInitializationPlan initializerDidNotReturn =
            QueueSentinelInitializationProjection.Project(Captures(
                initialPool: QueueSentinelInitializationCapture<uint>.Captured(0),
                initializerReturned: QueueSentinelInitializationCapture<bool>.Captured(false),
                reloadedPool: QueueSentinelInitializationCapture<uint>.Unreached(),
                acquiredPointer: QueueSentinelInitializationCapture<uint>.Unreached()));
        QueueSentinelInitializationPlan diagnosticDidNotReturn =
            QueueSentinelInitializationProjection.Project(Captures(
                receiver: QueueSentinelInitializationCapture<uint>.Captured(0x1001),
                diagnosticReturned: QueueSentinelInitializationCapture<bool>.Captured(false),
                reloadedPool: QueueSentinelInitializationCapture<uint>.Unreached(),
                acquiredPointer: QueueSentinelInitializationCapture<uint>.Unreached()));
        check("Nonreturning opaque calls stop before every later capture",
            initializerDidNotReturn.Outcome
                == QueueSentinelInitializationOutcome.PoolInitializerDidNotReturn
            && initializerDidNotReturn.Actions[^1].Kind
                == QueueSentinelInitializationActionKind.StopAtPoolInitializerNonreturn
            && initializerDidNotReturn.Actions.All(action =>
                action.Kind != QueueSentinelInitializationActionKind.TestReceiverAlignment
                && action.Kind != QueueSentinelInitializationActionKind.ReloadPoolPointer)
            && diagnosticDidNotReturn.Outcome
                == QueueSentinelInitializationOutcome.AlignmentDiagnosticDidNotReturn
            && diagnosticDidNotReturn.Actions[^1].Kind
                == QueueSentinelInitializationActionKind.StopAtAlignmentDiagnosticNonreturn
            && diagnosticDidNotReturn.Actions.All(action =>
                action.Kind != QueueSentinelInitializationActionKind.ReloadPoolPointer
                && action.Kind != QueueSentinelInitializationActionKind.CallOpaquePoolAcquisition));

        bool rejectsUnreachedCapturesAtReachedSites =
            ThrowsArgumentException(() => QueueSentinelInitializationProjection.Project(Captures(
                initialPool: QueueSentinelInitializationCapture<uint>.Captured(0),
                initializerReturned: QueueSentinelInitializationCapture<bool>.Unreached())))
            && ThrowsArgumentException(() => QueueSentinelInitializationProjection.Project(Captures(
                receiver: QueueSentinelInitializationCapture<uint>.Captured(0x1001),
                diagnosticReturned: QueueSentinelInitializationCapture<bool>.Unreached())))
            && ThrowsArgumentException(() => QueueSentinelInitializationProjection.Project(Captures(
                reloadedPool: QueueSentinelInitializationCapture<uint>.Unreached())))
            && ThrowsArgumentException(() => QueueSentinelInitializationProjection.Project(Captures(
                acquiredPointer: QueueSentinelInitializationCapture<uint>.Unreached())));
        bool rejectsCapturesOnUnreachedPaths =
            ThrowsArgumentException(() => QueueSentinelInitializationProjection.Project(Captures(
                initialPool: QueueSentinelInitializationCapture<uint>.Captured(0x2000),
                initializerReturned: QueueSentinelInitializationCapture<bool>.Captured(true))))
            && ThrowsArgumentException(() => QueueSentinelInitializationProjection.Project(Captures(
                receiver: QueueSentinelInitializationCapture<uint>.Captured(0x1000),
                diagnosticReturned: QueueSentinelInitializationCapture<bool>.Captured(true))))
            && ThrowsArgumentException(() => QueueSentinelInitializationProjection.Project(Captures(
                initialPool: QueueSentinelInitializationCapture<uint>.Captured(0),
                initializerReturned: QueueSentinelInitializationCapture<bool>.Missing(),
                reloadedPool: QueueSentinelInitializationCapture<uint>.Captured(0x5000))));
        check("Missing, reached, and explicitly unreached captures are validated consistently",
            rejectsUnreachedCapturesAtReachedSites && rejectsCapturesOnUnreachedPaths);
    }

    private static QueueSentinelInitializationCaptures Captures(
        QueueSentinelInitializationCapture<uint>? receiver = null,
        QueueSentinelInitializationCapture<uint>? initialPool = null,
        QueueSentinelInitializationCapture<bool>? initializerReturned = null,
        QueueSentinelInitializationCapture<bool>? diagnosticReturned = null,
        QueueSentinelInitializationCapture<uint>? reloadedPool = null,
        QueueSentinelInitializationCapture<uint>? acquiredPointer = null) =>
        new(
            receiver ?? QueueSentinelInitializationCapture<uint>.Captured(0x1000),
            initialPool ?? QueueSentinelInitializationCapture<uint>.Captured(0x2000),
            initializerReturned ?? QueueSentinelInitializationCapture<bool>.Unreached(),
            diagnosticReturned ?? QueueSentinelInitializationCapture<bool>.Unreached(),
            reloadedPool ?? QueueSentinelInitializationCapture<uint>.Captured(0x3000),
            acquiredPointer ?? QueueSentinelInitializationCapture<uint>.Captured(0x4000));

    private static bool RelativeCallTargets(
        uint rawFileOffset,
        string instructionBytes,
        uint targetRawEa)
    {
        byte[] bytes = Convert.FromHexString(instructionBytes.Replace(" ", string.Empty));
        return bytes.Length == 5
            && bytes[0] == 0xE8
            && rawFileOffset + 5 + BitConverter.ToInt32(bytes, 1) == targetRawEa;
    }

    private static bool HasSlice(
        string capturedBytes,
        uint captureStart,
        uint rawFileOffset,
        string expected)
    {
        string[] body = capturedBytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] instruction = expected.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (rawFileOffset < captureStart)
        {
            return false;
        }

        int start = checked((int)(rawFileOffset - captureStart));
        return start + instruction.Length <= body.Length
            && body.Skip(start).Take(instruction.Length).SequenceEqual(instruction);
    }

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
