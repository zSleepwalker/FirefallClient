using Firefall.Reconstruction.Startup;

internal static class ReceiveGuardForwardingChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE identity, raw range, VA mapping, and captured guard-forwarding bytes agree",
            ReceiveGuardForwardingProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ReceiveGuardForwardingProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ReceiveGuardForwardingProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && ReceiveGuardForwardingProjection.GateStartRawFileOffset == 0x00BE2F50
            && ReceiveGuardForwardingProjection.GateStartVirtualAddress == 0x00FE3B50
            && ReceiveGuardForwardingProjection.RawFileOffsetToVirtualAddress(0x00BE2F50)
                == 0x00FE3B50
            && ReceiveGuardForwardingProjection.GateEndRawFileOffsetExclusive == 0x00BE2F73
            && ReceiveGuardForwardingProjection.RawFileOffsetToVirtualAddress(0x00BE2F73)
                == 0x00FE3B73
            && ReceiveGuardForwardingProjection.CapturedGateBytes
                == "55 8B EC 8B 4D 08 8B 55 0C 85 C9 74 05 3B 51 04 "
                    + "7C 0F 8D 45 14 50 FF 75 10 E8 12 00 00 00 83 C4 08 5D C3"
            && ReceiveGuardForwardingProjection.CapturedGateBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x23
            && InstructionEvidenceMatches());

        check("The short branches and rel32 call resolve to their observed raw and virtual destinations",
            ReceiveGuardForwardingProjection.ShortBranchTarget(
                ReceiveGuardForwardingProjection.NullBranchRawFileOffset, 0x05)
                == ReceiveGuardForwardingProjection.NullBranchDestinationRawFileOffset
            && ReceiveGuardForwardingProjection.RawFileOffsetToVirtualAddress(
                ReceiveGuardForwardingProjection.NullBranchDestinationRawFileOffset)
                == 0x00FE3B62
            && ReceiveGuardForwardingProjection.ShortBranchTarget(
                ReceiveGuardForwardingProjection.SignedLessBranchRawFileOffset, 0x0F)
                == ReceiveGuardForwardingProjection.EarlyReturnRawFileOffset
            && ReceiveGuardForwardingProjection.RawFileOffsetToVirtualAddress(
                ReceiveGuardForwardingProjection.EarlyReturnRawFileOffset)
                == 0x00FE3B71
            && ReceiveGuardForwardingProjection.RelativeCallTarget(
                ReceiveGuardForwardingProjection.OpaqueCallRawFileOffset, 0x12)
                == ReceiveGuardForwardingProjection.OpaqueCallTargetRawFileOffset
            && ReceiveGuardForwardingProjection.RawFileOffsetToVirtualAddress(
                ReceiveGuardForwardingProjection.OpaqueCallTargetRawFileOffset)
                == 0x00FE3B80);

        ReceiveGuardForwardingPlan nullArgument = ReceiveGuardForwardingProjection.Project(
            new(0xFFFFFFF0, 0, 0xFFFFFFFF, null, 0, true));
        check("A null argument 0 skips the threshold read even with high-bit argument 1 and forwards DWORD zero",
            nullArgument.Outcome == ReceiveGuardForwardingOutcome.NormalEpilogue
            && !nullArgument.ThresholdRead
            && nullArgument.ThresholdAddress is null
            && nullArgument.SignedLessComparison is null
            && nullArgument.HandoffSelected
            && nullArgument.TailPointerValue == 4
            && nullArgument.Argument2Pushed
            && nullArgument.ForwardedArgument2 == 0
            && nullArgument.StackArgumentBytesPushed == 8
            && nullArgument.StackArgumentBytesCleaned == 8
            && nullArgument.EbpPopped == true
            && nullArgument.FunctionReturned == true);

        ReceiveGuardForwardingPlan missingThreshold = ReceiveGuardForwardingProjection.Project(
            new(0x1000, 0xFFFFFFFC, 0x80000000, null, null, null));
        ReceiveGuardForwardingPlan laterCaptureWithMissingThreshold =
            ReceiveGuardForwardingProjection.Project(
                new(0x1000, 0x1000, 0, null, 0, true));
        ReceiveGuardForwardingPlan unreachedNullThreshold = ReceiveGuardForwardingProjection.Project(
            new(0x1000, 0, 0x80000000, 0, null, null));
        check("Threshold captures are mandatory only for nonnull argument 0 and are not read on the null path",
            missingThreshold.Outcome == ReceiveGuardForwardingOutcome.MissingThresholdCapture
            && !missingThreshold.ThresholdRead
            && missingThreshold.ThresholdAddress == 0
            && missingThreshold.StopAtRawFileOffsetExclusive
                == ReceiveGuardForwardingProjection.ThresholdCompareRawFileOffset
            && laterCaptureWithMissingThreshold.Outcome
                == ReceiveGuardForwardingOutcome.UnreachedHandoffCapture
            && !laterCaptureWithMissingThreshold.ThresholdRead
            && laterCaptureWithMissingThreshold.StopAtRawFileOffsetExclusive
                == ReceiveGuardForwardingProjection.ThresholdCompareRawFileOffset
            && unreachedNullThreshold.Outcome == ReceiveGuardForwardingOutcome.UnreachedThresholdCapture
            && !unreachedNullThreshold.ThresholdRead
            && unreachedNullThreshold.ThresholdAddress is null
            && unreachedNullThreshold.StopAtRawFileOffsetExclusive
                == ReceiveGuardForwardingProjection.ThresholdCompareRawFileOffset);

        ReceiveGuardForwardingPlan signedLessHighBit = ReceiveGuardForwardingProjection.Project(
            new(0, 0x1000, 0x80000000, 0, null, null));
        ReceiveGuardForwardingPlan signedLessNegative = ReceiveGuardForwardingProjection.Project(
            new(0, 0x1000, 0xFFFFFFFF, 0, null, null));
        ReceiveGuardForwardingPlan signedNotLessAcrossBoundary = ReceiveGuardForwardingProjection.Project(
            new(0, 0x1000, 0x7FFFFFFF, 0x80000000, 0, true));
        ReceiveGuardForwardingPlan signedNotLessEqual = ReceiveGuardForwardingProjection.Project(
            new(0, 0x1000, 0xFFFFFFFF, 0xFFFFFFFF, 0, true));
        check("JL uses signed DWORD ordering at high-bit, negative, boundary, and equal values",
            signedLessHighBit.Outcome == ReceiveGuardForwardingOutcome.EarlySignedLessReturn
            && signedLessHighBit.SignedLessComparison == true
            && signedLessNegative.Outcome == ReceiveGuardForwardingOutcome.EarlySignedLessReturn
            && signedLessNegative.SignedLessComparison == true
            && signedNotLessAcrossBoundary.Outcome == ReceiveGuardForwardingOutcome.NormalEpilogue
            && signedNotLessAcrossBoundary.SignedLessComparison == false
            && signedNotLessEqual.Outcome == ReceiveGuardForwardingOutcome.NormalEpilogue
            && signedNotLessEqual.SignedLessComparison == false
            && signedLessHighBit.EbpPopped == true
            && signedLessHighBit.FunctionReturned == true
            && !signedLessHighBit.HandoffSelected
            && signedLessHighBit.StackArgumentBytesPushed == 0
            && signedLessHighBit.StackArgumentBytesCleaned == 0
            && signedLessHighBit.StopAtRawFileOffsetExclusive
                == ReceiveGuardForwardingProjection.GateEndRawFileOffsetExclusive);

        ReceiveGuardForwardingPlan unreachedAfterEarlyReturn = ReceiveGuardForwardingProjection.Project(
            new(0, 0x1000, 0xFFFFFFFF, 0, 0xABCD, true));
        check("Early signed-less return rejects supplied but unreachable forwarding captures without claiming an EAX result",
            unreachedAfterEarlyReturn.Outcome == ReceiveGuardForwardingOutcome.UnreachedHandoffCapture
            && unreachedAfterEarlyReturn.SignedLessComparison == true
            && !unreachedAfterEarlyReturn.HandoffSelected
            && !unreachedAfterEarlyReturn.Argument2Pushed
            && unreachedAfterEarlyReturn.StackArgumentBytesPushed == 0
            && unreachedAfterEarlyReturn.EbpPopped is null
            && unreachedAfterEarlyReturn.FunctionReturned is null);

        ReceiveGuardForwardingPlan missingArgument2 = ReceiveGuardForwardingProjection.Project(
            new(0xFFFFFFF0, 0, 0, null, null, null));
        ReceiveGuardForwardingPlan laterCaptureWithoutArgument2 =
            ReceiveGuardForwardingProjection.Project(
                new(0xFFFFFFF0, 0, 0, null, null, true));
        ReceiveGuardForwardingPlan missingCallReturn = ReceiveGuardForwardingProjection.Project(
            new(0xFFFFFFF0, 0, 0, null, 0xCAFEBABE, null));
        ReceiveGuardForwardingPlan nonReturningCall = ReceiveGuardForwardingProjection.Project(
            new(0xFFFFFFF0, 0, 0, null, 0xCAFEBABE, false));
        check("Handoff captures stop strictly at the missing argument, opaque call return, or unreturned call",
            missingArgument2.Outcome == ReceiveGuardForwardingOutcome.MissingArgument2Capture
            && missingArgument2.TailPointerPushed
            && missingArgument2.TailPointerValue == 4
            && missingArgument2.StackArgumentBytesPushed == 4
            && missingArgument2.StackArgumentBytesCleaned == 0
            && missingArgument2.StopAtRawFileOffsetExclusive
                == ReceiveGuardForwardingProjection.Argument2PushRawFileOffset
            && laterCaptureWithoutArgument2.Outcome
                == ReceiveGuardForwardingOutcome.UnreachedHandoffCapture
            && laterCaptureWithoutArgument2.StackArgumentBytesPushed == 4
            && laterCaptureWithoutArgument2.StopAtRawFileOffsetExclusive
                == ReceiveGuardForwardingProjection.Argument2PushRawFileOffset
            && missingCallReturn.Outcome == ReceiveGuardForwardingOutcome.MissingOpaqueCallReturnCapture
            && missingCallReturn.Argument2Pushed
            && missingCallReturn.ForwardedArgument2 == 0xCAFEBABE
            && missingCallReturn.StackArgumentBytesPushed == 8
            && missingCallReturn.StackArgumentBytesCleaned == 0
            && missingCallReturn.StopAtRawFileOffsetExclusive
                == ReceiveGuardForwardingProjection.OpaqueCallReturnRawFileOffset
            && nonReturningCall.Outcome == ReceiveGuardForwardingOutcome.OpaqueCallDidNotReturn
            && nonReturningCall.StackArgumentBytesPushed == 8
            && nonReturningCall.StackArgumentBytesCleaned == 0
            && nonReturningCall.EbpPopped is null
            && nonReturningCall.FunctionReturned is null);

        check("A normal opaque-call return alone enables the 8-byte caller cleanup, EBP pop, and plain RET",
            nullArgument.StackArgumentBytesCleaned
                == ReceiveGuardForwardingProjection.StackArgumentBytes
            && ReceiveGuardForwardingProjection.StackArgumentBytes
                == ReceiveGuardForwardingProjection.Argument2StackOffset
                    - ReceiveGuardForwardingProjection.Argument0StackOffset
            && ReceiveGuardForwardingProjection.RetPopBytes == 0
            && ReceiveGuardForwardingProjection.EpilogueReturnRawFileOffset
                == ReceiveGuardForwardingProjection.GateEndRawFileOffsetExclusive - 1
            && !ReceiveGuardForwardingProjection.CapturedGateBytes.Contains("C2"));
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE2F50, 0x00FE3B50, "55 8B EC 8B 4D 08 8B 55 0C", null, null),
            (0x00BE2F59, 0x00FE3B59, "85 C9 74 05", 0x00BE2F62, 0x00FE3B62),
            (0x00BE2F5D, 0x00FE3B5D, "3B 51 04 7C 0F", 0x00BE2F71, 0x00FE3B71),
            (0x00BE2F62, 0x00FE3B62, "8D 45 14 50 FF 75 10", null, null),
            (0x00BE2F69, 0x00FE3B69, "E8 12 00 00 00 83 C4 08 5D C3",
                0x00BE2F80, 0x00FE3B80),
            (0x00BE2F71, 0x00FE3B71, "5D C3", null, null)
        };
        return ReceiveGuardForwardingProjection.Instructions
            .Select(instruction =>
                (instruction.RawFileOffset, instruction.VirtualAddress, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.BranchTargetVirtualAddress))
            .SequenceEqual(expected);
    }
}
