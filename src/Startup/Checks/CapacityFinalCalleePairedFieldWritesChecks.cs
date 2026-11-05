using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleePairedFieldWritesChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The paired-write window, absolute operand, and instruction order match",
            CapacityFinalCalleePairedFieldWritesProjection.EntryRawFileOffset
                == 0x012C8EE9
            && CapacityFinalCalleePairedFieldWritesProjection.EntryVirtualAddress
                == 0x016C9AE9
            && CapacityFinalCalleePairedFieldWritesProjection.FirstWriteRawFileOffset
                == 0x012C8EEC
            && CapacityFinalCalleePairedFieldWritesProjection.SecondWriteRawFileOffset
                == 0x012C8EEF
            && CapacityFinalCalleePairedFieldWritesProjection.FreshGlobalLoadRawFileOffset
                == 0x012C8EF2
            && CapacityFinalCalleePairedFieldWritesProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8EF8
            && CapacityFinalCalleePairedFieldWritesProjection.GlobalValuePeVirtualAddress
                == 0x01EDADD8
            && CapacityFinalCalleePairedFieldWritesProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleePairedFieldWritesProjection.EntryRawFileOffset)
                == CapacityFinalCalleePairedFieldWritesProjection.EntryVirtualAddress
            && Normalize(CapacityFinalCalleePairedFieldWritesProjection.CapturedWindowBytes)
                == "8B4DFC89410C8948088B1DD8ADED01"
            && InstructionEvidenceMatches());

        CapacityFinalCalleePairedFieldWritesPlan missingFrame =
            CapacityFinalCalleePairedFieldWritesProjection.Project(Capture(
                validatedEntryEBP: 4,
                validatedEntryEAX: 0x100,
                freshFrameMinus4: null,
                freshGlobalValue: 0));
        check("A missing fresh frame DWORD blocks both writes and the later global read",
            missingFrame.Outcome
                == CapacityFinalCalleePairedFieldWritesOutcome.MissingFreshFrameMinus4
            && missingFrame.FreshFrameMinus4Address == 0
            && missingFrame.FreshFrameMinus4ReadReached
            && !missingFrame.FreshFrameMinus4Captured
            && missingFrame.ECXLoaded is null
            && missingFrame.EAXRetained == 0x100
            && missingFrame.UnreachedValidatedEntryEAXCaptureProvided
            && !missingFrame.FirstWriteReached
            && !missingFrame.SecondWriteReached
            && !missingFrame.FreshGlobalReadReached
            && missingFrame.UnreachedFreshGlobalValueCaptureProvided
            && missingFrame.StopAtRawFileOffsetExclusive == 0x012C8EE9);

        CapacityFinalCalleePairedFieldWritesPlan missingEax =
            CapacityFinalCalleePairedFieldWritesProjection.Project(Capture(
                validatedEntryEBP: 0x100,
                validatedEntryEAX: null,
                freshFrameMinus4: 0,
                freshGlobalValue: 0x77));
        check("ECX loads independently before missing retained EAX blocks both writes",
            missingEax.Outcome
                == CapacityFinalCalleePairedFieldWritesOutcome.MissingValidatedEntryEAX
            && missingEax.FreshFrameMinus4Captured
            && missingEax.ECXLoaded == 0
            && missingEax.EAXRetained is null
            && !missingEax.FirstWriteReached
            && !missingEax.SecondWriteReached
            && !missingEax.FreshGlobalReadReached
            && missingEax.UnreachedFreshGlobalValueCaptureProvided
            && missingEax.StopAtRawFileOffsetExclusive == 0x012C8EEC);

        CapacityFinalCalleePairedFieldWritesPlan missingGlobal =
            CapacityFinalCalleePairedFieldWritesProjection.Project(Capture(
                validatedEntryEBP: 0x100,
                validatedEntryEAX: 0x200,
                freshFrameMinus4: 0x300,
                freshGlobalValue: null));
        check("A missing global capture is reached only after both ordered writes and preserves them",
            missingGlobal.Outcome
                == CapacityFinalCalleePairedFieldWritesOutcome.MissingFreshGlobalValue
            && missingGlobal.ECXLoaded == 0x300
            && missingGlobal.EAXRetained == 0x200
            && missingGlobal.FirstWriteReached
            && missingGlobal.FirstWriteAddress == 0x30C
            && missingGlobal.FirstWriteValue == 0x200
            && missingGlobal.SecondWriteReached
            && missingGlobal.SecondWriteAddress == 0x208
            && missingGlobal.SecondWriteValue == 0x300
            && missingGlobal.FreshGlobalReadReached
            && !missingGlobal.FreshGlobalValueCaptured
            && missingGlobal.EBXFresh is null
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C8EF2);

        CapacityFinalCalleePairedFieldWritesPlan zeros =
            CapacityFinalCalleePairedFieldWritesProjection.Project(Capture(
                validatedEntryEBP: 0,
                validatedEntryEAX: 0,
                freshFrameMinus4: 0,
                freshGlobalValue: 0));
        check("Captured zero registers and memory DWORDs remain distinct from missing captures",
            zeros.Outcome == CapacityFinalCalleePairedFieldWritesOutcome.Completed
            && zeros.FreshFrameMinus4Address == 0xFFFFFFFC
            && zeros.FreshFrameMinus4ReadReached
            && zeros.FreshFrameMinus4Captured
            && zeros.ECXLoaded == 0
            && zeros.EAXRetained == 0
            && zeros.FirstWriteReached
            && zeros.FirstWriteAddress == 0xC
            && zeros.FirstWriteValue == 0
            && zeros.SecondWriteReached
            && zeros.SecondWriteAddress == 8
            && zeros.SecondWriteValue == 0
            && zeros.FreshGlobalReadReached
            && zeros.FreshGlobalValueCaptured
            && zeros.EBXFresh == 0
            && zeros.StopAtRawFileOffsetExclusive == 0x012C8EF8);

        CapacityFinalCalleePairedFieldWritesPlan wrappedAndAliased =
            CapacityFinalCalleePairedFieldWritesProjection.Project(Capture(
                validatedEntryEBP: 0,
                validatedEntryEAX: 0xFFFFFFFC,
                freshFrameMinus4: 0xFFFFFFF8,
                freshGlobalValue: 0));
        check("DWORD address arithmetic wraps and the two scalar writes remain ordered when aliased",
            wrappedAndAliased.FreshFrameMinus4Address == 0xFFFFFFFC
            && wrappedAndAliased.FirstWriteAddress == 4
            && wrappedAndAliased.FirstWriteValue == 0xFFFFFFFC
            && wrappedAndAliased.SecondWriteAddress == 4
            && wrappedAndAliased.SecondWriteValue == 0xFFFFFFF8
            && wrappedAndAliased.FirstWriteReached
            && wrappedAndAliased.SecondWriteReached
            && wrappedAndAliased.StopAtRawFileOffsetExclusive == 0x012C8EF8
            && CapacityFinalCalleePairedFieldWritesProjection.DeriveFrameMinus4Address(0)
                == 0xFFFFFFFC
            && CapacityFinalCalleePairedFieldWritesProjection.DeriveFirstWriteAddress(
                0xFFFFFFF8) == 4
            && CapacityFinalCalleePairedFieldWritesProjection.DeriveSecondWriteAddress(
                0xFFFFFFFC) == 4);

        CapacityFinalCalleePairedFieldWritesPlan independentGlobal =
            CapacityFinalCalleePairedFieldWritesProjection.Project(Capture(
                validatedEntryEBP: 0x1000,
                validatedEntryEAX: 0x01EDADD0,
                freshFrameMinus4: 0x01EDADCC,
                freshGlobalValue: 0));
        check("The fresh global capture stays independent when both writes alias its absolute address",
            independentGlobal.FirstWriteAddress
                == CapacityFinalCalleePairedFieldWritesProjection.GlobalValuePeVirtualAddress
            && independentGlobal.SecondWriteAddress
                == CapacityFinalCalleePairedFieldWritesProjection.GlobalValuePeVirtualAddress
            && independentGlobal.FirstWriteValue == 0x01EDADD0
            && independentGlobal.SecondWriteValue == 0x01EDADCC
            && independentGlobal.FreshGlobalReadReached
            && independentGlobal.FreshGlobalValueCaptured
            && independentGlobal.EBXFresh == 0
            && independentGlobal.StopAtRawFileOffsetExclusive == 0x012C8EF8);

        CapacityFinalCalleePairedFieldWritesPlan missingEntryEbp =
            CapacityFinalCalleePairedFieldWritesProjection.Project(Capture(
                validatedEntryEBP: null,
                validatedEntryEAX: 0,
                freshFrameMinus4: 0,
                freshGlobalValue: 0));
        check("An absent validated EBP leaves the load and all later effects unreached",
            missingEntryEbp.Outcome
                == CapacityFinalCalleePairedFieldWritesOutcome.MissingValidatedEntryEBP
            && missingEntryEbp.FreshFrameMinus4Address is null
            && !missingEntryEbp.FreshFrameMinus4ReadReached
            && !missingEntryEbp.FreshFrameMinus4Captured
            && missingEntryEbp.UnreachedFreshFrameMinus4CaptureProvided
            && missingEntryEbp.ECXLoaded is null
            && !missingEntryEbp.FirstWriteReached
            && !missingEntryEbp.SecondWriteReached
            && !missingEntryEbp.FreshGlobalReadReached
            && missingEntryEbp.UnreachedValidatedEntryEAXCaptureProvided
            && missingEntryEbp.UnreachedFreshGlobalValueCaptureProvided
            && missingEntryEbp.StopAtRawFileOffsetExclusive == 0x012C8EE9);
    }

    private static CapacityFinalCalleePairedFieldWritesCapture Capture(
        uint? validatedEntryEBP = 0x1000,
        uint? validatedEntryEAX = 0x200,
        uint? freshFrameMinus4 = 0x300,
        uint? freshGlobalValue = 0x400) =>
        new(
            validatedEntryEBP,
            validatedEntryEAX,
            freshFrameMinus4,
            freshGlobalValue);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand)[]
        {
            (0x012C8EE9, "8B 4D FC", null),
            (0x012C8EEC, "89 41 0C", null),
            (0x012C8EEF, "89 48 08", null),
            (0x012C8EF2, "8B 1D D8 AD ED 01", 0x01EDADD8)
        };
        uint nextRawOffset =
            CapacityFinalCalleePairedFieldWritesProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleePairedFieldWritesInstructionEvidence instruction
            in CapacityFinalCalleePairedFieldWritesProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleePairedFieldWritesProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleePairedFieldWritesProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleePairedFieldWritesProjection.CapturedWindowBytes)
            && CapacityFinalCalleePairedFieldWritesProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.PeVirtualAddressOperand))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
