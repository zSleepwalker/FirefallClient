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
