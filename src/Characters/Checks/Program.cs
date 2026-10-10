// Self-contained wire-format checks for the reconstructed Character commands.
// Expected bytes are derived from the traced native encoders (see ..\README.md).
// Exit code 0 = all checks passed, 1 = at least one failure.
using Firefall.Characters;

int failures = 0;

void Check(string name, bool condition, string? detail = null)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}{(condition || detail is null ? "" : "  -> " + detail)}");
    if (!condition) failures++;
}

void CheckHex(string name, byte[] actual, string expected)
{
    string hex = Convert.ToHexString(actual);
    Check(name, hex == expected, $"got {hex}, expected {expected}");
}

const ulong Target = 0x0000000100000004;
const string TargetHex = "0400000001000000";

// 0xAE LogDirectActivityRequest (encoder 0x90FD40): target + id, no payload.
CheckHex("0xAE LogDirectActivityRequest", CharacterCommandWire.Encode(new LogDirectActivityRequest(Target)), TargetHex + "AE");

// 0xAF LogActivityPush (encoder 0x90FE20): target + id + u32 LE.
CheckHex("0xAF LogActivityPush", CharacterCommandWire.Encode(new LogActivityPush(Target, 0x11223344)), TargetHex + "AF" + "44332211");

// 0xC2 UpdateShoppingList (encoder 0x910040): setA unique/sorted by Second; setB unique/sorted by (Value, Tag).
var shopping = new UpdateShoppingList(
    Target,
    [new(1, 9), new(2, 3), new(7, 9)],          // (7,9) dropped: duplicate key Second=9, first insert wins
    [new(5, 2), new(5, 1), new(5, 1)]);         // duplicate (5,1) dropped
CheckHex("0xC2 UpdateShoppingList ordering/dedupe", CharacterCommandWire.Encode(shopping),
    TargetHex + "C2" + "02" + "0200000003000000" + "0100000009000000" + "02" + "0500000001" + "0500000002");
CheckHex("0xC2 UpdateShoppingList empty sets", CharacterCommandWire.Encode(new UpdateShoppingList(Target, [], [])), TargetHex + "C2" + "00" + "00");

// Chunk boundaries (0x5BA110 / 0x5BA240): u8 n=min(rem,255) then n items; repeat while n==255.
static int[] ChunkHeaders(byte[] body, int offset, int itemSize, out int end)
{
    var headers = new List<int>();
    int n;
    do
    {
        n = body[offset++];
        headers.Add(n);
        offset += n * itemSize;
    }
    while (n == 0xFF);
    end = offset;
    return headers.ToArray();
}

foreach (var (count, expected) in new (int, int[])[]
{
    (0, [0]), (1, [1]), (254, [254]), (255, [255, 0]), (256, [255, 1]), (510, [255, 255, 0]), (511, [255, 255, 1]),
})
{
    var a = Enumerable.Range(1, count).Select(i => new ShoppingEntryA(0, (uint)i));
    var b = Enumerable.Range(1, count).Select(i => new ShoppingEntryB((uint)i, 0));
    byte[] body = CharacterCommandWire.Encode(new UpdateShoppingList(Target, a, b));
    int[] headersA = ChunkHeaders(body, 9, 8, out int endA);
    int[] headersB = ChunkHeaders(body, endA, 5, out int endB);
    Check($"chunks setA n={count} [{string.Join(",", expected)}]", headersA.SequenceEqual(expected), string.Join(",", headersA));
    Check($"chunks setB n={count} [{string.Join(",", expected)}]", headersB.SequenceEqual(expected), string.Join(",", headersB));
    Check($"chunks length n={count}", endB == body.Length && body.Length == 9 + expected.Length * 2 + count * (8 + 5));
}

// MessageID packing (0x320900).
byte[] Id(uint id)
{
    using var ms = new MemoryStream();
    using var w = new BinaryWriter(ms);
    CharacterCommandWire.WriteMessageId(w, id);
    w.Flush();
    return ms.ToArray();
}
CheckHex("MessageID 0xFF (1 byte)", Id(0xFF), "FF");
CheckHex("MessageID 0x100 (escape + u16)", Id(0x100), "000001");
CheckHex("MessageID 0x1234 (escape + u16)", Id(0x1234), "003412");
Check("MessageID 0 rejected", Throws(() => Id(0)));
Check("MessageID 0x10000 rejected", Throws(() => Id(0x10000)));

// Flush throttle (0x49B3D0).
var throttle = new ShoppingListFlushThrottle(100) { Dirty = true };
Check("throttle fires, phase-aligned", throttle.Poll(250) && throttle.LastTick == 200 && !throttle.Dirty);
throttle.Dirty = true;
Check("throttle holds before interval", !throttle.Poll(260));

Check("remote-character dispatch gates and sender offsets are cataloged",
    CharacterObserverDispatchLayout.DispatchesByMessageId.SequenceEqual(
    [
        new CharacterObserverDispatchSnapshot(0xAE, "LogDirectActivityRequest", 0x90CBC0, 0x90EE20, false),
        new CharacterObserverDispatchSnapshot(0xAF, "LogActivityPush", 0x90CC30, 0x90EF50, false),
        new CharacterObserverDispatchSnapshot(0xC2, "UpdateShoppingList", 0x90CF60, 0x90F260, true)
    ])
    && CharacterObserverDispatchLayout.ShoppingListBuilderMethodFileOffset == 0x9095B0
    && CharacterObserverDispatchLayout.ShoppingListBuilderCallers.SequenceEqual(
    [
        new CharacterObserverBuilderCallerSnapshot(0x49B3D0, 0x49B40A),
        new CharacterObserverBuilderCallerSnapshot(0x4A7980, 0x4A7EE3)
    ]));
Check("remote-character observer gate offsets and descriptor fields are recorded",
    CharacterObserverDispatchLayout.ObserverAvailabilityFieldOffset == 0x820
    && CharacterObserverDispatchLayout.ObserverSignedThresholdFieldOffset == 0x0C
    && CharacterObserverDispatchLayout.AcceptedObserverThresholdExclusive == 3
    && CharacterObserverDispatchLayout.ObserverFlagsByteOffset == 0x14
    && CharacterObserverDispatchLayout.OptionalLogFlagMask == 0x08
    && CharacterObserverDispatchLayout.ShoppingListMessageIdFieldOffset == 0x10
    && CharacterObserverDispatchLayout.EncodedDescriptorFirstWordOffset == 0x18
    && CharacterObserverDispatchLayout.EncodedDescriptorSecondWordOffset == 0x1C);
Check("shopping-list source map and flush state fields are recorded",
    CharacterObserverDispatchLayout.ShoppingListStateOwnerOffset == 0x43C8
    && CharacterObserverDispatchLayout.ShoppingListPersistentOrderedMapOffset == 0
    && CharacterObserverDispatchLayout.ShoppingListTextKeyedMapOffset == 0x2C
    && CharacterObserverDispatchLayout.ShoppingListViewTreeOffset == 0x4C
    && CharacterObserverDispatchLayout.ShoppingListAdditionalCollectionOffset == 0x24
    && CharacterObserverDispatchLayout.ShoppingListStateDestructorMethodFileOffset == 0x908D30
    && CharacterObserverDispatchLayout.ShoppingListResetOwnerMethodFileOffset == 0x4A0700
    && CharacterObserverDispatchLayout.ShoppingListComputedViewReaderMethodFileOffset == 0xA242A0
    && CharacterObserverDispatchLayout.ShoppingListScriptViewReaderMethodFileOffset == 0x9F65E0
    && CharacterObserverDispatchLayout.ShoppingListNamedValueUpdateMethodFileOffset == 0xACDD40
    && CharacterObserverDispatchLayout.ShoppingListFormatNamedValueMethodFileOffset == 0x909B40
    && CharacterObserverDispatchLayout.ShoppingListNamedValueEventId == 230
    && CharacterObserverDispatchLayout.ShoppingListSourceMethodFileOffset == 0x909120
    && CharacterObserverDispatchLayout.ShoppingListAddEntryMethodFileOffset == 0x9092A0
    && CharacterObserverDispatchLayout.ShoppingListResolveComponentsMethodFileOffset == 0x9D4C00
    && CharacterObserverDispatchLayout.ShoppingListStateFieldsByOffset.SequenceEqual(
    [
        new ShoppingListSourceFieldSnapshot(0x00, "Outer ordered map head pointer"),
        new ShoppingListSourceFieldSnapshot(0x04, "Outer map size"),
        new ShoppingListSourceFieldSnapshot(0x08, "Dirty byte"),
        new ShoppingListSourceFieldSnapshot(0x0C, "Flush interval"),
        new ShoppingListSourceFieldSnapshot(0x10, "Last-send tick")
    ])
    && CharacterObserverDispatchLayout.ShoppingListOuterMapEntryFirstWordOffset == 0x10
    && CharacterObserverDispatchLayout.ShoppingListOuterMapEntrySecondWordOffset == 0x14
    && CharacterObserverDispatchLayout.ShoppingListOuterMapKeyOffset == 0x14
    && CharacterObserverDispatchLayout.ShoppingListOuterMapInnerSetHeadOffset == 0x18
    && CharacterObserverDispatchLayout.ShoppingListOuterMapInnerSetSizeOffset == 0x1C
    && CharacterObserverDispatchLayout.ShoppingListInnerElementFirstWordOffset == 0x10
    && CharacterObserverDispatchLayout.ShoppingListInnerElementByteOffset == 0x14);
Check("shopping-list context key shape and ready event are recorded",
    CharacterObserverDispatchLayout.ShoppingListContextKeyLowWordOffset == 0x18
    && CharacterObserverDispatchLayout.ShoppingListContextKeyHighWordOffset == 0x1C
    && CharacterObserverDispatchLayout.ShoppingListContextKeyForcedLowByte == 4
    && CharacterObserverDispatchLayout.ShoppingListReadyEventId == 219);

// The equipment models mirror constructor layouts, not unverified wire formats.
var moduleEntries = new[] { new ModuleSlotEntry(11, 12), new ModuleSlotEntry(21, 22) };
var moduleRequest = new SlotModuleRequestPayload(Target, 0x123456789ABCDEF0, moduleEntries);
moduleEntries[0] = default;
Check("message 204 fields and copied pair vector",
    SlotModuleRequestPayload.MessageId == 204 &&
    SlotModuleRequestPayload.EntriesOffset == 0x30 &&
    moduleRequest.Target == Target &&
    moduleRequest.Identifier == 0x123456789ABCDEF0 &&
    moduleRequest.Entries.SequenceEqual([new(11, 12), new(21, 22)]));

var unslot = new UnslotAllModulesRequestPayload(Target, 0xFEDCBA9876543210);
Check("message 205 identifier layout",
    UnslotAllModulesRequestPayload.MessageId == 205 &&
    UnslotAllModulesRequestPayload.IdentifierOffset == 0x28 &&
    unslot.Target == Target && unslot.Identifier == 0xFEDCBA9876543210);

var visualValues = new uint[] { 0x11223344, 0x55667788 };
var visual = new SlotVisualRequestPayload(Target, 1, 2, 3, 4, 5, 6, visualValues);
visualValues[0] = 0;
Check("message 207 fields and copied u32 vector",
    SlotVisualRequestPayload.MessageId == 207 &&
    SlotVisualRequestPayload.ValuesOffset == 0x3C &&
    visual.Target == Target && visual.First == 1 && visual.Second == 2 &&
    visual.FirstFlag == 3 && visual.SecondFlag == 4 &&
    visual.Third == 5 && visual.Fourth == 6 &&
    visual.Values.SequenceEqual([0x11223344u, 0x55667788u]));

byte[] MultiPayload(SlotVisualMultiRequest request)
{
    using var output = new MemoryStream();
    using var writer = new BinaryWriter(output);
    request.WritePayload(writer);
    return output.ToArray();
}

CheckHex("message 208 empty payload", MultiPayload(new SlotVisualMultiRequest(Target, 1, 0, [], [])),
    "01000000" + "00000000" + "00" + "00");

var nestedValues = new uint[] { 0x55667788 };
var firstText = new byte[] { (byte)'A' };
var descriptor = new VisualDescriptor(0x11223344, 0x7F, 0xAABBCCDD, 0x01020304, nestedValues);
var named = new NamedVisualDescriptor(1, firstText, 2, 3, new byte[] { (byte)'B', (byte)'C' }, 4, 5);
var multi = new SlotVisualMultiRequest(Target, 9, 1, [descriptor], [named]);
nestedValues[0] = 0;
firstText[0] = 0;
Check("message 208 native layout", SlotVisualMultiRequest.MessageId == 208 &&
    SlotVisualMultiRequest.NativeSize == 0x218 &&
    SlotVisualMultiRequest.InlineVisualCapacity == 16 &&
    SlotVisualMultiRequest.NamedVisualsOffset == 0x20C);
CheckHex("message 208 nested descriptor and text payload", MultiPayload(multi),
    "09000000" + "01000000" +
    "01" + "44332211" + "7F" + "DDCCBBAA" + "04030201" + "01" + "88776655" +
    "01" + "01000000" + "4100" + "02000000" + "03000000" + "424300" + "04000000" + "05000000");
Check("message 208 rejects embedded text NUL",
    ThrowsArgument(() => new NamedVisualDescriptor(0, new byte[] { 0 }, 0, 0, ReadOnlyMemory<byte>.Empty, 0, 0)));
var manyVisuals = Enumerable.Range(0, 255).Select(i => new VisualDescriptor((uint)i, 0, 0, 0, []));
var chunkedVisuals = MultiPayload(new SlotVisualMultiRequest(Target, 0, 0, manyVisuals, []));
Check("message 208 visual chunk boundary", chunkedVisuals[8] == 255 &&
    chunkedVisuals[8 + 1 + 255 * 14] == 0 &&
    chunkedVisuals[^1] == 0 &&
    chunkedVisuals.Length == 8 + 1 + 255 * 14 + 1 + 1);

byte[] RepairPayload(RepairRequest request)
{
    using var output = new MemoryStream();
    using var writer = new BinaryWriter(output);
    request.WritePayload(writer);
    return output.ToArray();
}

Check("message 203 native ID and vector offset",
    RepairRequest.MessageId == 203 && RepairRequest.IdentifiersOffset == 0x28);
