using Firefall.Reconstruction.Startup;

internal static class GameSocketParserSecondByteScalarGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] bytes = ParseBytes(
            GameSocketParserSecondByteScalarGateProjection.CapturedBytes);
        check("The second-byte scalar and gate bytes are pinned to the sample mapping",
            GameSocketParserSecondByteScalarGateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && GameSocketParserSecondByteScalarGateProjection.SampleFileSize == 40197848
            && GameSocketParserSecondByteScalarGateProjection.RawToCodeVirtualAddressDelta
                == 0x00400C00
            && GameSocketParserSecondByteScalarGateProjection.RawFileOffsetToCodeVirtualAddress(
                GameSocketParserSecondByteScalarGateProjection.SecondHeaderByteLoadRawFileOffset)
                == 0x00FD3E1D
            && GameSocketParserSecondByteScalarGateProjection.CodeVirtualAddressToRawFileOffset(
                0x00FD3E1D)
                == GameSocketParserSecondByteScalarGateProjection.SecondHeaderByteLoadRawFileOffset
            && bytes.Length == 30
            && string.Join(" ", bytes.Select(value => value.ToString("X2")))
                == "0F B6 42 01 66 C1 E1 08 66 0B C8 0F B7 C1 "
                    + "89 44 24 18 8D 48 FE 66 3B CF 0F 87 4D 02 00 00"
            && GameSocketParserSecondByteScalarGateProjection.BranchTargetRawFileOffset
                == 0x00BD3488
            && GameSocketParserSecondByteScalarGateProjection.FallthroughDestinationRawFileOffset
                == 0x00BD323B
            && GameSocketParserSecondByteScalarGateProjection.InheritedDIValue
                == 0x056E);

        bool exhaustiveValuesExact = true;
        for (int scalar = 0; scalar <= 0x7FF; scalar++)
        {
            byte firstByte = (byte)(scalar >> 8);
            byte secondByte = (byte)scalar;
            GameSocketParserSecondByteScalarGatePlan plan =
                Project(firstByte, secondByte);
            ushort lea16 = unchecked((ushort)(scalar - 2));
            bool above = lea16 > GameSocketParserSecondByteScalarGateProjection.InheritedDIValue;
            GameSocketParserSecondByteScalarGateFlags compareFlags =
                ExpectedCompareFlags(lea16, 0x056E);
            bool scalarExact = plan.ParentHandoffAdmitted
                && plan.ParentPlan.ParentPlan.ResultEDI == 0x056E
                && plan.InheritedDI == 0x056E
                && plan.CaptureState
                    == GameSocketParserSecondByteScalarGateCaptureState.Captured
                && plan.CombinedScalar == scalar
                && plan.CombinedScalar <= 2047
                && plan.ResultEAX == scalar
                && plan.ResultECX == unchecked((uint)scalar - 2)
                && plan.ComparedCX == lea16
                && plan.ComparedDI == 0x056E
                && plan.BranchTaken == above
                && plan.FlagsAfterShiftCX8 is {
                    Carry: false,
                    Parity: true,
                    AuxiliaryCarry: null,
                    Sign: false,
                    Overflow: null
                }
                && plan.FlagsAfterOrCXAX is {
                    Carry: false,
                    Parity: var parity,
                    AuxiliaryCarry: null,
                    Sign: false,
                    Overflow: false
                }
                && plan.FlagsAfterShiftCX8?.Zero == (firstByte == 0)
                && plan.FlagsAfterOrCXAX?.Zero == (scalar == 0)
                && parity == EvenParity(secondByte)
                && plan.FlagsAtStop == compareFlags
                && plan.Reads is [{
                    Kind: GameSocketParserSecondByteScalarGateReadKind.FreshSecondHeaderByte,
                    InstructionRawFileOffset:
                        GameSocketParserSecondByteScalarGateProjection.SecondHeaderByteLoadRawFileOffset,
                    Address: 0x6001,
                    Value: var capturedSecondByte,
                    Attempted: true,
                    Captured: true,
                    Committed: false
                }]
                && capturedSecondByte == secondByte
                && plan.Stores is [{
                    Kind: GameSocketParserSecondByteScalarGateStoreKind.CombinedScalar,
                    InstructionRawFileOffset:
                        GameSocketParserSecondByteScalarGateProjection.CombinedScalarStackStoreRawFileOffset,
                    StackAddress: 0x3018,
                    Value: var storedScalar,
                    Attempted: true,
                    Committed: true
                }]
                && storedScalar == scalar
                && plan.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[] {
                    GameSocketParserSecondByteScalarGateProjection.SecondHeaderByteLoadRawFileOffset,
                    GameSocketParserSecondByteScalarGateProjection.ShiftCX8RawFileOffset,
                    GameSocketParserSecondByteScalarGateProjection.OrCXAXRawFileOffset,
                    GameSocketParserSecondByteScalarGateProjection.MoveZXEAXFromCXRawFileOffset,
                    GameSocketParserSecondByteScalarGateProjection.CombinedScalarStackStoreRawFileOffset,
                    GameSocketParserSecondByteScalarGateProjection.LeaECXMinusTwoRawFileOffset,
                    GameSocketParserSecondByteScalarGateProjection.CompareCXDI16RawFileOffset,
                    GameSocketParserSecondByteScalarGateProjection.UnsignedAboveBranchRawFileOffset
                })
                && plan.Outcome
                    == GameSocketParserSecondByteScalarGateOutcome.BoundedBeforeEitherGateDestination
                && plan.StopBeforeRawFileOffset
                    == (above
                        ? GameSocketParserSecondByteScalarGateProjection.BranchTargetRawFileOffset
                        : GameSocketParserSecondByteScalarGateProjection.FallthroughDestinationRawFileOffset)
                && plan.ReachedBoundedEndpoint
                && plan.ExactBoundedCompletion
                && plan.IsCleanCompletion
                && plan.BranchTargetEntered == false
                && plan.FallthroughDestinationEntered == false
                && !plan.TargetExecutionModeled
                && !plan.ParserExecutionModeled
                && !plan.InputSemanticsModeled;

            int combinedFromOperations =
                (((firstByte & 0x07) << 8) | secondByte);
            exhaustiveValuesExact &= scalarExact
                && combinedFromOperations == scalar
                && plan.BranchTaken
                    == (scalar == 0 || scalar == 1 || scalar >= 1393);
        }

        check("All 2048 scalar values retain exact registers, flags, stack store, and unsigned gate",
            exhaustiveValuesExact);

        bool boundariesExact = true;
        foreach ((
            int scalar,
            bool expectedTaken,
            uint expectedEcx,
            ushort expectedCX,
            uint expectedStop) in new[]
        {
            (0, true, 0xFFFFFFFEu, (ushort)0xFFFE,
                GameSocketParserSecondByteScalarGateProjection.BranchTargetRawFileOffset),
            (1, true, 0xFFFFFFFFu, (ushort)0xFFFF,
                GameSocketParserSecondByteScalarGateProjection.BranchTargetRawFileOffset),
            (2, false, 0u, (ushort)0,
                GameSocketParserSecondByteScalarGateProjection.FallthroughDestinationRawFileOffset),
            (1392, false, 1390u, (ushort)1390,
                GameSocketParserSecondByteScalarGateProjection.FallthroughDestinationRawFileOffset),
            (1393, true, 1391u, (ushort)1391,
                GameSocketParserSecondByteScalarGateProjection.BranchTargetRawFileOffset),
            (2047, true, 2045u, (ushort)2045,
                GameSocketParserSecondByteScalarGateProjection.BranchTargetRawFileOffset)
        })
        {
            GameSocketParserSecondByteScalarGatePlan plan =
                Project((byte)(scalar >> 8), (byte)scalar);
            boundariesExact &= plan.CombinedScalar == scalar
                && plan.BranchTaken == expectedTaken
                && plan.ResultECX == expectedEcx
                && plan.ComparedCX == expectedCX
                && plan.ComparedDI == 0x056E
                && plan.StopBeforeRawFileOffset == expectedStop
                && plan.ReachedBoundedEndpoint;
        }

        check("Unsigned CMP boundaries distinguish wrapped 0/1 and the 1392/1393 transition",
            boundariesExact);

        GameSocketParserSecondByteScalarGatePlan missingSecondByte =
            Project(firstByte: 0xA5, secondByte: null);
        check("A missing fresh second byte preserves the inherited bit-3 flags and stops at its load",
            missingSecondByte.ParentHandoffAdmitted
            && missingSecondByte.CaptureState
                == GameSocketParserSecondByteScalarGateCaptureState.Missing
            && missingSecondByte.Reads is [{
                Attempted: true,
                Captured: false,
                Committed: false,
                Value: null
            }]
            && missingSecondByte.Stores.IsEmpty
            && missingSecondByte.ReachedInstructionRawFileOffsets
                .SequenceEqual([
                    GameSocketParserSecondByteScalarGateProjection.SecondHeaderByteLoadRawFileOffset])
            && missingSecondByte.FlagsAtStop
                == ToFlags(missingSecondByte.ParentPlan.FlagsAtStop)
            && missingSecondByte.StopBeforeRawFileOffset
                == GameSocketParserSecondByteScalarGateProjection.SecondHeaderByteLoadRawFileOffset
            && !missingSecondByte.ExactBoundedCompletion
            && missingSecondByte.SecondHeaderByteReadAddress == 0x6001);

        GameSocketParserSecondByteScalarGatePlan wrapped =
            Project(
                firstByte: 0xFF,
                secondByte: 0xFF,
                postResizeEsp: 0xFFFFFFF0,
                fieldPlus4: 0xFFFFFFFF);
        check("The independent byte read and DWORD stack store use wrapping 32-bit addresses",
            wrapped.ParentHandoffAdmitted
            && wrapped.ParentResultEDX == 0xFFFFFFFF
            && wrapped.SecondHeaderByteReadAddress == 0
            && wrapped.InheritedFreshPostResizeESP == 0xFFFFFFF0
            && wrapped.StackStoreAddress == 8
            && wrapped.Stores is [{ StackAddress: 8, Value: 2047, Committed: true }]
            && wrapped.ParentPlan.Stores[0].StackAddress == 2
            && wrapped.ParentPlan.Stores[1].StackAddress == 3);

        GameSocketParserSecondByteScalarGatePlan aliasedRead =
            Project(
                firstByte: 0xA5,
                secondByte: 0x5A,
                postResizeEsp: 0x3000,
                fieldPlus4: 0x3011);
        int parentEffectCount = aliasedRead.ParentPlan.OrderedMemoryEffects.Length;
        check("A second-byte read aliasing the first-byte store remains a fresh uncommitted capture",
            aliasedRead.SecondHeaderByteReadAddress == 0x3012
            && aliasedRead.ParentPlan.Stores[0].StackAddress
                == aliasedRead.SecondHeaderByteReadAddress
            && aliasedRead.ParentPlan.Stores[0].Value == 2
            && aliasedRead.Reads[0].Value == 0x5A
            && aliasedRead.Reads[0].Captured
            && !aliasedRead.Reads[0].Committed
            && aliasedRead.OrderedMemoryEffects.Length == parentEffectCount + 2
            && aliasedRead.OrderedMemoryEffects[parentEffectCount].Kind
                == GameSocketParserSecondByteScalarGateMemoryEffectKind.FreshSecondHeaderByteRead
            && aliasedRead.OrderedMemoryEffects[parentEffectCount].Address == 0x3012
            && aliasedRead.OrderedMemoryEffects[parentEffectCount].Value == 0x5A
            && !aliasedRead.OrderedMemoryEffects[parentEffectCount].Committed
            && aliasedRead.OrderedMemoryEffects[parentEffectCount + 1].Kind
                == GameSocketParserSecondByteScalarGateMemoryEffectKind.StackStore
            && aliasedRead.OrderedMemoryEffects[parentEffectCount + 1].Committed
            && ParentEffectPrefixPreserved(aliasedRead));

        GameSocketParserSecondByteScalarGatePlan dirtyParentEndpoint =
            Project(
                firstByte: 0x5A,
                secondByte: 0xC3,
                field38: 0);
        GameSocketParserSecondByteScalarGatePlan rejectedParent =
            Project(
                firstByte: 0x5A,
                secondByte: 0xC3,
                parentValidated: false);
        check("Parent admission preserves exact bounded stops and leaves supplied child bytes unused",
            !dirtyParentEndpoint.ParentPlan.ReachedBoundedEndpoint
            && dirtyParentEndpoint.ParentPlan.ExactBoundedCompletion
            && !dirtyParentEndpoint.ParentPlan.IsCleanCompletion
            && !dirtyParentEndpoint.ParentHandoffAdmitted
            && dirtyParentEndpoint.ExactBoundedCompletion
            && !dirtyParentEndpoint.IsCleanCompletion
            && dirtyParentEndpoint.CaptureState
                == GameSocketParserSecondByteScalarGateCaptureState.SuppliedButUnused
            && dirtyParentEndpoint.UnusedCaptures is [
                GameSocketParserSecondByteScalarGateCaptureKind.FreshSecondHeaderByte]
            && dirtyParentEndpoint.Reads.IsEmpty
            && dirtyParentEndpoint.StopBeforeRawFileOffset
                == dirtyParentEndpoint.ParentPlan.StopBeforeRawFileOffset
            && rejectedParent.ParentPlan.StopBeforeRawFileOffset
                == rejectedParent.StopBeforeRawFileOffset
            && !rejectedParent.ParentPlan.ReachedBoundedEndpoint
            && !rejectedParent.ExactBoundedCompletion
            && !rejectedParent.ParentHandoffAdmitted
            && rejectedParent.CaptureState
                == GameSocketParserSecondByteScalarGateCaptureState.SuppliedButUnused
            && rejectedParent.Reads.IsEmpty);
    }

    private static GameSocketParserSecondByteScalarGatePlan Project(
        byte? firstByte,
        byte? secondByte,
        uint postResizeEsp = 0x3000,
        uint fieldPlus4 = 0x6000,
        uint? field38 = 0x5000,
        bool? parentValidated = true) =>
        GameSocketParserSecondByteScalarGateProjection.Project(
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
                secondByte));

    private static bool ParentEffectPrefixPreserved(
        GameSocketParserSecondByteScalarGatePlan plan)
    {
        if (plan.OrderedMemoryEffects.Length < plan.ParentPlan.OrderedMemoryEffects.Length)
        {
            return false;
        }

        for (int index = 0; index < plan.ParentPlan.OrderedMemoryEffects.Length; index++)
        {
            GameSocketParserFirstByteFieldsMemoryEffect parent =
                plan.ParentPlan.OrderedMemoryEffects[index];
            GameSocketParserSecondByteScalarGateMemoryEffect child =
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

    private static GameSocketParserSecondByteScalarGateFlags? ToFlags(
        GameSocketParserFirstByteFieldsFlags? flags) =>
        flags is GameSocketParserFirstByteFieldsFlags value
            ? new GameSocketParserSecondByteScalarGateFlags(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static GameSocketParserSecondByteScalarGateFlags ExpectedCompareFlags(
        ushort left,
        ushort right)
    {
        ushort result = unchecked((ushort)(left - right));
        return new(
            Carry: left < right,
            Parity: EvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x8000) != 0,
            Overflow: ((left ^ right) & (left ^ result) & 0x8000) != 0);
    }

    private static bool EvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;

    private static byte[] ParseBytes(string text) =>
        text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
