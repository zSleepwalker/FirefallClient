using Firefall.Reconstruction.Startup;

internal static class HighCountObjectHelperEntryChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The bounded object-helper window is pinned to the authorized PE bytes and raw-to-VA mapping",
            HighCountObjectHelperEntryProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && HighCountObjectHelperEntryProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && HighCountObjectHelperEntryProjection.EntryRawFileOffset == 0x00BE6440
            && HighCountObjectHelperEntryProjection.EntryVirtualAddress == 0x00FE7040
            && HighCountObjectHelperEntryProjection.EndRawFileOffsetExclusive == 0x00BE6458
            && HighCountObjectHelperEntryProjection.EndVirtualAddressExclusive == 0x00FE7058
            && HighCountObjectHelperEntryProjection.CapturedWindowLength == 0x18
            && HighCountObjectHelperEntryProjection.HelperTargetRawFileOffset == 0x00BE6B30
            && HighCountObjectHelperEntryProjection.HelperTargetVirtualAddress == 0x00FE7730
            && HighCountObjectHelperEntryProjection.RawFileOffsetToVirtualAddress(
                0x00BE6440) == 0x00FE7040
            && HighCountObjectHelperEntryProjection.RawFileOffsetToVirtualAddress(
                0x00BE6458) == 0x00FE7058
            && string.Concat(HighCountObjectHelperEntryProjection.CapturedWindowBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "558BEC83EC0C8D45F45650E8E00600008B3085F674028B36"
            && VerifyInstructionEvidence());

        HighCountObjectHelperEntryPlan wrappedLocalArgument =
            HighCountObjectHelperEntryProjection.Project(
                new(0x00000008, 0xA1B2C3D4, true, 0, 0));
        check("The prologue-defined frame address wraps at DWORD width and is passed without reading the local argument",
            wrappedLocalArgument.LocalArgumentAddress == 0xFFFFFFFC
            && wrappedLocalArgument.LocalArgumentPointerPushed
            && !wrappedLocalArgument.LocalArgumentReadBeforeHelperCall
            && wrappedLocalArgument.HelperCallReached
            && wrappedLocalArgument.HelperEntryEcxReceiver == 0xA1B2C3D4
            && wrappedLocalArgument.HelperReturnedNormally == true
            && wrappedLocalArgument.HelperReturnEaxCaptured
            && wrappedLocalArgument.HelperReturnEax == 0
            && wrappedLocalArgument.DwordReadReached
            && wrappedLocalArgument.DwordReadAddress == 0
            && wrappedLocalArgument.DwordAtHelperEaxCaptured
            && wrappedLocalArgument.DwordAtHelperEax == 0
            && wrappedLocalArgument.ZeroBranchTaken == true
            && wrappedLocalArgument.NextRawFileOffset == 0x00BE6458
            && wrappedLocalArgument.StopAtRawFileOffsetExclusive == 0x00BE6458);

        HighCountObjectHelperEntryPlan missingHelperStatus =
            HighCountObjectHelperEntryProjection.Project(
                new(0x1000, 0x12345678, null, 0x2000, 0x3000));
        HighCountObjectHelperEntryPlan nonreturningHelper =
            HighCountObjectHelperEntryProjection.Project(
                new(0x1000, 0x12345678, false, 0x2000, 0x3000));
        check("Missing or nonnormal helper return blocks the EAX read and marks supplied later captures unreached",
            missingHelperStatus.Outcome
                == HighCountObjectHelperEntryOutcome.UnreachedCaptureProvided
            && missingHelperStatus.HelperCallReached
            && missingHelperStatus.HelperReturnedNormally is null
            && !missingHelperStatus.HelperReturnEaxCaptured
            && missingHelperStatus.HelperReturnEax is null
            && !missingHelperStatus.DwordReadReached
            && missingHelperStatus.DwordReadAddress is null
            && missingHelperStatus.UnreachedHelperReturnEaxCaptureProvided
            && missingHelperStatus.UnreachedDwordAtHelperEaxCaptureProvided
            && missingHelperStatus.StopAtRawFileOffsetExclusive == 0x00BE644B
            && nonreturningHelper.Outcome
                == HighCountObjectHelperEntryOutcome.UnreachedCaptureProvided
            && nonreturningHelper.HelperReturnedNormally == false
            && !nonreturningHelper.HelperReturnEaxCaptured
            && !nonreturningHelper.DwordReadReached
            && nonreturningHelper.UnreachedHelperReturnEaxCaptureProvided
            && nonreturningHelper.UnreachedDwordAtHelperEaxCaptureProvided
            && nonreturningHelper.StopAtRawFileOffsetExclusive == 0x00BE644B);

        HighCountObjectHelperEntryPlan missingEax =
            HighCountObjectHelperEntryProjection.Project(
                new(0x2000, 0xABCDEF01, true, null, 0));
        check("A missing helper-return EAX leaves the later DWORD capture unreached",
            missingEax.Outcome == HighCountObjectHelperEntryOutcome.UnreachedCaptureProvided
            && missingEax.HelperReturnedNormally == true
            && !missingEax.HelperReturnEaxCaptured
            && !missingEax.DwordReadReached
            && missingEax.DwordReadAddress is null
            && !missingEax.UnreachedHelperReturnEaxCaptureProvided
            && missingEax.UnreachedDwordAtHelperEaxCaptureProvided
            && missingEax.StopAtRawFileOffsetExclusive == 0x00BE6450);

        HighCountObjectHelperEntryPlan missingDword =
            HighCountObjectHelperEntryProjection.Project(
                new(0x2000, 0xABCDEF01, true, 0, null));
        check("A missing loaded DWORD preserves the known helper-return EAX address, including address zero",
            missingDword.Outcome
                == HighCountObjectHelperEntryOutcome.MissingDwordAtHelperEax
            && missingDword.HelperReturnEaxCaptured
            && missingDword.HelperReturnEax == 0
            && missingDword.DwordReadReached
            && missingDword.DwordReadAddress == 0
            && !missingDword.DwordAtHelperEaxCaptured
            && missingDword.DwordAtHelperEax is null
            && !missingDword.ZeroTestProjected
            && missingDword.ZeroBranchTaken is null
            && missingDword.StopAtRawFileOffsetExclusive == 0x00BE6450);

        HighCountObjectHelperEntryPlan nonzeroLoadedDword =
            HighCountObjectHelperEntryProjection.Project(
                new(0xFFFFFFFF, 0, true, 0x4000, 0xFFFFFFFF));
        check("A nonzero loaded DWORD falls through to the bounded nonzero destination without projecting later body behavior",
            nonzeroLoadedDword.Outcome
                == HighCountObjectHelperEntryOutcome.NonzeroLoadedDwordFallsThrough
            && nonzeroLoadedDword.LocalArgumentAddress == 0xFFFFFFF3
            && nonzeroLoadedDword.HelperEntryEcxReceiver == 0
            && nonzeroLoadedDword.DwordReadAddress == 0x4000
            && nonzeroLoadedDword.DwordAtHelperEaxCaptured
            && nonzeroLoadedDword.DwordAtHelperEax == 0xFFFFFFFF
            && nonzeroLoadedDword.ZeroTestProjected
            && nonzeroLoadedDword.ZeroBranchTaken == false
            && nonzeroLoadedDword.NextRawFileOffset == 0x00BE6456
            && nonzeroLoadedDword.StopAtRawFileOffsetExclusive == 0x00BE6456);
    }

    private static bool VerifyInstructionEvidence()
    {
        uint nextRawOffset = HighCountObjectHelperEntryProjection.EntryRawFileOffset;
        string instructionBytes = string.Empty;
        var pushOffsets = new List<uint>();
        foreach (HighCountObjectHelperEntryInstructionEvidence instruction
            in HighCountObjectHelperEntryProjection.Instructions)
        {
            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != HighCountObjectHelperEntryProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            if (instruction.BranchTargetRawFileOffset is uint branchTarget
                && instruction.BranchTargetVirtualAddress
                    != HighCountObjectHelperEntryProjection.RawFileOffsetToVirtualAddress(
                        branchTarget))
            {
                return false;
            }

            if (instruction.CallTargetRawFileOffset is uint callTarget
                && (instruction.CallTargetVirtualAddress
                        != HighCountObjectHelperEntryProjection.RawFileOffsetToVirtualAddress(
                            callTarget)
                    || callTarget != HighCountObjectHelperEntryProjection.HelperTargetRawFileOffset))
            {
                return false;
            }

            if (bytes is "55" or "56" or "50")
            {
                pushOffsets.Add(instruction.RawFileOffset);
            }

            instructionBytes += bytes;
            nextRawOffset += (uint)(bytes.Length / 2);
        }

        return nextRawOffset
                == HighCountObjectHelperEntryProjection.EndRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(
                    HighCountObjectHelperEntryProjection.CapturedWindowBytes
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            && pushOffsets.SequenceEqual(
                HighCountObjectHelperEntryProjection.PushInstructionRawFileOffsets)
            && pushOffsets.SequenceEqual(new uint[] { 0x00BE6440, 0x00BE6449, 0x00BE644A })
            && HighCountObjectHelperEntryProjection.ReservedLocalBytes == 0x0C
            && HighCountObjectHelperEntryProjection.LocalArgumentOffsetFromFrameEbp == 0x0C
            && HighCountObjectHelperEntryProjection.LocalArgumentAddress(0) == 0xFFFFFFF4
            && HighCountObjectHelperEntryProjection.ZeroBranchTargetRawFileOffset == 0x00BE6458
            && HighCountObjectHelperEntryProjection.NonzeroFallthroughRawFileOffset == 0x00BE6456;
    }
}
