using System.Buffers.Binary;
using System.Numerics;

namespace Firefall.Reconstruction.Serialization;

/// <summary>
/// Confirmed byte-level subset of the SDB row converter at raw offset 0x1311100.
/// This does not implement row allocation, null-bit propagation or type coercion
/// outside the integer and floating-point cases listed below.
/// </summary>
public static class SdbFieldConversion
{
    public static void Convert(
        ReadOnlySpan<byte> source,
        byte sourceType,
        Span<byte> destination,
        byte destinationType)
    {
        var sourceInfo = SdbFieldTypes.Lookup(sourceType)
            ?? throw new NotSupportedException($"Unknown SDB source type {sourceType}.");
        var destinationInfo = SdbFieldTypes.Lookup(destinationType)
            ?? throw new NotSupportedException($"Unknown SDB destination type {destinationType}.");
        if (source.Length < sourceInfo.StorageBytes)
            throw new ArgumentException("The SDB source field is truncated.", nameof(source));
        if (destination.Length < destinationInfo.StorageBytes)
            throw new ArgumentException("The SDB destination field is truncated.", nameof(destination));

        if (sourceType == destinationType)
        {
            source[..sourceInfo.StorageBytes].CopyTo(destination);
            return;
        }

        if (sourceType == 9 && destinationType == 10)
        {
            float floatValue = BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(source));
            BinaryPrimitives.WriteInt64LittleEndian(destination, BitConverter.DoubleToInt64Bits((double)floatValue));
            return;
        }

