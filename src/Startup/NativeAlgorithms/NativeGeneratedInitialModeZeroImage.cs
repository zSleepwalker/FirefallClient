namespace Firefall.Reconstruction.Startup;

public sealed record NativeGeneratedInitialModeZeroSnapshot(
    NativeGeneratedProviderPrefixSnapshot Prefix,
    uint CapturedTableAddress,
    IReadOnlyList<ReadOnlyMemory<byte>> ForwardBodyFragments,
    IReadOnlyList<ReadOnlyMemory<byte>> InverseBodyFragments,
    IReadOnlyList<NativeGeneratedByteStep> ForwardByteSteps,
    IReadOnlyList<NativeGeneratedByteStep> InverseByteSteps,
    NativeGeneratedWrappedBody Forward,
    NativeGeneratedWrappedBody Inverse,
    int UInt32CallsBeforeWrappers,
    IReadOnlyList<uint> ConsumedUInt32Samples)
{
    public byte ControlByteAt18DuringGeneration { get; init; } = 1;
    public uint DiscardedUInt32Calls { get; init; }
}

/// <summary>Fresh mode0/header0200,03/control1 projection, or explicit captured-control override. No native allocation or execution.</summary>
public static class NativeGeneratedInitialModeZeroImage
{
    public const uint GeneratorFileOffset = NativeGeneratedDefaultHeaderImage.GeneratorFileOffset;
    public const uint DispatcherFileOffset = NativeGeneratedDefaultHeaderImage.DispatcherFileOffset;

    public static NativeGeneratedInitialModeZeroSnapshot FromSeed(uint seed, uint capturedTableAddress)
    {
        var rng = new NativeMt19937(seed);
        return Generate(rng.NextUInt32, capturedTableAddress);
    }

    public static NativeGeneratedInitialModeZeroSnapshot Generate(Func<uint> nextUInt32, uint capturedTableAddress) =>
        GenerateWithCapturedControlByte(nextUInt32, capturedTableAddress, 1);

    public static NativeGeneratedInitialModeZeroSnapshot GenerateWithCapturedControlByte(
        Func<uint> nextUInt32, uint capturedTableAddress, byte controlByteAt18)
    {
        var image = NativeGeneratedDefaultHeaderImage.GenerateCaptured(nextUInt32, capturedTableAddress, 0, controlByteAt18);
        return new(image.Prefix, image.CapturedTableAddress, image.ForwardBodyFragments, image.InverseBodyFragments,
            image.ForwardByteSteps, image.InverseByteSteps, image.Forward, image.Inverse,
            image.UInt32CallsBeforeWrappers, image.ConsumedUInt32Samples)
        {
            ControlByteAt18DuringGeneration = image.ControlByteAt18DuringGeneration,
            DiscardedUInt32Calls = image.DiscardedUInt32Calls
        };
    }
}
