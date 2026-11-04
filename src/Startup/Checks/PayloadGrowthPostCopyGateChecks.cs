using Firefall.Reconstruction.Startup;

internal static class PayloadGrowthPostCopyGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE identity, raw range, VA mapping, and exact instruction bytes are pinned",
            PayloadGrowthPostCopyGateProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadGrowthPostCopyGateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadGrowthPostCopyGateProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PayloadGrowthPostCopyGateProjection.GateStartRawFileOffset == 0x00BE694D
            && PayloadGrowthPostCopyGateProjection.GateEndRawFileOffsetExclusive
                == 0x00BE6960
            && PayloadGrowthPostCopyGateProjection.CapturedCodeLength == 19
            && PayloadGrowthPostCopyGateProjection.GateStartVirtualAddress == 0x00FE754D
            && PayloadGrowthPostCopyGateProjection.GateEndVirtualAddressExclusive
                == 0x00FE7560
            && PayloadGrowthPostCopyGateProjection.UnsignedGreaterThanBranchTargetRawFileOffset
                == 0x00BE69A7
            && PayloadGrowthPostCopyGateProjection.UnsignedGreaterThanBranchTargetVirtualAddress
                == 0x00FE75A7
            && string.Concat(PayloadGrowthPostCopyGateProjection.CapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B4B0483C40C8D14078B7DF88955F03BFE7747"
            && VerifyInstructionEvidence());

        PayloadGrowthPostCopyGatePlan normal = Project(
            retainedEdiByteCount: 0x100,
            retainedEsiGrowthDelta: 0x20,
            priorCallEax: 0x40,
            receiverDword4: 0,
            frameDwordMinus8: 0x20);
        check("The normal path reads receiver first, exposes cleanup after that read, stores wrapped EDX, and takes equality fallthrough",
            normal.Outcome
                == PayloadGrowthPostCopyGateOutcome.UnsignedGreaterThanBranchNotTaken
            && normal.ReceiverDword4ReadAddress == 0x1004
            && normal.ReceiverDword4Read
            && normal.ReceiverDword4Value == 0
            && normal.StackCleanupBytes == 12
            && normal.PriorCallEaxUsed == 0x40
            && normal.Edx == 0x140
            && normal.FrameDwordMinus8ReadAddress == 0x1FF8
            && normal.FrameDwordMinus8Read
            && normal.FrameDwordMinus10WriteAddress == 0x1FF0
            && normal.FrameDwordMinus10Written
            && normal.FrameDwordMinus10WriteValue == 0x140
            && normal.ComparedEdi == 0x20
            && normal.ComparedEsi == 0x20
            && normal.UnsignedGreaterThan == false
            && normal.RouteRawFileOffset == 0x00BE6960
            && normal.StopAtRawFileOffsetExclusive == 0x00BE6960);

        PayloadGrowthPostCopyGatePlan unsignedMaximumEquality = Project(
            retainedEsiGrowthDelta: uint.MaxValue,
            frameDwordMinus8: uint.MaxValue);
        PayloadGrowthPostCopyGatePlan unsignedMaximumGreater = Project(
            retainedEsiGrowthDelta: 0xFFFFFFFE,
            frameDwordMinus8: uint.MaxValue);
        PayloadGrowthPostCopyGatePlan highBitUnsignedGreater = Project(
            retainedEsiGrowthDelta: 0x7FFFFFFF,
            frameDwordMinus8: 0x80000000);
        check("The JA comparison is unsigned, including equality, maximum DWORD, and the high-bit boundary",
            unsignedMaximumEquality.Outcome
                == PayloadGrowthPostCopyGateOutcome.UnsignedGreaterThanBranchNotTaken
            && unsignedMaximumEquality.UnsignedGreaterThan == false
            && unsignedMaximumGreater.Outcome
                == PayloadGrowthPostCopyGateOutcome.UnsignedGreaterThanBranchTaken
            && unsignedMaximumGreater.UnsignedGreaterThan == true
            && unsignedMaximumGreater.RouteRawFileOffset == 0x00BE69A7
            && highBitUnsignedGreater.Outcome
                == PayloadGrowthPostCopyGateOutcome.UnsignedGreaterThanBranchTaken
            && highBitUnsignedGreater.UnsignedGreaterThan == true);

        PayloadGrowthPostCopyGatePlan wrappedPointersAndEdx = Project(
            receiverPointer: uint.MaxValue - 3,
            framePointer: 3,
            retainedEdiByteCount: uint.MaxValue - 0x0F,
            priorCallEax: 0x30,
            receiverDword4: 0,
            frameDwordMinus8: 0);
        check("Receiver/frame addresses and EDX arithmetic wrap as DWORDs, and zero captures remain valid",
            wrappedPointersAndEdx.ReceiverDword4ReadAddress == 0
            && wrappedPointersAndEdx.FrameDwordMinus8ReadAddress == 0xFFFFFFFB
            && wrappedPointersAndEdx.FrameDwordMinus10WriteAddress == 0xFFFFFFF3
            && wrappedPointersAndEdx.Edx == 0x20
            && wrappedPointersAndEdx.FrameDwordMinus10WriteValue == 0x20
            && wrappedPointersAndEdx.ReceiverDword4Read
            && wrappedPointersAndEdx.FrameDwordMinus8Read);

        PayloadGrowthPostCopyGatePlan missingReturnStatus = Project(
            priorCallReturnedNormally: null,
            priorCallEax: null,
            receiverDword4: null,
            frameDwordMinus8: null);
        PayloadGrowthPostCopyGatePlan laterAfterMissingStatus = Project(
            priorCallReturnedNormally: null,
            priorCallEax: 1);
        PayloadGrowthPostCopyGatePlan nonreturningPriorCall = Project(
            priorCallReturnedNormally: false,
            priorCallEax: null,
            receiverDword4: null,
            frameDwordMinus8: null);
        PayloadGrowthPostCopyGatePlan laterAfterNonreturn = Project(
            priorCallReturnedNormally: false,
            priorCallEax: 1);
        check("Missing or nonreturning prior-call status rejects subsequent captures",
            missingReturnStatus.Outcome
                == PayloadGrowthPostCopyGateOutcome.PriorCallReturnStatusNotCaptured
            && missingReturnStatus.StopAtRawFileOffsetExclusive == 0x00BE694D
            && laterAfterMissingStatus.Outcome
                == PayloadGrowthPostCopyGateOutcome.UnreachedCaptureProvided
            && nonreturningPriorCall.Outcome
                == PayloadGrowthPostCopyGateOutcome.PriorCallDidNotReturnNormally
            && laterAfterNonreturn.Outcome
                == PayloadGrowthPostCopyGateOutcome.UnreachedCaptureProvided
            && laterAfterNonreturn.StackCleanupBytes is null);

        PayloadGrowthPostCopyGatePlan missingReceiverRead = Project(
            receiverDword4: null,
            priorCallEax: null,
            frameDwordMinus8: null);
        PayloadGrowthPostCopyGatePlan laterAfterMissingReceiver = Project(
            receiverDword4: null,
            priorCallEax: 0);
        PayloadGrowthPostCopyGatePlan missingEax = Project(
            priorCallEax: null,
            frameDwordMinus8: null);
        PayloadGrowthPostCopyGatePlan laterAfterMissingEax = Project(
            priorCallEax: null,
            frameDwordMinus8: 0);
        check("Strict capture gates preserve read order and expose cleanup only after the receiver read",
            missingReceiverRead.Outcome
                == PayloadGrowthPostCopyGateOutcome.MissingReceiverDword4Capture
            && missingReceiverRead.StackCleanupBytes is null
            && missingReceiverRead.StopAtRawFileOffsetExclusive == 0x00BE694D
            && laterAfterMissingReceiver.Outcome
                == PayloadGrowthPostCopyGateOutcome.UnreachedCaptureProvided
            && missingEax.Outcome
                == PayloadGrowthPostCopyGateOutcome.MissingPriorCallEaxCapture
            && missingEax.ReceiverDword4Read
            && missingEax.ReceiverDword4Value == 0
            && missingEax.StackCleanupBytes == 12
            && missingEax.Edx is null
            && missingEax.StopAtRawFileOffsetExclusive == 0x00BE6953
            && laterAfterMissingEax.Outcome
                == PayloadGrowthPostCopyGateOutcome.UnreachedCaptureProvided
            && laterAfterMissingEax.StackCleanupBytes == 12);

        PayloadGrowthPostCopyGatePlan missingFrameMinus8 = Project(
            frameDwordMinus8: null);
        check("A missing frame-8 capture stops before its dependent frame-10 write",
            missingFrameMinus8.Outcome
                == PayloadGrowthPostCopyGateOutcome.MissingFrameDwordMinus8Capture
            && missingFrameMinus8.StackCleanupBytes == 12
            && missingFrameMinus8.PriorCallEaxUsed == 0
            && missingFrameMinus8.Edx == 0x100
            && missingFrameMinus8.FrameDwordMinus8ReadAddress == 0x1FF8
            && !missingFrameMinus8.FrameDwordMinus8Read
            && missingFrameMinus8.FrameDwordMinus10WriteAddress is null
            && !missingFrameMinus8.FrameDwordMinus10Written
            && missingFrameMinus8.FrameDwordMinus10WriteValue is null
            && missingFrameMinus8.StopAtRawFileOffsetExclusive == 0x00BE6956);
    }

    private static PayloadGrowthPostCopyGatePlan Project(
        uint receiverPointer = 0x1000,
        uint framePointer = 0x2000,
        uint retainedEdiByteCount = 0x100,
        uint retainedEsiGrowthDelta = 0x20,
        bool? priorCallReturnedNormally = true,
        uint? priorCallEax = 0,
        uint? receiverDword4 = 0,
        uint? frameDwordMinus8 = 0x10) =>
        PayloadGrowthPostCopyGateProjection.Project(new(
            ReceiverPointer: receiverPointer,
            FramePointer: framePointer,
            RetainedEdiByteCount: retainedEdiByteCount,
            RetainedEsiGrowthDelta: retainedEsiGrowthDelta,
            PriorCallReturnedNormally: priorCallReturnedNormally,
            PriorCallEax: priorCallEax,
            ReceiverDword4: receiverDword4,
            FrameDwordMinus8: frameDwordMinus8));

    private static bool VerifyInstructionEvidence()
    {
        uint nextRawOffset = PayloadGrowthPostCopyGateProjection.GateStartRawFileOffset;
        string instructionBytes = string.Empty;
        foreach (PayloadGrowthPostCopyGateInstructionEvidence instruction
            in PayloadGrowthPostCopyGateProjection.Instructions)
        {
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != PayloadGrowthPostCopyGateProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            instructionBytes += string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            nextRawOffset += (uint)instruction.Bytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Length;
        }

        PayloadGrowthPostCopyGateInstructionEvidence branch =
            PayloadGrowthPostCopyGateProjection.Instructions[^1];
        return nextRawOffset
                == PayloadGrowthPostCopyGateProjection.GateEndRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(PayloadGrowthPostCopyGateProjection.CapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            && branch.BranchTargetRawFileOffset == 0x00BE69A7
            && branch.BranchTargetVirtualAddress == 0x00FE75A7;
    }
}
