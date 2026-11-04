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
- `ClientApi_PumpRequestWorker` (`0xBC2B30`) registers `ClientApi_WinHttpStatusCallback` (`0xBC26E0`) with `WinHttpSetStatusCallback(session, callback, 0xFFFFFFFF, 0)` at `0xBC31E9`. The original sample's IAT slot `0x1AC4D0C` resolves to `WINHTTP.dll!WinHttpSetStatusCallback`. It then passes the 0x1C-byte worker context as `dwContext` to `WinHttpSendRequest` at `0xBC31F8/0xBC3213`; IAT slot `0x1AC4D10` resolves to `WINHTTP.dll!WinHttpSendRequest`.
- `ClientApi_WinHttpStatusCallback` (`0xBC26E0`) asserts the callback handle matches the handle stored in its context. Status `0x800` (`WINHTTP_CALLBACK_STATUS_HANDLE_CLOSING`) sets the response-kind field at context `+0x20` to 3 only if it was unset, clears the context handle, and conditionally releases/clears context `+0x18` when context byte `+0x11` is set. Other statuses are dispatched only while context `+0x14` is zero. A nonzero dispatcher result is stored there if still empty, sets response-kind `+0x20` to 4 only if unset, and closes the WinHTTP handle via `WinHttpCloseHandle`. Remaining status handling and response-kind meanings are still incomplete.
- `ClientApi_DispatchWinHttpStatus` (`0xBC1DA0`) handles `WRITE_COMPLETE` (`0x100000`) by advancing its upload count and request progress, then calling `WinHttpReceiveResponse` once the upload is complete. `HEADERS_AVAILABLE` (`0x20000`) queries the numeric HTTP status and raw headers, inserts parsed header pairs into response `+0x10`, then calls `WinHttpQueryDataAvailable`. `DATA_AVAILABLE` (`0x40000`) reserves response-body capacity and starts `WinHttpReadData`; `READ_COMPLETE` (`0x80000`) appends received bytes to response text `+0x1C` and queries for more data, or marks observed response kind 1 and terminates on zero bytes. `REQUEST_ERROR` (`0x200000`) is an eight-byte async result; `SECURE_FAILURE` (`0x10000`) reports the security flags, and `REDIRECT` (`0x4000`) has a separate path. These status values are modeled as callback notifications, not a complete response-kind enum.
- The request URL parser stores scheme, host, port, path, and query at request offsets `+0x0C`, `+0x10`, `+0x14`, `+0x18`, and `+0x1C`; it lowercases the stored host through imported `tolower`. On `REDIRECT` (`0x4000`), the dispatcher cracks the new URL but compares only its host text with the stored host using imported `_stricmp` (IAT slot `0x1AC48A8`). Thus scheme- or port-only changes do not trigger this host-change condition. When the request header count at `+0x24` is nonzero, the callback's one-shot replay flag is clear, and the host comparison is unequal, it traverses the stored headers and calls `WinHttpAddRequestHeaders` with `0x80000000` (`WINHTTP_ADDREQ_FLAG_REPLACE`), then sets context byte `+0x10`.
- `ClientApiResponse_Ctor` (`0x3845D0`) initializes a 0x20-byte response object through `ClientApiResponse_Reset` (`0xBC11D0`). Reset sets `+4=0`, `+8=1`, and `+0xC=0`, initializes the tree sentinel at `+0x10`, clears `+0x14`, and empties shared-text slots at `+0x18/+0x1C`. The destructor `0x3844B0` releases those shared-text slots and destroys the tree through `0x326EF0`; status processing uses the tree at `+0x10` for response headers. `ClientApi_PumpRequestWorker` later sets `+4` to its request pointer and `+8=0`; the callback uses the same `+8` slot for observed values 3 and 4. These state numbers are recorded without assigning broader protocol meaning.
- `ClientApi_PumpRequestWorker` allocates a separate 0x1C-byte WinHTTP callback context, zeroes it, stores the response pointer at `+0x18`, later stores the request handle at `+0`, and passes the context as `dwContext` to `WinHttpSendRequest`. The status callback reads the result/dispatch gate at `+0x14` and tests byte `+0x11` before extra handle-closing cleanup. That flag is initialized to zero; its nonzero source has not been identified.
- `0x3054F9` is the default branch of the URI handler: path `untrusted/<uri>` prompts `'%s' is an untrusted location. Are you sure...` (MessageBox-shaped call, caption `Firefall Launcher`) before continuing; `socket/host?port=N` builds `sock://host[:port]` and queues it through `0x306050` (error `Missing socket address: %s:%u`); anything else logs `Cannot open unknown URI: %s`.

