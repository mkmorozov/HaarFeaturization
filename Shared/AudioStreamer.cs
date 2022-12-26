using NAudio.Wave.SampleProviders;
using NAudio.Wave;

namespace Shared;

public static class AudioStreamer
{
    const int bufferSize = 1024;

    public static IEnumerable<double> FileAsMono(string filePath)
    {
        using var reader = new AudioFileReader(filePath);
        var sampleProvider = reader.GetMonoSampleProvider();
        var sampleBuffer = new float[bufferSize];
        while (true)
        {
            var samplesRead = sampleProvider.Read(sampleBuffer, 0, bufferSize);
            if (samplesRead == 0)
                break;

            for (var i = 0; i < samplesRead; i++)
                yield return sampleBuffer[i];
        }
    }

    private static ISampleProvider GetMonoSampleProvider(this AudioFileReader reader)
        => reader.WaveFormat.Channels == 1
        ? reader.ToSampleProvider()
        : new StereoToMonoSampleProvider(reader);
}
