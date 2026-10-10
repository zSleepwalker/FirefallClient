using System.Buffers.Binary;

namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixClockParameters(
    ulong InitialTickAt90, ulong ReferenceTickAt98, ulong ScaleBitsAtA0,
    ulong UpperLimitTickAtA8, ulong OffsetBitsAtB0, bool HasUpperLimit)
{
    public double Scale => BitConverter.Int64BitsToDouble(unchecked((long)ScaleBitsAtA0));

    public static MatrixClockParameters FromEnterZoneRawFields(ReadOnlySpan<byte> rawFieldsAt80,
        bool normalizedFlagAtB8)
    {
        if (rawFieldsAt80.Length != MatrixEnterZoneWire.RawFieldByteCountAt80)
            throw new ArgumentException("The EnterZone raw suffix fields must contain exactly 56 bytes.",
                nameof(rawFieldsAt80));
        return new(
            BinaryPrimitives.ReadUInt64LittleEndian(rawFieldsAt80[16..]),
            BinaryPrimitives.ReadUInt64LittleEndian(rawFieldsAt80[24..]),
            BinaryPrimitives.ReadUInt64LittleEndian(rawFieldsAt80[32..]),
            BinaryPrimitives.ReadUInt64LittleEndian(rawFieldsAt80[40..]),
            BinaryPrimitives.ReadUInt64LittleEndian(rawFieldsAt80[48..]),
            normalizedFlagAtB8);
    }
}

public readonly record struct MatrixClockSnapshot(
    MatrixClockParameters Parameters, ulong CurrentTick, uint LastDeltaLowWord);

/// <summary>
/// Reconstructed clock values for finite rates and in-range conversion inputs.
/// Floating-point exception flags are not simulated. Comparisons use the sign bit
/// of wrapping subtraction, not ordinary unsigned ordering.
/// </summary>
public static class MatrixClockNativeLayout
{
    public const uint InitializeFileOffset = 0x8FA620;
    public const uint AdvanceFileOffset = 0x8FA700;
    public const uint ProjectFileOffset = 0x8FA780;
    public const uint SampleConnectionClockFileOffset = 0x493650;
    public const uint Unsigned64ToDoubleFileOffset = 0x159E17E;
    public const uint DoubleToUnsigned64FileOffset = 0x159E060;
    public const int ConnectionClockObjectOffset = 0x918;
    public const int InitializedByteOffset = 0;
    public const int ReferenceTickOffset = 8;
    public const int ScaleOffset = 0x10;
    public const int CurrentTickOffset = 0x18;
    public const int DeltaLowWordOffset = 0x20;
    public const int TickOffsetOffset = 0x28;
    public const int HasUpperLimitOffset = 0x30;
    public const int UpperLimitTickOffset = 0x38;
    public const double ScaleTolerance = 0.001;

    public static MatrixClockSnapshot Initialize(MatrixClockParameters parameters)
    {
        ulong current = unchecked(parameters.InitialTickAt90 + parameters.OffsetBitsAtB0);
        if (parameters.HasUpperLimit && NativeClockArithmetic.DifferenceIsNegative(parameters.UpperLimitTickAtA8, current))
            current = parameters.UpperLimitTickAtA8;
        return new(parameters, current, 0);
    }

    public static MatrixClockSnapshot AdvanceWithoutRateConversion(MatrixClockSnapshot clock,
        ulong connectionClockTick)
    {
        double scale = clock.Parameters.Scale;
        if (!double.IsFinite(scale) || Math.Abs(scale - 1) > ScaleTolerance)
            throw new NotSupportedException(
                "This entry point supports only the native near-unit-rate branch.");
        return AdvanceProjectedTick(clock, connectionClockTick);
    }

    public static MatrixClockSnapshot Advance(MatrixClockSnapshot clock, ulong connectionClockTick)
    {
        double scale = clock.Parameters.Scale;
        if (!double.IsFinite(scale))
            throw new NotSupportedException("Native nonfinite-rate conversion and exception flags are not modeled.");
        ulong projected = connectionClockTick;
        if (Math.Abs(scale - 1) > ScaleTolerance)
        {
            ulong elapsed = unchecked(connectionClockTick - clock.Parameters.ReferenceTickAt98);
            double scaledElapsed = (double)elapsed * scale;
            ulong scaledBits = NativeClockArithmetic.ConvertInRangeDoubleToUnsigned64Bits(scaledElapsed);
            projected = unchecked(clock.Parameters.ReferenceTickAt98 + scaledBits);
        }
        return AdvanceProjectedTick(clock, projected);
    }

    private static MatrixClockSnapshot AdvanceProjectedTick(MatrixClockSnapshot clock, ulong projectedTick)
    {
        ulong candidate = unchecked(projectedTick + clock.Parameters.OffsetBitsAtB0);
        if (clock.Parameters.HasUpperLimit
            && NativeClockArithmetic.DifferenceIsNegative(clock.Parameters.UpperLimitTickAtA8, candidate))
            candidate = clock.Parameters.UpperLimitTickAtA8;
        ulong next = NativeClockArithmetic.DifferenceIsNegative(clock.CurrentTick, candidate) ? candidate : clock.CurrentTick;
        return new(clock.Parameters, next, unchecked((uint)(next - clock.CurrentTick)));
    }
}
