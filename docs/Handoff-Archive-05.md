# Reverse-engineering handoff archive 05

## Awesomium switch-name conversion and ASCII getter

- Hardware capture/DirectX batch committed/pushed ascc8aa49.
- Parsed name conversion1029A9C0 feeds builder1028D7F0. Its helper10007AB0
  lowercases ASCII A-Z only over stored byte length, then recognized prefix
  length is removed before map key insertion. Lookup folds narrow key too.
- Original wide values are copied unchanged; no value lowercasing.
- Generic conversion optionally diagnoses WORD>7F (global11596AF8==1
  and helper10288E40<=3). Diagnostic destructor1029AA58 not traced; do not
  guarantee continuation or non-ASCII aliases.
- Returning conversion copies low byte per WORD using1000B350; no UTF-8.
  Separate child-type getter1028C8A0 first rejects any non-ASCII WORD via
  10299E00 and returns empty. ASCII values preserve case; empty fails the
  existing nonempty child-entry gate.
- Saved SwitchNameNativeLayout and native checks. Build succeeds;1741
  sample-backed checks and1519 pure checks pass.
- Follow-ons: diagnostic cleanup10289EC0 behavior, and DumpTruck report
  layer serialization. No target execution, sample edits or remote calls.

## Awesomium severity-three conversion diagnostic

- Switch-name batch committed/pushed asf2cdfb7.
- Diagnostic constructor stores severity3, not4. Destructor10289EC0 calls
  optional interceptor11596B1C first; nonzero AL skips built-in logging and
  severity handling. Unconsumed severity3 selects callback11596B18 or
  default display helper10288EA0. Callback implementations remain unknown.
- Default helper returns if original message empty/display-enable byte0,
  or message conversion yields empty. Otherwise builds debug_message.exe
  beside current executable and calls CreateProcessW. Success waits
  INFINITE then closes thread/process; failure shows Fatal error message
  box. No unconditional termination or exit-code observation in helper.
- Returning paths can continue to name narrowing, but callbacks, child wait
  and UI still forbid guaranteed continuation. No helpers/UI executed.
- Saved ConversionDiagnosticNativeLayout and exact guards. Build succeeds;
  1764 sample-backed checks and1536 pure checks pass.
- Follow-ons: message conversion10296210; callback installation if needed.
  Keep logger sinks and helper internals out of the saved pure model.

## Awesomium diagnostic conversion scalar boundary

- Diagnostic-routing batch committed/pushed as626ab04.
- Wrapper10296210 initializes wide output, reserves using102B7A10 and calls
  conversion10295F10, then ignores its aggregate-error byte.
- Reader102B78A0 uses shared UTF-8 multibyte decoder102B7D10 in mode-1,
  then validates scalar outsideD800-DFFF and <=10FFFF. Loop replaces
  rejected scalar withFFFD and sets sticky error0, but continues.
- Saved scalar append and UTF-16 pair arithmetic, not a byte-stream parser.
  Malformed input does not imply empty converted message. Embedded scalar0
  remains a stored WORD; later null-terminated API behavior is separate.
- Loop length comparison is signed-positive; synthetic high-bit DWORD
  lengths skip decoding only if allocation/reservation reaches that point.
- Exact malformed-byte consumption, allocator failure and helper argv
  interpretation remain unresolved. No samples/codecs/helpers executed.
- Saved DiagnosticEncodingNativeLayout and native guards. Build succeeds;
  1791 sample-backed checks and1559 pure checks pass.

## DeploymentCommon exact ranges and GPU diagnostic configuration

- Diagnostic scalar-boundary batch committed/pushed as b14b0c4.
- GPT-6 Luna High agent e77c64d8-25dd-4020-93ef-cecd1cc1ff90 resolved
  DoFileDownload MethodDef7C/body RVA38B0. Parts start at0, first end is
  min(total,10485760), not min(total-1,10485759). Later start=end+1,
  end=min(total-1,end+10485760); signed Int64 comparison and unchecked
  arithmetic preserved. DownloadObject constructor9D/body44F4 uses .part0;
  request9E/body4550 formats supplied inclusive endpoints directly.
- Corrected DownloadPlanning instead of adding a duplicate planner. Small
  positive totals request one byte past EOF; remote response is unresolved.
  Added focused evidence docs and boundary/sample checks.
- Direct Awesomium trace found threshold setter10288E70 preserves negative
  inputs and clamps signed >=3 to static fallback3. Getter10288E40 returns
  storage11596B04; another writer1028A2A0 remains untraced.
- Interceptor setter10288E50 stores/returns supplied DWORD. GPU init1019CAA0
  installs1019C9C0 only without initialization-failure byte and without
  single-process/in-process-gpu switches. Teardown1019C7F0 clears it,
  not restoring a prior pointer. Callback attempts unresolved virtual
  forwarding and normally returns AL0, so does not consume severity3.
- Build succeeds,1588 pure and1829 sample-backed checks pass. No target,
  callback, downloader, registry/hardware query or service was executed.
- Follow-ons: DownloadObject non206 status/retry decisions; other threshold
  writer1028A2A0 or explicit setter caller10033BC0. These are not completed.

## Awesomium logging setup precedence

- Range/GPU-interceptor batch committed/pushed as68603b7.
- Traced setup10033BC0: ASCII getters for type/log path/log-level,
  numeric converter output initialized0 but success byte ignored,
  disable-logging tested by presence. Normal logger-init return is followed
  unconditionally by log-level setter10288E70, regardless of init success.
- Init1028A2A0 receives destination0/1, argument3=0, append=nonempty ASCII
  type and diagnostic-check mode0. The mode goes to11596AF8, so optional
  mode1 diagnostics are disabled by this observed caller.
- Corrected prior "other writer" shorthand:1028A51B passes address11596B04
  to verbose constructor102B5720. Nonempty v plus successful numeric parse
  writes unchecked negated result through that address, but the outer
  log-level setter subsequently overrides it on normal return.
- Numeric failure can retain a prefix value or saturate signed Int32.
  Outer setup uses that output even on failure; verbose constructor
  requires success. Saved observation-based decisions, not a byte parser.
- Added LoggingSetupNativeLayout and focused docs/native checks. Build
  succeeds;1637 pure and1882 sample-backed checks pass. No logger, file
  deletion/open, mutex, target, callback or services executed.
- Agent1eee0217-8d6a-414a-9e36-3b30e6ad8841 is assigned a bounded independent
  DownloadObject response/retry trace. Do not mark complete before receiving,
  integrating and validating its report.
- Further logger follow-ons: verbose module match semantics and actual
  file-opening helper10289C70; no implementation or completion claimed.

## DeploymentCommon response and shared retry boundary

- Logging setup batch committed/pushed as17e754f.
- Agent1eee0217-8d6a-414a-9e36-3b30e6ad8841 finished its response trace;
  integrated DownloadResponseNativeLayout and evidence docs.
- Method9E/bodyRVA4550: observed status206 reads stream directly, status200
  sleeps1500ms then increments local6 and retries when incremented count<=10.
  Other statuses log and read only if GetResponse returned normally; HTTP
  error exceptions before this boundary are not modeled or suppressed.
- Final MD5 mismatch increments the same local6 and uses the same strict
  >10 terminal comparison. Counter starts0 outside retry loop, not reset
  between attempts. Saved pure decisions perform no request/sleep/hash.
- Build succeeds;1663 pure and1910 sample-backed checks pass. No target
  assembly loaded/executed, no chunk files or network requests created.
- Both recently assigned agents are finished; no active assignment remains.
- Follow-ons remain logger file opening10289C70, verbose module matching,
  or downloader outer exception propagation; none marked complete.

## Awesomium log-file opening

- Response/retry batch committed/pushed as0dcf696.
- Open helper10289C70 returns true for any already nonzero stored handle,
  even syntheticFFFFFFFF. Zero first prepares path, then destinations1/3
  request file opening; other destinations return true without an open.
