using Microsoft.Data.Analysis;

namespace HaarFeaturization;

using Row = IEnumerable<KeyValuePair<string, double>>;

public static class DataFrameExtensions
{
    public static void AppendExisting(this DataFrame dataFrame, IEnumerable<Row> rows)
    {
        var columnNames = dataFrame.Columns.Select(column => column.Name).ToList();
        foreach (var row in rows)
            dataFrame.Append(
                row.Where(x => columnNames.Contains(x.Key))
                    .Select(x => new KeyValuePair<string, object>(x.Key, x.Value)),
                inPlace: true);
    }
}