## Interpreted
- The oracle HTTP flow is HTTPS-only, POST JSON, host from `clientapi_host`; the Matrix UDP endpoint is decided only by the oracle reply.

## Unresolved
- Actual `clientapi_host` value (config source) and exact field order/body of the JSON.

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
- `0x636550` (this=request): calls header builder `0x635E90(this+8)` then `0xBC1990(this, App+2600)` (App+2600 asserted non-null; it is the ClientApi worker/queue object). `0xBC1990` checks the request (WinHTTP-style handle via import thunks), calls URL lookup `0xBC1460`, passes its adjusted timestamp to builder `0x12E4100`, and stores the result in the map at this+32 under the literal key `"X-Red5-Signature"` (VA `0x1BB5328`); returns 1. In `ClientApi_PumpRequestWorker`, the request's name/value entries are formatted as `"%s: %s\r\n"` and passed to `WinHttpAddRequestHeaders`, tying this map entry to the native request-header path. The worker separately submits a `Content-Type` header. `0xBC1460` first builds the lowercase `host:port` key through local helper `0xBC1040` (format `"%s:%d"` at VA `0x1C73534`) and performs an ordered-tree lookup through `0xBC4D60`/`0xBC61C0`. If that misses, it scans host substrings beginning at each `.` from left to right and retries only for HTTP port 80 or HTTPS port 443; each candidate is ASCII-lowercased, so `www.Example.COM` yields `.example.com`, then `.com`. A matching entry supplies its learned server clock offset at `+0x14`; no match leaves the input timestamp unchanged. The global tree is protected through lock import slots and a reference/count field at global `+0x38`.
- `0xBC1680`, called by `ClientApi_PumpRequestWorker` at `0xBC338D`, learns and updates the same per-host clock-offset tree. The worker looks up the HTTP response header named `"date"` and passes its value to parser `0xBCB5A0`. That parser uses `sscanf` format `"%3s, %d %3s %d %d:%d:%d %3s"`, compares month abbreviations with `_strnicmp`, and converts the parsed UTC fields with `_mkgmtime64`. The worker subtracts current `_time64()` from the parsed server timestamp and supplies that difference to `0xBC1680`. The updater stores/replaces that offset in seconds under lowercase `host:port`; for default HTTP/HTTPS ports it also creates or updates dot-prefixed suffix keys with the same offset. `0xBC4CB0` returns the value slot for an existing key or inserts a new node; `0xBC7A10` performs ordered-tree insertion and balancing. Tree node key/value fields are at `+0x10/+0x14`. Sample-mapped strings, import slots, and call sites confirm the clock-offset path. `CaptureClientApiServerClockOffsetEntries` and `CaptureClientApiAdjustedTimestamp` model these operations; application policy remains unknown.
- Builder `0x12E4100` uses the adjusted timestamp from `0xBC1460` as the `tc` field in an inner query string. The PE-mapped VA `0x1C57568` is `ver=%d&tc=%u&nonce=%04x%04x%04x%04x&uid=%s&host=%s&path=%s&hbody=%s&cid=%llu`: version is constant `2`; `tc` receives the adjusted timestamp; four calls to the imported `rand()` supply 16-bit nonce words rendered as four-digit lowercase hex; the named string fields are inserted in `uid`, `host`, `path`, and `hbody` order; and `cid` receives a 64-bit unsigned value from two adjacent object words. The builder computes two optional HMAC-SHA1 values over this same query using distinct object-held keys; each 20-byte digest is rendered as 40 lowercase hex digits. It places these as the first and third fields of a `"Red5 %s %s %s"` wrapper at VA `0x1C576D0`, with the query in the middle. `CaptureClientApiRequestQuery` and `CaptureClientApiRequestSignature` model query formatting and the byte-level signature operation. The native text encoding and semantic roles of some input fields remain unresolved.
- Auto-login setup `Login_StartAutoLogin` at `0x63DE80` calls `0x12E3960` with a non-empty input string on its accepted branches. `0x12E3960` retains that string in fields at `+0x3C` and `+0x48` and clears `+0x40`; these offsets align with the first HMAC key, `uid`, and optional second HMAC key consumed by `0x12E4100`, although the cross-path object identity is not yet proven. The initializer also hashes two distinct suffixed forms of its input with CryptoAPI SHA-1, imports the second digest's first 16 bytes as a `PLAINTEXTKEYBLOB` AES-128 key (`0x660E`), and sets the first digest as `KP_IV` (parameter 1). The embedded suffix values are not copied into the reconstruction. The downstream encryption operation and the precise lifetime/ownership of this key object remain unresolved.
- Cipher helper `0x12E4680` uses the key object at `this+0x58` and delegates buffer decryption to `0x12EF0D0`, which calls imported `CryptDecrypt`; it conditionally resizes the stream buffer based on the configured block-size field. `0x12EF1E0` contains a corresponding `CryptEncrypt` wrapper, but no direct code xrefs to it were found. Call-through dispatch and the AES mode/consumer remain unresolved.
- Request timing fields use the imported `timeGetTime` at four observed points: reset (`ClientApiRequest_Reset`, `0xBC1280`) stores the tick at request `+0x68` and tick-minus-one at `+0x6C/+0x70/+0x74`; request tracking (`0xBC43C0`) stores a fresh tick at `+0x6C` after linking the request; `ClientApi_PumpRequestWorker` (`0xBC2B30`) stores a tick at `+0x70` after its WinHTTP-open-request branch; and its response-completion path stores a tick at `+0x74` immediately before callback acquisition/dispatch. These are millisecond tick samples, distinct from the `_time64` seconds timestamp used for the `tc` query field. The meanings of the four marks beyond their observed lifecycle positions remain unproven.
- `0x3823F0` default (type > 4, incl. type 5 `sock://`): VA 0x78331A jumps to an assertion call `dword_1C15EA0` with line 0x15C (348) then returns - no handler.

