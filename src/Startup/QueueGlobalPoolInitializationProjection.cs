namespace Firefall.Reconstruction.Startup;

public readonly record struct QueueGlobalPoolInitializationCapture(
    uint ReceiverPointer,
    byte? GuardByte,
    bool? Diagnostic104ReturnedNormally,
    byte? ReceiverFlag,
    bool? Diagnostic105ReturnedNormally,
    uint? AllocationResult,
    bool? ConfigurationReturnedNormally,
    uint? CapturedOldHeadPointer);

public enum QueueGlobalPoolInitializationOutcome
{
    AwaitingGuardByte,
    AwaitingDiagnostic104Return,
    Diagnostic104DidNotReturn,
    AwaitingReceiverFlag,
    AwaitingDiagnostic105Return,
    Diagnostic105DidNotReturn,
    AwaitingAllocationResult,
    AwaitingConfigurationReturn,
    ConfigurationDidNotReturn,
    AwaitingOldHeadPointer,
    FlagStoreReached
}

public enum QueueGlobalPoolInitializationActionKind
{
    TestGuardByte,
    CallDiagnostic104ThroughIndirectCell,
    CaptureDiagnostic104Return,
    ContinueAfterDiagnostic104,
    SkipDiagnostic104,
    CaptureReceiverFlag,
    CallDiagnostic105ThroughIndirectCell,
    CaptureDiagnostic105Return,
    ContinueAfterDiagnostic105,
    SkipDiagnostic105,
    CallAllocator,
    CaptureAllocationResult,
    SetEdiToZeroForNullAllocation,
    SkipConfigurationForNullAllocation,
    CallAllocatorConfiguration,
    CaptureConfigurationReturn,
    ContinueAfterConfiguration,
    CaptureOldHeadPointer,
    PublishPoolPointerToReceiver,
    PublishOldHeadToReceiver,
    PublishReceiverToGlobalHead,
    StoreInitializedFlag,
    StopAtGuardByteCaptureBoundary,
    StopAtDiagnostic104ReturnBoundary,
    StopAtDiagnostic104NonreturnBoundary,
    StopAtReceiverFlagCaptureBoundary,
    StopAtDiagnostic105ReturnBoundary,
    StopAtDiagnostic105NonreturnBoundary,
    StopAtAllocationResultBoundary,
    StopAtConfigurationReturnBoundary,
    StopAtConfigurationNonreturnBoundary,
    StopAtOldHeadCaptureBoundary
}

public readonly record struct QueueGlobalPoolInitializationAction(
    QueueGlobalPoolInitializationActionKind Kind,
    uint InstructionRawFileOffset,
    uint? ReceiverAddress = null,
    uint? CapturedValue = null,
    uint? Value = null,
    uint? DiagnosticId = null,
    uint? IndirectCallCellVirtualAddress = null,
    uint? ReferencedVirtualAddress = null,
    uint? TargetRawFileOffset = null,
    uint? BranchTargetRawFileOffset = null,
    uint? FirstArgument = null,
    uint? SecondArgument = null,
    uint? ThirdArgument = null);

public readonly record struct QueueGlobalPoolInitializationInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    int? ReceiverRelativeOffset = null,
    uint? ReferencedVirtualAddress = null,
    uint? DiagnosticId = null,
    uint? IndirectCallCellVirtualAddress = null,
    uint? BranchTargetRawFileOffset = null,
    uint? TargetRawFileOffset = null,
    uint? ImmediateValue = null);

public readonly record struct QueueGlobalPoolInitializationProjection(
    QueueGlobalPoolInitializationCapture Capture,
    QueueGlobalPoolInitializationOutcome Outcome,
    IReadOnlyList<QueueGlobalPoolInitializationAction> Actions);

