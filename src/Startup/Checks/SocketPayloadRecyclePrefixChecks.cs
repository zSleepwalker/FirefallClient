using Firefall.Reconstruction.Startup;

internal static class SocketPayloadRecyclePrefixChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The recycle prefix pins its sample, bounded raw/VA anchors, and independent mapping arithmetic",
            SocketPayloadRecyclePrefixProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && SocketPayloadRecyclePrefixProjection.PrefixStartRawFileOffset == 0xBD5C72
            && SocketPayloadRecyclePrefixProjection.PrefixStartVirtualAddress == 0xFD6872
            && SocketPayloadRecyclePrefixProjection.BoundaryRawFileOffset == 0xBD5C9C
            && SocketPayloadRecyclePrefixProjection.BoundaryVirtualAddress == 0xFD689C
            && SocketPayloadRecyclePrefixProjection.PrefixEndRawFileOffsetExclusive == 0xBD5C9C
            && SocketPayloadRecyclePrefixProjection.PrefixEndVirtualAddressExclusive == 0xFD689C
            && SocketPayloadRecyclePrefixProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x400C00
            && SocketPayloadRecyclePrefixProjection.PrefixStartRawFileOffset
                + SocketPayloadRecyclePrefixProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0xFD6872
            && SocketPayloadRecyclePrefixProjection.BoundaryRawFileOffset
                + SocketPayloadRecyclePrefixProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0xFD689C
            && SocketPayloadRecyclePrefixProjection.BoundaryRawFileOffset
                - SocketPayloadRecyclePrefixProjection.PrefixStartRawFileOffset == 0x2A);

        check("The capture contains exactly the bounded 0x2A-byte prefix and instruction-aligned slices",
            SocketPayloadRecyclePrefixProjection.CapturedPrefixBytes
                == "8B 73 10 85 F6 74 23 8D 46 04 8B CF F0 0F C1 08 "
                    + "75 18 8B 06 8B CE FF 10 8D 46 08 8B D7 F0 0F C1 10 "
                    + "75 07 8B 06 8B CE FF 50 04"
            && SocketPayloadRecyclePrefixProjection.CapturedPrefixBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x2A
            && SocketPayloadRecyclePrefixProjection.Instructions.All(InstructionMatchesCapturedSlice));

        check("The ordered pointer, counter, indirect-call, and normal-return offsets match the stated trace",
            SocketPayloadRecyclePrefixProjection.AssociatedPointerLoadRawFileOffset == 0xBD5C72
            && SocketPayloadRecyclePrefixProjection.AssociatedPointerTestRawFileOffset == 0xBD5C75
            && SocketPayloadRecyclePrefixProjection.AssociatedPointerBranchRawFileOffset == 0xBD5C77
            && SocketPayloadRecyclePrefixProjection.AssociatedPointerNullBranchTargetRawFileOffset == 0xBD5C9C
            && SocketPayloadRecyclePrefixProjection.Counter4AddressRawFileOffset == 0xBD5C79
            && SocketPayloadRecyclePrefixProjection.Counter4XaddRawFileOffset == 0xBD5C7E
            && SocketPayloadRecyclePrefixProjection.Counter4BranchRawFileOffset == 0xBD5C82
            && SocketPayloadRecyclePrefixProjection.Counter4BranchTargetRawFileOffset == 0xBD5C9C
            && SocketPayloadRecyclePrefixProjection.Slot0VtableLoadRawFileOffset == 0xBD5C84
            && SocketPayloadRecyclePrefixProjection.Slot0ThisArgumentRawFileOffset == 0xBD5C86
            && SocketPayloadRecyclePrefixProjection.Slot0CallRawFileOffset == 0xBD5C88
            && SocketPayloadRecyclePrefixProjection.Slot0CallNormalReturnRawFileOffset == 0xBD5C8A
            && SocketPayloadRecyclePrefixProjection.Counter8AddressRawFileOffset == 0xBD5C8A
            && SocketPayloadRecyclePrefixProjection.Counter8XaddRawFileOffset == 0xBD5C8F
            && SocketPayloadRecyclePrefixProjection.Counter8BranchRawFileOffset == 0xBD5C93
            && SocketPayloadRecyclePrefixProjection.Counter8BranchTargetRawFileOffset == 0xBD5C9C
            && SocketPayloadRecyclePrefixProjection.Slot4VtableLoadRawFileOffset == 0xBD5C95
            && SocketPayloadRecyclePrefixProjection.Slot4ThisArgumentRawFileOffset == 0xBD5C97
            && SocketPayloadRecyclePrefixProjection.Slot4CallRawFileOffset == 0xBD5C99
            && SocketPayloadRecyclePrefixProjection.Slot4CallNormalReturnRawFileOffset == 0xBD5C9C
            && SocketPayloadRecyclePrefixProjection.ExplicitStackArgumentBytes == 0
            && SocketPayloadRecyclePrefixProjection.XaddSource == uint.MaxValue);

        SocketPayloadRecyclePrefixPlan nullAssociated =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(
                    AssociatedPointerValue: 0,
                    Counter4BeforeXadd: null,
                    Slot0VtableValue: null,
                    Slot0ReturnedNormally: null,
                    Counter8BeforeXadd: null,
                    Slot4VtableValue: null,
                    Slot4ReturnedNormally: null));
        check("A null associated pointer branches directly to 0x9C without counter or call actions",
            nullAssociated.Counter4Outcome == SocketPayloadRecyclePrefixCounterOutcome.NotReached
            && nullAssociated.Slot0VtableOutcome == SocketPayloadRecyclePrefixVtableOutcome.NotReached
            && nullAssociated.Slot0CallOutcome == SocketPayloadRecyclePrefixCallOutcome.NotReached
            && nullAssociated.Counter8Outcome == SocketPayloadRecyclePrefixCounterOutcome.NotReached
            && nullAssociated.Slot4VtableOutcome == SocketPayloadRecyclePrefixVtableOutcome.NotReached
            && nullAssociated.Slot4CallOutcome == SocketPayloadRecyclePrefixCallOutcome.NotReached
            && nullAssociated.BoundaryReached
            && nullAssociated.StopAtRawFileOffsetExclusive == 0xBD5C9C
            && nullAssociated.Actions.Select(action => action.Kind).SequenceEqual(
                new[]
                {
                    SocketPayloadRecyclePrefixActionKind.LoadAssociatedPointer,
                    SocketPayloadRecyclePrefixActionKind.TestAssociatedPointer,
                    SocketPayloadRecyclePrefixActionKind.BranchNullAssociatedPointer
                }));

        SocketPayloadRecyclePrefixPlan missingPointer =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(null, null, null, null, null, null, null));
        check("A missing associated-pointer capture leaves branch-dependent paths visibly undetermined",
            missingPointer.Counter4Outcome == SocketPayloadRecyclePrefixCounterOutcome.PathUndetermined
            && missingPointer.Slot0CallOutcome == SocketPayloadRecyclePrefixCallOutcome.PathUndetermined
            && missingPointer.Counter8Outcome == SocketPayloadRecyclePrefixCounterOutcome.PathUndetermined
            && missingPointer.Slot4CallOutcome == SocketPayloadRecyclePrefixCallOutcome.PathUndetermined
            && !missingPointer.BoundaryReached
            && missingPointer.StopAtRawFileOffsetExclusive == 0xBD5C75
            && missingPointer.Actions.Select(action => action.Kind).SequenceEqual(
                new[] { SocketPayloadRecyclePrefixActionKind.LoadAssociatedPointer })
            && Rejects(
                new SocketPayloadRecyclePrefixCapture(0, 1, null, null, null, null, null),
                nameof(SocketPayloadRecyclePrefixCapture.Counter4BeforeXadd)));

        SocketPayloadRecyclePrefixPlan firstCounterUpdatedNonzero =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(
                    AssociatedPointerValue: 0x1000,
                    Counter4BeforeXadd: 2,
                    Slot0VtableValue: null,
                    Slot0ReturnedNormally: null,
                    Counter8BeforeXadd: null,
                    Slot4VtableValue: null,
                    Slot4ReturnedNormally: null));
        check("A nonzero updated first counter branches to 0x9C and skips both calls and the second counter",
            firstCounterUpdatedNonzero.Counter4Address == 0x1004
            && firstCounterUpdatedNonzero.Counter4OldValue == 2
            && firstCounterUpdatedNonzero.Counter4UpdatedValue == 1
            && firstCounterUpdatedNonzero.Counter8Address is null
            && firstCounterUpdatedNonzero.Counter8Outcome
                == SocketPayloadRecyclePrefixCounterOutcome.NotReached
            && firstCounterUpdatedNonzero.Slot0CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.NotReached
            && firstCounterUpdatedNonzero.Slot4CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.NotReached
            && firstCounterUpdatedNonzero.BoundaryReached
            && firstCounterUpdatedNonzero.Actions.Single(
                action => action.Kind == SocketPayloadRecyclePrefixActionKind.BranchOnCounter4UpdatedValue)
                .ConditionMet == true
            && Rejects(
                new SocketPayloadRecyclePrefixCapture(0x1000, 2, 0x1111, null, null, null, null),
                nameof(SocketPayloadRecyclePrefixCapture.Slot0VtableValue)));

        SocketPayloadRecyclePrefixPlan bothCallsReturn =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(
                    AssociatedPointerValue: 0xFFFFFFFC,
                    Counter4BeforeXadd: 1,
                    Slot0VtableValue: 0x11111111,
                    Slot0ReturnedNormally: true,
                    Counter8BeforeXadd: 1,
                    Slot4VtableValue: 0x22222222,
                    Slot4ReturnedNormally: true));
        check("The two counter addresses wrap, and slot 4 uses a fresh vtable capture after slot 0 returns",
            bothCallsReturn.Counter4Address == 0
            && bothCallsReturn.Counter4UpdatedValue == 0
            && bothCallsReturn.Slot0VtableValue == 0x11111111
            && bothCallsReturn.Slot0CallOutcome == SocketPayloadRecyclePrefixCallOutcome.ReturnedNormally
            && bothCallsReturn.Counter8Address == 4
            && bothCallsReturn.Counter8UpdatedValue == 0
            && bothCallsReturn.Slot4VtableValue == 0x22222222
            && bothCallsReturn.Slot4CallOutcome == SocketPayloadRecyclePrefixCallOutcome.ReturnedNormally
            && bothCallsReturn.BoundaryReached
            && bothCallsReturn.StopAtRawFileOffsetExclusive == 0xBD5C9C
            && bothCallsReturn.Actions.Select(action => action.RawFileOffset).Contains(0xBD5C95u)
            && bothCallsReturn.Actions.Select(action => action.RawFileOffset).Contains(0xBD5C99u)
            && bothCallsReturn.Actions.Single(
                action => action.Kind == SocketPayloadRecyclePrefixActionKind.CallSlot0)
                .CapturedValue == 0x11111111
            && bothCallsReturn.Actions.Single(
                action => action.Kind == SocketPayloadRecyclePrefixActionKind.CallSlot4)
                .CapturedValue == 0x22222222
            && bothCallsReturn.Actions.Single(
                action => action.Kind == SocketPayloadRecyclePrefixActionKind.CallSlot0)
                .CallReturnedNormally == true
            && bothCallsReturn.Actions.Single(
                action => action.Kind == SocketPayloadRecyclePrefixActionKind.CallSlot4)
                .CallReturnedNormally == true);

        SocketPayloadRecyclePrefixPlan secondCounterUpdatedNonzero =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(
                    AssociatedPointerValue: 0x9000,
                    Counter4BeforeXadd: 1,
                    Slot0VtableValue: 0xABCDEF01,
                    Slot0ReturnedNormally: true,
                    Counter8BeforeXadd: 2,
                    Slot4VtableValue: null,
                    Slot4ReturnedNormally: null));
        check("A nonzero updated second counter reaches the boundary without loading or calling slot 4",
            secondCounterUpdatedNonzero.Counter4UpdatedValue == 0
            && secondCounterUpdatedNonzero.Counter8Address == 0x9008
            && secondCounterUpdatedNonzero.Counter8OldValue == 2
            && secondCounterUpdatedNonzero.Counter8UpdatedValue == 1
            && secondCounterUpdatedNonzero.Slot0CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.ReturnedNormally
            && secondCounterUpdatedNonzero.Slot4VtableOutcome
                == SocketPayloadRecyclePrefixVtableOutcome.NotReached
            && secondCounterUpdatedNonzero.Slot4CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.NotReached
            && secondCounterUpdatedNonzero.BoundaryReached
            && secondCounterUpdatedNonzero.Actions.Single(
                action => action.Kind == SocketPayloadRecyclePrefixActionKind.BranchOnCounter8UpdatedValue)
                .ConditionMet == true
            && Rejects(
                new SocketPayloadRecyclePrefixCapture(0x9000, 1, 0xABCDEF01, true, 2, 0x2222, null),
                nameof(SocketPayloadRecyclePrefixCapture.Slot4VtableValue)));

        SocketPayloadRecyclePrefixPlan missingSlot0Vtable =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(
                    AssociatedPointerValue: 0x2000,
                    Counter4BeforeXadd: 1,
                    Slot0VtableValue: null,
                    Slot0ReturnedNormally: null,
                    Counter8BeforeXadd: null,
                    Slot4VtableValue: null,
                    Slot4ReturnedNormally: null));
        check("A missing slot-0 vtable capture stops at its load and rejects later call/counter captures",
            missingSlot0Vtable.Slot0VtableOutcome
                == SocketPayloadRecyclePrefixVtableOutcome.MissingValueCapture
            && missingSlot0Vtable.Slot0CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.PathUndetermined
            && missingSlot0Vtable.Counter8Outcome
                == SocketPayloadRecyclePrefixCounterOutcome.PathUndetermined
            && missingSlot0Vtable.Slot4CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.PathUndetermined
            && missingSlot0Vtable.StopAtRawFileOffsetExclusive == 0xBD5C86
            && missingSlot0Vtable.Actions.Any(
                action => action.Kind == SocketPayloadRecyclePrefixActionKind.LoadSlot0Vtable)
            && !missingSlot0Vtable.Actions.Any(
                action => action.Kind is
                    SocketPayloadRecyclePrefixActionKind.SetSlot0ThisArgument
                    or SocketPayloadRecyclePrefixActionKind.CallSlot0)
            && Rejects(
                new SocketPayloadRecyclePrefixCapture(0x2000, 1, null, true, null, null, null),
                nameof(SocketPayloadRecyclePrefixCapture.Slot0ReturnedNormally))
            && Rejects(
                new SocketPayloadRecyclePrefixCapture(0x2000, 1, null, null, 2, null, null),
                nameof(SocketPayloadRecyclePrefixCapture.Counter8BeforeXadd))
            && Rejects(
                new SocketPayloadRecyclePrefixCapture(0x2000, 1, null, null, null, 0x9999, null),
                nameof(SocketPayloadRecyclePrefixCapture.Slot4VtableValue)));

        SocketPayloadRecyclePrefixPlan missingSlot4Vtable =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(
                    AssociatedPointerValue: 0x2800,
                    Counter4BeforeXadd: 1,
                    Slot0VtableValue: 0x12345678,
                    Slot0ReturnedNormally: true,
                    Counter8BeforeXadd: 1,
                    Slot4VtableValue: null,
                    Slot4ReturnedNormally: null));
        check("A missing slot-4 vtable capture stops at its load without a slot-4 call action",
            missingSlot4Vtable.Slot4VtableOutcome
                == SocketPayloadRecyclePrefixVtableOutcome.MissingValueCapture
            && missingSlot4Vtable.Slot4CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.PathUndetermined
            && !missingSlot4Vtable.BoundaryReached
            && missingSlot4Vtable.StopAtRawFileOffsetExclusive == 0xBD5C97
            && missingSlot4Vtable.Actions.Any(
                action => action.Kind == SocketPayloadRecyclePrefixActionKind.LoadSlot4Vtable)
            && !missingSlot4Vtable.Actions.Any(
                action => action.Kind is
                    SocketPayloadRecyclePrefixActionKind.SetSlot4ThisArgument
                    or SocketPayloadRecyclePrefixActionKind.CallSlot4)
            && Rejects(
                new SocketPayloadRecyclePrefixCapture(0x2800, 1, 0x12345678, true, 1, null, false),
                nameof(SocketPayloadRecyclePrefixCapture.Slot4ReturnedNormally)));

        SocketPayloadRecyclePrefixPlan missingCounter4 =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(0x3000, null, null, null, null, null, null));
        check("A missing first-counter capture remains distinct from an unreached call and rejects later captures",
            missingCounter4.Counter4Outcome == SocketPayloadRecyclePrefixCounterOutcome.MissingValueCapture
            && missingCounter4.Slot0VtableOutcome == SocketPayloadRecyclePrefixVtableOutcome.PathUndetermined
            && missingCounter4.Slot0CallOutcome == SocketPayloadRecyclePrefixCallOutcome.PathUndetermined
            && missingCounter4.Counter8Outcome == SocketPayloadRecyclePrefixCounterOutcome.PathUndetermined
            && missingCounter4.StopAtRawFileOffsetExclusive == 0xBD5C82
            && Rejects(
                new SocketPayloadRecyclePrefixCapture(0x3000, null, 1, null, null, null, null),
                nameof(SocketPayloadRecyclePrefixCapture.Slot0VtableValue)));

        SocketPayloadRecyclePrefixPlan slot0DidNotReturn =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(0x4000, 1, 0x3333, false, null, null, null));
        check("A reached slot-0 call that does not return blocks the second counter and slot-4 path",
            slot0DidNotReturn.Slot0CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.DidNotReturnNormally
            && slot0DidNotReturn.Counter8Outcome == SocketPayloadRecyclePrefixCounterOutcome.NotReached
            && slot0DidNotReturn.Slot4CallOutcome == SocketPayloadRecyclePrefixCallOutcome.NotReached
            && !slot0DidNotReturn.BoundaryReached
            && slot0DidNotReturn.StopAtRawFileOffsetExclusive == 0xBD5C8A
            && Rejects(
                new SocketPayloadRecyclePrefixCapture(0x4000, 1, 0x3333, false, 1, null, null),
                nameof(SocketPayloadRecyclePrefixCapture.Counter8BeforeXadd)));

        SocketPayloadRecyclePrefixPlan missingSlot0Return =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(0x5000, 1, 0x4444, null, null, null, null));
        check("A reached slot-0 call with a missing return capture leaves later execution undetermined",
            missingSlot0Return.Slot0CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.MissingReturnCapture
            && missingSlot0Return.Counter8Outcome
                == SocketPayloadRecyclePrefixCounterOutcome.PathUndetermined
            && missingSlot0Return.Slot4CallOutcome == SocketPayloadRecyclePrefixCallOutcome.PathUndetermined
            && !missingSlot0Return.BoundaryReached);

        SocketPayloadRecyclePrefixPlan missingCounter8 =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(0x6000, 1, 0x5555, true, null, null, null));
        check("A missing second-counter capture leaves the slot-4 vtable and call path undetermined",
            missingCounter8.Slot0CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.ReturnedNormally
            && missingCounter8.Counter8Outcome
                == SocketPayloadRecyclePrefixCounterOutcome.MissingValueCapture
            && missingCounter8.Slot4VtableOutcome
                == SocketPayloadRecyclePrefixVtableOutcome.PathUndetermined
            && missingCounter8.Slot4CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.PathUndetermined
            && Rejects(
                new SocketPayloadRecyclePrefixCapture(0x6000, 1, 0x5555, true, null, 0x6666, null),
                nameof(SocketPayloadRecyclePrefixCapture.Slot4VtableValue)));

        SocketPayloadRecyclePrefixPlan missingSlot4Return =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(
                    0x7000, 1, 0x7771, true, 1, 0x7772, null));
        check("A reached slot-4 call with no normal-return capture stops before the 0x9C boundary",
            missingSlot4Return.Slot4VtableOutcome
                == SocketPayloadRecyclePrefixVtableOutcome.Captured
            && missingSlot4Return.Slot4CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.MissingReturnCapture
            && !missingSlot4Return.BoundaryReached
            && missingSlot4Return.StopAtRawFileOffsetExclusive == 0xBD5C9C);

        SocketPayloadRecyclePrefixPlan slot4DidNotReturn =
            SocketPayloadRecyclePrefixProjection.Project(
                new SocketPayloadRecyclePrefixCapture(
                    0x8000, 1, 0x8881, true, 1, 0x8882, false));
        check("A slot-4 call that does not return is distinguished from reaching the normal-return boundary",
            slot4DidNotReturn.Slot4CallOutcome
                == SocketPayloadRecyclePrefixCallOutcome.DidNotReturnNormally
            && !slot4DidNotReturn.BoundaryReached
            && slot4DidNotReturn.StopAtRawFileOffsetExclusive == 0xBD5C9C);
    }

    private static bool InstructionMatchesCapturedSlice(
        SocketPayloadRecyclePrefixInstructionEvidence instruction)
    {
        string[] captured = SocketPayloadRecyclePrefixProjection.CapturedPrefixBytes
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] expected = instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (instruction.RawFileOffset < SocketPayloadRecyclePrefixProjection.PrefixStartRawFileOffset)
        {
            return false;
        }

        int offset = checked((int)(
            instruction.RawFileOffset - SocketPayloadRecyclePrefixProjection.PrefixStartRawFileOffset));
        return offset + expected.Length <= captured.Length
            && captured.Skip(offset).Take(expected.Length)
                .SequenceEqual(expected, StringComparer.OrdinalIgnoreCase);
    }

    private static bool Rejects(
        SocketPayloadRecyclePrefixCapture capture,
        string expectedParameterName)
    {
        try
        {
            SocketPayloadRecyclePrefixProjection.Project(capture);
            return false;
        }
        catch (ArgumentException exception)
        {
            return exception.ParamName == expectedParameterName;
        }
    }
}
