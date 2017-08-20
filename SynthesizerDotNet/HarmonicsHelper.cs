using System.Collections.Generic;
using System.Linq;

namespace SynthesizerDotNet
{
    public static class HarmonicsHelper
    {
        /// <summary>
        /// 倍音列を取得します
        /// </summary>
        public static IEnumerable<double> GetHarmonics(double baseFrequency, int count)
        {
            return Enumerable
                .Range(1, count)
                .Select(n => n * baseFrequency);
        }
    }
}