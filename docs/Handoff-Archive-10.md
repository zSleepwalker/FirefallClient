# Reverse-engineering handoff archive 10

## Prefix replacement does not release old value locally; failure cleanup precedes publication

The bounded follow-up confirms raw `0xBFCBC1`'s duplicate assertion,
if returning, continues to `0xC07380` and stores the new pointer at
`0xBFCC05` without reading/releasing the prior mapped value locally.
Its broader replacement ownership policy remains unresolved.

Crucially, the only identified direct caller of `0xC341C0` is
`0xC34100`'s construction-failure path, before registry insertion.
Absence of prefix-tree erase in that failure cleanup does not establish
missing erase for successfully published objects.
Repeated tracing of that path is parked. The next task follows actual
manager shutdown/destructor and bulk prefix-container cleanup.

## Direct mapping is threshold gated and falls back to normal region growth

Raw `0x12C6A90` initializes **VA** `0x1EDAFC4` to `0x40000`
(256 KiB), trim threshold `0x1EDAFC8` to `0x200000`,
and flags `0x1EDAFCC/0x1EDAF84` to 7.
Raw `0x12C7690` attempts direct mapping when heap flags `+0x1BC`
include mask 1, normalized request meets the threshold, and top-size
field `+0x0C` is nonzero. Factory flags 5 preserve that direct-map mask.
The prior report's `0x12C327D0` publication address was a typo for
the previously verified raw `0xC327D0`.

Raw `0x12CA410` searches bins/available chunks first. A failed direct
attempt in `0x12C7690` falls through to normal region growth, not a new
bin search. Both paths account the full region extent at `+0x1B0`
and update peak `+0x1B4`; direct mapping also updates minimum base `+0x10`.
The new report gives direct VirtualAlloc flags `0x103000`, versus normal
`0x3000`; exact operands and its inconsistent round-up notation require
a narrow byte-backed follow-up before assigning further flag semantics.

## Direct allocation uses 0x103000; exact size expressions are mask operations

Raw helper `0x12C6E70` computes
`(granularity + normalizedRequest + 0x1E) & ~(granularity-1)`
with 32-bit arithmetic. Its inspected push sequence at
`0x12C6EAF..0x12C6EB9` supplies protection 4, allocation type
`0x103000`, computed size, and null address to the already PE-verified
VirtualAlloc slot **VA** `0x1AC42B8`.
The additional `0x100000` bit is MEM_TOP_DOWN; ordinary fallback instead
uses `0x3000` and expression
`(granularity + normalizedRequest + 0x2F) & ~(granularity-1)`.
Neither expression should be rewritten as conventional round-up of the whole
sum, which would add another granularity-1 before masking.

With heap limit `+0x1B8` nonzero, both paths reject unsigned
`current+size <= current` or sum above that limit; a zero limit skips that check.
Failed direct allocation falls through to the ordinary growth path.
The report's final alternative raw addresses `0x12862AF/0x1286B10`
are inconsistent with the raw-IDB basis and are not propagated.
The next trace follows direct-chunk resize through `0x12C94D0`.

## Manager shutdown drains published prefix payloads before releasing key nodes

Vptr **VA** `0x1C73EBC`, raw `0x1872ABC`, contains:
slot `+0x10` bytes `B0 9D FF 00` -> raw `0xBF91B0`;
`+0x14` bytes `D0 9D FF 00` -> raw `0xBF91D0`;
`+0x34` bytes `D0 AC FF 00` -> raw `0xBFA0D0`.
Shutdown calls `+0x34` via `0x30D150` before `+0x10/+0x14`.

With manager byte `+0x671` set, raw `0xBFA0D0` drains registry `+0x5E4`:
payload cleanup `0xC33060` and release/tracking precede
node erase `0xC09D10`. Node cleanup separately releases shared-text
key `+0x10` and frees the tree node.
Later `0xBF91D0` sweeps remaining entries; destructor `0xBF8AC0`
also clears the tree via `0xC0CBF0`.
This supplies the actual published-object bulk path missing from the
pre-publication failure trace.
Inside `0xC33060`, stored items receive virtual slot 0 with flag 1;
the dynamic item types/targets remain unresolved, so deleting-destructor
interpretation is not an established concrete deletion contract.
The readiness byte's producer/phase meaning is next.

## Direct-mapped resize is a no-write eligibility check followed by optional move

Raw `0x12C94D0` routes low-two-bit-zero chunk headers through
`0x12C6F40` at `0x12C9521..0x12C9529`.
That helper returns the same chunk only when normalized request >=`0x100`,
current size >=request+4, and size difference <=2*granularity.
Otherwise it returns null, making no calls, header writes, or accounting changes.
It does not grow/shrink the mapping itself.

Outer raw `0x12CAE30` converts user pointer to header and back.
On failed eligibility, successful fallback `0x12CA410` copies
min(request,old usable capacity), frees old allocation, and returns new.
Failed fallback returns null preserving old allocation in this helper;
the separately traced VM callback can then free it.
Ordinary chunks instead enter split/coalesce logic `0x12C953B..0x12C984B`,
whose complete transition cases are the next task.

## Ordinary in-place resize splits at 16 bytes except for positive top remainder

Raw `0x12C94D0` accepts ordinary header low bits 2/3 after validation.
Shrink retains the whole chunk for slack <16; otherwise it splits a tail
and sends it to `0x12C7D30` (`0x12C9538..0x12C9580`).
Growing into top requires combined size strictly greater than request,
then moves heap top pointer/size `+0x18/+0x0C`.
Unlike other split paths, this branch has no 16-byte remainder check;
positive aligned slack can be 8 (`0x12C9581..0x12C95C0`).

Growing into last remainder updates `+0x14/+8` for slack >=16,
or absorbs the whole remainder and clears those fields for smaller slack
(`0x12C95C1..0x12C963A`).
Other adjacent free chunks are unlinked from small bins (<`0x100`) or
tree bins, then absorbed for slack <16 or split/coalesced for larger slack
(`0x12C963B..0x12C9853`).
Busy/insufficient neighbors return null before the observed resize writes;
the outer fallback can allocate/copy/free while preserving old storage
if allocation itself fails.

These in-place transitions do not write region-byte accounting `+0x1B0`.
Consistency-error calls through **VA** `0x1AC4A24` are the already
PE-verified MSVCR120 abort boundary, not the raw-IDB apparent `0x08080808`.
The explicit normal-failure no-write observation does not generalize to
all partially executed consistency-error branches.

## Manager drain gate covers partial initialization, not only successful startup

Constructor raw `0xBF7A80` writes word `this+0x670=1`,
so byte `+0x671` starts zero, and clears byte `+0x672`.
Init raw `0xBFB310` first invokes virtual `+4`; nonzero returns zero
before arming cleanup. Otherwise it sets `+0x671=1` at `0xBFB350`.
Later failure at `0xBFB668` leaves that byte set; only success sets
`+0x672=1` at `0xBFB8DC`.
Thus `+0x671` supports partial-initialization cleanup, not just a success flag.

Shutdown helper `0x30D150` calls slot `+0x34` at `0x30D1EB`.
Target `0xBFA0D0` skips when `+0x671` is zero; otherwise it clears
the word at `+0x671` (both cleanup/success bytes) and drains the prefix
registry, calling `0xC33060` at `0xBFA278`, then `0xC09D10`
at `0xBFA29C`.
Subsequent slot `+0x10`, raw `0xBF91B0`, invokes the object at
manager `+0xA04`'s virtual `+0x10(1)` and clears `+0xA04`.
Slot `+0x14`, raw `0xBF91D0`, sweeps remaining prefix/nested tracking entries.
Neither changes the two initialization bytes.
The normal-startup indirect binding to `0xBFB310` remains the next task.

## Realloc copy bound uses ordinary size minus 4, not minus 8

