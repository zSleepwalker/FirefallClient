using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonHighSizeCeilingLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The composition preserves both exact instruction windows and destinations",
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.EntryRawFileOffset
                == 0x012C9354
            && CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.HighSizeDestinationRawFileOffset
                == 0x012C9361
            && CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.ZeroSizeDestinationRawFileOffset
                == 0x012C9384
            && CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.BelowOrEqualDestinationRawFileOffset
                == 0x012C936F
            && CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.AboveDestinationRawFileOffset
                == 0x012C9384
            && CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.BoundaryRawFileOffsetExclusive
                == 0x012C936F
            && CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.EntryRawFileOffset)
                == 0x016C9F54
            && CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.BelowOrEqualDestinationRawFileOffset)
                == 0x016C9F6F
            && CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.AboveDestinationRawFileOffset)
                == 0x016C9F84
            && Normalize(CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.EntryWindowBytes)
                == "8BC7C1E80885C0750433C9EB23"
            && Normalize(CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.CeilingWindowBytes)
                == "3DFFFF00007607B91F000000EB15"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan missing =
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Project(new(EntryEDI: null));
        check("Missing parent EDI stops at 9354 with no child or effects",
            missing.Outcome
                == CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome.MissingEntryEDIAt9354
            && !missing.EntryEDICaptured
            && !missing.ParentShiftReached
            && !missing.ChildEntered
            && missing.EntryGate?.Outcome
                == CapacityFinalCalleeCommonHighSizeEntryGateOutcome.MissingEntryEDI
            && missing.CeilingGate is null
            && missing.Events.IsEmpty
            && missing.FlagsAtStop is null
            && missing.StopAtRawFileOffsetExclusive == 0x012C9354
            && NoMemoryEffects(missing));

        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan zero =
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Project(new(EntryEDI: 0));
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan byteOnly =
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Project(new(EntryEDI: 0xFF));
        check("Parent-zero path retains XOR ECX zero and jumps directly to 9384",
            new[] { zero, byteOnly }.All(plan =>
                plan.Outcome
                    == CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome.ParentZeroAt9384
                && plan.EntryEDICaptured
                && plan.ParentShiftReached
                && plan.ParentShiftResultEAX == 0
                && !plan.ChildEntered
                && plan.CeilingGate is null
                && plan.ParentJNZReached
                && plan.ParentJNZTaken == false
                && plan.ParentJNZSelectedDestinationRawFileOffset == 0x012C935D
                && plan.ParentXorReached
                && plan.ParentXorResultECX == 0
                && plan.ECXAtStopKnown
                && plan.ECXAtStop == 0
                && plan.FinalSelectedDestinationRawFileOffset == 0x012C9384
                && plan.FlagsAtStop is
                {
                    Carry: false,
                    Parity: true,
                    Auxiliary: null,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                }
                && plan.Events.Select(item => item.RawFileOffset).SequenceEqual(new uint[]
                {
                    0x012C9354, 0x012C9356, 0x012C9359, 0x012C935B, 0x012C935D, 0x012C935F
                })
                && plan.StopAtRawFileOffsetExclusive == 0x012C9384
                && plan.ExactCleanAdmission
                && NoMemoryEffects(plan))
            && byteOnly.EntryGate?.ShiftFlags is
            {
                Carry: true,
                Zero: true
            }
            && byteOnly.ParentTestFlags is
            {
                Carry: false,
                Zero: true
            });

        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan firstChildLow =
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Project(new(EntryEDI: 0x100));
        check("Parent shift result supplies child EAX and unsigned-below stops before BSR",
            firstChildLow.ParentShiftResultEAX == 1
            && firstChildLow.ChildEntered
            && firstChildLow.ChildEntryEAX == firstChildLow.ParentShiftResultEAX
            && firstChildLow.ChildCompareInputEAX == firstChildLow.ParentShiftResultEAX
            && firstChildLow.ChildCompareReached
            && firstChildLow.ChildCompareImmediate == 0xFFFF
            && firstChildLow.ChildCompareFlags is
            {
                Carry: true,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && firstChildLow.ParentTestFlags is
            {
                Carry: false,
                Parity: false,
                Zero: false,
                Overflow: false
            }
            && firstChildLow.FlagsAtStop == firstChildLow.ChildCompareFlags
            && firstChildLow.ChildJBEReached
            && firstChildLow.ChildJBETaken == true
            && firstChildLow.ChildJBESelectedDestinationRawFileOffset == 0x012C936F
            && !firstChildLow.ChildMoveReached
            && !firstChildLow.ChildJMPReached
            && !firstChildLow.ECXAtStopKnown
            && firstChildLow.ECXAtStop is null
            && firstChildLow.FinalSelectedDestinationRawFileOffset == 0x012C936F
            && firstChildLow.Outcome
                == CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
                    .ChildBelowOrEqualAt936FBeforeBSR
            && firstChildLow.StopAtRawFileOffsetExclusive == 0x012C936F
            && firstChildLow.Events.Select(item => item.RawFileOffset).SequenceEqual(new uint[]
            {
                0x012C9354, 0x012C9356, 0x012C9359, 0x012C935B, 0x012C9361, 0x012C9366
            })
            && firstChildLow.ExactCleanAdmission
            && NoMemoryEffects(firstChildLow));

        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan equalCeiling =
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Project(
                new(EntryEDI: 0x00FFFF00));
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan maxBelowBoundary =
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Project(
                new(EntryEDI: 0x00FFFFFF));
        check("The FFFF shifted boundary is unsigned-equal and stops at 936F",
            new[] { equalCeiling, maxBelowBoundary }.All(plan =>
                plan.ParentShiftResultEAX == 0xFFFF
                && plan.ChildEntryEAX == 0xFFFF
                && plan.ChildCompareInputEAX == 0xFFFF
                && plan.ChildCompareFlags is
                {
                    Carry: false,
                    Parity: true,
                    Auxiliary: false,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                }
                && plan.ChildJBETaken == true
                && plan.ChildJBESelectedDestinationRawFileOffset == 0x012C936F
                && !plan.ChildMoveReached
                && !plan.ECXAtStopKnown
                && plan.Outcome
                    == CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
                        .ChildBelowOrEqualAt936FBeforeBSR
                && plan.StopAtRawFileOffsetExclusive == 0x012C936F
                && NoMemoryEffects(plan)));

        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan firstAboveBoundary =
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Project(
                new(EntryEDI: 0x01000000));
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan maxDerived =
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Project(
                new(EntryEDI: 0xFFFFFFFF));
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan highBit =
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Project(
                new(EntryEDI: 0x80000000));
        check("Above-boundary derived values set ECX to 31 and jump to 9384",
            firstAboveBoundary.ParentShiftResultEAX == 0x10000
            && maxDerived.ParentShiftResultEAX == 0x00FFFFFF
            && highBit.ParentShiftResultEAX == 0x00800000
            && maxDerived.ChildEntryEAX != uint.MaxValue
            && new[] { firstAboveBoundary, maxDerived, highBit }.All(plan =>
                plan.ChildEntryEAX == plan.ParentShiftResultEAX
                && plan.ChildCompareInputEAX == plan.ParentShiftResultEAX
                && plan.ChildJBETaken == false
                && plan.ChildJBESelectedDestinationRawFileOffset == 0x012C9368
                && plan.ChildMoveReached
                && plan.ChildMoveResultECX == 31
                && plan.ECXAtStopKnown
                && plan.ECXAtStop == 31
                && plan.ChildJMPReached
                && plan.ChildJMPTaken == true
                && plan.ChildJMPSelectedDestinationRawFileOffset == 0x012C9384
                && plan.FinalSelectedDestinationRawFileOffset == 0x012C9384
                && plan.FlagsAtStop == plan.ChildCompareFlags
                && plan.Outcome
                    == CapacityFinalCalleeCommonHighSizeCeilingLifecycleOutcome
                        .ChildAboveCeilingAt9384BeforeStore
                && plan.StopAtRawFileOffsetExclusive == 0x012C9384
                && NoMemoryEffects(plan)));

        check("Ceiling CMP flags are the last-defined flags on each child branch",
            firstAboveBoundary.ChildCompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && maxDerived.ChildCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && highBit.ChildCompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && new[] { firstAboveBoundary, maxDerived, highBit }.All(plan =>
                plan.Events[^2].Flags == plan.ChildCompareFlags
                && plan.Events[^1].Flags == plan.ChildCompareFlags
                && plan.FlagsAtStop == plan.ChildCompareFlags));

        check("The aggregate trace retains parent TEST flags until child CMP overrides them",
            firstAboveBoundary.ParentTestFlags is
            {
                Carry: false,
                Parity: true,
                Zero: false,
                Overflow: false
            }
            && firstAboveBoundary.Events[3] is
            {
                Gate: CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Entry,
                Kind: CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.BranchNotZero,
                RawFileOffset: 0x012C935B
            } parentBranch
            && parentBranch.Flags == firstAboveBoundary.ParentTestFlags
            && firstAboveBoundary.Events[4] is
            {
                Gate: CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Ceiling,
                Kind: CapacityFinalCalleeCommonHighSizeCeilingLifecycleEventKind.Compare,
                RawFileOffset: 0x012C9361
            } childCompare
            && childCompare.Flags == firstAboveBoundary.ChildCompareFlags
            && firstAboveBoundary.FlagsAtStop == firstAboveBoundary.ChildCompareFlags
            && firstAboveBoundary.Events[^1].Flags == firstAboveBoundary.ChildCompareFlags);

        check("Every represented path is bounded and has no memory or target effects",
            new[]
            {
                missing, zero, byteOnly, firstChildLow, equalCeiling, maxBelowBoundary,
                firstAboveBoundary, maxDerived, highBit
            }.All(NoMemoryEffects));
    }

    private static bool NoMemoryEffects(
        CapacityFinalCalleeCommonHighSizeCeilingLifecyclePlan plan) =>
        !plan.FrameReadReached
        && !plan.GlobalReadReached
        && plan.Reads.IsEmpty
        && plan.Writes.IsEmpty
        && !plan.TargetEntered;

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate Gate,
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Entry,
                0x012C9354, "8B C7", null, null),
            (CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Entry,
                0x012C9356, "C1 E8 08", null, null),
            (CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Entry,
                0x012C9359, "85 C0", null, null),
            (CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Entry,
                0x012C935B, "75 04", 0x012C9361, 0x016C9F61),
            (CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Entry,
                0x012C935D, "33 C9", null, null),
            (CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Entry,
                0x012C935F, "EB 23", 0x012C9384, 0x016C9F84),
            (CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Ceiling,
                0x012C9361, "3D FF FF 00 00", null, null),
            (CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Ceiling,
                0x012C9366, "76 07", 0x012C936F, 0x016C9F6F),
            (CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Ceiling,
                0x012C9368, "B9 1F 00 00 00", null, null),
            (CapacityFinalCalleeCommonHighSizeCeilingLifecycleGate.Ceiling,
                0x012C936D, "EB 15", 0x012C9384, 0x016C9F84)
        };
        var actual = CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection.Instructions;
        return actual.Length == expected.Length
            && actual.Zip(expected).All(pair =>
                pair.First.Gate == pair.Second.Gate
                && pair.First.RawFileOffset == pair.Second.Raw
                && pair.First.FlatIdaAddress == pair.Second.Raw
                && pair.First.VirtualAddress
                    == CapacityFinalCalleeCommonHighSizeCeilingLifecycleProjection
                        .RawFileOffsetToVirtualAddress(pair.Second.Raw)
                && Normalize(pair.First.Bytes) == Normalize(pair.Second.Bytes)
                && pair.First.BranchTargetRawFileOffset == pair.Second.BranchTargetRaw
                && pair.First.BranchTargetVirtualAddress == pair.Second.BranchTargetVirtual);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
