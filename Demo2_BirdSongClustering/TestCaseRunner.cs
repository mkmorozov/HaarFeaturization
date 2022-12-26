using HaarFeaturization;
using Microsoft.Data.Analysis;
using Serilog;
using Shared;
using System.Drawing;

namespace Demo2_BirdSongClustering;

internal static class TestCaseRunner
{
    public static async Task Run(
        string caseName,
        string audioFilePath,
        string iconFilePath,
        int blockSize = 512,
        int numberOfAttempts = 20,
        int numberOfSamplesToPlot = 100000,
        IFeaturizer? featurizer = null)
    {
        Log.Information(
            "Started: {case}, {blockSize} block size, {attempts} attempts",
            caseName,
            blockSize,
            numberOfAttempts);

        var picturesFolder = Path.Combine(Configuration.ResultsPath(), "Demo2");
        var samples = AudioStreamer.FileAsMono(audioFilePath).ToList();
        using var icon = (Bitmap)Image.FromFile(iconFilePath);

        var dataset = samples.HaarFeaturize(blockSize, featurizer: featurizer);
        var result = await ClusteringPipelines.OptimalKMeansClusterAsync(
            dataset, 
            numberOfAttempts: numberOfAttempts)
            .ConfigureAwait(false);

        Log.Information(
            "Done\n"
                + "Best score   : {score}\n"
                + "Top features : {features}\n",
            result.score,
            result.labeledDataset
                .ClusterSeparation()
                .Select(x => $"{x.featureName} {x.separationScore:f2}")
                .Take(5));

        DataFrame.SaveCsv(
            result.labeledDataset,
            Path.Combine(picturesFolder, $"{caseName}_features.csv"));

        DataFrame.SaveCsv(
            result.labeledDataset.GroupBy("label").Mean(),
            Path.Combine(picturesFolder, $"{caseName}_separation.csv"));

        samples.Take(numberOfSamplesToPlot).ToList().ToPng(
            Path.Combine(picturesFolder, $"{caseName}_clusters.png"),
            result.labeledDataset["label"]
                .Cast<uint>()
                .SortLabelsByClusterAmplitude(samples, blockSize)
                .Select((x, i) => (
                    i * blockSize,
                    (i + 1) * blockSize,
                    x == 1 ? Color.Blue : Color.Transparent))
                .ToList(),
        plotWidth: 1200,
        plotHeight: 400,
        icon: icon);
    }
}
