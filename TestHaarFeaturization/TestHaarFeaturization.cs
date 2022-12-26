using FluentAssertions;
using HaarFeaturization;
using Shared;

namespace TestHaarFeaturization;

using Feature = KeyValuePair<string, double>;

[TestClass]
public class TestHaarFeaturization
{
    private const double epsilon = 1E-10;

    [TestMethod]
    public void Should_haar_featurize_simple_signal()
    {
        var expected = new List<Feature> { 
            new Feature("scale0_mean", 1),
            new Feature("signal_mean", 0.5) };
        var actual = SignalProvider.Generate(SignalType.Saw, 2).HaarFeaturize();
        expected.Should().Equal(
            actual,
            (x1, x2) => x1.Key == x2.Key && Math.Abs(x1.Value - x2.Value) < epsilon);
    }

    [TestMethod]
    public void Should_filter_scales_when_haar_featurizing()
    {
        var expected = new List<Feature> { new Feature("signal_mean", 0.5) };
        var actual = SignalProvider
            .Generate(SignalType.Saw, 2)
            .HaarFeaturize(featurizer: new DefaultFeaturizer(scalesToDrop: new() { 0 }));
        expected.Should().Equal(
            actual,
            (x1, x2) => x1.Key == x2.Key && Math.Abs(x1.Value - x2.Value) < epsilon);
    }

    [TestMethod]
    public void Should_haar_featurize_simple_signal_set()
    {
        var expectedColumns = new List<string> { 
            "scale0_mean", 
            "scale0_std", 
            "scale1_mean",
            "signal_mean"};
        var signals = new List<List<double>> {
            SignalProvider.Generate(SignalType.Const, 8),
            SignalProvider.Generate(SignalType.Saw, 4),
            SignalProvider.Generate(SignalType.Saw, 4, period: 2) };
        var dataFrame = signals.HaarFeaturize();
        var actualColumns = dataFrame.Columns.Select(x => x.Name).ToList();
        expectedColumns.Should().Equal(actualColumns);
    }

    [TestMethod]
    public void Should_split_and_featurize_simple_signal()
    {
        var expectedColumns = new List<string> {
            "scale0_mean",
            "scale0_std",
            "scale1_mean",
            "signal_mean"};
        var signal = SignalProvider.Generate(SignalType.Saw, 8);
        var dataFrame = signal.HaarFeaturize(4);
        var actualColumns = dataFrame.Columns.Select(x => x.Name).ToList();
        dataFrame.Rows.Count.Should().Be(2);
        expectedColumns.Should().Equal(actualColumns);
    }

        [TestMethod]
    public void Should_normalize_haar_features_of_simple_signal()
    {
        var expected = new List<Feature> {
            new Feature("scale0_mean", 1),
            new Feature("scale0_std", 0),
            new Feature("scale1_mean", 0.5),
            new Feature("signal_mean", 1.5)};
        var actual = SignalProvider
            .Generate(SignalType.Square, 4, period: 2)
            .Normalize(amplitude: 2)
            .Add(SignalProvider.Generate(SignalType.Square, 4, period: 4))
            .HaarFeaturize();
        expected.Should().Equal(
            actual,
            (x1, x2) => x1.Key == x2.Key && Math.Abs(x1.Value - x2.Value) < epsilon);
    }
}