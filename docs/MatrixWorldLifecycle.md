# Matrix world and engine lifecycle

### tfLogin update `0x638B89` guard and `{2,0}` queue consumer

Observation (raw offsets, bytes checked against `.original`): at `0x638AB5`
`C7461C00000000 A1A4CAE501 8A4040 84C0 0F84BD000000` clears `[esi+1Ch]`,
loads the pointer cell at VA `0x1E5CAA4`, tests byte `+0x40` of its pointee,
and jumps to `0x638B89` when zero. When nonzero it tests
`[0x1E50C8C+0x40]` and `[0x1E6075C+0x40]` (conditionally
sending message strings via `0x1E69348` vtable `+0x88`), then queues mode 3
through `0x30D590`. `0x638B89` builds `{2,0}` and calls `0x30DEB0` on
`[0x1DFFD98]`. `0x30DEB0` checks `[this+0xC]` and request mode 1 (dedupe vs
current-mode object `[this+0xA28]`), else pushes the pair into the queue at
`this+0xB84` (`0x315060`). `0x30D850` drains it; mode 2 -> `AppMode_World_Ctor`.

Not found: any writer of the pointer cell at VA `0x1E5CAA4`, or a link from
message `0x145`.
Interpretation: `{2,0}` requests World. The pointer cell's semantic role,
writer, and relationship to successful login remain unresolved.

#### Switch selector, jump table and `0x1E5CAA4` writer search

Observation (bytes checked against `.original`): `0x6385C0` is a per-frame
update (`this=ecx->esi`, state `[esi+0x9C4]`), not a message handler. At
`0x6388D8` `8B500C 83FA04 0F87CF010000 FF24951C99A300` loads
`edx=[eax+0xC]` where `eax=[esi+0x1C]`, the first node of the list at
`[esi+0x20]` (`mov ecx,[esi+0x20]; mov eax,[ecx]; mov [esi+0x1C],eax`), then
`cmp edx,4; ja default; jmp [edx*4+0xA3991C]`. Table raw `0x638D1C` (5 DWORDs):
cases 0,2,3,4 -> VA `0xA3983A` (raw `0x638C3A`); case 1 -> VA `0xA395F2` (raw
`0x6389F2`). Values >4 take the `ja` target raw `0x638AB3` (`89 01` store, then
`0x638AB5` `mov [esi+1Ch],0` and the `0x1E5CAA4` test).
Case 1 builds a string from `[node+0x10]` (fallback `0x1C5EA68`), sends message
`0x146` via `[0x1E002B8]` vtable `+0xAC` (`6846010000 8B01 FF90AC000000` at
`0x638A3B`), destroys the node (`vtable[0](1)`), clears `[esi+1Ch]`, and if
`[esi+0xA48]==6` sets it to 5 with a deadline of `GetTickCount+0xBB8`.
Cases 0/2/3/4 share `0x638C3A` (tests `[esi+0x14]`, calls `0xAE3C60`, updates
`[esi+0x9D0]`), no distinguishing code.
The `0x63C1E2` site (message `0x145`) lies in a different function (after the
`ret` at `0x63C0A7`); it switches on its own object's `[+8]-1` (0..3, table VA
`0xA3D1D4`) to pick log strings, and I found no store of a `[node+0xC]` value
or call into `0x6385C0` in it. So `0x145` provably emits no case of this
switch directly; the producer of the `+0x20` list nodes was not located.
`0x1E5CAA4`: the byte pattern `A4CAE501` occurs once in the file (raw
`0x638ABD`), and `.reloc` has exactly one HIGHLOW entry for it (`0xA396BD`).
So no direct `mov [0x1E5CAA4],...`, `lea`, `push`, or data pointer exists. The
initial image holds `05050505`. Neighbouring `0x1E5CAAC..0x1E5CAD0`
belong to a table wrapper whose initializer is at `0xB637F0`; sibling slots
`0x1E5C8A4`/`0x1E5C9A4` pattern likewise have readers only. Interpretation:
the slot is written indirectly (computed base/loop or struct copy), not found.
Whether the case-1 node is an error/notification event is inferred from the
`0x146` send only.

### World scene-factory root and owned renderer interface

The static sample is
`D:\SteamLibrary\steamapps\common\Firefall\system\bin\FirefallClient.exe.original`
(40,197,848 bytes; SHA-256
`E0E8448393AC1CE05BE73E06F62DD70E3EFF3404CC2E7AAF3F04C79DEC0DE235`).
The following raw code ranges and data bytes were re-read from that file;
the database's flat/raw code addresses are not treated as PE VAs.

**Observations**

- Factory raw `0x9B73F0` allocates `0xA0` bytes and calls constructor
  `0x9B82F0`. The constructor initializes most root child slots; scene
  pointers at `+0/+4/+8` are populated later, and not every root byte is
  initialized. No root vfptr store or root RTTI descriptor was observed. The
  returned pointer is `EDI`, moved to `EAX` at `0x9B82B4`; `RET` is at
  `0x9B82C6`.
