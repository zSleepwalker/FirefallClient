using Firefall.Reconstruction.Startup;

internal static class ReliableReceiveSlotPublicationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The exact publication bytes, sample identity, raw bounds, and VA mapping are pinned",
            ReliableReceiveSlotPublicationProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ReliableReceiveSlotPublicationProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReliableReceiveSlotPublicationProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReliableReceiveSlotPublicationProjection.PublicationStartRawFileOffset
                == 0x00BE53E9
            && ReliableReceiveSlotPublicationProjection.PublicationStartVirtualAddress
                == 0x00FE5FE9
            && ReliableReceiveSlotPublicationProjection.RawFileOffsetToVirtualAddress(0x00BE53E9)
                == 0x00FE5FE9
            && ReliableReceiveSlotPublicationProjection.PublicationEndRawFileOffsetExclusive
                == 0x00BE53F7
            && ReliableReceiveSlotPublicationProjection.RawFileOffsetToVirtualAddress(0x00BE53F7)
                == 0x00FE5FF7
            && ReliableReceiveSlotPublicationProjection.CapturedPublicationBytes
                == "8B 45 08 89 38 B0 01 5F 5E 5B 5D C2 04 00"
            && ReliableReceiveSlotPublicationProjection.CapturedPublicationBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 14
            && InstructionEvidenceMatches());

        ReliableReceiveSlotPublicationPlan missingCapture =
            ReliableReceiveSlotPublicationProjection.Project(
                0x12345678,
                0x87654321,
                new ReliableReceiveSlotPublicationCapture(null));
        check("A missing frame+8 capture models no publication, AL update, pops, or return",
            missingCapture.Outcome
                == ReliableReceiveSlotPublicationOutcome.MissingFrameArgument8Capture
            && missingCapture.FrameArgument8Address == 0x12345680
            && !missingCapture.FrameArgument8Read
            && missingCapture.CapturedPointer is null
            && !missingCapture.PayloadDwordWriteProjected
            && missingCapture.PayloadDwordWriteAddress is null
            && missingCapture.PayloadDwordWriteValue is null
            && !missingCapture.AlOneEffectProjected
            && missingCapture.ProjectedEaxAfterAlOne is null
            && missingCapture.StackPopRegisters.IsEmpty
            && missingCapture.ReturnStackBytes is null
            && missingCapture.StopAtRawFileOffsetExclusive
                == ReliableReceiveSlotPublicationProjection.FrameArgument8ReadRawFileOffset);

        ReliableReceiveSlotPublicationPlan independentFrameArgument =
            ReliableReceiveSlotPublicationProjection.Project(
                0xFFFFFFF8,
                0x87654321,
                new ReliableReceiveSlotPublicationCapture(0x123456AB));
        check("The DWORD reload uses wrapped EBP+8 and the independent capture as the publication address",
            ReliableReceiveSlotPublicationProjection.FrameArgument8Offset == 8
            && ReliableReceiveSlotPublicationProjection.FrameArgument8Address(0xFFFFFFF8) == 0
            && independentFrameArgument.FrameArgument8Address == 0
            && independentFrameArgument.FrameArgument8Read
            && independentFrameArgument.CapturedPointer == 0x123456AB
            && independentFrameArgument.PayloadDwordWriteProjected
            && independentFrameArgument.PayloadDwordWriteAddress == 0x123456AB
            && independentFrameArgument.PayloadDwordWriteValue == 0x87654321
            && independentFrameArgument.Outcome
                == ReliableReceiveSlotPublicationOutcome.FrameArgument8CaptureAvailable);

        ReliableReceiveSlotPublicationPlan zeroPointer =
            ReliableReceiveSlotPublicationProjection.Project(
                0x1000,
                0xA1B2C3D4,
                new ReliableReceiveSlotPublicationCapture(0));
        ReliableReceiveSlotPublicationPlan nonzeroPointer =
            ReliableReceiveSlotPublicationProjection.Project(
                0x2000,
                0x01020304,
                new ReliableReceiveSlotPublicationCapture(0xFEDCBA98));
        check("Zero is a captured pointer value, and AL=1 preserves the pointer's upper 24 EAX bits",
            zeroPointer.FrameArgument8Read
            && zeroPointer.CapturedPointer == 0
            && zeroPointer.PayloadDwordWriteProjected
            && zeroPointer.PayloadDwordWriteAddress == 0
            && zeroPointer.PayloadDwordWriteValue == 0xA1B2C3D4
            && zeroPointer.AlOneEffectProjected
            && zeroPointer.ProjectedEaxAfterAlOne == 1
            && nonzeroPointer.PayloadDwordWriteAddress == 0xFEDCBA98
            && nonzeroPointer.AlOneEffectProjected
            && nonzeroPointer.ProjectedEaxAfterAlOne == 0xFEDCBA01);

        check("The payload write precedes AL clobbering and the observed pops and RET 4",
            ReliableReceiveSlotPublicationProjection.PayloadDwordWriteRawFileOffset
                < ReliableReceiveSlotPublicationProjection.AlOneRawFileOffset
            && independentFrameArgument.PayloadDwordWriteProjected
            && independentFrameArgument.PayloadDwordWriteValue
                == independentFrameArgument.PayloadEdi
            && independentFrameArgument.AlOneEffectProjected
            && independentFrameArgument.ProjectedEaxAfterAlOne
                == ((independentFrameArgument.CapturedPointer!.Value & 0xFFFFFF00) | 1)
            && independentFrameArgument.StackPopRegisters.SequenceEqual(
                new[] { "EDI", "ESI", "EBX", "EBP" })
            && independentFrameArgument.ReturnStackBytes == 4
            && independentFrameArgument.StopAtRawFileOffsetExclusive == 0x00BE53F7);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes)[]
        {
            (0x00BE53E9, 0x00FE5FE9, "8B 45 08"),
            (0x00BE53EC, 0x00FE5FEC, "89 38"),
            (0x00BE53EE, 0x00FE5FEE, "B0 01"),
            (0x00BE53F0, 0x00FE5FF0, "5F 5E 5B 5D"),
            (0x00BE53F4, 0x00FE5FF4, "C2 04 00")
        };
        return ReliableReceiveSlotPublicationProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes))
                .SequenceEqual(expected)
            && ReliableReceiveSlotPublicationProjection.FrameArgument8ReadRawFileOffset
                == 0x00BE53E9
            && ReliableReceiveSlotPublicationProjection.PayloadDwordWriteRawFileOffset
                == 0x00BE53EC
            && ReliableReceiveSlotPublicationProjection.AlOneRawFileOffset == 0x00BE53EE
            && ReliableReceiveSlotPublicationProjection.StackPopsRawFileOffset == 0x00BE53F0
            && ReliableReceiveSlotPublicationProjection.ReturnRawFileOffset == 0x00BE53F4
            && ReliableReceiveSlotPublicationProjection.ReturnStackBytes == 4;
    }
}
