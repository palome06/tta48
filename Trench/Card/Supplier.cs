using System.Collections.Generic;
using System.Data;
using System.Linq;
using Trench.Utils;

namespace Trench.Card
{
    public abstract class Supplier
    {
        // First is a map from ofcode to Bard
        public IDictionary<string, Bard> First { private set; get; }
        // Second is a map from ut to Bard
        public IDictionary<ushort, Bard> Second { private set; get; }
        // Database file of sqlite
        public abstract string Database { get; }
        // sql sentence for querying
        public abstract string SQL { get; }
        // GetBard from DataRow
        public abstract Bard GetBard(DataRow row);
        // load data
        public void LoadData()
        {
            First = new Dictionary<string, Bard>();
            Second = new Dictionary<ushort, Bard>();
            ReadonlySQL adapter = new ReadonlySQL(Database);
            DataRowCollection data = adapter.GetDataTable(SQL).Rows;
            foreach (DataRow row in data)
            {
                Bard bard = GetBard(row);
                First[bard.OfCode] = bard;
                foreach (Range range in bard.Package.Values)
                {
                    for (ushort start = range.First; start <= range.Second; ++start)
                        Second[start] = bard;
                }
            }
        }
        // get Bard Item from the ut rank
        public Bard Decode(ushort ut)
        {
            return Second.ContainsKey(ut) ? Second[ut] : null;
        }
        // get Bard Item from its ofcode
        public Bard Encode(string code)
        {
            return First.ContainsKey(code) ? First[code] : null;
        }
        // list all Bards under packages limits
        public List<Bard> ListAll(IEnumerable<ushort> packages)
        {
            if (packages == null)
            {
                return First.Values.ToList();
            }
            else
            {
                ushort[] pkgs = packages.ToArray();
                return First.Values.Where(p => p.Package.Keys.Intersect(pkgs).Any()).ToList();
            }
        }
        // list all uts under packages limits
        public List<ushort> ListAllUts(IEnumerable<ushort> packages)
        {
            if (packages == null)
            {
                return Second.Keys.ToList();
            }
            else
            {
                ushort[] pkgs = packages.ToArray();
                return First.Values.Select(p => p.Package).SelectMany(p => p.ToList()).Where(
                    p => pkgs.Contains(p.Key)).SelectMany(p => p.Value.ToArray()).ToList();
            }
        }
    }
}
