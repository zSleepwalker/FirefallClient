using Firefall.Reconstruction.Startup;

internal static class FrameHandoffHelperSlotChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE range matches the independently decoded IDA instructions and exact bytes",
            FrameHandoffHelperSlotProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffHelperSlotProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffHelperSlotProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffHelperSlotProjection.EntryRawFileOffset == 0x00BE64C5
            && FrameHandoffHelperSlotProjection.EntryVirtualAddress == 0x00FE70C5
            && FrameHandoffHelperSlotProjection.EndRawFileOffsetExclusive == 0x00BE64F2
            && FrameHandoffHelperSlotProjection.RawFileOffsetToVirtualAddress(0x00BE64F2)
                == 0x00FE70F2
            && FrameHandoffHelperSlotProjection.FirstSlotTestRawFileOffset == 0x00BE64EC
            && FrameHandoffHelperSlotProjection.FirstSlotBranchRawFileOffset == 0x00BE64F0
            && FrameHandoffHelperSlotProjection.FirstSlotNonzeroBranchDestinationRawFileOffset
                == 0x00BE650E
            && FrameHandoffHelperSlotProjection.FirstSlotZeroFallthroughRawFileOffset
                == 0x00BE64F2
            && string.Concat(FrameHandoffHelperSlotProjection.CapturedWindowBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
                == "8B4E0853578B7E108D048DFFFFFFFF4921460C037E0C8BC7C1E80223C88B46048D1C8D00000000833C0300751C"
            && FrameHandoffHelperSlotProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 0x2D
            && InstructionEvidenceMatches());

        check("The short JNZ uses its signed displacement and its fallthrough is the exclusive stop boundary",
            ShortBranchTarget(
                FrameHandoffHelperSlotProjection.FirstSlotBranchRawFileOffset,
                0x1C)
                == FrameHandoffHelperSlotProjection.FirstSlotNonzeroBranchDestinationRawFileOffset
            && FrameHandoffHelperSlotProjection.Instructions[^1].RawFileOffset
                + (uint)FrameHandoffHelperSlotProjection.Instructions[^1].Bytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
                == FrameHandoffHelperSlotProjection.EndRawFileOffsetExclusive
            && FrameHandoffHelperSlotProjection.ReceiverAddress(0, 0x0C) == 0x0C
            && FrameHandoffHelperSlotProjection.ReceiverAddress(uint.MaxValue - 0x0B, 0x0C) == 0
            && FrameHandoffHelperSlotProjection.ComputeReceiverPlus08Mask(0) == uint.MaxValue
            && FrameHandoffHelperSlotProjection.ComputeReceiverPlus08Mask(0x40000001) == 3);

        FrameHandoffHelperSlotPlan missingPlus08 =
            FrameHandoffHelperSlotProjection.Project(new(0x1000, null, 0, 1, 2, 4, 0));
        FrameHandoffHelperSlotPlan missingPlus10 =
            FrameHandoffHelperSlotProjection.Project(new(0x2000, 0, null, 1, 2, 4, 0));
        FrameHandoffHelperSlotPlan missingOldValue =
            FrameHandoffHelperSlotProjection.Project(new(0x3000, 2, 3, null, 4, 5, 6));
        FrameHandoffHelperSlotPlan missingFreshValue =
            FrameHandoffHelperSlotProjection.Project(new(0x4000, 2, 3, 0xFFFFFFFF, null, 5, 6));
        FrameHandoffHelperSlotPlan missingBase =
            FrameHandoffHelperSlotProjection.Project(new(0x5000, 2, 3, 0xFFFFFFFF, 4, null, 6));
        FrameHandoffHelperSlotPlan missingSlot =
            FrameHandoffHelperSlotProjection.Project(new(0x6000, 2, 3, 0xFFFFFFFF, 4, 5, null));
        check("Missing captures stop at their reached instruction and preserve every supplied later capture as unreached",
            missingPlus08.Outcome
                == FrameHandoffHelperSlotOutcome.MissingReceiverPlus08Capture
            && missingPlus08.ReceiverPlus08ReadReached
            && !missingPlus08.ReceiverPlus08Captured
            && missingPlus08.ReceiverPlus08ReadAddress == 0x1008
            && missingPlus08.ReceiverPlus10ReadAddress is null
            && missingPlus08.UnreachedReceiverPlus10CaptureProvided
            && missingPlus08.UnreachedReceiverPlus0COldCaptureProvided
            && missingPlus08.UnreachedReceiverPlus0CFreshCaptureProvided
            && missingPlus08.UnreachedReceiverPlus04CaptureProvided
            && missingPlus08.UnreachedFirstSlotDwordCaptureProvided
            && missingPlus08.StopAtRawFileOffsetExclusive == 0x00BE64C5
            && missingPlus10.Outcome
                == FrameHandoffHelperSlotOutcome.MissingReceiverPlus10Capture
            && missingPlus10.ReceiverPlus08ReadValue == 0
            && missingPlus10.EbxPushReached
            && missingPlus10.EdiPushReached
            && missingPlus10.ReceiverPlus10ReadReached
            && !missingPlus10.ReceiverPlus10Captured
            && missingPlus10.UnreachedReceiverPlus0COldCaptureProvided
            && missingPlus10.UnreachedReceiverPlus0CFreshCaptureProvided
            && missingPlus10.StopAtRawFileOffsetExclusive == 0x00BE64CA
            && missingOldValue.Outcome
                == FrameHandoffHelperSlotOutcome.MissingReceiverPlus0COldCapture
            && missingOldValue.MaskFromReceiverPlus08 == 7
            && missingOldValue.DecrementedReceiverPlus08 == 1
            && missingOldValue.ReceiverPlus0COldReadReached
            && missingOldValue.ReceiverPlus0COldReadAddress == 0x300C
            && !missingOldValue.ReceiverPlus0COldCaptured
            && !missingOldValue.ReceiverPlus0CWriteReached
            && missingOldValue.UnreachedReceiverPlus0CFreshCaptureProvided
            && missingOldValue.UnreachedReceiverPlus04CaptureProvided
            && missingOldValue.UnreachedFirstSlotDwordCaptureProvided
            && missingOldValue.StopAtRawFileOffsetExclusive == 0x00BE64D5
            && missingFreshValue.Outcome
                == FrameHandoffHelperSlotOutcome.MissingReceiverPlus0CFreshCapture
            && missingFreshValue.ReceiverPlus0COldReadValue == 0xFFFFFFFF
            && missingFreshValue.ReceiverPlus0CWriteReached
            && missingFreshValue.ReceiverPlus0CWriteValue == 7
            && missingFreshValue.ReceiverPlus0CFreshReadReached
            && !missingFreshValue.ReceiverPlus0CFreshCaptured
            && missingFreshValue.UnreachedReceiverPlus04CaptureProvided
            && missingFreshValue.UnreachedFirstSlotDwordCaptureProvided
            && missingFreshValue.StopAtRawFileOffsetExclusive == 0x00BE64D8
            && missingBase.Outcome == FrameHandoffHelperSlotOutcome.MissingReceiverPlus04Capture
            && missingBase.ReceiverPlus0COldReadValue == 0xFFFFFFFF
            && missingBase.ReceiverPlus0CFreshReadValue == 4
            && missingBase.WrappingEdiPlusReceiverPlus0CFresh == 7
            && missingBase.ShiftedEdiPlusReceiverPlus0CFresh == 1
            && missingBase.FirstSlotIndex == 1
            && missingBase.ReceiverPlus04ReadReached
            && !missingBase.ReceiverPlus04Captured
            && missingBase.UnreachedFirstSlotDwordCaptureProvided
            && missingBase.StopAtRawFileOffsetExclusive == 0x00BE64E2
            && missingSlot.Outcome
                == FrameHandoffHelperSlotOutcome.MissingFirstSlotDwordCapture
            && missingSlot.FirstSlotIndex == 1
            && missingSlot.FirstSlotByteOffset == 4
            && missingSlot.FirstSlotAddress == 9
            && missingSlot.FirstSlotDwordReadReached
            && !missingSlot.FirstSlotDwordCaptured
            && missingSlot.StopAtRawFileOffsetExclusive == 0x00BE64EC);

        FrameHandoffHelperSlotPlan wrapping =
            FrameHandoffHelperSlotProjection.Project(
                new(0xFFFFFFF8, 0x40000001, 0xFFFFFFFF, 0xFFFFFFFF, 2, 0xFFFFFFFE, 0));
        check("The mask, decrement, EDI addition, scaled slot address, and receiver fields wrap as DWORD operations",
            wrapping.Outcome == FrameHandoffHelperSlotOutcome.FirstSlotZeroFallsThrough
            && wrapping.MaskFromReceiverPlus08 == 3
            && wrapping.DecrementedReceiverPlus08 == 0x40000000
            && wrapping.ReceiverPlus0COldReadValue == 0xFFFFFFFF
            && wrapping.ReceiverPlus0CWriteValue == 3
            && wrapping.ReceiverPlus0CFreshReadValue == 2
            && wrapping.WrappingEdiPlusReceiverPlus0CFresh == 1
            && wrapping.ShiftedEdiPlusReceiverPlus0CFresh == 0
            && wrapping.FirstSlotIndex == 0
            && wrapping.FirstSlotByteOffset == 0
            && wrapping.FirstSlotAddress == 0xFFFFFFFE
            && wrapping.FirstSlotDwordReadValue == 0
            && wrapping.FirstSlotZeroTestProjected
            && wrapping.FirstSlotNonzero == false
            && wrapping.SelectedDestinationRawFileOffset == 0x00BE64F2
            && wrapping.StopAtRawFileOffsetExclusive == 0x00BE64F2);

        FrameHandoffHelperSlotPlan zeroSlot =
            FrameHandoffHelperSlotProjection.Project(new(0x7000, 2, 1, 0xA5, 3, 0x8000, 0));
        FrameHandoffHelperSlotPlan nonzeroSlot =
            FrameHandoffHelperSlotProjection.Project(
                new(0x8000, 2, 1, 0xA5, 3, 0x8000, 0x80000000));
        check("Zero falls through at raw 0xBE64F2 while nonzero branches to raw 0xBE650E",
            zeroSlot.Outcome == FrameHandoffHelperSlotOutcome.FirstSlotZeroFallsThrough
            && zeroSlot.ReceiverPlus0COldReadValue == 0xA5
            && zeroSlot.ReceiverPlus0CWriteValue == 5
            && zeroSlot.ReceiverPlus0CFreshReadValue == 3
            && zeroSlot.FirstSlotIndex == 1
            && zeroSlot.FirstSlotAddress == 0x8004
            && zeroSlot.ReceiverPlus04ReadValue == 0x8000
            && zeroSlot.FirstSlotDwordReadValue == 0
            && zeroSlot.FirstSlotZeroTestProjected
            && zeroSlot.FirstSlotNonzero == false
            && zeroSlot.SelectedDestinationRawFileOffset == 0x00BE64F2
            && zeroSlot.StopAtRawFileOffsetExclusive == 0x00BE64F2
            && nonzeroSlot.Outcome == FrameHandoffHelperSlotOutcome.FirstSlotNonzeroBranches
            && nonzeroSlot.FirstSlotIndex == 1
            && nonzeroSlot.FirstSlotAddress == 0x8004
            && nonzeroSlot.FirstSlotDwordReadValue == 0x80000000
            && nonzeroSlot.FirstSlotZeroTestProjected
            && nonzeroSlot.FirstSlotNonzero == true
            && nonzeroSlot.SelectedDestinationRawFileOffset == 0x00BE650E
            && nonzeroSlot.StopAtRawFileOffsetExclusive == 0x00BE650E);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, uint Virtual, string Bytes, uint? TargetRaw, uint? TargetVirtual)[]
        {
            (0x00BE64C5, 0x00FE70C5, "8B 4E 08", null, null),
            (0x00BE64C8, 0x00FE70C8, "53", null, null),
            (0x00BE64C9, 0x00FE70C9, "57", null, null),
            (0x00BE64CA, 0x00FE70CA, "8B 7E 10", null, null),
            (0x00BE64CD, 0x00FE70CD, "8D 04 8D FF FF FF FF", null, null),
            (0x00BE64D4, 0x00FE70D4, "49", null, null),
            (0x00BE64D5, 0x00FE70D5, "21 46 0C", null, null),
            (0x00BE64D8, 0x00FE70D8, "03 7E 0C", null, null),
            (0x00BE64DB, 0x00FE70DB, "8B C7", null, null),
            (0x00BE64DD, 0x00FE70DD, "C1 E8 02", null, null),
            (0x00BE64E0, 0x00FE70E0, "23 C8", null, null),
            (0x00BE64E2, 0x00FE70E2, "8B 46 04", null, null),
            (0x00BE64E5, 0x00FE70E5, "8D 1C 8D 00 00 00 00", null, null),
            (0x00BE64EC, 0x00FE70EC, "83 3C 03 00", null, null),
            (0x00BE64F0, 0x00FE70F0, "75 1C", 0x00BE650E, 0x00FE710E)
        };
        uint nextRawOffset = FrameHandoffHelperSlotProjection.EntryRawFileOffset;
        string instructionBytes = string.Empty;
        foreach (FrameHandoffHelperSlotInstructionEvidence instruction
            in FrameHandoffHelperSlotProjection.Instructions)
        {
            string bytes = string.Concat(
                instruction.Bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.VirtualAddress
                    != FrameHandoffHelperSlotProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            if (instruction.BranchTargetRawFileOffset is uint targetRaw
                && instruction.BranchTargetVirtualAddress
                    != FrameHandoffHelperSlotProjection.RawFileOffsetToVirtualAddress(targetRaw))
            {
                return false;
            }

            nextRawOffset += (uint)(bytes.Length / 2);
            instructionBytes += bytes;
        }

        return nextRawOffset == FrameHandoffHelperSlotProjection.EndRawFileOffsetExclusive
            && instructionBytes
                == string.Concat(FrameHandoffHelperSlotProjection.CapturedWindowBytes
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            && FrameHandoffHelperSlotProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.VirtualAddress,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static uint ShortBranchTarget(uint instructionRawFileOffset, byte displacement) =>
        unchecked(instructionRawFileOffset + 2 + (uint)(sbyte)displacement);
}
