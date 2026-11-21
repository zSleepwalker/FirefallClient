# Reverse-engineering handoff archive 03

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
  socket state. `0xBD3140` now records the chunk header, sequence, shared
  body slice and time sample; lengths2/3 pass the header gate despite an
  incomplete sequence prefix. This is a declared-chunk overread observation,
  not proof of an allocation-boundary fault.
- **Control routing and the transport Connected gate are established.**
  Logged native state IDs0..9 are cataloged in
  `GameSocketStateAndControlCatalog.cs`. Translated `0xBD3510`/`0xBD3760`
  switches separate immediate keepalive/clock/close controls from deferred
  ACKs for queues1/2. A deferred clock reply while socket state3 and
  clock-object sample count>=2 calls `0xBD1B70` to set5/Connected.
  `0xBD3E50` now has channel0..3 dispatch and token1..3 provenance recorded
  in `MatrixConnection.md`: close completion queues1, completion errors and
  failed binding queue2, and successful binding queues3. The exact chunk
  record offsets and short sequence-prefix boundary case are also recorded.
  `GameSocketChunkHeader.cs` remains a header-only model; full reassembly,
  reliable-window policy, and callback lifetime are not reconstructed.
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
  object+20 channel/+24 size, queues+60, then advances/recycles fragment
  and sets ACK-pending+32. The `socket+120` limit is checked on continuation
  only; the final append has no visible same check. Codes3/4 propagate
  through BD5080 to pump-close. The ring/window model remains non-mutating.
  Constructor BE3CE0 zeros the queue tag/decoder fields; MatrixConnection
  passes its codec pair at+824 to BD0960, which installs pairs in queue+10/+14
  for channel1 and channels2/3 (3 shares2). Decoder ABI and selective ACK
  details remain partial.
- **Decoder addresses are now proven from constructor stores.** Native48F12C
  installs owner+828 VA73EEE0(raw33E2E0), tag+824=B968;48F14E installs
  owner+830 VAC227A0(raw821BA0), tag+82C=4C5F. Socket open copies pair1 to
  channel1, pair2 to2/3 and tag WORDs to socket+30/+32 (POKE/KISS). Channel1
  decoder calls34A020 then33D4B0. Its decompiler reports bad SP: verify
  arguments/instruction framing before reconstructing wire fields.
