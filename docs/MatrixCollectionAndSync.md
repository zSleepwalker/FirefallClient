# Matrix collection, synchronization, and metadata

## ForceUnqueue, remaining-buffer and collection messages56-61

`MatrixServerMessages56To61Wire.cs` saves these normal-buffer bodies:

| Code | Factory / reader raw offsets | Body, excluding compact header | Minimum packet |
|---|---|---|---|
| 56 | `0x33DD16` / `0x34FAA0` | byte28 reason, chunked QWORD list2C, DWORD38 | 7 |
| 57 | `0x33DD79` / `0x34F380` | **Entire remaining buffer**, copied through descriptor+28 virtual+10 | 1 |
| 58 | `0x33DDDC` / shared `0x356B70` | Five QWORDs followed by one nonzero-normalized byte | 42 |
| 59 | `0x33DE40` / `0x34F0D0` | Chunked QWORD list28, DWORD34 | 6 |
| 60 | `0x33DEA3` / two calls to `0x34E700` | Two independent chunked lists at28/34; each element WORD0 + NUL raw text4 | 3 |
| 61 | `0x33DF12` / `0x34D6D0` | Chunked roots: WORD0 then recursive node list4 | 2 |

Collections preserve order and duplicates. Every count255 requires a further
chunk; exact multiples terminate with0. Message57 has **no** WORD-length
prefix, chunk count or assumed text encoding. Its supplied span must end at
the native buffer boundary: bytes that would be trailing data for another
decoder are part of57's payload. Handler `0x491F10` obtains descriptor-buffer
pointer/length through virtual methods and, if global byte VA`0x1E4AC84` is
set, passes them to `0x124B3F0`.
RTTI now proves57 is `matrix_fury::SynchronizationRequest`; the bounded
integrity/dispatch envelope of that consumer is reconstructed below.

Original handler56 logs identify **ForceUnqueue**. Handler `0x496460`
forwards reason/DWORD/list to consumer `0x85DCC0` through the same gated
downstream pointer used by42/44. The consumer clears recorded queue state
and its record vector, then emits runtime291 properties `"reason"`,
`"local_player"`, `"text"`. Original20-entry reason table
VA`0x1C6FD10` is saved verbatim in `MatrixForceUnqueueNativeLayout.cs`:
code0 is mixed-case `"Unknown"`; codes20..255 use uppercase `"UNKNOWN"`.
The local-player comparison ignores the low8 ID bits, requires the remaining
56 bits to be nonzero, and substitutes0 when the local-client getter is null.
This is **not** the exact challenge-ID comparison policy. The model records
the raw DWORD that supplies `"text"` formatting, not an invented formatted
string; log-only entity searches and native property ownership are omitted.

Message59 is identified by original `"SuperPing timing names out of date?"`
diagnostics, but its **original descriptor class is `matrix_fury::SuperPong`**.
Existing SuperPing-labelled C# payload names are compatibility labels from
that diagnostic, not claims about the RTTI name.
Handler `0x496690` ignores an empty input vector. Otherwise it
samples the connection clock and appends one raw QWORD via `0x4849A0`.
DWORD34 equal1 forwards to `0x484280`; other values assert **six total**
QWORDs after append and log timing deltas. The C# decoder preserves the
received list, not the later clock sample; diagnostics and downstream timing
aggregation are not simulated.

Message60 handler `0x4967B0` only operates if global VA`0x1E6A470` is nonnull:
it passes both lists to `0xE64A00(global+190,...)`, then writes a sampled
DWORD tick minus5000 at global+18C, wrapping uint32.
`MatrixServerProfilerNames60NativeLayout.cs` now saves the consumer's logical
two-map updates. The first map is global+190 and the second global+1B0;
both use **complete unsigned16-bit keys**, despite the lookup's inferred
byte-pointer argument type. Lookup`0xE671C0` hashes both little-endian
key bytes with plain FNV1a and compares a WORD at node+8.
Initializer`0xE6AF80` stores the complete WORD and zeroes text pointer+0C
in a0x10-byte node. Each source vector uses8-byte entries, WORD0/text4.

Updates preserve unmentioned keys, find-or-insert new keys, then replace
the shared-text reference. Repeated keys are therefore last-value-wins,
including empty text. Empty lists do not clear either map, but the enabled
handler still writes the tick marker. Missing global skips everything.
The C# projection independently copies raw text bytes into two logical
dictionaries and reports missing-consumer explicitly; native reference
counts, pointer-equality fast paths, allocation limits and iteration order
are not simulated. The two tables' distinct **name categories** remain
unresolved; do not label them as threads/functions solely from profiler RTTI.

Message61 `0x4967F0` checks that same global and tail-calls
`0xE65290` with descriptor+28. Its missing-name/refresh gates are now
reconstructed below; no formal event identity or metric units are assigned.

Message61's native root stride is16: WORD0, padding, child-vector4.
Its recursive native node stride is32: WORD0, padding, child-vector4,
eight WORDs at10..1E. **Wire order is WORD0, those eight WORDs, then the
chunked child list**, not native memory order; a leaf takes19 wire bytes.
The node reader `0x34BE80` calls itself after the nine WORDs; the root reader
calls that same node reader after its single WORD. The saved decoder uses
an explicit stack so depth does not impose an arbitrary CLR recursion limit.
No field labels or relationship semantics are guessed.

### Message57 SynchronizationRequest integrity and routing envelope

`MatrixSynchronizationRequest57NativeLayout.cs` models a **post-transform**
buffer, excluding the compact57 header. Native consumer raw`0x124B3F0`
enters the critical section at owner+58. A nonzero byte at owner+70
rejects entry; the byte's formal state name remains unknown. Lengths below20
return false. With owner mode byte+1C nonzero, a function pointer at
`[owner+28]+14` is called with mutable buffer and total length **before**
digest verification. Its transformation and inverse remain unknown and are
not executed by the reconstruction.

The last16 bytes are a standard MD5 digest of all preceding bytes. Evidence:
the four standard initial state words in the consumer, all64 MD5 additive
constants and exact ordered rotations in transform`0x1283FE0`, standard
Boolean rounds, update`0x12847D0`, and finalizer`0x12848A0`. The finalizer
uses VA`0x1C0E4A0` padding (`80` then63 zero bytes), appends the original
64-bit bit count, and emits the four state words little-endian. Native
comparison tests all four trailing DWORDs; there is no additional hash
fold or secret key. This is legacy integrity checking, **not authenticated
origin verification**.