- Configured/fallback attempts use GENERIC_WRITE, share3, OPEN_ALWAYS4,
  normal attributes80. Zero/FFFFFFFF configured result tries .\debug.log.
  Accepted handle seeks0 from FILE_END2, ignores result and returns true.
  Failed fallback clears storage11596B0C and returns false. No close here.
- Default builder10289A20 uses GetModuleFileNameW260 with ignored return,
  scans WCHAR terminator, truncates after last backslash when found, then
  appends debug.log. Missing backslash does not add a separator. Allocation
  failure/path API errors remain unresolved; no safe fallback invented.
- Saved LogFileNativeLayout and focused docs plus native guards. Build
  succeeds;1703 pure and1954 sample-backed checks pass. No log/files/API
  execution. True does not prove a valid stored handle or successful seek.
- Next bounded trace: destructor10289EC0 actual log-file write and error
  return handling, or independent downloader outer exception propagation.

## Awesomium log sink admission and writes

- File-opening batch committed/pushed as0e0cd83.
- Unconsumed destructor10289EC0: destinations2/3 call OutputDebugStringA
  and stderr regardless of severity; others call stderr only signed
  severity>=2. Destination0 therefore does not suppress all diagnostics.
- stderr uses fprintf("%s") and fflush on CRT array11553420+40, whose
  static FILE descriptor is2. Results ignored. File block admits any
  destination except0/2, attempts synchronization, calls open helper,
  checks only AL, then seeks EOF again and writes stored string byte length.
- WriteFile BOOL and bytes-written local ignored, no partial-write retry,
  FlushFileBuffers, close or result-driven failure branch in this path.
  Null-terminated debugger/stderr output differs from length-based write.
- Synthetic unknown destinations can enter file block while zero-handle
  open helper skips actual open and returns true; do not normalize them
  or equate attempted output with delivery.
- Saved LogSinkNativeLayout, focused docs and native guards. Build succeeds;
  1835 pure and2089 sample-backed checks pass. No target/sink/filesystem or
  synchronization API executed.
- Follow-ons: mutex name/setup10289AE0 (default path or caller path,
  backslash normalization, Global prefix, initialized flag/failure branch);
  downloader outer exception propagation. No active agent assignment.

## Awesomium logger synchronization setup

- Sink-admission batch committed/pushed as8edea89.
- Initializer10289AE0 skips changes when byte11596B20!=0. Otherwise stores
  supplied mode11596B28; nonzero mode attempts24-byte critical-section
  allocation, zero mode reuses nonzero mutex11596B2C or builds/creates one.
- Null critical allocation stores pointer0 yet marks initialized1. Mutex
  creation0 stores0 and returns before flag assignment, allowing later retry.
  Existing/nonzero mutex is not validated. Flag is not an interlocked once.
- Nonnull path (even empty) copied via WCHAR terminator; null invokes
  default builder10289A20. Copied path backslashes become forward slashes,
  exact Global\ prefix prepended; case/colon/dot segments untouched.
- CreateMutexW null security/initial ownerFALSE. Only zero is failure; no
  GetLastError or existing-object branch. Saved names are observed copied
  strings, not Win32 acceptance guarantees or a real lock implementation.
- Build succeeds;1881 pure and2139 sample-backed checks pass. No target,
  API, file or mutex executed/created.
- Agent0c7dcf0a-3f05-4c4e-974a-fd9bdbe8191e is assigned immediate managed
  downloader worker/caller exception propagation. Await its report before
  completing that task; no outcome claimed yet.

## DeploymentCommon worker exception propagation

- Synchronization batch committed/pushed as c82c959.
- Agent0c7dcf0a-3f05-4c4e-974a-fd9bdbe8191e finished. Integrated
  DownloadWorkerNativeLayout and focused worker evidence docs.
- ThreadRunner Method7D/bodyRVA3E10 catches System.Exception around
  DoDownload: WebException plus global recoverable count<10 and either
  status14 or exact message "test" requeues/reset event and increments
  part/global recoverable counters. This is separate from row9E's shared
  response200/MD5 retry counter.
- Nonqualifying exception records first fatal slot under lock, stops
  running, aborts other workers and signals queued events. Already occupied
  fatal slot is preserved. Terminal custom range exception is not WebException.
- DoDownload finally marks complete/signals supplied event. Outer row7C
  waits events/workers, logs captured error and throws retrieved exception
  object, not IL rethrow. Empty slot only permits later assembly; it does
  not guarantee all later filesystem/reporting operations succeed.
- Build succeeds;2122 pure and2386 sample-backed checks pass. No target,
  worker threads, network, files or callbacks executed.
- Agent assignment complete; no active agent assignment remains.
- Follow-ons: Awesomium verbose rule matching102B53A0, or managed worker
  cancellation/Thread.Abort aftermath. Neither is captured as completed.

## Awesomium verbose rule source and precedence

- Worker-propagation batch committed/pushed as4192d02.
- Lookup102B53A0 scans36-byte records forward and returns first matching
  rule level+1C; no-match/empty rules negate live threshold via object+10,
  unchecked. Kind+20 exactly1 selects full original source; otherwise module.
- Pattern containing either slash creates kind1 (constructor102B58A7);
  append102B5650 preserves order/level/kind. Source module102B5210 strips
  through last slash/backslash, last-dot extension, then exact "-inl".
  No Unicode decode, lowercase, filesystem canonicalization or NUL truncation
  in saved observed byte-span transformation.
- Matcher102B52B0 literals case-sensitive, separator spellings equivalent,
  question mark consumes one byte, star recursively tries suffix positions.
  Saved only match-observation selection, not wildcard/rule-text parser.
- Added VerboseRuleNativeLayout and focused docs. Build succeeds;
  2146 pure and2414 sample-backed checks pass. No target/logger execution.
- No active agent assignment. Follow-on: numeric failure while constructing
  per-module rule still appends output; rule-text helper102A0670 split and
  empty/duplicate handling unresolved. Keep prior snapshot-state2-writer
  task pending; no new evidence resolves it.

## Awesomium verbose construction error retention

- Verbose lookup batch committed/pushed as3b7c8b9.
- Followed102A0670 ->102A03F0 ->102A0150: comma fragments trimmed on both
  edges using exact bytes09/0A/0B/0C/0D/20. Empty trimmed fragments skipped.
  Pair102A04D0 clears outputs, first "=" separates key, consecutive "="
  skipped before one remaining value suffix. No independent key/value trim.
- Pair error sets sticky false but still appends retained/empty outputs.
  Constructor likewise continues with produced pairs despite aggregate false.
  Numeric conversion102B58E6 status controls diagnostic cleanup only,
  both outcomes append the observed output level at102B5911. Duplicate
  patterns remain append-ordered; lookup earlier first-match wins.
- Saved evidence docs/ledger and native guards, deliberately no parser.
  Build succeeds;2146 pure and2418 sample-backed checks pass. Derived text
  examples are static deductions, not runtime parser captures.
- Agentbdfbfb74-e45b-490e-baa3-793c603663af has bounded managed downloader
  cancellation/cleanup assignment; await and integrate report. No claim
  that Thread.Abort means completed cancellation or disposed resources.
- Next static follow-ons: verify verbose module levels relative to caller
  source-span construction1028A790; allocator/aliasing behavior if required.

## DeploymentCommon cancellation and bounded cleanup

- Verbose construction batch committed/pushed as f9c2dc8.
- Agent bdfbfb74-e45b-490e-baa3-793c603663af completed cancellation trace.
  Integrated DownloadCancellationNativeLayout and focused evidence document.
- First fatal worker clears mIsRunning, requests Thread.Abort only for
  other enumerated threads, drains queue and signals ManualResetEvent
  values. ThreadAbortException skips ordinary diagnostic block, but CLR
  automatic rethrow behavior is not suppressed or modeled as graceful exit.
- Outer row7C waits individual events then live workers. Timer finally
  try28D/69 handler2F6/C disposes nonnull timer local; no whole-method
  timer cleanup and no individual wait-handle disposal in that method.
  Dispose does not establish completion of an in-flight timer callback.