## Interpreted
- `sock://` is not consumed by the replay-source switch; it is rejected by assert there. Handshake first packet is type 17 `MatrixInitialPacket` carrying ticket text, flags, and client identity blobs, sent over UDP (consistent with docs\MatrixConnection.md).

## Unresolved
- Wire layout of the encoded packet (`0x4980A0`), meanings of this+2120/2408 blobs, 1962 constant, request-struct field map and queue storage inside `0xBC2B30`, and where type 5 is consumed outside this switch.

# Round 18: UDP open, initial-packet emission, existing-code mapping

## Observed
- `0xBD0960 MatrixGameSocket_OpenUdp(this=GameSocket, a2=two 8-byte addr blocks (remote/second), a3, a4=&port, a5=flags+addr...)`: asserts this+268 (handler) set and this+264 (state) == 0; stores security flags/addr at this+560..584; creates an overlapped UDP socket (`0xBE3510`, WSASocket) and sets a socket option (import thunk `dword_1AC4DDC`). Local port: tries bind candidates starting at the configured port (`*a4`) incrementing for up to 6 attempts (2 ephemeral attempts if port is 0), using the local address from `dword_1DFFF70`+48..; each failure is logged, after all fail the socket is closed and 0 returned. Then `getsockname` (`dword_1AC4D80`) records the bound endpoint at this+16..40; creates an 0x68-byte object (`0xBE8170`) at this+3448; sets state this+264=1, timestamp this+592; calls handler (this+8) vtable+4 with the socket (registers with IOCP); posts the first overlapped recv (WSARecvFrom-style import `dword_1AC4D54`): one buffer, 1400 bytes at this+632, address buffer at `*(this+3572)+24`. Pending (WSA 997) returns 1. Error path sets state 7.
- `0x4980A0` (stdcall, arg = packet): builds a stack stream (vtable VA 0x1C81144 over a `0x1C80CC4` buffer), calls `0x498A00` to serialize, asserts the stream is non-empty, passes it to `0xBD3B20` (channel arg 1) and returns the byte count (v8-v10).
- Existing `src\Startup\MatrixInitialPacket.cs` already records MessageId 0x11, 1962 constant, encoder `0x349E60`, emission `0x4980A0`, submit `0xBD3B20`; my round-17 field offsets agree (this+40 mode, +44 =1962, +56/60 pair, +64..72, +76/77 bytes, +80 text, +88 optional, +112 trailing blob). No mismatch found in the fields compared. Framing, sequence, reliability and datagram assembly are already documented in docs\MatrixConnection.md (lines ~180-320); incoming dispatch (translated table raw `0x33D424`) is in its server-message section.

## Unresolved
- I did not re-derive the per-type incoming handler name table this round (not decompiled; relied on the existing doc). Encryption/signature: no cipher found at this layer; `0xBD4780` is documented as not a general encryption pass (existing doc). Meaning of OpenUdp a3/a4 beyond the port pointer is partial.

# Round 19: serializer orchestrator and submit path