- Through indirect calls (global VA `0x1DFE970`, getter slot `+0x3C`, then
  returned object's slot `+0x30`), it looks up the literal scene keys and
  stores returned pointers at root `+0`, `+4`, and `+8`:
  `RenderScene` (raw string `0x179BD28`), `WorldMapScene` (`0x179BE38`),
  `SinvironmentScene` (`0x179BE48`). Each pointer is checked non-null before
  setup continues. The actual virtual targets and resource-provider type are
  unresolved.
- Factory allocations are recorded as `(root field, byte count, conditional)`
  in `SceneFactoryNativeLayout.KnownChildAllocations`; those records preserve
  observed allocation sizes and field destinations, not semantic class names.
  In the `TfWorld_Initialize` call, ECX is zero; the optional allocations at
  root `+0x2C`, `+0x50`, and `+0x88` are therefore on the taken path.
- Root `+0x14` receives a `0x188`-byte child. Constructor `0x9B5440` receives
  root `[0]` and installs vtable VA `0x1C70968`; following the original PE
  section/RTTI records identifies this child's type as
  `.?AVtfPhysicsDrawInterface@@`. Its constructor stores the scene pointer at
  child `+0x44`, clears `+0x48/+0x4C`, copies four floats from
  `xmmword_1DFF438` to `+0x50..+0x5C`, clears byte `+0x60`, and initializes
  bounds at `+0x16C..+0x180` to `FLT_MAX`/`-FLT_MAX`.
- Root `+0x10` gets the result of a helper called with global VA `0x1E001A0`,
  a per-call object, and static descriptor VA `0x1DFC4FC`. The destructor
  `0x9B8490` invokes the handle's vtable slot 0 with argument 1 and clears the
  field. It also destroys/releases child objects in their root slots and
  releases the three scene references; this supports ownership, but does not
  identify the registry's higher-level meaning. The factory also copies the
  pair at root `+0x44` into an 8-byte temporary and passes it to an insertion
  helper on the container at `[root+0x3C]+0x3C`; the registered object's type
  and exact semantics remain unresolved.
- `TfWorld_Initialize` calls the factory at `0x64B500`, stores its result in
  `tfWorld+0x14` at `0x64B508`, and on success publishes root `[0]` to global
  VA `0x1E00060+0xF8`. The other static callsite is `0x9B2D1A`; that caller
  stores the result at its object base.

**Fingerprint checks**

`StartupBinaryEvidence` checks the allocation, constructor call, three
lookup/store instruction sequences, exact NUL-terminated scene keys, return
sequence, `tfWorld+0x14` store, child allocation/constructor and vtable write,
RTTI chain/name, registration-handle sequence, and destructor clearing of
root `+0x14`. The vtable VA is `0x1C70968` (file offset `0x186F568`); its
complete-object-locator VA is `0x1CC7700`, type-descriptor VA `0x1E28A34`,
and RTTI name file offset `0x1A26E3C`. These are checked using PE section
mapping, not by subtracting a universal VA delta. No runtime execution or IDB
write was performed.

### tfPhysicsDrawInterface vtable, constructor and global scene publish

**Observations** (original PE, raw offsets re-read; VA = raw + `0x400C00` in
`.text`)

- Vtable VA `0x1C70968` has 14 slots, all pointing into `.text` (VAs
  `0xDB7530, 0xDB6440, 0xDB6530, 0xDB65C0, 0xDB66A0, 0xDB6960, 0xDB6A60,
  0xDB6EC0, 0xDB7160, 0xDB7330, 0xDB62F0, 0xDB7340, 0xDB7460, 0xDB74F0`; raw
  `0x9B6930, 0x9B5840, 0x9B5930, 0x9B59C0, 0x9B5AA0, 0x9B5D60, 0x9B5E60,
  0x9B62C0, 0x9B6560, 0x9B6730, 0x9B56F0, 0x9B6740, 0x9B6860, 0x9B68F0`).
  The next dword (`0x1CC76EC`) is not a `.text` address.
- Constructor `0x9B5440` first stores base vtable VA `0x1C709B8`, calls
  `0xDBC170` on `this+8`, zeroes `+0x28/+0x2C/+0x30/+0x34/+0x38`, stores the
  scene argument at `+0x44`, then overwrites `[this]` with `0x1C70968`. It
  clears `+0x48/+0x4C`, copies four floats from `0x1DFF438..444` to
  `+0x50..+0x5C`, clears byte `+0x60`, and writes `0x7F7FFFFF` to
  `+0x16C/+0x170/+0x174` and `0xFF7FFFFF` to `+0x178/+0x17C/+0x180`. If
  `+0x44` is null it calls an indirect assertion-like function pointer
  (`[0x1C15EA0]`) with a `0x7F` argument; the callee is unresolved.
- Slot 0 stores the vtable back, pushes 1, calls a helper, and conditionally
  tests the delete flag (`[esp+8] & 1`) before a second call.
- Slot 9 (`0x9B6730`) is a thunk: `jmp [vtable+0x20]`, i.e. slot 8.
- Slot 10 returns false immediately when argument 2 has bit `0x2000` set.
- Slot 13 (`0x9B68F0`) sign-extends a 24-bit argument, logs the string
  `DebugCollision:ReleaseModelObject(model %i)` (VA `0x1C70834`), then calls
  `[global 0x1DFE970 vtable+0x48]` and tail-jumps to returned object's
  slot `+0x34`. Slot 11 uses the same `+0x48` getter with flag `0x200`;
  slot 12 uses another global (`0x1E732C0`, slot `+0x64`).
- Slots 1-8 use the scene pointer/float math (SSE, `+0x30/+0x34` float
  reads from an argument) with float stack arguments.
- Publish at `0x64B528`: `mov ecx,[eax]` (root `[0]`), `mov eax,[0x1E00060]`,
  `mov [eax+0xF8],ecx`. `0x1E00060` holds a pointer; the store is to the
  pointed-to object's `+0xF8`, not to the global's own address plus `0xF8`.
  A null factory result logs string `Unable to create global scene`
  (VA `0x1B635CC`).
- The three scene lookup targets remain indirect through runtime global
  `0x1DFE970`; static bytes do not name their classes.

**Interpretation** (not proven)

- Slots 1-8 are likely debug-draw primitives and 13 a model-release
  callback, from float-heavy bodies and the log string; names unverified.
  Root `[0]` (RenderScene result) is plausibly the "global scene".

### Scene factory named lookup: oRenderSystem getter -> oD3DRenderAPI create-scene slot

**Observations** (original PE, raw offsets; IDA static read-only, bytes re-read):

- Global VA `0x1DFE970` (raw `0x19FCD70`) is **statically initialized** in
  `.data` with vtable VA `0x1C7870C` (raw `0x187730C`); no code store of a
  constructed pointer to it was found (all ~2,880 xrefs are `mov eax,[g]` /
  `mov ecx,offset g` virtual-call pairs). RTTI via COL `0x1CD59A4` ->
  type descriptor `0x1E3103C`: `.?AVoRenderSystem@@`. Its only code writer is
  an exit-time reset stub at raw `0xE93F70`
  (`C70570E9DF0184 87C701 B870E9DF01 C3`) that stores the following
  vtable `0x1C78784` (all slots `0x199DFC8`).
- Factory raw `0x9B7D11`..: `mov eax,[g]; mov ecx,g; call [eax+0x3C]` then
  `push <name>; mov edx,[eax]; call [edx+0x30]`. Slot `+0x3C` of the
  `oRenderSystem` vtable is raw `0xE96380`: `A1 FC05DE01 C3`, i.e. it only
  returns the pointer stored at global VA `0x1DE05FC`.
- `0x1DE05FC` is written at raw `0xED17DC` (`A3FC05DE01`, in the function at
  `0xED1650`, which is itself `oRenderSystem` vtable slot `+0x34`) after
  `new 0x49B8` (`68B8490000E8FC673E00`, raw `0xED17AA`) and the constructor at
  raw `0xF05FE0`, which stores vtable VA `0x1C79ED4` (`C707D49EC701` at
  `+0x3C`). RTTI: type descriptor `0x1E311F0`, `.?AVoD3DRenderAPI@@`. A second
  store at `0xED196B` assigns static `0x1E72DF8` (alternate path, not traced).
- `oD3DRenderAPI` vtable slot `+0x30` is raw `0xF255A0`: allocates `0x18F48`
  bytes and calls `0xF0FCD0(obj, this, name)`. That routine passes the name
  to base ctor `0xE11E40`, stores vtable `0x1C7A334` (`+0x46`), and inserts
  the object into a 32-entry static table at VA `0x1EDACD8` at the first free
  index (`893C85D8ACED01 89877C020000` at raw `0xF104C2`: table entry and
  `this+0x27C`).

**Follow-up observations: scene class, name, ctor fields, `0xED196B`**
(IDA static read-only; bytes checked against `.original`)

- Vtable `0x1C7A334`: COL `0x1CD683C`, RTTI `.?AVoD3DScene@@`, hierarchy
  base `.?AVoScene@@`. Base ctor `0xE11E40` stores `oScene` vtable `0x1C77A94`
  (COL `0x1CD419C`, `.?AVoScene@@`), so `0xF0FCD0` is the `oD3DScene` ctor
  (`C70734A3C701` at `+0x46` overwrites the base vtable).
- Name: `0xF255E1` pushes name then `this`; `0xF0FCFE` forwards it
  (`mov eax,[ebp+0xC]; push eax`) to `0xE11E40`, which calls
  `VIBE_AssignSharedTextFromCString(obj+8, name)` (`E8F2B34DFF` at base `+0x19`)
  and zeroes `+0xC/+0x10`. No compare or hash of the name appears in the
  `0xF0FCD0` body or the base ctor (the only use of the name argument in
  `0xF0FCD0` is the forward at `0xF0FD00`). Readers of `obj+8` were not traced.
- Ctor fields: `+0x27C` registry index (`0xF104C9`); `+0x284`=0 byte;
  `+0x290`=8; `+0x1758` array (6 x `0x188`); `+0x2090` = result of a
  `[[0x1DFE970]+0x40]` object's slot `+0x60` call given
  `"engine/textures/default_cube.dds"` (VA `0x1BEE1E4`; string verified);
  `+0x17C28` = `0x12CE400()-1` if positive, else `lock xadd [obj+0x18F00]`;
  `+0x18F38/+0x18F3C(-1)/+0x18F40`. On a full table the ctor logs
  `"Too many scenes were allocated.  Max is %i\n"` (VA `0x1BEE208`) with `0x20`
  via `[[0x1E60EA4]]` slot `+0x24` and still returns the object (`+0x27C`=32).
- `0xED196B` is `mov [0x1DE05FC], 0x1E72DF8` (`C705FC05DE01F82DE701`) after
  `0xED194C` deletes the old `0x1DE0600` object (`8B01 6A01 FF10` = slot 0
  with arg 1) and reloads `0x1DE05FC`. File-image initial values:
  `0x1DE05FC=0x1E72DF8`, `0x1DE0600=0x1E730E0`, `0x1DE0604=0x1E73230`.
  Stores to `0x1DE05FC` by raw byte pattern: `0xED17DC`, `0xED196B`, and
  `0xED1AC4` (inside a no-function byte region; not decoded).

**Interpretation** (not proven)

- `0xF255A0` is a by-name scene factory returning a new `oD3DScene`; the name
  is only retained as `oScene` text, not used as a registry key. `0xED196B`
  looks like the render API's shutdown path restoring the static placeholder
  after virtual deletion of the previous object; that it is a teardown (vs
  re-init) is inferred from `push 1; call [vtable+0]`, not proven.

**Interpretation** (not proven)

- The "lookup" is plausibly a create-by-name scene factory (allocation +
  construct on each call), not a name->existing-object map. The scenes are
  registered by index, not by name, in the `0x1EDACD8` table; the name is only
  stored by the base constructor (field not traced). No name-keyed registry was
  found within this budget. `oRenderSystem` slot `+0x3C` looks like a
  "get active render API" accessor.

## tfLogin pending-message cell (static, read-only; raw offsets)

**Observation** (bytes checked against `.original`):

- Ctor `0x6365B0` stores stack arg1 at `this+0x20` and arg2 at `this+0x24`
  (`0x63664E`), and zeroes `this+0x1C`. Vtable `0x1C676C0` (base `0x1C67710`).
- The only static ctor caller is the App mode-apply function (`0x30DC1F`),
  which passes `&App+0x9E8` as arg1 (`lea eax,[esi+9E8h]` at `0x30DC18`).
  The App ctor zeroes `App+0x9E8/+0x9EC` (`0x303104`); the mode-apply code
  clears `App+0x9E8` after other mode ctors (`0x30DB8A`).
- Update `0x6385C0` loads `ecx=[this+0x20]`, `eax=[ecx]` and, if it differs
  from `[this+0x1C]`, stores it in `+0x1C` (`0x638897`/`0x6388C5`). It then
  switches on `[node+0xC]` (`<=4`, table VA `0xA3991C`, raw `0x638D1C`):
  entries 0,2,3,4 -> raw `0x638C3A`, entry 1 -> raw `0x6389F2`.
- Case 1 builds a message with format `0x1B61A78` and text `node+0x10`
  (default `0x1C5EA68` when null), calls vtable `+0xAC` on the receiver
  `[0x1E002B8]` with id `0x146`, then virtual-deletes the node
  (`push 1; call [vtable+0]`) and zeroes `this+0x1C`; if `+0xA48==6` it sets
  `+0xA48=5` and `+0xA50=tick+0xBB8`.
- IDA has no function/xrefs for the case-1 block (`0x6389F2`).

**Interpretation / unresolved**

- `+0x20` is a pointer to a single-slot cell holding one polymorphic node
  pointer, not a node list; the "list" reading is not supported. Selector
  case 1 is the only provable name: "send text via notification 0x146".
- **Producer search pass 2 (observation, bytes checked against `.original`):**
  a displacement-`0x9E8` byte scan of `.text` found only these App-class
  sites: ctor zeroing `0x303104` (`C787E8090000 00000000`, `+0x9EC` at
  `0x30310E`), mode-apply clear `0x30DB8A`, reads `push [esi+9E8h]` at
  `0x30DAF7`/`0x30DB6F` (value passed as arg to two other mode ctors
  `0xA5AD90`/`0xA4BA40`), the `lea` at `0x30DC18` (tfLogin ctor), and the
  App dtor funclet `0x16000E9` (`add ecx,9E8h; jmp 0xA37730`). Other
  `+0x9E8` hits (`0x543488`, `0x54B1C6`, `0x636802`, `0x8E1296`,
  `0xB5C457`, `0xB619C7`, `0xBFADA1`, `0xD4032D`, ...) belong to other
  classes (different surrounding fields), not App.
- `0xA37730` (raw `0x636B30`) is a ref-counted release: `ctrl=[ecx+4]`;
  `lock xadd [ctrl+4],-1` -> `call [ctrl_vtbl+0]`; `lock xadd [ctrl+8],-1`
  -> `jmp [ctrl_vtbl+4]`. Interpretation: App+0x9E8/+0x9EC is a
  shared_ptr-like `{ptr, ctrl}` pair, so `[cell]` is the managed pointer.
- tfLogin methods only read `[this+0x20]` (`0x638897`, `0x6388C5`; the
  init path at `0x63694D..0x636970` asserts the cell pointer and `[cell]`
  are non-null/empty, lines `0x117/0x118/0x119`); none store through it.
  Node class/vtable/size, selector values and the triggering event remain
  **unproven**: the producer must reach the cell by an indirect path
  (SIB/computed address or a shared_ptr assignment helper) that the
  displacement scan does not cover. Next step: xrefs to `0xA371B0`'s
  arg3 (`App+0xBE4`) and shared_ptr assign helpers with `App` as target.
- No producer of nodes was found: no writer of `App+0x9E8` besides the
  zeroing sites above, and `Login_HandleLoginResponse` was not traced to a
  node store. Selectors 0/2/3/4 and per-producer values remain unproven.
  The `0x9E8` scans were displacement-byte matches and are not exhaustive.

### Store at raw 0xED1AC4 and name-keyed registry search (static, read-only)

**Observed** (bytes checked against `.original`; disassembled from the file image):

- `0xED1AC4` = `C705FC05DE01F82DE701` = `mov [0x1DE05FC], 0x1E72DF8`, inside the
  function starting at raw `0xED1A10` (`cmp byte [0x1E49B7F],0; push edi; mov
  edi,ecx; je end`; `ret`). That function is `oRenderSystem` vtable slot `+0x38`
  (vtable `0x1C7870C`: slot `+0x34`=`0xED1650`, `+0x38`=`0xED1A10`, `+0x3C`=
  `0xE96380`). No direct call xrefs exist.
- Body: thread check (`[0x1AC43B0]` vs `[0x1E49B24]`, assert-like call); virtual
  call `[[0x1DE05FC]+0xC](0)`; `[[0x1DFE9C8]+4]()`; then for `0x1DE0608`,
  `0x1DE0604`, `0x1DE0600`, `0x1DE05FC` in order: load old pointer, store the
  placeholder (`0x1E61E0C`, `0x1E73230`, `0x1E730E0`, `0x1E72DF8`), and if the old
  pointer was non-null call virtual slot 0 with arg 1. Then `esi=[edi+4]`,
  the `0xED1AC4` store, `[0x1E49B7F]=0`, and if `esi` non-null `call 0x11F3650`,
  `call 0x16C9960`; finally `[edi+4]=0`. Condition: runs only if the byte
  `0x1E49B7F` (initialized flag) is non-zero.
- `0xED196B` is in `0xED1650` (slot `+0x34`, the init that publishes the new API
  at `0xED17DC`), in a block with the same four-object delete/placeholder
  sequence, followed by a log with `"Failed to initialize \"%s\" rendering API\n"`
  (VA `0x1BE934C`). Both stores write the same value `0x1E72DF8`.
- Raw search for the VAs of `RenderScene`/`WorldMapScene`/`SinvironmentScene`
  finds only the factory at `0x9B7D23/0x9B7D42/0x9B7D62`; no other code
  references them.

**Interpreted** (not proven): `0xED1AC4` is the render-system shutdown that
resets the API pointer to the placeholder; this corrects the earlier guess that
`0xED196B` is a shutdown: it is the init-failure rollback. The name-keyed
"lookup" remains a create-by-name call (`0xF255A0`) with index registration;
no name-keyed registry was found.

**Unresolved**: callers of slot `+0x38`/`+0x34` (indirect); what the
`0x1DE0600/04/08` objects are; readers of `oScene+8`; a binary-wide negative
for name-keyed registries (only direct string refs were searched).
### oRenderSystem init/shutdown callers and manager globals (static, read-only)
- **Observed.** A scan of every function referencing `0x1DFE970` (537) for
  `call [reg+0x34]`/`[reg+0x38]` whose register is the vtable loaded from, or
  `ecx` set to, that global finds exactly one real init call: raw `0xE53C72`
  (`mov eax,[0x1DFE970]; mov ecx,0x1DFE970; ... call [eax+0x34]` with six pushes;
  `0xED1650` ends `retn 0x18`). It is inside `0xE53AC0`, whose only direct
  caller is `0xE550D5` (in `0xE54BF0`, after `0xE546F0` returns true). Its next
  step calls getter `+0x3C` and slot `+0x13C` on the result. Other `+0x34/+0x38`
  hits are on objects returned by getter `+0x48`/`+0x3C`/`+0x44` or on
  unrelated objects, not on oRenderSystem.
- **Observed.** No direct `call [reg+0x38]` on oRenderSystem exists in any
  function referencing the global; `0xED1A10` (no xrefs; undefined in the IDB,
  decoded from file bytes) takes no stack args (`ret`). It is not called
  directly by name either, so its caller is **unresolved** (likely via a copy of
  the pointer or a path not tied to the global).
- **Observed.** Init `0xED1650` allocates and publishes (raw offsets of stores):
  `0x49B8` bytes via ctor `0xF05FE0` -> `0x1DE05FC` (vtable `0x1C79ED4`,
  RTTI `oD3DRenderAPI`); `0x190` via `0xF4F7D0` -> `0x1DE0600` (vtable
  `0x1C7A520`, `oD3DTextureManager`); `0x268` via `0xF5F880` -> `0x1DE0604`
  (vtable `0x1C7A618`, `oD3DShaderManager`); `0x9C` via `0xF800C0` ->
  `0x1DE0608` (vtable `0x1C7A874`, `oD3DModelManager`). Shutdown calls the render
  API's slot `+0xC(0)`, `[0x1DFE9C8]` slot `+4`, deletes (slot 0, arg 1) the
  model, shader, texture managers then the render API, restoring placeholders
  `0x1E61E0C`, `0x1E73230`, `0x1E730E0`, `0x1E72DF8` and clearing `0x1E49B7F`.
- **Interpreted.** `0xE53AC0` is an application/window start routine passing
  window-size/fullscreen-like fields (`edi+0x138/0x13C/0x140`, `edi+0x130==1`).
- **Unresolved.** Shutdown's caller, the meaning of the six init arguments, and
  the `0x1DFE9C8` object type.

### Render init arguments, oEngineSystem, and the oShaderBuilderSystem global

- **Observed.** `0xE53AC0` (non-virtual, `this`=edi) pushes the six `0xED1650`
  args at `0xE53C42..0xE53C6F`: arg1 = `(this+0x130 == 1)`, arg2 = `this+0x138`,
  arg3 = `this+0x13C`, arg4 = `this+0x140`, arg5/arg6 = window client
  width/height from `GetClientRect` (clamped >= 1). `0xED1650` forwards all six
  to oD3DRenderAPI vtable (`0x1C79ED4`) slot +8 = `0xF08680`, which stores them
  at api `+0x4928` (byte), `+0x492C`, `+0x4930` (clamped >= 1), `+0x4934` (raw),
  `+0x493C`, `+0x4940` (clamped >= 1). `this+0x130` is a window mode (1 and 3
  are fullscreen-style: popup style, mode 3 sizes to `GetSystemMetrics(0/1)`;
  mode 1 is promoted to 3 when `this+0xC0 > 1`). After init the code reads the
  display values back via api slot +0xA4 and compares them with cvars whose
  descriptor names are `rd.fullscreenWidth`, `rd.fullscreenHeight`,
  `rd.fullscreenRefresh`, `rd.windowWidth`, `rd.windowHeight`, `rd.videoMode`
  (descriptors at `0x1E6A830..0x1E6AA38`, constructed near `0xE53CF3..0xE53DC5`).
- **Observed (class).** `0xE54BF0` is vtable slot 0 of `0x1C77C2C`; its COL names
  `.?AVoEngineSystem@@` (bases: `oEngineSystem`, `oEngineInterface`). `0xE53AC0`
  is a non-virtual helper (sole caller `0xE550D5` in `0xE54BF0`, after
  `0xE546F0`); neighbouring slots 20/21 are `0xE53A40`/`0xE53FA0`. `0xE54BF0` logs
  "Initializing Main Window", "Operating System", memory and
  "--- Initialization ---"/"--- Finished Initialization (%.2f seconds) ---".
  It has no direct callers (virtual); the vtable is stored by a ctor at `0xE4DFE0`.
- **Observed (`0x1DFE9C8`).** Static by-value global whose initial vtable
  `0x1C79E00` has COL -> `.?AVoShaderBuilderSystem@@`; `0xED1650` calls its
  slot 0 with the render API pointer, `0xED1A10` calls slot +4.
- **Interpreted.** arg1 = fullscreen flag; arg2/arg3 = back-buffer/display
  width/height (`rd.fullscreenWidth/Height` or window values depending on mode);
  arg4 = `rd.fullscreenRefresh`-style third display value (stored unclamped, so
  not a dimension); arg5/arg6 = window client size. Pairing of each field with a
  specific cvar is inferred from comparison order, not proven.
- **Unresolved.** Exact cvar-to-field mapping; caller of ctor `0xE4DFE0` and the
  path up to the app main loop; whether `oEngineSystem` slot 0 is `Init`.

### oEngineSystem construction and vtable (static pass)

- **Observed (addressing).** The IDB maps code at raw offsets; the doc's "ctor `0xE4DFE0`" is
  imprecise: the constructor function starts at raw `0xE4DFA0` (size `0x15E`) and stores the
  vtable VA `0x1C77C2C` at raw `0xE4DFDE` (`C7 07 2C 7C C7 01`). It zero/initialises
  fields through `+0x168` (`+0x164 = 0x8F`, `+0x134 = 1`, `+0x158/0x15C = -1`).
- **Observed (constructor caller).** Sole direct caller is `0xE4E660` (call at `0xE4E6E3`):
  allocates `0x170` bytes via `0x12B7FB0`, runs the ctor, stores the object at `factory+0x10`,
  copies old list head `dword_1E4B0A8` to `factory+0xC`, makes `factory` the new head, sets
  `factory+4 = 1`. It has no direct callers: it is slot 1 of a factory vtable
  (VA `0x1C77C1C`, raw `0x187681C` = `{0x124F410, 0x124F260(=0xE4E660), 0x124F320}`).
- **Observed (factory).** The factory is a static `.data` object at VA `0x1E00460` (raw
  `0x19FE860`, holds `0x1C77C1C`). Its static initializer at raw `0xE4F630` links it into
  list head `dword_1E4B0A4` (`+8` = next) and sets vptr/flag. No `0x1E00470`
  (instance slot) code reference exists except accessor `0xE4E630` (`mov eax,1E00470; ret`),
  which has no direct call/jump xref.
- **Observed (who runs the factories).** `0x12C3A80` (called from `ClientApplicationBootstrap`
  `0x2EAE70` at `0x2EAF3B`) walks `dword_1E4B0A4` and calls vtable slot 1 (`+4`) of every
  factory whose `+4` byte is 0, i.e. it creates `oEngineSystem` (and its siblings).
  Upward chain: `0x2E9630` (wrapper) <- `0x136AEDA` in `CrtInitializeAndInvokeClient`
  (`0x136ADBA`, the CRT start / WinMain-equivalent). `ClientApplicationBootstrap` then
  runs `ClientApplicationInitialize(dword_1DFFD98)` (`0x3063D0`), `ClientApplicationRunLoop`
  (`0x3098C0`), `ClientApplicationShutdown` (`0x3095D0`); `dword_1DFFD98` is the App object.
- **Observed (vtable).** Vtable raw `0x187682C`, COL `0x1CD439C`, 26 slots (raw targets):
  0 `E54BF0` (1412 B), 1 `E553F0`, 2 `E55580`, 3 `E4E1E0`, 4 `E4E1F0`, 5 `E4E200`,
  6 `E55370`, 7 `E55380` (104), 8 `E55990` (746), 9 `E55E80` (601), 10 `E4E470` (34),
  11 `E4E4A0` (15), 12 `E55E60` (25), 13 `E6EF70`, 14 `E6F0C0` (158), 15 `E4FB80`,
  16 `E4FB60` (28), 17 `E4FB50`, 18 `E4FB40`, 19 `E4FB30` (16), 20 `E53A40` (120),
  21 `E53FA0` (1717), 22 `E4FB20`, 23 `E54660` (52), 24 `E4FB10`, 25 `E4FB00` (16).
  Slots without a size are not IDA-defined function starts. No slot name is proven by a string
  beyond slot 0's log text.
- **Interpreted.** Engine-system objects are created lazily by the bootstrap factory walk, not by
  a direct call; AppMode objects (Login/World) are not on this path - they come from
  `dword_1DFFD98` (App) during `ClientApplicationInitialize`/run loop.
- **Unresolved.** The caller that invokes slot 0 (`E54BF0`) and the others (no direct call or
  vtable-literal reference found; likely through `factory+0x10` / `0x1E4B0A8` list consumers
  such as `0x110D060`, `0x2F1540`, `0x311090`; not traced); role names of slots 1-25.

### Factory list consumers and engine-system init (static pass 2)

- **Observed.** `0x12C3A80` (create walk) iterates `dword_1E4B0A4` via `+8` and calls factory vtable
  `+4` (create) on entries whose `+4` byte is 0; `byte_1E4B095` = "walking", `byte_1E4B096` =
  "walked". `0x12C3AF0` (teardown walk, raw bytes `85C9740F8B01FF5008...` at `+0x34`) loops on
  `dword_1E4B0A8` and calls factory vtable `+8` (destroy) until the head is empty. Create slots
  (e.g. `0xE4E660`, `0x2EC510`) link the factory into the instance list (`+0xC` = old `A8` head,
  `A8` = factory, `+4 = 1`, object at `+0x10`); destroy slots (e.g. `0x2F1540`) pop the `A8` head.
  `dword_1E4B0A4` has exactly one code reference (the create walker); `dword_1E4B0A8` is referenced
  by 80 functions, each with exactly two references (create/destroy slots) plus the teardown walker.
  No code reads factory `+0x10` by a static address: raw scans find the immediates `0x1E00460`/
  `0x1E00470` only in the static init (`0xE4F630`) and accessor `0xE4E630` (no callers, no jump).
- **Observed (class list).** 43 create slots with a resolvable constructor store these vtables
  (RTTI): `slUsesSingleton<nl::Network>`, `slUsesSingleton<slSafeQueue::NodePool>`, `nl::SocketPumpI`,
  `smSoundThread`, `ilHiddenInputWindowThread`, `fsAssetDB`, `tsLodManager`, `tfGUIInterface`,
  a `tfAudioManager` bound functor, `tfDebugEvent`, `tfDebugLag`, `oParticleInterface`,
  `EnglishRules`, `tfGlobalizationManager`, `smSoundActionManager`, `vcs::StaticDBDataStore`,
  `tfToggleFireModeCommand`, `uiManager`, `oEngineSystem` (rest have no ctor-stored RTTI vtable).
- **Interpreted.** The list is a generic static singleton/service registry (network, sound, asset DB,
  GUI, UI, debug, rules, engine), not an engine-system-only list. Instance order is creation order in
  `A4` order (A4 head = last registered static init); teardown is the reverse (LIFO via `A8`). The
  registration order across translation units (CRT initialiser table) was not recovered.
- **Observed (negative).** No loop over either list calls a slot on `factory+0x10`; the only list
  consumers are the create and teardown walkers. Hence `oEngineSystem` slot 0 (`0xE54BF0`) is NOT
  invoked by a factory-list init loop.
- **Unresolved.** The real caller of `0xE54BF0` (likely a virtual call through an interface pointer
  obtained elsewhere, e.g. a ctor-time global store or `oEngineInterface`); `Init` naming remains
  inferred from its log strings; init order of the other classes; classes without RTTI-resolved ctors.

### Engine-system slot callers and crash-hook pass (static, FirefallClient.exe.original)

Observed:
- `dword_1DFC82C` (raw 0x19FAC2C, bytes `70 04 E0 01`) holds 0x1E00470 = `factory+0x10`, the oEngineSystem instance slot. Accessor 0xE4E630 is `mov eax,1E00470; ret`. 0x2D2D60/0x2D2D70 copy it into aliases `1E60E3C` and `1E60EA4` (478 raw immediate hits).
- Consumers load `eax=[cell]; ecx=[eax]; eax=[ecx]; call [eax+disp]`. Slot calls found through `1DFC82C`: +0x44 (0xF5FF6B), +0x54 (0x46CC7A, 0x470F1B), +0x58 (0x46CA6B), +0x5C (0x3D32B6), +0x60 (0xE651D6, 0xE65233, 0xE65339, 0xE66D5C), +0x64 (0x3072B2 with arg 1 after init, 0x30979A with arg 0 in shutdown). Through `1E60EA4` a chain-tracking scan also found +4 (0x30C168), +0x1C, +0x24, +0x2C, +0x30, +0x38, +0x3C, +0x40, +0x44, +0x48, +0x4C, +0x54, +0x58.
- Direct fields through the same object: +0x160 (byte), +0x164, +0x168, +0x24 (float), +0x78/+0x80 sub-objects; these are fields, not slots.
- Slot 0 (0xE54BF0) appears only as the vtable literal at raw 0x187682C. No call with disp 0 through either cell was found by the chain scan (cells `1DFC82C` and `1E60EA4` only); the F54D82-area disp-0 hits are loads, not slot calls.

Interpreted: slot +0x64 is a boolean-style toggle (1 at init, 0 at shutdown). +0x60 returns an object pointer that callers null-test. Roles of other slots are unproven.

Unresolved: the caller of slot 0 (indirect calls through other copies of the instance pointer, or through registers not traced, were not searched); the writer that fills `1DFC82C`.

### oEngineSystem vtable slot table (static, FirefallClient.exe.original)

Slot index = disp/4; raw targets as in the vtable at raw `0x187682C`. Consumer pattern `eax=[1DFC82C]; ecx=[eax]; eax=[ecx]; call [eax+disp]` passes the object as `this`.

| Slot | Disp | Raw target | Role | Evidence |
|---|---|---|---|---|
| 17 | +0x44 | E4FB50 | `lea eax,[this+0xB8]; ret` - returns address of an embedded block (caller 0xF5FF6B reads `+0x1C` of it as a text/path part) | Observed bytes; block meaning unresolved |
| 21 | +0x54 | E53FA0 (1717 B) | ApplyDisplayMode(mode, width, height, refresh): writes `this[76]` (+0x130) = mode, caches size in `this[78..82]`, drives window-style/resize through the render-system (`dword_1DFE970`) | Observed decompile; name Interpreted. Caller 0x46CC7A pushes 4 args (0,edx,ebx,0) |
| 22 | +0x58 | E4FB20 | GetDisplayMode: `mov eax,[this+0x130]` (the field slot 21 writes) | Observed; caller 0x46CA6B stores the result to `[esi+0x7C]` |
| 23 | +0x5C | E54660 | Out-param getter of three dwords `+0x14C/+0x150/+0x154` (each pointer null-checked, `ret 0xC`) | Observed; semantics of the triple unresolved; caller 0x3D32B6 |
| 24 | +0x60 | E4FB10 | GetHandle: `mov eax,[this+0xB4]` (callers null-test; 0xE651D6 feeds it to an import thunk) | Observed bytes; "window handle" is Interpreted only |
| 25 | +0x64 | E4FB00 | SetHandle: `[this+0xB4]=arg; ret 4` | Observed. **Correction:** init call 0x3072AE pushes global `dword_1E00128` (not literal 1); shutdown call 0x30979A pushes 0 |

Other slots, bytes only (Observed, no consumer found, role unproven): 3 `E4E1E0` = `mov al,[this+0xBC]`; 4 `E4E1F0` = `mov al,[this+0xBD]`; 5 `E4E200` = `mov al,[this+0xDB]`; 6 `E55370` = returns constant 0x1E81D00; 12 `E55E60` = calls `[this+0x10C]` callback if set; 15 `E4FB80` = `mov eax,[this+0xAC]`; 16 `E4FB60` = bounds-checked (`idx<2`) getter of `[this+0x158+idx*4]`; 18 `E4FB40` getter `[this+0xB0]`; 19 `E4FB30` setter `[this+0xB0]`; 0,1,2,7-11,13,14,20 not characterised here (slot 0 = Init by log strings, earlier pass).

Slot 0 callers (scoped negative): I scanned `ClientApplicationBootstrap` 0x2EAE70, `Initialize` 0x3063D0, `RunLoop` 0x3098C0, `Shutdown` 0x3095D0 and wrapper 0x2E9630 for `call [reg]` (disp 0) and references to cells `1DFC82C`/`1E60EA4`/`1E00470`. No slot-0 call through the engine-system cell exists in them. The only disp-0 call is 0x30976B (deleting-dtor style, `push 1; call [eax]`) on the App member `[esi+0xB98]`, not the engine object. `Initialize` has a direct field write `[[1DFC82C]]+0x164` at 0x3071B9 (field, not slot). Callers elsewhere (other copies of the pointer, other functions) were not searched: slot 0 caller remains Unresolved.

Checks: `StartupBinaryEvidence` verifies slots 17/21-25 vtable entries, accessor bytes, and the +0x64 init/shutdown call-site bytes. Constants are in `SceneFactoryNativeLayout` (`EngineSystemSlot*`).
