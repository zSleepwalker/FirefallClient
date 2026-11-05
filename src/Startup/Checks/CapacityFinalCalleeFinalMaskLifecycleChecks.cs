using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeFinalMaskLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition uses the exact equal handoff and bounded mask successor",
            CapacityFinalCalleeFinalMaskLifecycleProjection.UpstreamEntryRawFileOffset
                == 0x012C9160
            && CapacityFinalCalleeFinalMaskLifecycleProjection.MaskTailEntryRawFileOffset
                == 0x012C9167
            && CapacityFinalCalleeFinalMaskLifecycleProjection
                .UpstreamEqualDestinationRawFileOffset == 0x012C9167
            && CapacityFinalCalleeFinalMaskLifecycleProjection
                .UpstreamUnequalDestinationRawFileOffset == 0x012C917C
            && CapacityFinalCalleeFinalMaskLifecycleProjection
                .MaskTailSuccessorRawFileOffset == 0x012C92BD
            && Normalize(CapacityFinalCalleeAlternateFinalFrameGateProjection
                .CapturedWindowBytes) == "8B45EC3BC17515"
            && Normalize(CapacityFinalCalleeAlternateMaskTailProjection
                .CapturedWindowBytes)
                == "8B45F88B4DFCD3E0F7D02105C8ADED01E941010000");

        CapacityFinalCalleeFinalMaskLifecyclePlan countZero =
            Project(0x10, 0x80000001, 0x80000001, 0x80000001, 0xFFFFFFE0, 0xFFFFFFFF);
        check("Clean equality carries EBP and CMP flags into the reached zero-count mask",
            countZero.Outcome == CapacityFinalCalleeFinalMaskLifecycleOutcome.Reached92BD
            && countZero.UpstreamFinalFrameGate.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && countZero.UpstreamFinalFrameGate.JnzTaken == false
            && countZero.UpstreamFinalFrameGate.JnzSelectedDestinationRawFileOffset
                == 0x012C9167
            && countZero.MaskTailReached
            && countZero.ExactCleanHandoffToMaskTail
            && countZero.EntryEBPCarriedToMaskTail
            && countZero.EntryEBPCarried == 0x10
            && countZero.MaskTail?.Capture.EntryEBP == 0x10
            && countZero.MaskTail?.ShiftReached == true
            && countZero.MaskTail.EffectiveShiftCount == 0
            && countZero.DerivedEntryFlagsForMaskHelper is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && countZero.UpstreamCompareFlagsDerivedForMaskHelper
            && countZero.MaskTail.EntryFlagsConsumed
            && countZero.MaskTail.ShiftFlags == countZero.DerivedEntryFlagsForMaskHelper
            && !countZero.MaskTail.UnusedEntryFlagsCaptureProvided
            && countZero.MaskTail.GlobalWriteReached
            && countZero.MaskTail.GlobalWriteAddress == 0x01EDADC8
            && countZero.MaskTail.GlobalWriteValue == 0x7FFFFFFE
            && countZero.MaskTail.FinalJumpReached
            && countZero.MaskTail.FinalJumpTaken == true
            && countZero.MaskTail.FinalJumpSelectedDestinationRawFileOffset
                == 0x012C92BD
            && countZero.MaskTail.StopAtRawFileOffsetExclusive == 0x012C92BD
            && countZero.MaskHelperExactCleanExitAdmitted
            && countZero.ExactCleanExitAdmitted
            && countZero.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeFinalMaskLifecyclePlan upstreamUnequal =
            Project(0x100, 0x20000000, 0x20000001, 0x12345678, 0x21, 0xFFFFFFFF);
        check("Contaminated upstream inequality stops at 917C and rejects later mask captures",
            upstreamUnequal.Outcome
                == CapacityFinalCalleeFinalMaskLifecycleOutcome.UpstreamUnequalTo917C
            && upstreamUnequal.UpstreamFinalFrameGate.ExactCleanExitAdmitted
            && upstreamUnequal.UpstreamFinalFrameGate.JnzTaken == true
            && upstreamUnequal.UpstreamFinalFrameGate.JnzSelectedDestinationRawFileOffset
                == 0x012C917C
            && upstreamUnequal.UpstreamFinalFrameGate.StopAtRawFileOffsetExclusive
                == 0x012C917C
            && upstreamUnequal.MaskTail is null
            && !upstreamUnequal.MaskTailReached
            && !upstreamUnequal.ExactCleanHandoffToMaskTail
            && !upstreamUnequal.EntryEBPCarriedToMaskTail
            && upstreamUnequal.UnreachedFreshFrameMinus8CaptureProvided
            && upstreamUnequal.UnreachedFreshFrameMinus4CaptureProvided
            && upstreamUnequal.UnreachedFreshGlobalCaptureProvided
            && upstreamUnequal.DerivedEntryFlagsForMaskHelper is null
            && !upstreamUnequal.UpstreamCompareFlagsDerivedForMaskHelper
            && !upstreamUnequal.MaskHelperExactCleanExitAdmitted
            && !upstreamUnequal.ExactCleanExitAdmitted
            && upstreamUnequal.StopAtRawFileOffsetExclusive == 0x012C917C);

        CapacityFinalCalleeFinalMaskLifecyclePlan cleanUpstreamUnequal =
            Project(0x100, 0x20000000, 0x20000001, null, null, null);
        check("A fully evidenced unequal route admits its clean bounded 917C exit without a child",
            cleanUpstreamUnequal.Outcome
                == CapacityFinalCalleeFinalMaskLifecycleOutcome.UpstreamUnequalTo917C
            && cleanUpstreamUnequal.UpstreamFinalFrameGate.EntryEBPCaptured
            && cleanUpstreamUnequal.UpstreamFinalFrameGate.FrameMinus14Captured
            && cleanUpstreamUnequal.UpstreamFinalFrameGate.EntryECXCaptured
            && cleanUpstreamUnequal.UpstreamFinalFrameGate.CompareReached
            && cleanUpstreamUnequal.UpstreamFinalFrameGate.CompareLeft == 0x20000000
            && cleanUpstreamUnequal.UpstreamFinalFrameGate.CompareRight == 0x20000001
            && cleanUpstreamUnequal.UpstreamFinalFrameGate.CompareFlags is { Zero: false }
            && cleanUpstreamUnequal.UpstreamFinalFrameGate.ExactCleanExitAdmitted
            && cleanUpstreamUnequal.UpstreamFinalFrameGate.JnzReached
            && cleanUpstreamUnequal.UpstreamFinalFrameGate.JnzTaken == true
            && cleanUpstreamUnequal.UpstreamFinalFrameGate
                .JnzSelectedDestinationRawFileOffset == 0x012C917C
            && cleanUpstreamUnequal.MaskTail is null
            && !cleanUpstreamUnequal.MaskTailReached
            && !cleanUpstreamUnequal.ExactCleanHandoffToMaskTail
            && !cleanUpstreamUnequal.UnreachedFreshFrameMinus8CaptureProvided
            && !cleanUpstreamUnequal.UnreachedFreshFrameMinus4CaptureProvided
            && !cleanUpstreamUnequal.UnreachedFreshGlobalCaptureProvided
            && !cleanUpstreamUnequal.MaskHelperExactCleanExitAdmitted
            && cleanUpstreamUnequal.ExactCleanExitAdmitted
            && cleanUpstreamUnequal.StopAtRawFileOffsetExclusive == 0x012C917C);

        CapacityFinalCalleeFinalMaskLifecyclePlan missingUpstreamEbp =
            Project(null, 0, 0, 0, 0, 0);
        check("Missing upstream EBP cannot be bypassed with later upstream or mask captures",
            missingUpstreamEbp.Outcome
                == CapacityFinalCalleeFinalMaskLifecycleOutcome.UpstreamMissingEntryEBP
            && !missingUpstreamEbp.UpstreamFinalFrameGate.FrameMinus14ReadReached
            && !missingUpstreamEbp.UpstreamFinalFrameGate.CompareReached
            && missingUpstreamEbp.UpstreamFinalFrameGate
                .UnreachedFreshFrameMinus14CaptureProvided
            && missingUpstreamEbp.UpstreamFinalFrameGate.UnreachedEntryECXCaptureProvided
            && missingUpstreamEbp.MaskTail is null
            && missingUpstreamEbp.UnreachedFreshFrameMinus8CaptureProvided
            && missingUpstreamEbp.UnreachedFreshFrameMinus4CaptureProvided
            && missingUpstreamEbp.UnreachedFreshGlobalCaptureProvided
            && !missingUpstreamEbp.ExactCleanExitAdmitted
            && missingUpstreamEbp.StopAtRawFileOffsetExclusive == 0x012C9160);

        CapacityFinalCalleeFinalMaskLifecyclePlan missingUpstreamFrame =
            Project(0x14, null, 0x100, 0, 0, 0);
        check("Missing upstream EBP-14 rejects late ECX and all downstream mask captures",
            missingUpstreamFrame.Outcome
                == CapacityFinalCalleeFinalMaskLifecycleOutcome
                    .UpstreamMissingFreshFrameMinus14
            && missingUpstreamFrame.UpstreamFinalFrameGate.FrameMinus14ReadReached
            && !missingUpstreamFrame.UpstreamFinalFrameGate.FrameMinus14Captured
            && missingUpstreamFrame.UpstreamFinalFrameGate
                .LateEntryECXCaptureProvided
            && missingUpstreamFrame.MaskTail is null
            && missingUpstreamFrame.UnreachedFreshFrameMinus8CaptureProvided
            && missingUpstreamFrame.UnreachedFreshFrameMinus4CaptureProvided
            && missingUpstreamFrame.UnreachedFreshGlobalCaptureProvided
            && !missingUpstreamFrame.ExactCleanExitAdmitted
            && missingUpstreamFrame.StopAtRawFileOffsetExclusive == 0x012C9160);

        CapacityFinalCalleeFinalMaskLifecyclePlan missingUpstreamEcx =
            Project(0x100, 0x20000000, null, 1, 1, 1);
        check("Missing upstream ECX blocks the CMP and cannot admit later mask operands",
            missingUpstreamEcx.Outcome
                == CapacityFinalCalleeFinalMaskLifecycleOutcome.UpstreamMissingEntryECX
            && missingUpstreamEcx.UpstreamFinalFrameGate.FrameMinus14Captured
            && !missingUpstreamEcx.UpstreamFinalFrameGate.CompareReached
            && missingUpstreamEcx.MaskTail is null
            && missingUpstreamEcx.UnreachedFreshFrameMinus8CaptureProvided
            && missingUpstreamEcx.UnreachedFreshFrameMinus4CaptureProvided
            && missingUpstreamEcx.UnreachedFreshGlobalCaptureProvided
            && !missingUpstreamEcx.ExactCleanExitAdmitted
            && missingUpstreamEcx.StopAtRawFileOffsetExclusive == 0x012C9163);

        CapacityFinalCalleeFinalMaskLifecyclePlan missingFrameMinus8 =
            Project(0x20, 7, 7, null, 0x20, 0xFFFFFFFF);
        check("Missing fresh mask EBP-8 stops before the independent EBP-4 read",
            missingFrameMinus8.Outcome
                == CapacityFinalCalleeFinalMaskLifecycleOutcome.MaskMissingFreshFrameMinus8
            && missingFrameMinus8.EntryEBPCarriedToMaskTail
            && missingFrameMinus8.MaskTail?.FrameMinus8ReadReached == true
            && !missingFrameMinus8.MaskTail.FrameMinus8Captured
            && !missingFrameMinus8.MaskTail.FrameMinus4ReadReached
            && !missingFrameMinus8.MaskTail.ShiftReached
            && missingFrameMinus8.UnreachedFreshFrameMinus4CaptureProvided
            && missingFrameMinus8.UnreachedFreshGlobalCaptureProvided
            && !missingFrameMinus8.MaskHelperExactCleanExitAdmitted
            && missingFrameMinus8.StopAtRawFileOffsetExclusive == 0x012C9167);

        CapacityFinalCalleeFinalMaskLifecyclePlan missingFrameMinus4 =
            Project(0x20, 7, 7, 0x80000001, null, 0xFFFFFFFF);
        check("Missing fresh mask EBP-4 preserves the independent EBP-8 read",
            missingFrameMinus4.Outcome
                == CapacityFinalCalleeFinalMaskLifecycleOutcome.MaskMissingFreshFrameMinus4
            && missingFrameMinus4.MaskTail?.FrameMinus8Captured == true
            && missingFrameMinus4.MaskTail.FrameMinus8 == 0x80000001
            && missingFrameMinus4.MaskTail.FrameMinus4ReadReached
            && !missingFrameMinus4.MaskTail.FrameMinus4Captured
            && !missingFrameMinus4.MaskTail.ShiftReached
            && missingFrameMinus4.UnreachedFreshGlobalCaptureProvided
            && !missingFrameMinus4.MaskHelperExactCleanExitAdmitted
            && missingFrameMinus4.StopAtRawFileOffsetExclusive == 0x012C916A);

        CapacityFinalCalleeFinalMaskLifecyclePlan missingGlobal =
            Project(0x20, 7, 7, 0x2468ACE0, 0x20, null);
        check("Missing global stops before the AND/write while retaining zero-count flags",
            missingGlobal.Outcome
                == CapacityFinalCalleeFinalMaskLifecycleOutcome.MaskMissingFreshGlobalValue
            && missingGlobal.MaskTail?.ShiftReached == true
            && missingGlobal.MaskTail.EffectiveShiftCount == 0
            && missingGlobal.MaskTail.EntryFlagsConsumed
            && missingGlobal.MaskTail.NotReached
            && missingGlobal.MaskTail.GlobalReadReached
            && !missingGlobal.MaskTail.GlobalCaptured
            && !missingGlobal.MaskTail.GlobalAndReached
            && !missingGlobal.MaskTail.GlobalWriteReached
            && !missingGlobal.MaskTail.FinalJumpReached
            && !missingGlobal.MaskHelperExactCleanExitAdmitted
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9171);

        CapacityFinalCalleeFinalMaskLifecyclePlan countOne =
            Project(0x40, 0x90000000, 0x90000000, 0x40000000, 0xA1, 0xFFFFFFFF);
        check("Count one uses shift-produced flags and does not fabricate upstream entry flags",
            countOne.MaskTail?.EffectiveShiftCount == 1
            && countOne.MaskTail.ShiftResult == 0x80000000
            && countOne.DerivedEntryFlagsForMaskHelper is null
            && !countOne.UpstreamCompareFlagsDerivedForMaskHelper
            && countOne.MaskTail.Capture.EntryFlags is null
            && !countOne.MaskTail.EntryFlagsConsumed
            && !countOne.MaskTail.UnusedEntryFlagsCaptureProvided
            && countOne.MaskTail.ShiftFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && countOne.MaskHelperExactCleanExitAdmitted
            && countOne.ExactCleanExitAdmitted);

        CapacityFinalCalleeFinalMaskLifecyclePlan countThirtyTwo =
            Project(0x40, 0x90000000, 0x90000000, 0x2468ACE0, 0x20, 0xFFFFFFFF);
        check("Count 32 aliases zero and consumes only the upstream CMP flags",
            countThirtyTwo.MaskTail?.EffectiveShiftCount == 0
            && countThirtyTwo.MaskTail.ShiftResult == 0x2468ACE0
            && countThirtyTwo.UpstreamCompareFlagsDerivedForMaskHelper
            && countThirtyTwo.MaskTail.Capture.EntryFlags
                == countThirtyTwo.DerivedEntryFlagsForMaskHelper
            && countThirtyTwo.MaskTail.ShiftFlags
                == ToMaskTailFlags(
                    countThirtyTwo.UpstreamFinalFrameGate.CompareFlags)
            && countThirtyTwo.MaskTail.EntryFlagsConsumed
            && !countThirtyTwo.MaskTail.UnusedEntryFlagsCaptureProvided
            && countThirtyTwo.MaskTail.GlobalWriteValue == 0xDB97531F
            && countThirtyTwo.MaskHelperExactCleanExitAdmitted);

        CapacityFinalCalleeFinalMaskLifecyclePlan countThirtyThree =
            Project(0x40, 0x90000000, 0x90000000, 0x80000001, 0x21, 0);
        check("Count 33 aliases one and leaves upstream CMP flags unprovided to the helper",
            countThirtyThree.MaskTail?.EffectiveShiftCount == 1
            && countThirtyThree.MaskTail.ShiftResult == 2
            && countThirtyThree.DerivedEntryFlagsForMaskHelper is null
            && !countThirtyThree.UpstreamCompareFlagsDerivedForMaskHelper
            && countThirtyThree.MaskTail.Capture.EntryFlags is null
            && !countThirtyThree.MaskTail.EntryFlagsConsumed
            && !countThirtyThree.MaskTail.UnusedEntryFlagsCaptureProvided
            && countThirtyThree.MaskTail.NotResult == 0xFFFFFFFD
            && countThirtyThree.MaskTail.GlobalWriteReached
            && countThirtyThree.MaskTail.GlobalWriteValue == 0
            && countThirtyThree.MaskTail.FinalFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && countThirtyThree.MaskTail.FinalJumpSelectedDestinationRawFileOffset
                == 0x012C92BD
            && countThirtyThree.MaskHelperExactCleanExitAdmitted);

        CapacityFinalCalleeFinalMaskLifecyclePlan independentFreshValues =
            Project(0x12345678, 0x87654321, 0x87654321,
                0x12345678, 0xFFFFFFE1, 0x55555555);
        uint expectedIndependentWrite =
            0x55555555u & ~0x2468ACF0u;
        check("The two mask frame captures and global value remain independent",
            independentFreshValues.EntryEBPCarried == 0x12345678
            && independentFreshValues.MaskTail?.FrameMinus8Address == 0x12345670
            && independentFreshValues.MaskTail.FrameMinus4Address == 0x12345674
            && independentFreshValues.MaskTail.FrameMinus8 == 0x12345678
            && independentFreshValues.MaskTail.FrameMinus4 == 0xFFFFFFE1
            && independentFreshValues.MaskTail.ShiftInput == 0x12345678
            && independentFreshValues.MaskTail.ShiftCountByte == 0xE1
            && independentFreshValues.MaskTail.EffectiveShiftCount == 1
            && independentFreshValues.MaskTail.ShiftResult == 0x2468ACF0
            && independentFreshValues.MaskTail.GlobalBefore == 0x55555555
            && independentFreshValues.MaskTail.GlobalWriteValue
                == expectedIndependentWrite);

        CapacityFinalCalleeFinalMaskLifecyclePlan zeroGlobal =
            Project(0, 0x55, 0x55, 0xFFFFFFFF, 0x21, 0);
        check("A zero fresh global still receives the exact AND write before the jump",
            zeroGlobal.MaskTail?.GlobalAddress == 0x01EDADC8
            && zeroGlobal.MaskTail.GlobalBefore == 0
            && zeroGlobal.MaskTail.GlobalAndReached
            && zeroGlobal.MaskTail.GlobalWriteReached
            && zeroGlobal.MaskTail.GlobalWriteAddress == 0x01EDADC8
            && zeroGlobal.MaskTail.GlobalWriteValue == 0
            && zeroGlobal.MaskTail.FinalJumpReached
            && zeroGlobal.MaskTail.FinalJumpTaken == true
            && zeroGlobal.MaskTail.FinalJumpSelectedDestinationRawFileOffset
                == 0x012C92BD
            && zeroGlobal.MaskTail.StopAtRawFileOffsetExclusive == 0x012C92BD
            && zeroGlobal.ExactCleanExitAdmitted);

        CapacityFinalCalleeFinalMaskLifecyclePlan wrapping =
            Project(0, 1, 1, 0x80000001, 0x21, 0xFFFFFFFF);
        check("Frame addressing and shift values preserve DWORD wrap semantics",
            wrapping.MaskTail?.FrameMinus8Address == 0xFFFFFFF8
            && wrapping.MaskTail.FrameMinus4Address == 0xFFFFFFFC
            && wrapping.MaskTail.ShiftResult == 2
            && wrapping.MaskTail.GlobalWriteValue == 0xFFFFFFFD
            && CapacityFinalCalleeAlternateMaskTailProjection.ShiftLeft(
                0x80000001, 33) == 2
            && wrapping.MaskTail.StopAtRawFileOffsetExclusive == 0x012C92BD);
    }

    private static CapacityFinalCalleeFinalMaskLifecyclePlan Project(
        uint? entryEbp,
        uint? freshFrameMinus14,
        uint? entryEcx,
        uint? freshFrameMinus8,
        uint? freshFrameMinus4,
        uint? freshGlobalValue) =>
        CapacityFinalCalleeFinalMaskLifecycleProjection.Project(new(
            new(entryEbp, freshFrameMinus14, entryEcx),
            freshFrameMinus8,
            freshFrameMinus4,
            freshGlobalValue));

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static CapacityFinalCalleeAlternateMaskTailFlags ToMaskTailFlags(
        CapacityFinalCalleeAlternateFinalFrameGateFlags? flags) =>
        flags is CapacityFinalCalleeAlternateFinalFrameGateFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign,
                value.Overflow)
            : new(null, null, null, null, null, null);
}
