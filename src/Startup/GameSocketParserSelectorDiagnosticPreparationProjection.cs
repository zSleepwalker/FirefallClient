using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct GameSocketParserSelectorDiagnosticPreparationCapture(
    GameSocketParserStoredByteSelectorGateCapture ParentCapture,
    uint? FreshESIAt327B,
    uint? FreshReceiverFieldDWORD);

public enum GameSocketParserSelectorDiagnosticPreparationCaptureState
{
    NotReached,
    Missing,
    Captured,
    SuppliedButUnused
}

public enum GameSocketParserSelectorDiagnosticPreparationCaptureKind
{
    FreshESIAt327B,
    FreshReceiverFieldDWORD
}

public enum GameSocketParserSelectorDiagnosticPreparationOutcome
{
    ParentHandoffNotAdmitted,
    MissingInheritedESP,
    MissingFreshESIAt327B,
    MissingFreshReceiverFieldDWORD,
    BoundedBeforeCall
}

public enum GameSocketParserSelectorDiagnosticPreparationPushKind
{
    OpaqueImmediateAt326A,
    OpaqueImmediateAt326F,
    OpaqueImmediateAt3274,
    OpaqueImmediateAt3279,
    FreshReceiverFieldDWORDAt327B
}

public enum GameSocketParserSelectorDiagnosticPreparationMemoryEffectKind
{
    ParentPrefix,
    FreshReceiverFieldDWORDRead,
    StackPush
}

public readonly record struct GameSocketParserSelectorDiagnosticPreparationPush(
    GameSocketParserSelectorDiagnosticPreparationPushKind Kind,
    uint InstructionRawFileOffset,
    uint CodeVirtualAddress,
    string Encoding,
    uint? StackPointerBefore,
    uint? StackAddress,
    uint? Value,
    uint? StackPointerAfter,
    bool Attempted,
    bool Reached,
    bool Committed,
    GameSocketParserStoredByteSelectorGateFlags? FlagsBefore,
    GameSocketParserStoredByteSelectorGateFlags? FlagsAfter);

public readonly record struct GameSocketParserSelectorDiagnosticPreparationRead(
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed);

public readonly record struct GameSocketParserSelectorDiagnosticPreparationStore(
    GameSocketParserSelectorDiagnosticPreparationPushKind Kind,
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Committed);

public readonly record struct GameSocketParserSelectorDiagnosticPreparationMemoryEffect(
    GameSocketParserSelectorDiagnosticPreparationMemoryEffectKind Kind,
    uint InstructionRawFileOffset,
    uint? Address,
    uint? Value,
    bool Attempted,
    bool Captured,
    bool Committed,
    GameSocketParserCountConsumptionStoresMemoryEffectKind? ParentKind);

public sealed class GameSocketParserSelectorDiagnosticPreparationPlan
{
    internal GameSocketParserSelectorDiagnosticPreparationPlan(
        GameSocketParserSelectorDiagnosticPreparationCapture capture,
        GameSocketParserStoredByteSelectorGatePlan parentPlan,
        ImmutableArray<GameSocketParserSelectorDiagnosticPreparationPush> pushes,
        ImmutableArray<GameSocketParserSelectorDiagnosticPreparationRead> reads,
        ImmutableArray<GameSocketParserSelectorDiagnosticPreparationStore> stores,
        ImmutableArray<GameSocketParserSelectorDiagnosticPreparationMemoryEffect> memoryEffects,
        ImmutableArray<GameSocketParserSelectorDiagnosticPreparationCaptureKind> unusedCaptures,
        GameSocketParserSelectorDiagnosticPreparationCaptureState freshESICaptureState,
        GameSocketParserSelectorDiagnosticPreparationCaptureState receiverFieldCaptureState,
        GameSocketParserSelectorDiagnosticPreparationOutcome outcome,
        bool parentHandoffAdmitted,
        uint? resultESP,
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop,
        uint stopBeforeRawFileOffset)
    {
        Capture = capture;
        ParentPlan = parentPlan;
        Pushes = pushes;
        Reads = reads;
        Stores = stores;
        MemoryEffects = memoryEffects;
        UnusedCaptures = unusedCaptures;
        FreshESICaptureState = freshESICaptureState;
        ReceiverFieldCaptureState = receiverFieldCaptureState;
        Outcome = outcome;
        ParentHandoffAdmitted = parentHandoffAdmitted;
        ResultESP = resultESP;
        FlagsAtStop = flagsAtStop;
        StopBeforeRawFileOffset = stopBeforeRawFileOffset;
    }

