using Demo3_BirdSongClassification;
using Microsoft.Data.Analysis;
using Microsoft.ML;
using Serilog;
using Shared;
using System.Data;
using System.Drawing;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/Demo3_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var blockSize = 256;
    var picturesFolder = Path.Combine(Configuration.ResultsPath(), "Demo3");
    var icon = (Bitmap)Image.FromFile(@"data/Elegant_Pitta.jpg");
    var trainAudioFiles = new List<string> {
        @"data/XC161623 - Elegant Pitta - Pitta elegans vigorsii.mp3",
        @"data/XC166727 - Elegant Pitta - Pitta elegans concinna.mp3",
        @"data/XC166728 - Elegant Pitta - Pitta elegans concinna.mp3"};

    // Dataset preparation:
    Log.Information("Preparing trainig set...");
    var featurizedDatasets = await Task.WhenAll(trainAudioFiles
        .Select((trainAudioFile, i) => TestCaseRunner.ClusterizeBirdSongFile(
            $"{i}",
            trainAudioFile,
            picturesFolder,
            blockSize: blockSize,
            icon: icon))
        .ToArray());

    var trainDataset = new DataFrame(featurizedDatasets[0].Columns);
    for (var i = 1; i < featurizedDatasets.Length; i++)
        trainDataset.Append(featurizedDatasets[i].Rows, true);
    Log.Information("Done training set preparation");

    // Training:
    var mlContext = new MLContext();
    var pipeline = mlContext.Transforms
        .Concatenate(
            "UnscaledFeatures",
            trainDataset.Columns
                .Select(column => column.Name)
                .Where(columnName => columnName != "label")
                .ToArray())
        .Append(
            mlContext.Transforms.NormalizeMeanVariance(
                "Features",
                inputColumnName: "UnscaledFeatures"))
        .Append(
            mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                labelColumnName: "label", 
                featureColumnName: "Features"));

    Log.Information("Training classification model...");
    var model = pipeline.Fit(trainDataset);
    Log.Information("Done model training");

    // Predictions:
    TestCaseRunner.ClassifyBirdSongFile(
        "elegant_pitta_1",
        @"data/XC161619 - Elegant Pitta - Pitta elegans vigorsii.mp3", 
        picturesFolder,
        model,
        blockSize: blockSize,
        icon: icon);

    TestCaseRunner.ClassifyBirdSongFile(
        "elegant_pitta_2",
        @"data/XC166730 - Elegant Pitta - Pitta elegans concinna.mp3",
        picturesFolder,
        model,
        blockSize: blockSize,
        icon: icon);
}
catch (Exception ex)
{
    Log.Error(ex, "Unhandled exception:");
}
finally
{
    Log.CloseAndFlush();
}