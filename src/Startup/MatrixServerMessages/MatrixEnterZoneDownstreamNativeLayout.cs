namespace Firefall.Reconstruction.Startup;

public readonly record struct MatrixEnterZoneDownstreamPlan(
    MatrixClockSnapshot InitialClock, bool EmitRuntime110Paused,
    byte GlobalSubsystemInputByte, bool SetDownstreamByteAt18, bool AttemptMessage24);

/// <summary>
/// Proven selections after the zone-field copy. External subsystem calls, runtime
/// message delivery and socket submission are not simulated.
/// </summary>
public static class MatrixEnterZoneDownstreamNativeLayout
{
    public const int ConnectionClockHasUpperLimitOffset = 0x948;
    public const uint RuntimeMessageId = 110;
    public const string RuntimePropertyName = "paused";
    public const int RuntimeReceiverVirtualOffset = 0xAC;
    public const uint RuntimePropertyBuilderFileOffset = 0xC243E0;
    public const int GlobalSubsystemPointerOffset = 0xC64;
    public const int ConnectionSubsystemInputByteOffset = 0x8A0;
    public const uint GlobalSubsystemCallFileOffset = 0x386900;
    public const int ConnectionDownstreamPointerOffset = 0x910;
    public const int DownstreamRequiredZeroWordOffset = 0x28;
    public const int DownstreamActivatedByteOffset = 0x18;
    public const int ConnectionRequestMessage24ByteOffset = 0xB94;
    public const uint FindModeSpecificObjectFileOffset = 0x6F4540;
    public const int ModeSpecificWordOffset = 0x1E0;
    public const int ModeSpecificConditionalWordOffset = 0x1E4;
    public const int ModeSpecificConditionByteOffset = 0x1E8;

    public static int? FindModeObjectPointerOffset(int modeId) => modeId switch
    {
        (int)ClientAppModeId.Login => 0x14,
        (int)ClientAppModeId.World => 0x28E8,
        (int)ClientAppModeId.LookingGlass => 0x68,
        _ => null
    };

    public static MatrixEnterZoneDownstreamPlan CapturePlan(MatrixEnterZonePayload payload,
        bool hasGlobalSubsystem, bool hasDownstreamObject, uint downstreamWordAt28, byte requestMessage24Byte)
    {
        ArgumentNullException.ThrowIfNull(payload);
        if (!hasGlobalSubsystem)
            throw new ArgumentException("The native handler requires the global subsystem at +C64.",
                nameof(hasGlobalSubsystem));
        if (!hasDownstreamObject)
            throw new ArgumentException("The native handler requires connection +910.",
                nameof(hasDownstreamObject));
        if (downstreamWordAt28 != 0)
            throw new ArgumentOutOfRangeException(nameof(downstreamWordAt28),
                "The native handler asserts that downstream object +28 is zero.");
        var clock = MatrixClockNativeLayout.Initialize(payload.ClockParameters);
        return new(clock, clock.Parameters.HasUpperLimit, payload.Zone.ByteAt50, true, requestMessage24Byte != 0);
    }

    public static (uint WordAt1E0, uint WordAt1E4) CaptureModeSpecificWordUpdate(
        uint wordAt1E0, uint wordAt1E4, byte conditionByteAt1E8) =>
        (wordAt1E0 == 0 ? 1u : wordAt1E0,
            conditionByteAt1E8 != 0 && wordAt1E4 == 0 ? 1u : wordAt1E4);
}
