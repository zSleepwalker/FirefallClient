using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeDescentClearLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Only a clean FDD terminal joins the exact clear-tail endpoints",
            CapacityFinalCalleeDescentClearLifecycleProjection.EntryRawFileOffset
                == 0x012C8FC2
            && CapacityFinalCalleeDescentClearLifecycleProjection.TerminalRawFileOffset
                == 0x012C8FDD
            && CapacityFinalCalleeDescentClearLifecycleProjection.LoopbackRawFileOffset
                == 0x012C8FC5
            && CapacityFinalCalleeDescentClearLifecycleProjection.BelowDestinationRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeDescentClearLifecycleProjection
                .ClearTailCompletionRawFileOffsetExclusive == 0x012C8FFE);

        CapacityFinalCalleeDescentClearLifecyclePlan cleanBelow =
            CapacityFinalCalleeDescentClearLifecycleProjection.Project(Capture(
                descent: Descent(
                    freshFrameMinus4: 0x7FFFFFFF,
                    freshEaxPlus14: 0,
                    freshEaxPlus10: 0),
                freshGlobalForEntryCompare: 0x80000000,
                freshFrameMinus12: 0xFFFFFFFF,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null));
        check("A clean terminal derives original EDX and EBP and admits the exact E76 clear branch",
            cleanBelow.Outcome
                == CapacityFinalCalleeDescentClearLifecycleOutcome.ClearBelowDestination
            && cleanBelow.DescentPlan.Outcome
                == CapacityFinalCalleeFieldDescentStepOutcome.ReachedTerminal
            && cleanBelow.DescentPlan.EDXLoadedFromFrameMinus4 == 0x7FFFFFFF
            && cleanBelow.DescentPlan.EDXLoadedFromEcx is null
            && !cleanBelow.DescentPlan.FreshEcxReadReached
            && cleanBelow.ClearTailPlan is
                CapacityFinalCalleeFieldClearTailPlan below
            && below.Capture.ValidatedEntryEDX == 0x7FFFFFFF
            && below.Capture.ValidatedEntryEBP == 0x2000
            && below.CompareResult == 0xFFFFFFFF
            && below.CompareFlags is
            {
                Carry: true,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && below.FrameMinus12 == 0xFFFFFFFF
            && below.FrameMinus4ForZeroStoreAddress is null
            && below.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x1FFC, 0x7FFFFFFF)
            })
            && below.SelectedBranchDestinationRawFileOffset == 0x012C8E76
            && cleanBelow.ClearTailReached
            && cleanBelow.Admitted
            && !cleanBelow.UnreachedClearTailCaptureProvided
            && cleanBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeDescentClearLifecyclePlan contaminatedBelow =
            CapacityFinalCalleeDescentClearLifecycleProjection.Project(Capture(
                descent: Descent(
                    freshFrameMinus4: 0x7FFFFFFF,
                    freshEaxPlus14: 0,
                    freshEaxPlus10: 0),
                freshGlobalForEntryCompare: 0x80000000,
                freshFrameMinus12: 0xFFFFFFFF,
                freshFrameMinus4ForZeroStoreAddress: 0,
                freshGlobalForTailLoad: 0));
        check("Captures beyond the taken E76 branch preserve its effects but prevent admission",
            contaminatedBelow.Outcome
                == CapacityFinalCalleeDescentClearLifecycleOutcome.ClearTailContaminated
            && contaminatedBelow.ClearTailPlan is
                CapacityFinalCalleeFieldClearTailPlan contaminated
            && contaminated.SelectedBranchDestinationRawFileOffset == 0x012C8E76
            && contaminated.UnreachedFreshFrameMinus4CaptureProvided
            && contaminated.UnreachedFreshGlobalForTailLoadCaptureProvided
            && contaminated.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x1FFC, 0x7FFFFFFF)
            })
            && contaminatedBelow.ClearTailReached
            && contaminatedBelow.UnreachedClearTailCaptureProvided
            && !contaminatedBelow.Admitted
            && contaminatedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeDescentClearLifecyclePlan cleanTail =
            CapacityFinalCalleeDescentClearLifecycleProjection.Project(Capture(
                descent: Descent(
                    freshFrameMinus4: 0x30,
                    freshEaxPlus14: 0,
                    freshEaxPlus10: 0),
                freshGlobalForEntryCompare: 0,
                freshFrameMinus12: 0x11111111,
                freshFrameMinus4ForZeroStoreAddress: 0,
                freshGlobalForTailLoad: 0));
        check("Equal zero reads stay captured and the admitted normal tail ends exactly at FFE",
            cleanTail.Outcome
                == CapacityFinalCalleeDescentClearLifecycleOutcome.ClearTailCompleted
            && cleanTail.ClearTailPlan is
                CapacityFinalCalleeFieldClearTailPlan tail
            && tail.Capture.ValidatedEntryEDX == 0x30
            && tail.CompareResult == 0x30
            && tail.BranchTaken == false
            && tail.CompareGlobalValue == 0
            && tail.TailGlobalCaptured
            && tail.TailGlobalValue == 0
            && tail.Reads.Select(read => read.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C8FDD, 0x012C8FE6, 0x012C8FEF, 0x012C8FF8 })
            && tail.Reads[0].Address == tail.Reads[^1].Address
            && tail.Reads[0].Value == 0
            && tail.Reads[^1].Value == 0
            && tail.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x1FFC, 0x30),
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FF2, 0, 0)
            })
            && cleanTail.Admitted
            && cleanTail.StopAtRawFileOffsetExclusive == 0x012C8FFE);

        CapacityFinalCalleeDescentClearLifecyclePlan cleanLoopback =
            CapacityFinalCalleeDescentClearLifecycleProjection.Project(Capture(
                descent: Descent(
                    freshFrameMinus4: 0x3000,
                    freshEaxPlus14: 1,
                    freshEaxPlus10: null,
                    freshEcxValue: 0),
                freshGlobalForEntryCompare: null,
                freshFrameMinus12: null,
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: null));
        CapacityFinalCalleeDescentClearLifecyclePlan contaminatedLoopback =
            CapacityFinalCalleeDescentClearLifecycleProjection.Project(Capture(
                descent: Descent(
                    freshFrameMinus4: 0x3000,
                    freshEaxPlus14: 1,
                    freshEaxPlus10: null,
                    freshEcxValue: 0),
                freshGlobalForEntryCompare: 0));
        check("FC5 is a single bounded clean loopback and clear captures contaminate its route",
            cleanLoopback.Outcome
                == CapacityFinalCalleeDescentClearLifecycleOutcome.LoopbackBounded
            && cleanLoopback.DescentPlan.LoopbackReached
            && cleanLoopback.DescentPlan.EAXLoadedFromEcx == 0
            && cleanLoopback.ClearTailPlan is null
            && cleanLoopback.Admitted
            && !cleanLoopback.ClearTailCaptureSupplied
            && cleanLoopback.StopAtRawFileOffsetExclusive == 0x012C8FC5
            && contaminatedLoopback.Outcome
                == CapacityFinalCalleeDescentClearLifecycleOutcome.LoopbackContaminated
            && contaminatedLoopback.ClearTailPlan is null
            && contaminatedLoopback.UnreachedClearTailCaptureProvided
            && !contaminatedLoopback.Admitted
            && contaminatedLoopback.StopAtRawFileOffsetExclusive == 0x012C8FC5);

        CapacityFinalCalleeDescentClearLifecyclePlan terminalWithUnreachedEcx =
            CapacityFinalCalleeDescentClearLifecycleProjection.Project(Capture(
                descent: Descent(
                    freshFrameMinus4: 0x1234,
                    freshEaxPlus14: 0,
                    freshEaxPlus10: 0,
                    freshEcxValue: 0),
                freshGlobalForEntryCompare: 0));
        CapacityFinalCalleeDescentClearLifecyclePlan descentMissingRead =
            CapacityFinalCalleeDescentClearLifecycleProjection.Project(Capture(
                descent: Descent(
                    freshFrameMinus4: null,
                    freshEaxPlus14: 0,
                    freshEaxPlus10: 0),
                freshGlobalForEntryCompare: 0));
        check("A contaminated terminal or missing descent read never starts clear-tail projection",
            terminalWithUnreachedEcx.Outcome
                == CapacityFinalCalleeDescentClearLifecycleOutcome
                    .DescentBoundaryContaminated
            && terminalWithUnreachedEcx.DescentPlan.UnreachedFreshEcxCaptureProvided
            && terminalWithUnreachedEcx.ClearTailPlan is null
            && terminalWithUnreachedEcx.UnreachedClearTailCaptureProvided
            && !terminalWithUnreachedEcx.Admitted
            && terminalWithUnreachedEcx.StopAtRawFileOffsetExclusive == 0x012C8FDD
            && descentMissingRead.Outcome
                == CapacityFinalCalleeDescentClearLifecycleOutcome.DescentBlocked
            && descentMissingRead.DescentPlan
                .Outcome == CapacityFinalCalleeFieldDescentStepOutcome.MissingFreshFrameMinus4
            && descentMissingRead.ClearTailPlan is null
            && descentMissingRead.UnreachedClearTailCaptureProvided
            && !descentMissingRead.Admitted);

        CapacityFinalCalleeDescentClearLifecyclePlan missingFrameC =
            CapacityFinalCalleeDescentClearLifecycleProjection.Project(Capture(
                freshFrameMinus12: null,
                freshFrameMinus4ForZeroStoreAddress: 0xABC,
                freshGlobalForTailLoad: 0));
        CapacityFinalCalleeDescentClearLifecyclePlan missingFrame4 =
            CapacityFinalCalleeDescentClearLifecycleProjection.Project(Capture(
                freshFrameMinus4ForZeroStoreAddress: null,
                freshGlobalForTailLoad: 0));
        check("Missing clear reads preserve prior writes and report later supplied values as unreached",
            missingFrameC.Outcome
                == CapacityFinalCalleeDescentClearLifecycleOutcome.ClearTailContaminated
            && !missingFrameC.Admitted
            && missingFrameC.ClearTailPlan is
                CapacityFinalCalleeFieldClearTailPlan missingC
            && missingC.CompareReached
            && missingC.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x1FFC, 0x30)
            })
            && !missingC.BranchReached
            && missingC.UnreachedFreshFrameMinus4CaptureProvided
            && missingC.UnreachedFreshGlobalForTailLoadCaptureProvided
            && missingFrameC.StopAtRawFileOffsetExclusive == 0x012C8FE6
            && missingFrame4.Outcome
                == CapacityFinalCalleeDescentClearLifecycleOutcome.ClearTailContaminated
            && !missingFrame4.Admitted
            && missingFrame4.ClearTailPlan is
                CapacityFinalCalleeFieldClearTailPlan missing4
            && missing4.BranchTaken == false
            && missing4.FrameMinus4ReadReached
            && !missing4.FrameMinus4Captured
            && missing4.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeFieldClearTailWrite(
                    0x012C8FE3, 0x1FFC, 0x30)
            })
            && missing4.UnreachedFreshGlobalForTailLoadCaptureProvided
            && missingFrame4.StopAtRawFileOffsetExclusive == 0x012C8FEF);

        CapacityFinalCalleeDescentClearLifecyclePlan independentFrameReads =
            CapacityFinalCalleeDescentClearLifecycleProjection.Project(Capture(
                descent: Descent(
                    validatedEntryEBP: 2,
                    validatedEntryEAX: 0xFFFFFFF8,
                    freshFrameMinus4: 0xAAAAAAAA,
                    freshEaxPlus14: 0,
                    freshEaxPlus10: 0),
                freshGlobalForEntryCompare: 0xAAAAAAAA,
                freshFrameMinus12: 0xBBBBBBBB,
                freshFrameMinus4ForZeroStoreAddress: 0xCCCCCCCC,
                freshGlobalForTailLoad: 0xDDDDDDDD));
        check("Independent frame reads and wrapped addresses do not substitute the descent EDX value",
            independentFrameReads.DescentPlan.FreshFrameMinus4Address == 0xFFFFFFFE
            && independentFrameReads.DescentPlan.EDXLoadedFromFrameMinus4 == 0xAAAAAAAA
            && independentFrameReads.DescentPlan.FreshEaxPlus14Address == 0xC
            && independentFrameReads.DescentPlan.FreshEaxPlus10Address == 8
            && independentFrameReads.ClearTailPlan is
                CapacityFinalCalleeFieldClearTailPlan independent
            && independent.Capture.ValidatedEntryEDX == 0xAAAAAAAA
            && independent.Capture.ValidatedEntryEBP == 2
            && independent.FrameMinus12Address == 0xFFFFFFF6
            && independent.FrameMinus4Address == 0xFFFFFFFE
            && independent.FrameMinus12 == 0xBBBBBBBB
            && independent.FrameMinus4ForZeroStoreAddress == 0xCCCCCCCC
            && independent.CompareGlobalValue == 0xAAAAAAAA
            && independent.TailGlobalValue == 0xDDDDDDDD
            && independent.Writes[0].Address == 0xFFFFFFFE
            && independent.Writes[0].Value == 0xAAAAAAAA
            && independent.Writes[1].Address == 0xCCCCCCCC
            && independent.Writes[1].Value == 0
            && independentFrameReads.Admitted
            && independentFrameReads.StopAtRawFileOffsetExclusive == 0x012C8FFE);
    }

    private static CapacityFinalCalleeDescentClearLifecycleCapture Capture(
        CapacityFinalCalleeFieldDescentStepCapture? descent = null,
        uint? freshGlobalForEntryCompare = 0,
        uint? freshFrameMinus12 = 0x1234,
        uint? freshFrameMinus4ForZeroStoreAddress = 0,
        uint? freshGlobalForTailLoad = 0xFFFFFFFF) =>
        new(
            descent ?? Descent(),
            freshGlobalForEntryCompare,
            freshFrameMinus12,
            freshFrameMinus4ForZeroStoreAddress,
            freshGlobalForTailLoad);

    private static CapacityFinalCalleeFieldDescentStepCapture Descent(
        uint? validatedEntryEBP = 0x2000,
        uint? validatedEntryEAX = 0x1000,
        uint? freshFrameMinus4 = 0x30,
        uint? freshEaxPlus14 = 0,
        uint? freshEaxPlus10 = 0,
        uint? freshEcxValue = null) =>
        new(
            validatedEntryEBP,
            validatedEntryEAX,
            freshFrameMinus4,
            freshEaxPlus14,
            freshEaxPlus10,
            freshEcxValue);
}