## Observed
- `0x498A00(packet, writerStream)` (stdcall): local header DWORD = 17 (message id); asserts stream flag a2+4 clear (line 82); ensures stream capacity >= 29 bytes (vtable+16 grow); calls `Wire_WriteCompactPositiveUInt16` (`0x320900`) on the stream, then `MatrixInitialPacket_Encode` (`0x349E60`); calls stream vtable+0 (finalize); returns 1.
- `0xBD3B20 SubmitApplicationBuffer(channel, stream)` (disasm; decompile fails): asserts this+0x108 and this+0x10C non-zero (lines 0x47E/0x47F); returns early if byte this+0xD95 set; stream vtable+0x0C yields the byte length (0 = return); if length > WORD this+0x104 and channel==3 and length > 0x56C it asserts (line 0x499), logs and promotes channel to 2; channel switch (jump table 0xFD4A3C, channels 0-3, default asserts line 0x4D3).
- No checksum, signature or cipher instruction appears in these two functions.

## Interpreted
- The 'compact positive UInt16' written before the packet body is probably a length/size prefix (not proven which); existing `MatrixInitialPacket.cs` documents `FixedEncodedSize = 30` and no prefix, so the prefix may belong to the stream layer, not the encoder. Possible mismatch; unverified.
- Framing/ack/reliability: already in docs\MatrixConnection.md (lines ~180-320); nothing contradicting it was seen.

## Unresolved
- The channel jump-table targets, the IOCP recv completion handler for the 1400-byte buffer, and the type-id to handler-name list were not re-derived (existing doc covers table raw 0x33D424). Scope: only these two functions were inspected this round.

# Round 20: compact UInt16 is the message id; channel table

