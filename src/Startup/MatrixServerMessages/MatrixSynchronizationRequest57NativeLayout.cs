using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Firefall.Reconstruction.Startup;

public enum MatrixSynchronizationEnvelopeOutcome
{
    GateBlocked,
    TooShort,
    DigestMismatch,
    EnvelopeVerified
}

public sealed record MatrixSynchronizationEnvelope(
    ReadOnlyMemory<byte> PayloadBytes, ushort RawPayloadPrefixWord, ReadOnlyMemory<byte> DigestBytes);

public sealed record MatrixSynchronizationEnvelopeResult(
    MatrixSynchronizationEnvelopeOutcome Outcome, MatrixSynchronizationEnvelope? Envelope);

public enum MatrixSynchronizationDispatchSelection
{
    None,
    RegisteredHandlerByLeadingWord,
    DirectByLeadingWord
}

public static class MatrixSynchronizationRequest57NativeLayout
{
    public const uint ConsumerFileOffset = 0x124B3F0;
    public const uint Md5UpdateFileOffset = 0x12847D0;
    public const uint Md5FinalizeFileOffset = 0x12848A0;
    public const uint Md5TransformFileOffset = 0x1283FE0;
    public const uint LeadingWordReaderFileOffset = 0x11FC1C0;
    public const int MinimumBufferLength = 20;
    public const int DigestLength = 16;
    public const int OwnerModeByteOffset = 0x1C;
    public const int OwnerTransformTablePointerOffset = 0x28;
    public const int TransformFunctionPointerOffset = 0x14;
    public const int OwnerNativeLockOffset = 0x58;
    public const int OwnerGateByteOffset = 0x70;

    public static bool SelectExternalTransform(byte sampledGateByte, byte sampledModeByte, uint bufferLength) =>
        sampledGateByte == 0 && bufferLength >= MinimumBufferLength && sampledModeByte != 0;

    /// <summary>Potential sampled branch if reader construction/inflation completes; MD5 verification alone does not dispatch.</summary>
    public static MatrixSynchronizationDispatchSelection SelectDispatchAfterVerification(
        MatrixSynchronizationEnvelopeOutcome outcome, byte sampledModeByte) =>
        outcome != MatrixSynchronizationEnvelopeOutcome.EnvelopeVerified
            ? MatrixSynchronizationDispatchSelection.None
            : sampledModeByte != 0
                ? MatrixSynchronizationDispatchSelection.RegisteredHandlerByLeadingWord
                : MatrixSynchronizationDispatchSelection.DirectByLeadingWord;

    /// <summary>Opt-in offline path when the owner provider is known to be the recovered fresh generated mode0 provider.</summary>
    public static MatrixSynchronizationEnvelopeResult VerifyWithReconstructedInitialProvider(
        ReadOnlySpan<byte> bytes, byte sampledGateByte, byte sampledModeByte,
        NativeGeneratedInitialModeZeroSnapshot? provider)
    {
        if (!SelectExternalTransform(sampledGateByte, sampledModeByte, (uint)bytes.Length))
            return VerifyPostTransformBuffer(bytes, sampledGateByte);
        ArgumentNullException.ThrowIfNull(provider);
        byte[] transformed = NativeGeneratedInitialModeZeroByteTransform.Inverse(bytes, provider);
        return VerifyPostTransformBuffer(transformed, sampledGateByte);
    }

    /// <summary>Input already includes external-hook changes. Verifies MD5 only; payload is still zlib plus its length footer, not a decoded routing key.</summary>
    public static MatrixSynchronizationEnvelopeResult VerifyPostTransformBuffer(
        ReadOnlySpan<byte> bytes, byte sampledGateByte = 0)
    {
        if (sampledGateByte != 0)
            return new(MatrixSynchronizationEnvelopeOutcome.GateBlocked, null);
        if (bytes.Length < MinimumBufferLength)
            return new(MatrixSynchronizationEnvelopeOutcome.TooShort, null);
        var payload = bytes[..^DigestLength];
        var digest = bytes[^DigestLength..];
        if (!MD5.HashData(payload).AsSpan().SequenceEqual(digest))
            return new(MatrixSynchronizationEnvelopeOutcome.DigestMismatch, null);
        return new(MatrixSynchronizationEnvelopeOutcome.EnvelopeVerified,
            new(payload.ToArray(), BinaryPrimitives.ReadUInt16LittleEndian(payload), digest.ToArray()));
    }
}
