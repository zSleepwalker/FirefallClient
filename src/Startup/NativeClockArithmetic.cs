namespace Firefall.Reconstruction.Startup;

internal static class NativeClockArithmetic
{
    public static bool DifferenceIsNegative(ulong left, ulong right) =>
        unchecked((long)(left - right)) < 0;

    public static ulong ConvertInRangeDoubleToUnsigned64Bits(double value)
    {
        if (!double.IsFinite(value) || value < -9223372036854775808d || value >= 18446744073709551616d)
            throw new NotSupportedException(
                "The native out-of-range conversion returns integer-indefinite bits and signals floating-point status.");
        return value < 0 ? unchecked((ulong)(long)value) : unchecked((ulong)value);
    }
}