/// <summary>
/// Projects captured gates, allocation/configuration boundaries, and list publication
/// over raw file offsets 0xBBFAE7-0xBBFB67. Opaque reporter/configuration calls are
/// advanced only by explicit normal-return captures. This performs no native reads,
/// allocation, writes, ownership transfer, synchronization, or function-return modeling.
/// </summary>
public static class QueueGlobalPoolInitializationProjectionRules
{
    public const string SamplePath =
        @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe";
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint HelperRawFileOffset = 0x00BBFAC0;
    public const uint SliceStartRawFileOffset = 0x00BBFAE7;
    public const uint FlagStoreRawFileOffset = 0x00BBFB63;
    public const uint SliceEndRawFileOffsetExclusive = 0x00BBFB67;
    public const uint HelperVirtualAddress =
        HelperRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint SliceStartVirtualAddress =
        SliceStartRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint SliceEndVirtualAddress =
        SliceEndRawFileOffsetExclusive + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint GuardByteVirtualAddress = 0x01E4B095;
    public const uint GlobalHeadVirtualAddress = 0x01E4B0A8;
    public const uint ReporterIndirectCallCellVirtualAddress = 0x01C15EA0;
    public const uint DiagnosticTextVirtualAddress = 0x01C5EA58;
    public const uint Diagnostic105DetailVirtualAddress = 0x01C5E9C8;
    public const uint AllocationHelperRawFileOffset = 0x012B7FB0;
    public const uint AllocationHelperVirtualAddress =
        AllocationHelperRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint AllocatorConfigurationHelperRawFileOffset = 0x012CDB40;
    public const uint AllocatorConfigurationHelperVirtualAddress =
        AllocatorConfigurationHelperRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint Diagnostic104Id = 0x68;
    public const uint Diagnostic105Id = 0x69;
    public const uint AllocationSize = 0x28;
    public const uint ConfigurationFirstArgument = 8;
    public const uint ConfigurationSecondArgument = 32;
    public const uint ConfigurationThirdArgument = 8;
    public const int ReceiverFlagOffset = 0x04;
    public const int ReceiverPoolPointerOffset = 0x10;
    public const int ReceiverPreviousHeadOffset = 0x0C;

    public const string CapturedSliceBytes =
        "80 3D 95 B0 E4 01 00 75 17 6A 00 6A 68 68 58 EA C5 01 " +
        "68 58 EA C5 01 FF 15 A0 5E C1 01 83 C4 10 80 7E 04 00 " +
        "74 1A 68 C8 E9 C5 01 6A 69 68 58 EA C5 01 68 58 EA C5 01 " +
        "FF 15 A0 5E C1 01 83 C4 10 6A 28 E8 82 84 6F 00 8B F8 " +
        "83 C4 04 89 7D F0 C7 45 FC 00 00 00 00 85 FF 74 0F " +
        "6A 08 6A 20 6A 08 8B CF E8 F2 DF 70 00 EB 02 33 FF " +
        "A1 A8 B0 E4 01 89 7E 10 89 46 0C 89 35 A8 B0 E4 01 " +
        "C6 46 04 01";