Raw `0x12CAE30` reads size/flags at user pointer-4, masks `0xFFFFFFF8`,
and subtracts 8 for low-two-bits-zero direct mappings, otherwise 4.
Fallback copy at `0x12CAF14` uses min(original requested bytes, that bound),
then frees old allocation at `0x12CAF20`.
Raw `0x12C9D10` independently uses the same subtraction rule.
This supersedes earlier ordinary usable-capacity statements of chunkSize-8:
the observed copy ceiling is ordinary chunkSize-4, direct chunkSize-8.
Exact next-header/previous-size word lifetime explaining this distinction
is the next bounded check.

Null old pointer delegates to `0x12CA410`; nonnull with zero request frees
and returns zero. Requests >=`0xFFFFFFC0` return zero before that free path.
Wrapper `0x12B8030` passes requested size only and does not query capacity.
No separate usable-size helper call was found on the inspected copy path;
the bound is computed inline.

## Ordinary usable tail word becomes successor previous-size metadata only after free

Addresses below remain raw IDB offsets despite the follow-up's VA label.
Allocation raw `0x12C8A20` returns header+8, writes allocated size|3,
and marks the successor's previous-in-use bit
(`0x12C8BB5..0x12C8BCB`, `0x12C8CD8..0x12C8CFC`).
The successor's first word is not required for backward coalescing while
that bit is set. This accounts for ordinary usable size C-4: the last
four usable bytes overlap that otherwise-unused successor previous-size word.

Free raw `0x12C8D60`, including `0x12C92E0..0x12C92E9`,
clears the successor's previous-in-use bit and writes freed size at H+C.
Only then does that word serve as a valid free-chunk footer.
Direct mappings retain the distinct C-8 capacity rule.

The follow-up incorrectly described `(n+11)&~7` as ceil-to-8 of n+11
and overclaimed its slack. The mask rounds that sum down to a multiple of 8;
no `C-4 >= n+7` guarantee follows. Exact normalization comparisons and
small/upper-bound cases are the next arithmetic check.

## Manager init table slot confirmed; Login startup receiver remains conditional

Manager vtable **VA** `0x1C73EBC`, raw `0x1872ABC`, slot `+0x1C`
at raw `0x1872AD8` contains **VA** `0xFFBF10`, code raw `0xBFB310`.
This binds the known initialization routine to that concrete table.

Login mode init raw `0x636FF0` calls dispatch routine `0x30AF50`
at `0x63703E`. Its call `0x30B1F6` invokes virtual `+0x1C`
with ECX=EBX, obtained from global **VA** `0x1DFE970`'s virtual
`+0x3C` getter. The receiver's vptr was not established; matching slot
offset alone does not bind this startup call to the manager's init.
The next trace follows that getter's concrete target/publication and return.

## Request normalization boundaries verified by unsigned branches and offline arithmetic

Raw `0x12C8A20/0x12CA410` use unsigned n<11 for chunk size 16;
n>=11 uses `(n+11)&0xFFFFFFF8`. Representative exact values:
n=0..12 gives C=16; n=13..20 gives C=24.
Ordinary usable C-4 therefore covers each request, with zero slack at
n=12/20. For n>=11, slack is `7-((n+11) mod 8)`, from 0 through 7.
This corrects the earlier ceil/slack claim without changing the boundary-tag
ownership evidence.

Unsigned cutoff n>=`0xFFFFFFC0` is checked before addition.
Last formula-normalized input `0xFFFFFFBF` gives `0xFFFFFFC8`;
larger inputs select internal `0xFFFFFFFF` sentinel, whose downstream
return behavior is the next check, not an allocation-success guarantee.
For raw `0x12CAE30`, null old pointer delegates even n=0/oversized;
nonnull n=0 frees/returns null, and nonnull oversized returns null before
normalization. Offline integer checks verified these values without running
or allocating through the target.

## Login getter receiver and separately loaded UI manager must remain distinct

Raw `0x30B19B` invokes global **VA** `0x1DFE970`'s slot `+0x3C`;
`0x30B1AF` saves its return in EBX. Call `0x30B1F6`, bytes
`FF 50 1C`, uses ECX=EBX and that returned object's table.
The getter's concrete target/publication remains unresolved and is parked.
Initial zero bytes in its backed pointer slot do not establish a runtime vptr.

The UI-manager pointer is separately loaded at `0x30B1A9` and dispatched
at `0x30B200`. The follow-up below binds that distinct initialization dispatch.
The already verified manager table's `+0x1C -> 0xBFB310` entry does
not bind the preceding getter-returned object merely by slot coincidence.

## Allocator edge-case return contract complete within valid chunk-state assumptions

Oversized requests select internal size `0xFFFFFFFF`, not an immediate-null
branch. Raw `0x12C8A20` traverses its large-bin/top path, whereas
`0x12CA410` skips bins for that sentinel and checks top before growth.
Growth `0x12C7690` and direct helper `0x12C6E70` reject their wrapped
size before VirtualAlloc. For valid aligned chunk metadata the request fails;
the raw top checks do not independently rule out a stored impossible
`0xFFFFFFFF` top size. This is a static precondition, not a runtime finding.

Zero-byte raw allocation normalizes to chunk size 16 and may succeed or fail.
Null-pointer realloc delegates even zero/oversized requests; nonnull zero
frees, while nonnull oversized returns null before freeing.
Ordinary positive realloc failure preserves the old pointer in `0x12CAE30`.
VM callback `0xC3C0B0` instead reports failure then frees its original pointer
if reporting returns. Public allocation wrapper `0x12B8030` similarly reports
null before returning it. Both use the previously bound project reporter;
the follow-up's unresolved-indirect label does not reopen that mechanism.
No explicit errno assignment was found in the inspected oversized branches.
The allocator edge investigation is closed at these boundaries; the next
independent task follows VM finalization, not another allocation micro-case.

## Login UI initialization dispatch and inherited stack arguments

In the active sample `D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe`,
raw `0x30B1A9` loads UI-manager global VA `0x1E002B8`, saves the pointer
at `0x30B1B1`, and loads its vptr at `0x30B1BA`. Call `0x30B200`
(VA `0x70BE00`) uses that receiver and slot `+0x1C`. With the previously
verified constructor vptr VA `0x1C73EBC`, table raw `0x1872AD8` contains
`10 BF FF 00`, mapping VA `0xFFBF10` to raw `0xBFB310`.
This composes constructor/table evidence with the concrete call; it does not
assert that the global cannot hold a different runtime vptr.

Only the return from the separate call `0x30B1F6` is newly pushed before
`0x30B200`; seven prior arguments remain on the stack. Callee order is:
preceding return, zero, `[VA 0x1DFFFC0]`, `[VA 0x1DE0868 + EDX*4]`,
EDI, caller `arg_0`, caller `arg_8`, second getter return plus `0x48`.
`0xBFB310` ends with `retn 0x20`, consistent with eight stack arguments.
These are positional values, not recovered semantic names or ownership.

Login raw `0x636FF0` calls `0x30AF50` at `0x63703E` and tests AL
at `0x637043`. One input flag comes from
`byte[[VA 0x1E5C834] + 0x40] == 0`. Initialization failure passes through
`0x30AF50`'s common local object/text cleanup at `0x30BDF0` and returns
zero; Login then logs and returns zero. Success branches to `0x63706C`.
The next bounded investigation follows the eight inputs' retained fields
and ownership, without reopening the unidentified getter receiver.

## VM close finalizer phases precede internal and outer storage release

For the same active sample, raw `0xCD5510` calls `0xCD5AB0`, then
`0xCD9870`, releases internal buffers, and finally invokes the allocator
for `(VM - 4, 0x390, 0)`. This is the VM close reached from scripting
object `0xC33060`; freeing the VM is not proof of freeing that native object.

