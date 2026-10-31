namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Static singleton and event-routing observations for the client's UI manager.
/// This does not construct the object or dispatch application events.
/// </summary>
public static class UiManagerNativeLayout
{
    public const uint SingletonRegistrationStubFileOffset = 0x3EE8C0;
    public const uint SingletonInitializeMethodFileOffset = 0x3E44F0;
    public const uint SingletonListWalkerFileOffset = 0x12C3A80;
    public const uint ClientApplicationBootstrapFileOffset = 0x2EAE70;
    public const uint UiManagerConstructorFileOffset = 0xBF7A80;
    public const uint UiManagerEventDispatchMethodFileOffset = 0xBF9D80;
    public const uint UiManagerEventReadyFlagTestFileOffset = 0xBF9DBA;
    public const uint UiManagerEventReadyFlagInitializeFileOffset = 0xBF7DCB;
    public const uint UiManagerEventListenerDispatchFileOffset = 0xBF9C40;
    public const uint UiManagerEventListenerLookupFileOffset = 0xC0AA20;
    public const uint UiManagerEventCallbackRouteFileOffset = 0xC38400;
    public const uint LoginMessage145GlobalLoadFileOffset = 0x63C1E2;
    public const uint LoginMessage145CallFileOffset = 0x63C1F5;
    public const uint SingletonObjectVirtualAddress = 0x1E002A8;
    public const uint SingletonVtableVirtualAddress = 0x1C62FF0;
    public const uint SingletonTypeDescriptorVirtualAddress = 0x1E06728;
    public const uint UiManagerGlobalPointerVirtualAddress = 0x1E002B8;
    public const uint UiManagerVtableVirtualAddress = 0x1C73EBC;
    public const uint UiManagerTypeDescriptorVirtualAddress = 0x1E2CC90;
    public const uint SingletonFactoryListHeadVirtualAddress = 0x1E4B0A4;
    public const uint InitializedSingletonListHeadVirtualAddress = 0x1E4B0A8;
    public const int SingletonNextPointerOffset = 0x08;
    public const int SingletonInitializedFlagOffset = 0x04;
    public const int SingletonInstancePointerOffset = 0x10;
    public const int UiManagerAllocationSize = 0xA10;
    public const int UiManagerEventDispatchVtableOffset = 0xAC;
    public const int UiManagerEventRegistryOffset = 0x1BC;
    public const int UiManagerEventReadyFlagOffset = 0x672;
    public const int UiManagerListenerNameOffset = 0x70;
    public const int LoginResponseNotificationId = 0x145;
    public const int EnterZonePausedNotificationId = 110;
    public const string SingletonRttiName = ".?AV?$slSingleton@VuiManager@@@@";
    public const string UiManagerRttiName = ".?AVuiManager@@";
}