CheckHex("message 203 writes little-endian u64 identifiers",
    RepairPayload(new RepairRequest(Target, [0x0102030405060708, 0xFFEEDDCCBBAA9988])),
    "02" + "0807060504030201" + "8899AABBCCDDEEFF");
Check("message 203 sender rejects empty and over-100 requests",
    Throws(() => new RepairRequest(Target, [])) &&
    Throws(() => new RepairRequest(Target, Enumerable.Range(0, 101).Select(i => (ulong)i))));
Check("message 203 accepts exactly 100 entries",
    RepairPayload(new RepairRequest(Target, Enumerable.Range(0, 100).Select(i => (ulong)i))).Length == 1 + 100 * 8);

Check("message 202 native ID and vector offset",
    SalvageRequest.MessageId == 202 && SalvageRequest.RecordsOffset == 0x28);
var salvageInput = new[] { new SalvageRecord(0x0102030405060708, 0x11223344, 0xAABBCCDD) };
var salvage = new SalvageRequest(Target, salvageInput);
salvageInput[0] = default;
CheckHex("message 202 target, ID, and sixteen-byte record",
    salvage.EncodeBody(),
    TargetHex + "CA" + "01" + "0807060504030201" + "44332211" + "DDCCBBAA");
Check("message 202 sender enforces 1..200 records",
    Throws(() => new SalvageRequest(Target, [])) &&
    Throws(() => new SalvageRequest(Target, Enumerable.Repeat(new SalvageRecord(0, 0, 0), 201))));
Check("message 202 accepts 200 records",
    new SalvageRequest(Target, Enumerable.Repeat(new SalvageRecord(0, 0, 0), 200))
        .EncodeBody().Length == 9 + 1 + 200 * 16);

var teleport = new RequestTeleport(Target, 0x0102030405060708);
Check("message 210 constructor field layout",
    RequestTeleport.MessageId == 210 && RequestTeleport.ValueOffset == 0x28 &&
    teleport.Target == Target && teleport.Value == 0x0102030405060708);
var respawn = new RequestRespawn(Target);
Check("RequestRespawn message 198 kind-only constructor",
    RequestRespawn.MessageId == 198 && respawn.Target == Target);
CheckHex("RequestRespawn message body", respawn.EncodeBody(), TargetHex + "C6");

var duelPairs = new[] { new DuelRequestPair(1, 2) };
var duelDescriptor = new DuelRequestDescriptor(
    3, new byte[] { 0x41 }, 4, 5, new byte[] { 0x42 }, 6, 7,
    new byte[] { 0x43 }, duelPairs, [], 8, 9, 10, 11, 12, 13);
duelPairs[0] = default;
var duel = new DuelRequest(Target, duelDescriptor, 4);
Check("DuelRequest native descriptor layout",
    DuelRequest.MessageId == 188 && DuelRequest.DescriptorOffset == 0x28 &&
    DuelRequest.DescriptorSize == 0x58 && DuelRequest.ModeOffset == 0x80 &&
    duel.Target == Target && duel.Mode == 4 &&
    duel.Descriptor.FirstPairs.SequenceEqual([new DuelRequestPair(1, 2)]));

var transfer = new RequestTransfer(Target, 0x11223344, 1);
Check("RequestTransfer native field offsets",
    RequestTransfer.MessageId == 199 &&
    RequestTransfer.ValueOffset == 0x28 &&
    RequestTransfer.OptionOffset == 0x2C);
CheckHex("RequestTransfer message body",
    transfer.EncodeBody(), TargetHex + "C7" + "44332211" + "01");
var findNearby = new FindNearbyResourceAreas(Target);
Check("FindNearbyResourceAreas kind-only native layout",
    FindNearbyResourceAreas.MessageId == 183 && findNearby.Target == Target);

var vehicleBlock = Enumerable.Range(0, VehicleCalldownRequest.OpaqueBlockSize)
    .Select(i => (byte)i).ToArray();
var vehicleRequest = new VehicleCalldownRequest(
    Target, 0x1234, 1, 2, 3, vehicleBlock, 4, 5, 6, 7, 8, 9);
vehicleBlock[0] = 0xFF;
Check("VehicleCalldownRequest native layout and copied block",
    VehicleCalldownRequest.MessageId == 179 &&
    VehicleCalldownRequest.SelectorOffset == 0x28 &&
    VehicleCalldownRequest.ValuesOffset == 0x2C &&
    VehicleCalldownRequest.OpaqueBlockOffset == 0x38 &&
    VehicleCalldownRequest.TrailingValuesOffset == 0x78 &&
    VehicleCalldownRequest.FlagsOffset == 0x88 &&
    vehicleRequest.Target == Target && vehicleRequest.Selector == 0x1234 &&
    vehicleRequest.Value2C == 1 && vehicleRequest.Value30 == 2 && vehicleRequest.Value34 == 3 &&
    vehicleRequest.OpaqueBlock.Length == 0x40 && vehicleRequest.OpaqueBlock.Span[0] == 0 &&
    vehicleRequest.Value78 == 4 && vehicleRequest.Value7C == 5 &&
    vehicleRequest.Value80 == 6 && vehicleRequest.Value84 == 7 &&
    vehicleRequest.Flag88 == 8 && vehicleRequest.Flag89 == 9);
Check("VehicleCalldownRequest rejects incorrect opaque block size",
    ThrowsArgument(() => new VehicleCalldownRequest(Target, 0, 0, 0, 0, new byte[0x3F], 0, 0, 0, 0, 0, 0)));

var deployableBlock = Enumerable.Range(0, DeployableCalldownRequest.OpaqueBlockSize)
    .Select(i => (byte)(0xFF - i)).ToArray();
var deployableRequest = new DeployableCalldownRequest(
    Target, 10, 11, 12, 13, deployableBlock, 14, 15, 16, 17, 18, 19);
deployableBlock[^1] = 0;
Check("DeployableCalldownRequest native layout and copied block",
    DeployableCalldownRequest.MessageId == 180 &&
    DeployableCalldownRequest.ValueOffset == 0x28 &&
    DeployableCalldownRequest.ValuesOffset == 0x2C &&
    DeployableCalldownRequest.OpaqueBlockOffset == 0x38 &&
    DeployableCalldownRequest.TrailingValuesOffset == 0x78 &&
    DeployableCalldownRequest.FlagsOffset == 0x88 &&
    deployableRequest.Target == Target && deployableRequest.Value28 == 10 &&
    deployableRequest.Value2C == 11 && deployableRequest.Value30 == 12 &&
    deployableRequest.Value34 == 13 && deployableRequest.OpaqueBlock.Length == 0x40 &&
    deployableRequest.OpaqueBlock.Span[^1] == 0xC0 &&
    deployableRequest.Value78 == 14 && deployableRequest.Value7C == 15 &&
    deployableRequest.Value80 == 16 && deployableRequest.Value84 == 17 &&
    deployableRequest.Flag88 == 18 && deployableRequest.Flag89 == 19);
Check("DeployableCalldownRequest rejects incorrect opaque block size",
    ThrowsArgument(() => new DeployableCalldownRequest(Target, 0, 0, 0, 0, new byte[0x41], 0, 0, 0, 0, 0, 0)));

var clearPreviousBounties = new ClearPreviousBounties(Target, 0xA1B2C3D4);
Check("ClearPreviousBounties native field layout",
    ClearPreviousBounties.MessageId == 162 && ClearPreviousBounties.ValueOffset == 0x28 &&
    clearPreviousBounties.Target == Target && clearPreviousBounties.Value == 0xA1B2C3D4);

var requestAchievementStatus = new RequestAchievementStatus(Target, 0x10203040);
Check("RequestAchievementStatus native field layout",
    RequestAchievementStatus.MessageId == 169 && RequestAchievementStatus.ValueOffset == 0x28 &&
    requestAchievementStatus.Target == Target && requestAchievementStatus.Value == 0x10203040);

var rerollValues = new uint[] { 0x11223344, 0x55667788 };
var rerollRequest = new RequestRerollBounties(Target, rerollValues);
rerollValues[0] = 0;
Check("RequestRerollBounties native vector layout and copy",
    RequestRerollBounties.MessageId == 164 &&
    RequestRerollBounties.ValuesOffset == 0x28 &&
    RequestRerollBounties.NativeVectorHeaderSize == 0x0C &&
    rerollRequest.Target == Target &&
    rerollRequest.Values.SequenceEqual([0x11223344u, 0x55667788u]));
Check("RequestRerollBounties rejects null values",
    ThrowsArgument(() => new RequestRerollBounties(Target, null!)));

var movementSimulation = new SetMovementSimulation(Target, 0xABCD);
Check("SetMovementSimulation native field layout",
    SetMovementSimulation.MessageId == 197 && SetMovementSimulation.ValueOffset == 0x28 &&
    movementSimulation.Target == Target && movementSimulation.Value == 0xABCD);

var movementDescriptor = Enumerable.Range(0, MovementInput.DescriptorPrefixSize)
    .Select(i => (byte)i).ToArray();
var movement = new MovementInput(Target, 1, 2, 3, movementDescriptor, 4, 5, 6);
movementDescriptor[0] = 0xFF;
Check("MovementInput native layout and copied descriptor prefix",
    MovementInput.MessageId == 115 &&
    MovementInput.Value28Offset == 0x28 &&
    MovementInput.Value2AOffset == 0x2A &&
    MovementInput.Value2EOffset == 0x2E &&
    MovementInput.DescriptorOffset == 0x34 &&
    MovementInput.NativeConstantOffset == 0x94 &&
    MovementInput.NativeConstantValue == 0x0101 &&
    MovementInput.SharedReferenceOffset == 0xBC &&
    MovementInput.ValueC0Offset == 0xC0 &&
    MovementInput.ValueC4Offset == 0xC4 &&
    MovementInput.FlagC6Offset == 0xC6 &&
    movement.Target == Target && movement.Value28 == 1 &&
    movement.Value2A == 2 && movement.Value2E == 3 &&
    movement.DescriptorPrefix.Length == 0x88 && movement.DescriptorPrefix.Span[0] == 0 &&
    movement.ValueC0 == 4 && movement.ValueC4 == 5 && movement.FlagC6 == 6);
Check("MovementInput rejects incorrect descriptor size",
    ThrowsArgument(() => new MovementInput(Target, 0, 0, 0, new byte[0x87], 0, 0, 0)));

var movementFakePrefix = Enumerable.Range(0, MovementInputFake.OptionalDescriptorSize)
    .Select(i => (byte)(0x80 + i)).ToArray();
var movementFake = new MovementInputFake(Target, 1, 2, 3, 4, 5, 6,
    movementFakePrefix, 7, 8, 9);
movementFakePrefix[0] = 0;
Check("MovementInputFake optional descriptor layout and copy",
    MovementInputFake.MessageId == 116 &&
    MovementInputFake.Value28Offset == 0x28 &&
    MovementInputFake.OptionalFlagOffset == 0x3C &&
    MovementInputFake.OptionalDescriptorOffset == 0x40 &&
    MovementInputFake.SharedReferenceOffset == 0xC8 &&
    MovementInputFake.OptionalValueOffset == 0xCC &&
    MovementInputFake.ValueD0Offset == 0xD0 &&
    MovementInputFake.FlagD2Offset == 0xD2 &&
    movementFake.Target == Target && movementFake.Value28 == 1 &&
    movementFake.Value2A == 2 && movementFake.Value2E == 3 &&
    movementFake.Value32 == 4 && movementFake.Value36 == 5 && movementFake.Value38 == 6 &&
    movementFake.HasOptionalDescriptor && movementFake.OptionalDescriptorPrefix.Length == 0x88 &&
    movementFake.OptionalDescriptorPrefix.Span[0] == 0x80 &&
    movementFake.OptionalValueCC == 7 && movementFake.ValueD0 == 8 && movementFake.FlagD2 == 9);
var movementFakeWithoutDescriptor = new MovementInputFake(Target, 1, 2, 3, 4, 5, 6,
    ReadOnlyMemory<byte>.Empty, null, 7, 8);
Check("MovementInputFake absent optional descriptor",
    !movementFakeWithoutDescriptor.HasOptionalDescriptor && movementFakeWithoutDescriptor.OptionalValueCC is null);

var projectileOptionalValues = new uint[] { 10, 11, 12 };
var projectile = new FireWeaponProjectile(Target, 1, 2, 3, 4, projectileOptionalValues);
projectileOptionalValues[0] = 0;
Check("FireWeaponProjectile native optional fields and copy",
    FireWeaponProjectile.MessageId == 121 &&
    FireWeaponProjectile.Value28Offset == 0x28 &&
    FireWeaponProjectile.ValuesOffset == 0x2C &&
    FireWeaponProjectile.OptionalFlagOffset == 0x38 &&
    FireWeaponProjectile.OptionalValuesOffset == 0x3C &&
    projectile.Target == Target && projectile.Value28 == 1 &&
    projectile.Value2C == 2 && projectile.Value30 == 3 && projectile.Value34 == 4 &&
    projectile.HasOptionalValues && projectile.OptionalValues.SequenceEqual([10u, 11u, 12u]));
Check("FireWeaponProjectile rejects incomplete optional fields",
    ThrowsArgument(() => new FireWeaponProjectile(Target, 0, 0, 0, 0, [1, 2])));

var selectWeapon = new SelectWeapon(Target, 0x11223344, 0x55, 0x66);
Check("SelectWeapon native field layout",
    SelectWeapon.MessageId == 125 &&
    SelectWeapon.Value28Offset == 0x28 &&
    SelectWeapon.Value2COffset == 0x2C &&
    SelectWeapon.Value2DOffset == 0x2D &&
    selectWeapon.Target == Target && selectWeapon.Value28 == 0x11223344 &&
    selectWeapon.Value2C == 0x55 && selectWeapon.Value2D == 0x66);

var reloadWeapon = new ReloadWeapon(Target, 0xAABBCCDD);
Check("ReloadWeapon native field layout",
    ReloadWeapon.MessageId == 126 && ReloadWeapon.ValueOffset == 0x28 &&
    reloadWeapon.Target == Target && reloadWeapon.Value == 0xAABBCCDD);