`0xCD9870` transfers context `+0x48`'s list to the tail of `+0x60`.
Helper `0xCD9580` drains `+0x60`, moving entries to `+0x40` and,
for qualifying tag-6 values, making the protected callback through
`0xCD7890`/`0xCD6C10`. Callback literal VA `0x10D9E50` maps to raw
`0xCD9250`, which calls `0xCD7410` and dispatches through `0xCD7110`.
The drain does not check the protected-call status and continues; no retry
was found on this close path. `0xCD9870` then cleans lists at context
`+0x48`, `+0x40`, and `+0x64`.

Afterward `0xCD5510` frees context `+0x18`, context `+0x6C` and zeros
its size, then, if present, VM `+0x1C` after draining its `+0x0C` chain
via `0xCD51E0`. The outer `0x390` block is last.
Instruction-level clarification: `0xCD98A7` calls the drain; `0xCD98BB`,
`0xCD98CA`, and `0xCD98D9` call `0xCD8F70` for the three lists above.
The drain removes its head at `0xCD95AA`, calls `0xCD7890` at
`0xCD96A7`, restores saved state at `0xCD96AC..0xCD96BA` without
testing the returned status, and tests/repeats at `0xCD96C1..0xCD96C5`.
`0xCD7890` tests the protected result at `0xCD78C7..0xCD78C9`;
its error path returns status at `0xCD7907`, without retry.

Internal buffer free calls occur at `0xCD5538`, `0xCD5547`, and
conditional `0xCD557A`. Only afterward, `0xCD558D` loads allocator
userdata from context `+4`, `0xCD5590` loads the callback from context
`+0`, and `0xCD5592` invokes the final outer-block free.
The ordinary `0xD675A0` bridge similarly loads userdata at `0xD675C5`,
callback at `0xD675CC`, and invokes it at `0xD675CE`.

## UI initialization retains a mixture of copied text and unclassified pointers

The eight positional arguments to raw `0xBFB310` have these observed
uses in the active sample; pointer stores alone do not prove ownership:

| Argument | Handling |
|---|---|
| 1: preceding virtual-call result | Passed to `0xCA6370`; stored at child `+4` at `0xCA639F`. Child pointer stored at UI `+0x24` at `0xBFB416`; no refcount acquisition identified. |
| 2: zero | No read found in this bounded trace. |
| 3: global VA `0x1DFFFC0` value | Stored at UI `+0x974` at `0xBFB51A`; virtual call through it at `0xBFB8C6`. Ownership unresolved. |
| 4: indexed global value | Copied into shared-text UI `+0x34` at `0xBFB48C..0xBFB4CB` through byte-copy helper `0x2FCC90`, not retained as the source pointer. |
| 5: EDI | Copied to shared-text UI `+0x5C8` at `0xBFB403..0xBFB419`, then path-normalized. |
| 6: outer caller `arg_0` | Passed to formatter `0xBFDE80` at `0xBFB748..0xBFB74D`; no direct field store identified here. |
| 7: outer caller `arg_8` | Conditional mode flag at `0xBFB84B..0xBFB864`, not stored. |
| 8: second getter result plus `0x48` | Stored at UI `+0xA08` only near success, `0xBFB8C9..0xBFB8CE`; ownership unresolved. |

Late initialization failures converge on `0xBFB668` and return zero via
`retn 0x20` without clearing these retained fields in that epilogue.
The caller's subsequent `0x30D150` call site is `0x30B204..0x30B209`;
its exact predicate and concrete retained-field cleanup are the next task,
not evidence that those resources have already been released.

## Published scripting objects are freed after VM close during tree drain

The active sample's manager shutdown slot `+0x34`, raw `0xBFA0D0`,
does not merely close each published object's VM. With the payload from
tree node `+0x14`, it calls `0xC33060` at `0xBFA278`, then allocator
free `0x12C8D60` at `0xBFA27F` with the same object pointer.
Identity removal `0xBFCF30` follows at `0xBFA28B`, and key release
`0xC09D10` at `0xBFA29C`. This closes the native-storage ownership
gap for this concrete tree-drain path.

`0x12C8D60` is allocator free logic, not an object-vtable destructor;
no separate top-level C++ destructor dispatch was identified here.
`0xBFCFF0` has a similar close/free/identity-remove sequence, but its
caller remains unresolved and is not required to establish bulk shutdown.
The next task applies these verified lifecycle boundaries to the existing
Startup reconstruction and focused synthetic checks, without inventing
native ownership rules for the still-unclassified initialization pointers.

## Failed UI initialization invokes shutdown, not the manager destructor

At raw `0x30B200..0x30B209`, a zero UI-initialization return falls through
to `0x30D150`; nonzero skips it via `jnz 0x30B232`.
`0x30D150` tail-dispatches UI slot `+0x34`, bound to `0xBFA0D0`.
That routine is gated by byte `+0x671` and clears the word at `+0x671`
(both initialization flags) and field `+0x950` before cleanup.
It does not invoke UI slot zero, so this failure handling is not proof
that all startup-retained fields are destroyed.

The manager's slot `+4` is getter `0xBFA0C0` for byte `+0x671`.
Consequently `0xBFB310`'s initial nonzero check is an already-initialized
guard, not an unidentified initialization operation.
Destructor-style slot zero maps to `0xBF82A0`, which calls `0xBF8AC0`.
That teardown releases copied shared-text `+0x5C8` directly and
`+0x34` through `0x30A9D0`'s release-and-zero helper.

Initialization allocates a `0x188`-byte child, initializes it with
`0xCA6370`, and stores it at UI `+0x24`. The external argument at child
`+4` has no observed retain/copy. The inspected parent teardown does not
directly release `+0x24`; tracing this child's concrete ownership is the
next bounded task. Ownership of directly stored `+0x974` and success-only
`+0xA08` remains unresolved; absence of a direct clear is not a leak finding.

## UI lifecycle reconstruction added; shutdown tests a byte, not the cleared word

`src\Startup\UiManagerNativeLayout.cs` now records the verified initialization
guard, partial/success flag snapshots, and ordered payload close/free/identity/
key-release call sites. Startup checks cover these synthetic contracts;
`docs\Startup.md` records their static-only scope.

Coordinator instruction verification corrected the initial projection:
raw `0xBFA102` compares **byte** `[this+0x671]` with zero and
`0xBFA109` skips shutdown when zero. Only after passing that guard does
`0xBFA112` clear **word** `[this+0x671]`; `0xBFA11B` also clears a
word at `+0x950`. Therefore ready byte `+0x672` alone cannot enable
shutdown. The code separates the byte predicate from the two-byte state
snapshot; exhaustive combinations include the `(partial=0, ready=1)` case.
This supersedes any wording that treats the cleared word as the guard.
Focused Startup checks and `git diff --check` passed.

## UI child has no established vptr or concrete storage-release chain

The bounded `+0x24` investigation stops at an ownership gap, not a leak
claim. Raw `0xBFB3DA` requests `0x188` bytes, conditionally invokes
`0xCA6370`, and stores the result at UI `+0x24` at `0xBFB416`.
`0xCA639F` stores the external dependency at child `+4`;
`0xCA63A2` zeros child `+0`, rather than assigning a child vptr.
The indirect call at `0xCA651E` uses the dependency's vtable.
Direct initializer `0xCA66F0` is invoked at `0xBFB542`.

Parent deleting wrapper `0xBF82A0` calls `0xBF8AC0`, then conditionally
frees parent storage at `0xBF82B3`. This is not a child free.
Neither that inspected destructor nor the initialization failure epilogue
establishes release of the stored child. Child teardown remains parked;
do not infer a destructor slot from its zero first word or conflate the
borrowed dependency's dispatch with a child vtable.

## UI success pointer aliases a mutable sixteen-byte record sequence

