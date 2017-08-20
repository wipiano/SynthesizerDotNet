using System.Runtime.InteropServices;

namespace SynthesizerDotNet
{
    public class SoundPlayer
    {
        [DllImport("winmm.dll", EntryPoint = "PlaySound")]
        private static extern int PlaySound(
            [MarshalAs(UnmanagedType.LPStr)] string pszSound,
            int hModule,
            int dwFlags);

        [DllImport("winmm.dll", EntryPoint = "PlaySound")]
        private static extern int PlaySound(
            [MarshalAs(UnmanagedType.LPArray)] byte[] pszSound,
            int hModule,
            int dwFlags);

        // 再生フラグ
        private const int SND_SYNC = 0x0;    // 同期再生
        private const int SND_ASYNC = 0x1;    // 非同期再生
        private const int SND_MEMORY = 0x4;    // バッファからの再生
        private const int SND_LOOP = 0x8;    // ループ再生
        private const int SND_NOSTOP = 0x10;   // 再生中のサウンドを停止しない
        private const int SND_NOWAIT = 0x2000; // ビジー状態なら即座に処理を返す

        public void Play(byte[] bytes, bool loop = false, bool noStop = false, bool noWaitIfBusy = true)
        {
            int flags = SND_SYNC | SND_MEMORY;
            if (loop) flags |= SND_LOOP;
            if (noStop) flags |= SND_NOSTOP;
            if (noWaitIfBusy) flags |= SND_NOWAIT;

            PlaySound(bytes, 0, flags);
        }
    }
}