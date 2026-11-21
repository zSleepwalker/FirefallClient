# Matrix profiler and server routes

### Profiler frame61 missing-name collection and refresh gates

`MatrixServerProfilerFrame61NativeLayout.cs` saves the bounded portion of
consumer`0xE65290`. Root collector`0xE669F0` looks up each root WORD in the
map at owner+190; recursive node collector`0xE66B90` looks up each node
WORD in owner+1B0. This establishes the maps' **root-name / node-name
roles**, not formal thread/function categories. State exposes `RootNames`
and `NodeNames` aliases. Missing entries append IDs to two WORD vectors.
Root order and node **depth-first preorder** are retained; repeated missing
IDs are **not deduplicated**, and a present key with empty text is known.
The C# collector walks iteratively without imposing a recursion limit.
Native temporary subtree copies/refcounts are not reproduced.

If either missing-ID vector is nonempty, native samples its DWORD clock.
Only unsigned wrapping `now-owner[18C] > 5000` enters the request boundary;
equality does **not**. It stores `now` at18C **before** resolving an
endpoint through global VA`0x1DFC82C` object's virtual60. A nonnull endpoint
receives virtual8 with opaque owner DWORD1D0 and both missing-ID vectors.
An absent endpoint therefore still advances the throttle marker. Actual
request encoding/dispatch is unresolved; C# reports selection, not success.
Message60's `tick-5000` store means same-tick elapsed5000 remains throttled,
and the next tick can select this boundary.

Separately, byte VA`0x1E49A5A` equal0 selects opaque refresh calls
`0xE8D440` and`0xE8DB00` on owner+4. A nonzero byte skips those calls,
**not** missing-name traversal or request throttling. Later owner+0C
null-handle construction and frame-data/rendering semantics remain outside
the saved projection. The eight WORD metrics per node remain unnamed.

IDA initially misclassified raw`0xE65300..0xE653DE` as switch-table data.
The original entry/ret4/padding and all instructions were verified;
only that222-byte analysis region was redefined as code inside function
`0xE65290..0xE653DE`. Every executable byte was preserved.

### Profiler61 metric expansion, numeric aggregation and80-frame history

`NativeAlgorithms\NativeProfilerSamples.cs` now reconstructs the numeric
stage, without native tree widgets or rendering. Decoder`0x1303020`
uses the static65536-entry float table at VA`0x1C165B8`; **every finite
entry** was checked against IEEE binary16-to-float32 conversion. The
nonfinite entries classify as NaN/infinity. Selected values are multiplied
by float32 **65536** (bits`47800000`, VA`0x1C821D4`), passed to the shared
signed64 conversion entry`0x159DE70`, and only low32 retained.
This establishes scaled numeric representation, **not metric units**.

| Source node offset | Saved sample offset | Conversion |
|---|---|---|
| 10 | WORD0 | Raw WORD, later used as exclusive divisor |
| 12 | DWORD4 | binary16 → float32 ×65536 → truncated signed64 low32 |
| 14 | DWORD8 | Same scaled conversion |
| 16 | DWORD0C | Same scaled conversion |
| 18 | WORD10 | Raw WORD |
| 1A | WORD12 | Raw WORD |
| 1C | DWORD14 | Same scaled conversion |
| 1E | DWORD18 | Same scaled conversion |

Native sample stride is28; padding at2 is not modeled or serialized.
Finite negatives are not sanitized: encoded BC00(-1) becomes FFFF0000,
7BFF becomes FFE00000, and FBFF becomes00200000 after low32 wrapping.
Tiny finite values truncate toward0. The C# decoder explicitly declines
NaN/infinity instead of claiming to emulate native CRT exceptional-FP
handling; it reports the failing source WORD index and no partial sample.

Numeric updater`0xE8D760` uses a dedicated kind3 child with the same ID for
an **exclusive** sample. Starting from decoded DWORD4, it subtracts each
processed child DWORD4, saturating at0 **per child**. Exclusive DWORD8/0C
both become that remainder divided by WORD0, or the undivided remainder
when WORD0 is0. Exclusive late fields keep the decoded values. The regular
inclusive sample retains its decoded first fields while adding children's
WORD10/12 and DWORD14/18, with16/32-bit wrapping respectively.

Root updater`0xE8DB00` sums direct child DWORD4, sets all three DWORD4/8/0C
to that total, sets WORD0=0, and sums the four late fields with native
wrapping. The pure projections require already processed/matched child
samples: UI node construction, missing-node assertions, selection recovery
and sibling matching remain outside the API.

