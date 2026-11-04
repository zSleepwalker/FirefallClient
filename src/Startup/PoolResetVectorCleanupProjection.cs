namespace Firefall.Reconstruction.Startup;

public readonly record struct PoolResetVectorCleanupCapture(
    uint VectorAddress,
    uint FirstFieldValue,
    bool? OpaqueHelperReturnedNormally);

public enum PoolResetVectorCleanupBoundary
{
    NullFirstField,
    OpaqueHelperCallBoundary,
    OpaqueHelperDidNotReturn,
    ZeroStoresReached
}

public readonly record struct PoolResetVectorCleanupProjection(
    PoolResetVectorCleanupCapture Capture,
    bool OpaqueHelperCallReached,
    uint? OpaqueHelperEcXValue,
    bool ZeroStoresReached,
    PoolResetVectorCleanupBoundary Boundary);

/// <summary>
/// Projects the callback's null gate and the conditional zero-store sequence
/// from captured values. It performs no memory reads, frees, ownership
/// operations, or reset traversal. The opaque helper is modeled only by whether
/// it returned normally; the zero-store result records instruction reachability,
/// not a readback of memory.
/// </summary>
public static class PoolResetVectorCleanupProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint CallbackRawFileOffset = 0x12C0920;
    public const uint CallbackVirtualAddress =
        CallbackRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;
    public const uint CallbackEndRawFileOffsetExclusive = 0x12C0949;
    public const uint CallbackSize =
        CallbackEndRawFileOffsetExclusive - CallbackRawFileOffset;

    public const uint RegistrationCallerRawFileOffset = 0x12C02F0;
    public const uint RegistrationCallBytesRawFileOffset = 0x12C0333;
    public const uint CallbackPointerPushRawFileOffset = 0x12C0336;
    public const int ResetReceiverThisOffset = 0x20;
    public const uint ResetReceiverRegisterMoveRawFileOffset = 0x12C033B;
    public const uint ResetCallRawFileOffset = 0x12C0340;
    public const uint ResetTargetRawFileOffset = 0x12CDC50;

    public const uint CallbackArgumentLoadRawFileOffset = 0x12C0924;
    public const int CallbackStackArgumentOffset = 8;
    public const int VectorFirstFieldOffset = 0;
    public const int VectorSecondFieldOffset = 4;
    public const int VectorThirdFieldOffset = 8;
    public const uint FirstFieldTestRawFileOffset = 0x12C0929;
    public const uint NullBranchRawFileOffset = 0x12C092B;
    public const uint NullBranchTargetRawFileOffset = 0x12C0946;
    public const uint OpaqueHelperCallRawFileOffset = 0x12C092D;
    public const uint OpaqueHelperTargetRawFileOffset = 0x12C8D60;
    public const uint FirstZeroStoreRawFileOffset = 0x12C0932;
    public const uint SecondZeroStoreRawFileOffset = 0x12C0938;
    public const uint ThirdZeroStoreRawFileOffset = 0x12C093F;
    public const uint NormalReturnRawFileOffset = 0x12C0948;
    public const int CallbackArgumentCount = 1;
    public const string CallbackCallingConvention = "cdecl";

    public const uint CallbackInvocationArgumentPushRawFileOffset = 0x12CDDD3;
    public const uint CallbackInvocationCallRawFileOffset = 0x12CDDD4;
    public const uint CallbackCallerStackCleanupRawFileOffset = 0x12CDDDa;
    public const int CallbackCallerStackCleanupBytes = 4;

    public const string CapturedRegistrationCallBytes =
        "8D 43 20 68 20 15 6C 01 8B C8 89 45 F0 E8 0B D9 00 00";

    public const string CapturedCallbackBytes =
        "55 8B EC 56 8B 75 08 8B 0E 85 C9 74 19 E8 2E 84 00 00 " +
        "C7 06 00 00 00 00 C7 46 04 00 00 00 00 C7 46 08 00 00 00 00 " +
        "5E 5D C3";

    public const string CapturedCallbackInvocationBytes =
        "51 FF 55 08 8B 55 CC 83 C4 04 FF 45 E8";

    public static PoolResetVectorCleanupProjection Project(
        PoolResetVectorCleanupCapture capture)
    {
        if (capture.FirstFieldValue == 0)
        {
            if (capture.OpaqueHelperReturnedNormally.HasValue)
            {
                throw new ArgumentException(
                    "The helper return outcome is not reached when the captured first field is null.",
                    nameof(capture));
            }

            return new PoolResetVectorCleanupProjection(
                capture,
                OpaqueHelperCallReached: false,
                OpaqueHelperEcXValue: null,
                ZeroStoresReached: false,
                PoolResetVectorCleanupBoundary.NullFirstField);
        }

        if (!capture.OpaqueHelperReturnedNormally.HasValue)
        {
            return new PoolResetVectorCleanupProjection(
                capture,
                OpaqueHelperCallReached: true,
                OpaqueHelperEcXValue: capture.FirstFieldValue,
                ZeroStoresReached: false,
                PoolResetVectorCleanupBoundary.OpaqueHelperCallBoundary);
        }

        if (!capture.OpaqueHelperReturnedNormally.Value)
        {
            return new PoolResetVectorCleanupProjection(
                capture,
                OpaqueHelperCallReached: true,
                OpaqueHelperEcXValue: capture.FirstFieldValue,
                ZeroStoresReached: false,
                PoolResetVectorCleanupBoundary.OpaqueHelperDidNotReturn);
        }

        return new PoolResetVectorCleanupProjection(
            capture,
            OpaqueHelperCallReached: true,
            OpaqueHelperEcXValue: capture.FirstFieldValue,
            ZeroStoresReached: true,
            PoolResetVectorCleanupBoundary.ZeroStoresReached);
    }
}
