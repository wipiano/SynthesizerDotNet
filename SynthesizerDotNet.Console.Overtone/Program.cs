using System.Linq;
using SynthesizerDotNet.Wav;
using SynthesizerDotNet.WaveGenerators;

namespace SynthesizerDotNet.Console.Overtone
{
    class Program
    {
        static void Main(string[] args)
        {
            const int sps = 44100;
            const double A4 = 440;
            var calc = new EqualTemperamentFrequencyCalculator(A4);

            var volumes = new[] {1.0, 0.5, 0.3, 0.4, 0.25, 0.35};

            var generator = HarmonicsHelper
                .GetHarmonics(A4, 6)
                .Select((freq, i) => new SinWaveGenerator(sps, freq).Volume(volumes[i]))
                .Aggregate((first, second) => first.Combine(second))
                .Normalize(sps);

            var wave = generator.GetWave().Take(sps).ToArray();

            var wav = new WavData()
            {
                Format = WavFormat.PCM,
                Channels = 2,
                SamplingLate = sps,
                BitsPerSample = 8,
                Extensions = new byte[] { },
                Left = wave,
                Right = wave
            };

            new SoundPlayer().Play(wav.ToBytes());
        }
    }
}