var acquireWeaponTarget = new AcquireWeaponTarget(Target, 1, 2, 3, 4, 5);
Check("AcquireWeaponTarget native field layout",
    AcquireWeaponTarget.MessageId == 128 &&
    AcquireWeaponTarget.Value28Offset == 0x28 &&
    AcquireWeaponTarget.Value30Offset == 0x30 &&
    AcquireWeaponTarget.Value34Offset == 0x34 &&
    AcquireWeaponTarget.Value38Offset == 0x38 &&
    AcquireWeaponTarget.Value3COffset == 0x3C &&
    acquireWeaponTarget.Target == Target && acquireWeaponTarget.Value28 == 1 &&
    acquireWeaponTarget.Value30 == 2 && acquireWeaponTarget.Value34 == 3 &&
    acquireWeaponTarget.Value38 == 4 && acquireWeaponTarget.Value3C == 5);

var loseWeaponTarget = new LoseWeaponTarget(Target, 6, 7, 8);
Check("LoseWeaponTarget native field layout",
    LoseWeaponTarget.MessageId == 129 &&
    LoseWeaponTarget.Value28Offset == 0x28 &&
    LoseWeaponTarget.Value30Offset == 0x30 &&
    LoseWeaponTarget.Value34Offset == 0x34 &&
    loseWeaponTarget.Target == Target && loseWeaponTarget.Value28 == 6 &&
    loseWeaponTarget.Value30 == 7 && loseWeaponTarget.Value34 == 8);

var selectFireMode = new SelectFireMode(Target, 0x11223344, 0x55);
Check("SelectFireMode native field layout",
    SelectFireMode.MessageId == 123 &&
    SelectFireMode.Value28Offset == 0x28 && SelectFireMode.Value2COffset == 0x2C &&
    selectFireMode.Target == Target && selectFireMode.Value28 == 0x11223344 &&
    selectFireMode.Value2C == 0x55);

var useScope = new UseScope(Target, 0xAABBCCDD, 0x66);
Check("UseScope native field layout",
    UseScope.MessageId == 124 &&
    UseScope.Value28Offset == 0x28 && UseScope.Value2COffset == 0x2C &&
    useScope.Target == Target && useScope.Value28 == 0xAABBCCDD &&
    useScope.Value2C == 0x66);

var cancelReload = new CancelReload(Target, 0x12345678);
Check("CancelReload native field layout",
    CancelReload.MessageId == 127 && CancelReload.ValueOffset == 0x28 &&
    cancelReload.Target == Target && cancelReload.Value == 0x12345678);

var projectileHit = new ReportProjectileHit(Target, 1, 2, 3, 4, 5, 6, 7);
Check("ReportProjectileHit unaligned native field layout",
    ReportProjectileHit.MessageId == 122 &&
    ReportProjectileHit.Value28Offset == 0x28 &&
    ReportProjectileHit.Value2AOffset == 0x2A &&
    ReportProjectileHit.Value2COffset == 0x2C &&
    ReportProjectileHit.Value2DOffset == 0x2D &&
    ReportProjectileHit.Value2FOffset == 0x2F &&
    ReportProjectileHit.Value30Offset == 0x30 &&
    ReportProjectileHit.Value32Offset == 0x32 &&
    projectileHit.Target == Target && projectileHit.Value28 == 1 &&
    projectileHit.Value2A == 2 && projectileHit.Value2C == 3 &&
    projectileHit.Value2D == 4 && projectileHit.Value2F == 5 &&
    projectileHit.Value30 == 6 && projectileHit.Value32 == 7);

var fireInputIgnored = new FireInputIgnored(Target, 0x11223344, 0x55);
Check("FireInputIgnored native field layout",
    FireInputIgnored.MessageId == 117 &&
    FireInputIgnored.Value28Offset == 0x28 && FireInputIgnored.Value2COffset == 0x2C &&
    fireInputIgnored.Target == Target && fireInputIgnored.Value28 == 0x11223344 &&
    fireInputIgnored.Value2C == 0x55);

var fireBurst = new FireBurst(Target, 0xAABBCCDD);
Check("FireBurst native field layout",
    FireBurst.MessageId == 118 && FireBurst.ValueOffset == 0x28 &&
    fireBurst.Target == Target && fireBurst.Value == 0xAABBCCDD);

var fireEnd = new FireEnd(Target, 0x12345678);
Check("FireEnd native field layout",
    FireEnd.MessageId == 119 && FireEnd.ValueOffset == 0x28 &&
    fireEnd.Target == Target && fireEnd.Value == 0x12345678);

var fireCancel = new FireCancel(Target, 0x87654321);
Check("FireCancel native field layout",
    FireCancel.MessageId == 120 && FireCancel.ValueOffset == 0x28 &&
    fireCancel.Target == Target && fireCancel.Value == 0x87654321);

var npcApplyEffect = new NpcApplyEffect(Target, 1, 2, 3);
Check("NPCApplyEffect native field layout",
    NpcApplyEffect.MessageId == 130 &&
    NpcApplyEffect.Value28Offset == 0x28 &&
    NpcApplyEffect.Value30Offset == 0x30 &&
    NpcApplyEffect.Value34Offset == 0x34 &&
    npcApplyEffect.Target == Target && npcApplyEffect.Value28 == 1 &&
    npcApplyEffect.Value30 == 2 && npcApplyEffect.Value34 == 3);

var npcRemoveEffect = new NpcRemoveEffect(Target, 4);
Check("NPCRemoveEffect native field layout",
    NpcRemoveEffect.MessageId == 131 && NpcRemoveEffect.ValueOffset == 0x28 &&
    npcRemoveEffect.Target == Target && npcRemoveEffect.Value == 4);

var changeLookAtValues = new uint[] { 10, 11, 12, 13, 14, 15 };
var changeLookAt = new ChangeLookAtTarget(Target, changeLookAtValues);
changeLookAtValues[0] = 0;
Check("ChangeLookAtTarget optional six-DWORD payload copy",
    ChangeLookAtTarget.MessageId == 133 &&
    ChangeLookAtTarget.OptionalFlagOffset == 0x28 &&
    ChangeLookAtTarget.OptionalValuesOffset == 0x30 &&
    changeLookAt.Target == Target && changeLookAt.HasOptionalValues &&
    changeLookAt.OptionalValues.SequenceEqual([10u, 11u, 12u, 13u, 14u, 15u]));
var changeLookAtWithoutValues = new ChangeLookAtTarget(Target, []);
Check("ChangeLookAtTarget absent optional payload",
    !changeLookAtWithoutValues.HasOptionalValues);
Check("ChangeLookAtTarget rejects incomplete optional payload",
    ThrowsArgument(() => new ChangeLookAtTarget(Target, [1, 2, 3])));

var targetAbility = new TargetAbility(Target, 0x7F);
Check("TargetAbility native field layout",
    TargetAbility.MessageId == 136 && TargetAbility.ValueOffset == 0x28 &&
    targetAbility.Target == Target && targetAbility.Value == 0x7F);

var dockToPlayer = new DockToPlayer(Target, 1, 2, 3, 4, 5);
Check("DockToPlayer native field layout",
    DockToPlayer.MessageId == 132 &&
    DockToPlayer.Value28Offset == 0x28 && DockToPlayer.Value2COffset == 0x2C &&
    DockToPlayer.Value30Offset == 0x30 && DockToPlayer.Value34Offset == 0x34 &&
    DockToPlayer.Value38Offset == 0x38 &&
    dockToPlayer.Target == Target && dockToPlayer.Value28 == 1 &&
    dockToPlayer.Value2C == 2 && dockToPlayer.Value30 == 3 &&
    dockToPlayer.Value34 == 4 && dockToPlayer.Value38 == 5);

var npcInteract = new NpcInteractWithTarget(Target, 6, 7, 8, 9, 10);
Check("NPCInteractWithTarget native field layout",
    NpcInteractWithTarget.MessageId == 135 &&
    NpcInteractWithTarget.Value28Offset == 0x28 &&
    NpcInteractWithTarget.Value30Offset == 0x30 &&
    NpcInteractWithTarget.Value34Offset == 0x34 &&
    NpcInteractWithTarget.Value38Offset == 0x38 &&
    NpcInteractWithTarget.Value3COffset == 0x3C &&
    npcInteract.Target == Target && npcInteract.Value28 == 6 &&
    npcInteract.Value30 == 7 && npcInteract.Value34 == 8 &&
    npcInteract.Value38 == 9 && npcInteract.Value3C == 10);

var deactivateAbility = new DeactivateAbility(Target, 0x11223344, 0x55);
Check("DeactivateAbility native field layout",
    DeactivateAbility.MessageId == 137 &&
    DeactivateAbility.Value28Offset == 0x28 && DeactivateAbility.Value2COffset == 0x2C &&
    deactivateAbility.Target == Target && deactivateAbility.Value28 == 0x11223344 &&
    deactivateAbility.Value2C == 0x55);

var performEmote = new PerformEmote(Target, 0xAABBCCDD, -1234);
Check("PerformEmote native field layout",
    PerformEmote.MessageId == 142 &&
    PerformEmote.Value28Offset == 0x28 && PerformEmote.Value2COffset == 0x2C &&
    performEmote.Target == Target && performEmote.Value28 == 0xAABBCCDD &&
    performEmote.Value2C == -1234);

var activateConsumable = new ActivateConsumable(Target, 0x11223344, 0x55667788);
Check("ActivateConsumable native field layout",
    ActivateConsumable.MessageId == 138 &&
    ActivateConsumable.Value28Offset == 0x28 && ActivateConsumable.Value2COffset == 0x2C &&
    activateConsumable.Target == Target && activateConsumable.Value28 == 0x11223344 &&
    activateConsumable.Value2C == 0x55667788);

var setNoSpread = new SetNoSpreadFlag(Target, 0x12, 0x34);
Check("SetNoSpreadFlag native field layout",
    SetNoSpreadFlag.MessageId == 139 &&
    SetNoSpreadFlag.Value28Offset == 0x28 && SetNoSpreadFlag.Value29Offset == 0x29 &&
    setNoSpread.Target == Target && setNoSpread.Value28 == 0x12 && setNoSpread.Value29 == 0x34);

var exitAttachment = new ExitAttachmentRequest(Target);
Check("ExitAttachmentRequest kind-only native layout",
    ExitAttachmentRequest.MessageId == 140 && exitAttachment.Target == Target);

var requestAllAchievements = new RequestAllAchievements(Target);
Check("RequestAllAchievements kind-only native layout",
    RequestAllAchievements.MessageId == 170 && requestAllAchievements.Target == Target);

var listAchievements = new ListAchievements(Target, 0x5A);
Check("ListAchievements native field layout",
    ListAchievements.MessageId == 168 && ListAchievements.ValueOffset == 0x28 &&
    listAchievements.Target == Target && listAchievements.Value == 0x5A);

var listPreviousBounties = new ListPreviousBounties(Target, 0xAABBCCDD);
Check("ListPreviousBounties native field layout",
    ListPreviousBounties.MessageId == 163 && ListPreviousBounties.ValueOffset == 0x28 &&
    listPreviousBounties.Target == Target && listPreviousBounties.Value == 0xAABBCCDD);

var abortCampaignMission = new AbortCampaignMission(Target, 0x12345678);
Check("AbortCampaignMission native field layout",
    AbortCampaignMission.MessageId == 152 && AbortCampaignMission.ValueOffset == 0x28 &&
    abortCampaignMission.Target == Target && abortCampaignMission.Value == 0x12345678);

var animationUpdate = new AnimationUpdate(Target, 0x11223344, 0x55667788, 0x99);
Check("AnimationUpdate native field layout",
    AnimationUpdate.MessageId == 149 && AnimationUpdate.Value1Offset == 0x28 &&
    AnimationUpdate.Value2Offset == 0x2C && AnimationUpdate.Value3Offset == 0x30 &&
    animationUpdate.Target == Target && animationUpdate.Value1 == 0x11223344 &&
    animationUpdate.Value2 == 0x55667788 && animationUpdate.Value3 == 0x99);

var resumeTutorial = new TryResumeTutorialChain(Target);
Check("TryResumeTutorialChain kind-only native layout",
    TryResumeTutorialChain.MessageId == 153 && resumeTutorial.Target == Target);

var callForHelp = new CallForHelp(Target, 0x12345678);
Check("CallForHelp native field layout",
    CallForHelp.MessageId == 151 && CallForHelp.ValueOffset == 0x28 &&
    callForHelp.Target == Target && callForHelp.Value == 0x12345678);

var listAvailableBounties = new ListAvailableBounties(Target, 0x11223344, 0x55667788);
Check("ListAvailableBounties native field layout",
    ListAvailableBounties.MessageId == 160 &&
    ListAvailableBounties.Value1Offset == 0x28 &&
    ListAvailableBounties.Value2Offset == 0x2C &&
    listAvailableBounties.Target == Target &&
    listAvailableBounties.Value1 == 0x11223344 &&
    listAvailableBounties.Value2 == 0x55667788);

var setDialogTag = new SetDialogTag(Target, 0x10203040, 0x50607080, 0x90, 0xA0);
Check("SetDialogTag native field layout",
    SetDialogTag.MessageId == 147 &&
    SetDialogTag.SharedTextReferenceOffset == 0x28 &&
    SetDialogTag.Value2Offset == 0x2C &&
    SetDialogTag.Value3Offset == 0x30 &&
    SetDialogTag.Value4Offset == 0x31 &&
    setDialogTag.Target == Target &&
    setDialogTag.SharedTextReference == 0x10203040 &&
    setDialogTag.Value2 == 0x50607080 &&
    setDialogTag.Value3 == 0x90 &&
    setDialogTag.Value4 == 0xA0);

var notifyDialogScriptComplete = new NotifyDialogScriptComplete(Target, 0x11223344, 0x55667788);
Check("NotifyDialogScriptComplete native field layout",
    NotifyDialogScriptComplete.MessageId == 143 &&
    NotifyDialogScriptComplete.Value1Offset == 0x28 &&
    NotifyDialogScriptComplete.Value2Offset == 0x2C &&
    notifyDialogScriptComplete.Target == Target &&
    notifyDialogScriptComplete.Value1 == 0x11223344 &&
    notifyDialogScriptComplete.Value2 == 0x55667788);

var performQuickChatCommand = new PerformQuickChatCommand(Target, 0x12345678);
Check("PerformQuickChatCommand native field layout",
    PerformQuickChatCommand.MessageId == 144 &&
    PerformQuickChatCommand.ValueOffset == 0x28 &&
    performQuickChatCommand.Target == Target &&
    performQuickChatCommand.Value == 0x12345678);

