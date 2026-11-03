# Awesomium logging setup and threshold precedence

Static inspection of `awesomium.dll`, 23,470,048 bytes, SHA-256
`DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E`.
Addresses are preferred PE VAs (image base `0x10000000`).
`LoggingSetupNativeLayout.cs` saves inert call arguments and assignments.
It does not parse numbers, configure a logger, open/delete a file, acquire
a mutex or invoke callbacks.

## Caller sequence

Setup function `0x10033BC0` obtains `type`, `awesomium-log-path` and
`log-level` through the previously traced ASCII value getter `0x1028C8A0`.
Thus non-ASCII values become empty; values that pass are not lowercased.
It initializes an Int32 output slot to zero at `0x10033D34`, calls numeric
conversion wrapper `0x1029CB30` at `0x10033D6E`, and ignores its returned AL.
It also tests `disable-logging` through presence query `0x1028C3C0`, not a
nonempty-value comparison.

After constructing a wide path, `0x10033E6E` calls `0x1028A2A0` with:

| Argument | Observed value |
|---|---|
| Path | Converted ASCII `awesomium-log-path` value |
| Output destination | 0 when `disable-logging` is present, otherwise1 |
| Argument3 | 0 |
| Append-existing flag | Whether the ASCII `type` getter result is nonempty |
| Diagnostic-check mode | 0 |

The initializer stores the last argument to global `0x11596AF8` at
`0x1028A2E1`. That mode0 disables the optional mode-equals1 checks previously
seen in name conversion; this observation does not establish process-wide
immutability or other callers' arguments.

The initializer stores destination at `0x1028A697`. With destination0 or2
it returns true without the later file-opening branch. Otherwise its
`0x1028A703` append flag gates `DeleteFileW`: false attempts deletion before
opening, true skips deletion. Return values of deletion and the caller's
overall initialization result are not used to select the subsequent threshold.
No actual deletion or open occurred during analysis.

On any normal initializer return, `0x10033E78` invokes the minimum-severity
setter with the converter's output slot. There is no branch on initializer
success or numeric-conversion success between these calls.
The setter preserves signed values below3, including negatives, and clamps
values3 or above to3, as documented in
[conversion diagnostics](AwesomiumConversionDiagnostic.md).

## Verbose configuration can write first, then be overridden

Inside the initializer, presence of `v` or `vmodule` can construct verbose
configuration `0x102B5720`. The xref at `0x1028A51B` is a pushed address of
threshold storage `0x11596B04`, not itself a threshold write.

The constructor saves that address in object+`0x10`. For a nonempty `v`
value it calls the same numeric converter at `0x102B57E7`. Unlike the outer
caller, it tests AL at `0x102B57EF`: only success reaches the unchecked
negation and indirect storage write at `0x102B57FA-0x102B57FC`.
A failed parse or empty `v` leaves this threshold unchanged on this path.
Module-pattern parsing and later verbose lookup are not modeled here.

Consequently, during `0x10033BC0` setup a successful verbose-threshold write
can occur inside initialization but is overwritten afterward by the
`log-level` setter. Intermediate logging, indirect calls and exceptions
remain distinct from the final threshold after normal return.

## Numeric failure does not imply unchanged output

Wrapper `0x1029CB30` passes an explicit begin/end span to `0x1029C8B0`.
That routine handles sign and leading `isspace` classification. The positive
and negative digit loops at `0x1029C430` and `0x1029C3C0` initialize output0,
accumulate digits, retain a prefix result when a nondigit causes failure,
and saturate to `Int32.MaxValue`/`Int32.MinValue` on overflow while returning
false. Leading whitespace marks the final success byte false even when the
digit loop succeeds. Whitespace classification is CRT-dependent; no managed
character-classification replacement is inferred.

For example, the native path can leave2 for `2x` or leading-space ` 2`,
despite a false success byte. The outer setup uses that2, while the verbose
constructor rejects the assignment. Empty/invalid-first-digit input leaves0.
Positive overflow leaves `Int32.MaxValue`, which the outer setter clamps3;
negative overflow leaves `Int32.MinValue`, which it preserves.

The saved code accepts observed converter output and, where relevant,
success status. It deliberately does not implement a replacement parser or
promise a complete grammar. Native checks bind the call order, mode store,
negation/success gate and numeric overflow/prefix boundaries to this sample.
