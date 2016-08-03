using Trench.Card;

namespace TTA.Base.Card
{
    public enum TTAGenre
    {
        PRODUCTION, URBAN, MILIUNIT, WONDER, LEADER, GOVT, SPECIAL, ACTION,
        EVENT, BONUS, AGGRESSION, WAR, PACT, TACTICS
    };

    public abstract class Card : Bard
    {
        public virtual TTAGenre Genre { get; }

        public virtual int Age { get; }

        public abstract bool IsCivil();

        public abstract bool IsMilitary();
    }
}
