using Firefall.Reconstruction.Startup;

internal static class GameSocketParserSelectorDiagnosticPreparationChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] bytes = ParseBytes(
            GameSocketParserSelectorDiagnosticPreparationProjection.CapturedBytes);
        check("The diagnostic push and call bytes are pinned to the parser mapping",
            GameSocketParserSelectorDiagnosticPreparationProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && GameSocketParserSelectorDiagnosticPreparationProjection.SampleFileSize == 40197848
            && bytes.Length == 28
            && string.Join(" ", bytes.Select(value => value.ToString("X2")))
                == "68 57 03 00 00 68 D8 6E BB 01 68 B4 6E BB 01 6A 01 "
                    + "FF B6 10 01 00 00 E8 CA FC 00 00"
            && GameSocketParserSelectorDiagnosticPreparationProjection.FirstPushRawFileOffset
                == 0x00BD326A
            && GameSocketParserSelectorDiagnosticPreparationProjection.SecondPushRawFileOffset
                == 0x00BD326F
            && GameSocketParserSelectorDiagnosticPreparationProjection.ThirdPushRawFileOffset
                == 0x00BD3274
            && GameSocketParserSelectorDiagnosticPreparationProjection.FourthPushRawFileOffset
                == 0x00BD3279
            && GameSocketParserSelectorDiagnosticPreparationProjection.ReceiverFieldPushRawFileOffset
                == 0x00BD327B
            && GameSocketParserSelectorDiagnosticPreparationProjection.CallRawFileOffset
                == 0x00BD3281
            && GameSocketParserSelectorDiagnosticPreparationProjection.CallTargetRawFileOffset
                == 0x00BE2F50
            && GameSocketParserSelectorDiagnosticPreparationProjection.CallTargetCodeVirtualAddress
                == 0x00FE3B50
            && GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
                GameSocketParserSelectorDiagnosticPreparationProjection.FirstPushRawFileOffset)
                == 0x00FD3E6A
            && GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
                GameSocketParserSelectorDiagnosticPreparationProjection.CallRawFileOffset)
                == 0x00FD3E81
            && GameSocketParserSelectorDiagnosticPreparationProjection.FirstPushRawFileOffset + 5
                == GameSocketParserSelectorDiagnosticPreparationProjection.SecondPushRawFileOffset
            && GameSocketParserSelectorDiagnosticPreparationProjection.SecondPushRawFileOffset + 5
                == GameSocketParserSelectorDiagnosticPreparationProjection.ThirdPushRawFileOffset
            && GameSocketParserSelectorDiagnosticPreparationProjection.ThirdPushRawFileOffset + 5
                == GameSocketParserSelectorDiagnosticPreparationProjection.FourthPushRawFileOffset
            && GameSocketParserSelectorDiagnosticPreparationProjection.FourthPushRawFileOffset + 2
                == GameSocketParserSelectorDiagnosticPreparationProjection.ReceiverFieldPushRawFileOffset
            && GameSocketParserSelectorDiagnosticPreparationProjection.ReceiverFieldPushRawFileOffset + 6
                == GameSocketParserSelectorDiagnosticPreparationProjection.CallRawFileOffset
            && GameSocketParserSelectorDiagnosticPreparationProjection.CallRawFileOffset + 5
                + 0x0000FCCA
                == GameSocketParserSelectorDiagnosticPreparationProjection.CallTargetRawFileOffset);

        bool allFreshByteAndAcceptedSelectorPairsExact = true;
        foreach (byte selector in new byte[] { 0, 3 })
        {
            for (int freshByte = 1; freshByte <= byte.MaxValue; freshByte++)
            {
                uint freshESI = 0x7000;
                uint receiverValue = unchecked(0xA0000000u + (uint)freshByte);
                GameSocketParserSelectorDiagnosticPreparationPlan plan =
                    Project(
                        selector,
                        (byte)freshByte,
                        freshESI,
                        receiverValue);
                GameSocketParserStoredByteSelectorGateFlags expectedFlags =
                    ExpectedCompareFlags32(selector, 1);
                uint stackPointer = 0x3000;
                uint[] expectedAddresses = new uint[5];
                for (int index = 0; index < expectedAddresses.Length; index++)
                {
                    stackPointer = unchecked(stackPointer - 4);
                    expectedAddresses[index] = stackPointer;
                }

                bool storesExact =
                    plan.Stores.Length == 5
                    && plan.Stores.Select(store => store.Address)
                        .SequenceEqual(expectedAddresses.Select(address => (uint?)address))
                    && plan.Stores.Select(store => store.Value)
                        .SequenceEqual(new uint?[] {
                            GameSocketParserSelectorDiagnosticPreparationProjection
                                .FirstOpaqueImmediate,
                            GameSocketParserSelectorDiagnosticPreparationProjection
                                .SecondOpaqueImmediate,
                            GameSocketParserSelectorDiagnosticPreparationProjection
                                .ThirdOpaqueImmediate,
                            GameSocketParserSelectorDiagnosticPreparationProjection
                                .FourthOpaqueImmediate,
                            receiverValue
                        })
                    && plan.Stores.All(store => store.Attempted && store.Committed);
                bool pushesExact =
                    plan.Pushes.Length == 5
                    && plan.Pushes.Select(push => push.Kind).SequenceEqual(
                        new[] {
                            GameSocketParserSelectorDiagnosticPreparationPushKind
                                .OpaqueImmediateAt326A,
                            GameSocketParserSelectorDiagnosticPreparationPushKind
                                .OpaqueImmediateAt326F,
                            GameSocketParserSelectorDiagnosticPreparationPushKind
                                .OpaqueImmediateAt3274,
                            GameSocketParserSelectorDiagnosticPreparationPushKind
                                .OpaqueImmediateAt3279,
                            GameSocketParserSelectorDiagnosticPreparationPushKind
                                .FreshReceiverFieldDWORDAt327B
                        })
                    && plan.Pushes.All(push =>
                        push.Attempted
                        && push.Reached
                        && push.Committed
                        && push.FlagsBefore == expectedFlags
                        && push.FlagsAfter == expectedFlags)
                    && plan.Pushes.Select(push => push.InstructionRawFileOffset)
                        .SequenceEqual(new uint[] {
                            GameSocketParserSelectorDiagnosticPreparationProjection
                                .FirstPushRawFileOffset,
                            GameSocketParserSelectorDiagnosticPreparationProjection
                                .SecondPushRawFileOffset,
                            GameSocketParserSelectorDiagnosticPreparationProjection
                                .ThirdPushRawFileOffset,
                            GameSocketParserSelectorDiagnosticPreparationProjection
                                .FourthPushRawFileOffset,
                            GameSocketParserSelectorDiagnosticPreparationProjection
                                .ReceiverFieldPushRawFileOffset
                        })
                    && plan.Pushes.Select(push => push.StackAddress)
                        .SequenceEqual(expectedAddresses.Select(address => (uint?)address))
                    && plan.Pushes.Select(push => push.Value)
                        .SequenceEqual(plan.Stores.Select(store => store.Value))
                    && plan.Pushes.Select(push => push.StackPointerAfter)
                        .SequenceEqual(expectedAddresses.Select(address => (uint?)address));
                int parentEffectCount = plan.ParentPlan.OrderedMemoryEffects.Length;
                bool orderedEffectsExact =
                    plan.OrderedMemoryPrefixPreserved
                    && plan.OrderedMemoryEffects.Length == parentEffectCount + 6
                    && plan.OrderedMemoryEffects[parentEffectCount]
                        .Kind == GameSocketParserSelectorDiagnosticPreparationMemoryEffectKind.StackPush
                    && plan.OrderedMemoryEffects[parentEffectCount + 3]
                        .InstructionRawFileOffset
                        == GameSocketParserSelectorDiagnosticPreparationProjection
                            .FourthPushRawFileOffset
                    && plan.OrderedMemoryEffects[parentEffectCount + 4]
                        .Kind
                        == GameSocketParserSelectorDiagnosticPreparationMemoryEffectKind
                            .FreshReceiverFieldDWORDRead
                    && plan.OrderedMemoryEffects[parentEffectCount + 4].Address
                        == unchecked(freshESI
                            + GameSocketParserSelectorDiagnosticPreparationProjection
                                .ReceiverFieldDisplacement)
                    && plan.OrderedMemoryEffects[parentEffectCount + 4].Value == receiverValue
                    && !plan.OrderedMemoryEffects[parentEffectCount + 4].Committed
                    && plan.OrderedMemoryEffects[parentEffectCount + 5]
                        .Kind == GameSocketParserSelectorDiagnosticPreparationMemoryEffectKind.StackPush
                    && plan.OrderedMemoryEffects[parentEffectCount + 5].Address
                        == expectedAddresses[4]
                    && plan.OrderedMemoryEffects[parentEffectCount + 5].Value == receiverValue;

                allFreshByteAndAcceptedSelectorPairsExact &=
                    plan.ParentHandoffAdmitted
                    && plan.AllSelectorBranchesNotTaken
                    && plan.ParentPlan.InheritedEBX == selector
                    && plan.ParentPlan.ParentPlan.Capture.FreshStackByteAtESPPlus13
                        == freshByte
                    && plan.FreshESICaptureState
                        == GameSocketParserSelectorDiagnosticPreparationCaptureState.Captured
                    && plan.ReceiverFieldCaptureState
                        == GameSocketParserSelectorDiagnosticPreparationCaptureState.Captured
                    && plan.ParentESP == 0x3000
                    && plan.ResultESP == 0x2FEC
                    && plan.FlagsAtStop == expectedFlags
                    && plan.Outcome
                        == GameSocketParserSelectorDiagnosticPreparationOutcome.BoundedBeforeCall
                    && plan.StopBeforeRawFileOffset
                        == GameSocketParserSelectorDiagnosticPreparationProjection.CallRawFileOffset
                    && plan.ReachedBoundedEndpoint
                    && plan.ExactBoundedCompletion
                    && plan.IsCleanCompletion
                    && plan.ExactlyFivePushesCommitted
                    && !plan.CallReached
                    && !plan.CallTargetEntered
                    && !plan.OpaqueCallEffectsModeled
                    && !plan.ESIIdentitySemanticsModeled
                    && plan.LogicalMemoryEffectsOnly
                    && !plan.NativeMemoryEffectsModeled
                    && !plan.ParserExecutionModeled
                    && !plan.InputSemanticsModeled
                    && storesExact
                    && pushesExact
                    && plan.Reads is [{
                        InstructionRawFileOffset:
                            GameSocketParserSelectorDiagnosticPreparationProjection
                                .ReceiverFieldPushRawFileOffset,
                        Address: var readAddress,
                        Value: var readValue,
                        Attempted: true,
                        Captured: true,
                        Committed: false
                    }]
                    && readAddress
                        == unchecked(freshESI
                            + GameSocketParserSelectorDiagnosticPreparationProjection
                                .ReceiverFieldDisplacement)
                    && readValue == receiverValue
                    && orderedEffectsExact
                    && plan.UnusedCaptures.IsEmpty;
            }
        }

        check("Selectors 0/3 with every nonzero byte preserve flags and commit only five pushes",
            allFreshByteAndAcceptedSelectorPairsExact);

        GameSocketParserSelectorDiagnosticPreparationPlan zeroReceiverValue =
            Project(selector: 0, freshByte: 1, freshESI: 0, receiverValue: 0);
        GameSocketParserSelectorDiagnosticPreparationPlan maximumReceiverValue =
            Project(
                selector: 3,
                freshByte: 0xFF,
                freshESI: 0xFFFFFFFF,
                receiverValue: uint.MaxValue);
        check("Fresh receiver ESI zero and DWORD values zero/maximum remain ordinary captures",
            zeroReceiverValue.ParentHandoffAdmitted
            && zeroReceiverValue.FreshESIAt327B == 0
            && zeroReceiverValue.ReceiverFieldReadAddress == 0x110
            && zeroReceiverValue.Reads is [{ Address: 0x110, Value: 0 }]
            && zeroReceiverValue.Stores[^1].Value == 0
            && zeroReceiverValue.IsCleanCompletion
            && maximumReceiverValue.ParentHandoffAdmitted
            && maximumReceiverValue.ReceiverFieldReadAddress == 0x10F
            && maximumReceiverValue.Reads is [{ Address: 0x10F, Value: uint.MaxValue }]
            && maximumReceiverValue.Stores[^1].Value == uint.MaxValue
            && maximumReceiverValue.IsCleanCompletion);

        GameSocketParserSelectorDiagnosticPreparationPlan wrappedAddresses =
            Project(
                selector: 0,
                freshByte: 1,
                freshESI: 0xFFFFFF00,
                receiverValue: 0xCAFEBABE,
                postResizeESP: 0);
        check("Stack and ESI-relative DWORD addresses wrap independently at 32 bits",
            wrappedAddresses.ParentESP == 0
            && wrappedAddresses.Reads is [{ Address: 0x10, Value: 0xCAFEBABE }]
            && wrappedAddresses.Stores.Select(store => store.Address)
                .SequenceEqual(new uint?[] {
                    0xFFFFFFFC,
                    0xFFFFFFF8,
                    0xFFFFFFF4,
                    0xFFFFFFF0,
                    0xFFFFFFEC
                })
            && wrappedAddresses.ResultESP == 0xFFFFFFEC
            && wrappedAddresses.IsCleanCompletion);

        const uint aliasingStackPointer = 0x4000;
        uint aliasingESI = unchecked(
            aliasingStackPointer - 4
            - GameSocketParserSelectorDiagnosticPreparationProjection.ReceiverFieldDisplacement);
        GameSocketParserSelectorDiagnosticPreparationPlan aliasedIndependentRead =
            Project(
                selector: 3,
                freshByte: 7,
                freshESI: aliasingESI,
                receiverValue: 0xDEADBEEF,
                postResizeESP: aliasingStackPointer);
        int aliasedPrefixCount =
            aliasedIndependentRead.ParentPlan.OrderedMemoryEffects.Length;
        GameSocketParserSelectorDiagnosticPreparationPlan reprojected =
            GameSocketParserSelectorDiagnosticPreparationProjection.Project(
                aliasedIndependentRead.Capture);
        check("Immutable captures reproduce the ordered preparation without array identity assumptions",
            reprojected.Outcome == aliasedIndependentRead.Outcome
            && reprojected.ResultESP == aliasedIndependentRead.ResultESP
            && reprojected.StopBeforeRawFileOffset == aliasedIndependentRead.StopBeforeRawFileOffset
            && reprojected.FlagsAtStop == aliasedIndependentRead.FlagsAtStop
            && reprojected.Pushes.SequenceEqual(aliasedIndependentRead.Pushes)
            && reprojected.Reads.SequenceEqual(aliasedIndependentRead.Reads)
            && reprojected.OrderedMemoryEffects.SequenceEqual(aliasedIndependentRead.OrderedMemoryEffects));
        check("The fresh receiver read remains independent when it aliases the first logical push",
            aliasedIndependentRead.Reads is [{ Address: 0x3FFC, Value: 0xDEADBEEF }]
            && aliasedIndependentRead.Stores[0] is {
                Address: 0x3FFC,
                Value: GameSocketParserSelectorDiagnosticPreparationProjection
                    .FirstOpaqueImmediate
            }
            && aliasedIndependentRead.OrderedMemoryEffects[aliasedPrefixCount].Address == 0x3FFC
            && aliasedIndependentRead.OrderedMemoryEffects[aliasedPrefixCount].Value
                == GameSocketParserSelectorDiagnosticPreparationProjection.FirstOpaqueImmediate
            && aliasedIndependentRead.OrderedMemoryEffects[aliasedPrefixCount + 4].Kind
                == GameSocketParserSelectorDiagnosticPreparationMemoryEffectKind
                    .FreshReceiverFieldDWORDRead
            && aliasedIndependentRead.OrderedMemoryEffects[aliasedPrefixCount + 4].Address
                == 0x3FFC
            && aliasedIndependentRead.OrderedMemoryEffects[aliasedPrefixCount + 4].Value
                == 0xDEADBEEF
            && aliasedIndependentRead.OrderedMemoryEffects[aliasedPrefixCount + 4].Committed
                == false
            && aliasedIndependentRead.OrderedMemoryEffects[aliasedPrefixCount + 5].Address
                == 0x3FEC
            && aliasedIndependentRead.IsCleanCompletion);

        GameSocketParserSelectorDiagnosticPreparationPlan missingESI =
            Project(
                selector: 0,
                freshByte: 1,
                freshESI: null,
                receiverValue: 0x12345678);
        check("Missing fresh ESI preserves the first four pushes without inventing a source read",
            missingESI.ParentHandoffAdmitted
            && missingESI.FreshESICaptureState
                == GameSocketParserSelectorDiagnosticPreparationCaptureState.Missing
            && missingESI.ReceiverFieldCaptureState
                == GameSocketParserSelectorDiagnosticPreparationCaptureState.SuppliedButUnused
            && missingESI.UnusedCaptures is [
                GameSocketParserSelectorDiagnosticPreparationCaptureKind.FreshReceiverFieldDWORD]
            && missingESI.Reads.IsEmpty
            && missingESI.Pushes.Length == 5
            && missingESI.Pushes.Take(4).All(push => push.Committed)
            && missingESI.Pushes[4] is {
                InstructionRawFileOffset:
                    GameSocketParserSelectorDiagnosticPreparationProjection
                        .ReceiverFieldPushRawFileOffset,
                Attempted: true,
                Reached: true,
                Committed: false,
                StackAddress: 0x2FEC,
                StackPointerAfter: null
            }
            && missingESI.Stores.Length == 4
            && missingESI.ResultESP == 0x2FF0
            && missingESI.Outcome
                == GameSocketParserSelectorDiagnosticPreparationOutcome.MissingFreshESIAt327B
            && missingESI.StopBeforeRawFileOffset
                == GameSocketParserSelectorDiagnosticPreparationProjection.ReceiverFieldPushRawFileOffset
            && !missingESI.ExactBoundedCompletion
            && !missingESI.IsCleanCompletion
            && missingESI.OrderedMemoryPrefixPreserved);

        GameSocketParserSelectorDiagnosticPreparationPlan missingReceiverField =
            Project(
                selector: 3,
                freshByte: 1,
                freshESI: 0xFFFFFF00,
                receiverValue: null);
        check("Missing fresh receiver DWORD records its known wrapped read and keeps four pushes",
            missingReceiverField.FreshESICaptureState
                == GameSocketParserSelectorDiagnosticPreparationCaptureState.Captured
            && missingReceiverField.ReceiverFieldCaptureState
                == GameSocketParserSelectorDiagnosticPreparationCaptureState.Missing
            && missingReceiverField.Reads is [{
                Address: 0x10,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }]
            && missingReceiverField.Pushes.Length == 5
            && missingReceiverField.Pushes.Take(4).All(push => push.Committed)
            && !missingReceiverField.Pushes[4].Committed
            && missingReceiverField.Stores.Length == 4
            && missingReceiverField.ResultESP == 0x2FF0
            && missingReceiverField.Outcome
                == GameSocketParserSelectorDiagnosticPreparationOutcome
                    .MissingFreshReceiverFieldDWORD
            && missingReceiverField.StopBeforeRawFileOffset
                == GameSocketParserSelectorDiagnosticPreparationProjection.ReceiverFieldPushRawFileOffset
            && !missingReceiverField.ExactBoundedCompletion
            && !missingReceiverField.IsCleanCompletion);

        bool rejectedSelectorAndZeroByteCasesExact = true;
        foreach (byte selector in new byte[] { 0, 1, 2, 3 })
        {
            foreach (byte freshByte in new byte[] { 0, 1 })
            {
                GameSocketParserSelectorDiagnosticPreparationPlan plan =
                    Project(selector, freshByte, 0x7000, 0xABCDEF01);
                bool eligible = (selector is 0 or 3) && freshByte != 0;
                if (eligible)
                {
                    rejectedSelectorAndZeroByteCasesExact &=
                        plan.ParentHandoffAdmitted
                        && plan.Outcome
                            == GameSocketParserSelectorDiagnosticPreparationOutcome
                                .BoundedBeforeCall;
                }
                else
                {
                    rejectedSelectorAndZeroByteCasesExact &=
                        !plan.ParentHandoffAdmitted
                        && plan.Pushes.IsEmpty
                        && plan.Reads.IsEmpty
                        && plan.Stores.IsEmpty
                        && plan.FreshESICaptureState
                            == GameSocketParserSelectorDiagnosticPreparationCaptureState
                                .SuppliedButUnused
                        && plan.ReceiverFieldCaptureState
                            == GameSocketParserSelectorDiagnosticPreparationCaptureState
                                .SuppliedButUnused
                        && plan.UnusedCaptures.Length == 2
                        && plan.StopBeforeRawFileOffset
                            == plan.ParentPlan.StopBeforeRawFileOffset
                        && plan.ExactBoundedCompletion
                            == plan.ParentPlan.ExactBoundedCompletion
                        && plan.ReachedBoundedEndpoint
                            == plan.ParentPlan.ReachedBoundedEndpoint
                        && !plan.IsCleanCompletion
                        && plan.OrderedMemoryPrefixPreserved;
                    uint expectedParentStop = freshByte == 0
                        ? GameSocketParserStoredByteSelectorGateProjection
                            .SelectedDestinationRawFileOffset
                        : GameSocketParserStoredByteSelectorGateProjection.FirstPushRawFileOffset;
                    if (freshByte != 0 && selector is 1 or 2)
                    {
                        expectedParentStop =
                            GameSocketParserStoredByteSelectorGateProjection
                                .SelectedDestinationRawFileOffset;
                    }

                    rejectedSelectorAndZeroByteCasesExact &=
                        plan.StopBeforeRawFileOffset == expectedParentStop;
                }
            }
        }

        check("Zero bytes and selectors 1/2 preserve the parent's exact 3294 bound and unused captures",
            rejectedSelectorAndZeroByteCasesExact);

        GameSocketParserSelectorDiagnosticPreparationPlan rejectedAncestor =
            Project(
                selector: 0,
                freshByte: 1,
                freshESI: 0x7000,
                receiverValue: 0x12345678,
                callerCount: 1);
        check("A rejected ancestor forwards its actual bound without replaying or consuming child captures",
            !rejectedAncestor.ParentHandoffAdmitted
            && rejectedAncestor.Outcome
                == GameSocketParserSelectorDiagnosticPreparationOutcome.ParentHandoffNotAdmitted
            && rejectedAncestor.StopBeforeRawFileOffset
                == rejectedAncestor.ParentPlan.StopBeforeRawFileOffset
            && rejectedAncestor.StopBeforeRawFileOffset
                == GameSocketParserCallerCountGateProjection.BranchTargetRawFileOffset
            && rejectedAncestor.ExactBoundedCompletion
                == rejectedAncestor.ParentPlan.ExactBoundedCompletion
            && rejectedAncestor.ReachedBoundedEndpoint
                == rejectedAncestor.ParentPlan.ReachedBoundedEndpoint
            && rejectedAncestor.FreshESICaptureState
                == GameSocketParserSelectorDiagnosticPreparationCaptureState.SuppliedButUnused
            && rejectedAncestor.ReceiverFieldCaptureState
                == GameSocketParserSelectorDiagnosticPreparationCaptureState.SuppliedButUnused
            && rejectedAncestor.UnusedCaptures.Length == 2
            && rejectedAncestor.Pushes.IsEmpty
            && rejectedAncestor.Reads.IsEmpty
            && rejectedAncestor.Stores.IsEmpty
            && !rejectedAncestor.IsCleanCompletion
            && rejectedAncestor.OrderedMemoryPrefixPreserved);
    }

    private static GameSocketParserSelectorDiagnosticPreparationPlan Project(
        byte selector,
        byte freshByte,
        uint? freshESI,
        uint? receiverValue,
        uint postResizeESP = 0x3000,
        uint callerCount = 100) =>
        GameSocketParserSelectorDiagnosticPreparationProjection.Project(
            new GameSocketParserSelectorDiagnosticPreparationCapture(
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
                                        postResizeESP,
                                        0x5000,
                                        0x6000),
                                    (byte)(selector << 6)),
                                FreshSecondHeaderByte: 2),
                            0x7000,
                            callerCount),
                        FreshStackDwordAtESPPlus14: 0x1000,
                        FreshStackByteAtESPPlus13: freshByte)),
                freshESI,
                receiverValue));

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

    private static bool EvenParity(byte value) =>
        (System.Numerics.BitOperations.PopCount((uint)value) & 1) == 0;

    private static byte[] ParseBytes(string text) =>
        text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
}
