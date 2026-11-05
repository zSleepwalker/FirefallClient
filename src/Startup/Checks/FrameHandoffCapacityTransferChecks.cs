using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityTransferChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE identity and flat-IDA raw-byte window map to the expected VA range",
            FrameHandoffCapacityTransferProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityTransferProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityTransferProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityTransferProjection.EntryRawFileOffset == 0x00BE6BCA
            && FrameHandoffCapacityTransferProjection.EntryVirtualAddress == 0x00FE77CA
            && FrameHandoffCapacityTransferProjection.NormalBoundaryRawFileOffsetExclusive
                == 0x00BE6BFD
            && FrameHandoffCapacityTransferProjection.NormalBoundaryVirtualAddressExclusive
                == 0x00FE77FD
            && FrameHandoffCapacityTransferProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityTransferProjection.EntryRawFileOffset)
                == FrameHandoffCapacityTransferProjection.EntryVirtualAddress
            && FrameHandoffCapacityTransferProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityTransferProjection.NormalBoundaryRawFileOffsetExclusive)
                == FrameHandoffCapacityTransferProjection.NormalBoundaryVirtualAddressExclusive
            && Normalize(FrameHandoffCapacityTransferProjection.CapturedWindowBytes)
                == "8B4B048D04BD000000008945F48D14088B4308C1E0022BC203C1C1F802"
                    + "8D3C85000000008B45FC0345F4575250FF159C48AC01"
            && FrameHandoffCapacityTransferProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x33
            && InstructionEvidenceMatches());

        FrameHandoffCapacityTransferPlan missingReceiver4 =
            FrameHandoffCapacityTransferProjection.Project(new(
                EntryEbx: 0x8000,
                EntryEbp: 0x9000,
                EntryEdi: 7,
                Receiver4: null,
                Receiver8: 8,
                FrameMinus4: 0x10,
                FrameMinusC: 0x20,
                HelperReturnedNormally: true));
        check("An unreached receiver-4 read reports every supplied later capture",
            missingReceiver4.Outcome
                == FrameHandoffCapacityTransferOutcome.MissingReceiver4Capture
            && missingReceiver4.UnreachedReceiver8CaptureProvided
            && missingReceiver4.UnreachedFrameMinus4CaptureProvided
            && missingReceiver4.UnreachedFrameMinusCCaptureProvided
            && missingReceiver4.UnreachedHelperReturnCaptureProvided
            && missingReceiver4.FrameWrites.IsEmpty
            && missingReceiver4.StopAtRawFileOffsetExclusive == 0x00BE6BCA);

        FrameHandoffCapacityTransferPlan missingReceiver8 =
            FrameHandoffCapacityTransferProjection.Project(new(
                EntryEbx: 0x1000,
                EntryEbp: 0x2000,
                EntryEdi: 3,
                Receiver4: 5,
                Receiver8: null,
                FrameMinus4: 0x10,
                FrameMinusC: 0x20,
                HelperReturnedNormally: true));
        check("A missing receiver-8 read stops there while retaining the earlier frame write",
            missingReceiver8.Outcome
                == FrameHandoffCapacityTransferOutcome.MissingReceiver8Capture
            && missingReceiver8.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityTransferRead(0x00BE6BCA, 0x1004, 5),
                new FrameHandoffCapacityTransferRead(0x00BE6BDA, 0x1008, null)
            })
            && missingReceiver8.FrameWrites.SequenceEqual(new[]
            {
                new FrameHandoffCapacityTransferFrameWrite(0x00BE6BD4, 0x1FF4, 12)
            })
            && missingReceiver8.MemoryAccesses.SequenceEqual(new[]
            {
                new FrameHandoffCapacityTransferMemoryAccess(
                    FrameHandoffCapacityTransferMemoryAccessKind.Read, 0x00BE6BCA, 0x1004, 5),
                new FrameHandoffCapacityTransferMemoryAccess(
                    FrameHandoffCapacityTransferMemoryAccessKind.Write, 0x00BE6BD4, 0x1FF4, 12),
                new FrameHandoffCapacityTransferMemoryAccess(
                    FrameHandoffCapacityTransferMemoryAccessKind.Read, 0x00BE6BDA, 0x1008, null)
            })
            && missingReceiver8.EaxAfterLea == 12
            && missingReceiver8.EdxAfterLea == 17
            && !missingReceiver8.Receiver8ShiftReached
            && missingReceiver8.HelperReturnedNormally is null
            && !missingReceiver8.UnreachedReceiver8CaptureProvided
            && missingReceiver8.UnreachedFrameMinus4CaptureProvided
            && missingReceiver8.UnreachedFrameMinusCCaptureProvided
            && missingReceiver8.UnreachedHelperReturnCaptureProvided
            && missingReceiver8.ScalarPushes.IsEmpty
            && !missingReceiver8.IndirectCallReached
            && missingReceiver8.StopAtRawFileOffsetExclusive == 0x00BE6BDA);

        FrameHandoffCapacityTransferPlan normal =
            FrameHandoffCapacityTransferProjection.Project(new(
                EntryEbx: 0x3000,
                EntryEbp: 0x4000,
                EntryEdi: 3,
                Receiver4: 5,
                Receiver8: 8,
                FrameMinus4: 0x20,
                FrameMinusC: 0x30,
                HelperReturnedNormally: true));
        check("Independent frame reads feed scalar pushes in order and normal return stops at FD",
            normal.Outcome == FrameHandoffCapacityTransferOutcome.ReturnedNormally
            && normal.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityTransferRead(0x00BE6BCA, 0x3004, 5),
                new FrameHandoffCapacityTransferRead(0x00BE6BDA, 0x3008, 8),
                new FrameHandoffCapacityTransferRead(0x00BE6BEE, 0x3FFC, 0x20),
                new FrameHandoffCapacityTransferRead(0x00BE6BF1, 0x3FF4, 0x30)
            })
            && normal.FrameWrites.SequenceEqual(new[]
            {
                new FrameHandoffCapacityTransferFrameWrite(0x00BE6BD4, 0x3FF4, 12)
            })
            && normal.MemoryAccesses.Select(access =>
                    (access.Kind, access.RawFileOffset, access.Address, access.Value))
                .SequenceEqual(new (
                    FrameHandoffCapacityTransferMemoryAccessKind Kind,
                    uint RawFileOffset,
                    uint Address,
                    uint? Value)[]
                {
                    (FrameHandoffCapacityTransferMemoryAccessKind.Read, 0x00BE6BCA, 0x3004, (uint?)5),
                    (FrameHandoffCapacityTransferMemoryAccessKind.Write, 0x00BE6BD4, 0x3FF4, (uint?)12),
                    (FrameHandoffCapacityTransferMemoryAccessKind.Read, 0x00BE6BDA, 0x3008, (uint?)8),
                    (FrameHandoffCapacityTransferMemoryAccessKind.Read, 0x00BE6BEE, 0x3FFC, (uint?)0x20),
                    (FrameHandoffCapacityTransferMemoryAccessKind.Read, 0x00BE6BF1, 0x3FF4, (uint?)0x30)
                })
            && normal.EaxAfterLea == 12
            && normal.EdxAfterLea == 17
            && normal.Receiver8AfterShift == 32
            && normal.EaxAfterSubtract == 15
            && normal.EaxAfterAddReceiver4 == 20
            && normal.EaxAfterArithmeticShiftRight == 5
            && normal.EdiAfterLea == 20
            && normal.EaxAfterFrameMinusCAdd == 0x50
            && normal.ScalarPushes.SequenceEqual(new[]
            {
                new FrameHandoffCapacityTransferScalarPush(0x00BE6BF4, "EDI", 20),
                new FrameHandoffCapacityTransferScalarPush(0x00BE6BF5, "EDX", 17),
                new FrameHandoffCapacityTransferScalarPush(0x00BE6BF6, "EAX", 0x50)
            })
            && normal.IndirectCallReached
            && normal.IndirectCallCellAddress == 0x01AC489C
            && normal.HelperReturnedNormally == true
            && normal.NormalBoundaryReached
            && !normal.UnreachedReceiver8CaptureProvided
            && !normal.UnreachedFrameMinus4CaptureProvided
            && !normal.UnreachedFrameMinusCCaptureProvided
            && !normal.UnreachedHelperReturnCaptureProvided
            && normal.StopAtRawFileOffsetExclusive == 0x00BE6BFD);

        FrameHandoffCapacityTransferPlan missingFrameMinusC =
            FrameHandoffCapacityTransferProjection.Project(new(
                EntryEbx: 0x5000,
                EntryEbp: 0x6000,
                EntryEdi: 0x40000000,
                Receiver4: 0,
                Receiver8: 0,
                FrameMinus4: 7,
                FrameMinusC: null,
                HelperReturnedNormally: null));
        check("A missing independent frame-C read retains the preceding frame write but stops before pushes",
            missingFrameMinusC.Outcome
                == FrameHandoffCapacityTransferOutcome.MissingFrameMinusCCapture
            && missingFrameMinusC.FrameWrites.Single().Value == 0
            && missingFrameMinusC.Reads[^1]
                == new FrameHandoffCapacityTransferRead(0x00BE6BF1, 0x5FF4, null)
            && missingFrameMinusC.EdiAfterLea == 0
            && missingFrameMinusC.FrameMinusCAddReached == false
            && !missingFrameMinusC.UnreachedFrameMinusCCaptureProvided
            && !missingFrameMinusC.UnreachedHelperReturnCaptureProvided
            && missingFrameMinusC.ScalarPushes.IsEmpty
            && !missingFrameMinusC.IndirectCallReached
            && missingFrameMinusC.StopAtRawFileOffsetExclusive == 0x00BE6BF1);

        FrameHandoffCapacityTransferPlan missingHelperCapture =
            FrameHandoffCapacityTransferProjection.Project(new(
                EntryEbx: uint.MaxValue,
                EntryEbp: 3,
                EntryEdi: uint.MaxValue,
                Receiver4: uint.MaxValue,
                Receiver8: uint.MaxValue,
                FrameMinus4: 1,
                FrameMinusC: 2,
                HelperReturnedNormally: null));
        FrameHandoffCapacityTransferPlan nonNormal =
            FrameHandoffCapacityTransferProjection.Project(
                missingHelperCapture.Capture with { HelperReturnedNormally = false });
        check("The helper normal gate is nullable, preserves completed pushes, and excludes later calls",
            missingHelperCapture.Outcome
                == FrameHandoffCapacityTransferOutcome.MissingHelperNormalCapture
            && missingHelperCapture.ScalarPushes.Select(push => push.Register)
                .SequenceEqual(new[] { "EDI", "EDX", "EAX" })
            && missingHelperCapture.IndirectCallReached
            && !missingHelperCapture.NormalBoundaryReached
            && missingHelperCapture.StopAtRawFileOffsetExclusive == 0x00BE6BF7
            && nonNormal.Outcome
                == FrameHandoffCapacityTransferOutcome.HelperDidNotReturnNormally
            && nonNormal.HelperReturnedNormally == false
            && !nonNormal.NormalBoundaryReached
            && nonNormal.StopAtRawFileOffsetExclusive == 0x00BE6BF7
            && nonNormal.ScalarPushes.SequenceEqual(missingHelperCapture.ScalarPushes));

        FrameHandoffCapacityTransferPlan missingFrameMinus4 =
            FrameHandoffCapacityTransferProjection.Project(new(
                EntryEbx: 0,
                EntryEbp: 2,
                EntryEdi: 1,
                Receiver4: 0,
                Receiver8: 0,
                FrameMinus4: null,
                FrameMinusC: 1,
                HelperReturnedNormally: true));
        check("A missing frame-minus-four read preserves the prior frame write and stops before frame-C read",
            missingFrameMinus4.Outcome
                == FrameHandoffCapacityTransferOutcome.MissingFrameMinus4Capture
            && missingFrameMinus4.FrameWrites.Single().Address == uint.MaxValue - 9
            && missingFrameMinus4.Reads[^1]
                == new FrameHandoffCapacityTransferRead(0x00BE6BEE, 0xFFFFFFFE, null)
            && missingFrameMinus4.Reads.Length == 3
            && !missingFrameMinus4.FrameMinusCAddReached
            && !missingFrameMinus4.UnreachedReceiver8CaptureProvided
            && !missingFrameMinus4.UnreachedFrameMinus4CaptureProvided
            && missingFrameMinus4.UnreachedFrameMinusCCaptureProvided
            && missingFrameMinus4.UnreachedHelperReturnCaptureProvided
            && missingFrameMinus4.ScalarPushes.IsEmpty);

        FrameHandoffCapacityTransferPlan signedArithmeticShift =
            FrameHandoffCapacityTransferProjection.Project(new(
                EntryEbx: 0x1000,
                EntryEbp: 0x2000,
                EntryEdi: 0,
                Receiver4: 0,
                Receiver8: 0x20000000,
                FrameMinus4: uint.MaxValue,
                FrameMinusC: 2,
                HelperReturnedNormally: true));
        check("The transfer projects SAR for a negative DWORD and retains wrapped scalar arithmetic",
            signedArithmeticShift.Receiver8AfterShift == 0x80000000
            && signedArithmeticShift.EaxAfterSubtract == 0x80000000
            && signedArithmeticShift.EaxAfterAddReceiver4 == 0x80000000
            && signedArithmeticShift.EaxAfterArithmeticShiftRight == 0xE0000000
            && signedArithmeticShift.EdiAfterLea == 0x80000000
            && signedArithmeticShift.EaxAfterFrameMinusCAdd == 1
            && signedArithmeticShift.ScalarPushes.SequenceEqual(new[]
            {
                new FrameHandoffCapacityTransferScalarPush(0x00BE6BF4, "EDI", 0x80000000),
                new FrameHandoffCapacityTransferScalarPush(0x00BE6BF5, "EDX", 0),
                new FrameHandoffCapacityTransferScalarPush(0x00BE6BF6, "EAX", 1)
            }));

        check("Address arithmetic and scalar math preserve DWORD wrap and signed SAR semantics",
            FrameHandoffCapacityTransferProjection.Receiver4ReadAddress(uint.MaxValue) == 3
            && FrameHandoffCapacityTransferProjection.Receiver8ReadAddress(uint.MaxValue) == 7
            && FrameHandoffCapacityTransferProjection.FrameMinus4Address(2) == 0xFFFFFFFE
            && FrameHandoffCapacityTransferProjection.FrameMinusCAddress(3)
                == 0xFFFFFFF7
            && FrameHandoffCapacityTransferProjection.ArithmeticShiftRightByTwo(0x80000003)
                == 0xE0000000
            && FrameHandoffCapacityTransferProjection.ArithmeticShiftRightByTwo(0x7FFFFFFF)
                == 0x1FFFFFFF);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x00BE6BCA, "8B 4B 04"),
            (0x00BE6BCD, "8D 04 BD 00 00 00 00"),
            (0x00BE6BD4, "89 45 F4"),
            (0x00BE6BD7, "8D 14 08"),
            (0x00BE6BDA, "8B 43 08"),
            (0x00BE6BDD, "C1 E0 02"),
            (0x00BE6BE0, "2B C2"),
            (0x00BE6BE2, "03 C1"),
            (0x00BE6BE4, "C1 F8 02"),
            (0x00BE6BE7, "8D 3C 85 00 00 00 00"),
            (0x00BE6BEE, "8B 45 FC"),
            (0x00BE6BF1, "03 45 F4"),
            (0x00BE6BF4, "57"),
            (0x00BE6BF5, "52"),
            (0x00BE6BF6, "50"),
            (0x00BE6BF7, "FF 15 9C 48 AC 01")
        };
        uint nextRawOffset = FrameHandoffCapacityTransferProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (FrameHandoffCapacityTransferInstructionEvidence instruction
            in FrameHandoffCapacityTransferProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacityTransferProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == FrameHandoffCapacityTransferProjection.NormalBoundaryRawFileOffsetExclusive
            && bytes == Normalize(FrameHandoffCapacityTransferProjection.CapturedWindowBytes)
            && FrameHandoffCapacityTransferProjection.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
