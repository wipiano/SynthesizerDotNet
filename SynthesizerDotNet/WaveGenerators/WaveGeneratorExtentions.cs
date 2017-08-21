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

        /// <summary>
        /// 引数に時間をとる関数をあらわす
        /// </summary>
        /// <param name="time">時間 (秒)</param>
        /// <returns></returns>
        public delegate double FunctionForTime(double time);

        /// <summary>
        /// 時間によって変化する任意の関数を適用します
        /// </summary>
        public static IWaveGenerator ApplyTimeFunction(this IWaveGenerator generator, FunctionForTime func)
        {
            return new FunctionAppliedWaveGenerator(generator, func);
        }

        /// <summary>
        /// 直線的に音量を減衰させます
        /// </summary>
        /// <param name="seconds">減衰にかかる時間</param>
        public static IWaveGenerator LinearDecrease(this IWaveGenerator generator, double seconds)
        {
            return generator.ApplyTimeFunction((sec) => sec > seconds ? 0 : 1 - (sec / seconds));
        }

        /// <summary>
        /// 正弦波によるビブラートを適用します
        /// </summary>
        /// <param name="generator">対象</param>
        /// <param name="freq">1 秒間のビブラート回数</param>
        /// <param name="depth">ビブラートの深さ (大きいほど深い)</param>
        /// <param name="minVolume">最小の音量。</param>
        /// <returns></returns>
        public static IWaveGenerator SineVibrato(this IWaveGenerator generator, double freq, double depth, double minVolume)
        {
            var vibrato = new SinWaveGenerator(generator.SamplePerSecond, freq).Volume(depth).AddConstant(depth + minVolume);
            return new MultipleWaveGenerator(generator, vibrato);
        }

        /// <summary>
        /// 定数のげたをはかせる
        /// </summary>
        /// <returns></returns>
        public static IWaveGenerator AddConstant(this IWaveGenerator generator, double value)
        {
            return new AddConstantWaveGenerator(generator, value);
        }
    }

    /// <summary>
    /// Generator 同士を足し合わせてできる新しい Generator
    /// </summary>
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

    internal abstract class SimpleCustormizedWaveGeneratorBase : IWaveGenerator
    {
        public int SamplePerSecond => this.BaseGenerator.SamplePerSecond;

        protected readonly IWaveGenerator BaseGenerator;

        protected SimpleCustormizedWaveGeneratorBase(IWaveGenerator baseGenerator)
        {
            this.BaseGenerator = baseGenerator;
        }

        public abstract double GetValue(int n);

        public IEnumerable<double> GetWave()
            => Enumerable.Range(0, int.MaxValue).Select(this.GetValue);
    }

    internal class NormalizedWaveGenerator : SimpleCustormizedWaveGeneratorBase
    {
        private readonly double[] _buffer;
        private readonly double _max;

        public NormalizedWaveGenerator(IWaveGenerator generator, int sampleSize)
            : base (generator)
        {
            _buffer = generator.GetWave().Take(sampleSize).ToArray();

            _max = Math.Max(Math.Abs(_buffer.Max()), Math.Abs(_buffer.Min()));
        }

        public override double GetValue(int n)
            => ((n < _buffer.Length) ? _buffer[n] : this.BaseGenerator.GetValue(n)) / _max;
    }

    internal class VolumedWaveGenerator : SimpleCustormizedWaveGeneratorBase
    {
        private readonly double _volume;

        public VolumedWaveGenerator(IWaveGenerator generator, double volume)
            : base(generator)
        {
            _volume = volume;
        }

        public override double GetValue(int n)
            => this.BaseGenerator.GetValue(n) * _volume;
    }

    internal class AddConstantWaveGenerator : SimpleCustormizedWaveGeneratorBase
    {
        private readonly double _const;

        public AddConstantWaveGenerator(IWaveGenerator generator, double value)
            : base(generator)
        {
            _const = value;
        }

        public override double GetValue(int n)
            => this.BaseGenerator.GetValue(n) + _const;
    }

    internal class FunctionAppliedWaveGenerator : SimpleCustormizedWaveGeneratorBase
    {
        private readonly WaveGeneratorExtentions.FunctionForTime _func;

        public FunctionAppliedWaveGenerator(IWaveGenerator generator, WaveGeneratorExtentions.FunctionForTime func)
            : base(generator)
        {
            _func = func;
        }

        public override double GetValue(int n)
        {
            // 秒に変換
            double sec = (double)n / this.SamplePerSecond;

            return this.BaseGenerator.GetValue(n) * _func(sec);
        }
    }

    /// <summary>
    /// Generator 同士を掛け算してできるあたらしい Generator をあらわす
    /// </summary>
    internal class MultipleWaveGenerator : SimpleCustormizedWaveGeneratorBase
    {
        private readonly IWaveGenerator _secondGenerator;

        public MultipleWaveGenerator(IWaveGenerator first, IWaveGenerator second)
            : base(first)
        {
            _secondGenerator = second;
        }

        public override double GetValue(int n)
        {
            return this.BaseGenerator.GetValue(n) * _secondGenerator.GetValue(n);
        }
    }
}