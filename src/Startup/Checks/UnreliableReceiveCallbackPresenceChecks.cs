using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveCallbackPresenceChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned sample, 32-byte raw window, and raw-to-VA mapping are exact",
            UnreliableReceiveCallbackPresenceProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveCallbackPresenceProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveCallbackPresenceProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveCallbackPresenceProjection.EntryRawFileOffset == 0x00BE5B32
            && UnreliableReceiveCallbackPresenceProjection.EntryVirtualAddress == 0x00FE6732
            && UnreliableReceiveCallbackPresenceProjection.ContinuationRawFileOffset == 0x00BE5B52
            && UnreliableReceiveCallbackPresenceProjection.ContinuationVirtualAddress == 0x00FE6752
            && UnreliableReceiveCallbackPresenceProjection.EndRawFileOffsetExclusive == 0x00BE5B52
            && UnreliableReceiveCallbackPresenceProjection.RawFileOffsetToVirtualAddress(0x00BE5B32)
                == 0x00FE6732
            && UnreliableReceiveCallbackPresenceProjection.RawFileOffsetToVirtualAddress(0x00BE5B52)
                == 0x00FE6752
            && UnreliableReceiveCallbackPresenceProjection.CapturedWindowBytes
                == "83 7F 14 00 75 1A 6A 00 68 CA 02 00 00 68 06 8C B5 01 "
                    + "68 07 8C B5 01 FF 15 A0 5E C1 01 83 C4 10"
            && UnreliableReceiveCallbackPresenceProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 32
            && UnreliableReceiveCallbackPresenceProjection.NonzeroBranchDestination(0x00BE5B36)
                == 0x00BE5B52
            && UnreliableReceiveCallbackPresenceProjection.IndirectCallPointerCellVirtualAddress
                == 0x01C15EA0
            && InstructionsMatch());

        UnreliableReceiveCallbackPresencePlan rejectedEntry =
            UnreliableReceiveCallbackPresenceProjection.Project(
                new(0x10203040, false, 0, true));
        check("A false entry-boundary validation rejects without projection effects or flags",
            rejectedEntry.Outcome
                == UnreliableReceiveCallbackPresenceOutcome.EntryBoundaryNotValidated
            && rejectedEntry.ReceiverDwordAt14Address is null
            && !rejectedEntry.ReceiverDwordAt14Read
            && rejectedEntry.CapturedReceiverDwordAt14 is null
            && !rejectedEntry.UnreachedDiagnosticCallReturnProvided
            && rejectedEntry.StackPushesInInstructionOrder.IsEmpty
            && !rejectedEntry.IndirectCallReached
            && rejectedEntry.DiagnosticCallReturnedNormally is null
            && !rejectedEntry.NormalCallBoundaryReached
            && !rejectedEntry.StackCleanupReached
            && rejectedEntry.StackCleanupBytes is null
            && rejectedEntry.SelectedDestinationRawFileOffset is null
            && rejectedEntry.StopAtRawFileOffsetExclusive == 0x00BE5B32);

        check("A validated boundary does not presume which selector/callback path arrived",
            UnreliableReceiveCallbackPresenceProjection.Project(
                new(0x10203040, true, 0, true)).Outcome
                == UnreliableReceiveCallbackPresenceOutcome.NormalReturnReached
            && UnreliableReceiveCallbackPresenceProjection.Project(
                new(0x10203040, true, 1, null)).Outcome
                == UnreliableReceiveCallbackPresenceOutcome.NonzeroReceiverDword);

        UnreliableReceiveCallbackPresencePlan missingReceiverRead =
            UnreliableReceiveCallbackPresenceProjection.Project(new(0x10203040, true, null, null));
        UnreliableReceiveCallbackPresencePlan diagnosticAfterMissingRead =
            UnreliableReceiveCallbackPresenceProjection.Project(new(0x10203040, true, null, true));
        check("A missing first receiver read blocks the route and any later diagnostic capture",
            missingReceiverRead.Outcome
                == UnreliableReceiveCallbackPresenceOutcome.MissingReceiverDwordAt14
            && missingReceiverRead.ReceiverDwordAt14Address == 0x10203054
            && !missingReceiverRead.ReceiverDwordAt14Read
            && !missingReceiverRead.IndirectCallReached
            && missingReceiverRead.StackPushesInInstructionOrder.IsEmpty
            && missingReceiverRead.StopAtRawFileOffsetExclusive == 0x00BE5B32
            && diagnosticAfterMissingRead.Outcome
                == UnreliableReceiveCallbackPresenceOutcome.UnreachedCaptureProvided
            && diagnosticAfterMissingRead.UnreachedDiagnosticCallReturnProvided
            && !diagnosticAfterMissingRead.IndirectCallReached);

        UnreliableReceiveCallbackPresencePlan nonzeroReceiver14 =
            UnreliableReceiveCallbackPresenceProjection.Project(
                new(0x10203040, true, 1, null));
        UnreliableReceiveCallbackPresencePlan maxReceiver14 =
            UnreliableReceiveCallbackPresenceProjection.Project(
                new(0x10203040, true, uint.MaxValue, null));
        UnreliableReceiveCallbackPresencePlan unreachableDiagnostic =
            UnreliableReceiveCallbackPresenceProjection.Project(
                new(0x10203040, true, 1, false));
        check("Any nonzero receiver DWORD skips the callback path and reaches the branch destination",
            nonzeroReceiver14.Outcome == UnreliableReceiveCallbackPresenceOutcome.NonzeroReceiverDword
            && nonzeroReceiver14.CapturedReceiverDwordAt14 == 1
            && nonzeroReceiver14.SelectedDestinationRawFileOffset == 0x00BE5B52
            && nonzeroReceiver14.StackPushesInInstructionOrder.IsEmpty
            && !nonzeroReceiver14.IndirectCallReached
            && !nonzeroReceiver14.StackCleanupReached
            && maxReceiver14.Outcome == UnreliableReceiveCallbackPresenceOutcome.NonzeroReceiverDword
            && maxReceiver14.CapturedReceiverDwordAt14 == uint.MaxValue
            && unreachableDiagnostic.Outcome
                == UnreliableReceiveCallbackPresenceOutcome.UnreachedCaptureProvided
            && unreachableDiagnostic.UnreachedDiagnosticCallReturnProvided
            && unreachableDiagnostic.SelectedDestinationRawFileOffset == 0x00BE5B52
            && !unreachableDiagnostic.IndirectCallReached
            && !unreachableDiagnostic.StackCleanupReached);

        UnreliableReceiveCallbackPresencePlan missingDiagnostic =
            UnreliableReceiveCallbackPresenceProjection.Project(
                new(0x10203040, true, 0, null));
        UnreliableReceiveCallbackPresencePlan nonReturningDiagnostic =
            UnreliableReceiveCallbackPresenceProjection.Project(
                new(0x10203040, true, 0, false));
        UnreliableReceiveCallbackPresencePlan normalDiagnostic =
            UnreliableReceiveCallbackPresenceProjection.Project(
                new(0x10203040, true, 0, true));
        uint[] expectedPushes = { 0, 0x2CA, 0x01B58C06, 0x01B58C07 };
        check("Zero enters the opaque call; only its normal return reaches the 0x10-byte cleanup",
            missingDiagnostic.Outcome
                == UnreliableReceiveCallbackPresenceOutcome.MissingDiagnosticCallReturn
            && missingDiagnostic.ReceiverDwordAt14Read
            && missingDiagnostic.CapturedReceiverDwordAt14 == 0
            && missingDiagnostic.StackPushesInInstructionOrder.SequenceEqual(expectedPushes)
            && missingDiagnostic.IndirectCallReached
            && !missingDiagnostic.NormalCallBoundaryReached
            && !missingDiagnostic.StackCleanupReached
            && missingDiagnostic.StackCleanupBytes is null
            && missingDiagnostic.StopAtRawFileOffsetExclusive == 0x00BE5B49
            && nonReturningDiagnostic.Outcome
                == UnreliableReceiveCallbackPresenceOutcome.DiagnosticCallDidNotReturnNormally
            && nonReturningDiagnostic.IndirectCallReached
            && nonReturningDiagnostic.DiagnosticCallReturnedNormally == false
            && !nonReturningDiagnostic.StackCleanupReached
            && nonReturningDiagnostic.StackCleanupBytes is null
            && nonReturningDiagnostic.SelectedDestinationRawFileOffset is null
            && nonReturningDiagnostic.StopAtRawFileOffsetExclusive == 0x00BE5B49
            && normalDiagnostic.Outcome
                == UnreliableReceiveCallbackPresenceOutcome.NormalReturnReached
            && normalDiagnostic.IndirectCallReached
            && normalDiagnostic.NormalCallBoundaryReached
            && normalDiagnostic.StackCleanupReached
            && normalDiagnostic.StackCleanupBytes == 0x10
            && normalDiagnostic.SelectedDestinationRawFileOffset == 0x00BE5B52
            && normalDiagnostic.StopAtRawFileOffsetExclusive == 0x00BE5B52);

        UnreliableReceiveCallbackPresencePlan wrappedReceiverAddress =
            UnreliableReceiveCallbackPresenceProjection.Project(
                new(uint.MaxValue - 0x13, true, null, null));
        check("The independently captured receiver field address uses DWORD wraparound",
            wrappedReceiverAddress.ReceiverDwordAt14Address == 0
            && UnreliableReceiveCallbackPresenceProjection.ReceiverDwordAt14Address(uint.MaxValue)
                == 0x13);
    }

    private static bool InstructionsMatch()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes)[]
        {
            (0x00BE5B32, 0x00FE6732, "83 7F 14 00"),
            (0x00BE5B36, 0x00FE6736, "75 1A"),
            (0x00BE5B38, 0x00FE6738, "6A 00"),
            (0x00BE5B3A, 0x00FE673A, "68 CA 02 00 00"),
            (0x00BE5B3F, 0x00FE673F, "68 06 8C B5 01"),
            (0x00BE5B44, 0x00FE6744, "68 07 8C B5 01"),
            (0x00BE5B49, 0x00FE6749, "FF 15 A0 5E C1 01"),
            (0x00BE5B4F, 0x00FE674F, "83 C4 10")
        };
        return UnreliableReceiveCallbackPresenceProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
            .SequenceEqual(expected);
    }
}
