using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveHighCountForwardChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE sample identity, raw-window endpoints, raw-to-VA mapping, and exact bytes match",
            UnreliableReceiveHighCountForwardProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveHighCountForwardProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveHighCountForwardProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveHighCountForwardProjection.EntryRawFileOffset == 0x00BE5BC4
            && UnreliableReceiveHighCountForwardProjection.EntryVirtualAddress == 0x00FE67C4
            && UnreliableReceiveHighCountForwardProjection.EndRawFileOffsetExclusive == 0x00BE5BF1
            && UnreliableReceiveHighCountForwardProjection.EndVirtualAddressExclusive == 0x00FE67F1
            && UnreliableReceiveHighCountForwardProjection.RawFileOffsetToVirtualAddress(0x00BE5BC4)
                == 0x00FE67C4
            && UnreliableReceiveHighCountForwardProjection.RawFileOffsetToVirtualAddress(0x00BE5BF1)
                == 0x00FE67F1
            && UnreliableReceiveHighCountForwardProjection.CapturedWindowBytes
                == "68 D8 02 00 00 68 C0 8E BB 01 FF 77 04 68 0C 8F BB 01 "
                    + "6A 00 FF 77 18 E8 70 D3 FF FF 8B 47 08 8D 4F 3C "
                    + "83 C4 18 FF 40 1C E8 4F 08 00 00"
            && UnreliableReceiveHighCountForwardProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x2D
            && InstructionEvidenceMatches());

        check("Both relative calls resolve to their exact targets and end at exclusive raw 0xBE5BF1",
            UnreliableReceiveHighCountForwardProjection.RelativeCallTarget(
                0x00BE5BDB,
                unchecked((int)0xFFFFD370)) == 0x00BE2F50
            && UnreliableReceiveHighCountForwardProjection.RelativeCallTarget(
                0x00BE5BEC,
                0x84F) == 0x00BE6440
            && UnreliableReceiveHighCountForwardProjection.ForwardCallTargetRawFileOffset
                == 0x00BE2F50
            && UnreliableReceiveHighCountForwardProjection.ForwardCallTargetVirtualAddress
                == 0x00FE3B50
            && UnreliableReceiveHighCountForwardProjection.SecondCallTargetRawFileOffset
                == 0x00BE6440
            && UnreliableReceiveHighCountForwardProjection.SecondCallTargetVirtualAddress
                == 0x00FE7040
            && UnreliableReceiveHighCountForwardProjection.InstructionsInOrder.SequenceEqual(
                new[]
                {
                    UnreliableReceiveHighCountForwardInstruction.Push2D8,
                    UnreliableReceiveHighCountForwardInstruction.Push1BB8EC0,
                    UnreliableReceiveHighCountForwardInstruction.PushReceiverDwordAtOffset04,
                    UnreliableReceiveHighCountForwardInstruction.Push1BB8F0C,
                    UnreliableReceiveHighCountForwardInstruction.PushZero,
                    UnreliableReceiveHighCountForwardInstruction.PushReceiverDwordAtOffset18,
                    UnreliableReceiveHighCountForwardInstruction.CallForwardHelper,
                    UnreliableReceiveHighCountForwardInstruction.ReloadReceiverDwordAtOffset08IntoEax,
                    UnreliableReceiveHighCountForwardInstruction.LeaReceiverPlus3CIntoEcx,
                    UnreliableReceiveHighCountForwardInstruction.AddEsp18,
                    UnreliableReceiveHighCountForwardInstruction.IncrementObjectDwordAtOffset1C,
                    UnreliableReceiveHighCountForwardInstruction.CallSecondHelper
                }));

        UnreliableReceiveHighCountForwardCapture completeCapture =
            new(true, 0, 0x11223344, 0x55667788, true, 0, uint.MaxValue, true);
        UnreliableReceiveHighCountForwardPlan complete =
            UnreliableReceiveHighCountForwardProjection.Project(completeCapture);
        check("Validated execution preserves push order, independent reloads, cleanup, and wrapping INC",
            complete.Outcome == UnreliableReceiveHighCountForwardOutcome.SecondHelperReturnedNormally
            && complete.ReceiverDwordAtOffset04Address == 4
            && complete.ReceiverDwordAtOffset04Read
            && complete.CapturedReceiverDwordAtOffset04 == 0x11223344
            && complete.ReceiverDwordAtOffset18Address == 0x18
            && complete.ReceiverDwordAtOffset18Read
            && complete.CapturedReceiverDwordAtOffset18 == 0x55667788
            && complete.StackPushesInInstructionOrder.SequenceEqual(
                new uint[] { 0x2D8, 0x01BB8EC0, 0x11223344, 0x01BB8F0C, 0, 0x55667788 })
            && complete.ForwardCallReached
            && complete.ForwardHelperReturnedNormally == true
            && complete.ReceiverDwordAtOffset08Address == 8
            && complete.ReceiverDwordAtOffset08Read
            && complete.LoadedObjectPointerEax == 0
            && complete.ReceiverPlus3CAddressPassedInEcx == 0x3C
            && complete.StackCleanupProjected
            && complete.StackCleanupBytes == 0x18
            && complete.ObjectDwordAtOffset1CAddress == 0x1C
            && complete.ObjectDwordAtOffset1CRead
            && complete.CounterValueBeforeIncrement == uint.MaxValue
            && complete.CounterIncrementProjected
            && complete.CounterValueAfterIncrement == 0
            && complete.SecondCallReached
            && complete.SecondHelperReturnedNormally == true
            && complete.NormalCallBoundaryReached
            && complete.StopAtRawFileOffsetExclusive == 0x00BE5BF1
            && complete.InstructionsExecuted.SequenceEqual(
                UnreliableReceiveHighCountForwardProjection.InstructionsInOrder));

        UnreliableReceiveHighCountForwardPlan rejectedBoundary =
            UnreliableReceiveHighCountForwardProjection.Project(
                completeCapture with { ValidatedHighCountBoundary = false });
        check("Boundary rejection performs no loads or pushes and does not reject a zero receiver",
            rejectedBoundary.Outcome
                == UnreliableReceiveHighCountForwardOutcome.EntryBoundaryNotValidated
            && rejectedBoundary.StackPushesInInstructionOrder.IsEmpty
            && rejectedBoundary.InstructionsExecuted.IsEmpty
            && rejectedBoundary.ReceiverDwordAtOffset04Address is null
            && !rejectedBoundary.ReceiverDwordAtOffset04Read
            && !rejectedBoundary.ForwardCallReached
            && !rejectedBoundary.ReceiverDwordAtOffset08Read
            && !rejectedBoundary.StackCleanupProjected
            && !rejectedBoundary.CounterIncrementProjected
            && !rejectedBoundary.SecondCallReached
            && rejectedBoundary.UnreachedReceiverDwordAtOffset04CaptureProvided
            && rejectedBoundary.UnreachedReceiverDwordAtOffset18CaptureProvided
            && rejectedBoundary.UnreachedForwardHelperReturnCaptureProvided
            && rejectedBoundary.UnreachedReceiverDwordAtOffset08CaptureProvided
            && rejectedBoundary.UnreachedObjectDwordAtOffset1CCaptureProvided
            && rejectedBoundary.UnreachedSecondHelperReturnCaptureProvided
            && rejectedBoundary.StopAtRawFileOffsetExclusive == 0x00BE5BC4
            && complete.Outcome == UnreliableReceiveHighCountForwardOutcome.SecondHelperReturnedNormally);

        UnreliableReceiveHighCountForwardPlan missingReceiver04 =
            UnreliableReceiveHighCountForwardProjection.Project(
                completeCapture with { ReceiverDwordAtOffset04 = null });
        UnreliableReceiveHighCountForwardPlan missingReceiver18 =
            UnreliableReceiveHighCountForwardProjection.Project(
                completeCapture with { ReceiverDwordAtOffset18 = null });
        check("Missing early receiver fields preserve preceding pushes and reject later captures as unreached",
            missingReceiver04.Outcome
                == UnreliableReceiveHighCountForwardOutcome.UnreachedCaptureProvided
            && missingReceiver04.ReceiverDwordAtOffset04Address == 4
            && !missingReceiver04.ReceiverDwordAtOffset04Read
            && missingReceiver04.StackPushesInInstructionOrder.SequenceEqual(
                new uint[] { 0x2D8, 0x01BB8EC0 })
            && missingReceiver04.UnreachedReceiverDwordAtOffset18CaptureProvided
            && missingReceiver04.UnreachedForwardHelperReturnCaptureProvided
            && missingReceiver04.UnreachedReceiverDwordAtOffset08CaptureProvided
            && missingReceiver04.UnreachedObjectDwordAtOffset1CCaptureProvided
            && missingReceiver04.UnreachedSecondHelperReturnCaptureProvided
            && missingReceiver04.StopAtRawFileOffsetExclusive == 0x00BE5BCE
            && missingReceiver18.Outcome
                == UnreliableReceiveHighCountForwardOutcome.UnreachedCaptureProvided
            && missingReceiver18.ReceiverDwordAtOffset04Read
            && missingReceiver18.StackPushesInInstructionOrder.SequenceEqual(
                new uint[] { 0x2D8, 0x01BB8EC0, 0x11223344, 0x01BB8F0C, 0 })
            && !missingReceiver18.ReceiverDwordAtOffset18Read
            && missingReceiver18.UnreachedForwardHelperReturnCaptureProvided
            && missingReceiver18.UnreachedReceiverDwordAtOffset08CaptureProvided
            && missingReceiver18.UnreachedObjectDwordAtOffset1CCaptureProvided
            && missingReceiver18.UnreachedSecondHelperReturnCaptureProvided
            && missingReceiver18.StopAtRawFileOffsetExclusive == 0x00BE5BD8);

        UnreliableReceiveHighCountForwardPlan missingForwardReturn =
            UnreliableReceiveHighCountForwardProjection.Project(
                completeCapture with { ForwardHelperReturnedNormally = null });
        UnreliableReceiveHighCountForwardPlan failedForward =
            UnreliableReceiveHighCountForwardProjection.Project(
                completeCapture with { ForwardHelperReturnedNormally = false });
        check("The first helper return gates the independent reload and later cleanup",
            missingForwardReturn.Outcome
                == UnreliableReceiveHighCountForwardOutcome.UnreachedCaptureProvided
            && missingForwardReturn.ForwardCallReached
            && missingForwardReturn.ForwardHelperReturnedNormally is null
            && missingForwardReturn.StackPushesInInstructionOrder.Length == 6
            && missingForwardReturn.UnreachedReceiverDwordAtOffset08CaptureProvided
            && missingForwardReturn.UnreachedObjectDwordAtOffset1CCaptureProvided
            && missingForwardReturn.UnreachedSecondHelperReturnCaptureProvided
            && missingForwardReturn.StopAtRawFileOffsetExclusive == 0x00BE5BDB
            && failedForward.Outcome
                == UnreliableReceiveHighCountForwardOutcome.UnreachedCaptureProvided
            && failedForward.ForwardHelperReturnedNormally == false
            && !failedForward.ReceiverDwordAtOffset08Read
            && !failedForward.StackCleanupProjected
            && !failedForward.CounterIncrementProjected
            && failedForward.StopAtRawFileOffsetExclusive == 0x00BE5BDB);

        UnreliableReceiveHighCountForwardPlan missingReceiver08 =
            UnreliableReceiveHighCountForwardProjection.Project(
                completeCapture with { ReceiverDwordAtOffset08 = null });
        check("A missing independent EDI+8 reload stops before ECX formation and cleanup",
            missingReceiver08.Outcome
                == UnreliableReceiveHighCountForwardOutcome.UnreachedCaptureProvided
            && missingReceiver08.ForwardHelperReturnedNormally == true
            && missingReceiver08.ReceiverDwordAtOffset08Address == 8
            && !missingReceiver08.ReceiverDwordAtOffset08Read
            && missingReceiver08.LoadedObjectPointerEax is null
            && missingReceiver08.ReceiverPlus3CAddressPassedInEcx is null
            && !missingReceiver08.StackCleanupProjected
            && !missingReceiver08.CounterIncrementProjected
            && missingReceiver08.UnreachedObjectDwordAtOffset1CCaptureProvided
            && missingReceiver08.UnreachedSecondHelperReturnCaptureProvided
            && missingReceiver08.StackPushesInInstructionOrder.Length == 6
            && missingReceiver08.StopAtRawFileOffsetExclusive == 0x00BE5BE0);

        UnreliableReceiveHighCountForwardPlan missingCounter =
            UnreliableReceiveHighCountForwardProjection.Project(
                completeCapture with { ObjectDwordAtOffset1C = null });
        check("A missing counter capture retains the completed LEA and 0x18 cleanup without writing",
            missingCounter.Outcome
                == UnreliableReceiveHighCountForwardOutcome.UnreachedCaptureProvided
            && missingCounter.ReceiverDwordAtOffset08Read
            && missingCounter.LoadedObjectPointerEax == 0
            && missingCounter.ReceiverPlus3CAddressPassedInEcx == 0x3C
            && missingCounter.StackCleanupProjected
            && missingCounter.StackCleanupBytes == 0x18
            && missingCounter.ObjectDwordAtOffset1CAddress == 0x1C
            && !missingCounter.ObjectDwordAtOffset1CRead
            && !missingCounter.CounterIncrementProjected
            && missingCounter.CounterValueBeforeIncrement is null
            && missingCounter.CounterValueAfterIncrement is null
            && missingCounter.UnreachedSecondHelperReturnCaptureProvided
            && missingCounter.StopAtRawFileOffsetExclusive == 0x00BE5BE9);

        UnreliableReceiveHighCountForwardPlan missingSecondReturn =
            UnreliableReceiveHighCountForwardProjection.Project(
                completeCapture with { SecondHelperReturnedNormally = null });
        UnreliableReceiveHighCountForwardPlan failedSecondHelper =
            UnreliableReceiveHighCountForwardProjection.Project(
                completeCapture with { SecondHelperReturnedNormally = false });
        check("The second helper boundary is reported without inferring its return EAX",
            missingSecondReturn.Outcome
                == UnreliableReceiveHighCountForwardOutcome.MissingSecondHelperReturn
            && missingSecondReturn.SecondCallReached
            && missingSecondReturn.SecondHelperReturnedNormally is null
            && missingSecondReturn.CounterValueAfterIncrement == 0
            && !missingSecondReturn.NormalCallBoundaryReached
            && missingSecondReturn.StopAtRawFileOffsetExclusive == 0x00BE5BEC
            && failedSecondHelper.Outcome
                == UnreliableReceiveHighCountForwardOutcome.SecondHelperDidNotReturnNormally
            && failedSecondHelper.SecondCallReached
            && failedSecondHelper.SecondHelperReturnedNormally == false
            && failedSecondHelper.CounterIncrementProjected
            && !failedSecondHelper.NormalCallBoundaryReached
            && failedSecondHelper.StopAtRawFileOffsetExclusive == 0x00BE5BEC);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE5BC4, 0x00FE67C4, "68 D8 02 00 00", null, null),
            (0x00BE5BC9, 0x00FE67C9, "68 C0 8E BB 01", null, null),
            (0x00BE5BCE, 0x00FE67CE, "FF 77 04", null, null),
            (0x00BE5BD1, 0x00FE67D1, "68 0C 8F BB 01", null, null),
            (0x00BE5BD6, 0x00FE67D6, "6A 00", null, null),
            (0x00BE5BD8, 0x00FE67D8, "FF 77 18", null, null),
            (0x00BE5BDB, 0x00FE67DB, "E8 70 D3 FF FF", 0x00BE2F50, 0x00FE3B50),
            (0x00BE5BE0, 0x00FE67E0, "8B 47 08", null, null),
            (0x00BE5BE3, 0x00FE67E3, "8D 4F 3C", null, null),
            (0x00BE5BE6, 0x00FE67E6, "83 C4 18", null, null),
            (0x00BE5BE9, 0x00FE67E9, "FF 40 1C", null, null),
            (0x00BE5BEC, 0x00FE67EC, "E8 4F 08 00 00", 0x00BE6440, 0x00FE7040)
        };
        return UnreliableReceiveHighCountForwardProjection.Evidence
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.CallTargetRawFileOffset, instruction.CallTargetVirtualAddress))
            .SequenceEqual(expected);
    }
}