## Observed
- `0x320900` writes the DWORD at `*this` (asserts non-zero, line 50; asserts <= 0xFFFF, line 57): if <= 0xFF it writes ONE byte, else a 0x?? marker byte (from `0x1B57625`) plus a 16-bit value. In `0x498A00` the value is the local 17 (0x11), so the first byte emitted is 0x11 = `MatrixInitialPacket.MessageId`. It is the message-type id, not a length prefix.
- `0xBD3B20` jump table: VA 0xFD4A3C = raw 0xBD3E3C (IDA's 'invalid jump table' label points at the VA; file offset = VA-0x400C00). Targets: ch0 -> raw 0xBD3C19 (asserts, line 0x4A1: invalid channel for submit); ch1 and ch2 -> 0xBD3C3C (shared path: helper `0x12CDE60`/`0x12CB8C0`, `GameSocketFragment_Enqueue` 0xBE47C0, `0xBD5C30`, `GameSocketFragment_PackHeader` 0xBCFE30, `0xBD5BA0`); ch3 -> 0xBD3DC6 (`PackHeader` then `0xBD5C00` and `0xBE5D10`, no enqueue seen in the listing, i.e. unreliable direct path).

## Interpreted
- Round-19 'possible mismatch' is withdrawn: `MatrixInitialPacket.cs` (first byte 0x11, FixedEncodedSize 30) matches; no src change. Channel 3 = unreliable, 1/2 = fragment/reliable queue is consistent with docs\MatrixConnection.md ~199-202; per-channel names beyond that are not proven.

## Unresolved
- IOCP recv completion handler for the this+632 buffer and the type-id -> handler list were not traced this round (existing doc: IOCP receive dispatch and table raw 0x33D424). Scope: only 0x320900 and 0xBD3B20 targets inspected.

# Round 21: GameSocket receive completion chain (verification)

## Observed
- GameSocket vtable VA 0x1C73A5C slots: +0x18 = raw `0xBD2E30 GameSocket_HandleReceiveCompletion`, +0x1C = `0xBD2D10` close, +0x20 = `0xBD2D70` completion error (matches the PumpThread slot use in docs\MatrixConnection.md).
- `0xBD2E30`: locks socket+0xD98; skips if suppression byte +0xD95; `WSAGetOverlappedResult` (`dword_1AC4D64`) on the operation record at this+3572 gives the byte count; decodes the sender sockaddr at record+24 (`0xBE3A00`); calls `0xBD3030 GameSocket_AdmitReceivedDatagram(count, sender)`; then reposts the 1400-byte `WSARecvFrom` into this+632 (address length 128). Error != 996/997 goes to slot +0x20.
- `0xBD3030`: accepts 5..1400 bytes; first 4 bytes (network order, via `dword_1AC4D94`) must equal DWORD this+276 (context); sender must be specific/non-zero port (`0xBCCA60`); failure logs and increments this+236. Body = count-4 bytes at this+636: context 0 -> `0xBD39D0 GameSocket_ParseZeroContextHandshakeReply`; non-zero -> `0xBD3140 GameSocket_ParseNonzeroContextChunks`; tracks max size at this+3464.
- Downstream (already documented, not re-derived): queue consumed by `0xBD3E50` in tick `0xBD4BC0`; channel1 descriptors reach the receiver dispatcher `0x33CE40` (ids 34..68, table raw `0x33D424`).

## Unresolved
- I did not decompile `0xBD3140`/`0xBD3E50` this round, and did not produce a new id->handler name list; names beyond `docs\MatrixServerMessageBodies.md` / `MatrixServerMessageCatalog.cs` are unproven. Fragment reassembly/ack internals not re-traced (see MatrixConnection.md).

# Round 22: ClientApi automatic redirect limit

## Observed
- `ClientApiRequest_Reset` (`0xBC1280`) writes the word `0x0205` at request `+0x3C` (`0xBC12CD`) and clears the following byte at `+0x3E`. `ClientApi_PumpRequestWorker` (`0xBC2B30`) reads only the low byte at request `+0x3C`, zero-extends it, and passes its address with a four-byte length to `WinHttpSetOption` at `0xBC2F44`, using option `0x59`. The sample's IAT slot `0x1AC4D08` resolves to `WinHttpSetOption`; Microsoft documents option `0x59` as `WINHTTP_OPTION_MAX_HTTP_AUTOMATIC_REDIRECTS`. Thus this request's WinHTTP automatic redirect ceiling is 5. The remaining `0x02` byte in the reset word is not interpreted here.

## Interpreted
- The five-redirect ceiling is copied from each request's initialized byte into a 32-bit WinHTTP option value when the worker configures the request handle.

## Unresolved
- The purpose of request byte `+0x3D` and whether any path changes the redirect limit after reset.

# Round 23: ClientApi per-URL proxy configuration

## Observed
- In `ClientApi_PumpRequestWorker` (`0xBC2B30`), the proxy path is guarded by a nonzero byte at global-object `+0x10` and a nonzero DWORD at `+0`. It builds a `WINHTTP_AUTOPROXY_OPTIONS` structure with `dwFlags=2` (`WINHTTP_AUTOPROXY_CONFIG_URL`), `lpszAutoConfigUrl` from global-object `+4`, zeroed other reserved/auto-detect fields, and `fAutoLogonIfChallenged=TRUE`. The worker formats the request URL and calls `WinHttpGetProxyForUrl` at `0xBC2E7F` (IAT `0x1AC4D00`).
- On successful PAC lookup, the returned 12-byte `WINHTTP_PROXY_INFO` is applied to the request handle by `WinHttpSetOption(handle, 0x26, &proxyInfo, 12)` at `0xBC2EEB`. If PAC lookup fails, the worker queries the existing `WINHTTP_OPTION_PROXY` value with `WinHttpQueryOption` at `0xBC2EB9`; it applies that value with the same `WinHttpSetOption` only if the query succeeds. If the fallback query also fails, it logs and skips the set. The two optional strings in the proxy-info structure are released with imported `GlobalFree` at `0xBC2F18` and `0xBC2F25`.
- The original PE import table maps `0x1AC4D00/0x1AC4D04/0x1AC4D08` to `WinHttpGetProxyForUrl`/`WinHttpQueryOption`/`WinHttpSetOption`, and `0x1AC4320` to `GlobalFree`. Microsoft's WinHTTP references identify option `0x26` as `WINHTTP_OPTION_PROXY` and flag `2` as `WINHTTP_AUTOPROXY_CONFIG_URL`; see [WinHttpGetProxyForUrl](https://learn.microsoft.com/en-us/windows/win32/api/winhttp/nf-winhttp-winhttpgetproxyforurl), [WINHTTP_AUTOPROXY_OPTIONS](https://learn.microsoft.com/en-us/windows/win32/api/winhttp/ns-winhttp-winhttp_autoproxy_options), and [option flags](https://learn.microsoft.com/en-us/windows/win32/winhttp/option-flags).

## Interpreted
- When the proxy gate is enabled, the client uses a configured PAC URL to resolve a proxy separately for each request URL, then explicitly applies the resulting proxy info to the request handle. On lookup failure it preserves the handle's current proxy configuration when that value can be queried.

## Unresolved
- The global object's field names and initialization source are unknown; the auto-proxy gate and PAC URL are recorded by offsets only. The meaning of the `0x02` byte adjacent to the redirect-limit value at request `+0x3D` remains separate and unresolved.

# Round 24: ClientApi WinHTTP session settings

## Observed
- `0xBC39B0` creates the session with `WinHttpOpen` (IAT `0x1AC4D1C`), passing `0x10000000` (`WINHTTP_FLAG_ASYNC`) as flags. It derives access type as 3 (`WINHTTP_ACCESS_TYPE_NAMED_PROXY`) when the configured proxy-name pointer at global-object `+8` is nonzero, otherwise 0 (`WINHTTP_ACCESS_TYPE_DEFAULT_PROXY`); proxy name and bypass pointers come from `+8/+0x0C`.
- After a session handle is returned, the initializer calls `WinHttpSetOption` on it twice. Option 79 (`WINHTTP_OPTION_ENABLE_FEATURE`) receives value 2 (`WINHTTP_ENABLE_SSL_REVERT_IMPERSONATION`); option 84 (`WINHTTP_OPTION_SECURE_PROTOCOLS`) receives `0x80` (`WINHTTP_FLAG_SECURE_PROTOCOL_TLS1`). Both values and calls use four-byte buffers. If either call fails, the initializer logs that option's failure but continues and still reports successful session creation when the handle is valid.
- The original PE maps IAT slots `0x1AC4D1C` and `0x1AC4D08` to `WinHttpOpen` and `WinHttpSetOption`. CompanionTools checks the imports, asynchronous flag, option IDs, values, and call sequences. The names and values are corroborated by Microsoft's [WinHTTP option reference](https://learn.microsoft.com/en-us/windows/win32/winhttp/option-flags).

## Interpreted
- The initialized session explicitly opts into asynchronous operation, enables SSL impersonation reversion during certificate operations, and supplies only the TLS 1.0 protocol bit to WinHTTP's secure-protocol setting.

## Unresolved
- The source and runtime contents of the proxy name and bypass strings are not established. The effective TLS behavior may also depend on how the operating system handles this legacy protocol option.

# Round 25: ClientApi WinHTTP platform preflight

## Observed
- `0xBC39B0` begins with `WinHttpCheckPlatform` at `0xBC39DA` (original PE IAT `0x1AC4D18`). A false result returns failure immediately; a true result continues to the `WinHttpOpen` and session-option setup described in Round 24. Microsoft's API reference defines the result as whether the current platform is supported by WinHTTP. CompanionTools checks the IAT name and call/branch bytes.

## Interpreted
- ClientApi session initialization refuses to proceed when WinHTTP reports the platform unsupported; this check is synchronous even though the subsequently created session is asynchronous.

## Unresolved
- No runtime behavior is inferred for platforms on which the check returns false.

# Round 26: ClientApi deferred callback-context cleanup

## Observed
- `ClientApi_WorkerDetachCompletionOnTeardown` (`0xBC2890`) reads the worker's
  pointer at `+0x1C`; when non-null it sets byte `+0x11` on that context at
  `0xBC28CB`. It separately closes the worker's handle at `+0x20` through
  `WinHttpCloseHandle` and clears that field. The `0x1C`-byte callback context
  is passed as `WinHttpSendRequest`'s `dwContext` (Round 14), then stored to
  worker `+0x1C` at `0xBC3280`; its `+0x11` byte is initialized to zero by
  `ClientApi_PumpRequestWorker`.
- On `WINHTTP_CALLBACK_STATUS_HANDLE_CLOSING` (`0x800`),
  `ClientApi_WinHttpStatusCallback` tests context byte `+0x11`. When set, it
  releases the response reference at `+0x18` when non-null, clears the pointer,
  then releases the callback context. This connects the teardown flag store to
  the callback's deferred cleanup path. Manager teardown calls the detach
  helper before freeing workers; route-worker pruning also uses it.
- CompanionTools now verifies the worker-context test and flag store, the
  context-to-worker pointer store, separate handle-close/clear sequence,
  callback's matching flag test, and `WinHttpCloseHandle` import against the
  original sample.

## Interpreted
- Worker teardown transfers response/context cleanup to the asynchronous
  `HANDLE_CLOSING` callback by setting the callback-context flag before closing
  its separate worker handle.

## Unresolved
- The observed teardown code does not itself close or cancel the request
  handle stored at callback-context `+0`; cancellation and completion
  guarantees remain unestablished.

# Round 27: ClientApi WinHTTP request-error conversion

## Observed
- `ClientApi_DispatchWinHttpStatus` (`0xBC1DA0`) recognizes
  `WINHTTP_CALLBACK_STATUS_REQUEST_ERROR` (`0x200000`) at `0xBC2580`. It
  checks the status-information length against 8 at `0xBC2599` and emits a
  diagnostic if it differs, but still reads the DWORD at offset `+4`
  (`dwError`) at `0xBC25BB`; `dwResult` at offset `+0` is not used in this
  branch. The value is passed to the shared failure helper `0xBC1C40`.
- `0xBC1C40` preserves negative input values, maps positive values to
  `0x80070000 | (error & 0xFFFF)`, and substitutes `0x80004005` when the
  resulting value is nonnegative. CompanionTools now checks the callback
  status compare, length diagnostic, `dwError` load, conversion instructions,
  the 8-byte layout and `+4` offset, and the modeled conversion cases against
  the original sample.

## Interpreted
- Under the Windows `WINHTTP_ASYNC_RESULT` layout, positive WinHTTP error
  codes become HRESULT-style `0x8007xxxx` values; zero becomes `E_FAIL`, and
  already-negative values are retained. The callback layer then records this
  failure result and closes the request handle as described in Round 28.

## Unresolved
- A malformed status-information length is logged but does not prevent the
  `+4` read. Normal WinHTTP callback data is expected to use the documented
  8-byte structure; behavior for an invalid pointer/length is not established.

# Round 28: ClientApi secure-failure details

## Observed
- `ClientApi_DispatchWinHttpStatus` selects `SECURE_FAILURE` (`0x10000`) at
  `0xBC1E15`. It reads the DWORD status-information mask only when the reported
  length is at least 4; shorter data becomes a zero mask without a diagnostic.
  The message formatter appends separate labels for mask bits `0x01` through
  `0x40` and for bit `0x80000000`; it does not append per-bit labels for
  `0x80` through `0x40000000`. It then reports failure code 12175 through the
  shared helper with its logging flag enabled. CompanionTools checks the
  status/length/load sequence, each tested bit, the helper call, and the
  observed mask model against the original sample.
- Microsoft documents the secure-failure callback payload as a DWORD bitwise
  OR of error values in [`WINHTTP_STATUS_CALLBACK`](https://learn.microsoft.com/en-us/windows/win32/api/winhttp/nc-winhttp-winhttp_status_callback).

## Interpreted
- The callback always terminates this dispatch as a secure-failure result,
  even when the status payload is shorter than one DWORD; in that case its
  additional flag labels are absent. The generic error helper maps code 12175
  to `0x80072F8F`.

## Unresolved
- The client-specific text labels for the recognized bits remain encoded in
  the sample; no names are assigned to individual bits here.

# Round 29: ClientApi pre-send failure gate

## Observed
- In `ClientApi_PumpRequestWorker` (`0xBC2B30`), a failure result is first
  stored at callback-context `+0x14` at `0xBC31C3`. The worker tests that field
  at `0xBC31D8`; a nonzero result branches around both status-callback
  registration (`0xBC31E9`) and `WinHttpSendRequest` (`0xBC3213`).
- On that failure branch, the worker closes the already-open request handle
  through `WinHttpCloseHandle` at `0xBC3222`, clears callback-context handle
  `+0` at `0xBC322E`, and sets response kind 4 at `0xBC3273`. It then stores
  the callback context to worker `+0x1C` at `0xBC3280`, allowing the normal
  completion/result pump to observe the nonzero failure. Sample checks verify
  the result gate, skipped send call path, handle close/clear, and response
  kind store.
- The result at `+0x14` is produced by `ClientApi_WinHttpFailureHelper`
  (`0xBC1C40`) and recorded before the gate. The observed error-capture call
  uses the original PE IAT slot `0x1AC4378`, identified as `GetLastError` by
  the sample import table and the repository's import-slot mapping.

## Interpreted
- This is a synchronous request-setup failure route, distinct from the
  asynchronous `REQUEST_ERROR` callback: it tears down the request handle
  before sending and delivers a response-kind-4 completion through the
  worker's ordinary completion path.

## Unresolved
- The precise WinHTTP setup operation responsible for every stored
  pre-send failure is not established by this branch alone.

# Round 30: ClientApi zero-byte DATA_AVAILABLE completion

## Observed
- In `ClientApi_DispatchWinHttpStatus` (`0xBC1DA0`), the
  `DATA_AVAILABLE` branch reads the available-byte count from the first DWORD
  of the callback payload. At `0xBC2423`, a zero count skips the response-body
  capacity and `WinHttpReadData` path. If response kind at `+8` is still zero,
  it writes 1, then returns 1 as a terminal dispatcher result. A nonzero
  count continues to reserve body capacity and issue `WinHttpReadData`.
- The outer callback (`0xBC26E0`, `0xBC2767` onward) stores that nonzero
  dispatcher result in context `+0x14` if unset, changes response kind to 4
  only if it is still zero, and closes the request handle. Since the
  zero-available branch already set response kind 1, the callback preserves
  that value while signaling completion and closing the handle. CompanionTools
  checks the exact zero-count branch and the callback's result/close sequence.

## Interpreted
- An empty `DATA_AVAILABLE` notification is treated as end-of-body by this
  client and is a terminal result, parallel to its previously documented
  zero-byte `READ_COMPLETE` path. The response kind remains 1 through callback
  closure; no body read is issued for the empty notification.

## Unresolved
- The native status-payload length is not checked in this branch before the
  DWORD read. Normal WinHTTP `DATA_AVAILABLE` notifications carry four bytes;
  malformed callback payload behavior is not established.

# Round 31: ClientApi WinHttpOpenRequest flags

## Observed
- `ClientApi_PumpRequestWorker` (`0xBC2B30`) constructs `dwFlags` for
  `WinHttpOpenRequest` from the request's parsed HTTPS indicator at `+0x0C`
  and byte `+0x60`. It sets `0x00800000` only when the DWORD at `+0x0C` is
  exactly 1, and ORs `0x00000100` when byte `+0x60` is nonzero
  (`0xBC2CCD`–`0xBC2CE9`). The URL parser stores its HTTPS boolean at
  request `+0x0C` (Round 16), so this path selects the secure flag for HTTPS.
- At `0xBC2D92`, the worker supplies the flags, then three null optional
  arguments (HTTP version, referrer, accept types); it passes the worker's
  session handle, selected verb, and request object-name pointer before calling
  `WinHttpOpenRequest` at `0xBC2DB7`. Original PE IAT VA `0x1AC4CFC` resolves
  to `WINHTTP.dll!WinHttpOpenRequest`. CompanionTools checks the imports,
  flag-construction bytes, null argument sequence, callsite, and modeled
  flag truth table against the sample.
- WinHTTP names `0x00800000` as `WINHTTP_FLAG_SECURE`; `0x00000100` is
  `WINHTTP_FLAG_REFRESH` (also named `WINHTTP_FLAG_BYPASS_PROXY_CACHE`) in
  the [`WinHttpOpenRequest` reference](https://learn.microsoft.com/en-us/windows/win32/api/winhttp/nf-winhttp-winhttpopenrequest).
- `ClientApiRequest_Reset` (`0xBC1280`) initializes request byte `+0x60` to
  zero at `0xBC131C`; CompanionTools checks this default store as well.

## Interpreted
- Request byte `+0x60` controls whether the refresh/bypass-cache flag is
  passed to the handle-opening API. Its request-reset default is disabled.
  This establishes only the flag argument, not effective cache behavior or
  which callers later enable the byte.
- The null HTTP-version argument delegates version selection to WinHTTP; it
  does not establish an explicit HTTP-version policy in the client.

## Unresolved
- Writer provenance for request byte `+0x60` beyond reset has not yet been
  located. No caller behavior is inferred from this field alone.

# Round 32: ClientApi route-worker timeout

## Observed
- `ClientApi_PumpRouteWorkers` (`0xBC3F80`) calls
  `ClientApi_PumpRequestWorker` for each route worker. A nonzero worker field
  at `+0x1C` counts as active completion and bypasses timeout evaluation.
- Otherwise it calls the `timeGetTime` import at IAT VA `0x1AC4D24`, converts
  the millisecond tick to microseconds through helper `0x159DC30` with
  multiplier 1000, and subtracts the worker's 64-bit timestamp at `+0x28`.
  The code removes the worker only when elapsed time is strictly greater than
  `0x07270E00` microseconds (120 seconds); equality is retained.
- The timeout path logs, swap-removes the worker from the route-worker array,
  calls `ClientApi_WorkerDetachCompletionOnTeardown` (`0xBC2890`), then
  destroys the worker. The teardown helper marks callback-context byte `+0x11`
  when present, closes the worker's WinHTTP handle at `+0x20`, and clears it.
- Worker constructor `0xBC27A0` seeds `+0x28/+0x2C` with
  `timeGetTime()*1000` at `0xBC2831`. `ClientApi_PumpRequestWorker` also
  overwrites that timestamp on the observed path at `0xBC2C0F`.
  CompanionTools checks the original sample bytes for the active-completion
  gate, 64-bit timeout comparison, constructor seed, and pump-path store.

## Interpreted
- The route loop's timeout arithmetic is unsigned 64-bit, and the threshold
  comparison is strict. No wrap correction is present between the sampled
  32-bit `timeGetTime` value and the stored 64-bit microsecond timestamp; a
  tick wrap can therefore make the subtraction underflow and satisfy the
  timeout check.

## Unresolved
- The pump-path timestamp rewrite is branch-dependent; its precise renewal
  cadence is not established.
- The teardown helper's completion-context cleanup behavior is observed, but
  cancellation guarantees for an in-flight WinHTTP operation are not proven.
