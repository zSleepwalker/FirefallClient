# ClientApi / WinHTTP analysis handoff

Detailed ClientApi rounds moved here from [Handoff.md](Handoff.md) to keep the cross-project handoff concise. The focused login/network chronology and sample-backed models are in [LoginNetwork.md](LoginNetwork.md).

# Round 20: ClientApi URL lookup and host-suffix fallback

## Observed
- `0xBC1460` is called by request handler `0xBC1990`; it returns the input
  integer plus the selected global-tree entry's field at `+0x14`. It acquires
  the tree lock through import slots `0x1AC4284/0x1AC4278`, increments the
  tree counter at global `0x1E003D0 + 0x38`, performs lookups through
  `0xBC4D60`/`0xBC61C0`, then decrements the counter and releases the lock.
- The first lookup uses key builder `0xBC1040`. On a miss, the routine scans
  the parsed host from its first `.` through later dots, forming suffixes
  starting with each dot (for example `.example.com`, then `.com`). It
  lowercases each suffix before lookup. This fallback runs only for HTTP on
  port 80 or HTTPS on port 443. The direct key is lowercased `host:port`,
  formatted by `"%s:%d"` at VA `0x1C73534`. The tree search is an ordered
  string search using `strcmp`.
- The host-suffix behavior is modeled by
  `CaptureUrlHostPortLookupKey`, `CaptureDefaultPortHostSuffixLookupKeys`,
  and `CaptureClientApiServerClockOffsetEntries`, covered by CompanionTools
  cases for key normalization, default/non-default ports, case folding,
  multiple labels, and missing hosts. `CaptureClientApiAdjustedTimestamp`
  models the native 32-bit timestamp-plus-offset arithmetic. Sample guards
  verify the direct key formatter, lookup order, `strchr` suffix path, and
  direct/suffix value writes.
- `0xBC1680`, called by `ClientApi_PumpRequestWorker` at `0xBC338D`, inserts
  or updates the direct `host:port` entry and, for default ports, the same
  suffix entries. The worker extracts the `"date"` response header and parses
  it at `0xBCB5A0` using the HTTP-date layout
  `"%3s, %d %3s %d %d:%d:%d %3s"`, `_strnicmp` month matching, and
  `_mkgmtime64`. It stores parsed server time minus current `_time64()` in
  node field `+0x14`, an offset in seconds. Lookup `0xBC1460` adds the
  matching offset to the input timestamp; no match leaves the timestamp
  unchanged. `0xBC4CB0` returns an existing value slot or inserts a missing
  key, and `0xBC7A10` performs balanced ordered-tree insertion. Sample-mapped
  bytes/imports and CompanionTools guards confirm the date/clock-offset path.

## Unresolved
- The application policy for the learned server clock correction, including
  how long entries persist and when they are invalidated, is unknown. IDA's
  string read at `0x1C73534` disagreed with the PE-section-mapped sample bytes,
  so prefer the latter for data addresses.
  Earlier attempted mapping of these `0xBCxxxx` code addresses using the
  unrelated `+0x400C00` scheme was incorrect; analyze them at their direct
  IDA addresses.

# Round 21: ClientApi adjusted-time query field

## Observed
- Request handler `0xBC1990` passes the timestamp returned by `0xBC1460` as
  the last argument to builder `0x12E4100`. In that builder the value is
  formatted as the unsigned `tc` field in the inner request query.
- PE-mapped sample VA `0x1C57568` contains
  `ver=%d&tc=%u&nonce=%04x%04x%04x%04x&uid=%s&host=%s&path=%s&hbody=%s&cid=%llu`.
  The version argument is fixed at `2`; four calls to imported `rand()` supply
  nonce values truncated to 16 bits and rendered as four-digit lowercase hex;
  the named string fields are supplied in the order `uid`, `host`, `path`, and
  `hbody`; the `cid` conversion consumes a 64-bit unsigned value from two
  adjacent object words.
- Builder `0x12E4100` then wraps the inner query as the middle `%s` in
  `"Red5 %s %s %s"` at VA `0x1C576D0`; the other two fields are separately
  constructed strings. `CaptureClientApiRequestQuery` models the inner query.
  The sample-backed CompanionTools checks cover the format and `rand` import.

