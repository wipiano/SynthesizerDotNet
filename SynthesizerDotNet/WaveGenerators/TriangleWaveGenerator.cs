using System.Collections.Generic;
using System.Linq;

namespace SynthesizerDotNet.WaveGenerators
{
    /// <summary>
    /// 三角波ジェネレータ
    /// </summary>
    public class TriangleWaveGenerator : IWaveGenerator
    {
        public int SamplePerSecond { get; }

        private readonly double _freq;

        public TriangleWaveGenerator(int samplePerSecond, double freq)
        {
            this.SamplePerSecond = samplePerSecond;
            _freq = freq;
        }

        public double GetValue(int n)
            => UnitTriangleWave(n * _freq / this.SamplePerSecond);

        public IEnumerable<double> GetWave()
           => Enumerable.Range(0, int.MaxValue).Select(this.GetValue);

        /// <summary>
        /// 周期一秒の三角波関数
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        private static double UnitTriangleWave(double sec)
        {
            bool isPositive = sec % 1 > 0.5;
            var x = sec % 0.5;

            // positive: y = 4x - 1 (-1 -> 1)
            // negative: y = 1 - 4x (1 -> -1)

            return isPositive ? 4 * x - 1 : 1 - 4 * x;
        } 
        
    }
}