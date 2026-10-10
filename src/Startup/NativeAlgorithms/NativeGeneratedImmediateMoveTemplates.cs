namespace Firefall.Reconstruction.Startup;

/// <summary>Mode0 ordinary8/32-bit and explicit mode1 ordinary8/64-bit immediate MOV candidates in native table order.</summary>
public static class NativeGeneratedImmediateMoveTemplates
{
    public const uint CompactByteEmitterFileOffset = 0x12911A0;
    public const uint CompactWideEmitterFileOffset = 0x12912C0;
    public const uint ModRmByteEmitterFileOffset = 0x12913D0;
    public const uint ModRmWideEmitterFileOffset = 0x12914A0;

    public static IReadOnlyList<ReadOnlyMemory<byte>> ForRegister(uint descriptorCode, uint value)
    {
        byte width;
        byte index;
        if (descriptorCode is >= 1 and <= 8)
        {
            width = 8;
            index = (byte)(descriptorCode - 1);
        }
        else if (descriptorCode is >= 37 and <= 44)
        {
            width = 32;
            index = (byte)(descriptorCode - 37);
        }
        else
            throw new ArgumentOutOfRangeException(nameof(descriptorCode), "Only recovered mode0 ordinary8/32-bit register templates are supported.");

        var projection = NativeGeneratedImmediateBytes.Project(value, width);
        ReadOnlyMemory<byte> immediate = projection.Bytes
            ?? throw new InvalidOperationException("Supported native MOV width must resolve immediate bytes.");
        byte[] compact = new byte[1 + immediate.Length];
        compact[0] = (byte)((width == 8 ? 0xB0 : 0xB8) + index);
        immediate.Span.CopyTo(compact.AsSpan(1));
        byte[] modRm = new byte[2 + immediate.Length];
        modRm[0] = width == 8 ? (byte)0xC6 : (byte)0xC7;
        modRm[1] = (byte)(0xC0 | index);
        immediate.Span.CopyTo(modRm.AsSpan(2));
        return Array.AsReadOnly<ReadOnlyMemory<byte>>([compact, modRm]);
    }

    public static IReadOnlyList<ReadOnlyMemory<byte>> ForModeOneOrdinaryRegister(
        uint descriptorCode, ulong value, byte sourceImmediateWidth)
    {
        if (descriptorCode is >= 1 and <= 8)
        {
            if (sourceImmediateWidth != 8)
                throw new ArgumentOutOfRangeException(nameof(sourceImmediateWidth), "Native byte MOV requires an8-bit immediate descriptor.");
            return ForRegister(descriptorCode, unchecked((uint)value));
        }
        if (descriptorCode is < 53 or > 60)
            throw new ArgumentOutOfRangeException(nameof(descriptorCode), "This mode1 catalog supports ordinary64-bit registers only, not extended or16/32-bit destinations.");
        if (sourceImmediateWidth is not (32 or 64))
            throw new ArgumentOutOfRangeException(nameof(sourceImmediateWidth), "Native MOV to a64-bit register requires a32/64-bit immediate descriptor.");
        byte index = (byte)(descriptorCode - 53);
        var candidates = new List<ReadOnlyMemory<byte>>(2);
        if (sourceImmediateWidth == 64)
        {
            ReadOnlyMemory<byte> immediate = NativeGeneratedImmediateBytes.Project(value, 64).Bytes
                ?? throw new InvalidOperationException("Native64-bit MOV immediate must resolve bytes.");
            byte[] compact = new byte[2 + immediate.Length];
            compact[0] = 0x48;
            compact[1] = (byte)(0xB8 + index);
            immediate.Span.CopyTo(compact.AsSpan(2));
            candidates.Add(compact);
        }
        bool fitsSigned32 = value == unchecked((ulong)(long)(int)(uint)value);
        if (sourceImmediateWidth == 32 || fitsSigned32)
        {
            ReadOnlyMemory<byte> immediate = NativeGeneratedImmediateBytes.Project(value, 32).Bytes
                ?? throw new InvalidOperationException("Native32-bit MOV immediate must resolve bytes.");
            byte[] modRm = new byte[3 + immediate.Length];
            modRm[0] = 0x48;
            modRm[1] = 0xC7;
            modRm[2] = (byte)(0xC0 | index);
            immediate.Span.CopyTo(modRm.AsSpan(3));
            candidates.Add(modRm);
        }
        return candidates.AsReadOnly();
    }
}
