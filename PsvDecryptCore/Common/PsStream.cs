using System.IO;
using System.Threading.Tasks;

namespace PsvDecryptCore.Common
{
    public class PsStream
    {
        private readonly Stream _fileStream;

        public PsStream(string filenamePath)
        {
            _fileStream = File.OpenRead(filenamePath);
            Length = new FileInfo(filenamePath).Length;
        }

        public long Length { get; private set; }

        public void Seek(int offset, SeekOrigin begin)
        {
            if (Length <= 0L)
                return;
            _fileStream.Seek(offset, begin);
        }

        public Task<int> ReadAsync(byte[] pv, int i, int count) => Length <= 0L
            ? Task.FromResult(0)
            : _fileStream.ReadAsync(pv, i, count);

        public void Dispose()
        {
            Length = 0L;
            _fileStream.Dispose();
        }
    }
}