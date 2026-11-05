using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeAlternateFrameLoadsChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The raw/VA window, branch target, and instruction bytes match",
            CapacityFinalCalleeAlternateFrameLoadsProjection.EntryRawFileOffset
                == 0x012C910C
            && CapacityFinalCalleeAlternateFrameLoadsProjection.EntryVirtualAddress
                == 0x016C9D0C
            && CapacityFinalCalleeAlternateFrameLoadsProjection.OpaqueCallRawFileOffset
                == 0x012C911C
            && CapacityFinalCalleeAlternateFrameLoadsProjection.UnequalDestinationRawFileOffset
                == 0x012C9122
            && CapacityFinalCalleeAlternateFrameLoadsProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeAlternateFrameLoadsProjection.EntryRawFileOffset)
                == CapacityFinalCalleeAlternateFrameLoadsProjection.EntryVirtualAddress
            && Normalize(
                CapacityFinalCalleeAlternateFrameLoadsProjection.CapturedWindowBytes)
                == "8B42088945F08B420C8945EC3BD07506"
            && InstructionEvidenceMatches());

        CapacityFinalCalleeAlternateFrameLoadsPlan missingEdx =
            CapacityFinalCalleeAlternateFrameLoadsProjection.Project(Capture(
                entryEdx: null,
                entryEbp: 0x100,
                freshEdxPlus8: 0,
                freshEdxPlusC: 0));
        check("Missing EDX gates the first read and marks later captures unreached",
            missingEdx.Outcome
                == CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingEntryEDX
            && missingEdx.FirstReadReached
            && !missingEdx.EntryEDXCaptured
            && missingEdx.Reads.Length == 1
            && missingEdx.Reads[0] is
            {
                RawFileOffset: 0x012C910C,
                Address: null,
                ValueCaptured: false,
                Value: null
            }
            && !missingEdx.FirstWriteReached
            && !missingEdx.SecondReadReached
            && !missingEdx.CompareReached
            && missingEdx.UnreachedFreshEdxPlus8CaptureProvided
            && missingEdx.UnreachedEntryEBPCaptureProvided
            && missingEdx.UnreachedFreshEdxPlusCCaptureProvided
            && missingEdx.StopAtRawFileOffsetExclusive == 0x012C910C
            && missingEdx.Writes.IsEmpty);

        CapacityFinalCalleeAlternateFrameLoadsPlan missingFirst =
            CapacityFinalCalleeAlternateFrameLoadsProjection.Project(Capture(
                entryEdx: 0x20,
                entryEbp: 0x100,
                freshEdxPlus8: null,
                freshEdxPlusC: 0));
        check("Missing fresh EDX+8 is distinct from a captured zero and stops at its load",
            missingFirst.Outcome
                == CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingFreshEdxPlus8
            && missingFirst.FirstReadReached
            && missingFirst.EntryEDXCaptured
            && missingFirst.FreshEdxPlus8Address == 0x28
            && !missingFirst.FreshEdxPlus8Captured
            && missingFirst.FreshEdxPlus8 is null
            && !missingFirst.FirstWriteReached
            && !missingFirst.SecondReadReached
            && !missingFirst.CompareReached
            && missingFirst.UnreachedEntryEBPCaptureProvided
            && missingFirst.UnreachedFreshEdxPlusCCaptureProvided
            && missingFirst.StopAtRawFileOffsetExclusive == 0x012C910C);

        CapacityFinalCalleeAlternateFrameLoadsPlan missingEbp =
            CapacityFinalCalleeAlternateFrameLoadsProjection.Project(Capture(
                entryEdx: 0x20,
                entryEbp: null,
                freshEdxPlus8: 0,
                freshEdxPlusC: 0x30));
        check("Missing EBP retains the first fresh read but blocks its own store",
            missingEbp.Outcome
                == CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingEntryEBPForFirstWrite
            && missingEbp.FreshEdxPlus8Captured
            && missingEbp.FreshEdxPlus8 == 0
            && !missingEbp.EntryEBPCaptured
            && !missingEbp.FirstWriteReached
            && missingEbp.FirstWriteAddress is null
            && !missingEbp.SecondReadReached
            && !missingEbp.CompareReached
            && missingEbp.UnreachedFreshEdxPlusCCaptureProvided
            && missingEbp.StopAtRawFileOffsetExclusive == 0x012C910F);

        CapacityFinalCalleeAlternateFrameLoadsPlan missingSecond =
            CapacityFinalCalleeAlternateFrameLoadsProjection.Project(Capture(
                entryEdx: 0x100,
                entryEbp: 0x200,
                freshEdxPlus8: 0xAABBCCDD,
                freshEdxPlusC: null));
        check("Missing fresh EDX+C preserves the first frame write and stops before the second",
            missingSecond.Outcome
                == CapacityFinalCalleeAlternateFrameLoadsOutcome.MissingFreshEdxPlusC
            && missingSecond.FirstWriteReached
            && missingSecond.FirstWriteAddress == 0x1F0
            && missingSecond.FirstWriteValue == 0xAABBCCDD
            && missingSecond.SecondReadReached
            && missingSecond.FreshEdxPlusCAddress == 0x10C
            && !missingSecond.FreshEdxPlusCCaptured
            && !missingSecond.SecondWriteReached
            && !missingSecond.CompareReached
            && missingSecond.Writes.Length == 1
            && missingSecond.Writes[0]
                == new CapacityFinalCalleeAlternateFrameLoadsWrite(
                    0x012C910F, 0x1F0, 0xAABBCCDD)
            && missingSecond.StopAtRawFileOffsetExclusive == 0x012C9112);

        CapacityFinalCalleeAlternateFrameLoadsPlan aliasedReload =
            CapacityFinalCalleeAlternateFrameLoadsProjection.Project(Capture(
                entryEdx: 0x100,
                entryEbp: 0x11C,
                freshEdxPlus8: 0xDEADBEEF,
                freshEdxPlusC: 0x12345678));
        check("Aliased first store and second read remain an independent fresh capture",
            aliasedReload.FirstWriteAddress == 0x10C
            && aliasedReload.FreshEdxPlusCAddress == 0x10C
            && aliasedReload.FirstWriteValue == 0xDEADBEEF
            && aliasedReload.FreshEdxPlusCCaptured
            && aliasedReload.FreshEdxPlusC == 0x12345678
            && aliasedReload.SecondWriteAddress == 0x108
            && aliasedReload.SecondWriteValue == 0x12345678
            && aliasedReload.CompareRight == 0x12345678
            && aliasedReload.CompareResult == unchecked(0x100u - 0x12345678u)
            && aliasedReload.Reads.Select(read => read.Address)
                .SequenceEqual(new uint?[] { 0x108, 0x10C })
            && aliasedReload.Writes.Select(write => write.RawFileOffset)
                .SequenceEqual(new uint[] { 0x012C910F, 0x012C9115 })
            && aliasedReload.Writes.Select(write => write.Address)
                .SequenceEqual(new uint[] { 0x10C, 0x108 })
            && aliasedReload.Writes.Select(write => write.Value)
                .SequenceEqual(new uint[] { 0xDEADBEEF, 0x12345678 }));

        CapacityFinalCalleeAlternateFrameLoadsPlan wrapped =
            CapacityFinalCalleeAlternateFrameLoadsProjection.Project(Capture(
                entryEdx: 0xFFFFFFF8,
                entryEbp: 0x10,
                freshEdxPlus8: 0,
                freshEdxPlusC: 0));
        check("DWORD read and frame destinations wrap without treating zero as missing",
            wrapped.FreshEdxPlus8Address == 0
            && wrapped.FreshEdxPlusCAddress == 4
            && wrapped.FreshEdxPlus8Captured
            && wrapped.FreshEdxPlus8 == 0
            && wrapped.FreshEdxPlusCCaptured
            && wrapped.FreshEdxPlusC == 0
            && wrapped.FirstWriteAddress == 0
            && wrapped.SecondWriteAddress == 0xFFFFFFFC
            && CapacityFinalCalleeAlternateFrameLoadsProjection.DeriveEdxPlus8Address(
                0xFFFFFFF8) == 0
            && CapacityFinalCalleeAlternateFrameLoadsProjection.DeriveEdxPlusCAddress(
                0xFFFFFFF8) == 4
            && CapacityFinalCalleeAlternateFrameLoadsProjection.DeriveEbpMinus10Address(
                0x10) == 0
            && CapacityFinalCalleeAlternateFrameLoadsProjection.DeriveEbpMinus14Address(
                0x10) == 0xFFFFFFFC);

        CapacityFinalCalleeAlternateFrameLoadsPlan equal =
            CapacityFinalCalleeAlternateFrameLoadsProjection.Project(Capture(
                entryEdx: 0,
                entryEbp: 0,
                freshEdxPlus8: 0,
                freshEdxPlusC: 0));
        check("Equal zero compare reaches both writes and stops before opaque CALL 911C",
            equal.Outcome
                == CapacityFinalCalleeAlternateFrameLoadsOutcome.CompareEqualToOpaqueCallBoundary
            && equal.Writes.Length == 2
            && equal.FirstWriteAddress == 0xFFFFFFF0
            && equal.FirstWriteValue == 0
            && equal.SecondWriteAddress == 0xFFFFFFEC
            && equal.SecondWriteValue == 0
            && equal.CompareReached
            && equal.CompareLeft == 0
            && equal.CompareRight == 0
            && equal.CompareResult == 0
            && equal.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equal.BranchReached
            && equal.BranchTaken == false
            && equal.BranchSelectedDestinationRawFileOffset == 0x012C911C
            && !equal.OpaqueCallReached
            && equal.ExactCleanExitAdmitted
            && equal.StopAtRawFileOffsetExclusive == 0x012C911C);

        CapacityFinalCalleeAlternateFrameLoadsPlan unsignedBorrow =
            CapacityFinalCalleeAlternateFrameLoadsProjection.Project(Capture(
                entryEdx: 0,
                entryEbp: 0x40,
                freshEdxPlus8: 0x55,
                freshEdxPlusC: 1));
        check("CMP models wrapped subtraction and unsigned borrow flags",
            unsignedBorrow.CompareResult == 0xFFFFFFFF
            && unsignedBorrow.CompareFlags is
            {
                Carry: true,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && unsignedBorrow.BranchTaken == true
            && unsignedBorrow.BranchSelectedDestinationRawFileOffset == 0x012C9122
            && unsignedBorrow.StopAtRawFileOffsetExclusive == 0x012C9122
            && !unsignedBorrow.OpaqueCallReached);

        CapacityFinalCalleeAlternateFrameLoadsPlan signedOverflow =
            CapacityFinalCalleeAlternateFrameLoadsProjection.Project(Capture(
                entryEdx: 0x80000000,
                entryEbp: 0x40,
                freshEdxPlus8: 0x100,
                freshEdxPlusC: 1));
        check("CMP distinguishes signed overflow, parity, and a clear sign result",
            signedOverflow.CompareResult == 0x7FFFFFFF
            && signedOverflow.CompareFlags is
            {
                Carry: false,
                Parity: true,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && signedOverflow.BranchTaken == true);

        CapacityFinalCalleeAlternateFrameLoadsPlan oddParity =
            CapacityFinalCalleeAlternateFrameLoadsProjection.Project(Capture(
                entryEdx: 0x80000000,
                entryEbp: 0x40,
                freshEdxPlus8: 0x100,
                freshEdxPlusC: 0x7FFFFFFF));
        check("CMP parity is computed from the wrapped result low byte",
            oddParity.CompareResult == 1
            && oddParity.CompareFlags is
            {
                Carry: false,
                Parity: false,
                Auxiliary: true,
                Zero: false,
                Sign: false,
                Overflow: true
            });
    }

    private static CapacityFinalCalleeAlternateFrameLoadsCapture Capture(
        uint? entryEdx = 0x1000,
        uint? entryEbp = 0x2000,
        uint? freshEdxPlus8 = 0x3000,
        uint? freshEdxPlusC = 0x4000) =>
        new(entryEdx, entryEbp, freshEdxPlus8, freshEdxPlusC);

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw)[] {
            (0x012C910C, "8B 42 08", null),
            (0x012C910F, "89 45 F0", null),
            (0x012C9112, "8B 42 0C", null),
            (0x012C9115, "89 45 EC", null),
            (0x012C9118, "3B D0", null),
            (0x012C911A, "75 06", 0x012C9122)
        };
        uint nextRawOffset =
            CapacityFinalCalleeAlternateFrameLoadsProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (CapacityFinalCalleeAlternateFrameLoadsInstructionEvidence instruction
            in CapacityFinalCalleeAlternateFrameLoadsProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != CapacityFinalCalleeAlternateFrameLoadsProjection
                        .RawFileOffsetToVirtualAddress(instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != CapacityFinalCalleeAlternateFrameLoadsProjection
                            .RawFileOffsetToVirtualAddress(targetRaw)))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == CapacityFinalCalleeAlternateFrameLoadsProjection
                    .BoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                CapacityFinalCalleeAlternateFrameLoadsProjection.CapturedWindowBytes)
            && CapacityFinalCalleeAlternateFrameLoadsProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
