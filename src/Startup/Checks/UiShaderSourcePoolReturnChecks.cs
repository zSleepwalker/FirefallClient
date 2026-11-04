using Firefall.Reconstruction.Startup;

internal static class UiShaderSourcePoolReturnChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Pool-return anchors identify the requested raw range and sample",
            UiShaderSourcePoolReturnNativeLayout.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && UiShaderSourcePoolReturnNativeLayout.EntryRawFileOffset == 0x12CCA10
            && UiShaderSourcePoolReturnNativeLayout.EndRawFileOffsetExclusive == 0x12CCA7E
            && UiShaderSourcePoolReturnNativeLayout.EntryVirtualAddress == 0x16CD610
            && UiShaderSourcePoolReturnNativeLayout.EntryVirtualAddress
                - UiShaderSourcePoolReturnNativeLayout.CodeVirtualAddressDelta
                == UiShaderSourcePoolReturnNativeLayout.EntryRawFileOffset
            && UiShaderSourcePoolReturnNativeLayout.Instructions[0].Bytes == "83 7F 08 00"
            && UiShaderSourcePoolReturnNativeLayout.Instructions[^1]
                == new UiShaderSourcePoolReturnInstructionEvidence(
                    "Return", 0x12CCA7D, "C3"));

        check("Captured instruction anchors match the exact raw byte sequence and order",
            UiShaderSourcePoolReturnNativeLayout.Instructions
                .Select(instruction => (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(new (uint RawFileOffset, string Bytes)[]
                {
                    (0x12CCA15, "83 7F 08 00"),
                    (0x12CCA19, "74 17"),
                    (0x12CCA1D, "6A 3F"),
                    (0x12CCA29, "FF 15 A0 5E C1 01"),
                    (0x12CCA32, "81 3F 00 08 00 00"),
                    (0x12CCA38, "74 17"),
                    (0x12CCA3C, "6A 40"),
                    (0x12CCA48, "FF 15 A0 5E C1 01"),
                    (0x12CCA51, "8B 4F 08"),
                    (0x12CCA54, "8B 35 10 05 E0 01"),
                    (0x12CCA5A, "F7 D9"),
                    (0x12CCA5C, "C7 47 04 00 00 00 00"),
                    (0x12CCA63, "E8 F8 C2 FF FF"),
                    (0x12CCA68, "83 CA FF"),
                    (0x12CCA6B, "8D 46 10"),
                    (0x12CCA6E, "F0 0F C1 10"),
                    (0x12CCA72, "57"),
                    (0x12CCA73, "8B CE"),
                    (0x12CCA75, "E8 E6 0F 00 00"),
                    (0x12CCA7D, "C3")
                }));

        check("Initial header tests retain their distinct diagnostic IDs, reporter, and return-through branches",
            UiShaderSourcePoolReturnNativeLayout.HeaderSpanOffset == 8
            && UiShaderSourcePoolReturnNativeLayout.HeaderSpanCompareRawFileOffset == 0x12CCA15
            && UiShaderSourcePoolReturnNativeLayout.HeaderSpanDiagnosticBranchRawFileOffset == 0x12CCA19
            && UiShaderSourcePoolReturnNativeLayout.HeaderSpanDiagnosticIdPushRawFileOffset == 0x12CCA1D
            && UiShaderSourcePoolReturnNativeLayout.HeaderSpanDiagnosticCallRawFileOffset == 0x12CCA29
            && UiShaderSourcePoolReturnNativeLayout.HeaderWord0Offset == 0
            && UiShaderSourcePoolReturnNativeLayout.HeaderWord0ExpectedValue == 0x800
            && UiShaderSourcePoolReturnNativeLayout.HeaderWord0CompareRawFileOffset == 0x12CCA32
            && UiShaderSourcePoolReturnNativeLayout.HeaderWord0DiagnosticBranchRawFileOffset == 0x12CCA38
            && UiShaderSourcePoolReturnNativeLayout.HeaderWord0DiagnosticIdPushRawFileOffset == 0x12CCA3C
            && UiShaderSourcePoolReturnNativeLayout.HeaderWord0DiagnosticCallRawFileOffset == 0x12CCA48
            && UiShaderSourcePoolReturnNativeLayout.DiagnosticReporterFunctionPointerVirtualAddress
                == 0x1C15EA0
            && UiShaderSourcePoolReturnNativeLayout.HeaderSpanDiagnosticId == 0x3F
            && UiShaderSourcePoolReturnNativeLayout.HeaderWord0DiagnosticId == 0x40);

        check("Post-diagnostic evidence keeps the later header capture, negation, clear, and helper target exact",
            UiShaderSourcePoolReturnNativeLayout.HeaderSpanRereadRawFileOffset == 0x12CCA51
            && UiShaderSourcePoolReturnNativeLayout.PoolPointerLoadRawFileOffset == 0x12CCA54
            && UiShaderSourcePoolReturnNativeLayout.PoolPointerGlobalVirtualAddress == 0x1E00510
            && UiShaderSourcePoolReturnNativeLayout.HeaderSpanNegateRawFileOffset == 0x12CCA5A
            && UiShaderSourcePoolReturnNativeLayout.HeaderWord4ClearRawFileOffset == 0x12CCA5C
            && UiShaderSourcePoolReturnNativeLayout.HeaderClearedWordOffset == 4
            && UiShaderSourcePoolReturnNativeLayout.OffsetReleaseCallRawFileOffset == 0x12CCA63
            && UiShaderSourcePoolReturnNativeLayout.OffsetReleaseHelperRawFileOffset == 0x12C8D60
            && UiShaderSourcePoolReturnNativeLayout.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CCA63)
                .Bytes == "E8 F8 C2 FF FF");

        check("Pool decrement and handoff anchors preserve their observed sequence and register roles",
            UiShaderSourcePoolReturnNativeLayout.PoolCounterDeltaSetupRawFileOffset == 0x12CCA68
            && UiShaderSourcePoolReturnNativeLayout.PoolCounterAddressSetupRawFileOffset == 0x12CCA6B
            && UiShaderSourcePoolReturnNativeLayout.PoolCounterOffset == 0x10
            && UiShaderSourcePoolReturnNativeLayout.PoolCounterXaddRawFileOffset == 0x12CCA6E
            && UiShaderSourcePoolReturnNativeLayout.PoolCounterXaddDeltaBits == uint.MaxValue
            && UiShaderSourcePoolReturnNativeLayout.HandoffHeaderPushRawFileOffset == 0x12CCA72
            && UiShaderSourcePoolReturnNativeLayout.HandoffPoolRegisterSetupRawFileOffset == 0x12CCA73
            && UiShaderSourcePoolReturnNativeLayout.PoolHandoffCallRawFileOffset == 0x12CCA75
            && UiShaderSourcePoolReturnNativeLayout.PoolHandoffHelperRawFileOffset == 0x12CDA60
            && UiShaderSourcePoolReturnNativeLayout.ReturnRawFileOffset == 0x12CCA7D);

        UiShaderSourcePoolReturnProjection allActions =
            UiShaderSourcePoolReturnNativeLayout.Project(
                capturedHeaderPointer: 0x11112222,
                initialHeaderWord0: 0x7FF,
                initialHeaderSpan: 0x20,
                laterHeaderSpan: 0x08,
                capturedPoolPointer: 0x33334444);
        check("Diagnostics are conditional and later actions remain projected after returning diagnostics",
            allActions.Actions.Select(action => action.Kind).SequenceEqual(
            [
                UiShaderSourcePoolReturnActionKind.ReportHeaderSpanDiagnostic,
                UiShaderSourcePoolReturnActionKind.ReportHeaderWord0Diagnostic,
                UiShaderSourcePoolReturnActionKind.RereadHeaderSpan,
                UiShaderSourcePoolReturnActionKind.LoadPoolPointer,
                UiShaderSourcePoolReturnActionKind.NegateLaterSpanIntoEcx,
                UiShaderSourcePoolReturnActionKind.ClearHeaderWord4,
                UiShaderSourcePoolReturnActionKind.CallOffsetReleaseHelper,
                UiShaderSourcePoolReturnActionKind.AtomicXaddPoolCounter,
                UiShaderSourcePoolReturnActionKind.CallPoolHandoffHelper,
                UiShaderSourcePoolReturnActionKind.Return
            ])
            && allActions.Actions[0].DiagnosticId == 0x3F
            && allActions.Actions[1].DiagnosticId == 0x40
            && allActions.Actions[0].CapturedValue == 0x20
            && allActions.Actions[1].CapturedValue == 0x7FF);

        UiShaderSourcePoolReturnProjection noDiagnostics =
            UiShaderSourcePoolReturnNativeLayout.Project(
                capturedHeaderPointer: 0xAAAA5555,
                initialHeaderWord0: 0x800,
                initialHeaderSpan: 0,
                laterHeaderSpan: 0x100,
                capturedPoolPointer: 0x22223333);
        check("Initial and reread header +8 captures remain distinct in pure diagnostic projection",
            noDiagnostics.InitialHeaderSpan == 0
            && noDiagnostics.LaterHeaderSpan == 0x100
            && noDiagnostics.NegatedLaterSpan == 0xFFFFFF00
            && noDiagnostics.Actions[0].Kind == UiShaderSourcePoolReturnActionKind.RereadHeaderSpan
            && noDiagnostics.Actions[0].CapturedValue == 0x100
            && noDiagnostics.Actions.All(action =>
                action.Kind is not UiShaderSourcePoolReturnActionKind.ReportHeaderSpanDiagnostic
                and not UiShaderSourcePoolReturnActionKind.ReportHeaderWord0Diagnostic));

        check("Captured helper argument stays the negated span, while clear, XADD, and handoff use their observed values",
            allActions.NegatedLaterSpan == 0xFFFFFFF8
            && allActions.Actions[4].Kind == UiShaderSourcePoolReturnActionKind.NegateLaterSpanIntoEcx
            && allActions.Actions[5].Kind == UiShaderSourcePoolReturnActionKind.ClearHeaderWord4
            && allActions.Actions[6].CapturedValue == 0xFFFFFFF8
            && allActions.Actions[6].TargetRawFileOffset == 0x12C8D60
            && allActions.Actions[7].CapturedValue == uint.MaxValue
            && allActions.Actions[7].TargetAddress == 0x33334454
            && allActions.Actions[8].CapturedValue == 0x11112222
            && allActions.Actions[8].SecondaryValue == 0x33334444
            && allActions.Actions[8].TargetRawFileOffset == 0x12CDA60
            && allActions.Actions[9].Kind == UiShaderSourcePoolReturnActionKind.Return);

        check("Instruction evidence marks only the observed helper, global, and pool targets",
            UiShaderSourcePoolReturnNativeLayout.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CCA54)
                .TargetVirtualAddress == 0x1E00510
            && UiShaderSourcePoolReturnNativeLayout.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CCA63)
                .TargetRawFileOffset == 0x12C8D60
            && UiShaderSourcePoolReturnNativeLayout.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12CCA75)
                .TargetRawFileOffset == 0x12CDA60
            && UiShaderSourcePoolReturnNativeLayout.Instructions.All(instruction =>
                instruction.RawFileOffset != 0x12CCA5A
                || instruction.TargetRawFileOffset is null));
    }
}
