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
