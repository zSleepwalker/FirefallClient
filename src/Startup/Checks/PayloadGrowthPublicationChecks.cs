using Firefall.Reconstruction.Startup;

internal static class PayloadGrowthPublicationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The PE identity, raw range, VA mapping, and exact IDA instruction bytes are pinned",
            PayloadGrowthPublicationProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadGrowthPublicationProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadGrowthPublicationProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PayloadGrowthPublicationProjection.StartRawFileOffset == 0x00BE69F2
            && PayloadGrowthPublicationProjection.CounterAddRawFileOffset == 0x00BE6A01
            && PayloadGrowthPublicationProjection.PointerStoreRawFileOffset == 0x00BE6A04
            && PayloadGrowthPublicationProjection.OpaqueCallRawFileOffset == 0x00BE69FC
            && PayloadGrowthPublicationProjection.NormalBoundaryRawFileOffsetExclusive
                == 0x00BE6A10
            && PayloadGrowthPublicationProjection.CapturedCodeLength == 30
            && PayloadGrowthPublicationProjection.StartVirtualAddress == 0x00FE75F2
            && PayloadGrowthPublicationProjection.NormalBoundaryVirtualAddressExclusive
                == 0x00FE7610
            && PayloadGrowthPublicationProjection.JumpZeroTargetRawFileOffset == 0x00BE6A01
            && PayloadGrowthPublicationProjection.OpaqueCallTargetAddress == 0x12C8D60
            && string.Concat(PayloadGrowthPublicationProjection.CapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B7DEC8B4F0485C97405E85F236E00017708895F045F5E5B8BE55DC20400"
            && VerifyInstructionEvidence());

        PayloadGrowthPublicationPlan skippedZeroCall = Project(
            frameDwordMinus14ReceiverPointer: 0,
            receiverDword4: 0,
            callReturnedNormally: null,
            receiverDword8OldCounter: 0);
        check("A zero receiver pointer is not guarded; zero receiver DWORD +4 skips the call but still reads, updates, and publishes",
            skippedZeroCall.Outcome == PayloadGrowthPublicationOutcome.ReturnedNormally
            && skippedZeroCall.FrameDwordMinus14ReadAddress == 0x1FEC
            && skippedZeroCall.ReceiverPointer == 0
            && skippedZeroCall.ReceiverDword4ReadAddress == 4
            && skippedZeroCall.ReceiverDword4Value == 0
            && skippedZeroCall.OpaqueCallSkipped == true
            && !skippedZeroCall.OpaqueCallReached
            && skippedZeroCall.OpaqueCallReturnedNormally is null
            && skippedZeroCall.ReceiverDword8ReadAddress == 8
            && skippedZeroCall.ReceiverDword8Read
            && skippedZeroCall.ReceiverDword8OldCounter == 0
            && skippedZeroCall.ReceiverDword8WriteAddress == 8
            && skippedZeroCall.ReceiverDword8Written
            && skippedZeroCall.ReceiverDword8WriteValue == 0x20
            && skippedZeroCall.ReceiverDword4PointerWritten
            && skippedZeroCall.ReceiverDword4PointerWriteAddress == 4
            && skippedZeroCall.ReceiverDword4PointerWriteValue == 0x40
            && skippedZeroCall.Ret4Reached
            && skippedZeroCall.Ret4StackBytes == 4);

        PayloadGrowthPublicationPlan normalCall = Project(
            frameDwordMinus14ReceiverPointer: 0x1000,
            receiverDword4: 0x90,
            callReturnedNormally: true,
            receiverDword8OldCounter: 0x1234);
        check("A nonzero receiver DWORD +4 reaches the opaque call, then counter addition precedes pointer publication and RET 4",
            normalCall.Outcome == PayloadGrowthPublicationOutcome.ReturnedNormally
            && normalCall.ReceiverDword4ReadAddress == 0x1004
            && normalCall.OpaqueCallSkipped == false
            && normalCall.OpaqueCallReached
            && normalCall.OpaqueCallReturnedNormally == true
            && normalCall.OpaqueCallTargetAddress == 0x12C8D60
            && normalCall.OpaqueCallEcxValue == 0x90
            && normalCall.ReceiverDword8ReadAddress == 0x1008
            && normalCall.ReceiverDword8WriteAddress == 0x1008
            && normalCall.ReceiverDword8WriteValue == 0x1254
            && normalCall.ReceiverDword4PointerWriteAddress == 0x1004
            && normalCall.ReceiverDword4PointerWriteValue == 0x40
            && normalCall.Ret4Reached
            && normalCall.StopAtRawFileOffsetExclusive == 0x00BE6A10);

        PayloadGrowthPublicationPlan wrappedCounter = Project(
            receiverDword8OldCounter: uint.MaxValue,
            retainedEsiGrowthDelta: 1);
        PayloadGrowthPublicationPlan wrappedReceiverAddress = Project(
            frameDwordMinus14ReceiverPointer: uint.MaxValue - 7,
            receiverDword4: 0,
            callReturnedNormally: null,
            receiverDword8OldCounter: uint.MaxValue);
        PayloadGrowthPublicationPlan wrappedFrameAddress = Project(
            framePointer: 0x10,
            receiverDword4: 0,
            callReturnedNormally: null,
            receiverDword8OldCounter: 5);
        check("DWORD counter addition and receiver/frame address arithmetic wrap without guards",
            wrappedCounter.ReceiverDword8WriteValue == 0
            && wrappedReceiverAddress.FrameDwordMinus14ReadAddress == 0x1FEC
            && wrappedReceiverAddress.ReceiverDword4ReadAddress == uint.MaxValue - 3
            && wrappedReceiverAddress.ReceiverDword8ReadAddress == 0
            && wrappedReceiverAddress.ReceiverDword8WriteAddress == 0
            && wrappedReceiverAddress.ReceiverDword4PointerWriteAddress == uint.MaxValue - 3
            && wrappedFrameAddress.FrameDwordMinus14ReadAddress == 0xFFFFFFFC
            && wrappedFrameAddress.ReceiverDword8WriteValue == 0x25);

        PayloadGrowthPublicationPlan missingFrameMinus14 = Project(
            frameDwordMinus14ReceiverPointer: null,
            receiverDword4: null,
            callReturnedNormally: null,
            receiverDword8OldCounter: null);
        PayloadGrowthPublicationPlan laterAfterMissingFrameMinus14 = Project(
            frameDwordMinus14ReceiverPointer: null,
            receiverDword4: 0);
        PayloadGrowthPublicationPlan missingReceiver4 = Project(
            receiverDword4: null,
            callReturnedNormally: null,
            receiverDword8OldCounter: null);
        PayloadGrowthPublicationPlan laterAfterMissingReceiver4 = Project(
            receiverDword4: null,
            receiverDword8OldCounter: 0);
        check("Frame-pointer and receiver +4 captures gate all later observations in strict order",
            missingFrameMinus14.Outcome
                == PayloadGrowthPublicationOutcome.MissingFrameDwordMinus14Capture
            && missingFrameMinus14.StopAtRawFileOffsetExclusive == 0x00BE69F2
            && laterAfterMissingFrameMinus14.Outcome
                == PayloadGrowthPublicationOutcome.UnreachedCaptureProvided
            && missingReceiver4.Outcome
                == PayloadGrowthPublicationOutcome.MissingReceiverDword4Capture
            && missingReceiver4.FrameDwordMinus14Read
            && missingReceiver4.StopAtRawFileOffsetExclusive == 0x00BE69F5
            && laterAfterMissingReceiver4.Outcome
                == PayloadGrowthPublicationOutcome.UnreachedCaptureProvided);

        PayloadGrowthPublicationPlan skippedCallWithReturnCapture = Project(
            receiverDword4: 0,
            callReturnedNormally: true);
        PayloadGrowthPublicationPlan missingCallStatus = Project(
            receiverDword4: 1,
            callReturnedNormally: null,
            receiverDword8OldCounter: null);
        PayloadGrowthPublicationPlan laterAfterMissingCallStatus = Project(
            receiverDword4: 1,
            callReturnedNormally: null,
            receiverDword8OldCounter: 0);
        PayloadGrowthPublicationPlan nonreturningCall = Project(
            receiverDword4: 1,
            callReturnedNormally: false,
            receiverDword8OldCounter: null);
        PayloadGrowthPublicationPlan laterAfterNonreturningCall = Project(
            receiverDword4: 1,
            callReturnedNormally: false,
            receiverDword8OldCounter: 0);
        check("Skipped, missing-status, and nonreturning calls reject evidence from instructions they do not reach",
            skippedCallWithReturnCapture.Outcome
                == PayloadGrowthPublicationOutcome.UnreachedCaptureProvided
            && skippedCallWithReturnCapture.ReceiverDword8ReadAddress is null
            && missingCallStatus.Outcome
                == PayloadGrowthPublicationOutcome.OpaqueCallReturnStatusNotCaptured
            && missingCallStatus.OpaqueCallReached
            && missingCallStatus.OpaqueCallTargetAddress == 0x12C8D60
            && missingCallStatus.OpaqueCallEcxValue == 1
            && missingCallStatus.StopAtRawFileOffsetExclusive == 0x00BE69FC
            && laterAfterMissingCallStatus.Outcome
                == PayloadGrowthPublicationOutcome.UnreachedCaptureProvided
            && nonreturningCall.Outcome
                == PayloadGrowthPublicationOutcome.OpaqueCallDidNotReturnNormally
            && nonreturningCall.OpaqueCallReturnedNormally == false
            && nonreturningCall.ReceiverDword8ReadAddress is null
            && laterAfterNonreturningCall.Outcome
                == PayloadGrowthPublicationOutcome.UnreachedCaptureProvided);

        PayloadGrowthPublicationPlan missingCounter = Project(
            receiverDword4: 0,
            callReturnedNormally: null,
            receiverDword8OldCounter: null);
        check("The old receiver counter is mandatory before either publication write or RET 4",
            missingCounter.Outcome
                == PayloadGrowthPublicationOutcome.MissingReceiverDword8OldCounterCapture
            && missingCounter.ReceiverDword8ReadAddress == 0x1008
            && !missingCounter.ReceiverDword8Read
            && missingCounter.ReceiverDword8WriteAddress is null
            && !missingCounter.ReceiverDword8Written
            && missingCounter.ReceiverDword4PointerWriteAddress is null
            && !missingCounter.ReceiverDword4PointerWritten
            && missingCounter.ReceiverDword4PointerWriteValue is null
            && !missingCounter.Ret4Reached
            && missingCounter.Ret4StackBytes is null
            && missingCounter.StopAtRawFileOffsetExclusive == 0x00BE6A01);
    }

    private static PayloadGrowthPublicationPlan Project(
        uint framePointer = 0x2000,
        uint retainedEsiGrowthDelta = 0x20,
        uint retainedEbxPublishedPointer = 0x40,
        uint? frameDwordMinus14ReceiverPointer = 0x1000,
        uint? receiverDword4 = 1,
        bool? callReturnedNormally = true,
        uint? receiverDword8OldCounter = 0x100) =>
        PayloadGrowthPublicationProjection.Project(new(
            framePointer,
            retainedEsiGrowthDelta,
            retainedEbxPublishedPointer,
            frameDwordMinus14ReceiverPointer,
            receiverDword4,
            callReturnedNormally,
            receiverDword8OldCounter));

    private static bool VerifyInstructionEvidence()
    {
        uint nextRawOffset = PayloadGrowthPublicationProjection.StartRawFileOffset;
        string instructionBytes = string.Empty;
        foreach (PayloadGrowthPublicationInstructionEvidence instruction
            in PayloadGrowthPublicationProjection.Instructions)
        {
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != PayloadGrowthPublicationProjection.RawFileOffsetToVirtualAddress(
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

        PayloadGrowthPublicationInstructionEvidence jump =
            PayloadGrowthPublicationProjection.Instructions[3];
        PayloadGrowthPublicationInstructionEvidence call =
            PayloadGrowthPublicationProjection.Instructions[4];
        return nextRawOffset
                == PayloadGrowthPublicationProjection.NormalBoundaryRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(PayloadGrowthPublicationProjection.CapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            && jump.BranchTargetRawFileOffset == 0x00BE6A01
            && jump.BranchTargetVirtualAddress == 0x00FE7601
            && call.CallTargetAddress == 0x12C8D60;
    }
}
