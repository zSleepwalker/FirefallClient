using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityAboveTransferChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE identity and exact raw bytes cover BE6C57 through BE6C6C",
            FrameHandoffCapacityAboveTransferProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityAboveTransferProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityAboveTransferProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityAboveTransferProjection.EntryRawFileOffset == 0x00BE6C57
            && FrameHandoffCapacityAboveTransferProjection.EntryVirtualAddress == 0x00FE7857
            && FrameHandoffCapacityAboveTransferProjection.NormalBoundaryRawFileOffsetExclusive
                == 0x00BE6C6D
            && FrameHandoffCapacityAboveTransferProjection.NormalBoundaryVirtualAddressExclusive
                == 0x00FE786D
            && FrameHandoffCapacityAboveTransferProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityAboveTransferProjection.EntryRawFileOffset)
                == FrameHandoffCapacityAboveTransferProjection.EntryVirtualAddress
            && FrameHandoffCapacityAboveTransferProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityAboveTransferProjection.NormalBoundaryRawFileOffsetExclusive)
                == FrameHandoffCapacityAboveTransferProjection.NormalBoundaryVirtualAddressExclusive
            && Normalize(FrameHandoffCapacityAboveTransferProjection.CapturedWindowBytes)
                == "8D3CB5000000008BC783E0FC50A19C48AC015152FFD0"
            && FrameHandoffCapacityAboveTransferProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 22
            && InstructionEvidenceMatches());

        FrameHandoffCapacityAboveTransferPlan invalidEntry =
            FrameHandoffCapacityAboveTransferProjection.Project(new(
                ValidatedBranchEntry: false,
                EntryEsi: 0xFFFFFFFF,
                EntryEcx: 0x11,
                EntryEdx: 0x22,
                CallCellDword: 0,
                CallReturnedNormally: true));
        check("An unvalidated entry leaves all instructions and supplied captures unreached",
            invalidEntry.Outcome
                == FrameHandoffCapacityAboveTransferOutcome.EntryNotValidated
            && invalidEntry.Reads.IsEmpty
            && invalidEntry.Pushes.IsEmpty
            && invalidEntry.Scalars.IsEmpty
            && !invalidEntry.EdiLeaReached
            && !invalidEntry.EaxPushReached
            && !invalidEntry.CallCellReadReached
            && !invalidEntry.CallReached
            && invalidEntry.UnreachedCallCellCaptureProvided
            && invalidEntry.UnreachedCallReturnCaptureProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x00BE6C57);

        FrameHandoffCapacityAboveTransferPlan missingCell =
            FrameHandoffCapacityAboveTransferProjection.Project(new(
                ValidatedBranchEntry: true,
                EntryEsi: 3,
                EntryEcx: 0x11,
                EntryEdx: 0x22,
                CallCellDword: null,
                CallReturnedNormally: true));
        check("A missing call-cell read preserves the first push and stops before later pushes",
            missingCell.Outcome
                == FrameHandoffCapacityAboveTransferOutcome.MissingCallCellCapture
            && missingCell.Pushes.SequenceEqual(new[]
            {
                new FrameHandoffCapacityAboveTransferPush(0x00BE6C63, "EAX", 12)
            })
            && missingCell.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityAboveTransferRead(0x00BE6C64, 0x01AC489C, null)
            })
            && missingCell.Scalars.Select(scalar => scalar.RawFileOffset)
                .SequenceEqual(new uint[] { 0x00BE6C57, 0x00BE6C5E, 0x00BE6C60 })
            && missingCell.EaxPushReached
            && missingCell.CallCellReadReached
            && !missingCell.CallCellCaptured
            && !missingCell.EcxPushReached
            && !missingCell.EdxPushReached
            && !missingCell.CallReached
            && missingCell.UnreachedCallReturnCaptureProvided
            && missingCell.StopAtRawFileOffsetExclusive == 0x00BE6C64);

        FrameHandoffCapacityAboveTransferPlan wrapped =
            FrameHandoffCapacityAboveTransferProjection.Project(new(
                ValidatedBranchEntry: true,
                EntryEsi: 0x40000001,
                EntryEcx: 0xAABBCCDD,
                EntryEdx: 0x11223344,
                CallCellDword: 0,
                CallReturnedNormally: true));
        check("LEA multiplication wraps, AND clears low bits, and a zero call target is valid",
            wrapped.Outcome == FrameHandoffCapacityAboveTransferOutcome.ReturnedNormally
            && wrapped.EdiAfterLea == 4
            && wrapped.EaxAfterMove == 4
            && wrapped.EaxAfterAnd == 4
            && wrapped.CallCellCaptured
            && wrapped.CallTargetAddress == 0
            && wrapped.Pushes.SequenceEqual(new[]
            {
                new FrameHandoffCapacityAboveTransferPush(0x00BE6C63, "EAX", 4),
                new FrameHandoffCapacityAboveTransferPush(0x00BE6C69, "ECX", 0xAABBCCDD),
                new FrameHandoffCapacityAboveTransferPush(0x00BE6C6A, "EDX", 0x11223344)
            })
            && wrapped.CallReached
            && wrapped.CallReturnedNormally == true
            && wrapped.NormalBoundaryReached
            && wrapped.StopAtRawFileOffsetExclusive == 0x00BE6C6D);

        FrameHandoffCapacityAboveTransferPlan partialCall =
            Project(callReturnedNormally: null);
        FrameHandoffCapacityAboveTransferPlan nonreturning =
            Project(callReturnedNormally: false);
        check("Missing and nonreturning call outcomes retain all three pushes and stop at CALL",
            partialCall.Outcome
                == FrameHandoffCapacityAboveTransferOutcome.MissingCallReturnCapture
            && partialCall.CallReached
            && partialCall.CallReturnedNormally is null
            && partialCall.Pushes.Length == 3
            && partialCall.Pushes[0].Source == "EAX"
            && partialCall.Pushes[1].Source == "ECX"
            && partialCall.Pushes[2].Source == "EDX"
            && partialCall.StopAtRawFileOffsetExclusive == 0x00BE6C6B
            && nonreturning.Outcome
                == FrameHandoffCapacityAboveTransferOutcome.CallDidNotReturnNormally
            && nonreturning.CallReached
            && nonreturning.CallReturnedNormally == false
            && nonreturning.Pushes.Length == 3
            && !nonreturning.NormalBoundaryReached
            && nonreturning.StopAtRawFileOffsetExclusive == 0x00BE6C6B);
    }

    private static FrameHandoffCapacityAboveTransferPlan Project(bool? callReturnedNormally) =>
        FrameHandoffCapacityAboveTransferProjection.Project(new(
            ValidatedBranchEntry: true,
            EntryEsi: 0xFFFFFFFF,
            EntryEcx: 0x1234,
            EntryEdx: 0x5678,
            CallCellDword: 0,
            CallReturnedNormally: callReturnedNormally));

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (uint Raw, string Bytes)[]
        {
            (0x00BE6C57, "8D 3C B5 00 00 00 00"),
            (0x00BE6C5E, "8B C7"),
            (0x00BE6C60, "83 E0 FC"),
            (0x00BE6C63, "50"),
            (0x00BE6C64, "A1 9C 48 AC 01"),
            (0x00BE6C69, "51"),
            (0x00BE6C6A, "52"),
            (0x00BE6C6B, "FF D0")
        };
        uint nextRawOffset = FrameHandoffCapacityAboveTransferProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (FrameHandoffCapacityAboveTransferInstructionEvidence instruction
            in FrameHandoffCapacityAboveTransferProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacityAboveTransferProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == FrameHandoffCapacityAboveTransferProjection.NormalBoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                FrameHandoffCapacityAboveTransferProjection.CapturedWindowBytes)
            && FrameHandoffCapacityAboveTransferProjection.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
