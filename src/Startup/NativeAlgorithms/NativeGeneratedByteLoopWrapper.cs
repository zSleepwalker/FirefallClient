namespace Firefall.Reconstruction.Startup;

public sealed record NativeGeneratedWrappedBody(
    ReadOnlyMemory<byte> Bytes, IReadOnlyList<uint> ConsumedUInt32Samples);

/// <summary>Default-header mode0 or explicit mode1 wrapper around captured body bytes. No native allocation, body generation or execution.</summary>
public static class NativeGeneratedByteLoopWrapper
{
    public const uint MethodFileOffset = 0x12868D0;
    public const int UInt32CallsConsumed = 24;
    public const int ModeOneUInt32CallsConsumed = 21;

    public static NativeGeneratedWrappedBody AssembleCapturedBody(ReadOnlySpan<byte> capturedBody, Func<uint> nextUInt32) =>
        AssembleDefaultHeader(capturedBody, nextUInt32, false);

    public static NativeGeneratedWrappedBody AssembleCapturedBodyForModeOne(ReadOnlySpan<byte> capturedBody, Func<uint> nextUInt32) =>
        AssembleDefaultHeader(capturedBody, nextUInt32, true);

    private static NativeGeneratedWrappedBody AssembleDefaultHeader(
        ReadOnlySpan<byte> capturedBody, Func<uint> nextUInt32, bool modeOne)
    {
        ArgumentNullException.ThrowIfNull(nextUInt32);
        byte[] body = capturedBody.ToArray();
        var samples = new List<uint>(modeOne ? ModeOneUInt32CallsConsumed : UInt32CallsConsumed);
        ReadOnlyMemory<byte> Select(IReadOnlyList<ReadOnlyMemory<byte>> candidates)
        {
            if (candidates.Count == 0)
                throw new InvalidOperationException($"Native candidate builder would diagnose {NativeGeneratedConditionalBranches.NoCandidatesDiagnostic}.");
            uint sample = nextUInt32();
            samples.Add(sample);
            return candidates[(int)(sample % (uint)candidates.Count)];
        }

        var catalog = modeOne ? NativeGeneratedBoundaryEncodingCatalog.ModeOneDefaultHeader()
            : NativeGeneratedBoundaryEncodingCatalog.InitialModeZero();
        var opening = catalog.Opening.Select(entry => Select(entry.Candidates)).ToArray();
        ReadOnlyMemory<byte> condition = Select(modeOne
            ? [new byte[] { 0x48, 0x09, 0xD2 }, new byte[] { 0x48, 0x0B, 0xD2 }]
            : [new byte[] { 0x09, 0xD2 }, new byte[] { 0x0B, 0xD2 }]);
        ReadOnlyMemory<byte> store = Select([new byte[] { 0x88, 0x18 }]);
        ReadOnlyMemory<byte> increment = Select(modeOne
            ? NativeGeneratedScratchTemplates.UnaryForModeOneOrdinaryRegister(NativeGeneratedOperationCode.Increment, 53)
            : [new byte[] { 0x40 }, new byte[] { 0xFF, 0xC0 }]);
        ReadOnlyMemory<byte> decrement = Select(modeOne
            ? NativeGeneratedScratchTemplates.UnaryForModeOneOrdinaryRegister(NativeGeneratedOperationCode.Decrement, 55)
            : [new byte[] { 0x4A }, new byte[] { 0xFF, 0xCA }]);
        int tailPrefixLength = checked(store.Length + increment.Length + decrement.Length);
        int backOffset = checked(-checked(body.Length + tailPrefixLength));
        ReadOnlyMemory<byte> loopBack = Select(NativeGeneratedConditionalBranches.ProjectShapedBranch(
            NativeGeneratedOperationCode.JumpNotZero, backOffset).Candidates);
        int skipOffset = checked(body.Length + tailPrefixLength + loopBack.Length);
        ReadOnlyMemory<byte> skip = Select(NativeGeneratedConditionalBranches.ProjectShapedBranch(
            NativeGeneratedOperationCode.JumpZero, skipOffset).Candidates);
        var closing = catalog.Closing.Select(entry => Select(entry.Candidates)).ToArray();

        var parts = new List<ReadOnlyMemory<byte>>(opening.Length + closing.Length + 7);
        parts.AddRange(opening);
        parts.Add(condition);
        parts.Add(skip);
        parts.Add(body);
        parts.Add(store);
        parts.Add(increment);
        parts.Add(decrement);
        parts.Add(loopBack);
        parts.AddRange(closing);
        int total = 0;
        foreach (var part in parts)
            total = checked(total + part.Length);
        byte[] bytes = new byte[total];
        int offset = 0;
        foreach (var part in parts)
        {
            part.Span.CopyTo(bytes.AsSpan(offset));
            offset += part.Length;
        }
        return new(bytes, samples.AsReadOnly());
    }
}
