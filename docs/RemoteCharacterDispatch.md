# Remote-character dispatch helpers (partial)

Addresses below are raw IDA file offsets; for `.text`, PE VA = raw + `0x400C00`.
The observations cover selected remote-observer dispatch helpers and the
already-modeled Character messages 174, 175, and 194. These helpers do not
establish the original native class identity or the type/semantics of every
descriptor field.

## Shared observer gate

The three helper gates take the remote-observer pointer in `ECX` and a
descriptor pointer as their explicit second argument. Each returns `0` without
sending when the DWORD at observer `+0x820` is zero or the signed DWORD at
`+0x0C` is at most 3. On the permitted path, byte `+0x14` bit `0x08` enables
an optional log, then the gate invokes its message sender and returns `1`.

| Message | Gate | Sender | Sender call |
|---|---:|---:|---|
| `LogDirectActivityRequest` (174 / `0xAE`) | `Character_GateAndSend_LogDirectActivityRequest` (`0x90CBC0`) | `0x90EE20` | Descriptor and observer pointer |
| `LogActivityPush` (175 / `0xAF`) | `Character_GateAndSend_LogActivityPush` (`0x90CC30`) | `0x90EF50` | Descriptor and observer pointer |
| `UpdateShoppingList` (194 / `0xC2`) | `Character_GateAndSend_UpdateShoppingList` (`0x90CF60`) | `0x90F260` | Descriptor and observer pointer |

The senders serialize descriptor DWORDs at `+0x18` and `+0x1C` and the message
code. The observer argument is not used by the wire encoders. Message-specific
payloads are documented in [`src\Characters\README.md`](../src/Characters/README.md)
and are covered by the Character checks.

Direct gate paths include observer-selection thunks and callbacks from
`0x908F20` / `0x908FA0`, as well as producers at `0xA2D680` / `0xA2D770`.
The short thunks return `0` when no remote observer is selected. The C2194 gate
is reached from a short thunk at `0x90C4A0`.

## Inline shopping-list builder

`Character_BuildAndDispatch_UpdateShoppingList` (`0x9095B0`, PE VA `0xD0A1B0`)
has no explicit stack arguments; `ECX` points to a shopping-list state object.
Its fields are an ordered-map head pointer at `+0`, map size at `+4`, dirty
byte at `+8`, flush interval at `+0x0C`, and last-send tick at `+0x10`.
Callers `0x49B3D0` and `0x4A7980` reach it only when dirty and the elapsed
time meets the interval; they clear the dirty flag and advance the last-send
tick by whole intervals before calling it.

The builder walks the source outer map in key order. Each outer entry contains
two DWORDs at `+0x10/+0x14`, with the second DWORD used as the map key, and an
embedded inner set at `+0x18/+0x1C`. For each outer entry it inserts the
inner-set payloads into temporary set L2, then inserts the outer entry's
two-DWORD value into temporary set L1. It copy-constructs both sets for the
message descriptor at `+0x28` (L1) and `+0x30` (L2), without transforming
their elements.

L1 is unique and ordered by its second DWORD; since the source map already
uses that same unique key, it is effectively copied in ascending key order.
L2 is a union of all inner sets, unique and ordered lexicographically by a
DWORD and byte. The sender `0x910040` writes the context DWORDs at descriptor
`+0x18/+0x1C` as one eight-byte value, then message ID `0xC2`, then chunked
L1 and L2. For each list, it emits a byte count up to 255 followed by that
chunk's entries, repeating while the count is 255; empty lists emit one zero
byte and exact multiples of 255 end with a zero chunk.

The context value comes from a client-side object and a second object reached
through it. Its low DWORD has its lowest byte replaced with `4`; the resulting
64-bit value is checked against zero and the all-ones invalid sentinel. The
type-4 routing-key pattern suggests an entity identifier, but its exact
identity is not established here.

The source methods provide a partial population path. `0x909120` rejects
duplicate second-word keys and a zero key; otherwise it clears the current map,
marks the state dirty, and calls `0x9092A0`. That method inserts an outer entry
and invokes recursive component resolution at `0x9D4C00`; resolved components
are inserted into the entry's inner set as `{DWORD, byte}` values. On success,
the path marks the state dirty and fires UI event 219; on failure it removes
the entry. Exact meanings of the key, first DWORD, component value, and byte
remain uncertain.

## Separate adjacent state collections

The shopping state also contains distinct structures that are not part of the
UpdateShoppingList payload:

| State offset | Observed use | Status |
|---:|---|---|
| `+0x00` | Persistent ordered outer map serialized as message 194 | Source analyzed above |
| `+0x24` | Collection cleared by state destructor `0x908D30` | Element semantics unknown |
| `+0x2C` | Text-keyed hash map with two DWORD payload values | Updated by `0xACDD40` / `0x909AE0`; not the message-194 source |
| `+0x4C` | Ordered tree of shared-text keys and vectors of 16-byte records | Read by script/view path `0x9F65E0`; record field meanings unknown |

`0x4A0700` is reset/teardown, not a second producer: it destroys the state
object at owner `+0x43C8`, then clears that owner field. Its destructor clears
the `+0x4C` tree, `+0x2C` text-key map, `+0x24` collection, and the persistent
`+0x00` ordered map.

Reader `0xA242A0` walks the persistent `+0x00` map and invokes `0x9D4C00`
with a stack-local output set for each outer key. It emits computed view data
only on successful resolution; this does not mutate the persistent inner set.
By contrast, `0x9092A0` passes the inner set embedded in a newly inserted
persistent outer-map node to the same resolver.

`0xACDD40` updates the separate text-keyed `+0x2C` map using a supplied
shared-text key and two DWORD values, then calls `0x909B40`. That formatter
uses one stored value and fires event 230; the second stored value's meaning
is unresolved. `0x909AE0` is another callable writer for this same map, but
its callers were not found. Script path `0x9F65E0` reads the separate `+0x4C`
tree and consults the `+0x2C` map to format an output record; VM property
names remain unknown. These structures must not be merged into the
outer-map message model.

After the conditional send (or skip), four cleanup paths destroy the two
message-copy sets and two temporary sets: `0x5AC520`/`0x5AC210` erase elements,
then `0x12C8D60` frees each set head. These are lifetime cleanup, not additional
dispatches.

The builder has callers `0x49B3D0` (callsite `0x49B40A`) and `0x4A7980`
(callsite `0x4A7EE3`, PE VA `0x8A8AE3`); the former accesses state at its
current object, while the latter finds it at owner offset `+0x43C8`. Other
population paths were not followed. `src\Characters\CharacterObserverDispatchLayout.cs`
records the verified message gates, sender addresses, builder/source offsets,
and observed observer/descriptor fields.

## Unresolved

- The semantic types and names of the observer fields at `+0x820`, `+0x0C`,
  and `+0x14`.
- Exact semantic identity of the 64-bit context/routing value.
- Whether outer-map `A` is a mode/count flag and `B` a recipe/blueprint ID;
  the Lua-facing single-add path supplies `A=1`, but names/meaning are not
  independently confirmed.
- Whether L2's DWORD is a resource/item ID and its byte a quality/tier value.
- Other callers of the callable `+0x2C` map writer `0x909AE0` and the meaning
  of its second stored DWORD.
- Whether any untraced path outside the candidate callers populates the
  persistent `+0x00` map; none of the examined alternate candidates did so.

The C# catalog and message models preserve the verified field widths,
ordering, uniqueness, and context-key construction without assigning
speculative semantic names to unresolved values.
