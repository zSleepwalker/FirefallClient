using Firefall.Reconstruction.Startup;

internal static class PayloadWordSlotIndexChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The sampled PE hash, section mapping, bounded range, and literal bytes are pinned",
            PayloadWordSlotIndexProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadWordSlotIndexProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadWordSlotIndexProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PayloadWordSlotIndexProjection.GateStartRawFileOffset == 0x00BE6325
            && PayloadWordSlotIndexProjection.GateStartVirtualAddress == 0x00FE6F25
            && PayloadWordSlotIndexProjection.GateStartVirtualAddressFromPeSection
                == 0x00FE6F25
            && PayloadWordSlotIndexProjection.GateStartVirtualAddressFromRawOffset
                == 0x00FE6F25
            && PayloadWordSlotIndexProjection.GateEndRawFileOffsetExclusive == 0x00BE6352
            && PayloadWordSlotIndexProjection.GateEndVirtualAddressExclusive == 0x00FE6F52
            && PayloadWordSlotIndexProjection.BranchTargetRawFileOffset == 0x00BE636E
            && PayloadWordSlotIndexProjection.BranchTargetVirtualAddress == 0x00FE6F6E
            && string.Concat(PayloadWordSlotIndexProjection.CapturedGateBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B4E0853578B7E108D04CDFFFFFFFF4921460C037E0C8BC7C1E80323C88B46048D1C8D00000000833C0300751C"
            && PayloadWordSlotIndexProjection.Instructions.SequenceEqual(
                ExpectedInstructions()));

        PayloadWordSlotIndexPlan zeroCount = Project(
            receiverDword8: 0,
            receiverDword10: 0,
            dword0CBeforeAnd: 0x12345678,
            dword0CAfterAnd: 0,
            receiverDword4: 0x1000,
            slotDword: 0);
        check("Zero count wraps the LEA mask and ECX decrement as DWORD operations",
            zeroCount.ComputedMask == uint.MaxValue
            && zeroCount.ReceiverDword0CWriteValue == 0x12345678
            && zeroCount.DecrementedCount == uint.MaxValue);

        PayloadWordSlotIndexPlan multiplyOverflow = Project(
            receiverDword8: 0x20000000,
            receiverDword10: 0,
            dword0CBeforeAnd: uint.MaxValue,
            dword0CAfterAnd: 0,
            receiverDword4: 0,
            slotDword: 1);
        check("The ECX times eight LEA mask wraps before subtracting one",
            multiplyOverflow.ComputedMask == uint.MaxValue
            && multiplyOverflow.DecrementedCount == 0x1FFFFFFF);

        PayloadWordSlotIndexPlan unsignedShift = Project(
            receiverDword8: uint.MaxValue,
            receiverDword10: 0x7FFFFFF8,
            dword0CBeforeAnd: 0,
            dword0CAfterAnd: 8,
            receiverDword4: 0,
            slotDword: 0);
        check("The EAX right shift is unsigned and the addition wraps at DWORD width",
            unsignedShift.AddedDword == 0x80000000
            && unsignedShift.ShiftedDword == 0x10000000
            && unsignedShift.Index == 0x10000000);

        PayloadWordSlotIndexPlan nonPowerOfTwoIndex = Project(
            receiverDword8: 11,
            receiverDword10: 0,
            dword0CBeforeAnd: 0xAA,
            dword0CAfterAnd: 88,
            receiverDword4: 0xFFFFFFFC,
            slotDword: 0);
        check("The index is the native bitwise AND rather than a modulo or power-of-two assumption",
            nonPowerOfTwoIndex.DecrementedCount == 10
            && nonPowerOfTwoIndex.ShiftedDword == 11
            && nonPowerOfTwoIndex.Index == 10
            && nonPowerOfTwoIndex.ScaledOffset == 40
            && nonPowerOfTwoIndex.SlotAddress == 36);

        PayloadWordSlotIndexPlan independentReload = Project(
            receiverDword8: 4,
            receiverDword10: 100,
            dword0CBeforeAnd: 0x12345678,
            dword0CAfterAnd: 0xFFFFFFF8,
            receiverDword4: 0xFFFFFFFC,
            slotDword: 0);
        check("The post-AND +0x0C reload is independent of the computed write value",
            independentReload.ComputedMask == 31
            && independentReload.ReceiverDword0CWriteValue == 0x18
            && independentReload.ReceiverDword0CAfterAnd == 0xFFFFFFF8
            && independentReload.AddedDword == 92
            && independentReload.ShiftedDword == 11
            && independentReload.Index == 3);

        PayloadWordSlotIndexPlan missingFirst = PayloadWordSlotIndexProjection.Project(
            new(0x1000, null, null, null, null, null, null));
        PayloadWordSlotIndexPlan laterFirst = PayloadWordSlotIndexProjection.Project(
            new(0x1000, null, null, null, null, null, 0));
        PayloadWordSlotIndexPlan missingDword10 = PayloadWordSlotIndexProjection.Project(
            new(0x1000, 4, null, null, null, null, null));
        PayloadWordSlotIndexPlan laterDword10 = PayloadWordSlotIndexProjection.Project(
            new(0x1000, 4, null, 0, null, null, null));
        PayloadWordSlotIndexPlan missingBeforeAnd = PayloadWordSlotIndexProjection.Project(
            new(0x1000, 4, 8, null, null, null, null));
        PayloadWordSlotIndexPlan laterBeforeAnd = PayloadWordSlotIndexProjection.Project(
            new(0x1000, 4, 8, null, 0, null, null));
        PayloadWordSlotIndexPlan missingAfterAnd = PayloadWordSlotIndexProjection.Project(
            new(0x1000, 4, 8, 0, null, null, null));
        PayloadWordSlotIndexPlan laterAfterAnd = PayloadWordSlotIndexProjection.Project(
            new(0x1000, 4, 8, 0, null, 0, null));
        PayloadWordSlotIndexPlan missingBase = PayloadWordSlotIndexProjection.Project(
            new(0x1000, 4, 8, 0, 0, null, null));
        PayloadWordSlotIndexPlan laterBase = PayloadWordSlotIndexProjection.Project(
            new(0x1000, 4, 8, 0, 0, null, 0));

        check("Each missing sequential capture stops strictly; a supplied later capture is rejected as unreached",
            missingFirst.Outcome
                == PayloadWordSlotIndexOutcome.MissingReceiverDword8Capture
            && missingFirst.StopAtRawFileOffsetExclusive
                == PayloadWordSlotIndexProjection.ReceiverDword8ReadRawFileOffset
            && laterFirst.Outcome == PayloadWordSlotIndexOutcome.UnreachedCaptureProvided
            && missingDword10.Outcome
                == PayloadWordSlotIndexOutcome.MissingReceiverDword10Capture
            && laterDword10.Outcome == PayloadWordSlotIndexOutcome.UnreachedCaptureProvided
            && missingBeforeAnd.Outcome
                == PayloadWordSlotIndexOutcome.MissingReceiverDword0CBeforeAndCapture
            && laterBeforeAnd.Outcome == PayloadWordSlotIndexOutcome.UnreachedCaptureProvided
            && missingAfterAnd.Outcome
                == PayloadWordSlotIndexOutcome.MissingReceiverDword0CAfterAndCapture
            && laterAfterAnd.Outcome == PayloadWordSlotIndexOutcome.UnreachedCaptureProvided
            && missingBase.Outcome
                == PayloadWordSlotIndexOutcome.MissingReceiverDword4Capture
            && laterBase.Outcome == PayloadWordSlotIndexOutcome.UnreachedCaptureProvided
            && missingBase.ComputedMask == 31
            && missingBase.ReceiverDword0CWriteValue == 0);

        PayloadWordSlotIndexPlan missingSlot = PayloadWordSlotIndexProjection.Project(
            new(0x1000, 4, 8, 0x20, 0, 0x2000, null));
        PayloadWordSlotIndexPlan slotNonzero = PayloadWordSlotIndexProjection.Project(
            new(0x1000, 4, 8, 0x20, 0, 0x2000, 1));
        PayloadWordSlotIndexPlan slotZero = PayloadWordSlotIndexProjection.Project(
            new(0x1000, 4, 8, 0x20, 0, 0x2000, 0));
        check("The slot capture is required; the final branch stops at its target or fall-through boundary",
            missingSlot.Outcome == PayloadWordSlotIndexOutcome.MissingSlotCapture
            && missingSlot.SlotAddress == 0x2004
            && !missingSlot.SlotDwordRead
            && slotNonzero.Outcome
                == PayloadWordSlotIndexOutcome.SlotDwordNonzeroBranchesToBoundary
            && slotNonzero.RouteRawFileOffset == 0x00BE636E
            && slotNonzero.StopAtRawFileOffsetExclusive == 0x00BE636E
            && slotZero.Outcome
                == PayloadWordSlotIndexOutcome.SlotDwordZeroFallsThroughToBoundary
            && slotZero.RouteRawFileOffset == 0x00BE6352
            && slotZero.StopAtRawFileOffsetExclusive == 0x00BE6352);

        PayloadWordSlotIndexPlan wrappedAddresses = PayloadWordSlotIndexProjection.Project(
            new(uint.MaxValue, 0, 0, 0, 0, uint.MaxValue, 0));
        check("Receiver addresses, scaled offsets, and slot addresses wrap at DWORD width",
            PayloadWordSlotIndexProjection.ReceiverDwordAddress(uint.MaxValue, 8) == 7
            && PayloadWordSlotIndexProjection.ReceiverDwordAddress(uint.MaxValue, 0x10) == 15
            && wrappedAddresses.ReceiverDword8Address == 7
            && wrappedAddresses.ReceiverDword10Address == 15
            && wrappedAddresses.ReceiverDword0CAddress == 11
            && wrappedAddresses.ReceiverDword4Address == 3
            && wrappedAddresses.SlotAddress == uint.MaxValue);
    }

    private static PayloadWordSlotIndexPlan Project(
        uint receiverDword8,
        uint receiverDword10,
        uint dword0CBeforeAnd,
        uint dword0CAfterAnd,
        uint receiverDword4,
        uint slotDword) =>
        PayloadWordSlotIndexProjection.Project(
            new(
                0x1000,
                receiverDword8,
                receiverDword10,
                dword0CBeforeAnd,
                dword0CAfterAnd,
                receiverDword4,
                slotDword));

    private static PayloadWordSlotIndexInstructionEvidence[] ExpectedInstructions() =>
    [
        new("Capture the receiver DWORD at +8 into ECX", 0x00BE6325, 0x00FE6F25, "8B 4E 08"),
        new("Push EBX", 0x00BE6328, 0x00FE6F28, "53"),
        new("Push EDI", 0x00BE6329, 0x00FE6F29, "57"),
        new("Capture the receiver DWORD at +0x10 into EDI", 0x00BE632A, 0x00FE6F2A, "8B 7E 10"),
        new("Form wrapping ECX*8-1 in EAX", 0x00BE632D, 0x00FE6F2D, "8D 04 CD FF FF FF FF"),
        new("Decrement ECX with 32-bit wraparound", 0x00BE6334, 0x00FE6F34, "49"),
        new("Read, AND, and write the receiver DWORD at +0x0C", 0x00BE6335, 0x00FE6F35, "21 46 0C"),
        new("Independently reload the receiver DWORD at +0x0C and add it to EDI", 0x00BE6338, 0x00FE6F38, "03 7E 0C"),
        new("Move EDI to EAX and shift it right as an unsigned DWORD", 0x00BE633B, 0x00FE6F3B, "8B C7 C1 E8 03"),
        new("AND the decremented count with the shifted DWORD to form the index", 0x00BE6340, 0x00FE6F40, "23 C8"),
        new("Capture the receiver DWORD at +4 into EAX", 0x00BE6342, 0x00FE6F42, "8B 46 04"),
        new("Form the wrapping index*4 slot offset in EBX", 0x00BE6345, 0x00FE6F45, "8D 1C 8D 00 00 00 00"),
        new("Compare the DWORD at base+scaled offset against zero", 0x00BE634C, 0x00FE6F4C, "83 3C 03 00"),
        new("A nonzero slot DWORD branches to raw 0xBE636E; zero falls through to 0xBE6352", 0x00BE6350, 0x00FE6F50, "75 1C", 0x00BE636E, 0x00FE6F6E)
    ];
}
