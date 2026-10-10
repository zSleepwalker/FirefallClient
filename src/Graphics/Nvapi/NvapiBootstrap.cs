namespace Firefall.Reconstruction.Graphics.Nvapi;

// Behavioral reconstruction of a bounded native cluster, not an original API or source definition.
public sealed class NvapiBootstrap
{
    public const int Ok = 0;
    public const int Busy = -1;
    public const int OpenFailed = -2;
    public const int FunctionMissing = -3;
    public const int NotOpen = -4;
    public const int SnapshotRefreshed = -14;

    // Original PE virtual addresses of NUL-terminated strings (not IDA raw offsets).
    private const uint ExistingLibraryNameVa = 0x1B33FD4; // "nvapi.dll"
    private const uint FallbackLibraryNameVa = 0x1B33FE0; // "nvapi.dll"
    private const uint QueryInterfaceNameVa = 0x1B33FA4; // "nvapi_QueryInterface"
    private const uint InitializeInterfaceId = 0x150E828;
    private const uint ShutdownInterfaceId = 0xD22BDD7E;
    private const uint PreHookInterfaceId = 0x33C7358C;
    private const uint PostHookInterfaceId = 0x593E8644;
    private const uint ExampleOperationInterfaceId = 0x6C2D048C;

    private readonly IBackend _backend;
    private uint _currentHandle;
    private uint _resolver;
    private uint _preHook;
    private uint _postHook;
    private uint _exampleOperation;
    private Snapshot _slot0;
    private bool _closing;
    private bool _permanentOpenFailure;
    private int _inFlight;

    public NvapiBootstrap(IBackend backend) => _backend = backend;

    // 0x1F8B70. The handle is written only after passing the initial flags/wait.
    // A successful return (including -14) retains the in-flight increment for the caller.
    public int Open(ref uint handle)
    {
        if (_permanentOpenFailure)
            return OpenFailed;

        uint waits = 0;
        while (_closing)
        {
            _backend.Delay(100);
            if (++waits >= 10)
                return Busy;
        }

        ++_inFlight;
        handle = _backend.FindExisting(ExistingLibraryNameVa);
        if (handle == 0)
        {
            handle = _backend.OpenFallback(FallbackLibraryNameVa);
            if (handle == 0)
            {
                --_inFlight;
                _permanentOpenFailure = true;
                return OpenFailed;
            }
            return FinishChangedHandle(handle);
        }

        if (DetectAndStoreChange(handle))
            return FinishChangedHandle(handle);
        return Ok;
    }

    // 0x1F8920. This wrapper does not balance Open's retained increment.
    public int OpenNormalizePendingToOk()
    {
        uint handle = 0;
        int result = Open(ref handle);
        return result == SnapshotRefreshed ? Ok : result;
    }

    // 0x1F8E20. This flag/counter protocol is reproduced, not asserted thread-safe.
    public int Close()
    {
        _closing = true;
        if (_currentHandle == 0 || _resolver == 0)
        {
            _closing = false;
            return NotOpen;
        }
        if (_inFlight != 0)
        {
            _closing = false;
            return Busy;
        }

        uint shutdown = _backend.Resolve(_resolver, ShutdownInterfaceId);
        if (shutdown == 0)
        {
            _closing = false;
            return FunctionMissing;
        }
        if (_backend.CallNoArguments(shutdown) != 0)
        {
            _closing = false;
            return Busy;
        }

        _resolver = 0;
        _backend.CloseHandle(_currentHandle);
        _currentHandle = 0;
        _preHook = 0;
        _postHook = 0;
        _closing = false;
        return Ok;
    }

    // Representative two-argument relay, 0x1F8F70. Other observed relays use
    // the same skeleton with different tokens, signatures and cache globals.
    public int InvokeExample(int first, int second)
    {
        uint handle = 0;
        int status = Open(ref handle);
        if (status != Ok && status != SnapshotRefreshed)
            return status;
        if (status == SnapshotRefreshed)
            _exampleOperation = 0;
        if (_exampleOperation == 0 && handle != 0 && _resolver != 0)
            _exampleOperation = _backend.Resolve(_resolver, ExampleOperationInterfaceId);
        if (_exampleOperation == 0)
            return FunctionMissing; // No decrement in the observed early-exit path.

        HookScratch scratch = default;
        if (_preHook != 0)
            _backend.BeforeInvoke(_preHook, ExampleOperationInterfaceId, ref scratch);
        int result = _backend.InvokeTwo(_exampleOperation, first, second);
        if (_postHook != 0)
            _backend.AfterInvoke(_postHook, ExampleOperationInterfaceId, scratch, result);
        --_inFlight;
        return result;
    }

    private int FinishChangedHandle(uint handle)
    {
        int result = ResolveFunctionTable(handle);
        if (result != Ok)
        {
            --_inFlight;
            _backend.CloseHandle(handle);
            return result;
        }

        _currentHandle = handle;
        CaptureSlot(handle);
        return SnapshotRefreshed;
    }

    // 0x1F8A50, slot-zero branch. The export descriptor's bytes/name are unknown.
    private int ResolveFunctionTable(uint handle)
    {
        _resolver = _backend.LookupSymbol(handle, QueryInterfaceNameVa);
        if (_resolver == 0)
            return Busy;

        uint initialize = _backend.Resolve(_resolver, InitializeInterfaceId);
        if (initialize == 0 || _backend.CallNoArguments(initialize) != 0)
        {
            _resolver = 0;
            return Busy;
        }

        _preHook = _backend.Resolve(_resolver, PreHookInterfaceId);
        _postHook = _backend.Resolve(_resolver, PostHookInterfaceId);
        // The native branches clear both only when pre is present and post is absent.
        if (_preHook != 0 && _postHook == 0)
        {
            _preHook = 0;
            _postHook = 0;
        }
        return Ok;
    }

    // 0x1F8950 and 0x1F89A0. For a PE image, these are CheckSum and TimeDateStamp.
    private Snapshot ReadSnapshot(uint handle)
    {
        uint baseAddress = unchecked(handle + _backend.ReadWord(handle + 0x3C));
        return new Snapshot(
            _backend.ReadWord(unchecked(baseAddress + 0x58)),
            _backend.ReadWord(unchecked(baseAddress + 0x08)));
    }

    private void CaptureSlot(uint handle) => _slot0 = ReadSnapshot(handle);

    private bool DetectAndStoreChange(uint handle)
    {
        Snapshot candidate = ReadSnapshot(handle);
        bool changed = candidate.Checksum != 0
            ? candidate.Checksum != _slot0.Checksum
            : candidate.Timestamp != _slot0.Timestamp;
        if (changed)
            _slot0 = candidate; // Updated before the subsequent resolver can fail.
        return changed;
    }

    private readonly record struct Snapshot(uint Checksum, uint Timestamp);
    public readonly record struct HookScratch(uint First, uint Second);

    // Calls are indirect in the image. Library/symbol addresses are original
    // PE virtual addresses; interface IDs are numeric, not code pointers.
    public interface IBackend
    {
        void Delay(int amount);
        uint FindExisting(uint libraryNameVa);
        uint OpenFallback(uint libraryNameVa);
        void CloseHandle(uint handle);
        uint LookupSymbol(uint handle, uint symbolNameVa);
        uint Resolve(uint resolver, uint interfaceId);
        int CallNoArguments(uint function);
        uint ReadWord(uint address);
        void BeforeInvoke(uint hook, uint token, ref HookScratch scratch);
        void AfterInvoke(uint hook, uint token, HookScratch scratch, int result);
        int InvokeTwo(uint function, int first, int second);
    }
}
