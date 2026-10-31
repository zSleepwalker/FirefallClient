# Reverse-engineering handoff

The supported generated transformation-provider boundary was completed, then
explicitly resumed by the user for another bounded reconstruction pass. The
latest work extends the tfWorld zone-cleanup layout from static binary evidence;
read-only IDA follow-up also verified four constructor callsites for the
generated provider. Both caller paths use the constructor that writes mode
DWORD+4=0 and control byte+0x18=1 before generation; no override store was
found in those observed paths. This is not an exhaustive global writer search,
so native non-default mode/control reachability remains unproven.
Overall client reconstruction remains incomplete. No recurring session
automation is configured.

The latest bounded world-cleanup pass confirms the raw callsite bytes for the
resource getter dispatch (`0x860F44`, vtable byte offset `+0x24`) and returned
object factory (`0x860F6F`, member `+0x10`, vtable byte offset `+0x40`), plus
the result store at `0x860F76`. Both targets remain indirect: no concrete
vtable or static candidate was resolved, so slot0's deleting-destructor
interpretation remains unproven. The slot0 cleanup call at `0x8610C8` passes
argument1; this alone is not destructor evidence. Fingerprint checks now pin
these observed instruction sequences without asserting their targets.

The latest arena lifetime follow-up confirms that the traced block-return path
recycles spans through list metadata without coalescing or releasing the
generated-output regions. `VirtualFree` is present in the generic allocator
release path for other allocation classes; this does not prove output-region
teardown. IDA xrefs undercount byte references, so the negative result remains
limited to the inspected paths rather than a binary-wide claim.
Two additional raw sites, `0x12ABD6C` and `0x12ABDBC`, contain identical
`VirtualFree(ECX,0,MEM_RELEASE)` call sequences after a non-null test. IDA has
no function boundary or xrefs for them, and the caller/ECX provenance is
unknown; they are not evidence of generated-output region release.

Latest tfLogin pass: update `0x6385C0` reads a single pointer cell (`this+0x20` = ctor arg1 = `App+0x9E8`), not a node list; selector case 1 sends message `0x146` with `node+0xC/+0x10` and virtual-deletes the node. Producers of the cell and cases 0/2/3/4 are unproven; see `docs\MatrixConnection.md`.

## What is saved

- **Generated provider initialization prefix saved:** `NativeGeneratedTransformProviderPrefix`
  reconstructs count60..100 modulo draw BEFORE264 raw low-byte draws,
  headerWORD0200/byte3, initialDWORD+4=0/byte+18=1 and table at+C.
  Exactly265 UInt32 calls for this prefix ONLY. FromSeed uses saved MT;
  independent seed0 table SHA256 regression included.
  Read-only agent7ed40f8c-f5e1-42fe-9c6a-e812a685ac4e completed bounded
  construction trace. Outputs+10/+14 are arena-allocated assembled spans;
  original region allocation uses executable/read/write protection, but the
  reconstruction never allocates or executes them. Captured mode0 byte images
  are saved; no named cipher identity is established.
  Builder-local1285290/1285970 now defined through explicit verified ends
  128596C/1285E27; ECX-this/one-stack-argument prototypes recover decompile.
  `NativeGeneratedProviderBoundaryPlanner` saves modeDWORD+4==0:
  opening10/closing8 operation selections, raw operand descriptor codes,
  header-derived choices, offsets8/12 and final16-bit value8.
  `NativeGeneratedDescriptorFactories` now saves numeric code classification,
  code-to-operand sparse writes and no/unary/binary operation constructors.
  Preserve captured unassigned bytes; absent operands are not zero-filled.
  Immediate1299D00 uses floored bitWidth>>3 and LE byte counts1/2/4/8,
  including noncanonical bit widths; saved in NativeGeneratedImmediateBytes.
  Initial mode0 boundary candidates now saved; MOV EBP,ESP has89E5/8BEC
  alternatives in static table order. All other boundary steps have one.
  Immediate operand1284EF0 and post-candidate1286780 now saved:
  latter discards unsigned byte-sum%5 draws only when byte+18==0,
  rather than inserting junk bytes. Initial byte+18=1 disables it.
  IDA immediate/module-path switches are restored with SWI_USER protection;
  automatic reanalysis had replaced both mappings with erroneous PE VAs.
  Fixed data-section mapping for these objects is VA=raw+401C00, verified
  through original PE section headers; never apply a blanket406C00 delta.
  Named earlier temporary check logs have been cleaned up.
  NativeGeneratedBodyDecisionRules saves exact seven opcode pairs, mode0
  scratch/count-register parity and bounded table scale/index/window choices.
  Pairs are ADD/SUB, SUB/ADD, INC/DEC, DEC/INC, ROL/ROR, ROR/ROL, XOR/XOR.
  Reverse helper1287C20 reverses12-byte byte-vector records, not bytes.
  Generator builds forward/inverse fragment lists then reverses inverse.
  NativeGeneratedConditionalBranches preserves original JZ/JNZ range gates:
  short accepts0..255 except128/129 (no negatives), so130..255 wrap rel8;
  near rejects Int32 min..min+5 and subtracts6 only for negative offsets.
  NativeGeneratedByteLoopWrapper now saves exact captured-body initial-mode0
  byte concatenation and24 RNG draws per wrapper. Body fragments precede
  both wrapper builders; correct earlier assumption of interleaved body draws.
  NativeGeneratedScratchTemplates now saves exact initial scratch INC/DEC
  and byte ROL/ROR candidates, including count1's extra implicit-one form.
  NativeGeneratedImmediateMoveTemplates saves preparatory8/32-bit register
  MOV alternatives in compact/ModRM table order, including captured pointers.
  NativeGeneratedArithmeticTemplates saves initial BL/EBX ADD/SUB/XOR:
  one80/81 immediate candidate (no83), two ordered register-source
  candidates, one EDI or scaled-ESI table-memory candidate.
  Memory encoder129A0C0 and scale checker129A5B0 had faulty IDA switch
  mappings; restored direct/byte-indexed tables and validated calculated
  targets against original bytes. SWI_USER re-applied after autoanalysis;
  cached decompilation/block recognition can remain incomplete.
  NativeGeneratedInitialModeZeroImage now assembles both logical byte images
  with supplied captured table address and complete replayable UInt32 ledger.
  All-zero sample stream:60 iterations/735 draws/two405-byte images.
  Constant2:62 unary iterations/625 draws. Constant4:64 CL-path rotations/955.
  Scripted register and indexed table streams check local/global inverse
  fragment reversal and independent forward/inverse candidate draws.
  NativeGeneratedArenaPolicy saves wrapped length+16,64/128/256 preferred
  lists then large-list fallback, first-tie best-fit, remainder>=32 splitting
  and64KiB region rounding. Original PE IAT proves VirtualAlloc with
  MEM_RESERVE|MEM_COMMIT/PAGE_EXECUTE_READWRITE; model never allocates.
  NativeGeneratedProviderLifetimePlan saves RNG/table then output-release
  order and verified tail-jump/two-stack-word/RET8 callback image facts.
  Native release returns outputs to arena, not VirtualFree on that path.
  NativeGeneratedInitialModeZeroByteTransform saves managed positive-length
  low-byte effects from recorded operation steps, not machine-code execution.
  Logical forward/inverse round-trips cover all byte values/count boundaries
  for32 MT seeds. Empty input stays explicitly unsupported because of native
  short-JZ quirks. Request57 opt-in reconstructed-provider verifier applies
  managed inverse before MD5 while preserving existing selection bypasses.
  Arena singleton xrefs and adjacent1251220..1251900 helpers were checked;
  no true region teardown was established (do not invent VirtualFree).
  Mode1 boundary schedule/default-header bytes are now saved separately:
  six PUSHes; saved RCX/RDX loads from RSP+32/+24; six reverse POPs andRET.
  Original memory encoder emits unusual no-index SIB E4, not24; preserve it.
  Native constructor still writes mode0; no mode1 entry path established.
  Independent seed0 semantic golden now saved:61 iterations,684 pre-wrapper
  draws,732 total and exact513-byte output SHA256/first16 bytes.
  Explicit captured-control0 mode0 generation now saves exact native
  post-candidate discard sites, full replay ledger and discard totals.
  Do not apply discards globally: direct immediate arithmetic, table-base MOV,
  scratch loads and wrappers omit the helper. Constructor still writes1;
  no native control0 entry/setter was established. Default behavior unchanged.
  Incoming/outgoing direct provider consumers discard callback return values;
  thunk-entry callers and a meaningful generated return remain unproven.
  Mode1 memory-shape validation and separate no-SIB fast-path predicate saved:
  32/64-bit address registers, equal widths, no ESP/RSP index, scale1/2/4/8,
  index required for scaled forms; returned flag means address-size override,
  not successful validation. Explicit diagnostic results cover8712 shapes.
  Repaired mode1 direct/compressed switches at129A4C1/129A734 from original
  tables129A584/129A7D8+129A7E4; all six known switches protected and saved.
  Validator boundary now excludes tables and the separate129A7F0 predicate;
  cached decompiler block recognition may still remain incomplete.
  Mode1 captured ModRM/SIB/displacement field projection now saved with
  address-prefix selection and REX X/B bits. Default boundary MOVs reuse it.
  Preserve no-index R12/R13 native fast-path quirks (raw rm4/rm5, no canonical
  extra bytes), E4 SIB, signed displacement limits and absent-base DWORDs.
  Checks cover18657 accepted boundary combinations, not machine execution.
  Mode1 ordinary immediate MOV templates now saved: byte candidates unchanged;
  64-bit destination/source32 uses C7 only; source64 adds compact imm64 first
  and signed32-fit C7 second. Original CDQ128624F proves signed-DWORD table
  pointer expansion. Extended destinations remain outside this catalog.
  Mode1 scratch/ordinary-register unary templates saved: BL/RBX parity;
  one FE or REX.W FF INC/DEC candidate, no compact40..4F mode1 candidate.
  Mode1 ADD/SUB/XOR and operand-decision components saved: BL/RBX, CL/RCX
  or DL/RDX; one REX.W81 imm32, two ordered register forms, one RDI/RSI table
  form.264-byte table bounds checked for byte/64-bit reads and pointer wrap.
  Captured mode1 wrapper saved:21 selections versus mode0's24, RDX OR-test,
  RAX increment/RDX decrement, retained branch displacement quirks.
  Shared NativeGeneratedDefaultHeaderImage now composes complete logical
  forward/inverse default-header mode0/1 images with captured control byte.
  The old mode0 APIs remain compatible; constructor0/1 facts are distinct
  from explicitly supplied mode/control overrides. Native mode1/control0
  entry paths still unproven. No host pointer capture or native allocation.
  Mode1 zero stream:729 draws/two468-byte images; constant2/4:619/949 draws;
  control0 adds124/576 discarded draws respectively; seed0 total726.
  NativeGeneratedDefaultHeaderByteTransform evaluates recorded byte steps,
  not machine-code arrays.32-seed semantic/round-trip/full replay checks and
  independent seed0 digest retained. Arbitrary headers, empty-input safety,
  native region teardown and full CPU/ABI effects remain unresolved.
  **Completed boundary for today:** supported generated-provider projection,
  managed effects, numerical arena/lifetime and IDA metadata. Do not start
  another subsystem today. Suggested next work only after the user resumes:
  native override entry paths/arena teardown, or another requested subsystem.
  Final verification: complete Startup checks passed against both original
  and installed binaries; full solution build passed with zero warnings/errors.
  IDB saved after verifying all six protected switch mappings. No executable
  patches, native/generated-code execution, network requests or temporary
  source artifacts were introduced. Build outputs are not reconstructed source.
  No native execution; no callback delegates created.
  Calls12866EE/1286700 both virtual slot0; do not infer a UInt64 RNG method
  merely from decompiler temporary widths. No generated code executed.

- **Captured module-directory provider saved:** `NativeModulePathProvider`
  reconstructs126CBD0/126D9E0. GetModuleFileNameW(NULL) starts260 units,
  retries GetLastError122 with260+4*previous regardless returned count,
  no last-error clearing; terminal non122 needs nonzero return.
  Captured secure splitter result0, drive3/other parts256 capacities.
  Selector enum0 full path,1 filename,2 filename+extension,3 drive+directory;
  unsupported selector checked AFTER query/split. Selector3 feeds alias254.
  No module path or CRT query executed. Original PE jump tableVA166DAE0
  ->raw126CEE0; corrected IDA switch metadata and cases restore decompilation.
  Original/installed suites and solution build passed; native module/splitter
  names and corrected switch/ABI persisted in IDB. Generated provider
  prefix is now saved above; its remaining builder primitives are next.

