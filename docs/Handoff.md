# Reverse-engineering handoff

## Current dispatcher/parser suffix review

New reviewed coverage now includes:

- **Dispatcher**: fallback diagnostic preparation (`BBDE1A..BBDE2A`), direct-call
  return bound at `BBDE2B`, and call-target arithmetic prefix
  (`BBDE2B..BBDE3D`, stop before `BBDE3E` JZ), plus both immediate
  branch projections:
  - nonzero path `BBDE40..BBDE49` (stop before `BBDE4B` JNZ),
  - zero path `BBDE3E` taken to `BBDE83` (stop before `BBDE86` indirect CALL),
  - nonzero-path opaque call boundaries at `BBDE5D` and `BBDE6C` with explicit
    normal-return stops at `BBDE63` / `BBDE6F`,
  - zero-path post-call window `BBDE8C..BBDE96` (stop before `BBDE97` JZ).
- **Parser first-branch**: FS restore (`BD34E2..BD34EC`), register POP chain
  (`BD34ED..BD34F0`), and frame/RET8 suffix (`BD34F1..BD34F6`).
- **Binary anchors**: independent first-branch chain anchors added to
  `StartupBinaryEvidence` for the three windows above plus one contiguous
  `BD34E2..BD34F6` assertion.

The latest full checks-project build (run 4748) succeeded with zero errors and
nine nullable warnings. This is compile evidence only, not focused runtime
acceptance.

The current focused dispatcher run now rebuilds and executes from the matching
focused output path and reports `Failures: 0` for the call-target prefix,
nonzero branch (including JB), zero branch, and dispatcher binary anchors.
Earlier stale-binary reports remain historical only and should not be used as
evidence for newer source revisions.

The current dispatcher source extends through the nonzero JB at `BBDE81` and
the zero-path `BBDE97` fallthrough window, with focused runtime acceptance now
recorded by the fresh focused run above.

Normal second-call parser suffixes now complete in focused execution with
`Failures: 0`, but they remain out of `Program.cs` registration until those
untracked production/check pairs are explicitly accepted and staged:

- `SecondVirtualCallReturn`
- `SecondCallFsRestore`
- `SecondCallRegisterPops`
- `SecondCallFrameReset`
- `SecondCallReturn`

Second-branch focused runtime now completes conclusive fresh focused runs with
`Failures: 0` for FS restore, register pops, and frame return. Second-call
focused runtime also now completes conclusive fresh runs with `Failures: 0`
across virtual-call return, FS restore, register pops, frame reset, and return.

Previously assigned GPT-6 Luna work covered the `BBDE3E` branch boundary:

- **nonzero JZ-not-taken path** from `BBDE40` (agent `93b4088b`);
- **zero JZ-taken path** from `BBDE83` (agent `26355611`).

ClientApi/WinHTTP details are in [ClientApiWinHttp.md](ClientApiWinHttp.md).

## Latest continuation navigation (November 4)

