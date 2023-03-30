using System;

namespace AtemCommandTestGenerator
{
    class Util
    {
        public static readonly Random random = new Random();
        public static byte[] RandomBytes(int size)
        {
            var res = new byte[size];
            random.NextBytes(res);
            return res;
        }
    }
}
