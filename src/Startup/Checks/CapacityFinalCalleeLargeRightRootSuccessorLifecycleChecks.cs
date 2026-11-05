using System;
using System.Buffers.Binary;
using System.Linq;
using Firefall.Reconstruction.Startup;
using Projection =
    Firefall.Reconstruction.Startup
        .CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection;

internal static class CapacityFinalCalleeLargeRightRootSuccessorLifecycleChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The equal root-store and unequal size-gate windows match their bounded native bytes",
            InstructionEvidenceMatches());

        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan equal =
            Projection.Project(EqualRoute(freshRootGlobalDWORD: 0x80));
        check("A clean equal root handoff stores derived parent EDI once and jumps to 9441",
            equal.ParentLifecycle.Outcome
                == CapacityFinalCalleeLargeRightTagRootLifecycleOutcome
                    .EqualToFallthrough92D0
            && equal.ExactCleanParentAdmission
            && equal.EqualRouteSelected
            && !equal.UnequalRouteSelected
            && equal.ChildHelperEntered
            && equal.EqualRootStoreEntered
            && !equal.UnequalSizeGateEntered
            && equal.DerivedEDI == equal.Capture.Parent.Parent.LateEntryEDI
            && equal.DerivedEDI == 0x20
            && equal.EqualRootStore?.Capture.EntryEDI == equal.DerivedEDI
            && equal.EqualRootStoreCommitted
            && equal.EqualRootStoreAddress == 0x01EDADD0
            && equal.EqualRootStoreValue == equal.DerivedEDI
            && equal.EqualRootStoreJumpReached
            && equal.EqualRootStoreJumpTargetRawFileOffset == 0x012C9441
            && equal.EqualRootStore?.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeCommonTailRootStoreWrite(
                    0x012C92D0,
                    0x01EDADD0,
                    0x20)
            }) == true
            && equal.OrderedWrites.Length == equal.ParentLifecycle.OrderedWrites.Length + 1
            && equal.OrderedWrites[^1]
                == new CapacityFinalCalleeLargeRightRootSuccessorLifecycleWrite(
                    CapacityFinalCalleeLargeRightRootSuccessorLifecycleWriteKind
                        .EqualRootGlobalStore,
                    0x012C92D0,
                    0x01EDADD0,
                    0x20)
            && equal.ParentRootCompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.EqualRootFlagsPreservedThroughStoreAndJump
            && equal.ExactCleanCompletion
            && equal.StopAtRawFileOffsetExclusive == 0x012C9441
            && !equal.EqualDestinationEntered
            && !equal.UnequalFallthroughEntered
            && !equal.UnequalBranchTargetEntered);

        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan unequalZero =
            Projection.Project(UnequalRoute(0));
        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan unequalTen =
            Projection.Project(UnequalRoute(0x10));
        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan unequalHundred =
            Projection.Project(UnequalRoute(0x100));
        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan unequalMax =
            Projection.Project(UnequalRoute(uint.MaxValue));
        check("Unequal root routes use derived EDI, fixed SHR-by-three, and unsigned 20h classification",
            MatchesSizeGate(unequalZero, 0, 0, false, 0x012C92F6)
            && MatchesSizeGate(unequalTen, 0x10, 2, false, 0x012C92F6)
            && MatchesSizeGate(unequalHundred, 0x100, 0x20, true, 0x012C9354)
            && MatchesSizeGate(unequalMax, uint.MaxValue, 0x1FFFFFFF, true, 0x012C9354)
            && new[] { unequalZero, unequalTen, unequalHundred, unequalMax }.All(plan =>
                plan.ParentRootFlagsPreservedUntilSizeShift
                && plan.ParentRootCompareFlags is
                {
                    Carry: true,
                    Parity: true,
                    Auxiliary: true,
                    Zero: false,
                    Sign: true,
                    Overflow: false
                }
                && plan.UnequalSizeGate?.CompareLeft == plan.SizeGateCount
                && plan.UnequalSizeGate?.CompareRight == 0x20
                && plan.UnequalSizeGate?.CompareFlags.HasValue == true
                && plan.OrderedWrites.Length == plan.ParentLifecycle.OrderedWrites.Length
                && plan.StopAtRawFileOffsetExclusive
                    == plan.UnequalSizeGate?.JnbSelectedDestinationRawFileOffset
                && !plan.EqualRootStoreEntered
                && !plan.EqualDestinationEntered
                && !plan.UnequalFallthroughEntered
                && !plan.UnequalBranchTargetEntered
                && !plan.OpaqueCallEntered));

        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan aliasWrap =
            Projection.Project(AliasedUnequalRoute(uint.MaxValue));
        check("The later root read remains independent when earlier parent stores alias it",
            aliasWrap.ExactCleanParentAdmission
            && aliasWrap.UnequalRouteSelected
            && aliasWrap.ParentLifecycle.Reads.Select(read =>
                    (read.RawFileOffset, read.Address, read.ValueCaptured, read.Value))
                .SequenceEqual(new[]
                {
                    (0x012C92A4u, (uint?)0x214, true, (uint?)2),
                    (0x012C92ABu, (uint?)0x01EDADD8, true, (uint?)2),
                    (0x012C92C8u, (uint?)0x01EDADDC, true, (uint?)2)
                })
            && aliasWrap.ParentLifecycle.OrderedWrites.Any(write =>
                write.Address == 0x01EDADDC && write.Value == 2)
            && aliasWrap.ParentLifecycle.RootGlobalReadAddress == 0x01EDADDC
            && aliasWrap.ParentLifecycle.FreshRootGlobalDWORD == 2
            && aliasWrap.DerivedEDI == uint.MaxValue
            && aliasWrap.SizeGateCount == 0x1FFFFFFF
            && aliasWrap.UnequalSizeGate?.JnbTaken == true
            && aliasWrap.StopAtRawFileOffsetExclusive == 0x012C9354
            && CapacityFinalCalleeCommonTailSizeGateProjection.ShiftRight(
                0x80000000,
                35) == 0x10000000
            && aliasWrap.ExactCleanCompletion);

        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan missingParent =
            Projection.Project(new(
                Parent: new(
                    Parent: new(
                        EntryEDX: null,
                        FreshEDXPlus14: 0,
                        FreshGlobalValue: null,
                        LateEntryEAX: null,
                        LateEntryEDI: 0x20,
                        LateEntryESI: 0x80),
                    FreshRootGlobalDWORD: 0x1234)));
        check("A missing parent prerequisite cannot enter either successor and leaves its supplied root value unused",
            missingParent.Outcome
                == CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                    .ParentStoppedWithUnusedFreshRootGlobalDWORD
            && missingParent.ParentLifecycle.Reads.IsEmpty
            && !missingParent.ParentLifecycle.RootGateEntered
            && missingParent.ParentLifecycle.UnusedFreshRootGlobalDWORDCaptureProvided
            && !missingParent.ChildHelperEntered
            && missingParent.OrderedWrites.IsEmpty
            && !missingParent.ExactCleanParentAdmission
            && !missingParent.ExactCleanCompletion
            && missingParent.StopAtRawFileOffsetExclusive == 0x012C92A4);

        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan missingRootGlobal =
            Projection.Project(EqualRoute(freshRootGlobalDWORD: null));
        check("A missing root-gate value retains the attempted read but blocks branch successors",
            missingRootGlobal.Outcome
                == CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome.ParentStopped
            && missingRootGlobal.ParentLifecycle.ExactCleanParentHandoff
            && missingRootGlobal.ParentLifecycle.RootGlobalReadAttempted
            && !missingRootGlobal.ParentLifecycle.RootGlobalCaptured
            && missingRootGlobal.ParentLifecycle.RootCompareReached == false
            && missingRootGlobal.ParentLifecycle.RootJnzReached == false
            && missingRootGlobal.Reads[^1] is
            {
                RawFileOffset: 0x012C92C8,
                Address: 0x01EDADDC,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && missingRootGlobal.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && !missingRootGlobal.ChildHelperEntered
            && !missingRootGlobal.ExactCleanParentAdmission
            && !missingRootGlobal.ExactCleanCompletion
            && missingRootGlobal.StopAtRawFileOffsetExclusive == 0x012C92C8);

        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan dirtyParent =
            Projection.Project(new(
                Parent: new(
                    Parent: new(
                        EntryEDX: 0x200,
                        FreshEDXPlus14: 0,
                        FreshGlobalValue: 0x10,
                        LateEntryEAX: null,
                        LateEntryEDI: 0x20,
                        LateEntryESI: 0x80),
                    FreshRootGlobalDWORD: 0x80)));
        check("A dirty parent route preserves its earlier state without admitting the root child",
            dirtyParent.Outcome
                == CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                    .ParentStoppedWithUnusedFreshRootGlobalDWORD
            && dirtyParent.ParentLifecycle.ParentLifecycle.ParentOutcome
                == CapacityFinalCalleeLargeRightFieldThresholdLifecycleOutcome
                    .ZeroTo92BDWithUnusedFreshGlobalValue
            && dirtyParent.Reads.Select(read => read.RawFileOffset)
                .SequenceEqual(new[] { 0x012C92A4u })
            && dirtyParent.OrderedWrites.SequenceEqual(
                dirtyParent.ParentLifecycle.OrderedWrites.Select(write =>
                    new CapacityFinalCalleeLargeRightRootSuccessorLifecycleWrite(
                        CapacityFinalCalleeLargeRightRootSuccessorLifecycleWriteKind
                            .ParentWrite,
                        write.RawFileOffset,
                        write.Address,
                        write.Value)))
            && dirtyParent.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && !dirtyParent.ChildHelperEntered
            && dirtyParent.ParentLifecycle.UnusedFreshRootGlobalDWORDCaptureProvided
            && !dirtyParent.ExactCleanParentAdmission
            && !dirtyParent.ExactCleanCompletion
            && dirtyParent.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan missingParentEDI =
            Projection.Project(EqualRoute(
                freshRootGlobalDWORD: 0x80,
                lateEntryEDI: null));
        check("A missing upstream EDI is not recaptured or fabricated for either child helper",
            !missingParentEDI.ChildHelperEntered
            && missingParentEDI.DerivedEDI is null
            && !missingParentEDI.EqualRootStoreEntered
            && !missingParentEDI.UnequalSizeGateEntered
            && !missingParentEDI.ExactCleanCompletion
            && missingParentEDI.StopAtRawFileOffsetExclusive
                < Projection.EqualRootStoreEntryRawFileOffset);

        check("Only the selected bounded successor helper runs and no destination or callee is entered",
            new[] { equal }.Concat(
                new[] { unequalZero, unequalTen, unequalHundred, unequalMax, aliasWrap })
                .All(plan => plan.ExactCleanCompletion
                    && plan.ChildHelperEntered
                    && (plan.EqualRootStoreEntered ^ plan.UnequalSizeGateEntered)
                    && !plan.EqualDestinationEntered
                    && !plan.UnequalFallthroughEntered
                    && !plan.UnequalBranchTargetEntered
                    && !plan.OpaqueCallEntered)
            && equal.StopAtRawFileOffsetExclusive == 0x012C9441
            && unequalZero.StopAtRawFileOffsetExclusive == 0x012C92F6
            && unequalHundred.StopAtRawFileOffsetExclusive == 0x012C9354
            && !missingParent.ChildHelperEntered
            && !missingRootGlobal.ChildHelperEntered
            && !dirtyParent.ChildHelperEntered);
    }

    private static bool MatchesSizeGate(
        CapacityFinalCalleeLargeRightRootSuccessorLifecyclePlan plan,
        uint edi,
        uint count,
        bool branchTaken,
        uint destination)
    {
        CapacityFinalCalleeCommonTailSizeGatePlan? gate = plan.UnequalSizeGate;
        return plan.ExactCleanParentAdmission
            && plan.UnequalRouteSelected
            && plan.Outcome == (branchTaken
                ? CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                    .UnequalAtOrAboveUnsignedThresholdTo9354
                : CapacityFinalCalleeLargeRightRootSuccessorLifecycleOutcome
                    .UnequalBelowUnsignedThresholdTo92F6)
            && plan.DerivedEDI == edi
            && gate is
            {
                EntryEDICaptured: true,
                MoveReached: true,
                ShiftReached: true,
                ShiftInput: { } input,
                ShiftCount: 3,
                ShiftResult: { } shifted,
                CompareReached: true,
                CompareLeft: { } left,
                CompareRight: 0x20,
                CompareResult: { } compareResult,
                CompareFlags: { } compareFlags,
                JnbReached: true,
                JnbTaken: { } taken,
                JnbSelectedDestinationRawFileOffset: { } selected,
                ExactCleanAdmission: true,
                TargetEntered: false
            }
            && input == edi
            && shifted == count
            && left == count
            && compareResult == unchecked(count - 0x20)
            && compareFlags == ExpectedCompareFlags(count, compareResult)
            && taken == branchTaken
            && selected == destination
            && gate.StopAtRawFileOffsetExclusive == destination
            && plan.StopAtRawFileOffsetExclusive == destination
            && plan.ParentRootFlagsPreservedUntilSizeShift
            && plan.FlagsAtStop == new CapacityFinalCalleeLargeRightRootSuccessorLifecycleFlags(
                compareFlags.Carry,
                compareFlags.Parity,
                compareFlags.Auxiliary,
                compareFlags.Zero,
                compareFlags.Sign,
                compareFlags.Overflow)
            && gate.Writes.IsEmpty;
    }

    private static CapacityFinalCalleeCommonTailSizeGateCompareFlags ExpectedCompareFlags(
        uint left,
        uint result) =>
        new(
            Carry: left < 0x20,
            Parity: (System.Numerics.BitOperations.PopCount((uint)(byte)result) & 1) == 0,
            Auxiliary: ((left ^ 0x20 ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ 0x20) & (left ^ result) & 0x80000000) != 0);

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture EqualRoute(
        uint? freshRootGlobalDWORD,
        uint? lateEntryEDI = 0x20) =>
        new(new(
            Parent: new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 0,
                FreshGlobalValue: null,
                LateEntryEAX: null,
                LateEntryEDI: lateEntryEDI,
                LateEntryESI: 0x80),
            FreshRootGlobalDWORD: freshRootGlobalDWORD));

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture UnequalRoute(
        uint lateEntryEDI) =>
        new(new(
            Parent: new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 2,
                FreshGlobalValue: 2,
                LateEntryEAX: 0x200,
                LateEntryEDI: lateEntryEDI,
                LateEntryESI: 0x100),
            FreshRootGlobalDWORD: 0x101));

    private static CapacityFinalCalleeLargeRightRootSuccessorLifecycleCapture
        AliasedUnequalRoute(uint lateEntryEDI) =>
        new(new(
            Parent: new(
                EntryEDX: 0x200,
                FreshEDXPlus14: 2,
                FreshGlobalValue: 2,
                LateEntryEAX: 0x01EDADC8,
                LateEntryEDI: lateEntryEDI,
                LateEntryESI: 0x01EDADD8),
            FreshRootGlobalDWORD: 2));

    private static bool InstructionEvidenceMatches()
    {
        var expectedStore = new (uint Raw, string Bytes, uint? Target)[]
        {
            (0x012C92D0, "89 3D D0 AD ED 01", null),
            (0x012C92D6, "E9 66 01 00 00", 0x012C9441)
        };
        var expectedSize = new (uint Raw, string Bytes, uint? Target)[]
        {
            (0x012C92EC, "8B CF", null),
            (0x012C92EE, "C1 E9 03", null),
            (0x012C92F1, "83 F9 20", null),
            (0x012C92F4, "73 5E", 0x012C9354)
        };

        foreach (CapacityFinalCalleeLargeRightRootSuccessorLifecycleInstructionEvidence instruction
            in Projection.Instructions)
        {
            byte[] instructionBytes = Convert.FromHexString(Normalize(instruction.Bytes));
            if (instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeRightTagRootLifecycleProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeRightTagRootLifecycleProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            if (instruction.BranchTargetRawFileOffset is uint branchTarget)
            {
                int displacement = instructionBytes.Length == 2
                    ? unchecked((sbyte)instructionBytes[1])
                    : BinaryPrimitives.ReadInt32LittleEndian(
                        instructionBytes.AsSpan(instructionBytes.Length - 4, 4));
                if ((long)instruction.RawFileOffset + instructionBytes.Length + displacement
                    != branchTarget)
                {
                    return false;
                }
            }
        }

        return CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection
                .EqualRootStoreEntryRawFileOffset == 0x012C92D0
            && CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection
                .EqualRootStorePEVirtualAddress == 0x01EDADD0
            && CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection
                .UnequalSizeGateEntryRawFileOffset == 0x012C92EC
            && CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection
                .EqualStopRawFileOffset == 0x012C9441
            && CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection
                .UnequalBelowStopRawFileOffset == 0x012C92F6
            && CapacityFinalCalleeLargeRightRootSuccessorLifecycleProjection
                .UnequalAtOrAboveStopRawFileOffset == 0x012C9354
            && MatchesEvidence(expectedStore)
            && MatchesEvidence(expectedSize)
            && !Projection.Instructions.Any(instruction =>
                instruction.RawFileOffset is 0x012C92F6 or 0x012C9304 or 0x012C9441);
    }

    private static bool MatchesEvidence((uint Raw, string Bytes, uint? Target)[] expected)
    {
        foreach ((uint raw, string bytes, uint? target) in expected)
        {
            CapacityFinalCalleeLargeRightRootSuccessorLifecycleInstructionEvidence[] matches =
                Projection.Instructions.Where(candidate =>
                    candidate.RawFileOffset == raw).ToArray();
            if (matches.Length != 1)
            {
                return false;
            }

            CapacityFinalCalleeLargeRightRootSuccessorLifecycleInstructionEvidence instruction =
                matches[0];
            if (Normalize(instruction.Bytes) != Normalize(bytes)
                || instruction.BranchTargetRawFileOffset != target)
            {
                return false;
            }
        }

        return true;
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