## Unresolved
- The native text encoding and semantic roles of several string/ID fields
  beyond their literal query keys remain unknown.

# Round 22: ClientApi request signatures

## Observed
- Builder `0x12E4100` applies the CryptoAPI SHA-1 construction to the inner
  query twice, with separate keys from object fields at `+0x3C` and `+0x40`.
  The helper at `0x12EEAD0` hashes keys longer than 64 bytes, zero-pads the
  key block, and hashes its XOR-with-`0x36` inner pad. `0x12EECF0` appends the
  query bytes. `0x12EED70` completes the inner SHA-1 digest, hashes the
  key-block XOR `0x5C` plus that digest, and obtains the outer digest.
- `0x12EE800` converts each 20-byte digest to 40 lowercase hexadecimal
  characters using the table at VA `0x1C58590`. The two strings become the
  first and third fields of `"Red5 %s %s %s"`, surrounding the query.
  `CaptureClientApiRequestSignature` models the byte-level HMAC-SHA1 and
  lowercase-hex conversion; a known-vector test and sample import/table guards
  cover the model.

## Unresolved
- The source and semantics of both signing keys, including whether either can
  be absent, and the native text encoding used when hashing the query remain
  unresolved.

# Round 25: ClientApi signature header key

## Observed
- `0xBC1990` inserts the string produced by `0x12E4100` into the request map
  at `this+0x20` using key VA `0x1BB5328`. The PE-mapped sample string is
  `"X-Red5-Signature"`, now modeled as
  `ClientApiRequestSignatureHeaderName`.
- The header-shaped key supports interpreting the signed query as an HTTP
  request header. `ClientApi_PumpRequestWorker` traverses a name/value tree,
  formats each pair as `"%s: %s\r\n"`, and calls
  `WinHttpAddRequestHeaders`; a separate call adds the `Content-Type` header.
  `CaptureClientApiRequestHeaderLine` models the map-entry format, and
  CompanionTools checks the native format, imports, and both worker callsites.

## Unresolved
- The worker tree's precise relation to the map object at `request+0x20` is
  inferred from the matching name/value layout and store path; a full request
  object type has not been reconstructed.

# Round 26: ClientApi request timer defaults

## Observed
- `ClientApiRequest_Reset` at IDA address `0xBC1280` calls the imported
  `timeGetTime` at `0xBC1367` and writes the returned 32-bit tick at request
  offset `+0x68`. It then decrements EAX and writes that value to offsets
  `+0x6C`, `+0x70`, and `+0x74` (`0xBC136D` through `0xBC1377`).
- The sample is
  `D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe.original`.
  Its PE-mapped code VA for IDA `0xBC1367` is `0x00FC1F67` under the already
  validated `raw + 0x400C00` code mapping. Bytes there are
  `FF 15 24 4D AC 01 89 47 68 48 89 47 6C 89 47 70 89 47 74`, confirming the
  import call and four destination offsets. The IAT slot `0x1AC4D24` resolves
  to `WINMM.dll!timeGetTime`.
- `ClientApiRequestTimerDefaults` models the tick and its predecessor with
  unsigned 32-bit wraparound. CompanionTools covers both ordinary and zero
  tick behavior and checks the sample import and instruction bytes.

## Interpretation and unresolved details
- The initialized fields likely participate in request timeout/elapsed-time
  tracking, but their individual runtime roles have not been traced. Do not
  conflate this millisecond tick source with the `_time64` seconds used by
  the server-adjusted query timestamp.

# Round 27: ClientApi request timer lifecycle

## Observed
- The same `timeGetTime` import is sampled at three further request lifecycle
  points. `dispatch_control_routine_BC43C0` (`0xBC43C0`) links the request into
  the tracker at `0xBC4503`, then calls the import at `0xBC4508` and stores the
  result at request `+0x6C` (`0xBC4512`).
- `ClientApi_PumpRequestWorker` (`0xBC2B30`) calls the `WinHttpOpenRequest`
  thunk at `0xBC2DB7` when a URL is available, then calls `timeGetTime` at
  `0xBC2DD5` and stores the result at request `+0x70` (`0xBC2DE1`). When no URL
  is available, the branch still samples the tick before the handle check.
