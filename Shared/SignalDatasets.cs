namespace Shared;

public static class SignalDatasets
{
    public static List<List<double>> AAndB(
        int length,
        SignalType signalTypeA,
        int periodA,
        SignalType signalTypeB,
        int periodB)
    {
        var signalA = SignalProvider.Generate(signalTypeA, length, period: periodA);
        var signalB = SignalProvider.Generate(signalTypeB, length, period: periodB);
        var result = new List<List<double>> {
            signalA.Add(signalB).Normalize(),
            signalA.ReverseCopy().Add(signalB).Normalize(),
            signalA.Add(signalB.ReverseCopy()).Normalize(),
            signalA.Add(signalB).ReverseCopy().Normalize()};
        return result;
    }

    public static List<List<double>> AThenB(
        int length,
        SignalType signalTypeA,
        int periodA,
        SignalType signalTypeB, 
        int periodB)
    {
        var partLength = length / 2;
        var signalA = SignalProvider.Generate(signalTypeA, partLength, period: periodA);
        var signalB = SignalProvider.Generate(signalTypeB, partLength, period: periodB);
        var result = new List<List<double>> {
            signalA.Concat(signalB).ToList(),
            signalB.Concat(signalA).ToList(),
            signalA.ReverseCopy().Concat(signalB).ToList(),
            signalB.ReverseCopy().Concat(signalA).ToList(),
            signalA.Concat(signalB.ReverseCopy()).ToList(),
            signalB.Concat(signalA.ReverseCopy()).ToList(),
            signalA.Concat(signalB).ToList().ReverseCopy(),
            signalB.Concat(signalA).ToList().ReverseCopy()};
        return result;
    }
}
