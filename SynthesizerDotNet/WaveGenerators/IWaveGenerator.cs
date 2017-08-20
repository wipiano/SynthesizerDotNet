using System.Collections.Generic;

namespace SynthesizerDotNet.WaveGenerators
{
    /// <summary>
    /// 波形ジェネレータ
    /// </summary>
    public interface IWaveGenerator
    {
        int SamplePerSecond { get; }

        /// <summary>
        /// n　番目の値を取得します
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        double GetValue(int n);

        IEnumerable<double> GetWave();
    }
}