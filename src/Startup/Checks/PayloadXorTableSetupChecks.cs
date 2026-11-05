using Firefall.Reconstruction.Startup;

internal static class PayloadXorTableSetupChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The authorized sample, bounded raw bytes, raw-to-VA mapping, and instruction boundaries are pinned",
            PayloadXorTableSetupProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadXorTableSetupProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadXorTableSetupProjection.EntryStartRawFileOffset == 0x00BD5912
            && PayloadXorTableSetupProjection.EntryEndRawFileOffsetExclusive == 0x00BD5939
            && PayloadXorTableSetupProjection.CapturedCodeLength == 0x27
            && PayloadXorTableSetupProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PayloadXorTableSetupProjection.EntryStartVirtualAddress == 0x00FD6512
            && PayloadXorTableSetupProjection.EntryEndVirtualAddressExclusive == 0x00FD6539
            && string.Concat(PayloadXorTableSetupProjection.CapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B4D080FB6C003CB8B14C59074BB018B34C59474BB013BD973228D430890"
                    + "F6C30775113BC1770D"
            && InstructionEvidenceMatches());

        bool allIndicesUseNativeTableAddresses = true;
        bool allIndicesReadTablesBeforeTheLengthGate = true;
        for (int index = byte.MinValue; index <= byte.MaxValue; index++)
        {
            byte al = (byte)index;
            uint firstAddress = unchecked(0x01BB7490u + ((uint)al * 8));
            uint secondAddress = unchecked(0x01BB7494u + ((uint)al * 8));
            PayloadXorTableSetupPlan plan = Project(
                retainedEbx: 0x1000,
                al: al,
                stackLength: 0,
                first: 0,
                second: 0);
            int firstRead = IndexOf(
                plan.Actions,
                PayloadXorTableSetupActionKind.ReadFirstTableDword);
            int secondRead = IndexOf(
                plan.Actions,
                PayloadXorTableSetupActionKind.ReadSecondTableDword);
            int compare = IndexOf(
                plan.Actions,
                PayloadXorTableSetupActionKind.CompareRetainedEbxWithEnd);
            allIndicesUseNativeTableAddresses &=
                PayloadXorTableSetupProjection.FirstTableDwordAddress(al) == firstAddress
                && PayloadXorTableSetupProjection.SecondTableDwordAddress(al) == secondAddress
                && plan.FirstTableDwordAddress == firstAddress
                && plan.SecondTableDwordAddress == secondAddress
                && plan.ZeroExtendedAl == index
                && plan.FirstTableDword == 0
                && plan.SecondTableDword == 0;
            allIndicesReadTablesBeforeTheLengthGate &=
                plan.Outcome == PayloadXorTableSetupOutcome.UnsignedStartAtOrBeyondEnd
                && plan.StackLengthRead
                && plan.ComputedEnd == 0x1000
                && firstRead >= 0
                && secondRead > firstRead
                && compare > secondRead
                && plan.Actions[firstRead].RawFileOffset
                    == PayloadXorTableSetupProjection.FirstTableReadRawFileOffset
                && plan.Actions[secondRead].RawFileOffset
                    == PayloadXorTableSetupProjection.SecondTableReadRawFileOffset;
        }

        check("All 256 AL values are zero-extended native table indices, including 4..255",
            allIndicesUseNativeTableAddresses);
        check("Both table reads precede the zero-length unsigned branch, even for native zero DWORDs",
            allIndicesReadTablesBeforeTheLengthGate);

        PayloadXorTableSetupPlan invalidBoundary = PayloadXorTableSetupProjection.Project(
            new(false, 0x1000, 0x2000, 0xFF, 0, 0x11223344, 0x55667788));
        PayloadXorTableSetupPlan missingStack = PayloadXorTableSetupProjection.Project(
            new(true, 0xFFFFFFF8, 0x12345678, 0xFF, null, 0, 0));
        PayloadXorTableSetupPlan missingFirst = PayloadXorTableSetupProjection.Project(
            new(true, 0x2000, 0x3000, 0xFF, 0, null, 0));
        PayloadXorTableSetupPlan missingSecond = PayloadXorTableSetupProjection.Project(
            new(true, 0x2000, 0x3000, 0xFF, 0, 0xA1B2C3D4, null));
        check("Boundary validation and strict missing/unreached captures stop at the required read",
            invalidBoundary.Outcome
                == PayloadXorTableSetupOutcome.EntryBoundaryNotValidated
            && !invalidBoundary.StackLengthRead
            && invalidBoundary.UnreachedStackLengthCaptureProvided
            && invalidBoundary.UnreachedFirstTableDwordCaptureProvided
            && invalidBoundary.UnreachedSecondTableDwordCaptureProvided
            && invalidBoundary.Actions.Length == 0
            && missingStack.Outcome == PayloadXorTableSetupOutcome.UnreachedCaptureProvided
            && missingStack.StackLengthAddress == 0
            && !missingStack.StackLengthRead
            && missingStack.UnreachedFirstTableDwordCaptureProvided
            && missingStack.UnreachedSecondTableDwordCaptureProvided
            && missingStack.StopAtRawFileOffsetExclusive
                == PayloadXorTableSetupProjection.StackLengthReadRawFileOffset
            && missingFirst.Outcome
                == PayloadXorTableSetupOutcome.UnreachedCaptureProvided
            && !missingFirst.FirstTableDwordRead
            && missingFirst.FirstTableDwordAddress
                == PayloadXorTableSetupProjection.FirstTableDwordAddress(0xFF)
            && missingFirst.UnreachedSecondTableDwordCaptureProvided
            && missingFirst.StopAtRawFileOffsetExclusive
                == PayloadXorTableSetupProjection.FirstTableReadRawFileOffset
            && missingSecond.Outcome
                == PayloadXorTableSetupOutcome.MissingSecondTableDword
            && missingSecond.FirstTableDwordRead
            && missingSecond.FirstTableDword == 0xA1B2C3D4
            && missingSecond.SecondTableDwordAddress
                == PayloadXorTableSetupProjection.SecondTableDwordAddress(0xFF)
            && !missingSecond.SecondTableDwordRead
            && missingSecond.StopAtRawFileOffsetExclusive
                == PayloadXorTableSetupProjection.SecondTableReadRawFileOffset);

        PayloadXorTableSetupPlan zeroLength = Project(
            retainedEbx: 0x1000,
            al: 0,
            stackLength: 0,
            first: 0,
            second: 0);
        PayloadXorTableSetupPlan wrappedEnd = Project(
            retainedEbx: 0xFFFFFFFC,
            al: 2,
            stackLength: 8,
            first: 0xFFFFFFFF,
            second: 0);
        PayloadXorTableSetupPlan unsignedHighAddresses = Project(
            retainedEbx: 0x80000000,
            al: 3,
            stackLength: 8,
            first: 1,
            second: 2);
        check("The end addition and guards use exact DWORD wrap and unsigned comparisons",
            zeroLength.Outcome
                == PayloadXorTableSetupOutcome.UnsignedStartAtOrBeyondEnd
            && zeroLength.ComputedEnd == 0x1000
            && zeroLength.FirstTableDwordRead
            && zeroLength.SecondTableDwordRead
            && wrappedEnd.ComputedEnd == 4
            && wrappedEnd.Outcome
                == PayloadXorTableSetupOutcome.UnsignedStartAtOrBeyondEnd
            && wrappedEnd.SelectedBranchTargetRawFileOffset
                == PayloadXorTableSetupProjection.UnsignedStartBranchTargetRawFileOffset
            && unsignedHighAddresses.Outcome
                == PayloadXorTableSetupOutcome.EightByteSpanWithinEnd
            && unsignedHighAddresses.ComputedEnd == 0x80000008
            && unsignedHighAddresses.PlusEightAddress == 0x80000008
            && unsignedHighAddresses.RetainedEbxIsEightByteAligned == true);

        PayloadXorTableSetupPlan exactEightBytes = Project(
            retainedEbx: 0x1000,
            al: 1,
            stackLength: 8,
            first: 0,
            second: 0);
        PayloadXorTableSetupPlan shortTail = Project(
            retainedEbx: 0x1000,
            al: 1,
            stackLength: 7,
            first: 0,
            second: 0);
        PayloadXorTableSetupPlan unaligned = Project(
            retainedEbx: 0x1001,
            al: 1,
            stackLength: 16,
            first: 0,
            second: 0);
        check("Aligned eight-byte exact spans pass, short tails branch, and unaligned starts bypass the span compare",
            exactEightBytes.Outcome
                == PayloadXorTableSetupOutcome.EightByteSpanWithinEnd
            && exactEightBytes.PlusEightAddress == exactEightBytes.ComputedEnd
            && exactEightBytes.StopAtRawFileOffsetExclusive
                == PayloadXorTableSetupProjection.EntryEndRawFileOffsetExclusive
            && exactEightBytes.Actions[^1].Kind
                == PayloadXorTableSetupActionKind.StopBeforeXorBody
            && shortTail.Outcome
                == PayloadXorTableSetupOutcome.EightByteSpanExceedsEnd
            && shortTail.SelectedBranchTargetRawFileOffset
                == PayloadXorTableSetupProjection.AlternateBranchTargetRawFileOffset
            && unaligned.Outcome == PayloadXorTableSetupOutcome.UnalignedStart
            && unaligned.PlusEightAddress == 0x1009
            && unaligned.RetainedEbxIsEightByteAligned == false
            && unaligned.SelectedBranchTargetRawFileOffset
                == PayloadXorTableSetupProjection.AlternateBranchTargetRawFileOffset
            && !unaligned.Actions.Any(action =>
                action.Kind is PayloadXorTableSetupActionKind.ComparePlus8WithEnd
                    or PayloadXorTableSetupActionKind.BranchIfPlus8AboveEnd));

        PayloadXorTableSetupPlan wrappedFrameAddress = Project(
            framePointer: 0xFFFFFFF8,
            retainedEbx: 0x1000,
            al: 255,
            stackLength: 8,
            first: 0,
            second: 0);
        check("Frame+8, table indexing, and EBX+8 use DWORD address arithmetic without marker validation",
            PayloadXorTableSetupProjection.StackLengthAddress(0xFFFFFFF8) == 0
            && wrappedFrameAddress.StackLengthAddress == 0
            && PayloadXorTableSetupProjection.FirstTableDwordAddress(255)
                == unchecked(0x01BB7490u + 255u * 8)
            && PayloadXorTableSetupProjection.SecondTableDwordAddress(255)
                == unchecked(0x01BB7494u + 255u * 8)
            && PayloadXorTableSetupProjection.TableDwordAddress(uint.MaxValue, 255)
                == 0x7F7
            && PayloadXorTableSetupProjection.ComputeEnd(uint.MaxValue - 3, 8) == 4
            && PayloadXorTableSetupProjection.ComputePlusEight(uint.MaxValue - 3) == 4
            && wrappedFrameAddress.Outcome
                == PayloadXorTableSetupOutcome.EightByteSpanWithinEnd
            && wrappedFrameAddress.ZeroExtendedAl == 255);
    }

    private static PayloadXorTableSetupPlan Project(
        uint framePointer = 0x2000,
        uint retainedEbx = 0x1000,
        byte al = 1,
        uint? stackLength = 8,
        uint? first = 0,
        uint? second = 0) =>
        PayloadXorTableSetupProjection.Project(new(
            true,
            framePointer,
            retainedEbx,
            al,
            stackLength,
            first,
            second));

    private static int IndexOf(
        PayloadXorTableSetupAction[] actions,
        PayloadXorTableSetupActionKind kind) =>
        Array.FindIndex(actions, action => action.Kind == kind);

    private static bool InstructionEvidenceMatches()
    {
        uint nextRawOffset = PayloadXorTableSetupProjection.EntryStartRawFileOffset;
        string instructionBytes = string.Empty;
        foreach (PayloadXorTableSetupInstructionEvidence instruction
            in PayloadXorTableSetupProjection.Instructions)
        {
            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != PayloadXorTableSetupProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != PayloadXorTableSetupProjection.RawFileOffsetToVirtualAddress(
                            targetRaw))
            {
                return false;
            }

            instructionBytes += bytes;
            nextRawOffset += (uint)(bytes.Length / 2);
        }

        return nextRawOffset
                == PayloadXorTableSetupProjection.EntryEndRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(
                    PayloadXorTableSetupProjection.CapturedCodeBytes
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