Raw `0xC95400` (VA `0x1096000`) checks its object's `+0x72C` flag,
loads UI global VA `0x1E002B8`, and reads UI `+0xA08`.
When nonnull, the descriptor becomes ECX for `0xC97380`, with a
16-byte record assembled from float fields `+0x338..+0x344`.
`0xC97380` uses descriptor begin/end/capacity pointers at `+0/+4/+8`,
copies four DWORDs at end, and advances end by `0x10`; full storage
reaches grow helper `0xC978F0`. This is a mutable alias, not a copied
container, and does not establish semantic record names or wire encoding.

Initializer `0xBFB8CE` stores its eighth argument verbatim at UI `+0xA08`
after the earlier failure checks. Caller `0x30B1B4`'s indirect getter
result plus `0x48` supplies that argument; the containing object's concrete
identity/lifetime is unresolved. A reconstruction task now records the
verified layout without implementing speculative ownership or allocation.

## Late service callback has no checked return; final UI dispatch remains separate

Raw `0xBFB518..0xBFB51A` stores explicit argument three at UI `+0x974`.
At `0xBFB8BC..0xBFB8C6`, the initializer reloads it as ECX, pushes
one, and invokes vtable `+0x30`. Its return is not tested: the next
operation loads argument eight and stores it at UI `+0xA08`. A later
UI virtual `+0xB0` call precedes ready-byte `+0x672` publication.
Thus this service callback is non-gating in the observed return-value
control flow, not proof of a notification's semantic purpose.

Helper `0xBFEEC0` returning false is not itself an initialization failure:
the branch at `0xBFB8A0` logs on true, while false falls through.
The callback receiver has no null guard at the dispatch; normal completion
still depends on a valid receiver. Earlier checks at `0xBFB532`,
`0xBFB542`, or `0xBFB642` can return through `0xBFB668`.
The report's empty direct xrefs to `0xBFB310` do not negate the already
verified virtual caller, and its numeric-VA byte observation does not
supersede the caller's recorded global argument load. Concrete service
implementation binding remains unresolved; it is not inferred from that load.

## Final initialization dispatch is gated before the ready byte is published

Raw `0xBFB8D6` calls the UI receiver's slot `+0xB0`, after `+0xA08`
publication and before ready-byte `+0x672` write at `0xBFB8DC`.
For constructor vptr VA `0x1C73EBC`, table raw `0x1872B6C` contains
`90 06 00 01`, mapping VA `0x1000690` to raw `0xBFFA90`.
The helper requires its slot-4 initialization check and ready byte.
On the constructor-initialized path, ready is still zero at this call,
so it returns without issuing the later component synchronization.
The initializer ignores its return and then sets ready.

When its gates pass, `0xBFFA90` can call component `+0x974`'s slot
`+0x1C` with `(0,0)` or `(1,value)`; that return is also ignored.
Concrete selection inputs and a post-ready caller are the next task.
An indirect callee throwing before the ready store is not ruled out merely
because no explicit throw was seen here. Runtime vptr substitution remains
outside the constructor-table binding.

The record sink's static constants and focused checks are now implemented
in `UiManagerNativeLayout` and documented in `Startup.md`.
Coordinator review corrected the eighth argument from a mistaken `+8`
stack-offset label to ordinal 8 / frame offset `+0x24`, and corrected the
getter source to **address addition** `getter()+0x48`, not a field load.
Startup checks passed with those corrections; no wire format, grow policy,
record meaning, or descriptor lifetime was inferred.

## Aliased record sink drain remains unbound; producer iteration is concrete

The bounded read/drain search did not identify a consumer or reset of the
aliased descriptor. Raw `0xC5EF40` calls producer `0xC95400` while
iterating objects; producer flag decimal 1836 is the already recorded
`+0x72C`, not a new field. Descriptor append and its sixteen-byte stride
remain verified, but a per-frame clear or drain is not established.

A separate offset match in raw `0x8DDA10` feeds a helper that treats its
first DWORD as a handle, not this vector descriptor; offset equality
does not identify the same object. The focused scan's lack of another
matching global/offset access is a bounded search result, not proof of
absence of indirect readers. Drain ownership is parked. The next task
traces `0xC5EF40`'s concrete iteration bounds and producer-call gates.

## UI synchronization selection and callback suppression predicates

Raw `0xBFFA90` requires nonzero slot-4 getter result and ready byte
`+0x672`; otherwise it returns zero. On reached paths it returns one,
including when it issues no component call. A 20-byte initialized helper
record supplies two output DWORDs, here named `h0` and `h1`; their
provider/meaning is not bound by this report.

Selection starts with `+0x58C`. Unless byte `+0x590` and that value are
both nonzero, `+0x588` replaces it. If the result is zero, `+0x584`
is the fallback. Component `+0x974`'s slot `+0x1C` receives:

| Predicate | Arguments |
|---|---|
| byte `+0x670 == 0`, or byte `+0x598 != 0` and (`h0 == 0` or `h1 != selected`) | `(1, selected)` |
| byte `+0x670 != 0`, byte `+0x598 == 0`, and `h0 == 1` | `(0, 0)` |
| Otherwise | No component call |

The component return is ignored. Raw `0xBFF930`, call site `0xBFFA3F`,
updates `+0x58C/+0x590/+0x594` and invokes UI slot `+0xB0`.
It does not itself test ready; this is a concrete caller, not proof that
every invocation occurs after ready publication. The callee suppresses
dispatch before readiness. A bounded code task now projects these exact
positional decisions without assigning speculative field semantics.

## Record producer iteration uses eight-byte entries and independent call gates

Raw `0xC5EF40` tests byte `+0x2D4` and compares its float `+0x2C0`
against VA `0x1C818E8` at `0xC5EF7D`. For ordered finite values,
the observed path proceeds at or above the threshold. Unordered behavior
and operand order require instruction-level clarification before a projection.

The loop starts at parent `+0x5C`, ends on equality with `+0x60`,
and advances eight bytes at `0xC5F026`. Each record's first DWORD
is an object pointer; its second DWORD is not read here.
A qualifying object has its `+0x1C` pointee's byte `+0x7F` set,
object byte `+0x40` set, and DWORD `+0x70 != 2`
(`0xC5EFF2..0xC5F005`). Call `0xC5F01D` passes the object as ECX
and parent `+0x2B0` as argument to `0xC95400`.
Coordinate change gates an earlier virtual `+0x30` call, not this producer.

Matching initializer `0xC5CFE0` sets `+0x2D4=1` at `0xC5D116`,
zeros `+0x5C/+0x60`, and initializes `+0x2C0` to float one.
Concrete calls include `0xC88495`, `0xCC18A7`, guarded `0xCEE4A9`,
and `0xCF17F7`, plus tail-call `0xCF28E0`. Each invocation or duplicate
record can repeat production; these entries do not establish frame cadence.
The next task traces the supplied `+0x2B0` argument's effect on the four
source floats rather than assuming an untransformed record copy.

## UI synchronization projection integrated; record-order clarification pending

The pure `UiManagerNativeLayout.ProjectStateSync` now models the verified
gates, selection precedence, and optional component-call arguments, using
neutral offset-based inputs rather than inferred field meanings. Focused
Startup checks cover all byte values for gates/flags, exact helper-output
zero/one comparisons, fallback precedence, and suppressed calls.
Startup checks and `git diff --check` passed during integration.

The producer follow-up confirms raw `0xC95516..0xC9554E` uses bit-preserving
MOVSS transfers rather than arithmetic with the supplied parent argument.
Earlier opaque callees may still modify source state. It also corrects the
global guard: `0xC95507..0xC95514` dereferences UI global VA `0x1E002B8`
without checking that global pointer, then checks the `+0xA08` descriptor.
The prior `Startup.md` claim of a null-checked UI global is superseded.

The report's claimed local-memory order conflicts with its own listed
source/destination pairs, so exact record order is awaiting a bounded
instruction clarification; no reordered code projection is accepted yet.
Its C5/C9 addresses are raw IDB offsets, despite its mistaken VA labels.
At `0xC5EF75..0xC5EF84`, COMISS compares global float against parent
`+0x2C0` and JA exits for ordered global greater than field. Unordered
flags do not take JA; an unmasked floating-point exception remains outside
that branch-only observation.