The [verified import and receiver mappings](Handoff-Archive-05.md#november-4-continuation-verified-import-and-receiver-mappings)
and subsequent sections supersede earlier dynamic-import assumptions,
PrivateDialog receiver identification, AES "block-size" interpretation,
and tracker `+0x20` uncertainty. Active-sample identity and raw/VA mapping
are recorded there; do not merge evidence from `.original` without comparison.

Application shutdown's service-cell dispatch now reaches the concrete console
file-close path. Current continuation is the UI shader-source `slBlob`
lifecycle: read gates/tails, capacity reuse, preservation copy, pooled/nonpool
allocation and release, and wrapper copy/destruction have captured code and
immutable PE anchors. See the latest appended sections for corrections and
active assignments. Singleton teardown and source-initialization copy are
complete; current active scopes are tagged-pool replenishment and adjacent
drain identity. Callback publication and composite container semantic identity remain
parked. UI lifecycle, arithmetic/import regressions, and immutable backing-slot
projection code/checks are committed. Tagged-pool replenishment now also has a composed projection
through its initial loop gate and at most one entered iteration's repeat
decision; it reuses the bounded stage projections and does not model a whole
loop. Its focused checks run with
`dotnet run --project src\Startup\Checks\Startup.Checks.csproj -- --shader-pool-replenishment`;
the epilogue bytes at raw `0x12CE1F5` are additionally pinned in
`StartupBinaryEvidence`. No runtime behavior, allocator/push effects, later
iterations, or termination are established. The active handoff remains static
and IDA-backed, not REA Evidence: the REA IDA provider is still
unavailable/unconfigured, and no configuration was changed.
The separate helper at raw `0x12CC2B0` now has a distinct projection and full
`0x31`-byte sample anchor; its owner/vtable identity remains unresolved, and
the prior bounded scans found no direct pointer/caller binding. Do not broaden
that search without new evidence. A temporary REA 6.9.0 native-provider probe
reached the active IDA MCP but failed result normalization (`asm.lines` shape
and missing `ok` field); its partial Evidence is not complete. Direct IDA
disassembly and byte reads corroborate the helper. The resident REA MCP still
reports the IDA provider as unconfigured; no persistent REA registration was
changed.
The traceback fallback's state-0 unwind map is decoded and has no restore action.
VM-frame insertion/removal is a bounded unresolved stop, not an ongoing scan.
Normal native CRT return selects `exit(status)`.
Exit registration and its logger cleanup predicate are resolved.
Generic wrapper field `+8` is a duplicated diagnostic label:
MSVCR120.dll!_strdup/free; the earlier constructor candidate is another destructor.
Logger unwind binding is now composed
from the recorded guard initialization and exact EH action. Assertion interception is a bounded
unresolved stop. Mode-16 fixed scatter and normal
buffer release are resolved.
Shutdown-request callers
and texture writer dispatch/serialization are bounded unresolved stops,
not ongoing broad scans.
Descriptor `+0x24/+0x28` are structure pointers, both nonnull on normal
startup, not boolean configuration fields.
Replay timing callback `0x65C1F0` is bound through the object's vtable `+8`.
App `+0xB78` is a one-iteration timing override, not a stop flag.
The worker's queue-work guard and normal null write-prefix override are resolved.
The normal application startup descriptor explicitly enables shutdown autosave;
constructor allocation/readiness and conditional shutdown-export trigger are resolved.
Local bind port and remote-peer
endpoint are distinguished; SavedLanguage qualifies for settings.con export,
but automatic persistence remains conditional on the documented shutdown path.
Endpoint matching and language selector default/setter/availability are resolved. Reader nonnull `+4`
attachment and localization row ordering are unresolved bounded stops,
not ongoing broad scans. Reset sample
identity is confirmed and the exact triples clear before holder destruction.
Publication owner identity/consumers remain unresolved; the constructor's reciprocal link
is distinct. Arming
`0xBDFD80` forwards no request bytes to its concrete diagnostic-only target;
the loader's success/reset callback order is resolved.
Case-0 destination replacement, borrowed metadata ownership,
outer discriminator framing, type-1 byte copying, metadata-binding hashes,
and localization missing-ID/language fallback are resolved. ManagedUDP packet byte snapshots and provider identity
`tfGlobalizationManager` are resolved. Inspected closing-state maintenance
has no independent timeout; sentinel producers, localization accessor,
field assignment, and service allocation are resolved in bounded scope.
IOCP stop observation,
keyed-close ownership, and extension format-operand mapping are now
resolved in the later sections. Completed traces distinguish GUI logout event 83
from application HTTP drain, readiness from manager admission, and returned
VM-stack cleanup from the earlier partial-parse callable-selection effect.
All remain static evidence, not observed runtime behavior.

## Archived research notes

Detailed historical investigation notes are split by original section order:

- [Handoff archive 01](Handoff-Archive-01.md) (60 KB)
- [Handoff archive 02](Handoff-Archive-02.md) (44 KB)
- [Handoff archive 03](Handoff-Archive-03.md) (60 KB)
- [Handoff archive 04](Handoff-Archive-04.md) (61 KB)
- [Handoff archive 05](Handoff-Archive-05.md) (61 KB)
- [Handoff archive 06](Handoff-Archive-06.md) (60 KB)
- [Handoff archive 07](Handoff-Archive-07.md) (62 KB)
- [Handoff archive 08](Handoff-Archive-08.md) (62 KB)
- [Handoff archive 09](Handoff-Archive-09.md) (62 KB)
- [Handoff archive 10](Handoff-Archive-10.md) (61 KB)
- [Handoff archive 11](Handoff-Archive-11.md) (38 KB)
- [Handoff archive 12](Handoff-Archive-12.md) (62 KB)
- [Handoff archive 13](Handoff-Archive-13.md) (46 KB)
- [Handoff archive 14](Handoff-Archive-14.md) (45 KB)
- [Handoff archive 15](Handoff-Archive-15.md) (55 KB)