var performDialog = new PerformDialog(
    Target, 0x01020304, 0x11121314, 0x21222324, 0x31323334, 0x41424344,
    0x51525354, 0x61, 0x71727374, 0x81, 0x91929394, 0xA1A2A3A4, 0xB1B2B3B4);
Check("PerformDialog native field layout",
    PerformDialog.MessageId == 146 && performDialog.Target == Target &&
    performDialog.Value28 == 0x01020304 && performDialog.Value30 == 0x11121314 &&
    performDialog.Value34 == 0x21222324 && performDialog.Value38 == 0x31323334 &&
    performDialog.Value3C == 0x41424344 && performDialog.Value40 == 0x51525354 &&
    performDialog.Value44 == 0x61 && performDialog.Value48 == 0x71727374 &&
    performDialog.Value4C == 0x81 && performDialog.Value50 == 0x91929394 &&
    performDialog.Value58 == 0xA1A2A3A4 && performDialog.Value5C == 0xB1B2B3B4);

var setEffectsFlag = new SetEffectsFlag(Target, 1, 0x5A);
Check("SetEffectsFlag native field layout",
    SetEffectsFlag.MessageId == 148 && SetEffectsFlag.Value28Offset == 0x28 &&
    SetEffectsFlag.Value29Offset == 0x29 && setEffectsFlag.Target == Target &&
    setEffectsFlag.Value28 == 1 && setEffectsFlag.Value29 == 0x5A);

var selectLoadout = new SelectLoadout(Target, 0x12345678, 0x5A);
Check("SelectLoadout native field layout",
    SelectLoadout.MessageId == 150 && SelectLoadout.ValueOffset == 0x28 &&
    SelectLoadout.FlagOffset == 0x2C && selectLoadout.Target == Target &&
    selectLoadout.Value == 0x12345678 && selectLoadout.Flag == 0x5A);

var debugMission = new DebugMission(Target, 0x12345678, 0xAABBCCDD);
Check("DebugMission native field layout",
    DebugMission.MessageId == 154 && DebugMission.ValueOffset == 0x28 &&
    DebugMission.SharedTextReferenceOffset == 0x2C && debugMission.Target == Target &&
    debugMission.Value == 0x12345678 && debugMission.SharedTextReference == 0xAABBCCDD);

var assignBounties = new AssignBounties(Target, 0x12345678);
Check("AssignBounties native field layout",
    AssignBounties.MessageId == 155 && AssignBounties.ValueOffset == 0x28 &&
    assignBounties.Target == Target && assignBounties.Value == 0x12345678);

var abortBounty = new AbortBounty(Target, 0x12345678);
Check("AbortBounty native field layout",
    AbortBounty.MessageId == 156 && AbortBounty.ValueOffset == 0x28 &&
    abortBounty.Target == Target && abortBounty.Value == 0x12345678);

var activateBounty = new ActivateBounty(Target, 0x12345678);
Check("ActivateBounty native field layout",
    ActivateBounty.MessageId == 157 && ActivateBounty.ValueOffset == 0x28 &&
    activateBounty.Target == Target && activateBounty.Value == 0x12345678);

var listActiveBounties = new ListActiveBounties(Target, 0x12345678, 0x5A);
Check("ListActiveBounties native field layout",
    ListActiveBounties.MessageId == 158 && ListActiveBounties.ValueOffset == 0x28 &&
    ListActiveBounties.FlagOffset == 0x2C && listActiveBounties.Target == Target &&
    listActiveBounties.Value == 0x12345678 && listActiveBounties.Flag == 0x5A);

var listActiveBountyDetails = new ListActiveBountyDetails(Target, 0x12345678, 0x5A);
Check("ListActiveBountyDetails native field layout",
    ListActiveBountyDetails.MessageId == 159 &&
    ListActiveBountyDetails.ValueOffset == 0x28 &&
    ListActiveBountyDetails.FlagOffset == 0x2C &&
    listActiveBountyDetails.Target == Target && listActiveBountyDetails.Value == 0x12345678 &&
    listActiveBountyDetails.Flag == 0x5A);

var clearBounties = new ClearBounties(Target, 0x12345678);
Check("ClearBounties native field layout",
    ClearBounties.MessageId == 161 && ClearBounties.ValueOffset == 0x28 &&
    clearBounties.Target == Target && clearBounties.Value == 0x12345678);

var trackBounty = new TrackBounty(Target, 0x12345678);
Check("TrackBounty native field layout",
    TrackBounty.MessageId == 165 && TrackBounty.ValueOffset == 0x28 &&
    trackBounty.Target == Target && trackBounty.Value == 0x12345678);

var setBountyVar = new SetBountyVar(Target, 0x12345678, 0xAABBCCDD, 0xEEFF0011);
Check("SetBountyVar native field layout",
    SetBountyVar.MessageId == 166 && SetBountyVar.ValueOffset == 0x28 &&
    SetBountyVar.OpaqueReference1Offset == 0x2C &&
    SetBountyVar.OpaqueReference2Offset == 0x30 && setBountyVar.Target == Target &&
    setBountyVar.Value == 0x12345678 && setBountyVar.OpaqueReference1 == 0xAABBCCDD &&
    setBountyVar.OpaqueReference2 == 0xEEFF0011);

var refreshBounties = new RefreshBounties(Target);
Check("RefreshBounties kind-only native layout",
    RefreshBounties.MessageId == 167 && refreshBounties.Target == Target);

var interactionEntries = new ulong[] { 0x1122334455667788, 0x99AABBCCDDEEFF00 };
var npcSetInteractionType = new NpcSetInteractionType(Target, 0x5A, interactionEntries);
interactionEntries[0] = 0;
Check("NPCSetInteractionType native vector snapshot",
    NpcSetInteractionType.MessageId == 141 &&
    NpcSetInteractionType.Value28Offset == 0x28 &&
    NpcSetInteractionType.EntriesOffset == 0x30 &&
    NpcSetInteractionType.InlineStorageOffset == 0x40 &&
    npcSetInteractionType.Target == Target && npcSetInteractionType.Value28 == 0x5A &&
    npcSetInteractionType.Entries.Count == 2 &&
    npcSetInteractionType.Entries[0] == 0x1122334455667788 &&
    npcSetInteractionType.Entries[1] == 0x99AABBCCDDEEFF00);

var abilityEntries = new[]
{
    new ActivateAbilityEntry(0x11223344, 0x55667788),
    new ActivateAbilityEntry(0x99AABBCC, 0xDDEEFF00)
};
var activateAbility = new ActivateAbility(Target, 0x12345678, 0x5A, abilityEntries, 0xA5);
abilityEntries[0] = new ActivateAbilityEntry(0, 0);
Check("ActivateAbility native vector snapshot",
    ActivateAbility.MessageId == 134 &&
    ActivateAbility.Value28Offset == 0x28 &&
    ActivateAbility.Value2COffset == 0x2C &&
    ActivateAbility.EntriesOffset == 0x30 &&
    ActivateAbility.InlineStorageOffset == 0x4C &&
    ActivateAbility.ObservedInlineCapacity == 4 &&
    ActivateAbility.Value6COffset == 0x6C &&
    ActivateAbility.Value70Offset == 0x70 &&
    ActivateAbility.Value74Offset == 0x74 &&
    ActivateAbility.Value78Offset == 0x78 &&
    activateAbility.Target == Target && activateAbility.Value28 == 0x12345678 &&
    activateAbility.Value2C == 0x5A && activateAbility.Value78 == 0xA5 &&
    activateAbility.Value6C is null &&
    activateAbility.Value70 is null &&
    activateAbility.Value74 is null &&
    activateAbility.Entries.Count == 2 &&
    activateAbility.Entries[0] == new ActivateAbilityEntry(0x11223344, 0x55667788));
var activateAbilityPopulated = new ActivateAbility(
    Target, 1, 2, Array.Empty<ActivateAbilityEntry>(), 3, 4, 5, 6);
Check("ActivateAbility optional trailing DWORDs",
    activateAbilityPopulated.Value6C == 4 &&
    activateAbilityPopulated.Value70 == 5 &&
    activateAbilityPopulated.Value74 == 6);

var mailItems = new[]
{
    new SendMailToPlayerItem(1, 2, 3, 4, 5, 6),
    new SendMailToPlayerItem(7, 8, 9, 10, 11, 12)
};
var sendMailToPlayer = new SendMailToPlayer(
    Target, 0x10111213, 0x20212223, 0x30313233, 0x40414243, 0x50515253, mailItems);
mailItems[0] = new SendMailToPlayerItem(0, 0, 0, 0, 0, 0);
Check("SendMailToPlayer native vector snapshot",
    SendMailToPlayer.MessageId == 84 &&
    SendMailToPlayer.TargetOffset == 0x18 &&
    SendMailToPlayer.Value28Offset == 0x28 &&
    SendMailToPlayer.Value2COffset == 0x2C &&
    SendMailToPlayer.SharedText30Offset == 0x30 &&
    SendMailToPlayer.SharedText34Offset == 0x34 &&
    SendMailToPlayer.SharedText38Offset == 0x38 &&
    SendMailToPlayer.ItemsOffset == 0x3C &&
    SendMailToPlayer.ItemSize == 24 &&
    sendMailToPlayer.Target == Target &&
    sendMailToPlayer.Value28 == 0x10111213 &&
    sendMailToPlayer.Value2C == 0x20212223 &&
    sendMailToPlayer.SharedText30 == 0x30313233 &&
    sendMailToPlayer.SharedText34 == 0x40414243 &&
    sendMailToPlayer.SharedText38 == 0x50515253 &&
    sendMailToPlayer.Items.Count == 2 &&
    sendMailToPlayer.Items[0] == new SendMailToPlayerItem(1, 2, 3, 4, 5, 6));

var listItemForSale = new ListItemForSale(
    Target, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0xAB);
Check("ListItemForSale native positional layout",
    ListItemForSale.MessageId == 83 &&
    ListItemForSale.TargetOffset == 0x18 &&
    ListItemForSale.Value28AOffset == 0x28 &&
    ListItemForSale.Value28BOffset == 0x2C &&
    ListItemForSale.Value30AOffset == 0x30 &&
    ListItemForSale.Value30BOffset == 0x34 &&
    ListItemForSale.Value38Offset == 0x38 &&
    ListItemForSale.Value3COffset == 0x3C &&
    ListItemForSale.SharedText40Offset == 0x40 &&
    ListItemForSale.Value48Offset == 0x48 &&
    ListItemForSale.Value4COffset == 0x4C &&
    ListItemForSale.SharedText50Offset == 0x50 &&
    ListItemForSale.Flag54Offset == 0x54 &&
    listItemForSale.Target == Target &&
    listItemForSale.Value28A == 1 &&
    listItemForSale.Value28B == 2 &&
    listItemForSale.Value30A == 3 &&
    listItemForSale.Value30B == 4 &&
    listItemForSale.Value38 == 5 &&
    listItemForSale.Value3C == 6 &&
    listItemForSale.SharedText40 == 7 &&
    listItemForSale.Value48 == 8 &&
    listItemForSale.Value4C == 9 &&
    listItemForSale.SharedText50 == 10 &&
    listItemForSale.Flag54 == 0xAB);

var clearSavedMatchQueue = new ClearSavedMatchQueue(0x11223344, 0x55667788);
Check("ClearSavedMatchQueue initialized native prefix",
    ClearSavedMatchQueue.MessageId == 91 &&
    ClearSavedMatchQueue.Value18Offset == 0x18 &&
    ClearSavedMatchQueue.Value1COffset == 0x1C &&
    ClearSavedMatchQueue.Value20Offset == 0x20 &&
    ClearSavedMatchQueue.Value24Offset == 0x24 &&
    ClearSavedMatchQueue.InitializedPrefixSize == 0x28 &&
    clearSavedMatchQueue.Value18 == 0x11223344 &&
    clearSavedMatchQueue.Value1C == 0x55667788 &&
    clearSavedMatchQueue.Value20 == 0 &&
    clearSavedMatchQueue.Value24 == 0);

var matchAccept = new MatchAccept(Target, 0x11223344, 0x55667788, 0x99AABBCC, 0xDD);
Check("MatchAccept native positional layout",
    MatchAccept.MessageId == 93 &&
    MatchAccept.TargetOffset == 0x18 &&
    MatchAccept.Value28Offset == 0x28 &&
    MatchAccept.Value30Offset == 0x30 &&
    MatchAccept.Value34Offset == 0x34 &&
    MatchAccept.Value38Offset == 0x38 &&
    matchAccept.Target == Target &&
    matchAccept.Value28 == 0x11223344 &&
    matchAccept.Value30 == 0x55667788 &&
    matchAccept.Value34 == 0x99AABBCC &&
    matchAccept.Value38 == 0xDD);

var playerReadyEntries = new ulong[] { 0x1122334455667788, 0x99AABBCCDDEEFF00 };
var playerReady = new PlayerReady(Target, 0x12, 0x34, true, playerReadyEntries);
playerReadyEntries[0] = 0;
Check("PlayerReady optional native vector snapshot",
    PlayerReady.MessageId == 88 &&
    PlayerReady.TargetOffset == 0x18 &&
    PlayerReady.Value28Offset == 0x28 &&
    PlayerReady.Value29Offset == 0x29 &&
    PlayerReady.HasEntriesOffset == 0x2C &&
    PlayerReady.EntriesOffset == 0x30 &&
    PlayerReady.EntrySize == 8 &&
    playerReady.Target == Target &&
    playerReady.Value28 == 0x12 &&
    playerReady.Value29 == 0x34 &&
    playerReady.HasEntries &&
    playerReady.Entries.Count == 2 &&
    playerReady.Entries[0] == 0x1122334455667788);

var toggleMarketplace = new ToggleMarketplace(Target, 0x7F);
Check("ToggleMarketplace native field layout",
    ToggleMarketplace.MessageId == 86 &&
    ToggleMarketplace.TargetOffset == 0x18 &&
    ToggleMarketplace.Value28Offset == 0x28 &&
    toggleMarketplace.Target == Target &&
    toggleMarketplace.Value28 == 0x7F);

var fillBuyOrder = new FillBuyOrder(
    Target, 1, 2, 3, 4, 5, 6, 7, 8);