- In the worker's response-completion path, after response processing and
  immediately before acquiring the request callback pair, it samples the same
  import at `0xBC37C9` and stores the result at request `+0x74` (`0xBC37D5`).
- The sample is
  `D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe.original`.
  The validated code-VA mapping is `IDA address + 0x400C00`. The byte stores
  are `89 46 6C` at mapped VA `0xFC5112`, `89 41 70` at `0xFC39E1`, and
  `89 41 74` at `0xFC43D5`. CompanionTools checks these stores in addition to
  the reset initialization and import identity.

## Interpretation and unresolved details
- The stores provide request tracking, open-branch, and pre-callback tick
  checkpoints, but no elapsed-time arithmetic or consumer of these specific
  fields has been established. Their intended timer policy remains unresolved.

# Round 28: ClientApi WinHTTP callback close transitions

## Observed
- `ClientApi_WinHttpStatusCallback` at IDA address `0xBC26E0` first checks
  that callback handle `a1` equals the handle in its context at `a2+0`.
- For status `0x800`, the callback sets the response-kind word at
  `*(a2+0x18)+8` to 3 only when that field is zero, clears the context handle,
  and, when context byte `+0x11` is set, invokes the object at `a2+0x18` with
  argument 1 and clears that pointer before releasing the callback context.
- For other statuses, dispatch is skipped once context `+0x14` is nonzero.
  Otherwise it calls `ClientApi_DispatchWinHttpStatus` (`0xBC1DA0`). A nonzero
  return is saved to context `+0x14` if still empty, response kind is set to 4
  only if unset, and the callback closes `a1` through the IAT slot at
  `0x1AC4CF8`, resolved as `WINHTTP.dll!WinHttpCloseHandle`.
- In the sample
  `D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe.original`,
  IDA `0xBC26E0` maps to code VA `0x00FC32E0` under `IDA + 0x400C00`. The
  comparison for status `0x800` is at `0xFC330C`; the response-kind 3 and
  handle-clear stores are at `0xFC331D` and `0xFC3328`. Response-kind 4 and
  the indirect close call are at `0xFC337D` and `0xFC3385`. CompanionTools
  checks these instructions and the import slot.

## Interpretation and unresolved details
- The callback has explicit handle-closing and close-after-dispatch paths,
  but the full status-code mapping in `ClientApi_DispatchWinHttpStatus` and
  the meanings of response-kind values 3 and 4 have not been established.

# Round 29: ClientApi response context layout

## Observed
- `ClientApiResponse_Ctor` (`0x3845D0`) initializes a 0x20-byte response
  object and calls `ClientApiResponse_Reset` (`0xBC11D0`). Reset clears
  offsets `+4`, `+0xC`, and `+0x14`, sets `+8` to 1, initializes the
  tree-shaped sentinel at `+0x10`, and clears shared-text fields at `+0x18`
  and `+0x1C`.
- `ClientApi_PumpRequestWorker` allocates `0x20` bytes for this object, then
  stores its request pointer at response `+4` and sets response `+8` to 0.
  The callback previously traced changes `+8` to 3 on HANDLE_CLOSING or 4
  after a nonzero status-dispatch result. These are enumerated as observed
  states only; their broader meanings remain unresolved.
- CompanionTools now verifies the reset stores, constructor-to-reset call,
  allocation size, request pointer/kind initialization, and kind values.

## Interpretation and unresolved details
- The sentinel at `+0x10` appears to head an ordered container, likely the
  response's header collection based on its use in status/header processing;
  that association is not established by constructor code alone. The purpose
  of fields `+0xC`, `+0x14`, `+0x18`, and `+0x1C` is still unknown.

# Round 30: ClientApi response ownership

## Observed
- The response vtable's first code pointer resolves to `0x3845B0`, which
  forwards to destructor `0x3844B0`. That destructor releases shared-text
  fields at `+0x18` and `+0x1C`, then destroys the tree rooted at `+0x10`
  through `0x326EF0`.
- `ClientApi_DispatchWinHttpStatus` inserts response header name/value pairs
  into the container at response `+0x10`, supporting its identification as
  the owned response-header tree. The field-specific meaning of `+0x14` and
  the two shared-text values remains unresolved.
