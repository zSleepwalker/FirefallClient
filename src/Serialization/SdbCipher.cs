using System.Buffers.Binary;
using System.Text;
using Firefall.Reconstruction.Shared;

namespace Firefall.Reconstruction.Serialization;

public static class SdbCipher
{
    // 0x12F16D0: FNV-1a followed by the engine's final mixing steps.
    public static uint ComputeNameSeed(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return EngineByteHash32.Compute(Encoding.Latin1.GetBytes(name));
    }

    // 0x1317410: one MT19937 word per full dword, then one fresh word per leftover byte.
    public static void Apply(Span<byte> payload, uint seed)
    {
        var random = new Mt19937(seed);
        var offset = 0;
        for (; offset + 4 <= payload.Length; offset += 4)
        {
            var value = BinaryPrimitives.ReadUInt32LittleEndian(payload[offset..]);
            BinaryPrimitives.WriteUInt32LittleEndian(payload[offset..], value ^ random.Next());
        }
        for (; offset < payload.Length; offset++)
            payload[offset] ^= (byte)random.Next();
    }

    public sealed class Mt19937
    {
        private readonly uint[] state = new uint[624];
        private int index = 624;

        public Mt19937(uint seed)
        {
            state[0] = seed;
            for (var i = 1; i < state.Length; i++)
                state[i] = unchecked(1812433253u * (state[i - 1] ^ (state[i - 1] >> 30)) + (uint)i);
        }

        public uint Next()
        {
            if (index == 624)
                Twist();
            var value = state[index++];
            value ^= value >> 11;
            value ^= (value << 7) & 0x9D2C5680;
            value ^= (value << 15) & 0xEFC60000;
            return value ^ (value >> 18);
        }

        private void Twist()
        {
            for (var i = 0; i < state.Length; i++)
            {
                var value = (state[i] & 0x80000000) | (state[(i + 1) % 624] & 0x7FFFFFFF);
                state[i] = state[(i + 397) % 624] ^ (value >> 1) ^
                           ((value & 1) == 0 ? 0u : 0x9908B0DFu);
            }
            index = 0;
        }
    }
}
