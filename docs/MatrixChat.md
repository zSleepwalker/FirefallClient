# Matrix chat, message bodies, and file logging

## Initial bodies34/36 and catalog-wide wire coverage

`MatrixServerMessages34And36Wire.cs` closes the two remaining initial body
gaps. Code34 factory `0x33D4F4`, ctor `0x339950` allocates40h; reader
`0x357180` reads a WORD-length-prefixed **opaque blob** at descriptor+28,
then one raw byte+38. Minimum packet4; the full unsigned WORD length65535
is supported. Blob contents may include NULs and are not interpreted as
text or nested messages.

Handler `0x490AA0` normalizes the byte by `!=0`, then calls manager consumer
`0x30EE20`. This obtains the incoming blob's pointer and length through its
interface and always invokes the **embedded slBlob at manager+C44**
virtual+18 operation, which appends pointer/length bytes.
If normalized flag is nonzero, it allocates an8-byte pointer/length pair at
manager+C3C, copies the **whole accumulated manager buffer**, invokes its
virtual+14 **no-argument clear**, and sets pending byte+C38=1.
Flag0 does not clear an existing pending byte or snapshot pointer.
`MatrixBufferSnapshotNativeLayout.cs` saves selection, offsets and logical
append/snapshot/clear projection. **Snapshot bytes include earlier chunks**,
not necessarily only the newest incoming blob. IDA's inferred memcpy signature drops
the length in pseudocode; original instructions explicitly push the sampled
length before source/destination, so the zero-length-looking decompilation
is not evidence for an empty copy.

The application constructor writes vtable VA`0x1C80CC4` at+C44; RTTI
identifies **slBlob**. Table slots+4/+C/+14/+18 resolve respectively to
`0x12CB960` writable data at offset, `0x12CB9C0` logical length,
`0x12CBA00` clear length, and `0x12CBA10` append. Append resizes to
old length+incoming length with preserve flag1, then copies incoming bytes
to old end. Clear calls the shared resizer`0x12CBCB0` with length0/preserve0.
Resize can retain uniquely owned allocation capacity or detach/release shared
storage; the C# projection models **logical bytes**, not that allocation
policy, copy-on-write/refcounts, native pointer validity or out-of-memory behavior.
The earlier inferred "consume sampled length" call was a decompiler
argument mistake: original instructions push no argument at virtual+14.

Snapshot helper`0x30EDA0` returns null for a null pair, otherwise allocates
an independent8-byte pair and a separate data allocation and copies all
bytes. The returned snapshot is not a borrowed pointer to cached storage;
caller-side freeing of that returned pair was not traced here.
Request helper`0x30EED0`, after its startup-ready wait and native lock,
reuses the cached pair only if key+C40 equals the requested DWORD and
pair+C3C exists; this test does not use pending byte+C38. Cache misses clear
slBlob and explicitly free old cached data and pair, zero key/pointer,
prepare outgoing message32, clear pending, then wait for the message34
completion. On completion it clones the pair and records the key.
It polls in100ms sleeps; shutdown or app mode1 aborts the response wait.
Missing send prerequisites do not produce an invented immediate success.
The startup wait/callback synchronization and
returned-snapshot caller lifetime remain outside the pure projection.

### Outgoing32 RequestSigscan and incoming34 SigscanData

RTTI names request32 `matrix_fury::RequestSigscan` (vtable VA`0x1C62384`)
and response34 `matrix_fury::SigscanData` (vtable VA`0x1C62354`).
These names identify the native traffic; response bytes still remain opaque,
and no actual signature scanning or live requests are performed by the C#.

Encoder`0x31E7B0` emits exactly **six normal application-buffer bytes**:
compact code32, LE32 descriptor+28 cache-key word, raw byte+2C.
It has **no eight-byte entity/context prefix**. Body reader`0x345D20`
mirrors DWORD then byte. Sender`0x31D350` submits that serialized buffer
on application channel1. Outer socket framing remains the separate saved
application-buffer pipeline.
`Client_RequestOrCloneBufferSnapshot` supplies its second argument as the
key and sets byte+2C to1 iff its first argument equals0, otherwise0.
Neither flag's original semantic name nor cache key's original identity
is established; they are not called a nonce, address or scan mode.
`MatrixRequestSigscan32Wire.cs` saves exact raw encode/decode and this
argument mapping with explicit truncated/noncanonical/wrong-code outcomes.

Code36 factory `0x33D5BA`, ctor `0x339C80` allocates30h; the factory directly
reads NUL raw texts+28/+2C through `0x12EE010`. Minimum packet3.
Original handler `0x491400` logs `"Announcement type='%s' data='%s'"`.
Only exact case-sensitive type `"sysmsg"` gets the current mode-specific
object and, if present, calls `0x6E3460` with the data text and fixed label
`"broadcast"`; other types are log-only in this handler. Native null shared
refs substitute empty strings. `MatrixAnnouncementNativeLayout.cs` saves
the byte-exact filter, not text decoding or the full downstream display.

Every one of the35 dispatcher IDs **34..68** now has a saved normal-buffer
body decoder. The cross-catalog check exercises the exact minimum packet
and rejects its one-byte-short prefix for each route, in addition to
individual nonempty/chunk-boundary/malformed-input cases. This is body-layout
coverage, **not** proof of all field meanings, live interoperability,
alternate-header-buffer support, complete native ownership or full consumers.
The [body index](MatrixServerMessageBodies.md) maps each range to its C# API.

## Shared chat display: announcement36 and zone67

Consumer `0x6E3460` posts runtime**335**, then calls `0x6F46F0` after the
runtime callback. Its original property names prove:

