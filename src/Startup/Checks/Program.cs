using Firefall.Reconstruction.Startup;

int failures = 0;
PayloadWordGrowthGateChecks.Run(Check);
PayloadWordSlotIndexChecks.Run(Check);
PayloadWordSlotAllocationChecks.Run(Check);
PayloadWordCopyTailChecks.Run(Check);
PayloadWordGrowthDoublingChecks.Run(Check);
PayloadGrowthBufferAllocationChecks.Run(Check);
PayloadWordAppendChecks.Run(Check);
PayloadGrowthFirstCopyChecks.Run(Check);
PayloadGrowthPostCopyGateChecks.Run(Check);
PayloadGrowthBranchCopyChecks.Run(Check);
PayloadGrowthTakenSecondCopyChecks.Run(Check);
PayloadGrowthPublicationChecks.Run(Check);
ReliableReceiveSlotGateChecks.Run(Check);
ReliableReceiveForwardingCallerChecks.Run(Check);
ReliableReceiveRecordWordChecks.Run(Check);
ReliableReceiveRecycleReturnChecks.Run(Check);
ReliableReceiveEmptySlotCallbackChecks.Run(Check);
ReliableReceiveForwardingReturnChecks.Run(Check);
ReliableReceiveSlotPublicationChecks.Run(Check);
ReliableReceiveCallbackResultChecks.Run(Check);
ReliableReceiveLengthCallbackChecks.Run(Check);
ReliableReceiveCallbackScalarHandoffChecks.Run(Check);
UnreliableReceiveResidueCounterChecks.Run(Check);
UnreliableReceiveResidueHandoffChecks.Run(Check);
UnreliableReceiveJoinedHeaderChecks.Run(Check);
ReliableReceiveOccupiedSlotChecks.Run(Check);
UnreliableReceiveMarkerCallbackChecks.Run(Check);
UnreliableReceiveCallbackScalarChecks.Run(Check);
UnreliableReceiveResidueJoinChecks.Run(Check);
ReliableReceiveEmptyPublicationChecks.Run(Check);
UnreliableReceiveCallbackPresenceChecks.Run(Check);
UnreliableReceiveDirectCallbackChecks.Run(Check);
UnreliableReceiveMarkerScalarChecks.Run(Check);
UnreliableReceivePresenceCallbackChecks.Run(Check);
ReliableReceiveResultLengthChecks.Run(Check);
UnreliableReceiveCallbackRecycleChecks.Run(Check);
UnreliableReceiveZeroResultChecks.Run(Check);
UnreliableReceiveZeroReturnChecks.Run(Check);
PayloadXorDiagnosticEntryChecks.Run(Check);
UnreliableReceiveResultStoresChecks.Run(Check);
PayloadXorTableSetupChecks.Run(Check);
UnreliableReceiveSelectorCallbackChecks.Run(Check);
UnreliableReceiveFrameHandoffChecks.Run(Check);
ReliableReceiveLengthPublicationChecks.Run(Check);
UnreliableReceiveCallbackRecycleJoinChecks.Run(Check);
PayloadXorReturnChecks.Run(Check);
UnreliableReceiveZeroForwardReturnChecks.Run(Check);
UnreliableReceiveHighCountForwardChecks.Run(Check);
UnreliableReceiveHighCountCallbackChecks.Run(Check);
UnreliableReceiveHighCountCounterChecks.Run(Check);
PayloadXorEntrySetupChecks.Run(Check);
UnreliableReceiveBelowThresholdReturnChecks.Run(Check);
UnreliableReceiveHighCountSuffixChecks.Run(Check);
UnreliableReceiveHighCountResultChecks.Run(Check);
UnreliableReceiveHighCountReturnChecks.Run(Check);
HighCountObjectHelperEntryChecks.Run(Check);
FrameHandoffHelperEntryChecks.Run(Check);
UnreliableReceiveHighCountLifecycleChecks.Run(Check);
FrameHandoffHelperSlotChecks.Run(Check);
HighCountObjectHelperReturnChecks.Run(Check);
FrameHandoffHelperEntrySlotChecks.Run(Check);
FrameHandoffCapacitySeedChecks.Run(Check);
HighCountObjectHelperLifecycleChecks.Run(Check);
FrameHandoffEmptySlotCallChecks.Run(Check);
FrameHandoffSlotPublicationChecks.Run(Check);
FrameHandoffFinalWriteReturnChecks.Run(Check);
FrameHandoffCapacityLoopChecks.Run(Check);
FrameHandoffCapacitySeedLoopChecks.Run(Check);
FrameHandoffPublicationReturnChecks.Run(Check);
FrameHandoffSlotCallPublicationChecks.Run(Check);
FrameHandoffCapacityPrefixChecks.Run(Check);
FrameHandoffCapacityCallChecks.Run(Check);
FrameHandoffCapacityPrefixCallChecks.Run(Check);
FrameHandoffCapacityTransferChecks.Run(Check);
FrameHandoffCapacityTransferReturnChecks.Run(Check);
FrameHandoffCapacityTransferLifecycleChecks.Run(Check);
FrameHandoffCapacitySecondTransferChecks.Run(Check);
FrameHandoffCapacitySecondReturnGateChecks.Run(Check);
FrameHandoffCapacitySecondTransferLifecycleChecks.Run(Check);
FrameHandoffCapacityPrefixTransferChecks.Run(Check);
FrameHandoffCapacityAboveTransferChecks.Run(Check);
FrameHandoffCapacityRepeatStoreChecks.Run(Check);
FrameHandoffCapacityFillJoinChecks.Run(Check);
FrameHandoffCapacityLoopPrefixChecks.Run(Check);
FrameHandoffCapacityAboveSecondTransferChecks.Run(Check);
FrameHandoffCapacityAboveReturnGateChecks.Run(Check);
FrameHandoffCapacityGateRepeatChecks.Run(Check);
FrameHandoffCapacityFinalReturnChecks.Run(Check);
FrameHandoffCapacityFinalLifecycleChecks.Run(Check);
FrameHandoffCapacityAboveLifecycleChecks.Run(Check);
FrameHandoffCapacityFillSharedRepeatChecks.Run(Check);
FrameHandoffCapacityRepeatFillJoinChecks.Run(Check);
FrameHandoffCapacityAboveRepeatChecks.Run(Check);
FrameHandoffCapacityFillFinalLifecycleChecks.Run(Check);
CapacityFinalCalleeExitChecks.Run(Check);
FrameHandoffCapacityFirstFillLifecycleChecks.Run(Check);
FrameHandoffCapacityAboveFinalLifecycleChecks.Run(Check);
FrameHandoffCapacityFirstFinalLifecycleChecks.Run(Check);
CapacityFinalCalleeEntryChecks.Run(Check);
FrameHandoffCapacityOverflowTailChecks.Run(Check);
FrameHandoffCapacityLoopOverflowChecks.Run(Check);
CapacityFinalCalleePrefixCallChecks.Run(Check);
CapacityFinalCalleeFirstValidationChecks.Run(Check);
CapacityFinalCalleePrefixLifecycleChecks.Run(Check);
CapacityFinalCalleeValidationLifecycleChecks.Run(Check);
CapacityFinalCalleeEntryValidationChecks.Run(Check);
CapacityFinalCalleeAlignmentDispatchChecks.Run(Check);
CapacityFinalCalleeTagValidationChecks.Run(Check);
FrameHandoffCapacityDispatchLifecycleChecks.Run(Check);
CapacityFinalCalleeZeroExitLifecycleChecks.Run(Check);
CapacityFinalCalleeAlignmentTagLifecycleChecks.Run(Check);
CapacityFinalCalleeTagEqualityContinuationChecks.Run(Check);
CapacityFinalCalleeNonzeroValidationChecks.Run(Check);
CapacityFinalCalleeEntryAlignmentLifecycleChecks.Run(Check);
CapacityFinalCalleeFrameComparisonCallChecks.Run(Check);
CapacityFinalCalleeTagEqualityLifecycleChecks.Run(Check);
CapacityFinalCalleeSizeComparisonCallChecks.Run(Check);
CapacityFinalCalleeAlignmentNonzeroLifecycleChecks.Run(Check);
CapacityFinalCalleeTagEqualityExitLifecycleChecks.Run(Check);
CapacityFinalCalleeDescriptorReloadChecks.Run(Check);
CapacityFinalCalleeFrameMaskUpdateChecks.Run(Check);
CapacityFinalCalleeFrameSizeLifecycleChecks.Run(Check);
CapacityFinalCalleeDescriptorComparisonCallChecks.Run(Check);
CapacityFinalCalleePairedFieldWritesChecks.Run(Check);
CapacityFinalCalleeFrameMaskTagLifecycleChecks.Run(Check);
CapacityFinalCalleeSizeDescriptorLifecycleChecks.Run(Check);
CapacityFinalCalleePairedTagLifecycleChecks.Run(Check);
CapacityFinalCalleeLargeSizeFieldsChecks.Run(Check);
CapacityFinalCalleeDescriptorMaskLifecycleChecks.Run(Check);
CapacityFinalCalleeDescriptorPairedLifecycleChecks.Run(Check);
CapacityFinalCalleeAlternateFieldSelectionChecks.Run(Check);
CapacityFinalCalleeFieldDescentStepChecks.Run(Check);
CapacityFinalCalleeFieldClearTailChecks.Run(Check);
CapacityFinalCalleeDescriptorFallbackMaskLifecycleChecks.Run(Check);
CapacityFinalCalleeConditionalFieldStoreChecks.Run(Check);
CapacityFinalCalleeLargeSizeAlternateLifecycleChecks.Run(Check);
CapacityFinalCalleeDescentClearLifecycleChecks.Run(Check);
CapacityFinalCalleeIndexedRootMaskChecks.Run(Check);
CapacityFinalCalleeSelectionDescentLifecycleChecks.Run(Check);
CapacityFinalCalleeGuardedFieldStoreChecks.Run(Check);
CapacityFinalCalleeSuccessorPairedStoresChecks.Run(Check);
CapacityFinalCalleeIndexedConditionalLifecycleChecks.Run(Check);
CapacityFinalCalleeSecondFieldTestChecks.Run(Check);
CapacityFinalCalleeClearIndexedLifecycleChecks.Run(Check);
CapacityFinalCalleeSelectionClearLifecycleChecks.Run(Check);
CapacityFinalCalleeFinalPairedTailChecks.Run(Check);
CapacityFinalCalleeConditionalGuardedLifecycleChecks.Run(Check);
CapacityFinalCalleeSecondTestTailLifecycleChecks.Run(Check);
CapacityFinalCalleeAlternateTagGateChecks.Run(Check);
CapacityFinalCalleeGuardedSuccessorLifecycleChecks.Run(Check);
CapacityFinalCalleeAlternateFieldWritesChecks.Run(Check);
CapacityFinalCalleeIndexedGuardedLifecycleChecks.Run(Check);
CapacityFinalCalleePairedFinalLifecycleChecks.Run(Check);
CapacityFinalCalleeAlternateRootWritesChecks.Run(Check);
CapacityFinalCalleeGatedEpilogueChecks.Run(Check);
CapacityFinalCalleeAlternateTagWritesLifecycleChecks.Run(Check);
CapacityFinalCalleeGuardedFinalLifecycleChecks.Run(Check);
CapacityFinalCalleeAlternateRootGateChecks.Run(Check);
CapacityFinalCalleeAlternateEpilogueLifecycleChecks.Run(Check);
CapacityFinalCalleeAlternateAlignedSizeChecks.Run(Check);
CapacityFinalCalleeAlternateRootLifecycleChecks.Run(Check);
CapacityFinalCalleeAlternateFrameLoadsChecks.Run(Check);
CapacityFinalCalleeAlternateRootSizeLifecycleChecks.Run(Check);
CapacityFinalCalleeRootEpilogueLifecycleChecks.Run(Check);
CapacityFinalCalleeAlternateSecondFrameGateChecks.Run(Check);
CapacityFinalCalleeAlternateShiftedFrameGateChecks.Run(Check);
CapacityFinalCalleeAlignedFrameLifecycleChecks.Run(Check);
CapacityFinalCalleePairedFrameGateLifecycleChecks.Run(Check);
CapacityFinalCalleeSecondShiftedLifecycleChecks.Run(Check);
CapacityFinalCalleeAlternateBacklinkGateChecks.Run(Check);
CapacityFinalCalleeAlternateFinalFrameGateChecks.Run(Check);
FrameHandoffCapacityAboveBranchLifecycleChecks.Run(Check);
FrameHandoffCapacitySecondBranchLifecycleChecks.Run(Check);
FrameHandoffCapacityFinalCallGateChecks.Run(Check);
FrameHandoffCapacitySharedRepeatChecks.Run(Check);
FrameHandoffHelperLifecycleChecks.Run(Check);
FrameHandoffContinuationLifecycleChecks.Run(Check);
PayloadGrowthZeroFillChecks.Run(Check);

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (!condition)
        failures++;
}

UiNestedChildCleanupChecks.Run(Check);
UiChildBackingAllocationChecks.Run(Check);
UiChildInitializationResultChecks.Run(Check);
UiChildRequiredLookupChecks.Run(Check);
UiChildPairReplacementChecks.Run(Check);
UiChildScratchCleanupChecks.Run(Check);
UiChildShaderCompileChecks.Run(Check);
UiChildShaderBlobCleanupChecks.Run(Check);
UiShaderSourceWrapperChecks.Run(Check);
UiShaderSourceCopyChecks.Run(Check);
UiShaderSourceInitialCopyChecks.Run(Check);
UiShaderSourcePairReplacementChecks.Run(Check);
UiShaderSourceClearChecks.Run(Check);
UiShaderSourceDestructorChecks.Run(Check);
UiShaderSourceControlReleaseChecks.Run(Check);
UiShaderSourcePoolReturnChecks.Run(Check);
UiShaderSourceNonPoolReleaseChecks.Run(Check);
UiShaderSourceAllocationChecks.Run(Check);
UiShaderSourcePoolCandidateChecks.Run(Check);
UiShaderSourcePoolInitializationChecks.Run(Check);
UiShaderSourcePoolConfigurationChecks.Run(Check);
UiShaderSourcePoolResetEntryChecks.Run(Check);
UiShaderSourcePoolResetCallbackChecks.Run(Check);
UiShaderSourcePoolResetFinalizationChecks.Run(Check);
UiShaderSourcePoolResetAlignmentChecks.Run(Check);
UiShaderSourcePoolResetAppendChecks.Run(Check);
UiShaderSourcePoolResetGrowthChecks.Run(Check);
UiShaderSourcePoolResetRemovalChecks.Run(Check);
UiShaderSourcePoolResetStorageChecks.Run(Check);
UiShaderSourcePoolResetSecondaryGateChecks.Run(Check);
UiShaderSourcePoolResetPrimaryGateChecks.Run(Check);
PoolResetVectorCleanupChecks.Run(Check);
StackAllocationResultChecks.Run(Check);
UiShaderSourcePoolResetIterationTailChecks.Run(Check);
UiShaderSourcePoolResetSecondaryRepeatChecks.Run(Check);
QueuePopCompletionChecks.Run(Check);
QueuePopAttemptChecks.Run(Check);
QueueCleanupEmptyAttemptChecks.Run(Check);
QueueAppendAttemptChecks.Run(Check);
QueueAppendPublicationChecks.Run(Check);
QueueSentinelReturnChecks.Run(Check);
QueueEmbeddedPoolDrainChecks.Run(Check);
QueueSentinelInitializationChecks.Run(Check);
QueueAppendAcquisitionChecks.Run(Check);
QueueDestructorFinalizationChecks.Run(Check);
QueueGlobalPoolInitializationChecks.Run(Check);
SocketReceivedQueuePopChecks.Run(Check);
SocketFragmentEnqueueChecks.Run(Check);
ContextQueueAppendChecks.Run(Check);
OwnerCallbackQueueDeliveryChecks.Run(Check);
SocketSmallStatusChecks.Run(Check);
SocketLargeQueueGateChecks.Run(Check);
DeferredControlCallbackChecks.Run(Check);
SocketPayloadHeaderDispatchChecks.Run(Check);
SocketStatePredicateChecks.Run(Check);
SocketStateEntryChecks.Run(Check);
SocketPayloadRecycleTailChecks.Run(Check);
SocketPayloadRecyclePrefixChecks.Run(Check);
SocketHeaderFieldGetterChecks.Run(Check);
SocketHeaderCaseContinuationChecks.Run(Check);
ReliableReceiveEntryChecks.Run(Check);
UnreliableReceiveEntryChecks.Run(Check);
ReliableReceiveDeltaGateChecks.Run(Check);
UnreliableReceiveDeltaGateChecks.Run(Check);
ReceiveGuardForwardingChecks.Run(Check);
ReliableReceiveAlternateHandoffChecks.Run(Check);
SharedTextPoolSingletonChecks.Run(Check);
UiShaderSourcePoolTeardownChecks.Run(Check);
UiShaderSourcePoolSingletonDestructorChecks.Run(Check);
UiShaderSourcePoolDrainChecks.Run(Check);
UiShaderSourcePoolPopChecks.Run(Check);
UiShaderSourcePoolPushChecks.Run(Check);
UiShaderSourcePoolAcquisitionChecks.Run(Check);
UiShaderSourcePoolReplenishmentChecks.Run(Check);
UiShaderSourcePoolReplenishmentResultChecks.Run(Check);
UiShaderSourcePoolReplenishmentAlignmentChecks.Run(Check);
UiShaderSourcePoolReplenishmentIterationChecks.Run(Check);
SingletonTeardownWalkerChecks.Run(Check);
UiShaderSourceBackingAlignmentChecks.Run(Check);
UiShaderSourcePoolControlChecks.Run(Check);
UiShaderSourcePreserveCopyChecks.Run(Check);
UiShaderSourceReuseChecks.Run(Check);
UiShaderSourceReadOnlyOffsetChecks.Run(Check);
UiShaderSourceReadChecks.Run(Check);

var expected = new[]
{
    new AppModeSnapshot(ClientAppModeId.Login, "tfLogin", 0xA58, 0x6365B0, 0x1C676C0, 0x1E128C4, 0x1A10CC4),
    new AppModeSnapshot(ClientAppModeId.World, "tfWorld", 0x2904, 0x64AE40, 0x1C677BC, 0x1E12A9C, 0x1A10E9C),
    new AppModeSnapshot(ClientAppModeId.LookingGlass, "tfLookingGlass", 0x1368, 0x65A190, 0x1C6781C, 0x1E12C48, 0x1A11048)
};

Check("Structured chat selects exactly 19 cases and leaves all other byte selectors unselected",
    Enumerable.Range(0, 256).All(value =>
        StructuredChatMessageNativeLayout.Find((byte)value) is { } entry
            ? value is >= 1 and <= 19 && entry.Selector == value
            : value is not (>= 1 and <= 19))
    && StructuredChatMessageNativeLayout.Cases.Count == 19);
Check("Structured chat selectors7/8 share a body; later cases extend beyond the old IDA function boundary",
    StructuredChatMessageNativeLayout.Find(7)!.Value.CaseFileOffset == 0x6E3DAB
    && StructuredChatMessageNativeLayout.Find(8)!.Value.CaseFileOffset == 0x6E3DAB
    && StructuredChatMessageNativeLayout.Find(5)!.Value.CaseFileOffset == 0x6E3B12
    && StructuredChatMessageNativeLayout.Find(13)!.Value.CaseFileOffset == 0x6E44A2
    && StructuredChatMessageNativeLayout.Find(19)!.Value.PayloadReaderFileOffset == 0x6FE240);
Check("Structured chat payload ledger separates one-byte input from zero-extended native DWORD storage",
    StructuredChatMessageNativeLayout.PayloadFields.Count == 15
    && StructuredChatMessageNativeLayout.PayloadFields
        .Where(field => field.Encoding == StructuredChatFieldEncoding.ByteZeroExtendedToDword)
        .Select(field => (field.ReaderFileOffset, field.NativeOffset,
            field.NativeByteCount, field.FixedWireByteCount))
        .SequenceEqual([(0x6FDC30u, 4, 4, (int?)1), (0x6FDCC0u, 8, 4, (int?)1)]));
Check("Structured chat simple payload minimum byte counts include NUL terminators, not native padding",
    new (uint Reader, int Minimum)[]
    {
        (0x6FDBB0, 2), (0x6FDC30, 10), (0x6FDCC0, 6), (0x6FDD50, 1),
        (0x6FDF10, 10), (0x6FE120, 4), (0x6FE240, 1)
    }.All(expectedPayload => StructuredChatMessageNativeLayout.PayloadFields
        .Where(field => field.ReaderFileOffset == expectedPayload.Reader)
        .Sum(field => field.FixedWireByteCount ?? 1) == expectedPayload.Minimum));
Check("Structured chat pending flag treats every nonzero byte as incoming and omits status on zero",
    Enumerable.Range(0, 256).All(value =>
        StructuredChatMessageNativeLayout.CapturePendingNotification((byte)value)
            == (value == 0
                ? new StructuredChatPendingNotificationSnapshot(346, 0x28, null)
                : new StructuredChatPendingNotificationSnapshot(345, 0x34, "PENDING_IN"))));
Check("Structured chat optional readers consume1 byte when absent and15/29 for every nonzero presence byte",
    Enumerable.Range(0, 256).All(value =>
        StructuredChatMessageNativeLayout.Case5OptionalConsumedByteCount((byte)value)
            == (value == 0 ? 1 : 15)
        && StructuredChatMessageNativeLayout.Case10OptionalConsumedByteCount((byte)value)
            == (value == 0 ? 1 : 29)));
Check("Structured chat optional values distinguish wire widths from native padding and copy extent",
    StructuredChatMessageNativeLayout.OptionalValueFields.Count == 10
    && StructuredChatMessageNativeLayout.OptionalValueFields
        .Where(field => field.ReaderFileOffset == 0x709650)
        .Sum(field => field.FixedWireByteCount) == 14
    && StructuredChatMessageNativeLayout.OptionalValueFields
        .Where(field => field.ReaderFileOffset == 0x703910)
        .Sum(field => field.FixedWireByteCount) == 28
    && StructuredChatMessageNativeLayout.OptionalValueFields
        .Where(field => field.ReaderFileOffset == 0x703910)
        .Select(field => field.NativeOffset).SequenceEqual([0, 8, 16, 20, 24, 28])
    && StructuredChatMessageNativeLayout.Case5OptionalCopyByteCount == 16
    && StructuredChatMessageNativeLayout.Case10OptionalValueNativeExtent == 32
    && StructuredChatMessageNativeLayout.OptionalValueNativeOffset == 8);
Check("Structured chat case5 fixed fields plus absent optional consume37 bytes with empty text",
    StructuredChatMessageNativeLayout.Case5FixedFields.Count == 13
    && StructuredChatMessageNativeLayout.Case5FixedFields.Sum(field => field.FixedWireByteCount ?? 1)
        + StructuredChatMessageNativeLayout.Case5OptionalConsumedByteCount(0) == 37
    && StructuredChatMessageNativeLayout.Case5MinimumPayloadByteCount == 37
    && StructuredChatMessageNativeLayout.Case5FixedFields.Select(field => field.NativeOffset)
        .SequenceEqual([0, 4, 8, 9, 12, 16, 24, 28, 32, 64, 65, 68, 72]));
Check("Structured chat collection entries distinguish ten wire bytes from twelve native bytes",
    StructuredChatCollectionNativeLayout.ContinuationCount == 255
    && StructuredChatCollectionNativeLayout.Collections.SequenceEqual(
    [
        new StructuredChatCollectionSnapshot(9, 0x7093F0, 0x0C, 8, 8, 0x70950A),
        new StructuredChatCollectionSnapshot(14, 0x5DE550, 0x18, 10, 12, 0x5DE6BE),
        new StructuredChatCollectionSnapshot(17, 0x6002C0, 0x14, 4, 4, 0x60046B)
    ]));
Check("Structured chat collection prefixes and suffixes preserve minimum consumption without native padding",
    StructuredChatCollectionNativeLayout.SurroundingFields.Count == 19
    && StructuredChatCollectionNativeLayout.SurroundingFields
        .Where(field => field.ReaderFileOffset == 0x709560)
        .Sum(field => field.FixedWireByteCount) == StructuredChatCollectionNativeLayout.Case9PrefixWireByteCount
    && StructuredChatCollectionNativeLayout.SurroundingFields
        .Where(field => field.ReaderFileOffset == 0x6FE070)
        .Sum(field => field.FixedWireByteCount) == StructuredChatCollectionNativeLayout.Case14PrefixWireByteCount
    && StructuredChatCollectionNativeLayout.SurroundingFields
        .Where(field => field.ReaderFileOffset == 0x6FE190)
        .Sum(field => field.FixedWireByteCount ?? 1) + 1
            == StructuredChatCollectionNativeLayout.Case17MinimumPayloadByteCount);
Check("Structured chat case12 has a four-byte key and two independently counted DWORD collections",
    StructuredChatCollectionNativeLayout.Case12KeyWireByteCount == 4
    && StructuredChatCollectionNativeLayout.Case12MinimumPayloadByteCount == 1
    && StructuredChatCollectionNativeLayout.Case12MinimumEntryWireByteCount
        == StructuredChatCollectionNativeLayout.Case12KeyWireByteCount + 2
    && StructuredChatCollectionNativeLayout.Case12NativeNodeValueOffset == 12
    && StructuredChatCollectionNativeLayout.Case12NativeValueVectorOffset == 0
    && StructuredChatCollectionNativeLayout.Case12NativeValueKeySetOffset == 32
    && StructuredChatCollectionNativeLayout.UnpackExceptionPointerRttiName
        == ".PAVunpack_exception@serialize@sl@@");
Check("Structured chat case12 preserves owner layout and both notification field names",
    StructuredChatMessageNativeLayout.Case12OwnerCollectionOffset == 0x1FC
    && StructuredChatMessageNativeLayout.Case12OwnerNodeKeyOffset == 0x10
    && StructuredChatMessageNativeLayout.Case12OwnerNodeValueOffset == 0x18
    && StructuredChatMessageNativeLayout.Case12ChatIdField == "chatId"
    && StructuredChatMessageNativeLayout.Case12EntityIdField == "entityId");
foreach (bool ownerKeyPresent in new[] { false, true })
foreach (uint decodedEntryCount in new uint[] { 0, 1, 255, 256, uint.MaxValue })
foreach (bool mapsEqual in new[] { false, true })
{
    var update = StructuredChatMessageNativeLayout.CaptureCase12MapUpdate(
        ownerKeyPresent, decodedEntryCount, mapsEqual);
    bool expectedAssign = !ownerKeyPresent || !mapsEqual;
    bool expectedNotify = ownerKeyPresent ? !mapsEqual : decodedEntryCount != 0;
    Check($"Structured chat case12 existing={ownerKeyPresent}, entries={decodedEntryCount}, equal={mapsEqual}",
        update.CompareExistingMap == ownerKeyPresent
        && update.AssignDecodedMap == expectedAssign
        && update.EventCode == (expectedNotify ? 268 : (int?)null));
}
Check("Structured chat multi-entry prerequisite records six gated selectors and native range layout",
    StructuredChatMessageNativeLayout.MultiEntryPrerequisiteCalls.Select(call => call.Selector)
        .SequenceEqual(new byte[] { 9, 10, 11, 12, 13, 18 })
    && StructuredChatMessageNativeLayout.MultiEntryPrerequisiteCalls
        .All(call => call.FailureReturn == (call.Selector == 13))
    && StructuredChatMessageNativeLayout.PrerequisiteWordOffset == 0x118
    && StructuredChatMessageNativeLayout.PrerequisiteRangeBeginOffset == 0x10C
    && StructuredChatMessageNativeLayout.PrerequisiteRangeEndOffset == 0x110
    && StructuredChatMessageNativeLayout.PrerequisiteEntryNativeByteCount == 104);
foreach (uint nativeWord in new uint[] { 0, 1, uint.MaxValue })
foreach (var range in new (uint Begin, uint End, bool PassesWhenNonzero)[]
{
    (0, 0, false),
    (0, 103, false),
    (0, 104, false),
    (0, 207, false),
    (0, 208, true),
    (1000, 1312, true),
    (uint.MaxValue - 103, 104, true),
    (1, 0, false),
    (104, 0, true),
    (208, 0, true),
    (0, 0x80000000, true)
})
{
    Check($"Structured chat prerequisite word={nativeWord:X}, begin={range.Begin:X}, end={range.End:X}",
        StructuredChatMessageNativeLayout.PassesMultiEntryPrerequisite(nativeWord, range.Begin, range.End)
            == (nativeWord != 0 && range.PassesWhenNonzero));
}
Check("Structured chat paired-word helper compares both complete DWORDs without a nonzero prerequisite",
    StructuredChatMessageNativeLayout.OwnerCurrentWordPairOffset == 0x120
    && StructuredChatMessageNativeLayout.OwnerComparedWordPairOffset == 8
    && StructuredChatMessageNativeLayout.OwnerWordPairsEqual(0, 0)
    && StructuredChatMessageNativeLayout.OwnerWordPairsEqual(ulong.MaxValue, ulong.MaxValue)
    && !StructuredChatMessageNativeLayout.OwnerWordPairsEqual(0x0000000100000000, 0)
    && !StructuredChatMessageNativeLayout.OwnerWordPairsEqual(0x0000000000000001, 0)
    && !StructuredChatMessageNativeLayout.OwnerWordPairsEqual(0x1234567887654321, 0x8765432112345678));
foreach (bool prerequisite in new[] { false, true })
foreach (bool ownerPairsEqual in new[] { false, true })
foreach (bool keyPresent in new[] { false, true })
{
    var cache = StructuredChatMessageNativeLayout.CaptureCase13TextCache(
        prerequisite, ownerPairsEqual, keyPresent);
    bool lookup = prerequisite && ownerPairsEqual;
    Check($"Structured chat case13 prerequisite={prerequisite}, ownerEqual={ownerPairsEqual}, keyPresent={keyPresent}",
        cache.LookupTextKey == lookup
        && cache.RefreshExistingKeyPosition == (lookup && keyPresent)
        && cache.EventCodeBeforeStore == (lookup && !keyPresent ? 265 : (int?)null)
        && cache.StoreCallerQwordAndText == (lookup && !keyPresent)
        && cache.ReturnValue);

    var tip = StructuredChatMessageNativeLayout.CaptureCase18InstanceTip(
        prerequisite, ownerPairsEqual, keyPresent);
    Check($"Structured chat case18 prerequisite={prerequisite}, ownerEqual={ownerPairsEqual}, callerEqual={keyPresent}",
        tip.LocalizeAndDisplay == (prerequisite && !ownerPairsEqual && keyPresent)
        && !tip.ReturnValue);
}
Check("Structured chat text-cache native capacity and recency-list fields are recorded",
    StructuredChatMessageNativeLayout.Case13TextCacheOffset == 0x170
    && StructuredChatMessageNativeLayout.Case13CacheListSentinelOffset == 8
    && StructuredChatMessageNativeLayout.Case13CacheListCountOffset == 12
    && StructuredChatMessageNativeLayout.Case13CacheCapacityOffset == 16
    && StructuredChatMessageNativeLayout.Case13CacheTrimEnabledOffset == 20);
foreach (bool enabled in new[] { false, true })
foreach (var trimCase in new (uint Capacity, uint Count, uint Target, uint Remove)[]
{
    (0, 0, uint.MaxValue, 0),
    (0, 100, uint.MaxValue, 0),
    (0, uint.MaxValue, uint.MaxValue, 0),
    (1, 0, 0, 0),
    (1, 1, 0, 1),
    (1, 8, 0, 8),
    (3, 1, 2, 0),
    (3, 2, 2, 0),
    (3, 3, 2, 1),
    (3, 8, 2, 6),
    (uint.MaxValue, uint.MaxValue - 1, uint.MaxValue - 1, 0),
    (uint.MaxValue, uint.MaxValue, uint.MaxValue - 1, 1)
})
{
    var trim = StructuredChatMessageNativeLayout.CaptureCase13CacheTrim(
        enabled, trimCase.Capacity, trimCase.Count);
    Check($"Structured chat cache trim enabled={enabled}, capacity={trimCase.Capacity}, count={trimCase.Count}",
        trim.TargetCountBeforeAppend == (enabled ? trimCase.Target : (uint?)null)
        && trim.OldestKeysToRemove == (enabled ? trimCase.Remove : 0));
}
Check("All three observed app modes are cataloged", AppModeCatalog.KnownModes.SequenceEqual(expected));
Check("Mode 1 maps to tfLogin", AppModeCatalog.Find(1) == expected[0]);
Check("Mode 2 maps to tfWorld", AppModeCatalog.Find(2) == expected[1]);
Check("Mode 3 maps to tfLookingGlass", AppModeCatalog.Find(3) == expected[2]);
Check("Unobserved modes are not guessed", AppModeCatalog.Find(0) is null && AppModeCatalog.Find(4) is null);
Check("Pending-mode queue records the verified mode mapping and queue offsets",
    AppModeCatalog.KnownPendingRequests.SequenceEqual(
    [
        new PendingAppModeRequestSnapshot(1, "tfLogin", 0x6365B0),
        new PendingAppModeRequestSnapshot(2, "tfWorld", 0x64AE40),
        new PendingAppModeRequestSnapshot(3, "tfLookingGlass", 0x65A190)
    ])
    && AppModeCatalog.PendingModeQueueInsertMethodFileOffset == 0x30DEB0
    && AppModeCatalog.EnqueueModeIdWithZeroPayloadMethodFileOffset == 0x324080
    && AppModeCatalog.ApplicationModeInitializationHelperFileOffset == 0x307224
    && AppModeCatalog.DefaultInitialModeEnqueueCallFileOffset == 0x30797C
    && AppModeCatalog.InitialPendingModeDrainCallFileOffset == 0x30798E
    && AppModeCatalog.DefaultInitialModeId == 1
    && AppModeCatalog.LoginFallbackRequestHelperFileOffset == 0x30D790
    && AppModeCatalog.QueueLookingGlassRequestMethodFileOffset == 0x30D590
    && AppModeCatalog.TryQueueLookingGlassRequestMethodFileOffset == 0x306050
    && AppModeCatalog.ConditionalLoginModeQueueMethodFileOffset == 0x6385C0
    && AppModeCatalog.PendingModeQueueStorageOffset == 0xB84
    && AppModeCatalog.PendingModeQueueHeadOffset == 0xB90
    && AppModeCatalog.PendingModeQueueCountOffset == 0xB94
    && AppModeCatalog.PendingModeRequestModeIdOffset == 0
    && AppModeCatalog.PendingModeRequestPayloadOffset == 4);
Check("LookingGlass queue helper has six observed call sites",
    AppModeCatalog.KnownLookingGlassQueueCallSites.Select(site => site.FileOffset)
        .SequenceEqual([0x306050u, 0x3D60A0u, 0x6385C0u, 0x65D400u, 0x6AE3E0u, 0x6AF120u])
    && AppModeCatalog.KnownLookingGlassQueueCallSites.All(site => !string.IsNullOrWhiteSpace(site.ObservedUse)));
Check("LookingGlass exposes the replay-reader interface",
    AppModeCatalog.KnownInterfaces.SequenceEqual(
    [
        new AppModeInterfaceSnapshot(
            ClientAppModeId.LookingGlass,
            "nl::ReplayReader::GameInterface",
            4,
            0x1C6786C,
            0x186646C,
            0x1E12AB4,
            0x1A10EB4,
            0x661473,
            0x65E870,
            0x65E940,
            0x65EB80)
    ]));
Check("LookingGlass reader pointer offset is recorded",
    LookingGlassNativeLayout.ReplayReaderPointerOffset == 0x1334
    && LookingGlassNativeLayout.ConstructorFileOffset == 0x65A190
    && LookingGlassNativeLayout.RequestContextArgumentOffset == 0x18
    && LookingGlassNativeLayout.RequestContextCopySize == 0x34
    && LookingGlassNativeLayout.RequestContextSourceAtApplicationOffset == 0x9F0
    && LookingGlassNativeLayout.AdditionalConstructorArgumentApplicationOffset == 0x9E8
    && LookingGlassNativeLayout.AdditionalConstructorArgumentDestinationOffset == 0x58
    && LookingGlassNativeLayout.RequestContextFirstSharedTextOffset == 0x14
    && LookingGlassNativeLayout.RequestContextSecondSharedTextOffset == 0x1C);
Check("tfWorld native allocation and owner/scene offsets are recorded",
    TfWorldNativeLayout.AllocationSize == 0x2904
    && TfWorldNativeLayout.OwnerContextOffset == 0x10
    && TfWorldNativeLayout.ScenePointerOffset == 0x14);
Check("tfWorld input-layer offsets preserve observed IDs",
    TfWorldNativeLayout.LayersByOffset.SequenceEqual(
    [
        new TfWorldLayerSnapshot(0x2FC, 0),
        new TfWorldLayerSnapshot(0xC4C, 9),
        new TfWorldLayerSnapshot(0x159C, 8),
        new TfWorldLayerSnapshot(0x1EEC, 9)
    ]));
Check("tfWorld layer-mode method and layer-stack address are recorded",
    TfWorldNativeLayout.InitializationMethodFileOffset == 0x64B460
    && TfWorldNativeLayout.ShutdownMethodFileOffset == 0x64B950
    && TfWorldNativeLayout.ObserverStateMethodFileOffset == 0x64BC40
    && TfWorldNativeLayout.LayerModeMethodFileOffset == 0x64C020
    && TfWorldNativeLayout.RecordInputMethodFileOffset == 0x64DCF0
    && TfWorldNativeLayout.CallbackStateResetMethodFileOffset == 0x651A60
    && TfWorldNativeLayout.MemberAt2E8GetterMethodFileOffset == 0x64A0C0
    && TfWorldNativeLayout.MemberAt2ECGetterMethodFileOffset == 0x64A0D0
    && TfWorldNativeLayout.LayerStackGlobalVirtualAddress == 0x1DFFEF4);
Check("tfWorld record-input size, state gate, and observed float offsets are recorded",
    TfWorldNativeLayout.RecordInputSize == 0x20
    && TfWorldNativeLayout.MemberAt2E8Offset == 0x2E8
    && TfWorldNativeLayout.MemberAt2ECOffset == 0x2EC
    && TfWorldNativeLayout.MemberAt2E8InitialValue == 0
    && TfWorldNativeLayout.MemberAt2ECAllocatedBlockSize == 0x2C
    && TfWorldNativeLayout.RecordInputRequiredSceneSubstate == 3
    && TfWorldNativeLayout.RecordInputStoredInModeOffset == 0x20
    && TfWorldNativeLayout.RecordInputFirstObservedFloatOffset == 8
    && TfWorldNativeLayout.RecordInputSecondObservedFloatOffset == 12);
Check("World scene-factory resources, owned allocations, and tfPhysicsDrawInterface RTTI are recorded",
    SceneFactoryNativeLayout.FactoryFileOffset == 0x9B73F0
    && SceneFactoryNativeLayout.RootAllocationSize == 0xA0
    && SceneFactoryNativeLayout.RootConstructorFileOffset == 0x9B82F0
    && SceneFactoryNativeLayout.RootDestructorFileOffset == 0x9B8490
    && SceneFactoryNativeLayout.RootReturnFileOffset == 0x9B82B4
    && SceneFactoryNativeLayout.KnownSceneResources.SequenceEqual(
    [
        new SceneFactoryResourceSnapshot("RenderScene", 0x00, 0x179BD28),
        new SceneFactoryResourceSnapshot("WorldMapScene", 0x04, 0x179BE38),
        new SceneFactoryResourceSnapshot("SinvironmentScene", 0x08, 0x179BE48)
    ])
    && SceneFactoryNativeLayout.KnownChildAllocations.SequenceEqual(
    [
        new SceneFactoryChildAllocationSnapshot(0x0C, 0x138, false),
        new SceneFactoryChildAllocationSnapshot(0x14, 0x188, false),
        new SceneFactoryChildAllocationSnapshot(0x18, 0x060, false),
        new SceneFactoryChildAllocationSnapshot(0x1C, 0x044, false),
        new SceneFactoryChildAllocationSnapshot(0x20, 0x044, false),
        new SceneFactoryChildAllocationSnapshot(0x24, 0x060, false),
        new SceneFactoryChildAllocationSnapshot(0x28, 0x03C, false),
        new SceneFactoryChildAllocationSnapshot(0x2C, 0x138, true),
        new SceneFactoryChildAllocationSnapshot(0x30, 0x024, false),
        new SceneFactoryChildAllocationSnapshot(0x34, 0x25C8, false),
        new SceneFactoryChildAllocationSnapshot(0x3C, 0x058, false),
        new SceneFactoryChildAllocationSnapshot(0x4C, 0x390, false),
        new SceneFactoryChildAllocationSnapshot(0x50, 0x1B4, true),
        new SceneFactoryChildAllocationSnapshot(0x54, 0xF28, false),
        new SceneFactoryChildAllocationSnapshot(0x58, 0x080, false),
        new SceneFactoryChildAllocationSnapshot(0x5C, 0x058, false),
        new SceneFactoryChildAllocationSnapshot(0x60, 0x02C, false),
        new SceneFactoryChildAllocationSnapshot(0x64, 0x2F4C, false),
        new SceneFactoryChildAllocationSnapshot(0x68, 0x044, false),
        new SceneFactoryChildAllocationSnapshot(0x6C, 0x008, false),
        new SceneFactoryChildAllocationSnapshot(0x70, 0x0C0, false),
        new SceneFactoryChildAllocationSnapshot(0x74, 0x0D8, false),
        new SceneFactoryChildAllocationSnapshot(0x78, 0x21C, false),
        new SceneFactoryChildAllocationSnapshot(0x84, 0x024, false),
        new SceneFactoryChildAllocationSnapshot(0x88, 0x048, true),
        new SceneFactoryChildAllocationSnapshot(0x90, 0x114, false)
    ])
    && SceneFactoryNativeLayout.PhysicsDrawInterfacePointerOffset == 0x14
    && SceneFactoryNativeLayout.PhysicsDrawInterfaceAllocationSize == 0x188
    && SceneFactoryNativeLayout.PhysicsDrawInterfaceVtableVirtualAddress == 0x1C70968
    && SceneFactoryNativeLayout.PhysicsDrawInterfaceRttiName == ".?AVtfPhysicsDrawInterface@@"
    && SceneFactoryNativeLayout.TfWorldScenePointerOffset == TfWorldNativeLayout.ScenePointerOffset);
Check("tfLogin native layout and lifecycle addresses are recorded",
    LoginModeNativeLayout.AllocationSize == 0xA58
    && LoginModeNativeLayout.ConstructorFileOffset == 0x6365B0
    && LoginModeNativeLayout.InitializeFileOffset == 0x636FF0
    && LoginModeNativeLayout.ShutdownFileOffset == 0x63B360
    && LoginModeNativeLayout.DeletingDestructorFileOffset == 0x636AD0
    && LoginModeNativeLayout.VtableVirtualAddress == 0x1C676C0
    && LoginModeNativeLayout.VtableFileOffset == 0x18662C0
    && LoginModeNativeLayout.CharacterRecordsBeginOffset == 0x9F4
    && LoginModeNativeLayout.CharacterRecordsEndOffset == 0x9F8
    && LoginModeNativeLayout.CharacterRecordStride == 0xD0
    && LoginModeNativeLayout.CharacterRecordMatchTextOffset == 8
    && LoginModeNativeLayout.SelectedCharacterPairFirstDestinationOffset == 0xA40
    && LoginModeNativeLayout.SelectedCharacterPairSecondDestinationOffset == 0xA44
    && LoginModeNativeLayout.CharacterRefreshHandlerFileOffset == 0x63C5F0
    && LoginModeNativeLayout.CharacterRefreshRequestMethodVtableOffset == 0xAC
    && LoginModeNativeLayout.CharacterRefreshRequestMessageId == 0x89
    && LoginModeNativeLayout.LoginRequestSubmissionMethodFileOffset == 0x63B550
    && LoginModeNativeLayout.LoginRequestCallbackPairOffset == 0x9D8
    && LoginModeNativeLayout.LoginRequestInFlightFlagOffset == 0x9E0
    && LoginModeNativeLayout.LoginRequestOptionFlagOffset == 0x9E2
    && LoginModeNativeLayout.LoginRequestObjectAllocationSize == 0x78
    && LoginModeNativeLayout.LoginRequestCallbackPairOffsetInObject == 0x50
    && LoginModeNativeLayout.LoginRequestTypeOffsetInObject == 0x08
    && LoginModeNativeLayout.LoginRequestTypeValue == 2
    && LoginModeNativeLayout.LoginAutoRetryCounterOffset == 0x9E4
    && LoginModeNativeLayout.LoginAutoRetryCounterInitialValue == 3
    && LoginModeNativeLayout.LoginAutoRetryDelayMilliseconds == 500);
Check("tfLogin observed UI layout and 15 input action names are recorded",
    LoginModeNativeLayout.UiLayout == "LoginUI.xml"
    && LoginModeNativeLayout.InputContextOffset == 0x50
    && LoginModeNativeLayout.KnownInputActions.SequenceEqual(
    [
        "LeftClick", "RightClick", "ArrowUp", "ArrowDown", "ArrowLeft",
        "ArrowRight", "ScrollUp", "ScrollDown", "ToStart", "ToEnd",
        "Delete", "Tab", "Submit", "Escape", "RunConsoleMacro"
    ]));
Check("tfLogin response handler field catalogs are recorded",
    LoginModeNativeLayout.KnownResponseHandlers.Select(handler => handler.MethodFileOffset)
        .SequenceEqual([0x63B7C0u, 0x63C5F0u, 0x63D270u])
    && LoginModeNativeLayout.KnownResponseHandlers[0].JsonKeys.Contains("can_login")
    && LoginModeNativeLayout.KnownResponseHandlers[1].JsonKeys.Contains("character_guid")
    && LoginModeNativeLayout.KnownResponseHandlers[2].Purpose == "Zone list response");
Check("tfLogin constructor callback wrappers target the three response handlers",
    LoginModeNativeLayout.ResponseCallbackObjectSize == 0x20
    && LoginModeNativeLayout.ResponseCallbackOwnerPointerOffset == 4
    && LoginModeNativeLayout.ResponseCallbackTargetPointerOffset == 0x10
    && LoginModeNativeLayout.ResponseCallbackOwnerAdjustmentOffset == 0x14
    && LoginModeNativeLayout.ResponseCallbackOwnerAdjustmentInitialValue == 0
    && LoginModeNativeLayout.ResponseCallbackDispatchMethodVtableOffset == 4
    && LoginModeNativeLayout.ResponseCallbackDispatchMethodFileOffset == 0x643380
    && LoginModeNativeLayout.ResponseCallbackVtableVirtualAddress == 0x1C674A8
    && LoginModeNativeLayout.ResponseCallbackVtableFileOffset == 0x18660A8
    && LoginModeNativeLayout.KnownResponseCallbacks.SequenceEqual(
    [
        new LoginResponseCallbackSnapshot("Login response", 0x9D8, 0xA3C3C0, 0x63B7C0),
        new LoginResponseCallbackSnapshot("Character refresh response", 0x9E8, 0xA3D1F0, 0x63C5F0),
        new LoginResponseCallbackSnapshot("Zone-list response", 0xA00, 0xA3DE70, 0x63D270)
    ]));
Check("All three Login response callbacks have observed request attachment sites",
    LoginModeNativeLayout.KnownRequestAttachments.SequenceEqual(
    [
        new LoginRequestAttachmentSnapshot("Login response", 0x63B550, 0x63B653,
            0x9D8, 2, "%s/api/v2/accounts/login", 0x9E0),
        new LoginRequestAttachmentSnapshot("Character refresh response", 0x63ECE0, 0x63EDBC,
            0x9E8, 1, "%s/api/v2/characters/list", 0x9F0),
        new LoginRequestAttachmentSnapshot("Zone-list response", 0x63E390, 0x63E56C,
            0xA00, 2, "%s/api/v1/server/list", null)
    ])
    && LoginModeNativeLayout.KnownRequestAttachments.All(request =>
        LoginModeNativeLayout.KnownResponseCallbacks.Any(callback =>
            callback.OwnerMemberOffset == request.OwnerCallbackPairOffset
            && callback.Purpose == request.Purpose))
    && LoginModeNativeLayout.RequestObjectConstructorFileOffset == 0x3843F0
    && LoginModeNativeLayout.RequestPreparationMethodFileOffset == 0x636550
    && LoginModeNativeLayout.RequestSubmissionWrapperFileOffset == 0x6499F0
    && LoginModeNativeLayout.RequestManagerSubmissionMethodFileOffset == 0xBC43C0);
Check("Zone-list request state switch is translated from code-section VAs",
    LoginModeNativeLayout.AutoLoginStateDispatchMethodFileOffset == 0x63E390
    && LoginModeNativeLayout.AutoLoginStateOffset == 0xA48
    && LoginModeNativeLayout.ZoneListRequestAutoLoginState == 2
    && LoginModeNativeLayout.AutoLoginStateJumpTableVirtualAddress == 0xA3F574
    && LoginModeNativeLayout.AutoLoginStateJumpTableFileOffset == 0x63E974
    && LoginModeNativeLayout.CharacterRefreshRequestInFlightFlagOffset == 0x9F0);
Check("Character-refresh outgoing dispatch uses a distinct runtime receiver global",
    LoginModeNativeLayout.CharacterRefreshRequestReceiverGlobalVirtualAddress == 0x1E002B8
    && LoginModeNativeLayout.CharacterRefreshRequestReceiverGlobalFileOffset == 0x19FE6B8);
Check("Shared request worker selects the verified WinHTTP verbs and default",
    new[] { 1, 2, 3, 4, 5 }.Select(ClientApiRequestNativeLayout.ResolveHttpMethod)
        .SequenceEqual(["GET", "POST", "PUT", "DELETE", "HEAD"])
    && new[] { int.MinValue, -1, 0, 6, int.MaxValue }
        .All(type => ClientApiRequestNativeLayout.ResolveHttpMethod(type) == "GET")
    && LoginModeNativeLayout.KnownRequestAttachments.Select(request => request.HttpMethod)
        .SequenceEqual(["POST", "GET", "POST"])
    && ClientApiRequestNativeLayout.AllocationSize == 0x78
    && ClientApiRequestNativeLayout.TypeOffset == 8
    && ClientApiRequestNativeLayout.WeakCallbackPointerOffset == 0x50
    && ClientApiRequestNativeLayout.WeakCallbackControlBlockOffset == 0x54
    && ClientApiRequestNativeLayout.MethodJumpTableVirtualAddress == 0xFC44C0
    && ClientApiRequestNativeLayout.MethodJumpTableFileOffset == 0xBC38C0
    && ClientApiRequestNativeLayout.WinHttpOpenRequestCallFileOffset == 0xBC2DB7);
Check("Request completion delivers the response through an acquired weak callback",
    ClientApiRequestNativeLayout.ManagerPumpMethodFileOffset == 0xBC4550
    && ClientApiRequestNativeLayout.RoutePumpMethodFileOffset == 0xBC3F80
    && ClientApiRequestNativeLayout.WorkerPumpMethodFileOffset == 0xBC2B30
    && ClientApiCallbackDeliveryNativeLayout.WorkerActiveCompletionPointerOffset == 0x1C
    && ClientApiCallbackDeliveryNativeLayout.CompletionRequestHandleOffset == 0
    && ClientApiCallbackDeliveryNativeLayout.RequiredCompletionRequestHandleValue == 0
    && ClientApiCallbackDeliveryNativeLayout.CompletionResponsePointerOffset == 0x18
    && ClientApiCallbackDeliveryNativeLayout.AcquireCallbackCallFileOffset == 0xBC37E2
    && ClientApiCallbackDeliveryNativeLayout.AcquireCallbackMethodFileOffset == 0xBC4A80
    && ClientApiCallbackDeliveryNativeLayout.TryAcquireStrongReferenceMethodFileOffset == 0x327730
    && ClientApiCallbackDeliveryNativeLayout.ControlBlockStrongCountOffset == 4
    && ClientApiCallbackDeliveryNativeLayout.ControlBlockWeakCountOffset == 8
    && ClientApiCallbackDeliveryNativeLayout.CallbackVirtualCallFileOffset == 0xBC37FC
    && ClientApiCallbackDeliveryNativeLayout.CallbackVirtualMethodOffset
        == LoginModeNativeLayout.ResponseCallbackDispatchMethodVtableOffset
    && ClientApiResponseNativeLayout.AllocationSize == 0x20
    && ClientApiResponseNativeLayout.RequestPointerOffset == 4
    && ClientApiResponseNativeLayout.ResultKindOffset == 8
    && ClientApiResponseNativeLayout.ResultKindInitialValue == 1
    && ClientApiResponseNativeLayout.StatusWordOffset == 0x0C
    && ClientApiResponseNativeLayout.MetadataTreeHeadOffset == 0x10
    && ClientApiResponseNativeLayout.MetadataTreeCountOffset == 0x14
    && ClientApiResponseNativeLayout.FirstTextOffset == 0x18
    && ClientApiResponseNativeLayout.SecondTextOffset == 0x1C);
Check("WinHTTP registered callback and handle-closing signaling are recorded",
    ClientApiWinHttpCompletionNativeLayout.CompletionAllocationSize == 0x1C
    && ClientApiWinHttpCompletionNativeLayout.CompletionRequestHandleOffset == 0
    && ClientApiWinHttpCompletionNativeLayout.CompletionCleanupFlagOffset == 0x11
    && ClientApiWinHttpCompletionNativeLayout.CompletionDispatchResultOffset == 0x14
    && ClientApiWinHttpCompletionNativeLayout.CompletionResponsePointerOffset == 0x18
    && ClientApiWinHttpCompletionNativeLayout.StatusCallbackFileOffset == 0xBC26E0
    && ClientApiWinHttpCompletionNativeLayout.StatusCallbackVirtualAddress == 0xFC32E0
    && ClientApiWinHttpCompletionNativeLayout.InternalStatusDispatcherFileOffset == 0xBC1DA0
    && ClientApiWinHttpCompletionNativeLayout.RegisterStatusCallbackCallFileOffset == 0xBC31E9
    && ClientApiWinHttpCompletionNativeLayout.RegisterStatusCallbackImportVirtualAddress == 0x1AC4D0C
    && ClientApiWinHttpCompletionNativeLayout.SendRequestCallFileOffset == 0xBC3213
    && ClientApiWinHttpCompletionNativeLayout.SendRequestContextPushFileOffset == 0xBC31F8
    && ClientApiWinHttpCompletionNativeLayout.WorkerTeardownMethodFileOffset == 0xBC2890
    && ClientApiWinHttpCompletionNativeLayout.MarkCallbackOwnedCleanupFileOffset == 0xBC28CB
    && ClientApiWinHttpCompletionNativeLayout.RouteDestructorFileOffset == 0xBC3B80
    && ClientApiWinHttpCompletionNativeLayout.WorkerAdditionalHandleOffset == 0x20
    && ClientApiWinHttpCompletionNativeLayout.CloseAdditionalHandleDuringTeardownFileOffset == 0xBC28D7
    && ClientApiWinHttpCompletionNativeLayout.ClearRequestHandleOnClosingFileOffset == 0xBC2728
    && ClientApiWinHttpCompletionNativeLayout.HandleClosingStatus == 0x800
    && ClientApiWinHttpCompletionNativeLayout.ReadCompleteStatus == 0x80000
    && ClientApiWinHttpCompletionNativeLayout.RequestErrorStatus == 0x200000);
Check("Handle closing supplies result 3 only when no result has been assigned",
    ClientApiWinHttpCompletionNativeLayout.ApplyHandleClosingResult(0) == 3
    && new[] { int.MinValue, -1, 1, 2, 3, 4, int.MaxValue }.All(result =>
        ClientApiWinHttpCompletionNativeLayout.ApplyHandleClosingResult(result) == result));
LoginZoneRecord Zone(uint id, ushort protocol = 0x4C5F, ushort players = 1) =>
    new(id, $"zone-{id}", protocol, "build", $"matrix-{id}", players);

var zoneRecords = new[] { Zone(1, 0), Zone(2), Zone(3, players: 0), Zone(4), Zone(5) };
Check("Zone decoder field names preserve the verified 24-byte layout",
    LoginZoneListNativeLayout.RecordDecoderFileOffset == 0x635BE0
    && LoginZoneListNativeLayout.RecordStride == 24
    && LoginZoneListNativeLayout.ZoneIdOffset == 0
    && LoginZoneListNativeLayout.ZoneNameOffset == 4
    && LoginZoneListNativeLayout.ProtocolVersionOffset == 8
    && LoginZoneListNativeLayout.BuildOffset == 12
    && LoginZoneListNativeLayout.MatrixUrlOffset == 16
    && LoginZoneListNativeLayout.PlayersOffset == 20
    && LoginModeNativeLayout.KnownResponseHandlers[2].JsonKeys.SequenceEqual(
        ["zone_list", "zone_id", "zone_name", "protocol_version", "build", "matrix_url", "players"]));
Check("Zone response is deferred below signed auto-login state 3",
    LoginZoneListSelection.Select(-1, "unchanged", zoneRecords, false, 0,
        _ => throw new InvalidOperationException("Deferred response must not match zones"))
        == new LoginZoneSelectionResult(LoginZoneSelectionOutcome.Deferred, -1, "unchanged", 0));
Check("Zone filtering preserves native swap-removal order and selects matrix_url",
    new[] { 0u, 1u, 2u, 3u }.Select(sample =>
        LoginZoneListSelection.Select(3, "random", zoneRecords, true, sample,
            _ => throw new InvalidOperationException("Random selection must not match zones")).DestinationText)
        .SequenceEqual(["matrix-5", "matrix-2", "matrix-4", "matrix-5"])
    && zoneRecords.Select(record => record.ZoneId).SequenceEqual([1u, 2u, 3u, 4u, 5u]));
Check("Named zone selection samples only matching eligible candidates",
    LoginZoneListSelection.Select(4, "filter", zoneRecords, false, 1, id => id >= 4)
        == new LoginZoneSelectionResult(LoginZoneSelectionOutcome.Selected, 5, "matrix-4", 2));
Check("Zone selection explicitly reports failure and clears destination",
    LoginZoneListSelection.Select(3, "filter", zoneRecords, false, 0, _ => false)
        == new LoginZoneSelectionResult(LoginZoneSelectionOutcome.NoMatchingCandidate, 4, "", 0)
    && LoginZoneListSelection.Select(3, "random", Array.Empty<LoginZoneRecord>(), true, 0, _ => true)
        == new LoginZoneSelectionResult(LoginZoneSelectionOutcome.NoMatchingCandidate, 4, "", 0));
Check("Zone response increments the native signed state with DWORD wraparound",
    LoginZoneListSelection.Select(int.MaxValue, "random", [Zone(9)], true, uint.MaxValue, _ => false)
        == new LoginZoneSelectionResult(LoginZoneSelectionOutcome.Selected, int.MinValue, "matrix-9", 1));
Check("Auto-login state 5 prepares selected character and requests matrix connection",
    LoginModeNativeLayout.ConnectSelectedCharacterAutoLoginState == 5
    && LoginModeNativeLayout.ConnectSelectedCharacterStateBranchFileOffset == 0x63E890
    && LoginModeNativeLayout.PrepareSelectedCharacterMethodFileOffset == 0x63EE30
    && LoginModeNativeLayout.RequestMatrixConnectionMethodFileOffset == 0x63F230
    && LoginModeNativeLayout.SelectedCharacterPropertiesOffsetInRecord == 0xC8
    && LoginModeNativeLayout.CharacterConnectionObjectMemberOffset == 0x1C
    && LoginModeNativeLayout.CharacterConnectionObjectAllocationSize == 0xBB0
    && LoginModeNativeLayout.CharacterConnectionConstructorCallSiteFileOffset == 0x63F0EE
    && LoginModeNativeLayout.CharacterConnectionStateOffset == 0x0C
    && LoginModeNativeLayout.CharacterConnectionRequiredStateUpperBoundExclusive == 2
    && LoginModeNativeLayout.MatrixConnectionRequestBoundaryFileOffset == 0x494CA0
    && LoginModeNativeLayout.KnownCharacterConnectionPropertyKeys
        .SequenceEqual(["name", "is_dev", "race", "gender"])
    && LoginModeNativeLayout.MatrixOverrideConfigSection == "Development"
    && LoginModeNativeLayout.MatrixOverrideConfigKey == "MatrixOverride");
Check("Lua System API bindings connect preload character setup to Matrix connection entry",
    LoginModeNativeLayout.SystemApiRegistrationRoutineFileOffset == 0x6429E0
    && LoginModeNativeLayout.KnownLuaSystemApiBindings.SequenceEqual(
    [
        new LoginSystemApiBindingSnapshot("SetupCharacter", 0x1B63184, 0x642DF7,
            0xA46A60, 0x645E60, 0x641950),
        new LoginSystemApiBindingSnapshot("StartInstanceConnection", 0x1B63194, 0x642E47,
            0xA46A70, 0x645E70, 0x641C50)
    ])
    && LoginModeNativeLayout.SetupCharacterPreparationCallSiteFileOffset == 0x641BF3
    && LoginModeNativeLayout.StartInstanceConnectionValidationCallSiteFileOffset == 0x641DD4
    && LoginModeNativeLayout.MatrixConnectionBeginCallSiteFileOffset == 0x63F57F
    && LoginModeNativeLayout.KnownCharacterConnectionPropertyKeys
        .SequenceEqual(["name", "is_dev", "race", "gender"]));
Check("Matrix destination source precedence and empty input are explicit",
    LoginMatrixDestinationSelection.Select("lua-url", "development-url", "login-url")
        == new LoginMatrixDestinationSelectionResult(
            "login-url", LoginMatrixDestinationSource.LoginSelectedMatrixUrl)
    && LoginMatrixDestinationSelection.Select("lua-url", "development-url", null)
        == new LoginMatrixDestinationSelectionResult(
            "development-url", LoginMatrixDestinationSource.DevelopmentMatrixOverride)
    && LoginMatrixDestinationSelection.Select("lua-url", null, "")
        == new LoginMatrixDestinationSelectionResult("lua-url", LoginMatrixDestinationSource.LuaArgument)
    && LoginMatrixDestinationSelection.Select(null, "", null)
        == new LoginMatrixDestinationSelectionResult("", LoginMatrixDestinationSource.None));
Check("tfLogin default update branch queues World mode {2,0} behind the zero guard",
    LoginModeNativeLayout.LoginUpdateMethodFileOffset == 0x6385C0
    && LoginModeNativeLayout.LoginUpdateSwitchDefaultTargetFileOffset == 0x638AB3
    && LoginModeNativeLayout.LoginUpdateGuardGlobalVa == 0x1E5CAA4
    && LoginModeNativeLayout.LoginUpdateGuardByteOffset == 0x40
    && LoginModeNativeLayout.LoginUpdateWorldModeGuardBranchFileOffset == 0x638AC6
    && LoginModeNativeLayout.LoginUpdateWorldModeQueueRequestBuildFileOffset == 0x638B89
    && LoginModeNativeLayout.LoginUpdateWorldModeQueueCallSiteFileOffset == 0x638BAF);
Check("Matrix connection requests oracle tickets through a distinct callback wrapper",
    MatrixConnectionNativeLayout.AllocationSize == 0xBB0
    && MatrixConnectionNativeLayout.PreferredMatrixTextOffset == 0xBA0
    && MatrixConnectionNativeLayout.PreferredMatrixTextInitializationFileOffset == 0x48F0F9
    && MatrixConnectionNativeLayout.OracleCallbackPairOffset == 0xB98
    && MatrixConnectionNativeLayout.OracleRequestInFlightFlagOffset == 0x1C
    && MatrixConnectionNativeLayout.OracleCallbackAllocationSize == 0x10
    && MatrixConnectionNativeLayout.OracleCallbackOwnerPointerOffset == 4
    && MatrixConnectionNativeLayout.OracleCallbackTargetPointerOffset == 8
    && MatrixConnectionNativeLayout.OracleCallbackOwnerAdjustmentOffset == 12
    && MatrixConnectionNativeLayout.OracleCallbackTargetVirtualAddress == 0x896060
    && MatrixConnectionNativeLayout.OracleCallbackTrampolineFileOffset == 0x496DF0
    && MatrixConnectionNativeLayout.QueueOracleTicketCallSiteFileOffset == 0x494D0B
    && MatrixConnectionNativeLayout.QueueOracleTicketMethodFileOffset == 0x494E30
    && MatrixConnectionNativeLayout.EmptyTextPredicateMethodFileOffset == 0x2FCD00
    && MatrixConnectionNativeLayout.ExistingConnectionResetMethodFileOffset == 0x490090
    && MatrixConnectionNativeLayout.CharacterMetadataApplyMethodFileOffset == 0x490390
    && MatrixConnectionNativeLayout.PreferredDestinationEmptyCheckCallSiteFileOffset == 0x494CF4
    && MatrixConnectionNativeLayout.OracleMatrixFieldEmptyCheckCallSiteFileOffset == 0x495257
    && MatrixConnectionNativeLayout.OracleCallbackAttachmentFileOffset == 0x49510C
    && MatrixConnectionNativeLayout.OracleRequestSubmissionFileOffset == 0x4953DF
    && MatrixConnectionNativeLayout.OracleResponseHandlerFileOffset == 0x495460
    && MatrixConnectionNativeLayout.OracleResponseParserFileOffset == 0x4956C0
    && MatrixConnectionNativeLayout.OpenMatrixConnectionMethodFileOffset == 0x495F20
    && MatrixConnectionNativeLayout.EndpointFormat == "%s/api/v1/oracle/ticket"
    && ClientApiRequestNativeLayout.ResolveHttpMethod(MatrixConnectionNativeLayout.OracleRequestType) == "POST"
    && MatrixConnectionNativeLayout.KnownResponseKeys.SequenceEqual(
        ["matrix_url", "ticket", "datacenter", "operator_override", "session_id", "hostname"]));
Check("UI manager singleton and global event routing boundaries are recorded",
    UiManagerNativeLayout.SingletonRegistrationStubFileOffset == 0x3EE8C0
    && UiManagerNativeLayout.SingletonInitializeMethodFileOffset == 0x3E44F0
    && UiManagerNativeLayout.SingletonListWalkerFileOffset == 0x12C3A80
    && UiManagerNativeLayout.ClientApplicationBootstrapFileOffset == 0x2EAE70
    && UiManagerNativeLayout.SingletonObjectVirtualAddress == 0x1E002A8
    && UiManagerNativeLayout.UiManagerGlobalPointerVirtualAddress == 0x1E002B8
    && UiManagerNativeLayout.UiManagerAllocationSize == 0xA10
    && UiManagerNativeLayout.UiManagerEventDispatchMethodFileOffset == 0xBF9D80
    && UiManagerNativeLayout.UiManagerEventNameToIdMethodFileOffset == 0xBF9AB0
    && UiManagerNativeLayout.UiManagerEventListenerRegisterMethodFileOffset == 0xBF9B40
    && UiManagerNativeLayout.UiManagerEventListenerUnregisterMethodFileOffset == 0xBF9BD0
    && UiManagerNativeLayout.UiManagerEventListenerDispatchFileOffset == 0xBF9C40
    && UiManagerNativeLayout.UiManagerEventListenerLookupFileOffset == 0xC0AA20
    && UiManagerNativeLayout.UiManagerEventCallbackRouteFileOffset == 0xC38400
    && UiManagerNativeLayout.UiManagerNamedSubscriptionFileOffset == 0xC35BB0
    && UiManagerNativeLayout.UiComponentRootXmlParserFileOffset == 0xC34F50
    && UiManagerNativeLayout.UiComponentEventsSectionParserFileOffset == 0xC35740
    && UiManagerNativeLayout.UiComponentEventsSectionParserCallSiteFileOffset == 0xC353B0
    && UiManagerNativeLayout.UiComponentEventNameToIdCallSiteFileOffset == 0xC358AF
    && UiManagerNativeLayout.UiComponentEventBindLookupCallSiteFileOffset == 0xC358FE
    && UiManagerNativeLayout.UiComponentEventListenerRegisterCallSiteFileOffset == 0xC35AD8
    && UiManagerNativeLayout.ComponentBindEventApiFileOffset == 0xC32460
    && UiManagerNativeLayout.ComponentBindEventNamedSubscriptionCallSiteFileOffset == 0xC32643
    && UiManagerNativeLayout.ComponentEventsSectionNameVirtualAddress == 0x1BBCC6C
    && UiManagerNativeLayout.ComponentEventElementNameVirtualAddress == 0x1BBCCDC
    && UiManagerNativeLayout.ComponentEventNameAttributeVirtualAddress == 0x1BBCD1C
    && UiManagerNativeLayout.ComponentEventBindAttributeVirtualAddress == 0x1BBCCD4
    && UiManagerNativeLayout.ComponentBindEventUsageVirtualAddress == 0x1BBC760
    && UiManagerNativeLayout.UiManagerEventDispatchVtableOffset == 0xAC
    && UiManagerNativeLayout.UiManagerEventRegistryOffset == 0x6F0
    && UiManagerNativeLayout.UiManagerEventNameMapOffset == 0x6E8
    && UiManagerNativeLayout.UiManagerEventNameArrayBeginOffset == 0x71C
    && UiManagerNativeLayout.UiManagerEventNameArrayEndOffset == 0x720
    && UiManagerNativeLayout.UiManagerEventNameArrayBeginInitialValue == 0
    && UiManagerNativeLayout.UiManagerEventNameArrayEndInitialValue == 0
    && UiManagerNativeLayout.UiManagerEventNameArrayBeginInitializationFileOffset == 0xBF7FF6
    && UiManagerNativeLayout.UiManagerEventNameArrayEndInitializationFileOffset == 0xBF8000
    && UiManagerNativeLayout.UiManagerEventReadyFlagOffset == 0x672
    && UiManagerNativeLayout.LoginModeMessage145GlobalLoadFileOffset == 0x63C1E2
    && UiManagerNativeLayout.LoginModeMessage145CallFileOffset == 0x63C1F5
    && UiManagerNativeLayout.LoginModeMessageId == 0x145);
Check("UI manager initialization guard rejects nonzero slot+4 getter results and preserves later flag outcomes",
    UiManagerNativeLayout.UiManagerInitializeMethodFileOffset == 0xBFB310
    && UiManagerNativeLayout.UiManagerInitializationFlagGetterMethodFileOffset == 0xBFA0C0
    && UiManagerNativeLayout.UiManagerInitializationFlagGetterVtableOffset == 0x04
    && UiManagerNativeLayout.UiManagerInitializePartialFlagWriteFileOffset == 0xBFB350
    && UiManagerNativeLayout.UiManagerInitializeReadyFlagWriteFileOffset == 0xBFB8DC
    && UiManagerNativeLayout.UiManagerConstructorStateWordOffset == 0x670
    && UiManagerNativeLayout.UiManagerConstructorStateWordValue == 1
    && UiManagerNativeLayout.UiManagerInitializationPartialFlagOffset == 0x671
    && UiManagerNativeLayout.UiManagerEventReadyFlagOffset == 0x672
    && Enumerable.Range(0, byte.MaxValue + 1).All(value =>
        UiManagerNativeLayout.ShouldEnterInitialization((byte)value) == (value == 0))
    && UiManagerNativeLayout.CaptureObservedInitializationFlags(
        UiManagerInitializationOutcome.PartialInitializationFailed)
        == new UiManagerInitializationFlagSnapshot(1, 0)
    && UiManagerNativeLayout.CaptureObservedInitializationFlags(
        UiManagerInitializationOutcome.Initialized)
        == new UiManagerInitializationFlagSnapshot(1, 1)
    && UiManagerNativeLayout.CaptureObservedInitializationFlags(
        UiManagerInitializationOutcome.Initialized).ShutdownStateWord == 0x0101);
Check("UI startup callback stores the getter-sourced descriptor at +A08 before setting the ready flag",
    UiManagerNativeLayout.UiManagerDescriptorGetterCallSiteFileOffset == 0x30B1B4
    && UiManagerNativeLayout.UiManagerDescriptorGetterResultAddressOffset == 0x48
    && UiManagerNativeLayout.UiManagerInitializeDescriptorArgumentOrdinal == 8
    && UiManagerNativeLayout.UiManagerInitializeDescriptorArgumentFrameOffset == 0x24
    && UiManagerNativeLayout.UiManagerInitializeDescriptorCallbackCallSiteFileOffset == 0xBFB8C6
    && UiManagerNativeLayout.UiManagerInitializeDescriptorCallbackReceiverOffset == 0x974
    && UiManagerNativeLayout.UiManagerInitializeDescriptorCallbackVtableOffset == 0x30
    && UiManagerNativeLayout.UiManagerInitializeDescriptorCallbackArgument == 1
    && !UiManagerNativeLayout.UiManagerInitializeDescriptorCallbackReturnIsUsed
    && UiManagerNativeLayout.UiManagerInitializeDescriptorStoreFileOffset == 0xBFB8CE
    && UiManagerNativeLayout.UiManagerPayloadDescriptorOffset == 0xA08
    && UiManagerNativeLayout.UiManagerInitializeReadyFlagWriteFileOffset == 0xBFB8DC);
var syncInput = new UiManagerStateSyncInput(
    1, 1, 1, 0x58C, 1, 0x588, 0x584, 1, 1, 0x58C);
Check("UI state-sync projection requires both raw gates and preserves the ignored callback result",
    UiManagerNativeLayout.UiManagerStateSyncMethodFileOffset == 0xBFFA90
    && UiManagerNativeLayout.UiManagerStateSyncCallerMethodFileOffset == 0xBFF930
    && UiManagerNativeLayout.UiManagerStateSyncCallerCallSiteFileOffset == 0xBFFA3F
    && UiManagerNativeLayout.UiManagerStateSyncGetCursorInfoIatVirtualAddress == 0x1AC4C24
    && UiManagerNativeLayout.UiManagerStateSyncGetCursorInfoIatFileOffset == 0x16C3824
    && UiManagerNativeLayout.UiManagerStateSyncCursorInfoSize == 20
    && UiManagerNativeLayout.UiManagerStateSyncCursorInfoCbSizeOffset == 0
    && UiManagerNativeLayout.UiManagerStateSyncCursorInfoFlagsOffset == 4
    && UiManagerNativeLayout.UiManagerStateSyncCursorInfoCursorHandleOffset == 8
    && UiManagerNativeLayout.UiManagerStateSyncCursorInfoXOffset == 12
    && UiManagerNativeLayout.UiManagerStateSyncCursorInfoYOffset == 16
    && UiManagerNativeLayout.UiManagerInitializationFlagGetterVtableOffset == 0x04
    && UiManagerNativeLayout.UiManagerStateSyncComponentReceiverOffset == 0x974
    && UiManagerNativeLayout.UiManagerStateSyncComponentVtableOffset == 0x1C
    && !UiManagerNativeLayout.UiManagerStateSyncComponentReturnIsUsed
    && UiManagerNativeLayout.ProjectStateSync(syncInput)
        == new UiManagerStateSyncProjection(1, null)
    && Enumerable.Range(0, byte.MaxValue + 1).All(value =>
        UiManagerNativeLayout.ProjectStateSync(syncInput with { GetterResult = (byte)value }).GateResult
            == (value == 0 ? 0 : 1)
        && UiManagerNativeLayout.ProjectStateSync(syncInput with { ReadyFlag = (byte)value }).GateResult
            == (value == 0 ? 0 : 1)
        && (value != 0
            || (UiManagerNativeLayout.ProjectStateSync(syncInput with { GetterResult = (byte)value }).ComponentCall is null
                && UiManagerNativeLayout.ProjectStateSync(syncInput with { ReadyFlag = (byte)value }).ComponentCall is null))));
Check("UI state-sync selection follows only the recorded +58C/+590, +588, then +584 fallback",
    UiManagerNativeLayout.UiManagerStateSyncField58COffset == 0x58C
    && UiManagerNativeLayout.UiManagerStateSyncField590Offset == 0x590
    && UiManagerNativeLayout.UiManagerStateSyncField588Offset == 0x588
    && UiManagerNativeLayout.UiManagerStateSyncField584Offset == 0x584
    && UiManagerNativeLayout.ProjectStateSync(syncInput with { Field598 = 1, CursorFlags = 0 })
        .ComponentCall == new UiManagerStateSyncCall(1, 0x58C)
    && UiManagerNativeLayout.ProjectStateSync(syncInput with
        {
            Field590 = 0,
            Field598 = 1,
            CursorFlags = 0
        }).ComponentCall == new UiManagerStateSyncCall(1, 0x588)
    && UiManagerNativeLayout.ProjectStateSync(syncInput with
        {
            Field58C = 0,
            Field598 = 1,
            CursorFlags = 0
        }).ComponentCall == new UiManagerStateSyncCall(1, 0x588)
    && UiManagerNativeLayout.ProjectStateSync(syncInput with
        {
            Field590 = 0,
            Field588 = 0,
            Field598 = 1,
            CursorFlags = 0
        }).ComponentCall == new UiManagerStateSyncCall(1, 0x584)
    && UiManagerNativeLayout.ProjectStateSync(syncInput with
        {
            Field590 = 0,
            Field588 = 0,
            Field584 = 0,
            Field598 = 1,
            CursorFlags = 0
        }).ComponentCall == new UiManagerStateSyncCall(1, 0));
Check("UI state-sync nonzero-byte branches and captured cursor comparisons use exact zero/one tests",
    UiManagerNativeLayout.UiManagerStateSyncField598Offset == 0x598
    && Enumerable.Range(0, byte.MaxValue + 1).All(value =>
        UiManagerNativeLayout.ProjectStateSync(syncInput with { Field590 = (byte)value, Field598 = 1, CursorFlags = 0 })
            .ComponentCall == new UiManagerStateSyncCall(1, value == 0 ? 0x588u : 0x58Cu)
        && UiManagerNativeLayout.ProjectStateSync(syncInput with
            {
                Field670 = (byte)value,
                Field598 = 0,
                CursorFlags = 1
            }).ComponentCall == (value == 0
                ? new UiManagerStateSyncCall(1, 0x58Cu)
                : new UiManagerStateSyncCall(0, 0))
        && UiManagerNativeLayout.ProjectStateSync(syncInput with
            {
                Field670 = 1,
                Field598 = (byte)value,
                CursorFlags = 1,
                CursorHandle = 0x58C
            }).ComponentCall == (value == 0
                ? new UiManagerStateSyncCall(0, 0)
                : null)
        && UiManagerNativeLayout.ProjectStateSync(syncInput with
            {
                Field670 = 1,
                Field598 = 0,
                CursorFlags = (uint)value
            }).ComponentCall == (value == 1
                ? new UiManagerStateSyncCall(0, 0)
                : null))
    && UiManagerNativeLayout.ProjectStateSync(syncInput with
        {
            Field670 = 1,
            Field598 = 1,
            CursorFlags = 1,
            CursorHandle = 0x58C
        }).ComponentCall is null
    && UiManagerNativeLayout.ProjectStateSync(syncInput with
        {
            Field670 = 1,
            Field598 = 1,
            CursorFlags = 2,
            CursorHandle = 0x58C
        }).ComponentCall is null
    && UiManagerNativeLayout.ProjectStateSync(syncInput with
        {
            Field670 = 1,
            Field598 = 1,
            CursorFlags = 2,
            CursorHandle = 0x58D
        }).ComponentCall == new UiManagerStateSyncCall(1, 0x58C)
    && UiManagerNativeLayout.ProjectStateSync(syncInput with
        {
            Field670 = 1,
            Field598 = 1,
            CursorFlags = 1,
            CursorHandle = 0x58D
        }).ComponentCall == new UiManagerStateSyncCall(1, 0x58C)
    && UiManagerNativeLayout.ProjectStateSync(syncInput with
        {
            Field670 = 1,
            Field598 = 0,
            CursorFlags = 0x100
        }).ComponentCall is null
    && UiManagerNativeLayout.ProjectStateSync(syncInput with
        {
            Field670 = 1,
            Field598 = 1,
            CursorFlags = 2,
            CursorHandle = 0x8000058C
        }).ComponentCall == new UiManagerStateSyncCall(1, 0x58C));
var angleCorrection = UiManagerAngleCorrectionProjectionRules.Project(
    new UiManagerAngleCorrectionInput(-1f, -1f, 1, 1));
var negativeZeroAngleCorrection = UiManagerAngleCorrectionProjectionRules.Project(
    new UiManagerAngleCorrectionInput(-0f, -0f, 0, 1));
var negativeMultipleAngleCorrection = UiManagerAngleCorrectionProjectionRules.Project(
    new UiManagerAngleCorrectionInput(-UiManagerAngleCorrectionProjectionRules.TwoPi, -0f, 1, 0));
var nanSumAngleCorrection = UiManagerAngleCorrectionProjectionRules.Project(
    new UiManagerAngleCorrectionInput(float.NaN, 0.25f, 1, 1));
var nanRemainderAngleCorrection = UiManagerAngleCorrectionProjectionRules.Project(
    new UiManagerAngleCorrectionInput(-1f, float.NaN, 1, 1));
Check("UI angle correction uses captured fmod results, preserves zero, and keeps the separate byte flag gate",
    UiManagerAngleCorrectionProjectionRules.DerivedStateMethodFileOffset == 0xCCA190
    && UiManagerAngleCorrectionProjectionRules.AngleCorrectionStubFileOffset == 0x159DD00
    && UiManagerAngleCorrectionProjectionRules.AngleCorrectionStubBytes == "FF 25 20 48 AC 01"
    && UiManagerAngleCorrectionProjectionRules.AngleCorrectionIatVirtualAddress == 0x1AC4820
    && UiManagerAngleCorrectionProjectionRules.AngleCorrectionIatFileOffset == 0x16C3420
    && UiManagerAngleCorrectionProjectionRules.AngleCorrectionImport == "MSVCR120.dll!_CIfmod"
    && UiManagerAngleCorrectionProjectionRules.TwoPiConstantVirtualAddress == 0x1C5E1C4
    && UiManagerAngleCorrectionProjectionRules.TwoPiConstantFileOffset == 0x185CDC4
    && UiManagerAngleCorrectionProjectionRules.TwoPiConstantBits == 0x40C90FDB
    && BitConverter.SingleToUInt32Bits(UiManagerAngleCorrectionProjectionRules.TwoPi) == 0x40C90FDB
    && UiManagerAngleCorrectionProjectionRules.ZeroConstantVirtualAddress == 0x1C817A4
    && UiManagerAngleCorrectionProjectionRules.ZeroConstantFileOffset == 0x18803A4
    && UiManagerAngleCorrectionProjectionRules.OneConstantVirtualAddress == 0x1C81BE8
    && UiManagerAngleCorrectionProjectionRules.OneConstantFileOffset == 0x18807E8
    && UiManagerAngleCorrectionProjectionRules.FirstValueInputOffset == 8
    && UiManagerAngleCorrectionProjectionRules.ParentValueInputOffset == 0x14
    && UiManagerAngleCorrectionProjectionRules.ProjectedValueOffset == 0x14
    && UiManagerAngleCorrectionProjectionRules.InputFlagOffset == 1
    && UiManagerAngleCorrectionProjectionRules.ParentFlagOffset == 0x24
    && UiManagerAngleCorrectionProjectionRules.ProjectedFlagOffset == 0x24
    && BitConverter.SingleToUInt32Bits(angleCorrection.ProjectedValue)
        == BitConverter.SingleToUInt32Bits(-1f + UiManagerAngleCorrectionProjectionRules.TwoPi)
    && angleCorrection.ProjectedFlag
    && BitConverter.SingleToUInt32Bits(negativeZeroAngleCorrection.ProjectedValue)
        == 0x80000000
    && !negativeZeroAngleCorrection.ProjectedFlag
    && BitConverter.SingleToUInt32Bits(negativeMultipleAngleCorrection.ProjectedValue)
        == 0x80000000
    && !negativeMultipleAngleCorrection.ProjectedFlag
    && UiManagerAngleCorrectionProjectionRules.Project(
        new UiManagerAngleCorrectionInput(1f, -0.5f, 1, 1)).ProjectedValue == -0.5f
    && nanSumAngleCorrection.ProjectedValue == 0.25f
    && nanSumAngleCorrection.ProjectedFlag
    && float.IsNaN(nanRemainderAngleCorrection.ProjectedValue)
    && nanRemainderAngleCorrection.ProjectedFlag
    && Enumerable.Range(0, byte.MaxValue + 1).All(value =>
        UiManagerAngleCorrectionProjectionRules.Project(
           new UiManagerAngleCorrectionInput(1f, 0f, (byte)value, 1)).ProjectedFlag
               == (value != 0)
        && UiManagerAngleCorrectionProjectionRules.Project(
           new UiManagerAngleCorrectionInput(1f, 0f, 1, (byte)value)).ProjectedFlag
               == (value != 0)));
Check("UI payload descriptor consumer and append helper retain the observed pointer and positional-record layout",
    UiManagerNativeLayout.UiPayloadDescriptorConsumerMethodFileOffset == 0xC95400
    && UiManagerNativeLayout.UiPayloadDescriptorConsumerMethodVirtualAddress == 0x1096000
    && UiManagerNativeLayout.UiPayloadDescriptorConsumerObjectFlagOffset == 0x72C
    && UiManagerNativeLayout.UiManagerGlobalPointerVirtualAddress == 0x1E002B8
    && UiManagerNativeLayout.UiManagerPayloadDescriptorOffset == 0xA08
    && UiManagerNativeLayout.UiPayloadDescriptorAppendMethodFileOffset == 0xC97380
    && UiManagerNativeLayout.UiPayloadDescriptorGrowMethodFileOffset == 0xC978F0
    && UiManagerNativeLayout.UiPayloadDescriptorBeginPointerOffset == 0
    && UiManagerNativeLayout.UiPayloadDescriptorEndPointerOffset == 4
    && UiManagerNativeLayout.UiPayloadDescriptorCapacityPointerOffset == 8
    && UiManagerNativeLayout.UiPayloadDescriptorPositionRecordByteCount == 0x10
    && UiManagerNativeLayout.UiPayloadDescriptorPositionRecordCopiedDwordCount == 4
    && UiManagerNativeLayout.UiPayloadDescriptorPositionRecordEndAdvance == 0x10
    && new[]
    {
        UiManagerNativeLayout.UiPayloadDescriptorPositionField0Offset,
        UiManagerNativeLayout.UiPayloadDescriptorPositionField1Offset,
        UiManagerNativeLayout.UiPayloadDescriptorPositionField2Offset,
        UiManagerNativeLayout.UiPayloadDescriptorPositionField3Offset
    }.SequenceEqual([0x33C, 0x338, 0x340, 0x344]));
Check("UI manager payload-tree drain tests byte +671 but clears both initialization bytes",
    UiManagerNativeLayout.UiManagerShutdownStateWordOffset == 0x671
    && UiManagerNativeLayout.UiManagerShutdownStateWordSize == 2
    && UiManagerNativeLayout.UiManagerPayloadTreeOffset == 0x5E4
    && UiManagerNativeLayout.UiManagerPayloadTreeNodeValueOffset == 0x14
    && Enumerable.Range(0, ushort.MaxValue + 1).All(value =>
        UiManagerNativeLayout.ShouldDrainPayloadTree((byte)(value & 0xFF)) == ((value & 0xFF) != 0))
    && !UiManagerNativeLayout.ShouldDrainPayloadTree(
        new UiManagerInitializationFlagSnapshot(0, 1).PartialInitializationFlag));
Check("UI manager payload-tree cleanup keeps internal close, object free, identity removal, then key release",
    UiManagerNativeLayout.UiManagerShutdownMethodFileOffset == 0xBFA0D0
    && UiManagerNativeLayout.PayloadTreeCleanupSequence.SequenceEqual(
    [
        new UiManagerPayloadCleanupStep("InternalClose", 0xBFA278, 0xC33060),
        new UiManagerPayloadCleanupStep("NativeObjectFree", 0xBFA27F, 0x12C8D60),
        new UiManagerPayloadCleanupStep("IdentityRemoval", 0xBFA28B, 0xBFCF30),
        new UiManagerPayloadCleanupStep("KeyRelease", 0xBFA29C, 0xC09D10)
    ]));
Check("UI manager gated shutdown records the null-gated child +0x24 trace",
    UiManagerNativeLayout.UiManagerShutdownMethodFileOffset == 0xBFA0D0
    && UiManagerNativeLayout.UiManagerShutdownChild24PointerLoadFileOffset == 0xBFAD2A
    && UiManagerNativeLayout.UiManagerShutdownChild24InitialNullTestFileOffset == 0xBFAD37
    && UiManagerNativeLayout.UiManagerShutdownChild24PointerOffset == 0x24
    && UiManagerNativeLayout.UiManagerShutdownChild24ReloadFileOffset == 0xBFAD40
    && UiManagerNativeLayout.UiManagerShutdownChild24SecondNullTestFileOffset == 0xBFAD43
    && UiManagerNativeLayout.UiManagerShutdownChild24SecondNullBranchFileOffset == 0xBFAD45
    && UiManagerNativeLayout.Child24ShutdownSequence.SequenceEqual(
    [
        new UiManagerChild24ShutdownStep("Child24PrimaryCall", 0xBFAD3B, 0xCA7330),
        new UiManagerChild24ShutdownStep("Child24ReleaseHelperCall", 0xBFAD48, 0xBFAF40),
        new UiManagerChild24ShutdownStep("Child150Cleanup", 0xBFAF50, 0xCA6070),
        new UiManagerChild24ShutdownStep("Child150NativeFree", 0xBFAF57, 0x12C8D60),
        new UiManagerChild24ShutdownStep("ChildDependencyVirtualCall", 0xBFAF82, null),
        new UiManagerChild24ShutdownStep("ChildObjectNativeFree", 0xBFAF87, 0x12C8D60)
    ])
    && UiManagerNativeLayout.UiManagerShutdownChild24PointerClearFileOffset == 0xBFAD4D
    && UiManagerNativeLayout.UiManagerShutdownChild24CallsAreNullGated
    && UiManagerNativeLayout.UiManagerChild24ShutdownResetMethodFileOffset == 0xCA7330
    && UiManagerNativeLayout.UiManagerChildShutdownResetPairLoopCount == 8
    && UiManagerNativeLayout.UiManagerChildShutdownResetLoopPointerFieldCount == 16
    && UiManagerNativeLayout.UiManagerChildShutdownResetLoopPointerOffsets.SequenceEqual(
        Enumerable.Range(0, UiManagerNativeLayout.UiManagerChildShutdownResetLoopPointerFieldCount)
            .Select(index => 0x24 + index * 4))
    && UiManagerNativeLayout.UiManagerChildShutdownResetLaterPointerOffsets.SequenceEqual(
        [0x08, 0x14, 0x0C, 0x10, 0x74])
    && UiManagerNativeLayout.UiManagerChild24ShutdownResetFirstLoopCallFileOffset == 0xCA734A
    && UiManagerNativeLayout.UiManagerChild24ShutdownResetFirstLoopStoreFileOffset == 0xCA734D
    && UiManagerNativeLayout.UiManagerChild24ShutdownResetSecondLoopCallFileOffset == 0xCA735D
    && UiManagerNativeLayout.UiManagerChild24ShutdownResetSecondLoopStoreFileOffset == 0xCA7360
    && UiManagerNativeLayout.UiManagerChild24ShutdownResetChild8StoreFileOffset == 0xCA7379
    && UiManagerNativeLayout.UiManagerChild24ShutdownResetChild64FreeCallFileOffset == 0xCA73F2
    && UiManagerNativeLayout.UiManagerChild24ShutdownResetChild68ClearFileOffset == 0xCA73F7
    && UiManagerNativeLayout.UiManagerChild24ShutdownResetReturnFileOffset == 0xCA7401
    && UiManagerNativeLayout.UiManagerChildObjectCleanupFieldOffset == 0x150
    && UiManagerNativeLayout.UiManagerChildReleaseHelperUsesThiscallReceiverInEcx
    && UiManagerNativeLayout.UiManagerChildReleaseHelperCallerPushesUnreadChildCopy
    && UiManagerNativeLayout.UiManagerChildReleaseHelperReturnStackBytes == 4
    && UiManagerNativeLayout.UiManagerChild150CleanupCallSiteFileOffset == 0xBFAF50
    && UiManagerNativeLayout.UiManagerChildDependencyZeroTestFileOffset == 0xBFAF5C
    && UiManagerNativeLayout.UiManagerChildDependencyZeroBranchFileOffset == 0xBFAF60
    && UiManagerNativeLayout.UiManagerChildDependencyLoadFileOffset == 0xBFAF7C
    && UiManagerNativeLayout.UiManagerChildDependencyPointerOffset == 4
    && UiManagerNativeLayout.UiManagerChildDependencyVtableOffset == 8
    && UiManagerNativeLayout.UiManagerChildObjectNativeFreeCallSiteFileOffset == 0xBFAF87
    && UiManagerNativeLayout.UiManagerChildReleaseHelperReturnFileOffset == 0xBFAF90);
Check("UI child backing initialization records the constructor zeros and ordered 0x800-byte allocation stores",
    UiManagerNativeLayout.UiManagerChildBackingConstructorFileOffset == 0xCA6370
    && UiManagerNativeLayout.UiManagerChildBackingInitializeMethodFileOffset == 0xCA66F0
    && UiManagerNativeLayout.UiManagerChildBackingPointerFieldOffset == 0x64
    && UiManagerNativeLayout.UiManagerChildBackingField68Offset == 0x68
    && UiManagerNativeLayout.UiManagerChildBackingField6COffset == 0x6C
    && UiManagerNativeLayout.UiManagerChildBackingAllocationRequestSize == 0x800
    && UiManagerNativeLayout.UiManagerChildBackingField68InitializedValue == 0x20
    && UiManagerNativeLayout.ChildBackingInitializationSequence.SequenceEqual(
    [
        new UiManagerChildBackingInitializationStep("ConstructorField64Zero", 0xCA63D9, null, 0x64, 0),
        new UiManagerChildBackingInitializationStep("ConstructorField68Zero", 0xCA63E0, null, 0x68, 0),
        new UiManagerChildBackingInitializationStep("AllocationSizeArgument", 0xCA6748, null, null, 0x800),
        new UiManagerChildBackingInitializationStep("AllocatorCall", 0xCA674D, 0x12C8A20, null, null),
        new UiManagerChildBackingInitializationStep("BackingPointerStore", 0xCA67A0, null, 0x64, null),
        new UiManagerChildBackingInitializationStep("Field68ValueStore", 0xCA67A3, null, 0x68, 0x20),
        new UiManagerChildBackingInitializationStep("Field6CZeroStore", 0xCA67AA, null, 0x6C, 0)
    ]));
Check("UI-set folder traversal and native file-enumeration imports are recorded",
    UiComponentLoadingNativeLayout.UiSetXmlLoaderFileOffset == 0x30AF50
    && UiComponentLoadingNativeLayout.FolderPathLoaderFileOffset == 0x30AB90
    && UiComponentLoadingNativeLayout.DirectoryEnumerationMethodFileOffset == 0x12B6A00
    && UiComponentLoadingNativeLayout.FolderSearchPatternVirtualAddress == 0x1C1548C
    && UiComponentLoadingNativeLayout.DirectoryEnumerationCallSiteFileOffset == 0x30AC07
    && UiComponentLoadingNativeLayout.FindFirstFileCallSiteFileOffset == 0x12B6AD0
    && UiComponentLoadingNativeLayout.FindNextFileFunctionPointerLoadFileOffset == 0x12B6AEA
    && UiComponentLoadingNativeLayout.FindNextFileCallSiteFileOffset == 0x12B6C06
    && UiComponentLoadingNativeLayout.FindCloseCallSiteFileOffset == 0x12B6C15
    && UiComponentLoadingNativeLayout.DirectoryAttributeTestFileOffset == 0x12B6B94
    && UiComponentLoadingNativeLayout.DirectoryToFileBranchFileOffset == 0x12B6BA9
    && UiComponentLoadingNativeLayout.DirectoryListAppendCallSiteFileOffset == 0x12B6BC5
    && UiComponentLoadingNativeLayout.FileListAppendCallSiteFileOffset == 0x12B6BEA
    && UiComponentLoadingNativeLayout.ChildDirectoryProcessCallSiteFileOffset == 0x30AC35
    && UiComponentLoadingNativeLayout.FileListLoopStartFileOffset == 0x30AC9C
    && UiComponentLoadingNativeLayout.FileLoadCallSiteFileOffset == 0x30AE85
    && UiComponentLoadingNativeLayout.FindFirstFileIatVirtualAddress == 0x1AC473C
    && UiComponentLoadingNativeLayout.FindNextFileIatVirtualAddress == 0x1AC4738
    && UiComponentLoadingNativeLayout.FindCloseIatVirtualAddress == 0x1AC4734
    && UiComponentLoadingNativeLayout.DirectoryAttributeMask == 0x10
    && UiComponentLoadingNativeLayout.ChildDirectoryProcessCallSiteFileOffset
        < UiComponentLoadingNativeLayout.FileListLoopStartFileOffset
    && UiComponentLoadingNativeLayout.UiSetFolderTraversalCallSites.SequenceEqual(
        [0x30B58Bu, 0x30B67Du]));
int retryDelay = 0;
var retryDelays = new List<int>();
for (int i = 0; i < 12; i++)
{
    retryDelay = MatrixConnectionNativeLayout.NextOracleRetryDelaySeconds(retryDelay);
    retryDelays.Add(retryDelay);
}
Check("Oracle retry delay follows the native recurrence and saturates at 300 seconds",
    retryDelays.SequenceEqual([2, 5, 10, 17, 28, 44, 68, 104, 158, 239, 300, 300]));
bool RejectsRetryDelay(int delay)
{
    try
    {
        MatrixConnectionNativeLayout.NextOracleRetryDelaySeconds(delay);
        return false;
    }
    catch (ArgumentOutOfRangeException)
    {
        return true;
    }
}
Check("Unobserved retry-delay inputs are explicitly rejected",
    RejectsRetryDelay(-1) && RejectsRetryDelay(301));
Check("Matrix transport is an overlapped UDP socket with distinct native state",
    MatrixTransportNativeLayout.AllocationSize == 0xDF8
    && MatrixTransportNativeLayout.OpenMethodFileOffset == 0xBD0960
    && MatrixTransportNativeLayout.NativeDatagramSocketType == 2
    && MatrixTransportNativeLayout.NativeUdpProtocol == 17
    && MatrixTransportNativeLayout.NativeOverlappedSocketFlag == 1
    && MatrixTransportNativeLayout.StateOffset == 0x108
    && MatrixTransportNativeLayout.SocketHandleOffset == 0x118
    && MatrixTransportNativeLayout.InitialReceiveBufferSize == 1400
    && MatrixTransportNativeLayout.InitialReceiveCallFileOffset == 0xBD1211
    && MatrixTransportNativeLayout.IoPendingError == 997
    && MatrixTransportNativeLayout.ResolveSocketAddressFamily(1) == 2
    && MatrixTransportNativeLayout.ResolveSocketAddressFamily(2) == 23);
Check("Failed local binds try five requested ports then two ephemeral ports",
    MatrixTransportNativeLayout.GetLocalBindPortAttempts(4500)
        .SequenceEqual(new ushort[] { 4500, 4501, 4502, 4503, 4504, 0, 0 })
    && MatrixTransportNativeLayout.GetLocalBindPortAttempts(0)
        .SequenceEqual(new ushort[] { 0, 0 }));
Check("Local port wrap remains zero rather than retrying port one",
    MatrixTransportNativeLayout.GetLocalBindPortAttempts(65535)
        .SequenceEqual(new ushort[] { 65535, 0, 0, 0, 0, 0, 0 })
    && MatrixTransportNativeLayout.GetLocalBindPortAttempts(65533)
        .SequenceEqual(new ushort[] { 65533, 65534, 65535, 0, 0, 0, 0 }));
bool invalidFamilyRejected = false;
try
{
    MatrixTransportNativeLayout.ResolveSocketAddressFamily(0);
}
catch (ArgumentOutOfRangeException)
{
    invalidFamilyRejected = true;
}
Check("An unobserved socket-family mode is explicitly rejected", invalidFamilyRejected);
var initialPacket = new MatrixInitialPacket(
    0x11, 0x0807060504030201, 0x11223344, 0xAABBCCDD, 0x5566, 0x77, 0x88,
    new byte[] { 0x41, 0x42 }, new byte[] { 0xDE, 0xAD });
Check("Initial matrix application packet preserves native field order and little endian widths",
    initialPacket.EncodeOpenSuccess().SequenceEqual(Convert.FromHexString(
        "1111AA07000000010203040506070844332211DDCCBBAA665577008841420000DEAD"))
    && initialPacket.EncodeOpenSuccess().Length == MatrixInitialPacket.FixedEncodedSize + 4);
var emptyInitialPacket = new MatrixInitialPacket(0, 0, 0, 0, 0, 0, 0,
    ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty);
Check("Empty initial packet is 30 bytes and reserves no wire length prefixes",
    emptyInitialPacket.EncodeOpenSuccess().Length == 30
    && emptyInitialPacket.EncodeOpenSuccess()[0] == 0x11
    && emptyInitialPacket.EncodeOpenSuccess()[2] == 0xAA
    && emptyInitialPacket.EncodeOpenSuccess()[3] == 0x07
    && emptyInitialPacket.EncodeOpenSuccess().AsSpan(4).IndexOfAnyExcept((byte)0) < 0);
Check("Native packet construction, encoding, destruction and transport submission are distinct",
    MatrixInitialPacket.ConstructorFileOffset == 0x338250
    && MatrixInitialPacket.DestructorFileOffset == 0x338120
    && MatrixInitialPacket.EncoderFileOffset == 0x349E60
    && MatrixInitialPacket.TransportSubmissionMethodFileOffset == 0xBD3B20);
var exactFragments = MatrixTransportFragmentPlan.Create(1, 200, 100);
Check("Exact transport segment multiples end in a full final fragment, not an empty one",
    exactFragments.Count == 2
    && exactFragments[0] == new MatrixTransportFragment(0x4868, 0, 100, true)
    && exactFragments[1] == new MatrixTransportFragment(0x4068, 100, 100, false));
var shortFinalFragments = MatrixTransportFragmentPlan.Create(2, 201, 100);
Check("Transport fragment headers pack channel, continuation and size including four header bytes",
    shortFinalFragments.SequenceEqual(new[]
    {
        new MatrixTransportFragment(0x8868, 0, 100, true),
        new MatrixTransportFragment(0x8868, 100, 100, true),
        new MatrixTransportFragment(0x8005, 200, 1, false)
    })
    && MatrixTransportFragmentPlan.MaximumQueuedFragmentSize == 1392
    && MatrixTransportFragmentPlan.MaximumPayloadPerFragment == 1388
    && MatrixTransportFragmentPlan.Create(1, 1388, 1388).Single()
        == new MatrixTransportFragment(0x4570, 0, 1388, false)
    && MatrixTransportFragmentPlan.Create(1, 0, 1388).Count == 0);
bool RejectsFragmentPlan(int channel, int length, int segmentSize)
{
    try
    {
        MatrixTransportFragmentPlan.Create(channel, length, segmentSize);
        return false;
    }
    catch (ArgumentOutOfRangeException)
    {
        return true;
    }
}
Check("Unsupported channels and fragment sizes cannot silently produce transport plans",
    RejectsFragmentPlan(0, 1, 100) && RejectsFragmentPlan(3, 1, 100)
    && RejectsFragmentPlan(1, -1, 100) && RejectsFragmentPlan(1, 1, 0)
    && RejectsFragmentPlan(1, 1, 1389));
uint transportRetry = 0;
var transportRetryIntervals = new List<uint>();
for (int i = 0; i < 9; i++)
{
    transportRetry = GameSocketReliableQueueNativeLayout.NextRetryIntervalMilliseconds(transportRetry);
    transportRetryIntervals.Add(transportRetry);
}
Check("Transport retry interval has its own bounded millisecond policy",
    transportRetryIntervals.SequenceEqual(new uint[] { 250, 375, 562, 843, 1264, 1896, 2500, 2500, 2500 })
    && GameSocketReliableQueueNativeLayout.NextRetryIntervalMilliseconds(249) == 373
    && GameSocketReliableQueueNativeLayout.MaximumOutstandingCount == 512
    && MatrixTransportFragmentPlan.DefaultMaximumSegmentSize == 1388);
Check("Transport send window uses unsigned 16-bit modular sequence distance",
    GameSocketReliableQueueNativeLayout.IsWithinSendWindow(0, 0)
    && GameSocketReliableQueueNativeLayout.IsWithinSendWindow(255, 0)
    && !GameSocketReliableQueueNativeLayout.IsWithinSendWindow(256, 0)
    && GameSocketReliableQueueNativeLayout.IsWithinSendWindow(0, 65535)
    && !GameSocketReliableQueueNativeLayout.IsWithinSendWindow(65535, 0));
bool invalidTransportIntervalRejected = false;
try
{
    GameSocketReliableQueueNativeLayout.NextRetryIntervalMilliseconds(2501);
}
catch (ArgumentOutOfRangeException)
{
    invalidTransportIntervalRejected = true;
}
Check("Unobserved transport retry intervals are explicitly rejected", invalidTransportIntervalRejected);
var fragmentBytes = new byte[] { 0xAA, 0x00, 0x55, 0xFF, 0xBB };
var wireFragment = MatrixTransportFragmentPlan.Create(1, 3, 100).Single() with { PayloadOffset = 1 };
Check("Datagram fragment header and sequence are big endian while application bytes stay in source order",
    MatrixDatagramFragmentEncoder.Encode(wireFragment, 0x1234, fragmentBytes)
        .SequenceEqual(Convert.FromHexString("400712340055FF")));
Check("Attempt markers transform only payload with the literal FF/AA/CC masks",
    MatrixDatagramFragmentEncoder.Encode(wireFragment, 0x1234, fragmentBytes, 1)
        .SequenceEqual(Convert.FromHexString("50071234FFAA00"))
    && MatrixDatagramFragmentEncoder.Encode(wireFragment, 0x1234, fragmentBytes, 2)
        .SequenceEqual(Convert.FromHexString("60071234AAFF55"))
    && MatrixDatagramFragmentEncoder.Encode(wireFragment, 0x1234, fragmentBytes, 3)
        .SequenceEqual(Convert.FromHexString("70071234CC9933"))
    && fragmentBytes.SequenceEqual(new byte[] { 0xAA, 0x00, 0x55, 0xFF, 0xBB }));
bool RejectsWireFragment(MatrixTransportFragment fragment, byte marker = 0)
{
    try
    {
        MatrixDatagramFragmentEncoder.Encode(fragment, 0, fragmentBytes, marker);
        return false;
    }
    catch (ArgumentException)
    {
        return true;
    }
}
Check("Malformed slices, contradictory headers and unsupported attempt markers are rejected",
    RejectsWireFragment(wireFragment with { PayloadOffset = -1 })
    && RejectsWireFragment(wireFragment with { PayloadOffset = 4 })
    && RejectsWireFragment(wireFragment with { InitialHeaderWord = 0x4008 })
    && RejectsWireFragment(wireFragment, 4));
Check("UDP completion port routes by socket key and independent receive/close virtual slots",
    NetworkIoCompletionNativeLayout.NetworkNativeTypeName == "nl::Network"
    && NetworkIoCompletionNativeLayout.PumpThreadNativeTypeName == "nl::Network::PumpThread"
    && MatrixTransportNativeLayout.NativeTypeName == "nl::GameSocket"
    && NetworkIoCompletionNativeLayout.CompletionPortHandleOffset == 0xDC
    && NetworkIoCompletionNativeLayout.EmbeddedPumpThreadOffset == 0x48
    && NetworkIoCompletionNativeLayout.DrainIterationLimit == 100
    && NetworkIoCompletionNativeLayout.IdleWaitMilliseconds == 500
    && NetworkIoCompletionNativeLayout.ResolveSuccessfulCompletionSlot(true) == 0x18
    && NetworkIoCompletionNativeLayout.ResolveSuccessfulCompletionSlot(false) == 0x1C);
Check("Game-socket receive callback uses retained OVERLAPPED and reposts the 1400-byte buffer",
    GameSocketReceiveNativeLayout.OperationAllocationSize == 0xA0
    && GameSocketReceiveNativeLayout.OverlappedOffset == 0
    && GameSocketReceiveNativeLayout.OverlappedSize == 0x14
    && GameSocketReceiveNativeLayout.RetainedOperationPointerOffset == 0xDF4
    && GameSocketReceiveNativeLayout.SenderSockaddrOffset == 0x18
    && GameSocketReceiveNativeLayout.SenderSockaddrLengthOffset == 0x98
    && GameSocketReceiveNativeLayout.SenderSockaddrCapacity == 128
    && GameSocketReceiveNativeLayout.ReceiveCompletionMethodFileOffset == 0xBD2E30
    && GameSocketReceiveNativeLayout.PacketReceiveEntryFileOffset == 0xBD3030
    && GameSocketReceiveNativeLayout.RepostReceiveCallFileOffset == 0xBD2FE2);
Check("UDP outer admission accepts exactly the observed 5..1400 byte range",
    GameSocketPacketAdmission.Classify(new byte[4], 0, true, 1)
        == GameSocketPacketAdmissionOutcome.InvalidLength
    && GameSocketPacketAdmission.Classify(new byte[5], 0, true, 1)
        == GameSocketPacketAdmissionOutcome.AcceptedZeroContext
    && GameSocketPacketAdmission.Classify(new byte[1400], 0, true, 1)
        == GameSocketPacketAdmissionOutcome.AcceptedZeroContext
    && GameSocketPacketAdmission.Classify(new byte[1401], 0, true, 1)
        == GameSocketPacketAdmissionOutcome.InvalidLength);
var admissionPacket = Convert.FromHexString("12345678AB");
Check("UDP outer admission reads a big-endian opaque context word and checks sender specificity/port",
    GameSocketPacketAdmission.Classify(admissionPacket, 0x12345678, true, 4500)
        == GameSocketPacketAdmissionOutcome.AcceptedNonzeroContext
    && GameSocketPacketAdmission.Classify(admissionPacket, 0x78563412, true, 4500)
        == GameSocketPacketAdmissionOutcome.MismatchedContextWord
    && GameSocketPacketAdmission.Classify(admissionPacket, 0x12345678, false, 4500)
        == GameSocketPacketAdmissionOutcome.InvalidSender
    && GameSocketPacketAdmission.Classify(admissionPacket, 0x12345678, true, 0)
        == GameSocketPacketAdmissionOutcome.InvalidSender);
Check("Zero-context handshake requests retain literal magic and exact network-order fields",
    GameSocketHandshake.EncodePoke(0x1234)
        .SequenceEqual(Convert.FromHexString("00000000504F4B4500041234"))
    && GameSocketHandshake.EncodeKiss(0x12345678, 0x9ABC)
        .SequenceEqual(Convert.FromHexString("000000004B495353123456789ABC")));
var heheBody = Convert.FromHexString("4845484512345678");
var huggBody = Convert.FromHexString("485547479ABC1194");
Check("Handshake reply decoding respects native stage and nonzero remote port",
    GameSocketHandshake.DecodeReplyBody(heheBody, 1)
        is GameSocketHandshakeReply.Hehe { ContextCandidate: 0x12345678 }
    && GameSocketHandshake.DecodeReplyBody(huggBody, 2)
        is GameSocketHandshakeReply.Hugg { SequenceSeed: 0x9ABC, RemotePort: 4500 }
    && GameSocketHandshake.DecodeReplyBody(heheBody, 2) is GameSocketHandshakeReply.Rejected
    && GameSocketHandshake.DecodeReplyBody(huggBody, 1) is GameSocketHandshakeReply.Rejected
    && GameSocketHandshake.DecodeReplyBody(Convert.FromHexString("4855474700010000"), 2)
        is GameSocketHandshakeReply.Rejected
    && GameSocketHandshake.DecodeReplyBody(heheBody.AsSpan(0, 7), 1)
        is GameSocketHandshakeReply.Rejected);
Check("Native handshake accepts trailing bytes and ABRT before stage-specific replies",
    GameSocketHandshake.DecodeReplyBody(Convert.FromHexString("4845484512345678FF"), 1)
        is GameSocketHandshakeReply.Hehe { ContextCandidate: 0x12345678 }
    && GameSocketHandshake.DecodeReplyBody(Convert.FromHexString("4142525403"), 99)
        is GameSocketHandshakeReply.Abort { Reason: 3, NativeErrorCode: 10 }
    && GameSocketHandshake.DecodeReplyBody(Convert.FromHexString("41425254FF"), 1)
        is GameSocketHandshakeReply.Abort { Reason: 255, NativeErrorCode: null }
    && GameSocketHandshake.DecodeReplyBody("ABRT"u8, 1)
        is GameSocketHandshakeReply.Rejected);
Check("Native logged socket states distinguish transport Connected from Login connection state",
    Enum.GetNames<NativeGameSocketState>().SequenceEqual(new[]
    {
        "NotOpened", "Initializing", "Transferring", "Connecting", "WaitingContact",
        "Connected", "DisconnectedNotify", "Disconnected", "Destroyed", "Closed"
    })
    && (int)NativeGameSocketState.Connected == 5
    && GameSocketStateAndControlCatalog.CanFinishConnecting(NativeGameSocketState.Connecting, 2)
    && !GameSocketStateAndControlCatalog.CanFinishConnecting(NativeGameSocketState.Connecting, 1)
    && !GameSocketStateAndControlCatalog.CanFinishConnecting(NativeGameSocketState.WaitingContact, 2));
var inspectedFragment = GameSocketChunkHeaderDecoder.Inspect(
    MatrixDatagramFragmentEncoder.Encode(wireFragment, 0x1234, fragmentBytes, 2));
Check("Chunk header inspection recovers network-order channel, marker and declared size",
    inspectedFragment.Outcome == GameSocketChunkHeaderOutcome.Accepted
    && inspectedFragment.Header == new GameSocketChunkHeader(1, 2, false, 7)
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("0002")).Outcome
        == GameSocketChunkHeaderOutcome.Accepted
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("4002")).Outcome
        == GameSocketChunkHeaderOutcome.Accepted);
Check("Chunk header errors preserve exact length and continuation gates",
    GameSocketChunkHeaderDecoder.Inspect(new byte[1]).Outcome
        == GameSocketChunkHeaderOutcome.TruncatedHeader
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("0001")).Outcome
        == GameSocketChunkHeaderOutcome.InvalidLength
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("0571")).Outcome
        == GameSocketChunkHeaderOutcome.InvalidLength
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("4007")).Outcome
        == GameSocketChunkHeaderOutcome.TruncatedChunk
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("C802")).Outcome
        == GameSocketChunkHeaderOutcome.InvalidContinuation
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("1002")).Outcome
        == GameSocketChunkHeaderOutcome.UnsupportedControlMarker);
Check("Header-only gate accepts 2/3-byte nonzero chunks despite the full sequence prefix being 4 bytes",
    GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("4002")).Outcome
        == GameSocketChunkHeaderOutcome.Accepted
    && GameSocketChunkHeaderDecoder.Inspect(Convert.FromHexString("4003AA")).Outcome
        == GameSocketChunkHeaderOutcome.Accepted
    && GameSocketQueuedReceiveNativeLayout.DataChannelSequencePrefixSize == 4);
Check("Context-ready tick transitions follow Initializing/Transferring without inventing timers",
    GameSocketTickNativeLayout.SelectReadyHandshakeState(NativeGameSocketState.Initializing, 1, 0, true)
        == NativeGameSocketState.Transferring
    && GameSocketTickNativeLayout.SelectReadyHandshakeState(NativeGameSocketState.Initializing, 0, 0, true) == null
    && GameSocketTickNativeLayout.SelectReadyHandshakeState(NativeGameSocketState.Transferring, 1, 1, true)
        == NativeGameSocketState.Connecting
    && GameSocketTickNativeLayout.SelectReadyHandshakeState(NativeGameSocketState.Transferring, 1, 1, false)
        == NativeGameSocketState.Connected
    && GameSocketTickNativeLayout.SelectReadyHandshakeState(NativeGameSocketState.Transferring, 1, 0, true) == null
    && GameSocketTickNativeLayout.SelectReadyHandshakeState(NativeGameSocketState.Connected, 1, 1, true) == null);
uint activeQueueRetry = GameSocketReliableQueueNativeLayout.InitialRetryIntervalMilliseconds;
var activeQueueRetryIntervals = new List<uint>();
for (int i = 0; i < 4; i++)
{
    activeQueueRetry = GameSocketReliableQueueNativeLayout.NextRetryIntervalMilliseconds(activeQueueRetry);
    activeQueueRetryIntervals.Add(activeQueueRetry);
}
Check("Activated reliable queues start retry interval at 1000 milliseconds",
    activeQueueRetryIntervals.SequenceEqual(new uint[] { 1500, 2250, 2500, 2500 })
    && GameSocketHandshake.HuggSequenceSeedOffset == 0x228);
Check("Connecting delivers only deferred control, not early application fragments",
    GameSocketQueuedReceiveNativeLayout.SelectRoute(NativeGameSocketState.Connecting, 0, false)
        == GameSocketReceivedFragmentRoute.DeferredControl
    && Enumerable.Range(1, 3).All(channel =>
        GameSocketQueuedReceiveNativeLayout.SelectRoute(NativeGameSocketState.Connecting, channel, false)
            == GameSocketReceivedFragmentRoute.ReleaseWithoutDelivery));
Check("Connected and Destroyed queued receives route all four channels unless suppressed",
    new[] { NativeGameSocketState.Connected, NativeGameSocketState.Destroyed }.All(state =>
        Enumerable.Range(0, 4).All(channel =>
            GameSocketQueuedReceiveNativeLayout.SelectRoute(state, channel, false) == channel switch
            {
                0 => GameSocketReceivedFragmentRoute.DeferredControl,
                1 => GameSocketReceivedFragmentRoute.ReliableChannel1,
                2 => GameSocketReceivedFragmentRoute.ReliableChannel2,
                _ => GameSocketReceivedFragmentRoute.UnreliableChannel3
            }
            && GameSocketQueuedReceiveNativeLayout.SelectRoute(state, channel, true)
                == GameSocketReceivedFragmentRoute.ReleaseWithoutDelivery)));
Check("Owner delivery separates channel 1 from channels 2/3 and preserves their flag",
    GameSocketQueuedReceiveNativeLayout.GetOwnerDelivery(1) == new GameSocketOwnerDelivery(4, null)
    && GameSocketQueuedReceiveNativeLayout.GetOwnerDelivery(2) == new GameSocketOwnerDelivery(8, 1)
    && GameSocketQueuedReceiveNativeLayout.GetOwnerDelivery(3) == new GameSocketOwnerDelivery(8, 0));
Check("Received-queue records and internal event tokens retain their observed native offsets",
    (int)GameSocketReceivedQueueEvent.CloseCompletion == 1
    && (int)GameSocketReceivedQueueEvent.PumpCloseRequest == 2
    && (int)GameSocketReceivedQueueEvent.SenderBindingSucceeded == 3
    && GameSocketQueuedReceiveNativeLayout.ParseNonzeroContextChunksMethodFileOffset == 0xBD3140
    && GameSocketQueuedReceiveNativeLayout.ReceivedQueueOffset == 0x260
    && GameSocketQueuedReceiveNativeLayout.QueueEventPointerThreshold == 0x10
    && GameSocketQueuedReceiveNativeLayout.FragmentHeaderWordOffset == 0
    && GameSocketQueuedReceiveNativeLayout.FragmentSequenceWordOffset == 2
    && GameSocketQueuedReceiveNativeLayout.FragmentBodySliceOffsetFieldOffset == 4
    && GameSocketQueuedReceiveNativeLayout.FragmentBodyObjectOffset == 8
    && GameSocketQueuedReceiveNativeLayout.FragmentArrivalTickOffset == 0x18
    && GameSocketQueuedReceiveNativeLayout.Channel0BodySliceOffset == 2
    && GameSocketQueuedReceiveNativeLayout.DataChannelBodySliceOffset == 4
    && GameSocketQueuedReceiveNativeLayout.Channel1QueueOffset == 0xD6C
    && GameSocketQueuedReceiveNativeLayout.Channel2QueueOffset == 0xD70
    && GameSocketQueuedReceiveNativeLayout.Channel3QueueOffset == 0xD74);
bool invalidHandshakeStateRejected = false;
bool invalidQueuedStateRejected = false;
bool invalidQueuedChannelRejected = false;
bool controlOwnerDeliveryRejected = false;
try { GameSocketTickNativeLayout.SelectReadyHandshakeState((NativeGameSocketState)10, 0, 0, true); }
catch (ArgumentOutOfRangeException) { invalidHandshakeStateRejected = true; }
try { GameSocketQueuedReceiveNativeLayout.SelectRoute(NativeGameSocketState.Initializing, 0, false); }
catch (ArgumentOutOfRangeException) { invalidQueuedStateRejected = true; }
try { GameSocketQueuedReceiveNativeLayout.SelectRoute(NativeGameSocketState.Connected, 4, false); }
catch (ArgumentOutOfRangeException) { invalidQueuedChannelRejected = true; }
try { GameSocketQueuedReceiveNativeLayout.GetOwnerDelivery(0); }
catch (ArgumentOutOfRangeException) { controlOwnerDeliveryRejected = true; }
Check("Invalid tick states and queued delivery domains fail explicitly",
    invalidHandshakeStateRejected && invalidQueuedStateRejected
    && invalidQueuedChannelRejected && controlOwnerDeliveryRejected);
Check("Matrix secondary receiver catalogs all 35 native descriptor IDs without guessing names",
    MatrixServerMessageCatalog.Entries.Count == 35
    && MatrixServerMessageCatalog.Get(34) == new MatrixServerMessageEntry(34, "msg-34", 4, 0x490AA0)
    && MatrixServerMessageCatalog.Get(35).HandlerFileOffset == MatrixMessage35NativeLayout.HandlerFileOffset
    && MatrixServerMessageCatalog.Get(68) == new MatrixServerMessageEntry(68, "msg-68", 0x8C, 0x4925F0));
Check("Message 35 advances connection state 3 to 4 with observed scalar writes, not World mode",
    MatrixMessage35NativeLayout.CaptureScalarUpdate(3, 0x11223344, 0xAABBCCDD, 123456, true, false, true)
        == new MatrixMessage35ObservedScalarUpdate(4, 0x11223344, 0xAABBCCDD, 123456, 5)
    && MatrixMessage35NativeLayout.ConnectionFirstWordOffset == 0x860
    && MatrixMessage35NativeLayout.ConnectionRuntimeFlagsOffset == 0x83D);
bool unknownMessageRejected = false;
bool wrongMessage35StateRejected = false;
try { MatrixServerMessageCatalog.Get(69); }
catch (ArgumentOutOfRangeException) { unknownMessageRejected = true; }
try { MatrixMessage35NativeLayout.CaptureScalarUpdate(4, 0, 0, 0, false, false, false); }
catch (ArgumentOutOfRangeException) { wrongMessage35StateRejected = true; }
Check("Unobserved matrix descriptor IDs and message-35 states fail explicitly",
    unknownMessageRejected && wrongMessage35StateRejected);
var enteredZone = new MatrixEnterZoneObservedFields(0x1234567804, 321, 0x1122334455667788, 0xA7);
Check("EnterZone advances connection state 4 to 6 and preserves identified scalar fields",
    MatrixEnterZoneNativeLayout.CaptureScalarUpdate(4, enteredZone, 1, new(false, false, false, false), false)
        == new MatrixEnterZoneObservedScalarUpdate(6, enteredZone, 1, 30)
    && MatrixServerMessageCatalog.Get(37).HandlerFileOffset == MatrixEnterZoneNativeLayout.HandlerFileOffset);
Check("Disabled preview clears low-three flags before independent force options set them",
    MatrixEnterZoneNativeLayout.CaptureScalarUpdate(4, enteredZone, 1, new(true, false, false, false), false)
        == new MatrixEnterZoneObservedScalarUpdate(6, enteredZone with { PreviewFlags = 0xA0 }, 1, 15)
    && MatrixEnterZoneNativeLayout.CaptureScalarUpdate(4, enteredZone, 1, new(true, true, true, false), true)
        == new MatrixEnterZoneObservedScalarUpdate(6, enteredZone with { PreviewFlags = 0xA5 }, 3, 30)
    && MatrixEnterZoneNativeLayout.CaptureScalarUpdate(4, enteredZone, 3, new(true, false, false, true), false)
        == new MatrixEnterZoneObservedScalarUpdate(6, enteredZone with { PreviewFlags = 0xA2 }, 3, 30));
bool RejectsEnterZone(int state, ulong instanceId)
{
    try
    {
        MatrixEnterZoneNativeLayout.CaptureScalarUpdate(state, enteredZone with { InstanceId = instanceId },
            0, new(false, false, false, false), false);
        return false;
    }
    catch (ArgumentOutOfRangeException) { return true; }
}
Check("EnterZone rejects wrong state, zero instance and sentinel regardless of low byte",
    RejectsEnterZone(3, 1) && RejectsEnterZone(4, 0)
    && RejectsEnterZone(4, 0xFFFFFFFFFFFFFF04) && RejectsEnterZone(4, ulong.MaxValue));
Check("Receive-window classification matches the native shifted-WORD gate across all 65536 distances",
    Enumerable.Range(0, 65536).All(distance =>
    {
        var result = GameSocketReliableReceiveWindow.Classify((ushort)distance, 0);
        bool nativeAcceptedBranch = unchecked((ushort)(distance - 256)) > 0x7EFF;
        var expected = distance < 256 ? GameSocketReceiveSequenceClass.WithinWindow
            : distance >= 0x8000 ? GameSocketReceiveSequenceClass.BeforeWindow
            : GameSocketReceiveSequenceClass.BeyondWindow;
        return result == expected
            && nativeAcceptedBranch == (result != GameSocketReceiveSequenceClass.BeyondWindow);
    }));
Check("Receive-ring indices wrap independently of the 16-bit packet sequence",
    GameSocketReliableReceiveWindow.GetRingSlot(0, 65535, 255) == 0
    && GameSocketReliableReceiveWindow.GetRingSlot(254, 65535, 1) == 0
    && GameSocketReliableReceiveWindow.GetRingSlot(255, 0, 0) == 255
    && GameSocketReliableReceiveWindow.Classify(65535, 0) == GameSocketReceiveSequenceClass.BeforeWindow
    && GameSocketReliableReceiveWindow.Classify(256, 0) == GameSocketReceiveSequenceClass.BeyondWindow);
Check("Reliable receive layout preserves fragment slices, continuation assembly, and decoded channel metadata",
    GameSocketReliableReceiveWindow.FragmentSequenceWordOffset == 2
    && GameSocketReliableReceiveWindow.FragmentBodySliceOffsetFieldOffset == 4
    && GameSocketReliableReceiveWindow.FragmentPayloadHeaderSize == 4
    && GameSocketReliableReceiveWindow.ContinuationFlagMask == 0x0800
    && GameSocketReliableReceiveWindow.ReassemblyBufferOffset == 0x594
    && GameSocketReliableReceiveWindow.ReassemblyLimitSocketOffset == 0x120
    && GameSocketReliableReceiveWindow.DecodedDeliveryQueueOffset == 0x60
    && GameSocketReliableQueueNativeLayout.ChannelOffset == 0x04
    && GameSocketReliableReceiveWindow.DecodedObjectChannelOffset == 0x20
    && GameSocketReliableReceiveWindow.DecodedObjectPayloadSizeOffset == 0x24
    && GameSocketReliableQueueNativeLayout.FailureFlagOffset == 0x1C
    && GameSocketReliableQueueNativeLayout.FailureCodeOffset == 0x20
    && GameSocketReliableQueueNativeLayout.AcknowledgmentPendingFlagOffset == 0x32);
bool invalidReceiveRingHeadRejected = false;
bool outOfWindowRingSlotRejected = false;
try { GameSocketReliableReceiveWindow.GetRingSlot(0, 0, 256); }
catch (ArgumentOutOfRangeException) { invalidReceiveRingHeadRejected = true; }
try { GameSocketReliableReceiveWindow.GetRingSlot(256, 0, 0); }
catch (ArgumentOutOfRangeException) { outOfWindowRingSlotRejected = true; }
Check("Receive-ring slot lookup rejects invalid head indices and out-of-window packets",
    invalidReceiveRingHeadRejected && outOfWindowRingSlotRejected);
Check("Matrix codec pairs map to the native GameSocket queue tag and decoder slots",
    MatrixChannelCodecNativeLayout.GetForChannel(1) == new MatrixChannelCodec(0xB968, 0x33E2E0, 0x73EEE0)
    && MatrixChannelCodecNativeLayout.GetForChannel(2) == new MatrixChannelCodec(0x4C5F, 0x821BA0, 0xC227A0)
    && MatrixChannelCodecNativeLayout.GetForChannel(2) == MatrixChannelCodecNativeLayout.GetForChannel(3)
    && MatrixChannelCodecNativeLayout.GameSocketOpenMethodFileOffset == 0xBD0960
    && MatrixChannelCodecNativeLayout.GameSocketOpenCallSiteFileOffset == 0x4961AD
    && MatrixChannelCodecNativeLayout.ReliableQueueConstructorFileOffset == 0xBE3CE0
    && MatrixChannelCodecNativeLayout.ConnectionChannel1TagOffset == 0x824
    && MatrixChannelCodecNativeLayout.ConnectionChannel1DecoderOffset == 0x828
    && MatrixChannelCodecNativeLayout.ConnectionOtherChannelsTagOffset == 0x82C
    && MatrixChannelCodecNativeLayout.ConnectionOtherChannelsDecoderOffset == 0x830
    && MatrixChannelCodecNativeLayout.GameSocketQueueTagOffset == 0x10
    && MatrixChannelCodecNativeLayout.GameSocketQueueDecoderOffset == 0x14
    && MatrixChannelCodecNativeLayout.GameSocketChannel1QueueOffset == 0xD6C
    && MatrixChannelCodecNativeLayout.GameSocketChannel2QueueOffset == 0xD70
    && MatrixChannelCodecNativeLayout.GameSocketChannel3QueueOffset == 0xD74);
Check("Secondary command dispatcher separates primary-byte, secondary-selector, and zero-return paths",
    MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(0)
        == MatrixSecondaryCommandInputPath.SecondarySelectorTable
    && MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(0xF0)
        == MatrixSecondaryCommandInputPath.SecondarySelectorTable
    && MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(1)
        == MatrixSecondaryCommandInputPath.PrimaryByteRouteMap
    && MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(0x35)
        == MatrixSecondaryCommandInputPath.PrimaryByteRouteMap
    && MatrixSecondaryCommandDispatcherNativeLayout.PrimaryByteMapMaximumLeadingByte == 0x36
    && MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(0x36)
        == MatrixSecondaryCommandInputPath.PrimaryByteRouteMap
    && MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(0x37)
        == MatrixSecondaryCommandInputPath.ReturnZero
    && MatrixSecondaryCommandDispatcherNativeLayout.ClassifyLeadingByte(0xEF)
        == MatrixSecondaryCommandInputPath.ReturnZero
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0) is null
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(1) == 0
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x0E) == 0
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x0F) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x12) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x13) == 2
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x1A) == 3
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x1F) == 3
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x20) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x21) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x22) == 4
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x25) == 4
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x26) == 5
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x28) == 5
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x29) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x33) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x34) == 6
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x35) == 6
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x36) == 1
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryRouteIndex(0x37) is null
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryTargetFileOffset(0) == 0x81CCAD
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryTargetFileOffset(1) == 0x8217D9
    && MatrixSecondaryCommandDispatcherNativeLayout.GetPrimaryTargetFileOffset(6) == 0x81F34A
    && MatrixSecondaryCommandDispatcherNativeLayout.GetSecondarySelectorIndex(0x20) == 0
    && MatrixSecondaryCommandDispatcherNativeLayout.GetSecondarySelectorIndex(0x7A) == 0x5A
    && MatrixSecondaryCommandDispatcherNativeLayout.GetSecondarySelectorIndex(0x7B) is null);
Check("Secondary decoder table snapshot records all 91 selectors",
    MatrixSecondaryDecoderTableNativeLayout.ParserFileOffset == 0x81CC50
    && MatrixSecondaryDecoderTableNativeLayout.SelectorComparisonFileOffset == 0x81F3A8
    && MatrixSecondaryDecoderTableNativeLayout.SelectorJumpFileOffset == 0x81F3B4
    && MatrixSecondaryDecoderTableNativeLayout.TableVirtualAddress == 0xC22630
    && MatrixSecondaryDecoderTableNativeLayout.TableFileOffset == 0x821A30
    && MatrixSecondaryDecoderTableNativeLayout.CaseSetupHelperFileOffset == 0x3571B0
    && MatrixSecondaryDecoderTableNativeLayout.EntryCount == 0x5B
    && MatrixSecondaryDecoderTableNativeLayout.DefaultZeroReturnFileOffset == 0x8217D9
    && MatrixSecondaryDecoderTableNativeLayout.SharedCaseSetupArgument3VirtualAddress == 0x1E60904
    && MatrixSecondaryDecoderTableNativeLayout.RemainingInputBlobVtableVirtualAddress == 0x1C80CC4
    && MatrixSecondaryDecoderTableNativeLayout.RemainingInputBlobReaderVtableSlotOffset == 0x10
    && MatrixSecondaryDecoderTableNativeLayout.RemainingInputBlobStorageObjectFieldOffset == 4
    && MatrixSecondaryDecoderTableNativeLayout.RemainingInputBlobReaderMethodVirtualAddress == 0x16CC5D0
    && MatrixSecondaryDecoderTableNativeLayout.RemainingInputBlobReaderMethodFileOffset == 0x12CB9D0
    && MatrixSecondaryDecoderTableNativeLayout.RemainingInputBlobResizeHelperFileOffset == 0x12CBCB0
    && MatrixSecondaryDecoderTableNativeLayout.RemainingInputBlobCopyRoutineFileOffset == 0x159D8F4
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x20) == 0
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x27) == 7
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x28) == 8
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x2F) == 15
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x30) == 16
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x37) == 23
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x40) == 32
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x47) == 39
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x48) == 40
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x4F) == 47
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x50) == 48
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x57) == 55
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x58) == 56
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x60) == 64
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x67) == 71
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x68) == 72
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x6F) == 79
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x70) == 80
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x77) == 87
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x78) == 88
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x79) == 89
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x7A) == 0x5A
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x1F) is null
    && MatrixSecondaryDecoderTableNativeLayout.GetTableIndex(0x7B) is null
    && MatrixSecondaryDecoderTableNativeLayout.VerifiedEntries.Count == 91
    && MatrixSecondaryDecoderTableNativeLayout.VerifiedEntries
        .Select(entry => entry.Selector)
        .SequenceEqual(Enumerable.Range(0x20, 91).Select(selector => (byte)selector))
    && MatrixSecondaryDecoderTableNativeLayout.VerifiedEntries
        .Select(entry => entry.TableIndex)
        .SequenceEqual(Enumerable.Range(0, 91))
    && MatrixSecondaryDecoderTableNativeLayout.VerifiedEntries
        .Select(entry => entry.TargetVirtualAddress)
        .SequenceEqual(new uint[]
        {
            0xC1FFBB, 0xC20023, 0xC2008B, 0xC200F3,
            0xC2015D, 0xC201C5, 0xC2022F, 0xC20283,
            0xC202EB, 0xC20356, 0xC203BE, 0xC20426,
            0xC2048E, 0xC20502, 0xC2054A, 0xC205BE,
            0xC20628, 0xC20690, 0xC206E4, 0xC2074E,
            0xC207B6, 0xC207FE, 0xC20866, 0xC208CE,
            0xC20936, 0xC2099E, 0xC20A06, 0xC20A6E,
            0xC20AD8, 0xC20B4C, 0xC20BB4, 0xC20C1E,
            0xC20C88, 0xC20CF0, 0xC20D58, 0xC20DC0,
            0xC20E28, 0xC20E90, 0xC20EF8, 0xC20F60,
            0xC20FC8, 0xC21030, 0xC21098, 0xC21100,
            0xC21168, 0xC211D0, 0xC21238, 0xC212A0,
            0xC2130B, 0xC21375, 0xC213BD, 0xC21405,
            0xC21479, 0xC214E1, 0xC21529, 0xC21593,
            0xC215FD, 0xC21645, 0xC21699, 0xC216E1,
            0xC21749, 0xC217B1, 0xC21819, 0xC21881,
            0xC218E9, 0xC21951, 0xC219B9, 0xC21A21,
            0xC21A89, 0xC21AF1, 0xC21B59, 0xC21BA1,
            0xC21C0C, 0xC21C74, 0xC21CDE, 0xC21D46,
            0xC21DAE, 0xC21E16, 0xC21E7E, 0xC21EE6,
            0xC21F4E, 0xC21FB6, 0xC2201E, 0xC22088,
            0xC220F0, 0xC22158, 0xC221CC, 0xC22236,
            0xC2229E, 0xC22306, 0xC2236E
        })
    && MatrixSecondaryDecoderTableNativeLayout.VerifiedEntries
        .All(entry => entry.TargetKind == MatrixSecondaryDecoderTargetKind.MessageCase)
    && MatrixSecondaryDecoderTableNativeLayout.VerifiedEntries
        .All(entry => entry.TargetVirtualAddress == entry.TargetFileOffset + 0x400C00)
    && MatrixSecondaryDecoderTableNativeLayout.VerifiedEntries
        .Select(entry => entry.TargetVirtualAddress)
        .Distinct().Count() == 91
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x27)?.TargetFileOffset == 0x81F683
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x28)?.TargetFileOffset == 0x81F6EB
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x2F)?.TargetFileOffset == 0x81F9BE
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x30)?.TargetFileOffset == 0x81FA28
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x37)?.TargetFileOffset == 0x81FCCE
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x38)?.TargetFileOffset == 0x81FD36
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x3F)?.TargetFileOffset == 0x82001E
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x47)?.TargetFileOffset == 0x820360
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x48)?.TargetFileOffset == 0x8203C8
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x4F)?.TargetFileOffset == 0x8206A0
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x50)?.TargetFileOffset == 0x82070B
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x57)?.TargetFileOffset == 0x820993
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x58)?.TargetFileOffset == 0x8209FD
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x5F)?.TargetFileOffset == 0x820C81
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x60)?.TargetFileOffset == 0x820CE9
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x67)?.TargetFileOffset == 0x820FA1
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x68)?.TargetFileOffset == 0x82100C
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x6F)?.TargetFileOffset == 0x8212E6
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x70)?.TargetFileOffset == 0x82134E
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x77)?.TargetFileOffset == 0x821636
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x78)?.TargetFileOffset == 0x82169E
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x79)?.TargetFileOffset == 0x821706
    && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(0x7A)?.TargetFileOffset == 0x82176E);
Check("Secondary selectors 0x70-0x77 retain native identities and wire layouts",
    EncounterPublicInfoNativeLayout.AllocationSize == 0x40
    && EncounterPublicInfoNativeLayout.ConstructorNativeTypeId == 0x70
    && EncounterPublicInfoNativeLayout.DescriptorAnchorVirtualAddress == 0x1B89A84
    && EncounterPublicInfoNativeLayout.QwordFieldOffset == 0x28
    && EncounterPublicInfoNativeLayout.RemainingInputBlobFieldOffset == 0x30
    && EncounterPublicInfoNativeLayout.QwordWireWidth == 8
    && RequestActiveEncountersResponseNativeLayout.AllocationSize == 0x38
    && RequestActiveEncountersResponseNativeLayout.ConstructorNativeTypeId == 0x71
    && RequestActiveEncountersResponseNativeLayout.DescriptorAnchorVirtualAddress == 0x1B89A8C
    && RequestActiveEncountersResponseNativeLayout.RemainingInputBlobFieldOffset == 0x28
    && ShoppingListInitNativeLayout.AllocationSize == 0x30
    && ShoppingListInitNativeLayout.ConstructorNativeTypeId == 0x72
    && ShoppingListInitNativeLayout.DescriptorAnchorVirtualAddress == 0x1B89A74
    && ShoppingListInitNativeLayout.TreeFieldOffset == 0x28
    && ShoppingListInitNativeLayout.RecordFirstDwordWireWidth == 4
    && ShoppingListInitNativeLayout.RecordSecondDwordWireWidth == 4
    && SetClientDailyInfoNativeLayout.AllocationSize == 0x50
    && SetClientDailyInfoNativeLayout.ConstructorNativeTypeId == 0x73
    && SetClientDailyInfoNativeLayout.DescriptorAnchorVirtualAddress == 0x1B89A7C
    && SetClientDailyInfoNativeLayout.FirstDwordFieldOffset == 0x28
    && SetClientDailyInfoNativeLayout.CountedSequenceFieldOffset == 0x2C
    && SetClientDailyInfoNativeLayout.RecordHelperFileOffset == 0x38EC20
    && GlobalCounterUpdateNativeLayout.AllocationSize == 0x38
    && GlobalCounterUpdateNativeLayout.ConstructorNativeTypeId == 0x74
    && GlobalCounterUpdateNativeLayout.DescriptorAnchorVirtualAddress == 0x1B89A64
    && GlobalCounterUpdateNativeLayout.SharedTextFieldOffset == 0x28
    && GlobalCounterUpdateNativeLayout.QwordFieldOffset == 0x30
    && GlobalCounterMilestoneInfoNativeLayout.AllocationSize == 0x38
    && GlobalCounterMilestoneInfoNativeLayout.ConstructorNativeTypeId == 0x75
    && GlobalCounterMilestoneInfoNativeLayout.DescriptorAnchorVirtualAddress == 0x1B89A6C
    && GlobalCounterMilestoneInfoNativeLayout.FirstCollectionFieldOffset == 0x28
    && GlobalCounterMilestoneInfoNativeLayout.SecondCollectionFieldOffset == 0x30
    && GlobalCounterMilestoneInfoNativeLayout.NestedRecordFirstDwordWireWidth == 4
    && GlobalCounterMilestoneInfoNativeLayout.NestedRecordSecondDwordWireWidth == 4
    && GlobalCounterMilestoneInfoNativeLayout.NestedRecordTrailingDwordWireWidth == 4
    && ChatMessageListNativeLayout.AllocationSize == 0x38
    && ChatMessageListNativeLayout.ConstructorNativeTypeId == 0x76
    && ChatMessageListNativeLayout.DescriptorAnchorVirtualAddress == 0x1B89A54
    && ChatMessageListNativeLayout.RecordVectorFieldOffset == 0x28
    && ChatMessageListNativeLayout.RecordNativeStride == 0x40
    && ChatMessageListNativeLayout.RecordOptionalBlobOffset == 0x14
    && ChatMessageListNativeLayout.OptionalBlobLengthReaderFileOffset == 0x12EE170
    && ChatMessageListNativeLayout.OptionalBlobUpdateFileOffset == 0x5F1920
    && ChatMessageListNativeLayout.OptionalBlobClearFileOffset == 0x49AC30
    && ChatMessageListNativeLayout.RecordOptionalQwordOffset == 0x28
    && ChatMessageListNativeLayout.OptionalQwordPrimitiveReaderFileOffset == 0x356F80
    && ChatMessageListNativeLayout.RecordNestedReaderOffset == 0x38
    && CurrentLoadoutResponseNativeLayout.AllocationSize == 0x50
    && CurrentLoadoutResponseNativeLayout.ConstructorNativeTypeId == 0x77
    && CurrentLoadoutResponseNativeLayout.DescriptorAnchorVirtualAddress == 0x1B89A5C
    && CurrentLoadoutResponseNativeLayout.QwordFieldOffset == 0x28
    && CurrentLoadoutResponseNativeLayout.RecordVectorFieldOffset == 0x44
    && CurrentLoadoutResponseNativeLayout.RecordNativeStride == 0x2E0
    && CurrentLoadoutResponseNativeLayout.RecordOptionalBlockGateOffset == 0x34
    && CurrentLoadoutResponseNativeLayout.RecordOptionalBlockByteOffset == 0x2D8
    && new uint[]
    {
        EncounterPublicInfoNativeLayout.BodyReaderFileOffset,
        RequestActiveEncountersResponseNativeLayout.BodyReaderFileOffset,
        ShoppingListInitNativeLayout.BodyReaderFileOffset,
        SetClientDailyInfoNativeLayout.BodyReaderFileOffset,
        GlobalCounterUpdateNativeLayout.BodyReaderFileOffset,
        GlobalCounterMilestoneInfoNativeLayout.BodyReaderFileOffset,
        ChatMessageListNativeLayout.BodyReaderFileOffset,
        CurrentLoadoutResponseNativeLayout.BodyReaderFileOffset
    }.All(reader => reader != 0));
Check("Secondary selectors 0x78-0x7A preserve RTTI-backed identities and reader layouts",
    VendorProductsResponseNativeLayout.AllocationSize == 0x60
    && VendorProductsResponseNativeLayout.ConstructorNativeTypeId == 0x78
    && VendorProductsResponseNativeLayout.DescriptorAnchorVirtualAddress == 0x1B89A9C
    && VendorProductsResponseNativeLayout.VtableVirtualAddress == 0x1C6DD18
    && VendorProductsResponseNativeLayout.RttiName == ".?AVVendorProductsResponse@firefall@@"
    && VendorProductsResponseNativeLayout.FirstVectorFieldOffset == 0x44
    && VendorProductsResponseNativeLayout.FirstVectorNativeStride == 8
    && VendorProductsResponseNativeLayout.OuterVectorFieldOffset == 0x50
    && VendorProductsResponseNativeLayout.OuterRecordNativeStride == 0x30
    && VendorProductsResponseNativeLayout.FirstNestedVectorNativeStride == 0x18
    && VendorProductsResponseNativeLayout.SecondNestedVectorNativeStride == 8
    && VendorProductsResponseNativeLayout.CountWireWidth == 1
    && VendorProductsResponseNativeLayout.ChunkContinuationCount == 0xFF
    && VendorPurchaseResponseNativeLayout.AllocationSize == 0x48
    && VendorPurchaseResponseNativeLayout.ConstructorNativeTypeId == 0x79
    && VendorPurchaseResponseNativeLayout.DescriptorAnchorVirtualAddress == 0x1B89AA4
    && VendorPurchaseResponseNativeLayout.VtableVirtualAddress == 0x1C6DCE8
    && VendorPurchaseResponseNativeLayout.RttiName == ".?AVVendorPurchaseResponse@firefall@@"
    && VendorPurchaseResponseNativeLayout.ByteFieldOffset == 0x28
    && VendorPurchaseResponseNativeLayout.FirstQwordFieldOffset == 0x30
    && VendorPurchaseResponseNativeLayout.SecondQwordFieldOffset == 0x38
    && VendorPurchaseResponseNativeLayout.DwordFieldOffset == 0x40
    && VendorPurchaseResponseNativeLayout.TextFieldOffset == 0x44
    && ConductorGlobalAnnouncementNativeLayout.AllocationSize == 0x88
    && ConductorGlobalAnnouncementNativeLayout.ConstructorNativeTypeId == 0x7A
    && ConductorGlobalAnnouncementNativeLayout.DescriptorAnchorVirtualAddress == 0x1B89A94
    && ConductorGlobalAnnouncementNativeLayout.VtableVirtualAddress == 0x1C6DCD8
    && ConductorGlobalAnnouncementNativeLayout.RttiName == ".?AVConductorGlobalAnnouncement@firefall@@"
    && ConductorGlobalAnnouncementNativeLayout.NestedBlockReaderFileOffset == 0x83B7A0
    && ConductorGlobalAnnouncementNativeLayout.FirstGatedObjectFieldOffset == 0x3C
    && ConductorGlobalAnnouncementNativeLayout.SecondGatedObjectFieldOffset == 0x4C
    && ConductorGlobalAnnouncementNativeLayout.ThirdTextFieldOffset == 0x80
    && ConductorGlobalAnnouncementNativeLayout.FourthGatedObjectFieldOffset == 0x5C
    && ConductorGlobalAnnouncementNativeLayout.FifthGatedObjectFieldOffset == 0x70
    && ConductorGlobalAnnouncementNativeLayout.TrailingByteFieldOffset == 0x84);
Check("Secondary selectors 0x58-0x5F retain positional wire layouts",
    TrackRecipeNativeLayout.Selector == 0x58
    && TrackRecipeNativeLayout.AllocationSize == 0x30
    && TrackRecipeNativeLayout.ConstructorNativeTypeId == 0x58
    && TrackRecipeNativeLayout.DwordFieldOffset == 0x28
    && TrackRecipeNativeLayout.DwordWireWidth == 4
    && ClearTrackedRecipeNativeLayout.Selector == 0x59
    && ClearTrackedRecipeNativeLayout.AllocationSize == 0x28
    && ClearTrackedRecipeNativeLayout.ConstructorNativeTypeId == 0x59
    && ClearTrackedRecipeNativeLayout.SuccessfulConstructionReturnFileOffset
        == ClearTrackedRecipeNativeLayout.CaseFileOffset + 0x35
    && !ClearTrackedRecipeNativeLayout.HasBodyReader
    && SlotTechNativeLayout.Selector == 0x5A
    && SlotTechNativeLayout.AllocationSize == 0x30
    && SlotTechNativeLayout.ConstructorNativeTypeId == 0x5A
    && SlotTechNativeLayout.DwordFieldOffset == 0x28
    && InteractableStatusChangedNativeLayout.AllocationSize == 0x40
    && InteractableStatusChangedNativeLayout.ConstructorNativeTypeId == 0x5B
    && InteractableStatusChangedNativeLayout.ByteFieldOffset == 0x28
    && InteractableStatusChangedNativeLayout.DwordFieldOffset == 0x2C
    && InteractableStatusChangedNativeLayout.QwordFieldOffset == 0x30
    && InteractableStatusChangedNativeLayout.TrailingByteFieldOffset == 0x38
    && InteractableStatusChangedNativeLayout.QwordWireWidth == 8
    && SendTipMessageNativeLayout.AllocationSize == 0x38
    && SendTipMessageNativeLayout.ConstructorNativeTypeId == 0x5C
    && SendTipMessageNativeLayout.ByteFieldOffset == 0x28
    && SendTipMessageNativeLayout.FirstDwordFieldOffset == 0x2C
    && SendTipMessageNativeLayout.SecondDwordFieldOffset == 0x30
    && SendTipMessageNativeLayout.WordFieldOffset == 0x34
    && SendTipMessageNativeLayout.TrailingByteFieldOffset == 0x36
    && DebugEventSampleNativeLayout.AllocationSize == 0x30
    && DebugEventSampleNativeLayout.ConstructorNativeTypeId == 0x5D
    && DebugEventSampleNativeLayout.FirstWordFieldOffset == 0x28
    && DebugEventSampleNativeLayout.SecondWordFieldOffset == 0x2A
    && DebugEventSampleNativeLayout.FirstByteFieldOffset == 0x2C
    && DebugEventSampleNativeLayout.SecondByteFieldOffset == 0x2D
    && DebugLagPlayerSampleNativeLayout.AllocationSize == 0x38
    && DebugLagPlayerSampleNativeLayout.ConstructorNativeTypeId == 0x5E
    && DebugLagPlayerSampleNativeLayout.WordFieldCount == 5
    && DebugLagPlayerSampleNativeLayout.FifthWordFieldOffset == 0x30
    && DebugLagSimulationSampleNativeLayout.AllocationSize == 0x48
    && DebugLagSimulationSampleNativeLayout.ConstructorNativeTypeId == 0x5F
    && DebugLagSimulationSampleNativeLayout.WordFieldCount == 13
    && DebugLagSimulationSampleNativeLayout.LastWordFieldOffset == 0x40
    && DebugLagSimulationSampleNativeLayout.WordFieldStride == 2);
Check("Secondary selectors 0x60-0x67 preserve their positional table and RTTI identities",
    new (byte Selector, uint CaseFileOffset, uint CaseVirtualAddress, uint TypeId,
        uint Anchor, uint Vtable, string RttiName)[] {
        (DebugLagRaiaSampleNativeLayout.Selector,
            DebugLagRaiaSampleNativeLayout.CaseFileOffset,
            DebugLagRaiaSampleNativeLayout.CaseVirtualAddress,
            DebugLagRaiaSampleNativeLayout.ConstructorNativeTypeId,
            DebugLagRaiaSampleNativeLayout.DescriptorAnchorVirtualAddress,
            DebugLagRaiaSampleNativeLayout.VtableVirtualAddress,
            DebugLagRaiaSampleNativeLayout.RttiName),
        (DebugEncounterVolumesNativeLayout.Selector,
            DebugEncounterVolumesNativeLayout.CaseFileOffset,
            DebugEncounterVolumesNativeLayout.CaseVirtualAddress,
            DebugEncounterVolumesNativeLayout.ConstructorNativeTypeId,
            DebugEncounterVolumesNativeLayout.DescriptorAnchorVirtualAddress,
            DebugEncounterVolumesNativeLayout.VtableVirtualAddress,
            DebugEncounterVolumesNativeLayout.RttiName),
        (TrailNativeLayout.Selector, TrailNativeLayout.CaseFileOffset,
            TrailNativeLayout.CaseVirtualAddress, TrailNativeLayout.ConstructorNativeTypeId,
            TrailNativeLayout.DescriptorAnchorVirtualAddress,
            TrailNativeLayout.VtableVirtualAddress, TrailNativeLayout.RttiName),
        (EncounterDebugNotificationNativeLayout.Selector,
            EncounterDebugNotificationNativeLayout.CaseFileOffset,
            EncounterDebugNotificationNativeLayout.CaseVirtualAddress,
            EncounterDebugNotificationNativeLayout.ConstructorNativeTypeId,
            EncounterDebugNotificationNativeLayout.DescriptorAnchorVirtualAddress,
            EncounterDebugNotificationNativeLayout.VtableVirtualAddress,
            EncounterDebugNotificationNativeLayout.RttiName),
        (EncounterUIScopeInNativeLayout.Selector,
            EncounterUIScopeInNativeLayout.CaseFileOffset,
            EncounterUIScopeInNativeLayout.CaseVirtualAddress,
            EncounterUIScopeInNativeLayout.ConstructorNativeTypeId,
            EncounterUIScopeInNativeLayout.DescriptorAnchorVirtualAddress,
            EncounterUIScopeInNativeLayout.VtableVirtualAddress,
            EncounterUIScopeInNativeLayout.RttiName),
        (EncounterUIUpdateNativeLayout.Selector,
            EncounterUIUpdateNativeLayout.CaseFileOffset,
            EncounterUIUpdateNativeLayout.CaseVirtualAddress,
            EncounterUIUpdateNativeLayout.ConstructorNativeTypeId,
            EncounterUIUpdateNativeLayout.DescriptorAnchorVirtualAddress,
            EncounterUIUpdateNativeLayout.VtableVirtualAddress,
            EncounterUIUpdateNativeLayout.RttiName),
        (EncounterUIScopeOutNativeLayout.Selector,
            EncounterUIScopeOutNativeLayout.CaseFileOffset,
            EncounterUIScopeOutNativeLayout.CaseVirtualAddress,
            EncounterUIScopeOutNativeLayout.ConstructorNativeTypeId,
            EncounterUIScopeOutNativeLayout.DescriptorAnchorVirtualAddress,
            EncounterUIScopeOutNativeLayout.VtableVirtualAddress,
            EncounterUIScopeOutNativeLayout.RttiName),
        (DisplayUiNotificationNativeLayout.Selector,
            DisplayUiNotificationNativeLayout.CaseFileOffset,
            DisplayUiNotificationNativeLayout.CaseVirtualAddress,
            DisplayUiNotificationNativeLayout.ConstructorNativeTypeId,
            DisplayUiNotificationNativeLayout.DescriptorAnchorVirtualAddress,
            DisplayUiNotificationNativeLayout.VtableVirtualAddress,
            DisplayUiNotificationNativeLayout.RttiName)
    }.Select((item, index) =>
        item.Selector == 0x60 + index
        && item.CaseVirtualAddress == item.CaseFileOffset + 0x400C00
        && item.TypeId == item.Selector
        && item.Anchor != 0
        && item.Vtable != 0
        && !string.IsNullOrWhiteSpace(item.RttiName)
        && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(item.Selector)
            ?.TargetFileOffset == item.CaseFileOffset
        && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(item.Selector)
            ?.TargetVirtualAddress == item.CaseVirtualAddress)
        .All(matches => matches)
    && new (uint Helper, uint Constructor, uint Reader, uint ReaderCall,
        uint SetupArgument2, int Allocation, uint Failure)[] {
        (DebugLagRaiaSampleNativeLayout.BaseMetadataConstructorFileOffset,
            DebugLagRaiaSampleNativeLayout.ConstructorFileOffset,
            DebugLagRaiaSampleNativeLayout.BodyReaderFileOffset,
            DebugLagRaiaSampleNativeLayout.BodyReaderCallSiteFileOffset,
            DebugLagRaiaSampleNativeLayout.CaseSetupArgument2VirtualAddress,
            DebugLagRaiaSampleNativeLayout.AllocationSize,
            DebugLagRaiaSampleNativeLayout.AllocationFailureZeroObjectFileOffset),
        (DebugEncounterVolumesNativeLayout.BaseMetadataConstructorFileOffset,
            DebugEncounterVolumesNativeLayout.ConstructorFileOffset,
            DebugEncounterVolumesNativeLayout.BodyReaderFileOffset,
            DebugEncounterVolumesNativeLayout.BodyReaderCallSiteFileOffset,
            DebugEncounterVolumesNativeLayout.CaseSetupArgument2VirtualAddress,
            DebugEncounterVolumesNativeLayout.AllocationSize,
            DebugEncounterVolumesNativeLayout.AllocationFailureZeroObjectFileOffset),
        (TrailNativeLayout.BaseMetadataConstructorFileOffset,
            TrailNativeLayout.ConstructorFileOffset, TrailNativeLayout.BodyReaderFileOffset,
            TrailNativeLayout.BodyReaderCallSiteFileOffset,
            TrailNativeLayout.CaseSetupArgument2VirtualAddress,
            TrailNativeLayout.AllocationSize,
            TrailNativeLayout.AllocationFailureZeroObjectFileOffset),
        (EncounterDebugNotificationNativeLayout.BaseMetadataConstructorFileOffset,
            EncounterDebugNotificationNativeLayout.ConstructorFileOffset,
            EncounterDebugNotificationNativeLayout.BodyReaderFileOffset,
            EncounterDebugNotificationNativeLayout.BodyReaderCallSiteFileOffset,
            EncounterDebugNotificationNativeLayout.CaseSetupArgument2VirtualAddress,
            EncounterDebugNotificationNativeLayout.AllocationSize,
            EncounterDebugNotificationNativeLayout.AllocationFailureZeroObjectFileOffset),
        (EncounterUIScopeInNativeLayout.BaseMetadataConstructorFileOffset,
            EncounterUIScopeInNativeLayout.ConstructorFileOffset,
            EncounterUIScopeInNativeLayout.BodyReaderFileOffset,
            EncounterUIScopeInNativeLayout.BodyReaderCallSiteFileOffset,
            EncounterUIScopeInNativeLayout.CaseSetupArgument2VirtualAddress,
            EncounterUIScopeInNativeLayout.AllocationSize,
            EncounterUIScopeInNativeLayout.AllocationFailureZeroObjectFileOffset),
        (EncounterUIUpdateNativeLayout.BaseMetadataConstructorFileOffset,
            EncounterUIUpdateNativeLayout.ConstructorFileOffset,
            EncounterUIUpdateNativeLayout.BodyReaderFileOffset,
            EncounterUIUpdateNativeLayout.BodyReaderCallSiteFileOffset,
            EncounterUIUpdateNativeLayout.CaseSetupArgument2VirtualAddress,
            EncounterUIUpdateNativeLayout.AllocationSize,
            EncounterUIUpdateNativeLayout.AllocationFailureZeroObjectFileOffset),
        (EncounterUIScopeOutNativeLayout.BaseMetadataConstructorFileOffset,
            EncounterUIScopeOutNativeLayout.ConstructorFileOffset,
            EncounterUIScopeOutNativeLayout.BodyReaderFileOffset,
            EncounterUIScopeOutNativeLayout.SharedBodyContinuationFileOffset,
            EncounterUIScopeOutNativeLayout.CaseSetupArgument2VirtualAddress,
            EncounterUIScopeOutNativeLayout.AllocationSize,
            EncounterUIScopeOutNativeLayout.AllocationFailureZeroObjectFileOffset),
        (DisplayUiNotificationNativeLayout.BaseMetadataConstructorFileOffset,
            DisplayUiNotificationNativeLayout.ConstructorFileOffset,
            DisplayUiNotificationNativeLayout.BodyReaderFileOffset,
            DisplayUiNotificationNativeLayout.BodyReaderCallSiteFileOffset,
            DisplayUiNotificationNativeLayout.CaseSetupArgument2VirtualAddress,
            DisplayUiNotificationNativeLayout.AllocationSize,
            DisplayUiNotificationNativeLayout.AllocationFailureZeroObjectFileOffset)
    }.All(item => item.Helper != 0 && item.Constructor != 0 && item.Reader != 0
        && item.ReaderCall != 0 && item.SetupArgument2 != 0
        && item.Allocation >= MatrixSecondaryMessageHeaderNativeLayout.BodyOffset
        && item.Failure != 0));
Check("Secondary selectors 0x60-0x67 retain observed wire and native body layouts",
    DebugLagRaiaSampleNativeLayout.FirstWordFieldOffset == 0x28
    && DebugLagRaiaSampleNativeLayout.WordFieldCount == 4
    && DebugLagRaiaSampleNativeLayout.WordFieldStride == 2
    && DebugLagRaiaSampleNativeLayout.WordWireWidth == 2
    && DebugEncounterVolumesNativeLayout.FirstVectorBeginOffset == 0x28
    && DebugEncounterVolumesNativeLayout.FirstRecordFloatCount == 4
    && DebugEncounterVolumesNativeLayout.FirstRecordWireWidth == 0x10
    && DebugEncounterVolumesNativeLayout.FirstRecordNativeStride == 0x10
    && DebugEncounterVolumesNativeLayout.SecondRecordWireWidth == 0x40
    && DebugEncounterVolumesNativeLayout.SecondRecordNativeStride == 0x40
    && DebugEncounterVolumesNativeLayout.ThirdRecordWireWidth == 0x40
    && DebugEncounterVolumesNativeLayout.FourthRecordFixedWireWidth == 0x40
    && DebugEncounterVolumesNativeLayout.FourthRecordNativeStride == 0x44
    && DebugEncounterVolumesNativeLayout.CountWireWidth == 1
    && DebugEncounterVolumesNativeLayout.ChunkContinuationCount == 0xFF
    && TrailNativeLayout.DwordFieldOffset == 0x28
    && TrailNativeLayout.ByteFieldOffset == 0x2C
    && TrailNativeLayout.RecordVectorBeginOffset == 0x30
    && TrailNativeLayout.RecordFloatCount == 3
    && TrailNativeLayout.RecordWireWidth == 0x0C
    && TrailNativeLayout.RecordNativeStride == 0x0C
    && EncounterDebugNotificationNativeLayout.QwordFieldOffset == 0x28
    && EncounterDebugNotificationNativeLayout.FirstTextFieldOffset == 0x30
    && EncounterDebugNotificationNativeLayout.SecondTextFieldOffset == 0x34
    && EncounterUIScopeInNativeLayout.LeadingBlobOffset == 0x30
    && EncounterUIScopeInNativeLayout.LeadingBlobLengthPrefixWireWidth == 2
    && EncounterUIScopeInNativeLayout.WordFieldOffset == 0x40
    && EncounterUIScopeInNativeLayout.TreeRootPointerOffset == 0x44
    && EncounterUIScopeInNativeLayout.TreeCountOffset == 0x48
    && EncounterUIScopeInNativeLayout.TrailingBlobOffset == 0x4C
    && EncounterUIScopeInNativeLayout.TaggedEntryNativeStride == 0x18
    && EncounterUIScopeInNativeLayout.TaggedEntryTagOffset == 0
    && EncounterUIScopeInNativeLayout.TagDwordAtEight == 0
    && EncounterUIScopeInNativeLayout.TaggedEntryDwordAtEightOffset == 8
    && EncounterUIScopeInNativeLayout.TaggedEntryTagZeroClearedDwordOffset == 0x0C
    && EncounterUIScopeInNativeLayout.TagDwordAtSixteen == 1
    && EncounterUIScopeInNativeLayout.TagQwordAtEightA == 2
    && EncounterUIScopeInNativeLayout.TagQwordAtEightB == 3
    && EncounterUIScopeInNativeLayout.TagByteAtTwenty == 4
    && EncounterUIScopeInNativeLayout.UnsupportedTag == 5
    && EncounterUIScopeInNativeLayout.TagWordAtTwenty == 6
    && EncounterUIScopeInNativeLayout.TagFlagAndConditionalPayload == 7
    && EncounterUIScopeInNativeLayout.TagSevenDwordPayloadOffset == 0x10
    && EncounterUIScopeInNativeLayout.TagSevenQwordPayloadOffset == 8
    && EncounterUIScopeInNativeLayout.ConditionalDwordFlagMask == 2
    && EncounterUIScopeInNativeLayout.TagSetWordOne == 8
    && EncounterUIScopeInNativeLayout.SpecialZeroTag == 0xFE
    && EncounterUIScopeInNativeLayout.SpecialNoPayloadTag == 0xFF
    && EncounterUIScopeInNativeLayout.SpecialNoPayloadTagHandlerFileOffset == 0x609912
    && EncounterUIUpdateNativeLayout.QwordFieldOffset == 0x28
    && EncounterUIUpdateNativeLayout.LeadingBlobOffset == 0x30
    && EncounterUIUpdateNativeLayout.TrailingBlobOffset == 0x40
    && EncounterUIScopeOutNativeLayout.HasBodyReader
    && EncounterUIScopeOutNativeLayout.SharedBodyContinuationFileOffset == 0x81F72E
    && DisplayUiNotificationNativeLayout.FirstDwordFieldOffset == 0x28
    && DisplayUiNotificationNativeLayout.SecondDwordFieldOffset == 0x2C
    && DisplayUiNotificationNativeLayout.RecordVectorBeginOffset == 0x30
    && DisplayUiNotificationNativeLayout.RecordNativeStride == 0x10
    && DisplayUiNotificationNativeLayout.RecordTextOffset == 4
    && DisplayUiNotificationNativeLayout.RecordDwordOffset == 8
    && DisplayUiNotificationNativeLayout.RecordFloatOffset == 0x0C
    && DisplayUiNotificationNativeLayout.RecordTextTerminatorWireWidth == 1
    && DisplayUiNotificationNativeLayout.TrailingByteFieldOffset == 0x8C
    && DisplayUiNotificationNativeLayout.ChunkContinuationCount == 0xFF);
Check("Secondary selectors 0x68-0x6F record their independent RTTI-backed cases",
    new (byte Selector, uint CaseOffset, uint CaseVa, uint TypeId,
        uint Anchor, uint Vtable, string RttiName)[] {
        (DisplayMoneyBombBannerNativeLayout.Selector,
            DisplayMoneyBombBannerNativeLayout.CaseFileOffset,
            DisplayMoneyBombBannerNativeLayout.CaseVirtualAddress,
            DisplayMoneyBombBannerNativeLayout.ConstructorNativeTypeId,
            DisplayMoneyBombBannerNativeLayout.DescriptorAnchorVirtualAddress,
            DisplayMoneyBombBannerNativeLayout.VtableVirtualAddress,
            DisplayMoneyBombBannerNativeLayout.RttiName),
        (SetPreloadPositionNativeLayout.Selector,
            SetPreloadPositionNativeLayout.CaseFileOffset,
            SetPreloadPositionNativeLayout.CaseVirtualAddress,
            SetPreloadPositionNativeLayout.ConstructorNativeTypeId,
            SetPreloadPositionNativeLayout.DescriptorAnchorVirtualAddress,
            SetPreloadPositionNativeLayout.VtableVirtualAddress,
            SetPreloadPositionNativeLayout.RttiName),
        (PlaySoundIdNativeLayout.Selector, PlaySoundIdNativeLayout.CaseFileOffset,
            PlaySoundIdNativeLayout.CaseVirtualAddress,
            PlaySoundIdNativeLayout.ConstructorNativeTypeId,
            PlaySoundIdNativeLayout.DescriptorAnchorVirtualAddress,
            PlaySoundIdNativeLayout.VtableVirtualAddress,
            PlaySoundIdNativeLayout.RttiName),
        (PlaySoundIdAtLocationNativeLayout.Selector,
            PlaySoundIdAtLocationNativeLayout.CaseFileOffset,
            PlaySoundIdAtLocationNativeLayout.CaseVirtualAddress,
            PlaySoundIdAtLocationNativeLayout.ConstructorNativeTypeId,
            PlaySoundIdAtLocationNativeLayout.DescriptorAnchorVirtualAddress,
            PlaySoundIdAtLocationNativeLayout.VtableVirtualAddress,
            PlaySoundIdAtLocationNativeLayout.RttiName),
        (PlayDialogScriptMessageNativeLayout.Selector,
            PlayDialogScriptMessageNativeLayout.CaseFileOffset,
            PlayDialogScriptMessageNativeLayout.CaseVirtualAddress,
            PlayDialogScriptMessageNativeLayout.ConstructorNativeTypeId,
            PlayDialogScriptMessageNativeLayout.DescriptorAnchorVirtualAddress,
            PlayDialogScriptMessageNativeLayout.VtableVirtualAddress,
            PlayDialogScriptMessageNativeLayout.RttiName),
        (StopDialogScriptMessageNativeLayout.Selector,
            StopDialogScriptMessageNativeLayout.CaseFileOffset,
            StopDialogScriptMessageNativeLayout.CaseVirtualAddress,
            StopDialogScriptMessageNativeLayout.ConstructorNativeTypeId,
            StopDialogScriptMessageNativeLayout.DescriptorAnchorVirtualAddress,
            StopDialogScriptMessageNativeLayout.VtableVirtualAddress,
            StopDialogScriptMessageNativeLayout.RttiName),
        (PingMapNativeLayout.Selector, PingMapNativeLayout.CaseFileOffset,
            PingMapNativeLayout.CaseVirtualAddress,
            PingMapNativeLayout.ConstructorNativeTypeId,
            PingMapNativeLayout.DescriptorAnchorVirtualAddress,
            PingMapNativeLayout.VtableVirtualAddress,
            PingMapNativeLayout.RttiName),
        (PingMapMarkerNativeLayout.Selector, PingMapMarkerNativeLayout.CaseFileOffset,
            PingMapMarkerNativeLayout.CaseVirtualAddress,
            PingMapMarkerNativeLayout.ConstructorNativeTypeId,
            PingMapMarkerNativeLayout.DescriptorAnchorVirtualAddress,
            PingMapMarkerNativeLayout.VtableVirtualAddress,
            PingMapMarkerNativeLayout.RttiName)
    }.Select((item, index) =>
        item.Selector == 0x68 + index
        && item.CaseVa == item.CaseOffset + 0x400C00
        && item.TypeId == item.Selector
        && item.Anchor != 0 && item.Vtable != 0
        && !string.IsNullOrWhiteSpace(item.RttiName)
        && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(item.Selector)
            ?.TargetFileOffset == item.CaseOffset
        && MatrixSecondaryDecoderTableNativeLayout.GetVerifiedEntry(item.Selector)
            ?.TargetVirtualAddress == item.CaseVa)
        .All(matches => matches)
    && new uint[] {
        DisplayMoneyBombBannerNativeLayout.BaseMetadataConstructorFileOffset,
        SetPreloadPositionNativeLayout.BaseMetadataConstructorFileOffset,
        PlaySoundIdNativeLayout.BaseMetadataConstructorFileOffset,
        PlaySoundIdAtLocationNativeLayout.BaseMetadataConstructorFileOffset,
        PlayDialogScriptMessageNativeLayout.BaseMetadataConstructorFileOffset,
        StopDialogScriptMessageNativeLayout.BaseMetadataConstructorFileOffset,
        PingMapNativeLayout.BaseMetadataConstructorFileOffset,
        PingMapMarkerNativeLayout.BaseMetadataConstructorFileOffset
    }.SequenceEqual(new uint[] { 0x825AA0, 0x825B10, 0x825B80, 0x825BF0,
        0x825C60, 0x825CD0, 0x825D40, 0x825DB0 }));
Check("Secondary selectors 0x68-0x6F preserve exact positional wire layouts",
    DisplayMoneyBombBannerNativeLayout.AllocationSize == 0x38
    && DisplayMoneyBombBannerNativeLayout.FirstDwordFieldOffset == 0x28
    && DisplayMoneyBombBannerNativeLayout.SharedTextFieldOffset == 0x2C
    && DisplayMoneyBombBannerNativeLayout.SecondDwordFieldOffset == 0x30
    && DisplayMoneyBombBannerNativeLayout.DwordWireWidth == 4
    && DisplayMoneyBombBannerNativeLayout.SharedTextReaderFileOffset == 0x12EE010
    && DisplayMoneyBombBannerNativeLayout.TextTerminatorWireWidth == 1
    && DisplayMoneyBombBannerNativeLayout.TextChunkSize == 0x100
    && SetPreloadPositionNativeLayout.AllocationSize == 0x68
    && SetPreloadPositionNativeLayout.FirstDwordFieldOffset == 0x28
    && SetPreloadPositionNativeLayout.DwordFieldCount == 16
    && SetPreloadPositionNativeLayout.DwordFieldStride == 4
    && SetPreloadPositionNativeLayout.DwordRows == 4
    && SetPreloadPositionNativeLayout.DwordColumns == 4
    && SetPreloadPositionNativeLayout.LastDwordFieldOffset == 0x64
    && SetPreloadPositionNativeLayout.NominalInputByteCount == 0x40
    && PlaySoundIdNativeLayout.FirstQwordFieldOffset == 0x28
    && PlaySoundIdNativeLayout.SecondQwordFieldOffset == 0x30
    && PlaySoundIdNativeLayout.QwordWireWidth == 8
    && PlaySoundIdNativeLayout.NominalInputByteCount == 0x10
    && PlaySoundIdAtLocationNativeLayout.QwordFieldOffset == 0x28
    && PlaySoundIdAtLocationNativeLayout.FirstDwordFieldOffset == 0x30
    && PlaySoundIdAtLocationNativeLayout.SecondDwordFieldOffset == 0x34
    && PlaySoundIdAtLocationNativeLayout.ThirdDwordFieldOffset == 0x38
    && PlaySoundIdAtLocationNativeLayout.QwordWireWidth == 8
    && PlaySoundIdAtLocationNativeLayout.DwordWireWidth == 4
    && PlaySoundIdAtLocationNativeLayout.NominalInputByteCount == 0x14
    && PlayDialogScriptMessageNativeLayout.DwordFieldOffset == 0x28
    && PlayDialogScriptMessageNativeLayout.VectorBeginOffset == 0x2C
    && PlayDialogScriptMessageNativeLayout.VectorEndOffset == 0x30
    && PlayDialogScriptMessageNativeLayout.VectorCapacityOffset == 0x34
    && PlayDialogScriptMessageNativeLayout.RecordWireWidth == 8
    && PlayDialogScriptMessageNativeLayout.RecordNativeStride == 8
    && PlayDialogScriptMessageNativeLayout.CountWireWidth == 1
    && PlayDialogScriptMessageNativeLayout.ChunkContinuationCount == 0xFF
    && PlayDialogScriptMessageNativeLayout.TerminalCount == 0
    && PlayDialogScriptMessageNativeLayout.MinimumInputByteCount == 5
    && StopDialogScriptMessageNativeLayout.DwordFieldOffset == 0x28
    && StopDialogScriptMessageNativeLayout.VectorBeginOffset == 0x2C
    && StopDialogScriptMessageNativeLayout.VectorEndOffset == 0x30
    && StopDialogScriptMessageNativeLayout.VectorCapacityOffset == 0x34
    && StopDialogScriptMessageNativeLayout.RecordWireWidth == 8
    && StopDialogScriptMessageNativeLayout.RecordNativeStride == 8
    && StopDialogScriptMessageNativeLayout.CountWireWidth == 1
    && StopDialogScriptMessageNativeLayout.ChunkContinuationCount == 0xFF
    && StopDialogScriptMessageNativeLayout.TerminalCount == 0
    && StopDialogScriptMessageNativeLayout.TrailingByteFieldOffset == 0x38
    && StopDialogScriptMessageNativeLayout.TrailingByteWireWidth == 1
    && StopDialogScriptMessageNativeLayout.MinimumInputByteCount == 6
    && PingMapNativeLayout.FirstDwordFieldOffset == 0x28
    && PingMapNativeLayout.SecondDwordFieldOffset == 0x2C
    && PingMapNativeLayout.ThirdDwordFieldOffset == 0x30
    && PingMapNativeLayout.FourthDwordFieldOffset == 0x34
    && PingMapNativeLayout.DwordFieldCount == 4
    && PingMapNativeLayout.DwordWireWidth == 4
    && PingMapNativeLayout.NominalInputByteCount == 0x10
    && PingMapMarkerNativeLayout.QwordFieldOffset == 0x28
    && PingMapMarkerNativeLayout.DwordFieldOffset == 0x30
    && PingMapMarkerNativeLayout.QwordWireWidth == 8
    && PingMapMarkerNativeLayout.DwordWireWidth == 4
    && PingMapMarkerNativeLayout.NominalInputByteCount == 0x0C);
Check("Secondary decoder RTTI and native body layouts retain observed offsets and widths",
    MatrixSecondaryMessageHeaderNativeLayout.BaseMetadataFlagOffset == 0x08
    && MatrixSecondaryMessageHeaderNativeLayout.DescriptorAnchorPointerOffset == 0x0C
    && MatrixSecondaryMessageHeaderNativeLayout.NativeTypeIdOffset == 0x10
    && MatrixSecondaryMessageHeaderNativeLayout.ParserInputFirstDwordCopyOffset == 0x18
    && MatrixSecondaryMessageHeaderNativeLayout.ParserInputSecondDwordCopyOffset == 0x1C
    && MatrixSecondaryMessageHeaderNativeLayout.BodyOffset == 0x28
    && EncounterToUIMessageNativeLayout.AllocationSize == 0x38
    && EncounterToUIMessageNativeLayout.RawQwordFieldOffset == 0x28
    && EncounterToUIMessageNativeLayout.FirstSharedTextFieldOffset == 0x30
    && EncounterToUIMessageNativeLayout.SecondSharedTextFieldOffset == 0x34
    && VotekickInitiatedNativeLayout.AllocationSize == 0x50
    && VotekickInitiatedNativeLayout.DwordFieldOffset == 0x38
    && VotekickInitiatedNativeLayout.SecondRawQwordFieldOffset == 0x40
    && VotekickInitiatedNativeLayout.ByteFieldOffset == 0x48
    && VotekickRespondedNativeLayout.FirstByteFieldOffset == 0x38
    && VotekickRespondedNativeLayout.FirstWordFieldOffset == 0x3A
    && VotekickRespondedNativeLayout.SecondWordFieldOffset == 0x3C
    && ScoreBoardEnableNativeLayout.ByteFieldOffset == 0x28
    && ScoreBoardInitNativeLayout.RecordVectorBeginPointerOffset == 0x28
    && ScoreBoardInitNativeLayout.RecordVectorEndPointerOffset == 0x2C
    && ScoreBoardInitNativeLayout.RecordVectorCapacityPointerOffset == 0x30
    && ScoreBoardInitNativeLayout.RecordStride == ScoreBoardInitRecordNativeLayout.Size
    && ScoreBoardInitNativeLayout.NestedRecordStride == 0x5C
    && ScoreBoardInitRecordNativeLayout.NestedDwordSequenceCount == 18
    && ScoreBoardInitRecordNativeLayout.CountedFourDwordItemsOffset == 0x74
    && ScoreBoardInitRecordNativeLayout.CountedRecordVectorOffset == 0x88
    && ScoreBoardInitRecordNativeLayout.CountedKeyedEntriesOffset == 0x94
    && ScoreBoardSetWinnerNativeLayout.ByteFieldOffset == 0x28
    && !ScoreBoardClearNativeLayout.HasBodyReader
    && ScoreBoardClearNativeLayout.AllocationFailureZeroReturnFileOffset == 0x8217D9
    && ScoreBoardAddPlayerNativeLayout.FirstRawQwordFieldOffset == 0x28
    && ScoreBoardAddPlayerNativeLayout.SharedTextFieldOffset == 0x38
    && ScoreBoardAddPlayerNativeLayout.FirstDwordFieldOffset == 0x3C
    && ScoreBoardAddPlayerNativeLayout.FourthByteFieldOffset == 0x51
    && ScoreBoardAddPlayerNativeLayout.FifthByteFieldOffset == 0x50
    && ScoreBoardRemovePlayerNativeLayout.AllocationSize == 0x30
    && ScoreBoardRemovePlayerNativeLayout.RawQwordFieldOffset == 0x28
    && ScoreBoardUpdatePlayerStatsNativeLayout.AllocationSize == 0x48
    && ScoreBoardUpdatePlayerStatsNativeLayout.RawQwordFieldOffset == 0x28
    && ScoreBoardUpdatePlayerStatsNativeLayout.ConditionalBlobGateWireWidth == 4
    && ScoreBoardUpdatePlayerStatsNativeLayout.ConditionalBlobGateValue == 1
    && ScoreBoardUpdatePlayerStatsNativeLayout.ConditionalBlobOffset == 0x34
    && ScoreBoardUpdatePlayerStatsNativeLayout.BlobLengthPrefixWidth == 2
    && ScoreBoardUpdatePlayerStatsFromStatsNativeLayout.CollectionBeginPointerOffset == 0x30
    && ScoreBoardUpdatePlayerStatsFromStatsNativeLayout.CollectionEndPointerOffset == 0x34
    && ScoreBoardUpdatePlayerStatsFromStatsNativeLayout.CollectionCapacityPointerOffset == 0x38
    && ScoreBoardUpdatePlayerStatsFromStatsNativeLayout.CollectionEntryStride == 0x10
    && ScoreBoardUpdatePlayerStatsFromStatsNativeLayout.CollectionCountWireWidth == 1
    && ScoreBoardUpdatePlayerStatsFromStatsNativeLayout.NestedEntryCountWireWidth == 4
    && ScoreBoardUpdatePlayerStatsFromStatsNativeLayout.ChunkContinuationCount == 0xFF
    && ScoreBoardUpdatePlayerStatsFromStatsNativeLayout.NestedItemTagMinimum == 1
    && ScoreBoardUpdatePlayerStatsFromStatsNativeLayout.NestedItemTagMaximum == 16
    && ScoreBoardUpdatePlayerStatusNativeLayout.RawQwordFieldOffset == 0x28
    && ScoreBoardUpdatePlayerStatusNativeLayout.DwordFieldOffset == 0x34
    && ScoreBoardUpdatePlayerStatusNativeLayout.FourthByteFieldOffset == 0x41
    && ScoreBoardUpdatePlayerStatusNativeLayout.FifthByteFieldOffset == 0x40
    && ScoreBoardUpdatePlayerStatusNativeLayout.ConstructorDefaultWordOffset == 0x40
    && ScoreBoardUpdatePlayerStatusNativeLayout.ConstructorDefaultWordValue == 1
    && MatchLoadingStateNativeLayout.FirstByteFieldOffset == 0x28
    && MatchLoadingStateNativeLayout.CollectionBeginPointerOffset == 0x2C
    && MatchLoadingStateNativeLayout.EntryStride == 0x10
    && MatchLoadingStateNativeLayout.EntryRawQwordOffset == 0
    && MatchLoadingStateNativeLayout.EntrySharedTextOffset == 0x08
    && MatchLoadingStateNativeLayout.EntryByteOffset == 0x0C
    && MatchLoadingStateNativeLayout.CountByteWidth == 1
    && MatchLoadingStateNativeLayout.ChunkContinuationCount == 0xFF
    && MatchEndAckNativeLayout.BodyReaderFileOffset == ScoreBoardRemovePlayerNativeLayout.BodyReaderFileOffset
    && MatchEndAckNativeLayout.SharedBodyContinuationFileOffset == 0x81F72E
    && MatchEndAckNativeLayout.AllocationFailureZeroTargetFileOffset == 0x81F72C
    && ServerProfilerSendNamesNativeLayout.FirstCollectionBeginPointerOffset == 0x28
    && ServerProfilerSendNamesNativeLayout.SecondCollectionBeginPointerOffset == 0x34
    && ServerProfilerSendNamesNativeLayout.EntryStride == 0x08
    && ServerProfilerSendNamesNativeLayout.EntryWordOffset == 0
    && ServerProfilerSendNamesNativeLayout.EntrySharedTextOffset == 4
    && ServerProfilerSendNamesNativeLayout.CountByteWidth == 1
    && ServerProfilerSendNamesNativeLayout.ChunkContinuationCount == 0xFF
    && ServerProfilerSendFrameNativeLayout.RootCollectionBeginPointerOffset == 0x28
    && ServerProfilerSendFrameNativeLayout.RootEntryStride == 0x10
    && ServerProfilerSendFrameNativeLayout.RootEntryChildrenVectorOffset == 0x04
    && ServerProfilerSendFrameNativeLayout.RecursiveEntryStride == 0x20
    && ServerProfilerSendFrameNativeLayout.RecursiveEntryChildrenVectorOffset == 0x04
    && ServerProfilerSendFrameNativeLayout.RecursiveEntryTrailingWordSequenceOffset == 0x10
    && ServerProfilerSendFrameNativeLayout.RecursiveEntryTrailingWordCount == 8
    && ServerProfilerSendFrameNativeLayout.CountByteWidth == 1
    && ServerProfilerSendFrameNativeLayout.ChunkContinuationCount == 0xFF);
Check("Secondary selectors 0x30-0x37 retain RTTI, constructor, and body read layouts",
    TempConsoleMessageNativeLayout.AllocationSize == 0x48
    && TempConsoleMessageNativeLayout.BaseMetadataConstructorFileOffset == 0x823840
    && TempConsoleMessageNativeLayout.FirstSharedTextFieldOffset == 0x28
    && TempConsoleMessageNativeLayout.FourthSharedTextFieldOffset == 0x34
    && TempConsoleMessageNativeLayout.BlobFieldOffset == 0x38
    && TempConsoleMessageNativeLayout.BlobLengthPrefixWidth == 2
    && TempConsoleMessageNativeLayout.ConstructorDefaultBlobValue == 0x1C80CC4
    && ReloadStaticDataNativeLayout.AllocationSize == 0x28
    && !ReloadStaticDataNativeLayout.HasBodyReader
    && ReloadStaticDataNativeLayout.AllocationFailureZeroReturnFileOffset == 0x8217D9
    && EncDebugChatMessageNativeLayout.SharedTextFieldOffset == 0x28
    && EncDebugChatMessageNativeLayout.BodyReaderFileOffset ==
        NpcBehaviorInfoNativeLayout.BodyReaderFileOffset
    && NpcBehaviorInfoNativeLayout.SharedBodyContinuationFileOffset == 0x81FB27
    && EncDebugChatMessageNativeLayout.AllocationFailureZeroObjectFileOffset ==
        NpcBehaviorInfoNativeLayout.AllocationFailureZeroObjectFileOffset
    && SendRadioMessageNativeLayout.FirstDwordFieldOffset == 0x28
    && SendRadioMessageNativeLayout.SecondDwordFieldOffset == 0x2C
    && SendRadioMessageNativeLayout.DwordWireWidth == 4
    && NpcMonitoringLogNativeLayout.FirstSharedTextFieldOffset == 0x28
    && NpcMonitoringLogNativeLayout.ByteFieldOffset == 0x2C
    && NpcMonitoringLogNativeLayout.SecondSharedTextFieldOffset == 0x30
    && NpcNavigationInfoNativeLayout.RawQwordFieldOffset == 0x28
    && NpcNavigationInfoNativeLayout.ThreeDwordVectorBeginPointerOffset == 0x30
    && NpcNavigationInfoNativeLayout.ThreeDwordVectorItemWireWidth == 0x0C
    && NpcNavigationInfoNativeLayout.ThreeDwordVectorNativeStride == 0x0C
    && NpcNavigationInfoNativeLayout.ConditionalVectorBeginPointerOffset == 0x3C
    && NpcNavigationInfoNativeLayout.ConditionalVectorEntryPrefixDwordCount == 7
    && NpcNavigationInfoNativeLayout.ConditionalVectorEntryTailDwordCount == 3
    && NpcNavigationInfoNativeLayout.ConditionalVectorEntryTailByteCount == 1
    && NpcNavigationInfoNativeLayout.ConditionalVectorNativeStride == 0x70
    && NpcNavigationInfoNativeLayout.ConditionalVectorBranchFileOffset == 0x84EF54
    && NpcHostilityDebugInfoNativeLayout.RawQwordFieldOffset == 0x28
    && NpcHostilityDebugInfoNativeLayout.NestedVectorBeginPointerOffset == 0x30
    && NpcHostilityDebugInfoNativeLayout.EntryQwordOffset == 0
    && NpcHostilityDebugInfoNativeLayout.EntryDwordOffset == 8
    && NpcHostilityDebugInfoNativeLayout.EntryWireWidth == 0x0C
    && NpcHostilityDebugInfoNativeLayout.EntryNativeStride == 0x10
    && NpcHostilityDebugInfoNativeLayout.ChunkContinuationCount == 0xFF);
Check("Secondary selectors 0x38-0x3F retain RTTI, positional body shapes, and framing",
    NpcPositionalDebugInfoNativeLayout.Selector == 0x38
    && new byte[]
    {
        NpcPositionalDebugInfoNativeLayout.Selector,
        NpcShapeDebugInfoNativeLayout.Selector,
        NpcVoxelInfoNativeLayout.Selector,
        DebugDrawInfoNativeLayout.Selector,
        NpcDevCmdResponseNativeLayout.Selector,
        DevRequestObjectPositionsNativeLayout.Selector,
        DevRequestSpawnTablesNativeLayout.Selector,
        DevRequestResourceNodeDebugNativeLayout.Selector
    }.SequenceEqual(Enumerable.Range(0x38, 8).Select(value => (byte)value))
    && new uint[]
    {
        NpcPositionalDebugInfoNativeLayout.CaseVirtualAddress,
        NpcShapeDebugInfoNativeLayout.CaseVirtualAddress,
        NpcVoxelInfoNativeLayout.CaseVirtualAddress,
        DebugDrawInfoNativeLayout.CaseVirtualAddress,
        NpcDevCmdResponseNativeLayout.CaseVirtualAddress,
        DevRequestObjectPositionsNativeLayout.CaseVirtualAddress,
        DevRequestSpawnTablesNativeLayout.CaseVirtualAddress,
        DevRequestResourceNodeDebugNativeLayout.CaseVirtualAddress
    }.SequenceEqual(new uint[]
    {
        0xC20936, 0xC2099E, 0xC20A06, 0xC20A6E,
        0xC20AD8, 0xC20B4C, 0xC20BB4, 0xC20C1E
    })
    && new uint[]
    {
        NpcPositionalDebugInfoNativeLayout.CaseFileOffset,
        NpcShapeDebugInfoNativeLayout.CaseFileOffset,
        NpcVoxelInfoNativeLayout.CaseFileOffset,
        DebugDrawInfoNativeLayout.CaseFileOffset,
        NpcDevCmdResponseNativeLayout.CaseFileOffset,
        DevRequestObjectPositionsNativeLayout.CaseFileOffset,
        DevRequestSpawnTablesNativeLayout.CaseFileOffset,
        DevRequestResourceNodeDebugNativeLayout.CaseFileOffset
    }.SequenceEqual(new uint[]
    {
        0x81FD36, 0x81FD9E, 0x81FE06, 0x81FE6E,
        0x81FED8, 0x81FF4C, 0x81FFB4, 0x82001E
    })
    && new uint[]
    {
        NpcPositionalDebugInfoNativeLayout.ConstructorFileOffset,
        NpcShapeDebugInfoNativeLayout.ConstructorFileOffset,
        NpcVoxelInfoNativeLayout.ConstructorFileOffset,
        DebugDrawInfoNativeLayout.ConstructorFileOffset,
        NpcDevCmdResponseNativeLayout.ConstructorFileOffset,
        DevRequestObjectPositionsNativeLayout.ConstructorFileOffset,
        DevRequestSpawnTablesNativeLayout.ConstructorFileOffset,
        DevRequestResourceNodeDebugNativeLayout.ConstructorFileOffset
    }.SequenceEqual(new uint[]
    {
        0x815890, 0x815A80, 0x815C70, 0x815E40,
        0x815F90, 0x8161A0, 0x8163A0, 0x8164E0
    })
    && new uint[]
    {
        NpcPositionalDebugInfoNativeLayout.BaseMetadataConstructorFileOffset,
        NpcShapeDebugInfoNativeLayout.BaseMetadataConstructorFileOffset,
        NpcVoxelInfoNativeLayout.BaseMetadataConstructorFileOffset,
        DebugDrawInfoNativeLayout.BaseMetadataConstructorFileOffset,
        NpcDevCmdResponseNativeLayout.BaseMetadataConstructorFileOffset,
        DevRequestObjectPositionsNativeLayout.BaseMetadataConstructorFileOffset,
        DevRequestSpawnTablesNativeLayout.BaseMetadataConstructorFileOffset,
        DevRequestResourceNodeDebugNativeLayout.BaseMetadataConstructorFileOffset
    }.SequenceEqual(new uint[]
    {
        0x823DB0, 0x823F20, 0x824090, 0x824100,
        0x824170, 0x8241E0, 0x824250, 0x8243C0
    })
    && new uint[]
    {
        NpcPositionalDebugInfoNativeLayout.ConstructorNativeTypeId,
        NpcShapeDebugInfoNativeLayout.ConstructorNativeTypeId,
        NpcVoxelInfoNativeLayout.ConstructorNativeTypeId,
        DebugDrawInfoNativeLayout.ConstructorNativeTypeId,
        NpcDevCmdResponseNativeLayout.ConstructorNativeTypeId,
        DevRequestObjectPositionsNativeLayout.ConstructorNativeTypeId,
        DevRequestSpawnTablesNativeLayout.ConstructorNativeTypeId,
        DevRequestResourceNodeDebugNativeLayout.ConstructorNativeTypeId
    }.SequenceEqual(Enumerable.Range(0x38, 8).Select(value => (uint)value))
    && new uint[]
    {
        NpcPositionalDebugInfoNativeLayout.BodyReaderFileOffset,
        NpcShapeDebugInfoNativeLayout.BodyReaderFileOffset,
        NpcVoxelInfoNativeLayout.BodyReaderFileOffset,
        DebugDrawInfoNativeLayout.BodyReaderFileOffset,
        NpcDevCmdResponseNativeLayout.SharedTextReaderFileOffset,
        DevRequestObjectPositionsNativeLayout.BodyReaderFileOffset,
        DevRequestSpawnTablesNativeLayout.BodyReaderFileOffset,
        DevRequestResourceNodeDebugNativeLayout.BodyReaderFileOffset
    }.SequenceEqual(new uint[]
    {
        0x84D830, 0x84C960, 0x84BC10, 0x609E50,
        0x12EE010, 0x84BA20, 0x84B470, 0x84E230
    })
    && new uint[]
    {
        NpcPositionalDebugInfoNativeLayout.BodyReaderCallSiteFileOffset,
        NpcShapeDebugInfoNativeLayout.BodyReaderCallSiteFileOffset,
        NpcVoxelInfoNativeLayout.BodyReaderCallSiteFileOffset,
        DebugDrawInfoNativeLayout.BodyReaderCallSiteFileOffset,
        NpcDevCmdResponseNativeLayout.FirstSharedTextReaderCallSiteFileOffset,
        DevRequestObjectPositionsNativeLayout.BodyReaderCallSiteFileOffset,
        DevRequestSpawnTablesNativeLayout.BodyReaderCallSiteFileOffset,
        DevRequestResourceNodeDebugNativeLayout.BodyReaderCallSiteFileOffset
    }.SequenceEqual(new uint[]
    {
        0x81FD84, 0x81FDEC, 0x81FE54, 0x81FEBE,
        0x81FF28, 0x81FF9A, 0x820004, 0x82006E
    })
    && new uint[]
    {
        NpcPositionalDebugInfoNativeLayout.AllocationFailureZeroObjectFileOffset,
        NpcShapeDebugInfoNativeLayout.AllocationFailureZeroObjectFileOffset,
        NpcVoxelInfoNativeLayout.AllocationFailureZeroObjectFileOffset,
        DebugDrawInfoNativeLayout.AllocationFailureZeroObjectFileOffset,
        NpcDevCmdResponseNativeLayout.AllocationFailureZeroObjectFileOffset,
        DevRequestObjectPositionsNativeLayout.AllocationFailureZeroObjectFileOffset,
        DevRequestSpawnTablesNativeLayout.AllocationFailureZeroObjectFileOffset,
        DevRequestResourceNodeDebugNativeLayout.AllocationFailureZeroObjectFileOffset
    }.SequenceEqual(new uint[]
    {
        0x81FD77, 0x81FDDF, 0x81FE47, 0x81FEAF,
        0x81FF19, 0x81FF8D, 0x81FFF5, 0x82005F
    })
    && new uint[]
    {
        NpcPositionalDebugInfoNativeLayout.VtableVirtualAddress,
        NpcShapeDebugInfoNativeLayout.VtableVirtualAddress,
        NpcVoxelInfoNativeLayout.VtableVirtualAddress,
        DebugDrawInfoNativeLayout.VtableVirtualAddress,
        NpcDevCmdResponseNativeLayout.VtableVirtualAddress,
        DevRequestObjectPositionsNativeLayout.VtableVirtualAddress,
        DevRequestSpawnTablesNativeLayout.VtableVirtualAddress,
        DevRequestResourceNodeDebugNativeLayout.VtableVirtualAddress
    }.SequenceEqual(new uint[]
    {
        0x1C6E0D8, 0x1C6E0F8, 0x1C6E0E8, 0x1C6E0B8,
        0x1C6E0A8, 0x1C6E0C8, 0x1C6E088, 0x1C6E078
    })
    && NpcPositionalDebugInfoNativeLayout.AllocationSize == 0x48
    && NpcPositionalDebugInfoNativeLayout.RecordVectorBeginPointerOffset == 0x34
    && NpcPositionalDebugInfoNativeLayout.RecordVectorEndPointerOffset == 0x38
    && NpcPositionalDebugInfoNativeLayout.RecordVectorCapacityPointerOffset == 0x3C
    && NpcPositionalDebugInfoNativeLayout.RecordCountWireWidth == 1
    && NpcPositionalDebugInfoNativeLayout.ChunkContinuationCount == 0xFF
    && NpcPositionalDebugInfoNativeLayout.RecordQwordWireWidth == 8
    && NpcPositionalDebugInfoNativeLayout.RecordDwordCount == 8
    && NpcPositionalDebugInfoNativeLayout.RecordWireWidth == 0x28
    && NpcPositionalDebugInfoNativeLayout.RecordNativeStride == 0x28
    && NpcPositionalDebugInfoNativeLayout.TrailingByteFieldOffset == 0x40
    && NpcShapeDebugInfoNativeLayout.RttiName == ".?AVNpcShapeDebugInfo@firefall@@"
    && NpcShapeDebugInfoNativeLayout.RecordLeadingByteOffset == 0
    && NpcShapeDebugInfoNativeLayout.RecordFixedDwordBlockOffset == 4
    && NpcShapeDebugInfoNativeLayout.RecordFixedDwordBlockCount == 16
    && NpcShapeDebugInfoNativeLayout.RecordTrailingDwordBlockOffset == 0x44
    && NpcShapeDebugInfoNativeLayout.RecordTrailingDwordCount == 4
    && NpcShapeDebugInfoNativeLayout.RecordTrailingByteOffset == 0x54
    && NpcShapeDebugInfoNativeLayout.RecordWireFixedWidth == 0x52
    && NpcShapeDebugInfoNativeLayout.RecordNativeStride == 0x58
    && NpcVoxelInfoNativeLayout.RawQwordFieldOffset == 0x28
    && NpcVoxelInfoNativeLayout.ThreeDwordVectorBeginPointerOffset == 0x30
    && NpcVoxelInfoNativeLayout.DwordVectorBeginPointerOffset == 0x3C
    && NpcVoxelInfoNativeLayout.ThreeDwordItemWireWidth == 0x0C
    && NpcVoxelInfoNativeLayout.ThreeDwordItemNativeStride == 0x0C
    && NpcVoxelInfoNativeLayout.DwordItemWireWidth == 4
    && NpcVoxelInfoNativeLayout.DwordItemNativeStride == 4
    && NpcVoxelInfoNativeLayout.CountWireWidth == 1
    && NpcVoxelInfoNativeLayout.ChunkContinuationCount == 0xFF
    && NpcVoxelInfoNativeLayout.TrailingByteFieldOffset == 0x48
    && DebugDrawInfoNativeLayout.ByteVectorBeginPointerOffset == 0x28
    && DebugDrawInfoNativeLayout.ByteVectorEndPointerOffset == 0x2C
    && DebugDrawInfoNativeLayout.ByteVectorCapacityPointerOffset == 0x30
    && DebugDrawInfoNativeLayout.CountWireWidth == 1
    && DebugDrawInfoNativeLayout.ItemWireWidth == 1
    && DebugDrawInfoNativeLayout.ItemNativeStride == 1
    && DebugDrawInfoNativeLayout.ChunkContinuationCount == 0xFF
    && NpcDevCmdResponseNativeLayout.FirstSharedTextFieldOffset == 0x28
    && NpcDevCmdResponseNativeLayout.SecondSharedTextFieldOffset == 0x2C
    && DevRequestObjectPositionsNativeLayout.FirstByteFieldOffset == 0x28
    && DevRequestObjectPositionsNativeLayout.SecondByteFieldOffset == 0x29
    && DevRequestObjectPositionsNativeLayout.ThreeDwordVectorBeginPointerOffset == 0x2C
    && DevRequestObjectPositionsNativeLayout.FirstDwordVectorBeginPointerOffset == 0x38
    && DevRequestObjectPositionsNativeLayout.SecondDwordVectorBeginPointerOffset == 0x44
    && DevRequestObjectPositionsNativeLayout.ThirdDwordVectorBeginPointerOffset == 0x50
    && DevRequestObjectPositionsNativeLayout.CountWireWidth == 1
    && DevRequestObjectPositionsNativeLayout.ChunkContinuationCount == 0xFF
    && DevRequestObjectPositionsNativeLayout.ThreeDwordItemWireWidth == 0x0C
    && DevRequestObjectPositionsNativeLayout.ThreeDwordItemNativeStride == 0x0C
    && DevRequestObjectPositionsNativeLayout.DwordItemWireWidth == 4
    && DevRequestObjectPositionsNativeLayout.DwordItemNativeStride == 4
    && DevRequestSpawnTablesNativeLayout.RecordVectorBeginPointerOffset == 0x28
    && DevRequestSpawnTablesNativeLayout.RecordVectorEndPointerOffset == 0x2C
    && DevRequestSpawnTablesNativeLayout.RecordVectorCapacityPointerOffset == 0x30
    && DevRequestSpawnTablesNativeLayout.CountWireWidth == 1
    && DevRequestSpawnTablesNativeLayout.ChunkContinuationCount == 0xFF
    && DevRequestSpawnTablesNativeLayout.RecordFirstSharedTextOffset == 0
    && DevRequestSpawnTablesNativeLayout.RecordDwordCount == 5
    && DevRequestSpawnTablesNativeLayout.RecordFirstDwordOffset == 4
    && DevRequestSpawnTablesNativeLayout.RecordByteOffset == 0x18
    && DevRequestSpawnTablesNativeLayout.RecordSecondSharedTextOffset == 0x1C
    && DevRequestSpawnTablesNativeLayout.RecordNativeStride == 0x20
    && DevRequestResourceNodeDebugNativeLayout.RecordVectorBeginPointerOffset == 0x28
    && DevRequestResourceNodeDebugNativeLayout.RecordVectorEndPointerOffset == 0x2C
    && DevRequestResourceNodeDebugNativeLayout.RecordVectorCapacityPointerOffset == 0x30
    && DevRequestResourceNodeDebugNativeLayout.CountWireWidth == 1
    && DevRequestResourceNodeDebugNativeLayout.ChunkContinuationCount == 0xFF
    && DevRequestResourceNodeDebugNativeLayout.RecordDwordCount == 3
    && DevRequestResourceNodeDebugNativeLayout.RecordWireWidth == 0x0C
    && DevRequestResourceNodeDebugNativeLayout.RecordNativeStride == 0x0C
    && new uint[]
    {
        NpcPositionalDebugInfoNativeLayout.CaseLocalDwordValue,
        NpcShapeDebugInfoNativeLayout.CaseLocalDwordValue,
        NpcVoxelInfoNativeLayout.CaseLocalDwordValue,
        DebugDrawInfoNativeLayout.CaseLocalDwordValue,
        NpcDevCmdResponseNativeLayout.CaseLocalDwordValue,
        DevRequestObjectPositionsNativeLayout.CaseLocalDwordValue,
        DevRequestSpawnTablesNativeLayout.CaseLocalDwordValue,
        DevRequestResourceNodeDebugNativeLayout.CaseLocalDwordValue
    }.SequenceEqual(Enumerable.Range(0x18, 8).Select(value => (uint)value)));
Check("Secondary selectors 0x40-0x47 retain RTTI, constructor, and native body layouts",
    new byte[]
    {
        MissionObjectiveUpdatedNativeLayout.Selector,
        MissionStatusChangedNativeLayout.Selector,
        MissionReturnToChangedNativeLayout.Selector,
        MissionsAvailableNativeLayout.Selector,
        MissionActivationAckNativeLayout.Selector,
        BountyStatusChangedNativeLayout.Selector,
        BountyAbortAckNativeLayout.Selector,
        BountyActivationAckNativeLayout.Selector
    }.SequenceEqual(Enumerable.Range(0x40, 8).Select(value => (byte)value))
    && new uint[]
    {
        MissionObjectiveUpdatedNativeLayout.CaseVirtualAddress,
        MissionStatusChangedNativeLayout.CaseVirtualAddress,
        MissionReturnToChangedNativeLayout.CaseVirtualAddress,
        MissionsAvailableNativeLayout.CaseVirtualAddress,
        MissionActivationAckNativeLayout.CaseVirtualAddress,
        BountyStatusChangedNativeLayout.CaseVirtualAddress,
        BountyAbortAckNativeLayout.CaseVirtualAddress,
        BountyActivationAckNativeLayout.CaseVirtualAddress
    }.SequenceEqual(new uint[]
    {
        0xC20C88, 0xC20CF0, 0xC20D58, 0xC20DC0,
        0xC20E28, 0xC20E90, 0xC20EF8, 0xC20F60
    })
    && new uint[]
    {
        MissionObjectiveUpdatedNativeLayout.CaseFileOffset,
        MissionStatusChangedNativeLayout.CaseFileOffset,
        MissionReturnToChangedNativeLayout.CaseFileOffset,
        MissionsAvailableNativeLayout.CaseFileOffset,
        MissionActivationAckNativeLayout.CaseFileOffset,
        BountyStatusChangedNativeLayout.CaseFileOffset,
        BountyAbortAckNativeLayout.CaseFileOffset,
        BountyActivationAckNativeLayout.CaseFileOffset
    }.SequenceEqual(new uint[]
    {
        0x820088, 0x8200F0, 0x820158, 0x8201C0,
        0x820228, 0x820290, 0x8202F8, 0x820360
    })
    && new uint[]
    {
        MissionObjectiveUpdatedNativeLayout.AllocationSize,
        MissionStatusChangedNativeLayout.AllocationSize,
        MissionReturnToChangedNativeLayout.AllocationSize,
        MissionsAvailableNativeLayout.AllocationSize,
        MissionActivationAckNativeLayout.AllocationSize,
        BountyStatusChangedNativeLayout.AllocationSize,
        BountyAbortAckNativeLayout.AllocationSize,
        BountyActivationAckNativeLayout.AllocationSize
    }.SequenceEqual(new uint[] { 0x48, 0x38, 0x30, 0x40, 0x30, 0x30, 0x30, 0x50 })
    && new uint[]
    {
        MissionObjectiveUpdatedNativeLayout.ConstructorNativeTypeId,
        MissionStatusChangedNativeLayout.ConstructorNativeTypeId,
        MissionReturnToChangedNativeLayout.ConstructorNativeTypeId,
        MissionsAvailableNativeLayout.ConstructorNativeTypeId,
        MissionActivationAckNativeLayout.ConstructorNativeTypeId,
        BountyStatusChangedNativeLayout.ConstructorNativeTypeId,
        BountyAbortAckNativeLayout.ConstructorNativeTypeId,
        BountyActivationAckNativeLayout.ConstructorNativeTypeId
    }.SequenceEqual(Enumerable.Range(0x40, 8).Select(value => (uint)value))
    && new uint[]
    {
        MissionObjectiveUpdatedNativeLayout.CaseLocalDwordValue,
        MissionStatusChangedNativeLayout.CaseLocalDwordValue,
        MissionReturnToChangedNativeLayout.CaseLocalDwordValue,
        MissionsAvailableNativeLayout.CaseLocalDwordValue,
        MissionActivationAckNativeLayout.CaseLocalDwordValue,
        BountyStatusChangedNativeLayout.CaseLocalDwordValue,
        BountyAbortAckNativeLayout.CaseLocalDwordValue,
        BountyActivationAckNativeLayout.CaseLocalDwordValue
    }.SequenceEqual(Enumerable.Range(0x20, 8).Select(value => (uint)value))
    && new uint[]
    {
        MissionObjectiveUpdatedNativeLayout.BaseMetadataConstructorFileOffset,
        MissionStatusChangedNativeLayout.BaseMetadataConstructorFileOffset,
        MissionReturnToChangedNativeLayout.BaseMetadataConstructorFileOffset,
        MissionsAvailableNativeLayout.BaseMetadataConstructorFileOffset,
        MissionActivationAckNativeLayout.BaseMetadataConstructorFileOffset,
        BountyStatusChangedNativeLayout.BaseMetadataConstructorFileOffset,
        BountyAbortAckNativeLayout.BaseMetadataConstructorFileOffset,
        BountyActivationAckNativeLayout.BaseMetadataConstructorFileOffset
    }.SequenceEqual(new uint[]
    {
        0x824430, 0x8244A0, 0x824510, 0x824580,
        0x8246E0, 0x824750, 0x8247C0, 0x824830
    })
    && new uint[]
    {
        MissionObjectiveUpdatedNativeLayout.BodyReaderFileOffset,
        MissionStatusChangedNativeLayout.BodyReaderFileOffset,
        MissionReturnToChangedNativeLayout.BodyReaderFileOffset,
        MissionsAvailableNativeLayout.BodyReaderFileOffset,
        MissionActivationAckNativeLayout.BodyReaderFileOffset,
        BountyStatusChangedNativeLayout.BodyReaderFileOffset,
        BountyAbortAckNativeLayout.BodyReaderFileOffset,
        BountyActivationAckNativeLayout.BodyReaderFileOffset
    }.SequenceEqual(new uint[]
    {
        0x84A7F0, 0x84A640, 0x84A520, 0x84A480,
        0x849AB0, 0x8499F0, 0x8498D0, 0x849790
    })
    && new uint[]
    {
        MissionObjectiveUpdatedNativeLayout.VtableVirtualAddress,
        MissionStatusChangedNativeLayout.VtableVirtualAddress,
        MissionReturnToChangedNativeLayout.VtableVirtualAddress,
        MissionsAvailableNativeLayout.VtableVirtualAddress,
        MissionActivationAckNativeLayout.VtableVirtualAddress,
        BountyStatusChangedNativeLayout.VtableVirtualAddress,
        BountyAbortAckNativeLayout.VtableVirtualAddress,
        BountyActivationAckNativeLayout.VtableVirtualAddress
    }.SequenceEqual(new uint[]
    {
        0x1C6E098, 0x1C6E048, 0x1C6E038, 0x1C6E068,
        0x1C6E058, 0x1C6E008, 0x1C6E028, 0x1C6E018
    })
    && MissionObjectiveUpdatedNativeLayout.RttiName == ".?AVMissionObjectiveUpdated@firefall@@"
    && MissionStatusChangedNativeLayout.RttiName == ".?AVMissionStatusChanged@firefall@@"
    && MissionReturnToChangedNativeLayout.RttiName == ".?AVMissionReturnToChanged@firefall@@"
    && MissionsAvailableNativeLayout.RttiName == ".?AVMissionsAvailable@firefall@@"
    && BountyStatusChangedNativeLayout.RttiName == ".?AVBountyStatusChanged@firefall@@"
    && MissionActivationAckNativeLayout.RttiName == ".?AVMissionActivationAck@firefall@@"
    && BountyAbortAckNativeLayout.RttiName == ".?AVBountyAbortAck@firefall@@"
    && BountyActivationAckNativeLayout.RttiName == ".?AVBountyActivationAck@firefall@@"
    && MissionObjectiveUpdatedNativeLayout.FirstSharedTextFieldOffset == 0x34
    && MissionObjectiveUpdatedNativeLayout.SecondSharedTextFieldOffset == 0x38
    && MissionObjectiveUpdatedNativeLayout.ByteFieldOffset == 0x3C
    && MissionStatusChangedNativeLayout.RawQwordFieldOffset == 0x28
    && MissionStatusChangedNativeLayout.ByteFieldOffset == 0x30
    && MissionReturnToChangedNativeLayout.DwordFieldOffset == 0x28
    && MissionReturnToChangedNativeLayout.ByteFieldOffset == 0x2C
    && MissionsAvailableNativeLayout.RecordWireWidth == 5
    && MissionsAvailableNativeLayout.RecordNativeStride == 8
    && MissionsAvailableNativeLayout.CountWireWidth == 1
    && MissionsAvailableNativeLayout.ChunkContinuationCount == 0xFF
    && MissionsAvailableNativeLayout.RawQwordFieldOffset == 0x38
    && MissionActivationAckNativeLayout.SecondByteFieldOffset == 0x2D
    && BountyStatusChangedNativeLayout.ByteFieldOffset == 0x2C
    && BountyAbortAckNativeLayout.SecondByteFieldOffset == 0x2D
    && BountyActivationAckNativeLayout.FirstTrailingDwordFieldOffset == 0x30
    && BountyActivationAckNativeLayout.SecondTrailingDwordFieldOffset == 0x34
    && BountyActivationAckNativeLayout.TrailingByteFieldOffset == 0x38
    && BountyActivationAckNativeLayout.QwordFieldOffset == 0x40
    && BountyActivationAckNativeLayout.SharedTextFieldOffset == 0x48);
Check("Secondary selectors 0x48-0x4F retain RTTI and native body layouts",
    new byte[]
    {
        BountyListActiveAckNativeLayout.Selector,
        BountyListActiveDetailsAckNativeLayout.Selector,
        BountyListAvailableAckNativeLayout.Selector,
        BountyClearAckNativeLayout.Selector,
        BountyClearPreviousAckNativeLayout.Selector,
        BountyListPreviousAckNativeLayout.Selector,
        BountyRerollResponseNativeLayout.Selector,
        DisplayUiTrackBountyNativeLayout.Selector
    }.SequenceEqual(Enumerable.Range(0x48, 8).Select(value => (byte)value))
    && new uint[]
    {
        BountyListActiveAckNativeLayout.CaseVirtualAddress,
        BountyListActiveDetailsAckNativeLayout.CaseVirtualAddress,
        BountyListAvailableAckNativeLayout.CaseVirtualAddress,
        BountyClearAckNativeLayout.CaseVirtualAddress,
        BountyClearPreviousAckNativeLayout.CaseVirtualAddress,
        BountyListPreviousAckNativeLayout.CaseVirtualAddress,
        BountyRerollResponseNativeLayout.CaseVirtualAddress,
        DisplayUiTrackBountyNativeLayout.CaseVirtualAddress
    }.SequenceEqual(new uint[]
    {
        0xC20FC8, 0xC21030, 0xC21098, 0xC21100,
        0xC21168, 0xC211D0, 0xC21238, 0xC212A0
    })
    && new uint[]
    {
        BountyListActiveAckNativeLayout.CaseFileOffset,
        BountyListActiveDetailsAckNativeLayout.CaseFileOffset,
        BountyListAvailableAckNativeLayout.CaseFileOffset,
        BountyClearAckNativeLayout.CaseFileOffset,
        BountyClearPreviousAckNativeLayout.CaseFileOffset,
        BountyListPreviousAckNativeLayout.CaseFileOffset,
        BountyRerollResponseNativeLayout.CaseFileOffset,
        DisplayUiTrackBountyNativeLayout.CaseFileOffset
    }.SequenceEqual(new uint[]
    {
        0x8203C8, 0x820430, 0x820498, 0x820500,
        0x820568, 0x8205D0, 0x820638, 0x8206A0
    })
    && new uint[]
    {
        BountyListActiveAckNativeLayout.AllocationSize,
        BountyListActiveDetailsAckNativeLayout.AllocationSize,
        BountyListAvailableAckNativeLayout.AllocationSize,
        BountyClearAckNativeLayout.AllocationSize,
        BountyClearPreviousAckNativeLayout.AllocationSize,
        BountyListPreviousAckNativeLayout.AllocationSize,
        BountyRerollResponseNativeLayout.AllocationSize,
        DisplayUiTrackBountyNativeLayout.AllocationSize
    }.SequenceEqual(new uint[] { 0x40, 0x40, 0x40, 0x30, 0x30, 0x40, 0x38, 0x30 })
    && new uint[]
    {
        BountyListActiveAckNativeLayout.ConstructorNativeTypeId,
        BountyListActiveDetailsAckNativeLayout.ConstructorNativeTypeId,
        BountyListAvailableAckNativeLayout.ConstructorNativeTypeId,
        BountyClearAckNativeLayout.ConstructorNativeTypeId,
        BountyClearPreviousAckNativeLayout.ConstructorNativeTypeId,
        BountyListPreviousAckNativeLayout.ConstructorNativeTypeId,
        BountyRerollResponseNativeLayout.ConstructorNativeTypeId,
        DisplayUiTrackBountyNativeLayout.ConstructorNativeTypeId
    }.SequenceEqual(Enumerable.Range(0x48, 8).Select(value => (uint)value))
    && new uint[]
    {
        BountyListActiveAckNativeLayout.CaseLocalDwordValue,
        BountyListActiveDetailsAckNativeLayout.CaseLocalDwordValue,
        BountyListAvailableAckNativeLayout.CaseLocalDwordValue,
        BountyClearAckNativeLayout.CaseLocalDwordValue,
        BountyClearPreviousAckNativeLayout.CaseLocalDwordValue,
        BountyListPreviousAckNativeLayout.CaseLocalDwordValue,
        BountyRerollResponseNativeLayout.CaseLocalDwordValue,
        DisplayUiTrackBountyNativeLayout.CaseLocalDwordValue
    }.SequenceEqual(Enumerable.Range(0x28, 8).Select(value => (uint)value))
    && new uint[]
    {
        BountyListActiveAckNativeLayout.BaseMetadataConstructorFileOffset,
        BountyListActiveDetailsAckNativeLayout.BaseMetadataConstructorFileOffset,
        BountyListAvailableAckNativeLayout.BaseMetadataConstructorFileOffset,
        BountyClearAckNativeLayout.BaseMetadataConstructorFileOffset,
        BountyClearPreviousAckNativeLayout.BaseMetadataConstructorFileOffset,
        BountyListPreviousAckNativeLayout.BaseMetadataConstructorFileOffset,
        BountyRerollResponseNativeLayout.BaseMetadataConstructorFileOffset,
        DisplayUiTrackBountyNativeLayout.BaseMetadataConstructorFileOffset
    }.SequenceEqual(new uint[]
    {
        0x8248A0, 0x824910, 0x824980, 0x8249F0,
        0x824A60, 0x824AD0, 0x824B40, 0x824BB0
    })
    && new uint[]
    {
        BountyListActiveAckNativeLayout.BodyReaderFileOffset,
        BountyListActiveDetailsAckNativeLayout.BodyReaderFileOffset,
        BountyListAvailableAckNativeLayout.BodyReaderFileOffset,
        BountyClearAckNativeLayout.BodyReaderFileOffset,
        BountyClearPreviousAckNativeLayout.BodyReaderFileOffset,
        BountyListPreviousAckNativeLayout.BodyReaderFileOffset,
        BountyRerollResponseNativeLayout.BodyReaderFileOffset,
        DisplayUiTrackBountyNativeLayout.BodyReaderFileOffset
    }.SequenceEqual(new uint[]
    {
        0x849760, 0x8496A0, 0x849600, 0x849560,
        0x8494C0, 0x849420, 0x849380, 0x3370A0
    })
    && new uint[]
    {
        BountyListActiveAckNativeLayout.VtableVirtualAddress,
        BountyListActiveDetailsAckNativeLayout.VtableVirtualAddress,
        BountyListAvailableAckNativeLayout.VtableVirtualAddress,
        BountyClearAckNativeLayout.VtableVirtualAddress,
        BountyClearPreviousAckNativeLayout.VtableVirtualAddress,
        BountyListPreviousAckNativeLayout.VtableVirtualAddress,
        BountyRerollResponseNativeLayout.VtableVirtualAddress,
        DisplayUiTrackBountyNativeLayout.VtableVirtualAddress
    }.SequenceEqual(new uint[]
    {
        0x1C6DFD8, 0x1C6DFF8, 0x1C6DFE8, 0x1C6DFB8,
        0x1C6DFA8, 0x1C6DFC8, 0x1C6DF88, 0x1C6DF78
    })
    && new string[]
    {
        BountyListActiveAckNativeLayout.RttiName,
        BountyListActiveDetailsAckNativeLayout.RttiName,
        BountyListAvailableAckNativeLayout.RttiName,
        BountyClearAckNativeLayout.RttiName,
        BountyClearPreviousAckNativeLayout.RttiName,
        BountyListPreviousAckNativeLayout.RttiName,
        BountyRerollResponseNativeLayout.RttiName,
        DisplayUiTrackBountyNativeLayout.RttiName
    }.SequenceEqual(new[]
    {
        ".?AVBountyListActiveAck@firefall@@",
        ".?AVBountyListActiveDetailsAck@firefall@@",
        ".?AVBountyListAvailableAck@firefall@@",
        ".?AVBountyClearAck@firefall@@",
        ".?AVBountyClearPreviousAck@firefall@@",
        ".?AVBountyListPreviousAck@firefall@@",
        ".?AVBountyRerollResponse@firefall@@",
        ".?AVDisplayUiTrackBounty@firefall@@"
    })
    && BountyListActiveAckNativeLayout.FirstByteFieldOffset == 0x28
    && BountyListActiveAckNativeLayout.ByteWireValueDwordOffset == 0x2C
    && BountyListActiveAckNativeLayout.EmbeddedBlobObjectOffset == 0x30
    && BountyListActiveDetailsAckNativeLayout.ByteFieldOffset == 0x28
    && BountyListActiveDetailsAckNativeLayout.EmbeddedBlobObjectOffset == 0x2C
    && BountyListAvailableAckNativeLayout.ByteFieldOffset == 0x28
    && BountyListAvailableAckNativeLayout.EmbeddedBlobObjectOffset == 0x2C
    && BountyClearAckNativeLayout.FirstByteFieldOffset == 0x28
    && BountyClearAckNativeLayout.ByteWireValueDwordOffset == 0x2C
    && BountyClearPreviousAckNativeLayout.FirstByteFieldOffset == 0x28
    && BountyClearPreviousAckNativeLayout.ByteWireValueDwordOffset == 0x2C
    && BountyListPreviousAckNativeLayout.ByteFieldOffset == 0x28
    && BountyListPreviousAckNativeLayout.EmbeddedBlobObjectOffset == 0x2C
    && BountyRerollResponseNativeLayout.DwordFieldOffset == 0x28
    && BountyRerollResponseNativeLayout.ByteFieldOffset == 0x2C
    && BountyRerollResponseNativeLayout.SharedTextFieldOffset == 0x30
    && DisplayUiTrackBountyNativeLayout.DwordFieldOffset == 0x28
    && MatrixSecondaryEmbeddedBlobNativeLayout.LengthPrefixWireWidth == 2
    && MatrixSecondaryEmbeddedBlobNativeLayout.ObjectCopyVirtualMethodOffset == 0x10
    && MatrixSecondaryEmbeddedBlobNativeLayout.ZeroLengthPayloadWidth == 0);
Check("Secondary selectors 0x50-0x57 retain RTTI identities and native reader shapes",
    new byte[]
    {
        AchievementsNativeLayout.Selector,
        AchievementUnlockedNativeLayout.Selector,
        TotalAchievementPointsNativeLayout.Selector,
        MissionCompletionCountsNativeLayout.Selector,
        ContentUnlockedNativeLayout.Selector,
        ClientUIActionNativeLayout.Selector,
        ArcCompletionHistoryUpdateNativeLayout.Selector,
        JobLedgerEntriesUpdateNativeLayout.Selector
    }.SequenceEqual(Enumerable.Range(0x50, 8).Select(value => (byte)value))
    && new uint[]
    {
        AchievementsNativeLayout.CaseFileOffset,
        AchievementUnlockedNativeLayout.CaseFileOffset,
        TotalAchievementPointsNativeLayout.CaseFileOffset,
        MissionCompletionCountsNativeLayout.CaseFileOffset,
        ContentUnlockedNativeLayout.CaseFileOffset,
        ClientUIActionNativeLayout.CaseFileOffset,
        ArcCompletionHistoryUpdateNativeLayout.CaseFileOffset,
        JobLedgerEntriesUpdateNativeLayout.CaseFileOffset
    }.SequenceEqual(new uint[]
    {
        0x82070B, 0x820775, 0x8207BD, 0x820805,
        0x820879, 0x8208E1, 0x820929, 0x820993
    })
    && new uint[]
    {
        AchievementsNativeLayout.CaseVirtualAddress,
        AchievementUnlockedNativeLayout.CaseVirtualAddress,
        TotalAchievementPointsNativeLayout.CaseVirtualAddress,
        MissionCompletionCountsNativeLayout.CaseVirtualAddress,
        ContentUnlockedNativeLayout.CaseVirtualAddress,
        ClientUIActionNativeLayout.CaseVirtualAddress,
        ArcCompletionHistoryUpdateNativeLayout.CaseVirtualAddress,
        JobLedgerEntriesUpdateNativeLayout.CaseVirtualAddress
    }.SequenceEqual(new uint[]
    {
        0xC2130B, 0xC21375, 0xC213BD, 0xC21405,
        0xC21479, 0xC214E1, 0xC21529, 0xC21593
    })
    && new int[]
    {
        AchievementsNativeLayout.AllocationSize,
        AchievementUnlockedNativeLayout.AllocationSize,
        TotalAchievementPointsNativeLayout.AllocationSize,
        MissionCompletionCountsNativeLayout.AllocationSize,
        ContentUnlockedNativeLayout.AllocationSize,
        ClientUIActionNativeLayout.AllocationSize,
        ArcCompletionHistoryUpdateNativeLayout.AllocationSize,
        JobLedgerEntriesUpdateNativeLayout.AllocationSize
    }.SequenceEqual(new[] { 0x38, 0x30, 0x30, 0x38, 0x38, 0x30, 0x30, 0x38 })
    && new uint[]
    {
        AchievementsNativeLayout.ConstructorNativeTypeId,
        AchievementUnlockedNativeLayout.ConstructorNativeTypeId,
        TotalAchievementPointsNativeLayout.ConstructorNativeTypeId,
        MissionCompletionCountsNativeLayout.ConstructorNativeTypeId,
        ContentUnlockedNativeLayout.ConstructorNativeTypeId,
        ClientUIActionNativeLayout.ConstructorNativeTypeId,
        ArcCompletionHistoryUpdateNativeLayout.ConstructorNativeTypeId,
        JobLedgerEntriesUpdateNativeLayout.ConstructorNativeTypeId
    }.SequenceEqual(Enumerable.Range(0x50, 8).Select(value => (uint)value))
    && new uint[]
    {
        AchievementsNativeLayout.VtableVirtualAddress,
        AchievementUnlockedNativeLayout.VtableVirtualAddress,
        TotalAchievementPointsNativeLayout.VtableVirtualAddress,
        MissionCompletionCountsNativeLayout.VtableVirtualAddress,
        ContentUnlockedNativeLayout.VtableVirtualAddress,
        ClientUIActionNativeLayout.VtableVirtualAddress,
        ArcCompletionHistoryUpdateNativeLayout.VtableVirtualAddress,
        JobLedgerEntriesUpdateNativeLayout.VtableVirtualAddress
    }.SequenceEqual(new uint[]
    {
        0x1C6DF98, 0x1C6DF48, 0x1C6DF38, 0x1C6DF68,
        0x1C6DF58, 0x1C6DF08, 0x1C6DF28, 0x1C6DF18
    })
    && new uint[]
    {
        AchievementsNativeLayout.ConstructorFileOffset,
        AchievementUnlockedNativeLayout.ConstructorFileOffset,
        TotalAchievementPointsNativeLayout.ConstructorFileOffset,
        MissionCompletionCountsNativeLayout.ConstructorFileOffset,
        ContentUnlockedNativeLayout.ConstructorFileOffset,
        ClientUIActionNativeLayout.ConstructorFileOffset,
        ArcCompletionHistoryUpdateNativeLayout.ConstructorFileOffset,
        JobLedgerEntriesUpdateNativeLayout.ConstructorFileOffset
    }.SequenceEqual(new uint[]
    {
        0x8178C0, 0x817A10, 0x817AD0, 0x817B90,
        0x817D60, 0x817F20, 0x8180B0, 0x818220
    })
    && new uint[]
    {
        AchievementsNativeLayout.BaseMetadataConstructorFileOffset,
        AchievementUnlockedNativeLayout.BaseMetadataConstructorFileOffset,
        TotalAchievementPointsNativeLayout.BaseMetadataConstructorFileOffset,
        MissionCompletionCountsNativeLayout.BaseMetadataConstructorFileOffset,
        ContentUnlockedNativeLayout.BaseMetadataConstructorFileOffset,
        ClientUIActionNativeLayout.BaseMetadataConstructorFileOffset,
        ArcCompletionHistoryUpdateNativeLayout.BaseMetadataConstructorFileOffset,
        JobLedgerEntriesUpdateNativeLayout.BaseMetadataConstructorFileOffset
    }.SequenceEqual(new uint[]
    {
        0x824C20, 0x824D80, 0x824DF0, 0x824E60,
        0x824FD0, 0x825040, 0x8250B0, 0x825120
    })
    && new uint[]
    {
        AchievementsNativeLayout.DescriptorAnchorVirtualAddress,
        AchievementUnlockedNativeLayout.DescriptorAnchorVirtualAddress,
        TotalAchievementPointsNativeLayout.DescriptorAnchorVirtualAddress,
        MissionCompletionCountsNativeLayout.DescriptorAnchorVirtualAddress,
        ContentUnlockedNativeLayout.DescriptorAnchorVirtualAddress,
        ClientUIActionNativeLayout.DescriptorAnchorVirtualAddress,
        ArcCompletionHistoryUpdateNativeLayout.DescriptorAnchorVirtualAddress,
        JobLedgerEntriesUpdateNativeLayout.DescriptorAnchorVirtualAddress
    }.SequenceEqual(new uint[]
    {
        0x1B89B5C, 0x1B89B64, 0x1B89B4C, 0x1B89B54,
        0x1B89B3C, 0x1B89B44, 0x1B89B2C, 0x1B89B34
    })
    && new string[]
    {
        AchievementsNativeLayout.RttiName,
        AchievementUnlockedNativeLayout.RttiName,
        TotalAchievementPointsNativeLayout.RttiName,
        MissionCompletionCountsNativeLayout.RttiName,
        ContentUnlockedNativeLayout.RttiName,
        ClientUIActionNativeLayout.RttiName,
        ArcCompletionHistoryUpdateNativeLayout.RttiName,
        JobLedgerEntriesUpdateNativeLayout.RttiName
    }.SequenceEqual(new[]
    {
        ".?AVAchievements@firefall@@",
        ".?AVAchievementUnlocked@firefall@@",
        ".?AVTotalAchievementPoints@firefall@@",
        ".?AVMissionCompletionCounts@firefall@@",
        ".?AVContentUnlocked@firefall@@",
        ".?AVClientUIAction@firefall@@",
        ".?AVArcCompletionHistoryUpdate@firefall@@",
        ".?AVJobLedgerEntriesUpdate@firefall@@"
    })
    && new uint[]
    {
        AchievementsNativeLayout.BodyReaderFileOffset,
        AchievementUnlockedNativeLayout.BodyReaderFileOffset,
        TotalAchievementPointsNativeLayout.BodyReaderFileOffset,
        MissionCompletionCountsNativeLayout.BodyReaderFileOffset,
        ContentUnlockedNativeLayout.BodyReaderFileOffset,
        ClientUIActionNativeLayout.BodyReaderFileOffset,
        ArcCompletionHistoryUpdateNativeLayout.BodyReaderFileOffset,
        JobLedgerEntriesUpdateNativeLayout.BodyReaderFileOffset
    }.SequenceEqual(new uint[]
    {
        0x848FC0, 0x356F80, 0x3370A0, 0x848610,
        0x847DC0, 0x12EE010, 0x847A70, 0x6095C0
    })
    && new uint[]
    {
        AchievementsNativeLayout.BodyReaderCallSiteFileOffset,
        AchievementUnlockedNativeLayout.BodyReaderCallSiteFileOffset,
        TotalAchievementPointsNativeLayout.BodyReaderCallSiteFileOffset,
        MissionCompletionCountsNativeLayout.BodyReaderCallSiteFileOffset,
        ContentUnlockedNativeLayout.BodyReaderCallSiteFileOffset,
        ClientUIActionNativeLayout.BodyReaderCallSiteFileOffset,
        ArcCompletionHistoryUpdateNativeLayout.BodyReaderCallSiteFileOffset,
        JobLedgerEntriesUpdateNativeLayout.BodyReaderCallSiteFileOffset
    }.SequenceEqual(new uint[]
    {
        0x82075B, 0x81F73C, 0x8206F1, 0x820855,
        0x8208C7, 0x81FB34, 0x820979, 0x8209E3
    })
    && AchievementUnlockedNativeLayout.SharedBodyContinuationFileOffset == 0x81F72E
    && TotalAchievementPointsNativeLayout.SharedBodyContinuationFileOffset == 0x8206E3
    && ClientUIActionNativeLayout.SharedBodyContinuationFileOffset == 0x81FB27
    && AchievementsNativeLayout.RecordWireWidth == 5
    && AchievementsNativeLayout.RecordNativeStride == 8
    && AchievementsNativeLayout.CountWireWidth == 1
    && AchievementsNativeLayout.ChunkContinuationCount == 0xFF
    && AchievementUnlockedNativeLayout.QwordFieldOffset == 0x28
    && AchievementUnlockedNativeLayout.QwordWireWidth == 8
    && TotalAchievementPointsNativeLayout.DwordFieldOffset == 0x28
    && TotalAchievementPointsNativeLayout.DwordWireWidth == 4
    && MissionCompletionCountsNativeLayout.RecordWireWidth == 16
    && MissionCompletionCountsNativeLayout.RecordNativeStride == 24
    && MissionCompletionCountsNativeLayout.SharedTextFieldOffset == 0x34
    && ContentUnlockedNativeLayout.SharedTextFieldOffset == 0x28
    && ContentUnlockedNativeLayout.DwordFieldOffset == 0x2C
    && ContentUnlockedNativeLayout.QwordFieldOffset == 0x30
    && ClientUIActionNativeLayout.SharedTextFieldOffset == 0x28
    && ArcCompletionHistoryUpdateNativeLayout.RecordWireWidth == 16
    && ArcCompletionHistoryUpdateNativeLayout.TreeSentinelAllocationSize == 0x20
    && ArcCompletionHistoryUpdateNativeLayout.TreeSentinelMarkerWordOffset == 0x0C
    && ArcCompletionHistoryUpdateNativeLayout.TreeSentinelMarkerWordValue == 0x0101
    && JobLedgerEntriesUpdateNativeLayout.RecordWireWidth == 4
    && JobLedgerEntriesUpdateNativeLayout.RecordNativeStride == 4
    && new uint[]
    {
        AchievementsNativeLayout.CaseLocalDwordValue,
        AchievementUnlockedNativeLayout.CaseLocalDwordValue,
        TotalAchievementPointsNativeLayout.CaseLocalDwordValue,
        MissionCompletionCountsNativeLayout.CaseLocalDwordValue,
        ContentUnlockedNativeLayout.CaseLocalDwordValue,
        ClientUIActionNativeLayout.CaseLocalDwordValue,
        ArcCompletionHistoryUpdateNativeLayout.CaseLocalDwordValue,
        JobLedgerEntriesUpdateNativeLayout.CaseLocalDwordValue
    }.SequenceEqual(Enumerable.Range(0x30, 8).Select(value => (uint)value)));
Check("Secondary parser-local DWORD literals remain separate from selector mapping",
    new uint[]
    {
        ScoreBoardRemovePlayerNativeLayout.ParserLocalDwordValue,
        ScoreBoardUpdatePlayerStatsNativeLayout.ParserLocalDwordValue,
        ScoreBoardUpdatePlayerStatsFromStatsNativeLayout.ParserLocalDwordValue,
        ScoreBoardUpdatePlayerStatusNativeLayout.ParserLocalDwordValue,
        MatchLoadingStateNativeLayout.ParserLocalDwordValue,
        MatchEndAckNativeLayout.ParserLocalDwordValue,
        ServerProfilerSendNamesNativeLayout.ParserLocalDwordValue,
        ServerProfilerSendFrameNativeLayout.ParserLocalDwordValue,
        TempConsoleMessageNativeLayout.CaseLocalDwordValue,
        ReloadStaticDataNativeLayout.CaseLocalDwordValue,
        EncDebugChatMessageNativeLayout.CaseLocalDwordValue,
        SendRadioMessageNativeLayout.CaseLocalDwordValue,
        NpcBehaviorInfoNativeLayout.CaseLocalDwordValue,
        NpcMonitoringLogNativeLayout.CaseLocalDwordValue,
        NpcNavigationInfoNativeLayout.CaseLocalDwordValue,
        NpcHostilityDebugInfoNativeLayout.CaseLocalDwordValue,
        NpcPositionalDebugInfoNativeLayout.CaseLocalDwordValue,
        NpcShapeDebugInfoNativeLayout.CaseLocalDwordValue,
        NpcVoxelInfoNativeLayout.CaseLocalDwordValue,
        DebugDrawInfoNativeLayout.CaseLocalDwordValue,
        NpcDevCmdResponseNativeLayout.CaseLocalDwordValue,
        DevRequestObjectPositionsNativeLayout.CaseLocalDwordValue,
        DevRequestSpawnTablesNativeLayout.CaseLocalDwordValue,
        DevRequestResourceNodeDebugNativeLayout.CaseLocalDwordValue
    }.SequenceEqual(Enumerable.Range(8, 24).Select(value => (uint)value))
    && ScoreBoardRemovePlayerNativeLayout.ParserLocalDwordValue !=
        ScoreBoardRemovePlayerNativeLayout.Selector
    && ServerProfilerSendFrameNativeLayout.ParserLocalDwordValue !=
        ServerProfilerSendFrameNativeLayout.Selector
    && NpcHostilityDebugInfoNativeLayout.CaseLocalDwordValue ==
        NpcHostilityDebugInfoNativeLayout.Selector - 0x20
    && DevRequestResourceNodeDebugNativeLayout.CaseLocalDwordValue ==
        DevRequestResourceNodeDebugNativeLayout.Selector - 0x20);
Check("AreaVisualData LootObjectCollected route-index-2 subcase layout is recorded",
    AreaVisualDataLootObjectCollectedNativeLayout.DispatcherFileOffset == 0x81EE63
    && AreaVisualDataLootObjectCollectedNativeLayout.DispatcherValue == 0x53
    && AreaVisualDataLootObjectCollectedNativeLayout.CaseFileOffset == 0x81EF1C
    && AreaVisualDataLootObjectCollectedNativeLayout.CaseLocalDwordValue == 0xCD
    && AreaVisualDataLootObjectCollectedNativeLayout.AllocationSize == 0x40
    && AreaVisualDataLootObjectCollectedNativeLayout.ConstructorFileOffset == 0xADBAA0
    && AreaVisualDataLootObjectCollectedNativeLayout.BaseConstructorHelperFileOffset == 0xB07660
    && AreaVisualDataLootObjectCollectedNativeLayout.BaseConstructorArgument == 0x53
    && AreaVisualDataLootObjectCollectedNativeLayout.BaseMetadataStringVirtualAddress == 0x1BB0224
    && AreaVisualDataLootObjectCollectedNativeLayout.MessageDiscriminatorFieldOffset == 0x10
    && AreaVisualDataLootObjectCollectedNativeLayout.MessageDiscriminator == 0x53
    && AreaVisualDataLootObjectCollectedNativeLayout.EnvelopeKeyLowDwordFieldOffset == 0x18
    && AreaVisualDataLootObjectCollectedNativeLayout.EnvelopeKeyHighDwordFieldOffset == 0x1C
    && AreaVisualDataLootObjectCollectedNativeLayout.BodyReaderFileOffset == 0x83AD90
    && AreaVisualDataLootObjectCollectedNativeLayout.MessagePointerEaxRestoreFileOffset == 0x81EF58
    && AreaVisualDataLootObjectCollectedNativeLayout.VtableVirtualAddress == 0x1C723C0
    && AreaVisualDataLootObjectCollectedNativeLayout.RttiName == ".?AVLootObjectCollected@AreaVisualData@firefall@@"
    && AreaVisualDataLootObjectCollectedNativeLayout.DwordFieldOffset == 0x28
    && AreaVisualDataLootObjectCollectedNativeLayout.FirstQwordFieldOffset == 0x30
    && AreaVisualDataLootObjectCollectedNativeLayout.SecondQwordFieldOffset == 0x38);
Check("Vehicle AbilityActivated route-index-3 switch-entry layout is recorded",
    VehicleAbilityActivatedNativeLayout.DispatcherFileOffset == 0x81EF6D
    && VehicleAbilityActivatedNativeLayout.SwitchTableVirtualAddress == 0xC2260C
    && VehicleAbilityActivatedNativeLayout.SwitchTableFileOffset == 0x821A0C
    && VehicleAbilityActivatedNativeLayout.SwitchEntryIndex == 0
    && VehicleAbilityActivatedNativeLayout.SwitchArgumentAdjustment == -0x53
    && VehicleAbilityActivatedNativeLayout.CaseFileOffset == 0x81EF83
    && VehicleAbilityActivatedNativeLayout.CaseLocalDwordValue == 0xD0
    && VehicleAbilityActivatedNativeLayout.AllocationSize == 0x30
    && VehicleAbilityActivatedNativeLayout.ConstructorFileOffset == 0xB093C0
    && VehicleAbilityActivatedNativeLayout.BaseConstructorHelperFileOffset == 0xB474B0
    && VehicleAbilityActivatedNativeLayout.BaseConstructorArgument == 0x53
    && VehicleAbilityActivatedNativeLayout.BodyReaderFileOffset == 0x83A840
    && VehicleAbilityActivatedNativeLayout.VtableVirtualAddress == 0x1C72998
    && VehicleAbilityActivatedNativeLayout.RttiName == ".?AVAbilityActivated@Vehicle@firefall@@"
    && VehicleAbilityActivatedNativeLayout.FirstDwordFieldOffset == 0x28
    && VehicleAbilityActivatedNativeLayout.SecondDwordFieldOffset == 0x2C);
var vehicleAbilityMessageProvenance = MatrixMessageParserFlowNativeLayout.MessageDiscriminatorProvenance
    .Single(provenance =>
        provenance.MessageRttiName == VehicleAbilityActivatedNativeLayout.RttiName);
Check("Vehicle AbilityActivated's shared selector-0x53 consumer reads beyond its recorded object extent",
    vehicleAbilityMessageProvenance.MessageDiscriminator == 0x53
    && vehicleAbilityMessageProvenance.ReceiverVtableSlotOffset
        == MatrixMessageNamespaceRoutingNativeLayout.Selector53ProtocolHandlerVtableSlotOffset
    && vehicleAbilityMessageProvenance.ReceiverMethodFileOffset
        == MatrixMessageNamespaceRoutingNativeLayout.Selector53ProtocolHandlerMethodFileOffset
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerFilterMessageFieldOffset
        >= VehicleAbilityActivatedNativeLayout.AllocationSize
    && VehicleAbilityActivatedNativeLayout.FirstDwordFieldOffset == 0x28
    && VehicleAbilityActivatedNativeLayout.SecondDwordFieldOffset == 0x2C);
Check("Deployable TookHit route-index-4 subcase layout is recorded",
    DeployableTookHitNativeLayout.DispatcherFileOffset == 0x81F206
    && DeployableTookHitNativeLayout.DispatcherValue == 0x53
    && DeployableTookHitNativeLayout.CaseFileOffset == 0x81F29E
    && DeployableTookHitNativeLayout.CaseLocalDwordValue == 0xD9
    && DeployableTookHitNativeLayout.AllocationSize == 0x60
    && DeployableTookHitNativeLayout.ConstructorFileOffset == 0xB27E80
    && DeployableTookHitNativeLayout.BaseConstructorHelperFileOffset == 0xB47C40
    && DeployableTookHitNativeLayout.BaseConstructorArgument == 0x53
    && DeployableTookHitNativeLayout.BodyReaderFileOffset == 0x83A120
    && DeployableTookHitNativeLayout.ConditionalRecordReaderFileOffset == 0x5F0CF0
    && DeployableTookHitNativeLayout.VtableVirtualAddress == 0x1C727A8
    && DeployableTookHitNativeLayout.RttiName == ".?AVTookHit@Deployable@firefall@@"
    && DeployableTookHitNativeLayout.ConditionalRecordOffset == 0x28
    && DeployableTookHitNativeLayout.ConditionalRecordQwordReaderFileOffset == 0x356F80
    && DeployableTookHitNativeLayout.ConditionalRecordMergeHelperFileOffset == 0x5F0C30
    && DeployableTookHitNativeLayout.ConditionalRecordQwordFieldOffset == 0x30
    && DeployableTookHitNativeLayout.ConditionalRecordQwordHighDwordFieldOffset == 0x34
    && DeployableTookHitNativeLayout.ConditionalRecordNestedQwordPresenceFieldOffset == 0x38
    && DeployableTookHitNativeLayout.ConditionalRecordNestedQwordFieldOffset == 0x40
    && DeployableTookHitNativeLayout.ConditionalRecordDwordFieldOffset == 0x48
    && DeployableTookHitNativeLayout.ConditionalRecordByteFieldOffset == 0x4C
    && DeployableTookHitNativeLayout.FirstByteFieldOffset == 0x50
    && DeployableTookHitNativeLayout.SecondByteFieldOffset == 0x51
    && DeployableTookHitNativeLayout.WordFieldOffset == 0x58
    && DeployableTookHitNativeLayout.FinalByteFieldOffset == 0x5A);
Check("Deployable TookHit selector-0x53 consumer's filter field aliases a conditional QWORD half",
    DeployableTookHitNativeLayout.BaseConstructorArgument == 0x53
    && DeployableTookHitNativeLayout.ConditionalRecordQwordHighDwordFieldOffset
        == MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerFilterMessageFieldOffset
    && DeployableTookHitNativeLayout.ConditionalRecordQwordHighDwordFieldOffset
        < DeployableTookHitNativeLayout.AllocationSize
    && DeployableTookHitNativeLayout.ConditionalRecordOffset
        < DeployableTookHitNativeLayout.ConditionalRecordQwordFieldOffset);
Check("Turret WeaponProjectileFired route-index-5 subcase layout is recorded",
    TurretWeaponProjectileFiredNativeLayout.DispatcherFileOffset == 0x81F2EF
    && TurretWeaponProjectileFiredNativeLayout.DispatcherValue == 0x53
    && TurretWeaponProjectileFiredNativeLayout.CaseFileOffset == 0x81F2F9
    && TurretWeaponProjectileFiredNativeLayout.CaseLocalDwordValue == 0xDC
    && TurretWeaponProjectileFiredNativeLayout.AllocationSize == 0x48
    && TurretWeaponProjectileFiredNativeLayout.ConstructorFileOffset == 0xB36520
    && TurretWeaponProjectileFiredNativeLayout.BaseConstructorHelperFileOffset == 0xB47D90
    && TurretWeaponProjectileFiredNativeLayout.BaseConstructorArgument == 0x53
    && TurretWeaponProjectileFiredNativeLayout.BodyReaderFileOffset == 0x839EB0
    && TurretWeaponProjectileFiredNativeLayout.OpaqueFieldReaderFileOffset == 0x5C0270
    && TurretWeaponProjectileFiredNativeLayout.VtableVirtualAddress == 0x1C726E8
    && TurretWeaponProjectileFiredNativeLayout.RttiName == ".?AVWeaponProjectileFired@Turret@firefall@@"
    && TurretWeaponProjectileFiredNativeLayout.DwordFieldOffset == 0x28
    && TurretWeaponProjectileFiredNativeLayout.FirstRepeatedReaderFieldOffset == 0x2C
    && TurretWeaponProjectileFiredNativeLayout.SecondRepeatedReaderFieldOffset == 0x30
    && TurretWeaponProjectileFiredNativeLayout.ThirdRepeatedReaderFieldOffset == 0x34
    && TurretWeaponProjectileFiredNativeLayout.OpaqueFieldOffset == 0x38);
Check("Turret WeaponProjectileFired's selector-0x53 consumer reads its third DWORD as a filter candidate",
    TurretWeaponProjectileFiredNativeLayout.BaseConstructorArgument == 0x53
    && TurretWeaponProjectileFiredNativeLayout.ThirdRepeatedReaderFieldOffset
        == MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerFilterMessageFieldOffset
    && TurretWeaponProjectileFiredNativeLayout.ThirdRepeatedReaderFieldOffset
        < TurretWeaponProjectileFiredNativeLayout.AllocationSize
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53ProtocolHandlerMethodFileOffset == 0xACA580);
Check("LootStoreExtension LootObjectCollected route-index-6 subcase layout is recorded",
    LootStoreExtensionLootObjectCollectedNativeLayout.DispatcherFileOffset == 0x81F34A
    && LootStoreExtensionLootObjectCollectedNativeLayout.DispatcherValue == 0x53
    && LootStoreExtensionLootObjectCollectedNativeLayout.CaseFileOffset == 0x81F354
    && LootStoreExtensionLootObjectCollectedNativeLayout.CaseLocalDwordValue == 0xDD
    && LootStoreExtensionLootObjectCollectedNativeLayout.AllocationSize == 0x40
    && LootStoreExtensionLootObjectCollectedNativeLayout.ConstructorFileOffset == 0xB43A70
    && LootStoreExtensionLootObjectCollectedNativeLayout.BaseConstructorHelperFileOffset == 0xB48040
    && LootStoreExtensionLootObjectCollectedNativeLayout.BaseConstructorArgument == 0x53
    && LootStoreExtensionLootObjectCollectedNativeLayout.BodyReaderFileOffset == 0x839DB0
    && LootStoreExtensionLootObjectCollectedNativeLayout.VtableVirtualAddress == 0x1C72608
    && LootStoreExtensionLootObjectCollectedNativeLayout.RttiName == ".?AVLootObjectCollected@LootStoreExtension@firefall@@"
    && LootStoreExtensionLootObjectCollectedNativeLayout.DwordFieldOffset == 0x28
    && LootStoreExtensionLootObjectCollectedNativeLayout.FirstQwordFieldOffset == 0x30
    && LootStoreExtensionLootObjectCollectedNativeLayout.SecondQwordFieldOffset == 0x38);
Check("LootStoreExtension LootObjectCollected's selector-0x53 filter candidate aliases its first QWORD",
    LootStoreExtensionLootObjectCollectedNativeLayout.BaseConstructorArgument == 0x53
    && LootStoreExtensionLootObjectCollectedNativeLayout.FirstQwordFieldOffset + sizeof(uint)
        == MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerFilterMessageFieldOffset
    && LootStoreExtensionLootObjectCollectedNativeLayout.FirstQwordFieldOffset + sizeof(uint)
        < LootStoreExtensionLootObjectCollectedNativeLayout.AllocationSize
    && LootStoreExtensionLootObjectCollectedNativeLayout.DwordFieldOffset == 0x28);
Check("AreaVisualData AudioEmitterSpawned value0x54 layout is recorded",
    AreaVisualDataAudioEmitterSpawnedNativeLayout.DispatcherFileOffset == 0x81EE63
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.DispatcherValue == 0x54
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.CaseFileOffset == 0x81EECB
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.CaseLocalDwordValue == 0xCE
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.AllocationSize == 0x40
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.ConstructorFileOffset == 0xADBB40
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.BaseConstructorHelperFileOffset == 0xB076D0
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.BaseConstructorArgument == 0x54
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.BodyReaderFileOffset == 0x83ACA0
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.VtableVirtualAddress == 0x1C72358
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.RttiName == ".?AVAudioEmitterSpawned@AreaVisualData@firefall@@"
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.QwordFieldOffset == 0x28
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.FirstDwordFieldOffset == 0x30
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.SecondDwordFieldOffset == 0x34
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.ThirdDwordFieldOffset == 0x38
    && AreaVisualDataAudioEmitterSpawnedNativeLayout.WordFieldOffset == 0x3C);
Check("AreaVisualData ParticleEffectSpawned value0x55 layout preserves conditional records",
    AreaVisualDataParticleEffectSpawnedNativeLayout.DispatcherValue == 0x55
    && AreaVisualDataParticleEffectSpawnedNativeLayout.CaseFileOffset == 0x81EE79
    && AreaVisualDataParticleEffectSpawnedNativeLayout.CaseLocalDwordValue == 0xCF
    && AreaVisualDataParticleEffectSpawnedNativeLayout.AllocationSize == 0x70
    && AreaVisualDataParticleEffectSpawnedNativeLayout.ConstructorFileOffset == 0xADBBC0
    && AreaVisualDataParticleEffectSpawnedNativeLayout.BaseConstructorHelperFileOffset == 0xB07740
    && AreaVisualDataParticleEffectSpawnedNativeLayout.BaseConstructorArgument == 0x55
    && AreaVisualDataParticleEffectSpawnedNativeLayout.BodyReaderFileOffset == 0x83AAA0
    && AreaVisualDataParticleEffectSpawnedNativeLayout.VtableVirtualAddress == 0x1C72368
    && AreaVisualDataParticleEffectSpawnedNativeLayout.RttiName == ".?AVParticleEffectSpawned@AreaVisualData@firefall@@"
    && AreaVisualDataParticleEffectSpawnedNativeLayout.QwordFieldOffset == 0x28
    && AreaVisualDataParticleEffectSpawnedNativeLayout.FirstDwordFieldOffset == 0x30
    && AreaVisualDataParticleEffectSpawnedNativeLayout.FirstConditionalFlagOffset == 0x40
    && AreaVisualDataParticleEffectSpawnedNativeLayout.FirstConditionalValuesOffset == 0x44
    && AreaVisualDataParticleEffectSpawnedNativeLayout.FirstWordFieldOffset == 0x50
    && AreaVisualDataParticleEffectSpawnedNativeLayout.ByteFieldOffset == 0x58
    && AreaVisualDataParticleEffectSpawnedNativeLayout.DwordFieldOffset == 0x5C
    && AreaVisualDataParticleEffectSpawnedNativeLayout.WordFieldOffset == 0x60
    && AreaVisualDataParticleEffectSpawnedNativeLayout.SecondConditionalFlagOffset == 0x64
    && AreaVisualDataParticleEffectSpawnedNativeLayout.SecondConditionalValuesOffset == 0x68
    && AreaVisualDataParticleEffectSpawnedNativeLayout.ConstructorZeroedFirstFlagOffset == 0x40
    && AreaVisualDataParticleEffectSpawnedNativeLayout.ConstructorZeroedSecondFlagOffset == 0x64);
Check("Vehicle AbilityFailed route-index-3 switch-entry 1 layout is recorded",
    VehicleAbilityFailedNativeLayout.SwitchEntryIndex == 1
    && VehicleAbilityFailedNativeLayout.DispatcherValue + VehicleAbilityFailedNativeLayout.SwitchArgumentAdjustment
        == VehicleAbilityFailedNativeLayout.SwitchEntryIndex
    && VehicleAbilityFailedNativeLayout.DispatcherValue == 0x54
    && VehicleAbilityFailedNativeLayout.CaseFileOffset == 0x81EFD4
    && VehicleAbilityFailedNativeLayout.CaseLocalDwordValue == 0xD1
    && VehicleAbilityFailedNativeLayout.AllocationSize == 0x30
    && VehicleAbilityFailedNativeLayout.ConstructorFileOffset == 0xB09440
    && VehicleAbilityFailedNativeLayout.BaseConstructorHelperFileOffset == 0xB47520
    && VehicleAbilityFailedNativeLayout.BaseConstructorArgument == 0x54
    && VehicleAbilityFailedNativeLayout.BodyReaderFileOffset == 0x83A7A0
    && VehicleAbilityFailedNativeLayout.VtableVirtualAddress == 0x1C72978
    && VehicleAbilityFailedNativeLayout.RttiName == ".?AVAbilityFailed@Vehicle@firefall@@"
    && VehicleAbilityFailedNativeLayout.FirstDwordFieldOffset == 0x28
    && VehicleAbilityFailedNativeLayout.SecondDwordFieldOffset == 0x2C);
Check("Vehicle ForcedMovementCancelled route-index-3 switch-entry 6 layout is recorded",
    VehicleForcedMovementCancelledNativeLayout.SwitchEntryIndex == 6
    && VehicleForcedMovementCancelledNativeLayout.DispatcherValue + VehicleForcedMovementCancelledNativeLayout.SwitchArgumentAdjustment
        == VehicleForcedMovementCancelledNativeLayout.SwitchEntryIndex
    && VehicleForcedMovementCancelledNativeLayout.DispatcherValue == 0x59
    && VehicleForcedMovementCancelledNativeLayout.CaseFileOffset == 0x81F110
    && VehicleForcedMovementCancelledNativeLayout.CaseLocalDwordValue == 0xD6
    && VehicleForcedMovementCancelledNativeLayout.AllocationSize == 0x30
    && VehicleForcedMovementCancelledNativeLayout.ConstructorFileOffset == 0xB097A0
    && VehicleForcedMovementCancelledNativeLayout.BaseConstructorHelperFileOffset == 0xB47760
    && VehicleForcedMovementCancelledNativeLayout.BaseConstructorArgument == 0x59
    && VehicleForcedMovementCancelledNativeLayout.BodyReaderFileOffset == 0x83A440
    && VehicleForcedMovementCancelledNativeLayout.VtableVirtualAddress == 0x1C72928
    && VehicleForcedMovementCancelledNativeLayout.RttiName == ".?AVForcedMovementCancelled@Vehicle@firefall@@"
    && VehicleForcedMovementCancelledNativeLayout.DwordFieldOffset == 0x28
    && VehicleForcedMovementCancelledNativeLayout.WordFieldOffset == 0x2C);
Check("Vehicle PublicCombatLog route-index-3 switch-entry 2 layout is recorded",
    VehiclePublicCombatLogNativeLayout.DispatcherFileOffset == 0x81EF6D
    && VehiclePublicCombatLogNativeLayout.SwitchTableVirtualAddress == 0xC2260C
    && VehiclePublicCombatLogNativeLayout.SwitchTableFileOffset == 0x821A0C
    && VehiclePublicCombatLogNativeLayout.SwitchEntryIndex == 2
    && VehiclePublicCombatLogNativeLayout.DispatcherValue + VehiclePublicCombatLogNativeLayout.SwitchArgumentAdjustment
        == VehiclePublicCombatLogNativeLayout.SwitchEntryIndex
    && VehiclePublicCombatLogNativeLayout.DispatcherValue == 0x55
    && VehiclePublicCombatLogNativeLayout.CaseFileOffset == 0x81F025
    && VehiclePublicCombatLogNativeLayout.CaseLocalDwordValue == 0xD2
    && VehiclePublicCombatLogNativeLayout.AllocationSize == 0x40
    && VehiclePublicCombatLogNativeLayout.ConstructorFileOffset == 0xB094C0
    && VehiclePublicCombatLogNativeLayout.BaseConstructorHelperFileOffset == 0xB47590
    && VehiclePublicCombatLogNativeLayout.BaseConstructorArgument == 0x55
    && VehiclePublicCombatLogNativeLayout.BodyReaderFileOffset == 0x5F3350
    && VehiclePublicCombatLogNativeLayout.LengthPrefixedBlobReaderFileOffset == 0x12EE170
    && VehiclePublicCombatLogNativeLayout.VtableVirtualAddress == 0x1C72988
    && VehiclePublicCombatLogNativeLayout.RttiName == ".?AVPublicCombatLog@Vehicle@firefall@@"
    && VehiclePublicCombatLogNativeLayout.DwordGateFieldOffset == 0x28
    && VehiclePublicCombatLogNativeLayout.LengthPrefixedBlobFieldOffset == 0x2C
    && VehiclePublicCombatLogNativeLayout.LengthPrefixWidth == 2
    && VehiclePublicCombatLogNativeLayout.ConstructorSeededDwordValue == 0x1C80CC4);
Check("Vehicle CurrentPoseUpdate route-index-3 switch-entry 3 layout is recorded",
    VehicleCurrentPoseUpdateNativeLayout.SwitchEntryIndex == 3
    && VehicleCurrentPoseUpdateNativeLayout.DispatcherValue + VehicleCurrentPoseUpdateNativeLayout.SwitchArgumentAdjustment
        == VehicleCurrentPoseUpdateNativeLayout.SwitchEntryIndex
    && VehicleCurrentPoseUpdateNativeLayout.DispatcherValue == 0x56
    && VehicleCurrentPoseUpdateNativeLayout.CaseFileOffset == 0x81F056
    && VehicleCurrentPoseUpdateNativeLayout.CaseLocalDwordValue == 0xD3
    && VehicleCurrentPoseUpdateNativeLayout.AllocationSize == 0x58
    && VehicleCurrentPoseUpdateNativeLayout.ConstructorFileOffset == 0xB095B0
    && VehicleCurrentPoseUpdateNativeLayout.BaseConstructorHelperFileOffset == 0xB47600
    && VehicleCurrentPoseUpdateNativeLayout.BaseConstructorArgument == 0x56
    && VehicleCurrentPoseUpdateNativeLayout.BodyReaderFileOffset == 0x83A530
    && VehicleCurrentPoseUpdateNativeLayout.ByteReaderFileOffset == 0x337180
    && VehicleCurrentPoseUpdateNativeLayout.WordReaderFileOffset == 0x336AC0
    && VehicleCurrentPoseUpdateNativeLayout.DwordReaderFileOffset == 0x3568E0
    && VehicleCurrentPoseUpdateNativeLayout.ThreeByteReaderFileOffset == 0x336B50
    && VehicleCurrentPoseUpdateNativeLayout.FinalWordReaderFileOffset == 0x6097B0
    && VehicleCurrentPoseUpdateNativeLayout.AlternateByteReaderFileOffset == 0x356CF0
    && VehicleCurrentPoseUpdateNativeLayout.VtableVirtualAddress == 0x1C72958
    && VehicleCurrentPoseUpdateNativeLayout.RttiName == ".?AVCurrentPoseUpdate@Vehicle@firefall@@"
    && VehicleCurrentPoseUpdateNativeLayout.FlagFieldOffset == 0x28
    && VehicleCurrentPoseUpdateNativeLayout.FirstConditionalByteFieldOffset == 0x29
    && VehicleCurrentPoseUpdateNativeLayout.FirstConditionalWordFieldOffset == 0x2A
    && VehicleCurrentPoseUpdateNativeLayout.FollowingByteFieldOffset == 0x2C
    && VehicleCurrentPoseUpdateNativeLayout.SecondConditionalWordFieldOffset == 0x2E
    && VehicleCurrentPoseUpdateNativeLayout.FirstThreeByteFieldOffset == 0x30
    && VehicleCurrentPoseUpdateNativeLayout.FirstDwordRunOffset == 0x34
    && VehicleCurrentPoseUpdateNativeLayout.FirstDwordRunCount == 3
    && VehicleCurrentPoseUpdateNativeLayout.FirstByteUnionOffset == 0x40
    && VehicleCurrentPoseUpdateNativeLayout.FirstWordRunOffset == 0x44
    && VehicleCurrentPoseUpdateNativeLayout.FirstWordRunCount == 4
    && VehicleCurrentPoseUpdateNativeLayout.SecondThreeByteFieldOffset == 0x4C
    && VehicleCurrentPoseUpdateNativeLayout.FinalWordRunOffset == 0x50
    && VehicleCurrentPoseUpdateNativeLayout.FinalWordRunCount == 3
    && VehicleCurrentPoseUpdateNativeLayout.FirstByteChoiceFlag == 0x01
    && VehicleCurrentPoseUpdateNativeLayout.SecondWordChoiceFlag == 0x02
    && VehicleCurrentPoseUpdateNativeLayout.ThreeByteChoiceFlags == 0x0C
    && VehicleCurrentPoseUpdateNativeLayout.FirstUnionChoiceFlags == 0x30
    && VehicleCurrentPoseUpdateNativeLayout.FinalThreeByteChoiceFlags == 0xC0);
Check("Vehicle TookDebugWeaponHitPublic route-index-3 switch-entry 4 layout is recorded",
    VehicleTookDebugWeaponHitPublicNativeLayout.SwitchEntryIndex == 4
    && VehicleTookDebugWeaponHitPublicNativeLayout.DispatcherValue + VehicleTookDebugWeaponHitPublicNativeLayout.SwitchArgumentAdjustment
        == VehicleTookDebugWeaponHitPublicNativeLayout.SwitchEntryIndex
    && VehicleTookDebugWeaponHitPublicNativeLayout.DispatcherValue == 0x57
    && VehicleTookDebugWeaponHitPublicNativeLayout.CaseFileOffset == 0x81F0A8
    && VehicleTookDebugWeaponHitPublicNativeLayout.CaseLocalDwordValue == 0xD4
    && VehicleTookDebugWeaponHitPublicNativeLayout.AllocationSize == 0xA8
    && VehicleTookDebugWeaponHitPublicNativeLayout.ConstructorFileOffset == 0xB09630
    && VehicleTookDebugWeaponHitPublicNativeLayout.BaseConstructorHelperFileOffset == 0xB47680
    && VehicleTookDebugWeaponHitPublicNativeLayout.BaseConstructorArgument == 0x57
    && VehicleTookDebugWeaponHitPublicNativeLayout.BodyReaderFileOffset == 0x5F60F0
    && VehicleTookDebugWeaponHitPublicNativeLayout.FirstConditionalReaderFileOffset == 0x5F4FD0
    && VehicleTookDebugWeaponHitPublicNativeLayout.FirstConditionalValueReaderFileOffset == 0x5F4D60
    && VehicleTookDebugWeaponHitPublicNativeLayout.FirstConditionalCopyHelperFileOffset == 0x5F4F60
    && VehicleTookDebugWeaponHitPublicNativeLayout.SecondConditionalReaderFileOffset == 0x5F5FB0
    && VehicleTookDebugWeaponHitPublicNativeLayout.SecondConditionalValueReaderFileOffset == 0x5F5420
    && VehicleTookDebugWeaponHitPublicNativeLayout.SecondConditionalCopyHelperFileOffset == 0x5F5EB0
    && VehicleTookDebugWeaponHitPublicNativeLayout.ScalarDwordReaderFileOffset == 0x3568E0
    && VehicleTookDebugWeaponHitPublicNativeLayout.QwordReaderFileOffset == 0x356F80
    && VehicleTookDebugWeaponHitPublicNativeLayout.FourValueReaderFileOffset == 0x384B40
    && VehicleTookDebugWeaponHitPublicNativeLayout.VtableVirtualAddress == 0x1C72968
    && VehicleTookDebugWeaponHitPublicNativeLayout.RttiName == ".?AVTookDebugWeaponHitPublic@Vehicle@firefall@@"
    && VehicleTookDebugWeaponHitPublicNativeLayout.FirstByteFieldOffset == 0x28
    && VehicleTookDebugWeaponHitPublicNativeLayout.FirstDwordFieldOffset == 0x2C
    && VehicleTookDebugWeaponHitPublicNativeLayout.SecondDwordFieldOffset == 0x30
    && VehicleTookDebugWeaponHitPublicNativeLayout.WordFieldOffset == 0x34
    && VehicleTookDebugWeaponHitPublicNativeLayout.FirstDwordRunOffset == 0x38
    && VehicleTookDebugWeaponHitPublicNativeLayout.FirstDwordRunCount == 6
    && VehicleTookDebugWeaponHitPublicNativeLayout.FollowingDwordFieldOffset == 0x50
    && VehicleTookDebugWeaponHitPublicNativeLayout.FirstConditionalFlagOffset == 0x58
    && VehicleTookDebugWeaponHitPublicNativeLayout.SecondConditionalFlagOffset == 0x88
    && VehicleTookDebugWeaponHitPublicNativeLayout.ConditionalRecordStride == 0x40);
Check("Vehicle ForcedMovement route-index-3 switch-entry 5 layout is recorded",
    VehicleForcedMovementNativeLayout.SwitchEntryIndex == 5
    && VehicleForcedMovementNativeLayout.DispatcherValue + VehicleForcedMovementNativeLayout.SwitchArgumentAdjustment
        == VehicleForcedMovementNativeLayout.SwitchEntryIndex
    && VehicleForcedMovementNativeLayout.DispatcherValue == 0x58
    && VehicleForcedMovementNativeLayout.CaseFileOffset == 0x81F0DC
    && VehicleForcedMovementNativeLayout.CaseLocalDwordValue == 0xD5
    && VehicleForcedMovementNativeLayout.AllocationSize == 0x1E0
    && VehicleForcedMovementNativeLayout.ConstructorFileOffset == 0xB09710
    && VehicleForcedMovementNativeLayout.BaseConstructorHelperFileOffset == 0xB476F0
    && VehicleForcedMovementNativeLayout.BaseConstructorArgument == 0x58
    && VehicleForcedMovementNativeLayout.NestedStateInitializationHelperFileOffset == 0x92B8A0
    && VehicleForcedMovementNativeLayout.BodyReaderFileOffset == 0x92F800
    && VehicleForcedMovementNativeLayout.DwordReaderFileOffset == 0x3370A0
    && VehicleForcedMovementNativeLayout.ByteReaderFileOffset == 0x337180
    && VehicleForcedMovementNativeLayout.ThreeDwordReaderFileOffset == 0x384C80
    && VehicleForcedMovementNativeLayout.FourValueReaderFileOffset == 0x384C20
    && VehicleForcedMovementNativeLayout.ScalarDwordReaderFileOffset == 0x3568E0
    && VehicleForcedMovementNativeLayout.QwordReaderFileOffset == 0x356F80
    && VehicleForcedMovementNativeLayout.OptionalQwordReaderFileOffset == 0x5FDA00
    && VehicleForcedMovementNativeLayout.VtableVirtualAddress == 0x1C72938
    && VehicleForcedMovementNativeLayout.RttiName == ".?AVForcedMovement@Vehicle@firefall@@"
    && VehicleForcedMovementNativeLayout.NestedStateOffset == 0x28
    && VehicleForcedMovementNativeLayout.NestedStateSize == 0x1B8
    && VehicleForcedMovementNativeLayout.StateDiscriminatorObjectOffset == 0x30
    && VehicleForcedMovementNativeLayout.StateSwitchTableVirtualAddress == 0xD308BC
    && VehicleForcedMovementNativeLayout.StateSwitchTableFileOffset == 0x92FCBC
    && VehicleForcedMovementNativeLayout.StateSwitchCaseCount == 14
    && VehicleForcedMovementNativeLayout.StateSwitchCaseTargetFileOffsets.SequenceEqual(new uint[]
    {
        0x92FC83, 0x92F8E8, 0x92F92E, 0x92F990, 0x92FA16, 0x92FA5E, 0x92FA94,
        0x92FBCF, 0x92FAC6, 0x92FAF8, 0x92FB32, 0x92FB87, 0x92FBC1, 0x92FBED
    }));
Check("Vehicle FlipPunch route-index-3 switch-entry 7 layout is recorded",
    VehicleFlipPunchNativeLayout.SwitchEntryIndex == 7
    && VehicleFlipPunchNativeLayout.DispatcherValue + VehicleFlipPunchNativeLayout.SwitchArgumentAdjustment
        == VehicleFlipPunchNativeLayout.SwitchEntryIndex
    && VehicleFlipPunchNativeLayout.DispatcherValue == 0x5A
    && VehicleFlipPunchNativeLayout.CaseFileOffset == 0x81F161
    && VehicleFlipPunchNativeLayout.CaseLocalDwordValue == 0xD7
    && VehicleFlipPunchNativeLayout.AllocationSize == 0x40
    && VehicleFlipPunchNativeLayout.ConstructorFileOffset == 0xB09820
    && VehicleFlipPunchNativeLayout.BaseConstructorHelperFileOffset == 0xB477D0
    && VehicleFlipPunchNativeLayout.BaseConstructorArgument == 0x5A
    && VehicleFlipPunchNativeLayout.BodyReaderFileOffset == 0x83A370
    && VehicleFlipPunchNativeLayout.DwordReaderFileOffset == 0x3568E0
    && VehicleFlipPunchNativeLayout.VtableVirtualAddress == 0x1C72948
    && VehicleFlipPunchNativeLayout.RttiName == ".?AVFlipPunch@Vehicle@firefall@@"
    && VehicleFlipPunchNativeLayout.FirstDwordFieldOffset == 0x28
    && VehicleFlipPunchNativeLayout.DwordFieldCount == 6);
Check("Vehicle DebugMovementUpdate route-index-3 switch-entry 8 layout is recorded",
    VehicleDebugMovementUpdateNativeLayout.SwitchEntryIndex == 8
    && VehicleDebugMovementUpdateNativeLayout.DispatcherValue + VehicleDebugMovementUpdateNativeLayout.SwitchArgumentAdjustment
        == VehicleDebugMovementUpdateNativeLayout.SwitchEntryIndex
    && VehicleDebugMovementUpdateNativeLayout.DispatcherValue == 0x5B
    && VehicleDebugMovementUpdateNativeLayout.CaseFileOffset == 0x81F1B2
    && VehicleDebugMovementUpdateNativeLayout.CaseLocalDwordValue == 0xD8
    && VehicleDebugMovementUpdateNativeLayout.AllocationSize == 0xA0
    && VehicleDebugMovementUpdateNativeLayout.ConstructorFileOffset == 0xB098A0
    && VehicleDebugMovementUpdateNativeLayout.BaseConstructorHelperFileOffset == 0xB47840
    && VehicleDebugMovementUpdateNativeLayout.BaseConstructorArgument == 0x5B
    && VehicleDebugMovementUpdateNativeLayout.BodyReaderFileOffset == 0x83A230
    && VehicleDebugMovementUpdateNativeLayout.NestedBodyReaderFileOffset == 0x608370
    && VehicleDebugMovementUpdateNativeLayout.FourValueReaderFileOffset == 0x384B40
    && VehicleDebugMovementUpdateNativeLayout.VtableVirtualAddress == 0x1C728F8
    && VehicleDebugMovementUpdateNativeLayout.RttiName == ".?AVDebugMovementUpdate@Vehicle@firefall@@"
    && VehicleDebugMovementUpdateNativeLayout.FirstByteFieldOffset == 0x28
    && VehicleDebugMovementUpdateNativeLayout.NestedReadFieldOffset == 0x2C
    && VehicleDebugMovementUpdateNativeLayout.FirstNestedDwordOffset == 0x2C
    && VehicleDebugMovementUpdateNativeLayout.NestedDwordCount == 3
    && VehicleDebugMovementUpdateNativeLayout.FourValueInputOffset == 0x38
    && VehicleDebugMovementUpdateNativeLayout.FourValueInputSize == 0x10
    && VehicleDebugMovementUpdateNativeLayout.FourValueCopyOffset == 0x78
    && VehicleDebugMovementUpdateNativeLayout.ConstructorSeededWordOffset == 0x88
    && VehicleDebugMovementUpdateNativeLayout.ConstructorSeededWordValue == 0x0101
    && VehicleDebugMovementUpdateNativeLayout.NestedReaderWordOffset == 0x88
    && VehicleDebugMovementUpdateNativeLayout.FinalDwordRunOffset == 0x8C
    && VehicleDebugMovementUpdateNativeLayout.FinalDwordRunCount == 3
    && VehicleDebugMovementUpdateNativeLayout.TrailingWordFieldOffset == 0x98
    && VehicleDebugMovementUpdateNativeLayout.TrailingDwordFieldOffset == 0x9C);
Check("Deployable AbilityProjectileFired route-index-4 value0x54 layout is recorded",
    DeployableAbilityProjectileFiredNativeLayout.DispatcherValue == 0x54
    && DeployableAbilityProjectileFiredNativeLayout.CaseFileOffset == 0x81F24D
    && DeployableAbilityProjectileFiredNativeLayout.CaseLocalDwordValue == 0xDA
    && DeployableAbilityProjectileFiredNativeLayout.AllocationSize == 0x68
    && DeployableAbilityProjectileFiredNativeLayout.ConstructorFileOffset == 0xB27F40
    && DeployableAbilityProjectileFiredNativeLayout.BaseConstructorHelperFileOffset == 0xB47CB0
    && DeployableAbilityProjectileFiredNativeLayout.BaseConstructorArgument == 0x54
    && DeployableAbilityProjectileFiredNativeLayout.BodyReaderFileOffset == 0x839F50
    && DeployableAbilityProjectileFiredNativeLayout.VtableVirtualAddress == 0x1C72764
    && DeployableAbilityProjectileFiredNativeLayout.RttiName == ".?AVAbilityProjectileFired@Deployable@firefall@@"
    && DeployableAbilityProjectileFiredNativeLayout.FirstWordFieldOffset == 0x28
    && DeployableAbilityProjectileFiredNativeLayout.EighthWordFieldOffset == 0x36
    && DeployableAbilityProjectileFiredNativeLayout.FirstDwordFieldOffset == 0x38
    && DeployableAbilityProjectileFiredNativeLayout.ConditionalQwordFlagOffset == 0x58
    && DeployableAbilityProjectileFiredNativeLayout.ConditionalQwordValueOffset == 0x60);
Check("Deployable PublicCombatLog route-index-4 value0x55 records versioned word-length-prefixed blob",
    DeployablePublicCombatLogNativeLayout.DispatcherValue == 0x55
    && DeployablePublicCombatLogNativeLayout.CaseFileOffset == 0x81F21C
    && DeployablePublicCombatLogNativeLayout.CaseLocalDwordValue == 0xDB
    && DeployablePublicCombatLogNativeLayout.AllocationSize == 0x40
    && DeployablePublicCombatLogNativeLayout.ConstructorFileOffset == 0xB27FE0
    && DeployablePublicCombatLogNativeLayout.BaseConstructorHelperFileOffset == 0xB47D20
    && DeployablePublicCombatLogNativeLayout.BaseConstructorArgument == 0x55
    && DeployablePublicCombatLogNativeLayout.BodyReaderFileOffset == 0x5F3350
    && DeployablePublicCombatLogNativeLayout.WordLengthPrefixedBlobReaderFileOffset == 0x12EE170
    && DeployablePublicCombatLogNativeLayout.VtableVirtualAddress == 0x1C72754
    && DeployablePublicCombatLogNativeLayout.RttiName == ".?AVPublicCombatLog@Deployable@firefall@@"
    && DeployablePublicCombatLogNativeLayout.EmbeddedBlobVtableVirtualAddress == 0x1C80CC4
    && DeployablePublicCombatLogNativeLayout.RequiredBodyDword == 1
    && DeployablePublicCombatLogNativeLayout.RequiredBodyDwordWidth == 4
    && DeployablePublicCombatLogNativeLayout.BlobLengthPrefixWidth == 2
    && DeployablePublicCombatLogNativeLayout.BlobFieldOffset == 0x2C);
Check("MarketRequestComplete selector 0x53 preserves its RTTI and native body field offsets",
    CharacterMarketRequestCompleteNativeLayout.InnerSelector == 0x53
    && CharacterMarketRequestCompleteNativeLayout.MessageDiscriminator == 0x53
    && CharacterMarketRequestCompleteNativeLayout.MessageDiscriminatorFieldOffset == 0x10
    && CharacterMarketRequestCompleteNativeLayout.CaseFileOffset == 0x81CCC3
    && CharacterMarketRequestCompleteNativeLayout.AllocationSize == 0x48
    && CharacterMarketRequestCompleteNativeLayout.ConstructorFileOffset == 0x524C30
    && CharacterMarketRequestCompleteNativeLayout.ConstructorBaseConstructorCallFileOffset == 0x524C48
    && CharacterMarketRequestCompleteNativeLayout.BaseConstructorHelperFileOffset == 0x5A0400
    && CharacterMarketRequestCompleteNativeLayout.BaseConstructorArgument == 0x53
    && CharacterMarketRequestCompleteNativeLayout.BaseMetadataStringVirtualAddress == 0x1B5BEAC
    && CharacterMarketRequestCompleteNativeLayout.BodyReaderFileOffset == 0x5F6B10
    && CharacterMarketRequestCompleteNativeLayout.QwordReaderFileOffset == 0x356F80
    && CharacterMarketRequestCompleteNativeLayout.MessagePointerEaxRestoreFileOffset == 0x81CCFF
    && CharacterMarketRequestCompleteNativeLayout.VtableVirtualAddress == 0x1C67278
    && CharacterMarketRequestCompleteNativeLayout.FirstByteFieldOffset == 0x28
    && CharacterMarketRequestCompleteNativeLayout.RawQwordFieldOffset == 0x30
    && CharacterMarketRequestCompleteNativeLayout.SecondByteFieldOffset == 0x38
    && CharacterMarketRequestCompleteNativeLayout.FirstSharedTextFieldOffset == 0x3C
    && CharacterMarketRequestCompleteNativeLayout.SecondSharedTextFieldOffset == 0x40
    && CharacterMarketRequestCompleteNativeLayout.ThirdSharedTextFieldOffset == 0x44);
Check("Character MarketRequestComplete's selector-0x53 consumer aliases its raw QWORD high dword",
    CharacterMarketRequestCompleteNativeLayout.MessageDiscriminator == 0x53
    && CharacterMarketRequestCompleteNativeLayout.RawQwordFieldOffset + sizeof(uint)
        == MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerFilterMessageFieldOffset
    && CharacterMarketRequestCompleteNativeLayout.RawQwordFieldOffset + sizeof(uint)
        < CharacterMarketRequestCompleteNativeLayout.AllocationSize
    && CharacterMarketRequestCompleteNativeLayout.FirstByteFieldOffset == 0x28);
Check("ReceiveWeaponTweaks selector 0x54 preserves its RTTI and raw body field offsets",
    CharacterReceiveWeaponTweaksNativeLayout.InnerSelector == 0x54
    && CharacterReceiveWeaponTweaksNativeLayout.CaseFileOffset == 0x81CD14
    && CharacterReceiveWeaponTweaksNativeLayout.AllocationSize == 0x40
    && CharacterReceiveWeaponTweaksNativeLayout.ConstructorFileOffset == 0x524DF0
    && CharacterReceiveWeaponTweaksNativeLayout.BodyReaderFileOffset == 0x5F68D0
    && CharacterReceiveWeaponTweaksNativeLayout.VtableVirtualAddress == 0x1C67268
    && CharacterReceiveWeaponTweaksNativeLayout.FirstByteFieldOffset == 0x28
    && CharacterReceiveWeaponTweaksNativeLayout.SecondByteFieldOffset == 0x29
    && CharacterReceiveWeaponTweaksNativeLayout.TrailingBlobFieldOffset == 0x2C
    && CharacterReceiveWeaponTweaksNativeLayout.TrailingBlobVtableVirtualAddress == 0x1C80CC4
    && CharacterReceiveWeaponTweaksNativeLayout.TrailingBlobCopyMethodFileOffset == 0x12CB9D0
    && CharacterReceiveWeaponTweaksNativeLayout.TrailingBlobResizeMethodFileOffset == 0x12CBCB0);
Check("ReceiveWeaponTweaks selector 0x54 records the conditional event consumer field overlaps",
    CharacterReceiveWeaponTweaksConsumerNativeLayout.MessageDiscriminator
        == CharacterReceiveWeaponTweaksNativeLayout.InnerSelector
    && CharacterReceiveWeaponTweaksConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0xD4
    && CharacterReceiveWeaponTweaksConsumerNativeLayout.ProtocolHandlerMethodVirtualAddress == 0xECB3E0
    && CharacterReceiveWeaponTweaksConsumerNativeLayout.ProtocolHandlerMethodFileOffset == 0xACA7E0
    && CharacterReceiveWeaponTweaksConsumerNativeLayout.ContentTypeMessageFieldOffset
        == CharacterReceiveWeaponTweaksNativeLayout.FirstByteFieldOffset
    && CharacterReceiveWeaponTweaksConsumerNativeLayout.ContentIdMessageFieldOffset
        == CharacterReceiveWeaponTweaksNativeLayout.TrailingBlobFieldOffset
    && CharacterReceiveWeaponTweaksConsumerNativeLayout.DurationMessageFieldOffset
        == CharacterReceiveWeaponTweaksConsumerNativeLayout.ContentIdMessageFieldOffset + sizeof(uint)
    && CharacterReceiveWeaponTweaksConsumerNativeLayout.PayloadEntrySize == 8
    && CharacterReceiveWeaponTweaksConsumerNativeLayout.PayloadReservedEntryCount == 8
    && CharacterReceiveWeaponTweaksConsumerNativeLayout.EventSinkVtableSlotOffset == 0xAC
    && CharacterReceiveWeaponTweaksConsumerNativeLayout.ApplicationEventCode == 130);
Check("TookDebugWeaponHitPublic selector 0x55 records its scalar prefix and conditional-tail boundaries",
    CharacterTookDebugWeaponHitPublicNativeLayout.InnerSelector == 0x55
    && CharacterTookDebugWeaponHitPublicNativeLayout.CaseFileOffset == 0x81CD65
    && CharacterTookDebugWeaponHitPublicNativeLayout.CaseLocalDwordValue == 0x5D
    && CharacterTookDebugWeaponHitPublicNativeLayout.AllocationSize == 0xA8
    && CharacterTookDebugWeaponHitPublicNativeLayout.ConstructorFileOffset == 0x524EE0
    && CharacterTookDebugWeaponHitPublicNativeLayout.BodyReaderFileOffset == 0x5F60F0
    && CharacterTookDebugWeaponHitPublicNativeLayout.VtableVirtualAddress == 0x1C67258
    && CharacterTookDebugWeaponHitPublicNativeLayout.FirstByteFieldOffset == 0
    && CharacterTookDebugWeaponHitPublicNativeLayout.RawWordFieldOffset == 0x0C
    && CharacterTookDebugWeaponHitPublicNativeLayout.RawDwordFieldOffsets.SequenceEqual(
        new[] { 0x04, 0x08, 0x10, 0x14, 0x18, 0x1C, 0x20, 0x24, 0x28 })
    && CharacterTookDebugWeaponHitPublicNativeLayout.FixedPrefixByteCount == 0x27
    && CharacterTookDebugWeaponHitPublicNativeLayout.FirstConditionalTailMarkerOffset == 0x30
    && CharacterTookDebugWeaponHitPublicNativeLayout.FirstConditionalTailPayloadByteCount == 0x26
    && CharacterTookDebugWeaponHitPublicNativeLayout.FirstConditionalTailFourFloatGroupOffset == 0x4C
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailMarkerOffset == 0x60
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailCopiedPrefixOffset == 0x68
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailCopiedPrefixDwordCount == 3
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailVectorBeginOffset == 0x74
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailVectorEndOffset == 0x78
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailVectorCapacityOffset == 0x7C
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailRecordByteCount == 0x40
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailRecordDwordCount == 16
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailCountRepeatMarker == 0xFF
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailTemporaryPrefixSize == 0x0C
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailQwordReadByteCount == 8
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailTemporaryThirdDwordOffset == 8
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailQwordReaderFileOffset == 0x356F80
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailStateTransferFileOffset == 0x5F5EB0
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailVectorReaderFileOffset == 0x5F5420
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailCopiedPrefixThirdDwordObjectOffset == 0x70
    && CharacterTookDebugWeaponHitPublicNativeLayout.SecondConditionalTailReaderByteCount == 0x121);
Check("TookDebugWeaponHitPublic selector 0x55 records its conditional SharedText consumer",
    CharacterTookDebugWeaponHitPublicConsumerNativeLayout.MessageDiscriminator
        == CharacterTookDebugWeaponHitPublicNativeLayout.InnerSelector
    && CharacterTookDebugWeaponHitPublicConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0xD8
    && CharacterTookDebugWeaponHitPublicConsumerNativeLayout.ProtocolHandlerMethodVirtualAddress == 0xECB4E0
    && CharacterTookDebugWeaponHitPublicConsumerNativeLayout.ProtocolHandlerMethodFileOffset == 0xACA8E0
    && CharacterTookDebugWeaponHitPublicConsumerNativeLayout.SharedTextMessageFieldOffset == 0x28
    && CharacterTookDebugWeaponHitPublicConsumerNativeLayout.SharedTextEventHelperFileOffset == 0x8EDBD0
    && CharacterTookDebugWeaponHitPublicConsumerNativeLayout.EventSinkVtableSlotOffset == 0xAC
    && CharacterTookDebugWeaponHitPublicConsumerNativeLayout.ApplicationEventCode == 197);
Check("TookDebugWeaponHit selector 0x56 has a distinct RTTI type and shares selector 0x55's body reader",
    CharacterTookDebugWeaponHitNativeLayout.InnerSelector == 0x56
    && CharacterTookDebugWeaponHitNativeLayout.CaseFileOffset == 0x81CDBA
    && CharacterTookDebugWeaponHitNativeLayout.CaseLocalDwordValue == 0x5E
    && CharacterTookDebugWeaponHitNativeLayout.AllocationSize == 0xA8
    && CharacterTookDebugWeaponHitNativeLayout.ConstructorFileOffset == 0x524FC0
    && CharacterTookDebugWeaponHitNativeLayout.ConstructorHelperFileOffset == 0x5A0550
    && CharacterTookDebugWeaponHitNativeLayout.BodyReaderFileOffset
        == CharacterTookDebugWeaponHitPublicNativeLayout.BodyReaderFileOffset
    && CharacterTookDebugWeaponHitNativeLayout.VtableVirtualAddress == 0x1C67248
    && CharacterTookDebugWeaponHitNativeLayout.SharedBodyReaderSetupFileOffset == 0x81CD94
    && CharacterTookDebugWeaponHitNativeLayout.ConstructorZeroedByteField1Offset == 0x58
    && CharacterTookDebugWeaponHitNativeLayout.ConstructorZeroedByteField2Offset == 0x88);
Check("TookDebugWeaponHit selector 0x56 records its mapped consumer and collection/event offsets",
    CharacterTookDebugWeaponHitConsumerNativeLayout.MessageDiscriminator
        == CharacterTookDebugWeaponHitNativeLayout.InnerSelector
    && CharacterTookDebugWeaponHitConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0xDC
    && CharacterTookDebugWeaponHitConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECEA30
    && CharacterTookDebugWeaponHitConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACDE30
    && CharacterTookDebugWeaponHitConsumerNativeLayout.ConsumerFunctionFileOffset == 0xACDE30
    && CharacterTookDebugWeaponHitConsumerNativeLayout.OwnerStatePointerOffset == 0x43D0
    && CharacterTookDebugWeaponHitConsumerNativeLayout.OwnerTreeOffset == 0x30
    && CharacterTookDebugWeaponHitConsumerNativeLayout.InputCollectionPointerOffset == 0x28
    && CharacterTookDebugWeaponHitConsumerNativeLayout.InputCollectionCountOffset == 0x2C
    && CharacterTookDebugWeaponHitConsumerNativeLayout.CollectionMergeHelperFileOffset == 0x927F60
    && CharacterTookDebugWeaponHitConsumerNativeLayout.EventSinkVtableSlotOffset == 0xAC
    && CharacterTookDebugWeaponHitConsumerNativeLayout.ApplicationEventCode == 222);
Check("DebugWeaponStats selector 0x57 records eight raw WORD fields",
    CharacterDebugWeaponStatsNativeLayout.InnerSelector == 0x57
    && CharacterDebugWeaponStatsNativeLayout.CaseFileOffset == 0x81CDE7
    && CharacterDebugWeaponStatsNativeLayout.CaseLocalDwordValue == 0x5F
    && CharacterDebugWeaponStatsNativeLayout.AllocationSize == 0x38
    && CharacterDebugWeaponStatsNativeLayout.ConstructorFileOffset == 0x5250A0
    && CharacterDebugWeaponStatsNativeLayout.BodyReaderFileOffset == 0x5F4A10
    && CharacterDebugWeaponStatsNativeLayout.VtableVirtualAddress == 0x1C67238
    && CharacterDebugWeaponStatsNativeLayout.FirstWordFieldOffset == 0x28
    && CharacterDebugWeaponStatsNativeLayout.WordFieldCount == 8
    && CharacterDebugWeaponStatsNativeLayout.WordFieldOffsets.SequenceEqual(
        new[] { 0x28, 0x2A, 0x2C, 0x2E, 0x30, 0x32, 0x34, 0x36 }));
Check("DebugWeaponStats selector 0x57 records its consumer, vector range, and event",
    CharacterDebugWeaponStatsConsumerNativeLayout.MessageDiscriminator
        == CharacterDebugWeaponStatsNativeLayout.InnerSelector
    && CharacterDebugWeaponStatsConsumerNativeLayout.ProtocolHandlerDispatchTableVirtualAddress == 0xC1D6DC
    && CharacterDebugWeaponStatsConsumerNativeLayout.ProtocolHandlerDispatchTableSelectorBias == 0x20
    && CharacterDebugWeaponStatsConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0xE0
    && CharacterDebugWeaponStatsConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECEF10
    && CharacterDebugWeaponStatsConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACE310
    && CharacterDebugWeaponStatsConsumerNativeLayout.ConsumerFunctionFileOffset == 0xACE310
    && CharacterDebugWeaponStatsConsumerNativeLayout.OwnerStatePointerOffset == 0x43D0
    && CharacterDebugWeaponStatsConsumerNativeLayout.OwnerVectorOffset == 0x90
    && CharacterDebugWeaponStatsConsumerNativeLayout.InputRangeBeginOffset == 0x28
    && CharacterDebugWeaponStatsConsumerNativeLayout.InputRangeEndOffset == 0x2C
    && CharacterDebugWeaponStatsConsumerNativeLayout.VectorAssignHelperFileOffset == 0x929920
    && CharacterDebugWeaponStatsConsumerNativeLayout.EventSinkVtableSlotOffset == 0xAC
    && CharacterDebugWeaponStatsConsumerNativeLayout.ApplicationEventCode == 227);
Check("RewardInfo selector 0x58 records its byte, QWORD, and DWORD fields",
    CharacterRewardInfoNativeLayout.InnerSelector == 0x58
    && CharacterRewardInfoNativeLayout.CaseFileOffset == 0x81CE38
    && CharacterRewardInfoNativeLayout.CaseLocalDwordValue == 0x60
    && CharacterRewardInfoNativeLayout.AllocationSize == 0x48
    && CharacterRewardInfoNativeLayout.ConstructorFileOffset == 0x525120
    && CharacterRewardInfoNativeLayout.BodyReaderFileOffset == 0x5F47C0
    && CharacterRewardInfoNativeLayout.VtableVirtualAddress == 0x1C67218
    && CharacterRewardInfoNativeLayout.FirstByteFieldOffset == 0x28
    && CharacterRewardInfoNativeLayout.SecondByteFieldOffset == 0x29
    && CharacterRewardInfoNativeLayout.RawQwordFieldOffset == 0x30
    && CharacterRewardInfoNativeLayout.RawDwordFieldOffsets.SequenceEqual(new[] { 0x38, 0x3C, 0x40 })
    && CharacterRewardInfoNativeLayout.NominalInputByteCount == 0x16
    && CharacterRewardInfoNativeLayout.ReaderLastWrittenFieldEndOffset == 0x44
    && CharacterRewardInfoNativeLayout.ConstructorZeroedDwordField1Offset == 0x30
    && CharacterRewardInfoNativeLayout.ConstructorZeroedDwordField2Offset == 0x34);
Check("RewardInfo selector 0x58 records its state update and conditional follow-up",
    CharacterRewardInfoConsumerNativeLayout.MessageDiscriminator
        == CharacterRewardInfoNativeLayout.InnerSelector
    && CharacterRewardInfoConsumerNativeLayout.ProtocolHandlerDispatchTableVirtualAddress == 0xC1D6DC
    && CharacterRewardInfoConsumerNativeLayout.ProtocolHandlerDispatchTableSelectorBias == 0x20
    && CharacterRewardInfoConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0xE4
    && CharacterRewardInfoConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECB580
    && CharacterRewardInfoConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACA980
    && CharacterRewardInfoConsumerNativeLayout.ConsumerFunctionFileOffset == 0xACA980
    && CharacterRewardInfoConsumerNativeLayout.OwnerStatePointerOffset == 0x43C8
    && CharacterRewardInfoConsumerNativeLayout.InputValueFieldOffset == 0x28
    && CharacterRewardInfoConsumerNativeLayout.StateUpdateHelperFileOffset == 0x909120
    && CharacterRewardInfoConsumerNativeLayout.ConditionalFollowupFileOffset == 0x9092A0
    && CharacterRewardInfoConsumerNativeLayout.ConditionalRequestMethodFileOffset == 0x909980
    && CharacterRewardInfoConsumerNativeLayout.EventSinkVtableSlotOffset == 0xAC
    && CharacterRewardInfoConsumerNativeLayout.ConditionalApplicationEventCode == 219
    && CharacterRewardInfoConsumerNativeLayout.MissingOwnerDiagnosticCode == 1651);
Check("ProgressionXpRefresh selector 0x59 records its count-blocked 20-byte vector",
    CharacterProgressionXpRefreshNativeLayout.InnerSelector == 0x59
    && CharacterProgressionXpRefreshNativeLayout.CaseFileOffset == 0x81CE89
    && CharacterProgressionXpRefreshNativeLayout.CaseLocalDwordValue == 0x61
    && CharacterProgressionXpRefreshNativeLayout.AllocationSize == 0x38
    && CharacterProgressionXpRefreshNativeLayout.ConstructorFileOffset == 0x5251B0
    && CharacterProgressionXpRefreshNativeLayout.BodyReaderFileOffset == 0x5F4170
    && CharacterProgressionXpRefreshNativeLayout.RecordAppendFileOffset == 0x5200F0
    && CharacterProgressionXpRefreshNativeLayout.VtableVirtualAddress == 0x1C67228
    && CharacterProgressionXpRefreshNativeLayout.VectorBeginOffset == 0x28
    && CharacterProgressionXpRefreshNativeLayout.VectorEndOffset == 0x2C
    && CharacterProgressionXpRefreshNativeLayout.VectorCapacityOffset == 0x30
    && CharacterProgressionXpRefreshNativeLayout.RecordDwordCount == 5
    && CharacterProgressionXpRefreshNativeLayout.RecordByteCount == 0x14
    && CharacterProgressionXpRefreshNativeLayout.CountFieldByteCount == 1
    && CharacterProgressionXpRefreshNativeLayout.CountBlockRepeatMarker == 0xFF);
Check("ProgressionXpRefresh selector 0x59 records its state-tree clearing consumer",
    CharacterProgressionXpRefreshConsumerNativeLayout.MessageDiscriminator
        == CharacterProgressionXpRefreshNativeLayout.InnerSelector
    && CharacterProgressionXpRefreshConsumerNativeLayout.ProtocolHandlerDispatchTableVirtualAddress == 0xC1D6DC
    && CharacterProgressionXpRefreshConsumerNativeLayout.ProtocolHandlerDispatchTableSelectorBias == 0x20
    && CharacterProgressionXpRefreshConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0xE8
    && CharacterProgressionXpRefreshConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECB5D0
    && CharacterProgressionXpRefreshConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACA9D0
    && CharacterProgressionXpRefreshConsumerNativeLayout.ConsumerFunctionFileOffset == 0xACA9D0
    && CharacterProgressionXpRefreshConsumerNativeLayout.OwnerStatePointerOffset == 0x43C8
    && CharacterProgressionXpRefreshConsumerNativeLayout.OwnerTreeSentinelPointerOffset == 0
    && CharacterProgressionXpRefreshConsumerNativeLayout.OwnerTreeSentinelSizeOffset == 4
    && CharacterProgressionXpRefreshConsumerNativeLayout.OwnerStateFlagOffset == 8
    && CharacterProgressionXpRefreshConsumerNativeLayout.TreeClearHelperFileOffset == 0x90A7B0
    && CharacterProgressionXpRefreshConsumerNativeLayout.MissingOwnerDiagnosticCode == 1651);
Check("ReceivedDeferredXP selector 0x5A records its count-blocked 16-byte vector",
    CharacterReceivedDeferredXPNativeLayout.InnerSelector == 0x5A
    && CharacterReceivedDeferredXPNativeLayout.CaseFileOffset == 0x81CEDC
    && CharacterReceivedDeferredXPNativeLayout.CaseLocalDwordValue == 0x62
    && CharacterReceivedDeferredXPNativeLayout.AllocationSize == 0x38
    && CharacterReceivedDeferredXPNativeLayout.ConstructorFileOffset == 0x5252A0
    && CharacterReceivedDeferredXPNativeLayout.BodyReaderFileOffset == 0x5F3A80
    && CharacterReceivedDeferredXPNativeLayout.VectorGrowthHelperFileOffset == 0x5F3880
    && CharacterReceivedDeferredXPNativeLayout.VtableVirtualAddress == 0x1C67208
    && CharacterReceivedDeferredXPNativeLayout.VectorBeginOffset == 0x28
    && CharacterReceivedDeferredXPNativeLayout.VectorEndOffset == 0x2C
    && CharacterReceivedDeferredXPNativeLayout.VectorCapacityOffset == 0x30
    && CharacterReceivedDeferredXPNativeLayout.RecordDwordCount == 4
    && CharacterReceivedDeferredXPNativeLayout.RecordByteCount == 0x10
    && CharacterReceivedDeferredXPNativeLayout.CountFieldByteCount == 1
    && CharacterReceivedDeferredXPNativeLayout.CountBlockRepeatMarker == 0xFF);
Check("ReceivedDeferredXP selector 0x5A records its conditional control consumer",
    CharacterReceivedDeferredXPConsumerNativeLayout.MessageDiscriminator
        == CharacterReceivedDeferredXPNativeLayout.InnerSelector
    && CharacterReceivedDeferredXPConsumerNativeLayout.ProtocolHandlerDispatchTableVirtualAddress == 0xC1D6DC
    && CharacterReceivedDeferredXPConsumerNativeLayout.ProtocolHandlerDispatchTableSelectorBias == 0x20
    && CharacterReceivedDeferredXPConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0xEC
    && CharacterReceivedDeferredXPConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECB630
    && CharacterReceivedDeferredXPConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACAA30
    && CharacterReceivedDeferredXPConsumerNativeLayout.ConsumerFunctionFileOffset == 0xACAA30
    && CharacterReceivedDeferredXPConsumerNativeLayout.InputMessageFieldOffset == 0x28
    && CharacterReceivedDeferredXPConsumerNativeLayout.ContextAccessorFileOffset == 0x49DCC0
    && CharacterReceivedDeferredXPConsumerNativeLayout.ControlRoutineFileOffset == 0x4BCBD0
    && CharacterReceivedDeferredXPConsumerNativeLayout.ControlRoutineSelector == 9
    && CharacterReceivedDeferredXPConsumerNativeLayout.EventSinkVtableSlotOffset == 0xAC
    && CharacterReceivedDeferredXPConsumerNativeLayout.ConditionalApplicationEventCode == 50);
Check("PublicCombatLog selector 0x5B records its gated blob body without naming its fields",
    CharacterPublicCombatLogNativeLayout.InnerSelector == 0x5B
    && CharacterPublicCombatLogNativeLayout.CaseFileOffset == 0x81CF2F
    && CharacterPublicCombatLogNativeLayout.CaseLocalDwordValue == 0x63
    && CharacterPublicCombatLogNativeLayout.AllocationSize == 0x40
    && CharacterPublicCombatLogNativeLayout.ConstructorFileOffset == 0x525390
    && CharacterPublicCombatLogNativeLayout.BodyReaderFileOffset == 0x5F3350
    && CharacterPublicCombatLogNativeLayout.WordLengthPrefixedBlobReaderFileOffset == 0x12EE170
    && CharacterPublicCombatLogNativeLayout.VtableVirtualAddress == 0x1C671F8
    && CharacterPublicCombatLogNativeLayout.ConstructorZeroedByteFieldOffset == 0x28
    && CharacterPublicCombatLogNativeLayout.EmbeddedBlobFieldOffset == 0x2C
    && CharacterPublicCombatLogNativeLayout.EmbeddedBlobVtableVirtualAddress == 0x1C80CC4
    && CharacterPublicCombatLogNativeLayout.BodyReaderThisObjectOffset == 0x28
    && CharacterPublicCombatLogNativeLayout.BlobDestinationOffsetFromReaderThis == 4
    && CharacterPublicCombatLogNativeLayout.RequiredRawDwordValue == 1
    && CharacterPublicCombatLogNativeLayout.RawDwordWidth == 4
    && CharacterPublicCombatLogNativeLayout.BlobLengthPrefixByteCount == 2
    && CharacterPublicCombatLogNativeLayout.MinimumSuccessBodyPrefixByteCount == 6);
Check("PublicCombatLog selector 0x5B records its conditional receiver branches",
    CharacterPublicCombatLogConsumerNativeLayout.MessageDiscriminator
        == CharacterPublicCombatLogNativeLayout.InnerSelector
    && CharacterPublicCombatLogConsumerNativeLayout.ProtocolHandlerDispatchTableVirtualAddress == 0xC1D6DC
    && CharacterPublicCombatLogConsumerNativeLayout.ProtocolHandlerDispatchTableSelectorBias == 0x20
    && CharacterPublicCombatLogConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0xF0
    && CharacterPublicCombatLogConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECB260
    && CharacterPublicCombatLogConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACA660
    && CharacterPublicCombatLogConsumerNativeLayout.ConsumerFunctionFileOffset == 0xACA660
    && CharacterPublicCombatLogConsumerNativeLayout.MessageEmbeddedQwordOffset == 0x30
    && CharacterPublicCombatLogConsumerNativeLayout.MessageModeByteOffset == 0x38
    && CharacterPublicCombatLogConsumerNativeLayout.OwnerStatePointerOffset == 0x43BC
    && CharacterPublicCombatLogConsumerNativeLayout.OwnerMapSubobjectOffset == 8
    && CharacterPublicCombatLogConsumerNativeLayout.ContextAccessorFileOffset == 0x49DCC0
    && CharacterPublicCombatLogConsumerNativeLayout.EmptyOrSentinelFlaggedHelperFileOffset == 0xB901F0
    && CharacterPublicCombatLogConsumerNativeLayout.EmptyOrSentinelUnflaggedHelperFileOffset == 0xB903B0
    && CharacterPublicCombatLogConsumerNativeLayout.QwordFlaggedHelperFileOffset == 0xB90A30
    && CharacterPublicCombatLogConsumerNativeLayout.QwordUnflaggedSearchHelperFileOffset == 0xB90810
    && CharacterPublicCombatLogConsumerNativeLayout.QwordUnflaggedRangeUpdateHelperFileOffset == 0x36F480
    && CharacterPublicCombatLogConsumerNativeLayout.SentinelLowDwordMask == 0xFFFFFF00
    && CharacterPublicCombatLogConsumerNativeLayout.SentinelLowDwordMaskedValue == 0xFFFFFF00
    && CharacterPublicCombatLogConsumerNativeLayout.SentinelHighDwordValue == 0xFFFFFFFF
    && CharacterPublicCombatLogConsumerNativeLayout.MissingOwnerDiagnosticCode == 1647);
Check("PrivateCombatLog selector 0x5C has distinct RTTI and shares the public-log body reader",
    CharacterPrivateCombatLogNativeLayout.InnerSelector == 0x5C
    && CharacterPrivateCombatLogNativeLayout.CaseFileOffset == 0x81CF81
    && CharacterPrivateCombatLogNativeLayout.CaseLocalDwordValue == 0x64
    && CharacterPrivateCombatLogNativeLayout.AllocationSize == 0x40
    && CharacterPrivateCombatLogNativeLayout.ConstructorFileOffset == 0x525480
    && CharacterPrivateCombatLogNativeLayout.ConstructorHelperFileOffset == 0x5A0810
    && CharacterPrivateCombatLogNativeLayout.BodyReaderFileOffset
        == CharacterPublicCombatLogNativeLayout.BodyReaderFileOffset
    && CharacterPrivateCombatLogNativeLayout.SharedBodyReaderSetupFileOffset == 0x81CF5B
    && CharacterPrivateCombatLogNativeLayout.VtableVirtualAddress == 0x1C671E8
    && CharacterPrivateCombatLogNativeLayout.ConstructorZeroedByteFieldOffset == 0x28
    && CharacterPrivateCombatLogNativeLayout.EmbeddedBlobFieldOffset == 0x2C
    && CharacterPrivateCombatLogNativeLayout.EmbeddedBlobVtableVirtualAddress == 0x1C80CC4
    && CharacterPrivateCombatLogNativeLayout.BodyReaderThisObjectOffset == 0x28
    && CharacterPrivateCombatLogNativeLayout.BlobDestinationOffsetFromReaderThis == 4);
Check("PrivateCombatLog selector 0x5C receiver records its fields and event route",
    CharacterPrivateCombatLogConsumerNativeLayout.MessageDiscriminator == 0x5C
    && CharacterPrivateCombatLogConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0xF4
    && CharacterPrivateCombatLogConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECC460
    && CharacterPrivateCombatLogConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACB860
    && CharacterPrivateCombatLogConsumerNativeLayout.MessageByteFieldOffset == 0x28
    && CharacterPrivateCombatLogConsumerNativeLayout.MessageEmbeddedBlobFieldOffset == 0x2C
    && CharacterPrivateCombatLogConsumerNativeLayout.MessageBlobDataPointerFieldOffset == 0x30
    && CharacterPrivateCombatLogConsumerNativeLayout.MessageWordFieldOffset == 0x34
    && CharacterPrivateCombatLogConsumerNativeLayout.MessageTrailingByteFieldOffset == 0x36
    && CharacterPrivateCombatLogConsumerNativeLayout.EmbeddedBlobLookupHelperFileOffset == 0x391470
    && CharacterPrivateCombatLogConsumerNativeLayout.EmbeddedBlobFallbackHelperFileOffset == 0xC24210
    && CharacterPrivateCombatLogConsumerNativeLayout.SharedTextAppendHelperFileOffset == 0xC23FE0
    && CharacterPrivateCombatLogConsumerNativeLayout.BlobDataVtableCallFileOffset == 0xACB93A
    && CharacterPrivateCombatLogConsumerNativeLayout.ByteFieldLookupCallFileOffset == 0xACB9C3
    && CharacterPrivateCombatLogConsumerNativeLayout.WordFieldConversionHelperFileOffset == 0xC242F0
    && CharacterPrivateCombatLogConsumerNativeLayout.TrailingByteConversionHelperFileOffset == 0xC243E0
    && CharacterPrivateCombatLogConsumerNativeLayout.ApplicationEventSinkVtableSlotOffset == 0xAC
    && CharacterPrivateCombatLogConsumerNativeLayout.ApplicationEventDispatchCallFileOffset == 0xACBA78
    && CharacterPrivateCombatLogConsumerNativeLayout.ApplicationEventCode == 122);
Check("AnimationUpdated selector 0x5D records its WORD and byte fields",
    CharacterAnimationUpdatedNativeLayout.InnerSelector == 0x5D
    && CharacterAnimationUpdatedNativeLayout.CaseFileOffset == 0x81CFAB
    && CharacterAnimationUpdatedNativeLayout.CaseLocalDwordValue == 0x65
    && CharacterAnimationUpdatedNativeLayout.AllocationSize == 0x30
    && CharacterAnimationUpdatedNativeLayout.ConstructorFileOffset == 0x525570
    && CharacterAnimationUpdatedNativeLayout.ConstructorHelperFileOffset == 0x5A0880
    && CharacterAnimationUpdatedNativeLayout.BodyReaderFileOffset == 0x5F30F0
    && CharacterAnimationUpdatedNativeLayout.VtableVirtualAddress == 0x1C671D8
    && CharacterAnimationUpdatedNativeLayout.RawWordFieldOffset == 0x28
    && CharacterAnimationUpdatedNativeLayout.RawByteFieldOffset == 0x2A
    && CharacterAnimationUpdatedNativeLayout.NominalInputByteCount == 3);
Check("AnimationUpdated selector 0x5D records counter extension and queued relay layout",
    CharacterAnimationUpdatedConsumerNativeLayout.MessageDiscriminator == 0x5D
    && CharacterAnimationUpdatedConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0xF8
    && CharacterAnimationUpdatedConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECCB40
    && CharacterAnimationUpdatedConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACBF40
    && CharacterAnimationUpdatedConsumerNativeLayout.HandlerContextPointerOffset == 8
    && CharacterAnimationUpdatedConsumerNativeLayout.ContextCounterPointerOffset == 8
    && CharacterAnimationUpdatedConsumerNativeLayout.ContextCounterValueOffset == 8
    && CharacterAnimationUpdatedConsumerNativeLayout.MessageFirstWordOffset == 0x28
    && CharacterAnimationUpdatedConsumerNativeLayout.MessageSecondWordOffset == 0x2A
    && CharacterAnimationUpdatedConsumerNativeLayout.MessageQueueIndexByteOffset == 0x2C
    && CharacterAnimationUpdatedConsumerNativeLayout.MessageModeByteOffset == 0x2D
    && CharacterAnimationUpdatedConsumerNativeLayout.CounterWrapModulus == 0x10000
    && CharacterAnimationUpdatedConsumerNativeLayout.CounterWrapHalfRange == 0x8000
    && CharacterAnimationUpdatedConsumerNativeLayout.RelayHelperFileOffset == 0x3343B0
    && CharacterAnimationUpdatedConsumerNativeLayout.RelayRecordQueueFileOffset == 0x334460
    && CharacterAnimationUpdatedConsumerNativeLayout.RelayMaximumQueueIndex == 10
    && CharacterAnimationUpdatedConsumerNativeLayout.RelayMaximumMode == 3
    && CharacterAnimationUpdatedConsumerNativeLayout.RelayQueueFlagsOffset == 5
    && CharacterAnimationUpdatedConsumerNativeLayout.RelayQueueAcceptFlagOffset == 6
    && CharacterAnimationUpdatedConsumerNativeLayout.RelayRecordSize == 0x10
    && CharacterAnimationUpdatedConsumerNativeLayout.RelayRecordArrayBaseOffset == 0x50
    && CharacterAnimationUpdatedConsumerNativeLayout.RelayRecordIndexBias == 5
    && CharacterAnimationUpdatedConsumerNativeLayout.RelayQueueCapacity == 0xBB8);
Check("RaiaNPCDebugging selector 0x5E records its NUL-terminated shared-text field",
    CharacterRaiaNpcDebuggingNativeLayout.InnerSelector == 0x5E
    && CharacterRaiaNpcDebuggingNativeLayout.CaseFileOffset == 0x81CFFC
    && CharacterRaiaNpcDebuggingNativeLayout.CaseLocalDwordValue == 0x66
    && CharacterRaiaNpcDebuggingNativeLayout.AllocationSize == 0x30
    && CharacterRaiaNpcDebuggingNativeLayout.ConstructorFileOffset == 0x5255F0
    && CharacterRaiaNpcDebuggingNativeLayout.ConstructorHelperFileOffset == 0x5A08F0
    && CharacterRaiaNpcDebuggingNativeLayout.SharedTextReaderSetupFileOffset == 0x81FB27
    && CharacterRaiaNpcDebuggingNativeLayout.SharedTextReaderFileOffset == 0x12EE010
    && CharacterRaiaNpcDebuggingNativeLayout.VtableVirtualAddress == 0x1C671B8
    && CharacterRaiaNpcDebuggingNativeLayout.SharedTextFieldOffset == 0x28
    && CharacterRaiaNpcDebuggingNativeLayout.SharedTextChunkByteLimit == 0x100
    && CharacterRaiaNpcDebuggingNativeLayout.SharedTextTemporaryBufferByteCount == 0x104
    && CharacterRaiaNpcDebuggingNativeLayout.SharedTextTerminatorByte == 0);
Check("RaiaNPCDebugging selector 0x5E records its receiver's wider WORD reads",
    CharacterRaiaNpcDebuggingConsumerNativeLayout.MessageDiscriminator == 0x5E
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0xFC
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECCBE0
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACBFE0
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.ContextPointerOffset == 8
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.ContextCounterPointerOffset == 8
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.ContextCounterValueOffset == 8
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.FirstMessageWordOffset == 0x28
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.MessageWordWidth == 2
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.MessageWordReadCount == 5
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.LastMessageWordOffset == 0x30
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.ReceiverReadEndOffsetExclusive == 0x32
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.MetricStateGlobalVirtualAddress == 0x1E0004C
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.MetricAppenderFileOffset == 0x481430
    && CharacterRaiaNpcDebuggingConsumerNativeLayout.MetricAppenderCount == 5);
Check("WeaponProjectileFired selector 0x5F records its prefix and optional tail",
    CharacterWeaponProjectileFiredNativeLayout.InnerSelector == 0x5F
    && CharacterWeaponProjectileFiredNativeLayout.CaseFileOffset == 0x81D02D
    && CharacterWeaponProjectileFiredNativeLayout.CaseLocalDwordValue == 0x67
    && CharacterWeaponProjectileFiredNativeLayout.AllocationSize == 0x38
    && CharacterWeaponProjectileFiredNativeLayout.ConstructorFileOffset == 0x525700
    && CharacterWeaponProjectileFiredNativeLayout.ConstructorHelperFileOffset == 0x5A0960
    && CharacterWeaponProjectileFiredNativeLayout.BodyReaderFileOffset == 0x5F2990
    && CharacterWeaponProjectileFiredNativeLayout.OptionalTailReaderFileOffset == 0x5F27E0
    && CharacterWeaponProjectileFiredNativeLayout.WordReaderFileOffset == 0x34B740
    && CharacterWeaponProjectileFiredNativeLayout.VtableVirtualAddress == 0x1C671C8
    && CharacterWeaponProjectileFiredNativeLayout.PrefixWordFieldOffset == 0x28
    && CharacterWeaponProjectileFiredNativeLayout.PrefixWordCount == 4
    && CharacterWeaponProjectileFiredNativeLayout.OptionalTailInputGateOffset == 8
    && CharacterWeaponProjectileFiredNativeLayout.OptionalTailMarkerFieldOffset == 0x30
    && CharacterWeaponProjectileFiredNativeLayout.OptionalTailPayloadFieldOffset == 0x32
    && CharacterWeaponProjectileFiredNativeLayout.OptionalTailPayloadWordCount == 3
    && CharacterWeaponProjectileFiredNativeLayout.InputByteCountWithoutOptionalTail == 9
    && CharacterWeaponProjectileFiredNativeLayout.InputByteCountWithOptionalTail == 15);
Check("WeaponProjectileFired selector 0x5F records its gated metrics consumer and allocation overread",
    CharacterWeaponProjectileFiredConsumerNativeLayout.MessageDiscriminator == 0x5F
    && CharacterWeaponProjectileFiredConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x100
    && CharacterWeaponProjectileFiredConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECCF30
    && CharacterWeaponProjectileFiredConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACC330
    && CharacterWeaponProjectileFiredConsumerNativeLayout.MetricStateGlobalVirtualAddress == 0x1E0004C
    && CharacterWeaponProjectileFiredConsumerNativeLayout.MetricStateEnabledByteOffset == 4
    && CharacterWeaponProjectileFiredConsumerNativeLayout.MetricStateSuppressedByteOffset == 5
    && CharacterWeaponProjectileFiredConsumerNativeLayout.MetricAppenderFileOffset == 0x481430
    && CharacterWeaponProjectileFiredConsumerNativeLayout.MessageFirstWordOffset == 0x28
    && CharacterWeaponProjectileFiredConsumerNativeLayout.MessageWordWidth == 2
    && CharacterWeaponProjectileFiredConsumerNativeLayout.MessageWordReadCount == 13
    && CharacterWeaponProjectileFiredConsumerNativeLayout.MessageLastWordOffset == 0x40
    && CharacterWeaponProjectileFiredConsumerNativeLayout.ReceiverReadEndOffsetExclusive == 0x42
    && CharacterWeaponProjectileFiredConsumerNativeLayout.DeclaredAllocationReadPastEndByteCount == 10
    && CharacterWeaponProjectileFiredConsumerNativeLayout.MessageWordReadOffsets.SequenceEqual(
        new[] { 0x28, 0x2A, 0x2C, 0x2E, 0x32, 0x30, 0x36, 0x34, 0x38, 0x3A, 0x3C, 0x3E, 0x40 }));
Check("AbilityProjectileFired selector 0x60 records its four gated metric reads",
    CharacterAbilityProjectileFiredConsumerNativeLayout.MessageDiscriminator
        == CharacterAbilityProjectileFiredNativeLayout.InnerSelector
    && CharacterAbilityProjectileFiredConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x104
    && CharacterAbilityProjectileFiredConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECD580
    && CharacterAbilityProjectileFiredConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACC980
    && CharacterAbilityProjectileFiredConsumerNativeLayout.MetricStateGlobalVirtualAddress == 0x1E0004C
    && CharacterAbilityProjectileFiredConsumerNativeLayout.MetricStateEnabledByteOffset == 4
    && CharacterAbilityProjectileFiredConsumerNativeLayout.MetricStateSuppressedByteOffset == 5
    && CharacterAbilityProjectileFiredConsumerNativeLayout.MetricAppenderFileOffset == 0x481430
    && CharacterAbilityProjectileFiredConsumerNativeLayout.Metrics.SequenceEqual(
    [
        new CharacterAbilityProjectileFiredMetricSnapshot(0x28, 0x1DFABF0, 908),
        new CharacterAbilityProjectileFiredMetricSnapshot(0x2A, 0x1DFAC10, 924),
        new CharacterAbilityProjectileFiredMetricSnapshot(0x2C, 0x1DFAC30, 940),
        new CharacterAbilityProjectileFiredMetricSnapshot(0x2E, 0x1DFAC50, 956)
    ]));
Check("AbilityProjectileFired selector 0x60 records its fixed fields and optional QWORD",
    CharacterAbilityProjectileFiredNativeLayout.InnerSelector == 0x60
    && CharacterAbilityProjectileFiredNativeLayout.CaseFileOffset == 0x81D07E
    && CharacterAbilityProjectileFiredNativeLayout.CaseLocalDwordValue == 0x68
    && CharacterAbilityProjectileFiredNativeLayout.AllocationSize == 0x68
    && CharacterAbilityProjectileFiredNativeLayout.ConstructorFileOffset == 0x5257A0
    && CharacterAbilityProjectileFiredNativeLayout.BaseConstructorHelperFileOffset == 0x5A09F0
    && CharacterAbilityProjectileFiredNativeLayout.BaseConstructorArgument == 0x60
    && CharacterAbilityProjectileFiredNativeLayout.BodyReaderFileOffset == 0x5F22A0
    && CharacterAbilityProjectileFiredNativeLayout.OptionalQwordReaderFileOffset == 0x5FDA00
    && CharacterAbilityProjectileFiredNativeLayout.QwordReaderFileOffset == 0x356F80
    && CharacterAbilityProjectileFiredNativeLayout.VtableVirtualAddress == 0x1C671A8
    && CharacterAbilityProjectileFiredNativeLayout.WordField0Offset == 0x28
    && CharacterAbilityProjectileFiredNativeLayout.WordField1Offset == 0x2A
    && CharacterAbilityProjectileFiredNativeLayout.WordField2Offset == 0x2C
    && CharacterAbilityProjectileFiredNativeLayout.WordField3Offset == 0x2E
    && CharacterAbilityProjectileFiredNativeLayout.WordField4Offset == 0x30
    && CharacterAbilityProjectileFiredNativeLayout.WordField5Offset == 0x32
    && CharacterAbilityProjectileFiredNativeLayout.WordField6Offset == 0x34
    && CharacterAbilityProjectileFiredNativeLayout.WordField7Offset == 0x36
    && CharacterAbilityProjectileFiredNativeLayout.DwordField0Offset == 0x38
    && CharacterAbilityProjectileFiredNativeLayout.DwordField1Offset == 0x3C
    && CharacterAbilityProjectileFiredNativeLayout.ByteField0Offset == 0x40
    && CharacterAbilityProjectileFiredNativeLayout.DwordField2Offset == 0x44
    && CharacterAbilityProjectileFiredNativeLayout.ByteField1Offset == 0x48
    && CharacterAbilityProjectileFiredNativeLayout.DwordField3Offset == 0x4C
    && CharacterAbilityProjectileFiredNativeLayout.DwordField4Offset == 0x50
    && CharacterAbilityProjectileFiredNativeLayout.OptionalQwordPresenceFieldOffset == 0x58
    && CharacterAbilityProjectileFiredNativeLayout.OptionalQwordFieldOffset == 0x60
    && CharacterAbilityProjectileFiredNativeLayout.OptionalQwordInputGateOffset == 0x26
    && CharacterAbilityProjectileFiredNativeLayout.OptionalQwordByteCount == 8
    && CharacterAbilityProjectileFiredNativeLayout.InputByteCountWithoutOptionalQword == 0x27
    && CharacterAbilityProjectileFiredNativeLayout.InputByteCountWithOptionalQword == 0x2F);
Check("ProjectileHitReported selector 0x61 records two WORDs and two bytes",
    CharacterProjectileHitReportedNativeLayout.InnerSelector == 0x61
    && CharacterProjectileHitReportedNativeLayout.CaseFileOffset == 0x81D0CF
    && CharacterProjectileHitReportedNativeLayout.CaseLocalDwordValue == 0x69
    && CharacterProjectileHitReportedNativeLayout.AllocationSize == 0x30
    && CharacterProjectileHitReportedNativeLayout.ConstructorFileOffset == 0x525840
    && CharacterProjectileHitReportedNativeLayout.BaseConstructorHelperFileOffset == 0x5A0A60
    && CharacterProjectileHitReportedNativeLayout.BaseConstructorArgument == 0x61
    && CharacterProjectileHitReportedNativeLayout.BodyReaderFileOffset == 0x5F20E0
    && CharacterProjectileHitReportedNativeLayout.VtableVirtualAddress == 0x1C67198
    && CharacterProjectileHitReportedNativeLayout.WordField0Offset == 0x28
    && CharacterProjectileHitReportedNativeLayout.WordField1Offset == 0x2A
    && CharacterProjectileHitReportedNativeLayout.ByteField0Offset == 0x2C
    && CharacterProjectileHitReportedNativeLayout.ByteField1Offset == 0x2D
    && CharacterProjectileHitReportedNativeLayout.NominalInputByteCount == 6);
Check("ProjectileHitReported selector 0x61 records its 22-DWORD receiver read extent",
    CharacterProjectileHitReportedConsumerNativeLayout.MessageDiscriminator
        == CharacterProjectileHitReportedNativeLayout.InnerSelector
    && CharacterProjectileHitReportedConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x108
    && CharacterProjectileHitReportedConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECA340
    && CharacterProjectileHitReportedConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xAC9740
    && CharacterProjectileHitReportedConsumerNativeLayout.BulkLoadInstructionFileOffset == 0xAC974C
    && CharacterProjectileHitReportedConsumerNativeLayout.MessageReadStartOffset == 0
    && CharacterProjectileHitReportedConsumerNativeLayout.MessageDwordReadCount == 22
    && CharacterProjectileHitReportedConsumerNativeLayout.MessageDwordWidth == 4
    && CharacterProjectileHitReportedConsumerNativeLayout.MessageReadEndOffsetExclusive == 0x58
    && CharacterProjectileHitReportedConsumerNativeLayout.LastLoadedDwordOffset == 0x54
    && CharacterProjectileHitReportedConsumerNativeLayout.DeclaredAllocationReadPastEndByteCount == 0x28);
Check("Stumble selector 0x62 records two WORDs and a byte",
    CharacterStumbleNativeLayout.InnerSelector == 0x62
    && CharacterStumbleNativeLayout.CaseFileOffset == 0x81D120
    && CharacterStumbleNativeLayout.CaseLocalDwordValue == 0x6A
    && CharacterStumbleNativeLayout.AllocationSize == 0x30
    && CharacterStumbleNativeLayout.ConstructorFileOffset == 0x5258C0
    && CharacterStumbleNativeLayout.BaseConstructorHelperFileOffset == 0x5A0AD0
    && CharacterStumbleNativeLayout.BaseConstructorArgument == 0x62
    && CharacterStumbleNativeLayout.BodyReaderFileOffset == 0x5F1ED0
    && CharacterStumbleNativeLayout.VtableVirtualAddress == 0x1C67188
    && CharacterStumbleNativeLayout.WordField0Offset == 0x28
    && CharacterStumbleNativeLayout.WordField1Offset == 0x2A
    && CharacterStumbleNativeLayout.ByteFieldOffset == 0x2C
    && CharacterStumbleNativeLayout.NominalInputByteCount == 5);
Check("Stumble selector 0x62 records its key, status, and bounded vector mismatch",
    CharacterStumbleConsumerNativeLayout.MessageDiscriminator == CharacterStumbleNativeLayout.InnerSelector
    && CharacterStumbleConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x10C
    && CharacterStumbleConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECC250
    && CharacterStumbleConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACB650
    && CharacterStumbleConsumerNativeLayout.MessageStateGateGlobalVirtualAddress == 0x1E68354
    && CharacterStumbleConsumerNativeLayout.MessageStateGateByteOffset == 0x40
    && CharacterStumbleConsumerNativeLayout.MessageKeyDwordOffset == 0x28
    && CharacterStumbleConsumerNativeLayout.MessageStatusByteOffset == 0x2C
    && CharacterStumbleConsumerNativeLayout.MessageVectorBeginPointerOffset == 0x30
    && CharacterStumbleConsumerNativeLayout.MessageVectorEndPointerOffset == 0x34
    && CharacterStumbleConsumerNativeLayout.VectorEntrySizeBytes == 12
    && CharacterStumbleConsumerNativeLayout.VectorHeaderReadEndOffsetExclusive == 0x38
    && CharacterStumbleConsumerNativeLayout.VectorHeaderReadPastAllocationByteCount == 8
    && CharacterStumbleConsumerNativeLayout.StatusValueZero == 0
    && CharacterStumbleConsumerNativeLayout.StatusValueOne == 1
    && CharacterStumbleConsumerNativeLayout.StatusValueOther == 2
    && CharacterStumbleConsumerNativeLayout.VectorCopyHelperFileOffset == 0x611A30
    && CharacterStumbleConsumerNativeLayout.SubsystemAccessorFileOffset == 0x60FB60
    && CharacterStumbleConsumerNativeLayout.SubsystemMessageHandlerFileOffset == 0x60FBD0);
Check("QuickChat selector 0x63 records its DWORD field and shared reader continuation",
    CharacterQuickChatNativeLayout.InnerSelector == 0x63
    && CharacterQuickChatNativeLayout.CaseFileOffset == 0x81D171
    && CharacterQuickChatNativeLayout.CaseLocalDwordValue == 0x6B
    && CharacterQuickChatNativeLayout.AllocationSize == 0x30
    && CharacterQuickChatNativeLayout.ConstructorFileOffset == 0x525940
    && CharacterQuickChatNativeLayout.BaseConstructorHelperFileOffset == 0x5A0B40
    && CharacterQuickChatNativeLayout.BaseConstructorArgument == 0x63
    && CharacterQuickChatNativeLayout.AllocationFailureContinuationFileOffset == 0x8206E1
    && CharacterQuickChatNativeLayout.BodyReaderContinuationFileOffset == 0x8206E3
    && CharacterQuickChatNativeLayout.DwordReaderFileOffset == 0x3370A0
    && CharacterQuickChatNativeLayout.VtableVirtualAddress == 0x1C67178
    && CharacterQuickChatNativeLayout.DwordFieldOffset == 0x28
    && CharacterQuickChatNativeLayout.NominalInputByteCount == 4);
Check("QuickChat selector 0x63 records its notification fields and pointer overread",
    CharacterQuickChatConsumerNativeLayout.MessageDiscriminator == CharacterQuickChatNativeLayout.InnerSelector
    && CharacterQuickChatConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x110
    && CharacterQuickChatConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECD7E0
    && CharacterQuickChatConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACCBE0
    && CharacterQuickChatConsumerNativeLayout.MessageLogDwordOffset == 0x28
    && CharacterQuickChatConsumerNativeLayout.MessageTextPointerOffset == 0x30
    && CharacterQuickChatConsumerNativeLayout.MessageTypePointerOffset == 0x34
    && CharacterQuickChatConsumerNativeLayout.ReceiverPointerReadEndOffsetExclusive == 0x38
    && CharacterQuickChatConsumerNativeLayout.DeclaredAllocationReadPastEndByteCount == 8
    && CharacterQuickChatConsumerNativeLayout.ChatGetterFileOffset == 0x6F4540
    && CharacterQuickChatConsumerNativeLayout.NotificationMethodFileOffset == 0x6E3460
    && CharacterQuickChatConsumerNativeLayout.LoggerFileOffset == 0x325A90);
Check("ProximityTextChat selector 0x64 records shared text and optional blob framing",
    CharacterProximityTextChatNativeLayout.InnerSelector == 0x64
    && CharacterProximityTextChatNativeLayout.CaseFileOffset == 0x81D1A2
    && CharacterProximityTextChatNativeLayout.CaseLocalDwordValue == 0x6C
    && CharacterProximityTextChatNativeLayout.AllocationSize == 0x48
    && CharacterProximityTextChatNativeLayout.ConstructorFileOffset == 0x5259C0
    && CharacterProximityTextChatNativeLayout.BaseConstructorHelperFileOffset == 0x5A0BB0
    && CharacterProximityTextChatNativeLayout.BaseConstructorArgument == 0x64
    && CharacterProximityTextChatNativeLayout.BodyReaderFileOffset == 0x5F1BC0
    && CharacterProximityTextChatNativeLayout.SharedTextReaderFileOffset == 0x12EE010
    && CharacterProximityTextChatNativeLayout.OptionalBlobReaderFileOffset == 0x5F19A0
    && CharacterProximityTextChatNativeLayout.WordLengthPrefixedBlobReaderFileOffset == 0x12EE170
    && CharacterProximityTextChatNativeLayout.OptionalBlobUpdateHelperFileOffset == 0x5F1920
    && CharacterProximityTextChatNativeLayout.OptionalBlobClearHelperFileOffset == 0x49AC30
    && CharacterProximityTextChatNativeLayout.VtableVirtualAddress == 0x1C67158
    && CharacterProximityTextChatNativeLayout.SharedTextFieldOffset == 0x28
    && CharacterProximityTextChatNativeLayout.SharedTextTerminatorByte == 0
    && CharacterProximityTextChatNativeLayout.SharedTextChunkByteLimit == 0x100
    && CharacterProximityTextChatNativeLayout.SharedTextTemporaryBufferByteCount == 0x104
    && CharacterProximityTextChatNativeLayout.ByteField0Offset == 0x2C
    && CharacterProximityTextChatNativeLayout.ByteField1Offset == 0x2D
    && CharacterProximityTextChatNativeLayout.ByteField2Offset == 0x2E
    && CharacterProximityTextChatNativeLayout.OptionalBlobPresenceFieldOffset == 0x30
    && CharacterProximityTextChatNativeLayout.FixedByteCountAfterSharedTextBeforeBlobGate == 3
    && CharacterProximityTextChatNativeLayout.OptionalBlobGateByteCount == 1
    && CharacterProximityTextChatNativeLayout.OptionalBlobLengthPrefixByteCount == 2
    && CharacterProximityTextChatNativeLayout.MaximumOptionalBlobPayloadByteCount == 0xFFFF
    && CharacterProximityTextChatNativeLayout.EmptyTextInputByteCountWithoutOptionalBlob == 5
    && CharacterProximityTextChatNativeLayout.EmptyTextInputByteCountWithEmptyOptionalBlob == 7);
Check("ProximityTextChat selector 0x64 records its gated transport handoff",
    CharacterProximityTextChatConsumerNativeLayout.MessageDiscriminator
        == CharacterProximityTextChatNativeLayout.InnerSelector
    && CharacterProximityTextChatConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x114
    && CharacterProximityTextChatConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECD900
    && CharacterProximityTextChatConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACCD00
    && CharacterProximityTextChatConsumerNativeLayout.MessagePayloadTypeWordOffset == 0x40
    && CharacterProximityTextChatConsumerNativeLayout.AcceptedPayloadTypeWordValue == 2
    && CharacterProximityTextChatConsumerNativeLayout.TransportContextFieldOffset == 0x10
    && CharacterProximityTextChatConsumerNativeLayout.TextParameterFieldOffset == 0x28
    && CharacterProximityTextChatConsumerNativeLayout.OptionalBlobParameterFieldOffset == 0x30
    && CharacterProximityTextChatConsumerNativeLayout.ReferenceTreeParameterFieldOffset == 0x44
    && CharacterProximityTextChatConsumerNativeLayout.AuxiliaryParameterPointerOffset == 0x4C
    && CharacterProximityTextChatConsumerNativeLayout.AuxiliaryPointerPastAllocationByteCount == 4
    && CharacterProximityTextChatConsumerNativeLayout.MissingTransportDiagnosticCode == 91
    && CharacterProximityTextChatConsumerNativeLayout.NetworkSendRoutineFileOffset == 0x977150
    && CharacterProximityTextChatConsumerNativeLayout.UnhandledPayloadLoggerFileOffset == 0x2FC4F0);
Check("JumpActioned selector 0x65 records its opaque subobject handoff",
    CharacterJumpActionedConsumerNativeLayout.MessageDiscriminator
        == CharacterJumpActionedNativeLayout.InnerSelector
    && CharacterJumpActionedConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x118
    && CharacterJumpActionedConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECD970
    && CharacterJumpActionedConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACCD70
    && CharacterJumpActionedConsumerNativeLayout.TransportContextFieldOffset == 0x10
    && CharacterJumpActionedConsumerNativeLayout.MessageWordFieldOffset == 0x28
    && CharacterJumpActionedConsumerNativeLayout.FirstSubobjectAddressOffset == 0x30
    && CharacterJumpActionedConsumerNativeLayout.SecondSubobjectAddressOffset == 0x40
    && CharacterJumpActionedConsumerNativeLayout.AllocationSize == 0x30
    && CharacterJumpActionedConsumerNativeLayout.SecondSubobjectAddressPastAllocationByteCount == 0x10
    && CharacterJumpActionedConsumerNativeLayout.SubobjectVtableMethodSlotOffset == 0x0C
    && CharacterJumpActionedConsumerNativeLayout.MissingTransportDiagnosticCode == 91
    && CharacterJumpActionedConsumerNativeLayout.HandoffRoutineFileOffset == 0x977750
    && CharacterJumpActionedConsumerNativeLayout.HandoffEventIdForRepeatedItems == 360
    && CharacterJumpActionedConsumerNativeLayout.HandoffEventIdForNonemptyTail == 359);
Check("JumpRolled selector 0x66 records its keyed drain receiver",
    CharacterJumpRolledConsumerNativeLayout.MessageDiscriminator
        == CharacterJumpRolledNativeLayout.InnerSelector
    && CharacterJumpRolledConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x11C
    && CharacterJumpRolledConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECD9B0
    && CharacterJumpRolledConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACCDB0
    && CharacterJumpRolledConsumerNativeLayout.TransportContextFieldOffset == 0x10
    && CharacterJumpRolledConsumerNativeLayout.MessageKeyFieldOffset == 0x28
    && CharacterJumpRolledConsumerNativeLayout.MessageKeyFieldByteCount == 8
    && CharacterJumpRolledConsumerNativeLayout.KeyedDrainRoutineFileOffset == 0x977F20
    && CharacterJumpRolledConsumerNativeLayout.KeyLookupRoutineFileOffset == 0x979C80
    && CharacterJumpRolledConsumerNativeLayout.DrainItemRoutineFileOffset == 0x977630
    && CharacterJumpRolledConsumerNativeLayout.DrainFinalizeRoutineFileOffset == 0x975020
    && CharacterJumpRolledConsumerNativeLayout.DrainReleaseRoutineFileOffset == 0x12C8D60
    && CharacterJumpRolledConsumerNativeLayout.MissingTransportDiagnosticCode == 91);
Check("Respawned selector 0x67 records its typed vector receiver",
    CharacterRespawnedConsumerNativeLayout.MessageDiscriminator
        == CharacterRespawnedNativeLayout.InnerSelector
    && CharacterRespawnedConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x120
    && CharacterRespawnedConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECD9F0
    && CharacterRespawnedConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACCDF0
    && CharacterRespawnedConsumerNativeLayout.TransportContextFieldOffset == 0x10
    && CharacterRespawnedConsumerNativeLayout.VectorBeginPointerOffset == 0x30
    && CharacterRespawnedConsumerNativeLayout.VectorEndPointerOffset == 0x34
    && CharacterRespawnedConsumerNativeLayout.VectorEntryStride == 0x10
    && CharacterRespawnedConsumerNativeLayout.VectorEntryKindOffset == 0
    && CharacterRespawnedConsumerNativeLayout.VectorEntryTextPointerOffset == 4
    && CharacterRespawnedConsumerNativeLayout.VectorEntryIntegerOffset == 8
    && CharacterRespawnedConsumerNativeLayout.VectorEntryFloatOffset == 0x0C
    && CharacterRespawnedConsumerNativeLayout.OptionalSharedTextPointerOffset == 0x2C
    && CharacterRespawnedConsumerNativeLayout.OptionalByteFieldOffset == 0x8C
    && CharacterRespawnedConsumerNativeLayout.DispatchedEventId == 167);
Check("CalledForHelp selector 0x68 records its keyed event receiver",
    CharacterCalledForHelpConsumerNativeLayout.MessageDiscriminator
        == CharacterCalledForHelpNativeLayout.InnerSelector
    && CharacterCalledForHelpConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x124
    && CharacterCalledForHelpConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECE040
    && CharacterCalledForHelpConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACD440
    && CharacterCalledForHelpConsumerNativeLayout.KeyedRegistryVirtualAddress == 0x1E5A830
    && CharacterCalledForHelpConsumerNativeLayout.KeyedLookupRoutineFileOffset == 0x327890
    && CharacterCalledForHelpConsumerNativeLayout.MessageKeyFieldOffset == 0x28
    && CharacterCalledForHelpConsumerNativeLayout.MessageAuxiliaryByteFieldOffset == 0x2C
    && CharacterCalledForHelpConsumerNativeLayout.MessageAuxiliaryDwordFieldOffset == 0x30
    && CharacterCalledForHelpConsumerNativeLayout.AllocationSize == 0x30
    && CharacterCalledForHelpConsumerNativeLayout.MessageAuxiliaryDwordPastAllocationByteCount == 4
    && CharacterCalledForHelpConsumerNativeLayout.UnhandledMessageLoggerFileOffset == 0x2FC4F0
    && CharacterCalledForHelpConsumerNativeLayout.DispatchedEventId == 176);
Check("TookHit selector 0x69 records its state-copy receiver",
    CharacterTookHitConsumerNativeLayout.MessageDiscriminator
        == CharacterTookHitNativeLayout.InnerSelector
    && CharacterTookHitConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x128
    && CharacterTookHitConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECE210
    && CharacterTookHitConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACD610
    && CharacterTookHitConsumerNativeLayout.MessageCopySourceOffset == 0x28
    && CharacterTookHitConsumerNativeLayout.CopiedPayloadByteCount == 0x40
    && CharacterTookHitConsumerNativeLayout.AllocationSize == 0x60
    && CharacterTookHitConsumerNativeLayout.CopiedPayloadPastAllocationByteCount == 8
    && CharacterTookHitConsumerNativeLayout.AdjustedFloatMessageOffset == 0x60
    && CharacterTookHitConsumerNativeLayout.FloatAdjustmentVirtualAddress == 0x1C81CA8
    && CharacterTookHitConsumerNativeLayout.CopyRoutineFileOffset == 0x37F810
    && CharacterTookHitConsumerNativeLayout.StateAccessorFileOffset == 0x49DDD0
    && CharacterTookHitConsumerNativeLayout.StateDestinationOffset == 0xC8
    && CharacterTookHitConsumerNativeLayout.StateUpdateFlagOffset == 0x108
    && CharacterTookHitConsumerNativeLayout.StateWriteGateByteOffset == 0x18C
    && CharacterTookHitConsumerNativeLayout.StateDestinationPointerOffset == 0x14C8);
Check("AlmostHit selector 0x6A records its keyed callback and signed-value receiver",
    CharacterAlmostHitConsumerNativeLayout.MessageDiscriminator
        == CharacterAlmostHitNativeLayout.InnerSelector
    && CharacterAlmostHitConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x12C
    && CharacterAlmostHitConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECEAD0
    && CharacterAlmostHitConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACDED0
    && CharacterAlmostHitConsumerNativeLayout.MessageSignedValueFieldOffset == 0x28
    && CharacterAlmostHitConsumerNativeLayout.MessageLookupKeyFieldOffset == 0x30
    && CharacterAlmostHitConsumerNativeLayout.LookupKeyByteMask == unchecked((int)0xFFFFFF00)
    && CharacterAlmostHitConsumerNativeLayout.InvalidLookupKeyUpperSentinel == 0xFFFFFFFFFFFFFF00
    && CharacterAlmostHitConsumerNativeLayout.AllocationSize == 0x30
    && CharacterAlmostHitConsumerNativeLayout.LookupKeyPastAllocationByteCount == 8
    && CharacterAlmostHitConsumerNativeLayout.LookupTableAccessorFileOffset == 0x35E700
    && CharacterAlmostHitConsumerNativeLayout.LookupTableBucketsFieldOffset == 0x54
    && CharacterAlmostHitConsumerNativeLayout.LookupRoutineFileOffset == 0x4093E0
    && CharacterAlmostHitConsumerNativeLayout.LookupEntryTargetPointerOffset == 0x10
    && CharacterAlmostHitConsumerNativeLayout.LookupTargetVirtualMethodSlotOffset == 0x70
    && CharacterAlmostHitConsumerNativeLayout.SignedValueHandlerFileOffset == 0x369080
    && CharacterAlmostHitConsumerNativeLayout.SignedValueHandlerTypeArgument == 12
    && CharacterAlmostHitConsumerNativeLayout.ManagerGlobalAddress == 0x1DFFFE8
    && CharacterAlmostHitConsumerNativeLayout.ManagerAvailabilityFieldOffset == 0x28);
Check("DealtHit selector 0x6B records its position and follow-up receiver",
    CharacterDealtHitConsumerNativeLayout.MessageDiscriminator
        == CharacterDealtHitNativeLayout.InnerSelector
    && CharacterDealtHitConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x130
    && CharacterDealtHitConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECEB70
    && CharacterDealtHitConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACDF70
    && CharacterDealtHitConsumerNativeLayout.ConsumerSwitchTargetVirtualAddress == 0xC1D409
    && CharacterDealtHitConsumerNativeLayout.ConsumerSwitchTargetFileOffset == 0x81C809
    && CharacterDealtHitConsumerNativeLayout.ConsumerVtableCallFileOffset == 0x81C828
    && CharacterDealtHitConsumerNativeLayout.MessageSignedValueFieldOffset == 0x28
    && CharacterDealtHitConsumerNativeLayout.MessagePositionArgumentOffset == 0x30
    && CharacterDealtHitConsumerNativeLayout.PositionComponentCount == 3
    && CharacterDealtHitConsumerNativeLayout.PositionDistanceOriginXVirtualAddress == 0x1E62C44
    && CharacterDealtHitConsumerNativeLayout.PositionDistanceOriginYVirtualAddress == 0x1E62C48
    && CharacterDealtHitConsumerNativeLayout.PositionDistanceOriginZVirtualAddress == 0x1E62C4C
    && CharacterDealtHitConsumerNativeLayout.PositionUpdateGateByteOffset == 0xE1
    && CharacterDealtHitConsumerNativeLayout.PositionUpdateHelperFileOffset == 0x100F7A0
    && CharacterDealtHitConsumerNativeLayout.PositionDistanceCallbackFileOffset == 0x100AE90
    && CharacterDealtHitConsumerNativeLayout.MessageValueHandlerFileOffset == 0x369080
    && CharacterDealtHitConsumerNativeLayout.MessageValueHandlerTypeArgument == 12
    && CharacterDealtHitConsumerNativeLayout.FollowupDurationSeconds == 0x3C
    && CharacterDealtHitConsumerNativeLayout.FollowupDurationMillisecondsMultiplier == 1000
    && CharacterDealtHitConsumerNativeLayout.FollowupTimerRegistrationFileOffset == 0x100C5D0
    && CharacterDealtHitConsumerNativeLayout.SharedDispatchHelperFileOffset == 0x368EA0);
Check("Killed selector 0x6C records its manager/context receiver",
    CharacterKilledConsumerNativeLayout.MessageDiscriminator
        == CharacterKilledNativeLayout.InnerSelector
    && CharacterKilledConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x134
    && CharacterKilledConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECEBA0
    && CharacterKilledConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACDFA0
    && CharacterKilledConsumerNativeLayout.ConsumerSwitchTargetVirtualAddress == 0xC1D434
    && CharacterKilledConsumerNativeLayout.ConsumerSwitchTargetFileOffset == 0x81C834
    && CharacterKilledConsumerNativeLayout.ConsumerVtableCallFileOffset == 0x81C853
    && CharacterKilledConsumerNativeLayout.ManagerGlobalAddress == 0x1DFFFFC
    && CharacterKilledConsumerNativeLayout.MessageValueFieldOffset == 0x28
    && CharacterKilledConsumerNativeLayout.MessageAuxiliaryFieldOffset == 0x2C
    && CharacterKilledConsumerNativeLayout.MessageAuxiliarySecondDwordOffset == 0x30
    && CharacterKilledConsumerNativeLayout.AllocationSize == 0x40
    && CharacterKilledConsumerNativeLayout.DispatchHelperFileOffset == 0x466790
    && CharacterKilledConsumerNativeLayout.KeyedObjectLookupFileOffset == 0x421640
    && CharacterKilledConsumerNativeLayout.ContextUpdateRoutineFileOffset == 0x465A10
    && CharacterKilledConsumerNativeLayout.ConditionalContextAllocationSize == 0xB0);
Check("WarnLockTargeted selector 0x6D records its gated manager update",
    CharacterWarnLockTargetedConsumerNativeLayout.MessageDiscriminator
        == CharacterWarnLockTargetedNativeLayout.InnerSelector
    && CharacterWarnLockTargetedConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x138
    && CharacterWarnLockTargetedConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECEBC0
    && CharacterWarnLockTargetedConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACDFC0
    && CharacterWarnLockTargetedConsumerNativeLayout.ConsumerSwitchTargetVirtualAddress == 0xC1D45F
    && CharacterWarnLockTargetedConsumerNativeLayout.ConsumerSwitchTargetFileOffset == 0x81C85F
    && CharacterWarnLockTargetedConsumerNativeLayout.ConsumerVtableCallFileOffset == 0x81C87E
    && CharacterWarnLockTargetedConsumerNativeLayout.ManagerGlobalAddress == 0x1DFFFFC
    && CharacterWarnLockTargetedConsumerNativeLayout.MessageTargetFieldOffset == 0x28
    && CharacterWarnLockTargetedConsumerNativeLayout.MessageBooleanFieldOffset == 0x38
    && CharacterWarnLockTargetedConsumerNativeLayout.AllocationSize == 0x28
    && CharacterWarnLockTargetedConsumerNativeLayout.MessageTargetPastAllocationByteCount == 4
    && CharacterWarnLockTargetedConsumerNativeLayout.MessageBooleanPastAllocationByteCount == 17
    && CharacterWarnLockTargetedConsumerNativeLayout.StateUpdateHelperFileOffset == 0x465D40);
Check("CurrentPoseUpdate selector 0x6E records its position and optional-text event receiver",
    CharacterCurrentPoseUpdateConsumerNativeLayout.MessageDiscriminator
        == CharacterCurrentPoseUpdateNativeLayout.InnerSelector
    && CharacterCurrentPoseUpdateConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x13C
    && CharacterCurrentPoseUpdateConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECE2A0
    && CharacterCurrentPoseUpdateConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACD6A0
    && CharacterCurrentPoseUpdateConsumerNativeLayout.ConsumerSwitchTargetVirtualAddress == 0xC1D48A
    && CharacterCurrentPoseUpdateConsumerNativeLayout.ConsumerSwitchTargetFileOffset == 0x81C88A
    && CharacterCurrentPoseUpdateConsumerNativeLayout.ConsumerVtableCallFileOffset == 0x81C8A9
    && CharacterCurrentPoseUpdateConsumerNativeLayout.FirstFloatFieldOffset == 0x28
    && CharacterCurrentPoseUpdateConsumerNativeLayout.SecondFloatFieldOffset == 0x2C
    && CharacterCurrentPoseUpdateConsumerNativeLayout.ThirdFloatFieldOffset == 0x30
    && CharacterCurrentPoseUpdateConsumerNativeLayout.OptionalTextKeyFieldOffset == 0x34
    && CharacterCurrentPoseUpdateConsumerNativeLayout.OptionalTextRegistryVirtualAddress == 0x1E5A4F0
    && CharacterCurrentPoseUpdateConsumerNativeLayout.OptionalTextLookupRoutineFileOffset == 0x327890
    && CharacterCurrentPoseUpdateConsumerNativeLayout.OptionalTextExtractionRoutineFileOffset == 0x13122D0
    && CharacterCurrentPoseUpdateConsumerNativeLayout.DispatchedEventId == 239
    && CharacterCurrentPoseUpdateConsumerNativeLayout.EventDispatcherGlobalAddress == 0x1E002B8
    && CharacterCurrentPoseUpdateConsumerNativeLayout.EventDispatcherVtableSlotOffset == 0xAC);
Check("ConfirmedPoseUpdate selector 0x6F records its typed values and optional-text receiver",
    CharacterConfirmedPoseUpdateConsumerNativeLayout.MessageDiscriminator
        == CharacterConfirmedPoseUpdateNativeLayout.InnerSelector
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x140
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECE400
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACD800
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.ConsumerSwitchTargetVirtualAddress == 0xC1D4B5
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.ConsumerSwitchTargetFileOffset == 0x81C8B5
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.ConsumerVtableCallFileOffset == 0x81C8D4
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.FirstMessageDwordOffset == 0x28
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.SecondMessageDwordOffset == 0x2C
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.OptionalTextKeyFieldOffset == 0x30
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.OptionalTextRegistryVirtualAddress == 0x1E5A4F0
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.OptionalTextLookupRoutineFileOffset == 0x327890
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.OptionalTextExtractionRoutineFileOffset == 0x13122D0
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.DispatchedEventId == 239
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.EventDispatcherGlobalAddress == 0x1E002B8
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.EventDispatcherVtableSlotOffset == 0xAC
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.TypedMessageValueAppenderFileOffset == 0xC245A0
    && CharacterConfirmedPoseUpdateConsumerNativeLayout.AppendedValueKind == 5);
Check("PublicDebugMovementUpdate selector 0x70 records its nested-object event receiver",
    CharacterPublicDebugMovementUpdateConsumerNativeLayout.MessageDiscriminator
        == CharacterPublicDebugMovementUpdateNativeLayout.InnerSelector
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x144
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECEBF0
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACDFF0
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.ConsumerSwitchTargetVirtualAddress == 0xC1D4E0
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.ConsumerSwitchTargetFileOffset == 0x81C8E0
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.ConsumerVtableCallFileOffset == 0x81C8FF
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.FirstMessageDwordOffset == 0x28
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.SecondMessageDwordOffset == 0x2C
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.NestedMessageObjectOffset == 0x30
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.NestedObjectQueryVtableSlotOffset == 0x0C
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.NestedObjectUpdateVtableSlotOffset == 0x18
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.TypedMessageValueAppenderFileOffset == 0xC245A0
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.NestedObjectValueAppenderFileOffset == 0xC24780
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.AppendedValueKind == 5
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.DispatchedEventId == 363
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.EventDispatcherGlobalAddress == 0x1E002B8
    && CharacterPublicDebugMovementUpdateConsumerNativeLayout.EventDispatcherVtableSlotOffset == 0xAC);
Check("ForcedMovement selector 0x71 records its nested-state event receiver",
    CharacterForcedMovementConsumerNativeLayout.MessageDiscriminator
        == CharacterForcedMovementNativeLayout.InnerSelector
    && CharacterForcedMovementConsumerNativeLayout.ProtocolHandlerDispatchTableVirtualAddress == 0xC1D6DC
    && CharacterForcedMovementConsumerNativeLayout.ProtocolHandlerDispatchTableSelectorBias == 0x20
    && CharacterForcedMovementConsumerNativeLayout.ProtocolHandlerVtableSlotOffset == 0x148
    && CharacterForcedMovementConsumerNativeLayout.ProtocolHandlerVtableTargetVirtualAddress == 0xECED30
    && CharacterForcedMovementConsumerNativeLayout.ProtocolHandlerVtableTargetFileOffset == 0xACE130
    && CharacterForcedMovementConsumerNativeLayout.ConsumerFunctionFileOffset == 0xACE130
    && CharacterForcedMovementConsumerNativeLayout.ConsumerSwitchTargetVirtualAddress == 0xC1D50B
    && CharacterForcedMovementConsumerNativeLayout.ConsumerSwitchTargetFileOffset == 0x81C90B
    && CharacterForcedMovementConsumerNativeLayout.ConsumerVtableCallFileOffset == 0x81C92A
    && CharacterForcedMovementConsumerNativeLayout.NestedStateOffset == 0x28
    && CharacterForcedMovementConsumerNativeLayout.NestedStateValueAppenderFileOffset == 0xC24780
    && CharacterForcedMovementConsumerNativeLayout.DispatchedEventId == 365
    && CharacterForcedMovementConsumerNativeLayout.EventDispatcherGlobalAddress == 0x1E002B8
    && CharacterForcedMovementConsumerNativeLayout.EventDispatcherVtableSlotOffset == 0xAC);
Check("JumpActioned selector 0x65 records its shared WORD reader continuation",
    CharacterJumpActionedNativeLayout.InnerSelector == 0x65
    && CharacterJumpActionedNativeLayout.CaseFileOffset == 0x81D1F3
    && CharacterJumpActionedNativeLayout.CaseLocalDwordValue == 0x6D
    && CharacterJumpActionedNativeLayout.AllocationSize == 0x30
    && CharacterJumpActionedNativeLayout.ConstructorFileOffset == 0x525AF0
    && CharacterJumpActionedNativeLayout.BaseConstructorHelperFileOffset == 0x5A0C20
    && CharacterJumpActionedNativeLayout.BaseConstructorArgument == 0x65
    && CharacterJumpActionedNativeLayout.AllocationFailureContinuationFileOffset == 0x81D21D
    && CharacterJumpActionedNativeLayout.BodyReaderContinuationFileOffset == 0x81D21F
    && CharacterJumpActionedNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterJumpActionedNativeLayout.VtableVirtualAddress == 0x1C67168
    && CharacterJumpActionedNativeLayout.WordFieldOffset == 0x28
    && CharacterJumpActionedNativeLayout.NominalInputByteCount == 2);
Check("JumpRolled selector 0x66 records its shared WORD reader continuation",
    CharacterJumpRolledNativeLayout.InnerSelector == 0x66
    && CharacterJumpRolledNativeLayout.CaseFileOffset == 0x81D247
    && CharacterJumpRolledNativeLayout.CaseLocalDwordValue == 0x6E
    && CharacterJumpRolledNativeLayout.AllocationSize == 0x30
    && CharacterJumpRolledNativeLayout.ConstructorFileOffset == 0x525B70
    && CharacterJumpRolledNativeLayout.BaseConstructorHelperFileOffset == 0x5A0C90
    && CharacterJumpRolledNativeLayout.BaseConstructorArgument == 0x66
    && CharacterJumpRolledNativeLayout.AllocationFailureContinuationFileOffset == 0x81D21D
    && CharacterJumpRolledNativeLayout.BodyReaderContinuationFileOffset == 0x81D21F
    && CharacterJumpRolledNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterJumpRolledNativeLayout.VtableVirtualAddress == 0x1C67148
    && CharacterJumpRolledNativeLayout.WordFieldOffset == 0x28
    && CharacterJumpRolledNativeLayout.NominalInputByteCount == 2);
Check("CalledForHelp selector 0x68 records its shared WORD reader continuation",
    CharacterCalledForHelpNativeLayout.InnerSelector == 0x68
    && CharacterCalledForHelpNativeLayout.CaseFileOffset == 0x81D2C2
    && CharacterCalledForHelpNativeLayout.CaseLocalDwordValue == 0x70
    && CharacterCalledForHelpNativeLayout.AllocationSize == 0x30
    && CharacterCalledForHelpNativeLayout.ConstructorFileOffset == 0x525C70
    && CharacterCalledForHelpNativeLayout.BaseConstructorHelperFileOffset == 0x5A0D70
    && CharacterCalledForHelpNativeLayout.BaseConstructorArgument == 0x68
    && CharacterCalledForHelpNativeLayout.AllocationFailureContinuationFileOffset == 0x81D21D
    && CharacterCalledForHelpNativeLayout.BodyReaderContinuationFileOffset == 0x81D21F
    && CharacterCalledForHelpNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterCalledForHelpNativeLayout.VtableVirtualAddress == 0x1C67118
    && CharacterCalledForHelpNativeLayout.WordFieldOffset == 0x28
    && CharacterCalledForHelpNativeLayout.NominalInputByteCount == 2);
Check("Respawned selector 0x67 records its WORD and byte reader layout",
    CharacterRespawnedNativeLayout.InnerSelector == 0x67
    && CharacterRespawnedNativeLayout.InnerSelectorTableVirtualAddress == 0xC22444
    && CharacterRespawnedNativeLayout.InnerSelectorTableFileOffset == 0x821844
    && CharacterRespawnedNativeLayout.CaseFileOffset == 0x81D271
    && CharacterRespawnedNativeLayout.CaseLocalDwordValue == 0x6F
    && CharacterRespawnedNativeLayout.AllocationSize == 0x30
    && CharacterRespawnedNativeLayout.AllocationFunctionFileOffset == 0x12B7FB0
    && CharacterRespawnedNativeLayout.ConstructorFileOffset == 0x525BF0
    && CharacterRespawnedNativeLayout.BaseConstructorHelperFileOffset == 0x5A0D00
    && CharacterRespawnedNativeLayout.BaseConstructorArgument == 0x67
    && CharacterRespawnedNativeLayout.AllocationFailureContinuationFileOffset == 0x81D29B
    && CharacterRespawnedNativeLayout.BodyReaderContinuationFileOffset == 0x81D29D
    && CharacterRespawnedNativeLayout.BodyReaderFileOffset == 0x5F1550
    && CharacterRespawnedNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterRespawnedNativeLayout.FirstByteReaderFileOffset == 0x356CF0
    && CharacterRespawnedNativeLayout.SecondByteReaderFileOffset == 0x337180
    && CharacterRespawnedNativeLayout.VtableVirtualAddress == 0x1C67138
    && CharacterRespawnedNativeLayout.RttiName == ".?AVRespawned@Character@firefall@@"
    && CharacterRespawnedNativeLayout.WordFieldOffset == 0x28
    && CharacterRespawnedNativeLayout.FirstByteFieldOffset == 0x2A
    && CharacterRespawnedNativeLayout.SecondByteFieldOffset == 0x2B
    && CharacterRespawnedNativeLayout.NominalInputByteCount == 4);
Check("TookHit selector 0x69 records its conditional and trailing field layout",
    CharacterTookHitNativeLayout.InnerSelector == 0x69
    && CharacterTookHitNativeLayout.InnerSelectorTableVirtualAddress == 0xC22444
    && CharacterTookHitNativeLayout.InnerSelectorTableFileOffset == 0x821844
    && CharacterTookHitNativeLayout.CaseFileOffset == 0x81D2F3
    && CharacterTookHitNativeLayout.CaseLocalDwordValue == 0x71
    && CharacterTookHitNativeLayout.AllocationSize == 0x60
    && CharacterTookHitNativeLayout.AllocationFunctionFileOffset == 0x12B7FB0
    && CharacterTookHitNativeLayout.ConstructorFileOffset == 0x525CF0
    && CharacterTookHitNativeLayout.BaseConstructorHelperFileOffset == 0x5A0DE0
    && CharacterTookHitNativeLayout.BaseConstructorArgument == 0x69
    && CharacterTookHitNativeLayout.AllocationFailureContinuationFileOffset == 0x81D31D
    && CharacterTookHitNativeLayout.BodyReaderContinuationFileOffset == 0x81D31F
    && CharacterTookHitNativeLayout.BodyReaderFileOffset == 0x5F0FC0
    && CharacterTookHitNativeLayout.ConditionalRecordReaderFileOffset == 0x5F0CF0
    && CharacterTookHitNativeLayout.ConditionalQwordReaderFileOffset == 0x5FDA00
    && CharacterTookHitNativeLayout.QwordReaderFileOffset == 0x356F80
    && CharacterTookHitNativeLayout.DwordReaderFileOffset == 0x353310
    && CharacterTookHitNativeLayout.ByteReaderFileOffset == 0x337180
    && CharacterTookHitNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterTookHitNativeLayout.VtableVirtualAddress == 0x1C67128
    && CharacterTookHitNativeLayout.RttiName == ".?AVTookHit@Character@firefall@@"
    && CharacterTookHitNativeLayout.ConditionalRecordGateByteOffset == 0x28
    && CharacterTookHitNativeLayout.ConditionalRecordFirstQwordOffset == 0x30
    && CharacterTookHitNativeLayout.ConditionalRecordNestedGateByteOffset == 0x38
    && CharacterTookHitNativeLayout.ConditionalRecordSecondQwordOffset == 0x40
    && CharacterTookHitNativeLayout.ConditionalRecordDwordOffset == 0x48
    && CharacterTookHitNativeLayout.ConditionalRecordByteOffset == 0x4C
    && CharacterTookHitNativeLayout.FirstTrailingByteFieldOffset == 0x50
    && CharacterTookHitNativeLayout.SecondTrailingByteFieldOffset == 0x51
    && CharacterTookHitNativeLayout.TrailingWordFieldOffset == 0x58
    && CharacterTookHitNativeLayout.FinalByteFieldOffset == 0x5A
    && CharacterTookHitNativeLayout.MinimumInputByteCount == 6);
Check("AlmostHit selector 0x6A records its QWORD field layout",
    CharacterAlmostHitNativeLayout.InnerSelector == 0x6A
    && CharacterAlmostHitNativeLayout.InnerSelectorTableVirtualAddress == 0xC22444
    && CharacterAlmostHitNativeLayout.InnerSelectorTableFileOffset == 0x821844
    && CharacterAlmostHitNativeLayout.CaseFileOffset == 0x81D344
    && CharacterAlmostHitNativeLayout.CaseLocalDwordValue == 0x72
    && CharacterAlmostHitNativeLayout.AllocationSize == 0x30
    && CharacterAlmostHitNativeLayout.AllocationFunctionFileOffset == 0x12B7FB0
    && CharacterAlmostHitNativeLayout.ConstructorFileOffset == 0x525DD0
    && CharacterAlmostHitNativeLayout.BaseConstructorHelperFileOffset == 0x5A0E50
    && CharacterAlmostHitNativeLayout.BaseConstructorArgument == 0x6A
    && CharacterAlmostHitNativeLayout.AllocationFailureContinuationFileOffset == 0x81F72C
    && CharacterAlmostHitNativeLayout.BodyReaderContinuationFileOffset == 0x81F72E
    && CharacterAlmostHitNativeLayout.BodyReaderFileOffset == 0x356F80
    && CharacterAlmostHitNativeLayout.VtableVirtualAddress == 0x1C67108
    && CharacterAlmostHitNativeLayout.RttiName == ".?AVAlmostHit@Character@firefall@@"
    && CharacterAlmostHitNativeLayout.QwordFieldOffset == 0x28
    && CharacterAlmostHitNativeLayout.NominalInputByteCount == 8);
Check("DealtHit selector 0x6B records its conditional and trailing byte layout",
    CharacterDealtHitNativeLayout.InnerSelector == 0x6B
    && CharacterDealtHitNativeLayout.InnerSelectorTableVirtualAddress == 0xC22444
    && CharacterDealtHitNativeLayout.InnerSelectorTableFileOffset == 0x821844
    && CharacterDealtHitNativeLayout.CaseFileOffset == 0x81D375
    && CharacterDealtHitNativeLayout.CaseLocalDwordValue == 0x73
    && CharacterDealtHitNativeLayout.AllocationSize == 0x58
    && CharacterDealtHitNativeLayout.AllocationFunctionFileOffset == 0x12B7FB0
    && CharacterDealtHitNativeLayout.ConstructorFileOffset == 0x525E60
    && CharacterDealtHitNativeLayout.BaseConstructorHelperFileOffset == 0x5A0EC0
    && CharacterDealtHitNativeLayout.BaseConstructorArgument == 0x6B
    && CharacterDealtHitNativeLayout.AllocationFailureContinuationFileOffset == 0x81D39F
    && CharacterDealtHitNativeLayout.BodyReaderContinuationFileOffset == 0x81D3A1
    && CharacterDealtHitNativeLayout.BodyReaderFileOffset == 0x5F0800
    && CharacterDealtHitNativeLayout.ConditionalRecordReaderFileOffset == 0x5F0CF0
    && CharacterDealtHitNativeLayout.ConditionalQwordReaderFileOffset == 0x5FDA00
    && CharacterDealtHitNativeLayout.QwordReaderFileOffset == 0x356F80
    && CharacterDealtHitNativeLayout.DwordReaderFileOffset == 0x353310
    && CharacterDealtHitNativeLayout.ByteReaderFileOffset == 0x337180
    && CharacterDealtHitNativeLayout.VtableVirtualAddress == 0x1C670F8
    && CharacterDealtHitNativeLayout.RttiName == ".?AVDealtHit@Character@firefall@@"
    && CharacterDealtHitNativeLayout.ConditionalRecordGateByteOffset == 0x28
    && CharacterDealtHitNativeLayout.ConditionalRecordFirstQwordOffset == 0x30
    && CharacterDealtHitNativeLayout.ConditionalRecordNestedGateByteOffset == 0x38
    && CharacterDealtHitNativeLayout.ConditionalRecordSecondQwordOffset == 0x40
    && CharacterDealtHitNativeLayout.ConditionalRecordDwordOffset == 0x48
    && CharacterDealtHitNativeLayout.ConditionalRecordByteOffset == 0x4C
    && CharacterDealtHitNativeLayout.FirstTrailingByteFieldOffset == 0x50
    && CharacterDealtHitNativeLayout.SecondTrailingByteFieldOffset == 0x51
    && CharacterDealtHitNativeLayout.MinimumInputByteCount == 3);
Check("Killed selector 0x6C records its word, QWORD, and trailing byte layout",
    CharacterKilledNativeLayout.InnerSelector == 0x6C
    && CharacterKilledNativeLayout.InnerSelectorTableVirtualAddress == 0xC22444
    && CharacterKilledNativeLayout.InnerSelectorTableFileOffset == 0x821844
    && CharacterKilledNativeLayout.CaseFileOffset == 0x81D3C6
    && CharacterKilledNativeLayout.CaseLocalDwordValue == 0x74
    && CharacterKilledNativeLayout.AllocationSize == 0x40
    && CharacterKilledNativeLayout.AllocationFunctionFileOffset == 0x12B7FB0
    && CharacterKilledNativeLayout.ConstructorFileOffset == 0x525F20
    && CharacterKilledNativeLayout.BaseConstructorHelperFileOffset == 0x5A0F30
    && CharacterKilledNativeLayout.BaseConstructorArgument == 0x6C
    && CharacterKilledNativeLayout.AllocationFailureContinuationFileOffset == 0x81D3F0
    && CharacterKilledNativeLayout.BodyReaderContinuationFileOffset == 0x81D3F2
    && CharacterKilledNativeLayout.BodyReaderFileOffset == 0x5F0560
    && CharacterKilledNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterKilledNativeLayout.QwordReaderFileOffset == 0x356F80
    && CharacterKilledNativeLayout.ByteReaderFileOffset == 0x337180
    && CharacterKilledNativeLayout.VtableVirtualAddress == 0x1C670E8
    && CharacterKilledNativeLayout.RttiName == ".?AVKilled@Character@firefall@@"
    && CharacterKilledNativeLayout.WordFieldOffset == 0x28
    && CharacterKilledNativeLayout.QwordFieldOffset == 0x30
    && CharacterKilledNativeLayout.FirstByteFieldOffset == 0x38
    && CharacterKilledNativeLayout.SecondByteFieldOffset == 0x39
    && CharacterKilledNativeLayout.ThirdByteFieldOffset == 0x3A
    && CharacterKilledNativeLayout.NominalInputByteCount == 13);
Check("WarnLockTargeted selector 0x6D records its constructor-only case",
    CharacterWarnLockTargetedNativeLayout.InnerSelector == 0x6D
    && CharacterWarnLockTargetedNativeLayout.InnerSelectorTableVirtualAddress == 0xC22444
    && CharacterWarnLockTargetedNativeLayout.InnerSelectorTableFileOffset == 0x821844
    && CharacterWarnLockTargetedNativeLayout.CaseFileOffset == 0x81D417
    && CharacterWarnLockTargetedNativeLayout.CaseLocalDwordValue == 0x75
    && CharacterWarnLockTargetedNativeLayout.ConstructorCallSiteFileOffset == 0x81D43C
    && CharacterWarnLockTargetedNativeLayout.SuccessPathReturnEpilogueFileOffset == 0x81D441
    && CharacterWarnLockTargetedNativeLayout.AllocationSize == 0x28
    && CharacterWarnLockTargetedNativeLayout.AllocationFunctionFileOffset == 0x12B7FB0
    && CharacterWarnLockTargetedNativeLayout.ConstructorFileOffset == 0x525FB0
    && CharacterWarnLockTargetedNativeLayout.BaseConstructorHelperFileOffset == 0x5A0FA0
    && CharacterWarnLockTargetedNativeLayout.BaseConstructorArgument == 0x6D
    && CharacterWarnLockTargetedNativeLayout.CommonQwordFirstDwordFieldOffset == 0x18
    && CharacterWarnLockTargetedNativeLayout.CommonQwordSecondDwordFieldOffset == 0x1C
    && CharacterWarnLockTargetedNativeLayout.ConstructorInitializedEndOffsetExclusive
        == CharacterWarnLockTargetedNativeLayout.AllocationSize
    && CharacterWarnLockTargetedNativeLayout.AllocationFailureContinuationFileOffset == 0x8217D9
    && CharacterWarnLockTargetedNativeLayout.BodyReaderContinuationFileOffset == 0
    && CharacterWarnLockTargetedNativeLayout.BodyReaderFileOffset == 0
    && CharacterWarnLockTargetedNativeLayout.VtableVirtualAddress == 0x1C670C8
    && CharacterWarnLockTargetedNativeLayout.RttiName == ".?AVWarnLockTargeted@Character@firefall@@");
Check("CurrentPoseUpdate selector 0x6E records its gated scalar and block layout",
    CharacterCurrentPoseUpdateNativeLayout.InnerSelector == 0x6E
    && CharacterCurrentPoseUpdateNativeLayout.InnerSelectorTableVirtualAddress == 0xC22444
    && CharacterCurrentPoseUpdateNativeLayout.InnerSelectorTableFileOffset == 0x821844
    && CharacterCurrentPoseUpdateNativeLayout.CaseFileOffset == 0x81D454
    && CharacterCurrentPoseUpdateNativeLayout.CaseLocalDwordValue == 0x76
    && CharacterCurrentPoseUpdateNativeLayout.AllocationSize == 0x58
    && CharacterCurrentPoseUpdateNativeLayout.AllocationFunctionFileOffset == 0x12B7FB0
    && CharacterCurrentPoseUpdateNativeLayout.ConstructorFileOffset == 0x526030
    && CharacterCurrentPoseUpdateNativeLayout.BaseConstructorHelperFileOffset == 0x5A1010
    && CharacterCurrentPoseUpdateNativeLayout.BaseConstructorArgument == 0x6E
    && CharacterCurrentPoseUpdateNativeLayout.AllocationFailureContinuationFileOffset == 0x81D47E
    && CharacterCurrentPoseUpdateNativeLayout.BodyReaderContinuationFileOffset == 0x81D480
    && CharacterCurrentPoseUpdateNativeLayout.BodyReaderFileOffset == 0x5EFE30
    && CharacterCurrentPoseUpdateNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterCurrentPoseUpdateNativeLayout.ByteReaderFileOffset == 0x337180
    && CharacterCurrentPoseUpdateNativeLayout.DwordReaderFileOffset == 0x3568E0
    && CharacterCurrentPoseUpdateNativeLayout.ThreeByteReaderFileOffset == 0x336B50
    && CharacterCurrentPoseUpdateNativeLayout.AlternateWordReaderFileOffset == 0x6097B0
    && CharacterCurrentPoseUpdateNativeLayout.AlternateByteReaderFileOffset == 0x356CF0
    && CharacterCurrentPoseUpdateNativeLayout.VtableVirtualAddress == 0x1C670D8
    && CharacterCurrentPoseUpdateNativeLayout.RttiName == ".?AVCurrentPoseUpdate@Character@firefall@@"
    && CharacterCurrentPoseUpdateNativeLayout.GateByteOffset == 0x28
    && CharacterCurrentPoseUpdateNativeLayout.FirstConditionalByteOffset == 0x29
    && CharacterCurrentPoseUpdateNativeLayout.FirstConditionalWordOffset == 0x2A
    && CharacterCurrentPoseUpdateNativeLayout.AlwaysByteOffset == 0x2C
    && CharacterCurrentPoseUpdateNativeLayout.SecondConditionalWordOffset == 0x2E
    && CharacterCurrentPoseUpdateNativeLayout.ConditionalThreeByteBlockOffset == 0x30
    && CharacterCurrentPoseUpdateNativeLayout.ConditionalDwordBlockOffset == 0x34
    && CharacterCurrentPoseUpdateNativeLayout.MidModeSingleByteOffset == 0x40
    && CharacterCurrentPoseUpdateNativeLayout.MidModeThreeByteBlockOffset == 0x41
    && CharacterCurrentPoseUpdateNativeLayout.MidModeFourWordBlockOffset == 0x44
    && CharacterCurrentPoseUpdateNativeLayout.TailModeThreeByteBlockOffset == 0x4C
    && CharacterCurrentPoseUpdateNativeLayout.TailModeThreeWordBlockOffset == 0x50);
Check("ConfirmedPoseUpdate selector 0x6F records its conditional nested layout",
    CharacterConfirmedPoseUpdateNativeLayout.InnerSelector == 0x6F
    && CharacterConfirmedPoseUpdateNativeLayout.InnerSelectorTableVirtualAddress == 0xC22444
    && CharacterConfirmedPoseUpdateNativeLayout.InnerSelectorTableFileOffset == 0x821844
    && CharacterConfirmedPoseUpdateNativeLayout.CaseFileOffset == 0x81D4A6
    && CharacterConfirmedPoseUpdateNativeLayout.CaseLocalDwordValue == 0x77
    && CharacterConfirmedPoseUpdateNativeLayout.AllocationSize == 0xC0
    && CharacterConfirmedPoseUpdateNativeLayout.AllocationFunctionFileOffset == 0x12B7FB0
    && CharacterConfirmedPoseUpdateNativeLayout.ConstructorFileOffset == 0x5260B0
    && CharacterConfirmedPoseUpdateNativeLayout.BaseConstructorHelperFileOffset == 0x5A1090
    && CharacterConfirmedPoseUpdateNativeLayout.BaseConstructorArgument == 0x6F
    && CharacterConfirmedPoseUpdateNativeLayout.AllocationFailureContinuationFileOffset == 0x81D4D3
    && CharacterConfirmedPoseUpdateNativeLayout.BodyReaderContinuationFileOffset == 0x81D4D5
    && CharacterConfirmedPoseUpdateNativeLayout.BodyReaderFileOffset == 0x5EFAB0
    && CharacterConfirmedPoseUpdateNativeLayout.ConditionalRecordReaderFileOffset == 0x5EF8D0
    && CharacterConfirmedPoseUpdateNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterConfirmedPoseUpdateNativeLayout.ByteReaderFileOffset == 0x337180
    && CharacterConfirmedPoseUpdateNativeLayout.DwordReaderFileOffset == 0x3568E0
    && CharacterConfirmedPoseUpdateNativeLayout.FourDwordBlockReaderFileOffset == 0x384B40
    && CharacterConfirmedPoseUpdateNativeLayout.AlternateWordReaderFileOffset == 0x6097B0
    && CharacterConfirmedPoseUpdateNativeLayout.NestedFieldReaderFileOffset == 0x5FFB00
    && CharacterConfirmedPoseUpdateNativeLayout.VtableVirtualAddress == 0x1C670B8
    && CharacterConfirmedPoseUpdateNativeLayout.RttiName == ".?AVConfirmedPoseUpdate@Character@firefall@@"
    && CharacterConfirmedPoseUpdateNativeLayout.InitialWordFieldOffset == 0xB4
    && CharacterConfirmedPoseUpdateNativeLayout.FirstGateByteOffset == 0xA6
    && CharacterConfirmedPoseUpdateNativeLayout.SecondGateByteOffset == 0xA7
    && CharacterConfirmedPoseUpdateNativeLayout.FirstConditionalDwordBlockOffset == 0x2C
    && CharacterConfirmedPoseUpdateNativeLayout.ConditionalSixteenByteBlockOffset == 0x78
    && CharacterConfirmedPoseUpdateNativeLayout.ConditionalFollowingWordOffset == 0xA4
    && CharacterConfirmedPoseUpdateNativeLayout.AlwaysDwordBlockOffset == 0x98
    && CharacterConfirmedPoseUpdateNativeLayout.SecondConditionalDwordBlockOffset == 0x8C
    && CharacterConfirmedPoseUpdateNativeLayout.TrailingWordBlockOffset == 0xA8
    && CharacterConfirmedPoseUpdateNativeLayout.NestedFieldOffset == 0xB0
    && CharacterConfirmedPoseUpdateNativeLayout.FinalWordFieldOffset == 0xB8
    && CharacterConfirmedPoseUpdateNativeLayout.ConstructorInitializedWordOffset == 0x88
    && CharacterConfirmedPoseUpdateNativeLayout.ConstructorInitializedWordValue == 0x0101
    && CharacterConfirmedPoseUpdateNativeLayout.ConstructorInitializedDwordOffset == 0xB0
    && CharacterConfirmedPoseUpdateNativeLayout.ConstructorInitializedDwordValue == 0);
Check("PublicDebugMovementUpdate selector 0x70 records its seven-element nested layout",
    CharacterPublicDebugMovementUpdateNativeLayout.InnerSelector == 0x70
    && CharacterPublicDebugMovementUpdateNativeLayout.InnerSelectorTableEntryOffset == 0x74
    && CharacterPublicDebugMovementUpdateNativeLayout.CaseVirtualAddress == 0xC1E0FA
    && CharacterPublicDebugMovementUpdateNativeLayout.CaseFileOffset == 0x81D4FA
    && CharacterPublicDebugMovementUpdateNativeLayout.CaseLocalDwordValue == 0x78
    && CharacterPublicDebugMovementUpdateNativeLayout.AllocationSize == 0x5A0
    && CharacterPublicDebugMovementUpdateNativeLayout.ConstructorFileOffset == 0x526180
    && CharacterPublicDebugMovementUpdateNativeLayout.BaseConstructorHelperFileOffset == 0x5A1100
    && CharacterPublicDebugMovementUpdateNativeLayout.BaseConstructorArgument == 0x70
    && CharacterPublicDebugMovementUpdateNativeLayout.ArrayInitializerFileOffset == 0x60E6A0
    && CharacterPublicDebugMovementUpdateNativeLayout.BodyReaderFileOffset == 0x5FF970
    && CharacterPublicDebugMovementUpdateNativeLayout.ArrayOffset == 0x28
    && CharacterPublicDebugMovementUpdateNativeLayout.ArrayElementCount == 7
    && CharacterPublicDebugMovementUpdateNativeLayout.ArrayElementStride == 0xC8
    && CharacterPublicDebugMovementUpdateNativeLayout.ElementFirstWordOffset == 0
    && CharacterPublicDebugMovementUpdateNativeLayout.ElementNestedRecordOffset == 4
    && CharacterPublicDebugMovementUpdateNativeLayout.ElementSecondWordOffset == 0x70
    && CharacterPublicDebugMovementUpdateNativeLayout.ElementTaggedByteOffset == 0x74
    && CharacterPublicDebugMovementUpdateNativeLayout.NestedRecordFourDwordBlockOffset == 0x50
    && CharacterPublicDebugMovementUpdateNativeLayout.NestedRecordFourDwordBlockSize == 0x10
    && CharacterPublicDebugMovementUpdateNativeLayout.NestedRecordPresenceWordOffset == 0x60
    && CharacterPublicDebugMovementUpdateNativeLayout.NestedRecordTrailingDwordsOffset == 0x64
    && CharacterPublicDebugMovementUpdateNativeLayout.NestedRecordTrailingDwordCount == 3
    && CharacterPublicDebugMovementUpdateNativeLayout.ConstructorInitializedWordValue == 0x0101
    && CharacterPublicDebugMovementUpdateNativeLayout.ReaderWrittenPresenceWordValue == 1
    && CharacterPublicDebugMovementUpdateNativeLayout.ConstructorInitializedWordOffsets
        .SequenceEqual([0x88, 0x150, 0x218, 0x2E0, 0x3A8, 0x470, 0x538])
    && CharacterPublicDebugMovementUpdateNativeLayout.ConstructorInitializedByteOffsets
        .SequenceEqual([0x9C, 0x164, 0x22C, 0x2F4, 0x3BC, 0x484, 0x54C]));
Check("ForcedMovement selector 0x71 records its initialized nested state and trailing WORD",
    CharacterForcedMovementNativeLayout.InnerSelector == 0x71
    && CharacterForcedMovementNativeLayout.InnerSelectorTableEntryOffset == 0x78
    && CharacterForcedMovementNativeLayout.CaseVirtualAddress == 0xC1E14F
    && CharacterForcedMovementNativeLayout.CaseFileOffset == 0x81D54F
    && CharacterForcedMovementNativeLayout.CaseLocalDwordValue == 0x79
    && CharacterForcedMovementNativeLayout.AllocationSize == 0x1E8
    && CharacterForcedMovementNativeLayout.ConstructorFileOffset == 0x526230
    && CharacterForcedMovementNativeLayout.BaseConstructorHelperFileOffset == 0x5A1170
    && CharacterForcedMovementNativeLayout.BaseConstructorArgument == 0x71
    && CharacterForcedMovementNativeLayout.NestedStateInitializerFileOffset == 0x92B8A0
    && CharacterForcedMovementNativeLayout.BodyReaderFileOffset == 0x5EF690
    && CharacterForcedMovementNativeLayout.DiscriminatedStateReaderFileOffset == 0x92F800
    && CharacterForcedMovementNativeLayout.ConditionalPairReaderFileOffset == 0x5FDA00
    && CharacterForcedMovementNativeLayout.DwordReaderFileOffset == 0x3370A0
    && CharacterForcedMovementNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterForcedMovementNativeLayout.NestedStateOffset == 0x28
    && CharacterForcedMovementNativeLayout.NestedStateSize == 0x1B8
    && CharacterForcedMovementNativeLayout.NestedStateDwordOffset == 0x2C
    && CharacterForcedMovementNativeLayout.DiscriminatorByteOffset == 0x30
    && CharacterForcedMovementNativeLayout.ConditionalPairOffset == 0x138
    && CharacterForcedMovementNativeLayout.ConditionalPairSize == 8
    && CharacterForcedMovementNativeLayout.DiscriminatorCaseCount == 14
    && CharacterForcedMovementNativeLayout.TrailingWordOffset == 0x1E0
    && CharacterForcedMovementNativeLayout.ConstructorInitializedDwords.SequenceEqual(
        [(0x28, 0u), (0x2C, 0u), (0x30, 0u), (0x114, 0u), (0x118, 0u),
            (0x138, 0u), (0x13C, 0u), (0x1B8, 0u), (0x1BC, 0u), (0x1C0, 0u), (0x1C4, 0u)])
    && CharacterForcedMovementNativeLayout.ConstructorInitializedWords.SequenceEqual(
        [(0x34, (ushort)0), (0x94, (ushort)0x0101), (0x100, (ushort)0x0101),
            (0x110, (ushort)0), (0x19C, (ushort)0x0101), (0x1CC, (ushort)0)]));
Check("ForcedMovementCancelled selector 0x72 records its DWORD and WORD body fields",
    CharacterForcedMovementCancelledNativeLayout.InnerSelector == 0x72
    && CharacterForcedMovementCancelledNativeLayout.InnerSelectorTableEntryOffset == 0x7C
    && CharacterForcedMovementCancelledNativeLayout.CaseVirtualAddress == 0xC1E1A3
    && CharacterForcedMovementCancelledNativeLayout.CaseFileOffset == 0x81D5A3
    && CharacterForcedMovementCancelledNativeLayout.CaseLocalDwordValue == 0x7A
    && CharacterForcedMovementCancelledNativeLayout.AllocationSize == 0x30
    && CharacterForcedMovementCancelledNativeLayout.ConstructorFileOffset == 0x5262C0
    && CharacterForcedMovementCancelledNativeLayout.BaseConstructorHelperFileOffset == 0x5A11E0
    && CharacterForcedMovementCancelledNativeLayout.BaseConstructorArgument == 0x72
    && CharacterForcedMovementCancelledNativeLayout.BodyReaderFileOffset == 0x5EF490
    && CharacterForcedMovementCancelledNativeLayout.DwordReaderFileOffset == 0x3370A0
    && CharacterForcedMovementCancelledNativeLayout.WordReaderFileOffset == 0x336AC0
    && CharacterForcedMovementCancelledNativeLayout.FirstReaderFieldOffset == 0x28
    && CharacterForcedMovementCancelledNativeLayout.SecondReaderFieldOffset == 0x2C);
Check("GrappleClimbPermission selector 0x73 records its initialized nested state",
    CharacterGrappleClimbPermissionNativeLayout.InnerSelector == 0x73
    && CharacterGrappleClimbPermissionNativeLayout.InnerSelectorTableEntryOffset == 0x80
    && CharacterGrappleClimbPermissionNativeLayout.CaseVirtualAddress == 0xC1E1F4
    && CharacterGrappleClimbPermissionNativeLayout.CaseFileOffset == 0x81D5F4
    && CharacterGrappleClimbPermissionNativeLayout.CaseLocalDwordValue == 0x7B
    && CharacterGrappleClimbPermissionNativeLayout.AllocationSize == 0x1E0
    && CharacterGrappleClimbPermissionNativeLayout.ConstructorFileOffset == 0x526340
    && CharacterGrappleClimbPermissionNativeLayout.BaseConstructorHelperFileOffset == 0x5A1250
    && CharacterGrappleClimbPermissionNativeLayout.BaseConstructorArgument == 0x73
    && CharacterGrappleClimbPermissionNativeLayout.NestedStateInitializerFileOffset == 0x92B8A0
    && CharacterGrappleClimbPermissionNativeLayout.BodyReaderFileOffset == 0x92F800
    && CharacterGrappleClimbPermissionNativeLayout.ConditionalPairReaderFileOffset == 0x5FDA00
    && CharacterGrappleClimbPermissionNativeLayout.DwordReaderFileOffset == 0x3370A0
    && CharacterGrappleClimbPermissionNativeLayout.NestedStateOffset == 0x28
    && CharacterGrappleClimbPermissionNativeLayout.NestedStateSize == 0x1B8
    && CharacterGrappleClimbPermissionNativeLayout.NestedStateDwordOffset == 0x2C
    && CharacterGrappleClimbPermissionNativeLayout.DiscriminatorByteOffset == 0x30
    && CharacterGrappleClimbPermissionNativeLayout.ConditionalPairOffset == 0x138
    && CharacterGrappleClimbPermissionNativeLayout.ConditionalPairSize == 8
    && CharacterGrappleClimbPermissionNativeLayout.DiscriminatorCaseCount == 14
    && CharacterGrappleClimbPermissionNativeLayout.ConstructorInitializedDwords
        .SequenceEqual(CharacterForcedMovementNativeLayout.ConstructorInitializedDwords)
    && CharacterGrappleClimbPermissionNativeLayout.ConstructorInitializedWords
        .SequenceEqual(CharacterForcedMovementNativeLayout.ConstructorInitializedWords));
Check("AbilityActivated selector 0x74 records its two DWORDs and trailing slBlob",
    CharacterAbilityActivatedNativeLayout.InnerSelector == 0x74
    && CharacterAbilityActivatedNativeLayout.InnerSelectorTableEntryOffset == 0x84
    && CharacterAbilityActivatedNativeLayout.CaseVirtualAddress == 0xC1E249
    && CharacterAbilityActivatedNativeLayout.CaseFileOffset == 0x81D649
    && CharacterAbilityActivatedNativeLayout.CaseLocalDwordValue == 0x7C
    && CharacterAbilityActivatedNativeLayout.AllocationSize == 0x40
    && CharacterAbilityActivatedNativeLayout.ConstructorFileOffset == 0x5263D0
    && CharacterAbilityActivatedNativeLayout.BaseConstructorHelperFileOffset == 0x5A12C0
    && CharacterAbilityActivatedNativeLayout.BaseConstructorArgument == 0x74
    && CharacterAbilityActivatedNativeLayout.BodyReaderFileOffset == 0x5EF1F0
    && CharacterAbilityActivatedNativeLayout.DwordReaderFileOffset == 0x3370A0
    && CharacterAbilityActivatedNativeLayout.FirstReaderFieldOffset == 0x28
    && CharacterAbilityActivatedNativeLayout.SecondReaderFieldOffset == 0x2C
    && CharacterAbilityActivatedNativeLayout.PayloadOffset == 0x30
    && CharacterAbilityActivatedNativeLayout.PayloadStateSize == 0x10
    && CharacterAbilityActivatedNativeLayout.PayloadStateInitializer == 0x1C80CC4);
Check("AbilityFailed selector 0x75 records its two DWORDs and trailing slBlob",
    CharacterAbilityFailedNativeLayout.InnerSelector == 0x75
    && CharacterAbilityFailedNativeLayout.InnerSelectorTableEntryOffset == 0x88
    && CharacterAbilityFailedNativeLayout.CaseVirtualAddress == 0xC1E29A
    && CharacterAbilityFailedNativeLayout.CaseFileOffset == 0x81D69A
    && CharacterAbilityFailedNativeLayout.CaseLocalDwordValue == 0x7D
    && CharacterAbilityFailedNativeLayout.AllocationSize == 0x40
    && CharacterAbilityFailedNativeLayout.ConstructorFileOffset == 0x5264C0
    && CharacterAbilityFailedNativeLayout.BaseConstructorHelperFileOffset == 0x5A1330
    && CharacterAbilityFailedNativeLayout.BaseConstructorArgument == 0x75
    && CharacterAbilityFailedNativeLayout.BodyReaderFileOffset == 0x5EF090
    && CharacterAbilityFailedNativeLayout.DwordReaderFileOffset == 0x3370A0
    && CharacterAbilityFailedNativeLayout.FirstReaderFieldOffset == 0x28
    && CharacterAbilityFailedNativeLayout.SecondReaderFieldOffset == 0x2C
    && CharacterAbilityFailedNativeLayout.PayloadOffset == 0x30
    && CharacterAbilityFailedNativeLayout.PayloadStateSize == 0x10
    && CharacterAbilityFailedNativeLayout.PayloadStateInitializer == 0x1C80CC4);
Check("AbilityCooldowns selector 0x76 records an slBlob state",
    CharacterAbilityCooldownsNativeLayout.InnerSelector == 0x76
    && CharacterAbilityCooldownsNativeLayout.InnerSelectorTableEntryOffset == 0x8C
    && CharacterAbilityCooldownsNativeLayout.CaseVirtualAddress == 0xC1E2EB
    && CharacterAbilityCooldownsNativeLayout.CaseFileOffset == 0x81D6EB
    && CharacterAbilityCooldownsNativeLayout.CaseLocalDwordValue == 0x7E
    && CharacterAbilityCooldownsNativeLayout.AllocationSize == 0x38
    && CharacterAbilityCooldownsNativeLayout.ConstructorFileOffset == 0x5265B0
    && CharacterAbilityCooldownsNativeLayout.BaseConstructorHelperFileOffset == 0x5A13C0
    && CharacterAbilityCooldownsNativeLayout.BaseConstructorArgument == 0x76
    && CharacterAbilityCooldownsNativeLayout.BodyReaderFileOffset == 0x5EEE60
    && CharacterAbilityCooldownsNativeLayout.PayloadOffset == 0x28
    && CharacterAbilityCooldownsNativeLayout.PayloadStateSize == 0x10
    && CharacterAbilityCooldownsNativeLayout.PayloadStateInitializer == 0x1C80CC4);
Check("NPCInteraction selector 0x77 records its raw QWORD body field",
    CharacterNPCInteractionNativeLayout.InnerSelector == 0x77
    && CharacterNPCInteractionNativeLayout.InnerSelectorTableEntryOffset == 0x90
    && CharacterNPCInteractionNativeLayout.CaseVirtualAddress == 0xC1E33C
    && CharacterNPCInteractionNativeLayout.CaseFileOffset == 0x81D73C
    && CharacterNPCInteractionNativeLayout.CaseLocalDwordValue == 0x7F
    && CharacterNPCInteractionNativeLayout.AllocationSize == 0x30
    && CharacterNPCInteractionNativeLayout.ConstructorFileOffset == 0x5266A0
    && CharacterNPCInteractionNativeLayout.BaseConstructorHelperFileOffset == 0x5A1430
    && CharacterNPCInteractionNativeLayout.BaseConstructorArgument == 0x77
    && CharacterNPCInteractionNativeLayout.BodyReaderFileOffset == 0x81F72E
    && CharacterNPCInteractionNativeLayout.BodyReaderQwordCallSiteFileOffset == 0x81F73C
    && CharacterNPCInteractionNativeLayout.BodyReaderReturnPointerLoadFileOffset == 0x81F741
    && CharacterNPCInteractionNativeLayout.QwordReaderFileOffset == 0x356F80
    && CharacterNPCInteractionNativeLayout.FirstQwordFieldOffset == 0x28
    && CharacterNPCInteractionNativeLayout.FirstQwordFieldSize == 0x08
    && CharacterNPCInteractionNativeLayout.ConstructorInitializedEndOffsetExclusive == 0x30
    && CharacterNPCInteractionNativeLayout.ParserBodyReadEndOffsetExclusive == 0x30
    && CharacterNPCInteractionNativeLayout.ConsumerStateBufferOffset == 0x30
    && CharacterNPCInteractionNativeLayout.ConsumerStateBufferByteCount == 0x20
    && CharacterNPCInteractionNativeLayout.ConsumerStateBufferEndOffsetExclusive == 0x50
    && CharacterNPCInteractionNativeLayout.VtableWriterMethodVirtualAddress == 0x9EF950
    && CharacterNPCInteractionNativeLayout.VtableWriterFileOffset == 0x5EED50
    && CharacterNPCInteractionNativeLayout.VtableReaderMethodVirtualAddress == 0x9EF8E0
    && CharacterNPCInteractionNativeLayout.VtableReaderFileOffset == 0x5EECE0
    && CharacterNPCInteractionNativeLayout.VtableDeletingDestructorMethodVirtualAddress == 0x9272E0
    && CharacterNPCInteractionNativeLayout.VtableDeletingDestructorFileOffset == 0x5266E0);
Check("Character receiver behavior model preserves selector slots and observed application paths",
    CharacterAbilityApplicationNativeLayout.ReceiverRttiName == ".?AVtfGameProtocolHandler@@"
    && CharacterAbilityApplicationNativeLayout.ReceiverConstructorFileOffset == 0xAC8CA0
    && CharacterAbilityApplicationNativeLayout.ReceiverVtableVirtualAddress == 0x1C71EC0
    && CharacterAbilityApplicationNativeLayout.ReceiverVtableFileOffset == 0x1870AC0
    && CharacterAbilityApplicationNativeLayout.ReceiverHandlers.SequenceEqual(
    [
        new CharacterAbilityReceiverHandlerSnapshot(0x74, 0x154, 0xECE940, 0xACDD40),
        new CharacterAbilityReceiverHandlerSnapshot(0x75, 0x158, 0xECE9E0, 0xACDDE0),
        new CharacterAbilityReceiverHandlerSnapshot(0x76, 0x15C, 0xECEE10, 0xACE210),
        new CharacterAbilityReceiverHandlerSnapshot(0x77, 0x160, 0xECEEA0, 0xACE2A0)
    ])
    && CharacterAbilityApplicationNativeLayout.AbilityActivatedEventPayloadTag == 1
    && CharacterAbilityApplicationNativeLayout.AbilityActivatedNumericApplicationEventCode == 230
    && CharacterAbilityApplicationNativeLayout.AbilityFailedApplicationHelperFileOffset == 0x909C60
    && CharacterAbilityApplicationNativeLayout.AbilityFailedSourceTreeRangeCopyFileOffset == 0x90C580
    && CharacterAbilityApplicationNativeLayout.AbilityFailedSourceTreeEntryCloneFileOffset == 0x82C2E0
    && CharacterAbilityApplicationNativeLayout.AbilityFailedOwnerTreeInsertFileOffset == 0x8410B0
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsObservedRecordStride == 0x40
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsBeginOffset == 0x28
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsEndOffset == 0x2C
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsObservedAuthorIdOffset == 0
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsObservedAuthorTextOffset == 0x08
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsObservedMessageTextOffset == 0x0C
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsObservedNativeChannelOffset == 0x10
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsObservedDisplayFlagsOffset == 0x11
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsObservedTemplateSelectorOffset == 0x12
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsChatProcessorFileOffset == 0x6E2590
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsFormattedMessageHelperFileOffset == 0x6E3670
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsDisplayNotificationFileOffset == 0x6E3460
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsDisplayNotificationMessageId == 335
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsObservedOptionalBranchOffset == 0x14
    && CharacterAbilityApplicationNativeLayout.AbilityCooldownsObservedOptionalDataOffset == 0x18
    && CharacterAbilityApplicationNativeLayout.NpcInteractionMessagePointerLoadFileOffset == 0xACE2B8
    && CharacterAbilityApplicationNativeLayout.NpcInteractionMessageKeyOffset == 0x28
    && CharacterAbilityApplicationNativeLayout.NpcInteractionMessageStateOffset == 0x30
    && CharacterAbilityApplicationNativeLayout.NpcInteractionApplicationHelperCallFileOffset == 0xACE2CC
    && CharacterAbilityApplicationNativeLayout.NpcInteractionMessageStateArgumentLoadFileOffset == 0x4C927E
    && CharacterAbilityApplicationNativeLayout.NpcInteractionMessageStateArgumentPushFileOffset == 0x4C92B9
    && CharacterAbilityApplicationNativeLayout.NpcInteractionMessageStateSwapCallFileOffset == 0x4C92BD
    && CharacterAbilityApplicationNativeLayout.NpcInteractionOwnerMapOffset == 0x4338
    && CharacterAbilityApplicationNativeLayout.NpcInteractionMessageStateSwapByteCount == 0x20
    && CharacterAbilityApplicationNativeLayout.NpcInteractionMessageStateEndOffsetExclusive == 0x50
    && CharacterAbilityApplicationNativeLayout.NpcInteractionNumericApplicationEventCode == 356
    && CharacterAbilityApplicationNativeLayout.NpcInteractionEventPayloadTag == 5);
Check("Character movement application receiver model records concrete slots, helpers, and observed layout conflicts",
    CharacterMovementApplicationNativeLayout.ReceiverRttiName == ".?AVtfGameProtocolHandler@@"
    && CharacterMovementApplicationNativeLayout.ReceiverConstructorFileOffset == 0xAC8CA0
    && CharacterMovementApplicationNativeLayout.ReceiverVtableVirtualAddress == 0x1C71EC0
    && CharacterMovementApplicationNativeLayout.ReceiverVtableFileOffset == 0x1870AC0
    && CharacterMovementApplicationNativeLayout.MessageDispatcherFileOffset == 0x81BB90
    && CharacterMovementApplicationNativeLayout.MessageDispatcherSelectorTableVirtualAddress == 0xC1D6DC
    && CharacterMovementApplicationNativeLayout.MessageDispatcherSelectorTableFileOffset == 0x81CADC
    && CharacterMovementApplicationNativeLayout.ReceiverHandlers.SequenceEqual(
    [
        new CharacterMovementReceiverHandlerSnapshot(0x70, 0xC1D4E0, 0x81C8E0, 0x144,
            0xECEBF0, 0xACDFF0, 0xC245A0, 363),
        new CharacterMovementReceiverHandlerSnapshot(0x71, 0xC1D50B, 0x81C90B, 0x148,
            0xECED30, 0xACE130, 0xC24780, 365),
        new CharacterMovementReceiverHandlerSnapshot(0x72, 0xC1D536, 0x81C936, 0x14C,
            0xECB670, 0xACAA70, 0x9090A0, null),
        new CharacterMovementReceiverHandlerSnapshot(0x73, 0xC1D561, 0x81C961, 0x150,
            0xECB6C0, 0xACAAC0, 0x8EDD10, 213)
    ])
    && CharacterMovementApplicationNativeLayout.ReceiverHandlers.All(handler =>
        handler.VtableSlotOffset
            == CharacterMovementApplicationNativeLayout.VtableSlotBaseOffset
                + CharacterMovementApplicationNativeLayout.VtableEntrySize
                    * (handler.MessageSelector - CharacterMovementApplicationNativeLayout.FirstMessageSelector)
        && handler.DispatcherCaseVirtualAddress == handler.DispatcherCaseFileOffset + 0x400C00
        && handler.MethodVirtualAddress == handler.MethodFileOffset + 0x400C00)
    && CharacterMovementApplicationNativeLayout.PublicDebugMovementFirstReaderFieldWidth == 2
    && CharacterMovementApplicationNativeLayout.PublicDebugMovementFirstHandlerReadWidth == 4
    && CharacterMovementApplicationNativeLayout.PublicDebugMovementFirstHandlerReadOffset == 0x28
    && CharacterMovementApplicationNativeLayout.PublicDebugMovementFirstRecordStateOffset == 0x30
    && CharacterPublicDebugMovementUpdateNativeLayout.ArrayOffset == 0x28
    && CharacterPublicDebugMovementUpdateNativeLayout.ArrayElementCount == 7
    && CharacterPublicDebugMovementUpdateNativeLayout.ArrayElementStride == 0xC8
    && CharacterMovementApplicationNativeLayout.PublicDebugMovementOptionalGateOffset == 0x74
    && CharacterMovementApplicationNativeLayout.PublicDebugMovementOptionalGateReaderFileOffset == 0x5FF760
    && CharacterMovementApplicationNativeLayout.PublicDebugMovementOptionalValueReaderFileOffset == 0x5FF520
    && CharacterMovementApplicationNativeLayout.PublicDebugMovementConditionalPayloadHelperFileOffset == 0xC24780
    && CharacterMovementApplicationNativeLayout.ForcedMovementNestedStateOffset == 0x28
    && CharacterMovementApplicationNativeLayout.ForcedMovementNestedStateFirstDwordValue == 0
    && CharacterMovementApplicationNativeLayout.ForcedMovementNestedReaderFirstWrittenDwordOffset == 0x2C
    && CharacterMovementApplicationNativeLayout.ForcedMovementDiscriminatorOffset == 0x30
    && CharacterMovementApplicationNativeLayout.ForcedMovementConditionalPairOffset == 0x138
    && CharacterMovementApplicationNativeLayout.ForcedMovementTrailingWordOffset == 0x1E0
    && CharacterMovementApplicationNativeLayout.ForcedMovementCancelledInputListPointerOffset
        == CharacterForcedMovementCancelledNativeLayout.FirstReaderFieldOffset
    && CharacterMovementApplicationNativeLayout.ForcedMovementCancelledIgnoredWordOffset
        == CharacterForcedMovementCancelledNativeLayout.SecondReaderFieldOffset
    && CharacterMovementApplicationNativeLayout.ForcedMovementCancelledOwnerStateFieldOffset == 0x43C8
    && CharacterMovementApplicationNativeLayout.ForcedMovementCancelledMissingStateDiagnostic == 1651
    && CharacterMovementApplicationNativeLayout.GrappleClimbPermissionOwnerStateFieldOffset == 0x43B8
    && CharacterMovementApplicationNativeLayout.GrappleClimbPermissionMissingStateDiagnostic == 1646
    && CharacterMovementApplicationNativeLayout.GrappleClimbPermissionNestedStateFirstDwordValue == 0
    && CharacterMovementApplicationNativeLayout.GrappleClimbPermissionNestedReaderFirstWrittenDwordOffset == 0x2C
    && CharacterMovementApplicationNativeLayout.GrappleClimbPermissionMessageValueOffset == 0x28
    && CharacterMovementApplicationNativeLayout.GrappleClimbPermissionMessageListPointerOffset == 0x2C
    && CharacterMovementApplicationNativeLayout.GrappleClimbPermissionManagedRecordStride == 20
    && CharacterMovementApplicationNativeLayout.GrappleClimbPermissionGeneratedRecordStride == 84
    && CharacterMovementApplicationNativeLayout.PublicDebugMovementNumericApplicationEventCode == 363
    && CharacterMovementApplicationNativeLayout.ForcedMovementNumericApplicationEventCode == 365
    && CharacterMovementApplicationNativeLayout.GrappleClimbPermissionNumericApplicationEventCode == 213
    && CharacterMovementApplicationNativeLayout.UiManagerDispatchVtableSlotOffset == 0xAC);
Check("OpenMovieDialog selector 0x78 records its shared text field",
    CharacterOpenMovieDialogNativeLayout.InnerSelector == 0x78
    && CharacterOpenMovieDialogNativeLayout.InnerSelectorTableEntryOffset == 0x94
    && CharacterOpenMovieDialogNativeLayout.CaseVirtualAddress == 0xC1E36D
    && CharacterOpenMovieDialogNativeLayout.CaseFileOffset == 0x81D76D
    && CharacterOpenMovieDialogNativeLayout.CaseLocalDwordValue == 0x80
    && CharacterOpenMovieDialogNativeLayout.AllocationSize == 0x30
    && CharacterOpenMovieDialogNativeLayout.ConstructorFileOffset == 0x526730
    && CharacterOpenMovieDialogNativeLayout.BaseConstructorHelperFileOffset == 0x5A14A0
    && CharacterOpenMovieDialogNativeLayout.BaseConstructorArgument == 0x78
    && CharacterOpenMovieDialogNativeLayout.AllocationFailureContinuationFileOffset == 0x81FB25
    && CharacterOpenMovieDialogNativeLayout.BodyReaderSetupFileOffset == 0x81FB27
    && CharacterOpenMovieDialogNativeLayout.BodyReaderFileOffset == 0x12EE010
    && CharacterOpenMovieDialogNativeLayout.FirstTextFieldOffset == 0x28);
Check("PrivateDialog selector 0x79 records its parser-side DWORD/QWORD/DWORD reader fields",
    CharacterPrivateDialogNativeLayout.InnerSelector == 0x79
    && CharacterPrivateDialogNativeLayout.InnerSelectorTableEntryOffset == 0x98
    && CharacterPrivateDialogNativeLayout.CaseVirtualAddress == 0xC1E39E
    && CharacterPrivateDialogNativeLayout.CaseFileOffset == 0x81D79E
    && CharacterPrivateDialogNativeLayout.CaseLocalDwordValue == 0x81
    && CharacterPrivateDialogNativeLayout.AllocationSize == 0x40
    && CharacterPrivateDialogNativeLayout.ConstructorFileOffset == 0x526840
    && CharacterPrivateDialogNativeLayout.BaseConstructorHelperFileOffset == 0x5A1510
    && CharacterPrivateDialogNativeLayout.BaseConstructorArgument == 0x79
    && CharacterPrivateDialogNativeLayout.BodyReaderFileOffset == 0x5EE900
    && CharacterPrivateDialogNativeLayout.DwordReaderFileOffset == 0x3370A0
    && CharacterPrivateDialogNativeLayout.QwordReaderFileOffset == 0x356F80
    && CharacterPrivateDialogNativeLayout.FirstDwordFieldOffset == 0x28
    && CharacterPrivateDialogNativeLayout.QwordFieldOffset == 0x30
    && CharacterPrivateDialogNativeLayout.SecondDwordFieldOffset == 0x38
    && CharacterPrivateDialogNativeLayout.ParserBodyReadEndOffsetExclusive == 0x3C);
Check("PrivateDialog parser allocation and application receiver extents remain explicitly inconsistent",
    CharacterPrivateDialogNativeLayout.AllocationSize == 0x40
    && CharacterPrivateDialogNativeLayout.ParserBodyReadEndOffsetExclusive == 0x3C
    && DialogApplicationDispatchNativeLayout.PrivateDialogVendorIdOffset == 0x40
    && DialogApplicationDispatchNativeLayout.PrivateDialogCodePointerOffset == 0x44
    && DialogApplicationDispatchNativeLayout.PrivateDialogCodePointerWidth == 4
    && DialogApplicationDispatchNativeLayout.PrivateDialogReceiverFixedExtentEndOffsetExclusive == 0x48
    && DialogApplicationDispatchNativeLayout.PrivateDialogVendorIdOffset
        >= CharacterPrivateDialogNativeLayout.AllocationSize
    && DialogApplicationDispatchNativeLayout.PrivateDialogReceiverFixedExtentEndOffsetExclusive
        > CharacterPrivateDialogNativeLayout.AllocationSize
    && MatrixMessageParserFlowNativeLayout.MessageDispatchVtableCases.Any(
        messageCase => messageCase.MessageDiscriminator == 0x79
            && messageCase.VtableSlotOffset == 0x168)
    && MatrixMessageParserFlowNativeLayout.ReplayReaderMessageReceiver.DialogSlots.Any(
        receiverSlot => receiverSlot.MessageDiscriminator == 0x79
            && receiverSlot.VtableSlotOffset == 0x168
            && receiverSlot.MethodFileOffset == 0xACE300
            && receiverSlot.TailJumpTargetFileOffset == 0x991510));
Check("PublicDialog selector 0x7A records two DWORD reader fields",
    CharacterPublicDialogNativeLayout.InnerSelector == 0x7A
    && CharacterPublicDialogNativeLayout.InnerSelectorTableEntryOffset == 0x9C
    && CharacterPublicDialogNativeLayout.CaseVirtualAddress == 0xC1E3EF
    && CharacterPublicDialogNativeLayout.CaseFileOffset == 0x81D7EF
    && CharacterPublicDialogNativeLayout.CaseLocalDwordValue == 0x82
    && CharacterPublicDialogNativeLayout.AllocationSize == 0x30
    && CharacterPublicDialogNativeLayout.ConstructorFileOffset == 0x5268D0
    && CharacterPublicDialogNativeLayout.BaseConstructorHelperFileOffset == 0x5A1580
    && CharacterPublicDialogNativeLayout.BaseConstructorArgument == 0x7A
    && CharacterPublicDialogNativeLayout.BodyReaderFileOffset == 0x5EE730
    && CharacterPublicDialogNativeLayout.DwordReaderFileOffset == 0x3370A0
    && CharacterPublicDialogNativeLayout.FirstDwordFieldOffset == 0x28
    && CharacterPublicDialogNativeLayout.SecondDwordFieldOffset == 0x2C);
Check("Shared channel-2/3 compact-code routing groups the observed native cases",
    Enumerable.Range(1, 6).All(code =>
        MatrixOtherChannelDecoderNativeLayout.SelectRoute(code)
            == MatrixOtherChannelDecoderRoute.RoutedMessage)
    && MatrixOtherChannelDecoderNativeLayout.SelectRoute(7)
        == MatrixOtherChannelDecoderRoute.SecondaryDispatcher
    && MatrixOtherChannelDecoderNativeLayout.SelectRoute(8)
        == MatrixOtherChannelDecoderRoute.RoutedMultipleMessage
    && MatrixOtherChannelDecoderNativeLayout.SelectRoute(9)
        == MatrixOtherChannelDecoderRoute.RoutedMessage
    && MatrixOtherChannelDecoderNativeLayout.SelectRoute(0)
        == MatrixOtherChannelDecoderRoute.SecondaryDispatcher
    && MatrixOtherChannelDecoderNativeLayout.SelectRoute(10)
        == MatrixOtherChannelDecoderRoute.SecondaryDispatcher
    && MatrixOtherChannelDecoderNativeLayout.RoutedMessageSerializeMethodFileOffset == 0xBCB070
    && MatrixOtherChannelDecoderNativeLayout.RoutedMultipleMessageSerializeMethodFileOffset == 0xBCB120
    && MatrixOtherChannelDecoderNativeLayout.RoutedMessageBlobSliceOffsetFieldOffset == 0x38
    && MatrixOtherChannelDecoderNativeLayout.RoutedMessageBlobSliceLengthFieldOffset == 0x3C
    && MatrixOtherChannelDecoderNativeLayout.RoutedMultipleMessageBlobSliceOffsetFieldOffset == 0x38
    && MatrixOtherChannelDecoderNativeLayout.RoutedMultipleMessageBlobSliceLengthFieldOffset == 0x3A);
bool controlCodecRejected = false;
try { MatrixChannelCodecNativeLayout.GetForChannel(0); }
catch (ArgumentOutOfRangeException) { controlCodecRejected = true; }
Check("Control channel cannot silently select an application codec", controlCodecRejected);
Check("Compact positive integers preserve short and zero-escaped little-endian WORD boundaries",
    CompactPositiveUInt16Codec.Encode(1).SequenceEqual(Convert.FromHexString("01"))
    && CompactPositiveUInt16Codec.Encode(255).SequenceEqual(Convert.FromHexString("FF"))
    && CompactPositiveUInt16Codec.Encode(256).SequenceEqual(Convert.FromHexString("000001"))
    && CompactPositiveUInt16Codec.Encode(65535).SequenceEqual(Convert.FromHexString("00FFFF")));
Check("Compact positive integer encoding round-trips every supported value",
    Enumerable.Range(1, ushort.MaxValue).All(value =>
    {
        byte[] bytes = CompactPositiveUInt16Codec.Encode((uint)value);
        return CompactPositiveUInt16Codec.Decode(bytes)
            == new CompactPositiveUInt16Result(CompactPositiveUInt16Outcome.Accepted, (uint)value, bytes.Length);
    }));
Check("Compact decoder reports truncated and noncanonical escapes, and consumes only the header",
    CompactPositiveUInt16Codec.Decode([]).Outcome == CompactPositiveUInt16Outcome.Truncated
    && CompactPositiveUInt16Codec.Decode(Convert.FromHexString("00")).Outcome == CompactPositiveUInt16Outcome.Truncated
    && CompactPositiveUInt16Codec.Decode(Convert.FromHexString("00FF")).Outcome == CompactPositiveUInt16Outcome.Truncated
    && new[] { "000000", "000100", "00FF00" }.All(hex =>
        CompactPositiveUInt16Codec.Decode(Convert.FromHexString(hex))
            == new CompactPositiveUInt16Result(CompactPositiveUInt16Outcome.NonCanonicalExtendedValue, null, 0))
    && CompactPositiveUInt16Codec.Decode(Convert.FromHexString("23DEAD"))
        == new CompactPositiveUInt16Result(CompactPositiveUInt16Outcome.Accepted, 35, 1));
bool RejectsCompactValue(uint value)
{
    try { CompactPositiveUInt16Codec.Encode(value); return false; }
    catch (ArgumentOutOfRangeException) { return true; }
}
Check("Compact encoder rejects zero and values wider than 16 bits",
    RejectsCompactValue(0) && RejectsCompactValue(65536) && RejectsCompactValue(uint.MaxValue));
var message35Bytes = Convert.FromHexString("2308070605040302010200AABB0100CC");
var message35 = MatrixMessage35Wire.Decode(message35Bytes);
Check("Message 35 reads a raw little-endian QWORD and two WORD-length-prefixed blobs",
    message35.Outcome == MatrixMessage35ReadOutcome.Accepted
    && message35.BytesConsumed == 16
    && message35.Payload is { ValueAt28: 0x0102030405060708 } message35Payload
    && message35Payload.BlobAt30.Span.SequenceEqual(Convert.FromHexString("AABB"))
    && message35Payload.BlobAt40.Span.SequenceEqual(Convert.FromHexString("CC")));
var emptyMessage35 = new byte[MatrixMessage35Wire.MinimumPacketSize];
emptyMessage35[0] = 35;
var emptyMessage35Result = MatrixMessage35Wire.Decode(emptyMessage35);
Check("Message 35 accepts zero opaque value and empty blobs in a freshly created descriptor",
    emptyMessage35Result.Outcome == MatrixMessage35ReadOutcome.Accepted
    && emptyMessage35Result.BytesConsumed == 13
    && emptyMessage35Result.Payload is { ValueAt28: 0 } emptyMessage35Payload
    && emptyMessage35Payload.BlobAt30.IsEmpty && emptyMessage35Payload.BlobAt40.IsEmpty);
Check("Every truncated msg-35 prefix reports failure without a partial success object",
    Enumerable.Range(0, message35Bytes.Length).All(length =>
    {
        var decoded = MatrixMessage35Wire.Decode(message35Bytes.AsSpan(0, length));
        return decoded.Outcome != MatrixMessage35ReadOutcome.Accepted
            && decoded.Payload == null && decoded.BytesConsumed == 0;
    }));
Check("Message 35 distinguishes invalid headers and leaves trailing bytes unconsumed",
    MatrixMessage35Wire.Decode(Convert.FromHexString("002300")).Outcome == MatrixMessage35ReadOutcome.NonCanonicalHeader
    && MatrixMessage35Wire.Decode(Convert.FromHexString("22")).Outcome == MatrixMessage35ReadOutcome.UnexpectedMessageCode
    && MatrixMessage35Wire.Decode(message35Bytes.Concat(new byte[] { 0xDE, 0xAD }).ToArray()).BytesConsumed == 16);
var maximumMessage35Blob = new byte[MatrixMessage35Wire.MinimumPacketSize + ushort.MaxValue];
maximumMessage35Blob[0] = 35;
maximumMessage35Blob[9] = 0xFF;
maximumMessage35Blob[10] = 0xFF;
var maximumMessage35BlobResult = MatrixMessage35Wire.Decode(maximumMessage35Blob);
Check("Message 35 accepts the exact 65535-byte blob limit and rejects a one-byte truncation",
    maximumMessage35BlobResult.Outcome == MatrixMessage35ReadOutcome.Accepted
    && maximumMessage35BlobResult.Payload?.BlobAt30.Length == ushort.MaxValue
    && maximumMessage35BlobResult.BytesConsumed == maximumMessage35Blob.Length
    && MatrixMessage35Wire.Decode(maximumMessage35Blob.AsSpan(0, maximumMessage35Blob.Length - 1)).Outcome
        == MatrixMessage35ReadOutcome.TruncatedPayload);
message35Bytes[11] = 0;
Check("Decoded message-35 blobs own managed copies rather than borrowing caller memory",
    message35.Payload?.BlobAt30.Span[0] == 0xAA);
(byte[] Packet, int MetadataFlagOffset) MakeEnterZonePacket()
{
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    writer.Write((byte)37);
    writer.Write(0x1234567804UL);
    writer.Write(321u);
    writer.Write(0x0102030405060708UL);
    writer.Write((byte)0xA7);
    writer.Write(new byte[] { 0xFF, 0x81, 0 });
    writer.Write((ushort)0x1234);
    writer.Write(0xAABBCCDDu);
    writer.Write((byte)0x42);
    writer.Write(0x1122334455667788UL);
    writer.Write((byte)0xFE);
    writer.Write(0x33445566u);
    writer.Write(new byte[] { 0x41, 0xFF, 0 });
    int metadataOffset = (int)stream.Position;
    writer.Write((byte)0);
    writer.Write(Enumerable.Range(0, 56).Select(i => (byte)i).ToArray());
    writer.Write((byte)0x80);
    writer.Write((byte)5);
    return (stream.ToArray(), metadataOffset);
}
var (enterZoneWireBytes, enterZoneMetadataOffset) = MakeEnterZonePacket();
var enterZoneWireResult = MatrixEnterZoneWire.Decode(enterZoneWireBytes);
Check("EnterZone absent-metadata variant preserves named fields, opaque values and raw text bytes",
    enterZoneWireResult.Outcome == MatrixEnterZoneReadOutcome.Accepted
    && enterZoneWireResult.BytesConsumed == 107
    && enterZoneWireResult.Payload is { } enterZonePayload
    && enterZonePayload.NestedMetadata == null
    && enterZonePayload.Zone.ObservedFields == enteredZone
    && enterZonePayload.Zone.ValueAt38 == 0x0102030405060708
    && enterZonePayload.Zone.WordAt48 == 0x1234 && enterZonePayload.Zone.WordAt4C == 0xAABBCCDD
    && enterZonePayload.Zone.ByteAt50 == 0x42 && enterZonePayload.Zone.ByteAt60 == 0xFE
    && enterZonePayload.Zone.WordAt64 == 0x33445566
    && enterZonePayload.Zone.TextBytesAt44.Span.SequenceEqual(Convert.FromHexString("FF81"))
    && enterZonePayload.Zone.ZoneNameBytes.Span.SequenceEqual(Convert.FromHexString("41FF")));
Check("EnterZone normalizes the suffix bool but retains raw spectator byte and omits memory padding",
    enterZoneWireResult.Payload is { NormalizedFlagAtB8: true, SpectatorByteAtC0: 5 } enterZoneSuffix
    && enterZoneSuffix.ConnectAsSpectator
    && enterZoneSuffix.RawFieldsAt80.Length == 56
    && enterZoneSuffix.RawFieldsAt80.Span.SequenceEqual(Enumerable.Range(0, 56).Select(i => (byte)i).ToArray()));
Check("Every EnterZone truncated prefix fails explicitly without returning a partial packet",
    Enumerable.Range(0, enterZoneWireBytes.Length).All(length =>
    {
        var decoded = MatrixEnterZoneWire.Decode(enterZoneWireBytes.AsSpan(0, length));
        return decoded.Outcome != MatrixEnterZoneReadOutcome.Accepted && decoded.Payload == null;
    }));
var unsupportedEnterZone = enterZoneWireBytes.ToArray();
unsupportedEnterZone[enterZoneMetadataOffset] = 0xFF;
Check("Earlier restricted decoder still explicitly rejects present metadata",
    MatrixEnterZoneWire.DecodeWithoutNestedMetadata(unsupportedEnterZone)
        == new MatrixEnterZoneReadResult(MatrixEnterZoneReadOutcome.UnsupportedNestedMetadata, null, 0));
var minimalEnterZone = new byte[MatrixEnterZoneWire.MinimumPacketSizeWithoutNestedMetadata];
minimalEnterZone[0] = 37;
Check("Minimal EnterZone packet is 103 bytes, not a serialized copy of its padded C8-byte descriptor",
    MatrixEnterZoneWire.Decode(minimalEnterZone).BytesConsumed == 103
    && MatrixEnterZoneWire.Decode(enterZoneWireBytes.Concat(new byte[] { 0xFE }).ToArray())
        .BytesConsumed == 107);
byte[] MakePresentEnterZone(byte[] metadataBytes)
{
    using var stream = new MemoryStream();
    stream.Write(enterZoneWireBytes.AsSpan(0, enterZoneMetadataOffset));
    stream.WriteByte(0xFF);
    stream.Write(metadataBytes);
    stream.Write(enterZoneWireBytes.AsSpan(enterZoneMetadataOffset + 1));
    return stream.ToArray();
}
byte[] MakeEnterZoneMetadata(
    IReadOnlyList<(byte[] Key, ushort Value)> words16,
    IReadOnlyList<(byte[] Key, uint Value)> words32)
{
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    void WriteMap<T>(IReadOnlyList<(byte[] Key, T Value)> entries, Action<T> writeValue)
    {
        int position = 0;
        int count;
        do
        {
            count = Math.Min(entries.Count - position, byte.MaxValue);
            writer.Write((byte)count);
            for (int i = 0; i < count; i++)
            {
                var entry = entries[position++];
                writer.Write(entry.Key);
                writer.Write((byte)0);
                writeValue(entry.Value);
            }
        }
        while (count == byte.MaxValue);
    }
    WriteMap(words16, value => writer.Write(value));
    WriteMap(words32, value => writer.Write(value));
    return stream.ToArray();
}
var emptyEnterZoneMetadata = MatrixEnterZoneWire.Decode(MakePresentEnterZone([0, 0]));
var minimumPresentEnterZone = new byte[MatrixEnterZoneWire.MinimumPacketSizeWithEmptyNestedMetadata];
minimumPresentEnterZone[0] = 37;
minimumPresentEnterZone[44] = 0x80;
Check("Present empty metadata remains distinct from absent metadata and accepts any nonzero presence byte",
    emptyEnterZoneMetadata is { Outcome: MatrixEnterZoneReadOutcome.Accepted, BytesConsumed: 109 }
    && emptyEnterZoneMetadata.Payload?.NestedMetadata is { Word16Entries.Count: 0, Word32Entries.Count: 0 }
    && MatrixEnterZoneWire.Decode(minimumPresentEnterZone).BytesConsumed == 105);
var enterZoneMetadataBytes = MakeEnterZoneMetadata(
    [(new byte[] { 0xFF }, 0xFFFF), ("z"u8.ToArray(), 2), ("a"u8.ToArray(), 1),
     (Array.Empty<byte>(), 0x8000), (new byte[] { 0xFF }, 0xABCD),
     ("a"u8.ToArray(), 0x1234), (new byte[] { 0xFF, 1 }, 0x42)],
    [(new byte[] { 0xFF }, 0x7FC00001), ("a"u8.ToArray(), uint.MaxValue),
     (Array.Empty<byte>(), 0), ("a"u8.ToArray(), 0x80000000), (new byte[] { 0xFE }, 0x12345678)]);
var presentEnterZoneBytes = MakePresentEnterZone(enterZoneMetadataBytes);
var presentEnterZone = MatrixEnterZoneWire.Decode(presentEnterZoneBytes);
var enterZoneMetadata = presentEnterZone.Payload?.NestedMetadata;
Check("Metadata WORD map sorts unsigned byte keys, keeps empty/prefix keys and overwrites duplicates",
    enterZoneMetadata?.Word16Entries.Count == 5
    && enterZoneMetadata.Word16Entries.Select(entry => Convert.ToHexString(entry.KeyBytes.Span))
        .SequenceEqual(new[] { "", "61", "7A", "FF", "FF01" })
    && enterZoneMetadata.Word16Entries.Select(entry => entry.ValueBits)
        .SequenceEqual(new ushort[] { 0x8000, 0x1234, 2, 0xABCD, 0x42 }));
Check("Metadata DWORD map stays separate and preserves opaque bit patterns without float interpretation",
    enterZoneMetadata?.Word32Entries.Count == 4
    && enterZoneMetadata.Word32Entries.Select(entry => Convert.ToHexString(entry.KeyBytes.Span))
        .SequenceEqual(new[] { "", "61", "FE", "FF" })
    && enterZoneMetadata.Word32Entries.Select(entry => entry.ValueBits)
        .SequenceEqual(new uint[] { 0, 0x80000000, 0x12345678, 0x7FC00001 }));
Check("Present metadata consumes both maps before the unchanged 58-byte suffix and permits trailing bytes",
    presentEnterZone is { Outcome: MatrixEnterZoneReadOutcome.Accepted } && presentEnterZone.Payload is { } presentPayload
    && presentEnterZone.BytesConsumed == presentEnterZoneBytes.Length
    && presentPayload.NormalizedFlagAtB8 && presentPayload.SpectatorByteAtC0 == 5
    && presentPayload.RawFieldsAt80.Span.SequenceEqual(enterZoneWireResult.Payload!.RawFieldsAt80.Span)
    && MatrixEnterZoneWire.Decode(presentEnterZoneBytes.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed
        == presentEnterZoneBytes.Length);
Check("Every prefix of present metadata fails explicitly without leaking partially decoded maps",
    Enumerable.Range(0, presentEnterZoneBytes.Length).All(length =>
    {
        var result = MatrixEnterZoneWire.Decode(presentEnterZoneBytes.AsSpan(0, length));
        return result.Outcome != MatrixEnterZoneReadOutcome.Accepted && result.Payload == null;
    }));
Check("Both metadata maps honor chunk boundaries at 0, 1, 254, 255, 256, 510 and 511 entries",
    new[] { 0, 1, 254, 255, 256, 510, 511 }.All(count =>
    {
        byte[] Key(int i) => System.Text.Encoding.ASCII.GetBytes(i.ToString("X4"));
        var words16 = Enumerable.Range(0, count).Select(i => (Key(i), (ushort)i)).ToArray();
        var words32 = Enumerable.Range(0, count).Select(i => (Key(i), 0x80000000u | (uint)i)).ToArray();
        var encoded = MakeEnterZoneMetadata(words16, words32);
        int word16Size = count * 7 + count / 255 + 1;
        var packet = MakePresentEnterZone(encoded);
        var decoded = MatrixEnterZoneWire.Decode(packet);
        return encoded.Length == count * 16 + 2 * (count / 255 + 1)
            && encoded[word16Size] == Math.Min(count, 255)
            && (count % 255 != 0 || encoded[word16Size - 1] == 0)
            && decoded.Outcome == MatrixEnterZoneReadOutcome.Accepted
            && decoded.BytesConsumed == packet.Length
            && decoded.Payload?.NestedMetadata is { } metadata
            && metadata.Word16Entries.Count == count && metadata.Word32Entries.Count == count
            && metadata.Word16Entries.Select(entry => entry.ValueBits).SequenceEqual(words16.Select(entry => entry.Item2))
            && metadata.Word32Entries.Select(entry => entry.ValueBits).SequenceEqual(words32.Select(entry => entry.Item2));
    }));
presentEnterZoneBytes[enterZoneMetadataOffset + 2] = 1;
Check("Metadata keys own managed copies rather than borrowing the packet backing array",
    enterZoneMetadata?.Word16Entries[3].KeyBytes.Span.SequenceEqual(new byte[] { 0xFF }) == true);
var message24 = new MatrixMessage24(0x1122334455667788);
Check("Message 24 encodes instance context before its compact code, with no extra body",
    message24.Encode().SequenceEqual(Convert.FromHexString("887766554433221118"))
    && message24.Encode().Length == MatrixMessage24.EncodedSize
    && MatrixMessage24.TransportChannel == 2
    && new MatrixMessage24(0).Encode().SequenceEqual(Convert.FromHexString("000000000000000018")));
Check("Message-24 gate tests signed state greater than 4 and a nonnull transport before stamping",
    new[] { int.MinValue, -1, 0, 3, 4, 5, 6, 9, int.MaxValue }.All(state =>
    {
        var selected = MatrixMessage24NativeLayout.SelectDispatch(state, true, 0x1234567804,
            uint.MaxValue, 0xFF);
        return state <= 4
            ? selected is { Outcome: MatrixMessage24DispatchOutcome.ConnectionStateNotPermitted,
                TimestampAtE0: null, SelectsDiagnosticLog: false, Message: null }
            : selected is { Outcome: MatrixMessage24DispatchOutcome.ReadyForSubmission,
                TimestampAtE0: 4294967295000, SelectsDiagnosticLog: true }
                && selected.Message?.InstanceContext == 0x1234567804;
    })
    && MatrixMessage24NativeLayout.SelectDispatch(6, false, 1, 123, 2)
        == new MatrixMessage24DispatchDecision(MatrixMessage24DispatchOutcome.MissingTransport, null, false, null)
    && MatrixMessage24NativeLayout.SelectDispatch(6, true, 1, 123, 1)
        is { TimestampAtE0: 123000, SelectsDiagnosticLog: false });
var clockFields = new byte[56];
ulong[] clockWords = [1000, 900, 0x3FF0000000000000, 2000, 100];
for (int i = 0; i < clockWords.Length; i++)
    System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(clockFields.AsSpan(16 + i * 8), clockWords[i]);
var clockParameters = MatrixClockParameters.FromEnterZoneRawFields(clockFields, false);
Check("EnterZone suffix fields +90..B8 describe the clock's seed, reference, double scale, limit and offset",
    clockParameters == new MatrixClockParameters(1000, 900, 0x3FF0000000000000, 2000, 100, false)
    && clockParameters.Scale == 1
    && MatrixClockNativeLayout.ConnectionClockObjectOffset + MatrixClockNativeLayout.HasUpperLimitOffset
        == MatrixEnterZoneDownstreamNativeLayout.ConnectionClockHasUpperLimitOffset);
var initialClock = MatrixClockNativeLayout.Initialize(clockParameters);
var advancedClock = MatrixClockNativeLayout.AdvanceWithoutRateConversion(initialClock, 1200);
Check("Clock initialization adds the offset and unscaled advancement never moves its cursor backward",
    initialClock.CurrentTick == 1100 && initialClock.LastDeltaLowWord == 0
    && advancedClock.CurrentTick == 1300 && advancedClock.LastDeltaLowWord == 200
    && MatrixClockNativeLayout.AdvanceWithoutRateConversion(advancedClock, 1100)
        is { CurrentTick: 1300, LastDeltaLowWord: 0 });
var limitedClock = MatrixClockNativeLayout.Initialize(clockParameters with
    { HasUpperLimit = true, UpperLimitTickAtA8 = 1050 });
Check("Clock limits apply at initialization and projection but cannot move an existing cursor backward",
    limitedClock.CurrentTick == 1050
    && MatrixClockNativeLayout.AdvanceWithoutRateConversion(limitedClock, 1200).CurrentTick == 1050
    && MatrixClockNativeLayout.AdvanceWithoutRateConversion(limitedClock with { CurrentTick = 1300 }, 1200)
        is { CurrentTick: 1300, LastDeltaLowWord: 0 });
var wrappingClockParameters = clockParameters with { InitialTickAt90 = ulong.MaxValue - 3, OffsetBitsAtB0 = 0 };
var wrappingClock = MatrixClockNativeLayout.AdvanceWithoutRateConversion(
    MatrixClockNativeLayout.Initialize(wrappingClockParameters), 5);
Check("Clock comparisons use signed wrapping differences and delta storage keeps only its low DWORD",
    wrappingClock is { CurrentTick: 5, LastDeltaLowWord: 9 }
    && MatrixClockNativeLayout.Initialize(wrappingClockParameters with { OffsetBitsAtB0 = 10 }).CurrentTick == 6
    && MatrixClockNativeLayout.AdvanceWithoutRateConversion(
        MatrixClockNativeLayout.Initialize(clockParameters with { InitialTickAt90 = 0, OffsetBitsAtB0 = 0 }),
        (1UL << 32) + 7).LastDeltaLowWord == 7);
bool RejectsRestrictedClockScale(double scale)
{
    try
    {
        var parameters = clockParameters with { ScaleBitsAtA0 = unchecked((ulong)BitConverter.DoubleToInt64Bits(scale)) };
        MatrixClockNativeLayout.AdvanceWithoutRateConversion(MatrixClockNativeLayout.Initialize(parameters), 1200);
        return false;
    }
    catch (NotSupportedException) { return true; }
}
Check("The near-unit-only entry point honors the native tolerance and rejects rate conversion",
    MatrixClockNativeLayout.AdvanceWithoutRateConversion(MatrixClockNativeLayout.Initialize(
        clockParameters with { ScaleBitsAtA0 = unchecked((ulong)BitConverter.DoubleToInt64Bits(Math.BitDecrement(1.001))) }),
        1200).CurrentTick == 1300
    && RejectsRestrictedClockScale(Math.BitIncrement(1.001))
    && RejectsRestrictedClockScale(2) && RejectsRestrictedClockScale(double.NaN)
    && RejectsRestrictedClockScale(double.PositiveInfinity));
MatrixClockSnapshot AdvanceAtScale(double scale, ulong sourceTick)
{
    var parameters = clockParameters with { ScaleBitsAtA0 = unchecked((ulong)BitConverter.DoubleToInt64Bits(scale)) };
    return MatrixClockNativeLayout.Advance(MatrixClockNativeLayout.Initialize(parameters), sourceTick);
}
Check("Finite clock rates scale unsigned elapsed ticks, truncate fractions and preserve monotonic output",
    AdvanceAtScale(2, 1200) is { CurrentTick: 1600, LastDeltaLowWord: 500 }
    && AdvanceAtScale(0.5, 1201) is { CurrentTick: 1150, LastDeltaLowWord: 50 }
    && AdvanceAtScale(0, 1200) is { CurrentTick: 1100, LastDeltaLowWord: 0 }
    && AdvanceAtScale(-0.5, 1200) is { CurrentTick: 1100, LastDeltaLowWord: 0 }
    && AdvanceAtScale(1.0005, 10000900).CurrentTick == 10001000);
var scaledLimitedParameters = clockParameters with { ScaleBitsAtA0 = 0x4000000000000000,
    HasUpperLimit = true, UpperLimitTickAtA8 = 1500 };
Check("Rate-scaled projection still applies the upper limit before the monotonic cursor test",
    MatrixClockNativeLayout.Advance(MatrixClockNativeLayout.Initialize(scaledLimitedParameters), 1200)
        is { CurrentTick: 1500, LastDeltaLowWord: 400 });
Check("Large unsigned elapsed values round to double before rate multiplication",
    AdvanceAtScale(0.5, ulong.MaxValue).CurrentTick == (1UL << 63) + 1000
    && AdvanceAtScale(-0.5, ulong.MaxValue).CurrentTick == (1UL << 63) + 1000);
bool RejectsUnsupportedClockConversion(double scale, ulong sourceTick)
{
    try { AdvanceAtScale(scale, sourceTick); return false; }
    catch (NotSupportedException) { return true; }
}
Check("Nonfinite and out-of-range clock conversions remain explicit unsupported boundaries",
    RejectsUnsupportedClockConversion(double.NaN, 1200)
    && RejectsUnsupportedClockConversion(double.PositiveInfinity, 1200)
    && RejectsUnsupportedClockConversion(double.MaxValue, 1200)
    && RejectsUnsupportedClockConversion(2, ulong.MaxValue)
    && RejectsUnsupportedClockConversion(-2, ulong.MaxValue));
var downstreamPayload = enterZoneWireResult.Payload! with { RawFieldsAt80 = clockFields, NormalizedFlagAtB8 = true };
var downstreamPlan = MatrixEnterZoneDownstreamNativeLayout.CapturePlan(downstreamPayload, true, true, 0, 0x80);
Check("Paused runtime message 110 follows the clock limit flag, separately from message-24 request selection",
    downstreamPlan.EmitRuntime110Paused && downstreamPlan.AttemptMessage24
    && downstreamPlan.SetDownstreamByteAt18 && downstreamPlan.GlobalSubsystemInputByte == 0x42
    && MatrixEnterZoneDownstreamNativeLayout.RuntimeMessageId == 110
    && MatrixEnterZoneDownstreamNativeLayout.RuntimePropertyName == "paused"
    && MatrixEnterZoneDownstreamNativeLayout.CapturePlan(
        downstreamPayload with { NormalizedFlagAtB8 = false }, true, true, 0, 0)
        is { EmitRuntime110Paused: false, AttemptMessage24: false });
bool RejectsDownstream(bool hasGlobal, bool hasDownstream, uint requiredZeroWord)
{
    try
    {
        MatrixEnterZoneDownstreamNativeLayout.CapturePlan(downstreamPayload, hasGlobal, hasDownstream, requiredZeroWord, 0);
        return false;
    }
    catch (ArgumentException) { return true; }
}
Check("Native downstream prerequisites are not silently replaced with success-shaped defaults",
    RejectsDownstream(false, true, 0) && RejectsDownstream(true, false, 0) && RejectsDownstream(true, true, 1));
Check("Final EnterZone lookup follows the existing mode without requesting a new app mode",
    MatrixEnterZoneDownstreamNativeLayout.FindModeObjectPointerOffset(1) == 0x14
    && MatrixEnterZoneDownstreamNativeLayout.FindModeObjectPointerOffset(2) == 0x28E8
    && MatrixEnterZoneDownstreamNativeLayout.FindModeObjectPointerOffset(3) == 0x68
    && MatrixEnterZoneDownstreamNativeLayout.FindModeObjectPointerOffset(0) == null
    && MatrixEnterZoneDownstreamNativeLayout.CaptureModeSpecificWordUpdate(0, 0, 1) == (1u, 1u)
    && MatrixEnterZoneDownstreamNativeLayout.CaptureModeSpecificWordUpdate(0, 0, 0) == (1u, 0u)
    && MatrixEnterZoneDownstreamNativeLayout.CaptureModeSpecificWordUpdate(uint.MaxValue, 7, 0x80) == (uint.MaxValue, 7u));
var synchronizedClock = new GameSocketSynchronizedClockSnapshot(0x3F800000, 0, 0, 0);
Check("Socket synchronized clock rounds through double then single before applying its float rate",
    GameSocketSynchronizedClock.SampleFinite(synchronizedClock, 1_000_001).LastSampleAt60 == 1_000_001_024
    && GameSocketSynchronizedClock.SampleFinite(synchronizedClock with { RateBitsAt14 = 0x3DCCCCCD },
        1_000_001).LastSampleAt60 == 100_000_104);
Check("Synchronized clock subtracts its local reference and adds a separate synchronized base",
    GameSocketSynchronizedClock.SampleFinite(synchronizedClock with
        { LocalReferenceTickAt18 = 1_000_000, SynchronizedBaseTickAt20 = 500 }, 1001)
        .LastSampleAt60 == 1500);
Check("Synchronized clock never moves backward and compares wrapping differences by their sign bit",
    GameSocketSynchronizedClock.SampleFinite(synchronizedClock with { LastSampleAt60 = 2000 }, 1)
        .LastSampleAt60 == 2000
    && GameSocketSynchronizedClock.SampleFinite(synchronizedClock with
        { SynchronizedBaseTickAt20 = ulong.MaxValue - 999, LastSampleAt60 = ulong.MaxValue - 500 }, 1)
        .LastSampleAt60 == 0);
bool RejectsUnsupportedSocketClock(uint rateBits)
{
    try
    {
        GameSocketSynchronizedClock.SampleFinite(synchronizedClock with { RateBitsAt14 = rateBits }, 1_000_001);
        return false;
    }
    catch (NotSupportedException) { return true; }
}
Check("Socket clock invalid float results remain explicit rather than producing plausible timestamps",
    RejectsUnsupportedSocketClock(0x7FC00001) && RejectsUnsupportedSocketClock(0x7F800000)
    && RejectsUnsupportedSocketClock(0x7F7FFFFF));
byte[] message38Bytes = Convert.FromHexString("2688776655443322110100C07FFFFFFF80");
var message38Decoded = MatrixScalarUpdateWire.Decode(message38Bytes);
Check("Message 38 decodes the packed u64/u32/u32 block without interpreting its bit patterns",
    message38Decoded is { Outcome: MatrixScalarUpdateReadOutcome.Accepted, BytesConsumed: 17 }
    && message38Decoded.Payload == new MatrixMessage38Payload(0x1122334455667788, 0x7FC00001, 0x80FFFFFF)
    && MatrixScalarUpdateWire.Message38DefaultWordBitsAt30 == 0x3F800000
    && MatrixServerMessageCatalog.Get(38).HandlerFileOffset == MatrixScalarUpdateNativeLayout.Message38HandlerFileOffset
    && MatrixScalarUpdateNativeLayout.Message38DestinationBlockOffset == MatrixEnterZoneNativeLayout.ConnectionOpaqueBlockOffset);
Check("Message 39 retains its raw byte and selects the same subsystem input before storing it",
    MatrixScalarUpdateWire.Decode(new byte[] { 39, 0xFF })
        is { Outcome: MatrixScalarUpdateReadOutcome.Accepted, BytesConsumed: 2, Payload: MatrixMessage39Payload { ByteAt28: 0xFF } }
    && MatrixScalarUpdateNativeLayout.CaptureMessage39Update(0xFF, true) == new MatrixMessage39ObservedUpdate(0xFF, 0xFF)
    && MatrixServerMessageCatalog.Get(39).HandlerFileOffset == MatrixScalarUpdateNativeLayout.Message39HandlerFileOffset);
Check("Both scalar-update decoders reject every incomplete prefix without returning partial data",
    new[] { message38Bytes, new byte[] { 39, 0xFF } }.All(packet =>
        Enumerable.Range(0, packet.Length).All(length =>
        {
            var decoded = MatrixScalarUpdateWire.Decode(packet.AsSpan(0, length));
            return decoded.Outcome != MatrixScalarUpdateReadOutcome.Accepted && decoded.Payload == null;
        })));
Check("Scalar-update framing rejects noncanonical and unrelated codes but exposes trailing bytes",
    MatrixScalarUpdateWire.Decode(new byte[] { 0, 38, 0 }).Outcome == MatrixScalarUpdateReadOutcome.NonCanonicalHeader
    && MatrixScalarUpdateWire.Decode(new byte[] { 37, 0 }).Outcome == MatrixScalarUpdateReadOutcome.UnexpectedMessageCode
    && MatrixScalarUpdateWire.Decode(message38Bytes.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == 17
    && MatrixScalarUpdateWire.Decode(new byte[] { 39, 0, 0xAA }).BytesConsumed == 2);
bool message39MissingSubsystemRejected = false;
try { MatrixScalarUpdateNativeLayout.CaptureMessage39Update(0, false); }
catch (ArgumentException) { message39MissingSubsystemRejected = true; }
Check("Message-39 subsystem absence is an explicit native prerequisite failure", message39MissingSubsystemRejected);
var queueStatusBytes = Convert.FromHexString("3EFFFFFFFF00000080");
var queueStatus = MatrixScalarUpdateWire.Decode(queueStatusBytes);
Check("Message 62 is queue status, preserving both DWORDs and selecting runtime message 295",
    queueStatus is { Outcome: MatrixScalarUpdateReadOutcome.Accepted, BytesConsumed: 9,
        Payload: MatrixQueueStatusPayload { QueuePositionBits: uint.MaxValue, QueueLengthBits: 0x80000000 } }
    && MatrixQueueStatusNativeLayout.CaptureNotification(new(5, 25))
        == new MatrixQueueStatusRuntimeNotification(295, 5, 25)
    && MatrixQueueStatusNativeLayout.PositionPropertyName == "queue_position"
    && MatrixQueueStatusNativeLayout.LengthPropertyName == "queue_length"
    && MatrixServerMessageCatalog.Get(62).HandlerFileOffset == MatrixQueueStatusNativeLayout.HandlerFileOffset);
Check("Queue-status prefixes fail explicitly and trailing data is not counted as its body",
    Enumerable.Range(0, queueStatusBytes.Length).All(length =>
        MatrixScalarUpdateWire.Decode(queueStatusBytes.AsSpan(0, length)).Outcome != MatrixScalarUpdateReadOutcome.Accepted)
    && MatrixScalarUpdateWire.Decode(queueStatusBytes.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == 9);
Check("ExitZone msg-40 is header-only, distinct from queue status and its outgoing acknowledgement",
    MatrixExitZoneNativeLayout.Decode(new byte[] { 40 })
        == new MatrixExitZoneReadResult(MatrixExitZoneReadOutcome.Accepted, 1)
    && MatrixExitZoneNativeLayout.Decode(new byte[] { 40, 0xAA }).BytesConsumed == 1
    && MatrixExitZoneNativeLayout.Decode(Array.Empty<byte>()).Outcome == MatrixExitZoneReadOutcome.TruncatedHeader
    && MatrixExitZoneNativeLayout.Decode(new byte[] { 0, 40, 0 }).Outcome == MatrixExitZoneReadOutcome.NonCanonicalHeader
    && MatrixExitZoneNativeLayout.Decode(new byte[] { 62 }).Outcome == MatrixExitZoneReadOutcome.UnexpectedMessageCode
    && MatrixExitZoneAck.Encode().SequenceEqual(new byte[] { 19 })
    && MatrixExitZoneAck.TransportChannel == 1);
Check("Known ExitZone cleanup requires state 6 or 7 and passes through connection state 5",
    MatrixExitZoneNativeLayout.CaptureCleanupState(6, true, true) == 5
    && MatrixExitZoneNativeLayout.CaptureCleanupState(7, true, true) == 5);
bool RejectsExitZoneCleanup(int state, bool hasDownstream, bool hasMode)
{
    try { MatrixExitZoneNativeLayout.CaptureCleanupState(state, hasDownstream, hasMode); return false; }
    catch (ArgumentException) { return true; }
}
Check("ExitZone state and downstream/current-mode prerequisites fail explicitly",
    RejectsExitZoneCleanup(4, true, true) && RejectsExitZoneCleanup(6, false, true)
    && RejectsExitZoneCleanup(6, true, false));
Check("Disconnect-on-exit branch selects socket release, optional hook and connection state 1",
    MatrixExitZoneNativeLayout.SelectAfterCleanup(5, 0x80, true, true, 0xFF)
        == new MatrixExitZoneBranchSelection(1, true, true, true, false, false)
    && MatrixExitZoneNativeLayout.SelectAfterCleanup(5, 1, false, false, 0xFF)
        == new MatrixExitZoneBranchSelection(1, false, false, false, false, false));
Check("Stay-connected branch selects ExitZoneAck only with transport and signed post-cleanup state greater than 3",
    MatrixExitZoneNativeLayout.SelectAfterCleanup(5, 0, true, true, 1)
        == new MatrixExitZoneBranchSelection(4, false, false, false, true, true)
    && MatrixExitZoneNativeLayout.SelectAfterCleanup(5, 0, false, true, 1)
        == new MatrixExitZoneBranchSelection(4, false, false, false, false, false)
    && MatrixExitZoneNativeLayout.SelectAfterCleanup(int.MinValue, 0, true, false, 1)
        == new MatrixExitZoneBranchSelection(4, false, false, false, false, false)
    && MatrixExitZoneNativeLayout.SelectAfterCleanup(5, 0, true, false, 2)
        == new MatrixExitZoneBranchSelection(4, false, false, false, true, false));
Check("Socket destroy is a state selection, not unconditional object deallocation",
    new[] { NativeGameSocketState.Initializing, NativeGameSocketState.Transferring, NativeGameSocketState.WaitingContact }
        .All(state => GameSocketDestroyNativeLayout.SelectStateBranch(state)
            == new GameSocketDestroyBranch(NativeGameSocketState.Destroyed, true, false, false, false))
    && new[] { NativeGameSocketState.Connecting, NativeGameSocketState.Connected }
        .All(state => GameSocketDestroyNativeLayout.SelectStateBranch(state)
            == new GameSocketDestroyBranch(NativeGameSocketState.Destroyed, false, false, false, false))
    && GameSocketDestroyNativeLayout.SelectStateBranch(NativeGameSocketState.NotOpened)
        == new GameSocketDestroyBranch(NativeGameSocketState.Closed, false, true, true, true)
    && new[] { NativeGameSocketState.DisconnectedNotify, NativeGameSocketState.Disconnected }
        .All(state => GameSocketDestroyNativeLayout.SelectStateBranch(state)
            == new GameSocketDestroyBranch(NativeGameSocketState.Closed, false, false, true, true)));
bool RejectsSocketDestroyState(NativeGameSocketState state)
{
    try { GameSocketDestroyNativeLayout.SelectStateBranch(state); return false; }
    catch (ArgumentOutOfRangeException) { return true; }
}
Check("Socket destroy rejects terminal/unknown states and captures metadata only on the first atomic request",
    RejectsSocketDestroyState(NativeGameSocketState.Destroyed)
    && RejectsSocketDestroyState(NativeGameSocketState.Closed)
    && RejectsSocketDestroyState((NativeGameSocketState)10)
    && RejectsSocketDestroyState((NativeGameSocketState)(-1))
    && GameSocketDestroyNativeLayout.SelectFirstRequestCapture(0)
    && !GameSocketDestroyNativeLayout.SelectFirstRequestCapture(6)
    && !GameSocketDestroyNativeLayout.SelectFirstRequestCapture(uint.MaxValue));
Check("World cleanup observer gate compares both raw words; connection state and socket state remain separate",
    TfWorldZoneCleanupNativeLayout.SelectObserverCall(0x80000000, uint.MaxValue, 0x80000000, uint.MaxValue)
    && !TfWorldZoneCleanupNativeLayout.SelectObserverCall(1, 2, 1, 3)
    && !TfWorldZoneCleanupNativeLayout.SelectObserverCall(1, 2, 3, 2)
    && TfWorldZoneCleanupNativeLayout.EnteredZoneCleanupCallerFileOffset == 0x494B00
    && TfWorldZoneCleanupNativeLayout.EnteredZoneCleanupCallFileOffset == 0x494BDD
    && TfWorldZoneCleanupNativeLayout.CleanupGuardedMetadataLookupMethodFileOffset == 0x1010AC0
    && TfWorldZoneCleanupNativeLayout.CleanupGuardedRecordDispatchMethodFileOffset == 0x369A30
    && TfWorldZoneCleanupNativeLayout.CleanupGuardedRecordDispatchTargetFileOffset == 0x101D710
    && TfWorldZoneCleanupNativeLayout.CleanupGuardedMetadataLookupIndex == 0x6A
    && TfWorldZoneCleanupNativeLayout.ConnectionCleanupCallFileOffset == 0x64E27D
    && TfWorldZoneCleanupNativeLayout.ConnectionStateFieldOffset == 0x0C
    && TfWorldZoneCleanupNativeLayout.RequiredConnectionState == 5
    && TfWorldZoneCleanupNativeLayout.ConnectionDownstreamPointerOffset == 0x910
    && TfWorldZoneCleanupNativeLayout.DownstreamActionPointerOffset == 0x08
    && TfWorldZoneCleanupNativeLayout.ConnectionDownstreamHelperFileOffset == 0x7FEB50
    && TfWorldZoneCleanupNativeLayout.DownstreamHelperCallFileOffset == 0x494AB9
    && TfWorldZoneCleanupNativeLayout.CleanupFirstListHeadOffset == 0x20
    && TfWorldZoneCleanupNativeLayout.CleanupFirstNodeForwardOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupFirstNodeValuePointerOffset == 0x10
    && TfWorldZoneCleanupNativeLayout.CleanupFirstListResetWordOffset == 0x24
    && TfWorldZoneCleanupNativeLayout.CleanupFirstNodeHelperFileOffset == 0x977E20
    && TfWorldZoneCleanupNativeLayout.CleanupFirstValueHelperFileOffset == 0x975020
    && TfWorldZoneCleanupNativeLayout.CleanupSecondListHeadOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNodeForwardOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNodeDataOffset == 0x10
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNodeNestedRangeOffset == 0x20
    && TfWorldZoneCleanupNativeLayout.CleanupSecondListResetWordOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNodeHelperFileOffset == 0x976D30
    && TfWorldZoneCleanupNativeLayout.CleanupSecondListHelperFileOffset == 0x9795B0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeCleanupHelperFileOffset == 0x3F7680
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeWholeTreeClearHelperFileOffset == 0x3F7300
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeItemHelperFileOffset == 0x3F7430
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeRecursiveNodeHelperFileOffset == 0x3F72C0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNodeNestedRangeCountOffset == 0x24
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeTreeHeaderFirstLinkOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeTreeHeaderSecondLinkOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeTreeHeaderThirdLinkOffset == 8
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeNodeForwardLinkOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeNodeRecursiveChildOffset == 8
    && TfWorldZoneCleanupNativeLayout.CleanupSecondNestedRangeNodeSentinelByteOffset == 0x0D
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueMethodFileOffset == 0x975020
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerOneFileOffset == 0x976200
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerOneCallFileOffset == 0x976254
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoFileOffset == 0x977F20
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoCallFileOffset == 0x977F78
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoGuardWordOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoWaitHelperFileOffset == 0x977630
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoWaitCallFileOffset == 0x977F6B
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoWaitRemovalCallFileOffset == 0x977726
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValueCallerTwoGuardDecrementFileOffset == 0x978673
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValuePreCleanVirtualCallFileOffset == 0x977ECC
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValuePreCleanEventPushFileOffset == 0x977EC5
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValuePreCleanVirtualSlotOffset == 0xAC
    && TfWorldZoneCleanupNativeLayout.CleanupAdditionalValuePreCleanEventId == 0x169
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueFirstTreeHeadOffset == 0x08
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSecondTreeHeadOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueFirstTreeEntryPointerOffset == 0x14
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSecondTreeEntryPointerOffset == 0x18
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSharedTextPointerOffset == 0x18
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSpanRangeOffset == 0x10
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSpanElementStride == 24
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueFirstEntryHelperFileOffset == 0x9751F0
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSecondEntryHelperFileOffset == 0x88AD60
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueFirstTreeClearFileOffset == 0x97AE50
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSecondTreeClearFileOffset == 0x97AA70
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueFirstTreeRemoveNodeFileOffset == 0x97BCC0
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSecondTreeRemoveNodeFileOffset == 0x978440
    && TfWorldZoneCleanupNativeLayout.CleanupNestedValueSpanRangeReleaseFileOffset == 0x979A20
    && TfWorldZoneCleanupNativeLayout.CleanupNestedEntrySharedTextPointerAOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupNestedEntrySharedTextPointerBOffset == 8
    && TfWorldZoneCleanupNativeLayout.CleanupNestedEntrySpanRangeOffset == 0x10
    && TfWorldZoneCleanupNativeLayout.CleanupNestedTreeNodeLeftPointerOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupNestedTreeNodeParentPointerOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupNestedTreeNodeRightPointerOffset == 8
    && TfWorldZoneCleanupNativeLayout.CleanupNestedTreeNodeColorByteOffset == 0x0C
    && TfWorldZoneCleanupNativeLayout.CleanupNestedTreeNodeSentinelByteOffset == 0x0D
    && TfWorldZoneCleanupNativeLayout.CleanupFirstRangeStartOffset == 0x28
    && TfWorldZoneCleanupNativeLayout.CleanupFirstRangeEndOffset == 0x2C
    && TfWorldZoneCleanupNativeLayout.CleanupFirstRangeHelperFileOffset == 0x97CBB0
    && TfWorldZoneCleanupNativeLayout.CleanupSecondRangeStartOffset == 8
    && TfWorldZoneCleanupNativeLayout.CleanupSecondRangeEndOffset == 0x0C
    && TfWorldZoneCleanupNativeLayout.CleanupSecondRangeHelperFileOffset == 0x97C690
    && TfWorldZoneCleanupNativeLayout.CleanupRangeBeginPointerOffset == 0
    && TfWorldZoneCleanupNativeLayout.CleanupRangeEndPointerOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupRangeCapacityEndPointerOffset == 8
    && TfWorldZoneCleanupNativeLayout.CleanupRangeRequestedCount == 0x10
    && TfWorldZoneCleanupNativeLayout.CleanupRangeElementStride == 4
    && TfWorldZoneCleanupNativeLayout.CleanupRangeHelpersMayReplaceBackingStorage
    && TfWorldZoneCleanupNativeLayout.CleanupFirstRangePostValueOffset == 0x34
    && TfWorldZoneCleanupNativeLayout.CleanupFirstRangePostCountOffset == 0x38
    && TfWorldZoneCleanupNativeLayout.CleanupSecondRangePostValueOffset == 0x14
    && TfWorldZoneCleanupNativeLayout.CleanupSecondRangePostCountOffset == 0x18
    && TfWorldZoneCleanupNativeLayout.CleanupRangePostValue == 7
    && TfWorldZoneCleanupNativeLayout.CleanupRangePostCount == 8
    && TfWorldZoneCleanupNativeLayout.LocalClientResetMethodFileOffset == 0x4A2FB0
    && TfWorldZoneCleanupNativeLayout.LocalClientMember4ResetMethodFileOffset == 0x431C90
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupManagerCallFileOffset == 0x8610B5
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupRecordPointerOffset == 0x328
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerOffset == 0x32C
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerInitializeFileOffset == 0x8602CD
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerProviderFileOffset == 0x860810
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerFactoryCallFileOffset == 0x860F6F
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerStoreFileOffset == 0x860F76
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerCallFileOffset == 0x8610C8
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerVtableSlotOffset == 0
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupVirtualPointerCallArgument == 1
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectResourceManagerCleanupMethodFileOffset == 0xD74A20
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupRecordLeadingDword == 0x3C8
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupRecordTokenOffset == 0x398
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectCleanupRecordManagerStateByteOffset == 0x39C
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerStateByteOffset == 4
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerLockOffset == 0x38
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerActivityCountOffset == 0x68
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerTokenArrayBeginPointerOffset == 0xB8
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerTokenArrayEndPointerOffset == 0xBC
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerGlobalPointerVirtualAddress == 0x1DFFD98
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerGlobalHolderOffset == 0xB98
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerGetterVirtualOffset == 0x24
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerResultMemberOffset == 0x0C
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerTokenHookFileOffset == 0xD935F0
    && TfWorldZoneCleanupNativeLayout.CleanupResourceManagerPostRemovalHelperFileOffset == 0xD78900
    && TfWorldZoneCleanupNativeLayout.ReferenceReleaseHelperFileOffset == 0x424790
    && TfWorldZoneCleanupNativeLayout.ReferenceReleaseHelperCallFileOffset == 0x8610D4
    && TfWorldZoneCleanupNativeLayout.ReleasedObjectReferencePointerOffset == 0x320
    && TfWorldZoneCleanupNativeLayout.ReferenceAllocationTagOffsetFromPayload == -8
    && TfWorldZoneCleanupNativeLayout.ReferenceAllocationCountOffsetFromPayload == -4
    && TfWorldZoneCleanupNativeLayout.SharedTextAllocatorNodeTag == 0x17
    && TfWorldZoneCleanupNativeLayout.SharedTextAllocatorSingletonMethodFileOffset == 0x12B9F70
    && TfWorldZoneCleanupNativeLayout.SharedTextAllocatorNodeReleaseMethodFileOffset == 0x12CDEC0
    && TfWorldZoneCleanupNativeLayout.SharedTextAllocatorNodePushMethodFileOffset == 0x12CDA60
    && TfWorldZoneCleanupNativeLayout.SharedTextAllocatorNodeReleaseCallFileOffset == 0x4247C2
    && TfWorldZoneCleanupNativeLayout.SharedTextAllocatorNodeReferenceCountOffset == 0x10
    && TfWorldZoneCleanupNativeLayout.RuntimeMessageId == 97);
Check("Destroyed-state tick skips an in-flight close and otherwise checks keepalive/retries before assembly",
    GameSocketCloseCompletionNativeLayout.SelectDestroyedTick(true, false, false)
        == DestroyedSocketTickSelection.CloseAlreadyRequested
    && GameSocketCloseCompletionNativeLayout.SelectDestroyedTick(false, false, true)
        == DestroyedSocketTickSelection.RequestPumpClose
    && GameSocketCloseCompletionNativeLayout.SelectDestroyedTick(false, true, false)
        == DestroyedSocketTickSelection.RequestPumpClose
    && GameSocketCloseCompletionNativeLayout.SelectDestroyedTick(false, true, true)
        == DestroyedSocketTickSelection.AssembleOutgoingThenCheckState
    && GameSocketCloseCompletionNativeLayout.SelectAssemblerTailClose(NativeGameSocketState.Destroyed)
    && !GameSocketCloseCompletionNativeLayout.SelectAssemblerTailClose(NativeGameSocketState.Connected));
bool RejectsCloseCompletion(bool handleInvalid, bool recorded)
{
    try { GameSocketCloseCompletionNativeLayout.CaptureCompletionEvent(handleInvalid, recorded); return false; }
    catch (ArgumentException) { return true; }
}
Check("Close callback queues token1 and rejects valid handles or repeated completion",
    GameSocketCloseCompletionNativeLayout.CaptureCompletionEvent(true, false) == 1
    && RejectsCloseCompletion(false, false) && RejectsCloseCompletion(true, true));
Check("Queued close events clean a Destroyed socket, notify active sockets and ignore already closed/disconnected",
    GameSocketCloseCompletionNativeLayout.SelectQueuedCloseEvent(NativeGameSocketState.Destroyed)
        == new QueuedSocketCloseSelection(NativeGameSocketState.Closed, true, false)
    && Enumerable.Range(1, 5).All(state =>
        GameSocketCloseCompletionNativeLayout.SelectQueuedCloseEvent((NativeGameSocketState)state)
            == new QueuedSocketCloseSelection(NativeGameSocketState.DisconnectedNotify, false, false))
    && new[] { NativeGameSocketState.Disconnected, NativeGameSocketState.Closed }.All(state =>
        GameSocketCloseCompletionNativeLayout.SelectQueuedCloseEvent(state) == new QueuedSocketCloseSelection(state, false, true)));
bool RejectsQueuedCloseState(NativeGameSocketState state)
{
    try { GameSocketCloseCompletionNativeLayout.SelectQueuedCloseEvent(state); return false; }
    catch (ArgumentOutOfRangeException) { return true; }
}
Check("Queued close event rejects NotOpened, DisconnectedNotify and unknown states explicitly",
    RejectsQueuedCloseState(NativeGameSocketState.NotOpened)
    && RejectsQueuedCloseState(NativeGameSocketState.DisconnectedNotify)
    && RejectsQueuedCloseState((NativeGameSocketState)10));
var message41AbsentBytes = Convert.FromHexString("29FFFFFFFF0000008080FF3412FE000300DEADBE");
var message41Absent = MatrixServerMessages41To43Wire.Decode(message41AbsentBytes);
Check("Message41 preserves raw scalar widths, absent optional list and length-prefixed blob",
    message41Absent is { Outcome: MatrixServer41To43ReadOutcome.Accepted, BytesConsumed: 20,
        Payload: MatrixMessage41Payload { WordAt28: uint.MaxValue, WordAt2C: 0x80000000,
            ByteAt30: 0x80, ByteAt31: 0xFF, WordAt32: 0x1234, ByteAt34: 0xFE, OptionalEntries: null } }
    && ((MatrixMessage41Payload)message41Absent.Payload).BlobAt48.Span.SequenceEqual(Convert.FromHexString("DEADBE")));
var message41EntryBytes = Convert.FromHexString("08070605040302011020F080");
var message41PresentBytes = message41AbsentBytes[..14].Concat(new byte[] { 0xFE, 2 })
    .Concat(message41EntryBytes).Concat(message41EntryBytes).Concat(new byte[] { 0, 0 }).ToArray();
var message41Present = MatrixServerMessages41To43Wire.Decode(message41PresentBytes);
var message41PresentPayload = message41Present.Payload as MatrixMessage41Payload;
Check("Message41 any nonzero optional marker is present and entries consume twelve wire bytes, not native sixteen",
    message41Present.Outcome == MatrixServer41To43ReadOutcome.Accepted
    && message41Present.BytesConsumed == message41PresentBytes.Length
    && message41PresentPayload?.OptionalEntries is { Count: 2 } entries41
    && entries41[0] == new MatrixMessage41Entry(0x0102030405060708, 0x10, 0x20, 0xF0, 0x80)
    && entries41[0] == entries41[1] && message41PresentPayload.BlobAt48.IsEmpty
    && MatrixMessages41And43NativeLayout.SelectMessage41TransportByte(0xFE) == 0
    && MatrixMessages41And43NativeLayout.SelectMessage41TransportByte(0xFF) == 1);
var message41EmptyPresent = MatrixServerMessages41To43Wire.Decode(
    message41AbsentBytes[..14].Concat(new byte[] { 1, 0, 0, 0 }).ToArray());
Check("Message41 present-empty list differs from absence and packet minimums are exact",
    message41EmptyPresent is { BytesConsumed: 18, Payload: MatrixMessage41Payload { OptionalEntries.Count: 0 } }
    && MatrixServerMessages41To43Wire.Decode(message41AbsentBytes[..14].Concat(new byte[] { 0, 0, 0 }).ToArray())
        .BytesConsumed == MatrixServerMessages41To43Wire.Message41MinimumPacketSize);
var pvpRecordBytes = Convert.FromHexString("FFFFFFFF00000080004100");
var pvpBytes = new byte[] { 42, 2 }.Concat(pvpRecordBytes).Concat(pvpRecordBytes)
    .Concat(Convert.FromHexString("78563412")).ToArray();
var pvpRead = MatrixServerMessages41To43Wire.Decode(pvpBytes);
Check("Message42 PvP queue records preserve order/duplicates, both raw texts and the trailing DWORD",
    pvpRead.Outcome == MatrixServer41To43ReadOutcome.Accepted && pvpRead.BytesConsumed == pvpBytes.Length
    && pvpRead.Payload is MatrixPvpQueuePayload { Records.Count: 2, WordAt34: 0x12345678 } pvpPayload
    && pvpPayload.Records[0].WordAt0 == uint.MaxValue && pvpPayload.Records[0].WordAt4 == 0x80000000
    && pvpPayload.Records[0].TextBytesAt8.IsEmpty
    && pvpPayload.Records[0].TextBytesAtC.Span.SequenceEqual(new byte[] { 0x41 })
    && pvpPayload.Records[1].WordAt0 == uint.MaxValue);
var message43Bytes = Convert.FromHexString("2BFFFFFFFF00FF");
Check("Message43 has a raw DWORD/WORD body even though its handler has no observed payload use",
    MatrixServerMessages41To43Wire.Decode(message43Bytes)
        is { BytesConsumed: 7, Payload: MatrixMessage43Payload { WordAt28: uint.MaxValue, WordAt2C: 0xFF00 } }
    && MatrixServerMessages41To43Wire.Decode(Convert.FromHexString("2A0009000000"))
        is { BytesConsumed: 6, Payload: MatrixPvpQueuePayload { Records.Count: 0, WordAt34: 9 } });
Check("Messages41-43 reject all strict prefixes, malformed/foreign headers and ignore trailing bytes for consumed length",
    new[] { message41AbsentBytes, message41PresentBytes, pvpBytes, message43Bytes }.All(packet =>
        Enumerable.Range(0, packet.Length).All(length =>
            MatrixServerMessages41To43Wire.Decode(packet.AsSpan(0, length)).Outcome != MatrixServer41To43ReadOutcome.Accepted)
        && MatrixServerMessages41To43Wire.Decode(packet.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == packet.Length)
    && MatrixServerMessages41To43Wire.Decode(new byte[] { 0, 41, 0 }).Outcome == MatrixServer41To43ReadOutcome.NonCanonicalHeader
    && MatrixServerMessages41To43Wire.Decode(new byte[] { 40 }).Outcome == MatrixServer41To43ReadOutcome.UnexpectedMessageCode);
var pvp255Body = Enumerable.Range(0, 255).SelectMany(_ => pvpRecordBytes).ToArray();
var pvp255Bytes = new byte[] { 42, 255 }.Concat(pvp255Body)
    .Concat(new byte[] { 0, 9, 0, 0, 0 }).ToArray();
var message41Entries255 = Enumerable.Range(0, 255).SelectMany(_ => message41EntryBytes).ToArray();
var message41Bytes255 = message41AbsentBytes[..14].Concat(new byte[] { 1, 255 })
    .Concat(message41Entries255).Concat(new byte[] { 0, 0, 0 }).ToArray();
Check("Both message41/42 list readers require a terminal zero chunk after exactly255 entries",
    MatrixServerMessages41To43Wire.Decode(pvp255Bytes) is { Payload: MatrixPvpQueuePayload { Records.Count: 255 } }
    && MatrixServerMessages41To43Wire.Decode(message41Bytes255)
        is { Payload: MatrixMessage41Payload { OptionalEntries.Count: 255 } }
    && MatrixServerMessages41To43Wire.Decode(new byte[] { 42, 255 }.Concat(pvp255Body)
        .Concat(new byte[] { 9, 0, 0, 0 }).ToArray()).Outcome == MatrixServer41To43ReadOutcome.TruncatedPayload
    && MatrixServerMessages41To43Wire.Decode(message41Bytes255[..^1]).Outcome == MatrixServer41To43ReadOutcome.TruncatedPayload);
Check("PvP queue matching accepts wildcard9, clears mismatches but reports queued from original input presence",
    MatrixPvpQueueNativeLayout.SelectUpdate(true, 9, 3) == new MatrixPvpQueueUpdateSelection(true, false, true)
    && MatrixPvpQueueNativeLayout.SelectUpdate(true, 3, 3) == new MatrixPvpQueueUpdateSelection(true, false, true)
    && MatrixPvpQueueNativeLayout.SelectUpdate(true, 4, 3) == new MatrixPvpQueueUpdateSelection(false, true, true)
    && MatrixPvpQueueNativeLayout.SelectUpdate(false, 9, 3) == new MatrixPvpQueueUpdateSelection(false, true, false)
    && MatrixPvpQueueNativeLayout.SelectHandlerConsumer(true, true, true)
    && !MatrixPvpQueueNativeLayout.SelectHandlerConsumer(true, false, true)
    && !MatrixPvpQueueNativeLayout.SelectHandlerConsumer(true, true, false));
var pvp256Bytes = new byte[] { 42, 255 }.Concat(pvp255Body).Concat(new byte[] { 1 })
    .Concat(pvpRecordBytes).Concat(new byte[] { 9, 0, 0, 0 }).ToArray();
Check("PvP queue chunk continuation reads a second nonempty chunk without requiring a final zero after count1",
    MatrixServerMessages41To43Wire.Decode(pvp256Bytes)
        is { Payload: MatrixPvpQueuePayload { Records.Count: 256 } } pvp256
    && pvp256.BytesConsumed == pvp256Bytes.Length);
bool RejectsMissingPvpDownstream()
{
    try { MatrixPvpQueueNativeLayout.SelectHandlerConsumer(false, true, true); return false; }
    catch (ArgumentException) { return true; }
}
Check("PvP queue missing downstream is an explicit native prerequisite failure", RejectsMissingPvpDownstream());
var foundMatchBytes = Convert.FromHexString("2C785634120807060504030201FF1111222233334444");
Check("Message44 FoundMatchUpdate has twenty-one body bytes without native alignment padding",
    MatrixServerMessages44To47Wire.Decode(foundMatchBytes)
        is { BytesConsumed: 22, Payload: MatrixFoundMatchUpdatePayload { MatchmakerId: 0x12345678,
            MatchId: 0x0102030405060708, StateByte: 0xFF,
            WordAt3A: 0x1111, WordAt3C: 0x2222, WordAt3E: 0x3333, WordAt40: 0x4444 } });
var joinEmptyBytes = new byte[] { 45 }.Concat(Convert.FromHexString("0807060504030201"))
    .Concat(new byte[] { 0x80, 0, 0xFE }).Concat(Convert.FromHexString("00000080"))
    .Concat(new byte[] { 0 }).Concat(Convert.FromHexString("FFFFFFFF")).ToArray();
var joinEntryBytes = Convert.FromHexString("FFFFFFFF41000080FE01");
var joinBytes = joinEmptyBytes[..16].Concat(new byte[] { 2 }).Concat(joinEntryBytes).Concat(joinEntryBytes)
    .Concat(joinEmptyBytes[^4..]).ToArray();
var joinRead = MatrixServerMessages44To47Wire.Decode(joinBytes);
Check("Message45 retains raw boolean bytes and variable-length challenge records",
    joinRead.Outcome == MatrixServer44To47ReadOutcome.Accepted && joinRead.BytesConsumed == joinBytes.Length
    && joinRead.Payload is MatrixChallengeJoinResponsePayload { ChallengeId: 0x0102030405060708,
        JoinSucceededByte: 0x80, CreatedByte: 0, SquadReasonByte: 0xFE,
        WordAt34: 0x80000000, WordAt44: uint.MaxValue, Entries.Count: 2 } joinPayload
    && joinPayload.Entries[0].WordAt0 == uint.MaxValue
    && joinPayload.Entries[0].TextBytesAt4.Span.SequenceEqual(new byte[] { 0x41 })
    && joinPayload.Entries[0].TextBytesAt8.IsEmpty
    && joinPayload.Entries[0].ByteAtC == 0x80 && joinPayload.Entries[0].ByteAtD == 0xFE
    && joinPayload.Entries[0].ByteAtE == 1
    && MatrixServerMessages44To47Wire.Decode(joinEmptyBytes).BytesConsumed == 21);
var invitation46Bytes = new byte[] { 46 }.Concat(Convert.FromHexString("0807060504030201FFFFFFFFFFFFFFFFFF8000")).ToArray();
var invitation47Bytes = invitation46Bytes.ToArray();
invitation47Bytes[0] = 47;
Check("Messages46/47 have identical two-QWORD/text bodies and distinct consumer variant flags",
    MatrixServerMessages44To47Wire.Decode(invitation46Bytes)
        is { Payload: MatrixChallengeInvitationPayload { MessageCode: 46, QwordAt28: 0x0102030405060708,
            QwordAt30: ulong.MaxValue } } invitation46
    && ((MatrixChallengeInvitationPayload)invitation46.Payload).NameBytes.Span.SequenceEqual(new byte[] { 0xFF, 0x80 })
    && MatrixServerMessages44To47Wire.Decode(invitation47Bytes)
        is { Payload: MatrixChallengeInvitationPayload { MessageCode: 47 } }
    && !MatrixChallengeNativeLayout.SelectInvitationVariant(46) && MatrixChallengeNativeLayout.SelectInvitationVariant(47));
Check("Messages44-47 reject strict prefixes/noncanonical/foreign headers and consume exactly their own bodies",
    new[] { foundMatchBytes, joinEmptyBytes, joinBytes, invitation46Bytes, invitation47Bytes }.All(packet =>
        Enumerable.Range(0, packet.Length).All(length =>
            MatrixServerMessages44To47Wire.Decode(packet.AsSpan(0, length)).Outcome != MatrixServer44To47ReadOutcome.Accepted)
        && MatrixServerMessages44To47Wire.Decode(packet.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == packet.Length)
    && MatrixServerMessages44To47Wire.Decode(new byte[] { 0, 44, 0 }).Outcome == MatrixServer44To47ReadOutcome.NonCanonicalHeader
    && MatrixServerMessages44To47Wire.Decode(new byte[] { 43 }).Outcome == MatrixServer44To47ReadOutcome.UnexpectedMessageCode);
var join255Bytes = joinEmptyBytes[..16].Concat(new byte[] { 255 })
    .Concat(Enumerable.Range(0, 255).SelectMany(_ => joinEntryBytes)).Concat(new byte[] { 0 })
    .Concat(joinEmptyBytes[^4..]).ToArray();
Check("Message45 uses chunked255 counts and no serialized sixteenth native-record padding byte",
    MatrixServerMessages44To47Wire.Decode(join255Bytes)
        is { Payload: MatrixChallengeJoinResponsePayload { Entries.Count: 255 } } join255
    && join255.BytesConsumed == join255Bytes.Length
    && MatrixServerMessages44To47Wire.Decode(join255Bytes[..^1]).Outcome == MatrixServer44To47ReadOutcome.TruncatedPayload);
Check("Challenge join response gates missing/mismatched/invalid stored IDs before selecting release or apply",
    MatrixChallengeNativeLayout.SelectJoinResponse(null, 1, 1, 1, 0).Selection == MatrixChallengeJoinSelection.IgnoreUnexpectedResponse
    && MatrixChallengeNativeLayout.SelectJoinResponse(1, 2, 1, 1, 0).Selection == MatrixChallengeJoinSelection.IgnoreUnexpectedResponse
    && MatrixChallengeNativeLayout.SelectJoinResponse(0, 1, 1, 0, 0).Selection == MatrixChallengeJoinSelection.IgnoreUnexpectedResponse
    && MatrixChallengeNativeLayout.SelectJoinResponse(0xFFFFFFFFFFFFFF42, 1, 1, 0, 0).Selection
        == MatrixChallengeJoinSelection.IgnoreUnexpectedResponse
    && MatrixChallengeNativeLayout.SelectJoinResponse(1, 1, 0, 0, 0)
        == new MatrixChallengeJoinNotificationSelection(MatrixChallengeJoinSelection.ReleaseActiveChallenge, false, null, false));
Check("Challenge success normalizes raw bytes and selects exact runtime reason precedence",
    MatrixChallengeNativeLayout.SelectJoinResponse(0, 1, 0x80, 0xFE, 0)
        == new MatrixChallengeJoinNotificationSelection(MatrixChallengeJoinSelection.ApplyJoinedChallenge, true, "Created", true)
    && MatrixChallengeNativeLayout.SelectJoinResponse(1, 1, 1, 1, 0x80)
        == new MatrixChallengeJoinNotificationSelection(MatrixChallengeJoinSelection.ApplyJoinedChallenge, true, "Squad", true)
    && MatrixChallengeNativeLayout.SelectJoinResponse(1, 1, 1, 0, 0)
        == new MatrixChallengeJoinNotificationSelection(MatrixChallengeJoinSelection.ApplyJoinedChallenge, false, "Accepted", true));
Check("FoundMatch ordinary deadline gate and state2/3 notification suppression are distinct",
    MatrixPvpQueueNativeLayout.SelectFoundMatchNotificationSuppression(2, 2)
    && MatrixPvpQueueNativeLayout.SelectFoundMatchNotificationSuppression(3, 3)
    && !MatrixPvpQueueNativeLayout.SelectFoundMatchNotificationSuppression(1, 1)
    && !MatrixPvpQueueNativeLayout.SelectFoundMatchNotificationSuppression(2, 3)
    && MatrixPvpQueueNativeLayout.SelectFoundMatchDeadlineRefresh(1, 2, 100)
    && MatrixPvpQueueNativeLayout.SelectFoundMatchDeadlineRefresh(2, 2, 0)
    && !MatrixPvpQueueNativeLayout.SelectFoundMatchDeadlineRefresh(2, 2, 100)
    && !MatrixPvpQueueNativeLayout.SelectFoundMatchDeadlineRefresh(2, 4, 0)
    && MatrixPvpQueueNativeLayout.ComputeFoundMatchDeadline(uint.MaxValue - 999, 1) == 0);
var cancellationBytes = Convert.FromHexString("300807060504030201");
var invitationResponseBytes = Convert.FromHexString("310807060504030201FF8000FE");
var kickedBytes = Convert.FromHexString("3208070605040302014100FFFFFFFFFFFFFFFFFF8000");
Check("Messages48-50 decode cancellation, raw invitation-response status, and kicked-player fields",
    MatrixServerMessages48To50Wire.Decode(cancellationBytes)
        is { BytesConsumed: 9, Payload: MatrixChallengeInvitationCancelPayload { ChallengeId: 0x0102030405060708 } }
    && MatrixServerMessages48To50Wire.Decode(invitationResponseBytes)
        is { Payload: MatrixChallengeInvitationResponsePayload { StatusByte: 0xFE } } responseRead
    && ((MatrixChallengeInvitationResponsePayload)responseRead.Payload).NameBytes.Span.SequenceEqual(new byte[] { 0xFF, 0x80 })
    && MatrixServerMessages48To50Wire.Decode(kickedBytes)
        is { Payload: MatrixChallengeKickedPayload { ChallengeId: 0x0102030405060708, KickeeId: ulong.MaxValue } } kickedRead
    && ((MatrixChallengeKickedPayload)kickedRead.Payload).KickerNameBytes.Span.SequenceEqual(new byte[] { 0x41 })
    && ((MatrixChallengeKickedPayload)kickedRead.Payload).KickeeNameBytes.Span.SequenceEqual(new byte[] { 0xFF, 0x80 }));
Check("Messages49/50 empty text minima are eleven/nineteen bytes and all body prefixes fail",
    MatrixServerMessages48To50Wire.Decode(new byte[] { 49 }.Concat(new byte[10]).ToArray()).BytesConsumed == 11
    && MatrixServerMessages48To50Wire.Decode(new byte[] { 50 }.Concat(new byte[18]).ToArray()).BytesConsumed == 19
    && new[] { cancellationBytes, invitationResponseBytes, kickedBytes }.All(packet =>
        Enumerable.Range(0, packet.Length).All(length =>
            MatrixServerMessages48To50Wire.Decode(packet.AsSpan(0, length)).Outcome != MatrixServer48To50ReadOutcome.Accepted)
        && MatrixServerMessages48To50Wire.Decode(packet.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == packet.Length)
    && MatrixServerMessages48To50Wire.Decode(new byte[] { 0, 48, 0 }).Outcome == MatrixServer48To50ReadOutcome.NonCanonicalHeader
    && MatrixServerMessages48To50Wire.Decode(new byte[] { 47 }).Outcome == MatrixServer48To50ReadOutcome.UnexpectedMessageCode);
var expectedChallengeResponseReasons = new[]
{
    "Accepted", "Declined", "DoesNotExist", "Offline", "TeamFull", "MatchStarting",
    "TargetNotChallengeable", "SquadTeamFull"
};
Check("Invitation response accepted is strictly status0 and reason uses all eight native entries or explicit UNKNOWN",
    Enumerable.Range(0, 256).All(status =>
    {
        var notification = MatrixChallengeFollowupNativeLayout.CaptureResponse(new byte[] { 0x80 }, (byte)status);
        return notification.RuntimeMessageId == 272 && notification.Accepted == (status == 0)
            && notification.NameBytes.Span.SequenceEqual(new byte[] { 0x80 })
            && notification.Reason == (status < 8 ? expectedChallengeResponseReasons[status] : "UNKNOWN");
    }));
var invitationSnapshot = new[]
{
    new MatrixChallengeInvitationSnapshot(new byte[] { 1 }, 7),
    new MatrixChallengeInvitationSnapshot(new byte[] { 2 }, 7),
    new MatrixChallengeInvitationSnapshot(new byte[] { 3 }, 0xFFFFFFFFFFFFFF07),
    new MatrixChallengeInvitationSnapshot(new byte[] { 4 }, 7)
};
Check("Invitation cancellation selects every exact64-bit match, including adjacent duplicates",
    MatrixChallengeFollowupNativeLayout.SelectCancellationMatches(invitationSnapshot, 7).SequenceEqual(new[] { 0, 1, 3 })
    && MatrixChallengeFollowupNativeLayout.SelectCancellationMatches(invitationSnapshot, 8).Count == 0
    && invitationSnapshot.Length == 4);
Check("Kicked handler requires current-mode/active pointer and exact ID; local comparison retains native zero fallback",
    MatrixChallengeFollowupNativeLayout.ActiveChallengeModeMemberOffset == 0xE4
    && MatrixChallengeFollowupNativeLayout.SelectKickedHandler(true, 7, 7)
    && MatrixChallengeFollowupNativeLayout.SelectKickedHandler(true, 0, 0)
    && !MatrixChallengeFollowupNativeLayout.SelectKickedHandler(false, 7, 7)
    && !MatrixChallengeFollowupNativeLayout.SelectKickedHandler(true, null, 7)
    && !MatrixChallengeFollowupNativeLayout.SelectKickedHandler(true, 7, 0xFFFFFFFFFFFFFF07)
    && MatrixChallengeFollowupNativeLayout.SelectLocalKickedBranch(7, 7)
    && !MatrixChallengeFollowupNativeLayout.SelectLocalKickedBranch(7, null)
    && MatrixChallengeFollowupNativeLayout.SelectLocalKickedBranch(0, null));
var challengeIdOnlyBytes = new[] { 51, 53, 55 }.Select(code =>
    new byte[] { (byte)code }.Concat(Convert.FromHexString("0807060504030201")).ToArray()).ToArray();
var challengeRecordBytes = Convert.FromHexString("0807060504030201FF8000FEFFFFFFFF80");
var challengeRecordsBytes = new byte[] { 52 }.Concat(Convert.FromHexString("0807060504030201"))
    .Concat(new byte[] { 2 }).Concat(challengeRecordBytes).Concat(challengeRecordBytes).ToArray();
var challengeWordBytes = Convert.FromHexString("360807060504030201FFFFFFFF");
Check("Challenge51/53/55 bodies are QWORD-only;54 appends one raw DWORD",
    challengeIdOnlyBytes.All(packet => MatrixServerMessages51To55Wire.Decode(packet)
        is { BytesConsumed: 9, Payload: MatrixChallengeIdOnlyPayload { ChallengeId: 0x0102030405060708 } } result
        && ((MatrixChallengeIdOnlyPayload)result.Payload).MessageCode == packet[0])
    && MatrixServerMessages51To55Wire.Decode(challengeWordBytes)
        is { BytesConsumed: 13, Payload: MatrixChallengeWordUpdatePayload { WordAt30: uint.MaxValue } });
var challengeRecordsRead = MatrixServerMessages51To55Wire.Decode(challengeRecordsBytes);
Check("Challenge52 native24-byte records contain a QWORD, raw NUL text, byte, DWORD and byte without padding",
    challengeRecordsRead.Outcome == MatrixServer51To55ReadOutcome.Accepted
    && challengeRecordsRead.BytesConsumed == challengeRecordsBytes.Length
    && challengeRecordsRead.Payload is MatrixChallengeRecordListPayload { Records.Count: 2 } challengeRecords
    && challengeRecords.Records[0].QwordAt0 == 0x0102030405060708
    && challengeRecords.Records[0].TextBytesAt8.Span.SequenceEqual(new byte[] { 0xFF, 0x80 })
    && challengeRecords.Records[0].ByteAtC == 0xFE && challengeRecords.Records[0].WordAt10 == uint.MaxValue
    && challengeRecords.Records[0].ByteAt14 == 0x80
    && MatrixServerMessages51To55Wire.Decode(new byte[] { 52 }.Concat(new byte[9]).ToArray()).BytesConsumed == 10);
Check("Challenge51-55 all strict prefixes fail and trailing bytes do not enlarge consumed length",
    challengeIdOnlyBytes.Concat(new[] { challengeRecordsBytes, challengeWordBytes }).All(packet =>
        Enumerable.Range(0, packet.Length).All(length =>
            MatrixServerMessages51To55Wire.Decode(packet.AsSpan(0, length)).Outcome != MatrixServer51To55ReadOutcome.Accepted)
        && MatrixServerMessages51To55Wire.Decode(packet.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == packet.Length)
    && MatrixServerMessages51To55Wire.Decode(new byte[] { 0, 51, 0 }).Outcome == MatrixServer51To55ReadOutcome.NonCanonicalHeader
    && MatrixServerMessages51To55Wire.Decode(new byte[] { 50 }).Outcome == MatrixServer51To55ReadOutcome.UnexpectedMessageCode);
var challengeRecords255 = challengeRecordsBytes[..9].Concat(new byte[] { 255 })
    .Concat(Enumerable.Range(0, 255).SelectMany(_ => challengeRecordBytes)).Concat(new byte[] { 0 }).ToArray();
Check("Challenge52 records require terminal zero after count255 and preserve all entries",
    MatrixServerMessages51To55Wire.Decode(challengeRecords255)
        is { Payload: MatrixChallengeRecordListPayload { Records.Count: 255 } }
    && MatrixServerMessages51To55Wire.Decode(challengeRecords255[..^1]).Outcome == MatrixServer51To55ReadOutcome.TruncatedPayload);
Check("Challenge51/52/54 accept invalid stored ID as well as exact match, but53/55 do not",
    new uint[] { 51, 52, 54 }.All(code =>
        MatrixChallengeUpdateNativeLayout.SelectHandler(code, true, 0, 7).Action != MatrixChallengeUpdateAction.Ignore
        && MatrixChallengeUpdateNativeLayout.SelectHandler(code, true, 0xFFFFFFFFFFFFFF80, 7).Action != MatrixChallengeUpdateAction.Ignore)
    && new uint[] { 53, 55 }.All(code =>
        MatrixChallengeUpdateNativeLayout.SelectHandler(code, true, 0, 7).Action == MatrixChallengeUpdateAction.Ignore)
    && Enumerable.Range(51, 5).All(code =>
        MatrixChallengeUpdateNativeLayout.SelectHandler((uint)code, true, 7, 8).Action == MatrixChallengeUpdateAction.Ignore
        && MatrixChallengeUpdateNativeLayout.SelectHandler((uint)code, false, 7, 7).Action == MatrixChallengeUpdateAction.Ignore
        && MatrixChallengeUpdateNativeLayout.SelectHandler((uint)code, true, null, 7).Action == MatrixChallengeUpdateAction.Ignore));
Check("Challenge51-55 select their distinct release/copy/word/notify and runtime boundaries",
    MatrixChallengeUpdateNativeLayout.SelectHandler(51, true, 7, 7) == new MatrixChallengeUpdateSelection(MatrixChallengeUpdateAction.ReleaseActiveChallenge, 275)
    && MatrixChallengeUpdateNativeLayout.SelectHandler(52, true, 7, 7) == new MatrixChallengeUpdateSelection(MatrixChallengeUpdateAction.ReplaceRecords, 273)
    && MatrixChallengeUpdateNativeLayout.SelectHandler(53, true, 7, 7) == new MatrixChallengeUpdateSelection(MatrixChallengeUpdateAction.NotifyOnly, 276)
    && MatrixChallengeUpdateNativeLayout.SelectHandler(54, true, 7, 7) == new MatrixChallengeUpdateSelection(MatrixChallengeUpdateAction.SelectWord, 277)
    && MatrixChallengeUpdateNativeLayout.SelectHandler(55, true, 7, 7) == new MatrixChallengeUpdateSelection(MatrixChallengeUpdateAction.NotifyOnly, 278));
var challengeSelectionRecords = new[]
{
    new MatrixChallengeJoinEntry(5, default, default, 0, 0, 0),
    new MatrixChallengeJoinEntry(7, default, default, 0, 0, 0),
    new MatrixChallengeJoinEntry(7, default, default, 0, 0, 0)
};
Check("Challenge word selection picks first matching16-byte record or raw all-FF index and retains notification flag",
    MatrixChallengeUpdateNativeLayout.CaptureWordSelection(7, challengeSelectionRecords, true)
        == new MatrixChallengeWordSelection(7, 1, true)
    && MatrixChallengeUpdateNativeLayout.CaptureWordSelection(8, challengeSelectionRecords, false)
        == new MatrixChallengeWordSelection(8, uint.MaxValue, false)
    && MatrixChallengeUpdateNativeLayout.CaptureWordSelection(7, Array.Empty<MatrixChallengeJoinEntry>(), true).IndexAt24 == uint.MaxValue);
Check("tfLogin auto-login keys and endpoint template are recorded",
    LoginModeNativeLayout.KnownAutoLoginSettings.SequenceEqual(
    [
        "MaxTimes", "Account", "SavedPassword", "CharacterName",
        "DevMatrixAddress", "SpectatorMode", "SpectatorDirectorControl",
        "SpectatorTargetGuid", "RandomReplay"
    ])
    && LoginModeNativeLayout.LoginEndpointFormat == "%s/api/v2/accounts/login");

MatrixServer56To61Checks.Run(Check);
MatrixServer63To68Checks.Run(Check);
MatrixServer34And36Checks.Run(Check);
MatrixChatDisplayChecks.Run(Check);
MatrixChatLogChecks.Run(Check);
MatrixChatFileChecks.Run(Check);
MatrixChatIdentityChecks.Run(Check);
MatrixRequestSigscan32Checks.Run(Check);
MatrixSynchronization57Checks.Run(Check);
GameSocketAcknowledgmentBodyChecks.Run(Check);
MatrixServerMessageNativeTypeChecks.Run(Check);
MatrixSynchronizationDispatchChecks.Run(Check);
NativeMt19937Checks.Run(Check);
NativeByteVectorHeapChecks.Run(Check);
NativeRecordVectorCapacityProjectionChecks.Run(Check);
NativeByteVectorSortChecks.Run(Check);
NativeAdapterAddressFingerprintChecks.Run(Check);
NativeWindowsDriveSerialFingerprintChecks.Run(Check);
MatrixSynchronizationModeZeroDigestSlotsChecks.Run(Check);
MatrixModeZeroDescriptorChecks.Run(Check);
NativeMetadataTextTailChecks.Run(Check);
NativeFolderPrefixAliasChecks.Run(Check);
NativeModulePathProviderChecks.Run(Check);
NativeGeneratedTransformProviderPrefixChecks.Run(Check);
NativeGeneratedDescriptorFactoryChecks.Run(Check);
NativeGeneratedBodyDecisionChecks.Run(Check);
NativeGeneratedConditionalBranchChecks.Run(Check);
NativeGeneratedByteLoopWrapperChecks.Run(Check);
NativeGeneratedScratchTemplateChecks.Run(Check);
NativeGeneratedArithmeticTemplateChecks.Run(Check);
NativeGeneratedInitialModeZeroImageChecks.Run(Check);
NativeGeneratedArenaPolicyChecks.Run(Check);
NativeGeneratedModeOneBoundaryChecks.Run(Check);
NativeGeneratedModeOneMemoryShapeChecks.Run(Check);
NativeGeneratedModeOneMemoryEncodingChecks.Run(Check);
NativeGeneratedModeOneImmediateMoveChecks.Run(Check);
NativeGeneratedModeOneUnaryChecks.Run(Check);
NativeGeneratedModeOneArithmeticChecks.Run(Check);
NativeGeneratedModeOneWrapperChecks.Run(Check);
NativeGeneratedDefaultHeaderImageChecks.Run(Check);
MatrixServerProfilerNamesChecks.Run(Check);
MatrixServerProfilerFrameChecks.Run(Check);
UiPayloadProducerGateChecks.Run(Check);
UiCursorCacheProjectionChecks.Run(Check);
UiOldPairCallbackGateProjectionChecks.Run(Check);
NativeProfilerSampleChecks.Run(Check);
NativeProfilerTreeChecks.Run(Check);
NativeProfilerFrameRefreshChecks.Run(Check);
NativeProfilerDisplayNameChecks.Run(Check);
NativeProfilerNodeSnapshotChecks.Run(Check);
NativeProfilerNameRequestChecks.Run(Check);

var dialogMessageVtables = MatrixMessageParserFlowNativeLayout.DialogMessageVtables;
var expectedDialogSlots = new IReadOnlyList<MatrixMessageVtableSlotSnapshot>[]
{
    new[]
    {
        new MatrixMessageVtableSlotSnapshot(0, 0x5EEC40, MatrixMessageVtableSlotRole.WriteToStream),
        new MatrixMessageVtableSlotSnapshot(4, 0x5EEAE0, MatrixMessageVtableSlotRole.ReadFromStream),
        new MatrixMessageVtableSlotSnapshot(8, 0x526770, MatrixMessageVtableSlotRole.DeletingDestructor)
    },
    new[]
    {
        new MatrixMessageVtableSlotSnapshot(0, 0x5EEA40, MatrixMessageVtableSlotRole.WriteToStream),
        new MatrixMessageVtableSlotSnapshot(4, 0x5EE930, MatrixMessageVtableSlotRole.ReadFromStream),
        new MatrixMessageVtableSlotSnapshot(8, 0x526880, MatrixMessageVtableSlotRole.DeletingDestructor)
    },
    new[]
    {
        new MatrixMessageVtableSlotSnapshot(0, 0x5EE830, MatrixMessageVtableSlotRole.WriteToStream),
        new MatrixMessageVtableSlotSnapshot(4, 0x5EE760, MatrixMessageVtableSlotRole.ReadFromStream),
        new MatrixMessageVtableSlotSnapshot(8, 0x526900, MatrixMessageVtableSlotRole.DeletingDestructor)
    }
};
var callbackContextVtable = MatrixMessageParserFlowNativeLayout.ReplayReaderCallbackContextVtable;
var expectedCallbackContextSlots = new[]
{
    new MatrixCallbackContextVtableSlotSnapshot(
        4,
        0xBFE7E0,
        0x7FDBE0,
        MatrixCallbackContextVtableSlotRole.LogOnly),
    new MatrixCallbackContextVtableSlotSnapshot(
        8,
        0xBFE800,
        0x7FDC00,
        MatrixCallbackContextVtableSlotRole.ModeDependentMessageDispatch)
};
var messageReceiverVtable = MatrixMessageParserFlowNativeLayout.ReplayReaderMessageReceiver;
var expectedMessageReceiverDialogSlots = new[]
{
    new MatrixMessageReceiverDialogSlotSnapshot(
        0x78,
        0x164,
        0xECEEE0,
        0xACE2E0,
        MatrixMessageReceiverVtableTargetKind.DirectFunction,
        null,
        null),
    new MatrixMessageReceiverDialogSlotSnapshot(
        0x79,
        0x168,
        0xECEF00,
        0xACE300,
        MatrixMessageReceiverVtableTargetKind.TailJumpThunk,
        0xD92110,
        0x991510),
    new MatrixMessageReceiverDialogSlotSnapshot(
        0x7A,
        0x16C,
        0xECEF90,
        0xACE390,
        MatrixMessageReceiverVtableTargetKind.DirectFunction,
        null,
        null)
};
Check("Matrix parser callsites, callback invocation, and result consumption remain explicit",
    MatrixMessageParserFlowNativeLayout.NestedCommandParserFileOffset == 0x81CC50
    && MatrixMessageParserFlowNativeLayout.DirectParserCallSites.SequenceEqual(
    [
        new MatrixParserCallSiteSnapshot(0x821BA0, 0x821D87),
        new MatrixParserCallSiteSnapshot(0x821DE0, 0x821E1E)
    ])
    && MatrixMessageParserFlowNativeLayout.StreamParserCallerVirtualAddress
        == MatrixMessageParserFlowNativeLayout.StreamParserCallerFileOffset + 0x400C00
    && MatrixMessageParserFlowNativeLayout.ParserWrapperVirtualAddress
        == MatrixMessageParserFlowNativeLayout.ParserWrapperFileOffset + 0x400C00
    && MatrixMessageParserFlowNativeLayout.ReplayReaderAllocationCallFileOffset == 0x65DAF2
    && MatrixMessageParserFlowNativeLayout.ReplayReaderAllocationHelperFileOffset == 0x12B7FB0
    && MatrixMessageParserFlowNativeLayout.ReplayReaderAllocatorBackendCallFileOffset == 0x12B7FC7
    && MatrixMessageParserFlowNativeLayout.ReplayReaderAllocatorBackendFileOffset == 0x12C8A20
    && MatrixMessageParserFlowNativeLayout.ReplayReaderCallbackObjectCallFileOffset == 0x65DB3F
    && MatrixMessageParserFlowNativeLayout.ReplayReaderCallbackRegistrationCallFileOffset == 0x65DBD3
    && MatrixMessageParserFlowNativeLayout.ReplayReaderCallbackRegistrationHelperFileOffset == 0xBD6260
    && MatrixMessageParserFlowNativeLayout.ReplayReaderReadinessProbeFileOffset == 0xBD68D0
    && MatrixMessageParserFlowNativeLayout.ReplayReaderReadinessProbeCallFileOffset == 0xBD68F4
    && MatrixMessageParserFlowNativeLayout.RegisteredParserInvokerFileOffset == 0xBD6B40
    && MatrixMessageParserFlowNativeLayout.RegisteredParserCallbackLoadFileOffset == 0xBD6D29
    && MatrixMessageParserFlowNativeLayout.RegisteredParserCallbackCallFileOffset == 0xBD6D3C
    && MatrixMessageParserFlowNativeLayout.CallbackResultQueueInsertionCallFileOffset == 0xBD6D66
    && MatrixMessageParserFlowNativeLayout.CallbackResultQueueHelperFileOffset == 0xBD8B80
    && MatrixMessageParserFlowNativeLayout.CallbackResultConsumerFileOffset == 0xBD62F0
    && MatrixMessageParserFlowNativeLayout.CallbackResultConsumerQueueFieldSetupFileOffset == 0xBD65B6
    && MatrixMessageParserFlowNativeLayout.CallbackResultDispatchQueueFieldSetupFileOffset == 0xBD6615
    && MatrixMessageParserFlowNativeLayout.CallbackResultDispatchQueueInsertionCallFileOffset == 0xBD661B
    && MatrixMessageParserFlowNativeLayout.CallbackResultConsumerContextLoadFileOffset == 0xBD66B3
    && MatrixMessageParserFlowNativeLayout.CallbackResultConsumerContextDispatchFileOffset == 0xBD66F2
    && MatrixMessageParserFlowNativeLayout.CallbackResultConsumerAlternateContextDispatchFileOffset == 0xBD6702
    && MatrixMessageParserFlowNativeLayout.CallbackResultConsumerContextDispatchOtherFileOffset == 0xBD6710
    && MatrixMessageParserFlowNativeLayout.CallbackResultReferenceDecrementFileOffset == 0xBD6721
    && MatrixMessageParserFlowNativeLayout.CallbackResultFinalReleaseCallFileOffset == 0xBD6733
    && MatrixMessageParserFlowNativeLayout.CallbackResultQueueRecordCopyFileOffset == 0xBD8BE3
    && MatrixMessageParserFlowNativeLayout.CallbackResultQueueFieldOffset == 0xB4
    && MatrixMessageParserFlowNativeLayout.CallbackResultDispatchQueueFieldOffset == 0xC8
    && MatrixMessageParserFlowNativeLayout.ReplayReaderCallbackRecordParserFieldOffset == 4
    && MatrixMessageParserFlowNativeLayout.ReplayReaderRegisteredParserFieldOffset == 0x1C
    && MatrixMessageParserFlowNativeLayout.ReplayReaderRegisteredContextFieldOffset == 0x20
    && MatrixMessageParserFlowNativeLayout.Route2MessagePointerEaxRestoreFileOffset == 0x81EF58
    && MatrixMessageParserFlowNativeLayout.StreamParserCallerPostCallFileOffset == 0x821D8C
    && MatrixMessageParserFlowNativeLayout.ParserWrapperPostCallFileOffset == 0x821E23
    && MatrixMessageParserFlowNativeLayout.RegisteredParserResultEaxMoveFileOffset == 0xBD6D41
    && MatrixMessageParserFlowNativeLayout.RegisteredParserResultRecordStoreFileOffset == 0xBD6D63
    && MatrixMessageParserFlowNativeLayout.CallbackObjectParserPointerFieldOffset == 0x10);
Check("ReplayReader callback context resolves its constructor-returned primary vtable and dispatch slots",
    callbackContextVtable.RttiName == ".?AVtfGameSocketHandler@@"
    && callbackContextVtable.VtableVirtualAddress == 0x1C6D624
    && callbackContextVtable.ObjectVtableFieldOffset == 0
    && callbackContextVtable.PrimarySlots.SequenceEqual(expectedCallbackContextSlots)
    && MatrixMessageParserFlowNativeLayout.CallbackContextCompleteObjectLocatorVirtualAddress == 0x1CBF794
    && MatrixMessageParserFlowNativeLayout.CallbackContextCompleteObjectLocatorObjectOffset == 0
    && MatrixMessageParserFlowNativeLayout.CallbackContextRttiTypeDescriptorVirtualAddress == 0x1E239BC
    && MatrixMessageParserFlowNativeLayout.ReplayReaderContextPointerOwnerFieldOffset == 0x54
    && MatrixMessageParserFlowNativeLayout.CallbackObjectModeFlagFieldOffset == 0x18
    && MatrixMessageParserFlowNativeLayout.CallbackObjectDispatchSinkFieldOffset == 8
    && MatrixMessageParserFlowNativeLayout.CallbackObjectAlternateDispatchSinkFieldOffset == 0xB0);
Check("ReplayReader message dispatch receiver and discriminator vtable cases remain explicit",
    MatrixMessageParserFlowNativeLayout.CallbackContextDispatchHelperVirtualAddress == 0xBFE860
    && MatrixMessageParserFlowNativeLayout.CallbackContextDispatchHelperVirtualAddress
        == MatrixMessageParserFlowNativeLayout.CallbackContextDispatchHelperFileOffset + 0x400C00
    && MatrixMessageParserFlowNativeLayout.CallbackContextMessageDispatchReceiverFieldOffset == 8
    && MatrixMessageParserFlowNativeLayout.CallbackContextMessageSelectorSwitchFileOffset == 0x81BBBA
    && MatrixMessageParserFlowNativeLayout.CallbackContextMessageSelectorTableVirtualAddress == 0xC1D6DC
    && MatrixMessageParserFlowNativeLayout.CallbackContextMessageSelectorTableFileOffset == 0x81CADC
    && MatrixMessageParserFlowNativeLayout.CallbackContextMessageSelectorFirstDiscriminator == 0x20
    && MatrixMessageParserFlowNativeLayout.CallbackContextMessageSelectorLastDiscriminator == 0x7A
    && MatrixMessageParserFlowNativeLayout.CallbackContextMessageSelectorCaseCount == 0x5B
    && MatrixMessageParserFlowNativeLayout.CallbackContextProtocolMessagePointerLoadFileOffset == 0x7FDC6E
    && MatrixMessageParserFlowNativeLayout.CallbackContextProtocolMessageDispatchCallFileOffset == 0x7FDD4F
    && MatrixMessageParserFlowNativeLayout.ReplayReaderMessageRecipientGetterCallFileOffset == 0x65DB10
    && MatrixMessageParserFlowNativeLayout.ReplayReaderMessageRecipientFieldLoadFileOffset == 0x65DB38
    && MatrixMessageParserFlowNativeLayout.ReplayReaderMessageRecipientSourceFieldOffset == 0x78
    && MatrixMessageParserFlowNativeLayout.MessageDispatchVtableCases.SequenceEqual(
    [
        new MatrixMessageDispatchVtableCaseSnapshot(0x20, 0xC1C7CD, 0x81BBCD, 0x81BBEC, 4),
        new MatrixMessageDispatchVtableCaseSnapshot(0x21, 0xC1C7F5, 0x81BBF5, 0x81BC14, 8),
        new MatrixMessageDispatchVtableCaseSnapshot(0x40, 0xC1CCD0, 0x81C0D0, 0x81C0EF, 0x84),
        new MatrixMessageDispatchVtableCaseSnapshot(0x53, 0xC1D001, 0x81C401, 0x81C420, 0xD0),
        new MatrixMessageDispatchVtableCaseSnapshot(0x54, 0xC1D02C, 0x81C42C, 0x81C44B, 0xD4),
        new MatrixMessageDispatchVtableCaseSnapshot(0x55, 0xC1D057, 0x81C457, 0x81C476, 0xD8),
        new MatrixMessageDispatchVtableCaseSnapshot(0x56, 0xC1D082, 0x81C482, 0x81C4A1, 0xDC),
        new MatrixMessageDispatchVtableCaseSnapshot(0x57, 0xC1D0AD, 0x81C4AD, 0x81C4CC, 0xE0),
        new MatrixMessageDispatchVtableCaseSnapshot(0x58, 0xC1D0D8, 0x81C4D8, 0x81C4F7, 0xE4),
        new MatrixMessageDispatchVtableCaseSnapshot(0x59, 0xC1D103, 0x81C503, 0x81C522, 0xE8),
        new MatrixMessageDispatchVtableCaseSnapshot(0x5A, 0xC1D12E, 0x81C52E, 0x81C54D, 0xEC),
        new MatrixMessageDispatchVtableCaseSnapshot(0x5B, 0xC1D159, 0x81C559, 0x81C578, 0xF0),
        new MatrixMessageDispatchVtableCaseSnapshot(0x5C, 0xC1D184, 0x81C584, 0x81C5A3, 0xF4),
        new MatrixMessageDispatchVtableCaseSnapshot(0x5D, 0xC1D1AF, 0x81C5AF, 0x81C5CE, 0xF8),
        new MatrixMessageDispatchVtableCaseSnapshot(0x5E, 0xC1D1DA, 0x81C5DA, 0x81C5F9, 0xFC),
        new MatrixMessageDispatchVtableCaseSnapshot(0x5F, 0xC1D205, 0x81C605, 0x81C624, 0x100),
        new MatrixMessageDispatchVtableCaseSnapshot(0x60, 0xC1D230, 0x81C630, 0x81C64F, 0x104),
        new MatrixMessageDispatchVtableCaseSnapshot(0x61, 0xC1D25B, 0x81C65B, 0x81C67A, 0x108),
        new MatrixMessageDispatchVtableCaseSnapshot(0x62, 0xC1D286, 0x81C686, 0x81C6A5, 0x10C),
        new MatrixMessageDispatchVtableCaseSnapshot(0x63, 0xC1D2B1, 0x81C6B1, 0x81C6D0, 0x110),
        new MatrixMessageDispatchVtableCaseSnapshot(0x64, 0xC1D2DC, 0x81C6DC, 0x81C6FB, 0x114),
        new MatrixMessageDispatchVtableCaseSnapshot(0x65, 0xC1D307, 0x81C707, 0x81C726, 0x118),
        new MatrixMessageDispatchVtableCaseSnapshot(0x66, 0xC1D332, 0x81C732, 0x81C751, 0x11C),
        new MatrixMessageDispatchVtableCaseSnapshot(0x67, 0xC1D35D, 0x81C75D, 0x81C77C, 0x120),
        new MatrixMessageDispatchVtableCaseSnapshot(0x68, 0xC1D388, 0x81C788, 0x81C7A7, 0x124),
        new MatrixMessageDispatchVtableCaseSnapshot(0x69, 0xC1D3B3, 0x81C7B3, 0x81C7D2, 0x128),
        new MatrixMessageDispatchVtableCaseSnapshot(0x6A, 0xC1D3DE, 0x81C7DE, 0x81C7FD, 0x12C),
        new MatrixMessageDispatchVtableCaseSnapshot(0x6B, 0xC1D409, 0x81C809, 0x81C828, 0x130),
        new MatrixMessageDispatchVtableCaseSnapshot(0x6C, 0xC1D434, 0x81C834, 0x81C853, 0x134),
        new MatrixMessageDispatchVtableCaseSnapshot(0x6D, 0xC1D45F, 0x81C85F, 0x81C87E, 0x138),
        new MatrixMessageDispatchVtableCaseSnapshot(0x6E, 0xC1D48A, 0x81C88A, 0x81C8A9, 0x13C),
        new MatrixMessageDispatchVtableCaseSnapshot(0x6F, 0xC1D4B5, 0x81C8B5, 0x81C8D4, 0x140),
        new MatrixMessageDispatchVtableCaseSnapshot(0x70, 0xC1D4E0, 0x81C8E0, 0x81C8FF, 0x144),
        new MatrixMessageDispatchVtableCaseSnapshot(0x71, 0xC1D50B, 0x81C90B, 0x81C92A, 0x148),
        new MatrixMessageDispatchVtableCaseSnapshot(0x74, 0xC1D58C, 0x81C98C, 0x81C9AB, 0x154),
        new MatrixMessageDispatchVtableCaseSnapshot(0x75, 0xC1D5B7, 0x81C9B7, 0x81C9D6, 0x158),
        new MatrixMessageDispatchVtableCaseSnapshot(0x76, 0xC1D5E2, 0x81C9E2, 0x81CA01, 0x15C),
        new MatrixMessageDispatchVtableCaseSnapshot(0x77, 0xC1D60D, 0x81CA0D, 0x81CA2C, 0x160),
        new MatrixMessageDispatchVtableCaseSnapshot(0x78, 0xC1D638, 0x81CA38, 0x81CA57, 0x164),
        new MatrixMessageDispatchVtableCaseSnapshot(0x79, 0xC1D663, 0x81CA63, 0x81CA82, 0x168),
        new MatrixMessageDispatchVtableCaseSnapshot(0x7A, 0xC1D68E, 0x81CA8E, 0x81CAAD, 0x16C)
    ]));
var messageDiscriminatorProvenance = MatrixMessageParserFlowNativeLayout.MessageDiscriminatorProvenance;
Check("Matrix message discriminator provenance distinguishes parser stack locals from the message field",
    MatrixMessageParserFlowNativeLayout.MessagePrimaryVtableFieldOffset == 0
    && MatrixMessageParserFlowNativeLayout.MessageBaseAuxiliaryPointerFieldOffset == 0xC
    && MatrixMessageParserFlowNativeLayout.MessageDiscriminatorFieldOffset == 0x10
    && MatrixMessageParserFlowNativeLayout.MessageEnvelopeKeyLowDwordFieldOffset == 0x18
    && MatrixMessageParserFlowNativeLayout.MessageEnvelopeKeyHighDwordFieldOffset == 0x1C
    && MatrixMessageParserFlowNativeLayout.ParserStackLocalDwordOffset == 0x20
    && MatrixMessageParserFlowNativeLayout.ForcedMovementInlineStateOffset == 0x28
    && MatrixMessageParserFlowNativeLayout.ForcedMovementInlineStateInitializerFileOffset == 0x92B8A0
    && MatrixMessageParserFlowNativeLayout.ConstructorDiscriminatorStackStoreInstructionOffset == 0x12
    && MatrixMessageParserFlowNativeLayout.ConstructorBaseHelperCallInstructionOffset == 0x18
    && MatrixMessageParserFlowNativeLayout.BaseHelperInitialVtableVirtualAddress == 0x1C61B84
    && MatrixMessageParserFlowNativeLayout.BaseHelperFinalVtableStoreInstructionOffset == 0x5D
    && MatrixMessageParserFlowNativeLayout.BaseHelperDiscriminatorFieldStoreInstructionOffset == 0x21
    && messageDiscriminatorProvenance.Count == 5
    && messageDiscriminatorProvenance.All(provenance =>
        provenance.ConstructionSelector == provenance.MessageDiscriminator
        && provenance.ReceiverVtableSlotOffset
            == 4 + 4 * (provenance.MessageDiscriminator - 0x20)
        && provenance.BaseHelperDiscriminatorFieldStoreFileOffset
            == provenance.BaseHelperFileOffset
                + MatrixMessageParserFlowNativeLayout.BaseHelperDiscriminatorFieldStoreInstructionOffset
        && provenance.BaseHelperFinalVtableStoreFileOffset
            == provenance.BaseHelperFileOffset
                + MatrixMessageParserFlowNativeLayout.BaseHelperFinalVtableStoreInstructionOffset
        && provenance.ConstructorDiscriminatorStackStoreFileOffset
            == provenance.ConstructorFileOffset
                + MatrixMessageParserFlowNativeLayout.ConstructorDiscriminatorStackStoreInstructionOffset
        && provenance.ConstructorBaseHelperCallFileOffset
            == provenance.ConstructorFileOffset
                + MatrixMessageParserFlowNativeLayout.ConstructorBaseHelperCallInstructionOffset
        && provenance.BaseHelperEnvelopeKeyStoresInstructionOffset
            == (provenance.MessageRttiName == AreaVisualDataLootObjectCollectedNativeLayout.RttiName
                ? 0x28u
                : MatrixMessageParserFlowNativeLayout.BaseHelperConstructorArgumentStoresInstructionOffset)
        && provenance.ReceiverVtableCallFileOffset == provenance.ReceiverSwitchTargetFileOffset + 0x1F
        && provenance.MessageVtableCompleteObjectLocatorObjectOffset == 0)
    && messageDiscriminatorProvenance.Select(provenance =>
        (provenance.ConstructionSelector, provenance.ParserStackLocalDwordValue,
            provenance.ReceiverVtableSlotOffset, provenance.ReceiverMethodVirtualAddress))
        .SequenceEqual(
        [
            (0x71, 0x79u, 0x148, 0xECED30u),
            (0x77, 0x7Fu, 0x160, 0xECEEA0u),
            (0x79, 0x81u, 0x168, 0xECEF00u),
            (0x53, 0xCDu, 0xD0, 0xECB180u),
            (0x53, 0xD0u, 0xD0, 0xECB180u)
        ])
    && messageDiscriminatorProvenance.Any(provenance =>
        provenance.MessageRttiName == AreaVisualDataLootObjectCollectedNativeLayout.RttiName
        && provenance.ParserCaseFileOffset == AreaVisualDataLootObjectCollectedNativeLayout.CaseFileOffset
        && provenance.ParserConstructorCallFileOffset
            == AreaVisualDataLootObjectCollectedNativeLayout.ConstructorCallSiteFileOffset
        && provenance.MessageVtableVirtualAddress == AreaVisualDataLootObjectCollectedNativeLayout.VtableVirtualAddress
        && provenance.BaseHelperEnvelopeKeyStoresInstructionOffset == 0x28
        && provenance.BaseHelperAuxiliaryPointerVirtualAddress
            == AreaVisualDataLootObjectCollectedNativeLayout.BaseMetadataStringVirtualAddress));
var selector53Namespaces = MatrixMessageNamespaceRoutingNativeLayout.Selector53Namespaces;
Check("Matrix parser routes preserve distinct namespaces for duplicate selector 0x53",
    selector53Namespaces.Count == 6
    && selector53Namespaces.Select(snapshot => snapshot.ParserRouteIndex)
        .SequenceEqual(new[] { 0, 2, 3, 4, 5, 6 })
    && selector53Namespaces.All(snapshot => snapshot.ParserSelector
        == MatrixMessageNamespaceRoutingNativeLayout.CollisionSelector)
    && selector53Namespaces.Select(snapshot => snapshot.MessageRttiName).Distinct().Count() == 6
    && selector53Namespaces.Select(snapshot => snapshot.MessageVtableVirtualAddress).Distinct().Count() == 6
    && selector53Namespaces.Select(snapshot => snapshot.BaseMetadataStringVirtualAddress).Distinct().Count() == 6
    && selector53Namespaces.Any(snapshot =>
        snapshot.ParserRouteIndex == 2
        && snapshot.ParserCaseFileOffset == AreaVisualDataLootObjectCollectedNativeLayout.CaseFileOffset
        && snapshot.BaseMetadataStringVirtualAddress
            == AreaVisualDataLootObjectCollectedNativeLayout.BaseMetadataStringVirtualAddress
        && snapshot.MessageRttiName == AreaVisualDataLootObjectCollectedNativeLayout.RttiName)
    && MatrixMessageNamespaceRoutingNativeLayout.MessagePrimaryVtableFieldOffset == 0
    && MatrixMessageNamespaceRoutingNativeLayout.MessageReferenceCountFieldOffset == 0x08
    && MatrixMessageNamespaceRoutingNativeLayout.MessageMetadataPointerFieldOffset == 0x0C
    && MatrixMessageNamespaceRoutingNativeLayout.MessageSelectorFieldOffset == 0x10
    && MatrixMessageNamespaceRoutingNativeLayout.MessageFieldOffset14 == 0x14
    && MatrixMessageNamespaceRoutingNativeLayout.MessageEnvelopeKeyLowDwordFieldOffset == 0x18
    && MatrixMessageNamespaceRoutingNativeLayout.MessageEnvelopeKeyHighDwordFieldOffset == 0x1C
    && MatrixMessageNamespaceRoutingNativeLayout.MessageClearedDwordFieldOffset1 == 0x20
    && MatrixMessageNamespaceRoutingNativeLayout.MessageClearedDwordFieldOffset2 == 0x24
    && MatrixMessageNamespaceRoutingNativeLayout.BaseConstructorReferenceCountValue == 1
    && MatrixMessageNamespaceRoutingNativeLayout.EnvelopeKeyGateByteOffset == 0x18
    && MatrixMessageNamespaceRoutingNativeLayout.GetProtocolHandlerVtableSlotOffset(0x53) == 0xD0
    && MatrixMessageNamespaceRoutingNativeLayout.ProtocolHandlerObservedVtableGroupCount == 1
    && MatrixMessageNamespaceRoutingNativeLayout.ProtocolHandlerCompleteObjectOffset == 0
    && MatrixMessageNamespaceRoutingNativeLayout.ProtocolHandlerSecondBaseSubobjectOffset == 0
    && MatrixMessageNamespaceRoutingNativeLayout.ClassifyCallbackPath(
        0, false, true) == MatrixMessageCallbackDispatchPath.ProtocolHandlerPrimaryVtable
    && MatrixMessageNamespaceRoutingNativeLayout.ClassifyCallbackPath(
        1, false, true) == MatrixMessageCallbackDispatchPath.EnvelopeKeyRegistry
    && MatrixMessageNamespaceRoutingNativeLayout.ClassifyCallbackPath(
        0, true, true) == MatrixMessageCallbackDispatchPath.NestedMessageChildren
    && MatrixMessageNamespaceRoutingNativeLayout.ClassifyCallbackPath(
        0, false, false) == MatrixMessageCallbackDispatchPath.ProtocolHandlerUnavailable);
var envelopeRegistry = MatrixMessageNamespaceRoutingNativeLayout.EnvelopeKeyRegistry;
Check("Matrix envelope-QWORD registry lookup and insertion layout is recorded",
    envelopeRegistry.CallbackFileOffset == 0x7FDC60
    && envelopeRegistry.EnvelopeDispatcherFileOffset == 0x800420
    && envelopeRegistry.RegistryLookupFileOffset == 0x800070
    && envelopeRegistry.MessageKeyLowDwordFieldOffset == 0x18
    && envelopeRegistry.MessageKeyHighDwordFieldOffset == 0x1C
    && envelopeRegistry.FirstRegistryFieldOffset == 0x24
    && envelopeRegistry.SecondRegistryFieldOffset == 0x2C
    && envelopeRegistry.RegistryNodeKeyLowDwordFieldOffset == 0x10
    && envelopeRegistry.RegistryNodeKeyHighDwordFieldOffset == 0x14
    && envelopeRegistry.RegistryNodeValuePointerFieldOffset == 0x18
    && envelopeRegistry.RegistryTreeConstructorFileOffset == 0x805130
    && envelopeRegistry.RegistrySearchFileOffset == 0x8013B0
    && envelopeRegistry.RegistryEntryConstructorFileOffset == 0x806910
    && envelopeRegistry.RegistryInsertFileOffset == 0x8064C0
    && envelopeRegistry.DynamicFactoryFileOffset == 0x81B1B0
    && MatrixMessageNamespaceRoutingNativeLayout.EnvelopeRegistryDynamicFactoryByteDispatchFileOffset == 0x81B1DC
    && MatrixMessageNamespaceRoutingNativeLayout.EnvelopeRegistryDynamicFactoryVirtualAddress == 0xC1BDB0
    && envelopeRegistry.DynamicFactoryFirstByteMinimum == 2
    && envelopeRegistry.DynamicFactoryFirstByteMaximum == 0x35
    && envelopeRegistry.DirectProtocolHandlerDispatcherFileOffset == 0x81BB90
    && MatrixMessageNamespaceRoutingNativeLayout.EnvelopeRegistrySecondMapUpdateFileOffset == 0x800300
    && MatrixMessageNamespaceRoutingNativeLayout.EnvelopeRegistrySecondMapInsertCallFileOffset == 0x8003FF);
var keyedReceiverFirstEightCases = MatrixKeyedReceiverFactoryNativeLayout.FirstEightCases;
var keyedReceiverNextSixCases = MatrixKeyedReceiverFactoryNativeLayout.NextSixCases;
var keyedReceiverSelectors12To19Cases =
    MatrixKeyedReceiverFactoryNativeLayout.Selectors12To19ConstructingCases;
var keyedReceiverSelectors1ATo21Cases =
    MatrixKeyedReceiverFactoryNativeLayout.Selectors1ATo21ConstructingCases;
var keyedReceiverSelectors22To29Cases =
    MatrixKeyedReceiverFactoryNativeLayout.Selectors22To29ConstructingCases;
var keyedReceiverSelectors2ATo35Cases =
    MatrixKeyedReceiverFactoryNativeLayout.Selectors2ATo35ConstructingCases;
var keyedReceiverCases = keyedReceiverFirstEightCases
    .Concat(keyedReceiverNextSixCases)
    .Concat(keyedReceiverSelectors12To19Cases)
    .Concat(keyedReceiverSelectors1ATo21Cases)
    .Concat(keyedReceiverSelectors22To29Cases)
    .Concat(keyedReceiverSelectors2ATo35Cases)
    .ToArray();
var keyedReceiverSelectors0ATo11 = MatrixKeyedReceiverFactoryNativeLayout.Selectors0ATo11TableEntries;
var keyedReceiverSelectors12To19 = MatrixKeyedReceiverFactoryNativeLayout.Selectors12To19TableEntries;
var keyedReceiverSelectors1ATo21 = MatrixKeyedReceiverFactoryNativeLayout.Selectors1ATo21TableEntries;
var keyedReceiverSelectors22To29 = MatrixKeyedReceiverFactoryNativeLayout.Selectors22To29TableEntries;
var keyedReceiverSelectors2ATo35 = MatrixKeyedReceiverFactoryNativeLayout.Selectors2ATo35TableEntries;
var keyedReceiverSelectorValues = keyedReceiverFirstEightCases.Select(snapshot => snapshot.Selector)
    .Concat(keyedReceiverSelectors0ATo11.Select(entry => entry.Selector))
    .Concat(keyedReceiverSelectors12To19.Select(entry => entry.Selector))
    .Concat(keyedReceiverSelectors1ATo21.Select(entry => entry.Selector))
    .Concat(keyedReceiverSelectors22To29.Select(entry => entry.Selector))
    .Concat(keyedReceiverSelectors2ATo35.Select(entry => entry.Selector))
    .Order()
    .ToArray();
Check("Matrix keyed receiver factory bounds and thirty-six RTTI-backed receiver constructors are recorded",
    MatrixKeyedReceiverFactoryNativeLayout.FactoryFileOffset == 0x81B1B0
    && MatrixKeyedReceiverFactoryNativeLayout.FactoryVirtualAddress == 0xC1BDB0
    && MatrixKeyedReceiverFactoryNativeLayout.FactoryByteDispatchFileOffset == 0x81B1DC
    && MatrixKeyedReceiverFactoryNativeLayout.ReceiverRegistryFactoryArgumentSequenceFileOffset == 0x800148
    && MatrixKeyedReceiverFactoryNativeLayout.ReceiverRegistryFactoryInvocationFileOffset == 0x800151
    && MatrixKeyedReceiverFactoryNativeLayout.BaseControllerFactoryConstructorArgumentSetupFileOffset == 0x81B213
    && MatrixKeyedReceiverFactoryNativeLayout.BaseControllerFactoryConstructorCallFileOffset == 0x81B21B
    && MatrixKeyedReceiverFactoryNativeLayout.JumpTableFileOffset == 0x81BAA8
    && MatrixKeyedReceiverFactoryNativeLayout.JumpTableVirtualAddress == 0xC1C6A8
    && MatrixKeyedReceiverFactoryNativeLayout.JumpTableEntryCount == 0x34
    && MatrixKeyedReceiverFactoryNativeLayout.JumpTableEntrySize == 4
    && MatrixKeyedReceiverFactoryNativeLayout.FactoryIndexSubtract == 2
    && MatrixKeyedReceiverFactoryNativeLayout.FactoryIndexMaximum == 0x33
    && MatrixKeyedReceiverFactoryNativeLayout.FactorySelectorMinimum == 2
    && MatrixKeyedReceiverFactoryNativeLayout.FactorySelectorMaximum == 0x35
    && MatrixKeyedReceiverFactoryNativeLayout.EnvelopeKeyLowDwordFieldOffset == 0x18
    && MatrixKeyedReceiverFactoryNativeLayout.EnvelopeKeyHighDwordFieldOffset == 0x1C
    && MatrixKeyedReceiverFactoryNativeLayout.SharedBaseConstructorHelperFileOffset == 0xBCAD70
    && MatrixKeyedReceiverFactoryNativeLayout.SharedBaseVtableVirtualAddress == 0x1C660B4
    && keyedReceiverFirstEightCases.Count == 8
    && keyedReceiverNextSixCases.Count == 6
    && keyedReceiverSelectors12To19Cases.Count == 7
    && keyedReceiverSelectors1ATo21Cases.Count == 6
    && keyedReceiverSelectors22To29Cases.Count == 5
    && keyedReceiverSelectors2ATo35Cases.Count == 4
    && keyedReceiverCases.Length == 36
    && keyedReceiverSelectorValues.SequenceEqual(Enumerable.Range(2, 0x34))
    && keyedReceiverFirstEightCases.Select(snapshot => snapshot.Selector).SequenceEqual(Enumerable.Range(2, 8))
    && keyedReceiverCases.All(snapshot =>
        snapshot.CaseTargetVirtualAddress
            == snapshot.CaseTargetFileOffset
                + MatrixKeyedReceiverFactoryNativeLayout.RawTextOffsetToVirtualAddressDelta
        && snapshot.TableEntryFileOffset
            == MatrixKeyedReceiverFactoryNativeLayout.JumpTableFileOffset
                + (uint)((snapshot.Selector
                    - MatrixKeyedReceiverFactoryNativeLayout.FactorySelectorMinimum)
                    * MatrixKeyedReceiverFactoryNativeLayout.JumpTableEntrySize)
        && snapshot.AllocatorCallFileOffset == snapshot.CaseTargetFileOffset
            + (snapshot.AllocationSize <= 0x7F ? 2u : 5u)
        && snapshot.RttiName.Contains("@firefall@@", StringComparison.Ordinal)
        && snapshot.CompleteObjectLocatorSubobjectOffset == 0)
    && keyedReceiverFirstEightCases.Select(snapshot => snapshot.AllocationSize).SequenceEqual(
        [0xB80u, 0xD80u, 0x11D8u, 0x12C0u, 0x538u, 0xA0u, 0x4F8u, 0x420u])
    && keyedReceiverFirstEightCases.Select(snapshot => snapshot.ConstructorFileOffset).SequenceEqual(
        [0x53F980u, 0x54E7B0u, 0x55E830u, 0x56B970u, 0x577320u, 0x57FF80u, 0x581070u, 0x588830u])
    && keyedReceiverFirstEightCases.Select(snapshot => snapshot.PrimaryVtableVirtualAddress).Distinct().Count() == 8
    && keyedReceiverFirstEightCases.Select(snapshot => snapshot.PrimaryVtableFileOffset).SequenceEqual(
        [0x1864C80u, 0x1864B1Cu, 0x1864AF8u, 0x1864AD4u, 0x1864A8Cu, 0x1864AB0u, 0x1864A2Cu, 0x18649F4u])
    && keyedReceiverSelectors12To19Cases.Select(snapshot => snapshot.Selector)
        .SequenceEqual([0x12, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19])
    && keyedReceiverSelectors12To19Cases.Select(snapshot => snapshot.AllocationSize)
        .SequenceEqual([0x58u, 0x158u, 0x628u, 0x760u, 0x438u, 0x758u, 0x5C8u])
    && keyedReceiverSelectors12To19Cases.Select(snapshot => snapshot.ConstructorFileOffset)
        .SequenceEqual([0xADA400u, 0xADBCE0u, 0xAE10D0u, 0xAE42E0u, 0xAE9640u, 0xAEE240u, 0xAF1C00u])
    && keyedReceiverSelectors12To19Cases.Select(snapshot => snapshot.PrimaryVtableVirtualAddress)
        .SequenceEqual([0x1C7239Cu, 0x1C72378u, 0x1C72334u, 0x1C722ECu, 0x1C72310u, 0x1C722C8u, 0x1C722A4u])
    && keyedReceiverSelectors12To19Cases.Select(snapshot => snapshot.PrimaryVtableFileOffset)
        .SequenceEqual([0x1870F9Cu, 0x1870F78u, 0x1870F34u, 0x1870EECu, 0x1870F10u, 0x1870EC8u, 0x1870EA4u])
    && keyedReceiverSelectors12To19Cases.Select(snapshot => snapshot.RttiName).SequenceEqual(
        [
            ".?AVObserverView@MeldingBubble@firefall@@",
            ".?AVObserverView@AreaVisualData@firefall@@",
            ".?AVParticleEffectsView@AreaVisualData@firefall@@",
            ".?AVMapMarkerView@AreaVisualData@firefall@@",
            ".?AVTinyObjectView@AreaVisualData@firefall@@",
            ".?AVLootObjectView@AreaVisualData@firefall@@",
            ".?AVForceShieldView@AreaVisualData@firefall@@"
        ])
    && keyedReceiverSelectors12To19Cases.All(snapshot => snapshot.CompleteObjectLocatorSubobjectOffset == 0)
    && keyedReceiverSelectors1ATo21Cases.Select(snapshot => snapshot.Selector)
        .SequenceEqual([0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x21])
    && keyedReceiverSelectors1ATo21Cases.Select(snapshot => snapshot.AllocationSize)
        .SequenceEqual([0x570u, 0x1120u, 0x478u, 0x10F8u, 0xB8u, 0x78u])
    && keyedReceiverSelectors1ATo21Cases.Select(snapshot => snapshot.ConstructorFileOffset)
        .SequenceEqual([0xB0A990u, 0xB12490u, 0xB19380u, 0xB204A0u, 0xB26D40u, 0xAF6AE0u])
    && keyedReceiverSelectors1ATo21Cases.Select(snapshot => snapshot.PrimaryVtableVirtualAddress)
        .SequenceEqual([0x1C72868u, 0x1C72824u, 0x1C727E8u, 0x1C727C4u, 0x1C72784u, 0x1C72254u])
    && keyedReceiverSelectors1ATo21Cases.Select(snapshot => snapshot.PrimaryVtableFileOffset)
        .SequenceEqual([0x1871468u, 0x1871424u, 0x18713E8u, 0x18713C4u, 0x1871384u, 0x1870E54u])
    && keyedReceiverSelectors1ATo21Cases.Select(snapshot => snapshot.RttiName).SequenceEqual(
        [
            ".?AVBaseController@Vehicle@firefall@@",
            ".?AVCombatController@Vehicle@firefall@@",
            ".?AVObserverView@Vehicle@firefall@@",
            ".?AVCombatView@Vehicle@firefall@@",
            ".?AVMovementView@Vehicle@firefall@@",
            ".?AVAIObserverView@Anchor@firefall@@"
        ])
    && keyedReceiverSelectors1ATo21Cases.All(snapshot => snapshot.CompleteObjectLocatorSubobjectOffset == 0)
    && keyedReceiverCases.All(snapshot => snapshot.TypeDescriptorVirtualAddress != 0
        && snapshot.CompleteObjectLocatorVirtualAddress != 0));
Check("Matrix keyed receiver selectors 0A..11 map exact table indexes, cases, and default entries",
    keyedReceiverSelectors0ATo11.Count == 8
    && keyedReceiverSelectors0ATo11.Select(entry => entry.Selector).SequenceEqual(Enumerable.Range(0x0A, 8))
    && keyedReceiverSelectors0ATo11.Select(entry => entry.TableIndex).SequenceEqual(Enumerable.Range(8, 8))
    && keyedReceiverSelectors0ATo11.All(entry =>
        entry.TableIndex == entry.Selector - MatrixKeyedReceiverFactoryNativeLayout.FactoryIndexSubtract
        && entry.TableEntryFileOffset
            == MatrixKeyedReceiverFactoryNativeLayout.JumpTableFileOffset
                + (uint)(entry.TableIndex * MatrixKeyedReceiverFactoryNativeLayout.JumpTableEntrySize))
    && keyedReceiverSelectors0ATo11.Select(entry => entry.TargetFileOffset).SequenceEqual(
        [0x81B3E2u, 0x81B41Du, 0x81B45Bu, 0x81B499u, 0x81B4D7u, 0x81BA93u, 0x81B515u, 0x81BA93u])
    && keyedReceiverSelectors0ATo11.Select(entry => entry.ConstructsReceiver)
        .SequenceEqual([true, true, true, true, true, false, true, false])
    && keyedReceiverNextSixCases.Select(snapshot => snapshot.Selector)
        .SequenceEqual([0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x10])
    && keyedReceiverNextSixCases.Select(snapshot => snapshot.AllocationSize)
        .SequenceEqual([0x58u, 0x1258u, 0xA8u, 0x438u, 0x1F8u, 0x88u])
    && keyedReceiverNextSixCases.Select(snapshot => snapshot.ConstructorFileOffset)
        .SequenceEqual([0x58A530u, 0x58B5F0u, 0x595680u, 0x5962A0u, 0x59AEB0u, 0xAD9580u])
    && keyedReceiverNextSixCases.Select(snapshot => snapshot.PrimaryVtableVirtualAddress)
        .SequenceEqual([0x1C65DACu, 0x1C65D88u, 0x1C65D58u, 0x1C65D10u, 0x1C65D34u, 0x1C723D8u])
    && keyedReceiverNextSixCases.Select(snapshot => snapshot.PrimaryVtableFileOffset)
        .SequenceEqual([0x18649ACu, 0x1864988u, 0x1864958u, 0x1864910u, 0x1864934u, 0x1870FD8u])
    && keyedReceiverNextSixCases.Select(snapshot => snapshot.RttiName).SequenceEqual(
        [
            ".?AVAIObserverView@Character@firefall@@",
            ".?AVCombatView@Character@firefall@@",
            ".?AVMovementView@Character@firefall@@",
            ".?AVTinyObjectView@Character@firefall@@",
            ".?AVDynamicProjectileView@Character@firefall@@",
            ".?AVObserverView@Melding@firefall@@"
        ])
    && keyedReceiverNextSixCases.All(snapshot => snapshot.CompleteObjectLocatorSubobjectOffset == 0)
    && MatrixKeyedReceiverFactoryNativeLayout.FactorySelectorMinimum - 1
        - MatrixKeyedReceiverFactoryNativeLayout.FactoryIndexSubtract < 0
    && MatrixKeyedReceiverFactoryNativeLayout.FactorySelectorMaximum + 1
        - MatrixKeyedReceiverFactoryNativeLayout.FactoryIndexSubtract
        > MatrixKeyedReceiverFactoryNativeLayout.FactoryIndexMaximum);
Check("Matrix keyed receiver selectors 12..19 map exact table indexes and the shared zero-return entry",
    keyedReceiverSelectors12To19.Count == 8
    && keyedReceiverSelectors12To19.Select(entry => entry.Selector).SequenceEqual(Enumerable.Range(0x12, 8))
    && keyedReceiverSelectors12To19.Select(entry => entry.TableIndex).SequenceEqual(Enumerable.Range(16, 8))
    && keyedReceiverSelectors12To19.All(entry =>
        entry.TableIndex == entry.Selector - MatrixKeyedReceiverFactoryNativeLayout.FactoryIndexSubtract
        && entry.TableEntryFileOffset
            == MatrixKeyedReceiverFactoryNativeLayout.JumpTableFileOffset
                + (uint)(entry.TableIndex * MatrixKeyedReceiverFactoryNativeLayout.JumpTableEntrySize))
    && keyedReceiverSelectors12To19.Select(entry => entry.TargetFileOffset).SequenceEqual(
        [0x81B553u, 0x81BA93u, 0x81B58Eu, 0x81B5CCu, 0x81B60Au, 0x81B648u, 0x81B686u, 0x81B6C4u])
    && keyedReceiverSelectors12To19.Select(entry => entry.ConstructsReceiver)
        .SequenceEqual([true, false, true, true, true, true, true, true])
    && keyedReceiverSelectors12To19.All(entry =>
        entry.TargetVirtualAddress
            == entry.TargetFileOffset
                + MatrixKeyedReceiverFactoryNativeLayout.RawTextOffsetToVirtualAddressDelta)
    && keyedReceiverSelectors12To19.Count(entry => !entry.ConstructsReceiver) == 1
    && keyedReceiverSelectors12To19.Single(entry => entry.Selector == 0x13).TargetFileOffset
        == MatrixKeyedReceiverFactoryNativeLayout.OutOfRangeReturnFileOffset);
Check("Matrix keyed receiver selectors 1A..21 map exact table indexes, cases, and default entries",
    keyedReceiverSelectors1ATo21.Count == 8
    && keyedReceiverSelectors1ATo21.Select(entry => entry.Selector).SequenceEqual(Enumerable.Range(0x1A, 8))
    && keyedReceiverSelectors1ATo21.Select(entry => entry.TableIndex).SequenceEqual(Enumerable.Range(24, 8))
    && keyedReceiverSelectors1ATo21.All(entry =>
        entry.TableIndex == entry.Selector - MatrixKeyedReceiverFactoryNativeLayout.FactoryIndexSubtract
        && entry.TableEntryFileOffset
            == MatrixKeyedReceiverFactoryNativeLayout.JumpTableFileOffset
                + (uint)(entry.TableIndex * MatrixKeyedReceiverFactoryNativeLayout.JumpTableEntrySize))
    && keyedReceiverSelectors1ATo21.Select(entry => entry.TargetFileOffset).SequenceEqual(
        [0x81BA93u, 0x81B702u, 0x81B740u, 0x81B77Eu, 0x81B7BCu, 0x81B7FAu, 0x81BA93u, 0x81B838u])
    && keyedReceiverSelectors1ATo21.Select(entry => entry.ConstructsReceiver)
        .SequenceEqual([false, true, true, true, true, true, false, true])
    && keyedReceiverSelectors1ATo21.All(entry =>
        entry.TargetVirtualAddress
            == entry.TargetFileOffset
                + MatrixKeyedReceiverFactoryNativeLayout.RawTextOffsetToVirtualAddressDelta)
    && keyedReceiverSelectors1ATo21.Count(entry => !entry.ConstructsReceiver) == 2
    && keyedReceiverSelectors1ATo21.Where(entry => !entry.ConstructsReceiver)
        .All(entry => entry.TargetFileOffset == MatrixKeyedReceiverFactoryNativeLayout.OutOfRangeReturnFileOffset));
Check("Matrix keyed receiver selectors 2A..35 map exact table indexes, four constructors, and eight default entries",
    keyedReceiverSelectors2ATo35.Count == 12
    && keyedReceiverSelectors2ATo35.Select(entry => entry.Selector).SequenceEqual(Enumerable.Range(0x2A, 12))
    && keyedReceiverSelectors2ATo35.Select(entry => entry.TableIndex).SequenceEqual(Enumerable.Range(40, 12))
    && keyedReceiverSelectors2ATo35.All(entry =>
        entry.TableIndex == entry.Selector - MatrixKeyedReceiverFactoryNativeLayout.FactoryIndexSubtract
        && entry.TableEntryFileOffset
            == MatrixKeyedReceiverFactoryNativeLayout.JumpTableFileOffset
                + (uint)(entry.TableIndex * MatrixKeyedReceiverFactoryNativeLayout.JumpTableEntrySize))
    && keyedReceiverSelectors2ATo35.Select(entry => entry.TargetFileOffset).SequenceEqual(
        [
            0x81BA93u, 0x81BA93u, 0x81BA93u, 0x81B9A3u,
            0x81BA93u, 0x81BA93u, 0x81B9E1u, 0x81BA93u,
            0x81BA93u, 0x81BA1Fu, 0x81BA93u, 0x81BA59u
        ])
    && keyedReceiverSelectors2ATo35.Select(entry => entry.ConstructsReceiver)
        .SequenceEqual([false, false, false, true, false, false, true, false, false, true, false, true])
    && keyedReceiverSelectors2ATo35.Count(entry => !entry.ConstructsReceiver) == 8
    && keyedReceiverSelectors2ATo35.All(entry =>
        entry.TargetVirtualAddress
            == entry.TargetFileOffset
                + MatrixKeyedReceiverFactoryNativeLayout.RawTextOffsetToVirtualAddressDelta)
    && keyedReceiverSelectors2ATo35.Where(entry => !entry.ConstructsReceiver)
        .All(entry => entry.TargetFileOffset == MatrixKeyedReceiverFactoryNativeLayout.OutOfRangeReturnFileOffset));
Check("Matrix keyed receiver selectors 22..29 map exact table indexes, five constructors, and three default entries",
    keyedReceiverSelectors22To29.Count == 8
    && keyedReceiverSelectors22To29.Select(entry => entry.Selector).SequenceEqual(Enumerable.Range(0x22, 8))
    && keyedReceiverSelectors22To29.Select(entry => entry.TableIndex).SequenceEqual(Enumerable.Range(32, 8))
    && keyedReceiverSelectors22To29.All(entry =>
        entry.TableIndex == entry.Selector - MatrixKeyedReceiverFactoryNativeLayout.FactoryIndexSubtract
        && entry.TableEntryFileOffset
            == MatrixKeyedReceiverFactoryNativeLayout.JumpTableFileOffset
                + (uint)(entry.TableIndex * MatrixKeyedReceiverFactoryNativeLayout.JumpTableEntrySize))
    && keyedReceiverSelectors22To29.Select(entry => entry.TargetFileOffset).SequenceEqual(
        [0x81BA93u, 0x81B873u, 0x81B8B1u, 0x81B8ECu, 0x81BA93u, 0x81B927u, 0x81B965u, 0x81BA93u])
    && keyedReceiverSelectors22To29.Select(entry => entry.ConstructsReceiver)
        .SequenceEqual([false, true, true, true, false, true, true, false])
    && keyedReceiverSelectors22To29.Count(entry => !entry.ConstructsReceiver) == 3
    && keyedReceiverSelectors22To29.All(entry =>
        entry.TargetVirtualAddress
            == entry.TargetFileOffset
                + MatrixKeyedReceiverFactoryNativeLayout.RawTextOffsetToVirtualAddressDelta)
    && keyedReceiverSelectors22To29.Where(entry => !entry.ConstructsReceiver)
        .All(entry => entry.TargetFileOffset == MatrixKeyedReceiverFactoryNativeLayout.OutOfRangeReturnFileOffset)
    && keyedReceiverSelectors22To29Cases.Select(snapshot => snapshot.Selector)
        .SequenceEqual([0x23, 0x24, 0x25, 0x27, 0x28])
    && keyedReceiverSelectors22To29Cases.Select(snapshot => snapshot.AllocationSize)
        .SequenceEqual([0x1710u, 0x48u, 0x48u, 0x128u, 0xD8u])
    && keyedReceiverSelectors22To29Cases.Select(snapshot => snapshot.ConstructorFileOffset)
        .SequenceEqual([0xB281A0u, 0xB34FC0u, 0xB35CC0u, 0xB37030u, 0xB39900u])
    && keyedReceiverSelectors22To29Cases.Select(snapshot => snapshot.PrimaryVtableVirtualAddress)
        .SequenceEqual([0x1C72730u, 0x1C72700u, 0x1C726C4u, 0x1C72648u, 0x1C72618u])
    && keyedReceiverSelectors22To29Cases.Select(snapshot => snapshot.PrimaryVtableFileOffset)
        .SequenceEqual([0x1871330u, 0x1871300u, 0x18712C4u, 0x1871248u, 0x1871218u])
    && keyedReceiverSelectors22To29Cases.Select(snapshot => snapshot.RttiName).SequenceEqual(
        [
            ".?AVObserverView@Deployable@firefall@@",
            ".?AVNPCObserverView@Deployable@firefall@@",
            ".?AVHardpointView@Deployable@firefall@@",
            ".?AVBaseController@Turret@firefall@@",
            ".?AVObserverView@Turret@firefall@@"
        ])
    && keyedReceiverSelectors22To29Cases.All(snapshot => snapshot.CompleteObjectLocatorSubobjectOffset == 0)
    && keyedReceiverSelectors2ATo35Cases.Select(snapshot => snapshot.Selector)
        .SequenceEqual([0x2D, 0x30, 0x33, 0x35])
    && keyedReceiverSelectors2ATo35Cases.Select(snapshot => snapshot.AllocationSize)
        .SequenceEqual([0x100u, 0x11A0u, 0x1690u, 0x758u])
    && keyedReceiverSelectors2ATo35Cases.Select(snapshot => snapshot.ConstructorCallFileOffset)
        .SequenceEqual([0x81B9CCu, 0x81BA0Au, 0x81BA44u, 0x81BA7Eu])
    && keyedReceiverSelectors2ATo35Cases.Select(snapshot => snapshot.ConstructorFileOffset)
        .SequenceEqual([0xAF7300u, 0xB3C0F0u, 0xAFC100u, 0xB43B10u])
    && keyedReceiverSelectors2ATo35Cases.Select(snapshot => snapshot.PrimaryVtableVirtualAddress)
        .SequenceEqual([0x1C72278u, 0x1C725E4u, 0x1C721E0u, 0x1C72574u])
    && keyedReceiverSelectors2ATo35Cases.Select(snapshot => snapshot.PrimaryVtableFileOffset)
        .SequenceEqual([0x1870E78u, 0x18711E4u, 0x1870DE0u, 0x1871174u])
    && keyedReceiverSelectors2ATo35Cases.Select(snapshot => snapshot.CompleteObjectLocatorVirtualAddress)
        .SequenceEqual([0x1CCA910u, 0x1CCBA18u, 0x1CCA8A0u, 0x1CCBA04u])
    && keyedReceiverSelectors2ATo35Cases.Select(snapshot => snapshot.TypeDescriptorVirtualAddress)
        .SequenceEqual([0x1E29EC0u, 0x1E2A3F0u, 0x1E29EECu, 0x1E2A3B4u])
    && keyedReceiverSelectors2ATo35Cases.Select(snapshot => snapshot.RttiName).SequenceEqual(
        [
            ".?AVObserverView@Outpost@firefall@@",
            ".?AVObserverView@ResourceNode@firefall@@",
            ".?AVObserverView@CarryableObject@firefall@@",
            ".?AVLootObjectView@LootStoreExtension@firefall@@"
        ])
    && keyedReceiverSelectors2ATo35Cases.All(snapshot => snapshot.CompleteObjectLocatorSubobjectOffset == 0));
Check("Matrix keyed receiver selectors 02..35 total thirty-six constructors, sixteen zero returns, and complete coverage",
    keyedReceiverCases.Length == 36
    && keyedReceiverSelectors0ATo11.Count(entry => !entry.ConstructsReceiver)
        + keyedReceiverSelectors12To19.Count(entry => !entry.ConstructsReceiver)
        + keyedReceiverSelectors1ATo21.Count(entry => !entry.ConstructsReceiver)
        + keyedReceiverSelectors22To29.Count(entry => !entry.ConstructsReceiver)
        + keyedReceiverSelectors2ATo35.Count(entry => !entry.ConstructsReceiver) == 16
    && keyedReceiverSelectorValues.SequenceEqual(Enumerable.Range(2, 0x34))
    && MatrixKeyedReceiverFactoryNativeLayout.FactorySelectorMaximum == 0x35);
var keyedReceiverSlots = MatrixKeyedReceiverFactoryNativeLayout.PrimaryVtableSlots;
Check("Matrix keyed receiver selector routes distinguish message dispatchers, forwarding defaults, and setup wrappers",
    keyedReceiverSlots.Count == keyedReceiverCases.Length * 4
    && keyedReceiverCases.All(receiverCase => keyedReceiverSlots
        .Where(slot => slot.FactorySelector == receiverCase.Selector)
        .Select(slot => slot.SlotOffset)
        .SequenceEqual([0x04, 0x08, 0x0C, 0x18]))
    && keyedReceiverSlots.All(slot =>
        slot.MethodVirtualAddress
            == slot.MethodFileOffset
                + MatrixKeyedReceiverFactoryNativeLayout.RawTextOffsetToVirtualAddressDelta)
    && keyedReceiverSlots.Where(slot => slot.SlotOffset == 0x18)
        .All(slot => slot.ImplementationObservation.Contains("State-refresh wrapper", StringComparison.Ordinal))
    && keyedReceiverSlots.Count(slot => slot.SlotOffset == 0x04
        && slot.ImplementationObservation.Contains("Message-ID dispatcher", StringComparison.Ordinal)) == 19
    && keyedReceiverSlots.Count(slot => slot.SlotOffset == 0x04
        && slot.ImplementationObservation.Contains("Optional nested-receiver forwarder", StringComparison.Ordinal)) == 17
    && keyedReceiverSlots.Where(slot =>
            (slot.FactorySelector is 0x0D or 0x0E or 0x10 or 0x12 or 0x16 or 0x17 or 0x19 or 0x21
                or 0x25 or 0x27 or 0x2D or 0x30 or 0x33)
            && slot.SlotOffset == 0x04)
        .All(slot => slot.ImplementationObservation.Contains(
            "fixed 'this view doesn't have any valid events' log", StringComparison.Ordinal)
            && !slot.ImplementationObservation.Contains("unhandled-message-ID", StringComparison.Ordinal))
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x1B && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("exactly 0x5B", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x1C && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("0x53..0x5A", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x1D && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("exactly 0x55", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x1E && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("exactly 0x57", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x1F && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("exactly 0x56", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x0A && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("0x69", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x0B && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("0x54..0x6C", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x0C && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("0x6E", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x14 && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("exactly 0x54", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x15 && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("exactly 0x55", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x18 && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("exactly 0x53", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x23 && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("0x54 and 0x55", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x24 && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("exactly 0x53", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x28 && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("exactly 0x53", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x35 && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("exactly 0x53", StringComparison.Ordinal)
    && keyedReceiverSlots.Single(slot => slot.FactorySelector == 0x35 && slot.SlotOffset == 0x04)
        .ImplementationObservation.Contains("VIBE_LogUnhandledMessageId", StringComparison.Ordinal)
    && keyedReceiverSlots.Where(slot =>
            (slot.FactorySelector is 0x12 or 0x14 or 0x15 or 0x16 or 0x17 or 0x18 or 0x19
                or 0x1B or 0x1C or 0x1D or 0x1E or 0x1F or 0x21
                or 0x23 or 0x24 or 0x25 or 0x27 or 0x28
                or 0x2D or 0x30 or 0x33 or 0x35)
            && slot.SlotOffset == 0x18)
        .All(slot => slot.ImplementationObservation.Contains(
            "stores the dword_1AC4D24 result at receiver +0x28 and calls 0xBCADF0",
            StringComparison.Ordinal))
    && keyedReceiverSlots.Where(slot =>
            (slot.FactorySelector is 0x12 or 0x14 or 0x15 or 0x16 or 0x17 or 0x18 or 0x19
                or 0x1B or 0x1C or 0x1D or 0x1E or 0x1F or 0x21
                or 0x23 or 0x24 or 0x25 or 0x27 or 0x28
                or 0x2D or 0x30 or 0x33 or 0x35)
            && (slot.SlotOffset is 0x08 or 0x0C))
        .All(slot => slot.ImplementationObservation.Contains(
                "pointer and length", StringComparison.Ordinal)
            && slot.ImplementationObservation.Contains("body not exhaustively mapped", StringComparison.Ordinal))
    && MatrixKeyedReceiverFactoryNativeLayout.FactorySelectorMaximum == 0x35
    && MatrixKeyedReceiverFactoryNativeLayout.CompactSelectorDispatcherFileOffset == 0x800420
    && MatrixKeyedReceiverFactoryNativeLayout.CompactMessageSelectorFieldOffset == 0x10
    && MatrixKeyedReceiverFactoryNativeLayout.CompactSelectorRoutes.Select(route => route.Selector)
        .SequenceEqual(Enumerable.Range(1, 9))
    && MatrixKeyedReceiverFactoryNativeLayout.CompactSelectorRoutes
        .Where(route => route.ReceiverVtableSlotOffset is not null)
        .Select(route => (route.Selector, route.ReceiverVtableSlotOffset))
        .SequenceEqual(
        [
            (1, (int?)0x08),
            (2, 0x18),
            (3, 0x0C),
            (4, 0x0C),
            (7, 0x04),
            (8, 0x04),
            (9, 0x18)
        ])
    && MatrixKeyedReceiverFactoryNativeLayout.CompactSelectorRoutes
        .Where(route => route.DirectHelperFileOffset is not null)
        .Select(route => route.Selector).SequenceEqual([5, 6])
    && keyedReceiverSlots.Where(slot => slot.FactorySelector == 0x03 && slot.SlotOffset == 0x0C)
        .Single().MethodFileOffset == 0x5536C0
    && keyedReceiverSlots.Where(slot => slot.FactorySelector == 0x06 && slot.SlotOffset == 0x0C)
        .Single().MethodFileOffset == 0x5795A0);
var keyedCharacterDialogCases = CharacterKeyedDialogDispatchNativeLayout.Cases;
var keyedCharacterBaseDialogCases = keyedCharacterDialogCases
    .Where(snapshot => snapshot.FactorySelector == 0x02)
    .ToArray();
var keyedCharacterNpcDialogCases = keyedCharacterDialogCases
    .Where(snapshot => snapshot.FactorySelector == 0x03)
    .ToArray();
Check("Keyed Character receiver dialog IDs map through their actual BaseController and NPCController slots",
    CharacterKeyedDialogDispatchNativeLayout.RawTextOffsetToVirtualAddressDelta
        == MatrixKeyedReceiverFactoryNativeLayout.RawTextOffsetToVirtualAddressDelta
    && MatrixKeyedReceiverFactoryNativeLayout.FirstEightCases
        .Single(snapshot => snapshot.Selector == 0x02).RttiName
        == ".?AVBaseController@Character@firefall@@"
    && MatrixKeyedReceiverFactoryNativeLayout.FirstEightCases
        .Single(snapshot => snapshot.Selector == 0x02).PrimaryVtableVirtualAddress
        == CharacterKeyedDialogDispatchNativeLayout.BaseControllerPrimaryVtableVirtualAddress
    && MatrixKeyedReceiverFactoryNativeLayout.FirstEightCases
        .Single(snapshot => snapshot.Selector == 0x03).RttiName
        == ".?AVNPCController@Character@firefall@@"
    && MatrixKeyedReceiverFactoryNativeLayout.FirstEightCases
        .Single(snapshot => snapshot.Selector == 0x03).PrimaryVtableVirtualAddress
        == CharacterKeyedDialogDispatchNativeLayout.NpcControllerPrimaryVtableVirtualAddress
    && MatrixKeyedReceiverFactoryNativeLayout.PrimaryVtableSlots.Single(slot =>
        slot.FactorySelector == 0x02 && slot.SlotOffset == 0x04).MethodFileOffset
        == CharacterKeyedDialogDispatchNativeLayout.BaseControllerMessageDispatcherFileOffset
    && MatrixKeyedReceiverFactoryNativeLayout.PrimaryVtableSlots.Single(slot =>
        slot.FactorySelector == 0x03 && slot.SlotOffset == 0x04).MethodFileOffset
        == CharacterKeyedDialogDispatchNativeLayout.NpcControllerMessageDispatcherFileOffset
    && CharacterKeyedDialogDispatchNativeLayout.MessageIdFieldOffset == 0x10
    && CharacterKeyedDialogDispatchNativeLayout.MessageForwardedToNestedReceiverFieldOffset == 0x24
    && CharacterKeyedDialogDispatchNativeLayout.ReceiverOptionalNestedObjectFieldOffset == 0x10
    && CharacterKeyedDialogDispatchNativeLayout.ReceiverOptionalCallbackObjectPointerFieldOffset == 0x2C
    && CharacterKeyedDialogDispatchNativeLayout.CallbackObjectPointerLoadRelativeOffset == 0x1A
    && CharacterKeyedDialogDispatchNativeLayout.CallbackMessagePointerPushRelativeOffset == 0x1D
    && CharacterKeyedDialogDispatchNativeLayout.CallbackObjectVtableLoadRelativeOffset == 0x1E
    && CharacterKeyedDialogDispatchNativeLayout.CallbackVirtualCallRelativeOffset == 0x20
    && CharacterKeyedDialogDispatchNativeLayout.BaseControllerCallbackObjectNullStoreFileOffset == 0x53F9D2
    && CharacterKeyedDialogDispatchNativeLayout.NpcControllerCallbackObjectNullStoreFileOffset == 0x54E802
    && CharacterKeyedDialogDispatchNativeLayout.BaseControllerMessagePointerLoadFileOffset == 0x540390
    && CharacterKeyedDialogDispatchNativeLayout.NpcControllerMessagePointerLoadFileOffset == 0x54F4B0
    && CharacterKeyedDialogDispatchNativeLayout.BaseControllerNestedReceiverLoadFileOffset == 0x54038D
    && CharacterKeyedDialogDispatchNativeLayout.NpcControllerNestedReceiverLoadFileOffset == 0x54F4AD
    && CharacterKeyedDialogDispatchNativeLayout.BaseControllerPayloadObjectPointerReadFileOffset == 0x544288
    && CharacterKeyedDialogDispatchNativeLayout.NpcControllerPayloadObjectPointerReadFileOffset == 0x553798
    && CharacterKeyedDialogDispatchNativeLayout.ReceiverLoggerContextFieldOffset == 0x30
    && CharacterKeyedDialogDispatchNativeLayout.DispatchMessageIdMinimum == 0x53
    && CharacterKeyedDialogDispatchNativeLayout.DispatchMessageIdMaximum == 0xC4
    && keyedCharacterDialogCases.Count == 8
    && keyedCharacterBaseDialogCases.Select(snapshot => snapshot.MessageId)
        .SequenceEqual(Enumerable.Range(0x77, 4))
    && keyedCharacterNpcDialogCases.Select(snapshot => snapshot.MessageId)
        .SequenceEqual(Enumerable.Range(0x77, 4))
    && keyedCharacterBaseDialogCases.Select(snapshot => snapshot.SelectorMapValue)
        .SequenceEqual(new byte[] { 0x08, 0x09, 0x0A, 0x4B })
    && keyedCharacterNpcDialogCases.Select(snapshot => snapshot.SelectorMapValue)
        .SequenceEqual(new byte[] { 0x0A, 0x0B, 0x4E, 0x4E })
    && keyedCharacterBaseDialogCases.Select(snapshot => snapshot.HasSpecificHandler)
        .SequenceEqual([true, true, true, false])
    && keyedCharacterNpcDialogCases.Select(snapshot => snapshot.HasSpecificHandler)
        .SequenceEqual([true, true, false, false])
    && keyedCharacterDialogCases.All(snapshot =>
        snapshot.CaseTargetVirtualAddress
            == snapshot.CaseTargetFileOffset
                + CharacterKeyedDialogDispatchNativeLayout.RawTextOffsetToVirtualAddressDelta
        && !snapshot.ReadsMessagePayload)
    && keyedCharacterBaseDialogCases.Where(snapshot => snapshot.HasSpecificHandler)
        .Select(snapshot => snapshot.CallbackVtableSlotOffset)
        .SequenceEqual([0x10, 0x14, 0x18])
    && keyedCharacterNpcDialogCases.Where(snapshot => snapshot.HasSpecificHandler)
        .Select(snapshot => snapshot.CallbackVtableSlotOffset)
        .SequenceEqual([0x14, 0x18])
    && keyedCharacterBaseDialogCases.Where(snapshot => snapshot.HasSpecificHandler)
        .All(snapshot => snapshot.ForwardsOriginalMessagePointer
            && snapshot.CallbackObjectFieldOffset == 0x2C
            && snapshot.LoggerContextFieldOffset == 0x30)
    && keyedCharacterNpcDialogCases.Where(snapshot => snapshot.HasSpecificHandler)
        .All(snapshot => snapshot.ForwardsOriginalMessagePointer
            && snapshot.CallbackObjectFieldOffset == 0x2C
            && snapshot.LoggerContextFieldOffset == 0x30)
    && CharacterKeyedDialogDispatchNativeLayout.OuterEnvelopeDispatcherFileOffset == 0x800420
    && CharacterKeyedDialogDispatchNativeLayout.OuterSelectorDefaultBranchFileOffset == 0x800697
    && CharacterKeyedDialogDispatchNativeLayout.OuterSelectorDefaultReceiverCallFileOffset == 0x8008D3);
var keyedCharacterBaseAdapterSlots =
    CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterVtableSlots;
var keyedCharacterSelectorThreeAdapterSlots =
    CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterVtableSlots;
Check("Keyed Character selector-2 state factory metadata identifies a separate tfGenericCharacter state",
    CharacterKeyedDialogCallbackNativeLayout.RawTextOffsetToVirtualAddressDelta
        == CharacterKeyedDialogDispatchNativeLayout.RawTextOffsetToVirtualAddressDelta
    && CharacterKeyedDialogCallbackNativeLayout.GenericCharacterRttiName == ".?AVtfGenericCharacter@@"
    && CharacterKeyedDialogCallbackNativeLayout.GenericCharacterPrimaryVtableVirtualAddress == 0x1C63998
    && CharacterKeyedDialogCallbackNativeLayout.GenericCharacterPrimaryVtableFileOffset == 0x1862598
    && CharacterKeyedDialogCallbackNativeLayout.GenericCharacterAllocationSize == 0x5AE0
    && CharacterKeyedDialogCallbackNativeLayout.GenericCharacterConstructorFileOffset == 0x422E80
    && CharacterKeyedDialogCallbackNativeLayout.GenericCharacterVtableInstallFileOffset == 0x422FEA
    && CharacterKeyedDialogCallbackNativeLayout.GenericCharacterSetupDispatcherFileOffset == 0x4263C0
    && CharacterKeyedDialogCallbackNativeLayout.GenericCharacterSetupMethodFileOffset == 0x426840
    && CharacterKeyedDialogCallbackNativeLayout.GenericCharacterSetupSelectorTwoCaseFileOffset == 0x426436
    && CharacterKeyedDialogCallbackNativeLayout.SetupPrivateContextAllocationCallFileOffset == 0x426881
    && CharacterKeyedDialogCallbackNativeLayout.SetupPrivateContextAllocationSize == 0x43D8
    && CharacterKeyedDialogCallbackNativeLayout.SetupPrivateContextConstructorCallFileOffset == 0x4268A1
    && CharacterKeyedDialogCallbackNativeLayout.SetupPrivateContextConstructorFileOffset == 0x49EB30
    && CharacterKeyedDialogCallbackNativeLayout.SetupPrivateContextStateFieldOffset == 0x4D20
    && CharacterKeyedDialogCallbackNativeLayout.SetupPrivateContextNullPointerInitializationFileOffset == 0x49EBE9
    && CharacterKeyedDialogCallbackNativeLayout.SetupIndirectSourceObjectPointerFieldOffset == 0x20
    && CharacterKeyedDialogCallbackNativeLayout.SetupIndirectChildObjectPointerFieldOffset == 0x9C
    && CharacterKeyedDialogCallbackNativeLayout.SetupIndirectBackPointerFieldOffset == 0x2C
    && CharacterKeyedDialogCallbackNativeLayout.SetupIndirectBackPointerDataflowSequenceFileOffset == 0x4A2EFE
    && CharacterKeyedDialogCallbackNativeLayout.SetupIndirectBackPointerStoreFileOffset == 0x4A2F07
    && CharacterKeyedDialogCallbackNativeLayout.StateFactoryMethodFileOffset == 0xAC9350
    && CharacterKeyedDialogCallbackNativeLayout.StateFactorySelectorMapFileOffset == 0xAC9704
    && CharacterKeyedDialogCallbackNativeLayout.StateFactoryJumpTableFileOffset == 0xAC96DC
    && CharacterKeyedDialogCallbackNativeLayout.GenericCharacterStateFactoryBranchFileOffset == 0xAC9483
    && CharacterKeyedDialogCallbackNativeLayout.UnsupportedNpcStateFactoryBranchFileOffset == 0xAC96A8
    && CharacterKeyedDialogCallbackNativeLayout.GenericCharacterSetupDispatcherFileOffset == 0x4263C0
    && CharacterKeyedDialogCallbackNativeLayout.GenericCharacterSetupMethodFileOffset == 0x426840
    && CharacterKeyedDialogCallbackNativeLayout.StateFactorySetupVirtualCallFileOffset == 0x800A7C);
Check("Selector-2 BaseController +0x2C binding resolves to its constructed BaseNetController adapter",
    CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterAllocationFileOffset == 0x4A182C
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterAllocationSize == 0x228
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterConstructorCallFileOffset == 0x4A1853
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterConstructorFileOffset == 0x4C66D0
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterPrivateContextFieldStoreFileOffset == 0x4C6714
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterPrivateContextFieldOffset == 0x170
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterVtableStoreFileOffset == 0x4C671D
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterControllerFieldOffset == 0x174
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterControllerFieldStoreFileOffset == 0x4C672D
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterVtableVirtualAddress == 0x1C64648
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterVtableFileOffset == 0x1863248
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterRttiName
        == ".?AVBaseNetController@tfLocalCharacter@@"
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterBindingMethodFileOffset == 0x4C7360
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterControllerCallbackPointerStoreFileOffset
        == 0x4C7393
    && CharacterKeyedDialogCallbackNativeLayout.SelectorTwoAdapterControllerContextPointerStoreFileOffset
        == 0x4C73A5
    && keyedCharacterBaseAdapterSlots.Select(slot => slot.SlotOffset)
        .SequenceEqual([0x10, 0x14, 0x18])
    && keyedCharacterBaseAdapterSlots.Select(slot => slot.MethodFileOffset)
        .SequenceEqual([0x4C82E0u, 0x4C8300u, 0x4C9B80u])
    && keyedCharacterBaseAdapterSlots.All(slot =>
        slot.MethodVirtualAddress
            == slot.MethodFileOffset
                + CharacterKeyedDialogCallbackNativeLayout.RawTextOffsetToVirtualAddressDelta));
var localControllerDialogCases = CharacterKeyedDialogDispatchNativeLayout.Cases
    .Where(snapshot => snapshot.FactorySelector == 0x02
        && snapshot.MessageId is >= 0x77 and <= 0x79)
    .ToArray();
Check("BaseNetController callback targets match the exact BaseController message-ID case slots",
    CharacterLocalControllerApplicationNativeLayout.Callbacks.SequenceEqual(
    [
        new CharacterLocalControllerCallbackSnapshot(0x77, 0x10, 0x540446, 0x4C82E0),
        new CharacterLocalControllerCallbackSnapshot(0x78, 0x14, 0x54046F, 0x4C8300),
        new CharacterLocalControllerCallbackSnapshot(0x79, 0x18, 0x540498, 0x4C9B80)
    ])
    && localControllerDialogCases.Select(snapshot =>
            (snapshot.MessageId, snapshot.CallbackVtableSlotOffset ?? -1, snapshot.CaseTargetFileOffset))
        .SequenceEqual(CharacterLocalControllerApplicationNativeLayout.Callbacks.Select(callback =>
            (callback.MessageId, callback.VtableSlotOffset, callback.DispatchCaseFileOffset)))
    && CharacterLocalControllerApplicationNativeLayout.RawTextOffsetToVirtualAddressDelta
        == CharacterKeyedDialogCallbackNativeLayout.RawTextOffsetToVirtualAddressDelta
    && CharacterLocalControllerApplicationNativeLayout.Callbacks.Select(callback =>
            callback.CallbackFileOffset
                + CharacterLocalControllerApplicationNativeLayout.RawTextOffsetToVirtualAddressDelta)
        .SequenceEqual(keyedCharacterBaseAdapterSlots.Select(slot => slot.MethodVirtualAddress))
    && CharacterLocalControllerApplicationNativeLayout.AdapterAllocationSize == 0x228
    && CharacterLocalControllerApplicationNativeLayout.AdapterConstructorFileOffset == 0x4C66D0
    && CharacterLocalControllerApplicationNativeLayout.AdapterContextPointerOffset == 0x170
    && CharacterLocalControllerApplicationNativeLayout.AdapterKeyedReceiverPointerOffset == 0x174
    && CharacterLocalControllerApplicationNativeLayout.AdapterConstructorZeroedDwordOffset == 0x16C
    && CharacterLocalControllerApplicationNativeLayout.PrivateContextAdapterPointerOffset == 0x18
    && CharacterLocalControllerApplicationNativeLayout.PrivateContextPointerSourceOffset == 4
    && CharacterLocalControllerApplicationNativeLayout.AdapterBindingMethodFileOffset == 0x4C7360
    && CharacterLocalControllerApplicationNativeLayout.AdapterReceiverPointerLoadFileOffset == 0x4C738D
    && CharacterLocalControllerApplicationNativeLayout.KeyedReceiverCallbackObjectPointerOffset == 0x2C
    && CharacterLocalControllerApplicationNativeLayout.KeyedReceiverContextPointerOffset == 0x24);
Check("Bound BaseNetController callback observations retain only evidenced message offsets and numeric effects",
    CharacterLocalControllerApplicationNativeLayout.Message77UnhandledLoggerFileOffset == 0x2FC4F0
    && CharacterLocalControllerApplicationNativeLayout.Message78SharedTextFieldOffset == 0x28
    && CharacterLocalControllerApplicationNativeLayout.Message78EventSinkGlobalVirtualAddress == 0x1E002B8
    && CharacterLocalControllerApplicationNativeLayout.Message78EventSinkVtableOffset == 0xAC
    && CharacterLocalControllerApplicationNativeLayout.Message78NumericEventId == 285
    && CharacterLocalControllerApplicationNativeLayout.Message78EventDispatchFileOffset == 0x4C83BA
    && CharacterLocalControllerApplicationNativeLayout.Message79RegistryAccessorFileOffset == 0x35E700
    && CharacterLocalControllerApplicationNativeLayout.Message79RegistryMapOffsetFromAccessorResult == 0x54
    && CharacterLocalControllerApplicationNativeLayout.Message79RegistryLookupFileOffset == 0x4093E0
    && CharacterLocalControllerApplicationNativeLayout.Message79KeyFirstDwordOffset == 0x30
    && CharacterLocalControllerApplicationNativeLayout.Message79KeyFirstDwordMask == 0xFFFFFF00
    && CharacterLocalControllerApplicationNativeLayout.Message79KeySecondDwordOffset == 0x34
    && CharacterLocalControllerApplicationNativeLayout.Message79OpaqueMessageFieldOffset == 0x28
    && CharacterLocalControllerApplicationNativeLayout.Message79LookupValueFieldOffset == 0x38
    && CharacterLocalControllerApplicationNativeLayout.Message79RecipientGateByteOffset == 8
    && CharacterLocalControllerApplicationNativeLayout.Message79RegistryEntryRecipientPointerOffset == 0x10
    && CharacterLocalControllerApplicationNativeLayout.Message79KeyedUpdateFileOffset == 0x466660
    && CharacterLocalControllerApplicationNativeLayout.Message79RecipientFallbackFileOffset == 0x439A50
    && CharacterLocalControllerApplicationNativeLayout.Message79RecipientConditionalStatePresenceByteOffset == 0x4CA8
    && CharacterLocalControllerApplicationNativeLayout.Message79RecipientConditionalStateFirstDwordOffset == 0x4CB0
    && CharacterLocalControllerApplicationNativeLayout.Message79RecipientConditionalStateLastDwordOffset == 0x4CC4
    && CharacterLocalControllerApplicationNativeLayout.Message79RecipientHelperFieldOffset == 0x4CFC
    && CharacterLocalControllerApplicationNativeLayout.Message79RecipientDescriptorDwordFirstOffset == 0x4D00
    && CharacterLocalControllerApplicationNativeLayout.Message79RecipientDescriptorDwordLastOffset == 0x4D0C
    && CharacterLocalControllerApplicationNativeLayout.Message79RecipientAuxiliaryByteOffset == 0x4D10
    && CharacterLocalControllerApplicationNativeLayout.Message79RecipientArgumentOffsetPassedToAuxiliarySink == 0x58A0
    && CharacterLocalControllerApplicationNativeLayout.Message79RecipientObjectPointerLoadOffset == 0x46A4
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackConditionalSourcePresenceByteOffset == 0x24
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackLookupRecordAuxiliaryGateDwordOffset == 0x08
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackLookupRecordAuxiliaryValueDwordOffset == 0x10
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackLookupRecordDescriptorDwordOffset == 0x14
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackRecordConditionalPresenceByteOffset == 0x4CA8
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackRecordFirstDwordOffset == 0x4CB0
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackRecordDwordCount == 6
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackRecordLastDwordOffset == 0x4CC4
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackDescriptorTextOffset == 0x4CFC
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackDescriptorStateDwordOffset == 0x4D00
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackDescriptorFirstValueDwordOffset == 0x4D04
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackDescriptorLastValueDwordOffset == 0x4D0C
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackDescriptorByteOffset == 0x4D10
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackDescriptorControlByteOffset == 0x4C70
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackDescriptorClearMask == 0x08
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackDescriptorSetMask == 0x10
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackStateUpdateFileOffset == 0x457D30
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackDescriptorDispatchFileOffset == 0x439380
    && CharacterLocalControllerApplicationNativeLayout.Message79FallbackDescriptorHelperFileOffset == 0x439410);
var oldConditionalPayload = new CharacterKeyedSixDwordPayload(
    0x10101010, 0x20202020, 0x30303030, 0x40404040, 0x50505050, 0x60606060);
var copiedConditionalPayload = new CharacterKeyedSixDwordPayload(
    0x11111111, 0x22222222, 0x33333333, 0x44444444, 0x55555555, 0x66666666);
var conditionalRecord = new CharacterKeyedConditionalRecordSnapshot(0, oldConditionalPayload);
Check("Message-0x79 fallback leaves the six-DWORD record untouched when the source presence byte is zero",
    CharacterKeyedRecipientFallbackModel.ApplyConditionalRecord(
        conditionalRecord, new CharacterKeyedConditionalRecordInput(0, copiedConditionalPayload))
        == conditionalRecord);
var firstConditionalCopy = CharacterKeyedRecipientFallbackModel.ApplyConditionalRecord(
    conditionalRecord, new CharacterKeyedConditionalRecordInput(1, copiedConditionalPayload));
Check("Message-0x79 fallback copies six ordered DWORDs and sets an absent destination marker",
    firstConditionalCopy
        == new CharacterKeyedConditionalRecordSnapshot(1, copiedConditionalPayload));
var populatedConditionalCopy = CharacterKeyedRecipientFallbackModel.ApplyConditionalRecord(
    new CharacterKeyedConditionalRecordSnapshot(0x7F, oldConditionalPayload),
    new CharacterKeyedConditionalRecordInput(0x80, copiedConditionalPayload));
Check("Message-0x79 fallback preserves a nonzero destination marker while replacing all six DWORDs",
    populatedConditionalCopy
        == new CharacterKeyedConditionalRecordSnapshot(0x7F, copiedConditionalPayload));
var descriptorCurrent = new CharacterKeyedDescriptorSnapshot(
    "old", 0xA0A0A0A0, new CharacterKeyedThreeDwordPayload(1, 2, 3), 0x44, 0x55);
var descriptorInput = new CharacterKeyedDescriptorInput(
    "selected", 0x18, new CharacterKeyedThreeDwordPayload(0x11112222, 0x33334444, 0x55556666), 0x77);
var descriptorCleared = CharacterKeyedRecipientFallbackModel.ApplyDescriptorFields(
    descriptorCurrent, descriptorInput);
Check("Message-0x79 descriptor update preserves +0x4D10 without a 0x02 transition and clear bit wins for +0x4C70",
    descriptorCleared
        == new CharacterKeyedDescriptorSnapshot(
            "selected", 0x18, descriptorInput.ResolvedDwordsAt4D04, 0x44, 0));
var descriptorSetToSet = CharacterKeyedRecipientFallbackModel.ApplyDescriptorFields(
    descriptorCurrent with { DwordAt4D00 = 0x02 },
    descriptorInput with { DwordAt4D00 = 0x12 });
Check("Message-0x79 descriptor preserves +0x4D10 on a set-to-set state and sets +0x4C70",
    descriptorSetToSet
        == new CharacterKeyedDescriptorSnapshot(
            "selected", 0x12, descriptorInput.ResolvedDwordsAt4D04, 0x44, 1));
var descriptorClearToSet = CharacterKeyedRecipientFallbackModel.ApplyDescriptorFields(
    descriptorCurrent, descriptorInput with { DwordAt4D00 = 0x02 });
Check("Message-0x79 descriptor updates +0x4D10 from the source byte on a clear-to-set state",
    descriptorClearToSet
        == new CharacterKeyedDescriptorSnapshot(
            "selected", 0x02, descriptorInput.ResolvedDwordsAt4D04, 0x77, 0x55));
var descriptorClearToSetZeroSource = CharacterKeyedRecipientFallbackModel.ApplyDescriptorFields(
    descriptorCurrent, descriptorInput with { DwordAt4D00 = 0x02, SourceByteAt4D10 = 0 });
Check("Message-0x79 descriptor can clear +0x4D10 on a clear-to-set state when its source byte is zero",
    descriptorClearToSetZeroSource.ByteAt4D10 == 0);
var descriptorSetToClear = CharacterKeyedRecipientFallbackModel.ApplyDescriptorFields(
    descriptorCurrent with { DwordAt4D00 = 0x02 },
    descriptorInput with { DwordAt4D00 = 0x08 });
Check("Message-0x79 descriptor preserves +0x4D10 on a set-to-clear state and clears +0x4C70",
    descriptorSetToClear
        == new CharacterKeyedDescriptorSnapshot(
            "selected", 0x08, descriptorInput.ResolvedDwordsAt4D04, 0x44, 0));
var descriptorNoControlFlags = CharacterKeyedRecipientFallbackModel.ApplyDescriptorFields(
    descriptorCurrent, descriptorInput with { DwordAt4D00 = 0 });
Check("Message-0x79 descriptor preserves +0x4C70 when neither control flag is set",
    descriptorNoControlFlags.ControlByteAt4C70 == descriptorCurrent.ControlByteAt4C70);
Check("BaseNetController message 0x79 resolves the inserted tfGenericCharacter at the exact shared keyed registry",
    CharacterKeyedRecipientNativeLayout.RawTextOffsetToVirtualAddressDelta == 0x400C00
    && CharacterKeyedRecipientNativeLayout.RawRdataOffsetToVirtualAddressDelta == 0x401400
    && CharacterKeyedRecipientNativeLayout.CallbackFileOffset == 0x4C9B80
    && CharacterKeyedRecipientNativeLayout.CallbackVirtualAddress == 0x8CA780
    && CharacterKeyedRecipientNativeLayout.RegistryAccessorFileOffset == 0x35E700
    && CharacterKeyedRecipientNativeLayout.RegistryAccessorVirtualAddress == 0x75F300
    && CharacterKeyedRecipientNativeLayout.RegistryMapOffsetFromAccessorResult == 0x54
    && CharacterKeyedRecipientNativeLayout.RegistryLookupFileOffset == 0x4093E0
    && CharacterKeyedRecipientNativeLayout.RegistryInsertionFileOffset == 0x407F00
    && CharacterKeyedRecipientNativeLayout.RegistryInsertionVirtualAddress == 0x808B00
    && CharacterKeyedRecipientNativeLayout.RegistryEntryRecipientPointerOffset == 0x10
    && CharacterKeyedRecipientNativeLayout.GenericCharacterInsertionProducerFileOffset == 0x943360
    && CharacterKeyedRecipientNativeLayout.GenericCharacterInsertionProducerVirtualAddress == 0xD43F60
    && CharacterKeyedRecipientNativeLayout.GenericCharacterFactoryFileOffset == 0x425C90
    && CharacterKeyedRecipientNativeLayout.GenericCharacterFactoryVirtualAddress == 0x826890
    && CharacterKeyedRecipientNativeLayout.GenericCharacterAllocationSize == 0x5AE0
    && CharacterKeyedRecipientNativeLayout.GenericCharacterConstructorFileOffset == 0x422E80
    && CharacterKeyedRecipientNativeLayout.GenericCharacterConstructorVirtualAddress == 0x823A80
    && CharacterKeyedRecipientNativeLayout.GenericCharacterMemberPointerLoadFileOffset == 0x425E17
    && CharacterKeyedRecipientNativeLayout.GenericCharacterMemberPointerFieldOffset == 0x46A4
    && CharacterKeyedRecipientNativeLayout.GenericCharacterAuxiliaryPointerStoreFileOffset == 0x425EBC
    && CharacterKeyedRecipientNativeLayout.GenericCharacterAuxiliaryPointerFieldOffset == 0x58A0
    && CharacterKeyedRecipientNativeLayout.GenericCharacterVtableVirtualAddress == 0x1C63998
    && CharacterKeyedRecipientNativeLayout.GenericCharacterVtableFileOffset == 0x1862598
    && CharacterKeyedRecipientNativeLayout.GenericCharacterRttiName == ".?AVtfGenericCharacter@@"
    && CharacterKeyedRecipientNativeLayout.GenericCharacterVptrOffset == 0
    && CharacterKeyedRecipientNativeLayout.GenericCharacterRegistryAccessorCallFileOffset == 0x94378C
    && CharacterKeyedRecipientNativeLayout.GenericCharacterRegistryInsertionCallFileOffset == 0x943793
    && CharacterKeyedRecipientNativeLayout.GenericCharacterFactoryCallSiteVirtualAddress == 0xD4437C
    && CharacterKeyedRecipientNativeLayout.GenericCharacterRegistryInsertionCallVirtualAddress == 0xD44393
    && CharacterKeyedRecipientNativeLayout.RegistryInsertionPayloadSetupFileOffset == 0x407FA4
    && CharacterKeyedRecipientNativeLayout.ObjectKeyFirstDwordOffset == 8
    && CharacterKeyedRecipientNativeLayout.ObjectKeySecondDwordOffset == 0x0C
    && CharacterKeyedRecipientNativeLayout.ObjectKeyFirstDwordConstructorMask == 0xFFFFFF01
    && CharacterKeyedRecipientNativeLayout.ObjectKeyFirstDwordRegistryMask == 0xFFFFFF00
    && CharacterKeyedRecipientNativeLayout.ObjectKeyLowByteMarker == 1
    && CharacterKeyedRecipientNativeLayout.CallbackRecipientGateByteOffset == 8
    && CharacterKeyedRecipientNativeLayout.CallbackFallbackHighestReferencedOffset == 0x58A0
    && CharacterKeyedRecipientNativeLayout.GenericCharacterAllocationSize
        > CharacterKeyedRecipientNativeLayout.CallbackFallbackHighestReferencedExtent
    && CharacterKeyedRecipientNativeLayout.RegistryNodeAllocationSize == 0x18
    && CharacterKeyedRecipientNativeLayout.RegistryNodeInsertionHelperFileOffset == 0x40C470
    && CharacterKeyedRecipientNativeLayout.RegistryNodeAllocationHelperFileOffset == 0x40BBC0
    && CharacterKeyedRecipientNativeLayout.RegistryNodeFieldInitializerFileOffset == 0x40E6C0);
Check("Selector-3 MandM adapter construction is conditional and does not prove an NPC binding",
    CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterAllocationFileOffset == 0x4A190F
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterAllocationSize == 0xB0
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterConstructorCallFileOffset == 0x4A192E
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterConstructorFileOffset == 0x4C6360
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterPrivateContextFieldStoreFileOffset
        == 0x4C63AF
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterPrivateContextFieldOffset == 0x10
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterVtableStoreFileOffset
        == 0x4C63B5
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterControllerFieldOffset == 0x14
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterControllerFieldStoreFileOffset
        == 0x4C63BB
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterVtableVirtualAddress == 0x1C64640
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterVtableFileOffset == 0x1863240
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterRttiName
        == ".?AVMandMController@tfLocalCharacter@@"
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterBindingMethodFileOffset
        == 0x4C6490
    && CharacterKeyedDialogCallbackNativeLayout.SelectorThreeConditionalAdapterControllerCallbackPointerStoreFileOffset
        == 0x4C64A1
    && CharacterKeyedDialogCallbackNativeLayout.UnsupportedNpcStateFactoryBranchFileOffset == 0xAC96A8
    && keyedCharacterSelectorThreeAdapterSlots.Select(slot => slot.SlotOffset)
        .SequenceEqual([0x14, 0x18])
    && keyedCharacterSelectorThreeAdapterSlots.Select(slot => slot.MethodFileOffset)
        .SequenceEqual([0x4C7C50u, 0x4C82E0u]));
Check("Optional nested Character receiver is a separate +0x10 field with forwarding payload at +0x24",
    CharacterKeyedDialogCallbackNativeLayout.SharedKeyedReceiverConstructorHelperFileOffset == 0xBCAD70
    && CharacterKeyedDialogCallbackNativeLayout.SharedOptionalNestedReceiverNullStoreFileOffset == 0xBCAD91
    && CharacterKeyedDialogCallbackNativeLayout.SharedOptionalNestedReceiverFieldOffset == 0x10
    && CharacterKeyedDialogCallbackNativeLayout.BaseControllerOptionalNestedForwardCallFileOffset == 0x54039A
    && CharacterKeyedDialogCallbackNativeLayout.NpcControllerOptionalNestedForwardCallFileOffset == 0x54F4BA
    && CharacterKeyedDialogDispatchNativeLayout.MessageForwardedToNestedReceiverFieldOffset == 0x24
    && CharacterKeyedDialogDispatchNativeLayout.ReceiverOptionalCallbackObjectPointerFieldOffset == 0x2C);
Check("Conditional message fan-out records its constructed keyed state and recipient-vector call",
    MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateLazyConstructionMethodFileOffset == 0x7FE1C0
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutMessageQwordWriterFileOffset == 0x325BE0
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutCompactSelectorWriterFileOffset == 0x320900
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutMessageSelectorLoadFileOffset == 0xBD712B
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutWireSelectorWriterCallFileOffset == 0xBD7138
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutMessageSerializerCallFileOffset == 0xBD7145
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutOutputBufferPointerCallFileOffset == 0xBD71F3
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateConstructorFileOffset == 0xBD6E10
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateAllocationSize == 0x48
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateQwordArgumentLowFieldOffset == 8
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateQwordArgumentHighFieldOffset == 0x0C
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateContextPairSourceOffset == 0x0C
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateContextPairLowFieldOffset == 0x18
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateContextPairHighFieldOffset == 0x1C
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientVectorBeginFieldOffset == 0x2C
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientVectorEndFieldOffset == 0x30
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientVectorCapacityFieldOffset == 0x34
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientVectorAppendFileOffset == 0xBD8C90
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientRegisterMethodFileOffset == 0xBD6F30
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientFanoutMethodFileOffset == 0xBD7460
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientRegistrationVtableSlotOffset == 4
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientDispatchVtableSlotOffset == 0x0C
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientPointerSize == 4
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientDispatchRecordSize == 8
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientDispatchContextOffset == 0
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientDispatchDataLengthOffset == 4
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientDispatchSelectorOffset == 6
    && MatrixMessageParserFlowNativeLayout.ConditionalFanoutStateRecipientDispatchReservedOffset == 7);
Check("Observed DefaultReplayWriter recipient RTTI resolves the fan-out slot implementation",
    MatrixMessageParserFlowNativeLayout.DefaultReplayWriterObjectAllocationSize == 0x64
    && MatrixMessageParserFlowNativeLayout.DefaultReplayWriterConstructorFileOffset == 0xBD7EA0
    && MatrixMessageParserFlowNativeLayout.DefaultReplayWriterVtableStoreFileOffset == 0xBD7EE6
    && MatrixMessageParserFlowNativeLayout.DefaultReplayWriterVtableVirtualAddress == 0x1C73ADC
    && MatrixMessageParserFlowNativeLayout.DefaultReplayWriterCompleteObjectLocatorVirtualAddress == 0x1CCDF9C
    && MatrixMessageParserFlowNativeLayout.DefaultReplayWriterTypeDescriptorVirtualAddress == 0x1E2C7D4
    && MatrixMessageParserFlowNativeLayout.DefaultReplayWriterRttiName == ".?AVDefaultReplayWriter@nl@@"
    && MatrixMessageParserFlowNativeLayout.DefaultReplayWriterFanoutMethodVirtualAddress == 0xFD9010
    && MatrixMessageParserFlowNativeLayout.DefaultReplayWriterFanoutMethodFileOffset == 0xBD8410
    && MatrixMessageParserFlowNativeLayout.DefaultReplayWriterFanoutMethodHelperFileOffset == 0x12A0660);
Check("Direct context+8 selector 0x53 representative remains the RTTI-backed protocol handler",
    MatrixMessageNamespaceRoutingNativeLayout.ProtocolHandlerRttiName == ".?AVtfGameProtocolHandler@@"
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53ProtocolHandlerVtableSlotOffset == 0xD0
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53ProtocolHandlerMethodFileOffset == 0xACA580
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53ProtocolHandlerObservedMessageReadOffset == 0x34
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerFilterStringVirtualAddress == 0x1BAE9E0
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerNullFilterFallbackVirtualAddress == 0x1C5EA68
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerFilterMessageFieldOffset == 0x34
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerStateAccessorCallFileOffset == 0xACA5C9
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerStateAccessorFileOffset == 0x49DCC0
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerEqualFilterVectorCallFileOffset == 0xACA60C
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerEqualFilterVectorHelperFileOffset == 0x8F0180
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerVectorOwnerPointerFieldOffset == 0x43B8
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerVectorFieldOffset == 0x18
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerMessageRangeBeginFieldOffset == 0x28
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerMessageRangeEndFieldOffset == 0x2C
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerVectorRecordSize == 0x18
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerVectorCopyCallFileOffset == 0x8F021B
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerVectorCopyHelperFileOffset == 0x8F1AD0
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerUnequalFilterMapCallFileOffset == 0xACA64B
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerUnequalFilterMapHelperFileOffset == 0x937E40
    && MatrixMessageNamespaceRoutingNativeLayout.Selector53HandlerMapOwnerPointerFieldOffset == 0x43C4);
Check("ReplayReader getter and owner construction resolve the concrete message receiver field",
    MatrixMessageParserFlowNativeLayout.ReplayReaderMessageRecipientGetterOwnerRttiName
        == ".?AVtfLookingGlass@@"
    && MatrixMessageParserFlowNativeLayout.ReplayReaderMessageRecipientGetterOwnerVtableVirtualAddress
        == 0x1C6781C
    && MatrixMessageParserFlowNativeLayout.ReplayReaderMessageRecipientGetterOwnerCompleteObjectLocatorVirtualAddress
        == 0x1CA6FB4
    && MatrixMessageParserFlowNativeLayout.ReplayReaderMessageRecipientGetterOwnerTypeDescriptorVirtualAddress
        == 0x1E12C48
    && MatrixMessageParserFlowNativeLayout.ReplayReaderMessageRecipientGetterOwnerVtableSlotOffset == 0x24
    && MatrixMessageParserFlowNativeLayout.ReplayReaderMessageRecipientGetterVirtualAddress == 0xA5A370
    && MatrixMessageParserFlowNativeLayout.ReplayReaderMessageRecipientGetterFileOffset == 0x659770
    && MatrixMessageParserFlowNativeLayout.ReplayReaderMessageRecipientGetterContextFieldOffset == 0x50
    && MatrixMessageParserFlowNativeLayout.LookingGlassOwnerContextSetupCallFileOffset == 0x65A903
    && MatrixMessageParserFlowNativeLayout.LookingGlassOwnerContextSetupHelperFileOffset == 0x9B2D00
    && MatrixMessageParserFlowNativeLayout.LookingGlassOwnerPointerStoreFileOffset == 0x9B2D22
    && MatrixMessageParserFlowNativeLayout.ReplayMessageOwnerFactoryFileOffset == 0x9B73F0
    && MatrixMessageParserFlowNativeLayout.ReplayMessageOwnerFactoryCallFileOffset == 0x9B2D1A
    && MatrixMessageParserFlowNativeLayout.ReplayMessageOwnerConstructorCallFileOffset == 0x9B743D
    && MatrixMessageParserFlowNativeLayout.ReplayMessageOwnerConstructorFileOffset == 0x9B82F0
    && MatrixMessageParserFlowNativeLayout.ReplayMessageOwnerReceiverFieldOffset == 0x78
    && MatrixMessageParserFlowNativeLayout.ReplayReaderMessageRecipientSourceFieldOffset
        == MatrixMessageParserFlowNativeLayout.ReplayMessageOwnerReceiverFieldOffset
    && MatrixMessageParserFlowNativeLayout.ReplayMessageOwnerReceiverInitialNullStoreFileOffset == 0x9B8411
    && MatrixMessageParserFlowNativeLayout.ReplayMessageOwnerReceiverConstructorCallFileOffset == 0x9B7B21
    && MatrixMessageParserFlowNativeLayout.ReplayMessageOwnerReceiverFieldStoreFileOffset == 0x9B7B56
    && MatrixMessageParserFlowNativeLayout.ReplayMessageReceiverConstructorFileOffset == 0xAC8CA0
    && MatrixMessageParserFlowNativeLayout.ReplayMessageReceiverConstructorVtableStoreFileOffset == 0xAC8CE2
    && MatrixMessageParserFlowNativeLayout.OpenMovieDialogHandlerOrchestratorCallFileOffset == 0xACE2F3
    && MatrixMessageParserFlowNativeLayout.OpenMovieDialogApplicationDispatcherFileOffset == 0x991320
    && MatrixMessageParserFlowNativeLayout.OpenMovieDialogEventCodeFileOffset == 0x99147C
    && MatrixMessageParserFlowNativeLayout.OpenMovieDialogEventCode == 228
    && MatrixMessageParserFlowNativeLayout.PrivateDialogEventCodeFileOffset == 0x99165B
    && MatrixMessageParserFlowNativeLayout.PrivateDialogEventCode == 229
    && MatrixMessageParserFlowNativeLayout.PublicDialogDispatchContextFactoryCallFileOffset == 0xACE3A8
    && MatrixMessageParserFlowNativeLayout.PublicDialogApplicationHelperCallFileOffset == 0xACE3AF
    && MatrixMessageParserFlowNativeLayout.PublicDialogEventCodeFileOffset == 0x4D6D43
    && MatrixMessageParserFlowNativeLayout.PublicDialogEventCode == 357);
Check("tfGameProtocolHandler RTTI and concrete dialog receiver slots remain explicit",
    messageReceiverVtable.RttiName == ".?AVtfGameProtocolHandler@@"
    && messageReceiverVtable.VtableVirtualAddress == 0x1C71EC0
    && messageReceiverVtable.CompleteObjectLocatorVirtualAddress == 0x1CCA404
    && messageReceiverVtable.TypeDescriptorVirtualAddress == 0x1E29DF8
    && messageReceiverVtable.ObjectVtableFieldOffset == 0
    && messageReceiverVtable.CompleteObjectLocatorObjectOffset == 0
    && messageReceiverVtable.ConstructorFileOffset == 0xAC8CA0
    && messageReceiverVtable.DialogSlots.SequenceEqual(expectedMessageReceiverDialogSlots));
Check("Dialog message primary vftables map stream readers and writers separately from destruction",
    dialogMessageVtables.Count == 3
    && dialogMessageVtables.Select(vtable => vtable.VtableVirtualAddress)
        .SequenceEqual([0x1C67028u, 0x1C67018u, 0x1C66FF8u])
    && dialogMessageVtables.Select(vtable => vtable.RttiName).SequenceEqual(
    [
        ".?AVOpenMovieDialog@Character@firefall@@",
        ".?AVPrivateDialog@Character@firefall@@",
        ".?AVPublicDialog@Character@firefall@@"
    ])
    && dialogMessageVtables.Select((vtable, index) =>
        vtable.PrimarySlots.SequenceEqual(expectedDialogSlots[index])).All(matches => matches)
    && dialogMessageVtables[0].SerializedFieldOffsets.SequenceEqual([0x28])
    && dialogMessageVtables[1].SerializedFieldOffsets.SequenceEqual([0x28, 0x30, 0x38])
    && dialogMessageVtables[2].SerializedFieldOffsets.SequenceEqual([0x28, 0x2C]));
Check("Dialog application event routes use the uiManager dispatcher and preserve observed payload keys and offsets",
    DialogApplicationDispatchNativeLayout.UiManagerGlobalPointerFileOffset == 0x19FE6B8
    && DialogApplicationDispatchNativeLayout.UiManagerVtableFileOffset == 0x1872ABC
    && DialogApplicationDispatchNativeLayout.UiManagerEventDispatchSlotFileOffset == 0x1872B68
    && DialogApplicationDispatchNativeLayout.UiManagerEventDispatchMethodVirtualAddress == 0xFFA980
    && DialogApplicationDispatchNativeLayout.UiManagerEventDispatchMethodFileOffset == 0xBF9D80
    && DialogApplicationDispatchNativeLayout.UiManagerEventDispatchVtableSlotOffset == 0xAC
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameMapOffset == 0x6E8
    && DialogApplicationDispatchNativeLayout.UiManagerEventRegistryOffset == 0x6F0
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameArrayBeginOffset == 0x71C
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameArrayEndOffset == 0x720
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameMapEntryEventIdOffset == 0x14
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameArrayEntrySize == 4
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameArrayInitialCount == 0
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameMapLookupHelperFileOffset == 0x3E63B0
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameMapInsertHelperFileOffset == 0xC079A0
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameArrayAppendHelperFileOffset == 0xC07D10
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameMapLookupCallSiteFileOffset == 0xBF9ACE
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameMapInsertCallSiteFileOffset == 0xBF9AFE
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameArrayAppendCallSiteFileOffset == 0xBF9B2B
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameExistingIdReadFileOffset == 0xBF9ADA
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameArrayCountCalculationFileOffset == 0xBF9AE6
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameMapSharedStringCleanupCallSiteFileOffset == 0xC07A34
    && DialogApplicationDispatchNativeLayout.UiManagerEventNameMapSharedStringCleanupMethodFileOffset == 0xC08B40
    && DialogApplicationDispatchNativeLayout.UiManagerEventDeliveryQueueOffset == 0x5EC
    && DialogApplicationDispatchNativeLayout.UiManagerListenerEventMapOffset == 0x168
    && DialogApplicationDispatchNativeLayout.UiManagerListenerPendingDispatchOffset == 0x4C4
    && DialogApplicationDispatchNativeLayout.UiManagerEventListenerCallbackCallSiteFileOffset == 0xBF9D5B
    && DialogApplicationDispatchNativeLayout.UiManagerEventCallbackLookupCallSiteFileOffset == 0xC38438
    && DialogApplicationDispatchNativeLayout.UiManagerEventListenerMapLookupMethodFileOffset == 0xC41BC0
    && DialogApplicationDispatchNativeLayout.UiManagerEventListenerDispatchInsertCallSiteFileOffset == 0xC3847C
    && DialogApplicationDispatchNativeLayout.UiManagerEventListenerDispatchInsertMethodFileOffset == 0xC42180
    && DialogApplicationDispatchNativeLayout.UiManagerEventDeliveryQueueInsertCallSiteFileOffset == 0xC384D3
    && DialogApplicationDispatchNativeLayout.PayloadInt32FieldWriterMethodFileOffset == 0xC24210
    && DialogApplicationDispatchNativeLayout.PayloadBooleanFieldWriterMethodFileOffset == 0xC243E0
    && DialogApplicationDispatchNativeLayout.PayloadInt32PairFieldWriterMethodFileOffset == 0xC245A0
    && DialogApplicationDispatchNativeLayout.PayloadInt32FieldWriterFileOffset == 0xC24254
    && DialogApplicationDispatchNativeLayout.PayloadBooleanFieldWriterFileOffset == 0xC24424
    && DialogApplicationDispatchNativeLayout.PayloadInt32PairFieldWriterFileOffset == 0xC245E4
    && DialogApplicationDispatchNativeLayout.PayloadTextFieldWriterFileOffset == 0xC240F0
    && DialogApplicationDispatchNativeLayout.Dispatches.SequenceEqual(
    [
        new DialogApplicationEventDispatchSnapshot(0x78, 0xACE2E0, 0x991320, 0x99147C, 228, true),
        new DialogApplicationEventDispatchSnapshot(0x79, 0xACE300, 0x991510, 0x99165B, 229, true),
        new DialogApplicationEventDispatchSnapshot(0x7A, 0xACE390, 0x4D6C10, 0x4D6D43, 357, false)
    ])
    && DialogApplicationDispatchNativeLayout.OpenMoviePayloadFields.SequenceEqual(
    [
        new DialogApplicationPayloadFieldSnapshot(
            "vendor_id", 0x28, DialogApplicationPayloadValueKind.Int32)
    ])
    && DialogApplicationDispatchNativeLayout.PrivateDialogPayloadFields.SequenceEqual(
    [
        new DialogApplicationPayloadFieldSnapshot(
            "success", 0x28, DialogApplicationPayloadValueKind.BooleanByte),
        new DialogApplicationPayloadFieldSnapshot(
            "vendor_id",
            DialogApplicationDispatchNativeLayout.PrivateDialogVendorIdOffset,
            DialogApplicationPayloadValueKind.Int32),
        new DialogApplicationPayloadFieldSnapshot(
            "product_id", 0x30, DialogApplicationPayloadValueKind.Int32Pair),
        new DialogApplicationPayloadFieldSnapshot(
            "price_id", 0x38, DialogApplicationPayloadValueKind.Int32Pair),
        new DialogApplicationPayloadFieldSnapshot(
            "code",
            DialogApplicationDispatchNativeLayout.PrivateDialogCodePointerOffset,
            DialogApplicationPayloadValueKind.NullTerminatedText)
    ])
    && DialogApplicationDispatchNativeLayout.PublicDialogPreparedFields.SequenceEqual(
    [
        new DialogApplicationPayloadFieldSnapshot(
            "action_id", 0x30, DialogApplicationPayloadValueKind.Int32),
        new DialogApplicationPayloadFieldSnapshot(
            "activated", 0x84, DialogApplicationPayloadValueKind.BooleanByte)
    ])
    && DialogApplicationDispatchNativeLayout.PrivateDialogCodeFallback == "PURCHASE_SUCCESSFUL"
    && !DialogApplicationDispatchNativeLayout.PublicDialogDispatchPassesPayloadRecord
    && UiManagerNativeLayout.UiManagerEventNameMapLookupHelperFileOffset == 0x3E63B0
    && UiManagerNativeLayout.UiManagerEventNameMapInsertHelperFileOffset == 0xC079A0
    && UiManagerNativeLayout.UiManagerEventNameArrayAppendHelperFileOffset == 0xC07D10
    && UiManagerNativeLayout.UiManagerEventNameMapIdCountArgumentPreparationFileOffset == 0xBF9AF3
    && UiManagerNativeLayout.UiManagerEventNameMapEntryPairConstructionCallSiteFileOffset == 0xC079FE
    && UiManagerNativeLayout.UiManagerEventNameMapEntryPairIdInputFileOffset == 0xC079F4
    && UiManagerNativeLayout.UiManagerEventNameMapEntryPairConstructionMethodFileOffset == 0xC14660
    && UiManagerNativeLayout.UiManagerEventNameMapEntryPairIdInputLoadInstructionFileOffset == 0xC14692
    && UiManagerNativeLayout.UiManagerEventNameMapEntryPairIdStoreInstructionFileOffset == 0xC14694
    && UiManagerNativeLayout.UiManagerEventNameMapNodeConstructionCallSiteFileOffset == 0xC07A11
    && UiManagerNativeLayout.UiManagerEventNameMapNodeConstructionMethodFileOffset == 0xC16CB0
    && UiManagerNativeLayout.UiManagerEventNameMapNodeInsertCallSiteFileOffset == 0xC07A23
    && UiManagerNativeLayout.UiManagerEventNameMapNodeInsertMethodFileOffset == 0xC16D60
    && UiManagerNativeLayout.UiManagerEventNameMapNodeIdBaseInitializationFileOffset == 0xC16CEC
    && UiManagerNativeLayout.UiManagerEventNameMapNodePairIdInputLoadInstructionFileOffset == 0xC16D2A
    && UiManagerNativeLayout.UiManagerEventNameMapNodePairIdStoreInstructionFileOffset == 0xC16D2D
    && UiManagerNativeLayout.UiManagerEventNameMapSharedStringCleanupCallSiteFileOffset == 0xC07A34
    && UiManagerNativeLayout.UiManagerEventNameMapSharedStringCleanupMethodFileOffset == 0xC08B40
    && UiManagerNativeLayout.UiManagerEventNameMapEntryEventIdOffset == 0x14
    && UiManagerNativeLayout.UiManagerEventNameArrayEntrySize == 4
    && UiManagerNativeLayout.UiManagerEventNameArrayBeginInitialValue == 0
    && UiManagerNativeLayout.UiManagerEventNameArrayEndInitialValue == 0);
Check("Selected application event IDs remain explicitly unbound; native event-like literals are kept as candidates only",
    DialogApplicationDispatchNativeLayout.SelectedEventNameBindings.SequenceEqual(
    [
        new DialogApplicationEventNameBindingSnapshot(228, null, null),
        new DialogApplicationEventNameBindingSnapshot(229, null, null),
        new DialogApplicationEventNameBindingSnapshot(357, null, null)
    ])
    && DialogApplicationDispatchNativeLayout.UnreferencedNativeEventNameLiterals.SequenceEqual(
    [
        new DialogApplicationNativeEventNameLiteralSnapshot(
            "ON_VENDOR_PURCHASE_RESPONSE", 0x1749050, 0x1B4A450),
        new DialogApplicationNativeEventNameLiteralSnapshot(
            "ON_PLAY_MOVIE", 0x1749578, 0x1B4A978)
    ])
    && DialogApplicationDispatchNativeLayout.UnreferencedNativeEventNameLiterals.All(literal =>
        !DialogApplicationDispatchNativeLayout.SelectedEventNameBindings.Any(binding =>
            binding.EventName == literal.Name)));
Check("Scripting-object cleanup closes the VM before the ordered payload passes and resets",
    StartupObjectLifecycleNativeLayout.VmCloseCallFileOffset
        < StartupObjectLifecycleNativeLayout.ObservedPayloadDispatches[0].LoopFileOffset
    && StartupObjectLifecycleNativeLayout.PayloadPassContainerOffsets.SequenceEqual(
        [0xF0, 0xE8, 0xE0, 0xD8, 0x100, 0x110, 0x118, 0x140])
    && StartupObjectLifecycleNativeLayout.PayloadResetContainerOffsets.SequenceEqual(
        StartupObjectLifecycleNativeLayout.PayloadPassContainerOffsets)
    && StartupObjectLifecycleNativeLayout.Container118ResetBlockStartFileOffset
        < StartupObjectLifecycleNativeLayout.Container118ResetBlockEndFileOffset
    && StartupObjectLifecycleNativeLayout.PostPassContainer120CallFileOffset
        > StartupObjectLifecycleNativeLayout.Container118ResetBlockEndFileOffset
    && StartupObjectLifecycleNativeLayout.StoredObjectReleaseBlockStartFileOffset
        < StartupObjectLifecycleNativeLayout.StoredObjectReleaseBlockEndFileOffset);
Check("Cleanup callback payload offsets, virtual slots, and explicit arguments remain distinct",
    StartupObjectLifecycleNativeLayout.ObservedPayloadDispatches.SequenceEqual(
    [
        new StartupPayloadDispatchSnapshot(0xE8, 0xC33326, 0xC3335D, 0x14, 0, 1),
        new StartupPayloadDispatchSnapshot(0x118, 0xC336DA, 0xC33706, 0x14, 0, 1)
    ])
    && StartupObjectLifecycleNativeLayout.CallbackWalkerFileOffset == 0xC376A0
    && StartupObjectLifecycleNativeLayout.CallbackWalkerVirtualSlotOffset == 0x14
    && StartupObjectLifecycleNativeLayout.ObservedPayloadDispatches.All(dispatch =>
        dispatch.VirtualSlotOffset != dispatch.ExplicitArgumentValue));
Check("Nested container +0x120 keeps its observed fields and backing-array geometry separate from node-relative fields",
    StartupObjectLifecycleNativeLayout.Container120InitializerCallerSetupFileOffset == 0xC32A73
    && StartupObjectLifecycleNativeLayout.Container120InitializerCallFileOffset == 0xC32A79
    && StartupObjectLifecycleNativeLayout.Container120InitializerMethodFileOffset == 0xC41670
    && StartupObjectLifecycleNativeLayout.Container120HeadOffset == 0
    && StartupObjectLifecycleNativeLayout.Container120CountOffset == 4
    && StartupObjectLifecycleNativeLayout.Container120BackingBeginOffset == 8
    && StartupObjectLifecycleNativeLayout.Container120BackingEndOffset == 0xC
    && StartupObjectLifecycleNativeLayout.Container120BackingCapacityOffset == 0x10
    && StartupObjectLifecycleNativeLayout.Container120Field14Offset == 0x14
    && StartupObjectLifecycleNativeLayout.Container120Field14Value == 7
    && StartupObjectLifecycleNativeLayout.Container120Field18Offset == 0x18
    && StartupObjectLifecycleNativeLayout.Container120Field18Value == 8
    && StartupObjectLifecycleNativeLayout.Container120Field1COffset == 0x1C
    && StartupObjectLifecycleNativeLayout.Container120Field1CValueBits == 0x3F800000
    && StartupObjectLifecycleNativeLayout.Container120SentinelSize == 0xC
    && StartupObjectLifecycleNativeLayout.Container120SentinelNextOffset == 0
    && StartupObjectLifecycleNativeLayout.Container120SentinelPreviousOffset == 4
    && StartupObjectLifecycleNativeLayout.Container120InitialBackingSlotCount == 16
    && StartupObjectLifecycleNativeLayout.Container120BackingSlotSize == 4
    && StartupObjectLifecycleNativeLayout.Container120InitialBackingByteCount == 0x40
    && StartupObjectLifecycleNativeLayout.Container120ListNodeField8Offset == 8
    && StartupObjectLifecycleNativeLayout.PostPassContainer120Offset
        + StartupObjectLifecycleNativeLayout.Container120BackingBeginOffset == 0x128
    && StartupObjectLifecycleNativeLayout.Container120ListNodeField8Offset == 8);
Check("Nested container initial backing projection repeats the captured sentinel pointer in sixteen immutable slots",
    new uint[] { 0, 0x81234567, uint.MaxValue }.All(capturedSentinel =>
    {
        var slots = StartupObjectLifecycleNativeLayout.ProjectContainer120InitialBackingSlots(
            capturedSentinel);
        return slots.Count == 16
            && slots.All(slot => slot == capturedSentinel)
            && slots is IList<uint> readOnlySlots
            && readOnlySlots.IsReadOnly;
    }));
Check("Manager destruction and insertion-failure anchors record only observed release actions",
    StartupObjectLifecycleNativeLayout.Manager6B0DestructorFileOffset == 0xBF8AC0
    && StartupObjectLifecycleNativeLayout.ManagerKeyCleanupMethodFileOffset == 0xC0BD90
    && StartupObjectLifecycleNativeLayout.ManagerNodeReleaseMethodFileOffset == 0xC0EB00
    && StartupObjectLifecycleNativeLayout.ManagerNodeKeyOffset == 0x10
    && StartupObjectLifecycleNativeLayout.ManagerDestructorPathFileOffsets.SequenceEqual(
        [0xBF8AC0u, 0xC0BD90u, 0xC0EB00u])
    && StartupObjectLifecycleNativeLayout.InsertFailurePathHelperFileOffset == 0xC3C9B0
    && StartupObjectLifecycleNativeLayout.InsertFailurePathAnchorFileOffset == 0xC64420
    && StartupObjectLifecycleNativeLayout.InsertFailurePathFileOffsets.SequenceEqual(
        [0xC3C9B0u, 0xC64420u])
    && StartupObjectLifecycleNativeLayout.InsertFailurePayloadVirtualSlotOffset == 0
    && StartupObjectLifecycleNativeLayout.InsertFailurePayloadExplicitArgumentValue == 1);

if (args.Length != 0)
{
    if (args.Length != 2 || args[0] != "--binary")
    {
        Console.Error.WriteLine("Usage: Startup.Checks [--binary <FirefallClient.exe path>]");
        return 2;
    }
    StartupBinaryEvidence.Verify(args[1], Check);
}

return failures == 0 ? 0 : 1;
