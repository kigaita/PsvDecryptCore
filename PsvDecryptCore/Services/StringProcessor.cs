using System.IO;
using System.Linq;
using System.Text;

namespace PsvDecryptCore.Common
{
    public class StringProcessor
    {
        private readonly string _invalidChars;

        public StringProcessor() => _invalidChars =
            new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars());

        /// <summary>
        ///     Humanizes the title index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string TitleToFileIndex(int index)
            => index.ToString().PadLeft(2, '0');

        /// <summary>
        ///     Removes invalid chars within a title.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public string SanitizeTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return null;
            var sb = new StringBuilder();
            foreach (char c in title)
                sb.Append(_invalidChars.Contains(c) ? '.' : c);
            return sb.ToString();
        }
    }
}