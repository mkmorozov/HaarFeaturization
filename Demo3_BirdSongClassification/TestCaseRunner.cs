using HaarFeaturization;
using Microsoft.Data.Analysis;
using Microsoft.ML;
using Microsoft.ML.Data;
using Serilog;
using Shared;
using System.Drawing;

namespace Demo3_BirdSongClassification;

internal static class TestCaseRunner
{
    public static async Task<DataFrame> ClusterizeBirdSongFile(
        string caseName,
        string filePath,
        string picturesFolder,
        int blockSize = 256,
        int numberOfAttempts = 40,
        Bitmap? icon = null)
    {
        Log.Information(
            "Clusterizing {file}, {blockSize} block size, {attempts} attempts",
            filePath,
            blockSize,
            numberOfAttempts);

        var samples = AudioStreamer.FileAsMono(filePath).ToList();
        var dataset = samples.HaarFeaturize(blockSize);
        var result = await ClusteringPipelines.OptimalKMeansClusterAsync(
            dataset,
            numberOfAttempts: numberOfAttempts)
            .ConfigureAwait(false);

        var topFeatures = result.labeledDataset
            .ClusterSeparation()
            .Select(x => $"{x.featureName} {x.separationScore:f2}")
            .Take(5)
            .ToList();
        result.labeledDataset["label"] = new PrimitiveDataFrameColumn<bool>(
            "label",
            result.labeledDataset["label"]
                .Cast<uint>()
                .SortLabelsByClusterAmplitude(samples, blockSize)
                .Select(x => x == 1));

        Log.Information(
            "Done clusterizing {file}\n"
                + "Best score   : {score}\n"
                + "Blocks in 1  : {label1}\n"
                + "Blocks in 2  : {label2}\n"
                + "Top features : {features}\n",
            filePath,
            result.score,
            result.labeledDataset["label"].Cast<bool>().Count(x => x),
            result.labeledDataset["label"].Cast<bool>().Count(x => !x),
            topFeatures);

        DataFrame.SaveCsv(
            result.labeledDataset,
            Path.Combine(picturesFolder, $"training_clusters_features_{caseName}.csv"));

        samples.ToPng(
            Path.Combine(picturesFolder, $"training_clusters_{caseName}.png"),
            result.labeledDataset["label"]
                .Cast<bool>()
                .Select((x, i) => (
                    i * blockSize,
                    (i + 1) * blockSize,
                    x ? Color.Blue : Color.Transparent))
                .ToList(),
        plotWidth: 1200,
        plotHeight: 400,
        icon: icon);

        return result.labeledDataset;
    }

    public static void ClassifyBirdSongFile(
        string caseName,
        string filePath,
        string picturesFolder,
        ITransformer classificationModel,
        int blockSize = 256,
        int numberOfAttempts = 40,
        Bitmap? icon = null)
    {
        Log.Information(
            "Classifying {file}, {blockSize} block size",
            filePath,
            blockSize);
        var testSamples = AudioStreamer.FileAsMono(filePath).ToList();
        var testDataset = testSamples.HaarFeaturize(blockSize);
        var predictions = classificationModel.Transform(testDataset);
        testSamples.ToPng(
            Path.Combine(picturesFolder, $"{caseName}_classified.png"),
            predictions
                .GetColumn<bool>("PredictedLabel")
                .Select((x, i) => (
                    i * blockSize,
                    (i + 1) * blockSize,
                    x ? Color.Blue : Color.Transparent))
                .ToList(),
            plotWidth: 1200,
            plotHeight: 400,
            icon: icon);
    }
}
