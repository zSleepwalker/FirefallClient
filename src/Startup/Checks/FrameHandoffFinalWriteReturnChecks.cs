using Firefall.Reconstruction.Startup;

internal static class FrameHandoffFinalWriteReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE range, raw/VA mapping, flat IDA evidence, and return boundary agree",
            FrameHandoffFinalWriteReturnProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffFinalWriteReturnProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffFinalWriteReturnProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffFinalWriteReturnProjection.EntryRawFileOffset == 0x00BE651C
            && FrameHandoffFinalWriteReturnProjection.RawFileOffsetToVirtualAddress(0x00BE651C)
                == 0x00FE711C
            && FrameHandoffFinalWriteReturnProjection.ZeroBranchTargetRawFileOffset == 0x00BE6527
            && FrameHandoffFinalWriteReturnProjection.ReturnBoundaryRawFileOffsetExclusive
                == 0x00BE652F
            && FrameHandoffFinalWriteReturnProjection.EcxTestBytes == "85 C9"
            && FrameHandoffFinalWriteReturnProjection.ZeroBranchBytes == "74 07"
            && Normalize(FrameHandoffFinalWriteReturnProjection.CapturedWindowBytes)
                == "85C974078B45088B008901FF46105E5DC20400"
            && InstructionEvidenceMatches());

        FrameHandoffFinalWriteReturnPlan zeroEcx =
            FrameHandoffFinalWriteReturnProjection.Project(new(
                EntryEcx: 0,
                EntryEsi: 0x1000,
                EntryEbp: 0x2000,
                Frame8Pointer: null,
                PointeeDword: null,
                Receiver10OldValue: 9));
        check("Zero ECX skips both independent EBP reads and still increments the receiver counter",
            zeroEcx.Outcome == FrameHandoffFinalWriteReturnOutcome.ReturnedNormally
            && zeroEcx.EcxTestReached
            && zeroEcx.ZeroBranchTaken == true
            && !zeroEcx.Frame8ReadReached
            && !zeroEcx.PointeeReadReached
            && !zeroEcx.FinalWriteReached
            && !zeroEcx.FinalWriteProjected
            && zeroEcx.FinalWriteAddress is null
            && zeroEcx.FinalWriteValue is null
            && zeroEcx.ReturnEaxValue is null
            && zeroEcx.Receiver10ReadAddress == 0x1010
            && zeroEcx.CounterIncrementReached
            && zeroEcx.CounterValueBeforeIncrement == 9
            && zeroEcx.CounterValueAfterIncrement == 10
            && zeroEcx.StackPopRegisters.SequenceEqual(new[] { "ESI", "EBP" })
            && zeroEcx.ReturnReached
            && zeroEcx.ReturnImmediateBytes == 4
            && zeroEcx.StopAtRawFileOffsetExclusive == 0x00BE652F);

        FrameHandoffFinalWriteReturnPlan zeroPointee =
            FrameHandoffFinalWriteReturnProjection.Project(new(
                EntryEcx: 0x3000,
                EntryEsi: 0x4000,
                EntryEbp: 0x5000,
                Frame8Pointer: 0,
                PointeeDword: 0,
                Receiver10OldValue: 0xFFFFFFFE));
        check("A captured null pointer and zero pointee remain valid independent read values",
            zeroPointee.Outcome == FrameHandoffFinalWriteReturnOutcome.ReturnedNormally
            && zeroPointee.ZeroBranchTaken == false
            && zeroPointee.Frame8ReadAddress == 0x5008
            && zeroPointee.Frame8ReadReached
            && zeroPointee.Frame8PointerCaptured
            && zeroPointee.PointeeReadAddress == 0
            && zeroPointee.PointeeReadReached
            && zeroPointee.PointeeDwordCaptured
            && zeroPointee.FinalWriteReached
            && zeroPointee.FinalWriteProjected
            && zeroPointee.FinalWriteAddress == 0x3000
            && zeroPointee.FinalWriteValue == 0
            && zeroPointee.ReturnEaxValue == 0
            && zeroPointee.CounterValueAfterIncrement == 0xFFFFFFFF
            && zeroPointee.ReturnReached);

        FrameHandoffFinalWriteReturnPlan missingAll =
            FrameHandoffFinalWriteReturnProjection.Project(new(
                EntryEcx: 1,
                EntryEsi: 0x6000,
                EntryEbp: 0x7000,
                Frame8Pointer: null,
                PointeeDword: null,
                Receiver10OldValue: null));
        check("Missing first-read capture stops at MOV EAX,[EBP+8] before any later effects",
            missingAll.Outcome
                == FrameHandoffFinalWriteReturnOutcome.MissingFrame8PointerCapture
            && missingAll.Frame8ReadAddress == 0x7008
            && missingAll.Frame8ReadReached
            && !missingAll.Frame8PointerCaptured
            && !missingAll.PointeeReadReached
            && !missingAll.FinalWriteReached
            && !missingAll.CounterIncrementReached
            && missingAll.UnreachedPointeeDwordCaptureProvided == false
            && missingAll.UnreachedReceiver10OldValueCaptureProvided == false
            && missingAll.StopAtRawFileOffsetExclusive == 0x00BE6520);

        FrameHandoffFinalWriteReturnPlan missingPointee =
            FrameHandoffFinalWriteReturnProjection.Project(new(
                EntryEcx: 0x8000,
                EntryEsi: 0x9000,
                EntryEbp: 0xA000,
                Frame8Pointer: 0xB000,
                PointeeDword: null,
                Receiver10OldValue: 4));
        FrameHandoffFinalWriteReturnPlan partialWrite =
            FrameHandoffFinalWriteReturnProjection.Project(new(
                EntryEcx: 0xC000,
                EntryEsi: 0xD000,
                EntryEbp: 0xE000,
                Frame8Pointer: 0xF000,
                PointeeDword: 0x12345678,
                Receiver10OldValue: null));
        check("Each missing read stops at its own site, while a missing counter preserves the completed write",
            missingPointee.Outcome
                == FrameHandoffFinalWriteReturnOutcome.MissingPointeeDwordCapture
            && missingPointee.Frame8PointerCaptured
            && missingPointee.PointeeReadAddress == 0xB000
            && missingPointee.PointeeReadReached
            && !missingPointee.PointeeDwordCaptured
            && !missingPointee.FinalWriteReached
            && !missingPointee.CounterIncrementReached
            && missingPointee.UnreachedReceiver10OldValueCaptureProvided
            && missingPointee.StopAtRawFileOffsetExclusive == 0x00BE6523
            && partialWrite.Outcome
                == FrameHandoffFinalWriteReturnOutcome.MissingReceiver10OldValueCapture
            && partialWrite.FinalWriteReached
            && partialWrite.FinalWriteProjected
            && partialWrite.FinalWriteAddress == 0xC000
            && partialWrite.FinalWriteValue == 0x12345678
            && partialWrite.ReturnEaxValue == 0x12345678
            && partialWrite.Receiver10ReadAddress == 0xD010
            && partialWrite.CounterIncrementReached
            && !partialWrite.Receiver10OldValueCaptured
            && partialWrite.CounterValueAfterIncrement is null
            && !partialWrite.ReturnReached
            && partialWrite.StopAtRawFileOffsetExclusive == 0x00BE6527);

        FrameHandoffFinalWriteReturnPlan zeroWithUnreachedCaptures =
            FrameHandoffFinalWriteReturnProjection.Project(new(
                EntryEcx: 0,
                EntryEsi: 0xFFFFFFF0,
                EntryEbp: 0xFFFFFFFC,
                Frame8Pointer: 0,
                PointeeDword: 0xDEADBEEF,
                Receiver10OldValue: 5));
        check("Zero-ECX read captures are rejected before the counter increment can be projected",
            zeroWithUnreachedCaptures.Outcome
                == FrameHandoffFinalWriteReturnOutcome.UnreachedReadCaptureProvided
            && zeroWithUnreachedCaptures.ZeroBranchTaken == true
            && !zeroWithUnreachedCaptures.Frame8ReadReached
            && !zeroWithUnreachedCaptures.PointeeReadReached
            && !zeroWithUnreachedCaptures.CounterIncrementReached
            && zeroWithUnreachedCaptures.UnreachedFrame8PointerCaptureProvided
            && zeroWithUnreachedCaptures.UnreachedPointeeDwordCaptureProvided
            && zeroWithUnreachedCaptures.UnreachedReceiver10OldValueCaptureProvided
            && !zeroWithUnreachedCaptures.ReturnReached
            && zeroWithUnreachedCaptures.StopAtRawFileOffsetExclusive == 0x00BE651E);

        FrameHandoffFinalWriteReturnPlan wrapping =
            FrameHandoffFinalWriteReturnProjection.Project(new(
                EntryEcx: 1,
                EntryEsi: uint.MaxValue - 7,
                EntryEbp: uint.MaxValue - 3,
                Frame8Pointer: uint.MaxValue,
                PointeeDword: 0xAABBCCDD,
                Receiver10OldValue: uint.MaxValue));
        check("Address formation and the independent receiver increment wrap at DWORD width",
            FrameHandoffFinalWriteReturnProjection.Frame8ReadAddress(uint.MaxValue - 3) == 4
            && FrameHandoffFinalWriteReturnProjection.Receiver10ReadAddress(
                uint.MaxValue - 7) == 8
            && wrapping.Frame8ReadAddress == 4
            && wrapping.PointeeReadAddress == uint.MaxValue
            && wrapping.FinalWriteAddress == 1
            && wrapping.FinalWriteValue == 0xAABBCCDD
            && wrapping.Receiver10ReadAddress == 8
            && wrapping.CounterValueBeforeIncrement == uint.MaxValue
            && wrapping.CounterValueAfterIncrement == 0
            && wrapping.ReturnReached);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x00BE651C, "85 C9"),
            (0x00BE651E, "74 07"),
            (0x00BE6520, "8B 45 08"),
            (0x00BE6523, "8B 00"),
            (0x00BE6525, "89 01"),
            (0x00BE6527, "FF 46 10"),
            (0x00BE652A, "5E"),
            (0x00BE652B, "5D"),
            (0x00BE652C, "C2 04 00")
        };
        uint nextRawOffset = FrameHandoffFinalWriteReturnProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (FrameHandoffFinalWriteReturnInstructionEvidence instruction
            in FrameHandoffFinalWriteReturnProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffFinalWriteReturnProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == FrameHandoffFinalWriteReturnProjection.ReturnBoundaryRawFileOffsetExclusive
            && bytes == Normalize(FrameHandoffFinalWriteReturnProjection.CapturedWindowBytes)
            && FrameHandoffFinalWriteReturnProjection.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected)
            && ShortBranchTarget(
                FrameHandoffFinalWriteReturnProjection.ZeroBranchRawFileOffset,
                0x07)
                == FrameHandoffFinalWriteReturnProjection.ZeroBranchTargetRawFileOffset
            && FrameHandoffFinalWriteReturnProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffFinalWriteReturnProjection.ZeroBranchTargetRawFileOffset)
                == 0x00FE7127;
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
