using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolCandidateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Pool-candidate caller gate, acquisition, branches, stores, and wrapper calls retain the captured bytes",
            UiShaderSourcePoolCandidateProjectionRules.MaximumPoolRequestSize == 0x800
            && UiShaderSourcePoolCandidateProjectionRules.RequiredPoolAlignment == 0
            && UiShaderSourcePoolCandidateProjectionRules.CandidateAcquisitionRawFileOffset == 0x12CBF94
            && UiShaderSourcePoolCandidateProjectionRules.CandidateAcquisitionBytes == "E8 C7 1E 00 00"
            && UiShaderSourcePoolCandidateProjectionRules.CandidateNullTestRawFileOffset == 0x12CBF99
            && UiShaderSourcePoolCandidateProjectionRules.CandidateNullTestBytes == "85 C0"
            && UiShaderSourcePoolCandidateProjectionRules.CandidateNullBranchRawFileOffset == 0x12CBF9B
            && UiShaderSourcePoolCandidateProjectionRules.CandidateNullBranchBytes == "74 3C"
            && UiShaderSourcePoolCandidateProjectionRules.CandidateNullBranchTargetRawFileOffset == 0x12CBFD9
            && UiShaderSourcePoolCandidateProjectionRules.CandidateNullBranchTargetBytes == "33 C0"
            && UiShaderSourcePoolCandidateProjectionRules.HeaderDataPointerSetupRawFileOffset == 0x12CBF9D
            && UiShaderSourcePoolCandidateProjectionRules.HeaderDataPointerSetupBytes == "8D 48 10"
            && UiShaderSourcePoolCandidateProjectionRules.HeaderFlagZeroStoreRawFileOffset == 0x12CBFA0
            && UiShaderSourcePoolCandidateProjectionRules.HeaderFlagZeroStoreBytes == "C6 45 EC 00"
            && UiShaderSourcePoolCandidateProjectionRules.HeaderDataPointerStoreRawFileOffset == 0x12CBFA7
            && UiShaderSourcePoolCandidateProjectionRules.HeaderDataPointerStoreBytes == "89 48 04"
            && UiShaderSourcePoolCandidateProjectionRules.HeaderDisplacementStoreRawFileOffset == 0x12CBFAD
            && UiShaderSourcePoolCandidateProjectionRules.HeaderDisplacementStoreBytes == "C7 40 08 00 00 00 00"
            && UiShaderSourcePoolCandidateProjectionRules.HeaderCapacityStoreRawFileOffset == 0x12CBFB4
            && UiShaderSourcePoolCandidateProjectionRules.HeaderCapacityStoreBytes == "C7 00 00 08 00 00"
            && UiShaderSourcePoolCandidateProjectionRules.HeaderLengthStoreRawFileOffset == 0x12CBFBA
            && UiShaderSourcePoolCandidateProjectionRules.HeaderLengthStoreBytes == "C7 40 0C 00 00 00 00"
            && UiShaderSourcePoolCandidateProjectionRules.NonNullWrapperCallRawFileOffset == 0x12CBFC1
            && UiShaderSourcePoolCandidateProjectionRules.NonNullWrapperCallBytes == "E8 DA 04 00 00"
            && UiShaderSourcePoolCandidateProjectionRules.NullWrapperCallRawFileOffset == 0x12CBFE4
            && UiShaderSourcePoolCandidateProjectionRules.NullWrapperFlagSetupBytes == "33 C0 8B CE 88 45 EC"
            && UiShaderSourcePoolCandidateProjectionRules.NullWrapperCallBytes == "E8 B7 04 00 00"
            && UiShaderSourcePoolCandidateProjectionRules.HeaderCapacityOffset == 0
            && UiShaderSourcePoolCandidateProjectionRules.HeaderDataPointerOffset == 4
            && UiShaderSourcePoolCandidateProjectionRules.HeaderAlignmentDisplacementOffset == 8
            && UiShaderSourcePoolCandidateProjectionRules.HeaderLogicalLengthOffset == 0x0C
            && UiShaderSourcePoolCandidateProjectionRules.HeaderDataPointerDisplacement == 0x10
            && UiShaderSourcePoolCandidateProjectionRules.HeaderCapacity == 0x800
            && UiShaderSourcePoolCandidateProjectionRules.WrapperFlag == 0);

        const uint outputPairPointer = 0x12345678;
        UiShaderSourcePoolCandidatePlan nullCandidate =
            UiShaderSourcePoolCandidateProjectionRules.Project(
                new UiShaderSourcePoolCandidateCapture(outputPairPointer, 0));
        check("A captured null candidate records the explicit wrapper branch without header stores",
            nullCandidate.HeaderFieldWrites.IsEmpty
            && nullCandidate.WrapperCallRawFileOffset
                == UiShaderSourcePoolCandidateProjectionRules.NullWrapperCallRawFileOffset
            && nullCandidate.WrapperArguments
                == new UiShaderSourcePoolCandidateWrapperArguments(outputPairPointer, 0, 0));

        UiShaderSourcePoolCandidatePlan candidate =
            UiShaderSourcePoolCandidateProjectionRules.Project(
                new UiShaderSourcePoolCandidateCapture(outputPairPointer, 0x20000000));
        check("A nonnull candidate records each captured header write in instruction order and forwards the wrapper arguments",
            candidate.HeaderFieldWrites.SequenceEqual(
            [
                new UiShaderSourcePoolCandidateHeaderFieldWrite(4, 0x20000010),
                new UiShaderSourcePoolCandidateHeaderFieldWrite(8, 0),
                new UiShaderSourcePoolCandidateHeaderFieldWrite(0, 0x800),
                new UiShaderSourcePoolCandidateHeaderFieldWrite(0x0C, 0)
            ])
            && candidate.WrapperCallRawFileOffset
                == UiShaderSourcePoolCandidateProjectionRules.NonNullWrapperCallRawFileOffset
            && candidate.WrapperArguments
                == new UiShaderSourcePoolCandidateWrapperArguments(
                    outputPairPointer,
                    0x20000000,
                    0));

        UiShaderSourcePoolCandidatePlan maximumPointer =
            UiShaderSourcePoolCandidateProjectionRules.Project(
                new UiShaderSourcePoolCandidateCapture(uint.MaxValue, uint.MaxValue));
        check("Maximum captured pointers preserve unchecked 32-bit data-pointer addition",
            maximumPointer.HeaderFieldWrites.Length == 4
            && maximumPointer.HeaderFieldWrites[0]
                == new UiShaderSourcePoolCandidateHeaderFieldWrite(4, 0x0F)
            && maximumPointer.WrapperArguments
                == new UiShaderSourcePoolCandidateWrapperArguments(
                    uint.MaxValue,
                    uint.MaxValue,
                    0));

        UiShaderSourcePoolCandidatePlan nearMaximumPointer =
            UiShaderSourcePoolCandidateProjectionRules.Project(
                new UiShaderSourcePoolCandidateCapture(0xAABBCCDD, uint.MaxValue - 7));
        check("Candidate data pointer addition wraps without changing the forwarded candidate pointer",
            nearMaximumPointer.HeaderFieldWrites[0]
                == new UiShaderSourcePoolCandidateHeaderFieldWrite(4, 8)
            && nearMaximumPointer.WrapperArguments
                == new UiShaderSourcePoolCandidateWrapperArguments(
                    0xAABBCCDD,
                    uint.MaxValue - 7,
                    0));

        UiShaderSourcePoolCandidatePlan repeated =
            UiShaderSourcePoolCandidateProjectionRules.Project(
                new UiShaderSourcePoolCandidateCapture(outputPairPointer, 0x20000000));
        check("Repeated projections return independent immutable write arrays",
            !candidate.HeaderFieldWrites.Equals(repeated.HeaderFieldWrites)
            && !candidate.HeaderFieldWrites.IsDefault
            && !repeated.HeaderFieldWrites.IsDefault
            && candidate.HeaderFieldWrites.SequenceEqual(repeated.HeaderFieldWrites));
    }
}