- **Captured folder-prefix alias projection saved:** `NativeFolderPrefixAlias`
  reconstructs126DAC0: remove one trailing slash/backslash, ordinal `\\?\`
  prefix removal (static initializer2E33D0), fixed provider-ID order
  254,255,37,41,36,38,42,5,28,26,35,0,25,40,7. Per-unit captured towupper
  prefix match, whole-string/backslash boundary, first match not longest,
  replace root+one backslash with `<decimalID>` without separator.
  No match falls back to original input, not modified candidate.
  Root availability/case captures can remain unknown; unknown earlier
  provider is not skipped. No GetTempPathW/SHGetFolderPathW/locale calls.
  Original/installed suites and solution build passed; symbols/IDB saved.
  Provider254 is now saved above as module-directory selector3.
  Metadata ownership and generated activation provider remain boundaries.

- **Metadata text tail saved:** `NativeMetadataTextTail` reconstructs1283720
  shortening (>64 UTF16 units→first31 + literal dots +last30) and
  strict UTF16→UTF8 range/scalar encoders1258C70/1258E90. Stored NUL
  preserved; no BOM/terminator/replacement characters. Shortening may split
  valid surrogate pairs; explicit invalid-unit outcomes retain those cases.
  Every valid Unicode scalar cross-checked against standard UTF8.
  Input boundary is captured normalized/locale-towlower text, not raw paths.
  Earlier126DAC0 prefix aliasing and126D7D0 prefix removal are now saved
  above; provider identities remain partial. Original/installed checks and
  whole solution build passed; meaningfully named functions and IDB saved.

- **Mode-zero descriptor and zlib boundaries saved:** `MatrixModeZeroDescriptorCodec`
  writes WORD2, Windows-version DWORD, two raw16-byte digests and two
  opaque DWORD/ushort-length narrow-byte groups (+24/+28 and+40/+44).
  Provider metadata is captured after1273530 conversion; its UTF8 tail is
  now recovered separately above.
  `NativeWindowsVersionCode` projects captured GetVersionExW284 and
  exact native packed-version codes; no host OS query.
  124E720 compresses via11E8910 (native zlib1.2.8/default6/window15/memory8),
  appends original LE32 size, then124BC10 MD5 over compressed bytes+size.
  `NativeZlibBufferFraming` saves framing and captured inflater projection.
  **Correction:** incoming124AC10 calls120C250/1256A60 before reading key;
  the MD5-verified prefix is compressed, not the routing key.
  Incoming allocation is footer DWORD, prefilledFF; stream-end1 and unused
  input0 required, but shorter output accepted with FF suffix retained.
  Pre-transform serializer/framing only; generated provider+10 callback
  and queue/send effects remain unresolved. Original/installed checks and
  whole solution build passed; descriptor/zlib/version names and IDB saved.
  Next: generated provider1285060/1286F00 and metadata source ownership.

- **Windows-drive storage serial MD5 saved:** `NativeWindowsDriveSerialFingerprint`
  projects127C730/127C550. GetWindowsDirectoryW260, reject only zero,
  force UTF16[2]=NUL; device prefix `\\.\` +first2 units.
  CreateFileW read80000000/share3/open3; reject handleFFFFFFFF only.
  IOCTL2D1400,12 zero input bytes; first output8 and returned8, allocate
  header+4 size, second returned exactly size. Serial offsetDWORD+24;
  CString copied and standard MD5 raw bytes, no trim/case/hex conversion.
  Offset0 reads header bytes, not automatic empty/missing; valid empty
  serial succeeds with MD5(empty). Unsupported offset/NUL reads and
  incomplete captures report unknown native outcomes, not false/zero.
  Captured projection only; no directory/drive/IOCTL execution.
  Mode-zero result exposes WindowsDriveSerialSlot alias and
  ProjectCapturedFingerprints drives+adapters overload under a distinct
  method name. Original/installed byte evidence and complete solution build
  passed; native helper names/comments persisted in IDB. Downstream
  descriptor serialization/framing is now saved above.

- **Offline adapter-address MD5 saved:** `NativeAdapterAddressFingerprint.cs`
  projects127D0C0 with captured API statuses and registry-filter decisions.
  Original PE IAT proves1AC412C = GetAdaptersInfo; initial probe MUST return111,
  second call zero. Select types6/71 with address length>=6, then filter;
  copy exact declared lengths, sort via native prefix/introsort model and
  MD5-concatenate without counts/separators/deduplication. Success with no
  selected addresses yields MD5(empty); API failure leaves output unwritten.
  Filter127CCD0 uses HKLM Network/{4d36e972-...}/adapter/Connection and
  PnpInstanceID; missing/unopened key ALLOWS, available key's failed query,
  non-REG_SZ or failed data read REJECTS. Prefix before first backslash is
  case-insensitive pci/usb/bth. ASCII/C-locale captured projection only;
  unresolved filter/non-ASCII comparisons never invent a digest.
  No host adapters/registry were queried. Upstream digest consumers and
  original identifier semantics remain bounded: mode-zero selector124B7C0
  copies this digest at124B93A only on true return, else retains initialized
  zeros and proceeds. `MatrixSynchronizationModeZeroDigestSlots` saves
  opaque first slot+4 and adapter slot+14, independent copy decisions,
  explicit unresolved attempts and exact16-byte capture requirements.
  First helper127C730 is now saved; downstream descriptor clone/transform
  at124E010 and generator-wrapper1285060 remain next targets.

- **Byte-buffer introsort saved:** `NativeByteVectorSort.cs` reconstructs
  sort127E8A0, partition127EB60, median127F3D0, order-three127F8F0,
  insertion127F6F0 and count-initialized wrappers127E4F0/127E6D0.
  Threshold32 before budget check; positive ideal becomes half+quarter;
  recurse smaller side/tail-iterate larger; exhausted budget -> heap sort.
  Ninther starts at42 records (native compares last-index difference to40).
  Partition dynamically reads its first pivot representative; absorbing
  empty prefixes can collapse comparisons between otherwise differing bytes.
  Insertion's first-element branch shifts the whole prefix, preserving
  [02,empty,01] ->[01,02,empty]. Original partition-range bytes verified
  before IDA-only function/type edits; caller-cleanup ABI recovers result pair.
  One movzx instruction remains data/JUMPOUT in IDA; original Capstone
  decode is authoritative there. Fixed-width checks cover2048 records,
  duplicates, all rotations, threshold and forced-budget paths.
  Concrete next caller:127D0C0 enumerates adapter-like records, filters,
  sorts copied addresses and MD5-hashes their concatenation. Verify import
  off1AC412C and filter127CCD0 before assigning device-ID semantics.

- **Owning byte-buffer heap algorithms saved:** `NativeByteVectorHeap.cs`
  corrects the adjacent-routine scope: 12-byte begin/end/capacity headers,
  not scalar triplets. Comparator127C8F0 compares unsigned bytes only through
  the common prefix and returns false without a length tie-break, including
  empty buffers. Prefix equivalence is nontransitive across arbitrary lengths;
  do not substitute ordinary lexicographic ordering or assert an unproved
  equal-width caller invariant. Make127F550, sort127F610, adjust127FA80,
  push127FBD0 and duplicate pops127FB10/127FC50 are projected over copied
  buffer contents. Right child wins equivalent-child comparisons; push stops
  at top; sort requires an existing heap. Pointer/capacity/allocator behavior
  is not emulated. Whole outer sort127E8A0 is now saved separately above.
  No general payload meaning is established by the heap alone.

- **MT array seeds / inclusive range saved:** `NativeMt19937` now supports
  nonempty seed-word spans, standard init_by_array1280010 (19650218,
  multipliers1664525/1566083941, finalstate0=80000000). Canonical four-word
  vector and independent Python stdlib goldens cover lengths1/623/624/625/1000
  across2000 outputs; empty input is rejected, not defaulted.
  `NativeRandomInclusive32` saves127FD70: virtual draw BEFORE unsigned
  lower/upper validation, diagnostic1C271921 for reversed bounds,
  inclusive modulo width with bias, and full[0,FFFFFFFF] width0 fault
  reported explicitly. Singleton/failed draws advance MT state.
  Native constructors twist immediately; managed model lazily twists
  before first output with same sequence. No native faults/activation.
  Float wrappers and profiler virtual60 provider remain useful next targets.

- **Outgoing profiler name requests saved:** `NativeProfilerNameRequests.cs`
  reconstructs concrete RTTI `tfServerProfiler` vtable1C61B50 slot8/6AAB20.
  Mode2 first; selector1 matrix30/channel1 (>3, logmask01), selector0
  firefall18/channel2 (>4, logmask02, raw routing pair878/87C, tick*1000
  atobserverB0). Both nonzero observer820. Wire normal buffers preserve
  duplicated/ordered root/node WORD lists,255 chunks/zero terminators.
  Matrix30 header alone; firefall18 has rawLE64 prefix then header18.
  Frame61's endpoint virtual8 ABI matches, but global virtual60 concrete
  provider is **not proved**. Global1DFC82C statically points to cell1E00470;
  runtime installer/provider unresolved. Do not claim end-to-end binding.

- **Profiler derived-node subtree snapshot codec saved:**
  `NativeProfilerNodeSnapshot.cs` projects E8CE00/E8CE70 + E8DD00/E8DDA0.
  Signed32 count -> child bodies -> raw expansion byte ->80 physical
  samples packed26 bytes (omit native padding2) -> raw next-slotDWORD ->
  captured CString/NUL. Empty-name leaf2090 bytes. Negative count reads
  no children; encode re-counts actual children. Kind/ID are not saved;
  derived factoryE8CEA0 creates kind0/IDFFFF, so DTOs stay separate from
  live tree. Iterative600-depth encode/decode, every truncated leaf prefix,
  raw flags/FFFF index, independent bytes and explicit malformed/cyclic
  input errors saved. No network61/whole-file framing or object activation.

- **Profiler display-name getter saved:** `NativeProfilerDisplayNames.cs`
  projects virtual+10/E8CD70. Nonempty cached shared text+8E4 overrides
  kind/table lookup; getter itself does not populate/clear the cache.
  Kind1 root names/E648E0 and kind2 node names/E64920 use full WORD IDs.
  Missing ->`<Waiting For Server>`; present-empty ->empty; kind3
  ->`[Exclusive]`; other kinds ->`<Error>`. Snapshot writerE8CE00 copies
  resolved C-string into cache before base/name output; readerE8CE70 loads
  cache. No invented invalidation. Getter144 analysis bytes defined from
  original instructions; executable unchanged. Full profiler node snapshot
  codec remains a bounded follow-up.

- **Profiler graph histories coupled:** `NativeProfilerFrameRefresh.Apply`
  saves E8DB00 root/child refresh and E8D760 regular/exclusive refresh.
  Important native mismatch: roots are found forward by WORD ID, but their
  direct children are paired **positionally**, not by ID. Old[20,40], new[40]
  writes40's sample to20's history. Deeper children use forward-ID matching;
  missing children are skipped without decoding/aggregation. Only processed
  source children contribute; stale history stays untouched. Empty children
  append zero roots; absent roots do not advance. Zero-filled80-slot node
  histories, iterative bottom-up updates, explicit mismatch/skip counters
  and partial-write unsupported-half outcomes saved. No native GUI execution.
  Next useful boundary: outgoing missing-profiler-name request codec via
  endpoint virtual8; alternatively cached profiler-name resolution.

- **Profiler selection recovery saved:** `NativeProfilerTree.MergeWithSelection`
  retains exact visited objects (including exclusive children), otherwise
  searches preorder by kind/WORD-ID, recovers first stale duplicates, and
  expands ancestors only. `FindVisibleRow` preserves expanded-child preorder
  excluding the hidden container, whose expansion is an explicit argument.
  E72480 is generic visible-row lookup, **not handle creation**. Frame owner+0C
  is selected-node pointer; its null-row0 fallback runs even when refresh
  is suppressed. E8D3F0 identity lookup ignores collapsed flags. New nodes
  begin collapsed. No GUI execution; numeric history and names remain separate.

- **Profiler persistent tree saved:** `NativeAlgorithms\NativeProfilerTree.cs`
  reconstructs rootE8D440/childE8D010 structural merging. Native kinds
  root1/node2/exclusive3; WORD ID8DC. Forward-only sibling cursors retain
  stale nodes and can create duplicate IDs on reorder (old[1,2], new[2,1]
  ->[1old,2,1new]). No global dictionary normalization or erasure.
  Every regular node gets same-ID exclusive first child; roots do not.
  Stable managed references, parent links and iterative600-level handling
  saved. `MergeStructure` remains structural-only; optional selection is now
  saved separately. History updates/name resolution and native allocation
  behavior remain separate.

- **Profiler61 numeric samples/history:** saved
  `NativeAlgorithms\NativeProfilerSamples.cs`. Decoder1303020 copies
  three raw WORDs and expands five binary16 values through table1C165B8,
  float32×65536 then signed64 truncation/low32. All65536 table entries
  checked (finite exact; nonfinite classified). Finite negative wrapping
  retained; exceptional nonfinite CRT behavior explicitly unsupported.
  Saved exact28-byte logical sample fields, per-child saturating exclusive
  DWORD4 subtraction/divisor, wrapping inclusive/root aggregates, and
  filled80-slot history (template/index0, modulo80). Numeric functions
  E8D760/E8DB00; initializerE8E090. UI tree matching/materialization and
  metric units remain unknown; no rendering/native execution.

- **Profiler frame61 name recovery:** saved
  `MatrixServerProfilerFrame61NativeLayout.cs`. Roots use owner+190 names;
  nodes use+1B0 names. Missing IDs preserve root order / node preorder and
  duplicates; present-empty names are known. Iterative collector covers
  deep trees. Missing-name request selection uses unsigned elapsed>5000,
  updates marker18C before endpoint lookup, and advances it even on null
  endpoint. Unknown virtual8 request carries owner1D0 and two WORD vectors.
  Independent suppression byte1E49A5A gates opaque E8D440/E8DB00 refresh,
  not collection/requests. Frame entryE65290 had false data classification
  in E65300..E653DE; corrected222 analysis bytes, no executable patches.
  Numeric samples, structure and selection are saved in the later units above.
  Open: outgoing missing-name request encoding and native rendering details.

- **Profiler names60 persistent state:** saved
  `MatrixServerProfilerNames60NativeLayout.cs`. ConsumerE64A00 updates
  independent WORD-key/shared-text maps at global1E6A470+190/+1B0.
  Native lookupE671C0 hashes both key bytes and compares WORD, not BYTE.
  Existing keys retained, duplicates last-value-wins, empty replaces text;
  empty vectors do not clear maps. Enabled handler stores tick-5000
  wrapping at+18C; absent consumer changes nothing. Raw-text copies
  and gates saved, native refcounts/order omitted. Name categories and
  frame61 consumerE65290 remain unresolved.

- **Native MT19937 saved:** `NativeAlgorithms\NativeMt19937.cs` reconstructs
  seed127FFD0, twist127FF20 and UInt32 output127FEA0. Vtable1C7EBAC
  slot0 is output; slot4/127FE40 is deletion. Standard624-word /397-lag
  twist and9908B0DF table proven; native pre-shift masks resolve to
  standard tempering. Canonical5489 outputs plus separate native split-loop
  projections cover2000 outputs for0,1,5489,FFFFFFFF and reseeding.
  No native activation; array seeds/inclusive bounds are now saved above.
  Float wrappers remain open.

- **Synchronization dispatch clarified:** `MatrixSynchronizationDispatchNativeLayout.cs`
  saves mode0's LE16 key followed by two LE32 values. Helper124E6E0
  reads both DWORDs through1206B20; **second DWORD==8 is a value gate,
  not a buffer length**. Key1 / value8 select native activation; first
  DWORD seeds the624-word MT19937 recurrence at127FFD0. Opaque activation
  is not executed. Short reads precede diagnostics; trailing bytes are
  ignored by this field path. Nonzero mode124C010 is hash-map lookup and
  virtual callback, **not proven queuing**; prior tentative wording corrected.
  Saved raw diagnostic codes and plain-FNV32 numeric-key/split-bucket
  projection. Shared hash prefix extraction preserves chat/SDB final mix.
  Next boundaries: callback registration/handlers and external transform;
  float generator wrappers remain untraced; inclusive unsigned range is saved.

- **All35 incoming original class names:** saved
  `MatrixServerMessageNativeTypes.cs`, exposed separately from numeric logs
  through catalog entry `OriginalType`. Factory33D4B0/jump-table33E250,
  distinct constructors, vtable/COL/type descriptor and exact names are
  recorded and binary checked using PE section-aware translation.
  Key clarifications:57 SynchronizationRequest;58 GamePaused;59 SuperPong
  (older SuperPing C# labels came from diagnostics);60 ServerProfiler_SendNames;
  61 ServerProfiler_SendFrame;67 ReceiveEmergencyChat. No field meanings
  inferred solely from names.

- **SynchronizationRequest57 consumer envelope:** saved offline
  `MatrixSynchronizationRequest57NativeLayout.cs`. Consumer124B3F0
  rejects nonzero owner gate70 and buffers shorter than20; mode1C can
  invoke an unknown mutable-buffer external hook before verification.
  Standard MD5 of preceding bytes must match the final16. Leading LE16
  becomes registered-handler/direct dispatch key; remaining semantics unresolved.
  Verify only already post-transform bytes; no hook or downstream execution.
  Golden digest, prefixes, mutations, MD5 padding boundaries, copy isolation
  and dispatch selections are covered. Whole-remainder57 wire boundary
  remains unchanged. Next useful targets: direct dispatcher124B7C0 and
  registered key helper124C010; profiler60/61 consumer identities.

- **Native sigscan traffic names and request32 wire:** RTTI identifies
  RequestSigscan32 vtable1C62384 and SigscanData34 vtable1C62354.
  Saved offline `MatrixRequestSigscan32Wire.cs`: exactly6 application bytes,
  code32 + LE32 cache key + raw flag; **no entity64 prefix**.
  Producer flag is first argument==0; second argument is the key.
  Sender31D350/encoder31E7B0 submit channel1; reader345D20 mirrors fields.
  No live requests/scanning performed; flags/key formal identities and
  returned-snapshot caller free remain unproven.

- **Message34 accumulation resolved:** application+C44 is embedded RTTI
  slBlob (vtable1C80CC4), not an arbitrary transformation pointer.
  Virtual18 appends raw bytes; virtual14 clears logical length with **no
  argument** (corrects earlier decompiler "consume length" claim).
  Saved accumulation/snapshot/clear and cached deep-copy projections.
  Snapshot includes previous chunks; zero flag preserves prior pending/cache.
  Helper30EDA0 returns an independent copied pair/data. Request30EED0
  compares cache keyC40 and pairC3C, frees old cached data/pair on miss,
  requests message32 then waits for34 completion. Full request32 encoding,
  callback synchronization and returned-copy caller free remain untraced.

- **Sibling chat initialization:** Login63B250 gates on sampled application
  A28 object's byte34, constructs chat at Login14, generates a 64-bit pair
  via BC9A20, forces type1 and copies explicit CString argument into chat10.
  Generator is native lock-xadd old DWORD counter1DFC588 shifted8, OR base
  pair1E487F8, clear low8; returns EDX:EAX despite misleading DWORD
  pseudocode. Saved pure sampled-counter/base projection and wrapping next
  counter. Do not call it an authenticated/account ID getter.
  LookingGlass65A880 constructs fresh zero/empty identity at+68; later
  replay producers untraced. Source/meaning of Login CString still opaque.

- **World tfChat identity source recovered:** successful World setup tail
  64B59C..64B5F4 loads World owner+10, copies owner+848/+84C into chat+8/+C
  with low byte forced1, and copies owner+850 CString into shared text+10.
  Saved raw-byte `MatrixChatIdentityNativeLayout.cs`; no invalid-ID filter
  invented. This resolves the World source of the chat-log checksum text.
  The owner pair also feeds authentication; formal account/character field
  identities and Login/LookingGlass producers are still not proven.

- **tfChat file lifecycle:** RTTI proves the former generic mode-specific
  object is tfChat (vtable1C67CDC/type descriptor1E12EF8). Saved bounded
  initialization/flush/shutdown/OpenChatLog selections in
  `MatrixChatFileNativeLayout.cs`. Path is cached VIBE base +`chat.log`;
  first-attempt flag0 uses `wt`, later uses `at`, and the flag becomes1
  even when opening fails. UTF8 path/mode conversion leads to `_wfopen`;
  content uses CRT text mode. OpenChatLog flushes present FILE then invokes
  ShellExecuteA, requiring chat but not a successful file open. Models
  perform no IO/viewer launch. Text+10 original name/source remains unresolved.

- **Chat file-log checksum:** post-runtime6F46F0 uses mode FILE*+18,
  native timestamp/label/text formatting, and folded modified-FNV32 suffix.
  Saved raw-byte prefix, hash input and pre-CRT line generation in
  `MatrixChatLogNativeLayout.cs`. Input includes prefix NUL, author ID with
  low8bits cleared, exact static25 bytes, and first32 bytes of mode text+10
  (positional meaning unresolved). Hash12F1780 uses FNV1a plus final mix,
  not CRC/plain FNV; shared existing SDB mix via linked
  `src\Shared\EngineByteHash32.cs`. SDB seeds/cipher behavior preserved.
  Native time locale, log-file opening/lifetime and CRT newline translation
  remain outside the model. PE imports also establish `_stricmp` for the
  chat channel parser; no ordinal comparison assumption is made.

- **Shared display36/67 resolved:** server67 is **zone chat**, not whisper:
  fixed category1 label `"ZONE"` is lowercased to `"zone"`. Native6E3460
  emits runtime335 channel/author/text, optional nonzero author_id, and
  dev/mentor/ranger/publisher/use_encounter_icon from bits0/1/2/3/6.
  `MatrixZoneChatPayload` now names these proven fields. Saved projection,
  announcement broadcast mapping and distinct category2/4/11 gates.
  High DWORD flags are not extra roles; all nonzero IDs are forwarded,
  without challenge/LFG validity filtering. Post-runtime6F46F0 consumer and
  callback reentrancy remain unresolved.

- **All35 normal-buffer server bodies34-68 saved:** final34 body is a
  WORD-length opaque blob plus raw flag;36 is announcement type/data raw
  texts. Only exact `"sysmsg"` displays with label `"broadcast"` and an
  existing mode. Nonzero34 flag snapshots the post-virtual-operation manager
  buffer; zero does not clear existing pending state. Added all35 minimum-
  packet/one-byte-short coverage checks and `MatrixServerMessageBodies.md`
  index. This is wire-layout coverage, not complete field/consumer semantics
  or alternate-header-buffer support.34 blob transformation and snapshot
  lifetime remain unresolved.

- **Matrix63-68:** saved two/seven WORD samples63/64, original-log-named
  LFGMatchFound65 and LFGLeaderChange66, positional QWORD/text/text/DWORD67,
  and68's two mandatory EnterZone-schema metadata maps (no presence byte on
  wire; destination optional becomes present). Separate LFG+48/+50 gates
  and nonmonotonic WORD-sample destinations are modeled. Corrected misleading
  native quick-chat labels at6F4A50/6E8BB0 to bounded LFG roles; actual RTTI
  Character command144 remains unchanged. Full LFG/67 consumers and sample
  meanings remain unresolved. Normal-buffer bodies now cover37-68;34/36
  are the remaining initial catalog bodies to inspect.

- **Matrix56-61:** saved ForceUnqueue reason/QWORD list/DWORD;57's whole-
  remaining-buffer body (not length-prefixed);58's shared five-QWORD/boolean
  clock parameters;59 SuperPing QWORD list/DWORD;60's two WORD/raw-text lists;
  61's recursive WORD records. Native61 roots stride16, nodes stride32:
  nodes serialize nine WORDs before their child list, not native memory
  order. Iterative decoder covers600-level depth and255/256 chunks.
  ForceUnqueue runtime291 reasons/local-player ID mask and clock58's separate
  stored/running pause flags, rebase, retained repeated bound and native
  assertion conditions are modeled. Full57/60/61 consumer semantics remain
  unresolved; do not invent event names or text encodings.

- **Challenge51-55 continuation:** saved QWORD-only51/53/55, QWORD/chunked
  native24-byte-record list52, and QWORD/DWORD54 wire bodies. Native24-byte
  records contain only QWORD/text/byte/DWORD/byte on wire, not padding.
  `MatrixChallengeUpdateNativeLayout.cs` preserves distinct ID policies:
  51/52/54 accept exact or invalid stored ID;53/55 require exact equality.
  Runtime boundaries are275/273/276/277/278, respectively. The word selector
  chooses first matching16-byte join record, or raw all-FF index. Meanings
  of that selected word and runtime276/278 remain unresolved.

- **Challenge follow-up continuation:** server48 cancellation is a single
  QWORD;49 invitation response is QWORD/name/status;50 kicked notification
  is QWORD/kicker-name/kickee-QWORD/kickee-name. Saved normal decoders in
  `MatrixServerMessages48To50Wire.cs` and snapshot selections in
  `MatrixChallengeFollowupNativeLayout.cs`. Cancellation selects every
  exact invitation-ID match. Response runtime272 accepts only status0;
  all8 reason names plus explicit `UNKNOWN` for8..255 are recovered.
  Kicked runtime274 uses kicker/kickee labels; matching local ID selects
  active+E4 teardown and runtime275. Native null-local getter becomes0 for
  that comparison; no valid-identity assumption or reentrancy simulation.

- **Found-match/challenge continuation:** saved server44-47 bodies in
  `MatrixServerMessages44To47Wire.cs`. Native logs establish44 as
  FoundMatchUpdate with matchmaker/match IDs,45 as a challenge join response,
  and46/47 as invitation variants sharing a consumer with final flags0/1.
  `MatrixChallengeNativeLayout.cs` captures join-ID gating, active-object
  release selection and runtime269/270/277 reason precedence; deeper party
  behavior remains unresolved. Added bounded found-match deadline/suppression
  selections to `MatrixPvpQueueNativeLayout.cs`. List reader reuse now lives
  in `MatrixChunkedListWire.cs`; native16-byte record padding is never assumed
  to be wire padding, and raw join-flag bytes normalize only at the handler.

- **Adjacent server-body continuation:** `MatrixServerMessages41To43Wire.cs`
  now decodes41's opaque scalar prefix/optional12-byte entries/blob,42's
  PvP queue record list plus comparison word, and43's DWORD/WORD body.
  Both list readers use255-chunk counts and preserve order/duplicates.
  PvP consumer `0x85D560` accepts nonempty records on wildcard9 or a matching
  raw comparison word, but runtime289 `"queued"` reports original input
  presence even if mismatch clears stored records. Saved bounded selection
  in `MatrixPvpQueueNativeLayout.cs`; text/record field identities and deeper
  callbacks are unresolved. Handler43 has no observed payload use, but its
  wire body is still present. See the final `docs\MatrixConnection.md` section.

- **Latest Matrix lifecycle continuation:** queue status is server message62,
  not ExitZone; runtime295 exposes the two raw DWORD properties. Actual
  ExitZone40 is header-only and uses outgoing header-only ExitZoneAck19 on
  channel1. Both wire/state models are saved. The follow-up at `0xBD2090`
  establishes a socket destroy request, not unconditional object release:
  active states select Destroyed8, disconnected/NotOpened select Closed9,
  and terminal states assert. `GameSocketDestroyNativeLayout.cs` preserves
  this selection and the first atomic-marker gate. Deeper world cleanup
  (`0x64E210`, secondary connection cleanup `0x494A40`) is saved as raw
  offsets/call boundaries in `WorldZoneCleanupNativeLayout.cs`. Follow-up
  static evidence records that the state-5 test is assertion-only with visible
  fallthrough; the downstream action pointer gates `0x7FEB50`; helper
  `0x976430` records two link chains, value/node release order, an event367
  callback, node+0x20 nested-range walking followed by separate slot release,
  and two 16-entry range helpers that can replace their backing pointers. The
  `0x9795B0` call takes the `0x3F7680` full-tree clear path; `0x3F7300` clears
  child nodes and the owner count, while `0x3F7430` handles partial erasure.
  `0x975020` adds two nested sentinel walks and tree-like node-removal helpers.
  Two other callers pass node+0x10 values to it with different guards; the
  `0x977F20` value+4 loop is a synchronous drain because `0x978440` directly
  decrements that field. This supports a compatible shape, not a proven type.
  Container identities and callback effects remain unresolved. Local-client
  cleanup `0x64E210` has no observed direct pending-mode enqueue: its current
  xref lists only caller `0x494B00` (state5), and the guarded `0x369A30` path
  forwards an opaque local record to `0x101D710`. This is not a transition
  proof or a binary-wide negative. Reset helpers `0x4A2FB0`/`0x431C90` are
  bounded. The `0x861020` path also
  releases a guarded record through `0xD74A20`; its `0x3C8` header, state
  comparison, token-vector removal, and cleanup are saved without claiming an
  app-mode transition. Its separate +0x32C virtual cleanup object is populated
  by a runtime vtable result; slot0 argument1 is observed, but its target is
  unknown and is not named as a destructor. `0x424790` atomically
  releases the pointer at released-object+0x320. Its tag-0x17 final-release
  branch routes the header to the shared-text allocator's lock-free free
  stack; other tag meanings remain unresolved. General ownership remains
  unresolved. Startup checks
  against original and installed binaries and the full solution build passed.
  Deferred close is now
  traced: Destroyed tick checks keepalive/retries, assembles data, then the
  assembler tail requests pump close if still Destroyed. The close callback
  sets+D94 and enqueues token1; queued dispatch changes Destroyed8 to Closed9
  with channel cleanup, or active1..5 to DisconnectedNotify6. Saved in
  `GameSocketCloseCompletionNativeLayout.cs`. See the final
  sections of `docs\MatrixConnection.md`.

- `Firefall.Reconstruction.slnx` builds the partial C# reconstruction. Start
  with the root `README.md` for the subsystem list and check commands.
- `src\Serialization\` contains the SDB block reader, schema-inspection CLI,
  type table, fixed-width field conversions, and runtime row-layout helpers.
  `tests\Serialization\` has synthetic checks. The most recent pass added
  float/integer and half-float conversion paths, runtime-layout comparison,
  first-match field-hash mapping, source/destination null-bit operations, and
  bounds-checked raw row/field access. A helper expands parsed table metadata
  into the compared runtime layout, normalizing type bytes >= 27 to zero.
  `SdbRowConverter.ConvertRows` now reconstructs the bounded field-hash
  fallback for scalar conversions, null propagation and zero-initialized
  unmapped fields; it does not model native callbacks or the identical-layout
  fast path. The native differing-layout row-buffer allocation and callback
  sequence is documented in `src\Serialization\README.md`.
  SDB table fields formerly called `UnknownA` and `UnknownB` are now named
  `RowStride` and `NullBitmapOffset`, and the
  field's `Unknown` is now `ValueOffset`, based on row-mapper reads.
  See `src\Serialization\README.md` for exact instruction offsets and limits.
- Other documented areas: `src\Containers\`, `src\Collections\`,
  `src\Graphics\Nvapi\`, `src\Characters\`, `src\Scripting\LuaCache\`, and
  `docs\Startup.md` / `docs\Platform.md`. These are separate partial
  reconstructions, not a replacement client.
- **App-mode dispatch update (this session):** `Client_ApplyPendingAppModes`
  (`0x30D850`) maps queued mode values 1/2/3 to RTTI-confirmed
  `tfLogin`, `tfWorld`, and `tfLookingGlass` objects, allocated at `0xA58`,
  `0x2904`, and `0x1368` bytes, respectively. `tfLogin` is also identified by
  its initialization-failure string; `tfLookingGlass`'s RTTI hierarchy
  includes `nl::ReplayReader::GameInterface`. Follow-up read-only traces
  identified the second interface vtable and replay-reader initialization,
  failure, completion, and disabled UI binding; mode 2's constructor, scene,
  layer objects, and selected virtual behavior are documented in
  `docs\WorldMode.md`. `src\Startup\` now records the replay interface and
  tested `tfWorld` layout constants. Full evidence and uncertainty boundaries
  are in `docs\Startup.md` and `docs\WorldMode.md`.
- **`src\Characters\` major update (this session)**: found the full, dense
  array of all 169 `firefall::Character::Commands` message-class vtables
  (raw `0x1864cd4`-`0x1865774`, 16 bytes/entry, zero gaps) by noticing two
  newly-traced classes' vtables sat back-to-back and walking the array
  outward. Dumped the complete catalog (class name + vtable + 3 vtable-slot
  addresses + COL/type-descriptor addresses, all raw) to
  `src\Characters\MessageCatalog.csv` — this is now the master index for all
  future Character::Commands work. Also documented and validated a general,
  reusable **RTTI name → type-descriptor → COL → vtable → constructor**
  raw-byte-search pipeline (IDA's automatic xref analysis is unreliable in
  this raw-loaded IDB) — see `src\Characters\README.md` "Technique" section.
  Used it to fully trace two new classes, `ResetTutorialId` and
  `DismissTutorialId`: both are a single `tutorialId` u32 field at `+0x28`
  with a `[1,14]` range check in both a production and debug-logging
  constructor variant; `DismissTutorialId` is built from a Lua-script-exposed
  dispatcher (raw `0xa2ba30`), confirming it's reachable from in-game script.
  **Caveat**: the 3 generic vtable slots per class are *not* per-class
  Sender/Encoder/Gate functions — IDA has merged many of these small stubs
  into large undifferentiated blobs (e.g. raw `0x9B73F0`, 0xEF8 bytes), so
  they resist cheap per-class tracing; the constructor-via-vtable-xref step
  of the pipeline is the practical way to learn a class's fields instead.
- The active IDA database at
  `D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe.i64`
  contains saved function names and annotations, including the renamed
  `Sdb_InitConvertedTableResult` and lifecycle comments for
  `Sdb_ResetLoadedDatabaseState`. The original executable was not modified.

## Current findings (October 2 continuation)

These updates supersede older next-step suggestions below where they overlap.

- **SDB result-holder teardown is resolved.** The tree at database `this+0x34`
  maps catalog table hashes to native 0x20-byte result holders. The holder
  stores catalog pointer at `+0`, row count at `+4`, exposed rows pointer at
  `+0x0C`, and a refcounted handle at `+0x18`. Reset follows the holder's
  catalog pointer to the optional catalog `+0x24` callback, calls it with the
  holder's count and exposed rows pointer, then releases/frees the holder.
  `SdbNativeMappedTableResult` models the verified 32-bit layout. Catalog
  compiled catalog. Its opaque tail shows 573/575 hex-string/DWORD pairs and
  two exceptions; no semantic meaning or consumer is established. The separate
  database tree at `this+0x28` is now laid out as an auxiliary buffer map whose
  key is also used as a backing-buffer displacement; key/value semantics and
  conditional value ownership remain unresolved.
- **SDB row-handle dispatch is bounded.** The example handle vtable at PE VA
  `0x1C6883C` is at raw file offset `0x186743C` by the executable's section
  table (the PE headers give this `.rdata` section a VA-minus-raw delta of
  `0x401400`). Its first two pointers map to raw `0x70D3F0` (slot 0) and
  `0x716B20` (slot 1). Slot 0 stamps `unk_1C68830` into the object and
  conditionally calls raw offset `0x12C8D60`; slot 1 clears global state and a
  16-bit member at `+8` for 12-byte records in a global range. The relationship of
  those slot effects to row-handle ownership is not established.
  `Sdb_InitConvertedTableResult` initializes holder `+0x18` to null; the
  mapped-row path has no assignment after initialization, so the non-null
  producer remains unknown. In the base-zero IDA database, `0x13122D0` is a
  raw file offset with 283 code-call xrefs. The decoder at raw `0x131233C`
  uses bit 0 as an encoding selector: set means displacement `(value>>1)+2`
  and a `u16` length read at `dword_1E4BAD0+value`; clear means displacement
  `(value>>1)&0x7FFFFF` and length from the DWORD's high byte. Its fingerprint
  is `8B 0B 8A 03 D1 E9 24 01 74 05 8D 79 02 EB 08 8B F9 81 E7 FF FF 7F 00
  84 C0 74 0B A1 D0 BA E4 01 0F B7 34 01 EB 04 0F B6 73 03`. A 15-function
  sample across the binary shows `arg1` pointing at runtime object/table
  fields (including direct `this+offset`, table slots, and map-result fields),
  not a consistent `.data` constant or a common type-tag class. The returned
  values are used as string-like data in sampled callers; exact producer-side
  meaning of the two encoded forms and common ownership remain unresolved.
  Examples: table slots at raw `0x3090F0`; record field `+0x10` at `0x38BCD0`;
  map/member sources at `0x63D160`, `0x4B6B40`, `0x6A8610`, `0xAB60C0`,
  `0x1333080`, and `0x9E6C50`; direct fields `+0x94/+0xB0` at `0x1338B00`.
  The wrappers at raw `0x2FE4D0`/`0x2FE4F0` pass `this` and `this+4`,
  respectively, as `arg1`; wrapper fingerprints begin `55 8B EC 56 8B 75 08
  56 51 51 E8` and `55 8B EC 56 8B 75 08 8D 41 04 56 50 51 E8`. This sample
  classifies only observed callers, not all 283 xrefs. Full evidence and
  uncertainty boundaries are in `src\Serialization\README.md`.
- **`tfLogin` mode behavior has a bounded native snapshot.** The `0xA58`-byte
  mode loads `LoginUI.xml`, registers 15 observed input actions, selects a
  background zone, and handles login, character-refresh, and zone-list
  responses. Auto-login settings and request branches are cataloged in
  `src\Startup\LoginModeNativeLayout.cs`; behavior and uncertainty boundaries
  are in `docs\LoginMode.md`. The observed Demo/DefaultSettings path can remove
  saved UI settings and keybinds; this is documented only and is not performed
  by the reconstruction.
- **Login response callbacks are matched to their handler bodies.** The
  constructor creates three `0x20`-byte wrapper objects with the owner at
  wrapper `+4` and target function pointer at `+0x10`. Their targets map to
  the Login, character-refresh, and zone-list response handlers at raw
  `0x63B7C0`, `0x63C5F0`, and `0x63D270`, respectively (constructor target
  VAs `0xA3C3C0`, `0xA3D1F0`, `0xA3DE70`). Raw
  `Login_QueueLoginRequest` (`0x63B550`) copies the first wrapper/member pair
  from `+0x9D8/+0x9DC` to request `+0x50/+0x54`. This establishes the Login
  request's callback target. Its vtable slot `+4` reaches raw `0x643380`,
  which tail-calls the stored target with the Login owner as `this`; the
  request manager's worker delivery path and the other two attachment sites
  are now resolved as described below.   OS completion signaling is now traced below.
- **All three Login callback request attachments are identified.** Character
  refresh builder raw `0x63ECE0` copies Login `+0x9E8/+0x9EC` into request
  `+0x50/+0x54`, sets native type 1 at `+8`, formats
  `%s/api/v2/characters/list`, and gates successful submissions with byte
  `+0x9F0`. The state-2 branch of auto-login dispatcher `0x63E390` constructs
  the zone-list request with callback pair `+0xA00/+0xA04`, native type 2,
  and `%s/api/v1/server/list`; it is conditional, not unconditional. All
  three request paths share constructor `0x3843F0`, preparation `0x636550`,
  and submission wrapper `0x6499F0`. Worker inspection now proves type 1
  selects GET and type 2 selects POST.
  The state jump table at VA `0xA3F574` maps to raw `.text` `0x63E974`;
  uncorrected raw-loaded IDA decompilation misses its branch bodies.
  Catalog and checks are in `src\Startup\`; details in `docs\LoginMode.md`.
- **Request-manager callback delivery is now traced.** Manager pump
  `0xBC4550` visits route pump `0xBC3F80`, then worker `0xBC2B30`.
  A nonnull worker `+0x1C` completion whose first DWORD is zero reaches
  result handling. Completion `+0x18` points to the response, whose `+4`
  points to the request. Raw `0xBC37E2` acquires a strong callback from
  request weak pair `+0x50/+0x54` using `0xBC4A80` and atomic helper
  `0x327730`; raw `0xBC37FC` invokes acquired callback slot `+4` with the
  response pointer. This connects to the Login wrappers' `0x643380`
  trampoline. Expired callbacks are skipped, not resurrected.
  Worker `0xBC2B30` calls imported `WinHttpOpenRequest`; its corrected
  `.text` table at raw `0xBC38C0` maps types 2/3/4/5 to
  POST/PUT/DELETE/HEAD, with GET as the default. This mapping and response/
  completion layout snapshots are in `ClientApiRequestNativeLayout.cs`.
  OS completion signaling is now established below; detailed retry/error
  handling and thread affinity remain incomplete.
- **WinHTTP completion signaling is now established.** The registered
  five-argument status callback is raw `0xBC26E0` / VA `0xFC32E0`;
  raw `0xBC1DA0` is its internal dispatcher, not the OS entry.
  Worker `0xBC31E9` registers it via `WinHttpSetStatusCallback` and
  `0xBC31F8` passes the `0x1C`-byte completion record as SendRequest context.
  Completion `+0` is the actual live request handle. HANDLE_CLOSING status
  `0x800` clears it at `0xBC2728`, making the worker's zero-handle gate
  eligible. Terminal internal dispatcher results close the handle; empty
  READ_COMPLETE sets result kind 1 if unset. A distinct synchronous
  failure path also clears the handle. Context byte `+0x11` selects
  callback-owned immediate cleanup versus retaining it for the worker;
  its teardown ownership transfer is now established below. Layout and result-preservation
  behavior are saved in `ClientApiWinHttpCompletionNativeLayout`.
- **Detached WinHTTP cleanup ownership is bounded.** Teardown `0xBC2890`
  sets active completion byte `+0x11` at `0xBC28CB`; route destructor
  `0xBC3B80` calls it before freeing workers, so HANDLE_CLOSING owns the
  detached response/context cleanup. Idle pruning bypasses workers with
  active `+0x1C` completions. Teardown closes a separate worker handle at
  `+0x20`, not visibly completion request handle `+0`; full request-close/
  cancellation guarantees remain unknown and are not claimed.
- **Zone-list fields and bounded selection are reconstructed.** Decoder
  `0x635BE0` builds 24-byte records from `zone_id`, `zone_name`,
  `protocol_version`, `build`, `matrix_url`, `players`. Handler `0x63D270`
  gates signed auto-login state >=3 and swap-removes records whose protocol
  differs from `0x4C5F` or whose players WORD is zero. It selects with
  `rand()%count` among all candidates for filter `random` or among native
  zone-name matches otherwise, then overwrites Login `+0xA20` with
  `matrix_url` (not zone name/ID). Failure logs, clears that text, and
  increments state as success does. `LoginZoneListSelection.cs` models
  the pure bounded behavior with injected zone matching/random sample;
  SDB/localization and CRT locale are not guessed.
- **Auto-login state 5 now connects selection to connection preparation.**
  Corrected jump-table case 5 reaches `0x63E890`, searches character records
  by the selected pair, and calls `0x63EE30` on record `+0xC8` properties.
  Keys `name/is_dev/race/gender` are verified; it resets/allocates a
  `0xBB0` connection object at Login `+0x1C`. Then `0x63F230` uses matrix
  destination/override text, validates host/port, gates unsigned connection
  state <2, and calls `0x494CA0`. That reachable branch sets connection
  state2 and calls `0x494E30`; the helper's message24 branch is only for
  states6/7 and is not reached from this gate. No successful World-mode
  transition is inferred.
- **Matrix connection's oracle-ticket entry is traced.** `0x494E30`
  builds a type-2 POST `%s/api/v1/oracle/ticket`, attaches callback
  pair connection `+0xB98/+0xB9C`, and uses the shared request manager.
  This wrapper is 16 bytes (owner+4,target+8,adjustment+C), unlike Login's
  32-byte wrapper; its target is raw `0x495460`. Oracle parsing
  `0x4956C0` checks response kind1 and status/100==2, extracts literal-backed
  `matrix_url/ticket/datacenter/operator_override/session_id/hostname`,
  and reaches `0x495F20`, which allocates/opens a `0xDF8` transport via
  `0xBD00B0/0xBD0960`, then sets connection state3. Failure while state2
  updates delay `min(300,current+2+(current+1)/2)` and schedules another
  ticket request. C# bounded delay model and native snapshots are in
  `MatrixConnectionNativeLayout.cs`; evidence and unknowns in
  `docs\MatrixConnection.md`. No handshake/World transition is inferred.
- **Matrix transport and initial packet are preserved.** `0xBD0960`
  creates an overlapped UDP socket, binds up to five preferred ports plus
  two ephemeral candidates, registers with a pump, and posts a 1400-byte
  `WSARecvFrom`. `MatrixTransportNativeLayout.cs` models the pure bind
  candidate calculation including 16-bit wrap. The open-success application
  packet is message `0x11`, with fixed size30 plus injected text/blob bytes,
  encoded by `0x349E60`; **`0x338120` is its destructor**. Unknown field
  meanings remain offset-named in `MatrixInitialPacket.cs`. `0xBD3B20`
  submits that buffer on channel1; raw switch translation establishes
  shared channel1/2 fragment queues and unreliable channel3 promotion to2.
  `MatrixTransportFragmentPlan.cs` models only queued header/slice plans,
  not final UDP framing. Continue with queue draining/sequence assignment
  or IOCP receive dispatch; no complete handshake or World-mode edge proven.
- **Shared channel1/2 queue promotion/retry is partly decoded.** Constructor
  `0xBE3CE0` creates `0x5A8` queues; `0xBE49E0` promotes up to512 outstanding
  fragments, assigns wrapping WORD sequences, and considers a 256-sequence
  window. `0xBE4DE0` prepares retries with a distinct 250..2500 millisecond
  policy and header attempt markers. Observed failure thresholds are16
  attempts or15000ms full-window timeout, conditional on socket `+0x100`.
  `GameSocketReliableQueueNativeLayout.cs` saves pure interval/window helpers.
  Continue from acknowledgment handler `0xBE43B0` or actual queue-to-datagram
  emission; selected-entry flags and final framing remain incomplete.
- **Fragment-to-datagram boundary is now bounded.** `0xBD4340` assembles
  data only in socket states5/8, beginning with an opaque network-order
  DWORD prefix and offering ACK/control blocks before data. `0xBD4240`
  serializes fragment header/sequence big-endian; `0xBD58E0` XORs payload
  with FF/AA/CC for markers1/2/3. `MatrixDatagramFragmentEncoder.cs`
  preserves this portion without claiming a complete datagram. `0xBD4780`
  appends optional probe-like filler, not a general compression/encryption
  pass. `0xBD2980` reaches Winsock sendto/send and explicitly reports errors.
  Still unknown: handshake states, opaque prefix semantics, full ACK/control
  layout, connected-send flag setter. Read-only child “Matrix receive
  dispatch” is tracing the separate IOCP receive boundary.
- **IOCP receive boundary is integrated.** RTTI proves `nl::Network`,
  its embedded `nl::Network::PumpThread`, and `nl::GameSocket`. Network
  associates each socket with IOCP using the socket as completion key;
  PumpThread `0xBBC860` routes successful nonnull operations to socket
  slot+18 (`0xBD2E30`), which calls `WSAGetOverlappedResult`, decodes
  sender sockaddr, enters packet parser `0xBD3030`, and normally reposts
  `WSARecvFrom`. `NetworkIoCompletionNativeLayout.cs` preserves layout
  and dispatch evidence. Error/suppression paths do not all repost.
  The read-only child is finished; continue at packet entry or handshake
  response routing, not by re-requesting its completed IOCP trace.
- **Network shutdown ownership is partly bounded.** Network vtable slot+8
  (`0xBBD940`) calls WSOCK32 ordinals22/3 (shutdown mode2, close) on a
  socket handle supplied by pointer, writes `-1`, and posts a completion to
  wake the pump. GameSocket deleting destruction closes its handle at+`0x118`
  through `0xBE36B0`; slot+`0x1C` (`0xBD2D10`) is only the queue/close
  notification. Helpers `0xBC3970`/`0xBC3990` call `WinHttpCloseHandle`
  and clear `this+0x20`/`this`; no direct `E8`/`E9` caller or `.rdata`
  VA/vtable pointer was found for those helpers. The eventual close of Network IOCP handle+`0xDC`
  and standalone-drain owner remain unresolved. See
  `docs\MatrixConnection.md` for exact references and caveats.
- **Outer admission and zero-context handshake are saved.**
  `GameSocketPacketAdmission.cs` models exactly5..1400 bytes, BE context
  comparison, and sender-address/port gates. `GameSocketHandshake.cs`
  encodes POKE/KISS and decodes ABRT/HEHE/HUGG with exact stage/port
  behavior and abort map. Native replies write candidate+224, word+228,
  remoteport+248, and expectedcontext+114, but do not themselves advance
  socket state. Continue at nonzero-context control handler `0xBD3510`
  (decompiler fails due raw switch) and its state-transition driver.
- **Control routing and the transport Connected gate are established.**
  Logged native state IDs0..9 are cataloged in
  `GameSocketStateAndControlCatalog.cs`. Translated `0xBD3510`/`0xBD3760`
  switches separate immediate keepalive/clock/close controls from deferred
  ACKs for queues1/2. A deferred clock reply while socket state3 and
  clock-object sample count>=2 calls `0xBD1B70` to set5/Connected.
  `GameSocketChunkHeader.cs` inspects only exact2..1392-byte header gates,
  not unsafe/complete payload decoding. Follow virtual+10 (`0xBD4BC0`)
  through queued dispatcher `0xBD3E50`, and state setter `0xBD1B70`'s
  translated cases, to establish owner notification/tick progression.
- **Handshake tick progression is now linked.** `0xBD4BC0` consumes
  receive queue+260 then translated state table+rawBD505C. Nonzero HEHE
  candidate causes Initializing1→Transferring2 and immediate KISS; HUGG
  context causes2→Connecting3 with clock object, or2→Connected5 without
  one. Queue activator `0xBE3F60` proves HUGG word+228 is a sequence seed
  and initializes reliable retry to1000ms. Transfer timeout restarts1
  using separate byte+11D, not a direct KISS resend.
  `GameSocketTickNativeLayout.cs` models only ready-context guards.
  Next: Connected tick calls `0xBD4A20/0xBD5080` and virtual+14
  `0xBD5170` likely delivers pending owner events; verify rather than infer.
- **Queued delivery is now verified, not inferred.** `0xBD5170` invokes
  owner slot4 for channel1, slot8(flag1) for channel2, slot8(flag0) for
  channel3, and slotC when state6 transitions to7. No Connected callback
  was established. Connecting3 releases early data-channel fragments;
  successful normal data handlers assume ownership rather than the caller
  always freeing them. Native primary MatrixConnection vtable1C642D0 maps
  these slots to4904A0/4904C0/490540. Next concrete trace: channel1 wrapper
  4904A0→33CE40; other channels forward via owner+910.
- **Channel1's object is a descriptor, not raw packet bytes.** Wrapper4904A0
  adds4 to ECX: dispatcher33CE40 uses the secondary interface at connection+4.
  Descriptor DWORD+10 IDs34..68 map to35 slots+4..8C; all mappings are saved
  under `src\Startup\MatrixServerMessages\`. Native names are numeric msg-N.
  Msg35→490AD0 asserts receiver+8 (connection+C) state3, copies descriptor
  +28/+2C to connection+860/+864 and a reference field+34 to+86C, samples
  clock into+818, changes connection state4, combines config masks at+83D.
  Scalar model and guards are saved; field semantics and wire decoding remain
  unresolved. Next: msg37 handler490C10 and the descriptor creation path in
  reliable receive, accounting for the secondary receiver adjustment.
- **EnterZone is now anchored to msg37.** Original log argument pushes prove
  descriptor instance+28, zone ID+30, match+58, zone-name text+68. Handler
  490C10 asserts connection4 then writes6; copy491070 maps descriptor+28 to
  owner+878, plus64-byte block+80→owner+8D0. Preview/Disabled clears low3;
  ForceZone/Asset/DataPreview set1/4/2. Spectator byte+C0 sets owner+83D bit2.
  Scalar C# model saved. Native state4 log is "Connected to matrix".
  Next: queued descriptor creation/reassembly and downstream EnterZone
  initialization; do not identify connection6 with socket6 or World mode.
- **Reliable receive storage/decode boundary is linked.** BE51F0 classifies
  WORD delta from queue+2A:0..255 window,256..32767 future drop,>=32768 old
  drop. Ring+78 indexed by(head+478+delta)%256; new fragments reverse XOR
  once before storage. BE56D0→BE5440 consumes contiguous slots, appends
  continuation into+594, final-decodes via plain pointer+14, stamps decoded
  object+20 channel/+24 size, queues+60, then advances/recycles fragment.
  Pure ring/window model saved and exhaustively checked in synthetic suite.
  Next: constructor BE3CE0 / GameSocket open queue creation to identify
  decoder+14. Accumulator callback ABI and selective ACK logic remain partial.
- **Decoder addresses are now proven from constructor stores.** Native48F12C
  installs owner+828 VA73EEE0(raw33E2E0), tag+824=B968;48F14E installs
  owner+830 VAC227A0(raw821BA0), tag+82C=4C5F. Socket open copies pair1 to
  channel1, pair2 to2/3 and tag WORDs to socket+30/+32 (POKE/KISS). Channel1
  decoder calls34A020 then33D4B0. Its decompiler reports bad SP: verify
  arguments/instruction framing before reconstructing wire fields.
- **Header/factory roles corrected by instructions.** 34A020 reads compact
  positive U16 (nonzero byte, or zero+LE WORD strictly>255); malformed
  extended0..255 throws. Matching writer320900 is reconstructed and all
  65535 valid values round-trip checked. Factory is33D4B0, not34A020;
  table raw33E250 covers34..68. Next focused bodies: msg35 factory target
  33D557→constructor339AB0/read3570B0; msg37 corresponding factory case.
  Keep bad-SP decoder pseudocode separate from verified instructions.
- **Msg35 normal-buffer wire body is saved.** Factory33D557 allocates50h
  via339AB0 then body3570B0 reads8 raw bytes(+28) via356F80 and two blobs
  (+30/+40) via12EE170, each raw LE ushort length+bytes. Packet minimum13;
  blob cap65535. C# decoder includes prefix truncation, exact cap, malformed
  compact headers, managed-copy and trailing-byte checks. The word pair
  at owner+860/+864 is one opaque u64; semantic names still unresolved.
  Next: EnterZone/msg37 factory/body and shared-text reader12EE010,
  without assuming its bytes are UTF-8 or synthesizing object references.
- **EnterZone absent-metadata wire variant saved.** Factory33D629 allocatesC8
  via339EA0, body356E60→zone-fields356770. Zone fields use raw LE scalars
  and two NUL byte strings; optional metadata3565C0 starts with presence
  byte and, when present, parses two trees355140/355E50. C# explicitly
  rejects that present variant. Absent minimum103 bytes. Suffix is56 raw
  bytes, bool-normalized byte, raw spectator byte: seven native padding
  bytes are not serialized. Shared span reader reused by msg35.
  Next concrete boundary: those two optional-metadata tree readers.
- **EnterZone present metadata is now decoded.** Readers355140/355E50
  parse separate NUL-byte-key maps with raw LE16/32-bit values, chunked
  u8 counts (continue on255, including terminal0 for exact multiples).
  Lookup3550A0/355DB0 proves unsigned byte ordering and last-value-wins.
  Copy3563E0 uses input+0/+8: both adjacent temporary maps survive, not
  just the first decompiler local. Saved C# preserves opaque value bits,
  owned key bytes, absent versus present-empty, and the unchanged suffix.
  Full `Decode` supports both variants; restricted entry point remains.
  Startup binary-evidence checker renamed to match its broader scope.
  Next: downstream EnterZone state6 initialization and outgoing message24;
  do not infer a World app-mode request from connection state6.
- **EnterZone downstream clock/response is now linked.** Five suffix QWORDs
  +90..B0 and normalized+B8 initialize clock owner+918 via8FA620; +A0 is
  double rate, +98 reference tick, +B0 offset, +A8 upper limit, +B8 limit flag.
  Sampler493650→advance8FA700 follows. Limit flag owner+948 selects runtime110
  `"paused":1`, independently of spectator. Global subsystem386900 receives
  owner byte+8A0; downstream owner+910 requires DWORD+28 zero then sets byte+18.
  Owner+B94 selects msg24: gate498350 requires socket and signed state>4,
  stamps timeGetTime*1000, copies owner+878 context, sender499840→49A520 emits
  raw u64 context then compact24 (9 bytes), channel2. Saved separate client
  message and downstream-plan C# models. Getter6F4540 selects existing mode
  object's pointer (Login+14, World+28E8, LookingGlass+68), not a new mode.
  Clock initialization/unscaled projection saved; scaled numeric conversion
  and native floating-point exception flags remain the next bounded trace.
- **Finite clock scaling is now saved.** Original8FA780 subtracts reference
  with unsigned64 wrap, helper159E17E converts to double, MULSD applies rate,
  helper159E060 truncates into unsigned64 bits, then reference/offset wrap.
  C# supports finite in-range values including negative scaled elapsed bits;
  near-unit tolerance skips conversion, limits apply before monotonic cursor.
  Nonfinite/out-of-range conversion remains explicitly unsupported because
  native integer-indefinite return and floating exception flags are not simulated.
- **Socket synchronized-clock value projection saved.** BE8410 returns full
  EDX:EAX, not DWORD. Source is timeGetTime*1000 minus clock+18 reference,
  unsigned64→double→single, MULSS rate+14, helper159DE7F→u64 bits, add base+20,
  then sign-of-wrapped-difference monotonic clamp against cached+60.
  Do not replace its single-precision stage with EnterZone's double arithmetic.
  Shared bounded clock arithmetic, original-instruction fingerprints and synthetic
  rounding/wrap checks are saved. Native critical section+28/calibration remains
  outside the value model; numeric exception flags remain an explicit boundary.
- **Adjacent messages38/39 are scalar updates, not proven zone exit.**
  38→491380 copies descriptor+28's16 bytes to owner+8D0 (first16 bytes
  of EnterZone block), no state/clock change. Factory33D68F→33A340 alloc38h;
  reader3545D0 reads raw LE8+4+4, ctor words0/0/3F800000/0. 39→4913A0 calls
  global+C64 subsystem386900 with raw desc+28 byte then stores owner+8A0.
  Factory33D6F2→33A420 alloc30h;337180 reads raw byte. Saved packet decoders
  and opaque bit-preserving models, sizes17/2 bytes. Formal value semantics
  unresolved; next likely lifecycle target is msg62 handler491140.
- **Lifecycle target correction and actual ExitZone saved.** Msg62/491140 is
  queue status: raw DWORDs+28/+2C become runtime295 queue_position/queue_length,
  variant tag1, receiver+AC, no state write. Factory33DF77→33C310 alloc30h;
  reader34B610 raw4+4, packet9. Actual ExitZone msg40/491230 is header-only,
  factory33D757→33A4E0 alloc28h, packet1. Cleanup494B00 requires downstream
  +910, releases nested+B4, asserts state6/7→5 and calls current-mode64E210
  (resource resets/runtime97). Post-cleanup+B94 selects release socket/state1;
  zero selects ExitZoneAck/state4, send allowed iff socket && signed state>3.
  Ack native log identifies code19, encoder499970 emits only that compact code,
  channel1 via498D70. Saved bounded branch/cleanup guards; full lifetime/resource
  reset behavior and external-call reentrancy remain unresolved.
- **Character-refresh outgoing receiver is now explicit.** Raw `0x63CEF2`
  loads global VA `0x1E002B8` (raw `.data` `0x19FE6B8`). The indirect call
  at `0x63CF05` uses vtable `+0xAC` with message `0x89`, local buffer, and
  zero. The translated file value is zero; reading raw EA `0x1E002B8`
  instead reads unrelated resource bytes. Its runtime class and connection
  to a successful Login-to-World transition remain unresolved.
- **Login request callback attachment is now directly traced.** Raw
  `Login_QueueLoginRequest` (`0x63B550`) allocates a `0x78`-byte request
  object, writes type 2 at object `+8`, and copies the Login callback pair at
  `+0x9D8/+0x9DC` to request `+0x50/+0x54` before submission through the
  global request manager. The Login object gates submission with `+0x9E0`
  and records the caller's option byte at `+0x9E2` on success. The copied
  pair is the constructor wrapper targeting the Login response handler.
  Its callback vtable trampoline and worker delivery gate are now known.
  Shared submitter `0xBC43C0` validates the request, performs a per-route
  lookup/create, and links the request into a `0x38`-byte tracker containing
  the manager pointer, request shared text at `+0x10`, and a DWORD at `+0x14`.
  That submission path does not consume the callback pair; the worker pump
  consumes it after completion as documented above.
  The login-response routine's retry branch is also now bounded: option byte
  `+0x9E2` must be nonzero and response text nonempty; it decrements the
  counter at `+0x9E4` (constructor value 3), waits 500 ms while the remaining
  count is positive, then calls `Login_StartAutoLogin`. The interactive
  request passes option 0 and the auto-login request passes option 1. Checks
  and constants for these observations are saved with the Login layout.
  C# constants and checks are in `src\Startup\LoginModeNativeLayout.cs` and
  `src\Startup\Checks\Program.cs`; see `docs\LoginMode.md`.
- **Login-to-mode transition boundary was checked directly.** Full decompilation
  of `Client_ApplyPendingAppModes` (`0x30D850`) confirms request IDs 1/2/3 map
  to Login/World/LookingGlass, respectively; an earlier delegated report had
  reversed IDs 2 and 3. Queue entries are `{modeId, payload}` at application
  `+0xB84` (head/index `+0xB90`, count `+0xB94`). The character-refresh
  callback copies the first two DWORDs of a matched `0xD0`-byte record to
  `tfLogin+0xA40/+0xA44`, then crosses an indirect vtable `+0xAC` call with
  message `0x89`. Separately, raw `0x6385C0` is present in the `tfLogin`
  vtable at raw `0x18662C0 + 0x0C`; its conditional branch at `0x638B89`
  passes request pair `{2,0}` to `AppMode_EnqueuePendingModeRequest`
  (`0x30DEB0`). The login response handler also calls the shared runtime
  message endpoint at raw `0x63C1E2` with ID `0x145`. These establish a World
  queue path and a response-side event dispatch, but not that this event causes
  that update path or that successful login reaches its queue condition. The
  selected pair is not labeled a GUID and the complete successful transition
  remains unresolved. Details and layout constants are in `docs\LoginMode.md`
  and `src\Startup\`.
- **tfLogin `0x638B89` guard (static).** Observed: the default switch case
  (`0x638AB5`, `mov [esi+1Ch],0`) reads global `0x1E5CAA4` and tests byte
  `[obj+0x40]` (`0x638ABC..0x638AC6`); zero jumps to `0x638B89` (`{2,0}` to
  `0x30DEB0` on global `0x1DFFD98`), nonzero checks cvar-like objects
  `0x1E50C8C`/`0x1E6075C` (+0x40) and queues mode 3 via `0x30D590`. The only
  code reference to `0x1E5CAA4` is that read; its writer is not found
  statically, and the 0x145 login-response link is unproven. `0x30DEB0`: if
  `[app+0xC]!=0` and mode==1 it dedupes against current-mode `[+0xA28]`,
  otherwise pushes `{mode,payload}` into the `+0xB84` queue (`0x315060`);
  `0x30D850` drains it, mode 2 constructs `AppMode_World_Ctor`.
  Interpretation: this is a world-load request; trigger unresolved. Bytes are
  checked in `StartupBinaryEvidence`.
- **tfLogin `0x6385C0` switch (static).** Observed: per-frame update; selector
  `[[esi+0x1C]+0xC]` (`0x6388D8`), 5-entry table raw `0x638D1C` (VA `0xA3991C`):
  case 1 -> `0x6389F2` (sends message `0x146`, string from `[node+0x10]`),
  cases 0/2/3/4 -> `0x638C3A`, >4 -> `0x638AB3` (then `0x638AB5` guard).
  `0x63C1E2` (msg `0x145`) is in a separate function and was not seen
  producing a `[node+0xC]` value. `0x1E5CAA4` has one byte/reloc reference
  (`0x638ABD`); its writer is indirect and unfound. Producer of the list at
  `tfLogin+0x20` unresolved. Checked in `StartupBinaryEvidence`.
- **World scene-factory return boundary.** Raw `0x9B73F0`, called from
  `TfWorld_Initialize` (`0x64B460`), returns `EDI` through `EAX` at raw
  `0x9B82B4` (`8B C7`); that epilogue returns at `0x9B82C6` (`C3`). The
  factory allocates a `0xA0` root aggregate (constructor `0x9B82F0`, cleanup
  `0x9B8490`), loads `RenderScene`, `WorldMapScene`, and `SinvironmentScene`
  into root offsets `0/4/8`, and owns multiple child allocations. The root has
  no observed vfptr; its child at `+0x14` is `tfPhysicsDrawInterface`
  (`0x188` bytes, vtable VA `0x1C70968`, RTTI `.?AVtfPhysicsDrawInterface@@`).
  It passes a registration helper's result into root `+0x10`; teardown releases
  that handle and the scene references. `TfWorld_Initialize` stores the root at
  `tfWorld+0x14` (`0x64B508`) and publishes root `[0]` to global
  `0x1E00060+0xF8`. Another static caller at `0x9B2D1A` stores the result at
  its object base. Field meanings beyond these observed roles and registration
  target semantics remain unresolved. Details and original-file fingerprints
  are in `docs\MatrixConnection.md` and `SceneFactoryNativeLayout`.
- **LookingGlass entry points expanded.** The shared mode-3 enqueue helper
  (`0x30D590`) has six static callers: `0x306050`, `0x3D60A0`, `0x6385C0`,
  `0x65D400`, `0x6AE3E0`, and `0x6AF120`. Several request paths update the
  replay reader directly if LookingGlass is already active and enqueue mode 3
  otherwise; the specific `GoLookingGlass` UI binding remains disabled. This
  corrects any interpretation that the disabled UI handler makes mode 3
  unreachable. `AppMode_TryQueueLookingGlassRequest` (`0x306050`) packages a
  text value and two DWORDs with registry-dependent text handling. The caller
  offsets are cataloged in `AppModeCatalog`; request semantics and complete
  external entry conditions remain partly unknown.
- **LookingGlass request context is now traced through construction.** The
  mode-3 enqueue helper copies `0x34` bytes from the caller's request context
  into application `+0x9F0`; `Client_ApplyPendingAppModes` passes that address
  to the LookingGlass constructor, which copies it to mode `+0x18`. Two
  shared-text references at context `+0x14/+0x1C` are explicitly retained.
  A separate DWORD from application `+0x9E8` is passed to the constructor and
  stored at mode `+0x58`. Other fields and the extra DWORD remain opaque.
  `LookingGlassNativeLayout` records these offsets, and IDA names the copy
  helpers `SharedTextContext_AssignFrom` and
  `LookingGlass_CopyQueuedRequestContext`.
- **Default initial mode is confirmed.** In the guarded application-init path,
  helper `0x307224` checks queue count at application `+0xB94`; if empty, it
  calls `AppMode_EnqueueModeIdWithZeroPayload` (`0x324080`) with ID 1 and then
  drains the queue immediately through `Client_ApplyPendingAppModes`. This
  establishes Login as the observed default startup mode only when no pending
  request already exists; it does not resolve a later Login-to-World response
  transition.
- **Another Login request source is observed.** Helper `0x30D790` performs an
  opaque state update, then enqueues `{1,0}` if a selected observer object
  is missing, has null `+0x820`, or signed `+0x0C <= 3`; otherwise it calls a
  different routine on that object. The meaning of that object and the
  fallback's product semantics remain unknown. Documented in `docs\Startup.md`.
- **LookingGlass replay behavior is traced, format is not.** Its
  `GameInterface` subobject is at `+4`; the reader is built at object
  `+0x1334` (4916).
  Initialization errors and replay-end auto-quit behavior have concrete
  functions/strings in `docs\Startup.md`. The `GoLookingGlass` UI route is
  explicitly disabled, but this does not prove that no other route can enter
  mode 3.
- **`tfWorld` mode 2 has a bounded native snapshot.** Its size is `0x2904`,
  owner/context is at `+0x10`, scene pointer at `+0x14`, and four layer
  members and their constructor IDs are recorded in
  `TfWorldNativeLayout`. `docs\WorldMode.md` records selected virtual methods
  and the bounded 32-byte slot-11 record handling, while separating unknown
  layer/owner/record semantics. Slots 17/18 are confirmed as simple getters
  for `+0x2E8/+0x2EC`; the constructor initializes the former to zero and the
  latter to a pointer to an opaque `0x2C`-byte block.
- **Selected remote-observer dispatch gates are documented.** Messages 174,
  175, and 194 use a shared observer availability and signed-threshold gate;
  the UpdateShoppingList builder also applies that gate inline. Its source map
  has a unique key at entry `+0x14` and an embedded inner set; the builder
  copies outer entries into one ordered set and unions inner-set values into a
  second, then serializes both with 255-item chunks. The two descriptor words
  at `+0x18/+0x1C` form a 64-bit routing key whose low byte is forced to 4;
  exact entity identity and element semantics remain hypotheses. Four
  post-send calls are collection cleanup, not more dispatches. Details and
  evidence are in `docs\RemoteCharacterDispatch.md`; wire models remain
  unchanged. Alternate-candidate tracing found no additional producer for
  the persistent `+0x00` map: `0xA242A0` and `0x9F65E0` are view readers,
  `0x4A0700` is teardown, and writers at `0xACDD40`/`0x909AE0` target a
  distinct text-keyed map at state `+0x2C`.
- Replay, World, SDB callback, Login, SDB catalog-metadata, and selected
  remote-observer dispatch investigations completed read-only analysis; their
  results were integrated here to avoid concurrent file writes. The original
  executable remains untouched. IDA names/comments and the IDB were saved.
- UpdateShoppingList collection/source and alternate-channel traces completed
  read-only analysis and were integrated. The Login-to-World report was
  reconciled against the dispatcher and integrated with its uncertainty
  boundaries; the app-mode queue mapping conflict is resolved.

## Verification and evidence cautions

- Latest October 2 continuation additionally validates zone-list filtering/
  swap order, signed state progression, WinHTTP close signaling and teardown
  ownership, oracle callback layout, and capped retry-delay sequence. The
  solution builds with zero warnings/errors; binary-backed checks use the
  `.original` backup as well as the installed file. New names/comments are
  saved in the IDB. Native request/callback code remains offline-only.

- October 2 continuation: all Startup checks, including 14 optional
  binary-backed assertions for callback attachment, verb selection, completion
  dispatch, and translated global data, passed against the installed
  `FirefallClient.exe` and separately against `FirefallClient.exe.original`.
  The solution build passed with zero warnings/errors.
  Eleven verified function names and nine evidence comments were saved in the
  existing IDB. No executable patches were made. The earlier idle reports have
  been integrated; none was waiting for input or approval.
- Executable baseline caution: the installed file and `.original` backup have
  equal lengths but differ at 13 bytes: raw `0x2EAD58-0x2EAD59`,
  `0x30682D`, `0x1250EB3-0x1250EB4`, and `0x178E5A4-0x178E5AB`.
  This continuation made no executable writes and did not restore these
  differences; their origin is not established here. None overlaps the
  newly recorded request/callback instructions, which pass the binary-backed
  checks in both files. Do not infer whole-file equality from those checks.
- WinHTTP status callback registration/signaling and context byte `+0x11`
  cleanup-ownership follow-ups completed and were integrated immediately.
  Next connection-system boundary:
  `0x494E30` after state-5 character preparation. Keep OS callback completion
  distinct from app-mode transition.

- Latest Login continuation: callback wrapper targets now match all three
  response-handler bodies; its dispatch trampoline forwards the Login owner
  to the stored target. Startup checks passed, and
  `dotnet build Firefall.Reconstruction.slnx -v:q` succeeded with zero
  warnings/errors after recording the request callback-pair attachment and
  bounded retry gate/counter.

- Latest continuation verification: Character, all 15 Startup, Serialization,
  Collections, and LuaCache checks passed; `dotnet build
  Firefall.Reconstruction.slnx -v:q` succeeded with zero warnings and errors.
  Startup checks passed with pending-mode, LookingGlass-call-site,
  request-context, and initial-mode assertions; the full solution build
  succeeded with zero warnings and errors. Login, app-mode queue,
  LookingGlass-call-path, `tfWorld`, and observer-dispatch IDA names/comments
  were saved to the IDB at
  `D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe.i64`.
- A workspace scan found no standalone `.sdb`, `.db`, or `.dat` fixture under
  the reconstruction folder; current Serialization verification remains
  synthetic-only.
- Latest verification after the row-converter type guard:
  `dotnet run --project tests\Serialization\SerializationChecks.csproj -v:q`
  passed; `dotnet build Firefall.Reconstruction.slnx -v:q` passed with zero
  warnings and errors. These checks use synthetic SDB data, **not** a shipped
  SDB file.
- Current equipment-request continuation: `dotnet run --project
  src\Characters\Checks\Characters.Checks.csproj -v:q` passed, including
  message 208 empty/nested/chunk-boundary payload checks; solution build
  passed with zero warnings/errors. IDA names/comments and IDB were saved;
  generated Character bin/obj directories were removed after verification.
- Repair/salvage continuation: focused Character checks passed for message
  203 (1..100 caller constraint, little-endian u64 payload) and message
  202 (1..200 caller constraint, 16-byte records, complete body order).
  `dotnet build Firefall.Reconstruction.slnx -v:q` passed with zero warnings
  and errors; IDA annotations were saved and Character bin/obj cleaned.
- Teleport/respawn continuation: focused Character checks and full solution
  build passed after adding conservative native-layout records. `RequestTeleport`
  has a real caller and opaque u64 payload; `RequestRespawn` is kind-only
  (metadata ID 198) and is queued from a timed/flag-gated caller. Its concrete
  writer was independently verified at 0x5062D0/0x50BA00 to emit handle + ID
  (9 bytes before transport framing); `RequestRespawn.EncodeBody()` now covers
  this. IDA names/comments and IDB were saved; generated Character outputs
  are cleaned after final checks.
- Duel request continuation: `DuelRequest` (188) now has a conservative
  descriptor/object-layout model and documented real chat-related callers
  (modes 4 and 3/5). Shared text and two pair vectors are confirmed copied;
  semantics and class-specific wire order remain unknown. Focused checks
  and the full solution build passed; IDA annotations saved; generated
  Character outputs cleaned.
- Transfer-request continuation: `RequestTransfer` (199) has a Lua-reachable
  builder and verified 14-byte body writer (target + ID + u32 + byte).
  `FindNearbyResourceAreas` is instead message 183 and has a kind-only
  constructor; the two RTTI/vtable identities are now corrected in the code,
  notes, and IDB.
- Vehicle-calldown continuation: `VehicleCalldownRequest` (179) has a
  populated native-layout model with its 0x40-byte opaque copied block,
  positional values, and two trailing flag bytes. Two construction/send
  call paths are documented; no class-specific wire writer was identified,
  so no packet encoder was inferred. IDA annotations are saved.
- Deployable-calldown continuation: `DeployableCalldownRequest` (180) has a
  populated native-layout model with the same copied 0x40-byte block and
  positional tail as VehicleCalldownRequest, but a u32 rather than u16 at
  `+0x28`. Its discovered state-gated caller is documented; no wire encoder
  was inferred. IDA annotations are saved.
- Bounty/achievement-request continuation: the former provisional traces
  have been aligned to catalog RTTI identity. `ClearPreviousBounties` (162)
  has a u32 at `+0x28`; `RequestRerollBounties` (164) owns a copied u32
  vector at `+0x28`; `RequestAchievementStatus` (169) has a u32 at `+0x28`.
  Each caller/layout is documented without overclaiming field meanings.
- Movement/projectile class identities were rechecked against the correct
  vtable VA for each catalog row: `MovementInput` (115) has a complex
  descriptor with a process-local shared reference; `MovementInputFake`
  (116) has scalar fields plus an optional descriptor and shared reference;
  `FireWeaponProjectile` (121) has four u32s and an optional three-u32 block.
  Their constructors and discovered uses are documented; no wire order was
  inferred for these three.
- Weapon-command continuation: `SelectWeapon` (125) stores a u32 and two
  bytes, `ReloadWeapon` (126) stores one u32, `AcquireWeaponTarget` (128)
  stores four positional u32s and a byte, and `LoseWeaponTarget` (129)
  stores three positional u32s. Their constructors and real call paths are
  documented. Semantic field names and packet encoders remain unverified.
- Adjacent weapon commands: `ReportProjectileHit` (122) has an unaligned
  sequence of u16 and byte fields; `SelectFireMode` (123) and `UseScope`
  (124) each store a u32 and byte; `CancelReload` (127) stores a u32.
  Constructor/caller evidence is documented, without guessing wire formats.
- Firing-state commands: `FireInputIgnored` (117) stores a u32 and byte;
  `FireBurst` (118), `FireEnd` (119), and `FireCancel` (120) each store a
  u32. Their populated constructors are reached by gameplay/control paths;
  fields remain positional pending stronger semantic evidence.
- Effect/target commands: `NPCApplyEffect` (130) has three positional u32s,
  `NPCRemoveEffect` (131) one u32, `ChangeLookAtTarget` (133) an optional
  six-u32 group, and `TargetAbility` (136) one byte. Constructor layouts
  are documented; no static construction callers were resolved for these
  populated variants.
- Interaction commands: `DockToPlayer` (132) copies five u32 fields;
  `NPCInteractWithTarget` (135) writes five u32s with a gap at `+0x2C`.
  Their populated constructors currently have no resolved static call refs.
- Ability/emote commands: `DeactivateAbility` (137) writes a u32 and byte
  and is constructed from a guarded dispatch path; `PerformEmote` (142)
  writes a u32 and signed u16 and has an update-path caller. Field meanings
  and packet order remain open.
- Consumable/weapon-control commands: `ActivateConsumable` (138) carries
  two positional u32s from a guarded state path; `SetNoSpreadFlag` (139)
  carries two bytes; `ExitAttachmentRequest` (140) is kind-only. Their
  layouts and available caller/constructor evidence are in the Character
  README.
- Achievement/bounty queries: `ListPreviousBounties` (163) stores a u32
  and has a caller; `ListAchievements` (168) stores one byte; and
  `RequestAllAchievements` (170) is kind-only. Field meanings and unobserved
  wire layouts remain explicitly unknown.
- `AbortCampaignMission` (152) stores one positional u32 and has a
  production caller; `AnimationUpdate` (149) stores two u32s and a byte
  with no populated caller found; `TryResumeTutorialChain` (153) is
  kind-only with a timed caller.
- `CallForHelp` (151) stores a u32 and has a timed/state-gated caller;
  `ListAvailableBounties` (160) stores two u32s and has a caller;
  `SetDialogTag` (147) stores a process-local shared-text reference, one
  u32, and two bytes, with no populated caller identified. These are
  RTTI-matched names/IDs; meanings and wire layouts remain unknown.
- Added the nearby dialog/loadout commands `NotifyDialogScriptComplete`
  (143), `PerformQuickChatCommand` (144), `PerformDialog` (146),
  `SetEffectsFlag` (148), `SelectLoadout` (150), and `DebugMission` (154).
  The broad field layout of `PerformDialog` is positional; `DebugMission`
  contains an opaque shared-text pointer.
- Expanded bounty coverage with `AssignBounties` (155), `AbortBounty`
  (156), `ActivateBounty` (157), `ListActiveBounties` (158),
  `ListActiveBountyDetails` (159), `ClearBounties` (161), `TrackBounty`
  (165), `SetBountyVar` (166), and kind-only `RefreshBounties` (167).
  Caller evidence and unassigned field semantics are in the Character README.
- `ActivateAbility` (134) now has both constructors and a real build/send
  path traced. The populated constructor at `0x531840` copies three DWORDs
  at `+0x6C..+0x74`, but its only known send path uses the debug constructor
  and leaves those fields untouched even though writer `0x5BEEA0` emits them;
  their values may be uninitialized. The C# model preserves them as nullable.
  `NPCSetInteractionType` (141) has a conservative
  snapshot for its byte plus copied u64 entry vector; bitset semantics and
  wire order remain unknown.
- `SendMailToPlayer` (84) stores two positional u32s, three shared-text
  references, and a copied vector of 24-byte records. One populated caller
  was found; record semantics and wire serialization remain unknown.
- Latest continuation added native snapshots for `SendMailToPlayer` (84),
  `ListItemForSale` (83), `ClearSavedMatchQueue` (91), `MatchAccept` (93),
  `PlayerReady` (88), `ToggleMarketplace` (86), `FillBuyOrder` (85),
  `PerformTextChat` (145), `MatchmakerSetPenalties` (92), `JoinGroupLeader`
  (87), `FetchQueueInfo` (89), `ClientUIEvent` (196),
  `VendorPurchaseRequest` (238), `VendorTokenMachineRequest` (200),
  `FindServiceProvider` (195), `UpdateFriendStatus` (249),
  `FriendsListRequest` (248), `ClientQueryInteractionStatus` (186), and
  `ClaimBountyRewards` (250), with regression checks. It also extended
  `ActivateAbility`'s partial model with nullable fields copied by its
  uncalled populated constructor.
  `dotnet run --project
  src\Characters\Checks\Characters.Checks.csproj -v:q` passed, followed by
  `dotnet build Firefall.Reconstruction.slnx -v:q` with zero warnings or
  errors. Generated Character `bin`/`obj` directories were removed. The
  `MessageCatalog.csv` header was corrected: `vtable_entry_raw` is the COL
  slot and `vtable_raw` is the actual vtable address used in code references.
  The span also contains an unrelated `nl::MessageBaseT<firefall::Character::
  Fabrication_Finalize_Response>` RTTI record at `0x1865754`, intentionally
  excluded from the 169-row Character-command CSV.
  IDA constructor names/comments were saved to the IDB.
- `ListItemForSale` (83) now has a positional native-layout snapshot with
  two retained shared-text references and a trailing byte; no populated
  caller or field semantics were recovered.
- `ClearSavedMatchQueue` (91) has a minimum initialized prefix model:
  two positional u32s at `+0x18/+0x1C`, zeroed words at `+0x20/+0x24`,
  and a real caller that packs a discriminator-2 target. The words remain
  unnamed and no wire body is claimed.
- `MatchAccept` (93) has an inherited target pair, a u32 at `+0x28`,
  two u32s at `+0x30/+0x34`, and one byte at `+0x38`; no populated caller
  was found.
- `PlayerReady` (88) has two positional bytes and a presence-flagged vector
  of copied 8-byte entries at `+0x30`; entries are opaque u64s, with no
  populated caller or wire layout established.
- `ToggleMarketplace` (86) stores one byte at `+0x28`; its empty constructor
  sets that byte to 1, but the byte's semantics and caller remain unknown.
- `FillBuyOrder` (85) has eight positional u32s at `+0x28..+0x44` and a
  real parsed-input caller; field semantics and wire order remain unknown.
- `MatchmakerSetPenalties` (92) stores u32/byte/u32/byte fields from
  `+0x28` through `+0x34`; no populated caller or field meanings were found.
- `JoinGroupLeader` (87) has a single observed constructor with no subclass
  fields; only the inherited target is initialized.
- `FetchQueueInfo` (89) copies and retains 4-byte references in a vector at
  `+0x28`, followed by three positional u32s; no caller or field meanings
  were found.
- Movement-simulation continuation: `SetMovementSimulation` (197) stores
  one u16 at `+0x28`. Its only found tick caller requires a set state byte,
  an observer connection, and a >1000 ms interval; it then clears the state
  and refreshes the timer. Field meaning and packet layout remain unknown.
- Latest Character-command validation: added checks for the corrected
  `MovementInput`/`MovementInputFake`/`FireWeaponProjectile` layouts and
  firing/weapon command layouts 117-129, plus NPC/effect/target layouts
  84, 130-144, 146-170. The Character checks passed and
  `dotnet build Firefall.Reconstruction.slnx -v:q` passed with zero warnings
  or errors. The IDB was saved after naming/commenting the weapon-command
  constructors; generated Character `bin`/`obj` directories were cleaned.
- This continuation **corrected** a prior mistake and resolved the catalog
  question: the runtime table catalog at database-object `+4` is a static,
  compiled-in array of 575 entries (count `+4`, pointer array `+8`); no code
  mutates it after global init, so there is no runtime "registration" routine
  to find. The catalog **entry itself** (not a nested `entry+0x14` descriptor
  as previously stated) is passed directly into `Sdb_ConvertRowsByFieldMapping`
  as the destination schema; `entry+0x14` was a misreading of the *locally
  parsed file's* tree-node offset, not the catalog entry's. A worked example
  (table hash `0x8E3A6BCC`) shows the catalog entry layout (`+0x08` stride,
  `+0x14` field count, `+0x18` field array, `+0x1C`/`+0x20` callbacks) and
  decompiles both its per-row init callback (clears a field before conversion)
  and its finalize callback (builds an array of lightweight row-handle view
  objects over the real converted data). See `src\Serialization\README.md`.
- Two requested read-only sub-agent traces could not access the active IDA
  database in their isolated contexts. They returned no new findings; direct
  analysis in the parent session remains available.
- IDA opened the PE as a raw image at base zero. Its function addresses are
  **file offsets**; absolute operands and indirect jump targets are PE virtual
  addresses. Translate through `docs\BinaryLayout.md` before interpreting
  them. The decompilation around `0x1311100` and `0x1310600` misreads switch
  tables/raw bytes; use corrected raw disassembly as evidence.
- Numeric conversions model output bytes under normal masked floating-point
  exceptions. Native exception flags/control-state effects and rounding-mode
  changes are not reproduced.

## Earlier suggested next work (superseded where noted above)

1. The differing-layout path at raw `0x1311BD0` now has a partial native trace:
   it builds a 0x20-byte result holder, reserves `rowCount * destinationStride`
   bytes, invokes destination layout callback `+0x1C` per row, updates a
   destination-field bitset from `0x1311100` return values, and optionally
   calls callback `+0x20` with row count, first-row address, and a stride
   pointer. The identical-layout path has now been traced too: it bypasses
   mapping, allocation, row initialization and conversion, keeps the source
   row pointer, then reaches the common callback/result assignment. **Resolved**:
   the destination-field bitset (`Sdb_CreateFieldConversionWarnedBitset`, raw
   `0x1312B40`) is a one-warning-per-field-per-call suppressor for unsupported
   type-conversion pairs, not a correctness mechanism — see
   `src\Serialization\README.md` for the full evidence trail (bitset seeded
   all-true, cleared on first conversion failure per field, gates the
   `"Could not convert field %s (%s) to %s"` log call at raw `0x1311847`).
   Example `+0x1C`/`+0x20` callback bodies are documented, but their full
   per-table behavior and ownership remain unresolved. The `+0x24` teardown
   call path is now traced through database reset; see current findings above.
   Full offsets are in `src\Serialization\README.md`.
2. **Resolved for the mapped-table tree:** `Sdb_ResetLoadedDatabaseState`
   (`0x1312030`) traverses the tree at `this+0x34`, whose nodes contain the
   catalog table hash at `+0x10` and result-holder pointer at `+0x14`. The
   mapper inserts `[catalogEntry+4, resultHolder]` into this tree; reset
   follows the holder's catalog pointer to catalog callback `+0x24`, calls it
   with holder row count `+4` and exposed rows pointer `+0x0C`, releases its
   refcounted handle at `+0x18`, then frees it. This supersedes the earlier
   hypothesis that the mapped value's `+0x00` was a vtable: it is a catalog
   entry pointer. The separate tree at `this+0x28` is now modeled as an
   auxiliary buffer map with a derived displacement key and pointer-like
   value; key/value semantics and its conditional value ownership remain
   unresolved. Function
   names and offsets for both tree cleanup paths are documented in
   `src\Serialization\README.md`.
3. **Resolved**: the parser's database object has a static, compiled-in table
   catalog at `this+4` (count at catalog `+4`, pointer array at `+8`, 575
   entries in the shipped binary). Each entry **is itself** the destination
   layout descriptor (hash at `+4`, stride `+8`, field count `+14`, field
   array `+18`, init callback `+1C`, finalize callback `+20`) — there is no
   extra indirection and no runtime registration call; the catalog is static
   data established before any traced code runs. Two different example
   callback pairs are now decompiled and documented (hash `0x8E3A6BCC`: clear
   two bytes + build a row-handle view array; hash `0x4CCBB136`: write real
   per-field float defaults + a **NULL** finalize callback, confirming the
   mapper's "use the raw row address directly" fallback is real). A further
   finding: both structs' `+0x00` field, previously guessed as a vtable, is
   actually a pointer to a packed, NUL-terminated ASCII string holding the
   hex text of that struct's own hash (verified against two independent
   examples) — almost certainly a debug-name aid generated alongside the hash
   at build time, since the real source name is not retained anywhere found.
   **Correction (this session)**: the previous guess above for `+0x24`/
   `+0x28`/`+0x2C` was wrong, confirmed by dumping raw struct bytes for both
   examples. `+0x24` is **not** a hash — it is a genuine third callback-
   pointer slot, the teardown counterpart to `+0x20`'s row-handle-array
   builder: example `0x8E3A6BCC` has `+0x24` = raw `0x712920`
   (`Sdb_RowHandleArrayDestroyCallback_Example`), which, given a non-null
   array pointer, invokes each element's vtable slot 0 (`(**v5)(v5, 0)`) at
   an 8-byte stride before freeing the array via the generic allocator —
   exactly undoing what `+0x20`'s builder constructs. Example `0x4CCBB136`
   has `+0x24 = 0` (NULL), consistent with its `+0x20` also being NULL (no
   handle array is ever built for that table, so there is nothing to tear
   down). A full scan of all575 compiled catalog entries found573 cases where
   `+0x28` points to lowercase `"0x"` plus eight hex digits parsing to the
   `+0x2C` DWORD. Entry index10 instead points to `"hands"` and a relocated
   `"backpack"` pointer; entry index19 is all-`0xFFFFFFFF`. The relocation
   table confirms +0x28 is pointer-valued for574 entries and +0x2C for the
   single `"backpack"` exception. The repeated pairing is not a universal
   invariant or semantic name; traced parser/mapper/reset paths do not consume
   either field, so retain them as opaque raw DWORDs.
   **(b) Resolved**: type codes 1/3/9/11 seen in example #2's field array
   were cross-checked field-by-field against the init callback's exact byte
   offsets (`src\Serialization\README.md`) — every write's offset and width
   matches `SdbFieldTypes`' documented storage size for each type with zero
   discrepancy, independently confirming the storage-width table against
   real runtime callback behavior.
4. If a genuine SDB fixture is located within an authorized workspace,
   compare parser/field output against it before claiming compatibility.
5. **`src\Characters\MessageCatalog.csv` has 169 `Character::Commands`
   classes, all now represented by C# native-layout snapshots.** Exact
   same-name file matching finds 163; the six remaining classes use grouped
   helper models (`SlotVisualRequest`, `UnslotAllModulesRequest`,
   `SlotModuleRequest`, `UpdateShoppingList`, `LogActivityPush`, and
   `LogDirectActivityRequest`). See `src\Characters\README.md` for offsets,
   constructor/caller evidence, and explicit unknowns. The raw-image
   constructor pipeline remains reusable for additional classes outside this
   catalog: RTTI string → type descriptor → COL → vtable → raw-byte-search
   the vtable VA for code refs → decompile constructor hits. Keep native
   layouts distinct from wire encodings; selected message encoders have
   separate evidence and regression checks.
6. **Startup follow-up completed:** LookingGlass' secondary
   `nl::ReplayReader::GameInterface` vtable, reader initialization, failure
   path, replay-end auto-quit, and disabled `GoLookingGlass` UI binding are
   documented in `docs\Startup.md`. The file format and other possible entry
   paths remain unknown. `tfWorld` layout and selected virtual behaviors are
   covered by `docs\WorldMode.md`.

   Recent additions: `LootDistributionSetVotes` (219) snapshots a DWORD and
   copied opaque u64 vector; caller `0x6F2C90` deduplicates values from parsed
   records. `UiQueryResponse` (59) snapshots three DWORDs and a copied
   u32-to-u32 map; caller `0x976E80` builds the map from matched UI queries.
   `LootDistributionSetState` (218) snapshots a DWORD and byte with varying
   caller values; `RequestTrackerUpdate` (217) snapshots two positional
   DWORDs and submits on one validated caller branch. `LeaveArc` (214) is
   kind-only with caller `0x927EE0`; `RequestFrameLevelReset` (211) stores
   one DWORD with no found production xref to its populated constructor.

   The previous batch added forty-four message snapshots: `JobLedgerOperation` (215)
   contains a shared-text reference and copied u32 vector; observed callers
   `0x9282C0`/`0x928400` add an absent or remove a matching list value before
   submission. `ExecuteTinkeringPlan` (237) has a DWORD and three copied
   vectors of opaque 16-byte elements. `QueueUnstuck` (178) is kind-only.
   `NPCCombatUpdate` (233), `EquipExperimentalLoadout` (236),
   `Fabrication_Finalize` (246), `Fabrication_Claim` (247),
   `Fabrication_GenerateResult` (245), and `Fabrication_ApplyAction` (244)
   preserve positional scalar fields. For these other than the cited
   JobLedger callers, no direct populated-constructor production xrefs were
   found; meanings and wire formats remain unknown. Constructor/destructor
   names and evidence comments were saved to the client IDB.
   `Fabrication_FetchInstance` (242) adds one positional DWORD at `+0x28`;
   no populated-constructor caller was found.
   `Fabrication_Start` (243) adds a DWORD and byte at `+0x28/+0x2C`, with no
   populated-constructor caller found.

   `Fabrication_FetchAllInstances` (240) is kind-only; constructor `0x53DFB0`
   writes no subclass payload and no direct production caller was found.
   `Fabrication_FetchAllRecipes` (241) is likewise kind-only; constructor
   `0x53E0E0` writes no subclass payload.
   `SetSteamUserId` (235) copies a 64-bit positional value to `+0x28`;
   interpreting it as Steam User ID follows the RTTI class name but lacks
   caller confirmation.
   `FlushCharacterCache` (231) copies five positional bytes at `+0x28..+0x2C`;
   no populated-constructor caller was found.
   `BagInventorySettings` (234) has an optional nested object at `+0x2C`
   with a copied shared-object handle at `+0x30/+0x34` and trailing DWORD
   `+0x38`; its pointer semantics remain intentionally opaque. Nested vtable
   slot 1 resolves to deleting wrapper `0x5A9E20`; reference-pair cleanup
   behavior was not established. `TutorialEventTriggeredCmd` (239) stores
   DWORDs at `+0x28/+0x30/+0x34` and a byte at `+0x38`; `+0x2C` is left
   untouched by both observed constructors, and field semantics remain unknown.
   The elite-level commands `RevertAllEliteLevelsUpgrades` (228),
   `SelectEliteLevelsAward` (229), and `ResetAllEliteLevelsUpgrades_Debug`
   (230) include two kind-only forms and a one-DWORD form; `RunTeamManagerCommand`
   (232) has two bytes and a retained shared-text reference.
   Reward commands `ClaimDailyRewardStreak` (223), `BuyBackPreviousDay` (224),
   and `AcceptRewards` (225) each store one positional DWORD.
   `ClaimDailyRewardItem` (222) has two observed constructors that zero a
   DWORD at `+0x28` but no populated copy form found; `FlushRewards` (226)
   and `RerollEliteLevelsAwardList` (227) are kind-only.
   `RequestMissionAvailability` (171) has three DWORDs; `RequestNewActivity`
   (172) is kind-only; `RequestPushMission` (173) has one DWORD.
   `TimedDailyRewardRequest` (201) and `SeatChangeRequest` (216) each have
   one byte, while `RequestSelfRevive` (209) is kind-only.
   `MapOpened` (109) and `BattleframePurchased` (110) are kind-only;
   `CollectLoot` (111) has four DWORDs; `TempSlotAbilities` (112) initializes
   four zero DWORDs and a `0xFFFFFFFF` sentinel; `SinAcquire_Source` (113)
   has two DWORDs; and `BroadcastWeaponTweaks` (114) has two bytes, a
   reference-counted pair, and a trailing DWORD.
   Challenge messages 94-106 are now modeled with positional fields:
   `ChallengeSwapTeam` has six DWORDs; `ChallengeSetRoleAndTeam` has four
   DWORDs, a byte, and a DWORD; `ChallengeSetMatchParameters` has three
   DWORDs; `ChallengeSetPowerPrivilege` has four DWORDs and a byte; and
   `ChallengeStartMatch` has two DWORDs. A default `ChallengeSetMatchParameters`
   constructor was not confidently resolved. IDs 94-101 are modeled too:
   `ChallengeCreate` and `ChallengeInvitationSquadInfo` copy opaque u64
   identifier vectors, and the latter plus `ChallengeInvitation` and
   `ChallengeKick` retain opaque shared-text references. Challenge
   constructors have been named/commented in IDA; the
   `ChallengeSetMatchParameters` default form remains unlocated. Regression
   checks cover all thirteen Challenge models and passed.

   `PickupCarryableObjectByProximity` (189) and `DropCarryableObject` (190)
   are modeled as opaque 64-bit identifiers at `+0x28`. The pickup populated
   constructor is used by callers `0x93E450` and `0x93E590`; the drop
   populated constructor has no direct static caller in the current IDB.
   `SlotGearRequest` (206) is also modeled with the observed untouched gap at
   `+0x2C`, a positional u32 at `+0x28`, an 8-byte value at `+0x30`, a byte
   at `+0x38`, and a DWORD at `+0x3C`. Caller `0x4D6710` matches a slot-list
   entry before constructing and submitting it.
   `LFGCheckin` (107) and `LFGLeave` (108) are now modeled with two positional
   DWORDs each at `+0x28/+0x2C`. The populated `LFGLeave` form is used in
   `0x6E3060`; no direct populated `LFGCheckin` caller was found.
   `JoinSquadLeadersArc` (213) and `ResourceLocationInfosRequest` (187) have
   no subclass payload in their observed constructors. `LeaveEncounterParty`
   (212) copies two DWORDs. `UpdateChatPartyMembers` (185) copies an opaque
   vector of 16-byte records, followed by DWORDs at `+0x38/+0x3C`, with an
   untouched gap at `+0x34`.
   `MatchQueue` (90) has two copied vectors (8-byte and 4-byte elements),
   an opaque qword, positional scalar fields, and two retained shared-text
   references; its four observed constructor callers are recorded in the
   Character README.
   `ResetTutorialId` (220) and `DismissTutorialId` (221) are modeled with
   their tutorial-ID DWORDs; the observed Lua-facing caller range is 1..14.
   `DeployableHardpointSelection` (181) and
   `ResourceNodeBeaconCalldownRequest` (182) each copy three positional
   DWORDs at `+0x28/+0x2C/+0x30`; populated-constructor callers are documented.
   `CameraPoseUpdate` (177) is modeled with the inherited target and two
   optional, positional payload blocks. The simple constructor is used by two
   camera-pose-change paths; the richer overload has no static caller.
   `AiSignal` (192) and `AiError` (191) preserve their copied opaque fields
   and process-local shared-text references without assigning semantics.
   `LogLongTimeWithoutPush` (176) is kind-only, `GeographicalReportRequest`
   (184) has one positional byte, and `NonDevDebugCommand` (193) copies a
   byte, a DWORD, and a shared-text reference. Character regression checks
   passed and the solution build succeeded with zero warnings/errors.
   Name matching shows **163/169** source-file matches; the remaining six
   catalog names are represented by helper models (`SlotVisualRequest`,
   `UnslotAllModulesRequest`, `SlotModuleRequest`, `UpdateShoppingList`,
   `LogActivityPush`, and `LogDirectActivityRequest`), so all catalog entries
   are modeled. The IDB was saved after naming/comment updates; clean generated
   Character build artifacts before wrapping up.

   Character command coverage is complete for the current 169-entry catalog.
   Next, map another subsystem from the binary RTTI/message catalog. Preserve
   uncertain fields as positional/opaque and avoid inferring wire encodings
   from native object constructors alone.

The earlier stop-for-day boundary was explicitly resumed by subsequent user
requests. Continue with bounded, evidence-backed static reconstruction as
directed; preserve saved source and IDB state, and do not execute or patch the
sample. No recurring session automation is configured. Overall reconstruction
remains incomplete.

Scene-factory follow-up: tfPhysicsDrawInterface has 14 verified in-text vtable slots (slot 9 forwards to slot 8; slot 13 logs the model-release string), its constructor field layout is saved in SceneFactoryNativeLayout, and the 0x64B528 publish stores root[0] into [[0x1E00060]+0xF8] (pointer deref, correcting earlier wording). Scene class identities remain unresolved (indirect via global 0x1DFE970).

## Scene factory named lookup (static)

Global `0x1DFE970` is a statically initialized `oRenderSystem` (vtable
`0x1C7870C`); slot `+0x3C` returns global `0x1DE05FC`, an `oD3DRenderAPI`
(ctor raw `0xF05FE0`, published at `0xED17DC`). Its slot `+0x30` (raw
`0xF255A0`) allocates a `0x18F48` scene and runs `0xF0FCD0(name)`, which
registers it by index in the 32-slot table at VA `0x1EDACD8`. No name-keyed
registry found; create-by-name reading is interpretation. Observed: scene
is `oD3DScene` (vtable `0x1C7A334`, base `oScene` `0x1C77A94`); the name is
copied to `oScene` shared text at `obj+8` by `0xE11E40` (no compare/hash in
the ctor); full table logs "Too many scenes were allocated." but still
returns the object. `0xED196B` stores static `0x1E72DF8` into `0x1DE05FC`
after deleting the previous `0x1DE0600` object (teardown reading is
interpretation); `0xED1AC4` also matches a store pattern but is undecoded.
Details in
`docs\MatrixConnection.md`; constants in `SceneFactoryNativeLayout`, pinned by
`StartupBinaryEvidence`.
