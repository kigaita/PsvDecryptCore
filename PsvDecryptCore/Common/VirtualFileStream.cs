using System;
using System.IO;
using System.Threading.Tasks;

namespace PsvDecryptCore.Common
{
    public class VirtualFileStream : IDisposable
    {
        private readonly PsStream _encryptedVideoFile;

        public VirtualFileStream(string encryptedVideoFilePath) => _encryptedVideoFile =
            new PsStream(encryptedVideoFilePath);

        public long Length => _encryptedVideoFile.Length;

        public void Dispose() => _encryptedVideoFile.Dispose();

        public async Task<byte[]> ReadAsync()
        {
            if (Length == 0L) return null;
            var pv = new byte[Length];
            _encryptedVideoFile.Seek(0, SeekOrigin.Begin);
            int length = await _encryptedVideoFile.ReadAsync(pv, 0, (int)Length).ConfigureAwait(false);
            var result = await VideoEncryption.XorBufferAsync(pv, length, 0).ConfigureAwait(false);
            return result;
        }
    }
}