## Exact local record order corrected from source register stores

The bounded clarification resolves the conflicting report. Raw `0xC95516`,
`0xC95521`, `0xC95529`, and `0xC95531` load source `+0x338` into XMM2,
`+0x344` into XMM3, `+0x33C` into XMM0, and `+0x340` into XMM1.
Stores at `0xC9553A/0xC9553F/0xC95544/0xC95549` place XMM0/XMM2/
XMM1/XMM3 at `EBP-0x34/-0x30/-0x2C/-0x28`.
LEA `0xC9551E` and push `0xC95539` pass `EBP-0x34` to the append.
Therefore actual ascending record order is **33C, 338, 340, 344**.
The earlier report's destination associations, not that ordering, were wrong.

`UiManagerNativeLayout`'s positional source constants and their regression
check now use this exact order, superseding the earlier ascending-source
assumption. The code still does not assign semantic names or wire meaning.
The successor task follows only pre-append calls and their possible source
state updates, not another scan of the now-resolved copy instructions.

## Synchronization helper record is USER32 CURSORINFO

Read-only PE import parsing binds IAT VA `0x1AC4C24` (raw `0x16C3824`)
to `USER32.dll!GetCursorInfo`. Raw `0xBFFA90` initializes a twenty-byte
record with `cbSize=20`, zeroes its remaining sixteen bytes, and passes
it to that import. Thus previously neutral `h0` is CURSORINFO flags
at `+4`, and `h1` is the PE32 cursor handle at `+8`; point coordinates
occupy `+12/+16`. This resolves the record provider without guessing
from its size. The existing exact-zero/exact-one predicates remain unchanged.

The API return is ignored; failure does not select a separate branch.
Pre-zeroing does not prove output remains zero if the API returns failure.
A code task now updates captured-input naming and layout evidence without
calling WinAPI or introducing a success-shaped failure fallback.

## Pre-append helper writes derived fields, not the four record source fields

At raw `0xC95428..0xC95443`, producer `0xC95400` passes output
`this+0x2DC`, input `this+0x98`, and its argument to `0xCCA190`.
That helper writes derived fields `+0x2DC..+0x2FF` and a boolean
at `+0x300` (`0xCCA24A..0xCCA32C`). The boolean and a comparison
against `+0x2EC` gate subsequent work at `0xC95450`.
Indirect slots `+0x34/+0x20/+0x28/+0x74` remain unresolved.
The `+0x74` call requires the `+0x1C` pointee's byte `+0x80`;
its return feeds helpers including `0xC225E0` and `0xCA90B0`.

The parent `+0x2B0` is the caller's supplied argument, not necessarily
`this+0x2B0` in the producer. No local write to source `+0x338..+0x344`
was identified; opaque virtual side effects remain possible.
The report's VA `0x1696000` is arithmetic error: raw `0xC95400`
maps to VA `0x1096000`. The successor follows only `0xCCA190`'s
concrete output arithmetic, not the unresolved virtual targets.

## Captured cursor observation is represented in reconstruction code

The UI synchronization input now names the PE-verified outputs
`CursorFlags` and `CursorHandle`, keeping the latter a 32-bit value.
Layout constants preserve GetCursorInfo's IAT and CURSORINFO offsets.
Focused checks retain exact zero/one comparisons, cover flags outside a
byte and high-bit handle differences, and leave the other fields neutral.
No WinAPI call or inferred zero-output-on-failure behavior is implemented.
The delegated Startup checks reported 849 passes and no failures.
The next bounded trace follows the cursor-selection setter's actual
arguments and sources, not another pass over the resolved API binding.

## Derived producer state is nine float slots followed by an independent byte

Raw `0xC95430..0xC95443` passes output `this+0x2DC`, input
`this+0x98`, and the parent's `+0x2B0` argument to `0xCCA190`.
Its output is nine floats at relative `+0..+0x20` and a byte at
`+0x24` (object `+0x300`), not a proven matrix or rectangle type.
The byte is one iff input byte `+1` and parent byte `+0x24` are nonzero.
This stored byte must remain distinct from the helper's scalar return.

Direct helper `0xCCA350` derives a pair using input offsets `+8`,
`+0x1FC/+0x200`, and `+0x20C..+0x218`, including FSINCOS and
single-precision stores. `0xCCA190` may rotate/translate that pair
using parent angle `+0x14` and coordinates `+0/+4`.
Other output slots are anchored as follows:

| Output | Observed operands |
|---|---|
| `+8` | parent `+8` plus input `+0x204` |
| `+0xC` | parent `+0xC` plus input `+0x208` |
| `+0x10` | input `+4` times parent `+0x10` |
| `+0x14` | Angle-path result, import/branch clarification pending |
| `+0x18` | input `+0xC` times parent `+0x18` |
| `+0x1C` | input `+0x10` times parent `+0x1C` |
| `+0x20` | input `+0x18` plus parent `+0x20` |

Ordered-equality tests at `0xCCA1AE..0xCCA1BA` and
`0xCCA35B..0xCCA369` select the direct pair branches; NaN does not.
Angle normalization crosses IAT VA `0x1AC4820`. Its exact import,
constant mapping, and unusual sign/unordered branch report are a separate
bounded follow-up before accepting a complete arithmetic projection.
Earlier virtual calls remain unresolved, so this derived-state trace does
not prove absence of indirect writes to the later record source fields.

## Selection setter copies a shared-key tree payload and a separate byte flag

Raw `0xBFF930` receives a pointer whose first DWORD supplies a shared-text
lookup key. Lookup resolves a tree node; when it differs from UI `+0x6C8`
sentinel, the node's `+0x14` DWORD is read at `0xBFF9ED` and copied
to UI `+0x58C` at `0xBFFA14`. Argument two's byte is copied to
`+0x590` at `0xBFFA2B`; `+0x594` is copied from UI `+0x990`
at `0xBFFA37`. Constructor `0xBF7A80` initializes `+0x584/+0x588`
to zero at `0xBF7C54/0xBF7C5E`, not to proven nonzero cursor handles.

No WinAPI call in this setter proves the tree payload's origin. Its use
in the cursor comparison is evidence of consumption, not independent
proof that every payload is a cursor handle. The successor follows the
tree's concrete publisher. The report's unresolved label for final
`0xBFFA3F` does not reopen the already composed UI vtable binding to
`0xBFFA90`; runtime replacement remains the existing qualification.

## Derived angle uses CRT fmod; correction applies to negative nonzero remainders

Read-only PE mapping resolves stub raw `0x159DD00`, bytes
`FF 25 20 48 AC 01`, through IAT VA `0x1AC4820` / raw `0x16C3420`
to `MSVCR120.dll!_CIfmod`. The on-disk IAT points at import-name data,
not a resolved runtime target; no bound-import directory is present.
Empty raw-IDB import enumeration is not a dynamic-resolver finding.

Mapped float constants: VA `0x1C5E1C4` / raw `0x185CDC4` contains
`DB 0F C9 40` (bits `0x40C90FDB`, float two-pi); VA `0x1C817A4` /
raw `0x18803A4` is zero, and VA `0x1C81BE8` / raw `0x18807E8` is one.
VA `0x1EDB020` is virtual-tail loader-zero data, not a file-backed pair;
later runtime writes are not excluded.

Raw `0xCCA29F..0xCCA2A8` supplies the sum and two-pi to the CRT stub.
At `0xCCA2BA..0xCCA2CC`, SETNBE predicates select correction for an
ordered negative original sum. In that branch, UCOMISS remainder/zero,
LAHF, TEST AH/0x44, and JNP skip addition only for exact zero.
Otherwise two-pi is added, including for unordered remainder (still NaN).
This supersedes the earlier incorrect "add only if unordered" interpretation.
Exact zero includes either sign, so a captured signed-zero remainder
passes unchanged. Output float `+0x14` is distinct from output byte
`+0x24`; the epilogue does not establish a meaningful EAX boolean return.
The next code task projects only correction of a captured CRT remainder,
not an unverified managed emulation of x87 `_CIfmod`.

