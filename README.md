# FirefallClient.exe reconstruction

This workspace contains **partial, evidence-based C# reconstructions** of selected Firefall client systems. They are not recovered original source code and do not form a replacement game client. Names and layouts are inferred; each subsystem documents the supporting binary offsets and unresolved details.

The analyzed PE identifies itself as `Firefall (v1.5.1962)` and its SDB build identifier as `prod-1962`. No game assets or binary copies are included here.

The [ongoing handoff](docs\Handoff.md) records the last verified work and starting points for a future session.

| Area | Files | Status |
| --- | --- | --- |
| SDB database envelope, decryption, decompression, metadata and value conversion | `src\Serialization\` | Usable parser/inspection CLI, bounds-checked raw rows, a partial row converter, and documented native result-holder/callback lifecycle; full native object conversion remains unresolved. |
| Container and text primitives | `src\Containers\` | Annotated reconstruction; see local README. |
| Hash-slot table | `src\Collections\` | Reconstructed layout, operations and focused checks. |
| NVIDIA NVAPI bootstrap | `src\Graphics\Nvapi\` | Reconstructed dynamic API resolution and lifecycle, not network sessions. |
| Character command catalog (169 classes) | `src\Characters\` | Native layout snapshots cover all catalog entries (some grouped under helper model names); selected wire bodies and remote-observer dispatch gates are independently reconstructed and checked. |
| Client app-mode startup dispatch | `src\Startup\` | RTTI-backed catalog of Login, World, and LookingGlass modes, Login request/callback attachments, shared WinHTTP method selection and completion delivery, the replay interface, and a bounded `tfWorld` snapshot. |
| Matrix connection and UDP transport | `src\Startup\`, `src\Startup\MatrixServerMessages\`, `src\Startup\MatrixClientMessages\` | Oracle tickets, bounded handshake/framing/receive-window models, all35 observed server normal-buffer bodies34-68, partial clocks/zone/queue/challenge/LFG consumers, and outgoing msg-24/ExitZoneAck encoding; not a live network client. |
| Generated transformation providers | `src\Startup\NativeAlgorithms\` | Default-header mode0 and explicitly captured mode1 forward/inverse byte images, RNG/discard replay, managed positive-length byte effects, and numerical arena/lifetime plans; no native allocation or generated-code execution. |
| UI Lua bytecode cache | `src\Scripting\LuaCache\` | Reconstructed path hash, timestamp invalidation, Lua 5.3 header and load/fallback behavior. |
| Synthetic SDB checks | `tests\Serialization\` | Standalone console checks requiring no external test packages. |
| Shared native byte hashing | `src\Shared\` | FNV32 with engine final mixing, reused by SDB name seeds and the chat-log checksum. |

The active IDA database was opened as a **single raw 32-bit file image at base zero**, so listed function addresses are file offsets, not normal PE virtual addresses. [PE section mapping](docs\BinaryLayout.md) explains how to translate absolute data references. The latest IDA survey reports 147,733 discovered functions and 144,627 strings; those counts represent its current analysis, **not** a claim that they are all reconstructed here. Raw-image mapping makes absolute data references and indirect jump targets unreliable: in particular, the field-type conversion switch at `0x1311100` decompiles through invalid targets unless its PE VAs are translated back to raw offsets. Verify uncertain behaviors against instructions or real data before extending the implementation.

[Process startup](docs\Startup.md) identifies the actual translated PE entry, CRT initialization path, and replay-mode behavior; the [`tfLogin` notes](docs\LoginMode.md) and [`tfWorld` notes](docs\WorldMode.md) document mode-specific behavior and evidence boundaries. [Remote-character dispatch notes](docs\RemoteCharacterDispatch.md) cover the selected observer gates and shopping-list builder. [Platform imports](docs\Platform.md) list the linked runtime and external components.

The [matrix connection notes](docs\MatrixConnection.md) follow selected-character
preparation through oracle tickets, the initial application packet, UDP fragment
encoding and retries, IOCP receive delivery, the bounded POKE/KISS handshake,
context-ready tick transitions, queued delivery to the matrix owner, decoded
server-message routing, bounded EnterZone wire decoding and its native state advance.
Opaque fields and incomplete protocol/state transitions remain explicitly marked.
The [server body index](docs\MatrixServerMessageBodies.md) maps all35 IDs to
their saved C# decoding APIs and summarizes shared collection rules and limits.

Open `Firefall.Reconstruction.slnx` or build it with `dotnet build Firefall.Reconstruction.slnx`. Run the self-contained checks with `dotnet run --project tests\Serialization\SerializationChecks.csproj`, `dotnet run --project src\Characters\Checks\Characters.Checks.csproj`, `dotnet run --project src\Startup\Checks\Startup.Checks.csproj`, `dotnet run --project src\Collections\Checks\CollectionsChecks.csproj -c Release`, and `dotnet run --project src\Scripting\LuaCache\Checks\LuaCache.Checks.csproj -c Release`. See each subsystem README for address mappings and confidence limits. Generated `bin` and `obj` folders are build outputs, not reverse-engineered source.
