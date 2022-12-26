using Microsoft.Data.Analysis;

namespace Shared;

public static class DataFrameExtensions
{
    public static List<(string featureName, double separationScore)> ClusterSeparation(this DataFrame labeledDataset)
    {
        var groupedDataset = labeledDataset.GroupBy("label").Mean();
        var means = groupedDataset.Columns
            .Where(column => column.Name != "label")
            .ToDictionary(
                column => column.Name, 
                column => (column.Mean(), column.Abs().Mean()));
        
        return groupedDataset.Columns
            .Where(column => column.Name != "label")
            .Select(column => (
                column.Name, 
                Separation: Math.Abs(
                    (column - means[column.Name].Item1).Abs().Mean() 
                    / means[column.Name].Item2)))
            .OrderByDescending(x => x.Separation)
            .ToList();
    }

    public static Dictionary<uint, double> BoundingDiameter(this DataFrame dataset, string labelColumnName)
    {
        var result = new Dictionary<uint, double>();
        foreach (var grouping in dataset.GroupBy<uint>(labelColumnName).Groupings)
        {
            var minima = new double[0];
            var maxima = new double[0];
            foreach (var row in grouping)
            {
                var rowDoubles = row
                    .Where(x => x.GetType() == typeof(float) || x.GetType() == typeof(double))
                    .Select(Convert.ToDouble)
                    .ToArray();

                if (!maxima.Any() || !minima.Any())
                {
                    maxima = rowDoubles.ToArray();
                    minima = rowDoubles.ToArray();
                }

                for (var i = 0; i < maxima.Length; i++)
                {
                    if (rowDoubles[i] > maxima[i])
                        maxima[i] = rowDoubles[i];
                    if (rowDoubles[i] < minima[i])
                        minima[i] = rowDoubles[i];
                }
            }
            
            var boundingDiameter = 0d;
            for (var i = 0; i < maxima.Length; i++)
            {
                var d = maxima[i] - minima[i];
                boundingDiameter += d * d;
            }
            result[grouping.Key] = Math.Sqrt(boundingDiameter);
        }

        return result;
    }
}
