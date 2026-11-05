using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveResidueCounterChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The exact sample, raw bytes, raw-to-VA mapping, and instruction sequence are pinned",
            UnreliableReceiveResidueCounterProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveResidueCounterProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveResidueCounterProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveResidueCounterProjection.CounterUpdateStartRawFileOffset
                == 0x00BE5AE4
            && UnreliableReceiveResidueCounterProjection.CounterUpdateStartVirtualAddress
                == 0x00FE66E4
            && UnreliableReceiveResidueCounterProjection.RawFileOffsetToVirtualAddress(0x00BE5AE4)
                == 0x00FE66E4
            && UnreliableReceiveResidueCounterProjection.CounterUpdateEndRawFileOffsetExclusive
                == 0x00BE5AF0
            && UnreliableReceiveResidueCounterProjection.CounterUpdateEndVirtualAddressExclusive
                == 0x00FE66F0
            && UnreliableReceiveResidueCounterProjection.RawFileOffsetToVirtualAddress(0x00BE5AF0)
                == 0x00FE66F0
            && UnreliableReceiveResidueCounterProjection.CapturedCounterUpdateBytes
                == "8B 4F 08 8D 46 FF 83 C4 14 01 41 1C"
            && UnreliableReceiveResidueCounterProjection.CapturedCounterUpdateBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 12
            && InstructionEvidenceMatches());

        var unreachedCapture = new UnreliableReceiveResidueCounterCapture(
            PriorHelperReturnedNormally: null,
            ReceiverEdi: 0xFFFFFFF8,
            RetainedEsi: 0,
            ReceiverDwordAtOffset08: 0,
            CounterDwordAtLoadedPointerOffset1C: 0xFFFFFFFF);
        UnreliableReceiveResidueCounterPlan missingNormalStatus =
            UnreliableReceiveResidueCounterProjection.Project(unreachedCapture);
        UnreliableReceiveResidueCounterPlan nonreturningHelper =
            UnreliableReceiveResidueCounterProjection.Project(
                unreachedCapture with { PriorHelperReturnedNormally = false });
        check("Missing and nonreturning prior-helper states do not enter this continuation window",
            missingNormalStatus.Outcome
                == UnreliableReceiveResidueCounterOutcome.MissingPriorHelperReturnCapture
            && missingNormalStatus.PriorHelperNormalGatePassed is null
            && !missingNormalStatus.ReceiverDwordAtOffset08Read
            && !missingNormalStatus.EsiMinusOneComputed
            && !missingNormalStatus.StackCleanupProjected
            && !missingNormalStatus.CounterDwordRead
            && !missingNormalStatus.CounterDwordWriteProjected
            && missingNormalStatus.UnreachedReceiverDwordAtOffset08CaptureProvided
            && missingNormalStatus.UnreachedCounterDwordAtLoadedPointerOffset1CCaptureProvided
            && missingNormalStatus.StopAtRawFileOffsetExclusive == 0x00BE5AE4
            && nonreturningHelper.Outcome
                == UnreliableReceiveResidueCounterOutcome.PriorHelperDidNotReturnNormally
            && nonreturningHelper.PriorHelperNormalGatePassed == false
            && !nonreturningHelper.ReceiverDwordAtOffset08Read
            && !nonreturningHelper.StackCleanupProjected
            && !nonreturningHelper.CounterDwordWriteProjected
            && nonreturningHelper.UnreachedReceiverDwordAtOffset08CaptureProvided
            && nonreturningHelper.UnreachedCounterDwordAtLoadedPointerOffset1CCaptureProvided
            && nonreturningHelper.StopAtRawFileOffsetExclusive == 0x00BE5AE4);

        UnreliableReceiveResidueCounterPlan missingReceiverCapture =
            UnreliableReceiveResidueCounterProjection.Project(
                new(
                    PriorHelperReturnedNormally: true,
                    ReceiverEdi: 0xFFFFFFF8,
                    RetainedEsi: 0,
                    ReceiverDwordAtOffset08: null,
                    CounterDwordAtLoadedPointerOffset1C: 0xFFFFFFFF));
        check("A missing receiver+8 load prevents later instructions and marks the counter capture unreached",
            missingReceiverCapture.Outcome
                == UnreliableReceiveResidueCounterOutcome.MissingReceiverDwordAtOffset08Capture
            && missingReceiverCapture.PriorHelperNormalGatePassed == true
            && missingReceiverCapture.ReceiverDwordAtOffset08Address == 0
            && !missingReceiverCapture.ReceiverDwordAtOffset08Read
            && !missingReceiverCapture.EsiMinusOneComputed
            && !missingReceiverCapture.StackCleanupProjected
            && !missingReceiverCapture.CounterDwordRead
            && !missingReceiverCapture.CounterDwordWriteProjected
            && !missingReceiverCapture.UnreachedReceiverDwordAtOffset08CaptureProvided
            && missingReceiverCapture.UnreachedCounterDwordAtLoadedPointerOffset1CCaptureProvided
            && missingReceiverCapture.StopAtRawFileOffsetExclusive == 0x00BE5AE4);

        UnreliableReceiveResidueCounterPlan zeroEsiAndZeroPointer =
            UnreliableReceiveResidueCounterProjection.Project(
                new(
                    PriorHelperReturnedNormally: true,
                    ReceiverEdi: 0xFFFFFFF8,
                    RetainedEsi: 0,
                    ReceiverDwordAtOffset08: 0,
                    CounterDwordAtLoadedPointerOffset1C: uint.MaxValue));
        UnreliableReceiveResidueCounterPlan maxEsiAndWrappedPointer =
            UnreliableReceiveResidueCounterProjection.Project(
                new(
                    PriorHelperReturnedNormally: true,
                    ReceiverEdi: 0x1234,
                    RetainedEsi: uint.MaxValue,
                    ReceiverDwordAtOffset08: 0xFFFFFFE8,
                    CounterDwordAtLoadedPointerOffset1C: 2));
        check("ESI and counter arithmetic wrap, and zero or wrapped pointers remain valid captures",
            zeroEsiAndZeroPointer.Outcome == UnreliableReceiveResidueCounterOutcome.CounterDwordAdded
            && zeroEsiAndZeroPointer.ReceiverDwordAtOffset08Address == 0
            && zeroEsiAndZeroPointer.ReceiverDwordAtOffset08Read
            && zeroEsiAndZeroPointer.LoadedPointerEcx == 0
            && zeroEsiAndZeroPointer.EsiMinusOneComputed
            && zeroEsiAndZeroPointer.EaxAfterEsiMinusOne == uint.MaxValue
            && zeroEsiAndZeroPointer.CounterDwordAtLoadedPointerOffset1CAddress == 0x1C
            && zeroEsiAndZeroPointer.CounterDwordRead
            && zeroEsiAndZeroPointer.CounterDwordReadValue == uint.MaxValue
            && zeroEsiAndZeroPointer.CounterDwordWriteProjected
            && zeroEsiAndZeroPointer.CounterDwordWriteValue == 0xFFFFFFFE
            && zeroEsiAndZeroPointer.StackCleanupProjected
            && zeroEsiAndZeroPointer.StackCleanupBytes == 0x14
            && zeroEsiAndZeroPointer.StopAtRawFileOffsetExclusive == 0x00BE5AF0
            && maxEsiAndWrappedPointer.ReceiverDwordAtOffset08Read
            && maxEsiAndWrappedPointer.LoadedPointerEcx == 0xFFFFFFE8
            && maxEsiAndWrappedPointer.EaxAfterEsiMinusOne == 0xFFFFFFFE
            && maxEsiAndWrappedPointer.CounterDwordAtLoadedPointerOffset1CAddress == 4
            && maxEsiAndWrappedPointer.CounterDwordReadValue == 2
            && maxEsiAndWrappedPointer.CounterDwordWriteValue == 0
            && maxEsiAndWrappedPointer.CounterDwordWriteProjected);

        UnreliableReceiveResidueCounterPlan missingCounterCapture =
            UnreliableReceiveResidueCounterProjection.Project(
                new(
                    PriorHelperReturnedNormally: true,
                    ReceiverEdi: 0x1000,
                    RetainedEsi: 0,
                    ReceiverDwordAtOffset08: 0x2000,
                    CounterDwordAtLoadedPointerOffset1C: null));
        check("A missing counter capture retains the completed EAX calculation and 0x14-byte cleanup",
            missingCounterCapture.Outcome
                == UnreliableReceiveResidueCounterOutcome.MissingCounterDwordAtLoadedPointerOffset1C
            && missingCounterCapture.ReceiverDwordAtOffset08Read
            && missingCounterCapture.LoadedPointerEcx == 0x2000
            && missingCounterCapture.EsiMinusOneComputed
            && missingCounterCapture.EaxAfterEsiMinusOne == uint.MaxValue
            && missingCounterCapture.StackCleanupProjected
            && missingCounterCapture.StackCleanupBytes == 0x14
            && missingCounterCapture.CounterDwordAtLoadedPointerOffset1CAddress == 0x201C
            && !missingCounterCapture.CounterDwordRead
            && missingCounterCapture.CounterDwordReadValue is null
            && !missingCounterCapture.CounterDwordWriteProjected
            && missingCounterCapture.CounterDwordWriteValue is null
            && !missingCounterCapture.UnreachedCounterDwordAtLoadedPointerOffset1CCaptureProvided
            && missingCounterCapture.StopAtRawFileOffsetExclusive == 0x00BE5AED);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes)[]
        {
            (0x00BE5AE4, 0x00FE66E4, "8B 4F 08"),
            (0x00BE5AE7, 0x00FE66E7, "8D 46 FF"),
            (0x00BE5AEA, 0x00FE66EA, "83 C4 14"),
            (0x00BE5AED, 0x00FE66ED, "01 41 1C")
        };
        return UnreliableReceiveResidueCounterProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
                .SequenceEqual(expected)
            && UnreliableReceiveResidueCounterProjection.ReceiverDwordAtOffset08ReadRawFileOffset
                == 0x00BE5AE4
            && UnreliableReceiveResidueCounterProjection.EsiMinusOneRawFileOffset == 0x00BE5AE7
            && UnreliableReceiveResidueCounterProjection.StackCleanupRawFileOffset == 0x00BE5AEA
            && UnreliableReceiveResidueCounterProjection.CounterDwordAddRawFileOffset == 0x00BE5AED
            && UnreliableReceiveResidueCounterProjection.StackCleanupBytes == 0x14;
    }
}
