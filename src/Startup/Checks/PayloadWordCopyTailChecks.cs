using Firefall.Reconstruction.Startup;

internal static class PayloadWordCopyTailChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE sample hash, tail range, instruction bytes, and RET 4 boundary are pinned",
            PayloadWordCopyTailProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadWordCopyTailProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadWordCopyTailProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PayloadWordCopyTailProjection.TailStartRawFileOffset == 0x00BE636E
            && PayloadWordCopyTailProjection.Ret4BoundaryRawFileOffsetExclusive
                == 0x00BE6391
            && PayloadWordCopyTailProjection.TailStartVirtualAddress == 0x00FE6F6E
            && PayloadWordCopyTailProjection.RawFileOffsetToVirtualAddress(0x00BE6391)
                == 0x00FE6F91
            && string.Concat(PayloadWordCopyTailProjection.CapturedTailBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B460483E7078B04188D0C785F5B85C974098B4508668B00668901FF46105E5DC20400"
            && PayloadWordCopyTailProjection.Instructions.SequenceEqual(
                ExpectedInstructions()));

        check("The retained EDI low three bits select each of eight candidate WORD slots",
            VerifyAllEdiMasks());

        PayloadWordCopyTailPlan zeroCandidate =
            PayloadWordCopyTailProjection.Project(
                new(0x1000, 0, 0, 0x2000, 0, 0, null, null, 5));
        PayloadWordCopyTailPlan zeroSlotNonzeroIndex =
            PayloadWordCopyTailProjection.Project(
                new(0x1000, 1, 0, 0x2000, 0x2000, 0, 0, 0xFFFF, 8));
        PayloadWordCopyTailPlan wrappedCandidateZero =
            PayloadWordCopyTailProjection.Project(
                new(0xFFFFFFFC, 1, 0, 0xFFFFFFF8, 0xFFFFFFFC, 0xFFFFFFFE,
                    null, null, uint.MaxValue));

        check("Zero candidate skips only the optional WORD copy; a zero slot pointer can still yield a nonzero candidate",
            zeroCandidate.Outcome
                == PayloadWordCopyTailOutcome.CandidateZeroSkippedWordCopy
            && zeroCandidate.CandidateSlotAddress == 0
            && zeroCandidate.CandidateIsZero == true
            && !zeroCandidate.StackArgumentPointerRead
            && !zeroCandidate.WordRead
            && !zeroCandidate.WordCopyPerformed
            && zeroCandidate.IncrementOldDwordRead
            && zeroCandidate.IncrementedDwordValue == 6
            && zeroSlotNonzeroIndex.CandidateSlotAddress == 2
            && zeroSlotNonzeroIndex.CandidateIsZero == false
            && zeroSlotNonzeroIndex.StackArgumentPointerRead
            && zeroSlotNonzeroIndex.StackArgumentPointerReadAddress == 0x2008
            && zeroSlotNonzeroIndex.WordRead
            && zeroSlotNonzeroIndex.WordReadAddress == 0
            && zeroSlotNonzeroIndex.WordCopyPerformed
            && zeroSlotNonzeroIndex.WordCopyDestinationAddress == 2
            && zeroSlotNonzeroIndex.WordCopyValue == 0xFFFF
            && wrappedCandidateZero.CandidateSlotAddress == 0
            && wrappedCandidateZero.Outcome
                == PayloadWordCopyTailOutcome.CandidateZeroSkippedWordCopy
            && wrappedCandidateZero.ReceiverDword4ReadAddress == 0
            && wrappedCandidateZero.SlotPointerReadAddress == 0xFFFFFFFC
            && wrappedCandidateZero.IncrementOldDwordReadAddress == 0x0C
            && wrappedCandidateZero.IncrementedDwordValue == 0);

        PayloadWordCopyTailPlan wrappedAddresses =
            PayloadWordCopyTailProjection.Project(
                new(uint.MaxValue, 1, 4, 0xFFFFFFF8, 0, uint.MaxValue,
                    0, 0, uint.MaxValue));
        check("All address additions and the captured DWORD increment wrap at 32 bits",
            PayloadWordCopyTailProjection.ReceiverDwordAddress(uint.MaxValue, 4) == 3
            && PayloadWordCopyTailProjection.SlotPointerReadAddress(uint.MaxValue, 1) == 0
            && PayloadWordCopyTailProjection.CandidateAddress(uint.MaxValue, 1) == 1
            && wrappedAddresses.ReceiverDword4ReadAddress == 3
            && wrappedAddresses.SlotPointerReadAddress == 4
            && wrappedAddresses.StackArgumentPointerReadAddress == 0
            && wrappedAddresses.WordReadAddress == 0
            && wrappedAddresses.CandidateSlotAddress == 1
            && wrappedAddresses.WordCopyDestinationAddress == 1
            && wrappedAddresses.CounterDwordWriteAddress == 15
            && wrappedAddresses.IncrementedDwordValue == 0);

        PayloadWordCopyTailPlan missingReceiver4 =
            PayloadWordCopyTailProjection.Project(
                new(0x1000, 0, 0, 0x2000, null, null, null, null, null));
        PayloadWordCopyTailPlan laterReceiver4 =
            PayloadWordCopyTailProjection.Project(
                new(0x1000, 0, 0, 0x2000, null, 0, null, null, null));
        PayloadWordCopyTailPlan missingSlotPointer =
            PayloadWordCopyTailProjection.Project(
                new(0x1000, 0, 0, 0x2000, 0x1000, null, null, null, null));
        PayloadWordCopyTailPlan laterSlotPointer =
            PayloadWordCopyTailProjection.Project(
                new(0x1000, 0, 0, 0x2000, 0x1000, null, 0, null, null));
        PayloadWordCopyTailPlan missingStackArgument =
            PayloadWordCopyTailProjection.Project(
                new(0x1000, 0, 0, 0x2000, 0x1000, 0x1000, null, null, null));
        PayloadWordCopyTailPlan laterStackArgument =
            PayloadWordCopyTailProjection.Project(
                new(0x1000, 0, 0, 0x2000, 0x1000, 0x1000, null, 0, null));
        PayloadWordCopyTailPlan missingWord =
            PayloadWordCopyTailProjection.Project(
                new(0x1000, 0, 0, 0x2000, 0x1000, 0x1000, 0x3000, null, null));
        PayloadWordCopyTailPlan laterWord =
            PayloadWordCopyTailProjection.Project(
                new(0x1000, 0, 0, 0x2000, 0x1000, 0x1000, 0x3000, null, 0));
        PayloadWordCopyTailPlan missingCounter =
            PayloadWordCopyTailProjection.Project(
                new(0x1000, 0, 0, 0x2000, 0x1000, 0x1000, 0x3000, 0xFFFF, null));
        PayloadWordCopyTailPlan skippedCopyCapture =
            PayloadWordCopyTailProjection.Project(
                new(0x1000, 0, 0, 0x2000, 0x1000, 0, 0, null, 5));

        check("Missing reads stop in instruction order and reject captures from later or skipped reads",
            missingReceiver4.Outcome
                == PayloadWordCopyTailOutcome.MissingReceiverDword4Capture
            && missingReceiver4.StopAtRawFileOffsetExclusive == 0x00BE636E
            && laterReceiver4.Outcome
                == PayloadWordCopyTailOutcome.UnreachedCaptureProvided
            && !laterReceiver4.SlotPointerRead
            && missingSlotPointer.Outcome
                == PayloadWordCopyTailOutcome.MissingSlotPointerCapture
            && missingSlotPointer.SlotPointerReadAddress == 0x1000
            && !missingSlotPointer.SlotPointerRead
            && laterSlotPointer.Outcome
                == PayloadWordCopyTailOutcome.UnreachedCaptureProvided
            && missingStackArgument.Outcome
                == PayloadWordCopyTailOutcome.MissingStackArgumentPointerCapture
            && missingStackArgument.StackArgumentPointerReadAddress == 0x2008
            && !missingStackArgument.StackArgumentPointerRead
            && laterStackArgument.Outcome
                == PayloadWordCopyTailOutcome.UnreachedCaptureProvided
            && missingWord.Outcome == PayloadWordCopyTailOutcome.MissingWordCapture
            && missingWord.WordReadAddress == 0x3000
            && !missingWord.WordRead
            && laterWord.Outcome
                == PayloadWordCopyTailOutcome.UnreachedCaptureProvided
            && missingCounter.Outcome
                == PayloadWordCopyTailOutcome.MissingIncrementOldDwordCapture
            && missingCounter.WordCopyPerformed
            && missingCounter.WordCopyDestinationAddress == 0x1000
            && missingCounter.IncrementOldDwordReadAddress == 0x1010
            && !missingCounter.IncrementOldDwordRead
            && missingCounter.CounterDwordWriteAddress is null
            && missingCounter.Ret4BoundaryRawFileOffsetExclusive is null
            && skippedCopyCapture.Outcome
                == PayloadWordCopyTailOutcome.UnreachedCaptureProvided
            && !skippedCopyCapture.StackArgumentPointerRead
            && !skippedCopyCapture.WordRead);

        PayloadWordCopyTailPlan completedCopy =
            PayloadWordCopyTailProjection.Project(
                new(0x2000, 7, 4, 0x3000, 0x1000, 0x4000, 0x5000,
                    0xFFFF, uint.MaxValue));
        check("The DWORD counter write and RET 4 boundary are exposed only after the increment capture",
            completedCopy.Outcome
                == PayloadWordCopyTailOutcome.WordCopiedAndCounterIncremented
            && completedCopy.WordCopyPerformed
            && completedCopy.WordCopyDestinationAddress == 0x400E
            && completedCopy.WordCopyValue == 0xFFFF
            && completedCopy.IncrementOldDwordReadAddress == 0x2010
            && completedCopy.IncrementOldDwordRead
            && completedCopy.CounterDwordWriteAddress == 0x2010
            && completedCopy.IncrementedDwordValue == 0
            && completedCopy.Ret4BoundaryRawFileOffsetExclusive == 0x00BE6391
            && completedCopy.StopAtRawFileOffsetExclusive == 0x00BE6391);
    }

    private static bool VerifyAllEdiMasks() =>
        Enumerable.Range(0, 8).All(index =>
        {
            uint edi = 0xFFFFFFF8u | (uint)index;
            PayloadWordCopyTailPlan plan =
                PayloadWordCopyTailProjection.Project(
                    new(0x1000, edi, 0, 0x2000, 0x2000, 0x3000,
                        0x4000, (ushort)index, 0));
            return plan.MaskedEdi == (uint)index
                && plan.CandidateSlotAddress == 0x3000u + ((uint)index * 2)
                && plan.WordCopyDestinationAddress == plan.CandidateSlotAddress
                && plan.WordCopyValue == index
                && plan.Outcome
                    == PayloadWordCopyTailOutcome.WordCopiedAndCounterIncremented;
        });

    private static PayloadWordCopyTailInstructionEvidence[] ExpectedInstructions() =>
    [
        new("Read the receiver DWORD at +4", 0x00BE636E, 0x00FE6F6E, "8B 46 04"),
        new("Mask retained EDI to its low three bits", 0x00BE6371, 0x00FE6F71, "83 E7 07"),
        new("Read the slot pointer at receiverDword4+retainedEBX", 0x00BE6374, 0x00FE6F74, "8B 04 18"),
        new("Form slotPointer+(maskedEdi*2) as the candidate address", 0x00BE6377, 0x00FE6F77, "8D 0C 78"),
        new("Restore retained EDI and EBX", 0x00BE637A, 0x00FE6F7A, "5F 5B"),
        new("Test the candidate address for zero", 0x00BE637C, 0x00FE6F7C, "85 C9"),
        new("A zero candidate skips the stack-argument read and WORD copy", 0x00BE637E, 0x00FE6F7E, "74 09", 0x00BE6389, 0x00FE6F89),
        new("Read the stack argument pointer from framePointer+8", 0x00BE6380, 0x00FE6F80, "8B 45 08"),
        new("Read a WORD from the captured stack argument pointer", 0x00BE6383, 0x00FE6F83, "66 8B 00"),
        new("Write the captured WORD to the candidate address", 0x00BE6386, 0x00FE6F86, "66 89 01"),
        new("Increment the receiver DWORD at +0x10 with wrapping arithmetic", 0x00BE6389, 0x00FE6F89, "FF 46 10"),
        new("Restore ESI and EBP, then return while popping four stack bytes", 0x00BE638C, 0x00FE6F8C, "5E 5D C2 04 00")
    ];
}
