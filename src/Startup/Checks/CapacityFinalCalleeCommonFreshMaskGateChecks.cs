using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonFreshMaskGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        var evidence =
            CapacityFinalCalleeCommonFreshMaskGateProjection.ProductionInstructionEvidence;
        check("MOV/SHL/TEST/JNZ evidence and branch mapping match the bounded byte window",
            evidence.Length == 4
            && evidence[0] is
            {
                Mnemonic: "MOV EAX, [01EDADCC]",
                RawFileOffset: 0x012C93A1,
                FlatIdaAddress: 0x012C93A1,
                PEVirtualAddress: 0x016C9FA1
            }
            && HasBytes(evidence[0], 0xA1, 0xCC, 0xAD, 0xED, 0x01)
            && evidence[1] is
            {
                Mnemonic: "SHL EDX, CL",
                RawFileOffset: 0x012C93A6,
                PEVirtualAddress: 0x016C9FA6
            }
            && HasBytes(evidence[1], 0xD3, 0xE2)
            && evidence[2] is
            {
                Mnemonic: "TEST EAX, EDX",
                RawFileOffset: 0x012C93A8,
                PEVirtualAddress: 0x016C9FA8
            }
            && HasBytes(evidence[2], 0x85, 0xD0)
            && evidence[3] is
            {
                Mnemonic: "JNZ 012C93C0",
                RawFileOffset: 0x012C93AA,
                PEVirtualAddress: 0x016C9FAA,
                BranchTargetRawFileOffset: 0x012C93C0,
                BranchTargetPEVirtualAddress: 0x016C9FC0
            }
            && HasBytes(evidence[3], 0x75, 0x14)
            && Normalize(CapacityFinalCalleeCommonFreshMaskGateProjection.CapturedWindowBytes)
                == "A1CCADED01D3E285D07514"
            && EvidenceLookupMatches());

        var destinations =
            CapacityFinalCalleeCommonFreshMaskGateProjection.BoundedDestinations;
        check("Both branch destinations are bounded before their target instructions",
            destinations.Length == 2
            && destinations[0] is
            {
                RawFileOffset: 0x012C93AC,
                FlatIdaAddress: 0x012C93AC,
                PEVirtualAddress: 0x016C9FAC,
                BeforeTargetInstruction: true
            }
            && destinations[1] is
            {
                RawFileOffset: 0x012C93C0,
                FlatIdaAddress: 0x012C93C0,
                PEVirtualAddress: 0x016C9FC0,
                BeforeTargetInstruction: true
            }
            && CapacityFinalCalleeCommonFreshMaskGateProjection.RawFileOffsetToVirtualAddress(
                uint.MaxValue) == 0x00400BFF);

        CapacityFinalCalleeCommonFreshMaskGatePlan missingGlobal =
            CapacityFinalCalleeCommonFreshMaskGateProjection.Project(
                new(FreshGlobalValue: null, EntryEDX: 0xFFFFFFFF, EntryECX: 32));
        check("Missing global records the absolute read attempt and leaves both shift captures unused",
            missingGlobal.Outcome
                == CapacityFinalCalleeCommonFreshMaskGateOutcome.MissingFreshGlobalValue
            && missingGlobal.FreshGlobalReadReached
            && !missingGlobal.FreshGlobalValueCaptured
            && missingGlobal.FreshGlobalReadAddress == 0x01EDADCC
            && missingGlobal.ReadAttempts is
            [
                {
                    RawFileOffset: 0x012C93A1,
                    AbsoluteAddress: 0x01EDADCC,
                    ReadReached: true,
                    ValueCaptured: false,
                    Value: null
                }
            ]
            && missingGlobal.OrderedEvents.Length == 1
            && !missingGlobal.OrderedEvents[0].Completed
            && missingGlobal.UnreachedEntryEDXCaptureProvided
            && missingGlobal.UnreachedEntryECXCaptureProvided
            && missingGlobal.EntryEDXCaptureProvidedButUnused
            && missingGlobal.EntryECXCaptureProvidedButUnused
            && !missingGlobal.ShiftInstructionReached
            && !missingGlobal.TestReached
            && !missingGlobal.JnzReached
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C93A1);

        CapacityFinalCalleeCommonFreshMaskGatePlan missingEDX =
            CapacityFinalCalleeCommonFreshMaskGateProjection.Project(
                new(FreshGlobalValue: 0xFFFFFFFF, EntryEDX: null, EntryECX: 31));
        CapacityFinalCalleeCommonFreshMaskGatePlan missingECX =
            CapacityFinalCalleeCommonFreshMaskGateProjection.Project(
                new(FreshGlobalValue: 0xFFFFFFFF, EntryEDX: 0x80000001, EntryECX: null));
        CapacityFinalCalleeCommonFreshMaskGatePlan missingBoth =
            CapacityFinalCalleeCommonFreshMaskGateProjection.Project(
                new(FreshGlobalValue: 0, EntryEDX: null, EntryECX: null));
        check("Shift operands are independently retained but not consumed when either is missing",
            missingEDX.Outcome
                == CapacityFinalCalleeCommonFreshMaskGateOutcome.MissingEntryEDX
            && missingEDX.Capture.EntryECX == 31
            && missingEDX.ShiftCountMasked == 31
            && missingEDX.EntryECXCaptureProvidedButUnused
            && missingEDX.EntryEDXOperandConsumed == false
            && missingEDX.ShiftInstructionReached
            && !missingEDX.ShiftExecuted
            && missingEDX.OrderedEvents.Length == 2
            && missingEDX.OrderedEvents[1].Stage
                == CapacityFinalCalleeCommonFreshMaskGateInstructionStage.ShiftEDXByCL
            && !missingEDX.OrderedEvents[1].Completed
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C93A6
            && missingECX.Outcome
                == CapacityFinalCalleeCommonFreshMaskGateOutcome.MissingEntryECX
            && missingECX.Capture.EntryEDX == 0x80000001
            && missingECX.ShiftInputEDX == 0x80000001
            && missingECX.EntryEDXCaptureProvidedButUnused
            && missingECX.StopAtRawFileOffsetExclusive == 0x012C93A6
            && missingBoth.Outcome
                == CapacityFinalCalleeCommonFreshMaskGateOutcome
                    .MissingEntryEDXAndEntryECX
            && missingBoth.StopAtRawFileOffsetExclusive == 0x012C93A6);

        (uint CountSource, byte ExpectedCount)[] countCases =
        [
            (0, 0),
            (1, 1),
            (31, 31),
            (32, 0),
            (0xFFFFFFFF, 31)
        ];
        foreach ((uint countSource, byte expectedCount) in countCases)
        {
            uint[] edxValues = [0, 1, 0x80000000, 0xFFFFFFFF];
            foreach (uint edx in edxValues)
            {
                CapacityFinalCalleeCommonFreshMaskGatePlan plan =
                    CapacityFinalCalleeCommonFreshMaskGateProjection.Project(
                        new(FreshGlobalValue: uint.MaxValue, EntryEDX: edx, EntryECX: countSource));
                uint shifted = edx << expectedCount;
                bool? carry = expectedCount == 0
                    ? null
                    : ((edx >> (32 - expectedCount)) & 1u) != 0;
                bool? overflow = expectedCount == 1
                    ? ((shifted & 0x80000000u) != 0) ^ carry!.Value
                    : null;
                var expectedShiftFlags = expectedCount == 0
                    ? new CapacityFinalCalleeCommonFreshMaskGateFlags(
                        null, null, null, null, null, null)
                    : new CapacityFinalCalleeCommonFreshMaskGateFlags(
                        carry,
                        EvenParity((byte)shifted),
                        null,
                        shifted == 0,
                        (shifted & 0x80000000u) != 0,
                        overflow);

                check($"ECX {countSource:X8} and EDX {edx:X8} follow masked SHL semantics",
                    plan.ShiftCountSource == countSource
                    && plan.ShiftCountLowByte == (byte)countSource
                    && plan.ShiftCountMasked == expectedCount
                    && plan.ShiftInputEDX == edx
                    && plan.ShiftResult == shifted
                    && plan.ShiftFlags == expectedShiftFlags
                    && plan.TestResult == shifted
                    && plan.TestFlags is
                    {
                        Carry: false,
                        Auxiliary: null,
                        Overflow: false
                    }
                    && plan.FlagsAtStop == plan.TestFlags
                    && plan.StopAtRawFileOffsetExclusive
                        == (shifted == 0 ? 0x012C93ACu : 0x012C93C0u)
                    && plan.ExactCleanBoundedDestination);
            }
        }

        CapacityFinalCalleeCommonFreshMaskGatePlan zeroBranch =
            CapacityFinalCalleeCommonFreshMaskGateProjection.Project(
                new(FreshGlobalValue: 0x7FFFFFFF, EntryEDX: 0x80000000, EntryECX: 0));
        CapacityFinalCalleeCommonFreshMaskGatePlan nonzeroBranch =
            CapacityFinalCalleeCommonFreshMaskGateProjection.Project(
                new(FreshGlobalValue: 0x80000000, EntryEDX: 0x80000000, EntryECX: 0));
        check("Unsigned high bits select the correct TEST/JNZ branch and TEST flags replace SHL flags",
            zeroBranch.Outcome
                == CapacityFinalCalleeCommonFreshMaskGateOutcome.ZeroTestResultToBounded93AC
            && zeroBranch.TestResult == 0
            && zeroBranch.TestFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zeroBranch.JnzTaken == false
            && zeroBranch.JnzSelectedDestinationRawFileOffset == 0x012C93AC
            && zeroBranch.StopAtRawFileOffsetExclusive == 0x012C93AC
            && nonzeroBranch.Outcome
                == CapacityFinalCalleeCommonFreshMaskGateOutcome.NonzeroTestResultToBounded93C0
            && nonzeroBranch.TestResult == 0x80000000
            && nonzeroBranch.TestFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonzeroBranch.JnzTaken == true
            && nonzeroBranch.JnzSelectedDestinationRawFileOffset == 0x012C93C0
            && nonzeroBranch.StopAtRawFileOffsetExclusive == 0x012C93C0
            && zeroBranch.OrderedEvents.Length == 4
            && zeroBranch.OrderedEvents[3].Stage
                == CapacityFinalCalleeCommonFreshMaskGateInstructionStage.Jnz
            && !zeroBranch.MemoryWritePerformed);

        CapacityFinalCalleeCommonFreshMaskGatePlan independentFreshRead =
            CapacityFinalCalleeCommonFreshMaskGateProjection.Project(
                new(FreshGlobalValue: 0x40000000, EntryEDX: 0x80000000, EntryECX: 1));
        check("The captured global is tested independently against the shifted entry EDX",
            independentFreshRead.ReadAttempts[0].Value == 0x40000000
            && independentFreshRead.ShiftInputEDX == 0x80000000
            && independentFreshRead.ShiftResult == 0
            && independentFreshRead.TestResult == 0
            && independentFreshRead.JnzTaken == false);
    }

    private static bool EvidenceLookupMatches()
    {
        foreach (CapacityFinalCalleeCommonFreshMaskGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonFreshMaskGateProjection.ProductionInstructionEvidence)
        {
            if (instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.PEVirtualAddress
                    != CapacityFinalCalleeCommonFreshMaskGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || !CapacityFinalCalleeCommonFreshMaskGateProjection.TryGetInstructionEvidence(
                    instruction.RawFileOffset, out var found)
                || found != instruction)
            {
                return false;
            }
        }

        return true;
    }

    private static bool EvenParity(byte value)
    {
        int setBits = 0;
        for (int bit = 0; bit < 8; bit++)
        {
            setBits += (value >> bit) & 1;
        }

        return (setBits & 1) == 0;
    }

    private static bool HasBytes(
        CapacityFinalCalleeCommonFreshMaskGateInstructionEvidence instruction,
        params byte[] expected)
    {
        if (instruction.Bytes.Length != expected.Length)
        {
            return false;
        }

        for (int index = 0; index < expected.Length; index++)
        {
            if (instruction.Bytes[index] != expected[index])
            {
                return false;
            }
        }

        return true;
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
