using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace In_office.Models.Serucity
{
    public static class Encryption
    {
        public static string Generate512ByteKey()
        {
            Random random = new Random();
            byte[] key0 = new byte[512];
            random.NextBytes(key0);
            byte[] key1 = new byte[512];
            random.NextBytes(key1);
            byte[] key2 = new byte[512];
            random.NextBytes(key2);

            List<Byte> finalKey = new List<byte>();

            for(int i = 0; i < 512; i++)
            {
                var chance = random.NextDouble();
                finalKey.Add(chance < 0.333 ? key0[i] : (chance < 0.666 ? key1[i] : key2[i]));
            }
            var strResult = BitConverter.ToString(finalKey.ToArray());

            return strResult.Replace("-", string.Empty);
        }
    }
}
