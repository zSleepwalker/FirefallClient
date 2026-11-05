using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonOrRingLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonOrRingLifecyclePlan missingECX =
            CapacityFinalCalleeCommonOrRingLifecycleProjection.Project(new(
                ParentEntryECX: null,
                ParentEntryEDI: 0x10,
                ChildEntryEDX: null,
                ChildEntryEAX: 0x20,
                ChildEntryESI: 0x30));
        check("Missing parent ECX retains the external EAX capture and never enters the child",
            missingECX.Outcome
                == CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingParentECX
            && !missingECX.ParentExactCleanHandoffVerified
            && !missingECX.GlobalWriteReached
            && missingECX.ChildPlan is null
            && missingECX.Stages.Length == 1
            && missingECX.Writes.IsEmpty
            && missingECX.ExternalUnusedChildEntryEAXCaptureProvided
            && !missingECX.ChildEntryEAXCaptureWithheld);

        CapacityFinalCalleeCommonOrRingLifecyclePlan missingEDI =
            CapacityFinalCalleeCommonOrRingLifecycleProjection.Project(new(
                ParentEntryECX: 0x10,
                ParentEntryEDI: null,
                ChildEntryEDX: 0x1000,
                ChildEntryEAX: 0x2000,
                ChildEntryESI: 0x3000));
        check("Missing parent EDI retains ECX and child captures without fabricating a parent write",
            missingEDI.Outcome
                == CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingParentEDI
            && missingEDI.ParentPlan is
                { EntryECXCaptured: true, EntryEDICaptured: false }
            && !missingEDI.GlobalWriteReached
            && missingEDI.Stages.Length == 1
            && missingEDI.ChildPlan is null
            && missingEDI.ExternalUnusedChildEntryEAXCaptureProvided
            && missingEDI.Writes.IsEmpty);

        CapacityFinalCalleeCommonOrRingLifecyclePlan missingChildEDX =
            CapacityFinalCalleeCommonOrRingLifecycleProjection.Project(new(
                ParentEntryECX: 0,
                ParentEntryEDI: 0,
                ChildEntryEDX: null,
                ChildEntryEAX: 0x2000,
                ChildEntryESI: 0x3000));
        check("Parent global write survives missing child EDX and child EAX remains externally unused",
            missingChildEDX.Outcome
                == CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingChildEDXForFirstWrite
            && missingChildEDX.ParentExactCleanHandoffVerified
            && missingChildEDX.GlobalWriteReached
            && missingChildEDX.GlobalWriteAddress == 0x01EDADC8
            && missingChildEDX.GlobalWriteValue == 0
            && missingChildEDX.Writes.Length == 2
            && missingChildEDX.Writes[0] is
                {
                    RawFileOffset: 0x012C931D,
                    IsParentGlobalWrite: true,
                    Committed: true,
                    Value: 0
                }
            && missingChildEDX.Writes[1] is
                {
                    RawFileOffset: 0x012C9323,
                    IsParentGlobalWrite: false,
                    Address: null,
                    AddressCaptured: false,
                    Value: 0x3000,
                    ValueCaptured: true,
                    Committed: false
                }
            && missingChildEDX.ChildEntryEAXCaptureWithheld
            && missingChildEDX.ExternalUnusedChildEntryEAXCaptureProvided
            && missingChildEDX.FlagsAtStop == missingChildEDX.ParentPlan?.OrFlags
            && missingChildEDX.StopAtRawFileOffsetExclusive == 0x012C9323);

        CapacityFinalCalleeCommonOrRingLifecyclePlan missingChildESI =
            CapacityFinalCalleeCommonOrRingLifecycleProjection.Project(new(
                ParentEntryECX: 0x10,
                ParentEntryEDI: 0x20,
                ChildEntryEDX: 0x1000,
                ChildEntryEAX: 0x2000,
                ChildEntryESI: null));
        check("Parent global write and known child address survive missing first-store ESI",
            missingChildESI.Outcome
                == CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingChildESIForFirstWrite
            && missingChildESI.GlobalWriteReached
            && missingChildESI.Writes.Length == 2
            && missingChildESI.Writes[1] is
                {
                    RawFileOffset: 0x012C9323,
                    Address: 0x1008,
                    AddressCaptured: true,
                    Value: null,
                    ValueCaptured: false,
                    Committed: false
                }
            && missingChildESI.ChildEntryEAXCaptureWithheld
            && missingChildESI.ExternalUnusedChildEntryEAXCaptureProvided
            && missingChildESI.StopAtRawFileOffsetExclusive == 0x012C9323);

        CapacityFinalCalleeCommonOrRingLifecyclePlan missingChildEAX =
            CapacityFinalCalleeCommonOrRingLifecycleProjection.Project(new(
                ParentEntryECX: 1,
                ParentEntryEDI: 2,
                ChildEntryEDX: 0x1000,
                ChildEntryEAX: null,
                ChildEntryESI: 0x3000));
        check("Missing child EAX preserves the parent write and first child commit before the second attempt",
            missingChildEAX.Outcome
                == CapacityFinalCalleeCommonOrRingLifecycleOutcome.MissingChildEAXForSecondWrite
            && missingChildEAX.GlobalWriteReached
            && missingChildEAX.Writes.Length == 3
            && missingChildEAX.Writes[0].IsParentGlobalWrite
            && missingChildEAX.Writes[1] is
                {
                    RawFileOffset: 0x012C9323,
                    Address: 0x1008,
                    Value: 0x3000,
                    Committed: true
                }
            && missingChildEAX.Writes[2] is
                {
                    RawFileOffset: 0x012C9326,
                    Address: null,
                    AddressCaptured: false,
                    Value: 0x3000,
                    ValueCaptured: true,
                    Committed: false
                }
            && !missingChildEAX.ChildEntryEAXCaptureWithheld
            && !missingChildEAX.ExternalUnusedChildEntryEAXCaptureProvided
            && missingChildEAX.StopAtRawFileOffsetExclusive == 0x012C9326);

        CapacityFinalCalleeCommonOrRingLifecyclePlan zero =
            CapacityFinalCalleeCommonOrRingLifecycleProjection.Project(new(0, 0, 0, 0, 0));
        check("Zero parent and child operands preserve OR flags and complete at 9441 without entering the epilogue",
            zero.Outcome
                == CapacityFinalCalleeCommonOrRingLifecycleOutcome.CompletedTo012C9441
            && zero.ParentExactCleanHandoffVerified
            && zero.GlobalWriteAddress == 0x01EDADC8
            && zero.GlobalWriteValue == 0
            && zero.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.FlagsAtStop == zero.ParentPlan?.OrFlags
            && zero.ChildFlagsUnaffected
            && zero.BranchReached
            && zero.BranchTargetRawFileOffset == 0x012C9441
            && zero.ExactCleanCompletion
            && zero.StopAtRawFileOffsetExclusive == 0x012C9441
            && !zero.DestinationEntered
            && !zero.EpilogueEntered
            && zero.Stages is
                [
                    { Kind: CapacityFinalCalleeCommonOrRingLifecycleStageKind.ParentOrStore, ExactCleanStop: true },
                    { Kind: CapacityFinalCalleeCommonOrRingLifecycleStageKind.ChildRingWrites, ExactCleanStop: true }
                ]
            && zero.Stages[1].FlagsAtStop == zero.ParentPlan?.OrFlags
            && zero.Writes.Length == 5
            && zero.Writes[0].RawFileOffset == 0x012C931D
            && zero.Writes.Skip(1).All(write => write.Committed));

        CapacityFinalCalleeCommonOrRingLifecyclePlan wrapping =
            CapacityFinalCalleeCommonOrRingLifecycleProjection.Project(new(
                0xFFFFFFFF, 0x80000000,
                0xFFFFFFFC, 0xFFFFFFF8, 0xFFFFFFF8));
        check("Child address arithmetic wraps independently while parent flags retain the parent OR result",
            wrapping.GlobalWriteValue == 0xFFFFFFFF
            && wrapping.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && wrapping.Writes.Length == 5
            && wrapping.Writes[1].Address == 4
            && wrapping.Writes[2].Address == 4
            && wrapping.Writes[3].Address == 0
            && wrapping.Writes[4].Address == 4
            && wrapping.ExactCleanCompletion);

        CapacityFinalCalleeCommonOrRingLifecyclePlan maximum =
            CapacityFinalCalleeCommonOrRingLifecycleProjection.Project(new(
                uint.MaxValue, 0x80000000,
                uint.MaxValue, uint.MaxValue, uint.MaxValue));
        check("Maximum captures retain unsigned DWORD values and distinct ordered writes",
            maximum.GlobalWriteValue == uint.MaxValue
            && maximum.Writes is
                [
                    { RawFileOffset: 0x012C931D, Address: 0x01EDADC8, Value: uint.MaxValue },
                    { RawFileOffset: 0x012C9323, Address: 7, Value: uint.MaxValue },
                    { RawFileOffset: 0x012C9326, Address: 0xB, Value: uint.MaxValue },
                    { RawFileOffset: 0x012C9329, Address: 7, Value: uint.MaxValue },
                    { RawFileOffset: 0x012C932C, Address: 0xB, Value: uint.MaxValue }
                ]
            && maximum.ExactCleanCompletion);

        CapacityFinalCalleeCommonOrRingLifecyclePlan aliasing =
            CapacityFinalCalleeCommonOrRingLifecycleProjection.Project(new(
                ParentEntryECX: 0x1111,
                ParentEntryEDI: 0x2222,
                ChildEntryEDX:
                    CapacityFinalCalleeCommonMaskOrStoreProjection.GlobalVirtualAddress - 8,
                ChildEntryEAX: 0x5555,
                ChildEntryESI: 0x4444));
        check("Aliased parent global and child target uses fresh child captures without deriving its value",
            aliasing.Writes.Length == 5
            && aliasing.GlobalWriteAddress == 0x01EDADC8
            && aliasing.Writes[0].Address == aliasing.Writes[1].Address
            && aliasing.Writes[0] is
                {
                    Address: 0x01EDADC8,
                    Value: 0x3333,
                    IsParentGlobalWrite: true
                }
            && aliasing.Writes[1] is
                {
                    Address: 0x01EDADC8,
                    Value: 0x4444,
                    IsParentGlobalWrite: false
                }
            && aliasing.Writes[2].Value == 0x4444
            && aliasing.Writes[3].Value == 0x5555
            && aliasing.Writes[4].Value == 0x01EDADC0
            && aliasing.Writes[3].Value != aliasing.Writes[0].Value
            && aliasing.ExactCleanCompletion);
    }
}
