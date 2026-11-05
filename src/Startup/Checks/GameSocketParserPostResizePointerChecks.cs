using Firefall.Reconstruction.Startup;

internal static class GameSocketParserPostResizePointerChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] bytes = ParseBytes(
            GameSocketParserPostResizePointerProjection.CapturedBytes);
        check("The post-resize caller window is pinned to the authorized sample and raw/VA mapping",
            GameSocketParserPostResizePointerProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && GameSocketParserPostResizePointerProjection.SampleFileSize == 40197848
            && GameSocketParserPostResizePointerProjection.RawToCodeVirtualAddressDelta
                == 0x00400C00
            && GameSocketParserPostResizePointerProjection.RawFileOffsetToCodeVirtualAddress(
                GameSocketParserPostResizePointerProjection.FreshField38LoadRawFileOffset)
                == 0x00FD3DDC
            && GameSocketParserPostResizePointerProjection.CodeVirtualAddressToRawFileOffset(
                0x00FD3DDC)
                == GameSocketParserPostResizePointerProjection.FreshField38LoadRawFileOffset
            && bytes.Length == 30
            && HasExactSlice(bytes, 0x00BD31DC, 0x00BD31DC, "8B 54 24 38")
            && HasExactSlice(bytes, 0x00BD31DC, 0x00BD31E0, "85 D2")
            && HasExactSlice(bytes, 0x00BD31DC, 0x00BD31E2, "74 05")
            && HasExactSlice(bytes, 0x00BD31DC, 0x00BD31E4, "8B 52 04")
            && HasExactSlice(bytes, 0x00BD31DC, 0x00BD31E7, "EB 02")
            && HasExactSlice(bytes, 0x00BD31DC, 0x00BD31E9, "33 D2")
            && HasExactSlice(bytes, 0x00BD31DC, 0x00BD31EB, "89 54 24 14")
            && HasExactSlice(bytes, 0x00BD31DC, 0x00BD31EF, "BF 6E 05 00 00")
            && HasExactSlice(bytes, 0x00BD31DC, 0x00BD31F4, "89 54 24 24")
            && HasExactSlice(bytes, 0x00BD31DC, 0x00BD31F8, "8A 02")
            && GameSocketParserPostResizePointerProjection.ResultEDIValue
                == 0x0000056E
            && !GameSocketParserPostResizePointerProjection.OpaqueResizeTargetExecutionModeled
            && !GameSocketParserPostResizePointerProjection.AllocationSemanticsModeled
            && !GameSocketParserPostResizePointerProjection.ParserExecutionModeled);

        GameSocketParserPostResizePointerPlan normalNull =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                resizeReturnedNormally: true,
                postResizeEsp: 0x3000,
                field38: 0));
        check("A captured null field follows XOR-zero and stores the null result without reading a child",
            normalNull.ParentPlan.IsCleanCompletion
            && normalNull.ParentResizeHandoffAdmitted
            && normalNull.Outcome
                == GameSocketParserPostResizePointerOutcome.BoundedBeforeHeaderByteRead
            && normalNull.ReachedBoundedEndpoint
            && normalNull.ExactBoundedCompletion
            && normalNull.IsCleanCompletion
            && normalNull.ParentPlan.StopBeforeRawFileOffset
                == GameSocketParserPostResizePointerProjection.ResizeCallRawFileOffset
            && normalNull.CaptureStates.FreshPostResizeESP
                == GameSocketParserPostResizePointerCaptureState.Captured
            && normalNull.CaptureStates.FreshField38Dword
                == GameSocketParserPostResizePointerCaptureState.Captured
            && normalNull.CaptureStates.FreshFieldPlus4Dword
                == GameSocketParserPostResizePointerCaptureState.NotReached
            && normalNull.Reads is [{
                Kind: GameSocketParserPostResizePointerReadKind.FreshField38Dword,
                InstructionRawFileOffset:
                    GameSocketParserPostResizePointerProjection.FreshField38LoadRawFileOffset,
                Address: 0x3038,
                Value: 0,
                Attempted: true,
                Captured: true,
                Committed: false
            }]
            && normalNull.ResultEDX == 0
            && normalNull.ResultEDI
                == GameSocketParserPostResizePointerProjection.ResultEDIValue
            && normalNull.Stores is [
                {
                    InstructionRawFileOffset:
                        GameSocketParserPostResizePointerProjection.FirstStackStoreRawFileOffset,
                    StackAddress: 0x3014,
                    Value: 0,
                    Committed: true
                },
                {
                    InstructionRawFileOffset:
                        GameSocketParserPostResizePointerProjection.SecondStackStoreRawFileOffset,
                    StackAddress: 0x3024,
                    Value: 0,
                    Committed: true
                }]
            && normalNull.FlagsAtStop is {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: true,
                Sign: false,
                Overflow: false
            }
            && normalNull.StopBeforeRawFileOffset
                == GameSocketParserPostResizePointerProjection.HeaderByteLoadRawFileOffset
            && normalNull.HeaderByteReadAddress == 0
            && !normalNull.HeaderByteReadAttempted);

        GameSocketParserPostResizePointerPlan nonNullChildZero =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                resizeReturnedNormally: true,
                postResizeEsp: 0x4000,
                field38: 0x80000000,
                fieldPlus4: 0));
        check("A nonzero DWORD is followed by a fresh child read; child zero preserves TEST flags",
            nonNullChildZero.Outcome
                == GameSocketParserPostResizePointerOutcome.BoundedBeforeHeaderByteRead
            && nonNullChildZero.CaptureStates.FreshFieldPlus4Dword
                == GameSocketParserPostResizePointerCaptureState.Captured
            && nonNullChildZero.Reads is [
                {
                    Kind: GameSocketParserPostResizePointerReadKind.FreshField38Dword,
                    Address: 0x4038,
                    Value: 0x80000000,
                    Committed: false
                },
                {
                    Kind: GameSocketParserPostResizePointerReadKind.FreshFieldPlus4Dword,
                    InstructionRawFileOffset:
                        GameSocketParserPostResizePointerProjection.FreshFieldPlus4LoadRawFileOffset,
                    Address: 0x80000004,
                    Value: 0,
                    Attempted: true,
                    Captured: true,
                    Committed: false
                }]
            && nonNullChildZero.ResultEDX == 0
            && nonNullChildZero.FlagsAtStop is {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: true,
                Overflow: false
            }
            && nonNullChildZero.Stores.Length == 2
            && nonNullChildZero.Stores.All(store => store.Value == 0 && store.Committed)
            && nonNullChildZero.ParentPlan.InitialLocalAddress == 0x1034
            && nonNullChildZero.ParentPlan.Capture.PostGetterESP == 0x2000
            && nonNullChildZero.Capture.FreshPostResizeESP == 0x4000);

        GameSocketParserPostResizePointerPlan wrapped =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                resizeReturnedNormally: true,
                postResizeEsp: 0xFFFFFFF0,
                field38: 0xFFFFFFFC,
                fieldPlus4: 0xAABBCCDD));
        check("Post-resize ESP and field-plus-four arithmetic wrap independently",
            wrapped.Addresses.FreshField38ReadAddress == 0x28
            && wrapped.Addresses.FreshFieldPlus4ReadAddress == 0
            && wrapped.Addresses.FirstStackStoreAddress == 4
            && wrapped.Addresses.SecondStackStoreAddress == 0x14
            && wrapped.Reads[1].Address == 0
            && wrapped.Stores is [
                { StackAddress: 4, Value: 0xAABBCCDD, Committed: true },
                { StackAddress: 0x14, Value: 0xAABBCCDD, Committed: true }]);

        GameSocketParserPostResizePointerPlan nullWithUnusedChild =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                resizeReturnedNormally: true,
                postResizeEsp: 0x5000,
                field38: 0,
                fieldPlus4: 0));
        check("A supplied child DWORD is unused on the null branch and dirties only clean completion",
            nullWithUnusedChild.ReachedBoundedEndpoint
            && !nullWithUnusedChild.IsCleanCompletion
            && nullWithUnusedChild.CaptureStates.FreshFieldPlus4Dword
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && nullWithUnusedChild.UnusedCaptures.Contains(
                GameSocketParserPostResizePointerCaptureKind.FreshFieldPlus4Dword)
            && nullWithUnusedChild.Capture.FreshFieldPlus4Dword == 0
            && nullWithUnusedChild.Reads.Length == 1
            && nullWithUnusedChild.Stores.Length == 2);

        GameSocketParserPostResizePointerPlan readAliasesStore =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                resizeReturnedNormally: true,
                postResizeEsp: 0x1000,
                field38: 0x1010,
                fieldPlus4: 0xDEADBEEF));
        check("An independent fresh child read is not forwarded from an aliased stack store",
            readAliasesStore.Reads[1].Address == 0x1014
            && readAliasesStore.Stores[0].StackAddress == 0x1014
            && readAliasesStore.Reads[1].Value == 0xDEADBEEF
            && readAliasesStore.Stores[0].Value == 0xDEADBEEF
            && readAliasesStore.Reads[1].Committed == false
            && readAliasesStore.Stores[0].Committed);

        GameSocketParserPostResizePointerPlan missingResizeStatus =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                resizeReturnedNormally: null,
                postResizeEsp: 0x6000,
                field38: 1,
                fieldPlus4: 2));
        GameSocketParserPostResizePointerPlan nonreturningResize =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                resizeReturnedNormally: false,
                postResizeEsp: 0x6000,
                field38: 1,
                fieldPlus4: 2));
        check("A missing or non-normal resize return stops at the call without consuming child captures",
            missingResizeStatus.Outcome
                == GameSocketParserPostResizePointerOutcome.MissingOpaqueResizeReturnStatus
            && nonreturningResize.Outcome
                == GameSocketParserPostResizePointerOutcome.OpaqueResizeDidNotReturnNormally
            && missingResizeStatus.StopBeforeRawFileOffset
                == GameSocketParserPostResizePointerProjection.ResizeCallRawFileOffset
            && nonreturningResize.StopBeforeRawFileOffset
                == GameSocketParserPostResizePointerProjection.ResizeCallRawFileOffset
            && missingResizeStatus.FlagsAtStop
                == ToFlags(missingResizeStatus.ParentPlan.FlagsAtStop)
            && nonreturningResize.FlagsAtStop
                == ToFlags(nonreturningResize.ParentPlan.FlagsAtStop)
            && missingResizeStatus.Reads.IsEmpty
            && nonreturningResize.Reads.IsEmpty
            && missingResizeStatus.ParentPlan.ReachedBoundedEndpoint
            && nonreturningResize.ParentPlan.ReachedBoundedEndpoint
            && missingResizeStatus.ParentResizeHandoffAdmitted
            && nonreturningResize.ParentResizeHandoffAdmitted
            && !missingResizeStatus.ReachedBoundedEndpoint
            && !nonreturningResize.ReachedBoundedEndpoint
            && !missingResizeStatus.ExactBoundedCompletion
            && !nonreturningResize.ExactBoundedCompletion
            && nonreturningResize.CaptureStates.FreshPostResizeESP
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && nonreturningResize.CaptureStates.FreshField38Dword
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && nonreturningResize.CaptureStates.FreshFieldPlus4Dword
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && !nonreturningResize.HeaderByteReadAttempted);

        GameSocketParserPostResizePointerPlan missingPostResizeEsp =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                resizeReturnedNormally: true,
                postResizeEsp: null,
                field38: 1,
                fieldPlus4: 2));
        GameSocketParserPostResizePointerPlan missingField38 =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                resizeReturnedNormally: true,
                postResizeEsp: 0x7000,
                field38: null,
                fieldPlus4: 2));
        GameSocketParserPostResizePointerPlan missingChild =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                resizeReturnedNormally: true,
                postResizeEsp: 0x7000,
                field38: 0x12345678,
                fieldPlus4: null));
        check("Missing post-call captures stop at their exact reads without synthesizing values",
            missingPostResizeEsp.Outcome
                == GameSocketParserPostResizePointerOutcome.MissingPostResizeESP
            && missingPostResizeEsp.StopBeforeRawFileOffset
                == GameSocketParserPostResizePointerProjection.FreshField38LoadRawFileOffset
            && missingPostResizeEsp.Reads.IsEmpty
            && missingPostResizeEsp.CaptureStates.FreshField38Dword
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && missingField38.Outcome
                == GameSocketParserPostResizePointerOutcome.MissingFreshField38Dword
            && missingField38.StopBeforeRawFileOffset
                == GameSocketParserPostResizePointerProjection.FreshField38LoadRawFileOffset
            && missingField38.Reads is [{
                Address: 0x7038,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }]
            && missingChild.Outcome
                == GameSocketParserPostResizePointerOutcome.MissingFreshFieldPlus4Dword
            && missingChild.StopBeforeRawFileOffset
                == GameSocketParserPostResizePointerProjection.FreshFieldPlus4LoadRawFileOffset
            && missingChild.Reads is [
                { Kind: GameSocketParserPostResizePointerReadKind.FreshField38Dword },
                {
                    Kind: GameSocketParserPostResizePointerReadKind.FreshFieldPlus4Dword,
                    Address: 0x1234567C,
                    Value: null,
                    Attempted: true,
                    Captured: false,
                    Committed: false
                }]
            && missingChild.FlagsAtStop is {
                Carry: false,
                Parity: true,
                AuxiliaryCarry: null,
                Zero: false,
                Sign: false,
                Overflow: false
            }
            && missingChild.Stores.IsEmpty);

        GameSocketParserPostResizePointerPlan missingParentValidation =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                parentValidated: false,
                resizeReturnedNormally: true,
                postResizeEsp: 0x8000,
                field38: 1,
                fieldPlus4: 2));
        GameSocketParserPostResizePointerPlan missingParentGetterEsp =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                parentGetterEsp: null,
                resizeReturnedNormally: true,
                postResizeEsp: 0x8000,
                field38: 1,
                fieldPlus4: 2));
        check("Child evidence cannot forge parent completion and remains explicitly unused",
            missingParentValidation.Outcome
                == GameSocketParserPostResizePointerOutcome.ParentDidNotReachResizeCallBoundary
            && !missingParentValidation.ParentResizeHandoffAdmitted
            && !missingParentValidation.ParentPlan.ReachedBoundedEndpoint
            && !missingParentValidation.ExactBoundedCompletion
            && missingParentValidation.ParentPlan.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.PostConstructorPathNotValidated
            && missingParentValidation.StopBeforeRawFileOffset
                == missingParentValidation.ParentPlan.StopBeforeRawFileOffset
            && missingParentValidation.CaptureStates.OpaqueResizeReturnedNormally
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && missingParentValidation.CaptureStates.FreshPostResizeESP
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && missingParentValidation.CaptureStates.FreshField38Dword
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && missingParentValidation.CaptureStates.FreshFieldPlus4Dword
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && missingParentValidation.UnusedCaptures.Length == 4
            && missingParentValidation.Reads.IsEmpty
            && missingParentGetterEsp.ParentPlan.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.MissingPostGetterESPBeforeFirstPush
            && missingParentGetterEsp.ParentPlan.StopBeforeRawFileOffset
                == GameSocketParserLocalSizeDispatchProjection.FirstPushRawFileOffset
            && missingParentGetterEsp.StopBeforeRawFileOffset
                == missingParentGetterEsp.ParentPlan.StopBeforeRawFileOffset
            && !missingParentGetterEsp.ParentResizeHandoffAdmitted
            && !missingParentGetterEsp.ParentPlan.ReachedBoundedEndpoint
            && !missingParentGetterEsp.ExactBoundedCompletion
            && missingParentGetterEsp.CaptureStates.OpaqueResizeReturnedNormally
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && missingParentGetterEsp.CaptureStates.FreshPostResizeESP
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && missingParentGetterEsp.CaptureStates.FreshField38Dword
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && missingParentGetterEsp.CaptureStates.FreshFieldPlus4Dword
                == GameSocketParserPostResizePointerCaptureState.SuppliedButUnused
            && missingParentGetterEsp.UnusedCaptures.Length == 4
            && missingParentGetterEsp.Reads.IsEmpty);

        GameSocketParserPostResizePointerPlan ordered =
            GameSocketParserPostResizePointerProjection.Project(Capture(
                resizeReturnedNormally: true,
                postResizeEsp: 0x9000,
                field38: 0x12340000,
                fieldPlus4: 0xCAFEBABE));
        int parentEffectCount = ordered.ParentPlan.MemoryEffects.Length;
        check("Parent reads and pushes precede fresh child reads and committed stack stores",
            ordered.OrderedMemoryEffects.Length == parentEffectCount + 4
            && ordered.OrderedMemoryEffects
                .Take(parentEffectCount)
                .Select(effect => effect.Kind)
                .SequenceEqual(ordered.ParentPlan.MemoryEffects.Select(effect => effect.Kind switch
                {
                    GameSocketParserLocalSizeDispatchMemoryEffectKind.FreshLocalVtableRead =>
                        GameSocketParserPostResizePointerMemoryEffectKind.ParentFreshLocalVtableRead,
                    GameSocketParserLocalSizeDispatchMemoryEffectKind.FreshSlotCIndirectTargetRead =>
                        GameSocketParserPostResizePointerMemoryEffectKind.ParentFreshSlotCIndirectTargetRead,
                    _ => GameSocketParserPostResizePointerMemoryEffectKind.ParentStackPush
                }))
            && ordered.OrderedMemoryEffects[parentEffectCount].Kind
                == GameSocketParserPostResizePointerMemoryEffectKind.FreshField38DwordRead
            && ordered.OrderedMemoryEffects[parentEffectCount + 1].Kind
                == GameSocketParserPostResizePointerMemoryEffectKind.FreshFieldPlus4DwordRead
            && ordered.OrderedMemoryEffects[parentEffectCount + 2].Kind
                == GameSocketParserPostResizePointerMemoryEffectKind.StackStore
            && ordered.OrderedMemoryEffects[parentEffectCount + 3].Kind
                == GameSocketParserPostResizePointerMemoryEffectKind.StackStore
            && ordered.OrderedMemoryEffects[parentEffectCount + 2].Committed
            && ordered.OrderedMemoryEffects[parentEffectCount + 3].Committed
            && !ordered.OrderedMemoryEffects[parentEffectCount].Committed
            && !ordered.OrderedMemoryEffects[parentEffectCount + 1].Committed
            && ordered.ParentResizeHandoffAdmitted
            && !ordered.HeaderByteReadAttempted
            && ordered.HeaderByteReadAddress == 0xCAFEBABE
            && ordered.StopBeforeRawFileOffset
                == GameSocketParserPostResizePointerProjection.HeaderByteLoadRawFileOffset);
    }

    private static GameSocketParserPostResizePointerCapture Capture(
        bool? parentValidated = true,
        uint? parentEntryEsp = 0x1000,
        uint? parentVtable = 0x5000,
        uint? parentSlotTarget = 0x6000,
        bool? parentGetterReturnedNormally = true,
        uint? parentGetterEax = 0x1234,
        uint? parentGetterEsp = 0x2000,
        bool? resizeReturnedNormally = true,
        uint? postResizeEsp = 0x3000,
        uint? field38 = 0,
        uint? fieldPlus4 = null) =>
        new(
            new GameSocketParserLocalSizeDispatchCapture(
                parentValidated,
                parentEntryEsp,
                parentVtable,
                parentSlotTarget,
                parentGetterReturnedNormally,
                parentGetterEax,
                parentGetterEsp,
                new GameSocketParserLocalSizeDispatchFlags(
                    Carry: true,
                    Parity: null,
                    AuxiliaryCarry: false,
                    Zero: null,
                    Sign: true,
                    Overflow: false)),
            resizeReturnedNormally,
            postResizeEsp,
            field38,
            fieldPlus4);

    private static GameSocketParserPostResizePointerFlags? ToFlags(
        GameSocketParserLocalSizeDispatchFlags? flags) =>
        flags is GameSocketParserLocalSizeDispatchFlags value
            ? new GameSocketParserPostResizePointerFlags(
                value.Carry,
                value.Parity,
                value.AuxiliaryCarry,
                value.Zero,
                value.Sign,
                value.Overflow)
            : null;

    private static byte[] ParseBytes(string text) =>
        text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => Convert.ToByte(value, 16))
            .ToArray();

    private static bool HasExactSlice(
        byte[] bytes,
        uint evidenceStartRawFileOffset,
        uint instructionRawFileOffset,
        string expectedHex)
    {
        byte[] expected = ParseBytes(expectedHex);
        int start = checked((int)(instructionRawFileOffset - evidenceStartRawFileOffset));
        return start >= 0
            && start + expected.Length <= bytes.Length
            && bytes.AsSpan(start, expected.Length).SequenceEqual(expected);
    }
}
