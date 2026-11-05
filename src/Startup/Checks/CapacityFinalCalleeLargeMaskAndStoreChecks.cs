using System;
using System.Linq;
using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeMaskAndStoreChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The AND/jump bytes, raw offsets, VA mapping, global address, and branch are exact",
            CapacityFinalCalleeLargeMaskAndStoreProjection.EntryRawFileOffset
                == 0x012C925B
            && CapacityFinalCalleeLargeMaskAndStoreProjection.EntryFlatIdaAddress
                == 0x012C925B
            && CapacityFinalCalleeLargeMaskAndStoreProjection.EntryVirtualAddress
                == 0x016C9E5B
            && CapacityFinalCalleeLargeMaskAndStoreProjection.AndRawFileOffset
                == 0x012C925B
            && CapacityFinalCalleeLargeMaskAndStoreProjection.JumpRawFileOffset
                == 0x012C9261
            && CapacityFinalCalleeLargeMaskAndStoreProjection.BoundaryRawFileOffsetExclusive
                == 0x012C9263
            && CapacityFinalCalleeLargeMaskAndStoreProjection.JumpTargetRawFileOffset
                == 0x012C92BD
            && CapacityFinalCalleeLargeMaskAndStoreProjection.GlobalVirtualAddress
                == 0x01EDADCC
            && CapacityFinalCalleeLargeMaskAndStoreProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeMaskAndStoreProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeMaskAndStoreProjection.EntryVirtualAddress
            && CapacityFinalCalleeLargeMaskAndStoreProjection.CalculateJumpTargetRawFileOffset()
                == 0x012C92BD
            && Normalize(CapacityFinalCalleeLargeMaskAndStoreProjection.CapturedWindowBytes)
                == "2105CCADED01EB5A"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeMaskAndStorePlan zero =
            CapacityFinalCalleeLargeMaskAndStoreProjection.Project(new(0, 0));
        check("Zero operands write zero and produce the exact logical-AND flags",
            zero.Outcome == CapacityFinalCalleeLargeMaskAndStoreOutcome.Completed
            && zero.EntryEAXCaptured
            && zero.FreshGlobalDwordCaptured
            && zero.GlobalReadReached
            && zero.GlobalReadAddress == 0x01EDADCC
            && zero.GlobalReadValue == 0
            && zero.AndExecuted
            && zero.AndEAX == 0
            && zero.AndFreshGlobalDword == 0
            && zero.AndResult == 0
            && zero.AndFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && zero.GlobalWriteReached
            && zero.GlobalWriteAddress == 0x01EDADCC
            && zero.GlobalWriteValue == 0
            && zero.Reads.Length == 1
            && zero.Writes.Length == 1
            && zero.Writes[0].RawFileOffset == 0x012C925B
            && zero.Writes[0].Address == 0x01EDADCC
            && zero.Writes[0].Value == 0
            && zero.Writes[0].Committed
            && zero.Events.Length == 2
            && zero.Events[0].Kind == CapacityFinalCalleeLargeMaskAndStoreEventKind.Read
            && zero.Events[1].Kind == CapacityFinalCalleeLargeMaskAndStoreEventKind.Write
            && zero.JumpReached
            && zero.JumpTargetRawFileOffset == 0x012C92BD
            && zero.ExactStopAtJumpTarget
            && !zero.CommonTailEntered
            && zero.FlagsAtStop == zero.AndFlags
            && zero.StopAtRawFileOffsetExclusive == 0x012C92BD);

        CapacityFinalCalleeLargeMaskAndStorePlan allOnes =
            CapacityFinalCalleeLargeMaskAndStoreProjection.Project(
                new(0xFFFFFFFF, 0xFFFFFFFF));
        check("All-one AND result is stored unchanged with even low-byte parity",
            allOnes.AndResult == 0xFFFFFFFF
            && allOnes.AndFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && allOnes.GlobalWriteAddress == 0x01EDADCC
            && allOnes.GlobalWriteValue == 0xFFFFFFFF
            && allOnes.Writes.Length == 1
            && allOnes.Writes[0].Value == 0xFFFFFFFF
            && allOnes.JumpTargetRawFileOffset == 0x012C92BD
            && !allOnes.CommonTailEntered);

        CapacityFinalCalleeLargeMaskAndStorePlan highBit =
            CapacityFinalCalleeLargeMaskAndStoreProjection.Project(
                new(0x80000000, 0xFFFFFFFF));
        check("The high bit sets sign while zero low byte has even parity",
            highBit.AndResult == 0x80000000
            && highBit.AndFlags is
            {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            });

        CapacityFinalCalleeLargeMaskAndStorePlan oddParity =
            CapacityFinalCalleeLargeMaskAndStoreProjection.Project(new(1, 1));
        check("Parity is computed from the result low byte",
            oddParity.AndResult == 1
            && oddParity.AndFlags is
            {
                Carry: false,
                Parity: false,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            });

        CapacityFinalCalleeLargeMaskAndStorePlan missingBoth =
            CapacityFinalCalleeLargeMaskAndStoreProjection.Project(new(null, null));
        check("Missing both operands retains the known read address and stops before write or jump",
            missingBoth.Outcome
                == CapacityFinalCalleeLargeMaskAndStoreOutcome.MissingBothOperands
            && !missingBoth.EntryEAXCaptured
            && !missingBoth.FreshGlobalDwordCaptured
            && missingBoth.GlobalReadReached
            && missingBoth.GlobalReadAddress == 0x01EDADCC
            && missingBoth.GlobalReadValue is null
            && missingBoth.Reads.Length == 1
            && missingBoth.Reads[0].AddressCaptured
            && !missingBoth.Reads[0].ValueCaptured
            && !missingBoth.AndExecuted
            && !missingBoth.GlobalWriteReached
            && missingBoth.Writes.IsEmpty
            && !missingBoth.JumpReached
            && !missingBoth.ExactStopAtJumpTarget
            && missingBoth.StopAtRawFileOffsetExclusive == 0x012C925B);

        CapacityFinalCalleeLargeMaskAndStorePlan missingEAX =
            CapacityFinalCalleeLargeMaskAndStoreProjection.Project(new(null, 0xA5A55A5A));
        check("Missing EAX independently retains the captured global read and stops at the AND",
            missingEAX.Outcome
                == CapacityFinalCalleeLargeMaskAndStoreOutcome.MissingEntryEAX
            && !missingEAX.EntryEAXCaptured
            && missingEAX.FreshGlobalDwordCaptured
            && missingEAX.GlobalReadReached
            && missingEAX.GlobalReadAddress == 0x01EDADCC
            && missingEAX.GlobalReadValue == 0xA5A55A5A
            && missingEAX.Reads.Length == 1
            && missingEAX.Reads[0].Address == 0x01EDADCC
            && missingEAX.Reads[0].Value == 0xA5A55A5A
            && !missingEAX.AndExecuted
            && !missingEAX.GlobalWriteReached
            && missingEAX.Writes.IsEmpty
            && !missingEAX.JumpReached
            && missingEAX.StopAtRawFileOffsetExclusive == 0x012C925B);

        CapacityFinalCalleeLargeMaskAndStorePlan missingGlobal =
            CapacityFinalCalleeLargeMaskAndStoreProjection.Project(new(0x12345678, null));
        check("Missing fresh global independently retains EAX and the absolute read address",
            missingGlobal.Outcome
                == CapacityFinalCalleeLargeMaskAndStoreOutcome.MissingFreshGlobalDword
            && missingGlobal.EntryEAXCaptured
            && !missingGlobal.FreshGlobalDwordCaptured
            && missingGlobal.GlobalReadReached
            && missingGlobal.GlobalReadAddress == 0x01EDADCC
            && missingGlobal.Reads.Length == 1
            && missingGlobal.Reads[0].AddressCaptured
            && !missingGlobal.Reads[0].ValueCaptured
            && !missingGlobal.AndExecuted
            && !missingGlobal.GlobalWriteReached
            && missingGlobal.Writes.IsEmpty
            && !missingGlobal.JumpReached
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C925B);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes)[]
        {
            (0x012C925B, "21 05 CC AD ED 01"),
            (0x012C9261, "EB 5A")
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeMaskAndStoreProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeMaskAndStoreInstructionEvidence instruction
            in CapacityFinalCalleeLargeMaskAndStoreProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeMaskAndStoreProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return CapacityFinalCalleeLargeMaskAndStoreProjection.Instructions.Length
                == expected.Length
            && nextRawOffset
                == CapacityFinalCalleeLargeMaskAndStoreProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeMaskAndStoreProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeMaskAndStoreProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
