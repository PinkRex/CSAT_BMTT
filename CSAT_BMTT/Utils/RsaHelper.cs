using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace CSAT_BMTT.Utils
{
    public class RsaHelper
    {
        private static readonly BigInteger ZERO = BigInteger.Zero;
        private static readonly BigInteger ONE = BigInteger.One;
        private static readonly BigInteger TWO = new BigInteger(2);
        private static readonly BigInteger THREE = new BigInteger(3);
        private static readonly int KEY_SIZE = 512;

        public static bool IsProbablePrime(BigInteger n, int k)
        {
            if (n < 2) return false;

            if (n == 2 || n == 3) return true;

            if (n % 2 == 0) return false;
            for (BigInteger i = 3; i * i <= n; i += 2)
            {
                if (n % i == 0)
                    return false;
            }
            return true;
        }

        private static BigInteger RandomBigInteger(BigInteger min, BigInteger max, Random rnd)
        {
            byte[] bytes = max.ToByteArray();
            BigInteger result;
            do
            {
                rnd.NextBytes(bytes);
                result = new BigInteger(bytes);
            } while (result < min || result >= max);
            return result;
        }

        private static BigInteger Gcd(BigInteger a, BigInteger b)
        {
            return b == ZERO ? a : Gcd(b, a % b);
        }

        public static Dictionary<string, string> GenerateKey()
        {
            BigInteger p, q;
            do
            {
                p = GeneratePrime(KEY_SIZE);
            } while (!IsProbablePrime(p, 10));

            do
            {
                q = GeneratePrime(KEY_SIZE);
            } while (!IsProbablePrime(q, 10));

            Dictionary<string, string> keys = new();
            BigInteger n = p * q;
            BigInteger z = (p - ONE) * (q - ONE);
            BigInteger e, d = ZERO;

            for (e = TWO; e < z; e++)
            {
                if (Gcd(e, z) == ONE)
                    break;
            }

            for (BigInteger i = ZERO; i <= new BigInteger(1000000000); i++)
            {
                BigInteger x = ONE + i * z;
                if (x % e == ZERO)
                {
                    d = x / e;
                    break;
                }
            }

            string publicKey = $"e:{e},n:{n}";
            string privateKey = $"d:{d},n:{n}";

            keys["public_key"] = StringToHex(publicKey);
            keys["private_key"] = StringToHex(privateKey);

            return keys;
        }

        private static BigInteger GeneratePrime(int bitLength)
        {
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[bitLength / 8];
            BigInteger prime;

            do
            {
                rng.GetBytes(bytes);
                prime = new BigInteger(bytes);
            } while (prime < TWO || !IsProbablePrime(prime, 10));

            return prime;
        }

        private static string ByteArrayToHex(byte[] bytes)
        {
            StringBuilder sb = new();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        private static byte[] HexToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        private static string ByteArrayToString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        private static List<string> KeyToValues(string key)
        {
            key = ByteArrayToString(HexToByteArray(key));
            List<string> values = new();
            string[] split = key.Split(',');
            values.Add(split[0][2..]);
            values.Add(split[1][2..]);
            return values;
        }

        private static string StringToHex(string text)
        {
            return ByteArrayToHex(Encoding.UTF8.GetBytes(text));
        }

        public static string Encrypt(string text, string publicKey)
        {
            byte[] plainText = Encoding.UTF8.GetBytes(text);
            List<string> values = KeyToValues(publicKey);
            BigInteger e = BigInteger.Parse(values[0]);
            BigInteger n = BigInteger.Parse(values[1]);
            byte[] result = BigInteger.ModPow(new BigInteger(plainText), e, n).ToByteArray();
            return ByteArrayToHex(result);
        }

        public static string Decrypt(string hex, string privateKey)
        {
            byte[] cipherText = HexToByteArray(hex);
            List<string> values = KeyToValues(privateKey);
            BigInteger d = BigInteger.Parse(values[0]);
            BigInteger n = BigInteger.Parse(values[1]);
            byte[] result = BigInteger.ModPow(new BigInteger(cipherText), d, n).ToByteArray();
            return ByteArrayToString(result);
        }
    }
}
