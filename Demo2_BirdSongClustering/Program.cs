using Demo2_BirdSongClustering;
using HaarFeaturization;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/Demo2_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    await TestCaseRunner.Run(
        "sardinian_warbler",
        @"data/XC355049 - Sardinian Warbler - Sylvia melanocephala.mp3",
        @"data/Sardinian_Warbler.jpg");

    await TestCaseRunner.Run(
        "cocha_antshrike",
        @"data/XC146664 - Cocha Antshrike - Thamnophilus praecox.mp3",
        @"data/Cocha_Antshrike.jpg",
        numberOfSamplesToPlot: 300000);

    await TestCaseRunner.Run(
        "song_thrush",
        @"data/XC36805 - Song Thrush - Turdus philomelos clarkei.mp3",
        @"data/Turdus_Philomelos.jpg",
        numberOfSamplesToPlot: 300000);

    await TestCaseRunner.Run(
        "chopi_blackbird",
        @"data/XC702488 - Chopi Blackbird - Gnorimopsar chopi megistus.mp3",
        @"data/Gnorimopsar_Chopi.jpg",
        numberOfSamplesToPlot: 300000);

    await TestCaseRunner.Run(
        "chopi_blackbird_filtered",
        @"data/XC702488 - Chopi Blackbird - Gnorimopsar chopi megistus.mp3",
        @"data/Gnorimopsar_Chopi.jpg",
        featurizer: new DefaultFeaturizer(scalesToDrop: new() { 0 }),
        numberOfSamplesToPlot: 300000);
}
catch (Exception ex)
{
    Log.Error(ex, "Unhandled exception:");
}
finally
{
    Log.CloseAndFlush();
}