using System.Threading.Tasks;

namespace PsvDecryptCore.Common
{
    public class VideoEncryption
    {
        private const string EncryptionStringA = "pluralsight";

        private const string EncryptionStringB =
                "\x0006?zY¢\x00B2\x0085\x009FL\x00BEî0Ö.ì\x0017#©>Å£Q\x0005¤°\x00018Þ^\x008Eú\x0019Lqß'\x009D\x0003ßE\x009EM\x0080'x:\0~\x00B9\x0001ÿ 4\x00B3õ\x0003Ã§Ê\x000EAË\x00BC\x0090è\x009Eî~\x008B\x009Aâ\x001B¸UD<\x007FKç*\x001Döæ7H\v\x0015Arý*v÷%Âþ\x00BEä;pü"
            ;

        public static byte[] XorBuffer(byte[] buff, int length, long position)
        {
            for (int index = 0; index < length; ++index)
            {
                byte num = (byte) ((ulong) (EncryptionStringA[(int) ((position + index) % EncryptionStringA.Length)] ^
                                            EncryptionStringB[(int) ((position + index) % EncryptionStringB.Length)]) ^
                                   (ulong) ((position + index) % 251L));
                buff[index] = (byte) (buff[index] ^ (uint) num);
            }
            return buff;
        }
    }
}