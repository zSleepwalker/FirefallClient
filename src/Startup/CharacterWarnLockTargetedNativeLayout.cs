namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x6D raw field layout. Semantic meaning remains unknown.
/// </summary>
public static class CharacterWarnLockTargetedNativeLayout
{
    public const int InnerSelector = 0x6D;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint CaseFileOffset = 0x81D417;
    public const uint CaseLocalDwordValue = 0x75;
    public const int AllocationSize = 0x28;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x525FB0;
    public const uint BaseConstructorHelperFileOffset = 0x5A0FA0;
    public const int BaseConstructorArgument = 0x6D;
    public const uint AllocationFailureContinuationFileOffset = 0x8217D9;
    public const uint BodyReaderContinuationFileOffset = 0;
    public const uint BodyReaderFileOffset = 0;
    public const uint VtableVirtualAddress = 0x1C670C8;
    public const string RttiName = ".?AVWarnLockTargeted@Character@firefall@@";
}