History initializer`0xE8E090` fills **all80 slots** with a supplied28-byte
template and sets next-slot0. Samples start at object+14, index at+8D4.
Writes replace the current slot then advance modulo80; latest is
`(next+79)%80`, including the template before the first append.
`NativeProfilerHistory80` preserves this filled-history behavior, rather
than treating unused slots as absent. Native object kinds, keys and
shared-text pointers around the history are not recreated.

### Profiler persistent sibling-tree merge

`NativeAlgorithms\NativeProfilerTree.cs` saves the structural stage of
root merger`0xE8D440` and child merger`0xE8D010`.
Native kind1 is a root, kind2 a regular node, and kind3 its dedicated
exclusive child. Kind is DWORD+8D8 and identity a full WORD+8DC.
Parent/next-sibling/first-child links are+4/+8/+0C. The managed projection
uses ordered lists and stable object references; its top-level parent is
null instead of reproducing the hidden native root-holder object.

Each incoming level searches **only forward** from the current sibling
cursor for a matching ID. A match retains that object; a miss inserts a
new object immediately after the previous matched/inserted sibling (or at
the head), **before** any remaining stale siblings. It never globally
indexes by ID, moves matched objects, or deletes absent siblings.
For example, existing roots `[1old,2]` followed by frame `[2,1]` become
`[1old,2,1new]`, not a reordered or deduplicated map. Duplicate source IDs
likewise remain separate objects. Empty frames/child lists preserve stale
structure.

A kind2 node ensures its first child is kind3 with the **same ID**, even
when the incoming child list is empty. Its real incoming children are
matched after that special child; roots do not get an exclusive child.
The native regular-child level first ensures all siblings, then descends
in source order. C# uses an explicit task stack and preserves these final
structural effects without a recursion-depth cap.

`MergeStructure` deliberately updates **structure only**. It does not
append numeric histories, resolve names, emulate allocation order/failures,
change expansion flags, or recover selection. The optional
`MergeWithSelection` stage below adds only the proven selection/expansion
rules, still without executing the UI.

### Profiler selection and visible-row traversal

Identity search`0xE8D3F0` compares DWORD kind and full WORD ID, then searches
children recursively in sibling-order preorder. It ignores expansion
flags and returns the **first** match, including stale objects and kind3
exclusive children. Native `0xE72480` is instead a generic tree
**visible-descendant index lookup**, not a constructor/handle allocator.
It only descends if parent byte+10 is nonzero, counts each child before
recursing, and excludes the starting container from row numbering.
`FindVisibleRow` saves the zero-initialized cursor projection; the hidden
container's expansion state is an explicit input. New profiler nodes
start collapsed (`0xE8E090` writes WORD+10=`0x0100`).

Root merger`0xE8D440` remembers the previous selection's kind/ID. An exact
object visited while merging retains selection without changing expansion
flags. Visit tracking includes the exclusive first child of every matched
regular node, even with an empty incoming child list. Otherwise it searches
the persistent tree by remembered identity. A successful recovery expands
**ancestors only**, including the hidden container, and need not choose a
currently reported node: the first stale duplicate can win. If no prior
identity matches (or there was no selection), it selects visible row0;
that can be null for a collapsed/empty container.

`MergeWithSelection` saves these outcomes with stable object references.
`IsExpanded` is offline metadata, not a GUI operation. The native hidden
container and arbitrary non-profiler widget nodes are not instantiated by
this model; search/recovery covers kinds1/2/3 in the saved profiler tree.
After the optional refresh, frame consumer`0xE65290` independently fills a
null selected-node pointer at owner+0C via the same row0 lookup
(`0xE65392`). This fallback still runs when refresh is suppressed. Owner+8
is the hidden container pointer; the former tentative "lazy handle
creation" interpretation is superseded. Callers modeling suppressed
refresh can use `FindVisibleRow` without performing a structural merge.

### Profiler graph sample refresh and positional root-child mismatch

`NativeAlgorithms\NativeProfilerFrameRefresh.cs` couples persistent tree
objects to the saved numeric projections and80-slot histories. Normally
call `MergeStructure` (or `MergeWithSelection`) first, then `Apply`.
Structural merges alone leave histories unchanged; new nodes'80 slots
contain zero templates. The sample stage updates only currently traversed
nodes; untouched stale nodes retain their previous samples and next-slot.

