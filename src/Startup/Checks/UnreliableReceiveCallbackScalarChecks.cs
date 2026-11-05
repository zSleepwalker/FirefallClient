using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveCallbackScalarChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample, raw-to-VA mapping, exact bytes, call target, and boundaries match",
            UnreliableReceiveCallbackScalarProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveCallbackScalarProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveCallbackScalarProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveCallbackScalarProjection.EntryRawFileOffset == 0x00BE5B25
            && UnreliableReceiveCallbackScalarProjection.EntryVirtualAddress == 0x00FE6725
            && UnreliableReceiveCallbackScalarProjection.RawFileOffsetToVirtualAddress(0x00BE5B25)
                == 0x00FE6725
            && UnreliableReceiveCallbackScalarProjection.RawFileOffsetToVirtualAddress(0x00BE5B32)
                == 0x00FE6732
            && UnreliableReceiveCallbackScalarProjection.PriorCallbackEaxCopyRawFileOffset
                == 0x00BE5B28
            && UnreliableReceiveCallbackScalarProjection.HelperCallRawFileOffset == 0x00BE5B2A
            && UnreliableReceiveCallbackScalarProjection.HelperCallTargetAddressFromIda
                == 0x00BD58E0
            && UnreliableReceiveCallbackScalarProjection.HelperCallContinuationRawFileOffset
                == 0x00BE5B2F
            && UnreliableReceiveCallbackScalarProjection.EndRawFileOffsetExclusive
                == 0x00BE5B32
            && UnreliableReceiveCallbackScalarProjection.ExplicitPushCount == 0
            && UnreliableReceiveCallbackScalarProjection.StackCleanupAmount == 4
            && UnreliableReceiveCallbackScalarProjection.CapturedWindowBytes
                == "8A 4D 08 8B D0 E8 B1 FD FE FF 83 C4 04"
            && UnreliableReceiveCallbackScalarProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 13
            && InstructionsMatch());

        UnreliableReceiveCallbackScalarPlan normal =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0x1000, true, 0xAABBCCDD, 0x5A, 0x12345678, true));
        check("The frame byte replaces only CL while the full prior EAX value reaches EDX",
            normal.Outcome == UnreliableReceiveCallbackScalarOutcome.HelperReturnedNormally
            && normal.PriorIndirectCallReturnedNormally == true
            && normal.FrameByteRead
            && normal.FrameByteAddress == 0x1008
            && normal.CapturedCl == 0x5A
            && normal.CapturedEdx == 0x12345678
            && normal.ProjectedEcx == 0xAABBCC5A
            && normal.HelperCallReached
            && normal.HelperReturnedNormally == true
            && normal.NormalCallBoundaryReached
            && normal.StackCleanupBytes == 4
            && normal.SelectedDestinationRawFileOffset == 0x00BE5B32
            && normal.StopAtRawFileOffsetExclusive == 0x00BE5B32);

        UnreliableReceiveCallbackScalarPlan unknownEcxUpper =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0x1000, true, null, 0xA7, 0, true));
        check("Missing entry ECX upper bits leave projected ECX unknown without blocking the helper",
            unknownEcxUpper.Outcome == UnreliableReceiveCallbackScalarOutcome.HelperReturnedNormally
            && unknownEcxUpper.CapturedCl == 0xA7
            && unknownEcxUpper.CapturedEdx == 0
            && unknownEcxUpper.ProjectedEcx is null
            && unknownEcxUpper.HelperCallReached
            && unknownEcxUpper.StackCleanupBytes == 4);

        UnreliableReceiveCallbackScalarPlan zeroFrameByte =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0x1000, true, 0x11223344, 0, 7, true));
        check("A captured zero frame byte is a legitimate CL value",
            zeroFrameByte.Outcome == UnreliableReceiveCallbackScalarOutcome.HelperReturnedNormally
            && zeroFrameByte.FrameByteRead
            && zeroFrameByte.CapturedCl == 0
            && zeroFrameByte.ProjectedEcx == 0x11223300);

        UnreliableReceiveCallbackScalarPlan wrappedAddress =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0xFFFFFFF8, true, 0xAABBCC00, 0x7F, 0, true));
        check("The EBP+8 byte address wraps as an unchecked 32-bit address",
            UnreliableReceiveCallbackScalarProjection.FrameByteAddress(0xFFFFFFF8) == 0
            && wrappedAddress.FrameByteRead
            && wrappedAddress.FrameByteAddress == 0
            && wrappedAddress.ProjectedEcx == 0xAABBCC7F);

        UnreliableReceiveCallbackScalarPlan missingFrameByte =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0x1000, true, 0xAABBCCDD, null, null, null));
        UnreliableReceiveCallbackScalarPlan missingFrameByteWithLaterCapture =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0x1000, true, null, null, 0, true));
        check("A missing frame read stops before EAX copy and rejects later captures as unreached",
            missingFrameByte.Outcome == UnreliableReceiveCallbackScalarOutcome.MissingFrameByteAt8
            && !missingFrameByte.FrameByteRead
            && missingFrameByte.FrameByteAddress == 0x1008
            && missingFrameByte.CapturedCl is null
            && missingFrameByte.CapturedEdx is null
            && missingFrameByte.StopAtRawFileOffsetExclusive == 0x00BE5B25
            && missingFrameByteWithLaterCapture.Outcome
                == UnreliableReceiveCallbackScalarOutcome.UnreachedCaptureProvided
            && missingFrameByteWithLaterCapture.UnreachedPriorCallbackEaxCaptureProvided
            && missingFrameByteWithLaterCapture.UnreachedHelperReturnCaptureProvided
            && !missingFrameByteWithLaterCapture.FrameByteRead
            && missingFrameByteWithLaterCapture.CapturedCl is null);

        UnreliableReceiveCallbackScalarPlan missingEax =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0x1000, true, 0xAABBCCDD, 0x6C, null, null));
        UnreliableReceiveCallbackScalarPlan missingEaxWithLaterCapture =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0x1000, true, null, 0x6C, null, true));
        check("The frame byte remains known in CL when the prior EAX copy capture is missing",
            missingEax.Outcome == UnreliableReceiveCallbackScalarOutcome.MissingPriorCallbackEax
            && missingEax.FrameByteRead
            && missingEax.CapturedCl == 0x6C
            && missingEax.ProjectedEcx == 0xAABBCC6C
            && missingEax.CapturedEdx is null
            && !missingEax.HelperCallReached
            && missingEax.StopAtRawFileOffsetExclusive == 0x00BE5B28
            && missingEaxWithLaterCapture.Outcome
                == UnreliableReceiveCallbackScalarOutcome.UnreachedCaptureProvided
            && missingEaxWithLaterCapture.CapturedCl == 0x6C
            && missingEaxWithLaterCapture.CapturedEdx is null
            && missingEaxWithLaterCapture.UnreachedHelperReturnCaptureProvided);

        UnreliableReceiveCallbackScalarPlan missingPriorGate =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0x1000, null, 0xAABBCCDD, 0, 0, true));
        UnreliableReceiveCallbackScalarPlan priorDidNotReturn =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0x1000, false, null, null, null, null));
        UnreliableReceiveCallbackScalarPlan unreachedAfterPrior =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0x1000, false, null, 0, 0, true));
        check("The prior indirect call normal-return gate blocks every later read and call",
            missingPriorGate.Outcome == UnreliableReceiveCallbackScalarOutcome.UnreachedCaptureProvided
            && !missingPriorGate.FrameByteRead
            && missingPriorGate.CapturedCl is null
            && missingPriorGate.CapturedEdx is null
            && !missingPriorGate.HelperCallReached
            && missingPriorGate.StopAtRawFileOffsetExclusive == 0x00BE5B25
            && priorDidNotReturn.Outcome
                == UnreliableReceiveCallbackScalarOutcome.PriorIndirectCallDidNotReturnNormally
            && !priorDidNotReturn.FrameByteRead
            && !priorDidNotReturn.HelperCallReached
            && unreachedAfterPrior.Outcome
                == UnreliableReceiveCallbackScalarOutcome.UnreachedCaptureProvided
            && unreachedAfterPrior.UnreachedFrameByteCaptureProvided
            && unreachedAfterPrior.UnreachedPriorCallbackEaxCaptureProvided
            && unreachedAfterPrior.UnreachedHelperReturnCaptureProvided
            && !unreachedAfterPrior.FrameByteRead);

        UnreliableReceiveCallbackScalarPlan missingHelperReturn =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0x1000, true, null, 0x22, 1, null));
        UnreliableReceiveCallbackScalarPlan helperDidNotReturn =
            UnreliableReceiveCallbackScalarProjection.Project(
                new(0x1000, true, null, 0x22, 1, false));
        check("Helper status gates the cleanup boundary without inferring helper semantics",
            missingHelperReturn.Outcome
                == UnreliableReceiveCallbackScalarOutcome.MissingHelperReturn
            && missingHelperReturn.HelperCallReached
            && missingHelperReturn.NormalCallBoundaryReached == false
            && missingHelperReturn.StackCleanupBytes is null
            && missingHelperReturn.StopAtRawFileOffsetExclusive == 0x00BE5B2F
            && helperDidNotReturn.Outcome
                == UnreliableReceiveCallbackScalarOutcome.HelperDidNotReturnNormally
            && helperDidNotReturn.HelperCallReached
            && helperDidNotReturn.HelperReturnedNormally == false
            && !helperDidNotReturn.NormalCallBoundaryReached
            && helperDidNotReturn.StackCleanupBytes is null
            && helperDidNotReturn.SelectedDestinationRawFileOffset is null);
    }

    private static bool InstructionsMatch()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes)[]
        {
            (0x00BE5B25, 0x00FE6725, "8A 4D 08"),
            (0x00BE5B28, 0x00FE6728, "8B D0"),
            (0x00BE5B2A, 0x00FE672A, "E8 B1 FD FE FF"),
            (0x00BE5B2F, 0x00FE672F, "83 C4 04")
        };
        return UnreliableReceiveCallbackScalarProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
                .SequenceEqual(expected)
            && UnreliableReceiveCallbackScalarProjection.RawFileOffsetToVirtualAddress(
                0x00BE5B28) == 0x00FE6728
            && UnreliableReceiveCallbackScalarProjection.RawFileOffsetToVirtualAddress(
                0x00BE5B2F) == 0x00FE672F
            && UnreliableReceiveCallbackScalarProjection.ReplaceLowByte(
                0xAABBCCDD, 0x5A) == 0xAABBCC5A;
    }
}
