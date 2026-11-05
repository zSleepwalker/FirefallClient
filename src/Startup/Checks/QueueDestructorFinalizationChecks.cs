using Firefall.Reconstruction.Startup;

internal static class QueueDestructorFinalizationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Destructor-tail anchors match the pinned sample range and raw-offset-to-VA mapping",
            QueueDestructorFinalizationProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && QueueDestructorFinalizationProjection.EntryRawFileOffset == 0x12C03AF
            && QueueDestructorFinalizationProjection.EndRawFileOffsetExclusive == 0x12C03CA
            && QueueDestructorFinalizationProjection.EndRawFileOffsetExclusive
                - QueueDestructorFinalizationProjection.EntryRawFileOffset == 0x1B
            && QueueDestructorFinalizationProjection.EntryVirtualAddress == 0x16C0FAF
            && QueueDestructorFinalizationProjection.EndVirtualAddressExclusive == 0x16C0FCA
            && QueueDestructorFinalizationProjection.EntryRawFileOffset
                + QueueDestructorFinalizationProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueDestructorFinalizationProjection.EntryVirtualAddress
            && QueueDestructorFinalizationProjection.CapturedTailBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x1B
            && QueueDestructorFinalizationProjection.CapturedTailBytes
                == "8B 45 EC C7 00 CC 34 C7 01 8B 4D F4 64 89 0D 00 00 00 00 "
                    + "59 5F 5E 5B 8B E5 5D C3"
            && QueueDestructorFinalizationProjection.FirstDwordStoreValue == 0x01C734CC
            && QueueDestructorFinalizationProjection.FirstDwordStoreValueVirtualAddress
                == 0x01C734CC
            && QueueDestructorFinalizationProjection.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(new (uint RawFileOffset, string Bytes)[]
                {
                    (0x12C03AF, "8B 45 EC"),
                    (0x12C03B2, "C7 00 CC 34 C7 01"),
                    (0x12C03B8, "8B 4D F4"),
                    (0x12C03BB, "64 89 0D 00 00 00 00"),
                    (0x12C03C2, "59"),
                    (0x12C03C3, "5F"),
                    (0x12C03C4, "5E"),
                    (0x12C03C5, "5B"),
                    (0x12C03C6, "8B E5"),
                    (0x12C03C8, "5D"),
                    (0x12C03C9, "C3")
                })
            && QueueDestructorFinalizationProjection.PlainReturnRawFileOffset + 1
                == QueueDestructorFinalizationProjection.EndRawFileOffsetExclusive
            && QueueDestructorFinalizationProjection.Instructions.All(instruction =>
                !instruction.Bytes.StartsWith("E8", StringComparison.Ordinal)
                && !instruction.Bytes.StartsWith("FF 15", StringComparison.Ordinal)));

        QueueDestructorFinalizationPlan returned =
            QueueDestructorFinalizationProjection.Project(
                new QueueDestructorFinalizationCaptures(
                    FramePointer: 0x1000,
                    ReceiverPointer:
                        QueueDestructorFinalizationCapture<uint>.Captured(0xFFFFFFF8),
                    PriorExceptionChain:
                        QueueDestructorFinalizationCapture<uint>.Captured(0x55667788)));

        check("Captured stack slots, first-DWORD store projection, exception restore, and plain epilogue stay ordered",
            returned.Outcome == QueueDestructorFinalizationOutcome.PlainReturnReached
            && returned.ReceiverFrameSlotAddress == 0xFEC
            && returned.ExceptionChainFrameSlotAddress == 0xFF4
            && returned.FirstDwordStoreAddress == 0xFFFFFFF8
            && returned.FirstDwordStoreValue == 0x01C734CC
            && returned.RestoredExceptionChain == 0x55667788
            && returned.Actions.Select(action => action.Kind).SequenceEqual(
            [
                QueueDestructorFinalizationActionKind.CaptureReceiverPointerFromFrame,
                QueueDestructorFinalizationActionKind.ProjectFirstDwordStore,
                QueueDestructorFinalizationActionKind.CapturePriorExceptionChainFromFrame,
                QueueDestructorFinalizationActionKind.ProjectRestoreFsExceptionChain,
                QueueDestructorFinalizationActionKind.RestoreEcx,
                QueueDestructorFinalizationActionKind.RestoreEdi,
                QueueDestructorFinalizationActionKind.RestoreEsi,
                QueueDestructorFinalizationActionKind.RestoreEbx,
                QueueDestructorFinalizationActionKind.RestoreStackPointerFromFrame,
                QueueDestructorFinalizationActionKind.RestoreEbp,
                QueueDestructorFinalizationActionKind.PlainReturn
            ])
            && returned.Actions[0].Address == 0xFEC
            && returned.Actions[0].CapturedValue == 0xFFFFFFF8
            && returned.Actions[1].RawFileOffset == 0x12C03B2
            && returned.Actions[1].Address == 0xFFFFFFF8
            && returned.Actions[1].CapturedValue == 0x01C734CC
            && returned.Actions[2].Address == 0xFF4
            && returned.Actions[2].CapturedValue == 0x55667788
            && returned.Actions[3].RawFileOffset == 0x12C03BB
            && returned.Actions[^1].RawFileOffset == 0x12C03C9);

        QueueDestructorFinalizationPlan missingReceiver =
            QueueDestructorFinalizationProjection.Project(
                new QueueDestructorFinalizationCaptures(
                    FramePointer: 0x1000,
                    ReceiverPointer: QueueDestructorFinalizationCapture<uint>.Missing(),
                    PriorExceptionChain: QueueDestructorFinalizationCapture<uint>.Unreached()));
        QueueDestructorFinalizationPlan missingExceptionChain =
            QueueDestructorFinalizationProjection.Project(
                returned.Captures with
                {
                    PriorExceptionChain =
                        QueueDestructorFinalizationCapture<uint>.Missing()
                });
        check("Missing captures stop at their boundaries without projecting later operations",
            missingReceiver.Outcome
                == QueueDestructorFinalizationOutcome.MissingReceiverCaptureBoundary
            && missingReceiver.Actions.Length == 1
            && missingReceiver.Actions[^1].Kind
                == QueueDestructorFinalizationActionKind.CaptureReceiverPointerFromFrame
            && missingReceiver.FirstDwordStoreAddress is null
            && missingReceiver.RestoredExceptionChain is null
            && missingExceptionChain.Outcome
                == QueueDestructorFinalizationOutcome.MissingPriorExceptionChainCaptureBoundary
            && missingExceptionChain.Actions.Length == 3
            && missingExceptionChain.Actions[^1].Kind
                == QueueDestructorFinalizationActionKind.CapturePriorExceptionChainFromFrame
            && missingExceptionChain.FirstDwordStoreAddress == 0xFFFFFFF8
            && missingExceptionChain.RestoredExceptionChain is null
            && missingExceptionChain.Actions.All(action =>
                action.Kind != QueueDestructorFinalizationActionKind.ProjectRestoreFsExceptionChain
                && action.Kind != QueueDestructorFinalizationActionKind.PlainReturn));

        bool reachedReceiverCannotBeMarkedUnreached = ThrowsArgumentException(
            () => QueueDestructorFinalizationProjection.Project(
                missingReceiver.Captures with
                {
                    ReceiverPointer = QueueDestructorFinalizationCapture<uint>.Unreached()
                }));
        bool laterCaptureMustBeUnreachedAfterMissingReceiver = ThrowsArgumentException(
            () => QueueDestructorFinalizationProjection.Project(
                missingReceiver.Captures with
                {
                    PriorExceptionChain =
                        QueueDestructorFinalizationCapture<uint>.Captured(0x1234)
                }));
        bool reachedChainCannotBeMarkedUnreached = ThrowsArgumentException(
            () => QueueDestructorFinalizationProjection.Project(
                returned.Captures with
                {
                    PriorExceptionChain =
                        QueueDestructorFinalizationCapture<uint>.Unreached()
                }));
        check("Projection rejects unreached captures when their straight-line load is reached",
            reachedReceiverCannotBeMarkedUnreached
            && laterCaptureMustBeUnreachedAfterMissingReceiver
            && reachedChainCannotBeMarkedUnreached);

        QueueDestructorFinalizationPlan wrappedFrameSlots =
            QueueDestructorFinalizationProjection.Project(
                new QueueDestructorFinalizationCaptures(
                    FramePointer: 8,
                    ReceiverPointer:
                        QueueDestructorFinalizationCapture<uint>.Captured(uint.MaxValue),
                    PriorExceptionChain:
                        QueueDestructorFinalizationCapture<uint>.Captured(0)));
        check("Frame-relative slot addresses use 32-bit wrapping while the receiver store uses the captured pointer unchanged",
            wrappedFrameSlots.ReceiverFrameSlotAddress == 0xFFFFFFF4
            && wrappedFrameSlots.ExceptionChainFrameSlotAddress == 0xFFFFFFFC
            && wrappedFrameSlots.FirstDwordStoreAddress == uint.MaxValue
            && wrappedFrameSlots.Actions[1].Address == uint.MaxValue);
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
