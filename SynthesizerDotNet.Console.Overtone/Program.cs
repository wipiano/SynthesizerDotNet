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

            var volumes = new[] {1.0, 0.5, 0.8, 0.4, 0.25, 0.3, 0.1, 0.2, 0.025, 0.05, 0.01};

            var baseGenerator = HarmonicsHelper
                .GetHarmonics(A4, 11)
                .Select((freq, i) => factory.Create(freq).Volume(volumes[i]))
                .Aggregate((first, second) => first.Combine(second));

            var decreaseGenerator = baseGenerator
                .LinearDecrease(2.0)
                .Normalize(sps)
                .Volume(0.5);

            Play(decreaseGenerator, 100000, sps);

            var vibratoGenerator = baseGenerator.SineVibrato(4, 0.1, 0.8).Normalize(sps).Volume(0.5);

            Play(vibratoGenerator, 100000, sps);
        }

        private static void Play(IWaveGenerator generator, int count, uint samplingLate)
        {
            var wave = generator.GetWave().Take(count).ToArray();

            var wav = new WavData()
            {
                Format = WavFormat.PCM,
                Channels = 2,
                SamplingLate = samplingLate,
                BitsPerSample = 8,
                Extensions = new byte[] { },
                Left = wave,
                Right = wave
            };

            new SoundPlayer().Play(wav.ToBytes());
        }
    }
}
