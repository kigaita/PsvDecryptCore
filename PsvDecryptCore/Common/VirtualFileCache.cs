using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PsvDecryptCore.Common
{
    public class VirtualFileCache : IDisposable
    {
        private readonly PsStream _encryptedVideoFile;

        public VirtualFileCache(string encryptedVideoFilePath) => _encryptedVideoFile = new PsStream(encryptedVideoFilePath);
        
        public long Length => _encryptedVideoFile.Length;

        public void Dispose() => _encryptedVideoFile.Dispose();

        public async Task ReadAsync(byte[] pv, int offset, int count, IntPtr pcbRead)
        {
            if (Length == 0L)
                return;
            _encryptedVideoFile.Seek(offset, SeekOrigin.Begin);
            int length = await _encryptedVideoFile.ReadAsync(pv, 0, count).ConfigureAwait(false);
            await VideoEncryption.XorBufferAsync(pv, length, offset).ConfigureAwait(false);
            if (IntPtr.Zero != pcbRead)
                Marshal.WriteIntPtr(pcbRead, new IntPtr(length));
        }
    }
}