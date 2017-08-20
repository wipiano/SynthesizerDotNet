using System;
using System.Collections.Generic;
using System.Linq;

namespace SynthesizerDotNet.WaveGenerators
{
    public static class WaveGeneratorExtentions
    {
        /// <summary>
        /// 波形出力関数を合成します
        /// </summary>
        /// <returns></returns>
        public static IWaveGenerator Combine(this IWaveGenerator first, IWaveGenerator second)
        {
            if (first is CombinedWaveGenerator combined)
            {
                return combined.AddGenerator(second);
            }

            return new CombinedWaveGenerator(first, second);
        }

        /// <summary>
        /// 音量を調整します
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public static IWaveGenerator Volume(this IWaveGenerator generator, double volume)
        {
            return new VolumedWaveGenerator(generator, volume);
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
        public int SamplePerSecond => _generators.First().SamplePerSecond;

        private readonly IEnumerable<IWaveGenerator> _generators;

        public CombinedWaveGenerator(IWaveGenerator first, IWaveGenerator second)
            :this(new [] { first, second })
        {
        }

        public CombinedWaveGenerator(params IWaveGenerator[] generators)
        {
            // サンプリング周波数が等しいこと
            var isSameSampling = generators.Select(g => g.SamplePerSecond).All(s => s == generators[0].SamplePerSecond);

            if (!isSameSampling)
            {
                throw new InvalidOperationException("サンプリング周波数がことなる波形ジェネレータを合成できません");
            }

            _generators = generators.ToArray();
        }

        public CombinedWaveGenerator AddGenerator(IWaveGenerator generator)
        {
            return new CombinedWaveGenerator(_generators.Concat(new [] { generator }).ToArray());
        }

        public double GetValue(int n)
            => _generators.Select(g => g.GetValue(n)).Sum();

        public IEnumerable<double> GetWave()
            => Enumerable.Range(0, int.MaxValue).Select(this.GetValue);
    }

    internal class NormalizedWaveGenerator : IWaveGenerator
    {
        public int SamplePerSecond => _generator.SamplePerSecond;

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

    internal class VolumedWaveGenerator : IWaveGenerator
    {
        public int SamplePerSecond => _generator.SamplePerSecond;

        private readonly IWaveGenerator _generator;
        private readonly double _volume;

        public VolumedWaveGenerator(IWaveGenerator generator, double volume)
        {
            _generator = generator;
            _volume = volume;
        }

        public double GetValue(int n)
            => _generator.GetValue(n) * _volume;

        public IEnumerable<double> GetWave()
            => Enumerable.Range(0, int.MaxValue).Select(this.GetValue);
    }
}