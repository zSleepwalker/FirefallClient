using System.Buffers.Binary;
using Firefall.Reconstruction.Startup;

internal static class NativeGeneratedConditionalBranchChecks
{
    public static void Run(Action<string, bool> check)
    {
        bool positiveRanges = true;
        for (int offset = 0; offset <= 256; offset++)
        {
            var result = NativeGeneratedConditionalBranches.ProjectShapedBranch(NativeGeneratedOperationCode.JumpZero, offset);
            bool shortAccepted = offset <= 255 && offset is not (128 or 129);
            positiveRanges &= result.Candidates.Count == (shortAccepted ? 2 : 1)
                && result.NativeDiagnosticCode is null
                && BinaryPrimitives.ReadInt32LittleEndian(result.Candidates[^1].Span[2..]) == offset;
        }
        check("Native JZ positive offset gates retain both short/near candidates except128/129 and above255", positiveRanges);
        var wrap = NativeGeneratedConditionalBranches.ProjectShapedBranch(NativeGeneratedOperationCode.JumpZero, 180);
        check("Native accepted short JZ offset180 is byte B4, not repaired to a forward signed displacement",
            Convert.ToHexString(wrap.Candidates[0].Span) == "74B4"
            && Convert.ToHexString(wrap.Candidates[1].Span) == "0F84B4000000");
        var backward = NativeGeneratedConditionalBranches.ProjectShapedBranch(NativeGeneratedOperationCode.JumpNotZero, -100);
        check("Native negative JNZ matches near only and subtracts its own six-byte length",
            backward.Candidates.Count == 1 && Convert.ToHexString(backward.Candidates[0].Span) == "0F8596FFFFFF");
        bool lowBounds = true;
        for (int delta = 0; delta <= 6; delta++)
        {
            var result = NativeGeneratedConditionalBranches.ProjectShapedBranch(NativeGeneratedOperationCode.JumpNotZero, int.MinValue + delta);
            lowBounds &= delta < 6
                ? result.Candidates.Count == 0 && result.NativeDiagnosticCode == 1040188017u
                : result.Candidates.Count == 1 && result.NativeDiagnosticCode is null
                    && BinaryPrimitives.ReadInt32LittleEndian(result.Candidates[0].Span[2..]) == int.MinValue;
        }
        check("Native near jump rejects Int32 minimum through minimum+5 before negative length adjustment", lowBounds);
    }
}
