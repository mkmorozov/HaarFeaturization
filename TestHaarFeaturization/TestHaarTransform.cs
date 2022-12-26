using FluentAssertions;
using HaarFeaturization;
using Shared;

namespace TestHaarFeaturization;

[TestClass]
public class TestHaarTransform
{
    private const double epsilon = 1E-10;

    [DataTestMethod]
    [DataRow(SignalType.Saw, 0, 0, 0)]
    [DataRow(SignalType.Saw, 0, 1, 0)]
    [DataRow(SignalType.Saw, 0, 3, 2)]
    [DataRow(SignalType.Saw, 0, 8, 8)]
    [DataRow(SignalType.Const, 0, 8, 8)]
    [DataRow(SignalType.Harmonic, 0, 8, 8)]
    [DataRow(SignalType.Saw, 3, 8, 8)]
    [DataRow(SignalType.Square, 3, 8, 8)]
    
    public void Should_transform_and_restore_signal_Given_signal(
        SignalType signalType,
        int period,
        int signalLength, 
        int expectedLength)
    {
        var signal = SignalProvider.Generate(signalType, signalLength, period: period);
        var expected = signal.Take(expectedLength).ToList();
        var actual = signal.HaarTransfom().InverseHaarTransform();
        expected.Should().Equal(actual, (x1, x2) => Math.Abs(x1 - x2) < epsilon);
    }

    [TestMethod]
    public void Should_get_equal_Haar_coefficients_for_signals_of_equal_amplitude()
    { 
        var haarImage1 = SignalProvider.Generate(SignalType.Square, 4, period: 2).HaarTransfom();
        var haarImage2 = SignalProvider.Generate(SignalType.Square, 4, period: 4).HaarTransfom();
        haarImage1[0][0].Should().Be(haarImage2[1][0]);
        haarImage1[0][1].Should().Be(haarImage2[1][0]);
    }

    [TestMethod]
    public void Should_get_equal_spectra_for_sum_and_sequence_of_same_signals()
    { 
        var length = 16;
        var period = 8;
        var sumSpectrum = SignalProvider.Generate(SignalType.Square, 2*length, period: period)
            .Add(SignalProvider.Generate(SignalType.Saw, 2*length, period: period))
            .Normalize()
            .HaarTransfom()
            .Select(x => x.Average())
            .ToList();
        var abSpectrum = SignalProvider.Generate(SignalType.Square, length, period: period)
            .Concat(SignalProvider.Generate(SignalType.Saw, length, period: period))
            .ToList()
            .Normalize()
            .HaarTransfom()
            .Select(x => x.Average())
            .ToList();
        sumSpectrum.Should().Equal(abSpectrum, (x1, x2) => Math.Abs(x1 - x2) < epsilon);
    }

    [TestMethod]
    public void Should_get_equal_spectra_for_same_signals_of_different_lengths()
    {
        var length = 16;
        var period = 8;
        var spectrumA = SignalProvider.Generate(SignalType.Harmonic, length, period: period)
            .HaarTransfom()
            .Select(x => x.Average())
            .ToList();
        var spectrumB = SignalProvider.Generate(SignalType.Harmonic, length + period, period: period)
            .HaarTransfom()
            .Select(x => x.Average())
            .ToList();
        spectrumA.Should().Equal(spectrumB, (x1, x2) => Math.Abs(x1 - x2) < epsilon);
    }
}