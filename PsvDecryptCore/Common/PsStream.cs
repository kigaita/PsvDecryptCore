using System.IO;

namespace PsvDecryptCore.Common
{
    public class PsStream : IPsStream
    {
        private readonly Stream _fileStream;

        public PsStream(string filenamePath)
        {
            _fileStream = File.Open(filenamePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            Length = new FileInfo(filenamePath).Length;
        }

        public int BlockSize => 262144;

        public long Length { get; private set; }

        public void Seek(int offset, SeekOrigin begin)
        {
            if (Length <= 0L)
                return;
            _fileStream.Seek(offset, begin);
        }

        public int Read(byte[] pv, int i, int count) => Length <= 0L ? 0 : _fileStream.Read(pv, i, count);

        public void Dispose()
        {
            Length = 0L;
            _fileStream.Dispose();
        }
    }
}