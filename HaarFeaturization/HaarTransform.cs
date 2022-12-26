namespace HaarFeaturization;

public static class HaarTransform
{
    public static List<List<double>> HaarTransfom(this IEnumerable<double> signal)
    {
        var (diffs, sums) = ComputeScale(signal);
        var scales = new List<List<double>>();
        scales.Add(diffs);
        while (diffs.Count > 1)
        {
            (diffs, sums) = ComputeScale(sums);
            scales.Add(diffs);
        }
        scales.Add(sums);
        
        return scales;
    }

    public static List<double> InverseHaarTransform(this List<List<double>> scales)
    {
        var signal = new List<double>();
        for (var i = scales.Count - 1; i >=0; i--)
            if (signal.Count == 0)
                signal = scales[i];
            else
                signal = RestoreScale(scales[i], signal);

        return signal;
    }

    private static (List<double> Diffs, List<double> Sums) ComputeScale(IEnumerable<double> signal)
    {
        var (diffs, sums) = (new List<double>(), new List<double>());
        var previous = double.NaN;
        foreach(var sample in signal) 
            if (previous is double.NaN)
                previous = sample;
            else
            { 
                diffs.Add(previous - sample);
                sums.Add((previous + sample) / 2);
                previous = double.NaN;
            }
        
        return (diffs, sums);
    }

    private static List<double> RestoreScale(List<double> diffs, List<double> sums)
    {
        if (diffs.Count != sums.Count)
            throw new ArgumentException("Inputs must have the same length.");

        var signal = new List<double>(); 
        foreach (var (diff, sum) in diffs.Zip(sums))
        {
            signal.Add(sum + diff / 2);
            signal.Add(sum - diff / 2);
        }

        return signal;
    }
}