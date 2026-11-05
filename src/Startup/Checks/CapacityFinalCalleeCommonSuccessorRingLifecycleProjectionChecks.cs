using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonSuccessorRingLifecycleProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        var missingEntry = Project(null, 0x2000, 1, 0x3000);
        var missingField = Project(0x1000, null, 1, 0x3000);
        check("Missing successor prerequisites cannot admit or derive the ring child",
            missingEntry.Child is null
            && missingEntry.DerivedChildEntryEDX is null
            && missingEntry.DerivedChildEntryEAX is null
            && missingEntry.StopAtRawFileOffsetExclusive == 0x012C9334
            && !missingEntry.ExactCleanCompletion
            && missingField.Child is null
            && missingField.DerivedChildEntryEAX is null
            && missingField.StopAtRawFileOffsetExclusive == 0x012C9334
            && !missingField.ExactCleanCompletion
            && missingField.UnreachedChildESICaptureProvided);
        CapacityFinalCalleeCommonSuccessorRingLifecyclePlan below =
            Project(edx: 0x1000, field: 1, global: 2, esi: null);
        CapacityFinalCalleeCommonSuccessorRingLifecyclePlan belowWithUnusedESI =
            Project(edx: 0x1000, field: 1, global: 2, esi: 0x3000);
        check("Unsigned below selects 8E76 and never enters the ring; supplied ESI is reported unused",
            below.Outcome
                == CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                    .ParentUnsignedBelowBranchTarget
            && below.Parent.JBTaken == true
            && below.Parent.JBSelectedDestinationRawFileOffset == 0x012C8E76
            && below.StopAtRawFileOffsetExclusive == 0x012C8E76
            && below.Child is null
            && !below.UnreachedChildESICaptureProvided
            && below.ExactCleanCompletion
            && belowWithUnusedESI.Child is null
            && belowWithUnusedESI.UnreachedChildESICaptureProvided
            && !belowWithUnusedESI.ExactCleanCompletion);

        CapacityFinalCalleeCommonSuccessorRingLifecyclePlan missingGlobal =
            Project(edx: 0x1000, field: 0x2000, global: null, esi: 0x3000);
        check("Missing parent global retains the fresh EAX read and stops before CMP without child execution",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                    .ParentMissingFreshGlobal1EDADD8
            && missingGlobal.Parent.FreshEdxPlus8ReadReached
            && missingGlobal.Parent.FreshEdxPlus8Captured
                && missingGlobal.Parent.FreshEdxPlus8 == 0x2000
                && !missingGlobal.DerivedChildEntryEAXCaptured
                && missingGlobal.DerivedChildEntryEAX is null
                && !missingGlobal.DerivedChildEntryEAXUnused
            && !missingGlobal.Parent.CompareReached
            && missingGlobal.FlagsAtStop is null
            && missingGlobal.Parent.JBReached == false
            && missingGlobal.Child is null
            && missingGlobal.UnreachedChildESICaptureProvided
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C9337);

        const uint parentEDX = 0x11112222;
        const uint freshEAX = 0x33334444;
        const uint childESI = 0x55556666;
        CapacityFinalCalleeCommonSuccessorRingLifecyclePlan completed =
            Project(parentEDX, freshEAX, freshEAX, childESI);
        check("Exact not-below handoff derives child EDX/EAX and commits the ordered shared ring writes",
            completed.Outcome
                == CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                    .CompletedTo012C9441
            && completed.ParentExactBranchHandoff
            && completed.Parent.JBTaken == false
            && completed.Parent.JBSelectedDestinationRawFileOffset == 0x012C9343
            && completed.StopAtRawFileOffsetExclusive == 0x012C9441
            && completed.DerivedChildEntryEDX == parentEDX
            && completed.DerivedChildEntryEAX == freshEAX
            && completed.ChildCapture.HasValue
            && completed.ChildCapture.Value.EntryEDX == parentEDX
            && completed.ChildCapture.Value.EntryEAX == freshEAX
            && completed.ChildCapture.Value.EntryESI == childESI
            && !completed.UnreachedChildESICaptureProvided
            && completed.Writes is
                [
                    { RawFileOffset: 0x012C9343, Address: 0x1111222A, Value: childESI, Committed: true },
                    { RawFileOffset: 0x012C9346, Address: 0x33334450, Value: childESI, Committed: true },
                    { RawFileOffset: 0x012C9349, Address: 0x5555666E, Value: freshEAX, Committed: true },
                    { RawFileOffset: 0x012C934C, Address: 0x55556672, Value: parentEDX, Committed: true }
                ]
            && completed.Writes[1].Address != unchecked(parentEDX + 0xC)
            && completed.Writes[2].Value != parentEDX
            && completed.BranchReached
            && completed.BranchTargetRawFileOffset == 0x012C9441
            && completed.ExactCleanCompletion
            && completed.ChildFlagsUnaffected
            && completed.FlagsAtStop == completed.Parent.CompareFlags
            && !completed.DestinationEntered
            && !completed.EpilogueEntered);

        CapacityFinalCalleeCommonSuccessorRingLifecyclePlan missingESI =
            Project(parentEDX, freshEAX, freshEAX, esi: null);
        check("Missing late ESI attempts only EDX+8 and leaves derived EAX unused rather than externally captured",
            missingESI.Outcome
                == CapacityFinalCalleeCommonSuccessorRingLifecycleOutcome
                    .ChildMissingESIForFirstWrite
            && missingESI.Child is
                {
                    EntryEDXReadReached: true,
                    EntryESIReadReached: true,
                    EntryEAXReadReached: false,
                    UnreachedEntryEAXCaptureProvided: false,
                    BranchReached: false,
                    StopAtRawFileOffsetExclusive: 0x012C9343
                }
            && missingESI.ChildCapture.HasValue
            && missingESI.ChildCapture.Value.EntryEAX is null
            && missingESI.Writes is
                [{
                    RawFileOffset: 0x012C9343,
                    Address: 0x1111222A,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null,
                    Committed: false
                }]
            && missingESI.DerivedChildEntryEAXCaptured
            && missingESI.DerivedChildEntryEAXUnused
            && !missingESI.ExternalUnusedChildEntryEAXCaptureProvided
            && missingESI.FlagsAtStop == missingESI.Parent.CompareFlags
            && !missingESI.ExactCleanCompletion);

        CapacityFinalCalleeCommonSuccessorRingLifecyclePlan wrapping =
            Project(uint.MaxValue - 7, uint.MaxValue, uint.MaxValue, uint.MaxValue);
        CapacityFinalCalleeCommonSuccessorRingLifecyclePlan maximum =
            Project(uint.MaxValue, uint.MaxValue, uint.MaxValue, uint.MaxValue);
        check("Wrapped, zero, maximum, and aliased addresses remain ordered DWORD stores",
            wrapping.Child is not null
            && wrapping.Writes.Length == 4
            && wrapping.Writes[0].Address == 0
            && wrapping.Writes[1].Address == 0xB
            && wrapping.Writes[2].Address == 7
            && wrapping.Writes[3].Address == 0xB
            && wrapping.Writes.Select(write => write.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C9343, 0x012C9346, 0x012C9349, 0x012C934C })
            && maximum.Writes.Length == 4
            && maximum.Writes[0].Address == 7
            && maximum.Writes[1].Address == 0xB
            && maximum.Writes[2].Address == 7
            && maximum.Writes[3].Address == 0xB
            && maximum.Writes.All(write => write.Committed)
            && maximum.BranchReached
            && maximum.StopAtRawFileOffsetExclusive == 0x012C9441);
    }

    private static CapacityFinalCalleeCommonSuccessorRingLifecyclePlan Project(
        uint? edx,
        uint? field,
        uint? global,
        uint? esi) =>
        CapacityFinalCalleeCommonSuccessorRingLifecycleProjection.Project(new(
            edx,
            field,
            global,
            esi));
}
