using System.Linq;
using System.Numerics;

namespace CSGOFriendCodeConverter
{
    public class ByteSwap
    {
        public static BigInteger from_little_endian(byte[] bytes) {
            BigInteger result = 0;
            BigInteger bs = 1;
            foreach (byte b in bytes)
            {
                result += bs * (BigInteger) b;
                bs *= 256;
            }
            return result;
        }
        
        public static BigInteger from_big_endian(byte[] bytes) {
            return from_little_endian(bytes.Reverse().ToArray());
        }
        
        public static byte[] to_little_endian(BigInteger bigNumber) {
            byte[] result = new byte[8];
            int i = 0;
            while (bigNumber > 0) {
                result[i] = (byte) (bigNumber % 256);
                bigNumber /= 256;
                i += 1;
            }
            return result;
        }
        
        public static byte[] to_big_endian(BigInteger bigNumber) {
            return to_little_endian(bigNumber).Reverse().ToArray();
        }
    }
}