Root updater`0xE8DB00` finds each root **forward by WORD ID**, but pairs its
incoming direct children with stored direct siblings **by position, with
no ID check/search**. This differs from structural matching and must not
be silently normalized. Existing children `[20old,40]`, incoming `[40]`
leave that structure intact; the numeric updater then writes40's sample
into20old's history, leaves40's history unchanged, and aggregates the
updated20old sample for the root. This observed mismatch is preserved and
counted in `PositionalIdentityMismatches`; it is not a claim about intended
behavior.

Regular-node updater`0xE8D760` validates its same-ID exclusive first child,
decodes its own metric words, then searches real children **forward by
WORD ID**. A missing child is skipped without recursion, metric decoding,
subtraction, or late-field aggregation; after exhausting that cursor,
remaining source children are also skipped. Source node ID is not checked
against the receiving regular node's ID. Only successfully processed
children contribute to the existing inclusive/exclusive formulas.
Exclusive history is appended before regular history; parents consume
the child's newly appended inclusive sample. An explicit work stack
preserves this bottom-up order.

Root totals include only direct children processed for this frame, not
all stored siblings or every descendant. Empty source children append a
zero root sample; absent source roots do not advance. If the stored root
has no children, native code takes its zero-root branch without inspecting
any incoming children (saved as `SkippedRootChildren`).

The result reports updated roots/regular nodes, skipped child occurrences,
and positional identity mismatches. Nonfinite half-floats remain an
**unsupported native CRT boundary**, not zero/success: the exact rejected
source and metric-word index are returned, with earlier completed history
writes retained. Invalid metric counts are explicit too. Missing roots,
exhausted positional sibling lists, and invalid exclusive-child structure
raise managed invariant failures rather than emulating native assertions
or invalid dereferences. This is offline sample state, not rendering,
name resolution, native memory ownership, or exceptional FP emulation.

### Profiler display-name selection and snapshot override

Profiler vtable VA`0x1C78680` slot+10 targets`0xE8CD70`, a previously
undefined144-byte routine now defined from verified original instructions.
No executable bytes changed. `NativeProfilerDisplayNames.Resolve`
saves its raw logical text selection; it does not decode character
encoding, render strings, or manage native shared-text lifetimes.

A nonempty cached shared text at node+8E4 overrides **every** kind/table
branch. Empty or absent cached text falls through. Kind1 looks up the
full WORD ID in the root-name table via`0xE648E0`; kind2 uses the separate
node-name table via`0xE64920`. A missing key returns the exact literal
`<Waiting For Server>`, but a present empty name returns empty text, not
that placeholder. Kind3 returns `[Exclusive]`. Other kinds return
`<Error>` when there is no nonempty override.

The getter does **not** populate the cache or clear it on name-table
updates. Snapshot writer`0xE8CE00` first resolves the getter, copies its
C-string into+8E4 with`0x2FCC90` (`strlen`, byte copy and terminal zero),
then writes base state and cached text. Reader`0xE8CE70` reads base state
and null-terminated shared text into+8E4. Thus snapshots can leave a
nonempty override that takes precedence over later server name updates.
The saved getter API accepts an explicit sampled cache; it does not
invent invalidation or implement the whole snapshot codec. Logical byte
selection remains separate from C-string prefix/termination rules.

### Profiler derived-node subtree snapshots

`NativeProfilerNodeSnapshotCodec` saves the bounded derived-node format
from writers`0xE8CE00`/`0xE8DD00` and readers`0xE8CE70`/`0xE8DDA0`.
This is **not** ServerProfiler_SendFrame61's network body, nor a claim
about whole-file framing or the hidden container's dynamic type.

One derived node writes, in order: LE signed32 child count; every child
body in sibling order; raw expansion byte+10;80 physical history samples;
raw LE DWORD next-slot+8D4; cached name bytes plus terminal NUL.
Each sample writes WORD0, DWORD4/8/0C, WORD10/12, DWORD14/18:
**26 bytes**, omitting the native28-byte structure's padding at+2.
History is physical slot order, not chronological order. A nameless leaf
is2090 bytes; no sample count or half-float conversion is present.

The generic tree base writer`0xE726E0` counts siblings and invokes each
child's virtual writer before expansion; reader`0xE72740` recursively
creates children using virtual+0C and reads the byte after them.
Its count test is signed: negative counts mean no children. Decode keeps
that raw count for inspection; encode re-counts actual children as native
writer does. The next-slot DWORD and nonzero expansion byte are copied
without normalization/validation by native code and remain raw in the DTO.

