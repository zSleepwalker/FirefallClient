using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeBacklinkSuccessorLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeLargeBacklinkSuccessorLifecyclePlan allMissing =
            CapacityFinalCalleeLargeBacklinkSuccessorLifecycleProjection.Project(new(
                EntryEBP: null,
                FreshFrameMinus14: null,
                EntryEDX: null,
                FreshBacklink: null,
                ChildEntryEAX: null,
                ChildFreshEaxPlus8: null));
        check("All missing upstream evidence stops before creating a child",
            allMissing.Outcome
                == CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .MissingUpstreamEntryEBP
            && allMissing.BacklinkGate.Outcome
                == CapacityFinalCalleeLargeFrameBacklinkGateOutcome.MissingEntryEBP
            && allMissing.SuccessorWrites is null
            && !allMissing.ChildReached
            && !allMissing.UpstreamEDXRetainedForChild
            && allMissing.FlagsAtStop is null
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C91B9);

        CapacityFinalCalleeLargeBacklinkSuccessorLifecyclePlan missingParentEDX =
            CapacityFinalCalleeLargeBacklinkSuccessorLifecycleProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus14: 0x100,
                EntryEDX: null,
                FreshBacklink: 0x300,
                ChildEntryEAX: 0x400,
                ChildFreshEaxPlus8: 0x500));
        check("Missing parent EDX retains the independent backlink read and rejects child captures",
            missingParentEDX.Outcome
                == CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .MissingUpstreamEntryEDX
            && missingParentEDX.BacklinkGate.BacklinkReadReached
            && missingParentEDX.BacklinkGate.BacklinkAddress == 0x10C
            && missingParentEDX.BacklinkGate.BacklinkCaptured
            && missingParentEDX.BacklinkGate.BacklinkValue == 0x300
            && missingParentEDX.SuccessorWrites is null
            && !missingParentEDX.ChildReached
            && missingParentEDX.UnreachedChildEntryEAXCaptureProvided
            && missingParentEDX.UnreachedChildFreshFieldCaptureProvided
            && missingParentEDX.FlagsAtStop is null
            && missingParentEDX.StopAtRawFileOffsetExclusive == 0x012C91BC);

        CapacityFinalCalleeLargeBacklinkSuccessorLifecyclePlan missingParentFrame =
            CapacityFinalCalleeLargeBacklinkSuccessorLifecycleProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus14: null,
                EntryEDX: 0x30,
                FreshBacklink: 0x40,
                ChildEntryEAX: 0x400,
                ChildFreshEaxPlus8: 0x500));
        check("Missing parent frame stops before EDX and backlink use without creating a child",
            missingParentFrame.Outcome
                == CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .MissingUpstreamFreshFrameMinus14
            && missingParentFrame.BacklinkGate.Reads.Length == 1
            && missingParentFrame.BacklinkGate.UnreachedEntryEDXCaptureProvided
            && missingParentFrame.BacklinkGate.UnreachedFreshBacklinkCaptureProvided
            && missingParentFrame.SuccessorWrites is null
            && missingParentFrame.UnreachedChildEntryEAXCaptureProvided
            && missingParentFrame.UnreachedChildFreshFieldCaptureProvided
            && missingParentFrame.FlagsAtStop is null
            && missingParentFrame.StopAtRawFileOffsetExclusive == 0x012C91B9);

        CapacityFinalCalleeLargeBacklinkSuccessorLifecyclePlan missingParentBacklink =
            CapacityFinalCalleeLargeBacklinkSuccessorLifecycleProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus14: 0x100,
                EntryEDX: 0x30,
                FreshBacklink: null,
                ChildEntryEAX: 0x400,
                ChildFreshEaxPlus8: 0x500));
        check("Missing parent backlink preserves both parent reads and never creates a child",
            missingParentBacklink.Outcome
                == CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .MissingUpstreamFreshBacklink
            && missingParentBacklink.BacklinkGate.Reads.Length == 2
            && missingParentBacklink.BacklinkGate.BacklinkReadReached
            && missingParentBacklink.BacklinkGate.BacklinkAddress == 0x10C
            && !missingParentBacklink.BacklinkGate.BacklinkCaptured
            && missingParentBacklink.SuccessorWrites is null
            && missingParentBacklink.UnreachedChildEntryEAXCaptureProvided
            && missingParentBacklink.UnreachedChildFreshFieldCaptureProvided
            && missingParentBacklink.FlagsAtStop is null
            && missingParentBacklink.StopAtRawFileOffsetExclusive == 0x012C91BC);

        CapacityFinalCalleeLargeBacklinkSuccessorLifecyclePlan upstreamUnequal =
            CapacityFinalCalleeLargeBacklinkSuccessorLifecycleProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus14: 0x100,
                EntryEDX: 0x20,
                FreshBacklink: 0x21,
                ChildEntryEAX: 0x400,
                ChildFreshEaxPlus8: 0x500));
        check("Upstream inequality rejects both child captures at the bounded 8E76 exit",
            upstreamUnequal.Outcome
                == CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .UpstreamUnequalTo8E76
            && upstreamUnequal.BacklinkGate.JnzTaken == true
            && upstreamUnequal.BacklinkGate.JnzSelectedDestinationRawFileOffset == 0x012C8E76
            && upstreamUnequal.SuccessorWrites is null
            && !upstreamUnequal.ChildReached
            && upstreamUnequal.UnreachedChildEntryEAXCaptureProvided
            && upstreamUnequal.UnreachedChildFreshFieldCaptureProvided
            && !upstreamUnequal.ExactCleanExitAdmitted
            && upstreamUnequal.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeBacklinkSuccessorLifecyclePlan missingChildEAX =
            CapacityFinalCalleeLargeBacklinkSuccessorLifecycleProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus14: 0x100,
                EntryEDX: 0x20,
                FreshBacklink: 0x20,
                ChildEntryEAX: null,
                ChildFreshEaxPlus8: 0x20));
        check("Parent EDX is retained for the child CMP only after child EAX provides its address",
            missingChildEAX.BacklinkGate.Outcome
                == CapacityFinalCalleeLargeFrameBacklinkGateOutcome.EqualToFallthrough
            && missingChildEAX.ChildReached
            && !missingChildEAX.UpstreamEDXRetainedForChild
            && missingChildEAX.ChildEntryEDX is null
            && missingChildEAX.SuccessorWrites is
            {
                Outcome: CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingEntryEAX,
                FreshFieldReadReached: false,
                CompareReached: false,
                UnreachedFreshFieldCaptureProvided: true
            }
            && missingChildEAX.UnreachedChildFreshFieldCaptureProvided
            && missingChildEAX.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && !missingChildEAX.ChildLateEBXDerivedFromFreshFrameMinus14
            && missingChildEAX.SuccessorWrites!.Capture.LateEntryEBX is null
            && missingChildEAX.StopAtRawFileOffsetExclusive == 0x012C91C5);

        CapacityFinalCalleeLargeBacklinkSuccessorLifecyclePlan missingChildField =
            CapacityFinalCalleeLargeBacklinkSuccessorLifecycleProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus14: 0x100,
                EntryEDX: 0x20,
                FreshBacklink: 0x20,
                ChildEntryEAX: 0x400,
                ChildFreshEaxPlus8: null));
        check("Child fresh field is an independent read and missing data preserves parent CMP flags",
            missingChildField.UpstreamEDXRetainedForChild
            && missingChildField.ChildEntryEDX == 0x20
            && missingChildField.SuccessorWrites is
            {
                Outcome: CapacityFinalCalleeLargeSuccessorWritesOutcome.MissingFreshField,
                FreshFieldAddress: 0x408,
                FreshFieldCaptured: false,
                CompareReached: false
            }
            && missingChildField.SuccessorWrites!.Capture.FreshEaxPlus8 is null
            && !missingChildField.ChildLateEBXDerivedFromFreshFrameMinus14
            && missingChildField.SuccessorWrites.Capture.LateEntryEBX is null
            && missingChildField.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && missingChildField.StopAtRawFileOffsetExclusive == 0x012C91C5);

        CapacityFinalCalleeLargeBacklinkSuccessorLifecyclePlan childUnequal =
            CapacityFinalCalleeLargeBacklinkSuccessorLifecycleProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus14: 0x100,
                EntryEDX: 0x20,
                FreshBacklink: 0x20,
                ChildEntryEAX: 0x104,
                ChildFreshEaxPlus8: 0x21));
        check("Aliased parent and child read addresses retain separate values and hold back late EBX",
            childUnequal.BacklinkGate.BacklinkAddress == 0x10C
            && childUnequal.BacklinkGate.BacklinkValue == 0x20
            && childUnequal.SuccessorWrites is
            {
                FreshFieldAddress: 0x10C,
                FreshFieldCaptured: true,
                FreshFieldValue: 0x21,
                CompareResult: 1,
                JnzTaken: true,
                JnzSelectedDestinationRawFileOffset: 0x012C8E76
            }
            && childUnequal.Outcome
                == CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .ChildUnequalTo8E76
            && !childUnequal.ChildLateEBXDerivedFromFreshFrameMinus14
            && childUnequal.ChildLateEBX is null
            && childUnequal.SuccessorWrites!.Capture.LateEntryEBX is null
            && childUnequal.SuccessorWrites.Writes.IsEmpty
            && childUnequal.FlagsAtStop is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && childUnequal.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeBacklinkSuccessorLifecyclePlan childEqualWrapped =
            CapacityFinalCalleeLargeBacklinkSuccessorLifecycleProjection.Project(new(
                EntryEBP: 0x10,
                FreshFrameMinus14: 0xFFFFFFF4,
                EntryEDX: 0,
                FreshBacklink: 0,
                ChildEntryEAX: 0xFFFFFFF8,
                ChildFreshEaxPlus8: 0));
        check("Both clean equal gates reach ordered wrapped stores with independently captured zero",
            childEqualWrapped.Outcome
                == CapacityFinalCalleeLargeBacklinkSuccessorLifecycleOutcome
                    .ChildEqualTo9229
            && childEqualWrapped.BacklinkGate.Outcome
                == CapacityFinalCalleeLargeFrameBacklinkGateOutcome.EqualToFallthrough
            && childEqualWrapped.BacklinkGate.BacklinkAddress == 0
            && childEqualWrapped.SuccessorWrites is
            {
                Outcome: CapacityFinalCalleeLargeSuccessorWritesOutcome.EqualTo9229,
                FreshFieldAddress: 0,
                FreshFieldCaptured: true,
                FreshFieldValue: 0,
                CompareResult: 0,
                CompareFlags:
                {
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: false,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                },
                FirstStoreReached: true,
                FirstStoreAddress: 0,
                FirstStoreValue: 0xFFFFFFF8,
                SecondStoreReached: true,
                SecondStoreAddress: 0,
                SecondStoreValue: 0xFFFFFFF4,
                JumpTargetRawFileOffset: 0x012C9229,
                FlagsPreservedAcrossStores: true,
                ExactCleanExitAdmitted: true
            }
            && childEqualWrapped.ChildLateEBXDerivedFromFreshFrameMinus14
            && childEqualWrapped.ChildLateEBX == 0xFFFFFFF4
            && childEqualWrapped.SuccessorWrites!.Capture.LateEntryEBX == 0xFFFFFFF4
            && childEqualWrapped.SuccessorWrites.Writes.Select(write =>
                (write.RawFileOffset, write.Address, write.Value)).SequenceEqual(new[]
                {
                    (0x012C91CEu, 0u, 0xFFFFFFF8u),
                    (0x012C91D1u, 0u, 0xFFFFFFF4u)
                })
            && childEqualWrapped.ExactCleanExitAdmitted
            && childEqualWrapped.StopAtRawFileOffsetExclusive == 0x012C9229);

        CapacityFinalCalleeLargeBacklinkSuccessorLifecyclePlan childAllFlags =
            CapacityFinalCalleeLargeBacklinkSuccessorLifecycleProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus14: 0x100,
                EntryEDX: 1,
                FreshBacklink: 1,
                ChildEntryEAX: 0x200,
                ChildFreshEaxPlus8: 0x80000000));
        check("Child CMP exposes all six exact flags without deriving late EBX on inequality",
            childAllFlags.SuccessorWrites is
            {
                CompareResult: 0x7FFFFFFF,
                CompareFlags:
                {
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: true,
                    Zero: false,
                    Sign: false,
                    Overflow: true
                },
                JnzTaken: true,
                JnzSelectedDestinationRawFileOffset: 0x012C8E76
            }
            && childAllFlags.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && !childAllFlags.ChildLateEBXDerivedFromFreshFrameMinus14
            && childAllFlags.SuccessorWrites!.Capture.LateEntryEBX is null
            && childAllFlags.ExactCleanExitAdmitted
            && childAllFlags.StopAtRawFileOffsetExclusive == 0x012C8E76);
    }
}
