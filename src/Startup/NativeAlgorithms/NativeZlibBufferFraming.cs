using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Firefall.Reconstruction.Startup;

public sealed record NativeZlibInflateCapture(
    int? ResultCode, uint? RemainingInputBytes, ReadOnlyMemory<byte> ProducedBytes);

public enum NativeZlibInflationOutcome
{
    InflatedSnapshotReady,
    LengthFooterMissing,
    UnresolvedCapture,
    NativeInflateRejected,
    UnsupportedManagedAllocation
}

public sealed record NativeZlibInflationResult(
    NativeZlibInflationOutcome Outcome, uint? DeclaredLength, ReadOnlyMemory<byte>? InflatedBytes);

/// <summary>Native zlib framing over captured compression/inflation results; does not substitute a compressor implementation.</summary>
public static class NativeZlibBufferFraming
{
    public const uint CompressStreamFileOffset = 0x11E8910;
    public const uint CompressBufferFileOffset = 0x1256370;
    public const uint InflateFramedBufferFileOffset = 0x120C250;
    public const uint InflateBufferFileOffset = 0x1256A60;
    public const uint AppendMd5FileOffset = 0x124BC10;
    public const int LengthFooterSize = 4;
    public const int DigestSize = 16;
    public const int CompressionLevel = 6;
    public const int WindowBits = 15;
    public const int MemoryLevel = 8;
    public const int Strategy = 0;
    public const int FinishFlush = 4;
    public const int StreamEndResult = 1;

    public static byte[] AppendLengthAndMd5(ReadOnlySpan<byte> capturedZlibBytes, uint nativeUncompressedLength)
    {
        byte[] frame = new byte[checked(capturedZlibBytes.Length + LengthFooterSize + DigestSize)];
        capturedZlibBytes.CopyTo(frame);
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(capturedZlibBytes.Length), nativeUncompressedLength);
        MD5.HashData(frame.AsSpan(0, frame.Length - DigestSize)).CopyTo(frame.AsSpan(frame.Length - DigestSize));
        return frame;
    }

    /// <summary>Input excludes MD5. Successful native inflation can leave an FF-filled suffix; it does not require total_out == declared length.</summary>
    public static NativeZlibInflationResult ProjectCapturedInflation(
        ReadOnlySpan<byte> verifiedFramedBytes, NativeZlibInflateCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        if (verifiedFramedBytes.Length < LengthFooterSize)
            return new(NativeZlibInflationOutcome.LengthFooterMissing, null, null);
        uint length = BinaryPrimitives.ReadUInt32LittleEndian(verifiedFramedBytes[^LengthFooterSize..]);
        if (capture.ResultCode is { } code && code != StreamEndResult
            || capture.RemainingInputBytes is { } remaining && remaining != 0)
            return new(NativeZlibInflationOutcome.NativeInflateRejected, length, null);
        if (capture.ResultCode is null || capture.RemainingInputBytes is null)
            return new(NativeZlibInflationOutcome.UnresolvedCapture, length, null);
        if (capture.ProducedBytes.Length > length)
            throw new ArgumentException("Captured inflater output exceeds the declared native destination allocation.", nameof(capture));
        if (length > Array.MaxLength)
            return new(NativeZlibInflationOutcome.UnsupportedManagedAllocation, length, null);
        byte[] output = new byte[checked((int)length)];
        output.AsSpan().Fill(0xFF);
        capture.ProducedBytes.Span.CopyTo(output);
        return new(NativeZlibInflationOutcome.InflatedSnapshotReady, length, output);
    }
}
