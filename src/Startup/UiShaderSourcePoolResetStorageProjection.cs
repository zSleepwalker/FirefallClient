namespace Firefall.Reconstruction.Startup;

public readonly record struct UiShaderSourcePoolResetStorageCapture(
    uint RequestedDwordCount,
    uint? AllocatorReturnedBasePointer = null,
    uint? InitialDescriptorBeginPointer = null,
    uint? InitialDescriptorEndPointer = null,
    bool? CopyTargetReturnedNormally = null,
    uint? PostCopyDescriptorBeginPointer = null,
    uint? PostCopyDescriptorEndPointer = null,
    bool? ReleaseHelperReturnedNormally = null);

public enum UiShaderSourcePoolResetStorageBoundary
{
    AllocationReportCellCallBoundary,
    CopyCallBoundary,
    CopyTargetDidNotReturn,
    ReleaseHelperCallBoundary,
    ReleaseHelperDidNotReturn,
    NormalReturn
}

public readonly record struct UiShaderSourcePoolResetStorageProjection(
    UiShaderSourcePoolResetStorageCapture Capture,
    bool AllocationCallReached,
    uint? RequestedAllocationBytes,
    bool FailureReportCellCallReached,
    bool CopyCallReached,
    uint? CopyTargetCellVirtualAddress,
    uint? CopyNewBaseArgument,
    uint? CopyOldBeginArgument,
    uint? CopyLengthBytes,
    bool InitialCopyDescriptorReadsReached,
    uint? InitialCopyDescriptorBeginRead,
    uint? InitialCopyDescriptorEndRead,
    bool PostCopyDescriptorReadsReached,
    uint? PostCopyDescriptorBeginRead,
    uint? PostCopyDescriptorEndRead,
    uint? RetainedDwordCount,
    bool ReleaseHelperCallReached,
    uint? ReleaseHelperPointerArgument,
    bool? ReleaseHelperReturnedNormally,
    bool BeginPublicationReached,
    uint? PublishedBeginPointer,
    bool StorageEndPublicationReached,
    uint? PublishedStorageEndPointer,
    bool EndPublicationReached,
    uint? PublishedEndPointer,
    uint? ReturnedEax,
    uint? ReturnRawFileOffset,
    UiShaderSourcePoolResetStorageBoundary Boundary);

public readonly record struct UiShaderSourcePoolResetStorageInstructionEvidence(
    string Observation,
    uint RawFileOffset,
    string Bytes,
    uint? BranchTargetRawFileOffset = null,
    uint? CallTargetRawFileOffset = null,
    uint? IndirectCellVirtualAddress = null);

/// <summary>
/// Projects the bounded reset storage helper without reading or writing native
/// memory or modeling allocator, reporter, copy-target, or release-helper effects.
/// </summary>
public static class UiShaderSourcePoolResetStorageProjectionRules
{
    public const string SampleSha256 =
        "97362d668ff64778193a64759265e4ae90842043c4a15b627df9902fd9135445";

    public const uint CodeVirtualAddressDeltaFromRawFileOffset = 0x00400C00;

    public const uint FunctionRawFileOffset = 0x006A5900;
    public const uint FunctionEndRawFileOffsetExclusive = 0x006A5979;
    public const uint FunctionCodeVirtualAddress = 0x00AA6500;
    public const uint FunctionSize = 0x79;
    public const uint MaximumRequestedDwordCount = 0x3FFFFFFF;

    public const uint AllocationCallRawFileOffset = 0x006A5921;
    public const uint AllocationTargetRawFileOffset = 0x012B7FB0;
    public const uint FailureReportCallRawFileOffset = 0x006A592F;
    public const uint FailureReportCellVirtualAddress = 0x01AC4424;
    public const uint CopyTargetCallRawFileOffset = 0x006A5942;
    public const uint CopyTargetCellVirtualAddress = 0x01AC489C;
    public const uint ReleaseHelperCallRawFileOffset = 0x006A595C;
    public const uint ReleaseHelperTargetRawFileOffset = 0x012C8D60;

