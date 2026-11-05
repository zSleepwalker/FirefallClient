using Firefall.Reconstruction.Startup;

internal static class PayloadXorReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The exact authorized raw range, sample identity, mapping, and bytes are pinned",
            PayloadXorReturnProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && PayloadXorReturnProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && PayloadXorReturnProjection.EntryStartRawFileOffset == 0x00BD594E
            && PayloadXorReturnProjection.EntryEndRawFileOffsetExclusive == 0x00BD5954
            && PayloadXorReturnProjection.CapturedCodeLength == 6
            && PayloadXorReturnProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && PayloadXorReturnProjection.EntryStartVirtualAddress == 0x00FD654E
            && PayloadXorReturnProjection.EntryEndVirtualAddressExclusive == 0x00FD6554
            && string.Concat(PayloadXorReturnProjection.CapturedCodeBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "5E5B8BE55DC3"
            && InstructionEvidenceMatches());

        PayloadXorReturnPlan invalidBoundary = PayloadXorReturnProjection.Project(
            new(false, 0xA1B2C3D4));
        PayloadXorReturnPlan invalidBoundaryWithoutEax =
            PayloadXorReturnProjection.Project(new(false, null));
        check("An unvalidated boundary has no epilogue effects and marks a supplied EAX capture unreached",
            invalidBoundary.Outcome
                == PayloadXorReturnOutcome.EntryBoundaryNotValidated
            && invalidBoundary.UnreachedEaxAtEntryCaptureProvided
            && !invalidBoundary.EsiPopped
            && !invalidBoundary.EbxPopped
            && !invalidBoundary.FrameStackPointerReset
            && !invalidBoundary.EbpPopped
            && invalidBoundary.EaxAtReturn is null
            && !invalidBoundary.EaxPreservedThroughEpilogue
            && !invalidBoundary.ReturnRegisterWriteProjected
            && !invalidBoundary.ReturnReached
            && !invalidBoundary.ReturnImmediatePresent
            && invalidBoundary.StopAtRawFileOffsetExclusive
                == PayloadXorReturnProjection.EntryStartRawFileOffset
            && invalidBoundary.Actions.Length == 0
            && !invalidBoundaryWithoutEax.UnreachedEaxAtEntryCaptureProvided
            && invalidBoundaryWithoutEax.Actions.Length == 0);

        PayloadXorReturnPlan missingEax =
            PayloadXorReturnProjection.Project(new(true, null));
        PayloadXorReturnPlan nativeZeroEax =
            PayloadXorReturnProjection.Project(new(true, 0));
        const uint arbitraryEax = 0xA1B2C3D4;
        PayloadXorReturnPlan preservedEax =
            PayloadXorReturnProjection.Project(new(true, arbitraryEax));
        check("A validated native return does not require EAX; captured zero and arbitrary EAX values pass through unchanged",
            missingEax.Outcome == PayloadXorReturnOutcome.NativeReturnReached
            && missingEax.ReturnReached
            && missingEax.EaxAtReturn is null
            && missingEax.EaxPreservedThroughEpilogue
            && nativeZeroEax.ReturnReached
            && nativeZeroEax.EaxAtReturn == 0
            && nativeZeroEax.EaxPreservedThroughEpilogue
            && preservedEax.ReturnReached
            && preservedEax.EaxAtReturn == arbitraryEax
            && preservedEax.EaxPreservedThroughEpilogue
            && !preservedEax.ReturnRegisterWriteProjected);

        PayloadXorReturnPlan returned = PayloadXorReturnProjection.Project(
            new(true, 0x12345678));
        check("The native epilogue pops ESI then EBX, resets ESP from EBP, pops EBP, and returns without an immediate",
            returned.Actions.Select(action => action.Kind).SequenceEqual(new[]
            {
                PayloadXorReturnActionKind.PopEsi,
                PayloadXorReturnActionKind.PopEbx,
                PayloadXorReturnActionKind.ResetEspFromEbp,
                PayloadXorReturnActionKind.PopEbp,
                PayloadXorReturnActionKind.Return
            })
            && returned.Actions.Select(action => action.RawFileOffset).SequenceEqual(new[]
            {
                PayloadXorReturnProjection.PopEsiRawFileOffset,
                PayloadXorReturnProjection.PopEbxRawFileOffset,
                PayloadXorReturnProjection.FrameStackPointerResetRawFileOffset,
                PayloadXorReturnProjection.PopEbpRawFileOffset,
                PayloadXorReturnProjection.ReturnRawFileOffset
            })
            && returned.EsiPopped
            && returned.EbxPopped
            && returned.FrameStackPointerReset
            && returned.EbpPopped
            && returned.ReturnReached
            && !returned.ReturnImmediatePresent
            && !returned.ReturnRegisterWriteProjected
            && returned.StopAtRawFileOffsetExclusive
                == PayloadXorReturnProjection.EntryEndRawFileOffsetExclusive);
    }

    private static bool InstructionEvidenceMatches()
    {
        uint nextRawOffset = PayloadXorReturnProjection.EntryStartRawFileOffset;
        string instructionBytes = string.Empty;
        foreach (PayloadXorReturnInstructionEvidence instruction
            in PayloadXorReturnProjection.Instructions)
        {
            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != PayloadXorReturnProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            instructionBytes += bytes;
            nextRawOffset += (uint)(bytes.Length / 2);
        }

        return nextRawOffset
                == PayloadXorReturnProjection.EntryEndRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(
                    PayloadXorReturnProjection.CapturedCodeBytes
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
