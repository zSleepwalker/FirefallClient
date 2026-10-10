using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public readonly record struct NativeGeneratedCodeFields(uint Kind, byte Width, byte Index);

public readonly record struct NativeGeneratedCodeResult(int Code, NativeGeneratedCodeFields? Fields)
{
    public uint? NativeDiagnosticCode => Fields is null ? 1040197660u : null;
}

/// <summary>Numeric operand classification and sparse descriptor writes; no instruction emission or execution.</summary>
public static class NativeGeneratedDescriptorFactories
{
    public const int OperandSize = 64;
    public const int OperationSize = 200;
    public const uint CodeMethodFileOffset = 0x128A840;
    public const uint CodeOperandMethodFileOffset = 0x12506F0;
    public const uint NoOperandsMethodFileOffset = 0x1250780;
    public const uint OneOperandMethodFileOffset = 0x1284F90;
    public const uint TwoOperandsMethodFileOffset = 0x1250850;
    public const uint ImmediateOperandMethodFileOffset = 0x1284EF0;

    public static NativeGeneratedCodeResult ProjectCode(int code)
    {
        NativeGeneratedCodeFields? fields = code switch
        {
            0 => new(0, 0, 0),
            >= 1 and <= 20 => new(4, 8, (byte)(code - 1)),
            >= 21 and <= 36 => new(4, 16, (byte)(code - 21)),
            >= 37 and <= 52 => new(4, 32, (byte)(code - 37)),
            >= 53 and <= 68 => new(4, 64, (byte)(code - 53)),
            >= 69 and <= 74 => new(5, 16, (byte)(code - 69)),
            >= 75 and <= 82 => new(6, 64, (byte)(code - 75)),
            >= 83 and <= 90 => new(7, 64, (byte)(code - 83)),
            >= 91 and <= 106 => new(8, 128, (byte)(code - 91)),
            >= 107 and <= 122 => new(9, 32, (byte)(code - 107)),
            >= 123 and <= 138 => new(10, 32, (byte)(code - 123)),
            _ => null
        };
        return new(code, fields);
    }

    public static NativeGeneratedCodeResult ApplyCodeOperand(Span<byte> operand, int code)
    {
        RequireLength(operand, OperandSize, nameof(operand));
        var result = ProjectCode(code);
        BinaryPrimitives.WriteInt32LittleEndian(operand[12..], code);
        if (result.Fields is not { } fields)
            return result;
        WriteWord(operand, 8, fields.Kind);
        operand[16] = fields.Width;
        operand[17] = fields.Index;
        WriteWord(operand, 24, 0);
        WriteWord(operand, 20, 0);
        WriteHalfWord(operand, 28, 0);
        WriteWord(operand, 0, fields.Kind);
        WriteWord(operand, 48, fields.Width);
        return result;
    }

    public static void ApplyImmediateOperand(Span<byte> operand, uint lowWord, uint highWord, uint bitWidthArgument)
    {
        RequireLength(operand, OperandSize, nameof(operand));
        WriteWord(operand, 0, 2);
        WriteWord(operand, 12, 0);
        WriteWord(operand, 8, 0);
        WriteHalfWord(operand, 16, 0);
        WriteWord(operand, 24, 0);
        WriteWord(operand, 20, 0);
        WriteHalfWord(operand, 28, 0);
        WriteWord(operand, 40, lowWord);
        WriteWord(operand, 44, highWord);
        WriteWord(operand, 48, unchecked((byte)bitWidthArgument));
        operand[56] = 1;
    }

    public static void ApplyNoOperands(Span<byte> operation, uint operationCode)
    {
        RequireLength(operation, OperationSize, nameof(operation));
        WriteWord(operation, 0, operationCode);
        ClearAbsentOperand(operation.Slice(8, OperandSize));
        ClearAbsentOperand(operation.Slice(72, OperandSize));
        ClearAbsentOperand(operation.Slice(136, OperandSize));
    }

    public static void ApplyOneOperand(Span<byte> operation, uint operationCode, ReadOnlySpan<byte> first)
    {
        RequireLength(operation, OperationSize, nameof(operation));
        RequireOperand(first, operation, nameof(first));
        WriteWord(operation, 0, operationCode);
        first.CopyTo(operation.Slice(8, OperandSize));
        ClearAbsentOperand(operation.Slice(72, OperandSize));
        ClearAbsentOperand(operation.Slice(136, OperandSize));
    }

    public static void ApplyTwoOperands(Span<byte> operation, uint operationCode,
        ReadOnlySpan<byte> first, ReadOnlySpan<byte> second)
    {
        RequireLength(operation, OperationSize, nameof(operation));
        RequireOperand(first, operation, nameof(first));
        RequireOperand(second, operation, nameof(second));
        WriteWord(operation, 0, operationCode);
        first.CopyTo(operation.Slice(8, OperandSize));
        second.CopyTo(operation.Slice(72, OperandSize));
        ClearAbsentOperand(operation.Slice(136, OperandSize));
    }

    private static void ClearAbsentOperand(Span<byte> operand)
    {
        WriteWord(operand, 0, 0);
        WriteWord(operand, 12, 0);
        WriteWord(operand, 8, 0);
        WriteHalfWord(operand, 16, 0);
        WriteWord(operand, 24, 0);
        WriteWord(operand, 20, 0);
        WriteHalfWord(operand, 28, 0);
        WriteWord(operand, 48, 0);
    }

    private static void RequireOperand(ReadOnlySpan<byte> operand, Span<byte> destination, string name)
    {
        if (operand.Length != OperandSize)
            throw new ArgumentException("Captured native operand must contain exactly 64 bytes.", name);
        if (operand.Overlaps(destination))
            throw new ArgumentException("Overlapping native SIMD source/destination writes are not modeled.", name);
    }

    private static void RequireLength(Span<byte> storage, int size, string name)
    {
        if (storage.Length < size)
            throw new ArgumentException($"Captured native storage must contain at least {size} bytes.", name);
    }

    private static void WriteWord(Span<byte> storage, int offset, uint value) =>
        BinaryPrimitives.WriteUInt32LittleEndian(storage[offset..], value);

    private static void WriteHalfWord(Span<byte> storage, int offset, ushort value) =>
        BinaryPrimitives.WriteUInt16LittleEndian(storage[offset..], value);
}
