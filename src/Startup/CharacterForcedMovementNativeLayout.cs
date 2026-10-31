namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x71 raw field layout. The discriminator-selected tail remains unresolved.
/// </summary>
public static class CharacterForcedMovementNativeLayout
{
    public const int InnerSelector = 0x71;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint InnerSelectorTableEntryOffset = 0x78;
    public const uint CaseVirtualAddress = 0xC1E14F;
    public const uint CaseFileOffset = 0x81D54F;
    public const uint CaseLocalDwordValue = 0x79;
    public const int AllocationSize = 0x1E8;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x526230;
    public const uint ConstructorCallSiteFileOffset = 0x81D573;
    public const uint BaseConstructorHelperFileOffset = 0x5A1170;
    public const int BaseConstructorArgument = 0x71;
    public const uint NestedStateInitializerFileOffset = 0x92B8A0;
    public const uint AllocationFailureContinuationFileOffset = 0x81D57C;
    public const uint BodyReaderCallSiteFileOffset = 0x81D589;
    public const uint BodyReaderFileOffset = 0x5EF690;
    public const uint DiscriminatedStateReaderFileOffset = 0x92F800;
    public const uint ConditionalPairReaderFileOffset = 0x5FDA00;
    public const uint DwordReaderFileOffset = 0x3370A0;
    public const uint WordReaderFileOffset = 0x336AC0;
    public const uint VtableVirtualAddress = 0x1C67098;
    public const string RttiName = ".?AVForcedMovement@Character@firefall@@";

    public const int NestedStateOffset = 0x28;
    public const int NestedStateSize = 0x1B8;
    public const int NestedStateDwordOffset = 0x2C;
    public const int DiscriminatorByteOffset = 0x30;
    public const int ConditionalPairOffset = 0x138;
    public const int ConditionalPairSize = 8;
    public const int DiscriminatorCaseCount = 14;
    public const int TrailingWordOffset = 0x1E0;

    public static readonly (int Offset, uint Value)[] ConstructorInitializedDwords =
    [
        (0x28, 0), (0x2C, 0), (0x30, 0), (0x114, 0), (0x118, 0),
        (0x138, 0), (0x13C, 0), (0x1B8, 0), (0x1BC, 0), (0x1C0, 0), (0x1C4, 0)
    ];

    public static readonly (int Offset, ushort Value)[] ConstructorInitializedWords =
    [
        (0x34, 0), (0x94, 0x0101), (0x100, 0x0101),
        (0x110, 0), (0x19C, 0x0101), (0x1CC, 0)
    ];
}
