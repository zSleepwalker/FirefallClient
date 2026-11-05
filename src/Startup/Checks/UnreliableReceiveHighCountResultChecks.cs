using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveHighCountResultChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composed window joins result stores directly to the high-count forward entry",
            UnreliableReceiveHighCountResultProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveHighCountResultProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveHighCountResultProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveHighCountResultProjection.EntryRawFileOffset == 0x00BE5BAC
            && UnreliableReceiveHighCountResultProjection.EntryVirtualAddress == 0x00FE67AC
            && UnreliableReceiveHighCountResultProjection.HighCountForwardEntryRawFileOffset
                == 0x00BE5BC4
            && UnreliableReceiveHighCountResultProjection.EndRawFileOffsetExclusive == 0x00BE5BF1
            && UnreliableReceiveHighCountResultProjection.EndVirtualAddressExclusive == 0x00FE67F1
            && UnreliableReceiveHighCountResultProjection.RawFileOffsetToVirtualAddress(0x00BE5BAC)
                == 0x00FE67AC
            && UnreliableReceiveHighCountResultProjection.RawFileOffsetToVirtualAddress(0x00BE5BF1)
                == 0x00FE67F1
            && UnreliableReceiveHighCountResultProjection.CapturedWindowBytes
                == UnreliableReceiveResultStoresProjection.CapturedWindowBytes
                    + " "
                    + UnreliableReceiveHighCountForwardProjection.CapturedWindowBytes
            && UnreliableReceiveHighCountResultProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 69
            && UnreliableReceiveHighCountResultProjection.Instructions.Length == 19
            && UnreliableReceiveHighCountResultProjection.Instructions[0].RawFileOffset
                == 0x00BE5BAC
            && UnreliableReceiveHighCountResultProjection.Instructions[6].RawFileOffset
                == 0x00BE5BC2
            && UnreliableReceiveHighCountResultProjection.Instructions[7].RawFileOffset
                == 0x00BE5BC4
            && UnreliableReceiveHighCountResultProjection.Instructions[^1].RawFileOffset
                == 0x00BE5BEC
            && UnreliableReceiveHighCountResultProjection.Instructions.All(instruction =>
                instruction.VirtualAddress
                    == UnreliableReceiveHighCountResultProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)));

        UnreliableReceiveHighCountResultPlan complete =
            UnreliableReceiveHighCountResultProjection.Project(
                new(
                    new(0x1000, 0x2000, 0x3000, 0xAABBCCDD, 0x1234FFFF, 0x00010000),
                    new(true, 0x1000, 0x11223344, 0x55667788, true, 0, 0xCAFEBABE, true)));
        check("Only the exact at-or-above result outcome admits a matching validated forward boundary",
            complete.ResultStoresPlan.Outcome
                == UnreliableReceiveResultStoresOutcome.CounterAtOrAboveThreshold
            && complete.CounterAtOrAboveThreshold
            && complete.ReceiverIdentityMatches
            && complete.HighCountBoundaryAdmitted
            && complete.Outcome
                == UnreliableReceiveHighCountResultOutcome.ForwardCompletedNormally
            && complete.ResultStoresPlan.Receiver4StoredValue == 0xAABBCCDD
            && complete.ResultStoresPlan.FrameMinus4LowWordStoredValue == 0xFFFF
            && complete.HighCountForwardPlan.Capture.ReceiverDwordAtOffset04 == 0x11223344
            && complete.HighCountForwardPlan.Capture.ReceiverDwordAtOffset04
                != complete.ResultStoresPlan.CapturedReceiver4
            && complete.HighCountForwardPlan.ReceiverDwordAtOffset08Read
            && complete.HighCountForwardPlan.LoadedObjectPointerEax == 0
            && complete.HighCountForwardPlan.ObjectDwordAtOffset1CAddress == 0x1C
            && complete.HighCountForwardPlan.CounterValueBeforeIncrement == 0xCAFEBABE
            && complete.HighCountForwardPlan.CounterIncrementProjected
            && complete.HighCountForwardPlan.CounterValueAfterIncrement == 0xCAFEBABF
            && complete.Complete
            && complete.StopAtRawFileOffsetExclusive == 0x00BE5BF1);

        UnreliableReceiveHighCountResultPlan belowThreshold =
            UnreliableReceiveHighCountResultProjection.Project(
                new(
                    new(0x1000, 0x2000, 0x3000, 0xAABBCCDD, 0x1234FFFF, 0x0000FFFF),
                    new(true, 0x1000, 1, 2, true, 3, 4, true)));
        check("A below-threshold result preserves both stores and suppresses every high-count push and read",
            belowThreshold.ResultStoresPlan.Outcome
                == UnreliableReceiveResultStoresOutcome.CounterBelowThreshold
            && !belowThreshold.CounterAtOrAboveThreshold
            && !belowThreshold.HighCountBoundaryAdmitted
            && belowThreshold.Outcome
                == UnreliableReceiveHighCountResultOutcome.ResultStoresDidNotReachHighCountBoundary
            && belowThreshold.ResultStoresPlan.Receiver4Stored
            && belowThreshold.ResultStoresPlan.Receiver4StoredValue == 0xAABBCCDD
            && belowThreshold.ResultStoresPlan.FrameMinus4LowWordStored
            && belowThreshold.ResultStoresPlan.FrameMinus4LowWordStoredValue == 0xFFFF
            && belowThreshold.HighCountForwardPlan.Outcome
                == UnreliableReceiveHighCountForwardOutcome.EntryBoundaryNotValidated
            && belowThreshold.HighCountForwardPlan.StackPushesInInstructionOrder.IsEmpty
            && !belowThreshold.HighCountForwardPlan.ReceiverDwordAtOffset04Read
            && !belowThreshold.HighCountForwardPlan.ReceiverDwordAtOffset08Read
            && !belowThreshold.HighCountForwardPlan.ObjectDwordAtOffset1CRead
            && !belowThreshold.HighCountForwardPlan.CounterIncrementProjected
            && belowThreshold.HighCountForwardPlan.UnreachedReceiverDwordAtOffset04CaptureProvided
            && belowThreshold.StopAtRawFileOffsetExclusive == 0x00BE5C0F);

        UnreliableReceiveHighCountResultPlan missingForwardRead =
            UnreliableReceiveHighCountResultProjection.Project(
                new(
                    new(0x1000, 0x2000, 0x3000, 0xAABBCCDD, 0x12340001, 0x00010000),
                    new(true, 0x1000, null, 0x55667788, true, 0x4000, 9, true)));
        UnreliableReceiveHighCountResultPlan invalidForwardBoundary =
            UnreliableReceiveHighCountResultProjection.Project(
                new(
                    new(0x1000, 0x2000, 0x3000, 0xAABBCCDD, 0x12340001, 0x00010000),
                    new(false, 0x1000, 0x11223344, 0x55667788, true, 0x4000, 9, true)));
        check("Missing high-count captures and an unvalidated boundary retain both stores without inventing forwarding",
            missingForwardRead.ResultStoresPlan.Receiver4Stored
            && missingForwardRead.ResultStoresPlan.FrameMinus4LowWordStored
            && missingForwardRead.HighCountForwardPlan.StackPushesInInstructionOrder
                .SequenceEqual(new uint[] { 0x2D8, 0x01BB8EC0 })
            && !missingForwardRead.HighCountForwardPlan.ReceiverDwordAtOffset04Read
            && missingForwardRead.HighCountForwardPlan.UnreachedReceiverDwordAtOffset18CaptureProvided
            && missingForwardRead.StopAtRawFileOffsetExclusive == 0x00BE5BCE
            && invalidForwardBoundary.ResultStoresPlan.Receiver4Stored
            && invalidForwardBoundary.ResultStoresPlan.FrameMinus4LowWordStored
            && invalidForwardBoundary.Outcome
                == UnreliableReceiveHighCountResultOutcome.HighCountBoundaryNotValidated
            && !invalidForwardBoundary.HighCountBoundaryAdmitted
            && invalidForwardBoundary.HighCountForwardPlan.StackPushesInInstructionOrder.IsEmpty
            && !invalidForwardBoundary.HighCountForwardPlan.ReceiverDwordAtOffset04Read
            && invalidForwardBoundary.StopAtRawFileOffsetExclusive == 0x00BE5BC4);

        UnreliableReceiveHighCountResultPlan identityMismatch =
            UnreliableReceiveHighCountResultProjection.Project(
                new(
                    new(0x1000, 0x2000, 0x3000, 0xAABBCCDD, 0x12340001, 0x00010000),
                    new(true, 0x1004, 0x11223344, 0x55667788, true, 0x4000, 9, true)));
        check("Receiver identity mismatch blocks forwarding without discarding the completed stores",
            identityMismatch.ResultStoresPlan.Receiver4Stored
            && identityMismatch.ResultStoresPlan.FrameMinus4LowWordStored
            && !identityMismatch.ReceiverIdentityMatches
            && !identityMismatch.HighCountBoundaryAdmitted
            && identityMismatch.Outcome
                == UnreliableReceiveHighCountResultOutcome.ReceiverIdentityMismatch
            && identityMismatch.HighCountForwardPlan.Outcome
                == UnreliableReceiveHighCountForwardOutcome.EntryBoundaryNotValidated
            && identityMismatch.HighCountForwardPlan.StackPushesInInstructionOrder.IsEmpty
            && !identityMismatch.HighCountForwardPlan.ReceiverDwordAtOffset04Read
            && identityMismatch.HighCountForwardPlan.UnreachedReceiverDwordAtOffset04CaptureProvided
            && identityMismatch.StopAtRawFileOffsetExclusive == 0x00BE5BC4);

        UnreliableReceiveHighCountResultPlan partialForward =
            UnreliableReceiveHighCountResultProjection.Project(
                new(
                    new(0x1000, 0x2000, 0x3000, 0xAABBCCDD, 0x12340001, 0x00010000),
                    new(true, 0x1000, 0x11223344, 0x55667788, true, 0x4000, null, null)));
        check("An admitted partial forward keeps its independent reload, cleanup, and pending increment nested",
            partialForward.HighCountBoundaryAdmitted
            && partialForward.ResultStoresPlan.Receiver4Stored
            && partialForward.ResultStoresPlan.FrameMinus4LowWordStored
            && partialForward.HighCountForwardPlan.ReceiverDwordAtOffset08Read
            && partialForward.HighCountForwardPlan.LoadedObjectPointerEax == 0x4000
            && partialForward.HighCountForwardPlan.StackCleanupProjected
            && partialForward.HighCountForwardPlan.StackCleanupBytes == 0x18
            && partialForward.HighCountForwardPlan.ObjectDwordAtOffset1CAddress == 0x401C
            && !partialForward.HighCountForwardPlan.ObjectDwordAtOffset1CRead
            && !partialForward.HighCountForwardPlan.CounterIncrementProjected
            && !partialForward.HighCountForwardPlan.UnreachedSecondHelperReturnCaptureProvided
            && partialForward.Outcome == UnreliableReceiveHighCountResultOutcome.ForwardIncomplete
            && partialForward.StopAtRawFileOffsetExclusive == 0x00BE5BE9);
    }
}
