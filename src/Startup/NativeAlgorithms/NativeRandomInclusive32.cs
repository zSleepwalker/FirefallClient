namespace Firefall.Reconstruction.Startup;

public enum NativeRandomInclusive32Outcome
{
    Accepted,
    InvalidBoundsException,
    WrappedDivisorZero
}

public readonly record struct NativeRandomInclusive32Result(
    NativeRandomInclusive32Outcome Outcome, uint ConsumedSample, uint? Value)
{
    public uint? NativeDiagnosticCode =>
        Outcome == NativeRandomInclusive32Outcome.InvalidBoundsException ? 0x1C271921U : null;
}

/// <summary>Inclusive unsigned modulo projection. The native virtual draw precedes validation; full UInt32 range divides by zero.</summary>
public static class NativeRandomInclusive32
{
    public const uint MethodFileOffset = 0x127FD70;

    public static NativeRandomInclusive32Result Draw(Func<uint> nextUInt32, uint lower, uint upper)
    {
        ArgumentNullException.ThrowIfNull(nextUInt32);
        return ProjectConsumedSample(nextUInt32(), lower, upper);
    }

    public static NativeRandomInclusive32Result ProjectConsumedSample(uint sample, uint lower, uint upper)
    {
        if (lower > upper)
            return new(NativeRandomInclusive32Outcome.InvalidBoundsException, sample, null);
        uint divisor = unchecked(upper - lower + 1);
        if (divisor == 0)
            return new(NativeRandomInclusive32Outcome.WrappedDivisorZero, sample, null);
        return new(NativeRandomInclusive32Outcome.Accepted, sample, unchecked(lower + sample % divisor));
    }
}
