using Firefall.Reconstruction.Startup;

internal static class UnreliableReceiveHighCountCounterChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE range, raw-to-VA mapping, and instruction/branch boundaries match",
            UnreliableReceiveHighCountCounterProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && UnreliableReceiveHighCountCounterProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UnreliableReceiveHighCountCounterProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && UnreliableReceiveHighCountCounterProjection.EntryRawFileOffset == 0x00BE5BFE
            && UnreliableReceiveHighCountCounterProjection.EntryVirtualAddress == 0x00FE67FE
            && UnreliableReceiveHighCountCounterProjection.RawFileOffsetToVirtualAddress(0x00BE5BFE)
                == 0x00FE67FE
            && UnreliableReceiveHighCountCounterProjection.NonzeroDecrementBranchRawFileOffset
                == 0x00BE5C01
            && UnreliableReceiveHighCountCounterProjection.ZeroDecrementStoreRawFileOffset
                == 0x00BE5C03
            && UnreliableReceiveHighCountCounterProjection.ZeroDecrementJumpRawFileOffset
                == 0x00BE5C0A
            && UnreliableReceiveHighCountCounterProjection.Counter48IncrementRawFileOffset
                == 0x00BE5C0C
            && UnreliableReceiveHighCountCounterProjection.JoinRawFileOffset == 0x00BE5C0F
            && UnreliableReceiveHighCountCounterProjection.EndRawFileOffsetExclusive == 0x00BE5C0F
            && UnreliableReceiveHighCountCounterProjection.RawFileOffsetToVirtualAddress(0x00BE5C0F)
                == 0x00FE680F
            && UnreliableReceiveHighCountCounterProjection.CapturedWindowBytes
                == "FF 4F 4C 75 09 C7 47 48 00 00 00 00 EB 03 FF 47 48"
            && UnreliableReceiveHighCountCounterProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 17
            && InstructionEvidenceMatches());

        UnreliableReceiveHighCountCounterPlan invalidEntry =
            UnreliableReceiveHighCountCounterProjection.Project(
                new(
                    ValidatedEntry: false,
                    ReceiverEdi: 0,
                    IndependentCounter4CReload: 0,
                    Counter48IfNonzeroPostDecrement: uint.MaxValue));
        check("An invalid entry performs no counter read or write and leaves both captures unreached",
            invalidEntry.Outcome == UnreliableReceiveHighCountCounterOutcome.InvalidEntry
            && invalidEntry.Counter4CAddress is null
            && !invalidEntry.Counter4CRead
            && !invalidEntry.Counter4CWriteProjected
            && invalidEntry.Counter4CNewValue is null
            && invalidEntry.NonzeroPostDecrementBranchTaken is null
            && !invalidEntry.Counter48Read
            && !invalidEntry.Counter48WriteProjected
            && invalidEntry.UnreachedIndependentCounter4CReloadCaptureProvided
            && invalidEntry.UnreachedCounter48CaptureProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x00BE5BFE);

        UnreliableReceiveHighCountCounterPlan missingCounter4C =
            UnreliableReceiveHighCountCounterProjection.Project(
                new(
                    ValidatedEntry: true,
                    ReceiverEdi: 0,
                    IndependentCounter4CReload: null,
                    Counter48IfNonzeroPostDecrement: 7));
        check("A missing independent 4C reload blocks the DEC and all following effects, including at EDI zero",
            missingCounter4C.Outcome
                == UnreliableReceiveHighCountCounterOutcome.MissingIndependentCounter4CReload
            && missingCounter4C.Counter4CAddress == 0x4C
            && !missingCounter4C.Counter4CRead
            && missingCounter4C.Counter4COldValue is null
            && missingCounter4C.Counter4CNewValue is null
            && !missingCounter4C.Counter4CWriteProjected
            && missingCounter4C.Counter4CIsZeroAfterDecrement is null
            && missingCounter4C.NonzeroPostDecrementBranchTaken is null
            && missingCounter4C.Counter48Address is null
            && !missingCounter4C.Counter48Read
            && !missingCounter4C.Counter48WriteProjected
            && !missingCounter4C.UnreachedIndependentCounter4CReloadCaptureProvided
            && missingCounter4C.UnreachedCounter48CaptureProvided
            && missingCounter4C.StopAtRawFileOffsetExclusive == 0x00BE5BFE);

        UnreliableReceiveHighCountCounterPlan zeroWrapsToNonzero =
            UnreliableReceiveHighCountCounterProjection.Project(
                new(
                    ValidatedEntry: true,
                    ReceiverEdi: 0,
                    IndependentCounter4CReload: 0,
                    Counter48IfNonzeroPostDecrement: 9));
        UnreliableReceiveHighCountCounterPlan oneStoresZeroWithoutReadingCounter48 =
            UnreliableReceiveHighCountCounterProjection.Project(
                new(
                    ValidatedEntry: true,
                    ReceiverEdi: 0xFFFFFFB4,
                    IndependentCounter4CReload: 1,
                    Counter48IfNonzeroPostDecrement: uint.MaxValue));
        UnreliableReceiveHighCountCounterPlan maxDecrementsNormally =
            UnreliableReceiveHighCountCounterProjection.Project(
                new(
                    ValidatedEntry: true,
                    ReceiverEdi: 0x1000,
                    IndependentCounter4CReload: uint.MaxValue,
                    Counter48IfNonzeroPostDecrement: 6));
        check("DEC wrap selects the exact zero/nonzero path; zero EDI is a valid captured address",
            zeroWrapsToNonzero.Outcome
                == UnreliableReceiveHighCountCounterOutcome.Counter4CDecrementedAndCounter48Incremented
            && zeroWrapsToNonzero.Counter4CAddress == 0x4C
            && zeroWrapsToNonzero.Counter4CRead
            && zeroWrapsToNonzero.Counter4COldValue == 0
            && zeroWrapsToNonzero.Counter4CNewValue == uint.MaxValue
            && zeroWrapsToNonzero.Counter4CWriteProjected
            && zeroWrapsToNonzero.Counter4CIsZeroAfterDecrement == false
            && zeroWrapsToNonzero.NonzeroPostDecrementBranchTaken == true
            && zeroWrapsToNonzero.Counter48Address == 0x48
            && zeroWrapsToNonzero.Counter48Read
            && zeroWrapsToNonzero.Counter48OldValue == 9
            && zeroWrapsToNonzero.Counter48NewValue == 10
            && zeroWrapsToNonzero.Counter48WriteProjected
            && !zeroWrapsToNonzero.Counter48ZeroStoreProjected
            && !zeroWrapsToNonzero.UnreachedCounter48CaptureProvided
            && zeroWrapsToNonzero.StopAtRawFileOffsetExclusive == 0x00BE5C0F
            && oneStoresZeroWithoutReadingCounter48.Outcome
                == UnreliableReceiveHighCountCounterOutcome.Counter4CDecrementedAndCounter48Zeroed
            && oneStoresZeroWithoutReadingCounter48.Counter4CAddress == 0
            && oneStoresZeroWithoutReadingCounter48.Counter4COldValue == 1
            && oneStoresZeroWithoutReadingCounter48.Counter4CNewValue == 0
            && oneStoresZeroWithoutReadingCounter48.Counter4CWriteProjected
            && oneStoresZeroWithoutReadingCounter48.Counter4CIsZeroAfterDecrement == true
            && oneStoresZeroWithoutReadingCounter48.NonzeroPostDecrementBranchTaken == false
            && oneStoresZeroWithoutReadingCounter48.Counter48Address == 0xFFFFFFFC
            && !oneStoresZeroWithoutReadingCounter48.Counter48Read
            && oneStoresZeroWithoutReadingCounter48.Counter48OldValue is null
            && oneStoresZeroWithoutReadingCounter48.Counter48NewValue == 0
            && oneStoresZeroWithoutReadingCounter48.Counter48WriteProjected
            && oneStoresZeroWithoutReadingCounter48.Counter48ZeroStoreProjected
            && oneStoresZeroWithoutReadingCounter48.UnreachedCounter48CaptureProvided
            && oneStoresZeroWithoutReadingCounter48.StopAtRawFileOffsetExclusive == 0x00BE5C0F
            && maxDecrementsNormally.Counter4COldValue == uint.MaxValue
            && maxDecrementsNormally.Counter4CNewValue == 0xFFFFFFFE
            && maxDecrementsNormally.NonzeroPostDecrementBranchTaken == true
            && maxDecrementsNormally.Counter48OldValue == 6
            && maxDecrementsNormally.Counter48NewValue == 7
            && maxDecrementsNormally.Counter48WriteProjected);

        UnreliableReceiveHighCountCounterPlan missingCounter48 =
            UnreliableReceiveHighCountCounterProjection.Project(
                new(
                    ValidatedEntry: true,
                    ReceiverEdi: 0x1000,
                    IndependentCounter4CReload: 2,
                    Counter48IfNonzeroPostDecrement: null));
        check("A missing 48 capture preserves the completed 4C decrement and nonzero branch without a 48 write",
            missingCounter48.Outcome
                == UnreliableReceiveHighCountCounterOutcome.MissingCounter48ForNonzeroPostDecrement
            && missingCounter48.Counter4CAddress == 0x104C
            && missingCounter48.Counter4CRead
            && missingCounter48.Counter4COldValue == 2
            && missingCounter48.Counter4CNewValue == 1
            && missingCounter48.Counter4CWriteProjected
            && missingCounter48.Counter4CIsZeroAfterDecrement == false
            && missingCounter48.NonzeroPostDecrementBranchTaken == true
            && missingCounter48.Counter48Address == 0x1048
            && !missingCounter48.Counter48Read
            && missingCounter48.Counter48OldValue is null
            && missingCounter48.Counter48NewValue is null
            && !missingCounter48.Counter48WriteProjected
            && !missingCounter48.Counter48ZeroStoreProjected
            && !missingCounter48.UnreachedCounter48CaptureProvided
            && missingCounter48.StopAtRawFileOffsetExclusive == 0x00BE5C0C);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? BranchTarget)[]
        {
            (0x00BE5BFE, 0x00FE67FE, "FF 4F 4C", null),
            (0x00BE5C01, 0x00FE6801, "75 09", 0x00BE5C0C),
            (0x00BE5C03, 0x00FE6803, "C7 47 48 00 00 00 00", null),
            (0x00BE5C0A, 0x00FE680A, "EB 03", 0x00BE5C0F),
            (0x00BE5C0C, 0x00FE680C, "FF 47 48", null)
        };
        return UnreliableReceiveHighCountCounterProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.VirtualAddress,
                        instruction.Bytes, instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected)
            && UnreliableReceiveHighCountCounterProjection.RawFileOffsetToVirtualAddress(0x00BE5C01)
                == 0x00FE6801
            && UnreliableReceiveHighCountCounterProjection.RawFileOffsetToVirtualAddress(0x00BE5C0C)
                == 0x00FE680C
            && UnreliableReceiveHighCountCounterProjection.NonzeroDecrementBranchTargetRawFileOffset
                == 0x00BE5C0C
            && UnreliableReceiveHighCountCounterProjection.ZeroDecrementBranchTargetRawFileOffset
                == 0x00BE5C0F
            && UnreliableReceiveHighCountCounterProjection.ReceiverCounter4COffset == 0x4C
            && UnreliableReceiveHighCountCounterProjection.ReceiverCounter48Offset == 0x48;
    }
}
