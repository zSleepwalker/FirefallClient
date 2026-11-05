using Firefall.Reconstruction.Startup;

internal static class SocketReceivedQueuePopChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Queue-pop caller evidence pins the sample, raw range, and raw-offset-to-code-VA mapping",
            SocketReceivedQueuePopProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && SocketReceivedQueuePopProjection.EntryRawFileOffset == 0x00BD3EA5
            && SocketReceivedQueuePopProjection.EntryEndRawFileOffsetExclusive == 0x00BD3EBF
            && SocketReceivedQueuePopProjection.EntryEndRawFileOffsetExclusive
                - SocketReceivedQueuePopProjection.EntryRawFileOffset == 0x1A
            && SocketReceivedQueuePopProjection.EntryVirtualAddress == 0x00FD4AA5
            && SocketReceivedQueuePopProjection.EntryEndVirtualAddressExclusive == 0x00FD4ABF
            && SocketReceivedQueuePopProjection.EntryRawFileOffset
                + SocketReceivedQueuePopProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == SocketReceivedQueuePopProjection.EntryVirtualAddress
            && SocketReceivedQueuePopProjection.CapturedFunctionBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x1A
            && SocketReceivedQueuePopProjection.PopCallTargetRawFileOffset == 0x012CE400
            && SocketReceivedQueuePopProjection.PopCallTargetVirtualAddress == 0x016CF000
            && SocketReceivedQueuePopProjection.QueueReceiverOffset == 0x260);

        check("Queue-pop caller bytes pin zero-test and unsigned threshold branch targets",
            SocketReceivedQueuePopProjection.CapturedFunctionBytes
                == "8D 8E 60 02 00 00 E8 50 A5 6F 00 8B F8 85 FF 74 AA "
                    + "83 FF 10 0F 83 00 02 00 00"
            && SocketReceivedQueuePopProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.BranchTargetRawFileOffset))
                .SequenceEqual(new (uint RawFileOffset, string Bytes, uint? BranchTargetRawFileOffset)[]
                {
                    (0x00BD3EA5, "8D 8E 60 02 00 00", null),
                    (0x00BD3EAB, "E8 50 A5 6F 00", 0x012CE400),
                    (0x00BD3EB0, "8B F8", null),
                    (0x00BD3EB2, "85 FF", null),
                    (0x00BD3EB4, "74 AA", 0x00BD3E60),
                    (0x00BD3EB6, "83 FF 10", null),
                    (0x00BD3EB9, "0F 83 00 02 00 00", 0x00BD40BF)
                })
            && SocketReceivedQueuePopProjection.UnsignedThresholdBranchMnemonic == "JNB"
            && SocketReceivedQueuePopProjection.UnsignedThresholdCondition
                == "EDI >= 0x10 (unsigned)"
            && !SocketReceivedQueuePopProjection.SignedThresholdComparisonObserved
            && SocketReceivedQueuePopProjection.PopCallRawFileOffset + 5
                + 0x006FA550 == SocketReceivedQueuePopProjection.PopCallTargetRawFileOffset
            && unchecked((sbyte)0xAA) == -0x56
            && unchecked(
                (int)SocketReceivedQueuePopProjection.ZeroBranchRawFileOffset
                + 2
                + (sbyte)0xAA)
                == (int)SocketReceivedQueuePopProjection.ZeroBranchTargetRawFileOffset
            && SocketReceivedQueuePopProjection.UnsignedThresholdBranchRawFileOffset + 6
                + 0x200 == SocketReceivedQueuePopProjection.UnsignedThresholdBranchTargetRawFileOffset);

        SocketReceivedQueuePopPlan missingResult =
            SocketReceivedQueuePopProjection.Project(
                new SocketReceivedQueuePopCapture(0x10203040, null));
        check("A missing opaque pop result stops at the call-return boundary",
            missingResult.Outcome
                == SocketReceivedQueuePopOutcome.MissingPopResultAtCallBoundary
            && missingResult.Capture.OpaquePopResult is null
            && missingResult.BranchTargetRawFileOffset is null
            && missingResult.StopAtRawFileOffsetExclusive == 0x00BD3EB0);

        SocketReceivedQueuePopPlan zeroResult =
            SocketReceivedQueuePopProjection.Project(
                new SocketReceivedQueuePopCapture(0x10203040, 0));
        check("The zero-result test takes the observed zero branch",
            zeroResult.Outcome
                == SocketReceivedQueuePopOutcome.ZeroResultTakesZeroBranch
            && zeroResult.BranchTargetRawFileOffset == 0x00BD3E60
            && zeroResult.StopAtRawFileOffsetExclusive == 0x00BD3EB6);

        SocketReceivedQueuePopPlan wrappingReceiver =
            SocketReceivedQueuePopProjection.Project(
                new SocketReceivedQueuePopCapture(uint.MaxValue - 0x100, 1));
        check("The receiver argument is formed with wrapping 32-bit this+0x260 arithmetic",
            wrappingReceiver.QueueReceiver == 0x15F
            && wrappingReceiver.Capture.ReceiverPointer == uint.MaxValue - 0x100);

        SocketReceivedQueuePopPlan atThreshold =
            SocketReceivedQueuePopProjection.Project(
                new SocketReceivedQueuePopCapture(0, 0x10));
        SocketReceivedQueuePopPlan larger =
            SocketReceivedQueuePopProjection.Project(
                new SocketReceivedQueuePopCapture(0, 0x1234));
        SocketReceivedQueuePopPlan highBit =
            SocketReceivedQueuePopProjection.Project(
                new SocketReceivedQueuePopCapture(0, 0x80000000));
        check("Exactly 0x10, larger values, and high-bit values take the unsigned JNB branch",
            new[] { atThreshold, larger, highBit }.All(plan =>
                plan.Outcome == SocketReceivedQueuePopOutcome.UnsignedAtLeast16TakesBranch
                && plan.BranchTargetRawFileOffset == 0x00BD40BF
                && plan.StopAtRawFileOffsetExclusive == 0x00BD3EBF));

        SocketReceivedQueuePopPlan belowThreshold =
            SocketReceivedQueuePopProjection.Project(
                new SocketReceivedQueuePopCapture(0, 0x0F));
        SocketReceivedQueuePopPlan smallStatusTwo =
            SocketReceivedQueuePopProjection.Project(
                new SocketReceivedQueuePopCapture(0, 2));
        SocketReceivedQueuePopPlan smallStatusThree =
            SocketReceivedQueuePopProjection.Project(
                new SocketReceivedQueuePopCapture(0, 3));
        check("Values below 0x10 stop at the branch fall-through without projecting later status handling",
            new[] { belowThreshold, smallStatusTwo, smallStatusThree }.All(plan =>
                plan.Outcome == SocketReceivedQueuePopOutcome.UnsignedBelow16FallsThrough
                && plan.BranchTargetRawFileOffset is null
                && plan.StopAtRawFileOffsetExclusive == 0x00BD3EBF));
    }
}
