using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveMarkerCallbackChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample, callback byte range, and raw-to-VA mapping are exact",
            UnreliableReceiveMarkerCallbackProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveMarkerCallbackProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveMarkerCallbackProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveMarkerCallbackProjection.EntryRawFileOffset == 0x00BE5B16
            && UnreliableReceiveMarkerCallbackProjection.EntryVirtualAddress == 0x00FE6716
            && UnreliableReceiveMarkerCallbackProjection.PayloadDwordAt8ReadRawFileOffset
                == 0x00BE5B1D
            && UnreliableReceiveMarkerCallbackProjection.PayloadDwordAt4PushRawFileOffset
                == 0x00BE5B1F
            && UnreliableReceiveMarkerCallbackProjection.IndirectCallRawFileOffset
                == 0x00BE5B22
            && UnreliableReceiveMarkerCallbackProjection.NormalCallBoundaryRawFileOffset
                == 0x00BE5B25
            && UnreliableReceiveMarkerCallbackProjection.NormalCallBoundaryVirtualAddress
                == 0x00FE6725
            && UnreliableReceiveMarkerCallbackProjection.EndRawFileOffsetExclusive
                == 0x00BE5B25
            && UnreliableReceiveMarkerCallbackProjection.RawFileOffsetToVirtualAddress(0x00BE5B16)
                == 0x00FE6716
            && UnreliableReceiveMarkerCallbackProjection.RawFileOffsetToVirtualAddress(0x00BE5B25)
                == 0x00FE6725
            && UnreliableReceiveMarkerCallbackProjection.CapturedCallbackBytes
                == "0F B7 C6 8D 4B 08 50 8B 01 FF 73 04 FF 50 04"
            && UnreliableReceiveMarkerCallbackProjection.CapturedCallbackBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 15
            && InstructionsMatch());

        check("Only prior-selector markers one through three are accepted",
            UnreliableReceiveMarkerCallbackProjection.IsValidMarker(1)
            && UnreliableReceiveMarkerCallbackProjection.IsValidMarker(2)
            && UnreliableReceiveMarkerCallbackProjection.IsValidMarker(3)
            && RejectsInvalidMarker(0)
            && RejectsInvalidMarker(4)
            && RejectsInvalidMarker(byte.MaxValue));

        UnreliableReceiveMarkerCallbackPlan zeroLength =
            UnreliableReceiveMarkerCallbackProjection.Project(new(0x1000, 0, 1, null, null, null, null));
        UnreliableReceiveMarkerCallbackPlan highBitLength =
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(0x1000, 0x8000, 2, null, null, null, null));
        UnreliableReceiveMarkerCallbackPlan maximumLength =
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(0x1000, ushort.MaxValue, 3, null, null, null, null));
        check("SI is zero-extended directly to the pushed DWORD without subtraction or halfword truncation",
            zeroLength.ZeroExtendedSiLength == 0
            && zeroLength.StackPushesInInstructionOrder.SequenceEqual(new uint[] { 0 })
            && highBitLength.ZeroExtendedSiLength == 0x00008000
            && highBitLength.StackPushesInInstructionOrder.SequenceEqual(new uint[] { 0x8000 })
            && maximumLength.ZeroExtendedSiLength == 0x0000FFFF
            && maximumLength.StackPushesInInstructionOrder.SequenceEqual(new uint[] { 0xFFFF }));

        UnreliableReceiveMarkerCallbackPlan missingPayload8 =
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(0x10203040, 0xFEDC, 1, null, null, null, null));
        UnreliableReceiveMarkerCallbackPlan laterCapturesAfterMissingPayload8 =
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(0x10203040, 0xFEDC, 1, null, 0, 0, true));
        check("The length push persists at a missing DWORD+8 read, and later captures remain unreached",
            missingPayload8.Outcome
                == UnreliableReceiveMarkerCallbackOutcome.MissingPayloadDwordAt8
            && missingPayload8.ZeroExtendedSiLength == 0xFEDC
            && missingPayload8.EcxPayloadAddress == 0x10203048
            && missingPayload8.PayloadDwordAt8Address == 0x10203048
            && !missingPayload8.PayloadDwordAt8Read
            && missingPayload8.StackPushesInInstructionOrder.SequenceEqual(new uint[] { 0xFEDC })
            && missingPayload8.StopAtRawFileOffsetExclusive == 0x00BE5B1D
            && laterCapturesAfterMissingPayload8.Outcome
                == UnreliableReceiveMarkerCallbackOutcome.UnreachedCaptureProvided
            && laterCapturesAfterMissingPayload8.UnreachedPayloadDwordAt4CaptureProvided
            && laterCapturesAfterMissingPayload8.UnreachedIndirectTargetCaptureProvided
            && laterCapturesAfterMissingPayload8.UnreachedNormalReturnCaptureProvided
            && !laterCapturesAfterMissingPayload8.PayloadDwordAt8Read
            && laterCapturesAfterMissingPayload8.StackPushesInInstructionOrder
                .SequenceEqual(new uint[] { 0xFEDC }));

        UnreliableReceiveMarkerCallbackPlan missingPayload4 =
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(0x10203040, 0xFFFF, 1, 0xAABBCCDD, null, null, null));
        UnreliableReceiveMarkerCallbackPlan laterCapturesAfterMissingPayload4 =
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(0x10203040, 0xFFFF, 1, 0xAABBCCDD, null, 0, true));
        check("DWORD+8 is read before the DWORD+4 push; a missing second capture retains only the length push",
            missingPayload4.Outcome
                == UnreliableReceiveMarkerCallbackOutcome.MissingPayloadDwordAt4
            && missingPayload4.PayloadDwordAt8Read
            && missingPayload4.PayloadDwordAt8 == 0xAABBCCDD
            && missingPayload4.PayloadDwordAt4Address == 0x10203044
            && !missingPayload4.PayloadDwordAt4Read
            && missingPayload4.StackPushesInInstructionOrder.SequenceEqual(new uint[] { 0xFFFF })
            && missingPayload4.StopAtRawFileOffsetExclusive == 0x00BE5B1F
            && laterCapturesAfterMissingPayload4.Outcome
                == UnreliableReceiveMarkerCallbackOutcome.UnreachedCaptureProvided
            && laterCapturesAfterMissingPayload4.UnreachedIndirectTargetCaptureProvided
            && laterCapturesAfterMissingPayload4.UnreachedNormalReturnCaptureProvided
            && laterCapturesAfterMissingPayload4.StackPushesInInstructionOrder
                .SequenceEqual(new uint[] { 0xFFFF }));

        UnreliableReceiveMarkerCallbackPlan missingTarget =
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(0x10203040, 0, 1, 0x50607080, 0x90A0B0C0, null, null));
        UnreliableReceiveMarkerCallbackPlan laterReturnAfterMissingTarget =
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(0x10203040, 0, 1, 0x50607080, 0x90A0B0C0, null, true));
        check("Both pushes precede the target read, and a return capture cannot bypass a missing target",
            missingTarget.Outcome == UnreliableReceiveMarkerCallbackOutcome.MissingIndirectTarget
            && missingTarget.IndirectTargetSlotAddress == 0x50607084
            && !missingTarget.IndirectTargetRead
            && missingTarget.StackPushesInInstructionOrder.SequenceEqual(
                new uint[] { 0, 0x90A0B0C0 })
            && missingTarget.StopAtRawFileOffsetExclusive == 0x00BE5B22
            && laterReturnAfterMissingTarget.Outcome
                == UnreliableReceiveMarkerCallbackOutcome.UnreachedCaptureProvided
            && laterReturnAfterMissingTarget.UnreachedNormalReturnCaptureProvided);

        UnreliableReceiveMarkerCallbackPlan missingReturn =
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(0x10203040, 7, 1, 0, 0, 0, null));
        UnreliableReceiveMarkerCallbackPlan nonReturningCall =
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(0x10203040, 7, 1, 0, 0, 0, false));
        UnreliableReceiveMarkerCallbackPlan normalReturn =
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(0x10203040, 7, 1, 0, 0, 0, true));
        check("Zero DWORD captures are valid; only a normal indirect-call return reaches raw 0xBE5B25",
            missingReturn.Outcome == UnreliableReceiveMarkerCallbackOutcome.MissingNormalReturn
            && missingReturn.IndirectTargetRead
            && missingReturn.IndirectTargetSlotAddress == 4
            && missingReturn.CapturedIndirectTarget == 0
            && missingReturn.IndirectCallReached
            && missingReturn.IndirectCallReturnedNormally is null
            && missingReturn.StackPushesInInstructionOrder.SequenceEqual(new uint[] { 7, 0 })
            && missingReturn.StopAtRawFileOffsetExclusive == 0x00BE5B22
            && nonReturningCall.Outcome
                == UnreliableReceiveMarkerCallbackOutcome.IndirectCallDidNotReturnNormally
            && !nonReturningCall.NormalCallBoundaryReached
            && nonReturningCall.SelectedDestinationRawFileOffset is null
            && nonReturningCall.StopAtRawFileOffsetExclusive == 0x00BE5B22
            && normalReturn.Outcome
                == UnreliableReceiveMarkerCallbackOutcome.IndirectCallReturnedNormally
            && normalReturn.NormalCallBoundaryReached
            && normalReturn.SelectedDestinationRawFileOffset == 0x00BE5B25
            && normalReturn.StopAtRawFileOffsetExclusive == 0x00BE5B25);

        UnreliableReceiveMarkerCallbackPlan wrappedAddresses =
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(uint.MaxValue - 7, 0xFFFF, 1, uint.MaxValue, 0, uint.MaxValue, true));
        check("Payload and indirect-target slot effective addresses use DWORD wraparound",
            wrappedAddresses.EcxPayloadAddress == 0
            && wrappedAddresses.PayloadDwordAt8Address == 0
            && wrappedAddresses.PayloadDwordAt4Address == uint.MaxValue - 3
            && wrappedAddresses.IndirectTargetSlotAddress == 3
            && wrappedAddresses.CapturedIndirectTarget == uint.MaxValue
            && UnreliableReceiveMarkerCallbackProjection.PayloadAddress(uint.MaxValue, 8) == 7
            && UnreliableReceiveMarkerCallbackProjection.PayloadAddress(uint.MaxValue, 4)
                == 3
            && UnreliableReceiveMarkerCallbackProjection.IndirectTargetSlotAddress(uint.MaxValue)
                == 3);
    }

    private static bool RejectsInvalidMarker(byte marker)
    {
        try
        {
            UnreliableReceiveMarkerCallbackProjection.Project(
                new(0, 0, marker, null, null, null, null));
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }

    private static bool InstructionsMatch()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes)[]
        {
            (0x00BE5B16, 0x00FE6716, "0F B7 C6"),
            (0x00BE5B19, 0x00FE6719, "8D 4B 08"),
            (0x00BE5B1C, 0x00FE671C, "50"),
            (0x00BE5B1D, 0x00FE671D, "8B 01"),
            (0x00BE5B1F, 0x00FE671F, "FF 73 04"),
            (0x00BE5B22, 0x00FE6722, "FF 50 04")
        };
        return UnreliableReceiveMarkerCallbackProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
            .SequenceEqual(expected);
    }
}
