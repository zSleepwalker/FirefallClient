# HWStats DirectX registry buffer decisions

Static inspection of `HWStats.exe`, 594,648 bytes, SHA256
`0AE559DA740DE2C51A77A652906F021833A17E2A634757C15A6ADE5BEB9D679D`.
Addresses are preferred PE VAs (base `0x400000`).
`src\HWStats\DirectXVersionNativeLayout.cs` preserves the query-buffer
decisions, not a registry reader or text formatter.

## Registry call and buffer setup

After calling the OS-description builder, System section `0x404020`
zeroes the first WORD of a 128-byte buffer at `0x404054` and clears the
remaining 126 bytes at `0x404062`. It opens HKLM
`SOFTWARE\Microsoft\DirectX` (wide literal `0x443A98`) through
`RegOpenKeyExW` at `0x404082`, with access `0x20019` (`KEY_READ`).
Open failure jumps directly to combined formatting with that initialized
empty DirectX buffer.

On successful open, `0x404092` sets the byte-count local to **127**,
not 128 or a wide-character count. `RegQueryValueExW` at `0x4040B3`
queries `Version` (wide literal `0x443AD0`), with null reserved and null
type output pointers. The query's status is not checked. There is no
type validation, environment expansion, retry or size-based allocation.

## Doubled count, not a byte-offset terminator

Immediately after the query, `0x4040B9` loads its observed byte-count
output; `0x4040BF` doubles that DWORD with native wraparound. It compares
the doubled result to **128**, unsigned:

| Doubled count | Next action |
|---|---|
| At least 128 | Branch `0x4040C6` to the range-check-failure call at `0x4041C9`; no ordinary key-close path first |
| Below 128 | Store a zero WORD at buffer base plus the doubled count (`0x4040D4`), then close key (`0x4040DC`) |

This treats the query's byte count like a WCHAR index for the terminator
and bound. The source preserves that arithmetic rather than replacing it
with a normal byte-capacity check. For ordinary non-wrapping values, 63
bytes pass and write the terminator at byte offset 126, while 64 bytes
take the failure branch. A query failure that leaves the input count
127 unchanged therefore also takes that branch; no deterministic
query-failure output is assumed.

Synthetic `0x80000000` doubles to zero. This demonstrates DWORD arithmetic
only, not a claim that the real query returns such a size. The pure model
accepts supplied observations and labels range-check failure explicitly;
it never invokes the native failure handler.

## Outer wide formatting

At `0x4040E2`-`0x4040FC`, the caller supplies the DirectX buffer and the
OS-description buffer to imported `_swprintf`, with literal
`%s, DirectX Version %s` at `0x443AE0`. There is no capacity argument in
this imported formatter call. The destination lies in the separate
256-WCHAR caller region at `[ebp-0x298]`. Exact maximum combined length
and CRT failure behavior are not reconstructed here.

The OS builder result was already ignored at `0x40404F`; only the
DirectX buffer is initialized on this path. An empty DirectX value
therefore does not make the entire OS/DirectX output deterministic after
an OS-builder failure.

## Evidence and bounds

Hash-guarded companion checks bind the zeroing, HKLM/access arguments,
wide key/value literals, query setup, unchecked status, doubled-count
comparison, terminator store, close and failure calls, and formatter
import. Pure cases cover failed open, count zero, 63/64-byte boundary,
unchanged 127 and synthetic unsigned wraparound.

No sample was executed, no registry was accessed, and no native buffer or
formatter was invoked.
