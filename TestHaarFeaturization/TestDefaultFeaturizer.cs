using FluentAssertions;
using HaarFeaturization;
using Shared;

namespace TestHaarFeaturization;

using Feature = KeyValuePair<string, double>;

[TestClass]
public class TestDefaultFeaturizer
{
    private const double epsilon = 1E-8;

    [DataTestMethod]
    [DataRow(SignalType.Saw, 4, -1.36, 0.5, 0, 0.37267799)]
    public void Should_featurize_Given_signal(
        SignalType signalType,
        int signalLength,
        double expectedKurtosis,
        double expectedMean,
        double expectedSkewness,
        double expectedStd)
    {
        var expected = new List<Feature> { 
            new Feature("kurtosis", expectedKurtosis),
            new Feature("mean", expectedMean),
            new Feature("skewness", expectedSkewness),
            new Feature("std", expectedStd) };
        var sig = SignalProvider.Generate(signalType, signalLength);
        var actual = new DefaultFeaturizer(higherMomentsThreshold: 4)
            .Featurize(SignalProvider.Generate(signalType, signalLength));
        expected.Should().Equal(actual, 
            (x1, x2) => x1.Key == x2.Key 
            && Math.Abs(x1.Value - x2.Value) < epsilon);
    }

    [DataTestMethod]
    [DataRow(SignalType.Saw, 4,0.5, 0.37267799)]
    public void Should_not_compute_higher_moments_Given_short_signal(
        SignalType signalType,
        int signalLength,
        double expectedMean,
        double expectedStd)
    {
        var expected = new List<Feature> {
            new Feature("mean", expectedMean),
            new Feature("std", expectedStd) };
        var sig = SignalProvider.Generate(signalType, signalLength);
        var actual = new DefaultFeaturizer(higherMomentsThreshold: 8)
            .Featurize(SignalProvider.Generate(signalType, signalLength));
        expected.Should().Equal(actual,
            (x1, x2) => x1.Key == x2.Key
            && Math.Abs(x1.Value - x2.Value) < epsilon);
    }
}
