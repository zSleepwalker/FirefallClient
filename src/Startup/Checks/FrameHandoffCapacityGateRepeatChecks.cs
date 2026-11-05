using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityGateRepeatChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The released repeat-store entry pins XOR EAX,EAX as 33 C0",
            FrameHandoffCapacityRepeatStoreProjection.XorEaxRawFileOffset == 0x00BE6C41
            && FrameHandoffCapacityRepeatStoreProjection.Instructions[0].Bytes == "33 C0");

        FrameHandoffCapacityGateRepeatPlan zero =
            FrameHandoffCapacityGateRepeatProjection.Project(new(
                GatePlan: Gate(frameMinusC: 0),
                RepeatEntryEcx: null,
                RepeatEntryEdx: null,
                RepeatEntryEbp: null,
                DirectionFlagSet: null,
                RepReturnedNormally: null,
                FreshFrameMinus8Value: null));
        check("An exact clean zero JZ admits only the BE6C4A join and never forms repeat EDI",
            zero.Outcome == FrameHandoffCapacityGateRepeatOutcome.ZeroBranchBypassedRepeat
            && zero.ZeroJoinAdmission
            && !zero.NonzeroGateAdmission
            && zero.GateUnreachedCapturesClear
            && zero.RepeatPlan is null
            && zero.DerivedEntryEcx is null
            && zero.DerivedEntryEdx is null
            && zero.DerivedEntryEbp is null
            && zero.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityGateRepeatProjection.ZeroJoinRawFileOffset);

        FrameHandoffCapacityGateRepeatPlan forward =
            FrameHandoffCapacityGateRepeatProjection.Project(NonzeroCapture(
                frameMinusC: 3,
                entryEbp: 0x2000,
                entryEbx: 0xFFFFFFFE,
                helperReturnEax: 3,
                entryEcx: 3,
                entryEdx: 1,
                entryEbpIdentity: 0x2000,
                directionFlagSet: false,
                freshFrameMinus8Value: 0xAABBCCDD));
        FrameHandoffCapacityGateRepeatPlan backward =
            FrameHandoffCapacityGateRepeatProjection.Project(NonzeroCapture(
                frameMinusC: 3,
                entryEbp: 0x2000,
                entryEbx: 0xFFFFFFFE,
                helperReturnEax: 3,
                entryEcx: 3,
                entryEdx: 1,
                directionFlagSet: true,
                freshFrameMinus8Value: 0x12345678));
        check("Only an exact clean nonzero gate derives REP inputs and admits exact completed forward or backward stores",
            IsComplete(forward)
            && forward.DerivedEntryEcx == 3
            && forward.DerivedEntryEdx == 1
            && forward.DerivedEntryEbp == 0x2000
            && forward.RepeatEntryIdentitiesValidated
            && forward.RepeatEntryEcxMatches == true
            && forward.RepeatEntryEdxMatches == true
            && forward.RepeatEntryEbpMatches == true
            && forward.RepeatPlan?.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(3, 1, 4, 0)
            && forward.RepeatPlan?.EdiAfterRep == 13
            && backward.Outcome == FrameHandoffCapacityGateRepeatOutcome.ProjectionComplete
            && backward.RepeatPlan?.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(3, 1, -4, 0)
            && backward.RepeatPlan?.EdiAfterRep == 0xFFFFFFF5
            && backward.RepeatPlan?.AllStoresCompleted == true
            && NoUnreachedCaptures(forward)
            && NoUnreachedCaptures(backward));

        FrameHandoffCapacityGateRepeatPlan zeroWithCaptures =
            FrameHandoffCapacityGateRepeatProjection.Project(new(
                GatePlan: Gate(frameMinusC: 0),
                RepeatEntryEcx: 0,
                RepeatEntryEdx: 0x1000,
                RepeatEntryEbp: 0x2000,
                DirectionFlagSet: false,
                RepReturnedNormally: true,
                FreshFrameMinus8Value: 0xBEEF));
        check("A zero JZ rejects all repeat captures without creating a REP plan or inferring EDI",
            zeroWithCaptures.Outcome
                == FrameHandoffCapacityGateRepeatOutcome.ZeroBranchHasUnreachedCaptures
            && !zeroWithCaptures.ZeroJoinAdmission
            && zeroWithCaptures.RepeatPlan is null
            && zeroWithCaptures.UnreachedRepeatEntryEcxProvided
            && zeroWithCaptures.UnreachedRepeatEntryEdxProvided
            && zeroWithCaptures.UnreachedRepeatEntryEbpProvided
            && zeroWithCaptures.UnreachedDirectionFlagCaptureProvided
            && zeroWithCaptures.UnreachedRepCompletionCaptureProvided
            && zeroWithCaptures.UnreachedFreshFrameMinus8CaptureProvided
            && zeroWithCaptures.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityGateRepeatProjection.ZeroJoinRawFileOffset);

        FrameHandoffCapacityGateRepeatPlan missingGate =
            FrameHandoffCapacityGateRepeatProjection.Project(new(
                GatePlan: null,
                RepeatEntryEcx: 5,
                RepeatEntryEdx: 0x4000,
                RepeatEntryEbp: 0x8000,
                DirectionFlagSet: true,
                RepReturnedNormally: true,
                FreshFrameMinus8Value: 0xCAFE));
        check("Missing gate blocks every repeat capture and leaves the REP plan absent",
            missingGate.Outcome == FrameHandoffCapacityGateRepeatOutcome.MissingGatePlan
            && !missingGate.NonzeroGateAdmission
            && !missingGate.ZeroJoinAdmission
            && missingGate.RepeatPlan is null
            && missingGate.UnreachedRepeatEntryEcxProvided
            && missingGate.UnreachedRepeatEntryEdxProvided
            && missingGate.UnreachedRepeatEntryEbpProvided
            && missingGate.UnreachedDirectionFlagCaptureProvided
            && missingGate.UnreachedRepCompletionCaptureProvided
            && missingGate.UnreachedFreshFrameMinus8CaptureProvided
            && missingGate.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityGateRepeatProjection.LifecycleStartRawFileOffset);

        FrameHandoffCapacityGateRepeatPlan identityMismatch =
            FrameHandoffCapacityGateRepeatProjection.Project(NonzeroCapture(
                frameMinusC: 4,
                entryEbp: 0x5000,
                entryEbx: 0x7000,
                helperReturnEax: 0x100,
                entryEcx: 5,
                entryEdx: 0x7101,
                entryEbpIdentity: 0x5001,
                directionFlagSet: false,
                repReturnedNormally: true,
                freshFrameMinus8Value: 0));
        check("Repeat ECX, EDX, and EBP identities are validated before any REP-window instruction",
            identityMismatch.Outcome
                == FrameHandoffCapacityGateRepeatOutcome.RepeatEntryIdentityMismatch
            && identityMismatch.NonzeroGateAdmission
            && identityMismatch.RepeatEntryIdentitiesValidated
            && identityMismatch.RepeatEntryEcxMatches == false
            && identityMismatch.RepeatEntryEdxMatches == false
            && identityMismatch.RepeatEntryEbpMatches == false
            && identityMismatch.RepeatPlan is null
            && identityMismatch.DerivedEntryEcx == 4
            && identityMismatch.DerivedEntryEdx == 0x7100
            && identityMismatch.DerivedEntryEbp == 0x5000
            && identityMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityGateRepeatProjection.RepeatEntryRawFileOffset);

        FrameHandoffCapacityGateRepeatPlan missingDirection =
            FrameHandoffCapacityGateRepeatProjection.Project(NonzeroCapture(
                directionFlagSet: null,
                repReturnedNormally: true,
                freshFrameMinus8Value: 0x1234));
        FrameHandoffCapacityGateRepeatPlan missingCompletion =
            FrameHandoffCapacityGateRepeatProjection.Project(NonzeroCapture(
                directionFlagSet: false,
                repReturnedNormally: null,
                freshFrameMinus8Value: 0x1234));
        check("Missing DF or REP-completion evidence blocks the repeat plan before claiming stores completed",
            missingDirection.Outcome == FrameHandoffCapacityGateRepeatOutcome.RepeatBlocked
            && missingDirection.RepeatPlan?.Outcome
                == FrameHandoffCapacityRepeatStoreOutcome.MissingDirectionFlagCapture
            && !missingDirection.RepeatPlan!.Value.AllStoresCompleted
            && missingDirection.RepeatPlan.Value.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityRepeatStoreProjection.RepStosdRawFileOffset
            && missingCompletion.Outcome
                == FrameHandoffCapacityGateRepeatOutcome.RepeatBlocked
            && missingCompletion.RepeatPlan?.Outcome
                == FrameHandoffCapacityRepeatStoreOutcome.MissingRepCompletionCapture
            && !missingCompletion.RepeatPlan!.Value.AllStoresCompleted
            && missingCompletion.RepeatPlan.Value.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityRepeatStoreProjection.RepStosdRawFileOffset);

        FrameHandoffCapacityGateRepeatPlan missingFreshRead =
            FrameHandoffCapacityGateRepeatProjection.Project(NonzeroCapture(
                frameMinusC: 2,
                entryEbx: 0xFFFFFFFC,
                helperReturnEax: 0,
                entryEdx: 0xFFFFFFFC,
                directionFlagSet: false,
                repReturnedNormally: true,
                freshFrameMinus8Value: null));
        check("A missing fresh frame-minus-eight read retains the already completed wrapped REP stores",
            missingFreshRead.Outcome == FrameHandoffCapacityGateRepeatOutcome.RepeatBlocked
            && missingFreshRead.RepeatPlan?.Outcome
                == FrameHandoffCapacityRepeatStoreOutcome.MissingFreshFrameMinus8Capture
            && missingFreshRead.RepeatPlan?.AllStoresCompleted == true
            && missingFreshRead.RepeatPlan?.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(2, 0xFFFFFFFC, 4, 0)
            && missingFreshRead.RepeatPlan?.EcxAfterRep == 0
            && missingFreshRead.RepeatPlan?.EdiAfterRep == 4
            && missingFreshRead.RepeatPlan?.FreshFrameMinus8ReadAddress == 0x1FF8
            && missingFreshRead.RepeatPlan?.FreshFrameMinus8Value is null
            && missingFreshRead.RepeatPlan?.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityRepeatStoreProjection.FreshFrameMinus8ReloadRawFileOffset);

        FrameHandoffCapacityGateRepeatPlan wrapped =
            FrameHandoffCapacityGateRepeatProjection.Project(NonzeroCapture(
                frameMinusC: uint.MaxValue,
                entryEbp: 3,
                entryEbx: uint.MaxValue,
                helperReturnEax: 2,
                entryEcx: uint.MaxValue,
                entryEdx: 1,
                entryEbpIdentity: 3,
                directionFlagSet: false,
                repReturnedNormally: true,
                freshFrameMinus8Value: 0));
        check("Gate EDX LEA, frame-minus-eight address, and maximum-count REP arithmetic wrap as DWORDs",
            wrapped.Outcome == FrameHandoffCapacityGateRepeatOutcome.ProjectionComplete
            && wrapped.DerivedEntryEcx == uint.MaxValue
            && wrapped.DerivedEntryEdx == 1
            && wrapped.DerivedEntryEbp == 3
            && wrapped.RepeatPlan?.FreshFrameMinus8ReadAddress == 0xFFFFFFFB
            && wrapped.RepeatPlan?.StoreDescriptor
                == new FrameHandoffCapacityRepeatStoreDescriptor(
                    uint.MaxValue, 1, 4, 0)
            && wrapped.RepeatPlan?.EcxAfterRep == 0
            && wrapped.RepeatPlan?.EdiAfterRep == 0xFFFFFFFD
            && wrapped.RepeatPlan?.AllStoresCompleted == true);

        FrameHandoffCapacityGateRepeatPlan gateMissingRead =
            FrameHandoffCapacityGateRepeatProjection.Project(new(
                GatePlan: FrameHandoffCapacitySecondReturnGateProjection.Project(new(
                    ValidatedNormalEntry: true,
                    EntryEbp: 0x2000,
                    EntryEbx: 0x3000,
                    FrameMinusC: null,
                    HelperReturnEax: 0x10)),
                RepeatEntryEcx: 7,
                RepeatEntryEdx: 0x4000,
                RepeatEntryEbp: 0x2000,
                DirectionFlagSet: true,
                RepReturnedNormally: true,
                FreshFrameMinus8Value: 0));
        check("A gate that did not reach its exact nonzero branch blocks all downstream repeat captures",
            gateMissingRead.Outcome == FrameHandoffCapacityGateRepeatOutcome.GateHasUnreachedCaptures
            && gateMissingRead.RepeatPlan is null
            && !gateMissingRead.NonzeroGateAdmission
            && gateMissingRead.UnreachedRepeatEntryEcxProvided
            && gateMissingRead.UnreachedRepeatEntryEdxProvided
            && gateMissingRead.UnreachedRepeatEntryEbpProvided
            && gateMissingRead.UnreachedDirectionFlagCaptureProvided
            && gateMissingRead.UnreachedRepCompletionCaptureProvided
            && gateMissingRead.UnreachedFreshFrameMinus8CaptureProvided);
    }

    private static bool IsComplete(FrameHandoffCapacityGateRepeatPlan plan) =>
        plan.Outcome == FrameHandoffCapacityGateRepeatOutcome.ProjectionComplete
        && plan.NonzeroGateAdmission
        && !plan.ZeroJoinAdmission
        && plan.GateUnreachedCapturesClear
        && plan.RepeatPlan?.Outcome == FrameHandoffCapacityRepeatStoreOutcome.Completed
        && plan.RepeatPlan?.AllStoresCompleted == true
        && plan.RepeatPlan?.EcxAfterRep == 0;

    private static bool NoUnreachedCaptures(FrameHandoffCapacityGateRepeatPlan plan) =>
        !plan.UnreachedRepeatEntryEcxProvided
        && !plan.UnreachedRepeatEntryEdxProvided
        && !plan.UnreachedRepeatEntryEbpProvided
        && !plan.UnreachedDirectionFlagCaptureProvided
        && !plan.UnreachedRepCompletionCaptureProvided
        && !plan.UnreachedFreshFrameMinus8CaptureProvided;

    private static FrameHandoffCapacitySecondReturnGatePlan Gate(
        uint frameMinusC,
        uint entryEbp = 0x2000,
        uint entryEbx = 0x3000,
        uint helperReturnEax = 0x100) =>
        FrameHandoffCapacitySecondReturnGateProjection.Project(new(
            ValidatedNormalEntry: true,
            EntryEbp: entryEbp,
            EntryEbx: entryEbx,
            FrameMinusC: frameMinusC,
            HelperReturnEax: helperReturnEax));

    private static FrameHandoffCapacityGateRepeatCapture NonzeroCapture(
        uint frameMinusC = 3,
        uint entryEbp = 0x2000,
        uint entryEbx = 0x3000,
        uint helperReturnEax = 0x100,
        uint? entryEcx = null,
        uint? entryEdx = null,
        uint? entryEbpIdentity = null,
        bool? directionFlagSet = false,
        bool? repReturnedNormally = true,
        uint? freshFrameMinus8Value = 0xAABBCCDD) =>
        new(
            Gate(frameMinusC, entryEbp, entryEbx, helperReturnEax),
            entryEcx,
            entryEdx,
            entryEbpIdentity,
            directionFlagSet,
            repReturnedNormally,
            freshFrameMinus8Value);
}