- Added exact finally-region metadata matcher to SampleImage and checks.
  Corrected agent-proposed queue-drain guard against immutable bytes:
  actual span has one getter/type-test, not duplicated getter sequence.
- Build succeeds;2150 pure and2428 sample-backed checks pass. No workers,
  targets, timers, cancellation requests, network or filesystem APIs run.
- No active agent assignment remains. Follow-ons: verbose source-span
  caller1028A790 and negative threshold fallback arithmetic; cancellation
  effects beyond immediate managed boundary remain unresolved.

## Awesomium verbose caller extent and fallback arithmetic

- Cancellation batch committed/pushed as173681b.
- Level helper1028A790 mode==1/minimum<=3/extent0 can diagnose N > 0U,
  then continues on returning paths and reloads threshold. Configured path
  decrements supplied extent modulo32, not strlen; synthetic0 yieldsFFFFFFFF.
- Representative caller1006D799 passes41-byte source literal with extent42,
  then signed level>=20 admits severity-20 diagnostic. Not generalized to
  every caller or malformed pointer validity.
- Unconfigured fallback is max(unchecked(-minimum),-1), verified NEG then
  CMP/JG flags. Configured no-match fallback remains raw negation, so3
  gives-1 versus-3, Int32.MinValue gives-1 versusInt32.MinValue.
- Saved VerboseLevelNativeLayout and updated evidence docs/native guards.
  Build succeeds;2224 pure and2505 sample-backed checks pass. No target,
  source-pointer dereference, diagnostic, callback or logger executed.
- No active agent assignment. Follow-ons: named mutex collisions and
  initialization reset writers (xref11596B20), or client-side logging
  bridge into Awesomium; retain unresolved state2 snapshot writer.

## Awesomium synchronization reset xref closure

- Caller-extent batch committed/pushed as7ee9dba.
- Direct xrefs in this Awesomium sample: initialized byte11596B20 is tested
  and set1 only in initializer10289AE0; no direct reset writer found.
  Critical-section pointer11596B24/mutex11596B2C are initialized and used
  for enter/leave or wait/release; no direct close/destroy/reset xref found.
  This is scoped to direct refs in this image, not proof about indirect
  writes or code outside it.
- Corrected distinction: reconfiguration1028A2A0 closes global log file
  handle11596B0C at1028A684 and stores0 at1028A68A. It does not close
  mutex or free critical-section object. Destinations0/2 retain stored
  path object; other destinations replace it.
- Added lifecycle decisions/docs and instruction guards. Build succeeds;
  2238 pure and2521 sample-backed checks pass. No target, file, logger,
  mutex or critical-section API executed.
- Remaining global reset/concurrent mutation via indirect stores is
  unresolved. Do not equate missing direct xrefs with proof of impossibility.

## FirefallClient Red5 logging-method setup

- Added an inert C# decision model and evidence checks for client raw
  initializer `0xD668D0`, plus a focused evidence doc.
- On the original client image, the initializer returns ready when byte
  `this+0x73` is nonzero; returns unavailable without retry when `this+0x73`
  is zero and `this+0x72` is nonzero; otherwise attempts setup. It requests
  the `Red5` JS object and, after validity succeeds, registers `errorlog` via
  imported `JSObject::SetCustomMethod` with trailing argument0, then sets
  `this+0x73`. Failure selects a bounded string index 0..7, logs
  `Unable to create Red5 JS object: %s`, and sets `this+0x72`.
- `SetCustomMethod` has two observed sites: raw function `0xD64530` and
  initializer `0xD668D0`. The other setup-gated call in raw function
  `0xD651B0` is `JSObject::SetPropertyAsync` via IAT slot `0x1AC4EF0`, not a
  third `SetCustomMethod` site. The separate WebPreferences constructor has
  one direct call at raw `0xCBC1B1`; its adjacent initialization is not
  evidence of a logging preference.
- Checked the call sites, decorated imports, UTF-16 object/method names and
  ASCII failure format against `FirefallClient.exe.original`. Build succeeds;
  2256 pure and2543 sample-backed checks pass. No browser, script, logging
  API, or callback was run.
- Follow-up: identify the mapped callback queue consumer and the
  JavaScript-side call site. The pending state2 snapshot-writer question
  also remains open.

## FirefallClient Red5 callback routing

- Corrected the Red5 call-site record: the call in raw function `0xD651B0`
  uses `JSObject::SetPropertyAsync`; only the raw `0xD64530` and `0xD668D0`
  sites observed here call `JSObject::SetCustomMethod`.
- RTTI associates the `JSMethodHandler` subobject table at VA `0x1C7656C`
  with raw callback function `0xD65A60` (VA `0x1166660`). The handler gets
  the saved JSObject remote ID through IAT VA `0x1AC4F00` and compares it
  against the incoming ID at raw `0xD65AF9`; mismatch enters the unknown
  callback logger.
- On the fallback path after the mapped-object lookup, raw `0xD65A60`
  compares the incoming method name with UTF-16 `errorlog` at VA `0x1BD4994`
  via `WebString::operator==` (IAT VA `0x1AC4EA4`). If no argument is
  present, or argument zero is not a string, it returns without message
  logging. Otherwise it reads argument zero, verifies `JSValue::IsString`,
  converts with `JSValue::ToString`, and calls helper raw `0x2FC4F0` with
  format `%s - %s` (VA `0x1BD498C`) and logger object VA `0x1E68688`.
  The first format value is the frame label when non-null, otherwise the
  empty-string pointer VA `0x1C5EA68`.
- Unknown names use format VA `0x1BD48F0` and logger object VA `0x1E68668`
  through helper raw `0x2FCC60`. No target or callback was executed.
- The mapped path uses an ordered string-keyed tree rooted at offset `+0x474`
  of the enclosing object. Raw `0xCCD640`/`0xCCDC70` compare the method key
  with `strcmp`; a lookup miss or second sentinel check falls through to the
  `errorlog` special case. On a match, raw `0xD65A60` creates a 12-byte
  record with retained method-name text, a `JSValue`, and a `JSArray`; the
  lookup result itself is not retained in the record.
- For nonempty arrays, the handler checks the final argument; if it is an
  object, raw `0xD64320` invokes a cached predicate from ASCII source VA
  `0x1BD4570`. The predicate requires truthy `object`, `constructor`, `call`,
  and `apply` properties. A defined, truthy result causes the handler to
  separate that final value and pop it from the copied array. Otherwise it
  preserves all arguments. Raw `0xD66B70` appends the record to the handler's
  collection at `this+0x34`.
- Added callback route and trailing-callable-like-argument decision models,
  plus sample guards for the lookup, predicate, array pop, and queue append.
  At that point, the queue consumer and actual JavaScript caller of
  `Red5.errorlog` remained unresolved.
- Validation: solution build succeeded with 0 warnings/errors; all 2556
  CompanionTools checks passed, including original-sample checks; `git diff
  --check` is clean.
- Validation: solution build succeeded with 0 warnings/errors; all 2552
  CompanionTools checks passed, including original-sample checks; `git diff
  --check` is clean.

## FirefallClient Red5 callback queue dispatch

- Corrected the queued record interpretation: its first field is retained
  method-name text, not a retained tree lookup result. Consumer raw
  `0xD64AE0` uses that key to perform a fresh lookup in the mapping tree.
- RTTI identifies `0xD64AE0` as virtual slot20 of the primary
  `uiWebFrameInternal_17` vtable (table VA `0x1C765FC`, slot VA
  `0x1C7664C`; complete-object locator VA `0x1CD0FE0`, object offset0).
- The consumer drains the queue at `this+0x4C` while the pending count at
  `this+0x5C` is nonzero. A missing current mapping discards that queued
  callback. Otherwise it retrieves the mapped Lua state/reference, converts
  the JavaScript arguments through `0xCBBE00`, and dispatches the Lua
  callback via adapter `0x1029540` inside wrapper `0xCD6C10`. The `JSValue`
  in the record is a separate, optional JavaScript continuation.
