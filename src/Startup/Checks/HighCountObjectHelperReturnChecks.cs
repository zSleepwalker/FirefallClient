using Firefall.Reconstruction.Startup;

internal static class HighCountObjectHelperReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The helper-return bytes are pinned to the authorized PE and raw-to-VA mapping",
            HighCountObjectHelperReturnProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && HighCountObjectHelperReturnProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && HighCountObjectHelperReturnProjection.EntryRawFileOffset == 0x00BE6458
            && HighCountObjectHelperReturnProjection.EntryVirtualAddress == 0x00FE7058
            && HighCountObjectHelperReturnProjection.EndRawFileOffsetExclusive == 0x00BE6477
            && HighCountObjectHelperReturnProjection.EndVirtualAddressExclusive == 0x00FE7077
            && HighCountObjectHelperReturnProjection.CapturedWindowLength == 0x1F
            && HighCountObjectHelperReturnProjection.RawFileOffsetToVirtualAddress(
                0x00BE6458) == 0x00FE7058
            && HighCountObjectHelperReturnProjection.RawFileOffsetToVirtualAddress(
                0x00BE6477) == 0x00FE7077
            && string.Concat(HighCountObjectHelperReturnProjection.CapturedWindowBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B50088BCA8B460848C1E90223C88B460483E2035E8B04888D04908BE55DC3"
            && VerifyInstructionEvidence());

        HighCountObjectHelperReturnPlan zeroInputs =
            HighCountObjectHelperReturnProjection.Project(
                new(0, 0, 0, 0, 0, 0));
        check("Zero EAX and ESI are valid read bases and do not trigger null guards",
            zeroInputs.Outcome == HighCountObjectHelperReturnOutcome.Returned
            && zeroInputs.DwordAtEntryEaxPlus8.Address == 8
            && zeroInputs.DwordAtEntryEsiPlus8.Address == 8
            && zeroInputs.DwordAtEntryEsiPlus4.Address == 4
            && zeroInputs.ComputedSlotAddress == 0
            && zeroInputs.ReturnEax == 0
            && !zeroInputs.OwnershipInferred);

        HighCountObjectHelperReturnPlan wrapped =
            HighCountObjectHelperReturnProjection.Project(
                new(0xFFFFFFF8, 0xFFFFFFF8, 3, 0, 0xFFFFFFFC, 0xFFFFFFFF));
        check("Address formation, decrement, slot selection, and full EAX return wrap at DWORD width",
            wrapped.Outcome == HighCountObjectHelperReturnOutcome.Returned
            && wrapped.DwordAtEntryEaxPlus8.Address == 0
            && wrapped.DwordAtEntryEsiPlus8.Address == 0
            && wrapped.DwordAtEntryEsiPlus4.Address == 0xFFFFFFFC
            && wrapped.DecrementedEax == 0xFFFFFFFF
            && wrapped.ShiftedEcx == 0
            && wrapped.MaskedEcx == 0
            && wrapped.MaskedEdx == 3
            && wrapped.ComputedSlotAddress == 0xFFFFFFFC
            && wrapped.ReturnEax == 0x0000000B
            && !wrapped.OwnershipInferred);

        HighCountObjectHelperReturnPlan missingEntryEax =
            HighCountObjectHelperReturnProjection.Project(
                new(null, 0, 0x10, 0x20, 0x30, 0x40));
        check("Missing entry EAX stops at the first read and marks downstream reads unreached",
            missingEntryEax.Outcome == HighCountObjectHelperReturnOutcome.MissingEntryEax
            && missingEntryEax.DwordAtEntryEaxPlus8.ReadSiteReached
            && missingEntryEax.DwordAtEntryEaxPlus8.Address is null
            && missingEntryEax.DwordAtEntryEaxPlus8.CaptureProvided
            && !missingEntryEax.DwordAtEntryEaxPlus8.MissingCapture
            && missingEntryEax.DwordAtEntryEaxPlus8.UnreachedCaptureProvided
            && missingEntryEax.DwordAtEntryEaxPlus8.Value is null
            && missingEntryEax.UnreachedEntryEsiCaptureProvided
            && missingEntryEax.DwordAtEntryEsiPlus8.UnreachedCaptureProvided
            && missingEntryEax.DwordAtEntryEsiPlus4.UnreachedCaptureProvided
            && missingEntryEax.DwordAtComputedSlot.UnreachedCaptureProvided
            && missingEntryEax.StopAtRawFileOffsetExclusive == 0x00BE6458);

        HighCountObjectHelperReturnPlan missingFirstRead =
            HighCountObjectHelperReturnProjection.Project(
                new(0, 0, null, 0x20, 0x30, 0x40));
        check("A missing first read preserves its reached site and rejects later read captures as unreached",
            missingFirstRead.Outcome
                == HighCountObjectHelperReturnOutcome.MissingDwordAtEntryEaxPlus8
            && missingFirstRead.DwordAtEntryEaxPlus8.Address == 8
            && missingFirstRead.DwordAtEntryEaxPlus8.MissingCapture
            && missingFirstRead.UnreachedEntryEsiCaptureProvided
            && missingFirstRead.DwordAtEntryEsiPlus8.UnreachedCaptureProvided
            && missingFirstRead.DwordAtEntryEsiPlus4.UnreachedCaptureProvided
            && missingFirstRead.DwordAtComputedSlot.UnreachedCaptureProvided
            && missingFirstRead.StopAtRawFileOffsetExclusive == 0x00BE6458);

        HighCountObjectHelperReturnPlan missingEntryEsi =
            HighCountObjectHelperReturnProjection.Project(
                new(0, null, 0x13, 0x22, 0x30, 0x40));
        check("Missing entry ESI preserves the first read and ECX copy, then stops at the second read",
            missingEntryEsi.Outcome == HighCountObjectHelperReturnOutcome.MissingEntryEsi
            && missingEntryEsi.DwordAtEntryEaxPlus8.Value == 0x13
            && missingEntryEsi.EntryEcxAfterMove == 0x13
            && missingEntryEsi.DwordAtEntryEsiPlus8.ReadSiteReached
            && missingEntryEsi.DwordAtEntryEsiPlus8.Address is null
            && missingEntryEsi.DwordAtEntryEsiPlus8.CaptureProvided
            && !missingEntryEsi.DwordAtEntryEsiPlus8.MissingCapture
            && missingEntryEsi.DwordAtEntryEsiPlus8.UnreachedCaptureProvided
            && missingEntryEsi.DwordAtEntryEsiPlus8.Value is null
            && missingEntryEsi.DwordAtEntryEsiPlus4.UnreachedCaptureProvided
            && missingEntryEsi.DwordAtComputedSlot.UnreachedCaptureProvided
            && missingEntryEsi.StopAtRawFileOffsetExclusive == 0x00BE645D);

        HighCountObjectHelperReturnPlan missingSecondRead =
            HighCountObjectHelperReturnProjection.Project(
                new(0, 0, 0x13, null, 0x30, 0x40));
        check("A missing second read stops before decrement and shift while retaining earlier reads",
            missingSecondRead.Outcome
                == HighCountObjectHelperReturnOutcome.MissingDwordAtEntryEsiPlus8
            && missingSecondRead.DwordAtEntryEaxPlus8.Value == 0x13
            && missingSecondRead.DwordAtEntryEsiPlus8.Address == 8
            && missingSecondRead.DwordAtEntryEsiPlus8.MissingCapture
            && missingSecondRead.DecrementedEax is null
            && missingSecondRead.ShiftedEcx is null
            && missingSecondRead.DwordAtEntryEsiPlus4.UnreachedCaptureProvided
            && missingSecondRead.DwordAtComputedSlot.UnreachedCaptureProvided
            && missingSecondRead.StopAtRawFileOffsetExclusive == 0x00BE645D);

        HighCountObjectHelperReturnPlan missingThirdRead =
            HighCountObjectHelperReturnProjection.Project(
                new(0, 0, 0x13, 2, null, 0x40));
        check("A missing third read preserves the prior DWORD arithmetic and leaves the slot read unreached",
            missingThirdRead.Outcome
                == HighCountObjectHelperReturnOutcome.MissingDwordAtEntryEsiPlus4
            && missingThirdRead.DecrementedEax == 1
            && missingThirdRead.ShiftedEcx == 4
            && missingThirdRead.MaskedEcx == 0
            && missingThirdRead.DwordAtEntryEsiPlus4.Address == 4
            && missingThirdRead.DwordAtEntryEsiPlus4.MissingCapture
            && missingThirdRead.DwordAtComputedSlot.UnreachedCaptureProvided
            && missingThirdRead.StopAtRawFileOffsetExclusive == 0x00BE6466);

        HighCountObjectHelperReturnPlan missingFourthRead =
            HighCountObjectHelperReturnProjection.Project(
                new(0, 0, 0x13, 2, 0x100, null));
        check("A missing indexed read preserves the masked index, low EDX bits, and computed read address",
            missingFourthRead.Outcome
                == HighCountObjectHelperReturnOutcome.MissingDwordAtComputedSlot
            && missingFourthRead.MaskedEcx == 0
            && missingFourthRead.MaskedEdx == 3
            && missingFourthRead.ComputedSlotAddress == 0x100
            && missingFourthRead.DwordAtComputedSlot.ReadSiteReached
            && missingFourthRead.DwordAtComputedSlot.Address == 0x100
            && missingFourthRead.DwordAtComputedSlot.MissingCapture
            && missingFourthRead.ReturnEax is null
            && missingFourthRead.StopAtRawFileOffsetExclusive == 0x00BE646D);
    }

    private static bool VerifyInstructionEvidence()
    {
        uint nextRawOffset = HighCountObjectHelperReturnProjection.EntryRawFileOffset;
        string instructionBytes = string.Empty;
        foreach (HighCountObjectHelperReturnInstructionEvidence instruction
            in HighCountObjectHelperReturnProjection.Instructions)
        {
            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != HighCountObjectHelperReturnProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            instructionBytes += bytes;
            nextRawOffset += (uint)(bytes.Length / 2);
        }

        return nextRawOffset
                == HighCountObjectHelperReturnProjection.EndRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(
                    HighCountObjectHelperReturnProjection.CapturedWindowBytes
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            && HighCountObjectHelperReturnProjection.ReturnRawFileOffset == 0x00BE6476;
    }
}
