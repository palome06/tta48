namespace TTA.Base.Card
{
    public enum TTAGenre
    {
        PRODUCTION, URBAN, MILIUNIT, WONDER, LEADER, GOVT, SPECIAL, ACTION,
        EVENT, BONUS, AGGRESSION, WAR, PACT, TACTICS
    };

    public abstract class Card : Bard
    {
        public enum TTAGenre Genre { get; }

        public int Age { get; }

        public bool IsCivil();

        public bool IsMilitary();
    }
}
