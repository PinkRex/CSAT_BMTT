using System.Text;

namespace CSAT_BMTT.Utils
{

    public class AesHelper
    {
        private static readonly int KEY_SIZE = 128;
        private static readonly int BLOCK_SIZE = KEY_SIZE / 8;
        private static readonly int STATE_COLUMN = 4;               // Nb
        private static readonly int KEY_LENGTH = KEY_SIZE / 32;     // Nk
        private static readonly int ROUND_COUNT = KEY_LENGTH + 6;   // Nr

        private static readonly string sKey = "hoangduccsatbmtt";
        private static readonly byte[] key = Encoding.UTF8.GetBytes(sKey);
        private static byte[,] state = new byte[4, 4];
        private static byte[] roundKey = new byte[240];

        private static readonly int[] sBox = {0x63, 0x7C, 0x77, 0x7B, 0xF2, 0x6B, 0x6F, 0xC5, 0x30, 0x01, 0x67, 0x2B, 0xFE, 0xD7,
        0xAB, 0x76, 0xCA, 0x82, 0xC9, 0x7D, 0xFA, 0x59, 0x47, 0xF0, 0xAD, 0xD4, 0xA2, 0xAF, 0x9C, 0xA4, 0x72, 0xC0,
        0xB7, 0xFD, 0x93, 0x26, 0x36, 0x3F, 0xF7, 0xCC, 0x34, 0xA5, 0xE5, 0xF1, 0x71, 0xD8, 0x31, 0x15, 0x04, 0xC7,
        0x23, 0xC3, 0x18, 0x96, 0x05, 0x9A, 0x07, 0x12, 0x80, 0xE2, 0xEB, 0x27, 0xB2, 0x75, 0x09, 0x83, 0x2C, 0x1A,
        0x1B, 0x6E, 0x5A, 0xA0, 0x52, 0x3B, 0xD6, 0xB3, 0x29, 0xE3, 0x2F, 0x84, 0x53, 0xD1, 0x00, 0xED, 0x20, 0xFC,
        0xB1, 0x5B, 0x6A, 0xCB, 0xBE, 0x39, 0x4A, 0x4C, 0x58, 0xCF, 0xD0, 0xEF, 0xAA, 0xFB, 0x43, 0x4D, 0x33, 0x85,
        0x45, 0xF9, 0x02, 0x7F, 0x50, 0x3C, 0x9F, 0xA8, 0x51, 0xA3, 0x40, 0x8F, 0x92, 0x9D, 0x38, 0xF5, 0xBC, 0xB6,
        0xDA, 0x21, 0x10, 0xFF, 0xF3, 0xD2, 0xCD, 0x0C, 0x13, 0xEC, 0x5F, 0x97, 0x44, 0x17, 0xC4, 0xA7, 0x7E, 0x3D,
        0x64, 0x5D, 0x19, 0x73, 0x60, 0x81, 0x4F, 0xDC, 0x22, 0x2A, 0x90, 0x88, 0x46, 0xEE, 0xB8, 0x14, 0xDE, 0x5E,
        0x0B, 0xDB, 0xE0, 0x32, 0x3A, 0x0A, 0x49, 0x06, 0x24, 0x5C, 0xC2, 0xD3, 0xAC, 0x62, 0x91, 0x95, 0xE4, 0x79,
        0xE7, 0xC8, 0x37, 0x6D, 0x8D, 0xD5, 0x4E, 0xA9, 0x6C, 0x56, 0xF4, 0xEA, 0x65, 0x7A, 0xAE, 0x08, 0xBA, 0x78,
        0x25, 0x2E, 0x1C, 0xA6, 0xB4, 0xC6, 0xE8, 0xDD, 0x74, 0x1F, 0x4B, 0xBD, 0x8B, 0x8A, 0x70, 0x3E, 0xB5, 0x66,
        0x48, 0x03, 0xF6, 0x0E, 0x61, 0x35, 0x57, 0xB9, 0x86, 0xC1, 0x1D, 0x9E, 0xE1, 0xF8, 0x98, 0x11, 0x69, 0xD9,
        0x8E, 0x94, 0x9B, 0x1E, 0x87, 0xE9, 0xCE, 0x55, 0x28, 0xDF, 0x8C, 0xA1, 0x89, 0x0D, 0xBF, 0xE6, 0x42, 0x68,
        0x41, 0x99, 0x2D, 0x0F, 0xB0, 0x54, 0xBB, 0x16};