- An undefined continuation, tested through imported `JSValue::IsUndefined`
  at IAT VA `0x1AC4EE8`, skips only that JavaScript continuation. When
  defined, Lua return values are marshalled into a `JSArray` and passed to
  its `call` method with a null receiver.
- Global VA `0x1E686C4` offset `+8` selects imported
  `JSObject::InvokeAsync` at IAT VA `0x1AC4EEC` when nonzero, or synchronous
  `JSObject::Invoke` at `0x1AC4EDC` otherwise. The synchronous path logs the
  return value; conditional diagnostics include the callback name and
  argument strings.
- Added an inert decision model and original-sample guards for the vtable
  slot, queue count, fresh method lookup, `call` method string, both invocation
  imports, and diagnostic formats. The JavaScript caller of `Red5.errorlog`
  remains unresolved.

## FirefallClient Red5 bridge teardown

- The deleting-destructor wrapper at raw `0xD64180` calls `0xD64250`; that
  destructor calls cleanup routine `0xD64710` before destroying its member
  containers.
- Cleanup drains the pending queue at `this+0x4C` with count at `this+0x5C`.
  Each queued record is passed to raw `0xD648A0`, which destroys its
  `JSArray` at `+8`, destroys its `JSValue` at `+4`, and decrements/releases
  the retained method-name text at `+0`. The queue item is then freed. This
  loop does not call `JSObject::Invoke` or `InvokeAsync`.
- Cleanup resets `this+0x24` to `-1`. The pointer at `this+4` is compared
  with shared sentinel `[dword_1E002B8+0x5D0]`; a distinct non-null pointer
  receives virtual slot0 with argument1 and is then cleared, while the
  sentinel value remains untouched. Its semantic type is unresolved.
- Added a decision model and sample guards for the destructor relationship,
  queue-record release, field destructors, and pointer sentinel handling.

## FirefallClient Lua/web bridge source check

- Installed `system\gui\components\Test\WebTest.lua` loads local
  `awesomium_test.html`, registers `myfunc`/`MoveMe` with `AddWebCallback`,
  exposes several `AddWebValue`s, and returns a nested Lua table from
  `TestCallback`. The HTML calls `Red5.myfunc` with a trailing JavaScript
  callback, calls `Red5.MoveMe`, and defines `Red5Callbacks.test_from_lua`
  for Lua's `CallWebFunc`. This establishes the generic bidirectional test
  contract, but does not show an `errorlog` call. The native
  `AddWebCallback` implementation is traced below.
- The main UI Lua source reads `ingame_host` and loads
  `host .. "/panelmanager"`; it also loads its selected panel URL in that
  frame. A case-insensitive search of the installed `system\gui` text/script
  sources found no `errorlog` call. The actual caller remains unresolved;
  dynamically loaded page code is a possibility, not confirmed evidence.

## FirefallClient AddWebCallback Lua registration and dispatch

- Raw `0xCCAD70` is the Lua `AddWebCallback` binding. It validates the
  callback argument, obtains a callback reference through `0xC56170`, and
  calls `0xCCC130` with the method name, Lua state, and reference.
- Registration helper `0xCCC130` requires a nonzero field at `this+0x418`,
  rejects reference values `-1` and `-2`, and rejects names containing `'`
  or `\`. It stores the Lua state and reference in the method tree at
  `this+0x474`; replacement releases the old reference through `0xC56250`
  before removing and replacing the entry.
- The helper calls virtual slot `+0x30` on the object at `this+0x41C`. A
  vtable at VA `0x1C765FC` shares the `uiWebFrameInternal_17` RTTI locator
  and has `0xD64530` at slot VA `0x1C7662C`; `0xD64530` gates and calls
  `JSObject::SetCustomMethod`. The field's dynamic vptr assignment was not
  independently located, so this is a strong slot match rather than a fully
  closed runtime pointer trace.
- Queue consumer `0xD64AE0` converts JavaScript arguments through
  `0xCBBE00`, then dispatches the registered Lua callback via adapter
  `0x1029540` inside wrapper `0xCD6C10`. The queued `JSValue` at `+4` is the
  optional trailing JavaScript continuation, not the registered Lua
  callback. The consumer invokes that continuation with marshalled Lua return
  values when it is defined; the undefined check skips only the continuation.
- Updated the inert C# decisions, sample guards, and Red5 bridge evidence
  document to reflect registration/replacement and the two-stage
  JavaScript-to-Lua-to-JavaScript flow. The installed `WebTest.lua` and HTML
  corroborate the generic callback flow, but do not show a call to
  `Red5.errorlog`; its JavaScript caller remains unresolved.
- Raw `0xCCAF10` is `AddWebValue`: it accepts string/numeric property names,
  dispatches through frame virtual slot `+0x54` to `0xD651B0`, marshals the
  value via `0xCBBDB0`, and uses `JSObject::SetPropertyAsync`.
- Raw `0xCCB060` is `CallWebFunc`: it dispatches through virtual slot `+0x58`
  to `0xD65250`, which resolves `Red5Callbacks`, checks the named function
  with `JSObject::HasMethod`, marshals Lua arguments into a `JSArray`, then
  selects `InvokeAsync` or `Invoke` using the shared `0x1E686C4+8` state.
  The local WebTest pair corroborates the `Red5Callbacks` target. Both
  bindings and their invocation decisions are recorded in the inert C# model
  and sample-backed checks; no executable or script was run.
- Raw `0xCCB9A0` registers twelve Lua WebFrame names from `LoadUrl` through
  `IsTypeTarget`. Twelve small stubs at `0xCCE8B0`-`0xCCE960` forward to
  their respective implementations. The names, stub bytes, and targets are
  recorded in the bridge document, along with traced URL, filter, dimension,
  logging, and type-target behavior.
- `SetUrlFilters` (`0xCCB1C0`) replaces the list at frame offset `+0x468`;
  `0xCCCB20` retains only nonempty patterns ending in `*` using `strncmp`,
  logging other values. `LogWebRequests` (`0xCCB3D0`) stores Lua truthiness
  at frame offset `+0x458` despite a boolean usage string. `ClearCookies`
  (`0xCCB4E0`) calls internal-object slot `+0x68`, resolved by the object
  constructor to thunk `0xD65A10`. The thunk calls nested-object slot `+0x04`;
  that final method remains unresolved. Its missing-argument usage string
  incorrectly names `LogWebRequests`.
- `LoadUrl` (`0xCCABA0`) converts numeric URLs to text and calls associated
  object slot `+0x5C` when the object at frame offset `+0x418` exists; it
  pushes `true` after dispatch and pushes no value when that object is null.
  `GetUrl` (`0xCCACC0`) returns the pointer at `+0x448` or an empty string.
- `SetNaturalDims` (`0xCCB5B0`) writes float dimensions at `+0x45C/+0x460`
  and calls `0xCCC5C0` only on change; `GetNaturalDims` (`0xCCB710`) returns
  both fields. `EnableTypeTarget` (`0xCCB7C0`) applies Lua truthiness via
  `0xCCD0F0`, which updates flag `+0x459` and may synchronize the active
  target; `IsTypeTarget` (`0xCCB8C0`) returns that flag. Its usage string
  misleadingly advertises a boolean argument that its code does not read.
- `LoadUrl`'s vtable slot `+0x5C` resolves to `0xD65690`. This handles
  `local://` specially using a suffix and base-path helper chain; ordinary
  URLs use `0x2EE810`, and both paths call nested-view slot `+0x64`. The
  natural-dimension helper computes a clamped, rounded scale percentage and
  synchronizes nested-view slots `+0x38/+0x3C` only when the value changes.
