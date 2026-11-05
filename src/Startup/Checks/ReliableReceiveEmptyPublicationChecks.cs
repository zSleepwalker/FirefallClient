using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveEmptyPublicationChecks
{
    private const uint ReceiverBase = 0x10203040;
    private const uint FrameBase = 0x50607080;
    private const uint PayloadEdi = 0xD0C0B0A0;

    public static void Run(Action<string, bool> check)
    {
        ReliableReceiveEmptyPublicationPlan independentReload = Project(
            callbackCapture: HeaderBranchCapture(),
            publicationCapture: new ReliableReceiveSlotPublicationCapture(0x76543210));
        ReliableReceiveSlotPublicationPlan? publication = independentReload.PublicationPlan;
        check("Only a clean zero-header branch reaches publication, reloading frame+8 independently",
            independentReload.Outcome
                == ReliableReceiveEmptyPublicationOutcome.PublicationFrameArgument8CaptureAvailable
            && independentReload.PublicationFrameArgument8CaptureAvailable
            && independentReload.PublicationProjectionReached
            && independentReload.CallbackPlan?.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.HeaderBranchTaken
            && independentReload.CallbackPlan?.SelectedDestinationRawFileOffset
                == ReliableReceiveEmptySlotCallbackProjection.HeaderBranchTargetRawFileOffset
            && independentReload.CallbackPlan?.Capture.PayloadEdi == PayloadEdi
            && publication?.FrameEbp == FrameBase
            && publication?.PayloadEdi == PayloadEdi
            && publication?.FrameArgument8Address
                == ReliableReceiveSlotPublicationProjection.FrameArgument8Address(FrameBase)
            && publication?.CapturedPointer == 0x76543210
            && publication?.PayloadDwordWriteAddress == 0x76543210
            && publication?.PayloadDwordWriteValue == PayloadEdi
            && independentReload.SlotGatePlan.FrameArgumentValue
                != publication?.FrameArgument8Address);

        ReliableReceiveEmptyPublicationPlan missingPublicationCapture = Project(
            callbackCapture: HeaderBranchCapture(),
            publicationCapture: null,
            useDefaultPublicationCapture: false);
        ReliableReceiveEmptyPublicationPlan missingCallbackCapture = Project(
            callbackCapture: null,
            useDefaultCallbackCapture: false);
        ReliableReceiveEmptyPublicationPlan missingFramePointer = Project(
            callbackCapture: HeaderBranchCapture(),
            publicationCapture: new ReliableReceiveSlotPublicationCapture(null));
        check("Publication requires its own frame+8 capture and stops at that reload when missing",
            missingPublicationCapture.Outcome
                == ReliableReceiveEmptyPublicationOutcome.MissingPublicationCapture
            && missingPublicationCapture.CallbackPlan?.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.HeaderBranchTaken
            && missingPublicationCapture.PublicationPlan is null
            && missingPublicationCapture.StopAtRawFileOffsetExclusive
                == ReliableReceiveSlotPublicationProjection.FrameArgument8ReadRawFileOffset
            && missingCallbackCapture.Outcome
                == ReliableReceiveEmptyPublicationOutcome.MissingCallbackCapture
            && missingCallbackCapture.CallbackPlan is null
            && missingCallbackCapture.PublicationPlan is null
            && missingCallbackCapture.UnreachedPublicationCaptureProvided
            && missingFramePointer.Outcome
                == ReliableReceiveEmptyPublicationOutcome.PublicationFrameArgument8CaptureMissing
            && missingFramePointer.PublicationPlan?.Outcome
                == ReliableReceiveSlotPublicationOutcome.MissingFrameArgument8Capture
            && missingFramePointer.PublicationPlan?.FrameEbp == FrameBase
            && !missingFramePointer.PublicationFrameArgument8CaptureAvailable
            && missingFramePointer.StopAtRawFileOffsetExclusive
                == ReliableReceiveSlotPublicationProjection.FrameArgument8ReadRawFileOffset);

        ReliableReceiveEmptyPublicationPlan nonzeroSlot = Project(
            slotDword: 1,
            callbackCapture: HeaderBranchCapture(),
            publicationCapture: new ReliableReceiveSlotPublicationCapture(0x11111111));
        ReliableReceiveEmptyPublicationPlan missingSlot = Project(
            slotDword: null,
            callbackCapture: HeaderBranchCapture(),
            publicationCapture: new ReliableReceiveSlotPublicationCapture(0x11111111));
        ReliableReceiveEmptyPublicationPlan invalidInput =
            ReliableReceiveEmptyPublicationProjection.Project(
                default,
                new ReliableReceiveSlotGateCapture(0, 0),
                PayloadEdi,
                HeaderBranchCapture(),
                new ReliableReceiveSlotPublicationCapture(0x11111111));
        check("Nonzero, missing, or invalid slot gates reject both later captures",
            nonzeroSlot.Outcome
                == ReliableReceiveEmptyPublicationOutcome.SlotGateDidNotSelectZeroBranch
            && nonzeroSlot.SlotGatePlan.Outcome
                == ReliableReceiveSlotGateOutcome.NonzeroSlotFallthroughSelected
            && nonzeroSlot.CallbackPlan is null
            && nonzeroSlot.PublicationPlan is null
            && nonzeroSlot.UnreachedCallbackCaptureProvided
            && nonzeroSlot.UnreachedPublicationCaptureProvided
            && nonzeroSlot.StopAtRawFileOffsetExclusive
                == ReliableReceiveSlotGateProjection.NonzeroFallthroughRawFileOffset
            && missingSlot.SlotGatePlan.Outcome
                == ReliableReceiveSlotGateOutcome.MissingSlotDwordCapture
            && missingSlot.CallbackPlan is null
            && missingSlot.PublicationPlan is null
            && missingSlot.UnreachedCallbackCaptureProvided
            && missingSlot.UnreachedPublicationCaptureProvided
            && invalidInput.SlotGatePlan.Outcome
                == ReliableReceiveSlotGateOutcome.InvalidOrUnvalidatedDelta
            && invalidInput.CallbackPlan is null
            && invalidInput.PublicationPlan is null
            && invalidInput.UnreachedCallbackCaptureProvided
            && invalidInput.UnreachedPublicationCaptureProvided);

        ReliableReceiveEmptyPublicationPlan payloadMismatch =
            ReliableReceiveEmptyPublicationProjection.Project(
                CreateInput(),
                new ReliableReceiveSlotGateCapture(0, 0),
                PayloadEdi,
                HeaderBranchCapture() with { PayloadEdi = PayloadEdi + 1 },
                new ReliableReceiveSlotPublicationCapture(0x11111111));
        check("The callback must carry the requested payload EDI before either later stage is accepted",
            payloadMismatch.Outcome
                == ReliableReceiveEmptyPublicationOutcome.CallbackPayloadEdiMismatch
            && payloadMismatch.CallbackPayloadEdi == PayloadEdi + 1
            && payloadMismatch.CallbackPlan is null
            && payloadMismatch.PublicationPlan is null
            && payloadMismatch.UnreachedPublicationCaptureProvided
            && payloadMismatch.StopAtRawFileOffsetExclusive
                == ReliableReceiveEmptySlotCallbackProjection.EntryRawFileOffset);

        ReliableReceiveEmptyPublicationPlan callbackLaterCapture = Project(
            callbackCapture: HeaderBranchCapture() with
            {
                PayloadDwordAt8 = 0,
                IndirectTargetAtDword8PlusC = 0,
                IndirectCallReturnedNormally = true
            },
            publicationCapture: new ReliableReceiveSlotPublicationCapture(0));
        check("An unreached callback capture cannot authorize publication even when its destination is the header target",
            callbackLaterCapture.CallbackPlan?.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.UnreachedCaptureProvided
            && callbackLaterCapture.CallbackPlan?.SelectedDestinationRawFileOffset
                == ReliableReceiveEmptySlotCallbackProjection.HeaderBranchTargetRawFileOffset
            && callbackLaterCapture.Outcome
                == ReliableReceiveEmptyPublicationOutcome.CallbackUnreachedCaptureProvided
            && callbackLaterCapture.PublicationPlan is null
            && callbackLaterCapture.UnreachedPublicationCaptureProvided);

        ReliableReceiveEmptyPublicationPlan normalCallbackReturn = Project(
            callbackCapture: new ReliableReceiveEmptySlotCallbackCapture(
                PayloadEdi,
                0x1000,
                0,
                0,
                true),
            publicationCapture: new ReliableReceiveSlotPublicationCapture(0));
        check("A completed nonzero-header callback stops at the result boundary, outside publication",
            normalCallbackReturn.CallbackPlan?.Outcome
                == ReliableReceiveEmptySlotCallbackOutcome.IndirectCallReturnedNormally
            && normalCallbackReturn.CallbackPlan?.StopAtRawFileOffsetExclusive
                == 0x00BE53A2
            && normalCallbackReturn.Outcome
                == ReliableReceiveEmptyPublicationOutcome.CallbackReturnedNormallyAtResultBoundary
            && normalCallbackReturn.PublicationPlan is null
            && normalCallbackReturn.UnreachedPublicationCaptureProvided
            && normalCallbackReturn.StopAtRawFileOffsetExclusive == 0x00BE53A2);

        ReliableReceiveEmptyPublicationPlan nativeZeroPointer = Project(
            callbackCapture: HeaderBranchCapture(),
            publicationCapture: new ReliableReceiveSlotPublicationCapture(0));
        ReliableReceiveEmptyPublicationPlan upperEax = Project(
            callbackCapture: HeaderBranchCapture(),
            publicationCapture: new ReliableReceiveSlotPublicationCapture(0xFEDCBA98));
        check("Captured native zero remains a projected pointer and AL=1 preserves the reloaded EAX upper bytes",
            nativeZeroPointer.PublicationFrameArgument8CaptureAvailable
            && nativeZeroPointer.PublicationPlan?.CapturedPointer == 0
            && nativeZeroPointer.PublicationPlan?.PayloadDwordWriteProjected == true
            && nativeZeroPointer.PublicationPlan?.PayloadDwordWriteAddress == 0
            && nativeZeroPointer.PublicationPlan?.PayloadDwordWriteValue == PayloadEdi
            && upperEax.PublicationPlan?.CapturedPointer == 0xFEDCBA98
            && upperEax.PublicationPlan?.ProjectedEaxAfterAlOne == 0xFEDCBA01
            && upperEax.PublicationPlan?.AlOneEffectProjected == true);
    }

    private static ReliableReceiveEmptyPublicationPlan Project(
        uint? slotDword = 0,
        ReliableReceiveEmptySlotCallbackCapture? callbackCapture = null,
        ReliableReceiveSlotPublicationCapture? publicationCapture = null,
        bool useDefaultCallbackCapture = true,
        bool useDefaultPublicationCapture = true) =>
        ReliableReceiveEmptyPublicationProjection.Project(
            CreateInput(),
            new ReliableReceiveSlotGateCapture(0, slotDword),
            PayloadEdi,
            useDefaultCallbackCapture ? callbackCapture ?? HeaderBranchCapture() : callbackCapture,
            useDefaultPublicationCapture
                ? publicationCapture ?? new ReliableReceiveSlotPublicationCapture(0x76543210)
                : publicationCapture);

    private static ReliableReceiveSlotGateValidatedInput CreateInput()
    {
        if (!ReliableReceiveSlotGateValidatedInput.TryCreate(
                ReceiverBase,
                FrameBase,
                0,
                out ReliableReceiveSlotGateValidatedInput input))
        {
            throw new InvalidOperationException("The check fixture must satisfy the slot-gate precondition.");
        }

        return input;
    }

    private static ReliableReceiveEmptySlotCallbackCapture HeaderBranchCapture() =>
        new(PayloadEdi, 0x0FFF, null, null, null);
}
