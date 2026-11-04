using Firefall.Reconstruction.Startup;

internal static class ContextQueueAppendChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Context queue append evidence pins the sample, byte range, and PE-derived VA mapping",
            ContextQueueAppendProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && ContextQueueAppendProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && ContextQueueAppendProjection.IdaAddressMapping
                == "Base-zero IDA EAs are raw file offsets; PE code VA is raw + 0x400C00, not RVA."
            && ContextQueueAppendProjection.ProjectionStartRawFileOffset == 0x00BD3456
            && ContextQueueAppendProjection.AppendCallRawFileOffset == 0x00BD346E
            && ContextQueueAppendProjection.AppendReturnBoundaryRawFileOffset == 0x00BD3473
            && ContextQueueAppendProjection.EvidenceEndRawFileOffsetExclusive == 0x00BD3477
            && ContextQueueAppendProjection.ProjectionStartVirtualAddress == 0x00FD4056
            && ContextQueueAppendProjection.AppendCallVirtualAddress == 0x00FD406E
            && ContextQueueAppendProjection.AppendReturnBoundaryVirtualAddress == 0x00FD4073
            && ContextQueueAppendProjection.EvidenceEndVirtualAddressExclusive == 0x00FD4077
            && ContextQueueAppendProjection.ProjectionStartRawFileOffset
                + ContextQueueAppendProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == ContextQueueAppendProjection.ProjectionStartVirtualAddress
            && ContextQueueAppendProjection.EvidenceEndRawFileOffsetExclusive
                + ContextQueueAppendProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == ContextQueueAppendProjection.EvidenceEndVirtualAddressExclusive
            && ContextQueueAppendProjection.AppendCalleeRawFileOffset == 0x12CE310
            && ContextQueueAppendProjection.AppendCalleeVirtualAddress == 0x16CEF10
            && ContextQueueAppendProjection.CapturedEvidenceBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x21
            && ContextQueueAppendProjection.CapturedEvidenceBytes
                == "8D 8E 60 02 00 00 2B 44 24 24 0F B7 C0 89 47 04 "
                    + "8B 44 24 28 57 89 47 18 E8 9D AE 6F 00 83 7D 0C 02");

        check("Instruction anchors preserve the ordered stores and stop before the following compare",
            ContextQueueAppendProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.TargetRawFileOffset,
                        instruction.TargetVirtualAddress))
                .SequenceEqual(new (
                    uint RawFileOffset,
                    string Bytes,
                    uint? TargetRawFileOffset,
                    uint? TargetVirtualAddress)[]
                {
                    (0x00BD3456, "8D 8E 60 02 00 00", null, null),
                    (0x00BD345C, "2B 44 24 24", null, null),
                    (0x00BD3460, "0F B7 C0", null, null),
                    (0x00BD3463, "89 47 04", null, null),
                    (0x00BD3466, "8B 44 24 28", null, null),
                    (0x00BD346A, "57", null, null),
                    (0x00BD346B, "89 47 18", null, null),
                    (0x00BD346E, "E8 9D AE 6F 00", 0x12CE310, 0x16CEF10),
                    (0x00BD3473, "83 7D 0C 02", null, null)
                })
            && ContextQueueAppendProjection.AppendCallRawFileOffset + 5 + 0x006FAE9D
                == ContextQueueAppendProjection.AppendCalleeRawFileOffset);

        var missingCursor = ContextQueueAppendProjection.Project(new(
            CapturedThisPointer: 0x1000,
            CapturedCursorEax: null,
            CapturedInitialPointerAtEsp24: null,
            CapturedPayloadPointerEdi: null,
            CapturedValueAtEsp28: null,
            AppendReturnEax: null));
        var missingInitialPointer = ContextQueueAppendProjection.Project(new(
            CapturedThisPointer: 0x1000,
            CapturedCursorEax: 0x2345,
            CapturedInitialPointerAtEsp24: null,
            CapturedPayloadPointerEdi: null,
            CapturedValueAtEsp28: null,
            AppendReturnEax: null));
        check("Missing cursor or initial-pointer captures stop before later work",
            missingCursor.Outcome == ContextQueueAppendOutcome.AwaitingCursorCapture
            && missingCursor.ReceiverAddress == 0x1260
            && missingCursor.Actions[^1].RawFileOffset == 0x00BD345C
            && missingInitialPointer.Outcome
                == ContextQueueAppendOutcome.AwaitingInitialPointerCapture
            && missingInitialPointer.Actions[^1].RawFileOffset == 0x00BD345C
            && ThrowsArgumentException(() => ContextQueueAppendProjection.Project(new(
                0x1000, null, 0x2000, 0x3000, 0x4000, 0x5000)))
            && ThrowsArgumentException(() => ContextQueueAppendProjection.Project(new(
                0x1000, 0x2345, null, 0x3000, 0x4000, 0x5000))));

        var wrappingDelta = ContextQueueAppendProjection.Project(new(
            CapturedThisPointer: 0xFFFFFEA0,
            CapturedCursorEax: 2,
            CapturedInitialPointerAtEsp24: 0xFFFFFFFE,
            CapturedPayloadPointerEdi: 0xFFFFFFFC,
            CapturedValueAtEsp28: 0xA1B2C3D4,
            AppendReturnEax: 0));
        var highBitsDiscarded = ContextQueueAppendProjection.Project(new(
            CapturedThisPointer: 0,
            CapturedCursorEax: 0x1234ABCD,
            CapturedInitialPointerAtEsp24: 0,
            CapturedPayloadPointerEdi: 0x1000,
            CapturedValueAtEsp28: 7,
            AppendReturnEax: 0xFFFFFFFF));
        check("DWORD subtraction wraps before MOVZX and its upper 16 bits are discarded",
            wrappingDelta.Outcome == ContextQueueAppendOutcome.AppendNormalReturnBoundary
            && wrappingDelta.ReceiverAddress == 0x100
            && wrappingDelta.WrappedDelta == 4
            && wrappingDelta.StoredDelta == 4
            && wrappingDelta.Actions.Any(action =>
                action.Kind == ContextQueueAppendActionKind.AttemptDeltaStore
                && action.Address == 0
                && action.Value == 4)
            && highBitsDiscarded.WrappedDelta == 0x1234ABCD
            && highBitsDiscarded.StoredDelta == 0xABCD
            && highBitsDiscarded.Actions.Any(action =>
                action.Kind == ContextQueueAppendActionKind.AttemptDeltaStore
                && action.Value == 0xABCD));

        var zeroPayload = ContextQueueAppendProjection.Project(new(
            CapturedThisPointer: 0xFFFFFFFF,
            CapturedCursorEax: 0x10000,
            CapturedInitialPointerAtEsp24: 0,
            CapturedPayloadPointerEdi: 0,
            CapturedValueAtEsp28: 0x11223344,
            AppendReturnEax: null));
        check("Zero payload still attempts both stores and is pushed unchanged before the opaque call",
            zeroPayload.Outcome == ContextQueueAppendOutcome.AwaitingAppendReturnCapture
            && zeroPayload.ReceiverAddress == 0x25F
            && zeroPayload.Actions.Any(action =>
                action.Kind == ContextQueueAppendActionKind.AttemptDeltaStore
                && action.Address == 4)
            && zeroPayload.Actions.Any(action =>
                action.Kind == ContextQueueAppendActionKind.PushPayloadPointerEdi
                && action.Value == 0)
            && zeroPayload.Actions.Any(action =>
                action.Kind == ContextQueueAppendActionKind.AttemptValueStore
                && action.Address == 0x18
                && action.Value == 0x11223344)
            && zeroPayload.Actions[^2].Kind == ContextQueueAppendActionKind.CallAppend
            && zeroPayload.Actions[^2].CallReceiverAddress == 0x25F
            && zeroPayload.Actions[^1].Kind
                == ContextQueueAppendActionKind.StopAtAppendReturnCaptureBoundary
            && zeroPayload.Actions[^1].RawFileOffset == 0x00BD3473
            && ThrowsArgumentException(() => ContextQueueAppendProjection.Project(new(
                0xFFFFFFFF, 0x10000, 0, 0, null, 0x99))));

        var returnedZero = ContextQueueAppendProjection.Project(new(
            CapturedThisPointer: 0x1000,
            CapturedCursorEax: 0x12345678,
            CapturedInitialPointerAtEsp24: 0x12340000,
            CapturedPayloadPointerEdi: 0x2000,
            CapturedValueAtEsp28: 0x55667788,
            AppendReturnEax: 0));
        check("A captured EAX of zero is returned as opaque data and stops at normal return",
            returnedZero.Outcome == ContextQueueAppendOutcome.AppendNormalReturnBoundary
            && returnedZero.AppendReturnEax == 0
            && returnedZero.Actions.Select(action => action.Kind).SequenceEqual(new[]
            {
                ContextQueueAppendActionKind.CaptureThisPointer,
                ContextQueueAppendActionKind.ComputeReceiver,
                ContextQueueAppendActionKind.CaptureCursorEax,
                ContextQueueAppendActionKind.CaptureInitialPointerAtEsp24,
                ContextQueueAppendActionKind.SubtractInitialPointerFromCursor,
                ContextQueueAppendActionKind.ZeroExtendLowWord,
                ContextQueueAppendActionKind.CapturePayloadPointerEdi,
                ContextQueueAppendActionKind.AttemptDeltaStore,
                ContextQueueAppendActionKind.CaptureValueAtEsp28,
                ContextQueueAppendActionKind.PushPayloadPointerEdi,
                ContextQueueAppendActionKind.AttemptValueStore,
                ContextQueueAppendActionKind.CallAppend,
                ContextQueueAppendActionKind.CaptureAppendReturnEax,
                ContextQueueAppendActionKind.StopAtAppendNormalReturnBoundary
            })
            && returnedZero.Actions[^3].CallReceiverAddress == 0x1260
            && returnedZero.Actions[^3].Address == 0x2000
            && returnedZero.Actions[^2]
                == new ContextQueueAppendAction(
                    ContextQueueAppendActionKind.CaptureAppendReturnEax,
                    0x00BD3473,
                    Value: 0)
            && returnedZero.Actions[^1]
                == new ContextQueueAppendAction(
                    ContextQueueAppendActionKind.StopAtAppendNormalReturnBoundary,
                    0x00BD3473,
                    Value: 0)
            && !returnedZero.Actions.Any(action =>
                action.RawFileOffset >= ContextQueueAppendProjection.AppendReturnBoundaryRawFileOffset
                && action.Kind != ContextQueueAppendActionKind.CaptureAppendReturnEax
                && action.Kind != ContextQueueAppendActionKind.StopAtAppendNormalReturnBoundary));
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
