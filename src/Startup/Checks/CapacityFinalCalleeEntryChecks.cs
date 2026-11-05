using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeEntryChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The entry bytes, branch target, and PE virtual addresses remain distinct",
            CapacityFinalCalleeEntryProjection.EntryRawFileOffset == 0x012C8D60
            && CapacityFinalCalleeEntryProjection.EntryVirtualAddress == 0x016C9960
            && CapacityFinalCalleeEntryProjection.NonzeroPrefixBoundaryRawFileOffsetExclusive
                == 0x012C8D78
            && CapacityFinalCalleeEntryProjection.NonzeroPrefixBoundaryVirtualAddressExclusive
                == 0x016C9978
            && CapacityFinalCalleeEntryProjection.ZeroBranchDestinationRawFileOffset
                == 0x012C9458
            && CapacityFinalCalleeEntryProjection.ZeroBranchDestinationVirtualAddress
                == 0x016CA058
            && CapacityFinalCalleeEntryProjection.ZeroReturnRawFileOffset == 0x012C945B
            && CapacityFinalCalleeEntryProjection.ZeroReturnBoundaryRawFileOffsetExclusive
                == 0x012C945C
            && CapacityFinalCalleeEntryProjection.ZeroReturnBoundaryVirtualAddressExclusive
                == 0x016CA05C
            && CapacityFinalCalleeEntryProjection.TestByteVirtualAddress == 0x01EDAF84
            && CapacityFinalCalleeEntryProjection.TestMask == 0x02
            && CapacityFinalCalleeEntryProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeEntryProjection.EntryRawFileOffset)
                == CapacityFinalCalleeEntryProjection.EntryVirtualAddress
            && CapacityFinalCalleeEntryProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeEntryProjection.ZeroReturnBoundaryRawFileOffsetExclusive)
                == CapacityFinalCalleeEntryProjection.ZeroReturnBoundaryVirtualAddressExclusive
            && Normalize(CapacityFinalCalleeEntryProjection.CapturedWindowBytes)
                == "558BEC83EC3085C90F84EA060000F60584AFED0102535657"
            && Normalize(CapacityFinalCalleeEntryProjection.ZeroEpilogueBytes) == "8BE55DC3"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeEntryPlan invalidEntry =
            CapacityFinalCalleeEntryProjection.Project(Capture(
                entryValidated: false,
                testedByte: 2,
                zeroPathPoppedEbp: 0xDEADBEEF));
        check("An unvalidated entry reaches no instructions and marks both later captures unreached",
            invalidEntry.Outcome == CapacityFinalCalleeEntryOutcome.EntryNotValidated
            && invalidEntry.StackStores.IsEmpty
            && !invalidEntry.PushEbpReached
            && !invalidEntry.MovEbpEspReached
            && !invalidEntry.SubEspReached
            && !invalidEntry.TestEcxReached
            && !invalidEntry.BranchReached
            && !invalidEntry.TestByteReadReached
            && !invalidEntry.ZeroPathPopEbpReached
            && !invalidEntry.ReturnReached
            && invalidEntry.UnreachedTestedByteCaptureProvided
            && invalidEntry.UnreachedZeroPathPoppedEbpCaptureProvided
            && invalidEntry.Capture.EntryEsi == 0x22222222
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C8D60);

        CapacityFinalCalleeEntryPlan zeroPath =
            CapacityFinalCalleeEntryProjection.Project(Capture(
                entryEsp: 0,
                entryEbp: 0x12345678,
                entryEcx: 0,
                testedByte: 0x02,
                zeroPathPoppedEbp: 0xDEADC0DE));
        check("The zero branch follows the existing EBP epilogue without reading the byte",
            zeroPath.Outcome == CapacityFinalCalleeEntryOutcome.ZeroPathReturned
            && zeroPath.StackStores.SequenceEqual(new[]
            {
                new CapacityFinalCalleeEntryStackStore(
                    0x012C8D60,
                    CapacityFinalCalleeEntryStackStoreKind.SavedEbp,
                    0xFFFFFFFC,
                    0x12345678)
            })
            && zeroPath.PushEbpAddress == 0xFFFFFFFC
            && zeroPath.FrameEbp == 0xFFFFFFFC
            && zeroPath.FrameEsp == 0xFFFFFFCC
            && zeroPath.TestEcxReached
            && zeroPath.EntryEcxIsZero == true
            && zeroPath.BranchReached
            && zeroPath.ZeroBranchTaken == true
            && zeroPath.SelectedDestinationRawFileOffset == 0x012C9458
            && !zeroPath.TestByteReadReached
            && !zeroPath.TestedByteCaptured
            && zeroPath.TestedByte is null
            && zeroPath.UnreachedTestedByteCaptureProvided
            && !zeroPath.PushEbxReached
            && !zeroPath.PushEsiReached
            && !zeroPath.PushEdiReached
            && zeroPath.MovEspFromEbpReached
            && zeroPath.EpilogueEsp == 0xFFFFFFFC
            && zeroPath.ZeroPathPopEbpReached
            && zeroPath.ZeroPathPoppedEbp == 0xDEADC0DE
            && zeroPath.ZeroPathPoppedEbp != zeroPath.Capture.EntryEbp
            && zeroPath.ReturnAddressStackAddress == 0
            && zeroPath.EspAfterReturn == 4
            && zeroPath.ReturnReached
            && zeroPath.StopAtRawFileOffsetExclusive == 0x012C945C);

        CapacityFinalCalleeEntryPlan missingZeroPathPop =
            CapacityFinalCalleeEntryProjection.Project(Capture(
                entryEsp: 0x1000,
                entryEbp: 0x2000,
                entryEcx: 0,
                testedByte: null,
                zeroPathPoppedEbp: null));
        check("A missing zero-path POP capture stops after MOV ESP,EBP without inferring the saved value",
            missingZeroPathPop.Outcome
                == CapacityFinalCalleeEntryOutcome.MissingZeroPathPoppedEbpValue
            && missingZeroPathPop.StackStores.Length == 1
            && missingZeroPathPop.StackStores[0].Address == 0x0FFC
            && missingZeroPathPop.StackStores[0].Value == 0x2000
            && missingZeroPathPop.MovEspFromEbpReached
            && missingZeroPathPop.EpilogueEsp == 0x0FFC
            && missingZeroPathPop.ZeroPathPopEbpReached
            && missingZeroPathPop.ZeroPathPoppedEbp is null
            && !missingZeroPathPop.ReturnReached
            && missingZeroPathPop.StopAtRawFileOffsetExclusive == 0x012C945A);

        CapacityFinalCalleeEntryPlan missingByte =
            CapacityFinalCalleeEntryProjection.Project(Capture(
                entryEsp: 0x20,
                entryEbp: 0xAABBCCDD,
                entryEcx: 1,
                testedByte: null,
                zeroPathPoppedEbp: 0x55667788));
        check("A missing nonzero-path byte preserves the allocated frame and stops before saving EBX",
            missingByte.Outcome == CapacityFinalCalleeEntryOutcome.MissingTestedByte
            && missingByte.StackStores.SequenceEqual(new[]
            {
                new CapacityFinalCalleeEntryStackStore(
                    0x012C8D60,
                    CapacityFinalCalleeEntryStackStoreKind.SavedEbp,
                    0x1C,
                    0xAABBCCDD)
            })
            && missingByte.FrameEbp == 0x1C
            && missingByte.FrameEsp == 0xFFFFFFEC
            && missingByte.TestEcxReached
            && missingByte.EntryEcxIsZero == false
            && missingByte.BranchReached
            && missingByte.ZeroBranchTaken == false
            && missingByte.TestByteReadReached
            && !missingByte.TestedByteCaptured
            && missingByte.TestedBitIsSet is null
            && !missingByte.PushEbxReached
            && !missingByte.PushEsiReached
            && !missingByte.PushEdiReached
            && missingByte.UnreachedZeroPathPoppedEbpCaptureProvided
            && missingByte.StopAtRawFileOffsetExclusive == 0x012C8D6E);

        CapacityFinalCalleeEntryPlan nonzeroBitClear =
            CapacityFinalCalleeEntryProjection.Project(Capture(
                entryEsp: 0x20,
                entryEbp: 0xAABBCCDD,
                entryEcx: 0x1234,
                entryEbx: 0x11111111,
                entryEsi: 0x22222222,
                entryEdi: 0x33333333,
                testedByte: 0xFD,
                zeroPathPoppedEbp: 0x55667788));
        CapacityFinalCalleeEntryPlan nonzeroBitSet =
            CapacityFinalCalleeEntryProjection.Project(Capture(
                entryEsp: 0x20,
                entryEcx: 0x1234,
                testedByte: 0x02));
        check("A captured TEST byte produces flags before ordered EBX, ESI, and EDI stack stores",
            nonzeroBitClear.Outcome
                == CapacityFinalCalleeEntryOutcome.NonzeroPrefixCompleted
            && nonzeroBitClear.TestByteReadReached
            && nonzeroBitClear.TestedByteCaptured
            && nonzeroBitClear.TestedByte == 0xFD
            && nonzeroBitClear.TestedBitIsSet == false
            && nonzeroBitClear.StackStores.SequenceEqual(new[]
            {
                new CapacityFinalCalleeEntryStackStore(
                    0x012C8D60,
                    CapacityFinalCalleeEntryStackStoreKind.SavedEbp,
                    0x1C,
                    0xAABBCCDD),
                new CapacityFinalCalleeEntryStackStore(
                    0x012C8D75,
                    CapacityFinalCalleeEntryStackStoreKind.SavedEbx,
                    0xFFFFFFE8,
                    0x11111111),
                new CapacityFinalCalleeEntryStackStore(
                    0x012C8D76,
                    CapacityFinalCalleeEntryStackStoreKind.SavedEsi,
                    0xFFFFFFE4,
                    0x22222222),
                new CapacityFinalCalleeEntryStackStore(
                    0x012C8D77,
                    CapacityFinalCalleeEntryStackStoreKind.SavedEdi,
                    0xFFFFFFE0,
                    0x33333333)
            })
            && nonzeroBitClear.PushEbxReached
            && nonzeroBitClear.PushEsiReached
            && nonzeroBitClear.PushEdiReached
            && nonzeroBitClear.StackPointerAfterNonzeroSaves == 0xFFFFFFE0
            && nonzeroBitClear.UnreachedZeroPathPoppedEbpCaptureProvided
            && nonzeroBitClear.StopAtRawFileOffsetExclusive == 0x012C8D78
            && nonzeroBitSet.Outcome
                == CapacityFinalCalleeEntryOutcome.NonzeroPrefixCompleted
            && nonzeroBitSet.TestedBitIsSet == true
            && nonzeroBitSet.StackStores.Length == 4
            && !nonzeroBitSet.UnreachedZeroPathPoppedEbpCaptureProvided
            && nonzeroBitSet.StopAtRawFileOffsetExclusive == 0x012C8D78);

        CapacityFinalCalleeEntryPlan wrappedAddresses =
            CapacityFinalCalleeEntryProjection.Project(Capture(
                entryEsp: 0,
                entryEbp: uint.MaxValue,
                entryEcx: uint.MaxValue,
                entryEbx: 1,
                entryEsi: 2,
                entryEdi: 3,
                testedByte: 0));
        check("Entry-derived stack addresses wrap modulo 32 bits and preserve native push order",
            CapacityFinalCalleeEntryProjection.PushAddress(0) == 0xFFFFFFFC
            && CapacityFinalCalleeEntryProjection.LocalFrameEsp(0) == 0xFFFFFFCC
            && wrappedAddresses.StackStores.Select(store => store.Address).SequenceEqual(new[]
            {
                0xFFFFFFFCu,
                0xFFFFFFC8u,
                0xFFFFFFC4u,
                0xFFFFFFC0u
            })
            && wrappedAddresses.StackStores.Select(store => store.Value).SequenceEqual(new[]
            {
                uint.MaxValue,
                1u,
                2u,
                3u
            })
            && wrappedAddresses.StackPointerAfterNonzeroSaves == 0xFFFFFFC0
            && wrappedAddresses.TestedBitIsSet == false
            && wrappedAddresses.Outcome
                == CapacityFinalCalleeEntryOutcome.NonzeroPrefixCompleted);
    }

    private static CapacityFinalCalleeEntryCapture Capture(
        bool entryValidated = true,
        uint entryEsp = 0x1000,
        uint entryEbp = 0x2000,
        uint entryEcx = 1,
        uint entryEbx = 0x11111111,
        uint entryEsi = 0x22222222,
        uint entryEdi = 0x33333333,
        byte? testedByte = 0,
        uint? zeroPathPoppedEbp = null) =>
        new(
            entryValidated,
            entryEsp,
            entryEbp,
            entryEcx,
            entryEbx,
            entryEsi,
            entryEdi,
            testedByte,
            zeroPathPoppedEbp);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? PeVirtualAddressOperand,
            byte? ImmediateValue,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual)[]
        {
            (0x012C8D60, "55", null, null, null, null),
            (0x012C8D61, "8B EC", null, null, null, null),
            (0x012C8D63, "83 EC 30", null, 0x30, null, null),
            (0x012C8D66, "85 C9", null, null, null, null),
            (0x012C8D68, "0F 84 EA 06 00 00", null, null, 0x012C9458, 0x016CA058),
            (0x012C8D6E, "F6 05 84 AF ED 01 02", 0x01EDAF84, 0x02, null, null),
            (0x012C8D75, "53", null, null, null, null),
            (0x012C8D76, "56", null, null, null, null),
            (0x012C8D77, "57", null, null, null, null)
        };
        uint nextRawOffset = CapacityFinalCalleeEntryProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeEntryInstructionEvidence instruction
            in CapacityFinalCalleeEntryProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeEntryProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeEntryProjection.RawFileOffsetToVirtualAddress(
                            targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        var expectedEpilogue = new (
            uint Raw,
            string Bytes)[]
        {
            (0x012C9458, "8B E5"),
            (0x012C945A, "5D"),
            (0x012C945B, "C3")
        };
        uint nextEpilogueOffset =
            CapacityFinalCalleeEntryProjection.ZeroBranchDestinationRawFileOffset;
        string epilogueBytes = string.Empty;
        foreach (CapacityFinalCalleeEntryInstructionEvidence instruction
            in CapacityFinalCalleeEntryProjection.ZeroEpilogueInstructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextEpilogueOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeEntryProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextEpilogueOffset += (uint)(normalized.Length / 2);
            epilogueBytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeEntryProjection.NonzeroPrefixBoundaryRawFileOffsetExclusive
            && bytes == Normalize(CapacityFinalCalleeEntryProjection.CapturedWindowBytes)
            && CapacityFinalCalleeEntryProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.PeVirtualAddressOperand,
                        instruction.ImmediateValue,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected)
            && nextEpilogueOffset
                == CapacityFinalCalleeEntryProjection.ZeroReturnBoundaryRawFileOffsetExclusive
            && epilogueBytes == Normalize(
                CapacityFinalCalleeEntryProjection.ZeroEpilogueBytes)
            && CapacityFinalCalleeEntryProjection.ZeroEpilogueInstructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expectedEpilogue);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
