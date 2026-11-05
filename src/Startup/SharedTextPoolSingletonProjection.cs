using System.Collections.Immutable;

namespace Firefall.Reconstruction.Startup;

public readonly record struct SharedTextPoolSingletonGetterCapture(
    uint InitialGuardWord);

public enum SharedTextPoolSingletonGetterActionKind
{
    TestGuardLowBit,
    BranchToReturnWhenGuardLowBitIsSet,
    SetGuardLowBitInRegister,
    StoreGuardWord,
    CallAllocatorConfiguration,
    PushRuntimeInitStatusArgument,
    CallRuntimeInitStatusHelper,
    CleanRuntimeInitStatusArgument,
    ReturnReceiver
}

public readonly record struct SharedTextPoolSingletonGetterAction(
    SharedTextPoolSingletonGetterActionKind Kind,
    uint InstructionRawFileOffset,
    uint? TargetVirtualAddress = null,
    uint? TargetRawFileOffset = null,
    uint? Value = null,
    uint? ReceiverPointer = null,
    uint? FirstArgument = null,
    uint? SecondArgument = null,
    uint? ThirdArgument = null);

public readonly record struct SharedTextPoolSingletonGetterProjection(
    SharedTextPoolSingletonGetterCapture Capture,
    bool GuardLowBitWasSet,
    uint ReturnValue,
    ImmutableArray<SharedTextPoolSingletonGetterAction> Actions);

public readonly record struct SharedTextPoolSingletonGetterInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? TargetVirtualAddress = null,
    uint? TargetRawFileOffset = null);