    public GameSocketParserSelectorDiagnosticPreparationCapture Capture { get; }
    public GameSocketParserStoredByteSelectorGatePlan ParentPlan { get; }
    public ImmutableArray<GameSocketParserSelectorDiagnosticPreparationPush> Pushes { get; }
    public ImmutableArray<GameSocketParserSelectorDiagnosticPreparationPush> Instructions =>
        Pushes;
    public ImmutableArray<GameSocketParserSelectorDiagnosticPreparationRead> Reads { get; }
    public ImmutableArray<GameSocketParserSelectorDiagnosticPreparationStore> Stores { get; }
    public ImmutableArray<GameSocketParserSelectorDiagnosticPreparationMemoryEffect> MemoryEffects { get; }
    public ImmutableArray<GameSocketParserSelectorDiagnosticPreparationMemoryEffect> OrderedMemoryEffects =>
        MemoryEffects;
    public ImmutableArray<GameSocketParserSelectorDiagnosticPreparationCaptureKind> UnusedCaptures { get; }
    public GameSocketParserSelectorDiagnosticPreparationCaptureState FreshESICaptureState { get; }
    public GameSocketParserSelectorDiagnosticPreparationCaptureState ReceiverFieldCaptureState { get; }
    public GameSocketParserSelectorDiagnosticPreparationOutcome Outcome { get; }
    public bool ParentHandoffAdmitted { get; }
    public uint? ParentESP => ParentPlan.ParentPlan.ResultESP;
    public uint? FreshESIAt327B => Capture.FreshESIAt327B;
    public uint? FreshReceiverFieldDWORD => Capture.FreshReceiverFieldDWORD;
    public uint? ReceiverFieldReadAddress =>
        FreshESIAt327B is uint esi
            ? unchecked(esi + GameSocketParserSelectorDiagnosticPreparationProjection
                .ReceiverFieldDisplacement)
            : null;
    public uint? ResultESP { get; }
    public uint? FinalESP => ResultESP;
    public GameSocketParserStoredByteSelectorGateFlags? FlagsAtStop { get; }
    public uint StopBeforeRawFileOffset { get; }
    public uint StopBeforeCodeVirtualAddress =>
        GameSocketParserCountConsumptionStoresProjection.RawFileOffsetToCodeVirtualAddress(
            StopBeforeRawFileOffset);
    public ImmutableArray<uint> ReachedInstructionRawFileOffsets =>
        Pushes.Where(push => push.Reached)
            .Select(push => push.InstructionRawFileOffset)
            .ToImmutableArray();
    public ImmutableArray<uint> AttemptedInstructionRawFileOffsets =>
        Pushes.Where(push => push.Attempted)
            .Select(push => push.InstructionRawFileOffset)
            .ToImmutableArray();
    public bool AllSelectorBranchesNotTaken =>
        ParentPlan.FirstJZState
            == GameSocketParserStoredByteSelectorGateBranchState.NotTaken
        && ParentPlan.Selector2JEState
            == GameSocketParserStoredByteSelectorGateBranchState.NotTaken
        && ParentPlan.Selector1JEState
            == GameSocketParserStoredByteSelectorGateBranchState.NotTaken;
    public bool ParentInstructionsReplayed => false;
    public bool LogicalMemoryEffectsOnly => true;
    public bool NativeMemoryEffectsModeled => false;
    public bool OrderedMemoryPrefixPreserved
    {
        get
        {
            ImmutableArray<GameSocketParserCountConsumptionStoresMemoryEffect> parentEffects =
                ParentPlan.OrderedMemoryEffects;
            if (MemoryEffects.Length < parentEffects.Length)
            {
                return false;
            }

            for (int index = 0; index < parentEffects.Length; index++)
            {
                GameSocketParserCountConsumptionStoresMemoryEffect parent = parentEffects[index];
                GameSocketParserSelectorDiagnosticPreparationMemoryEffect child =
                    MemoryEffects[index];
                if (child.Kind
                        != GameSocketParserSelectorDiagnosticPreparationMemoryEffectKind.ParentPrefix
                    || child.ParentKind != parent.Kind
                    || child.InstructionRawFileOffset != parent.InstructionRawFileOffset
                    || child.Address != parent.Address
                    || child.Value != parent.Value
                    || child.Attempted != parent.Attempted
                    || child.Captured != parent.Captured
                    || child.Committed != parent.Committed)
                {
                    return false;
                }
            }

            return true;
        }
    }
    public bool ReachedBoundedEndpoint =>
        Outcome == GameSocketParserSelectorDiagnosticPreparationOutcome.BoundedBeforeCall
        && StopBeforeRawFileOffset
            == GameSocketParserSelectorDiagnosticPreparationProjection.CallRawFileOffset
        || (!ParentHandoffAdmitted
            && Outcome
                == GameSocketParserSelectorDiagnosticPreparationOutcome.ParentHandoffNotAdmitted
            && ParentPlan.ReachedBoundedEndpoint
            && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset);
    public bool ExactBoundedCompletion =>
        Outcome == GameSocketParserSelectorDiagnosticPreparationOutcome.BoundedBeforeCall
        && StopBeforeRawFileOffset
            == GameSocketParserSelectorDiagnosticPreparationProjection.CallRawFileOffset
        || (!ParentHandoffAdmitted
            && Outcome
                == GameSocketParserSelectorDiagnosticPreparationOutcome.ParentHandoffNotAdmitted
            && ParentPlan.ExactBoundedCompletion
            && StopBeforeRawFileOffset == ParentPlan.StopBeforeRawFileOffset);
    public bool IsCleanCompletion =>
        ExactBoundedCompletion
        && ParentPlan.IsCleanCompletion
        && UnusedCaptures.IsEmpty
        && Outcome == GameSocketParserSelectorDiagnosticPreparationOutcome.BoundedBeforeCall;
    public bool ExactlyFivePushesCommitted =>
        Pushes.Length == 5 && Pushes.All(push => push.Committed);
    public bool CallReached => false;
    public bool CallTargetEntered => false;
    public bool OpaqueCallEffectsModeled => false;
    public bool ESIIdentitySemanticsModeled => false;
    public bool ParserExecutionModeled => false;
    public bool InputSemanticsModeled => false;
}