Check("FillBuyOrder native positional layout",
    FillBuyOrder.MessageId == 85 &&
    FillBuyOrder.TargetOffset == 0x18 &&
    FillBuyOrder.Value28Offset == 0x28 &&
    FillBuyOrder.Value2COffset == 0x2C &&
    FillBuyOrder.Value30Offset == 0x30 &&
    FillBuyOrder.Value34Offset == 0x34 &&
    FillBuyOrder.Value38Offset == 0x38 &&
    FillBuyOrder.Value3COffset == 0x3C &&
    FillBuyOrder.Value40Offset == 0x40 &&
    FillBuyOrder.Value44Offset == 0x44 &&
    fillBuyOrder.Target == Target &&
    fillBuyOrder.Value28 == 1 &&
    fillBuyOrder.Value2C == 2 &&
    fillBuyOrder.Value30 == 3 &&
    fillBuyOrder.Value34 == 4 &&
    fillBuyOrder.Value38 == 5 &&
    fillBuyOrder.Value3C == 6 &&
    fillBuyOrder.Value40 == 7 &&
    fillBuyOrder.Value44 == 8);

var performTextChat = new PerformTextChat(
    Target,
    0x11223344,
    0x55,
    true,
    0x66778899,
    0xAABBCCDD,
    true,
    0x12345678,
    0x9A,
    new PerformTextChatOptionalDescriptor(0x10203040, 0x50607080, 0x90A0B0C0));
var performTextChatWithoutOptional = new PerformTextChat(
    Target, 1, 2, false, null, null, false, null, 3, null);
Check("PerformTextChat native nested layout",
    PerformTextChat.MessageId == 145 &&
    PerformTextChat.TargetOffset == 0x18 &&
    PerformTextChat.SharedText28Offset == 0x28 &&
    PerformTextChat.Value2COffset == 0x2C &&
    PerformTextChat.HasNestedPairOffset == 0x30 &&
    PerformTextChat.Value38Offset == 0x38 &&
    PerformTextChat.Value3COffset == 0x3C &&
    PerformTextChat.HasSharedText44Offset == 0x40 &&
    PerformTextChat.SharedText44Offset == 0x44 &&
    PerformTextChat.Value48Offset == 0x48 &&
    PerformTextChat.HasOptionalDescriptorOffset == 0x4C &&
    PerformTextChat.OptionalDescriptorOffset == 0x50 &&
    PerformTextChat.OptionalDescriptorVtable == 0x1C80CC4 &&
    performTextChat.Target == Target &&
    performTextChat.SharedText28 == 0x11223344 &&
    performTextChat.Value2C == 0x55 &&
    performTextChat.HasNestedPair &&
    performTextChat.Value38 == 0x66778899 &&
    performTextChat.Value3C == 0xAABBCCDD &&
    performTextChat.HasSharedText44 &&
    performTextChat.SharedText44 == 0x12345678 &&
    performTextChat.Value48 == 0x9A &&
    performTextChat.OptionalDescriptor ==
        new PerformTextChatOptionalDescriptor(0x10203040, 0x50607080, 0x90A0B0C0) &&
    !performTextChatWithoutOptional.HasNestedPair &&
    performTextChatWithoutOptional.Value38 is null &&
    performTextChatWithoutOptional.Value3C is null &&
    !performTextChatWithoutOptional.HasSharedText44 &&
    performTextChatWithoutOptional.SharedText44 is null &&
    performTextChatWithoutOptional.OptionalDescriptor is null);

var matchmakerSetPenalties = new MatchmakerSetPenalties(Target, 1, 2, 3, 4);
Check("MatchmakerSetPenalties native positional layout",
    MatchmakerSetPenalties.MessageId == 92 &&
    MatchmakerSetPenalties.TargetOffset == 0x18 &&
    MatchmakerSetPenalties.Value28Offset == 0x28 &&
    MatchmakerSetPenalties.Value2COffset == 0x2C &&
    MatchmakerSetPenalties.Value30Offset == 0x30 &&
    MatchmakerSetPenalties.Value34Offset == 0x34 &&
    matchmakerSetPenalties.Target == Target &&
    matchmakerSetPenalties.Value28 == 1 &&
    matchmakerSetPenalties.Value2C == 2 &&
    matchmakerSetPenalties.Value30 == 3 &&
    matchmakerSetPenalties.Value34 == 4);

var joinGroupLeader = new JoinGroupLeader(Target);
Check("JoinGroupLeader kind-only native layout",
    JoinGroupLeader.MessageId == 87 &&
    JoinGroupLeader.TargetOffset == 0x18 &&
    joinGroupLeader.Target == Target);

var fetchQueueReferences = new uint[] { 0x11223344, 0x55667788 };
var fetchQueueInfo = new FetchQueueInfo(Target, fetchQueueReferences, 3, 4, 5);
fetchQueueReferences[0] = 0;
Check("FetchQueueInfo native vector layout",
    FetchQueueInfo.MessageId == 89 &&
    FetchQueueInfo.TargetOffset == 0x18 &&
    FetchQueueInfo.ReferencesOffset == 0x28 &&
    FetchQueueInfo.ReferenceSize == 4 &&
    FetchQueueInfo.Value38Offset == 0x38 &&
    FetchQueueInfo.Value3COffset == 0x3C &&
    FetchQueueInfo.Value40Offset == 0x40 &&
    fetchQueueInfo.Target == Target &&
    fetchQueueInfo.References.Count == 2 &&
    fetchQueueInfo.References[0] == 0x11223344 &&
    fetchQueueInfo.Value38 == 3 &&
    fetchQueueInfo.Value3C == 4 &&
    fetchQueueInfo.Value40 == 5);

var clientUIEvent = new ClientUIEvent(Target, 0xA1B2C3D4);
Check("ClientUIEvent native shared-text layout",
    ClientUIEvent.MessageId == 196 &&
    ClientUIEvent.TargetOffset == 0x18 &&
    ClientUIEvent.SharedTextReferenceOffset == 0x28 &&
    clientUIEvent.Target == Target &&
    clientUIEvent.SharedTextReference == 0xA1B2C3D4);

var vendorPurchase = new VendorPurchaseRequest(
    Target,
    0x0102030405060708,
    0x1112131415161718,
    0x2122232425262728,
    0x3132333435363738,
    new NativeOptionalDword(true, 0x41424344),
    new NativeOptionalDword(false, 0x51525354),
    new NativeOptionalDword(true, 0x61626364));
Check("VendorPurchaseRequest native fields and option flags",
    VendorPurchaseRequest.MessageId == 238 &&
    VendorPurchaseRequest.TargetOffset == 0x18 &&
    VendorPurchaseRequest.Value28Offset == 0x28 &&
    VendorPurchaseRequest.Value30Offset == 0x30 &&
    VendorPurchaseRequest.Value38Offset == 0x38 &&
    VendorPurchaseRequest.Value40Offset == 0x40 &&
    VendorPurchaseRequest.HasValue48Offset == 0x48 &&
    VendorPurchaseRequest.Value4COffset == 0x4C &&
    VendorPurchaseRequest.HasValue50Offset == 0x50 &&
    VendorPurchaseRequest.Value54Offset == 0x54 &&
    VendorPurchaseRequest.HasSharedText58Offset == 0x58 &&
    VendorPurchaseRequest.SharedText5COffset == 0x5C &&
    vendorPurchase.Target == Target &&
    vendorPurchase.Value28 == 0x0102030405060708 &&
    vendorPurchase.Value40 == 0x3132333435363738 &&
    vendorPurchase.Value4C is { IsPresent: true, Value: 0x41424344 } &&
    vendorPurchase.Value54 is { IsPresent: false } &&
    vendorPurchase.SharedText5C is { IsPresent: true, Value: 0x61626364 });

var vendorTokenMachineRequest = new VendorTokenMachineRequest(Target, 3, 0xAABBCCDD, 0x11223344);
Check("VendorTokenMachineRequest native field layout",
    VendorTokenMachineRequest.MessageId == 200 &&
    VendorTokenMachineRequest.TargetOffset == 0x18 &&
    VendorTokenMachineRequest.ModeOffset == 0x28 &&
    VendorTokenMachineRequest.Reserved29Offset == 0x29 &&
    VendorTokenMachineRequest.Reserved29Value == 0 &&
    VendorTokenMachineRequest.Value2COffset == 0x2C &&
    VendorTokenMachineRequest.Value30Offset == 0x30 &&
    vendorTokenMachineRequest.Target == Target &&
    vendorTokenMachineRequest.Mode == 3 &&
    vendorTokenMachineRequest.Value2C == 0xAABBCCDD &&
    vendorTokenMachineRequest.Value30 == 0x11223344);

var findServiceProvider = new FindServiceProvider(Target, 1);
Check("FindServiceProvider native field layout",
    FindServiceProvider.MessageId == 195 &&
    FindServiceProvider.TargetOffset == 0x18 &&
    FindServiceProvider.Value28Offset == 0x28 &&
    findServiceProvider.Target == Target &&
    findServiceProvider.Value28 == 1);

var updateFriendStatus = new UpdateFriendStatus(
    Target, 0x12345678, 1, new NativeOptionalDword(true, 0x9ABCDEF0));
var updateFriendStatusWithoutOptional = new UpdateFriendStatus(
    Target, 0x12345678, 0, new NativeOptionalDword(false, 0));
Check("UpdateFriendStatus native shared-text fields",
    UpdateFriendStatus.MessageId == 249 &&
    UpdateFriendStatus.TargetOffset == 0x18 &&
    UpdateFriendStatus.SharedText28Offset == 0x28 &&
    UpdateFriendStatus.Value2COffset == 0x2C &&
    UpdateFriendStatus.HasSharedText30Offset == 0x30 &&
    UpdateFriendStatus.SharedText34Offset == 0x34 &&
    updateFriendStatus.Target == Target &&
    updateFriendStatus.SharedText28 == 0x12345678 &&
    updateFriendStatus.Value2C == 1 &&
    updateFriendStatus.SharedText30 is { IsPresent: true, Value: 0x9ABCDEF0 } &&
    updateFriendStatusWithoutOptional.Value2C == 0 &&
    !updateFriendStatusWithoutOptional.SharedText30.IsPresent);

var friendsListRequest = new FriendsListRequest(Target, 0x1234ABCD);
Check("FriendsListRequest native field layout",
    FriendsListRequest.MessageId == 248 &&
    FriendsListRequest.TargetOffset == 0x18 &&
    FriendsListRequest.Value28Offset == 0x28 &&
    friendsListRequest.Target == Target &&
    friendsListRequest.Value28 == 0x1234ABCD);

var clientQueryInteractionStatus = new ClientQueryInteractionStatus(
    Target, 0x1122334455667788, 1);
Check("ClientQueryInteractionStatus native field layout",
    ClientQueryInteractionStatus.MessageId == 186 &&
    ClientQueryInteractionStatus.TargetOffset == 0x18 &&
    ClientQueryInteractionStatus.Value28Offset == 0x28 &&
    ClientQueryInteractionStatus.Value30Offset == 0x30 &&
    clientQueryInteractionStatus.Target == Target &&
    clientQueryInteractionStatus.Value28 == 0x1122334455667788 &&
    clientQueryInteractionStatus.Value30 == 1);

var claimBountyRewards = new ClaimBountyRewards(Target);
Check("ClaimBountyRewards kind-only native layout",
    ClaimBountyRewards.MessageId == 250 &&
    ClaimBountyRewards.TargetOffset == 0x18 &&
    claimBountyRewards.Target == Target);

var lootVoteEntries = new ulong[] { 0x1122334455667788, 0x99AABBCCDDEEFF00 };
var lootDistributionSetVotes = new LootDistributionSetVotes(Target, 7, lootVoteEntries);
lootVoteEntries[0] = 0;
Check("LootDistributionSetVotes native vector layout",
    LootDistributionSetVotes.MessageId == 219 &&
    LootDistributionSetVotes.TargetOffset == 0x18 &&
    LootDistributionSetVotes.Value28Offset == 0x28 &&
    LootDistributionSetVotes.EntriesOffset == 0x2C &&
    LootDistributionSetVotes.EntrySize == 8 &&
    lootDistributionSetVotes.Target == Target &&
    lootDistributionSetVotes.Value28 == 7 &&
    lootDistributionSetVotes.Entries.Count == 2 &&
    lootDistributionSetVotes.Entries[0] == 0x1122334455667788 &&
    lootDistributionSetVotes.Entries[1] == 0x99AABBCCDDEEFF00);

var uiQueryValues = new[]
{
    new KeyValuePair<uint, uint>(9, 90),
    new KeyValuePair<uint, uint>(3, 30),
};
var uiQueryResponse = new UiQueryResponse(Target, 1, 2, 3, uiQueryValues);
uiQueryValues[0] = new(99, 990);
Check("UiQueryResponse native fields and copied ordered map",
    UiQueryResponse.MessageId == 59 &&
    UiQueryResponse.TargetOffset == 0x18 &&
    UiQueryResponse.Value28Offset == 0x28 &&
    UiQueryResponse.Value2COffset == 0x2C &&
    UiQueryResponse.Value30Offset == 0x30 &&
    UiQueryResponse.ValuesOffset == 0x34 &&
    uiQueryResponse.Target == Target &&
    uiQueryResponse.Value28 == 1 &&
    uiQueryResponse.Value2C == 2 &&
    uiQueryResponse.Value30 == 3 &&
    uiQueryResponse.Values.SequenceEqual(new[]
    {
        new KeyValuePair<uint, uint>(3, 30),
        new KeyValuePair<uint, uint>(9, 90),
    }));
Check("UiQueryResponse rejects duplicate map keys",
    ThrowsArgument(() => new UiQueryResponse(Target, 0, 0, 0,
    [
        new KeyValuePair<uint, uint>(1, 2),
        new KeyValuePair<uint, uint>(1, 3),
    ])));

var lootDistributionSetState = new LootDistributionSetState(Target, 0x12345678, 2);
Check("LootDistributionSetState native positional fields",
    LootDistributionSetState.MessageId == 218 &&
    LootDistributionSetState.TargetOffset == 0x18 &&
    LootDistributionSetState.Value28Offset == 0x28 &&
    LootDistributionSetState.Value2COffset == 0x2C &&
    lootDistributionSetState.Target == Target &&
    lootDistributionSetState.Value28 == 0x12345678 &&
    lootDistributionSetState.Value2C == 2);

