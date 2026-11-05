using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveLengthPublicationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample and complete contiguous raw window match the composed stages",
            ReliableReceiveLengthPublicationProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ReliableReceiveLengthPublicationProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveLengthPublicationProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReliableReceiveLengthPublicationProjection.EntryRawFileOffset == 0x00BE53BF
            && ReliableReceiveLengthPublicationProjection.EntryVirtualAddress == 0x00FE5FBF
            && ReliableReceiveLengthPublicationProjection.EndRawFileOffsetExclusive
                == 0x00BE53F7
            && ReliableReceiveLengthPublicationProjection.EndVirtualAddressExclusive
                == 0x00FE5FF7
            && ReliableReceiveLengthPublicationProjection.RawFileOffsetToVirtualAddress(
                0x00BE53F7) == 0x00FE5FF7
            && ReliableReceiveLengthPublicationProjection.CapturedWindowBytes
                == "66 8B 07 B9 FF 07 00 00 66 23 C1 8D 4F 08 0F B7 C0 "
                    + "83 E8 04 50 8B 47 08 FF 77 04 FF 50 04 "
                    + "8B D0 8A CB E8 FA 04 FF FF 83 C4 04 "
                    + "8B 45 08 89 38 B0 01 5F 5E 5B 5D C2 04 00"
            && ReliableReceiveLengthPublicationProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 56
            && InstructionsMatch());

        ReliableReceiveLengthPublicationCapture normalCapture = new(
            new(true, 0x10203040, 0x1004, 0x11111111, 0x22222222, 0x33333333, true),
            new(true, 0xA1B2C3D4, 0x55667700, 0x7A, true),
            0x10203040,
            true,
            0xABCDEF00,
            0x10203040,
            new ReliableReceiveSlotPublicationCapture(0));
        ReliableReceiveLengthPublicationPlan normal =
            ReliableReceiveLengthPublicationProjection.Project(normalCapture);
        check("All three boundaries compose while callback EAX, selector BL, and publication reload stay independent",
            normal.Outcome
                == ReliableReceiveLengthPublicationOutcome.PublicationSequenceProjected
            && normal.LengthCallbackPlan?.CapturedIndirectTarget == 0x33333333
            && normal.LengthCallbackPlan?.ComputedLength == 0
            && normal.ScalarHandoffPlan?.CapturedEdx == 0xA1B2C3D4
            && normal.ScalarHandoffPlan?.CapturedEdx
                != normal.LengthCallbackPlan?.CapturedIndirectTarget
            && normal.ScalarHandoffPlan?.CapturedCl == 0x7A
            && normal.ScalarHandoffPlan?.CapturedCl
                != (byte)(normal.LengthCallbackPlan?.MaskedHeader ?? 0)
            && normal.ScalarHandoffPlan?.StackCleanupBytes == 4
            && normal.SlotPublicationPlan?.FrameArgument8Address == 0xABCDEF08
            && normal.SlotPublicationPlan?.CapturedPointer == 0
            && normal.SlotPublicationPlan?.PayloadDwordWriteAddress == 0
            && normal.SlotPublicationPlan?.PayloadDwordWriteValue == 0x10203040
            && normal.SlotPublicationPlan?.AlOneEffectProjected == true
            && normal.SlotPublicationPlan?.ReturnStackBytes == 4
            && normal.StopAtRawFileOffsetExclusive == 0x00BE53F7);

        ReliableReceiveLengthPublicationPlan missingLength =
            ReliableReceiveLengthPublicationProjection.Project(new(
                null,
                new(true, 0, 0, 0, true),
                0,
                true,
                0,
                0,
                new ReliableReceiveSlotPublicationCapture(0)));
        ReliableReceiveLengthPublicationPlan missingHeader =
            ReliableReceiveLengthPublicationProjection.Project(new(
                new(true, 0x10203040, null, null, null, null, null),
                new(true, 0, 0, 0, true),
                0x10203040,
                true,
                0,
                0x10203040,
                new ReliableReceiveSlotPublicationCapture(0)));
        check("Missing or unreached initial captures preserve gates and do not project later-stage effects",
            missingLength.Outcome
                == ReliableReceiveLengthPublicationOutcome.UnreachedLaterCaptureProvided
            && missingLength.LengthCallbackPlan is null
            && missingLength.UnreachedScalarHandoffCaptureProvided
            && missingLength.UnreachedSlotPublicationCaptureProvided
            && missingLength.StopAtRawFileOffsetExclusive == 0x00BE53BF
            && missingHeader.Outcome
                == ReliableReceiveLengthPublicationOutcome.LengthCallbackNotAdmitted
            && missingHeader.LengthCallbackPlan?.Outcome
                == ReliableReceiveLengthCallbackOutcome.MissingPayloadHeader
            && missingHeader.ScalarHandoffPlan is null
            && missingHeader.UnreachedScalarHandoffCaptureProvided
            && missingHeader.SlotPublicationPlan is null);

        ReliableReceiveLengthPublicationPlan nonReturningLength =
            ReliableReceiveLengthPublicationProjection.Project(new(
                new(true, 0x10203040, 0x1004, 0, 0, 0xABCDEF00, false),
                new(true, 0x55555555, 0, 0x7A, true),
                0x10203040,
                true,
                0xABCDEF00,
                0x10203040,
                new ReliableReceiveSlotPublicationCapture(0)));
        ReliableReceiveLengthPublicationPlan missingScalarReturn =
            ReliableReceiveLengthPublicationProjection.Project(new(
                new(true, 0x10203040, 0x1004, 0, 0, 0, true),
                new(null, 0x55555555, 0, 0x7A, true),
                0x10203040,
                true,
                0xABCDEF00,
                0x10203040,
                new ReliableReceiveSlotPublicationCapture(0)));
        ReliableReceiveLengthPublicationPlan scalarPayloadMismatch =
            ReliableReceiveLengthPublicationProjection.Project(new(
                new(true, 0x10203040, 0x1004, 0, 0, 0, true),
                new(true, 0x55555555, 0, 0x7A, true),
                0x10203041,
                true,
                0xABCDEF00,
                0x10203040,
                new ReliableReceiveSlotPublicationCapture(0)));
        check("Scalar handoff requires the length return and true prerequisite, with payload identity checked before projection",
            nonReturningLength.Outcome
                == ReliableReceiveLengthPublicationOutcome.LengthCallbackNotAdmitted
            && nonReturningLength.LengthCallbackPlan?.Outcome
                == ReliableReceiveLengthCallbackOutcome.IndirectCallDidNotReturnNormally
            && nonReturningLength.ScalarHandoffPlan is null
            && missingScalarReturn.Outcome
                == ReliableReceiveLengthPublicationOutcome.ScalarHandoffNotAdmitted
            && missingScalarReturn.ScalarHandoffPlan?.Outcome
                == ReliableReceiveCallbackScalarHandoffOutcome.UnreachedCaptureProvided
            && missingScalarReturn.ScalarHandoffPlan?.CapturedEdx is null
            && scalarPayloadMismatch.Outcome
                == ReliableReceiveLengthPublicationOutcome.ScalarPayloadMismatch
            && scalarPayloadMismatch.ScalarHandoffPlan is null
            && scalarPayloadMismatch.SlotPublicationPlan is null);

        ReliableReceiveLengthPublicationPlan missingHelperReturn =
            ReliableReceiveLengthPublicationProjection.Project(new(
                new(true, 0x10203040, 0x1004, 0, 0, 0, true),
                new(true, 0x55555555, null, 0x7A, null),
                0x10203040,
                true,
                0xABCDEF00,
                0x10203040,
                new ReliableReceiveSlotPublicationCapture(0)));
        ReliableReceiveLengthPublicationPlan missingPublicationValidation =
            ReliableReceiveLengthPublicationProjection.Project(new(
                new(true, 0x10203040, 0x1004, 0, 0, 0, true),
                new(true, 0x55555555, 0, 0x7A, true),
                0x10203040,
                null,
                0xABCDEF00,
                0x10203040,
                new ReliableReceiveSlotPublicationCapture(0)));
        ReliableReceiveLengthPublicationPlan missingFrameDword =
            ReliableReceiveLengthPublicationProjection.Project(new(
                new(true, 0x10203040, 0x1004, 0, 0, 0, true),
                new(true, 0x55555555, 0, 0x7A, true),
                0x10203040,
                true,
                0xABCDEF00,
                0x10203040,
                new ReliableReceiveSlotPublicationCapture(null)));
        check("Incomplete handoffs preserve cleanup and publication plans without claiming later effects",
            missingHelperReturn.Outcome
                == ReliableReceiveLengthPublicationOutcome.ScalarHandoffNotAdmitted
            && missingHelperReturn.ScalarHandoffPlan?.HelperCallReached == true
            && missingHelperReturn.ScalarHandoffPlan?.StackCleanupBytes is null
            && missingHelperReturn.SlotPublicationPlan is null
            && missingPublicationValidation.Outcome
                == ReliableReceiveLengthPublicationOutcome.MissingPublicationEntryValidation
            && missingPublicationValidation.ScalarHandoffPlan?.StackCleanupBytes == 4
            && missingPublicationValidation.SlotPublicationPlan is null
            && missingFrameDword.Outcome
                == ReliableReceiveLengthPublicationOutcome.SlotPublicationNotAdmitted
            && missingFrameDword.SlotPublicationPlan?.Outcome
                == ReliableReceiveSlotPublicationOutcome.MissingFrameArgument8Capture
            && missingFrameDword.SlotPublicationPlan?.PayloadDwordWriteProjected == false
            && missingFrameDword.SlotPublicationPlan?.AlOneEffectProjected == false
            && missingFrameDword.SlotPublicationPlan?.ReturnStackBytes is null);

        ReliableReceiveLengthPublicationPlan invalidPublicationEntry =
            ReliableReceiveLengthPublicationProjection.Project(new(
                new(true, 0x10203040, 0x1004, 0, 0, 0, true),
                new(true, 0x55555555, 0, 0x7A, true),
                0x10203040,
                false,
                0xABCDEF00,
                0x10203040,
                new ReliableReceiveSlotPublicationCapture(0)));
        ReliableReceiveLengthPublicationPlan publicationPayloadMismatch =
            ReliableReceiveLengthPublicationProjection.Project(new(
                new(true, 0x10203040, 0x1004, 0, 0, 0, true),
                new(true, 0x55555555, 0, 0x7A, true),
                0x10203040,
                true,
                0xABCDEF00,
                0x10203041,
                new ReliableReceiveSlotPublicationCapture(0)));
        check("Publication requires a validated entry and original payload identity before applying any effects",
            invalidPublicationEntry.Outcome
                == ReliableReceiveLengthPublicationOutcome.PublicationEntryNotValidated
            && invalidPublicationEntry.ScalarHandoffPlan?.StackCleanupBytes == 4
            && invalidPublicationEntry.SlotPublicationPlan is null
            && publicationPayloadMismatch.Outcome
                == ReliableReceiveLengthPublicationOutcome.PublicationPayloadMismatch
            && publicationPayloadMismatch.SlotPublicationPlan is null
            && publicationPayloadMismatch.StopAtRawFileOffsetExclusive
                == ReliableReceiveSlotPublicationProjection.PayloadDwordWriteRawFileOffset);
    }

    private static bool InstructionsMatch()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes)[]
        {
            (0x00BE53BF, 0x00FE5FBF, "66 8B 07"),
            (0x00BE53C2, 0x00FE5FC2, "B9 FF 07 00 00"),
            (0x00BE53C7, 0x00FE5FC7, "66 23 C1"),
            (0x00BE53CA, 0x00FE5FCA, "8D 4F 08"),
            (0x00BE53CD, 0x00FE5FCD, "0F B7 C0"),
            (0x00BE53D0, 0x00FE5FD0, "83 E8 04"),
            (0x00BE53D3, 0x00FE5FD3, "50"),
            (0x00BE53D4, 0x00FE5FD4, "8B 47 08"),
            (0x00BE53D7, 0x00FE5FD7, "FF 77 04"),
            (0x00BE53DA, 0x00FE5FDA, "FF 50 04"),
            (0x00BE53DD, 0x00FE5FDD, "8B D0"),
            (0x00BE53DF, 0x00FE5FDF, "8A CB"),
            (0x00BE53E1, 0x00FE5FE1, "E8 FA 04 FF FF"),
            (0x00BE53E6, 0x00FE5FE6, "83 C4 04"),
            (0x00BE53E9, 0x00FE5FE9, "8B 45 08"),
            (0x00BE53EC, 0x00FE5FEC, "89 38"),
            (0x00BE53EE, 0x00FE5FEE, "B0 01"),
            (0x00BE53F0, 0x00FE5FF0, "5F 5E 5B 5D"),
            (0x00BE53F4, 0x00FE5FF4, "C2 04 00")
        };
        return ReliableReceiveLengthPublicationProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
                .SequenceEqual(expected)
            && ReliableReceiveLengthCallbackProjection.EndRawFileOffsetExclusive
                == ReliableReceiveCallbackScalarHandoffProjection.EntryRawFileOffset
            && ReliableReceiveCallbackScalarHandoffProjection.EndRawFileOffsetExclusive
                == ReliableReceiveSlotPublicationProjection.PublicationStartRawFileOffset
            && ReliableReceiveSlotPublicationProjection.PublicationEndRawFileOffsetExclusive
                == ReliableReceiveLengthPublicationProjection.EndRawFileOffsetExclusive;
    }
}