        private static readonly int[] rsBox = {0x52, 0x09, 0x6A, 0xD5, 0x30, 0x36, 0xA5, 0x38, 0xBF, 0x40, 0xA3, 0x9E, 0x81, 0xF3,
        0xD7, 0xFB, 0x7C, 0xE3, 0x39, 0x82, 0x9B, 0x2F, 0xFF, 0x87, 0x34, 0x8E, 0x43, 0x44, 0xC4, 0xDE, 0xE9, 0xCB,
        0x54, 0x7B, 0x94, 0x32, 0xA6, 0xC2, 0x23, 0x3D, 0xEE, 0x4C, 0x95, 0x0B, 0x42, 0xFA, 0xC3, 0x4E, 0x08, 0x2E,
        0xA1, 0x66, 0x28, 0xD9, 0x24, 0xB2, 0x76, 0x5B, 0xA2, 0x49, 0x6D, 0x8B, 0xD1, 0x25, 0x72, 0xF8, 0xF6, 0x64,
        0x86, 0x68, 0x98, 0x16, 0xD4, 0xA4, 0x5C, 0xCC, 0x5D, 0x65, 0xB6, 0x92, 0x6C, 0x70, 0x48, 0x50, 0xFD, 0xED,
        0xB9, 0xDA, 0x5E, 0x15, 0x46, 0x57, 0xA7, 0x8D, 0x9D, 0x84, 0x90, 0xD8, 0xAB, 0x00, 0x8C, 0xBC, 0xD3, 0x0A,
        0xF7, 0xE4, 0x58, 0x05, 0xB8, 0xB3, 0x45, 0x06, 0xD0, 0x2C, 0x1E, 0x8F, 0xCA, 0x3F, 0x0F, 0x02, 0xC1, 0xAF,
        0xBD, 0x03, 0x01, 0x13, 0x8A, 0x6B, 0x3A, 0x91, 0x11, 0x41, 0x4F, 0x67, 0xDC, 0xEA, 0x97, 0xF2, 0xCF, 0xCE,
        0xF0, 0xB4, 0xE6, 0x73, 0x96, 0xAC, 0x74, 0x22, 0xE7, 0xAD, 0x35, 0x85, 0xE2, 0xF9, 0x37, 0xE8, 0x1C, 0x75,
        0xDF, 0x6E, 0x47, 0xF1, 0x1A, 0x71, 0x1D, 0x29, 0xC5, 0x89, 0x6F, 0xB7, 0x62, 0x0E, 0xAA, 0x18, 0xBE, 0x1B,
        0xFC, 0x56, 0x3E, 0x4B, 0xC6, 0xD2, 0x79, 0x20, 0x9A, 0xDB, 0xC0, 0xFE, 0x78, 0xCD, 0x5A, 0xF4, 0x1F, 0xDD,
        0xA8, 0x33, 0x88, 0x07, 0xC7, 0x31, 0xB1, 0x12, 0x10, 0x59, 0x27, 0x80, 0xEC, 0x5F, 0x60, 0x51, 0x7F, 0xA9,
        0x19, 0xB5, 0x4A, 0x0D, 0x2D, 0xE5, 0x7A, 0x9F, 0x93, 0xC9, 0x9C, 0xEF, 0xA0, 0xE0, 0x3B, 0x4D, 0xAE, 0x2A,
        0xF5, 0xB0, 0xC8, 0xEB, 0xBB, 0x3C, 0x83, 0x53, 0x99, 0x61, 0x17, 0x2B, 0x04, 0x7E, 0xBA, 0x77, 0xD6, 0x26,
        0xE1, 0x69, 0x14, 0x63, 0x55, 0x21, 0x0C, 0x7D};

