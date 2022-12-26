using Microsoft.Data.Analysis;

namespace HaarFeaturization;

public static class HaarFeaturization
{
    public static SortedDictionary<string, double> HaarFeaturize(this IEnumerable<double> signal, IFeaturizer? featurizer = null)
    { 
        var featurizerActual = featurizer ?? new DefaultFeaturizer();
        var features = featurizerActual.Postprocess(signal.HaarTransfom().Select(featurizerActual.Featurize).ToList());
        return new(features
            .SelectMany(
                (scaleFeatures, scaleNumber) => scaleNumber == features.Count - 1
                    ? scaleFeatures.AddFeaturePrefix("signal")
                    : scaleFeatures.AddFeaturePrefix($"scale{scaleNumber}"))
            .ToDictionary(x => x.Key, x=> x.Value));
    }

    public static DataFrame HaarFeaturize(this IEnumerable<IEnumerable<double>> signals, IFeaturizer? featurizer = null)
    { 
        var features = signals.Select(signal => signal.HaarFeaturize(featurizer)).ToList();
        var columnsNumber = features.Select(x => x.Count).Min();
        var dataFrame = new DataFrame(features
            .First(x => x.Count == columnsNumber)
            .Select(x => new PrimitiveDataFrameColumn<float>(x.Key)));

        dataFrame.AppendExisting(features);
        return dataFrame;
    }

    public static DataFrame HaarFeaturize(this IEnumerable<double> signal, int blockSize, IFeaturizer? featurizer = null)
        => signal.SplitToBlocks(blockSize).HaarFeaturize(featurizer: featurizer);

    private static IEnumerable<(string Key, double Value)> AddFeaturePrefix(
        this SortedDictionary<string, double> scaleFeatues, 
        string prefix)
        => scaleFeatues.Select(feature => ($"{prefix}_{feature.Key}", feature.Value));

    private static List<List<double>> SplitToBlocks(this IEnumerable<double> signal, int blockSize)
    {
        var result = new List<List<double>>();
        var currentBlock = new List<double>();
        foreach (var sample in signal)
        {
            currentBlock.Add(sample);
            if (currentBlock.Count == blockSize)
            {
                result.Add(currentBlock);
                currentBlock = new List<double>();
            }
        }

        return result;
    }
}
