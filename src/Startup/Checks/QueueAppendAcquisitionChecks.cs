using Firefall.Reconstruction.Startup;

internal static class QueueAppendAcquisitionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Queue-append acquisition anchors pin the sample, raw interval, VA mapping, and target",
            QueueAppendAcquisitionProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && QueueAppendAcquisitionProjection.EntryRawFileOffset == 0x12CE310
            && QueueAppendAcquisitionProjection.EntryVirtualAddress == 0x16CEF10
            && QueueAppendAcquisitionProjection.EntryRawFileOffset
                + QueueAppendAcquisitionProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueAppendAcquisitionProjection.EntryVirtualAddress
            && QueueAppendAcquisitionProjection.AcquisitionSetupEndRawFileOffsetExclusive
                == 0x12CE364
            && QueueAppendAcquisitionProjection.AcquisitionSetupEndRawFileOffsetExclusive
                + QueueAppendAcquisitionProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueAppendAcquisitionProjection.AcquisitionSetupEndVirtualAddress
            && QueueAppendAcquisitionProjection.AcquisitionSetupEndVirtualAddress == 0x16CEF64
            && QueueAppendAcquisitionProjection.AcquisitionCallRawFileOffset == 0x12CE334
            && QueueAppendAcquisitionProjection.AcquisitionFunctionRawFileOffset == 0x12CDE60
            && QueueAppendAcquisitionProjection.AcquisitionFunctionVirtualAddress == 0x16CEA60
            && QueueAppendAcquisitionProjection.AcquisitionFunctionRawFileOffset
                + QueueAppendAcquisitionProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueAppendAcquisitionProjection.AcquisitionFunctionVirtualAddress
            && QueueAppendAcquisitionProjection.GlobalPoolPointerVirtualAddress == 0x1E003BC
            && !QueueAppendAcquisitionProjection.CounterXaddOldValueUsed
            && QueueAppendAcquisitionProjection.CapturedFunctionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x54
            && QueueAppendAcquisitionProjection.CapturedFunctionBytes
                == "55 8B EC 83 E4 F8 83 EC 14 53 8B D9 BA 01 00 00 00 56 57 89 5C 24 14 "
                    + "8D 43 10 F0 0F C1 10 8B 0D BC 03 E0 01 E8 27 FB FF FF 89 44 24 0C "
                    + "85 C0 74 09 0F 57 C0 66 0F D6 00 EB 06 33 C0 89 44 24 0C "
                    + "8D 4B 08 C7 00 00 00 00 00 C7 40 04 00 00 00 00 89 4C 24 10");

        check("Disassembly anchors keep XADD before the independent global load and acquisition call",
            QueueAppendAcquisitionProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.BranchTargetRawFileOffset))
                .SequenceEqual(new (uint RawFileOffset, string Bytes, uint? BranchTargetRawFileOffset)[]
                {
                    (0x12CE32A, "F0 0F C1 10", null),
                    (0x12CE32E, "8B 0D BC 03 E0 01", null),
                    (0x12CE334, "E8 27 FB FF FF", 0x12CDE60),
                    (0x12CE339, "89 44 24 0C", null),
                    (0x12CE33F, "74 09", 0x12CE34A),
                    (0x12CE344, "66 0F D6 00", null),
                    (0x12CE34A, "33 C0", null),
                    (0x12CE350, "8D 4B 08", null),
                    (0x12CE353, "C7 00 00 00 00 00", null),
                    (0x12CE359, "C7 40 04 00 00 00 00", null),
                    (0x12CE360, "89 4C 24 10", null)
                })
            && QueueAppendAcquisitionProjection.ReceiverCounterOffset == 0x10
            && QueueAppendAcquisitionProjection.NonNullResultZeroByteCount == 8
            && QueueAppendAcquisitionProjection.ReceiverPairOffset == 8
            && QueueAppendAcquisitionProjection.FirstUnconditionalDwordZeroRawFileOffset
                < QueueAppendAcquisitionProjection.SecondUnconditionalDwordZeroRawFileOffset
            && QueueAppendAcquisitionProjection.SecondUnconditionalDwordZeroRawFileOffset
                < QueueAppendAcquisitionProjection.ReceiverPairAddressSaveRawFileOffset
            && QueueAppendAcquisitionProjection.ReceiverPairAddressSaveRawFileOffset
                < QueueAppendAcquisitionProjection.ReceiverWordCReadBoundaryRawFileOffset);

        QueueAppendAcquisitionPlan nonzero =
            QueueAppendAcquisitionProjection.Project(Captures(
                receiver: 0xFFFFFFF8,
                counter: QueueAppendAcquisitionCapture<uint>.Captured(uint.MaxValue),
                globalPool: QueueAppendAcquisitionCapture<uint>.Captured(0x12345678),
                result: QueueAppendAcquisitionCapture<uint>.Captured(0x87654321)));
        check("Captured nonzero acquisition result zeroes eight bytes then still reaches both DWORD attempts",
            nonzero.Outcome == QueueAppendAcquisitionOutcome.NonzeroResultBranchProjected
            && nonzero.CounterAddress == 8
            && nonzero.CounterOldValue == uint.MaxValue
            && nonzero.CounterNewValue == 0
            && nonzero.GlobalPoolPointer == 0x12345678
            && nonzero.AcquisitionResult == 0x87654321
            && nonzero.ReceiverPairAddress == 0
            && nonzero.Actions.Select(action => action.Kind).SequenceEqual(
            [
                QueueAppendAcquisitionActionKind.IncrementReceiverCounterAndCaptureOldValue,
                QueueAppendAcquisitionActionKind.LoadIndependentGlobalPoolPointerIntoEcx,
                QueueAppendAcquisitionActionKind.CallAcquisitionFunction,
                QueueAppendAcquisitionActionKind.StoreCapturedAcquisitionResult,
                QueueAppendAcquisitionActionKind.TestAcquisitionResult,
                QueueAppendAcquisitionActionKind.ZeroFirstEightResultBytes,
                QueueAppendAcquisitionActionKind.ComputeReceiverPairAddress,
                QueueAppendAcquisitionActionKind.AttemptUnconditionalFirstDwordZero,
                QueueAppendAcquisitionActionKind.AttemptUnconditionalSecondDwordZero,
                QueueAppendAcquisitionActionKind.SaveReceiverPairAddress,
                QueueAppendAcquisitionActionKind.StopBeforeReceiverWordCRead
            ])
            && nonzero.Actions[0].Address == 8
            && nonzero.Actions[0].Value == uint.MaxValue
            && nonzero.Actions[0].RelatedValue == 0
            && nonzero.Actions.Single(action =>
                action.Kind == QueueAppendAcquisitionActionKind.ZeroFirstEightResultBytes)
                is { Address: 0x87654321, ByteCount: 8 }
            && nonzero.Actions.Single(action =>
                action.Kind == QueueAppendAcquisitionActionKind.AttemptUnconditionalFirstDwordZero)
                is { Address: 0x87654321, ByteCount: 4 }
            && nonzero.Actions.Single(action =>
                action.Kind == QueueAppendAcquisitionActionKind.AttemptUnconditionalSecondDwordZero)
                is { Address: 0x87654325, ByteCount: 4 }
            && nonzero.Actions.Single(action =>
                action.Kind == QueueAppendAcquisitionActionKind.CallAcquisitionFunction)
                is { Value: 0x12345678, TargetRawFileOffset: 0x12CDE60, TargetVirtualAddress: 0x16CEA60 }
            && nonzero.Actions[^1].InstructionRawFileOffset == 0x12CE364);

        QueueAppendAcquisitionPlan nullResult =
            QueueAppendAcquisitionProjection.Project(Captures(
                receiver: 0x1000,
                counter: QueueAppendAcquisitionCapture<uint>.Captured(41),
                globalPool: QueueAppendAcquisitionCapture<uint>.Captured(0),
                result: QueueAppendAcquisitionCapture<uint>.Captured(0)));
        check("Null acquisition result is not a clean failure: EAX is zeroed and both null-address DWORD attempts remain",
            nullResult.Outcome == QueueAppendAcquisitionOutcome.NullResultBranchProjected
            && nullResult.AcquisitionResult == 0
            && nullResult.Actions.Select(action => action.Kind).SequenceEqual(
            [
                QueueAppendAcquisitionActionKind.IncrementReceiverCounterAndCaptureOldValue,
                QueueAppendAcquisitionActionKind.LoadIndependentGlobalPoolPointerIntoEcx,
                QueueAppendAcquisitionActionKind.CallAcquisitionFunction,
                QueueAppendAcquisitionActionKind.StoreCapturedAcquisitionResult,
                QueueAppendAcquisitionActionKind.TestAcquisitionResult,
                QueueAppendAcquisitionActionKind.SetEaxToZero,
                QueueAppendAcquisitionActionKind.StoreNullAcquisitionResult,
                QueueAppendAcquisitionActionKind.ComputeReceiverPairAddress,
                QueueAppendAcquisitionActionKind.AttemptUnconditionalFirstDwordZero,
                QueueAppendAcquisitionActionKind.AttemptUnconditionalSecondDwordZero,
                QueueAppendAcquisitionActionKind.SaveReceiverPairAddress,
                QueueAppendAcquisitionActionKind.StopBeforeReceiverWordCRead
            ])
            && !nullResult.Actions.Any(action =>
                action.Kind == QueueAppendAcquisitionActionKind.ZeroFirstEightResultBytes)
            && nullResult.Actions.Single(action =>
                action.Kind == QueueAppendAcquisitionActionKind.AttemptUnconditionalFirstDwordZero)
                is { Address: 0, ByteCount: 4 }
            && nullResult.Actions.Single(action =>
                action.Kind == QueueAppendAcquisitionActionKind.AttemptUnconditionalSecondDwordZero)
                is { Address: 4, ByteCount: 4 }
            && nullResult.Actions.Single(action =>
                action.Kind == QueueAppendAcquisitionActionKind.SaveReceiverPairAddress)
                .Address == 0x1008);

        QueueAppendAcquisitionPlan missingGlobal =
            QueueAppendAcquisitionProjection.Project(Captures(
                counter: QueueAppendAcquisitionCapture<uint>.Captured(5),
                globalPool: QueueAppendAcquisitionCapture<uint>.Missing(),
                result: QueueAppendAcquisitionCapture<uint>.Unreached()));
        QueueAppendAcquisitionPlan missingResult =
            QueueAppendAcquisitionProjection.Project(Captures(
                counter: QueueAppendAcquisitionCapture<uint>.Captured(5),
                globalPool: QueueAppendAcquisitionCapture<uint>.Captured(0xABC),
                result: QueueAppendAcquisitionCapture<uint>.Missing()));
        QueueAppendAcquisitionPlan missingCounter =
            QueueAppendAcquisitionProjection.Project(Captures(
                counter: QueueAppendAcquisitionCapture<uint>.Missing(),
                globalPool: QueueAppendAcquisitionCapture<uint>.Unreached(),
                result: QueueAppendAcquisitionCapture<uint>.Unreached()));
        check("Missing counter capture permits the later global-pool and result captures to be unreached",
            missingCounter.Outcome == QueueAppendAcquisitionOutcome.MissingCounterCapture
            && missingCounter.Actions.Single().Kind
                == QueueAppendAcquisitionActionKind.StopAtMissingCounterCapture
            && missingCounter.Captures.GlobalPoolPointer.Kind
                == QueueAppendAcquisitionCaptureKind.Unreached
            && missingCounter.Captures.AcquisitionResult.Kind
                == QueueAppendAcquisitionCaptureKind.Unreached);
        check("Missing global-pool capture permits an unreached acquisition result after a captured counter",
            missingGlobal.Outcome == QueueAppendAcquisitionOutcome.MissingGlobalPoolCapture
            && missingGlobal.Actions[^1].Kind
                == QueueAppendAcquisitionActionKind.StopAtMissingGlobalPoolCapture
            && missingGlobal.Captures.CounterOldValue.Kind
                == QueueAppendAcquisitionCaptureKind.Captured
            && missingGlobal.Captures.AcquisitionResult.Kind
                == QueueAppendAcquisitionCaptureKind.Unreached);
        check("Missing captures stop at their first missing instruction boundary and preserve earlier actions",
            missingCounter.Outcome == QueueAppendAcquisitionOutcome.MissingCounterCapture
            && missingCounter.Actions.Count == 1
            && missingCounter.Actions[0].Kind
                == QueueAppendAcquisitionActionKind.StopAtMissingCounterCapture
            && missingCounter.Actions[^1].InstructionRawFileOffset == 0x12CE32A
            && missingGlobal.Outcome == QueueAppendAcquisitionOutcome.MissingGlobalPoolCapture
            && missingGlobal.Actions.Select(action => action.Kind).SequenceEqual(
            [
                QueueAppendAcquisitionActionKind.IncrementReceiverCounterAndCaptureOldValue,
                QueueAppendAcquisitionActionKind.StopAtMissingGlobalPoolCapture
            ])
            && missingGlobal.Actions[^1].InstructionRawFileOffset == 0x12CE32E
            && missingResult.Outcome == QueueAppendAcquisitionOutcome.MissingAcquisitionResult
            && missingResult.Actions.Select(action => action.Kind).TakeLast(2).SequenceEqual(
            [
                QueueAppendAcquisitionActionKind.CallAcquisitionFunction,
                QueueAppendAcquisitionActionKind.StopAtMissingAcquisitionResult
            ])
            && missingResult.Actions[^1].InstructionRawFileOffset == 0x12CE339
            && missingResult.GlobalPoolPointer == 0xABC
            && missingResult.AcquisitionResult is null);

        bool missingCounterRejectsReachedGlobal = ThrowsArgumentException(
            () => QueueAppendAcquisitionProjection.Project(Captures(
                counter: QueueAppendAcquisitionCapture<uint>.Missing(),
                globalPool: QueueAppendAcquisitionCapture<uint>.Captured(1),
                result: QueueAppendAcquisitionCapture<uint>.Unreached())));
        bool missingGlobalRejectsReachedResult = ThrowsArgumentException(
            () => QueueAppendAcquisitionProjection.Project(Captures(
                counter: QueueAppendAcquisitionCapture<uint>.Captured(1),
                globalPool: QueueAppendAcquisitionCapture<uint>.Missing(),
                result: QueueAppendAcquisitionCapture<uint>.Captured(0))));
        bool reachedCallRejectsUnreachedResult = ThrowsArgumentException(
            () => QueueAppendAcquisitionProjection.Project(Captures(
                counter: QueueAppendAcquisitionCapture<uint>.Captured(1),
                globalPool: QueueAppendAcquisitionCapture<uint>.Captured(2),
                result: QueueAppendAcquisitionCapture<uint>.Unreached())));
        bool enteredPathRejectsUnreachedCounter = ThrowsArgumentException(
            () => QueueAppendAcquisitionProjection.Project(Captures(
                counter: QueueAppendAcquisitionCapture<uint>.Unreached(),
                globalPool: QueueAppendAcquisitionCapture<uint>.Unreached(),
                result: QueueAppendAcquisitionCapture<uint>.Unreached())));
        QueueAppendAcquisitionPlan defaultMissingCapture =
            QueueAppendAcquisitionProjection.Project(new QueueAppendAcquisitionCaptures(
                0,
                default,
                QueueAppendAcquisitionCapture<uint>.Unreached(),
                QueueAppendAcquisitionCapture<uint>.Unreached()));
        check("Projection rejects reached-later captures after missing values and accepts default missing capture",
            missingCounterRejectsReachedGlobal
            && missingGlobalRejectsReachedResult
            && reachedCallRejectsUnreachedResult
            && enteredPathRejectsUnreachedCounter
            && defaultMissingCapture.Outcome
                == QueueAppendAcquisitionOutcome.MissingCounterCapture
            && defaultMissingCapture.Captures.CounterOldValue.Kind
                == QueueAppendAcquisitionCaptureKind.Missing
            && defaultMissingCapture.Captures.GlobalPoolPointer.Kind
                == QueueAppendAcquisitionCaptureKind.Unreached
            && defaultMissingCapture.Captures.AcquisitionResult.Kind
                == QueueAppendAcquisitionCaptureKind.Unreached);
    }

    private static QueueAppendAcquisitionCaptures Captures(
        uint receiver = 0x10203040,
        QueueAppendAcquisitionCapture<uint>? counter = null,
        QueueAppendAcquisitionCapture<uint>? globalPool = null,
        QueueAppendAcquisitionCapture<uint>? result = null) =>
        new(
            receiver,
            counter ?? QueueAppendAcquisitionCapture<uint>.Captured(9),
            globalPool ?? QueueAppendAcquisitionCapture<uint>.Captured(0x1ABCDEF0),
            result ?? QueueAppendAcquisitionCapture<uint>.Captured(0x12345678));

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
