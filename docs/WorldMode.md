# `tfWorld` app mode (partial)

`Client_ApplyPendingAppModes` maps mode 2 to a `0x2904`-byte object constructed
by raw `0x64AE40`. RTTI attached to the installed vtable (VA `0x1C677BC`, raw
`0x18663BC`) identifies the class as `tfWorld` (`.?AVtfWorld@@`). Its class
hierarchy has `tfAppMode` at offset 0 and
`sl::callbackable<tfWorld, ilActionInfo const&>` at offset 4. The callbackable
subobject has no separate vfptr according to this RTTI.

## Constructor layout

All function addresses below are raw offsets in the IDA raw-image database;
absolute pointers are PE VAs. See `BinaryLayout.md` for translation.

- The constructor installs the `tfAppMode` vfptr, initializes the callbackable
  subobject at `+4` with the owner pointer, callback at raw `0x654210`, and a
  zero state word, then installs the `tfWorld` vfptr.
- The owner/context pointer is stored at `+0x10`; `+0x14` is a lazy scene
  pointer. Constructor assertions at lines 358–359 check that the owner is
  non-null and its byte at `+0x918` is nonzero. The owner's broader type and
  flag semantics are not established.
- Four layer objects of size `0x950` are constructed by raw `0xBF3FB0` at
  offsets `+0x2FC`, `+0xC4C`, `+0x159C`, and `+0x1EEC`, with observed IDs
  `0`, `9`, `8`, and `9`. Their concrete class and detailed contents remain
  unresolved. The address and IDs are recorded in
  `TfWorldNativeLayout.LayersByOffset`.
- Additional members include a small-vector-like structure at `+0x2850` and
  heap objects at `+0x28E8` (size `0x208`), `+0x28EC` (size `0x50`), and
  `+0x28F0` (size `0x18`). RTTI now identifies `+0x28E8` as **tfChat**;
  the other two exact roles remain unresolved.
- The constructor initializes the member at `+0x2E8` to null and allocates a
  `0x2C`-byte block for `+0x2EC`. The block is initialized with DWORD `20` at
  offset `+0`, DWORD `330` at `+0x24`, value `1` at `+0x28`, and a pointer-like
  value at `+0x20`; its type and field meanings are unresolved.

## Virtual behavior

The `tfAppMode` base has 19 virtual slots; most base implementations dispatch
to `_purecall`, so it is abstract. `tfWorld` overrides all 19 slots. The
following concrete behavior is supported by the traced bodies:

- Slot 1 (raw `0x64B460`) loads `Broadcast.xml` and `MainUI.xml`, then creates
  the global scene through raw `0x9B73F0` using scene names `RenderScene`,
  `WorldMapScene`, and `SinvironmentScene`. On failure it logs
  `Unable to create global scene`. On success it pushes a layer at `+0x2FC`,
  invokes slot 15 with mode 3, copies owner `+0x848/+0x84C` into tfChat's
  local pair with type byte1 and owner `+0x850` CString into chat text,
  and registers with a chat-manager-like global. See
  `MatrixConnection.md` for the bounded identity/file-log projections.
- Slot 2 (`0x64C1B0`) is a per-frame/time hook calling `0x650AB0` and an owner
  method at `0x492610`; it reads owner state near `+0x918` and scene state at
  `+0x74`.
- Slot 3 (`0x64C290`) updates state associated with strings `start`,
  `Streaming complete`, `isAfk`, `calm`, and `Logging out from AFK`.
- Slots 4 and 12 forward to the scene through slot 9. Slot 9 returns the scene
  at `+0x14` after an assertion. Slots 8 and 10 test whether that scene exists.
- Slot 15 (`0x64C020`) handles integer values 0–5. Its cases push/pop layer
  objects with IDs 8 or 9 through a global stack at VA `0x1DFFEF4` and update a
  global state byte. Exact user-facing names for these modes are not known.
- Slot 16 (`0x64C170`) operates on the layer at `+0x159C`. Slot 14 (raw
  `0x651A60`) checks a global enable byte, object byte `+4`, and pointer
  `+0x10`; it asserts a global pointer matches `this`, transfers the DWORD at
  `+0x0C` into that global, then clears `+0x0C/+0x10`. If the pointer was
  non-null, it clears its ordered container through `0x6536B0` and releases
  the container storage. The callback/state object semantics remain unknown.
  Slot 17 (`TfWorld_GetMember_2E8`, raw
  `0x64A0C0`) and slot 18 (`TfWorld_GetMember_2EC`, raw `0x64A0D0`) return the
  DWORDs at `+0x2E8` and `+0x2EC`, respectively; their semantic member types
  are unresolved. Slot 7 is empty.
- Slot 5 (raw `0x64B950`) is the shutdown/deactivation method invoked through
  the app-mode shutdown slot. It pops global layer IDs 0, 9, and 8; releases
  owned scene/UI/callback objects; clears the active layer vector; and resets
  world state.
- Slot 6 (raw `0x64BC40`) tests observer/remote-view selection state and, on
  matching branches, constructs/submits internal notifications 118 and 117.
  It also updates scene/render state. String references include `spectating`
  and `entityId`, but the exact message semantics are unknown.
- Slot 11 (`TfWorld_ApplyRecordInput`, raw `0x64DCF0`) returns false unless the
  pointer returned from member `+0x2E8` is non-null and its `+0x28` field is 3.
  It then copies
  the full 32-byte input record
  to the mode at `+0x20`, as well as to nested scene/render blocks. Input
  floats at `+8` and `+12` are used as numerator and denominator; the
  denominator is clamped to a stored lower bound, and the quotient is written
  to a nested render object's field at `+0x564`. Its byte flag participates in
  conditional `0x10000` state-bit updates. The record's semantic type and
  meanings of its other bytes remain unknown.

The destructor at slot 0 references `Forgot to call Cancel?` and restores the
base vfptr before deletion. These observations support identifying mode 2 as
the in-game world mode, but do not establish every lifecycle or input-layer
semantic.

## Reconstruction boundary

`src\Startup\WorldModeNativeLayout.cs` preserves the verified object size,
owner/scene offsets, layer offsets/IDs, selected method addresses, and global
stack VA, plus the record-input size, state gate, copied-to mode offset, and
observed float offsets. It intentionally does not recreate native virtual
dispatch, allocate or initialize game objects, or assign speculative names to
the layer types, callback state, or opaque record.
