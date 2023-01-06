using FluentAssertions;
using HaarFeaturization;
using Microsoft.ML;
using Shared;
using System.Reflection.Emit;

namespace TestEndToEnd;

// TODO:
// 1. Setting MLContext seed doesn't seem to make the pipeline deterministic. Why?
// 2. These asserts are not pretty.
[TestClass]
public class TestClusteringPipelines
{
    [TestMethod]
    public void Should_discern_stationary_and_unstationary_signals()
    {
        var mlContext = new MLContext(seed: 0);
        var dataset = SignalDatasets.AThenB(64, SignalType.Harmonic, 16, SignalType.Harmonic, 32)
            .Take(4)
            .Concat(SignalDatasets.AAndB(64, SignalType.Harmonic, 16, SignalType.Harmonic, 32))
            .ToList()
            .HaarFeaturize(new DefaultFeaturizer(4));
        var labels = mlContext.OptimalKMeansCluster(dataset, 2, 5)
            .labeledDataset["label"]
            .Cast<uint>()
            .ToList();

        labels[0].Should().NotBe(labels[4]);
        labels.Take(4).Should().OnlyContain(x => x == labels[0]);
        labels.Skip(4).Should().OnlyContain(x => x == labels[4]);
    }

    [TestMethod]
    public void Should_ignore_phase_differences_in_similar_signals()
    {
        var mlContext = new MLContext(seed: 1);
        var dataset = SignalDatasets.AThenB(128, SignalType.Harmonic, 64, SignalType.Saw, 64)
            .Concat(SignalDatasets.AThenB(128, SignalType.Saw, 64, SignalType.Square, 64))
            .Concat(SignalDatasets.AThenB(128, SignalType.Square, 64, SignalType.Harmonic, 64))
            .ToList()
            .HaarFeaturize(new DefaultFeaturizer(4));
        var labels = mlContext.OptimalKMeansCluster(dataset, 3, 5)
            .labeledDataset["label"]
            .Cast<uint>()
            .ToList();

        labels[0].Should().NotBe(labels[8]);
        labels[0].Should().NotBe(labels[16]);
        labels[8].Should().NotBe(labels[16]);
        labels.Take(8).Should().OnlyContain(x => x == labels[0]);
        labels.Skip(8).Take(8).Should().OnlyContain(x => x == labels[8]);
        labels.Skip(16).Should().OnlyContain(x => x == labels[16]);
    }
}