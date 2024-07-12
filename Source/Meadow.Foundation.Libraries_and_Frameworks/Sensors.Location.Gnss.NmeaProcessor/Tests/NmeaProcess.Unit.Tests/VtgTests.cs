namespace NmeaProcess.Unit.Tests;

public class VtgTests
{
    [Fact]
    public void TestGPVTG()
    {
        var testSentences = new string[]
        {
            "$GPVTG, 309.62, T, ,M, 0.13, N, 0.2, K, A*23"
        };

        foreach (var sentence in testSentences)
        {
        }
    }
}
