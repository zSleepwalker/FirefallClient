using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolCandidateCapture(
    uint OutputPairPointer,
    uint CandidatePointer);

public readonly record struct UiShaderSourcePoolCandidateHeaderFieldWrite(
    int Offset,
    uint Value);

public readonly record struct UiShaderSourcePoolCandidateWrapperArguments(
    uint OutputPairPointer,
    uint HeaderPointer,
    byte Flag);

public readonly record struct UiShaderSourcePoolCandidatePlan(
    UiShaderSourcePoolCandidateCapture Capture,
    ImmutableArray<UiShaderSourcePoolCandidateHeaderFieldWrite> HeaderFieldWrites,
    uint WrapperCallRawFileOffset,
    UiShaderSourcePoolCandidateWrapperArguments WrapperArguments);

/// <summary>
/// Projects the caller's captured pool-candidate result, ordered header stores,
/// and wrapper arguments. A zero candidate follows the explicit null branch;
/// this does not model a retry, helper internals, or a guaranteed nonnull result.
/// </summary>
public static class UiShaderSourcePoolCandidateProjectionRules
{
    public const uint MaximumPoolRequestSize = 0x800;
    public const uint RequiredPoolAlignment = 0;

    public const uint CandidateAcquisitionRawFileOffset = 0x12CBF94;
    public const uint CandidateNullTestRawFileOffset = 0x12CBF99;
    public const uint CandidateNullBranchRawFileOffset = 0x12CBF9B;
    public const uint CandidateNullBranchTargetRawFileOffset = 0x12CBFD9;

    public const uint HeaderDataPointerSetupRawFileOffset = 0x12CBF9D;
    public const uint HeaderFlagZeroStoreRawFileOffset = 0x12CBFA0;
    public const uint HeaderDataPointerStoreRawFileOffset = 0x12CBFA7;
    public const uint HeaderDisplacementStoreRawFileOffset = 0x12CBFAD;
    public const uint HeaderCapacityStoreRawFileOffset = 0x12CBFB4;
    public const uint HeaderLengthStoreRawFileOffset = 0x12CBFBA;
    public const uint NonNullWrapperCallRawFileOffset = 0x12CBFC1;
    public const uint NullWrapperCallRawFileOffset = 0x12CBFE4;

    public const string CandidateAcquisitionBytes = "E8 C7 1E 00 00";
    public const string CandidateNullTestBytes = "85 C0";
    public const string CandidateNullBranchBytes = "74 3C";
    public const string CandidateNullBranchTargetBytes = "33 C0";
    public const string HeaderDataPointerSetupBytes = "8D 48 10";
    public const string HeaderFlagZeroStoreBytes = "C6 45 EC 00";
    public const string HeaderDataPointerStoreBytes = "89 48 04";
    public const string HeaderDisplacementStoreBytes = "C7 40 08 00 00 00 00";
    public const string HeaderCapacityStoreBytes = "C7 00 00 08 00 00";
    public const string HeaderLengthStoreBytes = "C7 40 0C 00 00 00 00";
    public const string NonNullWrapperCallBytes = "E8 DA 04 00 00";
    public const string NullWrapperFlagSetupBytes = "33 C0 8B CE 88 45 EC";
    public const string NullWrapperCallBytes = "E8 B7 04 00 00";

    public const int HeaderCapacityOffset = 0x00;
    public const int HeaderDataPointerOffset = 0x04;
    public const int HeaderAlignmentDisplacementOffset = 0x08;
    public const int HeaderLogicalLengthOffset = 0x0C;
    public const uint HeaderDataPointerDisplacement = 0x10;
    public const uint HeaderCapacity = 0x800;
    public const byte WrapperFlag = 0;

    public static UiShaderSourcePoolCandidatePlan Project(
        UiShaderSourcePoolCandidateCapture capture)
    {
        bool hasCandidate = capture.CandidatePointer != 0;
        ImmutableArray<UiShaderSourcePoolCandidateHeaderFieldWrite> fieldWrites =
            hasCandidate
                ? ImmutableArray.Create(
                    new UiShaderSourcePoolCandidateHeaderFieldWrite(
                        HeaderDataPointerOffset,
                        unchecked(capture.CandidatePointer + HeaderDataPointerDisplacement)),
                    new UiShaderSourcePoolCandidateHeaderFieldWrite(
                        HeaderAlignmentDisplacementOffset,
                        0),
                    new UiShaderSourcePoolCandidateHeaderFieldWrite(
                        HeaderCapacityOffset,
                        HeaderCapacity),
                    new UiShaderSourcePoolCandidateHeaderFieldWrite(
                        HeaderLogicalLengthOffset,
                        0))
                : ImmutableArray<UiShaderSourcePoolCandidateHeaderFieldWrite>.Empty;

        return new UiShaderSourcePoolCandidatePlan(
            capture,
            fieldWrites,
            hasCandidate ? NonNullWrapperCallRawFileOffset : NullWrapperCallRawFileOffset,
            new UiShaderSourcePoolCandidateWrapperArguments(
                capture.OutputPairPointer,
                capture.CandidatePointer,
                WrapperFlag));
    }
}
