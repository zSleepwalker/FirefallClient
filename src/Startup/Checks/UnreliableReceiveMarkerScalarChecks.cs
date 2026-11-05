using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveMarkerScalarChecks
{
    public static void Run(Action<string, bool> check)
    {
        UnreliableReceiveMarkerCallbackCapture markerCapture =
            new(0x1000, 7, 1, 0xAABBCCDD, 0x11223344, 0xAABBCCF0, true);
        UnreliableReceiveCallbackScalarCapture scalarCapture =
            new(0x2000, true, null, 0xA7, 0x76543210, true);
        UnreliableReceiveMarkerScalarPlan complete =
            UnreliableReceiveMarkerScalarProjection.Project(markerCapture, scalarCapture);

        check("A normal marker callback admits the independent scalar reload and helper cleanup",
            complete.MarkerCallbackPlan.Outcome
                == UnreliableReceiveMarkerCallbackOutcome.IndirectCallReturnedNormally
            && complete.ScalarPlan?.Outcome
                == UnreliableReceiveCallbackScalarOutcome.HelperReturnedNormally
            && complete.Outcome
                == UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeHelperReturnedNormally
            && complete.ScalarProjectionReached
            && complete.ScalarOutcomeHelperReturnedNormally
            && complete.NormalCallBoundaryReached
            && complete.StackCleanupBytes == 4
            && complete.SelectedDestinationRawFileOffset == 0x00BE5B32
            && complete.StopAtRawFileOffsetExclusive == 0x00BE5B32);

        check("The reloaded frame byte may differ from marker 1 and prior EAX is independent of callback data",
            complete.ScalarPlan is UnreliableReceiveCallbackScalarPlan scalarPlan
            && markerCapture.ValidatedMarker == 1
            && scalarCapture.FrameByteAt8 == 0xA7
            && scalarPlan.CapturedCl == 0xA7
            && scalarPlan.FrameByteAddress == 0x2008
            && scalarCapture.PriorCallbackEax != markerCapture.IndirectTargetAtPayloadDword8Plus4
            && scalarCapture.PriorCallbackEax != markerCapture.PayloadDwordAt8
            && scalarPlan.CapturedEdx == scalarCapture.PriorCallbackEax);

        check("Unknown ECX upper bits do not prevent normal scalar completion",
            complete.ScalarPlan?.ProjectedEcx is null
            && complete.ScalarPlan?.CapturedCl == 0xA7
            && complete.ScalarOutcomeHelperReturnedNormally);

        UnreliableReceiveMarkerScalarPlan missingScalarCapture =
            UnreliableReceiveMarkerScalarProjection.Project(markerCapture, null);
        check("A missing scalar capture is reported only after the marker callback returns normally",
            missingScalarCapture.Outcome
                == UnreliableReceiveMarkerScalarOutcome.MissingScalarCapture
            && missingScalarCapture.MarkerCallbackPlan.NormalCallBoundaryReached
            && !missingScalarCapture.ScalarProjectionReached
            && missingScalarCapture.ScalarPlan is null
            && missingScalarCapture.StopAtRawFileOffsetExclusive
                == UnreliableReceiveCallbackScalarProjection.EntryRawFileOffset);

        UnreliableReceiveCallbackScalarCapture missingPrerequisite =
            new(0x2000, null, null, null, null, null);
        UnreliableReceiveMarkerScalarPlan missingScalarPrerequisite =
            UnreliableReceiveMarkerScalarProjection.Project(markerCapture, missingPrerequisite);
        UnreliableReceiveCallbackScalarCapture contradictedPrerequisite =
            new(0x2000, false, null, null, null, null);
        UnreliableReceiveMarkerScalarPlan contradictedScalarPrerequisite =
            UnreliableReceiveMarkerScalarProjection.Project(markerCapture, contradictedPrerequisite);
        check("The scalar indirect-call normal-return prerequisite must itself be captured as true",
            missingScalarPrerequisite.Outcome
                == UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeMissingPriorIndirectCallReturn
            && missingScalarPrerequisite.ScalarPlan?.Outcome
                == UnreliableReceiveCallbackScalarOutcome.MissingPriorIndirectCallReturn
            && contradictedScalarPrerequisite.Outcome
                == UnreliableReceiveMarkerScalarOutcome.ScalarOutcomePriorIndirectCallDidNotReturnNormally
            && contradictedScalarPrerequisite.ScalarPlan?.Outcome
                == UnreliableReceiveCallbackScalarOutcome.PriorIndirectCallDidNotReturnNormally
            && !contradictedScalarPrerequisite.ScalarOutcomeHelperReturnedNormally);

        UnreliableReceiveMarkerCallbackCapture markerDidNotReturn =
            markerCapture with { IndirectCallReturnedNormally = false };
        UnreliableReceiveMarkerScalarPlan forbiddenScalarCapture =
            UnreliableReceiveMarkerScalarProjection.Project(markerDidNotReturn, scalarCapture);
        UnreliableReceiveMarkerScalarPlan noScalarAfterMarkerFailure =
            UnreliableReceiveMarkerScalarProjection.Project(markerDidNotReturn, null);
        check("Scalar captures are unreached unless the marker callback has a normal return",
            forbiddenScalarCapture.Outcome
                == UnreliableReceiveMarkerScalarOutcome.UnreachedScalarCaptureProvided
            && forbiddenScalarCapture.UnreachedScalarCaptureProvided
            && forbiddenScalarCapture.ScalarPlan is null
            && !forbiddenScalarCapture.ScalarProjectionReached
            && noScalarAfterMarkerFailure.Outcome
                == UnreliableReceiveMarkerScalarOutcome.MarkerCallbackDidNotReturnNormally
            && noScalarAfterMarkerFailure.MarkerCallbackPlan.Outcome
                == UnreliableReceiveMarkerCallbackOutcome.IndirectCallDidNotReturnNormally);

        UnreliableReceiveCallbackScalarCapture missingFrameByte =
            new(0x2000, true, 0x11223344, null, 0x76543210, true);
        UnreliableReceiveMarkerScalarPlan missingScalarFrameByte =
            UnreliableReceiveMarkerScalarProjection.Project(markerCapture, missingFrameByte);
        check("A missing scalar frame-byte read rejects all later captures without losing the callback result",
            missingScalarFrameByte.Outcome
                == UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeUnreachedCaptureProvided
            && missingScalarFrameByte.ScalarPlan?.Outcome
                == UnreliableReceiveCallbackScalarOutcome.UnreachedCaptureProvided
            && missingScalarFrameByte.ScalarPlan?.UnreachedPriorCallbackEaxCaptureProvided == true
            && missingScalarFrameByte.ScalarPlan?.UnreachedHelperReturnCaptureProvided == true
            && missingScalarFrameByte.ScalarPlan?.FrameByteAddress == 0x2008
            && missingScalarFrameByte.ScalarPlan?.CapturedCl is null
            && missingScalarFrameByte.MarkerCallbackPlan.NormalCallBoundaryReached);

        UnreliableReceiveCallbackScalarCapture failedHelper =
            new(0x2000, true, 0x11223344, 0xA7, 0x76543210, false);
        UnreliableReceiveMarkerScalarPlan failedScalarHelper =
            UnreliableReceiveMarkerScalarProjection.Project(markerCapture, failedHelper);
        check("A failed helper preserves the captured CL and ECX projection without completing cleanup",
            failedScalarHelper.Outcome
                == UnreliableReceiveMarkerScalarOutcome.ScalarOutcomeHelperDidNotReturnNormally
            && failedScalarHelper.ScalarPlan?.Outcome
                == UnreliableReceiveCallbackScalarOutcome.HelperDidNotReturnNormally
            && failedScalarHelper.ScalarPlan?.CapturedCl == 0xA7
            && failedScalarHelper.ScalarPlan?.ProjectedEcx == 0x112233A7
            && failedScalarHelper.ScalarPlan?.HelperReturnedNormally == false
            && !failedScalarHelper.ScalarOutcomeHelperReturnedNormally
            && !failedScalarHelper.NormalCallBoundaryReached
            && failedScalarHelper.StackCleanupBytes is null
            && failedScalarHelper.SelectedDestinationRawFileOffset is null);
    }
}
