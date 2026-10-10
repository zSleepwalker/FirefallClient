namespace Firefall.Reconstruction.Startup;

public readonly record struct NativeGeneratedOperationPair(
    NativeGeneratedOperationCode Forward, NativeGeneratedOperationCode Reverse);

public readonly record struct NativeGeneratedScratchChoice(uint DescriptorCode, byte Width);

public readonly record struct NativeGeneratedRegisterSourceChoice(
    NativeGeneratedScratchChoice Scratch, uint SourceDescriptorCode, bool RequiresImmediateMove, uint ImmediateValue);

public readonly record struct NativeGeneratedTableSourceChoice(
    byte Width, uint Scale, uint IndexValue, uint BaseOffset, uint BaseAddress,
    uint BaseDescriptorCode, uint IndexDescriptorCode)
{
    public uint EffectiveTableOffset => BaseOffset + IndexValue * Scale;
    public ulong SignedDwordBaseAddressBits => unchecked((ulong)(long)(int)BaseAddress);
}

/// <summary>Default-header mode0 and explicit mode1 body decisions. Does not lower templates or produce a complete routine.</summary>
public static class NativeGeneratedBodyDecisionRules
{
    public const uint PairTableVirtualAddress = 0x1C0E464;
    public const uint PairTableFileOffset = 0x180D064;
    public const uint UnaryMethodFileOffset = 0x1285E30;
    public const uint RotateMethodFileOffset = 0x1285EC0;
    public const uint ArithmeticMethodFileOffset = 0x1286100;

    public static NativeGeneratedOperationPair PairFromConsumedSample(uint sample) => (sample % 7) switch
    {
        0 => new(NativeGeneratedOperationCode.Add, NativeGeneratedOperationCode.Subtract),
        1 => new(NativeGeneratedOperationCode.Subtract, NativeGeneratedOperationCode.Add),
        2 => new(NativeGeneratedOperationCode.Increment, NativeGeneratedOperationCode.Decrement),
        3 => new(NativeGeneratedOperationCode.Decrement, NativeGeneratedOperationCode.Increment),
        4 => new(NativeGeneratedOperationCode.RotateLeft, NativeGeneratedOperationCode.RotateRight),
        5 => new(NativeGeneratedOperationCode.RotateRight, NativeGeneratedOperationCode.RotateLeft),
        _ => new(NativeGeneratedOperationCode.Xor, NativeGeneratedOperationCode.Xor)
    };

    public static NativeGeneratedScratchChoice ScratchFromConsumedParitySample(uint sample) =>
        (sample & 1) != 0 ? new(4, 8) : new(40, 32);

    public static NativeGeneratedScratchChoice ScratchForModeOneFromConsumedParitySample(uint sample) =>
        (sample & 1) != 0 ? new(4, 8) : new(56, 64);

    public static bool RotateUsesCountRegisterFromConsumedSample(uint sample) => sample % 3 == 1;

    public static NativeGeneratedScratchChoice RotateCountRegisterFromConsumedParitySample(uint sample) =>
        (sample & 1) != 0 ? new(2, 8) : new(38, 32);

    public static NativeGeneratedScratchChoice RotateCountRegisterForModeOneFromConsumedParitySample(uint sample) =>
        (sample & 1) != 0 ? new(2, 8) : new(54, 64);

    public static NativeGeneratedRegisterSourceChoice ArithmeticRegisterSource(uint parameter, uint paritySample)
    {
        var scratch = ScratchFromConsumedParitySample(paritySample);
        bool useCounter = (parameter & 1) != 0;
        uint source = scratch.Width == 8 ? (useCounter ? 3u : 2u) : (useCounter ? 39u : 38u);
        return new(scratch, source, !useCounter, parameter);
    }

    public static NativeGeneratedTableSourceChoice ArithmeticTableSource(uint parameter, uint capturedTableAddress)
    {
        byte width = (parameter & 1) != 0 ? (byte)8 : (byte)32;
        uint scale = 1u << (int)((parameter >> 1) & 3);
        uint working = parameter >> 4;
        uint index = 0;
        uint window = 256;
        if (scale != 1)
        {
            index = working % (256 / scale);
            window -= index * scale;
            working >>= 8;
        }
        uint offset = working % window;
        return new(width, scale, index, offset, unchecked(capturedTableAddress + offset), 44, scale == 1 ? 0u : 43u);
    }

    public static NativeGeneratedRegisterSourceChoice ArithmeticRegisterSourceForModeOne(uint parameter, uint paritySample)
    {
        var source = ArithmeticRegisterSource(parameter, paritySample);
        return source.Scratch.Width == 8 ? source : source with
        {
            Scratch = new(56, 64),
            SourceDescriptorCode = source.SourceDescriptorCode + 16
        };
    }

    public static NativeGeneratedTableSourceChoice ArithmeticTableSourceForModeOne(uint parameter, uint capturedTableAddress)
    {
        var source = ArithmeticTableSource(parameter, capturedTableAddress);
        return source with
        {
            Width = source.Width == 8 ? (byte)8 : (byte)64,
            BaseDescriptorCode = 60,
            IndexDescriptorCode = source.IndexDescriptorCode == 0 ? 0u : 59u
        };
    }
}
