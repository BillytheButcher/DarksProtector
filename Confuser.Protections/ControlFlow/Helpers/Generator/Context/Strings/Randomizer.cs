using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helper.Generator.Context.Strings
{
    class Randomizer
    {
        Random random;
        public Randomizer(Random random)
        {
            if (random == null)
                new ArgumentNullException("random is Null");
            this.random = random;
        }
        public Randomizer()
        {
            if (random == null)
                random = new Random(Guid.NewGuid().GetHashCode());
        }
        public string Generate(RandomizerType randomizerType, int maxSize)
        {
            if (maxSize == 0)
                new ArgumentNullException("size cannot be zero");
            char[] chars = (randomizerType == RandomizerType.Alphabetic ? "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray() : "れづれなるまゝに日暮らし硯にむかひて心にうりゆくよな事を、こはかとなく書きつくればあやうこそものぐるほけれ".ToCharArray());
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
                result.Append(chars[b % (chars.Length)]);
            return result.ToString();
        }
        public string Generate(RandomizerType randomizerType)
        {
            return Generate(randomizerType, 10);
        }
        public string Generate()
        {
            return Generate(RandomizerType.Alphabetic, 10);
        }
        public List<string> GenerateList(RandomizerType randomizerType, int maxSize, int maxListSize)
        {
            if (maxSize == 0)
                new ArgumentNullException("size cannot be zero");
            if (maxListSize == 0)
                new ArgumentNullException("list size cannot be zero");
            List<string> stringList = new List<string>();
            for (int i = 0; i < maxListSize; i++)
                stringList.Add(Generate(randomizerType, maxSize));
            return stringList;
        }
        public List<string> GenerateList(RandomizerType randomizerType, int maxListSize)
        {
            if (maxListSize == 0)
                new ArgumentNullException("list size cannot be zero");
            List<string> stringList = new List<string>();
            for (int i = 0; i < maxListSize; i++)
                stringList.Add(Generate(randomizerType));
            return stringList;
        }
    }
}
