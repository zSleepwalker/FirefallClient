using System;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleProjection;

internal static class CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The small-bin epilogue joins the internal 9441 entry and stops after RET at 945C",
            Projection.EntryRawFileOffset
                == CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleProjection.EntryRawFileOffset
            && Projection.ChildEntryRawFileOffset == 0x012C9441
            && Projection.RingCompletionRawFileOffset == 0x012C9441
            && Projection.ReturnBoundaryRawFileOffsetExclusive == 0x012C945C
            && new uint[]
                {
                    Projection.ZeroRingFirstWriteRawFileOffset,
                    Projection.ZeroRingSecondWriteRawFileOffset,
                    Projection.ZeroRingThirdWriteRawFileOffset,
                    Projection.ZeroRingFourthWriteRawFileOffset,
                    Projection.ZeroRingFifthWriteRawFileOffset
                }.SequenceEqual(new uint[]
                    {
                        0x012C931D, 0x012C9323, 0x012C9326, 0x012C9329, 0x012C932C
                    })
            && new uint[]
                {
                    Projection.NonzeroRingFirstWriteRawFileOffset,
                    Projection.NonzeroRingSecondWriteRawFileOffset,
                    Projection.NonzeroRingThirdWriteRawFileOffset,
                    Projection.NonzeroRingFourthWriteRawFileOffset
                }.SequenceEqual(new uint[]
                    {
                        0x012C9343, 0x012C9346, 0x012C9349, 0x012C934C
                    }));

        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan[] completedRoutes =
            new[] { 5u, 31u }
                .SelectMany(index => new[]
                {
                    CompleteZeroRoute(index),
                    CompleteNonzeroRoute(index)
                })
                .ToArray();
        check("Both zero OR plus five writes and nonzero CMP plus four writes reach the epilogue for indices 5 and 31",
            completedRoutes.Length == 4
            && completedRoutes.Where(plan => plan.ParentExactZeroRingRoute).All(plan =>
                plan.ParentPlan.ZeroRoute?.Writes.Length == 5
                && plan.ParentPlan.ZeroRoute.Writes.All(write =>
                    write.AddressCaptured && write.ValueCaptured && write.Committed)
                && plan.ParentPlan.ZeroRoute.Writes[0].IsParentGlobalWrite
                && plan.ParentPlan.ZeroRoute.Writes.Skip(1).All(write =>
                    !write.IsParentGlobalWrite)
                && plan.OrderedEffects.Any(effect =>
                    effect.Kind
                        == CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                            .ZeroRouteGlobalWrite
                    && effect.RawFileOffset == 0x012C931D
                    && effect.Address == 0x01EDADC8
                    && effect.ValueCaptured
                    && effect.Committed)
                && plan.ParentPlan.ZeroRoute.BranchTargetRawFileOffset == 0x012C9441
                && plan.ParentPlan.ZeroRoute.StopAtRawFileOffsetExclusive == 0x012C9441
                && plan.InheritedESIDerivedFromParent
                && plan.InheritedESIConsumedByParentRing
                && plan.ParentRingEpilogueAdmission
                && plan.EpiloguePlan is
                {
                    Outcome: CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped,
                    ReturnReached: true,
                    StopAtRawFileOffsetExclusive: 0x012C945C
                }
                && plan.ParentMemoryEffectsCompleteBeforeChildTest
                && plan.ExactBoundedCompletion
                && plan.ExactCleanCompletion
                && plan.StopAtRawFileOffsetExclusive == 0x012C945C)
            && completedRoutes.Where(plan => plan.ParentExactNonzeroRingRoute).All(plan =>
                plan.ParentPlan.NonzeroRoute?.Parent.CompareReached == true
                && plan.ParentPlan.NonzeroRoute.Parent.JBReached
                && plan.ParentPlan.NonzeroRoute.Parent.JBTaken == false
                && plan.ParentPlan.NonzeroRoute.Writes.Length == 4
                && plan.ParentPlan.NonzeroRoute.Writes.All(write =>
                    write.AddressCaptured && write.ValueCaptured && write.Committed)
                && plan.ParentPlan.CombinedOrderedMemoryEffects.TakeLast(6)
                    .Select(effect => effect.RawFileOffset)
                    .SequenceEqual(new uint[]
                    {
                        0x012C9334, 0x012C9337,
                        0x012C9343, 0x012C9346, 0x012C9349, 0x012C934C
                    })
                && plan.ParentPlan.NonzeroRoute.BranchTargetRawFileOffset == 0x012C9441
                && plan.ParentPlan.NonzeroRoute.StopAtRawFileOffsetExclusive == 0x012C9441
                && plan.InheritedESIDerivedFromParent
                && plan.InheritedESIConsumedByParentRing
                && plan.ParentRingEpilogueAdmission
                && plan.EpiloguePlan is
                {
                    Outcome: CapacityFinalCalleeGatedEpilogueOutcome.ReturnedNormally,
                    ReturnReached: true,
                    StopAtRawFileOffsetExclusive: 0x012C945C
                }
                && plan.ParentMemoryEffectsCompleteBeforeChildTest
                && plan.ExactBoundedCompletion
                && plan.ExactCleanCompletion
                && plan.StopAtRawFileOffsetExclusive == 0x012C945C)
            && completedRoutes.All(plan =>
                !plan.CounterDecrementReached
                && !plan.DestinationEntered
                && (plan.EpiloguePlan is { CallReached: true })
                    != plan.ParentExactZeroRingRoute));

        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan cleanZero =
            CompleteZeroRoute(5);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan equalThreshold =
            CompleteNonzeroRoute(5, successorEDXPlus8: 0x80000000,
                successorGlobal: 0x80000000);
        check("OR flags and all six CMP flags enter the child; only TEST changes them and normal call return makes all six unknown",
            cleanZero.ParentFlagsAtChildEntry is
            {
                Carry: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && cleanZero.ParentFlagsAtChildEntry?.Auxiliary is null
            && cleanZero.ParentFlagsAtChildEntry?.Parity is bool
            && cleanZero.ParentFlagsAtChildEntry
                == ConvertOrFlags(cleanZero.ParentPlan.ZeroRoute?.FlagsAtStop)
            && equalThreshold.ParentFlagsAtChildEntry is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalThreshold.EpiloguePlan?.TestFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && equalThreshold.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && equalThreshold.EpiloguePlan?.CallReached == true
            && equalThreshold.OpaqueCallExecuted == false
            && equalThreshold.ParentFlagsPreservedUntilChildTest == false);

        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingChildEsp =
            Project(5, zeroRoute: false, lateChildEsp: null, lateChildEbp: 0x2000);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingChildEbp =
            Project(5, zeroRoute: false, lateChildEsp: 0x1000, lateChildEbp: null);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingBothChildRegisters =
            Project(5, zeroRoute: false, lateChildEsp: null, lateChildEbp: null);
        check("Late ESP and EBP are required independently; no missing register defaults to zero or enters TEST",
            new[]
            {
                missingChildEsp,
                missingChildEbp,
                missingBothChildRegisters
            }.All(plan =>
                plan.ParentRingCompletionTo9441
                && plan.ParentRingEpilogueAdmission
                && plan.ChildEntryPending
                && plan.ParentActualExactBoundedCompletion
                && !plan.ActualExactBoundedCompletion
                && plan.EpiloguePlan is null
                && !plan.Internal9441Entered
                && !plan.ExactCleanCompletion
                && plan.StopAtRawFileOffsetExclusive == 0x012C9441)
            && missingChildEsp.Outcome
                == CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome
                    .MissingLateChildEsp
            && missingChildEbp.Outcome
                == CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome
                    .MissingLateChildEbp
            && missingBothChildRegisters.Outcome
                == CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleOutcome
                    .MissingLateChildEspAndEbp);

        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingTest =
            Project(5, zeroRoute: true, testedByte: null,
                stackPopEdi: 1, stackPopEsi: 2, stackPopEbx: 3);
        check("A missing TEST byte attempts 9441, preserves the route's six entry flags, and is not complete",
            missingTest.ChildEntryTargetEntered
            && missingTest.EpiloguePlan is
            {
                Outcome: CapacityFinalCalleeGatedEpilogueOutcome.MissingTestedByte,
                TestReadReached: true,
                TestedByteCaptured: false,
                StopAtRawFileOffsetExclusive: 0x012C9441
            }
            && missingTest.ParentFlagsPreservedUntilChildTest
            && missingTest.FlagsAtStop == missingTest.ParentFlagsAtChildEntry
            && !missingTest.ActualExactBoundedCompletion
            && !missingTest.ExactCleanCompletion
            && missingTest.OrderedEffects.Last() is
            {
                Kind: CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                    .TestByteRead,
                Attempted: true,
                ReadAttempted: true,
                WriteAttempted: false,
                ValueCaptured: false,
                Committed: false
            });

        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingEdi =
            Project(5, zeroRoute: true, testedByte: 0, stackPopEdi: null);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingEsi =
            Project(5, zeroRoute: true, testedByte: 0, stackPopEdi: 1,
                stackPopEsi: null);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingEbx =
            Project(5, zeroRoute: true, testedByte: 0, stackPopEdi: 1,
                stackPopEsi: 2, stackPopEbx: null);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingEbpPop =
            Project(5, zeroRoute: true, testedByte: 0,
                stackPopEdi: 1, stackPopEsi: 2, stackPopEbx: 3,
                stackPopEbp: null, returnTarget: 5);
        check("Each missing pop stops at its own instruction without inventing stack values",
            missingEdi.EpiloguePlan is
            {
                Outcome: CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEdiValue,
                StackPopEdiReached: true,
                StackPopEsiReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9448
            }
            && missingEdi.OrderedEffects.Last() is
            {
                Kind: CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                    .StackPopEdi,
                ReadAttempted: true,
                WriteAttempted: false,
                ValueCaptured: false,
                Committed: false
            }
            && missingEsi.EpiloguePlan is
            {
                Outcome: CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEsiValue,
                StackPopEdiReached: true,
                StackPopEsiReached: true,
                StackPopEbxReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9449
            }
            && missingEbx.EpiloguePlan is
            {
                Outcome: CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbxValue,
                StackPopEbxReached: true,
                BranchReached: false,
                StopAtRawFileOffsetExclusive: 0x012C944A
            }
            && missingEbpPop.EpiloguePlan is
            {
                Outcome: CapacityFinalCalleeGatedEpilogueOutcome.MissingStackPopEbpValue,
                StackPopEbpReached: true,
                ReturnTargetReadReached: false,
                StopAtRawFileOffsetExclusive: 0x012C945A
            }
            && missingEbpPop.ParentActualExactBoundedCompletion
            && !missingEbpPop.ActualExactBoundedCompletion
            && !missingEbpPop.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingCallCell =
            Project(5, zeroRoute: false, testedByte: 2,
                stackPopEdi: 1, stackPopEsi: 2, stackPopEbx: 3);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingCallStatus =
            Project(5, zeroRoute: false, testedByte: 2,
                stackPopEdi: 1, stackPopEsi: 2, stackPopEbx: 3,
                callCellTarget: 0x1234, callReturnedNormally: null);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan nonreturningCall =
            Project(5, zeroRoute: false, testedByte: 2,
                stackPopEdi: 1, stackPopEsi: 2, stackPopEbx: 3,
                callCellTarget: 0x1234, callReturnedNormally: false);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingPostCallEbp =
            Project(5, zeroRoute: false, testedByte: 2,
                stackPopEdi: 1, stackPopEsi: 2, stackPopEbx: 3,
                callCellTarget: 0x1234, callReturnedNormally: true,
                postCallEbp: null, stackPopEbp: 4, returnTarget: 5);
        check("Call-cell, return-status, nonreturn, and late post-call EBP evidence remain bounded without simulating the call",
            missingCallCell.EpiloguePlan is
            {
                Outcome: CapacityFinalCalleeGatedEpilogueOutcome.MissingCallCellTarget,
                CallCellReadReached: true,
                CallReached: false,
                StopAtRawFileOffsetExclusive: 0x012C9452
            }
            && missingCallCell.OrderedEffects.Last() is
            {
                Kind: CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                    .CallCellRead,
                ReadAttempted: true,
                WriteAttempted: false,
                ValueCaptured: false,
                Committed: false
            }
            && missingCallStatus.EpiloguePlan is
            {
                Outcome: CapacityFinalCalleeGatedEpilogueOutcome.MissingCallReturnStatus,
                CallCellTargetCaptured: true,
                CallReached: true,
                CallReturnedNormally: null,
                StopAtRawFileOffsetExclusive: 0x012C9452
            }
            && nonreturningCall.EpiloguePlan is
            {
                Outcome: CapacityFinalCalleeGatedEpilogueOutcome.CallDidNotReturnNormally,
                CallReached: true,
                CallReturnedNormally: false,
                StopAtRawFileOffsetExclusive: 0x012C9452
            }
            && missingPostCallEbp.EpiloguePlan is
            {
                Outcome: CapacityFinalCalleeGatedEpilogueOutcome.MissingPostCallEbp,
                CallReached: true,
                CallReturnedNormally: true,
                PostCallEbpCaptureReached: true,
                PostCallEbpCaptured: false,
                StopAtRawFileOffsetExclusive: 0x012C9458
            }
            && missingPostCallEbp.FlagsAtStop is
            {
                Carry: null,
                Parity: null,
                Auxiliary: null,
                Zero: null,
                Sign: null,
                Overflow: null
            }
            && !missingPostCallEbp.ActualExactBoundedCompletion
            && !missingCallCell.OpaqueCallExecuted
            && !missingCallStatus.OpaqueCallExecuted
            && !nonreturningCall.OpaqueCallExecuted
            && missingCallStatus.OrderedEffects.Last() is
            {
                Kind: CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                    .OpaqueCallReturnStatus,
                Attempted: true,
                ValueCaptured: false,
                Committed: false
            }
            && nonreturningCall.OrderedEffects.Any(effect =>
                effect.Kind
                    == CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                        .PushImmediate
                && effect.WriteAttempted
                && effect.Address.HasValue
                && effect.ValueCaptured
                && effect.Committed));

        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan noReturnTarget =
            Project(5, zeroRoute: true, testedByte: 0,
                stackPopEdi: 1, stackPopEsi: 2, stackPopEbx: 3,
                stackPopEbp: 4, returnTarget: null);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan normalReturn =
            CompleteNonzeroRoute(5);
        check("RET requires its captured target and only the bounded RET at 945B completes",
            noReturnTarget.EpiloguePlan is
            {
                Outcome: CapacityFinalCalleeGatedEpilogueOutcome.MissingReturnTarget,
                ReturnTargetReadReached: true,
                ReturnReached: false,
                StopAtRawFileOffsetExclusive: 0x012C945B
            }
            && noReturnTarget.OrderedEffects.Last() is
            {
                Kind: CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleEffectKind
                    .ReturnTargetRead,
                ReadAttempted: true,
                WriteAttempted: false,
                ValueCaptured: false,
                Committed: false
            }
            && !noReturnTarget.ActualExactBoundedCompletion
            && normalReturn.EpiloguePlan is
            {
                ReturnReached: true,
                ReturnTargetCaptured: true,
                StopAtRawFileOffsetExclusive: 0x012C945C
            }
            && normalReturn.ActualExactBoundedCompletion
            && normalReturn.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan low9304 =
            ProjectFromParent(BelowParent(1, 8, 0x100, false),
                lateMaskGlobal: null,
                successorEDXPlus8: 1,
                successorGlobal: 2,
                lateChildEsp: 0x1000,
                lateChildEbp: 0x2000,
                testedByte: 0);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan equalRoot =
            ProjectFromParent(EqualRootParent(), null, 1, 2,
                lateChildEsp: 0x1000, lateChildEbp: 0x2000, testedByte: 0);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan cleanEqualRoot =
            ProjectFromParent(EqualRootParent(), null, null, null);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan large9354 =
            ProjectFromParent(BelowParent(32, 0x100, 0x100, false),
                null, 1, 2, lateChildEsp: 0x1000, lateChildEbp: 0x2000, testedByte: 0);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan thresholdBelow =
            Project(5, zeroRoute: false, successorEDXPlus8: 1,
                successorGlobal: 2, lateChildEsp: 0x1000, lateChildEbp: 0x2000,
                testedByte: 0);
        check("Low 9304, equal-root 9441, large 9354, and unsigned-below 8E76 are not small-bin route completions",
            low9304.ParentPlan.Parent.StopAtRawFileOffsetExclusive == 0x012C9304
            && equalRoot.ParentPlan.Parent.ParentLifecycle.Outcome
                == CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                    .EqualRootStoredAndJumped
            && equalRoot.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C9441
            && cleanEqualRoot.ParentPlan.Parent.ParentLifecycle.Outcome
                == CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                    .EqualRootStoredAndJumped
            && cleanEqualRoot.ParentPlan.ExactCleanCompletion
            && cleanEqualRoot.ActualExactBoundedCompletion
            && !cleanEqualRoot.ParentRingCompletionTo9441
            && !cleanEqualRoot.ParentRingEpilogueAdmission
            && cleanEqualRoot.EpiloguePlan is null
            && cleanEqualRoot.ExactCleanCompletion
            && large9354.ParentPlan.Parent.StopAtRawFileOffsetExclusive == 0x012C9354
            && thresholdBelow.ParentPlan.NonzeroRoute?.Parent.JBTaken == true
            && thresholdBelow.ParentPlan.NonzeroBranchTargetRawFileOffset == 0x012C8E76
            && new[] { low9304, equalRoot, large9354, thresholdBelow }.All(plan =>
                !plan.ParentRingCompletionTo9441
                && plan.EpiloguePlan is null
                && !plan.Internal9441Entered
                && plan.LateChildEspProvidedButUnused
                && plan.LateChildEbpProvidedButUnused
                && plan.UnreachedTestedByteCaptureProvided
                && !plan.ExactCleanCompletion)
            && thresholdBelow.ParentActualExactBoundedCompletion
            && thresholdBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan dirtyZeroRoute =
            ProjectFromParent(
                BelowParent(5, 0x28, 0x100, false),
                lateMaskGlobal: 0,
                successorEDXPlus8: 0x80000010,
                successorGlobal: null);
        check("An unconsumed fresh successor capture blocks epilogue admission without losing the completed ring branch",
            dirtyZeroRoute.ParentRingCompletionTo9441
            && !dirtyZeroRoute.ParentRingEpilogueAdmission
            && !dirtyZeroRoute.ChildEntryPending
            && dirtyZeroRoute.EpiloguePlan is null
            && dirtyZeroRoute.UnreachedFreshSuccessorEDXPlus8CaptureProvided
            && dirtyZeroRoute.ParentPlan.FreshSuccessorCapturesUnused
            && dirtyZeroRoute.ParentActualExactBoundedCompletion
            && dirtyZeroRoute.ActualExactBoundedCompletion
            && dirtyZeroRoute.StopAtRawFileOffsetExclusive == 0x012C9441
            && !dirtyZeroRoute.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingFreshEDX =
            Project(5, zeroRoute: false, successorEDXPlus8: null,
                successorGlobal: 0x80000000);
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan missingFreshGlobal =
            Project(5, zeroRoute: false, successorEDXPlus8: 0x80000010,
                successorGlobal: null);
        check("Both independent fresh successor reads remain visible, preserve parent TEST flags until CMP, and never enter 9441",
            missingFreshEDX.ParentPlan.NonzeroRoute?.Parent is
            {
                FreshEdxPlus8ReadReached: true,
                FreshGlobalReadReached: false,
                CompareReached: false
            }
            && missingFreshEDX.StopAtRawFileOffsetExclusive == 0x012C9334
            && missingFreshEDX.ParentFlagsPreservedUntilChildTest
            && missingFreshEDX.FlagsAtStop == missingFreshEDX.ParentFlagsAtChildEntry
            && missingFreshEDX.UnreachedFreshSuccessorGlobal1EDADD8CaptureProvided
            && missingFreshGlobal.ParentPlan.NonzeroRoute?.Parent is
            {
                FreshEdxPlus8ReadReached: true,
                FreshGlobalReadReached: true,
                CompareReached: false
            }
            && missingFreshGlobal.StopAtRawFileOffsetExclusive == 0x012C9337
            && missingFreshGlobal.ParentFlagsPreservedUntilChildTest
            && missingFreshGlobal.FlagsAtStop == missingFreshGlobal.ParentFlagsAtChildEntry
            && missingFreshGlobal.ParentPlan.CombinedOrderedMemoryEffects.TakeLast(2)
                .All(effect => effect.ReadAttempted && !effect.WriteAttempted
                    && !effect.Committed)
            && !missingFreshEDX.Internal9441Entered
            && !missingFreshGlobal.Internal9441Entered);

        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan aliasedZero =
            CompleteZeroRoute(5, aliasNestedESIToIndexedBin: true,
                lateChildEsp: uint.MaxValue - 7, lateChildEbp: uint.MaxValue - 3);
        uint aliasBin31 = unchecked(0x01EDADF0 + (31u << 3));
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan aliasedNonzero =
            CompleteNonzeroRoute(31,
                successorEDXPlus8: aliasBin31,
                successorGlobal: 0x01000000,
                aliasNestedESIToIndexedBin: true);
        check("The aliased zero route inherits ESI from the clean root and commits its five ordered writes",
            aliasedZero.ParentPlan.ZeroRoute?.Writes.Length == 5
            && aliasedZero.ParentPlan.ZeroRoute.Writes[0].RawFileOffset == 0x012C931D
            && aliasedZero.ParentPlan.ZeroRoute.Writes[1].Address
                == aliasedZero.ParentPlan.ZeroRoute.Writes[3].Address
            && aliasedZero.ParentPlan.ZeroRoute.Writes[2].Address
                == aliasedZero.ParentPlan.ZeroRoute.Writes[4].Address
            && aliasedZero.ParentPlan.Parent.ParentLifecycle.ExactCleanCompletion
            && aliasedZero.InheritedESIValue == 0x01EDAE18
            && aliasedZero.InheritedESIDerivedFromParent
            && aliasedZero.InheritedESIConsumedByParentRing
            && aliasedZero.ParentRingEpilogueAdmission
            && aliasedZero.EpiloguePlan is
            {
                Outcome: CapacityFinalCalleeGatedEpilogueOutcome.CallSkipped
            }
            && aliasedZero.OrderedEffects.Take(
                    aliasedZero.ParentPlan.CombinedOrderedMemoryEffects.Length)
                .Select(effect => effect.RawFileOffset)
                .SequenceEqual(aliasedZero.ParentPlan.CombinedOrderedMemoryEffects
                    .Select(effect => (uint?)effect.RawFileOffset)));
        check("The aliased zero route preserves wrapped stack, frame, and RET addresses through 945C",
            aliasedZero.ParentRingCompletionTo9441
            && aliasedZero.ParentRingEpilogueAdmission
            && aliasedZero.ParentPlan.StopAtRawFileOffsetExclusive == 0x012C9441
            && aliasedZero.EpiloguePlan?.StackPopEdiAddress == uint.MaxValue - 7
            && aliasedZero.EpiloguePlan?.StackPopEsiAddress == uint.MaxValue - 3
            && aliasedZero.EpiloguePlan?.StackPopEbxAddress == 0
            && aliasedZero.EpiloguePlan?.MovEspSourceEbp == uint.MaxValue - 3
            && aliasedZero.EpiloguePlan?.ReturnTargetStackAddress == 0
            && aliasedZero.EpiloguePlan?.EspAfterReturn == 4
            && aliasedZero.EpiloguePlan?.ReturnReached == true
            && aliasedZero.StopAtRawFileOffsetExclusive == 0x012C945C
            && aliasedZero.ActualExactBoundedCompletion
            && aliasedZero.ExactCleanCompletion);
        check("The nonzero threshold reads independently, then four stores alias through inherited ESI and stop at 9441",
            aliasedNonzero.ParentRingEpilogueAdmission
            && aliasedNonzero.InheritedESIValue == aliasBin31
            && aliasedNonzero.InheritedESIDerivedFromParent
            && aliasedNonzero.InheritedESIConsumedByParentRing
            && aliasedNonzero.ParentPlan.NonzeroRoute?.Parent.Reads is
            [
                { RawFileOffset: 0x012C9334, AddressCaptured: true, ValueCaptured: true },
                { RawFileOffset: 0x012C9337, Address: 0x01EDADD8, AddressCaptured: true,
                    ValueCaptured: true }
            ]
            && aliasedNonzero.ParentPlan.NonzeroRoute?.Parent.CompareLeft == aliasBin31
            && aliasedNonzero.ParentPlan.NonzeroRoute?.Parent.CompareRight == 0x01000000
            && aliasedNonzero.ParentPlan.NonzeroRoute.Writes.Length == 4
            && aliasedNonzero.ParentPlan.NonzeroRoute.BranchTargetRawFileOffset == 0x012C9441
            && aliasedNonzero.ParentPlan.NonzeroRoute.StopAtRawFileOffsetExclusive
                == 0x012C9441
            && aliasedNonzero.ParentPlan.NonzeroRoute.Writes[0].Address
                == aliasedNonzero.ParentPlan.NonzeroRoute.Writes[2].Address
            && aliasedNonzero.ParentPlan.NonzeroRoute.Writes[1].Address
                == aliasedNonzero.ParentPlan.NonzeroRoute.Writes[3].Address
            && aliasedNonzero.ActualExactBoundedCompletion
            && aliasedNonzero.ExactCleanCompletion
            && aliasedNonzero.StopAtRawFileOffsetExclusive == 0x012C945C
            && aliasedNonzero.ParentPlan.CombinedOrderedMemoryEffects
                .Where(effect =>
                    effect.Kind is
                        CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .SuccessorEDXPlus8Read
                        or CapacityFinalCalleeLargeRightRootSmallBinRingLifecycleMemoryEffectKind
                            .SuccessorGlobal1EDADD8Read)
                .All(effect => !effect.ForwardedFromPriorWrite && !effect.Committed));
    }

    private static CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan
        CompleteZeroRoute(
            uint index,
            bool aliasNestedESIToIndexedBin = false,
            uint lateChildEsp = 0x1000,
            uint lateChildEbp = 0x2000) =>
        Project(
            index,
            zeroRoute: true,
            aliasNestedESIToIndexedBin: aliasNestedESIToIndexedBin,
            lateChildEsp: lateChildEsp,
            lateChildEbp: lateChildEbp,
            testedByte: 0,
            stackPopEdi: 1,
            stackPopEsi: 2,
            stackPopEbx: 3,
            stackPopEbp: 4,
            returnTarget: 5);

    private static CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan
        CompleteNonzeroRoute(
            uint index,
            uint successorEDXPlus8 = 0x80000010,
            uint successorGlobal = 0x80000000,
            bool aliasNestedESIToIndexedBin = false) =>
        Project(
            index,
            zeroRoute: false,
            successorEDXPlus8: successorEDXPlus8,
            successorGlobal: successorGlobal,
            aliasNestedESIToIndexedBin: aliasNestedESIToIndexedBin,
            lateChildEsp: 0x3000,
            lateChildEbp: 0x4000,
            testedByte: 2,
            stackPopEdi: 1,
            stackPopEsi: 2,
            stackPopEbx: 3,
            callCellTarget: 0x12345678,
            callReturnedNormally: true,
            postCallEbp: 0x5000,
            stackPopEbp: 4,
            returnTarget: 5);

    private static CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan Project(
        uint index,
        bool zeroRoute,
        uint? successorEDXPlus8 = 0x80000010,
        uint? successorGlobal = 0x80000000,
        bool aliasNestedESIToIndexedBin = false,
        uint? lateChildEsp = 0x1000,
        uint? lateChildEbp = 0x2000,
        byte? testedByte = null,
        uint? stackPopEdi = null,
        uint? stackPopEsi = null,
        uint? stackPopEbx = null,
        uint? callCellTarget = null,
        bool? callReturnedNormally = null,
        uint? postCallEbp = null,
        uint? stackPopEbp = null,
        uint? returnTarget = null) =>
        ProjectFromParent(
            BelowParent(
                index,
                index << 3,
                0x100,
                aliasNestedESIToIndexedBin),
            lateMaskGlobal: zeroRoute ? 0 : 1u << (int)index,
            successorEDXPlus8: zeroRoute ? null : successorEDXPlus8,
            successorGlobal: zeroRoute ? null : successorGlobal,
            lateChildEsp,
            lateChildEbp,
            testedByte,
            stackPopEdi,
            stackPopEsi,
            stackPopEbx,
            callCellTarget,
            callReturnedNormally,
            postCallEbp,
            stackPopEbp,
            returnTarget);

    private static CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecyclePlan
        ProjectFromParent(
            CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture parent,
            uint? lateMaskGlobal,
            uint? successorEDXPlus8,
            uint? successorGlobal,
            uint? lateChildEsp = null,
            uint? lateChildEbp = null,
            byte? testedByte = null,
            uint? stackPopEdi = null,
            uint? stackPopEsi = null,
            uint? stackPopEbx = null,
            uint? callCellTarget = null,
            bool? callReturnedNormally = null,
            uint? postCallEbp = null,
            uint? stackPopEbp = null,
            uint? returnTarget = null) =>
        CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleProjection.Project(new(
            new(
                new(parent, lateMaskGlobal),
                successorEDXPlus8,
                successorGlobal),
            lateChildEsp,
            lateChildEbp,
            testedByte,
            stackPopEdi,
            stackPopEsi,
            stackPopEbx,
            callCellTarget,
            callReturnedNormally,
            postCallEbp,
            stackPopEbp,
            returnTarget));

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture BelowParent(
        uint index,
        uint originalEDI,
        uint? lateESI,
        bool aliasNestedESIToIndexedBin)
    {
        uint? derivedESI = aliasNestedESIToIndexedBin
            ? unchecked(0x01EDADF0 + (index << 3))
            : lateESI;
        return new(
            Parent: new(
                Parent: new(
                    EntryEDX: 0x200,
                    FreshEDXPlus14: 2,
                    FreshGlobalValue: 2,
                    LateEntryEAX: 0x200,
                    LateEntryEDI: originalEDI,
                    LateEntryESI: derivedESI),
                FreshRootGlobalDWORD: 2));
    }

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture
        EqualRootParent() =>
            new(
                Parent: new(
                    Parent: new(
                        EntryEDX: 0x200,
                        FreshEDXPlus14: 0,
                        FreshGlobalValue: null,
                        LateEntryEAX: null,
                        LateEntryEDI: 0x28,
                        LateEntryESI: 0x80),
                    FreshRootGlobalDWORD: 0x80));

    private static CapacityFinalCalleeLargeRightRootSmallBinEpilogueLifecycleFlags?
        ConvertOrFlags(CapacityFinalCalleeCommonMaskOrStoreFlags? flags) =>
            flags is { } value
                ? new(
                    value.Carry,
                    value.Parity,
                    value.AuxiliaryCarry,
                    value.Zero,
                    value.Sign,
                    value.Overflow)
                : null;
}