        if (sourceType == 10 && destinationType == 9)
        {
            double doubleValue = BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(source));
            BinaryPrimitives.WriteInt32LittleEndian(destination, BitConverter.SingleToInt32Bits((float)doubleValue));
            return;
        }

        if (sourceType == 26 && destinationType is 9 or 10)
        {
            float halfValue = BitConverter.Int32BitsToSingle(ExpandHalf(BinaryPrimitives.ReadUInt16LittleEndian(source)));
            if (destinationType == 9)
                BinaryPrimitives.WriteInt32LittleEndian(destination, BitConverter.SingleToInt32Bits(halfValue));
            else
                BinaryPrimitives.WriteInt64LittleEndian(destination, BitConverter.DoubleToInt64Bits((double)halfValue));
            return;
        }

        if (destinationType == 26 && sourceType is 9 or 10)
        {
            float floatValue = sourceType == 9
                ? BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(source))
                : (float)BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(source));
            BinaryPrimitives.WriteUInt16LittleEndian(destination, PackHalf(floatValue));
            return;
        }

        if (sourceType is >= 1 and <= 8 && destinationType is 9 or 10 or 26)
        {
            double doubleValue = sourceType switch
            {
                1 => source[0],
                2 => BinaryPrimitives.ReadUInt16LittleEndian(source),
                3 => (double)BinaryPrimitives.ReadUInt32LittleEndian(source),
                4 => (double)BinaryPrimitives.ReadUInt64LittleEndian(source),
                5 => (sbyte)source[0],
                6 => BinaryPrimitives.ReadInt16LittleEndian(source),
                7 => BinaryPrimitives.ReadInt32LittleEndian(source),
                8 => (double)BinaryPrimitives.ReadInt64LittleEndian(source),
                _ => throw new InvalidOperationException("Unexpected integer source type.")
            };
            if (destinationType == 10)
            {
                BinaryPrimitives.WriteInt64LittleEndian(destination, BitConverter.DoubleToInt64Bits(doubleValue));
                return;
            }

            float floatValue = (float)doubleValue;
            if (destinationType == 9)
                BinaryPrimitives.WriteInt32LittleEndian(destination, BitConverter.SingleToInt32Bits(floatValue));
            else
                BinaryPrimitives.WriteUInt16LittleEndian(destination, PackHalf(floatValue));
            return;
        }

        if (sourceType is 9 or 10 or 26 && destinationType is >= 1 and <= 8)
        {
            double floatingValue = sourceType switch
            {
                9 => BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(source)),
                10 => BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(source)),
                26 => BitConverter.Int32BitsToSingle(ExpandHalf(BinaryPrimitives.ReadUInt16LittleEndian(source))),
                _ => throw new InvalidOperationException("Unexpected floating-point source type.")
            };
            if (destinationType is 3 or 4 or 8)
            {
                ulong integerBits = ConvertFloatingTo64Bits(floatingValue, destinationType == 4);
                if (destinationType == 3)
                    BinaryPrimitives.WriteUInt32LittleEndian(destination, (uint)integerBits);
                else
                    BinaryPrimitives.WriteUInt64LittleEndian(destination, integerBits);
                return;
            }

            int integerValue = floatingValue >= int.MinValue && floatingValue < 2147483648d
                ? (int)floatingValue
                : int.MinValue;
            switch (destinationInfo.StorageBytes)
            {
                case 1:
                    destination[0] = unchecked((byte)integerValue);
                    break;
                case 2:
                    BinaryPrimitives.WriteUInt16LittleEndian(destination, unchecked((ushort)integerValue));
                    break;
                case 4:
                    BinaryPrimitives.WriteInt32LittleEndian(destination, integerValue);
                    break;
            }
            return;
        }

        if (sourceType is < 1 or > 8 || destinationType is < 1 or > 8)
            throw new NotSupportedException(
                $"SDB conversion from type {sourceType} to {destinationType} is not reconstructed.");

        var value = sourceType switch
        {
            1 => source[0],
            2 => BinaryPrimitives.ReadUInt16LittleEndian(source),
            3 => BinaryPrimitives.ReadUInt32LittleEndian(source),
            4 => BinaryPrimitives.ReadUInt64LittleEndian(source),
            5 => unchecked((ulong)(long)(sbyte)source[0]),
            6 => unchecked((ulong)(long)BinaryPrimitives.ReadInt16LittleEndian(source)),
            7 => unchecked((ulong)(long)BinaryPrimitives.ReadInt32LittleEndian(source)),
            8 => unchecked((ulong)BinaryPrimitives.ReadInt64LittleEndian(source)),
            _ => throw new InvalidOperationException("Unexpected integer source type.")
        };

        switch (destinationInfo.StorageBytes)
        {
            case 1:
                destination[0] = (byte)value;
                break;
            case 2:
                BinaryPrimitives.WriteUInt16LittleEndian(destination, (ushort)value);
                break;
            case 4:
                BinaryPrimitives.WriteUInt32LittleEndian(destination, (uint)value);
                break;
            case 8:
                BinaryPrimitives.WriteUInt64LittleEndian(destination, value);
                break;
            default:
                throw new InvalidOperationException("Unexpected integer field width.");
        }
    }

    private static ulong ConvertFloatingTo64Bits(double value, bool unsigned)
    {
        if (double.IsNaN(value) || value < -9223372036854775808d
            || value >= (unsigned ? 18446744073709551616d : 9223372036854775808d))
            return 0x8000000000000000;

        return value < 0
            ? unchecked((ulong)(long)value)
            : unchecked((ulong)value);
    }

    private static int ExpandHalf(ushort half)
    {
        uint sign = (uint)(half & 0x8000) << 16;
        int exponent = (half >> 10) & 0x1F;
        uint fraction = (uint)(half & 0x3FF);
        if (exponent == 0)
        {
            if (fraction == 0)
                return unchecked((int)sign);
            int highestBit = BitOperations.Log2(fraction);
            return unchecked((int)(sign | (uint)(highestBit + 103) << 23
                | (fraction - (1u << highestBit)) << (23 - highestBit)));
        }

        uint bits = exponent == 31 ? 0x7F800000u : (uint)(exponent + 112) << 23;
        return unchecked((int)(sign | bits | fraction << 13));
    }

    private static ushort PackHalf(float value)
    {
        if (value == 0)
            return 0;

        uint bits = unchecked((uint)BitConverter.SingleToInt32Bits(value));
        uint sign = (bits >> 16) & 0x8000;
        int exponent = (int)((bits >> 23) & 0xFF) - 112;
        uint fraction = bits & 0x7FFFFF;

        if (exponent is >= 1 and <= 29)
            return (ushort)(sign | (((uint)exponent << 10) + ((fraction + 0x1000) >> 13)));

        if (exponent <= 0)
        {
            if (exponent < -10)
                return 0;
            fraction = (fraction | 0x800000) >> (1 - exponent);
            if ((fraction & 0x1000) != 0)
                fraction += 0x2000;
            return (ushort)(sign | (fraction >> 13));
        }

        if (exponent == 143)
            return (ushort)(sign | 0x7C00 | (fraction == 0 ? 0 : Math.Max(1u, fraction >> 13)));

        if ((fraction & 0x1000) != 0)
        {
            fraction += 0x2000;
            if ((fraction & 0x800000) != 0)
            {
                fraction = 0;
                exponent++;
            }
        }

        if (exponent > 30)
            return (ushort)(sign | 0x7C00);
        return (ushort)(sign | ((uint)exponent << 10) | (fraction >> 13));
    }
}
