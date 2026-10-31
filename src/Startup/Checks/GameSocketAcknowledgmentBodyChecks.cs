using Firefall.Reconstruction.Startup;

internal static class GameSocketAcknowledgmentBodyChecks
{
    public static void Run(Action<string, bool> check)
    {
        byte[] words =
        [
            0x12, 0x34,
            0xAB, 0xCD,
            0x00, 0x07,
            0xFE, 0xDC
        ];
        bool decoded = GameSocketAcknowledgmentBody.TryDecode(
            GameSocketControlOpcode.AcknowledgeChannel1, words, out var ack, out var error);
        check("GameSocket channel-1 ACK decodes two frontier words and selective sequence words in network order",
            decoded
            && error == GameSocketAcknowledgmentDecodeError.None
            && ack is
            {
                Opcode: GameSocketControlOpcode.AcknowledgeChannel1,
                PeerWindowSequence: 0x1234,
                CumulativeAcknowledgmentSequence: 0xABCD
            }
            && ack.SelectiveAcknowledgmentSequences.SequenceEqual(new ushort[] { 0x0007, 0xFEDC }));

        bool channel2Decoded = GameSocketAcknowledgmentBody.TryDecode(
            GameSocketControlOpcode.AcknowledgeChannel2,
            words.AsSpan(0, 4),
            out var channel2Ack,
            out _);
        check("GameSocket channel-2 ACK maps to its own opcode with no selective words",
            channel2Decoded
            && channel2Ack is
            {
                Opcode: GameSocketControlOpcode.AcknowledgeChannel2,
                PeerWindowSequence: 0x1234,
                CumulativeAcknowledgmentSequence: 0xABCD
            }
            && channel2Ack.SelectiveAcknowledgmentSequences.Count == 0);

        bool unsupportedOpcode = !GameSocketAcknowledgmentBody.TryDecode(
            GameSocketControlOpcode.ClockQuery, [], out var invalidOpcodeAck, out var invalidOpcodeError)
            && invalidOpcodeAck is null
            && invalidOpcodeError == GameSocketAcknowledgmentDecodeError.UnsupportedOpcode;
        bool tooFewWords = !GameSocketAcknowledgmentBody.TryDecode(
            GameSocketControlOpcode.AcknowledgeChannel1, [0x00, 0x01, 0x00],
            out var shortAck, out var shortError)
            && shortAck is null
            && shortError == GameSocketAcknowledgmentDecodeError.TooFewWords;
        bool oddByteCount = !GameSocketAcknowledgmentBody.TryDecode(
            GameSocketControlOpcode.AcknowledgeChannel1, [0, 1, 0, 2, 0],
            out var oddAck, out var oddError)
            && oddAck is null
            && oddError == GameSocketAcknowledgmentDecodeError.OddWordByteCount;
        bool maximumAccepted = GameSocketAcknowledgmentBody.TryDecode(
            GameSocketControlOpcode.AcknowledgeChannel1,
            new byte[GameSocketAcknowledgmentBody.MaximumWordCount * sizeof(ushort)],
            out var maximumAck,
            out _)
            && maximumAck?.SelectiveAcknowledgmentSequences.Count
                == GameSocketAcknowledgmentBody.MaximumWordCount - GameSocketAcknowledgmentBody.RequiredFrontierWordCount;
        bool tooManyWords = !GameSocketAcknowledgmentBody.TryDecode(
            GameSocketControlOpcode.AcknowledgeChannel1,
            new byte[(GameSocketAcknowledgmentBody.MaximumWordCount + 1) * sizeof(ushort)],
            out var longAck,
            out var longError)
            && longAck is null
            && longError == GameSocketAcknowledgmentDecodeError.TooManyWords;
        check("GameSocket ACK decoder rejects unsupported, truncated, odd-length, and oversized word lists explicitly",
            unsupportedOpcode && tooFewWords && oddByteCount && maximumAccepted && tooManyWords);

        check("GameSocket ACK model pins native handlers, queue frontiers, and chunk-derived bound",
            GameSocketAcknowledgmentBody.DeferredControlMethodFileOffset == 0xBD3760
            && GameSocketAcknowledgmentBody.NativeHandlerFileOffset == 0xBE43B0
            && GameSocketAcknowledgmentBody.RequiredFrontierWordCount == 2
            && GameSocketAcknowledgmentBody.MaximumWordCount == 694
            && GameSocketReliableQueueNativeLayout.PeerWindowSequenceOffset == 0x2C
            && GameSocketReliableQueueNativeLayout.CumulativeAcknowledgmentSequenceOffset == 0x2E);

        check("GameSocket ACK emitter records receive frontiers, selective-ring layout, and one-byte chunk bound",
            GameSocketReliableQueueNativeLayout.DatagramAssemblerMethodFileOffset == 0xBD4340
            && GameSocketReliableQueueNativeLayout.OutgoingAcknowledgmentBuilderFileOffset == 0xBE4F20
            && GameSocketReliableQueueNativeLayout.NextReceiveSequenceOffset == 0x2A
            && GameSocketReliableQueueNativeLayout.LocalCumulativeAcknowledgmentSequenceOffset == 0x30
            && GameSocketReliableQueueNativeLayout.AcknowledgmentPendingFlagOffset == 0x32
            && GameSocketReliableQueueNativeLayout.PendingReceivedSequenceTrackerOffset == 0x47C
            && GameSocketReliableQueueNativeLayout.PendingReceivedSequenceTrackerCursorOffset == 0x488
            && GameSocketReliableQueueNativeLayout.PendingReceivedSequenceTrackerCountOffset == 0x48C
            && GameSocketReliableQueueNativeLayout.SelectiveAcknowledgmentBitmapOffset == 0x490
            && GameSocketReliableQueueNativeLayout.SelectiveAcknowledgmentBitmapHeadOffset == 0x590
            && GameSocketReliableQueueNativeLayout.EmittedAcknowledgmentBaseChunkLength == 7
            && GameSocketReliableQueueNativeLayout.MaximumEmittedAcknowledgmentChunkLength == 0xFF
            && GameSocketReliableQueueNativeLayout.MaximumEmittedSelectiveAcknowledgmentCount == 124);
    }
}
