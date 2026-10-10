# Firefall container reconstruction

`FirefallContainers.cs` models the bounded 32-bit container/string primitives from `FirefallClient.exe`. IDA loaded this PE32 image as a raw file segment at address `0`; **the addresses below are raw file offsets, not PE virtual addresses or RVAs**. Function offsets refer to raw code bytes. For absolute data operands, first translate `VA -> RVA -> raw file offset` using `docs\BinaryLayout.md`; do not treat the operand value as a raw offset. The allocator interface represents observed native calls and memory access; it deliberately does not supply a guessed allocator implementation.

Build the standalone library from the repository root with:

```powershell
dotnet build src\Containers\Firefall.Containers.csproj
```

| IDA address | Evidence-based mapping |
| --- | --- |
| `0x6860` | `EnsureCapacity`: desired element count is multiplied by element size. For a negative tagged capacity it allocates and copies `Count * elementSize`; otherwise it calls the allocator's resize slot with old capacity in bytes. Stores and returns the allocator-adjusted capacity (`requestedBytes / elementSize`). It does not update count or itself check whether growth is needed. |
| `0x68e0` | `GrowWithPolicy`: requests one element when count is zero, otherwise `Count * 2`, then applies the same allocation/resize branches as `0x6860`. The policy is based on count, not capacity. |
| `0x5a00` | `ReserveAtLeast`: masks capacity with `0x3fffffff`, returns it if already sufficient; otherwise requests `max(minimumCount, capacity * 2)` through `0x6860` with element size one. |
| `0x5800` | `ClearString`: sets the source pointer to null and the length mode to `-1`, then calls `0x5720`. The callee frees a previously owned string and stores zero, subject to its same-pointer fast path. |
| `0x5720` | String assignment core: compares source with the old tagged pointer minus its low-bit tag; frees old storage only when the old pointer is tagged odd; duplicates a non-null source and stores the returned data pointer plus one. `maximumLength == -1` selects full C-string duplication; other values select a bounded prefix. |
| `0x56b0`, `0x56e0` | Full-string and bounded-prefix duplication helpers used by `0x5720`. The full path allocates `strlen + 1` and copies through the terminator. The prefix path clamps `strlen` to the supplied maximum, copies that many bytes, then writes NUL. |
| `0x28a0` | Allocates a payload with 16 bytes of preceding metadata: requests `payloadBytes + 16` from allocator vtable slot `+4`, writes the allocator pointer at header offset `0` and payload size at `+4`, and returns base `+16`. |
| `0x28f0` | If passed a non-null payload, derives base as payload `-16`, reads payload size at base `+4`, zeroes the first header word, and invokes allocator vtable slot `+8` to free base with size `payloadBytes + 16`. The helper uses the allocator argument rather than the stored allocator word. |
| `0x159d8f4` | IDA-named `Runtime_CopyBytesThunk`: one raw-code instruction, `jmp ds:[0x1AC4AD0]`. The two array primitives call this location with destination, source, and byte count. `0x1AC4AD0` is an absolute data operand (VA), not an IDA raw offset; this reconstruction does not identify its target from the raw-map reference. |

## Layouts and conventions

* `DynArray32` is three 32-bit words at offsets `+0` data pointer, `+4` element count, and `+8` capacity/tag. The low 30 capacity bits are used for the existing byte count; a negative word selects allocate-and-copy rather than resize. Successful growth replaces the capacity word with the returned capacity, so the negative tag is not retained.
* `DynString32` is one 32-bit tagged pointer. An odd value represents an owned allocation; its data pointer is the stored value minus one. The code duplicates assigned text rather than retaining the caller's pointer.
* The string helpers' metadata header is 16 bytes. Only the first two words are read/written by the observed functions; no meaning is inferred for the other eight bytes.
* `IAllocator32` exposes separate array allocation/resize operations and raw allocation/free operations because the observed code invokes different vtable slots (`+12`/`+20` versus `+4`/`+8`). The byte-copy and C-string operations stand in for the corresponding native calls. The array code's call signature and IDA thunk name support a copy routine interpretation, but its absolute indirect target is unresolved here.

## Uncertainties and boundaries

These are behavioral reconstructions, not safe replacements for the native runtime. The disassembly has no visible checks for zero element size, arithmetic overflow, allocation failure, invalid pointers, or negative prefix lengths; callers/runtime contracts for those cases are unknown. In particular, the C# division by zero behavior is not asserted to match the native failure mode. The native allocator's capacity rounding and failure conventions are also unknown; the model preserves its in/out byte-count argument so an implementation can express that behavior. The names are independently checked against the instructions: `0x5800` passes `(-1, null, this)` to `0x5720`; the other names in the table are descriptive reconstruction names, not claims about original symbols.
