namespace Firefall.Reconstruction.Startup;

public sealed record NativeGeneratedProviderPrefixSnapshot(
    uint IterationCount, uint FirstRawSample, ReadOnlyMemory<byte> TableBytes)
{
    public ushort HeaderWordAt0 => 0x0200;
    public byte HeaderByteAt2 => 3;
    public uint InitialWordAt4 => 0;
    public byte InitialByteAt18 => 1;
    public int UInt32CallsConsumed => 265;
}

/// <summary>Only the fixed initial RNG/header/table stage, not subsequent descriptor generation or executable callbacks.</summary>
public static class NativeGeneratedTransformProviderPrefix
{
    public const uint ConstructorFileOffset = 0x1285060;
    public const uint GeneratorFileOffset = 0x1286F00;
    public const uint DescriptorSelectFileOffset = 0x12867D0;
    public const uint DescriptorDispatchFileOffset = 0x12866C0;
    public const uint OutputBuilderFileOffset = 0x12868D0;
    public const int RngPointerOffset = 8;
    public const int TablePointerOffset = 0xC;
    public const int FirstOutputPointerOffset = 0x10;
    public const int SecondOutputPointerOffset = 0x14;
    public const int TableLength = 0x108;
    public const uint MinimumIterations = 60;
    public const uint MaximumIterations = 100;

    public static NativeGeneratedProviderPrefixSnapshot FromSeed(uint seed)
    {
        var generator = new NativeMt19937(seed);
        return Initialize(generator.NextUInt32);
    }

    public static NativeGeneratedProviderPrefixSnapshot Initialize(Func<uint> nextUInt32)
    {
        ArgumentNullException.ThrowIfNull(nextUInt32);
        var range = NativeRandomInclusive32.Draw(nextUInt32, MinimumIterations, MaximumIterations);
        uint iterations = range.Value ?? throw new InvalidOperationException("Fixed native provider range must produce an accepted value.");
        byte[] table = new byte[TableLength];
        for (int i = 0; i < table.Length; i++)
            table[i] = unchecked((byte)nextUInt32());
        return new(iterations, range.ConsumedSample, table);
    }
}
