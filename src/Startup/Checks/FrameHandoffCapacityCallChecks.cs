using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityCallChecks
{
    public static void Run(Action<string, bool> check)
    {
        var invalidRoute = FrameHandoffCapacityCallProjection.Project(
            new((FrameHandoffCapacityCallEntryRoute)99, 0, true, 1, true));
        check("An invalid route cannot fall through to the direct call",
            invalidRoute.Outcome == FrameHandoffCapacityCallOutcome.InvalidEntryRoute
            && !invalidRoute.DirectCallReached
            && !invalidRoute.GuardIndirectCallReached
            && invalidRoute.UnreachedDirectCallReturnCaptureProvided
            && !invalidRoute.SuccessAdmission);
        check("The pinned PE range, raw/IDA address, VA mapping, and exact trace bytes agree",
            FrameHandoffCapacityCallProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityCallProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityCallProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityCallProjection.EntryRawFileOffset == 0x00BE6BB5
            && FrameHandoffCapacityCallProjection.EntryVirtualAddress == 0x00FE77B5
            && FrameHandoffCapacityCallProjection.RawFileOffsetToVirtualAddress(0x00BE6BCA)
                == 0x00FE77CA
            && FrameHandoffCapacityCallProjection.DirectCallTargetRawFileOffset == 0x012B7FB0
            && FrameHandoffCapacityCallProjection.DirectCallTargetVirtualAddress == 0x016B8BB0
            && FrameHandoffCapacityCallProjection.GuardIndirectCallCellVirtualAddress
                == 0x01AC4424
            && FrameHandoffCapacityCallProjection.CapturedWindowByteCount == 0x15
            && FrameHandoffCapacityCallProjection.CapturedWindowBytes
                == "E8F6136D0083C4048945FC85C07506FF152444AC01"
            && EvidenceBytesMatchCapturedWindow()
            && InstructionEvidenceMatches());

        check("The direct call and short branch resolve to their recorded destinations",
            FrameHandoffCapacityCallProjection.RelativeCallTarget(
                FrameHandoffCapacityCallProjection.DirectCallRawFileOffset,
                0x006D13F6)
                == FrameHandoffCapacityCallProjection.DirectCallTargetRawFileOffset
            && FrameHandoffCapacityCallProjection.ShortBranchTarget(
                FrameHandoffCapacityCallProjection.JnzRawFileOffset,
                0x06) == FrameHandoffCapacityCallProjection.NormalBoundaryRawFileOffsetExclusive
            && FrameHandoffCapacityCallProjection.GuardBranchEntryRawFileOffset
                == FrameHandoffCapacityCallProjection.GuardIndirectCallRawFileOffset);

        FrameHandoffCapacityCallPlan missingRoute =
            FrameHandoffCapacityCallProjection.Project(new(null, 0x1000, true, 0xFFFFFFFF, true));
        FrameHandoffCapacityCallPlan missingDirectReturn =
            FrameHandoffCapacityCallProjection.Project(
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0x1000, null, 0xFFFFFFFF, true));
        FrameHandoffCapacityCallPlan directDidNotReturn =
            FrameHandoffCapacityCallProjection.Project(
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0x1000, false, 0xFFFFFFFF, true));
        check("Missing routes and nonreturning direct calls stop at the direct call before cleanup",
            missingRoute.Outcome == FrameHandoffCapacityCallOutcome.MissingEntryRoute
            && missingRoute.UnreachedDirectCallReturnCaptureProvided
            && missingRoute.UnreachedDirectCallReturnEaxCaptureProvided
            && missingRoute.UnreachedGuardIndirectCallReturnCaptureProvided
            && missingRoute.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityCallProjection.EntryRawFileOffset
            && missingDirectReturn.Outcome
                == FrameHandoffCapacityCallOutcome.MissingDirectCallReturnCapture
            && missingDirectReturn.DirectCallReached
            && !missingDirectReturn.Cleanup4Reached
            && missingDirectReturn.UnreachedDirectCallReturnEaxCaptureProvided
            && missingDirectReturn.UnreachedGuardIndirectCallReturnCaptureProvided
            && missingDirectReturn.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityCallProjection.DirectCallRawFileOffset
            && directDidNotReturn.Outcome
                == FrameHandoffCapacityCallOutcome.DirectCallDidNotReturnNormally
            && directDidNotReturn.DirectCallReturnedNormally == false
            && !directDidNotReturn.Cleanup4Reached
            && !directDidNotReturn.FrameWriteReached
            && directDidNotReturn.UnreachedDirectCallReturnEaxCaptureProvided
            && directDidNotReturn.UnreachedGuardIndirectCallReturnCaptureProvided);

        FrameHandoffCapacityCallPlan missingEax =
            FrameHandoffCapacityCallProjection.Project(
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0x1000, true, null, null));
        check("Missing EAX after a normal direct call reaches cleanup but not the frame store or TEST",
            missingEax.Outcome
                == FrameHandoffCapacityCallOutcome.MissingDirectCallReturnEaxCapture
            && missingEax.DirectCallReturnedNormally == true
            && missingEax.Cleanup4Reached
            && !missingEax.FrameWriteReached
            && !missingEax.FrameWriteProjected
            && missingEax.FrameWriteAddress is null
            && missingEax.FrameWriteValue is null
            && !missingEax.EaxTestReached
            && !missingEax.GuardIndirectCallReached
            && missingEax.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityCallProjection.FrameWriteRawFileOffset);

        FrameHandoffCapacityCallPlan fullDwordNonzero =
            FrameHandoffCapacityCallProjection.Project(
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 3, true, uint.MaxValue, true));
        check("A full-width nonzero EAX is stored with wrapping EBP-4 arithmetic and skips the guard capture",
            fullDwordNonzero.Outcome
                == FrameHandoffCapacityCallOutcome.DirectNonzeroReturnedNormally
            && fullDwordNonzero.Cleanup4Reached
            && fullDwordNonzero.FrameWriteReached
            && fullDwordNonzero.FrameWriteProjected
            && fullDwordNonzero.FrameWriteAddress == uint.MaxValue
            && fullDwordNonzero.FrameWriteValue == uint.MaxValue
            && fullDwordNonzero.EaxTestReached
            && fullDwordNonzero.JnzTaken == true
            && !fullDwordNonzero.GuardIndirectCallReached
            && fullDwordNonzero.UnreachedGuardIndirectCallReturnCaptureProvided
            && fullDwordNonzero.NormalBoundaryReached
            && !fullDwordNonzero.SuccessAdmission
            && fullDwordNonzero.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityCallProjection.NormalBoundaryRawFileOffsetExclusive
            && FrameHandoffCapacityCallProjection.FrameWriteAddress(0) == 0xFFFFFFFC);

        FrameHandoffCapacityCallPlan directZero =
            FrameHandoffCapacityCallProjection.Project(
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0, true, 0, true));
        check("A zero direct return preserves the zero frame write and needs an independent guard-call return",
            directZero.Outcome
                == FrameHandoffCapacityCallOutcome.DirectZeroGuardIndirectReturnedNormally
            && directZero.Cleanup4Reached
            && directZero.FrameWriteReached
            && directZero.FrameWriteProjected
            && directZero.FrameWriteAddress == 0xFFFFFFFC
            && directZero.FrameWriteValue == 0
            && directZero.EaxTestReached
            && directZero.JnzTaken == false
            && directZero.GuardIndirectCallReached
            && directZero.GuardIndirectCallReturnedNormally == true
            && directZero.NormalBoundaryReached
            && directZero.SuccessAdmission
            && directZero.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityCallProjection.NormalBoundaryRawFileOffsetExclusive);

        FrameHandoffCapacityCallPlan missingGuardReturn =
            FrameHandoffCapacityCallProjection.Project(
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0x1000, true, 0, null));
        FrameHandoffCapacityCallPlan guardDidNotReturn =
            FrameHandoffCapacityCallProjection.Project(
                new(FrameHandoffCapacityCallEntryRoute.DirectCallB5, 0x1000, true, 0, false));
        check("Missing and nonreturning zero-route guard calls retain the completed zero write and stop at FF15",
            missingGuardReturn.Outcome
                == FrameHandoffCapacityCallOutcome.MissingGuardIndirectCallReturnCapture
            && missingGuardReturn.FrameWriteValue == 0
            && missingGuardReturn.GuardIndirectCallReached
            && missingGuardReturn.GuardIndirectCallReturnedNormally is null
            && missingGuardReturn.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityCallProjection.GuardIndirectCallRawFileOffset
            && guardDidNotReturn.Outcome
                == FrameHandoffCapacityCallOutcome.GuardIndirectCallDidNotReturnNormally
            && guardDidNotReturn.FrameWriteValue == 0
            && guardDidNotReturn.GuardIndirectCallReturnedNormally == false
            && !guardDidNotReturn.NormalBoundaryReached
            && guardDidNotReturn.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityCallProjection.GuardIndirectCallRawFileOffset);

        FrameHandoffCapacityCallPlan cleanGuardRoute =
            FrameHandoffCapacityCallProjection.Project(
                new(FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4, 0x1234, null, null, true));
        FrameHandoffCapacityCallPlan guardWithUnreachedDirectCaptures =
            FrameHandoffCapacityCallProjection.Project(
                new(FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4, 0x1234, true, 0xAABBCCDD, true));
        FrameHandoffCapacityCallPlan missingGuardRouteReturn =
            FrameHandoffCapacityCallProjection.Project(
                new(FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4, 0x1234, null, null, null));
        FrameHandoffCapacityCallPlan guardRouteDidNotReturn =
            FrameHandoffCapacityCallProjection.Project(
                new(FrameHandoffCapacityCallEntryRoute.GuardIndirectBC4, 0x1234, null, null, false));
        check("The explicit BC4 route skips direct cleanup and frame writes, while admission rejects unreachable captures",
            cleanGuardRoute.Outcome
                == FrameHandoffCapacityCallOutcome.GuardIndirectReturnedNormally
            && !cleanGuardRoute.DirectCallReached
            && cleanGuardRoute.DirectCallReturnedNormally is null
            && !cleanGuardRoute.Cleanup4Reached
            && !cleanGuardRoute.FrameWriteReached
            && !cleanGuardRoute.EaxTestReached
            && cleanGuardRoute.GuardIndirectCallReached
            && cleanGuardRoute.GuardIndirectCallReturnedNormally == true
            && cleanGuardRoute.NormalBoundaryReached
            && cleanGuardRoute.SuccessAdmission
            && guardWithUnreachedDirectCaptures.UnreachedDirectCallReturnCaptureProvided
            && guardWithUnreachedDirectCaptures.UnreachedDirectCallReturnEaxCaptureProvided
            && !guardWithUnreachedDirectCaptures.SuccessAdmission
            && missingGuardRouteReturn.Outcome
                == FrameHandoffCapacityCallOutcome.MissingGuardIndirectCallReturnCapture
            && missingGuardRouteReturn.GuardIndirectCallReached
            && !missingGuardRouteReturn.DirectCallReached
            && missingGuardRouteReturn.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityCallProjection.GuardIndirectCallRawFileOffset
            && guardRouteDidNotReturn.Outcome
                == FrameHandoffCapacityCallOutcome.GuardIndirectCallDidNotReturnNormally
            && guardRouteDidNotReturn.GuardIndirectCallReturnedNormally == false
            && !guardRouteDidNotReturn.Cleanup4Reached
            && !guardRouteDidNotReturn.FrameWriteReached
            && guardRouteDidNotReturn.StopAtRawFileOffsetExclusive
                == FrameHandoffCapacityCallProjection.GuardIndirectCallRawFileOffset);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected =
            new (
                uint Raw,
                uint BytesAddress,
                uint Virtual,
                string Bytes,
                uint? BranchTargetRaw,
                uint? BranchTargetVirtual,
                uint? CallTargetRaw,
                uint? CallTargetVirtual,
                uint? IndirectCell)[]
            {
                (0x00BE6BB5, 0x00BE6BB5, 0x00FE77B5, "E8 F6 13 6D 00",
                    null, null, 0x012B7FB0, 0x016B8BB0, null),
                (0x00BE6BBA, 0x00BE6BBA, 0x00FE77BA, "83 C4 04",
                    null, null, null, null, null),
                (0x00BE6BBD, 0x00BE6BBD, 0x00FE77BD, "89 45 FC",
                    null, null, null, null, null),
                (0x00BE6BC0, 0x00BE6BC0, 0x00FE77C0, "85 C0",
                    null, null, null, null, null),
                (0x00BE6BC2, 0x00BE6BC2, 0x00FE77C2, "75 06",
                    0x00BE6BCA, 0x00FE77CA, null, null, null),
                (0x00BE6BC4, 0x00BE6BC4, 0x00FE77C4, "FF 15 24 44 AC 01",
                    null, null, null, null, 0x01AC4424)
            };
        uint nextRawOffset = FrameHandoffCapacityCallProjection.EntryRawFileOffset;
        string instructionBytes = string.Empty;

        foreach (FrameHandoffCapacityCallInstructionEvidence instruction
            in FrameHandoffCapacityCallProjection.Evidence)
        {
            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacityCallProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            if (instruction.BranchTargetRawFileOffset is uint targetRaw
                && instruction.BranchTargetVirtualAddress
                    != FrameHandoffCapacityCallProjection.RawFileOffsetToVirtualAddress(targetRaw))
            {
                return false;
            }

            nextRawOffset += (uint)(bytes.Length / 2);
            instructionBytes += bytes;
        }

        return nextRawOffset == FrameHandoffCapacityCallProjection.NormalBoundaryRawFileOffsetExclusive
            && instructionBytes == FrameHandoffCapacityCallProjection.CapturedWindowBytes
            && FrameHandoffCapacityCallProjection.Evidence
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.FlatIdaAddress,
                        instruction.VirtualAddress,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress,
                        instruction.DirectCallTargetRawFileOffset,
                        instruction.DirectCallTargetVirtualAddress,
                        instruction.IndirectCallCellVirtualAddress))
                .SequenceEqual(expected);
    }

    private static bool EvidenceBytesMatchCapturedWindow() =>
        string.Concat(FrameHandoffCapacityCallProjection.Evidence.Select(
            instruction => string.Concat(instruction.Bytes.Where(character =>
                !char.IsWhiteSpace(character)))))
        == FrameHandoffCapacityCallProjection.CapturedWindowBytes;
}
