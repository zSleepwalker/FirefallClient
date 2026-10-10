using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public enum NativeProfilerRequestTarget { Firefall = 0, Matrix = 1 }

public enum NativeProfilerRequestOutcome
{
    IgnoredOutsideMode2,
    InvalidSelectorAssert,
    ObserverAbsent,
    ObserverDisabled,
    ObserverLevelTooLow,
    Selected
}

public sealed record NativeProfilerRequestSelection(
    NativeProfilerRequestOutcome Outcome, NativeProfilerRequestTarget? Target,
    int? MessageCode, int? TransportChannel, bool LogSelected,
    ulong? StampedRoutingKey, ulong? TickScaledBy1000);

public static class NativeProfilerNameRequests
{
    public const string OriginalEndpointClass = "tfServerProfiler";
    public const uint EndpointMethodFileOffset = 0x6AAB20;
    public const uint EndpointVtableVirtualAddress = 0x1C61B50;
    public const uint MatrixGateFileOffset = 0x6AADC0;
    public const uint FirefallGateFileOffset = 0x6AAD20;
    public const int ObserverEnableOffset = 0x820;
    public const int ObserverRoutingLowOffset = 0x878;
    public const int ObserverRoutingHighOffset = 0x87C;
    public const int ObserverScaledTickOffset = 0xB0;

    /// <summary>Selection only. Native matrix rejection has no defined return value; no transport/log/timestamp side effects are executed.</summary>
    public static NativeProfilerRequestSelection Select(
        int sampledApplicationMode, int selector, bool observerPresent, bool observerEnabled,
        int signedObserverLevel, byte logFlags, uint sampledTick, ulong sampledRoutingKey)
    {
        if (sampledApplicationMode != 2)
            return Rejected(NativeProfilerRequestOutcome.IgnoredOutsideMode2);
        if (selector is not (0 or 1))
            return Rejected(NativeProfilerRequestOutcome.InvalidSelectorAssert);
        if (!observerPresent)
            return Rejected(NativeProfilerRequestOutcome.ObserverAbsent);
        if (!observerEnabled)
            return Rejected(NativeProfilerRequestOutcome.ObserverDisabled);
        var target = (NativeProfilerRequestTarget)selector;
        if (signedObserverLevel <= (target == NativeProfilerRequestTarget.Matrix ? 3 : 4))
            return Rejected(NativeProfilerRequestOutcome.ObserverLevelTooLow);
        return target == NativeProfilerRequestTarget.Matrix
            ? new(NativeProfilerRequestOutcome.Selected, target, 30, 1, (logFlags & 1) != 0, null, null)
            : new(NativeProfilerRequestOutcome.Selected, target, 18, 2, (logFlags & 2) != 0,
                sampledRoutingKey, 1000UL * sampledTick);
    }

    private static NativeProfilerRequestSelection Rejected(NativeProfilerRequestOutcome outcome) =>
        new(outcome, null, null, null, false, null, null);
}

public sealed record NativeProfilerNameRequestPayload(
    IReadOnlyList<ushort> RootIds, IReadOnlyList<ushort> NodeIds);

public enum NativeProfilerRequestReadOutcome
{
    Accepted,
    Truncated,
    NonCanonicalHeader,
    UnexpectedMessageCode
}

public sealed record NativeProfilerRequestReadResult(
    NativeProfilerRequestReadOutcome Outcome, NativeProfilerNameRequestPayload? Payload,
    ulong? FirefallRoutingKey, int BytesConsumed);

/// <summary>Normal application buffers: Matrix30 on channel1, or raw QWORD routing key + Firefall18 on channel2. No socket framing.</summary>
public static class NativeProfilerNameRequestWire
{
    public const string OriginalMatrixClass = "matrix_fury::ServerProfiler_RequestNames";
    public const string OriginalFirefallClass = "firefall::ServerProfiler_RequestNames";
    public const uint MatrixEncoderFileOffset = 0x6AB080;
    public const uint FirefallEncoderFileOffset = 0x6AB190;
    public const uint WordListWriterFileOffset = 0x349030;
    public const uint MatrixConstructorFileOffset = 0x339320;
    public const uint FirefallConstructorFileOffset = 0x812820;

    public static byte[] EncodeMatrix(NativeProfilerNameRequestPayload payload) => Encode(payload, null);
    public static byte[] EncodeFirefall(NativeProfilerNameRequestPayload payload, ulong rawRoutingKey) =>
        Encode(payload, rawRoutingKey);

    private static byte[] Encode(NativeProfilerNameRequestPayload payload, ulong? routingKey)
    {
        ArgumentNullException.ThrowIfNull(payload);
        using var output = new MemoryStream();
        if (routingKey is ulong key)
        {
            Span<byte> bytes = stackalloc byte[8];
            BinaryPrimitives.WriteUInt64LittleEndian(bytes, key);
            output.Write(bytes);
        }
        output.Write(CompactPositiveUInt16Codec.Encode(routingKey.HasValue ? 18u : 30u));
        WriteIds(output, payload.RootIds);
        WriteIds(output, payload.NodeIds);
        return output.ToArray();
    }

    public static NativeProfilerRequestReadResult Decode(ReadOnlySpan<byte> bytes, NativeProfilerRequestTarget target)
    {
        if (target is not (NativeProfilerRequestTarget.Matrix or NativeProfilerRequestTarget.Firefall))
            throw new ArgumentOutOfRangeException(nameof(target));
        int prefix = target == NativeProfilerRequestTarget.Firefall ? 8 : 0;
        if (bytes.Length < prefix)
            return new(NativeProfilerRequestReadOutcome.Truncated, null, null, 0);
        ulong? key = prefix == 0 ? null : BinaryPrimitives.ReadUInt64LittleEndian(bytes);
        var header = CompactPositiveUInt16Codec.Decode(bytes[prefix..]);
        if (header.Outcome == CompactPositiveUInt16Outcome.Truncated)
            return new(NativeProfilerRequestReadOutcome.Truncated, null, key, 0);
        if (header.Outcome == CompactPositiveUInt16Outcome.NonCanonicalExtendedValue)
            return new(NativeProfilerRequestReadOutcome.NonCanonicalHeader, null, key, 0);
        if (header.Value != (target == NativeProfilerRequestTarget.Matrix ? 30u : 18u))
            return new(NativeProfilerRequestReadOutcome.UnexpectedMessageCode, null, key, 0);
        var reader = new MatrixMessageSpanReader(bytes[(prefix + header.BytesConsumed)..]);
        if (!MatrixChunkedListWire.TryRead<ushort>(ref reader, ReadWord, out var roots)
            || !MatrixChunkedListWire.TryRead<ushort>(ref reader, ReadWord, out var nodes))
            return new(NativeProfilerRequestReadOutcome.Truncated, null, key, 0);
        return new(NativeProfilerRequestReadOutcome.Accepted, new(roots, nodes), key,
            prefix + header.BytesConsumed + reader.Position);
    }

    private static bool ReadWord(ref MatrixMessageSpanReader reader, out ushort value) =>
        reader.TryReadUInt16(out value);

    private static void WriteIds(Stream output, IReadOnlyList<ushort> ids)
    {
        ArgumentNullException.ThrowIfNull(ids);
        int offset = 0;
        Span<byte> bytes = stackalloc byte[2];
        int count;
        do
        {
            count = Math.Min(ids.Count - offset, byte.MaxValue);
            output.WriteByte((byte)count);
            for (int i = 0; i < count; i++)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(bytes, ids[offset++]);
                output.Write(bytes);
            }
        } while (count == byte.MaxValue);
    }
}
