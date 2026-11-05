using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveLengthCallbackChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample, raw bytes, code VA mapping, and normal-call boundary are exact",
            ReliableReceiveLengthCallbackProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ReliableReceiveLengthCallbackProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveLengthCallbackProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReliableReceiveLengthCallbackProjection.EntryRawFileOffset == 0x00BE53BF
            && ReliableReceiveLengthCallbackProjection.EntryVirtualAddress == 0x00FE5FBF
            && ReliableReceiveLengthCallbackProjection.NormalCallBoundaryRawFileOffset
                == 0x00BE53DD
            && ReliableReceiveLengthCallbackProjection.NormalCallBoundaryVirtualAddress
                == 0x00FE5FDD
            && ReliableReceiveLengthCallbackProjection.RawFileOffsetToVirtualAddress(0x00BE53DD)
                == 0x00FE5FDD
            && ReliableReceiveLengthCallbackProjection.CapturedCallbackBytes
                == "66 8B 07 B9 FF 07 00 00 66 23 C1 8D 4F 08 0F B7 C0 "
                    + "83 E8 04 50 8B 47 08 FF 77 04 FF 50 04"
            && ReliableReceiveLengthCallbackProjection.CapturedCallbackBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 30
            && InstructionEvidenceMatches());

        check("Every WORD header from 0 through 0xFFF follows the masked, zero-extended, wrapping length oracle",
            Enumerable.Range(0, 0x1000).All(value =>
            {
                ushort header = (ushort)value;
                uint expectedLength = unchecked((uint)(header & 0x07FF) - 4u);
                ReliableReceiveLengthCallbackPlan plan =
                    ReliableReceiveLengthCallbackProjection.Project(
                        new(true, 0x10203040, header, 0, 0, 0, true));
                return plan.MaskedHeader == (ushort)(header & 0x07FF)
                    && plan.ComputedLength == expectedLength
                    && plan.EcxArgumentAddress == 0x10203048
                    && plan.Outcome
                        == ReliableReceiveLengthCallbackOutcome.IndirectCallReturnedNormally
                    && plan.NormalCallBoundaryReached;
            })
            && ReliableReceiveLengthCallbackProjection.ComputeLength(0) == 0xFFFFFFFC);

        ReliableReceiveLengthCallbackPlan entryGateNotMet =
            ReliableReceiveLengthCallbackProjection.Project(
                new(false, 0x10203040, 0x1234, 0x55667788, 0x11223344, 0x99AABBCC, true));
        ReliableReceiveLengthCallbackPlan missingHeader =
            ReliableReceiveLengthCallbackProjection.Project(
                new(true, 0x10203040, null, null, null, null, null));
        ReliableReceiveLengthCallbackPlan headerWithUnreachedLaterCaptures =
            ReliableReceiveLengthCallbackProjection.Project(
                new(true, 0x10203040, null, 0, 0, 0, true));
        check("The prior-return EAX gate is explicit and header reads precede all later captures",
            entryGateNotMet.Outcome == ReliableReceiveLengthCallbackOutcome.EntryGateNotMet
            && !entryGateNotMet.PayloadHeaderRead
            && entryGateNotMet.UnreachedPayloadHeaderCaptureProvided
            && entryGateNotMet.UnreachedPayloadDwordAt8CaptureProvided
            && entryGateNotMet.UnreachedPayloadDwordAt4CaptureProvided
            && entryGateNotMet.UnreachedIndirectTargetCaptureProvided
            && entryGateNotMet.UnreachedNormalReturnCaptureProvided
            && entryGateNotMet.StopAtRawFileOffsetExclusive == 0x00BE53BF
            && missingHeader.Outcome == ReliableReceiveLengthCallbackOutcome.MissingPayloadHeader
            && missingHeader.PayloadHeaderAddress == 0x10203040
            && !missingHeader.PayloadHeaderRead
            && headerWithUnreachedLaterCaptures.Outcome
                == ReliableReceiveLengthCallbackOutcome.UnreachedCaptureProvided
            && headerWithUnreachedLaterCaptures.UnreachedPayloadDwordAt8CaptureProvided
            && headerWithUnreachedLaterCaptures.UnreachedPayloadDwordAt4CaptureProvided
            && headerWithUnreachedLaterCaptures.UnreachedIndirectTargetCaptureProvided
            && headerWithUnreachedLaterCaptures.UnreachedNormalReturnCaptureProvided);

        ReliableReceiveLengthCallbackPlan missingPayload8 =
            ReliableReceiveLengthCallbackProjection.Project(
                new(true, 0x10203040, 0x0FFF, null, null, null, null));
        ReliableReceiveLengthCallbackPlan payload8WithLaterCaptures =
            ReliableReceiveLengthCallbackProjection.Project(
                new(true, 0x10203040, 0x1000, null, 0, 0, true));
        check("The ECX argument is the wrapped payload+8 address, not the DWORD loaded there",
            missingPayload8.Outcome
                == ReliableReceiveLengthCallbackOutcome.MissingPayloadDwordAt8
            && missingPayload8.PayloadHeaderRead
            && missingPayload8.PayloadDwordAt8Address == 0x10203048
            && missingPayload8.EcxArgumentAddress == 0x10203048
            && missingPayload8.EcxArgumentAddress != missingPayload8.Capture.PayloadDwordAt8
            && missingPayload8.ComputedLength == 0x000007FB
            && missingPayload8.StopAtRawFileOffsetExclusive == 0x00BE53D4
            && payload8WithLaterCaptures.Outcome
                == ReliableReceiveLengthCallbackOutcome.UnreachedCaptureProvided
            && payload8WithLaterCaptures.UnreachedPayloadDwordAt4CaptureProvided
            && payload8WithLaterCaptures.UnreachedIndirectTargetCaptureProvided
            && payload8WithLaterCaptures.UnreachedNormalReturnCaptureProvided);

        ReliableReceiveLengthCallbackPlan missingPayload4 =
            ReliableReceiveLengthCallbackProjection.Project(
                new(true, 0x10203040, 0x0003, 0, null, null, null));
        ReliableReceiveLengthCallbackPlan payload4WithLaterCaptures =
            ReliableReceiveLengthCallbackProjection.Project(
                new(true, 0x10203040, 0x0004, 0, null, 0, true));
        check("The independent payload+4 read is required before target capture and argument setup",
            missingPayload4.Outcome
                == ReliableReceiveLengthCallbackOutcome.MissingPayloadDwordAt4
            && missingPayload4.PayloadDwordAt8Read
            && missingPayload4.PayloadDwordAt8Address == 0x10203048
            && missingPayload4.PayloadDwordAt4Address == 0x10203044
            && !missingPayload4.PayloadDwordAt4Read
            && missingPayload4.StopAtRawFileOffsetExclusive == 0x00BE53D7
            && payload4WithLaterCaptures.Outcome
                == ReliableReceiveLengthCallbackOutcome.UnreachedCaptureProvided
            && payload4WithLaterCaptures.UnreachedIndirectTargetCaptureProvided
            && payload4WithLaterCaptures.UnreachedNormalReturnCaptureProvided);

        ReliableReceiveLengthCallbackPlan missingTarget =
            ReliableReceiveLengthCallbackProjection.Project(
                new(true, 0x10203040, 0x0007, 0x50607080, 0x90A0B0C0, null, null));
        ReliableReceiveLengthCallbackPlan laterNormalReturn =
            ReliableReceiveLengthCallbackProjection.Project(
                new(true, 0x10203040, 0x0007, 0x50607080, 0x90A0B0C0, null, true));
        check("Push order is length then payload+4; the call stack presents payload+4 then length",
            missingTarget.Outcome
                == ReliableReceiveLengthCallbackOutcome.MissingIndirectTarget
            && missingTarget.IndirectTargetSlotAddress == 0x50607084
            && !missingTarget.IndirectTargetRead
            && missingTarget.StackPushes.SequenceEqual(
                new uint[] { 3, 0x90A0B0C0 })
            && missingTarget.CallStackArguments.SequenceEqual(
                new uint[] { 0x90A0B0C0, 3 })
            && missingTarget.StopAtRawFileOffsetExclusive == 0x00BE53DA
            && laterNormalReturn.Outcome
                == ReliableReceiveLengthCallbackOutcome.UnreachedCaptureProvided
            && laterNormalReturn.UnreachedNormalReturnCaptureProvided);

        ReliableReceiveLengthCallbackPlan missingNormalReturn =
            ReliableReceiveLengthCallbackProjection.Project(
                new(true, 0x10203040, 0, 0, 0, 0, null));
        ReliableReceiveLengthCallbackPlan nonReturningCall =
            ReliableReceiveLengthCallbackProjection.Project(
                new(true, 0x10203040, 0, 0, 0, 0, false));
        ReliableReceiveLengthCallbackPlan normalReturn =
            ReliableReceiveLengthCallbackProjection.Project(
                new(true, 0x10203040, 0, 0, 0, 0, true));
        check("Zero payload values and target remain captured; only normal return reaches the boundary",
            missingNormalReturn.Outcome
                == ReliableReceiveLengthCallbackOutcome.MissingNormalReturn
            && missingNormalReturn.IndirectCallReached
            && missingNormalReturn.CapturedIndirectTarget == 0
            && !missingNormalReturn.NormalCallBoundaryReached
            && missingNormalReturn.StopAtRawFileOffsetExclusive == 0x00BE53DA
            && nonReturningCall.Outcome
                == ReliableReceiveLengthCallbackOutcome.IndirectCallDidNotReturnNormally
            && nonReturningCall.CapturedIndirectTarget == 0
            && nonReturningCall.IndirectCallReturnedNormally == false
            && !nonReturningCall.NormalCallBoundaryReached
            && !normalReturn.UnreachedNormalReturnCaptureProvided
            && normalReturn.Outcome
                == ReliableReceiveLengthCallbackOutcome.IndirectCallReturnedNormally
            && normalReturn.CapturedIndirectTarget == 0
            && normalReturn.EcxArgumentAddress == 0x10203048
            && normalReturn.EcxArgumentAddress != normalReturn.Capture.PayloadDwordAt8
            && normalReturn.IndirectCallReturnedNormally == true
            && normalReturn.NormalCallBoundaryReached
            && normalReturn.SelectedDestinationRawFileOffset == 0x00BE53DD
            && normalReturn.StopAtRawFileOffsetExclusive == 0x00BE53DD);

        ReliableReceiveLengthCallbackPlan wrappedAddresses =
            ReliableReceiveLengthCallbackProjection.Project(
                new(true, uint.MaxValue - 7, 4, uint.MaxValue - 3, 0, 0, true));
        check("Payload read and ECX addresses wrap independently, and target address uses loaded DWORD8",
            ReliableReceiveLengthCallbackProjection.PayloadAddress(uint.MaxValue, 8) == 7
            && ReliableReceiveLengthCallbackProjection.IndirectTargetSlotAddress(uint.MaxValue) == 3
            && wrappedAddresses.PayloadHeaderAddress == uint.MaxValue - 7
            && wrappedAddresses.PayloadDwordAt8Address == 0
            && wrappedAddresses.EcxArgumentAddress == 0
            && wrappedAddresses.PayloadDwordAt4Address == uint.MaxValue - 3
            && wrappedAddresses.IndirectTargetSlotAddress == 0
            && wrappedAddresses.IndirectCallReached
            && wrappedAddresses.NormalCallBoundaryReached);
    }

    private static bool InstructionEvidenceMatches()
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
            (0x00BE53DA, 0x00FE5FDA, "FF 50 04")
        };
        return ReliableReceiveLengthCallbackProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
            .SequenceEqual(expected);
    }
}
