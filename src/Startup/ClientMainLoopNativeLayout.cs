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

    public const int InitializeTailFileOffset = 0x307224;
    public const int InitializeStoreFlag183FileOffset = 0x306441;
    public const int PlatformStatePumpFileOffset = 0x3289D0;
    public const int StatusTextBuilderFileOffset = 0x30E1D0;
    public const int EngineSystemVtableSlot8FileOffset = 0x187682C + 0x20;
    public const int EngineStepSlotIndex = 8;
    public const int EngineStepSlotDisplacement = 0x20;
    public const int EngineStepTargetFileOffset = 0xE55990;
    public const int EngineStepPeekMessageLoopFileOffset = 0xE55A70;
    public const int LoadStatePendingObjectOffset = 0x8;
    public const int LoadStatePendingIdOffset = 0x10;
    public const int LoadStateCurrentObjectOffset = 0x14;
    public const int LoadStateDeleteObjectOffset = 0x18;
    public const int LoadStateStateOffset = 0x1C;

    public const int InitCommandLineLikeStateFn304170FileOffset = 0x304170;
    public const int InitPathNormalizeFn3047E0FileOffset = 0x3047E0;
    public const int InitRegistryInstallFn304BB0FileOffset = 0x304BB0;
    public const int InitStringMapFindOrInsertFn325350FileOffset = 0x325350;
    public const int EngineStepFrameLimiterFileOffset = 0xE55C80;
    public const int EngineStepFpsCounterFileOffset = 0xE55750;
    public const int EngineStepLimiterCallSiteFileOffset = 0xE55C3E;
    public const int EngineStepFpsCallSiteFileOffset = 0xE55C4D;
    public const int EngineStepSleepCallSiteFileOffset = 0xE55DB3;
    public const int EngineStepTargetFpsFieldOffset = 0x164;
    public const int EngineStepFpsCapFieldOffset = 0x168;
    public const int InitializeTailEngineSlot54CallSiteFileOffset = 0x307403;
    public const int ArgvSwitchCheckFn2EA6F0FileOffset = 0x2EA6F0;
    public const int CommandLineDiagnosticsFn2EA990FileOffset = 0x2EA990;
    public const int GetCommandLineWCallSiteFileOffset = 0x2EAA69;
    public const int FirefallIniStringFileOffset = 0x173E9BC;
    public const int LocalAssetsFirefallIniStringFileOffset = 0x173E9A0;
    public const int MissingWritePathStringFileOffset = 0x173EBD0;
    public const int HotfixIniStringFileOffset = 0x1744318;
    public const int LowMipTexturesPakStringFileOffset = 0x173F4A8;
    public const int BigDumpSwitchStringFileOffset = 0x173DE3C;
    public const int DumptruckAutosubmitSwitchStringFileOffset = 0x173DE70;
    public const int IniContainerLoaderFn12DA710FileOffset = 0x12DA710;
    public const int ArgvFirstArgDirReaderFnE51590FileOffset = 0xE51590;
    public const int AssetDbSearchPathsLoggerFn1318400FileOffset = 0x1318400;
    public const int CfgFileExpanderFn1361890FileOffset = 0x1361890;
    public const int CfgFileExpanderCallSiteFileOffset = 0xF07FFB;
    public const int CfgPatternStringFileOffset = 0x185BA4C;
    public const int DirectoryListFn12B6C60FileOffset = 0x12B6C60;
    public const int LowMipTexturePakLoaderFnEB38B0FileOffset = 0xEB38B0;
    public const int LowMipTexturePakLoadCallSiteFileOffset = 0x3070E6;
    public const int LmtdMagicStringFileOffset = 0x17E73D8;
    public const int EngineInitFn10D8A40FileOffset = 0x10D8A40;
    public const int HotfixDownloadFn386D90FileOffset = 0x386D90;
    public const int AdditionalIniStringFileOffset = 0x173E9CC;
    public const int ConsoleStringFileOffset = 0x173EA88;
    public const int TextRegistryLookupFn12BE6E0FileOffset = 0x12BE6E0;
    public const int ArgvRegistryPushFn12BDCA0FileOffset = 0x12BDCA0;
    public const int ArgvRegistryPushFn12BE230FileOffset = 0x12BE230;
    public const int SwitchNameDirFileOffset = 0x18559D4;
    public const int SwitchNamePlatformFileOffset = 0x1855A98;
    public const int SwitchNameDumpConfigFileOffset = 0x1855AC4;
    // Round 6: firefall:// URI registration, hotfix, AssetDB search-path log (file offsets = VA - 0x400C00 for code, VA - 0x401400 for strings).
    public const int UriProtocolRegisterFn304BB0FileOffset = 0x304BB0;
    public const int OpenSwitchStringFileOffset = 0x173EC34;
    public const int UriProtocolFirefallStringFileOffset = 0x173EC90;
    public const int UriShellCommandTemplateStringFileOffset = 0x173ECE4;
    public const int HotfixStartStringFileOffset = 0x1744244;
    public const int HotfixDownloadPathKeyStringFileOffset = 0x1744398;
    public const int HotfixSectionStringFileOffset = 0x17443A8;
    public const int HotfixHttpTransportFn388330FileOffset = 0x388330;
    public const int AssetDbSearchPathsHeaderStringFileOffset = 0x1C5A694 - 0x401400;
    public const int OrdinarySearchPathsHeaderStringFileOffset = 0x1C5A6AC - 0x401400;}