## Captured-remainder correction implemented without emulating the native CRT

`UiManagerAngleCorrectionProjection.cs` projects the observed correction
from an original sum and captured remainder, preserving either signed zero
unchanged and treating the derived output byte independently.
Focused checks cover negative sums, exact multiples, NaN, zero signs,
positive sums, and all byte values for each flag input.
Coordinator review corrected hexadecimal `+0x14/+0x24` offsets that had
been transcribed as decimal 14/24, distinguished helper `0xCCA190` from
import stub `0x159DD00`, and represented float bits as `0x40C90FDB`
rather than a misleading integer named as byte order.
The next task traces which producer pre-work these derived gates suppress,
without assuming the derived helper has a meaningful scalar return.

## A concrete cursor registration publishes LoadCursorA's result

Raw `0xBFD6D0` calls IAT VA `0x1AC4C2C`, PE-bound to
`USER32.dll!LoadCursorA`, with `(NULL, 0x7F8A)`. Its result is passed
with a shared-text key and UI tree `+0x6C8` to `0xC06D40`.
That helper inserts only for an absent key. Allocator `0xC166E0`
stores key at node `+0x10` and value at `+0x14` (`0xC1675D`);
`0xC16790` links via `0xC1A450`.
This proves one registration's payload source, not that every selector
lookup must return such a handle. The successor follows tree cleanup.

## Derived state rejects producer work before the append flag is reached

Raw `0xC9544D`'s virtual `+0x34` call occurs before testing derived
byte `+0x300`. Zero at `0xC95456..0xC95458` returns zero.
COMISS/JA at `0xC9545E..0xC9546D` likewise rejects ordered global
threshold greater than derived float `+0x2EC`. Equality passes;
unordered branch flags do not take JA if the comparison completes.
Rejecting either gate skips virtual `+0x20/+0x28`, conditional `+0x74`
selectors two/one, and subsequent append work.

Thus append also depends on these earlier gates, not just `+0x72C`
and descriptor nonnull. UI global itself remains unchecked before its
`+0xA08` dereference. The report's C9-address VA labels are erroneous;
these are raw IDB offsets. A code task now projects captured branch
outcomes without emulating FP exceptions or unresolved virtual side effects.

## Cursor-tree teardown releases key and node, not the registered handle

The concrete parent destructor path `0xBF82A0 -> 0xBF8AC0` reaches
tree erase helper `0xC0C370` at raw `0xBF8F55`.
Node cleanup `0xC0F590` releases the shared key at `+0x10`, then
frees the node. It does not read or release payload `+0x14` in this
traced path, and does not pass that handle to DestroyCursor.
This is the tree cleanup's policy, not proof about every lifecycle path.
The report's "IDA VA" labels for these offsets are superseded by the
raw-IDB convention.

The producer-gate reconstruction now distinguishes earlier rejection from
later append admission and tests zero/nonzero bytes, equality, NaN and
infinities. It does not simulate FP exceptions, virtual calls, or append.
Cursor registration constants distinguish the registration method
`0xBFD6D0` from an individual call site, and node payload store
`0xC1675D` from its enclosing constructor. The delegated focused Startup
run reported 858 passes and no failures.

## Selection lookup miss shares completion; cache equality bypasses lookup

Raw `0xBFF95E` compares UI `+0x990` with `+0x594`; equality bypasses
lookup, flag updates, temporary cleanup, and synchronization.
Otherwise the `+0x6C8` tree is searched once through
`0xC21760/0xC21850`, with no alternate-name lookup found.
Sentinel or zero payload reaches conditional logging at `0xBFFA0C`
when the log threshold permits. Shared-text release at `0xBFFA21`
precedes `+0x590` and `+0x594` updates and `0xBFFA3F`'s sync call.
Miss is not a separate explicit-zero return. Exact zero-store/cache-return
details are awaiting a bounded instruction clarification.

Clarification: on every cache-miss completion, raw `0xBFFA14`
unconditionally writes EBX to `+0x58C`. EBX is zeroed at `0xBFF9D4`;
sentinel skips the payload load at `0xBFF9ED`, so a missing key clears
the selection rather than preserving an old value. A zero payload can log,
then reaches the same store. At `0xBFF958`, EAX loads `+0x990`;
equality at `0xBFF95E` returns that loaded value through `0xBFFA45`.
The clarification's newly subtracted `0x7FF...` offsets are invalid:
its `0xBFF...` IDB addresses were already raw, not runtime VAs.
Only the instruction evidence and original raw addresses are retained.

## Record vector growth separates count arithmetic from failure guard behavior

Raw `0xC97380` preserves an in-range source's record offset across
`0xC978F0` growth and rebases before its four-DWORD copy.
Growth uses `(end-begin)>>4` and `(capacity-begin)>>4`; required count
is used plus one, guarded at maximum `0x0FFFFFFF`.
If `max - floor(capacity/2) >= capacity`, it selects the larger of
required and capacity plus half; otherwise it selects required.

`0xC97FA0` requests count times sixteen through
`0x12B7FB0 -> 0x12C8A20`, copies, frees old storage, then publishes
capacity/end/begin. Null allocation reaches indirect IAT VA `0x1AC4424`;
max-count paths also cross indirect guards. Their precise binding and
nonreturn behavior must be resolved before claiming old-buffer failure
preservation or continuation. A separate code task projects only bounded
count selection, not allocation, pointer publication, or guard behavior.
These are raw IDB addresses despite the report's VA labels.

## Bounded vector count projection implemented independently of allocation

`NativeRecordVectorCapacityProjection.cs` records the sixteen-byte stride,
maximum count, guard-before-increment decision, and overflow-aware
capacity-plus-half selection. Inputs above the established count domain
raise argument errors; the projection does not claim native metadata
validation or impose a `used <= capacity` invariant absent from its scope.
Guard results have no projected count or bytes, rather than a fabricated
successful capacity. Maximum projected byte size is `0xFFFFFFF0`.

Focused checks cover small odd/even capacities, required-count precedence,
the exact half-growth boundary, maximum-used guard, and out-of-domain
arguments. The delegated Startup run reported 865 passes and no failures.
Allocation outcome, source rebasing, pointer publication, and guard
nonreturn behavior remain separate static contracts.

## Vector guards bind to standard-library exception helpers

Read-only PE parsing binds IAT VA `0x1AC4424` to
`MSVCP120.dll!std::_Xbad_alloc(void)`. Raw `0xC97FFB` calls it
without explicit arguments on allocation failure, bytes `FF 15 24 44 AC 01`;
fallthrough is `0xC98001`. Under the normal runtime contract it throws.
The EXE alone does not establish that an externally replaced implementation
cannot return; this qualification is not evidence that normal failure
continues into copy/free/publication.

At maximum used count, raw `0xC9791C` calls IAT VA `0x1AC45C0`,
bound to `MSVCP120.dll!std::_Xlength_error(char const*)`, after pushing
VA `0x1C5EA14`, mapped string `"vector<T> too long"`.
Normal behavior throws length_error; hypothetical fallthrough at
`0xC97922` can reach the allocation helper's over-limit guard.
The report does not supply that guard's exact call site, so it remains
distinct from these two verified call sites.

Only under hypothetical returning exception helpers does copy helper
`0xC9BBD0`'s null-destination skip/advance become relevant; it does not
establish a normal failure-preservation guarantee. No runtime finding is
claimed. A code task now records the verified imports and call sites
without simulating hypothetical memory accesses or external exceptions.

