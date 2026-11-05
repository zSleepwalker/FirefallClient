using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolDrainChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Pool-drain anchors match the pinned sample, raw offsets, and code VA mapping",
            UiShaderSourcePoolDrainProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolDrainProjection.EntryRawFileOffset == 0x12CC1D0
            && UiShaderSourcePoolDrainProjection.EntryReturnRawFileOffset == 0x12CC27E
            && UiShaderSourcePoolDrainProjection.EntryEndRawFileOffsetExclusive == 0x12CC27F
            && UiShaderSourcePoolDrainProjection.EntryRawFileOffset
                + UiShaderSourcePoolDrainProjection.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x16CCDD0);

        check("Captured drain instruction bytes and branch destinations retain the one-step boundary",
            UiShaderSourcePoolDrainProjection.DrainContinuationBytes
                == "85 FF 74 29 8D 4F 08 E8 85 18 00 00 85 C0 74 14 "
                    + "90 8B C8 E8 F9 CA FF FF 8D 4F 08 E8 71 18 00 00 "
                    + "85 C0 75 ED 8B CF 5F 5E E9 E4 CA FF FF 5F 5E C3"
            && UiShaderSourcePoolDrainProjection.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes, instruction.TargetRawFileOffset))
                .SequenceEqual(new (uint RawFileOffset, string Bytes, uint? TargetRawFileOffset)[]
                {
                    (0x12CC24F, "85 FF", null),
                    (0x12CC251, "74 29", 0x12CC27C),
                    (0x12CC253, "8D 4F 08", null),
                    (0x12CC256, "E8 85 18 00 00", 0x12CDAE0),
                    (0x12CC25B, "85 C0", null),
                    (0x12CC25D, "74 14", 0x12CC273),
                    (0x12CC260, "8B C8", null),
                    (0x12CC262, "E8 F9 CA FF FF", 0x12C8D60),
                    (0x12CC267, "8D 4F 08", null),
                    (0x12CC26A, "E8 71 18 00 00", 0x12CDAE0),
                    (0x12CC26F, "85 C0", null),
                    (0x12CC271, "75 ED", 0x12CC260),
                    (0x12CC273, "8B CF", null),
                    (0x12CC277, "E9 E4 CA FF FF", 0x12C8D60),
                    (0x12CC27C, "5F 5E C3", null)
                })
            && UiShaderSourcePoolDrainProjection.NullPoolBranchRawFileOffset + 2 + 0x29
                == UiShaderSourcePoolDrainProjection.NullPoolReturnRawFileOffset
            && UiShaderSourcePoolDrainProjection.EmptyPopBranchRawFileOffset + 2 + 0x14
                == UiShaderSourcePoolDrainProjection.PoolTailTransferRawFileOffset - 4
            && UiShaderSourcePoolDrainProjection.LoopBackBranchRawFileOffset + 2 - 0x13
                == UiShaderSourcePoolDrainProjection.LoopBackTargetRawFileOffset
            && unchecked(UiShaderSourcePoolDrainProjection.InitialPopCallRawFileOffset + 5 + 0x1885)
                == UiShaderSourcePoolDrainProjection.PoolPopHelperRawFileOffset
            && unchecked(UiShaderSourcePoolDrainProjection.CapturedNodeHelperCallRawFileOffset + 5 - 0x3507)
                == UiShaderSourcePoolDrainProjection.NodeAndPoolHelperRawFileOffset
            && unchecked(UiShaderSourcePoolDrainProjection.NextPopCallRawFileOffset + 5 + 0x1871)
                == UiShaderSourcePoolDrainProjection.PoolPopHelperRawFileOffset
            && unchecked(UiShaderSourcePoolDrainProjection.PoolTailTransferRawFileOffset + 5 - 0x351C)
                == UiShaderSourcePoolDrainProjection.NodeAndPoolHelperRawFileOffset);

        UiShaderSourcePoolDrainPlan nullPool =
            UiShaderSourcePoolDrainProjection.Project(
                new UiShaderSourcePoolDrainCapture(0, null));
        check("Null captured pool returns normally without a pop or helper call",
            nullPool.Actions.SequenceEqual(new[]
            {
                new UiShaderSourcePoolDrainAction(
                    UiShaderSourcePoolDrainActionKind.ReturnNormallyWithoutPoolCall,
                    0x12CC251,
                    BranchTargetRawFileOffset: 0x12CC27C)
            })
            && !nullPool.EndsAtCapturedPoolTailTransfer);

        UiShaderSourcePoolDrainPlan emptyPop =
            UiShaderSourcePoolDrainProjection.Project(
                new UiShaderSourcePoolDrainCapture(0x10203040, 0));
        check("Empty pop still tail-transfers the captured nonnull pool to the helper",
            emptyPop.Actions.SequenceEqual(new[]
            {
                new UiShaderSourcePoolDrainAction(
                    UiShaderSourcePoolDrainActionKind.CallPopForCapturedResult,
                    0x12CC256,
                    0x10203048,
                    0x12CDAE0),
                new UiShaderSourcePoolDrainAction(
                    UiShaderSourcePoolDrainActionKind.TailTransferCapturedPoolToHelper,
                    0x12CC277,
                    0x10203040,
                    0x12C8D60)
            })
            && emptyPop.EndsAtCapturedPoolTailTransfer);

        UiShaderSourcePoolDrainPlan oneNode =
            UiShaderSourcePoolDrainProjection.Project(
                new UiShaderSourcePoolDrainCapture(0x10203040, 0x55667788));
        check("One captured node is passed to the helper before a single unmodeled next-pop continuation",
            oneNode.Actions.SequenceEqual(new[]
            {
                new UiShaderSourcePoolDrainAction(
                    UiShaderSourcePoolDrainActionKind.CallPopForCapturedResult,
                    0x12CC256,
                    0x10203048,
                    0x12CDAE0),
                new UiShaderSourcePoolDrainAction(
                    UiShaderSourcePoolDrainActionKind.CallHelperForCapturedNode,
                    0x12CC262,
                    0x55667788,
                    0x12C8D60),
                new UiShaderSourcePoolDrainAction(
                    UiShaderSourcePoolDrainActionKind.CallPopForUnmodeledContinuation,
                    0x12CC26A,
                    0x10203048,
                    0x12CDAE0),
                new UiShaderSourcePoolDrainAction(
                    UiShaderSourcePoolDrainActionKind.ConditionalLoopBackUnmodeled,
                    0x12CC271,
                    BranchTargetRawFileOffset: 0x12CC260)
            })
            && !oneNode.EndsAtCapturedPoolTailTransfer);

        UiShaderSourcePoolDrainPlan wrappingHead =
            UiShaderSourcePoolDrainProjection.Project(
                new UiShaderSourcePoolDrainCapture(0xFFFFFFFC, 0));
        check("Pool-head argument uses unchecked 32-bit pointer arithmetic",
            wrappingHead.Actions[0].Argument == 4);

        bool nullPoolPopRejected = false;
        try
        {
            UiShaderSourcePoolDrainProjection.Project(
                new UiShaderSourcePoolDrainCapture(0, 1));
        }
        catch (ArgumentException exception)
        {
            nullPoolPopRejected = exception.ParamName == "capture";
        }

        bool missingPopRejected = false;
        try
        {
            UiShaderSourcePoolDrainProjection.Project(
                new UiShaderSourcePoolDrainCapture(1, null));
        }
        catch (ArgumentException exception)
        {
            missingPopRejected = exception.ParamName == "capture";
        }

        check("Projection rejects impossible/missing pop captures without simulating further results",
            nullPoolPopRejected && missingPopRejected);
    }
}
