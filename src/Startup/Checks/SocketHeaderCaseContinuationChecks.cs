using System.Buffers.Binary;
using Firefall.Reconstruction.Startup;

internal static class SocketHeaderCaseContinuationChecks
{
    public static void Run(Action<string, bool> check)
    {
        SocketHeaderCaseContinuationCase case1 = SocketHeaderCaseContinuationCase.Case1;
        SocketHeaderCaseContinuationCase case2 = SocketHeaderCaseContinuationCase.Case2;
        SocketHeaderCaseContinuationCase case3 = SocketHeaderCaseContinuationCase.Case3;

        check("All three continuations pin the sample, raw anchors, targets, and raw-to-VA mapping",
            SocketHeaderCaseContinuationProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && SocketHeaderCaseContinuationProjection.CodeVirtualAddressDeltaFromRawFileOffset == 0x00400C00
            && AnchorsMatch(
                case1,
                0x00BD415A, 0x00BD4161, 0x00BD5BA0, 0x00BD5BC2,
                0x00BD4166, 0x00BD4168, 0x00BE51F0, 0x00BD416D, 0x00BD416F, 0x00BD4175,
                0x00FD4D66, 0x00FD4D68, 0x00FE5DF0)
            && AnchorsMatch(
                case2,
                0x00BD418D, 0x00BD4194, 0x00BD5BA0, 0x00BD5BC2,
                0x00BD4199, 0x00BD419B, 0x00BE51F0, 0x00BD41A0, 0x00BD41A2, 0x00BD41A8,
                0x00FD4D99, 0x00FD4D9B, 0x00FE5DF0)
            && AnchorsMatch(
                case3,
                0x00BD41C0, 0x00BD41C7, 0x00BD5C00, 0x00BD5C22,
                0x00BD41CC, 0x00BD41CE, 0x00BE5A40, 0x00BD41D3, 0x00BD41D5, 0x00BD41DB,
                0x00FD4DCC, 0x00FD4DCE, 0x00FE6640)
            && SocketHeaderCaseContinuationProjection.RawFileOffsetToVirtualAddress(
                SocketHeaderCaseContinuationProjection.BranchTakenDestinationRawFileOffset) == 0x00FD4A60
            && SocketHeaderCaseContinuationProjection.RawFileOffsetToVirtualAddress(uint.MaxValue)
                == 0x00400BFF);

        check("The getter return, MOV ECX,EAX, direct calls, TEST AL,AL, and JNZ bytes match each route",
            InstructionsMatch(case1,
                "E8 3A 1A 00 00", "E8 83 10 01 00", "0F 85 EB FC FF FF",
                0x00BD3E60, 0x00FD4A60)
            && InstructionsMatch(case2,
                "E8 07 1A 00 00", "E8 50 10 01 00", "0F 85 B8 FC FF FF",
                0x00BD3E60, 0x00FD4A60)
            && InstructionsMatch(case3,
                "E8 34 1A 00 00", "E8 6D 18 01 00", "0F 85 85 FC FF FF",
                0x00BD3E60, 0x00FD4A60));

        check("Each plain getter RET leaves the earlier pushed EDI present before the next direct call",
            StackAndCallSequenceMatch(case1, "E8 3A 1A 00 00", "E8 83 10 01 00")
            && StackAndCallSequenceMatch(case2, "E8 07 1A 00 00", "E8 50 10 01 00")
            && StackAndCallSequenceMatch(case3, "E8 34 1A 00 00", "E8 6D 18 01 00"));

        check("Every unsigned AL value selects exactly its TEST/JNZ destination on all three routes",
            AllAlValuesSelectCorrectDestination(case1)
            && AllAlValuesSelectCorrectDestination(case2)
            && AllAlValuesSelectCorrectDestination(case3));

        SocketHeaderCaseContinuationPlan missingGetter =
            SocketHeaderCaseContinuationProjection.Project(
                new(case1, GetterEax: null, OpaqueCallReturnAl: 0xFF, PushedEdiValue: 0xA1B2C3D4));
        check("Without a captured normal getter return, neither ECX nor a later AL branch is inferred",
            missingGetter.Outcome == SocketHeaderCaseContinuationOutcome.UnreachedOpaqueReturnAlCapture
            && !missingGetter.NextCallReached
            && missingGetter.GetterEax is null
            && missingGetter.NextCallEcX is null
            && missingGetter.OpaqueCallReturnAl is null
            && missingGetter.BranchTaken is null
            && missingGetter.SelectedBranchDestinationRawFileOffset is null
            && !missingGetter.PushedEdiStackEntryRemainsBeforeNextCall
            && missingGetter.PreexistingPushedEdiValue == 0xA1B2C3D4
            && missingGetter.StopAtRawFileOffsetExclusive == 0x00BD4161);

        SocketHeaderCaseContinuationPlan zeroEaxMissingAl =
            SocketHeaderCaseContinuationProjection.Project(
                new(case2, GetterEax: 0, OpaqueCallReturnAl: null, PushedEdiValue: 0xDEADBEEF));
        check("A captured zero EAX is still passed in ECX, while a missing AL capture stops before the branch",
            zeroEaxMissingAl.Outcome == SocketHeaderCaseContinuationOutcome.MissingOpaqueReturnAlCapture
            && zeroEaxMissingAl.NextCallReached
            && zeroEaxMissingAl.GetterEax == 0
            && zeroEaxMissingAl.NextCallEcX == 0
            && zeroEaxMissingAl.OpaqueCallReturnAl is null
            && zeroEaxMissingAl.OpaqueCallReturnAlNonZero is null
            && zeroEaxMissingAl.SelectedBranchDestinationRawFileOffset is null
            && zeroEaxMissingAl.PushedEdiStackEntryRemainsBeforeNextCall
            && zeroEaxMissingAl.PreexistingPushedEdiValue == 0xDEADBEEF
            && zeroEaxMissingAl.StopAtRawFileOffsetExclusive == 0x00BD41A0);

        check("Case selection rejects values outside the three supported routes",
            ThrowsArgumentOutOfRangeException(
                () => SocketHeaderCaseContinuationProjection.Project(
                    new((SocketHeaderCaseContinuationCase)0, 0, 0))));
    }

