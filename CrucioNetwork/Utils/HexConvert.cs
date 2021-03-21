using System;
using System.Collections.Generic;
using System.Text;

namespace CrucioNetwork.Utils
{
    class HexConvert
    {
        public static string ToStringLC(byte[] bytes)
        {
            int chArrayLength = bytes.Length * 2;
            char[] chArray = new char[chArrayLength];
            int index = 0;
            for (int i = 0; i < chArrayLength; i += 2)
            {
                byte b = bytes[index++];
                chArray[i] = HexDightLC(b / 16);
                chArray[i + 1] = HexDightLC(b % 16);
            }
            return new string(chArray);
        }
        private static char HexDightLC(int i)
        {
            if (i < 0 || i > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(i), "i must be between 0 and 15.");
            }
            if (i < 10)
            {
                return (char)(i + '0');
            }
            return (char)(i - 10 + 'a');
        }
    }
}