/// <summary>
/// Reprojects the clean parser selector fallthrough and records only the diagnostic
/// argument pushes, stopping immediately before the following opaque call.
/// </summary>
public static class GameSocketParserSelectorDiagnosticPreparationProjection
{
    public const uint FirstPushRawFileOffset = 0x00BD326A;
    public const uint SecondPushRawFileOffset = 0x00BD326F;
    public const uint ThirdPushRawFileOffset = 0x00BD3274;
    public const uint FourthPushRawFileOffset = 0x00BD3279;
    public const uint ReceiverFieldPushRawFileOffset = 0x00BD327B;
    public const uint CallRawFileOffset = 0x00BD3281;
    public const uint CallTargetRawFileOffset = 0x00BE2F50;

    public const uint FirstOpaqueImmediate = 0x00000357;
    public const uint SecondOpaqueImmediate = 0x01BB6ED8;
    public const uint ThirdOpaqueImmediate = 0x01BB6EB4;
    public const uint FourthOpaqueImmediate = 0x00000001;
    public const uint ReceiverFieldDisplacement = 0x00000110;
    public const uint PushWidth = 4;
    public const uint CallTargetCodeVirtualAddress =
        CallTargetRawFileOffset + GameSocketParserCountConsumptionStoresProjection
            .RawToCodeVirtualAddressDelta;

    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";
    public const uint SampleFileSize = 40197848;
    public const string CapturedBytes =
        "68 57 03 00 00 68 D8 6E BB 01 68 B4 6E BB 01 6A 01 "
        + "FF B6 10 01 00 00 E8 CA FC 00 00";

