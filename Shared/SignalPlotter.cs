using ScottPlot;
using System.Drawing;
using System.Drawing.Imaging;

namespace Shared;

public static class SignalPlotter
{
    public static void ToPng(
        this List<double> signal,
        string path,
        string label = "",
        int plotWidth = 300,
        int plotHeight = 300)
        => signal
        .Plot(plotWidth, plotHeight)
        .AddLabel(label, 0, 0)
        .SaveFig(path);

    public static void ToPng(
        this List<double> signal,
        string path,
        List<(int, int, Color)> higlightRegions,
        int plotWidth = 1600,
        int plotHeight = 400,
        bool axes = false,
        Bitmap? icon = null)
    {
        Plot plotWithRegions(int width, int height)
        {
            var plot = signal.Plot(width, height);
            foreach (var region in higlightRegions)
            {
                var regionHighlight = plot.AddHorizontalSpan(region.Item1, region.Item2, color: Color.Empty);
                regionHighlight.HatchColor = region.Item3;
                regionHighlight.HatchStyle = ScottPlot.Drawing.HatchStyle.StripedDownwardDiagonal;
            }

            if (!axes)
                plot.Frameless();

            return plot;
        }
        
        if (icon is null)
            plotWithRegions(plotWidth, plotHeight).SaveFig(path);
        else
        {
            var iconScaledWidth = Convert.ToInt32(icon.Width * Convert.ToDouble(plotHeight) / icon.Height);
            var bitmap = new Bitmap(plotWidth, plotHeight);
            var graphics = Graphics.FromImage(bitmap);
            graphics.DrawImage(icon, 0, 0, iconScaledWidth, plotHeight);
            graphics.DrawImage(
                plotWithRegions(plotWidth - iconScaledWidth, plotHeight).Render(), 
                iconScaledWidth,
                0);
            bitmap.Save(path, ImageFormat.Png);
        }          
    }

    public static void ToPng(
        this List<List<double>> signals,
        string path,
        List<string>? labels = null,
        int plotsPerRow = 3,
        int plotWidth = 300,
        int plotHeight = 300)
    {
        var labelsActual = labels is not null
            ? labels
            : signals.Select((x, i) => $"{i+1}").ToList();
        
        if (signals.Count != labelsActual.Count)
            throw new ArgumentException("Number of signals should be equal to number of labels.");

        var plotsPerColumn = signals.Count / plotsPerRow + (signals.Count % plotsPerRow == 0 ? 0 : 1);
        var bitmap = new Bitmap(plotWidth * plotsPerRow, plotHeight * plotsPerColumn);
        var graphics = Graphics.FromImage(bitmap);
        var plots = signals
            .Zip(labelsActual)
            .Select(x => x.First
                .Plot(plotWidth, plotHeight)
                .AddLabel(x.Second, 0, 0))
            .ToList();
        for (var i = 0; i < plots.Count; i++)
            graphics.DrawImage(
                plots[i].Render(), 
                (i % plotsPerRow) * plotWidth, 
                (i / plotsPerRow) * plotHeight);

        bitmap.Save(path, ImageFormat.Png);
    }

    private static Plot Plot(this IEnumerable<double> signal, int plotWidth, int plotHeight) 
    {
        var plot = new Plot(plotWidth, plotHeight);
        plot.XAxis.TickLabelStyle(fontSize: 15);
        plot.YAxis.TickLabelStyle(fontSize: 15);

        var signalArray = signal.ToArray();
        var sig = plot.AddSignal(signalArray);
        sig.LineWidth = 3;
        sig.MarkerSize = 0;

        var signalAmplitude = signalArray.Select(Math.Abs).Max();
        plot.SetAxisLimits(xMin: 0, xMax: signal.Count(), yMin: -signalAmplitude, yMax: signalAmplitude);

        return plot;
    }

    private static Plot AddLabel(this Plot plot, string label, double x, double y)
    {
        var sigLabel = plot.AddAnnotation(label, x, y);
        sigLabel.Font.Size = 30;
        sigLabel.Background = false;
        sigLabel.Border = false;
        sigLabel.Shadow = false;

        return plot;
    }
}
