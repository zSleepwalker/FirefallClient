# Opaque auth forwarding into DumpTruck

Static, read-only analysis of the identified original FirefallClient,
RedHanded, and DumpTruck samples. Identity hashes and address conventions
are recorded in [the companion ledger](CompanionBinaryConnections.md).
Addresses below are preferred PE VAs except the explicitly labeled
original-client raw offsets. No registry values, live credentials, or
service responses were accessed.

## Forwarding chain

1. Original-client raw `0x63B15B` emits the static format
   `RED5-DUMPAUTH=!%s\n` through OutputDebugStringA.
2. RedHanded's ANSI debug-event reader assigns bytes beginning at offset14
   into debugger owner `+0x10`, retaining the exclamation. See
   [debug-string producer/consumer evidence](RedHanded.md#client-debug-string-handoff).
3. RedHanded wWinMain (`0x404540`) selects that owner field at
   `0x404BF2..0x404BFD` and pairs it with `--auth=` at
   `0x404C90..0x404C94`. The variadic launcher is called at `0x404CAC`.
4. Launcher `0x4042D0` uses the builder `0x4041D0`, which pairs an option
   ending in `=` with the next value. Helper `0x404130` quotes the joined
   argument. Byte copier `0x4040C0` escapes quote and certain backslash
   cases; `!` is an ordinary byte. CreateProcessA at `0x404391` uses the
   named executable directly with flag8 (DETACHED_PROCESS), not a shell.
5. DumpTruck parses `auth` at `0x405403`, storing it at options owner
   `+0x08` via `0x405412`. This is a different object from RedHanded's
   debugger owner; equal offsets elsewhere do not establish object identity.

## Two distinct empty-auth fallbacks

The options initializer checks forge flag byte `+0x14` at `0x405479`,
then auth emptiness via `0x409240` at `0x405482`. Only forge plus empty
auth invokes stored-credential helper `0x404850` at `0x405491`.
Existing nonempty auth is not replaced by this branch. The helper accesses
the Dump Truck registry settings and additional local credential-processing
routines; their complete algorithm is not reconstructed here.

Independently, the submit command checks current auth emptiness at
`0x403B8B`. Empty auth invokes `0x4024E0`, a registry **LoginName reader**,
not a dialog prompt or authentication request. That function queries
HKCU's Firefall settings using a zero-initialized1024-byte buffer and
cbData1023. A nonempty returned name is formatted with `!%s` at
`0x403BD2` and assigned into options `+0x08`. Empty output leaves auth
empty and the handler continues; this local branch does not reject
submission. We do not infer server-side permission from the marker or
from this continuation.

The shared-text empty predicate `0x409240` returns true for a null
pointer or zero stored length; a nonzero short-header discriminator
selects a16-bit length, otherwise a32-bit length.

## XML preparation occurs before request formatting

Dialog code passes the option auth to request-context constructor
`0x409830` at `0x403E1C`. The constructor passes its third argument
through `0x409460` at `0x40995A`, assigning the result to request-context
`+0x08` at `0x40996B`.

The helper first copies the source C string (strlen-visible bytes only),
then calls `0x410BB0` three times:

| Call VA | Match | Replacement |
|---|---|---|
| `0x4094FE` | `&` | `&amp;` |
| `0x409510` | `<` | `&lt;` |
| `0x409522` | `>` | `&gt;` |

Replacement advances beyond the inserted text, so each pass does not
reprocess its own replacement. Ampersands are handled first, avoiding
an extra escape of entities inserted by the later angle-bracket passes.
Already-entity-looking input is deliberately escaped again. Quotes,
apostrophes, newline, and `!` are not modified by these three passes.

Crash-request builder `0x409A60` then selects context `+0x08` at
`0x409B78` and supplies it to formatter `0x44B820` at `0x409B8C`,
with media type `application/xml` and template
`<report-crash><auth>%s</auth></report-crash>`.
Thus this template's auth is **preescaped**; the marker is not removed
on the traced constructor-to-format path. Any interpretation by the
server, and later request-signing or confirmation handling, remain
separate questions.

`src\DumpTruck\AuthHandoffNativeLayout.cs` preserves initialization
and submit fallback decisions and the observed C-string-visible XML
transformation. It performs no native calls or uploads. Checks use
synthetic noncredential text, and sample checks verify native gates,
call targets, literal replacements, and forwarding offsets.
