using System;
using System.Collections.Generic;
using System.Linq;

namespace SynthesizerDotNet.WaveGenerators
{
    public static class WaveCombinator
    {
        /// <summary>
        /// 波形出力関数を合成します
        /// </summary>
        /// <returns></returns>
        public static IWaveGenerator Combine(this IWaveGenerator first, IWaveGenerator second)
        {
            return new CombinedWaveGenerator(first, second);
        }

        /// <summary>
        /// 正規化を行います
        /// </summary>
        /// <param name="sampleSize">先頭からのサンプル数</param>
        public static IWaveGenerator Normalize(this IWaveGenerator generator, int sampleSize)
        {
            return new NormalizedWaveGenerator(generator, sampleSize);
        }
    }

    internal class CombinedWaveGenerator : IWaveGenerator
    {
        public int SamplePerSecond { get; }

        private readonly IEnumerable<IWaveGenerator> _generators;

        public CombinedWaveGenerator(IWaveGenerator first, IWaveGenerator second)
        {
            if (first.SamplePerSecond != second.SamplePerSecond)
            {
                throw new InvalidOperationException("サンプリング周波数がことなる波形ジェネレータを合成できません");
            }

            this.SamplePerSecond = first.SamplePerSecond;
            _generators = new IWaveGenerator[] {first, second};
        }

        public double GetValue(int n)
            => _generators.Select(g => g.GetValue(n)).Sum();

        public IEnumerable<double> GetWave()
            => Enumerable.Range(0, int.MaxValue).Select(this.GetValue);
    }

    internal class NormalizedWaveGenerator : IWaveGenerator
    {
        public int SamplePerSecond { get; }

        private readonly IWaveGenerator _generator;
        private readonly double[] _buffer;
        private readonly double _max;

        public NormalizedWaveGenerator(IWaveGenerator generator, int sampleSize)
        {
            _generator = generator;
            _buffer = generator.GetWave().Take(sampleSize).ToArray();

            _max = Math.Max(Math.Abs(_buffer.Max()), Math.Abs(_buffer.Min()));
        }

        public double GetValue(int n)
            => ((n < _buffer.Length) ? _buffer[n] : _generator.GetValue(n)) / _max;

        public IEnumerable<double> GetWave()
            => Enumerable.Range(0, int.MaxValue).Select(this.GetValue);
    }
}