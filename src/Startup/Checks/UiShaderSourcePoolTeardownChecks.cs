using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolTeardownChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Teardown entry diagnostic gates retain their independent raw reads and reporter cell",
            UiShaderSourcePoolTeardownNativeLayout.EntryDiagnosticInstructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.ThisRelativeOffset,
                        instruction.TargetVirtualAddress))
                .SequenceEqual(new
                    (uint RawFileOffset, string Bytes, int? ThisRelativeOffset, uint? TargetVirtualAddress)[]
                {
                    (0x12CC1D0, "80 3D 95 B0 E4 01 00", null, 0x1E4B095),
                    (0x12CC1DB, "75 17", null, null),
                    (0x12CC1DF, "6A 76", null, null),
                    (0x12CC1EB, "FF 15 A0 5E C1 01", null, 0x1C15EA0),
                    (0x12CC1F1, "83 C4 10", null, null),
                    (0x12CC1F4, "80 7E 04 00", 0x04, null),
                    (0x12CC1F8, "74 06", null, null),
                    (0x12CC1FA, "83 7E 10 00", 0x10, null),
                    (0x12CC1FE, "75 17", null, null),
                    (0x12CC202, "6A 77", null, null),
                    (0x12CC20E, "FF 15 A0 5E C1 01", null, 0x1C15EA0),
                    (0x12CC214, "83 C4 10", null, null),
                    (0x12CC217, "39 35 A8 B0 E4 01", null, 0x1E4B0A8),
                    (0x12CC21D, "74 17", null, null),
                    (0x12CC221, "6A 78", null, null),
                    (0x12CC22D, "FF 15 A0 5E C1 01", null, 0x1C15EA0),
                    (0x12CC233, "83 C4 10", null, null),
                    (0x12CC236, "8B 7E 10 8B 46 0C A3 A8 B0 E4 01", 0x10, 0x1E4B0A8)
                })
            && UiShaderSourcePoolTeardownNativeLayout.GlobalReadinessDiagnosticId == 0x76
            && UiShaderSourcePoolTeardownNativeLayout.ReceiverFlagOrPoolDiagnosticId == 0x77
            && UiShaderSourcePoolTeardownNativeLayout.SingletonListHeadDiagnosticId == 0x78
            && UiShaderSourcePoolTeardownNativeLayout.GlobalReadinessByteVirtualAddress == 0x1E4B095
            && UiShaderSourcePoolTeardownNativeLayout.DiagnosticReporterCellVirtualAddress == 0x1C15EA0
            && UiShaderSourcePoolTeardownNativeLayout.GlobalReadinessBranchRawFileOffset + 2 + 0x17
                == UiShaderSourcePoolTeardownNativeLayout.ReceiverFlagTestRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.ReceiverFlagBranchRawFileOffset + 2 + 0x06
                == UiShaderSourcePoolTeardownNativeLayout.ReceiverFlagDiagnosticSetupRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.ReceiverFlagDiagnosticSetupRawFileOffset + 2
                == UiShaderSourcePoolTeardownNativeLayout.ReceiverFlagDiagnosticIdPushRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.GlobalReadinessDiagnosticIdPushRawFileOffset + 12
                == UiShaderSourcePoolTeardownNativeLayout.GlobalReadinessDiagnosticCallRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.ReceiverFlagDiagnosticIdPushRawFileOffset + 12
                == UiShaderSourcePoolTeardownNativeLayout.ReceiverFlagDiagnosticCallRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.SingletonListHeadDiagnosticIdPushRawFileOffset + 12
                == UiShaderSourcePoolTeardownNativeLayout.SingletonListHeadDiagnosticCallRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.GlobalReadinessDiagnosticCallRawFileOffset + 6
                == UiShaderSourcePoolTeardownNativeLayout.GlobalReadinessDiagnosticCleanupRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.GlobalReadinessDiagnosticCleanupRawFileOffset + 3
                == UiShaderSourcePoolTeardownNativeLayout.ReceiverFlagTestRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.ReceiverFlagDiagnosticCallRawFileOffset + 6
                == UiShaderSourcePoolTeardownNativeLayout.ReceiverFlagDiagnosticCleanupRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.ReceiverFlagDiagnosticCleanupRawFileOffset + 3
                == UiShaderSourcePoolTeardownNativeLayout.SingletonListHeadDiagnosticCompareRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.SingletonListHeadDiagnosticCallRawFileOffset + 6
                == UiShaderSourcePoolTeardownNativeLayout.SingletonListHeadDiagnosticCleanupRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.SingletonListHeadDiagnosticCleanupRawFileOffset + 3
                == UiShaderSourcePoolTeardownNativeLayout.DetachContinuationRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.PoolPointerDiagnosticBranchRawFileOffset + 2 + 0x17
                == UiShaderSourcePoolTeardownNativeLayout.SingletonListHeadDiagnosticCompareRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.SingletonListHeadDiagnosticBranchRawFileOffset + 2 + 0x17
                == UiShaderSourcePoolTeardownNativeLayout.DetachContinuationRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.EntryRawFileOffset
                + UiShaderSourcePoolTeardownNativeLayout.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CCDD0);

        bool entryDiagnosticTruthTableIsExact = true;
        for (int globalReadiness = byte.MinValue;
             globalReadiness <= byte.MaxValue && entryDiagnosticTruthTableIsExact;
             globalReadiness++)
        {
            for (int receiverFlag = byte.MinValue;
                 receiverFlag <= byte.MaxValue && entryDiagnosticTruthTableIsExact;
                 receiverFlag++)
            {
                int poolCases = receiverFlag == 0 ? 1 : 2;
                for (int poolCase = 0; poolCase < poolCases && entryDiagnosticTruthTableIsExact; poolCase++)
                {
                    bool poolIsZero = poolCase == 0;
                    for (int headCase = 0; headCase < 2; headCase++)
                    {
                        bool headsMatch = headCase == 0;
                        var capture = new UiShaderSourcePoolTeardownEntryDiagnosticCapture(
                            ThisPointer: 0x1000,
                            CapturedGlobalReadinessByte: (byte)globalReadiness,
                            CapturedReceiverFlagByte: (byte)receiverFlag,
                            CapturedPoolPointerAfterNonzeroReceiverFlag:
                                receiverFlag == 0 ? (uint?)null : poolIsZero ? 0u : 0x2000u,
                            CapturedLaterSingletonListHead: headsMatch ? 0x1000u : 0x2000u);

                        var expected = new List<uint>(3);
                        if (globalReadiness == 0)
                        {
                            expected.Add(0x76);
                        }

                        if (receiverFlag == 0 || poolIsZero)
                        {
                            expected.Add(0x77);
                        }

                        if (!headsMatch)
                        {
                            expected.Add(0x78);
                        }

                        if (!UiShaderSourcePoolTeardownNativeLayout.ProjectEntryDiagnosticIds(capture)
                            .SequenceEqual(expected))
                        {
                            entryDiagnosticTruthTableIsExact = false;
                            break;
                        }
                    }
                }
            }
        }

        check("Diagnostic projection exhausts all byte pairs and independently crosses reached pool/head gates",
            entryDiagnosticTruthTableIsExact);

        bool rejectsMissingPoolRead = false;
        bool rejectsUnreachedPoolRead = false;
        try
        {
            UiShaderSourcePoolTeardownNativeLayout.ProjectEntryDiagnosticIds(
                new UiShaderSourcePoolTeardownEntryDiagnosticCapture(0x1000, 1, 1, null, 0x1000));
        }
        catch (ArgumentException)
        {
            rejectsMissingPoolRead = true;
        }

        try
        {
            UiShaderSourcePoolTeardownNativeLayout.ProjectEntryDiagnosticIds(
                new UiShaderSourcePoolTeardownEntryDiagnosticCapture(0x1000, 1, 0, 0, 0x1000));
        }
        catch (ArgumentException)
        {
            rejectsUnreachedPoolRead = true;
        }

        check("Diagnostic capture rejects a missing reached pool read and an extra short-circuited pool read",
            rejectsMissingPoolRead && rejectsUnreachedPoolRead);

        check("Shader-source pool teardown anchors match the pinned sample and raw/VA mapping",
            UiShaderSourcePoolTeardownNativeLayout.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolTeardownNativeLayout.EntryRawFileOffset == 0x12CC1D0
            && UiShaderSourcePoolTeardownNativeLayout.EntryReturnRawFileOffset == 0x12CC27E
            && UiShaderSourcePoolTeardownNativeLayout.EntryEndRawFileOffsetExclusive == 0x12CC27F
            && UiShaderSourcePoolTeardownNativeLayout.EntryVirtualAddress == 0x16CCDD0
            && UiShaderSourcePoolTeardownNativeLayout.EntryVirtualAddress
                - UiShaderSourcePoolTeardownNativeLayout.CodeVirtualAddressDeltaFromRawFileOffset
                == UiShaderSourcePoolTeardownNativeLayout.EntryRawFileOffset
            && UiShaderSourcePoolTeardownNativeLayout.SingletonVtableVirtualAddress == 0x1C80CA0
            && UiShaderSourcePoolTeardownNativeLayout.SingletonVtableRawFileOffset == 0x187F8A0
            && UiShaderSourcePoolTeardownNativeLayout.SingletonVtableSlotRawFileOffset == 0x187F8A8
            && UiShaderSourcePoolTeardownNativeLayout.SingletonVtableSlotBytes == "D0 CD 6C 01"
            && UiShaderSourcePoolTeardownNativeLayout.SingletonVtableSlotBytesTargetVirtualAddress
                == UiShaderSourcePoolTeardownNativeLayout.EntryVirtualAddress
            && UiShaderSourcePoolTeardownNativeLayout.SampleObjectVirtualAddress == 0x1E00500
            && UiShaderSourcePoolTeardownNativeLayout.SampleObjectRawFileOffset == 0x19FE900
            && UiShaderSourcePoolTeardownNativeLayout.SampleObjectVtableVirtualAddress
                == UiShaderSourcePoolTeardownNativeLayout.SingletonVtableVirtualAddress
            && UiShaderSourcePoolTeardownNativeLayout.SampleObjectVtableBytes == "A0 0C C8 01");

        check("Fresh instruction anchors preserve capture, global publication, field-clear order, and release boundary",
            UiShaderSourcePoolTeardownNativeLayout.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.ThisRelativeOffset))
                .SequenceEqual(new (uint RawFileOffset, string Bytes, int? ThisRelativeOffset)[]
                {
                    (0x12CC1F4, "80 7E 04 00", 4),
                    (0x12CC217, "39 35 A8 B0 E4 01", null),
                    (0x12CC236, "8B 7E 10", 0x10),
                    (0x12CC239, "8B 46 0C", 0x0C),
                    (0x12CC23C, "A3 A8 B0 E4 01", null),
                    (0x12CC241, "C7 46 0C 00 00 00 00", 0x0C),
                    (0x12CC248, "C7 46 10 00 00 00 00", 0x10),
                    (0x12CC24F, "85 FF", null),
                    (0x12CC251, "74 29", null)
                })
            && UiShaderSourcePoolTeardownNativeLayout.SingletonListHeadGlobalVirtualAddress
                == 0x1E4B0A8
            && UiShaderSourcePoolTeardownNativeLayout.ThisFlagReadOffset == 4
            && UiShaderSourcePoolTeardownNativeLayout.PoolPointerReleaseTestRawFileOffset == 0x12CC24F
            && UiShaderSourcePoolTeardownNativeLayout.PoolPointerReleaseBranchRawFileOffset == 0x12CC251);

        check("Later drain helpers are retained as byte-backed anchors only",
            UiShaderSourcePoolTeardownNativeLayout.DrainHelperAnchors
                .Select(anchor =>
                    (anchor.CallRawFileOffset, anchor.CallBytes, anchor.TargetRawFileOffset))
                .SequenceEqual(new (uint CallRawFileOffset, string CallBytes, uint TargetRawFileOffset)[]
                {
                    (0x12CC256, "E8 85 18 00 00", 0x12CDAE0),
                    (0x12CC262, "E8 F9 CA FF FF", 0x12C8D60)
                }));

        (uint Pool, uint Next)[] crossings =
        {
            (0, 0),
            (0, 0xA1B2C3D4),
            (0x10203040, 0),
            (0x10203040, 0xA1B2C3D4)
        };
        bool capturesRemainIndependent = true;
        bool orderedWritesMatch = true;
        bool poolBranchMatchesCapture = true;
        foreach ((uint pool, uint next) in crossings)
        {
            UiShaderSourcePoolTeardownPlan plan =
                UiShaderSourcePoolTeardownNativeLayout.Project(
                    new UiShaderSourcePoolTeardownCapture(
                        ThisPointer: 0x1000,
                        CapturedPoolPointer: pool,
                        CapturedNextPointer: next));

            capturesRemainIndependent &=
                plan.Capture.CapturedPoolPointer == pool
                && plan.Capture.CapturedNextPointer == next
                && plan.OrderedWrites[0].Value == next
                && plan.ReleaseBoundary.CapturedPoolPointer == pool;
            orderedWritesMatch &=
                plan.OrderedWrites.Length == 3
                && plan.OrderedWrites[0]
                    == new UiShaderSourcePoolTeardownWrite(
                        UiShaderSourcePoolTeardownWriteTarget.GlobalSingletonListHead,
                        0x12CC23C,
                        0x1E4B0A8,
                        next)
                && plan.OrderedWrites[1]
                    == new UiShaderSourcePoolTeardownWrite(
                        UiShaderSourcePoolTeardownWriteTarget.ThisField,
                        0x12CC241,
                        0x100C,
                        0,
                        0x0C)
                && plan.OrderedWrites[2]
                    == new UiShaderSourcePoolTeardownWrite(
                        UiShaderSourcePoolTeardownWriteTarget.ThisField,
                        0x12CC248,
                        0x1010,
                        0,
                        0x10)
                && plan.OrderedWrites
                    .Where(write => write.Target == UiShaderSourcePoolTeardownWriteTarget.ThisField)
                    .Select(write => write.ThisRelativeOffset)
                    .SequenceEqual(new int?[] { 0x0C, 0x10 })
                && plan.OrderedWrites.All(write =>
                    write.ThisRelativeOffset != UiShaderSourcePoolTeardownNativeLayout.ThisFlagReadOffset);
            poolBranchMatchesCapture &=
                plan.ReleaseBoundary.TestRawFileOffset == 0x12CC24F
                && plan.ReleaseBoundary.BranchRawFileOffset == 0x12CC251
                && plan.ReleaseBoundary.SelectsUnmodeledPoolDrain == (pool != 0);
        }

        check("Null-pool/next-pointer crosses preserve independent captures and unconditional ordered detach writes",
            capturesRemainIndependent && orderedWritesMatch);
        check("The release boundary selects only from captured this + 0x10, not captured this + 0x0C",
            poolBranchMatchesCapture);

        UiShaderSourcePoolTeardownPlan wrappedAddresses =
            UiShaderSourcePoolTeardownNativeLayout.Project(
                new UiShaderSourcePoolTeardownCapture(
                    ThisPointer: 0xFFFFFFF8,
                    CapturedPoolPointer: 0x12345678,
                    CapturedNextPointer: 0x87654321));
        check("Captured this-field destination addresses wrap as 32-bit native addresses",
            wrappedAddresses.OrderedWrites[0].TargetVirtualAddress == 0x1E4B0A8
            && wrappedAddresses.OrderedWrites[1].TargetVirtualAddress == 0x00000004
            && wrappedAddresses.OrderedWrites[2].TargetVirtualAddress == 0x00000008
            && wrappedAddresses.OrderedWrites[1].Value == 0
            && wrappedAddresses.OrderedWrites[2].Value == 0);
    }
}
