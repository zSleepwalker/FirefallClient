using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public readonly record struct NativeGeneratedImmediateResult(ReadOnlyMemory<byte>? Bytes, uint? NativeDiagnosticCode);

/// <summary>Immediate-width switch only; surrounding fixed native buffer capacity and allocation are separate.</summary>
public static class NativeGeneratedImmediateBytes
{
    public const uint MethodFileOffset = 0x1299D00;
    public const uint UnsupportedWidthDiagnostic = 1040336665;

    public static NativeGeneratedImmediateResult Project(ulong value, uint bitWidth)
    {
        uint byteCount = bitWidth >> 3;
        if (byteCount is not (1 or 2 or 4 or 8))
            return new(null, UnsupportedWidthDiagnostic);
        Span<byte> littleEndian = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(littleEndian, value);
        return new(littleEndian[..(int)byteCount].ToArray(), null);
    }
}
