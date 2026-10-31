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
