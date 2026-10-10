namespace Firefall.Reconstruction.Startup;

/// <summary>Mode0 scratch and explicit mode1 ordinary-register INC/DEC; byte ROL/ROR candidates in native table order.</summary>
public static class NativeGeneratedScratchTemplates
{
    public static IReadOnlyList<ReadOnlyMemory<byte>> UnaryFromConsumedParitySample(
        NativeGeneratedOperationCode operation, uint paritySample)
    {
        bool increment = IsIncrement(operation);
        byte modRm = increment ? (byte)0xC3 : (byte)0xCB;
        var scratch = NativeGeneratedBodyDecisionRules.ScratchFromConsumedParitySample(paritySample);
        if (scratch.Width == 8)
            return Array.AsReadOnly<ReadOnlyMemory<byte>>([new byte[] { 0xFE, modRm }]);
        return Array.AsReadOnly<ReadOnlyMemory<byte>>(
            [new byte[] { increment ? (byte)0x43 : (byte)0x4B }, new byte[] { 0xFF, modRm }]);
    }

    public static IReadOnlyList<ReadOnlyMemory<byte>> UnaryForModeOneFromConsumedParitySample(
        NativeGeneratedOperationCode operation, uint paritySample) =>
        UnaryForModeOneOrdinaryRegister(operation,
            NativeGeneratedBodyDecisionRules.ScratchForModeOneFromConsumedParitySample(paritySample).DescriptorCode);

    public static IReadOnlyList<ReadOnlyMemory<byte>> UnaryForModeOneOrdinaryRegister(
        NativeGeneratedOperationCode operation, uint descriptorCode)
    {
        bool increment = IsIncrement(operation);
        byte group = increment ? (byte)0xC0 : (byte)0xC8;
        if (descriptorCode is >= 1 and <= 8)
            return Array.AsReadOnly<ReadOnlyMemory<byte>>(
                [new byte[] { 0xFE, (byte)(group | (descriptorCode - 1)) }]);
        if (descriptorCode is >= 53 and <= 60)
            return Array.AsReadOnly<ReadOnlyMemory<byte>>(
                [new byte[] { 0x48, 0xFF, (byte)(group | (descriptorCode - 53)) }]);
        throw new ArgumentOutOfRangeException(nameof(descriptorCode), "This mode1 unary catalog supports ordinary8/64-bit registers only.");
    }

    public static IReadOnlyList<ReadOnlyMemory<byte>> RotateImmediate(
        NativeGeneratedOperationCode operation, byte count)
    {
        byte modRm = RotationModRm(operation);
        var candidates = new List<ReadOnlyMemory<byte>>(2) { new byte[] { 0xC0, modRm, count } };
        if (count == 1)
            candidates.Add(new byte[] { 0xD0, modRm });
        return candidates.AsReadOnly();
    }

    public static IReadOnlyList<ReadOnlyMemory<byte>> RotateViaCl(NativeGeneratedOperationCode operation) =>
        Array.AsReadOnly<ReadOnlyMemory<byte>>([new byte[] { 0xD2, RotationModRm(operation) }]);

    private static byte RotationModRm(NativeGeneratedOperationCode operation) => operation switch
    {
        NativeGeneratedOperationCode.RotateLeft => 0xC3,
        NativeGeneratedOperationCode.RotateRight => 0xCB,
        _ => throw new ArgumentOutOfRangeException(nameof(operation), "Only recovered ROL/ROR scratch templates are supported.")
    };

    private static bool IsIncrement(NativeGeneratedOperationCode operation) => operation switch
    {
        NativeGeneratedOperationCode.Increment => true,
        NativeGeneratedOperationCode.Decrement => false,
        _ => throw new ArgumentOutOfRangeException(nameof(operation), "Only recovered INC/DEC scratch templates are supported.")
    };
}