        private static readonly int[] rCon = {0x8d, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab,
        0x4d, 0x9a, 0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39,
        0x72, 0xe4, 0xd3, 0xbd, 0x61, 0xc2, 0x9f, 0x25, 0x4a, 0x94, 0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8,
        0xcb, 0x8d, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a, 0x2f,
        0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39, 0x72, 0xe4, 0xd3,
        0xbd, 0x61, 0xc2, 0x9f, 0x25, 0x4a, 0x94, 0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8, 0xcb, 0x8d, 0x01,
        0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a, 0x2f, 0x5e, 0xbc, 0x63,
        0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39, 0x72, 0xe4, 0xd3, 0xbd, 0x61, 0xc2,
        0x9f, 0x25, 0x4a, 0x94, 0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8, 0xcb, 0x8d, 0x01, 0x02, 0x04, 0x08,
        0x10, 0x20, 0x40, 0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a, 0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35,
        0x6a, 0xd4, 0xb3, 0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39, 0x72, 0xe4, 0xd3, 0xbd, 0x61, 0xc2, 0x9f, 0x25, 0x4a,
        0x94, 0x33, 0x66, 0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8, 0xcb, 0x8d, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40,
        0x80, 0x1b, 0x36, 0x6c, 0xd8, 0xab, 0x4d, 0x9a, 0x2f, 0x5e, 0xbc, 0x63, 0xc6, 0x97, 0x35, 0x6a, 0xd4, 0xb3,
        0x7d, 0xfa, 0xef, 0xc5, 0x91, 0x39, 0x72, 0xe4, 0xd3, 0xbd, 0x61, 0xc2, 0x9f, 0x25, 0x4a, 0x94, 0x33, 0x66,
        0xcc, 0x83, 0x1d, 0x3a, 0x74, 0xe8, 0xcb};

        private static byte GetSBoxValue(int num)
        {
            return (byte)(sBox[num] & 0xFF);
        }

        public static byte GetSBoxInvert(int num)
        {
            return (byte)(rsBox[num] & 0xFF);
        }

        private static void KeyExpansion()
        {
            int i, j;
            byte[] temp = new byte[4];
            byte k;

            for (i = 0; i < KEY_LENGTH; i++)
            {
                roundKey[i * 4] = key[i * 4];
                roundKey[i * 4 + 1] = key[i * 4 + 1];
                roundKey[i * 4 + 2] = key[i * 4 + 2];
                roundKey[i * 4 + 3] = key[i * 4 + 3];
            }

            while (i < (STATE_COLUMN * (ROUND_COUNT + 1)))
            {
                for (j = 0; j < 4; j++)
                {
                    temp[j] = roundKey[(i - 1) * 4 + j];
                }
                if (i % KEY_LENGTH == 0)
                {
                    // Rotword
                    {
                        k = temp[0];
                        temp[0] = temp[1];
                        temp[1] = temp[2];
                        temp[2] = temp[3];
                        temp[3] = k;
                    }
                    // SubWord
                    {
                        temp[0] = GetSBoxValue(temp[0] & 0x000000ff);
                        temp[1] = GetSBoxValue(temp[1] & 0x000000ff);
                        temp[2] = GetSBoxValue(temp[2] & 0x000000ff);
                        temp[3] = GetSBoxValue(temp[3] & 0x000000ff);
                    }
                    temp[0] = (byte)(temp[0] ^ rCon[i / KEY_LENGTH]);
                }
                roundKey[i * 4 + 0] = (byte)(roundKey[(i - KEY_LENGTH) * 4 + 0] ^ temp[0]);
                roundKey[i * 4 + 1] = (byte)(roundKey[(i - KEY_LENGTH) * 4 + 1] ^ temp[1]);
                roundKey[i * 4 + 2] = (byte)(roundKey[(i - KEY_LENGTH) * 4 + 2] ^ temp[2]);
                roundKey[i * 4 + 3] = (byte)(roundKey[(i - KEY_LENGTH) * 4 + 3] ^ temp[3]);
                i++;
            }
        }

        private static void AddRoundKey(int round)
        {
            int i, j;
            for (i = 0; i < 4; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    state[j, i] = (byte)(state[j, i] ^ roundKey[round * STATE_COLUMN * 4 + i * STATE_COLUMN + j]);
                }
            }
        }

