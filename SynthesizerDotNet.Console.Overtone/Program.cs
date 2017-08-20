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
            var factory = new FactorySinWaveGenerator() {SamplesPerSec = sps};

            var volumes = new[] {1.0, 0.5, 0.8, 0.4, 0.25, 0.9};

            var generator = HarmonicsHelper
                .GetHarmonics(A4, 6)
                .Select((freq, i) => factory.Create(freq).Volume(volumes[i]))
                .Aggregate((first, second) => first.Combine(second))
                .Normalize(sps)
                .Volume(0.5);

            var wave = generator.GetWave().Take(100000).ToArray();

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
