using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveResultStoresChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample raw window, instruction bytes, and raw-to-VA mapping match",
            UnreliableReceiveResultStoresProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveResultStoresProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveResultStoresProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveResultStoresProjection.EntryRawFileOffset == 0x00BE5BAC
            && UnreliableReceiveResultStoresProjection.EntryVirtualAddress == 0x00FE67AC
            && UnreliableReceiveResultStoresProjection.FrameMinus4LowWordZeroExtendRawFileOffset
                == 0x00BE5BB5
            && UnreliableReceiveResultStoresProjection.FrameMinus4LowWordStoreRawFileOffset
                == 0x00BE5BB8
            && UnreliableReceiveResultStoresProjection.EndRawFileOffsetExclusive == 0x00BE5BC4
            && UnreliableReceiveResultStoresProjection.RawFileOffsetToVirtualAddress(0x00BE5BAC)
                == 0x00FE67AC
            && UnreliableReceiveResultStoresProjection.RawFileOffsetToVirtualAddress(0x00BE5BC4)
                == 0x00FE67C4
            && UnreliableReceiveResultStoresProjection.BelowThresholdDestinationRawFileOffset
                == 0x00BE5C0F
            && UnreliableReceiveResultStoresProjection.CapturedWindowBytes
                == "8B 47 04 8B 4D FC 89 46 20 0F B7 C1 89 46 24 81 7F 4C 00 00 01 00 72 4B"
            && UnreliableReceiveResultStoresProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 24
            && InstructionsMatch());

        UnreliableReceiveResultStoresPlan bothStores =
            UnreliableReceiveResultStoresProjection.Project(
                new(0x1000, 0x2000, 0x3000, 0xAABBCCDD, 0x1234FFFF, 0x0000FFFF));
        check("The reloaded frame DWORD supplies only its low WORD, and both stores precede the counter read",
            bothStores.Outcome
                == UnreliableReceiveResultStoresOutcome.CounterBelowThreshold
            && bothStores.Receiver4Read
            && bothStores.Receiver4Address == 0x1004
            && bothStores.CapturedReceiver4 == 0xAABBCCDD
            && bothStores.FrameMinus4Read
            && bothStores.FrameMinus4Address == 0x2FFC
            && bothStores.CapturedFrameMinus4 == 0x1234FFFF
            && bothStores.Receiver4Stored
            && bothStores.Receiver4StoreAddress == 0x2020
            && bothStores.Receiver4StoredValue == 0xAABBCCDD
            && bothStores.FrameMinus4LowWordStored
            && bothStores.FrameMinus4StoreAddress == 0x2024
            && bothStores.FrameMinus4LowWordStoredValue == 0xFFFF
            && bothStores.Receiver4CRead
            && bothStores.Receiver4CAddress == 0x104C
            && bothStores.CapturedReceiver4C == 0xFFFF
            && bothStores.CounterBelowThreshold == true
            && bothStores.SelectedDestinationRawFileOffset == 0x00BE5C0F);

        UnreliableReceiveResultStoresPlan zeroResult =
            UnreliableReceiveResultStoresProjection.Project(
                new(0x1000, 0, 0x3000, 1, 2, 3));
        UnreliableReceiveResultStoresPlan receiverZero =
            UnreliableReceiveResultStoresProjection.Project(
                new(0, 0x2000, 0x3000, 0xAABBCCDD, 0x12340003, 0x00010000));
        check("Only a zero result is invalid at entry; receiver EDI zero remains a valid address base",
            zeroResult.Outcome == UnreliableReceiveResultStoresOutcome.InvalidEntry
            && zeroResult.ReceiverEdiNonzero
            && !zeroResult.ResultEsiNonzero
            && zeroResult.UnreachedReceiver4CaptureProvided
            && zeroResult.UnreachedFrameMinus4CaptureProvided
            && zeroResult.UnreachedReceiver4CCaptureProvided
            && !zeroResult.Receiver4Read
            && !zeroResult.FrameMinus4Read
            && !zeroResult.Receiver4Stored
            && !zeroResult.FrameMinus4LowWordStored
            && !zeroResult.Receiver4CRead
            && zeroResult.StopAtRawFileOffsetExclusive == 0x00BE5BAC
            && receiverZero.Outcome
                == UnreliableReceiveResultStoresOutcome.CounterAtOrAboveThreshold
            && !receiverZero.ReceiverEdiNonzero
            && receiverZero.ResultEsiNonzero
            && receiverZero.Receiver4Read
            && receiverZero.Receiver4Address == 4
            && receiverZero.Receiver4Stored
            && receiverZero.Receiver4StoreAddress == 0x2020
            && receiverZero.FrameMinus4LowWordStoredValue == 3
            && receiverZero.Receiver4CRead
            && receiverZero.Receiver4CAddress == 0x4C
            && receiverZero.SelectedDestinationRawFileOffset == 0x00BE5BC4);

        UnreliableReceiveResultStoresPlan missingReceiver4 =
            UnreliableReceiveResultStoresProjection.Project(
                new(0, 0x2000, 0x3000, null, null, null));
        UnreliableReceiveResultStoresPlan laterReceiver4Capture =
            UnreliableReceiveResultStoresProjection.Project(
                new(0, 0x2000, 0x3000, null, 0, 0));
        check("A missing first read blocks the frame reload and rejects later captures as unreached",
            missingReceiver4.Outcome
                == UnreliableReceiveResultStoresOutcome.MissingReceiver4
            && missingReceiver4.Receiver4Address == 4
            && !missingReceiver4.ReceiverEdiNonzero
            && !missingReceiver4.Receiver4Read
            && !missingReceiver4.FrameMinus4Read
            && !missingReceiver4.Receiver4Stored
            && missingReceiver4.StopAtRawFileOffsetExclusive == 0x00BE5BAC
            && laterReceiver4Capture.Outcome
                == UnreliableReceiveResultStoresOutcome.UnreachedCaptureProvided
            && laterReceiver4Capture.UnreachedFrameMinus4CaptureProvided
            && laterReceiver4Capture.UnreachedReceiver4CCaptureProvided
            && !laterReceiver4Capture.FrameMinus4Read);

        UnreliableReceiveResultStoresPlan missingFrameMinus4 =
            UnreliableReceiveResultStoresProjection.Project(
                new(0, 0x2000, 0x3000, 0xAABBCCDD, null, null));
        UnreliableReceiveResultStoresPlan laterCounterCapture =
            UnreliableReceiveResultStoresProjection.Project(
                new(0, 0x2000, 0x3000, 0xAABBCCDD, null, 0));
        check("A missing frame reload preserves the first capture but suppresses both stores",
            missingFrameMinus4.Outcome
                == UnreliableReceiveResultStoresOutcome.MissingFrameMinus4
            && missingFrameMinus4.Receiver4Read
            && !missingFrameMinus4.ReceiverEdiNonzero
            && missingFrameMinus4.CapturedReceiver4 == 0xAABBCCDD
            && missingFrameMinus4.FrameMinus4Address == 0x2FFC
            && !missingFrameMinus4.FrameMinus4Read
            && !missingFrameMinus4.Receiver4Stored
            && missingFrameMinus4.Receiver4StoredValue is null
            && !missingFrameMinus4.FrameMinus4LowWordStored
            && missingFrameMinus4.FrameMinus4LowWordStoredValue is null
            && missingFrameMinus4.StopAtRawFileOffsetExclusive == 0x00BE5BAF
            && laterCounterCapture.Outcome
                == UnreliableReceiveResultStoresOutcome.UnreachedCaptureProvided
            && laterCounterCapture.UnreachedReceiver4CCaptureProvided
            && !laterCounterCapture.Receiver4Stored);

        UnreliableReceiveResultStoresPlan missingCounter =
            UnreliableReceiveResultStoresProjection.Project(
                new(0, 0x2000, 0x3000, 0xAABBCCDD, 0x12340001, null));
        check("A missing counter read preserves both completed stores without inferring the branch",
            missingCounter.Outcome
                == UnreliableReceiveResultStoresOutcome.MissingReceiver4C
            && missingCounter.Receiver4Stored
            && missingCounter.Receiver4StoredValue == 0xAABBCCDD
            && missingCounter.FrameMinus4LowWordStored
            && missingCounter.FrameMinus4LowWordStoredValue == 1
            && missingCounter.Receiver4CAddress == 0x4C
            && !missingCounter.Receiver4CRead
            && missingCounter.CapturedReceiver4C is null
            && missingCounter.CounterBelowThreshold is null
            && missingCounter.SelectedDestinationRawFileOffset is null
            && missingCounter.StopAtRawFileOffsetExclusive == 0x00BE5BBB);

        UnreliableReceiveResultStoresPlan atThreshold =
            ProjectCounter(0x00010000);
        UnreliableReceiveResultStoresPlan aboveThreshold =
            ProjectCounter(uint.MaxValue);
        UnreliableReceiveResultStoresPlan zeroCounter =
            ProjectCounter(0);
        check("The unsigned DWORD comparison branches below 0x10000 and not at or above it",
            zeroCounter.Outcome
                == UnreliableReceiveResultStoresOutcome.CounterBelowThreshold
            && zeroCounter.SelectedDestinationRawFileOffset == 0x00BE5C0F
            && atThreshold.Outcome
                == UnreliableReceiveResultStoresOutcome.CounterAtOrAboveThreshold
            && atThreshold.CounterBelowThreshold == false
            && atThreshold.SelectedDestinationRawFileOffset == 0x00BE5BC4
            && aboveThreshold.Outcome
                == UnreliableReceiveResultStoresOutcome.CounterAtOrAboveThreshold
            && aboveThreshold.CounterBelowThreshold == false
            && aboveThreshold.SelectedDestinationRawFileOffset == 0x00BE5BC4);

        UnreliableReceiveResultStoresPlan wrappedAddresses =
            UnreliableReceiveResultStoresProjection.Project(
                new(uint.MaxValue - 3, 0xFFFFFFDF, 3, 0, uint.MaxValue, 0x0000FFFF));
        check("DWORD effective addresses wrap, and the frame reload remains independent of the stored receiver value",
            wrappedAddresses.Receiver4Address == 0
            && wrappedAddresses.FrameMinus4Address == uint.MaxValue
            && wrappedAddresses.Receiver4StoreAddress == uint.MaxValue
            && wrappedAddresses.FrameMinus4StoreAddress == 3
            && wrappedAddresses.Receiver4CAddress == 0x48
            && wrappedAddresses.Receiver4StoredValue == 0
            && wrappedAddresses.CapturedFrameMinus4 == uint.MaxValue
            && wrappedAddresses.FrameMinus4LowWordStoredValue == 0xFFFF
            && UnreliableReceiveResultStoresProjection.Receiver4Address(uint.MaxValue) == 3
            && UnreliableReceiveResultStoresProjection.FrameMinus4Address(0) == uint.MaxValue - 3
            && UnreliableReceiveResultStoresProjection.Receiver4CAddress(uint.MaxValue) == 0x4B
            && UnreliableReceiveResultStoresProjection.Receiver4StoreAddress(uint.MaxValue) == 0x1F
            && UnreliableReceiveResultStoresProjection.FrameMinus4LowWordStoreAddress(uint.MaxValue)
                == 0x23);
    }

    private static UnreliableReceiveResultStoresPlan ProjectCounter(uint counter) =>
        UnreliableReceiveResultStoresProjection.Project(
            new(0x1000, 0x2000, 0x3000, 0x11223344, 0x55667788, counter));

    private static bool InstructionsMatch()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes)[]
        {
            (0x00BE5BAC, 0x00FE67AC, "8B 47 04"),
            (0x00BE5BAF, 0x00FE67AF, "8B 4D FC"),
            (0x00BE5BB2, 0x00FE67B2, "89 46 20"),
            (0x00BE5BB5, 0x00FE67B5, "0F B7 C1"),
            (0x00BE5BB8, 0x00FE67B8, "89 46 24"),
            (0x00BE5BBB, 0x00FE67BB, "81 7F 4C 00 00 01 00"),
            (0x00BE5BC2, 0x00FE67C2, "72 4B")
        };
        return UnreliableReceiveResultStoresProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
                .SequenceEqual(expected)
            && UnreliableReceiveResultStoresProjection.RawFileOffsetToVirtualAddress(
                0x00BE5BC2) == 0x00FE67C2;
    }
}