- Stored URL filters are consumed by `0xCCCC10`, which treats the final `*`
  as a prefix wildcard and uses case-sensitive `strncmp` for the preceding
  bytes. Callback `0xCBDDE0` returns false for an unchanged context key;
  otherwise it negates the match result. Constructor `0xCBC410` installs the
  table at VA `0x1C747B8` as an object's secondary vtable at offset `+0x04`;
  the callback occupies its second slot. Setup routine `0xBFB310` gets the
  object through factory `0xCBB7A0` using owner fields `+0x9A4/+0x9A8`,
  stores it at `+0xA04`, then calls primary slot `+0x08`. Its mapped callback
  VA is `0x10BE9E0`; the owner's API type and callback allow/cancel semantics
  remain unresolved. Owner cleanup `0xBF8AC0` calls the `+0xA04` object's
  primary slot `+0x10` with `1`, then clears that field. The slot at VA
  `0x1C747DC` points to `0xCBC4D0`, which calls reset `0xCBC510` and uses the
  low argument bit to select final release. Owner cleanup also releases and
  clears the shared-text values at `+0x9A8/+0x9A4`. Their meanings remain
  unknown. Setup copies its incoming path into owner field `+0x5C8`, adds a
  trailing separator as needed, and converts `/` to `\`; empty input uses
  `"./"` (`0x1BBA0FC`). Empty `+0x9A4` uses `"web"` (`0x1BBA15C`) composed
  with that base; empty `+0x9A8` uses `"./"` (`0x1BBA160`). These support a
  root/web-subdirectory interpretation, but the API purpose remains unknown.
  Secondary slot `+0x00` is raw
  `0xCBD2C0`; its normal
  return path sets `EAX=0` and consumes one stack argument after substantial
  object processing. Its contract remains unidentified. Callback `0xCBD2C0`
  passes text from incoming-object virtual methods to
  parser `0xBCE800`. The parser uses `strncpy` with a 2048-byte bound,
  recognizes `http://`/`https://` case-insensitively via `_stricmp`, defaults
  prefixless text to HTTP, parses optional ports with `atoi` (valid 1–65535,
  default 80/443), and fills output offsets `+0/+4/+8/+0x0C/+0x10` with
  secure flag, host, port, slash-prefixed path, and query. It removes the
  fragment before splitting the query, but only when a slash path exists;
  without a slash, query/fragment text remains part of the parsed host. The
  first-colon port split does not support IPv6 literals. Builder `0x12E4100`
  applies bytewise component escaper `0x12E4810` to parsed host/path and a
  configured identifier. It preserves ASCII alphanumerics and `-._~`, and
  emits uppercase `%HH` for other bytes. A separate local formatter uses
  `"Red5 %s %s %s"` at VA `0x1C576D0`; this alone does not establish network
  transmission. These details are sample-guarded; callback meaning remains
  unresolved. The primary table at file offset `0x18733CC` maps slot
  `+0x08` to VA `0x10BCD80` (raw `0xCBC180`). That method calls `0xCBC270`,
  invokes the helper at object `+0x08` twice using configuration at `+0x14`,
  stores results at `+0x0C/+0x10`, then conditionally calls primary slot
  `+0x04`. Setup passes flag `2` only if both its stack-byte argument and
  owner byte `+0x5C` are nonzero; else it passes `0`. Primary slots `+0x00`
  and `+0x04` point to `0xCBC150` and `0xCBC160`, which call virtual slot
  `+0x18` on the nonnull helper objects at `+0x08`, and `+0x0C/+0x10`,
  respectively. They appear to release those helpers; interface semantics
  remain unknown. Constructor capacity field `+0x2C` starts at `7`; `CBC270`
  enters only for nonzero mode and storage pointer `+0x28`. For mode other
  than `1`, it sets pending byte `+0x68` when helper `+0x08` exists and result
  `+0x0C` is absent. Reset routine `0xCBC510` (called by wrapper `0xCBC4D0`)
  calls `0xCBC270` with mode `2` only when `+0x68` is set, then clears it.
  It invokes cleanup on nonnull members `+0x0C/+0x10`, indirectly releases
  nonnull `+0x08`, calls `0xCBE080` with member `+0x48`, and conditionally
  passes buffers `+0x30/+0x18` to `0x12C8D60` when capacities are at least
  `8`. It restores observed capacities to `7`, clears pointer/length fields,
  invokes an unresolved initializer on `+0x14`, and leaves primary vtable
  `0x1C747E4` (the secondary-vtable source is unresolved). Wrapper
  `0xCBC4D0` always resets first and tests argument bit 0 for allocation
  release. Sample-backed assertions cover this cleanup sequence; exact
  helper identities and the user-facing meaning remain unresolved. The
  `0xCBE080` list helper passes an optional pointer at list `+0x08` to the
  shared allocator release path, clears it and `+0x0C/+0x10`, self-links the
  sentinel at `+0x00`, clears count `+0x04`, walks forward links releasing
  nodes, and tail-jumps to the release path with the sentinel. This matches
  circular doubly-linked-list cleanup; exact types remain unknown. Constructor
  helper `0xCBE310` allocates its `0x14`-byte sentinel through `0xCBF080`, then
  seeds 16 copies of that sentinel into a vector-like submember via
  `0xCBEE50`. The pool-like role and later values `7/8` are unresolved.

# Round 23: Auto-login AES key setup

## Observed
- `Login_StartAutoLogin` (`0x63DE80`) calls `0x12E3960` on accepted branches
  with a non-empty input string. The callee stores that string at
  object offsets `+0x3C` and `+0x48`, then clears `+0x40`. These offsets match
  the first signing-key field, `uid`, and optional second signing-key field
  read by `0x12E4100`, but identity between the object instances used by the
  login and ClientApi paths is not established.
- `0x12E3960` hashes two separate suffixed forms of the input with CryptoAPI
  SHA-1 (`CALG_SHA1`, 20-byte digest). The second digest is converted into a
  28-byte `PLAINTEXTKEYBLOB` for `CALG_AES_128` (`0x660E`): its first 16 bytes
  become the AES key. `CryptSetKeyParam` with parameter 1 sets the first
  digest as the IV. The key object is stored at `this+0x58`. Embedded suffix
  values are intentionally not reproduced in the source reconstruction.

## Unresolved
- The AES mode, the consumer of the imported key/IV, and the ownership/lifetime
  of the key object remain unknown. The relationship between these derived
  values and the optional second ClientApi HMAC key is also unresolved.

# Round 24: AES buffer helpers

## Observed
- `0x12E4680` passes the AES key object at parent offset `+0x58` to
  `0x12EF0D0`. That helper calls CryptoAPI `CryptDecrypt` on mutable blob
  storage. The former "configured block-size" interpretation is superseded:
  the later verified publication trace below identifies address alignment,
  not cipher block size.
- `0x12EF1E0` contains a corresponding `CryptEncrypt` call on the same key
  wrapper type. Neither `0x12E4680` nor `0x12EF1E0` has direct code xrefs in
  the current IDB; their higher-level callers may be indirect or unrecognized.

## Unresolved
- The AES mode, the encryption/decryption dispatch path, and the application
  data protected by these wrappers are not established. Do not model cipher
  output until the stream contract and dispatch caller are resolved.

# November 4 continuation: verified import and receiver mappings

The following checks used the active `FirefallClient.exe` image, 40,197,848
bytes, SHA-256
`97362D668FF64778193A64759265E4AE90842043C4A15B627DF9902FD9135445`.
They did not use `FirefallClient.exe.original`. Analysis was read-only; no
executable execution, patches, or IDB changes were made.

## CryptoAPI imports: correct the raw-address interpretation

The absolute operands `0x1AC4004` through `0x1AC406C` are PE virtual
addresses, not raw offsets. Reading raw `0x1AC4000` samples `.rsrc`, not the
IAT. Its repeated `07`/`06` bytes do not establish unresolved or dynamically
resolved imports. The empty IDA import enumeration is also not evidence that
the PE import table is empty.

The PE header is at raw `0x170`, image base is `0x400000`, and `.rdata`
starts at RVA `0x16C4000`, raw `0x16C2C00`. The six slots are ordinary
`ADVAPI32.dll` imports:

