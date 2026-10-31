namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Static UI-set folder traversal and file-enumeration observations. This does
/// not predict the host filesystem's enumeration order.
/// </summary>
public static class UiComponentLoadingNativeLayout
{
    public const uint UiSetXmlLoaderFileOffset = 0x30AF50;
    public const uint FolderPathLoaderFileOffset = 0x30AB90;
    public const uint DirectoryEnumerationMethodFileOffset = 0x12B6A00;
    public const uint FolderSearchPatternVirtualAddress = 0x1C1548C;
    public const uint DirectoryEnumerationCallSiteFileOffset = 0x30AC07;
    public const uint FindFirstFileCallSiteFileOffset = 0x12B6AD0;
    public const uint FindNextFileFunctionPointerLoadFileOffset = 0x12B6AEA;
    public const uint FindNextFileCallSiteFileOffset = 0x12B6C06;
    public const uint FindCloseCallSiteFileOffset = 0x12B6C15;
    public const uint DirectoryAttributeTestFileOffset = 0x12B6B94;
    public const uint DirectoryToFileBranchFileOffset = 0x12B6BA9;
    public const uint DirectoryListAppendCallSiteFileOffset = 0x12B6BC5;
    public const uint FileListAppendCallSiteFileOffset = 0x12B6BEA;
    public const uint ChildDirectoryProcessCallSiteFileOffset = 0x30AC35;
    public const uint FileListLoopStartFileOffset = 0x30AC9C;
    public const uint FileLoadCallSiteFileOffset = 0x30AE85;
    public const uint FindFirstFileIatVirtualAddress = 0x1AC473C;
    public const uint FindNextFileIatVirtualAddress = 0x1AC4738;
    public const uint FindCloseIatVirtualAddress = 0x1AC4734;
    public const int DirectoryAttributeMask = 0x10;

    public static IReadOnlyList<uint> UiSetFolderTraversalCallSites { get; } =
        Array.AsReadOnly(new[] { 0x30B58Bu, 0x30B67Du });
}
