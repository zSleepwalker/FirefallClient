using Firefall.Reconstruction.Startup;

internal static class PayloadXorEntrySetupChecks
{
    public static void Run(Action<string, bool> check)
    {
        bool allBoundaryMarkersCompose = true;
        bool allByteIndicesCompose = true;
        for (int value = byte.MinValue; value <= byte.MaxValue; value++)
        {
            byte al = (byte)value;
            PayloadXorDiagnosticEntryCapture entry;
            if (al is >= 1 and <= 3)
            {
                entry = new(al, 0xA1B2C3D4, 0x2000, null, null);
            }
            else
            {
                byte marker = al == 0 ? (byte)0 : (byte)4;
                entry = new(marker, 0xA1B2C3D4, 0x2000, true, al);
            }

            PayloadXorEntrySetupPlan plan = Project(
                entry,
                retainedEbx: 0xA1B2C3D4,
                al: al,
                stackLength: 8,
                first: 0,
                second: 0);
            allBoundaryMarkersCompose &=
                plan.Outcome == PayloadXorEntrySetupOutcome.TableSetupProjected
                && plan.EntryPlan.AlAtBoundary == al
                && plan.TablePlan is PayloadXorTableSetupPlan table
                && table.Capture.AlByte == al
                && table.FirstTableDwordAddress
                    == PayloadXorTableSetupProjection.FirstTableDwordAddress(al)
                && table.SecondTableDwordAddress
                    == PayloadXorTableSetupProjection.SecondTableDwordAddress(al);

            PayloadXorDiagnosticEntryCapture independentReloadEntry =
                new(0xFE, 0xA1B2C3D4, 0x2000, true, al);
            PayloadXorEntrySetupPlan independentReload = Project(
                independentReloadEntry,
                retainedEbx: 0xA1B2C3D4,
                al: al,
                stackLength: 8,
                first: 0,
                second: 0);
            allByteIndicesCompose &=
                independentReload.Outcome
                    == PayloadXorEntrySetupOutcome.TableSetupProjected
                && independentReload.EntryPlan.MarkerStoredAtFrameMinus1 == 0xFE
                && independentReload.EntryPlan.AlAtBoundary == al
                && independentReload.TablePlan is PayloadXorTableSetupPlan reloadedTable
                && reloadedTable.Capture.AlByte == al
                && reloadedTable.FirstTableDwordAddress
                    == PayloadXorTableSetupProjection.FirstTableDwordAddress(al)
                && reloadedTable.SecondTableDwordAddress
                    == PayloadXorTableSetupProjection.SecondTableDwordAddress(al);
        }

        check("All 256 boundary bytes, including independent diagnostic reloads, select native table indices",
            allBoundaryMarkersCompose && allByteIndicesCompose);

        PayloadXorEntrySetupPlan skippedWithUnreachedReturn = Project(
            new(2, 0x11223344, 0x3000, true, null),
            stackLength: 8,
            first: 0xAABBCCDD,
            second: 0x55667788);
        PayloadXorEntrySetupPlan skippedWithUnreachedReload = Project(
            new(3, 0x11223344, 0x3000, null, 0x7F),
            stackLength: 8,
            first: 0xAABBCCDD,
            second: 0x55667788);
        PayloadXorEntrySetupPlan missingDiagnosticReturn = Project(
            new(4, 0x11223344, 0x3000, null, null),
            stackLength: 8,
            first: 0xAABBCCDD,
            second: 0x55667788);
        PayloadXorEntrySetupPlan diagnosticDidNotReturn = Project(
            new(4, 0x11223344, 0x3000, false, null),
            stackLength: 8,
            first: 0xAABBCCDD,
            second: 0x55667788);
        check("Only the two completed entry outcomes admit table setup; route and stop offsets cannot admit unreached captures",
            skippedWithUnreachedReturn.EntryPlan.Outcome
                == PayloadXorDiagnosticEntryOutcome.UnreachedCaptureProvided
            && skippedWithUnreachedReturn.EntryPlan.RouteRawFileOffset
                == PayloadXorDiagnosticEntryProjection.DiagnosticBoundaryRawFileOffsetExclusive
            && skippedWithUnreachedReturn.EntryPlan.StopAtRawFileOffsetExclusive
                == PayloadXorDiagnosticEntryProjection.DiagnosticBoundaryRawFileOffsetExclusive
            && skippedWithUnreachedReturn.Outcome
                == PayloadXorEntrySetupOutcome.EntryOutcomeNotAdmitted
            && skippedWithUnreachedReturn.TablePlan is null
            && skippedWithUnreachedReload.EntryPlan.Outcome
                == PayloadXorDiagnosticEntryOutcome.UnreachedCaptureProvided
            && skippedWithUnreachedReload.Outcome
                == PayloadXorEntrySetupOutcome.EntryOutcomeNotAdmitted
            && skippedWithUnreachedReload.TablePlan is null
            && missingDiagnosticReturn.Outcome
                == PayloadXorEntrySetupOutcome.EntryOutcomeNotAdmitted
            && missingDiagnosticReturn.TablePlan is null
            && diagnosticDidNotReturn.Outcome
                == PayloadXorEntrySetupOutcome.EntryOutcomeNotAdmitted
            && diagnosticDidNotReturn.TablePlan is null);

        PayloadXorEntrySetupPlan invalidTableBoundary = Project(
            new(1, 0x11223344, 0x3000, null, null),
            tableBoundaryValidated: false,
            framePointer: 0x3000,
            retainedEbx: 0x11223344,
            al: 1,
            stackLength: 8,
            first: 0,
            second: 0);
        PayloadXorEntrySetupPlan mismatchedFrame = Project(
            new(1, 0x11223344, 0x3000, null, null),
            framePointer: 0x3004,
            retainedEbx: 0x11223344,
            al: 1,
            stackLength: 8,
            first: 0,
            second: 0);
        PayloadXorEntrySetupPlan mismatchedEbx = Project(
            new(1, 0x11223344, 0x3000, null, null),
            framePointer: 0x3000,
            retainedEbx: 0x44332211,
            al: 1,
            stackLength: 8,
            first: 0,
            second: 0);
        PayloadXorEntrySetupPlan mismatchedAl = Project(
            new(1, 0x11223344, 0x3000, null, null),
            framePointer: 0x3000,
            retainedEbx: 0x11223344,
            al: 2,
            stackLength: 8,
            first: 0,
            second: 0);
        check("Boundary validation and all entry/table identity fields are required before any stack or table read is projected",
            invalidTableBoundary.Outcome
                == PayloadXorEntrySetupOutcome.TableEntryBoundaryNotValidated
            && invalidTableBoundary.TablePlan is null
            && mismatchedFrame.Outcome
                == PayloadXorEntrySetupOutcome.TableFramePointerMismatch
            && mismatchedFrame.TablePlan is null
            && mismatchedEbx.Outcome
                == PayloadXorEntrySetupOutcome.TableRetainedEbxMismatch
            && mismatchedEbx.TablePlan is null
            && mismatchedAl.Outcome
                == PayloadXorEntrySetupOutcome.TableAlByteMismatch
            && mismatchedAl.TablePlan is null
            && new[]
            {
                invalidTableBoundary,
                mismatchedFrame,
                mismatchedEbx,
                mismatchedAl
            }.All(plan => plan.EntryPlan.Outcome
                == PayloadXorDiagnosticEntryOutcome.DiagnosticSkippedAtBoundary));

        PayloadXorEntrySetupPlan missingSecond = Project(
            new(0xFE, 0xCAFEBABE, 0xFFFFFFFC, true, 0),
            framePointer: 0xFFFFFFFC,
            retainedEbx: 0xCAFEBABE,
            al: 0,
            stackLength: 8,
            first: 0xA1B2C3D4,
            second: null);
        check("A partial composed table plan retains the first DWORD when the second table capture is missing",
            missingSecond.Outcome == PayloadXorEntrySetupOutcome.TableSetupProjected
            && missingSecond.EntryPlan.Outcome
                == PayloadXorDiagnosticEntryOutcome.DiagnosticReloadedAndStackCleaned
            && missingSecond.EntryPlan.AlAtBoundary == 0
            && missingSecond.TablePlan is PayloadXorTableSetupPlan missingSecondTable
            && missingSecondTable.Outcome
                == PayloadXorTableSetupOutcome.MissingSecondTableDword
            && missingSecondTable.FirstTableDwordRead
            && missingSecondTable.FirstTableDword == 0xA1B2C3D4
            && !missingSecondTable.SecondTableDwordRead
            && missingSecondTable.SecondTableDwordAddress
                == PayloadXorTableSetupProjection.SecondTableDwordAddress(0)
            && missingSecondTable.StopAtRawFileOffsetExclusive
                == PayloadXorTableSetupProjection.SecondTableReadRawFileOffset);

        PayloadXorEntrySetupPlan completedDiagnostic = Project(
            new(0xFE, 0x1000, 0x2000, true, 0),
            retainedEbx: 0x1000,
            al: 0,
            stackLength: 8,
            first: 0,
            second: 0);
        int reloadIndex = IndexOf(
            completedDiagnostic.EntryPlan.Actions,
            PayloadXorDiagnosticEntryActionKind.ReloadAlFromFrameMinus1);
        int cleanupIndex = IndexOf(
            completedDiagnostic.EntryPlan.Actions,
            PayloadXorDiagnosticEntryActionKind.CleanupDiagnosticArguments);
        int boundaryIndex = IndexOf(
            completedDiagnostic.EntryPlan.Actions,
            PayloadXorDiagnosticEntryActionKind.StopBeforeStackLengthRead);
        check("Diagnostic stack cleanup follows a successful independent reload, and the composed table plan stops before the XOR body",
            completedDiagnostic.EntryPlan.Outcome
                == PayloadXorDiagnosticEntryOutcome.DiagnosticReloadedAndStackCleaned
            && reloadIndex >= 0
            && cleanupIndex > reloadIndex
            && boundaryIndex > cleanupIndex
            && completedDiagnostic.EntryPlan.Actions[boundaryIndex].RawFileOffset
                == PayloadXorDiagnosticEntryProjection.DiagnosticBoundaryRawFileOffsetExclusive
            && completedDiagnostic.TablePlan is PayloadXorTableSetupPlan completedTable
            && completedTable.Outcome
                == PayloadXorTableSetupOutcome.EightByteSpanWithinEnd
            && completedTable.StopAtRawFileOffsetExclusive
                == PayloadXorTableSetupProjection.EntryEndRawFileOffsetExclusive
            && completedTable.Actions[^1].Kind
                == PayloadXorTableSetupActionKind.StopBeforeXorBody
            && completedTable.Actions.All(action =>
                action.Kind is not (
                    PayloadXorTableSetupActionKind.ReadStackLengthFromFramePlus8
                        or PayloadXorTableSetupActionKind.ReadFirstTableDword
                        or PayloadXorTableSetupActionKind.ReadSecondTableDword)
                || action.RawFileOffset
                    < PayloadXorTableSetupProjection.EntryEndRawFileOffsetExclusive));
    }

    private static PayloadXorEntrySetupPlan Project(
        PayloadXorDiagnosticEntryCapture entry,
        bool tableBoundaryValidated = true,
        uint framePointer = 0x2000,
        uint retainedEbx = 0x1000,
        byte al = 1,
        uint? stackLength = 8,
        uint? first = 0,
        uint? second = 0) =>
        PayloadXorEntrySetupProjection.Project(new(
            entry,
            new(
                tableBoundaryValidated,
                framePointer,
                retainedEbx,
                al,
                stackLength,
                first,
                second)));

    private static int IndexOf(
        PayloadXorDiagnosticEntryAction[] actions,
        PayloadXorDiagnosticEntryActionKind kind) =>
        Array.FindIndex(actions, action => action.Kind == kind);
}
