using System.Buffers.Binary;
using Firefall.Reconstruction.Startup;

internal static class MatrixServer56To61Checks
{
    public static void Run(Action<string, bool> check)
    {
        var force = new byte[] { 56, 19, 2 }.Concat(Qword(0x0102030405060708))
            .Concat(Qword(0x01020304050607FE)).Concat(Word32(uint.MaxValue)).ToArray();
        var clock = new byte[] { 58 }.Concat(Qword(1)).Concat(Qword(2))
            .Concat(Qword(0x3FF0000000000000)).Concat(Qword(4)).Concat(Qword(5)).Concat(new byte[] { 0x80 }).ToArray();
        var ping = new byte[] { 59, 2 }.Concat(Qword(9)).Concat(Qword(9)).Concat(Word32(1)).ToArray();
        var textEntry = new byte[] { 0xFF, 0xFF, 0x80, 0xFF, 0 };
        var text = new byte[] { 60, 2 }.Concat(textEntry).Concat(textEntry).Concat(new byte[] { 0 }).ToArray();
        var nodeHeader = Enumerable.Range(1, 9).SelectMany(i => Word16((ushort)i)).ToArray();
        var leaf = nodeHeader.Concat(new byte[] { 0 }).ToArray();
        var nested = new byte[] { 61, 1, 0xFE, 0xFF, 2 }.Concat(leaf).Concat(leaf).ToArray();
        check("Server56/59 preserve QWORD duplicates and DWORD suffixes",
            MatrixServerMessages56To61Wire.Decode(force)
                is { BytesConsumed: 23, Payload: MatrixForceUnqueuePayload { ReasonCode: 19, WordAt38: uint.MaxValue } f }
                && f.InvolvedIds.SequenceEqual(new ulong[] { 0x0102030405060708, 0x01020304050607FE })
            && MatrixServerMessages56To61Wire.Decode(ping)
                is { BytesConsumed: 22, Payload: MatrixSuperPingPayload { WordAt34: 1 } p }
                && p.Ticks.SequenceEqual(new ulong[] { 9, 9 }));
        check("Server57 is a whole-remainder body, accepting empty/binary content without a length prefix",
            MatrixServerMessages56To61Wire.Decode(new byte[] { 57 })
                is { BytesConsumed: 1, Payload: MatrixRemainingBufferPayload { Bytes.Length: 0 } }
            && MatrixServerMessages56To61Wire.Decode(new byte[] { 57, 0xFF, 0, 0x80 })
                is { BytesConsumed: 4, Payload: MatrixRemainingBufferPayload b }
                && b.Bytes.Span.SequenceEqual(new byte[] { 0xFF, 0, 0x80 }));
        check("Server58 uses five raw QWORDs and a normalized nonzero byte, without native padding",
            MatrixServerMessages56To61Wire.Decode(clock)
                is { BytesConsumed: 42, Payload: MatrixClockUpdatePayload c }
                && c.Parameters == new MatrixClockParameters(1, 2, 0x3FF0000000000000, 4, 5, true));
        check("Server60 retains ordered duplicate WORD/raw-text pairs in two independent lists",
            MatrixServerMessages56To61Wire.Decode(text)
                is { Payload: MatrixWordTextListsPayload t }
                && t.ListAt28.Count == 2 && t.ListAt34.Count == 0
                && t.ListAt28.All(entry => entry.WordAt0 == ushort.MaxValue
                    && entry.TextBytesAt4.Span.SequenceEqual(new byte[] { 0x80, 0xFF })));
        check("Server61 root/native16 and recursive node/native32 widths exclude pointer/padding bytes",
            MatrixServerMessages56To61Wire.Decode(nested)
                is { BytesConsumed: 43, Payload: MatrixNestedWordRootsPayload n }
                && n.Roots.Count == 1 && n.Roots[0].WordAt0 == 0xFFFE
                && n.Roots[0].Children.Count == 2
                && n.Roots[0].Children.All(child => child.WordAt0 == 1 && child.Children.Count == 0
                    && child.WordsAt10To1E.SequenceEqual(Enumerable.Range(2, 8).Select(i => (ushort)i))));
        check("Server56-61 fixed/collection bodies reject every incomplete prefix and stop before trailing bytes",
            new[] { force, clock, ping, text, nested }.All(packet =>
                Enumerable.Range(0, packet.Length).All(length =>
                    MatrixServerMessages56To61Wire.Decode(packet.AsSpan(0, length)).Outcome != MatrixServer56To61ReadOutcome.Accepted)
                && MatrixServerMessages56To61Wire.Decode(packet.Concat(new byte[] { 0xAA }).ToArray()).BytesConsumed == packet.Length)
            && MatrixServerMessages56To61Wire.Decode(new byte[] { 0, 56, 0 }).Outcome == MatrixServer56To61ReadOutcome.NonCanonicalHeader
            && MatrixServerMessages56To61Wire.Decode(new byte[] { 62 }).Outcome == MatrixServer56To61ReadOutcome.UnexpectedMessageCode);
        check("Server56-61 minimum packets include required empty list counts",
            MatrixServerMessages56To61Wire.Decode(new byte[] { 56, 0, 0, 0, 0, 0, 0 }).BytesConsumed == 7
            && MatrixServerMessages56To61Wire.Decode(new byte[] { 59, 0, 0, 0, 0, 0 }).BytesConsumed == 6
            && MatrixServerMessages56To61Wire.Decode(new byte[] { 60, 0, 0 }).BytesConsumed == 3
            && MatrixServerMessages56To61Wire.Decode(new byte[] { 61, 0 }).BytesConsumed == 2);
        foreach (int count in new[] { 255, 256 })
        {
            var qwords = Chunked(Enumerable.Repeat(Qword(ulong.MaxValue), count));
            var qwordPacket = new byte[] { 56, 0 }.Concat(qwords).Concat(Word32(0)).ToArray();
            var textPacket = new byte[] { 60 }.Concat(Chunked(Enumerable.Repeat(textEntry, count)))
                .Concat(new byte[] { 0 }).ToArray();
            var nodePacket = new byte[] { 61, 1, 1, 0 }.Concat(Chunked(Enumerable.Repeat(leaf, count))).ToArray();
            var rootPacket = new byte[] { 61 }.Concat(Chunked(Enumerable.Repeat(new byte[] { 1, 0, 0 }, count))).ToArray();
            check($"Server56/60/61 chunk{count} preserves entries and exact continuation shape",
                MatrixServerMessages56To61Wire.Decode(qwordPacket)
                    is { Payload: MatrixForceUnqueuePayload q } && q.InvolvedIds.Count == count
                && MatrixServerMessages56To61Wire.Decode(textPacket)
                    is { Payload: MatrixWordTextListsPayload textLists } && textLists.ListAt28.Count == count
                && MatrixServerMessages56To61Wire.Decode(nodePacket)
                    is { Payload: MatrixNestedWordRootsPayload nodes } && nodes.Roots[0].Children.Count == count
                && MatrixServerMessages56To61Wire.Decode(rootPacket)
                    is { Payload: MatrixNestedWordRootsPayload roots } && roots.Roots.Count == count);
            if (count == 255)
                check("Server61 exact255 recursive/root records require terminal zero chunks",
                    MatrixServerMessages56To61Wire.Decode(nodePacket[..^1]).Outcome == MatrixServer56To61ReadOutcome.TruncatedPayload
                    && MatrixServerMessages56To61Wire.Decode(rootPacket[..^1]).Outcome == MatrixServer56To61ReadOutcome.TruncatedPayload);
        }
        var deep = new List<byte> { 61, 1, 1, 0 };
        for (int i = 0; i < 600; i++)
        {
            deep.Add(1);
            deep.AddRange(nodeHeader);
        }
        deep.Add(0);
        var deepResult = MatrixServerMessages56To61Wire.Decode(deep.ToArray());
        int depth = 0;
        if (deepResult.Payload is MatrixNestedWordRootsPayload deepRoots)
        {
            var children = deepRoots.Roots[0].Children;
            while (children.Count == 1)
            {
                depth++;
                children = children[0].Children;
            }
        }
        check("Server61 recursive collections decode600 levels without recursive CLR calls",
            deepResult.Outcome == MatrixServer56To61ReadOutcome.Accepted && deepResult.BytesConsumed == deep.Count && depth == 600);
        var expectedReasons = new[]
        {
            "Unknown", "DidNotAccept", "ServerError", "InChallenge", "NotEnoughPlayers",
            "TooManyPlayers", "InvalidTeamId", "LackingAccess", "TooManyPlayersPvP",
            "CannotQueueFromThisInstance", "InvalidQueues", "NotSquadLeader", "NotInSquad",
            "SquadDoesNotExist", "SquadReadyTimedOut", "InvalidLFGCategory", "TooManyQuitters",
            "InvalidZoneId", "MinimumLevelNotMet", "DeserterDebuff"
        };
        check("ForceUnqueue has20 exact reason strings;20..255 map to uppercase UNKNOWN",
            Enumerable.Range(0, 256).All(i =>
                MatrixForceUnqueueNativeLayout.GetReason((byte)i) == (i < 20 ? expectedReasons[i] : "UNKNOWN")));
        var forcePayload = new MatrixForceUnqueuePayload(19, new ulong[] { 0x123456789ABCDEF0 }, 8);
        check("ForceUnqueue runtime291 local_player comparison ignores only low8bits; zero upper56bits never matches",
            MatrixForceUnqueueNativeLayout.CaptureNotification(forcePayload, 0x123456789ABCDE01)
                == new MatrixForceUnqueueNotification("DeserterDebuff", true, 8)
            && !MatrixForceUnqueueNativeLayout.CaptureNotification(forcePayload, 0x123456789ABDDE01).LocalPlayer
            && !MatrixForceUnqueueNativeLayout.CaptureNotification(new(0, new ulong[] { 0, 1, 255 }, 0), null).LocalPlayer
            && !MatrixForceUnqueueNativeLayout.CaptureNotification(new(0, new ulong[] { 0, 1, 255 }, 0), 255).LocalPlayer);
        var paused = new MatrixClockParameters(10, 20, 0x3FF0000000000000, 30, 5, true);
        var set = MatrixClockUpdateNativeLayout.Select(false, new(true, false, 0, 5), paused);
        var repeated = MatrixClockUpdateNativeLayout.Select(true, new(true, true, 25, 5), paused);
        var released = MatrixClockUpdateNativeLayout.Select(true, new(true, true, 5, 0),
            paused with { HasUpperLimit = false, OffsetBitsAtB0 = ulong.MaxValue - 4 });
        check("Clock58 pause only sets a previously absent bound; repeated pause retains running bound",
            set.EmitPausedRuntime110 && set.SetRunningUpperLimit && set.RunningClock.UpperLimit == 30
            && !set.RebaseRunningReferenceAndScale && !set.OffsetAssertion
            && !repeated.EmitPausedRuntime110 && !repeated.SetRunningUpperLimit
            && repeated.RunningClock.UpperLimit == 25);
        check("Clock58 unpause rebases reference/scale with wrapping bound-minus-initial offset and separate assertions",
            released.EmitPausedRuntime110 && released.RebaseRunningReferenceAndScale
            && released.RebasedReferenceTick == 10 && released.RebasedScaleBits == paused.ScaleBitsAtA0
            && released.RunningClock.Offset == ulong.MaxValue - 4 && !released.OffsetAssertion
            && !released.RunningClock.HasUpperLimit
            && MatrixClockUpdateNativeLayout.Select(false, new(false, false, 0, 9), paused)
                is { InitializationAssertion: true, OffsetAssertion: true });
    }

    private static byte[] Word16(ushort value)
    {
        var bytes = new byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, value);
        return bytes;
    }

    private static byte[] Word32(uint value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        return bytes;
    }

    private static byte[] Qword(ulong value)
    {
        var bytes = new byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
        return bytes;
    }

    private static byte[] Chunked(IEnumerable<byte[]> entries)
    {
        var source = entries.ToArray();
        var bytes = new List<byte>();
        int position = 0, count;
        do
        {
            count = Math.Min(255, source.Length - position);
            bytes.Add((byte)count);
            for (int i = 0; i < count; i++)
                bytes.AddRange(source[position++]);
        }
        while (count == 255);
        return bytes.ToArray();
    }
}