| Property | Observed source / condition |
|---|---|
| `"channel"` | Label argument; omitted only when null/empty |
| `"author"` | Author-name argument; always added, empty for native null |
| `"text"` | Message-text argument; always added, empty for native null |
| `"author_id"` | QWORD author argument; omitted only for0, no upper56-ID validity filter |
| `"dev"` | Flags bit0 |
| `"mentor"` | Flags bit1 |
| `"ranger"` | Flags bit2 |
| `"publisher"` | Flags bit3 |
| `"use_encounter_icon"` | Flags bit6 |

The native flag argument is a byte. Message67 reads a DWORD on wire but only
the low-byte masks above contribute to this notification; bits4/5/7 and all
higher DWORD bits are not interpreted as extra role flags by this consumer.
All nonzero author IDs, including upper56-all-FF values, reach `"author_id"`;
do not reuse the challenge/LFG stored-ID validity tests here.

Channel parsing through `0x80CD70` only suppresses three recognized categories:
2 `"COMMUNITY"` requires global VA`0x1E5E42C` object byte+40;
4 `"ZONE_LANG"` requires mode byte+14;
11 `"DEBUG"` requires global VA`0x1E5E4BC` object byte+40.
Other recognized categories and unrecognized labels pass this selection.
Native globals on those filtered paths are dereferenced, not optional values.
The C# gate takes the parser's result and already-sampled enable bytes; it
does not substitute for native pointer validity or reconstruct the imported
text comparator. PE imports now confirm comparator VA`0x1AC48A8` is
MSVCR120.dll `_stricmp`; native locale-dependent comparison is not replaced
by an assumed managed ordinal comparison.

`MatrixChatDisplayNativeLayout.cs` saves the property/flag projection and
gates. Zone67 selects `"zone"` with decoded author/text/ID/flags. Selected
`"sysmsg"` announcement36 selects `"broadcast"`, empty author, omitted ID
and zero flags. An unselected announcement is rejected by the capture API,
not fabricated as a broadcast. These captures do not invoke runtime callbacks,
simulate their reentrancy, or model the post-notification `0x6F46F0` consumer.

## Structured chat consumer switch (raw 0x6E3670)

Evidence: read-only disassembly of
`D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe.original`.
Code addresses below are raw file offsets; `.text` VA = raw + `0x400C00`.
The PE section table translates embedded pointers and literals independently.
The active zero-based IDA database labels this routine
`mid_helper_multicall_6E3670`, but its function ends at raw `0x6E3B12` and its
switch operand is an unadjusted VA. Consequently its decompiler omits most
cases and reports an invalid table. The original table is valid after PE
translation: VA `0xAE5254` maps to raw `0x6E4654`.

The optional-data path of `0x6E2590` supplies the record's `+0x12` byte to this
consumer. At `0x6E36F2` it zero-extends the byte, subtracts one, and rejects an
unsigned result greater than `18` (`0x6E3700/0x6E3703`). Thus selectors
`1..19` reach the table; `0` and `20..255` return false at `0x6E3AF2`.
Fourteen case bodies call distinct or shared payload readers. The other
bodies use already-supplied identity/text/category or owner state; a missing
reader below does not mean the outer message has no payload.

| Selector | Case raw | Payload reader raw | Observed downstream behavior |
| --- | --- | --- | --- |
| 1 | `0x6E3710` | `0x6FDBB0` | Localizes literal `CHAT_DUEL_RESULT`, then calls shared chat display |
| 2 | `0x6E38AE` | `0x6FDC30` | Resolves a row through `0x321EA0`; if present, dispatches numeric `353` |
| 3 | `0x6E3998` | `0x6FDCC0` | Checks current identity, then conditionally calls `0x937A70` |
| 4 | `0x6E3A3C` | `0x6FDD50` | Two owner/identity gates, then dispatches numeric `266` |
| 5 | `0x6E3B12` | `0x6FDDC0` | Category `14` can dispatch `347`; categories `12/13` update matching state and dispatch `262`, conditionally also `224` |
| 6 | `0x6E3D58` | none observed | Categories `12/13` set owner DWORD `+0x1E0`; category `14` sets `+0x1E4` |
| 7 | `0x6E3DAB` | none observed | Category `14`: found entry can dispatch `347` before removal |
| 8 | `0x6E3DAB` | none observed | Same body as `7`, but skips that `347` dispatch before removal |
| 9 | `0x6E3E79` | `0x6FDE30` | Updates owner `+0x188` through `0x6F6230`, then dispatches `261` |
| 10 | `0x6E3EFD` | `0x6FDEA0` | Updates owner `+0x1A0` through `0x6F7C90`, then dispatches `263` |
| 11 | `0x6E3F6C` | `0x6FDF10` | Gated lookup/validation path can call `0x85B850`; failure paths log and return false |
| 12 | `0x6E40D0` | `0x6FDFA0` | Owner `+0x1FC` lookup/compare/copy; selected updates dispatch `268` |
| 13 | `0x6E44A2` | none observed | Gated owner `+0x170` lookup; missing entry dispatches `265` and is inserted |
| 14 | `0x6E4213` | `0x6FE070` | Identity check, decode and gated call `0x95BCD0` |
| 15 | `0x6E42B9` | `0x6FE120` | Identity check, decode and gated call `0x95BF50` |
| 16 | `0x6E4347` | `0x6FE120` | Identity check, decode and call `0x488830` |
| 17 | `0x6E43BB` | `0x6FE190` | Identity check, decode and call `0x96E2F0` |
| 18 | `0x6E4423` | none observed | Gated localization through `0x390B00`, then shared chat display |
| 19 | `0x6E458A` | `0x6FE240` | Decoded flag selects pending-state removal and numeric `345` or `346` |

These are conditional consumer effects, not event names or a guarantee of
dispatch. Case `13`, for example, returns true even when either initial gate
prevents the event. The caller `0x6E2590` does not use this routine's Boolean
return. No managed capture here executes the native callbacks.

Selected literal-backed outputs:

