using System.IO;

namespace PsvDecryptCore.Common
{
    public interface IPsStream
    {
        int BlockSize { get; }
        long Length { get; }

        void Seek(int offset, SeekOrigin begin);

        int Read(byte[] pv, int i, int count);

        void Dispose();
    }
}