using System.IO;
using System.Linq;
using SynthesizerDotNet.Wav;
using SynthesizerDotNet.WaveGenerators;

namespace SynthesizerDotNet.Console.A440
{
    class Program
    {
        static void Main(string[] args)
        {
            var bytes = new SinWaveGenerator(8000, 440)
                .Normalize(8000)
                .GetWave()
                .Take(32000)
                .ToArray();

            var wav = new WavData()
            {
                Format = WavFormat.PCM,
                Channels = 2,
                SamplingLate = 8000,
                BitsPerSample = 8,
                Extensions = new byte[] { },
                Left = bytes,
                Right = bytes
            };

            var path = $"{Directory.GetCurrentDirectory()}/test.wav";
            System.Console.WriteLine($"path: {path}");
            using (var stream = File.Create(path))
            {
                foreach (var b in wav.ToBytes())
                {
                    stream.WriteByte(b);
                }
            }

            System.Console.WriteLine("440Hz の音を再生します");
            var player = new SoundPlayer();
            player.Play(wav.ToBytes(), noWaitIfBusy: false);

            System.Console.WriteLine("完了");

            System.Console.ReadKey();
        }
    }
}
