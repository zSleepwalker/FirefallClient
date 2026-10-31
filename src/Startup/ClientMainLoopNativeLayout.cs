namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Proven static layout of the client App object (global dword_1DFFD98) main loop.
/// File offsets equal IDA addresses for .text (VA = offset + 0x400C00). See docs\ClientMainLoop.md.
/// Only items backed by disassembly and byte fingerprints are listed; names are descriptive, not original symbols.
/// </summary>
public static class ClientMainLoopNativeLayout
{
    public const int AppGlobalVirtualAddress = 0x1DFFD98;
    public const int QuitFlagGlobalVirtualAddress = 0x1E4B0B0;
    public const int NetworkManagerGlobalVirtualAddress = 0x1DFFF70;
    public const int EngineStepObjectGlobalVirtualAddress = 0x1E60EA4;
    public const int PerFrameServiceGlobalVirtualAddress = 0x1E46774;

    public const int InitializeFileOffset = 0x3063D0;
    public const int ShutdownFileOffset = 0x3095D0;
    public const int RunLoopFileOffset = 0x3098C0;
    public const int BootstrapInitializeCallSiteFileOffset = 0x2EB18F;
    public const int BootstrapShutdownCallSiteFileOffset = 0x2EB1F9;
    public const int ApplyPendingAppModesFileOffset = 0x30D850;
    public const int FrameTimerUpdateFileOffset = 0x323E00;
    public const int NetworkPumpFileOffset = 0xBBDB60;
    public const int StatusUpdateFileOffset = 0x30E1D0;
    public const int LoadStateStepFileOffset = 0x386430;

    public const int RunLoopQuitCheckSiteFileOffset = 0x3098F1;
    public const int RunLoopQuitCheckTailSiteFileOffset = 0x309F79;
    public const int RunLoopFrameTimerCallSiteFileOffset = 0x3098FE;
    public const int RunLoopEngineStepSlotCallSiteFileOffset = 0x3099CD;
    public const int RunLoopApplyPendingModesCallSiteFileOffset = 0x309A0C;
    public const int RunLoopServiceSlot2CallSiteFileOffset = 0x309B0D;
    public const int RunLoopCurrentModeSlot24CallSiteFileOffset = 0x309C8E;
    public const int ShutdownCurrentModeDeleteSiteFileOffset = 0x309767;

    public const int EngineStepVtableSlotOffset = 0x20;
    public const int CurrentModeContextVtableSlotOffset = 0x24;
    public const int CurrentModeLeaveVtableSlotOffset = 0x14;
    public const int CurrentModeDeletingDestructorVtableSlotOffset = 0x00;

    public const int AppFirstFrameFlagOffset = 0x00C;
    public const int AppSharedStatePointerOffset = 0xA28;
    public const int AppModeTimeoutStartTickOffset = 0xA2C;
    public const int AppModeTimeoutMillisecondsOffset = 0xA30;
    public const int AppFrameTimerObjectOffset = 0xA88;
    public const int AppFixedStepTicksOffset = 0xB48;
    public const int AppStepScaleFloatOffset = 0xB70;
    public const int AppStepOverrideFloatOffset = 0xB74;
    public const int AppStepOneShotFlagOffset = 0xB78;
    public const int AppModeSelectorOffset = 0xB80;
    public const int AppCurrentModeObjectOffset = 0xB98;
    public const int AppLoadStateObjectOffset = 0xC64;
    public const int AppPendingModeNodeOffset = 0x9E8;
}
