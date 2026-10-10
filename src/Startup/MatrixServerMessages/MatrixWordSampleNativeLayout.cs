namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixWordSampleRoute(int DescriptorOffset, int ReceiverOffset, uint NativeTagVirtualAddress);

public static class MatrixWordSampleNativeLayout
{
    public const uint Message63HandlerFileOffset = 0x491F90;
    public const uint Message64HandlerFileOffset = 0x4920B0;
    public const uint RecordSampleFileOffset = 0x481430;
    public const uint ReceiverVirtualAddress = 0x1E0004C;

    public static IReadOnlyList<MatrixWordSampleRoute> Message63Routes { get; } = Array.AsReadOnly(new[]
    {
        new MatrixWordSampleRoute(0x28, 0x2DC, 0x1DFAA90),
        new MatrixWordSampleRoute(0x2A, 0x2EC, 0x1DFAAB0)
    });

    public static IReadOnlyList<MatrixWordSampleRoute> Message64Routes { get; } = Array.AsReadOnly(new[]
    {
        new MatrixWordSampleRoute(0x28, 0x34C, 0x1DFAB70),
        new MatrixWordSampleRoute(0x2A, 0x35C, 0x1DFAB90),
        new MatrixWordSampleRoute(0x2C, 0x36C, 0x1DFABB0),
        new MatrixWordSampleRoute(0x2E, 0x37C, 0x1DFABD0),
        new MatrixWordSampleRoute(0x30, 0x2FC, 0x1DFAAD0),
        new MatrixWordSampleRoute(0x32, 0x30C, 0x1DFAAF0),
        new MatrixWordSampleRoute(0x34, 0x33C, 0x1DFAB50)
    });

    public static bool SelectRecording(byte receiverByte4, byte receiverByte5) =>
        receiverByte4 != 0 && receiverByte5 == 0;

    public static float ToSample(ushort value) => value;
}