    private static bool AnchorsMatch(
        SocketHeaderCaseContinuationCase @case,
        uint pushedEdi,
        uint getterCall,
        uint getter,
        uint getterPlainRet,
        uint postGetter,
        uint nextCall,
        uint nextCallTarget,
        uint alTest,
        uint branch,
        uint fallthrough,
        uint postGetterVirtualAddress,
        uint nextCallVirtualAddress,
        uint nextCallTargetVirtualAddress) =>
        SocketHeaderCaseContinuationProjection.PushedEdiRawFileOffset(@case) == pushedEdi
        && SocketHeaderCaseContinuationProjection.GetterCallRawFileOffset(@case) == getterCall
        && SocketHeaderCaseContinuationProjection.GetterRawFileOffset(@case) == getter
        && SocketHeaderCaseContinuationProjection.GetterPlainRetRawFileOffset(@case) == getterPlainRet
        && SocketHeaderCaseContinuationProjection.PostGetterRawFileOffset(@case) == postGetter
        && SocketHeaderCaseContinuationProjection.NextCallRawFileOffset(@case) == nextCall
        && SocketHeaderCaseContinuationProjection.NextCallTargetRawFileOffset(@case) == nextCallTarget
        && SocketHeaderCaseContinuationProjection.AlTestRawFileOffset(@case) == alTest
        && SocketHeaderCaseContinuationProjection.BranchRawFileOffset(@case) == branch
        && SocketHeaderCaseContinuationProjection.BranchFallthroughRawFileOffset(@case) == fallthrough
        && SocketHeaderCaseContinuationProjection.RawFileOffsetToVirtualAddress(postGetter)
            == postGetterVirtualAddress
        && SocketHeaderCaseContinuationProjection.RawFileOffsetToVirtualAddress(nextCall)
            == nextCallVirtualAddress
        && SocketHeaderCaseContinuationProjection.RawFileOffsetToVirtualAddress(nextCallTarget)
            == nextCallTargetVirtualAddress
        && SocketHeaderCaseContinuationProjection.BranchTakenDestinationRawFileOffset == 0x00BD3E60;

    private static bool InstructionsMatch(
        SocketHeaderCaseContinuationCase @case,
        string getterCallBytes,
        string nextCallBytes,
        string branchBytes,
        uint branchTargetRaw,
        uint branchTargetVirtualAddress)
    {
        var instructions = SocketHeaderCaseContinuationProjection.Instructions(@case);
        return instructions.Select(instruction =>
                (instruction.RawFileOffset, instruction.Bytes,
                    instruction.BranchTargetRawFileOffset, instruction.TargetRawFileOffset,
                    instruction.TargetVirtualAddress))
            .SequenceEqual(new (
                uint RawFileOffset,
                string Bytes,
                uint? BranchTargetRawFileOffset,
                uint? TargetRawFileOffset,
                uint? TargetVirtualAddress)[]
            {
                (SocketHeaderCaseContinuationProjection.PushedEdiRawFileOffset(@case),
                    "57", null, null, null),
                (SocketHeaderCaseContinuationProjection.GetterCallRawFileOffset(@case),
                    getterCallBytes, null,
                    SocketHeaderCaseContinuationProjection.GetterRawFileOffset(@case),
                    SocketHeaderCaseContinuationProjection.RawFileOffsetToVirtualAddress(
                        SocketHeaderCaseContinuationProjection.GetterRawFileOffset(@case))),
                (SocketHeaderCaseContinuationProjection.GetterPlainRetRawFileOffset(@case),
                    "C3", null, null, null),
                (SocketHeaderCaseContinuationProjection.PostGetterRawFileOffset(@case),
                    "8B C8", null, null, null),
                (SocketHeaderCaseContinuationProjection.NextCallRawFileOffset(@case),
                    nextCallBytes, null,
                    SocketHeaderCaseContinuationProjection.NextCallTargetRawFileOffset(@case),
                    SocketHeaderCaseContinuationProjection.RawFileOffsetToVirtualAddress(
                        SocketHeaderCaseContinuationProjection.NextCallTargetRawFileOffset(@case))),
                (SocketHeaderCaseContinuationProjection.AlTestRawFileOffset(@case),
                    "84 C0", null, null, null),
                (SocketHeaderCaseContinuationProjection.BranchRawFileOffset(@case),
                    branchBytes, branchTargetRaw, null, branchTargetVirtualAddress)
            })
            && RelativeCallTargets(
                SocketHeaderCaseContinuationProjection.GetterCallRawFileOffset(@case),
                getterCallBytes,
                SocketHeaderCaseContinuationProjection.GetterRawFileOffset(@case))
            && RelativeCallTargets(
                SocketHeaderCaseContinuationProjection.NextCallRawFileOffset(@case),
                nextCallBytes,
                SocketHeaderCaseContinuationProjection.NextCallTargetRawFileOffset(@case))
            && RelativeConditionalBranchTargets(
                SocketHeaderCaseContinuationProjection.BranchRawFileOffset(@case),
                branchBytes,
                branchTargetRaw)
            && SocketHeaderCaseContinuationProjection.BranchFallthroughRawFileOffset(@case)
                == SocketHeaderCaseContinuationProjection.BranchRawFileOffset(@case) + 6;
    }