- Case `1` at `0x6E380F` supplies VA `0x1B6E374` (`CHAT_DUEL_RESULT`) to
  localization helper `0x390F20`. The decoded pair is added as two formatting
  arguments. It then uses helper `0x6E9E30` for the channel and calls
  `0x6E3460` at `0x6E3855` with empty author, zero author ID, and zero flags.
  The translated message wording and which participant won are not inferred.
- Case `2` builds `player_name` from the decoded first field, `item_type_id`
  from resolved row `+0x10`, and `quality` from row byte `+0x33`, then calls
  `uiManager+0xAC` with code `353` at `0x6E395C`.
- Case `4` builds `inviter` from the supplied author text and `player` from
  the decoded field, then calls the same slot with `266` at `0x6E3ABE`.
- Case `19` always builds `player_name` from the supplied text and
  `unique_name` through `0x45DD50`. A nonzero decoded byte adds
  `status_type = "PENDING_IN"`, selects owner `+0x34`, and dispatches `345`;
  zero selects owner `+0x28` and `346` without that status property.
  Both call `0x6F5BF0` before dispatch at `0x6E462B`.

`StructuredChatMessageNativeLayout.cs` preserves all 19 case offsets and
payload-reader targets. Startup checks verify the complete PE-translated
table, the 14 reader call sites, selected output calls, and original literals.
This does not reconcile selector `0x76`'s parser-side `slBlob` with the
receiver's record range or establish the semantics of the conflicting
Character RTTI name.

### Structured-chat simple payload field ledger

Same immutable sample: 40,197,848 bytes, SHA-256
`E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235`.
These layouts describe the byte stream passed to the consumer's payload
readers, not the outer Matrix message or the parser-side blob's meaning.
No packet capture or runtime invocation was used. Wire offsets are relative
to the start of that payload; native offsets are relative to the destination
temporary, not the `0x40`-byte chat record.

`L`, `A`, and `B` below count text bytes excluding their NUL terminators.
The shared-text helper `0x12EE010` copies raw bytes through the terminator,
with no length prefix and no character decoding established. Raw multi-byte
integers use x86 little-endian loads. No wire alignment bytes are introduced
by these readers.

| Selector / reader raw | Wire offset and width, in read order | Native destination offsets | Consumed bytes |
| --- | --- | --- | --- |
| 1 / `0x6FDBB0` | `0`: text A + NUL; `A+1`: text B + NUL | `+0`, `+4`: 32-bit shared-text pointers | `A+B+2` |
| 2 / `0x6FDC30` | `0`: text + NUL; `L+1`: byte; `L+2`: DWORD; `L+6`: DWORD | `+0`: pointer; `+4`: zero-extended DWORD; `+8/+0xC`: DWORDs | `L+10` |
| 3 / `0x6FDCC0` | `0`: DWORD; `4`: text + NUL; `L+5`: byte | `+0`: DWORD; `+4`: pointer; `+8`: zero-extended DWORD | `L+6` |
| 4 / `0x6FDD50` | `0`: text + NUL | `+0`: shared-text pointer | `L+1` |
| 11 / `0x6FDF10` | `0`: QWORD; `8`: text A + NUL; `A+9`: text B + NUL | `+0`: QWORD; `+8/+0xC`: shared-text pointers | `A+B+10` |
| 15, 16 / `0x6FE120` | `0`: DWORD | `+0`: DWORD | `4` |
| 19 / `0x6FE240` | `0`: byte | `+0`: byte | `1` |

Important width evidence: raw `0x3849C0` bounds/refills **one** byte at
`0x3849D1`, reads it at `0x3849EB`, decrements the stream's remaining count
by one at `0x384A14`, then zero-extends and stores a DWORD at `0x384A17`
and `0x384A1B`. Its native four-byte output is neither four wire bytes nor a
float. In contrast, `0x356CF0` stores a byte (`0x356D12..0x356D16`).
`0x3370A0` advances four bytes (`0x3370C5..0x3370C7`), and `0x356F80`
copies two DWORDs and advances eight (`0x356FA3..0x356FAD`).

For selector2, the first text becomes `player_name`; native `+8` is the row
lookup input, whose resolved row supplies `item_type_id` and `quality`.
The byte stored at native `+4` and DWORD at `+0xC` are decoded but their
meanings are not established by this consumer. For selector19, the reader
preserves the full byte; the later `cmp byte,0` selects the two branches
for every nonzero value, not just `1`. `CapturePendingNotification` saves
only that already-decoded decision and does not remove entries or emit
notifications. The return value of removal helper `0x6F5BF0` is ignored;
failure to find an entry does not add a dispatch gate.

`StructuredChatMessageNativeLayout.PayloadFields` records the seven unique
simple-reader layouts, primitive call sites, native widths, and wire widths.
Checks cover all 15 field calls, their destination setup, the primitive
width instructions, minimum sizes (`2/10/6/1/10/4/1`), and all 256 pending
flag values. Those minimums are consumption arithmetic, not evidence that
the native wrapper rejects shorter/trailing inputs gracefully. Remaining
readers include nested optional/collection structures and remain outside
this simple ledger.

### Structured-chat optional values (cases5 and10)

The original executable's case5 wrapper calls `0x709750` at raw `0x6FDDF6`.
That reader reaches `0x709650` at `0x7097BE`, passing its destination's
`+0x28` optional member. Case10 wrapper calls `0x703910` at `0x6FDED5`.
Both optional readers first consume one raw presence byte; zero skips the
value reads, while **any nonzero byte** reads a value. Both store the native
value beginning at optional `+8`, with a flag at optional `+0`.

