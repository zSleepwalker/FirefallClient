namespace Firefall.Reconstruction.Startup;

/// <summary>
/// Selector-0x77 parser, message-vtable, and receiver span evidence. Field
/// meanings remain unknown.
/// </summary>
public static class CharacterNPCInteractionNativeLayout
{
    public const int InnerSelector = 0x77;
    public const uint InnerSelectorTableVirtualAddress = 0xC22444;
    public const uint InnerSelectorTableFileOffset = 0x821844;
    public const uint InnerSelectorTableEntryOffset = 0x90;
    public const uint CaseVirtualAddress = 0xC1E33C;
    public const uint CaseFileOffset = 0x81D73C;
    public const uint CaseLocalDwordValue = 0x7F;
    // Requested by the parser allocator; the runtime block extent is unknown.
    public const int AllocationSize = 0x30;
    public const uint AllocationFunctionFileOffset = 0x12B7FB0;
    public const uint ConstructorFileOffset = 0x5266A0;
    public const uint ConstructorCallSiteFileOffset = 0x81D761;
    public const uint BaseConstructorHelperFileOffset = 0x5A1430;
    public const int BaseConstructorArgument = 0x77;
    public const uint AllocationFailureContinuationFileOffset = 0x81F72C;
    public const uint BodyReaderCallSiteFileOffset = 0x81D768;
    public const uint BodyReaderFileOffset = 0x81F72E;
    public const uint BodyReaderQwordCallSiteFileOffset = 0x81F73C;
    public const uint BodyReaderReturnPointerLoadFileOffset = 0x81F741;
    public const uint QwordReaderFileOffset = 0x356F80;
    public const uint VtableVirtualAddress = 0x1C67048;
    public const string RttiName = ".?AVNPCInteraction@Character@firefall@@";

    public const int FirstQwordFieldOffset = 0x28;
    public const int FirstQwordFieldSize = 0x08;
    public const int ConstructorInitializedEndOffsetExclusive = 0x30;
    public const int ParserBodyReadEndOffsetExclusive = FirstQwordFieldOffset + FirstQwordFieldSize;
    public const int ConsumerStateBufferOffset = 0x30;
    public const int ConsumerStateBufferByteCount = 0x20;
    public const int ConsumerStateBufferEndOffsetExclusive =
        ConsumerStateBufferOffset + ConsumerStateBufferByteCount;

    public const int VtableWriterSlotOffset = 0x00;
    public const uint VtableWriterMethodVirtualAddress = 0x9EF950;
    public const uint VtableWriterFileOffset = 0x5EED50;
    public const int VtableReaderSlotOffset = 0x04;
    public const uint VtableReaderMethodVirtualAddress = 0x9EF8E0;
    public const uint VtableReaderFileOffset = 0x5EECE0;
    public const int VtableDeletingDestructorSlotOffset = 0x08;
    public const uint VtableDeletingDestructorMethodVirtualAddress = 0x9272E0;
    public const uint VtableDeletingDestructorFileOffset = 0x5266E0;
}
