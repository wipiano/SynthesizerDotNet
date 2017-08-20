namespace SynthesizerDotNet.WaveGenerators
{
    public class FactorySinWaveGenerator
    {
        public int SamplesPerSec { get; set; }

        public FactorySinWaveGenerator()
        {
        }

        public SinWaveGenerator Create(double freq)
        {
            return new SinWaveGenerator(this.SamplesPerSec, freq);
        }
    }
}