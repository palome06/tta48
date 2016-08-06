using System.Collections.Generic;
using System.Data;
using Trench.Card;
using Trench.Utils;

namespace TTA.Base.Card.Model
{
    public class Leader : Card
    {
        #region Implemented
        public int Age { internal set; get; }

        public int Avatar { internal set; get; }

        public int Genre { get { return CardGenre.LEADER; } }

        public string Nick { internal set; get; }

        public string OfCode { internal set; get; }

        public IDictionary<ushort, Range> Package { set; get; }

        public bool IsCivil() { return true; }

        public bool IsMilitary() { return false; }

        public bool PeaceAvailable() { return true; }
        #endregion Implemented
        // TODO: not sure whether put it here or not
        //public int GetFood { set; get; }
        //public int GetMine { set; get; }
        //public int GetTech { set; get; }
        //public int GetCult { set; get; }
        //public int GetRedToken { set; get; }
        //public Ops NextOps { set; get; }
    }

    internal class LeaderSupplier : Supplier
    {
        public override string Database { get { return "tta.db3"; } }

        public override string SQL
        {
            get
            {
                return "select OFCODE, AVATAR, AGE, PACKAGE, NICK from `Leader` where ENABLED = 1";
            }
        }

        public override Bard GetBard(DataRow row)
        {
            return new Leader()
            {
                OfCode = (string)row["OFCODE"],
                Avatar = (int)row["AVATAR"],
                Age = (int)row["AGE"],
                Nick = (string)row["NICK"],
                Package = Range.Parse((string)row["PACKAGE"])
            };
        }
    }
}