- CompanionTools checks the destructor's text-field cleanup and tree
  destructor call against the sample.

# Round 31: ClientApi WinHTTP callback context

## Observed
- `ClientApi_PumpRequestWorker` allocates a 0x1C-byte context at `0xBC2D0E`.
  It clears the context, explicitly zeroes byte `+0x11` and dword `+0x14`,
  stores the `ClientApiResponse` pointer at `+0x18`, and later stores the
  opened WinHTTP request handle at `+0`. The sample IAT slot `0x1AC4D0C`
  resolves to `WINHTTP.dll!WinHttpSetStatusCallback`; the call at `0xBC31E9`
  registers the callback with all notification bits (`0xFFFFFFFF`) and
  reserved argument zero. At `0xBC31F8`, the worker pushes this context as
  the final `dwContext` argument to `WinHttpSendRequest` (`0x1AC4D10` resolves
  to `WINHTTP.dll!WinHttpSendRequest`).
- `ClientApi_WinHttpStatusCallback` checks that the callback's handle equals
  context `+0`, gates non-closing status dispatch on context `+0x14`, accesses
  the response through `+0x18`, and tests byte `+0x11` on HANDLE_CLOSING.
- No nonzero write to byte `+0x11` was found in `ClientApi_PumpRequestWorker`;
  whether another path sets it and what ownership behavior it enables remain
  unresolved. Other context fields at `+4`, `+8`, `+0xC`, and `+0x10` are kept
  unnamed.
- CompanionTools checks the sample allocation size, initial flag/result
  stores, subsequent handle/result/response stores, and the callback/send
  import slots and callsites.

# Round 32: ClientApi WinHTTP response stages

## Observed
- `ClientApi_DispatchWinHttpStatus` (`0xBC1DA0`) handles callback status
  `0x100000` by counting completed upload bytes, updating request progress,
  and calling `WinHttpReceiveResponse` (`0x1AC4CE0`) after upload completion.
- Status `0x20000` queries the numeric response status and raw response
  headers through `WinHttpQueryHeaders` (`0x1AC4CE4`). The parsed header
  pairs are inserted into the response tree at `+0x10`, then the dispatcher
  calls `WinHttpQueryDataAvailable` (`0x1AC4CE8`).
- Status `0x40000` reserves response-body capacity and starts
  `WinHttpReadData` (`0x1AC4CEC`). Status `0x80000` appends read bytes to
  response text at `+0x1C`, then requests more available data. A zero-byte
  read assigns observed response kind 1 if unset and returns the terminal
  result.
- Statuses `0x10000`, `0x200000`, and `0x4000` have separate secure-failure,
  request-error, and redirect handling paths. Redirect policy and the exact
  response-kind meanings are not fully established.
- CompanionTools checks the import-slot names, representative indirect call
  sites, and the callback-status numeric values against the sample/model.

# Round 33: ClientApi redirect header replay

## Observed
- In `ClientApi_DispatchWinHttpStatus` (`0xBC1DA0`), the `0x4000`
  redirect branch calls `WinHttpCrackUrl` at `0xBC1F8C` and compares its
  parsed host text with the request's stored host text through `0x2FC540`
  (`0xBC1FF0`).
- The branch returns without replay when request `+0x24` is zero or callback
  context byte `+0x10` is already set. When the host comparison is unequal,
  it walks the request header tree, formats each entry into a temporary
  buffer, and calls
  `WinHttpAddRequestHeaders` at `0xBC2085` with `0x80000000`
  (`WINHTTP_ADDREQ_FLAG_REPLACE`). It then sets context byte `+0x10` at
  `0xBC20B5`.
- The request gate's purpose and the host comparison's case sensitivity are
  unresolved. CompanionTools checks the import names, host-comparison call,
  replacement flag, header submission, and one-shot context-flag store.

# Round 34: ClientApi request header tree count

## Observed
- `ClientApiRequest_Reset` (`0xBC1280`) clears the DWORD at request `+0x24`
  while resetting the tree sentinel at `+0x20`.
