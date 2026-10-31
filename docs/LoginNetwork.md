# Login / network notes (round 12)

## Observed
- `0x304F30` (thiscall, this=App) is the `firefall://` URI handler. Prefix cached from string `firefall://`. Non-matching URIs go to `0x306050` (looking-glass queue). Matching paths tested in order: `matches/`, `replays/`, `adventure/`, `devspectate/`, `devjoingame/`, `operator/` (remainder validated by `0x2FCD00`; empty -> log `URI missing operator host`; else stored at App+0x2900). Unmatched -> `0x3054F9`. Query `?k=v&k=v` parsed into a map first.
- User agent `firefall/%s (%s)` has one xref: `0xCBCA50` (Awesomium WebCore init), formatted with `prod-1962` (string at VA 0x1C15EAC) and `byte_1E4AC90`. Only the embedded browser is proven to use it.
- Oracle ticket: `0x494E30` (callers `0x494CA0` BeginConnection, `0x495460`) posts `%s/api/v1/oracle/ticket`; response parsed at `0x4956C0` (`matrix_url`, `ticket`). Login zone record JSON decoded at `0x635BE0`. Details: MatrixConnection.md.
- Session-ticket header names and error strings exist (X-*-Session-Ticket; hotfix/vtex/AssetStream dl.firefall.com URLs) but were not xref-traced.
- imports_query for winhttp/wininet/WSAStartup returned no hits (imports appear resolved via dword_1AC* tables; scope: import-name filter only).

## Interpreted
- Matrix transport is UDP/IOCP per MatrixConnection.md; the HTTP oracle request is the only HTTP step in login.

## Unresolved
- HTTP client implementation behind the oracle POST, hosts/ports/config keys beyond `matrix_url`, the Matrix->World transition beyond EnterZone (MatrixConnection.md), bodies of `0x3054F9` and `0x306050`.

# Round 13: networking layers

## Observed
- The import table is empty to `imports`/`imports_query` (all modules, 0 rows): imports are resolved at runtime (dword_1AC*/1C15E* pointers), so import-based searches cannot show networking. Strings do exist: `WSAStartup failed.`, `(Windows)WSAStartup failed with error!`, `WinHttpWriteData`, `WinHttpReceiveResponse`, `WinHttpAddRequestHeaders`, `WINHTTP_CALLBACK_STATUS_*`. I did not search for libcurl strings beyond `curl_easy|libcurl` (no hits in the first 20 matches, which were all WinHTTP/WSA; scope: that page only).
- `0xBBD110` (called from ClientApplicationInitialize at 0x307208 and from 0xE9BE03): ref-counted network-subsystem init (this+8 refcount). On first use it calls a resolved `WSAStartup(0x202)`-shaped pointer (dword_1AC4DA8(514,buf); `WSAStartup failed.` string 0x1BB4C70 is its error log), then a resolved call `(−1,0,0,1)` stored at this+0xDC (matches CreateIoCompletionPort; consistent with the IOCP receive dispatch in MatrixConnection.md), builds a 0x28-byte object (`0xBC4190`) at this+0xA0, derives a 32-bit id from a resolved pointer calls, and logs. Failure path runs `0xBBD570` with 3000.
- `ClientApi_DispatchWinHttpStatus` (0xBC1DA0) is the only xref of `WinHttpWriteData` string; its sole caller is `ClientApi_WinHttpStatusCallback` (0xBC26E0), which has no code xref (callback pointer registered indirectly). So WinHTTP is the ClientApi HTTP layer, distinct from the Matrix UDP transport.
- `0x494CA0` MatrixConnection_BeginConnection: if state==2 does nothing; state 6/7 builds a message-24 descriptor and sends via `0x498350`; otherwise runs `0x490050`, sets state=2, stores the text arg at this+2976 unless `0x2FCD00` says it is empty-ish, and calls `0x494E30`.
- `0x306050` AppMode_TryQueueLookingGlassRequest(this, text, int64* ids, flag): wraps text; if registry entry `replay_autoselect` exists text is replaced by `0x3813A0` result; if non-empty it fills a descriptor (id pair, flags 4 or 0x14, |8 when `replay_autocontrol` entry exists) and calls `AppMode_QueueLookingGlassMode` (0x30D590). Returns 0 on empty text.

