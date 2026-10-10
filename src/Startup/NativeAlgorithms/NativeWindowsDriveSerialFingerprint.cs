using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Firefall.Reconstruction.Startup;

public sealed record NativeStorageIoctlCapture(bool? NativeSucceeded, uint BytesReturned, ReadOnlyMemory<byte> Bytes);

public sealed record NativeWindowsDriveSerialCapture(
    uint? WindowsDirectoryReturnValue, ReadOnlyMemory<char> DirectoryBuffer,
    uint? FileHandle, NativeStorageIoctlCapture? HeaderQuery, NativeStorageIoctlCapture? DescriptorQuery);

public enum NativeWindowsDriveFingerprintOutcome
{
    Accepted,
    DirectoryApiFailed,
    InvalidFileHandle,
    HeaderQueryFailed,
    HeaderLengthMismatch,
    DescriptorQueryFailed,
    DescriptorLengthMismatch,
    UnresolvedCapture,
    UnsupportedNativeBufferAccess
}

public sealed record NativeWindowsDriveFingerprintResult(
    NativeWindowsDriveFingerprintOutcome Outcome, string? DevicePath,
    ReadOnlyMemory<byte>? Digest, ReadOnlyMemory<byte>? SerialBytes)
{
    public bool? NativeReturnValue => Outcome switch
    {
        NativeWindowsDriveFingerprintOutcome.Accepted => true,
        NativeWindowsDriveFingerprintOutcome.UnresolvedCapture
            or NativeWindowsDriveFingerprintOutcome.UnsupportedNativeBufferAccess => null,
        NativeWindowsDriveFingerprintOutcome.DirectoryApiFailed
            or NativeWindowsDriveFingerprintOutcome.InvalidFileHandle
            or NativeWindowsDriveFingerprintOutcome.HeaderQueryFailed
            or NativeWindowsDriveFingerprintOutcome.HeaderLengthMismatch
            or NativeWindowsDriveFingerprintOutcome.DescriptorQueryFailed
            or NativeWindowsDriveFingerprintOutcome.DescriptorLengthMismatch => false,
        _ => throw new InvalidOperationException("Unknown Windows-drive fingerprint outcome.")
    };
}

/// <summary>Captured Windows-directory/storage-query projection; never opens a drive or invokes DeviceIoControl.</summary>
public static class NativeWindowsDriveSerialFingerprint
{
    public const uint MethodFileOffset = 0x127C730;
    public const uint QuerySerialMethodFileOffset = 0x127C550;
    public const uint WindowsDirectorySlotVirtualAddress = 0x1AC41C4;
    public const uint DeviceIoControlSlotVirtualAddress = 0x1AC41C8;
    public const uint CreateFileSlotVirtualAddress = 0x1AC4348;
    public const uint CloseHandleSlotVirtualAddress = 0x1AC4388;
    public const uint IoctlStorageQueryProperty = 0x2D1400;
    public const uint DesiredAccess = 0x80000000;
    public const uint ShareMode = 3;
    public const uint CreationDisposition = 3;
    public const int DirectoryCapacity = 260;
    public const int QueryInputLength = 12;
    public const int HeaderLength = 8;
    public const int SerialNumberOffsetField = 24;
    public const string DevicePathPrefix = @"\\.\";

    public static byte[] CreateQueryInput() => new byte[QueryInputLength];

    public static NativeWindowsDriveFingerprintResult Compute(NativeWindowsDriveSerialCapture capture)
    {
        ArgumentNullException.ThrowIfNull(capture);
        if (capture.WindowsDirectoryReturnValue is null)
            return Incomplete(null);
        if (capture.WindowsDirectoryReturnValue == 0)
            return new(NativeWindowsDriveFingerprintOutcome.DirectoryApiFailed, null, null, null);
        if (capture.DirectoryBuffer.Length < 2)
            return Incomplete(null);
        var drive = capture.DirectoryBuffer.Span[..2];
        int driveZero = drive.IndexOf('\0');
        string path = DevicePathPrefix + new string(driveZero < 0 ? drive : drive[..driveZero]);
        if (capture.FileHandle is null)
            return Incomplete(path);
        if (capture.FileHandle == uint.MaxValue)
            return new(NativeWindowsDriveFingerprintOutcome.InvalidFileHandle, path, null, null);

        var header = capture.HeaderQuery;
        if (header?.NativeSucceeded is null)
            return Incomplete(path);
        if (header.NativeSucceeded == false)
            return new(NativeWindowsDriveFingerprintOutcome.HeaderQueryFailed, path, null, null);
        if (header.BytesReturned != HeaderLength)
            return new(NativeWindowsDriveFingerprintOutcome.HeaderLengthMismatch, path, null, null);
        if (header.Bytes.Length < HeaderLength)
            return Incomplete(path);
        uint size = BinaryPrimitives.ReadUInt32LittleEndian(header.Bytes.Span[4..]);
        var descriptor = capture.DescriptorQuery;
        if (descriptor?.NativeSucceeded is null)
            return Incomplete(path);
        if (descriptor.NativeSucceeded == false)
            return new(NativeWindowsDriveFingerprintOutcome.DescriptorQueryFailed, path, null, null);
        if (descriptor.BytesReturned != size)
            return new(NativeWindowsDriveFingerprintOutcome.DescriptorLengthMismatch, path, null, null);
        if (descriptor.Bytes.Length < size)
            return Incomplete(path);
        if (size < SerialNumberOffsetField + 4)
            return Unsupported(path);

        var bytes = descriptor.Bytes.Span[..checked((int)size)];
        uint offset = BinaryPrimitives.ReadUInt32LittleEndian(bytes[SerialNumberOffsetField..]);
        if (offset >= size)
            return Unsupported(path);
        var serial = bytes[checked((int)offset)..];
        int end = serial.IndexOf((byte)0);
        if (end < 0)
            return Unsupported(path);
        byte[] copied = serial[..end].ToArray();
        return new(NativeWindowsDriveFingerprintOutcome.Accepted, path, MD5.HashData(copied), copied);
    }

    private static NativeWindowsDriveFingerprintResult Incomplete(string? path) =>
        new(NativeWindowsDriveFingerprintOutcome.UnresolvedCapture, path, null, null);

    private static NativeWindowsDriveFingerprintResult Unsupported(string path) =>
        new(NativeWindowsDriveFingerprintOutcome.UnsupportedNativeBufferAccess, path, null, null);
}
