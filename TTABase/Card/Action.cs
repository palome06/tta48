namespace TTA.Base.Card
{
    public class Action : Card
    {
        public enum TTAGenre Genre { get { return TTAGenre.ACTION; } }

        public bool IsCivil() { return true; }

        public bool IsMilitary() { return false; }

        private int mAge;
        public int Age { get { return mAge; }}

        public int Food { internal set; get; }
        public int Mine { internal set; get; }
        public int Tech { internal set; get; }
        public int Cult { internal set; get; }
    }

    public class ActionLib
    {
    	
    }
}