| Reader / case | Payload-relative wire offsets when present | Temporary native offsets | Total consumed, including presence |
| --- | --- | --- | --- |
| `0x709650` / nested case5 optional | `0`: presence; `1`: QWORD; `9`: DWORD; `13/14`: bytes | `+0`: QWORD; `+8`: DWORD; `+0xC/+0xD`: bytes | zero: `1`; nonzero: `15` |
| `0x703910` / case10 | `0`: presence; `1`: DWORD; `5`: QWORD; `13/17/21/25`: four DWORDs | `+0`: DWORD; `+8`: QWORD; `+0x10/+0x14/+0x18/+0x1C`: DWORDs | zero: `1`; nonzero: `29` |

No four-byte alignment gap precedes the case10 wire QWORD: the gap at
temporary native `+4..+7` is a destination-layout feature. Helper
`0x6F7D20`, called at `0x7039CB`, assigns fields individually to optional
`+8/+0x10/+0x14/+0x18/+0x1C/+0x20/+0x24`; it does not copy the temporary's
padding. The final three DWORDs are read through `0x3568E0`; its observed
load/store and four-byte cursor advance establish their width but not their
meaning, so the ledger preserves raw words rather than naming coordinates.

For case5's optional value, the temporary holds 14 initialized/read bytes
but both assignment paths copy **16** bytes. Initialization at
`0x7096B0..0x7096C3` writes a QWORD, DWORD, and WORD; the following reads
replace those same 14 bytes. The two-byte tail is not written on the traced
local path. Instructions at `0x7096F1/0x7096F6` load/store a full 16 bytes,
as do `0x709709/0x70970E` for first assignment. This is a static
initialization/copy-extent observation: it does not establish a visible
runtime effect or imply two additional wire bytes.

On absent input, `0x7039D7..0x7039DC` and
`0x70971C..0x709721` clear an existing presence flag without clearing the
stored value bytes. Present paths normalize the native flag to `1`.
`OptionalValueFields` records the ten primitive read calls relative to each
temporary, and the two consumed-byte-count helpers describe only branch
arithmetic; they neither decode a packet nor synthesize absent values.
Checks cover every presence byte, all field calls/destinations, the wrapper
chain, flag writes, and copy extents against the immutable original sample.

### Structured-chat case5 full read order

Reader `0x709750` processes the following fields in order. Native offsets
are relative to its decoded destination; `ztext` means raw NUL-terminated
bytes. These are storage/width descriptions, not semantic field names.

| Native destination | Wire field | Primitive call raw |
| --- | --- | --- |
| `+0` (DWORD) | one byte, zero-extended | `0x70975F` |
| `+4` | DWORD | `0x70976A` |
| `+8`, `+9` | two bytes | `0x709774`, `0x70977E` |
| `+0xC` | DWORD | `0x709789` |
| `+0x10` | QWORD | `0x709794` |
| `+0x18` | DWORD | `0x70979F` |
| `+0x1C` | ztext | `0x7097A9` |
| `+0x20` | DWORD | `0x7097B4` |
| `+0x28` | presence byte and conditional14-byte value, as above | `0x7097BE` |
| `+0x40`, `+0x41` | two bytes | `0x7097C8`, `0x7097D2` |
| `+0x44` | DWORD | `0x7097DD` |
| `+0x48` | WORD | `0x7097E8` |

With text length `L` excluding NUL, consumption is `L+37` for an absent
optional value or `L+51` when present. In particular, native gaps at
`+0xA/+0xB` or `+0x24..+0x27` are not serialized padding.
`Case5FixedFields` preserves the 13 primitive calls separately from the
optional member and records the last WORD's two-byte wire width.

### Structured-chat count-byte collections (cases9,14,17)

The three readers below reset their destination collection before decoding.
Each group starts with an unsigned count byte and reads exactly that many
entries. Count `255` requires another count group; every smaller count
ends the collection. Zero therefore represents an empty collection, and a
final nonzero count below255 does **not** require a trailing zero. Full
255-entry final groups need an additional count (possibly zero).

| Case / wrapper | Prefix | Collection destination / reader raw | Entry wire layout | Suffix |
| --- | --- | --- | --- | --- |
| 9 / `0x709560` | six bytes, WORD, byte (`9` bytes) | native `+0xC` / `0x7093F0` | QWORD, `8` bytes | none observed |
| 14 / `0x6FE070` | QWORD, DWORD, three four-byte words (`24` bytes) | native `+0x18` / `0x5DE550` | DWORD, WORD, four-byte raw value (`10` bytes) | none observed |
| 17 / `0x6FE190` | DWORD, ztext, three DWORDs | native `+0x14` / `0x6002C0` | DWORD, `4` bytes | byte at native `+0x48` |

Case9's reader appends QWORD-valued linked nodes through `0x709FD0`.
Case14 builds a temporary with DWORD at native `+0`, WORD at `+4`, and
four-byte value at `+8`. Its last value uses `movss` at raw `0x5DE675`,
but its semantic role is unknown. The append helper `0x5DE4C0` advances
the native vector end by **12** (`0x5DE513` and `0x5DE53F`); only **10**
bytes came from wire. Native padding at entry `+6/+7` is not a wire field.
Case17's helper appends four-byte values; its trailing byte is read only
after the complete chunk sequence.

With `N` entries and `G` count groups, the case9 total is `9 + G + 8*N`,
case14 is `24 + G + 10*N`, and case17 is `L + 18 + G + 4*N`.
`G` is at least one, so empty-list minima are `10`, `25`, and `L+19`.
These formulas count transmitted entries, not necessarily unique entries.
They establish no meanings for the opaque words or byte fields.

`StructuredChatCollectionNativeLayout.cs` records the collection offsets,
entry wire/native value sizes, continuation comparison sites, and the 19
surrounding primitive reads. Binary checks cover those calls, the parent
collection calls, per-entry cursor changes, 12-byte native append stride,
and count255 comparisons at raw `0x70950A`, `0x5DE6BE`, and `0x60046B`.
No new wire parser is introduced.

