using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSavedFrameGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The complete raw window and its VA mapping match",
            CapacityFinalCalleeLargeSavedFrameGateProjection.EntryRawFileOffset
                == 0x012C9226
            && CapacityFinalCalleeLargeSavedFrameGateProjection.EntryVirtualAddress
                == 0x016C9E26
            && CapacityFinalCalleeLargeSavedFrameGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9231
            && CapacityFinalCalleeLargeSavedFrameGateProjection.ZeroDestinationRawFileOffset
                == 0x012C92BD
            && CapacityFinalCalleeLargeSavedFrameGateProjection.NonzeroDestinationRawFileOffset
                == 0x012C9231
            && CapacityFinalCalleeLargeSavedFrameGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeSavedFrameGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeSavedFrameGateProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleeLargeSavedFrameGateProjection.CapturedWindowBytes)
                == "8B4DF085C90F848C000000"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeSavedFrameGatePlan allMissing =
            CapacityFinalCalleeLargeSavedFrameGateProjection.Project(new(
                EntryEBP: null,
                FreshFrameMinus10: null));
        check("Missing EBP stops at the single read without a later TEST",
            allMissing.Outcome
                == CapacityFinalCalleeLargeSavedFrameGateOutcome.MissingEntryEBP
            && !allMissing.EntryEBPCaptured
            && allMissing.FreshReadReached
            && allMissing.Reads.Length == 1
            && allMissing.Reads[0] is
            {
                RawFileOffset: 0x012C9226,
                Address: null,
                AddressCaptured: false,
                ValueCaptured: false,
                Value: null
            }
            && allMissing.ReachedInstructions.SequenceEqual(new[] { 0x012C9226u })
            && allMissing.Events.Select(item => item.Kind)
                .SequenceEqual(new[]
                {
                    CapacityFinalCalleeLargeSavedFrameGateEventKind.Read
                })
            && !allMissing.FreshFrameMinus10Captured
            && !allMissing.TestReached
            && !allMissing.JzReached
            && allMissing.UnreachedFreshFrameMinus10CaptureProvided == false
            && !allMissing.ExactCleanExitAdmitted
            && allMissing.StopAtRawFileOffsetExclusive == 0x012C9226);

        CapacityFinalCalleeLargeSavedFrameGatePlan missingEbpWithFreshValue =
            CapacityFinalCalleeLargeSavedFrameGateProjection.Project(new(
                EntryEBP: null,
                FreshFrameMinus10: 0));
        check("A supplied field value remains unreached when its address register is missing",
            missingEbpWithFreshValue.Reads.Length == 1
            && missingEbpWithFreshValue.Reads[0].Address is null
            && !missingEbpWithFreshValue.Reads[0].ValueCaptured
            && !missingEbpWithFreshValue.TestReached
            && !missingEbpWithFreshValue.JzReached
            && missingEbpWithFreshValue.UnreachedFreshFrameMinus10CaptureProvided
            && !missingEbpWithFreshValue.ExactCleanExitAdmitted);

        CapacityFinalCalleeLargeSavedFrameGatePlan missingFresh =
            CapacityFinalCalleeLargeSavedFrameGateProjection.Project(new(
                EntryEBP: 0x200,
                FreshFrameMinus10: null));
        check("Missing fresh memory data retains the computed read address but prevents TEST",
            missingFresh.Outcome
                == CapacityFinalCalleeLargeSavedFrameGateOutcome.MissingFreshFrameMinus10
            && missingFresh.EntryEBPCaptured
            && missingFresh.FreshFrameMinus10Address == 0x1F0
            && missingFresh.Reads.Length == 1
            && missingFresh.Reads[0] is
            {
                Address: 0x1F0,
                AddressCaptured: true,
                ValueCaptured: false,
                Value: null
            }
            && missingFresh.ReachedInstructions.SequenceEqual(new[] { 0x012C9226u })
            && !missingFresh.TestReached
            && !missingFresh.JzReached
            && !missingFresh.ExactCleanExitAdmitted
            && missingFresh.StopAtRawFileOffsetExclusive == 0x012C9226);

        CapacityFinalCalleeLargeSavedFrameGatePlan zero =
            CapacityFinalCalleeLargeSavedFrameGateProjection.Project(new(
                EntryEBP: 0x10,
                FreshFrameMinus10: 0));
        check("Captured zero reaches TEST and branches to 92BD with exact clean admission",
            zero.Outcome == CapacityFinalCalleeLargeSavedFrameGateOutcome.ZeroToTarget
            && zero.FreshFrameMinus10Address == 0
            && zero.Reads.Length == 1
            && zero.Reads[0] is
            {
                Address: 0,
                AddressCaptured: true,
                ValueCaptured: true,
                Value: 0
            }
            && zero.FreshFrameMinus10Captured
            && zero.FreshFrameMinus10 == 0
            && zero.TestOperand == 0
            && zero.TestResult == 0
            && zero.TestFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.JzReached
            && zero.JzTaken == true
            && zero.JzSelectedDestinationRawFileOffset == 0x012C92BD
            && zero.ReachedInstructions.SequenceEqual(new[]
            {
                0x012C9226u, 0x012C9229u, 0x012C922Bu
            })
            && zero.Events.Select(item => item.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeSavedFrameGateEventKind.Read,
                CapacityFinalCalleeLargeSavedFrameGateEventKind.Test,
                CapacityFinalCalleeLargeSavedFrameGateEventKind.Branch
            })
            && zero.ExactCleanExitAdmitted
            && zero.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeSavedFrameGatePlan nonzero =
            CapacityFinalCalleeLargeSavedFrameGateProjection.Project(new(
                EntryEBP: 0x200,
                FreshFrameMinus10: 1));
        check("Nonzero falls through at the exact bounded endpoint",
            nonzero.Outcome
                == CapacityFinalCalleeLargeSavedFrameGateOutcome.NonzeroToFallthrough
            && nonzero.FreshFrameMinus10Address == 0x1F0
            && nonzero.TestFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && nonzero.JzReached
            && nonzero.JzTaken == false
            && nonzero.JzSelectedDestinationRawFileOffset == 0x012C9231
            && nonzero.ExactCleanExitAdmitted
            && nonzero.StopAtRawFileOffsetExclusive == 0x012C9231);

        CapacityFinalCalleeLargeSavedFrameGatePlan maximum =
            CapacityFinalCalleeLargeSavedFrameGateProjection.Project(new(
                EntryEBP: 0x10,
                FreshFrameMinus10: uint.MaxValue));
        check("The maximum captured DWORD uses its low byte for parity and bit 31 for sign",
            maximum.FreshFrameMinus10 == uint.MaxValue
            && maximum.TestOperand == uint.MaxValue
            && maximum.TestResult == uint.MaxValue
            && maximum.TestFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && maximum.JzTaken == false
            && maximum.StopAtRawFileOffsetExclusive == 0x012C9231);

        CapacityFinalCalleeLargeSavedFrameGatePlan wrappedAddress =
            CapacityFinalCalleeLargeSavedFrameGateProjection.Project(new(
                EntryEBP: 0x0F,
                FreshFrameMinus10: 0x80000001));
        check("The EBP-relative address wraps as a DWORD without changing the fresh value",
            wrappedAddress.FreshFrameMinus10Address == 0xFFFFFFFF
            && wrappedAddress.Reads[0].Value == 0x80000001
            && CapacityFinalCalleeLargeSavedFrameGateProjection
                .DeriveFrameMinus10Address(0x0F) == 0xFFFFFFFF
            && wrappedAddress.TestFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: true,
                Overflow: false
            });

        CapacityFinalCalleeLargeSavedFrameGatePlan lowByteParity =
            CapacityFinalCalleeLargeSavedFrameGateProjection.Project(new(
                EntryEBP: 0x20,
                FreshFrameMinus10: 0x01000001));
        check("TEST parity uses only the low byte while preserving the upper result bits",
            lowByteParity.TestResult == 0x01000001
            && lowByteParity.TestFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: null,
                Zero: false,
                Sign: false,
                Overflow: false
            });
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[] {
            (0x012C9226, "8B 4D F0", null, null),
            (0x012C9229, "85 C9", null, null),
            (0x012C922B, "0F 84 8C 00 00 00", 0x012C92BD, 0x016C9EBD)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeSavedFrameGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeSavedFrameGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeSavedFrameGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeSavedFrameGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeSavedFrameGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeLargeSavedFrameGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeSavedFrameGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeSavedFrameGateProjection.Instructions
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
