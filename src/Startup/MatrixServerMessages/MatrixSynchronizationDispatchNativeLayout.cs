using System.Buffers.Binary;
using Firefall.Reconstruction.Shared;

namespace Firefall.Reconstruction.Startup;

public enum MatrixSynchronizationDirectOutcome
{
    TruncatedLeadingWord,
    TruncatedDwordPair,
    UnsupportedKey,
    UnsupportedSecondDword,
    ActivationPathSelected
}

public sealed record MatrixSynchronizationDirectFields(ushort Key, uint GeneratorSeed, uint SecondDwordAt6);

public sealed record MatrixSynchronizationDirectResult(
    MatrixSynchronizationDirectOutcome Outcome, MatrixSynchronizationDirectFields? Fields,
    int BytesConsumed, uint? NativeDiagnosticCode);

/// <summary>Offline structural projections only; no native activation, external hook or registered callback is executed.</summary>
public static class MatrixSynchronizationDispatchNativeLayout
{
    public const uint DirectDispatcherFileOffset = 0x124B7C0;
    public const uint DwordPairReaderFileOffset = 0x124E6E0;
    public const uint DwordReaderFileOffset = 0x1206B20;
    public const uint RegisteredDispatcherFileOffset = 0x124C010;
    public const uint RegisteredLookupFileOffset = 0x124C9B0;
    public const uint RegisteredBucketFileOffset = 0x1248050;
    public const uint GeneratorSeedFileOffset = 0x127FFD0;
    public const int OwnerHandlerMapOffset = 0x38;
    public const uint UnsupportedKeyDiagnostic = 0x0FFACB37;
    public const uint UnsupportedSecondDwordDiagnostic = 0x0FA98FC0;

    /// <summary>Input is the INFLATED reader buffer including its leading WORD, not the compressed MD5-verified envelope. Both DWORDs are read before gates.</summary>
    public static MatrixSynchronizationDirectResult DecodeModeZeroFields(ReadOnlySpan<byte> verifiedPayload)
    {
        if (verifiedPayload.Length < 2)
            return new(MatrixSynchronizationDirectOutcome.TruncatedLeadingWord, null, 0, null);
        if (verifiedPayload.Length < 10)
            return new(MatrixSynchronizationDirectOutcome.TruncatedDwordPair, null,
                verifiedPayload.Length < 6 ? 2 : 6, null);
        var fields = new MatrixSynchronizationDirectFields(
            BinaryPrimitives.ReadUInt16LittleEndian(verifiedPayload),
            BinaryPrimitives.ReadUInt32LittleEndian(verifiedPayload[2..]),
            BinaryPrimitives.ReadUInt32LittleEndian(verifiedPayload[6..]));
        if (fields.Key != 1)
            return new(MatrixSynchronizationDirectOutcome.UnsupportedKey, fields, 10, UnsupportedKeyDiagnostic);
        if (fields.SecondDwordAt6 != 8)
            return new(MatrixSynchronizationDirectOutcome.UnsupportedSecondDword, fields, 10,
                UnsupportedSecondDwordDiagnostic);
        return new(MatrixSynchronizationDirectOutcome.ActivationPathSelected, fields, 10, null);
    }

    public static uint ComputeRegisteredKeyHash(uint key)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, key);
        return EngineByteHash32.ComputeFnv1a(bytes);
    }

    /// <summary>Projects the native split-bucket formula from sampled container fields; does not validate or access a native container.</summary>
    public static uint SelectRegisteredBucket(uint key, uint sampledMask, uint sampledActiveBucketCount)
    {
        uint bucket = ComputeRegisteredKeyHash(key) & sampledMask;
        return sampledActiveBucketCount > bucket ? bucket : unchecked(bucket - 1 - (sampledMask >> 1));
    }
}
