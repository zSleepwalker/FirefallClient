using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveDirectCallbackChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample, 18-byte callback range, and raw-to-VA mapping are exact",
            UnreliableReceiveDirectCallbackProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveDirectCallbackProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveDirectCallbackProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveDirectCallbackProjection.EntryRawFileOffset == 0x00BE5B52
            && UnreliableReceiveDirectCallbackProjection.EntryVirtualAddress == 0x00FE6752
            && UnreliableReceiveDirectCallbackProjection.PayloadDwordAt4PushRawFileOffset
                == 0x00BE5B58
            && UnreliableReceiveDirectCallbackProjection.PayloadAddressAt8PushRawFileOffset
                == 0x00BE5B5E
            && UnreliableReceiveDirectCallbackProjection.ReceiverDwordAt14ReadRawFileOffset
                == 0x00BE5B5F
            && UnreliableReceiveDirectCallbackProjection.IndirectCallRawFileOffset == 0x00BE5B62
            && UnreliableReceiveDirectCallbackProjection.NormalCallBoundaryRawFileOffset
                == 0x00BE5B64
            && UnreliableReceiveDirectCallbackProjection.NormalCallBoundaryVirtualAddress
                == 0x00FE6764
            && UnreliableReceiveDirectCallbackProjection.EndRawFileOffsetExclusive
                == 0x00BE5B64
            && UnreliableReceiveDirectCallbackProjection.RawFileOffsetToVirtualAddress(0x00BE5B52)
                == 0x00FE6752
            && UnreliableReceiveDirectCallbackProjection.RawFileOffsetToVirtualAddress(0x00BE5B64)
                == 0x00FE6764
            && UnreliableReceiveDirectCallbackProjection.CapturedCallbackBytes
                == "6A 00 0F B7 C6 50 FF 73 04 8D 43 08 50 8B 47 14 FF D0"
            && UnreliableReceiveDirectCallbackProjection.CapturedCallbackBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 18
            && InstructionsMatch());

        UnreliableReceiveDirectCallbackPlan unvalidatedEntry =
            UnreliableReceiveDirectCallbackProjection.Project(
                new(0x1000, 0x2000, 0xFFFF, false, 0, 0, true));
        check("The explicit entry-boundary gate prevents all callback pushes and later captures",
            unvalidatedEntry.Outcome
                == UnreliableReceiveDirectCallbackOutcome.UnreachedCaptureProvided
            && unvalidatedEntry.StackPushesInInstructionOrder.IsEmpty
            && unvalidatedEntry.UnreachedPayloadDwordAt4CaptureProvided
            && !unvalidatedEntry.PayloadDwordAt4Read
            && !unvalidatedEntry.ReceiverDwordAt14Read
            && unvalidatedEntry.UnreachedReceiverDwordAt14CaptureProvided
            && unvalidatedEntry.UnreachedNormalReturnCaptureProvided
            && !unvalidatedEntry.IndirectCallReached
            && unvalidatedEntry.StopAtRawFileOffsetExclusive == 0x00BE5B52);

        UnreliableReceiveDirectCallbackPlan zeroSi =
            UnreliableReceiveDirectCallbackProjection.Project(
                new(0x1000, 0x2000, 0, true, null, null, null));
        UnreliableReceiveDirectCallbackPlan maximumSi =
            UnreliableReceiveDirectCallbackProjection.Project(
                new(0x1000, 0x2000, ushort.MaxValue, true, null, null, null));
        check("SI values zero and 0xFFFF are pushed as zero-extended DWORDs",
            zeroSi.ZeroExtendedSi == 0
            && zeroSi.StackPushesInInstructionOrder.SequenceEqual(new uint[] { 0, 0 })
            && maximumSi.ZeroExtendedSi == 0x0000FFFF
            && maximumSi.StackPushesInInstructionOrder.SequenceEqual(
                new uint[] { 0, 0x0000FFFF }));

        UnreliableReceiveDirectCallbackPlan missingPayload4 =
            UnreliableReceiveDirectCallbackProjection.Project(
                new(0x10203040, 0x10203040, 0xFFFF, true, null, null, null));
        UnreliableReceiveDirectCallbackPlan laterCapturesAfterMissingPayload4 =
            UnreliableReceiveDirectCallbackProjection.Project(
                new(0x10203040, 0x10203040, 0xFFFF, true, null, 0, true));
        check("A missing payload DWORD+4 preserves the initial zero and SI pushes",
            missingPayload4.Outcome
                == UnreliableReceiveDirectCallbackOutcome.MissingPayloadDwordAt4
            && missingPayload4.PayloadDwordAt4Address == 0x10203044
            && !missingPayload4.PayloadDwordAt4Read
            && missingPayload4.StackPushesInInstructionOrder.SequenceEqual(
                new uint[] { 0, 0xFFFF })
            && missingPayload4.StopAtRawFileOffsetExclusive == 0x00BE5B58
            && laterCapturesAfterMissingPayload4.Outcome
                == UnreliableReceiveDirectCallbackOutcome.UnreachedCaptureProvided
            && laterCapturesAfterMissingPayload4.UnreachedReceiverDwordAt14CaptureProvided
            && laterCapturesAfterMissingPayload4.UnreachedNormalReturnCaptureProvided
            && laterCapturesAfterMissingPayload4.StackPushesInInstructionOrder.SequenceEqual(
                new uint[] { 0, 0xFFFF }));

        UnreliableReceiveDirectCallbackPlan missingReceiver =
            UnreliableReceiveDirectCallbackProjection.Project(
                new(0x10203040, 0x10203040, 7, true, 0xAABBCCDD, null, null));
        UnreliableReceiveDirectCallbackPlan laterReturnAfterMissingReceiver =
            UnreliableReceiveDirectCallbackProjection.Project(
                new(0x10203040, 0x10203040, 7, true, 0xAABBCCDD, null, true));
        check("The payload DWORD push and EBX+8 address push precede the receiver target reload",
            missingReceiver.Outcome
                == UnreliableReceiveDirectCallbackOutcome.MissingReceiverDwordAt14
            && missingReceiver.PayloadDwordAt4Read
            && missingReceiver.PayloadDwordAt4 == 0xAABBCCDD
            && missingReceiver.PayloadAddressAt8 == 0x10203048
            && missingReceiver.ReceiverDwordAt14Address == 0x10203054
            && !missingReceiver.ReceiverDwordAt14Read
            && missingReceiver.StackPushesInInstructionOrder.SequenceEqual(
                new uint[] { 0, 7, 0xAABBCCDD, 0x10203048 })
            && missingReceiver.StopAtRawFileOffsetExclusive == 0x00BE5B5F
            && laterReturnAfterMissingReceiver.Outcome
                == UnreliableReceiveDirectCallbackOutcome.UnreachedCaptureProvided
            && laterReturnAfterMissingReceiver.UnreachedNormalReturnCaptureProvided);

        UnreliableReceiveDirectCallbackPlan missingReturn =
            UnreliableReceiveDirectCallbackProjection.Project(
                new(0x10203040, 0x10203040, 7, true, 0, 0, null));
        UnreliableReceiveDirectCallbackPlan nonReturningCall =
            UnreliableReceiveDirectCallbackProjection.Project(
                new(0x10203040, 0x10203040, 7, true, 0, 0, false));
        UnreliableReceiveDirectCallbackPlan normalReturn =
            UnreliableReceiveDirectCallbackProjection.Project(
                new(0x10203040, 0x10203040, 7, true, 0, 0, true));
        check("A zero receiver target is still called; only a normal return reaches raw 0xBE5B64",
            missingReturn.Outcome == UnreliableReceiveDirectCallbackOutcome.MissingNormalReturn
            && missingReturn.ReceiverDwordAt14Read
            && missingReturn.ReceiverDwordAt14Address == 0x10203054
            && missingReturn.CapturedCallbackTarget == 0
            && missingReturn.IndirectCallReached
            && missingReturn.IndirectCallReturnedNormally is null
            && missingReturn.StackPushesInInstructionOrder.SequenceEqual(
                new uint[] { 0, 7, 0, 0x10203048 })
            && missingReturn.StopAtRawFileOffsetExclusive == 0x00BE5B62
            && nonReturningCall.Outcome
                == UnreliableReceiveDirectCallbackOutcome.IndirectCallDidNotReturnNormally
            && !nonReturningCall.NormalCallBoundaryReached
            && nonReturningCall.SelectedDestinationRawFileOffset is null
            && nonReturningCall.StopAtRawFileOffsetExclusive == 0x00BE5B62
            && normalReturn.Outcome
                == UnreliableReceiveDirectCallbackOutcome.IndirectCallReturnedNormally
            && normalReturn.NormalCallBoundaryReached
            && normalReturn.SelectedDestinationRawFileOffset == 0x00BE5B64
            && normalReturn.StopAtRawFileOffsetExclusive == 0x00BE5B64);

        UnreliableReceiveDirectCallbackPlan wrappedAddresses =
            UnreliableReceiveDirectCallbackProjection.Project(
                new(uint.MaxValue - 0x13, uint.MaxValue - 7, ushort.MaxValue, true,
                    0xDEADBEEF, 0, true));
        check("Payload and receiver effective addresses wrap at DWORD width, not WORD width",
            wrappedAddresses.PayloadDwordAt4Address == uint.MaxValue - 3
            && wrappedAddresses.PayloadAddressAt8 == 0
            && wrappedAddresses.ReceiverDwordAt14Address == 0
            && wrappedAddresses.CapturedCallbackTarget == 0
            && wrappedAddresses.StackPushesInInstructionOrder.SequenceEqual(
                new uint[] { 0, 0xFFFF, 0xDEADBEEF, 0 })
            && UnreliableReceiveDirectCallbackProjection.PayloadAddress(uint.MaxValue, 4) == 3
            && UnreliableReceiveDirectCallbackProjection.PayloadAddress(uint.MaxValue, 8) == 7
            && UnreliableReceiveDirectCallbackProjection.ReceiverDwordAt14Address(
                uint.MaxValue - 0x13) == 0);
    }

    private static bool InstructionsMatch()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes)[]
        {
            (0x00BE5B52, 0x00FE6752, "6A 00"),
            (0x00BE5B54, 0x00FE6754, "0F B7 C6"),
            (0x00BE5B57, 0x00FE6757, "50"),
            (0x00BE5B58, 0x00FE6758, "FF 73 04"),
            (0x00BE5B5B, 0x00FE675B, "8D 43 08"),
            (0x00BE5B5E, 0x00FE675E, "50"),
            (0x00BE5B5F, 0x00FE675F, "8B 47 14"),
            (0x00BE5B62, 0x00FE6762, "FF D0")
        };
        return UnreliableReceiveDirectCallbackProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
            .SequenceEqual(expected);
    }
}
