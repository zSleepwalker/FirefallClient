namespace Firefall.Reconstruction.Startup;

public sealed record NativeGeneratedModeOneMemoryFields(
    byte ModRm, byte? Sib, byte RexAddressBitsToOr,
    bool EmitAddressSizeOverride, ReadOnlyMemory<byte> DisplacementBytes)
{
    public byte[] ToAddressBytes()
    {
        byte[] bytes = new byte[1 + (Sib.HasValue ? 1 : 0) + DisplacementBytes.Length];
        bytes[0] = ModRm;
        int offset = 1;
        if (Sib is { } sib)
            bytes[offset++] = sib;
        DisplacementBytes.Span.CopyTo(bytes.AsSpan(offset));
        return bytes;
    }
}

public sealed record NativeGeneratedModeOneMemoryProjection(
    NativeGeneratedModeOneMemoryShapeResult Shape, NativeGeneratedModeOneMemoryFields? Fields);

/// <summary>Pure captured ModRM/SIB/displacement writes. Preserves native quirks; no opcode, prefix layout, allocation or execution.</summary>
public static class NativeGeneratedModeOneMemoryEncoding
{
    public const uint EncoderFileOffset = 0x129A320;
    public const uint BaseRegisterBitsWriterFileOffset = 0x129ADA0;
    public const uint RexPrefixSlotHelperFileOffset = 0x1299F10;

    public static NativeGeneratedModeOneMemoryProjection Project(
        int baseDescriptorCode, int indexDescriptorCode, byte scale, int displacement,
        byte capturedModRm, byte capturedSib = 0, bool addressSizeOverrideAlreadyPresent = false)
    {
        var shape = NativeGeneratedModeOneMemoryShape.Project(baseDescriptorCode, indexDescriptorCode, scale);
        if (!shape.IsAccepted)
            return new(shape, null);
        var baseFields = NativeGeneratedDescriptorFactories.ProjectCode(baseDescriptorCode).Fields
            ?? throw new InvalidOperationException("Accepted native address shape must have classified base fields.");
        var indexFields = NativeGeneratedDescriptorFactories.ProjectCode(indexDescriptorCode).Fields
            ?? throw new InvalidOperationException("Accepted native address shape must have classified index fields.");
        bool wideDisplacement = displacement is < -128 or > 127;
        byte mod = wideDisplacement ? (byte)2 : (byte)1;
        int displacementLength = 0;
        byte rex = 0;
        byte modRm;
        byte? sib = null;
        if (shape.UsesNativeNoSibFastPath)
        {
            if (baseFields.Kind == 0)
            {
                modRm = (byte)((capturedModRm & 0x38) | 5);
                displacementLength = 4;
            }
            else
            {
                bool forceDisplacement = baseFields.Index == 5;
                modRm = (byte)((capturedModRm & 0x38) | (baseFields.Index & 7)
                    | (displacement != 0 || forceDisplacement ? mod << 6 : 0));
                displacementLength = displacement != 0 || forceDisplacement ? (wideDisplacement ? 4 : 1) : 0;
                if ((baseFields.Index & 8) != 0)
                    rex |= 1;
            }
        }
        else
        {
            bool forceDisplacement = baseFields.Index is 5 or 13;
            modRm = (byte)((capturedModRm & 0x38) | 4
                | (displacement != 0 || forceDisplacement ? mod << 6 : 0));
            displacementLength = displacement != 0 || forceDisplacement ? (wideDisplacement ? 4 : 1) : 0;
            byte sibByte;
            if (indexFields.Kind == 0)
                sibByte = (byte)((capturedSib & 7) | 0xE0);
            else
            {
                sibByte = scale switch
                {
                    1 => (byte)(capturedSib & 0x3F),
                    2 => (byte)((capturedSib & 0x3F) | 0x40),
                    4 => (byte)((capturedSib & 0x3F) | 0x80),
                    8 => (byte)(capturedSib | 0xC0),
                    _ => throw new InvalidOperationException("Accepted native address shape must have a supported scale.")
                };
                sibByte = (byte)((sibByte & 0xC7) | ((indexFields.Index & 7) << 3));
                if ((indexFields.Index & 8) != 0)
                    rex |= 2;
            }
            if (baseFields.Kind == 0)
            {
                modRm &= 0x3F;
                sibByte = (byte)((sibByte & 0xF8) | 5);
                displacementLength = 4;
            }
            else
            {
                sibByte = (byte)((sibByte & 0xF8) | (baseFields.Index & 7));
                if ((baseFields.Index & 8) != 0)
                    rex |= 1;
            }
            sib = sibByte;
        }
        ReadOnlyMemory<byte> displacementBytes = displacementLength == 0 ? ReadOnlyMemory<byte>.Empty
            : NativeGeneratedImmediateBytes.Project(unchecked((uint)displacement), (uint)(displacementLength * 8)).Bytes
                ?? throw new InvalidOperationException("Native address displacement must have a supported byte count.");
        return new(shape, new(modRm, sib, rex,
            shape.NeedsAddressSizeOverride && !addressSizeOverrideAlreadyPresent, displacementBytes));
    }
}
