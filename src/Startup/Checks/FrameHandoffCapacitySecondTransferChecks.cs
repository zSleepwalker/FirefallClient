using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacitySecondTransferChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE identity and raw IDA bytes cover exactly BE6C10 through BE6C33",
            FrameHandoffCapacitySecondTransferProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacitySecondTransferProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacitySecondTransferProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacitySecondTransferProjection.EntryRawFileOffset == 0x00BE6C10
            && FrameHandoffCapacitySecondTransferProjection.EntryVirtualAddress == 0x00FE7810
            && FrameHandoffCapacitySecondTransferProjection.NormalBoundaryRawFileOffsetExclusive
                == 0x00BE6C34
            && FrameHandoffCapacitySecondTransferProjection.NormalBoundaryVirtualAddressExclusive
                == 0x00FE7834
            && FrameHandoffCapacitySecondTransferProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacitySecondTransferProjection.EntryRawFileOffset)
                == FrameHandoffCapacitySecondTransferProjection.EntryVirtualAddress
            && FrameHandoffCapacitySecondTransferProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacitySecondTransferProjection.NormalBoundaryRawFileOffsetExclusive)
                == FrameHandoffCapacitySecondTransferProjection.NormalBoundaryVirtualAddressExclusive
            && Normalize(FrameHandoffCapacitySecondTransferProjection.CapturedWindowBytes)
                == "8D04BD000000008BD6C1F8022BD78955F48D1C8500000000A19C48AC015351FF75F0FFD0"
            && FrameHandoffCapacitySecondTransferProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 36
            && InstructionEvidenceMatches());

        FrameHandoffCapacitySecondTransferPlan normal =
            FrameHandoffCapacitySecondTransferProjection.Project(new(
                EntryEdi: 0x20000000,
                EntryEsi: 1,
                EntryEcx: 0x99,
                EntryEbp: 0x10,
                CallCellDword: 0,
                FrameMinus10Dword: 0xA,
                CallReturnedNormally: true));
        check("Signed SAR, DWORD wrap, ordered accesses, zero target, and normal return are exact",
            normal.Outcome == FrameHandoffCapacitySecondTransferOutcome.ReturnedNormally
            && normal.EaxAfterLea == 0x80000000
            && normal.EdxAfterMove == 1
            && normal.EaxAfterArithmeticShiftRight == 0xE0000000
            && normal.EdxAfterSubtract == 0xE0000001
            && normal.FrameMinusCWriteReached
            && normal.FrameMinusCWriteAddress == 4
            && normal.FrameMinusCWriteValue == 0xE0000001
            && normal.EbxAfterLea == 0x80000000
            && normal.CallCellAddress == 0x01AC489C
            && normal.CallCellCaptured
            && normal.CallTargetAddress == 0
            && normal.EbxPushReached
            && normal.EcxPushReached
            && normal.FrameMinus10ReadReached
            && normal.FrameMinus10Captured
            && normal.FrameMinus10PushReached
            && normal.CallReached
            && normal.CallReturnedNormally == true
            && normal.NormalBoundaryReached
            && !normal.UnreachedFrameMinus10CaptureProvided
            && !normal.UnreachedCallReturnCaptureProvided
            && normal.StopAtRawFileOffsetExclusive == 0x00BE6C34
            && normal.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacitySecondTransferRead(0x00BE6C28, 0x01AC489C, 0),
                new FrameHandoffCapacitySecondTransferRead(0x00BE6C2F, 0, 0xA)
            })
            && normal.MemoryAccesses.SequenceEqual(new[]
            {
                new FrameHandoffCapacitySecondTransferMemoryAccess(
                    FrameHandoffCapacitySecondTransferMemoryAccessKind.Write,
                    0x00BE6C1E, 4, 0xE0000001),
                new FrameHandoffCapacitySecondTransferMemoryAccess(
                    FrameHandoffCapacitySecondTransferMemoryAccessKind.Read,
                    0x00BE6C28, 0x01AC489C, 0),
                new FrameHandoffCapacitySecondTransferMemoryAccess(
                    FrameHandoffCapacitySecondTransferMemoryAccessKind.Read,
                    0x00BE6C2F, 0, 0xA)
            })
            && normal.FrameWrites.SequenceEqual(new[]
            {
                new FrameHandoffCapacitySecondTransferFrameWrite(
                    0x00BE6C1E, 4, 0xE0000001)
            })
            && normal.MemoryAccesses.Select(access => access.RawFileOffset)
                .SequenceEqual(new uint[] { 0x00BE6C1E, 0x00BE6C28, 0x00BE6C2F }));

        FrameHandoffCapacitySecondTransferPlan missingCallCell =
            FrameHandoffCapacitySecondTransferProjection.Project(new(
                EntryEdi: 1,
                EntryEsi: 2,
                EntryEcx: 3,
                EntryEbp: 4,
                CallCellDword: null,
                FrameMinus10Dword: 5,
                CallReturnedNormally: true));
        check("Missing call-cell capture stops before any push and marks later captures unreached",
            missingCallCell.Outcome
                == FrameHandoffCapacitySecondTransferOutcome.MissingCallCellCapture
            && missingCallCell.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacitySecondTransferRead(0x00BE6C28, 0x01AC489C, null)
            })
            && missingCallCell.FrameWrites.Single().RawFileOffset == 0x00BE6C1E
            && missingCallCell.CallCellReadReached
            && !missingCallCell.CallCellCaptured
            && missingCallCell.CallTargetAddress is null
            && !missingCallCell.EbxPushReached
            && !missingCallCell.EcxPushReached
            && !missingCallCell.FrameMinus10ReadReached
            && !missingCallCell.CallReached
            && missingCallCell.UnreachedFrameMinus10CaptureProvided
            && missingCallCell.UnreachedCallReturnCaptureProvided
            && missingCallCell.StopAtRawFileOffsetExclusive == 0x00BE6C28);

        FrameHandoffCapacitySecondTransferPlan missingFrameMinus10 =
            FrameHandoffCapacitySecondTransferProjection.Project(new(
                EntryEdi: 0xFFFFFFFF,
                EntryEsi: 0,
                EntryEcx: 0x1234,
                EntryEbp: 0xC,
                CallCellDword: 0,
                FrameMinus10Dword: null,
                CallReturnedNormally: false));
        check("Missing frame-minus-ten preserves the first two pushes and stops at its read",
            missingFrameMinus10.Outcome
                == FrameHandoffCapacitySecondTransferOutcome.MissingFrameMinus10Capture
            && missingFrameMinus10.EaxAfterLea == 0xFFFFFFFC
            && missingFrameMinus10.EaxAfterArithmeticShiftRight == 0xFFFFFFFF
            && missingFrameMinus10.EdxAfterSubtract == 1
            && missingFrameMinus10.FrameMinusCWriteAddress == 0
            && missingFrameMinus10.EbxAfterLea == 0xFFFFFFFC
            && missingFrameMinus10.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacitySecondTransferRead(0x00BE6C28, 0x01AC489C, 0),
                new FrameHandoffCapacitySecondTransferRead(0x00BE6C2F, 0xFFFFFFFC, null)
            })
            && missingFrameMinus10.EbxPushReached
            && missingFrameMinus10.EcxPushReached
            && missingFrameMinus10.Pushes.SequenceEqual(new[]
            {
                new FrameHandoffCapacitySecondTransferPush(
                    0x00BE6C2D, "EBX", 0xFFFFFFFC),
                new FrameHandoffCapacitySecondTransferPush(
                    0x00BE6C2E, "ECX", 0x1234)
            })
            && missingFrameMinus10.FrameMinus10ReadReached
            && !missingFrameMinus10.FrameMinus10Captured
            && !missingFrameMinus10.FrameMinus10PushReached
            && !missingFrameMinus10.CallReached
            && !missingFrameMinus10.UnreachedFrameMinus10CaptureProvided
            && missingFrameMinus10.UnreachedCallReturnCaptureProvided
            && missingFrameMinus10.StopAtRawFileOffsetExclusive == 0x00BE6C2F
            && missingFrameMinus10.MemoryAccesses[^1].Kind
                == FrameHandoffCapacitySecondTransferMemoryAccessKind.Read
            && missingFrameMinus10.MemoryAccesses[^1].Value is null);

        FrameHandoffCapacitySecondTransferPlan missingReturnStatus =
            Project(callReturnedNormally: null);
        FrameHandoffCapacitySecondTransferPlan nonreturning =
            Project(callReturnedNormally: false);
        check("Missing and nonreturning calls stop at the call with all three pushes preserved",
            missingReturnStatus.Outcome
                == FrameHandoffCapacitySecondTransferOutcome.MissingCallReturnCapture
            && missingReturnStatus.CallReached
            && missingReturnStatus.CallReturnedNormally is null
            && missingReturnStatus.FrameMinus10PushReached
            && missingReturnStatus.Pushes.Length == 3
            && missingReturnStatus.Pushes[0].Source == "EBX"
            && missingReturnStatus.Pushes[1].Source == "ECX"
            && missingReturnStatus.Pushes[2].Source == "[EBP-0x10]"
            && missingReturnStatus.StopAtRawFileOffsetExclusive == 0x00BE6C32
            && nonreturning.Outcome
                == FrameHandoffCapacitySecondTransferOutcome.CallDidNotReturnNormally
            && nonreturning.CallReached
            && nonreturning.CallReturnedNormally == false
            && nonreturning.FrameMinus10PushReached
            && nonreturning.Pushes.Length == 3
            && nonreturning.Pushes[0].Value == 0xFFFFFFFC
            && nonreturning.Pushes[1].Value == 0x1234
            && nonreturning.Pushes[2].Value == 0x5678
            && !nonreturning.NormalBoundaryReached
            && nonreturning.StopAtRawFileOffsetExclusive == 0x00BE6C32);
    }

    private static FrameHandoffCapacitySecondTransferPlan Project(bool? callReturnedNormally) =>
        FrameHandoffCapacitySecondTransferProjection.Project(new(
            EntryEdi: 0xFFFFFFFF,
            EntryEsi: 0,
            EntryEcx: 0x1234,
            EntryEbp: 0xC,
            CallCellDword: 0,
            FrameMinus10Dword: 0x5678,
            CallReturnedNormally: callReturnedNormally));

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x00BE6C10, "8D 04 BD 00 00 00 00"),
            (0x00BE6C17, "8B D6"),
            (0x00BE6C19, "C1 F8 02"),
            (0x00BE6C1C, "2B D7"),
            (0x00BE6C1E, "89 55 F4"),
            (0x00BE6C21, "8D 1C 85 00 00 00 00"),
            (0x00BE6C28, "A1 9C 48 AC 01"),
            (0x00BE6C2D, "53"),
            (0x00BE6C2E, "51"),
            (0x00BE6C2F, "FF 75 F0"),
            (0x00BE6C32, "FF D0")
        };
        uint nextRawOffset = FrameHandoffCapacitySecondTransferProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (FrameHandoffCapacitySecondTransferInstructionEvidence instruction
            in FrameHandoffCapacitySecondTransferProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacitySecondTransferProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == FrameHandoffCapacitySecondTransferProjection.NormalBoundaryRawFileOffsetExclusive
            && bytes == Normalize(FrameHandoffCapacitySecondTransferProjection.CapturedWindowBytes)
            && FrameHandoffCapacitySecondTransferProjection.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