| IAT VA | RVA | Raw offset | Import |
| --- | --- | --- | --- |
| `0x1AC4004` | `0x16C4004` | `0x16C2C04` | `CryptDecrypt` |
| `0x1AC4008` | `0x16C4008` | `0x16C2C08` | `CryptDuplicateKey` |
| `0x1AC400C` | `0x16C400C` | `0x16C2C0C` | `CryptSetKeyParam` |
| `0x1AC4014` | `0x16C4014` | `0x16C2C14` | `CryptAcquireContextA` |
| `0x1AC403C` | `0x16C403C` | `0x16C2C3C` | `CryptImportKey` |
| `0x1AC406C` | `0x16C406C` | `0x16C2C6C` | `CryptEncrypt` |

The import directory is RVA `0x19BCDCC`, raw `0x19BB9CC`, size `0x280`.
Its ADVAPI32 descriptor at raw `0x19BBB20` has OriginalFirstThunk RVA
`0x19BD04C`, DLL-name RVA `0x19C0A32` (raw `0x19BF632`), and FirstThunk
RVA `0x16C4000`. The DLL name is `ADVAPI32.dll`; its 31 imports end with
a null thunk. This does not exclude unrelated dynamic resolution or runtime
IAT replacement elsewhere.

Exact six-byte call starts, all raw offsets:

| Callsite | Instruction bytes | API |
| --- | --- | --- |
| `0x12EF15A` | `FF 15 04 40 AC 01` | `CryptDecrypt` |
| `0x12EF103` / `0x12EF238` | `FF 15 08 40 AC 01` | `CryptDuplicateKey` |
| `0x12EF0A0` | `FF 15 0C 40 AC 01` | `CryptSetKeyParam` |
| `0x12EEF8E` | `FF 15 14 40 AC 01` | `CryptAcquireContextA` |
| `0x12EEFBE` | `FF 15 3C 40 AC 01` | `CryptImportKey` |
| `0x12EF298` | `FF 15 6C 40 AC 01` | `CryptEncrypt` |

The bounded mode/lifetime continuation verified the following:

- Constructor `0x12EEF20` imports `CALG_AES_128` (`0x660E`) with a 16-byte
  key. In constructor, IV setter, encrypt/decrypt, and inspected parent
  construction/replacement paths, no explicit `KP_MODE=4` or padding setter
  was observed. Setter `0x12EF0A0` uses `KP_IV=1` (`6A 01` at
  `0x12EF09B`). A whole-image literal-call scan additionally found import
  jump thunk `0x15A0CD0` (`FF 25 0C 40 AC 01`), with no recorded IDA xrefs.
  These checks do not exclude arbitrary indirect callers. CBC/default
  padding remains a provider-default interpretation, not an observed
  application setting.
- Decrypt duplicates the key at `0x12EF103` and passes that duplicate to
  `CryptDecrypt` (`FF 75 FC` at `0x12EF157`). Encrypt duplicates at
  `0x12EF238`, but reloads the original wrapper at `0x12EF28B`
  (`8B 45 F4`) and passes its original key at `0x12EF295` (`FF 70 04`).
  Both paths destroy their duplicates on success and failure. Both crypto
  calls use `Final=TRUE`; original-key encryption alone therefore does not
  establish cross-message CBC chaining.
- Parent `+0x58` is a pointer to an allocated eight-byte key/provider
  wrapper, not inline handles. Replacement calls destructor `0x12EF000`
  at `0x12E3B14`, frees the old allocation at `0x12E3B1B`, stores the new
  pointer at `0x12E3B23`, then sets its IV at `0x12E3B29`. Other replacement
  paths occur at `0x12E4089..0x12E40A1` and through `0x12E4EA0`.
- Explicit reset `0x12E37A0` destroys/frees the wrapper at
  `0x12E386E/0x12E3875` and clears the pointer at `0x12E387A`
  (`C7 46 58 00 00 00 00`). Owner teardown `0x31C3A0` destroys/frees it at
  `0x31C438/0x31C43F`. Wrapper destructor `0x12EF000` destroys its key
  handle at `0x12EF00B`, clears wrapper `+4`, releases the provider at
  `0x12EF039`, and clears wrapper `+0`.

Next bounded task: establish the parent type and actual instance provenance
between auto-login key setup and ClientApi signing. Cipher consumers remain
unresolved.

## PrivateDialog receiver identity and numeric event 229

Keep selector `0x79` separate from selector `0x59`. For the former,
dispatcher entry VA `0xC1D840` / raw `0x81CC40` points to VA `0xC1D663` /
raw `0x81CA63`. The call at raw `0x81CA82` is
`FF 90 68 01 00 00` (receiver slot `+0x168`). Its vtable entry VA
`0x1C72028` / raw `0x1870C28` contains VA `0xECEF00`, raw `0xACE300`.
Bytes `E9 0B 32 EC FF` there tail-jump to raw `0x991510`.

Selector `0x59` instead calls slot `+0xE8` at raw `0x81C522` and reaches
raw `0xACA9D0`. That method clears/resets the tree at context `+0x43C8`
and sets state byte `+8`; it does not read the dispatched message argument.
The earlier coordinator assignment naming it as the PrivateDialog receiver
was incorrect.

At raw `0x991662`, `FF 90 AC 00 00 00` calls the `uiManager` loaded from
VA `0x1E002B8`, with arguments `(229, &stack_record, 0)`. The preceding
pushes at raw `0x991658` are `6A 00 52 68 E5 00 00 00`. Its vtable slot
`+0xAC` resolves to raw `0xBF9D80`. No event name is inferred.

The consumer reads byte `+0x28`, DWORD `+0x40`, QWORDs `+0x30/+0x34`
and `+0x38/+0x3C`, and a NUL-scanned pointer at `+0x44`. The record-building
path uses keys `success`, `vendor_id`, `product_id`, `price_id`, and `code`.
On the successful empty-code path, it passes address `message+0x44` to a
fallback helper with literal `PURCHASE_SUCCESSFUL`. These observations do
not resolve the previously documented parser/consumer representation
mismatch. Next bounded task: check constructor/parser `0x5EE900` provenance
for fields `+0x3C/+0x40/+0x44` and any intermediate transform.

The subsequent bounded check reconfirmed parser `0x5EE900` writes only
DWORD `+0x28`, QWORD `+0x30`, and DWORD `+0x38`, ending at exclusive
`+0x3C`. Consumer DWORD `+0x40` is read at `0x9915EA` (`FF 76 40`);
pointer `+0x44` is loaded at `0x99154E` (`8B 46 44`). Direct xref/pointer
searches did not locate the parser constructor/caller in that pass, so no
initializing transform was established. This is not proof that no indirect
initializer exists, nor a demonstration of runtime out-of-bounds behavior.
Next check starts at the known parser factory `0x81CC50` and its actual
selector-`0x79` branch rather than repeating the negative reference scans.

That factory linkage is now established. Route0 subtracts `0x53` at raw
`0x81CCAD`; selector `0x79`'s entry at raw `0x8218DC` / VA `0xC224DC`
contains `9E E3 C1 00`, selecting raw `0x81D79E`. That branch requests
`0x40` bytes (`6A 40`) and calls constructor `0x526840` at `0x81D7BF`.
The constructor passes the unadjusted allocation to base `0x5A1510`,
installs vptr VA `0x1C67018`, zeros payload `+0x30/+0x34`, and returns
the same pointer (`8B C6` at `0x526863`). Neither constructor initializes
`+0x3C/+0x40/+0x44`.

The factory moves EAX to ESI at `0x81D7C4`, pushes that pointer at
`0x81D7CA`, and calls reader `0x5EE900` at `0x81D7D5`
(`E8 26 11 DD FF`), without adjustment. The factory returns the same
pointer. Separately, dispatcher `0x81CA81` pushes its incoming message
pointer unchanged before the slot-`+0x168` call. Therefore the constructors
do not reconcile the mismatch: `+0x3C` is inside the requested allocation
but unwritten on this path; `+0x40/+0x44` are beyond the request. Actual
allocator usable size and runtime effects remain unknown. The next narrow
check is pointer identity or transformation between parser return and
consumer invocation, rather than another constructor scan.

