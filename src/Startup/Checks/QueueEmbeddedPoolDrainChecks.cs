using System.Buffers.Binary;
using Firefall.Reconstruction.Startup;

internal static class QueueEmbeddedPoolDrainChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Embedded-pool drain anchors pin the sample, bounded raw slice, VA mapping, and helper targets",
            QueueEmbeddedPoolDrainProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && QueueEmbeddedPoolDrainProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && QueueEmbeddedPoolDrainProjection.DestructorEntryRawFileOffset == 0x12C02F0
            && QueueEmbeddedPoolDrainProjection.DrainStartRawFileOffset == 0x12C0382
            && QueueEmbeddedPoolDrainProjection.DrainEndRawFileOffsetExclusive == 0x12C03AF
            && QueueEmbeddedPoolDrainProjection.DrainEndRawFileOffsetExclusive
                - QueueEmbeddedPoolDrainProjection.DrainStartRawFileOffset == 0x2D
            && QueueEmbeddedPoolDrainProjection.CapturedDrainBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x2D
            && QueueEmbeddedPoolDrainProjection.DrainStartVirtualAddress == 0x16C0F82
            && QueueEmbeddedPoolDrainProjection.DrainEndVirtualAddressExclusive == 0x16C0FAF
            && QueueEmbeddedPoolDrainProjection.PopTargetRawFileOffset == 0x12CDAE0
            && QueueEmbeddedPoolDrainProjection.PopTargetVirtualAddress == 0x16CE6E0
            && QueueEmbeddedPoolDrainProjection.ReleaseTargetRawFileOffset == 0x12C8D60
            && QueueEmbeddedPoolDrainProjection.ReleaseTargetVirtualAddress == 0x16C9960
            && QueueEmbeddedPoolDrainProjection.CleanupTargetRawFileOffset == 0x12CE280
            && QueueEmbeddedPoolDrainProjection.CleanupTargetVirtualAddress == 0x16CEE80
            && QueueEmbeddedPoolDrainProjection.CapturedDrainBytes
                == "8B 7D F0 8D 4F 08 E8 53 D7 00 00 85 C0 74 13 8B C8 "
                    + "E8 C8 89 00 00 8D 4F 08 E8 40 D7 00 00 85 C0 75 ED "
                    + "8B CE C6 45 FC 00 E8 D1 DE 00 00");

        check("Disassembly evidence preserves pre-reset pool capture, independent queue capture, branches, and call destinations",
            QueueEmbeddedPoolDrainProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.TargetRawFileOffset,
                        instruction.TargetVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    uint? TargetRawFileOffset,
                    uint? TargetVirtualAddress)[]
                {
                    (0x12C0333, "8D 43 20", null, null),
                    (0x12C033D, "89 45 F0", null, null),
                    (0x12C0340, "E8 0B D9 00 00", 0x12CDC50, 0x16CE850),
                    (0x12C0345, "8D 73 08", null, null),
                    (0x12C0382, "8B 7D F0", null, null),
                    (0x12C0385, "8D 4F 08", null, null),
                    (0x12C0388, "E8 53 D7 00 00", 0x12CDAE0, 0x16CE6E0),
                    (0x12C038D, "85 C0", null, null),
                    (0x12C038F, "74 13", 0x12C03A4, 0x16C0FA4),
                    (0x12C0391, "8B C8", null, null),
                    (0x12C0393, "E8 C8 89 00 00", 0x12C8D60, 0x16C9960),
                    (0x12C0398, "8D 4F 08", null, null),
                    (0x12C039B, "E8 40 D7 00 00", 0x12CDAE0, 0x16CE6E0),
                    (0x12C03A0, "85 C0", null, null),
                    (0x12C03A2, "75 ED", 0x12C0391, 0x16C0F91),
                    (0x12C03A4, "8B CE", null, null),
                    (0x12C03A6, "C6 45 FC 00", null, null),
                    (0x12C03AA, "E8 D1 DE 00 00", 0x12CE280, 0x16CEE80)
                })
            && QueueEmbeddedPoolDrainProjection.Instructions.All(instruction =>
                HasDrainSlice(instruction.RawFileOffset, instruction.Bytes)
                || instruction.RawFileOffset < QueueEmbeddedPoolDrainProjection.DrainStartRawFileOffset)
            && RelativeCallTargets(0x12C0340, "E8 0B D9 00 00", 0x12CDC50)
            && RelativeCallTargets(0x12C0388, "E8 53 D7 00 00", 0x12CDAE0)
            && RelativeCallTargets(0x12C0393, "E8 C8 89 00 00", 0x12C8D60)
            && RelativeCallTargets(0x12C039B, "E8 40 D7 00 00", 0x12CDAE0)
            && RelativeCallTargets(0x12C03AA, "E8 D1 DE 00 00", 0x12CE280)
            && ShortBranchTargets(0x12C038F, "74 13", 0x12C03A4)
            && ShortBranchTargets(0x12C03A2, "75 ED", 0x12C0391)
            && QueueEmbeddedPoolDrainProjection.Instructions
                .Where(instruction =>
                    instruction.TargetRawFileOffset is not null
                    && instruction.TargetVirtualAddress is not null)
                .All(instruction =>
                    instruction.TargetVirtualAddress
                        == instruction.TargetRawFileOffset
                            + QueueEmbeddedPoolDrainProjection.CodeVirtualAddressDeltaFromRawFileOffset));

        QueueEmbeddedPoolDrainPlan initialZero =
            QueueEmbeddedPoolDrainProjection.Project(new QueueEmbeddedPoolDrainCapture(
                SavedEmbeddedPoolPointer: 0xFFFFFFFC,
                CapturedQueuePointer: 0x00001000,
                InitialPopResult: 0,
                ReleaseCallReturnedNormally: null,
                NextPopResult: null,
                CleanupCallReturnedNormally: true));
        check("An initial zero pop result selects the separately captured queue for cleanup",
            initialZero.Outcome
                == QueueEmbeddedPoolDrainOutcome.CleanupReturnedToDestructorContinuationBoundary
            && initialZero.InitialPopReceiver == 4
            && initialZero.NextPopReceiver is null
            && initialZero.CleanupReceiver == 0x1000
            && initialZero.CleanupReceiver != initialZero.InitialPopReceiver
            && initialZero.InitialPopReturnedZero == true
            && initialZero.Actions[^2]
                == new QueueEmbeddedPoolDrainAction(
                    QueueEmbeddedPoolDrainActionKind.CallCleanup,
                    0x12C03AA,
                    Pointer: 0x1000,
                    CallReturnedNormally: true,
                    TargetRawFileOffset: 0x12CE280,
                    TargetVirtualAddress: 0x16CEE80)
            && initialZero.Actions[^1]
                == new QueueEmbeddedPoolDrainAction(
                    QueueEmbeddedPoolDrainActionKind.ReachDestructorContinuationBoundary,
                    0x12C03AF)
            && initialZero.Actions.All(action =>
                action.Kind != QueueEmbeddedPoolDrainActionKind.CallRelease
                && action.Kind != QueueEmbeddedPoolDrainActionKind.CallNextPop));

        QueueEmbeddedPoolDrainPlan nextPopZero =
            QueueEmbeddedPoolDrainProjection.Project(new QueueEmbeddedPoolDrainCapture(
                SavedEmbeddedPoolPointer: 0xFFFFFFFC,
                CapturedQueuePointer: 0xFFFFFFF8,
                InitialPopResult: 0x11223344,
                ReleaseCallReturnedNormally: true,
                NextPopResult: 0,
                CleanupCallReturnedNormally: null));
        check("A nonzero first result reaches release, then a later zero pop reaches cleanup using its captured queue pointer",
            nextPopZero.Outcome
                == QueueEmbeddedPoolDrainOutcome.CleanupCallReturnUnavailableAtCallBoundary
            && nextPopZero.InitialPopReceiver == 4
            && nextPopZero.NextPopReceiver == 4
            && nextPopZero.CleanupReceiver == 0xFFFFFFF8
            && nextPopZero.NextPopReturnedZero == true
            && nextPopZero.Actions.Any(action =>
                action.Kind == QueueEmbeddedPoolDrainActionKind.CallRelease
                && action.Pointer == 0x11223344
                && action.TargetRawFileOffset == 0x12C8D60)
            && nextPopZero.Actions.Any(action =>
                action.Kind == QueueEmbeddedPoolDrainActionKind.CallNextPop
                && action.Pointer == 4
                && action.TargetRawFileOffset == 0x12CDAE0)
            && nextPopZero.Actions[^1].Kind
                == QueueEmbeddedPoolDrainActionKind.CallCleanup
            && nextPopZero.Actions.All(action =>
                action.Kind != QueueEmbeddedPoolDrainActionKind.CallRelease
                || action.Pointer == 0x11223344));

        QueueEmbeddedPoolDrainPlan nextPopZeroCleanupReturned =
            QueueEmbeddedPoolDrainProjection.Project(
                nextPopZero.Capture with { CleanupCallReturnedNormally = true });
        QueueEmbeddedPoolDrainPlan nextPopZeroCleanupDidNotReturn =
            QueueEmbeddedPoolDrainProjection.Project(
                nextPopZero.Capture with { CleanupCallReturnedNormally = false });
        check("A zero next-pop result permits either observed cleanup-call return outcome",
            nextPopZeroCleanupReturned.Outcome
                == QueueEmbeddedPoolDrainOutcome.CleanupReturnedToDestructorContinuationBoundary
            && nextPopZeroCleanupReturned.Actions[^1]
                == new QueueEmbeddedPoolDrainAction(
                    QueueEmbeddedPoolDrainActionKind.ReachDestructorContinuationBoundary,
                    0x12C03AF)
            && nextPopZeroCleanupDidNotReturn.Outcome
                == QueueEmbeddedPoolDrainOutcome.CleanupCallDidNotReturnAtCallBoundary
            && nextPopZeroCleanupDidNotReturn.Actions[^1].RawFileOffset == 0x12C03AA);

        QueueEmbeddedPoolDrainPlan nextPopNonzero =
            QueueEmbeddedPoolDrainProjection.Project(new QueueEmbeddedPoolDrainCapture(
                SavedEmbeddedPoolPointer: 0x2000,
                CapturedQueuePointer: 0x2010,
                InitialPopResult: 1,
                ReleaseCallReturnedNormally: true,
                NextPopResult: 2,
                CleanupCallReturnedNormally: null));
        check("One captured nonzero next-pop result stops at the observed repeat boundary without another release or pop",
            nextPopNonzero.Outcome
                == QueueEmbeddedPoolDrainOutcome.NextPopNonzeroReachesSingleCycleRepeatBoundary
            && nextPopNonzero.Actions[^1]
                == new QueueEmbeddedPoolDrainAction(
                    QueueEmbeddedPoolDrainActionKind.ReachSingleCycleRepeatBoundary,
                    0x12C0391)
            && nextPopNonzero.Actions.Count(action =>
                action.Kind == QueueEmbeddedPoolDrainActionKind.CallRelease) == 1
            && nextPopNonzero.Actions.Count(action =>
                action.Kind is QueueEmbeddedPoolDrainActionKind.CallInitialPop
                    or QueueEmbeddedPoolDrainActionKind.CallNextPop) == 2
            && nextPopNonzero.CleanupReceiver is null);

        QueueEmbeddedPoolDrainPlan initialPopBoundary =
            QueueEmbeddedPoolDrainProjection.Project(new QueueEmbeddedPoolDrainCapture(
                SavedEmbeddedPoolPointer: 0x3000,
                CapturedQueuePointer: 0x4000,
                InitialPopResult: null,
                ReleaseCallReturnedNormally: null,
                NextPopResult: null,
                CleanupCallReturnedNormally: null));
        QueueEmbeddedPoolDrainPlan releaseBoundary =
            QueueEmbeddedPoolDrainProjection.Project(new QueueEmbeddedPoolDrainCapture(
                SavedEmbeddedPoolPointer: 0x3000,
                CapturedQueuePointer: 0x4000,
                InitialPopResult: 1,
                ReleaseCallReturnedNormally: false,
                NextPopResult: null,
                CleanupCallReturnedNormally: null));
        QueueEmbeddedPoolDrainPlan releaseReturnUnavailable =
            QueueEmbeddedPoolDrainProjection.Project(new QueueEmbeddedPoolDrainCapture(
                SavedEmbeddedPoolPointer: 0x3000,
                CapturedQueuePointer: 0x4000,
                InitialPopResult: 1,
                ReleaseCallReturnedNormally: null,
                NextPopResult: null,
                CleanupCallReturnedNormally: null));
        QueueEmbeddedPoolDrainPlan cleanupBoundary =
            QueueEmbeddedPoolDrainProjection.Project(new QueueEmbeddedPoolDrainCapture(
                SavedEmbeddedPoolPointer: 0x3000,
                CapturedQueuePointer: 0x4000,
                InitialPopResult: 0,
                ReleaseCallReturnedNormally: null,
                NextPopResult: null,
                CleanupCallReturnedNormally: false));
        check("Pop, release, and cleanup return uncertainty stays at the corresponding opaque call boundary",
            initialPopBoundary.Outcome
                == QueueEmbeddedPoolDrainOutcome.InitialPopReturnUnavailableAtCallBoundary
            && initialPopBoundary.Actions[^1].RawFileOffset == 0x12C0388
            && releaseBoundary.Outcome
                == QueueEmbeddedPoolDrainOutcome.ReleaseCallDidNotReturnAtCallBoundary
            && releaseBoundary.Actions[^1].RawFileOffset == 0x12C0393
            && releaseReturnUnavailable.Outcome
                == QueueEmbeddedPoolDrainOutcome.ReleaseCallReturnUnavailableAtCallBoundary
            && releaseReturnUnavailable.Actions[^1].RawFileOffset == 0x12C0393
            && cleanupBoundary.Outcome
                == QueueEmbeddedPoolDrainOutcome.CleanupCallDidNotReturnAtCallBoundary
            && cleanupBoundary.Actions[^1].RawFileOffset == 0x12C03AA);

        check("Captures from branches not reached in the one-cycle projection are rejected",
            ThrowsArgumentException(() =>
                QueueEmbeddedPoolDrainProjection.Project(
                    initialPopBoundary.Capture with { NextPopResult = 1 }))
            && ThrowsArgumentException(() =>
                QueueEmbeddedPoolDrainProjection.Project(
                    initialZero.Capture with { ReleaseCallReturnedNormally = true }))
            && ThrowsArgumentException(() =>
                QueueEmbeddedPoolDrainProjection.Project(
                    releaseBoundary.Capture with { NextPopResult = 1 }))
            && ThrowsArgumentException(() =>
                QueueEmbeddedPoolDrainProjection.Project(
                    nextPopNonzero.Capture with { CleanupCallReturnedNormally = true })));
    }

    private static bool HasDrainSlice(uint rawFileOffset, string expected)
    {
        string[] body = QueueEmbeddedPoolDrainProjection.CapturedDrainBytes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (rawFileOffset < QueueEmbeddedPoolDrainProjection.DrainStartRawFileOffset)
        {
            return false;
        }

        string[] instruction = expected.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int start = checked((int)(
            rawFileOffset - QueueEmbeddedPoolDrainProjection.DrainStartRawFileOffset));
        return start + instruction.Length <= body.Length
            && body.Skip(start).Take(instruction.Length).SequenceEqual(instruction);
    }

    private static bool ShortBranchTargets(
        uint rawFileOffset,
        string instructionHex,
        uint expectedTarget)
    {
        byte[] instruction = Convert.FromHexString(
            instructionHex.Replace(" ", string.Empty, StringComparison.Ordinal));
        return instruction.Length == 2
            && instruction[0] is 0x70 or 0x74 or 0x75 or 0xEB
            && rawFileOffset + 2 + (sbyte)instruction[1] == expectedTarget;
    }

    private static bool RelativeCallTargets(
        uint rawFileOffset,
        string instructionHex,
        uint expectedTarget)
    {
        byte[] instruction = Convert.FromHexString(
            instructionHex.Replace(" ", string.Empty, StringComparison.Ordinal));
        return instruction.Length == 5
            && instruction[0] == 0xE8
            && (long)rawFileOffset + instruction.Length
                + BinaryPrimitives.ReadInt32LittleEndian(instruction.AsSpan(1))
                == expectedTarget;
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