On a match, `0x124AC10` passes the bytes preceding the digest to
`0x120C250`: their final DWORD is an uncompressed allocation length, and
the earlier bytes are a zlib stream. The destination is prefilled with FF;
`0x1256A60` inflates with window15/finish4 and requires stream-end1 and
zero unused compressed input. It does **not** require total output to equal
the allocation length, so a shorter successful output leaves an FF suffix.
Only then does `0x11FC1C0` read the **inflated buffer's** first little-endian
WORD and advance its cursor2.
Nonzero sampled mode+1C selects registered-handler lookup at owner+38 through
`0x124C010`; zero selects direct dispatcher`0x124B7C0`. **This is not proven
queue insertion**: the lookup finds a hash-map entry and invokes its callback.
The key's formal meaning remains unresolved. Native returns
the downstream result, not merely digest validity; C# therefore reports
`EnvelopeVerified`, **not complete handling success**. Its dispatch and
external-transform selections describe sampled boundaries only, not native
locking, pointer ownership or downstream execution.

The minimum20 means at least4 preceding payload bytes for the length footer;
it does not establish a valid compressed stream or a readable inflated key.
Empty whole-remainder decoding for57
remains valid at the wire-reader layer and is rejected at this consumer
layer. Explicit C# outcomes distinguish gate rejection, short buffers and
digest mismatch; verified payload/footer are independent byte copies.
Integrity-only golden: ASCII`abcd` followed by
`E2FC714C4727EE9395F324CD2E7F331F`, raw prefix WORD`0x6261`.
This is not a valid native reader payload. `RawPayloadPrefixWord` deliberately
does not label that prefix a routing key. `NativeZlibBufferFraming` projects
captured inflater results; compression/inflation engines are not executed by
these offline projections, nor is a different compressor claimed byte-identical.

### Synchronization mode-zero fields and registered callbacks

`MatrixSynchronizationDispatchNativeLayout.cs` continues the bounded trace.
In mode0, `0x124E6E0` calls DWORD reader`0x1206B20` **twice**, consuming8
bytes after the already consumed leading WORD in the inflated reader buffer. Each read checks cursor+4,
reads a little-endian DWORD and advances4; short input throws natively.
The pair is **decoded DWORD values, not a pointer/length pair**. Only after
both reads does dispatcher`0x124B7C0` check:

| Gate | Native failure |
|---|---|
| Leading key must equal1 | Diagnostic callback receives raw`0x0FFACB37`, returns0 |
| Second decoded DWORD must equal8 | Diagnostic callback receives raw`0x0FA98FC0`, returns0 |

The second value8 is **not the remaining buffer length**. Trailing bytes
after the pair are not checked by this path. Thus a verified4-byte payload
can still fail reader construction or the mode0 readers; at least10 **inflated**
bytes are needed for these fields, not a fixed26-byte wire minimum.
Short data is detected before unsupported
key/value diagnostics. C# preserves completed read progress and reports an
explicit truncated-field outcome instead of attempting native exceptions.

The first DWORD is passed as the seed to`0x127FFD0`: it initializes624
words with `state[0]=seed` and wrapping
`state[i]=i+1812433253*(state[i-1]^(state[i-1]>>30))`, the MT19937 seeding
recurrence. The later activation calls remain unmodeled, while the bounded
optional digest slots are now saved below. If native activation reaches its tail at`0x124B9D2`, owner
mode+1C is set1. C# says `ActivationPathSelected`, not that initialization
or native side effects have run.