var requestTrackerUpdate = new RequestTrackerUpdate(Target, 0x11223344, 0x55667788);
Check("RequestTrackerUpdate native positional fields",
    RequestTrackerUpdate.MessageId == 217 &&
    RequestTrackerUpdate.TargetOffset == 0x18 &&
    RequestTrackerUpdate.Value28Offset == 0x28 &&
    RequestTrackerUpdate.Value2COffset == 0x2C &&
    requestTrackerUpdate.Target == Target &&
    requestTrackerUpdate.Value28 == 0x11223344 &&
    requestTrackerUpdate.Value2C == 0x55667788);

var leaveArc = new LeaveArc(Target);
Check("LeaveArc kind-only native layout",
    LeaveArc.MessageId == 214 &&
    LeaveArc.TargetOffset == 0x18 &&
    leaveArc.Target == Target);

var requestFrameLevelReset = new RequestFrameLevelReset(Target, 0x12345678);
Check("RequestFrameLevelReset native positional field",
    RequestFrameLevelReset.MessageId == 211 &&
    RequestFrameLevelReset.TargetOffset == 0x18 &&
    RequestFrameLevelReset.Value28Offset == 0x28 &&
    requestFrameLevelReset.Target == Target &&
    requestFrameLevelReset.Value28 == 0x12345678);

uint[] jobLedgerEntries = [0x11223344, 0x55667788];
var jobLedgerOperation = new JobLedgerOperation(Target, 0xAABBCCDD, jobLedgerEntries);
jobLedgerEntries[0] = 0;
Check("JobLedgerOperation native fields and copied u32 vector",
    JobLedgerOperation.MessageId == 215 &&
    JobLedgerOperation.TargetOffset == 0x18 &&
    JobLedgerOperation.SharedTextReferenceOffset == 0x28 &&
    JobLedgerOperation.EntriesOffset == 0x2C &&
    JobLedgerOperation.EntrySize == 4 &&
    jobLedgerOperation.Target == Target &&
    jobLedgerOperation.SharedTextReference == 0xAABBCCDD &&
    jobLedgerOperation.Entries.SequenceEqual([0x11223344u, 0x55667788u]));

byte[] tinkeringFirst = Enumerable.Range(0, 16).Select(i => (byte)i).ToArray();
byte[] tinkeringSecond = Enumerable.Range(16, 32).Select(i => (byte)i).ToArray();
var executeTinkeringPlan = new ExecuteTinkeringPlan(
    Target, 0x12345678, tinkeringFirst, tinkeringSecond, ReadOnlyMemory<byte>.Empty);
tinkeringFirst[0] = 0xFF;
Check("ExecuteTinkeringPlan native fields and copied opaque 16-byte vectors",
    ExecuteTinkeringPlan.MessageId == 237 &&
    ExecuteTinkeringPlan.TargetOffset == 0x18 &&
    ExecuteTinkeringPlan.Value28Offset == 0x28 &&
    ExecuteTinkeringPlan.FirstVectorOffset == 0x2C &&
    ExecuteTinkeringPlan.SecondVectorOffset == 0x38 &&
    ExecuteTinkeringPlan.ThirdVectorOffset == 0x44 &&
    ExecuteTinkeringPlan.VectorElementSize == 16 &&
    executeTinkeringPlan.Target == Target &&
    executeTinkeringPlan.Value28 == 0x12345678 &&
    executeTinkeringPlan.FirstVector.Span[0] == 0 &&
    executeTinkeringPlan.SecondVector.Length == 32 &&
    executeTinkeringPlan.ThirdVector.IsEmpty);
Check("ExecuteTinkeringPlan rejects incomplete vector elements",
    ThrowsArgument(() => new ExecuteTinkeringPlan(
        Target, 0, new byte[15], ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty)));

var queueUnstuck = new QueueUnstuck(Target);
Check("QueueUnstuck kind-only native layout",
    QueueUnstuck.MessageId == 178 &&
    QueueUnstuck.TargetOffset == 0x18 &&
    queueUnstuck.Target == Target);

var npcCombatUpdate = new NPCCombatUpdate(Target, 0x12345678, 0x9ABCDEF0);
Check("NPCCombatUpdate native positional fields",
    NPCCombatUpdate.MessageId == 233 &&
    NPCCombatUpdate.TargetOffset == 0x18 &&
    NPCCombatUpdate.Value28Offset == 0x28 &&
    NPCCombatUpdate.Value2COffset == 0x2C &&
    npcCombatUpdate.Target == Target &&
    npcCombatUpdate.Value28 == 0x12345678 &&
    npcCombatUpdate.Value2C == 0x9ABCDEF0);

var equipExperimentalLoadout = new EquipExperimentalLoadout(Target, 0x12345678);
Check("EquipExperimentalLoadout native positional field",
    EquipExperimentalLoadout.MessageId == 236 &&
    EquipExperimentalLoadout.TargetOffset == 0x18 &&
    EquipExperimentalLoadout.Value28Offset == 0x28 &&
    equipExperimentalLoadout.Target == Target &&
    equipExperimentalLoadout.Value28 == 0x12345678);

var fabricationFinalize = new Fabrication_Finalize(Target, 0x12345678, 0x9A);
Check("Fabrication_Finalize native positional fields",
    Fabrication_Finalize.MessageId == 246 &&
    Fabrication_Finalize.TargetOffset == 0x18 &&
    Fabrication_Finalize.Value28Offset == 0x28 &&
    Fabrication_Finalize.Value2COffset == 0x2C &&
    fabricationFinalize.Target == Target &&
    fabricationFinalize.Value28 == 0x12345678 &&
    fabricationFinalize.Value2C == 0x9A);

var fabricationClaim = new Fabrication_Claim(Target, 0x12345678, 0x9ABCDEF0);
Check("Fabrication_Claim native positional fields",
    Fabrication_Claim.MessageId == 247 &&
    Fabrication_Claim.TargetOffset == 0x18 &&
    Fabrication_Claim.Value28Offset == 0x28 &&
    Fabrication_Claim.Value2COffset == 0x2C &&
    fabricationClaim.Target == Target &&
    fabricationClaim.Value28 == 0x12345678 &&
    fabricationClaim.Value2C == 0x9ABCDEF0);

var fabricationGenerateResult = new Fabrication_GenerateResult(
    Target, 0x11223344, 0x55667788, 0x99AABBCC);
Check("Fabrication_GenerateResult native positional fields",
    Fabrication_GenerateResult.MessageId == 245 &&
    Fabrication_GenerateResult.TargetOffset == 0x18 &&
    Fabrication_GenerateResult.Value28Offset == 0x28 &&
    Fabrication_GenerateResult.Value2COffset == 0x2C &&
    Fabrication_GenerateResult.Value30Offset == 0x30 &&
    fabricationGenerateResult.Target == Target &&
    fabricationGenerateResult.Value28 == 0x11223344 &&
    fabricationGenerateResult.Value2C == 0x55667788 &&
    fabricationGenerateResult.Value30 == 0x99AABBCC);

var fabricationApplyAction = new Fabrication_ApplyAction(
    Target, 0x11223344, 0x55667788, 0x99AABBCC);
Check("Fabrication_ApplyAction native positional fields",
    Fabrication_ApplyAction.MessageId == 244 &&
    Fabrication_ApplyAction.TargetOffset == 0x18 &&
    Fabrication_ApplyAction.Value28Offset == 0x28 &&
    Fabrication_ApplyAction.Value2COffset == 0x2C &&
    Fabrication_ApplyAction.Value30Offset == 0x30 &&
    fabricationApplyAction.Target == Target &&
    fabricationApplyAction.Value28 == 0x11223344 &&
    fabricationApplyAction.Value2C == 0x55667788 &&
    fabricationApplyAction.Value30 == 0x99AABBCC);

var fabricationFetchInstance = new Fabrication_FetchInstance(Target, 0x12345678);
Check("Fabrication_FetchInstance native positional field",
    Fabrication_FetchInstance.MessageId == 242 &&
    Fabrication_FetchInstance.TargetOffset == 0x18 &&
    Fabrication_FetchInstance.Value28Offset == 0x28 &&
    fabricationFetchInstance.Target == Target &&
    fabricationFetchInstance.Value28 == 0x12345678);

var fabricationStart = new Fabrication_Start(Target, 0x12345678, 0x9A);
Check("Fabrication_Start native positional fields",
    Fabrication_Start.MessageId == 243 &&
    Fabrication_Start.TargetOffset == 0x18 &&
    Fabrication_Start.Value28Offset == 0x28 &&
    Fabrication_Start.Value2COffset == 0x2C &&
    fabricationStart.Target == Target &&
    fabricationStart.Value28 == 0x12345678 &&
    fabricationStart.Value2C == 0x9A);

var fabricationFetchAllInstances = new Fabrication_FetchAllInstances(Target);
Check("Fabrication_FetchAllInstances kind-only native layout",
    Fabrication_FetchAllInstances.MessageId == 240 &&
    Fabrication_FetchAllInstances.TargetOffset == 0x18 &&
    fabricationFetchAllInstances.Target == Target);

var fabricationFetchAllRecipes = new Fabrication_FetchAllRecipes(Target);
Check("Fabrication_FetchAllRecipes kind-only native layout",
    Fabrication_FetchAllRecipes.MessageId == 241 &&
    Fabrication_FetchAllRecipes.TargetOffset == 0x18 &&
    fabricationFetchAllRecipes.Target == Target);

var setSteamUserId = new SetSteamUserId(Target, 0x1122334455667788);
Check("SetSteamUserId native 64-bit field",
    SetSteamUserId.MessageId == 235 &&
    SetSteamUserId.TargetOffset == 0x18 &&
    SetSteamUserId.SteamUserIdOffset == 0x28 &&
    setSteamUserId.Target == Target &&
    setSteamUserId.SteamUserId == 0x1122334455667788);

var flushCharacterCache = new FlushCharacterCache(Target, 1, 2, 3, 4, 5);
Check("FlushCharacterCache native byte fields",
    FlushCharacterCache.MessageId == 231 &&
    FlushCharacterCache.TargetOffset == 0x18 &&
    FlushCharacterCache.Value28Offset == 0x28 &&
    FlushCharacterCache.Value29Offset == 0x29 &&
    FlushCharacterCache.Value2AOffset == 0x2A &&
    FlushCharacterCache.Value2BOffset == 0x2B &&
    FlushCharacterCache.Value2COffset == 0x2C &&
    flushCharacterCache.Target == Target &&
    flushCharacterCache.Value28 == 1 &&
    flushCharacterCache.Value29 == 2 &&
    flushCharacterCache.Value2A == 3 &&
    flushCharacterCache.Value2B == 4 &&
    flushCharacterCache.Value2C == 5);

var bagInventorySettings = new BagInventorySettings(
    Target, new BagInventorySettingsPayload(0x11223344, 0x55667788, 0x99AABBCC));
var bagInventorySettingsAbsent = new BagInventorySettings(Target, null);
Check("BagInventorySettings optional nested native fields",
    BagInventorySettings.MessageId == 234 &&
    BagInventorySettings.TargetOffset == 0x18 &&
    BagInventorySettings.HasSettingsOffset == 0x28 &&
    BagInventorySettings.SettingsSubobjectOffset == 0x2C &&
    BagInventorySettings.ReferencedObjectOffset == 0x30 &&
    BagInventorySettings.ReferenceControlBlockOffset == 0x34 &&
    BagInventorySettings.Value38Offset == 0x38 &&
    BagInventorySettings.NativeSettingsVTableAddress == 0x1C80CC4 &&
    bagInventorySettings.Target == Target &&
    bagInventorySettings.HasSettings &&
    bagInventorySettings.Settings is { ReferencedObject: 0x11223344, ReferenceControlBlock: 0x55667788, Value38: 0x99AABBCC } &&
    !bagInventorySettingsAbsent.HasSettings &&
    bagInventorySettingsAbsent.Settings is null);

var tutorialEvent = new TutorialEventTriggeredCmd(Target, 0x11223344, 0x55667788, 0x99AABBCC, 0xDD);
Check("TutorialEventTriggeredCmd positional native fields",
    TutorialEventTriggeredCmd.MessageId == 239 &&
    TutorialEventTriggeredCmd.TargetOffset == 0x18 &&
    TutorialEventTriggeredCmd.Value28Offset == 0x28 &&
    TutorialEventTriggeredCmd.Value30Offset == 0x30 &&
    TutorialEventTriggeredCmd.Value34Offset == 0x34 &&
    TutorialEventTriggeredCmd.Value38Offset == 0x38 &&
    tutorialEvent.Target == Target &&
    tutorialEvent.Value28 == 0x11223344 &&
    tutorialEvent.Value30 == 0x55667788 &&
    tutorialEvent.Value34 == 0x99AABBCC &&
    tutorialEvent.Value38 == 0xDD);

var resetEliteUpgrades = new ResetAllEliteLevelsUpgrades_Debug(Target);
var revertEliteUpgrades = new RevertAllEliteLevelsUpgrades(Target);
Check("ResetAllEliteLevelsUpgrades_Debug kind-only layout",
    ResetAllEliteLevelsUpgrades_Debug.MessageId == 230 &&
    ResetAllEliteLevelsUpgrades_Debug.TargetOffset == 0x18 &&
    resetEliteUpgrades.Target == Target);
Check("RevertAllEliteLevelsUpgrades kind-only layout",
    RevertAllEliteLevelsUpgrades.MessageId == 228 &&
    RevertAllEliteLevelsUpgrades.TargetOffset == 0x18 &&
    revertEliteUpgrades.Target == Target);

var selectEliteAward = new SelectEliteLevelsAward(Target, 0x11223344);
Check("SelectEliteLevelsAward positional native field",
    SelectEliteLevelsAward.MessageId == 229 &&
    SelectEliteLevelsAward.TargetOffset == 0x18 &&
    SelectEliteLevelsAward.Value28Offset == 0x28 &&
    selectEliteAward.Target == Target &&
    selectEliteAward.Value28 == 0x11223344);