- Request header insertion at `0xBC1990` passes `request+0x20` to the
  find-or-insert helper `0x4969A0`. Its insertion path enters the generic
  ordered-tree insert at `0x31D720`; the node-insert helper increments the
  container's `this+4` DWORD at `0x31E95F`. This confirms request `+0x24` is
  the header-tree element count.
- The `REDIRECT` handler's `request+0x24 != 0` condition is therefore a
  nonempty-header gate, not an unexplained request flag. CompanionTools checks
  the reset store, request tree base, insertion chain, and increment.

# Round 35: ClientApi redirect host comparison

## Observed
- The comparison helper `0x2FC540` calls import slot `0x1AC48A8` and tests
  for a zero return. Resolving the slot in the original PE import table
  identifies it as `_stricmp`, so redirect host comparison ignores ASCII
  case.
- The request URL parser at `0xBCE800` stores the host at request `+0x10`
  and calls `0x12B9B00` to lowercase the host; that helper calls imported
  `tolower` at IAT slot `0x1AC4978`.
- CompanionTools now verifies both IAT names and the host-normalization and
  comparison callsites.

# Round 36: Redirect comparison is host-only

## Observed
- The URL parser receives `request+0x0C`; its outputs are scheme at `+0x0C`,
  host at `+0x10`, port at `+0x14`, path at `+0x18`, and query at `+0x1C`.
- The redirect branch initializes `URL_COMPONENTS` with size `0x3C`, calls
  `WinHttpCrackUrl`, terminates its returned host using the returned host
  length, converts that host to shared text, and compares it against
  `request+0x10`. It does not read the cracked scheme or port for this
  comparison. Therefore only a host change triggers the observed
  cross-host header-replay condition; a scheme-only or port-only change does
  not.
- The replay flag is in callback-context byte `+0x10`, initialized to zero
  with the adjacent `+0x11` cleanup flag. It is set after the stored headers
  have been traversed and submitted, so subsequent redirect callbacks for
  this context skip replay. CompanionTools now includes the host-only policy
  cases and guards the URL-component offsets.

# Round 37: ClientApi automatic redirect limit

## Observed
- `ClientApiRequest_Reset` (`0xBC1280`) writes word `0x0205` at request
  `+0x3C` (`0xBC12CD`) and clears byte `+0x3E`. In
  `ClientApi_PumpRequestWorker` (`0xBC2B30`), `0xBC2F2F` zero-extends only
  request byte `+0x3C`; `0xBC2F2D` pushes a four-byte value length and
  `0xBC2F44` calls `WinHttpSetOption(handle, 0x59, &value, 4)`. Original
  sample IAT slot `0x1AC4D08` resolves to `WinHttpSetOption`. Microsoft's
  WinHTTP option reference names `0x59` as
  `WINHTTP_OPTION_MAX_HTTP_AUTOMATIC_REDIRECTS`; the value initialized and
  passed by the client is 5. The adjacent byte `+0x3D` is a separate retry
  limit initialized to 2, and `+0x3E` is the retry counter cleared at reset;
  their worker use is detailed in Round 50. CompanionTools checks the IAT,
  reset bytes, complete option-setup instruction sequence, and modeled
  redirect limit.

## Interpreted
- The WinHTTP request handle is configured with a five-redirect automatic
  redirect ceiling per request.

# Round 38: ClientApi per-URL proxy configuration

## Observed
- `ClientApi_PumpRequestWorker` (`0xBC2B30`) enters the proxy path only when
  global-object byte `+0x10` and DWORD `+0` are nonzero. It builds a
  `WINHTTP_AUTOPROXY_OPTIONS` structure with `dwFlags=2`,
  `lpszAutoConfigUrl` from global-object `+4`, and
  `fAutoLogonIfChallenged=TRUE`; its other auto-detect/reserved fields are
  zero. It formats the request URL and calls `WinHttpGetProxyForUrl` at
  `0xBC2E7F` (original PE IAT `0x1AC4D00`).
- A successful lookup supplies a 12-byte `WINHTTP_PROXY_INFO`, passed to
  `WinHttpSetOption(handle, 0x26, &proxyInfo, 12)` at `0xBC2EEB`. If the
  lookup fails, the worker queries the current `WINHTTP_OPTION_PROXY` with
  `WinHttpQueryOption` at `0xBC2EB9`, then sets that value only when the query
  succeeds. A failed fallback query logs and skips the set. Both optional
  proxy strings are released with `GlobalFree` at `0xBC2F18/0xBC2F25`.
