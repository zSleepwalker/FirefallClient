namespace Firefall.Characters;

/// <summary>
/// Native Character::Commands::ExecuteTinkeringPlan, metadata ID 237.
/// Its three copied vectors contain opaque 16-byte elements.
/// </summary>
public sealed class ExecuteTinkeringPlan
{
    public const ushort MessageId = 237;
    public const int TargetOffset = 0x18;
    public const int Value28Offset = 0x28;
    public const int FirstVectorOffset = 0x2C;
    public const int SecondVectorOffset = 0x38;
    public const int ThirdVectorOffset = 0x44;
    public const int VectorElementSize = 16;

    public ExecuteTinkeringPlan(
        ulong target,
        uint value28,
        ReadOnlyMemory<byte> firstVector,
        ReadOnlyMemory<byte> secondVector,
        ReadOnlyMemory<byte> thirdVector)
    {
        ValidateVector(firstVector, nameof(firstVector));
        ValidateVector(secondVector, nameof(secondVector));
        ValidateVector(thirdVector, nameof(thirdVector));

        Target = target;
        Value28 = value28;
        FirstVector = firstVector.ToArray();
        SecondVector = secondVector.ToArray();
        ThirdVector = thirdVector.ToArray();
    }

    public ulong Target { get; }
    public uint Value28 { get; }
    public ReadOnlyMemory<byte> FirstVector { get; }
    public ReadOnlyMemory<byte> SecondVector { get; }
    public ReadOnlyMemory<byte> ThirdVector { get; }

    private static void ValidateVector(ReadOnlyMemory<byte> vector, string parameterName)
    {
        if (vector.Length % VectorElementSize != 0)
            throw new ArgumentException(
                $"Vector length must be a multiple of {VectorElementSize} bytes.",
                parameterName);
    }
}
