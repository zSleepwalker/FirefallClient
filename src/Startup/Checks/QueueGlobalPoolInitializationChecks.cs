using Firefall.Reconstruction.Startup;

internal static class QueueGlobalPoolInitializationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Shared sentinel-pool slice is pinned to the sample, exact bounds, and raw-to-VA mapping",
            QueueGlobalPoolInitializationProjectionRules.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && QueueGlobalPoolInitializationProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && QueueGlobalPoolInitializationProjectionRules.HelperRawFileOffset == 0xBBFAC0
            && QueueGlobalPoolInitializationProjectionRules.HelperVirtualAddress == 0xFC06C0
            && QueueGlobalPoolInitializationProjectionRules.SliceStartRawFileOffset == 0xBBFAE7
            && QueueGlobalPoolInitializationProjectionRules.SliceStartVirtualAddress == 0xFC06E7
            && QueueGlobalPoolInitializationProjectionRules.FlagStoreRawFileOffset == 0xBBFB63
            && QueueGlobalPoolInitializationProjectionRules.SliceEndRawFileOffsetExclusive == 0xBBFB67
            && QueueGlobalPoolInitializationProjectionRules.SliceEndVirtualAddress == 0xFC0767
            && QueueGlobalPoolInitializationProjectionRules.SliceEndRawFileOffsetExclusive
                - QueueGlobalPoolInitializationProjectionRules.SliceStartRawFileOffset == 0x80
            && QueueGlobalPoolInitializationProjectionRules.SliceStartRawFileOffset
                + QueueGlobalPoolInitializationProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueGlobalPoolInitializationProjectionRules.SliceStartVirtualAddress
            && QueueGlobalPoolInitializationProjectionRules.SliceEndRawFileOffsetExclusive
                + QueueGlobalPoolInitializationProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueGlobalPoolInitializationProjectionRules.SliceEndVirtualAddress
            && QueueGlobalPoolInitializationProjectionRules.Instructions[^1]
                is { RawFileOffset: 0xBBFB63, Bytes: "C6 46 04 01" }
            && QueueGlobalPoolInitializationProjectionRules.Instructions[^1].RawFileOffset
                + 4 == QueueGlobalPoolInitializationProjectionRules.SliceEndRawFileOffsetExclusive);

        var expectedInstructions = new (uint RawFileOffset, string Bytes)[]
        {
            (0xBBFAE7, "80 3D 95 B0 E4 01 00"),
            (0xBBFAEE, "75 17"),
            (0xBBFAF0, "6A 00"),
            (0xBBFAF2, "6A 68"),
            (0xBBFAF4, "68 58 EA C5 01"),
            (0xBBFAF9, "68 58 EA C5 01"),
            (0xBBFAFE, "FF 15 A0 5E C1 01"),
            (0xBBFB04, "83 C4 10"),
            (0xBBFB07, "80 7E 04 00"),
            (0xBBFB0B, "74 1A"),
            (0xBBFB0D, "68 C8 E9 C5 01"),
            (0xBBFB12, "6A 69"),
            (0xBBFB14, "68 58 EA C5 01"),
            (0xBBFB19, "68 58 EA C5 01"),
            (0xBBFB1E, "FF 15 A0 5E C1 01"),
            (0xBBFB24, "83 C4 10"),
            (0xBBFB27, "6A 28"),
            (0xBBFB29, "E8 82 84 6F 00"),
            (0xBBFB2E, "8B F8"),
            (0xBBFB30, "83 C4 04"),
            (0xBBFB33, "89 7D F0"),
            (0xBBFB36, "C7 45 FC 00 00 00 00"),
            (0xBBFB3D, "85 FF"),
            (0xBBFB3F, "74 0F"),
            (0xBBFB41, "6A 08"),
            (0xBBFB43, "6A 20"),
            (0xBBFB45, "6A 08"),
            (0xBBFB47, "8B CF"),
            (0xBBFB49, "E8 F2 DF 70 00"),
            (0xBBFB4E, "EB 02"),
            (0xBBFB50, "33 FF"),
            (0xBBFB52, "A1 A8 B0 E4 01"),
            (0xBBFB57, "89 7E 10"),
            (0xBBFB5A, "89 46 0C"),
            (0xBBFB5D, "89 35 A8 B0 E4 01"),
            (0xBBFB63, "C6 46 04 01")
        };
        check("Captured bytes and instruction anchors match the entire bounded slice through the flag store",
            QueueGlobalPoolInitializationProjectionRules.CapturedSliceBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x80
            && QueueGlobalPoolInitializationProjectionRules.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expectedInstructions)
            && QueueGlobalPoolInitializationProjectionRules.Instructions.All(instruction =>
                HasSlice(
                    QueueGlobalPoolInitializationProjectionRules.CapturedSliceBytes,
                    QueueGlobalPoolInitializationProjectionRules.SliceStartRawFileOffset,
                    instruction.RawFileOffset,
                    instruction.Bytes))
            && QueueGlobalPoolInitializationProjectionRules.GuardByteVirtualAddress == 0x1E4B095
            && QueueGlobalPoolInitializationProjectionRules.GlobalHeadVirtualAddress == 0x1E4B0A8
            && QueueGlobalPoolInitializationProjectionRules.ReporterIndirectCallCellVirtualAddress
                == 0x1C15EA0
            && QueueGlobalPoolInitializationProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0xBBFB29)
                .TargetRawFileOffset == 0x12B7FB0
            && QueueGlobalPoolInitializationProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0xBBFB49)
                .TargetRawFileOffset == 0x12CDB40
            && QueueGlobalPoolInitializationProjectionRules.AllocationHelperVirtualAddress
                == 0x16B8BB0
            && QueueGlobalPoolInitializationProjectionRules.AllocatorConfigurationHelperVirtualAddress
                == 0x16CE740
            && QueueGlobalPoolInitializationProjectionRules.AllocationHelperRawFileOffset
                + QueueGlobalPoolInitializationProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueGlobalPoolInitializationProjectionRules.AllocationHelperVirtualAddress
            && QueueGlobalPoolInitializationProjectionRules.AllocatorConfigurationHelperRawFileOffset
                + QueueGlobalPoolInitializationProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == QueueGlobalPoolInitializationProjectionRules.AllocatorConfigurationHelperVirtualAddress
            && RelativeCallTargets(0xBBFB29, "E8 82 84 6F 00", 0x12B7FB0)
            && RelativeCallTargets(0xBBFB49, "E8 F2 DF 70 00", 0x12CDB40)
            && 0xBBFAEE + 2 + 0x17 == 0xBBFB07
            && 0xBBFB0B + 2 + 0x1A == 0xBBFB27
            && 0xBBFB3F + 2 + 0x0F == 0xBBFB50
            && 0xBBFB4E + 2 + 2 == 0xBBFB52);

        var missingGuard = QueueGlobalPoolInitializationProjectionRules.Project(Capture());
        var missingGuardWithUnreachedDiagnostic =
            ThrowsArgumentException(() => QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(diagnostic104Returned: true)));
        var guardZeroMissingReporterReturn =
            QueueGlobalPoolInitializationProjectionRules.Project(Capture(guard: 0));
        var guardZeroReporterDidNotReturn =
            QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(guard: 0, diagnostic104Returned: false));
        var guardZeroReporterReturnedMissingFlag =
            QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(guard: 0, diagnostic104Returned: true));
        var nonzeroGuard = QueueGlobalPoolInitializationProjectionRules.Project(
            Capture(guard: 1, receiverFlag: 0));
        var nonzeroGuardWithUnreachedDiagnostic =
            ThrowsArgumentException(() => QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(guard: 1, diagnostic104Returned: true)));

        check("The first gate reports 0x68 only for a zero captured guard and requires its own normal-return capture",
            missingGuard.Outcome == QueueGlobalPoolInitializationOutcome.AwaitingGuardByte
            && missingGuard.Actions[^1].Kind
                == QueueGlobalPoolInitializationActionKind.StopAtGuardByteCaptureBoundary
            && missingGuardWithUnreachedDiagnostic
            && guardZeroMissingReporterReturn.Outcome
                == QueueGlobalPoolInitializationOutcome.AwaitingDiagnostic104Return
            && guardZeroMissingReporterReturn.Actions.Any(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.CallDiagnostic104ThroughIndirectCell
                && action.DiagnosticId == 0x68
                && action.IndirectCallCellVirtualAddress == 0x1C15EA0)
            && guardZeroMissingReporterReturn.Actions[^1].Kind
                == QueueGlobalPoolInitializationActionKind.StopAtDiagnostic104ReturnBoundary
            && guardZeroReporterDidNotReturn.Outcome
                == QueueGlobalPoolInitializationOutcome.Diagnostic104DidNotReturn
            && guardZeroReporterReturnedMissingFlag.Outcome
                == QueueGlobalPoolInitializationOutcome.AwaitingReceiverFlag
            && nonzeroGuard.Outcome == QueueGlobalPoolInitializationOutcome.AwaitingAllocationResult
            && nonzeroGuard.Actions.Any(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.SkipDiagnostic104)
            && nonzeroGuardWithUnreachedDiagnostic);

        var missingFlag = QueueGlobalPoolInitializationProjectionRules.Project(
            Capture(guard: 1));
        var zeroFlag = QueueGlobalPoolInitializationProjectionRules.Project(
            Capture(guard: 1, receiverFlag: 0));
        var zeroFlagWithUnreachedDiagnostic =
            ThrowsArgumentException(() => QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(guard: 1, receiverFlag: 0, diagnostic105Returned: true)));
        var nonzeroFlagMissingReturn =
            QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(guard: 1, receiverFlag: 1));
        var nonzeroFlagDidNotReturn =
            QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(guard: 1, receiverFlag: 1, diagnostic105Returned: false));
        var nonzeroFlagReturned =
            QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(guard: 1, receiverFlag: 1, diagnostic105Returned: true));

        check("The independent receiver flag gates diagnostic 0x69 and its captured return gates allocation",
            missingFlag.Outcome == QueueGlobalPoolInitializationOutcome.AwaitingReceiverFlag
            && missingFlag.Actions[^1].Kind
                == QueueGlobalPoolInitializationActionKind.StopAtReceiverFlagCaptureBoundary
            && zeroFlag.Outcome == QueueGlobalPoolInitializationOutcome.AwaitingAllocationResult
            && zeroFlag.Actions.Any(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.SkipDiagnostic105)
            && zeroFlagWithUnreachedDiagnostic
            && nonzeroFlagMissingReturn.Outcome
                == QueueGlobalPoolInitializationOutcome.AwaitingDiagnostic105Return
            && nonzeroFlagMissingReturn.Actions.Any(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.CallDiagnostic105ThroughIndirectCell
                && action.DiagnosticId == 0x69)
            && nonzeroFlagDidNotReturn.Outcome
                == QueueGlobalPoolInitializationOutcome.Diagnostic105DidNotReturn
            && nonzeroFlagReturned.Outcome == QueueGlobalPoolInitializationOutcome.AwaitingAllocationResult);

        var missingAllocation = QueueGlobalPoolInitializationProjectionRules.Project(
            Capture(guard: 1, receiverFlag: 0));
        var allocationWithUnreachedConfiguration =
            ThrowsArgumentException(() => QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(guard: 1, receiverFlag: 0, configurationReturned: true)));
        check("Allocation size and result have an explicit capture boundary before configuration or publication",
            missingAllocation.Outcome == QueueGlobalPoolInitializationOutcome.AwaitingAllocationResult
            && missingAllocation.Actions.Any(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.CallAllocator
                && action.Value == 0x28
                && action.TargetRawFileOffset == 0x12B7FB0)
            && missingAllocation.Actions[^1].Kind
                == QueueGlobalPoolInitializationActionKind.StopAtAllocationResultBoundary
            && allocationWithUnreachedConfiguration);

        var nullAllocation = QueueGlobalPoolInitializationProjectionRules.Project(
            Capture(
                guard: 1,
                receiverFlag: 0,
                allocation: 0,
                oldHead: 0x55667788));
        var nullAllocationWithUnreachedConfiguration =
            ThrowsArgumentException(() => QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(
                    guard: 1,
                    receiverFlag: 0,
                    allocation: 0,
                    configurationReturned: false)));
        check("A null allocation sets EDI to zero, skips configuration, and still publishes the list and flag",
            nullAllocation.Outcome == QueueGlobalPoolInitializationOutcome.FlagStoreReached
            && nullAllocationWithUnreachedConfiguration
            && nullAllocation.Actions.Any(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.SetEdiToZeroForNullAllocation
                && action.InstructionRawFileOffset == 0xBBFB50
                && action.Value == 0)
            && nullAllocation.Actions.Any(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.SkipConfigurationForNullAllocation
                && action.BranchTargetRawFileOffset == 0xBBFB50)
            && !nullAllocation.Actions.Any(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.CallAllocatorConfiguration)
            && nullAllocation.Actions
                .Where(action => action.Kind is
                    QueueGlobalPoolInitializationActionKind.PublishPoolPointerToReceiver
                    or QueueGlobalPoolInitializationActionKind.PublishOldHeadToReceiver
                    or QueueGlobalPoolInitializationActionKind.PublishReceiverToGlobalHead
                    or QueueGlobalPoolInitializationActionKind.StoreInitializedFlag)
                .Select(action => action.InstructionRawFileOffset)
                .SequenceEqual(new uint[] { 0xBBFB57, 0xBBFB5A, 0xBBFB5D, 0xBBFB63 })
            && nullAllocation.Actions
                .Single(action => action.Kind
                    == QueueGlobalPoolInitializationActionKind.PublishPoolPointerToReceiver)
                is { ReceiverAddress: 0x1010, CapturedValue: 0, Value: 0 }
            && nullAllocation.Actions
                .Single(action => action.Kind
                    == QueueGlobalPoolInitializationActionKind.PublishOldHeadToReceiver)
                is { ReceiverAddress: 0x100C, CapturedValue: 0x55667788 }
            && nullAllocation.Actions
                .Single(action => action.Kind
                    == QueueGlobalPoolInitializationActionKind.PublishReceiverToGlobalHead)
                is { Value: 0x1000, ReferencedVirtualAddress: 0x1E4B0A8 }
            && nullAllocation.Actions[^1]
                is { Kind: QueueGlobalPoolInitializationActionKind.StoreInitializedFlag,
                    InstructionRawFileOffset: 0xBBFB63,
                    ReceiverAddress: 0x1004,
                    Value: 1 });

        var configurationMissing =
            QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(guard: 1, receiverFlag: 0, allocation: 0x2000));
        var configurationDidNotReturn =
            QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(
                    guard: 1,
                    receiverFlag: 0,
                    allocation: 0x2000,
                    configurationReturned: false));
        var configurationReturnWithUnreachedHead =
            ThrowsArgumentException(() => QueueGlobalPoolInitializationProjectionRules.Project(
                Capture(
                    guard: 1,
                    receiverFlag: 0,
                    allocation: 0x2000,
                    configurationReturned: null,
                    oldHead: 0x1234)));
        check("Non-null allocation configuration requires an explicit normal return before reading or publishing the old head",
            configurationMissing.Outcome
                == QueueGlobalPoolInitializationOutcome.AwaitingConfigurationReturn
            && configurationMissing.Actions.Any(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.CallAllocatorConfiguration
                && action.InstructionRawFileOffset == 0xBBFB49
                && action.ReceiverAddress == 0x2000
                && action.FirstArgument == 8
                && action.SecondArgument == 32
                && action.ThirdArgument == 8
                && action.TargetRawFileOffset == 0x12CDB40)
            && configurationMissing.Actions[^1].Kind
                == QueueGlobalPoolInitializationActionKind.StopAtConfigurationReturnBoundary
            && configurationDidNotReturn.Outcome
                == QueueGlobalPoolInitializationOutcome.ConfigurationDidNotReturn
            && !configurationDidNotReturn.Actions.Any(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.CaptureOldHeadPointer
                || action.Kind == QueueGlobalPoolInitializationActionKind.PublishReceiverToGlobalHead)
            && configurationReturnWithUnreachedHead);

        var missingOldHead = QueueGlobalPoolInitializationProjectionRules.Project(
            Capture(
                guard: 1,
                receiverFlag: 0,
                allocation: 0x2000,
                configurationReturned: true));
        var nonnullAllocation = QueueGlobalPoolInitializationProjectionRules.Project(
            Capture(
                guard: 1,
                receiverFlag: 0,
                allocation: 0x2000,
                configurationReturned: true,
                oldHead: 0xAABBCCDD));
        check("Captured old head remains independent, and publication order ends exactly at the flag store",
            missingOldHead.Outcome == QueueGlobalPoolInitializationOutcome.AwaitingOldHeadPointer
            && missingOldHead.Actions[^1].InstructionRawFileOffset == 0xBBFB52
            && nonnullAllocation.Outcome == QueueGlobalPoolInitializationOutcome.FlagStoreReached
            && nonnullAllocation.Actions
                .Where(action => action.Kind is
                    QueueGlobalPoolInitializationActionKind.CaptureOldHeadPointer
                    or QueueGlobalPoolInitializationActionKind.PublishPoolPointerToReceiver
                    or QueueGlobalPoolInitializationActionKind.PublishOldHeadToReceiver
                    or QueueGlobalPoolInitializationActionKind.PublishReceiverToGlobalHead
                    or QueueGlobalPoolInitializationActionKind.StoreInitializedFlag)
                .Select(action => action.InstructionRawFileOffset)
                .SequenceEqual(new uint[] { 0xBBFB52, 0xBBFB57, 0xBBFB5A, 0xBBFB5D, 0xBBFB63 })
            && nonnullAllocation.Actions
                .Single(action => action.Kind
                    == QueueGlobalPoolInitializationActionKind.CaptureOldHeadPointer)
                .CapturedValue == 0xAABBCCDD
            && nonnullAllocation.Actions[^1].InstructionRawFileOffset
                + 4 == QueueGlobalPoolInitializationProjectionRules.SliceEndRawFileOffsetExclusive
            && !nonnullAllocation.Actions.Any(action =>
                action.InstructionRawFileOffset >= 0xBBFB67));

        var wrappedReceiver = QueueGlobalPoolInitializationProjectionRules.Project(
            Capture(
                receiver: 0xFFFFFFFE,
                guard: 1,
                receiverFlag: 0,
                allocation: 0,
                oldHead: 0));
        check("Receiver-relative addresses use 32-bit wraparound without performing writes",
            wrappedReceiver.Outcome == QueueGlobalPoolInitializationOutcome.FlagStoreReached
            && wrappedReceiver.Actions.Single(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.CaptureReceiverFlag)
                .ReceiverAddress == 0x00000002
            && wrappedReceiver.Actions.Single(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.PublishPoolPointerToReceiver)
                .ReceiverAddress == 0x0000000E
            && wrappedReceiver.Actions.Single(action =>
                action.Kind == QueueGlobalPoolInitializationActionKind.PublishOldHeadToReceiver)
                .ReceiverAddress == 0x0000000A
            && wrappedReceiver.Actions[^1].ReceiverAddress == 0x00000002);
    }

    private static QueueGlobalPoolInitializationCapture Capture(
        uint receiver = 0x1000,
        byte? guard = null,
        bool? diagnostic104Returned = null,
        byte? receiverFlag = null,
        bool? diagnostic105Returned = null,
        uint? allocation = null,
        bool? configurationReturned = null,
        uint? oldHead = null) =>
        new(
            receiver,
            guard,
            diagnostic104Returned,
            receiverFlag,
            diagnostic105Returned,
            allocation,
            configurationReturned,
            oldHead);

    private static bool HasSlice(
        string fullHexBytes,
        uint startRawFileOffset,
        uint rawFileOffset,
        string expectedHexBytes)
    {
        string[] fullBytes = fullHexBytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] expectedBytes = expectedHexBytes.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int start = checked((int)(rawFileOffset - startRawFileOffset));
        return start >= 0
            && start + expectedBytes.Length <= fullBytes.Length
            && fullBytes.Skip(start).Take(expectedBytes.Length).SequenceEqual(expectedBytes);
    }

    private static bool RelativeCallTargets(
        uint instructionRawFileOffset,
        string callBytes,
        uint expectedTargetRawFileOffset)
    {
        byte[] bytes = callBytes.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();
        if (bytes.Length != 5 || bytes[0] != 0xE8)
        {
            return false;
        }

        int displacement = BitConverter.ToInt32(bytes, 1);
        return unchecked(instructionRawFileOffset + 5u + (uint)displacement)
            == expectedTargetRawFileOffset;
    }

    private static bool ThrowsArgumentException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }
}