### Structured-chat case12 nested map and duplicate rejection

The case12 wrapper at raw `0x6FDFA0` calls `0x705570` at `0x6FDFD5`.
That reader resets an outer DWORD-keyed map, then reads count-byte groups.
Each transmitted outer entry is:

1. A four-byte little-endian key, loaded at `0x70571B` and consumed by
   `0x705720..0x705745`.
2. A count-byte chunked DWORD vector through `0x7035C0`, called at
   `0x70574E`.
3. A count-byte chunked DWORD **key set** through `0x70A340`, called at
   `0x705758`. No value bytes follow individual keys in this inner set.

All three levels continue only after count255 (outer `0x7058F4`, vector
`0x70376B`, set `0x70A423`). Every shorter count terminates its level.
An empty outer map therefore consumes one byte. One outer entry with two
empty inner collections consumes seven bytes: outer count, DWORD key, zero
vector count, zero set count. In general consumption is the number of outer
count bytes plus the sum of each entry's `4 + vector-count-bytes +
4*vector-entry-count + set-count-bytes + 4*set-entry-count`.

The destination outer node's value begins at node `+0xC`
(`0x705830`). Its vector is assigned through `0x47A770`; the inner key set
at value `+0x20` is assigned through `0x706C20`. The reader selects or inserts
the outer node before these assignments. Do not apply the inner set's
duplicate rejection policy to outer keys: their lookup/assignment path is
different. The assignment-helper follow-up below establishes replacement on
their normal successful copy paths.

The decompiler's `JUMPOUT(0x70A4B1)` is explained by an exception path:

- The inner reader clears its destination once through `0x70A2E0` at
  `0x70A35D`, before the first count. A continued group jumps to
  `0x70A362`, so earlier keys remain in the set across groups.
- Each key is passed to lookup/insert helper `0x38EC20`. The comparison
  at `0x38EC59` checks the full DWORD key. Existing-key return
  `0x38EC92` stores insertion flag0; new-key return `0x38ED21` stores1.
- At `0x70A416`, a zero insertion flag branches to `0x70A43E`, while a new
  key proceeds to the next entry. This rejects repeated inner keys even
  when the repetition is in a later count group of the same inner set.
- The error path allocates a one-byte object, stores an object pointer in a
  stack local, and passes the address of that pointer plus ThrowInfo VA
  `0x1DB8108` to thunk raw `0x159E27A` at `0x70A4AC`. The thunk jumps
  through IAT VA `0x1AC4770`, which the original PE import table identifies
  as MSVCR120.dll `_CxxThrowException`; `0x70A4B1` is an `int3`, not an
  omitted normal decoding continuation.
- The original ThrowInfo's catchable array is VA `0x1DB80FC`, containing
  two types. Its first CatchableType VA `0x1DB80E0` refers to descriptor
  VA `0x1E125C4`, whose literal name is
  `.PAVunpack_exception@serialize@sl@@`. This names a pointer to
  `sl::serialize::unpack_exception`, not a by-value exception object.
  The second descriptor is `.PAX` (`void*`).

This is a statically established error route, not proof of an uncaught
exception, a crash, or a particular caller-visible failure result.
`StructuredChatCollectionNativeLayout` preserves the reader/assignment
layout and exception metadata. Checks verify wire order, all three
continuation branches, insert-result polarity, imported throw target, and
the PE-translated ThrowInfo/type-descriptor chain. No error is replaced by
silent duplicate collapse in the saved model.

The 19 case targets and all payload shapes now have direct traces, including
these nested structures. Open work is semantic: other cases' owner
update/compare helpers, callback names for numeric event codes, localized text inputs,
and the still-unresolved Character selector0x76 parser/receiver mismatch.

### Case12 stored-map equality and notification gate

The original instructions at raw `0x6E40D0..0x6E4213` establish case12's
owner update route. A failed `0x48C2B0` prerequisite or failed payload reader
returns false. After those gates, the owner collection is at `+0x1FC`
(`0x6E410B`); it is an ordered tree keyed by an unsigned QWORD. Lookup/create
helper `0x6F6420` compares the key at node `+0x10`, then returns its map at
node `+0x18` (`0x6F6475`, `0x6F64A6`).

| Existing owner key | Decoded map condition | Assignment | Event268 |
|---|---|---|---|
| Present | Equal to stored map | No | No |
| Present | Different, including replacement with an empty map | Yes | Yes |
| Absent | Nonempty | Create owner entry and assign | Yes |
| Absent | Empty | Create owner entry and assign empty map | No |

`0x6E4131` calls outer equality helper `0x6FE010`; a true result branches
directly to cleanup at `0x6E41B8`. A changed map goes through `0x6F9840` at
`0x6E414F` before notification. The absent-key branch tests decoded outer
count at `0x6E41CA`; zero takes the separate assignment at `0x6E41EC`, then
returns true without passing the notification code. Thus a successful case
return is not evidence that an event was emitted.

The equality chain is structural, not a bytewise comparison of native
allocations or hash bucket order:

- `0x6FE010` compares outer entry counts, looks up each DWORD key through
  `0x7001E0`, and compares the paired values through `0x6E2060`.
  Outer entry iteration order does not determine equality.
- `0x6E2060` compares vector element counts and then each DWORD in order
  (`0x6E2090..0x6E209C`), so reordered vector elements can change the map.
  Allocator/capacity state and native padding are not compared. The native
  length check masks off the low two bits of byte-length differences;
  element-count equality here assumes the valid DWORD ranges established
  by decoding, not arbitrary malformed native pointers.
- For the key set at value `+0x20`, `0x6FD3E0` compares counts and looks up
  each DWORD key through `0x6F6B00`. Set insertion order is irrelevant;
  membership is what is checked. These are valid decoded unique-key sets,
  given the duplicate rejection established above.

The event stores the same caller-provided QWORD under literal keys
`chatId` (VA `0x1B6E454`) and `entityId` (VA `0x1B6E448`), then dispatches
numeric268 through the application manager's slot `+0xAC`
(`0x6E419C..0x6E41A3`). No named event/subscriber or gameplay meaning is
inferred from those two field names.

Replacement helpers also explain repeated outer payload keys:
`0x7057C3..0x7057DA` selects an existing keyed node when found, then both
new and existing nodes reach the same value assignment. Vector assignment
`0x47A770` copies source elements and sets the destination end; an empty
source resets that end to begin at `0x47A7B0..0x47A7B2`. Set assignment
`0x706C20` clears destination nodes/count at `0x706C65..0x706C8B` before
copying source keys through `0x707540`. Thus, on successful normal copies,
later entries for the same outer key replace both nested collections,
rather than merging them. The owner-map assignment `0x6F9840` similarly
clears the old map via `0x6FB070`, then copies the new entries through
`0x6FF6A0`. Allocation/error behavior is not replaced with managed fallback
values in the saved capture.

One important decompiler limitation: `0x6F6600` initially appears to return
constant0. Its instructions instead initialize an output local at
`0x6F6612`, pass its address in ECX to range-count helper `0x701630`, and
reload it at `0x6F662A`. The helper writes the count through ECX at
`0x701691`. Treating the decompiler's constant as authoritative would
incorrectly eliminate the entire existing-key equality branch.

`StructuredChatMessageNativeLayout.CaptureCase12MapUpdate` captures the
decision only after the owner prerequisite and decode succeed. It does not
parse bytes, perform native lookups/copies, or dispatch an event. Binary
checks anchor the nested equality, output-local count, assignment paths,
two literal field names, and notification suppression branches.

### Shared structured-chat multi-entry prerequisite

The case12 prerequisite `0x48C2B0` is also called by cases9,10,11,13,18.
Original direct-call sites are respectively `0x6E40D2`, `0x6E3E7B`,
`0x6E3EFF`, `0x6E3F6E`, `0x6E44A4`, `0x6E4425`. Every call is followed
by an AL test before further handling. Cases9,10,11,12,18 return false
through `0x6E3AF2` on failure. **Case13 differs:** it branches to
`0x6E4583`, sets AL1, and returns true without processing its case body.
This is another reason not to equate a true dispatcher return with event
delivery. IDA's current xref list omits these unassigned switch bodies;
the original instructions, rather than that incomplete list, establish
the six calls.

The helper first requires the DWORD at owner `+0x118` to be nonzero.
It then subtracts the DWORD range begin at `+0x10C` from range end at
`+0x110`, interprets the wrapped32-bit difference as signed, divides by104
with signed truncation, and accepts only when the quotient cast to unsigned
is greater than1. Original `0x48C2BF..0x48C2D4` implements signed division
with multiplier `0x4EC4EC4F`, high-half shift5 and sign correction; the
`jbe` at `0x48C2D9` is unsigned.

For well-formed element ranges, this means nonzero owner word and at least
two104-byte elements. It does **not** validate range order or alignment.
A reversed range of exactly104 bytes yields quotient-1, which passes the
unsigned comparison when the owner word is nonzero. A reversed difference
of one byte truncates to0 and fails. These arithmetic facts are preserved
by `PassesMultiEntryPrerequisite`; they are not claims of valid native
objects or observable runtime consequences. The `+0x118` word's meaning
and the104-byte element type remain unnamed pending writer/consumer
evidence.

### Cases13/18 paired-word gates and text-cache side effects

The second predicate at raw `0x6E9F40` compares owner DWORDs
`+0x120/+0x124` with `+0x08/+0x0C`. Both must match; there is no nonzero
test. Treat this as QWORD equality, without inferring the semantic identity
of either field. All-zero pairs match too.

Case13 calls the predicate at `0x6E44B3` and skips its body when false.
It returns true on either prerequisite failure, on a cache hit, or after
the normal miss path. Once both prerequisites pass:

1. The caller text is materialized by `0x45DD50` at `0x6E44C8`. This helper
   uses `strlen` and copies the resulting byte range; it is not an
   additional structured wire reader.
2. `0x6FD280` looks up the text in owner cache `+0x170` at `0x6E44DE`.
   The lookup uses lexicographic NUL-terminated byte comparison through
   `0x700350/0x6FFFC0`, not pointer identity or a demonstrated
   case-insensitive normalization.
3. On a hit, `0x6FD280` calls `0x700680` before returning node value
   `+0x18`. The latter unlinks the associated key-list node
   (`0x7006C1..0x7006D0`), decrements the list count, releases that node,
   allocates a replacement through `0x7048C0`, and links it at the
   sentinel's previous end (`0x700712..0x700721`). The tree node's
   `+0x28` list pointer is updated. This is a key-position refresh, not a
   read-only membership test. The eviction/capacity follow-up below
   establishes the miss-path trimming policy.
4. On a miss, numeric event265 is dispatched first
   (`0x6E451A..0x6E4521`) with literal `player` (VA `0x1B6E534`) and the
   caller text. Only after dispatch returns normally does the code build
   a QWORD-plus-shared-text value from the caller QWORD and text
   (`0x6E4535..0x6E4541`) and call `0x6FD0E0` at `0x6E4554` to store it
   in the same cache.

The event-before-store ordering is directly observed. Do not model the
store as already completed when a subscriber runs, or collapse hit
behavior into a side-effect-free lookup. Reentrant callback behavior and
allocation failures are not simulated by the saved decision capture.

Case18 calls the same predicate at `0x6E4434` but rejects when it is
**true**. It then compares owner `+0x120/+0x124` with the caller QWORD
through `0x37C5F0` at `0x6E4448`, rejecting when unequal. This latter
helper is just two DWORD comparisons, not collection membership. Thus the
display path requires all three conditions:

- The shared `0x48C2B0` multi-entry prerequisite passes.
- Owner pair `+0x120` differs from owner pair `+0x08`.
- Caller QWORD equals owner pair `+0x120`.

The path localizes `TIP_JOIN_ANOTHER_INSTANCE` (VA `0x1B6E53C`) through
`0x390B00`, then calls shared display `0x6E3460` at `0x6E448E` with
literal category `notification` (VA `0x1B6E558`). Cleanup jumps to
`0x6E3AED`, whose destructor call falls through to `xor al,al` at
`0x6E3AF2`. **Case18 returns false even when it displayed the tip.**
Combined with case13's true-without-notification paths, this rules out
interpreting the dispatcher's Boolean as an event-delivery indicator.
Its enclosing caller already ignores that Boolean.

The two pure C# captures record only these post-input decisions and
ordering, not string encoding, cache ownership, actual dispatch, or
native allocation behavior. Exhaustive Boolean combinations and
original-binary checks cover the opposing predicates, hit refresh, miss
notification-before-store, exact literals, and displayed-but-false return.

### Case13 text-cache capacity and oldest-key eviction

The insertion helper `0x6FD0E0` rechecks the text key. If already present,
it refreshes the list position through `0x700680` at `0x6FD124`, then
updates the value QWORD and shared text at node `+0x18/+0x20`. A miss
reaches `0x6FD172`, where cache byte `+0x14` controls trimming. Relative
to the cache base (owner `+0x170`), the observed members are:

| Offset | Observed use |
|---|---|
| `+0x00` | Text-keyed tree sentinel |
| `+0x04` | Tree entry count |
| `+0x08` | Separate key-list sentinel |
| `+0x0C` | Key-list count |
| `+0x10` | Configured capacity DWORD |
| `+0x14` | Trimming-enabled byte, any nonzero value enables |

With trimming enabled, `0x6FD178..0x6FD17F` subtracts1 from capacity and
calls `0x700570` **before** appending the new key. The trim helper uses
an unsigned count comparison at `0x700584..0x700586`, then removes exactly
`current-list-count - target` entries when the current count exceeds the
target. It selects the list head's text at `0x700593..0x70059A`, finds that
key's tree range through `0x702F50`, and erases the range through
`0x6F9310`. It then unlinks/releases that list head and decrements the
list count. After the loop, assertions at `0x70063D` and `0x700659`
check both tree and list counts against the remaining count.

Insertion appends the new key at the opposite (previous/tail) end at
`0x6FD1B4..0x6FD1BA`. Hits also move their keys to that tail through the
unlink/reappend sequence described above. This establishes least-recently
accessed-key eviction on normal enabled-trim paths, rather than arbitrary
tree-order eviction. A hit does not add another cache entry or invoke
capacity trimming. The policy does not depend on elapsed time in these
helpers; no expiration timer is inferred.

For a positive configured capacity, a miss trims to at most
`capacity - 1`, then appends one key. Capacity1 evicts all existing keys
on a miss; capacity3 with three entries evicts one. With trimming disabled,
no such capacity reduction occurs. Capacity0 is not rejected here:
the decrement wraps to `0xFFFFFFFF`, so the unsigned trim comparison
requests no eviction. This is native arithmetic, not a recommendation to
configure a zero-sized cache. Default capacity, default enablement, and
other writers of these fields remain unestablished.

The saved `CaptureCase13CacheTrim` records only the trim target and
number of oldest keys requested for removal; it does not allocate,
append, mutate a collection, or assume successful completion at native
allocation limits. In particular, the list append has its own length-error
route and is not modeled as an unconditional successful increment for
arbitrary DWORD counts. The boundary checks include disabled trimming,
below/at/above capacity, capacity1, and unsigned zero-capacity wrap.

## Post-notification chat file log and folded hash

`0x6F46F0` is a **chat file-log writer**, gated by the current object's
FILE pointer+18. PE imports prove `_time64`, `_localtime64`, `strftime`,
`strncpy`, `sprintf` and `fputs`. It formats current local time with `%X`
for its false detail flag (the runtime335 caller supplies0), otherwise
`%c %Z`. The three prefix templates, including final space, are:
`"%s %s%s"` for no channel/author pointers, `"%s %s: %s%s"` for one,
`"%s %s %s: %s%s"` for both. Pointer presence matters even for an empty
CString. `MatrixChatLogNativeLayout.cs` takes already-formatted timestamp
bytes rather than guessing native locale/time-zone behavior.

The writer hashes a temporary binary suffix **not emitted as log text**:

| Region | Exact bytes |
|---|---|
| Prefix | Formatted timestamp/labels/text, including trailing space |
| Separator | One NUL |
| Author ID | Raw LE64 with low8bits cleared |
| Static footer | 25 bytes from VA`0x1C67C34`: `73616C74696E652C637261636B657273000000000C2AFFFF00` |
| Mode text+10 | First min(native stored byte length,32) bytes via `strncpy`, zero-padded after an embedded NUL |

The static footer begins `"saltine,crackers"` but its full25-byte value
includes zeros/other values; its purpose beyond this hash input is not
established. The mode+10 text is retained positionally, not claimed as an
account/character name. The byte limit may split a multibyte character.

Raw-span hash `0x12F1780` is FNV-1a32 (offset2166136261, prime16777619)
followed by wrapping final mixing: multiply8193, XOR unsigned shift7,
multiply9, XOR unsigned shift17, multiply33. This is **not plain FNV**
or CRC. It processes embedded NULs because an explicit length is supplied.
The same per-byte/final-mix implementation was already saved for SDB name
seeds; `src\Shared\EngineByteHash32.cs` now shares it between Startup and
Serialization without changing SDB string encoding or cipher behavior.

The writer XORs the hash's low/high16bits and overwrites the original
prefix NUL with `"%04X\n"` before `fputs`. The hidden ID/footer/name bytes
are not written. The C# model builds the pre-CRT line bytes and checksum;
it does not open files, emulate text-mode newline translation, format native
time, or handle file lifetime/write errors. The prefix
`"12:34 zone Alice: Hi! "` with author`0x0102030405060708` and mode text
`"Bob"` yields raw hash`0xC62BB938`, suffix`7F13`, and a single LF passed
to the CRT. Prefixes passed directly to the checksum API must be logical
CString bytes without their terminator; invalid embedded NUL is explicit.

## tfChat identity and file lifecycle

RTTI at type descriptor VA`0x1E12EF8` identifies the class with vtable
VA`0x1C67CDC` as **tfChat**. Constructor`0x6E26D0` and destructor`0x6E2C80`
install that table. The prior "current mode-specific object" returned by
`0x6F4540` is this chat component, not the entire app-mode allocation:
Login object+14, World+28E8, LookingGlass+68. It is a getter, not a transition.
Existing positional `Mode*` names are retained for compatibility; chat text+10
still has no proven original field name; its World source is recovered below.
Outgoing command echoes use it as their own author label. LFG fields+48/+50 likewise belong
to tfChat. The full class behavior is not reconstructed.

`MatrixChatFileNativeLayout.cs` saves file-operation **selections without IO**:

| Native path | Proven behavior |
|---|---|
| `0x6F4610` | Compose `"chat.log"` onto the existing cached VIBE base path; base identity/resolution remains outside this model |
| Constructor | Global byte VA`0x1EDB0A4` zero selects `"wt"`; nonzero selects `"at"` |
| Open wrapper`0x12BA030` | Convert UTF8 path/mode to UTF16 with `MultiByteToWideChar(65001,0,...,-1,...)`; `_wfopen`; store returned FILE at+18 |
| Open failure | Log exact `"Failed to open chat log file for writing"`; writer's null-FILE gate prevents a start line |
| Start marker | `"Chat session started"`, detailed timestamp flag1, no author/channel labels; author-ID pointer is global pair VA`0x1E68530`, not an invented zero |
| Attempt flag | Set global byte1 **even after open failure**; it denotes a prior attempt, not success |
| Destructor | Try shutdown line `"Chat session cleanly shut down"`, detail flag0; if FILE exists call `fclose`, then clear+18 |
| Flush helper`0x6F46E0` | `fflush` only for a nonnull FILE; no static callers found in the current IDB |
| OpenChatLog`0x6ACA60` | Absent chat logs `"OpenChatLog: Chat not initialized"`; otherwise flush nonnull FILE, compose path and `ShellExecuteA(0,"open",path,0,0,1)` even if FILE is null |

The modes explicitly select **CRT text mode**: on Windows, the log line's LF
is translated to CRLF during writing. The checksum remains over the pre-CRT
prefix/suffix input. UTF8 conversion above applies to the filename/mode, not
to the emitted `fputs` byte string. The same open wrapper already appears in
the saved Lua bytecode-cache code with `"wb"`; it is not chat-specific.
Native conversion/CRT/shell return handling, filesystem lifetime and actual
viewer launching are not simulated. Shutdown selection covers the FILE
prefix only, not the many other tfChat containers freed by its destructor.

### World setup supplies tfChat's local identity/text

The successful tail of World initializer`0x64B460`, raw`0x64B59C..0x64B5F4`,
loads the owner/context pointer from World+10 and tfChat pointer from
World+28E8. It copies owner DWORDs+848/+84C into chat+8/+C, forcing the
resulting 64-bit value's low byte to **1**. Upper56bits are unchanged;
there is no added zero/invalid-ID filter. It copies the CString at
owner+850, or an empty CString for a null pointer, into chat shared text+10
through `0x2FCC90`. This proves the source of the checksum's positional
text during World setup, rather than inferring it from the hash routine.

`MatrixChatIdentityNativeLayout.cs` projects this successful copy using
raw bytes and an opaque owner pair. The owner+848 pair also appears in the
previously reconstructed authentication request; its formal identifier type
and the original name of owner+850 are not established here. Do not label
these as account GUID/name or character GUID/name on that evidence alone.
Outgoing chat command echoes use chat+10 as their own author label and
chat+8/+C is compared with other identity pairs, so **local chat identity**
is a bounded functional name. Native shared-text allocation/refcounting
remain outside this projection.

Login has a **different source**, not a copy of World's owner fields.
`0x63B250` samples byte+34 of the object at application+A28 while holding
its native lock. Zero returns false before creating chat; nonzero constructs
tfChat at Login+14, calls`0xBC9A20`, forces returned pair's low byte1 and
copies the routine's explicit CString argument into chat+10. No static
callers were found, so the original meaning of that supplied text is not
established. `ProjectSuccessfulLoginInitialization` models only this
successful copy, not locks, allocation failures or callback registration.

Original instructions prove that`0xBC9A20` returns a full **EDX:EAX pair**;
IDA's DWORD-only inferred prototype hides the high return register.
`lock xadd` increments global DWORD VA`0x1DFC588`, using its old unsigned
value. It ORs that value shifted8 into the base pair at VA`0x1E487F8`,
then clears low8bits. The next counter wraps at32bits.
`ProjectGeneratedId` accepts sampled old counter/base, returning projected
pair/next counter without changing shared state or promising uniqueness.
The native base-bit-containment assertion is satisfied by the observed OR
for stable sampled values. This is **not** evidence that Login's ID is a
server-supplied account or character GUID.

LookingGlass initializer`0x65A880` constructs fresh tfChat at+68.
The constructor clears chat+8/+C/+10, so initial identity is zero/empty;
this does not prove those fields remain unchanged by replay callbacks.
No downstream LookingGlass identity producer was followed in this bounded
initialization trace.