Kind/ID, parent/sibling pointers, name-map bindings, and cached text's
allocation metadata are **not** in this body. The derived empty-peer
factory`0xE8CEA0` initializes kind0/IDFFFF before reading; the traced body
does not restore those live identities. Snapshot DTOs therefore remain
separate from the live kind1/2/3 tree and are not silently activated.

Encoder input represents **already captured** content: it does not invoke
the live getter or alter a tree's name cache. It writes the first C-string
prefix plus NUL, whereas decode owns name bytes up to the terminator.
The iterative codec leaves trailing framing bytes unconsumed, reports
truncation explicitly, rejects invalid80-slot/cyclic managed input, and
does not execute native refill hooks, constructors, destructors or
partial object mutations. Full file/container/type-dispatch framing is
still unresolved.

### Outgoing profiler-name requests: concrete dual-family endpoint

RTTI proves the concrete endpoint is `tfServerProfiler`, vtable
VA`0x1C61B50`. Slot+8 targets`0x6AAB20`, which builds either
`matrix_fury::ServerProfiler_RequestNames` or
`firefall::ServerProfiler_RequestNames`. `NativeProfilerNameRequests`
saves this method's sampled selection rules; `NativeProfilerNameRequestWire`
saves its normal application buffers. It performs no logging, timestamp
writes, socket submission, or other native execution.

The concrete method first requires manager+0xB80==2 (the verified World
mode). Other modes ignore even an invalid selector. In mode2, selector1
constructs matrix message30; selector0 constructs firefall message18;
other values take the native assertion path. Both first get a selected
observer and require its DWORD+820 to be nonzero. Matrix gate`0x6AADC0`
requires signed DWORD+0C **>3** and optional log mask`0x01`. Firefall
gate`0x6AAD20` instead requires **>4**, uses mask`0x02`, stores
`1000 * uint32(tick)` at observer+0xB0, and copies observer+878/+87C
to descriptor+18/+1C. These copied words are one raw64-bit header prefix;
no type-byte forcing or established character-ID interpretation is added.
The matrix rejection's EAX is not defined; modeled outcomes do not claim
a native Boolean return or delivered request.

Both descriptors copy first/root and second/node WORD vectors into+28
and+34, preserving order and duplicates. Writer`0x349030` emits
chunked-u8 counts and LE WORDs, with a terminal zero for exact multiples
of255. Whole normal buffers are:

- Matrix30 / channel1: compact code30, root-ID list, node-ID list.
- Firefall18 / channel2: raw LE64 routing pair, compact code18, root-ID
  list, node-ID list.

The emitters are`0x6AB080` and`0x6AB190`; send wrappers are`0x6AAE30`
and`0x6AAF50`. No socket envelopes, encryption or live traffic is modeled.
Original class RTTI and constructor codes are verified independently;
the two similar names are not collapsed into one protocol.

Frame consumer`0xE65290` invokes endpoint virtual+8 with owner+1D0 and
its collected root/node vectors. The `tfServerProfiler` method above
matches that slot and argument shape. **The global owner's virtual+60
provider has not yet been proved to return this concrete class**:
the static global VA`0x1DFC82C` points to pointer cell VA`0x1E00470`,
whose runtime installer/provider remains unresolved. Consequently the
new codec and concrete selector are established, but a continuous
frame-consumer-to-provider call path and formal meaning of owner+1D0
are not asserted.

### Message58 pause/unpause update

Handler `0x496680` applies the shared clock parameter schema via `0x4936B0`.
The five QWORDs correspond to EnterZone's initial tick, reference tick,
double-rate bits, upper-limit tick and offset bits. The final byte is
normalized exactly as in EnterZone; descriptor/native padding is not wire
data. The stored copy begins at primary connection+8E0.

`MatrixClockUpdateNativeLayout.cs` saves the distinct selections:
stored upper-limit flag change emits runtime110 `"paused"`; the full incoming
parameter object is then copied. Incoming paused/running-unpaused sets the
running bound at connection+950 and flag+948. Repeated paused updates **do
not overwrite an already-active running bound**. Incoming unpaused with a
running bound rebases running reference+920 to the incoming **initial** tick,
rate bits+928 to the incoming rate, and offset+940 to wrapping
`old bound - incoming initial tick`, then clears+948. Otherwise these running
values stay unchanged. Native assertions require initialized byte+918 and
running offset equal incoming offset after the branch. The model reports
those assertion conditions, rather than silently treating mismatches as
successful initialization; it does not invoke callbacks or emulate copy padding.

## Final server routes63-68: samples, LFG and shared maps

