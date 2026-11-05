using Firefall.Reconstruction.Startup;

internal static class GameSocketParserCallerCountGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] bytes = ParseBytes(GameSocketParserCallerCountGateProjection.CapturedBytes);
        check("The caller-count gate bytes and code mapping are pinned",
            GameSocketParserCallerCountGateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && GameSocketParserCallerCountGateProjection.SampleFileSize == 40197848
            && GameSocketParserCallerCountGateProjection.RawToCodeVirtualAddressDelta
                == 0x00400C00
            && GameSocketParserCallerCountGateProjection.RawFileOffsetToCodeVirtualAddress(
                GameSocketParserCallerCountGateProjection.MoveZXEDIAXRawFileOffset)
                == 0x00FD3E3B
            && GameSocketParserCallerCountGateProjection.CodeVirtualAddressToRawFileOffset(
                0x00FD3E3B)
                == GameSocketParserCallerCountGateProjection.MoveZXEDIAXRawFileOffset
            && bytes.Length == 14
            && string.Join(" ", bytes.Select(value => value.ToString("X2")))
                == "0F B7 F8 8B 45 0C 3B F8 0F 87 3F 02 00 00"
            && BitConverter.ToUInt32(bytes, 10) == 0x23F
            && unchecked(
                GameSocketParserCallerCountGateProjection.UnsignedAboveBranchRawFileOffset
                + 6
                + BitConverter.ToUInt32(bytes, 10))
                == GameSocketParserCallerCountGateProjection.BranchTargetRawFileOffset
            && GameSocketParserCallerCountGateProjection.MoveZXEDIAXRawFileOffset
                == 0x00BD323B
            && GameSocketParserCallerCountGateProjection.FreshCallerCountLoadRawFileOffset
                == 0x00BD323E
            && GameSocketParserCallerCountGateProjection.CompareEDIEAX32RawFileOffset
                == 0x00BD3241
            && GameSocketParserCallerCountGateProjection.UnsignedAboveBranchRawFileOffset
                == 0x00BD3243
            && GameSocketParserCallerCountGateProjection.FallthroughDestinationRawFileOffset
                == 0x00BD3249
            && GameSocketParserCallerCountGateProjection.BranchTargetRawFileOffset
                == 0x00BD3488);

        bool scalarAndCountFixturesExact = true;
        foreach (uint scalar in new uint[] { 2, 1392 })
        {
            uint[] counts =
            [
                0,
                1,
                scalar - 1,
                scalar,
                scalar + 1,
                uint.MaxValue,
                0x80000000
            ];
            foreach (uint count in counts)
            {
                GameSocketParserCallerCountGatePlan plan =
                    Project((byte)(scalar >> 8), (byte)scalar, 0x6000, count);
                bool expectedAbove = scalar > count;
                GameSocketParserCallerCountGateFlags expectedFlags =
                    ExpectedCompareFlags(scalar, count);
                scalarAndCountFixturesExact &= plan.ParentHandoffAdmitted
                    && plan.ParentPlan.BranchTaken == false
                    && plan.ParentPlan.ExactBoundedCompletion
                    && plan.ParentPlan.StopBeforeRawFileOffset
                        == GameSocketParserCallerCountGateProjection.MoveZXEDIAXRawFileOffset
                    && plan.ParentPlan.CombinedScalar is ushort parentScalar
                    && plan.ParentPlan.ResultEAX == (uint)parentScalar
                    && plan.CallerEBPCaptureState
                        == GameSocketParserCallerCountGateCaptureState.Captured
                    && plan.CallerCountCaptureState
                        == GameSocketParserCallerCountGateCaptureState.Captured
                    && plan.ResultEDI == scalar
                    && plan.ResultEAX == count
                    && plan.ComparedEDI == scalar
                    && plan.ComparedEAX == count
                    && plan.BranchTaken == expectedAbove
                    && plan.FlagsAtStop == expectedFlags
                    && plan.FlagsAfterMoveZXEDIAX
                        == ToFlags(plan.ParentPlan.FlagsAtStop)
                    && plan.FlagsAfterMoveEAXFromCaller
                        == ToFlags(plan.ParentPlan.FlagsAtStop)
                    && plan.StopBeforeRawFileOffset
                        == (expectedAbove
                            ? GameSocketParserCallerCountGateProjection.BranchTargetRawFileOffset
                            : GameSocketParserCallerCountGateProjection.FallthroughDestinationRawFileOffset)
                    && plan.ChildGateReachedEndpoint
                    && plan.ExactBoundedCompletion
                    && plan.IsCleanCompletion
                    && plan.UnusedCaptures.IsEmpty
                    && plan.Reads is [{
                        Kind: GameSocketParserCallerCountGateReadKind.FreshCallerCount,
                        InstructionRawFileOffset:
                            GameSocketParserCallerCountGateProjection.FreshCallerCountLoadRawFileOffset,
                        Address: 0x600C,
                        Value: var capturedCount,
                        Attempted: true,
                        Captured: true,
                        Committed: false
                    }]
                    && capturedCount == count
                    && plan.CallerCountReadAddress == 0x600C
                    && plan.ChildStoresEmpty
                    && plan.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[] {
                        GameSocketParserCallerCountGateProjection.MoveZXEDIAXRawFileOffset,
                        GameSocketParserCallerCountGateProjection.FreshCallerCountLoadRawFileOffset,
                        GameSocketParserCallerCountGateProjection.CompareEDIEAX32RawFileOffset,
                        GameSocketParserCallerCountGateProjection.UnsignedAboveBranchRawFileOffset
                    })
                    && !plan.BranchTargetEntered
                    && !plan.FallthroughDestinationEntered
                    && !plan.TargetExecutionModeled
                    && !plan.ParserExecutionModeled
                    && !plan.InputSemanticsModeled;
            }
        }

        check("Scalar 2 and 1392 compare against captured DWORD counts with full 32-bit unsigned flags",
            scalarAndCountFixturesExact);

        GameSocketParserCallerCountGatePlan signedHighCount =
            Project(firstByte: 0, secondByte: 2, callerEBP: 0x6000, callerCount: 0x80000000);
        check("A high-bit count uses a full-width unsigned compare and exact arithmetic flags",
            signedHighCount.BranchTaken == false
            && signedHighCount.FlagsAtStop is {
                Carry: true,
                Parity: false,
                AuxiliaryCarry: false,
                Zero: false,
                Sign: true,
                Overflow: true
            });

        GameSocketParserCallerCountGatePlan wrappedEBP =
            Project(
                firstByte: 0,
                secondByte: 2,
                callerEBP: 0xFFFFFFF8,
                callerCount: 2,
                postResizeEsp: 0xFFFFFFF0,
                fieldPlus4: 0xFFFFFFFF);
        int parentEffectCount = wrappedEBP.ParentPlan.OrderedMemoryEffects.Length;
        check("The fresh caller count uses wrapping EBP+0xC addressing and preserves parent effects",
            wrappedEBP.CallerCountReadAddress == 4
            && wrappedEBP.InheritedEDX == 0xFFFFFFFF
            && wrappedEBP.InheritedESP == 0xFFFFFFF0
            && wrappedEBP.Reads is [{ Address: 4, Value: 2, Captured: true, Committed: false }]
            && wrappedEBP.OrderedMemoryEffects.Length == parentEffectCount + 1
            && wrappedEBP.OrderedMemoryEffects[parentEffectCount].Kind
                == GameSocketParserCallerCountGateMemoryEffectKind.FreshCallerCountDwordRead
            && wrappedEBP.OrderedMemoryEffects[parentEffectCount].Address == 4
            && wrappedEBP.OrderedMemoryEffects[parentEffectCount].Value == 2
            && !wrappedEBP.OrderedMemoryEffects[parentEffectCount].Committed
            && ParentEffectPrefixPreserved(wrappedEBP)
            && wrappedEBP.ChildStoresEmpty);

        GameSocketParserCallerCountGatePlan missingEBP =
            Project(firstByte: 0, secondByte: 2, callerEBP: null, callerCount: 2);
        check("A missing EBP stops at the count-load instruction with an unknown address and unused count",
            missingEBP.ParentHandoffAdmitted
            && missingEBP.ResultEDI == 2
            && missingEBP.ResultEAX == null
            && missingEBP.CallerEBPCaptureState
                == GameSocketParserCallerCountGateCaptureState.Missing
            && missingEBP.CallerCountCaptureState
                == GameSocketParserCallerCountGateCaptureState.SuppliedButUnused
            && missingEBP.UnusedCaptures is [
                GameSocketParserCallerCountGateCaptureKind.FreshCallerCount]
            && missingEBP.Reads is [{
                Attempted: true,
                Address: null,
                Value: null,
                Captured: false,
                Committed: false
            }]
            && missingEBP.FreshCallerCountReadAttempted
            && missingEBP.CallerCountReadAddress == null
            && missingEBP.StopBeforeRawFileOffset
                == GameSocketParserCallerCountGateProjection.FreshCallerCountLoadRawFileOffset
            && !missingEBP.ExactBoundedCompletion
            && !missingEBP.IsCleanCompletion
            && missingEBP.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[] {
                GameSocketParserCallerCountGateProjection.MoveZXEDIAXRawFileOffset,
                GameSocketParserCallerCountGateProjection.FreshCallerCountLoadRawFileOffset
            })
            && missingEBP.FlagsAtStop == ToFlags(missingEBP.ParentPlan.FlagsAtStop)
            && missingEBP.ChildStoresEmpty);

        GameSocketParserCallerCountGatePlan missingCount =
            Project(firstByte: 0, secondByte: 2, callerEBP: 0x6000, callerCount: null);
        check("A missing count capture stops before the load and preserves the parent's 16-bit flags",
            missingCount.ResultEDI == 2
            && missingCount.ResultEAX == null
            && missingCount.CallerEBPCaptureState
                == GameSocketParserCallerCountGateCaptureState.Captured
            && missingCount.CallerCountCaptureState
                == GameSocketParserCallerCountGateCaptureState.Missing
            && missingCount.Reads is [{
                Attempted: true,
                Address: 0x600C,
                Value: null,
                Captured: false,
                Committed: false
            }]
            && missingCount.StopBeforeRawFileOffset
                == GameSocketParserCallerCountGateProjection.FreshCallerCountLoadRawFileOffset
            && !missingCount.ExactBoundedCompletion
            && !missingCount.IsCleanCompletion
            && missingCount.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[] {
                GameSocketParserCallerCountGateProjection.MoveZXEDIAXRawFileOffset,
                GameSocketParserCallerCountGateProjection.FreshCallerCountLoadRawFileOffset
            })
            && missingCount.FlagsAfterMoveZXEDIAX
                == ToFlags(missingCount.ParentPlan.FlagsAtStop)
            && missingCount.FlagsAfterMoveEAXFromCaller == null
            && missingCount.FlagsAtStop == ToFlags(missingCount.ParentPlan.FlagsAtStop)
            && missingCount.ChildStoresEmpty);

        bool parentBranchRejectsExact = true;
        foreach (byte[] bytesForScalar in new[]
        {
            new byte[] { 0, 0 },
            new byte[] { 5, 113 }
        })
        {
            GameSocketParserCallerCountGatePlan rejected =
                Project(bytesForScalar[0], bytesForScalar[1], 0x6000, 1);
            parentBranchRejectsExact &= !rejected.ParentHandoffAdmitted
                && rejected.ResultEDI == null
                && rejected.ResultEAX == null
                && rejected.CallerEBPCaptureState
                    == GameSocketParserCallerCountGateCaptureState.SuppliedButUnused
                && rejected.CallerCountCaptureState
                    == GameSocketParserCallerCountGateCaptureState.SuppliedButUnused
                && rejected.UnusedCaptures.Length == 2
                && rejected.Reads.IsEmpty
                && rejected.StopBeforeRawFileOffset
                    == rejected.ParentPlan.StopBeforeRawFileOffset
                && rejected.FlagsAtStop == ToFlags(rejected.ParentPlan.FlagsAtStop)
                && rejected.ExactBoundedCompletion
                && !rejected.IsCleanCompletion
                && rejected.ChildStoresEmpty;
        }

        check("The parent JA at scalar 0 or 1393 rejects the child and leaves both captures unused",
            parentBranchRejectsExact);

        GameSocketParserCallerCountGatePlan dirtyParent =
            Project(
                firstByte: 0,
                secondByte: 2,
                callerEBP: 0x6000,
                callerCount: 1,
                field38: 0);
        check("A dirty parent remains an exact parent-bound stop, not a clean child completion",
            !dirtyParent.ParentHandoffAdmitted
            && dirtyParent.ParentPlan.ExactBoundedCompletion
            && !dirtyParent.ParentPlan.IsCleanCompletion
            && dirtyParent.ExactBoundedCompletion
            && !dirtyParent.IsCleanCompletion
            && dirtyParent.Reads.IsEmpty
            && dirtyParent.UnusedCaptures.Length == 2);
    }

    private static GameSocketParserCallerCountGatePlan Project(
        byte firstByte,
        byte secondByte,
        uint? callerEBP,
        uint? callerCount,
        uint postResizeEsp = 0x3000,
        uint fieldPlus4 = 0x6000,
        uint? field38 = 0x5000,
        bool? parentValidated = true) =>
        GameSocketParserCallerCountGateProjection.Project(
            new GameSocketParserCallerCountGateCapture(
                new GameSocketParserSecondByteScalarGateCapture(
                    new GameSocketParserFirstByteFieldsCapture(
                        new GameSocketParserPostResizePointerCapture(
                            new GameSocketParserLocalSizeDispatchCapture(
                                parentValidated,
                                0x1000,
                                0x5000,
                                0x6000,
                                true,
                                0x1234,
                                0x2000,
                                new GameSocketParserLocalSizeDispatchFlags(
                                    Carry: true,
                                    Parity: false,
                                    AuxiliaryCarry: true,
                                    Zero: false,
                                    Sign: true,
                                    Overflow: false)),
                            true,
                            postResizeEsp,
                            field38,
                            fieldPlus4),
                        firstByte),
                    secondByte),
                callerEBP,
                callerCount));

    private static bool ParentEffectPrefixPreserved(
        GameSocketParserCallerCountGatePlan plan)
    {
        if (plan.OrderedMemoryEffects.Length < plan.ParentPlan.OrderedMemoryEffects.Length)
        {
            return false;
        }

        for (int index = 0; index < plan.ParentPlan.OrderedMemoryEffects.Length; index++)
        {
            GameSocketParserSecondByteScalarGateMemoryEffect parent =
                plan.ParentPlan.OrderedMemoryEffects[index];
            GameSocketParserCallerCountGateMemoryEffect child =
                plan.OrderedMemoryEffects[index];
            if (parent.InstructionRawFileOffset != child.InstructionRawFileOffset
                || parent.Address != child.Address
                || parent.Value != child.Value
                || parent.Attempted != child.Attempted
                || parent.Captured != child.Captured
                || parent.Committed != child.Committed)
            {
                return false;
            }
        }

        return true;
    }

    private static GameSocketParserCallerCountGateFlags? ToFlags(
        GameSocketParserSecondByteScalarGateFlags? flags) =>
        flags is GameSocketParserSecondByteScalarGateFlags value
            ? new GameSocketParserCallerCountGateFlags(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static GameSocketParserCallerCountGateFlags ExpectedCompareFlags(
        uint left,
        uint right)
    {
        uint result = unchecked(left - right);
        return new(
            Carry: left < right,
            Parity: EvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static bool EvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;

    private static byte[] ParseBytes(string text) =>
        text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
