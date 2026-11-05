using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonHighSizeNodeLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition preserves the bounded 9354-to-93A1 evidence window",
            CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.EntryRawFileOffset
                == 0x012C9354
            && CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.NodeEntryRawFileOffset
                == 0x012C9384
            && CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection
                .BoundaryRawFileOffsetExclusive == 0x012C93A1
            && CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection
                .RawFileOffsetToVirtualAddress(0x012C93A1) == 0x016C9FA1
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan missingEntry =
            CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.Project(
                new(EntryEDI: null, LateEntryEBP: 0x2000, EntryESI: 0x3000));
        check("Missing parent EDI stops at 9354 without a node handoff",
            missingEntry.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.MissingEntryEDIAt9354
            && missingEntry.ParentOutcomeValidated
            && !missingEntry.ParentHandoffValidated
            && !missingEntry.NodeHandoffReached
            && missingEntry.UnreachedEntryESICaptureProvided
            && missingEntry.NodeLifecycle is null
            && missingEntry.StopAtRawFileOffsetExclusive == 0x012C9354
            && missingEntry.LateEntryEBPProvidedButUnused
            && missingEntry.LateEntryEBPUnusedOnBypass
            && missingEntry.Writes.IsEmpty);

        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan missingEBP =
            CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.Project(
                new(EntryEDI: 0x100, LateEntryEBP: null, EntryESI: 0x3000));
        check("Missing late EBP stops at the 9374 frame store without node initialization",
            missingEBP.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome
                    .MissingLateEntryEBPAt9374
            && missingEBP.ParentOutcomeValidated
            && missingEBP.ParentLifecycle?.Outcome
                == CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome
                    .MissingLateEntryEBPAt9374
            && missingEBP.ParentLifecycle?.IndexLifecycle?.BSRReached == true
            && missingEBP.ParentLifecycle?.FrameStoreAttempted == true
            && missingEBP.ParentLifecycle?.FrameStoreCommitted == false
            && missingEBP.ParentLifecycle?.StopAtRawFileOffsetExclusive == 0x012C9374
            && !missingEBP.NodeHandoffReached
            && missingEBP.UnreachedEntryESICaptureProvided
            && missingEBP.NodeLifecycle is null
            && !missingEBP.FrameStoreCommitted
            && missingEBP.Writes.IsEmpty
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C9374);

        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan zero =
            CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.Project(
                new(EntryEDI: 0, LateEntryEBP: null, EntryESI: 0x3000));
        check("Zero parent XOR derives ECX zero and executes the ordered node stores",
            zero.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InitializedAt93A1
            && zero.ParentOutcomeValidated
            && zero.ParentHandoffValidated
            && zero.NodeHandoffReached
            && zero.ParentLifecycle?.Outcome
                == CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.ParentZeroAt9384
            && zero.ECXSource
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource.ParentZeroXor
            && zero.DerivedECX == 0
            && zero.NodeLifecycle?.Capture.EntryECX == 0
            && zero.FirstStoreValue == 0
            && zero.FrameStoreAddress is null
            && zero.Writes.Select(item => item.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C9384, 0x012C9393, 0x012C939A })
            && zero.Writes.All(item =>
                item.Stage
                    != CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.ParentFrameStore)
            && zero.DerivedEBX == 0x01EDAEF8
            && zero.DerivedEDX == 1
            && zero.StopAtRawFileOffsetExclusive == 0x012C93A1
            && zero.ExactCleanAdmission);

        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan above =
            CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.Project(
                new(EntryEDI: 0x01000000, LateEntryEBP: null, EntryESI: 0x3000));
        check("Above-ceiling parent MOV derives ECX 31 and reaches the same node window",
            above.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InitializedAt93A1
            && above.ParentLifecycle?.Outcome
                == CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome
                    .ParentAboveCeilingAt9384
            && above.ParentLifecycle?.ParentLifecycle?.ChildMoveResultECX == 31
            && above.ECXSource
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource
                    .ParentAboveCeilingMove31
            && above.DerivedECX == 31
            && above.NodeLifecycle?.Capture.EntryECX == 31
            && above.Writes.Select(item => item.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C9384, 0x012C9393, 0x012C939A })
            && above.StopAtRawFileOffsetExclusive == 0x012C93A1
            && above.ExactCleanAdmission);

        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan smallestIndex =
            CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.Project(
                new(EntryEDI: 0x100, LateEntryEBP: 0x2000, EntryESI: 0x3000));
        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan largestIndex =
            CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.Project(
                new(EntryEDI: 0x00FFFF00, LateEntryEBP: 0x2000, EntryESI: 0x3000));
        check("Index-child ECX comes from its final LEA and preserves frame-before-node write order",
            smallestIndex.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InitializedAt93A1
            && smallestIndex.ECXSource
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleECXSource.IndexChildFinalLEA
            && smallestIndex.ParentLifecycle?.IndexLifecycle?.FinalIndexECX == 0
            && smallestIndex.DerivedECX == 0
            && largestIndex.ParentLifecycle?.IndexLifecycle?.FinalIndexECX == 31
            && largestIndex.DerivedECX == 31
            && largestIndex.FirstStoreValue == 31
            && largestIndex.FrameStoreValue == 15
            && largestIndex.Writes.Select(item => item.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C9374, 0x012C9384, 0x012C9393, 0x012C939A })
            && largestIndex.Writes.Select(item => item.Stage).SequenceEqual(new[]
            {
                CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.ParentFrameStore,
                CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeFirstStore,
                CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeSecondStore,
                CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.NodeThirdStore
            })
            && largestIndex.ExactCleanAdmission);

        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan missingESI =
            CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.Project(
                new(EntryEDI: 0x100, LateEntryEBP: 0x2000, EntryESI: null));
        check("Missing late ESI stops at 9384 while retaining known first-store ECX",
            missingESI.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.MissingEntryESIAt9384
            && missingESI.ParentHandoffValidated
            && missingESI.NodeHandoffReached
            && missingESI.NodeLifecycle?.Outcome
                == CapacityFinalCalleeCommonIndexedNodeInitializationOutcome.MissingEntryESI
            && missingESI.DerivedECX == 0
            && missingESI.FirstStoreValue == 0
            && missingESI.FirstStoreAddress is null
            && !missingESI.FirstStoreReached
            && missingESI.Writes.Length == 1
            && missingESI.Writes[0].Stage
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleWriteStage.ParentFrameStore
            && missingESI.StopAtRawFileOffsetExclusive == 0x012C9384);

        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan unusedEBP =
            CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.Project(
                new(EntryEDI: 0, LateEntryEBP: 0x2000, EntryESI: 0x3000));
        check("A supplied EBP on a parent bypass is unused and contaminates clean admission",
            unusedEBP.ParentLifecycle?.LateEntryEBPProvidedButUnused == true
            && unusedEBP.ParentLifecycle?.LateEntryEBPUnusedOnBypass == true
            && unusedEBP.LateEntryEBPProvidedButUnused
            && unusedEBP.LateEntryEBPUnusedOnBypass
            && !unusedEBP.LateEntryEBPConsumed
            && !unusedEBP.ExactCleanAdmission
            && unusedEBP.Outcome
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleOutcome.InitializedAt93A1
            && unusedEBP.Writes.Length == 3);

        CapacityFinalCalleeCommonHighSizeNodeLifecyclePlan wrapped =
            CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.Project(
                new(EntryEDI: 0x00FFFF00, LateEntryEBP: 0x2000, EntryESI: 0xFFFFFFF0));
        check("Node-field addresses wrap as DWORDs, while MOV and LEA preserve parent flags",
            wrapped.NodeLifecycle?.FirstStoreAddress == 0x0C
            && wrapped.NodeLifecycle?.SecondStoreAddress == 0x04
            && wrapped.NodeLifecycle?.ThirdStoreAddress == 0
            && wrapped.Writes[^3].Address == 0x0C
            && wrapped.Writes[^2].Address == 0x04
            && wrapped.Writes[^1].Address == 0
            && wrapped.FlagsAtNodeEntry.HasValue
            && wrapped.FlagsAtStop == wrapped.FlagsAtNodeEntry
            && !wrapped.NodeFlagsModified
            && wrapped.NodeLifecycle?.MemoryReadPerformed == false
            && !wrapped.GlobalReadPerformed
            && !wrapped.InstructionBeyondBoundaryExecuted
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C93A1);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes, uint? BranchTargetRaw)[] {
            (0x012C9354, "8B C7", null),
            (0x012C9356, "C1 E8 08", null),
            (0x012C9359, "85 C0", null),
            (0x012C935B, "75 04", 0x012C9361),
            (0x012C935D, "33 C9", null),
            (0x012C935F, "EB 23", 0x012C9384),
            (0x012C9361, "3D FF FF 00 00", null),
            (0x012C9366, "76 07", 0x012C936F),
            (0x012C9368, "B9 1F 00 00 00", null),
            (0x012C936D, "EB 15", 0x012C9384),
            (0x012C936F, "0F BD C0", null),
            (0x012C9372, "8B D0", null),
            (0x012C9374, "89 45 EC", null),
            (0x012C9377, "8B C7", null),
            (0x012C9379, "8D 4A 07", null),
            (0x012C937C, "D3 E8", null),
            (0x012C937E, "83 E0 01", null),
            (0x012C9381, "8D 0C 50", null),
            (0x012C9384, "89 4E 1C", null),
            (0x012C9387, "8D 1C 8D F8 AE ED 01", null),
            (0x012C938E, "BA 01 00 00 00", null),
            (0x012C9393, "C7 46 14 00 00 00 00", null),
            (0x012C939A, "C7 46 10 00 00 00 00", null)
        };
        ImmutableArray<CapacityFinalCalleeCommonHighSizeNodeLifecycleInstructionEvidence>
            actual = CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.Instructions;
        if (actual.Length != expected.Length)
        {
            return false;
        }

        uint nextRawOffset =
            CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection.EntryRawFileOffset;
        foreach (var (instruction, expectedInstruction) in actual.Zip(expected))
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.RawFileOffset != expectedInstruction.Raw
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || normalized != Normalize(expectedInstruction.Bytes)
                || instruction.BranchTargetRawFileOffset
                    != expectedInstruction.BranchTargetRaw)
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
        }

        return nextRawOffset
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection
                    .BoundaryRawFileOffsetExclusive
            && actual[0].Stage
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleInstructionStage.Parent
            && actual[^1].Stage
                == CapacityFinalCalleeCommonHighSizeNodeLifecycleInstructionStage.Node
            && actual.Where(instruction =>
                    instruction.RawFileOffset
                        >= CapacityFinalCalleeCommonHighSizeNodeLifecycleProjection
                            .NodeEntryRawFileOffset)
                .All(instruction =>
                    instruction.Stage
                        == CapacityFinalCalleeCommonHighSizeNodeLifecycleInstructionStage.Node);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
