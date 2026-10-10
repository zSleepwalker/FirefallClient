namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketSynchronizedClockSnapshot(
    uint RateBitsAt14, ulong LocalReferenceTickAt18, ulong SynchronizedBaseTickAt20, ulong LastSampleAt60)
{
    public float Rate => BitConverter.Int32BitsToSingle(unchecked((int)RateBitsAt14));
}

/// <summary>
/// Value projection from socket+D78's clock. Native locks, calibration and floating
/// exception flags are not simulated. Retains the double-to-single rounding stage.
/// </summary>
public static class GameSocketSynchronizedClock
{
    public const uint SampleFileOffset = 0xBE8410;
    public const uint SingleToUnsigned64FileOffset = 0x159DE7F;
    public const int SocketClockPointerOffset = 0xD78;
    public const int RateOffset = 0x14;
    public const int LocalReferenceTickOffset = 0x18;
    public const int SynchronizedBaseTickOffset = 0x20;
    public const int CriticalSectionOffset = 0x28;
    public const int LastSampleOffset = 0x60;

    public static GameSocketSynchronizedClockSnapshot SampleFinite(
        GameSocketSynchronizedClockSnapshot clock, uint tickMilliseconds)
    {
        ulong localTick = 1000UL * tickMilliseconds;
        ulong elapsed = unchecked(localTick - clock.LocalReferenceTickAt18);
        double elapsedDouble = (double)elapsed;
        float elapsedSingle = (float)elapsedDouble;
        float scaledElapsed = elapsedSingle * clock.Rate;
        ulong scaledBits = NativeClockArithmetic.ConvertInRangeDoubleToUnsigned64Bits(scaledElapsed);
        ulong candidate = unchecked(clock.SynchronizedBaseTickAt20 + scaledBits);
        if (NativeClockArithmetic.DifferenceIsNegative(candidate, clock.LastSampleAt60))
            return clock;
        return clock with { LastSampleAt60 = candidate };
    }
}
