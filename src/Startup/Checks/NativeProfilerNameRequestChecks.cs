using Firefall.Reconstruction.Startup;

internal static class NativeProfilerNameRequestChecks
{
    public static void Run(Action<string, bool> check)
    {
        NativeProfilerRequestSelection Select(int mode = 2, int selector = 1, bool present = true,
            bool enabled = true, int level = 4, byte flags = 3) =>
            NativeProfilerNameRequests.Select(mode, selector, present, enabled, level, flags,
                uint.MaxValue, 0x0102030405060708);
        check("Profiler requester gates application mode before selector/assert and observer availability",
            Select(mode: 1, selector: 9).Outcome == NativeProfilerRequestOutcome.IgnoredOutsideMode2
            && Select(selector: 9, present: false).Outcome == NativeProfilerRequestOutcome.InvalidSelectorAssert
            && Select(present: false).Outcome == NativeProfilerRequestOutcome.ObserverAbsent
            && Select(enabled: false).Outcome == NativeProfilerRequestOutcome.ObserverDisabled);
        check("Profiler matrix30 gate requires signed level>3 and mask01 logging, with no route/timestamp stamp",
            Select(level: 3).Outcome == NativeProfilerRequestOutcome.ObserverLevelTooLow
            && Select(level: int.MinValue).Outcome == NativeProfilerRequestOutcome.ObserverLevelTooLow
            && Select(flags: 2) is { Outcome: NativeProfilerRequestOutcome.Selected, MessageCode: 30,
                TransportChannel: 1, LogSelected: false, StampedRoutingKey: null, TickScaledBy1000: null });
        check("Profiler firefall18 gate requires signed level>4 and mask02 logging, stamps raw routing pair and UInt32 tick times1000",
            Select(selector: 0).Outcome == NativeProfilerRequestOutcome.ObserverLevelTooLow
            && Select(selector: 0, level: 5, flags: 2) is { MessageCode: 18, TransportChannel: 2,
                LogSelected: true, StampedRoutingKey: 0x0102030405060708, TickScaledBy1000: 4294967295000 });
        var payload = new NativeProfilerNameRequestPayload(new ushort[] { 1, 0x1234, 1 }, new ushort[] { 65535 });
        byte[] matrix = NativeProfilerNameRequestWire.EncodeMatrix(payload);
        byte[] firefall = NativeProfilerNameRequestWire.EncodeFirefall(payload, 0x0102030405060708);
        check("Profiler request golden buffers preserve order/duplicates and separate matrix header from firefall raw-QWORD prefix",
            matrix.SequenceEqual(new byte[] { 30, 3, 1, 0, 0x34, 0x12, 1, 0, 1, 0xFF, 0xFF })
            && firefall.SequenceEqual(new byte[] { 8, 7, 6, 5, 4, 3, 2, 1, 18, 3, 1, 0, 0x34, 0x12, 1, 0, 1, 0xFF, 0xFF }));
        bool chunks = true;
        foreach (int count in new[] { 0, 1, 254, 255, 256, 510, 511 })
        {
            var roots = Enumerable.Range(0, count).Select(i => (ushort)i).ToArray();
            byte[] bytes = NativeProfilerNameRequestWire.EncodeMatrix(new(roots, []));
            var decoded = NativeProfilerNameRequestWire.Decode(bytes, NativeProfilerRequestTarget.Matrix);
            chunks &= decoded.Outcome == NativeProfilerRequestReadOutcome.Accepted
                && decoded.BytesConsumed == bytes.Length && decoded.Payload!.RootIds.SequenceEqual(roots)
                && decoded.Payload.NodeIds.Count == 0
                && bytes.Length == 1 + 2 * count + count / 255 + 2;
            if (count % 255 == 0)
                chunks &= bytes[^2] == 0;
        }
        check("Profiler request count chunks include zero terminators at255 multiples, including empty vectors", chunks);
        bool truncated = true;
        for (int i = 0; i < firefall.Length; i++)
            truncated &= NativeProfilerNameRequestWire.Decode(firefall.AsSpan(0, i), NativeProfilerRequestTarget.Firefall)
                is { Outcome: NativeProfilerRequestReadOutcome.Truncated, Payload: null };
        var decodedFirefall = NativeProfilerNameRequestWire.Decode(firefall.Concat(new byte[] { 99 }).ToArray(),
            NativeProfilerRequestTarget.Firefall);
        check("Profiler request decoder rejects every incomplete golden prefix and leaves transport/trailing bytes unconsumed",
            truncated && decodedFirefall.BytesConsumed == firefall.Length
            && decodedFirefall.FirefallRoutingKey == 0x0102030405060708
            && decodedFirefall.Payload!.RootIds.SequenceEqual(payload.RootIds));
        check("Profiler request normal buffer decoder rejects noncanonical and wrong message IDs",
            NativeProfilerNameRequestWire.Decode(new byte[] { 0, 30, 0, 0, 0 }, NativeProfilerRequestTarget.Matrix)
                .Outcome == NativeProfilerRequestReadOutcome.NonCanonicalHeader
            && NativeProfilerNameRequestWire.Decode(new byte[] { 18, 0, 0 }, NativeProfilerRequestTarget.Matrix)
                .Outcome == NativeProfilerRequestReadOutcome.UnexpectedMessageCode);
    }
}
