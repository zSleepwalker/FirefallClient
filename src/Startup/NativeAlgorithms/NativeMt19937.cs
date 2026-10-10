namespace Firefall.Reconstruction.Startup;

/// <summary>Offline reconstruction of the native 32-bit MT19937 seed/twist/output methods. Not a cryptographic generator.</summary>
public sealed class NativeMt19937
{
    public const uint SeedMethodFileOffset = 0x127FFD0;
    public const uint ArraySeedMethodFileOffset = 0x1280010;
    public const uint TwistMethodFileOffset = 0x127FF20;
    public const uint NextUInt32MethodFileOffset = 0x127FEA0;
    public const uint NativeVtableVirtualAddress = 0x1C7EBAC;
    public const int NativeAllocationSize = 0x9C8;
    public const int NativeIndexOffset = 4;
    public const int NativeStateOffset = 8;
    public const int StateWordCount = 624;

    private readonly uint[] _state = new uint[StateWordCount];
    private int _index;

    public NativeMt19937(uint seed) => Reseed(seed);
    public NativeMt19937(ReadOnlySpan<uint> seedWords) => Reseed(seedWords);

    public void Reseed(uint seed)
    {
        _state[0] = seed;
        for (int i = 1; i < StateWordCount; i++)
            _state[i] = unchecked((uint)i + 1812433253U * (_state[i - 1] ^ (_state[i - 1] >> 30)));
        _index = StateWordCount;
    }

    public void Reseed(ReadOnlySpan<uint> seedWords)
    {
        if (seedWords.IsEmpty)
            throw new ArgumentException("Native array initialization requires at least one readable seed word.", nameof(seedWords));
        Reseed(19650218U);
        int i = 1, j = 0;
        for (int remaining = Math.Max(StateWordCount, seedWords.Length); remaining > 0; remaining--)
        {
            _state[i] = unchecked((_state[i] ^ (1664525U * (_state[i - 1] ^ (_state[i - 1] >> 30))))
                + seedWords[j] + (uint)j);
            i++;
            if (++j == seedWords.Length)
                j = 0;
            if (i == StateWordCount)
            {
                _state[0] = _state[^1];
                i = 1;
            }
        }
        for (int remaining = StateWordCount - 1; remaining > 0; remaining--)
        {
            _state[i] = unchecked((_state[i] ^ (1566083941U * (_state[i - 1] ^ (_state[i - 1] >> 30)))) - (uint)i);
            if (++i == StateWordCount)
            {
                _state[0] = _state[^1];
                i = 1;
            }
        }
        _state[0] = 0x80000000;
    }

    public NativeRandomInclusive32Result NextUInt32Inclusive(uint lower, uint upper) =>
        NativeRandomInclusive32.ProjectConsumedSample(NextUInt32(), lower, upper);

    public uint NextUInt32()
    {
        if (_index == StateWordCount)
            Twist();
        uint value = _state[_index++];
        value ^= value >> 11;
        value ^= (value << 7) & 0x9D2C5680;
        value ^= (value << 15) & 0xEFC60000;
        return value ^ (value >> 18);
    }

    private void Twist()
    {
        for (int i = 0; i < StateWordCount; i++)
        {
            uint joined = (_state[i] & 0x80000000) | (_state[(i + 1) % StateWordCount] & 0x7FFFFFFF);
            _state[i] = _state[(i + 397) % StateWordCount] ^ (joined >> 1)
                ^ ((joined & 1) != 0 ? 0x9908B0DFU : 0U);
        }
        _index = 0;
    }
}
