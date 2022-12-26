using HaarFeaturization;
using Microsoft.Data.Analysis;
using Microsoft.ML;
using Serilog;
using Shared;

namespace Demo1_SyntheticSignalsClustering;

internal static class TestCaseRunner
{
    public static void RunTestCase(
        this MLContext mlContext, 
        string caseName, 
        List<List<double>> signals, 
        int numberOfClusters,
        int numberOfAttempts = 10,
        int plotsPerRow = 4)
    {
        var picturesFolder = Path.Combine(Configuration.ResultsPath(), "Demo1");
        var dataset = signals.HaarFeaturize(new DefaultFeaturizer(4));
        var result = mlContext.OptimalKMeansCluster(dataset, numberOfClusters, numberOfAttempts);

        Log.Information(
            "{case}\n"
                + "Labels       : {labels}\n"
                + "Score        : {score}\n"
                + "Top features : {features}\n",
            caseName, 
            result.labeledDataset["label"].Cast<uint>(), 
            result.score,
            result.labeledDataset.ClusterSeparation().Select(x => x.featureName).Take(5));

        DataFrame.SaveCsv(result.labeledDataset, Path.Combine(picturesFolder, $"{caseName}_features.csv"));
        signals.ToPng(
            Path.Combine(picturesFolder, $"{caseName}_dataset.png"),
            result.labeledDataset["label"]
                .Cast<uint>()
                .Select(x => $"Cluster {x}")
                .ToList(),
            plotsPerRow: plotsPerRow);
    }
}
