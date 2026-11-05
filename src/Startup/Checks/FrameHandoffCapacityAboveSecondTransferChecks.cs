using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityAboveSecondTransferChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE identity and raw IDA bytes cover exactly BE6C6D through BE6C8E",
            FrameHandoffCapacityAboveSecondTransferProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityAboveSecondTransferProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityAboveSecondTransferProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityAboveSecondTransferProjection.EntryRawFileOffset == 0x00BE6C6D
            && FrameHandoffCapacityAboveSecondTransferProjection.EntryVirtualAddress == 0x00FE786D
            && FrameHandoffCapacityAboveSecondTransferProjection.NormalBoundaryRawFileOffsetExclusive
                == 0x00BE6C8F
            && FrameHandoffCapacityAboveSecondTransferProjection.NormalBoundaryVirtualAddressExclusive
                == 0x00FE788F
            && FrameHandoffCapacityAboveSecondTransferProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityAboveSecondTransferProjection.EntryRawFileOffset)
                == FrameHandoffCapacityAboveSecondTransferProjection.EntryVirtualAddress
            && FrameHandoffCapacityAboveSecondTransferProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityAboveSecondTransferProjection.NormalBoundaryRawFileOffsetExclusive)
                == FrameHandoffCapacityAboveSecondTransferProjection.NormalBoundaryVirtualAddressExclusive
            && Normalize(FrameHandoffCapacityAboveSecondTransferProjection.CapturedWindowBytes)
                == "8B43048B55F48D0C072BD103D0C1FA028D1C95000000005351FF75FCFF159C48AC01"
            && FrameHandoffCapacityAboveSecondTransferProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 34
            && InstructionEvidenceMatches());

        FrameHandoffCapacityAboveSecondTransferPlan invalidEntry =
            FrameHandoffCapacityAboveSecondTransferProjection.Project(new(
                ValidatedNormalEntry: false,
                EntryEbx: 0x1000,
                EntryEdi: 0x2000,
                EntryEbp: 0x3000,
                Receiver4Dword: 0,
                FrameMinusCDword: 0,
                FrameMinus4Dword: 0,
                CallReturnedNormally: true));
        check("An unvalidated normal entry leaves its reads, scalars, pushes, and call unreached",
            invalidEntry.Outcome
                == FrameHandoffCapacityAboveSecondTransferOutcome.EntryNotValidated
            && invalidEntry.Reads.IsEmpty
            && invalidEntry.Scalars.IsEmpty
            && invalidEntry.Pushes.IsEmpty
            && !invalidEntry.Receiver4ReadReached
            && !invalidEntry.FrameMinusCReadReached
            && !invalidEntry.CallReached
            && invalidEntry.UnreachedReceiver4CaptureProvided
            && invalidEntry.UnreachedFrameMinusCCaptureProvided
            && invalidEntry.UnreachedFrameMinus4CaptureProvided
            && invalidEntry.UnreachedCallReturnCaptureProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x00BE6C6D);

        FrameHandoffCapacityAboveSecondTransferPlan normal =
            FrameHandoffCapacityAboveSecondTransferProjection.Project(new(
                ValidatedNormalEntry: true,
                EntryEbx: 0xFFFFFFFC,
                EntryEdi: 0xFFFFFFFF,
                EntryEbp: 0x0C,
                Receiver4Dword: 1,
                FrameMinusCDword: 0x80000000,
                FrameMinus4Dword: 0,
                CallReturnedNormally: true));
        check("Independent entry registers, zero read addresses, DWORD wrap, signed SAR, pushes, and normal return are exact",
            normal.Outcome == FrameHandoffCapacityAboveSecondTransferOutcome.ReturnedNormally
            && normal.Receiver4ReadAddress == 0
            && normal.FrameMinusCReadAddress == 0
            && normal.FrameMinus4ReadAddress == 8
            && normal.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityAboveSecondTransferRead(0x00BE6C6D, 0, 1),
                new FrameHandoffCapacityAboveSecondTransferRead(0x00BE6C70, 0, 0x80000000),
                new FrameHandoffCapacityAboveSecondTransferRead(0x00BE6C86, 8, 0)
            })
            && normal.EcxLeaReached
            && normal.EcxAfterLea == 0
            && normal.EdxSubtractReached
            && normal.EdxAfterSubtract == 0x80000000
            && normal.EdxAddReached
            && normal.EdxAfterAdd == 0x80000001
            && normal.EdxArithmeticShiftRightReached
            && normal.EdxAfterArithmeticShiftRight == 0xE0000000
            && normal.EbxLeaReached
            && normal.EbxAfterLea == 0x80000000
            && normal.Pushes.SequenceEqual(new[]
            {
                new FrameHandoffCapacityAboveSecondTransferPush(0x00BE6C84, "EBX", 0x80000000),
                new FrameHandoffCapacityAboveSecondTransferPush(0x00BE6C85, "ECX", 0),
                new FrameHandoffCapacityAboveSecondTransferPush(0x00BE6C86, "[EBP-0x4]", 0)
            })
            && normal.CallCellAddress == 0x01AC489C
            && normal.CallReached
            && normal.CallReturnedNormally == true
            && normal.NormalBoundaryReached
            && !normal.UnreachedReceiver4CaptureProvided
            && !normal.UnreachedFrameMinusCCaptureProvided
            && !normal.UnreachedFrameMinus4CaptureProvided
            && !normal.UnreachedCallReturnCaptureProvided
            && normal.StopAtRawFileOffsetExclusive == 0x00BE6C8F);

        FrameHandoffCapacityAboveSecondTransferPlan missingReceiver4 =
            FrameHandoffCapacityAboveSecondTransferProjection.Project(new(
                true, 0x1000, 0x2000, 0x3000, null, 0x11, 0x22, true));
        FrameHandoffCapacityAboveSecondTransferPlan missingFrameMinusC =
            FrameHandoffCapacityAboveSecondTransferProjection.Project(new(
                true, 0x1000, 0x2000, 0x3000, 0x44, null, 0x22, false));
        check("A missing receiver or frame-minus-C read stops exactly at that read and marks later captures unreached",
            missingReceiver4.Outcome
                == FrameHandoffCapacityAboveSecondTransferOutcome.MissingReceiver4Capture
            && missingReceiver4.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityAboveSecondTransferRead(0x00BE6C6D, 0x1004, null)
            })
            && missingReceiver4.Receiver4ReadReached
            && !missingReceiver4.Receiver4Captured
            && !missingReceiver4.FrameMinusCReadReached
            && missingReceiver4.UnreachedFrameMinusCCaptureProvided
            && missingReceiver4.UnreachedFrameMinus4CaptureProvided
            && missingReceiver4.UnreachedCallReturnCaptureProvided
            && missingReceiver4.StopAtRawFileOffsetExclusive == 0x00BE6C6D
            && missingFrameMinusC.Outcome
                == FrameHandoffCapacityAboveSecondTransferOutcome.MissingFrameMinusCCapture
            && missingFrameMinusC.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityAboveSecondTransferRead(0x00BE6C6D, 0x1004, 0x44),
                new FrameHandoffCapacityAboveSecondTransferRead(0x00BE6C70, 0x2FF4, null)
            })
            && missingFrameMinusC.Receiver4Captured
            && missingFrameMinusC.FrameMinusCReadReached
            && !missingFrameMinusC.FrameMinusCCaptured
            && missingFrameMinusC.Scalars.IsEmpty
            && missingFrameMinusC.Pushes.IsEmpty
            && missingFrameMinusC.UnreachedFrameMinus4CaptureProvided
            && missingFrameMinusC.UnreachedCallReturnCaptureProvided
            && missingFrameMinusC.StopAtRawFileOffsetExclusive == 0x00BE6C70);

        FrameHandoffCapacityAboveSecondTransferPlan missingFrameMinus4 =
            FrameHandoffCapacityAboveSecondTransferProjection.Project(new(
                true, 0x1000, 0x2000, 0x3000, 3, 0x5000, null, true));
        check("A missing frame-minus-four read preserves all computed scalars and the earlier pushes",
            missingFrameMinus4.Outcome
                == FrameHandoffCapacityAboveSecondTransferOutcome.MissingFrameMinus4Capture
            && missingFrameMinus4.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityAboveSecondTransferRead(0x00BE6C6D, 0x1004, 3),
                new FrameHandoffCapacityAboveSecondTransferRead(0x00BE6C70, 0x2FF4, 0x5000),
                new FrameHandoffCapacityAboveSecondTransferRead(0x00BE6C86, 0x2FFC, null)
            })
            && missingFrameMinus4.Scalars.Select(scalar => scalar.RawFileOffset)
                .SequenceEqual(new uint[]
                {
                    0x00BE6C73, 0x00BE6C76, 0x00BE6C78, 0x00BE6C7A, 0x00BE6C7D
                })
            && missingFrameMinus4.Pushes.SequenceEqual(new[]
            {
                new FrameHandoffCapacityAboveSecondTransferPush(
                    0x00BE6C84, "EBX", missingFrameMinus4.EbxAfterLea!.Value),
                new FrameHandoffCapacityAboveSecondTransferPush(
                    0x00BE6C85, "ECX", missingFrameMinus4.EcxAfterLea!.Value)
            })
            && missingFrameMinus4.FrameMinus4ReadReached
            && !missingFrameMinus4.FrameMinus4Captured
            && !missingFrameMinus4.FrameMinus4PushReached
            && !missingFrameMinus4.CallReached
            && missingFrameMinus4.UnreachedCallReturnCaptureProvided
            && missingFrameMinus4.StopAtRawFileOffsetExclusive == 0x00BE6C86);

        FrameHandoffCapacityAboveSecondTransferPlan missingReturn =
            Project(callReturnedNormally: null);
        FrameHandoffCapacityAboveSecondTransferPlan nonreturning =
            Project(callReturnedNormally: false);
        check("Missing and nonreturning indirect-call outcomes stop at 89 without claiming the normal boundary",
            missingReturn.Outcome
                == FrameHandoffCapacityAboveSecondTransferOutcome.MissingCallReturnCapture
            && missingReturn.CallReached
            && missingReturn.CallReturnedNormally is null
            && missingReturn.Pushes.Length == 3
            && !missingReturn.NormalBoundaryReached
            && missingReturn.StopAtRawFileOffsetExclusive == 0x00BE6C89
            && nonreturning.Outcome
                == FrameHandoffCapacityAboveSecondTransferOutcome.CallDidNotReturnNormally
            && nonreturning.CallReached
            && nonreturning.CallReturnedNormally == false
            && nonreturning.Pushes.Length == 3
            && !nonreturning.NormalBoundaryReached
            && nonreturning.StopAtRawFileOffsetExclusive == 0x00BE6C89);
    }

    private static FrameHandoffCapacityAboveSecondTransferPlan Project(
        bool? callReturnedNormally) =>
        FrameHandoffCapacityAboveSecondTransferProjection.Project(new(
            ValidatedNormalEntry: true,
            EntryEbx: 0xFFFFFFFF,
            EntryEdi: 0x1234,
            EntryEbp: 0x10,
            Receiver4Dword: 0,
            FrameMinusCDword: 0x5678,
            FrameMinus4Dword: 0x9ABC,
            CallReturnedNormally: callReturnedNormally));

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x00BE6C6D, "8B 43 04"),
            (0x00BE6C70, "8B 55 F4"),
            (0x00BE6C73, "8D 0C 07"),
            (0x00BE6C76, "2B D1"),
            (0x00BE6C78, "03 D0"),
            (0x00BE6C7A, "C1 FA 02"),
            (0x00BE6C7D, "8D 1C 95 00 00 00 00"),
            (0x00BE6C84, "53"),
            (0x00BE6C85, "51"),
            (0x00BE6C86, "FF 75 FC"),
            (0x00BE6C89, "FF 15 9C 48 AC 01")
        };
        uint nextRawOffset = FrameHandoffCapacityAboveSecondTransferProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (FrameHandoffCapacityAboveSecondTransferInstructionEvidence instruction
            in FrameHandoffCapacityAboveSecondTransferProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacityAboveSecondTransferProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == FrameHandoffCapacityAboveSecondTransferProjection.NormalBoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                FrameHandoffCapacityAboveSecondTransferProjection.CapturedWindowBytes)
            && FrameHandoffCapacityAboveSecondTransferProjection.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
