using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SynthesizerDotNet.Wav
{
    public class WavData
    {
        // RiffHeader, WaveHeader, FmtChunk, DataChunk が ビッグエンディアン
        // そのほかがリトルエンディアン
        public byte[] RiffHeader => Encoding.ASCII.GetBytes("RIFF");

        public uint Size
            => (uint) (this.WaveHeader.Length
                       + this.FmtChunk.Length
                       + 4 // FmtSize 4 bytes
                       + 2 // WavFormat 2 bytes
                       + 2 // Channels 2 bytes
                       + 4 // SamplingLate 4 bytes
                       + 4 // BytesPerSec
                       + 2 // BlockSize
                       + 2 // BitsPerSample
                       + 2 // ExtensionsSize
                       + this.Extensions.Length
                       + this.DataChunk.Length
                       + 4 // WaveBytes
                       + this.WaveBytes // Left + Right bytes
            );

        public byte[] WaveHeader => Encoding.ASCII.GetBytes("WAVE");
        public byte[] FmtChunk => Encoding.ASCII.GetBytes("fmt ");
        public uint FmtSize => (uint)(16 + this.ExtensionsSize);
        public WavFormat Format { get; set; }
        public ushort Channels { get; set; }
        public uint SamplingLate { get; set; }
        public uint BytesPerSec => (uint)(this.SamplingLate * (this.BitsPerSample / 8) * this.Channels);
        public ushort BlockSize => (ushort)((this.BitsPerSample / 8) * this.Channels);
        public ushort BitsPerSample { get; set; }
        public ushort ExtensionsSize => (ushort)this.Extensions.Length;
        public byte[] Extensions { get; set; }
        public byte[] DataChunk => Encoding.ASCII.GetBytes("data");
        public uint WaveBytes => (uint) ((this.BitsPerSample / 8) * (this.Left.Length + this.Right.Length));
        public double[] Left { get; set; }
        public double[] Right { get; set; }


        public byte[] ToBytes()
        {
            using (var stream = new MemoryStream())
            using (var bw = new BinaryWriter(stream))
            {
                bw.Write(this.RiffHeader);
                bw.Write(this.Size);
                bw.Write(this.WaveHeader);
                bw.Write(this.FmtChunk);
                bw.Write(this.FmtSize);
                bw.Write((ushort)this.Format);
                bw.Write(this.Channels);
                bw.Write(this.SamplingLate);
                bw.Write(this.BytesPerSec);
                bw.Write(this.BlockSize);
                bw.Write(this.BitsPerSample);
                bw.Write(this.DataChunk);
                bw.Write(this.WaveBytes);
                bw.Write(this.GetFormattedWave().ToArray());

                return stream.ToArray();
            }
        }

        private IEnumerable<byte> GetFormattedWave()
        {
            switch (this.BitsPerSample)
            {
                case 8:
                    return this.GetFormattedWave8bit();

                case 16:
                    return this.GetFormattedWave16bit();

                default:
                    throw new InvalidOperationException();
            }
        }

        private IEnumerable<byte> GetFormattedWave8bit()
        {
            var left = this.Left
                .Select(x => (x + 1.0) * 128)
                .Select(x => (int) x)
                .Select(x => x > 255 ? 255 : x < 0 ? 0 : x)
                .Select(x => (byte) x);

            var right = this.Right
                .Select(x => (x + 1.0) * 128)
                .Select(x => (int)x)
                .Select(x => x > 255 ? 255 : x < 0 ? 0 : x)
                .Select(x => (byte)x);

            using (var leftEnumerator = left.GetEnumerator())
            using (var rightEnumerator = right.GetEnumerator())
            {
                while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
                {
                    yield return leftEnumerator.Current;
                    yield return rightEnumerator.Current;
                }
            }
        }

        private IEnumerable<byte> GetFormattedWave16bit()
        {
            var left = this.Left
                .Select(x => x * 32768)
                .Select(x => (int) x)
                .Select(x => x > 32767 ? 32767 : x < -32768 ? -32768 : x)
                .Select(x => (short)x);

            var right = this.Right
                .Select(x => x * 32768)
                .Select(x => (int)x)
                .Select(x => x > 32767 ? 32767 : x < -32768 ? -32768 : x)
                .Select(x => (short)x);

            using (var leftEnumerator = left.GetEnumerator())
            using (var rightEnumerator = right.GetEnumerator())
            {
                while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
                {
                    foreach (var b in leftEnumerator.Current.ToBytes(false))
                    {
                        yield return b;
                    }

                    foreach (var b in rightEnumerator.Current.ToBytes(false))
                    {
                        yield return b;
                    }
                }
            }
        }

    }

    // TODO いまつかってないよ
    public class WavHeader
    {
        /// <summary>
        /// RIFF ヘッダ
        /// </summary>
        public string Riff { get; set; }

        /// <summary>
        /// これ以降のファイルサイズ
        /// (ファイルサイズ - 8)
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// WAVE ヘッダ
        /// </summary>
        public string Wave { get; set; }

        /// <summary>
        /// fmt チャンク
        /// </summary>
        public string Fmt { get; set; }

        /// <summary>
        /// fmt チャンクのバイト数。
        /// リニア PCM なら 16
        /// </summary>
        public int FmtSize { get; set; }

        /// <summary>
        /// format type
        /// リニア PCM なら 1
        /// </summary>
        public short FormatTag { get; set; }

        /// <summary>
        /// チャンネル数
        /// </summary>
        public short Channels { get; set; }

        /// <summary>
        /// Sample Rate
        /// </summary>
        public int SamplesPerSec { get; set; }

        /// <summary>
        /// 1 秒あたりの平均バイト数
        /// </summary>
        public int AverageBytesPerSec { get; set; }

        /// <summary>
        /// データのブロックサイズ
        /// </summary>
        public short BlockAlign { get; set; }

        public short BitsPerSample { get; set; }

        public string Data { get; set; }

        public int DataSize { get; set; }

        public static WavHeader CreateEmptyLinearPCM()
        {
            return new WavHeader()
            {
                Riff = "RIFF",
                FileSize = 0,
                Wave = "WAVE",
                Fmt = "fmt ",
                FmtSize = 16,
                FormatTag = 1,
                Channels = 0,
                SamplesPerSec = 0,
                AverageBytesPerSec = 0,
                BlockAlign = 0,
                BitsPerSample = 0,
                Data = "data",
                DataSize = 0
            };
        }
    }
}