var teamManagerCommand = new RunTeamManagerCommand(Target, 0x11, 0x22334455, 0x66);
Check("RunTeamManagerCommand native fields",
    RunTeamManagerCommand.MessageId == 232 &&
    RunTeamManagerCommand.TargetOffset == 0x18 &&
    RunTeamManagerCommand.Value28Offset == 0x28 &&
    RunTeamManagerCommand.SharedTextReferenceOffset == 0x2C &&
    RunTeamManagerCommand.Value30Offset == 0x30 &&
    teamManagerCommand.Target == Target &&
    teamManagerCommand.Value28 == 0x11 &&
    teamManagerCommand.SharedTextReference == 0x22334455 &&
    teamManagerCommand.Value30 == 0x66);

Check("FlushRewards kind-only layout",
    FlushRewards.MessageId == 226 &&
    FlushRewards.TargetOffset == 0x18 &&
    new FlushRewards(Target).Target == Target);
Check("RerollEliteLevelsAwardList kind-only layout",
    RerollEliteLevelsAwardList.MessageId == 227 &&
    RerollEliteLevelsAwardList.TargetOffset == 0x18 &&
    new RerollEliteLevelsAwardList(Target).Target == Target);

var buyBack = new BuyBackPreviousDay(Target, 0x11223344);
var acceptRewards = new AcceptRewards(Target, 0x22334455);
var claimStreak = new ClaimDailyRewardStreak(Target, 0x33445566);
var claimItem = new ClaimDailyRewardItem(Target);
Check("BuyBackPreviousDay positional field",
    BuyBackPreviousDay.MessageId == 224 &&
    BuyBackPreviousDay.Value28Offset == 0x28 &&
    buyBack.Target == Target && buyBack.Value28 == 0x11223344);
Check("AcceptRewards positional field",
    AcceptRewards.MessageId == 225 &&
    AcceptRewards.Value28Offset == 0x28 &&
    acceptRewards.Target == Target && acceptRewards.Value28 == 0x22334455);
Check("ClaimDailyRewardStreak positional field",
    ClaimDailyRewardStreak.MessageId == 223 &&
    ClaimDailyRewardStreak.Value28Offset == 0x28 &&
    claimStreak.Target == Target && claimStreak.Value28 == 0x33445566);
Check("ClaimDailyRewardItem positional field",
    ClaimDailyRewardItem.MessageId == 222 &&
    ClaimDailyRewardItem.ZeroedValue28Offset == 0x28 &&
    claimItem.Target == Target);

var seatChange = new SeatChangeRequest(Target, 0x12);
var selfRevive = new RequestSelfRevive(Target);
var timedDailyReward = new TimedDailyRewardRequest(Target, 0x34);
var newActivity = new RequestNewActivity(Target);
var pushMission = new RequestPushMission(Target, 0x55667788);
var missionAvailability = new RequestMissionAvailability(Target, 1, 2, 3);
Check("SeatChangeRequest positional byte",
    SeatChangeRequest.MessageId == 216 &&
    SeatChangeRequest.Value28Offset == 0x28 &&
    seatChange.Target == Target && seatChange.Value28 == 0x12);
Check("RequestSelfRevive kind-only layout",
    RequestSelfRevive.MessageId == 209 &&
    RequestSelfRevive.TargetOffset == 0x18 &&
    selfRevive.Target == Target);
Check("TimedDailyRewardRequest positional byte",
    TimedDailyRewardRequest.MessageId == 201 &&
    TimedDailyRewardRequest.Value28Offset == 0x28 &&
    timedDailyReward.Target == Target && timedDailyReward.Value28 == 0x34);
Check("RequestNewActivity kind-only layout",
    RequestNewActivity.MessageId == 172 &&
    RequestNewActivity.TargetOffset == 0x18 &&
    newActivity.Target == Target);
Check("RequestPushMission positional DWORD",
    RequestPushMission.MessageId == 173 &&
    RequestPushMission.Value28Offset == 0x28 &&
    pushMission.Target == Target && pushMission.Value28 == 0x55667788);
Check("RequestMissionAvailability positional DWORDs",
    RequestMissionAvailability.MessageId == 171 &&
    RequestMissionAvailability.Value28Offset == 0x28 &&
    RequestMissionAvailability.Value2COffset == 0x2C &&
    RequestMissionAvailability.Value30Offset == 0x30 &&
    missionAvailability.Target == Target &&
    missionAvailability.Value28 == 1 &&
    missionAvailability.Value2C == 2 &&
    missionAvailability.Value30 == 3);

var sinAcquire = new SinAcquire_Source(Target, 1, 2);
var broadcastTweaks = new BroadcastWeaponTweaks(Target, 3, 4, 5, 6, 7);
var tempSlotAbilities = new TempSlotAbilities(Target);
var collectLoot = new CollectLoot(Target, 8, 9, 10, 11);
var mapOpened = new MapOpened(Target);
var battleframePurchased = new BattleframePurchased(Target);
Check("SinAcquire_Source positional DWORDs",
    SinAcquire_Source.MessageId == 113 &&
    SinAcquire_Source.Value28Offset == 0x28 &&
    SinAcquire_Source.Value2COffset == 0x2C &&
    sinAcquire.Target == Target && sinAcquire.Value28 == 1 && sinAcquire.Value2C == 2);
Check("BroadcastWeaponTweaks nested reference layout",
    BroadcastWeaponTweaks.MessageId == 114 &&
    BroadcastWeaponTweaks.Value28Offset == 0x28 &&
    BroadcastWeaponTweaks.Value29Offset == 0x29 &&
    BroadcastWeaponTweaks.NestedSubobjectOffset == 0x2C &&
    BroadcastWeaponTweaks.ReferencedObjectOffset == 0x30 &&
    BroadcastWeaponTweaks.ReferenceControlBlockOffset == 0x34 &&
    BroadcastWeaponTweaks.Value38Offset == 0x38 &&
    BroadcastWeaponTweaks.NativeSubobjectVTableAddress == 0x1C80CC4 &&
    broadcastTweaks.Target == Target &&
    broadcastTweaks.Value28 == 3 && broadcastTweaks.Value29 == 4 &&
    broadcastTweaks.ReferencedObject == 5 &&
    broadcastTweaks.ReferenceControlBlock == 6 && broadcastTweaks.Value38 == 7);
Check("TempSlotAbilities default initialization",
    TempSlotAbilities.MessageId == 112 &&
    TempSlotAbilities.Value28Offset == 0x28 &&
    TempSlotAbilities.Value2COffset == 0x2C &&
    TempSlotAbilities.Value30Offset == 0x30 &&
    TempSlotAbilities.Value34Offset == 0x34 &&
    TempSlotAbilities.SentinelOffset == 0x38 &&
    tempSlotAbilities.Target == Target &&
    tempSlotAbilities.Value28 == 0 && tempSlotAbilities.Value2C == 0 &&
    tempSlotAbilities.Value30 == 0 && tempSlotAbilities.Value34 == 0 &&
    tempSlotAbilities.Sentinel == uint.MaxValue);
Check("CollectLoot positional DWORDs",
    CollectLoot.MessageId == 111 &&
    CollectLoot.Value28Offset == 0x28 &&
    CollectLoot.Value2COffset == 0x2C &&
    CollectLoot.Value30Offset == 0x30 &&
    CollectLoot.Value34Offset == 0x34 &&
    collectLoot.Target == Target && collectLoot.Value28 == 8 &&
    collectLoot.Value2C == 9 && collectLoot.Value30 == 10 && collectLoot.Value34 == 11);
Check("MapOpened kind-only layout",
    MapOpened.MessageId == 109 && mapOpened.Target == Target);
Check("BattleframePurchased kind-only layout",
    BattleframePurchased.MessageId == 110 && battleframePurchased.Target == Target);

var challengeStart = new ChallengeStartMatch(Target, 1, 2);
var challengePower = new ChallengeSetPowerPrivilege(Target, 3, 4, 5, 6, 7);
var challengeRole = new ChallengeSetRoleAndTeam(Target, 8, 9, 10, 11, 12, 13);
var challengeParameters = new ChallengeSetMatchParameters(Target, 14, 15, 16);
var challengeSwap = new ChallengeSwapTeam(Target, 17, 18, 19, 20, 21, 22);
Check("ChallengeStartMatch positional DWORDs",
    ChallengeStartMatch.MessageId == 106 &&
    ChallengeStartMatch.Value28Offset == 0x28 &&
    ChallengeStartMatch.Value2COffset == 0x2C &&
    challengeStart.Target == Target && challengeStart.Value28 == 1 && challengeStart.Value2C == 2);
Check("ChallengeSetPowerPrivilege positional fields",
    ChallengeSetPowerPrivilege.MessageId == 105 &&
    ChallengeSetPowerPrivilege.Value28Offset == 0x28 &&
    ChallengeSetPowerPrivilege.Value34Offset == 0x34 &&
    ChallengeSetPowerPrivilege.Value38Offset == 0x38 &&
    challengePower.Target == Target && challengePower.Value28 == 3 &&
    challengePower.Value2C == 4 && challengePower.Value30 == 5 &&
    challengePower.Value34 == 6 && challengePower.Value38 == 7);
Check("ChallengeSetRoleAndTeam positional fields",
    ChallengeSetRoleAndTeam.MessageId == 103 &&
    ChallengeSetRoleAndTeam.Value28Offset == 0x28 &&
    ChallengeSetRoleAndTeam.Value38Offset == 0x38 &&
    ChallengeSetRoleAndTeam.Value3COffset == 0x3C &&
    challengeRole.Target == Target && challengeRole.Value28 == 8 &&
    challengeRole.Value2C == 9 && challengeRole.Value30 == 10 &&
    challengeRole.Value34 == 11 && challengeRole.Value38 == 12 &&
    challengeRole.Value3C == 13);
Check("ChallengeSetMatchParameters positional DWORDs",
    ChallengeSetMatchParameters.MessageId == 104 &&
    ChallengeSetMatchParameters.Value28Offset == 0x28 &&
    ChallengeSetMatchParameters.Value2COffset == 0x2C &&
    ChallengeSetMatchParameters.Value30Offset == 0x30 &&
    challengeParameters.Target == Target && challengeParameters.Value28 == 14 &&
    challengeParameters.Value2C == 15 && challengeParameters.Value30 == 16);
Check("ChallengeSwapTeam positional DWORDs",
    ChallengeSwapTeam.MessageId == 102 &&
    ChallengeSwapTeam.Value28Offset == 0x28 &&
    ChallengeSwapTeam.Value3COffset == 0x3C &&
    challengeSwap.Target == Target && challengeSwap.Value28 == 17 &&
    challengeSwap.Value2C == 18 && challengeSwap.Value30 == 19 &&
    challengeSwap.Value34 == 20 && challengeSwap.Value38 == 21 &&
    challengeSwap.Value3C == 22);

var challengeReady = new ChallengeReadyCheck(Target, 1, 2);
var challengeKick = new ChallengeKick(Target, 3, 4, 5);
var challengeSetReady = new ChallengeSetReady(Target, 6, 7, 8);
var squadIdentifiers = new ulong[] { 0x1122334455667788, 0x8877665544332211 };
var challengeSquadInfo = new ChallengeInvitationSquadInfo(Target, 9, 10, 11, 12, 13, squadIdentifiers);
squadIdentifiers[0] = 0;
var challengeResponse = new ChallengeInvitationResponse(Target, 14, 15, 16);
var challengeLeave = new ChallengeLeave(Target, 17, 18);
var challengeInvitation = new ChallengeInvitation(Target, 19, 20, 21, 22, 23);
var challengeCreateIds = new ulong[] { 0x0102030405060708, 0x0807060504030201 };
var challengeCreate = new ChallengeCreate(Target, challengeCreateIds);
challengeCreateIds[0] = 0;
Check("ChallengeReadyCheck positional DWORDs",
    ChallengeReadyCheck.MessageId == 101 &&
    ChallengeReadyCheck.Value28Offset == 0x28 && ChallengeReadyCheck.Value2COffset == 0x2C &&
    challengeReady.Target == Target && challengeReady.Value28 == 1 && challengeReady.Value2C == 2);
Check("ChallengeKick fields and shared-text pointer",
    ChallengeKick.MessageId == 99 &&
    ChallengeKick.Value28Offset == 0x28 && ChallengeKick.Value2COffset == 0x2C &&
    ChallengeKick.SharedTextReferenceOffset == 0x30 &&
    challengeKick.Target == Target && challengeKick.Value28 == 3 &&
    challengeKick.Value2C == 4 && challengeKick.SharedTextReference == 5);
Check("ChallengeSetReady positional fields",
    ChallengeSetReady.MessageId == 100 &&
    ChallengeSetReady.Value28Offset == 0x28 && ChallengeSetReady.Value2COffset == 0x2C &&
    ChallengeSetReady.Value30Offset == 0x30 &&
    challengeSetReady.Target == Target && challengeSetReady.Value28 == 6 &&
    challengeSetReady.Value2C == 7 && challengeSetReady.Value30 == 8);
Check("ChallengeInvitationSquadInfo fields and copied identifiers",
    ChallengeInvitationSquadInfo.MessageId == 98 &&
    ChallengeInvitationSquadInfo.Value28Offset == 0x28 &&
    ChallengeInvitationSquadInfo.Value34Offset == 0x34 &&
    ChallengeInvitationSquadInfo.SharedTextReferenceOffset == 0x38 &&
    ChallengeInvitationSquadInfo.IdentifiersOffset == 0x3C &&
    ChallengeInvitationSquadInfo.IdentifierEntrySize == 8 &&
    challengeSquadInfo.Target == Target && challengeSquadInfo.Value28 == 9 &&
    challengeSquadInfo.Value2C == 10 && challengeSquadInfo.Value30 == 11 &&
    challengeSquadInfo.Value34 == 12 && challengeSquadInfo.SharedTextReference == 13 &&
    challengeSquadInfo.Identifiers.SequenceEqual([0x1122334455667788UL, 0x8877665544332211UL]));
Check("ChallengeInvitationResponse positional fields",
    ChallengeInvitationResponse.MessageId == 97 &&
    ChallengeInvitationResponse.Value28Offset == 0x28 &&
    ChallengeInvitationResponse.Value2COffset == 0x2C &&
    ChallengeInvitationResponse.Value30Offset == 0x30 &&
    challengeResponse.Target == Target && challengeResponse.Value28 == 14 &&
    challengeResponse.Value2C == 15 && challengeResponse.Value30 == 16);