## Interpreted
- Oracle ticket HTTP rides ClientApi/WinHTTP; Matrix game traffic rides WSA + IOCP UDP.

## Unresolved
- Registration of `0xBC26E0` as the WinHTTP callback; the headers set by `0x494E30` (not re-decompiled; see MatrixConnection.md); body of `0x3054F9`; libcurl absence beyond the scope above.

# Round 14: oracle request, WinHTTP callback registration, URI default branch

## Observed
- Strings in the earlier `asc_*` IDA labels were junk; decoding at VA-0x401400 gives: `clientapi_host` (0x1B520A8; host key looked up via `0x322CB0(App, this, key)`), `Keep-Alive`/`Connection`, `application/json`, `ver`, `%s/api/v1/oracle/ticket`, `Unable to resolve oracle URL %s`, `Oracle URL [%s] not configured for HTTPS (request must be secure)`, `Oracle auth not available (was cleared?)`, `Accept-Language`, `OS-Language`, `?`, `build`, `matrix`, `squad`, `platoon`, `group`, `Querying Oracle at '%s' with character 0x%016llx`.
- `0x494E30` requires state 2 and in-flight flag this+0x1C clear. Needs App+0xA28 (2600) object with a flag at +52 set (else text `Oracle auth not available` into this+16 and state=1). Resolves the oracle host from config key `clientapi_host`, formats `%s/api/v1/oracle/ticket` (a request object of 0x78 bytes from `0x3233B0`; URL accepted by `0xBCE800`, HTTPS required: on failure state=1 with an error text at this+16). It then sets method type 2 (POST), copies this+0xB98 auth blob (`0x498680` from this+2968), adds `Connection: Keep-Alive` style header and `application/json` content-type, a timeout from this+0x18 when >0, writes a JSON body beginning with `ver`, (snprintf at `dword_1AC4870` into a 127-byte buffer) then fields through `0x49B050` (character id/`build`, session text this+2976 when non-empty, and chat `squad`/`platoon`/`group` when a current chat exists), adds Accept-Language/OS-Language headers, then submits via `0x49AEF0`; success sets this+0x1C=1. No Matrix UDP host/port is chosen here: they come from the oracle response (`matrix_url`/`ticket`, `0x4956C0`).
- `0xBC2B30 ClientApi_PumpRequestWorker` contains at 0xBC31E0 `push 0; push -1; push 0xFC32E0; push [edi]; call [0x1AC4D0C]` - the callback VA 0xFC32E0 = `0xBC26E0` ClientApi_WinHttpStatusCallback, i.e. a runtime-resolved WinHttpSetStatusCallback(session, cb, ALL, 0) (name by shape; the pointer slot 0x1AC4D0C name was not read).
- `0x3054F9` is the default branch of the URI handler: path `untrusted/<uri>` prompts `'%s' is an untrusted location. Are you sure...` (MessageBox-shaped call, caption `Firefall Launcher`) before continuing; `socket/host?port=N` builds `sock://host[:port]` and queues it through `0x306050` (error `Missing socket address: %s:%u`); anything else logs `Cannot open unknown URI: %s`.

## Interpreted
- The oracle HTTP flow is HTTPS-only, POST JSON, host from `clientapi_host`; the Matrix UDP endpoint is decided only by the oracle reply.

## Unresolved
- Actual `clientapi_host` value (config source), the exact field order/body of the JSON, and the slot `0x1AC4D0C` import identity.

# Round 15: oracle reply, ClientApi headers, sock:// consumer

