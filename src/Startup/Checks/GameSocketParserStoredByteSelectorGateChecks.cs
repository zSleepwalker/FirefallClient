using Firefall.Reconstruction.Startup;

internal static class GameSocketParserStoredByteSelectorGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] bytes = ParseBytes(
            GameSocketParserStoredByteSelectorGateProjection.CapturedBytes);
        check("The selector branch bytes, destinations, and address mapping are pinned",
            GameSocketParserStoredByteSelectorGateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && GameSocketParserStoredByteSelectorGateProjection.SampleFileSize == 40197848
            && bytes.Length == 12
            && string.Join(" ", bytes.Select(value => value.ToString("X2")))
                == "74 34 83 FB 02 74 2F 83 FB 01 74 2A"
            && GameSocketParserStoredByteSelectorGateProjection.FirstJZRawFileOffset
                == 0x00BD325E
            && GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
                GameSocketParserStoredByteSelectorGateProjection.FirstJZRawFileOffset)
                == 0x00FD3E5E
            && GameSocketParserStoredByteSelectorGateProjection.CompareEBX2RawFileOffset
                == 0x00BD3260
            && GameSocketParserStoredByteSelectorGateProjection.Selector2JERawFileOffset
                == 0x00BD3263
            && GameSocketParserStoredByteSelectorGateProjection.CompareEBX1RawFileOffset
                == 0x00BD3265
            && GameSocketParserStoredByteSelectorGateProjection.Selector1JERawFileOffset
                == 0x00BD3268
            && GameSocketParserStoredByteSelectorGateProjection.FirstPushRawFileOffset
                == 0x00BD326A
            && GameSocketParserStoredByteSelectorGateProjection.SelectedDestinationRawFileOffset
                == 0x00BD3294
            && GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
                GameSocketParserStoredByteSelectorGateProjection.SelectedDestinationRawFileOffset)
                == 0x00FD3E94
            && GameSocketParserStoredByteSelectorGateProjection.FirstJZRawFileOffset + 2
                == GameSocketParserStoredByteSelectorGateProjection.CompareEBX2RawFileOffset
            && GameSocketParserStoredByteSelectorGateProjection.CompareEBX2RawFileOffset + 3
                == GameSocketParserStoredByteSelectorGateProjection.Selector2JERawFileOffset
            && GameSocketParserStoredByteSelectorGateProjection.Selector2JERawFileOffset + 2
                == GameSocketParserStoredByteSelectorGateProjection.CompareEBX1RawFileOffset
            && GameSocketParserStoredByteSelectorGateProjection.CompareEBX1RawFileOffset + 3
                == GameSocketParserStoredByteSelectorGateProjection.Selector1JERawFileOffset
            && GameSocketParserStoredByteSelectorGateProjection.Selector1JERawFileOffset + 2
                == GameSocketParserStoredByteSelectorGateProjection.FirstPushRawFileOffset
            && GameSocketParserStoredByteSelectorGateProjection.FirstJZRawFileOffset
                + 2 + bytes[1]
                == GameSocketParserStoredByteSelectorGateProjection.SelectedDestinationRawFileOffset
            && GameSocketParserStoredByteSelectorGateProjection.Selector2JERawFileOffset
                + 2 + bytes[6]
                == GameSocketParserStoredByteSelectorGateProjection.SelectedDestinationRawFileOffset
            && GameSocketParserStoredByteSelectorGateProjection.Selector1JERawFileOffset
                + 2 + bytes[11]
                == GameSocketParserStoredByteSelectorGateProjection.SelectedDestinationRawFileOffset
            && bytes.Take(2).SequenceEqual(new byte[] { 0x74, 0x34 })
            && bytes.Skip(2).Take(3).SequenceEqual(new byte[] { 0x83, 0xFB, 0x02 })
            && bytes.Skip(5).Take(2).SequenceEqual(new byte[] { 0x74, 0x2F })
            && bytes.Skip(7).Take(3).SequenceEqual(new byte[] { 0x83, 0xFB, 0x01 })
            && bytes.Skip(10).SequenceEqual(new byte[] { 0x74, 0x2A }));

        bool allSelectorsAndBytesExact = true;
        for (int selector = 0; selector < 4; selector++)
        {
            byte firstHeaderByte = (byte)(selector << 6);
            for (int freshByte = 0; freshByte < 256; freshByte++)
            {
                GameSocketParserStoredByteSelectorGatePlan plan =
                    Project(firstHeaderByte, (byte)freshByte);
                bool firstBranchTaken = freshByte == 0;
                bool selector2BranchTaken = !firstBranchTaken && selector == 2;
                bool selector1BranchTaken = !firstBranchTaken
                    && selector != 2
                    && selector == 1;
                uint destination = firstBranchTaken
                    || selector2BranchTaken
                    || selector1BranchTaken
                        ? GameSocketParserStoredByteSelectorGateProjection
                            .SelectedDestinationRawFileOffset
                        : GameSocketParserStoredByteSelectorGateProjection.FirstPushRawFileOffset;
                int expectedInstructionCount = firstBranchTaken
                    ? 1
                    : selector2BranchTaken
                        ? 3
                        : 5;
                GameSocketParserStoredByteSelectorGateFlags expectedFlags =
                    !firstBranchTaken
                        ? ExpectedCompareFlags32(
                            (uint)selector,
                            selector == 2 ? 2u : 1u)
                        : ExpectedCompareFlags8((byte)freshByte);
                GameSocketParserStoredByteSelectorGateBranchState firstState =
                    firstBranchTaken
                        ? GameSocketParserStoredByteSelectorGateBranchState.Taken
                        : GameSocketParserStoredByteSelectorGateBranchState.NotTaken;
                GameSocketParserStoredByteSelectorGateBranchState selector2State =
                    firstBranchTaken
                        ? GameSocketParserStoredByteSelectorGateBranchState.NotReached
                        : selector2BranchTaken
                            ? GameSocketParserStoredByteSelectorGateBranchState.Taken
                            : GameSocketParserStoredByteSelectorGateBranchState.NotTaken;
                GameSocketParserStoredByteSelectorGateBranchState selector1State =
                    firstBranchTaken || selector2BranchTaken
                        ? GameSocketParserStoredByteSelectorGateBranchState.NotReached
                        : selector1BranchTaken
                            ? GameSocketParserStoredByteSelectorGateBranchState.Taken
                            : GameSocketParserStoredByteSelectorGateBranchState.NotTaken;

                bool instructionTraceExact =
                    plan.Instructions.Length == expectedInstructionCount
                    && plan.AttemptedInstructionRawFileOffsets
                        .SequenceEqual(plan.ReachedInstructionRawFileOffsets)
                    && plan.ReachedInstructionRawFileOffsets.SequenceEqual(
                        expectedInstructionCount switch
                        {
                            1 => new uint[] {
                                GameSocketParserStoredByteSelectorGateProjection.FirstJZRawFileOffset
                            },
                            3 => new uint[] {
                                GameSocketParserStoredByteSelectorGateProjection.FirstJZRawFileOffset,
                                GameSocketParserStoredByteSelectorGateProjection.CompareEBX2RawFileOffset,
                                GameSocketParserStoredByteSelectorGateProjection.Selector2JERawFileOffset
                            },
                            _ => new uint[] {
                                GameSocketParserStoredByteSelectorGateProjection.FirstJZRawFileOffset,
                                GameSocketParserStoredByteSelectorGateProjection.CompareEBX2RawFileOffset,
                                GameSocketParserStoredByteSelectorGateProjection.Selector2JERawFileOffset,
                                GameSocketParserStoredByteSelectorGateProjection.CompareEBX1RawFileOffset,
                                GameSocketParserStoredByteSelectorGateProjection.Selector1JERawFileOffset
                            }
                        })
                    && plan.Instructions.All(instruction =>
                        instruction.Attempted
                        && instruction.Reached
                        && instruction.CodeVirtualAddress
                            == GameSocketParserCountConsumptionStoresProjection
                                .RawFileOffsetToCodeVirtualAddress(instruction.RawFileOffset))
                    && plan.FirstJZState == firstState
                    && plan.Selector2JEState == selector2State
                    && plan.Selector1JEState == selector1State;

                GameSocketParserStoredByteSelectorGateFlags expectedIncomingFlags =
                    ExpectedCompareFlags8((byte)freshByte);
                bool traceFlagsExact =
                    plan.IncomingFlags == expectedIncomingFlags
                    && plan.Instructions[0].FlagsBefore == expectedIncomingFlags
                    && plan.Instructions[0].FlagsAfter == expectedIncomingFlags;
                if (!firstBranchTaken)
                {
                    GameSocketParserStoredByteSelectorGateFlags expectedCompare2Flags =
                        ExpectedCompareFlags32((uint)selector, 2);
                    traceFlagsExact &= plan.Instructions[1].FlagsAfter == expectedCompare2Flags
                        && plan.Instructions[2].FlagsBefore == expectedCompare2Flags
                        && plan.Instructions[2].FlagsAfter == expectedCompare2Flags;
                    if (!selector2BranchTaken)
                    {
                        GameSocketParserStoredByteSelectorGateFlags expectedCompare1Flags =
                            ExpectedCompareFlags32((uint)selector, 1);
                        traceFlagsExact &= plan.Instructions[3].FlagsAfter == expectedCompare1Flags
                            && plan.Instructions[4].FlagsBefore == expectedCompare1Flags
                            && plan.Instructions[4].FlagsAfter == expectedCompare1Flags;
                    }
                }

                bool parentAndMemoryPrefixExact =
                    plan.ParentHandoffAdmitted
                    && plan.ParentPlan.IsCleanCompletion
                    && plan.ParentPlan.ParentPlan.ParentPlan.ParentPlan.ResultEBX
                        == (uint)selector
                    && plan.InheritedEBX == (uint)selector
                    && plan.ResultEBX == plan.InheritedEBX
                    && plan.EBXUnchanged
                    && !plan.ParentInstructionsReplayed
                    && plan.ParentPlan.Capture.FreshStackByteAtESPPlus13 == (byte)freshByte
                    && plan.ParentPlan.ParentPlan.ParentPlan.ParentPlan.Capture.FreshHeaderByte
                        == firstHeaderByte
                    && (firstHeaderByte & 0x08) == 0
                    && plan.ParentPlan.ParentPlan.ParentPlan.ParentPlan.StoredBit3 == 0
                    && plan.ChildReadsEmpty
                    && plan.ChildStoresEmpty
                    && plan.Reads.IsEmpty
                    && plan.Stores.IsEmpty
                    && plan.OrderedMemoryPrefixPreserved
                    && plan.OrderedMemoryEffects.SequenceEqual(
                        plan.ParentPlan.OrderedMemoryEffects);

                bool endpointExact =
                    plan.Outcome
                        == GameSocketParserStoredByteSelectorGateOutcome
                            .BoundedBeforeSelectedDestination
                    && plan.StopBeforeRawFileOffset == destination
                    && plan.StopBeforeCodeVirtualAddress
                        == GameSocketParserCountConsumptionStoresProjection
                            .RawFileOffsetToCodeVirtualAddress(destination)
                    && plan.SelectedDestinationRawFileOffset == destination
                    && plan.FlagsAtStop == expectedFlags
                    && plan.ReachedBoundedEndpoint
                    && plan.ExactBoundedCompletion
                    && plan.IsCleanCompletion
                    && !plan.DestinationEntered
                    && !plan.BranchTargetEntered
                    && !plan.FallthroughDestinationEntered
                    && !plan.FirstPushReached
                    && !plan.OpaqueEffectsModeled
                    && !plan.ParserExecutionModeled
                    && !plan.InputSemanticsModeled;

                allSelectorsAndBytesExact &= instructionTraceExact
                    && traceFlagsExact
                    && parentAndMemoryPrefixExact
                    && endpointExact;
            }
        }

        check("All 1024 independent stack-byte and two-bit selector combinations preserve ordered gates",
            allSelectorsAndBytesExact);

        GameSocketParserStoredByteSelectorGatePlan missingParentByte =
            Project(firstHeaderByte: 0, freshByte: null);
        check("A missing parent CMP byte stops at 3256 without claiming the child endpoint",
            !missingParentByte.ParentHandoffAdmitted
            && missingParentByte.Outcome
                == GameSocketParserStoredByteSelectorGateOutcome.ParentHandoffNotAdmitted
            && missingParentByte.ParentPlan.StopBeforeRawFileOffset
                == GameSocketParserCountConsumptionStoresProjection
                    .CompareFreshStackByteRawFileOffset
            && missingParentByte.StopBeforeRawFileOffset
                == GameSocketParserCountConsumptionStoresProjection
                    .CompareFreshStackByteRawFileOffset
            && missingParentByte.Instructions.IsEmpty
            && missingParentByte.AttemptedInstructionRawFileOffsets.IsEmpty
            && missingParentByte.ReachedInstructionRawFileOffsets.IsEmpty
            && !missingParentByte.ReachedBoundedEndpoint
            && !missingParentByte.ExactBoundedCompletion
            && !missingParentByte.IsCleanCompletion
            && missingParentByte.OrderedMemoryPrefixPreserved);

        GameSocketParserStoredByteSelectorGatePlan rejectedParent =
            Project(firstHeaderByte: 0, freshByte: 1, callerCount: 1);
        check("A rejected caller-count parent preserves its actual dirty bound and adds no child trace",
            !rejectedParent.ParentHandoffAdmitted
            && rejectedParent.ParentPlan.StopBeforeRawFileOffset
                == GameSocketParserCallerCountGateProjection.BranchTargetRawFileOffset
            && rejectedParent.StopBeforeRawFileOffset
                == rejectedParent.ParentPlan.StopBeforeRawFileOffset
            && rejectedParent.ExactBoundedCompletion
                == rejectedParent.ParentPlan.ExactBoundedCompletion
            && rejectedParent.ReachedBoundedEndpoint
                == rejectedParent.ParentPlan.ReachedBoundedEndpoint
            && !rejectedParent.IsCleanCompletion
            && rejectedParent.Instructions.IsEmpty
            && rejectedParent.Reads.IsEmpty
            && rejectedParent.Stores.IsEmpty
            && rejectedParent.OrderedMemoryPrefixPreserved);
    }

    private static GameSocketParserStoredByteSelectorGatePlan Project(
        byte firstHeaderByte,
        byte? freshByte,
        uint callerCount = 100) =>
        GameSocketParserStoredByteSelectorGateProjection.Project(
            new GameSocketParserStoredByteSelectorGateCapture(
                new GameSocketParserCountConsumptionStoresCapture(
                    new GameSocketParserCallerCountGateCapture(
                        new GameSocketParserSecondByteScalarGateCapture(
                            new GameSocketParserFirstByteFieldsCapture(
                                new GameSocketParserPostResizePointerCapture(
                                    new GameSocketParserLocalSizeDispatchCapture(
                                        true,
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
                                    0x3000,
                                    0x5000,
                                    0x1234),
                                firstHeaderByte),
                            FreshSecondHeaderByte: 2),
                        0x7000,
                        callerCount),
                    FreshStackDwordAtESPPlus14: 0x1000,
                    FreshStackByteAtESPPlus13: freshByte)));

    private static GameSocketParserStoredByteSelectorGateFlags ExpectedCompareFlags32(
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

    private static GameSocketParserStoredByteSelectorGateFlags ExpectedCompareFlags8(
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
