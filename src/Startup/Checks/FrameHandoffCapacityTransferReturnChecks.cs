using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityTransferReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE and exact 19-byte raw IDA window end before the next instruction",
            FrameHandoffCapacityTransferReturnProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityTransferReturnProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityTransferReturnProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityTransferReturnProjection.EntryRawFileOffset == 0x00BE6BFD
            && FrameHandoffCapacityTransferReturnProjection.EntryVirtualAddress == 0x00FE77FD
            && FrameHandoffCapacityTransferReturnProjection.NormalBoundaryRawFileOffsetExclusive
                == 0x00BE6C10
            && FrameHandoffCapacityTransferReturnProjection.NormalBoundaryVirtualAddressExclusive
                == 0x00FE7810
            && FrameHandoffCapacityTransferReturnProjection.AboveCapacityDestinationRawFileOffset
                == 0x00BE6C57
            && FrameHandoffCapacityTransferReturnProjection.AboveCapacityDestinationVirtualAddress
                == 0x00FE7857
            && FrameHandoffCapacityTransferReturnProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityTransferReturnProjection.EntryRawFileOffset)
                == FrameHandoffCapacityTransferReturnProjection.EntryVirtualAddress
            && FrameHandoffCapacityTransferReturnProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityTransferReturnProjection.NormalBoundaryRawFileOffsetExclusive)
                == FrameHandoffCapacityTransferReturnProjection.NormalBoundaryVirtualAddressExclusive
            && Normalize(FrameHandoffCapacityTransferReturnProjection.CapturedWindowBytes)
                == "8B4B0483C40C8D14078B7DF88955F03BFE7747"
            && FrameHandoffCapacityTransferReturnProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 19
            && InstructionEvidenceMatches());

        FrameHandoffCapacityTransferReturnPlan invalidEntry =
            FrameHandoffCapacityTransferReturnProjection.Project(new(
                ValidatedNormalEntry: false,
                EntryEbx: 0x1000,
                EntryEbp: 0x2000,
                EntryEdi: 0x3000,
                EntryEsi: 0x4000,
                HelperReturnEax: 1,
                Receiver4: 2,
                FrameMinus8: 3));
        check("An unvalidated entry does not reach the fresh receiver read or cleanup",
            invalidEntry.Outcome
                == FrameHandoffCapacityTransferReturnOutcome.EntryNotValidated
            && invalidEntry.Reads.IsEmpty
            && invalidEntry.MemoryAccesses.IsEmpty
            && !invalidEntry.Receiver4ReadReached
            && !invalidEntry.StackCleanupReached
            && invalidEntry.UnreachedReceiver4CaptureProvided
            && invalidEntry.UnreachedHelperReturnEaxCaptureProvided
            && invalidEntry.UnreachedFrameMinus8CaptureProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x00BE6BFD);

        FrameHandoffCapacityTransferReturnPlan missingReceiver4 =
            FrameHandoffCapacityTransferReturnProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbx: 0x1000,
                EntryEbp: 0x2000,
                EntryEdi: 0x3000,
                EntryEsi: 0x4000,
                HelperReturnEax: 1,
                Receiver4: null,
                FrameMinus8: 3));
        check("Missing receiver-4 stops at its fresh read before cleanup",
            missingReceiver4.Outcome
                == FrameHandoffCapacityTransferReturnOutcome.MissingReceiver4Capture
            && missingReceiver4.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityTransferReturnRead(0x00BE6BFD, 0x1004, null)
            })
            && missingReceiver4.MemoryAccesses.SequenceEqual(new[]
            {
                new FrameHandoffCapacityTransferReturnMemoryAccess(
                    FrameHandoffCapacityTransferReturnMemoryAccessKind.Read,
                    0x00BE6BFD, 0x1004, null)
            })
            && missingReceiver4.Receiver4ReadReached
            && !missingReceiver4.Receiver4Captured
            && !missingReceiver4.StackCleanupReached
            && missingReceiver4.UnreachedHelperReturnEaxCaptureProvided
            && missingReceiver4.UnreachedFrameMinus8CaptureProvided
            && missingReceiver4.StopAtRawFileOffsetExclusive == 0x00BE6BFD);

        FrameHandoffCapacityTransferReturnPlan missingHelperEax =
            FrameHandoffCapacityTransferReturnProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbx: 0x1000,
                EntryEbp: 0x2000,
                EntryEdi: 0xFFFFFFFE,
                EntryEsi: 0x4000,
                HelperReturnEax: null,
                Receiver4: 0,
                FrameMinus8: 3));
        check("Missing helper EAX stops at the LEA after preserving the read and cleanup",
            missingHelperEax.Outcome
                == FrameHandoffCapacityTransferReturnOutcome.MissingHelperReturnEaxCapture
            && missingHelperEax.Reads.Single()
                == new FrameHandoffCapacityTransferReturnRead(0x00BE6BFD, 0x1004, 0)
            && missingHelperEax.Receiver4Captured
            && missingHelperEax.StackCleanupReached
            && !missingHelperEax.EdxLeaReached
            && !missingHelperEax.FrameMinus8ReadReached
            && missingHelperEax.UnreachedFrameMinus8CaptureProvided
            && missingHelperEax.StopAtRawFileOffsetExclusive == 0x00BE6C03);

        FrameHandoffCapacityTransferReturnPlan missingFrameMinus8 =
            FrameHandoffCapacityTransferReturnProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbx: 0x2000,
                EntryEbp: 0x3000,
                EntryEdi: 0xFFFFFFFE,
                EntryEsi: 0,
                HelperReturnEax: 5,
                Receiver4: uint.MaxValue,
                FrameMinus8: null));
        check("Missing independent frame-minus-eight read keeps cleanup and LEA but has no store",
            missingFrameMinus8.Outcome
                == FrameHandoffCapacityTransferReturnOutcome.MissingFrameMinus8Capture
            && missingFrameMinus8.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityTransferReturnRead(0x00BE6BFD, 0x2004, uint.MaxValue),
                new FrameHandoffCapacityTransferReturnRead(0x00BE6C06, 0x2FF8, null)
            })
            && missingFrameMinus8.StackCleanupReached
            && missingFrameMinus8.EdxAfterLea == 3
            && missingFrameMinus8.FrameMinus8ReadReached
            && !missingFrameMinus8.FrameMinus8Captured
            && !missingFrameMinus8.FrameMinus10StoreReached
            && !missingFrameMinus8.CompareReached
            && missingFrameMinus8.StopAtRawFileOffsetExclusive == 0x00BE6C06);

        FrameHandoffCapacityTransferReturnPlan unsignedAbove =
            Project(entryEsi: 1, frameMinus8: 0x80000000);
        FrameHandoffCapacityTransferReturnPlan unsignedNotAbove =
            Project(entryEsi: uint.MaxValue, frameMinus8: 0x7FFFFFFF);
        check("The CMP and JA use an unsigned comparison and select the exact bounded successors",
            unsignedAbove.Outcome
                == FrameHandoffCapacityTransferReturnOutcome
                    .FrameMinus8UnsignedGreaterThanEntryEsi
            && unsignedAbove.FrameMinus8UnsignedGreaterThanEntryEsi == true
            && unsignedAbove.BranchTaken == true
            && unsignedAbove.SelectedDestinationRawFileOffset == 0x00BE6C57
            && unsignedAbove.StopAtRawFileOffsetExclusive == 0x00BE6C57
            && unsignedNotAbove.Outcome
                == FrameHandoffCapacityTransferReturnOutcome
                    .FrameMinus8UnsignedLessThanOrEqualToEntryEsi
            && unsignedNotAbove.FrameMinus8UnsignedGreaterThanEntryEsi == false
            && unsignedNotAbove.BranchTaken == false
            && unsignedNotAbove.SelectedDestinationRawFileOffset == 0x00BE6C10
            && unsignedNotAbove.StopAtRawFileOffsetExclusive == 0x00BE6C10);

        FrameHandoffCapacityTransferReturnPlan wrapped =
            FrameHandoffCapacityTransferReturnProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbx: uint.MaxValue,
                EntryEbp: 3,
                EntryEdi: uint.MaxValue,
                EntryEsi: 0,
                HelperReturnEax: 2,
                Receiver4: 0,
                FrameMinus8: 0));
        FrameHandoffCapacityTransferReturnPlan zeroAddresses =
            FrameHandoffCapacityTransferReturnProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbx: uint.MaxValue - 3,
                EntryEbp: 0x10,
                EntryEdi: 0,
                EntryEsi: 0,
                HelperReturnEax: 0,
                Receiver4: 0,
                FrameMinus8: 0));
        FrameHandoffCapacityTransferReturnPlan zeroFrameMinus8Address =
            FrameHandoffCapacityTransferReturnProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbx: 0,
                EntryEbp: 8,
                EntryEdi: 0,
                EntryEsi: 0,
                HelperReturnEax: 0,
                Receiver4: 0,
                FrameMinus8: 0));
        check("Address and LEA arithmetic wrap as DWORDs, and captured zero values/addresses are valid",
            FrameHandoffCapacityTransferReturnProjection.Receiver4ReadAddress(uint.MaxValue) == 3
            && FrameHandoffCapacityTransferReturnProjection.FrameMinus8Address(3)
                == 0xFFFFFFFB
            && FrameHandoffCapacityTransferReturnProjection.FrameMinus10Address(3)
                == 0xFFFFFFF3
            && wrapped.Reads[0].Address == 3
            && wrapped.MemoryAccesses[^1].Address == 0xFFFFFFF3
            && wrapped.EdxAfterLea == 1
            && wrapped.FrameMinus10StoreValue == 1
            && wrapped.FrameMinus8UnsignedGreaterThanEntryEsi == false
            && zeroAddresses.Reads[0].Address == 0
            && zeroAddresses.Reads[1].Address == 8
            && zeroAddresses.FrameMinus10StoreAddress == 0
            && zeroAddresses.FrameMinus8Captured
            && zeroAddresses.FrameMinus10StoreReached
            && zeroFrameMinus8Address.Reads[1].Address == 0
            && zeroFrameMinus8Address.FrameMinus8Captured);
    }

    private static FrameHandoffCapacityTransferReturnPlan Project(uint entryEsi, uint frameMinus8) =>
        FrameHandoffCapacityTransferReturnProjection.Project(new(
            ValidatedNormalEntry: true,
            EntryEbx: 0x3000,
            EntryEbp: 0x4000,
            EntryEdi: 0x10,
            EntryEsi: entryEsi,
            HelperReturnEax: 0x20,
            Receiver4: 0,
            FrameMinus8: frameMinus8));

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x00BE6BFD, "8B 4B 04"),
            (0x00BE6C00, "83 C4 0C"),
            (0x00BE6C03, "8D 14 07"),
            (0x00BE6C06, "8B 7D F8"),
            (0x00BE6C09, "89 55 F0"),
            (0x00BE6C0C, "3B FE"),
            (0x00BE6C0E, "77 47")
        };
        uint nextRawOffset = FrameHandoffCapacityTransferReturnProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (FrameHandoffCapacityTransferReturnInstructionEvidence instruction
            in FrameHandoffCapacityTransferReturnProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacityTransferReturnProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == FrameHandoffCapacityTransferReturnProjection.NormalBoundaryRawFileOffsetExclusive
            && bytes == Normalize(FrameHandoffCapacityTransferReturnProjection.CapturedWindowBytes)
            && FrameHandoffCapacityTransferReturnProjection.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
