using Firefall.Reconstruction.Startup;

internal static class SharedTextPoolSingletonChecks
{
    public static void Run(Action<string, bool> check)
    {
        check("Shared-text singleton getter is pinned to the requested sample and address mapping",
            SharedTextPoolSingletonProjectionRules.SampleSha256
                == "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445"
            && SharedTextPoolSingletonProjectionRules.GetterRawFileOffset == 0x12B9F70
            && SharedTextPoolSingletonProjectionRules.GetterVirtualAddress == 0x16BAB70
            && SharedTextPoolSingletonProjectionRules.CodeVirtualAddressDeltaFromRawFileOffset
                == 0x00400C00
            && SharedTextPoolSingletonProjectionRules.GetterReturnRawFileOffset == 0x12B9FAF
            && SharedTextPoolSingletonProjectionRules.GetterEndExclusiveRawFileOffset == 0x12B9FB0
            && SharedTextPoolSingletonProjectionRules.GuardGlobalVirtualAddress == 0x1F113D0
            && SharedTextPoolSingletonProjectionRules.ReceiverGlobalVirtualAddress == 0x1F113D8);

        check("Getter evidence preserves every instruction byte through the bounded return",
            SharedTextPoolSingletonProjectionRules.Instructions
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.Bytes))
                .SequenceEqual(new (uint RawFileOffset, string Bytes)[]
                {
                    (0x12B9F70, "55"),
                    (0x12B9F71, "8B EC"),
                    (0x12B9F73, "83 E4 F8"),
                    (0x12B9F76, "A1 D0 13 F1 01"),
                    (0x12B9F7B, "A8 01"),
                    (0x12B9F7D, "75 28"),
                    (0x12B9F7F, "6A 10"),
                    (0x12B9F81, "68 80 00 00 00"),
                    (0x12B9F86, "83 C8 01"),
                    (0x12B9F89, "B9 D8 13 F1 01"),
                    (0x12B9F8E, "6A 20"),
                    (0x12B9F90, "A3 D0 13 F1 01"),
                    (0x12B9F95, "E8 A6 3B 01 00"),
                    (0x12B9F9A, "68 70 22 AC 01"),
                    (0x12B9F9F, "E8 84 16 0B 00"),
                    (0x12B9FA4, "83 C4 04"),
                    (0x12B9FA7, "B8 D8 13 F1 01"),
                    (0x12B9FAC, "8B E5"),
                    (0x12B9FAE, "5D"),
                    (0x12B9FAF, "C3")
                })
            && SharedTextPoolSingletonProjectionRules.GetterReturnRawFileOffset
                + 1 == SharedTextPoolSingletonProjectionRules.GetterEndExclusiveRawFileOffset
            && SharedTextPoolSingletonProjectionRules.Instructions[^1].RawFileOffset
                == SharedTextPoolSingletonProjectionRules.GetterReturnRawFileOffset);

        check("Getter call and global references resolve to the recorded addresses",
            SharedTextPoolSingletonProjectionRules.Instructions
                .Where(instruction => instruction.TargetVirtualAddress.HasValue)
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.TargetVirtualAddress!.Value))
                .SequenceEqual(new (uint RawFileOffset, uint TargetVirtualAddress)[]
                {
                    (0x12B9F76, 0x1F113D0),
                    (0x12B9F89, 0x1F113D8),
                    (0x12B9F90, 0x1F113D0),
                    (0x12B9F9A, 0x1AC2270),
                    (0x12B9FA7, 0x1F113D8)
                })
            && SharedTextPoolSingletonProjectionRules.Instructions
                .Where(instruction => instruction.TargetRawFileOffset.HasValue)
                .Select(instruction =>
                    (instruction.RawFileOffset, instruction.TargetRawFileOffset!.Value))
                .SequenceEqual(new (uint RawFileOffset, uint TargetRawFileOffset)[]
                {
                    (0x12B9F7D, 0x12B9FA7),
                    (0x12B9F95, 0x12CDB40),
                    (0x12B9F9F, 0x136B628)
                })
            && 0x12B9F7D + 2 + 0x28
                == SharedTextPoolSingletonProjectionRules.AlreadyInitializedBranchTargetRawFileOffset);

        check("Clear guard sets only the captured low bit before configuration and init-status calls",
            SharedTextPoolSingletonProjectionRules.ConfigurationFirstArgument == 32
            && SharedTextPoolSingletonProjectionRules.ConfigurationSecondArgument == 128
            && SharedTextPoolSingletonProjectionRules.ConfigurationThirdArgument == 16
            && SharedTextPoolSingletonProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12B9F90)
                .RawFileOffset
                < SharedTextPoolSingletonProjectionRules.Instructions
                    .Single(instruction => instruction.RawFileOffset == 0x12B9F95)
                    .RawFileOffset
            && SharedTextPoolSingletonProjectionRules.Instructions
                .Single(instruction => instruction.RawFileOffset == 0x12B9F95)
                .RawFileOffset
                < SharedTextPoolSingletonProjectionRules.Instructions
                    .Single(instruction => instruction.RawFileOffset == 0x12B9F9F)
                    .RawFileOffset);

        var guardProjectionCases = new[]
        {
            (Guard: 0x00000000u, AlreadySet: false, Updated: 0x00000001u),
            (Guard: 0x00000001u, AlreadySet: true, Updated: 0x00000001u),
            (Guard: 0x00000002u, AlreadySet: false, Updated: 0x00000003u),
            (Guard: 0xFFFFFFFEu, AlreadySet: false, Updated: 0xFFFFFFFFu),
            (Guard: 0xFFFFFFFFu, AlreadySet: true, Updated: 0xFFFFFFFFu)
        };
        bool guardProjectionIsExact = true;
        foreach (var testCase in guardProjectionCases)
        {
            SharedTextPoolSingletonGetterProjection projection =
                SharedTextPoolSingletonProjectionRules.Project(
                    new SharedTextPoolSingletonGetterCapture(testCase.Guard));
            SharedTextPoolSingletonGetterActionKind[] expectedKinds = testCase.AlreadySet
                ? new[]
                {
                    SharedTextPoolSingletonGetterActionKind.TestGuardLowBit,
                    SharedTextPoolSingletonGetterActionKind.BranchToReturnWhenGuardLowBitIsSet,
                    SharedTextPoolSingletonGetterActionKind.ReturnReceiver
                }
                : new[]
                {
                    SharedTextPoolSingletonGetterActionKind.TestGuardLowBit,
                    SharedTextPoolSingletonGetterActionKind.SetGuardLowBitInRegister,
                    SharedTextPoolSingletonGetterActionKind.StoreGuardWord,
                    SharedTextPoolSingletonGetterActionKind.CallAllocatorConfiguration,
                    SharedTextPoolSingletonGetterActionKind.PushRuntimeInitStatusArgument,
                    SharedTextPoolSingletonGetterActionKind.CallRuntimeInitStatusHelper,
                    SharedTextPoolSingletonGetterActionKind.CleanRuntimeInitStatusArgument,
                    SharedTextPoolSingletonGetterActionKind.ReturnReceiver
                };

            if (projection.GuardLowBitWasSet != testCase.AlreadySet
                || projection.ReturnValue != 0x1F113D8
                || !projection.Actions.Select(action => action.Kind).SequenceEqual(expectedKinds)
                || (!testCase.AlreadySet
                    && (projection.Actions[1].Value != testCase.Updated
                        || projection.Actions[2].Value != testCase.Updated
                        || projection.Actions[3].ReceiverPointer != 0x1F113D8
                        || projection.Actions[3].FirstArgument != 32
                        || projection.Actions[3].SecondArgument != 128
                        || projection.Actions[3].ThirdArgument != 16)))
            {
                guardProjectionIsExact = false;
                break;
            }
        }

        check("Captured guard values select the observed branch and preserve upper bits on the store",
            guardProjectionIsExact);

        SharedTextPoolSingletonGetterProjection guarded =
            SharedTextPoolSingletonProjectionRules.Project(
                new SharedTextPoolSingletonGetterCapture(0x12345679));
        check("Set guard low bit skips both calls and returns the receiver global address",
            guarded.GuardLowBitWasSet
            && guarded.ReturnValue == 0x1F113D8
            && guarded.Actions.All(action =>
                action.Kind != SharedTextPoolSingletonGetterActionKind.CallAllocatorConfiguration
                && action.Kind != SharedTextPoolSingletonGetterActionKind.CallRuntimeInitStatusHelper));
    }
}