## Signing-parent ownership and pending instance alias

The signing/crypto parent is a `0xB0`-byte object constructed at raw
`0x12E36F0` and owned at `tfApplication+0xA28`. Its constructor installs
no vtable; no native parent-type name was established. Application RTTI
names `.?AVtfApplication@@`.

| Raw address | Observed pointer flow |
| --- | --- |
| `0x3031B4` | `89 87 28 0A 00 00`: store constructed parent at application `+0xA28` |
| `0x30DC12` | `FF B6 28 0A 00 00`: pass application's parent to Login constructor |
| `0x636654/0x636657` | Load second argument; store at Login `+0x24` |
| `0x63DF0B/0x63DF13` | Load `[Login+0x24]` into ECX; call setup `0x12E3960` |
| `0x636579` | Load application global from PE VA `0x1DFFD98`, raw `0x19FE198` |
| `0x63659F` | Load that application's `+0xA28` into EDX before tail-transfer to `0xBC1990` |
| `0xBC1A5D/0xBC1A6D` | Restore parent into ECX; call signing builder `0x12E4100` |

These are concrete owner/member paths, not yet proof that Login creation's
application instance equals the global application's instance. Shared
synchronization helpers and nesting counter `+0x30` do not establish that
alias either. The next bounded task traces the application global writer
and mode-construction receiver provenance; do not claim the shared instance
until that edge is verified.

The bootstrap-path alias is now established. Wrapper receiver VA
`0x1DFFD88` owns the global slot at `+0x10`, VA `0x1DFFD98`.
Initializer `0x2EC530` preserves its wrapper receiver in ESI at
`0x2EC534`, constructs the allocated application through `0x303080`
at `0x2EC593`, and stores the returned application at `[ESI+0x10]`
at `0x2EC59C` (`89 46 10`). Constructor `0x303080` returns its
unchanged receiver. The initializer's observed callers at
`0x33402B`, `0x335A7E`, and `0x480568` test the global for zero
and supply wrapper VA `0x1DFFD88`.

Bootstrap loads `[VA 0x1DFFD98]` into ECX at `0x2EB184` and calls
application initialization `0x3063D0` at `0x2EB18F`. That receiver is
preserved through local `[EBP-0x40]`, restored at `0x30788F`, and passed
to mode creation by the call at `0x30798E`. Mode creation preserves the
receiver at `0x30D87E` and passes `[application+0xA28]` to Login at
`0x30DC12`, where the previously verified constructor stores it at
Login `+0x24`. Thus bootstrap-created Login and request preparation
reference the same application slot and signing parent, not just matching
types. This is path-scoped: raw `0x2EC6A4` also explicitly zeros the
global. Permanent uniqueness, concurrent reinitialization, and lifetime
immutability were not proven. The global is in `.data`, whose VA/raw
delta is `0x401C00`.

## PrivateDialog parsed-pointer delivery, ReplayReader branch

The parsed-pointer continuation establishes a concrete conditional
ReplayReader path, not every live Matrix owner path:

- Raw `0x821D87` calls parser `0x81CC50` and preserves its EAX result
  through return. The registered parser call at `0xBD6D3C` returns
  pointer P, which is moved to EDI at `0xBD6D41`, stored in local record
  `+8` at `0xBD6D63`, and enqueued through `0xBD8B80` at `0xBD6D66`.
- Queue helper `0xBD8B80` copies the pointer DWORD unchanged at
  `0xBD8BE7..0xBD8BEA`. Transfer at `0xBD6612..0xBD661B` invokes the
  same record-copy helper for queue `+0xC8`. Iterator helper `0xBD8AD0`
  and record-pointer-storage growth `0xBD8DB0` do not reconstruct messages.
- Delivery loads record `+8` into EDI at `0xBD6699` and into local
  `var_54` at `0xBD669C`. Routing byte 2 or 3 passes `&var_54` to
  context vtable slot `+8`, with selector 1 or 0 respectively.
- On the previously established ReplayReader callback binding,
  `0x7FDC4F..0x7FDC52` forwards the reference argument to `0x7FDC60`.
  That method dereferences it into EBX at `0x7FDC6E`. Its zero-key-byte,
  non-nested-sentinel, non-null-receiver branch pushes P unchanged at
  `0x7FDD4E` into dispatcher `0x81BB90` at `0x7FDD4F`.

No adjustment, replacement allocation, or reparse appears on this specific
branch between parser result and receiver. Nested-message child parsing is
a different branch and excluded. Callback arguments permit replacement:
`0xBD6713` reloads the possibly changed local pointer after the call.
Callbacks or other aliases can also mutate contents. The evidence therefore
does not prove receiver-only fields are globally untouched, nor establish
runtime consequences of the static representation mismatch.

## AES destination is slBlob, not a cursor stream

The concrete vtable at VA `0x1C80CC4` / raw `0x187F8C4` has RTTI
`.?AVslBlob@@`, with `slBufferInterface` in its hierarchy. Its
TypeDescriptor is VA `0x1E35F90`, raw `0x1A34390`.

| Slot | Target raw offset | Observed operation |
| --- | --- | --- |
| `+0x04` | `0x12CB960` | Writable pointer at supplied offset; may detach backing |
| `+0x08` | `0x12CB910` | Pointer at offset without writable-detachment step |
| `+0x0C` | `0x12CB9C0` | Return backing header `+0x0C`, or zero: logical length |
| `+0x10` | `0x12CB9D0` | Resize without preservation, then copy replacement bytes |
| `+0x14` | `0x12CBA00` | Resize to zero without preservation |
| `+0x18` | `0x12CBA10` | Append bytes |
| `+0x1C` | `0x12CBAC0` | Absolute resize with preservation |
| `+0x20` | `0x12CBA80` | Grow by increment and return old-end pointer |
| `+0x24` | `0x12CBA50` | Append nonempty C string without its terminator |

Slot `+0x1C` immediately changes logical length, not just capacity.
Its helper `0x12CBCB0` either reuses suitable unique backing and writes
header `+0x0C` at `0x12CBD33`, or allocates/preserves
`min(oldLength,newLength)` bytes and writes the new length at `0x12CBDFD`.
Capacity is separate at header `+0`; pooled backing explicitly has
capacity `0x800` and initial length zero. Slot `+4(0)` may detach the backing
even without changing length, then returns its data pointer without a
cursor advance.

The AES helpers therefore save the old logical length, enlarge it, obtain
writable storage, and copy input to `base+oldLength`. On success they
resize to old length plus reported output length; on failure they resize
back to old length. This restores logical contents/length, not necessarily
allocation identity, capacity, or discarded-tail bytes; no wiping was
established. Wrapper `0x12E4680` uses a fresh empty destination and publishes
its backing only after decrypt success. Generic source/backing alias safety
and wrapping length arithmetic were not established. The next check is the
wrapper's exact backing-header/object fields and post-publication size logic.

## Signing reset and GUI-logout entry

Reset `0x12E37A0` releases/zeros parent shared-text fields `+0x44`,
`+0x48`, `+0x3C`, and `+0x40` at raw `0x12E380D`, `0x12E3834`,
`0x12E385B`, and `0x12E38BB`. It destroys/frees wrapper `+0x58`,
clears readiness byte `+0x34` at `0x12E3881`, and clears DWORDs
`+0x38/+0x50/+0x54` at `0x12E3885/0x12E388C/0x12E3893`.
Nesting counter `+0x30` brackets the operation.

Observed direct reset callers:

| Callsite | Condition |
| --- | --- |
| `0x30D718` in `0x30D6E0` | Second argument nonzero; parent is application `+0xA28` |
| `0x63AA08` in `0x63A9B0` | Login parent `+0x24` reports readiness nonzero |
| `0x12E3D61` in `0x12E3B80` | Either of its first two text inputs is empty; returns false |
| `0x12E40C5` in code at `0x12E3DA0` | Corresponding empty-input branch; returns false |