- The original PE import table maps slots `0x1AC4D00`, `0x1AC4D04`,
  `0x1AC4D08`, and `0x1AC4320` to `WinHttpGetProxyForUrl`,
  `WinHttpQueryOption`, `WinHttpSetOption`, and `GlobalFree`. Microsoft's
  WinHTTP references identify option `0x26` as `WINHTTP_OPTION_PROXY` and
  auto-proxy flag `2` as `WINHTTP_AUTOPROXY_CONFIG_URL`. CompanionTools
  verifies the imports, option setup, fallback query/set sites, and the
  automatic-proxy field values.

## Interpreted
- When enabled, the client resolves a proxy per request URL through its
  configured PAC URL and applies the returned proxy info to the request
  handle. On lookup failure, it attempts to retain the handle's existing
  proxy settings.

## Unresolved
- Names and initialization source for the global object fields at `+0`,
  `+4`, and `+0x10`.

# Round 39: ClientApi WinHTTP session settings

## Observed
- `0xBC39B0` calls `WinHttpOpen` at `0xBC3A4B` (original PE IAT
  `0x1AC4D1C`) with flags `0x10000000` (`WINHTTP_FLAG_ASYNC`). Its access
  type is 3 (`WINHTTP_ACCESS_TYPE_NAMED_PROXY`) when the proxy-name pointer
  at global-object `+8` is nonzero, otherwise 0
  (`WINHTTP_ACCESS_TYPE_DEFAULT_PROXY`); name and bypass are passed from
  global-object `+8/+0x0C`.
- On a valid session handle, the initializer calls `WinHttpSetOption` on
  that handle with option 79/value 2 at `0xBC3A71` and option 84/value
  `0x80` at `0xBC3A9A`. The Windows SDK defines these as
  `WINHTTP_OPTION_ENABLE_FEATURE` /
  `WINHTTP_ENABLE_SSL_REVERT_IMPERSONATION` and
  `WINHTTP_OPTION_SECURE_PROTOCOLS` / `WINHTTP_FLAG_SECURE_PROTOCOL_TLS1`.
  Each call supplies a four-byte value. Failures are logged but do not
  change the initializer's successful return when the session handle exists.
  CompanionTools verifies the import names, exact calls, option IDs, values,
  and asynchronous flag.

## Interpreted
- The session is asynchronous, enables SSL impersonation reversion for
  certificate operations, and sets its secure-protocol mask to the TLS 1.0
  bit only.

## Unresolved
- The source and actual contents of proxy name/bypass strings remain
  unknown. Runtime TLS behavior on newer Windows versions is not inferred
  from this legacy configuration alone.

# Round 40: ClientApi WinHTTP platform preflight

## Observed
- `0xBC39B0` starts by calling `WinHttpCheckPlatform` at `0xBC39DA`
  (original PE IAT `0x1AC4D18`). If the result is false, it returns false
  before attempting `WinHttpOpen`; a true result continues through session
  creation and option setup. The IAT name and call/branch are sample-backed
  in CompanionTools. Microsoft documents this API as checking whether the
  current platform is supported by WinHTTP.

## Interpreted
- ClientApi initialization has an explicit supported-platform gate;
  `WinHttpCheckPlatform` itself is synchronous despite the later async
  session flag.

## Unresolved
- No behavior is inferred for the unsupported-platform path beyond the
  initializer's failure return.

# Round 41: ClientApi callback-context teardown

- Resolved the callback-context `+0x11` cleanup flag's nonzero source.
  `ClientApi_WorkerDetachCompletionOnTeardown` (`0xBC2890`) tests worker
  `+0x1C` and writes `1` to its context's `+0x11` at `0xBC28CB`; it separately
  closes and clears worker handle `+0x20`. The registered WinHTTP callback
  checks the same context byte on `HANDLE_CLOSING` and releases/clears response
  pointer `+0x18` before releasing the context. Sample checks now cover both
  sides of that flag, the context pointer store at `0xBC3280`, and the separate
  handle-close operation. This does not establish cancellation of the request
  handle at context `+0`.

