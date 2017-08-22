using System.IO;
using System.Linq;
using System.Text;

namespace PsvDecryptCore.Common
{
    public class StringUtil
    {
        /// <summary>
        ///     Humanizes the title index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string TitleToFileIndex(int index)
            => index.ToString().PadLeft(2, '0');

        /// <summary>
        ///     Humanizes the title to readable filenames.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string TitleToFileName(string title)
        {
            var sb = new StringBuilder();
            foreach (char c in title)
            {
                switch (c)
                {
                    case ' ':
                        sb.Append('-');
                        break;
                    case '-':
                    case '_':
                        sb.Append('-');
                        break;
                    default:
                        if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9')
                            sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        public static string SanitizeTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return null;
            var invalidPathChars = Path.GetInvalidPathChars();
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            return new string(title.Where(x => !invalidFileNameChars.Contains(x) &&
                                               !invalidPathChars.Contains(x)).ToArray());
        }
    }
}