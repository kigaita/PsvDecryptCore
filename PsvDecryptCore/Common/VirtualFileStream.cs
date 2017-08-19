using System;

namespace PsvDecryptCore.Common
{
    internal class VirtualFileStream : IDisposable
    {
        private readonly VirtualFileCache _cache;
        private long _position;

        public VirtualFileStream(string encryptedVideoFilePath) =>
            _cache = new VirtualFileCache(encryptedVideoFilePath);

        public void Dispose() => _cache.Dispose();

        public byte[] ReadAll()
        {
            unsafe
            {
                int pcbReadSign = 1;
                var pcbRead = new IntPtr(&pcbReadSign);
                long length = _cache.Length;
                var pv = new byte[length];
                _cache.ReadAsync(pv, (int) _position, (int) length, pcbRead).GetAwaiter().GetResult();
                return pv;
            }
        }
    }
}