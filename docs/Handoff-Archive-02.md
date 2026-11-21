# Reverse-engineering handoff archive 02

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