## Cache marker has a concrete same-class increment, not a proven frame cadence

Constructor raw `0xBF80AF` initializes UI `+0x990` to zero.
The installed table VA `0x1C73EBC` binds slot `+0x40` to raw
`0xC02730`; it takes ECX as EDI at `0xC0275D`, increments
`[EDI+0x990]` at `0xC02816`, and stores arguments at `+0x988/+0x98C`.
The same table's `+0x104` entry binds selector `0xBFF930`, which
compares that instance's `+0x990` against captured `+0x594`.
This supports an instance-local version/invalidation interpretation, not
a frame/time counter or proof of a particular external virtual caller.
The successor verifies the increment's surrounding gates and arguments.

The vector reconstruction now includes exact exception-import names,
decorated names, call sites, and the length-error message.
`StartupBinaryEvidence` verifies their PE module/name associations and
instruction bytes, independently of the synthetic count checks.
Coordinator integration ran Startup checks with `--binary` against the
authorized immutable sample and passed. Method `0xC97FA0` is labeled as
the reallocation method, not an exact guard instruction address.

## Cursor-cache projection represents state availability, not native return payload

`UiCursorCacheProjection.cs` projects captured version equality and completed
lookup refreshes; a missing payload maps to the observed stored zero.
Its cache-hit payload describes available cached state, **not** native EAX,
which returns the loaded version on the bypass path.
Coordinator review corrected the delegated documentation's payload-return
claim, increment site `0xC02816` versus ECX-to-EDI instruction `0xC0275D`,
and version-load `0xBFF958` versus compare `0xBFF95E`.
Unproven associations between setter arguments and fields `+0x988/+0x98C`
were removed from this projection pending the separate method trace.
The model does not execute lookup, synchronization, or version increment.

## Invalidation requires ready and stores two DWORD arguments after old-state work

UI table raw `0x1872AFC` (`+0x40` bytes) contains `30 33 00 01`,
binding raw `0xC02730`. A separate global counter increments at
`0xC02765` before ready-byte `+0x672` check at `0xC0276B..0xC02772`.
Zero ready skips to `0xC041EC` / `retn 8`, bypassing the object
version increment and argument stores. On the normal enabled path,
`0xC06670` runs before writes, then the global-init branch reconverges.
`0xC02816` increments DWORD `+0x990` modulo 2^32 once on that path.
`0xC0281C/0xC02825` store stack DWORDs `[EBP+8]/[EBP+0xC]`
at `+0x988/+0x98C`. These are not established cursor-key/byte fields.

The earlier helper reads that old pair as a QWORD and performs a
divide-by-1000/difference threshold calculation before callbacks.
Its exact arithmetic helper and signedness are a bounded follow-up;
the report's `C159DD10` target spelling is not accepted without verification.
Post-store component `+0x974` slots `+0x40/+0x44` receive temporary-buffer
pointers at `0xC02831/0xC0284E`, not the stored arguments directly.
No exact clock units or external caller cadence is inferred.

## Old-pair callback gate uses unsigned quotient truncation and modular subtraction

Raw `0xC06686` calls local helper `0x159DD10`, not an import and not
the earlier misspelled `C159DD10`. Bytes `E8 85 76 99 00` bind that call.
Arguments form the old `+0x988/+0x98C` unsigned QWORD and divisor 1000;
the helper uses unsigned DIV, and the caller retains only quotient EAX.
It computes unsigned modular `quotient_low32 - [this+0x20]`,
skips below 5000, otherwise stores the quotient at `+0x20` before callbacks.
Lists `+0x6B0/+0x6D8` dispatch entry slot `+0x14`; list `+0x5E4`
uses `0xC376A0`. Units and invocation cadence remain unproven.
The report's final VA example for helper `0x159DD10` is arithmetic error:
its code VA under the established mapping is `0x199E910`.

The ready-gated invalidation projection is integrated with all-byte and
uint-wrap checks, preserving prior state when ready is zero and projecting
the full two-DWORD stores only on the normal enabled write path.
It does not execute the preceding old-pair callbacks or guarantee completion.
Default `+0x584/+0x588` nonzero provenance remains unresolved after its
bounded search; only candidate `0xBF9D80` is being checked next, without
restarting a broad offset scan.

## Old-pair arithmetic projection integrated; mistaken candidate evidence rejected

`UiOldPairCallbackGateProjection.cs` combines captured words, divides
unsigned by 1000, truncates the quotient to 32 bits, and projects the
modular threshold and conditional field update. Focused checks cover
threshold boundaries, flooring, high words, truncation, and subtraction wrap.
Coordinator review separated tree offset `0x5E4` from method `0xC376A0`;
the delegated concatenation `0x5E4C376A0` was not an address.

The adjacent-method report is rejected: it transformed already-raw
addresses and inspected unrelated code, then claimed to invalidate the
established vtable. Coordinator reads on active IDA port 13337 confirm:
raw `0x1872B6C` is `90 06 00 01`; raw `0xBFFA90` begins
`55 8B EC 83 EC 14 56 8B F1 8B 06 8B 40 04 FF D0`;
raw `0xBF9D80` begins `55 8B EC 83 E4 F8 6A FF`.
These preserve the prior selector binding and disprove that report's
claimed raw function boundary. A narrowly corrected candidate task uses
those exact IDB offsets; default cursor sources remain unresolved meanwhile.

## Callback walks advance live tree nodes after receiver-only dispatch

After raw `0xC0669C` stores field `+0x20`, `0xC06670` visits trees
`+0x6B0`, `+0x6D8`, then `+0x5E4`.
The first two use node `+0x14` as ECX and invoke slot `+0x14`
without explicit stack arguments at `0xC066B0..0xC066B5` and
`0xC06710..0xC06715`. The third invokes `0xC376A0` on node
`+0x14` at `0xC06770..0xC06773`.
For the first two, successor is not captured before dispatch: after return,
the code examines current-node byte `+0x0D`, follows links `+8/+0/+4`,
then compares against a freshly read head. No snapshot/refcount operation
was observed; this does not establish mutation or thread safety.

`0xC376A0` then walks its own `+0xE8`, `+0x118`, and `+0x120`
containers, in order. The first pair dispatches node `+0x14`, the third
entry `+8`, all through receiver-only slot `+0x14`.
The next tasks record this static order and trace a concrete manager
registry publisher, rather than simulating tree mutation.

Corrected candidate `0xBF9D80` is a real raw function, unlike the rejected
earlier report: it gates on ready, works through fields `+0x6F0/+0x71C`,
and releases temporary references. Its reported negative search for
"584/588" used decimal offsets `0x248/0x24C`, so it does not resolve
the requested **hexadecimal** `+0x584/+0x588` default sources.
Its supplied converted VA was also incorrect. No default-source conclusion
is accepted from that mismatch; the default-source boundary is parked.

## Static traversal evidence integrated without simulating callbacks

The old-pair projection now carries read-only primary, direct, and nested
traversal records, preserving dispatch order and separating call ranges,
method targets, receiver offsets, and slot byte offsets.
Coordinator review corrected `+0x0D` to a **node-byte offset**, not
a comparison value of thirteen. Successor-link reads remain explicitly
after dispatch; no snapshot or safety guarantee is introduced.
The delegated Startup checks reported 886 passes and no failures.
Successor work follows nested-container cleanup and manager registry
publication, not another reconstruction of the established arithmetic.

## Manager callback publication acquires the key, not an observed payload reference

Raw `0xBFDE80`, reached from initialization, passes UI `+0x6B0` to
`0xC3C9B0` at `0xBFE2CB`. That helper derives a shared-text key,
creates/configures a payload through `0xC63D80/0xC64C90`, and inserts
through `0xC06A60`; existing keys are rejected.
`0xC164C0` stores node `+0x10` with explicit atomic key incref and
copies the payload pointer to `+0x14` without an observed payload incref.
`0xC16570` links, and `0xC08A20` releases the temporary key.
Failure invokes `0xC64420` and payload slot zero. Success publishes
the pointer for the later callback; the report's "tree-owned" interpretation
awaits a concrete successful-payload disposal path.

