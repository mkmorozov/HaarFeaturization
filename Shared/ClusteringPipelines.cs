using Microsoft.Data.Analysis;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Shared;

public static class ClusteringPipelines
{
    public static (DataFrame labeledDataset, double score) KMeansCluster(
        this MLContext mlContext,
        DataFrame dataset,
        int numberOfClusters = 2)
    {
        var dataRaw = dataset.Clone();
        var pipeline = mlContext.Transforms
            .Concatenate(
                "UnscaledFeatures",
                dataRaw.Columns.Select(column => column.Name).ToArray())
            .Append(
                mlContext.Transforms.NormalizeMeanVariance(
                    "Features",
                    inputColumnName: "UnscaledFeatures"))
            .Append(
                mlContext.Clustering.Trainers.KMeans(numberOfClusters: numberOfClusters));

        var model = pipeline.Fit(dataRaw);
        var clusteringData = model.Transform(dataRaw);
        var labels = clusteringData.GetColumn<uint>("PredictedLabel").ToList();
        var clusteringMetrics = mlContext.Clustering.Evaluate(
            clusteringData,
            labelColumnName: "PredictedLabel",
            scoreColumnName: "Score",
            featureColumnName: "Features");

        dataRaw.Columns.Add(
            new PrimitiveDataFrameColumn<uint>(
                "label",
                clusteringData.GetColumn<uint>("PredictedLabel")));
        return (dataRaw, clusteringMetrics.DaviesBouldinIndex);
    }

    public static (DataFrame labeledDataset, double score) OptimalKMeansCluster(
        this MLContext mlContext,
        DataFrame dataset,
        int numberOfClusters = 2,
        int numberOfAttempts = 10)
    {
        var results = new List<(DataFrame, double)>();
        for (var i = 0; i < numberOfAttempts; i++)
            results.Add(mlContext.KMeansCluster(
                dataset, 
                numberOfClusters: numberOfClusters));

        return results.OrderBy(result => result.Item2).First();
    }

    public static async Task<(DataFrame labeledDataset, double score)> OptimalKMeansClusterAsync(
        DataFrame dataset,
        int numberOfClusters = 2,
        int numberOfAttempts = 10)
    {
        var attempts = new List<Task<(DataFrame, double)>>();
        for (var i = 0; i < numberOfAttempts; i++)
            attempts.Add(Task.Run(
                () => {
                    var mlContext = new MLContext();
                    return mlContext.KMeansCluster(
                        dataset,
                        numberOfClusters: numberOfClusters);
                }));

        var results = await Task.WhenAll(attempts).ConfigureAwait(false);
        return results.OrderBy(result => result.Item2).First();
    }

    public static List<uint> SortLabelsByClusterAmplitude(
        this IEnumerable<uint> labels,
        List<double> samples,
        int blockSize)
    {
        var amplitudesArray = samples.Select(Math.Abs).ToArray();
        var clusterTotals = new Dictionary<uint, (double, int)>();
        var blockNumber = 0;
        foreach (var label in labels)
        {
            var currentBlock = amplitudesArray[(blockNumber * blockSize)..((blockNumber + 1) * blockSize)];
            if (clusterTotals.TryGetValue(label, out var currentTotals))
                clusterTotals[label] = (
                    currentTotals.Item1 + currentBlock.Sum(),
                    currentTotals.Item2 + currentBlock.Length);
            else
                clusterTotals[label] = (currentBlock.Sum(), currentBlock.Length);
            blockNumber++;
        }

        var labelMapping = clusterTotals
            .Select(x => (x.Key, x.Value.Item1 / x.Value.Item2))
            .OrderBy(x => x.Item2)
            .Select((x, i) => (x.Item1, Convert.ToUInt32(i)))
            .ToDictionary(x => x.Item1, x => x.Item2);
        return labels
            .Select(x => labelMapping[x])
            .ToList();
    }
}
