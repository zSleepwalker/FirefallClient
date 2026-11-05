using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Independent values captured at their respective tail operations. The
/// receiver value is the reload from EBP-4 at raw BD5CC0, not an earlier entry
/// capture; it is unavailable until the preceding path reaches that reload.
/// </summary>
public readonly record struct SocketPayloadRecycleTailCapture(
    uint? NodeValue,
    uint? SavedReceiverPointer,
    bool? DiagnosticReturnedNormally,
    uint? PoolCounterBeforeXadd,
    bool? PoolPushReturnedNormally);

public enum SocketPayloadRecycleTailCallOutcome
{
    PathUndetermined,
    NotReached,
    MissingReturnCapture,
    DidNotReturnNormally,
    ReturnedNormally
}

public enum SocketPayloadRecycleTailCounterOutcome
{
    PathUndetermined,
    NotReached,
    MissingValueCapture,
    Captured
}

public enum SocketPayloadRecycleTailReceiverOutcome
{
    PathUndetermined,
    NotReached,
    MissingCapture,
    Captured
}

public enum SocketPayloadRecycleTailActionKind
{
    WriteNodeTag,
    TestNodeForNull,
    BranchAroundDiagnostic,
    PushDiagnosticNode,
    PushDiagnosticLine,
    PushDiagnosticArgumentOne,
    PushDiagnosticArgumentTwo,
    CallDiagnostic,
    DiagnosticStackCleanup,
    ReloadSavedReceiver,
    FormPoolCounterAddress,
    AtomicSubtractPoolCounter,
    PushNodeForPoolHelper,
    CallPoolPushHelper,
    WrapperEpilogue
}

public readonly record struct SocketPayloadRecycleTailAction(
    SocketPayloadRecycleTailActionKind Kind,
    uint RawFileOffset,
    uint? CapturedValue = null,
    uint? SecondaryCapturedValue = null,
    bool? ConditionMet = null,
    bool? CallReturnedNormally = null,
    uint? BranchTargetRawFileOffset = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null);

public readonly record struct SocketPayloadRecycleTailPlan(
    SocketPayloadRecycleTailCapture Capture,
    SocketPayloadRecycleTailCallOutcome DiagnosticOutcome,
    SocketPayloadRecycleTailReceiverOutcome ReceiverOutcome,
    SocketPayloadRecycleTailCounterOutcome CounterOutcome,
    SocketPayloadRecycleTailCallOutcome PoolPushOutcome,
    ImmutableArray<SocketPayloadRecycleTailAction> Actions,
    uint? NodeTagAddress,
    uint? PoolCounterAddress,
    uint? PoolCounterOldValue,
    uint? PoolCounterValueAfterSubtract,
    uint? PoolPushReceiverPointer,
    uint? PoolPushNodeArgument,
    bool WrapperEpilogueReached,
    uint StopAtRawFileOffsetExclusive);

public readonly record struct SocketPayloadRecycleTailInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? TargetRawFileOffset = null,
    uint? TargetVirtualAddress = null,
    uint? NormalReturnRawFileOffset = null);

