using Demo1_SyntheticSignalsClustering;
using Microsoft.ML;
using Serilog;
using Shared;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/Demo1_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var mlContext = new MLContext();

    // Stationary vs. unstationary signal with same frequency spectra.
    mlContext.RunTestCase(
        "stationary_vs_unstationary",
        SignalDatasets.AThenB(64, SignalType.Harmonic, 16, SignalType.Harmonic, 32).Take(4)
            .Concat(SignalDatasets.AAndB(64, SignalType.Harmonic, 16, SignalType.Harmonic, 32))
            .ToList(),
        2);

    // Three families of short unstationary signals.
    mlContext.RunTestCase(
        "unstationary_3",
        SignalDatasets.AThenB(128, SignalType.Harmonic, 64, SignalType.Saw, 64)
            .Concat(SignalDatasets.AThenB(128, SignalType.Saw, 64, SignalType.Square, 64))
            .Concat(SignalDatasets.AThenB(128, SignalType.Square, 64, SignalType.Harmonic, 64))
            .ToList(),
        3,
        plotsPerRow: 8);

    // Signals with different lengths.
    var ab1 = SignalDatasets.AThenB(64, SignalType.Harmonic, 16, SignalType.Saw, 16);
    var ab2 = SignalDatasets.AThenB(64, SignalType.Harmonic, 16, SignalType.Square, 16);
    mlContext.RunTestCase(
        "unstationary_2_different_lengths_1",
        ab1.Concat(ab1.Select(signal => signal.Concat(signal).ToList()))
            .Concat(ab2)
            .Concat(ab2.Select(signal => signal.Concat(signal).ToList()))
            .ToList(),
        2,
        numberOfAttempts: 10,
        plotsPerRow: 8);

    mlContext.RunTestCase(
        "unstationary_2_different_lengths_2",
        SignalDatasets.AThenB(200, SignalType.Harmonic, 25, SignalType.Saw, 25)
            .Concat(SignalDatasets.AThenB(216, SignalType.Harmonic, 27, SignalType.Saw, 27))
            .Concat(SignalDatasets.AThenB(200, SignalType.Harmonic, 25, SignalType.Square, 25))
            .Concat(SignalDatasets.AThenB(217, SignalType.Harmonic, 27, SignalType.Square, 27))
            .ToList(),
        2,
        numberOfAttempts: 10,
        plotsPerRow: 8);

    mlContext.RunTestCase(
        "unstationary_2_different_lengths_3",
        SignalDatasets.AThenB(100, SignalType.Harmonic, 25, SignalType.Saw, 25)
            .Concat(SignalDatasets.AThenB(108, SignalType.Harmonic, 27, SignalType.Saw, 27))
            .Concat(SignalDatasets.AThenB(100, SignalType.Harmonic, 25, SignalType.Square, 25))
            .Concat(SignalDatasets.AThenB(108, SignalType.Harmonic, 27, SignalType.Square, 27))
            .ToList(),
        2,
        numberOfAttempts: 10,
        plotsPerRow: 8);
}
catch (Exception ex)
{
    Log.Error(ex, "Unhandled exception:");
}
finally
{
    Log.CloseAndFlush();
}
