namespace Firefall.Reconstruction.Startup;

/// <summary>Compatibility entry for recovered mode0 positive-length byte effects; no generated machine-code execution.</summary>
public static class NativeGeneratedInitialModeZeroByteTransform
{
    public static byte[] Forward(ReadOnlySpan<byte> input, NativeGeneratedInitialModeZeroSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return NativeGeneratedDefaultHeaderByteTransform.ProjectRecoveredSteps(input, snapshot.Prefix, snapshot.ForwardByteSteps, nameof(snapshot));
    }

    public static byte[] Inverse(ReadOnlySpan<byte> input, NativeGeneratedInitialModeZeroSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return NativeGeneratedDefaultHeaderByteTransform.ProjectRecoveredSteps(input, snapshot.Prefix, snapshot.InverseByteSteps, nameof(snapshot));
    }
}