## Observed
- `0x495460 MatrixConnection_HandleOracleResponse(this, resp)`: clears in-flight this+0x1C; if `0x4956C0` (ParseOracleResponse) succeeds it frees the text at this+2976 and zeroes this+0x18 (retry delay). On failure while state==2 it grows the retry delay `d+2+(d+1)/2` capped at 300, logs, sends UI message 383 via `dword_1E002B8` vtable+0xAC and calls `0x494E30` again (retry); otherwise logs an unhandled-message id.
- `0x4956C0` (size 0x804) references these JSON keys: `matrix_url`, `ticket`, `datacenter`, `operator_override`, `session_id`, `hostname`, and error keys `status`, `message`, `invalid_response`. Strings: `Invalid decrypted oracle login ticket`, `Decode: invalid oracle login ticket` (ticket is decoded/decrypted client-side), `Matrix connection initiated to %s (%s)` / `Could not initiate Matrix connection to %s (%s)`, `invalid oracle response (STATUS=%s,HTTP=%d): '%s'`. Decompile of 0x4956C0 failed; keys are from data xrefs only, field order/semantics not read.
- `0x635E90` (ClientApi header builder, this = header map, uses `0x4969A0` find-or-insert + `0x2FCC90` assign): `X-Red5-Session-ID` (App+2616, when non-empty), `X-Red5-Hotfix-Level` (%u from [[App+3172]+8]+56), `X-Steam-Client-UserId` (%llu) and `X-Steam-Session-Ticket` when the Steam object `dword_1E46774` reports ready (vtable +12/+28/+36), else `X-Youxi-Session-Ticket` if `dword_1E4B2DC` text non-empty, else if `byte_1E4B2E1` `X-369-Session-Ticket` + `X-369-Session-Site` (`369 auth was not ready, http request will not support it.` when `byte_1E4B2E0` clear); `X-Red5-Signature2` from `0x30F660(App)`; `Accept-Language` from table `dword_1DE0868[min(dword_1E00100[1],5)]`; `X-Red5-Is-Censored` `1`/`0` from `dword_1E00100+19`; `X-Red5-SiloPoolId` (%u, App+2620). No secrets recorded.
- `sock://` consumer: `0x3820E0` is a URL-source classifier (this+8 type, this+12 stripped path). Prefixes `http://`(/`https://`) -> type 4 (whole URL kept), `live_file://` -> 2, `sock://` -> 5 (7 chars stripped), `file://` -> stripped; no prefix -> type 1, upgraded to 3 when a `.chunk_index` path passes the locale check. It is used by the chunk/file source layer, not by the Matrix UDP path; the `sock://%s` string built by `0x3054F9` (VA 0x1B4036C) is a different, separate string.

## Interpreted
- Oracle reply gives `matrix_url` (host:port) + encrypted `ticket`; the Matrix connect log line is emitted from inside `0x4956C0`. Header builder is the shared ClientApi/hotfix auth header set (Red5/Steam/Youxi/369 ticket variants). Prefix matching assumes `0x325730` is a prefix test.

## Unresolved
- Callers of `0x635E90` and the request queue struct layout; the JSON-to-field mapping in `0x4956C0`; who consumes type 5 from `0x3820E0`; actual host values.

# Round 16: oracle parser internals, header-builder callers, sock:// consumer

## Observed
- `0x4956C0` is chunked (IDA decompile fails; disasm of the first chunk was followed via call targets, jumps into other chunks were not all followed). Calls: JSON accessors for `matrix_url`/`ticket`/`datacenter`/`operator_override`/`session_id`/`hostname`; for the ticket text it builds a stream object (vtable VA 0x1C80CC4) and calls `0x12CEB00` (decompiled): a standard-alphabet **Base64 decoder** (A-Z a-z 0-9 + /, `=` padding) that writes 3-byte groups through stream vtable+0x18. It then checks the decoded buffer: length > 2 and bytes `'R'`(0x52) `'5'`(0x35) at 0x4959CE; on mismatch the `Invalid decrypted oracle login ticket` / `Decode: invalid oracle login ticket` paths are used. After the prefix check it stores two strings into App+0xA34 and a second App field (matrix_url / hostname-derived, exact roles not proven), then calls `0x495F20 MatrixConnection_OpenTransport(...)` (4 args) at 0x495A43; success logs `Matrix connection initiated to %s (%s)`, failure `Could not initiate Matrix connection ...`. Cleanup path sets this+0x0C = 1 (state 1) and clears this+0x820 (`0xFD2C90` release).
- `0x636550` is a thin wrapper around the ClientApi header builder `0x635E90`; callers: `0x3813A0` (replay-related), `0x388BD0` (hotfix downloader), `0x48E8B0`, `MatrixConnection_QueueOracleTicket` (0x495327), `0x4A4A70`, `0x4A4D50`, `Login_QueueLoginRequest` (0x63B65C), `Login_QueueCharacterRefreshRequest` (0x63EDC5), `0x7E3510`. Another wrapper at 0x3E05B0 has no code callers.
- `0x381A10` is a replay/chunk source constructor (this+0 text, +4 arg, +8 type, +12 stripped path, +0x18 sub-object `0x380DF0`; log `unk_1B45258`), then runs the classifier `0x3820E0` and `0x3823F0`. Callers: `LookingGlass_CreateReplayReader` (0x65D7BF), `0x3D60A0`, `0x6AE3E0`, `0x6AF120`. `0x3823F0` switches on type with `cmp eax,4; ja default` (0x382423) and a 5-entry jump table at VA 0x783348 (types 0,1 -> 0x382433, 2 -> 0x38249E, 3 -> 0x382545, 4 -> 0x3825DE).

