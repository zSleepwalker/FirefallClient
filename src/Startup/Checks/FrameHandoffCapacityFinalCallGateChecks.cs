using Firefall.Reconstruction.Startup;

internal static class FrameHandoffCapacityFinalCallGateChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The pinned PE identity and exact 15 raw bytes cover BE6CA2 through BE6CB0",
            FrameHandoffCapacityFinalCallGateProjection.SamplePath
                == @"D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe"
            && FrameHandoffCapacityFinalCallGateProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && FrameHandoffCapacityFinalCallGateProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && FrameHandoffCapacityFinalCallGateProjection.EntryRawFileOffset == 0x00BE6CA2
            && FrameHandoffCapacityFinalCallGateProjection.EntryVirtualAddress == 0x00FE78A2
            && FrameHandoffCapacityFinalCallGateProjection.NormalBoundaryRawFileOffsetExclusive
                == 0x00BE6CB1
            && FrameHandoffCapacityFinalCallGateProjection.NormalBoundaryVirtualAddressExclusive
                == 0x00FE78B1
            && FrameHandoffCapacityFinalCallGateProjection.ZeroBranchDestinationRawFileOffset
                == 0x00BE6CB1
            && FrameHandoffCapacityFinalCallGateProjection.ZeroBranchDestinationVirtualAddress
                == 0x00FE78B1
            && FrameHandoffCapacityFinalCallGateProjection.CallTargetRawFileOffset == 0x012C8D60
            && FrameHandoffCapacityFinalCallGateProjection.CallTargetVirtualAddress == 0x016C9960
            && FrameHandoffCapacityFinalCallGateProjection.CallRelativeDisplacement == 0x006E20AF
            && FrameHandoffCapacityFinalCallGateProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityFinalCallGateProjection.EntryRawFileOffset)
                == FrameHandoffCapacityFinalCallGateProjection.EntryVirtualAddress
            && FrameHandoffCapacityFinalCallGateProjection.RawFileOffsetToVirtualAddress(
                FrameHandoffCapacityFinalCallGateProjection.NormalBoundaryRawFileOffsetExclusive)
                == FrameHandoffCapacityFinalCallGateProjection.NormalBoundaryVirtualAddressExclusive
            && Normalize(FrameHandoffCapacityFinalCallGateProjection.CapturedWindowBytes)
                == "8B7DEC8B4F0485C97405E8AF206E00"
            && FrameHandoffCapacityFinalCallGateProjection.CapturedWindowBytes
                .Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 15
            && InstructionEvidenceMatches());

        FrameHandoffCapacityFinalCallGatePlan invalidJoin =
            FrameHandoffCapacityFinalCallGateProjection.Project(new(
                CaptureValidatedJoin: false,
                EntryEbp: 0x1000,
                FrameMinus14: 0x2000,
                Receiver4: 0,
                NormalCallReturned: true));
        check("An unvalidated join reaches no reads and marks every supplied later capture unreached",
            invalidJoin.Outcome == FrameHandoffCapacityFinalCallGateOutcome.JoinNotValidated
            && invalidJoin.Reads.IsEmpty
            && !invalidJoin.FrameMinus14ReadReached
            && !invalidJoin.Receiver4ReadReached
            && !invalidJoin.TestReached
            && !invalidJoin.BranchReached
            && !invalidJoin.CallReached
            && invalidJoin.UnreachedFrameMinus14CaptureProvided
            && invalidJoin.UnreachedReceiver4CaptureProvided
            && invalidJoin.UnreachedNormalCallStatusProvided
            && invalidJoin.StopAtRawFileOffsetExclusive == 0x00BE6CA2);

        FrameHandoffCapacityFinalCallGatePlan missingFrameMinus14 =
            FrameHandoffCapacityFinalCallGateProjection.Project(new(
                CaptureValidatedJoin: true,
                EntryEbp: 0x10,
                FrameMinus14: null,
                Receiver4: 0,
                NormalCallReturned: true));
        check("A missing frame-minus-14 read stops at CA2 and leaves later captures unreached",
            missingFrameMinus14.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.MissingFrameMinus14
            && missingFrameMinus14.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityFinalCallGateRead(0x00BE6CA2, 0xFFFFFFFC, null)
            })
            && missingFrameMinus14.FrameMinus14ReadReached
            && !missingFrameMinus14.FrameMinus14Captured
            && !missingFrameMinus14.Receiver4ReadReached
            && !missingFrameMinus14.TestReached
            && missingFrameMinus14.UnreachedReceiver4CaptureProvided
            && missingFrameMinus14.UnreachedNormalCallStatusProvided
            && missingFrameMinus14.StopAtRawFileOffsetExclusive == 0x00BE6CA2);

        FrameHandoffCapacityFinalCallGatePlan missingReceiver4 =
            FrameHandoffCapacityFinalCallGateProjection.Project(new(
                CaptureValidatedJoin: true,
                EntryEbp: 0x10,
                FrameMinus14: 0x2000,
                Receiver4: null,
                NormalCallReturned: false));
        check("A missing receiver read stops at CA5 without testing or consuming call status",
            missingReceiver4.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.MissingReceiver4
            && missingReceiver4.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityFinalCallGateRead(0x00BE6CA2, 0xFFFFFFFC, 0x2000),
                new FrameHandoffCapacityFinalCallGateRead(0x00BE6CA5, 0x2004, null)
            })
            && missingReceiver4.FrameMinus14Captured
            && missingReceiver4.EdiAfterFrameMinus14Load == 0x2000
            && missingReceiver4.Receiver4ReadReached
            && !missingReceiver4.Receiver4Captured
            && !missingReceiver4.TestReached
            && missingReceiver4.UnreachedNormalCallStatusProvided
            && !missingReceiver4.CallReached
            && missingReceiver4.StopAtRawFileOffsetExclusive == 0x00BE6CA5);

        FrameHandoffCapacityFinalCallGatePlan zeroReceiver4 =
            FrameHandoffCapacityFinalCallGateProjection.Project(new(
                CaptureValidatedJoin: true,
                EntryEbp: 0x1014,
                FrameMinus14: 0x3000,
                Receiver4: 0,
                NormalCallReturned: null));
        FrameHandoffCapacityFinalCallGatePlan zeroReceiver4WithStatus =
            FrameHandoffCapacityFinalCallGateProjection.Project(new(
                CaptureValidatedJoin: true,
                EntryEbp: 0x1014,
                FrameMinus14: 0x3000,
                Receiver4: 0,
                NormalCallReturned: true));
        check("Zero receiver4 skips the call and exposes outgoing EDI only on that bypass",
            zeroReceiver4.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped
            && zeroReceiver4.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityFinalCallGateRead(0x00BE6CA2, 0x1000, 0x3000),
                new FrameHandoffCapacityFinalCallGateRead(0x00BE6CA5, 0x3004, 0)
            })
            && zeroReceiver4.TestReached
            && zeroReceiver4.Receiver4IsZero == true
            && zeroReceiver4.BranchReached
            && zeroReceiver4.BranchTaken == true
            && zeroReceiver4.SelectedDestinationRawFileOffset == 0x00BE6CB1
            && !zeroReceiver4.CallReached
            && zeroReceiver4.CallReturned is null
            && zeroReceiver4.OutgoingEdi == 0x3000
            && zeroReceiver4.NormalBoundaryAdmitted
            && zeroReceiver4.StopAtRawFileOffsetExclusive == 0x00BE6CB1
            && zeroReceiver4WithStatus.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4StatusCaptureUnreached
            && zeroReceiver4WithStatus.UnreachedNormalCallStatusProvided
            && zeroReceiver4WithStatus.BoundaryAdmissionBlockedByUnreachedCallStatus
            && !zeroReceiver4WithStatus.NormalBoundaryAdmitted
            && zeroReceiver4WithStatus.SelectedDestinationRawFileOffset == 0x00BE6CB1
            && !zeroReceiver4WithStatus.CallReached
            && zeroReceiver4WithStatus.OutgoingEdi == 0x3000);

        FrameHandoffCapacityFinalCallGatePlan nonzeroMissingStatus =
            FrameHandoffCapacityFinalCallGateProjection.Project(new(
                CaptureValidatedJoin: true,
                EntryEbp: 0x2014,
                FrameMinus14: 0x4000,
                Receiver4: 1,
                NormalCallReturned: null));
        FrameHandoffCapacityFinalCallGatePlan nonzeroNonreturn =
            FrameHandoffCapacityFinalCallGateProjection.Project(new(
                CaptureValidatedJoin: true,
                EntryEbp: 0x2014,
                FrameMinus14: 0x4000,
                Receiver4: uint.MaxValue,
                NormalCallReturned: false));
        FrameHandoffCapacityFinalCallGatePlan nonzeroReturned =
            FrameHandoffCapacityFinalCallGateProjection.Project(new(
                CaptureValidatedJoin: true,
                EntryEbp: 0x2014,
                FrameMinus14: 0x4000,
                Receiver4: 1,
                NormalCallReturned: true));
        check("Nonzero receiver4 requires a returning call before normal-boundary admission",
            nonzeroMissingStatus.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.MissingNormalCallStatus
            && nonzeroMissingStatus.CallReached
            && nonzeroMissingStatus.CallReturned is null
            && !nonzeroMissingStatus.NormalBoundaryAdmitted
            && nonzeroMissingStatus.SelectedDestinationRawFileOffset is null
            && nonzeroMissingStatus.OutgoingEdi is null
            && nonzeroMissingStatus.StopAtRawFileOffsetExclusive == 0x00BE6CAC
            && nonzeroNonreturn.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.NormalCallDidNotReturn
            && nonzeroNonreturn.CallReached
            && nonzeroNonreturn.CallReturned == false
            && !nonzeroNonreturn.NormalBoundaryAdmitted
            && nonzeroNonreturn.SelectedDestinationRawFileOffset is null
            && nonzeroNonreturn.OutgoingEdi is null
            && nonzeroNonreturn.StopAtRawFileOffsetExclusive == 0x00BE6CAC
            && nonzeroReturned.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.NormalCallReturned
            && nonzeroReturned.CallReached
            && nonzeroReturned.CallReturned == true
            && nonzeroReturned.NormalBoundaryAdmitted
            && nonzeroReturned.SelectedDestinationRawFileOffset == 0x00BE6CB1
            && nonzeroReturned.OutgoingEdi is null
            && nonzeroReturned.StopAtRawFileOffsetExclusive == 0x00BE6CB1);

        FrameHandoffCapacityFinalCallGatePlan wrappedReadAddresses =
            FrameHandoffCapacityFinalCallGateProjection.Project(new(
                CaptureValidatedJoin: true,
                EntryEbp: 0,
                FrameMinus14: uint.MaxValue,
                Receiver4: 0,
                NormalCallReturned: null));
        check("DWORD read-address arithmetic wraps and a zero pointer value remains a valid capture",
            FrameHandoffCapacityFinalCallGateProjection.FrameMinus14Address(0)
                == 0xFFFFFFEC
            && FrameHandoffCapacityFinalCallGateProjection.Receiver4Address(uint.MaxValue) == 3
            && wrappedReadAddresses.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityFinalCallGateRead(0x00BE6CA2, 0xFFFFFFEC, uint.MaxValue),
                new FrameHandoffCapacityFinalCallGateRead(0x00BE6CA5, 3, 0)
            })
            && wrappedReadAddresses.FrameMinus14Captured
            && wrappedReadAddresses.EdiAfterFrameMinus14Load == uint.MaxValue
            && wrappedReadAddresses.Receiver4Captured
            && wrappedReadAddresses.Receiver4IsZero == true
            && !wrappedReadAddresses.CallReached
            && wrappedReadAddresses.NormalBoundaryAdmitted);

        FrameHandoffCapacityFinalCallGatePlan zeroAddress =
            FrameHandoffCapacityFinalCallGateProjection.Project(new(
                CaptureValidatedJoin: true,
                EntryEbp: 0x14,
                FrameMinus14: 0,
                Receiver4: 0,
                NormalCallReturned: null));
        check("A zero frame-minus-14 value is read and receiver4 is then read at address four",
            zeroAddress.Reads.SequenceEqual(new[]
            {
                new FrameHandoffCapacityFinalCallGateRead(0x00BE6CA2, 0, 0),
                new FrameHandoffCapacityFinalCallGateRead(0x00BE6CA5, 4, 0)
            })
            && zeroAddress.FrameMinus14Captured
            && zeroAddress.EdiAfterFrameMinus14Load == 0
            && zeroAddress.Receiver4Captured
            && zeroAddress.Receiver4Address == 4
            && zeroAddress.Outcome
                == FrameHandoffCapacityFinalCallGateOutcome.ZeroReceiver4CallSkipped
            && zeroAddress.OutgoingEdi == 0
            && zeroAddress.NormalBoundaryAdmitted);
    }

    private static bool InstructionEvidenceMatches()
    {
        var expected = new (
            uint Raw,
            string Bytes,
            uint? BranchTargetRaw,
            uint? BranchTargetVirtual,
            uint? CallTargetRaw,
            uint? CallTargetVirtual)[]
        {
            (0x00BE6CA2, "8B 7D EC", null, null, null, null),
            (0x00BE6CA5, "8B 4F 04", null, null, null, null),
            (0x00BE6CA8, "85 C9", null, null, null, null),
            (0x00BE6CAA, "74 05", 0x00BE6CB1, 0x00FE78B1, null, null),
            (0x00BE6CAC, "E8 AF 20 6E 00", null, null, 0x012C8D60, 0x016C9960)
        };
        uint nextRawOffset = FrameHandoffCapacityFinalCallGateProjection.EntryRawFileOffset;
        string bytes = string.Empty;
        foreach (FrameHandoffCapacityFinalCallGateInstructionEvidence instruction
            in FrameHandoffCapacityFinalCallGateProjection.Instructions)
        {
            string normalized = Normalize(instruction.Bytes);
            if (instruction.RawFileOffset != nextRawOffset
                || instruction.FlatIdaAddress != instruction.RawFileOffset
                || instruction.VirtualAddress
                    != FrameHandoffCapacityFinalCallGateProjection.RawFileOffsetToVirtualAddress(
                        instruction.RawFileOffset)
                || (instruction.BranchTargetRawFileOffset is uint targetRaw
                    && instruction.BranchTargetVirtualAddress
                        != FrameHandoffCapacityFinalCallGateProjection
                            .RawFileOffsetToVirtualAddress(targetRaw))
                || (instruction.CallTargetRawFileOffset is uint callTargetRaw
                    && (instruction.CallTargetVirtualAddress
                        != FrameHandoffCapacityFinalCallGateProjection
                            .RawFileOffsetToVirtualAddress(callTargetRaw))))
            {
                return false;
            }

            nextRawOffset += (uint)(normalized.Length / 2);
            bytes += normalized;
        }

        return nextRawOffset
                == FrameHandoffCapacityFinalCallGateProjection.NormalBoundaryRawFileOffsetExclusive
            && bytes == Normalize(
                FrameHandoffCapacityFinalCallGateProjection.CapturedWindowBytes)
            && FrameHandoffCapacityFinalCallGateProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset,
                        instruction.Bytes,
                        instruction.BranchTargetRawFileOffset,
                        instruction.BranchTargetVirtualAddress,
                        instruction.CallTargetRawFileOffset,
                        instruction.CallTargetVirtualAddress))
                .SequenceEqual(expected);
    }

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