    public static GameSocketParserSelectorDiagnosticPreparationPlan Project(
        GameSocketParserSelectorDiagnosticPreparationCapture capture)
    {
        GameSocketParserStoredByteSelectorGatePlan parentPlan =
            GameSocketParserStoredByteSelectorGateProjection.Project(capture.ParentCapture);
        var pushes =
            ImmutableArray.CreateBuilder<GameSocketParserSelectorDiagnosticPreparationPush>();
        var reads =
            ImmutableArray.CreateBuilder<GameSocketParserSelectorDiagnosticPreparationRead>();
        var stores =
            ImmutableArray.CreateBuilder<GameSocketParserSelectorDiagnosticPreparationStore>();
        var effects =
            ImmutableArray.CreateBuilder<GameSocketParserSelectorDiagnosticPreparationMemoryEffect>();
        var unused =
            ImmutableArray.CreateBuilder<GameSocketParserSelectorDiagnosticPreparationCaptureKind>();
        foreach (GameSocketParserCountConsumptionStoresMemoryEffect parentEffect
            in parentPlan.OrderedMemoryEffects)
        {
            effects.Add(new(
                GameSocketParserSelectorDiagnosticPreparationMemoryEffectKind.ParentPrefix,
                parentEffect.InstructionRawFileOffset,
                parentEffect.Address,
                parentEffect.Value,
                parentEffect.Attempted,
                parentEffect.Captured,
                parentEffect.Committed,
                parentEffect.Kind));
        }

        GameSocketParserSelectorDiagnosticPreparationCaptureState esiState =
            GameSocketParserSelectorDiagnosticPreparationCaptureState.NotReached;
        GameSocketParserSelectorDiagnosticPreparationCaptureState fieldState =
            GameSocketParserSelectorDiagnosticPreparationCaptureState.NotReached;
        bool parentHandoffAdmitted =
            parentPlan.IsCleanCompletion
            && parentPlan.ExactBoundedCompletion
            && parentPlan.Outcome
                == GameSocketParserStoredByteSelectorGateOutcome.BoundedBeforeSelectedDestination
            && parentPlan.StopBeforeRawFileOffset == FirstPushRawFileOffset
            && parentPlan.InheritedEBX is 0 or 3
            && parentPlan.ParentPlan.Capture.FreshStackByteAtESPPlus13 is byte freshByte
            && freshByte != 0
            && parentPlan.FirstJZState
                == GameSocketParserStoredByteSelectorGateBranchState.NotTaken
            && parentPlan.Selector2JEState
                == GameSocketParserStoredByteSelectorGateBranchState.NotTaken
            && parentPlan.Selector1JEState
                == GameSocketParserStoredByteSelectorGateBranchState.NotTaken;

        uint? parentESP = parentPlan.ParentPlan.ResultESP;
        uint? resultESP = null;
        GameSocketParserStoredByteSelectorGateFlags? flagsAtStop = parentPlan.FlagsAtStop;
        GameSocketParserSelectorDiagnosticPreparationOutcome outcome;
        uint stopBefore;

        if (!parentHandoffAdmitted)
        {
            outcome =
                GameSocketParserSelectorDiagnosticPreparationOutcome.ParentHandoffNotAdmitted;
            stopBefore = parentPlan.StopBeforeRawFileOffset;
            esiState = UnreachedState(capture.FreshESIAt327B);
            fieldState = UnreachedState(capture.FreshReceiverFieldDWORD);
            AddUnusedIfSupplied(
                unused,
                esiState,
                GameSocketParserSelectorDiagnosticPreparationCaptureKind.FreshESIAt327B);
            AddUnusedIfSupplied(
                unused,
                fieldState,
                GameSocketParserSelectorDiagnosticPreparationCaptureKind.FreshReceiverFieldDWORD);
            return CreatePlan();
        }

        if (parentESP is not uint stackPointer)
        {
            outcome =
                GameSocketParserSelectorDiagnosticPreparationOutcome.MissingInheritedESP;
            stopBefore = FirstPushRawFileOffset;
            esiState = UnreachedState(capture.FreshESIAt327B);
            fieldState = UnreachedState(capture.FreshReceiverFieldDWORD);
            AddUnusedIfSupplied(
                unused,
                esiState,
                GameSocketParserSelectorDiagnosticPreparationCaptureKind.FreshESIAt327B);
            AddUnusedIfSupplied(
                unused,
                fieldState,
                GameSocketParserSelectorDiagnosticPreparationCaptureKind.FreshReceiverFieldDWORD);
            return CreatePlan();
        }

        resultESP = stackPointer;
        AddImmediatePush(
            GameSocketParserSelectorDiagnosticPreparationPushKind.OpaqueImmediateAt326A,
            FirstPushRawFileOffset,
            "68 57 03 00 00",
            FirstOpaqueImmediate);
        AddImmediatePush(
            GameSocketParserSelectorDiagnosticPreparationPushKind.OpaqueImmediateAt326F,
            SecondPushRawFileOffset,
            "68 D8 6E BB 01",
            SecondOpaqueImmediate);
        AddImmediatePush(
            GameSocketParserSelectorDiagnosticPreparationPushKind.OpaqueImmediateAt3274,
            ThirdPushRawFileOffset,
            "68 B4 6E BB 01",
            ThirdOpaqueImmediate);
        AddImmediatePush(
            GameSocketParserSelectorDiagnosticPreparationPushKind.OpaqueImmediateAt3279,
            FourthPushRawFileOffset,
            "6A 01",
            FourthOpaqueImmediate);

        esiState = CaptureState(capture.FreshESIAt327B);
        uint stackAddress = unchecked(stackPointer - PushWidth);
        uint? receiverAddress = capture.FreshESIAt327B is uint freshESI
            ? unchecked(freshESI + ReceiverFieldDisplacement)
            : null;
        bool esiCaptured = capture.FreshESIAt327B.HasValue;
        bool fieldCaptured = false;
        uint? fieldValue = null;
        if (!esiCaptured)
        {
            fieldState = capture.FreshReceiverFieldDWORD.HasValue
                ? GameSocketParserSelectorDiagnosticPreparationCaptureState.SuppliedButUnused
                : GameSocketParserSelectorDiagnosticPreparationCaptureState.NotReached;
            AddUnusedIfSupplied(
                unused,
                fieldState,
                GameSocketParserSelectorDiagnosticPreparationCaptureKind.FreshReceiverFieldDWORD);
            pushes.Add(new(
                GameSocketParserSelectorDiagnosticPreparationPushKind.FreshReceiverFieldDWORDAt327B,
                ReceiverFieldPushRawFileOffset,
                ToCodeVA(ReceiverFieldPushRawFileOffset),
                "FF B6 10 01 00 00",
                stackPointer,
                stackAddress,
                Value: null,
                StackPointerAfter: null,
                Attempted: true,
                Reached: true,
                Committed: false,
                flagsAtStop,
                flagsAtStop));
            outcome =
                GameSocketParserSelectorDiagnosticPreparationOutcome.MissingFreshESIAt327B;
            stopBefore = ReceiverFieldPushRawFileOffset;
        }
        else
        {
            fieldCaptured = capture.FreshReceiverFieldDWORD.HasValue;
            fieldValue = capture.FreshReceiverFieldDWORD;
            fieldState = CaptureState(fieldValue);
            reads.Add(new(
                ReceiverFieldPushRawFileOffset,
                receiverAddress,
                fieldValue,
                Attempted: true,
                Captured: fieldCaptured,
                Committed: false));
            effects.Add(new(
                GameSocketParserSelectorDiagnosticPreparationMemoryEffectKind.FreshReceiverFieldDWORDRead,
                ReceiverFieldPushRawFileOffset,
                receiverAddress,
                fieldValue,
                Attempted: true,
                Captured: fieldCaptured,
                Committed: false,
                ParentKind: null));

            if (!fieldCaptured)
            {
                pushes.Add(new(
                    GameSocketParserSelectorDiagnosticPreparationPushKind.FreshReceiverFieldDWORDAt327B,
                    ReceiverFieldPushRawFileOffset,
                    ToCodeVA(ReceiverFieldPushRawFileOffset),
                    "FF B6 10 01 00 00",
                    stackPointer,
                    stackAddress,
                    Value: null,
                    StackPointerAfter: null,
                    Attempted: true,
                    Reached: true,
                    Committed: false,
                    flagsAtStop,
                    flagsAtStop));
                outcome =
                    GameSocketParserSelectorDiagnosticPreparationOutcome
                        .MissingFreshReceiverFieldDWORD;
                stopBefore = ReceiverFieldPushRawFileOffset;
            }
            else
            {
                AddMemoryPush(
                    stackPointer,
                    stackAddress,
                    fieldValue!.Value);
                outcome =
                    GameSocketParserSelectorDiagnosticPreparationOutcome.BoundedBeforeCall;
                stopBefore = CallRawFileOffset;
            }
        }

        return CreatePlan();

        GameSocketParserSelectorDiagnosticPreparationPlan CreatePlan() =>
            new(
                capture,
                parentPlan,
                pushes.ToImmutable(),
                reads.ToImmutable(),
                stores.ToImmutable(),
                effects.ToImmutable(),
                unused.ToImmutable(),
                esiState,
                fieldState,
                outcome,
                parentHandoffAdmitted,
                resultESP,
                flagsAtStop,
                stopBefore);

        void AddImmediatePush(
            GameSocketParserSelectorDiagnosticPreparationPushKind kind,
            uint instructionOffset,
            string encoding,
            uint value)
        {
            uint address = unchecked(stackPointer - PushWidth);
            uint after = address;
            pushes.Add(new(
                kind,
                instructionOffset,
                ToCodeVA(instructionOffset),
                encoding,
                stackPointer,
                address,
                value,
                after,
                Attempted: true,
                Reached: true,
                Committed: true,
                flagsAtStop,
                flagsAtStop));
            stores.Add(new(
                kind,
                instructionOffset,
                address,
                value,
                Attempted: true,
                Committed: true));
            effects.Add(new(
                GameSocketParserSelectorDiagnosticPreparationMemoryEffectKind.StackPush,
                instructionOffset,
                address,
                value,
                Attempted: true,
                Captured: true,
                Committed: true,
                ParentKind: null));
            stackPointer = after;
            resultESP = after;
        }

        void AddMemoryPush(uint before, uint address, uint value)
        {
            uint after = address;
            const GameSocketParserSelectorDiagnosticPreparationPushKind kind =
                GameSocketParserSelectorDiagnosticPreparationPushKind
                    .FreshReceiverFieldDWORDAt327B;
            pushes.Add(new(
                kind,
                ReceiverFieldPushRawFileOffset,
                ToCodeVA(ReceiverFieldPushRawFileOffset),
                "FF B6 10 01 00 00",
                before,
                address,
                value,
                after,
                Attempted: true,
                Reached: true,
                Committed: true,
                flagsAtStop,
                flagsAtStop));
            stores.Add(new(
                kind,
                ReceiverFieldPushRawFileOffset,
                address,
                value,
                Attempted: true,
                Committed: true));
            effects.Add(new(
                GameSocketParserSelectorDiagnosticPreparationMemoryEffectKind.StackPush,
                ReceiverFieldPushRawFileOffset,
                address,
                value,
                Attempted: true,
                Captured: true,
                Committed: true,
                ParentKind: null));
            resultESP = after;
        }
    }

