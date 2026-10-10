namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixRunningClockUpdateSnapshot(
    bool Initialized, bool HasUpperLimit, ulong UpperLimit, ulong Offset);

public sealed record MatrixClockUpdateSelection(
    MatrixClockParameters StoredParameters, bool EmitPausedRuntime110,
    bool SetRunningUpperLimit, bool RebaseRunningReferenceAndScale,
    MatrixRunningClockUpdateSnapshot RunningClock,
    bool InitializationAssertion, bool OffsetAssertion)
{
    public ulong? RebasedReferenceTick => RebaseRunningReferenceAndScale ? StoredParameters.InitialTickAt90 : null;
    public ulong? RebasedScaleBits => RebaseRunningReferenceAndScale ? StoredParameters.ScaleBitsAtA0 : null;
}

public static class MatrixClockUpdateNativeLayout
{
    public const uint HandlerFileOffset = 0x496680;
    public const uint ApplyFileOffset = 0x4936B0;
    public const int ConnectionStoredParametersOffset = 0x8E0;

    public static MatrixClockUpdateSelection Select(bool previousStoredUpperLimit,
        MatrixRunningClockUpdateSnapshot running, MatrixClockParameters incoming)
    {
        bool setLimit = incoming.HasUpperLimit && !running.HasUpperLimit;
        bool rebase = !incoming.HasUpperLimit && running.HasUpperLimit;
        var next = running;
        if (setLimit)
            next = next with { HasUpperLimit = true, UpperLimit = incoming.UpperLimitTickAtA8 };
        else if (rebase)
            next = next with
            {
                HasUpperLimit = false,
                Offset = unchecked(running.UpperLimit - incoming.InitialTickAt90)
            };
        return new(incoming, previousStoredUpperLimit != incoming.HasUpperLimit,
            setLimit, rebase, next, !running.Initialized, next.Offset != incoming.OffsetBitsAtB0);
    }
}
