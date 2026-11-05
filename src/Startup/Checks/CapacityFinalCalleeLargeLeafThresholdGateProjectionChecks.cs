using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeLeafThresholdGateProjectionChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The instruction window, raw range, VA mapping, and JB target are exact",
            CapacityFinalCalleeLargeLeafThresholdGateProjection.EntryRawFileOffset
                == 0x012C920B
            && CapacityFinalCalleeLargeLeafThresholdGateProjection.EntryVirtualAddress
                == 0x016C9E0B
            && CapacityFinalCalleeLargeLeafThresholdGateProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeLeafThresholdGateProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeLeafThresholdGateProjection.EntryVirtualAddress
            && CapacityFinalCalleeLargeLeafThresholdGateProjection.BoundaryRawFileOffsetExclusive
                == 0x012C921D
            && CapacityFinalCalleeLargeLeafThresholdGateProjection.BranchTargetRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeLargeLeafThresholdGateProjection.FallthroughRawFileOffset
                == 0x012C921D
            && CapacityFinalCalleeLargeLeafThresholdGateProjection.GlobalValueAddress
                == 0x01EDADD8
            && Normalize(CapacityFinalCalleeLargeLeafThresholdGateProjection.CapturedWindowBytes)
                == "3B15D8ADED018955FC8B55F40F8259FCFFFF"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeLeafThresholdGatePlan missingGlobal =
            CapacityFinalCalleeLargeLeafThresholdGateProjection.Project(new(
                EntryEDX: 0x20,
                FreshGlobalValue: null,
                EntryEBP: 0x100,
                FreshEbpMinusC: 0xABC));
        CapacityFinalCalleeLargeLeafThresholdGatePlan missingEDX =
            CapacityFinalCalleeLargeLeafThresholdGateProjection.Project(new(
                EntryEDX: null,
                FreshGlobalValue: 0x20,
                EntryEBP: 0x100,
                FreshEbpMinusC: 0xABC));
        check("The independent global operand read survives missing CMP inputs without future effects",
            missingGlobal.Outcome
                == CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingFreshGlobalValue
            && missingGlobal.GlobalReadReached
            && !missingGlobal.GlobalValueCaptured
            && missingGlobal.GlobalAddress == 0x01EDADD8
            && missingGlobal.Reads is [{ RawFileOffset: 0x012C920B, Address: 0x01EDADD8 }]
            && !missingGlobal.CompareReached
            && !missingGlobal.StoreReached
            && !missingGlobal.FreshFrameReadReached
            && !missingGlobal.BranchReached
            && missingGlobal.EntryEDXCaptured
            && !missingGlobal.UnreachedEntryEDXCaptureProvided
            && missingGlobal.UnreachedEntryEBPCaptureProvided
            && missingGlobal.UnreachedFreshFrameCaptureProvided
            && !missingGlobal.ExactCleanAdmission
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C920B
            && missingEDX.Outcome
                == CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingEntryEDX
            && missingEDX.GlobalReadReached
            && missingEDX.GlobalValueCaptured
            && missingEDX.GlobalValue == 0x20
            && missingEDX.Reads is [{ ValueCaptured: true, Value: 0x20 }]
            && !missingEDX.EntryEDXCaptured
            && !missingEDX.CompareReached
            && !missingEDX.StoreReached
            && !missingEDX.FreshFrameReadReached
            && !missingEDX.BranchReached
            && !missingEDX.UnreachedEntryEDXCaptureProvided
            && missingEDX.UnreachedEntryEBPCaptureProvided
            && missingEDX.UnreachedFreshFrameCaptureProvided
            && !missingEDX.ExactCleanAdmission
            && missingEDX.StopAtRawFileOffsetExclusive == 0x012C920B);

        CapacityFinalCalleeLargeLeafThresholdGatePlan missingEbp =
            CapacityFinalCalleeLargeLeafThresholdGateProjection.Project(new(
                EntryEDX: 0x20,
                FreshGlobalValue: 0x20,
                EntryEBP: null,
                FreshEbpMinusC: 0xABC));
        CapacityFinalCalleeLargeLeafThresholdGatePlan missingFreshFrame =
            CapacityFinalCalleeLargeLeafThresholdGateProjection.Project(new(
                EntryEDX: 0x20,
                FreshGlobalValue: 0x20,
                EntryEBP: 0x100,
                FreshEbpMinusC: null));
        check("Missing later operands stop at their own instruction after reached CMP and store effects",
            missingEbp.Outcome
                == CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingEntryEBP
            && missingEbp.CompareReached
            && missingEbp.CompareFlags is { Carry: false, Zero: true }
            && missingEbp.Events.Select(e => e.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeLeafThresholdGateEventKind.Read,
                CapacityFinalCalleeLargeLeafThresholdGateEventKind.Compare
            })
            && !missingEbp.StoreReached
            && !missingEbp.FreshFrameReadReached
            && missingEbp.UnreachedFreshFrameCaptureProvided
            && !missingEbp.ExactCleanAdmission
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C9211
            && missingFreshFrame.Outcome
                == CapacityFinalCalleeLargeLeafThresholdGateOutcome.MissingFreshEbpMinusC
            && missingFreshFrame.CompareFlags is { Carry: false, Zero: true }
            && missingFreshFrame.Writes is
                [{ RawFileOffset: 0x012C9211, Address: 0xFC, Value: 0x20, Committed: true }]
            && missingFreshFrame.FreshFrameReadReached
            && missingFreshFrame.FreshFrameAddress == 0xF4
            && !missingFreshFrame.FreshFrameCaptured
            && !missingFreshFrame.BranchReached
            && missingFreshFrame.Events.Select(e => e.Kind).SequenceEqual(new[]
            {
                CapacityFinalCalleeLargeLeafThresholdGateEventKind.Read,
                CapacityFinalCalleeLargeLeafThresholdGateEventKind.Compare,
                CapacityFinalCalleeLargeLeafThresholdGateEventKind.Write,
                CapacityFinalCalleeLargeLeafThresholdGateEventKind.Read
            })
            && !missingFreshFrame.ExactCleanAdmission
            && missingFreshFrame.StopAtRawFileOffsetExclusive == 0x012C9214);

        CapacityFinalCalleeLargeLeafThresholdGatePlan wrapped =
            CapacityFinalCalleeLargeLeafThresholdGateProjection.Project(new(
                EntryEDX: 0,
                FreshGlobalValue: 0xFFFFFFFF,
                EntryEBP: 0,
                FreshEbpMinusC: 0x1234));
        check("DWORD subtraction and EBP-relative addresses wrap while the clean JB is bounded",
            wrapped.CompareResult == 1
            && wrapped.CompareFlags is
            {
                Carry: true,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && wrapped.StoreAddress == 0xFFFFFFFC
            && wrapped.FreshFrameAddress == 0xFFFFFFF4
            && wrapped.StoreValue == 0
            && wrapped.BranchReached
            && wrapped.BranchTaken == true
            && wrapped.BranchSelectedDestinationRawFileOffset == 0x012C8E76
            && wrapped.BranchFlags == wrapped.CompareFlags
            && wrapped.ExactCleanAdmission
            && wrapped.StopAtRawFileOffsetExclusive == 0x012C8E76
            && CapacityFinalCalleeLargeLeafThresholdGateProjection.DeriveEbpMinus4Address(0)
                == 0xFFFFFFFC
            && CapacityFinalCalleeLargeLeafThresholdGateProjection.DeriveEbpMinusCAddress(0)
                == 0xFFFFFFF4);

        CapacityFinalCalleeLargeLeafThresholdGatePlan fallthrough =
            CapacityFinalCalleeLargeLeafThresholdGateProjection.Project(new(
                EntryEDX: 0x80000000,
                FreshGlobalValue: 0x7FFFFFFF,
                EntryEBP: 0x200,
                FreshEbpMinusC: 0xDEADBEEF));
        check("The opposite clean endpoint uses unsigned JB and preserves all six CMP flags",
            fallthrough.CompareResult == 1
            && fallthrough.CompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && fallthrough.StoreValue == 0x80000000
            && fallthrough.FreshFrameValue == 0xDEADBEEF
            && fallthrough.BranchTaken == false
            && fallthrough.BranchSelectedDestinationRawFileOffset == 0x012C921D
            && fallthrough.BranchFlags == fallthrough.CompareFlags
            && fallthrough.ExactCleanAdmission
            && fallthrough.StopAtRawFileOffsetExclusive == 0x012C921D);

        CapacityFinalCalleeLargeLeafThresholdGatePlan reloadIsFresh =
            CapacityFinalCalleeLargeLeafThresholdGateProjection.Project(new(
                EntryEDX: 0x100,
                FreshGlobalValue: 1,
                EntryEBP: 0x200,
                FreshEbpMinusC: 0xCAFEBABE));
        check("The fresh EBP-C reload is not inferred from the EDX store and does not replace CMP flags",
            reloadIsFresh.Writes is [{ Address: 0x1FC, Value: 0x100 }]
            && reloadIsFresh.FreshFrameAddress == 0x1F4
            && reloadIsFresh.FreshFrameValue == 0xCAFEBABE
            && reloadIsFresh.CompareFlags is { Carry: false }
            && reloadIsFresh.BranchFlags == reloadIsFresh.CompareFlags
            && reloadIsFresh.Events[^2].Kind
                == CapacityFinalCalleeLargeLeafThresholdGateEventKind.Read
            && reloadIsFresh.Events[^2].Value == 0xCAFEBABE
            && reloadIsFresh.Events[^1].Kind
                == CapacityFinalCalleeLargeLeafThresholdGateEventKind.Branch
            && reloadIsFresh.Events[^1].Flags == reloadIsFresh.CompareFlags
            && reloadIsFresh.ExactCleanAdmission);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C920B, "3B 15 D8 AD ED 01", null, null),
            (0x012C9211, "89 55 FC", null, null),
            (0x012C9214, "8B 55 F4", null, null),
            (0x012C9217, "0F 82 59 FC FF FF", 0x012C8E76, 0x016C9A76)
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeLeafThresholdGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeLeafThresholdGateInstructionEvidence instruction
            in CapacityFinalCalleeLargeLeafThresholdGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeLeafThresholdGateProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    ? instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeLargeLeafThresholdGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)
                    : instruction.BranchTargetVirtualAddress.HasValue))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeLargeLeafThresholdGateProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeLargeLeafThresholdGateProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeLeafThresholdGateProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeLeafThresholdGateProjection.Instructions
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
