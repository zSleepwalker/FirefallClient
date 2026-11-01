namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Route-index-3 switch-entry 5 layout. Discriminator cases remain structurally named only.
/// </summary>
public static class VehicleForcedMovementNativeLayout
{
    public const uint DispatcherFileOffset = 0x81EF6D;
    public const uint SwitchTableVirtualAddress = 0xC2260C;
    public const uint SwitchTableFileOffset = 0x821A0C;
    public const int SwitchEntryIndex = 5;
    public const int SwitchArgumentAdjustment = -0x53;
    public const int DispatcherValue = 0x58;
    public const uint CaseFileOffset = 0x81F0DC;
    public const int CaseLocalDwordValue = 0xD5;
    public const int AllocationSize = 0x1E0;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0xB09710;
    public const uint ConstructorCallSiteFileOffset = 0x81F104;
    public const uint BaseConstructorHelperFileOffset = 0xB476F0;
    public const int BaseConstructorArgument = 0x58;
    public const uint NestedStateInitializationCallSiteFileOffset = 0xB09738;
    public const uint NestedStateInitializationHelperFileOffset = 0x92B8A0;
    public const uint BodyReaderCallSiteFileOffset = 0x81D62F;
    public const uint BodyReaderFileOffset = 0x92F800;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint ByteReaderFileOffset = 0x337180;
    public const uint ThreeDwordReaderFileOffset = 0x384C80;
    public const uint FourValueReaderFileOffset = 0x384C20;
    public const uint ScalarDwordReaderFileOffset = 0x3568E0;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint OptionalQwordReaderFileOffset = 0x5FDA00;
    public const uint VtableVirtualAddress = 0x1C72938;
    public const string RttiName = ".?AVForcedMovement@Vehicle@firefall@@";

    public const int NestedStateOffset = 0x28;
    public const int NestedStateSize = 0x1B8;
    public const int StateDiscriminatorOffset = 0x08;
    public const int StateDiscriminatorObjectOffset = NestedStateOffset + StateDiscriminatorOffset;
    public const int StateDwordOffset = 0x04;
    public const int StateOptionalValuesOffset = 0x110;
    public const int StateSwitchTableVirtualAddress = 0xD308BC;
    public const uint StateSwitchTableFileOffset = 0x92FCBC;
    public const int StateSwitchCaseCount = 14;

    public static IReadOnlyList<uint> StateSwitchCaseTargetFileOffsets { get; } =
        Array.AsReadOnly(new uint[]
        {
            0x92FC83, 0x92F8E8, 0x92F92E, 0x92F990, 0x92FA16, 0x92FA5E, 0x92FA94,
            0x92FBCF, 0x92FAC6, 0x92FAF8, 0x92FB32, 0x92FB87, 0x92FBC1, 0x92FBED
        });
}
