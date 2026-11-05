using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeLargeSizeFieldsChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The large-size instruction window, addresses, bytes, and endpoints match",
            CapacityFinalCalleeLargeSizeFieldsProjection.EntryRawFileOffset
                == 0x012C8F76
            && CapacityFinalCalleeLargeSizeFieldsProjection.EntryVirtualAddress
                == 0x016C9B76
            && CapacityFinalCalleeLargeSizeFieldsProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeLargeSizeFieldsProjection.EntryRawFileOffset)
                == CapacityFinalCalleeLargeSizeFieldsProjection.EntryVirtualAddress
            && CapacityFinalCalleeLargeSizeFieldsProjection.BranchExitRawFileOffset
                == 0x012C8E76
            && CapacityFinalCalleeLargeSizeFieldsProjection.EsiPlusCEqualsExitRawFileOffset
                == 0x012C8FA8
            && CapacityFinalCalleeLargeSizeFieldsProjection.PairedWritesExitRawFileOffset
                == 0x012C8FF8
            && CapacityFinalCalleeLargeSizeFieldsProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8FA8
            && Normalize(CapacityFinalCalleeLargeSizeFieldsProjection.CapturedWindowBytes)
                == "8B46188945EC8B460C3BC674258B4E083BCB0F82E8FEFFFF"
                    + "39710C0F85DFFEFFFF3970080F85D6FEFFFF89410C894808EB50"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeLargeSizeFieldsPlan missingEntryEsi =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                validatedEntryESI: null,
                freshEsiPlus18: 0,
                freshEsiPlusC: 0,
                freshEsiPlus8: 0,
                freshEcxPlusC: 0,
                freshEaxPlus8: 0));
        CapacityFinalCalleeLargeSizeFieldsPlan missingFirstRead =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                freshEsiPlus18: null));
        CapacityFinalCalleeLargeSizeFieldsPlan missingEntryEbp =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                validatedEntryEBP: null));
        CapacityFinalCalleeLargeSizeFieldsPlan missingSecondRead =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                freshEsiPlusC: null));
        check("Entry gates and the first two reads stop in native order while preserving prior frame work",
            missingEntryEsi.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.MissingValidatedEntryESI
            && !missingEntryEsi.FreshEsiPlus18ReadReached
            && missingEntryEsi.UnreachedFreshEsiPlus18CaptureProvided
            && missingEntryEsi.UnreachedValidatedEntryEBPCaptureProvided
            && missingEntryEsi.UnreachedValidatedEntryEBXCaptureProvided
            && missingEntryEsi.StopAtRawFileOffsetExclusive == 0x012C8F76
            && missingFirstRead.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.MissingFreshEsiPlus18
            && missingFirstRead.FreshEsiPlus18ReadReached
            && !missingFirstRead.FreshEsiPlus18Captured
            && missingFirstRead.FreshEsiPlus18Address == 0x1018
            && missingFirstRead.UnreachedValidatedEntryEBPCaptureProvided
            && missingFirstRead.UnreachedFreshEsiPlusCCaptureProvided
            && missingFirstRead.StopAtRawFileOffsetExclusive == 0x012C8F76
            && missingEntryEbp.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.MissingValidatedEntryEBP
            && missingEntryEbp.FreshEsiPlus18Captured
            && missingEntryEbp.EaxLoadedFromEsiPlus18 == 0x3000
            && !missingEntryEbp.FrameWriteReached
            && missingEntryEbp.UnreachedValidatedEntryEBPCaptureProvided == false
            && missingEntryEbp.StopAtRawFileOffsetExclusive == 0x012C8F79
            && missingSecondRead.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.MissingFreshEsiPlusC
            && missingSecondRead.FrameWriteReached
            && missingSecondRead.FrameWriteAddress == 0x1FEC
            && missingSecondRead.FrameWriteValue == 0x3000
            && missingSecondRead.FreshEsiPlusCReadReached
            && !missingSecondRead.FreshEsiPlusCCaptured
            && missingSecondRead.UnreachedFreshEsiPlus8CaptureProvided
            && missingSecondRead.StopAtRawFileOffsetExclusive == 0x012C8F7C);

        CapacityFinalCalleeLargeSizeFieldsPlan equalEsiPlusC =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                freshEsiPlusC: 0x1000,
                freshEsiPlus8: 0,
                freshEcxPlusC: 0,
                freshEaxPlus8: 0));
        CapacityFinalCalleeLargeSizeFieldsPlan missingEsiPlus8 =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                freshEsiPlus8: null));
        CapacityFinalCalleeLargeSizeFieldsPlan missingEntryEbx =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                validatedEntryEBX: null));
        CapacityFinalCalleeLargeSizeFieldsPlan unsignedBelow =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                freshEsiPlus8: 0xF,
                freshEcxPlusC: 0,
                freshEaxPlus8: 0));
        check("The equality exit, missing compare inputs, and unsigned-below exit remain distinct",
            equalEsiPlusC.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.EsiPlusCEqualsEntryESI
            && equalEsiPlusC.EsiPlusCCompareReached
            && equalEsiPlusC.EsiPlusCEqualsEntryESI == true
            && equalEsiPlusC.FrameWriteReached
            && !equalEsiPlusC.FreshEsiPlus8ReadReached
            && equalEsiPlusC.UnreachedFreshEsiPlus8CaptureProvided
            && equalEsiPlusC.UnreachedValidatedEntryEBXCaptureProvided
            && equalEsiPlusC.StopAtRawFileOffsetExclusive == 0x012C8FA8
            && missingEsiPlus8.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.MissingFreshEsiPlus8
            && missingEsiPlus8.FreshEsiPlus8ReadReached
            && !missingEsiPlus8.FreshEsiPlus8Captured
            && missingEsiPlus8.FreshEsiPlus8Address == 0x1008
            && missingEsiPlus8.FrameWriteReached
            && missingEsiPlus8.StopAtRawFileOffsetExclusive == 0x012C8F83
            && missingEntryEbx.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.MissingValidatedEntryEBX
            && missingEntryEbx.FreshEsiPlus8Captured
            && missingEntryEbx.EcxLoadedFromEsiPlus8 == 0x10
            && !missingEntryEbx.EcxEntryEbxCompareReached
            && missingEntryEbx.UnreachedFreshEcxPlusCCaptureProvided
            && missingEntryEbx.StopAtRawFileOffsetExclusive == 0x012C8F86
            && unsignedBelow.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.EsiPlus8UnsignedBelowEntryEBX
            && unsignedBelow.EcxEntryEbxCompareReached
            && unsignedBelow.EsiPlus8UnsignedBelowEntryEBX == true
            && !unsignedBelow.FreshEcxPlusCReadReached
            && unsignedBelow.UnreachedFreshEcxPlusCCaptureProvided
            && unsignedBelow.UnreachedFreshEaxPlus8CaptureProvided
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeSizeFieldsPlan missingEcxPlusC =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                freshEcxPlusC: null));
        CapacityFinalCalleeLargeSizeFieldsPlan ecxPlusCMismatch =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                freshEcxPlusC: 0x1001,
                freshEaxPlus8: 0));
        CapacityFinalCalleeLargeSizeFieldsPlan missingEaxPlus8 =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                freshEaxPlus8: null));
        CapacityFinalCalleeLargeSizeFieldsPlan eaxPlus8Mismatch =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                freshEaxPlus8: 0x1001));
        check("The two fresh compare reads distinguish missing data from their bounded mismatch branches",
            missingEcxPlusC.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.MissingFreshEcxPlusC
            && missingEcxPlusC.FreshEcxPlusCReadReached
            && missingEcxPlusC.FreshEcxPlusCAddress == 0x1C
            && !missingEcxPlusC.FreshEcxPlusCCaptured
            && missingEcxPlusC.FrameWriteReached
            && missingEcxPlusC.StopAtRawFileOffsetExclusive == 0x012C8F8E
            && ecxPlusCMismatch.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.EcxPlusCNotEqualEntryESI
            && ecxPlusCMismatch.EcxPlusCEntryEsiCompareReached
            && ecxPlusCMismatch.EcxPlusCEqualsEntryESI == false
            && !ecxPlusCMismatch.FreshEaxPlus8ReadReached
            && ecxPlusCMismatch.UnreachedFreshEaxPlus8CaptureProvided
            && ecxPlusCMismatch.StopAtRawFileOffsetExclusive == 0x012C8E76
            && missingEaxPlus8.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.MissingFreshEaxPlus8
            && missingEaxPlus8.FreshEaxPlus8ReadReached
            && missingEaxPlus8.FreshEaxPlus8Address == 0x1009
            && !missingEaxPlus8.FreshEaxPlus8Captured
            && missingEaxPlus8.StopAtRawFileOffsetExclusive == 0x012C8F97
            && eaxPlus8Mismatch.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.EaxPlus8NotEqualEntryESI
            && eaxPlus8Mismatch.EaxPlus8EntryEsiCompareReached
            && eaxPlus8Mismatch.EaxPlus8EqualsEntryESI == false
            && !eaxPlus8Mismatch.FirstPairedWriteReached
            && eaxPlus8Mismatch.UnreachedFreshEaxPlus8CaptureProvided == false
            && eaxPlus8Mismatch.StopAtRawFileOffsetExclusive == 0x012C8E76);

        CapacityFinalCalleeLargeSizeFieldsPlan zeroValues =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                validatedEntryESI: 0,
                validatedEntryEBP: 0,
                validatedEntryEBX: 0,
                freshEsiPlus18: 0,
                freshEsiPlusC: 1,
                freshEsiPlus8: 0,
                freshEcxPlusC: 0,
                freshEaxPlus8: 0));
        CapacityFinalCalleeLargeSizeFieldsPlan wrapped =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                validatedEntryESI: 0xFFFFFFF8,
                validatedEntryEBP: 0x10,
                validatedEntryEBX: 0xFFFFFFFC,
                freshEsiPlus18: 0,
                freshEsiPlusC: 0xFFFFFFFC,
                freshEsiPlus8: 0xFFFFFFFC,
                freshEcxPlusC: 0xFFFFFFF8,
                freshEaxPlus8: 0xFFFFFFF8));
        check("Captured zero values and wrapped DWORD calculations retain native scalar results",
            zeroValues.Outcome == CapacityFinalCalleeLargeSizeFieldsOutcome.Completed
            && zeroValues.FreshEsiPlus18Captured
            && zeroValues.EaxLoadedFromEsiPlus18 == 0
            && zeroValues.FrameWriteAddress == 0xFFFFFFEC
            && zeroValues.FrameWriteValue == 0
            && zeroValues.EsiPlusCEqualsEntryESI == false
            && zeroValues.EsiPlus8UnsignedBelowEntryEBX == false
            && zeroValues.EcxPlusCEqualsEntryESI == true
            && zeroValues.EaxPlus8EqualsEntryESI == true
            && zeroValues.FirstPairedWriteReached
            && zeroValues.FirstPairedWriteAddress == 0xC
            && zeroValues.FirstPairedWriteValue == 1
            && zeroValues.SecondPairedWriteAddress == 9
            && zeroValues.SecondPairedWriteValue == 0
            && zeroValues.StopAtRawFileOffsetExclusive == 0x012C8FF8
            && wrapped.Outcome == CapacityFinalCalleeLargeSizeFieldsOutcome.Completed
            && wrapped.FreshEsiPlus18Address == 0x10
            && wrapped.FrameWriteAddress == 0xFFFFFFFC
            && wrapped.FreshEsiPlusCAddress == 4
            && wrapped.FreshEsiPlus8Address == 0
            && wrapped.FreshEcxPlusCAddress == 8
            && wrapped.FreshEaxPlus8Address == 4
            && wrapped.FirstPairedWriteAddress == 8
            && wrapped.SecondPairedWriteAddress == 4
            && CapacityFinalCalleeLargeSizeFieldsProjection.DeriveFrameWriteAddress(0)
                == 0xFFFFFFEC
            && CapacityFinalCalleeLargeSizeFieldsProjection.DeriveEsiPlus18Address(
                0xFFFFFFF8) == 0x10);

        CapacityFinalCalleeLargeSizeFieldsPlan independentlyCaptured =
            CapacityFinalCalleeLargeSizeFieldsProjection.Project(Capture(
                validatedEntryESI: 0x1000,
                validatedEntryEBP: 0x102C,
                validatedEntryEBX: 0x2FFC,
                freshEsiPlus18: 0xAAAA,
                freshEsiPlusC: 0x3000,
                freshEsiPlus8: 0x2FFC,
                freshEcxPlusC: 0x1000,
                freshEaxPlus8: 0x1000));
        check("Fresh captures remain independent across frame and aliased paired-write addresses",
            independentlyCaptured.Outcome
                == CapacityFinalCalleeLargeSizeFieldsOutcome.Completed
            && independentlyCaptured.FrameWriteAddress == 0x1018
            && independentlyCaptured.FrameWriteValue == 0xAAAA
            && independentlyCaptured.FreshEsiPlus18Address
                == independentlyCaptured.FrameWriteAddress
            && independentlyCaptured.FreshEsiPlusCAddress
                == independentlyCaptured.FrameWriteAddress - 0xC
            && independentlyCaptured.FirstPairedWriteAddress == 0x3008
            && independentlyCaptured.SecondPairedWriteAddress == 0x3008
            && independentlyCaptured.FirstPairedWriteValue == 0x3000
            && independentlyCaptured.SecondPairedWriteValue == 0x2FFC
            && independentlyCaptured.FreshEcxPlusCAddress
                == independentlyCaptured.FirstPairedWriteAddress
            && independentlyCaptured.FreshEaxPlus8Address
                == independentlyCaptured.SecondPairedWriteAddress
            && independentlyCaptured.FreshEcxPlusCValue == 0x1000
            && independentlyCaptured.FreshEaxPlus8Value == 0x1000
            && independentlyCaptured.StopAtRawFileOffsetExclusive == 0x012C8FF8);
    }

    private static CapacityFinalCalleeLargeSizeFieldsCapture Capture(
        uint? validatedEntryESI = 0x1000,
        uint? validatedEntryEBP = 0x2000,
        uint? validatedEntryEBX = 0x10,
        uint? freshEsiPlus18 = 0x3000,
        uint? freshEsiPlusC = 0x1001,
        uint? freshEsiPlus8 = 0x10,
        uint? freshEcxPlusC = 0x1000,
        uint? freshEaxPlus8 = 0x1000) =>
        new(
            validatedEntryESI,
            validatedEntryEBP,
            validatedEntryEBX,
            freshEsiPlus18,
            freshEsiPlusC,
            freshEsiPlus8,
            freshEcxPlusC,
            freshEaxPlus8);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes)[]
        {
            (0x012C8F76, "8B 46 18"),
            (0x012C8F79, "89 45 EC"),
            (0x012C8F7C, "8B 46 0C"),
            (0x012C8F7F, "3B C6"),
            (0x012C8F81, "74 25"),
            (0x012C8F83, "8B 4E 08"),
            (0x012C8F86, "3B CB"),
            (0x012C8F88, "0F 82 E8 FE FF FF"),
            (0x012C8F8E, "39 71 0C"),
            (0x012C8F91, "0F 85 DF FE FF FF"),
            (0x012C8F97, "39 70 08"),
            (0x012C8F9A, "0F 85 D6 FE FF FF"),
            (0x012C8FA0, "89 41 0C"),
            (0x012C8FA3, "89 48 08"),
            (0x012C8FA6, "EB 50")
        };
        uint nextRawOffset =
            CapacityFinalCalleeLargeSizeFieldsProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeLargeSizeFieldsInstructionEvidence instruction
            in CapacityFinalCalleeLargeSizeFieldsProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeLargeSizeFieldsProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeLargeSizeFieldsProjection.BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeLargeSizeFieldsProjection.CapturedWindowBytes)
            && CapacityFinalCalleeLargeSizeFieldsProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
