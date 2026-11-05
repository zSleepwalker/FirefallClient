using Firefall.Reconstruction.Startup;

internal static class PayloadXorDiagnosticEntryChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The exact authorized raw-byte window, sample identity, and raw-to-VA mapping are pinned",
            PayloadXorDiagnosticEntryProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadXorDiagnosticEntryProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadXorDiagnosticEntryProjection.EntryStartRawFileOffset == 0x00BD58E0
            && PayloadXorDiagnosticEntryProjection.EntryEndRawFileOffsetExclusive == 0x00BD5912
            && PayloadXorDiagnosticEntryProjection.CapturedCodeLength == 0x32
            && PayloadXorDiagnosticEntryProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PayloadXorDiagnosticEntryProjection.EntryStartVirtualAddress == 0x00FD64E0
            && PayloadXorDiagnosticEntryProjection.EntryEndVirtualAddressExclusive == 0x00FD6512
            && PayloadXorDiagnosticEntryProjection.DiagnosticCallTargetVirtualAddress
                == 0x01C15EA0
            && PayloadXorDiagnosticEntryProjection.DiagnosticArgumentZero == 0
            && PayloadXorDiagnosticEntryProjection.DiagnosticArgumentLine == 0x70F
            && PayloadXorDiagnosticEntryProjection.DiagnosticArgumentMessagePointer
                == 0x1B585FB
            && PayloadXorDiagnosticEntryProjection.DiagnosticArgumentSourcePointer
                == 0x1B58601
            && PayloadXorDiagnosticEntryProjection.DiagnosticStackCleanupBytes == 0x10
            && string.Concat(PayloadXorDiagnosticEntryProjection.CapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "558BEC518AC18845FF538BDA5684C074043C04721D6A00680F070000"
                    + "68FB85B501680186B501FF15A05EC1018A45FF83C410"
            && VerifyInstructionEvidence());

        bool allMarkerGates = true;
        for (int marker = byte.MinValue; marker <= byte.MaxValue; marker++)
        {
            byte value = (byte)marker;
            bool diagnostic = value == 0 || value >= 4;
            PayloadXorDiagnosticEntryPlan plan = Project(
                marker: value,
                edx: 0xA1B2C3D4,
                framePointer: 0x1000,
                returnedNormally: diagnostic ? true : null,
                reload: diagnostic ? value : null);
            bool correct = plan.Capture.MarkerByte == value
                && plan.AlAfterClCopy == value
                && plan.MarkerStoredAtFrameMinus1 == value
                && plan.FrameMinus1Address == 0x0FFF
                && plan.RetainedEbxDword == 0xA1B2C3D4
                && plan.DiagnosticCallReached == diagnostic
                && plan.PostDiagnosticReloadRead == diagnostic
                && plan.StopAtRawFileOffsetExclusive
                    == PayloadXorDiagnosticEntryProjection.DiagnosticBoundaryRawFileOffsetExclusive;
            if (diagnostic)
            {
                correct &= plan.Outcome
                        == PayloadXorDiagnosticEntryOutcome.DiagnosticReloadedAndStackCleaned
                    && plan.AlAtBoundary == value
                    && plan.DiagnosticArgumentStackCleanupBytes
                        == PayloadXorDiagnosticEntryProjection.DiagnosticStackCleanupBytes
                    && plan.Actions.Any(action =>
                        action.Kind == PayloadXorDiagnosticEntryActionKind.CallDiagnostic);
            }
            else
            {
                correct &= plan.Outcome
                        == PayloadXorDiagnosticEntryOutcome.DiagnosticSkippedAtBoundary
                    && plan.AlAtBoundary == value
                    && plan.DiagnosticArgumentStackCleanupBytes == 0
                    && !plan.Actions.Any(action =>
                        action.Kind is PayloadXorDiagnosticEntryActionKind.CallDiagnostic
                            or PayloadXorDiagnosticEntryActionKind.ReloadAlFromFrameMinus1
                            or PayloadXorDiagnosticEntryActionKind.CleanupDiagnosticArguments);
            }

            allMarkerGates &= correct;
        }

        check("All 256 entry markers follow AL==0 or unsigned AL>=4 diagnostic gates without treating 4..255 as invalid",
            allMarkerGates);

        PayloadXorDiagnosticEntryPlan changedReload = Project(
            marker: 0xFE,
            edx: 0x12345678,
            framePointer: 0,
            returnedNormally: true,
            reload: 0x19);
        PayloadXorDiagnosticEntryPlan zeroReload = Project(
            marker: 0,
            framePointer: 1,
            returnedNormally: true,
            reload: 0);
        check("The post-call byte reload is independent of the stored marker and accepts a native zero",
            changedReload.Outcome
                == PayloadXorDiagnosticEntryOutcome.DiagnosticReloadedAndStackCleaned
            && changedReload.MarkerStoredAtFrameMinus1 == 0xFE
            && changedReload.AlAtBoundary == 0x19
            && changedReload.FrameMinus1Address == uint.MaxValue
            && changedReload.RetainedEbxDword == 0x12345678
            && zeroReload.Outcome
                == PayloadXorDiagnosticEntryOutcome.DiagnosticReloadedAndStackCleaned
            && zeroReload.MarkerStoredAtFrameMinus1 == 0
            && zeroReload.AlAtBoundary == 0
            && zeroReload.FrameMinus1Address == 0);

        PayloadXorDiagnosticEntryPlan skippedWithReturn = Project(
            marker: 2,
            returnedNormally: true);
        PayloadXorDiagnosticEntryPlan skippedWithReload = Project(
            marker: 3,
            reload: 0);
        PayloadXorDiagnosticEntryPlan missingReturnStatus = Project(
            marker: 4,
            returnedNormally: null);
        PayloadXorDiagnosticEntryPlan unreachedReload = Project(
            marker: 4,
            returnedNormally: null,
            reload: 0xAA);
        PayloadXorDiagnosticEntryPlan didNotReturn = Project(
            marker: 4,
            returnedNormally: false);
        PayloadXorDiagnosticEntryPlan reloadAfterNonreturn = Project(
            marker: 4,
            returnedNormally: false,
            reload: 0xAA);
        PayloadXorDiagnosticEntryPlan missingReload = Project(
            marker: 4,
            returnedNormally: true);
        check("Missing, unreached, and nonreturn captures stop before later instructions",
            skippedWithReturn.Outcome
                == PayloadXorDiagnosticEntryOutcome.UnreachedCaptureProvided
            && !skippedWithReturn.DiagnosticCallReached
            && !skippedWithReturn.PostDiagnosticReloadRead
            && skippedWithReload.Outcome
                == PayloadXorDiagnosticEntryOutcome.UnreachedCaptureProvided
            && !skippedWithReload.DiagnosticCallReached
            && missingReturnStatus.Outcome
                == PayloadXorDiagnosticEntryOutcome.DiagnosticReturnStatusNotCaptured
            && missingReturnStatus.DiagnosticCallReached
            && !missingReturnStatus.PostDiagnosticReloadRead
            && missingReturnStatus.StopAtRawFileOffsetExclusive
                == PayloadXorDiagnosticEntryProjection.DiagnosticCallRawFileOffset
            && unreachedReload.Outcome
                == PayloadXorDiagnosticEntryOutcome.UnreachedCaptureProvided
            && !unreachedReload.PostDiagnosticReloadRead
            && didNotReturn.Outcome
                == PayloadXorDiagnosticEntryOutcome.DiagnosticDidNotReturn
            && reloadAfterNonreturn.Outcome
                == PayloadXorDiagnosticEntryOutcome.UnreachedCaptureProvided
            && !reloadAfterNonreturn.PostDiagnosticReloadRead
            && missingReload.Outcome
                == PayloadXorDiagnosticEntryOutcome.MissingPostDiagnosticFrameMinus1Reload
            && missingReload.StopAtRawFileOffsetExclusive
                == PayloadXorDiagnosticEntryProjection.FrameMinus1ReloadRawFileOffset
            && !missingReload.PostDiagnosticReloadRead
            && missingReload.DiagnosticArgumentStackCleanupBytes is null);

        PayloadXorDiagnosticEntryPlan completed = Project(
            marker: 0,
            returnedNormally: true,
            reload: 0x7B);
        int callIndex = IndexOf(
            completed.Actions,
            PayloadXorDiagnosticEntryActionKind.CallDiagnostic);
        int returnedIndex = IndexOf(
            completed.Actions,
            PayloadXorDiagnosticEntryActionKind.DiagnosticReturnedNormally);
        int reloadIndex = IndexOf(
            completed.Actions,
            PayloadXorDiagnosticEntryActionKind.ReloadAlFromFrameMinus1);
        int cleanupIndex = IndexOf(
            completed.Actions,
            PayloadXorDiagnosticEntryActionKind.CleanupDiagnosticArguments);
        int boundaryIndex = IndexOf(
            completed.Actions,
            PayloadXorDiagnosticEntryActionKind.StopBeforeStackLengthRead);
        int storeIndex = IndexOf(
            completed.Actions,
            PayloadXorDiagnosticEntryActionKind.StoreAlAtFrameMinus1);
        check("The marker store precedes the opaque call; normal return, independent AL reload, cleanup, and boundary are ordered",
            completed.Outcome
                == PayloadXorDiagnosticEntryOutcome.DiagnosticReloadedAndStackCleaned
            && storeIndex >= 0
            && storeIndex < callIndex
            && callIndex < returnedIndex
            && returnedIndex < reloadIndex
            && reloadIndex < cleanupIndex
            && cleanupIndex < boundaryIndex
            && completed.Actions[storeIndex].RawFileOffset
                == PayloadXorDiagnosticEntryProjection.StoreAlAtFrameMinus1RawFileOffset
            && completed.Actions[callIndex].RawFileOffset
                == PayloadXorDiagnosticEntryProjection.DiagnosticCallRawFileOffset
            && completed.Actions[reloadIndex].RawFileOffset
                == PayloadXorDiagnosticEntryProjection.FrameMinus1ReloadRawFileOffset
            && completed.Actions[cleanupIndex].RawFileOffset
                == PayloadXorDiagnosticEntryProjection.DiagnosticStackCleanupRawFileOffset
            && completed.StopAtRawFileOffsetExclusive
                == PayloadXorDiagnosticEntryProjection.DiagnosticBoundaryRawFileOffsetExclusive
            && completed.Actions[^1].Kind
                == PayloadXorDiagnosticEntryActionKind.StopBeforeStackLengthRead
            && completed.Actions[^1].RawFileOffset
                == PayloadXorDiagnosticEntryProjection.DiagnosticBoundaryRawFileOffsetExclusive
            && completed.Actions
                .Where(action =>
                    action.Kind != PayloadXorDiagnosticEntryActionKind.StopBeforeStackLengthRead)
                .All(action =>
                    action.RawFileOffset
                        < PayloadXorDiagnosticEntryProjection.DiagnosticBoundaryRawFileOffsetExclusive)
            && completed.DiagnosticArgumentStackCleanupBytes == 0x10);
    }

    private static PayloadXorDiagnosticEntryPlan Project(
        byte marker = 0,
        uint edx = 0x1234,
        uint framePointer = 0x2000,
        bool? returnedNormally = true,
        byte? reload = null) =>
        PayloadXorDiagnosticEntryProjection.Project(new(
            marker,
            edx,
            framePointer,
            returnedNormally,
            reload));

    private static int IndexOf(
        PayloadXorDiagnosticEntryAction[] actions,
        PayloadXorDiagnosticEntryActionKind kind) =>
        Array.FindIndex(actions, action => action.Kind == kind);

    private static bool VerifyInstructionEvidence()
    {
        uint nextRawOffset = PayloadXorDiagnosticEntryProjection.EntryStartRawFileOffset;
        string instructionBytes = string.Empty;
        foreach (PayloadXorDiagnosticEntryInstructionEvidence instruction
            in PayloadXorDiagnosticEntryProjection.Instructions)
        {
            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != PayloadXorDiagnosticEntryProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            if (instruction.BranchTargetRawFileOffset is uint targetRaw
                && instruction.BranchTargetVirtualAddress
                    != PayloadXorDiagnosticEntryProjection.RawFileOffsetToVirtualAddress(targetRaw))
            {
                return false;
            }

            instructionBytes += bytes;
            nextRawOffset += (uint)(bytes.Length / 2);
        }

        return nextRawOffset
                == PayloadXorDiagnosticEntryProjection.EntryEndRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(
                    PayloadXorDiagnosticEntryProjection.CapturedCodeBytes
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
