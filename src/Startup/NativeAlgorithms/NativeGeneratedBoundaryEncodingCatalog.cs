namespace Firefall.Reconstruction.Startup;

public sealed record NativeGeneratedBoundaryByteCandidates(
    NativeGeneratedBoundaryStep Step, IReadOnlyList<ReadOnlyMemory<byte>> Candidates);

public sealed record NativeGeneratedBoundaryEncodingCatalog(
    IReadOnlyList<NativeGeneratedBoundaryByteCandidates> Opening,
    IReadOnlyList<NativeGeneratedBoundaryByteCandidates> Closing)
{
    /// <summary>Only mode0/header0200 boundary templates; no body, RNG draw, allocation or execution.</summary>
    public static NativeGeneratedBoundaryEncodingCatalog InitialModeZero()
    {
        var plan = NativeGeneratedProviderBoundaryPlanner.ForModeZero();
        var opening = plan.Opening.Span;
        var closing = plan.Closing.Span;
        NativeGeneratedBoundaryByteCandidates[] openingCandidates =
        [
            Single(opening[0], 0x55),
            new(opening[1], Array.AsReadOnly<ReadOnlyMemory<byte>>(
                [new byte[] { 0x89, 0xE5 }, new byte[] { 0x8B, 0xEC }])),
            Single(opening[2], 0x50),
            Single(opening[3], 0x51),
            Single(opening[4], 0x52),
            Single(opening[5], 0x53),
            Single(opening[6], 0x56),
            Single(opening[7], 0x57),
            Single(opening[8], 0x8B, 0x45, 0x08),
            Single(opening[9], 0x8B, 0x55, 0x0C)
        ];
        NativeGeneratedBoundaryByteCandidates[] closingCandidates =
        [
            Single(closing[0], 0x5F),
            Single(closing[1], 0x5E),
            Single(closing[2], 0x5B),
            Single(closing[3], 0x5A),
            Single(closing[4], 0x59),
            Single(closing[5], 0x58),
            Single(closing[6], 0xC9),
            Single(closing[7], 0xC2, 0x08, 0x00)
        ];
        return new(Array.AsReadOnly(openingCandidates), Array.AsReadOnly(closingCandidates));
    }

    /// <summary>Mode1 boundary branch with header0200 only; fresh native constructor still always selects mode0.</summary>
    public static NativeGeneratedBoundaryEncodingCatalog ModeOneDefaultHeader()
    {
        var plan = NativeGeneratedProviderBoundaryPlanner.ForModeOne();
        var opening = plan.Opening.Span;
        var closing = plan.Closing.Span;
        NativeGeneratedBoundaryByteCandidates[] openingCandidates =
        [
            Single(opening[0], 0x50),
            Single(opening[1], 0x51),
            Single(opening[2], 0x52),
            Single(opening[3], 0x53),
            Single(opening[4], 0x56),
            Single(opening[5], 0x57),
            ModeOneStackLoad(opening[6], 32, 0),
            ModeOneStackLoad(opening[7], 24, 0x10)
        ];
        NativeGeneratedBoundaryByteCandidates[] closingCandidates =
        [
            Single(closing[0], 0x5F),
            Single(closing[1], 0x5E),
            Single(closing[2], 0x5B),
            Single(closing[3], 0x5A),
            Single(closing[4], 0x59),
            Single(closing[5], 0x58),
            Single(closing[6], 0xC3)
        ];
        return new(Array.AsReadOnly(openingCandidates), Array.AsReadOnly(closingCandidates));
    }

    private static NativeGeneratedBoundaryByteCandidates Single(NativeGeneratedBoundaryStep step, params byte[] bytes) =>
        new(step, Array.AsReadOnly<ReadOnlyMemory<byte>>([bytes]));

    private static NativeGeneratedBoundaryByteCandidates ModeOneStackLoad(
        NativeGeneratedBoundaryStep step, int displacement, byte modRm)
    {
        var projection = NativeGeneratedModeOneMemoryEncoding.Project(
            (int)NativeGeneratedRegisterCode64.Rsp, 0, 1, displacement, modRm);
        var fields = projection.Fields
            ?? throw new InvalidOperationException("Default native mode1 stack load must have an accepted address shape.");
        return Single(step, [0x48, 0x8B, .. fields.ToAddressBytes()]);
    }
}
