using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacitySeedLoopChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The seed join feeds the loop entry, which projects no more than one iteration",
            FrameHandoffCapacitySeedProjection.JoinRawFileOffset
                == FrameHandoffCapacityLoopProjection.EntryRawFileOffset
            && FrameHandoffCapacitySeedProjection.EndRawFileOffsetExclusive
                == FrameHandoffCapacitySeedProjection.JoinRawFileOffset
            && FrameHandoffCapacityLoopProjection.LoopRawFileOffset
                == FrameHandoffCapacityLoopProjection.EntryRawFileOffset);

        FrameHandoffCapacitySeedLoopPlan zeroSeed =
            FrameHandoffCapacitySeedLoopProjection.Project(
                new(new(0xA000, 0x8000, 0)));
        check("A zero receiver read seeds ESI with one and derives loop EDX as zero before one doubling",
            zeroSeed.Outcome
                == FrameHandoffCapacitySeedLoopOutcome.OneLoopIterationProjected
            && zeroSeed.SeedPlan.Outcome
                == FrameHandoffCapacitySeedOutcome.ZeroReadKeepsInitialSeed
            && zeroSeed.LoopIdentityCapturesReached
            && zeroSeed.ActualLoopEsi == 1
            && zeroSeed.ActualLoopEdx == 0
            && zeroSeed.ExpectedLoopEsiMatched is null
            && zeroSeed.ExpectedLoopEdxMatched is null
            && zeroSeed.LoopPlan is FrameHandoffCapacityLoopPlan zeroLoop
            && zeroLoop.Capture.EntryEsi == 1
            && zeroLoop.Capture.EntryEdx == 0
            && zeroLoop.EaxAfterEntrySubtract == 1
            && zeroLoop.CompareEsiWithEightReached
            && zeroLoop.EsiAtLeastEight == false
            && zeroLoop.EsiAfterDouble == 2
            && zeroLoop.LoopBackReached
            && zeroSeed.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityLoopProjection.EndRawFileOffsetExclusive);

        FrameHandoffCapacitySeedLoopPlan eightSeed =
            FrameHandoffCapacitySeedLoopProjection.Project(
                new(new(0xB000, 0x9000, 8)));
        check("An eight-valued seed produces zero difference, bypasses the threshold gate, and doubles once",
            eightSeed.Outcome
                == FrameHandoffCapacitySeedLoopOutcome.OneLoopIterationProjected
            && eightSeed.ActualLoopEsi == 8
            && eightSeed.ActualLoopEdx == 8
            && eightSeed.LoopPlan is FrameHandoffCapacityLoopPlan eightLoop
            && eightLoop.EaxAfterEntrySubtract == 0
            && eightLoop.EntrySubtractResultIsZero == true
            && eightLoop.ZeroDifferenceBranchTaken == true
            && !eightLoop.CompareEsiWithEightReached
            && eightLoop.EsiAtLeastEight is null
            && eightLoop.EsiAfterDouble == 16
            && eightLoop.LoopBackReached);

        FrameHandoffCapacitySeedLoopPlan hugeSeed =
            FrameHandoffCapacitySeedLoopProjection.Project(
                new(new(0xC000, 0xA000, 0x08000000)));
        check("A huge nonzero seed takes the loop failure branch before doubling",
            hugeSeed.Outcome
                == FrameHandoffCapacitySeedLoopOutcome.OneLoopIterationProjected
            && hugeSeed.ActualLoopEsi == 0x08000000
            && hugeSeed.ActualLoopEdx == 0x08000000
            && hugeSeed.LoopPlan is FrameHandoffCapacityLoopPlan hugeLoop
            && hugeLoop.EaxAfterEntrySubtract == 0
            && hugeLoop.Outcome
                == FrameHandoffCapacityLoopOutcome.FailureAtRawFileOffset6CC0
            && hugeLoop.EaxAfterUpperSubtract == 0x07FFFFFF
            && hugeLoop.FailureBranchTaken == true
            && !hugeLoop.DoubleEsiReached
            && !hugeLoop.LoopBackReached
            && hugeSeed.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityLoopProjection.FailureRawFileOffset);

        FrameHandoffCapacitySeedLoopPlan missingRead =
            FrameHandoffCapacitySeedLoopProjection.Project(
                new(new(0xE000, 0x1234, null), 1, 0));
        check("A missing seed read preserves its frame store and ESI seed and leaves loop identities unreached",
            missingRead.Outcome
                == FrameHandoffCapacitySeedLoopOutcome.MissingReceiverPlus08Capture
            && missingRead.SeedPlan.ReceiverFrameStoreProjected
            && missingRead.SeedPlan.ReceiverFrameStoreAddress == 0xDFEC
            && missingRead.SeedPlan.ReceiverFrameStoreValue == 0x1234
            && missingRead.SeedPlan.EsiSeedEstablished
            && missingRead.SeedPlan.EsiSeedBeforeReceiverRead == 1
            && missingRead.LoopPlan is null
            && !missingRead.LoopIdentityCapturesReached
            && missingRead.UnreachedExpectedLoopEsiCaptureProvided
            && missingRead.UnreachedExpectedLoopEdxCaptureProvided
            && missingRead.ActualLoopEsi is null
            && missingRead.ActualLoopEdx is null
            && missingRead.ExpectedLoopEsiMatched is null
            && missingRead.ExpectedLoopEdxMatched is null
            && missingRead.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacitySeedProjection.ReceiverPlus08ReadRawFileOffset);

        FrameHandoffCapacitySeedLoopPlan identityMismatch =
            FrameHandoffCapacitySeedLoopProjection.Project(
                new(new(0xF000, 0x2000, 7), 8, 7));
        check("A supplied loop identity mismatch is reported before any loop branch or doubling",
            identityMismatch.Outcome
                == FrameHandoffCapacitySeedLoopOutcome.LoopIdentityCaptureMismatch
            && identityMismatch.LoopIdentityCapturesReached
            && identityMismatch.ActualLoopEsi == 7
            && identityMismatch.ActualLoopEdx == 7
            && identityMismatch.ExpectedLoopEsiMatched == false
            && identityMismatch.ExpectedLoopEdxMatched == true
            && identityMismatch.LoopPlan is null
            && identityMismatch.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityLoopProjection.EntryRawFileOffset);

        FrameHandoffCapacitySeedLoopPlan wrappedSeed =
            FrameHandoffCapacitySeedLoopProjection.Project(
                new(new(0x0C, uint.MaxValue - 7, 7), 7, 7));
        check("The composed seed preserves wrapped frame and receiver addresses while forwarding derived identities",
            wrappedSeed.Outcome
                == FrameHandoffCapacitySeedLoopOutcome.OneLoopIterationProjected
            && wrappedSeed.SeedPlan.ReceiverFrameStoreAddress == 0xFFFFFFF8
            && wrappedSeed.SeedPlan.ReceiverPlus08ReadAddress == 0
            && wrappedSeed.SeedPlan.EsiAtJoin == 7
            && wrappedSeed.ActualLoopEsi == 7
            && wrappedSeed.ActualLoopEdx == 7
            && wrappedSeed.ExpectedLoopEsiMatched == true
            && wrappedSeed.ExpectedLoopEdxMatched == true
            && wrappedSeed.LoopPlan is FrameHandoffCapacityLoopPlan wrappedLoop
            && wrappedLoop.Capture.EntryEsi == 7
            && wrappedLoop.Capture.EntryEdx == 7
            && wrappedLoop.EsiAfterDouble == 14
            && wrappedLoop.LoopBackReached);
    }
}
