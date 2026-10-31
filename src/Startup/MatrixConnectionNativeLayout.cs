namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Bounded Login-owned matrix connection and oracle-ticket request observations.
/// This is not a network implementation or a completed World-mode transition.
/// </summary>
public static class MatrixConnectionNativeLayout
{
    public const int AllocationSize = 0xBB0;
    public const uint ConstructorFileOffset = 0x48ED50;
    public const int StateOffset = 0x0C;
    public const int ErrorTextOffset = 0x10;
    public const int OracleRetryDelaySecondsOffset = 0x18;
    public const int OracleRequestInFlightFlagOffset = 0x1C;
    public const int OracleCallbackPairOffset = 0xB98;
    public const int PreferredMatrixTextOffset = 0xBA0;
    public const uint BeginConnectionMethodFileOffset = 0x494CA0;
    public const uint QueueOracleTicketCallSiteFileOffset = 0x494D0B;
    public const uint QueueOracleTicketMethodFileOffset = 0x494E30;
    public const uint EmptyTextPredicateMethodFileOffset = 0x2FCD00;
    public const uint ExistingConnectionResetMethodFileOffset = 0x490090;
    public const uint CharacterMetadataApplyMethodFileOffset = 0x490390;
    public const uint PreferredDestinationEmptyCheckCallSiteFileOffset = 0x494CF4;
    public const uint OracleMatrixFieldEmptyCheckCallSiteFileOffset = 0x495257;
    public const uint OracleCallbackAttachmentFileOffset = 0x49510C;
    public const uint OracleRequestSubmissionFileOffset = 0x4953DF;
    public const uint OracleResponseHandlerFileOffset = 0x495460;
    public const uint OracleResponseParserFileOffset = 0x4956C0;
    public const uint OpenMatrixConnectionMethodFileOffset = 0x495F20;
    public const int OracleCallbackAllocationSize = 0x10;
    public const int OracleCallbackOwnerPointerOffset = 0x04;
    public const int OracleCallbackTargetPointerOffset = 0x08;
    public const int OracleCallbackOwnerAdjustmentOffset = 0x0C;
    public const uint OracleCallbackTargetVirtualAddress = 0x896060;
    public const uint OracleCallbackVtableVirtualAddress = 0x1C641D8;
    public const uint OracleCallbackVtableFileOffset = 0x1862DD8;
    public const uint OracleCallbackTrampolineFileOffset = 0x496DF0;
    public const uint RequestConstructorFileOffset = 0x3233B0;
    public const int OracleRequestType = 2;
    public const int MaximumOracleRetryDelaySeconds = 300;
    public const int MatrixTransportObjectPointerOffset = 0x820;
    public const uint TransportHandlerVtableVirtualAddress = 0x1C642D0;
    public const uint TransportHandlerVtableFileOffset = 0x1862ED0;
    public const uint ReceiveChannel1MethodFileOffset = 0x4904A0;
    public const uint ReceiveOtherChannelsMethodFileOffset = 0x4904C0;
    public const uint TransportDisconnectedMethodFileOffset = 0x490540;
    public const int DownstreamHandlerPointerOffset = 0x910;
    public const int MatrixTransportAllocationSize = 0xDF8;
    public const uint MatrixTransportConstructorFileOffset = 0xBD00B0;
    public const uint MatrixTransportOpenMethodFileOffset = 0xBD0960;
    public const int StateAfterTransportOpen = 3;
    public const string EndpointFormat = "%s/api/v1/oracle/ticket";
    public const string RequestContentType = "application/json";

    public static IReadOnlyList<string> KnownResponseKeys { get; } =
        Array.AsReadOnly(new[]
        {
            "matrix_url", "ticket", "datacenter", "operator_override", "session_id", "hostname"
        });

    public static IReadOnlyList<string> KnownRequestBodyKeys { get; } =
        Array.AsReadOnly(new[] { "ver", "build", "matrix" });

    public static int NextOracleRetryDelaySeconds(int currentDelaySeconds)
    {
        if (currentDelaySeconds is < 0 or > MaximumOracleRetryDelaySeconds)
            throw new ArgumentOutOfRangeException(nameof(currentDelaySeconds),
                "Only the observed nonnegative, capped retry-delay range is reconstructed.");

        return Math.Min(MaximumOracleRetryDelaySeconds,
            currentDelaySeconds + 2 + (currentDelaySeconds + 1) / 2);
    }
}
