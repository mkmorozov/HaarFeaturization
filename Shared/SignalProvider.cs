namespace Shared;

public enum SignalType
{
    Const,
    Harmonic,
    Saw,
    Square,
    Chirp
}

public static class SignalProvider
{
    public static List<double> Generate(SignalType signalType, int length, int period = 0)
    {
        var periodActual = period == 0 ? length : period;
        var range = Enumerable.Range(0, length);
        switch (signalType)
        {
            case SignalType.Const:
                return range.Select(x => 1d).ToList();

            case SignalType.Harmonic:
                return range.Select(x => Math.Sin(2 * Math.PI * x / periodActual)).ToList();

            case SignalType.Saw:
                return range.Select(x => (double)(x % periodActual)).ToList().Normalize();

            case SignalType.Square:
                var halfPeriod = periodActual / 2;
                return range.Select(x => (x % periodActual) >= halfPeriod ? 1d : 0d).ToList();

            case SignalType.Chirp:
                return range.Select((x, i)
                    => Math.Sin(2 * Math.PI * i * x / (length * periodActual))).ToList();

            default:
                return new List<double>();
        }
    }

    public static List<double> Add(this List<double> signal, List<double> otherSignal)
    {
        if (signal.Count != otherSignal.Count)
            throw new ArgumentException("Signals must have the same length");

        return signal.Zip(otherSignal).Select(x => x.First + x.Second).ToList();
    }

    public static List<double> Multiply(this List<double> signal, List<double> otherSignal)
    { 
        if (signal.Count != otherSignal.Count)
            throw new ArgumentException("Signals must have the same length");

        return signal.Zip(otherSignal).Select(x => x.First * x.Second).ToList();
    }

    public static List<double> Normalize(this List<double> signal, double amplitude = 1)
    {
        if (signal.Count > 0)
        {
            var max = signal.Select(Math.Abs).Max();
            if (max != 0d && amplitude > 0d)
                return signal.Select(x => amplitude * x / max).ToList();
        }

        return signal;
    }

    public static List<double> ReverseCopy(this List<double> signal)
    {
        var result = new List<double>(signal);
        result.Reverse();
        return result;
    }

}