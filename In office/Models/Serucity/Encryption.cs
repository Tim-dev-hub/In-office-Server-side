using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="password"></param>
        /// <param name="commentary"></param>
        /// <returns>True if password OK</returns>
        public static bool ValidPassword(string password, out string commentary)
        {
            commentary = "";
            bool result = true;

            if(password.Length < 6)
            {
                commentary += "Пароль слишком короткий\n";
                result = false;
            }
            else if(password.Length > 40)
            {
                commentary += "Пароль слишком длинный\n";
                result = false;
            }

            char[] numbers = "0123456789".ToCharArray();
            char[] lowLetters = "qazwsxedcrfvtgbyhnmjuiklopйфяцычувскамепинртьогшлдщзъэё".ToCharArray();
            char[] upLetters = "QAZWSXEDCRFVTGBYHNMJUIKLOPЙФЯЦЫЧУВСКАМЕПИНРТЬОГШЛДЩЗЪЭЁ".ToCharArray();

            int numsCount = 0;
            int lowLetsCount = 0;
            int upLetsCount = 0;

            for(int i =0; i < password.Length; i++)
            {
                if (numbers.Contains(password[i]))
                {
                    numsCount++;
                }
                else if (lowLetters.Contains(password[i]))
                {
                    lowLetsCount++;
                }
                else if (upLetters.Contains(password[i]))
                {
                    upLetsCount++;
                }
            }

            if(numsCount <= 0)
            {
                commentary += "В пароле нету цифр\n";
                result = false;
            }

            if(lowLetsCount <= 0)
            {
                commentary += "В пароле нету маленьких букв\n";
                result = false;
            }

            if(upLetsCount <= 0)
            {
                commentary += "В пароле нету больших букв\n";
                result = false;
            }

            if (password.Contains("qwe"))
            {
                commentary += "Пароль имеет qwerty паттерн\n";
                result = false;
            }

            List<int> nums = new List<int>();

            for(int i = 0; i < password.Length; i++)
            {
                int res;
                if (int.TryParse(password[i].ToString(), out res))
                {
                    nums.Add(res);
                }
                else
                {
                    nums.Clear();
                }
            }

            if(nums.Count >= 3)
            {
                if(nums[1] == nums[0] + 1 && nums[2] == nums[1] + 1 ||
                    nums[1] == nums[0] - 1 && nums[2] == nums[1] - 1)
                {
                    commentary += "Пароль имеет 3 или более идущих подряд цифр\n";
                    result = false;
                }
            }

            StreamReader reader = new StreamReader(@"C:\Users\Timur\Downloads\rockyou.txt");
            int tryes = 0;
            while (true)
            {
                tryes++;
                string baseWord = reader.ReadLine();

                if(baseWord == null)
                {
                    break;
                }

                if (password == baseWord)
                {
                    commentary += "В словаре расспрастраннёных паролей сервера этот пароль стоит под номером " + tryes;
                    result = false;
                }
            }

            return result;
        }
    }
}