    public static IReadOnlyList<QueueGlobalPoolInitializationInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Compare the independent global guard byte with zero",
                0xBBFAE7, "80 3D 95 B0 E4 01 00",
                ReferencedVirtualAddress: GuardByteVirtualAddress),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Skip diagnostic 0x68 when the guard byte is nonzero",
                0xBBFAEE, "75 17",
                BranchTargetRawFileOffset: 0xBBFB07),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Push diagnostic argument zero",
                0xBBFAF0, "6A 00", ImmediateValue: 0),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Push diagnostic ID 0x68",
                0xBBFAF2, "6A 68", DiagnosticId: Diagnostic104Id),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Push diagnostic text address",
                0xBBFAF4, "68 58 EA C5 01",
                ReferencedVirtualAddress: DiagnosticTextVirtualAddress),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Push the second diagnostic text address",
                0xBBFAF9, "68 58 EA C5 01",
                ReferencedVirtualAddress: DiagnosticTextVirtualAddress),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Call the reporter through its indirect cell",
                0xBBFAFE, "FF 15 A0 5E C1 01",
                IndirectCallCellVirtualAddress: ReporterIndirectCallCellVirtualAddress),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Clean diagnostic 0x68 arguments after its call",
                0xBBFB04, "83 C4 10"),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Compare the independent receiver flag byte with zero",
                0xBBFB07, "80 7E 04 00",
                ReceiverRelativeOffset: ReceiverFlagOffset,
                ImmediateValue: 0),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Skip diagnostic 0x69 when the receiver flag is zero",
                0xBBFB0B, "74 1A",
                BranchTargetRawFileOffset: 0xBBFB27),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Push diagnostic 0x69 detail address",
                0xBBFB0D, "68 C8 E9 C5 01",
                ReferencedVirtualAddress: Diagnostic105DetailVirtualAddress),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Push diagnostic ID 0x69",
                0xBBFB12, "6A 69", DiagnosticId: Diagnostic105Id),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Push diagnostic text address",
                0xBBFB14, "68 58 EA C5 01",
                ReferencedVirtualAddress: DiagnosticTextVirtualAddress),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Push the second diagnostic text address",
                0xBBFB19, "68 58 EA C5 01",
                ReferencedVirtualAddress: DiagnosticTextVirtualAddress),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Call the reporter through its indirect cell",
                0xBBFB1E, "FF 15 A0 5E C1 01",
                IndirectCallCellVirtualAddress: ReporterIndirectCallCellVirtualAddress),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Clean diagnostic 0x69 arguments after its call",
                0xBBFB24, "83 C4 10"),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Push allocation size 0x28",
                0xBBFB27, "6A 28", ImmediateValue: AllocationSize),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Call the allocation wrapper",
                0xBBFB29, "E8 82 84 6F 00",
                TargetRawFileOffset: AllocationHelperRawFileOffset),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Capture the allocation result in EDI",
                0xBBFB2E, "8B F8"),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Clean the allocation-size argument",
                0xBBFB30, "83 C4 04"),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Save the captured allocation result",
                0xBBFB33, "89 7D F0"),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Initialize the local cleanup state",
                0xBBFB36, "C7 45 FC 00 00 00 00",
                ImmediateValue: 0),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Test the allocation result",
                0xBBFB3D, "85 FF"),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Skip allocator configuration for a null allocation",
                0xBBFB3F, "74 0F",
                BranchTargetRawFileOffset: 0xBBFB50),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Push configuration argument 8",
                0xBBFB41, "6A 08", ImmediateValue: ConfigurationFirstArgument),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Push configuration argument 32",
                0xBBFB43, "6A 20", ImmediateValue: ConfigurationSecondArgument),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Push configuration argument 8",
                0xBBFB45, "6A 08", ImmediateValue: ConfigurationThirdArgument),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Pass the allocation result as the configuration receiver",
                0xBBFB47, "8B CF"),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Call allocator configuration",
                0xBBFB49, "E8 F2 DF 70 00",
                TargetRawFileOffset: AllocatorConfigurationHelperRawFileOffset),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Join publication after a normal configuration return",
                0xBBFB4E, "EB 02",
                BranchTargetRawFileOffset: 0xBBFB52),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Set EDI to zero for a null allocation",
                0xBBFB50, "33 FF", ImmediateValue: 0),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Read the independent previous global head",
                0xBBFB52, "A1 A8 B0 E4 01",
                ReferencedVirtualAddress: GlobalHeadVirtualAddress),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Store the pool pointer in receiver field +0x10",
                0xBBFB57, "89 7E 10",
                ReceiverRelativeOffset: ReceiverPoolPointerOffset),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Store the previous head in receiver field +0x0C",
                0xBBFB5A, "89 46 0C",
                ReceiverRelativeOffset: ReceiverPreviousHeadOffset),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Publish the receiver as the global list head",
                0xBBFB5D, "89 35 A8 B0 E4 01",
                ReferencedVirtualAddress: GlobalHeadVirtualAddress),
            new QueueGlobalPoolInitializationInstructionEvidence(
                "Store initialized flag byte 1 in receiver field +4",
                0xBBFB63, "C6 46 04 01",
                ReceiverRelativeOffset: ReceiverFlagOffset,
                ImmediateValue: 1)
        });

    /// <summary>
    /// Follows the captured branch conditions in order. Nullable helper-return fields
    /// mean that the return boundary has not been captured; false explicitly means
    /// that normal return was not observed. Unreached captures are rejected.
    /// </summary>
    public static QueueGlobalPoolInitializationProjection Project(
        QueueGlobalPoolInitializationCapture capture)
    {
        var actions = new List<QueueGlobalPoolInitializationAction>();

        if (!capture.GuardByte.HasValue)
        {
            EnsureUnreached(
                capture.Diagnostic104ReturnedNormally,
                capture.ReceiverFlag,
                capture.Diagnostic105ReturnedNormally,
                capture.AllocationResult,
                capture.ConfigurationReturnedNormally,
                capture.CapturedOldHeadPointer);
            return Stop(
                QueueGlobalPoolInitializationOutcome.AwaitingGuardByte,
                QueueGlobalPoolInitializationActionKind.StopAtGuardByteCaptureBoundary,
                SliceStartRawFileOffset);
        }

        actions.Add(new QueueGlobalPoolInitializationAction(
            QueueGlobalPoolInitializationActionKind.TestGuardByte,
            SliceStartRawFileOffset,
            CapturedValue: capture.GuardByte.Value,
            ReferencedVirtualAddress: GuardByteVirtualAddress));

        if (capture.GuardByte.Value == 0)
        {
            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.CallDiagnostic104ThroughIndirectCell,
                0xBBFAFE,
                DiagnosticId: Diagnostic104Id,
                IndirectCallCellVirtualAddress: ReporterIndirectCallCellVirtualAddress));
            if (!capture.Diagnostic104ReturnedNormally.HasValue)
            {
                EnsureUnreached(
                    capture.ReceiverFlag,
                    capture.Diagnostic105ReturnedNormally,
                    capture.AllocationResult,
                    capture.ConfigurationReturnedNormally,
                    capture.CapturedOldHeadPointer);
                actions.Add(new QueueGlobalPoolInitializationAction(
                    QueueGlobalPoolInitializationActionKind.StopAtDiagnostic104ReturnBoundary,
                    0xBBFB04));
                return Result(QueueGlobalPoolInitializationOutcome.AwaitingDiagnostic104Return);
            }

            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.CaptureDiagnostic104Return,
                0xBBFB04,
                CapturedValue: capture.Diagnostic104ReturnedNormally.Value ? 1u : 0u));
            if (!capture.Diagnostic104ReturnedNormally.Value)
            {
                EnsureUnreached(
                    capture.ReceiverFlag,
                    capture.Diagnostic105ReturnedNormally,
                    capture.AllocationResult,
                    capture.ConfigurationReturnedNormally,
                    capture.CapturedOldHeadPointer);
                actions.Add(new QueueGlobalPoolInitializationAction(
                    QueueGlobalPoolInitializationActionKind.StopAtDiagnostic104NonreturnBoundary,
                    0xBBFB04));
                return Result(QueueGlobalPoolInitializationOutcome.Diagnostic104DidNotReturn);
            }

            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.ContinueAfterDiagnostic104,
                0xBBFB07));
        }
        else
        {
            EnsureUnreached(capture.Diagnostic104ReturnedNormally);
            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.SkipDiagnostic104,
                0xBBFAEE,
                BranchTargetRawFileOffset: 0xBBFB07));
        }

        if (!capture.ReceiverFlag.HasValue)
        {
            EnsureUnreached(
                capture.Diagnostic105ReturnedNormally,
                capture.AllocationResult,
                capture.ConfigurationReturnedNormally,
                capture.CapturedOldHeadPointer);
            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.StopAtReceiverFlagCaptureBoundary,
                0xBBFB07,
                ReceiverAddress: AddAddress(capture.ReceiverPointer, ReceiverFlagOffset)));
            return Result(QueueGlobalPoolInitializationOutcome.AwaitingReceiverFlag);
        }

        actions.Add(new QueueGlobalPoolInitializationAction(
            QueueGlobalPoolInitializationActionKind.CaptureReceiverFlag,
            0xBBFB07,
            ReceiverAddress: AddAddress(capture.ReceiverPointer, ReceiverFlagOffset),
            CapturedValue: capture.ReceiverFlag.Value));

        if (capture.ReceiverFlag.Value != 0)
        {
            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.CallDiagnostic105ThroughIndirectCell,
                0xBBFB1E,
                DiagnosticId: Diagnostic105Id,
                IndirectCallCellVirtualAddress: ReporterIndirectCallCellVirtualAddress));
            if (!capture.Diagnostic105ReturnedNormally.HasValue)
            {
                EnsureUnreached(
                    capture.AllocationResult,
                    capture.ConfigurationReturnedNormally,
                    capture.CapturedOldHeadPointer);
                actions.Add(new QueueGlobalPoolInitializationAction(
                    QueueGlobalPoolInitializationActionKind.StopAtDiagnostic105ReturnBoundary,
                    0xBBFB24));
                return Result(QueueGlobalPoolInitializationOutcome.AwaitingDiagnostic105Return);
            }

            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.CaptureDiagnostic105Return,
                0xBBFB24,
                CapturedValue: capture.Diagnostic105ReturnedNormally.Value ? 1u : 0u));
            if (!capture.Diagnostic105ReturnedNormally.Value)
            {
                EnsureUnreached(
                    capture.AllocationResult,
                    capture.ConfigurationReturnedNormally,
                    capture.CapturedOldHeadPointer);
                actions.Add(new QueueGlobalPoolInitializationAction(
                    QueueGlobalPoolInitializationActionKind.StopAtDiagnostic105NonreturnBoundary,
                    0xBBFB24));
                return Result(QueueGlobalPoolInitializationOutcome.Diagnostic105DidNotReturn);
            }

            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.ContinueAfterDiagnostic105,
                0xBBFB27));
        }
        else
        {
            EnsureUnreached(capture.Diagnostic105ReturnedNormally);
            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.SkipDiagnostic105,
                0xBBFB0B,
                BranchTargetRawFileOffset: 0xBBFB27));
        }

        actions.Add(new QueueGlobalPoolInitializationAction(
            QueueGlobalPoolInitializationActionKind.CallAllocator,
            0xBBFB29,
            Value: AllocationSize,
            TargetRawFileOffset: AllocationHelperRawFileOffset));
        if (!capture.AllocationResult.HasValue)
        {
            EnsureUnreached(
                capture.ConfigurationReturnedNormally,
                capture.CapturedOldHeadPointer);
            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.StopAtAllocationResultBoundary,
                0xBBFB2E));
            return Result(QueueGlobalPoolInitializationOutcome.AwaitingAllocationResult);
        }

        actions.Add(new QueueGlobalPoolInitializationAction(
            QueueGlobalPoolInitializationActionKind.CaptureAllocationResult,
            0xBBFB2E,
            CapturedValue: capture.AllocationResult.Value));

        if (capture.AllocationResult.Value == 0)
        {
            EnsureUnreached(capture.ConfigurationReturnedNormally);
            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.SkipConfigurationForNullAllocation,
                0xBBFB3F,
                BranchTargetRawFileOffset: 0xBBFB50));
            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.SetEdiToZeroForNullAllocation,
                0xBBFB50,
                Value: 0));
        }
        else
        {
            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.CallAllocatorConfiguration,
                0xBBFB49,
                ReceiverAddress: capture.AllocationResult.Value,
                TargetRawFileOffset: AllocatorConfigurationHelperRawFileOffset,
                FirstArgument: ConfigurationFirstArgument,
                SecondArgument: ConfigurationSecondArgument,
                ThirdArgument: ConfigurationThirdArgument));
            if (!capture.ConfigurationReturnedNormally.HasValue)
            {
                EnsureUnreached(capture.CapturedOldHeadPointer);
                actions.Add(new QueueGlobalPoolInitializationAction(
                    QueueGlobalPoolInitializationActionKind.StopAtConfigurationReturnBoundary,
                    0xBBFB49));
                return Result(QueueGlobalPoolInitializationOutcome.AwaitingConfigurationReturn);
            }

            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.CaptureConfigurationReturn,
                0xBBFB4E,
                CapturedValue: capture.ConfigurationReturnedNormally.Value ? 1u : 0u));
            if (!capture.ConfigurationReturnedNormally.Value)
            {
                EnsureUnreached(capture.CapturedOldHeadPointer);
                actions.Add(new QueueGlobalPoolInitializationAction(
                    QueueGlobalPoolInitializationActionKind.StopAtConfigurationNonreturnBoundary,
                    0xBBFB49));
                return Result(QueueGlobalPoolInitializationOutcome.ConfigurationDidNotReturn);
            }

            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.ContinueAfterConfiguration,
                0xBBFB52,
                BranchTargetRawFileOffset: 0xBBFB52));
        }

        if (!capture.CapturedOldHeadPointer.HasValue)
        {
            actions.Add(new QueueGlobalPoolInitializationAction(
                QueueGlobalPoolInitializationActionKind.StopAtOldHeadCaptureBoundary,
                0xBBFB52,
                ReferencedVirtualAddress: GlobalHeadVirtualAddress));
            return Result(QueueGlobalPoolInitializationOutcome.AwaitingOldHeadPointer);
        }

        actions.Add(new QueueGlobalPoolInitializationAction(
            QueueGlobalPoolInitializationActionKind.CaptureOldHeadPointer,
            0xBBFB52,
            CapturedValue: capture.CapturedOldHeadPointer.Value,
            ReferencedVirtualAddress: GlobalHeadVirtualAddress));
        actions.Add(new QueueGlobalPoolInitializationAction(
            QueueGlobalPoolInitializationActionKind.PublishPoolPointerToReceiver,
            0xBBFB57,
            ReceiverAddress: AddAddress(capture.ReceiverPointer, ReceiverPoolPointerOffset),
            CapturedValue: capture.AllocationResult.Value,
            Value: capture.AllocationResult.Value));
        actions.Add(new QueueGlobalPoolInitializationAction(
            QueueGlobalPoolInitializationActionKind.PublishOldHeadToReceiver,
            0xBBFB5A,
            ReceiverAddress: AddAddress(capture.ReceiverPointer, ReceiverPreviousHeadOffset),
            CapturedValue: capture.CapturedOldHeadPointer.Value,
            Value: capture.CapturedOldHeadPointer.Value));
        actions.Add(new QueueGlobalPoolInitializationAction(
            QueueGlobalPoolInitializationActionKind.PublishReceiverToGlobalHead,
            0xBBFB5D,
            CapturedValue: capture.ReceiverPointer,
            Value: capture.ReceiverPointer,
            ReferencedVirtualAddress: GlobalHeadVirtualAddress));
        actions.Add(new QueueGlobalPoolInitializationAction(
            QueueGlobalPoolInitializationActionKind.StoreInitializedFlag,
            FlagStoreRawFileOffset,
            ReceiverAddress: AddAddress(capture.ReceiverPointer, ReceiverFlagOffset),
            Value: 1));

        return Result(QueueGlobalPoolInitializationOutcome.FlagStoreReached);

        QueueGlobalPoolInitializationProjection Result(
            QueueGlobalPoolInitializationOutcome outcome) =>
            new(capture, outcome, Array.AsReadOnly(actions.ToArray()));

        QueueGlobalPoolInitializationProjection Stop(
            QueueGlobalPoolInitializationOutcome outcome,
            QueueGlobalPoolInitializationActionKind kind,
            uint instructionRawFileOffset)
        {
            actions.Add(new QueueGlobalPoolInitializationAction(
                kind,
                instructionRawFileOffset));
            return Result(outcome);
        }
    }

    private static uint AddAddress(uint address, int offset) =>
        unchecked(address + (uint)offset);

    private static void EnsureUnreached(params object?[] supplied)
    {
        if (supplied.Any(value => value is not null))
        {
            throw new ArgumentException(
                "A capture was supplied for a value that the selected path has not reached.");
        }
    }
}