    public const uint InitialCopyBeginReadRawFileOffset = 0x006A5935;
    public const uint InitialCopyEndReadRawFileOffset = 0x006A5937;
    public const uint InitialCopyLengthMaskRawFileOffset = 0x006A593C;
    public const uint PostCopyBeginReadRawFileOffset = 0x006A5948;
    public const uint PostCopyEndReadRawFileOffset = 0x006A594D;
    public const uint RetainedCountArithmeticShiftRawFileOffset = 0x006A5952;
    public const uint StorageEndValueComputeRawFileOffset = 0x006A5961;
    public const uint EndValueComputeRawFileOffset = 0x006A596C;
    public const uint BeginPublicationRawFileOffset = 0x006A5964;
    public const uint StorageEndPublicationRawFileOffset = 0x006A5966;
    public const uint EndPublicationRawFileOffset = 0x006A596F;
    public const uint NormalReturnRawFileOffset = 0x006A5976;
    public const uint NormalReturnStackPopBytes = 4;

    public const string CapturedFunctionBytes =
        "55 8B EC 53 8B 5D 08 56 33 F6 57 8B F9 85 DB 74 24 " +
        "81 FB FF FF FF 3F 77 16 8D 04 9D 00 00 00 00 50 E8 " +
        "8A 26 C1 00 8B F0 83 C4 04 85 F6 75 06 FF 15 24 44 " +
        "AC 01 8B 0F 8B 47 04 2B C1 83 E0 FC 50 51 56 FF 15 " +
        "9C 48 AC 01 8B 0F 83 C4 0C 8B 47 04 2B C1 C1 F8 02 " +
        "89 45 08 85 C9 74 05 E8 FF 33 C2 00 8D 04 9E 89 37 " +
        "89 47 08 8B 45 08 8D 04 86 89 47 04 5F 5E 5B 5D C2 04 00";

