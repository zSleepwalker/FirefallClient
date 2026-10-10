# 32-bit hash-slot table reconstruction

`HashSlotTable32.cs` models one bounded cluster from the **raw-mapped, base-zero**
`FirefallClient.exe` IDA database. Addresses below are **file offsets**, not PE
virtual addresses. It does not change the IDA database or reproduce the native
allocator/pointer representation.

## Address translation and limits

Read-only inspection of the PE headers in the mapped bytes confirms image base
`0x400000`: `.text` RVA `0x1000` / raw `0x400`, `.rdata` RVA
`0x16c4000` / raw `0x16c2c00`, and `.data` RVA `0x19c5000` /
raw `0x19c3400`. For example, the instruction at raw `0x3790`
corresponds to VA `0x404390`, **not** VA `0x3790`. Likewise, VA
`0x1c5a3b8` maps into `.rdata` at raw `0x1858fb8`, not raw
`0x1c5a3b8` (which is in `.rsrc`).

The table operations below use register-relative slot, header, and allocator
accesses plus relative code calls; none of their algorithmic conclusions
requires trusting an IDA absolute data xref. Convenience wrappers at raw
`0x3880`, `0x38a0`, and `0x3fd0` call the runtime-context getter at
raw `0x2970` and load an allocator from the returned context at `+0x2c`.
The getter's **instruction bytes** refer to absolute VAs `0x1e37248`
(state handle) and `0x1ac4300` (indirect call). The latter maps to
`.rdata` raw `0x16c2f00`, not raw `0x1ac4300`. VA `0x1e37248`
lies in the virtual `.data` section but beyond its raw file-backed extent;
its runtime contents cannot be recovered from these raw bytes. The identity,
initialization, and precise ABI of that getter and allocator remain unverified;
the C# allocator is only an explicit boundary for the observed calls.

| File offset | Verified behavior |
| --- | --- |
| `0x36a0` | Clear each key to `0xffffffff`, retaining payload words and the high bit of the count word. |
| `0x36d0` | Allocate at least eight 8-byte slots; mark keys empty, reset count, reinsert old occupied slots, release old storage only if the old count's high bit was clear. Does **not** round arbitrary requests to a power of two. |
| `0x3790` | If `2 * (count & 0x7fffffff) > mask`, rebuild with `2 * mask + 2` slots (at least eight). Probe by `((key >> 4) * 0x9e3779b1) & mask`, advancing `(index + 1) & mask`; insert or overwrite payload, returning 1 or 0 respectively. |
| `0x3810` | Clear first, call allocator vtable `+8` with pointer and `8 * (mask + 1)` bytes unless high bit is set, then reset pointer/count/mask to `0/0/-1`. |
| `0x3860` | Reserve via the smallest power of two at least eight and at least twice the requested entry count, then rebuild. |
| `0x101f0` | Find slot index; return `mask + 1` on miss (zero in an unallocated table). |
| `0x10230` | Return the matching payload or the caller-provided fallback. |
| `0x2f540` | Companion insert-if-absent returns slot index, preserving existing payload; unlike `0x3790`, it does not overwrite on a hit. |

Native header: 12 bytes at `+0` slots pointer, `+4` count with high-bit
non-owning/storage tag, `+8` slot mask (`slot count - 1`; initially `-1`).
Each slot: 32-bit key at `+0`, 32-bit payload at `+4`. `0xffffffff` is the
empty key. There is no observed tombstone or deletion in this cluster.
The rebuild allocator vtable `+4` takes slot count times eight; `+8` takes
old pointer and old byte size. Rebuild clears the tag on newly allocated
storage even when the old storage was borrowed.

The model uses managed arrays instead of native pointers and rejects the
reserved key, non-power-of-two capacity, overflowing capacity, and allocator
size mismatch; the native code does not visibly validate these. Lookup and
insertion assume a free slot exists; behavior of malformed/full tables,
allocation failure, integer overflow, and the allocator's handling of
`Release(null, 0)` on initial allocation are unknown. Native return registers
for clear/rebuild/reset are incidental loop or allocator results, not modeled
as a stable API. IDA's decompilation of `0x3790` misleadingly omits the
`and ecx, 0x7fffffff` before the growth test: this reconstruction follows
the verified instructions. Names are descriptive, not recovered symbols.

Build the self-contained library with
`dotnet build src\Collections\Firefall.Collections.csproj`; run the synthetic
collision, growth, ownership, and lookup checks with
`dotnet run --project src\Collections\Checks\CollectionsChecks.csproj`.