    private static bool StackAndCallSequenceMatch(
        SocketHeaderCaseContinuationCase @case,
        string getterCallBytes,
        string nextCallBytes)
    {
        uint getterCall = SocketHeaderCaseContinuationProjection.GetterCallRawFileOffset(@case);
        uint postGetter = SocketHeaderCaseContinuationProjection.PostGetterRawFileOffset(@case);
        uint nextCall = SocketHeaderCaseContinuationProjection.NextCallRawFileOffset(@case);
        return getterCall + 5 == postGetter
            && postGetter + 2 == nextCall
            && getterCallBytes.StartsWith("E8 ", StringComparison.Ordinal)
            && nextCallBytes.StartsWith("E8 ", StringComparison.Ordinal)
            && SocketHeaderCaseContinuationProjection.GetterPlainReturnBytes == "C3"
            && SocketHeaderCaseContinuationProjection.MoveGetterEaxToEcxBytes == "8B C8"
            && SocketHeaderCaseContinuationProjection.Instructions(@case)[0].Bytes == "57";
    }

    private static bool AllAlValuesSelectCorrectDestination(
        SocketHeaderCaseContinuationCase @case)
    {
        for (int value = byte.MinValue; value <= byte.MaxValue; value++)
        {
            byte al = (byte)value;
            bool branchTaken = al != 0;
            uint expectedDestination = branchTaken
                ? SocketHeaderCaseContinuationProjection.BranchTakenDestinationRawFileOffset
                : SocketHeaderCaseContinuationProjection.BranchFallthroughRawFileOffset(@case);
            uint getterEax = value == 0 ? 0u : unchecked((uint)(0xA0000000u + (uint)value));
            const uint pushedEdi = 0x76543210;
            SocketHeaderCaseContinuationPlan plan =
                SocketHeaderCaseContinuationProjection.Project(
                    new(@case, getterEax, al, pushedEdi));

            if (plan.Outcome != SocketHeaderCaseContinuationOutcome.BranchSelected
                || !plan.NextCallReached
                || plan.GetterEax != getterEax
                || plan.NextCallEcX != getterEax
                || plan.OpaqueCallReturnAl != al
                || plan.OpaqueCallReturnAlNonZero != branchTaken
                || plan.BranchTaken != branchTaken
                || plan.SelectedBranchDestinationRawFileOffset != expectedDestination
                || plan.SelectedBranchDestinationVirtualAddress
                    != SocketHeaderCaseContinuationProjection.RawFileOffsetToVirtualAddress(expectedDestination)
                || plan.StopAtRawFileOffsetExclusive != expectedDestination
                || !plan.PushedEdiStackEntryRemainsBeforeNextCall
                || plan.PreexistingPushedEdiValue != pushedEdi)
                return false;
        }

        return true;
    }

    private static bool RelativeCallTargets(uint instructionRaw, string bytes, uint expectedTarget)
    {
        byte[] encoded = Convert.FromHexString(bytes.Replace(" ", string.Empty, StringComparison.Ordinal));
        int displacement = BinaryPrimitives.ReadInt32LittleEndian(encoded.AsSpan(1, 4));
        return encoded.Length == 5
            && encoded[0] == 0xE8
            && unchecked(instructionRaw + 5 + (uint)displacement) == expectedTarget;
    }

    private static bool RelativeConditionalBranchTargets(
        uint instructionRaw,
        string bytes,
        uint expectedTarget)
    {
        byte[] encoded = Convert.FromHexString(bytes.Replace(" ", string.Empty, StringComparison.Ordinal));
        int displacement = BinaryPrimitives.ReadInt32LittleEndian(encoded.AsSpan(2, 4));
        return encoded.Length == 6
            && encoded[0] == 0x0F
            && encoded[1] == 0x85
            && unchecked(instructionRaw + 6 + (uint)displacement) == expectedTarget;
    }

    private static bool ThrowsArgumentOutOfRangeException(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }
}
