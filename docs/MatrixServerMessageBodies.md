# Matrix server body index

All35 observed descriptor IDs34..68 have a **normal-buffer** C# decoder.
These are offline reconstructions, not original source or a live client.
Detailed native evidence and consumer limits are in [MatrixConnection.md](MatrixConnection.md).
All source files below are under `src\Startup\MatrixServerMessages\`.

The canonical compact header is included in `Decode` input and reported
`BytesConsumed`. Raw text remains bytes; padding, pointers and allocation
metadata are not serialized. Truncated/malformed inputs return explicit
outcomes without a success-shaped payload. Native alternate-header buffers
remain outside these APIs.

| ID(s) | C# API / file | Body projection |
|---|---|---|
| 34,36 | `MatrixServerMessages34And36Wire` | WORD-length blob/raw flag; announcement type/data raw texts |
| 35 | `MatrixMessage35Wire` | Raw QWORD and two WORD-length blobs |
| 37 | `MatrixEnterZoneWire` | EnterZone fields, optional metadata, clock fields and spectator byte |
| 38,39,62 | `MatrixScalarUpdateWire` | Raw16-byte block; raw byte; queue-position/length DWORDs |
| 40 | `MatrixExitZoneNativeLayout.Decode` | Header-only ExitZone |
| 41-43 | `MatrixServerMessages41To43Wire` | Scalar/optional records/blob; PvP queue records; DWORD/WORD |
| 44-47 | `MatrixServerMessages44To47Wire` | Found-match update; challenge join response; two invitation variants |
| 48-50 | `MatrixServerMessages48To50Wire` | Invitation cancel; response/status; challenge kicked |
| 51-55 | `MatrixServerMessages51To55Wire` | ID-only notifications; native24-byte record list; selected DWORD |
| 56-61 | `MatrixServerMessages56To61Wire` | ForceUnqueue; remaining buffer; clock; SuperPing; text lists; recursive WORD nodes |
| 63-68 | `MatrixServerMessages63To68Wire` | WORD samples; LFG match/leader updates; zone chat; metadata maps |

Code57 is a deliberate boundary exception: it consumes **all** bytes after
the header in the supplied normal buffer. Do not concatenate another packet
onto that input or treat its remainder as an independently delimited body.
Its original class is `SynchronizationRequest`. The separate
`MatrixSynchronizationRequest57NativeLayout` verifies a **post-transform**
minimum20-byte envelope: preceding payload followed by16-byte MD5, then
reads the payload's leading LE16 dispatch key. It does not invoke the
unknown native transform or dispatch payloads. Wire-body decoding and
consumer-envelope acceptance remain different boundaries.
`MatrixSynchronizationDispatchNativeLayout` separately decodes mode0's
two DWORDs after the leading WORD, before its key1 / second-value8 gates.
The second value is not a length. Nonzero mode performs registered-handler
hash-map lookup and callback, not proven queue insertion.

## Original incoming descriptor identities

All names below are original RTTI classes in namespace **`matrix_fury`**.
`MatrixServerMessageNativeTypes.cs` records the complete evidence addresses;
catalog entries expose `OriginalType` without changing actual `msg-N`
log names. Existing generic/diagnostic-derived payload names remain
compatible; notably59's SuperPing label is not its original SuperPong class.
Names establish message identity, not unknown field units or semantics.

| ID | Original class |
|---|---|
| 34 | SigscanData |
| 35 | WelcomeToTheMatrix |
| 36 | Announce |
| 37 | EnterZone |
| 38 | UpdateZoneTimeSync |
| 39 | HotfixLevelChanged |
| 40 | ExitZone |
| 41 | MatrixStatus |
| 42 | MatchQueueResponse |
| 43 | MatchQueueUpdate |
| 44 | FoundMatchUpdate |
| 45 | ChallengeJoinResponse |
| 46 | ChallengeInvitation |
| 47 | ChallengeInvitationSquadInfoAck |
| 48 | ChallengeInvitationCancel |
| 49 | ChallengeInvitationResponse |
| 50 | ChallengeKicked |
| 51 | ChallengeLeave |
| 52 | ChallengeRosterUpdate |
| 53 | ChallengeReadyCheck |
| 54 | ChallengeMatchParametersUpdate |
| 55 | ChallengeMatchStarting |
| 56 | ForceUnqueue |
| 57 | SynchronizationRequest |
| 58 | GamePaused |
| 59 | SuperPong |
| 60 | ServerProfiler_SendNames |
| 61 | ServerProfiler_SendFrame |
| 62 | ZoneQueueUpdate |
| 63 | DebugLagSampleSim |
| 64 | DebugLagSampleClient |
| 65 | LFGMatchFound |
| 66 | LFGLeaderChange |
| 67 | ReceiveEmergencyChat |
| 68 | UpdateDevZoneInfo |

Code34 appends to an embedded native slBlob. A nonzero completion flag
snapshots **all accumulated chunks** and clears its logical byte buffer.
`MatrixBufferSnapshotNativeLayout` projects this separately from wire decoding;
snapshot cache keys and native allocation ownership are not wire fields.
RTTI names code34 `matrix_fury::SigscanData`; its paired outgoing code32
`RequestSigscan` has a separate six-byte offline codec in
`MatrixRequestSigscan32Wire.cs`. The opaque data contents are not decoded
as signatures or executable scanning instructions.

Byte-chunked collections repeat after count255, including a terminal0 for
exact multiples. Most preserve order/duplicates. EnterZone and68's metadata
maps instead use unsigned-byte-key ordering and last-value-wins duplicates.
The recursive61 decoder uses an explicit stack; field semantics remain opaque.

Consumer projections have separate named layout files. In particular, LFG
mode+48/+50 state, active challenge mode+E4 state, MatrixConnection state+C,
and GameSocket state+108 are distinct domains. Their ID masks, gates and
runtime notifications must not be generalized across messages.

`MatrixServerProfilerNames60NativeLayout` separately projects persistent
updates to two full-WORD-key name maps. Unmentioned entries survive,
duplicates overwrite in order, and empty lists do not clear either map.
The handler's tick-minus5000 marker and global-presence gate are not wire
fields. The tables' separate name categories remain unidentified.
Frame61 proves their structural roles are root names and node names.
`MatrixServerProfilerFrame61NativeLayout` collects missing WORD IDs without
deduplication in root order / node preorder. Its strict unsigned5000-tick
throttle advances before optional callback lookup; a separate raw byte
gates opaque frame refresh only. The saved API does not send name requests
or interpret the eight node metrics.
The separate `NativeAlgorithms\NativeProfilerSamples` now expands the
eight WORDs into three raw WORD fields and five finite binary16-scaled
DWORDs, and projects inclusive/exclusive/root numeric samples plus
filled80-frame histories. Actual metric units remain unknown; nonfinite
native FP behavior is explicitly unsupported. No native GUI or rendering
is executed.
`NativeAlgorithms\NativeProfilerTree` separately preserves forward-only
structural merging, stale siblings, duplicate-ID objects on reorder,
and same-ID exclusive first children. It does not normalize the native
tree into a dictionary or claim to update histories/rendering.
Its optional `MergeWithSelection` saves exact-object visitation, first
kind/WORD-ID recovery, ancestor expansion and visible-row0 fallback.
`FindVisibleRow` is a generic expanded-child preorder projection;
E72480 does not allocate a handle. Frame consumer owner+0C is the selected
node pointer and its independent null-selection fallback is not refresh-gated.
`NativeAlgorithms\NativeProfilerFrameRefresh.Apply` separately couples
samples to tree histories. It preserves the native root-child **positional**
pairing (including stale-ID mismatch), deeper forward-ID search/missing
skips, bottom-up append order and partial writes before an explicit
unsupported nonfinite outcome. Merge structure before applying a frame.
`NativeAlgorithms\NativeProfilerDisplayNames.Resolve` saves raw display
text selection: nonempty snapshot cache overrides all kinds, root/node
maps use full WORD keys, known-empty names stay empty, and native labels
are `[Exclusive]`, `<Waiting For Server>` and `<Error>`.
`NativeAlgorithms\NativeProfilerNodeSnapshotCodec` is a **separate snapshot
format**, not message61: signed32 child count/subtrees, expansion byte,
80 packed26-byte physical samples, raw next-slot DWORD, NUL-ended captured
name. It omits live kind/ID and native sample padding; no whole-file
framing or live object activation is claimed.
`NativeAlgorithms\NativeProfilerNameRequestWire` saves outgoing
`ServerProfiler_RequestNames` variants: matrix30/channel1 and
firefall18/channel2 (the latter with raw LE64 routing prefix), followed
by root/node chunked WORD vectors. Concrete `tfServerProfiler` slot8
selection is saved separately. The frame consumer's virtual60 provider
binding is still unresolved; ABI/slot compatibility alone is not treated
as proof of that continuous call path.

The Startup checks cover all35 minimum bodies, strict prefixes, nonempty
payloads,255/256 boundaries,65535-byte blobs,600-level recursive trees,
and selected native behavior. Optional `--binary` checks compare supporting
instructions/literals/tables against either installed or original executable.
