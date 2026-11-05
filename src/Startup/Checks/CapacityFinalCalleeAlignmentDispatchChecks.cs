using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlignmentDispatchChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window, instruction order, and three branch targets match",
            CapacityFinalCalleeAlignmentDispatchProjection.EntryRawFileOffset == 0x012C8DA7
            && CapacityFinalCalleeAlignmentDispatchProjection.EntryVirtualAddress
                == 0x016C99A7
            && CapacityFinalCalleeAlignmentDispatchProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8DED
            && CapacityFinalCalleeAlignmentDispatchProjection.BoundaryVirtualAddressExclusive
                == 0x016C99ED
            && CapacityFinalCalleeAlignmentDispatchProjection.EntryAlBranchDestinationRawFileOffset
                == 0x012C8EF8
            && CapacityFinalCalleeAlignmentDispatchProjection.EntryAlBranchDestinationVirtualAddress
                == 0x016C9AF8
            && CapacityFinalCalleeAlignmentDispatchProjection.EntryEcxBranchDestinationRawFileOffset
                == 0x012C8E45
            && CapacityFinalCalleeAlignmentDispatchProjection.EntryEcxBranchDestinationVirtualAddress
                == 0x016C9A45
            && CapacityFinalCalleeAlignmentDispatchProjection.ComputedEcxZeroBranchDestinationRawFileOffset
                == 0x012C8E3A
            && CapacityFinalCalleeAlignmentDispatchProjection.ComputedEcxZeroBranchDestinationVirtualAddress
                == 0x016C9A3A
            && CapacityFinalCalleeAlignmentDispatchProjection.GlobalDispatchCellVirtualAddress
                == 0x01AC42B4
            && CapacityFinalCalleeAlignmentDispatchProjection.CallCellVirtualAddress
                == 0x01AC42A8
            && CapacityFinalCalleeAlignmentDispatchProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlignmentDispatchProjection.EntryRawFileOffset)
                == CapacityFinalCalleeAlignmentDispatchProjection.EntryVirtualAddress
            && CapacityFinalCalleeAlignmentDispatchProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlignmentDispatchProjection.BoundaryRawFileOffsetExclusive)
                == CapacityFinalCalleeAlignmentDispatchProjection.BoundaryVirtualAddressExclusive
            && Normalize(CapacityFinalCalleeAlignmentDispatchProjection.CapturedWindowBytes)
                == "8BF8C745F80100000083E7F88D14378955F4A8010F85370100008B062BF085C9757C"
                    + "8D481003CF894DF08BF985C974638B1DB442AC018D49006A1C8D45D05056FF15A842AC01"
            && CapacityFinalCalleeAlignmentDispatchProjection.Instructions.Sum(
                instruction => instruction.Bytes.Split(
                    ' ', StringSplitOptions.RemoveEmptyEntries).Length) == 70
            && InstructionEvidenceMatches()
            && ReadDescriptorsMatch());

        CapacityFinalCalleeAlignmentDispatchPlan invalidEntry =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                captureValidatedEntry: false,
                entryEax: 0x12345679,
                entryEsi: 0xFFFFFFFF,
                entryEcx: 0,
                entryEbp: 0,
                entryEsp: 0,
                entryEsiDword: 0,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("An unvalidated entry executes no writes, reads, or pushes",
            invalidEntry.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryNotValidated
            && invalidEntry.Reads.IsEmpty
            && invalidEntry.FrameWrites.IsEmpty
            && invalidEntry.Pushes.IsEmpty
            && !invalidEntry.TestAlReached
            && !invalidEntry.EntryEsiReadReached
            && !invalidEntry.GlobalDispatchReadReached
            && !invalidEntry.CallCellReadReached
            && !invalidEntry.CallReached
            && invalidEntry.UnreachedEntryEsiDwordCaptureProvided
            && invalidEntry.UnreachedGlobalDispatchDwordCaptureProvided
            && invalidEntry.UnreachedCallCellTargetCaptureProvided
            && invalidEntry.UnreachedCallReturnStatusProvided
            && invalidEntry.EAX == 0x12345679
            && invalidEntry.ESI == 0xFFFFFFFF
            && invalidEntry.ECX == 0
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C8DA7);

        CapacityFinalCalleeAlignmentDispatchPlan alLowBitSet =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                entryEax: 0xFFFFFFFF,
                entryEsi: 0x10,
                entryEbp: 0,
                entryEsiDword: 0,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("The AL-bit branch preserves the earlier aligned-register calculation and frame stores",
            alLowBitSet.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryAlLowBitSet
            && alLowBitSet.EDI == 0xFFFFFFF8
            && alLowBitSet.EDX == 8
            && alLowBitSet.TestAlFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && alLowBitSet.TestAlBranchReached
            && alLowBitSet.TestAlBranchTaken == true
            && alLowBitSet.TestAlSelectedDestinationRawFileOffset == 0x012C8EF8
            && alLowBitSet.TestAlSelectedDestinationVirtualAddress == 0x016C9AF8
            && alLowBitSet.FrameWrites.SequenceEqual(new[]
            {
                new CapacityFinalCalleeAlignmentDispatchWrite(0x012C8DA9, 0xFFFFFFF8, 1),
                new CapacityFinalCalleeAlignmentDispatchWrite(0x012C8DB6, 0xFFFFFFF4, 8)
            })
            && alLowBitSet.Reads.IsEmpty
            && !alLowBitSet.EntryEsiReadReached
            && !alLowBitSet.TestEntryEcxReached
            && alLowBitSet.UnreachedEntryEsiDwordCaptureProvided
            && alLowBitSet.UnreachedGlobalDispatchDwordCaptureProvided
            && alLowBitSet.UnreachedCallCellTargetCaptureProvided
            && alLowBitSet.UnreachedCallReturnStatusProvided
            && alLowBitSet.StopAtRawFileOffsetExclusive == 0x012C8EF8);

        CapacityFinalCalleeAlignmentDispatchPlan missingEntryEsi =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                entryEax: 0x1000,
                entryEsi: 0x2000,
                entryEsiDword: null,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        CapacityFinalCalleeAlignmentDispatchPlan zeroEntryEsi =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                entryEax: 0x1000,
                entryEsi: 0x2000,
                entryEsiDword: 0,
                entryEcx: 1,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("Missing and captured-zero ESI reads remain distinct and later captures cannot bypass the read",
            missingEntryEsi.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.MissingEntryEsiDword
            && missingEntryEsi.Reads.SequenceEqual(new[]
            {
                new CapacityFinalCalleeAlignmentDispatchRead(
                    0x012C8DC1,
                    CapacityFinalCalleeAlignmentDispatchReadKind.EntryEsiDword,
                    0x2000,
                    false,
                    null)
            })
            && missingEntryEsi.EntryEsiReadReached
            && !missingEntryEsi.EntryEsiDwordCaptured
            && missingEntryEsi.EntryEsiDword is null
            && missingEntryEsi.EAX == 0x1000
            && missingEntryEsi.ESI == 0x2000
            && !missingEntryEsi.TestEntryEcxReached
            && missingEntryEsi.UnreachedGlobalDispatchDwordCaptureProvided
            && missingEntryEsi.UnreachedCallCellTargetCaptureProvided
            && missingEntryEsi.UnreachedCallReturnStatusProvided
            && missingEntryEsi.StopAtRawFileOffsetExclusive == 0x012C8DC1
            && zeroEntryEsi.EntryEsiReadReached
            && zeroEntryEsi.EntryEsiDwordCaptured
            && zeroEntryEsi.EntryEsiDword == 0
            && zeroEntryEsi.Reads[0].ValueCaptured
            && zeroEntryEsi.Reads[0].Value == 0
            && zeroEntryEsi.EAX == 0
            && zeroEntryEsi.ESI == 0x2000);

        CapacityFinalCalleeAlignmentDispatchPlan nonzeroEntryEcx =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                entryEax: 0x20,
                entryEsi: 0x100,
                entryEcx: 0x80000000,
                entryEsiDword: 0,
                globalDispatchDword: 0x1234,
                callCellTarget: 0x5678,
                callReturnedNormally: true));
        check("A nonzero entry ECX takes the second branch after the ESI read and subtraction",
            nonzeroEntryEcx.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.EntryEcxNonzero
            && nonzeroEntryEcx.TestEntryEcxFlags is { Zero: false }
            && nonzeroEntryEcx.TestEntryEcxBranchReached
            && nonzeroEntryEcx.TestEntryEcxBranchTaken == true
            && nonzeroEntryEcx.TestEntryEcxSelectedDestinationRawFileOffset == 0x012C8E45
            && nonzeroEntryEcx.TestEntryEcxSelectedDestinationVirtualAddress == 0x016C9A45
            && nonzeroEntryEcx.EAX == 0
            && nonzeroEntryEcx.ESI == 0x100
            && nonzeroEntryEcx.ECX == 0x80000000
            && !nonzeroEntryEcx.TestComputedEcxReached
            && !nonzeroEntryEcx.GlobalDispatchReadReached
            && nonzeroEntryEcx.UnreachedGlobalDispatchDwordCaptureProvided
            && nonzeroEntryEcx.UnreachedCallCellTargetCaptureProvided
            && nonzeroEntryEcx.UnreachedCallReturnStatusProvided
            && nonzeroEntryEcx.StopAtRawFileOffsetExclusive == 0x012C8E45);

        CapacityFinalCalleeAlignmentDispatchPlan computedEcx =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                entryEax: 0,
                entryEsi: 0x200,
                entryEcx: 0,
                entryEsiDword: 0xFFFFFFF0,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("Computed zero ECX takes the third branch after its persistent frame write",
            computedEcx.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.ComputedEcxZero
            && computedEcx.ESI == 0x210
            && computedEcx.EAX == 0xFFFFFFF0
            && computedEcx.ECX == 0
            && computedEcx.EDI == 0
            && computedEcx.TestComputedEcxFlags is { Zero: true }
            && computedEcx.TestComputedEcxBranchReached
            && computedEcx.TestComputedEcxBranchTaken == true
            && computedEcx.TestComputedEcxSelectedDestinationRawFileOffset == 0x012C8E3A
            && computedEcx.TestComputedEcxSelectedDestinationVirtualAddress == 0x016C9A3A
            && computedEcx.FrameWrites[^1]
                == new CapacityFinalCalleeAlignmentDispatchWrite(0x012C8DCE, 0x1FF0, 0)
            && !computedEcx.GlobalDispatchReadReached
            && computedEcx.UnreachedGlobalDispatchDwordCaptureProvided
            && computedEcx.UnreachedCallCellTargetCaptureProvided
            && computedEcx.UnreachedCallReturnStatusProvided
            && computedEcx.StopAtRawFileOffsetExclusive == 0x012C8E3A);

        CapacityFinalCalleeAlignmentDispatchPlan missingGlobal =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                entryEax: 0x18,
                entryEsi: 0x1000,
                entryEcx: 0,
                entryEsiDword: 0x20,
                globalDispatchDword: null,
                callCellTarget: 0,
                callReturnedNormally: true));
        CapacityFinalCalleeAlignmentDispatchPlan zeroGlobal =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                entryEax: 0x18,
                entryEsi: 0x1000,
                entryEcx: 0,
                entryEsiDword: 0x20,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("The dispatch-cell read is ordered after both untaken branches and zero is a captured value",
            missingGlobal.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.MissingGlobalDispatchDword
            && missingGlobal.Reads.Length == 2
            && missingGlobal.Reads[1] == new CapacityFinalCalleeAlignmentDispatchRead(
                0x012C8DD7,
                CapacityFinalCalleeAlignmentDispatchReadKind.GlobalDispatchDword,
                0x01AC42B4,
                false,
                null)
            && missingGlobal.TestEntryEcxBranchTaken == false
            && missingGlobal.TestComputedEcxBranchTaken == false
            && missingGlobal.ECX == 0x48
            && missingGlobal.EDI == 0x48
            && missingGlobal.FrameWrites[^1].Value == 0x48
            && missingGlobal.UnreachedCallCellTargetCaptureProvided
            && missingGlobal.UnreachedCallReturnStatusProvided
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C8DD7
            && zeroGlobal.GlobalDispatchReadReached
            && zeroGlobal.GlobalDispatchDwordCaptured
            && zeroGlobal.GlobalDispatchDword == 0
            && zeroGlobal.Reads[1].ValueCaptured
            && zeroGlobal.Reads[1].Value == 0
            && zeroGlobal.EBX == 0);

        CapacityFinalCalleeAlignmentDispatchPlan missingCallCell =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                entryEax: 0x18,
                entryEsi: 0x1000,
                entryEcx: 0,
                entryEbp: 0x10,
                entryEsp: 2,
                entryEsiDword: 0x20,
                globalDispatchDword: 0,
                callCellTarget: null,
                callReturnedNormally: true));
        CapacityFinalCalleeAlignmentDispatchPlan missingReturnStatus =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                entryEax: 0x18,
                entryEsi: 0x1000,
                entryEcx: 0,
                entryEbp: 0x10,
                entryEsp: 2,
                entryEsiDword: 0x20,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: null));
        CapacityFinalCalleeAlignmentDispatchPlan callDidNotReturn =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                entryEax: 0x18,
                entryEsi: 0x1000,
                entryEcx: 0,
                entryEbp: 0x10,
                entryEsp: 2,
                entryEsiDword: 0x20,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: false));
        CapacityFinalCalleeAlignmentDispatchPlan callReturned =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                entryEax: 0x18,
                entryEsi: 0x1000,
                entryEcx: 0,
                entryEbp: 0x10,
                entryEsp: 2,
                entryEsiDword: 0x20,
                globalDispatchDword: 0,
                callCellTarget: 0,
                callReturnedNormally: true));
        check("Ordered pushes precede the call-cell read and both call-cell and return gates are required",
            missingCallCell.Pushes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeAlignmentDispatchPush(0x012C8DE0, 0xFFFFFFFE, 0x1C),
                new CapacityFinalCalleeAlignmentDispatchPush(0x012C8DE5, 0xFFFFFFFA, 0xFFFFFFE0),
                new CapacityFinalCalleeAlignmentDispatchPush(0x012C8DE6, 0xFFFFFFF6, 0xFE0)
            })
            && missingCallCell.ESP == 0xFFFFFFF6
            && missingCallCell.EAX == 0xFFFFFFE0
            && missingCallCell.FrameWrites[^1]
                == new CapacityFinalCalleeAlignmentDispatchWrite(0x012C8DCE, 0, 0x48)
            && missingCallCell.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.MissingCallCellTarget
            && missingCallCell.Reads.Length == 3
            && missingCallCell.Reads[2] == new CapacityFinalCalleeAlignmentDispatchRead(
                0x012C8DE7,
                CapacityFinalCalleeAlignmentDispatchReadKind.CallCellTargetVirtualAddress,
                0x01AC42A8,
                false,
                null)
            && missingCallCell.CallCellReadReached
            && !missingCallCell.CallCellTargetCaptured
            && !missingCallCell.CallReached
            && missingCallCell.UnreachedCallReturnStatusProvided
            && missingCallCell.StopAtRawFileOffsetExclusive == 0x012C8DE7
            && missingReturnStatus.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.MissingCallReturnStatus
            && missingReturnStatus.CallCellTargetCaptured
            && missingReturnStatus.CallCellTargetVirtualAddress == 0
            && missingReturnStatus.CallReached
            && missingReturnStatus.CallReturnedNormally is null
            && missingReturnStatus.StopAtRawFileOffsetExclusive == 0x012C8DE7
            && callDidNotReturn.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.CallDidNotReturnNormally
            && callDidNotReturn.CallReached
            && callDidNotReturn.CallReturnedNormally == false
            && callDidNotReturn.StopAtRawFileOffsetExclusive == 0x012C8DE7
            && callReturned.Outcome
                == CapacityFinalCalleeAlignmentDispatchOutcome.CallReturnedNormally
            && callReturned.CallCellTargetCaptured
            && callReturned.CallCellTargetVirtualAddress == 0
            && callReturned.CallReached
            && callReturned.CallReturnedNormally == true
            && callReturned.CallRegisterOutputsUnknown
            && callReturned.StopAtRawFileOffsetExclusive == 0x012C8DED);

        CapacityFinalCalleeAlignmentDispatchPlan wrapped =
            CapacityFinalCalleeAlignmentDispatchProjection.Project(Capture(
                entryEax: 8,
                entryEsi: uint.MaxValue,
                entryEcx: 0,
                entryEbp: 7,
                entryEsp: 1,
                entryEsiDword: 1,
                globalDispatchDword: 0,
                callCellTarget: 0x12345678,
                callReturnedNormally: true));
        check("DWORD effective addresses, additions, subtraction, and stack pushes wrap independently",
            CapacityFinalCalleeAlignmentDispatchProjection.EntryEsiAddress(uint.MaxValue)
                == uint.MaxValue
            && CapacityFinalCalleeAlignmentDispatchProjection.FrameLocalAddress(7, 8)
                == 0xFFFFFFFF
            && wrapped.FrameWrites[0]
                == new CapacityFinalCalleeAlignmentDispatchWrite(0x012C8DA9, 0xFFFFFFFF, 1)
            && wrapped.FrameWrites[1]
                == new CapacityFinalCalleeAlignmentDispatchWrite(0x012C8DB6, 0xFFFFFFFB, 7)
            && wrapped.EDX == 7
            && wrapped.ESI == 0xFFFFFFFE
            && wrapped.EAX == 0xFFFFFFD7
            && wrapped.ESP == 0xFFFFFFF5
            && wrapped.Pushes.Select(push => push.StackAddress)
                .SequenceEqual(new[] { 0xFFFFFFFDu, 0xFFFFFFF9u, 0xFFFFFFF5u })
            && wrapped.Pushes[^1].Value == 0xFFFFFFFE
            && wrapped.CallReturnedNormally == true);
    }

    private static CapacityFinalCalleeAlignmentDispatchCapture Capture(
        bool captureValidatedEntry = true,
        uint entryEax = 0x18,
        uint entryEsi = 0x1000,
        uint entryEcx = 0,
        uint entryEbp = 0x2000,
        uint entryEsp = 0x3000,
        uint? entryEsiDword = 0x20,
        uint? globalDispatchDword = 0,
        uint? callCellTarget = 0x100000,
        bool? callReturnedNormally = true) =>
        new(
            captureValidatedEntry,
            entryEax,
            entryEsi,
            entryEcx,
            entryEbp,
            entryEsp,
            entryEsiDword,
            globalDispatchDword,
            callCellTarget,
            callReturnedNormally);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            byte? ImmediateValue,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C8DA7, "8B F8", null, null, null, null),
            (0x012C8DA9, "C7 45 F8 01 00 00 00", null, 1, null, null),
            (0x012C8DB0, "83 E7 F8", null, 0xF8, null, null),
            (0x012C8DB3, "8D 14 37", null, null, null, null),
            (0x012C8DB6, "89 55 F4", null, null, null, null),
            (0x012C8DB9, "A8 01", null, 1, null, null),
            (0x012C8DBB, "0F 85 37 01 00 00", null, null, 0x012C8EF8, 0x016C9AF8),
            (0x012C8DC1, "8B 06", null, null, null, null),
            (0x012C8DC3, "2B F0", null, null, null, null),
            (0x012C8DC5, "85 C9", null, null, null, null),
            (0x012C8DC7, "75 7C", null, null, 0x012C8E45, 0x016C9A45),
            (0x012C8DC9, "8D 48 10", null, null, null, null),
            (0x012C8DCC, "03 CF", null, null, null, null),
            (0x012C8DCE, "89 4D F0", null, null, null, null),
            (0x012C8DD1, "8B F9", null, null, null, null),
            (0x012C8DD3, "85 C9", null, null, null, null),
            (0x012C8DD5, "74 63", null, null, 0x012C8E3A, 0x016C9A3A),
            (0x012C8DD7, "8B 1D B4 42 AC 01", 0x01AC42B4, null, null, null),
            (0x012C8DDD, "8D 49 00", null, null, null, null),
            (0x012C8DE0, "6A 1C", null, 0x1C, null, null),
            (0x012C8DE2, "8D 45 D0", null, null, null, null),
            (0x012C8DE5, "50", null, null, null, null),
            (0x012C8DE6, "56", null, null, null, null),
            (0x012C8DE7, "FF 15 A8 42 AC 01", 0x01AC42A8, null, null, null)
        };
        uint nextRawOffset = CapacityFinalCalleeAlignmentDispatchProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlignmentDispatchInstructionEvidence instruction
            in CapacityFinalCalleeAlignmentDispatchProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlignmentDispatchProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlignmentDispatchProjection.RawFileOffsetToVirtualAddress(
                            targetRaw))
            )
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeAlignmentDispatchProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlignmentDispatchProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlignmentDispatchProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.PeVirtualAddressOperand,
                        instruction.ImmediateValue,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static bool ReadDescriptorsMatch()
    {
        var expected = new[]
        {
            new CapacityFinalCalleeAlignmentDispatchReadDescriptor(
                0x012C8DC1,
                CapacityFinalCalleeAlignmentDispatchReadKind.EntryEsiDword,
                null,
                "ESI",
                0),
            new CapacityFinalCalleeAlignmentDispatchReadDescriptor(
                0x012C8DD7,
                CapacityFinalCalleeAlignmentDispatchReadKind.GlobalDispatchDword,
                0x01AC42B4,
                null,
                0),
            new CapacityFinalCalleeAlignmentDispatchReadDescriptor(
                0x012C8DE7,
                CapacityFinalCalleeAlignmentDispatchReadKind.CallCellTargetVirtualAddress,
                0x01AC42A8,
                null,
                0)
        };
        return CapacityFinalCalleeAlignmentDispatchProjection.OrderedReadDescriptors
            .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