`MatrixServerMessages63To68Wire.cs` saves the final six descriptor bodies:

| Code | Factory / reader raw offsets | Wire body | Minimum packet |
|---|---|---|---|
| 63 | `0x33DFDA` / `0x34B570` | Two raw LE16 at descriptor28/2A | 5 |
| 64 | `0x33E03D` / `0x34B470` | Seven raw LE16 at28..34 | 15 |
| 65 | `0x33E0A0` / `0x34B420` | QWORD28 match ID, DWORD30 LFG category, QWORD38, NUL leader name40, chunked NUL text list44 | 23 |
| 66 | `0x33E103` / `0x34AF70` | QWORD28 match ID, QWORD30, NUL leader name38 | 18 |
| 67 | `0x33E166` / `0x34AE90` | QWORD28 author ID, NUL author/text30/34, DWORD38 flags | 15 |
| 68 | `0x33E1C9` / shared `0x355140` then `0x355E50` | Two mandatory byte-keyed maps of WORD16 and WORD32 | 3 |

Original65/66 logs explicitly name **LFGMatchFound**, **LFGLeaderChange**,
match ID and leader name. Category ID is named by the65 log. The other
QWORD and the list of names/texts are kept positional: sender/field type
identities are not assigned solely from their use. Lists retain duplicate
texts and ordering; count255 requires continuation including terminal0 for
exact multiples. Message68 uses the same unsigned-byte-key ordering and
last-value-wins maps as EnterZone, but **without an optional presence marker**.
Handler `0x4925F0` copies both maps via `0x3563E0` into primary connection+8BC,
setting the destination optional's presence byte true even for two empty maps.

### LFG is separate from challenge state

The existing mode-specific object's LFG presence byte is+48, match ID+50.
These are not the active-challenge pointer+E4 or its ID fields.
Handler65 `0x492400` gets the current mode object, chooses stored+50 when
presence is true, otherwise global pair VA`0x1E68530`, and only dispatches
to `0x6F4A50` when the selected ID is0 or upper56-all-FF. A valid selected
ID takes the `"already in match"` log path; no incoming-ID match is needed.
Handler66 `0x4924D0` selects the same pair but requires **exact two-word
equality with incoming match ID**, then calls `0x6F4C70`.
`MatrixLfgNativeLayout.cs` saves these different gates, taking the inactive
pair explicitly rather than inventing its runtime value.

The match-found consumer requires local client and its+4D20 member, asserts
no existing LFG presence, prepares a30000ms interval record, and initializes
the mode optional through `0x6F5CC0`. It compares descriptor QWORD38 to
mode+8/C, then either processes list entries or delegates leader-name lookup
to `0x6E8BB0`. Leader-change consumer similarly requires the local-client
gate, asserts presence, stores leader text at mode+5C and the second incoming
QWORD at mode+120/124, then chooses `0x6E81E0` versus `0x6E85D0` by equality
against mode+8/C. Full leader/party invitation actions are not modeled.

Earlier heuristic IDA names called `0x6F4A50`/`0x6E8BB0` quick-chat handlers.
The direct65 path and original LFG logs contradict that classification;
their names/comments are corrected to the bounded LFG/leader-name roles.
This does **not** change the separate RTTI-backed Character command144
`PerformQuickChatCommand` model or its proven constructor at `0x532AC0`.

### WORD sample receiver and zone chat67

63/64 handlers `0x491F90`/`0x4920B0` convert unsigned WORDs to float and
call sample recorder `0x481430` on global VA`0x1E0004C`, separately checking
receiver byte4 nonzero and byte5 zero before each call and taking its lock.
`MatrixWordSampleNativeLayout.cs` records exact descriptor order and
receiver destinations:63 ->2DC/2EC;64 ->34C/35C/36C/37C/2FC/30C/33C.
Native tag object VAs are retained, but metric meanings and recorder
aggregation are unresolved. The model does not pretend a snapshot gate
reproduces changing globals or locking between calls.

67 handler `0x492550` requires a mode-specific object and passes author ID,
author/text values (null shared refs substituted by empty strings), constant1,
DWORD38 and constant0 through `0x6E2590`. Its initially-absent temporary optional
selects the helper's generated-label branch: category1 via `0x80CC90` is
original `"ZONE"`, copied and lowercased to `"zone"` before `0x6E3460`.
Following that consumer proves the field names and status masks below.
`MatrixZoneChatPayload` retains raw text bytes and full DWORD; it does not
assign a text encoding.