/// <summary>
/// Projects the bounded getter's captured guard branch and ordinary return path.
/// The guard is an ordinary load/test/or/store; this does not assert thread safety,
/// readiness, or semantics for either called helper. The runtime-init-status helper
/// name labels the observed call target only. Calls on the clear-guard path are shown
/// only along the path where they return normally.
/// </summary>
public static class SharedTextPoolSingletonProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;
    public const uint GetterRawFileOffset = 0x12B9F70;
    public const uint GetterReturnRawFileOffset = 0x12B9FAF;
    public const uint GetterEndExclusiveRawFileOffset = 0x12B9FB0;
    public const uint GetterVirtualAddress =
        GetterRawFileOffset + CodeVirtualAddressDeltaFromRawFileOffset;

    public const uint GuardGlobalVirtualAddress = 0x1F113D0;
    public const uint ReceiverGlobalVirtualAddress = 0x1F113D8;
    public const uint AllocatorConfigurationHelperRawFileOffset = 0x12CDB40;
    public const uint RuntimeInitStatusHelperRawFileOffset = 0x136B628;
    public const uint RuntimeInitStatusArgumentVirtualAddress = 0x1AC2270;
    public const uint AlreadyInitializedBranchTargetRawFileOffset = 0x12B9FA7;

    public const uint ConfigurationFirstArgument = 32;
    public const uint ConfigurationSecondArgument = 128;
    public const uint ConfigurationThirdArgument = 16;
    public const uint GuardLowBitMask = 1;

    public static IReadOnlyList<SharedTextPoolSingletonGetterInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Save the frame pointer",
                0x12B9F70,
                "55"),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Establish the frame pointer",
                0x12B9F71,
                "8B EC"),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Align the stack",
                0x12B9F73,
                "83 E4 F8"),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Load the guard dword",
                0x12B9F76,
                "A1 D0 13 F1 01",
                TargetVirtualAddress: GuardGlobalVirtualAddress),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Test the guard's low bit",
                0x12B9F7B,
                "A8 01"),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Branch to the common receiver return when the low bit is set",
                0x12B9F7D,
                "75 28",
                TargetRawFileOffset: AlreadyInitializedBranchTargetRawFileOffset),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Push configuration argument 16",
                0x12B9F7F,
                "6A 10"),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Push configuration argument 128",
                0x12B9F81,
                "68 80 00 00 00"),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Set the low bit in the loaded guard value",
                0x12B9F86,
                "83 C8 01"),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Load the receiver global address into ECX",
                0x12B9F89,
                "B9 D8 13 F1 01",
                TargetVirtualAddress: ReceiverGlobalVirtualAddress),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Push configuration argument 32",
                0x12B9F8E,
                "6A 20"),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Store the modified guard dword",
                0x12B9F90,
                "A3 D0 13 F1 01",
                TargetVirtualAddress: GuardGlobalVirtualAddress),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Call the allocator configuration helper",
                0x12B9F95,
                "E8 A6 3B 01 00",
                TargetRawFileOffset: AllocatorConfigurationHelperRawFileOffset),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Push the runtime-init-status helper argument",
                0x12B9F9A,
                "68 70 22 AC 01",
                TargetVirtualAddress: RuntimeInitStatusArgumentVirtualAddress),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Call the runtime-init-status helper",
                0x12B9F9F,
                "E8 84 16 0B 00",
                TargetRawFileOffset: RuntimeInitStatusHelperRawFileOffset),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Remove the pushed runtime-init-status argument",
                0x12B9FA4,
                "83 C4 04"),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Load the receiver global address as the return value",
                0x12B9FA7,
                "B8 D8 13 F1 01",
                TargetVirtualAddress: ReceiverGlobalVirtualAddress),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Restore the stack pointer",
                0x12B9FAC,
                "8B E5"),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Restore the saved frame pointer",
                0x12B9FAE,
                "5D"),
            new SharedTextPoolSingletonGetterInstructionEvidence(
                "Return",
                0x12B9FAF,
                "C3")
        });

    /// <summary>
    /// Projects the normal-return path from the value captured by the native guard
    /// load. No global memory is read or written by this projection.
    /// </summary>
    public static SharedTextPoolSingletonGetterProjection Project(
        SharedTextPoolSingletonGetterCapture capture)
    {
        bool guardLowBitWasSet =
            (capture.InitialGuardWord & GuardLowBitMask) != 0;
        var actions =
            ImmutableArray.CreateBuilder<SharedTextPoolSingletonGetterAction>();

        actions.Add(new SharedTextPoolSingletonGetterAction(
            SharedTextPoolSingletonGetterActionKind.TestGuardLowBit,
            0x12B9F7B,
            TargetVirtualAddress: GuardGlobalVirtualAddress,
            Value: GuardLowBitMask));

        if (guardLowBitWasSet)
        {
            actions.Add(new SharedTextPoolSingletonGetterAction(
                SharedTextPoolSingletonGetterActionKind.BranchToReturnWhenGuardLowBitIsSet,
                0x12B9F7D,
                TargetRawFileOffset: AlreadyInitializedBranchTargetRawFileOffset));
        }
        else
        {
            uint updatedGuardWord = capture.InitialGuardWord | GuardLowBitMask;
            actions.Add(new SharedTextPoolSingletonGetterAction(
                SharedTextPoolSingletonGetterActionKind.SetGuardLowBitInRegister,
                0x12B9F86,
                Value: updatedGuardWord));
            actions.Add(new SharedTextPoolSingletonGetterAction(
                SharedTextPoolSingletonGetterActionKind.StoreGuardWord,
                0x12B9F90,
                TargetVirtualAddress: GuardGlobalVirtualAddress,
                Value: updatedGuardWord));
            actions.Add(new SharedTextPoolSingletonGetterAction(
                SharedTextPoolSingletonGetterActionKind.CallAllocatorConfiguration,
                0x12B9F95,
                TargetRawFileOffset: AllocatorConfigurationHelperRawFileOffset,
                ReceiverPointer: ReceiverGlobalVirtualAddress,
                FirstArgument: ConfigurationFirstArgument,
                SecondArgument: ConfigurationSecondArgument,
                ThirdArgument: ConfigurationThirdArgument));
            actions.Add(new SharedTextPoolSingletonGetterAction(
                SharedTextPoolSingletonGetterActionKind.PushRuntimeInitStatusArgument,
                0x12B9F9A,
                TargetVirtualAddress: RuntimeInitStatusArgumentVirtualAddress));
            actions.Add(new SharedTextPoolSingletonGetterAction(
                SharedTextPoolSingletonGetterActionKind.CallRuntimeInitStatusHelper,
                0x12B9F9F,
                TargetRawFileOffset: RuntimeInitStatusHelperRawFileOffset));
            actions.Add(new SharedTextPoolSingletonGetterAction(
                SharedTextPoolSingletonGetterActionKind.CleanRuntimeInitStatusArgument,
                0x12B9FA4,
                Value: 4));
        }

        actions.Add(new SharedTextPoolSingletonGetterAction(
            SharedTextPoolSingletonGetterActionKind.ReturnReceiver,
            0x12B9FA7,
            TargetVirtualAddress: ReceiverGlobalVirtualAddress,
            Value: ReceiverGlobalVirtualAddress));

        return new SharedTextPoolSingletonGetterProjection(
            capture,
            guardLowBitWasSet,
            ReceiverGlobalVirtualAddress,
            actions.ToImmutable());
    }
}
