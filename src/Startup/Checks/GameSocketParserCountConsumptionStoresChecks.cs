using Firefall.Reconstruction.Startup;

internal static class GameSocketParserCountConsumptionStoresChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] bytes = ParseBytes(
            GameSocketParserCountConsumptionStoresProjection.CapturedBytes);
        check("The count-consumption instruction window and mapping are pinned",
            GameSocketParserCountConsumptionStoresProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && GameSocketParserCountConsumptionStoresProjection.SampleFileSize == 40197848
            && GameSocketParserCountConsumptionStoresProjection.RawToCodeVirtualAddressDelta
                == 0x00400C00
            && GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
                GameSocketParserCountConsumptionStoresProjection.AddStackDwordAtESPPlus14RawFileOffset)
                == 0x00FD3E49
            && GameSocketParserCountConsumptionStoresProjection.CodeVirtualAddressToRawFileOffset(
                0x00FD3E49)
                == GameSocketParserCountConsumptionStoresProjection.AddStackDwordAtESPPlus14RawFileOffset
            && GameSocketParserCountConsumptionStoresProjection.AddEDX2RawFileOffset
                == 0x00BD324D
            && GameSocketParserCountConsumptionStoresProjection.SubEAXEDI32RawFileOffset
                == 0x00BD3250
            && GameSocketParserCountConsumptionStoresProjection.StoreEDXAtESPPlus20RawFileOffset
                == 0x00BD3252
            && GameSocketParserCountConsumptionStoresProjection.CompareFreshStackByteRawFileOffset
                == 0x00BD3256
            && GameSocketParserCountConsumptionStoresProjection.StoreEAXAtEBPPlusCRawFileOffset
                == 0x00BD325B
            && GameSocketParserCountConsumptionStoresProjection.JZRawFileOffset
                == 0x00BD325E
            && bytes.Length == 21
            && string.Join(" ", bytes.Select(value => value.ToString("X2")))
                == "01 7C 24 14 83 C2 02 2B C7 89 54 24 20 80 7C 24 13 00 89 45 0C"
            && bytes.Take(4).SequenceEqual(new byte[] { 0x01, 0x7C, 0x24, 0x14 })
            && bytes.Skip(4).Take(3).SequenceEqual(new byte[] { 0x83, 0xC2, 0x02 })
            && bytes.Skip(7).Take(2).SequenceEqual(new byte[] { 0x2B, 0xC7 })
            && bytes.Skip(9).Take(4).SequenceEqual(new byte[] { 0x89, 0x54, 0x24, 0x20 })
            && bytes.Skip(13).Take(5).SequenceEqual(new byte[] { 0x80, 0x7C, 0x24, 0x13, 0x00 })
            && bytes.Skip(18).SequenceEqual(new byte[] { 0x89, 0x45, 0x0C }));

        GameSocketParserCountConsumptionStoresPlan wrapping =
            Project(
                callerEBP: 0,
                callerCount: uint.MaxValue,
                freshStackDwordAtESPPlus14: 0x12345678,
                freshStackByteAtESPPlus13: 0xA5,
                postResizeEsp: 0xFFFFFFEC,
                fieldPlus4: 0xFFFFFFFE);
        check("The admitted fallthrough inherits registers and commits three ordered wrapping stores",
            wrapping.ParentHandoffAdmitted
            && wrapping.ParentPlan.BranchTaken == false
            && wrapping.StopBeforeRawFileOffset
                == GameSocketParserCountConsumptionStoresProjection.JZRawFileOffset
            && wrapping.ResultEDI == 2
            && wrapping.ResultEAX == 0xFFFFFFFD
            && wrapping.InheritedEDX == 0xFFFFFFFE
            && wrapping.ResultEDX == 0
            && wrapping.InheritedESP == 0xFFFFFFEC
            && wrapping.ResultESP == 0xFFFFFFEC
            && wrapping.ResultEBP == 0
            && wrapping.FreshStackDwordReadAddress == 0
            && wrapping.FreshStackByteReadAddress == 0xFFFFFFFF
            && wrapping.Reads is [
                {
                    Kind: GameSocketParserCountConsumptionStoresReadKind.FreshStackDwordAtESPPlus14,
                    InstructionRawFileOffset:
                        GameSocketParserCountConsumptionStoresProjection.AddStackDwordAtESPPlus14RawFileOffset,
                    Address: 0,
                    Value: 0x12345678,
                    Attempted: true,
                    Captured: true,
                    Committed: false
                },
                {
                    Kind: GameSocketParserCountConsumptionStoresReadKind.FreshStackByteAtESPPlus13,
                    InstructionRawFileOffset:
                        GameSocketParserCountConsumptionStoresProjection.CompareFreshStackByteRawFileOffset,
                    Address: 0xFFFFFFFF,
                    Value: 0xA5,
                    Attempted: true,
                    Captured: true,
                    Committed: false
                }
            ]
            && wrapping.Stores is [
                {
                    Kind: GameSocketParserCountConsumptionStoresStoreKind.StackDwordReadModifyWriteAtESPPlus14,
                    InstructionRawFileOffset:
                        GameSocketParserCountConsumptionStoresProjection.AddStackDwordAtESPPlus14RawFileOffset,
                    Address: 0,
                    Value: 0x1234567A,
                    Attempted: true,
                    Committed: true
                },
                {
                    Kind: GameSocketParserCountConsumptionStoresStoreKind.StackDwordAtESPPlus20,
                    InstructionRawFileOffset:
                        GameSocketParserCountConsumptionStoresProjection.StoreEDXAtESPPlus20RawFileOffset,
                    Address: 0x0C,
                    Value: 0,
                    Attempted: true,
                    Committed: true
                },
                {
                    Kind: GameSocketParserCountConsumptionStoresStoreKind.CallerEBPPlusC,
                    InstructionRawFileOffset:
                        GameSocketParserCountConsumptionStoresProjection.StoreEAXAtEBPPlusCRawFileOffset,
                    Address: 0x0C,
                    Value: 0xFFFFFFFD,
                    Attempted: true,
                    Committed: true
                }
            ]
            && wrapping.ExactlyThreeChildWritesCommitted
            && wrapping.Stores[1].Address == wrapping.Stores[2].Address
            && wrapping.Stores[1].Kind != wrapping.Stores[2].Kind
            && wrapping.StackDwordCaptureState
                == GameSocketParserCountConsumptionStoresCaptureState.Captured
            && wrapping.StackByteCaptureState
                == GameSocketParserCountConsumptionStoresCaptureState.Captured
            && wrapping.UnusedCaptures.IsEmpty
            && wrapping.FlagsAfterAddStackDword
                == ExpectedAddFlags32(0x12345678, 2)
            && wrapping.FlagsAfterAddEDX == ExpectedAddFlags32(0xFFFFFFFE, 2)
            && wrapping.FlagsAfterSubEAX
                == ExpectedSubFlags32(uint.MaxValue, 2)
            && wrapping.FlagsAfterCompareStackByte == ExpectedCompareFlags8(0xA5)
            && wrapping.FlagsAtStop == wrapping.FlagsAfterCompareStackByte
            && wrapping.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[] {
                GameSocketParserCountConsumptionStoresProjection.AddStackDwordAtESPPlus14RawFileOffset,
                GameSocketParserCountConsumptionStoresProjection.AddEDX2RawFileOffset,
                GameSocketParserCountConsumptionStoresProjection.SubEAXEDI32RawFileOffset,
                GameSocketParserCountConsumptionStoresProjection.StoreEDXAtESPPlus20RawFileOffset,
                GameSocketParserCountConsumptionStoresProjection.CompareFreshStackByteRawFileOffset,
                GameSocketParserCountConsumptionStoresProjection.StoreEAXAtEBPPlusCRawFileOffset
            })
            && wrapping.ExactBoundedCompletion
            && wrapping.IsCleanCompletion
            && !wrapping.JumpZeroModeled
            && !wrapping.JZTargetEntered
            && !wrapping.ParserExecutionModeled
            && !wrapping.InputSemanticsModeled
            && ParentEffectPrefixPreserved(wrapping));

        bool byteComparisonFlagsExact = true;
        for (int value = 0; value < 256; value++)
        {
            GameSocketParserCountConsumptionStoresPlan plan =
                Project(
                    callerEBP: 0x7000,
                    callerCount: 100,
                    freshStackDwordAtESPPlus14: 0x2468,
                    freshStackByteAtESPPlus13: (byte)value);
            byteComparisonFlagsExact &= plan.FlagsAfterCompareStackByte
                    == ExpectedCompareFlags8((byte)value)
                && plan.FlagsAtStop == ExpectedCompareFlags8((byte)value)
                && plan.StopBeforeRawFileOffset
                    == GameSocketParserCountConsumptionStoresProjection.JZRawFileOffset
                && plan.BranchTakenIsUnmodeled
                && !plan.JumpZeroModeled
                && !plan.JZTargetEntered;
        }

        check("All 256 byte CMP results preserve exact 8-bit flags and stop before JZ",
            byteComparisonFlagsExact);

        GameSocketParserCountConsumptionStoresPlan missingFirstRead =
            Project(
                callerEBP: 0x7000,
                callerCount: 100,
                freshStackDwordAtESPPlus14: null,
                freshStackByteAtESPPlus13: 0);
        check("A missing fresh ESP+14 DWORD prevents its RMW and leaves the later byte unused",
            missingFirstRead.ParentHandoffAdmitted
            && missingFirstRead.Outcome
                == GameSocketParserCountConsumptionStoresOutcome.MissingFreshStackDwordAtESPPlus14
            && missingFirstRead.StackDwordCaptureState
                == GameSocketParserCountConsumptionStoresCaptureState.Missing
            && missingFirstRead.StackByteCaptureState
                == GameSocketParserCountConsumptionStoresCaptureState.SuppliedButUnused
            && missingFirstRead.UnusedCaptures is [
                GameSocketParserCountConsumptionStoresCaptureKind.FreshStackByteAtESPPlus13]
            && missingFirstRead.Reads is [{
                Address: 0x7014,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }]
            && missingFirstRead.Stores is [{
                Kind: GameSocketParserCountConsumptionStoresStoreKind.StackDwordReadModifyWriteAtESPPlus14,
                Attempted: true,
                Committed: false
            }]
            && missingFirstRead.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[] {
                GameSocketParserCountConsumptionStoresProjection.AddStackDwordAtESPPlus14RawFileOffset
            })
            && missingFirstRead.StopBeforeRawFileOffset
                == GameSocketParserCountConsumptionStoresProjection.AddStackDwordAtESPPlus14RawFileOffset
            && missingFirstRead.FlagsAtStop
                == ToFlags(missingFirstRead.ParentPlan.FlagsAtStop)
            && !missingFirstRead.ExactBoundedCompletion
            && !missingFirstRead.IsCleanCompletion
            && !missingFirstRead.ExactlyThreeChildWritesCommitted);

        GameSocketParserCountConsumptionStoresPlan missingByte =
            Project(
                callerEBP: 0x7000,
                callerCount: 100,
                freshStackDwordAtESPPlus14: 0x11223344,
                freshStackByteAtESPPlus13: null);
        check("A missing fresh ESP+13 byte stops at CMP after exactly the first two stores",
            missingByte.ParentHandoffAdmitted
            && missingByte.Outcome
                == GameSocketParserCountConsumptionStoresOutcome.MissingFreshStackByteAtESPPlus13
            && missingByte.StackDwordCaptureState
                == GameSocketParserCountConsumptionStoresCaptureState.Captured
            && missingByte.StackByteCaptureState
                == GameSocketParserCountConsumptionStoresCaptureState.Missing
            && missingByte.Reads.Length == 2
            && missingByte.Reads[1] is {
                Kind: GameSocketParserCountConsumptionStoresReadKind.FreshStackByteAtESPPlus13,
                Address: 0x7013,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }
            && missingByte.Stores.Length == 2
            && missingByte.Stores.All(store => store.Committed)
            && missingByte.Stores[0].Kind
                == GameSocketParserCountConsumptionStoresStoreKind.StackDwordReadModifyWriteAtESPPlus14
            && missingByte.Stores[1].Kind
                == GameSocketParserCountConsumptionStoresStoreKind.StackDwordAtESPPlus20
            && missingByte.Stores.All(store =>
                store.Kind != GameSocketParserCountConsumptionStoresStoreKind.CallerEBPPlusC)
            && missingByte.ResultEAX == 98
            && missingByte.ResultEDX == 0x1236
            && missingByte.FlagsAtStop == missingByte.FlagsAfterSubEAX
            && missingByte.FlagsAfterCompareStackByte == null
            && missingByte.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[] {
                GameSocketParserCountConsumptionStoresProjection.AddStackDwordAtESPPlus14RawFileOffset,
                GameSocketParserCountConsumptionStoresProjection.AddEDX2RawFileOffset,
                GameSocketParserCountConsumptionStoresProjection.SubEAXEDI32RawFileOffset,
                GameSocketParserCountConsumptionStoresProjection.StoreEDXAtESPPlus20RawFileOffset,
                GameSocketParserCountConsumptionStoresProjection.CompareFreshStackByteRawFileOffset
            })
            && missingByte.StopBeforeRawFileOffset
                == GameSocketParserCountConsumptionStoresProjection.CompareFreshStackByteRawFileOffset
            && !missingByte.ExactBoundedCompletion
            && !missingByte.ExactlyThreeChildWritesCommitted);

        GameSocketParserCountConsumptionStoresPlan rejectedParent =
            Project(
                callerEBP: 0x7000,
                callerCount: 1,
                freshStackDwordAtESPPlus14: 0xAABBCCDD,
                freshStackByteAtESPPlus13: 0x80);
        check("The caller-count JA to 3488 rejects the child without fabricating reads or stores",
            !rejectedParent.ParentHandoffAdmitted
            && rejectedParent.ParentPlan.BranchTaken == true
            && rejectedParent.ParentPlan.StopBeforeRawFileOffset
                == GameSocketParserCallerCountGateProjection.BranchTargetRawFileOffset
            && rejectedParent.StopBeforeRawFileOffset
                == rejectedParent.ParentPlan.StopBeforeRawFileOffset
            && rejectedParent.FlagsAtStop == ToFlags(rejectedParent.ParentPlan.FlagsAtStop)
            && rejectedParent.StackDwordCaptureState
                == GameSocketParserCountConsumptionStoresCaptureState.SuppliedButUnused
            && rejectedParent.StackByteCaptureState
                == GameSocketParserCountConsumptionStoresCaptureState.SuppliedButUnused
            && rejectedParent.UnusedCaptures.SequenceEqual(new[] {
                GameSocketParserCountConsumptionStoresCaptureKind.FreshStackDwordAtESPPlus14,
                GameSocketParserCountConsumptionStoresCaptureKind.FreshStackByteAtESPPlus13
            })
            && rejectedParent.Reads.IsEmpty
            && rejectedParent.Stores.IsEmpty
            && rejectedParent.ReachedInstructionRawFileOffsets.IsEmpty
            && rejectedParent.OrderedMemoryEffects.Length
                == rejectedParent.ParentPlan.OrderedMemoryEffects.Length
            && rejectedParent.ExactBoundedCompletion
            && !rejectedParent.IsCleanCompletion
            && rejectedParent.ChildWritesEmpty);
    }

    private static GameSocketParserCountConsumptionStoresPlan Project(
        uint? callerEBP,
        uint? callerCount,
        uint? freshStackDwordAtESPPlus14,
        byte? freshStackByteAtESPPlus13,
        byte firstByte = 0,
        byte secondByte = 2,
        uint postResizeEsp = 0x7000,
        uint fieldPlus4 = 0x1234,
        uint? field38 = 0x5000,
        bool? parentValidated = true) =>
        GameSocketParserCountConsumptionStoresProjection.Project(
            new GameSocketParserCountConsumptionStoresCapture(
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
                    callerCount),
                freshStackDwordAtESPPlus14,
                freshStackByteAtESPPlus13));

    private static bool ParentEffectPrefixPreserved(
        GameSocketParserCountConsumptionStoresPlan plan)
    {
        if (plan.OrderedMemoryEffects.Length
            < plan.ParentPlan.OrderedMemoryEffects.Length)
        {
            return false;
        }

        for (int index = 0; index < plan.ParentPlan.OrderedMemoryEffects.Length; index++)
        {
            GameSocketParserCallerCountGateMemoryEffect parent =
                plan.ParentPlan.OrderedMemoryEffects[index];
            GameSocketParserCountConsumptionStoresMemoryEffect child =
                plan.OrderedMemoryEffects[index];
            if (child.InstructionRawFileOffset != parent.InstructionRawFileOffset
                || child.Address != parent.Address
                || child.Value != parent.Value
                || child.Attempted != parent.Attempted
                || child.Captured != parent.Captured
                || child.Committed != parent.Committed)
            {
                return false;
            }
        }

        return true;
    }

    private static GameSocketParserCountConsumptionStoresFlags? ToFlags(
        GameSocketParserCallerCountGateFlags? flags) =>
        flags is GameSocketParserCallerCountGateFlags value
            ? new(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static GameSocketParserCountConsumptionStoresFlags ExpectedAddFlags32(
        uint left,
        uint right)
    {
        ulong wideResult = (ulong)left + right;
        uint result = unchecked(left + right);
        return new(
            Carry: wideResult > uint.MaxValue,
            Parity: EvenParity((byte)result),
            AuxiliaryCarry: ((left ^ right ^ result) & 0x10) != 0,
            Zero: result == 0,
            Sign: (result & 0x80000000) != 0,
            Overflow: (~(left ^ right) & (left ^ result) & 0x80000000) != 0);
    }

    private static GameSocketParserCountConsumptionStoresFlags ExpectedSubFlags32(
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

    private static GameSocketParserCountConsumptionStoresFlags ExpectedCompareFlags8(
        byte value) =>
        new(
            Carry: false,
            Parity: EvenParity(value),
            AuxiliaryCarry: false,
            Zero: value == 0,
            Sign: (value & 0x80) != 0,
            Overflow: false);

    private static bool EvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;

    private static byte[] ParseBytes(string text) =>
        text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