    public static IReadOnlyList<UiShaderSourcePoolResetStorageInstructionEvidence>
        Instructions { get; } =
        Array.AsReadOnly(new[]
        {
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Capture the requested DWORD count",
                0x006A5904,
                "8B 5D 08"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Initialize the new-base register to zero",
                0x006A5908,
                "33 F6"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "A zero count skips the allocation and report paths",
                0x006A590F,
                "74 24",
                BranchTargetRawFileOffset: 0x006A5935),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Compare nonzero count with the inclusive maximum",
                0x006A5911,
                "81 FB FF FF FF 3F"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "An above-maximum count branches directly to the report cell",
                0x006A5917,
                "77 16",
                BranchTargetRawFileOffset: 0x006A592F),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Scale the accepted count by four for allocation",
                0x006A5919,
                "8D 04 9D 00 00 00 00"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Call the allocator",
                AllocationCallRawFileOffset,
                "E8 8A 26 C1 00",
                CallTargetRawFileOffset: AllocationTargetRawFileOffset),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "A null allocator result reaches the report cell",
                0x006A592D,
                "75 06",
                BranchTargetRawFileOffset: 0x006A5935),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Call through the captured failure-report cell",
                FailureReportCallRawFileOffset,
                "FF 15 24 44 AC 01",
                IndirectCellVirtualAddress: FailureReportCellVirtualAddress),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Read the initial descriptor begin for the copy arguments",
                InitialCopyBeginReadRawFileOffset,
                "8B 0F"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Read the initial descriptor end for the copy length",
                InitialCopyEndReadRawFileOffset,
                "8B 47 04"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Round the wrapping initial end-minus-begin byte length down to a DWORD boundary",
                InitialCopyLengthMaskRawFileOffset,
                "83 E0 FC"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Call through the captured copy-target cell with new base, old begin, and masked length",
                CopyTargetCallRawFileOffset,
                "FF 15 9C 48 AC 01",
                IndirectCellVirtualAddress: CopyTargetCellVirtualAddress),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Reread descriptor begin after the copy-target call",
                PostCopyBeginReadRawFileOffset,
                "8B 0F"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Reread descriptor end after the copy-target call",
                PostCopyEndReadRawFileOffset,
                "8B 47 04"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Compute retained DWORD count with a signed arithmetic shift",
                RetainedCountArithmeticShiftRawFileOffset,
                "C1 F8 02"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Null later begin skips the release-helper call",
                0x006A595A,
                "74 05",
                BranchTargetRawFileOffset: 0x006A5961),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Call the release helper only for a nonzero later begin",
                ReleaseHelperCallRawFileOffset,
                "E8 FF 33 C2 00",
                CallTargetRawFileOffset: ReleaseHelperTargetRawFileOffset),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Compute storage end as new base plus requested DWORD count times four",
                StorageEndValueComputeRawFileOffset,
                "8D 04 9E"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Publish new begin first",
                BeginPublicationRawFileOffset,
                "89 37"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Publish new storage end second",
                StorageEndPublicationRawFileOffset,
                "89 47 08"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Compute retained end as new base plus retained DWORD count times four",
                EndValueComputeRawFileOffset,
                "8D 04 86"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Publish retained end third",
                EndPublicationRawFileOffset,
                "89 47 04"),
            new UiShaderSourcePoolResetStorageInstructionEvidence(
                "Reach the normal RET 4 boundary",
                NormalReturnRawFileOffset,
                "C2 04 00")
        });

    public static UiShaderSourcePoolResetStorageProjection Project(
        UiShaderSourcePoolResetStorageCapture capture)
    {
        if (capture.RequestedDwordCount > MaximumRequestedDwordCount)
        {
            EnsureNoUnreachedCaptures(capture, allocatorCallReached: false);
            return StopAtFailureReport(capture, allocationCallReached: false, requestedBytes: null);
        }

        uint requestedBytes = unchecked(capture.RequestedDwordCount * 4);
        bool allocationCallReached = capture.RequestedDwordCount != 0;
        uint newBasePointer;
        if (allocationCallReached)
        {
            if (!capture.AllocatorReturnedBasePointer.HasValue)
            {
                throw new ArgumentException(
                    "The reached allocator call requires its independently captured return pointer.",
                    nameof(capture));
            }

            newBasePointer = capture.AllocatorReturnedBasePointer.Value;
        }
        else
        {
            if (capture.AllocatorReturnedBasePointer.HasValue)
            {
                throw new ArgumentException(
                    "The zero-count branch skips the allocator call.",
                    nameof(capture));
            }

            newBasePointer = 0;
        }

        if (allocationCallReached && newBasePointer == 0)
        {
            EnsureNoUnreachedCaptures(capture, allocatorCallReached: true);
            return StopAtFailureReport(
                capture,
                allocationCallReached: true,
                requestedBytes: requestedBytes);
        }

        if (!capture.InitialDescriptorBeginPointer.HasValue
            || !capture.InitialDescriptorEndPointer.HasValue)
        {
            throw new ArgumentException(
                "The copy call requires separate captures of the initial descriptor begin and end.",
                nameof(capture));
        }

        if (capture.CopyTargetReturnedNormally != true)
        {
            if (capture.PostCopyDescriptorBeginPointer.HasValue
                || capture.PostCopyDescriptorEndPointer.HasValue
                || capture.ReleaseHelperReturnedNormally.HasValue)
            {
                throw new ArgumentException(
                    "Post-copy and release-helper captures require an explicit normal copy-target return.",
                    nameof(capture));
            }
        }
        else if (!capture.PostCopyDescriptorBeginPointer.HasValue
            || !capture.PostCopyDescriptorEndPointer.HasValue)
        {
            throw new ArgumentException(
                "A normal copy-target return requires separate later descriptor begin and end captures.",
                nameof(capture));
        }

        uint copyLength = unchecked(
            capture.InitialDescriptorEndPointer.Value
            - capture.InitialDescriptorBeginPointer.Value) & 0xFFFFFFFC;

        if (!capture.CopyTargetReturnedNormally.HasValue)
        {
            return CreateProjection(
                capture,
                allocationCallReached,
                requestedBytes,
                newBasePointer,
                copyLength,
                copyTargetReturnedNormally: null,
                postCopyBegin: null,
                postCopyEnd: null,
                retainedCount: null,
                releaseHelperCallReached: false,
                releaseHelperReturnedNormally: null,
                boundary: UiShaderSourcePoolResetStorageBoundary.CopyCallBoundary);
        }

        if (!capture.CopyTargetReturnedNormally.Value)
        {
            return CreateProjection(
                capture,
                allocationCallReached,
                requestedBytes,
                newBasePointer,
                copyLength,
                copyTargetReturnedNormally: false,
                postCopyBegin: null,
                postCopyEnd: null,
                retainedCount: null,
                releaseHelperCallReached: false,
                releaseHelperReturnedNormally: null,
                boundary: UiShaderSourcePoolResetStorageBoundary.CopyTargetDidNotReturn);
        }

        uint postCopyBegin = capture.PostCopyDescriptorBeginPointer!.Value;
        uint postCopyEnd = capture.PostCopyDescriptorEndPointer!.Value;
        uint retainedCount = unchecked(
            (uint)(unchecked((int)(postCopyEnd - postCopyBegin)) >> 2));
        bool releaseHelperCallReached = postCopyBegin != 0;
        if (!releaseHelperCallReached)
        {
            if (capture.ReleaseHelperReturnedNormally.HasValue)
            {
                throw new ArgumentException(
                    "The null later-begin branch skips the release-helper call.",
                    nameof(capture));
            }

            return CreateProjection(
                capture,
                allocationCallReached,
                requestedBytes,
                newBasePointer,
                copyLength,
                copyTargetReturnedNormally: true,
                postCopyBegin: postCopyBegin,
                postCopyEnd: postCopyEnd,
                retainedCount: retainedCount,
                releaseHelperCallReached: false,
                releaseHelperReturnedNormally: null,
                boundary: UiShaderSourcePoolResetStorageBoundary.NormalReturn);
        }

        if (!capture.ReleaseHelperReturnedNormally.HasValue)
        {
            return CreateProjection(
                capture,
                allocationCallReached,
                requestedBytes,
                newBasePointer,
                copyLength,
                copyTargetReturnedNormally: true,
                postCopyBegin: postCopyBegin,
                postCopyEnd: postCopyEnd,
                retainedCount: retainedCount,
                releaseHelperCallReached: true,
                releaseHelperReturnedNormally: null,
                boundary: UiShaderSourcePoolResetStorageBoundary.ReleaseHelperCallBoundary);
        }

        if (!capture.ReleaseHelperReturnedNormally.Value)
        {
            return CreateProjection(
                capture,
                allocationCallReached,
                requestedBytes,
                newBasePointer,
                copyLength,
                copyTargetReturnedNormally: true,
                postCopyBegin: postCopyBegin,
                postCopyEnd: postCopyEnd,
                retainedCount: retainedCount,
                releaseHelperCallReached: true,
                releaseHelperReturnedNormally: false,
                boundary: UiShaderSourcePoolResetStorageBoundary.ReleaseHelperDidNotReturn);
        }

        return CreateProjection(
            capture,
            allocationCallReached,
            requestedBytes,
            newBasePointer,
            copyLength,
            copyTargetReturnedNormally: true,
            postCopyBegin: postCopyBegin,
            postCopyEnd: postCopyEnd,
            retainedCount: retainedCount,
            releaseHelperCallReached: true,
            releaseHelperReturnedNormally: true,
            boundary: UiShaderSourcePoolResetStorageBoundary.NormalReturn);
    }

    private static UiShaderSourcePoolResetStorageProjection StopAtFailureReport(
        UiShaderSourcePoolResetStorageCapture capture,
        bool allocationCallReached,
        uint? requestedBytes)
    {
        return new UiShaderSourcePoolResetStorageProjection(
            capture,
            allocationCallReached,
            requestedBytes,
            FailureReportCellCallReached: true,
            CopyCallReached: false,
            CopyTargetCellVirtualAddress: null,
            CopyNewBaseArgument: null,
            CopyOldBeginArgument: null,
            CopyLengthBytes: null,
            InitialCopyDescriptorReadsReached: false,
            InitialCopyDescriptorBeginRead: null,
            InitialCopyDescriptorEndRead: null,
            PostCopyDescriptorReadsReached: false,
            PostCopyDescriptorBeginRead: null,
            PostCopyDescriptorEndRead: null,
            RetainedDwordCount: null,
            ReleaseHelperCallReached: false,
            ReleaseHelperPointerArgument: null,
            ReleaseHelperReturnedNormally: null,
            BeginPublicationReached: false,
            PublishedBeginPointer: null,
            StorageEndPublicationReached: false,
            PublishedStorageEndPointer: null,
            EndPublicationReached: false,
            PublishedEndPointer: null,
            ReturnedEax: null,
            ReturnRawFileOffset: null,
            Boundary: UiShaderSourcePoolResetStorageBoundary.AllocationReportCellCallBoundary);
    }

    private static UiShaderSourcePoolResetStorageProjection CreateProjection(
        UiShaderSourcePoolResetStorageCapture capture,
        bool allocationCallReached,
        uint requestedBytes,
        uint newBasePointer,
        uint copyLength,
        bool? copyTargetReturnedNormally,
        uint? postCopyBegin,
        uint? postCopyEnd,
        uint? retainedCount,
        bool releaseHelperCallReached,
        bool? releaseHelperReturnedNormally,
        UiShaderSourcePoolResetStorageBoundary boundary)
    {
        bool copyCallReached = true;
        bool postCopyReadsReached = copyTargetReturnedNormally == true;
        bool publicationReached = boundary == UiShaderSourcePoolResetStorageBoundary.NormalReturn;
        uint? publishedStorageEnd = publicationReached
            ? unchecked(newBasePointer + requestedBytes)
            : null;
        uint? publishedEnd = publicationReached
            ? unchecked(newBasePointer + unchecked(retainedCount!.Value * 4))
            : null;

        return new UiShaderSourcePoolResetStorageProjection(
            capture,
            allocationCallReached,
            requestedBytes,
            FailureReportCellCallReached: false,
            CopyCallReached: copyCallReached,
            CopyTargetCellVirtualAddress: CopyTargetCellVirtualAddress,
            CopyNewBaseArgument: newBasePointer,
            CopyOldBeginArgument: capture.InitialDescriptorBeginPointer,
            CopyLengthBytes: copyLength,
            InitialCopyDescriptorReadsReached: true,
            InitialCopyDescriptorBeginRead: capture.InitialDescriptorBeginPointer,
            InitialCopyDescriptorEndRead: capture.InitialDescriptorEndPointer,
            PostCopyDescriptorReadsReached: postCopyReadsReached,
            PostCopyDescriptorBeginRead: postCopyBegin,
            PostCopyDescriptorEndRead: postCopyEnd,
            RetainedDwordCount: retainedCount,
            ReleaseHelperCallReached: releaseHelperCallReached,
            ReleaseHelperPointerArgument: releaseHelperCallReached ? postCopyBegin : null,
            ReleaseHelperReturnedNormally: releaseHelperReturnedNormally,
            BeginPublicationReached: publicationReached,
            PublishedBeginPointer: publicationReached ? newBasePointer : null,
            StorageEndPublicationReached: publicationReached,
            PublishedStorageEndPointer: publishedStorageEnd,
            EndPublicationReached: publicationReached,
            PublishedEndPointer: publishedEnd,
            ReturnedEax: publicationReached ? publishedEnd : null,
            ReturnRawFileOffset: publicationReached ? NormalReturnRawFileOffset : null,
            Boundary: boundary);
    }

    private static void EnsureNoUnreachedCaptures(
        UiShaderSourcePoolResetStorageCapture capture,
        bool allocatorCallReached)
    {
        if (capture.AllocatorReturnedBasePointer.HasValue != allocatorCallReached
            || capture.InitialDescriptorBeginPointer.HasValue
            || capture.InitialDescriptorEndPointer.HasValue
            || capture.CopyTargetReturnedNormally.HasValue
            || capture.PostCopyDescriptorBeginPointer.HasValue
            || capture.PostCopyDescriptorEndPointer.HasValue
            || capture.ReleaseHelperReturnedNormally.HasValue)
        {
            throw new ArgumentException(
                "Allocator, descriptor, and helper-return captures must match the paths reached before the report-cell boundary.",
                nameof(capture));
        }
    }
}
