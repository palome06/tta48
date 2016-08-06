using System.Collections.Generic;
using System.Linq;

namespace Trench.Utils
{
    public class Range : Couple<ushort, ushort>
    {
        public Range(ushort start, ushort end) : base(start, end) { }

        public static IDictionary<ushort, Range> Parse(string line)
        {
            ushort[] packages = line.Split(',').Select(p => ushort.Parse(p)).ToArray();
            IDictionary<ushort, Range> pkgDict = new Dictionary<ushort, Range>();
            for (int i = 0; i < packages.Length; i += 3)
            {
                pkgDict[packages[i]] = new Range(packages[i + 1], packages[i + 2]);
            }
            return pkgDict;
        }

        public ushort[] ToArray()
        {
            int sz = Second - First + 1;
            ushort[] result = new ushort[sz];
            for (int i = 0; i < sz; ++i)
            {
                result[i] = (ushort)(First + i);
            }
            return result;
        }
    }

    public class Couple<T, K>
    {
        public T First { set; get; }
        public K Second { set; get; }

        public Couple() { }
        public Couple(T first, K second)
        {
            First = first; Second = second;
        }
    }
}
