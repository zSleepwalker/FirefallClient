using Firefall.Reconstruction.Startup;

internal static class GameSocketParserFirstByteFieldsChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] bytes = ParseBytes(
            GameSocketParserFirstByteFieldsProjection.CapturedBytes);
        check("The first-byte scalar window and bounded second-byte stop are pinned",
            GameSocketParserFirstByteFieldsProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && GameSocketParserFirstByteFieldsProjection.SampleFileSize == 40197848
            && GameSocketParserFirstByteFieldsProjection.RawToCodeVirtualAddressDelta
                == 0x00400C00
            && GameSocketParserFirstByteFieldsProjection.RawFileOffsetToCodeVirtualAddress(
                GameSocketParserFirstByteFieldsProjection.HeaderByteLoadRawFileOffset)
                == 0x00FD3DF8
            && GameSocketParserFirstByteFieldsProjection.CodeVirtualAddressToRawFileOffset(
                0x00FD3DF8)
                == GameSocketParserFirstByteFieldsProjection.HeaderByteLoadRawFileOffset
            && bytes.Length == 37
            && string.Join(" ", bytes.Select(value => value.ToString("X2")))
                == "8A 02 8A C8 0F B6 D8 C0 E9 04 80 E1 03 C1 EB 06 "
                    + "88 4C 24 12 8A C8 C0 E9 03 24 07 80 E1 01 "
                    + "88 4C 24 13 0F B6 C8"
            && string.Join(" ",
                ParseBytes(GameSocketParserFirstByteFieldsProjection.SecondHeaderByteLoadBytes)
                    .Select(value => value.ToString("X2"))) == "0F B6 42 01"
            && GameSocketParserFirstByteFieldsProjection.HeaderByteLoadRawFileOffset
                == GameSocketParserPostResizePointerProjection.HeaderByteLoadRawFileOffset
            && GameSocketParserFirstByteFieldsProjection.SecondHeaderByteLoadRawFileOffset
                == 0x00BD321D
            && GameSocketParserFirstByteFieldsProjection.FirstStackStoreRawFileOffset
                == 0x00BD3208
            && GameSocketParserFirstByteFieldsProjection.SecondStackStoreRawFileOffset
                == 0x00BD3216);

        GameSocketParserFirstByteFieldsPlan exhaustive = Project(0, 0);
        bool allBytesExact = true;
        for (int input = byte.MinValue; input <= byte.MaxValue; input++)
        {
            byte value = (byte)input;
            GameSocketParserFirstByteFieldsPlan plan =
                input == 0
                    ? exhaustive
                    : Project(value, field38: 0x5000, fieldPlus4: 0x12340000);
            byte bits7To6 = (byte)(value >> 6);
            byte bits5To4 = (byte)((value >> 4) & 0x03);
            byte bit3 = (byte)((value >> 3) & 1);
            byte bits2To0 = (byte)(value & 7);
            byte shrCL4Result = (byte)(value >> 4);
            byte shrCL3Result = (byte)(value >> 3);

            bool byteExact = plan.ParentHandoffAdmitted
                && plan.HeaderByteCaptureState
                    == GameSocketParserFirstByteFieldsCaptureState.Captured
                && plan.ParentResultEDX == (input == 0 ? 0u : 0x12340000u)
                && plan.ResultAL == bits2To0
                && plan.ResultEBX == bits7To6
                && plan.ResultECX == bits2To0
                && plan.ResultCL == bits2To0
                && plan.ResultEAX is null
                && plan.StoredBits5To4 == bits5To4
                && plan.StoredBit3 == bit3
                && plan.FlagsAfterShrCL4 == Flags(
                    (value & 0x08) != 0,
                    EvenParity(shrCL4Result),
                    shrCL4Result == 0,
                    sign: false,
                    overflow: null)
                && plan.FlagsAfterAndCL3 == Flags(
                    false,
                    EvenParity(bits5To4),
                    bits5To4 == 0,
                    false,
                    false)
                && plan.FlagsAfterShrEBX6 == Flags(
                    (value & 0x20) != 0,
                    EvenParity(bits7To6),
                    bits7To6 == 0,
                    sign: false,
                    overflow: null)
                && plan.FlagsAfterShrCL3 == Flags(
                    (value & 0x04) != 0,
                    EvenParity(shrCL3Result),
                    shrCL3Result == 0,
                    sign: false,
                    overflow: null)
                && plan.FlagsAfterAndAL7 == Flags(
                    false,
                    EvenParity(bits2To0),
                    bits2To0 == 0,
                    false,
                    false)
                && plan.FlagsAtStop == Flags(
                    false,
                    bit3 == 0,
                    bit3 == 0,
                    false,
                    false)
                && plan.Reads.Length == 1
                && plan.Reads[0].Address == plan.ParentResultEDX
                && plan.Reads[0].Value == value
                && plan.Reads[0].Attempted
                && plan.Reads[0].Captured
                && !plan.Reads[0].Committed
                && plan.Stores.Length == 2
                && plan.Stores[0].Kind
                    == GameSocketParserFirstByteFieldsStoreKind.Bits5To4
                && plan.Stores[0].Value == bits5To4
                && plan.Stores[0].Committed
                && plan.Stores[1].Kind == GameSocketParserFirstByteFieldsStoreKind.Bit3
                && plan.Stores[1].Value == bit3
                && plan.Stores[1].Committed
                && plan.ReachedInstructionRawFileOffsets.SequenceEqual(new uint[] {
                    GameSocketParserFirstByteFieldsProjection.HeaderByteLoadRawFileOffset,
                    GameSocketParserFirstByteFieldsProjection.MoveCLFromALRawFileOffset,
                    GameSocketParserFirstByteFieldsProjection.MoveZXEBXFromALRawFileOffset,
                    GameSocketParserFirstByteFieldsProjection.ShrCL4RawFileOffset,
                    GameSocketParserFirstByteFieldsProjection.AndCL3RawFileOffset,
                    GameSocketParserFirstByteFieldsProjection.ShrEBX6RawFileOffset,
                    GameSocketParserFirstByteFieldsProjection.FirstStackStoreRawFileOffset,
                    GameSocketParserFirstByteFieldsProjection.ReloadCLFromALRawFileOffset,
                    GameSocketParserFirstByteFieldsProjection.ShrCL3RawFileOffset,
                    GameSocketParserFirstByteFieldsProjection.AndAL7RawFileOffset,
                    GameSocketParserFirstByteFieldsProjection.AndCL1RawFileOffset,
                    GameSocketParserFirstByteFieldsProjection.SecondStackStoreRawFileOffset,
                    GameSocketParserFirstByteFieldsProjection.MoveZXECXFromALRawFileOffset})
                && plan.Outcome
                    == GameSocketParserFirstByteFieldsOutcome.BoundedBeforeSecondHeaderByteLoad
                && plan.StopBeforeRawFileOffset
                    == GameSocketParserFirstByteFieldsProjection.SecondHeaderByteLoadRawFileOffset
                && plan.ReachedBoundedEndpoint
                && plan.ExactBoundedCompletion
                && plan.IsCleanCompletion
                && plan.SecondHeaderByteReadAddress
                    == unchecked(plan.ParentResultEDX!.Value + 1)
                && !plan.SecondHeaderByteReadAttempted
                && !plan.HeaderAddressValidityEstablished
                && !plan.FullEAXKnown
                && !plan.TargetExecutionModeled
                && !plan.ParserExecutionModeled
                && !plan.InputSemanticsModeled;
            allBytesExact &= byteExact;

            byte recomposed = (byte)(
                (bits7To6 << 6)
                | (bits5To4 << 4)
                | (bit3 << 3)
                | bits2To0);
            allBytesExact &= recomposed == value;
        }

        check("All 256 byte values preserve the exact masks, registers, flag history, and stop",
            allBytesExact);

        GameSocketParserFirstByteFieldsPlan zero = Project(0, 0);
        GameSocketParserFirstByteFieldsPlan allBits = Project(0xFF, 0x7000);
        GameSocketParserFirstByteFieldsPlan bit3Only = Project(0x08, 0x7100);
        GameSocketParserFirstByteFieldsPlan lowBits = Project(0x07, 0x7200);
        GameSocketParserFirstByteFieldsPlan topBit = Project(0x80, 0x7300);
        check("Header fixtures distinguish stored bit 3, final ECX, and final ZF",
            zero.HeaderByteReadAddress == 0
            && zero.Reads[0].Attempted
            && zero.Reads[0].Captured
            && zero.Reads[0].Value == 0
            && zero.ResultECX == 0
            && zero.ResultCL == 0
            && zero.FlagsAtStop?.Zero == true
            && allBits.ResultEBX == 3
            && allBits.StoredBits5To4 == 3
            && allBits.StoredBit3 == 1
            && allBits.ResultAL == 7
            && allBits.ResultECX == 7
            && allBits.ResultCL == 7
            && allBits.FlagsAtStop?.Zero == false
            && bit3Only.StoredBit3 == 1
            && bit3Only.ResultECX == 0
            && bit3Only.ResultCL == 0
            && bit3Only.FlagsAtStop?.Zero == false
            && lowBits.StoredBit3 == 0
            && lowBits.ResultECX == 7
            && lowBits.ResultCL == 7
            && lowBits.FlagsAtStop?.Zero == true
            && topBit.ResultEBX == 2
            && topBit.StoredBits5To4 == 0
            && topBit.StoredBit3 == 0
            && topBit.ResultECX == 0
            && topBit.ResultCL == 0
            && topBit.FlagsAtStop?.Zero == true);

        GameSocketParserFirstByteFieldsPlan missingByte =
            Project(byteValue: null, field38: 0x80000000, fieldPlus4: 0);
        check("A missing fresh byte records an attempted uncommitted read and preserves parent flags",
            missingByte.Outcome
                == GameSocketParserFirstByteFieldsOutcome.MissingFreshHeaderByte
            && missingByte.ParentHandoffAdmitted
            && missingByte.HeaderByteReadAddress == 0
            && missingByte.Reads is [{
                Attempted: true,
                Captured: false,
                Committed: false,
                Value: null
            }]
            && missingByte.Stores.IsEmpty
            && missingByte.ReachedInstructionRawFileOffsets
                .SequenceEqual([GameSocketParserFirstByteFieldsProjection.HeaderByteLoadRawFileOffset])
            && missingByte.FlagsAtStop == ParentFlags(missingByte.ParentPlan.FlagsAtStop)
            && missingByte.StopBeforeRawFileOffset
                == GameSocketParserFirstByteFieldsProjection.HeaderByteLoadRawFileOffset
            && missingByte.SecondHeaderByteReadAddress == 1
            && !missingByte.SecondHeaderByteReadAttempted);

        GameSocketParserFirstByteFieldsPlan wrapped =
            Project(0xA5, 0xFFFFFFF0, field38: 0x5000, fieldPlus4: 0xFFFFFFFE);
        check("Field stores wrap from the inherited post-resize ESP without changing it",
            wrapped.ParentHandoffAdmitted
            && wrapped.InheritedFreshPostResizeESP == 0xFFFFFFF0
            && wrapped.FirstStackStoreAddress == 2
            && wrapped.SecondStackStoreAddress == 3
            && wrapped.Stores is [
                { StackAddress: 2, Value: 2, Committed: true },
                { StackAddress: 3, Value: 0, Committed: true }]
            && wrapped.ParentPlan.Capture.FreshPostResizeESP == 0xFFFFFFF0);

        GameSocketParserFirstByteFieldsPlan aliasedRead =
            Project(0xA5, 0x3000, field38: 0x5000, fieldPlus4: 0x3012);
        int parentEffectCount = aliasedRead.ParentPlan.MemoryEffects.Length;
        check("The captured header byte remains independent when its address aliases a later store",
            aliasedRead.HeaderByteReadAddress == 0x3012
            && aliasedRead.Reads[0].Value == 0xA5
            && aliasedRead.Stores[0].StackAddress == aliasedRead.HeaderByteReadAddress
            && aliasedRead.Stores[0].Value == 2
            && aliasedRead.OrderedMemoryEffects.Length == parentEffectCount + 3
            && aliasedRead.OrderedMemoryEffects[parentEffectCount].Kind
                == GameSocketParserFirstByteFieldsMemoryEffectKind.FreshHeaderByteRead
            && aliasedRead.OrderedMemoryEffects[parentEffectCount + 1].Kind
                == GameSocketParserFirstByteFieldsMemoryEffectKind.StackStore
            && aliasedRead.OrderedMemoryEffects[parentEffectCount + 2].Kind
                == GameSocketParserFirstByteFieldsMemoryEffectKind.StackStore
            && !aliasedRead.OrderedMemoryEffects[parentEffectCount].Committed
            && aliasedRead.OrderedMemoryEffects[parentEffectCount + 1].Committed
            && aliasedRead.OrderedMemoryEffects[parentEffectCount + 2].Committed);

        GameSocketParserFirstByteFieldsPlan parentRejected =
            Project(0x5A, parentValidated: false);
        GameSocketParserFirstByteFieldsPlan dirtyParentEndpoint =
            Project(0x5A, field38: 0, fieldPlus4: 0);
        check("Unadmitted parents retain their exact stop and leave the supplied byte unused",
            !parentRejected.ParentHandoffAdmitted
            && parentRejected.Outcome
                == GameSocketParserFirstByteFieldsOutcome.ParentHandoffNotAdmitted
            && parentRejected.StopBeforeRawFileOffset
                == parentRejected.ParentPlan.StopBeforeRawFileOffset
            && !parentRejected.ParentPlan.ReachedBoundedEndpoint
            && !parentRejected.ExactBoundedCompletion
            && !parentRejected.IsCleanCompletion
            && parentRejected.HeaderByteCaptureState
                == GameSocketParserFirstByteFieldsCaptureState.SuppliedButUnused
            && parentRejected.UnusedCaptures is [
                GameSocketParserFirstByteFieldsCaptureKind.FreshHeaderByte]
            && parentRejected.Reads.IsEmpty
            && dirtyParentEndpoint.ParentPlan.ReachedBoundedEndpoint
            && dirtyParentEndpoint.ParentPlan.ExactBoundedCompletion
            && !dirtyParentEndpoint.ParentPlan.IsCleanCompletion
            && !dirtyParentEndpoint.ParentHandoffAdmitted
            && dirtyParentEndpoint.ExactBoundedCompletion
            && !dirtyParentEndpoint.IsCleanCompletion
            && dirtyParentEndpoint.StopBeforeRawFileOffset
                == dirtyParentEndpoint.ParentPlan.StopBeforeRawFileOffset
            && dirtyParentEndpoint.HeaderByteCaptureState
                == GameSocketParserFirstByteFieldsCaptureState.SuppliedButUnused
            && dirtyParentEndpoint.UnusedCaptures.Length == 1
            && dirtyParentEndpoint.Reads.IsEmpty);
    }

    private static GameSocketParserFirstByteFieldsPlan Project(
        byte? byteValue,
        uint postResizeEsp = 0x3000,
        uint? field38 = 0,
        uint? fieldPlus4 = null,
        bool? parentValidated = true) =>
        GameSocketParserFirstByteFieldsProjection.Project(
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
                byteValue));

    private static GameSocketParserFirstByteFieldsFlags? ParentFlags(
        GameSocketParserPostResizePointerFlags? flags) =>
        flags is GameSocketParserPostResizePointerFlags value
            ? new GameSocketParserFirstByteFieldsFlags(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static bool EvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;

    private static GameSocketParserFirstByteFieldsFlags Flags(
        bool? carry,
        bool? parity,
        bool? zero,
        bool? sign,
        bool? overflow) =>
        new(carry, parity, null, zero, sign, overflow);

    private static byte[] ParseBytes(string text) =>
        text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
