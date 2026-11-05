using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeTagEqualityContinuationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window, PE mapping, read descriptors, and branch/call targets match",
            CapacityFinalCalleeTagEqualityContinuationProjection.EntryRawFileOffset
                == 0x012C8F21
            && CapacityFinalCalleeTagEqualityContinuationProjection.EntryVirtualAddress
                == 0x016C9B21
            && CapacityFinalCalleeTagEqualityContinuationProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8F71
            && CapacityFinalCalleeTagEqualityContinuationProjection.BoundaryVirtualAddressExclusive
                == 0x016C9B71
            && CapacityFinalCalleeTagEqualityContinuationProjection.InitialGlobalDwordVirtualAddress
                == 0x01EDADD4
            && CapacityFinalCalleeTagEqualityContinuationProjection.EntryEsiPublicationVirtualAddress
                == 0x01EDADE0
            && CapacityFinalCalleeTagEqualityContinuationProjection.FreshTagDwordVirtualAddress
                == 0x01EDADDC
            && CapacityFinalCalleeTagEqualityContinuationProjection.SecondaryTagVirtualAddress
                == 0x01EDADD0
            && CapacityFinalCalleeTagEqualityContinuationProjection.CapacityDwordVirtualAddress
                == 0x01EDADE4
            && CapacityFinalCalleeTagEqualityContinuationProjection.CalleeVirtualAddress
                == 0x01EDADC8
            && CapacityFinalCalleeTagEqualityContinuationProjection.CapacityBranchDestinationRawFileOffset
                == 0x012C9441
            && CapacityFinalCalleeTagEqualityContinuationProjection.CapacityBranchDestinationVirtualAddress
                == 0x016CA041
            && CapacityFinalCalleeTagEqualityContinuationProjection.DirectCallTargetRawFileOffset
                == 0x012C7BF0
            && CapacityFinalCalleeTagEqualityContinuationProjection.DirectCallTargetVirtualAddress
                == 0x016C87F0
            && CapacityFinalCalleeTagEqualityContinuationProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeTagEqualityContinuationProjection.EntryRawFileOffset)
                == CapacityFinalCalleeTagEqualityContinuationProjection.EntryVirtualAddress
            && CapacityFinalCalleeTagEqualityContinuationProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeTagEqualityContinuationProjection.BoundaryRawFileOffsetExclusive)
                == CapacityFinalCalleeTagEqualityContinuationProjection.BoundaryVirtualAddressExclusive
            && Normalize(CapacityFinalCalleeTagEqualityContinuationProjection.CapturedWindowBytes)
                == "8B0DD4ADED0103CF8935E0ADED018BC1890DD4ADED0183C801894604"
                    + "3B35DCADED017514C705DCADED0100000000C705D0ADED0100000000"
                    + "3B0DE4ADED010F86DC04000033D2B9C8ADED01E87FECFFFF"
            && CapacityFinalCalleeTagEqualityContinuationProjection.Instructions.Sum(
                instruction => instruction.Bytes.Split(
                    ' ', StringSplitOptions.RemoveEmptyEntries).Length) == 80
            && InstructionEvidenceMatches()
            && ReadDescriptorsMatch());

        CapacityFinalCalleeTagEqualityContinuationPlan invalidEntry =
            CapacityFinalCalleeTagEqualityContinuationProjection.Project(Capture(
                captureValidatedEntry: false,
                entryEsi: 0xFFFFFFFF,
                entryEdi: 0xFFFFFFFF,
                entryEsp: 0x1000,
                initialGlobalDword: 0,
                freshTagDword: 0,
                capacityDword: 0,
                callReturnedNormally: true));
        check("An unvalidated entry executes no reads or writes and marks every supplied capture unreached",
            invalidEntry.Outcome
                == CapacityFinalCalleeTagEqualityContinuationOutcome.EntryNotValidated
            && invalidEntry.Reads.IsEmpty
            && invalidEntry.Writes.IsEmpty
            && invalidEntry.ESI == 0xFFFFFFFF
            && invalidEntry.EDI == 0xFFFFFFFF
            && invalidEntry.ESP == 0x1000
            && !invalidEntry.InitialGlobalReadReached
            && !invalidEntry.TagComparisonReadReached
            && !invalidEntry.CapacityReadReached
            && !invalidEntry.CallReached
            && invalidEntry.UnreachedInitialGlobalDwordCaptureProvided
            && invalidEntry.UnreachedFreshTagDwordCaptureProvided
            && invalidEntry.UnreachedCapacityDwordCaptureProvided
            && invalidEntry.UnreachedCallReturnStatusProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C8F21);

        CapacityFinalCalleeTagEqualityContinuationPlan missingInitial =
            CapacityFinalCalleeTagEqualityContinuationProjection.Project(Capture(
                initialGlobalDword: null,
                freshTagDword: 0,
                capacityDword: 1,
                callReturnedNormally: false));
        check("A missing initial read stops before all stores while later supplied captures remain unreached",
            missingInitial.Outcome
                == CapacityFinalCalleeTagEqualityContinuationOutcome.MissingInitialGlobalDword
            && missingInitial.Reads.SequenceEqual(new[]
            {
                new CapacityFinalCalleeTagEqualityContinuationRead(
                    0x012C8F21,
                    CapacityFinalCalleeTagEqualityContinuationReadKind.InitialGlobalDword,
                    0x01EDADD4,
                    false,
                    null)
            })
            && missingInitial.InitialGlobalReadReached
            && !missingInitial.InitialGlobalDwordCaptured
            && missingInitial.Writes.IsEmpty
            && !missingInitial.ComputedEcxReached
            && missingInitial.UnreachedFreshTagDwordCaptureProvided
            && missingInitial.UnreachedCapacityDwordCaptureProvided
            && missingInitial.UnreachedCallReturnStatusProvided
            && missingInitial.StopAtRawFileOffsetExclusive == 0x012C8F21);

        CapacityFinalCalleeTagEqualityContinuationPlan missingFresh =
            CapacityFinalCalleeTagEqualityContinuationProjection.Project(Capture(
                entryEsi: 0x1234,
                entryEdi: 0x20,
                initialGlobalDword: 0xFFFFFFF0,
                freshTagDword: null,
                capacityDword: 0,
                callReturnedNormally: true));
        check("A missing fresh tag read preserves the three preceding writes and stops before comparison",
            missingFresh.Outcome
                == CapacityFinalCalleeTagEqualityContinuationOutcome.MissingFreshTagDword
            && missingFresh.ComputedEcx == 0x10
            && missingFresh.EAX == 0x11
            && missingFresh.InitialGlobalDwordCaptured
            && missingFresh.TagComparisonReadReached
            && !missingFresh.FreshTagDwordCaptured
            && !missingFresh.TagComparisonReached
            && missingFresh.Reads.Length == 2
            && missingFresh.Reads[^1].Address == 0x01EDADDC
            && !missingFresh.Reads[^1].ValueCaptured
            && missingFresh.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeTagEqualityContinuationWrite(
                    0x012C8F29, 0x01EDADE0, 0x1234),
                new CapacityFinalCalleeTagEqualityContinuationWrite(
                    0x012C8F31, 0x01EDADD4, 0x10),
                new CapacityFinalCalleeTagEqualityContinuationWrite(
                    0x012C8F3A, 0x1238, 0x11)
            })
            && missingFresh.UnreachedCapacityDwordCaptureProvided
            && missingFresh.UnreachedCallReturnStatusProvided
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C8F3D);

        CapacityFinalCalleeTagEqualityContinuationPlan equalTagAtLimit =
            CapacityFinalCalleeTagEqualityContinuationProjection.Project(Capture(
                entryEsi: 0xFFFFFFFC,
                entryEdi: 4,
                initialGlobalDword: 0xFFFFFFFE,
                freshTagDword: 0xFFFFFFFC,
                capacityDword: 2,
                callReturnedNormally: true));
        check("DWORD arithmetic wraps, equality clears both tags, and unsigned equality takes JBE",
            equalTagAtLimit.Outcome
                == CapacityFinalCalleeTagEqualityContinuationOutcome.CapacityUnsignedBelowOrEqual
            && equalTagAtLimit.ComputedEcx == 2
            && equalTagAtLimit.EAX == 3
            && equalTagAtLimit.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeTagEqualityContinuationWrite(
                    0x012C8F29, 0x01EDADE0, 0xFFFFFFFC),
                new CapacityFinalCalleeTagEqualityContinuationWrite(
                    0x012C8F31, 0x01EDADD4, 2),
                new CapacityFinalCalleeTagEqualityContinuationWrite(
                    0x012C8F3A, 0, 3),
                new CapacityFinalCalleeTagEqualityContinuationWrite(
                    0x012C8F45, 0x01EDADDC, 0),
                new CapacityFinalCalleeTagEqualityContinuationWrite(
                    0x012C8F4F, 0x01EDADD0, 0)
            })
            && equalTagAtLimit.TagComparisonFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalTagAtLimit.EntryEsiEqualsFreshTagDword == true
            && equalTagAtLimit.TagNotEqualBranchTaken == false
            && equalTagAtLimit.TagClearWritesReached
            && equalTagAtLimit.CapacityComparisonFlags is
            {
                Carry: false,
                Zero: true
            }
            && equalTagAtLimit.CapacityJbeReached
            && equalTagAtLimit.CapacityJbeTaken == true
            && equalTagAtLimit.CapacityBranchDestinationRawFileOffset == 0x012C9441
            && equalTagAtLimit.CapacityBranchDestinationVirtualAddress == 0x016CA041
            && !equalTagAtLimit.CallReached
            && equalTagAtLimit.UnreachedCallReturnStatusProvided
            && equalTagAtLimit.StopAtRawFileOffsetExclusive == 0x012C9441);

        CapacityFinalCalleeTagEqualityContinuationPlan missingCapacity =
            CapacityFinalCalleeTagEqualityContinuationProjection.Project(Capture(
                entryEsi: 10,
                entryEdi: 5,
                initialGlobalDword: 7,
                freshTagDword: 10,
                capacityDword: null,
                callReturnedNormally: false));
        check("A missing capacity read retains earlier stores and the optional tag-clear branch",
            missingCapacity.Outcome
                == CapacityFinalCalleeTagEqualityContinuationOutcome.MissingCapacityDword
            && missingCapacity.Writes.Length == 5
            && missingCapacity.TagNotEqualBranchReached
            && missingCapacity.EntryEsiEqualsFreshTagDword == true
            && missingCapacity.TagClearWritesReached
            && missingCapacity.CapacityReadReached
            && !missingCapacity.CapacityDwordCaptured
            && !missingCapacity.CapacityComparisonReached
            && missingCapacity.Reads.Length == 3
            && !missingCapacity.Reads[^1].ValueCaptured
            && missingCapacity.UnreachedCallReturnStatusProvided
            && missingCapacity.StopAtRawFileOffsetExclusive == 0x012C8F59);

        CapacityFinalCalleeTagEqualityContinuationPlan unsignedHighCall =
            CapacityFinalCalleeTagEqualityContinuationProjection.Project(Capture(
                entryEsi: 1,
                entryEdi: 0x7FFFFFFF,
                initialGlobalDword: 1,
                freshTagDword: 2,
                capacityDword: 0x7FFFFFFF,
                callReturnedNormally: true));
        check("The unsigned-above comparison reaches the opaque direct call with explicit pre-call registers",
            unsignedHighCall.Outcome
                == CapacityFinalCalleeTagEqualityContinuationOutcome.CallReturnedNormally
            && unsignedHighCall.ComputedEcx == 0x80000000
            && unsignedHighCall.TagNotEqualBranchTaken == true
            && !unsignedHighCall.TagClearWritesReached
            && unsignedHighCall.Writes.Length == 3
            && unsignedHighCall.CapacityComparisonFlags is
            {
                Carry: false,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && unsignedHighCall.CapacityJbeReached
            && unsignedHighCall.CapacityJbeTaken == false
            && unsignedHighCall.CapacityBranchDestinationRawFileOffset is null
            && unsignedHighCall.CallReached
            && unsignedHighCall.CallReturnedNormally == true
            && unsignedHighCall.CallRegisterOutputsUnknown
            && unsignedHighCall.PreCallEAX == 0x80000001
            && unsignedHighCall.PreCallESI == 1
            && unsignedHighCall.PreCallEDI == 0x7FFFFFFF
            && unsignedHighCall.PreCallECX == 0x01EDADC8
            && unsignedHighCall.PreCallEDX == 0
            && unsignedHighCall.PreCallESP == 0x8000
            && !unsignedHighCall.UnreachedCallReturnStatusProvided
            && unsignedHighCall.StopAtRawFileOffsetExclusive == 0x012C8F71);

        CapacityFinalCalleeTagEqualityContinuationPlan capturedZeroReads =
            CapacityFinalCalleeTagEqualityContinuationProjection.Project(Capture(
                entryEsi: 1,
                entryEdi: 0,
                initialGlobalDword: 0,
                freshTagDword: 0,
                capacityDword: 0,
                callReturnedNormally: false));
        check("Captured-zero DWORD reads remain present and zero capacity equality takes JBE",
            capturedZeroReads.Outcome
                == CapacityFinalCalleeTagEqualityContinuationOutcome.CapacityUnsignedBelowOrEqual
            && capturedZeroReads.Reads.Length == 3
            && capturedZeroReads.Reads.All(read => read.ValueCaptured && read.Value == 0)
            && capturedZeroReads.InitialGlobalDwordCaptured
            && capturedZeroReads.ComputedEcx == 0
            && capturedZeroReads.FreshTagDwordCaptured
            && capturedZeroReads.EntryEsiEqualsFreshTagDword == false
            && capturedZeroReads.CapacityDwordCaptured
            && capturedZeroReads.CapacityComparisonFlags is { Zero: true }
            && capturedZeroReads.UnreachedCallReturnStatusProvided);

        CapacityFinalCalleeTagEqualityContinuationPlan missingCallStatus =
            CapacityFinalCalleeTagEqualityContinuationProjection.Project(Capture(
                entryEsi: 0,
                entryEdi: 1,
                initialGlobalDword: 1,
                freshTagDword: 2,
                capacityDword: 1,
                callReturnedNormally: null));
        CapacityFinalCalleeTagEqualityContinuationPlan nonreturningCall =
            CapacityFinalCalleeTagEqualityContinuationProjection.Project(Capture(
                entryEsi: 0,
                entryEdi: 1,
                initialGlobalDword: 1,
                freshTagDword: 2,
                capacityDword: 1,
                callReturnedNormally: false));
        check("The call gate distinguishes missing return evidence, a nonreturn, and a captured normal return",
            missingCallStatus.Outcome
                == CapacityFinalCalleeTagEqualityContinuationOutcome.MissingCallReturnStatus
            && missingCallStatus.CallReached
            && missingCallStatus.CallReturnedNormally is null
            && missingCallStatus.CallRegisterOutputsUnknown
            && missingCallStatus.PreCallEDX == 0
            && missingCallStatus.PreCallECX == 0x01EDADC8
            && missingCallStatus.StopAtRawFileOffsetExclusive == 0x012C8F6C
            && nonreturningCall.Outcome
                == CapacityFinalCalleeTagEqualityContinuationOutcome.CallDidNotReturnNormally
            && nonreturningCall.CallReturnedNormally == false
            && nonreturningCall.CallRegisterOutputsUnknown
            && nonreturningCall.StopAtRawFileOffsetExclusive == 0x012C8F6C);
    }

    private static CapacityFinalCalleeTagEqualityContinuationCapture Capture(
        bool captureValidatedEntry = true,
        uint entryEsi = 0,
        uint entryEdi = 0,
        uint entryEsp = 0x8000,
        uint? initialGlobalDword = 0,
        uint? freshTagDword = 0,
        uint? capacityDword = 0,
        bool? callReturnedNormally = false) =>
        new(
            captureValidatedEntry,
            entryEsi,
            entryEdi,
            entryEsp,
            initialGlobalDword,
            freshTagDword,
            capacityDword,
            callReturnedNormally);

    private static bool InstructionEvidenceMatches()
    {
        var instructions = CapacityFinalCalleeTagEqualityContinuationProjection.Instructions;
        uint[] rawOffsets =
        {
            0x012C8F21, 0x012C8F27, 0x012C8F29, 0x012C8F2F,
            0x012C8F31, 0x012C8F37, 0x012C8F3A, 0x012C8F3D,
            0x012C8F43, 0x012C8F45, 0x012C8F4F, 0x012C8F59,
            0x012C8F5F, 0x012C8F65, 0x012C8F67, 0x012C8F6C
        };
        return instructions.Length == rawOffsets.Length
            && instructions.Select(instruction => instruction.RawFileOffset)
                .SequenceEqual(rawOffsets)
            && instructions.All(instruction =>
                instruction.FlatIdaAddress == instruction.RawFileOffset
                && instruction.VirtualAddress
                    == CapacityFinalCalleeTagEqualityContinuationProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            && instructions[8].BranchTargetRawFileOffset == 0x012C8F59
            && instructions[12].BranchTargetRawFileOffset == 0x012C9441
            && instructions[12].BranchTargetVirtualAddress == 0x016CA041
            && instructions[15].BranchTargetRawFileOffset == 0x012C7BF0
            && instructions[15].BranchTargetVirtualAddress == 0x016C87F0;
    }

    private static bool ReadDescriptorsMatch()
    {
        var descriptors =
            CapacityFinalCalleeTagEqualityContinuationProjection.OrderedReadDescriptors;
        return descriptors.Length == 3
            && descriptors[0].RawFileOffset == 0x012C8F21
            && descriptors[0].Kind
                == CapacityFinalCalleeTagEqualityContinuationReadKind.InitialGlobalDword
            && descriptors[0].PeVirtualAddress == 0x01EDADD4
            && descriptors[1].RawFileOffset == 0x012C8F3D
            && descriptors[1].Kind
                == CapacityFinalCalleeTagEqualityContinuationReadKind.FreshTagDword
            && descriptors[1].PeVirtualAddress == 0x01EDADDC
            && descriptors[2].RawFileOffset == 0x012C8F59
            && descriptors[2].Kind
                == CapacityFinalCalleeTagEqualityContinuationReadKind.CapacityDword
            && descriptors[2].PeVirtualAddress == 0x01EDADE4;
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Where(character => !char.IsWhiteSpace(character)));
}
