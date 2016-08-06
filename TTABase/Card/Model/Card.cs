using Trench.Card;

namespace TTA.Base.Card.Model
{
    public class CardGenre
    {
        public const int PRODUCTION = 1;
        public const int URBAN = 2;
        public const int MILIUNIT = 3;
        public const int WONDER = 4;
        public const int LEADER = 5;
        public const int GOVT = 6;
        public const int SPECIAL = 7;
        public const int ACTION = 8;

        public const int EVENT = 11;
        public const int BONUS = 12;
        public const int AGGRESSION = 13;
        public const int WAR = 14;
        public const int PACT = 13;
        public const int TACTICS = 14;
    }

    public interface Card : Bard
    {
        // Age 0(A)/1/2/3
        int Age { get; }

        bool IsCivil();
        bool IsMilitary();
        bool PeaceAvailable();
    }
}
