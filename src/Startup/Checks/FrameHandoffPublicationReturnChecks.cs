using Firefall.Reconstruction.Startup;

internal static class FrameHandoffPublicationReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        FrameHandoffPublicationReturnPlan cleanStore =
            FrameHandoffPublicationReturnProjection.Project(new(
                Publication: new(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    ReceiverBaseEsi: 0x1000,
                    SlotArrayBaseEbx: 0x2000,
                    IncomingEdi: 1,
                    IncomingEcx: 0x4000,
                    ReceiverPlus04At6508: 0x10,
                    ReceiverPlus04At650E: 0x20,
                    SlotDwordAtEbxPlusFreshBase: 0x5000),
                Final: new(
                    EntryEcx: 0x5004,
                    EntryEsi: 0x1000,
                    EntryEbp: 0x3000,
                    Frame8Pointer: 0x6000,
                    PointeeDword: 0xCAFEBABE,
                    Receiver10OldValue: 7)));
        check("The clean 6508 route publishes then validates and projects the final write and return",
            cleanStore.Outcome == FrameHandoffPublicationReturnOutcome.ProjectionComplete
            && cleanStore.PublicationPlan.Outcome
                == FrameHandoffSlotPublicationOutcome.ProjectionComplete
            && cleanStore.PublicationPlan.StoreWriteProjected
            && cleanStore.PublicationPlan.StoreWriteAddress == 0x2010
            && cleanStore.PublicationPlan.StoreWriteValue == 0x4000
            && cleanStore.PublicationPlan.ComputedEcxBeforeTest == 0x5004
            && cleanStore.FinalEntryEcxMatchesPublication
            && cleanStore.FinalEntryEsiMatchesPublication
            && cleanStore.FinalPlan is { Outcome: FrameHandoffFinalWriteReturnOutcome.ReturnedNormally }
            && cleanStore.FinalPlan.Value.FinalWriteAddress == 0x5004
            && cleanStore.FinalPlan.Value.FinalWriteValue == 0xCAFEBABE
            && cleanStore.FinalPlan.Value.CounterValueAfterIncrement == 8
            && !AnyUnreachedCaptures(cleanStore));

        FrameHandoffPublicationReturnPlan cleanSkipStore =
            FrameHandoffPublicationReturnProjection.Project(new(
                Publication: new(
                    FrameHandoffSlotPublicationEntry.ExistingNonzeroSlotBranchAt650E,
                    ReceiverBaseEsi: 0x1100,
                    SlotArrayBaseEbx: 0x2200,
                    IncomingEdi: 0,
                    IncomingEcx: null,
                    ReceiverPlus04At6508: null,
                    ReceiverPlus04At650E: 0x30,
                    SlotDwordAtEbxPlusFreshBase: 0x5004),
                Final: new(
                    EntryEcx: 0x5004,
                    EntryEsi: 0x1100,
                    EntryEbp: 0x3300,
                    Frame8Pointer: 0x6100,
                    PointeeDword: 0x12345678,
                    Receiver10OldValue: 4)));
        check("The clean 650E route skips publication storage and composes through return",
            cleanSkipStore.Outcome == FrameHandoffPublicationReturnOutcome.ProjectionComplete
            && !cleanSkipStore.PublicationPlan.StoreInstructionReached
            && !cleanSkipStore.PublicationPlan.StoreWriteProjected
            && cleanSkipStore.PublicationPlan.StoreWriteAddress is null
            && cleanSkipStore.PublicationPlan.ComputedEcxBeforeTest == 0x5004
            && cleanSkipStore.FinalPlan is { ReturnReached: true }
            && cleanSkipStore.FinalPlan.Value.FinalWriteValue == 0x12345678
            && !AnyUnreachedCaptures(cleanSkipStore));

        FrameHandoffPublicationReturnPlan missingFresh =
            FrameHandoffPublicationReturnProjection.Project(new(
                Publication: new(
                    FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
                    ReceiverBaseEsi: 0x1200,
                    SlotArrayBaseEbx: 0x2300,
                    IncomingEdi: 0,
                    IncomingEcx: 0x4400,
                    ReceiverPlus04At6508: 0x18,
                    ReceiverPlus04At650E: null,
                    SlotDwordAtEbxPlusFreshBase: 0x8888),
                Final: new(
                    EntryEcx: 0,
                    EntryEsi: 0,
                    EntryEbp: 0x3400,
                    Frame8Pointer: 0x6200,
                    PointeeDword: 0xABCDEF01,
                    Receiver10OldValue: 9)));
        check("A missing fresh-base capture preserves the completed 6508 store and prevents final effects",
            missingFresh.Outcome
                == FrameHandoffPublicationReturnOutcome.PublicationProjectionBlocked
            && missingFresh.PublicationPlan.Outcome
                == FrameHandoffSlotPublicationOutcome.MissingFreshBaseCapture
            && missingFresh.PublicationPlan.StoreWriteProjected
            && missingFresh.PublicationPlan.StoreWriteAddress == 0x2318
            && missingFresh.PublicationPlan.StoreWriteValue == 0x4400
            && missingFresh.FinalPlan is null
            && missingFresh.UnreachedPublicationSlotDwordCaptureProvided
            && missingFresh.UnreachedFinalFrame8PointerCaptureProvided
            && missingFresh.UnreachedFinalPointeeDwordCaptureProvided
            && missingFresh.UnreachedFinalReceiver10OldValueCaptureProvided);

        FrameHandoffPublicationReturnPlan flaggedSkipStore =
            FrameHandoffPublicationReturnProjection.Project(new(
                Publication: new(
                    FrameHandoffSlotPublicationEntry.ExistingNonzeroSlotBranchAt650E,
                    ReceiverBaseEsi: 0x1300,
                    SlotArrayBaseEbx: 0x2400,
                    IncomingEdi: 0,
                    IncomingEcx: 0x4455,
                    ReceiverPlus04At6508: 0xDEAD,
                    ReceiverPlus04At650E: 0x20,
                    SlotDwordAtEbxPlusFreshBase: 0x7000),
                Final: new(
                    EntryEcx: 0x7000,
                    EntryEsi: 0x1300,
                    EntryEbp: 0x3500,
                    Frame8Pointer: 0x6300,
                    PointeeDword: 0x10203040,
                    Receiver10OldValue: 2)));
        check("Unreached 6508 inputs on a complete 650E route block every final effect",
            flaggedSkipStore.Outcome
                == FrameHandoffPublicationReturnOutcome.PublicationHasUnreachedCaptures
            && flaggedSkipStore.PublicationPlan.Outcome
                == FrameHandoffSlotPublicationOutcome.ProjectionComplete
            && flaggedSkipStore.UnreachedPublicationStoreBaseCaptureProvided
            && flaggedSkipStore.UnreachedPublicationEcxCaptureProvided
            && flaggedSkipStore.FinalPlan is null
            && flaggedSkipStore.UnreachedFinalFrame8PointerCaptureProvided
            && flaggedSkipStore.UnreachedFinalPointeeDwordCaptureProvided
            && flaggedSkipStore.UnreachedFinalReceiver10OldValueCaptureProvided);

        FrameHandoffPublicationReturnPlan ecxMismatch =
            FrameHandoffPublicationReturnProjection.Project(new(
                Publication: StorePublication(slotDword: 0x8000),
                Final: new(
                    EntryEcx: 0x8001,
                    EntryEsi: 0x1000,
                    EntryEbp: 0x3600,
                    Frame8Pointer: 0x6400,
                    PointeeDword: 0x55667788,
                    Receiver10OldValue: 3)));
        FrameHandoffPublicationReturnPlan esiMismatch =
            FrameHandoffPublicationReturnProjection.Project(new(
                Publication: StorePublication(slotDword: 0x8000),
                Final: new(
                    EntryEcx: 0x8000,
                    EntryEsi: 0x1001,
                    EntryEbp: 0x3700,
                    Frame8Pointer: 0x6500,
                    PointeeDword: 0x66778899,
                    Receiver10OldValue: 5)));
        check("ECX and receiver-ESI mismatches stop before final reads, writes, or increment",
            ecxMismatch.Outcome
                == FrameHandoffPublicationReturnOutcome.FinalEntryEcxMismatch
            && !ecxMismatch.FinalEntryEcxMatchesPublication
            && ecxMismatch.FinalEntryEsiMatchesPublication
            && ecxMismatch.FinalPlan is null
            && ecxMismatch.UnreachedFinalFrame8PointerCaptureProvided
            && ecxMismatch.UnreachedFinalPointeeDwordCaptureProvided
            && ecxMismatch.UnreachedFinalReceiver10OldValueCaptureProvided
            && ecxMismatch.PublicationPlan.StoreWriteProjected
            && esiMismatch.Outcome
                == FrameHandoffPublicationReturnOutcome.FinalEntryEsiMismatch
            && esiMismatch.FinalEntryEcxMatchesPublication
            && !esiMismatch.FinalEntryEsiMatchesPublication
            && esiMismatch.FinalPlan is null
            && esiMismatch.UnreachedFinalFrame8PointerCaptureProvided
            && esiMismatch.UnreachedFinalPointeeDwordCaptureProvided
            && esiMismatch.UnreachedFinalReceiver10OldValueCaptureProvided);

        FrameHandoffPublicationReturnPlan missingCounter =
            FrameHandoffPublicationReturnProjection.Project(new(
                Publication: StorePublication(slotDword: 0x9000),
                Final: new(
                    EntryEcx: 0x9000,
                    EntryEsi: 0x1000,
                    EntryEbp: 0x3800,
                    Frame8Pointer: 0x6600,
                    PointeeDword: 0xAABBCCDD,
                    Receiver10OldValue: null)));
        check("A missing final counter capture preserves both earlier publication and final stores",
            missingCounter.Outcome
                == FrameHandoffPublicationReturnOutcome.FinalProjectionBlocked
            && missingCounter.PublicationPlan.StoreWriteProjected
            && missingCounter.PublicationPlan.StoreWriteAddress == 0x2010
            && missingCounter.PublicationPlan.StoreWriteValue == 0x4000
            && missingCounter.FinalPlan is
                { Outcome: FrameHandoffFinalWriteReturnOutcome.MissingReceiver10OldValueCapture }
            && missingCounter.FinalPlan.Value.FinalWriteReached
            && missingCounter.FinalPlan.Value.FinalWriteAddress == 0x9000
            && missingCounter.FinalPlan.Value.FinalWriteValue == 0xAABBCCDD
            && missingCounter.FinalPlan.Value.CounterIncrementReached
            && missingCounter.FinalPlan.Value.CounterValueAfterIncrement is null);

        FrameHandoffPublicationReturnPlan zeroEcx =
            FrameHandoffPublicationReturnProjection.Project(new(
                Publication: new(
                    FrameHandoffSlotPublicationEntry.ExistingNonzeroSlotBranchAt650E,
                    ReceiverBaseEsi: 0x1400,
                    SlotArrayBaseEbx: 0x2500,
                    IncomingEdi: 0,
                    IncomingEcx: null,
                    ReceiverPlus04At6508: null,
                    ReceiverPlus04At650E: 0x40,
                    SlotDwordAtEbxPlusFreshBase: 0),
                Final: new(
                    EntryEcx: 0,
                    EntryEsi: 0x1400,
                    EntryEbp: 0x3900,
                    Frame8Pointer: null,
                    PointeeDword: null,
                    Receiver10OldValue: 11)));
        check("Zero ECX takes the final skip-read path without inventing a return EAX value",
            zeroEcx.Outcome == FrameHandoffPublicationReturnOutcome.ProjectionComplete
            && zeroEcx.FinalPlan is
                { Outcome: FrameHandoffFinalWriteReturnOutcome.ReturnedNormally }
            && zeroEcx.FinalPlan.Value.ZeroBranchTaken == true
            && !zeroEcx.FinalPlan.Value.Frame8ReadReached
            && !zeroEcx.FinalPlan.Value.PointeeReadReached
            && !zeroEcx.FinalPlan.Value.FinalWriteReached
            && zeroEcx.FinalPlan.Value.CounterValueAfterIncrement == 12
            && zeroEcx.FinalPlan.Value.ReturnEaxValue is null);

        FrameHandoffPublicationReturnPlan wrapping =
            FrameHandoffPublicationReturnProjection.Project(new(
                Publication: new(
                    FrameHandoffSlotPublicationEntry.ExistingNonzeroSlotBranchAt650E,
                    ReceiverBaseEsi: uint.MaxValue,
                    SlotArrayBaseEbx: uint.MaxValue - 3,
                    IncomingEdi: uint.MaxValue,
                    IncomingEcx: null,
                    ReceiverPlus04At6508: null,
                    ReceiverPlus04At650E: 8,
                    SlotDwordAtEbxPlusFreshBase: uint.MaxValue),
                Final: new(
                    EntryEcx: 11,
                    EntryEsi: uint.MaxValue,
                    EntryEbp: uint.MaxValue - 3,
                    Frame8Pointer: uint.MaxValue,
                    PointeeDword: 0xAABBCCDD,
                    Receiver10OldValue: uint.MaxValue)));
        check("Both composed projections preserve DWORD-width address and counter wrapping",
            wrapping.Outcome == FrameHandoffPublicationReturnOutcome.ProjectionComplete
            && wrapping.PublicationPlan.FreshBaseReadAddress == 3
            && wrapping.PublicationPlan.SlotReadAddress == 4
            && wrapping.PublicationPlan.ComputedEcxBeforeTest == 11
            && wrapping.FinalPlan is { Outcome: FrameHandoffFinalWriteReturnOutcome.ReturnedNormally }
            && wrapping.FinalPlan.Value.Frame8ReadAddress == 4
            && wrapping.FinalPlan.Value.PointeeReadAddress == uint.MaxValue
            && wrapping.FinalPlan.Value.FinalWriteAddress == 11
            && wrapping.FinalPlan.Value.Receiver10ReadAddress == 15
            && wrapping.FinalPlan.Value.CounterValueAfterIncrement == 0);
    }

    private static FrameHandoffSlotPublicationCapture StorePublication(uint slotDword) =>
        new(
            FrameHandoffSlotPublicationEntry.PublicationStorePathAt6508,
            ReceiverBaseEsi: 0x1000,
            SlotArrayBaseEbx: 0x2000,
            IncomingEdi: 0,
            IncomingEcx: 0x4000,
            ReceiverPlus04At6508: 0x10,
            ReceiverPlus04At650E: 0x20,
            SlotDwordAtEbxPlusFreshBase: slotDword);

    private static bool AnyUnreachedCaptures(FrameHandoffPublicationReturnPlan plan) =>
        plan.UnreachedPublicationStoreBaseCaptureProvided
        || plan.UnreachedPublicationEcxCaptureProvided
        || plan.UnreachedPublicationFreshBaseCaptureProvided
        || plan.UnreachedPublicationSlotDwordCaptureProvided
        || plan.UnreachedFinalFrame8PointerCaptureProvided
        || plan.UnreachedFinalPointeeDwordCaptureProvided
        || plan.UnreachedFinalReceiver10OldValueCaptureProvided;
}
