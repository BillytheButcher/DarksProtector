using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Helper.Generator.Context
{
    class GeneratorCtrl
    {
        Random random;
        public GeneratorCtrl(Random random)
        {
            if (random == null)
                new ArgumentNullException("random is Null");
            this.random = random;
        }
        public GeneratorCtrl()
        {
            if (random == null)
                random = new Random(Guid.NewGuid().GetHashCode());
            
        }

        public T Generate<T>(GeneratorType generatorType, int max)
        {
            if (max == 0)
                new ArgumentNullException("max cannot be zero");
            if (generatorType == GeneratorType.Integer)
                return (T)(object)new Integers.Randomizer(random).Generate(0, max);
            else
                return (T)(object)new Strings.Randomizer(random).Generate(Strings.RandomizerType.Alphabetic, max);
        }
        public List<T> GenerateList<T>(GeneratorType generatorType, int max)
        {
            List<T> ts = new List<T>();
            if (max == 0)
                new ArgumentNullException("max cannot be zero");
            if (generatorType == GeneratorType.Integer)
                ts.Add((T)(object)new Integers.Randomizer(random).Generate());
            else
                ts.Add((T)(object)new Strings.Randomizer(random).Generate());
            return ts;
        }
    }
}
