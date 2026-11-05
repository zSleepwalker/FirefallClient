using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonIndexedCountClassificationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Instruction evidence matches the exact bounded 93C0-93D4 window",
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.EntryRawFileOffset
                == 0x012C93C0
            && CapacityFinalCalleeCommonIndexedCountClassificationProjection
                .BoundaryRawFileOffsetExclusive == 0x012C93D4
            && CapacityFinalCalleeCommonIndexedCountClassificationProjection
                .FollowingInstructionRawFileOffset == 0x012C93D4
            && CapacityFinalCalleeCommonIndexedCountClassificationProjection
                .RawFileOffsetToVirtualAddress(0x012C93C0) == 0x016C9FC0
            && Normalize(CapacityFinalCalleeCommonIndexedCountClassificationProjection
                    .CapturedWindowBytes)
                == "8B1383F91F750433C0EB09D1E9B8190000002BC1"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonIndexedCountClassificationPlan missingPointer =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Project(
                new(EntryEBX: null, FreshIndexedDWORD: 0, LateEntryECX: 31));
        check("Missing EBX performs no read and leaves the supplied fresh observation unused",
            missingPointer.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.MissingEntryEBX
            && !missingPointer.IndexedDWORDReadAttempted
            && !missingPointer.IndexedDWORDReadCompleted
            && missingPointer.UnreachedFreshIndexedDWORDObservationProvided
            && missingPointer.UnreachedLateEntryECXCaptureProvided
            && missingPointer.FreshEDX is null
            && missingPointer.Reads.IsEmpty
            && missingPointer.StopAtRawFileOffsetExclusive == 0x012C93C0);

        CapacityFinalCalleeCommonIndexedCountClassificationPlan missingValue =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Project(
                new(EntryEBX: 0, FreshIndexedDWORD: null, LateEntryECX: 31));
        check("Known zero EBX attempts the fresh read and stops when its value is missing",
            missingValue.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingFreshIndexedDWORDAt93C0
            && missingValue.IndexedDWORDReadAttempted
            && !missingValue.IndexedDWORDReadCompleted
            && missingValue.IndexedDWORDReadAddress == 0
            && missingValue.Reads.SequenceEqual(new uint[] { 0 })
            && !missingValue.CompareReached
            && missingValue.StopAtRawFileOffsetExclusive == 0x012C93C0);

        CapacityFinalCalleeCommonIndexedCountClassificationPlan missingECX =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Project(
                new(EntryEBX: 0x1234, FreshIndexedDWORD: 0xA5A50000, LateEntryECX: null));
        CapacityFinalCalleeCommonIndexedCountClassificationPlan missingECXWithUnusedZero =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Project(
                new(EntryEBX: 0x1234, FreshIndexedDWORD: 0, LateEntryECX: null));
        check("Fresh EDX survives a missing late ECX stop at the CMP",
            missingECX.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome
                    .MissingLateEntryECXAt93C2
            && missingECX.IndexedDWORDReadCompleted
            && missingECX.FreshEDX == 0xA5A50000
            && missingECX.Reads.SequenceEqual(new uint[] { 0x1234 })
            && !missingECX.CompareReached
            && missingECX.CompareRight == 0x1F
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C93C2
            && missingECXWithUnusedZero.FreshEDX == 0
            && !missingECXWithUnusedZero.CompareReached);

        CapacityFinalCalleeCommonIndexedCountClassificationPlan zeroFresh =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Project(
                new(EntryEBX: 0, FreshIndexedDWORD: 0, LateEntryECX: 31));
        check("A fresh zero value is loaded into EDX and does not control classification",
            zeroFresh.IndexedDWORDReadAttempted
            && zeroFresh.IndexedDWORDReadCompleted
            && zeroFresh.FreshEDX == 0
            && zeroFresh.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
            && zeroFresh.StopAtRawFileOffsetExclusive == 0x012C93D4);

        CapacityFinalCalleeCommonIndexedCountClassificationPlan equal31 =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Project(
                new(EntryEBX: 0x20, FreshIndexedDWORD: 0xDEADBEEF, LateEntryECX: 31));
        check("ECX 1F follows XOR/JMP without assuming the incoming EAX value",
            equal31.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.Equal31At93D4
            && equal31.CompareResult == 0
            && equal31.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal31.JnzReached
            && equal31.JnzTaken == false
            && equal31.JnzSelectedDestinationRawFileOffset == 0x012C93C7
            && equal31.XorEAXReached
            && !equal31.XorEAXInputKnown
            && equal31.XorEAXResult == 0
            && equal31.XorFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal31.JumpReached
            && equal31.JumpSelectedDestinationRawFileOffset == 0x012C93D4
            && equal31.FlagsAtStop == equal31.XorFlags
            && !equal31.ShiftECXReached
            && !equal31.SubtractReached);

        CapacityFinalCalleeCommonIndexedCountClassificationPlan nonEqualZero =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Project(
                new(0, 0x11, 0));
        CapacityFinalCalleeCommonIndexedCountClassificationPlan nonEqual30 =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Project(
                new(0, 0x22, 30));
        CapacityFinalCalleeCommonIndexedCountClassificationPlan nonEqual32 =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Project(
                new(0, 0x33, 32));
        CapacityFinalCalleeCommonIndexedCountClassificationPlan nonEqualMax =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Project(
                new(0, 0x44, uint.MaxValue));
        check("Non-equal ECX values shift once, load 19h, and subtract modulo 32 bits",
            nonEqualZero.Outcome
                == CapacityFinalCalleeCommonIndexedCountClassificationOutcome.NonEqualAt93D4
            && nonEqualZero.CompareResult == 0xFFFFFFE1
            && nonEqualZero.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonEqualZero.JnzTaken == true
            && nonEqualZero.JnzSelectedDestinationRawFileOffset == 0x012C93CB
            && nonEqualZero.ShiftResultECX == 0
            && nonEqualZero.SubtractResult == 0x19
            && nonEqualZero.SubtractFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && nonEqual30.ShiftResultECX == 15
            && nonEqual30.SubtractResult == 10
            && nonEqual30.ShiftFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && nonEqual30.SubtractFlags is { Auxiliary: true, Zero: false, Carry: false }
            && nonEqual32.ShiftResultECX == 16
            && nonEqual32.SubtractResult == 9
            && nonEqual32.SubtractFlags is { Parity: true, Auxiliary: false, Carry: false }
            && nonEqualMax.CompareResult == 0xFFFFFFE0
            && nonEqualMax.CompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: false,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonEqualMax.ShiftResultECX == 0x7FFFFFFF
            && nonEqualMax.ShiftFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && nonEqualMax.SubtractResult == 0x8000001A
            && nonEqualMax.SubtractFlags is
            {
                Carry: true,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            });

        CapacityFinalCalleeCommonIndexedCountClassificationPlan sameBranchDifferentFresh =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.Project(
                new(0x99, uint.MaxValue, 32));
        check("Fresh indexed DWORD is preserved but independent of ECX branch arithmetic",
            sameBranchDifferentFresh.FreshEDX == uint.MaxValue
            && sameBranchDifferentFresh.CompareResult == nonEqual32.CompareResult
            && sameBranchDifferentFresh.CompareFlags == nonEqual32.CompareFlags
            && sameBranchDifferentFresh.ShiftResultECX == nonEqual32.ShiftResultECX
            && sameBranchDifferentFresh.SubtractResult == nonEqual32.SubtractResult
            && sameBranchDifferentFresh.FlagsAtStop == nonEqual32.FlagsAtStop);

        check("All completed paths stop exclusively before 93D4 without writes",
            new[]
            {
                zeroFresh,
                equal31,
                nonEqualZero,
                nonEqual30,
                nonEqual32,
                nonEqualMax
            }.All(plan =>
                plan.StopAtRawFileOffsetExclusive == 0x012C93D4
                && plan.Writes.IsEmpty
                && plan.CompareReached
                && plan.JnzReached
                && plan.IndexedDWORDReadCompleted));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C93C0, "8B 13", null, null),
            (0x012C93C2, "83 F9 1F", null, null),
            (0x012C93C5, "75 04", 0x012C93CB, 0x016C9FCB),
            (0x012C93C7, "33 C0", null, null),
            (0x012C93C9, "EB 09", 0x012C93D4, 0x016C9FD4),
            (0x012C93CB, "D1 E9", null, null),
            (0x012C93CD, "B8 19 00 00 00", null, null),
            (0x012C93D2, "2B C1", null, null)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonIndexedCountClassificationProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonIndexedCountClassificationInstructionEvidence instruction
            in CapacityFinalCalleeCommonIndexedCountClassificationProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonIndexedCountClassificationProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonIndexedCountClassificationProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCommonIndexedCountClassificationProjection.Instructions
                .Length == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCommonIndexedCountClassificationProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonIndexedCountClassificationProjection
                    .CapturedWindowBytes)
            && CapacityFinalCalleeCommonIndexedCountClassificationProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