# Round 42: ClientApi WinHTTP async errors

- `REQUEST_ERROR` (`0x200000`) checks the documented 8-byte async-result
  length, reads `dwError` at `+4` (not `dwResult`), and passes it through
  `0xBC1C40`. That helper preserves negative values, maps positive Win32 codes
  to `0x80070000 | (code & 0xFFFF)`, and maps zero to `0x80004005`. The
  sample-backed CompanionTools model and checks cover the field offset,
  length check, conversion, and representative positive/zero/negative cases.
  A length mismatch is logged but does not guard the subsequent read.

# Round 43: ClientApi secure-failure diagnostics

- The `SECURE_FAILURE` callback reads a four-byte status mask when available,
  appends labels for bits `0x01..0x40` and `0x80000000`, then reports
  `ERROR_WINHTTP_SECURE_FAILURE` (12175) through the common failure helper.
  Short status data is treated as a zero mask without an assertion. The
  sample-backed model records the labeled-bit mask `0x8000007F`; individual
  client text labels are still encoded and unnamed.

# Round 44: ClientApi pre-send failure handoff

- The worker stores a nonzero setup result in callback-context `+0x14`
  (`0xBC31C3`) and gates off callback registration/`WinHttpSendRequest`
  (`0xBC31D8`). That branch closes the opened request handle, clears context
  `+0`, assigns observed response kind 4, and attaches the context to worker
  `+0x1C` for normal result delivery. Its error-capture path calls imported
  `GetLastError` and `ClientApi_WinHttpFailureHelper`; exact setup operation
  ownership remains unresolved.

# Round 45: ClientApi empty response body

- For `DATA_AVAILABLE`, the dispatcher reads the four-byte available count.
  Zero skips capacity growth and `WinHttpReadData`, sets response kind 1 only
  if unset, and returns terminal status. The callback then records its
  completion result, preserves the already-set kind 1 (it writes kind 4 only
  when the kind is zero), and closes the request handle. `LoginNetwork.md`
  Round 30 and CompanionTools capture this empty-body path; the status length
  is not validated before the DWORD read.

# Round 46: ClientApi WinHttpOpenRequest flags

- The worker passes `WINHTTP_FLAG_SECURE` (`0x00800000`) only when parsed
  HTTPS field `request+0x0C` equals 1, and `WINHTTP_FLAG_REFRESH` /
  `WINHTTP_FLAG_BYPASS_PROXY_CACHE` (`0x100`) when request byte `+0x60` is
  nonzero; `ClientApiRequest_Reset` initializes that byte to zero. The
  `WinHttpOpenRequest` call receives null HTTP-version, referrer, and accept-
  type pointers. The C# model/test and `LoginNetwork.md` Round 31 save this
  policy. Round 50 identifies a retry-path writer for the byte; effective
  cache behavior is not established.

# Round 47: ClientApi route-worker timeout

- `ClientApi_PumpRouteWorkers` at `0xBC3F80` pumps each worker. A nonzero
  completion pointer at worker `+0x1C` bypasses timeout evaluation; otherwise
  it computes unsigned 64-bit elapsed microseconds from `timeGetTime()*1000`
  and the worker timestamp at `+0x28/+0x2C`. Elapsed time must be strictly
  greater than `0x07270E00` (120 seconds) to remove the worker.
- Removal logs, swap-removes the array entry, calls
  `ClientApi_WorkerDetachCompletionOnTeardown` (`0xBC2890`), then destroys
  the worker. That helper marks callback-context byte `+0x11` if present,
  closes and clears the WinHTTP handle at worker `+0x20`.
- No correction for `timeGetTime` wrap is visible before the unsigned
  subtraction; the C# model records the resulting underflow behavior. The
  constructor seeds the timestamp at `0xBC2831`, and the pump path rewrites it
  at `0xBC2C0F`; that rewrite's cadence and the teardown helper's cancellation
  guarantees remain unresolved. Details and sample checks are in
  `LoginNetwork.md` Round 32.

# Round 48: ClientApi route-worker pool growth

- After pumping workers, `ClientApi_PumpRouteWorkers` considers appending one
  when route field `+0x20` is nonzero, the count of workers with nonzero
  completion field `+0x1C` is at least the worker-array length, and that length
  is below `*(DWORD *)(*(DWORD *)(route + 0x28) + 0x24)`.