## Interpreted
- The oracle ticket is Base64 text whose decoded payload starts with `R5`; `Decode`/`decrypted` wording suggests further decryption, but no cipher call was identified in the chunk read. `sock://` (type 5) is therefore not handled by a case in the `0x3823F0` switch (goes to its default); `sock://` is a replay/LookingGlass stream source, consistent with `0x306050` queueing it as a looking-glass request.

## Unresolved
- Cipher/key for the ticket (if any), exact meaning of the two stored strings, the `0x495F20` argument roles and UDP host:port parse, the default-case handler of `0x3823F0` and whether type 5 is consumed elsewhere; the request struct/queue layout (`0x3233B0` 0x78 bytes) was not decoded.

# Round 17: MatrixConnection_OpenTransport, ClientApi request wrapper, 0x3823F0 default

## Observed
- `0x495F20` `__thiscall(this, a2=&matrix_url text, a3=&alt host text, a4=&shared text/ticket holder, a5)`. Steps: if this+2140 set, computes security flags (this+2108; log `Matrix is using security flags %u`, 1 or 17 by `0x3265A0`); parses a2 with `0xBCBF90` (sscanf formats `[%256[^]]]:%d` for bracketed IPv6 and `%256[^:]:%d` for host:port; else `0xBCD4C0` plain host); needs `0xBCCA60` success and nonzero port, otherwise logs `Invalid matrix (invalid address) for '%s'` and returns 0. Stores text into this+2100 and this+2104 (a3, else a2). Creates a GameSocket (0xDF8 bytes, ctor `0xBD00B0`, registered by `0xBBD730` on `dword_1DFFF70`), stores at this+2080, owner back-pointer at socket+0x10C. Calls `MatrixGameSocket_OpenUdp` (0xBD0960) with the parsed address, optional 2nd (IPv6-sized 16-byte) address, and port. Failure: destroys socket, formats a message into this+16, sets this+12=1, returns 0. Success: this+12=3, clears this+32..0x820 buffers, this+2424=timestamp (`dword_1AC4D24(0)`, import call), then builds `MatrixInitialPacket_Ctor` (0x338250; base ctor `0x33E990` type 17; fields: +40 security flags byte, +44 constant 1962 (a3 arg; likely protocol/build version - interpretation), +48 text (ticket), +56/60 8 bytes from this+2120, +64..72 12 bytes from this+2408, +77 byte, +80 text, +88 optional string, +112 stream holding data copied from stream arg) and `MatrixInitialPacket_EncodeAndSubmit` (0x4980A0), returns 1.
- `0x636550` (this=request): calls header builder `0x635E90(this+8)` then `0xBC1990(this, App+2600)` (App+2600 asserted non-null; it is the ClientApi worker/queue object). `0xBC1990` checks the request (WinHTTP-style handle via import thunks), builds a hashed key/signature (`0xBC1460`, `0x12E4100`) from fields this+16 (url), this+24, this+44 vtable slot+12 and stores the result in the map at this+32 keyed by a short string (`byte_1BB5328`); returns 1.
- `0x3823F0` default (type > 4, incl. type 5 `sock://`): VA 0x78331A jumps to an assertion call `dword_1C15EA0` with line 0x15C (348) then returns - no handler.

## Interpreted
- `sock://` is not consumed by the replay-source switch; it is rejected by assert there. Handshake first packet is type 17 `MatrixInitialPacket` carrying ticket text, flags, and client identity blobs, sent over UDP (consistent with docs\MatrixConnection.md).

## Unresolved
- Wire layout of the encoded packet (`0x4980A0`), meanings of this+2120/2408 blobs, 1962 constant, request-struct field map and queue storage inside `0xBC2B30`, and where type 5 is consumed outside this switch.
