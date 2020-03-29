using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helper.Generator.Context.Integers
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
        public int Generate(int min, int max)
        {
            if (min == max)
                new ArgumentException("minimum integer cannot be the same for maximum integer");
            if (min > max)
                new ArgumentOutOfRangeException("minimum integer cannot be bigger than maximum integer");
            return random.Next(min, max);
        }
        public int Generate()
        {
            return random.Next(0, int.MaxValue);
        }

        public List<int> GenerateList(int min, int max, int size)
        {
            if (size == 0)
                new ArgumentOutOfRangeException("count cannot be zero");
            List<int> vs = new List<int>();
            for (int i = 0; i < size; i++)
                vs.Add(Generate(min, max));
            return vs;
        }
        public List<int> GenerateList(int size)
        {
            if (size == 0)
                new ArgumentOutOfRangeException("count cannot be zero");
            List<int> vs = new List<int>();
            for (int i = 0; i < size; i++)
                vs.Add(Generate());
            return vs;
        }
    }
}
