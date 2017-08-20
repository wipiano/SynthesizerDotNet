using System;

namespace SynthesizerDotNet
{
    /// <summary>
    /// 平均律の周波数計算機
    /// </summary>
    public class EqualTemperamentFrequencyCalculator
    {
        public double A4 { get; }
        
        public EqualTemperamentFrequencyCalculator() : this(440.0) { }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="a4Frequency">A4 の周波数</param>
        public EqualTemperamentFrequencyCalculator(double a4Frequency)
        {
            this.A4 = a4Frequency;
        }

        /// <summary>
        /// 指定した音名の周波数を取得します
        /// </summary>
        public double GetFrequency(Note note)
        {
            return this.A4 * Math.Pow(2, ((int)note - (int)Note.A4) / 12.0);
        }
    }
}