## Nested E8 payload cleanup follows VM close, unlike the callback walker

Raw `0xC376A0` is traversal only in the inspected function; no unlink,
free, or payload-release operation was observed.
Concrete script close `0xC33060` calls `0xC33E60` at `0xC33206`,
then `0xC3E990` at `0xC3327C`, then VM close `0xCD5510` at
`0xC33291`. Its `+0xE8` cleanup loop begins at `0xC33326`.
For each nonnull node `+0x14` payload, raw `0xC33352..0xC3335D`
invokes slot zero with argument one. This is payload-directed cleanup
after VM close, not only key/node erasure. Concrete slot-zero target and
node-storage release remain unbound by this report.
The next task follows only remaining `+0x118/+0x120` cleanup surfaces.

## Manager callback-node erase does not establish successful payload destruction

The manager destructor `0xBF8AC0` erases `+0x6B0` through
`0xC0BD90`, with per-node helper `0xC0EB00`. That helper releases
key `+0x10` then frees the node; no payload `+0x14` destruction occurs
in this inspected path. Insertion failure separately calls `0xC64420`
then payload slot zero with flag one. Successful-payload ownership remains
parked; callback slot `+0x14` is not destructor evidence or a leak finding.

## Nested cleanup performs ordered payload passes before container resets

After VM close at raw `0xC33291`, `0xC33060` visits payload trees
`+0xF0`, `+0xE8`, `+0xE0`, `+0xD8`, `+0x100`, `+0x110`,
`+0x118`, then `+0x140`. The `+0x118` pass begins at `0xC336DA`
and dispatches node `+0x14` through slot zero with argument one at
`0xC33706..0xC33711`. The pass itself advances links, not key/node frees.
Containers are then reset in that same order. For `+0x118`, helper
`0xC0BE90` runs at `0xC33973..0xC33993`, followed by sentinel-link
and count reset; its body is not established by this report.

Later `+0x120` reaches `0xC3FC10` at `0xC33C2B`, before
`+0x118` helper `0xC0BD90` and stored-object release
(`0xC33C3A..0xC33C58`). Its exact policy is the next bounded task.
These slot-zero cleanup passes remain distinct from `0xC376A0`'s
slot-`+0x14` callback traversal; matching fields do not merge the operations.

## Nested list cleanup frees backing allocation and nodes, not observed callback objects

Raw `0xC3FC10` receives container base for `+0x120`.
Container `+0` points to sentinel, `+4` is count, and conditional
backing pointer `+8` reaches allocator transfer `0xC3FC1A`.
Only when that pointer is nonnull are container `+8/+0xC/+0x10`
zeroed. It self-links sentinel next/previous and clears count.
The forward walk reads node `+0`, saves next before node free
`0xC3FC52`, and finally tail-frees sentinel at `0xC3FC61`.
It does not read node `+8`; the backing pointer must not be conflated
with per-node callback objects. The initial report's payload-release
interpretation is superseded by this instruction-level distinction.
Its reopened VirtualFree gap is also invalid: IAT VA `0x1AC42B4`
was already PE-bound; raw numeric-VA fill is not an import value.

The static nested teardown sequence and slot/argument distinctions are
now represented in `StartupObjectLifecycleNativeLayout`. Coordinator review
made the evidence sequences read-only and named node `+0x10` as a key
field, not a refcount location. Additional immutable PE checks and a
bounded list-publication trace are active; no callback-object ownership
is inferred from node erasure.

## Immutable lifecycle checks caught a receiver-setup address mislabeled as a call

The new binary checks verify shutdown's byte predicate versus word clear,
the selector table entry, CURSORINFO import/initialization, exact MOVSS
record order, and the old-pair divide/threshold/store. Their first run also
caught a tightly related scripting cleanup check failure.
Coordinator byte reads show raw `0xC33C2B` is
`8D 8B 20 01 00 00` (LEA receiver), while the actual call is
`0xC33C31`, bytes `E8 DA BF 00 00`, targeting `0xC3FC10`.
The lifecycle constant and Startup documentation now use the call address.
All Startup checks with `--binary` passed after this correction.
Earlier references above to a call at `0xC33C2B` mean receiver setup,
not the CALL instruction, and are superseded here.

The nested-list publication search did not establish an insertion writer
or independent owner for entry `+8`; prefix-tree publication of the outer
script object is not proof of callback identity. That boundary is parked.
The successor traces only initializer `0xC41670`'s concrete list state,
not another unanchored ownership search.

## Constructor report requires raw-address correction before integration

The `0xC41670` report incorrectly labels requested raw offsets as VAs
and subtracts the code delta, including for the allocator and cleanup
helpers. It also treats caller's `this+0x120` receiver address as a
stack argument value. Its new initializer/allocation assertions are not
accepted pending a narrowly corrected raw-IDB verification.
The existing separation between container `+8` backing and node `+8`
callback remains supported by the earlier exact cleanup instructions.
IAT VA `0x1AC4424` remains PE-bound to standard `_Xbad_alloc`;
another empty import query does not reopen it.

## Correct raw initializer confirms sentinel and separate sixteen-slot allocation

Direct raw-IDB verification confirms caller `0xC32A73` loads ECX with
`this+0x120`, then calls `0xC41670`; `0x120` is not a stack count.
The initializer zeros container `+0/+4` and backing triple `+8/+0xC/+0x10`.
Wrapper `0xC52A10` requests a twelve-byte sentinel and stores its
pointer at container `+0`. A separate vector-helper count of sixteen
requests a sixty-four-byte backing buffer. Both use
`0x12B7FB0 -> 0x12C8A20`, but they are distinct allocation requests.

This supports sixteen DWORD backing slots, not sixteen list nodes.
The exact fill source/values and remaining structural fields are the next
bounded task before calling the composite container a specific library type.
The corrected trace supersedes only the rejected mapping/caller assumptions;
callback entry ownership remains unbound.

The report's apparent VA lookup discrepancy is not a new mapping gap:
the IDB is raw/base-zero and a numeric runtime VA must not be used directly
as an IDB address. Raw `0xCD5510 + 0x400C00 = VA 0x10D6110`,
not the report's `0x10D6120`; the callback arithmetic above is consistent.

## Backing-slot fill and arithmetic regression continuation

The bounded raw-IDB slot trace reports `0xC416E2` supplies a local sentinel
pointer to `0xC53290` with count `0x10`. Capacity helper `0xC52D10`
multiplies by four at `0xC52D23`; fill helper `0xC52E50`, called at
`0xC5333E`, fills the sixteen DWORD slots with that captured sentinel pointer.
The initializer also stores `7` at `+0x14`, `8` at `+0x18`, and float bits
`0x3F800000` at `+0x1C`. These are structural observations, not yet proof
of hash-key semantics or ownership. A structural projection and focused
checks are assigned separately from one bounded key/bucket operation trace.

Immutable PE checks now bind the angle import thunk and two-pi constant,
the original-sum sign/exact-zero correction predicate, and the ready gate,
version increment, and both full-DWORD invalidation stores. Coordinator
execution of the focused Startup checks against the immutable sample passed.
This validates captured instruction evidence, not native execution or x87
emulation.

The structural reconstruction is now implemented as an immutable sixteen-slot
captured-sentinel projection with focused geometry, high-bit-pointer, and
field-value checks. Coordinator review removed an unsupported stack-operand
constant and corrected the documentation: count sixteen is distinct from the
address of the local source pointer. No native allocator is emulated.
The bounded lookup-shape search found no anchored key/bucket operation;
semantic container identity remains parked, not disproved.
Active successors verify initializer PE anchors and separately trace UI child
`+0x24` release from the concrete manager destructor.