    private static uint ToCodeVA(uint rawFileOffset) =>
        GameSocketParserCountConsumptionStoresProjection
            .RawFileOffsetToCodeVirtualAddress(rawFileOffset);

    private static GameSocketParserSelectorDiagnosticPreparationCaptureState CaptureState(
        uint? value) =>
        value.HasValue
            ? GameSocketParserSelectorDiagnosticPreparationCaptureState.Captured
            : GameSocketParserSelectorDiagnosticPreparationCaptureState.Missing;

    private static GameSocketParserSelectorDiagnosticPreparationCaptureState UnreachedState(
        uint? value) =>
        value.HasValue
            ? GameSocketParserSelectorDiagnosticPreparationCaptureState.SuppliedButUnused
            : GameSocketParserSelectorDiagnosticPreparationCaptureState.NotReached;

    private static void AddUnusedIfSupplied(
        ImmutableArray<GameSocketParserSelectorDiagnosticPreparationCaptureKind>.Builder unused,
        GameSocketParserSelectorDiagnosticPreparationCaptureState state,
        GameSocketParserSelectorDiagnosticPreparationCaptureKind kind)
    {
        if (state
            == GameSocketParserSelectorDiagnosticPreparationCaptureState.SuppliedButUnused)
        {
            unused.Add(kind);
        }
    }
}
