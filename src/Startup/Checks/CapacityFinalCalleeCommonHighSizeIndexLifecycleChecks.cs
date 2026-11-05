using System.Collections.Immutable;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonHighSizeIndexLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan missingEntry =
            CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Project(
                new(EntryEDI: null, LateEntryEBP: 0x2000));
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan missingEBP =
            CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Project(
                new(EntryEDI: 0x00000100, LateEntryEBP: null));
        check("The composition retains the verified instruction windows and 9384 bound",
            CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.EntryRawFileOffset
                == 0x012C9354
            && CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.IndexEntryRawFileOffset
                == 0x012C936F
            && CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9384
            && CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.FollowingStoreRawFileOffset
                == 0x012C9384
            && CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.RawFileOffsetToVirtualAddress(
                0x012C9381) == 0x016C9F81
            && InstructionEvidenceMatches()
            && missingEntry.Outcome
                == CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.MissingEntryEDIAt9354
            && missingEntry.StopAtRawFileOffsetExclusive == 0x012C9354
            && missingEntry.Events.IsEmpty
            && missingEntry.IndexLifecycle is null
            && !missingEntry.ChildEntered
            && !missingEntry.LateEntryEBPConsumed
            && missingEntry.LateEntryEBPProvidedButUnused
            && missingEntry.LateEntryEBPUnusedOnBypass
            && missingEntry.Writes.IsEmpty
            && missingEBP.Outcome
                == CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.MissingLateEntryEBPAt9374
            && missingEBP.ChildEntered
            && missingEBP.DerivedChildEntryEAX == 1
            && missingEBP.IndexLifecycle?.Capture.EntryEAX == 1
            && missingEBP.IndexLifecycle?.Capture.LateEntryEDI == 0x00000100
            && missingEBP.FrameStoreAttempted
            && missingEBP.FrameStoreValue == 0
            && missingEBP.FrameStoreAddress is null
            && !missingEBP.FrameStoreCommitted
            && missingEBP.Writes.IsEmpty
            && missingEBP.OriginalEntryEDIFor9377 == 0x00000100
            && !missingEBP.OriginalEntryEDIReusedFor9377
            && missingEBP.FlagsAtStop
                == ConvertIndexFlags(missingEBP.IndexLifecycle?.BSRFlags)
            && !missingEBP.ExactCleanAdmission
            && missingEBP.StopAtRawFileOffsetExclusive == 0x012C9374);

        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan zero =
            CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Project(
                new(EntryEDI: 0, LateEntryEBP: null));
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan byteOnly =
            CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Project(
                new(EntryEDI: 0xFF, LateEntryEBP: 0x2000));
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan zeroWithUnusedEBP =
            CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Project(
                new(EntryEDI: 0, LateEntryEBP: 0x2000));
        check("Missing input and parent-zero remain distinct; zero bypasses BSR and leaves EBP unused",
            new[] { zero, byteOnly, zeroWithUnusedEBP }.All(plan =>
                plan.Outcome
                    == CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome.ParentZeroAt9384
                && plan.ParentLifecycle?.ParentShiftResultEAX == 0
                && plan.ParentLifecycle.ParentJNZReached
                && plan.ParentLifecycle.ParentJNZTaken == false
                && plan.ParentLifecycle.FinalSelectedDestinationRawFileOffset == 0x012C9384
                && plan.IndexLifecycle is null
                && !plan.ChildEntered
                && !plan.LateEntryEBPConsumed
                && !plan.FrameStoreAttempted
                && !plan.FrameStoreCommitted
                && plan.Writes.IsEmpty
                && plan.StopAtRawFileOffsetExclusive == 0x012C9384
                && plan.FlagsAtStop == ConvertParentFlags(plan.ParentLifecycle.FlagsAtStop))
            && !zero.LateEntryEBPProvidedButUnused
            && !zero.LateEntryEBPUnusedOnBypass
            && new[] { byteOnly, zeroWithUnusedEBP }.All(plan =>
                plan.LateEntryEBPProvidedButUnused
                && plan.LateEntryEBPUnusedOnBypass
                && !plan.ExactCleanAdmission)
            && zero.Events.Select(item => item.RawFileOffset).SequenceEqual(new uint[]
            {
                0x012C9354, 0x012C9356, 0x012C9359, 0x012C935B, 0x012C935D, 0x012C935F
            })
            && zero.Events.All(item =>
                item.Stage != CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index)
            && zero.ExactCleanAdmission
            && !byteOnly.ExactCleanAdmission
            && !zeroWithUnusedEBP.ExactCleanAdmission);

        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan firstAboveCeiling =
            CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Project(
                new(EntryEDI: 0x01000000, LateEntryEBP: null));
        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan maxAboveCeiling =
            CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Project(
                new(EntryEDI: uint.MaxValue, LateEntryEBP: 0x3000));
        check("Shifted values above FFFF jump to 9384 without entering the index child",
            firstAboveCeiling.ParentLifecycle?.ParentShiftResultEAX == 0x10000
            && maxAboveCeiling.ParentLifecycle?.ParentShiftResultEAX == 0x00FFFFFF
            && new[] { firstAboveCeiling, maxAboveCeiling }.All(plan =>
                plan.Outcome
                    == CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome
                        .ParentAboveCeilingAt9384
                && plan.ParentLifecycle?.ChildJBETaken == false
                && plan.ParentLifecycle?.ChildJMPReached == true
                && plan.ParentLifecycle?.ChildJMPSelectedDestinationRawFileOffset == 0x012C9384
                && plan.SelectedDestinationRawFileOffset == 0x012C9384
                && plan.IndexLifecycle is null
                && !plan.ChildEntered
                && !plan.FrameStoreAttempted
                && plan.Writes.IsEmpty
                && plan.StopAtRawFileOffsetExclusive == 0x012C9384)
            && !firstAboveCeiling.LateEntryEBPUnusedOnBypass
            && !firstAboveCeiling.LateEntryEBPProvidedButUnused
            && firstAboveCeiling.ExactCleanAdmission
            && maxAboveCeiling.LateEntryEBPUnusedOnBypass
            && maxAboveCeiling.LateEntryEBPProvidedButUnused
            && !maxAboveCeiling.ExactCleanAdmission);

        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan maxIndex =
            CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Project(
                new(EntryEDI: 0x00FFFF00, LateEntryEBP: 0x00002000));
        check("The exact FFFF handoff derives child EAX and reuses original EDI for index 31",
            maxIndex.Outcome
                == CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome
                    .IndexedResultAt9384BeforeFollowingStore
            && maxIndex.ParentLifecycle?.ParentShiftResultEAX == 0xFFFF
            && maxIndex.ParentLifecycle?.ChildEntryEAX == 0xFFFF
            && maxIndex.DerivedChildEntryEAX == 0xFFFF
            && maxIndex.IndexLifecycle?.Capture.EntryEAX == maxIndex.DerivedChildEntryEAX
            && maxIndex.IndexLifecycle?.Capture.LateEntryEDI == maxIndex.Capture.EntryEDI
            && maxIndex.IndexLifecycle?.StopAtRawFileOffsetExclusive == 0x012C9384
            && maxIndex.IndexLifecycle?.BSRReached == true
            && maxIndex.IndexLifecycle?.BSRDestinationDefined == true
            && maxIndex.IndexLifecycle?.BSRInputEAX == 0xFFFF
            && maxIndex.IndexLifecycle?.BSRResultEAX is <= 15
            && maxIndex.IndexLifecycle?.StoreCommitted == true
            && maxIndex.IndexLifecycle?.MoveEDIToEAXReached == true
            && maxIndex.IndexLifecycle?.ShiftReached == true
            && maxIndex.IndexLifecycle?.AndReached == true
            && maxIndex.IndexLifecycle?.FinalLeaReached == true
            && maxIndex.IndexLifecycle?.FinalIndexECX is <= 31
            && maxIndex.OriginalEntryEDIFor9377 == maxIndex.Capture.EntryEDI
            && maxIndex.OriginalEntryEDIReusedFor9377
            && maxIndex.IndexLifecycle?.BSRResultEAX == 15
            && maxIndex.IndexLifecycle?.ShiftInputEAX == 0x00FFFF00
            && maxIndex.IndexLifecycle?.ShiftCount == 22
            && maxIndex.IndexLifecycle?.ShiftResultEAX == 3
            && maxIndex.IndexLifecycle?.AndResultEAX == 1
            && maxIndex.IndexLifecycle?.FinalIndexECX == 31
            && maxIndex.FrameStoreAttempted
            && maxIndex.FrameStoreCommitted
            && maxIndex.LateEntryEBPConsumed
            && !maxIndex.LateEntryEBPProvidedButUnused
            && maxIndex.FrameStoreAddress == 0x00001FEC
            && maxIndex.FrameStoreValue == 15
            && maxIndex.Writes.SequenceEqual(new uint[] { 0x00001FEC })
            && maxIndex.CommittedFrameStores.Length == 1
            && maxIndex.CommittedFrameStores[0].Address == 0x00001FEC
            && maxIndex.CommittedFrameStores[0].Result is null
            && maxIndex.StopAtRawFileOffsetExclusive == 0x012C9384
            && maxIndex.SelectedDestinationRawFileOffset == 0x012C936F
            && maxIndex.ExactCleanAdmission);

        check("Aggregate events preserve branch-to-child order, committed store, and flag handoff",
            maxIndex.Events.Select(item => item.RawFileOffset).SequenceEqual(new uint[]
            {
                0x012C9354, 0x012C9356, 0x012C9359, 0x012C935B, 0x012C9361, 0x012C9366,
                0x012C936F, 0x012C9372, 0x012C9374, 0x012C9377, 0x012C9379, 0x012C937C,
                0x012C937E, 0x012C9381
            })
            && maxIndex.Events[5] is
            {
                Stage: CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Ceiling,
                Kind: CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.BranchBelowOrEqual,
                BranchTaken: true,
                SelectedDestinationRawFileOffset: 0x012C936F
            } ceilingBranch
            && maxIndex.Events[6] is
            {
                Stage: CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index,
                Kind: CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.BitScanReverse,
                RawFileOffset: 0x012C936F
            } bsr
            && ceilingBranch.Flags == ConvertParentFlags(maxIndex.ParentLifecycle?.ChildCompareFlags)
            && bsr.Flags == ConvertIndexFlags(maxIndex.IndexLifecycle?.BSRFlags)
            && bsr.Flags != ceilingBranch.Flags
            && maxIndex.FlagsAtStop == ConvertIndexFlags(maxIndex.IndexLifecycle?.AndFlags)
            && maxIndex.Events[^1].Kind
                == CapacityFinalCalleeCommonHighSizeIndexLifecycleEventKind.LeaIndexedResult
            && maxIndex.Events[^1].Flags == maxIndex.FlagsAtStop
            && maxIndex.IndexLifecycle?.Writes.SequenceEqual(maxIndex.Writes) == true
            && maxIndex.Reads.IsEmpty
            && !maxIndex.TargetEntered
            && maxIndex.StopAtRawFileOffsetExclusive
                == CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection
                    .FollowingStoreRawFileOffset);

        CapacityFinalCalleeCommonHighSizeIndexLifecyclePlan smallestChild =
            CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Project(
                new(EntryEDI: 0x00000100, LateEntryEBP: 0x1000));
        check("The nonzero lower-bound handoff reaches BSR with index zero",
            smallestChild.ParentLifecycle?.ParentShiftResultEAX == 1
            && smallestChild.DerivedChildEntryEAX == 1
            && smallestChild.IndexLifecycle?.BSRResultEAX == 0
            && smallestChild.IndexLifecycle?.FinalIndexECX == 0
            && smallestChild.Outcome
                == CapacityFinalCalleeCommonHighSizeIndexLifecycleOutcome
                    .IndexedResultAt9384BeforeFollowingStore
            && smallestChild.StopAtRawFileOffsetExclusive == 0x012C9384
            && smallestChild.FrameStoreCommitted
            && smallestChild.ExactCleanAdmission);

        check("Every represented path stays within the bounded projection",
            new[]
            {
                missingEntry, missingEBP, zero, byteOnly, firstAboveCeiling,
                maxAboveCeiling, smallestChild, maxIndex
            }.All(plan =>
                plan.StopAtRawFileOffsetExclusive
                    <= CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection
                        .BoundaryRawFileOffsetExclusive
                && plan.Writes.SequenceEqual(
                    plan.CommittedFrameStores
                        .Where(item => item.Address.HasValue)
                        .Select(item => item.Address!.Value))));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            CapacityFinalCalleeCommonHighSizeIndexLifecycleStage Stage,
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw)[] {
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Entry,
                0x012C9354, "8B C7", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Entry,
                0x012C9356, "C1 E8 08", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Entry,
                0x012C9359, "85 C0", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Entry,
                0x012C935B, "75 04", 0x012C9361),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Entry,
                0x012C935D, "33 C9", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Entry,
                0x012C935F, "EB 23", 0x012C9384),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Ceiling,
                0x012C9361, "3D FF FF 00 00", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Ceiling,
                0x012C9366, "76 07", 0x012C936F),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Ceiling,
                0x012C9368, "B9 1F 00 00 00", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Ceiling,
                0x012C936D, "EB 15", 0x012C9384),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index,
                0x012C936F, "0F BD C0", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index,
                0x012C9372, "8B D0", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index,
                0x012C9374, "89 45 EC", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index,
                0x012C9377, "8B C7", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index,
                0x012C9379, "8D 4A 07", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index,
                0x012C937C, "D3 E8", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index,
                0x012C937E, "83 E0 01", null),
            (CapacityFinalCalleeCommonHighSizeIndexLifecycleStage.Index,
                0x012C9381, "8D 0C 50", null)
        };
        ImmutableArray<CapacityFinalCalleeCommonHighSizeIndexLifecycleInstructionEvidence>
            actual = CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection.Instructions;
        return actual.Length == expected.Length
            && actual.Zip(expected).All(pair =>
                pair.First.Stage == pair.Second.Stage
                && pair.First.RawFileOffset == pair.Second.Raw
                && pair.First.FlatIdaAddress == pair.Second.Raw
                && pair.First.VirtualAddress
                    == CapacityFinalCalleeCommonHighSizeIndexLifecycleProjection
                        .RawFileOffsetToVirtualAddress(pair.Second.Raw)
                && Normalize(pair.First.Bytes) == Normalize(pair.Second.Bytes)
                && pair.First.BranchTargetRawFileOffset == pair.Second.BranchTargetRaw);
    }

    private static CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? ConvertParentFlags(
        CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags? flags) =>
        flags is CapacityFinalCalleeCommonHighSizeCeilingLifecycleFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;

    private static CapacityFinalCalleeCommonHighSizeIndexLifecycleFlags? ConvertIndexFlags(
        CapacityFinalCalleeCommonHighSizeBitScanFlags? flags) =>
        flags is CapacityFinalCalleeCommonHighSizeBitScanFlags value
            ? new(value.Carry, value.Parity, value.Auxiliary, value.Zero, value.Sign, value.Overflow)
            : null;

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
