using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonTraversalIterationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The one-iteration byte window, instruction addresses, and branch edges match",
            CapacityFinalCalleeCommonTraversalIterationProjection.EntryRawFileOffset
                == 0x012C93E4
            && CapacityFinalCalleeCommonTraversalIterationProjection.EntryVirtualAddress
                == 0x016C9FE4
            && CapacityFinalCalleeCommonTraversalIterationProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9403
            && CapacityFinalCalleeCommonTraversalIterationProjection.ZeroChildBranchTargetRawFileOffset
                == 0x012C945C
            && CapacityFinalCalleeCommonTraversalIterationProjection
                .ReturnImmediatelyBeforeZeroChildTargetRawFileOffset == 0x012C945B
            && CapacityFinalCalleeCommonTraversalIterationProjection.LoopBranchTargetRawFileOffset
                == 0x012C93E4
            && CapacityFinalCalleeCommonTraversalIterationProjection.EqualFallthroughRawFileOffset
                == 0x012C9403
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonTraversalIterationPlan missingEBX =
            Project(null, 0x100, 0x200, 0x208, 0x210);
        check("Missing entry EBX stops at the first consumer and leaves later observations unused",
            missingEBX.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome.MissingEntryEBXAt93E4
            && missingEBX.EntryMoveReached
            && !missingEBX.EntryEBXCaptured
            && missingEBX.Events is
            [
                {
                    Kind: CapacityFinalCalleeCommonTraversalIterationEventKind.MoveEBXToEAX,
                    RawFileOffset: 0x012C93E4,
                    Input: null,
                    Result: null
                }
            ]
            && missingEBX.ReachedInstructionRawFileOffsets.SequenceEqual(new[] { 0x012C93E4u })
            && missingEBX.UnreachedLateEntryEDXCaptureProvided
            && missingEBX.UnreachedFreshChildDWORDCaptureProvided
            && missingEBX.UnreachedLateFreshChildPlus4CaptureProvided
            && missingEBX.UnreachedEntryEDICaptureProvided
            && missingEBX.Reads.IsEmpty
            && !missingEBX.DoubleEBXReached
            && !missingEBX.ExactSingleIterationEvidence
            && missingEBX.StopAtRawFileOffsetExclusive == 0x012C93E4
            && !missingEBX.TargetEntered);

        CapacityFinalCalleeCommonTraversalIterationPlan missingEDX =
            Project(0x80000000, null, 0, 0, 0);
        check("Known index arithmetic survives a missing LEA base without attempting a read",
            missingEDX.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome.MissingLateEntryEDXAt93EE
            && missingEDX.EAXAfterEntryMove == 0x80000000
            && missingEDX.EBXAfterDouble == 0
            && missingEDX.DoubleEBXFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: true
            }
            && missingEDX.ShiftResultEAX == 1
            && missingEDX.ShiftFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: null
            }
            && missingEDX.EAXAfterAddFour == 5
            && missingEDX.ChildAddressLeaReached
            && !missingEDX.LateEntryEDXCaptured
            && missingEDX.ChildFieldOffset == 20
            && missingEDX.ChildFieldAddress is null
            && missingEDX.Reads.IsEmpty
            && missingEDX.UnreachedFreshChildDWORDCaptureProvided
            && missingEDX.FlagsAtStop == missingEDX.AddFourFlags
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C93EE);

        CapacityFinalCalleeCommonTraversalIterationPlan missingChild =
            Project(0x80000000, 0xFFFFFFF8, null, 0x5555, 0x7777);
        check("The indexed child load is attempted at the wrapped address before a missing value stops",
            missingChild.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome.MissingFreshChildDWORDAt93F1
            && missingChild.ChildFieldOffset == 20
            && missingChild.ChildFieldAddress == 0x0000000C
            && missingChild.Reads is
            [
                {
                    RawFileOffset: 0x012C93F1,
                    Address: 0x0000000C,
                    AddressCaptured: true,
                    ValueCaptured: false,
                    Value: null
                }
            ]
            && missingChild.FreshChildReadReached
            && !missingChild.FreshChildDWORDCaptured
            && missingChild.UnreachedLateFreshChildPlus4CaptureProvided
            && missingChild.UnreachedEntryEDICaptureProvided
            && !missingChild.ChildTestFlags.HasValue
            && missingChild.FlagsAtStop == missingChild.AddFourFlags
            && missingChild.StopAtRawFileOffsetExclusive == 0x012C93F1);

        CapacityFinalCalleeCommonTraversalIterationPlan missingPlus4 =
            Project(0, 0x100, 0x120, null, 0x120);
        check("A nonzero child is moved to EDX before the independent child-plus-four read",
            missingPlus4.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MissingLateFreshChildPlus4At93F9
            && missingPlus4.FreshChildDWORDCaptured
            && missingPlus4.FreshChildDWORD == 0x120
            && missingPlus4.ChildZeroBranchReached
            && missingPlus4.ChildZeroBranchTaken == false
            && missingPlus4.MoveChildToEDXReached
            && missingPlus4.EDXAfterChildMove == 0x120
            && missingPlus4.FreshChildPlus4Address == 0x124
            && missingPlus4.Reads.Length == 2
            && missingPlus4.Reads[1] is
            {
                RawFileOffset: 0x012C93F9,
                Address: 0x124,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && missingPlus4.UnreachedEntryEDICaptureProvided
            && !missingPlus4.AndEAXReached
            && missingPlus4.FlagsAtStop == missingPlus4.ChildTestFlags
            && missingPlus4.StopAtRawFileOffsetExclusive == 0x012C93F9);

        CapacityFinalCalleeCommonTraversalIterationPlan missingEDI =
            Project(0, 0x100, 0x120, 0x1237, null);
        check("A missing entry EDI retains the independent fresh value and AND flags at CMP",
            missingEDI.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome.MissingEntryEDIAt93FF
            && missingEDI.FreshChildDWORD == 0x120
            && missingEDI.EDXAfterChildMove == 0x120
            && missingEDI.FreshChildPlus4Address == 0x124
            && missingEDI.FreshChildPlus4 == 0x1237
            && missingEDI.AndInputEAX == 0x1237
            && missingEDI.MaskedChildDWORD == 0x1230
            && missingEDI.AndFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingEDI.CompareReached
            && !missingEDI.EntryEDICaptured
            && missingEDI.CompareLeft == 0x1230
            && missingEDI.CompareRight is null
            && missingEDI.CompareResult is null
            && missingEDI.FlagsAtStop == missingEDI.AndFlags
            && missingEDI.StopAtRawFileOffsetExclusive == 0x012C93FF);

        CapacityFinalCalleeCommonTraversalIterationPlan zeroChild =
            Project(0, 0x100, 0, 0x104, 0);
        check("A captured zero child takes 945C and leaves both later captures unused",
            zeroChild.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome.ZeroChildTo945C
            && zeroChild.FreshChildReadReached
            && zeroChild.FreshChildDWORDCaptured
            && zeroChild.FreshChildDWORD == 0
            && zeroChild.ChildTestFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroChild.ChildZeroBranchReached
            && zeroChild.ChildZeroBranchTaken == true
            && zeroChild.ChildZeroBranchDestinationRawFileOffset == 0x012C945C
            && CapacityFinalCalleeCommonTraversalIterationProjection
                .ReturnImmediatelyBeforeZeroChildTargetRawFileOffset == 0x012C945B
            && zeroChild.Reads.Length == 1
            && !zeroChild.FreshChildPlus4ReadReached
            && !zeroChild.MoveChildToEDXReached
            && !zeroChild.AndEAXReached
            && !zeroChild.CompareReached
            && zeroChild.UnreachedLateFreshChildPlus4CaptureProvided
            && zeroChild.UnreachedEntryEDICaptureProvided
            && !zeroChild.ExactSingleIterationEvidence
            && zeroChild.FlagsAtStop == zeroChild.ChildTestFlags
            && zeroChild.StopAtRawFileOffsetExclusive == 0x012C945C
            && !zeroChild.TargetEntered);

        CapacityFinalCalleeCommonTraversalIterationPlan zeroChildExact =
            Project(0, 0x100, 0, null, null);
        check("The zero-child edge stops at additional code rather than entering the preceding RET",
            zeroChildExact.ExactSingleIterationEvidence
            && zeroChildExact.StopAtRawFileOffsetExclusive == 0x012C945C
            && zeroChildExact.StopAtRawFileOffsetExclusive
                != CapacityFinalCalleeCommonTraversalIterationProjection
                    .ReturnImmediatelyBeforeZeroChildTargetRawFileOffset
            && zeroChildExact.ReachedInstructionRawFileOffsets.SequenceEqual(new[]
            {
                0x012C93E4u, 0x012C93E6u, 0x012C93E8u, 0x012C93EBu,
                0x012C93EEu, 0x012C93F1u, 0x012C93F3u, 0x012C93F5u
            })
            && zeroChildExact.Events[^1].BranchTaken == true
            && zeroChildExact.Events[^1].SelectedDestinationRawFileOffset == 0x012C945C);

        CapacityFinalCalleeCommonTraversalIterationPlan msbClear =
            Project(0, 0xFFFFFFF8, 0, 0, 0);
        CapacityFinalCalleeCommonTraversalIterationPlan msbSet =
            Project(0x80000000, 0xFFFFFFF8, 0, 0, 0);
        CapacityFinalCalleeCommonTraversalIterationPlan bit30Set =
            Project(0x40000000, 0x100, 0, 0, 0);
        CapacityFinalCalleeCommonTraversalIterationPlan doubledWrap =
            Project(uint.MaxValue, 0, 0, 0, 0);
        check("The source MSB selects byte offsets 16 or 20 while arithmetic and addresses wrap",
            msbClear.ShiftResultEAX == 0
            && msbClear.EAXAfterAddFour == 4
            && msbClear.ChildFieldOffset == 16
            && msbClear.ChildFieldAddress == 8
            && msbSet.ShiftResultEAX == 1
            && msbSet.EAXAfterAddFour == 5
            && msbSet.ChildFieldOffset == 20
            && msbSet.ChildFieldAddress == 0x0000000C
            && bit30Set.ShiftFlags?.Carry == true
            && bit30Set.ShiftResultEAX == 0
            && bit30Set.ChildFieldOffset == 16
            && doubledWrap.EBXAfterDouble == 0xFFFFFFFE
            && doubledWrap.DoubleEBXFlags?.Carry == true
            && doubledWrap.DoubleEBXFlags?.Overflow == false
            && CapacityFinalCalleeCommonTraversalIterationProjection
                .DeriveChildFieldOffset(0) == 16
            && CapacityFinalCalleeCommonTraversalIterationProjection
                .DeriveChildFieldOffset(0x80000000) == 20
            && CapacityFinalCalleeCommonTraversalIterationProjection
                .DeriveChildFieldAddress(0xFFFFFFF8, 0x80000000) == 0x0000000C
            && CapacityFinalCalleeCommonTraversalIterationProjection
                .DeriveFreshChildPlus4Address(0xFFFFFFFC) == 0);

        CapacityFinalCalleeCommonTraversalIterationPlan equal =
            Project(0, 0x100, 0x120, 0x1237, 0x1230);
        check("Fresh child-plus-four data is independent and an equal masked value falls through",
            equal.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome.MaskedChildEqualsEntryEDIAt9403
            && equal.Reads.Select(read => read.RawFileOffset).SequenceEqual(new[]
            {
                0x012C93F1u, 0x012C93F9u
            })
            && equal.Reads[0].Address == 0x110
            && equal.Reads[0].Value == 0x120
            && equal.EDXAfterChildMove == 0x120
            && equal.Reads[1].Address == 0x124
            && equal.Reads[1].Value == 0x1237
            && equal.AndInputEAX == 0x1237
            && equal.MaskedChildDWORD == 0x1230
            && equal.CompareLeft == 0x1230
            && equal.CompareRight == 0x1230
            && equal.CompareResult == 0
            && equal.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.LoopBranchReached
            && equal.LoopBranchTaken == false
            && equal.LoopBranchSelectedDestinationRawFileOffset == 0x012C9403
            && equal.StopAtRawFileOffsetExclusive == 0x012C9403
            && equal.FlagsAtStop == equal.CompareFlags
            && equal.ExactSingleIterationEvidence
            && equal.ReachedInstructionRawFileOffsets.SequenceEqual(new[]
            {
                0x012C93E4u, 0x012C93E6u, 0x012C93E8u, 0x012C93EBu,
                0x012C93EEu, 0x012C93F1u, 0x012C93F3u, 0x012C93F5u,
                0x012C93F7u, 0x012C93F9u, 0x012C93FCu, 0x012C93FFu, 0x012C9401u
            })
            && !equal.TargetEntered);

        CapacityFinalCalleeCommonTraversalIterationPlan different =
            CapacityFinalCalleeCommonTraversalIterationProjection.Project(new(
            EntryEBX: 0,
            LateEntryEDX: 0x100,
            FreshChildDWORD: 0x120,
            LateFreshChildPlus4: 0x1230,
            EntryEDI: 0x1238));
        check("A non-equal masked value selects one loop-back edge without another iteration",
            different.Outcome
                == CapacityFinalCalleeCommonTraversalIterationOutcome
                    .MaskedChildDiffersFromEntryEDITo93E4
            && different.CompareResult == 0xFFFFFFF8
            && different.CompareFlags is
            {
                Carry: true,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && different.LoopBranchTaken == true
            && different.LoopBranchSelectedDestinationRawFileOffset == 0x012C93E4
            && different.StopAtRawFileOffsetExclusive == 0x012C93E4
            && different.Events.Count(item =>
                item.Kind == CapacityFinalCalleeCommonTraversalIterationEventKind.MoveEBXToEAX) == 1
            && different.ReachedInstructionRawFileOffsets.Count(offset => offset == 0x012C93E4) == 1
            && different.ExactSingleIterationEvidence
            && !different.TargetEntered);

        check("Instruction order keeps reads, preserved flags, and both conditional branches bounded",
            equal.Events.Select(item => item.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeCommonTraversalIterationEventKind.MoveEBXToEAX,
                CapacityFinalCalleeCommonTraversalIterationEventKind.DoubleEBX,
                CapacityFinalCalleeCommonTraversalIterationEventKind.ShiftEAXRight31,
                CapacityFinalCalleeCommonTraversalIterationEventKind.AddFourToEAX,
                CapacityFinalCalleeCommonTraversalIterationEventKind.FormChildAddress,
                CapacityFinalCalleeCommonTraversalIterationEventKind.LoadFreshChildDWORD,
                CapacityFinalCalleeCommonTraversalIterationEventKind.TestChildDWORD,
                CapacityFinalCalleeCommonTraversalIterationEventKind.BranchIfChildZero,
                CapacityFinalCalleeCommonTraversalIterationEventKind.MoveChildToEDX,
                CapacityFinalCalleeCommonTraversalIterationEventKind.LoadFreshChildPlus4,
                CapacityFinalCalleeCommonTraversalIterationEventKind.AndEAXWithFFFFFFF8,
                CapacityFinalCalleeCommonTraversalIterationEventKind.CompareEAXWithEDI,
                CapacityFinalCalleeCommonTraversalIterationEventKind.BranchIfNotEqual
            })
            && equal.Events[3].Flags == equal.AddFourFlags
            && equal.Events[4].Flags == equal.AddFourFlags
            && equal.Events[5].Flags == equal.AddFourFlags
            && equal.Events[6].Flags == equal.ChildTestFlags
            && equal.Events[6].Result == 0x120
            && equal.Events[7].Flags == equal.ChildTestFlags
            && equal.Events[8].Flags == equal.ChildTestFlags
            && equal.Events[9].Flags == equal.ChildTestFlags
            && equal.Events[10].Flags == equal.AndFlags
            && equal.Events[11].Flags == equal.CompareFlags
            && equal.Events[12].Flags == equal.CompareFlags
            && new[] { equal, different, zeroChildExact }.All(plan =>
                !plan.TargetEntered
                && plan.StopAtRawFileOffsetExclusive is 0x012C9403 or 0x012C93E4 or 0x012C945C));
    }

    private static CapacityFinalCalleeCommonTraversalIterationPlan Project(
        uint? entryEBX,
        uint? lateEntryEDX,
        uint? freshChildDWORD,
        uint? lateFreshChildPlus4,
        uint? entryEDI) =>
        CapacityFinalCalleeCommonTraversalIterationProjection.Project(new(
            entryEBX,
            lateEntryEDX,
            freshChildDWORD,
            lateFreshChildPlus4,
            entryEDI));

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C93E4, "8B C3", null, null),
            (0x012C93E6, "03 DB", null, null),
            (0x012C93E8, "C1 E8 1F", null, null),
            (0x012C93EB, "83 C0 04", null, null),
            (0x012C93EE, "8D 0C 82", null, null),
            (0x012C93F1, "8B 01", null, null),
            (0x012C93F3, "85 C0", null, null),
            (0x012C93F5, "74 65", 0x012C945C, 0x016CA05C),
            (0x012C93F7, "8B D0", null, null),
            (0x012C93F9, "8B 42 04", null, null),
            (0x012C93FC, "83 E0 F8", null, null),
            (0x012C93FF, "3B C7", null, null),
            (0x012C9401, "75 E1", 0x012C93E4, 0x016C9FE4)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonTraversalIterationProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonTraversalIterationInstructionEvidence instruction
            in CapacityFinalCalleeCommonTraversalIterationProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonTraversalIterationProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonTraversalIterationProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCommonTraversalIterationProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCommonTraversalIterationProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonTraversalIterationProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonTraversalIterationProjection.Instructions
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