- **Shared channel2/3 dispatch is now mapped.** `0x821BA0` reads a raw QWORD
  and compact positive16 code; codes1..6/9 reach factory `0xBC9B80`, code8
  reaches `0xBC9DB0`, and code7/out-of-range reaches shared secondary-dispatch
  block `0x821D7C` then `0x81CC50`. The three route targets and byte-map/table
  addresses are in MatrixConnection.md. The inner `0x81CC50` tables are
  structurally mapped with a leading-byte decrement (accepted inputs1..0x36
  map to indices0..0x35);
  selector0x53 builds `MarketRequestComplete`, and selector0x54 builds
  `ReceiveWeaponTweaks` with two raw bytes followed by a remaining-stream
  blob. Selector0x55's `TookDebugWeaponHitPublic` reader has a verified
  39-byte scalar prefix and two conditional tails, including a count-blocked
  vector of 64-byte records. The copied third DWORD at object+0x70 has no
  observed input write in the reader path; its origin remains unresolved.
  Selector0x56 constructs the distinct RTTI class `TookDebugWeaponHit` and
  shares the same `0x5F60F0` body reader; this does not prove semantic
  equivalence.
  Selector0x57 constructs `DebugWeaponStats` and reads eight raw WORDs at
  object offsets+0x28 through+0x36; their meanings are unknown.
  Selector0x58 constructs `RewardInfo` and reads two bytes, one QWORD, and
  three DWORDs at object offsets+0x28..+0x40; meanings remain unknown.
  Selector0x59 constructs `ProgressionXpRefresh` and reads count-blocked
  records of five raw DWORDs (20 bytes) into the vector at object+0x28; field
  meanings remain unknown.
  Selector0x5A constructs `ReceivedDeferredXP` and reads count-blocked
  records of four raw DWORDs (16 bytes) into its vector; meanings remain
  unknown.
  Selector0x5B constructs `PublicCombatLog` and accepts raw DWORD gate1
  followed by a WORD-length-prefixed blob; the non-1 diagnostic path remains
  unresolved.
  Selector0x5C constructs distinct RTTI `PrivateCombatLog` and branches to
  the same reader with `this = object+0x28`; parser reuse does not prove
  semantic equivalence with selector0x5B.
  Selector0x5D constructs `AnimationUpdated` and reads a WORD plus a byte at
  object+0x28/+0x2A; meanings remain unknown.
  Selector0x5E constructs `RaiaNPCDebugging` and reads a NUL-terminated
  shared-text field at object+0x28 in 256-byte chunks; encoding and text
  meaning remain unknown.
  Selector0x5F constructs `WeaponProjectileFired`, reads four WORDs at
  object+0x28..+0x2E, then consumes a gate byte and an optional three-WORD
  tail at object+0x30..+0x36; field meanings remain unknown.
  Selector0x60 constructs `AbilityProjectileFired`, reads its fixed scalar
  fields through object+0x50, then consumes a gate byte and optional QWORD
  at +0x60 with presence at +0x58; meanings remain unknown.
  Selector0x61 constructs `ProjectileHitReported` and reads WORDs at
  object+0x28/+0x2A then bytes at +0x2C/+0x2D; meanings remain unknown.
  Selector0x62 constructs `Stumble` and reads WORDs at object+0x28/+0x2A
  and a byte at +0x2C; meanings remain unknown.
  Selector0x63 constructs `QuickChat` and reads a DWORD at object+0x28
  through shared continuation 0x8206E3; allocation-failure behavior remains
  unresolved.
  Selector0x64 constructs `ProximityTextChat`, reads NUL-terminated shared
  text at +0x28 and bytes at +0x2C..+0x2E, then consumes an optional
  WORD-length-prefixed blob behind a gate byte at +0x30; meanings unknown.
  Selector0x65 constructs `JumpActioned` and reads a WORD at object+0x28
  through shared continuation 0x81D21F; allocation-failure behavior remains
  unresolved.
  Selector0x66 constructs `JumpRolled` and shares that WORD continuation;
  its allocation-failure path also zeroes ESI before reading.
  Selector0x67 constructs `Respawned` and reads WORD +0x28 plus bytes +0x2A/+0x2B; allocation failure zeroes ESI before reading.
  Selector0x68 constructs `CalledForHelp` and shares the same WORD continuation; allocation-failure behavior remains unresolved.
  Selector0x69 constructs `TookHit` with a gated record at +0x28, bytes +0x50/+0x51, WORD +0x58, and byte +0x5A.
  Selector0x6A constructs `AlmostHit` and reads one QWORD at +0x28; allocation-failure behavior remains unresolved.
  Selector0x6B constructs `DealtHit` with the gated record at +0x28 and bytes +0x50/+0x51; allocation-failure behavior remains unresolved.
  Selector0x6C constructs `Killed` and reads WORD +0x28, QWORD +0x30, and bytes +0x38..+0x3A; allocation-failure behavior remains unresolved.
  Selector0x6D constructs `WarnLockTargeted`; the case has no body-reader call and allocation failure takes the zero-return switch epilogue.
  Selector0x6E constructs `CurrentPoseUpdate` and reads variable scalar groups controlled by masks in byte +0x28; field meanings remain unresolved.
  Selector0x6F constructs `ConfirmedPoseUpdate` and reads a conditional nested record at +0x28 plus WORD +0xB8; nested semantics remain unresolved.
  Selector0x70 parses seven 0xC8-stride records; receiver slot +0x144 reads DWORDs +0x28/+0x2C and probes +0x30 as a vtable, dispatching numeric code363.
  Selector0x71 parses a 14-case nested state; receiver slot +0x148 passes its zero-initialized +0x28 DWORD to a vtable helper and dispatches numeric code365.
  Selector0x72 reads DWORD +0x28/WORD +0x2C; receiver slot +0x14C treats +0x28 as a list pointer and updates owner state at +0x43C8; no event dispatch observed.
  Selector0x73 parses the same 14-case nested state; receiver slot +0x150 treats +0x2C as a list pointer, uses zero +0x28 as state, and dispatches numeric code213. Mismatches and unknown consequences are documented.
  Selector0x74 constructs `AbilityActivated`, reads DWORDs +0x28/+0x2C, and
  copies remaining input into an `slBlob` at +0x30 through virtual +0x10.
  Selector0x75 has the same parser shape; its receiver passes addresses
  +0x28/+0x30 to a helper that treats the latter field as a tree-header
  pointer, conflicting with its initialized slBlob vtable. No intervening
  conversion is known; runtime consequences and field meanings remain unknown.
  Selector0x76 copies remaining input into an `slBlob` at +0x28, but its
  receiver interprets +0x28/+0x2C as begin/end pointers to 0x40-byte records.
  This parser/consumer representation mismatch is confirmed statically; any
  intervening mutation and runtime consequence remain unknown. The records'
  semantics and cooldown timing are also unresolved.
  In the parser-message path, selector `0x77` constructs `NPCInteraction` and
  reads one raw QWORD at +0x28;
  its receiver uses it as a two-DWORD map key and dispatches numeric UI code
  356, with no event name assigned. The parser requests 0x30 bytes and writes
  only through exclusive +0x30, while the unchanged object pointer reaches a
  consumer that swaps 0x20 bytes at [+0x30,+0x50). Its vtable reader/writer
  also touch only +0x28; no alternative direct constructor/callsite or body
  transform explains the consumer state. This parser-message consumer is not
  evidence that the keyed selector-0x02/0x03 Character dispatchers reuse that
  method. An untraced indirect entry is not excluded. Actual block extent and
  runtime consequences remain unknown.
  Route0 Character selector0x78 constructs `OpenMovieDialog` and passes input
  through the shared NUL-terminated text reader at +0x28; field meaning and
  allocation-failure consequence remain unknown.
  Route0 Character selector0x79 constructs `PrivateDialog` and reads DWORD
  +0x28, QWORD +0x30, and DWORD +0x38; this ends at +0x3C despite a +0x40
  request, while the same parsed pointer reaches event-229 handling that reads
  through +0x48. The actual allocator block extent remains unresolved.
  Route0 Character selector0x7A constructs `PublicDialog` and reads DWORDs
  +0x28/+0x2C; field meanings and allocation-failure consequence remain
  unknown.
  The Route0 Character nested selector table 0x53..0x7A is fully traced, and
  all 17 case-bearing primary-route subcases have native layouts. Route3
  entries2,3,4,5,7,8 are
  mapped in `docs/MatrixConnection.md`; next trace parsed-object consumers.
  Field semantics remain open.
  RTTI names the results `nl::RoutedMessage` and
  `nl::RoutedMultipleMessage`; QWORD and message semantics remain unknown.
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
  instead reads unrelated resource bytes. The runtime class is now identified
  as `uiManager`; the message-89 subscriber and connection to a successful
  Login-to-World transition remain unresolved.
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
  (`0x30DEB0`). A separate Login-mode routine at raw `0x63C1E2` posts ID
  `0x145` through the shared `uiManager` endpoint; it is not the
  `Login_HandleLoginResponse` callback at `0x63B7C0`. These establish a World
  queue path and a Login-mode event dispatch, but not that this event causes
  that update path or that successful login reaches its queue condition. The
  selected pair is not labeled a GUID and the complete successful transition
  remains unresolved. Details and layout constants are in `docs\LoginMode.md`
  and `src\Startup\`.
- **tfLogin `0x638B89` guard (static).** Observed: the default switch case
  (`0x638AB5`, `mov [esi+1Ch],0`) loads the pointer cell at VA `0x1E5CAA4`
  and tests byte `[pointee+0x40]` (`0x638ABC..0x638AC6`); zero jumps to
  `0x638B89` (`{2,0}` to
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