- It allocates a `0x30`-byte worker, initializes it through `0xBC27A0`,
  appends it with dynamic-array helper `0xBC55C0`, and pumps it once through
  `ClientApi_PumpRequestWorker`. This gate allows at most one new worker in a
  single invocation; the settings object's meaning remains unknown.
- `ClientApi_PumpRouteWorkers` is called by `ClientApi_PumpRoutes`
  (`0xBC4550`), which iterates route records and aggregates per-route return
  values. The pool gate, worker allocation, and call targets are checked in
  `LoginNetwork.md` Round 33 against the original sample.

# Round 49: ClientApi route-pump thread loop

- Routine `0xBDE510` enters its outer loop only when byte `this+5` is nonzero.
  Each iteration calls a wait function with an event at `this+172` and timeout
  3000 ms, then checks stop byte `this+4`.
- While DWORD `this+176` is nonzero, it locks `this+120`, increments
  `this+168`, calls `ClientApi_PumpRoutes` with the manager at `this+116`,
  decrements the counter, and unlocks. It repeats after the active count
  drains, subject to the outer enable gate.
- CompanionTools now checks the exact active-count compare/branch bytes at
  `0xBDE59A` as well as the surrounding gates and PumpRoutes call edge.
- The wait timeout is not evidence of a fixed 3-second pump cadence. Field
  meanings, wait identity, active-count source, and the invocation schedule of
  the separate global-lock caller `0xE99940` remain unresolved.

# Round 50: ClientApi response retry and backoff

## Observed
- Retry eligibility helper `0xBC1130` reads response kind at `+8` and status
  at `+0xC`. It returns true for kinds 2 and 4; for kind 1 it returns true
  for status 408 or 501-599 except 504. The other kinds/statuses return false.
- In `ClientApi_PumpRequestWorker` (`0xBC2B30`), the retry branch at
  `0xBC3592` compares request byte `+0x3E` with limit byte `+0x3D` using an
  unsigned comparison, then calls the helper at `0xBC359D`. When the count is
  below the limit and the helper returns true, it increments `+0x3E` at
  `0xBC35AA` and stores 1 in request byte `+0x60` at `0xBC35AF`. Reset clears
  `+0x60`, writes `0x0205` at `+0x3C` (redirect limit 5, retry limit 2), and
  clears `+0x3E`.
- The retry deadline uses imported `_time64` and imported
  `__libm_sse2_powf`: the worker loads `_time64` at `0xBC2B65`, calls it at
  `0xBC2B72`, and reuses that function pointer at `0xBC35B3`. After incrementing
  the retry count, it computes `2^count` seconds with `powf` at `0xBC35DB`,
  clamps the value to 120 seconds, adds it to the current `_time64` value, and
  stores the resulting 64-bit deadline at request `+0x48/+0x4C`
  (`0xBC35FD/0xBC360C`). At the start of a later pump, a nonzero deadline is
  compared with `_time64`; a future deadline calls `0x12CE310` at `0xBC2BF0`
  and skips the normal pump path.
- After scheduling, the worker logs `Queuing retry %u of %u.` from sample VA
  `0x1C7389C` while the count remains below the limit. When the increment
  reaches the limit, it logs `Queuing final retry %u, resetting connection.`
  from sample VA `0x1C7373C`, closes the handle at request `+0x20` if present,
  and calls `0xBC38F0`. CompanionTools checks the retry policy/helper, count
  and flag stores, reset default, time/powf imports and call sites, constants,
  deadline stores, log strings, and final connection-reset call.

## Interpreted
- Request `+0x3D/+0x3E` act as a per-request retry limit and retry counter,
  separate from the automatic redirect limit at `+0x3C`. Eligible retries
  set the byte that later contributes `WINHTTP_FLAG_REFRESH` (`0x100`).
  Backoff grows exponentially from 2 seconds per retry count and is capped at
  120 seconds.

## Unresolved
- The semantic names of response kinds 2 and 4 and the full behavior of the
  `0x12CE310` handoff are not established. The refresh flag argument is
  observed; effective cache behavior is not.
