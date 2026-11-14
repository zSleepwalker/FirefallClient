using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct StartupHelper12CC2B0Capture(
    uint ThisPointer,
    ImmutableArray<uint> CapturedPopResults,
    ImmutableArray<bool?> CapturedBackendNormalReturns);

public enum StartupHelper12CC2B0ActionKind
{
    CallPop,
    CallBackendForCapturedPop,
    CallBackendForOriginalThis,
    ReturnNormallyWithOriginalThis
}

public readonly record struct StartupHelper12CC2B0Action(
    StartupHelper12CC2B0ActionKind Kind,
    uint InstructionRawFileOffset,
    uint? Argument = null,
    uint? TargetRawFileOffset = null);

public enum StartupHelper12CC2B0Boundary
{
    IncompleteAtPopResult,
    IncompleteAtBackendReturn,
    StoppedAtNonReturningBackend,
    ReturnedNormally
}

public readonly record struct StartupHelper12CC2B0Plan(
    StartupHelper12CC2B0Capture Capture,
    ImmutableArray<StartupHelper12CC2B0Action> Actions,
    StartupHelper12CC2B0Boundary Boundary,
    uint? ReturnedEax,
    byte? ReturnStackPopBytes);

/// <summary>
/// Projects the captured pop/backend loop at raw 0x12CC2B0 without assigning
/// an owner or inferring the called helpers' effects.
/// </summary>
public static class StartupHelper12CC2B0Projection
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint EntryRawFileOffset = 0x12CC2B0;
    public const uint EntryEndRawFileOffsetExclusive = 0x12CC2E1;
    public const uint InitialPopCallRawFileOffset = 0x12CC2B7;
    public const uint RepeatedPopCallRawFileOffset = 0x12CC2CA;
    public const uint CapturedPopBackendCallRawFileOffset = 0x12CC2C2;
    public const uint OriginalThisBackendCallRawFileOffset = 0x12CC2D5;
    public const uint ReturnValueSetupRawFileOffset = 0x12CC2DA;
    public const uint ReturnRawFileOffset = 0x12CC2DE;
    public const uint PoolPopHelperRawFileOffset = 0x12CDAE0;
    public const uint BackendHelperRawFileOffset = 0x12C8D60;
    public const uint PopReceiverOffset = 8;
    public const byte ReturnStackPopBytesValue = 4;

    public const string FunctionBytes =
        "56 57 8B F9 8D 4F 08 E8 24 18 00 00 85 C0 74 13 " +
        "8B C8 E8 99 CA FF FF 8D 4F 08 E8 11 18 00 00 " +
        "85 C0 75 ED 8B CF E8 86 CA FF FF 8B C7 5F 5E C2 04 00";

    /// <summary>
    /// Captures one result for each reached pop and one normal-return capture
    /// for each reached backend call. Missing captures and non-returning calls
    /// stop at their corresponding boundary.
    /// </summary>
    public static StartupHelper12CC2B0Plan Project(
        StartupHelper12CC2B0Capture capture)
    {
        if (capture.CapturedPopResults.IsDefault
            || capture.CapturedBackendNormalReturns.IsDefault)
        {
            throw new ArgumentException(
                "Captured result arrays must be initialized.",
                nameof(capture));
        }

        var actions = ImmutableArray.CreateBuilder<StartupHelper12CC2B0Action>();
        uint popReceiver = unchecked(capture.ThisPointer + PopReceiverOffset);
        int popIndex = 0;
        int backendIndex = 0;

        StartupHelper12CC2B0Plan Stop(
            StartupHelper12CC2B0Boundary boundary,
            uint? returnedEax)
        {
            if (popIndex != capture.CapturedPopResults.Length
                || backendIndex != capture.CapturedBackendNormalReturns.Length)
            {
                throw new ArgumentException(
                    "The capture contains results beyond the projected boundary.",
                    nameof(capture));
            }

            return new StartupHelper12CC2B0Plan(
                capture,
                actions.ToImmutable(),
                boundary,
                returnedEax,
                boundary == StartupHelper12CC2B0Boundary.ReturnedNormally
                    ? ReturnStackPopBytesValue
                    : null);
        }

        while (true)
        {
            uint popCall = popIndex == 0
                ? InitialPopCallRawFileOffset
                : RepeatedPopCallRawFileOffset;
            actions.Add(new StartupHelper12CC2B0Action(
                StartupHelper12CC2B0ActionKind.CallPop,
                popCall,
                popReceiver,
                PoolPopHelperRawFileOffset));

            if (popIndex >= capture.CapturedPopResults.Length)
            {
                return Stop(
                    StartupHelper12CC2B0Boundary.IncompleteAtPopResult,
                    returnedEax: null);
            }

            uint popResult = capture.CapturedPopResults[popIndex++];
            bool isEmpty = popResult == 0;
            uint backendCall = isEmpty
                ? OriginalThisBackendCallRawFileOffset
                : CapturedPopBackendCallRawFileOffset;
            actions.Add(new StartupHelper12CC2B0Action(
                isEmpty
                    ? StartupHelper12CC2B0ActionKind.CallBackendForOriginalThis
                    : StartupHelper12CC2B0ActionKind.CallBackendForCapturedPop,
                backendCall,
                isEmpty ? capture.ThisPointer : popResult,
                BackendHelperRawFileOffset));

            bool? backendReturnedNormally =
                backendIndex < capture.CapturedBackendNormalReturns.Length
                    ? capture.CapturedBackendNormalReturns[backendIndex++]
                    : null;
            if (!backendReturnedNormally.HasValue)
            {
                return Stop(
                    StartupHelper12CC2B0Boundary.IncompleteAtBackendReturn,
                    returnedEax: null);
            }

            if (!backendReturnedNormally.Value)
            {
                return Stop(
                    StartupHelper12CC2B0Boundary.StoppedAtNonReturningBackend,
                    returnedEax: null);
            }

            if (isEmpty)
            {
                actions.Add(new StartupHelper12CC2B0Action(
                    StartupHelper12CC2B0ActionKind.ReturnNormallyWithOriginalThis,
                    ReturnValueSetupRawFileOffset,
                    capture.ThisPointer));
                return Stop(
                    StartupHelper12CC2B0Boundary.ReturnedNormally,
                    capture.ThisPointer);
            }
        }
    }
}