The last code entry is not defined as a function in the current IDB;
its instructions, not a recovered function declaration, establish that path.
Function `0x63F5C0` calls cleanup `0x63A9B0` at `0x63F5CB`, then logs
`GUI Requested logout` at `0x63F5DA` (text VA `0x1B62944`, raw
`0x1761544`). This is concrete logout evidence, not a semantic guess from
the reset helper's name.

Preparation `0xBC1990` rejects null parent or false readiness before
signing builder `0x12E4100`, returning `AL=0` at `0xBC1AF9`.
Builder `0x12E4100` separately tests readiness at `0x12E4195` and calls
diagnostic handler `[VA 0x1C15EA0]` at `0x12E41AC` if false, but has no
explicit failure return there if the diagnostic returns. Diagnostic
termination behavior is unresolved. Cached/prepared/in-flight requests and
concurrent interleavings were not examined; this does not establish
cancellation. The next bounded trace follows immediate GUI-logout cleanup
to any actual request drain/cancel boundary.

## AES publication fields and realignment correction

The bounded instruction trace of `0x12E4680` resolves its object/header
layout. These offsets are structure-relative; instruction addresses are raw
offsets in the active sample identified above.

| Structure | Offset | Observed meaning |
| --- | --- | --- |
| slBlob | `+0` | Vtable |
| slBlob | `+4` | Backing header pointer |
| slBlob | `+8` | Ownership/control pointer |
| slBlob | `+0xC` | Byte-address alignment requirement |
| Backing header | `+0` | Capacity |
| Backing header | `+4` | Data pointer, not byte count |
| Backing header | `+8` | Allocation-to-data displacement |
| Backing header | `+0xC` | Logical byte length |

Constructor `0x12CB700..0x12CB765` allocates capacity plus alignment;
for nonzero alignment its displacement is
`alignment - (allocationAddress % alignment)`. Even an already aligned
allocation advances by a full alignment unit. Pool initialization at
`0x12CBF9D..0x12CBFBA` sets capacity `0x800`, data pointer `header+0x10`,
and zero displacement/length.

At `0x12E4739..0x12E4767`, the decrypt call receives backing data pointer,
virtual logical length, and fresh empty destination. Copy instructions
`0x12EF13A..0x12EF144` establish pointer/count ordering. Reading backing
`+4` as a length would invert those arguments.

Success `0x12E4776..0x12E477E` publishes by reference-counted pair
assignment, **not swap**: `0x12CC439..0x12CC44F` acquires incoming
ownership, and `0x12CBECA..0x12CBED0` replaces object `+8/+4`, releasing
prior ownership. The destination object's alignment remains unchanged.
Then `0x12E4783..0x12E47A3` checks nonzero alignment, nonnull header, and
`unsigned(dataPointer) % alignment != 0`. Only if misaligned does it
preserve-resize to the **same logical length**. Resize rejects misaligned
reuse at `0x12CBD03..0x12CBD13`, allocates using object alignment at
`0x12CBD9D..0x12CBDA6`, preserves bytes at `0x12CBDBC..0x12CBDD3`,
and restores the same length at `0x12CBDFD`.

This is publication followed by optional realignment, not padding removal
or cipher-block length normalization. Decrypt failure bypasses publication.
Allocation exceptions, generic aliasing, and overflow behavior remain
unproven.

## Logout cleanup: established effects and remaining indirect edge

The bounded GUI-logout trace found more than signing reset, but no concrete
ClientApi cancellation boundary. Call `0x63A9C3 -> 0x490090` destroys/nulls
connection field `+0x820` and sets connection state `+0xC=1`.
Virtual destructor call `0x63A9D3` was reported as reaching
`0x48F290 -> 0x48F360`, using original-file vtable evidence; that target
needs active-sample verification before treating the mapping as definitive.
Within that candidate destructor, `0x48F43C -> 0x308500(1)` clears
application tree `+0xA50` and count `+0xA54`; these fields are **not**
established as ClientApi request storage. Call `0x63F61D -> 0x30D790`
performs the mode transition/enqueue.

Requests that subsequently reach `0xBC1990` face the established readiness
rejection. Already-prepared/in-flight requests remain unresolved. In
particular, callback `0x48F41C` and indirect owned-object cleanup prevent a
global "logout never cancels requests" conclusion. Follow-on work is scoped
to that callback/active-sample mapping and to one concrete caller's handling
of preparation failure.

## Logout callback verified against the active sample

The candidate destructor mapping above is no longer dependent solely on
`.original`: the relevant constructor, vtable, callback, and callee bytes
match the active sample, although the two files differ globally. Callback
raw `0x48F41C` resolves to raw `0xBF9D80`, VA `0xFFA980`.
Initializer `0x3EE8CA` uses wrapper VA `0x1E002A8`;
`0x3E4573` constructs `0xBF7A80`, and `0x3E457C` stores its result
at wrapper `+0x10`, the uiManager global VA `0x1E002B8`.
Constructor `0xBF7ABB` installs vtable VA `0x1C73EBC`; slot `+0xAC`
at raw `0x1872B68` contains VA `0xFFA980`.

Logout supplies event arguments `(83,0,0)`. Dispatcher `0xBF9D80`
checks receiver byte `+0x672` at `0xBF9DBA`. If enabled, it uses event
container `+0x6F0`; the null-payload branch selects shared payload at
`0xBF9E21`. Call `0xBF9F2D -> 0xBF9C40` iterates recipients, invoking
`0xC38400` at `0xBF9D5B`. This establishes a gated event notification,
not cancellation. Registered event-83 handlers remain the next boundary.

## Login ignores failed signing preparation before manager submission

One concrete caller contradicts a stronger reading of "readiness rejection":
`0xBC1990` rejects **signing preparation**, not necessarily request
admission. `Login_QueueLoginRequest` at raw `0x63B550` allocates
`0x78` bytes at `0x63B5D9`, constructs the request at `0x63B5FE`
(URL parsing at `0x384487`), sets type `2` at `0x63B60D`, and copies
callback pair `+0x50/+0x54` at `0x63B653..0x63B659`.

Call `0x63B65C -> 0x636550` first prepares request collection `+0x20`
at `0x636571..0x636574`, then loads app global VA `0x1DFFD98`,
reads app `+0xA28`, and tail-jumps to `0xBC1990`
(`0x636579..0x6365A9`). The caller does **not** test its returned
`AL`; it immediately submits the request at `0x63B663`.

Submission `0x6499F0 -> 0xBC43C0` instead checks manager byte
`+0x21` enabled, `+0x20` clear, and validation `0xBC1380`.
Accepted requests use a per-route tracker, pass through `0xBC3F40`,
and receive timestamp field `+0x6C`. Manager success makes login set
in-flight byte `+0x9E0` and option `+0x9E2`
(`0x63B668..0x63B69F`). Manager failure, not signing failure, instead
destroys the request via slot zero with flag one, logs, and returns zero
(`0x63B66C..0x63B68F`).

Thus a request may enter tracker storage after signature construction was
skipped. This does not establish immediate WinHTTP handle creation, actual
network transmission, or a feasible concurrent logout/login interleaving.
The next trace follows one tracker worker and any later readiness/header
gate. Earlier references to queued requests encountering `0xBC1990`
must not be read as proof of end-to-end admission failure.

## Event 83 registration remains dynamic; delivery queues callbacks

The bounded recipient trace did not identify a concrete event-83 handler.
`0xC38438..0xC38446` requires the event entry in listener collection
`+0x168`; `0xC3847C` queues its callback record/payload at listener
`+0x4C4`, and `0xC384D3` inserts the listener into manager `+0x5EC`.
Zero name filter bypasses recipient-name matching at the dispatcher.
Registration paths include XML `0xC35AD8` and script `0xC35CF0`.
Name IDs derive from first-seen-name count at `0xBF9AE6..0xBF9AFE`;
recovering numeric 83 therefore needs registration order, not a string
guess. No concrete cancellation or request effect was established. The
next independent question is callback queue draining/lifetime, not another
unbounded search for a guessed logout event name.