Check("ChallengeLeave positional DWORDs",
    ChallengeLeave.MessageId == 95 &&
    ChallengeLeave.Value28Offset == 0x28 && ChallengeLeave.Value2COffset == 0x2C &&
    challengeLeave.Target == Target && challengeLeave.Value28 == 17 && challengeLeave.Value2C == 18);
Check("ChallengeInvitation fields and shared-text pointer",
    ChallengeInvitation.MessageId == 96 &&
    ChallengeInvitation.Value28Offset == 0x28 && ChallengeInvitation.Value2COffset == 0x2C &&
    ChallengeInvitation.SharedTextReferenceOffset == 0x30 &&
    ChallengeInvitation.Value34Offset == 0x34 && ChallengeInvitation.Value38Offset == 0x38 &&
    challengeInvitation.Target == Target && challengeInvitation.Value28 == 19 &&
    challengeInvitation.Value2C == 20 && challengeInvitation.SharedTextReference == 21 &&
    challengeInvitation.Value34 == 22 && challengeInvitation.Value38 == 23);
Check("ChallengeCreate copied identifiers",
    ChallengeCreate.MessageId == 94 && ChallengeCreate.IdentifiersOffset == 0x28 &&
    ChallengeCreate.IdentifierEntrySize == 8 && challengeCreate.Target == Target &&
    challengeCreate.Identifiers.SequenceEqual([0x0102030405060708UL, 0x0807060504030201UL]));

var pickupCarryable = new PickupCarryableObjectByProximity(Target, 0x1122334455667788);
var dropCarryable = new DropCarryableObject(Target, 0x8877665544332211);
Check("PickupCarryableObjectByProximity identifier layout",
    PickupCarryableObjectByProximity.MessageId == 189 &&
    PickupCarryableObjectByProximity.ObjectIdentifierOffset == 0x28 &&
    PickupCarryableObjectByProximity.ObjectIdentifierSize == 8 &&
    pickupCarryable.Target == Target && pickupCarryable.ObjectIdentifier == 0x1122334455667788);
Check("DropCarryableObject identifier layout",
    DropCarryableObject.MessageId == 190 &&
    DropCarryableObject.ObjectIdentifierOffset == 0x28 &&
    DropCarryableObject.ObjectIdentifierSize == 8 &&
    dropCarryable.Target == Target && dropCarryable.ObjectIdentifier == 0x8877665544332211);

var slotGear = new SlotGearRequest(Target, 1, 0x1122334455667788, 2, 3);
Check("SlotGearRequest native fields and unwritten gap",
    SlotGearRequest.MessageId == 206 &&
    SlotGearRequest.Value28Offset == 0x28 &&
    SlotGearRequest.UnwrittenGapOffset == 0x2C &&
    SlotGearRequest.UnwrittenGapSize == 4 &&
    SlotGearRequest.Value30Offset == 0x30 && SlotGearRequest.Value30Size == 8 &&
    SlotGearRequest.Value38Offset == 0x38 && SlotGearRequest.Value3COffset == 0x3C &&
    slotGear.Target == Target && slotGear.Value28 == 1 &&
    slotGear.Value30 == 0x1122334455667788 && slotGear.Value38 == 2 &&
    slotGear.Value3C == 3);

var lfgCheckin = new LFGCheckin(Target, 0x11223344, 0x55667788);
var lfgLeave = new LFGLeave(Target, 0x99AABBCC, 0xDDEEFF00);
Check("LFGCheckin positional fields",
    LFGCheckin.MessageId == 107 && LFGCheckin.Value28Offset == 0x28 &&
    LFGCheckin.Value2COffset == 0x2C && lfgCheckin.Target == Target &&
    lfgCheckin.Value28 == 0x11223344 && lfgCheckin.Value2C == 0x55667788);
Check("LFGLeave positional fields",
    LFGLeave.MessageId == 108 && LFGLeave.Value28Offset == 0x28 &&
    LFGLeave.Value2COffset == 0x2C && lfgLeave.Target == Target &&
    lfgLeave.Value28 == 0x99AABBCC && lfgLeave.Value2C == 0xDDEEFF00);

var joinSquadLeadersArc = new JoinSquadLeadersArc(Target);
var leaveEncounterParty = new LeaveEncounterParty(Target, 0x10203040, 0x50607080);
var resourceLocationInfos = new ResourceLocationInfosRequest(Target);
var chatPartyMembersBytes = Enumerable.Range(0, 32).Select(static value => (byte)value).ToArray();
var chatPartyMembers = new UpdateChatPartyMembers(Target, chatPartyMembersBytes, 0x11223344, 0x55667788);
chatPartyMembersBytes[0] = 0xFF;
Check("JoinSquadLeadersArc kind-only layout",
    JoinSquadLeadersArc.MessageId == 213 && joinSquadLeadersArc.Target == Target);
Check("LeaveEncounterParty positional fields",
    LeaveEncounterParty.MessageId == 212 &&
    LeaveEncounterParty.Value28Offset == 0x28 && LeaveEncounterParty.Value2COffset == 0x2C &&
    leaveEncounterParty.Target == Target && leaveEncounterParty.Value28 == 0x10203040 &&
    leaveEncounterParty.Value2C == 0x50607080);
Check("ResourceLocationInfosRequest kind-only layout",
    ResourceLocationInfosRequest.MessageId == 187 && resourceLocationInfos.Target == Target);
Check("UpdateChatPartyMembers copied opaque 16-byte records",
    UpdateChatPartyMembers.MessageId == 185 &&
    UpdateChatPartyMembers.MembersOffset == 0x28 &&
    UpdateChatPartyMembers.VectorElementSize == 16 &&
    UpdateChatPartyMembers.UnwrittenGapOffset == 0x34 &&
    UpdateChatPartyMembers.UnwrittenGapSize == 4 &&
    UpdateChatPartyMembers.Value38Offset == 0x38 &&
    UpdateChatPartyMembers.Value3COffset == 0x3C &&
    chatPartyMembers.Target == Target && chatPartyMembers.Members.Length == 32 &&
    chatPartyMembers.Members.Span[0] == 0 && chatPartyMembers.Value38 == 0x11223344 &&
    chatPartyMembers.Value3C == 0x55667788);
Check("UpdateChatPartyMembers rejects partial vector elements",
    ThrowsArgument(() => new UpdateChatPartyMembers(Target, new byte[17], 0, 0)));

var matchQueueFirst = new ulong[] { 0x1122334455667788, 0x8877665544332211 };
var matchQueueSecond = new uint[] { 1, 2, 3 };
var matchQueue = new MatchQueue(
    Target, matchQueueFirst, 0x1020304050607080, 4, 5, matchQueueSecond,
    6, 7, 8, 9, 10, 11, 12, 13);
matchQueueFirst[0] = 0;
matchQueueSecond[0] = 0;
Check("MatchQueue copied native vectors and positional fields",
    MatchQueue.MessageId == 90 &&
    MatchQueue.FirstVectorOffset == 0x28 && MatchQueue.FirstVectorElementSize == 8 &&
    MatchQueue.UnwrittenGapOffset == 0x34 && MatchQueue.UnwrittenGapSize == 4 &&
    MatchQueue.Value38Offset == 0x38 && MatchQueue.Value40Offset == 0x40 &&
    MatchQueue.Value44Offset == 0x44 &&
    MatchQueue.SecondVectorOffset == 0x48 && MatchQueue.SecondVectorElementSize == 4 &&
    MatchQueue.Value54Offset == 0x54 && MatchQueue.Value58Offset == 0x58 &&
    MatchQueue.Value5COffset == 0x5C && MatchQueue.Value60Offset == 0x60 &&
    MatchQueue.Value64Offset == 0x64 && MatchQueue.SharedText68Offset == 0x68 &&
    MatchQueue.Value6COffset == 0x6C && MatchQueue.SharedText70Offset == 0x70 &&
    matchQueue.Target == Target &&
    matchQueue.FirstVector.SequenceEqual([0x1122334455667788UL, 0x8877665544332211UL]) &&
    matchQueue.Value38 == 0x1020304050607080 && matchQueue.Value40 == 4 &&
    matchQueue.Value44 == 5 && matchQueue.SecondVector.SequenceEqual([1u, 2u, 3u]) &&
    matchQueue.Value54 == 6 && matchQueue.Value58 == 7 && matchQueue.Value5C == 8 &&
    matchQueue.Value60 == 9 && matchQueue.Value64 == 10 && matchQueue.SharedText68 == 11 &&
    matchQueue.Value6C == 12 && matchQueue.SharedText70 == 13);

var resetTutorial = new ResetTutorialId(Target, 7);
var dismissTutorial = new DismissTutorialId(Target, 14);
Check("ResetTutorialId native field and ID range",
    ResetTutorialId.MessageId == 220 && ResetTutorialId.TutorialIdOffset == 0x28 &&
    ResetTutorialId.MinimumTutorialId == 1 && ResetTutorialId.MaximumTutorialId == 14 &&
    resetTutorial.Target == Target && resetTutorial.TutorialId == 7);
Check("DismissTutorialId native field and ID range",
    DismissTutorialId.MessageId == 221 && DismissTutorialId.TutorialIdOffset == 0x28 &&
    DismissTutorialId.MinimumTutorialId == 1 && DismissTutorialId.MaximumTutorialId == 14 &&
    dismissTutorial.Target == Target && dismissTutorial.TutorialId == 14);

var beaconCalldown = new ResourceNodeBeaconCalldownRequest(Target, 1, 2, 3);
var hardpointSelection = new DeployableHardpointSelection(Target, 4, 5, 6);
Check("ResourceNodeBeaconCalldownRequest positional fields",
    ResourceNodeBeaconCalldownRequest.MessageId == 182 &&
    ResourceNodeBeaconCalldownRequest.Value28Offset == 0x28 &&
    ResourceNodeBeaconCalldownRequest.Value2COffset == 0x2C &&
    ResourceNodeBeaconCalldownRequest.Value30Offset == 0x30 &&
    beaconCalldown.Target == Target && beaconCalldown.Value28 == 1 &&
    beaconCalldown.Value2C == 2 && beaconCalldown.Value30 == 3);
Check("DeployableHardpointSelection positional fields",
    DeployableHardpointSelection.MessageId == 181 &&
    DeployableHardpointSelection.Value28Offset == 0x28 &&
    DeployableHardpointSelection.Value2COffset == 0x2C &&
    DeployableHardpointSelection.Value30Offset == 0x30 &&
    hardpointSelection.Target == Target && hardpointSelection.Value28 == 4 &&
    hardpointSelection.Value2C == 5 && hardpointSelection.Value30 == 6);

var cameraPose = new CameraPoseUpdate(
    Target,
    new CameraPoseUpdatePair(0x11223344, 0x55667788),
    new CameraPoseUpdateTriple(1, 2, 3));
Check("CameraPoseUpdate optional positional payloads",
    CameraPoseUpdate.MessageId == 177 &&
    CameraPoseUpdate.PairPresentOffset == 0x28 &&
    CameraPoseUpdate.PairOffset == 0x30 &&
    CameraPoseUpdate.TriplePresentOffset == 0x38 &&
    CameraPoseUpdate.TripleOffset == 0x3C &&
    cameraPose.Target == Target &&
    cameraPose.Pair == new CameraPoseUpdatePair(0x11223344, 0x55667788) &&
    cameraPose.Triple == new CameraPoseUpdateTriple(1, 2, 3));
var cameraPoseWithoutPayload = new CameraPoseUpdate(Target);
Check("CameraPoseUpdate common constructor has no optional payloads",
    cameraPoseWithoutPayload.Target == Target &&
    cameraPoseWithoutPayload.Pair is null &&
    cameraPoseWithoutPayload.Triple is null);

var aiSignal = new AiSignal(
    Target, 0x10101010, 0x20202020,
    new AiSignalOpaqueBlock(1, 2, 3, 4), 0x70707070, 0x74747474);
Check("AiSignal opaque fields and native offsets",
    AiSignal.MessageId == 192 &&
    AiSignal.FirstSharedTextReferenceOffset == 0x28 &&
    AiSignal.SecondSharedTextReferenceOffset == 0x2C &&
    AiSignal.BlockOffset == 0x30 &&
    AiSignal.Value70Offset == 0x70 &&
    AiSignal.Value74Offset == 0x74 &&
    aiSignal.Target == Target &&
    aiSignal.FirstSharedTextReference == 0x10101010 &&
    aiSignal.SecondSharedTextReference == 0x20202020 &&
    aiSignal.Block == new AiSignalOpaqueBlock(1, 2, 3, 4) &&
    aiSignal.Value70 == 0x70707070 && aiSignal.Value74 == 0x74747474);
var aiError = new AiError(Target, 0xA1B2C3D4);
Check("AiError shared-text field",
    AiError.MessageId == 191 &&
    AiError.SharedTextReferenceOffset == 0x28 &&
    aiError.Target == Target && aiError.SharedTextReference == 0xA1B2C3D4);
var longTimeWithoutPush = new LogLongTimeWithoutPush(Target);
Check("LogLongTimeWithoutPush kind-only layout",
    LogLongTimeWithoutPush.MessageId == 176 &&
    longTimeWithoutPush.Target == Target);
var geographicalReport = new GeographicalReportRequest(Target, 0xA5);
Check("GeographicalReportRequest positional byte",
    GeographicalReportRequest.MessageId == 184 &&
    GeographicalReportRequest.Value28Offset == 0x28 &&
    geographicalReport.Target == Target && geographicalReport.Value28 == 0xA5);
var nonDevDebug = new NonDevDebugCommand(Target, 0x12, 0x34567890, 0xAABBCCDD);
Check("NonDevDebugCommand native fields",
    NonDevDebugCommand.MessageId == 193 &&
    NonDevDebugCommand.Value28Offset == 0x28 &&
    NonDevDebugCommand.Value2COffset == 0x2C &&
    NonDevDebugCommand.SharedTextReferenceOffset == 0x30 &&
    nonDevDebug.Target == Target && nonDevDebug.Value28 == 0x12 &&
    nonDevDebug.Value2C == 0x34567890 &&
    nonDevDebug.SharedTextReference == 0xAABBCCDD);

Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) failed.");
return failures == 0 ? 0 : 1;

static bool Throws(Action action)
{
    try { action(); return false; }
    catch (ArgumentOutOfRangeException) { return true; }
}

static bool ThrowsArgument(Action action)
{
    try { action(); return false; }
    catch (ArgumentException) { return true; }
}
