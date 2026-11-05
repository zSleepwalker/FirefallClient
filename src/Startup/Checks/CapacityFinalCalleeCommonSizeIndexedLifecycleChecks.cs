using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonSizeIndexedLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        Fixture[] fixtures =
        {
            new(
                0x00000000,
                CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome
                    .ChildAddressGateBelowThresholdToOpaque9304,
                0x00000000,
                0x00000000,
                false,
                0x012C92F6,
                0x012C92F6,
                new CapacityFinalCalleeCommonTailSizeGateCompareFlags(
                    true, false, false, false, true, false),
                true,
                0x00000000,
                0x00000000,
                0x01EDADF0,
                false,
                0x012C9304,
                new CapacityFinalCalleeCommonIndexedAddressGateFlags(
                    true, true, false, false, true, false),
                0x012C9304,
                0x016C9F04),
            new(
                0x0000000F,
                CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome
                    .ChildAddressGateBelowThresholdToOpaque9304,
                0x00000001,
                0x00000001,
                false,
                0x012C92F6,
                0x012C92F6,
                new CapacityFinalCalleeCommonTailSizeGateCompareFlags(
                    true, true, false, false, true, false),
                true,
                0x00000001,
                0x0000000F,
                0x01EDADF8,
                false,
                0x012C9304,
                new CapacityFinalCalleeCommonIndexedAddressGateFlags(
                    true, true, false, false, true, false),
                0x012C9304,
                0x016C9F04),
            new(
                0x00000010,
                CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome
                    .ChildAddressGateAtOrAboveThresholdToBounded930A,
                0x00000002,
                0x00000002,
                false,
                0x012C92F6,
                0x012C92F6,
                new CapacityFinalCalleeCommonTailSizeGateCompareFlags(
                    true, true, false, false, true, false),
                true,
                0x00000002,
                0x00000010,
                0x01EDAE00,
                true,
                0x012C930A,
                new CapacityFinalCalleeCommonIndexedAddressGateFlags(
                    false, true, false, true, false, false),
                0x012C930A,
                0x016C9F0A),
            new(
                0x000000FF,
                CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome
                    .ChildAddressGateAtOrAboveThresholdToBounded930A,
                0x0000001F,
                0x0000001F,
                false,
                0x012C92F6,
                0x012C92F6,
                new CapacityFinalCalleeCommonTailSizeGateCompareFlags(
                    true, true, false, false, true, false),
                true,
                0x0000001F,
                0x000000FF,
                0x01EDAEE8,
                true,
                0x012C930A,
                new CapacityFinalCalleeCommonIndexedAddressGateFlags(
                    false, false, false, false, false, false),
                0x012C930A,
                0x016C9F0A),
            new(
                0x00000100,
                CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome
                    .ParentSizeGateAtOrAboveThresholdToBounded9354,
                0x00000020,
                0x00000020,
                true,
                0x012C9354,
                0x012C9354,
                new CapacityFinalCalleeCommonTailSizeGateCompareFlags(
                    false, true, false, true, false, false),
                false,
                null,
                null,
                null,
                null,
                null,
                null,
                0x012C9354,
                0x016C9F54),
            new(
                0xFFFFFFFF,
                CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome
                    .ParentSizeGateAtOrAboveThresholdToBounded9354,
                0x1FFFFFFF,
                0x1FFFFFFF,
                true,
                0x012C9354,
                0x012C9354,
                new CapacityFinalCalleeCommonTailSizeGateCompareFlags(
                    false, false, false, false, false, false),
                false,
                null,
                null,
                null,
                null,
                null,
                null,
                0x012C9354,
                0x016C9F54),
            new(
                null,
                CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome.MissingEntryEDI,
                null,
                null,
                null,
                null,
                0x012C92EC,
                null,
                false,
                null,
                null,
                null,
                null,
                null,
                null,
                0x012C92EC,
                0x016C9EEC)
        };

        CapacityFinalCalleeCommonSizeIndexedLifecyclePlan[] plans =
            fixtures.Select(Project).ToArray();

        check("The two unsigned thresholds independently compare shifted EDI against 20h and original EDI against 10h",
            fixtures.Zip(plans).All(pair =>
                pair.First.EntryEDI is null
                    ? pair.Second.ParentSizeGate.CompareRight is null
                        && pair.Second.ChildAddressGate is null
                    : pair.Second.ParentSizeGate.CompareLeft
                        == pair.First.ExpectedParentCompareLeft
                        && pair.Second.ParentSizeGate.CompareRight == 0x20
                        && (pair.Second.ChildAddressGate is null
                            || pair.Second.ChildAddressGate.CompareLeft
                                == pair.First.ExpectedChildCompareLeft
                                && pair.Second.ChildAddressGate.CompareRight == 0x10)));

        check("Clean below-threshold parent admission stops at 92F6 before the child gate",
            plans.Take(4).All(plan =>
                plan.ParentSizeGate.ExactCleanAdmission
                && plan.ParentSizeGate.JnbTaken == false
                && plan.ParentSizeGate.JnbSelectedDestinationRawFileOffset == 0x012C92F6
                && plan.ParentSizeGate.StopAtRawFileOffsetExclusive == 0x012C92F6
                && plan.ChildGateReached
                && plan.ChildAddressGate?.Capture.EntryECX == plan.ParentSizeGate.ShiftResult
                && plan.ChildAddressGate?.Capture.EntryEDI == plan.ParentSizeGate.Capture.EntryEDI));

        check("Exact high parent admission branches to 9354 and stops without entering the child",
            plans.Skip(4).Take(2).All(plan =>
                plan.ParentSizeGate.ExactCleanAdmission
                && plan.ParentSizeGate.JnbTaken == true
                && plan.ParentSizeGate.JnbSelectedDestinationRawFileOffset == 0x012C9354
                && plan.ParentSizeGate.StopAtRawFileOffsetExclusive == 0x012C9354
                && plan.StopAtRawFileOffsetExclusive == 0x012C9354
                && plan.ChildAddressGate is null));

        check("The EDI fixtures match their outcomes, LEA-derived addresses, flags, and bounded stops",
            fixtures.Zip(plans).All(pair => Matches(pair.First, pair.Second)));

        check("LEA DWORD multiply/add wrap and raw-to-virtual stop mappings are preserved",
            CapacityFinalCalleeCommonIndexedAddressGateProjection.Project(new(
                EntryECX: uint.MaxValue,
                EntryEDI: 0x10)) is
            {
                LEAScaledIndex: 0xFFFFFFF8,
                DerivedEDX: 0x01EDADE8,
                DerivedEAX: 0x01EDADE8
            }
            && fixtures.Zip(plans).All(pair =>
                CapacityFinalCalleeCommonIndexedAddressGateProjection
                    .RawFileOffsetToVirtualAddress(
                        pair.Second.StopAtRawFileOffsetExclusive)
                    == pair.First.ExpectedStopVirtualAddress));

        check("Missing parent EDI leaves both compare flags absent and does not create a child",
            plans[^1].Outcome
                == CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome.MissingEntryEDI
            && plans[^1].ParentCompareFlags is null
            && plans[^1].ChildCompareFlags is null
            && plans[^1].LastCompareFlags is null
            && plans[^1].ChildAddressGate is null
            && plans[^1].StopAtRawFileOffsetExclusive == 0x012C92EC);

        check("The last flags come from the child CMP when reached and otherwise from the parent CMP",
            fixtures.Zip(plans).All(pair =>
                pair.Second.LastCompareFlags == ExpectedLastFlags(pair.First)));
    }

    private static CapacityFinalCalleeCommonSizeIndexedLifecyclePlan Project(Fixture fixture) =>
        CapacityFinalCalleeCommonSizeIndexedLifecycleProjection.Project(
            new CapacityFinalCalleeCommonSizeIndexedLifecycleCapture(fixture.EntryEDI));

    private static bool Matches(Fixture fixture,
        CapacityFinalCalleeCommonSizeIndexedLifecyclePlan plan)
    {
        CapacityFinalCalleeCommonIndexedAddressGatePlan? child = plan.ChildAddressGate;
        return plan.Outcome == fixture.ExpectedOutcome
            && plan.ParentSizeGate.ShiftResult == fixture.ExpectedParentShiftResult
            && plan.ParentSizeGate.CompareLeft == fixture.ExpectedParentCompareLeft
            && plan.ParentSizeGate.CompareRight ==
                (fixture.ExpectedParentJnbTaken.HasValue ? 0x20u : null)
            && plan.ParentSizeGate.CompareFlags == fixture.ExpectedParentCompareFlags
            && plan.ParentSizeGate.JnbTaken == fixture.ExpectedParentJnbTaken
            && plan.ParentSizeGate.JnbSelectedDestinationRawFileOffset
                == fixture.ExpectedParentDestination
            && plan.ParentSizeGate.StopAtRawFileOffsetExclusive
                == fixture.ExpectedParentStop
            && (child is not null) == fixture.ExpectedChildGateReached
            && child?.Capture.EntryECX == fixture.ExpectedChildECX
            && child?.LEAIndex == fixture.ExpectedChildECX
            && child?.CompareLeft == fixture.ExpectedChildCompareLeft
            && child?.CompareRight ==
                (fixture.ExpectedChildJnbTaken.HasValue ? 0x10u : null)
            && child?.CompareFlags == fixture.ExpectedChildCompareFlags
            && child?.JnbTaken == fixture.ExpectedChildJnbTaken
            && child?.JnbSelectedDestinationRawFileOffset == fixture.ExpectedChildDestination
            && plan.ChildEDIAtCompare == fixture.ExpectedChildCompareLeft
            && plan.DerivedAddress == fixture.ExpectedDerivedAddress
            && plan.ExactCleanOutcome == fixture.EntryEDI.HasValue
            && plan.StopAtRawFileOffsetExclusive == fixture.ExpectedStop
            && CapacityFinalCalleeCommonIndexedAddressGateProjection
                .RawFileOffsetToVirtualAddress(plan.StopAtRawFileOffsetExclusive)
                == fixture.ExpectedStopVirtualAddress
            && !plan.MemoryReadPerformed
            && !plan.OpaqueCallEntered
            && plan.Writes.IsEmpty;
    }

    private static CapacityFinalCalleeCommonSizeIndexedLifecycleCompareFlags ToLifecycleFlags(
        CapacityFinalCalleeCommonTailSizeGateCompareFlags flags) =>
        new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow);

    private static CapacityFinalCalleeCommonSizeIndexedLifecycleCompareFlags ToLifecycleFlags(
        CapacityFinalCalleeCommonIndexedAddressGateFlags flags) =>
        new(flags.Carry, flags.Parity, flags.Auxiliary, flags.Zero, flags.Sign, flags.Overflow);

    private static CapacityFinalCalleeCommonSizeIndexedLifecycleCompareFlags? ExpectedLastFlags(
        Fixture fixture)
    {
        if (fixture.ExpectedChildCompareFlags is
            CapacityFinalCalleeCommonIndexedAddressGateFlags childFlags)
        {
            return ToLifecycleFlags(childFlags);
        }

        if (fixture.ExpectedParentCompareFlags is
            CapacityFinalCalleeCommonTailSizeGateCompareFlags parentFlags)
        {
            return ToLifecycleFlags(parentFlags);
        }

        return null;
    }

    private readonly record struct Fixture(
        uint? EntryEDI,
        CapacityFinalCalleeCommonSizeIndexedLifecycleOutcome ExpectedOutcome,
        uint? ExpectedParentShiftResult,
        uint? ExpectedParentCompareLeft,
        bool? ExpectedParentJnbTaken,
        uint? ExpectedParentDestination,
        uint ExpectedParentStop,
        CapacityFinalCalleeCommonTailSizeGateCompareFlags? ExpectedParentCompareFlags,
        bool ExpectedChildGateReached,
        uint? ExpectedChildECX,
        uint? ExpectedChildCompareLeft,
        uint? ExpectedDerivedAddress,
        bool? ExpectedChildJnbTaken,
        uint? ExpectedChildDestination,
        CapacityFinalCalleeCommonIndexedAddressGateFlags? ExpectedChildCompareFlags,
        uint ExpectedStop,
        uint ExpectedStopVirtualAddress);
}