/// <summary>
/// Projects only the late recycle-wrapper tail. Its entry assumes all earlier
/// opaque refcount calls have completed; it does not model those calls or the
/// helper's effects. The wrapper's prefix already read node+0x10, and this
/// tail writes node+8 before its late null test, so the diagnostic cannot
/// validate that earlier access or establish ownership or freeing.
/// </summary>
public static class SocketPayloadRecycleTailProjection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint WrapperEntryRawFileOffset = 0x00BD5C60;
    public const uint WrapperEntryVirtualAddress = 0x00FD6860;
    public const uint TailStartRawFileOffset = 0x00BD5C9C;
    public const uint TailStartVirtualAddress = 0x00FD689C;
    public const uint TailEndRawFileOffsetExclusive = 0x00BD5CD9;
    public const uint TailEndVirtualAddressExclusive = 0x00FD68D9;

    public const uint NodeTagWriteRawFileOffset = 0x00BD5C9C;
    public const uint NodeNullTestRawFileOffset = 0x00BD5CA3;
    public const uint DiagnosticBranchRawFileOffset = 0x00BD5CA5;
    public const uint DiagnosticBranchTargetRawFileOffset = 0x00BD5CC0;
    public const uint DiagnosticNodePushRawFileOffset = 0x00BD5CA7;
    public const uint DiagnosticLinePushRawFileOffset = 0x00BD5CA8;
    public const uint DiagnosticArgumentOnePushRawFileOffset = 0x00BD5CAD;
    public const uint DiagnosticArgumentTwoPushRawFileOffset = 0x00BD5CB2;
    public const uint DiagnosticCallRawFileOffset = 0x00BD5CB7;
    public const uint DiagnosticCallNormalReturnRawFileOffset = 0x00BD5CBD;
    public const uint DiagnosticStackCleanupRawFileOffset = 0x00BD5CBD;
    public const uint SavedReceiverReloadRawFileOffset = 0x00BD5CC0;
    public const uint PoolCounterAddressRawFileOffset = 0x00BD5CC3;
    public const uint PoolCounterXaddRawFileOffset = 0x00BD5CC6;
    public const uint PoolHelperNodePushRawFileOffset = 0x00BD5CCA;
    public const uint PoolPushCallRawFileOffset = 0x00BD5CCB;
    public const uint PoolPushCallNormalReturnRawFileOffset = 0x00BD5CD0;
    public const uint WrapperEpilogueRawFileOffset = 0x00BD5CD0;
    public const uint WrapperReturnRawFileOffset = 0x00BD5CD6;

    public const uint NodeTagValue = 0x01C5FC14;
    public const uint NodeTagOffset = 8;
    public const uint PoolCounterOffset = 0x10;
    public const uint PoolCounterXaddSource = 0xFFFFFFFF;
    public const uint DiagnosticLineValue = 0x25A;
    public const uint DiagnosticArgumentOneVirtualAddress = 0x01B74393;
    public const uint DiagnosticArgumentTwoVirtualAddress = 0x01B7436F;
    public const uint DiagnosticFunctionPointerCellVirtualAddress = 0x01C15EA0;
    public const uint PoolPushHelperRawFileOffset = 0x012CDA60;
    public const uint PoolPushHelperVirtualAddress = 0x016CE660;
    public const uint WrapperReturnStackBytes = 4;

    public const string CapturedTailBytes =
        "C7 43 08 14 FC C5 01 85 DB 75 19 53 68 5A 02 00 00 "
            + "68 93 43 B7 01 68 6F 43 B7 01 FF 15 A0 5E C1 01 "
            + "83 C4 10 8B 4D FC 8D 41 10 F0 0F C1 38 53 "
            + "E8 90 7D 6F 00 5F 5E 5B 8B E5 5D C2 04 00";

    public static IReadOnlyList<SocketPayloadRecycleTailInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new SocketPayloadRecycleTailInstructionEvidence(
                "Write the fixed tag through EBX+8 before testing EBX for null",
                NodeTagWriteRawFileOffset,
                "C7 43 08 14 FC C5 01"),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Test the node value after the tag write",
                NodeNullTestRawFileOffset,
                "85 DB"),
            new SocketPayloadRecycleTailInstructionEvidence(
                "A non-null node skips the late diagnostic and continues at the saved-receiver reload",
                DiagnosticBranchRawFileOffset,
                "75 19",
                DiagnosticBranchTargetRawFileOffset,
                TargetVirtualAddress:
                    DiagnosticBranchTargetRawFileOffset
                    + CodeVirtualAddressDeltaFromRawFileOffset),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Push EBX as the first value in the captured diagnostic push sequence",
                DiagnosticNodePushRawFileOffset,
                "53"),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Push the diagnostic line value 0x25A",
                DiagnosticLinePushRawFileOffset,
                "68 5A 02 00 00"),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Push diagnostic address 0x1B74393",
                DiagnosticArgumentOnePushRawFileOffset,
                "68 93 43 B7 01",
                TargetVirtualAddress: DiagnosticArgumentOneVirtualAddress),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Push diagnostic address 0x1B7436F",
                DiagnosticArgumentTwoPushRawFileOffset,
                "68 6F 43 B7 01",
                TargetVirtualAddress: DiagnosticArgumentTwoVirtualAddress),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Call through the diagnostic function-pointer cell; this tail continues only on normal return",
                DiagnosticCallRawFileOffset,
                "FF 15 A0 5E C1 01",
                TargetVirtualAddress: DiagnosticFunctionPointerCellVirtualAddress,
                NormalReturnRawFileOffset: DiagnosticCallNormalReturnRawFileOffset),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Clean up the diagnostic arguments after normal return",
                DiagnosticStackCleanupRawFileOffset,
                "83 C4 10"),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Reload the saved receiver independently from EBP-4",
                SavedReceiverReloadRawFileOffset,
                "8B 4D FC"),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Form the pool-counter address from the reloaded receiver plus 0x10",
                PoolCounterAddressRawFileOffset,
                "8D 41 10"),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Atomically add EDI=-1 to the counter; XADD's old value in EDI is unused",
                PoolCounterXaddRawFileOffset,
                "F0 0F C1 38"),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Push EBX as the stack argument to the pool-push helper",
                PoolHelperNodePushRawFileOffset,
                "53"),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Call the pool-push helper and continue at the register-restoring epilogue only on normal return",
                PoolPushCallRawFileOffset,
                "E8 90 7D 6F 00",
                TargetRawFileOffset: PoolPushHelperRawFileOffset,
                TargetVirtualAddress: PoolPushHelperVirtualAddress,
                NormalReturnRawFileOffset: PoolPushCallNormalReturnRawFileOffset),
            new SocketPayloadRecycleTailInstructionEvidence(
                "Restore saved registers and return with RET 4",
                WrapperEpilogueRawFileOffset,
                "5F 5E 5B 8B E5 5D C2 04 00")
        });

    public static uint AddNodeOffset(uint nodeValue, uint offset) =>
        unchecked(nodeValue + offset);

    public static uint NodeTagAddress(uint nodeValue) =>
        AddNodeOffset(nodeValue, NodeTagOffset);

    public static uint PoolCounterAddress(uint savedReceiverPointer) =>
        AddNodeOffset(savedReceiverPointer, PoolCounterOffset);

    /// <summary>
    /// Requires captures to agree with the reached control-flow path. A null
    /// or missing normal-return capture stops projection at the corresponding
    /// call boundary; the atomic counter update wraps as a 32-bit unsigned
    /// decrement, and the XADD old value is captured but otherwise unused.
    /// </summary>
    public static SocketPayloadRecycleTailPlan Project(
        SocketPayloadRecycleTailCapture capture)
    {
        var actions = ImmutableArray.CreateBuilder<SocketPayloadRecycleTailAction>(16);
        actions.Add(new SocketPayloadRecycleTailAction(
            SocketPayloadRecycleTailActionKind.WriteNodeTag,
            NodeTagWriteRawFileOffset,
            CapturedValue: NodeTagValue,
            SecondaryCapturedValue: capture.NodeValue is uint nodeValue
                ? NodeTagAddress(nodeValue)
                : null));

        if (capture.NodeValue is not uint capturedNodeValue)
        {
            Reject(capture.SavedReceiverPointer is null
                && capture.DiagnosticReturnedNormally is null
                && capture.PoolCounterBeforeXadd is null
                && capture.PoolPushReturnedNormally is null,
                nameof(capture.NodeValue),
                "The projection stops at the tag write when the node value is unavailable.");
            return Stop(
                capture,
                SocketPayloadRecycleTailCallOutcome.PathUndetermined,
                SocketPayloadRecycleTailReceiverOutcome.PathUndetermined,
                SocketPayloadRecycleTailCounterOutcome.PathUndetermined,
                SocketPayloadRecycleTailCallOutcome.PathUndetermined,
                actions,
                nodeTagAddress: null,
                poolCounterAddress: null,
                poolCounterOldValue: null,
                poolCounterValueAfterSubtract: null,
                poolPushReceiverPointer: null,
                poolPushNodeArgument: null,
                wrapperEpilogueReached: false,
                stopAtRawFileOffsetExclusive: NodeNullTestRawFileOffset);
        }

        actions.Add(new SocketPayloadRecycleTailAction(
            SocketPayloadRecycleTailActionKind.TestNodeForNull,
            NodeNullTestRawFileOffset,
            CapturedValue: capturedNodeValue));

        bool isNullNode = capturedNodeValue == 0;
        SocketPayloadRecycleTailCallOutcome diagnosticOutcome;
        if (!isNullNode)
        {
            Reject(capture.DiagnosticReturnedNormally is null,
                nameof(capture.DiagnosticReturnedNormally),
                "The diagnostic call is skipped on the non-null-node branch.");
            actions.Add(new SocketPayloadRecycleTailAction(
                SocketPayloadRecycleTailActionKind.BranchAroundDiagnostic,
                DiagnosticBranchRawFileOffset,
                ConditionMet: true,
                BranchTargetRawFileOffset: DiagnosticBranchTargetRawFileOffset,
                TargetVirtualAddress: DiagnosticBranchTargetRawFileOffset
                    + CodeVirtualAddressDeltaFromRawFileOffset));
            diagnosticOutcome = SocketPayloadRecycleTailCallOutcome.NotReached;
        }
        else
        {
            actions.Add(new SocketPayloadRecycleTailAction(
                SocketPayloadRecycleTailActionKind.BranchAroundDiagnostic,
                DiagnosticBranchRawFileOffset,
                ConditionMet: false));
            actions.Add(new SocketPayloadRecycleTailAction(
                SocketPayloadRecycleTailActionKind.PushDiagnosticNode,
                DiagnosticNodePushRawFileOffset,
                CapturedValue: capturedNodeValue));
            actions.Add(new SocketPayloadRecycleTailAction(
                SocketPayloadRecycleTailActionKind.PushDiagnosticLine,
                DiagnosticLinePushRawFileOffset,
                CapturedValue: DiagnosticLineValue));
            actions.Add(new SocketPayloadRecycleTailAction(
                SocketPayloadRecycleTailActionKind.PushDiagnosticArgumentOne,
                DiagnosticArgumentOnePushRawFileOffset,
                CapturedValue: DiagnosticArgumentOneVirtualAddress));
            actions.Add(new SocketPayloadRecycleTailAction(
                SocketPayloadRecycleTailActionKind.PushDiagnosticArgumentTwo,
                DiagnosticArgumentTwoPushRawFileOffset,
                CapturedValue: DiagnosticArgumentTwoVirtualAddress));
            actions.Add(new SocketPayloadRecycleTailAction(
                SocketPayloadRecycleTailActionKind.CallDiagnostic,
                DiagnosticCallRawFileOffset,
                CapturedValue: DiagnosticFunctionPointerCellVirtualAddress,
                CallReturnedNormally: capture.DiagnosticReturnedNormally,
                TargetVirtualAddress: DiagnosticFunctionPointerCellVirtualAddress));

            diagnosticOutcome = capture.DiagnosticReturnedNormally switch
            {
                null => SocketPayloadRecycleTailCallOutcome.MissingReturnCapture,
                false => SocketPayloadRecycleTailCallOutcome.DidNotReturnNormally,
                true => SocketPayloadRecycleTailCallOutcome.ReturnedNormally
            };
            if (capture.DiagnosticReturnedNormally is not true)
            {
                Reject(capture.SavedReceiverPointer is null
                    && capture.PoolCounterBeforeXadd is null
                    && capture.PoolPushReturnedNormally is null,
                    nameof(capture.DiagnosticReturnedNormally),
                    "The saved-receiver reload and all later operations are unreachable unless diagnostics return normally.");
                return Stop(
                    capture,
                    diagnosticOutcome,
                    SocketPayloadRecycleTailReceiverOutcome.NotReached,
                    SocketPayloadRecycleTailCounterOutcome.NotReached,
                    SocketPayloadRecycleTailCallOutcome.NotReached,
                    actions,
                    NodeTagAddress(capturedNodeValue),
                    poolCounterAddress: null,
                    poolCounterOldValue: null,
                    poolCounterValueAfterSubtract: null,
                    poolPushReceiverPointer: null,
                    poolPushNodeArgument: null,
                    wrapperEpilogueReached: false,
                    stopAtRawFileOffsetExclusive: DiagnosticCallNormalReturnRawFileOffset);
            }

            actions.Add(new SocketPayloadRecycleTailAction(
                SocketPayloadRecycleTailActionKind.DiagnosticStackCleanup,
                DiagnosticStackCleanupRawFileOffset));
        }

        actions.Add(new SocketPayloadRecycleTailAction(
            SocketPayloadRecycleTailActionKind.ReloadSavedReceiver,
            SavedReceiverReloadRawFileOffset,
            CapturedValue: capture.SavedReceiverPointer));

        if (capture.SavedReceiverPointer is not uint receiver)
        {
            Reject(capture.PoolCounterBeforeXadd is null
                && capture.PoolPushReturnedNormally is null,
                nameof(capture.SavedReceiverPointer),
                "The counter XADD and pool-push helper are unreachable without the saved receiver reload capture.");
            return Stop(
                capture,
                diagnosticOutcome,
                SocketPayloadRecycleTailReceiverOutcome.MissingCapture,
                SocketPayloadRecycleTailCounterOutcome.PathUndetermined,
                SocketPayloadRecycleTailCallOutcome.PathUndetermined,
                actions,
                NodeTagAddress(capturedNodeValue),
                poolCounterAddress: null,
                poolCounterOldValue: null,
                poolCounterValueAfterSubtract: null,
                poolPushReceiverPointer: null,
                poolPushNodeArgument: null,
                wrapperEpilogueReached: false,
                stopAtRawFileOffsetExclusive: PoolCounterAddressRawFileOffset);
        }

        uint counterAddress = PoolCounterAddress(receiver);
        actions.Add(new SocketPayloadRecycleTailAction(
            SocketPayloadRecycleTailActionKind.FormPoolCounterAddress,
            PoolCounterAddressRawFileOffset,
            CapturedValue: counterAddress));

        uint? counterOldValue = capture.PoolCounterBeforeXadd;
        uint? counterAfterSubtract = counterOldValue is uint oldCounter
            ? unchecked(oldCounter + PoolCounterXaddSource)
            : null;
        actions.Add(new SocketPayloadRecycleTailAction(
            SocketPayloadRecycleTailActionKind.AtomicSubtractPoolCounter,
            PoolCounterXaddRawFileOffset,
            CapturedValue: counterOldValue,
            SecondaryCapturedValue: counterAfterSubtract));

        if (counterOldValue is null)
        {
            Reject(capture.PoolPushReturnedNormally is null,
                nameof(capture.PoolCounterBeforeXadd),
                "The pool-push helper is not reached until after the counter XADD capture.");
            return Stop(
                capture,
                diagnosticOutcome,
                SocketPayloadRecycleTailReceiverOutcome.Captured,
                SocketPayloadRecycleTailCounterOutcome.MissingValueCapture,
                SocketPayloadRecycleTailCallOutcome.PathUndetermined,
                actions,
                NodeTagAddress(capturedNodeValue),
                counterAddress,
                poolCounterOldValue: null,
                poolCounterValueAfterSubtract: null,
                poolPushReceiverPointer: null,
                poolPushNodeArgument: null,
                wrapperEpilogueReached: false,
                stopAtRawFileOffsetExclusive: PoolHelperNodePushRawFileOffset);
        }

        actions.Add(new SocketPayloadRecycleTailAction(
            SocketPayloadRecycleTailActionKind.PushNodeForPoolHelper,
            PoolHelperNodePushRawFileOffset,
            CapturedValue: capturedNodeValue));
        actions.Add(new SocketPayloadRecycleTailAction(
            SocketPayloadRecycleTailActionKind.CallPoolPushHelper,
            PoolPushCallRawFileOffset,
            CapturedValue: PoolPushHelperRawFileOffset,
            CallReturnedNormally: capture.PoolPushReturnedNormally,
            TargetRawFileOffset: PoolPushHelperRawFileOffset,
            TargetVirtualAddress: PoolPushHelperVirtualAddress));

        SocketPayloadRecycleTailCallOutcome poolPushOutcome =
            capture.PoolPushReturnedNormally switch
            {
                null => SocketPayloadRecycleTailCallOutcome.MissingReturnCapture,
                false => SocketPayloadRecycleTailCallOutcome.DidNotReturnNormally,
                true => SocketPayloadRecycleTailCallOutcome.ReturnedNormally
            };
        bool epilogueReached = capture.PoolPushReturnedNormally is true;
        if (epilogueReached)
        {
            actions.Add(new SocketPayloadRecycleTailAction(
                SocketPayloadRecycleTailActionKind.WrapperEpilogue,
                WrapperEpilogueRawFileOffset));
        }

        return Stop(
            capture,
            diagnosticOutcome,
            SocketPayloadRecycleTailReceiverOutcome.Captured,
            SocketPayloadRecycleTailCounterOutcome.Captured,
            poolPushOutcome,
            actions,
            NodeTagAddress(capturedNodeValue),
            counterAddress,
            counterOldValue,
            counterAfterSubtract,
            capture.SavedReceiverPointer,
            capturedNodeValue,
            epilogueReached,
            epilogueReached
                ? TailEndRawFileOffsetExclusive
                : PoolPushCallNormalReturnRawFileOffset);
    }

    private static SocketPayloadRecycleTailPlan Stop(
        SocketPayloadRecycleTailCapture capture,
        SocketPayloadRecycleTailCallOutcome diagnosticOutcome,
        SocketPayloadRecycleTailReceiverOutcome receiverOutcome,
        SocketPayloadRecycleTailCounterOutcome counterOutcome,
        SocketPayloadRecycleTailCallOutcome poolPushOutcome,
        ImmutableArray<SocketPayloadRecycleTailAction>.Builder actions,
        uint? nodeTagAddress,
        uint? poolCounterAddress,
        uint? poolCounterOldValue,
        uint? poolCounterValueAfterSubtract,
        uint? poolPushReceiverPointer,
        uint? poolPushNodeArgument,
        bool wrapperEpilogueReached,
        uint stopAtRawFileOffsetExclusive) =>
        new(
            capture,
            diagnosticOutcome,
            receiverOutcome,
            counterOutcome,
            poolPushOutcome,
            actions.ToImmutable(),
            nodeTagAddress,
            poolCounterAddress,
            poolCounterOldValue,
            poolCounterValueAfterSubtract,
            poolPushReceiverPointer,
            poolPushNodeArgument,
            wrapperEpilogueReached,
            stopAtRawFileOffsetExclusive);

    private static void Reject(bool condition, string parameterName, string message)
    {
        if (!condition)
        {
            throw new ArgumentException(message, parameterName);
        }
    }
}
