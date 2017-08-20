using System;
using System.Collections.Generic;
using System.Linq;

namespace SynthesizerDotNet.WaveGenerators
{
    public class SinWaveGenerator : IWaveGenerator
    {
        private static readonly double PI = Math.PI;

        public int SamplePerSecond { get; }

        public double Frequency { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplePerSecond">サンプリング周波数</param>
        /// <param name="frequency">周波数</param>
        public SinWaveGenerator(int samplePerSecond, double frequency)
        {
            this.SamplePerSecond = samplePerSecond;
            this.Frequency = frequency;
        }

        public double GetValue(int n)
            => Math.Sin(this.GetRadian(n));

        public IEnumerable<double> GetWave()
            => Enumerable.Range(0, int.MaxValue).Select(this.GetValue);

        /// <summary>
        /// n 番目の sin 関数の angle (radian) を求める
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private double GetRadian(int n)
            => (2 * PI * this.Frequency * n) / this.SamplePerSecond;
    }
}