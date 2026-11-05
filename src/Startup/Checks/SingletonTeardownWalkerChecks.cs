using Firefall.Reconstruction.Startup;

internal static class SingletonTeardownWalkerChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("singleton teardown walker records verified raw PE file offsets, VA mapping, and control-flow sites",
            SingletonTeardownWalkerProjectionRules.EntryVa == 0x016C46F0
            && SingletonTeardownWalkerProjectionRules.EntryRawFileOffset == 0x012C3AF0
            && SingletonTeardownWalkerProjectionRules.RawCodeStartFileOffset == 0x012C3AF0
            && SingletonTeardownWalkerProjectionRules.RawCodeEndFileOffsetInclusive == 0x012C3B3E
            && SingletonTeardownWalkerProjectionRules.ObservedVaToRawFileOffsetDelta == 0x00400C00
            && SingletonTeardownWalkerProjectionRules.EntryVa
                == SingletonTeardownWalkerProjectionRules.EntryRawFileOffset
                    + SingletonTeardownWalkerProjectionRules.ObservedVaToRawFileOffsetDelta
            && SingletonTeardownWalkerProjectionRules.DiagnosticGateAddress == 0x01E4B096
            && SingletonTeardownWalkerProjectionRules.DiagnosticReportId == 0x1C
            && SingletonTeardownWalkerProjectionRules.DiagnosticCallCellAddress == 0x01C15EA0
            && SingletonTeardownWalkerProjectionRules.DiagnosticCallRawFileOffset == 0x012C3B07
            && SingletonTeardownWalkerProjectionRules.InitialHeadLoadRawFileOffset == 0x012C3B10
            && SingletonTeardownWalkerProjectionRules.HeadGlobalAddress == 0x01E4B0A8
            && SingletonTeardownWalkerProjectionRules.DiagnosticGateClearRawFileOffset == 0x012C3B16
            && SingletonTeardownWalkerProjectionRules.GuardSetRawFileOffset == 0x012C3B1D
            && SingletonTeardownWalkerProjectionRules.VirtualPointerLoadRawFileOffset == 0x012C3B28
            && SingletonTeardownWalkerProjectionRules.VirtualDispatchRawFileOffset == 0x012C3B2A
            && SingletonTeardownWalkerProjectionRules.VirtualSlotOffset == 8
            && SingletonTeardownWalkerProjectionRules.MaximumCapturedDispatchCount == 1
            && SingletonTeardownWalkerProjectionRules.NextLinkReadCount == 0
            && SingletonTeardownWalkerProjectionRules.HeadReloadRawFileOffset == 0x012C3B2D
            && SingletonTeardownWalkerProjectionRules.RepeatCheckRawFileOffset == 0x012C3B33
            && SingletonTeardownWalkerProjectionRules.RepeatBranchRawFileOffset == 0x012C3B35
            && SingletonTeardownWalkerProjectionRules.NullHeadGuardClearRawFileOffset == 0x012C3B37
            && SingletonTeardownWalkerProjectionRules.RawCodeReturnRawFileOffset == 0x012C3B3E);

        byte[] observedWalkerBytes =
        [
            0x80, 0x3D, 0x96, 0xB0, 0xE4, 0x01, 0x00, 0x75, 0x17, 0x6A, 0x00, 0x6A, 0x1C,
            0x68, 0xF3, 0x3D, 0xB7, 0x01, 0x68, 0xDB, 0x3D, 0xB7, 0x01, 0xFF, 0x15, 0xA0,
            0x5E, 0xC1, 0x01, 0x83, 0xC4, 0x10, 0x8B, 0x0D, 0xA8, 0xB0, 0xE4, 0x01, 0xC6,
            0x05, 0x96, 0xB0, 0xE4, 0x01, 0x00, 0xC6, 0x05, 0x95, 0xB0, 0xE4, 0x01, 0x01,
            0x85, 0xC9, 0x74, 0x0F, 0x8B, 0x01, 0xFF, 0x50, 0x08, 0x8B, 0x0D, 0xA8, 0xB0,
            0xE4, 0x01, 0x85, 0xC9, 0x75, 0xF1, 0xC6, 0x05, 0x95, 0xB0, 0xE4, 0x01, 0x00,
            0xC3
        ];
        check("singleton teardown walker raw instruction bytes match the read-only capture",
            SingletonTeardownWalkerProjectionRules.RawWalkerInstructionBytes.SequenceEqual(observedWalkerBytes)
            && SingletonTeardownWalkerProjectionRules.RawWalkerInstructionBytes.Count
                == SingletonTeardownWalkerProjectionRules.RawCodeEndFileOffsetInclusive
                    - SingletonTeardownWalkerProjectionRules.RawCodeStartFileOffset + 1);

        var nullHead = SingletonTeardownWalkerProjectionRules.Project(
            new SingletonTeardownWalkerCaptures(
                SingletonTeardownCapture<byte>.Captured(0),
                SingletonTeardownCapture<uint>.Captured(0),
                SingletonTeardownCapture<uint>.Unreached()));
        check("zero diagnostic gate reports ID 0x1C and a null initial head clears both bytes in observed order",
            nullHead.Succeeded
            && nullHead.Projection!.Value.DiagnosticReported
            && nullHead.Projection.Value.DiagnosticGateByte == 0
            && nullHead.Projection.Value.InitialHead == 0
            && nullHead.Projection.Value.LaterHead is null
            && nullHead.Projection.Value.Dispatch is null
            && nullHead.Projection.Value.Boundary == SingletonTeardownWalkerBoundary.NullHeadPath
            && nullHead.Projection.Value.OrderedWrites.SequenceEqual(new[]
            {
                new SingletonTeardownWalkerWrite(0x012C3B16, 0x01E4B096, 0),
                new SingletonTeardownWalkerWrite(0x012C3B1D, 0x01E4B095, 1),
                new SingletonTeardownWalkerWrite(0x012C3B37, 0x01E4B095, 0)
            }));

        var oneDispatch = SingletonTeardownWalkerProjectionRules.Project(
            new SingletonTeardownWalkerCaptures(
                SingletonTeardownCapture<byte>.Captured(0x7F),
                SingletonTeardownCapture<uint>.Captured(0x12345678),
                SingletonTeardownCapture<uint>.Captured(0)));
        check("non-null initial head dispatches once through slot 8 with itself as receiver and no explicit arguments",
            oneDispatch.Succeeded
            && !oneDispatch.Projection!.Value.DiagnosticReported
            && oneDispatch.Projection.Value.Dispatch
                == new SingletonTeardownWalkerDispatch(0x012C3B2A, 0x12345678, 8, 0)
            && oneDispatch.Projection.Value.LaterHead == 0
            && oneDispatch.Projection.Value.Boundary == SingletonTeardownWalkerBoundary.NullHeadPath
            && oneDispatch.Projection.Value.OrderedWrites.Count == 3
            && oneDispatch.Projection.Value.OrderedWrites[2]
                == new SingletonTeardownWalkerWrite(0x012C3B37, 0x01E4B095, 0));

        var repeatBoundary = SingletonTeardownWalkerProjectionRules.Project(
            new SingletonTeardownWalkerCaptures(
                SingletonTeardownCapture<byte>.Captured(1),
                SingletonTeardownCapture<uint>.Captured(0x12345678),
                SingletonTeardownCapture<uint>.Captured(0x87654321)));
        check("independently captured non-null later head stops at the repeat boundary without another dispatch or guard clear",
            repeatBoundary.Succeeded
            && repeatBoundary.Projection!.Value.LaterHead == 0x87654321
            && repeatBoundary.Projection.Value.Dispatch.HasValue
            && repeatBoundary.Projection.Value.Boundary == SingletonTeardownWalkerBoundary.RepeatBoundary
            && repeatBoundary.Projection.Value.OrderedWrites.Count == 2);

        var missingDiagnostic = SingletonTeardownWalkerProjectionRules.Project(
            new SingletonTeardownWalkerCaptures(
                SingletonTeardownCapture<byte>.Missing(),
                SingletonTeardownCapture<uint>.Captured(0),
                SingletonTeardownCapture<uint>.Unreached()));
        var unreachedDiagnostic = SingletonTeardownWalkerProjectionRules.Project(
            new SingletonTeardownWalkerCaptures(
                SingletonTeardownCapture<byte>.Unreached(),
                SingletonTeardownCapture<uint>.Captured(0),
                SingletonTeardownCapture<uint>.Unreached()));
        var zeroDiagnostic = SingletonTeardownWalkerProjectionRules.Project(
            new SingletonTeardownWalkerCaptures(
                SingletonTeardownCapture<byte>.Captured(0),
                SingletonTeardownCapture<uint>.Captured(0),
                SingletonTeardownCapture<uint>.Unreached()));
        check("missing, unreached, and captured-zero diagnostic bytes remain distinct",
            missingDiagnostic.Error == SingletonTeardownWalkerCaptureError.MissingDiagnosticGateByte
            && unreachedDiagnostic.Error == SingletonTeardownWalkerCaptureError.UnreachedDiagnosticGateByte
            && zeroDiagnostic.Succeeded
            && zeroDiagnostic.Projection!.Value.DiagnosticReported);

        var missingInitialHead = SingletonTeardownWalkerProjectionRules.Project(
            new SingletonTeardownWalkerCaptures(
                SingletonTeardownCapture<byte>.Captured(1),
                SingletonTeardownCapture<uint>.Missing(),
                SingletonTeardownCapture<uint>.Unreached()));
        var unreachedInitialHead = SingletonTeardownWalkerProjectionRules.Project(
            new SingletonTeardownWalkerCaptures(
                SingletonTeardownCapture<byte>.Captured(1),
                SingletonTeardownCapture<uint>.Unreached(),
                SingletonTeardownCapture<uint>.Unreached()));
        var missingLaterHead = SingletonTeardownWalkerProjectionRules.Project(
            new SingletonTeardownWalkerCaptures(
                SingletonTeardownCapture<byte>.Captured(1),
                SingletonTeardownCapture<uint>.Captured(1),
                SingletonTeardownCapture<uint>.Missing()));
        var unreachedLaterHead = SingletonTeardownWalkerProjectionRules.Project(
            new SingletonTeardownWalkerCaptures(
                SingletonTeardownCapture<byte>.Captured(1),
                SingletonTeardownCapture<uint>.Captured(1),
                SingletonTeardownCapture<uint>.Unreached()));
        var unexpectedlyCapturedLaterHead = SingletonTeardownWalkerProjectionRules.Project(
            new SingletonTeardownWalkerCaptures(
                SingletonTeardownCapture<byte>.Captured(1),
                SingletonTeardownCapture<uint>.Captured(0),
                SingletonTeardownCapture<uint>.Captured(0)));
        check("required missing or unreached heads are rejected and a later head must be unreached after a null initial head",
            missingInitialHead.Error == SingletonTeardownWalkerCaptureError.MissingInitialHead
            && unreachedInitialHead.Error == SingletonTeardownWalkerCaptureError.UnreachedInitialHead
            && missingLaterHead.Error == SingletonTeardownWalkerCaptureError.MissingLaterHead
            && unreachedLaterHead.Error == SingletonTeardownWalkerCaptureError.UnreachedLaterHead
            && unexpectedlyCapturedLaterHead.Error
                == SingletonTeardownWalkerCaptureError.LaterHeadWasReachedWithNullInitialHead);
    }
}
