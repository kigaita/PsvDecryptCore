using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PsvDecryptCore.Common
{
    public class VirtualFileCache : IDisposable
    {
        private readonly IPsStream _encryptedVideoFile;

        public VirtualFileCache(string encryptedVideoFilePath) => _encryptedVideoFile =
            new PsStream(encryptedVideoFilePath);

        public VirtualFileCache(IPsStream stream) => _encryptedVideoFile = stream;

        public long Length => _encryptedVideoFile.Length;

        public void Dispose() => _encryptedVideoFile.Dispose();

        public async Task ReadAsync(byte[] pv, int offset, int count, IntPtr pcbRead)
        {
            if (Length == 0L)
                return;
            _encryptedVideoFile.Seek(offset, SeekOrigin.Begin);
            int length = _encryptedVideoFile.Read(pv, 0, count);
            await VideoEncryption.XorBufferAsync(pv, length, offset).ConfigureAwait(false);
            if (!(IntPtr.Zero != pcbRead))
                return;
            Marshal.WriteIntPtr(pcbRead, new IntPtr(length));
        }
    }
}