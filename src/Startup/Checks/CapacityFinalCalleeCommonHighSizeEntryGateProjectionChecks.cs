using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeCommonHighSizeEntryGateProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw range, flat/VA mapping, branch destinations, and bytes match",
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.EntryRawFileOffset
                == 0x012C9354
            && CapacityFinalCalleeCommonHighSizeEntryGateProjection.EntryVirtualAddress
                == 0x016C9F54
            && CapacityFinalCalleeCommonHighSizeEntryGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonHighSizeEntryGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeCommonHighSizeEntryGateProjection.EntryVirtualAddress
            && CapacityFinalCalleeCommonHighSizeEntryGateProjection.HighSizeDestinationRawFileOffset
                == 0x012C9361
            && CapacityFinalCalleeCommonHighSizeEntryGateProjection.ZeroSizeDestinationRawFileOffset
                == 0x012C9384
            && CapacityFinalCalleeCommonHighSizeEntryGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonHighSizeEntryGateProjection.HighSizeDestinationRawFileOffset)
                == 0x016C9F61
            && CapacityFinalCalleeCommonHighSizeEntryGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeCommonHighSizeEntryGateProjection.ZeroSizeDestinationRawFileOffset)
                == 0x016C9F84
            && CapacityFinalCalleeCommonHighSizeEntryGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9361
            && Normalize(CapacityFinalCalleeCommonHighSizeEntryGateProjection.CapturedWindowBytes)
                == "8BC7C1E80885C0750433C9EB23"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeCommonHighSizeEntryGatePlan missing =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.Project(new(EntryEDI: null));
        check("Missing independent entry EDI stops at 9354 without executing instructions",
            missing.Outcome
                == CapacityFinalCalleeCommonHighSizeEntryGateOutcome.MissingEntryEDI
            && !missing.EntryEDICaptured
            && !missing.MoveReached
            && !missing.ShiftReached
            && !missing.TestReached
            && !missing.JNZReached
            && !missing.XorReached
            && !missing.JMPReached
            && missing.Events.IsEmpty
            && missing.StopAtRawFileOffsetExclusive == 0x012C9354
            && NoMemoryEffects(missing));

        CapacityFinalCalleeCommonHighSizeEntryGatePlan zero =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.Project(new(EntryEDI: 0));
        CapacityFinalCalleeCommonHighSizeEntryGatePlan byteOnly =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.Project(new(EntryEDI: 0xFF));
        CapacityFinalCalleeCommonHighSizeEntryGatePlan lastLowWordByte =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.Project(
                new(EntryEDI: 0x0000FFFF));
        CapacityFinalCalleeCommonHighSizeEntryGatePlan lastByte =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.Project(
                new(EntryEDI: 0x000000FF));
        check("Zero high-size values execute XOR and the later JMP to 9384",
            new[] { zero, byteOnly, lastByte }.All(plan =>
                plan.Outcome
                    == CapacityFinalCalleeCommonHighSizeEntryGateOutcome
                        .ZeroSizeAt9384BeforeIndexedStore
                && plan.EntryEDICaptured
                && plan.MoveResultEAX == plan.Capture.EntryEDI
                && plan.ShiftResultEAX == 0
                && plan.TestFlags is
                {
                    Carry: false,
                    Parity: true,
                    Auxiliary: null,
                    Zero: true,
                    Sign: false,
                    Overflow: false
                }
                && plan.JNZReached
                && plan.JNZTaken == false
                && plan.JNZSelectedDestinationRawFileOffset == 0x012C935D
                && plan.XorReached
                && plan.XorResultECX == 0
                && plan.JMPReached
                && plan.JMPTaken == true
                && plan.JMPSelectedDestinationRawFileOffset == 0x012C9384
                && plan.FlagsAtStop == plan.XorFlags
                && plan.StopAtRawFileOffsetExclusive == 0x012C9384
                && plan.Events.Select(e => e.Kind).SequenceEqual(new[]
                {
                    CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Move,
                    CapacityFinalCalleeCommonHighSizeEntryGateEventKind.ShiftRight,
                    CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Test,
                    CapacityFinalCalleeCommonHighSizeEntryGateEventKind.BranchNotZero,
                    CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Xor,
                    CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Jump
                })
                && NoMemoryEffects(plan)));

        check("SHR carry from bit 7 is intermediate and TEST resets carry and overflow",
            byteOnly.ShiftFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: null
            }
            && byteOnly.TestFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && byteOnly.FlagsAtStop is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.XorFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            });

        CapacityFinalCalleeCommonHighSizeEntryGatePlan firstNonzeroByte =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.Project(new(EntryEDI: 0x100));
        CapacityFinalCalleeCommonHighSizeEntryGatePlan lastValueBeforeWordBoundary =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.Project(
                new(EntryEDI: 0x0000FFFF));
        CapacityFinalCalleeCommonHighSizeEntryGatePlan firstNonzeroWord =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.Project(
                new(EntryEDI: 0x00010000));
        CapacityFinalCalleeCommonHighSizeEntryGatePlan highByteOnly =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.Project(
                new(EntryEDI: 0x0000FF00));
        CapacityFinalCalleeCommonHighSizeEntryGatePlan allOnes =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.Project(
                new(EntryEDI: 0xFFFFFFFF));
        CapacityFinalCalleeCommonHighSizeEntryGatePlan highBit =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.Project(
                new(EntryEDI: 0x80000000));
        check("Nonzero shifted values branch to 9361 and stop before the comparison",
            firstNonzeroByte.ShiftResultEAX == 1
            && lastValueBeforeWordBoundary.ShiftResultEAX == 0xFF
            && firstNonzeroWord.ShiftResultEAX == 0x100
            && highByteOnly.ShiftResultEAX == 0xFF
            && allOnes.ShiftResultEAX == 0x00FFFFFF
            && highBit.ShiftResultEAX == 0x00800000
            && new[]
            {
                firstNonzeroByte, lastValueBeforeWordBoundary, firstNonzeroWord,
                highByteOnly, allOnes, highBit
            }.All(plan =>
                plan.Outcome
                    == CapacityFinalCalleeCommonHighSizeEntryGateOutcome
                        .HighSizeAt9361BeforeCompare
                && plan.JNZReached
                && plan.JNZTaken == true
                && plan.JNZSelectedDestinationRawFileOffset == 0x012C9361
                && !plan.XorReached
                && !plan.JMPReached
                && plan.FlagsAtStop == plan.TestFlags
                && plan.StopAtRawFileOffsetExclusive == 0x012C9361
                && plan.Events.Length == 4
                && plan.Events[^1].Kind
                    == CapacityFinalCalleeCommonHighSizeEntryGateEventKind.BranchNotZero
                && NoMemoryEffects(plan)));

        check("The high-size branch observes TEST flags rather than SHR flags",
            allOnes.ShiftFlags?.Carry == true
            && allOnes.ShiftFlags?.Overflow is null
            && allOnes.TestFlags is
            {
                Carry: false,
                Auxiliary: null,
                Zero: false,
                Overflow: false
            }
            && allOnes.FlagsAtStop == allOnes.TestFlags
            && allOnes.JNZTaken == true);

        check("The zero branch and its unconditional jump remain distinct events and stops",
            zero.Events[^2].Kind
                == CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Xor
            && zero.Events[^1] is
            {
                Kind: CapacityFinalCalleeCommonHighSizeEntryGateEventKind.Jump,
                RawFileOffset: 0x012C935F,
                BranchTaken: true,
                SelectedDestinationRawFileOffset: 0x012C9384
            }
            && zero.JNZSelectedDestinationRawFileOffset == 0x012C935D
            && zero.StopAtRawFileOffsetExclusive == 0x012C9384
            && allOnes.JNZSelectedDestinationRawFileOffset == 0x012C9361
            && allOnes.StopAtRawFileOffsetExclusive == 0x012C9361);

        check("All paths remain bounded and produce no frame/global reads or writes",
            new[]
            {
                missing, zero, byteOnly, lastByte,
                firstNonzeroByte, lastValueBeforeWordBoundary, firstNonzeroWord,
                highByteOnly, allOnes, highBit
            }.All(NoMemoryEffects));
    }

    private static bool NoMemoryEffects(
        CapacityFinalCalleeCommonHighSizeEntryGatePlan plan) =>
        !plan.FrameReadReached
        && !plan.GlobalReadReached
        && plan.Reads.IsEmpty
        && plan.Writes.IsEmpty
        && !plan.TargetEntered;

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C9354, "8B C7", null, null),
            (0x012C9356, "C1 E8 08", null, null),
            (0x012C9359, "85 C0", null, null),
            (0x012C935B, "75 04", 0x012C9361, 0x016C9F61),
            (0x012C935D, "33 C9", null, null),
            (0x012C935F, "EB 23", 0x012C9384, 0x016C9F84)
        };
        uint nextRawOffset =
            CapacityFinalCalleeCommonHighSizeEntryGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeCommonHighSizeEntryGateInstructionEvidence instruction
            in CapacityFinalCalleeCommonHighSizeEntryGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeCommonHighSizeEntryGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeCommonHighSizeEntryGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeCommonHighSizeEntryGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeCommonHighSizeEntryGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeCommonHighSizeEntryGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeCommonHighSizeEntryGateProjection.Instructions
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
