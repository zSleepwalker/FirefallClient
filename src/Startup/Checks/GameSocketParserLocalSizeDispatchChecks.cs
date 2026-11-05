using Firefall.Reconstruction.Startup;

internal static class GameSocketParserLocalSizeDispatchChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] capturedBytes = ParseBytes(
            GameSocketParserLocalSizeDispatchProjection.CapturedCallerBytes);
        byte[] constructorBytes = ParseBytes(
            GameSocketParserLocalSizeDispatchProjection.CapturedConstructorBytes);
        check("The authorized sample, file/VA mapping, exact caller bytes, and constructor ordering are pinned",
            GameSocketParserLocalSizeDispatchProjection.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && GameSocketParserLocalSizeDispatchProjection.SampleFileSize == 40197848
            && GameSocketParserLocalSizeDispatchProjection.RawToCodeVirtualAddressDelta == 0x00400C00
            && GameSocketParserLocalSizeDispatchProjection.RawFileOffsetToCodeVirtualAddress(
                GameSocketParserLocalSizeDispatchProjection.InitialLocalLeaRawFileOffset)
                == 0x00FD3DC5
            && GameSocketParserLocalSizeDispatchProjection.CodeVirtualAddressToRawFileOffset(
                0x00FD3DC5)
                == GameSocketParserLocalSizeDispatchProjection.InitialLocalLeaRawFileOffset
            && unchecked(UiShaderSourceWrapperNativeLayout.WrapperVtableRawFileOffset
                + GameSocketParserLocalSizeDispatchProjection.WrapperDataRawToVirtualAddressDelta)
                == UiShaderSourceWrapperNativeLayout.WrapperVtableVirtualAddress
            && GameSocketParserLocalSizeDispatchProjection.RawFileOffsetToCodeVirtualAddress(
                UiShaderSourceWrapperNativeLayout.WrapperVtableSlotCEntryRawFileOffset)
                == UiShaderSourceWrapperNativeLayout.WrapperVtableSlotCEntryVirtualAddress
            && capturedBytes.Length == 44
            && constructorBytes.Length == 53
            && HasExactSlice(
                capturedBytes,
                GameSocketParserLocalSizeDispatchProjection.CallerEvidenceStartRawFileOffset,
                GameSocketParserLocalSizeDispatchProjection.InitialLocalLeaRawFileOffset,
                "8D 4C 24 34")
            && HasExactSlice(
                capturedBytes,
                GameSocketParserLocalSizeDispatchProjection.CallerEvidenceStartRawFileOffset,
                GameSocketParserLocalSizeDispatchProjection.FreshLocalVtableLoadRawFileOffset,
                "8B 44 24 34")
            && HasExactSlice(
                capturedBytes,
                GameSocketParserLocalSizeDispatchProjection.CallerEvidenceStartRawFileOffset,
                GameSocketParserLocalSizeDispatchProjection.FreshSlotCIndirectCallRawFileOffset,
                "FF 50 0C")
            && HasExactSlice(
                capturedBytes,
                GameSocketParserLocalSizeDispatchProjection.CallerEvidenceStartRawFileOffset,
                GameSocketParserLocalSizeDispatchProjection.FirstPushRawFileOffset,
                "6A 01")
            && HasExactSlice(
                capturedBytes,
                GameSocketParserLocalSizeDispatchProjection.CallerEvidenceStartRawFileOffset,
                GameSocketParserLocalSizeDispatchProjection.SecondPushRawFileOffset,
                "50")
            && HasExactSlice(
                capturedBytes,
                GameSocketParserLocalSizeDispatchProjection.CallerEvidenceStartRawFileOffset,
                GameSocketParserLocalSizeDispatchProjection.ResizeLocalLeaRawFileOffset,
                "8D 4C 24 3C")
            && HasExactSlice(
                capturedBytes,
                GameSocketParserLocalSizeDispatchProjection.CallerEvidenceStartRawFileOffset,
                GameSocketParserLocalSizeDispatchProjection.ResizeCallRawFileOffset,
                "E8 D4 8A 6F 00")
            && HasExactSlice(
                constructorBytes,
                GameSocketParserLocalSizeDispatchProjection.ConstructorEvidenceStartRawFileOffset,
                GameSocketParserLocalSizeDispatchProjection.ConstructorVtableWriteRawFileOffset,
                "C7 06 C4 0C C8 01")
            && BitConverter.ToUInt32(constructorBytes, 14)
                == UiShaderSourceWrapperNativeLayout.WrapperVtableVirtualAddress
            && HasExactSlice(
                constructorBytes,
                GameSocketParserLocalSizeDispatchProjection.ConstructorEvidenceStartRawFileOffset,
                GameSocketParserLocalSizeDispatchProjection.ConstructorOpaqueResizeCallRawFileOffset,
                "E8 27 04 00 00")
            && RelativeCallTarget(
                capturedBytes,
                GameSocketParserLocalSizeDispatchProjection.CallerEvidenceStartRawFileOffset,
                0x00BD31B8)
                == GameSocketParserLocalSizeDispatchProjection.OpaqueConstructorEntryRawFileOffset
            && RelativeCallTarget(
                capturedBytes,
                GameSocketParserLocalSizeDispatchProjection.CallerEvidenceStartRawFileOffset,
                GameSocketParserLocalSizeDispatchProjection.ResizeCallRawFileOffset)
                == GameSocketParserLocalSizeDispatchProjection.OpaqueResizeHelperTargetRawFileOffset
            && RelativeCallTarget(
                constructorBytes,
                GameSocketParserLocalSizeDispatchProjection.ConstructorEvidenceStartRawFileOffset,
                GameSocketParserLocalSizeDispatchProjection.ConstructorOpaqueResizeCallRawFileOffset)
                == GameSocketParserLocalSizeDispatchProjection.OpaqueResizeHelperTargetRawFileOffset
            && GameSocketParserLocalSizeDispatchProjection.ConstructorVtableWriteRawFileOffset
                < GameSocketParserLocalSizeDispatchProjection.ConstructorOpaqueResizeCallRawFileOffset
            && GameSocketParserLocalSizeDispatchProjection.ConstructorOpaqueResizeCallRawFileOffset
                < GameSocketParserLocalSizeDispatchProjection.OpaqueResizeHelperTargetRawFileOffset
            && GameSocketParserLocalSizeDispatchProjection.InitialLocalDisplacementFromESP == 0x34
            && GameSocketParserLocalSizeDispatchProjection.SlotCDisplacement
                == UiShaderSourceWrapperNativeLayout.WrapperVtableSlotCOffset
            && GameSocketParserLocalSizeDispatchProjection.ResizePreserveArgument
                == UiShaderSourceWrapperNativeLayout.PreservingResizePreserveArgument
            && GameSocketParserLocalSizeDispatchProjection.OpaqueIndirectTargetExecutionModeled == false);

        GameSocketParserLocalSizeDispatchPlan missingValidation =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: null,
                entryEsp: 0x1000,
                vtable: 0x2000,
                slotTarget: 0x3000,
                getterReturnedNormally: true,
                getterEax: 1,
                postGetterEsp: 0x4000));
        GameSocketParserLocalSizeDispatchPlan rejectedValidation =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: false,
                entryEsp: 0,
                vtable: 0,
                slotTarget: 0,
                getterReturnedNormally: false,
                getterEax: 0,
                postGetterEsp: 0));
        check("Missing or false post-constructor validation stops before reads and preserves unused captures",
            missingValidation.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.MissingPostConstructorValidation
            && missingValidation.Reads.IsEmpty
            && missingValidation.Pushes.IsEmpty
            && missingValidation.GetterCall is null
            && missingValidation.CaptureStates.EntryESPAtInitialLocalLea
                == GameSocketParserLocalSizeDispatchCaptureState.SuppliedButUnused
            && missingValidation.GetterReturnedNormally is null
            && missingValidation.GetterReturnedEAX is null
            && missingValidation.GetterReturnInvalidatedFlags is null
            && missingValidation.FreshVtableMatchesInitializer is null
            && missingValidation.FreshSlotCTargetMatchesStaticEntry is null
            && missingValidation.UnusedCaptures.Length == 6
            && missingValidation.StopBeforeRawFileOffset
                == GameSocketParserLocalSizeDispatchProjection.InitialLocalLeaRawFileOffset
            && rejectedValidation.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.PostConstructorPathNotValidated
            && rejectedValidation.Reads.IsEmpty
            && rejectedValidation.Pushes.IsEmpty
            && rejectedValidation.UnusedCaptures.Length == 6);

        GameSocketParserLocalSizeDispatchPlan missingEntryEsp =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: true,
                entryEsp: null,
                vtable: 0,
                slotTarget: 0,
                getterReturnedNormally: true,
                getterEax: 0,
                postGetterEsp: 0));
        GameSocketParserLocalSizeDispatchPlan zeroEntryEsp =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: true,
                entryEsp: 0,
                vtable: null,
                slotTarget: null,
                getterReturnedNormally: null,
                getterEax: null,
                postGetterEsp: null));
        check("Entry ESP is required at the initial LEA; captured zero is distinct from missing and wraps normally",
            missingEntryEsp.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.MissingEntryESPAtInitialLocalLea
            && missingEntryEsp.Reads.IsEmpty
            && missingEntryEsp.InitialLocalAddress is null
            && missingEntryEsp.CaptureStates.EntryESPAtInitialLocalLea
                == GameSocketParserLocalSizeDispatchCaptureState.Missing
            && missingEntryEsp.UnusedCaptures.Contains(
                GameSocketParserLocalSizeDispatchCaptureKind.FreshLocalVtable)
            && zeroEntryEsp.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.MissingFreshLocalVtable
            && zeroEntryEsp.InitialLocalAddress == 0x34
            && zeroEntryEsp.Reads is [{
                Kind: GameSocketParserLocalSizeDispatchReadKind.FreshLocalVtable,
                Address: 0x34,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }]
            && zeroEntryEsp.CaptureStates.EntryESPAtInitialLocalLea
                == GameSocketParserLocalSizeDispatchCaptureState.Captured);

        GameSocketParserLocalSizeDispatchPlan missingVtable =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: true,
                entryEsp: 0xFFFFFFF0,
                vtable: null,
                slotTarget: 0x1234,
                getterReturnedNormally: true,
                getterEax: 0,
                postGetterEsp: 0));
        GameSocketParserLocalSizeDispatchPlan missingSlot =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: true,
                entryEsp: 0xFFFFFFF0,
                vtable: 0xFFFFFFF8,
                slotTarget: null,
                getterReturnedNormally: true,
                getterEax: 0,
                postGetterEsp: 0));
        check("Fresh local-vtable and slot-C reads stay ordered, uncommitted, and address-wrap-safe",
            missingVtable.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.MissingFreshLocalVtable
            && missingVtable.InitialLocalAddress == 0x24
            && missingVtable.Reads is [{
                Kind: GameSocketParserLocalSizeDispatchReadKind.FreshLocalVtable,
                Address: 0x24,
                Value: null,
                Attempted: true,
                Captured: false,
                Committed: false
            }]
            && missingVtable.CaptureStates.FreshSlotCIndirectTarget
                == GameSocketParserLocalSizeDispatchCaptureState.SuppliedButUnused
            && missingVtable.CaptureStates.OpaqueGetterReturnedNormally
                == GameSocketParserLocalSizeDispatchCaptureState.SuppliedButUnused
            && missingVtable.CaptureStates.OpaqueGetterReturnedEAX
                == GameSocketParserLocalSizeDispatchCaptureState.SuppliedButUnused
            && missingVtable.GetterReturnedNormally is null
            && missingVtable.GetterReturnedEAX is null
            && missingVtable.GetterReturnInvalidatedFlags is null
            && missingVtable.FreshVtableMatchesInitializer is null
            && missingVtable.FreshSlotCTargetMatchesStaticEntry is null
            && !missingVtable.UnusedCaptures.IsEmpty
            && missingVtable.StopBeforeRawFileOffset
                == GameSocketParserLocalSizeDispatchProjection.FreshLocalVtableLoadRawFileOffset
            && missingSlot.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.MissingFreshSlotCIndirectTarget
            && missingSlot.Reads is [
                {
                    Kind: GameSocketParserLocalSizeDispatchReadKind.FreshLocalVtable,
                    Address: 0x24,
                    Value: 0xFFFFFFF8,
                    Captured: true,
                    Committed: false
                },
                {
                    Kind: GameSocketParserLocalSizeDispatchReadKind.FreshSlotCIndirectTarget,
                    Address: 4,
                    Value: null,
                    Attempted: true,
                    Captured: false,
                    Committed: false
                }]
            && missingSlot.MemoryEffects is [
                {
                    Kind: GameSocketParserLocalSizeDispatchMemoryEffectKind.FreshLocalVtableRead,
                    InstructionRawFileOffset:
                        GameSocketParserLocalSizeDispatchProjection.FreshLocalVtableLoadRawFileOffset,
                    Address: 0x24,
                    Value: 0xFFFFFFF8,
                    Committed: false
                },
                {
                    Kind: GameSocketParserLocalSizeDispatchMemoryEffectKind.FreshSlotCIndirectTargetRead,
                    InstructionRawFileOffset:
                        GameSocketParserLocalSizeDispatchProjection.FreshSlotCIndirectCallRawFileOffset,
                    Address: 4,
                    Value: null,
                    Attempted: true,
                    Captured: false,
                    Committed: false
                }]
            && missingSlot.StopBeforeRawFileOffset
                == GameSocketParserLocalSizeDispatchProjection.FreshSlotCIndirectCallRawFileOffset);

        GameSocketParserLocalSizeDispatchPlan zeroIndirectTarget =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: true,
                entryEsp: 0x1000,
                vtable: 0x5000,
                slotTarget: 0,
                getterReturnedNormally: true,
                getterEax: 0,
                postGetterEsp: 0x2000));
        check("The post-constructor vtable is freshly captured and slot-C zero is accepted without target execution",
            zeroIndirectTarget.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.BoundedBeforeResizeCall
            && zeroIndirectTarget.FreshVtableMatchesInitializer == false
            && zeroIndirectTarget.FreshSlotCTargetMatchesStaticEntry == false
            && zeroIndirectTarget.GetterCall is {
                SlotAddress: 0x500C,
                Target: 0,
                CallSiteReached: true,
                TargetExecutionModeled: false
            }
            && zeroIndirectTarget.Reads.Length == 2
            && zeroIndirectTarget.Reads.All(read => !read.Committed)
            && zeroIndirectTarget.GetterReturnInvalidatedFlags == true
            && zeroIndirectTarget.FlagsAtStop == GameSocketParserLocalSizeDispatchFlags.Unknown);

        GameSocketParserLocalSizeDispatchFlags pendingGetterFlags = new(
            Carry: true,
            Parity: null,
            AuxiliaryCarry: false,
            Zero: null,
            Sign: false,
            Overflow: null);
        GameSocketParserLocalSizeDispatchPlan missingReturnStatus =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: true,
                entryEsp: 0x1000,
                vtable: 0x5000,
                slotTarget: 0,
                getterReturnedNormally: null,
                getterEax: 0,
                postGetterEsp: 0x2000,
                flags: pendingGetterFlags));
        GameSocketParserLocalSizeDispatchPlan nonreturningGetter =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: true,
                entryEsp: 0x1000,
                vtable: 0x5000,
                slotTarget: 0x6000,
                getterReturnedNormally: false,
                getterEax: 0,
                postGetterEsp: 0x2000,
                flags: pendingGetterFlags));
        check("Pending or nonreturning opaque calls preserve inherited flags at the call boundary",
            missingReturnStatus.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.MissingOpaqueGetterReturnStatus
            && missingReturnStatus.GetterCallSiteReached
            && missingReturnStatus.FlagsBeforeGetterCall == pendingGetterFlags
            && missingReturnStatus.GetterReturnedNormally is null
            && missingReturnStatus.GetterReturnedEAX is null
            && missingReturnStatus.GetterReturnInvalidatedFlags is null
            && missingReturnStatus.FlagsAtStop == pendingGetterFlags
            && missingReturnStatus.StopBeforeRawFileOffset
                == GameSocketParserLocalSizeDispatchProjection.FreshSlotCIndirectCallRawFileOffset
            && missingReturnStatus.CaptureStates.OpaqueGetterReturnedEAX
                == GameSocketParserLocalSizeDispatchCaptureState.SuppliedButUnused
            && nonreturningGetter.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.OpaqueGetterDidNotReturnNormally
            && nonreturningGetter.Reads.Length == 2
            && nonreturningGetter.Pushes.IsEmpty
            && nonreturningGetter.GetterReturnedNormally == false
            && nonreturningGetter.GetterReturnedEAX is null
            && nonreturningGetter.GetterReturnInvalidatedFlags == false
            && nonreturningGetter.FlagsAtStop == pendingGetterFlags
            && nonreturningGetter.StopBeforeRawFileOffset
                == GameSocketParserLocalSizeDispatchProjection.FreshSlotCIndirectCallRawFileOffset);

        GameSocketParserLocalSizeDispatchFlags inheritedFlags = new(
            Carry: true,
            Parity: false,
            AuxiliaryCarry: true,
            Zero: false,
            Sign: true,
            Overflow: false);
        GameSocketParserLocalSizeDispatchPlan missingGetterEsp =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: true,
                entryEsp: 0x1000,
                vtable: UiShaderSourceWrapperNativeLayout.WrapperVtableVirtualAddress,
                slotTarget: UiShaderSourceWrapperNativeLayout.WrapperVtableSlotCEntryVirtualAddress,
                getterReturnedNormally: true,
                getterEax: 0,
                postGetterEsp: null,
                flags: inheritedFlags));
        GameSocketParserLocalSizeDispatchPlan missingGetterEax =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: true,
                entryEsp: 0x1000,
                vtable: UiShaderSourceWrapperNativeLayout.WrapperVtableVirtualAddress,
                slotTarget: UiShaderSourceWrapperNativeLayout.WrapperVtableSlotCEntryVirtualAddress,
                getterReturnedNormally: true,
                getterEax: null,
                postGetterEsp: 0x2000,
                flags: inheritedFlags));
        check("Inherited flags survive the pre-call LEA/MOVs and are unknown after normal opaque return",
            missingGetterEsp.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.MissingPostGetterESPBeforeFirstPush
            && missingGetterEsp.Pushes.IsEmpty
            && missingGetterEsp.FlagsBeforeGetterCall == inheritedFlags
            && missingGetterEsp.GetterReturnedEAX == 0
            && missingGetterEsp.CaptureStates.OpaqueGetterReturnedEAX
                == GameSocketParserLocalSizeDispatchCaptureState.Captured
            && missingGetterEsp.FlagsAtStop == GameSocketParserLocalSizeDispatchFlags.Unknown
            && missingGetterEax.Outcome
                == GameSocketParserLocalSizeDispatchOutcome.MissingGetterEAXBeforeSecondPush
            && missingGetterEax.Pushes is [{
                InstructionRawFileOffset:
                    GameSocketParserLocalSizeDispatchProjection.FirstPushRawFileOffset,
                StackAddress: 0x1FFC,
                Value: 1,
                Attempted: true,
                Committed: true
            }]
            && missingGetterEax.FlagsAtStop == GameSocketParserLocalSizeDispatchFlags.Unknown
            && missingGetterEax.StopBeforeRawFileOffset
                == GameSocketParserLocalSizeDispatchProjection.SecondPushRawFileOffset);

        GameSocketParserLocalSizeDispatchPlan complete =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: true,
                entryEsp: 0x1000,
                vtable: 0x5000,
                slotTarget: 0,
                getterReturnedNormally: true,
                getterEax: 0,
                postGetterEsp: 0x2000,
                flags: inheritedFlags));
        GameSocketParserLocalSizeDispatchPlan wrapped =
            GameSocketParserLocalSizeDispatchProjection.Project(Capture(
                validated: true,
                entryEsp: uint.MaxValue - 0x20,
                vtable: 0x1234,
                slotTarget: 0x5678,
                getterReturnedNormally: true,
                getterEax: 0xAABBCCDD,
                postGetterEsp: 4));
        check("Resize arguments preserve push order and independently derive ECX from post-getter ESP",
            complete.Outcome == GameSocketParserLocalSizeDispatchOutcome.BoundedBeforeResizeCall
            && complete.IsCleanCompletion
            && complete.ReachedResizeCallBoundary
            && !complete.ResizeCallInstructionReached
            && !complete.ResizeCallEntered
            && complete.Pushes is [
                {
                    InstructionRawFileOffset:
                        GameSocketParserLocalSizeDispatchProjection.FirstPushRawFileOffset,
                    StackAddress: 0x1FFC,
                    Value: 1,
                    Committed: true
                },
                {
                    InstructionRawFileOffset:
                        GameSocketParserLocalSizeDispatchProjection.SecondPushRawFileOffset,
                    StackAddress: 0x1FF8,
                    Value: 0,
                    Committed: true
                }]
            && complete.OrderedMemoryEffects is [
                {
                    Kind: GameSocketParserLocalSizeDispatchMemoryEffectKind.FreshLocalVtableRead,
                    Committed: false
                },
                {
                    Kind: GameSocketParserLocalSizeDispatchMemoryEffectKind.FreshSlotCIndirectTargetRead,
                    Committed: false
                },
                {
                    Kind: GameSocketParserLocalSizeDispatchMemoryEffectKind.StackPush,
                    Value: 1,
                    Captured: true,
                    Committed: true
                },
                {
                    Kind: GameSocketParserLocalSizeDispatchMemoryEffectKind.StackPush,
                    Value: 0,
                    Captured: true,
                    Committed: true
                }]
            && complete.ResizeLocalAddress == 0x2034
            && complete.InitialLocalAddress == 0x1034
            && complete.InitialAndResizeLocalAddressesMatch == false
            && complete.FlagsAtStop == GameSocketParserLocalSizeDispatchFlags.Unknown
            && wrapped.InitialLocalAddress == 0x13
            && wrapped.Pushes[0].StackAddress == 0
            && wrapped.Pushes[1].StackAddress == uint.MaxValue - 3
            && wrapped.ResizeLocalAddress == 0x38
            && GameSocketParserLocalSizeDispatchProjection.ResizeLocalDisplacementFromPostGetterESP
                == GameSocketParserLocalSizeDispatchProjection.InitialLocalDisplacementFromESP
            && GameSocketParserLocalSizeDispatchProjection.ResizeLocalDisplacementFromPostGetterESP
                == GameSocketParserLocalSizeDispatchProjection.ResizeLocalDisplacementFromPostPushESP
                    - GameSocketParserLocalSizeDispatchProjection.ResizeStackArgumentBytes);
    }

    private static GameSocketParserLocalSizeDispatchCapture Capture(
        bool? validated,
        uint? entryEsp,
        uint? vtable,
        uint? slotTarget,
        bool? getterReturnedNormally,
        uint? getterEax,
        uint? postGetterEsp,
        GameSocketParserLocalSizeDispatchFlags? flags = null) =>
        new(
            validated,
            entryEsp,
            vtable,
            slotTarget,
            getterReturnedNormally,
            getterEax,
            postGetterEsp,
            flags);

    private static byte[] ParseBytes(string bytes) =>
        bytes.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(token => Convert.ToByte(token, 16))
            .ToArray();

    private static bool HasExactSlice(
        byte[] capturedBytes,
        uint captureStartRawFileOffset,
        uint instructionRawFileOffset,
        string expectedBytes)
    {
        if (instructionRawFileOffset < captureStartRawFileOffset)
            return false;

        int start = checked((int)(instructionRawFileOffset - captureStartRawFileOffset));
        byte[] expected = ParseBytes(expectedBytes);
        return start + expected.Length <= capturedBytes.Length
            && capturedBytes.AsSpan(start, expected.Length).SequenceEqual(expected);
    }

    private static uint RelativeCallTarget(
        byte[] capturedBytes,
        uint captureStartRawFileOffset,
        uint callRawFileOffset)
    {
        int start = checked((int)(callRawFileOffset - captureStartRawFileOffset));
        int displacement = BitConverter.ToInt32(capturedBytes, start + 1);
        return unchecked(callRawFileOffset + 5u + (uint)displacement);
    }
}
