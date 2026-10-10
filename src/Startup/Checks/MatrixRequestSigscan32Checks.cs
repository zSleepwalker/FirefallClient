using Firefall.Reconstruction.Startup;

internal static class MatrixRequestSigscan32Checks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] golden = [32, 0x78, 0x56, 0x34, 0x12, 1];
        check("RequestSigscan32 normal application buffer is exactly code32/LE32 cache key/raw flag, no entity prefix",
            MatrixRequestSigscan32Wire.Encode(new(0x12345678, 1)).AsSpan().SequenceEqual(golden)
            && MatrixRequestSigscan32Wire.Decode(golden)
                is { Outcome: MatrixRequestSigscan32ReadOutcome.Accepted, BytesConsumed: 6,
                    Payload: { CacheKeyAt28: 0x12345678, RawFlagAt2C: 1 } }
            && MatrixRequestSigscan32Wire.ApplicationBufferChannel == 1);
        check("RequestSigscan32 producer flag is first-argument equals0, not a guessed boolean meaning",
            MatrixRequestSigscan32Wire.FromRequestArguments(0, 7) == new MatrixRequestSigscan32Payload(7, 1)
            && MatrixRequestSigscan32Wire.FromRequestArguments(1, 7) == new MatrixRequestSigscan32Payload(7, 0)
            && MatrixRequestSigscan32Wire.FromRequestArguments(-1, 7) == new MatrixRequestSigscan32Payload(7, 0));
        check("RequestSigscan32 preserves unsigned key boundaries and all raw flag values",
            new uint[] { 0, 0x80000000, uint.MaxValue }.All(key =>
                Enumerable.Range(0, 256).All(flag =>
                {
                    var payload = new MatrixRequestSigscan32Payload(key, (byte)flag);
                    return MatrixRequestSigscan32Wire.Decode(MatrixRequestSigscan32Wire.Encode(payload)).Payload == payload;
                })));
        check("RequestSigscan32 rejects every truncated prefix without partial payload and consumes exactly6 before trailing data",
            Enumerable.Range(0, golden.Length).All(length =>
                MatrixRequestSigscan32Wire.Decode(golden.AsSpan(0, length)) is { Payload: null, BytesConsumed: 0 })
            && MatrixRequestSigscan32Wire.Decode(golden.Concat(new byte[] { 0xFF }).ToArray()).BytesConsumed == 6
            && MatrixRequestSigscan32Wire.Decode(new byte[] { 0, 32, 0 }).Outcome
                == MatrixRequestSigscan32ReadOutcome.NonCanonicalHeader
            && MatrixRequestSigscan32Wire.Decode(new byte[] { 34 }).Outcome
                == MatrixRequestSigscan32ReadOutcome.UnexpectedMessageCode);
    }
}
