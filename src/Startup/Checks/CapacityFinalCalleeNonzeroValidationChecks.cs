using Firefall.Reconstruction.Startup;

internal static class CapacityFinalCalleeNonzeroValidationChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("The bounded instruction bytes, offsets, VA mapping, and memory operands match",
            CapacityFinalCalleeNonzeroValidationProjection.EntryRawFileOffset
                == 0x012C8E45
            && CapacityFinalCalleeNonzeroValidationProjection.EntryVirtualAddress
                == 0x016C9A45
            && CapacityFinalCalleeNonzeroValidationProjection.BoundaryRawFileOffsetExclusive
                == 0x012C8E76
            && CapacityFinalCalleeNonzeroValidationProjection.BoundaryVirtualAddressExclusive
                == 0x016C9A76
            && CapacityFinalCalleeNonzeroValidationProjection.GlobalDwordVirtualAddress
                == 0x01EDADDC
            && CapacityFinalCalleeNonzeroValidationProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeNonzeroValidationProjection.EntryRawFileOffset)
                == CapacityFinalCalleeNonzeroValidationProjection.EntryVirtualAddress
            && CapacityFinalCalleeNonzeroValidationProjection.RawFileOffsetToVirtualAddress(
                CapacityFinalCalleeNonzeroValidationProjection.BoundaryRawFileOffsetExclusive)
                == CapacityFinalCalleeNonzeroValidationProjection.BoundaryVirtualAddressExclusive
            && Normalize(CapacityFinalCalleeNonzeroValidationProjection.CapturedWindowBytes)
                == "03F83BF3722B3B35DCADED010F844B020000C1E8038945F083F820"
                    + "0F83100100008B4E08894DFC8B4E0C894DEC3BF17506"
            && CapacityFinalCalleeNonzeroValidationProjection.OrderedReadDescriptors.SequenceEqual(
                new[]
                {
                    new CapacityFinalCalleeNonzeroValidationReadDescriptor(
                        0x012C8E4B,
                        CapacityFinalCalleeNonzeroValidationReadKind.GlobalDword,
                        0x01EDADDC,
                        null,
                        0),
                    new CapacityFinalCalleeNonzeroValidationReadDescriptor(
                        0x012C8E66,
                        CapacityFinalCalleeNonzeroValidationReadKind.EsiPlus8Dword,
                        null,
                        "ESI",
                        8),
                    new CapacityFinalCalleeNonzeroValidationReadDescriptor(
                        0x012C8E6C,
                        CapacityFinalCalleeNonzeroValidationReadKind.EsiPlusCDword,
                        null,
                        "ESI",
                        0xC)
                }));

        CapacityFinalCalleeNonzeroValidationPlan invalidEntry =
            CapacityFinalCalleeNonzeroValidationProjection.Project(Capture(
                validated: false,
                globalDword: 0,
                esiPlus8Dword: 0,
                esiPlusCDword: 0));
        check("An unvalidated entry executes no instructions and marks all reads unreached",
            invalidEntry.Outcome
                == CapacityFinalCalleeNonzeroValidationOutcome.EntryNotValidated
            && !invalidEntry.EdiAddReached
            && invalidEntry.EdiAfterAdd is null
            && !invalidEntry.CompareEsiEbxReached
            && invalidEntry.Reads.IsEmpty
            && invalidEntry.Writes.IsEmpty
            && invalidEntry.UnreachedGlobalDwordCaptureProvided
            && invalidEntry.UnreachedEsiPlus8DwordCaptureProvided
            && invalidEntry.UnreachedEsiPlusCDwordCaptureProvided
            && invalidEntry.StopAtRawFileOffsetExclusive == 0x012C8E45);

        CapacityFinalCalleeNonzeroValidationPlan unsignedBelow =
            CapacityFinalCalleeNonzeroValidationProjection.Project(Capture(
                esi: 0x7FFFFFFF,
                ebx: 0x80000000,
                globalDword: 0,
                esiPlus8Dword: 0,
                esiPlusCDword: 0));
        CapacityFinalCalleeNonzeroValidationPlan highBitUnsignedAbove =
            CapacityFinalCalleeNonzeroValidationProjection.Project(Capture(
                esi: 0x80000000,
                ebx: 0x7FFFFFFF,
                globalDword: 0x80000001,
                esiPlus8Dword: 0,
                esiPlusCDword: 0));
        CapacityFinalCalleeNonzeroValidationPlan equalGlobal =
            CapacityFinalCalleeNonzeroValidationProjection.Project(Capture(
                esi: 0x80000000,
                ebx: 0x7FFFFFFF,
                globalDword: 0x80000000,
                esiPlus8Dword: 0xAABBCCDD,
                esiPlusCDword: 0x11223344));
        check("The first unsigned compare and equal-global branch stop at their observed destinations",
            unsignedBelow.Outcome
                == CapacityFinalCalleeNonzeroValidationOutcome
                    .EntryEsiUnsignedBelowEntryEbx
            && unsignedBelow.CompareEsiEbxFlags is
            {
                Carry: true,
                Zero: false,
                Sign: true,
                Overflow: true
            }
            && unsignedBelow.EntryEsiUnsignedBelowEntryEbx == true
            && !unsignedBelow.GlobalReadReached
            && unsignedBelow.UnreachedGlobalDwordCaptureProvided
            && unsignedBelow.UnreachedEsiPlus8DwordCaptureProvided
            && unsignedBelow.UnreachedEsiPlusCDwordCaptureProvided
            && unsignedBelow.StopAtRawFileOffsetExclusive == 0x012C8E76
            && highBitUnsignedAbove.CompareEsiEbxFlags is
            {
                Carry: false,
                Zero: false
            }
            && highBitUnsignedAbove.GlobalReadReached
            && highBitUnsignedAbove.GlobalEqualsBranchReached
            && highBitUnsignedAbove.CompareEsiGlobalFlags?.Zero == false
            && highBitUnsignedAbove.EntryEsiEqualsGlobalDword == false
            && equalGlobal.Outcome
                == CapacityFinalCalleeNonzeroValidationOutcome.EntryEsiEqualsGlobalDword
            && equalGlobal.CompareEsiGlobalFlags is
            {
                Carry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && equalGlobal.EntryEsiEqualsGlobalDword == true
            && equalGlobal.EsiPlus8ReadReached == false
            && equalGlobal.UnreachedEsiPlus8DwordCaptureProvided
            && equalGlobal.UnreachedEsiPlusCDwordCaptureProvided
            && equalGlobal.StopAtRawFileOffsetExclusive == 0x012C90A2);

        CapacityFinalCalleeNonzeroValidationPlan missingGlobal =
            CapacityFinalCalleeNonzeroValidationProjection.Project(Capture(
                globalDword: null,
                esiPlus8Dword: 0x12345678,
                esiPlusCDword: 0));
        CapacityFinalCalleeNonzeroValidationPlan capacityBranch =
            CapacityFinalCalleeNonzeroValidationProjection.Project(Capture(
                eax: 0x100,
                esi: 0x80000000,
                ebx: 0x7FFFFFFF,
                globalDword: 0x7FFFFFFF,
                esiPlus8Dword: 0,
                esiPlusCDword: 1));
        CapacityFinalCalleeNonzeroValidationPlan missingEsiPlus8 =
            CapacityFinalCalleeNonzeroValidationProjection.Project(Capture(
                eax: 0xFF,
                esi: 0x1000,
                ebx: 0,
                globalDword: 0,
                esiPlus8Dword: null,
                esiPlusCDword: 0x1000));
        check("Missing global and the unsigned capacity exit preserve only preceding effects",
            missingGlobal.Outcome
                == CapacityFinalCalleeNonzeroValidationOutcome.MissingGlobalDword
            && missingGlobal.EdiAddReached
            && missingGlobal.CompareEsiEbxReached
            && missingGlobal.GlobalReadReached
            && missingGlobal.Reads.Length == 1
            && !missingGlobal.Reads[0].ValueCaptured
            && missingGlobal.UnreachedEsiPlus8DwordCaptureProvided
            && missingGlobal.UnreachedEsiPlusCDwordCaptureProvided
            && missingGlobal.StopAtRawFileOffsetExclusive == 0x012C8E4B
            && capacityBranch.Outcome
                == CapacityFinalCalleeNonzeroValidationOutcome
                    .ShiftedEaxUnsignedAboveOrEqual32
            && capacityBranch.ShiftedEax == 0x20
            && capacityBranch.CompareShiftedEaxFlags is
            {
                Carry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && capacityBranch.ShiftedEaxUnsignedAboveOrEqual32 == true
            && capacityBranch.Writes.SequenceEqual(new[]
            {
                new CapacityFinalCalleeNonzeroValidationWrite(
                    0x012C8E5A,
                    0x1FF0,
                    0x20)
            })
            && capacityBranch.UnreachedEsiPlus8DwordCaptureProvided
            && capacityBranch.UnreachedEsiPlusCDwordCaptureProvided
            && capacityBranch.StopAtRawFileOffsetExclusive == 0x012C8F76
            && missingEsiPlus8.Outcome
                == CapacityFinalCalleeNonzeroValidationOutcome.MissingEsiPlus8Dword
            && missingEsiPlus8.GlobalDwordCaptured
            && missingEsiPlus8.GlobalDword == 0
            && missingEsiPlus8.CompareEsiGlobalReached
            && missingEsiPlus8.CompareEsiGlobalFlags?.Zero == false
            && missingEsiPlus8.Writes.Length == 1
            && missingEsiPlus8.Writes[0].Value == 0x1F
            && missingEsiPlus8.EsiPlus8ReadReached
            && !missingEsiPlus8.EsiPlus8DwordCaptured
            && missingEsiPlus8.UnreachedEsiPlusCDwordCaptureProvided
            && missingEsiPlus8.StopAtRawFileOffsetExclusive == 0x012C8E66);

        CapacityFinalCalleeNonzeroValidationPlan missingEsiPlusC =
            CapacityFinalCalleeNonzeroValidationProjection.Project(Capture(
                eax: 0xFF,
                esi: 0x1000,
                ebx: 0,
                globalDword: 0,
                esiPlus8Dword: 0,
                esiPlusCDword: null));
        CapacityFinalCalleeNonzeroValidationPlan finalUnequal =
            CapacityFinalCalleeNonzeroValidationProjection.Project(Capture(
                eax: 0xFF,
                edi: 0xFFFFFFFF,
                esi: 0x80000000,
                ebx: 0x7FFFFFFF,
                ebp: 0x10,
                globalDword: 0x7FFFFFFF,
                esiPlus8Dword: 0,
                esiPlusCDword: 0x7FFFFFFF));
        CapacityFinalCalleeNonzeroValidationPlan finalEqual =
            CapacityFinalCalleeNonzeroValidationProjection.Project(Capture(
                eax: 0xFF,
                edi: 0xFFFFFFFF,
                esi: 0x80000000,
                ebx: 0x7FFFFFFF,
                ebp: 0x10,
                globalDword: 0x7FFFFFFF,
                esiPlus8Dword: 0,
                esiPlusCDword: 0x80000000));
        check("Ordered stores survive a missing final read; zero and wrapped values reach both final routes",
            missingEsiPlusC.Outcome
                == CapacityFinalCalleeNonzeroValidationOutcome.MissingEsiPlusCDword
            && missingEsiPlusC.Writes.Select(write => write.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C8E5A, 0x012C8E69 })
            && missingEsiPlusC.Writes[1].Value == 0
            && missingEsiPlusC.UnreachedEsiPlusCDwordCaptureProvided == false
            && missingEsiPlusC.StopAtRawFileOffsetExclusive == 0x012C8E6C
            && finalUnequal.Outcome
                == CapacityFinalCalleeNonzeroValidationOutcome
                    .EntryEsiNotEqualEsiPlusCDword
            && finalUnequal.EntryEsiNotEqualEsiPlusCDword == true
            && finalUnequal.CompareEsiEsiPlusCFlags is
            {
                Carry: false,
                Zero: false,
                Sign: false,
                Overflow: true
            }
            && finalUnequal.Writes.Select(write => write.RawFileOffset).SequenceEqual(
                new uint[] { 0x012C8E5A, 0x012C8E69, 0x012C8E6F })
            && finalUnequal.Writes[0].Address == 0
            && finalUnequal.Writes[1].Address == 0xC
            && finalUnequal.Writes[2].Address == 0xFFFFFFFC
            && finalUnequal.EdiAfterAdd == 0xFE
            && finalUnequal.EsiPlus8Address == 0x80000008
            && finalUnequal.EsiPlusCAddress == 0x8000000C
            && finalUnequal.StopAtRawFileOffsetExclusive == 0x012C8E7C
            && finalEqual.Outcome
                == CapacityFinalCalleeNonzeroValidationOutcome
                    .EntryEsiEqualsEsiPlusCDword
            && finalEqual.EntryEsiNotEqualEsiPlusCDword == false
            && finalEqual.CompareEsiEsiPlusCFlags is
            {
                Carry: false,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && finalEqual.StopAtRawFileOffsetExclusive == 0x012C8E76
            && CapacityFinalCalleeNonzeroValidationProjection.EsiPlus8Address(
                0xFFFFFFF8) == 0
            && CapacityFinalCalleeNonzeroValidationProjection.EsiPlusCAddress(
                0xFFFFFFF8) == 4
            && CapacityFinalCalleeNonzeroValidationProjection.EbpMinus10Address(0)
                == 0xFFFFFFF0
            && CapacityFinalCalleeNonzeroValidationProjection.EbpMinus4Address(0)
                == 0xFFFFFFFC
            && CapacityFinalCalleeNonzeroValidationProjection.EbpMinus14Address(0)
                == 0xFFFFFFEC);
    }

    private static CapacityFinalCalleeNonzeroValidationCapture Capture(
        bool validated = true,
        uint eax = 0xFF,
        uint edi = 0x100,
        uint esi = 0x1000,
        uint ebx = 0,
        uint ebp = 0x2000,
        uint? globalDword = 0,
        uint? esiPlus8Dword = 0,
        uint? esiPlusCDword = 0x1000) =>
        new(
            validated,
            eax,
            edi,
            esi,
            ebx,
            ebp,
            globalDword,
            esiPlus8Dword,
            esiPlusCDword);

    private static string Normalize(string bytes) =>
        string.Concat(bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
