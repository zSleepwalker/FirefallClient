using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public sealed record MatrixModeZeroMetadataSnapshot(
    uint WordAt24, ReadOnlyMemory<byte> NarrowBytesAt28,
    uint WordAt40, ReadOnlyMemory<byte> NarrowBytesAt44);

public sealed record MatrixModeZeroDescriptorSnapshot(
    uint WindowsVersionCode, ReadOnlyMemory<byte> WindowsDriveSerialDigest,
    ReadOnlyMemory<byte> AdapterAddressDigest, MatrixModeZeroMetadataSnapshot Metadata);

/// <summary>Uncompressed operation2 serialization only; captured metadata remains opaque and no generated callback is executed.</summary>
public static class MatrixModeZeroDescriptorCodec
{
    public const uint BuilderFileOffset = 0x124E720;
    public const uint SerializeFieldsFileOffset = 0x124FFE0;
    public const uint CopyMetadataFileOffset = 0x124AA60;
    public const uint ConvertMetadataTextFileOffset = 0x1273530;
    public const ushort Operation = 2;
    public const int MinimumSerializedLength = 50;

    public static MatrixModeZeroDescriptorSnapshot FromCapturedDigestSlots(
        uint windowsVersionCode, MatrixModeZeroDigestSlotsResult slots, MatrixModeZeroMetadataSnapshot metadata)
    {
        ArgumentNullException.ThrowIfNull(slots);
        ArgumentNullException.ThrowIfNull(metadata);
        if (slots.WindowsDriveSerialSlot is not { } drive || slots.AdapterSlot is not { } adapter)
            throw new ArgumentException("Both native digest slot contents must be known before descriptor serialization.", nameof(slots));
        return new(windowsVersionCode, drive, adapter, metadata);
    }

    public static byte[] Serialize(MatrixModeZeroDescriptorSnapshot descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(descriptor.Metadata);
        if (descriptor.WindowsDriveSerialDigest.Length != 16 || descriptor.AdapterAddressDigest.Length != 16)
            throw new ArgumentException("Each descriptor digest must contain exactly sixteen bytes.", nameof(descriptor));
        var firstText = descriptor.Metadata.NarrowBytesAt28.Span;
        var secondText = descriptor.Metadata.NarrowBytesAt44.Span;
        if (firstText.Length > ushort.MaxValue || secondText.Length > ushort.MaxValue)
            throw new ArgumentException("Native narrow-string serialization rejects lengths greater than FFFF.", nameof(descriptor));
        byte[] bytes = new byte[checked(MinimumSerializedLength + firstText.Length + secondText.Length)];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, Operation);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(2), descriptor.WindowsVersionCode);
        descriptor.WindowsDriveSerialDigest.Span.CopyTo(bytes.AsSpan(6, 16));
        descriptor.AdapterAddressDigest.Span.CopyTo(bytes.AsSpan(22, 16));
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(38), descriptor.Metadata.WordAt24);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(42), (ushort)firstText.Length);
        firstText.CopyTo(bytes.AsSpan(44));
        int secondWordOffset = 44 + firstText.Length;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(secondWordOffset), descriptor.Metadata.WordAt40);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(secondWordOffset + 4), (ushort)secondText.Length);
        secondText.CopyTo(bytes.AsSpan(secondWordOffset + 6));
        return bytes;
    }
}
