using System.Collections.Generic;
using System.Linq;
using Trench.Utils;

namespace TTA.Base.Card
{
    public class Action : Card
    {
        public override TTAGenre Genre { get { return TTAGenre.ACTION; } }

        public override bool IsCivil() { return true; }

        public override bool IsMilitary() { return false; }

        private int mAge;
        public override int Age { get { return mAge; }}

        internal Action(int age)
        {
            mAge = age;
        }
    }

    public class ActionLib
    {
        public List<Action> Firsts { private set; get; }

        private IDictionary<ushort, Action> dicts;

        private ReadonlySQL sql;

        public ActionLib(string language)
        {
            //if (string.IsNullOrEmpty(language)) { language = "ZH"; }
            Firsts = new List<Action>();
            sql = new ReadonlySQL("tta.db3");
            List<string> list = new string[] {
                "ID", "CODE", "PACKAGE", "AGE"
            }.ToList();
            System.Data.DataRowCollection datas = sql.Query(list, "Action");
            //foreach (System.Data.DataRow data in datas)
            //{
            //    long lid = (long)data["ID"];
            //    string code = (string)data["CODE"];
            //    int genre = (ushort)((long)data["GENRE"]);
            //    string name = (string)data["NAME"];
            //    //ushort count = (ushort)((short)data["COUNT"]);
            //    Tux.TuxType type;
            //    switch (code.Substring(0, 2))
            //    {
            //        case "JP": type = Tux.TuxType.JP; break;
            //        case "ZP": type = Tux.TuxType.ZP; break;
            //        case "TP": type = Tux.TuxType.TP; break;
            //        case "WQ": type = Tux.TuxType.WQ; break;
            //        case "FJ": type = Tux.TuxType.FJ; break;
            //        case "XB": type = Tux.TuxType.XB; break;
            //        default: type = Tux.TuxType.HX; break;
            //    }
            //    string countStr = (string)data["COUNT"];
            //    string occur = (string)data["OCCURS"];
            //    string priority = (string)data["PRIORS"];
            //    string parasitismStr = (string)data["PARASITISM"];
            //    string description = (string)data["DESCRIPTION"];
            //    string descstr = (string)data["SPECIAL"];
            //    IDictionary<string, string> special = new Dictionary<string, string>();
            //    string[] descSpt = string.IsNullOrEmpty(descstr) ?
            //            new string[] { } : descstr.Split('|');
            //    for (int i = 1; i < descSpt.Length; i += 2)
            //        special.Add(descSpt[i], descSpt[i + 1]);
            //    string targets = (string)data["TARGET"];
            //    string terministr = (string)data["TERMHIND"];
            //    string growup = (string)data["GROWUP"];
            //    if (type == Tux.TuxType.XB && growup.Contains("L"))
            //    {
            //        var tux = new Luggage(name, code, genre, type, description, special, growup);
            //        tux.Parse(countStr, occur, parasitismStr, priority, targets, terministr, (ushort)lid);
            //        Firsts.Add(tux);
            //    }
            //    else if (type == Tux.TuxType.XB && growup.Contains("I"))
            //    {
            //        var tux = new Illusion(name, code, genre, type, description, special, growup);
            //        tux.Parse(countStr, occur, parasitismStr, priority, targets, terministr, (ushort)lid);
            //        Firsts.Add(tux);
            //    }
            //    else if (type == Tux.TuxType.WQ || type == Tux.TuxType.FJ || type == Tux.TuxType.XB)
            //    {
            //        var tux = new TuxEqiup(name, code, genre, type, description, special, growup);
            //        tux.Parse(countStr, occur, parasitismStr, priority, targets, terministr, (ushort)lid);
            //        Firsts.Add(tux);
            //    }
            //    else
            //    {
            //        var tux = new Tux(name, code, genre, type, description, special);
            //        tux.Parse(countStr, occur, parasitismStr, priority, targets, terministr, (ushort)lid);
            //        Firsts.Add(tux);
            //    }
            //}
            ////ushort cardx = 1;
            //dicts = new Dictionary<ushort, Tux>();
            //foreach (Tux tux in Firsts)
            //{
            //    for (int i = 0; i < tux.Range.Length; i += 2)
            //    {
            //        for (ushort j = tux.Range[i]; j <= tux.Range[i + 1]; ++j)
            //            dicts.Add(j, tux);
            //    }
            //    if (tux.IsTuxEqiup())
            //    {
            //        TuxEqiup te = tux as TuxEqiup;
            //        te.SingleEntry = tux.Range[0];
            //    }
            //}
        }

        public int Size { get { return dicts.Count; } }

        //public Tux DecodeTux(ushort code)
        //{
        //    Tux tux;
        //    if (dicts.TryGetValue(code, out tux))
        //        return tux;
        //    else return null;
        //}
        //public Tux EncodeTuxCode(string code)
        //{
        //    foreach (Tux tux in Firsts)
        //    {
        //        if (tux.Code.Equals(code))
        //            return tux;
        //    }
        //    return null;
        //}
        //public Tux EncodeTuxDbSerial(ushort dbSerial)
        //{
        //    List<Tux> tuxes = Firsts.Where(
        //        p => p.DBSerial == dbSerial).ToList();
        //    return tuxes.Count > 0 ? tuxes.First() : null;
        //}
        //public List<Tux> ListAllTuxs(int groups)
        //{
        //    int[] pkgs = Card.Level2Pkg(groups);
        //    if (pkgs == null)
        //        return Firsts.ToList();
        //    else
        //        return Firsts.Where(p => p.Package.Any(q => pkgs.Contains(q))).ToList();
        //}
        //public List<Tux> ListAllTuxSeleable(int groups)
        //{
        //    List<Tux> first = ListAllTuxs(groups);
        //    string[] duplicated = { "TPT2", "JPT1", "JPT4", "XBT2" };
        //    string[] keeppace = { "TPR1", "JPR1", "JPR2", "XBR1" };
        //    for (int i = 0; i < duplicated.Length; ++i)
        //    {
        //        if (first.Any(p => p.Code == duplicated[i]) && first.Any(p => p.Code == keeppace[i]))
        //            first.RemoveAll(p => p.Code == duplicated[i]);
        //    }
        //    return first;
        //}
        //public List<ushort> ListAllTuxCodes(int groups)
        //{
        //    int[] pkgs = Card.Level2Pkg(groups);
        //    List<Tux> txs = ListAllTuxSeleable(groups);
        //    List<ushort> us = new List<ushort>();
        //    foreach (Tux tux in txs)
        //    {
        //        for (int i = 0; i < tux.Package.Length; ++i)
        //        {
        //            if (pkgs == null || pkgs.Contains(tux.Package[i]))
        //            {
        //                for (ushort j = tux.Range[i * 2]; j <= tux.Range[i * 2 + 1]; ++j)
        //                    us.Add(j);
        //            }
        //        }
        //    }
        //    return us;
        //}
        //// Find the unique equip code number, return 0 when no or duplicated found
        //public ushort UniqueEquipSerial(string code)
        //{
        //    ushort ans = 0;
        //    foreach (var pair in dicts)
        //    {
        //        if (pair.Value.Code.Equals(code))
        //        {
        //            if (ans == 0)
        //                ans = pair.Key;
        //            else
        //                return 0;
        //        }
        //    }
        //    return ans;
        //}
        //public bool IsTuxInGroup(Tux tux, int level)
        //{
        //    return ListAllTuxSeleable(level).Contains(tux);
        //}
    }
}