`MatrixSynchronizationModeZeroDigestSlots` projects the descriptor's two
16-byte slots at`+4` and`+14`: both are explicitly initialized to zero.
Helper`0x127C730` is called first into a reusable temporary; only a true
return copies it into the first slot. Adapter-address MD5`0x127D0C0`,
called at`0x124B93A`, then uses the same temporary and only copies to the
second slot on success. A failure must **not copy the previous helper's
stale bytes**. Either optional helper's false return leaves its corresponding
slot zero without rejecting the already-selected branch.
The adapter algorithm is saved in
[Startup](Startup.md#sorted-adapter-address-md5-offline-projection).
The first helper queries a Windows-directory-derived drive and a storage
descriptor. Its serial-number/CString MD5 projection is now saved in
[Startup](Startup.md#windows-drive-storage-serial-md5-offline-projection);
`ProjectCapturedFingerprints` wires the two offline result types together.

The C# projection returns independent copied flags and owned16-byte slots.
An unknown native result is not treated as false: its slot stays unresolved,
while the other slot can retain a known snapshot. Successful captures must
contain exactly16 bytes; failed attempts need no readable output. These
results describe captured slot contents, not successful native activation,
allocation or transformation-provider binding. Outgoing serialization is now
saved separately below.

In nonzero mode, `0x124C010` instead finds a key in the **hash map** at
owner+38 using`0x124C9B0`. Missing entry returns0; found entry's callback
pointer at node+0C is invoked through virtual slot0 with the key and the
reader reference, whose cursor is still2. Its returned byte is normalized
to bool. This corrects the earlier tentative **queued path** description:
no queue insertion has been established.

Bucket helper`0x1248050` hashes all4 little-endian bytes of the zero-extended
key using **plain FNV1a32**, without the engine's final mix. It computes
`bucket=hash&mask` (container+14), then uses the bucket unchanged if active
count (container+18) is greater; otherwise subtracts `1+(mask>>1)`, wrapping
uint32. Shared `EngineByteHash32.ComputeFnv1a` now supplies that first stage,
while existing `Compute` retains the complete chat/SDB final mix unchanged.
Golden plain hashes: key0→`4B95F515`,1→`FB69B604`,1234hex→`83CE9A1B`,
FFFFhex→`16C70CDB`. Callback registration, implementations, and external
transform function selection remain open.

### Mode-zero operation2 descriptor and compressed framing

`MatrixModeZeroDescriptorCodec` saves the serialized fields passed by
`0x124B7C0` into `0x124E010`/`0x124E720`. A WORD2 precedes:

| Native descriptor | Serialized value |
|---|---|
| +0 | Little-endian DWORD Windows version code |
| +4 | Raw16-byte Windows-drive serial digest, or initialized zeros |
| +14 | Raw16-byte sorted adapter-address digest, or initialized zeros |
| +24 / +28 | Opaque DWORD and ushort byte count followed by narrow bytes |
| +40 / +44 | Second opaque DWORD and ushort byte count followed by narrow bytes |

Offsets here are hex. Serializer`0x124FFE0` writes the DWORDs as raw4-byte
ranges; `0x12504E0` writes each digest's16 bytes; `0x120BEF0` rejects a
stored string length aboveFFFF and writes exactly that count, without a
terminator. Embedded zero/high-bit bytes are preserved. Metadata copier
`0x124AA60` takes two DWORD/wide-string groups from the singleton's+F8
member and runs each string through`0x1273530`. Its tail is now recovered:
folder/path normalization is attempted, then per-code-unit locale
`towlower`, middle shortening and strict UTF8 conversion. The provider and
metadata identities remain unresolved. The descriptor codec accepts captured
**converted narrow bytes**; the bounded tail below can produce those bytes
from already captured normalized/lowercased text.

`NativeWindowsVersionCode` projects`0x12800F0`: captured GetVersionExW
OSVERSIONINFOEXW284, failure→0, otherwise wrapping
`((major<<8)|minor)<<8 | servicePackMajor`. Its argument can suppress the
service-pack bits; mode0 passes zero. Exact packed codes50000,50004,
50100..50103,60000..60002,60100..60101,60200 map respectively to1..12;
other packed values greater than60200 map13, other values map0.
This legacy API result can be compatibility-dependent; it is not a claim
about the actual host OS, which was not queried.

`0x11E8910` replaces the serialized stream with zlib output (native literal
version1.2.8, default level-1→6, deflate8, window15, memory8, strategy0,
finish4), then appends the **original uncompressed byte length** as LE32.
`0x124BC10` appends MD5 of the compressed bytes **and that length footer**.
`NativeZlibBufferFraming.AppendLengthAndMd5` reconstructs the framing from
captured zlib bytes; it does not assume managed compression is byte-identical
to the native1.2.8 engine. Synthetic independent zlib fixtures are labeled
as such, not captured game traffic.

`0x124E010` calls either the explicit provider's+10 function pointer or
the owner's+28 provider's+10 pointer with the mutable framed buffer and
its length, then queues its owning buffer under the owner+4 lock.
The selector supplies the generated provider from`0x1285060`/`0x1286F00`.
That callback and later send/ownership behavior are not executed or
identified as a particular cipher. The saved bytes are **before callback
transformation**, not a complete outgoing packet or proof of activation.

### Metadata text shortening and strict UTF8 tail

`NativeMetadataTextTail` reconstructs`0x1283720`, `0x1258C70` and
`0x1258E90` after the earlier normalization and locale case-conversion
boundary. Text with at most64 UTF16 code units is unchanged. Longer text
is first reduced to its first32 and last32 code units; native replaces
three units starting at31 with UTF16 literal`...` (VA1C7EC5C).
Equivalently, the final64 units are **first31 + dots + last30**.
These are code-unit offsets, not Unicode scalar or grapheme counts.

Encoding walks the full stored range, preserving embedded zero. High
surrogates require a following low surrogate; an orphan low surrogate,
missing low surrogate or mismatched next unit triggers native invalid-UTF16
handling, not replacement characters. For a mismatched pair the first
native diagnostic carries the following unit. Shortening can itself split
an otherwise valid pair at either retained boundary; this failure is
preserved. Scalar encoder accepts0..10FFFF excluding D800..DFFF and emits
standard one-to-four-byte UTF8, without BOM or appended terminator.
The checks compare every valid Unicode scalar against independent standard
UTF8 and exercise exact64/65 boundaries and both surrogate-splitting edges.

`NativeFolderPrefixAlias` now saves preceding helper`0x126DAC0`. It first
trims **one** trailing slash/backslash through`0x126D750` and removes the
case-sensitive prefix `\\?\` when present. The four-unit global prefix is
proven by static initializer raw2E33D0 and UTF16 literal VA1C7EA48; it is
not inferred from current host state.

Folder IDs are tried in the fixed order254,255,37,41,36,38,42,5,28,26,
35,0,25,40,7. ID254 uses module-directory provider126CBD0(selector3); ID255
uses GetTempPathW(buffer260), allowing only return1..260; others use
SHGetFolderPathW and require HRESULT0. Captured available roots lose one
trailing separator. `0x1270D40` compares the prefix by applying locale
`towupper` to each UTF16 code unit. A matching root must consume the whole
candidate or be followed by **backslash**, not forward slash. The first
match wins, not the longest root. It replaces the prefix **and one following
backslash** with `<decimalID>`; root+`\Foo` becomes `<ID>Foo`, without
reinserting a separator. No match makes1273530 fall back to the **original**
input, discarding partial trimming/prefix removal.

C# uses captured root availability/paths and explicit captured case mapping.
An unknown earlier provider cannot be silently skipped; non-ASCII mapping
is not guessed by its explicit ASCII/C-locale helper. `0x1273530` then
invokes locale `towlower` before the saved shortening/encoding tail.
No host folders, temporary paths or locale state are queried. Metadata
ownership/meaning and full captured lowercase integration remain
open; the representation algorithms do not identify either opaque
DWORD/string pair.

`NativeModulePathProvider` projects provider126CBD0 from captured
GetModuleFileNameW(NULL) attempts and `_wsplitpath_s` outputs. Its initial
buffer length is260 UTF16 units. Every sampled GetLastError122 retries with
`260+4*previous` (1300,5460,22100...), **irrespective of the return count**.
Native does not clear LastError before querying: even a nonzero return can
retry on stale122. A terminal non122 error permits any nonzero return count;
zero returns false. Query success is followed by split-helper126D9E0, which
requires nonempty input and CRT result0, using drive capacity3 and directory,
filename and extension capacities256. The C# model accepts captured
components rather than substituting platform-dependent `Path` parsing.

The selector is an enum, not flags:0 full module path,1 filename without
extension,2 filename+extension,3 drive+directory. Unsupported selectors
return false **after** query/split gates. Selector3 supplies alias254, whose
trailing directory separator is removed by the alias routine. Missing
attempt/error/split/text captures remain unresolved. Original PE switch
table VA166DAE0 maps to raw126CEE0; correcting IDA's mistaken raw/VA table
mapping and four cases recovered decompilation without changing any
executable bytes.

### Generated transformation provider: saved initialization prefix

`NativeGeneratedTransformProviderPrefix` saves only the initial deterministic
stage of constructor1285060/generator1286F00. Constructor stores RNG at+8,
zeros DWORDs+4/+C/+10/+14, sets byte+18=1 and invokes generation.
Generation draws an inclusive modulo-reduced count60..100 **first**, sets
WORD+0=0200 and byte+2=3, allocates264 bytes at+C, then fills each byte from
one virtual UInt32 draw's low byte. Thus the saved prefix consumes exactly
265 UInt32 calls. `FromSeed` uses the existing MT19937 reconstruction; its
lazy initial twist preserves the native immediately-twisted stream.

Further generation selects candidate spans by raw sample modulo count,
does the selected number of descriptor/template iterations, and assembles
two outputs stored at+10/+14. Bounded helper evidence identifies12867D0
selection,12866C0 descriptor-dependent dispatch and12868D0 span concatenation.
Its allocation requests total span bytes+16 through1250F90 with size-class
paths. This does not establish executable page protection or a cipher.
The later caller invokes+10/+14 as mutable-buffer callbacks, but **no
generated byte buffer is executed or converted into a delegate** by C#.

`NativeGeneratedProviderBoundaryPlanner` now saves the abstract selector
schedule in builder helpers1285290/1285970 for provider DWORD+4==0.
The opening ten operations are29,26, six29s,26,26; descriptor codes are
42;42/41;37,38,39,40,43,44; headerByte0+37 with code42/offset8;
headerByte1+37 with code42/offset12. Closing uses six28s with reversed
codes44,43,40,39,38,37, then25 and33 with a16-bit value8.
Operation codes now have evidence-backed names:25 LEAVE (C9),
26 MOV (89/8B),28 POP (58+index),29 PUSH (50+index,6A/68),
33 near RET (C2 with16-bit immediate or C3 without operands).
Code37..44 width32/index0..7 therefore names EAX,ECX,EDX,EBX,ESP,EBP,ESI,EDI.
This is a **call plan**, not an emitted instruction encoding. Mode1's
different schedule and operand/template encoders remain unmodeled.

`NativeGeneratedBoundaryEncodingCatalog.InitialModeZero` separately saves
the proven boundary candidates for initial header0200/mode0. Opening bytes
are `55 {89 E5 | 8B EC} 50 51 52 53 56 57 8B 45 08 8B 55 0C`;
closing bytes are `5F 5E 5B 5A 59 58 C9 C2 08 00`.
The two MOV EBP,ESP alternatives retain static table order (variants1/3
of operation26); other initial boundary steps each match one template.
This preserves registers, loads EAX/EDX from stack offsets8/12, and returns
while removing8 argument bytes. Argument semantic types remain unproven.
Candidate construction precedes the selector's single UInt32 draw, even
when only one candidate exists. Wrapper condition/tail construction and
their selector draws occur between opening and closing; transformation
body fragments are already constructed before either wrapper runs.
The catalog consumes no RNG and is not a full routine.

`NativeGeneratedDescriptorFactories` projects the numeric code classifier
128A840 and the sparse constructors12506F0/1250780/1284F90/1250850.
Code0 produces kind/width/index0; bands1..20,21..36,37..52,53..68
produce kind4 with widths8/16/32/64 and indices relative to the band start.
Bands69..74,75..82,83..90,91..106,107..122,123..138 produce
kinds5/6/7/8/9/10 and widths16/64/64/128/32/32 respectively.
Out-of-range signed codes store the supplied DWORD before diagnostic
1040197660; no succeeding wrapper writes are modeled on that path.
Names of register families are not assumed.

Operation records have three64-byte operand slots at8/72/136.
Unary/binary constructors copy the entire supplied slots; absent slots
receive only the observed sparse field clears, **not**64 zero bytes.
Header+4 and unassigned operand bytes retain captured preexisting storage.
The C# projection requires nonoverlapping captured buffers; alias-dependent
SIMD copy behavior remains outside its contract.
Immediate operand factory1284EF0 writes kind2, value DWORDs at40/44,
width DWORD48 from the input's **low byte**, and byte56=1.
Its final explicit argument is unused; holes remain captured storage.

`NativeGeneratedImmediateBytes` saves1299D00's immediate-width dispatch:
it uses `bitWidth >> 3`, accepting byte counts1/2/4/8, not just exact
bit widths8/16/32/64. Other counts diagnose1040336665.
Bytes are the corresponding low1/2/4/8 little-endian bytes of the supplied
64-bit value; fixed native16-byte builder capacity remains a separate gate.
Its switch metadata was repaired from unchanged original PE targets at
raw1299DD8; no binary bytes were changed.
Both this switch and the module-path switch are now user-defined so IDA
automatic analysis cannot replace the corrected raw targets with PE VAs.

`NativeGeneratedCandidateRngAdvance` saves1286780: when byte+18 is zero,
sum the last selected candidate's unsigned bytes with UInt32 wrapping,
then discard sum%5 raw UInt32 draws. It emits no extra bytes.
Any nonzero byte+18 skips the candidate read and the RNG calls entirely.
Constructor's initial byte+18=1 therefore disables these advances.
No writes to byte+18 were observed in1286F00 or12868D0.

`NativeGeneratedBodyDecisionRules` saves initial mode0 decision prefixes
and the seven static forward/reverse pairs at PE VA1C0E464:
ADD/SUB, SUB/ADD, INC/DEC, DEC/INC, ROL/ROR, ROR/ROL, XOR/XOR,
selected by a raw UInt32 modulo7. Numeric IDs are1/41,41/1,6/5,5/6,
34/35,35/34,43/43; identities follow their native emitted x86 opcodes.
Unary/arithmetic parity selects BL(width8) or EBX(width32).
The rotation helper's first range0..2 draw chooses a count-register path
only on1 with initial header0,2,3; its next parity selects CL or ECX
for the preparatory immediate MOV, while the rotation itself uses CL.
Register arithmetic uses DL/EDX when the parameter is odd; otherwise
it first loads the parameter into CL/ECX.

Table-source arithmetic uses width8 for odd parameters, otherwise32;
scale is1/2/4/8 from bits1..2. For scale>1 it derives an ESI index and
shrinks the base-offset window before shifting the remaining parameter
by8. EDI receives the captured table address plus that bounded offset;
effective table offset is always below256. Original table allocation264
bytes accommodates the wider reads. Pointer addition wraps as UInt32;
the model does not dereference or obtain a host address.
Full template lowering and output routine behavior remain incomplete.

`NativeGeneratedConditionalBranches` saves JNZ18/JZ22 shaped candidates.
Short predicate is exactly `(offset & FFFFFF00)==0` excluding128/129:
it accepts0..127 and130..255, **not negative offsets**.
Accepted130..255 values truncate to negative signed x86 rel8 bytes.
This is a verified native quirk, not corrected to conventional rel8 bounds.
Near predicate excludes Int32 minimum through minimum+5. Negative near
offsets subtract6 before LE32 encoding; nonnegative offsets stay unchanged.
Candidate order is short then near; no matches diagnose1040188017.

`NativeGeneratedByteLoopWrapper.AssembleCapturedBody` saves12868D0's
initial mode0 wrapper around externally captured body bytes. It selects
opening, OR EDX,EDX, store BL->[EAX], INC EAX, DEC EDX, JNZ back,
JZ skip, closing—in that **construction/RNG order**—then concatenates
opening, OR/JZ, body, store/INC/DEC/JNZ, closing.
It consumes exactly24 raw UInt32 draws, including single-candidate steps.
OR has09D2/0BD2 alternatives; INC EAX has40/FFC0; DEC EDX has4A/FFCA.
Backward offset is minus(body length+store/INC/DEC length), with the JNZ
encoder subtracting its own6 bytes. Skip offset includes body and full tail.
Both output bodies are built before either wrapper; inverse records are
reversed in12-byte vector units, never reversing bytes within a candidate.
C# projects logical byte concatenation only: native executable protection,
allocator size classes/pointers and full body lowering remain unresolved.
Managed array/length overflow is explicit rather than emulated native wrap.

`NativeGeneratedScratchTemplates` lowers initial scratch INC/DEC and
byte ROL/ROR alternatives. BL unary has one FE candidate; EBX has compact
43/4B followed by FFC3/FFCB. Immediate byte rotations use C0C3/C0CB
plus the captured count byte. Count1 adds D0C3/D0CB **after** that candidate;
CL rotations use D2C3/D2CB. Counts are not normalized modulo8 in the
emitted data. `NativeGeneratedImmediateMoveTemplates` saves ordinary
mode0 register MOV preparation: B0+index/imm8 then C6/ModRM/imm8;
B8+index/imm32 then C7/ModRM/imm32, preserving native table order.
The captured table pointer is encoded as a DWORD, never obtained from the
host. `NativeGeneratedArithmeticTemplates` now lowers the initial BL/EBX
ADD/SUB/XOR forms. Direct immediates have exactly one80/81 group candidate;
there is **no83 sign-shortened candidate** in the eight-entry native tables.
Register CL/ECX or DL/EDX sources have r/m-first then register-first
alternatives in table order. Table sources have one register-first candidate:
ModRM1F for `[EDI]`, or ModRM1C/SIB77,B7,F7 for `[EDI+ESI*2/4/8]`.
The pointer/base-offset and index are separate preparatory MOV fragments;
the memory operand has zero displacement and no address-size prefix.
These projections do not cover arbitrary native descriptors or mode1.
`NativeGeneratedInitialModeZeroImage` now combines these recovered slices
into both logical byte images for a fresh provider. The caller supplies a
**captured numeric table address** because native allocation is outside the
model. All UInt32 samples are retained for replay; no host address is queried
and the byte arrays are never executed or made into delegates.

RNG ordering is265 prefix draws, one forward `MOV BL,[EAX]` selection,
all iteration draws, one inverse load selection, then24 selections for each
wrapper. Each iteration first draws the seven-pair selector. Unary pairs
consume five total draws; rotations consume one shared low-byte count and
two independent operand-form selections, six/eight/ten draws in total.
Arithmetic draws one shared modulo3 form and one shared UInt32 parameter,
then builds forward and inverse independently: seven draws for immediate,
odd-parameter register, or unindexed table forms; nine for even-parameter
register or indexed table forms. Forward fragments retain construction
order. Multi-fragment inverse groups are reversed locally, then the complete
inverse list is reversed after appending its load; thus preparatory MOVs
remain before their associated inverse operation.

All-zero samples produce60 iterations,735 calls and two405-byte images.
Constant2 produces62 unary iterations and625 calls; constant4 produces64
CL-path rotation iterations and955 calls. Scripted register/table streams
check independent parity/template selection and both reversals. This is a
complete **logical initial-mode0 byte-image projection**, not a reconstructed
native executable provider. Allocation and ownership are saved separately as
captured numerical plans; no native allocation, execution or complete mode1
image is supplied. Existing branch-displacement quirks are retained.

`GenerateWithCapturedControlByte` additionally accepts an explicit captured
byte+18 override. Any nonzero value disables post-candidate RNG discards;
zero enables byte-sum%5 raw draws at the exact native helper call sites.
Unary selections and rotation selections (including preparatory CL/ECX MOV)
advance; register-source arithmetic advances after its optional MOV and the
arithmetic selection; table-source arithmetic advances after optional ESI MOV
and the arithmetic selection. **Direct immediate arithmetic, table-base EDI
MOV, initial scratch loads and wrapper selections do not advance.** Each
discard happens before the next selection and before inverse-group reversal.
The full replay ledger includes discarded samples and reports their count.

The constructor still writes1: no native setter/entry proving a control0
provider was identified. The explicit override does not change the prefix's
constructor fact or the default `Generate`/`FromSeed` behavior. Captured-control
regressions include constant streams and a1395-draw register transcript whose
discard values would select different forms if accidentally reused.

`NativeGeneratedArenaPolicy` saves numerical output-allocation policy without
performing allocation. Arena1250F90 adds16 bytes with DWORD wrap, searches
one preferred list for total block sizes<=64/128/256 (arena offsets4/36/68),
then the large list at100. Best-fit1251050 scans under the list's critical
section, selects the smallest sufficient block, and retains the first entry
among equal sizes. A remainder>=32 is split and returned to its size-class
list; a shorter tail stays with the allocation. Returned payload is header+16.
Growth12510F0 rounds the required size to64KiB with wrapping DWORD arithmetic,
then uses the original PE import `KERNEL32!VirtualAlloc` with address0,
allocation flags0x3000 (`MEM_RESERVE|MEM_COMMIT`) and protection0x40
(`PAGE_EXECUTE_READWRITE`). It uses the same32-byte split threshold.
The projection exposes wrapping sizes, including zero; it does not pretend
an allocation succeeds. Critical-section import identities are independently
checked against original import slots. Free blocks have captured address
tokens, not dereferenceable managed pointers; mutated lists and native locks
are not simulated.

Provider destructors12850F0/1285140 release nonnull RNG+8 via virtual slot4
with deleting argument1, free nonnull table+C, then unconditionally call
output-return helper1257690 on+10 and+14. That helper lazily obtains the arena,
then returns nonnull payload-16 through1251160; it does not free the region
throughVirtualFree on this path. `NativeGeneratedProviderLifetimePlan` saves
this release order as data, including null output calls. Provider replacement
124B1A0 performs old-provider cleanup/free under the owner lock, assigns
owner+28, clears byte+24 and signals the owner's event.

Thunks1285120/1285130 are **tail jumps** through+10/+14, not no-argument
function calls as the decompiler suggests: caller stack words are retained.
The generated mode0 opening loads buffer/count from EBP+8/+C and closes
withRET8. These are two32-bit stack arguments with callee stack cleanup;
no static xrefs were found to these thunk entries. The two observed direct
provider consumers do not use a callback return value: incoming124B3F0 calls
provider+14 at124B48D before MD5; outgoing124E010 calls provider+10 at124E085
before the queue lock. This does not establish a meaningful generated return
value or all possible callers. Saved ABI constants are facts about the emitted
mode0 image, not callable C# delegates.

`NativeGeneratedInitialModeZeroByteTransform` additionally saves the
**managed low-byte algorithm** for positive-length buffers. It evaluates
recorded logical steps, not the emitted machine-code arrays. INC/DEC and
ADD/SUB/XOR reduce to wrapping byte effects even when native templates use
EBX; higher scratch bits do not feed any later low-byte operation. ROL/ROR
affectBL only. The operand is the captured parameter's low byte, the
remaining buffer count's low byte for DL/EDX sources, or the table byte at
the recovered relative offset. Count starts at input length and decreases
once per byte. Rotate effects use count modulo8; emitted count bytes remain
unchanged in the image model.

Forward/inverse recorded steps round-trip all256 input values across
513-byte inputs and32 MT seeds, including the255/256 remaining-count
boundary. An independent Python MT/body-semantics reference for seed0
gives61 iterations,684 pre-wrapper draws,732 total draws and forward
SHA256 `42ff381c351aa6610e533d4d4716c49e7dac67a595c5db92f193e839096a6cf7`
for input byte`i&255` over513 bytes; this is retained as a managed regression.
This does not assign a named cipher or simulate full CPU flags,
pointer validity, register return values or side effects. Empty input is
explicitly unsupported: the native zero-count JZ can have the previously
recovered wrapped rel8 displacement, so returning an empty success would
invent control-flow safety.

Outgoing descriptor consumer124E010 invokes provider+10 on the complete
compressed/length/MD5 frame before queueing it. Incoming Request57 uses
provider+14 before MD5 verification. The opt-in
`VerifyWithReconstructedInitialProvider` method supplies the managed inverse
only when the existing gate/minimum-length/mode test selects transformation
and the caller identifies the provider as this recovered fresh mode0 type.
Skipped paths retain existing behavior and need no provider. Selected paths
require a complete snapshot; unknown providers are not silently substituted.
The existing post-transform verifier and captured-data APIs are unchanged.

The **mode1 boundary branch** is separately saved, without claiming a
mode1 constructor or whole body. `ForModeOne` records PUSH
RAX,RCX,RDX,RBX,RSI,RDI, then MOV header-derived64-bit registers from
`[RSP+32]` and`[RSP+24]`. These are the saved incoming RCX/RDX values after
six pushes. Closing POPs reverse that order and emits plainRET, with no
LEAVE or immediate stack-pop value. With default header0200,
`ModeOneDefaultHeader` emits:
`505152535657488B44E420488B54E418` and`5F5E5B5A5958C3`.
There are eight opening and seven closing selector calls.

The SIB byte is **E4, not conventional24**: original129A320 sets
scale bits to3 when no index is present. Index4 still means no index,
so those scale bits do not affect the address, but they are retained in
the bytes. MOV hasREX.W; PUSH/POP ordinary64-bit registers need noREX.
The original constructor writes mode0 unconditionally. No established
caller sets mode1. The shared captured-mode projection below exposes mode1
explicitly; the original mode0 convenience API does not silently select it.

`NativeGeneratedModeOneMemoryShape` saves the separate address validator:
present base/index operands must be kind4 with width32 or64; simultaneous
operands must have equal widths; an index with full register index4 (ESP/RSP)
is rejected, while index12 is allowed. Scale must be1/2/4/8, and scaled
forms require an index. Accepted shapes return whether a67 address-size
override is required by a32-bit address register, **not a success boolean**.
The managed result distinguishes acceptance from this returned flag and
preserves explicit diagnostic codes. Checks cover8712 shapes/2073 accepted
combinations.

The separate129A7F0 fast-path predicate is scale1/no index/base index!=4.
It compares the full index, not index modulo8; do not replace this with
canonical R12/R13 addressing assumptions. No complete mode1 encoder is
claimed. Two mode1 switch tables were PE-mapped and restored withSWI_USER:
129A4C1 -> raw129A584 and129A734 -> raw129A7D8 plus byte-index129A7E4.
Original validator return ends at129A76E; throw tail ends at129A7D8 before
its tables. The separate predicate is129A7F0..129A809, previously incorrectly
absorbed into the validator. Calculated targets match original bytes;
cached decompiler block recognition can still be incomplete.

`NativeGeneratedModeOneMemoryEncoding` additionally projects captured
ModRM/SIB/displacement writes, address-size-prefix selection and REX X/B bits
to OR into the caller's prefix. It preserves the captured ModRM register
field, signed-byte displacement boundaries, mandatory zero displacement for
RBP/indexed R13, and DWORD displacement for an absent base. The default
mode1 boundary catalog now uses this shared address projection and retains
its original byte goldens.

Native no-index R12/R13 fast paths emit rm4/rm5 without the canonical extra
SIB/mandatory DWORD displacement; the managed projection deliberately retains
these raw fields and REX.B rather than repairing them into ordinary x64
instructions. Goldens and18657 accepted boundary combinations cover the
recovered writes. This is **not a complete instruction encoder**: caller
opcode/REX.W/REX.R, prefix ordering and buffer-slot/capacity effects remain
separate. The default-header body assembler supplies its recovered instruction
forms explicitly; this memory-field API is not a general x64 assembler.

`ForModeOneOrdinaryRegister` saves immediate MOV candidates for ordinary
byte and64-bit registers. For a64-bit destination and a32-bit source descriptor,
only `48 C7 /0 imm32` matches; RCX/RSI preparatory MOVs do not acquire an
imm64 compact alternative. For a64-bit source descriptor, `48 B8+index imm64`
comes first, followed by C7 only when the full value is the sign extension
of its low DWORD. Original table-address construction hasCDQ at128624F:
wrapped32-bit pointer arithmetic is sign-extended, not zero-extended.
The byte form reuses the unchanged ordinary byte candidates. This catalog
does not claim extended-register destinations.

`UnaryForModeOneFromConsumedParitySample` saves initial scratch selection
BL/RBX and its one matching INC/DEC candidate: `FE C3`/`FE CB` or
`48 FF C3`/`48 FF CB`. The explicit ordinary-register variant also covers
the corresponding byte and64-bit register indexes0..7. Compact40..4F
alternatives are mode0-only, not mode1 REX-prefix substitutes. Byte rotation
candidates remain the existing BL immediate/CL forms.

Explicit mode1 body decisions and ADD/SUB/XOR candidates are also saved.
Parity chooses BL/RBX and CL/RCX or DL/RDX; the wide preparatory immediate
descriptor remains32-bit. Wide immediate arithmetic has one REX.W81 imm32
candidate (no83/imm64), register forms retain their two ordered alternatives,
and table forms use RDI/RSI with one register-first alternative. The264-byte
table bounds include64-bit reads through offset255. These components feed the
captured default-header assembler; they do not establish a native mode1 entry.

`AssembleCapturedBodyForModeOne` saves the enclosing64-bit byte loop: RDX
OR-test, BL store throughRAX, RAX increment, RDX decrement, the original
JZ/JNZ displacement choices, and the saved opening/closing sequence.
It consumes21 selector draws per wrapper versus24 in mode0, including draws
for singleton candidates. Branch128/129 exclusions and wrapped130..255
short displacements are retained, not normalized into canonical branches.

`NativeGeneratedDefaultHeaderImage.GenerateCaptured` now composes complete
logical forward/inverse images for explicit mode0/1 and captured byte+18.
Both architectures share the same body draw-order implementation and local
then global inverse reversals. The constructor's prefix facts stay0/1;
`ModeWordAt4DuringGeneration` and`ControlByteAt18DuringGeneration` separately
record the supplied overrides. A known table-address DWORD is supplied by
the caller; no host pointer is discovered.

Mode1 all-zero stream:60 iterations,687 pre-wrapper draws,729 total and
two468-byte images. Constant2:619 draws; constant4:949 draws. Control0
constant2 adds124 discarded draws; constant4 adds576. Seed0:61 iterations,
684 pre-wrapper draws and726 total. `NativeGeneratedDefaultHeaderByteTransform`
evaluates recorded low-byte operations directly, not emitted machine code.
Default-control mode0/mode1 semantic outputs agree across32 seeds and retain
the independent513-byte seed0 digest; both captured-mode control0/1 variants
have positive-length round-trip and full transcript-replay checks.

The original `NativeGeneratedInitialModeZeroImage` and byte-transform APIs
remain compatible entry points over this shared implementation, including the
Request57 opt-in consumer. Arbitrary headers, other modes, empty-buffer safety,
full CPU/ABI effects, native allocations and arena region teardown remain
unreconstructed. No native mode1/control0 entry has been established.

Previously undefined helpers were recovered from verified original bytes
with explicit ends128596C/1285E27 and verified ECX-this/one-stack-argument
ABI. Original failure was call analysis; corrected signatures expose the
mode0 bodies. Unsupported mode diagnostics carry3E377377. Exact logical
buffers and generation RNG consumption are saved for the default-header
mode0 and explicitly captured mode1 cases above; other headers remain incomplete.
Both observed RNG
calls at12866EE and1286700 use virtual slot0; a decompiler's apparent
64-bit temporary is not proof of a separate UInt64 RNG method or two-word
consumption. The C# prefix does not make that inference, emulate native
pointer ownership/allocation, or claim that construction has completed.

### Native MT19937 seed, twist and UInt32 output

`NativeAlgorithms\NativeMt19937.cs` saves the generator itself. Native
allocation is0x9C8: vtable at0, index DWORD4,624 state DWORDs at8.
Vtable VA`0x1C7EBAC` slot0 points to UInt32 output raw`0x127FEA0`;
slot4 is a deleting destructor, **not the output method**.
Seeding`0x127FFD0` completes with index624. Twist`0x127FF20` uses the
standard397-word lag, upper/lower masks`80000000`/`7FFFFFFF`,
and table`{0,9908B0DF}` at VA`0x1C7EBA4`. Its227-word /396-word /
last-word split performs the standard **in-place** update.

Output twists when index equals624, reads/increments the index, then
tempers with right11, left7/mask`9D2C5680`, left15/mask`EFC60000`,
right18. Native expresses the two left steps as masks`FF3A58AD` and
`FFFFDF8C` **before** shifts; their shifted uint32 masks are the same.
C# uses the conventional equivalent form and an in-place modulo-index
twist, verified against a separate native split-loop projection across
multiple624-word boundaries. Seed5489 begins
`3499211612,581869302,3890346734,3586334585,545404204`.
Seeds0,1,5489,FFFFFFFF and reseeding retain unsigned wrapping behavior.

The model requires an explicit seed and performs no native activation.
It is mutable, unsynchronized and **not cryptographically secure**.
The inclusive unsigned wrapper is now saved below. Floating-point
wrappers and other surrounding consumers remain outside this implementation.

### MT19937 array initialization and inclusive unsigned range

Array seeder`0x1280010` is standard `init_by_array`: begin with scalar
seed19650218 (the operand shown as address`loc_12BD6A7+3` is a **numeric
constant**, not dereferenced memory), mix `max(624,keyCount)` iterations
using1664525 and cyclic seed words/index, then623 iterations using
1566083941 and subtracting the state index. At every state-index wrap it
copies state623 to state0 and restarts at1; it finishes with state0
`0x80000000`. `NativeMt19937` saves nonempty-word constructor/reseed
overloads with uint32 wrapping and no retained caller-array dependency.
An empty array is explicitly rejected: native code would read word0
despite count0, not choose a safe default.

Native constructors`0x127FE20` (scalar) and`0x127FE70` (array) twist
immediately after seeding. C# defers that identical twist to first output;
the exposed sequence is preserved without claiming allocation/init timing.
Canonical seed words `[0x123,0x234,0x345,0x456]` begin
`1067595299,955945823,477289528,4107218783,4228976476`.
Independent standard-library goldens cover key counts1/623/624/625/1000
and outputs around624/1248-word boundaries.

Generic wrapper`0x127FD70` calls virtual slot0 **before** checking bounds.
It rejects unsigned lower>upper through an exception object carrying raw
code`0x1C271921`; valid input uses
`lower + sample % uint32(upper-lower+1)`, with inclusive endpoints.
There is no rejection sampling or modulo-bias correction, and even a
single-value interval consumes a sample. The full interval
`[0,0xFFFFFFFF]` wraps the divisor to0 and reaches native unsigned division
by zero; it does **not** just return the original sample.

`NativeRandomInclusive32` reports accepted/invalid-bounds/wrapped-divisor
outcomes with the consumed sample. `NativeMt19937.NextUInt32Inclusive`
advances the generator before these outcomes, matching native draw order.
No native exception object, hardware fault, callback or activation is
executed. `Draw` accepts an injected raw generator, since the wrapper is
not specific to MT19937; `ProjectConsumedSample` selects the pure arithmetic
stage only. Float wrappers remain untraced.

### Owning byte-buffer comparison and heap algorithms

The routines immediately before the generic random wrapper are **not**
random/float transforms over three scalar values. Their 12-byte records
are owning byte-buffer headers: begin/end/capacity pointers. Copy constructor
`0x3C60C0` zeroes these pointers, allocates the byte length and copies content;
assignment`0x3C7AF0` handles self-assignment, empty sources and capacity reuse.
`NativeByteVectorHeap.cs` saves a content-only projection, not native pointers,
allocation timing/capacity, destructor execution or allocation failures.
Writes copy bytes; saved heap values are independent snapshots.

Comparator`0x127C8F0` uses `min(leftByteLength,rightByteLength)`, skips equal
four-byte groups, and compares the first differing **unsigned byte**.
At the end of the common prefix it returns false, with **no length
tie-break**. Empty buffers compare less than nothing, and `[1]` / `[1,2]`
compare false in both directions. This is not ordinary lexicographic order.
For arbitrary unequal lengths its induced equivalence is nontransitive:
`[1]` is equivalent to `[1,2]` and `[1,3]`, but `[1,2] < [1,3]`.
Do not use it as a general ordered-set equality/comparer, or silently repair it.
Equal-length records have ordinary unsigned lexicographic order; an external
equal-length invariant is **not established**.

The saved algorithms preserve these comparator decisions:

- `0x127F550`: make-heap, processing parents from `count/2-1` down to zero.
- `0x127FA80`: adjust a hole downward, comparing right child against left
  and choosing **right on equivalence**; handle a lone left child, then
  push the saved value back up toward the original hole.
- `0x127FBD0`: push a saved value upward only while parent is less, bounded
  by the supplied top index; equivalent values do not rise.
- `0x127FB10` / `0x127FC50`: duplicate pop-heap bodies, saving last content,
  assigning root content to last, and adjusting the shortened prefix.
- `0x127F610`: repeatedly pop until the remaining prefix has one element;
  it does **not** first make an arbitrary range into a heap.

`MakeHeap`, `SortHeap`, `PopHeap`, `AdjustHole` and `PushSavedValue` operate
on spans of byte memories. Empty make/sort are no-ops; singleton pop retains
its contents. Empty pop and invalid hole/top accesses are rejected explicitly
instead of performing native invalid reads. Tests cover prefix/empty/high-byte
behavior, four-byte boundaries, right-child ties, bounded upward motion,
copied ownership, duplicate multiplicities and fixed-width heap/sort results.
No global ordering guarantee is claimed for arbitrary prefix-equivalent lengths.

### Byte-buffer introsort, pivot partition and insertion

`NativeByteVectorSort.cs` saves the full bounded sorter`0x127E8A0`,
partition`0x127EB60`, pivot helper`0x127F3D0`, order-three`0x127F8F0`
and insertion`0x127F6F0`. Wrapper`0x127E4F0` initializes its ideal budget
to the **element count**, not a logarithmic depth;`0x127E6D0` does the same
while forwarding an opaque token. Selected helpers never use that token to
change the comparison. `Sort` models the count-initialized wrapper;
`SortUnchecked` exposes the explicit signed budget, including zero/negative.

Count at most32 is handled first: zero/one are no-ops and2..32 use insertion
even with an exhausted budget. Larger ranges with positive ideal partition,
then reduce ideal to `ideal/2 + (ideal/2)/2`. They recursively process the
smaller outside range and iterate on the larger; on a tie, recurse right and
iterate left. Nonpositive ideal on a larger range triggers make-heap plus
sort-heap. Managed trace counters expose these decisions for inspection;
they are **not native state or serialized fields**.

The pivot is the middle index `count/2`. Counts up to41 order the
first/middle/last buffers by three pairwise less checks. Starting at42,
the helper uses a ninther: `step=count/8`, order triples around the first,
middle and last positions, then order their middle members. The native
threshold compares `(last-first)/12` to40, hence the inclusive41-record
median-of-three boundary.

The partition expands adjacent comparator-equivalent buffers around the
pivot, scans forward/backward, absorbs further equivalent buffers and rotates
the pivot block when one scan exhausts. It returns a half-open index pair for
that block. **The pivot representative is not captured once:** comparison
reads the current first pivot buffer. Absorbing a shorter matching prefix,
especially an empty buffer, can therefore change which other buffers are
classified equivalent. Tests explicitly preserve this behavior.

Insertion saves each next buffer, checks it against the **first** buffer,
and if less shifts the entire prefix backward. Only the other branch shifts
individual greater predecessors. With `[02,empty,01]`, native yields
`[01,02,empty]`; a conventional guarded insertion implementation could leave
the input unchanged. The saved model does not replace this special branch.
Buffer swaps/assignments retain content-copy ownership, not pointer moves.

Partition's original`0x650`-byte neighborhood was verified unchanged against
the original executable before adding a bounded IDA function definition.
Instruction analysis establishes hidden result pointer in ECX, first pointer
in EDX, last/token on the stack, caller cleanup, and two pointer stores for
the returned boundaries. Correcting helper calling conventions removes the
spurious stack warnings/arguments in the old decompilation. One instruction
remains rendered as data/JUMPOUT by IDA; its exact original bytes are decoded
with Capstone, not inferred from that jump. No executable bytes were patched.

Fixed-width randomized/sorted/reversed/all-equal/duplicate-heavy checks cover
32/42 thresholds, forced heap fallbacks, all partition rotation directions
and records through2048 entries. No general sorting guarantee is claimed
for arbitrary unequal-length prefixes. A concrete caller`0x127D0C0` sorts
enumerated adapter address buffers before concatenated MD5 hashing; its
verified import binding, registry filter and offline projection are documented
in [Startup's adapter-address section](Startup.md#sorted-adapter-address-md5-offline-projection).