        private static void SubBytes()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    state[i, j] = GetSBoxValue(state[i, j] & 0x000000ff);
                }
            }
        }

        private static void InvSubBytes()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    state[i, j] = GetSBoxInvert(state[i, j] & 0x000000ff);

                }
            }
        }

        private static void ShiftRows()
        {
            byte temp;

            temp = state[1, 0];
            state[1, 0] = state[1, 1];
            state[1, 1] = state[1, 2];
            state[1, 2] = state[1, 3];
            state[1, 3] = temp;

            temp = state[2, 0];
            state[2, 0] = state[2, 2];
            state[2, 2] = temp;

            temp = state[2, 1];
            state[2, 1] = state[2, 3];
            state[2, 3] = temp;

            temp = state[3, 0];
            state[3, 0] = state[3, 3];
            state[3, 3] = state[3, 2];
            state[3, 2] = state[3, 1];
            state[3, 1] = temp;
        }

        private static void InvShiftRow()
        {
            byte temp;

            temp = state[1, 3];
            state[1, 3] = state[1, 2];
            state[1, 2] = state[1, 1];
            state[1, 1] = state[1, 0];
            state[1, 0] = temp;

            temp = state[2, 0];
            state[2, 0] = state[2, 2];
            state[2, 2] = temp;

            temp = state[2, 1];
            state[2, 1] = state[2, 3];
            state[2, 3] = temp;

            temp = state[3, 0];
            state[3, 0] = state[3, 1];
            state[3, 1] = state[3, 2];
            state[3, 2] = state[3, 3];
            state[3, 3] = temp;
        }

        private static byte XTime(byte x)
        {
            int ux = x & 0xff;
            return (byte)(((ux << 1) ^ (((ux >> 7) & 1) * 0x1b)) & 0xff);
        }

        private static byte Multiply(byte x, byte y)
        {
            int ux = (x & 0xFF);
            int uy = (y & 0xFF);
            int result = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((uy & 1) == 1)
                {
                    result ^= ux;
                }
                bool carry = (ux & 0x80) != 0;
                ux <<= 1;
                if (carry)
                {
                    ux ^= 0x1B; // 00011011
                }
                uy >>= 1;
            }
            return (byte)(result & 0xFF);
        }

        private static void MixColumns()
        {
            byte Tmp, Tm, t;
            for (int i = 0; i < 4; i++)
            {
                t = state[0, i];
                Tmp = (byte)(state[0, i] ^ state[1, i] ^ state[2, i] ^ state[3, i]);
                Tm = (byte)(state[0, i] ^ state[1, i]);
                Tm = XTime(Tm);
                state[0, i] ^= (byte)(Tm ^ Tmp);
                Tm = (byte)(state[1, i] ^ state[2, i]);
                Tm = XTime(Tm);
                state[1, i] ^= (byte)(Tm ^ Tmp);
                Tm = (byte)(state[2, i] ^ state[3, i]);
                Tm = XTime(Tm);
                state[2, i] ^= (byte)(Tm ^ Tmp);
                Tm = (byte)(state[3, i] ^ t);
                Tm = XTime(Tm);
                state[3, i] ^= (byte)(Tm ^ Tmp);
            }
        }

        private static void InvMixColumns()
        {
            byte a, b, c, d;
            for (int i = 0; i < 4; i++)
            {
                a = state[0, i];
                b = state[1, i];
                c = state[2, i];
                d = state[3, i];

                state[0, i] = (byte)(Multiply(a, 0x0e) ^ Multiply(b, 0x0b) ^ Multiply(c, 0x0d) ^ Multiply(d, 0x09));
                state[1, i] = (byte)(Multiply(a, 0x09) ^ Multiply(b, 0x0e) ^ Multiply(c, 0x0b) ^ Multiply(d, 0x0d));
                state[2, i] = (byte)(Multiply(a, 0x0d) ^ Multiply(b, 0x09) ^ Multiply(c, 0x0e) ^ Multiply(d, 0x0b));
                state[3, i] = (byte)(Multiply(a, 0x0b) ^ Multiply(b, 0x0d) ^ Multiply(c, 0x09) ^ Multiply(d, 0x0e));
            }
        }

        private static byte[] Cipher(byte[] in_, byte[] iv)
        {
            byte[] out_ = new byte[BLOCK_SIZE];
            int round = 0;
            in_ = XorWithIv(in_, iv);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    state[j, i] = in_[i * 4 + j];
                }
            }
            KeyExpansion();
            AddRoundKey(0);
            for (round = 1; round < ROUND_COUNT; round++)
            {
                SubBytes();
                ShiftRows();
                MixColumns();
                AddRoundKey(round);
            }
            SubBytes();
            ShiftRows();
            AddRoundKey(ROUND_COUNT);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    out_[i * 4 + j] = state[j, i];
                }
            }
            return out_;
        }

        private static byte[] DeCipher(byte[] in_, byte[] iv)
        {
            byte[] out_ = new byte[BLOCK_SIZE];
            int round = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    state[j, i] = in_[i * 4 + j];
                }
            }
            KeyExpansion();
            AddRoundKey(ROUND_COUNT);
            for (round = ROUND_COUNT - 1; round > 0; round--)
            {
                InvShiftRow();
                InvSubBytes();
                AddRoundKey(round);
                InvMixColumns();
            }
            InvShiftRow();
            InvSubBytes();
            AddRoundKey(0);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    out_[i * 4 + j] = state[j, i];
                }
            }
            out_ = XorWithIv(out_, iv);
            return RemovePKCS7Padding(out_);
        }

        private static byte[] AddPKCS7Padding(byte[] data, int blockSize)
        {
            int paddingLength = blockSize - (data.Length % blockSize);
            byte[] paddedData = new byte[data.Length + paddingLength];
            Array.Copy(data, paddedData, data.Length);
            Array.Fill(paddedData, (byte)paddingLength, data.Length, paddingLength);
            return paddedData;
        }

        private static byte[] RemovePKCS7Padding(byte[] paddedData)
        {
            int paddingLength = paddedData[paddedData.Length - 1];
            if (paddingLength <= 0 || paddingLength > paddedData.Length)
            {
                return paddedData;
            }

            for (int i = paddedData.Length - paddingLength; i < paddedData.Length; i++)
            {
                if (paddedData[i] != paddingLength)
                {
                    return paddedData;
                }
            }
            return paddedData[..^paddingLength]; // C# 8.0+ syntax
        }

        private static byte[] XorWithIv(byte[] in_, byte[] iv)
        {
            for (int i = 0; i < BLOCK_SIZE; ++i)
            {
                in_[i] ^= iv[i];
            }
            return in_;
        }

        private static List<byte[]> SplitIntoBlocks(byte[] input)
        {
            List<byte[]> blocks = new List<byte[]>();
            int numBlocks = (int)Math.Ceiling((double)input.Length / BLOCK_SIZE);
            for (int i = 0; i < numBlocks; i++)
            {
                int startIdx = i * BLOCK_SIZE;
                int endIdx = Math.Min(startIdx + BLOCK_SIZE, input.Length);
                byte[] block = new byte[endIdx - startIdx];
                Array.Copy(input, startIdx, block, 0, endIdx - startIdx);
                if (block.Length < BLOCK_SIZE)
                {
                    block = AddPKCS7Padding(block, BLOCK_SIZE);
                }
                blocks.Add(block);
            }
            return blocks;
        }

        private static byte[] MergeArrays(byte[] array1, byte[] array2)
        {
            byte[] mergedArray = new byte[array1.Length + array2.Length];
            Array.Copy(array1, 0, mergedArray, 0, array1.Length);
            Array.Copy(array2, 0, mergedArray, array1.Length, array2.Length);
            return mergedArray;

        }

        private static string ByteArrayToHexString(byte[] in_)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in in_)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();

        }

        private static byte[] HexStringToByteArray(string hexString)
        {
            int length = hexString.Length;
            byte[] byteArray = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                byteArray[i / 2] = (byte)((Convert.ToInt32(hexString[i].ToString(), 16) << 4) + Convert.ToInt32(hexString[i + 1].ToString(), 16));
            }
            return byteArray;

        }

        public static string Encrypt(string plainText, string iv)
        {
            byte[] ivb = Encoding.UTF8.GetBytes(iv);
            ivb = AddPKCS7Padding(ivb, BLOCK_SIZE);
            List<byte[]> blocks = SplitIntoBlocks(Encoding.UTF8.GetBytes(plainText));
            byte[] rs = Array.Empty<byte>();
            foreach (byte[] b in blocks)
            {
                rs = MergeArrays(rs, Cipher(b, ivb));
            }
            return ByteArrayToHexString(rs);
        }

        public static string Decrypt(string cipherText, string iv)
        {
            byte[] ivb = Encoding.UTF8.GetBytes(iv);
            ivb = AddPKCS7Padding(ivb, BLOCK_SIZE);
            List<byte[]> blocks = SplitIntoBlocks(HexStringToByteArray(cipherText));
            byte[] rs = Array.Empty<byte>();
            foreach (byte[] b in blocks)
            {
                rs = MergeArrays(rs, DeCipher(b, ivb));
            }
            return Encoding.UTF8.GetString(rs);
        }
    }
}