namespace Firefall.Reconstruction.Startup;

public readonly record struct PayloadXorEntrySetupCapture(
    PayloadXorDiagnosticEntryCapture Entry,
    PayloadXorTableSetupCapture Table);

public enum PayloadXorEntrySetupOutcome
{
    EntryOutcomeNotAdmitted,
    EntryBoundaryByteNotCaptured,
    TableEntryBoundaryNotValidated,
    TableFramePointerMismatch,
    TableRetainedEbxMismatch,
    TableAlByteMismatch,
    TableSetupProjected
}

public readonly record struct PayloadXorEntrySetupPlan(
    PayloadXorEntrySetupCapture Capture,
    PayloadXorDiagnosticEntryPlan EntryPlan,
    PayloadXorEntrySetupOutcome Outcome,
    PayloadXorTableSetupPlan? TablePlan)
{
    public bool TableSetupProjected => TablePlan.HasValue;
}

/// <summary>
/// Composes the diagnostic entry boundary with the table-setup projection.
/// Table reads are projected only after the entry outcome and all boundary captures agree.
/// </summary>
public static class PayloadXorEntrySetupProjection
{
    public static PayloadXorEntrySetupPlan Project(
        PayloadXorEntrySetupCapture capture)
    {
        PayloadXorDiagnosticEntryPlan entryPlan =
            PayloadXorDiagnosticEntryProjection.Project(capture.Entry);

        if (entryPlan.Outcome is not (
            PayloadXorDiagnosticEntryOutcome.DiagnosticSkippedAtBoundary
            or PayloadXorDiagnosticEntryOutcome.DiagnosticReloadedAndStackCleaned))
        {
            return Plan(
                capture,
                entryPlan,
                PayloadXorEntrySetupOutcome.EntryOutcomeNotAdmitted);
        }

        if (entryPlan.AlAtBoundary is not byte alAtBoundary)
        {
            return Plan(
                capture,
                entryPlan,
                PayloadXorEntrySetupOutcome.EntryBoundaryByteNotCaptured);
        }

        if (!capture.Table.EntryBoundaryValidated)
        {
            return Plan(
                capture,
                entryPlan,
                PayloadXorEntrySetupOutcome.TableEntryBoundaryNotValidated);
        }

        if (capture.Table.FramePointer != entryPlan.Capture.FramePointer)
        {
            return Plan(
                capture,
                entryPlan,
                PayloadXorEntrySetupOutcome.TableFramePointerMismatch);
        }

        if (capture.Table.RetainedEbx != entryPlan.RetainedEbxDword)
        {
            return Plan(
                capture,
                entryPlan,
                PayloadXorEntrySetupOutcome.TableRetainedEbxMismatch);
        }

        if (capture.Table.AlByte != alAtBoundary)
        {
            return Plan(
                capture,
                entryPlan,
                PayloadXorEntrySetupOutcome.TableAlByteMismatch);
        }

        PayloadXorTableSetupPlan tablePlan =
            PayloadXorTableSetupProjection.Project(capture.Table);
        return new(
            capture,
            entryPlan,
            PayloadXorEntrySetupOutcome.TableSetupProjected,
            tablePlan);
    }

    private static PayloadXorEntrySetupPlan Plan(
        PayloadXorEntrySetupCapture capture,
        PayloadXorDiagnosticEntryPlan entryPlan,
        PayloadXorEntrySetupOutcome outcome) =>
        new(capture, entryPlan, outcome, null);
}
