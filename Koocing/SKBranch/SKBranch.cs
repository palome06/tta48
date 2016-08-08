using Koocing.Entity;
using System;
using System.Data;
using Trench.Utils;

namespace Koocing.SKBranch
{
    public class SKB
    {
        // Captain
        public NMB Captain { set; get; }
        // Consume Type, e.g. 0 = normal; 1 = burst; 2 = debut, etc.
        public int Consume { set; get; }
        // inner type
        public int InType { set; get; }
        // occur type, should dervied from Mint
        public System.Type Occur { set; get; }
        // int of priority
        public int Priority { set; get; }
        // lock or not, not support optimized yet
        public bool Lock { set; get; }
        // how many times it could be triggered in a stage, e.g. 1 means Once
        public int Live { set; get; }
        // 0x2, trigger one by one or altogether, not sure whether neccessary
        //public bool Serial { set; get; }
        //// 0x4, whether hind or not, should be useless
        //public bool Hind { set; get; }
        // change/raise new Mint enviormment, not sure whether neccessary
        //public bool Demiurgic { set; get; }
        
        // TODO: support link/AAS later
        // public bool Linked { get { return Occur.Contains("&"); } }
        //public static string GetAas(int aliasSerial)
        //{
        //    System.Data.DataRowCollection data = new Utils.ReadonlySQL("psd.db3")
        //        .Query(new string[] { "AVAL" }, "AAs", "AKEY = " + aliasSerial);
        //    if (data.Count == 1)
        //        return (string)data[0]["AVAL"];
        //    else
        //        return "";
        //}
    }

    public class SKBLib
    {
        public string Database { get { return "tta.db3"; } }

        public string SQL
        {
            get
            {
                return "select CAPTAIN, CONSUME, INTYPE, OCCUR, " +
                    "PRIOR, LOCK, LIVE, TERMIN from SKB";
            }
        }

        public void LoadData(Func<string, NMB> getNMB, Func<string, System.Type> getMintType)
        {
            ReadonlySQL adapter = new ReadonlySQL(Database);
            DataRowCollection data = adapter.GetDataTable(SQL).Rows;
            foreach (DataRow row in data)
            {
                NMB nmb = getNMB((string)row["CAPTAIN"]);
                System.Type mintType = getMintType((string)row["OCCUR"]);
                if (nmb != null && mintType != null)
                {
                    SKB skb = new SKB()
                    {
                        Captain = nmb,
                        Consume = (int)row["CONSUME"],
                        InType = (int)row["INTYPE"],
                        Priority = (int)row["PRIORITY"],
                        Lock = (bool)row["LOCK"],
                        Live = (int)row["LIVE"]
                    };
                    nmb.RegisterSKB(skb);
                }
            }
        }
    }
}