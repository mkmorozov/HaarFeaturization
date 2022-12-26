namespace HaarFeaturization;

public class DefaultFeaturizer : IFeaturizer
{
    private int HigherMomentsThreshold;
    private List<int> ScalesToDrop;

    public DefaultFeaturizer(
        int higherMomentsThreshold = 32, 
        List<int>? scalesToDrop = null)
        => (HigherMomentsThreshold, ScalesToDrop) = (
            Math.Max(4, higherMomentsThreshold), 
            scalesToDrop ?? new List<int>());

    public SortedDictionary<string, double> Featurize(List<double> signal)
    {
        var result = new SortedDictionary<string, double>();
        result["mean"] = signal.Select(Math.Abs).Average(); // note the abs.

        if (signal.Count > 1)
        {
            var mean = signal.Average();
            var std = Math.Sqrt(signal.Average(x => Math.Pow(x - mean, 2)));
            result["std"] = std;

            if (signal.Count >= HigherMomentsThreshold)
            {
                result["skewness"] = std > 0
                    ? Math.Abs(signal.Average(x => Math.Pow(x - mean, 3)) / Math.Pow(std, 3))
                    : 0;

                result["kurtosis"] = std > 0
                    ? signal.Average(x => Math.Pow(x - mean, 4)) / Math.Pow(std, 4) - 3
                    : 0;
            }
        }

        return result;
    }

    public List<SortedDictionary<string, double>> Postprocess(List<SortedDictionary<string, double>> features)
    {
        if (!features.Any())
            return features;

        var featureKeys = features[0].Select(feature => feature.Key).ToList();
        var filteredFeatures = FilterScales(features);

        // As wavelet transform is hierarhical by design,
        // we normalize all features except the signal mean value.
        var normalizeValues = featureKeys.ToDictionary(
            featureKey => featureKey, 
            featureKey => filteredFeatures
                .Where(x => x.ContainsKey(featureKey))
                .Select(x => Math.Abs(x[featureKey]))
                .DefaultIfEmpty(1)
                .Max());

        return filteredFeatures
            .Select((scaleFeatures, scaleNumber) 
                => scaleNumber == features.Count - 1
                    ? scaleFeatures // don't normalize the mean value.
                    : NormalizeScale(scaleFeatures, normalizeValues))
            .ToList();
    }

    private List<SortedDictionary<string, double>> FilterScales(List<SortedDictionary<string, double>> features)
        => features
            .Select((scaleFeatures, scaleNumber)
                => ScalesToDrop.Contains(scaleNumber)
                    ? new SortedDictionary<string, double>()
                    : scaleFeatures)
            .ToList();

    private static SortedDictionary<string, double> NormalizeScale(
        SortedDictionary<string, double> scaleFeatures, 
        Dictionary<string, double> normalizeValues)
        => new(scaleFeatures.ToDictionary(
            x => x.Key, 
            x => normalizeValues.TryGetValue(x.Key, out var normalizeValue) && normalizeValue > 0d 
                ? x.Value / normalizeValue
                : x.Value));
}
