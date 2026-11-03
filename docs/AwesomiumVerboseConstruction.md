# Awesomium verbose rule construction and error retention

Static inspection of `awesomium.dll`, 23,470,048 bytes, SHA-256
`DE27851602CB1387EF50086761B417D3D39A2A2648B5490939F843A711FF7F6E`.
Addresses below are preferred PE VAs (image base `0x10000000`).
This document records the text layout and output-retention boundary; no
replacement rule-text parser is implemented.

## Outer structure and byte ledger

Verbose constructor `0x102B5720` calls pair helper `0x102A0670` with
separator arguments `=` (`0x3D`) and comma (`0x2C`) at
`0x102B581E-0x102B582F`. Strings have stored byte lengths, not a Unicode
normalization step. The helper clears its output pair vector before parsing.

| Boundary | Exact bytes / width | Evidence |
|---|---|---|
| Record separator | Comma, one byte `2C` | Constructor push at `0x102B581E`, split helper `0x102A03F0` -> `0x102A0150` |
| Outer trim set | `09 0A 0B 0C 0D 20 00` (six bytes plus terminator) | Table at `0x1127C110`; wrapper `0x1029B0D0` |
| Trim flags | DWORD3, both edges | Split call `0x102A01DB`; trim `0x1029AE10` tests bits1/2 |
| Key/value delimiter | Equals, one byte `3D` | Pair helper `0x102A04D0` uses first occurrence via `0x1000A850` |
| Stored pair stride | 56 bytes, two28-byte narrow strings | Pair vector copied/iterated by constructor |
| Stored rule stride | 36 bytes, string + level + source-kind | Append `0x102B5650`, lookup `0x102B53A0` |

Splitter `0x102A0150` emits fragments at commas and at end, including
empty fragments, and applies both-edge trimming using the explicit set.
Pair aggregator skips fragments whose trimmed stored length is zero
(`0x102A0730`). It does not use a general managed whitespace predicate.

For each retained fragment, pair extraction clears key/value outputs, finds
the first equals and copies everything preceding it as key. Starting at
that equals, `0x1009F3F0` finds the first byte **not** equal to `=`, skipping
consecutive equals only. That remaining suffix is the single value.
It is not a split at every equals; later equals inside the value remain.
Key and value are not independently whitespace-trimmed on this path.
Empty keys are not rejected when a value exists.

No equals, or no non-equals byte after the delimiter, returns false.
However the aggregator only makes its aggregate status sticky-false at
`0x102A077C`; it continues building a pair and appends it at `0x102A090F`.
If value extraction produced no value it uses an empty string. Thus an
error status is not a guarantee of an empty vector.

## Numeric failure does not reject the rule

Constructor initializes each prospective rule's level slot0, calls the
numeric converter at `0x102B58E6`, then checks its byte only for a diagnostic
cleanup path. Both outcomes reach the append at `0x102B5911`. The per-rule
stored level is the **observed converter output**, not necessarily zero on
failure. The earlier [logging numeric trace](AwesomiumLoggingSetup.md)
records prefix retention and signed saturation. This differs from global
`v` threshold assignment, which requires conversion success.

Likewise the pair helper's aggregate false result is not used to discard
all pairs; after its optional diagnostic cleanup the constructor iterates
the produced vector. Diagnostic callbacks that do not return, exceptions
and allocation failures remain separate from this normal-flow statement.

The constructor appends in vector order; the inspected append and lookup
paths have no duplicate-key merge. Repeated patterns remain repeated rules,
and [first-match lookup](AwesomiumVerboseRules.md) gives the earlier
matching one precedence.

## Derived examples, not runtime captures

These are consequences of the inspected control flow, not claims that the
target parser was executed:

| Text fragment after comma splitting | Retained key/value before numeric conversion |
|---|---|
| Empty or all bytes from trim set | Skipped |
| `foo=2` | `foo` / `2` |
| `foo==2` | `foo` / `2` |
| `foo=2=3` | `foo` / `2=3`; numeric failure can retain2 |
| `foo = 2` | `foo ` / ` 2`; leading value space marks numeric status false |
| `=2` | Empty key / `2` |
| `foo=` | `foo` / empty; pair helper false, output still retained |
| `foo` | Empty key / empty; pair helper false, output still retained |

There is no observed quoting/escape interpretation in the inspected
comma/equals paths. A quoted comma still creates a boundary here; this is
not a claim about earlier command-line quoting or later wildcard semantics.
Input acquisition, malformed pointer spans and full diagnostic behavior
remain out of scope.

## Validation boundary

Hash-guarded instruction checks bind delimiter arguments, trim table,
empty-fragment skip, sticky false status, continued pair append, numeric
conversion and unconditional rule append. No target, parser, logger,
callback, filesystem API or service was executed. A safe further static
check is to follow pair-vector append `0x100C21A0` if allocator/aliasing
failure behavior matters; it is not needed to infer the normal append order.
